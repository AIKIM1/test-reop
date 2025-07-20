/*************************************************************************************
 Created Date : 2020.12.28
      Creator : 이제섭
   Decription : Cell 교체 처리(취출)
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.28  DEVELOPER : Initial Created.
  2022.10.19  이제섭    : IR 2회 NG 시, 자동 불량 등록 기능 추가. (ESNB Only) (동에 대한 구분은 Biz 내부에서 처리함.)
  2023.03.28  박나연    : 자동 포장 팔레트에는 자동 포장기로 작업했었던 CELL만 포장 가능하도록 validation 추가 (CELL_CHANGE_CHK_AREA)
  2023.05.31  조영대    : Pallet Barcode ID 컬럼 추가
  2023.06.26  홍석원    : Pallet Barcode ID 컬럼 추가 (조회오류 수정)
  2023.07.25  홍석원    : TOP_PRODID 컬럼 추가
  2023.12.14  홍석원    : Cell 추가 시 Pallet에 포함된 Cell이 있는 경우 확인 메시지 표시 처리, TAG 재발행 시 제품ID가 완제품ID로 출력되도록 수정
  2023.12.14  김호선    : (ESNJ 특화) Cell 추가시 수량체크 유무 선택기능(종이박스인 경우 셀 수량이 480개여서 수량 Vaildation에 걸림)
  2024.02.15  최경아    : Cell 추가,Cell 교체시 시생산 팔렛에 다른 타입 Cell 혼입 불가 및 cell 정보 popup 기능 추가. 
  2024.06.20  김용준    : 2D BCR 스캔 기능 추가 건 - (2D BCR 사용여부 CheckBox로 대체) [E20240601-001909]
  2024.06.25  박나연    : 다른 PALLET에 포장 진행 중인 CELL 추가 불가 INTERLOCK 추가 [E20240621-001674]
  2024.07.08  이현승    : 사외반품여부 컬럼 추가
  2024.07.22  최석준    : 반품여부 추가 (2025년 적용예정, 수정시 연락 부탁드립니다)
  2025.04.01  복현수    : 입력받은 팔렛아이디 대문자 처리 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_302 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        //bool _bChangeFlag = false;
        //bool _bMappingFlag = false;
        //bool _bUnMappingFlag = false;
        private bool bPilotChk = false;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private bool bTrayAdminChk = false;
        private int trayCellQty = 0; // mmd tray 수량
        private int trayChkQty = 0;
        private string palletEqsgid = string.Empty;
        private string palletProdid = string.Empty;
        private string palletSrctype = string.Empty;
        private string palletLottype = string.Empty;
        string sIRDefectCode = string.Empty;

        public BOX001_302()
        {
            InitializeComponent();

            Initialize();
        }
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();

        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        List<string> BCDType = new List<string>() {"1D바코드", "2D바코드" };

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            Loaded -= UserControl_Loaded;
            InitSet();

            if(GetTrayAdminArea() > 0)
            {
                bTrayAdminChk = true;
                TargettrayID.Visibility = Visibility.Visible;
                cboTray.Visibility = Visibility.Visible;

            }
            else
            {
                bTrayAdminChk = false;
                TargettrayID.Visibility = Visibility.Collapsed;
                cboTray.Visibility = Visibility.Collapsed;
            }
            // 수량 체크박스 표시 유무
            if (!GetPalletTotalQty_ChkYN()) 
            {
                chkPalletQty.Visibility = Visibility.Hidden;

            }
            GetIRDefectCode();
        }
     
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange1);
            listAuth.Add(btnChange2);
            listAuth.Add(btnChange3);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Initialize

        private void InitSet()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            dtpDateFrom.Text = DateTime.Now.ToString();
            dtpDateTo.Text = DateTime.Now.ToString();
            // Area 셋팅
            // _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            chkChangeYn.IsChecked = true;

            txtSourceCellID.Focus();

            //바코드 ID 컬럼 Visible
            dgTargetCell1.SetColumnVisibleForCommonCode("PLLT_BCD_ID", "CELL_PLT_BCD_USE_AREA", LoginInfo.CFG_AREA_ID);
            dgSourceCell1.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgPallet2.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgSourceCell2.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgPallet3.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgSourceCell3.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgPallet.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
            dgCellChangeHis.Columns["PLLT_BCD_ID"].Visibility = dgTargetCell1.Columns["PLLT_BCD_ID"].Visibility;
        }

        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgTargetCell1.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
                dgSourceCell1.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
                dgSourceCell2.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
                dgSourceCell3.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Event

        /// <summary>
        /// 교체 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 조회 전 이전 데이터 초기화
            dgCellChangeHis.ItemsSource = null;

            string from = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";  //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyy-MM-dd") + " 00:00:00";
            string to = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59"; //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyy-MM-dd") + " 23:59:59";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("TO_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("CELLID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MDLOT_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if ((!string.IsNullOrWhiteSpace(txtCellid.Text)) || (!string.IsNullOrWhiteSpace(txtBoxId.Text)))
                {
                    if (!string.IsNullOrWhiteSpace(txtCellid.Text)) dr["CELLID"] = txtCellid.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(txtBoxId.Text))
                    {
                        dr["BOXID"] = ConvertBarcodeId(txtBoxId.Text.Trim());
                    }
                }
                else
                {
                    dr["FROM_DTTM"] = from;
                    dr["TO_DTTM"] = to;
                    dr["AREAID"] = sAREAID;
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["MDLOT_ID"] = Util.NVC(cboModelLot.SelectedValue) == "" ? null : Util.NVC(cboModelLot.SelectedValue);
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_REPLACE_SUBLOT_HIST_BX", "RQSTDT", "RSLTDT", RQSTDT);
                ////dgCellChangeHis.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgCellChangeHis, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// CELL 교체처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet.GetRowCount() == 0 || dgCellList.GetRowCount() == 0)
                {
                    Util.MessageValidation("3"); //"입력데이터가 없습니다."  
                    txtSourceCellID.Focus();
                    return;
                }

                if (txtReason.Text == string.Empty)
                {
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    txtReason.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                inDataTable.Columns.Add("BOXID", typeof(string));                
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("BCR2D", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["REPACK_FLAG"] = chkChangeYn.IsChecked == true ? "Y" : "N";
                dr["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID"));
                dr["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;    // sUser;
                dr["NOTE"] = txtReason.Text.Trim();
                inDataTable.Rows.Add(dr);

                string from_sublotid = string.Empty;
                string to_sublotid = string.Empty;

                for (int i = 2; i < dgCellList.Rows.Count; i++)
                {
                    from_sublotid = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID"));

                    if (from_sublotid == string.Empty)
                    {
                        Util.MessageValidation("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }

                    to_sublotid = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID"));

                    if(chkChangeYn.IsChecked == true && to_sublotid == string.Empty)
                    {
                        Util.MessageValidation("SFU1462"); //"교체 대상 Cell ID가 입력되지 않았습니다."
                        return;
                    }

                    DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                    drsub["FROM_SUBLOTID"] = from_sublotid;
                    drsub["TO_SUBLOTID"] = to_sublotid;
                    drsub["BCR2D"] = string.Empty;

                    indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REPLACE_SUBLOT_BX", "INDATA,INSUBLOT", string.Empty, indataSet);               

                // 메시지 출력
                Util.MessageInfo("SFU1275"); //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                AllClear();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// EXCEL 교체
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExlChange_Click(object sender, RoutedEventArgs e)
        {
            string MAINFORMPATH = "LGC.GMES.MES.BOX001";
            string MAINFORMNAME = "BOX001_008_CHANGE_EXL";

            Assembly asm = Assembly.LoadFrom("ClientBin\\" + MAINFORMPATH + ".dll");
            Type targetType = asm.GetType(MAINFORMPATH + "." + MAINFORMNAME);
            object obj = Activator.CreateInstance(targetType);

            IWorkArea workArea = obj as IWorkArea;
            workArea.FrameOperation = FrameOperation;

            C1Window popup = obj as C1Window;
            if (popup != null)
            {
                popup.Closed -= popup_Closed;
                popup.Closed += popup_Closed;
                popup.ShowModal();
                popup.CenterOnScreen();
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sArea = LoginInfo.CFG_AREA_ID;
                    string sPalletID = string.Empty;
                    sPalletID = txtPalletID.Text.ToString().ToUpper().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    GetPalletInfo(sPalletID, sArea);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }
        void popup_Closed(object sender, EventArgs e)
        {

        }

        private void chk2DBCR_Checked(object sender, RoutedEventArgs e)
        {
            chk2DBCR.IsChecked = true;
            chk2DBCR2.IsChecked = true;
            chk2DBCR3.IsChecked = true;
        }

        private void chk2DBCR_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk2D = (CheckBox)sender;

            if (chk2D.IsChecked == false)
            {
                Util.MessageConfirm("SFU10019", (result) =>
                {
                    if (result == MessageBoxResult.Cancel)
                    {
                        chk2DBCR.IsChecked = true;
                        chk2DBCR2.IsChecked = true;
                        chk2DBCR3.IsChecked = true;
                    }
                    else if (result == MessageBoxResult.OK)
                    {
                        chk2DBCR.IsChecked = false;
                        chk2DBCR2.IsChecked = false;
                        chk2DBCR3.IsChecked = false;
                    }
                });
            }
        }        

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            AllClear();
        }

        private void btnBatchDel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSourceCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sSourceCellID = string.Empty;
                    sSourceCellID = txtSourceCellID.Text.ToString().Trim();

                    if (sSourceCellID == null)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                      //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1495"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable SearchResult = GetCellInfo(sSourceCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageInfo("SFU1905");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (dgPallet.GetRowCount() == 0)
                    {
                        GetPalletInfo(SearchResult.Rows[0]["OUTER_BOXID"].ToString(), LoginInfo.CFG_AREA_ID);
                    }
                    else 
                    { 
                        if (chkChangeYn.IsChecked == true && DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID").ToString() != SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                        {
                            //동일 팔레트 적재된 Cell 이 아닙니다.
                            Util.MessageValidation("SFU3148");
                         //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3148"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                            Util.MessageValidation("SFU1502");
                          //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    int seqno = dgCellList.GetRowCount();

                    if (seqno == 0)
                    {
                        InitCellList();
                    }
                    else
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString()
                                || Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString())
                            {
                                //중복된 Cell을 입력하였습니다.
                                Util.MessageValidation("SFU3170");
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3170"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    dgCellList.IsReadOnly = false;
                    dgCellList.BeginNewRow();
                    dgCellList.EndNewRow(true);
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "SEQ_NO", (seqno + 1).ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                    //DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "FROM_RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "RTN_FLAG", SearchResult.Rows[0]["RTN_FLAG"].ToString());
                    dgCellList.IsReadOnly = true;      
                                                                         
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    if (chkChangeYn.IsChecked == true)
                    {
                        txtTargetCellID.Text = string.Empty;
                        txtTargetCellID.Focus();
                        txtTargetCellID.SelectAll();
                    }
                    else
                    {
                        txtSourceCellID.Focus();
                        txtSourceCellID.SelectAll();
                    }
                }
            }
        }

        private void txtTargetCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgPallet.GetRowCount() == 0 || dgCellList.GetRowCount() == 0)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1495"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string sCellID = string.Empty;
                    sCellID = txtTargetCellID.Text.ToString().Trim();

                    if (sCellID == null)
                    {
                        //교체 Cell ID 가 없습니다.
                        Util.MessageValidation("SFU1462");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905");
                     //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "BOXID").ToString() == SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                    {
                        //동일 팔레트 존재하는 Cell은 교체 대상에 등록할 수 없습니다.
                        Util.MessageValidation("SFU3149");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3149"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (DataTableConverter.GetValue(dgPallet.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                    {
                        //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    int seqno = dgCellList.GetRowCount();

                    if (seqno == 0)
                    {
                        InitCellList();
                    }
                    else
                    {
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FROM_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString()
                                || Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "TO_SUBLOTID")) == SearchResult.Rows[0]["SUBLOTID"].ToString())
                            {
                                //중복된 Cell을 입력하였습니다.
                                Util.MessageValidation("SFU3170");
                              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3170"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                    }

                    dgCellList.IsReadOnly = false;                    
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_LOTID", SearchResult.Rows[0]["LOTID"].ToString());                    
                    DataTableConverter.SetValue(dgCellList.CurrentRow.DataItem, "TO_RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                    dgCellList.IsReadOnly = true;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                finally
                {
                    txtSourceCellID.Text = string.Empty;
                    txtSourceCellID.Focus();
                    txtSourceCellID.SelectAll();
                }
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
           Util.MessageConfirm("SFU1230", (result) =>
           {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;                    
                }
            });
        }

        private void chkChangeYn_Click(object sender, RoutedEventArgs e)
        {
            if (chkChangeYn.IsChecked == true)
            {
                txtTargetCellID.IsEnabled = true;
            }
            else
            {
                txtTargetCellID.IsEnabled = false;
            }
        }

        #endregion

        #region Mehod

        private void GetPalletInfo(string sPalletID, string sArea)
        {
            sPalletID = ConvertBarcodeId(sPalletID);

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("AREAID", typeof(String));
            RQSTDT.Columns.Add("BOXID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["AREAID"] = sArea;
            dr["BOXID"] = sPalletID;

            RQSTDT.Rows.Add(dr);

            DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

            if (SearchResult.Rows.Count == 0)
            {
                //조회된 Data가 없습니다.
                Util.MessageInfo("SFU1905");
             //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }

            ////dgPallet.ItemsSource = DataTableConverter.Convert(SearchResult);
            Util.GridSetData(dgPallet, SearchResult, FrameOperation);
            txtPalletID.IsReadOnly = true;
            txtPalletID.Text = SearchResult.Rows[0]["BOXID"].ToString();
            chkChangeYn.IsEnabled = false;
        }

        private bool GetPalletInfo(C1.WPF.DataGrid.C1DataGrid grid, string sPalletID)
        {
            try
            {
                sPalletID = ConvertBarcodeId(sPalletID);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = null;
                dr["BOXID"] = sPalletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_UNPACK", "INDATA", "OUTDATA", RQSTDT);

                if (dtPallet == null || dtPallet.Rows.Count == 0)
                {
                    //팔레트 정보가 없습니다.
                    Util.MessageInfo("SFU1994");
                    return false;
                }
                Util.GridSetData(grid, dtPallet, FrameOperation);
                palletEqsgid = Util.NVC(dtPallet.Rows[0]["EQSGID"]);
                palletProdid = Util.NVC(dtPallet.Rows[0]["PRODID"]);
                palletSrctype = Util.NVC(dtPallet.Rows[0]["PACK_WRK_TYPE_CODE"]);

                if (bTrayAdminChk == true)
                {
                    GetTrayID(sPalletID);
                    AddTrayCell();
                    GetTrayQty(cboTray.SelectedValue.ToString());
                }

                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT1";
                RQSTDT1.Columns.Add("LANGID", typeof(String));
                RQSTDT1.Columns.Add("LOTID", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["LOTID"] = sPalletID;

                RQSTDT1.Rows.Add(dr1);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_BY_PLLT", "RQSTDT", "RSLTDT", RQSTDT1); //최경아:DA_PRD_SEL_VW_LOT -> DA_SEL_LOT_BY_PLLT 변경

                if (dtLot != null && dtLot.Rows.Count > 0)
                {
                    palletLottype = Util.NVC(dtLot.Rows[0]["LOTTYPE"]);
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                cboTray.ItemsSource = null;
                return false;
            }
        }

        private DataTable GetCellInfo(string sCellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(String));
            RQSTDT.Columns.Add("SUBLOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SUBLOTID"] = sCellID;

            RQSTDT.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_INFO_BX", "INDATA", "OUTDATA", RQSTDT); //BR_GET_SUBLOT_INFO_FOR_REPLACE_BX ->DA_BAS_SEL_SUBLOT_INFO_BX
        }

        private bool GetCellHoldInfo(string sCellID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellID;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_HOLD", "INDATA", null, RQSTDT);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ClearDataGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.ItemsSource = null;
            //dg.BottomRows[0].Visibility = Visibility.Collapsed;
        }

        private void InitCellList()
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SEQ_NO", typeof(int));
            dtInit.Columns.Add("FROM_SUBLOTID", typeof(string));
            dtInit.Columns.Add("TO_SUBLOTID", typeof(string));
            dtInit.Columns.Add("2DBARCODE", typeof(string));
            dtInit.Columns.Add("FROM_OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("FROM_INNER_BOXID", typeof(string));
            dtInit.Columns.Add("FROM_BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("FROM_LOTID", typeof(string));
            dtInit.Columns.Add("FROM_RCV_ISS_ID", typeof(string));
            dtInit.Columns.Add("TO_OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("TO_INNER_BOXID", typeof(string));
            dtInit.Columns.Add("TO_BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("TO_LOTID", typeof(string));
            dtInit.Columns.Add("TO_RCV_ISS_ID", typeof(string));
            dtInit.Columns.Add("RTN_FLAG", typeof(string));

            dgCellList.ItemsSource = DataTableConverter.Convert(dtInit);
            ////Util.GridSetData(dgCellList, dtInit, FrameOperation);
        }

        private void InitCellList(C1.WPF.DataGrid.C1DataGrid dg)
        {
            DataTable dtInit = new DataTable();

            dtInit.Columns.Add("SEQ", typeof(int));
            dtInit.Columns.Add("SUBLOTID", typeof(string));            
            dtInit.Columns.Add("2DBARCODE", typeof(string));
            dtInit.Columns.Add("OUTER_BOXID", typeof(string));
            dtInit.Columns.Add("PLLT_BCD_ID", typeof(string));
            dtInit.Columns.Add("INNER_BOXID", typeof(string));
            dtInit.Columns.Add("BOXSEQ", typeof(string));
            dtInit.Columns.Add("BOX_PSTN_NO", typeof(string));
            dtInit.Columns.Add("LOTID", typeof(string));
            dtInit.Columns.Add("RCV_ISS_ID", typeof(string));
            dtInit.Columns.Add("PRODID", typeof(string));
            dtInit.Columns.Add("TOP_PRODID", typeof(string));
            dtInit.Columns.Add("LOTTYPE", typeof(string));
            dtInit.Columns.Add("LOTTYPE_NM", typeof(string));
            dtInit.Columns.Add("POSITION", typeof(string));
            dtInit.Columns.Add("RTN_FLAG", typeof(string));

            dg.ItemsSource = DataTableConverter.Convert(dtInit);
            ////Util.GridSetData(dgCellList, dtInit, FrameOperation);
        }

        private void AllClear()
        {
            ClearDataGrid(dgPallet);
            ClearDataGrid(dgCellList);

            txtPalletID.Text = string.Empty;
            txtReason.Text = string.Empty;
            txtSourceCellID.Text = string.Empty;
            txtTargetCellID.Text = string.Empty;

            txtPalletID.IsReadOnly = false;
            chkChangeYn.IsEnabled = true;
        }

        private void AllClear1(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet1.Text = string.Empty;
                txtReason1.Text = string.Empty;
            }

            ClearDataGrid(dgTargetCell1);
            ClearDataGrid(dgSourceCell1);

            lbl2DBCD_Target.Text = string.Empty;
            lbl2DBCD_Source.Text = string.Empty;

            txtSourceCellID1.Text = string.Empty;
            txtTargetCellID1.Text = string.Empty;            
        }

        private void AllClear2(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet2.Text = string.Empty;
                txtReason2.Text = string.Empty;
            }
            ClearDataGrid(dgPallet2);
            ClearDataGrid(dgSourceCell2);
            
            txtPalletID2.Text = string.Empty;
            txtSourceCellID2.Text = string.Empty;            
        }

        private void AllClear3(bool bAll = false)
        {
            if (bAll)
            {
                txtTagPallet3.Text = string.Empty;
                txtReason3.Text = string.Empty;
            }
            ClearDataGrid(dgPallet3);
            ClearDataGrid(dgSourceCell3);

        
            txtPalletID3.Text = string.Empty;
            txtSourceCellID3.Text = string.Empty;            
        }

        /// <summary>
        /// UNCODE 필수입력 Plant 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodePlant()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PLT_UNCODE_SHOP";
            dr["CMCODE"] = LoginInfo.CFG_SHOP_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        // 바코드 ID ==> Pallet ID 입력 변환
        private string ConvertBarcodeId(string lotId)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["CSTID"] = lotId;
            dtRqst.Rows.Add(drRqst);

            DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", dtRqst);
            if (dtPallet != null && dtPallet.Rows.Count > 0)
            {
                return Util.NVC(dtPallet.Rows[0]["CURR_LOTID"]);
            }
            return lotId;
        }
        #endregion

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = Util.NVC(cboArea.SelectedValue);
            if (sTemp == "" || sTemp == "SELECT")
            {
                sAREAID = "";
                sSHOPID = "";
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sAREAID = sArry[0];
                sSHOPID = sArry[1];
            }

            String[] sFilter = { sAREAID };    // Area
            _comboF.SetCombo(cboEquipmentSegment, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter, sCase: "LINE"); //, sCase: "EQSGID_PACK");
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //모델 Combo Set.
            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _comboF.SetCombo(cboModelLot, CommonCombo_Form.ComboStatus.ALL, cbParent: cboParent, sCase: "cboModelLot");
        }
        private string SelectCellID(string sBCR)
        {
            //QR_TC_2D_BCR_CELL

            try
            {
                string sCellID = sBCR.Substring(49, 9); 

                //BR_PRD_CHK_PACKING_CELL
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA_ROUTE");
                inDataTable.Columns.Add("AREAID", typeof(string));

                DataTable inDataCellTable = indataSet.Tables.Add("INDATA");
                inDataCellTable.Columns.Add("BCR", typeof(string));
                inDataCellTable.Columns.Add("CELL_ID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataTable.Rows.Add(inData);

                DataRow inDataCell = inDataCellTable.NewRow();
                inDataCell["BCR"] = sBCR;
                inDataCell["CELL_ID"] = sCellID;
                inDataCellTable.Rows.Add(inDataCell);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_INF_SEL_2DBCR_BY_CELL", "INDATA_ROUTE,INDATA", "OUTDATA", indataSet);
                
                return dsResult.Tables["OUTDATA"].Rows[0]["CELL_ID"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageValidation("SFU3248", new object[] { sBCR });
                return string.Empty; 
            }

        }

        private void txtTargetCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    Util.gridClear(dgTargetCell1);
                    TextBox txtCellid = sender as TextBox;
                  

                    string sCellID = txtCellid.Text.ToString().Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();
                    lbl2DBCD_Target.Text = string.Empty;
                    txtTagPallet1.Text = string.Empty;                    

                    if (sCellID == null)
                    {
                        //대상 Cell ID 가 없습니다. >> 대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                    // 2024-06-20 KYJ : 2D BCR 사용여부 CheckBox로 대체 [E20240601-001909]
                    //bool b2DBCD = sCellID.Length > 60;
                    bool b2DBCD = (chk2DBCR.IsChecked == true ? true:false);
                    

                    if (b2DBCD) 
                    {
                        string sData = SelectCellID(sCellID);

                        if (string.IsNullOrWhiteSpace(sData))
                        {                           
                            return;
                        }
                        
                        sCellID = sData;
                    }  
                              

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                   

                    if (string.IsNullOrWhiteSpace(Util.NVC(SearchResult.Rows[0]["OUTER_BOXID"])))
                    {
                        //팔레트 구성된 Cell만 교체 가능합니다.
                        Util.MessageValidation("SFU3483", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtTargetCellID1.Focus();
                                txtTargetCellID1.SelectAll();
                            }
                        });
                        return;
                    }

                    Util.GridSetData(dgTargetCell1, SearchResult, FrameOperation);

                    txtCellid.Text = sCellID;
                    lbl2DBCD_Target.Text = !b2DBCD ? ObjectDic.Instance.GetObjectName(BCDType[0]) : ObjectDic.Instance.GetObjectName(BCDType[1]);

                    txtSourceCellID1.Focus();
                    txtSourceCellID1.SelectAll();

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                }
            }
        }

        private void txtSourceCellID1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (dgTargetCell1.GetRowCount() == 0)
                    {
                        //대상 Cell ID가 입력되지 않았습니다.
                        Util.MessageValidation("SFU1495");
                        return;
                    }

                    string sCellID = string.Empty;
                    sCellID = txtSourceCellID1.Text.Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim();

                    if (sCellID == null)
                    {
                        //교체 Cell ID 가 없습니다.
                        Util.MessageValidation("SFU1462");
                        return;
                    }

                    // 2024-06-20 KYJ : 2D BCR 사용여부 CheckBox로 대체 [E20240601-001909]
                    //bool b2DBCD = sCellID.Length > 60;
                    bool b2DBCD = (chk2DBCR.IsChecked == true ? true : false);
                    

                    lbl2DBCD_Source.Text = !b2DBCD ? ObjectDic.Instance.GetObjectName(BCDType[0]) : ObjectDic.Instance.GetObjectName(BCDType[1]);

                    if (lbl2DBCD_Target.Text != lbl2DBCD_Source.Text)
                    {
                        // 대상 Cell 바코드와 동일한 타입으로 스캔하세요.
                        Util.MessageValidation("SFU3480");
                        return;
                    }

                    if (b2DBCD) 
                    {
                        string sData = SelectCellID(sCellID);

                        if (string.IsNullOrWhiteSpace(sData))
                        {
                            return;
                        }

                        sCellID = txtSourceCellID1.Text = sData;                       
                    }

                    DataTable SearchResult = GetCellInfo(sCellID);

                    if (SearchResult == null || SearchResult.Rows.Count == 0)
                    {
                        //조회된 Data가 없습니다.
                        Util.MessageValidation("SFU1905");
                        return;
                    }

                    if (Util.NVC(DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID")) == Util.NVC(SearchResult.Rows[0]["OUTER_BOXID"]))
                    {
                        //동일 팔레트 존재하는 Cell은 교체 대상에 등록할 수 없습니다.
                        Util.MessageValidation("SFU3149");
                        return;
                    }

                    if (DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                    {
                        //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                        Util.MessageValidation("SFU1502");
                        return;
                    }
           
                    if (GetCellChkArea() > 0) //공통코드에 등록된 동만 체크함
                    {

                        // 해당 Cell이 자동인지 수동인지 체크
                        // 교체 Cell이 포장되어 있는지 체크                      

                        DataTable SearchSrcType = GetSrcType(sCellID);

                        if (SearchSrcType.Rows.Count > 0)
                        {
                            SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = SearchSrcType.Rows[0]["PACK_WRK_TYPE_CODE"];
                        }
                        else
                        {
                            //이력정보가 없다는 뜻은 한번도 포장한 이력이 없다는 뜻임 : 그래서 강제적으로 UI로 입력함
                            SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = "UI";
                        }

                        if (DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE").ToString() == "EQ" && SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"].ToString() != "EQ")
                        {
                            //대상이 Cell이 자동포장일 경우  교체Cell도 자동포장이어야 합니다.
                            Util.MessageValidation("SFU8476");
                            return;
                        }
                    }

                    string tpLottype = string.Empty;
                    DataTable tCellP = new DataTable();
                    tCellP.Columns.Add("LANGID", typeof(String));
                    tCellP.Columns.Add("LOTID", typeof(String));

                    DataRow dr1 = tCellP.NewRow();
                    dr1["LANGID"] = LoginInfo.LANGID;
                    dr1["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID"));
                    tCellP.Rows.Add(dr1);

                    DataTable tPallet = new ClientProxy().ExecuteServiceSync("DA_SEL_LOT_BY_PLLT", "RQSTDT", "RSLTDT", tCellP); //최경아:DA_PRD_SEL_VW_LOT -> DA_SEL_LOT_BY_PLLT 변경

                    if (tPallet != null && tPallet.Rows.Count > 0)
                    {
                       tpLottype = Util.NVC(tPallet.Rows[0]["LOTTYPE"]);
                    }

                    if (tpLottype != SearchResult.Rows[0]["LOTTYPE"].ToString()
                      && ((tpLottype == "X" || tpLottype == "L") || (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X" || SearchResult.Rows[0]["LOTTYPE"].ToString() == "L")))
                    {

                        DataTable dtT = DataTableConverter.Convert(dgTargetCell1.ItemsSource);

                        string boxlist1 = string.Empty;
                        string boxlist2 = string.Empty;

                        for (int row = 0; row < dtT.Rows.Count; row++)
                        {
                            boxlist1 += "," + dtT.Rows[row]["OUTER_BOXID"].ToString();
                        }

                        if (string.IsNullOrWhiteSpace(boxlist1))
                        {
                            Util.AlertInfo("SFU1180"); //BOX 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            boxlist2 = boxlist1.Substring(1);
                        }


                        DataTable inData = new DataTable();
                        inData.Columns.Add("LANGID");
                        inData.Columns.Add("BOX_LIST");

                        DataRow inr = inData.NewRow();
                        inr["LANGID"] = LoginInfo.LANGID;
                        inr["BOX_LIST"] = boxlist2;
                        inData.Rows.Add(inr);

                        DataTable piResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", inData); // 시생산 cell 조회
                        DataTable piNResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_NOT_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", inData); // 시생산 아닌 cell 조회
                        
                        if (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X" || SearchResult.Rows[0]["LOTTYPE"].ToString() == "L")
                        {
                            DataRow pi = piResult.NewRow();
                            pi["SUBLOTID"] = SearchResult.Rows[0]["SUBLOTID"].ToString();
                            pi["LOTTYPE_NM"] = SearchResult.Rows[0]["LOTTYPE_NM"].ToString();
                            pi["BOXID"] = SearchResult.Rows[0]["OUTER_BOXID"].ToString();
                            pi["POSITION"] = SearchResult.Rows[0]["POSITION"].ToString();
                            piResult.Rows.Add(pi);
                        }
                        else
                        {
                            DataRow piN = piNResult.NewRow();
                            piN["SUBLOTID"] = SearchResult.Rows[0]["SUBLOTID"].ToString();
                            piN["LOTTYPE_NM"] = SearchResult.Rows[0]["LOTTYPE_NM"].ToString();
                            piN["BOXID"] = SearchResult.Rows[0]["OUTER_BOXID"].ToString();
                            piN["POSITION"] = SearchResult.Rows[0]["POSITION"].ToString();
                            piNResult.Rows.Add(piN);
                        }

                        if (piResult.Rows.Count > 0) // 시생산 cell 존재하는 경우
                        {
                            // 시생산 cell 있는 경우 popup
                            BOX001_PILOT_DETL popUp = new BOX001_PILOT_DETL();
                            popUp.FrameOperation = this.FrameOperation;

                            if (popUp != null)
                            {
                                object[] Parameters = new object[2];
                                Parameters[0] = piResult;
                                Parameters[1] = piNResult;

                                C1WindowExtension.SetParameters(popUp, Parameters);

                                popUp.Closed += new EventHandler(wndConfirm_Closed);

                                // 팝업 화면 숨겨지는 문제 수정.
                                //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));

                                grdMain.Children.Add(popUp);
                                popUp.BringToFront();
                            }
                        }
                        return;
                    }
                    DataSet indataSet = new DataSet();
                    DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                    RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                    RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));
                    //RQSTDT.Columns.Add("PILOT_MODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = sCellID;
                    dr["SHOPID"] = string.Empty;
                    dr["AREAID"] = string.Empty;
                    dr["EQSGID"] = string.Empty;
                    dr["MDLLOT_ID"] = string.Empty;
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "Y";
                    dr["INSP_SKIP_FLAG"] = "Y";
                    dr["2D_BCR_SKIP_FLAG"] = b2DBCD ? "N" : "Y";
                    dr["USERID"] = txtWorker.Tag.ToString();
                    //dr["PILOT_MODE"] = bPilotChk ? "Y" : "N";
                    RQSTDT.Rows.Add(dr);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_CHK_FORM_DATA_VALIDATION_BX", "INDATA", "OUTDATA", indataSet);

                    Util.GridSetData(dgSourceCell1, SearchResult, FrameOperation);

                }
                catch (Exception ex)
                {
                    if (ex.Data["CODE"].ToString() == "100000089")
                    {
                        SetSublotDefect(txtSourceCellID1.Text.Replace(Char.ConvertFromUtf32(29), string.Empty).Replace(Char.ConvertFromUtf32(30), string.Empty).Trim());
                    }

                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void btnChange1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTargetCell1.GetRowCount() == 0 || dgSourceCell1.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."              
                    return;
                }

                if (txtReason1.Text == string.Empty)
                {
                    txtReason1.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843"); //작업자를 선택해 주세요
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
               Util.MessageConfirm("SFU1170", (msgresult) =>
                {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            string sPalletID = string.Empty;

                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                            inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                            inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("BCR2D", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "Y";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgTargetCell1.CurrentRow.DataItem, "OUTER_BOXID"));
                            dr["USERID"] = txtWorker.Tag as string;//LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason1.Text.Trim();
                            dr["LOTTERM_FLAG"] = chkLotTerm.IsChecked == true ? "Y" : "N";
                            inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            from_sublotid = Util.NVC(DataTableConverter.GetValue(dgTargetCell1.Rows[0].DataItem, "SUBLOTID"));
                            to_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                            if (from_sublotid == string.Empty)
                            {
                                Util.AlertInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                return;
                            }

                            if (to_sublotid == string.Empty)
                            {
                                Util.AlertInfo("SFU1462"); //"교체 대상 Cell ID가 입력되지 않았습니다."
                                return;
                            }

                            DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                            drsub["FROM_SUBLOTID"] = from_sublotid;
                            drsub["TO_SUBLOTID"] = to_sublotid;
                            drsub["BCR2D"] = string.Empty;

                            indataSet.Tables["INSUBLOT"].Rows.Add(drsub);

                            new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REPLACE_SUBLOT_BX", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtTargetCellID1.Focus();
                                    txtTargetCellID1.SelectAll();
                                }
                            });
                            txtTagPallet1.Text = sPalletID;
                            AllClear1();                           
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit2_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear2(true);
                }

            }); 
        }

        private void btnChange2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet2.GetRowCount() == 0 || dgSourceCell2.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (txtReason2.Text == string.Empty)
                {
                    txtReason2.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                string messageID = "SFU1170"; //작업을 진행하시겠습니까?
                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    string palletID = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "OUTER_BOXID")); 
                    
                    if (!string.IsNullOrEmpty(palletID))
                    {
                        messageID = "SFU1260"; // 추가되는 CELL 중에는 이미 Pallet에 구성이 되어 있는 CELL이 있습니다. 추가 진행하시겠습니까?
                        break;
                    }
                }

               Util.MessageConfirm(messageID, (msgresult) =>
               {
                    if (msgresult == MessageBoxResult.OK)
                    {
                       try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                           if (chkPalletQty.IsChecked != true)
                           {
                               inDataTable.Columns.Add("TOTAL_QTY_CHK", typeof(string)); // 2023-12-14 KHS 추가
                           }
                           //inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                           inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                           inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                           inSublotTable.Columns.Add("BCR2D", typeof(string));
                           inSublotTable.Columns.Add("TRAYID", typeof(string));

                           DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "N";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID"));
                            dr["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason2.Text.Trim();
                           if(chkPalletQty.IsChecked != true)
                           {
                               dr["TOTAL_QTY_CHK"] =  "N";
                           }
                            
                           //dr["LOTTERM_FLAG"] = chkLotTerm2.IsChecked == true ? "Y" : "N";

                           inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                            {
                                from_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID"));
                                to_sublotid = null; //Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                                if (from_sublotid == string.Empty)
                                {
                                    Util.AlertInfo("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }
                                
                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["FROM_SUBLOTID"] = from_sublotid;
                                drsub["TO_SUBLOTID"] = to_sublotid;
                                drsub["BCR2D"] = string.Empty;
                                drsub["TRAYID"] = Util.NVC(DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "INNER_BOXID"));
                               indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REPLACE_SUBLOT_BX", "INDATA,INSUBLOT", string.Empty, indataSet);

                            // "정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID2.Focus();
                                    txtPalletID2.SelectAll();
                                }
                            });

                            txtTagPallet2.Text = sPalletID;
                            AllClear2();
                            if (bTrayAdminChk == true)
                            {
                               GetTrayID(null);
                            }
                       }
                        catch (Exception ex)
                        {           
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnInit3_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                   AllClear3(true);
                }

            }); 
        }

        private void btnChange3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0 || dgSourceCell3.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU1801"); //"입력 데이터가 존재하지 않습니다."
                    return;
                }

                if (txtReason3.Text == string.Empty)
                {
                    txtReason3.Focus();
                    Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입니다."
                    return;
                }

                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                //계속 진행하시겠습니까? >> 작업을 진행하시겠습니까?
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1170"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (msgresult) =>
              Util.MessageConfirm("SFU1170", (msgresult) =>
              {

                    if (msgresult == MessageBoxResult.OK)
                    {
                        try
                        {
                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");

                            string sPalletID = string.Empty;

                            inDataTable.Columns.Add("SRCTYPE", typeof(string));
                            inDataTable.Columns.Add("REPACK_FLAG", typeof(string));
                            inDataTable.Columns.Add("BOXID", typeof(string));
                            inDataTable.Columns.Add("USERID", typeof(string));
                            inDataTable.Columns.Add("NOTE", typeof(string));
                          inDataTable.Columns.Add("LOTTERM_FLAG", typeof(string));

                            inSublotTable.Columns.Add("FROM_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("TO_SUBLOTID", typeof(string));
                            inSublotTable.Columns.Add("BCR2D", typeof(string));

                            DataRow dr = inDataTable.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["REPACK_FLAG"] = "N";
                            dr["BOXID"] = sPalletID = Util.NVC(DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID"));
                            dr["USERID"] = txtWorker.Tag as string; //LoginInfo.USERID;    // sUser;
                            dr["NOTE"] = txtReason3.Text.Trim();
                          dr["LOTTERM_FLAG"] = "N";
                            inDataTable.Rows.Add(dr);

                            string from_sublotid = string.Empty;
                            string to_sublotid = string.Empty;

                            for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                            {
                                from_sublotid = Util.NVC(DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID"));
                                to_sublotid = null; //Util.NVC(DataTableConverter.GetValue(dgSourceCell1.Rows[0].DataItem, "SUBLOTID"));

                                if (from_sublotid == string.Empty)
                                {
                                    Util.MessageValidation("SFU1495"); //"대상 Cell ID가 입력되지 않았습니다."
                                    return;
                                }

                                DataRow drsub = indataSet.Tables["INSUBLOT"].NewRow();
                                drsub["FROM_SUBLOTID"] = from_sublotid;
                                drsub["TO_SUBLOTID"] = to_sublotid;
                                drsub["BCR2D"] = string.Empty;

                                indataSet.Tables["INSUBLOT"].Rows.Add(drsub);
                            }

                            new ClientProxy().ExecuteServiceSync_Multi("BR_SET_REPLACE_SUBLOT_BX", "INDATA,INSUBLOT", string.Empty, indataSet);
                            
                            //"정상적으로 처리하였습니다." >>정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtPalletID3.Focus();
                                    txtPalletID3.SelectAll();
                                }
                            });

                            txtTagPallet3.Text = sPalletID; 
                            AllClear3();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sPalletID = string.Empty;
                    txtTagPallet3.Text = string.Empty;

                    sPalletID = txtPalletID3.Text.ToString().ToUpper().Trim();

                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                       // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    GetPalletInfo(dgPallet3, sPalletID);
                                                         
                    ClearDataGrid(dgSourceCell3);
                    txtSourceCellID3.Focus();
                    txtSourceCellID3.SelectAll();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private bool SetSourceCellID3(string sSourceCellId)
        {
            try
            {
                if (dgPallet3.GetRowCount() == 0)
                {
                    //대상 팔레트를 먼저 등록하세요.
                    Util.MessageValidation("SFU3147");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3147"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }                

                if (string.IsNullOrEmpty(sSourceCellId))
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // 2024-07-11 KYJ : 2D BCR 사용 기능 추가 [E20240601-001909]
                bool b2DBCD = (chk2DBCR3.IsChecked == true ? true : false);          

                if (b2DBCD)
                {
                    string sData = SelectCellID(sSourceCellId);

                    if (string.IsNullOrWhiteSpace(sData))
                    {
                        return false;
                    }

                    sSourceCellId = txtSourceCellID3.Text = sData;
                }

                DataTable SearchResult = GetCellInfo(sSourceCellId);               

                if (SearchResult == null || SearchResult.Rows.Count == 0)
                {
                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905");
                 //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "BOXID").ToString() != SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                {
                    //팔레트에 등록되지 않은 Cell 입니다.
                    Util.MessageValidation("SFU3176");
                 //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3176"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet3.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                {
                    //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                    Util.MessageValidation("SFU1502");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                //dgSourceCell1.ItemsSource = DataTableConverter.Convert(SearchResult);

                if (dgSourceCell3.GetRowCount() == 0)
                {
                    InitCellList(dgSourceCell3);
                }

                for (int i = 0; i < dgSourceCell3.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSourceCell3.Rows[i].DataItem, "SUBLOTID").ToString() == SearchResult.Rows[0]["SUBLOTID"].ToString())
                    {
                        //이미 입력된 Cell 입니다.
                        Util.MessageValidation("SFU3164");
                     //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3164"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                dgSourceCell3.IsReadOnly = false;
                dgSourceCell3.BeginNewRow();
                dgSourceCell3.EndNewRow(true);
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "SEQ", dgSourceCell3.GetRowCount());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "PLLT_BCD_ID", SearchResult.Rows[0]["PLLT_BCD_ID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "BOXSEQ", SearchResult.Rows[0]["BOXSEQ"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "TOP_PRODID", SearchResult.Rows[0]["TOP_PRODID"].ToString());
                DataTableConverter.SetValue(dgSourceCell3.CurrentRow.DataItem, "RTN_FLAG", SearchResult.Rows[0]["RTN_FLAG"].ToString());
                dgSourceCell3.IsReadOnly = true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                txtSourceCellID3.Focus();
                txtSourceCellID3.SelectAll();
            }
        }

        private void txtSourceCellID3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID3(txtSourceCellID3.Text.Trim());
            }
        }

        private void txtPalletID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {                    
                    string sPalletID = string.Empty;
                    txtTagPallet2.Text = string.Empty;
                    sPalletID = txtPalletID2.Text.ToString().ToUpper().Trim();
                    if (sPalletID == null)
                    {
                        //Pallet ID 가 없습니다. >> PALLETID를 입력해주세요
                        Util.MessageValidation("SFU1411");
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1411"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (GetPalletInfo(dgPallet2, sPalletID))
                    {
                        txtSourceCellID2.Focus();
                        txtSourceCellID2.SelectAll();
                    }
                    else
                    {
                        txtPalletID2.SelectAll();
                    }
                    
                    ////dgPallet2.ItemsSource = DataTableConverter.Convert(dtPallet);
                 //   Util.GridSetData(dgPallet2, dtPallet, FrameOperation);
                    ClearDataGrid(dgSourceCell2);

                   
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private bool SetSourceCellID2(string sSouceCellId)
        {
            try
            {
                if (dgPallet2.GetRowCount() == 0)
                {
                    //대상 팔레트를 먼저 등록하세요.
                    Util.MessageValidation("SFU3147");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3147"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (sSouceCellId == null)
                {
                    //교체 Cell ID 가 없습니다.
                    Util.MessageValidation("SFU1462");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1462"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                //if (GetCellHoldInfo(sSouceCellId) == false ) // Cell 추가 시, HOLD 상태 Validation.
                //{
                //    return false;
                //}

                // 2024-07-11 KYJ : 2D BCR 사용 기능 추가 [E20240601-001909]
                bool b2DBCD = (chk2DBCR2.IsChecked == true ? true : false);             

                if (b2DBCD)
                {
                    string sData = SelectCellID(sSouceCellId);

                    if (string.IsNullOrWhiteSpace(sData))
                    {
                        return false;
                    }

                    sSouceCellId = txtSourceCellID2.Text = sData;
                }

                DataTable SearchResult = GetCellInfo(sSouceCellId);

                if (SearchResult == null || SearchResult.Rows.Count == 0)
                {
                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "BOXID").ToString() == SearchResult.Rows[0]["OUTER_BOXID"].ToString())
                {
                    //동일 팔레트 존재하는 Cell은 추가 대상에 등록할 수 없습니다.
                    Util.MessageValidation("SFU3249");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3249"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                {
                    //동일 제품 끼리만 교체할 수 있습니다. >> 동일 제품이 아닙니다.
                    Util.MessageValidation("SFU1502");
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1502"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return false;
                }

                if (SearchResult.Rows[0]["BOXSTAT"].ToString() == "PACKING")
                {
                    // 다른 PALLET에 포장 진행 중인 CELL은 추가할 수 없습니다.
                    Util.MessageValidation("SFU3832");
                    return false;
                }

                if (GetCellChkArea() > 0) //공통코드에 등록된 동만 체크함
                {

                    // 해당 Cell이 자동인지 수동인지 체크
                    // 교체 Cell이 포장되어 있는지 체크                      

                    DataTable SearchSrcType = GetSrcType(sSouceCellId);

                    if (SearchSrcType.Rows.Count > 0)
                    {
                        SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = SearchSrcType.Rows[0]["PACK_WRK_TYPE_CODE"];
                    }
                    else
                    {
                        //이력정보가 없다는 뜻은 한번도 포장한 이력이 없다는 뜻임 : 그래서 강제적으로 UI로 입력함
                        SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"] = "UI";
                    }

                    if (DataTableConverter.GetValue(dgPallet2.CurrentRow.DataItem, "PACK_WRK_TYPE_CODE").ToString() == "EQ" && SearchResult.Rows[0]["PACK_WRK_TYPE_CODE"].ToString() != "EQ")
                    {
                        //대상이 Cell이 자동포장일 경우  교체Cell도 자동포장이어야 합니다.
                        Util.MessageValidation("SFU8476");
                        return false;
                    }
                }

                for (int i = 0; i < dgSourceCell2.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgSourceCell2.Rows[i].DataItem, "SUBLOTID").ToString() == SearchResult.Rows[0]["SUBLOTID"].ToString())
                    {
                        //이미 입력된 Cell 입니다.
                        Util.MessageValidation("SFU3164");
                        //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3164"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                if (SearchResult.Rows[0]["OUTER_BOXID"].ToString() == string.Empty)
                {

                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                        RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                        RQSTDT.Columns.Add("SHOPID", typeof(string));
                        RQSTDT.Columns.Add("AREAID", typeof(string));
                        RQSTDT.Columns.Add("EQSGID", typeof(string));
                        RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                        RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("BCR_SKIP_FLAG", typeof(string));
                        RQSTDT.Columns.Add("USERID", typeof(string));
                        //RQSTDT.Columns.Add("LOTTYPE", typeof(string));

                        DataRow dr = RQSTDT.NewRow();
                        dr["SUBLOTID"] = sSouceCellId;
                        dr["SHOPID"] = string.Empty;
                        dr["AREAID"] = string.Empty;
                        dr["EQSGID"] = string.Empty;
                        dr["MDLLOT_ID"] = string.Empty;
                        dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                        dr["INSP_SKIP_FLAG"] = chkInspectSkip2.IsChecked == true ? "Y" : "N";
                        dr["BCR_SKIP_FLAG"] = "Y";
                        dr["USERID"] = txtWorker.Tag.ToString();
                        // dr["LOTTYPE"] = palletLottype;

                        RQSTDT.Rows.Add(dr);

                        // ClientProxy2007
                        DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_CHK_FORM_DATA_VALIDATION_BX", "INDATA", "OUTDATA", indataSet);

                    }
                    catch (Exception ex)
                    {
                        // IR NG 발생 시,
                        if (ex.Data["CODE"].ToString() == "100000089")
                        {
                            SetSublotDefect(sSouceCellId);
                        }

                        Util.MessageException(ex);
                        return false;
                    }
                }

                if (dgSourceCell2.GetRowCount() == 0)
                {
                    InitCellList(dgSourceCell2);
                }


                if (bTrayAdminChk == true && palletSrctype == "EQ")
                {
                    if (trayChkQty < trayCellQty)
                    {
                        trayChkQty++;
                    }
                    else
                    {
                        Util.MessageValidation("SFU8210", cboTray.SelectedValue.ToString());
                        return false;
                    }
                }

                if (palletLottype != SearchResult.Rows[0]["LOTTYPE"].ToString()
                      && ((palletLottype == "X" || palletLottype == "L") || (SearchResult.Rows[0]["LOTTYPE"].ToString() =="X" || SearchResult.Rows[0]["LOTTYPE"].ToString() =="L")))
                {

                    DataTable dtP = DataTableConverter.Convert(dgPallet2.ItemsSource);
                    DataTable dtS = DataTableConverter.Convert(dgSourceCell2.ItemsSource);

                    string boxlist1 = string.Empty;
                    string boxlist2 = string.Empty;

                    for (int row = 0; row < dtP.Rows.Count; row++)
                    {
                        boxlist1 += "," + dtP.Rows[row]["BOXID"].ToString();
                    }
                    
                    if (string.IsNullOrWhiteSpace(boxlist1))
                    {
                        Util.AlertInfo("SFU1180"); //BOX 정보가 없습니다.
                        return false;
                    }
                    else
                    {
                        boxlist2 = boxlist1.Substring(1);
                    }
                    

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LANGID");
                    RQSTDT.Columns.Add("BOX_LIST");

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["BOX_LIST"] = boxlist2;
                    RQSTDT.Rows.Add(dr);

                    DataTable dsResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", RQSTDT); // 시생산 cell 조회
                    DataTable dnResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_NOT_PILOT_LIST_PALLET_BX", "INDATA", "OUTDATA", RQSTDT); // 시생산 아닌 cell 조회

                    for (int i = 0; i < dtS.Rows.Count; i++)
                    {
                        if (dtS.Rows[i]["LOTTYPE"].ToString() == "X" || dtS.Rows[i]["LOTTYPE"].ToString() == "L")
                        {
                            DataRow pi = dsResult.NewRow();
                            pi["SUBLOTID"] = dtS.Rows[i]["SUBLOTID"].ToString();
                            pi["LOTTYPE_NM"] = dtS.Rows[i]["LOTTYPE_NM"].ToString();
                            pi["BOXID"] = dtS.Rows[i]["OUTER_BOXID"].ToString();
                            pi["POSITION"] = dtS.Rows[i]["POSITION"].ToString();
                            dsResult.Rows.Add(pi);
                        }
                        else
                        {
                            DataRow piN = dnResult.NewRow();
                            piN["SUBLOTID"] = dtS.Rows[i]["SUBLOTID"].ToString();
                            piN["LOTTYPE_NM"] = dtS.Rows[i]["LOTTYPE_NM"].ToString();
                            piN["BOXID"] = dtS.Rows[i]["OUTER_BOXID"].ToString();
                            piN["POSITION"] = dtS.Rows[i]["POSITION"].ToString();
                            dnResult.Rows.Add(piN);
                        }
                    }

                    if (SearchResult.Rows[0]["LOTTYPE"].ToString() == "X" || SearchResult.Rows[0]["LOTTYPE"].ToString() == "L")
                    {
                        DataRow pi = dsResult.NewRow();
                        pi["SUBLOTID"] = SearchResult.Rows[0]["SUBLOTID"].ToString();
                        pi["LOTTYPE_NM"] = SearchResult.Rows[0]["LOTTYPE_NM"].ToString();
                        pi["BOXID"] = SearchResult.Rows[0]["OUTER_BOXID"].ToString();
                        pi["POSITION"] = SearchResult.Rows[0]["POSITION"].ToString();
                        dsResult.Rows.Add(pi);
                    }
                    else
                    {
                        DataRow piN = dnResult.NewRow();
                        piN["SUBLOTID"] = SearchResult.Rows[0]["SUBLOTID"].ToString();
                        piN["LOTTYPE_NM"] = SearchResult.Rows[0]["LOTTYPE_NM"].ToString();
                        piN["BOXID"] = SearchResult.Rows[0]["OUTER_BOXID"].ToString();
                        piN["POSITION"] = SearchResult.Rows[0]["POSITION"].ToString();
                        dnResult.Rows.Add(piN);
                    }

                    if (dsResult.Rows.Count > 0) // 시생산 cell 존재하는 경우
                    {
                        // 시생산 cell 있는 경우 popup
                        BOX001_PILOT_DETL popUp = new BOX001_PILOT_DETL();
                        popUp.FrameOperation = this.FrameOperation;

                        if (popUp != null)
                        {
                            object[] Parameters = new object[2];
                            Parameters[0] = dsResult;
                            Parameters[1] = dnResult;                                    

                            C1WindowExtension.SetParameters(popUp, Parameters);

                            popUp.Closed += new EventHandler(wndConfirm_Closed);

                            grdMain.Children.Add(popUp);
                            popUp.BringToFront();
                        }
                    }
                    return false;
                }
                

                dgSourceCell2.IsReadOnly = false;
                dgSourceCell2.BeginNewRow();
                dgSourceCell2.EndNewRow(true);
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "SEQ", dgSourceCell2.GetRowCount());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "SUBLOTID", SearchResult.Rows[0]["SUBLOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "OUTER_BOXID", SearchResult.Rows[0]["OUTER_BOXID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "PLLT_BCD_ID", SearchResult.Rows[0]["PLLT_BCD_ID"].ToString());

                if (bTrayAdminChk == true)
                {
                    DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "INNER_BOXID", cboTray.SelectedValue.ToString());
                }
                else
                {
                    DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "INNER_BOXID", SearchResult.Rows[0]["INNER_BOXID"].ToString());
                }

                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "BOXSEQ", SearchResult.Rows[0]["BOXSEQ"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "BOX_PSTN_NO", SearchResult.Rows[0]["BOX_PSTN_NO"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "LOTID", SearchResult.Rows[0]["LOTID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "RCV_ISS_ID", SearchResult.Rows[0]["RCV_ISS_ID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "TOP_PRODID", SearchResult.Rows[0]["TOP_PRODID"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "LOTTYPE", SearchResult.Rows[0]["LOTTYPE"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "LOTTYPE_NM", SearchResult.Rows[0]["LOTTYPE_NM"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "POSITION", SearchResult.Rows[0]["POSITION"].ToString());
                DataTableConverter.SetValue(dgSourceCell2.CurrentRow.DataItem, "RTN_FLAG", SearchResult.Rows[0]["RTN_FLAG"].ToString());

                dgSourceCell2.IsReadOnly = true;
                                
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                txtSourceCellID2.Focus();
                txtSourceCellID2.SelectAll();
            }
        }
        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_PILOT_DETL popup = sender as BOX001_PILOT_DETL;

            this.grdMain.Children.Remove(popup);
        }

        private void txtSourceCellID2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetSourceCellID2(txtSourceCellID2.Text.Trim());
            }
        }

        private void btnInit1_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear1(true);
                }

            });
        }

        private void btnDelete3_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
           Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell3.IsReadOnly = false;
                    dgSourceCell3.RemoveRow(index);

                    for (int cnt = 0; cnt < dgSourceCell3.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell3.Rows[cnt].DataItem, "SEQ", cnt+1);
                    }
                    dgSourceCell3.IsReadOnly = true;
                }
            });
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            //삭제하시겠습니까?
           // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
           Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgSourceCell2.IsReadOnly = false;
                    dgSourceCell2.RemoveRow(index);


                    for (int cnt = 0; cnt < dgSourceCell2.GetRowCount(); cnt++)
                    {
                        DataTableConverter.SetValue(dgSourceCell2.Rows[cnt].DataItem, "SEQ", cnt + 1);
                    }

                    dgSourceCell2.IsReadOnly = true;
                }
            });
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER wndPopup = new GMES.MES.CMM001.Popup.CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = Process.CELL_BOXING; // LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                grdMain.Children.Add(wndPopup);
                wndPopup.BringToFront();
            }
        }
        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            grdMain.Children.Remove(window);
        }

        private void txtSourceCellID2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID2(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtSourceCellID3_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && SetSourceCellID3(sPasteStrings[i].Trim()) == false)
                            break;

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void PrintTag(C1.WPF.DataGrid.C1DataGrid grid, string palletID)
        {
            try
            {
                int iSelRow = 0;

                //Pallet Tag 정보Set
                DataTable dtPalletHisCard = setPalletTag(grid, LoginInfo.USERID, palletID, iSelRow);
                                            //SetPalletTag(LoginInfo.USERID, palletID, iSelRow, dtBox, dtAssyLot);
                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; ; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = "2";
                Parameters[3] = "Y";
                Parameters[4] = sSHOPID;

                LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                C1WindowExtension.SetParameters(rs, Parameters);
                rs.Closed += new EventHandler(rs_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                //  this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                grdMain.Children.Add(rs);
                rs.BringToFront();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        private void rs_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.BOX001.Report_Pallet_Hist wndPopup = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
            grdMain.Children.Remove(wndPopup);
            //// 초기화 함수 호출
            //if (tabCtrl.SelectedItem == tabChange)
            //{
            //    _bChangeFlag = false;
            //    AllClear1();
            //}
            //else if (tabCtrl.SelectedItem == tabMapping)
            //{
            //    _bMappingFlag = false;
            //    AllClear2();
            //}
            //else if (tabCtrl.SelectedItem == tabUnMapping)
            //{
            //    _bUnMappingFlag = false;
            //    AllClear3();
            //}
        }
        private void btnTagPrint1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet1.Text))
                {
                    //Cell 교체 작업 완료 후 재발행이 가능합니다.
                    Util.MessageValidation("SFU3355");
                    return;
                }
                //C1.WPF.DataGrid.C1DataGrid grid = dgTargetCell1;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["OUTER_BOXID"].Index).Value);
                PrintTag(dgTargetCell1, txtTagPallet1.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTagPrint2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet2.Text))
                {
                    //Cell 추가 작업 완료 후 재발행이 가능합니다.
                     Util.MessageValidation("SFU3356");
                    return;
                }
                //C1.WPF.DataGrid.C1DataGrid grid = dgPallet2;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["BOXID"].Index).Value);
                PrintTag(dgPallet2, txtTagPallet2.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnTagPrint3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTagPallet3.Text))
                {
                    // Cell 취출 작업 완료 후 재발행이 가능합니다.
                    Util.MessageValidation("SFU3357");
                    return;
                }

                //C1.WPF.DataGrid.C1DataGrid grid = dgPallet3;
                //string palletID = Util.NVC(grid.GetCell(0, grid.Columns["BOXID"].Index).Value);
                PrintTag(dgPallet3, txtTagPallet3.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 조립LotID, 해당 Lot 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SearchAssyLot(string palletID)
        {

            //BizData data = new BizData("QR_GETASSYLOT_PALLETID", "RSLTDT");
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletID;
                //dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SHOPID"] = sSHOPID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_GET_ASSY_LOT_BY_PALLET_BX", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult == null || dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("LOTID", typeof(string));
                lsDataTable.Columns.Add("CELLQTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();
                    row["LOTID"] = Util.NVC(dtResult.Rows[i]["LOTID"].ToString());
                    row["CELLQTY"] = Util.NVC(dtResult.Rows[i]["CELLQTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }

        }

        /// <summary>
        /// BOXID, 해당 BOX 별 수량 조회
        /// </summary>
        /// <param name="palletID"></param>
        /// <returns></returns>
        public DataTable SelectTagInformation(string palletID, string sPackWrkType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TRAY_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    return null;
                }

                #region # Data Column 정의
                DataTable lsDataTable = new DataTable();
                lsDataTable.Columns.Add("TRAY_MAGAZINE", typeof(string));
                lsDataTable.Columns.Add("QTY", typeof(string));
                #endregion

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    DataRow row = lsDataTable.NewRow();

                    if (sPackWrkType == "MGZ")
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TAG_ID"].ToString());
                    }
                    else
                    {
                        row["TRAY_MAGAZINE"] = Util.NVC(dtResult.Rows[i]["TRAYID"].ToString());
                    }

                    row["QTY"] = Util.NVC(dtResult.Rows[i]["QTY"].ToString());
                    lsDataTable.Rows.Add(row);
                }

                return lsDataTable;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }
        }

        private DataTable SelectScanPalletInfo(string palletID)
        {
            DataTable dtResult = new DataTable();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = sAREAID;
                dr["PALLETID"] = palletID;
                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtResult;
            }
        }
        private DataTable setPalletTag(C1.WPF.DataGrid.C1DataGrid grid, string sUserName, string sPalletID, int iSelRow)
        {
            string sProjectName = string.Empty;

            DataTable dtinfo = SelectScanPalletInfo(sPalletID);
            DataTable dtAssyLot = new DataTable();

                 // Tray _ MagazineID / 수량 저장을 위한 DataTable
            DataTable dtBox = new DataTable();

            // Palelt ID
            string sPackWrkType = Util.NVC(dtinfo.Rows[iSelRow]["LOT_TYPE"]);

            // 수작업 여부 확인 : MAG 컬럼이 Y면 수작업임
            if (sPackWrkType == "UI")
            {
                // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                dtAssyLot = SearchAssyLot(sPalletID);
            }
            else
            {
                //  Tray / Magazine 정보 조회 함수 호출
                dtBox = SelectTagInformation(sPalletID, sPackWrkType);
                if (dtBox == null)
                {
                    return null;
                }
                else
                {
                    // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
                    dtAssyLot = SearchAssyLot(sPalletID);
                }
            }

            //고객 모델 조회
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drMomel = RQSTDT.NewRow();
                drMomel["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);
                RQSTDT.Rows.Add(drMomel);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR_FOR_PROJECTNAME", "RQSTDT", "RSLTDT", RQSTDT);
                sProjectName = Util.NVC(dtResult.Rows[0]["PROJECTNAME"]);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //return null;
            }


            //Lot 편차 구하기... 2014.02.20 Add By Airman
            string sLotTerm = GetLotTerm2PalletID(sPalletID);

            DataTable dtPalletHisCard = new DataTable();

            dtPalletHisCard.Columns.Add("PALLETID", typeof(string));    //4,3   PALLETID_01
            dtPalletHisCard.Columns.Add("BARCODE1", typeof(string));    //4,9   PALLETID_02
            dtPalletHisCard.Columns.Add("CONBINESEQ1", typeof(string));  //4,17  PALLETD_03

            dtPalletHisCard.Columns.Add("SHIP_LOC", typeof(string));    //5,7   출하처
            dtPalletHisCard.Columns.Add("SHIPDATE", typeof(string));    //5,14  출하예정일
            dtPalletHisCard.Columns.Add("OUTGO", typeof(string));       //6,7   출하지
            dtPalletHisCard.Columns.Add("LOTTERM", typeof(string));     //6,16  LOT편차
            dtPalletHisCard.Columns.Add("NOTE", typeof(string));        //7,7   특이사항
            dtPalletHisCard.Columns.Add("UNCODE", typeof(string));      //UNCODE

            dtPalletHisCard.Columns.Add("PACKDATE", typeof(string));    //8,7   포장작업일자
            dtPalletHisCard.Columns.Add("LINE", typeof(string));        //8,15  생산호기
            dtPalletHisCard.Columns.Add("MODEL", typeof(string));       //9,7   모델
            dtPalletHisCard.Columns.Add("PRODID", typeof(string));      //9,15  제품id
            dtPalletHisCard.Columns.Add("SHIPQTY", typeof(string));     //10,7   출하수량
            dtPalletHisCard.Columns.Add("PARTNO", typeof(string));      //10,15  PART NO
            dtPalletHisCard.Columns.Add("OUTQTY", typeof(string));      //11,7   제품수량
            dtPalletHisCard.Columns.Add("USERID", typeof(string));      //11,15  작업자
            dtPalletHisCard.Columns.Add("CONBINESEQ2", typeof(string)); //12,7   구성차수관리No
            dtPalletHisCard.Columns.Add("SKIPYN", typeof(string));      //12,15  검사조건Skip여부
                                                                        //dtTRAY
            dtPalletHisCard.Columns.Add("SHIP_LOC_EN", typeof(string));
            dtPalletHisCard.Columns.Add("LINE_EN", typeof(string));
            for (int i = 0; i < 40; i++)
            {
                dtPalletHisCard.Columns.Add("TRAY_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("T_" + i.ToString(), typeof(string));
            }
            //lot
            for (int i = 0; i < 20; i++)
            {
                dtPalletHisCard.Columns.Add("LOTID_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("L_" + i.ToString(), typeof(string));
                dtPalletHisCard.Columns.Add("BCD" + i.ToString(), typeof(string));
            }

            string sShipToID = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_ID"]);
            string sShipToName = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME"]);

            DataRow dr = dtPalletHisCard.NewRow();
            dr["PALLETID"] = sPalletID;
            dr["BARCODE1"] = sPalletID;
            dr["LOTTERM"] = sLotTerm;

            if (sShipToID == string.Empty)
            {
                dr["SHIP_LOC"] = "";
                dr["SHIPDATE"] = "";
                dr["OUTGO"] = "";
                dr["NOTE"] = "";
                dr["PACKDATE"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["WIPDTTM_ED"].Index).Value);
                dr["LINE"] = "";
                dr["MODEL"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                //dr["PRODID"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                dr["PRODID"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["TOP_PRODID"].Index).Value); // 출력 시 완제품 ID로 프린트 되도록 수정
                dr["SHIPQTY"] = "";
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(grid.GetCell(iSelRow, grid.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(grid.GetCell(iSelRow, grid.Columns["REG_USER"].Index).Value);
                dr["CONBINESEQ2"] = "";
                dr["CONBINESEQ1"] = "";
                dr["SKIPYN"] = "";
                dr["SHIP_LOC_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME_EN"]);    //TB_MMD_SHIPTO.SHIPTO_NAME
                dr["LINE_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME_EN"]);        //EQUIPMENTSEGMENT.EQSGNAME
                dr["UNCODE"] = Util.NVC(dtinfo.Rows[iSelRow]["UN_CODE"]);
            }
            else
            {
                dr["SHIP_LOC"] = sShipToName;
                dr["SHIPDATE"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPDATE_SCHEDULE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPDATE_SCHEDULE"].Index).Value);
                dr["OUTGO"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NOTE"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPTO_NOTE"].Index).Value);
                dr["NOTE"] = Util.NVC(dtinfo.Rows[iSelRow]["PACK_NOTE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["PACK_NOTE"].Index).Value);
                dr["PACKDATE"] = Util.NVC(dtinfo.Rows[iSelRow]["WIPDTTM_ED"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["WIPDTTM_ED"].Index).Value);
                dr["LINE"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["EQSGNAME"].Index).Value);

                //if (sShipToName == "HLGP")
                //{
                //    if (sProjectName == "" || sProjectName == "N/A")
                //    {
                //        dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                //    }
                //    else
                //    {
                //        dr["MODEL"] = sProjectName;
                //    }
                //}
                //else
                //{
                //    dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["MODELID"].Index).Value);
                //}
                dr["MODEL"] = Util.NVC(dtinfo.Rows[iSelRow]["MODELID"]) + " (" + sProjectName + ")";
                //dr["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
                dr["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["TOP_PRODID"]);   // 출력 시 완제품 ID로 프린트 되도록 수정
                dr["SHIPQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["SHIPQTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["SHIPQTY"].Index).Value));
                dr["PARTNO"] = "";
                dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["QTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["QTY"].Index).Value));
                dr["USERID"] = Util.NVC(dtinfo.Rows[iSelRow]["REG_USERNAME"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["REG_USERNAME"].Index).Value);
                dr["CONBINESEQ2"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
                dr["CONBINESEQ1"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
                dr["SKIPYN"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]) == "Y" ? "SKIP" : "NO SKIP";   // Util.NVC(grid.GetCell(iSelRow, grid.Columns["INSP_SKIP_FLAG"].Index).Value) == "Y" ? "SKIP" : "NO SKIP";

                dr["SHIP_LOC_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NAME_EN"]);
                dr["LINE_EN"] = Util.NVC(dtinfo.Rows[iSelRow]["EQSGNAME_EN"]);
                dr["UNCODE"] = Util.NVC(dtinfo.Rows[iSelRow]["UN_CODE"]);
            }

            for (int i = 0; i < dtBox.Rows.Count && i<40; i++)
            {
                dr["TRAY_" + i.ToString()] = Util.NVC(dtBox.Rows[i]["TRAY_MAGAZINE"]);
                dr["T_" + i.ToString()] = Util.NVC_Int(dtBox.Rows[i]["QTY"]);
            }

            dtPalletHisCard.Rows.Add(dr);

            for (int cnt = 0; cnt < (dtAssyLot.Rows.Count + 1) / 20; cnt++)
            {
                DataTable dtNew = dtPalletHisCard.Copy();
                dtPalletHisCard.Merge(dtNew);
            }

            for (int i = 0; i < dtAssyLot.Rows.Count; i++)
            {
                int cnt = i / 20;
                dtPalletHisCard.Rows[cnt]["LOTID_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtAssyLot.Rows[i]["LOTID"]);
                dtPalletHisCard.Rows[cnt]["L_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC_Int(dtAssyLot.Rows[i]["CELLQTY"]).ToString();
                dtPalletHisCard.Rows[cnt]["BCD" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtAssyLot.Rows[i]["LOTID"]) + " " + Util.NVC_Int(dtAssyLot.Rows[i]["CELLQTY"]).ToString();
            }

            //  dtPalletHisCard.Rows.Add(dr);
            return dtPalletHisCard;
        }

        /// <summary>
        /// LOT 편차 구하기
        /// </summary>
        /// <param name="sPalletID"></param>
        /// <returns></returns>
        private string GetLotTerm2PalletID(string sPalletID)
        {
            // DO_CONFIRM_CHECK
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("OUTER_BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["OUTER_BOXID"] = sPalletID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_OUTER_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC_Int(dtResult.Rows[0]["LOTTERM"]).ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return "0";
            }
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility;
        }

        private void btnPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            string sMsg = string.Empty;

            sMsg = "SFU2875";

            Util.MessageConfirm(sMsg, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (bPilotChk == false) bPilotChk = true;
                    else bPilotChk = false;
                    GetPilotProdMode();
                    //      this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                }
            });
        }
        private bool GetPilotProdMode()
        {
            try
            {
                bool bRet = false;

                if (bPilotChk == true)
                {
                    ShowPilotProdMode();
                    bRet = true;
                }
                else
                {
                    HidePilotProdMode();
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void ShowPilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Visible;
                //txtPilotProdMode.Text = ObjectDic.Instance.GetObjectName("PILOT_PRODUCTION");
                ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }
        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rect)
        {
            rect.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
                opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }
        private int GetPilotAdminArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "PILOT_PROD_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }

        private int GetTrayAdminArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "BOX_ADD_ADMIN_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }
        private bool GetPalletTotalQty_ChkYN()
        {
            bool bReturn = false;
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_PACK_PALLET_QTY_CHK";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;
  
            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);
            if(dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["CMCDIUSE"].ToString() == "Y")
            {
                bReturn = true;
            }
            else
            {
                bReturn = false;
            }
            return bReturn;
        }

        /// <summary>
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GetTrayID(string sPalletID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = sPalletID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BOX_PALLET_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //if (dtResult.Rows.Count == 0)
                //{
                //    cboTray.DisplayMemberPath = "CBO_NAME";
                //    cboTray.SelectedValuePath = "CBO_CODE";
                //    cboTray.ItemsSource = dtResult.Copy().AsDataView();
                //    cboTray.SelectedValue = "";
                //    return;
                //}

                cboTray.DisplayMemberPath = "CBO_NAME";
                cboTray.SelectedValuePath = "CBO_CODE";
                cboTray.ItemsSource = dtResult.Copy().AsDataView();
                cboTray.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void AddTrayCell()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "BOX_ADD_ADMIN_AREA";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    GetMMDTrayQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetMMDTrayQty()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = palletEqsgid;
                dr["PRODID"] = palletProdid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    trayCellQty = int.Parse(Util.NVC(dtResult.Rows[0]["TRAY_CELL_QTY"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetTrayQty(string trayID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = trayID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    trayChkQty = Decimal.ToInt32(Util.NVC_Decimal(dtResult.Rows[0]["TOTAL_QTY"]));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        private void cboTray_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(bTrayAdminChk == true)
            {
                GetTrayQty (cboTray.SelectedValue.ToString());
            }
        }
    
        private int GetCellChkArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "CELL_CHANGE_CHK_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }

        private void GetIRDefectCode()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "IR_NG_DEFECT_CODE";

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", inTable);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                sIRDefectCode = Util.NVC(dtRslt.Rows[0]["COM_CODE"]);
            }
        }

        private void SetSublotDefect(string sCellID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sIRDefectCode))
                {
                    // SFU1578 불량 항목이 없습니다.
                    Util.MessageValidation("SFU1578");
                    return;
                }

                DataSet dsInDataSet = new DataSet();

                DataTable dtINDATA = new DataTable();
                dtINDATA.TableName = "INDATA";
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("IFMODE", typeof(string));
                dtINDATA.Columns.Add("USERID", typeof(string));
                dtINDATA.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("REMARKS_CNTT", typeof(string));
                dtINDATA.Columns.Add("CALDATE", typeof(DateTime));
                dsInDataSet.Tables.Add(dtINDATA);

                DataRow drInData = dtINDATA.NewRow();
                drInData["SRCTYPE"] = "UI";
                drInData["IFMODE"] = "OFF";
                drInData["USERID"] = txtWorker.Tag.ToString();
                drInData["LOT_DETL_TYPE_CODE"] = 'N';
                drInData["REMARKS_CNTT"] = "IR NG - Packing";
                drInData["CALDATE"] = DateTime.Now;
                dtINDATA.Rows.Add(drInData);

                DataTable dtIN_SUBLOT = new DataTable();
                dtIN_SUBLOT.TableName = "IN_SUBLOT";
                dtIN_SUBLOT.Columns.Add("SUBLOTID", typeof(string));
                dtIN_SUBLOT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtIN_SUBLOT.Columns.Add("DFCT_CODE", typeof(string));
                dsInDataSet.Tables.Add(dtIN_SUBLOT);

                DataRow drInSublot = dtIN_SUBLOT.NewRow();
                drInSublot["SUBLOTID"] = sCellID;
                drInSublot["DFCT_GR_TYPE_CODE"] = "5";
                drInSublot["DFCT_CODE"] = sIRDefectCode;
                dtIN_SUBLOT.Rows.Add(drInSublot);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE", "INDATA,IN_SUBLOT", "OUTDATA", dsInDataSet);

                if (dsResult.Tables[0].Rows[0]["RETVAL"].ToString() != "0")
                {
                    // SFU1583 불량정보 저장 오류 발생
                    Util.MessageInfo("SFU1583");
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable GetSrcType(string sCellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SUBLOTID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SUBLOTID"] = sCellID;

            RQSTDT.Rows.Add(dr);

            return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SUBLOT_SRCTYPE", "INDATA", "OUTDATA", RQSTDT);
        }

        
    }
}
