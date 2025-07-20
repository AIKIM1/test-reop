/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class EQP_Scenario_Test : UserControl, IWorkArea
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

        public EQP_Scenario_Test()
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
            GetBizList();
            SetBizListTabAdd();
            Set_Scenario_Combo();
        }

        #endregion

        #region Event

        //Button ========================================================================================================
        private void btnBizList_Click(object sender, RoutedEventArgs e)
        {
            GetBizList();
            SetBizListTabAdd();
        }

        private void btnBizRunData_Click(object sender, RoutedEventArgs e)
        {
            GetBizRunData();
        }

        //Data Grid =====================================================================================================

        private void dgBizList_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            tabBizList.SelectedIndex = dgBizList.SelectedIndex;
        }

        private void dgInData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            
        }

        //Tab =====================================================================================================

        private void tabBizList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgBizList.SelectedIndex = tabBizList.SelectedIndex;
        }

        #endregion

        #region Mehod

        private void GetBizList()
        {
            FrameOperation.PrintFrameMessage("");

            //GetBizInfoListCallXML =====================================================================================
            LGC.GMES.MES.Common.Common com = new Common.Common();
            //DataTable dtBizList = com.GetBizInfoListCallXML("bizactor_svclist", "BR", "", "", "");
            DataTable dtBizList = com.GetBizInfoListCallXML("bizactor_svclist", "BR", "", "", "");
            //GetBizInfoListCallXML =====================================================================================

            //dgBizList.ItemsSource = DataTableConverter.Convert(dtBizList);

            DataRow[] drs = dtBizList.Select("GRP_NAME ='" + "BasicInfo" + "'");
            DataTable tmp = new DataTable();
            tmp.Columns.Add("SVC_ID", typeof(string));
            tmp.Columns.Add("GRP_NAME", typeof(string));
            tmp.Columns.Add("COMP_ID", typeof(string));
            tmp.Columns.Add("LAYER", typeof(string));
            tmp.Columns.Add("SVC_NAME", typeof(string));
            tmp.Columns.Add("SVC_DESC", typeof(string));
            for (int idx = 0; idx < drs.Count(); idx++)
            {
                DataRow newrow = tmp.NewRow();
                newrow["SVC_ID"] = drs[idx]["SVC_ID"];
                newrow["GRP_NAME"] = drs[idx]["GRP_NAME"];
                newrow["COMP_ID"] = drs[idx]["COMP_ID"];
                newrow["LAYER"] = drs[idx]["LAYER"];
                newrow["SVC_NAME"] = drs[idx]["SVC_NAME"];
                newrow["SVC_DESC"] = drs[idx]["SVC_DESC"];
                tmp.Rows.Add(newrow);
            }
            dgBizList.ItemsSource = DataTableConverter.Convert(tmp);
        }

        private void SetBizListTabAdd()
        {
            tabBizList.Items.Clear();

            int bizmaxcnt = 0;

            if (dgBizList.Rows.Count > 100)
            {
                bizmaxcnt = 5;
            }
            else
            {
                bizmaxcnt = dgBizList.Rows.Count;
            }
            int bizcnt = 0;
            for (int idx = 0; idx < bizmaxcnt; idx++)
            //for (int idx = 0; idx < dgBizList.Rows.Count; idx++)
            {
                string header = "Step " + (idx + 1).ToString();
                string svc_id = Util.NVC(DataTableConverter.GetValue(dgBizList.Rows[idx].DataItem, "SVC_ID"));
                EQP_Biz_Rule_InOut newwindow = new EQP_Biz_Rule_InOut();
                bizcnt++;
                newwindow.GetBizInfo(dgBizList.Rows[idx].DataItem);
                C1TabItem newTabItem = new C1TabItem()
                {
                    Header = header,
                    Content = newwindow
                };
                tabBizList.Items.Add(newTabItem);
            }
        }

        private void GetBizRunData()
        {
            C1TabItem selitem = tabBizList.SelectedItem as C1TabItem;
            EQP_Biz_Rule_InOut selinoutdata = selitem.Content as EQP_Biz_Rule_InOut;
            TextBlock txtSVC_ID = selinoutdata.FindName("txtbSVC_ID") as TextBlock;

            //GetBizInfoCallXML =====================================================================================
            string strbizsvc = txtSVC_ID.Text.ToString();
            LGC.GMES.MES.Common.Common com = new Common.Common();
            DataSet dsBizInfo = com.GetBizInfoCallXML(strbizsvc);
            //GetBizInfoCallXML =====================================================================================

            string[] splitIn = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RQST"]).ToString().Split(',');
            string[] splitOut = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RSLT"]).ToString().Split(',');

            if (splitIn.Length == 1 && splitOut.Length == 1)
            {
                GetBizRunData_Single();
            }
            else
            {
                GetBizRunData_Multi(dsBizInfo);
            }
        }

        private void GetBizRunData_Single()
        {
            FrameOperation.PrintFrameMessage("");

            loadingIndicator.Visibility = Visibility.Visible;
            Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, Logger.MESSAGE_OPERATION_START, LogCategory.UI);


            C1TabItem selitem = tabBizList.SelectedItem as C1TabItem;
            EQP_Biz_Rule_InOut selinoutdata = selitem.Content as EQP_Biz_Rule_InOut;
            TextBlock txtSVC_ID = selinoutdata.FindName("txtbSVC_ID") as TextBlock;

            C1DataGrid dgInData = selinoutdata.FindName("dgInData01") as C1DataGrid;
            C1DataGrid dgOutData = selinoutdata.FindName("dgOutData01") as C1DataGrid;

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
            string strbizsvc = txtSVC_ID.Text.ToString();
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

        private void GetBizRunData_Multi(DataSet dsBizInfo)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            string strSVC_ID = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_ID"]).ToString();
            string strInData = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RQST"]).ToString();
            string strOutData = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RSLT"]).ToString();
            string[] splitIn = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RQST"]).ToString().Split(',');
            string[] splitOut = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RSLT"]).ToString().Split(',');

            DataSet dsInData = new DataSet();

            for (int idx01 = 0; idx01 < splitIn.Length; idx01++)
            {
                DataTable dtInData = new DataTable();
                for ( int idx02 = 0; idx02 < dsBizInfo.Tables[splitIn[idx01]].Columns.Count; idx02++)
                {
                    dtInData.Columns.Add(dsBizInfo.Tables[splitIn[idx01]].Columns[idx02].ToString(), typeof(string));
                }
                C1TabItem selitem = tabBizList.SelectedItem as C1TabItem;
                EQP_Biz_Rule_InOut selinoutdata = selitem.Content as EQP_Biz_Rule_InOut;

                C1DataGrid dgInData = selinoutdata.FindName("dgInData" + (idx01 +1).ToString("00")) as C1DataGrid;

                for (int idx03 = 0; idx03 < dgInData.Rows.Count; idx03++)
                {
                    DataRow newrow = dtInData.NewRow();
                    for (int idx04 = 0; idx04 < dtInData.Columns.Count; idx04++)
                    {
                        string colname = dtInData.Columns[idx04].ToString();
                        newrow[colname] = Util.NVC(DataTableConverter.GetValue(dgInData.Rows[idx03].DataItem, colname));
                    }
                    dtInData.Rows.Add(newrow);
                }
                dsInData.Tables.Add(dtInData);
                dsInData.Tables[idx01].TableName = splitIn[idx01];
            }
            new ClientProxy().ExecuteService_Multi(strSVC_ID, strInData, strOutData, (dsResult, dataException) =>
            {
                try
                {
                    if (dataException != null)
                    {
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage(dataException.Message));
                        Logger.Instance.WriteLine(this.Name.ToString() + " - " + "BR_CUS_SEL_UNFIT_DETAIL", dataException.Message);
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(dataException.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    try
                    {
                        for (int idx = 0; idx < dsResult.Tables.Count; idx++)
                        {
                            C1TabItem selitem = tabBizList.SelectedItem as C1TabItem;
                            EQP_Biz_Rule_InOut selinoutdata = selitem.Content as EQP_Biz_Rule_InOut;
                            C1DataGrid dgOutData = selinoutdata.FindName("dgOutData" + (idx +1).ToString("00")) as C1DataGrid;
                            dgOutData.ItemsSource = DataTableConverter.Convert(dsResult.Tables[splitOut[idx]]);
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        for (int idx = 0; idx < dsResult.Tables.Count; idx++)
                        {
                            C1TabItem selitem = tabBizList.SelectedItem as C1TabItem;
                            EQP_Biz_Rule_InOut selinoutdata = selitem.Content as EQP_Biz_Rule_InOut;
                            C1DataGrid dgOutData = selinoutdata.FindName("dgOutData" + (idx + 1).ToString("00")) as C1DataGrid;
                            dgOutData.ItemsSource = null;
                        }
                        Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, ex, LogCategory.UI);
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        Logger.Instance.WriteLine(Logger.OPERATION_R + MethodBase.GetCurrentMethod().Name, Logger.MESSAGE_OPERATION_END, LogCategory.UI);
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage(ex.Message));
                    Logger.Instance.WriteLine(this.Name.ToString() + " - " + "BR_CUS_SEL_UNFIT_DETAIL", ex);
                }
            }, dsInData);
        }

        private void Set_Scenario_Combo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CODE");
            dt.Columns.Add("NAME");

            dt.Rows.Add("Scenario01", "Scenario01");
            dt.Rows.Add("Scenario02", "Scenario02");
            dt.Rows.Add("Scenario03", "Scenario03");

            cboScenario.ItemsSource = DataTableConverter.Convert(dt);
        }

        #endregion
    }
}
