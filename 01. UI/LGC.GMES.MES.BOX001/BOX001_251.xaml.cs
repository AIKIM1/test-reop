/*************************************************************************************
 Created Date : 2020.09.15
      Creator : 서동현
   Decription : 포장 Pallet 구성 (개별 Cell/Carrier)
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.15  서동현 책임 : Initial Created.
  2020.10.17 오화백 : 공통코드(INSP_SKIP_BLOCK_AREA)를 통해서 검사 조건 Skip 사용여부 로직 추가
  2020.12.04 이동우 : 2D BCR CHK 사용 권한 확인 추가 (동별 공통코드로 분기 -  BOX_BCR_CHK_ADMIN) 
  2022.08.08 최도훈 : Line 호출부분 조립, 활성화 시스템에 따라 분기
**************************************************************************************/
using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_251 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        //EXCEL - CELL파일등록시 ERROR 발생시 처리용.
        private bool bErrChk = false;
        private bool bPilotChk = false;
        string sAREAID = string.Empty;
        string sSHOPID = string.Empty;

        private string sWO_DETL_ID = string.Empty;

        // 스프레드에 새로 Row를 추가해야 해서 필요한 변수
        private DataTable isDataTable = new DataTable();
        private DataTable isLotTable = new DataTable();

        // 스캔 수량 저장하기 위한 변수
        private int isScanQty = 0;
        // 카운트 시점의 Index 저장하기 위한 변수
        private int isIndex = 0;
        // 검사하지 않고 진행한 셀이 하나라도 존재하는지 확인하기 위한 변수
        private bool cellCheck = false;

        // lot 편차 구하기 위한 변수 선언.
        string sMINLOT = "";
        string sMAXLOT = "";
        string sMINDATE = "";
        string sMAXDATE = "";

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            Content = "ALL",
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public BOX001_251()
        {
            InitializeComponent();
            Loaded += BOX001_251_Loaded;
        }

        private void BOX001_251_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_251_Loaded;
            // 기본은 체크 해제
            //chkTrayRefresh.Checked = false;
            //chkDummy.Visible = false;
            Window window = Window.GetWindow(this);
            window.Closing += Window_Closing;
            
            setInit();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnTempSave);
            listAuth.Add(btnTempSave);
            listAuth.Add(btnConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (GetPilotAdminArea() > 0) // 공통코드로 시생산 제품 관리하는 동에 따라 버튼 Visible
            {
                btnExtra.Visibility = Visibility.Visible;
                this.RegisterName("redBrush", redBrush);
                GetPilotProdMode();
            }
            else
            {
                btnExtra.Visibility = Visibility.Collapsed;
            }

            //(2020.10.17 오화백) 검사 조건 Skip 사용여부 기능추가
            if (GetSkipBlockVisible() > 0)
            {
                chkSkip.Visibility = Visibility.Collapsed;
            }
            else
            {
                chkSkip.Visibility = Visibility.Visible;
            }
            //2D BCR CHK 사용 권한 확인 추가 (동별 공통코드로 분기)

            GetBCRSkipVisible();
        }

        bool bCloseYn = false;

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (bCloseYn)
                    return;

                bCloseYn = true;

                if (dgCELLInfo.GetRowCount() < 1)
                    return;
                
                // 기본적인 Validation
                string sArea = Util.NVC(cboArea.SelectedValue);
                if (sArea == string.Empty || sArea == "SELECT")
                    return;                

                string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
                if (sLine == string.Empty || sLine == "SELECT")
                    return;

                string sModel = Util.NVC(cboModelLot.SelectedValue);
                if (sModel == string.Empty || sModel == "SELECT")
                    return;

                if (cboPackOut_Go.SelectedValue.Equals("") || cboPackOut_Go.SelectedValue.Equals("SELECT"))
                    return;

                if (string.IsNullOrWhiteSpace(txtWorker.Text))
                    return;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("MP_FLAG", typeof(string));
                inDataTable.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                inDataTable.Columns.Add("BCD_2D_SKIP_FLAG", typeof(string));
                inDataTable.Columns.Add("TOTL_CELL_QTY", typeof(decimal));
                inDataTable.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inDataDtlTable = indataSet.Tables.Add("INSUBLOT");
                inDataDtlTable.Columns.Add("SUBLOTID", typeof(string));
                inDataDtlTable.Columns.Add("PACK_SEQ", typeof(decimal));

                //Data
                //string sGD_PROD_YN = (optProd_Type1.IsChecked == true ? "Y" : "N");
                string sGD_PROD_YN = (chkLot_Term.IsChecked == true ? "Y" : "N");
                string sCOND_SKIP_YN = (chkSkip.IsChecked == true ? "Y" : "N");
                string sBCR_SKIP_YN = (chk2D.IsChecked == true ? "Y" : "N");

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = sAREAID;
                inData["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                inData["MDLLOT_ID"] = cboModelLot.SelectedValue.ToString();
                inData["WOID"] = null;   // txtWO.Text.Trim();
                inData["PRODID"] = txtProdID.Text.Trim();
                inData["SHIPTO_ID"] = cboPackOut_Go.SelectedValue.ToString();
                inData["MP_FLAG"] = sGD_PROD_YN;
                inData["INSP_SKIP_FLAG"] = sCOND_SKIP_YN;
                inData["BCD_2D_SKIP_FLAG"] = sBCR_SKIP_YN;
                inData["TOTL_CELL_QTY"] = txtSettingQty.Value;
                inData["PACK_TMP_TYPE_CODE"] = "PACK_CELL";
                //    inData["USERID"] = txtProdUser.Text;
                inData["USERID"] = txtWorker.Text;
                inDataTable.Rows.Add(inData);

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    string sCellID = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SUBLOTID"].Index).Value);
                    string sPackCellSeq = (i + 1).ToString();

                    DataRow inDataDtl = inDataDtlTable.NewRow();
                    inDataDtl["SUBLOTID"] = sCellID;
                    inDataDtl["PACK_SEQ"] = sPackCellSeq;
                    inDataDtlTable.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TMP_PACK_CELL", "INDATA,INSUBLOT", null, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void setInit()
        {
            if (LoginInfo.LOGGEDBYSSO == true)
            {
                txtWorker.Tag = LoginInfo.USERID;
                txtWorker.Text = LoginInfo.USERNAME;
            }

            //라인,모델 셋팅
            // String[] sFilter = { LoginInfo.CFG_AREA_ID };    // Area
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");

            dgCELLInfo.Columns["BOXID"].Visibility = Visibility.Collapsed;

            txtCellID.MaxLength = 20;
            txtCellID.Text = "";    //"JD12010102";
            txtCellID.Focus();
            txtCellID.SelectAll();

            txtBoxCellQty.Value = 18;
            chkLot_Term.IsChecked = true;

            if (LGC.GMES.MES.Common.Common.APP_MODE == "DEBUG")
                chkTestMode.Visibility = Visibility.Visible;
        }
        #endregion

        #region Event

        /// <summary>
        /// 사용안함.........
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWOSearch_Click(object sender, RoutedEventArgs e)
        {
            string sModelid = Util.NVC(cboModelLot.SelectedValue);
            if (sModelid == string.Empty || sModelid == "SELECT")
            {
                Util.MessageValidation("SFU1257"); //"모델을 선택해주십시오."
                return;
            }

            BOX001_006_WORKORDER popUp = new BOX001_006_WORKORDER();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Process.CELL_BOXING;
                Parameters[3] = sModelid;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(wndWO_Closed);
                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        private void wndWO_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_006_WORKORDER wndPopup = sender as BOX001.BOX001_006_WORKORDER;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                //초기화
                AllClear();

                DataTable dt = wndPopup.retDT;
                if (dt.Rows.Count > 0)
                {
                    sWO_DETL_ID = Util.NVC(dt.Rows[0]["WO_DETL_ID"]);
                    //txtWO.Text = Util.NVC(dt.Rows[0]["WOID"]);
                    txtProdID.Text = Util.NVC(dt.Rows[0]["PRODID"]);
                    //txtProdName.Text = Util.NVC(dt.Rows[0]["PRODNAME"]);
                }
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            // 작업자에게 다시 한 번 삭제 여부 묻기
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

            System.Data.DataRowView row = btn.DataContext as System.Data.DataRowView;

            DataTable dtTmpCELL = DataTableConverter.Convert(dgCELLInfo.ItemsSource);
            string sCell = Util.NVC(row.Row.ItemArray[dtTmpCELL.Columns.IndexOf("SUBLOTID")]);
            // string sMsg = "[" + sCell + "] CellID를 List에서 삭제하시겠습니까?";
            //삭제하시겠습니까?
            //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string templot = Util.NVC(row.Row.ItemArray[dtTmpCELL.Columns.IndexOf("LOTID")]); 

                    if (dgLotInfo.GetRowCount() > 0)
                    {
                        // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                        for (int lsCount = 0; lsCount < dgLotInfo.GetRowCount(); lsCount++)
                        {
                            if ( Util.NVC(dgLotInfo.GetCell(lsCount, dgLotInfo.Columns["LOTID"].Index).Value) == templot)
                            {
                                int lotqty = Util.NVC_Int(dgLotInfo.GetCell(lsCount, dgLotInfo.Columns["QTY"].Index).Value);

                                if (lotqty > 1)
                                {
                                    lotqty -= 1;
                                    DataTableConverter.SetValue(dgLotInfo.Rows[lsCount].DataItem, "QTY", lotqty);

                                    //화면에 반영된 내용을 DataTable에 다시 반영함.
                                    isLotTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());
                                }
                                else
                                {
                                    // 선택된 행 삭제
                                    dgLotInfo.IsReadOnly = false;
                                    dgLotInfo.RemoveRow(lsCount);
                                    dgLotInfo.IsReadOnly = true;

                                    //화면에 반영된 내용을 DataTable에 다시 반영함.
                                    isLotTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());
                                }
                            }
                        }
                    }

                    // 선택된 행 삭제
                    dgCELLInfo.IsReadOnly = false;
                    dgCELLInfo.RemoveRow(iRow);
                    dgCELLInfo.IsReadOnly = true;

                    //화면에 반영된 내용을 DataTable에 다시 반영함.
                    isDataTable = DataTableConverter.Convert(dgCELLInfo.GetCurrentItems());

                    //GridUtil.RemoveRow(sprCellInfo, e.Row);

                    ReCheck_LotTerm();   //[CSR ID:2487466]

                    // 행 삭제되었으니 변수의 값과 보여지는 값도 제함
                    isScanQty = isScanQty - 1;
                    txtScanqty.Text = isScanQty.ToString();
                }
            });
        }

        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Add_Scan_Cellid(txtCellID.Text.Trim()))
                    Util.DingPlayer();
                else
                    Util.WarningPlayer();
            }
        }

        /// <summary>
        /// CELL ID 등록하기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCellExcel_Click(object sender, RoutedEventArgs e)
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

                        txtSettingQty.Value = sheet.Rows.Count;
                        System.Windows.Forms.Application.DoEvents();
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            //DataRow dataRow = dataTable.NewRow();
                            string sCellID = string.Empty;

                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                sCellID = Util.NVC(cell.Text);
                                //txtCellID.Text = "";
                                //txtCellID.Text = sCellID;

                                //Cell List 추가..
                                Add_Scan_Cellid(sCellID);

                                System.Windows.Forms.Application.DoEvents();

                                if (bErrChk)
                                {
                                    bErrChk = false;
                                    Util.WarningPlayer();
                                    break;
                                }      
                            }
                        }
                        Util.DingPlayer();
                    }
                }

                #region old code
                //if (fd.ShowDialog() == true)
                //{
                //    using (Stream stream = fd.OpenFile())
                //    {
                //        string sFileName = fd.FileName.ToString();

                //        if (exl != null)
                //        {
                //            //이전 연결 해제
                //            exl.Conn_Close();
                //        }
                //        //파일명 Set 으로 연결
                //        exl.ExcelFileName = sFileName;
                //        string[] str = exl.GetExcelSheets();

                //        //첫번째 시트 DataTable반환.
                //        if (str.Length > 0)
                //        {
                //            DataTable dt = exl.GetSheetData(str[0]);

                //            txtSettingQty.Value = dt.Rows.Count;
                //            //Application.DoEvents();

                //            string sCellID = "";
                //            for (int i = 0; i < dt.Rows.Count; i++)
                //            {
                //                sCellID = Util.NVC(dt.Rows[i][0]);
                //                txtCellID.Text = "";
                //                txtCellID.Text = sCellID;
                //                //Cell List 추가..
                //                Add_Scan_Cellid();

                //                if (bErrChk)
                //                {
                //                    bErrChk = false;
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 임시 저장
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTempSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgCELLInfo.GetRowCount() < 1)
                return;

            // 기본적인 Validation
            string sArea = Util.NVC(cboArea.SelectedValue);
            if (sArea == string.Empty || sArea == "SELECT")
            {
                //동을 선택해주십시오.
                Util.MessageValidation("SFU1499");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sLine == string.Empty || sLine == "SELECT")
            {
                //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                Util.MessageValidation("SFU1223");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            string sModel = Util.NVC(cboModelLot.SelectedValue);
            if (sModel == string.Empty || sModel == "SELECT")
            {
                //모델을 선택해주십시오.
                Util.MessageValidation("SFU1257");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            if (cboPackOut_Go.SelectedValue.Equals("") || cboPackOut_Go.SelectedValue.Equals("SELECT"))
            {
                //출하처를 선택 하십시오
                Util.MessageValidation("SFU3173");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3173"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }

            //임시저장 하시겠습니까?
            //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3166"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU3166", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //Save2Cell_List(Util.NVC(cboProcUser.Text));
                        Save2Cell_List();
                    }
             });
        }

        /// <summary>
        /// 임시저장 불러오기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTempSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                Util.MessageValidation("SFU1843");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }

            string sArea = Util.NVC(cboArea.SelectedValue);
            if (sArea == string.Empty || sArea == "SELECT")
            {
                //동을 선택해주십시오.
                Util.MessageValidation("SFU1499");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            BOX001_007_TMP_CELL popUp = new BOX001_007_TMP_CELL();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[1] = Util.NVC(cboModelLot.SelectedValue);
                //  Parameters[2] = txtProdUser.Text;
                Parameters[2] = txtWorker.Text;
                Parameters[3] = sSHOPID;
                Parameters[4] = sAREAID;
                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(wndTempCell_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440 초기화 하시겠습니까?	
            Util.MessageConfirm("SFU3440", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    AllClear();
                }
            });
        }

        /// <summary>
        /// 실적 확인
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // 기본적인 Validation
            string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sLine == string.Empty || sLine == "SELECT")
            {
                //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                Util.MessageValidation("SFU1223");
             //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            string sModel = Util.NVC(cboModelLot.SelectedValue);
            if (sModel == string.Empty || sModel == "SELECT")
            {
                //모델을 선택해주십시오.
                Util.MessageValidation("SFU1257");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }

            if (string.IsNullOrEmpty(txtWorker.Text))
            {
                //작업자를 입력해 주세요
                Util.MessageValidation("SFU1843");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return;
            }

            // 리스트에 데이터가 있는지 확인
            if (dgCELLInfo.GetRowCount() <= 0)
            {
                //실적 확정할 데이터가 없습니다. 오른쪽 List를 확인하세요.
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3158"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3158", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.SelectAll();
                    }
                });
                return;
            }

            // 현재 스캔된 수량이 설정치보다 작을 경우
            if (isScanQty < int.Parse(txtSettingQty.Value.ToString()))
            {
                //설정한 수량과 스캔한 수량이 일치하지 않습니다.
                //그래도 확정 하시겠습니까?
                //[실적 확정시 수정이 불가합니다.]
                //sMessageCode = "SFU3444";

                Util.MessageValidation("SFU4965"); // 2019.08.28 추가. 설정 수량과 스캔 수량이 일치하지 않을 시 실적확정 불가.
                return;
            }

            // 현재 스캔된 수량이 설정치보다 작을 경우
            if (!ValidationCarrier(this.txtCarrierID.Text.Trim())) return;

            string sMessageCode = "SFU3156";   //실적 확정시 수정이 불가합니다.그래도 확정 하시겠습니까 ?

            //실적 확정시 수정이 불가합니다.그래도 확정 하시겠습니까 ?
            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3156"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm(sMessageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PopUpConfirm();
                }
            });
        }

        private void PopUpConfirm()
        {
            // 총 셀 수량
            int totalqty = int.Parse(txtScanqty.Text.Trim());

            // CELL 저장할 데이터테이블
            DataTable lsDataTable = new DataTable();
            lsDataTable.Columns.Add("PACKCELLSEQ", typeof(decimal));
            lsDataTable.Columns.Add("CELLID", typeof(string));
            lsDataTable.Columns.Add("PROD_KIND", typeof(string));      //[CSR ID:2487466]  개발품, 양산품 여부
            lsDataTable.Columns.Add("BOXID", typeof(string));
            lsDataTable.Columns.Add("LOTID", typeof(string));

            //[CSR ID:2640443] Box Label 사용일때..  
            //if (chkBoxLableYN.Checked == true)
            //{
            //    lsDataTable.Columns.Add("BOXID", typeof(string));
            //}

            //string sProd_Kind = (optProd_Type1.IsChecked == true ? "양산품" : "개발품"); //[CSR ID:2487466]  개발품, 양산품 여부

            for (int lCount = 0; lCount < dgCELLInfo.GetRowCount(); lCount++)
            {
                DataRow row = lsDataTable.NewRow();
                row["PACKCELLSEQ"] = Util.NVC_Decimal(dgCELLInfo.GetCell(lCount, dgCELLInfo.Columns["PACK_SEQ"].Index).Value);
                row["CELLID"] = Util.NVC(dgCELLInfo.GetCell(lCount, dgCELLInfo.Columns["SUBLOTID"].Index).Value);
                row["PROD_KIND"] = "LOT_TERM_CHK";          //[CSR ID:2487466]  개발품, 양산품 여부           
                row["BOXID"] = chkBoxLableYN.IsChecked == true ? Util.NVC(dgCELLInfo.GetCell(lCount, dgCELLInfo.Columns["BOXID"].Index).Value) : "";
                row["LOTID"] = Util.NVC(dgCELLInfo.GetCell(lCount, dgCELLInfo.Columns["LOTID"].Index).Value);
                ////row["BOXID"]        = ConvertUtil.ToDecimal(GridUtil.GetValue(sprCellInfo, lCount, 4).ToString()); ////CSR:2640443 - BOX

                lsDataTable.Rows.Add(row);
            }

            // 폼 화면에 보여주는 메서드에 5개의 매개변수 전달
            BOX001_251_CONFIRM popUp = new BOX001_251_CONFIRM();
            popUp.FrameOperation = this.FrameOperation;

            if (popUp != null)
            {
                object[] Parameters = new object[17];
                Parameters[0] = Util.NVC(cboModelLot.SelectedValue);
                Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[2] = Util.NVC(cboEquipmentSegment.Text);
                Parameters[3] = txtProdID.Text.Trim();
                Parameters[4] = txtBoxCellQty.Value.ToString();
                Parameters[5] = totalqty.ToString();
                Parameters[6] = lsDataTable;
                Parameters[7] = chkSkip.IsChecked == true ? "SKIP": "NO SKIP";
                Parameters[8] = Util.NVC(cboPackOut_Go.SelectedValue);
                Parameters[9] = ""; // txtWO.Text.Trim();
                Parameters[10] = sWO_DETL_ID;
                Parameters[11] = sSHOPID;
                Parameters[12] = sAREAID;
                //Parameters[13] = txtProdUser.Text;//cboProcUser.Text;
                //Parameters[14] = txtProdUser.Tag as string;//cboProcUser.Text;
                Parameters[13] = txtWorker.Text;//cboProcUser.Text;
                Parameters[14] = txtWorker.Tag as string;//cboProcUser.Text;
                Parameters[15] = bPilotChk ? "Y" : "N" ;
                Parameters[16] = txtCarrierID.Text.Trim();

                C1WindowExtension.SetParameters(popUp, Parameters);

                popUp.Closed += new EventHandler(wndConfirm_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => popUp.ShowModal()));
                grdMain.Children.Add(popUp);
                popUp.BringToFront();
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_251_CONFIRM wndPopup = sender as BOX001.BOX001_251_CONFIRM;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                // 초기화 함수 호출
                //AllClear();
                CompleteClear();

                DataTable dtPalletHisCard = wndPopup.retDT;

                if (dtPalletHisCard != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = UseCommoncodePlant() ? "PalletHis_Tag_UNCODE" : "PalletHis_Tag"; // "PalletHis_Tag";
                    Parameters[1] = dtPalletHisCard;
                    Parameters[2] = "2";
                    Parameters[4] = sSHOPID;

                    LGC.GMES.MES.BOX001.Report_Pallet_Hist rs = new LGC.GMES.MES.BOX001.Report_Pallet_Hist();
                    C1WindowExtension.SetParameters(rs, Parameters);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                    grdMain.Children.Add(rs);
                    rs.BringToFront();
                }
            }
            grdMain.Children.Remove(wndPopup);
        }

        private void wndTempCell_Closed(object sender, EventArgs e)
        {
            BOX001.BOX001_007_TMP_CELL wndPopup = sender as BOX001.BOX001_007_TMP_CELL;
            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                // 초기화 함수 호출
                //AllClear();

                string sJobDate = wndPopup.retJOBDATE;
                string sSaveSeq = wndPopup.retSAVE_SEQ;

                ArrayList aData = new ArrayList();
                aData.Add(sJobDate);
                aData.Add(sSaveSeq);

                AllClear();

                Search2TempMaster(aData);
                Search2TempSubLotList(aData);
                Search2TempLotList(aData);

                dgCELLInfo.ScrollIntoView(dgCELLInfo.GetRowCount() - 1, 0);
            }

            grdMain.Children.Remove(wndPopup);
        }
        
        private void cboPackOut_Go_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboPackOut_Go.SelectedValue == null)   //Form Load 시 이 문장 없으면 Error 발생
                return;

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
                    dr["SHOPID"] = sSHOPID;
                    dr["SHIPTO_ID"] = sCode;
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_PACK_OUTGO_BYOUTCOMP_CP", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count > 0)
                    {
                        lblPackOut_Go_LotTerm.Text = Util.NVC(dtResult.Rows[0]["PROD_DEVL_DATE"]); //+ "일 미만";
                    }

                    if (lblPackOut_Go_LotTerm.Text != "999")
                    {
                        chkLot_Term.IsEnabled = false;
                        chkLot_Term.IsChecked = true;
                    }
                    else
                    {
                        chkLot_Term.IsEnabled = true;
                        chkLot_Term.IsChecked = false;
                    }

                    //int iGM_Col = sCodeName.IndexOf("GM");
                    //int iGM_Etc = sCodeName.IndexOf("(기타)");

                    //if (iGM_Col >= 0 && iGM_Etc < 0)   //출고지가 GM이고 GM(기타)가 아닌 경우 무조건 양산품으로 선택되도록 함.
                    //{
                    //    //optProd_Type1.IsEnabled = false;
                    //    //optProd_Type2.IsEnabled = false;

                    //    //optProd_Type1.IsChecked = true;

                    //    chkLot_Term.IsEnabled = false;
                    //    chkLot_Term.IsChecked = true;
                    //}
                    //else
                    //{
                    //    //optProd_Type1.IsEnabled = true;
                    //    //optProd_Type2.IsEnabled = true;

                    //    chkLot_Term.IsEnabled = true;
                    //}
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

        private void txtBoxCellQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ReCheck_BoxID();  //Box번호 다시 계산하기 
            }
        }

        private void chkBoxLableYN_Click(object sender, RoutedEventArgs e)
        {
            if (chkBoxLableYN.IsChecked == true)
            {
                txtBoxCellQty.IsEnabled = true;
                dgCELLInfo.Columns["BOXID"].Visibility = Visibility.Visible;
                ReCheck_BoxID();
            }
            else
            {
                //txtBoxCellQty.Value = 0;
                txtBoxCellQty.IsEnabled = false;
                dgCELLInfo.Columns["BOXID"].Visibility = Visibility.Collapsed;
            }
        }

        private void cboProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //AllClear();

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
                    bErrChk = true;
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
            SetCHKSkipVisible();
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
        #endregion

        #region Mehod
        private void AllClear()
        {       
            dgCELLInfo.ItemsSource = null;
            dgLotInfo.ItemsSource = null;

            sWO_DETL_ID = string.Empty;
            //txtWO.Text = string.Empty;

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

            //if (cboModel.Items.Count > 0) cboModel.SelectedIndex = 0;
            if (cboPackOut_Go.Items.Count > 0) cboPackOut_Go.SelectedIndex = 0;
            txtScanqty.Text = string.Empty;

            chkSkip.IsChecked = false;
            //chk2D.IsChecked = false;
            //chk2DBCR.IsChecked = false;
            //chkBoxLableYN.IsChecked = false;

            txtCellID.Text = "";    // "JD12010102";
			txtCarrierID.Text = "";

            // DataTable 초기화
            isDataTable = new DataTable();
            isLotTable = new DataTable();

            // 스캔 카운트 초기화
            isScanQty = 0;

            lblLotTerm.Text = "";
            lblMax.Text = "";
            lblMin.Text = "";
            lblPackOut_Go_LotTerm.Text = "0";
            cboPackOut_Go.IsEnabled = true;
            chkLot_Term.IsChecked = false;
            //optProd_Type1.IsEnabled = true;
            //optProd_Type2.IsEnabled = true;
            //cboPackOut_Go.SelectedIndex = 0;

            // lot 편차 구하기 위한 변수 선언.
            sMINLOT = "";
            sMAXLOT = "";
            sMINDATE = "";
            sMAXDATE = "";

            cellCheck = false;
        }

        private void CompleteClear()
        {
            dgCELLInfo.ItemsSource = null;
            dgLotInfo.ItemsSource = null;

            sWO_DETL_ID = string.Empty;
            //txtWO.Text = string.Empty;

            txtScanqty.Text = string.Empty;

            txtCellID.Text = "";    // "JD12010102";

            // DataTable 초기화
            isDataTable = new DataTable();
            isLotTable = new DataTable();

            // 스캔 카운트 초기화
            isScanQty = 0;

            lblLotTerm.Text = "";
            lblMax.Text = "";
            lblMin.Text = "";
            cboPackOut_Go.IsEnabled = true;
            
            // lot 편차 구하기 위한 변수 선언.
            sMINLOT = "";
            sMAXLOT = "";
            sMINDATE = "";
            sMAXDATE = "";

            cellCheck = false;
        }

        /// <summary>
        /// 임시 저장 마스터 불러오기
        /// </summary>
        /// <param name="aData"></param>
        private void Search2TempMaster(ArrayList aData)
        {
            if (aData.Count != 2)
                return;

            string sJobDate = aData[0].ToString();
            string sSave_Seq = aData[1].ToString();

            //BizData data = new BizData("QR_TEMP_SAVE_CELL_DATA", "RSLTDT");
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("SAVE_SEQ", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sAREAID;
                dr["PACK_TMP_TYPE_CODE"] = "PACK_CELL";
                dr["SAVE_SEQ"] = sSave_Seq;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CELL_PACK_TMP_SAVE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count <= 0)
                {
                    // 조회된 결과가 없을 경우 함수 종료
                    isScanQty = 0;
                    return;
                }
                else
                {
                    cboEquipmentSegment.SelectedValue = Util.NVC(dtResult.Rows[0]["EQSGID"]);
                    cboModelLot.SelectedValue = Util.NVC(dtResult.Rows[0]["MDLLOT_ID"]);
                    //cboProduct.SelectedValue = Util.NVC(dtResult.Rows[0]["PRODID"]);
                    cboPackOut_Go.SelectedValue = Util.NVC(dtResult.Rows[0]["SHIPTO_ID"]);
                    //cboProcUser.Text = Util.NVC(dtResult.Rows[0]["INSUSER"]);

                    //if (dtResult.Rows[0]["MP_FLAG"].ToString().Equals("Y"))
                    //    optProd_Type1.IsChecked = true;
                    //else
                    //    optProd_Type2.IsChecked = true;

                    chkSkip.IsChecked = (dtResult.Rows[0]["INSP_SKIP_FLAG"].ToString().Equals("Y") ? true : false);
                    chk2D.IsChecked = (dtResult.Rows[0]["BCD_2D_SKIP_FLAG"].ToString().Equals("Y") ? true : false);

                    //txtSettingQty.Value = Util.NVC_Int(dtResult.Rows[0]["TOTL_CELL_QTY"]);

                    //txtScanqty.Text = dtResult.Rows.Count.ToString();

                    txtScanqty.Text = Util.NVC(dtResult.Rows[0]["SCAN_QTY"]);

                    isScanQty = Util.NVC_Int(dtResult.Rows[0]["SCAN_QTY"]);

                    txtSettingQty.Value = Util.NVC_Int(dtResult.Rows[0]["TOTL_CELL_QTY"]);
                    //ReCheck_LotTerm();

                    txtCellID.Focus();
                    txtCellID.SelectAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// SUBLOT LIST 임시 저장 불러오기
        /// </summary>
        /// <param name="aData"></param>
        private void Search2TempSubLotList(ArrayList aData)
        {
            if (aData.Count != 2)
                return;

            string sJobDate = aData[0].ToString();
            string sSave_Seq = aData[1].ToString();

            //BizData data = new BizData("QR_TEMP_SAVE_CELL_LOT_DATA", "RSLTDT");

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SAVE_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SAVE_SEQNO"] = sSave_Seq;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CELL_PACK_TMP_SAVE_DETL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count <= 0)
                {
                    // 조회된 결과가 없을 경우 함수 종료
                    isScanQty = 0;
                    return;
                }
                else
                {
                    //for (int i = 0; i < dtResult.Rows.Count; i++)
                    //{
                    //    txtCellID.Text = Util.NVC(dtResult.Rows[i]["SUBLOTID"]);
                    //    Add_Scan_Cellid();
                    //}

                    isDataTable = dtResult;
                    if (isDataTable.Columns.Contains("BOXID") == false)
                    {
                        isDataTable.Columns.Add("BOXID", typeof(string));
                    }

                    ////dgCELLInfo.ItemsSource = DataTableConverter.Convert(isDataTable);
                    Util.GridSetData(dgCELLInfo, isDataTable, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
             //   LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// LOT LIST 임시 저장 불러오기
        /// </summary>
        /// <param name="aData"></param>
        private void Search2TempLotList(ArrayList aData)
        {
            if (aData.Count != 2)
                return;

            string sJobDate = aData[0].ToString();
            string sSave_Seq = aData[1].ToString();

            //BizData data = new BizData("QR_TEMP_SAVE_CELL_LOT_DATA", "RSLTDT");

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SAVE_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SAVE_SEQNO"] = sSave_Seq;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CELL_PACK_TMP_SAVE_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count <= 0)
                {
                    // 조회된 결과가 없을 경우 함수 종료
                    isScanQty = 0;
                    return;
                }
                else
                {
                    ////dgLotInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.GridSetData(dgLotInfo, dtResult, FrameOperation,true);
                    isLotTable = dtResult;
                    if (isLotTable.Columns.Contains("LOTID") == false)
                    {
                        //데이터 컬럼 정의
                        isLotTable.Columns.Add("EQSGNAME", typeof(string));
                        isLotTable.Columns.Add("LOTID", typeof(string));
                        isLotTable.Columns.Add("QTY", typeof(string));
                    }

                    //Search2LotTerm(dtResult.Rows[0]["LOTID"].ToString(), dtResult.Rows[dtResult.Rows.Count-1]["LOTID"].ToString());
                    ReCheck_LotTerm();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행  [CSR ID:2642448] 
        /// 2D barcode 를 읽어 CellID를 리턴함
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private string SelectCellID(string sBCR, out string msg)
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
                inData["AREAID"] = sAREAID;
                inDataTable.Rows.Add(inData);

                DataRow inDataCell = inDataCellTable.NewRow();
                inDataCell["BCR"] = sBCR;
                inDataCell["CELL_ID"] = sCellID;
                inDataCellTable.Rows.Add(inDataCell);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_INF_SEL_2DBCR_BY_CELL", "INDATA_ROUTE,INDATA", "OUTDATA", indataSet);

                msg = "";
                return dsResult.Tables["OUTDATA"].Rows[0]["CELL_ID"].ToString();
            }
            catch (Exception ex)
            {
                msg = MessageDic.Instance.GetMessage("SFU3248", new object[] { sBCR }) + System.Environment.NewLine + ex.Message;
                // msg = "입력한 [" + sBCR + "] 2D 바코드의 CELL ID 정보가 없습니다." + Convert.ToString((char)13) + Convert.ToString((char)10) + ex.Message;
                return "";
            }
        }

        /// <summary>
        /// 활성화 BizActor에 연결해서 로직 수행한 후 다시 전극/조립 BizActor로 연결하는 함수
        /// CellID를 매개변수로 하여 특성치 정보를 확인. NG판정을 하고 OutData 로 조립LotID 를 리턴함
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private string SelectAssyLotID(string cellID, string skip,out string msg)
        {
            // SET_SHIPMENT_CELL_INFO_v02
            try
            {
                msg = "";
                string sBCR_Check = chk2D.IsChecked == true ? "N" : "Y";
                string sModelID = cboModelLot.SelectedValue.ToString();
                string sLineID = cboEquipmentSegment.SelectedValue.ToString();
                string sPRODID = txtProdID.Text;
                //DataSet indataSet_chk = new DataSet();
                //DataTable inDataTable_chk = indataSet_chk.Tables.Add("INDATA");
                //inDataTable_chk.Columns.Add("AREAID", typeof(string));
                //inDataTable_chk.Columns.Add("EQSGID", typeof(string));
                //inDataTable_chk.Columns.Add("SUBLOT_TYPE", typeof(string));
                //inDataTable_chk.Columns.Add("MDLLOT_ID", typeof(string));

                //DataTable inDataCellTable_chk = indataSet_chk.Tables.Add("INSUBLOT");
                //inDataCellTable_chk.Columns.Add("SUBLOTID", typeof(string));

                //DataRow inData_chk = inDataTable_chk.NewRow();
                //inData_chk["AREAID"] = sAREAID;
                //inData_chk["EQSGID"] = sLineID;
                //inData_chk["SUBLOT_TYPE"] = "CELL";
                //inData_chk["MDLLOT_ID"] = sModelID;
                //inDataTable_chk.Rows.Add(inData_chk);

                //DataRow inDataCell_chk = inDataCellTable_chk.NewRow();
                //inDataCell_chk["SUBLOTID"] = cellID;
                //inDataCellTable_chk.Rows.Add(inDataCell_chk);

                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SUBLOT", "INDATA,INSUBLOT", "", indataSet_chk);

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
                    RQSTDT.Columns.Add("PILOT_MODE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["SUBLOTID"] = cellID;
                    dr["SHOPID"] = sSHOPID;
                    dr["AREAID"] = sAREAID;
                    dr["EQSGID"] = sLineID;
                    dr["PRODID"] = sPRODID;
                    dr["MDLLOT_ID"] = sModelID;
                    dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                    dr["INSP_SKIP_FLAG"] = chkSkip.IsChecked == true ? "Y" : "N";
                    dr["2D_BCR_SKIP_FLAG"] = chk2D.IsChecked == true ? "Y" : "N";
                    dr["USERID"] = txtWorker.Tag.ToString();
                    dr["PILOT_MODE"] = bPilotChk ? "Y" : "N";

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
                    inData["USERID"] = "";
                    inData["EQSGID"] = sLineID;
                    inData["MODELID"] = sModelID;
                    inDataTable.Rows.Add(inData);

                    DataRow inDataCell = inDataCellTable.NewRow();
                    inDataCell["SUBLOTID"] = cellID;
                    inDataCellTable.Rows.Add(inDataCell);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_PACKING_CELL_FCS", "IN_EQP,IN_SUBLOT", "OUT_SUBLOT", indataSet);

					string strReturn = Util.NVC(dsResult.Tables["OUT_SUBLOT"].Rows[0]["LOTID"]);
					return strReturn;
				}
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);

                // 2019.10.17 수정. SelectAssyLotID()에서 exception 발생 시 null return 하지 않고 throw ex로 변경.
                //msg = ex.Message;
                //return null;
                throw ex;
            }   
        }

        /// <summary>
        /// Lot 편차 재 조회.(Grid에서 Cell 삭제 시.)
        /// </summary>
        private void ReCheck_LotTerm()   
        {
            try
            {
                string sMin = "";
                string sMax = "";

                string sTmpDate = "";
                string sMinDate = "";
                string sMaxDate = "";

                lblMin.Text = "";
                lblMax.Text = "";
                lblLotTerm.Text = "";

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[i].DataItem, "PACK_SEQ", (i + 1).ToString());                  
                }

                for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                {
                    string sLotID = Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value);

                    //LOT 편차를 구하기 위한 CELLID 의 생성일자 구하기
                    sTmpDate = GetLotCreatDateByLot(sLotID);

                    if (i > 0)
                    {
                        int iMin = string.Compare(sMinDate, sTmpDate);
                        int iMax = string.Compare(sTmpDate, sMaxDate);

                        sMin = (iMin > 0 ? sLotID : sMin);
                        sMax = (iMax > 0 ? sLotID : sMax);

                        sMinDate = (iMin > 0 ? sTmpDate : sMinDate);
                        sMaxDate = (iMax > 0 ? sTmpDate : sMaxDate);

                        if (iMin > 0 || iMax > 0)
                        {
                            Search2LotTerm(sMin, sMax);
                        }
                    }
                    else
                    {
                        sMin = sLotID;
                        sMax = sLotID;
                        sMinDate = sTmpDate;
                        sMaxDate = sTmpDate;
                        Search2LotTerm(sMin, sMax);
                    }
                }

                if (dgCELLInfo.GetRowCount() <= 1)
                {
                    cboPackOut_Go.IsEnabled = true;
                    chkLot_Term.IsEnabled = true;

                    if (dgCELLInfo.GetRowCount() == 0)
                    {
                        lblMin.Text = "";
                        lblMax.Text = "";
                        lblLotTerm.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Lot 편찬 관련 추가 함수
        /// </summary>
        /// <returns></returns>
        private bool Check2LotTerm(string sLotID, string sCellID) //Lot 편차 구하는 함수
        {
            try
            {
                //string sLotID = txtCellID.Text.Substring(0, 4);
                
                //if (int.Parse(sLotID.Substring(2, 2)) > 31)
                //{
                //    return true;
                //}

                string sTmpDate = GetLotCreatDateBySubLot(sCellID);

                //if (isScanQty == 0)
                if (dgCELLInfo.GetRowCount() == 0)
                {
                    //lblMin.Text = sLotID;
                    //lblMax.Text = sLotID;
                    //lblLotTerm.Text = "1";
                    sMINLOT = sLotID;
                    sMAXLOT = sLotID;
                    Search2LotTerm(sMINLOT, sMAXLOT);

                    return true;
                }
                else
                {
                    // 두번째 Scan부터 사용자의 오동작을 예방하기 위해 선택 못하도록 함.
                    cboPackOut_Go.IsEnabled = false;
                    //optProd_Type1.IsEnabled = false;
                    //optProd_Type2.IsEnabled = false;
                    chkLot_Term.IsEnabled = false;

                    //if (lblMin.Text.Equals(""))   //1번째 Scan한 Cell의 Lot이 Test Lot인 경우를 대비하기 위함.
                    //    lblMin.Text = sLotID;

                    //int iMin = string.Compare(lblMin.Text, sLotID);
                    //int iMax = string.Compare(sLotID, lblMax.Text);

                    //sMin = (iMin > 0 ? sLotID : lblMin.Text);       // 새로 들어온 lot의 값이 기존 min값 보다 작은 경우
                    //sMax = (iMax > 0 ? sLotID : lblMax.Text);       // 새로 들어온 lot의 값이 기존 max값 보다 큰 경우

                    int iMin = string.Compare(sMINDATE, sTmpDate);
                    int iMax = string.Compare(sTmpDate, sMAXDATE);

                    sMINLOT = (iMin > 0 ? sLotID : sMINLOT);
                    sMAXLOT = (iMax > 0 ? sLotID : sMAXLOT);

                    sMINDATE = (iMin > 0 ? sTmpDate : sMINDATE);
                    sMAXDATE = (iMax > 0 ? sTmpDate : sMAXDATE);

                    if (iMin > 0 || iMax > 0)    // min, max값이 변동 된 경우 
                    {
                        return Search2LotTerm(sMINLOT, sMAXLOT);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return false;
            }
        }

        private bool Search2LotTerm(string sMinLot, string sMaxLot)   //Lot편차 조회 함수
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

                    //if (optProd_Type1.IsChecked == true)
                    if (chkLot_Term.IsChecked == true)
                    {
                        //int iCol = lblPackOut_Go_LotTerm.Text.LastIndexOf("일 미만");
                        int iLotTerm = Util.NVC_Int(lblPackOut_Go_LotTerm.Text);

                        if (int.Parse(sLotTerm) > iLotTerm)
                        {
                            //Lot편차 값이 설정된 값 {%1}일 보다 큽니다
                            //string sMsg = "SFU3247", Lot편차 값이 설정된 값 [" + iLotTerm.ToString() + "]일 보다 큽니다";
                            // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                            Util.MessageInfo("SFU3247", iLotTerm.ToString());                          
                            txtCellID.Focus();
                            txtCellID.SelectAll();
                            //bErrChk = true;
                            return false;
                        }
                    }

                    lblLotTerm.Text = sLotTerm;
                    lblMin.Text = sMinLot;
                    lblMax.Text = sMaxLot;
                    sMINLOT = sMinLot;
                    sMAXLOT = sMaxLot;
                    sMINDATE = dtResult.Rows[0]["MIN_PRODDATE"].ToString();
                    sMAXDATE = dtResult.Rows[0]["MAX_PRODDATE"].ToString();
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return false;
            }
        }

        /// <summary>
        /// 임시저장
        /// </summary>
        /// <param name="sUserName"></param>
        private void Save2Cell_List()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("MDLLOT_ID", typeof(string));
                inDataTable.Columns.Add("WOID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("MP_FLAG", typeof(string));
                inDataTable.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                inDataTable.Columns.Add("BCD_2D_SKIP_FLAG", typeof(string));
                inDataTable.Columns.Add("TOTL_CELL_QTY", typeof(decimal));
                inDataTable.Columns.Add("PACK_TMP_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inDataDtlTable = indataSet.Tables.Add("INSUBLOT");
                inDataDtlTable.Columns.Add("SUBLOTID", typeof(string));
                inDataDtlTable.Columns.Add("PACK_SEQ", typeof(decimal));

                //Data
                //string sGD_PROD_YN = (optProd_Type1.IsChecked == true ? "Y" : "N");
                string sGD_PROD_YN = (chkLot_Term.IsChecked == true ? "Y" : "N");
                string sCOND_SKIP_YN = (chkSkip.IsChecked == true ? "Y" : "N");
                string sBCR_SKIP_YN = (chk2D.IsChecked == true ? "Y" : "N");

                DataRow inData = inDataTable.NewRow();
                inData["AREAID"] = sAREAID;
                inData["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                inData["MDLLOT_ID"] = cboModelLot.SelectedValue.ToString();
                inData["WOID"] = null;   // txtWO.Text.Trim();
                inData["PRODID"] = txtProdID.Text.Trim();
                inData["SHIPTO_ID"] = cboPackOut_Go.SelectedValue.ToString();
                inData["MP_FLAG"] = sGD_PROD_YN;
                inData["INSP_SKIP_FLAG"] = sCOND_SKIP_YN;
                inData["BCD_2D_SKIP_FLAG"] = sBCR_SKIP_YN;
                inData["TOTL_CELL_QTY"] = txtSettingQty.Value;
                inData["PACK_TMP_TYPE_CODE"] = "PACK_CELL";
                //    inData["USERID"] = txtProdUser.Text;
                inData["USERID"] = txtWorker.Text;
                inDataTable.Rows.Add(inData);

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    string sCellID = Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SUBLOTID"].Index).Value);
                    string sPackCellSeq = (i + 1).ToString();

                    DataRow inDataDtl = inDataDtlTable.NewRow();
                    inDataDtl["SUBLOTID"] = sCellID;
                    inDataDtl["PACK_SEQ"] = sPackCellSeq;
                    inDataDtlTable.Rows.Add(inDataDtl);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TMP_PACK_CELL", "INDATA,INSUBLOT", null, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
               // Util.AlertByBiz("BR_PRD_REG_TMP_PACK_CELL", ex.Message, ex.ToString());
            }
        }

        /// <summary>
        /// Box번호 다시 계산하기 
        /// </summary>
        private void ReCheck_BoxID()
        {
            try
            {
                if (txtBoxCellQty.Value.ToString() == "NaN")
                {
                    Util.AlertInfo("SFU1316"); //"BOX내CELL수량을 입력하세요"
                    return;
                }

                double boxCount = 0;
                double nBoxCellQty = txtBoxCellQty.Value;   //Box내 Cell 수량

                for (int lsCount = 0; lsCount < dgCELLInfo.GetRowCount(); lsCount++)
                {
                    //2015.04.01 심준택K    [CSR ID:2683612]  BoxID 순서 계산하기
                    boxCount = Math.Ceiling(Convert.ToDouble(lsCount + 1) % nBoxCellQty);
                    if (boxCount == 0) boxCount = nBoxCellQty;

                    //CSR:2640443 - BoxID 계산하기
                    string sBoxID = Convert.ToString(Math.Ceiling(Convert.ToDouble(lsCount + 1) / nBoxCellQty)) + "-" + Convert.ToString(boxCount);
                    if (sBoxID == "") sBoxID = string.Empty;

                    DataTableConverter.SetValue(dgCELLInfo.Rows[lsCount].DataItem, "BOXID", sBoxID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Cell ID 스캔시 검증후 Cell List에 추가
        /// </summary>
        private bool Add_Scan_Cellid(string sCellId)
        {
            if (string.IsNullOrWhiteSpace(txtWorker.Text))
            {
                //작업자를 선택해 주세요
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1843"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);

                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            string sArea = Util.NVC(cboArea.SelectedValue);

            // MagazineID 공백 or 널값 여부 확인
            if (string.IsNullOrWhiteSpace(sCellId))
            {
                // CELLID를 스캔 또는 입력하세요.
               // Util.MessageValidation("SFU1323");

                Util.MessageValidation("SFU1323", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.SelectAll();
                    }
                });

                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            if (sCellId.Length < 10)
            {
                // CELL ID 길이가 잘못 되었습니다.
               // Util.MessageValidation("SFU1318");
                Util.MessageValidation("SFU1318", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.SelectAll();
                    }
                });

                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            // Scan시 앞두자리 문자가 "ZZ"이면 Temp Tag Scan
            string sData = sCellId;

            if (sData.Substring(0, 2).Equals("ZZ"))
            {
                if (sArea == string.Empty || sArea == "SELECT")
                {
                    //동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    Util.WarningPlayer();
                    bErrChk = true;
                    return false;
                }

                //if (sData.Length != 11)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("잘못된 ID를 Scan 했습니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                //    return;
                //}
                ArrayList aData = new ArrayList();

                aData.Add(sData.Substring(2, 8));

                //aData.Add(sData.Substring(10, 1));
                aData.Add(sData.Substring(10));

                Search2TempMaster(aData);
                Search2TempSubLotList(aData);
                Search2TempLotList(aData);       // 2015.04.01  심준택 [CSR ID:2683612]

                dgCELLInfo.ScrollIntoView(dgCELLInfo.GetRowCount() - 1, 0);
                txtCellID.Text = "";
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            // 기본적인 Validation
            if (sArea == string.Empty || sArea == "SELECT")
            {
                //동을 선택해주십시오.
                Util.MessageValidation("SFU1499");
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1499"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            string sLine = Util.NVC(cboEquipmentSegment.SelectedValue);
            if (sLine == string.Empty || sLine == "SELECT")
            {
                //"라인을 선택해주십시오." >> 라인을 선택해 주세요.
                Util.MessageValidation("SFU1223");
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1223"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            string sModel = Util.NVC(cboModelLot.SelectedValue);
            if (sModel == string.Empty || sModel == "SELECT")
            {
                //모델을 선택해주십시오.
                Util.MessageValidation("SFU1257");
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1257"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            string sProduct = Util.NVC(cboProduct.SelectedValue);
            if (sProduct == string.Empty || sProduct == "SELECT")
            {
                //제품을 선택해주십시오.
                Util.MessageValidation("SFU1895");
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1895"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            //if (string.IsNullOrEmpty(txtWO.Text))
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업지시를 먼저 선택해주십시오."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
            //    bErrChk = true;
            //    return;
            //}

            if (cboPackOut_Go.SelectedValue.ToString().Equals("") || cboPackOut_Go.SelectedValue.Equals("SELECT"))
            {
                //출하처를 선택하십시오.
                Util.MessageValidation("SFU3173");
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3173"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            if (string.IsNullOrEmpty(txtSettingQty.Value.ToString()) || txtSettingQty.Value.ToString() == "NaN")
            {
                //스캔 설정치를 먼저 입력해주세요.
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3154"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
              Util.MessageConfirm("SFU3154", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSettingQty.Focus();
                    }
                });
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            // 현재 스캔된 수량이 설정치보다 클 경우
            if (isScanQty >= int.Parse(txtSettingQty.Value.ToString()))
            {
                //설정치를 초과하였습니다. 설정치를 변경하시거나 스캔을 중지해주세요.
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3152"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU3152", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSettingQty.Focus();
                    }
                });
                Util.WarningPlayer();
                bErrChk = true;
                return false;
            }

            // 2D Barcode를 이용한 출하구성 件 : 2D 값으로 CELL ID 정보 조회
            string msg1 = "";
            if (chk2DBCR.IsChecked == true)
            {
                if (sCellId.Trim().Length > 60)
                {
                    sData = SelectCellID(sCellId.Trim(), out msg1);

                    if (sData.ToString() == "")
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(msg1), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //Util.MessageInfo(msg1, (result) =>
                         {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                            }
                        });
                        Util.WarningPlayer();
                        bErrChk = true;
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
            if (dgCELLInfo.GetRowCount() > 0)
            {
                // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    if ((Util.NVC(dgCELLInfo.GetCell(i, dgCELLInfo.Columns["SUBLOTID"].Index).Value) == sCellId))
                    {
                        //오른쪽 List에 이미 존재하는 Cell ID입니다.
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3161"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        Util.DingDongPlayer();

                        Util.MessageInfo("SFU3161", (result) =>
                       {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellID.Focus();
                                txtCellID.SelectAll();
                            }
                        });
                        bErrChk = true;
                        return false;
                    }
                }
            }

            try
            {
                if (isDataTable.Columns.Count <= 0)
                {
                    // 데이터 컬럼 정의
                    isDataTable.Columns.Add("PACK_SEQ", typeof(string));
                    isDataTable.Columns.Add("SUBLOTID", typeof(string));
                    isDataTable.Columns.Add("LOTID", typeof(string));
                    isDataTable.Columns.Add("BOXID", typeof(string));
                }

                DataRow row = isDataTable.NewRow();
                row["SUBLOTID"] = sCellId;                            // 현재 스캔된 CELL ID

                string msg = "";
                string skip = "";

                if (chkSkip.IsChecked == true)
                {
                    skip = "Y";
                    cellCheck = true;
                }
                else
                {
                    skip = "N";
                    //cellCheck = false;
                }

                // CELL ID 의 조립 LOTID 가져옴과 동시에 활성화 특성치 데이터 조회 함수 호출
                row["LOTID"] = SelectAssyLotID(sCellId, skip,out msg);

                // 2019.10.17 수정. SelectAssyLotID()에서 exception 발생 시 null return 하지 않고 throw ex로 변경.
                //if (row["LOTID"].ToString() == "")
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(msg, null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            txtCellID.Focus();
                //            txtCellID.SelectAll();
                //        }
                //    });
                //    Util.WarningPlayer();
                //    bErrChk = true;
                //    return false;
                //}

                if (!Check2LotTerm(row["LOTID"].ToString(), sCellId))
                    return false;

                string templot = row["LOTID"].ToString();

                // 스프레드에 데이터 바인딩
                isDataTable.Rows.Add(row);
                ////dgCELLInfo.ItemsSource = DataTableConverter.Convert(isDataTable);
                Util.GridSetData(dgCELLInfo, isDataTable, FrameOperation, true);
                //
                isScanQty = isScanQty + 1;
                txtScanqty.Text = isScanQty.ToString();

                // 스프레드의 색상 변경을 위해서 필요한 변수

                int colorCount = 0;
                double boxCount = 0;
                double nBoxCellQty = txtBoxCellQty.Value;   //CSR:2640443 - Box내 Cell 수량

                for (int i = 0; i < dgCELLInfo.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCELLInfo.Rows[i].DataItem, "PACK_SEQ", Convert.ToString(i + 1));

                    //2015.04.01 심준택K    [CSR ID:2683612]  BoxID 순서 계산하기
                    boxCount = Math.Ceiling(Convert.ToDouble(i + 1) % nBoxCellQty);
                    if (boxCount == 0) boxCount = nBoxCellQty;

                    //CSR:2640443 - BoxID 계산하기
                    string sBoxID = Convert.ToString(Math.Ceiling(Convert.ToDouble(i + 1) / nBoxCellQty)) + "-" + Convert.ToString(boxCount);
                    DataTableConverter.SetValue(dgCELLInfo.Rows[i].DataItem, "BOXID", sBoxID);

                    // for문의 카운트와는 별개의 카운트
                    //   rowCount = rowCount + 1;


                    // rowCount 가 20이 될 때마다 아래 로직 수행 : 스프레드 색상 변경
                    //if (rowCount == 20)
                    //{
                    //    isIndex = i;

                    //    Color color = new Color();

                    //    if (colorCount == 0)
                    //    {
                    //        //color = Color.AliceBlue;
                    //        colorCount = 1;
                    //    }
                    //    else
                    //    {
                    //        //color = Color.AntiqueWhite;
                    //        colorCount = 0;
                    //    }

                    //    //
                    //    //for (int forCount = 19; forCount >= 0; forCount--)
                    //    //{
                    //    //    //sprCellInfo.ActiveSheet.Cells[isIndex - forCount, 0].BackColor = color;
                    //    //    //sprCellInfo.ActiveSheet.Cells[isIndex - forCount, 1].BackColor = color;
                    //    //    //sprCellInfo.ActiveSheet.Cells[isIndex - forCount, 2].BackColor = color;
                    //    //}

                    //    // 색상 변경했으니 카운트 다시 초기화.
                    //    rowCount = 0;
                    //}
                }

                //// 2015.04.01 심준택K    [CSR ID:2683612] 
                //if (isLotTable.Columns.Contains("LOTID") == false)
                //{
                //    //데이터 컬럼 정의
                //    isLotTable.Columns.Add("EQSGNAME", typeof(string));
                //    isLotTable.Columns.Add("LOTID", typeof(string));
                //    isLotTable.Columns.Add("QTY", typeof(string));
                //}

                //DataRow row1 = isLotTable.NewRow();
                //Boolean chk = false;
                //if (dgLotInfo.GetRowCount() > 0)
                //{
                //    // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                //    for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                //    {
                //        if (Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value) == templot)
                //        {
                //            int lotqty = Util.NVC_Int(dgLotInfo.GetCell(i, dgLotInfo.Columns["QTY"].Index).Value) + 1;
                //            DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "QTY", lotqty.ToString());

                //            //화면에 반영된 내용을 DataTable에 다시 반영함.
                //            isLotTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());
                //            chk = true;
                //        }
                //    }
                //    if (chk == false)   // 없는 경우 추가
                //    {
                //        // 스프레드에 데이터 바인딩                        
                //        isLotTable.Rows.Add(row1);
                //        row1["LOTID"] = templot;
                //        row1["QTY"] = "1";
                //    }
                //}
                //else   // 처음인 경우 추가
                //{
                //    // 스프레드에 데이터 바인딩                        
                //    isLotTable.Rows.Add(row1);
                //    row1["LOTID"] = templot;
                //    row1["QTY"] = "1";

                //}

                //////dgLotInfo.ItemsSource = DataTableConverter.Convert(isLotTable);
                //Util.GridSetData(dgLotInfo, isLotTable, FrameOperation, true);
                // 스캔된 마지막 셀이 바로 보이도록 스프레드 스크롤 하단으로 이동
                dgCELLInfo.ScrollIntoView(dgCELLInfo.GetRowCount() - 1, 0);
                //sprCellInfo.ActiveSheet.ActiveRowIndex = sprCellInfo_Sheet1.Rows.Count - 1;
                //sprCellInfo.ActiveSheet.ActiveRow.Invalidate();
                //sprCellInfo.ShowActiveCell(VerticalPosition.Top, HorizontalPosition.Nearest);
                //sprCellInfo.ShowRow(sprCellInfo_Sheet1.Rows.Count - 1, sprCellInfo_Sheet1.Rows.Count - 1, VerticalPosition.Top);

                // 정상적인 경우 다음 Cell 입력할 수 있게 전체 선택.
                txtCellID.Focus();
                txtCellID.SelectAll();
                bErrChk = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                bErrChk = true;
                // PlayWarnSound();
                Util.WarningPlayer();
                return false;
            }
            finally
            {
                txtCellID.Focus();
                txtCellID.SelectAll();

                if (isScanQty > 0 && isScanQty >= int.Parse(txtSettingQty.Value.ToString()))
                {
                    txtCarrierID.Focus();
                    txtCarrierID.SelectAll();
                }
            }

            return true;
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
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return "";
            }
        }

        /// <summary>
        /// Cell 로 조립LOT의 생성일자 가져오기
        /// </summary>
        /// <param name="sCellid"></param>
        /// <returns></returns>
        private string GetLotCreatDateBySubLot(string sCellid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = sCellid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE_BY_SUBLOT_CP", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (isLotTable.Columns.Contains("LOTID") == false)
                    {
                        //데이터 컬럼 정의
                        isLotTable.Columns.Add("EQSGNAME", typeof(string));
                        isLotTable.Columns.Add("LOTID", typeof(string));
                        isLotTable.Columns.Add("QTY", typeof(string));
                    }
                    
                    DataRow row1 = isLotTable.NewRow();
                    Boolean chk = false;
                    if (dgLotInfo.GetRowCount() > 0)
                    {
                        // 스프레드에 있는 값과 동일한 값이 입력되었다면 return됨.
                        for (int i = 0; i < dgLotInfo.GetRowCount(); i++)
                        {
                            if (Util.NVC(dgLotInfo.GetCell(i, dgLotInfo.Columns["LOTID"].Index).Value) == dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8))
                            {
                                int lotqty = Util.NVC_Int(dgLotInfo.GetCell(i, dgLotInfo.Columns["QTY"].Index).Value) + 1;
                                DataTableConverter.SetValue(dgLotInfo.Rows[i].DataItem, "QTY", lotqty.ToString());

                                //화면에 반영된 내용을 DataTable에 다시 반영함.
                                isLotTable = DataTableConverter.Convert(dgLotInfo.GetCurrentItems());
                                chk = true;
                            }
                        }
                        if (chk == false)   // 없는 경우 추가
                        {
                            // 스프레드에 데이터 바인딩                        
                            isLotTable.Rows.Add(row1);
                            row1["EQSGNAME"] = dtResult.Rows[0]["EQSGNAME"].ToString();
                            row1["LOTID"] = dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8);
                            row1["QTY"] = "1";
                        }
                    }
                    else   // 처음인 경우 추가
                    {
                        // 스프레드에 데이터 바인딩                        
                        isLotTable.Rows.Add(row1);
                        row1["EQSGNAME"] = dtResult.Rows[0]["EQSGNAME"].ToString();
                        row1["LOTID"] = dtResult.Rows[0]["PROD_LOTID"].ToString().Substring(0, 8);
                        row1["QTY"] = "1";
                    }

                    ////dgLotInfo.ItemsSource = DataTableConverter.Convert(isLotTable);
                    Util.GridSetData(dgLotInfo, isLotTable, FrameOperation, true);
                }

                return Util.NVC(dtResult.Rows[0]["CALDATE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
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
            
            // 활성화 시스템 일 때,
            if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_FORM");
            }
            // 조립 시스템 일 때,
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LINE_CP");
            }
            
            //출하처 Combo Set.
            //string[] sFilter3 = { sSHOPID };
            //_combo.SetCombo(cboPackOut_Go, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");

            //작업자 Combo Set.
            //String[] sFilter4 = { sSHOPID, sAREAID, Process.CELL_BOXING };
            //_combo.SetCombo(cboProcUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter4, sCase: "PROC_USER");
            //txtProdUser.Text = LoginInfo.USERNAME;
        }

        string sEQSGID = null;
        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(cboEquipmentSegment.SelectedValue) != sEQSGID)
                sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue);
            else
                return;

            if (sEQSGID == string.Empty || sEQSGID == "SELECT")
            {
                sEQSGID = null;
                return;
            }

            C1ComboBox[] cboParent = { cboEquipmentSegment };
            _combo.SetCombo(cboModelLot, CommonCombo.ComboStatus.SELECT, cbParent: cboParent);

            if (sEQSGID != null)
            { 
                string[] sFilter3 = { sSHOPID, sEQSGID, sAREAID };
                _combo.SetCombo(cboPackOut_Go, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
            }
            
            //cboProduct.SelectedItem = null;
        }

        private void cboModelLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sModelLot = Util.NVC(cboModelLot.SelectedValue);
            if (sModelLot == "" || sModelLot == "SELECT")
            {
                sModelLot = "";
                txtSettingQty.Value = 0;
            }
            else
            {
                C1ComboBox[] cboParent2 = { cboEquipmentSegment, cboModelLot };
                String[] sFilter = { sModelLot };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.NONE, cbParent: cboParent2, sFilter: sFilter, sCase: "PROD_MDL");

                // Auto Scan 설정치 추가
                AutoScanSetting();
            }
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

        private void txtCellID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    bool bFlag = false;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && (bFlag = Add_Scan_Cellid(sPasteStrings[i].Trim())) == false)
                        {
                            break;
                        }                      
                        System.Windows.Forms.Application.DoEvents();
                    }
                    if (bFlag) Util.DingPlayer();
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

        private void dgCELLInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    double nBoxCellQty = txtBoxCellQty.Value;

                    int rowCount = ((int)e.Cell.Row.Index / (int)nBoxCellQty);
                    if (rowCount % 2 == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#f5f5f5"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#e8ebed"));
                    }
                }));
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

        private void chkSkip_Checked(object sender, RoutedEventArgs e)
        {
            chkLot_Term.IsEnabled = true;
            chkLot_Term.IsChecked = false;
        }

        private void chkSkip_Unchecked(object sender, RoutedEventArgs e)
        {
            if (lblPackOut_Go_LotTerm.Text != "999")
            {
                chkLot_Term.IsEnabled = false;
                chkLot_Term.IsChecked = true;
            }
            else
            {
                chkLot_Term.IsEnabled = true;
                chkLot_Term.IsChecked = false;
            }
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

        private void AutoScanSetting()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "AUTO_SCAN_SET";
                dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    GetScanModelQty();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetScanModelQty()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PRODID"] = txtProdID.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    if (Util.NVC(dtResult.Rows[0]["BOX_TOTL_CELL_QTY"]) == "")
                    {
                        txtSettingQty.Value = 0;
                    }
                    else
                    {
                        txtSettingQty.Value = double.Parse(Util.NVC(dtResult.Rows[0]["BOX_TOTL_CELL_QTY"]));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

        private void HidePilotProdMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdPilotProd.Visibility = Visibility.Collapsed;
                //ColorAnimationInredRectangle(recPilotProdMode);
            }));
        }

        private void txtCarrierID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if(ValidationCarrier(txtCarrierID.Text.Trim()))
                    Util.DingPlayer();
                else
                    Util.WarningPlayer();
            }
        }

        private bool ValidationCarrier(string carrierId)
        {
            bool bOK = false;

            // MagazineID 공백 or 널값 여부 확인
            if (string.IsNullOrWhiteSpace(carrierId))
            {
                // Carrier ID를 입력하세요.
                Util.MessageValidation("SFU7006", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCarrierID.Focus();
                        txtCarrierID.SelectAll();
                    }
                });

                Util.WarningPlayer();
                return false;
            }

            // 캐리어 ID 유효성검사
            if (!CarrierValidation(carrierId))
            {
                Util.WarningPlayer();
				return false;
            }

            bOK = true;

            return bOK;
        }

		private bool CarrierValidation(string carrierId)
		{
			try
			{
				DataTable inDataTable = new DataTable();
				inDataTable.Columns.Add("TAGID", typeof(string));
				inDataTable.Columns.Add("MODE", typeof(string));

				DataRow inData = inDataTable.NewRow();
				inData["TAGID"] = carrierId;
				inData["MODE"] = "M";		// Mapping Mode
				inDataTable.Rows.Add(inData);

				DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_USING_CSTID", "INDATA", "OUTDATA", inDataTable);

				if (dtResult.Rows.Count > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				//Util.MessageException(ex);
				Util.MessageException(ex, (result) =>
				  {
					  if (result == MessageBoxResult.OK)
					  {
						  txtCarrierID.Focus();
						  txtCarrierID.SelectAll();
					  }
				  });
				return false;
			}
		}

		private void txtCarrierID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    bool bFlag = false;

                    string strCarrierId = string.Empty;

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        strCarrierId = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(sPasteStrings[i]) && (bFlag = ValidationCarrier(strCarrierId)) == false)
                        {
                            break;
                        }
                        
                        System.Windows.Forms.Application.DoEvents();
                    }

                    txtCarrierID.Text = strCarrierId;

                    if (bFlag)
                        Util.DingPlayer();

                    /*
                    if (bFlag)
                    {
                        //txtCarrierID.Focus();
                        //txtCarrierID.SelectAll();
                        Util.DingPlayer();
                    }
                    else
                    {
                        //txtCarrierID.Focus();
                        //txtCarrierID.SelectAll();
                        Util.WarningPlayer();
                    }
                    */
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

        /// <summary>
        ///  수입 포장 검사 Skip 불가 여부 체크 (2020.10.17 오화백)
        /// </summary>
        /// <returns></returns>
        private int GetSkipBlockVisible()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "INSP_SKIP_BLOCK_AREA";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            return dtRslt.Rows.Count;
        }

        /// <summary>
        ///  BCR 검사 Skip 권한 CEHCK 확인 AREA (2020.12.02 이동우)
        /// </summary>
        /// <returns></returns>
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

        private void SetCHKSkipVisible()
        {
            try
            {
                string chkVisiable = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["COM_CODE"] = "BOX_BCR_CHK_ADMIN";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "INDATA", "OUTDATA", inTable);
                if (dtRslt.Rows.Count == 0)
                    return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow dr2 = RQSTDT.NewRow();
                dr2["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr2["PRODID"] = txtProdID.Text;

                RQSTDT.Rows.Add(dr2);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO_PACK_COND_AUTO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    chkVisiable = Util.NVC(dtResult.Rows[0]["BCD_2D_INSP_FLAG"]);
                    if (chkVisiable == "Y")
                    {
                        chk2D.IsChecked = false;
                    }
                    else
                    {
                        chk2D.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

    }
}
