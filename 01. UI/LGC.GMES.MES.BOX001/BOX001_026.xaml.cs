/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 포장 Pallet 구성 (BOX)
--------------------------------------------------------------------------------------
 [Change History]
    날  짜        이  름        CSR NO                    DESCRIPTION
  2016.06.16  DEVELOPER                            Initial Created.
  2019.11.07  이제섭                               CNB는 BOX 라벨 발행 시 Netweight, Gross Weight, Rated Power
                                                   항목 Key In 할 수 있도록 추가. (나머지 법인은 숨김처리)
  2019.11.12  이제섭                               체크박스 추가하여 Netweight, Gross Weight, Rated Power 사용/미사용 구분추가
                                                   체크박스 선택 시 항목 입력 Validation 
  2020.07.18  이제섭                               UNCODE 입력 기능 추가에 따라, Pallet Tag 디자인 분리되어 공통코드 조회하여 공통코드에 해당하는 동일 시 Tag 디자인 파일명 분리
  2022.01.03  김광오        C20211215-000009       [ESMI] 포장 Box 라벨 발행수 설정의 건
  2022.01.06  이상훈        C20211210-000008       용어 변경 (Cell -> celltobox ,pallet ->boxtopallet) LOT편차체크 기본 선택으로 변경
  2023.06.13  윤지해        E20230525-001352       GM향 라벨 발행 기능 추가

 
