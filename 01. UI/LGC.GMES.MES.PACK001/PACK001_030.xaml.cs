/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_030 : UserControl, IWorkArea
    {

        private string _Lot = string.Empty;
        private string _Proc = string.Empty;
        private string _Prod = string.Empty;

        #region Declaration & Constructor 
        public PACK001_030()
        {
            InitializeComponent();



            setComboBox();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFilter = {LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                //TAB1
                //동
                C1ComboBox[] cboAreaChild_tab1 = { cboEquipmentSegment_tab1 };
                _combo.SetCombo(cboAreaByAreaType_tab1, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_tab1, sFilter: sFilter, sCase: "cboAreaByAreaType");

                //라인
                C1ComboBox[] cboLineParent_tab1 = { cboAreaByAreaType_tab1 };
                _combo.SetCombo(cboEquipmentSegment_tab1, CommonCombo.ComboStatus.SELECT,  cbParent: cboLineParent_tab1, sCase: "cboEquipmentSegment");

                //TAB2
                //동
                C1ComboBox[] cboAreaChild_tab2 = { cboEquipmentSegment_tab2 };
                _combo.SetCombo(cboAreaByAreaType_tab2, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild_tab2, sFilter: sFilter, sCase: "cboAreaByAreaType");

                //라인
                C1ComboBox[] cboLineParent_tab2 = { cboAreaByAreaType_tab2 };
                _combo.SetCombo(cboEquipmentSegment_tab2, CommonCombo.ComboStatus.SELECT, cbParent: cboLineParent_tab2, sCase: "cboEquipmentSegment");

                String[] sFilter3 = { "JUDGE_OK" };
                _combo.SetCombo(cboResult, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
                 

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자권한별로 메뉴 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dpDateTo_tab1.SelectedDateTime = DateTime.Now;
            dpDateFrom_tab1.SelectedDateTime = DateTime.Now.AddDays(-7);

            dpDateTo_tab2.SelectedDateTime = DateTime.Now;
            dpDateFrom_tab2.SelectedDateTime = DateTime.Now.AddDays(-7);
        }


        private void btnSearch_tab1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getWipList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_tab2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getQualCheckWipList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetProductProcessCheckItemList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnQualitySave_Click(object sender, RoutedEventArgs e)
        {
            DataSet dsResult = null;
            try
            {
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";

                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("TOTAL_JUDGE", typeof(string));
                INDATA.Columns.Add("TOTAL_MEMO", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["LOTID"] = txtLotId.Text;
                drINDATA["TOTAL_JUDGE"] = cboResult.SelectedValue;
                drINDATA["TOTAL_MEMO"] = txtMemo.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(drINDATA);

                dsInput.Tables.Add(INDATA);

                //-------------------------------------------------------------------------------//
                DataTable IN_CHECKITEM = new DataTable();
                IN_CHECKITEM.TableName = "IN_CHECKITEM";
                IN_CHECKITEM.Columns.Add("CLCTITEM", typeof(string));
                IN_CHECKITEM.Columns.Add("JUDGE", typeof(string));
                IN_CHECKITEM.Columns.Add("MEMO", typeof(string));

                DataRow drIN_CHECKITEM = null;
                if (dgQualityInfo.Rows.Count > 0)
                {
                    for (int i = 0; i < dgQualityInfo.Rows.Count; i++)
                    {
                        drIN_CHECKITEM = IN_CHECKITEM.NewRow();
                        drIN_CHECKITEM["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dgQualityInfo.Rows[i].DataItem, "CLCTITEM"));
                        drIN_CHECKITEM["JUDGE"] = Util.NVC(DataTableConverter.GetValue(dgQualityInfo.Rows[i].DataItem, "JUDGE"));
                        drIN_CHECKITEM["MEMO"] = Util.NVC(DataTableConverter.GetValue(dgQualityInfo.Rows[i].DataItem, "MEMO"));
                        IN_CHECKITEM.Rows.Add(drIN_CHECKITEM);
                    }
                }
                dsInput.Tables.Add(IN_CHECKITEM);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SFC_WIP_PROC_CHK_RSLT", "INDATA,IN_CHECKITEM", "", dsInput, null);

                Util.Alert("10004"); //저장이 완료되었습니다.

                dgQualityInfo.ItemsSource = null;
                txtMemo.Text = string.Empty;
                cboResult.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_SFC_WIP_PROC_CHK_RSLT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sLOTID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "LOTID"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    string sPRODNAME = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODNAME"));

                    _Proc = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PROCID"));
                    _Prod = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "PRODID"));
                    _Lot = Util.NVC(DataTableConverter.GetValue(dgWipList.Rows[cell.Row.Index].DataItem, "LOTID"));

                    txtLotId.Text = _Lot;

                    GetProductProcessCheckItemList();

                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtLotId.Text.Length > 0)
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgCheckWipList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgCheckWipList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("WIPSEQ", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCheckWipList.Rows[cell.Row.Index].DataItem, "LOTID"));
                    dr["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgCheckWipList.Rows[cell.Row.Index].DataItem, "WIPSEQ"));
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SFC_WIP_PROC_CHK_RSLT_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                    dgQualityInfo_tab2.ItemsSource = DataTableConverter.Convert(dtResult);

                    lblLotID.Text = ObjectDic.Instance.GetObjectName("검사결과") + "[" + dr["LOTID"] + "]";
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_SFC_WIP_PROC_CHK_RSLT_DETL", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [WIP기본정보조회]
        private void getWipList()
        {
            try
            {
                if (Convert.ToDecimal(Convert.ToDateTime(dpDateFrom_tab1.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dpDateTo_tab1.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_tab1) == "" ? null : Util.GetCondition(cboEquipmentSegment_tab1); 
                dr["PROCID"] = "P9000";
                dr["UPDDTTM_FROM"] = dpDateFrom_tab1.SelectedDateTime.ToString("yyyyMMdd");
                dr["UPDDTTM_TO"] = dpDateTo_tab1.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_SFC_WIP_PROC_CHK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);

                dgWipList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WIP_SFC_WIP_PROC_CHK_RSLT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [WIP기본정보조회]
        private void getQualCheckWipList()
        {
            try
            {
                if (Convert.ToDecimal(Convert.ToDateTime(dpDateFrom_tab2.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dpDateTo_tab2.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_TO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = null;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_tab2) == "" ? null : Util.GetCondition(cboEquipmentSegment_tab2); 
                dr["ACTDTTM_FROM"] = dpDateFrom_tab2.SelectedDateTime.ToString("yyyyMMdd");
                dr["ACTDTTM_TO"] = dpDateTo_tab2.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SFC_WIP_PROC_CHK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);

                dgCheckWipList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_SFC_WIP_PROC_CHK_RSLT", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [제품별공정별CheckItem List]
        private void GetProductProcessCheckItemList()
        {
            try
            {
                if (!txtLotId.Text.Equals(String.Empty))
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("PRODID", typeof(string));


                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PRODID"] = _Prod;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTPROCESSQUALITEM_CHECK", "INDATA", "OUTDATA", dtRqst);

                    Util.gridClear(dgQualityInfo);
                    dgQualityInfo.ItemsSource = DataTableConverter.Convert(dtRslt);

                    Util.GridSetData(dgQualityInfo, dtRslt, FrameOperation);

                    

                    DataTable dt = new DataTable();
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dr1 = dt.NewRow();
                    dr1["CBO_CODE"] = "OK"; dr1["CBO_NAME"] = "OK";
                    dt.Rows.Add(dr1);

                    DataRow dr2 = dt.NewRow();
                    dr2["CBO_CODE"] = "NG"; dr2["CBO_NAME"] = "NG";
                    dt.Rows.Add(dr2);

                    C1.WPF.DataGrid.DataGridColumn col = dgQualityInfo.Columns["JUDGE"];
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);


                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }
        #endregion
        #endregion
    }
}
