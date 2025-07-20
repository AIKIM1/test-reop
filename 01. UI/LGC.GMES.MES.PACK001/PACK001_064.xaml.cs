/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 장만철
   Decription : BOX 포장및 라벨 발행, 재발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.16  염규범 : Initial Created.
  2023.10.25  김민석 이력 조회 시 PRODID가 NULL로 들어가던 것을 cboProduct 값을 가져오는 것으로 수정 [요청번호] E20231018-000854 
**************************************************************************************/

using System;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Windows.Input;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_064 : UserControl, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        #region Box 스캔시 Box정보를 담아두기 위한 변수(BOX정보)
        DataTable dtBoxWoResult;

        #endregion

        #region [ Initialize ] 
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_064()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            InitCombo();
            //날자 초기값 세팅
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboEquipmentSegment = new C1ComboBox();
            cboEquipmentSegment.SelectedValue = null;
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            //동           
            C1ComboBox[] cboAreaChild = { cboProductModel };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //모델          
            C1ComboBox[] cboProductModelParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProductModelChild = { cboProduct };
            _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild, sCase: "PRJ_MODEL_AUTH");

            //제품코드  
            C1ComboBox[] cboProductParent = { cboSHOPID, cboArea, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

        }
        #endregion

        #region [ Global variable ] 
        #endregion

        #region [ Event ] 

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            nbBoxingCnt.ValueChanged += nbBoxingCnt_ValueChanged;
        }

        #region [ BOX 수량 변경 ]
        private void nbBoxingCnt_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            int boxLotmax_cnt = nbBoxingCnt == null ? 5 : Convert.ToInt32(nbBoxingCnt.Value);
            int inPutLot = nbBoxingCnt == null ? 0 : dgBoxLot.GetRowCount();
            string stat = string.Empty;

            setBoxCnt(stat, boxLotmax_cnt, inPutLot);
        }
        #endregion

        #region [ BOX 자동 생성 여부 ]
        private void chkBoxId_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            txtBoxId.IsEnabled = false;
        }

        private void chkBoxId_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            txtBoxId.IsEnabled = true;
        }
        #endregion

        #region ( Lot 입력 )
        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    DataTable dt = null;

                    if ((bool)chkBoxId.IsChecked) // BOXID 체크 박스가 체크 있을경우 BOXID 자동 생성
                    {
                        dt = chkReworkBox();

                        if (dt == null)
                        {
                            Util.MessageInfo("SFU3381");
                            return;
                        }


                        if (dt.Rows.Count > 0)
                        {

                            if (!(dgBoxLot.GetRowCount() > 0) || dgBoxLot.ItemsSource == null)
                            {
                                #region ( BOXID 자동 생성시, 첫 LOT 투입 )
                                autoBoxIdCreate(dt.Rows[0]["CLASS"].ToString(), dt.Rows[0]["PRODID"].ToString(), dt.Rows[0]["EQSGID"].ToString(), dt.Rows[0]["WOID"].ToString());

                                Util.GridSetData(dgBoxLot, dt, FrameOperation, true);

                                txtBoxingModel.Text = dt.Rows[0]["BOXID"].ToString();
                                txtBoxingProd.Text = dt.Rows[0]["PRODID"].ToString();
                                txtEqsgID.Text = dt.Rows[0]["EQSGID"].ToString();
                                txtEqsgName.Text = dt.Rows[0]["EQSGNAME"].ToString();
                                txtBoxingPcsg.Text = dt.Rows[0]["CLASS"].ToString();
                                txtBoxingModel.Text = dt.Rows[0]["MODEL"].ToString();
                                txtProcId.Text = dt.Rows[0]["PROCID"].ToString();
                                txtWoId.Text = dt.Rows[0]["WOID"].ToString();

                                nbBoxingCnt.IsEnabled = false;
                                chkBoxId.IsEnabled = false;

                                setBoxCnt("포장중", Int32.Parse(nbBoxingCnt.Value.ToString()), 1);
                                #endregion
                            }
                            else
                            {
                                #region ( BOX ID 자동생성시, 추가 LOT 투입 )

                                int TotalRow = dgBoxLot.GetRowCount();
                                int CanBoxCnt = Int32.Parse(nbBoxingCnt.Value.ToString());

                                if (TotalRow >= CanBoxCnt)
                                {
                                    Util.MessageInfo("SFU4554");
                                    return;
                                }

                                for (int i = 0; i < TotalRow; i++)
                                {
                                    string strLotId = DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "LOTID").ToString();

                                    if (txtLotId.Text.ToString() == strLotId)
                                    {
                                        Util.MessageInfo("SFU8166");
                                        txtLotId.Text = "";
                                        txtLotId.Focus();
                                        return;
                                    }
                                }

                                if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                                {
                                    Util.MessageInfo("SFU3457");
                                    return;
                                }

                                GridAdd(dgBoxLot, dt);
                                setBoxCnt("포장중", CanBoxCnt, dgBoxLot.GetRowCount());
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(txtBoxId.Text.ToString()))
                        {
                            Util.MessageInfo("BOX ID를 먼저 생성하세요.");
                            return;
                        }

                        dt = chkReworkBox();

                        int TotalRow = dgBoxLot.GetRowCount();
                        int CanBoxCnt = Int32.Parse(nbBoxingCnt.Value.ToString());

                        if (!(dgBoxLot.GetRowCount() > 0) || dgBoxLot.ItemsSource == null)
                        {
                            #region ( 박스 ID 입력, 첫 LOT 투입 )
                            if (TotalRow >= CanBoxCnt)
                            {
                                Util.MessageInfo("SFU4554");
                                return;
                            }

                            if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                            {
                                Util.MessageInfo("SFU3457");
                                return;
                            }
                            Util.GridSetData(dgBoxLot, dt, FrameOperation, true);

                            txtEqsgID.Text = dt.Rows[0]["EQSGID"].ToString();
                            txtEqsgName.Text = dt.Rows[0]["EQSGNAME"].ToString();
                            txtWoId.Text = dt.Rows[0]["WOID"].ToString();


                            nbBoxingCnt.IsEnabled = false;
                            chkBoxId.IsEnabled = false;

                            setBoxCnt("포장중", Int32.Parse(nbBoxingCnt.Value.ToString()), 1);
                            #endregion
                        }
                        else
                        {
                            #region ( 박스 ID 입력, 추가 LOT 투입 )

                            if (TotalRow >= CanBoxCnt)
                            {
                                Util.MessageInfo("SFU4554");
                                return;
                            }

                            for (int i = 0; i < TotalRow; i++)
                            {
                                string strLotId = DataTableConverter.GetValue(dgBoxLot.Rows[i].DataItem, "LOTID").ToString();

                                if (txtLotId.Text.ToString() == strLotId)
                                {
                                    Util.MessageInfo("SFU8166");
                                    txtLotId.Text = "";
                                    txtLotId.Focus();
                                    return;
                                }
                            }

                            if (!dt.Rows[0]["PRODID"].ToString().Equals(txtBoxingProd.Text.ToString()))
                            {
                                Util.MessageInfo("SFU3457");
                                return;
                            }

                            GridAdd(dgBoxLot, dt);
                            setBoxCnt("포장중", Int32.Parse(nbBoxingCnt.Value.ToString()), dgBoxLot.GetRowCount());

                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 선택 )
        private void dgBoxLot_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxLot.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sColName = dgBoxLot.CurrentColumn.Name;

                    if (!sColName.Contains("CHK"))
                    {
                        return;
                    }

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region ( 취소 )
        private void btncancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                Util.gridClear(dgBoxLot);
                clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 선택 취소 )
        private void btnSelectCacel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (dgBoxLot.ItemsSource != null)
                {
                    for (int i = dgBoxLot.Rows.Count; 0 < i; i--)
                    {
                        var chkYn = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "CHK");
                        var lot_id = DataTableConverter.GetValue(dgBoxLot.Rows[i - 1].DataItem, "LOTID");

                        if (Convert.ToBoolean(chkYn))
                        {
                            dgBoxLot.EndNewRow(true);
                            dgBoxLot.RemoveRow(i - 1);

                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgBoxLot.ItemsSource);


                    if (dt.Rows.Count < 1)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3406"), null, "CAUSE", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (caution_result) =>
                        //정말 포장리스트를 삭제 하시겠습니까?
                        {
                            if (caution_result == MessageBoxResult.OK)
                            {
                                dgBoxLot.ItemsSource = null;

                                tbBoxingWait_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                                setBoxCnt("대기중", Convert.ToInt32(nbBoxingCnt.Value), 0);
                                //clearBoxingContents();
                                clear();
                            }
                            else
                            {
                                return;
                            }
                        }
                      );
                    }
                    else
                    {
                        setBoxCnt("포장중", Convert.ToInt32(nbBoxingCnt.Value), 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOXID 확인 )
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                    RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                    DataRow dr = RQSTDT.NewRow();
                    dr["BOXID"] = txtBoxId.Text;
                    dr["BOXTYPE"] = "BOX"; //"BOX";                

                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_REPACKING_PACK", "INDATA", "OUTDATA", RQSTDT);

                    //2023.03.29 PLT 포장 완료된 경우 BOX 포장 정보 변경을 막기위한 VALIDATION 추가(PACK001_015:boxValidation 참고) - KIM MIN SEOK
                    foreach (DataRow drw in dtResult.Rows)
                    {
                        if (drw["BOXSTAT"].ToString() == "PACKED")
                        {
                            ms.AlertWarning("SFU3315"); //입력오류 : 입력한 BOX는 포장완료 된 BOX입니다.[BOX 정보 확인].
                            txtBoxingProd.Text = "";
                            txtBoxingPcsg.Text = "";
                            return;
                        }
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1180"); //BOX 정보가 없습니다.
                        txtBoxId.Text = "";
                        txtBoxingProd.Text = "";
                        txtBoxingPcsg.Text = "";
                        //txtBoxId.GotFocus();
                        return;
                    }
                    //else if (!BoxWoInform())//BOX의 오더와 제품이 등록됐는지 확인.
                    //{
                    //    ms.AlertWarning("SFU3454"); //포장오더를 선택한 후 다시 스캔하세요
                    //    txtBoxingProd.Text = "";
                    //    txtBoxingPcsg.Text = "";
                    //    return;
                    //}
                    else
                    {
                        //txtBoxingModel.Text = dtResult.Rows[0]["BOXID"].ToString();
                        txtBoxingProd.Text = dtResult.Rows[0]["PRODID"].ToString();
                        txtBoxingPcsg.Text = dtResult.Rows[0]["PRDT_CLSS_CODE"].ToString();
                        txtBoxingModel.Text = dtResult.Rows[0]["PRJT_ABBR_NAME"].ToString();
                        txtProcId.Text = dtResult.Rows[0]["PROCID"].ToString();

                        chkBoxId.IsEnabled = false;
                        txtBoxId.IsEnabled = false;
                        chkBoxId.IsEnabled = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //2023.03.29 PLT 포장 완료된 경우 BOX 포장 정보 변경을 막기위한 VALIDATION 추가(PACK001_015:boxValidation 참고) - KIM MIN SEOK
        private bool BoxWoInform()
        {
            try
            {
                bool woYn = false;


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));     //       
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));   //                                 

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = txtBoxId.Text;
                dr["BOXTYPE"] = "BOX"; //"BOX";                

                RQSTDT.Rows.Add(dr);

                dtBoxWoResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_PROD_WO_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtBoxWoResult.Rows.Count > 0)
                {
                    woYn = true;
                }

                return woYn;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #region ( 조회 버튼 클릭 )
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboArea.SelectedIndex == 0)
                {
                    Util.MessageInfo("SFU1499"); //동을 선택하세요.
                    return;
                }

                search();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region (BOXID + ENTER)
        private void txtSearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtSearchBox.Text.Length == 0)
                    {
                        return;
                    }

                    SearchBox(Util.GetCondition(txtSearchBox), true);
                }
                catch (Exception ex)
                {
                    //Util.AlertInfo(ex.Message);
                    Util.MessageException(ex);
                }
            }
        }
        #endregion

        #region ( 초기화 처리 )
        private void clear()
        {
            try
            {
                chkBoxId.IsEnabled = true;
                chkBoxId.IsChecked = false;
                txtBoxId.IsEnabled = true;
                txtLotId.IsEnabled = true;
                nbBoxingCnt.IsEnabled = true;


                txtWoId.Text = "";
                txtLotId.Text = "";
                txtBoxId.Text = "";
                txtBoxingProd.Text = "";
                txtBoxingPcsg.Text = "";
                txtBoxingModel.Text = "";
                txtEqsgName.Text = "";
                txtProcId.Text = "";
                //txtProdClass.Text = "";

                Util.gridClear(dgBoxLot);

                nbBoxingCnt.Value = 5;
                tbCount.Text = "5";

                setBoxCnt("포장대기", 5, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        private void setBoxCnt(string boxing_stat, int max_cnt, int lot_cnt)
        {
            if (txtcnt == null)
            {
                return;
            }

            // 포장 여부 : 현재 포장 수량 / 최대 포장수량
            txtcnt.Text = ObjectDic.Instance.GetObjectName(boxing_stat) + " : " + lot_cnt.ToString() + " / " + max_cnt.ToString();
            //남은 수량
            tbCount.Text = (max_cnt - lot_cnt).ToString();
        }

        private void GridAdd(C1.WPF.DataGrid.C1DataGrid dg, DataTable dt)
        {
            try
            {
                int TotalRow = dg.GetRowCount();

                if (TotalRow == 0)
                {
                    return;
                }

                for (int k = 0; dt.Rows.Count > k; k++)
                {
                    dg.BeginNewRow();
                    dg.EndNewRow(true);
                    for (int i = 0; dg.Columns.Count > i; i++)
                    {
                        for (int j = 0; dt.Columns.Count > j; j++)
                        {
                            if (dg.Columns[i].Name.ToString() == dt.Columns[j].ToString())
                            {
                                DataTableConverter.SetValue(dg.Rows[TotalRow + k].DataItem, dg.Columns[i].Name.ToString(), dt.Rows[k][j].ToString());
                            }
                            //break;
                        }
                    }
                }

                DataTable dtTemp = Util.MakeDataTable(dg, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region ( BOX LABEL 버튼 클릭 )
        private void btnBoxLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                labelPrint();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 라벨 인쇄 )
        private void labelPrint()
        {
            try
            {
                if (
                string.IsNullOrWhiteSpace(txtBoxIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtEqsgIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProdIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProcIdH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtProdClassH.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtseletedWO.Text.ToString()) ||
                string.IsNullOrWhiteSpace(txtBoxQty.Text.ToString())
                //string.IsNullOrWhiteSpace(txtShipToId.Text.ToString())
                )
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                string sLBL_code = Util.NVC(cboLabelCode.SelectedValue).Length > 0 ? Util.NVC(cboLabelCode.SelectedValue) : "LBL0021";

                DataTable dtzpl = Util.getZPL_Pack(sLOTID: txtBoxIdH.Text.ToString()
                                        , sLABEL_TYPE: LABEL_TYPE_CODE.PACK_INBOX
                                        , sLABEL_CODE: sLBL_code//null /*"LBL0020"*/
                                        , sSAMPLE_FLAG: "N"
                                        , sPRN_QTY: "1"
                                        , sPRODID: txtProdIdH.Text.ToString()
                                        , sSHIPTO_ID: txtShipToId.Text.ToString()
                                        );

                if (dtzpl == null || dtzpl.Rows.Count == 0)
                {
                    return;
                }

                string zpl = Util.NVC(dtzpl.Rows[0]["ZPLSTRING"]);

                Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                DataTable dtBoxPrintHistory = setBoxResultList();

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                string print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                string print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print 여부 N인경우 Y로 update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }
                else
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(null, null);
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 박스 라벨 불러오기 ) 
        private void setBoxLabel()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { txtProdIdH.Text.ToString(), null, null, LABEL_TYPE_CODE.PACK_INBOX };

                _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");

                if (cboLabelCode.ItemsSource == null) return;

                cboLabelCode.SelectedIndex = 0;


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 이력화면 클릭 이벤트 )
        private void dgBoxhistory_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgBoxhistory.Rows.Count == 0 || dgBoxhistory == null)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgBoxhistory.GetCellFromPoint(pnt);

                if (cell == null || cell.Value == null)
                {
                    return;
                }

                int iRow = cell.Row.Index;
                int iCol = cell.Column.Index;

                txtBoxIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXID").ToString();
                txtEqsgIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "EQSGID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "EQSGID").ToString();
                txtProdIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRODID").ToString();
                txtProcIdH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PROCID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PROCID").ToString();
                txtProdClassH.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRDCLASS") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "PRDCLASS").ToString();
                txtseletedWO.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "WOID") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "WOID").ToString();
                txtBoxQty.Text = DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT") == null ? null : DataTableConverter.GetValue(dgBoxhistory.Rows[iRow].DataItem, "BOXLOTCNT").ToString();

                setBoxLabel();


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOX 상세 이력 )
        private void dgBoxhistory_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid grid = sender as C1.WPF.DataGrid.C1DataGrid;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = grid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }

                    if (cell.Column.Name == "BOXID")
                    {
                        PACK001_003_BOXINFO popup = new PACK001_003_BOXINFO();
                        popup.FrameOperation = this.FrameOperation;

                        if (popup != null)
                        {
                            DataTable dtData = new DataTable();
                            dtData.Columns.Add("BOXID", typeof(string));

                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["BOXID"] = cell.Text;

                            dtData.Rows.Add(newRow);

                            //========================================================================
                            object[] Parameters = new object[1];
                            Parameters[0] = dtData;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            //========================================================================

                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion


        #endregion

        #region [ BizActor ]

        #region ( BOXID 생성 )
        private void autoBoxIdCreate(string txtBoxingPcsg, string strProdId, string strEqsgId, string strWoId)
        {
            try
            {
                string setProcid = string.Empty;

                if (txtBoxingPcsg == "CMA")
                {
                    setProcid = "P5500";
                }
                else if (txtBoxingPcsg == "BMA")
                {
                    setProcid = "P9500";
                }
                else
                {
                    return;
                }

                //boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투이LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //ㅍ장공정 ID
                RQSTDT.Columns.Add("PRODID", typeof(string));   //lot의 제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //사용자ID
                RQSTDT.Columns.Add("WOID", typeof(string));   //사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = txtLotId.Text.ToString();
                dr["PROCID"] = setProcid;//lot_proc; // CMA:P5500, BMA:P9500
                dr["PRODID"] = strProdId; //wo가 있으면 설정된 포장제품, wo가 없으면 포장제품 찾아서 box table에 넣어줌(이 변수는 큰 의미 없음).
                dr["LOTQTY"] = nbBoxingCnt.Value.ToString();// 화면에서 선택한 수량....나중에 포장시 실제 담긴 수량으로 변경됨.         
                dr["EQSGID"] = strEqsgId;
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = strWoId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST_WO", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    txtBoxId.Text = dtResult.Rows[0][0].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region ( 포장 가능 여부 체크 ) 
        private DataTable chkReworkBox()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.GetCondition(txtLotId).Trim(); //LOTID

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_REWORK_BOXLOT", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return dtResult;
                }
                else
                {
                    return null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        #endregion

        #region  ( 포장 처리 )
        private void btnPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Boxing())
                {
                    clear();
                    Util.MessageInfo("SFU3386");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( 포장 처리 함수 )
        private Boolean Boxing()
        {
            try
            {
                string strProc = string.Empty;

                if (Util.GetCondition(txtBoxingPcsg.Text.ToString()) == "CMA")
                {
                    strProc = "P5500";
                }
                else if (Util.GetCondition(txtBoxingPcsg.Text.ToString()) == "BMA")
                {
                    strProc = "9500";
                }


                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                INDATA.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                INDATA.Columns.Add("BOXID", typeof(string));    //투이LOT(처음 LOT)
                INDATA.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정) 
                INDATA.Columns.Add("BOXQTY", typeof(string));   //투입 총수량
                INDATA.Columns.Add("EQSGID", typeof(string));   //라인ID
                INDATA.Columns.Add("USERID", typeof(string));   //사용자ID
                INDATA.Columns.Add("WOID", typeof(string));   //사용자ID

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = txtBoxId.Text.ToString();
                dr["PROCID"] = strProc;
                dr["BOXQTY"] = dgBoxLot.GetRowCount();
                dr["EQSGID"] = txtEqsgID.Text.ToString(); // BOX의 라인ID, 첫 등록 BOX 기준의 라인이 들어감
                dr["USERID"] = LoginInfo.USERID;
                dr["WOID"] = txtWoId.Text.ToString(); // BOX의 WOID
                INDATA.Rows.Add(dr);

                DataTable IN_LOTID = indataSet.Tables.Add("IN_LOTID");
                IN_LOTID.Columns.Add("LOTID", typeof(string));

                for (int i = 0; i < dgBoxLot.GetRowCount(); i++)
                {
                    string sLotId = Util.NVC(dgBoxLot.GetCell(i, dgBoxLot.Columns["LOTID"].Index).Value);

                    DataRow inDataDtl = IN_LOTID.NewRow();
                    inDataDtl["LOTID"] = sLotId;
                    IN_LOTID.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REWORK_BOXING_WO", "INDATA,IN_LOTID", "OUTDATA,OUT_LOTID", indataSet);

                if (dsResult != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    Util.MessageInfo("SFU3462"); //포장 작업 실패 BOXING BIZ 확인 하세요.
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #region ( 조회 ) 
        private void search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                //RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                //2023.10.26 - 기존 null로 들어가던 PRODOD 정보를 cboProduct로 조회하도록 수정 - KIM MIN SEOK
                dr["PRODID"] = Util.GetCondition(cboProduct) == "" ? null : Util.GetCondition(cboProduct);
                dr["MODLID"] = Util.GetCondition(cboProductModel) == "" ? null : Util.GetCondition(cboProductModel);
                dr["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                dr["TODATE"] = Util.GetCondition(dtpDateTo);
                //dr["BOXID"] = "";
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                dgBoxhistory.ItemsSource = null;

                if (dtResult.Rows.Count != 0)
                {
                    Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    txtSearchBox.Text = "";
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region (BOXID로 조회)
        private void SearchBox(string strBoxId, Boolean bGridClear)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgBoxhistory.ItemsSource);

                if (dt.Rows.Count > 0)
                {
                    if (dt.Select("BOXID = '" + strBoxId + "'").Count() > 0 && !bGridClear)
                    {
                        Util.MessageInfo("SFU8251");
                        return;
                    }
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = strBoxId;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PRODID"] = null;
                dr["MODLID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOXHISTORY_SEARCH", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (bGridClear)
                    {
                        txtSearchBox.Text = "";
                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    }
                    else
                    {
                        txtSearchBox.Text = "";

                        Util.gridClear(dgBoxhistory);

                        dt.AsEnumerable().CopyToDataTable(dtResult, LoadOption.Upsert);

                        Util.GridSetData(dgBoxhistory, dtResult, FrameOperation);
                    }
                }
                else
                {
                    Util.MessageInfo("SFU1179");
                }

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( 포장 취소 )

        private void btnPacCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtBoxIdH.Text.ToString()))
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PACK_LOT_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY", typeof(string));
                INDATA.Columns.Add("UNPACK_QTY2", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("ERP_IF_FLAG", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["BOXID"] = txtBoxIdH.Text.ToString();
                dr["PRODID"] = txtProdIdH.Text.ToString();
                dr["PACK_LOT_TYPE_CODE"] = "LOT";
                dr["UNPACK_QTY"] = txtBoxQty.Text.ToString();
                dr["UNPACK_QTY2"] = txtBoxQty.Text.ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["ERP_IF_FLAG"] = "C";
                dr["NOTE"] = "BOX UNPACK";
                dr["WOID"] = txtseletedWO.Text.ToString();
                INDATA.Rows.Add(dr);

                DataTable dsResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_BOX", "INDATA", "OUTDATA", INDATA);

                if (dsResult != null && dsResult.Rows.Count > 0)
                {
                    Util.MessageInfo("SFU3390");

                    search();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #region ( 라벨 선택시 ShipTo_ID )
        private void cboLabelCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (e.NewValue == null) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = txtProcIdH.Text.ToString();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                //기존 로직의 "LBL0020,LBL0067" 유지
                dr["LABEL_CODE"] = cboLabelCode.SelectedValue.ToString() == null ? "LBL0020,LBL0067" : cboLabelCode.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_BOX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt.Rows.Count > 0)
                {
                    txtShipToId.Text = dt.Rows[0]["SHIPTO_ID"].ToString();
                }
                else
                {
                    txtShipToId.Text = "";
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region ( BOX 라벨 발행 요청 이력 조회 )
        private DataTable setBoxResultList()
        {
            try
            {
                //DA_PRD_SEL_BOX_LIST_FOR_LABEL

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                dr["PROCID"] = null;
                dr["EQPTID"] = null;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["BOXID"] = Util.GetCondition(txtBoxIdH) == "" ? null : Util.GetCondition(txtBoxIdH);
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                return dtboxList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region ( BOX 라벨 발행 이력 UPDATE )
        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid = null, string sPRT_SEQ = null)
        {
            try
            {
                //DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = sPRT_SEQ;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG_USERID", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #endregion

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtLotId.Clear();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void MenuItem_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLotId.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtLotId.SelectedText.ToString());
                txtLotId.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBoxId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        string sPasteString = Clipboard.GetText();

                        if (!string.IsNullOrWhiteSpace(sPasteString))
                        {

                            Util.MessageInfo("SFU3180");
                            txtBoxId.Clear();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_BoxId_Copy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxId.SelectedText.ToString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void MenuItem_BoxId_Cut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(txtBoxId.SelectedText.ToString());
                txtBoxId.Clear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
