/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면 - 입고정보 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
  2018.09.14  손우석  3788562 GMES 셀 입고 시 요청번호 C20180910_88562
  2018.10.08  손우석  컬럼 명칭 오류 수정
  2019.01.10  손우석  팝업 호출시 RCV_ISS_ID 파라미터 처리 수정
  2019.08.29  손우석  CELL 투입라인 추가 및 입고라인 변경
  2020.01.17  염규범  폴란드의 경우, DB가 분리되어 있어서, 입고 잘못되는 Issus Validation Logic 추가
  2020.01.29  염규범  대량 CELL ( 대략 400개 이상 기준 ) 의 PALLET 입고 정보 변경시 Xml OverFlow문제 발생으로, 내용 수정
  2023.06.22  정유진  팩2동만 PLLT_BCD_ID 필드 표시, PLLT_BCD_ID OUTDATA 추가 - RTD
  2025.04.23  김선준  Tray ID 조회조건 추가
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_019_RECEIVEPRODUCT_PALLETINFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        DataTable dtCellListResult;
        String sPRODID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_019_RECEIVEPRODUCT_PALLETINFO()
        {
            InitializeComponent();
        }

        Util util = new Util();
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                #region BCD 
                DataTable dtReturn = GetCommonCode("CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
                if (dtReturn.Rows.Count > 0)
                {
                    borderBCDIDLB.Visibility = Visibility.Visible;
                    txtBCDIDLB.Visibility = Visibility.Visible;
                    borderBCDID.Visibility = Visibility.Visible;
                    txtInfoBCDID.Visibility = Visibility.Visible;
                }
                else
                {
                    borderBCDIDLB.Visibility = Visibility.Collapsed;
                    txtBCDIDLB.Visibility = Visibility.Collapsed;
                    borderBCDID.Visibility = Visibility.Collapsed;
                    txtInfoBCDID.Visibility = Visibility.Collapsed;
                }
                #endregion //BCD
                 
                #region Tray물류여부 체크
                DataTable dtDiffusionSite = dtDiffusionSite = GetCommonCode("DIFFUSION_SITE", "CELL_TRAY_LOGIS");
                if (null != dtDiffusionSite && dtDiffusionSite.Rows.Count > 0)
                {
                    if (Util.NVC(dtDiffusionSite.Rows[0]["ATTRIBUTE2"]).Contains(LoginInfo.CFG_AREA_ID))
                    { 
                        this.rdoTray.Visibility = Visibility.Visible; 
                    }
                    else
                    {
                        this.rdoTray.Visibility = Visibility.Collapsed;
                    }
                }
                #endregion //Tray물류여부 체크 

                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    string sRcvIssId = Util.NVC(tmps[0]);
                    string sPalletId = Util.NVC(tmps[1]);
                    txtPallet.Text = sPalletId;
                    txtPallet.Tag = sRcvIssId;

                    if (txtPallet.Text.Length > 0)
                    { 
                        getPalletInfoDetail(sRcvIssId); 
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnEcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtPallet.Text.Length > 0)
                {
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    chkInputYn.IsChecked = false;
                    getPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[1].Width = new GridLength(8);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0);
                gla.To = new GridLength(1);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gdContent.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[1].Width = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star); ;
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gdContent.ColumnDefinitions[0].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
        }

        private void getPalletInfo()
        {
            try
            {
                getPalletInfoDetail(null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private void getPalletInfo_CellList(string sRCV_ISS_ID, string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["PALLETID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                dtCellListResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_DTL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtCellListResult != null)
                {
                    if (dtCellListResult.Rows.Count > 0)
                    {
                        sPRODID = dtCellListResult.Rows[0]["PRODID"].ToString();

                        setComboBox_Model_schd(sPRODID);
                    }
                    else
                    {
                        sPRODID = string.Empty;
                    }
                }
                else
                {
                    sPRODID = string.Empty;
                }
                 
                Util.GridSetData(dgSearchResultList, dtCellListResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtCellListResult.Rows.Count));
            }
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }

        }

        private void setComboBox_Model_schd(string sMTRLID)
        {
            try
            {

                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = sMTRLID;
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = null;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    { 
                        Util.MessageException(ex);
                        return;
                    }

                    cboChangeModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboChangeModel.SelectedIndex = 0;
                    }
                    else
                    {
                        cboChangeModel_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setComboBox_Route_schd(string sMODLID, string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MODLID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MODLID"] = sMODLID;
                drIndata["MTRLID"] = sMTRLID;

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = null;
                }
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    { 
                        Util.MessageException(ex);
                        return;
                    }

                    cboChangeRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboChangeRoute.SelectedIndex = 0;
                    } 
                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getPalletInfo(string sRCV_ISS_ID, string sPalletID, string sCell)
        {
            try
            {
                ClearBoxInfo();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                dr["PALLETID"] = sPalletID;
                dr["CELLID"] = sCell;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_DTL_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        txtInfoRcvIssID.Text = Util.NVC(dtResult.Rows[0]["RCV_ISS_ID"]);
                        txtInfoBoxID.Text = Util.NVC(dtResult.Rows[0]["BOXID"]);
                        txtInfoBCDID.Text = Util.NVC(dtResult.Rows[0]["PLLT_BCD_ID"]);
                        txtInfoProductID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                        txtInfoBoxState.Text = Util.NVC(dtResult.Rows[0]["BOX_RCV_ISS_STAT_CODE_NAME"]);
                        txtInfoBoxFromArea.Text = Util.NVC(dtResult.Rows[0]["FROM_AREA_NAME"]);
                        txtBoxFromSloc.Text = Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                        txtInfoBoxTorea.Text = Util.NVC(dtResult.Rows[0]["TO_AREA_NAME"]);
                        txtBoxFromDate.Text = Util.NVC(dtResult.Rows[0]["ISS_DTTM"]);
                        txtBoxToDate.Text = Util.NVC(dtResult.Rows[0]["RCV_DTTM"]); 
                        txtInfoCellQty.Text = Util.NVC(dtResult.Rows[0]["RCV_COUNT"]); 
                    }
                    else
                    {
                        ClearBoxInfo();
                        Util.MessageInfo("SFU1498");
                    }
                }

            }
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }

        }

        private void getPalletInfoDetail(string sRCV_ISS_ID)
        {
            try
            {
                ClearBoxInfo();

                DataSet ds = new DataSet();
                DataTable INDATA = ds.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("CELLID", typeof(string));
                INDATA.Columns.Add("TRAYID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;

                if ((bool)this.rdoPallet.IsChecked)
                {
                    dr["BOXID"] = this.txtPallet.Text.Trim();
                }
                else if ((bool)rdoCell.IsChecked)
                {
                    dr["CELLID"] = this.txtPallet.Text.Trim();
                }
                else
                {
                    dr["TRAYID"] = this.txtPallet.Text.Trim();
                }

                INDATA.Rows.Add(dr);


                DataSet result = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_TB_SFC_RCV_ISS", "INDATA", "OUT_CELL,OUT_PALLET,OUT_PALLET_DETL", ds); 

                if (result.Tables["OUT_PALLET"].Rows.Count != 0)
                {
                    Util.GridSetData(dgSearchResultList, result.Tables["OUT_CELL"], FrameOperation);
                    Util.GridSetData(dgBoxDetail, result.Tables["OUT_PALLET"], FrameOperation);

                    dtCellListResult = result.Tables["OUT_CELL"];

                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(result.Tables["OUT_CELL"].Rows.Count));

                    txtInfoRcvIssID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_ISS_ID"]);
                    txtInfoBoxID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["BOXID"]);
                    txtInfoBCDID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["PLLT_BCD_ID"]);
                    txtInfoProductID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["PRODID"]);
                    txtInfoBoxState.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["BOX_RCV_ISS_STAT_CODE_NAME"]);
                    txtInfoBoxFromArea.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["FROM_AREA_NAME"]);
                    txtBoxFromSloc.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["FROM_SLOC_NAME"]);
                    txtInfoBoxTorea.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["TO_AREA_NAME"]);
                    txtBoxFromDate.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["ISS_DTTM"]);
                    txtBoxToDate.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_DTTM"]);
                    txtInfoCellQty.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_COUNT"]);


                    sPRODID = result.Tables["OUT_CELL"].Rows[0]["PRODID"].ToString();
                    setComboBox_Model_schd(sPRODID);
                }
                else if (result.Tables["OUT_PALLET_DETL"].Rows.Count != 0 && result.Tables["OUT_CELL"].Rows.Count != 0)
                {
                    Util.GridSetData(dgSearchResultList, result.Tables["OUT_CELL"], FrameOperation);
                    Util.gridClear(dgBoxDetail);

                    dtCellListResult = result.Tables["OUT_CELL"];

                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(result.Tables["OUT_CELL"].Rows.Count));

                    txtInfoRcvIssID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_ISS_ID"]);
                    txtInfoBoxID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["BOXID"]);
                    txtInfoBCDID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["PLLT_BCD_ID"]);
                    txtInfoProductID.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["PRODID"]);
                    txtInfoBoxState.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["BOX_RCV_ISS_STAT_CODE_NAME"]);
                    txtInfoBoxFromArea.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["FROM_AREA_NAME"]);
                    txtBoxFromSloc.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["FROM_SLOC_NAME"]);
                    txtInfoBoxTorea.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["TO_AREA_NAME"]);
                    txtBoxFromDate.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["ISS_DTTM"]);
                    txtBoxToDate.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_DTTM"]);
                    txtInfoCellQty.Text = Util.NVC(result.Tables["OUT_PALLET_DETL"].Rows[0]["RCV_COUNT"]);


                    sPRODID = result.Tables["OUT_CELL"].Rows[0]["PRODID"].ToString();
                    setComboBox_Model_schd(sPRODID);
                }
                else
                {
                    ClearBoxInfo();
                    Util.MessageInfo("SFU1498");
                }


            }
            catch (Exception ex)
            { 
                Util.MessageException(ex);
            }

        }

        private void ClearBoxInfo()
        {
            txtInfoRcvIssID.Text = string.Empty;
            txtInfoBoxID.Text = string.Empty;
            txtInfoBCDID.Text = string.Empty;
            txtInfoProductID.Text = string.Empty;
            txtInfoBoxState.Text = string.Empty;
            txtInfoBoxFromArea.Text = string.Empty;
            txtBoxFromSloc.Text = string.Empty;
            txtInfoBoxTorea.Text = string.Empty;
            txtBoxFromDate.Text = string.Empty;
            txtBoxToDate.Text = string.Empty;
            txtInfoCellQty.Text = string.Empty; 
        }

        #endregion

        private void chkInputYn_Checked(object sender, RoutedEventArgs e)
        {
            if (dtCellListResult == null)
            {
                return;
            }

            if (dtCellListResult.Rows.Count == 0 || dtCellListResult == null)
            {
                return;
            }

            DataTable dtBind = makeBindTable();

            dgSearchResultList.ItemsSource = null;
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(0));

            DataRow[] drr = dtCellListResult.Select("INPUT_STAT = 'N' "); //잔량 cell만 가져옴

            foreach (DataRow dr in drr)
            {
                DataRow drBind = dtBind.NewRow();
                drBind["CHK"] = dr["CHK"].ToString();
                drBind["RCV_ISS_ID"] = dr["RCV_ISS_ID"].ToString();
                drBind["PALLETID"] = dr["PALLETID"].ToString();
                drBind["LOTID"] = dr["LOTID"].ToString();
                drBind["PRODID"] = dr["PRODID"].ToString();
                drBind["MODLID"] = dr["MODLID"].ToString();
                drBind["MODLNAME"] = dr["MODLNAME"].ToString();
                drBind["PROD_SCHD_MODLID"] = dr["PROD_SCHD_MODLID"].ToString();
                drBind["PROD_SCHD_MODLNAME"] = dr["PROD_SCHD_MODLNAME"].ToString();
                drBind["OCV1"] = dr["OCV1"].ToString();
                drBind["OCV1DTTM"] = dr["OCV1DTTM"].ToString();
                drBind["OCV3"] = dr["OCV3"].ToString();
                drBind["OCV3DTTM"] = dr["OCV3DTTM"].ToString();
                drBind["SOCV"] = dr["SOCV"].ToString();
                drBind["SOCVDTTM"] = dr["SOCVDTTM"].ToString();
                drBind["MOVE_PERIOD"] = dr["MOVE_PERIOD"].ToString();
                drBind["INPUT_STAT"] = dr["INPUT_STAT"].ToString();

                dtBind.Rows.Add(drBind);
            }

            Util.GridSetData(dgSearchResultList, dtBind, FrameOperation, true);
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtBind.Rows.Count));
        }

        private DataTable makeBindTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CHK", typeof(string));
            dt.Columns.Add("RCV_ISS_ID", typeof(string));
            dt.Columns.Add("PALLETID", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("MODLID", typeof(string));
            dt.Columns.Add("MODLNAME", typeof(string));
            dt.Columns.Add("PROD_SCHD_MODLID", typeof(string));
            dt.Columns.Add("PROD_SCHD_MODLNAME", typeof(string));
            dt.Columns.Add("OCV1", typeof(string));
            dt.Columns.Add("OCV1DTTM", typeof(string));
            dt.Columns.Add("OCV3", typeof(string));
            dt.Columns.Add("OCV3DTTM", typeof(string));
            dt.Columns.Add("SOCV", typeof(string));
            dt.Columns.Add("SOCVDTTM", typeof(string));
            dt.Columns.Add("MOVE_PERIOD", typeof(string));
            dt.Columns.Add("INPUT_STAT", typeof(string));

            return dt;
        }

        private void chkInputYn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dtCellListResult == null)
            {
                return;
            }

            if (dtCellListResult.Rows.Count == 0 || dtCellListResult == null)
            {
                return;
            }

            DataTable dtBind = new DataTable();
            dgSearchResultList.ItemsSource = null;
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(0));

            dtBind = dtCellListResult;


            Util.GridSetData(dgSearchResultList, dtBind, FrameOperation, true);
            Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtBind.Rows.Count));
        }

        private void cboChangeModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboChangeModel.SelectedValue), sPRODID);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSAVE_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkInputData())
                {
                    //선택한 Lot의 정보를 변경 하시겠습니까?\n\nRout : {0}\n생산적용모델 : {1}
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3599", cboChangeRoute.Text, cboChangeModel.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                    {
                        if (sresult == MessageBoxResult.OK)
                        {
                            if (dgSearchResultList.Rows.Count < 1)
                            {
                                Util.MessageInfo("");
                                return;
                            }

                            loadingIndicator.Visibility = Visibility.Visible;
                            DoEvents();
                            //2020.01.29  염규범 대량 CELL(대략 400개 이상 기준) 의 PALLET 입고 정보 변경시 Xml OverFlow문제 발생으로, 내용 수정
                            DataTable dtTemp = new DataTable();
                            //DataView dtView = new DataView();
                            DataTable dtInput = new DataTable();
                            DataView dtGirdView = new DataView();
                            DataTable dtGird = new DataTable();
                            int rowCount;
                            //Xml OverFlow를 방지하기 위해서 200 단위로 처리
                            int i = 200;
                            int iConnectCnt;

                            dtTemp = Util.MakeDataTable(dgSearchResultList, true);
                            //필터 기능 가진 결과값 복사 - 추후 사용 여부 확인
                            //dtView = dtTemp.DefaultView;

                            dtGird = dtTemp.Copy();
                            dtGirdView = dtGird.DefaultView;
                            //DataTable 클론 만들기
                            dtInput = dtTemp.Clone();


                            //총Row 수량
                            rowCount = dtTemp.Rows.Count;
                            //Biz 연결할 횟수
                            iConnectCnt = rowCount % i > 0 ? (rowCount / i) + 1 : rowCount / i;
                            if (rowCount > 400)
                            {
                                for (int k = 0; k < iConnectCnt; k++)
                                {
                                    for (int j = 0; j < (rowCount < i ? rowCount : i); j++)
                                    {
                                        if (j >= i)
                                            break;

                                        dtInput.ImportRow(dtGirdView[(k == 0 ? j : i * k + j)].Row);
                                    }
                                    changeInputCell(dtInput);
                                    rowCount = rowCount - dtInput.Rows.Count;
                                    dtInput = new DataTable();
                                    dtInput = dtTemp.Clone();

                                    if (dtTemp.Rows.Count == 0)
                                        break;
                                }
                            }
                            else
                            {
                                changeInputData();
                            }

                            chkInputYn.IsChecked = false;

                            getPalletInfo();
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private bool chkInputData()
        {
            bool bReturn = true;

            if (dgSearchResultList.GetRowCount() == 0)
            {
                return false;
            }

            if (!(bool)chkInputYn.IsChecked)
            {
                ms.AlertWarning("투입되지 않은 CELL만 입고 정보를 변경 할 수 있습니다.");//투입되지 않은 CELL만 입고 정보를 변경 할 수 있습니다.               
                bReturn = false;
                return bReturn;
            }

            if (cboChangeRoute.SelectedIndex < 0)
            {
                //경로를 선택 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1455"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
                cboChangeRoute.Focus();
                return bReturn;
            }

            if (cboChangeModel.SelectedIndex < 0)
            {
                //모델을 선택 하세요.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bReturn = false;
                cboChangeModel.Focus();
                return bReturn;
            }
             
            return bReturn;
        }

        //CELL 단위로 입고 정보 변경
        private void changeInputData()
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["MODLID"] = cboChangeModel.SelectedValue;
                drINDATA["EQSGID"] = "";
                drINDATA["ROUTID"] = cboChangeRoute.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgSearchResultList.GetRowCount(); i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[i].DataItem, "LOTID"));
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add(dtINDATA);
                indataSet.Tables.Add(dtRCV_ISS);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK_CELL", "INDATA,RCV_ISS", "OUTDATA", indataSet, null);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        //CELL 단위로 입고 정보 변경
        private void changeInputCell(DataTable dt)
        {
            try
            {
                if (dt.Rows.Count <= 0)
                {
                    Util.MessageInfo("");
                    return;
                }

                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("MODLID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("ROUTID", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["MODLID"] = cboChangeModel.SelectedValue;
                drINDATA["EQSGID"] = "";
                drINDATA["ROUTID"] = cboChangeRoute.SelectedValue;
                drINDATA["USERID"] = LoginInfo.USERID;
                dtINDATA.Rows.Add(drINDATA);

                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["SUBLOTID"] = (dt.Rows[i]).ItemArray[3].ToString();
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }

                DataSet indataSet = new DataSet();
                indataSet.Tables.Add(dtINDATA);
                indataSet.Tables.Add(dtRCV_ISS);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK_CELL", "INDATA,RCV_ISS", "OUTDATA", indataSet, null);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_RECEIVE_PRODUCT_EDIT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Loaded, new System.Threading.ThreadStart(delegate { }));
        }
        
        private DataTable GetCommonCode(string codeType, string cmcode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CMCODE"] = cmcode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }
    }
}