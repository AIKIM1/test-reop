/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class ProtoType0204 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        Util _Util = new Util();

        DataSet dsBizRunInfo = new DataSet();

        public ProtoType0204()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        #endregion

        #region Initialize

        private void Initialize()
        {
            GetBizRunInfo();
            GetBizList();
        }

        #endregion

        #region Event

        //Button ========================================================================================================
        private void btnBizList_Click(object sender, RoutedEventArgs e)
        {
            GetBizList();
        }

        private void btnBizRunData_Click(object sender, RoutedEventArgs e)
        {
            GetBizRunData();
        }

        //Data Grid =====================================================================================================

        private void dgBizList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            GetBizInfo(sender);
        }

        private void dgInData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Column.Index + 1 == dg.Columns.Count)
                {
                    GetBizRunData();
                }
            }));
        }

        #endregion

        #region Mehod

        private void GetBizList()
        {
            FrameOperation.PrintFrameMessage("");

            //GetBizInfoListCallXML =====================================================================================
            LGC.GMES.MES.Common.Common com = new Common.Common();
            DataTable dtBizList = com.GetBizInfoListCallXML("bizactor_svclist", "BR", "", "", "");
            //DataTable dtBizList = com.GetBizInfoListCallXML("bizactor_svclist", "BR", "Test_Jungks", "", "");
            //GetBizInfoListCallXML =====================================================================================
            dgBizList.ItemsSource = DataTableConverter.Convert(dtBizList);
        }

        private void GetBizInfo(object sender)
        {
            FrameOperation.PrintFrameMessage("");

            C1DataGrid dg = sender as C1DataGrid;
            DataRowView rowview = dg.SelectedItem as DataRowView;
            if (rowview != null)
            {
                //GetBizInfoCallXML =====================================================================================
                string strbizsvc = Util.NVC(rowview["SVC_ID"]);
                LGC.GMES.MES.Common.Common com = new Common.Common();
                DataSet dsBizInfo = com.GetBizInfoCallXML(strbizsvc);
                //GetBizInfoCallXML =====================================================================================

                string strInData = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RQST"]);
                if (strInData.IndexOf(',') != -1)
                {
                    dgInData.Columns.Clear();
                    dgInData.ItemsSource = null;
                    FrameOperation.PrintFrameMessage("BIZ RULE MULTI INDATA");
                    return;
                }
                DataTable dtInData = new DataTable(strInData);
                foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                {
                    dtInData.Columns.Add(col.ColumnName, typeof(string));
                }
                DataRow drInData = dtInData.NewRow();
                drInData = dtInData.NewRow();

                if (dsBizRunInfo.Tables.Count != 0)
                {
                    DataRow[] drs = dsBizRunInfo.Tables["TB_BIZ_RULE"].Select("BIZ_RULE_NAME ='" + strbizsvc + "'");
                    if (drs.Count() != 0)
                    {
                        string[] split = Util.NVC(drs[0]["INDATA"]).Split(',');
                        if (split.Length == dsBizInfo.Tables[strInData].Columns.Count)
                        {
                            int idx = 0;
                            foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                            {
                                drInData[col.ColumnName] = split[idx];
                                idx++;
                            }
                        }
                        else
                        {
                            foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                            {
                                drInData[col.ColumnName] = "";
                            }
                        }
                    }
                    dtInData.Rows.Add(drInData);
                    dtInData.AcceptChanges();
                }
                else
                {
                    foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                    {
                        drInData[col.ColumnName] = "";
                    }
                    dtInData.Rows.Add(drInData);
                    dtInData.AcceptChanges();
                }

                dgInData.Columns.Clear();
                foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                {
                    Util.SetGridColumnText(dgInData, col.ColumnName, null, col.ColumnName, true, true, true, false, 200, HorizontalAlignment.Left, Visibility.Visible);
                }
                dgInData.ItemsSource = DataTableConverter.Convert(dtInData);

                foreach (C1.WPF.DataGrid.DataGridColumn col in dgInData.Columns)
                {
                    col.Width = new C1.WPF.DataGrid.DataGridLength(200, DataGridUnitType.Pixel);
                    col.EditOnSelection = true;
                }


                foreach (C1.WPF.DataGrid.DataGridColumn col in dgInData.Columns)
                {
                    foreach (TextBox txt in Util.FindVisualChildren<TextBox>(grCommonInput))
                    {
                        if (txt.Name.Contains(col.Name) == true)
                        {
                            DataTableConverter.SetValue(dgInData.Rows[0].DataItem, col.Name, txt.Text);
                            
                        }
                    }

                }
            }
        }

        private void GetBizRunData()
        {
            FrameOperation.PrintFrameMessage("");

            if (dgBizList.SelectedItem == null)
            {
                return;
            }
            loadingIndicator.Visibility = Visibility.Visible;
            Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, Logger.MESSAGE_OPERATION_START, LogCategory.UI);

            DataTable dtIndata = new DataTable();
            for (int idx = 0; idx < dgInData.Columns.Count; idx++)
            {
                string coltmp = Util.NVC(dgInData.Columns[idx].Name);
                dtIndata.Columns.Add(coltmp, typeof(string));
            }

            DataRow drInData = dtIndata.NewRow();
            for (int idx = 0; idx < dgInData.Columns.Count; idx++)
            {
                string coltmp = Util.NVC(dgInData.Columns[idx].Name);
                string rowtmp = Util.NVC(DataTableConverter.GetValue(dgInData.Rows[0].DataItem, dgInData.Columns[idx].Name));
                drInData[coltmp] = rowtmp;
            }
            dtIndata.Rows.Add(drInData);

            string strbizsvc = Util.NVC(DataTableConverter.GetValue(dgBizList.SelectedItem, "SVC_ID"));
            if (strbizsvc != string.Empty)
            {
                SetBizRunInfo(strbizsvc, dtIndata);
            }
            new ClientProxy().ExecuteService(strbizsvc, "INDATA", "OUTDATA", dtIndata, (dtOutdata, Exception) =>
            {
                if (Exception != null)
                {
                    dgOutData.ItemsSource = null;
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                try
                {
                    dgOutData.ItemsSource = DataTableConverter.Convert(dtOutdata);
                    for (int idx = 0; idx < dgOutData.Columns.Count; idx++)
                    {
                        dgOutData.Columns[idx].Width = new C1.WPF.DataGrid.DataGridLength(200, DataGridUnitType.Pixel);
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    dgOutData.ItemsSource = null;
                    Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, ex, LogCategory.UI);
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, Logger.MESSAGE_OPERATION_END, LogCategory.UI);
                    loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                }
            }
            );
        }

        private void GetBizRunInfo()
        {
            dsBizRunInfo = CustomConfig.Instance.ReadConfigSet(ConfigurationManager.AppSettings["EQP_TEST_FILE_NAME"]);
        }

        private void SetBizRunInfo(string strbizsvc, DataTable dtIndata)
        {
            if (dsBizRunInfo.Tables.Count == 0)
            {
                DataTable dtBizRuleInfo = new DataTable("TB_BIZ_RULE");
                dtBizRuleInfo.Columns.Add("BIZ_RULE_NAME", typeof(string));
                dtBizRuleInfo.Columns.Add("INDATA", typeof(string));
                DataRow drBizEQPTest = dtBizRuleInfo.NewRow();
                drBizEQPTest = dtBizRuleInfo.NewRow();
                drBizEQPTest["BIZ_RULE_NAME"] = strbizsvc;
                string strtmp = string.Empty;
                foreach (string strInData in dtIndata.Rows[0].ItemArray)
                {
                    strtmp += strInData + ",";
                }
                drBizEQPTest["INDATA"] = strtmp.Substring(0, strtmp.ToString().LastIndexOf(','));
                dtBizRuleInfo.Rows.Add(drBizEQPTest);
                dtBizRuleInfo.AcceptChanges();
                dsBizRunInfo.Tables.Add(dtBizRuleInfo.Copy());
                CustomConfig.Instance.WriteConfigSet(dsBizRunInfo, ConfigurationManager.AppSettings["EQP_TEST_FILE_NAME"]);
            }
            else
            {
                DataRow[] drs = dsBizRunInfo.Tables["TB_BIZ_RULE"].Select("BIZ_RULE_NAME ='" + strbizsvc + "'");
                if (drs.Count() == 0)
                {
                    DataRow drBizEQPTest = dsBizRunInfo.Tables["TB_BIZ_RULE"].NewRow();
                    drBizEQPTest = dsBizRunInfo.Tables["TB_BIZ_RULE"].NewRow();
                    drBizEQPTest["BIZ_RULE_NAME"] = strbizsvc;
                    string strtmp = string.Empty;
                    foreach (string strInData in dtIndata.Rows[0].ItemArray)
                    {
                        strtmp += strInData + ",";
                    }
                    drBizEQPTest["INDATA"] = strtmp.Substring(0, strtmp.ToString().LastIndexOf(','));
                    dsBizRunInfo.Tables["TB_BIZ_RULE"].Rows.Add(drBizEQPTest);
                    dsBizRunInfo.Tables["TB_BIZ_RULE"].AcceptChanges();
                    CustomConfig.Instance.WriteConfigSet(dsBizRunInfo, ConfigurationManager.AppSettings["EQP_TEST_FILE_NAME"]);
                }
                else
                {
                    foreach (DataRow dr in dsBizRunInfo.Tables["TB_BIZ_RULE"].Rows)
                    {
                        if (Util.NVC(dr["BIZ_RULE_NAME"]) == strbizsvc)
                        {
                            string strtmp = string.Empty;
                            foreach (string strInData in dtIndata.Rows[0].ItemArray)
                            {
                                strtmp += strInData + ",";
                            }
                            dr["INDATA"] = strtmp.Substring(0, strtmp.ToString().LastIndexOf(','));
                            dsBizRunInfo.Tables["TB_BIZ_RULE"].AcceptChanges();
                            CustomConfig.Instance.WriteConfigSet(dsBizRunInfo, ConfigurationManager.AppSettings["EQP_TEST_FILE_NAME"]);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
