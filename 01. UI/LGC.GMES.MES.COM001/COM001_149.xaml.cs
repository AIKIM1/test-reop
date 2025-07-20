using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using C1.WPF.DataGrid.Summaries;
using System.Collections;
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_149.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_149 : UserControl, IWorkArea
    {
        Util _Util = new Util();
        string sEqsg_ID = string.Empty;
        string sRownum_ID = string.Empty;
        string sBiz_Type = string.Empty;

        string Qrcode = string.Empty;
        
        private DataSet dsResult = new DataSet();
        List<string> _Rtlslist = new List<string>();
        List<string> _RtlsDefect = new List<string>();
        List<string> _RtlsJudge = new List<string>();
        List<string> _RtlsEqsg = new List<string>();
        List<string> _RtlsProc = new List<string>();
        List<string> _RtlsProd = new List<string>();

        string sCellJudg = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public COM001_149()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

            #region Tab 1 Load
            tabControlMain.SelectedIndex = 0;
            #endregion

            #region Tab 2 Lodad
            tabControlMain.SelectedIndex = 1;
            dtpFDate.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1 - DateTime.Now.Day);
            dtpFDate_3.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(1 - DateTime.Now.Day);
            dtpFDate.IsNullInitValue = true;
            dtpFDate_3.IsNullInitValue = true;
            Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
            Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");
            InitCombo();
            fn_Init_RtlsMultiCmb();

            #endregion

            #region Tab 3 Load
            tabControlMain.SelectedIndex = 2;
            InitCombo_3();
            InitGridColumns();

            #endregion

            #region Tab 4 Load
            tabControlMain.SelectedIndex = 3;
            InitCombo_4();
            #endregion

            #region Tab 5 Load
            tabControlMain.SelectedIndex = 4;
            InitCombo_5();
            #endregion


            Loaded -= UserControl_Loaded;
            tabControlMain.SelectedIndex = 0;
        }



        #region Tab 1

        #region Evnet
        /// LOT Scan 시
        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSearchLotId.Text.Length > 0)
                    {
                        if ((bool)rdoQr.IsChecked)
                        {
                            txtSearchLotId.Text = Qrcode;
                            txtLotId.Text = Qrcode;
                            Qrcode = string.Empty;
                        }

                        Clear_TextBox();
                        SetInfomation(true, txtSearchLotId.Text.Trim());
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void txtSearchLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (txtSearchLotId.Text == string.Empty) Qrcode = string.Empty;
            //if(e.Key != Key.LeftShift && e.Key != Key.RightShift  && e.Key != Key.Enter && e.Key != Key.Capital) 
            //{
            //Qrcode = Qrcode + e.Key;
            switch (e.Key)
            {
                case Key.D0:
                    Qrcode = Qrcode + "0";
                    break;
                case Key.D1:
                    Qrcode = Qrcode + "1";
                    break;
                case Key.D2:
                    Qrcode = Qrcode + "2";
                    break;
                case Key.D3:
                    Qrcode = Qrcode + "3";
                    break;
                case Key.D4:
                    Qrcode = Qrcode + "4";
                    break;
                case Key.D5:
                    Qrcode = Qrcode + "5";
                    break;
                case Key.D6:
                    Qrcode = Qrcode + "6";
                    break;
                case Key.D7:
                    Qrcode = Qrcode + "7";
                    break;
                case Key.D8:
                    Qrcode = Qrcode + "8";
                    break;
                case Key.D9:
                    Qrcode = Qrcode + "9";
                    break;
                case Key.Down:
                    Qrcode = Qrcode + "&";
                    break;
                default:
                    if ((int)e.Key >= 44 && (int)e.Key <= 69)
                        Qrcode = Qrcode + e.Key;

                    if (e.Key == Key.OemMinus)
                        Qrcode = Qrcode + "-";

                    if (e.Key == Key.OemPeriod)
                        Qrcode = Qrcode + ".";

                    if (e.Key == Key.Oem1)
                        Qrcode = Qrcode + ";";

                    if (e.Key == Key.Space)
                        Qrcode = Qrcode + " ";
                    
                    if(e.Key == Key.OemComma)
                        Qrcode = Qrcode + ",";

                    if (e.Key == Key.Oem2)
                        Qrcode = Qrcode + "/";

                    if (e.Key == Key.Oem6)
                        Qrcode = Qrcode + "]";

                    if (e.Key == Key.Oem4)
                        Qrcode = Qrcode + "[";
                    break;
            }
            //}
        }

        /// Tab1 조회 버튼클릭 
        private void btnSearchLotId_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSearchLotId.Text.Length > 0)
                {
                    Clear_TextBox();
                    SetInfomation(true, txtSearchLotId.Text.Trim());
                    //SetLotList();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// Tab1 저장버튼클릭
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });


            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        /// Lot 이력 메뉴 연계
        private void dgRtlsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRtlsList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "SCANLOT")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Lot, Pallet, Cst 라디오버튼 클릭
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (ContentRight != null)
            {
                txtSearchLotId.Text = string.Empty;
                Qrcode = string.Empty;
                txtSearchLotId.PreviewKeyDown -= txtSearchLotId_PreviewKeyDown;
                if ((bool)rdoLot.IsChecked)
                {
                    ContentRight.Visibility = Visibility.Visible;
                    ContentRight_Pallet.Visibility = Visibility.Hidden;
                    dgDefect.ClearRows();
                    dgRtlsList.ClearRows();

                }
                else if((bool)rdoCst.IsChecked || (bool)rdoPallet.IsChecked)
                {
                    ContentRight_Pallet.Visibility = Visibility.Visible;
                    ContentRight.Visibility = Visibility.Hidden;
                    dgMappingLotList.ClearRows();
                }
                else
                {
                    txtSearchLotId.PreviewKeyDown += txtSearchLotId_PreviewKeyDown;
                    ContentRight_Pallet.Visibility = Visibility.Visible;
                    ContentRight.Visibility = Visibility.Hidden;
                    dgMappingLotList.ClearRows();
                }

                Clear_TextBox();
            }
            txtSearchLotId.Focus();
        }


        // CMA, BMA  판정결과 등록 팝업
        private void btnJudgPop_Click(object sender, RoutedEventArgs e)
        {
            //Table 생성
            DataTable dt = null;
            string search_type = string.Empty;
            if (string.IsNullOrEmpty(txtLotId.Text)) return;
            string search_ID = txtLotId.Text;

            if ((bool)rdoLot.IsChecked)
            {
                search_type = "Lot";
            }
            else if ((bool)rdoCst.IsChecked)
            {
                search_type = "Cst";
            }
            else if((bool)rdoPallet.IsChecked)
            {
                search_type = "Pallet";
            }
            else
            {
                search_type = "QRCode";
            }

            COM001_149_JUDG_Save pop_Judg = new COM001_149_JUDG_Save();

            if (pop_Judg != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = search_type;
                Parameters[1] = search_ID;
                C1WindowExtension.SetParameters(pop_Judg, Parameters);

                pop_Judg.Closed += new EventHandler(pop_Judg_Closed);
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(pop_Judg);
                        pop_Judg.BringToFront();
                        break;
                    }
                }
            }
        }

        private void pop_Judg_Closed(object sender, EventArgs e)
        {
            COM001_149_JUDG_Save pop_Judg = sender as COM001_149_JUDG_Save;
            if (pop_Judg.DialogResult == MessageBoxResult.OK)
            {
                txtJudgResult.Text = pop_Judg.RESULT;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(pop_Judg);
                    break;
                }
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// Lot 조회 분배
        /// </summary>
        /// <param name="bMainLot_SubLot_Flag"></param>
        /// <param name="sLotid"></param>
        private void SetInfomation(bool bMainLot_SubLot_Flag, string sLotid)
        {

            if ((bool)rdoLot.IsChecked)
            {
                LotSearch(sLotid);
            }
            if ((bool)rdoPallet.IsChecked)
            {
                PalletSearch(sLotid);
            }
            if ((bool)rdoCst.IsChecked)
            {
                CstSearch(sLotid);
            }

            if ((bool)rdoQr.IsChecked)
            {
                QrCodeSearch(sLotid);
            }
        }

       

        private void LotSearch(string sLotid)
        {
            try
            {

                sCellJudg = string.Empty;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_RTLS_GET_PACK_LOT_INFO", "INDATA", "LOT_INFO,WIP_PROC,WIP_ROUTE,TB_SFC_WIP_INPUT_MTRL_HIST,ACTIVITYREASON,PRODUCTPROCESSQUALSPEC,WIPREASONCOLLECT,EM_LOT_MNGT,EM_LOT_JUDG", (bizResult, bizException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        if (bizResult != null && bizResult.Tables.Count > 0)
                        {
                            if ((bizResult.Tables.IndexOf("LOT_INFO") > -1))
                            {
                                if (bizResult.Tables["LOT_INFO"].Rows.Count > 0)
                                {
                                    txtLotInfoCreateDate.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["LOTDTTM_CR"]);
                                    txtLotInfoLotType.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["LOTTYPE"]);
                                    txtLotInfoProductName.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["PRODNAME"]);
                                    txtLotInfoProductId.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["PRODID"]);
                                    txtLotInfoWipLine.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["EQSGNAME"]);
                                    txtLotInfoModel.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["MODLNAME"]);
                                    txtLotInfoWipProcess.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["PROCNAME"]);
                                    txtLotInfoWipState.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["WIPSNAME"]);
                                    txtInfoBoxFromArea.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["FROM_AREA_NAME"]);
                                    txtInfoBoxFromArea.ToolTip = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["FROM_AREA_NAME"]);

                                    txtLotId.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["LOTID"]);
                                    txtGrade.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["GRADE"]);

                                    //CMA, BMA 판정 콤보 활성화
                                    if (!Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["PRDT_CLSS_CODE"]).Equals("CELL") && !Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["WIPSTAT"]).Equals("TERM"))
                                    {
                                        btnJudgPop.IsEnabled = true;
                                        txtJudgResult.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["JUDGNAME"]);
                                        txtJudgRemark.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["JUDG_NOTE"]);
                                        txtJudgResult_1.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["EM_JUDG_RSLT_CODE_1"]);
                                        txtJudgResult_2.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["EM_JUDG_RSLT_CODE_2"]);
                                        txtJudgSeq.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["JUDG_SEQ"]);
                                        if (Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["JUDG_NOTE"]) != "")
                                        {
                                            txtJudgResult.ToolTip = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["JUDG_NOTE"]);
                                        }
                                        else
                                        {
                                            txtJudgResult.ToolTip = "";
                                        }

                                    }
                                    else
                                    {
                                        btnJudgPop.IsEnabled = false;
                                    }

                                    if (Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_YN"]) == "Y")
                                    {
                                        txtHoldYN.Text = "Y";
                                        txtHoldYN.ToolTip = "HOLD_RESN  : " + bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_RESN"].ToString() + "\n" +
                                                            "HOLD_DTTM : " + bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_DTTM"].ToString() + "\n" +
                                                            "HOLD_CODE : " + bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_CODE"].ToString() + "\n" +
                                                            "HOLD_NOTE : " + bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_NOTE"].ToString().Trim();
                                        txtHold.Text = Util.NVC(bizResult.Tables["LOT_INFO"].Rows[0]["HOLD_REMARK"]);
                                    }
                                    else
                                    {
                                        txtHoldYN.Text = "N";
                                        txtHold.Text = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                                        txtHoldYN.ToolTip = ObjectDic.Instance.GetObjectName("홀드 상태가 아닙니다.");
                                    }
                                }
                            }

                            if ((bizResult.Tables.IndexOf("ACTIVITYREASON") > -1))
                            {
                                Util.GridSetData(dgDefect, bizResult.Tables["ACTIVITYREASON"], FrameOperation, true);
                            }

                            if (bizResult.Tables["EM_LOT_MNGT"].Rows.Count > 0)
                            {
                                txtDefept.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["RESNNAME"]);

                                txtRemark.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["NOTE"]);
                                txtClctitem.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["ITEM_LIST"]);
                                txtClctitemValue.Text = Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["VALUE_LIST"]);

                                if (dgDefect.Rows.Count > 0)
                                {
                                    for (int i = 0; i < dgDefect.Rows.Count; i++)
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE")).ToString() == Util.NVC(bizResult.Tables["EM_LOT_MNGT"].Rows[0]["FINL_EM_DFCT_CODE"]))
                                        {
                                            DataTableConverter.SetValue(dgDefect.Rows[i].DataItem, "CHK", true);
                                        }
                                    }
                                }
                            }
                            if (bizResult.Tables["EM_LOT_JUDG"].Rows.Count > 0)
                            {
                                sCellJudg = Util.NVC(bizResult.Tables["EM_LOT_JUDG"].Rows[0]["JUDGCODE"]);
                                txtRTLS.Text = Util.NVC(bizResult.Tables["EM_LOT_JUDG"].Rows[0]["JUDGNAME"]);

                                if (bizResult.Tables["LOT_INFO"].Rows.Count < 1)
                                {
                                    btnJudgPop.IsEnabled = false;
                                }
                            }

                            _Rtlslist.Add(sLotid);
                            _RtlsEqsg.Add(txtLotInfoWipLine.Text);
                            _RtlsProc.Add(txtLotInfoWipProcess.Text);
                            _RtlsDefect.Add(txtDefept.Text);
                            _RtlsProd.Add(txtLotInfoProductId.Text);

                            SetLotList();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });

            }
            finally
            {
                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();

            }
        }
        private void PalletSearch(string PalletID)
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");

                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLET_ID", typeof(string));

                DataRow drRQSTDT = RQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PALLET_ID"] = PalletID;
                RQSTDT.Rows.Add(drRQSTDT);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_PALLET_INFO", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dt.Rows.Count < 1)
                    {
                        Util.Alert("SFU1905");
                        return;
                    }
                    DataRow[] chkRow = dt.Select("PRDT_CLSS_CODE <> 'CELL' AND WIPSTAT <> 'TERM'", "");

                    if (chkRow.Length > 0)
                    {
                        btnJudgPop.IsEnabled = true;
                    }
                    else
                    {
                        btnJudgPop.IsEnabled = false;
                    }
                    Util.SetTextBlockText_DataGridRowCount(tbListCount_Pallet, Util.NVC(dt.Rows.Count));
                    Util.GridSetData(dgMappingLotList, dt, FrameOperation, true);
                    txtLotId.Text = PalletID;
                 
                });
            }
            catch (Exception ex)
            {

                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });
            }
            finally
            {
                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();

            }
        }
        private void CstSearch(string CstID)
        {

            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");

                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));

                DataRow drRQSTDT = RQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["PANCAKE_GR_ID"] = CstID;
                RQSTDT.Rows.Add(drRQSTDT);


                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_CST_INFO", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dt.Rows.Count < 1)
                    {
                        Util.Alert("SFU1905");
                        return;
                    }
                    DataRow[] chkRow = dt.Select("PRDT_CLSS_CODE <> 'CELL' AND WIPSTAT <> 'TERM'", "");

                    if (chkRow.Length > 0)
                    {
                        btnJudgPop.IsEnabled = true;
                    }
                    else
                    {
                        btnJudgPop.IsEnabled = false;
                    }
                    Util.SetTextBlockText_DataGridRowCount(tbListCount_Pallet, Util.NVC(dt.Rows.Count));
                    Util.GridSetData(dgMappingLotList, dt, FrameOperation, true);
                    txtLotId.Text = CstID;
           
                });
            }
            catch (Exception ex)
            {

                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });
            }
            finally
            {
                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();

            }
        }
        private void QrCodeSearch(string sLotid)
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");

                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("QRCODE", typeof(string));

                DataRow drRQSTDT = RQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["QRCODE"] = sLotid;
                RQSTDT.Rows.Add(drRQSTDT);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_RTLS_GET_QRCODE_INFO", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dt.Rows.Count < 1)
                    {
                        Util.Alert("SFU1905");
                        return;
                    }
                    DataRow[] chkRow = dt.Select("PRDT_CLSS_CODE <> 'CELL' AND WIPSTAT <> 'TERM'", "");

                    if (chkRow.Length > 0)
                    {
                        btnJudgPop.IsEnabled = true;
                    }
                    else
                    {
                        btnJudgPop.IsEnabled = false;
                    }
                    Util.SetTextBlockText_DataGridRowCount(tbListCount_Pallet, Util.NVC(dt.Rows.Count));
                    Util.GridSetData(dgMappingLotList, dt, FrameOperation, true);
                    txtLotId.Text = sLotid;
                });
            }
            catch (Exception ex)
            {

                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtSearchLotId.SelectAll();
                        txtSearchLotId.Focus();
                    }
                });
            }
            finally
            {
                txtSearchLotId.SelectAll();
                txtSearchLotId.Focus();

            }
        }
        /// <summary>
        /// 조회이력 그리드 출력
        /// </summary>
        private void SetLotList()
        {
            DataTable dt = new DataTable();
            dt.TableName = "RQSTDT";
            dt.Columns.Add("SCANLOT", typeof(string));
            dt.Columns.Add("EQSGID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("PRODID", typeof(string));
            dt.Columns.Add("DEFECT", typeof(string));

            for (int i = 0; i < _Rtlslist.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dr["SCANLOT"] = _Rtlslist[i].ToString();
                dr["EQSGID"] = _RtlsEqsg[i].ToString();
                dr["PROCID"] = _RtlsProc[i].ToString();
                dr["PRODID"] = _RtlsProd[i].ToString();
                dr["DEFECT"] = _RtlsDefect[i].ToString();
                dt.Rows.Add(dr);
            }

            Util.GridSetData(dgRtlsList, dt, FrameOperation, true);

        }

        /// <summary>
        /// 저장 분배
        /// </summary>
        private void Save()
        {

            if ((bool)rdoLot.IsChecked)
            {
                if (txtLotId.Text.Trim() == "")
                {
                    Util.MessageInfo("SFU1632");
                    return;
                }
                Save_Cell();
            }
            else
            {
                if (dgMappingLotList.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1632");
                    return;
                }
                Save_Pallet();
            }
        }
        private void Save_Pallet()
        {
            try
            {
                string sResnCode = string.Empty;

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EM_JUDG_RSLT_CODE", typeof(string));
                RQSTDT.Columns.Add("FINL_EM_DFCT_CODE", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("INSDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("JUDG_NOTE", typeof(DateTime));
                RQSTDT.Columns.Add("SAVE_TYPE", typeof(DateTime));
                DataRow row = null;

                for (int i = 0; i < dgMappingLotList.Rows.Count; i++)
                {
                    row = RQSTDT.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMappingLotList.Rows[i].DataItem, "LOTID"));
                    row["EM_TYPE_CODE"] = "OTHER";
                    row["EM_JUDG_RSLT_CODE"] = null;
                    row["FINL_EM_DFCT_CODE"] = null;
                    row["NOTE"] = txtRemark.Text;
                    row["INSUSER"] = LoginInfo.USERID;
                    row["INSDTTM"] = DateTime.Now;
                    row["UPDUSER"] = LoginInfo.USERID;
                    row["UPDDTTM"] = DateTime.Now;
                    RQSTDT.Rows.Add(row);

                }

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("BR_RTLS_LOT_SCAN_SAVE", "RQSTDT", "", RQSTDT, (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void Save_Cell()
        {
            try
            {
                string sResnCode = string.Empty;

                sResnCode = Util.gridFindDataRow_GetValue(ref dgDefect, "CHK", "True", "RESNCODE");

                DataSet indataSet = new DataSet();
                DataTable RQSTDT = indataSet.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EM_JUDG_RSLT_CODE", typeof(string));
                RQSTDT.Columns.Add("FINL_EM_DFCT_CODE", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("INSDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));
                RQSTDT.Columns.Add("UPDDTTM", typeof(DateTime));
                RQSTDT.Columns.Add("JUDG_NOTE", typeof(DateTime));
                RQSTDT.Columns.Add("SAVE_TYPE", typeof(DateTime));

                DataRow row = RQSTDT.NewRow();
                row["LOTID"] = txtLotId.Text.Trim();
                row["EM_TYPE_CODE"] = "OTHER";
                row["EM_JUDG_RSLT_CODE"] = sCellJudg;
                row["FINL_EM_DFCT_CODE"] = sResnCode;
                row["NOTE"] = txtRemark.Text;
                row["INSUSER"] = LoginInfo.USERID;
                row["INSDTTM"] = DateTime.Now;
                row["UPDUSER"] = LoginInfo.USERID;
                row["UPDDTTM"] = DateTime.Now;
                indataSet.Tables["RQSTDT"].Rows.Add(row);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_RTLS_LOT_SCAN_SAVE", "RQSTDT", "", (bizResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1518");


                }, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Text Box 초기화
        /// </summary>
        private void Clear_TextBox()
        {
            var childList = FindVisualChildren<TextBox>(ContentLeft);
            foreach (DependencyObject child in childList)
            {
                if (!((TextBox)child).Name.Equals("txtSearchLotId"))
                    ((TextBox)child).Text = "";
            }
        }

        private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Tab 2

        #region 초기화

        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                String[] sFilter = { LoginInfo.CFG_AREA_ID };
                //동
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);
                cboArea.SelectedIndex = 0;
                //라인
                C1ComboBox[] cboLineParent = { cboArea };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent);

                String[] sFilterType = { LoginInfo.LANGID, "RTLS_SCAN_YN" };
                _combo.SetCombo(cboScanYn, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilterType);
                cboScanYn.SelectedIndex = 0;

                //보관기간
                InitCmb_cboPreiod(cboPeriod);
                //sFilterType[1] = "RTLS_STOCK_PERIOD";
                //_combo.SetCombo(cboPeriod, CommonCombo.ComboStatus.ALL, sCase: "COMMCODES", sFilter: sFilterType);
                //cboScanYn.SelectedIndex = 0;

            }
            catch (Exception)
            {
                throw;
            }

        }

        private void Init_cboJudgResult_Scan(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LOT_USG_TYPE_CODE";
                dr["ATTRIBUTE1"] = "CMA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

            }

            catch
            {


            }
        }
        private void fn_Init_RtlsMultiCmb()
        {
            Init_cboEmType(cboEmType);
            Init_cboStatcode(cboStatcode);
            Init_cboJudgResult(cboJudgResult);

        }
        private void Init_cboEmType(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_LINEOUT_TYPE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch
            {


            }

            //cbo.CheckAll();
        }
        private void Init_cboStatcode(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "RTLS_MMD_EM_STAT_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch
            {

            }

            //cbo.CheckAll();
        }
        private void Init_cboJudgResult(MultiSelectionBox cbo)
        {
            try
            {
                string attr1 = string.Empty;

                if (cboPrdtClass.SelectedItemsToString.Contains("CELL"))
                {
                    attr1 = "C";
                }
                if(cboPrdtClass.SelectedItemsToString.Contains("CMA") || cboPrdtClass.SelectedItemsToString.Contains("BMA"))
                {
                    attr1 = string.IsNullOrEmpty(attr1) ? "P" : "C,P";
                }
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LOT_USG_TYPE_CODE";
                dr["ATTRIBUTE1"] = cboPrdtClass.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : attr1;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_MULTI", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch
            {

            }
        }

        #endregion


        #region Method

        private void SetcboProcess()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);


            }
            catch
            {

            }
        }

        private void SetcboPrdtClass()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboPrdtClass.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch
            {

            }
        }

        private void SetcboProduct()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedItemsToString == "" ? null : cboProcess.SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = cboPrdtClass.SelectedItemsToString == "" ? null : cboPrdtClass.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProduct.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch
            {

            }
        }

        #endregion


        #region Event
        /// 콤보 변경 이벤트
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgPalletInfo);
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                cboProcess.isAllUsed = true;
                SetcboProcess();
                SetcboPrdtClass();
                SetcboProduct();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboProcess_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cboPrdtClass.isAllUsed = true;
                SetcboPrdtClass();
                //this.Dispatcher.BeginInvoke(new System.Action(() =>
                //{
                //    //cboPrdtClass.isAllUsed = true;

                //    //SetcboProduct();


                //}));
            }
            catch
            {
            }
        }

        private void cboPrdtClass_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                cboProduct.isAllUsed = true;
                SetcboProduct();
                Init_cboJudgResult(cboJudgResult);
            }
            catch
            {
            }
        }

        private void dgPalletInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgPalletInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EM_STAT_CODE")) == "N" && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SCAN_FLAG")) == "N" &&
                    Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EM_TYPE_CODE")) != "NODATA")
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                    e.Cell.Row.Presenter.Background = new SolidColorBrush(Colors.Orange);
                }

            }));
        }
        /////////////////////////

        /// <summary>
        /// Tab2  조회버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string[] Period;
            string F_Period = string.Empty;
            string T_Period = string.Empty;


            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("EM_DTTM_ST", typeof(DateTime));
            RQSTDT.Columns.Add("EM_DTTM_ED", typeof(DateTime));
            RQSTDT.Columns.Add("EM_STAT_CODE", typeof(string));
            RQSTDT.Columns.Add("EM_TYPE_CODE", typeof(string)); // 카테고리 PORT_OUT, OVER_TAKE...
            RQSTDT.Columns.Add("PROCID_CAUSE", typeof(string)); //불량 발생 공정
            RQSTDT.Columns.Add("TYPE", typeof(string)); //제품타입
            RQSTDT.Columns.Add("SCAN_FLAG", typeof(string)); //스캔유무
            RQSTDT.Columns.Add("JUDG_RSLT", typeof(string)); //판정구분
            RQSTDT.Columns.Add("F_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("T_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("PRODID", typeof(string)); //제품코드

            if (Util.NVC(cboPeriod.SelectedValue.ToString()) != "")
            {
                Period = Util.NVC(cboPeriod.SelectedValue.ToString()).Split('~');
                if (Period[0] == "")
                {

                    F_Period = "0";

                    T_Period = Period[1];

                }
                else if (Period[1] == "")
                {

                    F_Period = Period[0];
                    T_Period = "99999";
                }
                else
                {
                    F_Period = Period[0];
                    T_Period = Period[1];
                }

            }


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment);
            dr["EM_DTTM_ST"] = dtpFDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString();
            dr["EM_DTTM_ED"] = dtpTDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString();
            dr["EM_TYPE_CODE"] = cboEmType.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboEmType.SelectedItemsToString) == "" ? null : cboEmType.SelectedItemsToString;  // Util.NVC(cboEmType.SelectedItemsToString) == "" ? null : cboEmType.SelectedItemsToString;
            dr["EM_STAT_CODE"] = cboStatcode.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboStatcode.SelectedItemsToString) == "" ? null : cboStatcode.SelectedItemsToString;
            dr["PROCID_CAUSE"] = cboProcess.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboProcess.SelectedItemsToString) == "" ? null : cboProcess.SelectedItemsToString; //Util.NVC(cboProcess.SelectedItemsToString) == "" ? null : cboProcess.SelectedItemsToString;
            dr["TYPE"] = Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : cboPrdtClass.SelectedItemsToString; // Util.NVC(cboPrdtClass.SelectedItemsToString) == "" ? null : cboPrdtClass.SelectedItemsToString;
            dr["SCAN_FLAG"] = Util.NVC(cboScanYn.SelectedValue.ToString()) == "" ? null : cboScanYn.SelectedValue.ToString();
            dr["JUDG_RSLT"] = cboJudgResult.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboJudgResult.SelectedItemsToString) == "" ? null : cboJudgResult.SelectedItemsToString; // cboJudgResult.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboJudgResult.SelectedItemsToString) == "" ? null : cboJudgResult.SelectedItemsToString;
            dr["F_PERIOD"] = Util.NVC(cboPeriod.SelectedValue.ToString()) == "" ? null : F_Period;
            dr["T_PERIOD"] = Util.NVC(cboPeriod.SelectedValue.ToString()) == "" ? null : T_Period;
            dr["PRODID"] = cboProduct.MultiSelectionBoxSource.Cast<MultiSelectionBoxItem>().ElementAt(0).IsChecked || Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString; //Util.NVC(cboProduct.SelectedItemsToString) == "" ? null : cboProduct.SelectedItemsToString;

            RQSTDT.Rows.Add(dr);
            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_SEL_EM_LOT_MNGT", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.GridSetData(dgPalletInfo, dt, FrameOperation, true);
                    Util.Alert("SFU1905");
                    return;
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount, Util.NVC(dt.Rows.Count));

                Util.GridSetData(dgPalletInfo, dt, FrameOperation);

            });
        }

        /// <summary>
        /// CMA, BMA  판정이력 팝업
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPalletInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.DataGridColumn column = dgPalletInfo.CurrentColumn;
            int rowIndex = dgPalletInfo.CurrentRow.Index;
            if (column == null) return;
            if (column.Name != "EM_JUDG_RSLT_CODE") return;
            if (new string[] { "BMA", "CMA" }.Contains(Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[rowIndex].DataItem, "TYPE"))))
            {
                string Value = Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[rowIndex].DataItem, "EM_JUDG_RSLT_CODE"));
                string LotID = Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[rowIndex].DataItem, "LOTID"));
                if (!string.IsNullOrEmpty(Value))
                {
                    COM001_149_JUDG_Hist pop_Judg_Hist = new COM001_149_JUDG_Hist();
                    if (pop_Judg_Hist != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = LotID;
                        C1WindowExtension.SetParameters(pop_Judg_Hist, Parameters);

                        pop_Judg_Hist.Closed += new EventHandler(pop_Judg_Hist_Closed);
                        foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                        {
                            if (tmp.Name == "grdMain")
                            {
                                tmp.Children.Add(pop_Judg_Hist);
                                pop_Judg_Hist.BringToFront();
                                break;
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void pop_Judg_Hist_Closed(object sender, EventArgs e)
        {
            COM001_149_JUDG_Hist pop_Judg_Hist = sender as COM001_149_JUDG_Hist;
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(pop_Judg_Hist);
                    break;
                }
            }

        }

        #endregion

        #endregion

        #region Tab 3

        #region Method
        /// <summary>
        /// 콤보 초기화
        /// </summary>
        private void InitCombo_3()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment_3 };
            _combo.SetCombo(cboArea_3, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "cboArea");

            //라인
            C1ComboBox[] cboLineParent = { cboArea_3 };
            _combo.SetCombo(cboEquipmentSegment_3, CommonCombo.ComboStatus.NONE, cbParent: cboLineParent, sCase: "cboEquipmentSegment");
        }

        //써머리 수량 더블클릭시 배출 연계 조건 설정 및 조회
        private void Search_Where_Set(string[] para, string eqsgID, string prodType, string procID, string judgResult)
        {
            fn_Init_Cmb();

            if (!string.IsNullOrEmpty(eqsgID))
            {
                cboEquipmentSegment.SelectedValue = eqsgID;
                this.cboProcess.SelectionChanged -= new System.EventHandler(cboProcess_SelectionChanged);
                this.cboPrdtClass.SelectionChanged -= new System.EventHandler(cboPrdtClass_SelectionChanged);
            }
            if (!string.IsNullOrEmpty(procID))
            {
                cboProcess.Check(procID == "P0100" ? "P1000" : procID);  //발생공정
            }

            // 제품유형 셀, CMA, BMA
            cboPrdtClass.Check(prodType);

            if (!string.IsNullOrEmpty(procID)) // 합계클릭 여부
            {
                //상태는 무조건 미조치
                cboStatcode.Check("N");

                if (para[0].Equals("TYPE"))
                {
                    cboScanYn.SelectedValue = "N"; //스캔플래그
                    cboEmType.Check(para[1]);  // 배출유형

                }
                // RE-CHECK  클릭시
                if (para[0].Equals("SCAN"))
                {
                    cboEmType.Check(para[1]);
                    cboScanYn.SelectedValue = "R"; //스캔플래그
                }
                if (para[0].Equals("PER"))
                {
                    cboScanYn.SelectedValue = "Y";
                    if (para[1] == "ALL")
                    {
                        cboPeriod.SelectedValue = ""; //기간
                    }
                    else
                    {
                        cboPeriod.SelectedValue = para[1]; //기간
                    }
                }

                if (prodType.Equals("CELL") && para[0].Equals("PER"))
                {
                    cboJudgResult.Check(judgResult);
                    cboProcess.UncheckAll();
                }

                if (para[0].Equals("LINE"))
                {
                    cboStatcode.UncheckAll();
                    cboStatcode.Check("BR");
                }
            }
            else
            {
                // 배출타입  클릭시
                if (para[0].Equals("TYPE"))
                {
                    cboEmType.Check(para[1]);
                    cboScanYn.SelectedValue = "N"; //스캔플래그
                }
                // RE-CHECK  클릭시
                if (para[0].Equals("SCAN"))
                {
                    cboEmType.Check(para[1]);
                    cboScanYn.SelectedValue = "R"; //스캔플래그
                }
                //기간  클릭시
                if (para[0].Equals("PER"))
                {
                    cboScanYn.SelectedValue = "Y";
                    if (para[1] == "ALL")
                    {
                        cboPeriod.SelectedValue = "";
                    }
                    else
                    {
                        cboPeriod.SelectedValue = para[1]; //기간
                    }
                }
                //상태는 무조건 미조치
                cboStatcode.Check("N");
            }

            //검색 기간 변경
            DateTime timeFDt = DateTime.Now.AddYears(-1);
            DateTime timeTDt = DateTime.Now;
            if (para[0].Equals("TYPE"))
            {
                if (para[1].Equals("NODATA"))
                {
                    dtpFDate.SelectedDateTime = dtpFDate_3.SelectedDateTime;
                }
                else
                {
                    dtpFDate.SelectedDateTime = timeFDt;
                }
            }
            else
            {
                dtpFDate.SelectedDateTime = timeFDt;
            }

            dtpTDate.SelectedDateTime = timeTDt;
            this.cboProcess.SelectionChanged += new System.EventHandler(cboProcess_SelectionChanged);
            this.cboPrdtClass.SelectionChanged += new System.EventHandler(cboPrdtClass_SelectionChanged);
        }


        //써머리 수량 더블클릭시 Wip 재공 연계 조건 설정 및 조회
        private void Wip_Search_Where_Set(string[] para, string eqsgID, string prodType, string procID)
        {

            cboEquipmentSegment_4.SelectedValue = eqsgID;
            cboProcess_4.SelectedValue = procID == "P0100" ? "P1000" : procID;
            cboWipPreiod.SelectedValue = para[1];

            cboWipState.UncheckAll();
            DataTable dt = DataTableConverter.Convert(cboWipState.ItemsSource);
            if (procID == "P0100")
            {
      
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Util.NVC(dt.Rows[i]["CBO_CODE"]) == "WAIT")
                        cboWipState.Check(i);
                }
            }
            else
            {
                if (procID == "P1000")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (Util.NVC(dt.Rows[i]["CBO_CODE"]) != "WAIT")
                            cboWipState.Check(i);
                    }
                }
                else
                {
                    cboWipState.CheckAll();
                }
            }
        }
        // Tab2 Combo  초기화 
        private void fn_Init_Cmb()
        {
            cboProcess.UncheckAll();
            cboPrdtClass.UncheckAll();
            cboProduct.UncheckAll();
            cboEmType.UncheckAll();
            cboJudgResult.UncheckAll();
            cboStatcode.UncheckAll();
            cboScanYn.SelectedIndex = 0;
            cboPeriod.SelectedIndex = 0;
        }

        private void InitGridColumns()
        {
            if (LoginInfo.CFG_AREA_ID == "P7")
            {
                dgSummary.Columns["STK_TOTAL"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["STK_1"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["STK_2"].Visibility = Visibility.Collapsed;
                ((C1TabItem)tabControlMain.Items[4]).Visibility = Visibility.Collapsed;
            }
            else
            {
                dgSummary.Columns["STK_TOTAL"].Visibility = Visibility.Visible;
                dgSummary.Columns["STK_1"].Visibility = Visibility.Visible;
                dgSummary.Columns["STK_2"].Visibility = Visibility.Visible;
                ((C1TabItem)tabControlMain.Items[4]).Visibility = Visibility.Visible;
            }

            //컬럼 설명 정보 가져오기
            DataTable dt = fn_Get_Col_info();

            if (dt.Rows.Count > 0)
            {
                foreach (C1.WPF.DataGrid.DataGridColumn col in dgSummary.Columns)
                {
                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromRgb(229, 229, 229)) });

                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });

                    var ro = dt.AsEnumerable().Where(x => x["CBO_CODE"].ToString() == col.Name).ToArray();
                    if (ro.Length > 0)
                    {
                        string sInfo = ro[0]["CBO_NAME"].ToString().Replace(";", Environment.NewLine);
                        style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, sInfo));
                    }
                    col.HeaderStyle = style;
                }
            }
        }

        private DataTable fn_Get_Col_info()
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_COLUMNS_INFO";
            dr["ATTRIBUTE1"] = "DGSUMMARY";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_RTLS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);
            return dtResult;
        }
        //써머리 수량 더블클릭시 Wip 재공 연계 조건 설정 및 조회
        private void Stk_Search_Where_Set(string[] para, string eqsgID, string prodType, string procID)
        {

            cboEquipmentSegment_5.SelectedValue = eqsgID;
            cboProcess_5.SelectedValue = procID == "P0100" ? "P1000" : procID;
            cboStkPreiod.SelectedValue = para[1] == "ALL" ? "" : para[1];
        }

        #endregion

        #region Evnet
        // 셀 병합 이벤트 
        private void CellMaerge(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgSummary.Rows.Count == 2) return;

                int startRow = -1;
                int endRow = -1;
                bool chk = true;

                for (int i = 3; i < dgSummary.GetRowCount(); i++)
                {
                    if (Util.NVC(dgSummary.GetCell(i, dgSummary.Columns["PROCNAME"].Index).Value) == Util.NVC(dgSummary.GetCell((i - 1), dgSummary.Columns["PROCNAME"].Index).Value))
                    {
                        if (chk)
                        {
                            chk = false;
                            startRow = i - 1;
                        }
                    }
                    else
                    {
                        if (startRow != -1) endRow = i - 1; ;
                        //Break
                        if (!chk) i = dgSummary.GetRowCount();
                    }
                }
                if (startRow != endRow)
                {
                    DataGridCellsRange currentRange = null;
                    for (int i = 0; i < dgSummary.Columns.Count; i++)
                    {
                        if (new string[] { "PROCNAME", "LINE_STOCK", "PORTOUT_CNT", "OVERTAKE_CNT", "NODATA_CNT", "NOINPUT_CNT", "WIP_PERIOD_1", "WIP_PERIOD_2", "WIP_PERIOD_3", "RE_CHECK", "STK_TOTAL","STK_1", "STK_2" }.Contains(dgSummary.Columns[i].Name))
                        {
                            C1.WPF.DataGrid.DataGridCell currentCell = dgSummary.GetCell(startRow, i);
                            C1.WPF.DataGrid.DataGridCell nextCell = dgSummary.GetCell(endRow, i);
                            currentRange = new DataGridCellsRange(currentCell, nextCell);
                            e.Merge(currentRange);
                        }
                    }
                }
            }
            catch
            {

            }

        }

        // Tab 3 조회
        private void btnSearch_Summary_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SCAN_DTTM_ST", typeof(DateTime));
                RQSTDT.Columns.Add("SCAN_DTTM_ED", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea_3);
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_3).Trim() == "" ? null : Util.GetCondition(cboEquipmentSegment_3);
                dr["SCAN_DTTM_ST"] = dtpFDate_3.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                dr["SCAN_DTTM_ED"] = dtpTDate_3.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

                RQSTDT.Rows.Add(dr);

                DataSet dsInput = new DataSet();
                dsInput.Tables.Add(RQSTDT);

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_RTLS_SEL_EM_LOT_MNGT_SUMMARY_NOSCANDATA", "RQSTDT", "RSLTDT,FULLPALLET_CNT", (bizResult, bizException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (bizException != null)
                    {
                        throw bizException;
                    }

                    if (bizResult != null && bizResult.Tables.Count > 0)
                    {
                        Util.SetTextBlockText_DataGridRowCount(tbListCount_3, Util.NVC(bizResult.Tables["FULLPALLET_CNT"].Rows.Count));
                        Util.GridSetData(dgSummary, bizResult.Tables["RSLTDT"], FrameOperation, true);
                    }
                    if (bizResult.Tables["RSLTDT"].Rows.Count > 0)
                    {
                        string[] sColumnName = new string[] { "EQSGNAME", "TYPE" };
                        _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                        dgSummary.GroupBy(dgPalletInfo.Columns["TYPE"], DataGridSortDirection.None);
                        dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;
                    }
                    else
                    {
                        Util.Alert("SFU1905");
                        return;
                    }

                }, dsInput);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
            }

        }

        // 배출랏 조회 연계 관련 이벤트
        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string eqsgID = string.Empty;
                string prodType = string.Empty;
                string procID = string.Empty;
                string judgResult = string.Empty;
                string em_type_code = string.Empty;
                if (dgSummary.CurrentRow == null) return;

                C1.WPF.DataGrid.DataGridColumn column = dgSummary.CurrentColumn;
                if (column.Tag == null) return;
                string[] para = column.Tag.ToString().Split(',');
                if (para.Length > 1)
                {
                    int rowIndex = dgSummary.CurrentRow.Index;
                    //값이 있는지 체크
                    string Value = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[rowIndex].DataItem, column.Name));
                    if (string.IsNullOrEmpty(Value)) return;
                    eqsgID = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[rowIndex].DataItem, "EQSGID"));
                    prodType = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[rowIndex].DataItem, "TYPE"));
                    procID = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[rowIndex].DataItem, "PROCID"));

                    if (para[0].Equals("STK"))
                    {
                        if (string.IsNullOrEmpty(procID)) return;
                        if (Value.Equals("0")) return;
                        tabControlMain.SelectedIndex = 4; // Tab이 열리지 않으면 OnApplyTemplate 때문에 에러발생함.
                        loadingIndicator.Visibility = Visibility.Visible;
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                              new Action(delegate { }));
                        this.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            Stk_Search_Where_Set(para, eqsgID, prodType, procID);
                            Search_Tab5();
                        }));
                    }
                    else if (para[0].Equals("WIP"))
                    {
                        if (string.IsNullOrEmpty(procID)) return;
                        if (Value.Equals("0")) return;
                        tabControlMain.SelectedIndex = 3; // Tab이 열리지 않으면 OnApplyTemplate 때문에 에러발생함.
                        loadingIndicator.Visibility = Visibility.Visible;
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                              new Action(delegate { }));
                        this.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            Wip_Search_Where_Set(para, eqsgID, prodType, procID);
                            Search_Tab4();
                        }));
                    }
                    else
                    {
                        //반품 요청공정은 배출랏 보여줌 재공에서
                        if (para[0].Equals("LINE"))
                        {
                            if (!procID.Equals("PS100")) return;
                            if (Value.Equals("0")) return;
                        }
                        //Scan 된 Cell 클릭시
                        if (prodType.Equals("CELL") && para[0].Equals("PER"))
                        {

                            if (Value.Equals("0")) return;
                            judgResult = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[rowIndex].DataItem, "EM_JUDG_RSLT_CODE"));
                        }

                        tabControlMain.SelectedIndex = 1;
                        loadingIndicator.Visibility = Visibility.Visible;
                        Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                              new Action(delegate { }));

                        this.Dispatcher.BeginInvoke(new System.Action(() =>
                        {
                            Search_Where_Set(para, eqsgID, prodType, procID, judgResult);
                            btnSearch_Click(null, null);
                        }));

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
        #endregion

        #endregion

        #region Tab 4
        #region Method
        private void InitCombo_4()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment_4 };
            _combo.SetCombo(cboArea_4, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "cboArea");

            //라인
            C1ComboBox[] cboLineParent = { cboArea_4 };

            C1ComboBox[] cboEquipmnetChild= { cboProcess_4 };

            _combo.SetCombo(cboEquipmentSegment_4, CommonCombo.ComboStatus.NONE,   cbChild: cboEquipmnetChild, cbParent: cboLineParent, sCase: "cboEquipmentSegment");

            //공정
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment_4 };

            _combo.SetCombo(cboProcess_4, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: "cboProcess");


            InitCmb_cboWipPreiod(cboWipPreiod);

            InitCmb_cboWipState();
            //재공기간


            //조회 버튼
        }


        private void InitCmb_cboPreiod(C1ComboBox cmb)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_STOCK_PERIOD";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            DataRow ro = dtResult.NewRow();

            ro["CBO_CODE"] = "";
            ro["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(ro, 0);
            cmb.ItemsSource = DataTableConverter.Convert(dtResult);
            cmb.SelectedIndex = 0;
        }

        private void InitCmb_cboWipPreiod(C1ComboBox cmb)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_WIP_STOCK_PERIOD";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            DataRow ro = dtResult.NewRow();

            ro["CBO_CODE"] = "";
            ro["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(ro, 0);
            cmb.ItemsSource = DataTableConverter.Convert(dtResult);
            cmb.SelectedIndex = 0;
        }

        private void InitCmb_cboWipState()
        {
            try
            {

                string sSelectedValue = cboWipState.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WIPSTAT_WIPSEARCH";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
               
                cboWipState.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch 
            {
           
            }
        }


        private void SetcboProcess_4()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = cboEquipmentSegment_4.SelectedValue.ToString();
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                cboProcess_4.ItemsSource = DataTableConverter.Convert(dtResult);


            }
            catch
            {

            }
        }

        private void Search_Tab4()
        {
            string[] Period;
            string F_Period = string.Empty;
            string T_Period = string.Empty;


            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string)); //불량 발생 공정
            RQSTDT.Columns.Add("F_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("T_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("WIPSTAT",  typeof(string)); //기간

            Period = Util.NVC(cboWipPreiod.SelectedValue.ToString()).Split('~');
            if (Util.NVC(cboWipPreiod.SelectedValue.ToString()) == "")
            {
                F_Period = "0";
                T_Period = "99999";
            }
            else
            {
                if (Period[0] == "")
                {

                    F_Period = "0";

                    T_Period = Period[1];

                }
                else if (Period[1] == "")
                {

                    F_Period = Period[0];
                    T_Period = "99999";
                }
                else
                {
                    F_Period = Period[0];
                    T_Period = Period[1];
                }
            }

            if (Util.NVC(cboWipState.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1438");  //wip 상태를 선택하세요.
                return;
            }


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_4).Trim();
            dr["PROCID"] = cboProcess_4.SelectedValue.ToString();
            dr["F_PERIOD"] = F_Period;
            dr["T_PERIOD"] = T_Period;
            dr["WIPSTAT"]  = cboWipState.SelectedItemsToString;

            RQSTDT.Rows.Add(dr);
            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_SEL_WIP_STOCK", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount_4, Util.NVC(dt.Rows.Count));

                Util.GridSetData(dgWip, dt, FrameOperation);

            });
        }
        #endregion

        #region Event
        // Tab 4 조회버튼 클릭
        private void btnSearch_Wip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search_Tab4();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void cboEquipmentSegment_4_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            SetcboProcess_4();
        }

        #endregion



        #endregion


        #region Tab 5
        #region method
        private void InitCombo_5()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment_5 };
            _combo.SetCombo(cboArea_5, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "cboArea");

            //라인
            C1ComboBox[] cboLineParent = { cboArea_5 };

            C1ComboBox[] cboEquipmnetChild = { cboProcess_5 };

            _combo.SetCombo(cboEquipmentSegment_5, CommonCombo.ComboStatus.NONE, cbChild: cboEquipmnetChild, cbParent: cboLineParent, sCase: "cboEquipmentSegment");

            //공정
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment_5 };

            _combo.SetCombo(cboProcess_5, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent, sCase: "cboProcess");

            InitCmb_cboStkPreiod(cboStkPreiod);

        }

        private void InitCmb_cboStkPreiod(C1ComboBox cmb)
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "RTLS_STK_STOCK_PERIOD";

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            DataRow ro = dtResult.NewRow();

            ro["CBO_CODE"] = "";
            ro["CBO_NAME"] = "-ALL-";
            dtResult.Rows.InsertAt(ro, 0);
            cmb.ItemsSource = DataTableConverter.Convert(dtResult);
            cmb.SelectedIndex = 0;
        }

        private void Search_Tab5()
        {
            string[] Period;
            string F_Period = string.Empty;
            string T_Period = string.Empty;


            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQSGID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string)); //불량 발생 공정
            RQSTDT.Columns.Add("F_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("T_PERIOD", typeof(string)); //기간
            RQSTDT.Columns.Add("AREAID", typeof(string)); //기간

            Period = Util.NVC(cboStkPreiod.SelectedValue.ToString()).Split('~');
            if (Util.NVC(cboStkPreiod.SelectedValue.ToString()) == "")
            {
                F_Period = "0";
                T_Period = "99999";
            }
            else
            {
                if (Period[0] == "")
                {

                    F_Period = "0";

                    T_Period = Period[1];

                }
                else if (Period[1] == "")
                {

                    F_Period = Period[0];
                    T_Period = "99999";
                }
                else
                {
                    F_Period = Period[0];
                    T_Period = Period[1];
                }
            }


            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment_5).Trim();
            dr["PROCID"] = cboProcess_5.SelectedValue.ToString();
            dr["F_PERIOD"] = F_Period;
            dr["T_PERIOD"] = T_Period;
            dr["AREAID"] = Util.GetCondition(cboArea_5).Trim();

            RQSTDT.Rows.Add(dr);
            loadingIndicator.Visibility = Visibility.Visible;
            new ClientProxy().ExecuteService("BR_RTLS_SEL_STOCKER_STOCK", "RQSTDT", "RSLTDT", RQSTDT, (dt, ex) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }

                if (dt.Rows.Count < 1)
                {
                    Util.Alert("SFU1905");
                }

                Util.SetTextBlockText_DataGridRowCount(tbListCount_5, Util.NVC(dt.Rows.Count));

                Util.GridSetData(dgStk, dt, FrameOperation);

            });
        }


        #endregion

        #region Event
        private void btnSearch_Stk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Search_Tab5();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion




        #endregion
    }
}
