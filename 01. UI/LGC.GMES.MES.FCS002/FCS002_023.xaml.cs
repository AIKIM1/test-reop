/*************************************************************************************
Created Date : 2020.10.15
     Creator : 
  Decription : Tray 정보생성
--------------------------------------------------------------------------------------
[Change History]
 2020.10.07  NAME : Initial Created
 2021.04.26  박수미 : 라인 자동선택 체크여부에 따른 라인 콤보박스 활성화 설정
 2021.09.02  강동희 : Lot 혼입 시 Lot ID 8자리 체크로직 추가
 2022.01.20  강동희 : 입력한 Cell ID가 대문자로 입력되도록 대응
 2022.05.26  조영대 : 엑셀에서 복사 후 Cell ID 붙여넣기 시 중복메세지 제거
 2022.07.17  조영대 : Cell ID 입력시 가장 마지막 로우로 스크롤.
 2022.07.19  조영대 : Cell ID 입력시 메세지 자동 닫힘처리 및 클리어.
 2022.08.23  조영대 : cboLane 이벤트변경, (IndexChanged 이벤트사용시 선택할때 이전값을 가져옴, SelectionCommitted 로 변경)
 2022.08.24  김진섭 : NB2동 요청사항 - 전 Cell 불량 Tray에 대해서 Selector 공정 완료 상태로 변경하는 기능 추가
 2022.09.21  이정미 : 작업 공정 변경 Tab 초기화 이벤트 추가
 2023.02.16  김태균 : 자동차 -> 소형용으로 변경 작업(UI 일부 변경 포함)
 2025.05.09  이준영 : MES2.0 전환 ->  SetDefaultChk 등 BizRule 명 변경 
 2025.05.27  이준영 : Cell 역순, 정방향 Cell 위치번호 별로 생성 하도록 수정 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Popup;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;


namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_023 : UserControl, IWorkArea
    {

        private bool isAOK = false;  //200330 KJE : A등급 Dummy 금지 Validation
        private bool isReworkRouteChk = true;
        DataTable _SubLotList = new DataTable();
        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;
        private string LOT_DETL_TYPE_CODE = string.Empty;
        private string _sToEQP = string.Empty;
        private string _sToPort = string.Empty;
        private string _sSRCEQP = string.Empty;


        public FCS002_023()
        {
            InitializeComponent();

            this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();

            //라인 자동 선택 체크
            //chkLineAuto.IsChecked = true;
            //cboDummyLineID.IsEnabled = false;

            btnStartOp.Visibility = Visibility.Hidden;

            //Check Box 기본 설정
            SetDefaultChk();

            //SubLotList 설정
            SetSubLotList();

            txtInputDummyTrayID.SelectAll();
            txtInputDummyTrayID.Focus();

            //충방전기 수동예약 기본 설정
            chkReservation.IsChecked = false;
            cboLane.IsEnabled = false;
            cboRow.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboStg.IsEnabled = false;

            chkCPFReservation.IsChecked = false;
            cboCPFLane.IsEnabled = false;
            cboCPFRow.IsEnabled = false;
            cboCPFStg.IsEnabled = false;
            cboCPFCol.IsEnabled = false;

            chkIROCV.Visibility = Visibility.Collapsed;
            cboDummyGrade.Visibility = Visibility.Collapsed;

            this.Loaded -= UserControl_Loaded;
        }

        private void txtInputDummyTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInputDummyTrayID.Text.Trim() == string.Empty)
                    return;
                Receive_ScanMsg(txtInputDummyTrayID.Text.ToUpper().Trim());

                txtCellInput.Clear();
                txtCellInput.Focus();
            }
        }

        private void txtCellInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (txtCellInput.Text.Trim() == string.Empty)
                    {
                        return;
                    }

                    ShowLoadingIndicator();
                    if (!ScanCell(txtCellInput.Text.Trim()))
                    {
                        return;
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        txtCellInput.Text = string.Empty;
                        txtCellInput.Focus();
                    }));
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    HiddenLoadingIndicator();
                    txtCellInput.Focus();
                    txtCellInput.SelectAll();
                }
            }
        }

        private void txtCellInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    string _ValueToFind = string.Empty;

                    //if (sPasteStrings.Count() > 100)
                    //{
                    //    Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                    //    return;
                    //}

                    if (sPasteStrings.Length == 0 || sPasteStrings[0].Trim() == "")
                    {
                        Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    ShowLoadingIndicator();
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!ScanCell(sPasteStrings[i].ToString().Trim()))
                        {
                            return;
                        }
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        txtCellInput.Text = string.Empty;
                        txtCellInput.Focus();
                    }));
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    HiddenLoadingIndicator();
                    txtCellInput.Focus();
                    txtCellInput.SelectAll();
                }
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            dgCell.ItemsSource = null;
            _SubLotList.Clear();

            lstRoute.ItemsSource = null;

            txtDummyTrayID.Text = string.Empty;
            txtDummyCellCnt.Text = string.Empty;
            txtDummyModel.Text = string.Empty;
            txtDummyLotID.Text = string.Empty;
            txtDummyProdCD.Text = string.Empty;

            txtSpecial.Text = string.Empty;
            txtUserName.Text = string.Empty;
            txtUserID.Text = string.Empty;

            txtSumCellQty.Text = "0";
        }

        private void btnDeleteCell_Click(object sender, RoutedEventArgs e)
        {

            dgCell.ItemsSource = null;
            lstRoute.ItemsSource = null;

            txtCellInput.Text = string.Empty;
            txtDummyModel.Text = string.Empty;
            txtDummyLotID.Text = string.Empty;
            txtDummyProdCD.Text = string.Empty;

            txtSpecial.Text = string.Empty;
            txtSumCellQty.Text = "0";

            _SubLotList.Clear();

            InitializeDataGrid(txtDummyTrayID.Text, dgCell);
        }

        private void btnCellRemove_Click(object sender, RoutedEventArgs e)
        {
            DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

            if (presenter == null)
                return;

            int clickedIndex = presenter.Row.Index;
            DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);
            dt.Rows.RemoveAt(clickedIndex);
            Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetTrayInfo(true);
        }

        private void btnChangeOp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Tray 정보를 변경하시겠습니까?
                Util.MessageConfirm("FM_ME_0079", (resultMessage) =>
                {
                    if (resultMessage == MessageBoxResult.OK)
                    {
                        // 공정 변경
                        DataSet ds = new DataSet();
                        DataTable dt = ds.Tables.Add("INDATA");

                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("IFMODE", typeof(string));
                        dt.Columns.Add("CSTID", typeof(string));
                        dt.Columns.Add("PROCID", typeof(string));
                        dt.Columns.Add("LANE_ID", typeof(string));
                        dt.Columns.Add("EQP_ROW_LOC", typeof(string));
                        dt.Columns.Add("EQP_COL_LOC", typeof(string));
                        dt.Columns.Add("EQP_STG_LOC", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));
                        dt.Columns.Add("FINL_JUDG_CODE", typeof(string));   //소형 추가

                        DataRow dr = dt.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["CSTID"] = Util.GetCondition(txtTrayID);
                        dr["PROCID"] = Util.GetCondition(cboOp);

                        dr["USERID"] = LoginInfo.USERID;

                        //if ((cboDummyGrade.Visibility == Visibility.Visible) && (cboDummyGrade.IsEnabled == true))
                        //{
                        //    dr["FINL_JUDG_CODE"] = Util.GetCondition(cboDummyGrade, sMsg: "FM_ME_0122"); //등급을 선택해주세요.
                        //}

                        dt.Rows.Add(dr);

                        ShowLoadingIndicator();
                        new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_CHANGE_TRAY_OP_MB", "INDATA", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                //if (chkIROCV.IsChecked == true)
                                //{
                                //    SetIROCVDummy();
                                //}
                                switch (bizResult.Tables[0].Rows[0]["RESULT"].ToString())
                                {
                                    case "0":
                                        //변경완료하였습니다
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0136"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }


                                        });
                                        break;

                                    case "1":
                                        //이전 작업 공정 종료, Tray 정보 화면 내 정상종료를 실행해 주세요.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0195"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "2":
                                        //작업 불가 공정 입니다.(델타, 판정)
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0198"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "3":
                                        //충방전기 열,단 정보가 없습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0243"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "4":
                                        //충방전기작업과 맞지않습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0244"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    case "5":
                                        //현재 예약된 Tray와 공정 정보가 맞지 않습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0264"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                    default:
                                        //Tray 정보 변경 중 오류가 발생하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0072"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                                        {
                                            if (result == MessageBoxResult.OK)
                                            {
                                                GetTrayInfo(false);
                                            }
                                        });
                                        break;

                                }

                               // GetEqpCurTray(); -- 사용 안함 MHS 영역에서 분석/정의

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

                });


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

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetTrayInfo(true);
            }
        }

        private void chkReservation_Checked(object sender, RoutedEventArgs e)
        {
            chkCPFReservation.IsChecked = false;

            cboLane.IsEnabled = true;
            cboStg.IsEnabled = true;
            cboCol.IsEnabled = true;
            cboRow.IsEnabled = true;
        }

        private void chkReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            cboLane.IsEnabled = false;
            cboStg.IsEnabled = false;
            cboCol.IsEnabled = false;
            cboRow.IsEnabled = false;
        }

        private void chkCPFReservation_Checked(object sender, RoutedEventArgs e)
        {
            chkReservation.IsChecked = false;

            cboCPFLane.IsEnabled = true;
            cboCPFRow.IsEnabled = true;
            cboCPFStg.IsEnabled = true;
            cboCPFCol.IsEnabled = true;
        }

        private void chkCPFReservation_Unchecked(object sender, RoutedEventArgs e)
        {
            cboCPFLane.IsEnabled = false;
            cboCPFRow.IsEnabled = false;
            cboCPFStg.IsEnabled = false;
            cboCPFCol.IsEnabled = false;
        }


        private void GetToEqptID()
        {
            _sToEQP = string.Empty;
            _sToPort = string.Empty;

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("LANEID", typeof(string));
            dtRqst.Columns.Add("ROW", typeof(string));
            dtRqst.Columns.Add("COL", typeof(string));
            dtRqst.Columns.Add("STG", typeof(string));
            dtRqst.Columns.Add("EQGRID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;

            if (chkReservation.IsChecked == true)
            {
                dr["LANEID"] = Util.GetCondition(cboLane);
                dr["ROW"] = Util.GetCondition(cboRow);
                dr["COL"] = Util.GetCondition(cboCol);
                dr["STG"] = Util.GetCondition(cboStg);
                dr["EQGRID"] = "1";
            }

            else if (chkCPFReservation.IsChecked == true)
            {
                dr["LANEID"] = Util.GetCondition(cboCPFLane);
                dr["ROW"] = Util.GetCondition(cboCPFRow);
                dr["COL"] = Util.GetCondition(cboCPFCol);
                dr["STG"] = Util.GetCondition(cboCPFStg);
                dr["EQGRID"] = "L";
            }

            dtRqst.Rows.Add(dr);
            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SELL_EQUIPMENT_BY_ROW_COL_STG_MB", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count > 0)
            {
                    _sToEQP = dtRslt.Rows[0]["EQPTID"].ToString();
                    _sToPort = dtRslt.Rows[0]["PORT_ID"].ToString();
            }

        }
        private void GetSRCEqptID()
        {

            _sSRCEQP = string.Empty;

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["CSTID"] = txtTrayID.Text;
            
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FROM_EQP_INFO_MB", "RQSTDT", "RSLTDT", dtRqst);

            if (dtRslt.Rows.Count > 0)
                _sSRCEQP = dtRslt.Rows[0]["SRC_EQPTID"].ToString();
            
        }
        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                ////////////////////////////////////////////  default 색상 및 Cursor
                e.Cell.Presenter.Cursor = Cursors.Arrow;
                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                e.Cell.Presenter.FontSize = 12;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                ///////////////////////////////////////////////////////////////////////////////////

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string[] NotUseRow = _sNotUseRowLIst.Split(',');
                    string[] NotUseCol = _sNotUseColLIst.Split(',');

                    if (NotUseRow.Contains(e.Cell.Row.Index.ToString()) && NotUseCol.Contains(e.Cell.Column.Index.ToString()))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                    }

                    e.Cell.Presenter.IsEnabled = false;
                }

                //if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "COLOR"))))
                //{
                //    //e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;
                //    e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "COLOR").ToString()) as SolidColorBrush;
                //}
            }));
        }

        private void btnCreateDummy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCurCnt = 0;

                if (chkJigRework.IsChecked == true)
                {
                    //JIG 재작업이 체크되어있습니다. 이대로 진행하시겠습니까?
                    Util.MessageConfirm("FM_ME_0330", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            chkJigRework.IsChecked = false;
                        }
                    });
                }

                DataSet ds = new DataSet();

                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("LANGID", typeof(string)); //추가
                dt.Columns.Add("PROD_LOTID", typeof(string));
                dt.Columns.Add("ROUTID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("CELL_CNT", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("SPCL_FLAG", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                dt.Columns.Add("SPCL_DESC", typeof(string));
                dt.Columns.Add("JIG_REWORK_YN", typeof(string));
                dt.Columns.Add("REQ_USERID", typeof(string));
                dt.Columns.Add("STORAGE_YN", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                //특별 예상 해제일 추가 2021.06.30 PSM
                //dt.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(string));
                dt.Columns.Add("FORM_SPCL_REL_SCHD_DTTM", typeof(DateTime)); // biz indata type datetime 으로 설정
                dt.Columns.Add("FORM_PRE_LOT_DETL_TYPE_CODE", typeof(string));                

                DataRow dr = dt.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["USERID"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROD_LOTID"] = Util.GetCondition(txtDummyLotID, sMsg: "FM_ME_0049");  //Lot ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["PROD_LOTID"].ToString())) return;

                //dr["ROUTID"] = Util.GetCondition(lstRoute, sMsg: "FM_ME_0106");  //공정경로를 선택해주세요.
                //if (string.IsNullOrEmpty(dr["ROUTID"].ToString())) return;

                if (string.IsNullOrEmpty(lstRoute.SelectedValue.ToString()))
                {
                    //공정경로를 선택해주세요.
                    Util.MessageInfo("FM_ME_0106");
                    return;
                }
                else
                {
                    dr["ROUTID"] = lstRoute.SelectedValue.ToString();
                }

                dr["CSTID"] = Util.GetCondition(txtDummyTrayID, sMsg: "FM_ME_0070");  //Tray ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["CSTID"].ToString())) return;

                dr["CELL_CNT"] = iCurCnt;

                dr["EQSGID"] = Util.GetCondition(cboDummyLineID, sMsg: "FM_ME_0044");  //Line ID를 입력해주세요.
                if (string.IsNullOrEmpty(dr["EQSGID"].ToString())) return;

                dr["SPCL_FLAG"] = Util.GetCondition(cboSpecial);
                dr["PRODID"] = Util.GetCondition(txtDummyProdCD);

                if (!Util.NVC(cboSpecial.SelectedValue).Equals("N") && string.IsNullOrEmpty(txtSpecial.Text))
                {
                    Util.Alert("FM_ME_0113");  //관리내역을 입력해주세요.
                    return;
                }
                dr["SPCL_DESC"] = txtSpecial.Text;
                //  if (string.IsNullOrEmpty(dr["SPCL_DESC"].ToString())) return;

                dr["JIG_REWORK_YN"] = (!chkStorageR.IsChecked == true && chkJigRework.IsChecked == true) ? "Y" : "N";

                if (!dr["SPCL_FLAG"].Equals("N"))
                {
                    dr["REQ_USERID"] = Util.GetCondition(txtUserID, sMsg: "FM_ME_0355"); //요청자를 입력해주세요.
                    if (string.IsNullOrEmpty(dr["REQ_USERID"].ToString())) return;
                }

                //200326 KJE : 저장Route 추가
                //저장Route 선택 시
                dr["STORAGE_YN"] = chkStorageR.IsChecked == true ? "Y" : "N";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if ((bool)chkReleaseDate.IsChecked)   //특별 예상 해제일 추가 2021.06.30 PSM
                {
                   dr["FORM_SPCL_REL_SCHD_DTTM"] = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + dtpFromTime.DateTime.Value.ToString("HH:mm:00");
                }

                dr["FORM_PRE_LOT_DETL_TYPE_CODE"] = LOT_DETL_TYPE_CODE;
                
                dt.Rows.Add(dr);

                DataTable dtCell = ds.Tables.Add("IN_CELL");
                dtCell.Columns.Add("SUBLOTID", typeof(string));
                dtCell.Columns.Add("CSTSLOT", typeof(string));


          
                
                DataRow drCell = null;
                for (int i = 0; i < _SubLotList.Rows.Count; i++)
                {
                    drCell = dtCell.NewRow();
                    drCell["SUBLOTID"] = _SubLotList.Rows[i]["SUBLOTID"].ToString();
                    drCell["CSTSLOT"] = _SubLotList.Rows[i]["CSTSLOT"].ToString();

                    if (Util.NVC(_SubLotList.Rows[i]["SUBLOTID"].ToString()) == "0000000000")
                    {
                        iCurCnt++;
                    }

                    dtCell.Rows.Add(drCell);
                 }

              

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi("BR_SET_TRAY_CREATE_DUMMY_TRAY_MB", "INDATA,IN_CELL", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString().Equals("0"))
                        {
                            //생성완료하였습니다.
                            Util.MessageInfo("FM_ME_0160");
                            btnClear_Click(null, null);
                            txtTrayID.Text = dt.Rows[0]["CSTID"].ToString();
                            btnSearch_Click(null, null);

                            //List 초기화
                            _SubLotList.Clear();

                            chkReverse.IsHitTestVisible = true;
                            rdoV.IsEnabled = true;
                            rdoH.IsEnabled = true;
                       
                        }
                        else
                        {
                            //생성실패하였습니다. (Result Code : {0})
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0012", bizResult.Tables["OUTDATA"].Rows[0]["RESULT"].ToString()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                }
                            });
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, ds);

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

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnSearchUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void cboLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchFormationBoxList(Util.GetCondition(cboLane));
        }

        private void cboCPFLane_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            SetSearchCPFFormationBoxList(Util.GetCondition(cboCPFLane));
        }

        //2021-04-26 추가
        private void chkLineAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = false;
        }

        private void chkLineAuto_Unchecked(object sender, RoutedEventArgs e)
        {
            if (cboDummyLineID == null) return;
            cboDummyLineID.IsEnabled = true;
        }

        //2021-05-04 RowHeader 추가 - 생산팀 요청
        private void dgCell_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgCell.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void chkReleaseDate_Checked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = true;
            dtpFromTime.IsEnabled = true;
        }

        private void chkReleaseDate_Unchecked(object sender, RoutedEventArgs e)
        {
            dtpFromDate.IsEnabled = false;
            dtpFromTime.IsEnabled = false;
        }

        private void cboSpecial_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (Util.NVC(e.NewValue).Equals("N"))
            {
                txtSpecial.IsEnabled = false;
            }
            else
            {
                txtSpecial.IsEnabled = true;
            }
        }

        private void btnStartOp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = txtTrayID.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERID"] = LoginInfo.USERID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_SELECTOR_TRAY_MANUAL_START_END_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    Util.Alert("FM_ME_0215");  //저장하였습니다.

                    btnStartOp.Visibility = Visibility.Hidden;
                }
                else
                {
                    Util.Alert("FM_ME_0213");  //저장실패하였습니다.
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

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            txtTrayID.Text = string.Empty;
            txtSTrayID.Text = string.Empty;
            txtOp.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtIssRsvFlag.Text = string.Empty;
            cboOp.Text = string.Empty;
            chkReservation.IsChecked = false;
            cboLane.SelectedIndex = 0;
            cboRow.SelectedIndex = 0;
            cboCol.SelectedIndex = 0;
            cboStg.SelectedIndex = 0;
            cboCPFLane.SelectedIndex = 0;
            cboCPFRow.SelectedIndex = 0;
            cboCPFStg.SelectedIndex = 0;
            cboCPFCol.SelectedIndex = 0;
            txtUpperTray.Text = string.Empty;
            txtLowerTray.Text = string.Empty;

            chkIROCV.Visibility = Visibility.Collapsed;
            cboDummyGrade.Visibility = Visibility.Collapsed;
        }

        private void chkABan_Checked(object sender, EventArgs e)
        {
            //A등급 Cell을 Dummy Tray에 추가 허용하시겠습니까?
            Util.MessageConfirm("FM_ME_0360", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    isAOK = true;
                }
                else
                {
                    chkABan.Unchecked -= chkABan_UnChecked;
                    chkABan.IsChecked = false;
                    chkABan.Unchecked += chkABan_UnChecked;

                    isAOK = false;
                }
            });
        }

        private void chkABan_UnChecked(object sender, EventArgs e)
        {
            //작업중이던 정보가 모두 초기화 됩니다.\r\n계속 진행 하시겠습니까?
            Util.MessageConfirm("FM_ME_0203", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    btnClear_Click(null, null);
                    isAOK = false;
                }
                else
                {
                    chkABan.Checked -= chkABan_Checked;
                    chkABan.IsChecked = true;
                    chkABan.Checked += chkABan_Checked;

                    isAOK = true;
                }
            });
        }

        private void cboOp_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            //try
            //{
            //    CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //    string sProcGrCode = string.Empty;  //공정 그룹코드
            //    string sRProcDetlTypeCode = string.Empty;   //공정 상세유형 코드

            //    DataTable dtRqst = new DataTable();
            //    dtRqst.TableName = "RQSTDT";
            //    dtRqst.Columns.Add("PROCID", typeof(string));

            //    DataRow dr = dtRqst.NewRow();
            //    dr["PROCID"] = Util.GetCondition(cboOp, bAllNull: true);
            //    dtRqst.Rows.Add(dr);

            //    ShowLoadingIndicator();
            //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_TBL", "RQSTDT", "RSLTDT", dtRqst);

            //    if (dtRslt.Rows.Count > 0)
            //    {
            //        sProcGrCode = dtRslt.Rows[0]["S26"].ToString();
            //        sRProcDetlTypeCode = dtRslt.Rows[0]["S27"].ToString();
            //    }
            //    else
            //    {
            //        return;
            //    }

            //    if (sProcGrCode.Equals("1"))
            //    {
            //        if (sRProcDetlTypeCode.Equals("17"))
            //        {
            //            chkCPFReservation.IsEnabled = true;
            //            chkReservation.IsEnabled = false;
            //            cboLane.IsEnabled = false;
            //            cboRow.IsEnabled = false;
            //            cboStg.IsEnabled = false;
            //            cboCol.IsEnabled = false;
            //            if (chkCPFReservation.IsEnabled == true)
            //            {
            //                cboCPFLane.IsEnabled = true;
            //                cboCPFRow.IsEnabled = true;
            //                cboCPFStg.IsEnabled = true;
            //                cboCPFCol.IsEnabled = true;

            //            }

            //        }
            //        else
            //        {
            //            chkReservation.IsEnabled = true;
            //            chkCPFReservation.IsEnabled = false;
            //            cboCPFLane.IsEnabled = false;
            //            cboCPFRow.IsEnabled = false;
            //            cboCPFStg.IsEnabled = false;
            //            cboCPFCol.IsEnabled = false;
            //            if (chkReservation.IsChecked == true)
            //            {
            //                cboLane.IsEnabled = true;
            //                cboRow.IsEnabled = true;
            //                cboStg.IsEnabled = true;
            //                cboCol.IsEnabled = true;

            //            }

            //        }
            //    }
            //    else
            //    {
            //        chkReservation.IsEnabled = false;
            //        cboLane.IsEnabled = false;
            //        cboRow.IsEnabled = false;
            //        cboStg.IsEnabled = false;
            //        cboCol.IsEnabled = false;
            //        chkCPFReservation.IsEnabled = false;
            //        cboCPFLane.IsEnabled = false;
            //        cboCPFRow.IsEnabled = false;
            //        cboCPFStg.IsEnabled = false;
            //        cboCPFCol.IsEnabled = false;
            //    }

            //    if (sProcGrCode == "I")
            //    {
            //        string[] sFilter = { txtSTrayID.Text };
            //        _combo.SetCombo(cboDummyGrade, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "IROCV_GRADE");

            //        chkIROCV.Visibility = Visibility.Visible;
            //        cboDummyGrade.Visibility = Visibility.Visible;

            //        // 생산팀 요구사항 TOP 권한만 등급더미 가능하게 변경 -> GMES 권한 ID는 운영에 반영 후 변경해줘야 함(현재는 NB2동 ADMIN 권한 명으로 해 놓음(2023/02/21)
            //        if (!LoginInfo.AUTHID.Equals("FORM_AUTO_MANA"))
            //        {
            //            chkIROCV.IsEnabled = true;
            //            cboDummyGrade.IsEnabled = true;
            //        }
            //        else
            //        {
            //            chkIROCV.IsEnabled = false;
            //            cboDummyGrade.IsEnabled = false;
            //        }
            //    }
            //    else
            //    {
            //        chkIROCV.Visibility = Visibility.Collapsed;
            //        cboDummyGrade.Visibility = Visibility.Collapsed;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            //finally
            //{
            //    HiddenLoadingIndicator();
            //}
        }

        private void cboOp_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string sProcGrCode = string.Empty;  //공정 그룹코드
                string sRProcDetlTypeCode = string.Empty;   //공정 상세유형 코드

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROCID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROCID"] = Util.GetCondition(cboOp, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSATTR_TBL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    sProcGrCode = dtRslt.Rows[0]["S26"].ToString();
                    sRProcDetlTypeCode = dtRslt.Rows[0]["S27"].ToString();
                }
                else
                {
                    return;
                }

                if (sProcGrCode.Equals("1"))
                {
                    if (sRProcDetlTypeCode.Equals("1A") || sRProcDetlTypeCode.Equals("1B") || sRProcDetlTypeCode.Equals("1C"))
                    {
                        chkCPFReservation.IsEnabled = true;
                        //if (chkCPFReservation.IsChecked == true)
                        //{
                        //    chkCPFReservation.IsChecked = false;
                        //}
                        //else
                        //{
                        //    cboCPFLane.IsEnabled = false;
                        //    cboCPFRow.IsEnabled = false;
                        //    cboCPFStg.IsEnabled = false;
                        //    cboCPFCol.IsEnabled = false;
                        //}

                        chkReservation.IsEnabled = false;
                        chkReservation.IsChecked = false;
                    }
                    else
                    {
                        chkReservation.IsEnabled = true;
                        //if (chkReservation.IsChecked == true)
                        //{
                        //    chkReservation.IsChecked = false;
                        //}
                        //else
                        //{
                        //    cboLane.IsEnabled = false;
                        //    cboStg.IsEnabled = false;
                        //    cboCol.IsEnabled = false;
                        //    cboRow.IsEnabled = false;
                        //}

                        chkCPFReservation.IsEnabled = false;
                        chkCPFReservation.IsChecked = false;
                    }
                }
                else
                {
                    chkReservation.IsEnabled = false;
                    chkReservation.IsChecked = false;
                    chkCPFReservation.IsEnabled = false;
                    chkCPFReservation.IsChecked = false;
                }

                //if (sProcGrCode == "I")
                //{
                //    string[] sFilter = { Util.NVC(txtSTrayID.Text) };
                //    _combo.SetCombo(cboDummyGrade, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "IROCV_GRADE");

                //    chkIROCV.Visibility = Visibility.Visible;
                //    cboDummyGrade.Visibility = Visibility.Visible;

                //    // 생산팀 요구사항 TOP 권한만 등급더미 가능하게 변경 -> GMES 권한 ID는 운영에 반영 후 변경해줘야 함(현재는 NB2동 ADMIN 권한 명으로 해 놓음(2023/02/21)
                //    if (!LoginInfo.AUTHID.Equals("FORM_AUTO_MANA"))
                //    {
                //        chkIROCV.IsEnabled = true;
                //        cboDummyGrade.IsEnabled = true;
                //    }
                //    else
                //    {
                //        chkIROCV.IsEnabled = false;
                //        cboDummyGrade.IsEnabled = false;
                //        cboDummyGrade.Clear();
                //    }
                //}
                //else
                //{
                //    chkIROCV.Visibility = Visibility.Collapsed;
                //    cboDummyGrade.Visibility = Visibility.Collapsed;
                //    cboDummyGrade.Clear();
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region Method

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            //동
            C1ComboBox[] cboAreaChild = { cboDummyLineID };
            _combo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.ALL, cbChild: cboAreaChild);

            //Login 한 AREA Setting
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //라인
            //ComCombo.SetCombo(cboDummyLIneID, ComboStatus.NONE, sCase: "LINE");

            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboDummyLineID, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LINE", cbParent: cboLineParent);

            string[] sFilter = { "FORM_SPCL_FLAG_MCC" };
            _combo.SetCombo(cboSpecial, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "CMN");

            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE");

            // CPF
            //_combo.SetCombo(cboCPFLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE");

            string[] sFilterLane = { "L" };
            _combo.SetCombo(cboCPFLane, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "LANE_MB", sFilter: sFilterLane);
        }

        private void SetSearchFormationBoxList(string sLaneID)
        {
            try
            {
                cboRow.Text = string.Empty;
                cboCol.Text = string.Empty;
                cboStg.Text = string.Empty;

                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string[] sFilter = { "1", sLaneID };    //EquipmentAttr : S70(설비 그룹 유형 코드[EQPT_GR_TYPE_CODE]), S71(LANE 아이디[LANE_ID])
                _combo.SetCombo(cboRow, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "ROW");
                _combo.SetCombo(cboCol, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "COL");
                _combo.SetCombo(cboStg, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "STG");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetSearchCPFFormationBoxList(string sLaneID)
        {
            try
            {
                cboCPFRow.Text = string.Empty;
                cboCPFCol.Text = string.Empty;
                cboCPFStg.Text = string.Empty;

                CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

                string[] sFilter = { "L", sLaneID };    //EquipmentAttr : S70(설비 그룹 유형 코드[EQPT_GR_TYPE_CODE]), S71(LANE 아이디[LANE_ID])
                _combo.SetCombo(cboCPFRow, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "ROW");
                _combo.SetCombo(cboCPFCol, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "COL");
                _combo.SetCombo(cboCPFStg, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "STG");
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void SetDefaultChk()
        {
            try
            {
                #region [A등급 금지]
                //200330 KJE : A등급 Dummy 금지 Validation
                //A등급 금지 기본값 SITE별 설정
                DataTable dtRqstA = new DataTable();
                dtRqstA.TableName = "RQSTDT";
                dtRqstA.Columns.Add("LANGID", typeof(string));
                dtRqstA.Columns.Add("CMCDTYPE", typeof(string));
                dtRqstA.Columns.Add("CMN_CD_LIST", typeof(string));

                DataRow drA = dtRqstA.NewRow();
                drA["LANGID"] = LoginInfo.LANGID;
                drA["CMCDTYPE"] = "SITE_BASE_INFO";
                drA["CMN_CD_LIST"] = "A_DUMMY_BAN";
                dtRqstA.Rows.Add(drA);

                ShowLoadingIndicator();
                DataTable dtABanYN = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_CMN_ALL_ITEMS", "RQSTDT", "RSLTDT", dtRqstA);

                if (dtABanYN.Rows.Count > 0 && dtABanYN.Rows[0]["ATTRIBUTE1"].ToString().Equals("Y"))
                {
                    chkABan.IsChecked = true;
                    isAOK = false;
                }
                else
                {
                    chkABan.IsChecked = false;
                    isAOK = true;
                }
                chkABan.Checked += chkABan_Checked;
                chkABan.Unchecked += chkABan_UnChecked;
                #endregion
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
        
        private void Receive_ScanMsg(string sScan)
        {
            string sResultMsg = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sScan) || sScan.Length < 10)
                {
                    //잘못된 ID입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        this.txtInputDummyTrayID.SelectAll();
                        this.txtInputDummyTrayID.Focus();
                    });
                    return;
                }

                //10자리 넘을 경우 10자리로 자름
                if (sScan.Length > 10)
                {
                    sScan = sScan.Substring(0, 10);
                }

                DataTable OutTable;

                //직전 전Cell불량 Tray인지 확인
                if (GetCheckLossTray(sScan, out OutTable))
                {
                    Util.gridClear(dgCell);
                    txtDummyTrayID.Text = sScan;
                    GetCellCnt(txtDummyTrayID.Text);

                    for (int i = 0; i < OutTable.Rows.Count; i++)
                    {
                        if (OutTable.Rows[i]["SUBLOTID"].ToString().Equals("0000000000"))
                        {
                            SetEmptyCellRow();
                            continue;
                        }
                        //모든 체크 끝나면 스프레드에 CELL ID 쓰기
                        Receive_ScanMsg(OutTable.Rows[i]["SUBLOTID"].ToString());
                    }
                }
                //재공유무 및 Tray상태 확인(청소,폐기는 Dummy 생성 불가)
                else if (GetCheckTrayStatus(sScan, out sResultMsg))
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sResultMsg), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.txtInputDummyTrayID.SelectAll();
                            this.txtInputDummyTrayID.Focus();
                        }
                    });
                    return;
                }

                txtDummyTrayID.Text = sScan.Trim();
                GetCellCnt(txtDummyTrayID.Text);

                this.txtInputDummyTrayID.Clear();
                this.txtInputDummyTrayID.Focus();

                return;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool GetCheckLossTray(string sTray, out DataTable OutTable)
        {
            bool bCheck = false;
            OutTable = null;

            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CSTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CSTID"] = sTray;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_TRAY_ALL_LOSS_CELL", "INDATA", "OUTDATA,CELLS", inDataSet);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0 && dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    OutTable = dsRslt.Tables["CELLS"];

                    //모든 Cell이 불량인 Tray입니다.\r\nTray 정보를 생성하시겠습니까?
                    Util.MessageConfirm("FM_ME_0131", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            bCheck = true;
                        }
                    });
                }

                return bCheck;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }

        private void GetCellCnt(string TrayID)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = TrayID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_TM_TRAY_TYPE_MB", "INDATA", "OUTDATA", ds);

                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    txtDummyCellCnt.Text = dsRslt.Tables["OUTDATA"].Rows[0]["CST_CELL_QTY"].ToString();

                    //Grid Setting
                    //SetTrayType(dgCell, dsRslt.Tables["OUTDATA"].Rows[0]["CST_CELL_TYPE_CODE"].ToString());
                    InitializeDataGrid(TrayID, dgCell);

                    return;
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

        private bool ScanCell(string sCellID)
        {
            string sWipStat = string.Empty;
            string sWipSName = string.Empty;
            string sLotDetlTypeCode = string.Empty;
            string sColor = string.Empty;
            string sTrayType = string.Empty;
            string sCstsLOT = string.Empty;
            string sLotDetlTypeName = string.Empty;
            string Convert_Cell = string.Empty;
            bool bValue = true;

            try
            {
                if (string.IsNullOrEmpty(txtDummyTrayID.Text))
                {
                    this.txtInputDummyTrayID.Clear();
                    //Tray를 먼저 입력해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0080"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return false;
                }

                //Tray Type Get
                GetTrayType(txtDummyTrayID.Text.Trim(), ref sTrayType);

                //25.04.22 Vent ID, CAN ID 입력 시 Cell ID 가져오도록 수정 요청 
                Convert_Cell = Util.Convert_CellID(sCellID);

                //셀 빈것 처리
                if (Convert_Cell.Equals("0000000000"))
                {
                    SetEmptyCellRow();
                    return false;
                }

                if (string.IsNullOrEmpty(Convert_Cell) || Convert_Cell.Length < 10)
                {
                    //잘못된 ID입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0205"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return false;
                }

                //이미 스캔한 CELL ID인지 Check
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    for (int k = 0; k < dgCell.Columns.Count; k++)
                    {
                        if (Util.NVC(dgCell.GetCell(i, k).Text) == Convert_Cell)
                        {
                            //이미 스캔한 ID 입니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0193"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                            return false;
                        }
                    }
                }

                GetCellCheck(Convert_Cell, ref sWipStat, ref sWipSName, ref sLotDetlTypeCode, ref sLotDetlTypeName, ref sColor, ref sCstsLOT);

                if (sWipStat == "TERM")
                {
                    //이미 종료된 LOT입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0401"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return false;
                }

                //조립등록 여부 체크
                if (!GetCellAssyCheck(Convert_Cell))
                {
                    //조립미등록 Cell입니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0228"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                    return false;
                }

                // 첫번째 셀이 아니고, 기존 조회된 Route정보가 여러개라면
                if (!string.IsNullOrEmpty(txtSumCellQty.Text) && int.Parse(txtSumCellQty.Text) > 0 && lstRoute.Items.Count > 1)
                {
                    isReworkRouteChk = false;
                }

                //등급/출하여부 체크
                DataTable dtGrade = new DataTable();
                if (!GetBadCellCheck(Convert_Cell, ref dtGrade))
                {
                    return false;
                }

                //// 재작업 라우트 자동선택 체크시  
                //// 첫번째 셀이 아니면 스캔한 셀 공정 체크
                //if (isReworkRouteChk && chkReRoute.IsChecked == true && dgCell.Rows.Count > 0)
                //{
                //    string sRouteId = string.Empty;

                //    if (lstRoute.Items.Count == 0)
                //    {
                //        //Tray의 공정경로가 존재하지 않습니다.
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0082"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                //        return;
                //    }
                //    else
                //    {
                //        lstRoute.SelectedIndex = 0;
                //        sRouteId = lstRoute.SelectedValue.ToString();
                //    }

                //    if (!GetReWorkCheck(sCellID, sRouteId, sTrayType))
                //    {
                //        //공정경로가 동일하지 않습니다.
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0101"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                //        return;
                //    }
                //}

                //최초 CELL 일경우 처리
                if (string.IsNullOrEmpty(txtSumCellQty.Text) || txtSumCellQty.Text == "0")  //Todo : 로직수정해야됨, 최초셀이지만 이전에 None True Cell 만 있을경우에도 최초True셀이 될수있음
                {
                    if (!GetFirstCellCheck(Convert_Cell, sTrayType))
                    {
                        return false;
                    }

                    if (lstRoute.Items.Count == 0)
                    {
                        //공정경로가 존재하지 않습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0102"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return false;
                    }
                }
                else
                {
                    if (!GetSecondLotCheck(Convert_Cell))
                    {
                        //이전 입력된 Cell과 Lot ID가 다릅니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0194"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return false;
                    }

                    //LOTTYPE Validation 추가
                    if (!GetLotTypeCheck(Convert_Cell))
                    {
                        //이전 입력된 Cell과 LOTTYPE이 다릅니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0430"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return false;
                    }

                    //LOT_DETL_TYPE_CODE Validation 추가
                    if (!GetLotDetlTypeCheck(Convert_Cell))
                    {
                        //이전 입력된 Cell과 Lot 상세 유형이 다릅니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0470"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return false;
                    }
                }

                //불량 폐기 셀 등록 여부 체크(TC_CELL_LOSS)
                //복구하는 로직이 있으므로 가장 마지막에 체크함
                //2020-06-30 KJE : CWA 3동 신규 실적로직 반영으로 인하여 더미생성시 불량복구 안함
                //if (!LoginInfo.AREAID.ToString().Substring(0, 3).Equals("CWA"))
                //{
                if (!GetLossCellCheck(Convert_Cell))
                {
                    return false;
                }
                //}

                //Binding
                DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);

                int iSeq = 1;
                int iSeqReverse = 144;

                if (chkReverse.IsChecked == true)   //역순일때
                {
                   // chkReverse.IsHitTestVisible = false;
                                                              
                    if (rdoV.IsChecked == true) //세로방향
                    {
                     //   rdoH.IsEnabled = false;

                        for (int i = dt.Columns.Count - 1; i >= 0; i--) //Column
                        {
                            for (int j = dt.Rows.Count - 1; j >= 0; j--) //Row
                            {
                                if (!dt.Rows[j][i].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    string sSubLot = dt.Rows[j][i].ToString();
                                    if (string.IsNullOrEmpty(sSubLot))
                                    {
                                        DataRow d = _SubLotList.NewRow();
                                        d["SUBLOTID"] = Convert_Cell.ToUpper();
                                        d["CSTSLOT"] = iSeqReverse;
                                        _SubLotList.Rows.Add(d);

                                        dt.Rows[j][i] = Convert_Cell.ToUpper();
                                        dgCell.ItemsSource = DataTableConverter.Convert(dt);
                                        txtSumCellQty.Text = CellCountCheck().ToString();

                                        return true;
                                    }
                                }
                                iSeqReverse--;
                            }                           
                        }
                    }
                    else if (rdoH.IsChecked == true) //가로방향
                    {
                       // rdoV.IsEnabled = false;
                        for (int i = dt.Rows.Count - 1; i >= 0; i--) //Row
                        {
                            for (int j = dt.Columns.Count - 1; j >= 0; j--) //Column
                            {
                                if (!dt.Rows[i][j].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    string sSubLot = dt.Rows[i][j].ToString();
                                    if (string.IsNullOrEmpty(sSubLot))
                                    {
                                        DataRow d = _SubLotList.NewRow();
                                        d["SUBLOTID"] = Convert_Cell.ToUpper();
                                        d["CSTSLOT"] = iSeqReverse;
                                        _SubLotList.Rows.Add(d);

                                        dt.Rows[i][j] = Convert_Cell.ToUpper();
                                        dgCell.ItemsSource = DataTableConverter.Convert(dt);
                                        txtSumCellQty.Text = CellCountCheck().ToString();

                                        return true;
                                    }
                                }
                                iSeqReverse -= dt.Rows.Count;
                            }
                        }
                    }
                }
                
                else //역순 아닐때
                {
                    if (rdoV.IsChecked == true) //세로방향
                    {                  
                        for (int i = 0; i < dt.Columns.Count; i++) //Column
                        {
                            for (int j = 0; j < dt.Rows.Count; j++) //Row
                            {
                                if (!dt.Rows[j][i].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    string sSubLot = dt.Rows[j][i].ToString();
                                    if (string.IsNullOrEmpty(sSubLot))
                                    {
                                        DataRow d = _SubLotList.NewRow();
                                        d["SUBLOTID"] = Convert_Cell.ToUpper();
                                        d["CSTSLOT"] = iSeq;
                                        _SubLotList.Rows.Add(d);

                                        dt.Rows[j][i] = Convert_Cell.ToUpper();
                                        dgCell.ItemsSource = DataTableConverter.Convert(dt);
                                        txtSumCellQty.Text = CellCountCheck().ToString();

                                        return true;
                                    }
                                }
                                iSeq++;
                            }
                        }
                    }
                    else if (rdoH.IsChecked == true) //가로방향
                    {
                     //   rdoV.IsEnabled = false;
                        for (int i = 0; i < dt.Rows.Count; i++) //Row
                        {
                            for (int j = 0; j < dt.Columns.Count; j++) //Column
                            {
                                if (!dt.Rows[i][j].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                                {
                                    string sSubLot = dt.Rows[i][j].ToString();
                                    if (string.IsNullOrEmpty(sSubLot))
                                    {
                                        DataRow d = _SubLotList.NewRow();
                                        d["SUBLOTID"] = Convert_Cell.ToUpper();
                                        d["CSTSLOT"] = iSeq;
                                        _SubLotList.Rows.Add(d);

                                        dt.Rows[i][j] = Convert_Cell.ToUpper();
                                        dgCell.ItemsSource = DataTableConverter.Convert(dt);
                                        txtSumCellQty.Text = CellCountCheck().ToString();

                                        return true;
                                    }
                                }
                                iSeq += dt.Rows.Count;
                            }
                        }
                    }
                }
                return bValue;
            }
             
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return bValue;
        }

        private void InitDT(ref DataTable dt)
        {
            dt.Columns.Add("SUBLOTID");
            dt.Columns.Add("FINL_JUDG_CODE");
            dt.Columns.Add("WIPSTAT");
            dt.Columns.Add("WIPSNAME");
            dt.Columns.Add("LOT_DETL_TYPE_CODE");
            dt.Columns.Add("LOT_DETL_TYPE_NAME");
            dt.Columns.Add("COLOR");
            dt.Columns.Add("CSTSLOT");
        }

        private bool GetLossCellCheck(string sCellId)
        {
            bool bFlag = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_LOSS_YN_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Rows[0]["EQPT_INPUT_AVAIL_FLAG"].ToString().Equals("T")) //복구불가 셀일경우
                    {
                        //복구불가 Cell은 Dummy 진행 할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0390"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);
                        return false;
                    }
                    else
                    {
                        //Cell ID : {0}는 불량 셀에 등록된 셀입니다. 복구 후 진행하시겠습니까?
                        Util.MessageConfirm("FM_ME_0357", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                bFlag = true;
                            }
                            else
                            {
                                //불량 셀 등록 Cell은 Dummy 진행 할 수 없습니다.
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0358"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, isAutoClosing: true);

                                bFlag = false;
                            }
                        }, new string[] { sCellId });
                    }
                }
                return bFlag;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

        private void GetTrayType(string sTray, ref string sTrayType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = sTray;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_F_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    sTrayType = SearchResult.Rows[0]["TRAY_TYPE_CODE"].ToString();
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

        private void SetEmptyCellRow()
        {
            try
            {
                DataTable dt;
                DataRow dr;

                if (dgCell.GetRowCount() == 0)
                {
                    InitGrid();

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgCell.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["SUBLOTID"] = "0000000000";
                    temp.AcceptChanges();

                    dgCell.ItemsSource = DataTableConverter.Convert(temp);
                }
                else
                {
                    dt = DataTableConverter.Convert(dgCell.ItemsSource);
                    dr = dt.NewRow();
                    dt.Rows.Add(dr);
                    dgCell.ItemsSource = DataTableConverter.Convert(dt);

                    //추가한 Row 자동 세팅
                    DataTable temp = DataTableConverter.Convert(dgCell.ItemsSource);

                    temp.Rows[temp.Rows.Count - 1]["SUBLOTID"] = "0000000000";
                    temp.AcceptChanges();

                    dgCell.ItemsSource = DataTableConverter.Convert(temp);
                }

                // 스프레드 스크롤 하단으로 이동
                dgCell.ScrollIntoView(dgCell.GetRowCount() - 1, 1);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool GetCheckTrayStatus(string sTray, out string sResultMsg)
        {
            bool bCheck = false;
            sResultMsg = string.Empty;

            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.Columns.Add("CSTID", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sTray;
                dr["WIPSTAT"] = "WAIT,PROC";
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CHK_TRAY_STATUS", "INDATA", "OUTDATA", ds);

                switch (dsRslt.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString())
                {
                    case "0":   //정상
                        bCheck = false;
                        break;

                    case "1":   //재공있음
                        bCheck = true;
                        sResultMsg = "FM_ME_0207";  //재공이 존재하는 Tray입니다.
                        break;

                    case "2":   //Tray 정상상태 아님
                        bCheck = true;
                        sResultMsg = "FM_ME_0379";  //Tray상태가 정상이 아닙니다.
                        break;

                    case "3":   //Tray 정보없음 - New Tray는 Dummy 생성 가능
                        bCheck = false;
                        break;
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
            return bCheck;
        }

        private void InitGrid()
        {
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("SUBLOTID", typeof(string));
            dtTable.Columns.Add("FINL_JUDG_CODE", typeof(string));
            dtTable.Columns.Add("WIPSTAT", typeof(string));
            dtTable.Columns.Add("WIPSNAME", typeof(string));
            dtTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
            dtTable.Columns.Add("LOT_DETL_TYPE_NAME", typeof(string));
            dtTable.Columns.Add("COLOR", typeof(string));
            dtTable.Columns.Add("CSTSLOT", typeof(string));

            DataRow dr = dtTable.NewRow();

            dtTable.Rows.Add(dr);

            dgCell.ItemsSource = DataTableConverter.Convert(dtTable);
        }

        private bool GetCellCheck(string sCellId)
        {
            bool bCheck = true;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                    bCheck = false;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return bCheck;
        }

        private void GetCellCheck(string sCellId, ref string sWipStat, ref string sWipSName, ref string sLotDetlTypeCode, ref string sLotDetlTypeName, ref string sColor, ref string sCstsLOT)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    sWipStat = SearchResult.Rows[0]["WIPSTAT"].ToString();
                    sWipSName = SearchResult.Rows[0]["WIPSNAME"].ToString();
                    sLotDetlTypeCode = SearchResult.Rows[0]["LOT_DETL_TYPE_CODE"].ToString();
                    sLotDetlTypeName = SearchResult.Rows[0]["LOT_DETL_TYPE_NAME"].ToString();
                    sColor = SearchResult.Rows[0]["COLOR"].ToString();
                    sCstsLOT = SearchResult.Rows[0]["CSTSLOT"].ToString();
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
        
        private bool GetCellAssyCheck(string sCellId)
        {
            try
            {
                //TC_MLB_CELL 존재여부 확인
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_MLB_CELL_CHECK_CELL_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    return true;
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

            return false;
        }

        private bool GetBadCellCheck(string sCellId, ref DataTable dtGrade)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                //TC_CELL_SCRAP테이블 삭제하고, SUBLOT테이블의 SUBLOTSCRAP을 일단 사용하는걸로.. 2020/12/14 WITH 정종덕
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SUBLOT_F_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    if (SearchResult.Rows[0]["SUBLOTSCRAP"].ToString() == "Y")
                    {
                        //폐기 Cell은 추가할 수 없습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0388"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtCellInput.SelectAll();
                                txtCellInput.Focus();
                            }
                        });
                        return false;
                    }
                }

                SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_A_MB", "RQSTDT", "RSLTDT", RQSTDT);

                dtGrade = SearchResult;

                DataTable dtDfct = GetDfctGRD();



                if (SearchResult.Rows.Count > 0 && dtDfct.Rows.Count > 0)
                {
                 
                    for (int i = 0; i < dtDfct.Rows.Count; i++)
                    {
                        if (SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals(dtDfct.Rows[i]["DFCT_CODE"].ToString()))

                        {
                            //[%]등급 Cell은 추가할 수 없습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0361", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtCellInput.SelectAll();
                                    txtCellInput.Focus();
                                }
                            });
                            return false;
                        }
                    }
                }

                //200330 KJE : A등급 Dummy 금지 Validation
                else if (!isAOK && SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("A"))
                {
                    //[%]등급 Cell은 추가할 수 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0361", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }

                if (SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["MVDAY_JUDG"].ToString().Trim().Equals("NG"))
                {
                    //mv/Day NG Cell은 추가할 수 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0052"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    //Util.SetTextBoxReadOnly(txtTrayCellID, string.Empty); -> 이기 머꼬?
                    return false;
                }

                ////Degas 전 T등급 추가 시 반드시 재작업 Route(자동) 체크되어있어야 함
                //if (SearchResult.Rows.Count > 0 && SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim().Equals("T"))
                //{
                //    DataTable dtReq = new DataTable();
                //    dtReq.TableName = "RQSTDT";
                //    dtReq.Columns.Add("SUBLOTID", typeof(string));
                //    dtReq.Columns.Add("DEGAS_B_A", typeof(string));

                //    DataRow dr1 = dtReq.NewRow();
                //    dr1["SUBLOTID"] = sCellId;
                //    dtReq.Rows.Add(dr1);

                //    ShowLoadingIndicator();
                //    DataTable dtDegasBA = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHK_DEGAS_B_A_MB", "RQSTDT", "RSLTDT", dtReq);

                //    if (dtDegasBA.Rows.Count > 0)
                //    {
                //        isReworkRouteChk = true;
                //        chkDegasOnly.IsChecked = false; //모든 Route 체크 해제
                //        if (chkReRoute.IsChecked == false)
                //        {
                //            //[%]등급 Cell 추가 시 반드시 재작업 Route(자동)에 체크해주세요.
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0399", SearchResult.Rows[0]["FINL_JUDG_CODE"].ToString().Trim()), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //            {
                //                if (result == MessageBoxResult.OK)
                //                {
                //                    txtCellInput.SelectAll();
                //                    txtCellInput.Focus();
                //                }
                //            });
                //            return false;
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return true;
        }

 




        private bool GetReWorkCheck(string sCellId, string sRouteId, string sTrayType)
        {
            bool isSuccess = false;
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("SUBLOTID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["ROUTID"] = sRouteId;
                dr["CST_TYPE_CODE"] = sTrayType;

                INDATA.Rows.Add(dr);
                inDataSet.Tables.Add(INDATA);

                ShowLoadingIndicator();
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_CHK_REWORK_ROUTE_MB", "INDATA", "OUTDATA", inDataSet, null);

                if (dsResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                {
                    //정상 CELL   
                    isSuccess = true;
                }
                else
                {
                    // 불량 CELL                    
                    isSuccess = false;
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

            return isSuccess;
        }

        private bool GetFirstCellCheck(string sCellId, string sTrayType)
        {
            bool isSuccess = true;

            try
            {
                LOT_DETL_TYPE_CODE = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL_MB", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    LOT_DETL_TYPE_CODE = SearchResult.Rows[0]["LOT_DETL_TYPE_CODE"].ToString().Trim();

                    cboDummyLineID.SelectedValue = SearchResult.Rows[0]["EQSGID"].ToString().Trim();

                    SetlstRoute(SearchResult.Rows[0]["LOTID"].ToString().Trim(), txtDummyTrayID.Text.ToString(), sTrayType, SearchResult.Rows[0]["ROUT_TYPE_CODE"].ToString().Trim());

                    // [CSR ID:2361359] LOTID 비교 완료후 LOTID 입력하기 위하여 정합성 확인 후로 수정함. //정상 
                    if (SearchResult.Rows.Count != 0)
                    {
                        txtDummyModel.Text = SearchResult.Rows[0]["MDLLOT_ID"].ToString();
                        txtDummyLotID.Text = SearchResult.Rows[0]["LOTID"].ToString().Trim();
                        txtDummyProdCD.Text = SearchResult.Rows[0]["PRODID"].ToString().Trim();
                    }
                }           
            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                isSuccess = false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return isSuccess;
        }

        private bool SetStorageRoute(string sCellId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_SEL_STORAGE_ROUTE_MB", "RQSTDT", "RSLTDT", RQSTDT);

                //저장 Route정보가 없다면 알람 후 종료
                if (SearchResult.Rows.Count == 0)
                {
                    //해당 Cell에 대한 저장 Route가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0359"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }
                else
                {
                    lstRoute.ItemsSource = SearchResult.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMCDNAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
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

            return true;
        }

        private void SetlstRoute(string sLotId, string sTrayId, string sTrayTypeCode, string sRouteTypeCd = null)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("ROUT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotId;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dr["ROUT_TYPE_CODE"] = sRouteTypeCd;
                dr["CST_TYPE_CODE"] = sTrayTypeCode;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_RETRIEVE_DUMMY_LOT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    lstRoute.ItemsSource = dtRslt.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMCDNAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
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

        private void SetIsWReWorkRoute(string sLotID, string sTrayID, string sTrayTypeCode)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotID;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dr["CST_TYPE_CODE"] = sTrayTypeCode;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUTE_SEARCH_W_REWORK_ROUTE_MB", "RQSTDT", "RSLTDT", dtRqst);

                //W등급 재작업 기준정보가 없다면 알람 후 종료
                if (dtRslt.Rows.Count == 0)
                {
                    //저전압 재작업 라우트 기준정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0216"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return;
                }
                else
                {
                    lstRoute.ItemsSource = dtRslt.AsDataView();
                    lstRoute.SelectedValuePath = "ROUTID";
                    lstRoute.DisplayMemberPath = "CMN_CD_NAME";

                    if (lstRoute.Items.Count > 0)
                        lstRoute.SelectedIndex = 0;
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

        private bool SetIsReWorkRoute(string sCellId, string sTrayId, string sLotId, string sTrayType)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = ds.Tables.Add("INDATA");

                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SUBLOTID", typeof(string));
                dt.Columns.Add("CST_TYPE_CODE", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellId;
                dr["CST_TYPE_CODE"] = sTrayType;
                dr["LOTID"] = sLotId;
                dr["EQSGID"] = cboDummyLineID.SelectedValue.ToString();
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_REWORK_ROUTE_MB", "INDATA", "OUTDATA", ds, null);

                //재작업 기준정보가 없다면 알람 후 종료
                if (dsResult.Tables["OUTDATA"].Rows.Count == 0)
                {
                    //재작업 라우트 기준정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0209"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtCellInput.SelectAll();
                            txtCellInput.Focus();
                        }
                    });
                    return false;
                }
                else if (dsResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("-1"))
                {
                    isReworkRouteChk = false;
                }

                lstRoute.ItemsSource = dsResult.Tables["OUTDATA"].AsDataView();
                lstRoute.SelectedValuePath = "ROUTID";
                lstRoute.DisplayMemberPath = "CMCDNAME";

                if (lstRoute.Items.Count > 0)
                    lstRoute.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

            return true;
        }

        private bool GetSecondLotCheck(string sCellId)
        {
            bool isSuccess = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 ||
                    dtRslt.Rows[0]["LOTID"].ToString().Length < 9 ||
                    txtDummyLotID.Text.Length < 9 ||
                    !dtRslt.Rows[0]["LOTID"].ToString().Substring(0, 9).Equals(txtDummyLotID.Text.Substring(0, 9)))
                {
                    isSuccess = false;
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
            return isSuccess;
        }

        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 START
        private bool GetSecondLotCheckDayGrLotID(string sCellId)
        {
            bool isSuccess = true;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = sCellId;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_FIRST_CELL_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 || !dtRslt.Rows[0]["DAY_GR_LOTID"].ToString().Equals(txtDummyLotID.Text.Substring(0, 8)))
                {
                    isSuccess = false;
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
            return isSuccess;
        }
        // 2021.09.02  강동희: Lot 혼입 시 Lot ID 8자리 체크로직 추가 END

        //LOTTYPE이 다를경우 Dummy 생성 불가
        private bool GetLotTypeCheck(string sCellID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTLIST"] = dgCell.GetCell(0, 0).Text.ToString() + "," + sCellID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_LOTTYPE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 2) return true;
                else
                {
                    if (dtRslt.Rows[0]["LOTTYPE"].ToString().Equals(dtRslt.Rows[1]["LOTTYPE"].ToString()))
                    {
                        return true;
                    }
                    else return false;
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
            return true;
        }

        private bool GetLotDetlTypeCheck(string sCellID)
        {
            try
            {
                // 기존 첫번째 Row 와 비교시 첫번째가 Dummy 일때 비교불가하여 더미가 아닌 로우중 첫번째를 찾아 비교.
                string firstCellId = string.Empty;
                for (int row = 0; row < dgCell.Rows.Count; row++)
                {
                    for (int col = 0; col < dgCell.Columns.Count; col++)
                    {
                        if (string.IsNullOrEmpty(Util.NVC(dgCell.GetCell(row, col).Text)) || Util.NVC(dgCell.GetCell(row, col).Text) == "0000000000") continue;

                        firstCellId = Util.NVC(dgCell.GetCell(row, col).Text);
                        break;
                    }

                    if (!string.IsNullOrEmpty(firstCellId)) break;
                }

                if (string.IsNullOrEmpty(firstCellId)) return true;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTLIST", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTLIST"] = firstCellId + "," + sCellID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_CHECK_LOT_DETL_TYPE_CODE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count != 2) return true;
                else
                {
                    if (dtRslt.Rows[0]["LOT_DETL_TYPE_CODE"].ToString().Equals(dtRslt.Rows[1]["LOT_DETL_TYPE_CODE"].ToString()))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
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
            return true;
        }

        private void GetTrayInfo(bool pOpCheck)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTrayID.Text) || txtTrayID.Text.Length < 10)
                {
                    //Tray ID를 정확히 입력해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0071"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayID.SelectAll();
                            txtTrayID.Focus();

                        }
                    });
                    return;
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = txtTrayID.Text;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_INFO_RETRIEVE_MB", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count == 0)
                {
                    //Tray 정보가 존재하지 않습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0078"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtTrayID.SelectAll();
                            txtTrayID.Focus();

                            txtSTrayID.Text = string.Empty; 
                            txtOp.Text = string.Empty;
                            txtStatus.Text = string.Empty;
                            txtIssRsvFlag.Text = string.Empty;
                            cboOp.ItemsSource = null;
                            cboOp.SelectedIndex = 0;

                        }
                    });
                    return;
                }
                else
                {
                    txtSTrayID.Text = dtRslt.Rows[0]["CSTID"].ToString();
                    txtPROCID.Text = dtRslt.Rows[0]["PROCID"].ToString();
                    txtOp.Text = dtRslt.Rows[0]["PROCNAME"].ToString();
                    txtROUT.Text = dtRslt.Rows[0]["ROUTID"].ToString();
                    txtStatus.Text = dtRslt.Rows[0]["WIPSNAME"].ToString();
                    //출고예약상태가 분리됨.
                    txtIssRsvFlag.Text = dtRslt.Rows[0]["ISS_RSV_FLAG"].ToString();
                    txtTrayID.Foreground = Brushes.Black;
                    txtEQP.Text = dtRslt.Rows[0]["EQPTID"].ToString();

                    if (dtRslt.Rows[0]["DUMMY_FLAG"].ToString().Equals("Y"))
                    {
                        txtTrayID.Foreground = Brushes.Blue;
                    }
                    if (dtRslt.Rows[0]["SPCL_TYPE_CODE"].ToString().Equals("Y"))
                    {
                        txtTrayID.Foreground = Brushes.Red;
                    }
                    else if (dtRslt.Rows[0]["SPCL_TYPE_CODE"].ToString().Equals("I"))
                    {
                        txtTrayID.Foreground = Brushes.DarkOrange;
                    }

                    CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
                    string sProcList = GetRoutProcByProcGRCode(dtRslt.Rows[0]["ROUTID"].ToString());
                    string[] sFilter = { dtRslt.Rows[0]["ROUTID"].ToString(), null, null, null, sProcList };
                    _combo.SetCombo(cboOp, CommonCombo_Form_MB.ComboStatus.NONE, sFilter: sFilter, sCase: "ROUTE_OP");
                    cboOp.SelectedValue = dtRslt.Rows[0]["PROCID"].ToString();
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

        private string GetRoutProcByProcGRCode(string sRoutID)
        {
            string sRtnValue = string.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ROUTID", typeof(string));
                dtRqst.Columns.Add("PROC_GRP_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = sRoutID;
                dr["PROC_GRP_CODE"] = "A";
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ROUT_PROC_BY_PROCGRP_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    foreach (DataRow drRslt in dtRslt.Rows)
                    {
                        sRtnValue += drRslt["PROCID"].ToString() + ",";
                    }
                    sRtnValue = sRtnValue.Remove(sRtnValue.LastIndexOf(","));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
        }

        private bool CheckExistWipCell(string pTrayID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = pTrayID;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_CHK_EXIST_WIP_CELL_MB", "RQSTDT", "RSLTDT", dtRqst);


                if (dtRslt.Rows.Count == 0) return false;

                if (dtRslt.Rows[0]["RETVAL"].ToString().Equals("1"))
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
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

            return false;
        }

        private void GetEqpCurTray()
        {
            try
            {
                txtLowerTray.Text = string.Empty;
                txtUpperTray.Text = string.Empty;

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("ROW_LOC", typeof(string));
                dtRqst.Columns.Add("COL_LOC", typeof(string));
                dtRqst.Columns.Add("STG_LOC", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANE_ID"] = Util.GetCondition(cboLane);
                dr["ROW_LOC"] = Util.GetCondition(cboRow);
                dr["COL_LOC"] = Util.GetCondition(cboCol);
                dr["STG_LOC"] = Util.GetCondition(cboStg);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CURRENT_TRAY", "RQSTDT", "RSLTDT", dtRqst);

                foreach (DataRow drRslt in dtRslt.Rows)
                {
                    if (drRslt["CST_LOAD_LOCATION_CODE"].ToString().Equals("1"))
                    {
                        txtLowerTray.Text = drRslt["CSTID"].ToString();
                    }
                    else
                    {
                        txtUpperTray.Text = drRslt["CSTID"].ToString();
                    }
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

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON { FrameOperation = FrameOperation };

            object[] parameters = new object[1];
            string userName;

            userName = txtUserName.Text;

            parameters[0] = userName;
            C1WindowExtension.SetParameters(wndPerson, parameters);

            wndPerson.Closed += new EventHandler(wndUser_Closed);
            grdMain.Children.Add(wndPerson);
            wndPerson.BringToFront();
        }

        private void SetTrayType(C1DataGrid dg, string sPalletTypeCd)
        {
            if (sPalletTypeCd.Equals("1"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, false);

                dg.GetCell(21, 0).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 2).Presenter.Background = new SolidColorBrush(Colors.DarkGray);

            }
            else if (sPalletTypeCd.Equals("2"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, false);

                dg.GetCell(21, 1).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 3).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
            }
            else if (sPalletTypeCd.Equals("3"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //24개 ROW 생성
                for (int i = 0; i < 24; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, false);
            }
            else if (sPalletTypeCd.Equals("4"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //16개 ROW 생성
                for (int i = 0; i < 16; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, false);
            }
            else if (sPalletTypeCd.Equals("5"))
            {
                DataTable dt = new DataTable();


                string[] sParamArr = { "A", "B", "C", "D", "E", "F", "G" };
                for (int i = 0; i < sParamArr.Length; i++)
                {
                    dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                    {
                        Name = sParamArr[i],
                        Header = sParamArr[i],
                        Binding = new Binding()
                        {
                            Path = new PropertyPath(sParamArr[i]),
                            Mode = BindingMode.TwoWay
                        },
                        TextWrapping = TextWrapping.Wrap,
                        IsReadOnly = true,
                        Width = new C1.WPF.DataGrid.DataGridLength(100, C1.WPF.DataGrid.DataGridUnitType.Star)
                    });

                    dt.Columns.Add(sParamArr[i], typeof(string));
                }

                //22개 ROW 생성
                for (int i = 0; i < 22; i++)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                //Data Binding
                Util.GridSetData(dg, dt, FrameOperation, false);

                dg.GetCell(21, 1).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 3).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
                dg.GetCell(21, 5).Presenter.Background = new SolidColorBrush(Colors.DarkGray);
            }
        }

        private void InitializeDataGrid(string sCSTID, C1DataGrid dg)
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CSTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = sCSTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_GRID_SET_INFO_MB", "INDATA", "OUTDATA", RQSTDT); // 20230207 개발필요.
                Util.gridClear(dg);

                if (dtRslt.Rows.Count > 0)
                {
                    int iColName = 65;
                    string sRowCnt = dtRslt.Rows[0]["ATTR1"].ToString();
                    string sColCnt = dtRslt.Rows[0]["ATTR2"].ToString();
                    _sNotUseRowLIst = dtRslt.Rows[0]["ATTR3"].ToString();
                    _sNotUseColLIst = dtRslt.Rows[0]["ATTR4"].ToString();

                    #region Grid 초기화
                    int iMaxCol;
                    int iMaxRow;
                    List<string> rowList = new List<string>();

                    int iColCount = dgCell.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        int index = (iColCount - i) - 1;
                        dgCell.Columns.RemoveAt(index);
                    }

                    iMaxRow = Convert.ToInt16(sRowCnt);
                    iMaxCol = Convert.ToInt16(sColCnt);

                    List<DataTable> dtList = new List<DataTable>();

                    double AAA = Math.Round((dgCell.ActualWidth - 70) / (iMaxCol - 1), 1);
                    int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

                    int iSeq = 1;
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";

                    for (int iCol = 0; iCol < iMaxCol; iCol++)
                    {
                        SetGridHeaderSingle(Convert.ToChar(iColName + iCol).ToString(), dg, iColWidth);
                        dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString(), typeof(string));

                        if (iCol == 0)
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                DataRow row1 = dt.NewRow();

                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                                dt.Rows.Add(row1);
                            }
                        }
                        else
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                            }
                        }
                    }

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson != null && wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserID.Text = wndPerson.USERID;
            }
        }

        private void SetSubLotList()
        {
            _SubLotList.TableName = "LIST";
            _SubLotList.Columns.Add("SUBLOTID", typeof(string));
            _SubLotList.Columns.Add("CSTSLOT", typeof(string));
        }

        private int CellCountCheck()
        {
            int cellCount = 0;
            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                for (int j = 0; j < dgCell.Columns.Count; j++)
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dgCell.GetCell(i, j).Text)))
                    {
                        cellCount++;
                    }
                }
            }
            return cellCount;
        }

        private void SetIROCVDummy()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("TRAY_ID", typeof(string));
                dtRqst.Columns.Add("OP_ID", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));
                dtRqst.Columns.Add("MDF_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["TRAY_ID"] = Util.GetCondition(txtSTrayID);
                dr["OP_ID"] = Util.GetCondition(cboOp);
                dr["GRADE_CD"] = Util.GetCondition(cboDummyGrade);
                dr["MDF_ID"] = LoginInfo.USERID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_TRAY_IROCV_DUMMY_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows[0]["RETVAL"].ToString() != "0")
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    //Util.AlertMsg("등급 더미 변경 중 오류가 발생하였습니다.");
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        private DataTable GetDfctGRD()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["DFCT_TYPE_CODE"] = "N";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_DFCT_CODE_BY_TYPE_MB", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        #endregion

        private void btnRsv_Click(object sender, RoutedEventArgs e)
        {
            if (chkReservation.IsChecked == true || chkCPFReservation.IsChecked == true)
            {
                try
                {
                    bool Rsv_Flag = false;
                    DataTable rsdt = new DataTable();
                    //Tray 정보를 변경하시겠습니까?
                    Util.MessageConfirm("FM_ME_0312", (resultMessage) =>
                    {
                        if (resultMessage == MessageBoxResult.OK)
                        {
                            //ShowLoadingIndicator();
                                                        
                            GetToEqptID();

                            DataTable RQSTDT1 = new DataTable();
                            RQSTDT1.TableName = "RQSTDT";
                            RQSTDT1.Columns.Add("PROCID", typeof(string));
                            RQSTDT1.Columns.Add("CSTID", typeof(string));
                            RQSTDT1.Columns.Add("EQPTID", typeof(string));
                            RQSTDT1.Columns.Add("ROUTID", typeof(string));
                            RQSTDT1.Columns.Add("AREAID", typeof(string));


                            DataRow newRow1 = RQSTDT1.NewRow();
                            newRow1["PROCID"] = Util.GetCondition(cboOp);
                            newRow1["CSTID"] = txtTrayID.Text;
                            newRow1["EQPTID"] = _sToEQP;
                            newRow1["ROUTID"] = txtROUT.Text;
                            newRow1["AREAID"] = LoginInfo.CFG_AREA_ID;
                            RQSTDT1.Rows.Add(newRow1);

                            DataTable dtResultCK = new ClientProxy().ExecuteServiceSync("BR_GET_TRF_CMD_ENABLE_MB", "RQSTDT", "RSLTDT", RQSTDT1);

                            switch (dtResultCK.Rows[0]["RETVAL"].ToString())
                            {
                                case "0":
                                    // 수동예약 가능 
                                    Rsv_Flag = true;
                                    break;

                                case "1":
                                    // 공정 정보가 없습니다.
                                    Util.Alert("SFU1456");
                                    break;

                                case "2":
                                    // BOX/MODEL 정보가 맞지 않습니다.
                                    Util.Alert("FM_ME_0522");
                                    break;

                                case "3":
                                    // 동일한 TRAY의 반송명령이 존재합니다.
                                    Util.Alert("FM_ME_0523");
                                    break;

                                case "4":

                                    // 작업 시작 가능한 설비 상태가 아닙니다. 설비 상태 확인하세요.
                                    // Box LR 상태확인
                                    Util.Alert("3003");
                                    break;
                                case "5":

                                    // 이미 예약된 TRAY입니다.
                                    Util.Alert("FM_ME_0532");
                                    break;


                                default:
                                    //Tray 정보 조회 중 오류가 발생하였습니다.

                                    Util.Alert("FM_ME_0077");
                                    break;
                            }
                        }

                        if (Rsv_Flag == true)
                        {
                            // 수동예약 
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "IN_DATA";

                            dtRqst.Columns.Add("EQUIPMENT_ID", typeof(string));
                            dtRqst.Columns.Add("PORT_ID", typeof(string));
                            dtRqst.Columns.Add("DURABLE_ID", typeof(string));
                            dtRqst.Columns.Add("USER_ID", typeof(string));
                            dtRqst.Columns.Add("SRCTYPE", typeof(string));

                            DataRow drIn = dtRqst.NewRow();
                            drIn["USER_ID"] = LoginInfo.USERID;
                            drIn["EQUIPMENT_ID"] = _sToEQP;
                            drIn["DURABLE_ID"] = txtTrayID.Text;
                            drIn["PORT_ID"] = _sToPort;
                            drIn["SRCTYPE"] = "UI";
                            dtRqst.Rows.Add(drIn);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_MHS_ACT_PORT_RESERVE_TRANSFER_CST", "IN_DATA", null, dtRqst);


                            //변경완료하였습니다
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0533"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                }
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }
            }
        }
    }
}