**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_026 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        private string _SaveBoxBizName = "BR_PRD_REG_PACKING_BOX_FOR_BOX";
        Util _Util = new Util();
        private string _LabelCode = string.Empty;
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        string _sPGM_ID = "BOX001_026";

        private bool bTop_ProdID_Use_Flag = false;

        public int BoxLabelCopy { get; set; } = 1;

        public BOX001_026()
        {
            InitializeComponent();

            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();

            #region [ C20211215-000009 ] 
            BoxLabelCopy = GetBoxLabelCopy();
            #endregion
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            GetBCRSkipVisible();
        }
        #endregion

        #region Initialize
        private void InitCombo()
        {
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            _combo.SetCombo(cboModelLot_Hist, CommonCombo.ComboStatus.ALL, sCase: "cboModelLot", sFilter: new string[] { null,null});

            string[] sFilter3 = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_EQSG_ID, LoginInfo.CFG_AREA_ID };
            _combo.SetCombo(cboComp, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "SHIPTO_CP");
            _combo.SetCombo(cboShipToID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            chk2D.IsChecked = true;
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
            // txtProdUser.Text = txtProdUser2.Text = LoginInfo.USERNAME;
            if (LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG")
            {
                chkTestMode.Visibility = Visibility.Visible;
                chkTestMode.IsChecked = true;
            }

            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo);

            // CNB는 라벨 정보 입력 TextBox Set
            if (LoginInfo.CFG_SHOP_ID == "G631" || LoginInfo.CFG_SHOP_ID == "G634") 
            {
                chkDetail.Visibility = Visibility.Visible;

                txtnetweightheader.Visibility = Visibility.Visible;
                txtnetweight.Visibility = Visibility.Visible;
                txtgrossweightheader.Visibility = Visibility.Visible;
                txtgrossweight.Visibility = Visibility.Visible;
                txtratedpowerheader.Visibility = Visibility.Visible;
                txtratedpower.Visibility = Visibility.Visible;

                txtnetweight.IsReadOnly = true;
                txtgrossweight.IsReadOnly = true;
                txtratedpower.IsReadOnly = true;
            }

            // 2023.06.13 2023.06.13 윤지해 E20230525-001352 GM향 출력 버튼 Visible 처리
            if(LoginInfo.CFG_SHOP_ID == "G451")
            {
                btnPrintGm.Visibility = Visibility.Visible;
            }
            else
            {
                btnPrintGm.Visibility = Visibility.Collapsed;
            }

        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        #region 날짜 변경시 이벤트
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion     

        #region 콤보 변경시 이벤트
        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            String[] sFilter = { GetAreaID(cboArea)};   
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter ,sCase: "LINE_CP");        
        }

        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            String[] sFilter = { GetAreaID(cboArea2) };
            _combo.SetCombo(cboEquipmentSegment2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINE_CP");
        }


        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);

            if (sEQSGID == string.Empty || sEQSGID == "SELECT")
                sEQSGID = null;

            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.SELECT, cbParent: cboParent);

            string[] sFilter3 = { GetShopID(cboArea), sEQSGID, GetAreaID(cboArea) };
            _combo.SetCombo(cboPackOut_Go, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
        }

        private void cboEquipmentSegment2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sEQSGID = Util.NVC(cboEquipmentSegment2.SelectedValue);

            if (sEQSGID == string.Empty || sEQSGID == "SELECT")
                sEQSGID = null;

            C1ComboBox[] cboParent = { cboEquipmentSegment2 };
            _combo.SetCombo(cboModelLot2, CommonCombo.ComboStatus.SELECT, sCase: "cboModelLot", cbParent: cboParent);

            string[] sFilter3 = { GetShopID(cboArea2), sEQSGID, GetAreaID(cboArea2) };
            _combo.SetCombo(cboPackOut_Go2, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
        }


        private void cboModelLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sModelLot = Util.NVC(cboModelLot.SelectedValue);
            if (sModelLot == "" || sModelLot == "SELECT")
            {
                sModelLot = "";
            }
            else
            {
                C1ComboBox[] cboParent2 = { cboEquipmentSegment, cboModelLot };
                String[] sFilter = { sModelLot };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, cbParent: cboParent2, sFilter: sFilter, sCase: "PROD_MDL");
            }
        }

        private void cboModelLot2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sModelLot = Util.NVC(cboModelLot2.SelectedValue);
            if (sModelLot == "" || sModelLot == "SELECT")
            {
                sModelLot = "";
            }
            else
            {
                C1ComboBox[] cboParent2 = { cboEquipmentSegment2, cboModelLot2 };
                String[] sFilter = { sModelLot };
                _combo.SetCombo(cboProduct2, CommonCombo.ComboStatus.NONE, cbParent: cboParent2, sFilter: sFilter, sCase: "PROD_MDL");
            }
        }

        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sProduct = Util.NVC(cboProduct.SelectedValue);
            if (sProduct == "" || sProduct == "SELECT")
            {
                txtProdID.Text = string.Empty;
                //txtProdName.Text = string.Empty;
            }
            else
            {
                txtProdID.Text = sProduct;
                //txtProdName.Text = cboProduct.Text;
            }

            //  [CSR ID:2642448]    2D Barcode를 이용한 출하구성 件 
            string sBCRCHK = "";
            chk2DBCR.IsChecked = false;

            if (sProduct == "" || sProduct == "SELECT")
            {
                sBCRCHK = Select2DBCRCHK();

                if (sBCRCHK.ToString() == "")
                {
                    return;
                }
                else if (sBCRCHK.ToString() == "N")
                {
                    chk2DBCR.IsEnabled = false;
                }
                else if (sBCRCHK.ToString() == "Y")
                {
                    chk2DBCR.IsEnabled = true;
                }
            }
        }

        private void cboProduct2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sProduct = Util.NVC(cboProduct2.SelectedValue);
            if (sProduct == "" || sProduct == "SELECT")
            {
                txtProdID2.Text = string.Empty;
                //txtProdName.Text = string.Empty;
            }
            else
            {
                txtProdID2.Text = sProduct;
                //txtProdName.Text = cboProduct.Text;
            }
        }
       
        #endregion

        #region Box 구성

        #region 버튼클릭시
        private void btnCellReg_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        bool bFlag = false;
                        System.Windows.Forms.Application.DoEvents();
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            //DataRow dataRow = dataTable.NewRow();
                            string sCellID = string.Empty;

                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                sCellID = Util.NVC(cell.Text);
                                txtCellID.Text = "";
                                txtCellID.Text = sCellID;

                                //Cell List 추가..
                                if(!(bFlag = AutoCreate())) return;
                                System.Windows.Forms.Application.DoEvents();
                            }
                        }
                       if(bFlag) Util.DingPlayer();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear_Cell(true);
                    cboPackOut_Go.SelectedItem = cboPackOut_Go.Items[0];
                    txtInputCell_Qty.Value = 0;
                    txtInputBox_Qty.Value = 0;
                }
            });        

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            CreateBox(false);
        }
        #endregion

        private bool AutoCreate()
        {
            try
            { // Cell 스캔
                if (!ScanCellID()) return false;

                // Auto Save
                if (int.Parse(txtScanCell_Qty.Text.Trim()) == int.Parse(txtInputCell_Qty.Value.ToString()))
                {
                    // 박스 생성
                    CreateBox(!(txtInputBox_Qty.Value == 0 || txtInputBox_Qty.Value == double.NaN));
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!AutoCreate())
                {
                    Util.WarningPlayer();
                }
                else
                    Util.DingPlayer();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            int iCol = dgCell_Info.Columns["SUBLOTID"].Index;
            string sCellID =Util.NVC(dgCell_Info.GetCell(iRow, iCol).Value);
            
            object[] parameters = new object[1];
            parameters[0] = sCellID;

            //SFU3274 [%1] CellID를 List에서 삭제하시겠습니까 ?
            Util.MessageConfirm("SFU3274", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 선택된 행 삭제     
                    DataTable dt = DataTableConverter.Convert(dgCell_Info.ItemsSource);
                    dt.Rows[iRow].Delete();

                    Util.GridSetData(dgCell_Info, dt, FrameOperation, true);
                
                    //편차 조회
                    SetLotTerm(string.Empty);

                    // 스캔수량 재설정
                    SetCellSeq();

                }
            }, parameters);
        }

        #endregion

        #region Pallet 구성

        private void txtBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!ScanBoxID())
                {
                    Util.WarningPlayer();
                }
                else
                    Util.DingPlayer();
            }
        }

        private void btnDelete2_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;
            int iCol = dgScanBox_Info.Columns["BOXID"].Index;
            string sBoxID = Util.NVC(dgScanBox_Info.GetCell(iRow, iCol).Value);

            object[] parameters = new object[1];
            parameters[0] = sBoxID;

            //[%1] BoxID를 List에서 삭제하시겠습니까 ?
            Util.MessageConfirm("SFU3260", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    // 선택된 행 삭제     
                    DataTable dt = DataTableConverter.Convert(dgScanBox_Info.ItemsSource);
                    dt.Rows[iRow].Delete();

                    Util.GridSetData(dgScanBox_Info, dt, FrameOperation, true);

                    DataTable dtPallet = DataTableConverter.Convert(dgPallet_Info.ItemsSource);
                    int rCnt = dtPallet.Select("BOXID <> '" + sBoxID + "'").ToList().Count();
                    dtPallet = rCnt>0? dtPallet.Select("BOXID <> '" + sBoxID + "'").CopyToDataTable() : new DataTable();

                    Util.GridSetData(dgPallet_Info, dtPallet, FrameOperation, true);

                    txtScanBoxQty.Text = dgScanBox_Info.Rows.Count().ToString();
                    lblCellQty.Text = rCnt.ToString();

                    string minLot;
                    string maxLot;
                //편차 조회
                SetLotTermByBox(string.Empty);
                }
            }, parameters);
        }

        private void btnRefresh2_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear_Box(true);
                    cboPackOut_Go2.SelectedItem = cboPackOut_Go2.Items[0];
                }
            });            
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            PopUpConfirm();
        }

        #endregion

        #region 실적 상세 조회

        private void txtBoxID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                SearchData();
        }

        private void txtCellID_Hist_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchData();
        }

        private void btnHist_Del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drInfo = Util.gridGetChecked(ref dgPalletInfo, "CHK");

                if (drInfo.Count() <= 0)
                {
                    Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                    return;
                }
                
                else if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                else if (!string.IsNullOrWhiteSpace(Util.NVC(drInfo[0]["OUTER_BOXID"])))
                {
                    //팔레트 구성된 박스는 해체할 수 없습니다. 
                    //팔레트 해체를 먼저 진행해주세요.
                    Util.MessageValidation("SFU3261"); 
                    return;
                }

                //선택한 Box를 해체하시겠습니까?
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2064"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
              Util.MessageConfirm("SFU2064", (result) =>
              {
                    try
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                            RQSTDT.Columns.Add("BOXID", typeof(string));
                            RQSTDT.Columns.Add("USERID", typeof(string));
                            RQSTDT.Columns.Add("NOTE", typeof(string));


                            DataRow dr = RQSTDT.NewRow();
                            dr["SRCTYPE"] = "UI";
                            dr["BOXID"] = Util.NVC(drInfo[0]["BOXID"].ToString());
                            dr["USERID"] = txtWorker.Tag;
                            RQSTDT.Rows.Add(dr);

                            DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_UNPACK_FOR_BOX", "INDATA", null, RQSTDT);

                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");

                            SearchData();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
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

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            //DataTable dtChk = DataTableConverter.Convert(dg.ItemsSource);
            try
            {
                DataRow[] drInfo = Util.gridGetChecked(ref dgPalletInfo, "CHK");

                if (drInfo == null)
                {
                    //조회 된 데이터가 없습니다.
                    Util.MessageInfo("SFU1905");
                    return;
                }

                if (drInfo.Count() <= 0)
                {
                    Util.MessageValidation("10008");
                    return;
                }
                // CNB 아니면 라벨 발행 BIZ 호출
                if (LoginInfo.CFG_SHOP_ID != "G631" && LoginInfo.CFG_SHOP_ID != "G634") 
                {
                    RePrint_Box(drInfo[0]["BOXID"].ToString());
                }
                // CNB는 팝업 호출
                else
                {

                    BOX001_026_REPRINT popup = new BOX001_026_REPRINT();
                    popup.FrameOperation = this.FrameOperation;

                    if (popup != null)
                    {
                        object[] Parameters = new object[2];
                        Parameters[0] = txtWorker.Tag; // 작업자 ID
                        Parameters[1] = Util.NVC(drInfo[0]["BOXID"].ToString()); //BOXID

                        C1WindowExtension.SetParameters(popup, Parameters);

                        popup.Closed += new EventHandler(puBoxReprint_Closed);

                        grdMain.Children.Add(popup);
                        popup.BringToFront();
                    }
                }
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

        private void puBoxReprint_Closed(object sender, EventArgs e)
        {
            BOX001_026_REPRINT popup = sender as BOX001_026_REPRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchData();
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

        private void dgPalletInfo_Choice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked)
                {
                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;


                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;
                    
                    string boxid = dtRow["BOXID"].ToString();
                    DataTable dt = SearchCellInfo(boxid);

                    Util.GridSetData(dgPallet_Hist, dt, FrameOperation, true);

                    DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[idx].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }

        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drInfo = Util.gridGetChecked(ref dgPalletInfo, "CHK");

                if (drInfo.Count() <= 0)
                {
                    Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                    return;
                }

                if (Util.NVC(cboShipToID.SelectedValue) == "" || Util.NVC(cboShipToID.SelectedValue) == "SELECT")
                {
                    // 출하처를 선택해주세요. >> 출하처 아이디를 선택해 주세요.
                    Util.MessageValidation("MMD0062");
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("TO_SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["TO_SHIPTO_ID"] = cboShipToID.SelectedValue;
                inData["USERID"] = txtWorker.Tag;
                inDataTable.Rows.Add(inData);

                DataRow inDataBox = inBoxTable.NewRow();
                inDataBox["BOXID"] = Util.NVC(drInfo[0]["BOXID"].ToString());
                inBoxTable.Rows.Add(inDataBox);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHANGE_SHIPTO_CP", "INDATA,INBOX", null, indataSet);
                Util.MessageInfo("SFU1935"); //"출하처가 변경되었습니다."

                SearchData();

                if(cboShipToID.Items.Count > 0)
                    cboShipToID.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }          
        }

        // 2023.06.13 2023.06.13 윤지해 E20230525-001352 GM향 라벨 발행 버튼 추가
        private void btnPrintGm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drInfo = Util.gridGetChecked(ref dgPalletInfo, "CHK");

                if (drInfo == null)
                {
                    // 조회 된 Data가 없습니다.
                    Util.MessageInfo("SFU1905");
                    return;
                }

                if (drInfo.Count() <= 0)
                {
                    // 선택된 데이터가 없습니다.
                    Util.MessageValidation("10008");
                    return;
                }

                BOX001_026_PRINT_GM popup = new BOX001_026_PRINT_GM();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = txtWorker.Tag; // 작업자
                    Parameters[1] = Util.NVC(drInfo[0]["BOXID"].ToString()); // BOX ID
                    Parameters[2] = Util.NVC_Int(drInfo[0]["TOTAL_QTY"].ToString()); // 제품수량
                    Parameters[3] = Util.NVC(drInfo[0]["PACKDTTM"].ToString()); // 작업일

                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.Closed += new EventHandler(puBoxReprint_Closed);

                    grdMain.Children.Add(popup);
                    popup.BringToFront();
                }
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

        #endregion

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
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
                // txtProdUser.Text = window.USERNAME;
                //  txtProdUser2.Text = window.USERNAME;
            }
            grdMain.Children.Remove(window);
        }

        private void btnPalletHist_Del_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drInfo = Util.gridGetChecked(ref dgPalletInfo, "CHK");

                if (drInfo.Count() <= 0)
                {
                    Util.MessageValidation("10008");//선택된 데이터가 없습니다.
                    return;
                }

                else if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                else if (string.IsNullOrWhiteSpace(Util.NVC(drInfo[0]["OUTER_BOXID"])))
                {
                    Util.MessageValidation("SFU3268"); //팔레트 구성된 박스가 아닙니다.
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataTable inDataBoxTable = indataSet.Tables.Add("INBOX");
                inDataBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["AREAID"] = GetAreaID(cboArea);
                inData["USERID"] = txtWorker.Tag;
                //   inData["NOTE"] = GetAreaID(cboArea);
                inDataTable.Rows.Add(inData);

                DataRow inDataBox = inDataBoxTable.NewRow();
                inDataBox["BOXID"] = Util.NVC(drInfo[0]["OUTER_BOXID"].ToString());
                inDataBoxTable.Rows.Add(inDataBox);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_PALLET_EXCEPT_BOX", "INDATA,INBOX", null, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                        return;
                    }
                    else
                    {
                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        SearchData();
                    }
                }, indataSet);
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

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_026_CONFIRM wndPopup = sender as BOX001_026_CONFIRM;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                // 초기화 함수 호출
                if (tabCtrl.SelectedItem == tabBox)
                    Clear_Cell(true);
                else if (tabCtrl.SelectedItem == tabPallet)
                    Clear_Box();

                DataTable dtReturn = wndPopup.retDT;
                string palletID = Util.NVC(dtReturn.Rows[0]["BOXID"]);

                #region 완제품 ID 존재 여부 체크
                // TOP_PRODID 조회.
                ChkUseTopProdID();

                string sTopProdID = "";

                if (bTop_ProdID_Use_Flag)
                {
                    sTopProdID = GetTopProdID(palletID);

                    if (sTopProdID.Equals(""))
                    {
                        // [%1]에 완제품 정보(TOP_PRODID)가 없습니다.
                        Util.MessageValidation("SFU5208", palletID);
                        grdMain.Children.Remove(wndPopup);
                        return;
                    }
                }
                #endregion

                DataTable dtPalletHisCard = setPalletTag(LoginInfo.USERID, palletID, 0, sTopProdID);

                // 태그 발행 창 화면에 띄움.
                object[] Parameters = new object[5];
                Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                Parameters[1] = dtPalletHisCard;
                Parameters[2] = "2";
                Parameters[4] = GetShopID(cboArea);

                LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                C1WindowExtension.SetParameters(rs, Parameters);
                this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void cboPackOut_Go_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sCode = cboPackOut_Go.SelectedValue.ToString();
            string sCodeName = cboPackOut_Go.Text;

            if (!sCode.Equals("") && !sCode.Equals("SELECT"))
            {
                try
                {
                    // BizData data = new BizData("R_PACK_OUTGO_BYOUTCOMP", "RSLTDT");
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SHOPID"] = GetShopID(cboArea);
                    dr["SHIPTO_ID"] = sCode;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_PACK_OUTGO_BYOUTCOMP_CP", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        lblPackOut_Go_LotTerm.Text = Util.NVC(dtResult.Rows[0]["PROD_DEVL_DATE"]); //+ "일 미만";
                    }

                    int iGM_Col = sCodeName.IndexOf("GM");
                    int iGM_Etc = sCodeName.IndexOf("(기타)");

                    if (iGM_Col >= 0 && iGM_Etc < 0)   //출고지가 GM이고 GM(기타)가 아닌 경우 무조건 양산품으로 선택되도록 함.
                    {
                        chkLot_Term.IsEnabled = false;
                        chkLot_Term.IsChecked = true;
                    }
                    else
                    {
                        chkLot_Term.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }
            else
            {
                lblPackOut_Go_LotTerm.Text = "";
            }
        }

        private void cboPackOut_Go2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sCode = cboPackOut_Go2.SelectedValue.ToString();
            string sCodeName = cboPackOut_Go2.Text;

            if (!sCode.Equals("") && !sCode.Equals("SELECT"))
            {
                try
                {
                    // BizData data = new BizData("R_PACK_OUTGO_BYOUTCOMP", "RSLTDT");
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SHOPID"] = GetShopID(cboArea2);
                    dr["SHIPTO_ID"] = sCode;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_PACK_OUTGO_BYOUTCOMP_CP", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        lblPackOut_Go_LotTerm2.Text = Util.NVC(dtResult.Rows[0]["PROD_DEVL_DATE"]); //+ "일 미만";
                    }

                    int iGM_Col = sCodeName.IndexOf("GM");
                    int iGM_Etc = sCodeName.IndexOf("(기타)");

                    if (iGM_Col >= 0 && iGM_Etc < 0)   //출고지가 GM이고 GM(기타)가 아닌 경우 무조건 양산품으로 선택되도록 함.
                    {
                        //optProd_Type1.IsEnabled = false;
                        //optProd_Type2.IsEnabled = false;

                        //optProd_Type1.IsChecked = true;

                        chkLot_Term2.IsEnabled = false;
                        chkLot_Term2.IsChecked = true;
                    }
                    else
                    {
                        chkLot_Term2.IsEnabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }
            else
            {
                lblPackOut_Go_LotTerm2.Text = "";
            }
        }
        #endregion

        #region Method

        private void SearchData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("CELLID", typeof(string));
                RQSTDT.Columns.Add("DATE_FROM", typeof(string));
                RQSTDT.Columns.Add("DATE_TO", typeof(string));
                RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));

                try
                {
                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    if (!string.IsNullOrWhiteSpace(txtBoxID_Hist.Text))
                        dr["BOXID"] = txtBoxID_Hist.Text;
                    else if (!string.IsNullOrWhiteSpace(txtCellID_Hist.Text))
                    {
                        dr["CELLID"] = txtCellID_Hist.Text;
                    }
                    else
                    {
                        dr["DATE_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                        dr["DATE_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                        if (!string.IsNullOrWhiteSpace(cboModelLot_Hist.SelectedValue.ToString())) dr["MDLLOT_ID"] = cboModelLot_Hist.SelectedValue;
                        if (!string.IsNullOrWhiteSpace(cboComp.SelectedValue.ToString())) dr["SHIPTO_ID"] = cboComp.SelectedValue;
                    }
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKING_BOX_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, true);
                    Util.gridClear(dgPallet_Hist);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                }

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

        private bool SetLotTerm(string templot)
        {
            try
            {
                string sTmp = templot;
                string sMin = string.IsNullOrWhiteSpace(lblMin.Text)? templot: lblMin.Text;
                string sMax = string.IsNullOrWhiteSpace(lblMax.Text) ? templot : lblMax.Text;

                string sMinDate = GetLotCreatDateByLot(sMin);
                string sMaxDate = GetLotCreatDateByLot(sMax);
                string sTmpDate = GetLotCreatDateByLot(sTmp);

                int iMin = string.Compare(sMinDate, sTmpDate);
                int iMax = string.Compare(sTmpDate, sMaxDate);

                if (iMin > 0)
                {
                    sMin = sTmp;
                    sMinDate = sTmpDate;
                }

                if (iMax > 0)
                {
                    sMax = sTmp;
                    sMaxDate = sTmpDate;
                }              

                if (string.IsNullOrWhiteSpace(templot))
                { 
                    for (int i = 0; i < dgCell_Info.GetRowCount(); i++)
                    {
                        string sLotID = Util.NVC(dgCell_Info.GetCell(i, dgCell_Info.Columns["LOTID"].Index).Value);

                        //LOT 편차를 구하기 위한 CELLID 의 생성일자 구하기
                        sTmpDate = GetLotCreatDateByLot(sLotID);
                        if (i == 0)
                        {
                            sMin = sMax = sLotID;
                            sMinDate = sMaxDate = sTmpDate;
                        }
                        else if (i > 0)
                        {
                            //int iMin = string.Compare(sMin, sLotID);
                            //int iMax = string.Compare(sLotID, sMax);

                            //sMin = (iMin > 0 ? sLotID : sMin);
                            //sMax = (iMax > 0 ? sLotID : sMax);

                             iMin = string.Compare(sMinDate, sTmpDate);
                             iMax = string.Compare(sTmpDate, sMaxDate);

                            sMin = (iMin > 0 ? sLotID : sMin);
                            sMax = (iMax > 0 ? sLotID : sMax);

                            sMinDate = (iMin > 0 ? sTmpDate : sMinDate);
                            sMaxDate = (iMax > 0 ? sTmpDate : sMaxDate);

                            if (iMin > 0 || iMax > 0)
                            {
                                if (!SearchLotTerm(sMin, sMax)) return false;
                            }
                        }
                    }
                }
                if (!SearchLotTerm(sMin, sMax)) return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return true;
        }

        private bool SetLotTermByBox(string boxID)
        {
            string minLot = string.Empty;
            string maxLot = string.Empty;
            string lotTerm = string.Empty;

            try
            {
                string sBoxID_List = boxID;

                for (int i = 0; i < dgScanBox_Info.GetRowCount(); i++)
                {
                    string sBoxID = Util.NVC(dgScanBox_Info.GetCell(i, dgScanBox_Info.Columns["BOXID"].Index).Value);
                    sBoxID_List += "," + sBoxID;
                }

                if (string.IsNullOrWhiteSpace(boxID)) sBoxID_List = sBoxID_List.Substring(1);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID_LIST"] = sBoxID_List;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_BOX_CP", "RQSTDT", "RSLTDT", RQSTDT);

                DataView dv = new DataView(dtResult);
                dv.Sort = "MIN_CALDATE";

                DataTable dtInfo = dv.ToTable();                 

                minLot = Util.NVC(dtInfo.Rows[0]["MIN_LOTID"]);
                string sMinDate = Util.NVC(dtInfo.Rows[0]["MIN_CALDATE"]);

                dv.Sort = "MAX_CALDATE DESC";
                dtInfo = dv.ToTable();

                maxLot = Util.NVC(dtInfo.Rows[0]["MAX_LOTID"]);
                string sMaxDate = Util.NVC(dtInfo.Rows[0]["MAX_CALDATE"]);

                return SearchLotTerm(minLot, maxLot,true);          
              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void PopUpConfirm()
        {
            try
            {
                string boxList = string.Empty;
                string boxQty = string.Empty;
                string cellQty = string.Empty;
                string model = string.Empty;
                string prodid = string.Empty;
                string skipFlag = string.Empty;
                string lineId = string.Empty;
                string lineName = string.Empty;
                string packout = string.Empty;
                string areaID = string.Empty;
                string shopID = string.Empty;

                if (tabCtrl.SelectedItem == tabBox)
                {
                    DataTable dt = DataTableConverter.Convert(dgBox_Info.ItemsSource);

                    string boxlist = string.Empty;
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        boxlist += "," + dt.Rows[row]["BOXID"].ToString();
                    }
                    if (string.IsNullOrWhiteSpace(boxlist))
                    {
                        Util.AlertInfo("SFU1180"); //BOX 정보가 없습니다.
                        return;
                    }
                    else
                    {
                        boxList = boxlist.Substring(1);
                    }

                    int sum = Util.NVC_Int(dt.Compute("Sum(TOTAL_QTY)","BOXID <> ''"));
                    boxQty = txtScanBox_Qty.Text;
                    cellQty = sum.ToString();
                    model = Util.NVC(cboModelLot.SelectedValue); 
                    prodid = txtProdID.Text.Trim();
                    lineId = Util.NVC(cboEquipmentSegment.SelectedValue);
                    lineName = Util.NVC(cboEquipmentSegment.Text);
                    packout = Util.NVC(cboPackOut_Go.SelectedValue);
                    areaID = GetAreaID(cboArea);
                    shopID = GetShopID(cboArea);
                }

                else if (tabCtrl.SelectedItem == tabPallet)
                {
                    DataTable dt = DataTableConverter.Convert(dgScanBox_Info.ItemsSource);

                    string boxlist = string.Empty;
                    for (int row = 0; row < dt.Rows.Count; row++)
                    {
                        boxlist += "," + dt.Rows[row]["BOXID"].ToString();
                    }
                    if (string.IsNullOrWhiteSpace(boxlist))
                    {
                        Util.AlertInfo("SFU1180"); //BOX 정보가 없습니다.
                        return;
                    }
                    else
                    {
                        boxList = boxlist.Substring(1);
                    }

                    boxQty = txtScanBoxQty.Text;
                    cellQty = lblCellQty.Text;
                    model = Util.NVC(cboModelLot2.SelectedValue);
                    prodid = txtProdID2.Text.Trim();
                    lineId = Util.NVC(cboEquipmentSegment2.SelectedValue);
                    lineName = cboEquipmentSegment2.Text.Replace(lineId, string.Empty).Replace(":", string.Empty).Trim();
                    packout = Util.NVC(cboPackOut_Go2.SelectedValue);
                    areaID = GetAreaID(cboArea2);
                    shopID = GetShopID(cboArea2);
                }
                else
                    return;

          
                DataTable dtBoxInfo = SearchBoxInfo(boxList);
                skipFlag = dtBoxInfo.Select("INSP_SKIP_FLAG = 'Y'").Count() > 0 ? "Y" : "N";
                // 폼 화면에 보여주는 메서드에 5개의 매개변수 전달
                BOX001_026_CONFIRM popUp = new BOX001_026_CONFIRM();
                popUp.FrameOperation = this.FrameOperation;

                if (popUp != null)
                {
                    object[] Parameters = new object[13];
                    Parameters[0] = boxList;
                    Parameters[1] = boxQty;                                     //박스수량
                    Parameters[2] = cellQty;                                    //셀수량
                    Parameters[3] = model;                                  //모델
                    Parameters[4] = prodid;                                 //제품
                    Parameters[5] = skipFlag;                                   //검사여부
                    Parameters[6] = lineId;
                    Parameters[7] = lineName;
                    Parameters[8] = packout;
                    Parameters[9] = areaID;
                    Parameters[10] = shopID;
                    Parameters[11] = txtWorker.Text;//cboProcUser.Text;
                    Parameters[12] = txtWorker.Tag as string;//cboProcUser.Text;

                    C1WindowExtension.SetParameters(popUp, Parameters);

                    popUp.Closed += new EventHandler(wndConfirm_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));

                    grdMain.Children.Add(popUp);
                    popUp.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }  
        }

      

        private void Clear_Cell(bool All = false)
        {
            if (All)
            {               
                Util.gridClear(dgBox_Info);
                txtScanBox_Qty.Text = "0";
            }

            // 그리드
            Util.gridClear(dgCell_Info);

            // 스캔 카운트 초기화
            txtScanCell_Qty.Text = "0";
            
            // Lot편차 초기화
            lblMin.Text = string.Empty;
            lblMax.Text = string.Empty;
            lblLotTerm.Text = string.Empty;

            //Cell 초기화
            txtCellID.Text = string.Empty;
            txtCellID.Focus();
            txtCellID.SelectAll();
        }

        private void Clear_Box(bool All = false)
        {
            if (All)
            {
                txtInputBoxQty.Value = 0;
            }

            // 그리드
            Util.gridClear(dgScanBox_Info);
            Util.gridClear(dgPallet_Info);
         
            // 스캔 카운트 초기화
            txtScanBoxQty.Text = "0";

            // Lot편차 초기화
            lblMin2.Text = string.Empty;
            lblMax2.Text = string.Empty;
            lblLotTerm2.Text = string.Empty;

            //Box 초기화
            txtBoxID.Text = string.Empty;
            txtBoxID.Focus();
            txtBoxID.SelectAll();
        }

        private void SetCellSeq()
        {
            DataTable dtCellInfo = DataTableConverter.Convert(dgCell_Info.ItemsSource);
            foreach (DataRow dr in dtCellInfo.Rows)
            {
                dr["SEQ"] = dtCellInfo.Rows.IndexOf(dr) + 1;
            }
            Util.GridSetData(dgCell_Info, dtCellInfo, FrameOperation, true);

            //스캔수량 계산
            txtScanCell_Qty.Text = dgCell_Info.Rows.Count.ToString();
        }

        /// <summary>
        /// Cell ID 스캔시 검증후 Cell List에 추가
        /// </summary>
        private bool ScanCellID()
        {
            if (!string.IsNullOrEmpty(txtCellID.Text))
            {
                // 기본적인 Validation
                if (string.IsNullOrWhiteSpace(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return false;
                }

                string sArea = Util.NVC(cboArea.SelectedValue);
                if (sArea == string.Empty || sArea == "SELECT")
                {
                    //동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
                if (sLine == string.Empty || sLine == "SELECT")
                {
                    //"라인을 선택해 주세요.
                    Util.MessageValidation("SFU1223");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sModel = Util.NVC(cboModelLot.SelectedValue);
                if (sModel == string.Empty || sModel == "SELECT")
                {
                    //모델을 선택해주십시오.
                    Util.MessageValidation("SFU1257");
                  //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sProduct = Util.NVC(cboProduct.SelectedValue);
                if (sProduct == string.Empty || sProduct == "SELECT")
                {
                    //제품을 선택해주십시오.
                    Util.MessageValidation("SFU1895");
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1895"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                if (cboPackOut_Go.SelectedValue.ToString().Equals("") || cboPackOut_Go.SelectedValue.Equals("SELECT"))
                {
                    //출하처를 선택하십시오.
                    Util.MessageValidation("SFU3173");
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3173"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                if (string.IsNullOrEmpty(txtInputCell_Qty.Value.ToString()) || txtInputCell_Qty.Value.ToString() == "0" || txtInputCell_Qty.Value.ToString() == "NaN")
                {
                    //스캔 설정치를 먼저 입력해주세요.
                    Util.MessageValidation("SFU3154", (result) =>
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3154"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        txtInputCell_Qty.Focus();
                       
                    });
                    return false;
                }

                // 현재 스캔된 수량이 설정치보다 클 경우
                if (Util.NVC_Int(txtScanCell_Qty.Text) >= Util.NVC_Int(txtInputCell_Qty.Value))
                {
                    //설정치를 초과하였습니다. 설정치를 변경하시거나 스캔을 중지해주세요.
                    Util.MessageValidation("SFU3152", (result) =>
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3152"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        txtInputCell_Qty.Focus();

                    });
                    return false;
                }

                // 2D Barcode를 이용한 출하구성 件 : 2D 값으로 CELL ID 정보 조회

                if (chk2DBCR.IsChecked == true)
                {
                    if (txtCellID.Text.Trim().Length > 60)
                    {
                        //string sCellID = string.Empty; 
                        string sData = SelectCellID(txtCellID.Text.Trim());

                        if (string.IsNullOrWhiteSpace(sData))
                        {
                            //입력한 [" + sBCR + "] 2D 바코드의 CELL ID 정보가 없습니다.
                            //입력한 [" + sBCR + "] 2D 바코드의 CELL ID 정보가 없습니다.
                            // msg = "SFU3248" + Convert.ToString((char)13) + Convert.ToString((char)10) + ex.Message;
                            object[] parameters = new object[1];
                            parameters[0] = txtCellID.Text.Trim();

                            Util.MessageConfirm("SFU3248", result =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellID.Focus();
                                    txtCellID.SelectAll();
                                }
                            }, parameters);



                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sCellID), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            //{
                            //    if (result == MessageBoxResult.OK)
                            //    {
                            //        txtCellID.Focus();
                            //        txtCellID.SelectAll();
                            //    }
                            //});
                            return false;
                        }
                        else
                        {
                            txtCellID.Text = sData;
                        }
                    }
                }

                // 동일한 Cell ID 가 입력되었는지 여부 확인
                // 스프레드 rows 카운트가 0보다 크면 아래 로직 수행
                if (dgCell_Info.GetRowCount() > 0)
                {
                    DataTable dtCellList = DataTableConverter.Convert(dgCell_Info.ItemsSource);
                    DataRow drCell = dtCellList.Select("SUBLOTID = '" + txtCellID.Text.Trim() + "'").FirstOrDefault();
                    if (drCell != null)
                    {
                        //오른쪽 List에 이미 존재하는 Cell ID입니다.
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3161"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        Util.MessageValidation("SFU3161", (result) =>
                          {
                            //  dgCell_Info.SelectedIndex = dtCellList.Rows.IndexOf(drCell);
                            
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                           
                        });
                        return false;
                    }
                }

                try
                {
                    DataTable isDataTable = DataTableConverter.Convert(dgCell_Info.ItemsSource);
                    if (dgCell_Info.ItemsSource == null)
                    {
                        isDataTable.Columns.Add("SEQ");
                        isDataTable.Columns.Add("SUBLOTID");
                        isDataTable.Columns.Add("LOTID");

                    }
                    DataRow row = isDataTable.NewRow();
                    row["SUBLOTID"] = txtCellID.Text.Trim();                            // 현재 스캔된 CELL ID

                    string msg = "";
                    string skip = "";

                    if (chkSkip.IsChecked == true)
                    {
                        skip = "Y";
                    }
                    else
                    {
                        skip = "N";
                    }

                    // CELL ID 의 조립 LOTID 가져옴과 동시에 활성화 특성치 데이터 조회 함수 호출
                    string templot = SelectAssyLotID(txtCellID.Text.Trim(), skip, out msg);
                    row["LOTID"] = templot;
                    // 조립LotID 정보 없으면 다시 스캔하도록 focus
                    if (string.IsNullOrWhiteSpace(templot))
                    {
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        Util.MessageValidation(msg, (result) =>
                        {
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                                return;
                          
                        });
                        return false;
                    }
                    if (SetLotTerm(templot))
                    {
                        // 그리드 데이터 바인딩
                        isDataTable.Rows.Add(row);
                        Util.GridSetData(dgCell_Info, isDataTable, FrameOperation);

                        // Cell순서
                        SetCellSeq();
                        // 정상적인 경우 다음 Cell 입력할 수 있게 전체 선택.
                        txtCellID.Focus();
                        txtCellID.SelectAll();
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return false;
                }
            }
            else
            {
              //  PlayWarnSound();
                // CELLID를 스캔 또는 입력하세요.
                Util.MessageValidation("SFU1323");
                return false;
            }
            return true;
        }
        #endregion

        private bool ScanBoxID()
        {
            if (!string.IsNullOrWhiteSpace(txtBoxID.Text))
            {
                // 기본적인 Validation
                if (string.IsNullOrWhiteSpace(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return false;
                }

                string sArea = Util.NVC(cboArea2.SelectedValue);
                if (sArea == string.Empty || sArea == "SELECT")
                {
                    //동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sLine = Util.NVC(cboEquipmentSegment2.SelectedValue);
                if (sLine == string.Empty || sLine == "SELECT")
                {
                    //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                    Util.MessageValidation("SFU1223");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sModel = Util.NVC(cboModelLot2.SelectedValue);
                if (sModel == string.Empty || sModel == "SELECT")
                {
                    //모델을 선택해주십시오.
                    Util.MessageValidation("SFU1257");
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                string sProduct = Util.NVC(cboProduct2.SelectedValue);
                if (sProduct == string.Empty || sProduct == "SELECT")
                {
                    //제품을 선택해주십시오.
                    Util.MessageValidation("SFU1895");
                  //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1895"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    return false;
                }

                if (string.IsNullOrEmpty(txtInputBoxQty.Value.ToString()) || txtInputBoxQty.Value.ToString() == "0" || txtInputBoxQty.Value.ToString() == "NaN")
                {
                    //스캔 설정치를 먼저 입력해주세요.
                    Util.MessageValidation("SFU3154", (result) =>
                 //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3154"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                       
                            txtInputBoxQty.Focus();
                       
                    });
                    return false;
                }

                // 현재 스캔된 수량이 설정치보다 클 경우
                if (Util.NVC_Int(txtScanBoxQty.Text) >= Util.NVC_Int(txtInputBoxQty.Value))
                {
                    //스캔 설정치를 먼저 입력해주세요.
                    //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3154"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>

                    Util.MessageValidation("SFU3154", (result) => 
                    {
                        
                            txtInputBoxQty.Focus();
                       
                    });
                    return false;
                }


                // 동일한 Cell ID 가 입력되었는지 여부 확인
                // 스프레드 rows 카운트가 0보다 크면 아래 로직 수행
                if (dgScanBox_Info.GetRowCount() > 0)
                {
                    // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                    for (int i = 0; i < dgScanBox_Info.GetRowCount(); i++)
                    {
                        if ((Util.NVC(dgScanBox_Info.GetCell(i, dgScanBox_Info.Columns["BOXID"].Index).Value) == txtBoxID.Text.Trim()))
                        {
                            // 해당 BOX는 이미 존재합니다.
                            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2011"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            Util.MessageValidation("SFU2011", (result) =>
                            {
                                dgScanBox_Info.SelectedIndex = i;
                                
                                    txtBoxID.Focus();
                                    txtBoxID.SelectAll();
                              
                            });
                            return false;
                        }
                    }
                }

                try
                {

                    string msg = "";
                    string sBoxID = txtBoxID.Text.Trim();


                    DataTable dtInfo = SearchCellInfo(sBoxID);

                    if (dtInfo == null || dtInfo.Rows.Count == 0)
                    {
                        //박스 [%1] 정보가 존재하지 않습니다.
                        Util.MessageValidation("SFU3363", (result) =>
                        {
                                txtBoxID.Focus();
                                txtBoxID.SelectAll();
                           
                        });
                        return false;
                    }

                    if (dtInfo.Select("OUTER_BOXID IS NOT NULL OR OUTER_BOXID <> ''").Count() > 0)
                    {
                        //해당 박스는 이미 팔레트 구성되었습니다.
                        Util.MessageValidation("SFU3362", (result) =>
                        {
                           
                                txtBoxID.Focus();
                                txtBoxID.SelectAll();
                           
                        });
                        return false;
                    }

                    if (dtInfo.Select("CELLHOLD = 'Y'").Count() > 0)
                    {
                        //해당 BOX는 HOLD된 CELL입니다.
                        //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3264"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        Util.MessageValidation("SFU3264", (result) =>
                        {
                            
                                txtBoxID.Focus();
                                txtBoxID.SelectAll();
                           
                        });
                        return false;
                    }

                    if (dtInfo.Select("WIPHOLD = 'Y'").Count() > 0)
                    {
                        //해당 BOX는 HOLD된 LOT입니다.
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3265"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>

                        Util.MessageValidation("SFU3265", (result) =>
                        {
                            txtBoxID.Focus();
                            txtBoxID.SelectAll();
                        });
                        return false;
                    }

                    string[] parameters = new string[2];
                    parameters[0] = dtInfo.Rows[0]["MDLLOT_ID"].ToString();
                    parameters[1] = cboModelLot2.SelectedValue.ToString();

                    if (parameters[0] != parameters[1])
                    {
                        //선택한 모델 ID([%1])와 입력한  BOX의 모델 ID([%2])가 상이합니다.
                        Util.MessageConfirm("SFU3266", result =>
                      {
                          if (result == MessageBoxResult.OK)
                          {
                              txtBoxID.Focus();
                              txtBoxID.SelectAll();
                          }
                      }, parameters);

                        return false;
                    }

                    string boxiD = txtBoxID.Text.Trim();

                    if (SetLotTermByBox(boxiD))
                    {
                        DataTable isDataTable = DataTableConverter.Convert(dgScanBox_Info.ItemsSource);
                        if (dgScanBox_Info.ItemsSource == null)
                        {
                            isDataTable.Columns.Add("BOXID");
                        }
                        DataRow row = isDataTable.NewRow();
                        row["BOXID"] = boxiD;                            // 현재 스캔된 Box ID

                        // 그리드 데이터 바인딩
                        isDataTable.Rows.Add(row);
                        Util.GridSetData(dgScanBox_Info, isDataTable, FrameOperation, true);

                        // 스캔된 마지막 셀이 바로 보이도록 스프레드 스크롤 하단으로 이동
                        dgScanBox_Info.ScrollIntoView(dgScanBox_Info.GetRowCount() - 1, 0);

                        // 정상적인 경우 다음 Box 입력할 수 있게 전체 선택.
                        txtBoxID.Focus();
                        txtBoxID.SelectAll();

                        DataTable dtPallet = DataTableConverter.Convert(dgPallet_Info.ItemsSource);
                        dtPallet.Merge(dtInfo);

                        Util.GridSetData(dgPallet_Info, dtPallet, FrameOperation, true);

                        txtScanBoxQty.Text = dgScanBox_Info.Rows.Count.ToString();
                        lblCellQty.Text = dgPallet_Info.Rows.Count.ToString();                        
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return false;
                }
            }
            else
            {
                Util.WarningPlayer();
                // 박스ID를 스캔 또는 입력하세요.
                Util.MessageValidation("SFU3413");
            }
            return true;
        }

        private void chk2DBCR_Click(object sender, RoutedEventArgs e)
        {
            if (chk2DBCR.IsChecked == true)
            {
                txtCellID.MaxLength = 80;
                txtCellID.Text = "";
            }
            else
            {
                txtCellID.MaxLength = 20;
                txtCellID.Text = "";
            }
        }

        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행  [CSR ID:2642448] 
        /// 2D barcode 를 읽어 CellID를 리턴함
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
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
                inData["AREAID"] = GetAreaID(cboArea);
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
                //입력한 [" + sBCR + "] 2D 바코드의 CELL ID 정보가 없습니다.
               // msg = "SFU3248" + Convert.ToString((char)13) + Convert.ToString((char)10) + ex.Message;
                return "";
            }

        }
        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수
        /// CellID를 매개변수로 하여 특성치 정보를 확인. NG판정을 하고 OutData 로 조립LotID 를 리턴함
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private string SelectAssyLotID(string cellID, string skip, out string msg)
        {
            try
            {
                if (chkTestMode.IsChecked != true)
                {
                    DataSet indataSet = new DataSet();
                    DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                    RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                    RQSTDT.Columns.Add("SHOPID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("EQSGID", typeof(string));
                    RQSTDT.Columns.Add("PRODID", typeof(string));
                    RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                    RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = cellID;
                    dr["SHOPID"] = GetShopID(cboArea);
                    dr["AREAID"] = GetAreaID(cboArea);
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                    dr["PRODID"] = txtProdID.Text;
                    dr["MDLLOT_ID"] = cboModelLot.SelectedValue;
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                    dr["INSP_SKIP_FLAG"] = chkSkip.IsChecked == true ? "Y" : "N";
                    dr["2D_BCR_SKIP_FLAG"] = chk2D.IsChecked == true ? "Y" : "N";
                    dr["USERID"] = txtWorker.Tag.ToString();
                    RQSTDT.Rows.Add(dr);
                    
                    // ClientProxy2007
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);

                    //DataTable RQSTDT = new DataTable();
                    //RQSTDT.Columns.Add("CELLID", typeof(string));
                    //RQSTDT.Columns.Add("COND_SKIP", typeof(string));
                    //RQSTDT.Columns.Add("BCRCHECK_YN", typeof(string));
                    ////RQSTDT.Columns.Add("LINEID", typeof(string));
                    ////RQSTDT.Columns.Add("MODELID", typeof(string));

                    //DataRow dr = RQSTDT.NewRow();
                    //dr["CELLID"] = cellID;
                    //dr["COND_SKIP"] = skip;
                    //dr["BCRCHECK_YN"] = sBCR_Check;
                    ////dr["LINEID"] = sLineID;
                    ////dr["MODELID"] = sModelID;
                    //RQSTDT.Rows.Add(dr);

                    //DataTable dtRslt = null;
                    //if (sAREAID.Equals("A1"))
                    //{
                    //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_v01", "INDATA", "OUTDATA", RQSTDT);
                    //}
                    //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                    //{
                    //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_v01", "INDATA", "OUTDATA", RQSTDT);
                    //}
                    //else
                    //{
                    //    msg = "";
                    //    return null;
                    //}

                    msg = "";
                    //return Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    return Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["LOTID"]);
                }
                else
                {
                    //BR_PRD_CHK_PACKING_CELL
                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                    inDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inDataTable.Columns.Add("IFMODE", typeof(string));
                    inDataTable.Columns.Add("USERID", typeof(string));
                    inDataTable.Columns.Add("EQSGID", typeof(string));
                    inDataTable.Columns.Add("MODELID", typeof(string));


                    DataTable inDataCellTable = indataSet.Tables.Add("IN_SUBLOT");
                    inDataCellTable.Columns.Add("SUBLOTID", typeof(string));

                    DataRow inData = inDataTable.NewRow();
                    inData["SRCTYPE"] = "UI";
                    inData["IFMODE"] = "OFF";
                    inData["USERID"] = txtWorker.Tag;
                    inData["EQSGID"] = cboEquipmentSegment.SelectedValue;
                    inData["MODELID"] = cboModelLot.SelectedValue;
                    inDataTable.Rows.Add(inData);

                    DataRow inDataCell = inDataCellTable.NewRow();
                    inDataCell["SUBLOTID"] = cellID;
                    inDataCellTable.Rows.Add(inDataCell);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_PACKING_CELL_FCS", "IN_EQP,IN_SUBLOT", "OUT_SUBLOT", indataSet);

                    msg = "";
                    return Util.NVC(dsResult.Tables["OUT_SUBLOT"].Rows[0]["LOTID"]);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return null;
            }

        }

        /// <summary>
        /// 활성화에서 리턴된 조립LOT의 생성일자 가져오기
        /// </summary>
        /// <param name="sLotid"></param>
        /// <returns></returns>
        private string GetLotCreatDateByLot(string sLotid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE_BY_LOT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return Util.NVC(dtResult.Rows[0]["CALDATE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        //private string GetLotTermByBox(string sBoxID_List, out minLot, out maxLot)
        //{
        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.Columns.Add("BOXID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["BOXID"] = sBoxID_List;
        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_BOX_CP", "RQSTDT", "RSLTDT", RQSTDT);
        //        minLot = "";
        //        maxLot = "";


        //        return Util.NVC(dtResult.Rows[0]["CALDATE"]);
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //        return "";
        //    }
        //}

        private bool SearchLotTerm(string sMinLot, string sMaxLot, bool bBoxFlag = false)   //Lot편차 조회 함수
        {
            //BizData data = new BizData("DO_BOX_LOTTERM", "RSLTDT");

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("MIN_LOTID", typeof(string));
                RQSTDT.Columns.Add("MAX_LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MIN_LOTID"] = sMinLot;
                dr["MAX_LOTID"] = sMaxLot;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VAL_LOT_TERM_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    string sLotTerm = Util.NVC_Int(dtResult.Rows[0]["LOTTERM"]).ToString();

                    if (bBoxFlag)
                    {
                        if (chkLot_Term2.IsChecked == true)
                        {
                            int iLotTerm = Util.NVC_Int(lblPackOut_Go_LotTerm2.Text);

                            if (int.Parse(sLotTerm) > iLotTerm)
                            {
                                Util.MessageValidation("SFU3267", iLotTerm.ToString());

                                //string sMsg = "Lot편차 값이 설정된 값 [" + iLotTerm.ToString() + "]일 보다 큽니다";
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                                //bErrChk = true;
                                return false;
                            }

                        }
                        lblMin2.Text = sMinLot;
                        lblMax2.Text = sMaxLot;
                        lblLotTerm2.Text = sLotTerm;

                    }
                    else
                    {

                        if (chkLot_Term.IsChecked == true)
                        {
                            int iLotTerm = Util.NVC_Int(lblPackOut_Go_LotTerm.Text);

                            if (int.Parse(sLotTerm) > iLotTerm)
                            {
                                Util.MessageValidation("SFU3267", iLotTerm.ToString());

                                //string sMsg = "Lot편차 값이 설정된 값 [" + iLotTerm.ToString() + "]일 보다 큽니다";
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                                //bErrChk = true;
                                return false;
                            }

                        }
                        lblMin.Text = sMinLot;
                        lblMax.Text = sMaxLot;
                        lblLotTerm.Text = sLotTerm;
                    }                 
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return false;
            }

        }
        private string CreateBox(bool bAuto = true)
        {
            string boxID = string.Empty;

            int totalqty = Convert.ToInt32(txtInputCell_Qty.Value);
            int settingQty = int.Parse(txtScanCell_Qty.Text.Trim());

            if (totalqty != settingQty)
            {
                //SFU3275  설정한 수량과 스캔한 수량이 일치하지 않습니다.
                Util.MessageValidation("SFU3275");
                return null;
            }

            else if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("INSPECT_SKIP", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO", typeof(string));
                inDataTable.Columns.Add("PACKQTY", typeof(Decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTUSER", typeof(string));

                DataTable inDataCellTable = indataSet.Tables.Add("INDATA_SUBLOT");
                inDataCellTable.Columns.Add("PACKCELLSEQ", typeof(decimal));
                inDataCellTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inData["PROCID"] = Process.CELL_BOXING;
                inData["MODLID"] = cboModelLot.SelectedValue;
                inData["PRODID"] = cboProduct.SelectedValue;
                //포장구성확인 해제 (포장검사 Skip 여부??)
                if (chkSkip.IsChecked == true)
                {
                    inData["INSPECT_SKIP"] = "Y";
                }
                else
                {
                    inData["INSPECT_SKIP"] = "N";
                }

                inData["PACKOUT_GO"] = Util.NVC(cboPackOut_Go.SelectedValue);
                inData["USERID"] = txtWorker.Tag;
                inData["ACTUSER"] = txtWorker.Text;
                inDataTable.Rows.Add(inData);


                if (dgCell_Info == null) return null;

                for (int row = 0; row < dgCell_Info.Rows.Count; row++)
                {
                    DataRow inDataCell = inDataCellTable.NewRow();
                    inDataCell["PACKCELLSEQ"] = dgCell_Info.GetCell(row, dgCell_Info.Columns["SEQ"].Index).Value;
                    inDataCell["SUBLOTID"] = dgCell_Info.GetCell(row, dgCell_Info.Columns["SUBLOTID"].Index).Value;
                    inDataCellTable.Rows.Add(inDataCell);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(_SaveBoxBizName, "INDATA,INDATA_SUBLOT", "OUTDATA", indataSet);
                boxID = dsResult.Tables["OUTDATA"].Rows[0][0].ToString();
               
                //Pallet Box 설정시
                if (Util.NVC_Int(txtInputBox_Qty.Value) == double.NaN || Util.NVC_Int(txtInputBox_Qty.Value) == 0)
                {
                    DataTable dtSource = SearchBoxInfo(boxID);
                    // Box정보 조회
                    Util.GridSetData(dgBox_Info, dtSource, FrameOperation,true);
                }
                else
                {
                    DataTable dtSource = DataTableConverter.Convert(dgBox_Info.ItemsSource);
                    dtSource.Merge(SearchBoxInfo(boxID));
                    // Box정보 조회
                    Util.GridSetData(dgBox_Info, dtSource, FrameOperation, true);
                    txtScanBox_Qty.Text = dgBox_Info.Rows.Count().ToString();
                }

                #region [ C20211215-000009 ] - Commented by kimgwango on 2022.01.02
                // Print_Box(boxID);
                #endregion
                
                #region [ C20211215-000009 ] - Added by kimgwango on 2022.01.02
                Print_Box(boxID, bAuto);
                #endregion


                if (txtInputBox_Qty.Value != 0 && txtInputBox_Qty.Value != double.NaN && txtScanBox_Qty.Text == Util.NVC(txtInputBox_Qty.Value))
                {
                    // 팝업창을 띄우고, Cell 정보 저장한 객체 넘김
                    //실적 확정시 수정이 불가합니다.그래도 확정 하시겠습니까 ?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3156"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            PopUpConfirm();
                        }
                        Clear_Cell(true);
                    });
                }
               if(!bAuto)
               {
                    //저장되었습니다. 
                    Util.MessageInfo("SFU1270", (result) =>
                    {
                        Clear_Cell(true);
                    });    
                }        
               else
                    Clear_Cell();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.Alert(ex.Message);
            }

            return boxID;
        }

        private DataTable SearchBoxInfo(string BoxID_List)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("BOXID_LIST", typeof(string));

            try
            {
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID_LIST"] = BoxID_List;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKING_BOX_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return RQSTDT;
            }
        }

        private DataTable SearchCellInfo(string BoxID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("BOXID", typeof(string));

            try
            {
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = BoxID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKING_CELL_INFO", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return RQSTDT;
            }
        }

        private DataTable GetAssyLotSum(string sBOXID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sBOXID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_SUM_BYPALLETID_CP", "RQSTDT", "RSLTDT", RQSTDT);
                // dgLotSum.ItemsSource = DataTableConverter.Convert(dtResult);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void Print_Box(string BOXID, bool bAuto = false)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                // CNB는 추가 Validation
                if (LoginInfo.CFG_SHOP_ID.Equals("G631") || LoginInfo.CFG_SHOP_ID.Equals("G634")) 
                {
                    // 상세정보입력 체크박스 선택 시 
                    if (chkDetail.IsChecked == true) 
                    {
                        // 중량 정보 입력 확인
                        if (String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) || String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()))
                        {
                            // 포장 중량을 입력하세요.
                            Util.MessageInfo("SFU4390");
                            return;
                        }
                        // 출력 정보 입력 확인
                        if (String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()))
                        {
                            // Rated Power를입력하세요.
                            Util.MessageInfo("SFU8121");
                            return;
                        }
                    }
                    
                }

                string sBizRule = "BR_PRD_GET_BOX_LABEL_CP";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("N_WEIGHT", typeof(string)); // Net Weight - CNB 전용
                inDataTable.Columns.Add("G_WEIGHT", typeof(string)); // Gross Weight - CNB 전용
                inDataTable.Columns.Add("R_POWER", typeof(string)); // Rated Power - CNB 전용
                inDataTable.Columns.Add("PGM_ID", typeof(string));
                inDataTable.Columns.Add("BZRULE_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = BOXID;
                newRow["LABEL_TYPE_CODE"] = "INBOX";
                newRow["USERID"] = txtWorker.Tag;
                newRow["N_WEIGHT"] = String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) ? null : txtnetweight.Text.ToString().Trim();
                newRow["G_WEIGHT"] = String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()) ? null : txtgrossweight.Text.ToString().Trim();
                newRow["R_POWER"] = String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()) ? null : txtratedpower.Text.ToString().Trim();
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK", typeof(string));
                inPrintTable.Columns.Add("RESO", typeof(string));
                inPrintTable.Columns.Add("PRCN", typeof(string));
                inPrintTable.Columns.Add("MARH", typeof(string));
                inPrintTable.Columns.Add("MARV", typeof(string));
                inPrintTable.Columns.Add("DARK", typeof(string));

                newRow = inPrintTable.NewRow();              
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                inPrintTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_LABEL_CP", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);                         
                            return;
                        }

                        if (bizResult == null || bizResult.Tables["OUTDATA"] == null || bizResult.Tables["OUTDATA"].Rows.Count <= 0 || string.IsNullOrWhiteSpace((string)bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"]))
                        {
                            Util.MessageValidation("SFU1348");  //프린트를 실패하였습니다
                            return;
                        }
                        #region [ C20211215-000009 ] [ESMI] 포장 Box 라벨 발행수 설정의 건 - Commented by kimgwango on 2022.01.03
                        //if (!PrintZPL(bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString(), _drPrtInfo))
                        //    Util.MessageValidation("SFU1348");  //프린트를 실패하였습니다
                        #endregion

                        #region [ C20211215-000009 ] [ESMI] 포장 Box 라벨 발행수 설정의 건 - Added by kimgwango on 2022.01.03
                        //int boxLabelCopyCnt = 1;
                        //if (bAuto) boxLabelCopyCnt = BoxLabelCopy;

                        for (int i = 0; i < BoxLabelCopy; i++)
                        {
                            if (!PrintZPL(bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString(), _drPrtInfo))
                            {
                                Util.MessageValidation("SFU1348");  //프린트를 실패하였습니다
                                break;
                            }
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);

                #region 삭제
                //DataTable dtRqstLabel = new DataTable();
                //dtRqstLabel.Columns.Add("LANGID", typeof(string));
                //dtRqstLabel.Columns.Add("PALLETID", typeof(string));
                //dtRqstLabel.Columns.Add("LABEL_TYPE_CODE", typeof(string));

                //DataRow drLabel = dtRqstLabel.NewRow();
                //drLabel["LANGID"] = LoginInfo.LANGID;
                //drLabel["PALLETID"] = BOXID;
                //drLabel["LABEL_TYPE_CODE"] = "INBOX";
                //dtRqstLabel.Rows.Add(drLabel);

                //DataTable dtRsltLable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_PALLET_INFO", "INDATA", "OUTDATA", dtRqstLabel);

                //if (dtRsltLable.Rows.Count == 0)
                //{
                //    Util.MessageValidation("SFU1180"); //BOX 정보가 없습니다.
                //    return;
                //}

                //else if (dtRsltLable.Rows.Count > 0 && dtRsltLable.Rows[0]["LABEL_CODE"].ToString() == "")
                //{
                //    Util.MessageValidation("SFU1520"); //"라벨 기준정보가 등록되지 않은 제품 입니다."
                //    return;
                //}

                //string sLabelCode = dtRsltLable.Rows[0]["LABEL_CODE"].ToString();

                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("LBCD", typeof(string));
                //dtRqst.Columns.Add("PRMK", typeof(string));
                //dtRqst.Columns.Add("RESO", typeof(string));
                //dtRqst.Columns.Add("PRCN", typeof(string));
                //dtRqst.Columns.Add("MARH", typeof(string));
                //dtRqst.Columns.Add("MARV", typeof(string));
                //dtRqst.Columns.Add("ATTVAL001", typeof(string));
                //dtRqst.Columns.Add("ATTVAL002", typeof(string));
                //dtRqst.Columns.Add("ATTVAL003", typeof(string));
                //dtRqst.Columns.Add("ATTVAL004", typeof(string));
                //dtRqst.Columns.Add("ATTVAL005", typeof(string));
                //dtRqst.Columns.Add("ATTVAL006", typeof(string));
                //dtRqst.Columns.Add("ATTVAL007", typeof(string));
                //dtRqst.Columns.Add("ATTVAL008", typeof(string));
                //dtRqst.Columns.Add("ATTVAL009", typeof(string));
                //dtRqst.Columns.Add("ATTVAL010", typeof(string));
                //dtRqst.Columns.Add("ATTVAL011", typeof(string));
                //dtRqst.Columns.Add("ATTVAL012", typeof(string));
                //dtRqst.Columns.Add("ATTVAL013", typeof(string));
                //dtRqst.Columns.Add("ATTVAL014", typeof(string));
                //dtRqst.Columns.Add("ATTVAL015", typeof(string));
                //dtRqst.Columns.Add("ATTVAL016", typeof(string));
                //dtRqst.Columns.Add("ATTVAL017", typeof(string));
                //dtRqst.Columns.Add("ATTVAL018", typeof(string));
                //dtRqst.Columns.Add("ATTVAL019", typeof(string));
                //dtRqst.Columns.Add("ATTVAL020", typeof(string));
                //dtRqst.Columns.Add("ATTVAL021", typeof(string));
                //dtRqst.Columns.Add("ATTVAL022", typeof(string));
                //dtRqst.Columns.Add("ATTVAL023", typeof(string));
                //dtRqst.Columns.Add("ATTVAL024", typeof(string));
                //dtRqst.Columns.Add("ATTVAL025", typeof(string));
                //dtRqst.Columns.Add("ATTVAL026", typeof(string));
                //dtRqst.Columns.Add("ATTVAL027", typeof(string));

                //DataRow dr = dtRqst.NewRow();
                //dtRqst.Rows.Add(dr);

                //DataRow drBox = SearchBoxInfo(BOXID).Select("BOXID = '" + BOXID + "'").FirstOrDefault();
                //DataTable dtInfo = GetAssyLotSum(BOXID);

                //dtRqst.Rows[0]["LBCD"] = sLabelCode;
                //dtRqst.Rows[0]["PRMK"] = _sPrt;
                //dtRqst.Rows[0]["RESO"] = _sRes;
                //dtRqst.Rows[0]["PRCN"] = _sCopy;
                //dtRqst.Rows[0]["MARH"] = _sXpos;
                //dtRqst.Rows[0]["MARV"] = _sYpos;
                //dtRqst.Rows[0]["ATTVAL001"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL002"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL003"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL004"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL005"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL006"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL007"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL008"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL009"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL010"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL011"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL012"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL013"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL014"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL015"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL016"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL017"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL018"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL019"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL020"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL021"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL022"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL023"] = "LOT";
                //dtRqst.Rows[0]["ATTVAL024"] = "QTY";
                //dtRqst.Rows[0]["ATTVAL025"] = drBox == null ? string.Empty : ((DateTime)drBox["PACKDTTM"]).ToString("yyyy.MM.dd");//포장일자
                //dtRqst.Rows[0]["ATTVAL026"] = BOXID;//boxid  
                //dtRqst.Rows[0]["ATTVAL027"] = BOXID;//boxid              

                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                //if (PrintZPL(dtRslt.Rows[0]["LABELCD"].ToString(), _drPrtInfo))
                //{
                //    DataTable dtLog = new DataTable();
                //    dtLog.Columns.Add("PALLETID", typeof(string));
                //    dtLog.Columns.Add("LABEL_CODE", typeof(string));
                //    dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                //    dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM01", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM02", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM03", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM04", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM05", typeof(string));
                //    dtLog.Columns.Add("PRT_ITEM06", typeof(string));
                //    dtLog.Columns.Add("INSUSER", typeof(string));

                //    DataRow drLog = dtLog.NewRow();

                //    drLog["PALLETID"] = BOXID;
                //    drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                //    drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                //    drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                //    drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                //    drLog["PRT_ITEM05"] = dtRqst.Rows[0]["ATTVAL005"];
                //    drLog["PRT_ITEM06"] = dtRqst.Rows[0]["ATTVAL006"];
                //    drLog["INSUSER"] = txtWorker.Tag;

                //    dtLog.Rows.Add(drLog);

                //    PrintLog(dtLog);
                //}
                #endregion
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

        private void RePrint_Box(string BOXID)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker.Text))
                {
                    //작업자를 선택해 주세요
                    Util.MessageValidation("SFU1843");
                    return;
                }

                string sBizRule = "BR_PRD_GET_BOX_LABEL_CP";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PGM_ID", typeof(string));
                inDataTable.Columns.Add("BZRULE_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = BOXID;
                newRow["LABEL_TYPE_CODE"] = "INBOX";
                newRow["USERID"] = txtWorker.Tag;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inDataTable.Rows.Add(newRow);
                newRow = null;

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK", typeof(string));
                inPrintTable.Columns.Add("RESO", typeof(string));
                inPrintTable.Columns.Add("PRCN", typeof(string));
                inPrintTable.Columns.Add("MARH", typeof(string));
                inPrintTable.Columns.Add("MARV", typeof(string));
                inPrintTable.Columns.Add("DARK", typeof(string));

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = _sPrt;
                newRow["RESO"] = _sRes;
                newRow["PRCN"] = _sCopy;
                newRow["MARH"] = _sXpos;
                newRow["MARV"] = _sYpos;
                newRow["DARK"] = _sDark;
                inPrintTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_LABEL_CP", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                new ClientProxy().ExecuteService_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult == null || bizResult.Tables["OUTDATA"] == null || bizResult.Tables["OUTDATA"].Rows.Count <= 0 || string.IsNullOrWhiteSpace((string)bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"]))
                        {
                            Util.MessageValidation("SFU1348");  //프린트를 실패하였습니다
                            return;
                        }
                        if (!PrintZPL(bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"].ToString(), _drPrtInfo))
                            Util.MessageValidation("SFU1348");  //프린트를 실패하였습니다
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
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
        private void Print_Pallet()
        {

        }
        private string CreatePallet()
        {
            string palletID = string.Empty;
            Print_Pallet();
            return palletID;
           
        }

      

        private string GetAreaID(C1ComboBox cb)
        {
            string sTemp = Util.NVC(cb.SelectedValue);

            if (string.IsNullOrWhiteSpace(sTemp) || sTemp == "SELECT")
            {
                sTemp = string.Empty;
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sTemp = sArry[0];
            }
            return sTemp;
        }

        private string GetShopID(C1ComboBox cb)
        {
            string sTemp = Util.NVC(cb.SelectedValue);

            if (string.IsNullOrWhiteSpace(sTemp) || sTemp == "SELECT")
            {
                sTemp = string.Empty;
            }
            else
            {
                string[] sArry = sTemp.Split('^');
                sTemp = sArry[1];
            }
            return sTemp;
        }

        /// <summary>
        ///  model, line 로  2D bar 사용여부 체크
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private string Select2DBCRCHK()
        {

            //BizData formationData = new BizData("QR_WEBMI122_SET_DATA", "OUTDATA");   //  cell id bizactor   
            try
            {
                string sLineID = Util.NVC(cboEquipmentSegment.SelectedValue);
                string sProdID = Util.NVC(cboProduct.SelectedValue);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = sLineID;
                dr["PRODID"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_USE_2DBCR_BY_MODL_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return Util.NVC(dtResult.Rows[0]["BCR2D_YN"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return "";
            }
        }
        private void PrintLog(DataTable dt)
        {
            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_BOX_LABEL_COUNT", "INDATA", null, dt);
                DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_LABEL_HIST", "INDATA", null, dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #region [바코드 프린터 발행용]
        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        //    try
        //    {
        //        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
        //        {
        //            if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
        //            {
        //                // FrameOperation.PrintFrameMessage(string.Empty);
        //                bool brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);//Barcode_ZPL_Print(dr, sZPL);
        //                if (brtndefault == false)
        //                {
        //                    loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
        //                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309")); //Barcode Print 실폐
        //                    return;
        //                }
        //            }
        //        }

        //        //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZPL);
        //        //wndPopup.Show();
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //    }
        #endregion

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

               // loadingIndicator.Visibility = Visibility.Visible;
                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return dtResult;
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
                dr["SHOPID"] = GetShopID(cboArea);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "RQSTDT", "RSLTDT", RQSTDT);

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
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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

        private DataTable setPalletTag(string sUserName, string sPalletID, int iSelRow, string sTopProdID)
        {
            string sProjectName = string.Empty;

            DataTable dtinfo = SelectScanPalletInfo(sPalletID);
            DataTable dtAssyLot = new DataTable();

            // Tray _ MagazineID / 수량 저장을 위한 DataTable
            DataTable dtBox = new DataTable();

            // Palelt ID
            string sPackWrkType = Util.NVC(dtinfo.Rows[iSelRow]["LOT_TYPE"]);

            dtBox = SelectTagInformation(sPalletID, sPackWrkType);
            dtAssyLot = SearchAssyLot(sPalletID);
            
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
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
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

            dr["SHIP_LOC"] = sShipToName;
            dr["SHIPDATE"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPDATE_SCHEDULE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPDATE_SCHEDULE"].Index).Value);
            dr["OUTGO"] = Util.NVC(dtinfo.Rows[iSelRow]["SHIPTO_NOTE"]);  //  Util.NVC(grid.GetCell(iSelRow, grid.Columns["SHIPTO_NOTE"].Index).Value);
            dr["NOTE"] = Util.NVC(dtinfo.Rows[iSelRow]["PACK_NOTE"]);  // Util.NVC(grid.GetCell(iSelRow, grid.Columns["PACK_NOTE"].Index).Value);
            dr["UNCODE"] = Util.NVC(dtinfo.Rows[iSelRow]["UN_CODE"]);
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
            if (bTop_ProdID_Use_Flag)
                dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID;
            else
                dr["PRODID"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["PRODID"].Index).Value);
            dr["SHIPQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["SHIPQTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["SHIPQTY"].Index).Value));
            dr["PARTNO"] = "";
            dr["OUTQTY"] = string.Format("{0:#,###}", Util.NVC(dtinfo.Rows[iSelRow]["QTY"]));// Util.NVC_Int(grid.GetCell(iSelRow, grid.Columns["QTY"].Index).Value));
            dr["USERID"] = Util.NVC(dtinfo.Rows[iSelRow]["REG_USERNAME"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["REG_USERNAME"].Index).Value);
            dr["CONBINESEQ2"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
            dr["CONBINESEQ1"] = Util.NVC(dtinfo.Rows[iSelRow]["COMBINESEQ"]);  //Util.NVC(grid.GetCell(iSelRow, grid.Columns["COMBINESEQ"].Index).Value);
            dr["SKIPYN"] = Util.NVC(dtinfo.Rows[iSelRow]["PRODID"]) == "Y" ? "SKIP" : "NO SKIP";   // Util.NVC(grid.GetCell(iSelRow, grid.Columns["INSP_SKIP_FLAG"].Index).Value) == "Y" ? "SKIP" : "NO SKIP";


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
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return "0";
            }
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

        private void btnPackList_Click (object sender, RoutedEventArgs e)
        {
            if (cboEquipmentSegment2.SelectedValue.ToString() == string.Empty || cboEquipmentSegment2.SelectedValue.ToString() == "SELECT")
            {
                // 동을 선택하세요.
                Util.MessageInfo("SFU1843");
                return;
            }
            BOX001_026_PACK_STANBY_LIST popupPackList = new BOX001_026_PACK_STANBY_LIST { FrameOperation = FrameOperation };

            object[] parameters = new object[5];
            parameters[0] = Util.NVC(txtWorker.Tag);



            C1WindowExtension.SetParameters(popupPackList, parameters);

            popupPackList.Closed += popupPackList_Closed;
            grdMain.Children.Add(popupPackList);
            popupPackList.BringToFront();

        }

        private void popupPackList_Closed(object sender, EventArgs e)
        {
            BOX001_026_PACK_STANBY_LIST popup = sender as BOX001_026_PACK_STANBY_LIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }


        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = false;
            txtgrossweight.IsReadOnly = false;
            txtratedpower.IsReadOnly = false;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = true;
            txtgrossweight.IsReadOnly = true;
            txtratedpower.IsReadOnly = true;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;
        }
        // C20210528-000335 ESNA 요청 검사조건 SKIP, 2D BCR SKIP MMD 기준 정보에 등록된 유저에게만 Visible 처리
        // 아래 로직은 포장 Pallet 구성 개별 Cell 화면과 동일함(BOX001_007.xaml.cs)
        private void GetBCRSkipVisible()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["COM_CODE"] = "BOX_BCR_CHK_ADMIN";
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count == 0)
            {
                chk2D.Visibility = Visibility.Visible;
            }
            else
            {
                DataTable inTable2 = new DataTable();
                inTable2.Columns.Add("AREAID", typeof(string));
                inTable2.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable2.Columns.Add("COM_CODE", typeof(string));

                DataRow dr2 = inTable2.NewRow();
                dr2["COM_TYPE_CODE"] = "BOX_BCR_CHK_ADMIN";
                dr2["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr2["COM_CODE"] = LoginInfo.USERID;

                inTable2.Rows.Add(dr2);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", inTable2);

                if (dtRslt2.Rows.Count == 0)
                {
                    chk2D.Visibility = Visibility.Collapsed;
                    chkSkip.Visibility = Visibility.Collapsed;
                }
                else
                {
                    chk2D.Visibility = Visibility.Visible;
                    chkSkip.Visibility = Visibility.Visible;
                }

            }

            return;
        }

        private string GetTopProdID(string sPalletID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("STR_ID", typeof(string));
                RQSTDT.Columns.Add("GBN_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TYPE_CODE"] = "B";
                dr["STR_ID"] = sPalletID;
                dr["GBN_ID"] = "A";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TOP_PRODID", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    return Util.NVC(dtResult.Rows[0]["TOP_PRODID"]).ToString();
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void ChkUseTopProdID()
        {
            try
            {
                bTop_ProdID_Use_Flag = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SHOPID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_TOP_PRODID_USE", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0 && Util.NVC(dtResult.Rows[0]["TOP_PRODID_USE_FLAG"]).Equals("Y"))
                {
                    bTop_ProdID_Use_Flag = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [ Util ] - DA
        private int GetBoxLabelCopy()
        {
            int result = 1;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "BOX_LABEL_COPY";
            dr["COM_CODE"] = "BOX_LABEL_COPY";

            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);
                
                if (dtRslt?.Rows?.Count > 0)
                {
                    int boxLabelCopy = 0;
                    string attr1 = dtRslt.Rows[0]["ATTR1"]?.ToString();

                    if (int.TryParse(attr1, out boxLabelCopy))
                    {
                        result = boxLabelCopy;
                    }
                }
            }catch(Exception ex)
            {
                // Util.MessageException(ex);
            }

            return result;
        }
        #endregion
    }
}
