/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.ProtoType02
{
    public partial class EQP_Biz_Rule_InOut : UserControl, IWorkArea
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

        public EQP_Scenario_Test EQP_Scenario;

        public EQP_Biz_Rule_InOut()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        #endregion

        #region Mehod

        public void GetBizInfo(object obj)
        {
            int maxincnt = tabBizInData.Items.Count;
            int maxoutcnt = tabBizOutData.Items.Count;

            DataRowView rowview = obj as DataRowView;
            if (rowview != null)
            {
                //GetBizInfoCallXML =====================================================================================
                string strbizsvc = Util.NVC(rowview["SVC_ID"]);
                LGC.GMES.MES.Common.Common com = new Common.Common();
                DataSet dsBizInfo = com.GetBizInfoCallXML(strbizsvc);
                //GetBizInfoCallXML =====================================================================================

                txtbSVC_ID.Text = strbizsvc;
                //InData Grid =====================================================================================================
                string[] splitIn = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RQST"]).ToString().Split(',');
                for (int idx = 0; idx < maxincnt; idx++)
                {
                    C1TabItem item = tabBizInData.Items[idx] as C1TabItem;
                    if (idx < splitIn.Length)
                    {
                        item.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        item.Visibility = Visibility.Collapsed;
                    }
                }
                for (int idx = 0; idx < splitIn.Length; idx++)
                {
                    string strInData = splitIn[idx];
                    if (string.IsNullOrEmpty(strInData) == true)
                    {
                        break;
                    }
                    string tmpidx = (idx + 1).ToString("00");
                    C1TabItem tabitem = tabBizInData.FindName("titemIn" + tmpidx) as C1TabItem;
                    tabitem.Header = strInData;
                    C1DataGrid dg = tabBizInData.FindName("dgInData" + tmpidx) as C1DataGrid;
                    DataTable dtInData = new DataTable(strInData);
                    foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                    {
                        dtInData.Columns.Add(col.ColumnName, typeof(string));
                    }
                    DataRow drInData = dtInData.NewRow();
                    drInData = dtInData.NewRow();
                    foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                    {
                        drInData[col.ColumnName] = "";
                    }
                    dtInData.Rows.Add(drInData);
                    dtInData.AcceptChanges();
                    dg.Columns.Clear();
                    foreach (DataColumn col in dsBizInfo.Tables[strInData].Columns)
                    {
                        Util.SetGridColumnText(dg, col.ColumnName, null, col.ColumnName, true, true, true, false, 200, HorizontalAlignment.Left, Visibility.Visible);
                    }
                    dg.ItemsSource = DataTableConverter.Convert(dtInData);

                    foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                    {
                        col.Width = new C1.WPF.DataGrid.DataGridLength(200, DataGridUnitType.Pixel);
                        col.EditOnSelection = true;
                    }
                }
                //OutData Grid =====================================================================================================
                string[] splitOut = Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RSLT"]).ToString().Split(',');
                if (Util.NVC(dsBizInfo.Tables["BIZACTOR_SVCINFO"].Rows[0]["SVC_RSLT"]).ToString() == string.Empty)
                {
                    tabBizOutData.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tabBizOutData.Visibility = Visibility.Visible;
                    for (int idx = 0; idx < maxoutcnt; idx++)
                    {
                        C1TabItem item = tabBizOutData.Items[idx] as C1TabItem;
                        if (idx < splitOut.Length)
                        {
                            item.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            item.Visibility = Visibility.Collapsed;
                        }
                    }
                    for (int idx = 0; idx < splitOut.Length; idx++)
                    {
                        string tmpidx = (idx + 1).ToString("00");
                        C1TabItem tabitem = tabBizOutData.FindName("titemOut" + tmpidx) as C1TabItem;
                        tabitem.Header = splitOut[idx];
                    }
                }
            }
        }

        #endregion
    }
}
