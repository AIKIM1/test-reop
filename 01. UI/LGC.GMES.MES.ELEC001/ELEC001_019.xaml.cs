/*************************************************************************************
 Created Date : 2016.11.21
      Creator : 이슬아
   Decription : 믹서원자재 수동 입고 및 라벨 발행
--------------------------------------------------------------------------------------
 [Change History]
  2016.11.21  이슬아 : 최초 생성





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_019 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private const string _SaveBizRule = "BR_PRD_REG_PALLET_FOR_RMTRL";
        private const string _UpdateBizRule = "DA_PRD_UPD_PRINT_FLAG_FROM_LABEL";   
        private const string _SearchReceivedListBizRule = "DA_PRD_SEL_RECEIVED_LIST_TODAY";
        private const string _SearchBizRule = "DA_PRD_SEL_RMTRL_LABEL_INFO_MIX";

        private const string _SelectValue = "SELECT";
        private const string _CheckColumn = "CHK";
        Util _Util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_019()
        {
            InitializeComponent();            
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();           
        }

        #endregion

        #region Event             
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            ldpMFGDate.SelectedDataTimeChanged += ldpMFGDate_SelectedDataTimeChanged;
            ldpVLDDate.SelectedDataTimeChanged += ldpVLDDate_SelectedDataTimeChanged;
        }

        private void InitControl()
        {
         //   chkTypeFlag.IsChecked = false;
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;
            ldpMFGDate.SelectedDateTime = DateTime.Now;
            ldpVLDDate.SelectedDateTime = DateTime.Now.AddDays(15);
        }
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        private void ldpMFGDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            ldpVLDDate.SelectedDateTime = ldpMFGDate.SelectedDateTime.AddDays(15);
        }

        private void ldpVLDDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;

            if (Convert.ToDecimal(ldpMFGDate.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpMFGDate.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

            if (Convert.ToDecimal(ldpMFGDate.SelectedDateTime.AddDays(15).ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpMFGDate.SelectedDateTime.AddDays(15);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }

        }
        private void chkTypeFlag_Checked(object sender, RoutedEventArgs e)
        {
            lblMFGDate.Visibility = Visibility.Visible;
            lblVLDDate.Visibility = Visibility.Visible;
            ldpMFGDate.SelectedDateTime = DateTime.Now;
            ldpVLDDate.SelectedDateTime = DateTime.Now.AddDays(15);
        }

        private void chkTypeFlag_Unchecked(object sender, RoutedEventArgs e)
        {
            lblMFGDate.Visibility = Visibility.Collapsed;
            lblVLDDate.Visibility = Visibility.Collapsed;

        }

        #region 조회조건 콤보 선택시
        private void cboArea1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea1.SelectedValue;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTGR_FOR_RMTRL_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);//DA_PRD_SEL_MTGR_FOR_RMTRL_MIX_CBO

            cboMTGR1.DisplayMemberPath = "MTGRNAME";
            cboMTGR1.SelectedValuePath = "MTGRID";

            cboMTGR1.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "MTGRID", "MTGRNAME").Copy().AsDataView();
            cboMTGR1.SelectedIndex = 0;

            txtSLotID.Text = string.Empty;
            SearchData();
        }
        
        private void cboMTGR1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("MTGRID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea1.SelectedValue;
            dr["MTGRID"] = cboMTGR1.SelectedValue;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(chkWorkOrder.IsChecked == true ? "DA_PRD_SEL_RMTRL_BY_MTGR_MIX_WO_CBO" : "DA_PRD_SEL_RMTRL_BY_MTGR_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboMTRL1.DisplayMemberPath = "MTRLDISP2";
            cboMTRL1.SelectedValuePath = "MTRLID";

            cboMTRL1.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.SELECT, "MTRLID", "MTRLDISP2").Copy().AsDataView();
            cboMTRL1.SelectedIndex = 0;
            txtSLotID.Text = string.Empty;
        }

        private void cboMTRL1_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if(cboMTRL1.Text == "")
            {
                Util.MessageValidation("SFU1828");
                return;
            }
            DataTable dtData = DataTableConverter.Convert(cboMTRL1.ItemsSource);
            DataRow drData = dtData.AsEnumerable().Where(c => c.Field<string>("MTRLID") == e.NewValue.ToString()).FirstOrDefault();
            txtMTRLName.Text = Util.NVC(drData["MTRLDESC"]);
            txtSLotID.Text = string.Empty;
        }

        private void cboArea2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea2.SelectedValue;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTGR_FOR_RMTRL_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            cboMTGR2.DisplayMemberPath = "MTGRNAME";
            cboMTGR2.SelectedValuePath = "MTGRID";

            cboMTGR2.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "MTGRID", "MTGRNAME").Copy().AsDataView();
            cboMTGR2.SelectedIndex = 0;           
        }


        private void cboMTGR2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("MTGRID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea2.SelectedValue;
            dr["MTGRID"] = cboMTGR2.SelectedValue;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(chkWorkOrder.IsChecked == true ? "DA_PRD_SEL_RMTRL_BY_MTGR_MIX_WO_CBO" : "DA_PRD_SEL_RMTRL_BY_MTGR_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboMTRL2.DisplayMemberPath = "MTRLDISP2";
            cboMTRL2.SelectedValuePath = "MTRLID";

            cboMTRL2.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "MTRLID", "MTRLDISP2").Copy().AsDataView();
            cboMTRL2.SelectedIndex = 0;
        }

        private void cboMTRL2_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dtData = DataTableConverter.Convert(cboMTRL2.ItemsSource);
            DataRow drData = dtData.AsEnumerable().Where(c => c.Field<string>("MTRLID") == e.NewValue.ToString()).FirstOrDefault();
            txtMTRLName2.Text = Util.NVC(drData["MTRLDESC"]);
        }

        private void chkResult2_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            //  DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
            dgResult2.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
        }

        #endregion

        #region 조회버튼 클릭시 조회 + loadingindicator
        /// <summary>
        /// 조회버튼 클릭 이벤트 전
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 조회버튼 클릭 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_SelectValue.Equals(cboArea2.SelectedValue))
            {
                Util.MessageValidation("SFU1499");  //동을 선택하세요.
            }
            SearchLabelData();
        }
        #endregion

        #region 입고 + 출력
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            string palletID = string.Empty;
            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)//설정값이 있으면 Label(CMI)
            {
                if (!string.IsNullOrWhiteSpace(palletID = SaveData())) PrintLabel(palletID);
            }
            else //설정값이 없으면 감열지
            {
                if (!string.IsNullOrWhiteSpace(palletID = SaveData())) Print(palletID);
            }   
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckCnt(dgResult2, _CheckColumn) < 1)
            {
                Util.MessageValidation("SFU1637");  //선택된 라벨이 없습니다.
                return;
            }
            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 0)
            {
                for (int row = 0; row < dgResult2.Rows.Count - dgResult2.BottomRows.Count; row++)
                {
                    if (_Util.GetDataGridCheckValue(dgResult2, _CheckColumn, row))
                        RePrintLabel(row);
                }
            }
            else
            {
                for (int row = 0; row < dgResult2.Rows.Count - dgResult2.BottomRows.Count; row++)
                {
                    if (_Util.GetDataGridCheckValue(dgResult2, _CheckColumn, row))
                        RePrint(row);
                }
            }
                           
        }

        #endregion

        #endregion

        #region Call Biz
        private void RePrint(int rowIndex)
        {
            try
            {
                string sPalletID = Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["RMTRL_LABEL_ID"].Index).Text);
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("PALLETID", sPalletID);
                dicParam.Add("MTGRNAME", Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["MTGRNAME"].Index).Text));
                dicParam.Add("MTRLDESC", Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["MTRLDESC"].Index).Text));
                dicParam.Add("MLOTID", Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["MTRL_LOTID"].Index).Text));
                dicParam.Add("INSDTTM", Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["INSDTTM"].Index).Text));

                LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT print = new LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT(dicParam);
                print.FrameOperation = FrameOperation;

                this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));
                UpdateData(sPalletID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Print(string sPalletID)//감열지
        {
            try
            {
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("PALLETID", sPalletID);
                dicParam.Add("MTGRNAME", cboMTGR1.Text);
                dicParam.Add("MTRLDESC", Util.NVC(((DataRowView)cboMTRL1.SelectedItem).Row["MTRLDESC"]));
                dicParam.Add("MLOTID", txtSLotID.Text);
                dicParam.Add("INSDTTM", DateTime.Today.ToShortDateString());

                LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT print = new LGC.GMES.MES.CMM001.CMM_RMTRL_PALLET_PRINT(dicParam);
                print.FrameOperation = FrameOperation;

                this.Dispatcher.BeginInvoke(new Action(() => print.ShowModal()));
                UpdateData(sPalletID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }   
        }        

        private string SaveData()
        {
            string returnValue = string.Empty;
            try
            {
                if (_SelectValue.Equals(cboArea1.SelectedValue))
                {
                    Util.MessageValidation("SFU1499");  //동을 선택하세요.
                    return returnValue;
                }

                if (_SelectValue.Equals(cboMTGR1.SelectedValue))
                {
                    Util.MessageValidation("SFU1826");  //자재군을 선택하세요.
                    return returnValue;
                }

                if (_SelectValue.Equals(cboMTRL1.SelectedValue) || string.IsNullOrWhiteSpace(cboMTRL1.Text))
                {
                    Util.MessageValidation("SFU1828");  //자재를 선택하세요.
                    return returnValue;
                }
                if (string.IsNullOrWhiteSpace(txtSLotID.Text))
                {
                    Util.MessageValidation("SFU1822");    //자재LOT을 입력하세요.
                    return returnValue;
                }

                DataTable dtData = new DataTable("RQSTDT");
                dtData.Columns.Add("AREAID", typeof(string));
                dtData.Columns.Add("MTRLID", typeof(string));
                dtData.Columns.Add("MTRL_LOTID", typeof(string));
                dtData.Columns.Add("MFG_DATE", typeof(string));
                dtData.Columns.Add("VLD_DATE", typeof(string));
                dtData.Columns.Add("USERID", typeof(string));

                DataRow row = dtData.NewRow();
                row["AREAID"] = cboArea1.SelectedValue;
                row["MTRLID"] = cboMTRL1.SelectedValue;
                row["MTRL_LOTID"] = txtSLotID.Text;
                if ((bool)chkTypeFlag.IsChecked)
                {
                    row["MFG_DATE"] = ldpMFGDate.SelectedDateTime.ToString("yyyyMMdd");
                    row["VLD_DATE"] = ldpVLDDate.SelectedDateTime.ToString("yyyyMMdd");
                }
                row["USERID"] = LoginInfo.USERID;

                dtData.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_SaveBizRule, "RQSTDT", "RSLTDT", dtData);
                if (dtResult != null)
                {
                    Util.AlertInfo("SFU1798"); //입고 처리 되었습니다.           
                    returnValue = Util.NVC(dtResult.Rows[0][0]);
                }
                SearchData();
                return returnValue;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return returnValue;
            }
        }
        private void PrintLabel(string sPalletID)//Label
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));   // 해상도
                inTable.Columns.Add("PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("ATTVAL001", typeof(string));
                inTable.Columns.Add("ATTVAL002", typeof(string));
                inTable.Columns.Add("ATTVAL003", typeof(string));
                inTable.Columns.Add("ATTVAL004", typeof(string));
                inTable.Columns.Add("ATTVAL005", typeof(string));
                inTable.Columns.Add("ATTVAL006", typeof(string));
                inTable.Columns.Add("ATTVAL007", typeof(string));
                inTable.Columns.Add("ATTVAL008", typeof(string));
                inTable.Columns.Add("ATTVAL009", typeof(string));
                inTable.Columns.Add("ATTVAL010", typeof(string));
                inTable.Columns.Add("ATTVAL011", typeof(string));
                inTable.Columns.Add("ATTVAL012", typeof(string));
                inTable.Columns.Add("ATTVAL013", typeof(string));
                inTable.Columns.Add("ATTVAL014", typeof(string));
                inTable.Columns.Add("ATTVAL015", typeof(string));


                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LBCD"] = "LBL0058";
                        indata["PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["PRCN"] = string.IsNullOrEmpty(Util.NVC(row["COPIES"])) ? "1" : Util.NVC(row["COPIES"]);
                        indata["MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["ATTVAL001"] = sPalletID;
                        indata["ATTVAL002"] = Util.NVC(((DataRowView)cboMTRL1.SelectedItem).Row["MTRLDESC"]);
                        indata["ATTVAL003"] = txtSLotID.Text;
                        indata["ATTVAL004"] = DateTime.Today.ToShortDateString();
                        indata["ATTVAL005"] = sPalletID;
                        indata["ATTVAL006"] = sPalletID;
                        indata["ATTVAL007"] = "";
                        indata["ATTVAL008"] = "";
                        indata["ATTVAL009"] = "";
                        indata["ATTVAL010"] = "";
                        indata["ATTVAL011"] = "";
                        indata["ATTVAL012"] = "";
                        indata["ATTVAL013"] = "";
                        indata["ATTVAL014"] = "";
                        indata["ATTVAL015"] = "";

                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["LABELCD"].ToString();

                    foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            FrameOperation.PrintFrameMessage(string.Empty);
                            bool brtndefault = Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME]).Equals("USB") ?
                                                           FrameOperation.Barcode_ZPL_USB_Print(zpl) : FrameOperation.Barcode_ZPL_Print(dr, zpl);
                            if (brtndefault == false)
                            {
                                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));        //Barcode Print 실패
                                return;
                            }
                        }
                    }
                }
                UpdateData(sPalletID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void RePrintLabel(int rowIndex)
        {
            try
            {
                string sPalletID = Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["RMTRL_LABEL_ID"].Index).Text);

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LBCD", typeof(string));   // 라벨코드
                inTable.Columns.Add("PRMK", typeof(string));   // 프린터기종
                inTable.Columns.Add("RESO", typeof(string));   // 해상도
                inTable.Columns.Add("PRCN", typeof(string));   // 출력매수
                inTable.Columns.Add("MARH", typeof(string));   // 시작위치H
                inTable.Columns.Add("MARV", typeof(string));   // 시작위치V
                inTable.Columns.Add("ATTVAL001", typeof(string));
                inTable.Columns.Add("ATTVAL002", typeof(string));
                inTable.Columns.Add("ATTVAL003", typeof(string));
                inTable.Columns.Add("ATTVAL004", typeof(string));
                inTable.Columns.Add("ATTVAL005", typeof(string));
                inTable.Columns.Add("ATTVAL006", typeof(string));
                inTable.Columns.Add("ATTVAL007", typeof(string));
                inTable.Columns.Add("ATTVAL008", typeof(string));
                inTable.Columns.Add("ATTVAL009", typeof(string));
                inTable.Columns.Add("ATTVAL010", typeof(string));
                inTable.Columns.Add("ATTVAL011", typeof(string));
                inTable.Columns.Add("ATTVAL012", typeof(string));
                inTable.Columns.Add("ATTVAL013", typeof(string));
                inTable.Columns.Add("ATTVAL014", typeof(string));
                inTable.Columns.Add("ATTVAL015", typeof(string));


                foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
                    {
                        DataRow indata = inTable.NewRow();
                        indata["LBCD"] = "LBL0058";
                        indata["PRMK"] = string.IsNullOrEmpty(Util.NVC(row["PRINTERTYPE"])) ? "Z" : string.Equals(row["PRINTERTYPE"], "Datamax") ? "D" : "Z";
                        indata["RESO"] = string.IsNullOrEmpty(Util.NVC(row["DPI"])) ? "203" : Util.NVC(row["DPI"]);
                        indata["PRCN"] = LoginInfo.CFG_LABEL_COPIES;
                        indata["MARH"] = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
                        indata["MARV"] = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
                        indata["ATTVAL001"] = sPalletID;
                        indata["ATTVAL002"] = Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["MTRLDESC"].Index).Text);
                        indata["ATTVAL003"] = Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["MTRL_LOTID"].Index).Text);
                        indata["ATTVAL004"] = Util.NVC(dgResult2.GetCell(rowIndex, dgResult2.Columns["INSDTTM"].Index).Text);
                        indata["ATTVAL005"] = sPalletID;
                        indata["ATTVAL006"] = sPalletID;
                        indata["ATTVAL007"] = "";
                        indata["ATTVAL008"] = "";
                        indata["ATTVAL009"] = "";
                        indata["ATTVAL010"] = "";
                        indata["ATTVAL011"] = "";
                        indata["ATTVAL012"] = "";
                        indata["ATTVAL013"] = "";
                        indata["ATTVAL014"] = "";
                        indata["ATTVAL015"] = "";

                        inTable.Rows.Add(indata);

                        break;
                    }
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM15", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count == 0)
                {
                    return;
                }
                else
                {
                    string zpl = dtMain.Rows[0]["LABELCD"].ToString();

                    foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                    {
                        if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                        {
                            FrameOperation.PrintFrameMessage(string.Empty);
                            bool brtndefault = Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPORT_PORTNAME]).Equals("USB") ?
                                                           FrameOperation.Barcode_ZPL_USB_Print(zpl) : FrameOperation.Barcode_ZPL_Print(dr, zpl);
                            if (brtndefault == false)
                            {
                                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));        //Barcode Print 실패
                                return;
                            }
                        }
                    }
                }
                UpdateData(sPalletID);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UpdateData(string sLabelID)
        {
            try
            {
                DataTable dtData = new DataTable("RQSTDT");
                dtData.Columns.Add("LABELID", typeof(string));

                DataRow row = dtData.NewRow();
                row["LABELID"] = sLabelID;
                dtData.Rows.Add(row);

                new ClientProxy().ExecuteService(_UpdateBizRule, "RQSTDT", null, dtData, (result, ex) =>
                {
                    if (ex != null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage(ex.Message), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        Util.AlertByBiz(_UpdateBizRule, ex.Message, ex.ToString());
                        return;
                    }
                    //   Util.AlertInfo("SFU1275"); //정상처리되었습니다.
                    if(PRINT.IsSelected)
                    {
                        SearchData();
                    }
                    else if(REPRINT.IsSelected)
                    {
                        SearchLabelData();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SearchData()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow row = RQSTDT.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = cboArea1.SelectedValue;
                RQSTDT.Rows.Add(row);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_SearchReceivedListBizRule, "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgResult1, dtResult, FrameOperation);
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

        private void SearchLabelData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("MTGRID", typeof(string));
                RQSTDT.Columns.Add("MTRLID", typeof(string));
                RQSTDT.Columns.Add("DATEFROM", typeof(string));
                RQSTDT.Columns.Add("DATETO", typeof(string));
                RQSTDT.Columns.Add("MES_GNRT_FLAG", typeof(string));
                

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea2.SelectedValue;
                if (!cboMTGR2.SelectedValue.Equals(""))
                    dr["MTGRID"] = cboMTGR2.SelectedValue;
                if(!cboMTRL2.SelectedValue.Equals(""))
                    dr["MTRLID"] = cboMTRL2.SelectedValue;
                dr["DATEFROM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["DATETO"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dr["MES_GNRT_FLAG"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(_SearchBizRule, "RQSTDT", "RSLTDT", RQSTDT);//DA_PRD_SEL_RMTRL_LABEL_INFO
                Util.GridSetData(dgResult2, dtResult, FrameOperation);
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

        #region Mehod

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
           
            //동
            _combo.SetCombo(cboArea1, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID },sCase: "AREA");
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID }, sCase: "AREA");            
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {

            DataRow dr = dt.NewRow();
            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = _SelectValue;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;


        }

        private void chkWorkOrder_Checked(object sender, RoutedEventArgs e)
        {
            cboMTGR1_SelectedValueChanged(null, null);
        }
        #endregion
    }
}
