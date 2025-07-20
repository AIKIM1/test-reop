/*************************************************************************************
 Created Date : 2023.06.22
      Creator : 이의철
   Decription : Route 선택
--------------------------------------------------------------------------------------
 [Change History]
  2023.08.24  이의철 : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_031_ROUTE_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string MDLLOT_ID = string.Empty;
        private string EQSGID = string.Empty;
        

        public FCS001_031_ROUTE_LIST()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        #endregion

        #region Initialize

        
        #endregion

        #region Event
        
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            MDLLOT_ID = tmps[0] as string;
            EQSGID = tmps[1] as string;

            GetList();
        }

        private void dgRouteList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index != dgRouteList.Rows.Count) //마지막 RowHeader 표시 x
            {
                tb.Text = (e.Row.Index + 1 - dgRouteList.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            DataSet ds = new DataSet();

            DataTable inData = ds.Tables.Add("INDATA");
            inData.Columns.Add("AREAID", typeof(string));
            inData.Columns.Add("USE_FLAG", typeof(string));
            inData.Columns.Add("ROUTID", typeof(string));
                        
            for (int i = 0; i <  dgRouteList.GetRowCount(); i++)
            {
                string USE_FLAG = String.Empty;
                if (DataTableConverter.GetValue(dgRouteList.Rows[i].DataItem, "CHK").Equals(true))
                {
                    USE_FLAG = "Y";
                }
                else
                {
                    USE_FLAG = "N";
                }

                DataRow dr = inData.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USE_FLAG"] = USE_FLAG;
                dr["ROUTID"] = Util.NVC(DataTableConverter.GetValue(dgRouteList.Rows[i].DataItem, "CBO_CODE"));
                inData.Rows.Add(dr);                
            }           

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_SET_TB_SFC_EQPT_ROUT_LIST", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("BR_SET_TB_SFC_EQPT_ROUT_LIST", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.MessageInfo("FM_ME_0136"); //변경완료하였습니다.
                        //this.DialogResult = MessageBoxResult.OK;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU2807"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning); //조회 오류
            }

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgRouteList);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgRouteList);
        }

        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["MDLLOT_ID"] = MDLLOT_ID;
                dr["EQSGID"] = EQSGID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_SEL_TB_SFC_EQPT_ROUT_LIST", "RQSTDT", "RSLTDT", dtRqst);
                
                if (result.Rows.Count > 0)
                {
                    DataTable dtRslt = result.Copy();

                    dtRslt.Columns.Add("CHK", typeof(bool));
                    
                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        string USE_FLAG = Util.NVC(dtRslt.Rows[i]["USE_FLAG"]);
                        string CBO_CODE = Util.NVC(dtRslt.Rows[i]["CBO_CODE"]);
                        string CBO_NAME = Util.NVC(dtRslt.Rows[i]["CBO_NAME"]);
                        string SORT = Util.NVC(dtRslt.Rows[i]["SORT"]);
                        string ROUT_RSLT_GR_CODE = Util.NVC(dtRslt.Rows[i]["ROUT_RSLT_GR_CODE"]);
                                                
                        //사용 여부
                        if (USE_FLAG.Equals("Y"))
                        {
                            dtRslt.Rows[i]["CHK"] = true;
                        }
                        else
                        {
                            dtRslt.Rows[i]["CHK"] = false;
                        }                        
                    }

                    Util.GridSetData(dgRouteList, dtRslt, FrameOperation, true);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        
    }
}
