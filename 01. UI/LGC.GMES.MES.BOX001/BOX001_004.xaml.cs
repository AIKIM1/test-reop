/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_004 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        public BOX001_004_REPRINT_REASON BOX001_004_REPRINT_REASON;

        public string RePrintcomment = string.Empty;
        public string pUserInfo = string.Empty;
        private string sSkip_Flag = "N";

        int iPrintCnt = 0;

        BarcodeLib.Barcode b = new BarcodeLib.Barcode();

        #region Declaration & Constructor 
        public BOX001_004()
        {
            InitializeComponent();

            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnMapping_Box);
            listAuth.Add(btnUnpack);
            listAuth.Add(btnPrint);
            listAuth.Add(btnProcess);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            SetEvent();

            txtLOTID.Focus();

        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom3.SelectedDataTimeChanged += dtpDateFrom3_SelectedDataTimeChanged;
            dtpDateTo3.SelectedDataTimeChanged += dtpDateTo3_SelectedDataTimeChanged;

            dtpDateFrom4.SelectedDataTimeChanged += dtpDateFrom4_SelectedDataTimeChanged;
            dtpDateTo4.SelectedDataTimeChanged += dtpDateTo4_SelectedDataTimeChanged;
        }

        private void dgAdd_Loaded(object sender, RoutedEventArgs e)
        {
            dgAdd.Loaded -= dgAdd_Loaded;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize_dgAdd();
                //dgAdd.ScrollIntoView(0, 0);
                //dgAdd.SelectedIndex = 0;
            }));
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            dgAdd.Loaded -= dgAdd_Loaded;
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                Initialize_dgAdd();
            }));
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dgAdd.Loaded += dgAdd_Loaded;

            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom3.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo3.SelectedDateTime = (DateTime)System.DateTime.Now;

            dtpDateFrom4.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo4.SelectedDateTime = (DateTime)System.DateTime.Now;

            CommonCombo _combo = new CommonCombo();
            
            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter2 = { "ELEC_TYPE" };
            //_combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            _combo.SetCombo(cboElecType2, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            //txtLOTID.Focus();
        }
        #endregion

        #region Event

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

        private void dtpDateFrom3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo3.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo3.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo3_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom3.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom3.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFrom4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo4.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo4.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo4_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom4.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom4.SelectedDateTime;
                return;
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string OK_NG = string.Empty;
                    string sLotid = string.Empty;
                    string sChg_ID = string.Empty;

                    sLotid = txtLOTID.Text.ToString().Trim();

                    if (dgBOXMapping.GetRowCount() > 0)
                    {
                        if (DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "ELECTRODE").ToString().Equals("A"))
                        {
                            if (dgBOXMapping.GetRowCount() > 2)
                            {
                                Util.Alert("SFU2995");     //박스구성 가능한 팬케익은 3 EA입니다.
                                return;
                            }
                        }
                        else
                        {
                            if (dgBOXMapping.GetRowCount() > 0)
                            {
                                Util.Alert("SFU2996");     //박스구성 가능한 점보롤은 1 EA입니다.
                                return;
                            }
                        }
                    }

                    // 중복 스캔 체크
                    for (int i = 0; i < dgBOXMapping.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "LOTID").ToString().Equals(sLotid))
                        {
                            Util.Alert("SFU1914");     //중복 스캔하셨습니다.
                            return;
                        }
                    }

                    if (chkPass.IsChecked.Value  == false)
                    {                   
                        string Elec_model = fn_Electro_GET(sLotid);

                        if (Elec_model == null)
                        {
                            Util.Alert("SFU2999");  //극성 정보를 확인할수 없습니다. IT 담당자에게 문의하세요.	
                            txtLOTID.Text = "";
                            return;
                        }
                        else if (Elec_model == "C")
                        {
                            //E3000
                            sChg_ID = fn_LotTrace(sLotid, "E3000");

                            OK_NG = fn_LotID_Hold_Chk(sChg_ID, "LQCM002", "DA_PRD_SEL_LOTID_HOLD_ROL_ALL");

                            if (OK_NG == "OK")
                            {
                                //E3900
                                OK_NG = fn_LotID_Hold_Chk(sLotid, "LQCM022", "DA_PRD_SEL_LOTID_HOLD_RWI_ALL");

                                if (OK_NG == "NG")
                                {
                                    Util.Alert("SFU3002");  //REWINDER 공정에서 검사가 이루어 지지 않았습니다.                        
                                    return;
                                }
                                else if (OK_NG == "QMS")
                                {
                                    Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                                    return;
                                }
                            }
                            else if (OK_NG == "NG")
                            {
                                Util.Alert("SFU3001");  //ROLL 공정에서 검사가 이루어 지지 않았습니다.
                                return;
                            }
                            else if (OK_NG == "QMS")
                            {
                                Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                                return;
                            }


                            #region 이전 로직 삭제
                            //OK_NG = fn_LotID_Hold_Chk_Rol(txtLOTID.Text.ToString());
                            //
                            //if (OK_NG.ToString() == "NG")
                            //{
                            //    //없음...
                            //}
                            //else
                            //{
                            //    //QMS 통합 프로젝트 지원 건 - 자동차 MES/Pack QMS 출하 Lot 홀드 프로세스 개선 
                            //    OK_NG = fn_LotID_Hold_Chk_Rwi(txtLOTID.Text.ToString());
                            //}
                            #endregion
                        }
                        else
                        {
                            //E3000
                            sChg_ID = fn_LotTrace(sLotid, "E3000");

                            OK_NG = fn_LotID_Hold_Chk(sChg_ID, "LQCM002", "DA_PRD_SEL_LOTID_HOLD_ROL_ALL");

                            if (OK_NG == "OK")
                            {
                                //E4000
                                OK_NG = fn_LotID_Hold_Chk(sLotid, "LQCM017", "DA_PRD_SEL_LOTID_HOLD_SLI_ALL");

                                if (OK_NG == "OK")
                                {
                                    //E8000
                                    OK_NG = fn_LotID_Hold_Chk(sLotid, "LQCM003", "DA_PRD_SEL_LOTID_HOLD_VD_ALL");

                                    if (OK_NG == "NG")
                                    {
                                        Util.Alert("SFU3004");  //전극 VD 공정에서 검사가 이루어 지지 않았습니다.
                                        return;
                                    }
                                    else if (OK_NG == "QMS")
                                    {
                                        Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                                        return;
                                    }
                                }
                                else if (OK_NG == "NG")
                                {
                                    Util.Alert("SFU3003");  //SLITTING 공정에서 검사가 이루어 지지 않았습니다.
                                    return;
                                }
                                else if (OK_NG == "QMS")
                                {
                                    Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                                    return;
                                }
                            }
                            else if (OK_NG == "NG")
                            {
                                Util.Alert("SFU3001");  //ROLL 공정에서 검사가 이루어 지지 않았습니다.
                                return;
                            }
                            else if (OK_NG == "QMS")
                            {
                                Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                                return;
                            }


                            #region 이전 로직 삭제
                            //OK_NG = fn_LotID_Hold_Chk_Rol(txtLOTID.Text.ToString());

                            //if (OK_NG.ToString() == "NG")
                            //{
                            //    //??????????
                            //}
                            //else
                            //{
                            //    //QMS 통합 프로젝트 지원 건 - 자동차 MES/Pack QMS 출하 Lot 홀드 프로세스 개선 
                            //    OK_NG = fn_LotID_Hold_Chk_Sli(txtLOTID.Text.ToString());

                            //    if (OK_NG.ToString() == "NG")
                            //    {

                            //    }
                            //    else
                            //    {
                            //        OK_NG = fn_LotID_Hold_Chk_Vd(txtLOTID.Text.ToString());
                            //    }
                            //}
                            #endregion
                        }

                        #region 이전 로직 삭제
                        //if (OK_NG.ToString() == "NG")
                        //{

                        //}
                        //else
                        //{
                        //    if (Elec_model != "C")
                        //    {
                        //        OK_NG = fn_LotID_VDHold_Chk(txtLOTID.Text.ToString());
                        //    }
                        //}
                        #endregion
                        

                        fn_Lotid_Chk(sLotid);

                        txtLOTID.SelectAll();
                        txtLOTID.Focus();

                    }
                    else if (chkPass.IsChecked.Value == true)
                    {
                        //패스 상태를 유지 하시겠습니까?
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2997"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                sSkip_Flag = "Y";
                                fn_Lotid_Chk(sLotid);

                                txtLOTID.SelectAll();
                                txtLOTID.Focus();
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Mehod
        private void fn_Lotid_Chk(string sLotid)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["PROCID"] = "E9100";

                RQSTDT.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_INFO_NISSAN", "RQSTDT", "RSLTDT", RQSTDT);

                if (result.Rows.Count == 0)
                {
                    Util.Alert("SFU3115");  //포장창고에 존재 하는지 또는 Nissan 향 제품코드가 맞는지 확인후 진행하세요.
                    return;
                }

                if (result.Rows[0]["BOXCHK"].ToString().Equals("N"))
                {
                    Util.Alert("SFU3116");  //이미 박스구성된 팬케익/점보롤입니다.
                    return;
                }

                //if (dgBOXMapping.Rows.Count == 1)
                if (dgBOXMapping.Rows.Count != 0)
                {
                    // 모델 체크
                    if (!DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "PRODID").ToString().Equals(result.Rows[0]["PRODID"].ToString()))
                    {
                        Util.Alert("SFU1897");  //제품코드가 다릅니다.
                        return;
                    }

                    // 극성 체크
                    if (!DataTableConverter.GetValue(dgBOXMapping.Rows[0].DataItem, "ELECTRODE").ToString().Equals(result.Rows[0]["ELECTRODE"].ToString()))
                    {
                        Util.Alert("SFU2057");  //극성 정보가 다릅니다.
                        return;
                    }

                    dgBOXMapping.IsReadOnly = false;
                    dgBOXMapping.BeginNewRow();
                    dgBOXMapping.EndNewRow(true);
                    //DataTableConverter.SetValue(dgBOXHist.CurrentRow.DataItem, "CHK", true);
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "LOTID", result.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "PRODID", result.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "MODLID", result.Rows[0]["MODLID"].ToString());
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "WIPQTY", result.Rows[0]["WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "WIPQTY2", result.Rows[0]["WIPQTY2"].ToString());
                    DataTableConverter.SetValue(dgBOXMapping.CurrentRow.DataItem, "ELECTRODE", result.Rows[0]["ELECTRODE"].ToString());
                    dgBOXMapping.IsReadOnly = true;
                }
                else
                {
                    //dgBOXMapping.ItemsSource = DataTableConverter.Convert(result);
                    Util.GridSetData(dgBOXMapping, result, FrameOperation);
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        private void popup_Closed(object sender, EventArgs e)
        {

        }

        #region Button Event

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            BOX001_004_WAITING_LOT wndConfirm = new BOX001_004_WAITING_LOT();
            wndConfirm.FrameOperation = FrameOperation;

            if (wndConfirm != null)
            {
                //object[] Parameters = new object[2];
                //Parameters[0] = sRCV_ISS_ID;
                //Parameters[1] = dtTempInfo;

                //C1WindowExtension.SetParameters(wndConfirm, Parameters);
                wndConfirm.Closed -= new EventHandler(wndConfirm_Load);
                wndConfirm.Closed += new EventHandler(wndConfirm_Load);
                this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                // 팝업 화면 숨겨지는 문제 수정.
                // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                //grdMain.Children.Add(wndConfirm);
                //wndConfirm.BringToFront();
            }
        }


        private void wndConfirm_Load(object sender, EventArgs e)
        {
            BOX001_004_WAITING_LOT window = sender as BOX001_004_WAITING_LOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                    
            }

            txtLOTID.Focus();
        }


        /// <summary>
        /// 600 Dpi 프린트
        /// 최초 발행시 2장 발행됨.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                iPrintCnt = 2;

                if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 1)
                {
                    Util.Alert("SFU2058");  //선택된 박스가 없습니다.
                    return;
                }

                string sReprint_Flag = string.Empty;

                for (int i = 0; i < dgBOXHist.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "CHK")) == "1")
                    {
                        if (DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "PRINTYN").ToString().Equals("Y"))
                        {
                            sReprint_Flag = "Y";
                        }
                        else
                        {
                            sReprint_Flag = "N";
                        }
                    }
                }

                if (sReprint_Flag == "N")
                {
                    Print_Label();
                }
                else if (sReprint_Flag == "Y")
                {
                    RePrint_Popup();
                }

                //fn_Search("HIST", dgBOXHist);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void RePrint_Popup()
        {
            //재발행 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2059"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataRow[] drChk = Util.gridGetChecked(ref dgBOXHist, "CHK");

                    string sLotid = drChk[0]["LOTID"].ToString();

                    BOX001_004_REPRINT_REASON wndConfirm = new BOX001_004_REPRINT_REASON();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = sLotid;
                        C1WindowExtension.SetParameters(wndConfirm, Parameters);

                        wndConfirm.Closed += new EventHandler(REPRINT_REASON_Closed);
                        // 팝업 화면 숨겨지는 문제 수정.
                        // this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                        grdMain.Children.Add(wndConfirm);
                        wndConfirm.BringToFront();
                    }
                }
                else
                {
                    return;
                }
            });
            
        }

        private void REPRINT_REASON_Closed(object sender, EventArgs e)
        {
            BOX001_004_REPRINT_REASON window = sender as BOX001_004_REPRINT_REASON;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                RePrintcomment = window.RePrintcomment.ToString();
                Print_Label();
            }
        }

        private void Print_Label()
        {
            try
            {
                decimal SeqNo = 0;

                for (int i = 0; i < dgBOXHist.GetRowCount(); i++)
                {
                    SeqNo = 0;

                    if (Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "CHK")) == "1")
                    {
                        if (DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "PRINTYN").ToString().Equals("Y"))
                        {
                            iPrintCnt = 1;
                        }
                        else
                        {
                            iPrintCnt = 2;
                        }

                        string sLotid = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LOTID").ToString();
                        string sRanid = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "RAN_ID").ToString();
                        string sBoxid = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString();
                        string sM_Roll_id = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "M_ROLL_ID").ToString();
                        //string sS_Roll_id = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "S_ROLL_ID").ToString();

                        int iLABEL_PRT_SEQNO = Convert.ToInt32(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LABEL_PRT_SEQNO").ToString());

                        string sElecrode = fn_Electro_GET(sLotid);

                        if (sElecrode == null)
                        {
                            Util.Alert("SFU2999");  //극성 정보를 확인할수 없습니다. IT 담당자에게 문의하세요.
                            return;
                        }

                        //양극
                        if (sElecrode == "C")
                        {
                            DataTable dtShipdata = Get_Ship_Data(sLotid, sElecrode);

                            if (dtShipdata.Rows.Count == 0)
                            {
                                Util.Alert("SFU3000");  //데이터 확인중 문제가 발생하였습니다.
                                return;
                            }
                            else
                            {
                                /////////////////////////////////////////////////////////////////////////////////////////////////
                                //  확인 필요....
                                /////////////////////////////////////////////////////////////////////////////////////////////////

                                double ichgQty = Convert.ToDouble(dtShipdata.Rows[0]["GOODQTY"].ToString());

                                /////////////////////////////////////////////////////////////////////////////////////////////////
                                double idefectQty1 = Convert.ToDouble(dtShipdata.Rows[0]["QTY1"].ToString());
                                double idefectQty2 = Convert.ToDouble(dtShipdata.Rows[0]["QTY2"].ToString());
                                double idefectQty3 = Convert.ToDouble(dtShipdata.Rows[0]["QTY3"].ToString());

                                double iqty1 = ichgQty - idefectQty1;
                                double iqty2 = ichgQty - idefectQty2;
                                double iqty3 = ichgQty - idefectQty3;
                                double iokPicTotal = iqty1 + iqty2 + iqty3; // 패턴값에서 NG 수 제한 값
                                double ingPicTotal = idefectQty1 + idefectQty2 + idefectQty3;

                                //DataTable RQSTDT = new DataTable();
                                //RQSTDT.TableName = "RQSTDT";
                                //RQSTDT.Columns.Add("LOTID", typeof(String));
                                //RQSTDT.Columns.Add("ELEC", typeof(String));

                                //DataRow dr = RQSTDT.NewRow();
                                //dr["LOTID"] = sLotid;
                                //dr["ELEC"] = sElecrode;

                                //RQSTDT.Rows.Add(dr);

                                //DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_NISSAN_ROLLID", "RQSTDT", "RSLTDT", RQSTDT);

                                //string sMotherID = SearchResult.Rows[0]["MROLLID"].ToString();

                                string sPgdate = dtShipdata.Rows[0]["YYYYMMDD"].ToString();

                                string sOK_PIC = string.Empty;

                                double iTotal_OK = 0;

                                string sChgRanID = "A" + sRanid;

                                // Image 버전.....ㅠ.ㅠ
                                #region 라벨 발행
                                //********************************************************************************************************************************
                                for (int p = 0; p < iPrintCnt; p++)
                                {

                                    b.RotateFlipType = System.Drawing.RotateFlipType.Rotate90FlipNone;
                                    System.Drawing.Image bitImg1 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sM_Roll_id, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg2 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, ichgQty.ToString("0000") + "-" + iqty1.ToString("0000") + "-" + iqty2.ToString("0000") + "-" + iqty3.ToString("0000"), System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg3 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, ingPicTotal.ToString(), System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg4 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sPgdate, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg7 = b.Encode(BarcodeLib.TYPE.CODE39, sChgRanID, System.Drawing.Color.Black, System.Drawing.Color.White);


                                    int iLng1, iLng2, iLng3, iLng4, iLng7;
                                    string s1, s2, s3, s4, s7;

                                    s1 = GetImageZplString(bitImg1, new System.Drawing.Size(240, 2505), out iLng1);   // 80, 835 -> 240, 2505
                                    s2 = GetImageZplString(bitImg2, new System.Drawing.Size(240, 2760), out iLng2);   // 80, 920 -> 240, 2760
                                    s3 = GetImageZplString(bitImg3, new System.Drawing.Size(240, 1040), out iLng3);   // 80, 348 -> 240, 1040
                                    s4 = GetImageZplString(bitImg4, new System.Drawing.Size(240, 1257), out iLng4);   // 80, 419 -> 240, 1257
                                    s7 = GetImageZplString(bitImg7, new System.Drawing.Size(240, 1500), out iLng7);   // 80, 500 -> 240, 1500


                                    StringBuilder str = new StringBuilder();

                                    str.Append("^XA");
                                    str.Append("^MCY");

                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^FWN^CFD,24^PW2496^LH0,0");    //^FWN^CFD,24^PW1140^LH0,0 -> ^FWN^CFD,24^PW2496^LH0,0   ^PW = 프린트 되는 폭
                                    str.Append("^LRN^FWN^CFD,24^LH20,460");     // ^LH30,05" -> ^LH20,460"                              ^LH = 프린트 시작위치     
                                    str.Append("^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^LRN");
                                    str.Append(" ^SEE:UHANGUL.DAT^FS");
                                    str.Append(" ^CW1,E:KFONT3.FNT^CI26^FS");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^MCY");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^DFR:TEMP_FMT.ZPL");

                                    str.Append("^LRN");

                                    str.Append("~DYE:BARCODE1,P,P" + "," + iLng1.ToString() + ",," + s1 + "^FS");
                                    str.Append("~DYE:BARCODE2,P,P" + "," + iLng2.ToString() + ",," + s2 + "^FS");
                                    str.Append("~DYE:BARCODE3,P,P" + "," + iLng3.ToString() + ",," + s3 + "^FS");
                                    str.Append("~DYE:BARCODE4,P,P" + "," + iLng4.ToString() + ",," + s4 + "^FS");
                                    str.Append("~DYE:BARCODE7,P,P" + "," + iLng7.ToString() + ",," + s7 + "^FS");
                                    str.Append("^A0N,240,84^FO1464,120^IME:BARCODE1.PNG^FS");    //^A0N,80,28^FO488,40 -> ^A0N,240,84^FO1464,120
                                    str.Append("^A0N,240,84^FO465,120^IME:BARCODE2.PNG^FS");    //^A0N,80,28^FO155,40 -> ^A0N,240,84^FO465,120
                                    str.Append("^A0N,240,84^FO015,117^IME:BARCODE3.PNG^FS");    //^A0N,80,28^FO005,39 -> ^A0N,240,84^FO015,117
                                    str.Append("^A0N,240,84^FO015,1320^IME:BARCODE4.PNG^FS");   //^A0N,80,28^FO005,440 -> ^A0N,240,84^FO015,1320
                                    str.Append("^A0N,240,84^FO1935,2400^IME:BARCODE7.PNG^FS");     //^A0N,80,28^FO645,800 -> ^A0N,240,84^FO1935,2400

                                    str.Append("^A0N,240,84^FO2190,120^A0R, 120,60^FDPART NO :^FS");   //^A0N,80,28^FO730,40^A0R, 40,20 -> ^A0N,240,84^FO2190,120^A0R, 120,60
                                    str.Append("^A0N,240,84^FO1920,162^BY9,2.7,240^B3R,N,240,N,N^FR^FH^FD>:P299M04NP0A^FS");  //^A0N,80,28^FO640,54^BY3,2.7,80^B3R,N,80  -> ^A0N,240,84^FO1920,162^BY9,2.7,240^B3R,N,240

                                    str.Append("^A0N,240,84^FO1719,120^A0R, 120,60^FDMR ID ^FS");        //A0N,80,28^FO573,40^A0R, 40,20 -> A0N,240,84^FO1719,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO1230,120^A0R, 120,60^FDSR ID ^FS");        //^A0N,80,28^FO410,40^A0R, 40,20 -> ^A0N,240,84^FO1230,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO729,120^A0R, 120,60^FDOK PIC^FS");         //^A0N,80,28^FO243,40^A0R, 40,20 -> ^A0N,240,84^FO729,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO270,120^A0R, 120,60^FDNG PIC ^FS ");       //^A0N,80,28^FO090,40^A0R, 40,20 -> ^A0N,240,84^FO270,120^A0R, 120,60



                                    str.Append("^A0N,240,84^FO2175,600^A0R,150,60^FD299M04NP0A^FS");        // ^A0N,80,28^FO725,200^A0R,50,20 -> ^A0N,240,84^FO2175,600^A0R,150,60

                                    str.Append("^A0N,240,84^FO510,2970^A0R,150,60^FD" + sLotid + "^FS");  // 변경  //^A0N,80,28^FO170,990^A0R,50,20 -> ^A0N,240,84^FO510,2970^A0R,150,60



                                    str.Append("^A0N,240,84^FO1701,600^A0R,150,75^FD" + sM_Roll_id + "^FS");             //^A0N,80,28^FO567,200^A0R,50,25  -> ^A0N,240,84^FO1701,600^A0R,150,75
                                    str.Append("^A0N,240,84^FO1185,600^A0R,150,75^FD^FS");                         // ^A0N,80,28^FO395,200^A0R,50,25 -> ^A0N,240,84^FO1185,600^A0R,150,75
                                    str.Append("^A0N,240,84^FO729,600^A0R,150,75^FD" + ichgQty.ToString("0000") + "-" +     //^A0N,80,28^FO243,200^A0R,50,25 -> ^A0N,240,84^FO729,600^A0R,150,75
                                                                                                        iqty1.ToString("0000") + "-" +
                                                                                                        iqty2.ToString("0000") + "-" +
                                                                                                        iqty3.ToString("0000") + "^FS");
                                    str.Append("^A0N,240,84^FO249,600^A0R,150,75^FH^FD" + ingPicTotal + "^FS");   //^A0N,80,28^FO083,200^A0R,50,25 -> ^A0N,240,84^FO249,600^A0R,150,75


                                    str.Append("^A0N,240,84^FO2190,2340^A0R, 120,60^FDRAN ^FS");                 //^A0N,80,28^FO730,780^A0R, 40,20 -> ^A0N,240,84^FO2190,2340^A0R, 120,60

                                    str.Append("^A0N,240,84^FO270,1254^A0R, 120,60^FDPacking Date^FS");          //^A0N,80,28^FO090,418^A0R, 40,20 -> ^A0N,240,84^FO270,1254^A0R, 120,60

                                    str.Append("^A0N,240,84^FO690,2970^A0R,156,174,120,150^FDLG Chem,Ltd. ^FS");       //^A0N,80,28^FO230,990^A0R,52,58,40,50 -> ^A0N,240,84^FO690,2970^A0R,156,174,120,150

                                    str.Append("^A0N,240,84^FO249,1890^A0R,150,75^FD" + sPgdate + "^FS");               //^A0N,80,28^FO083,630^A0R,50,25-> ^A0N,240,84^FO249,1890^A0R,150,75


                                    //int tempOK = int.Parse(okPicDetail) + int.Parse(okPicDetail2) + int.Parse(okPicDetail3);

                                    str.Append("^A0N,240,84^FO270,2715^A0R, 120,60^FDTotal OK^FS ");                           //^A0N,80,28^FO090,905^A0R, 40,20 -> ^A0N,240,84^FO270,2715^A0R, 120,60
                                    str.Append("^A0N,240,84^FO249,3090^A0R,150,75^FD" + iokPicTotal + "^FS");                       //^A0N,80,28^FO083,1030^A0R,50,25 -> ^A0N,240,84^FO249,3090^A0R,150,75
                                    str.Append("^A0N,240,84^FO015,2790^BY9,2.7,240^B3R,N,240,N,N^FR^FH^FD>:" + iokPicTotal + "^FS");    //^A0N,80,28^FO005,930^BY3,2.7,80^B3R,N,80 -> ^A0N,240,84^FO015,2790^BY9,2.7,240^B3R,N,240

                                    str.Append("^A0N,240,84^FO2175,2940^A0R,150,75^FD" + sRanid + "^FS");        // ^A0N,80,28^FO725,980^A0R,50,25  -> ^A0N,240,84^FO2175,2940^A0R,150,75

                                    str.Append("^FO405,120^GB0,3780,6,^FS");              // ^FO135,50^GB0,1260,2 -> ^FO405,120^GB0,3780,6
                                    str.Append("^FO894,120^GB0,3780,6,^FS");              // ^FO298,50^GB0,1260,6 -> ^FO894,120^GB0,3780,6
                                    str.Append("^FO1380,120^GB0,3780,6,^FS");             // ^FO460,50^GB0,1260,2 -> ^FO1380,120^GB0,3780,6
                                    str.Append("^FO1860,120^GB0,3780,6,^FS");  //24      // ^FO620,50^GB0,1260,2 -> ^FO1860,120^GB0,3780,6


                                    str.Append("^FO000,1200^GB414,0,6,^FS");              // ^FO000,400^GB138,0,2 -> ^FO000,1200^GB414,0,6
                                    str.Append("^FO000,2640^GB414,0,6,^FS");              // ^FO000,880^GB138,0,2 -> ^FO000,2640^GB414,0,6
                                    str.Append("^FO414,2910^GB1444,0,6,^FS");  //24       // ^FO138,970^GB483,0,2 -> ^FO414,2910^GB1444,0,6
                                    str.Append("^FO^FO1866,2250^GB450,0,6,^FS");  //24    // ^FO622,750^GB150,0,2 -> ^FO1866,2250^GB450,0,6

                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^XFR:TEMP_FMT.ZPL");
                                    str.Append("^PQ1,0,1,Y");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^IDR:TEMP_FMT.ZPL");
                                    str.Append("^JBE");
                                    str.Append("^XZ");


                                    string sPrt = string.Empty;
                                    string sRes = string.Empty;
                                    string sCopy = string.Empty;
                                    string sXpos = string.Empty;
                                    string sYpos = string.Empty;
                                    string sDark = string.Empty;
                                    DataRow drPrtInfo = null;

                                    if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                        return;

                                    if (PrintLabel(str.ToString(), drPrtInfo) == false)
                                    {
                                        //라벨 발행중 문제가 발생하였습니다.
                                        Util.Alert("SFU3150");
                                        return;
                                    }

                                }

                                //**********************************************************************************************************************************
                                #endregion

                                // GMES 버전.....ㅠ.ㅠ
                                #region 라벨 발행
                                DataTable ZPLResult = new DataTable();

                                // 양극 바코드 발행 메서드 호출.  
                                // 최초발행시 2장프린트
                                //for (int p = 0; p < iPrintCnt; p++)
                                //{

                                //    sOK_PIC = ichgQty.ToString("0000") + "-" + iqty1.ToString("0000") + "-" + iqty2.ToString("0000") + "-" + iqty3.ToString("0000");

                                //    iTotal_OK = iqty1 + iqty2 + iqty3;

                                //    DataTable RQSTDT = new DataTable();
                                //    RQSTDT.TableName = "RQSTDT";
                                //    RQSTDT.Columns.Add("LBCD", typeof(string));         // 라벨코드
                                //    RQSTDT.Columns.Add("PRMK", typeof(string));         // 프린터 기종
                                //    RQSTDT.Columns.Add("RESO", typeof(string));         // 해상도
                                //    RQSTDT.Columns.Add("PRCN", typeof(string));         // 발행수량
                                //    RQSTDT.Columns.Add("MARH", typeof(string));         // Horizontal Start Position 
                                //    RQSTDT.Columns.Add("MARV", typeof(string));         // Vertical Start Position
                                //                                                        // 인쇄항목 코드와 값
                                //    RQSTDT.Columns.Add("ATTVAL001", typeof(string));      // PART NO
                                //    RQSTDT.Columns.Add("ATTVAL002", typeof(string));      // PART NO BCD
                                //    RQSTDT.Columns.Add("ATTVAL003", typeof(string));      // RAN ID
                                //    RQSTDT.Columns.Add("ATTVAL004", typeof(string));      // RAN ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL005", typeof(string));      // M Roll ID
                                //    RQSTDT.Columns.Add("ATTVAL006", typeof(string));      // M Roll ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL007", typeof(string));      // C Roll ID
                                //    RQSTDT.Columns.Add("ATTVAL008", typeof(string));      // C Roll ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL009", typeof(string));      // OK PIC
                                //    RQSTDT.Columns.Add("ATTVAL010", typeof(string));      // OK PIC BCD
                                //    RQSTDT.Columns.Add("ATTVAL011", typeof(string));      // NG PIC
                                //    RQSTDT.Columns.Add("ATTVAL012", typeof(string));      // NG PIC BCD
                                //    RQSTDT.Columns.Add("ATTVAL013", typeof(string));      // Packing Date
                                //    RQSTDT.Columns.Add("ATTVAL014", typeof(string));      // Packing Date BCD
                                //    RQSTDT.Columns.Add("ATTVAL015", typeof(string));      // Total OK
                                //    RQSTDT.Columns.Add("ATTVAL016", typeof(string));      // Total OK BCD
                                //    RQSTDT.Columns.Add("ATTVAL017", typeof(string));      // 회사명
                                //    RQSTDT.Columns.Add("ATTVAL018", typeof(string));      // LOT ID

                                //    DataRow dr = RQSTDT.NewRow();
                                //    dr["LBCD"] = "LBL0003";
                                //    dr["PRMK"] = "Z";
                                //    dr["RESO"] = "600";
                                //    dr["PRCN"] = iPrintCnt;
                                //    dr["MARH"] = "0";
                                //    dr["MARV"] = "0";

                                //    dr["ATTVAL001"] = "299M04NP0A";
                                //    dr["ATTVAL002"] = "P299M04NP0A";
                                //    dr["ATTVAL003"] = sRanid;
                                //    dr["ATTVAL004"] = "A" + sRanid;
                                //    dr["ATTVAL005"] = sM_Roll_id;
                                //    dr["ATTVAL006"] = sM_Roll_id;
                                //    dr["ATTVAL007"] = "";
                                //    dr["ATTVAL008"] = "";
                                //    dr["ATTVAL009"] = sOK_PIC;
                                //    dr["ATTVAL010"] = sOK_PIC;
                                //    dr["ATTVAL011"] = ingPicTotal;
                                //    dr["ATTVAL012"] = ingPicTotal;
                                //    dr["ATTVAL013"] = sPgdate;
                                //    dr["ATTVAL014"] = sPgdate;
                                //    dr["ATTVAL015"] = iTotal_OK;
                                //    dr["ATTVAL016"] = iTotal_OK;
                                //    dr["ATTVAL017"] = "LG Chem,Ltd.";
                                //    dr["ATTVAL018"] = sLotid;

                                //    RQSTDT.Rows.Add(dr);

                                //    ZPLResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "RQSTDT", "RSLTDT", RQSTDT);

                                //    //PrintZPL(ZPLResult.Rows[0]["LABELCD"].ToString());
                                //    string sZPL = ZPLResult.Rows[0]["LABELCD"].ToString();

                                //    // 프린터 정보 조회
                                //    //string sPrt = string.Empty;
                                //    //string sRes = string.Empty;
                                //    //string sCopy = string.Empty;
                                //    //string sXpos = string.Empty;
                                //    //string sYpos = string.Empty;
                                //    //DataRow drPrtInfo = null;

                                //    //if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out drPrtInfo))
                                //    //    return;


                                //    if (PrintLabel(sZPL))
                                //        return;
                                //}

                                #endregion

                                //발행 이력 저장
                                if (iPrintCnt == 2)
                                {

                                    DataTable dtLog = new DataTable();
                                    dtLog.TableName = "RQSTDT";
                                    dtLog.Columns.Add("LABEL_CODE", typeof(string));
                                    dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                                    dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM01", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM02", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM03", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM04", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM05", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM06", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM07", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM08", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM09", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM10", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM11", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM12", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM13", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM14", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM15", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM16", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM17", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM18", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM19", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM20", typeof(string));
                                    dtLog.Columns.Add("INSUSER", typeof(string));
                                    dtLog.Columns.Add("NOTE", typeof(string));

                                    DataRow drLog = dtLog.NewRow();

                                    drLog["LABEL_CODE"] = "LBL0003";
                                    //drLog["LABEL_ZPL_CNTT"] = ZPLResult.Rows[0]["LABELCD"].ToString();
                                    drLog["LABEL_ZPL_CNTT"] = "";
                                    drLog["LABEL_PRT_COUNT"] = iPrintCnt;
                                    drLog["PRT_ITEM01"] = "299M04NP0A";
                                    drLog["PRT_ITEM02"] = "P299M04NP0A";
                                    drLog["PRT_ITEM03"] = sRanid;
                                    drLog["PRT_ITEM04"] = "A" + sRanid;
                                    drLog["PRT_ITEM05"] = sM_Roll_id;
                                    drLog["PRT_ITEM06"] = sM_Roll_id;
                                    drLog["PRT_ITEM07"] = "";
                                    drLog["PRT_ITEM08"] = "";
                                    drLog["PRT_ITEM09"] = sOK_PIC;
                                    drLog["PRT_ITEM10"] = sOK_PIC;
                                    drLog["PRT_ITEM11"] = ingPicTotal;
                                    drLog["PRT_ITEM12"] = ingPicTotal;
                                    drLog["PRT_ITEM13"] = sPgdate;
                                    drLog["PRT_ITEM14"] = sPgdate;
                                    drLog["PRT_ITEM15"] = iTotal_OK;
                                    drLog["PRT_ITEM16"] = iTotal_OK;
                                    drLog["PRT_ITEM17"] = "LG Chem,Ltd.";
                                    drLog["PRT_ITEM18"] = sLotid;
                                    drLog["PRT_ITEM19"] = "C";
                                    drLog["PRT_ITEM20"] = sBoxid;
                                    drLog["INSUSER"] = LoginInfo.USERID;
                                    drLog["NOTE"] = iPrintCnt == 1 ? RePrintcomment : null;

                                    dtLog.Rows.Add(drLog);

                                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_LABEL_HIST_NIS", "INDATA", "RSLTDT", dtLog);

                                    SeqNo = Convert.ToDecimal(dtRslt1.Rows[0]["SAVE_SEQNO"].ToString());



                                    DataTable RQSTDT = new DataTable();
                                    RQSTDT.TableName = "RQSTDT";
                                    RQSTDT.Columns.Add("LABEL_PRT_SEQNO", typeof(string));
                                    RQSTDT.Columns.Add("BOXID", typeof(string));

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LABEL_PRT_SEQNO"] = SeqNo;
                                    dr["BOXID"] = sBoxid;

                                    RQSTDT.Rows.Add(dr);

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_BOX_PRTHIS", "INDATA", null, RQSTDT);


                                }
                                // 재발행
                                else if (iPrintCnt == 1)
                                {                                    
                                    DataTable RQSTDT = new DataTable();
                                    RQSTDT.TableName = "RQSTDT";
                                    RQSTDT.Columns.Add("LABEL_PRT_SEQNO", typeof(Int32));
                                    RQSTDT.Columns.Add("NOTE", typeof(string));
                                    RQSTDT.Columns.Add("UPDUSER", typeof(string));

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LABEL_PRT_SEQNO"] = iLABEL_PRT_SEQNO;
                                    dr["NOTE"] = RePrintcomment;
                                    dr["UPDUSER"] = LoginInfo.USERID;

                                    RQSTDT.Rows.Add(dr);

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_PRT_PRTHIS", "INDATA", null, RQSTDT);
                                }

                            }
                        }
                        // 음극
                        else if (sElecrode.Equals("A"))
                        {
                            DataTable dtShipdata = Get_Ship_Data(sLotid, sElecrode);

                            if (dtShipdata == null)
                            {
                                Util.Alert("SFU3000");  //데이터 확인중 문제가 발생하였습니다.
                                return;
                            }
                            else
                            {

                                double ichgQty = Convert.ToDouble(dtShipdata.Rows[0]["GOODQTY"].ToString());
                                double idefectQty1 = Convert.ToDouble(dtShipdata.Rows[0]["QTY1"].ToString());
                                double idefectQty2 = Convert.ToDouble(dtShipdata.Rows[0]["QTY2"].ToString());
                                double idefectQty3 = Convert.ToDouble(dtShipdata.Rows[0]["QTY3"].ToString());

                                double iqty1 = ichgQty;
                                double iqty2 = ichgQty;
                                double iqty3 = ichgQty;
                                double iokPicTotal = iqty1 + iqty2 + iqty3; // 패턴값에서 NG 수 제한 값
                                double ingPicTotal = idefectQty1 + idefectQty2 + idefectQty3;

                                string sLane = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LOTID").ToString();

                                string Lane = sLane.Substring(sLane.Length - 1, 1);

                                string sS_Roll_id = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "S_ROLL_ID").ToString();

                                string sPgdate = dtShipdata.Rows[0]["YYYYMMDD"].ToString();

                                string stempQty = string.Empty;
                                double itempDefectQty = 0;
                                double itempOkPicQty1 = 0;
                                double itempOkPicQty2 = 0;
                                double itempOkPicQty3 = 0;

                                string sOK_PIC = string.Empty;

                                string sChgRanID = "A" + sRanid;

                                if (Lane.Equals("1"))
                                {
                                    stempQty = Convert.ToString(iqty1);
                                    itempOkPicQty1 = iqty1;
                                    itempDefectQty = idefectQty1;
                                }
                                else if (Lane.Equals("2"))
                                {
                                    stempQty = Convert.ToString(iqty2);
                                    itempOkPicQty2 = iqty2;
                                    itempDefectQty = idefectQty2;
                                }
                                else if (Lane.Equals("3"))
                                {
                                    stempQty = Convert.ToString(iqty3);
                                    itempOkPicQty3 = iqty3;
                                    itempDefectQty = idefectQty3;
                                }


                                // Image 버전
                                #region

                                for (int p = 0; p < iPrintCnt; p++)
                                {

                                    b.Alignment = BarcodeLib.AlignmentPositions.LEFT;

                                    b.RotateFlipType = System.Drawing.RotateFlipType.Rotate90FlipNone;
                                    System.Drawing.Image bitImg1 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sM_Roll_id, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg2 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, "0000-" + itempOkPicQty1.ToString("0000") + "-" + itempOkPicQty2.ToString("0000") + "-" + itempOkPicQty3.ToString("0000"), System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg3 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, itempDefectQty.ToString(), System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg4 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sPgdate, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg5 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sS_Roll_id, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg6 = b.Encode(BarcodeLib.TYPE.CODE39_Mod43, sS_Roll_id, System.Drawing.Color.Black, System.Drawing.Color.White);
                                    System.Drawing.Image bitImg7 = b.Encode(BarcodeLib.TYPE.CODE39, sChgRanID, System.Drawing.Color.Black, System.Drawing.Color.White);


                                    int iLng1, iLng2, iLng3, iLng4, iLng5, iLng6, iLng7;
                                    string s1, s2, s3, s4, s5, s6, s7;

                                    s1 = GetImageZplString(bitImg1, new System.Drawing.Size(240, 2508), out iLng1);     //80,836  -> 240,2508
                                    s2 = GetImageZplString(bitImg2, new System.Drawing.Size(240, 2775), out iLng2);    //80,836  -> 240,2775
                                    s3 = GetImageZplString(bitImg3, new System.Drawing.Size(240, 978), out iLng3);     //80,326  -> 240,978
                                    s4 = GetImageZplString(bitImg4, new System.Drawing.Size(240, 1266), out iLng4);    //80,422  -> 240,1266
                                    s5 = GetImageZplString(bitImg5, new System.Drawing.Size(240, 2508), out iLng5);    //80,836  -> 240,2508
                                    s6 = GetImageZplString(bitImg6, new System.Drawing.Size(240, 3456), out iLng6);    //80,1152 -> 240,3456
                                    s7 = GetImageZplString(bitImg7, new System.Drawing.Size(240, 1500), out iLng7);    //80,500  -> 240,1500

                                    StringBuilder str = new StringBuilder();

                                    b.Alignment = BarcodeLib.AlignmentPositions.LEFT;

                                    str.Append("^XA");

                                    str.Append("^MCY");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^FWN^CFD,24^PW2496^LH0,0");    //^FWN^CFD,24^PW1140^LH0,0 -> ^FWN^CFD,24^PW2496^LH0,0      ^PW = 프린트 되는 폭
                                    str.Append("^LRN^FWN^CFD,24^LH20,460");     // ^LH30,05" -> ^LH20,460"                                 ^LH = 프린트 시작위치     
                                    str.Append("^CI0^PR2^MNY^MTT^MMT^MD0^PON^PMN^LRN");
                                    str.Append(" ^SEE:UHANGUL.DAT^FS");
                                    str.Append(" ^CW1,E:KFONT3.FNT^CI26^FS");
                                    str.Append("^XZ");
                                    str.Append("^XA");


                                    str.Append("^MCY");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^DFR:TEMP_FMT.ZPL");
                                    str.Append("^LRN");

                                    b.Alignment = BarcodeLib.AlignmentPositions.LEFT;

                                    str.Append("~DYE:BARCODE6,P,P" + "," + iLng6.ToString() + ",," + s6 + "^FS");
                                    str.Append("~DYE:BARCODE1,P,P" + "," + iLng1.ToString() + ",," + s1 + "^FS");
                                    str.Append("~DYE:BARCODE2,P,P" + "," + iLng2.ToString() + ",," + s2 + "^FS");
                                    str.Append("~DYE:BARCODE3,P,P" + "," + iLng3.ToString() + ",," + s3 + "^FS");
                                    str.Append("~DYE:BARCODE4,P,P" + "," + iLng4.ToString() + ",," + s4 + "^FS");

                                    str.Append("~DYE:BARCODE7,P,P" + "," + iLng7.ToString() + ",," + s7 + "^FS");

                                    str.Append("^A0N,240,84^FO975,120^IME:BARCODE6.PNG^FS");         // ^A0N,80,28^FO325,40 -> ^A0N,240,84^FO975,120
                                    str.Append("^A0N,240,84^FO1464,120^IME:BARCODE1.PNG^FS");  //24  // ^A0N,80,28^FO488,40 -> ^A0N,240,84^FO1464,120
                                    str.Append("^A0N,240,84^FO465,120^IME:BARCODE2.PNG^FS");         // ^A0N,80,28^FO155,40 -> ^A0N,240,84^FO465,120
                                    str.Append("^A0N,240,84^FO015,114^IME:BARCODE3.PNG^FS");         // ^A0N,80,28^FO005,38 -> ^A0N,240,84^FO015,114
                                    str.Append("^A0N,240,84^FO015,1314^IME:BARCODE4.PNG^FS");        // ^A0N,80,28^FO005,438 -> ^A0N,240,84^FO015,1314

                                    str.Append("^A0N,240,84^FO1935,2400^IME:BARCODE7.PNG^FS");          // ^A0N,80,28^FO645,800 -> ^A0N,240,84^FO1935,2400

                                    str.Append("^A0N,240,84^FO2190,120^A0R, 120,60^FDPART NO :^FS");   //  24   // ^A0N,80,28^FO730,40^A0R, 40,20  -> ^A0N,240,84^FO2190,120^A0R, 120,60
                                    str.Append("^A0N,240,84^FO1920,150^BY9,2.7,240^B3R,N,240,N,N^FR^FH^FD>:P299D24NP0A^FS");   // 24  //^A0N,80,28^FO640,50^BY3,2.7,80^B3R,N,80 -> ^A0N,240,84^FO1920,150^BY9,2.7,240^B3R,N,240

                                    str.Append("^A0N,240,84^FO1701,120^A0R, 120,60^FDMR ID ^FS");  //24    //^A0N,80,28^FO567,40^A0R, 40,20  -> ^A0N,240,84^FO1701,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO1230,120^A0R, 120,60^FDSR ID ^FS");     // ^A0N,80,28^FO410,40^A0R, 40,20 -> ^A0N,240,84^FO1230,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO729,120^A0R, 120,60^FDOK PIC^FS");      //  ^A0N,80,28^FO243,40^A0R, 40,20 -> ^A0N,240,84^FO729,120^A0R, 120,60 

                                    str.Append("^A0N,240,84^FO270,120^A0R, 120,60^FDNG PIC ^FS");       //  ^A0N,80,28^FO090,40^A0R, 40,20  -> ^A0N,240,84^FO270,120^A0R, 120,60

                                    str.Append("^A0N,240,84^FO2175,600^A0R,150,75^FD299D24NP0A^FS");   // 24  // ^A0N,80,28^FO725,200^A0R,50,25  -> ^A0N,240,84^FO2175,600^A0R,150,75

                                    str.Append("^A0N,240,84^FO519,2970^A0R,150,75^FD" + sLotid + "^FS");  // 변경   // ^A0N,80,28^FO173,990^A0R,50,25 -> ^A0N,240,84^FO519,2970^A0R,150,75

                                    str.Append("^A0N,240,84^FO1701,600^A0R,150,75^FD" + sM_Roll_id + "^FS");  //24      // ^A0N,80,28^FO567,200^A0R,50,25   -> ^A0N,240,84^FO1701,600^A0R,150,75
                                    str.Append("^A0N,240,84^FO1215,600^A0R,150,75^FD" + sS_Roll_id + "^FS");            // ^A0N,80,28^FO405,200^A0R,50,25  -> ^A0N,240,84^FO1215,600^A0R,150,75
                                    str.Append("^A0N,240,84^FO729,600^A0R,150,75^FD" + "0000-" +                  //^A0N,80,28^FO243,200^A0R,50,25 -> ^A0N,240,84^FO729,600^A0R,150,75
                                                                                                itempOkPicQty1.ToString("0000") + "-" +
                                                                                                itempOkPicQty2.ToString("0000") + "-" +
                                                                                                itempOkPicQty3.ToString("0000") + "^FS");
                                    str.Append("^A0N,240,84^FO249,600^A0R,150,75^FH^FD" + itempDefectQty.ToString() + "^FS");      // ^A0N,80,28^FO083,200^A0R,50,25  -> ^A0N,240,84^FO249,600^A0R,150,75


                                    str.Append("^A0N,240,84^FO2190,2340^A0R, 120,60^FDRAN ^FS");  //24                  // ^A0N,80,28^FO730,780^A0R, 40,20 -> ^A0N,240,84^FO2190,2340^A0R, 120,60

                                    str.Append("^A0N,240,84^FO270,1317^A0R, 120,60^FDPacking Date^FS");                 // ^A0N,80,28^FO090,439^A0R, 40,20  -> ^A0N,240,84^FO270,1317^A0R, 120,60

                                    str.Append("^A0N,240,84^FO690,2970^A0R,156,174,120,150^FDLG Chem,Ltd. ^FS");   // 24      // ^A0N,80,28^FO230,990^A0R,52,58,40,50 -> ^A0N,240,84^FO690,2970^A0R,156,174,120,150

                                    str.Append("^A0N,240,84^FO249,1890^A0R,150,75^FD" + sPgdate + "^FS");                  // ^A0N,80,28^FO083,630^A0R,50,25  -> ^A0N,240,84^FO249,1890^A0R,150,75




                                    str.Append("^A0N,240,84^FO270,2715^A0R, 120,60^FDTotal OK^FS ");                        //^A0N,80,28^FO090,905^A0R, 40,20  -> ^A0N,240,84^FO270,2715^A0R, 120,60
                                    str.Append("^A0N,240,84^FO249,3090^A0R,150,75^FD" + (itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3) + "^FS");   //^A0N,80,28^FO083,1030^A0R,50,25 -> ^A0N,240,84^FO249,3090^A0R,150,75
                                    str.Append("^A0N,240,84^FO015,2790^BY9,2.7,240^B3R,N,240,N,N^FR^FH^FD>:" + (itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3) + "^FS");  //^A0N,80,28^FO005,930^BY3,2.7,80^B3R,N,80  -> ^A0N,240,84^FO015,2790^BY9,2.7,240^B3R,N,240

                                    str.Append("^A0N,240,84^FO2175,2940^A0R,150,75^FD" + sRanid + "^FS");  //24  // ^A0N,80,28^FO725,980^A0R,50,25 -> ^A0N,240,84^FO2175,2940^A0R,150,75

                                    str.Append("^FO405,120^GB0,3780,6,^FS");              // ^FO135,40^GB0,1260,2 -> ^FO405,120^GB0,3780,6
                                    str.Append("^FO894,120^GB0,3780,6,^FS");              // ^FO298,40^GB0,1260,6 -> ^FO894,120^GB0,3780,6
                                    str.Append("^FO1380,120^GB0,3780,6,^FS");             // ^FO460,40^GB0,1260,2 -> ^FO1380,120^GB0,3780,6
                                    str.Append("^FO1860,120^GB0,3780,6,^FS");  //24      // ^FO620,40^GB0,1260,2 -> ^FO1860,120^GB0,3780,6

                                    str.Append("^FO000,1200^GB414,0,6,^FS");              // ^FO000,400^GB138,0,2 -> ^FO000,1200^GB414,0,6
                                    str.Append("^FO000,2640^GB414,0,6,^FS");              // ^FO000,880^GB138,0,2 -> ^FO000,2640^GB414,0,6
                                    str.Append("^FO414,2910^GB1444,0,6,^FS");  //24       // ^FO138,970^GB483,0,2 -> ^FO414,2910^GB1444,0,6
                                    str.Append("^FO^FO1866,2250^GB450,0,6,^FS");  //24    // ^FO622,750^GB150,0,2 -> ^FO1866,2250^GB450,0,6

                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^XFR:TEMP_FMT.ZPL");
                                    str.Append("^PQ1,0,1,Y");
                                    str.Append("^XZ");
                                    str.Append("^XA");
                                    str.Append("^IDR:TEMP_FMT.ZPL");
                                    str.Append("^JBE");
                                    str.Append("^XZ");


                                    string sPrt = string.Empty;
                                    string sRes = string.Empty;
                                    string sCopy = string.Empty;
                                    string sXpos = string.Empty;
                                    string sYpos = string.Empty;
                                    string sDark = string.Empty;
                                    DataRow drPrtInfo = null;

                                    if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                        return;

                                    if (PrintLabel(str.ToString(), drPrtInfo) == false)
                                    {
                                        //라벨 발행중 문제가 발생하였습니다. IT 담당자에게 문의하세요.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3150"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                        return;
                                    }

                                }


                                #endregion


                                // GMES 버전.....ㅠ.ㅠ
                                #region

                                //DataTable ZPLResult = new DataTable();

                                // 양극 바코드 발행 메서드 호출.  
                                // 최초발행시 2장프린트
                                //for (int p = 0; p < iPrintCnt; p++)
                                //{
                                //    if (Lane.Equals("1"))
                                //    {
                                //        stempQty = Convert.ToString(iqty1);
                                //        itempOkPicQty1 = iqty1;
                                //        itempDefectQty = idefectQty1;
                                //    }
                                //    else if (Lane.Equals("2"))
                                //    {
                                //        stempQty = Convert.ToString(iqty2);
                                //        itempOkPicQty2 = iqty2;
                                //        itempDefectQty = idefectQty2;
                                //    }
                                //    else if (Lane.Equals("3"))
                                //    {
                                //        stempQty = Convert.ToString(iqty3);
                                //        itempOkPicQty3 = iqty3;
                                //        itempDefectQty = idefectQty3;
                                //    }


                                //    sOK_PIC = "0000-" + itempOkPicQty1.ToString("0000") + "-" + itempOkPicQty2.ToString("0000") + "-" + itempOkPicQty3.ToString("0000");

                                //    //string sOK_PIC = string.Empty;

                                //    //sOK_PIC = iChgQty.ToString("0000") + "-" + iQty1.ToString("0000") + "-" + iQty2.ToString("0000") + "-" + iQty3.ToString("0000");

                                //    //int iTotal_OK = 0;

                                //    //iTotal_OK = iQty1 + iQty2 + iQty3;


                                //    DataTable RQSTDT = new DataTable();
                                //    RQSTDT.TableName = "RQSTDT";
                                //    RQSTDT.Columns.Add("LBCD", typeof(string));         // 라벨코드
                                //    RQSTDT.Columns.Add("PRMK", typeof(string));         // 프린터 기종
                                //    RQSTDT.Columns.Add("RESO", typeof(string));         // 해상도
                                //    RQSTDT.Columns.Add("PRCN", typeof(string));         // 발행수량
                                //    RQSTDT.Columns.Add("MARH", typeof(string));         // Horizontal Start Position 
                                //    RQSTDT.Columns.Add("MARV", typeof(string));         // Vertical Start Position
                                //                                                        // 인쇄항목 코드와 값
                                //    RQSTDT.Columns.Add("ATTVAL001", typeof(string));      // PART NO
                                //    RQSTDT.Columns.Add("ATTVAL002", typeof(string));      // PART NO BCD
                                //    RQSTDT.Columns.Add("ATTVAL003", typeof(string));      // RAN ID
                                //    RQSTDT.Columns.Add("ATTVAL004", typeof(string));      // RAN ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL005", typeof(string));      // M Roll ID
                                //    RQSTDT.Columns.Add("ATTVAL006", typeof(string));      // M Roll ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL007", typeof(string));      // C Roll ID
                                //    RQSTDT.Columns.Add("ATTVAL008", typeof(string));      // C Roll ID BCD
                                //    RQSTDT.Columns.Add("ATTVAL009", typeof(string));      // OK PIC
                                //    RQSTDT.Columns.Add("ATTVAL010", typeof(string));      // OK PIC BCD
                                //    RQSTDT.Columns.Add("ATTVAL011", typeof(string));      // NG PIC
                                //    RQSTDT.Columns.Add("ATTVAL012", typeof(string));      // NG PIC BCD
                                //    RQSTDT.Columns.Add("ATTVAL013", typeof(string));      // Packing Date
                                //    RQSTDT.Columns.Add("ATTVAL014", typeof(string));      // Packing Date BCD
                                //    RQSTDT.Columns.Add("ATTVAL015", typeof(string));      // Total OK
                                //    RQSTDT.Columns.Add("ATTVAL016", typeof(string));      // Total OK BCD
                                //    RQSTDT.Columns.Add("ATTVAL017", typeof(string));      // 회사명
                                //    RQSTDT.Columns.Add("ATTVAL018", typeof(string));      // LOT ID

                                //    DataRow dr = RQSTDT.NewRow();
                                //    dr["LBCD"] = "LBL0003";
                                //    dr["PRMK"] = "Z";
                                //    dr["RESO"] = "600";
                                //    dr["PRCN"] = iPrintCnt;
                                //    dr["MARH"] = "0";
                                //    dr["MARV"] = "0";

                                //    dr["ATTVAL001"] = "299D24NP0A";
                                //    dr["ATTVAL002"] = "P299D24NP0A";
                                //    dr["ATTVAL003"] = sRanid;
                                //    dr["ATTVAL004"] = "A" + sRanid;
                                //    dr["ATTVAL005"] = sM_Roll_id;
                                //    dr["ATTVAL006"] = sM_Roll_id;
                                //    dr["ATTVAL007"] = sS_Roll_id;
                                //    dr["ATTVAL008"] = sS_Roll_id;
                                //    dr["ATTVAL009"] = sOK_PIC;
                                //    dr["ATTVAL010"] = sOK_PIC;
                                //    dr["ATTVAL011"] = itempDefectQty;
                                //    dr["ATTVAL012"] = itempDefectQty;
                                //    dr["ATTVAL013"] = sPgdate;
                                //    dr["ATTVAL014"] = sPgdate;
                                //    dr["ATTVAL015"] = itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3;
                                //    dr["ATTVAL016"] = itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3;
                                //    dr["ATTVAL017"] = "LG Chem,Ltd.";
                                //    dr["ATTVAL018"] = sLotid;

                                //    RQSTDT.Rows.Add(dr);

                                //    ZPLResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "RQSTDT", "RSLTDT", RQSTDT);

                                //    //PrintZPL(ZPLResult.Rows[0]["LABELCD"].ToString());
                                //    string sZPL = ZPLResult.Rows[0]["LABELCD"].ToString();

                                //    // 프린터 정보 조회
                                //    //string sPrt = string.Empty;
                                //    //string sRes = string.Empty;
                                //    //string sCopy = string.Empty;
                                //    //string sXpos = string.Empty;
                                //    //string sYpos = string.Empty;
                                //    //DataRow drPrtInfo = null;

                                //    //if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out drPrtInfo))
                                //    //    return;


                                //    if (PrintLabel(sZPL))
                                //        return;
                                //}

                                #endregion

                                if (iPrintCnt == 2)
                                {
                                    DataTable dtLog = new DataTable();
                                    dtLog.TableName = "RQSTDT";
                                    dtLog.Columns.Add("LABEL_CODE", typeof(string));
                                    dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                                    dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM01", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM02", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM03", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM04", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM05", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM06", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM07", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM08", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM09", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM10", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM11", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM12", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM13", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM14", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM15", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM16", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM17", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM18", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM19", typeof(string));
                                    dtLog.Columns.Add("PRT_ITEM20", typeof(string));
                                    dtLog.Columns.Add("INSUSER", typeof(string));
                                    dtLog.Columns.Add("NOTE", typeof(string));

                                    DataRow drLog = dtLog.NewRow();

                                    drLog["LABEL_CODE"] = "LBL0003";
                                    //drLog["LABEL_ZPL_CNTT"] = ZPLResult.Rows[0]["LABELCD"].ToString();
                                    drLog["LABEL_ZPL_CNTT"] = "";
                                    drLog["LABEL_PRT_COUNT"] = iPrintCnt;
                                    drLog["PRT_ITEM01"] = "299D24NP0A";
                                    drLog["PRT_ITEM02"] = "P299D24NP0A";
                                    drLog["PRT_ITEM03"] = sRanid;
                                    drLog["PRT_ITEM04"] = "A" + sRanid;
                                    drLog["PRT_ITEM05"] = sM_Roll_id;
                                    drLog["PRT_ITEM06"] = sM_Roll_id;
                                    drLog["PRT_ITEM07"] = sS_Roll_id;
                                    drLog["PRT_ITEM08"] = sS_Roll_id;
                                    drLog["PRT_ITEM09"] = sOK_PIC;
                                    drLog["PRT_ITEM10"] = sOK_PIC;
                                    drLog["PRT_ITEM11"] = itempDefectQty;
                                    drLog["PRT_ITEM12"] = itempDefectQty;
                                    drLog["PRT_ITEM13"] = sPgdate;
                                    drLog["PRT_ITEM14"] = sPgdate;
                                    drLog["PRT_ITEM15"] = itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3;
                                    drLog["PRT_ITEM16"] = itempOkPicQty1 + itempOkPicQty2 + itempOkPicQty3;
                                    drLog["PRT_ITEM17"] = "LG Chem,Ltd.";
                                    drLog["PRT_ITEM18"] = sLotid;
                                    drLog["PRT_ITEM19"] = "A";
                                    drLog["PRT_ITEM20"] = sBoxid;
                                    drLog["INSUSER"] = LoginInfo.USERID;
                                    drLog["NOTE"] = iPrintCnt == 1 ? RePrintcomment : null;

                                    dtLog.Rows.Add(drLog);

                                    DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_LABEL_HIST_NIS", "INDATA", "RSLTDT", dtLog);

                                    SeqNo = Convert.ToDecimal(dtRslt1.Rows[0]["SAVE_SEQNO"].ToString());


                                    DataTable RQSTDT = new DataTable();
                                    RQSTDT.TableName = "RQSTDT";
                                    RQSTDT.Columns.Add("LABEL_PRT_SEQNO", typeof(string));
                                    RQSTDT.Columns.Add("BOXID", typeof(string));

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LABEL_PRT_SEQNO"] = SeqNo;
                                    dr["BOXID"] = sBoxid;

                                    RQSTDT.Rows.Add(dr);

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_BOX_PRTHIS", "INDATA", null, RQSTDT);
                                }
                                else if (iPrintCnt == 1)
                                {
                                    DataTable RQSTDT = new DataTable();
                                    RQSTDT.TableName = "RQSTDT";
                                    RQSTDT.Columns.Add("LABEL_PRT_SEQNO", typeof(Int32));
                                    RQSTDT.Columns.Add("NOTE", typeof(string));
                                    RQSTDT.Columns.Add("UPDUSER", typeof(string));

                                    DataRow dr = RQSTDT.NewRow();
                                    dr["LABEL_PRT_SEQNO"] = iLABEL_PRT_SEQNO;
                                    dr["NOTE"] = RePrintcomment;
                                    dr["UPDUSER"] = LoginInfo.USERID;
                                    
                                    RQSTDT.Rows.Add(dr);

                                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_PRT_PRTHIS", "INDATA", null, RQSTDT);
                                }


                            }
                        }
                    }
                }

                Util.AlertInfo("SFU1236");  //인쇄 완료 되었습니다.
                fn_Search();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }


        private string GetImageZplString(System.Drawing.Image img, System.Drawing.Size sz, out int imgLength)
        {
            try
            {
                string ZPLImageDataString = string.Empty;
                System.Drawing.Image reImg = new Bitmap(img, sz);

                using (MemoryStream ms = new MemoryStream())
                {
                    string HexString = string.Empty;
                    reImg.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                    byte[] bitmapFileData = ms.GetBuffer();

                    imgLength = bitmapFileData.Length;

                    foreach (byte item in bitmapFileData)
                    {
                        string hexRep = string.Format("{0:X}", item);
                        if (hexRep.Length == 1)
                            hexRep = "0" + hexRep;

                        ZPLImageDataString += hexRep;
                    }
                }

                return ZPLImageDataString;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                imgLength = 0;
                return null;
            }
        }





        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
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
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    Util.MessageValidation("SFU3031"); //프린터 환경설정에 포트명 항목이 없습니다.
                }
            }
            else
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.

                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }

            return brtndefault;

            //FrameOperation.PrintFrameMessage(string.Empty);
            ////bool brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
            //bool brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
            
            //if (brtndefault == false)
            //{
            //    //loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
            //    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
            //}

            //return brtndefault;
        }


        private DataTable Get_Ship_Data(string sLotid, string sElectrode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                DataTable SearchResult = new DataTable();

                if (sElectrode.Equals("C"))
                {
                    //DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM1", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM2", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM3", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOTID"] = sLotid;
                    dr["CLCTITEM1"] = "CLCT4210";
                    dr["CLCTITEM2"] = "CLCT4211";
                    dr["CLCTITEM3"] = "CLCT4212";
                    dr["PROCID"] = "E3900";

                    RQSTDT.Rows.Add(dr);

                    SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LOT_QTY_C", "RQSTDT", "RSLTDT", RQSTDT);

                }
                else if (sElectrode.Equals("A"))
                {
                    //DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM1", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM2", typeof(string));
                    RQSTDT.Columns.Add("CLCTITEM3", typeof(string));
                    RQSTDT.Columns.Add("PROCID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LOTID"] = sLotid;
                    dr["CLCTITEM1"] = "P106000APC";
                    dr["CLCTITEM2"] = "P106000APC";
                    dr["CLCTITEM3"] = "P106000APC";
                    dr["PROCID"] = "E8000";

                    RQSTDT.Rows.Add(dr);

                    SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LOT_QTY", "RQSTDT", "RSLTDT", RQSTDT);

                }

                return SearchResult;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        #region [바코드 프린터 발행용]
        private void PrintZPL(string sZPL)
        {
            try
            {
                CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(sZPL);
                wndPopup.Show();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        
        private string Get_Elec(string sModelID)
        {
            try
            {
                string sElectrode = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("MODELID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["MODELID"] = sModelID;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (Electinfo, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

                    sElectrode = Electinfo.Rows[0]["ELECTRODE"].ToString();
                });

                return sElectrode;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }



        private DataTable Get_Barcode_Data(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable Get_Barcode_Data_M(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(DateTime));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private DataTable Get_Barcode_Data_C(string sLotid)
        {
            try
            {
                string sBizName = string.Empty;
                DataTable dtResult = new DataTable();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                new ClientProxy().ExecuteService("", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);                    

                    dtResult = result;
                });

                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //삭제 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    dgBOXMapping.IsReadOnly = false;
                    dgBOXMapping.RemoveRow(index);
                    dgBOXMapping.IsReadOnly = true;

                }
            });           
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            fn_Search();
        }

        private void fn_Search()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);
                string sMoterRoll = txtMotherRollID.Text.ToString().Trim();
                string sSlitRoll = txtSlitRoll.Text.ToString().Trim();
                string sRanid = txtRANID.Text.ToString().Trim();

                if (sMoterRoll == "")
                {
                    sMoterRoll = null;
                }

                if (sSlitRoll == "")
                {
                    sSlitRoll = null;
                }

                if (sRanid == "")
                {
                    sRanid = null;
                }

                if (sMoterRoll != null || sSlitRoll != null || sRanid != null)
                {
                    sStartdate = null;
                    sEnddate = null;
                }

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(string));
                RQSTDT.Columns.Add("ENDDATE", typeof(string));
                RQSTDT.Columns.Add("M_ROLL_ID", typeof(string));
                RQSTDT.Columns.Add("S_ROLL_ID", typeof(string));
                RQSTDT.Columns.Add("RAN_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sStartdate;
                dr["ENDDATE"] = sEnddate;
                dr["M_ROLL_ID"] = sMoterRoll;
                dr["S_ROLL_ID"] = sSlitRoll;
                dr["RAN_ID"] = sRanid;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_NISSAN", "RQSTDT", "RSLTDT", RQSTDT);

                //if (SearchResult.Rows.Count <= 0)
                //{
                //    Util.Alert("데이터가 없습니다.");
                //    Util.gridClear(dgBOXHist);
                //    return;
                //}

                Util.gridClear(dgBOXHist);
                //dgBOXHist.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgBOXHist, SearchResult, FrameOperation);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        //private void btnSave_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        string sRanid = string.Empty;

        //        if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 0)
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Ran ID '저장'하시기 전에 하나 이상의 박스를 선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        if (string.IsNullOrEmpty(txtRANID_Save.ToString()))
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Ran ID를 입력하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        sRanid = txtRANID_Save.ToString().Trim();

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("RAN_ID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["RAN_ID"] = sRanid;
        //        RQSTDT.Rows.Add(dr);

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_RANID_VALID", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
        //        {
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //            if (ex != null)
        //            {
        //                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }
        //            //dgBOXHist.ItemsSource = DataTableConverter.Convert(result);

        //            if (result.Rows[0]["LOTID"].ToString() != null)
        //            {
        //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("중복된 RANID 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }
        //        });

        //        for (int iCnt = 0; iCnt < dgBOXHist.Rows.Count; iCnt++)
        //        {
        //            String sLotid = string.Empty;

        //            DataTable RQSTDT1 = new DataTable();
        //            RQSTDT1.TableName = "RQSTDT";
        //            RQSTDT1.Columns.Add("LOT_ID", typeof(string));

        //            DataRow dr1 = RQSTDT1.NewRow();
        //            dr1["LOT_ID"] = Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[iCnt].DataItem, "LOT_ID").ToString());

        //            new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_ELECTRODE", "RQSTDT", "RSLTDT", RQSTDT1, (result, ex) =>
        //            {
        //                loadingIndicator.Visibility = Visibility.Collapsed;
        //                if (ex != null)
        //                {
        //                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return;
        //                }

        //                if (result.Rows[0]["ELECTRODE"].ToString() == "C")
        //                {
        //                    sRanid = fn_Get_RanID("CATHODE");
        //                }
        //                else
        //                {
        //                    sRanid = fn_Get_RanID("ANODE");
        //                }
        //            });
        //        }

        //        if (sRanid == "" || sRanid == null)
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("극성이 맞지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        // Biz 추가 필요...


        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //    }

        //}

        //private string fn_Get_RanID (string Electro)
        //{
        //    try
        //    { 
        //        String sRanid = string.Empty;

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("ELECTRO", typeof(string));
        //        RQSTDT.Columns.Add("RANID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["ELECTRO"] = Electro;
        //        dr["RANID"] = txtRANID_Save.ToString().Trim();

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_RANID", "RQSTDT", "RSLTDT", RQSTDT, (result, ex) =>
        //        {
        //            loadingIndicator.Visibility = Visibility.Collapsed;
        //            if (ex != null)
        //            {
        //                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                return;
        //            }

        //            sRanid = result.Rows[0]["LOTID"].ToString();

        //        });

        //        return sRanid;
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return null;
        //    }
        //}

        private void btnDelete_Box_Click(object sender, RoutedEventArgs e)
        {
            //C1.WPF.DataGrid.DataGridCell cell = dgBOXMapping.GetCellFromFrameworkElement(sender as Button);
            //int idx = _Util.GetDataGridCheckFirstRowIndex(dgBOXMapping, "CHK");
            //dgBOXMapping.RemoveRow(idx);

            if (dgBOXMapping.Rows.Count < 0)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("스캔된 LOT이 존재하지 않습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //return;
                Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                return;
            }

            for (int i = 0; i < dgBOXMapping.Rows.Count; i++)
            {
                if ((dgBOXMapping.GetCell(i, dgBOXMapping.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                    (bool)(dgBOXMapping.GetCell(i, dgBOXMapping.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                {
                    //dgBOXMapping.RemoveRow(i);

                    dgBOXMapping.IsReadOnly = false;
                    dgBOXMapping.RemoveRow(i);
                    dgBOXMapping.IsReadOnly = true;
                }
            }
        }

        private void btnMapping_Box_Click(object sender, RoutedEventArgs e)
        {
            try
            {               
                string sRan_ID = string.Empty;
                string sElectrode = string.Empty;

                if (dgBOXMapping.GetRowCount() == 0)
                {
                    Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                    return;
                }

                for (int i = 0; i < dgBOXMapping.Rows.Count; i++)
                {
                    DataSet indataSet = new DataSet();

                    DataTable inData = indataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(String));
                    inData.Columns.Add("BOXTYPE", typeof(String));
                    inData.Columns.Add("PRODID", typeof(String));
                    inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(String));
                    inData.Columns.Add("TOTAL_QTY", typeof(decimal));
                    inData.Columns.Add("TOTAL_QTY2", typeof(decimal));
                    inData.Columns.Add("BOXLAYER", typeof(int));
                    inData.Columns.Add("ELTR_TYPE_CODE", typeof(String));
                    inData.Columns.Add("USERID", typeof(String));
                    inData.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                    inData.Columns.Add("PROCID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["BOXTYPE"] = "BOX";
                    row["PRODID"] = DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, "PRODID").ToString();
                    row["PACK_LOT_TYPE_CODE"] = "LOT";
                    row["TOTAL_QTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, "WIPQTY").ToString());
                    row["TOTAL_QTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, "WIPQTY2").ToString());
                    row["BOXLAYER"] = 1;
                    row["ELTR_TYPE_CODE"] = DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, "ELECTRODE").ToString();
                    row["USERID"] = LoginInfo.USERID;
                    row["INSP_SKIP_FLAG"] = sSkip_Flag;
                    row["PROCID"] = "E9100";

                    indataSet.Tables["INDATA"].Rows.Add(row);


                    DataTable inLot = indataSet.Tables.Add("INLOT");
                    inLot.Columns.Add("LOTID", typeof(string));

                    DataRow row2 = inLot.NewRow();
                    row2["LOTID"] = DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, "LOTID").ToString();

                    indataSet.Tables["INLOT"].Rows.Add(row2);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACK_LOT_FOR_NISSAN", "INDATA,INLOT", null, indataSet);

                }

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

                Util.gridClear(dgBOXMapping);
                txtLOTID.Text = "";
                txtLOTID.Focus();
                chkPass.IsChecked = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private double GetSum(string sWipqty)
        {
            double dSum = 0;
            double dTotal = 0;

            for (int i = 0; i < dgBOXMapping.Rows.Count; i++)
            {
                dSum = Convert.ToDouble(DataTableConverter.GetValue(dgBOXMapping.Rows[i].DataItem, sWipqty).ToString());

                dTotal = dTotal + dSum;
            }

            return dTotal;
        }


        //private string fn_BoxID_GET()
        //{
        //    try
        //    {
        //        string sBoxID = string.Empty;

        //        //QR_GETLOTID
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        //RQSTDT.Columns.Add("SHOP", typeof(String));     // O2E
        //        //RQSTDT.Columns.Add("PROCID", typeof(String));   // 9600
        //        //RQSTDT.Columns.Add("TYPE", typeof(String));     // BOX

        //        DataRow dr = RQSTDT.NewRow();
        //        //dr[""] = "SHOP_ID";
        //        //dr[""] = "PROC_ID";
        //        //dr[""] = "TYPE";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_BOXID_GET", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (SearchResult.Rows.Count > 0)
        //        {
        //            sBoxID = SearchResult.Rows[0]["BOXID"].ToString();
        //        }
        //        else
        //        {
        //            sBoxID = null;
        //        }

        //        return sBoxID;

        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return null;
        //    }
        //}

        //private string GET_RANID(string Electro)
        //{
        //    try
        //    {
        //        string sRANID = string.Empty;

        //        //QR_GETRanID
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));
        //        RQSTDT.Columns.Add("USE_FLAG", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["ELTR_TYPE_CODE"] = Electro;
        //        dr["USE_FLAG"] = "N";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RANID", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (SearchResult.Rows.Count > 0)
        //        {
        //            sRANID = SearchResult.Rows[0]["RAN_ID"].ToString();
        //        }
        //        else
        //        {
        //            sRANID = null;
        //        }

        //        return sRANID;
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return null;
        //    }
        //}

        #endregion

        private string fn_Electro_GET(string sLotid)
        {
            try
            {
                string Electro = string.Empty;

                //극성 조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable SearchElectro = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELECTRODE_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchElectro.Rows.Count == 0)
                {                    
                    return null;
                }

                Electro = SearchElectro.Rows[0]["ELECTRODE"].ToString();

                return Electro;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        private string fn_LotTrace(string sChgid, string sProcid)
        {
            string sJUDG_FLAG = string.Empty;
            string sHOLD_FLAG = string.Empty;
            string sWIPSTAT = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sChgid;
                dr["PROCID"] = sProcid;
                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_BY_PRLOTID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    Util.Alert("SFU2061");  //ROLL 공정 재공 정보가 없습니다.
                    return "NG";
                }
                else
                {
                    string sLotid = Result.Rows[0]["LOTID"].ToString();
                    return sLotid;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return "NG";
            }
        }

        private string fn_LotID_Hold_Chk(string sLotid, string sClass_Code, string Bizname)
        {
            string sJUDG_FLAG = string.Empty;
            string sHOLD_FLAG = string.Empty;
            string sWIPSTAT = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["INSP_MED_CLASS_CODE"] = sClass_Code;
                RQSTDT.Rows.Add(dr);

                DataTable Result = new ClientProxy().ExecuteServiceSync(Bizname, "RQSTDT", "RSLTDT", RQSTDT);

                if (Result.Rows.Count == 0)
                {
                    //if (sClass_Code == "LQCM002")
                    //{
                    //    Util.Alert("SFU3001");  //ROLL 공정에서 검사가 이루어 지지 않았습니다.
                    //    return "NG";
                    //}
                    //else if (sClass_Code == "LQCM022")
                    //{
                    //    Util.Alert("SFU3002");  //REWINDER 공정에서 검사가 이루어 지지 않았습니다.                        
                    //    return "NG";
                    //}
                    //else if (sClass_Code == "LQCM017")
                    //{
                    //    Util.Alert("SFU3003");  //SLITTING 공정에서 검사가 이루어 지지 않았습니다.
                    //    return "NG";
                    //}
                    //else if (sClass_Code == "LQCM003")
                    //{
                    //    Util.Alert("SFU3004");  //전극 VD 공정에서 검사가 이루어 지지 않았습니다.
                    //    return "NG";
                    //}
                    //else
                    //{
                    //    Util.Alert("SFU2062");   //QMS 검사 이력을 확인해 주세요.
                    //    return "NG";
                    //}
                    return "NG";
                }
                else
                {
                    sJUDG_FLAG = Result.Rows[0]["JUDG_FLAG"].ToString();
                    sHOLD_FLAG = Result.Rows[0]["HOLD_FLAG"].ToString();
                    //sWIPSTAT = Result.Rows[0]["WIPSTAT"].ToString();

                    //if ((sJUDG_FLAG == "Y") && (sHOLD_FLAG == "N") && (sWIPSTAT == "WAIT"))
                    //if ((sJUDG_FLAG == "Y") && (sHOLD_FLAG == "N"))
                    if (sJUDG_FLAG == "Y")
                    {
                        return "OK";
                    }
                    else
                    {
                        //판정결과가 'F' 이더라도, HOLD_FLAG가 'N'(Release) 이면 PASS
                        if(sHOLD_FLAG == "N")
                        {
                            return "OK";

                        }

                        //if (sJUDG_FLAG == "F")
                        //{
                        //    Util.Alert("SFU3005");  //전극 출하 검사 불합격 LOT 입니다.
                        //    return "NG";
                        //}
                        //else if (sHOLD_FLAG == "Y")                        
                        //{
                        //    Util.Alert("SFU1340");  //HOLD 된 LOT ID 입니다.
                        //    return "NG";
                        //}
                        //else
                        //{
                        //    Util.Alert("SFU2062");  //QMS 검사 이력을 확인해 주세요.
                        //    return "NG";
                        //}

                        return "QMS";
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return "NG";
            }
        }

        //private string fn_LotID_Hold_Chk_Rol(string sLotid)
        //{
        //    string sJUDG_FLAG = string.Empty;
        //    string sHOLD_FLAG = string.Empty;

        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));
        //        RQSTDT.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;
        //        dr["INSP_MED_CLASS_CODE"] = "LQCM002";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_HOLD_ROL_ALL", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            sJUDG_FLAG = Result.Rows[0]["JUDG_FLAG"].ToString();
        //            sHOLD_FLAG = Result.Rows[0]["HOLD_FLAG"].ToString();

        //            if ((sJUDG_FLAG == "Y") && (sHOLD_FLAG == "E"))
        //            {
        //                return "OK";
        //            }
        //            else
        //            {
        //                if (sJUDG_FLAG != "Y")
        //                {
        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격 LOT 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    //return "NG";
        //                }

        //                if (sHOLD_FLAG != "E")
        //                {
        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD 된 LOT 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    //return "NG";
        //                }

        //                return "NG";
        //            }
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ROLL 공정에서 검사가 이루어 지지 않았습니다.. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }                
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}

        //private string fn_LotID_Hold_Chk_Rwi(string sLotid)
        //{
        //    string sJUDG_FLAG = string.Empty;
        //    string sHOLD_FLAG = string.Empty;

        //    try
        //    {
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));
        //        RQSTDT.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;
        //        dr["INSP_MED_CLASS_CODE"] = "LQCM022";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_HOLD_RWI_ALL", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            sJUDG_FLAG = Result.Rows[0]["JUDG_FLAG"].ToString();
        //            sHOLD_FLAG = Result.Rows[0]["HOLD_FLAG"].ToString();

        //            if ((sJUDG_FLAG == "Y") && (sHOLD_FLAG == "E"))
        //            {
        //                return "OK";
        //            }
        //            else
        //            {
        //                if (sJUDG_FLAG != "Y")
        //                {
        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격 LOT 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    //return "NG";
        //                }

        //                if (sHOLD_FLAG != "E")
        //                {
        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD 된 LOT 입니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    //return "NG";
        //                }

        //                return "NG";
        //            }
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ROLL 공정에서 검사가 이루어 지지 않았습니다.. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}

        #region 기존 로직 변경...
        //private string fn_LotID_Hold_Chk_Rol(string sLotid)
        //{
        //    string BoxID = string.Empty;
        //    string RESULT_TYPE = string.Empty;
        //    //string LAST_JUDGE_VAL = string.Empty;
        //    //string TR_TYPE = string.Empty;

        //    try
        //    {
        //        //QR_GET_LOTID_HOLD_ROL_ALL
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));
        //        RQSTDT.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;
        //        dr["INSP_MED_CLASS_CODE"] = "LQCM002";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_HOLD_ROL_ALL", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            // QR_GET_LOTID_HOLD_ROL
        //            DataTable RQSTDT1 = new DataTable();
        //            RQSTDT1.TableName = "RQSTDT";
        //            RQSTDT1.Columns.Add("LOTID", typeof(string));
        //            RQSTDT1.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));
        //            RQSTDT1.Columns.Add("JUDG_FLAG", typeof(string));

        //            DataRow dr1 = RQSTDT1.NewRow();
        //            dr1["LOTID"] = sLotid;
        //            dr1["INSP_MED_CLASS_CODE"] = "LQCM002";
        //            dr1["JUDG_FLAG"] = "Y";
        //            RQSTDT.Rows.Add(dr);

        //            DataTable Result1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_HOLD_ROL", "RQSTDT", "RSLTDT", RQSTDT1);

        //            if (Result1.Rows.Count > 0)
        //            {
        //                RESULT_TYPE = Result1.Rows[0]["RESULT_TYPE"].ToString();
        //                //LAST_JUDGE_VAL = Result.Rows[0]["LAST_JUDGE_VAL"].ToString();
        //                //TR_TYPE = Result.Rows[0]["TR_TYPE"].ToString();

        //                // 검사 반려/취소 된것
        //                if (RESULT_TYPE == "R" || RESULT_TYPE == "E")
        //                {
        //                    return "NG";
        //                }

        //                // QR_GET_LOTID_RELASE_ROL
        //                DataTable RQSTDT2 = new DataTable();
        //                RQSTDT2.TableName = "RQSTDT";
        //                RQSTDT2.Columns.Add("LOTID", typeof(string));
        //                RQSTDT2.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

        //                DataRow dr2 = RQSTDT2.NewRow();
        //                dr2["LOTID"] = sLotid;
        //                dr2["INSP_MED_CLASS_CODE"] = "LQCM002";
        //                RQSTDT.Rows.Add(dr);

        //                DataTable Result2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_RELASE_ROL", "RQSTDT", "RSLTDT", RQSTDT2);

        //                if (Result2.Rows.Count > 0)
        //                {
        //                    /////////////////////////////////////////////////////////////////////
        //                    // TO-BE 기준으로는 UPDATE 로직 필요 없음....그래서 제외했음. 
        //                    //
        //                    //Hold_change_Rol(slotid);
        //                    //Execute_Biz(sLotid, "");

        //                    //Release_chage_Rol(slotid);
        //                    //Execute_Biz(sLotid, "");

        //                    return "OK";
        //                    /////////////////////////////////////////////////////////////////////
        //                }
        //                else
        //                {
        //                    BoxID = Result1.Rows[0]["LOTID"].ToString();

        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격으로 HOLD 된 LOT 입니다. QA담당자에게 문의하시기 바랍니다(롤프레스 공정)."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return "NG";
        //                }
        //            }
        //            else
        //            {
        //                return "OK";
        //            }
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ROLL 공정에서 검사가 이루어 지지 않았습니다.. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}
        #endregion

        #region 기존 로직 변경....
        //private string fn_LotID_Hold_Chk_Rwi(string sLotid)
        //{
        //    string RESULT_TYPE = string.Empty;
        //    string BoxID = string.Empty;

        //    try
        //    {
        //        //QR_GET_LOTID_HOLD_RWI_ALL
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));
        //        RQSTDT.Columns.Add("INSP_MED_CLASS_CODE", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;
        //        dr["INSP_MED_CLASS_CODE"] = "LQCM022";
        //        RQSTDT.Rows.Add(dr);

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_HOLD_RWI_ALL", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            // QR_GET_LOTID_HOLD_RWI
        //            DataTable RQSTDT2 = new DataTable();
        //            RQSTDT2.TableName = "RQSTDT";
        //            RQSTDT2.Columns.Add("LOTID", typeof(string));

        //            DataRow dr2 = RQSTDT2.NewRow();
        //            dr2["LOTID"] = sLotid;

        //            DataTable Result2 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT2);

        //            if (Result2.Rows.Count > 0)
        //            {
        //                RESULT_TYPE = Result2.Rows[0][""].ToString();

        //                if (RESULT_TYPE == "R" || RESULT_TYPE == "E")
        //                {
        //                    return "NG";
        //                }

        //                // QR_GET_LOTID_RELASE_RWI
        //                DataTable RQSTDT3 = new DataTable();
        //                RQSTDT3.TableName = "RQSTDT";
        //                RQSTDT3.Columns.Add("LOTID", typeof(string));

        //                DataRow dr3 = RQSTDT3.NewRow();
        //                dr3["LOTID"] = sLotid;

        //                DataTable Result3 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT3);

        //                if (Result3.Rows.Count > 0)
        //                {
        //                    /////////////////////////////////////////////////////////////////////
        //                    // 확인필요
        //                    //Hold_change_Rwi(slotid);
        //                    //Execute_Biz(sLotid, "");

        //                    //Release_chage_Rwi(slotid);
        //                    //Execute_Biz(sLotid, "");

        //                    return "OK";
        //                    /////////////////////////////////////////////////////////////////////
        //                }
        //                else
        //                {
        //                    BoxID = Result2.Rows[0]["LOTID"].ToString();

        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격으로 HOLD 된 LOT 입니다. QA담당자에게 문의하시기 바랍니다(REWINDER 공정)"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return "NG";
        //                }
        //            }
        //            else
        //            {
        //                return "OK";
        //            }
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("REWINDER 공정에서 검사가 이루어 지지 않았습니다. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}
        #endregion

        #region 기존 로직 변경...
        //private string fn_LotID_Hold_Chk_Sli(string sLotid)
        //{
        //    string BoxID = string.Empty;
        //    string RESULT_TYPE = string.Empty;

        //    try
        //    {
        //        //QR_GET_LOTID_HOLD_SLII_ALL
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            //QR_GET_LOTID_HOLD_SLI
        //            DataTable RQSTDT1 = new DataTable();
        //            RQSTDT1.TableName = "RQSTDT";
        //            RQSTDT1.Columns.Add("LOTID", typeof(string));

        //            DataRow dr1 = RQSTDT1.NewRow();
        //            dr1["LOTID"] = sLotid;

        //            DataTable Result1 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT1);

        //            if (Result1.Rows.Count > 0)
        //            {
        //                RESULT_TYPE = Result1.Rows[0]["RESULT_TYPE"].ToString();

        //                if(RESULT_TYPE == "R" || RESULT_TYPE == "E")
        //                {
        //                    return "NG";
        //                }
        //                else
        //                {

        //                }

        //                //QR_GET_LOTID_RELASE_SLI
        //                DataTable RQSTDT2 = new DataTable();
        //                RQSTDT2.TableName = "RQSTDT";
        //                RQSTDT2.Columns.Add("LOTID", typeof(string));

        //                DataRow dr2 = RQSTDT2.NewRow();
        //                dr2["LOTID"] = sLotid;

        //                DataTable Result2 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT2);

        //                if (Result2.Rows.Count > 0)
        //                {
        //                    //Hold_change_Sli(lotid);
        //                    Execute_Biz(sLotid, "");

        //                    //Release_chage_Sli(lotid);
        //                    Execute_Biz(sLotid, "");

        //                    return "OK";
        //                }
        //                else
        //                {
        //                    BoxID = Result1.Rows[0]["LOTID"].ToString();

        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격으로 HOLD 된 LOT 입니다. QA담당자에게 문의하시기 바랍니다(SLITTER)"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return "NG";
        //                }
        //            }
        //            else
        //            {
        //                return "OK";
        //            }
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SLITTER 공정에서 검사가 이루어 지지 않았습니다. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}

        //private string fn_LotID_Hold_Chk_Vd(string sLotid)
        //{
        //    string BoxID = string.Empty;
        //    string RESULT_TYPE = string.Empty;

        //    try
        //    {
        //        // QR_GET_LOTID_HOLD_VD_ALL
        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LOTID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LOTID"] = sLotid;

        //        DataTable Result = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (Result.Rows.Count > 0)
        //        {
        //            // QR_GET_LOTID_HOLD_VD
        //            DataTable RQSTDT1 = new DataTable();
        //            RQSTDT1.TableName = "RQSTDT";
        //            RQSTDT1.Columns.Add("LOTID", typeof(string));

        //            DataRow dr1 = RQSTDT1.NewRow();
        //            dr1["LOTID"] = sLotid;

        //            DataTable Result1 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT1);

        //            if (Result1.Rows.Count > 0)
        //            {
        //                RESULT_TYPE = Result.Rows[0]["RESULT_TYPE"].ToString();

        //                if (RESULT_TYPE == "R" || RESULT_TYPE == "E")
        //                {
        //                    return "NG";
        //                }
        //                else
        //                {

        //                }

        //                // QR_GET_LOTID_RELASE_VD
        //                DataTable RQSTDT2 = new DataTable();
        //                RQSTDT2.TableName = "RQSTDT";
        //                RQSTDT2.Columns.Add("LOTID", typeof(string));

        //                DataRow dr2 = RQSTDT2.NewRow();
        //                dr2["LOTID"] = sLotid;

        //                DataTable Result2 = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT2);

        //                if (Result2.Rows.Count > 0)
        //                {
        //                    //Hold_change_Vd(lotid);
        //                    //Release_chage_Vd(lotid);

        //                    return "OK";
        //                }
        //                else
        //                {
        //                    BoxID = Result1.Rows[0]["LOTID"].ToString();

        //                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("전극 출하 검사 불합격으로 HOLD 된 LOT 입니다. QA담당자에게 문의하시기 바랍니다(VD 공정)"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //                    return "NG";
        //                }                        
        //            }
        //            else
        //            {
        //                return "OK";
        //            }                  
        //        }
        //        else
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("VD 공정에서 검사가 이루어 지지 않았습니다.. QA담당자에게 문의하시기 바랍니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return "NG";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
        //        return "NG";
        //    }
        //}
        #endregion

        private string fn_LotID_VDHold_Chk(string sLotid)
        {
            try
            {
                // QR_CPJT_VD_HOLD_CNT
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                DataTable Result = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                int cntCheck = Convert.ToInt32(Result.Rows[0]["CNT"].ToString());

                if (cntCheck >= 1)
                {
                    //VD HOLD Lot 입니다. HOLD 해제 후 박스구성 바랍니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3245"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return "NG";
                }
                else
                {
                    return "OK";
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return "NG";
            }
        }


        private void Execute_Biz(string sLotid, string sBizname)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;

                DataTable Result = new ClientProxy().ExecuteServiceSync(sBizname, "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (dgBOXMapping.CurrentRow.DataItem == null)
        //    {
        //        return;
        //    }

        //    _Util.SetDataGridUncheck(dgBOXMapping, "CHK", ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index);

        //}

        private void btnUnpack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_Util.GetDataGridCheckCnt(dgBOXHist, "CHK") < 1)
                {                    
                    Util.AlertInfo("10008");//선택된 데이터가 없습니다.
                    return;
                }

                //선택한 Box를 해체하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2064"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        for (int i = 0; i < dgBOXHist.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "CHK")) == "1")
                            {

                                DataSet indataSet = new DataSet();

                                DataTable inData = indataSet.Tables.Add("INDATA");
                                inData.Columns.Add("SRCTYPE", typeof(String));
                                inData.Columns.Add("BOXID", typeof(String));
                                inData.Columns.Add("RAN_ID", typeof(String));
                                inData.Columns.Add("PRODID", typeof(String));
                                inData.Columns.Add("PACK_LOT_TYPE_CODE", typeof(String));
                                inData.Columns.Add("UNPACK_QTY", typeof(decimal));
                                inData.Columns.Add("UNPACK_QTY2", typeof(decimal));
                                inData.Columns.Add("USERID", typeof(String));
                                inData.Columns.Add("NOTE", typeof(String));

                                DataRow row = inData.NewRow();
                                row["SRCTYPE"] = "UI";
                                row["BOXID"] = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "BOXID").ToString();
                                row["RAN_ID"] = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "RAN_ID").ToString();
                                row["PRODID"] = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "PRODID").ToString();
                                row["PACK_LOT_TYPE_CODE"] = "LOT";
                                row["UNPACK_QTY"] = Convert.ToDouble(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "WIPQTY").ToString());
                                row["UNPACK_QTY2"] = Convert.ToDouble(DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "WIPQTY2").ToString());
                                row["USERID"] = LoginInfo.USERID;
                                row["NOTE"] = "";

                                indataSet.Tables["INDATA"].Rows.Add(row);


                                DataTable inLot = indataSet.Tables.Add("INLOT");
                                inLot.Columns.Add("LOTID", typeof(string));

                                DataRow row2 = inLot.NewRow();
                                row2["LOTID"] = DataTableConverter.GetValue(dgBOXHist.Rows[i].DataItem, "LOTID").ToString();

                                indataSet.Tables["INLOT"].Rows.Add(row2);


                                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_LOT_FOR_NISSAN", "INDATA,INLOT", null, indataSet);
                            }
                        }
                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("정상 처리 되었습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                        fn_Search();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(dgPlanList.GetRowCount() == 0)
                {
                    Util.Alert("SFU2065");  //W/O 조회 데이터가 없습니다.
                    return;
                }

                if(dgPackList.GetRowCount() == 0)
                {
                    Util.Alert("SFU2066");  //포장 실적 데이터가 없습니다.
                    return;
                }

                DataRow[] drChk = Util.gridGetChecked(ref dgPlanList, "CHK");
                if (drChk.Length <= 0)
                {
                    Util.Alert("SFU1636"); //선택된 대상이 없습니다.
                    return;
                }

                string sElec = drChk[0]["PRDT_CLSS_CODE"].ToString();

                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        if (sElec != Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "ELEC")))
                        {
                            Util.Alert("SFU3006");  //선택한 W/O와 극성이 다릅니다.                            
                            return;
                        }
                    }
                }

                string sWorkorder = drChk[0]["WOID"].ToString();
                string sPlanqty = drChk[0]["PLANQTY"].ToString();

                decimal dSum = 0;
                decimal dSum2 = 0;
                decimal dTotal = 0;
                decimal dTotal2 = 0;

                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        dSum = Convert.ToDecimal(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY"));
                        dSum2 = Convert.ToDecimal(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY2"));

                        dTotal = dTotal + dSum;
                        dTotal2 = dTotal2 + dSum2;
                    }
                }

                if (Convert.ToDouble(sPlanqty) < Convert.ToDouble(dTotal))
                {
                    Util.Alert("SFU3007");  //선택한 W/O 의 계획수량을 초과하였습니다.
                    return;
                }

                DataTable dtNissanInfo = new DataTable();
                dtNissanInfo.Columns.Add("LOTID", typeof(string));
                dtNissanInfo.Columns.Add("PRODID", typeof(string));
                dtNissanInfo.Columns.Add("MODLID", typeof(string));
                dtNissanInfo.Columns.Add("WIPQTY", typeof(Decimal));
                dtNissanInfo.Columns.Add("WIPQTY2", typeof(Decimal));
                dtNissanInfo.Columns.Add("ELEC", typeof(string));
                dtNissanInfo.Columns.Add("UNIT_CODE", typeof(string));

                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        DataRow drNissan = dtNissanInfo.NewRow();

                        drNissan["LOTID"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "LOTID");
                        drNissan["PRODID"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "PRODID");
                        drNissan["MODLID"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "MODLID");
                        drNissan["WIPQTY"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY");
                        drNissan["WIPQTY2"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY2");
                        drNissan["ELEC"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "ELEC");
                        drNissan["UNIT_CODE"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "UNIT_CODE");

                        dtNissanInfo.Rows.Add(drNissan);
                    }
                }


                BOX001_004_CONFIRM wndConfirm = new BOX001_004_CONFIRM();
                wndConfirm.FrameOperation = FrameOperation;

                if (wndConfirm != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sWorkorder;
                    Parameters[1] = sPlanqty;
                    Parameters[2] = dtNissanInfo;

                    C1WindowExtension.SetParameters(wndConfirm, Parameters);

                    wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    grdMain.Children.Add(wndConfirm);
                    wndConfirm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_004_CONFIRM window = sender as BOX001_004_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                pUserInfo = window.UserInfo.ToString();
                Comfrim_Process(pUserInfo);
            }
        }

        private void Comfrim_Process(string sUserInfo)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgPlanList, "CHK");

                decimal dSum = 0;
                decimal dSum2 = 0;
                decimal dTotal = 0;
                decimal dTotal2 = 0;

                for (int i = 0; i< dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        dSum = Convert.ToDecimal(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY"));
                        dSum2 = Convert.ToDecimal(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "WIPQTY2"));

                        dTotal = dTotal + dSum;
                        dTotal2 = dTotal2 + dSum2;
                    }
                }

                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("SHOPID", typeof(string));
                inData.Columns.Add("WOID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("MOVE_ORD_QTY", typeof(decimal));
                inData.Columns.Add("MOVE_ORD_QTY2", typeof(decimal));

                DataRow row = inData.NewRow();
                row["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                row["WOID"] = drChk[0]["WOID"].ToString();
                row["USERID"] = sUserInfo;
                row["MOVE_ORD_QTY"] = dTotal;
                row["MOVE_ORD_QTY2"] = dTotal2;

                indataSet.Tables["INDATA"].Rows.Add(row);


                DataTable inLot = indataSet.Tables.Add("INBOX");

                inLot.Columns.Add("BOXID", typeof(string));

                
                for (int i = 0; i < dgPackList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        DataRow row2 = inLot.NewRow();

                        row2["BOXID"] = DataTableConverter.GetValue(dgPackList.Rows[i].DataItem, "BOXID");

                        indataSet.Tables["INBOX"].Rows.Add(row2);
                    }
                }


                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_NISSAN", "INDATA,INBOX", null, indataSet);

                Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                fn_Search_Pord();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgPlanList_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;// - 2;             

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgPlanList.SelectedIndex = idx;

                fn_Search_Pord();
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;            

            //SearchPancakeList(checkIndex);
        }

        private void btnSearch3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom3.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo3.SelectedDateTime);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STRT_DTTM", typeof(string));
                RQSTDT.Columns.Add("END_DTTM", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STRT_DTTM"] = sStartdate;
                dr["END_DTTM"] = sEnddate;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WO_FOR_NISSAN", "RQSTDT", "RSLTDT", RQSTDT);

                //if (SearchResult.Rows.Count <= 0)
                //{
                //    Util.Alert("데이터가 없습니다.");
                //    return;
                //}

                Util.gridClear(dgPlanList);
                //dgPlanList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgPlanList, SearchResult, FrameOperation, true);
            }


            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch4_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sStartdate = string.Format("{0:yyyyMMdd}", dtpDateFrom4.SelectedDateTime);
                string sEnddate = string.Format("{0:yyyyMMdd}", dtpDateTo4.SelectedDateTime);
                string sElect = string.Empty;

                if (cboElecType2.SelectedIndex < 0 || cboElecType2.SelectedValue.ToString().Trim().Equals(""))
                {
                    sElect = null;
                }
                else
                {
                    sElect = cboElecType2.SelectedValue.ToString();
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STARTDATE", typeof(string));
                RQSTDT.Columns.Add("ENDDATE", typeof(string));
                RQSTDT.Columns.Add("ELECT", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STARTDATE"] = sStartdate;
                dr["ENDDATE"] = sEnddate;
                dr["ELECT"] = sElect;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_NISSAN_IF_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                //if (SearchResult.Rows.Count <= 0)
                //{
                //    Util.Alert("데이터가 없습니다.");
                //    return;
                //}

                Util.gridClear(dgOutList);

                //dgOutList.ItemsSource = DataTableConverter.Convert(SearchResult);
                Util.GridSetData(dgOutList, SearchResult, FrameOperation);
            }


            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            fn_Search_Pord();
        }

        private void fn_Search_Pord()
        {
            try
            {
                DataTable RQSTDT = new DataTable();

                string sElect = string.Empty;
                string sProdid = string.Empty;

                //if (cboElecType.SelectedIndex < 0 || cboElecType.SelectedValue.ToString().Trim().Equals(""))
                //{
                //    sElect = null;
                //}
                //else
                //{
                //    sElect = cboElecType.SelectedValue.ToString();
                //}


                if (dgPlanList.GetRowCount() > 0)
                {
                    DataRow[] drChk = Util.gridGetChecked(ref dgPlanList, "CHK");

                    if (drChk.Length <= 0)
                    {
                        sProdid = null;
                    }
                    else
                    {
                        sProdid = drChk[0]["PRODID"].ToString();
                        sElect = drChk[0]["PRDT_CLSS_CODE"].ToString();
                    }
                }
                else
                {
                    sProdid = null;
                    sElect = null;
                }

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ELECT", typeof(string));
                RQSTDT.Columns.Add("PRINTYN", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("RAN_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ELECT"] = sElect;
                dr["PRINTYN"] = "Y";
                dr["PRODID"] = sProdid;

                string sRanIds = string.Empty;
                for (int i = 0; i < dgAdd.Rows.Count; i++)
                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID"))))
                        sRanIds += Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "RAN_ID")) + ",";

                if (!string.IsNullOrEmpty(sRanIds))
                {
                    sRanIds = sRanIds.Substring(0, sRanIds.Length - 1);
                    dr["RAN_ID"] = sRanIds;
                }

                RQSTDT.Rows.Add(dr);
                
                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_INFO_NISSAN_PROD", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgPackList);
                Util.GridSetData(dgPackList, SearchResult, FrameOperation, true);

                Util.gridClear(dgAdd);
                Initialize_dgAdd();

                if (!string.IsNullOrEmpty(sRanIds))
                    for (int i = 0; i < dgPackList.Rows.Count; i++)
                        DataTableConverter.SetValue(dgPackList.Rows[i].DataItem, "CHK", true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtRANID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtRANID.Text != "")
                    {
                        fn_Search();
                    }
                    else
                    {
                        Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtSlitRoll_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtSlitRoll.Text != "")
                    {
                        fn_Search();
                    }
                    else
                    {
                        Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtMotherRollID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtMotherRollID.Text != "")
                    {
                        fn_Search();
                    }
                    else
                    {
                        Util.Alert("SFU2067");  //스캔한 데이터가 없습니다.
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void dgAdd_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && KeyboardUtil.Ctrl)
                {
                    DataTable dt = DataTableConverter.Convert(dgAdd.ItemsSource);

                    for (int i = dgAdd.GetRowCount() - 1; i >= 0; i--)
                    {
                        dt.Rows[i].Delete();
                    }

                    string text = Clipboard.GetText();
                    string[] table = text.Split('\n');

                    if (table == null)
                    {
                        Util.Alert("SFU1482");   //다시 복사 해주세요.
                        Initialize_dgAdd();
                        return;
                    }
                    if (table.Length == 1)
                    {
                        Util.Alert("SFU1482");   //다시 복사 해주세요.
                        Initialize_dgAdd();
                        return;
                    }


                    for (int i = 0; i < table.Length - 1; i++)
                    {
                        string[] rw = table[i].Split('\t');
                        if (rw == null)
                        {
                            Util.Alert("SFU1498");   //데이터가 없습니다.
                            return;
                        }
                        if (rw.Length != 1)
                        {
                            Util.Alert("SFU1532");   //모든 항목을 다 복사해주세요.
                            Initialize_dgAdd();
                            return;
                        }
                        DataRow row = dt.NewRow();
                        row["NUMBER"] = i + 1;
                        row["RAN_ID"] = rw[0].ToUpper();

                        dt.Rows.Add(row);
                    }

                    dgAdd.BeginEdit();
                    dgAdd.ItemsSource = DataTableConverter.Convert(dt);
                    dgAdd.EndEdit();

                    dgAdd.Columns["NUMBER"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                Initialize_dgAdd();
            }
        }

        private void Initialize_dgAdd()
        {
            Util.gridClear(dgAdd);
            dgAdd.ItemsSource = null;

            DataTable dt = new DataTable();
            dt.Columns.Add("NUMBER", typeof(string));
            dt.Columns.Add("RAN_ID", typeof(string));

            DataRow row = dt.NewRow();
            row["NUMBER"] = "";
            row["RAN_ID"] = "";

            dt.Rows.Add(row);

            dgAdd.Columns["NUMBER"].Visibility = Visibility.Collapsed;

            dgAdd.BeginEdit();
            dgAdd.ItemsSource = DataTableConverter.Convert(dt);
            dgAdd.EndEdit();
        }
    }
}
