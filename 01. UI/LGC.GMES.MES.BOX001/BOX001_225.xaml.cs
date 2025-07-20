/*************************************************************************************
 Created Date : 2018.07.12
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.12  DEVELOPER : Initial Created.
  2019.01.21  INS 김동일K : 포장대기 기능 추가 (C20181115_44802)
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media.Animation;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_225 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private Util _Util = new Util();
        private System.Windows.Threading.DispatcherTimer timer;
        private LGC.GMES.MES.CMM001.CMM_ELEC_SAMPLING_OQC_RP sampling;
        private DataTable OriginSamplingData;
        private bool IsSamplingCheck = false;
        public string sTempLot_1 = string.Empty;
        public string sTempLot_2 = string.Empty;
        public string sTempLot_3 = string.Empty;
        public string sNote2 = string.Empty;
        private string _APPRV_PASS_NO = string.Empty;
        public Boolean bReprint = true;
        private BOX001_225_PACKINGCARD_MERGE window02 = new BOX001_225_PACKINGCARD_MERGE();
        public bool bNew_Load = false;
        public DataTable dtPackingCard;
        public bool bCancel = false;
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public BOX001_225()
        {
            InitializeComponent();

            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPackOut);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            #region Quality Check [자동차 1,2동만 적용]
            //2022-12-29 오화백  동 :EP 추가 
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                // SampleData
                SetActSamplingData();

                timer.Tick -= timer_Start;
                timer.Tick += timer_Start;
                timer.Start();
            }
            #endregion
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {    //2022-12-29 오화백  동 :EP 추가 
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                timer.Stop();
                timer.Tick -= timer_Start;
            }
        }

        #endregion


        #region Initialize
        private void Initialize()
        {

            #region Combo Setting
            CommonCombo _combo = new CommonCombo();

            String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            String[] sFilter2 = { "WH_DIVISION" };            

            _combo.SetCombo(cboTransLoc2, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "TRANSLOC");
            _combo.SetCombo(cboType, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");                                    
            #endregion

            cboType.SelectedValue = "PANCAKE";

            #region Quality Check [자동차 1,2동만 적용] 
            //2022-12-29 오화백  동 :EP 추가 
            if (string.Equals(LoginInfo.CFG_AREA_ID, "E5") || string.Equals(LoginInfo.CFG_AREA_ID, "E6") || string.Equals(LoginInfo.CFG_AREA_ID, "EP"))
            {
                stQuality.Visibility = Visibility.Visible;
                timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMinutes(1d); //1분 간격으로 설정

                btnStandbyInv.Visibility = Visibility.Visible;
            }
            else
            {
                btnStandbyInv.Visibility = Visibility.Collapsed;
            }
            #endregion

            txtLotID.Focus();
        }

        #endregion


        #region Event

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLotid = string.Empty;
                string sCstID = string.Empty;
                string sCstID2 = string.Empty;

                try
                {                    
                    if (sender.GetType() == typeof(string))
                    {
                        sLotid = sender.ToString();
                    }
                    else
                    {
                        sLotid = txtLotID.Text.ToString().Trim();
                    }

                    if (sLotid == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    // 출고 이력 조회
                    DataTable RQSTDT0 = new DataTable();
                    RQSTDT0.TableName = "RQSTDT";
                    RQSTDT0.Columns.Add("CSTID", typeof(String));
                    RQSTDT0.Columns.Add("AREAID", typeof(String));

                    DataRow dr0 = RQSTDT0.NewRow();
                    dr0["CSTID"] = sLotid;
                    dr0["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT0.Rows.Add(dr0);

                    DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                    int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                    if (iCnt <= 0)
                    {
                        Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                        return;
                    }

                    // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("LOTID", typeof(String));
                    RQSTDT1.Columns.Add("PROCID", typeof(String));
                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LOTID"] = sLotid;
                    dr1["PROCID"] = "E7000";
                    dr1["WIPSTAT"] = "WAIT";
                    RQSTDT1.Rows.Add(dr1);

                    DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT1);
                    if (Prod_Result.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");   //재공 정보가 없습니다.
                        return;
                    }

                    for (int i = 0; i < Prod_Result.Rows.Count; i++)
                    {
                        if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                        {
                            Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                            return;
                        }
                    }
                    #region # 시생산 Lot 
                    DataRow[] dRow = Prod_Result.Select("LOTTYPE = 'X'");
                    if (dRow.Length > 0)
                    {
                        Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다
                        return;
                    }
                    #endregion
                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dt.Columns.Add("COM_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                    dr["COM_CODE"] = "QMS_NOCHECK_PACKING";
                    dt.Rows.Add(dr);

                    DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);
                    if (AreaResult.Rows.Count == 0)
                    {
                        DataTable dtChk = new DataTable();
                        dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                        DataRow drChk = dtChk.NewRow();
                        drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();
                        dtChk.Rows.Add(drChk);

                        DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);

                        if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                        {
                            // 품질결과 검사 체크
                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotid;
                            row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);
                            loadingIndicator.Visibility = Visibility.Visible;

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (Exception != null)
                                {
                                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                    Util.MessageException(Exception);
                                    return;
                                }

                                Search_Pancake(sLotid);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            // 품질결과 Skip
                            Search_Pancake(sLotid);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else
                    {
                        Search_QMS_Validation(sLotid);
                        // 품질결과 Skip
                        Search_Pancake(sLotid);

                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }                    
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
            }
        }

        private void txtLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    try
                    {
                        //ShowLoadingIndicator();

                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);                        

                        foreach (string item in sPasteStrings)
                        {
                            KeyEventArgs args = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Enter);
                            txtLotID_KeyDown(item, args);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOut);
            dgSub.Children.Clear();

            dgOut.IsEnabled = true;
            txtLotID.IsReadOnly = false;
            btnPackOut.IsEnabled = true;
            txtLotID.Text = "";
            sNote2 = string.Empty;
            txtCARRIERID.IsReadOnly = false;
            txtCARRIERID.Text = string.Empty;

            txtLotID.Focus();

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgOut.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }
                
                txtLotID.IsReadOnly = true;
                txtCARRIERID.IsReadOnly = true;
                btnPackOut.IsEnabled = false;                
                dgOut.IsEnabled = false;                                
                bReprint = false;
                                
                Get_Sub_Merge();
                
                txtLotID.Text = "";                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                
                dgOut.IsEnabled = true;
                txtLotID.IsReadOnly = false;
                btnPackOut.IsEnabled = true;
                txtLotID.Text = "";
                txtLotID.Focus();
                return;
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

                    dgOut.IsReadOnly = false;
                    dgOut.RemoveRow(index);
                    dgOut.IsReadOnly = true;
                }
            });
        }

        private void cboType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sLot_Type = cboType.SelectedValue.ToString();

            if (sLot_Type == "JUMBO_ROLL")
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("출고");

                dgOut.Columns["M_WIPQTY"].Header = "S/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "N/ROLL";
            }
            else
            {
                btnPackOut.Content = ObjectDic.Instance.GetObjectName("포장구성");

                dgOut.Columns["M_WIPQTY"].Header = "C/ROLL";
                dgOut.Columns["CELL_WIPQTY"].Header = "S/ROLL";
            }
        }

        #endregion


        #region Method

        private void Search_Pancake(string sLotid, bool bSkipShipCompany = false)
        {
            try
            {
                string sCstID = string.Empty;
                int iCheckCounts = 1;

                // 재공정보 조회
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LOTID", typeof(String));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LOTID"] = sLotid;

                RQSTDT2.Rows.Add(dr2);

                DataTable Lot_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUT_LIST_BY_PANCAKE", "RQSTDT", "RSLTDT", RQSTDT2);

                if (Lot_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                string ValueToKey = string.Empty;
                string ValueToFind = string.Empty;

                Dictionary<string, string> ValueToCompany = getShipCompany(Util.NVC(Lot_Result.Rows[0]["PRODID"]));
                foreach (KeyValuePair<string, string> items in ValueToCompany)
                {
                    ValueToKey = items.Key;
                    ValueToFind = items.Value;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    // 포장 대기 팝업에서 이미 처리한 로직 스킵 처리.
                    if (bSkipShipCompany)
                    {
                        Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                    else
                    {
                        if (!string.Equals(ValueToKey, string.Empty))
                        {
                            Util.MessageConfirm("SFU5048", (result) =>
                            {
                                if (result == MessageBoxResult.Cancel)
                                {
                                    txtLotID.SelectAll();
                                    txtLotID.Focus();
                                    return;
                                }
                                Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                                txtLotID.SelectAll();
                                txtLotID.Focus();
                            }, new object[] { ValueToFind, ValueToKey });
                        }
                        else
                        {
                            Util.GridSetData(dgOut, Lot_Result, FrameOperation);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }                    
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                        #region # 시생산 Lot 
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE").ToString() != Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]))
                        {
                            if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTTYPE").ToString().Equals("X") || Util.NVC(Lot_Result.Rows[0]["LOTTYPE"]).Equals("X"))
                            {
                                Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다.
                                return;
                            }
                        }
                        #endregion
                    }

                    string sCstID1 = string.Empty;
                    string sCstID2 = string.Empty;

                    if (dgOut.Rows.Count > 0)
                        sCstID1 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "CSTID")).ToString();
                    for (int i = 1; i < dgOut.GetRowCount(); i++)
                    {
                        if (!sCstID1.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                        {
                            if (!sCstID2.Equals(Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"))))
                            {
                                sCstID2 = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "CSTID"));
                                iCheckCounts++;
                            }
                        }
                        if (iCheckCounts == 2)
                        {
                            if (!sCstID2.Equals(Lot_Result.Rows[0]["CSTID"].ToString()) && !sCstID1.Equals(Lot_Result.Rows[0]["CSTID"].ToString()))
                                iCheckCounts++;
                        }
                    }

                    //if (iCheckCounts > 2)
                    //{
                    //    Util.MessageValidation("SFU3015"); //최대 2개 SKID까지 포장가능합니다.
                    //    return;
                    //}

                    if (Lot_Result.Rows[0]["MKT_TYPE_CODE"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "MKT_TYPE_CODE").ToString())
                    {
                        Util.Alert("SFU4454");   //내수용과 수출용은 같이 포장할 수 없습니다.
                        return;
                    }

                    if (Lot_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }

                    // 포장 대기 팝업에서 이미 처리한 로직 스킵 처리.
                    if (bSkipShipCompany)
                    {
                        BindingPancake(Lot_Result);
                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                    else
                    {
                        if (!string.Equals(ValueToKey, string.Empty))
                        {
                            Util.MessageConfirm("SFU5048", (result) =>
                            {
                                if (result == MessageBoxResult.Cancel)
                                {
                                    txtLotID.SelectAll();
                                    txtLotID.Focus();
                                    return;
                                }
                                BindingPancake(Lot_Result);
                                txtLotID.SelectAll();
                                txtLotID.Focus();
                            }, new object[] { ValueToFind, ValueToKey });
                        }
                        else
                        {
                            BindingPancake(Lot_Result);
                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }                        
                }
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }
        private void BindingPancake(DataTable dt)
        {
            dgOut.IsReadOnly = false;
            dgOut.BeginNewRow();
            dgOut.EndNewRow(true);
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", dt.Rows[0]["LOTID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CSTID", dt.Rows[0]["CSTID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", dt.Rows[0]["PRODID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LANE_QTY", dt.Rows[0]["LANE_QTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", dt.Rows[0]["M_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", dt.Rows[0]["CELL_WIPQTY"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", dt.Rows[0]["MODLID"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MKT_TYPE_CODE", dt.Rows[0]["MKT_TYPE_CODE"].ToString());
            DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTTYPE", dt.Rows[0]["LOTTYPE"].ToString());
            dgOut.IsReadOnly = true;
        }

        private void Search_QMS_Validation(string sLotid)
        {
            //WIP HOLD 체크
            DataTable dt2 = new DataTable();
            dt2.Columns.Add("LOTID", typeof(string));

            DataRow dr2 = dt2.NewRow();
            dr2["LOTID"] = sLotid;

            dt2.Rows.Add(dr2);

            DataTable Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_FOR_HOLD_CHECK", "RQSTDT", "RSLTDT", dt2);
            if (Result.Rows.Count != 0)
            {
                if (Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                                             //return;
                }
                                
                DataTable dt3 = new DataTable();
                dt3.Columns.Add("LOTID", typeof(string));

                DataRow dr3 = dt3.NewRow();
                dr3["LOTID"] = sLotid;

                dt3.Rows.Add(dr3);

                DataTable Result2 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_QMS_INFO", "RQSTDT", "RSLTDT", dt3);

                // 포장카드에는“ OQC 검사 결과 없음”포장작업은 정상처리
                if (Result2.Rows.Count == 0)
                {
                    Util.Alert("SFU3492");   // 품질검사 결과가 없어서 출하가 불가합니다. 공정사에게 보고하세요.
                    sNote2 = ObjectDic.Instance.GetObjectName("OQC 검사 결과 없음");

                    // 품질검서 없을시 등록된 인원들에 대해 BIZ 내에서 메일을 보냄
                    DataTable dt4 = new DataTable();
                    dt4.TableName = "INDATA";
                    dt4.Columns.Add("LANGID", typeof(string));
                    dt4.Columns.Add("SKIDID", typeof(string));

                    DataRow dr4 = dt4.NewRow();
                    dr4["LANGID"] = LoginInfo.LANGID;
                    dr4["SKIDID"] = sLotid;

                    dt4.Rows.Add(dr4);
                    new ClientProxy().ExecuteService("BR_PRD_CHK_QMS_FOR_MAILING", "INDATA", null, dt4, (bizResult, bizException) =>
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                    });
                }
            }
        }

        private void Search_Roll(string sLotid)
        {
            try
            {
                // 재공정보 조회
                DataTable RQSTDT1 = new DataTable();
                RQSTDT1.TableName = "RQSTDT";
                RQSTDT1.Columns.Add("LOTID", typeof(String));
                RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                DataRow dr1 = RQSTDT1.NewRow();
                dr1["LOTID"] = sLotid;
                //dr1["PROCID"] = "E7000";  //<= 확인 필요
                dr1["WIPSTAT"] = "WAIT";

                RQSTDT1.Rows.Add(dr1);

                DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID_JB", "RQSTDT", "RSLTDT", RQSTDT1);

                if (Prod_Result.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                if (Prod_Result.Rows[0]["WIPHOLD"].ToString().Equals("Y"))
                {
                    Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                    return;
                }

                if (dgOut.GetRowCount() == 0)
                {
                    dgOut.Columns["LANE_QTY"].Visibility = Visibility.Collapsed;
                    Util.GridSetData(dgOut, Prod_Result, FrameOperation);
                }
                else
                {
                    for (int i = 0; i < dgOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgOut.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                        {
                            Util.Alert("SFU1914");   //중복 스캔되었습니다.
                            return;
                        }
                    }

                    if (Prod_Result.Rows[0]["PRODID"].ToString() != DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "PRODID").ToString())
                    {
                        Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                        return;
                    }
                    
                    dgOut.IsReadOnly = false;
                    dgOut.BeginNewRow();
                    dgOut.EndNewRow(true);
                    dgOut.IsReadOnly = true;

                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "LOTID", Prod_Result.Rows[0]["LOTID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "PRODID", Prod_Result.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "M_WIPQTY", Prod_Result.Rows[0]["M_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "CELL_WIPQTY", Prod_Result.Rows[0]["CELL_WIPQTY"].ToString());
                    DataTableConverter.SetValue(dgOut.CurrentRow.DataItem, "MODLID", Prod_Result.Rows[0]["MODLID"].ToString());                    
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
                return;
            }
        }

        private void Get_Sub_Merge()
        {
            if (dgSub.Children.Count == 0)
            {
                bNew_Load = true;
                window02.BOX001_225 = this;
                window02.FrameOperation = this.FrameOperation; //[E20230227-000318]전극 포장 이력카드 개선건
                window02.PancakeList = dgOut.ItemsSource as DataView;
                //window02.SkidID = Util.NVC(DataTableConverter.GetValue(dgOut.Rows[0].DataItem, "CSTID"));

                dgSub.Children.Add(window02);
            }
        }

        #endregion


        #region 자동차동 SAMPLING 전용 FUNCTION
        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
            sampling.FrameOperation = FrameOperation;

            if (sampling != null)
            {
                C1WindowExtension.SetParameters(sampling, null);

                sampling.Closed -= new EventHandler(OnCloseSampling);
                sampling.Closed += new EventHandler(OnCloseSampling);
                this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
            }
        }

        private void timer_Start(object sender, EventArgs e)
        {
            if (sampling == null && IsSamplingCheck == false)
            {
                SetActSamplingData();
            }
        }

        private void OnCloseSampling(object sender, EventArgs e)
        {
            CMM001.CMM_ELEC_SAMPLING_OQC_RP window = sender as CMM001.CMM_ELEC_SAMPLING_OQC_RP;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                if (IsSamplingCheck)
                {
                    IsSamplingCheck = false;
                    Storyboard board = (Storyboard)this.Resources["storyBoard"];
                    if (board != null)
                        board.Stop();
                }
            }

            sampling.Close();
            sampling = null;
            GC.Collect();
        }

        private void SetActSamplingData()
        {
            try
            {
                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.ROLL_PRESSING;
                IndataTable.Rows.Add(Indata);

                //DA_PRD_SEL_LOT_SAMPLE_CNA_QA - > DA_PRD_SEL_LOT_SAMPLE_QA 변경
                //사용 UI화면과 DA가 다름 동일하게 처리하기 위하여 변경 작업
                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_SAMPLE_QA", "INDATA", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                            throw searchException;

                        if (OriginSamplingData == null)
                            OriginSamplingData = result;
                        else
                            IsDiffSamplingData(OriginSamplingData, result);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                });
            }
            catch (Exception ex) { }
        }

        private void IsDiffSamplingData(DataTable oldData, DataTable newData)
        {
            bool IsChangeSampling = false;
            foreach (DataRow oldRow in oldData.Rows)
            {
                foreach (DataRow newRow in newData.Rows)
                {
                    if (string.Equals(oldRow["LOTID"], newRow["LOTID"]))
                    {
                        // 변경된 데이터 검증
                        if (!string.Equals(oldRow["JUDG_FLAG"], newRow["JUDG_FLAG"]))
                            IsChangeSampling = true;

                        // 미검사 OR 불합격 판정 -> 합격 변경 시
                        if ((string.IsNullOrEmpty(Util.NVC(oldRow["JUDG_FLAG"])) || string.Equals(oldRow["JUDG_FLAG"], "F")) && string.Equals(newRow["JUDG_FLAG"], "Y") && IsSamplingCheck == false)
                        {
                            IsSamplingCheck = true;
                            Storyboard board = (Storyboard)this.Resources["storyBoard"];
                            if (board != null)
                                board.Begin();

                            // 팝업 자동 생성
                            if (sampling == null && chkQuality.IsChecked == true)
                            {
                                sampling = new CMM001.CMM_ELEC_SAMPLING_OQC_RP();
                                sampling.FrameOperation = FrameOperation;

                                if (sampling != null)
                                {
                                    C1WindowExtension.SetParameters(sampling, null);

                                    sampling.Closed -= new EventHandler(OnCloseSampling);
                                    sampling.Closed += new EventHandler(OnCloseSampling);
                                    this.Dispatcher.BeginInvoke(new Action(() => sampling.ShowModal()));
                                }
                            }
                            break;
                        }
                    }
                }
            }

            // 갱신이 존재하면 신규 데이터로 변경 요청
            if (IsChangeSampling == false || (oldData.Rows.Count != newData.Rows.Count))
                IsChangeSampling = true;

            if (IsChangeSampling == true)
            {
                OriginSamplingData.Clear();
                OriginSamplingData = newData;
            }
        }

        private Dictionary<string, string> getShipCompany(string sProdID)
        {
            try
            {
                Dictionary<string, string> sCompany = new Dictionary<string, string>();

                DataTable IndataTable = new DataTable("INDATA");
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRODID"] = sProdID;

                IndataTable.Rows.Add(Indata);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SMPLG_SHIP_COMPANY", "RQSTDT", "RSLTDT", IndataTable);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return new Dictionary<string, string> { { string.Empty, string.Empty } };

                DataTable ShipTo = new DataTable("INDATA");
                ShipTo.Columns.Add("SHIPTO_ID", typeof(string));

                DataRow ShipToIndata = ShipTo.NewRow();
                ShipToIndata["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();

                ShipTo.Rows.Add(ShipToIndata);

                DataTable dtShipTo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", ShipTo);

                if (dtShipTo == null || dtShipTo.Rows.Count == 0)
                    return new Dictionary<string, string> { { string.Empty, string.Empty } };

                DataRow[] dr = dtResult.Select("COMPANY_CODE = '" + dtShipTo.Rows[0]["COMPANY_CODE"].ToString() + "'");

                if (dr.Length == 0 || dr == null)
                {
                    var ShipCompany = new List<string>();
                    foreach (DataRow dRow in dtResult.Rows)
                    {
                        ShipCompany.Add(Util.NVC(dRow["COMPANY_CODE"]));
                    }
                    sCompany.Add(dtShipTo.Rows[0]["COMPANY_CODE"].ToString(), string.Join(",", ShipCompany));
                }
                return sCompany;
            }
            catch (Exception ex) { }

            return new Dictionary<string, string> { { string.Empty, string.Empty } };
        }
        #endregion

        private void btnStandbyInv_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                BOX001_225_STANDBY_INVENTORY wndStdInv = new BOX001_225_STANDBY_INVENTORY();
                wndStdInv.FrameOperation = FrameOperation;
                
                if (wndStdInv != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = "";
                    Parameters[1] = "";

                    C1WindowExtension.SetParameters(wndStdInv, Parameters);

                    wndStdInv.PackingOutEvent -= new BOX001_225_STANDBY_INVENTORY.PackingOutDataHandler(SetSubMergeByStdInvEvent);
                    wndStdInv.PackingOutEvent += new BOX001_225_STANDBY_INVENTORY.PackingOutDataHandler(SetSubMergeByStdInvEvent);
                    wndStdInv.Closed -= new EventHandler(wndStdInv_Closed);
                    wndStdInv.Closed += new EventHandler(wndStdInv_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => wndStdInv.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void wndStdInv_Closed(object sender, EventArgs e)
        {
            BOX001_225_STANDBY_INVENTORY window = sender as BOX001_225_STANDBY_INVENTORY;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }

            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void SetSubMergeByStdInvEvent(object sender, string sTransLoc, string sPackWay)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        btnRefresh_Click(null, null);

                        if (sTransLoc?.Length > 0)
                            cboTransLoc2.SelectedValue = sTransLoc;
                        if (sPackWay?.Length > 0)
                            window02.cboPackWay.SelectedValue = sPackWay;

                        if (!ScanProcess(sender as DataTable))
                        {
                            btnRefresh_Click(null, null);
                            return;
                        }

                        System.Threading.Thread.Sleep(1000);
                        btnPackOut_Click(null, null);
                    }
                    catch (Exception ex)
                    {                        
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }));
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private bool ScanProcess(DataTable dtSelData)
        {
            try
            {
                bool bRet = false;
                string sLotid = string.Empty;
                
                foreach (DataRow drTmp in dtSelData?.Rows)
                {
                    sLotid = drTmp["LOTID"].ToString();

                    if (sLotid == "")
                    {
                        Util.Alert("SFU2060");   //스캔한 데이터가 없습니다.
                        return bRet;
                    }

                    // 출고 이력 조회
                    DataTable RQSTDT0 = new DataTable();
                    RQSTDT0.TableName = "RQSTDT";
                    RQSTDT0.Columns.Add("CSTID", typeof(String));
                    RQSTDT0.Columns.Add("AREAID", typeof(String));

                    DataRow dr0 = RQSTDT0.NewRow();
                    dr0["CSTID"] = sLotid;
                    dr0["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT0.Rows.Add(dr0);

                    DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                    int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                    if (iCnt <= 0)
                    {
                        Util.Alert(MessageDic.Instance.GetMessage("SFU3017") + " [" + sLotid + "]"); //출고 대상이 없습니다.
                        return bRet;
                    }

                    // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("LOTID", typeof(String));
                    RQSTDT1.Columns.Add("PROCID", typeof(String));
                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LOTID"] = sLotid;
                    dr1["PROCID"] = "E7000";
                    dr1["WIPSTAT"] = "WAIT";
                    RQSTDT1.Rows.Add(dr1);

                    DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT1);
                    if (Prod_Result.Rows.Count == 0)
                    {
                        Util.Alert("SFU4950", sLotid);   //재공 정보가 없습니다.
                        return bRet;
                    }

                    for (int i = 0; i < Prod_Result.Rows.Count; i++)
                    {
                        if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                        {
                            Util.Alert("SFU2888", sLotid);   //HOLD 된 LOT ID 입니다.
                            return bRet;
                        }
                    }

                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dt.Columns.Add("COM_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                    dr["COM_CODE"] = "QMS_NOCHECK_PACKING";
                    dt.Rows.Add(dr);

                    DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);
                    if (AreaResult.Rows.Count == 0)
                    {
                        DataTable dtChk = new DataTable();
                        dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                        DataRow drChk = dtChk.NewRow();
                        drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();
                        dtChk.Rows.Add(drChk);

                        DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);

                        if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                        {
                            // 품질결과 검사 체크
                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotid;
                            row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);
                            loadingIndicator.Visibility = Visibility.Visible;

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", indataSet);

                            Search_Pancake(sLotid, true);

                            txtLotID.SelectAll();
                            txtLotID.Focus();

                        }
                        else
                        {
                            // 품질결과 Skip
                            Search_Pancake(sLotid, true);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else
                    {
                        Search_QMS_Validation(sLotid);
                        // 품질결과 Skip
                        Search_Pancake(sLotid, true);

                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                }

                bRet = true;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void txtCARRIERID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearhCarrierID(txtCARRIERID.Text);

                txtCARRIERID.Focus();
                txtCARRIERID.SelectAll();
            }
        }

        private void SearhCarrierID(string sCarrierID)
        {
            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return;
                }
                else
                {
                    SearchLotID(dtLot.Rows[0]["LOTID"].ToString());
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void SearchLotID(string sLotID)
        {
            try
            {
                //string sLotid = string.Empty;
                string sCstID = string.Empty;
                string sCstID2 = string.Empty;

                try
                {
                    // 출고 이력 조회
                    DataTable RQSTDT0 = new DataTable();
                    RQSTDT0.TableName = "RQSTDT";
                    RQSTDT0.Columns.Add("CSTID", typeof(String));
                    RQSTDT0.Columns.Add("AREAID", typeof(String));

                    DataRow dr0 = RQSTDT0.NewRow();
                    dr0["CSTID"] = sLotID;
                    dr0["AREAID"] = LoginInfo.CFG_AREA_ID;
                    RQSTDT0.Rows.Add(dr0);

                    DataTable OutResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ISS_STAT", "RQSTDT", "RSLTDT", RQSTDT0);

                    int iCnt = Convert.ToInt32(OutResult.Rows[0]["CNT"].ToString());
                    if (iCnt <= 0)
                    {
                        Util.MessageValidation("SFU3017"); //출고 대상이 없습니다.
                        return;
                    }

                    // 슬리터 공정 실적 확인 여부 확인 및 Grid Data 조회
                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("LOTID", typeof(String));
                    RQSTDT1.Columns.Add("PROCID", typeof(String));
                    RQSTDT1.Columns.Add("WIPSTAT", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LOTID"] = sLotID;
                    dr1["PROCID"] = "E7000";
                    dr1["WIPSTAT"] = "WAIT";
                    RQSTDT1.Rows.Add(dr1);

                    DataTable Prod_Result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_STAT_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT1);
                    if (Prod_Result.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");   //재공 정보가 없습니다.
                        return;
                    }

                    for (int i = 0; i < Prod_Result.Rows.Count; i++)
                    {
                        if (Prod_Result.Rows[i]["WIPHOLD"].ToString().Equals("Y"))
                        {
                            Util.Alert("SFU1340");   //HOLD 된 LOT ID 입니다.
                            return;
                        }
                    }
                    #region # 시생산 Lot 
                    DataRow[] dRow = Prod_Result.Select("LOTTYPE = 'X'");
                    if (dRow.Length > 0)
                    {
                        Util.Alert("SFU8146"); //시생산LOT이 포함되어 있습니다
                        return;
                    }
                    #endregion
                    DataTable dt = new DataTable();
                    dt.Columns.Add("AREAID", typeof(string));
                    dt.Columns.Add("COM_TYPE_CODE", typeof(string));
                    dt.Columns.Add("COM_CODE", typeof(string));

                    DataRow dr = dt.NewRow();
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["COM_TYPE_CODE"] = "COM_TYPE_CODE";
                    dr["COM_CODE"] = "QMS_NOCHECK_PACKING";
                    dt.Rows.Add(dr);

                    DataTable AreaResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dt);
                    if (AreaResult.Rows.Count == 0)
                    {
                        DataTable dtChk = new DataTable();
                        dtChk.Columns.Add("SHIPTO_ID", typeof(string));

                        DataRow drChk = dtChk.NewRow();
                        drChk["SHIPTO_ID"] = cboTransLoc2.SelectedValue.ToString();
                        dtChk.Rows.Add(drChk);

                        DataTable Chk_Result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHIPTO", "RQSTDT", "RSLTDT", dtChk);

                        if (Chk_Result.Rows[0]["ELTR_OQC_INSP_CHK_FLAG"].ToString() == "Y")
                        {
                            // 품질결과 검사 체크
                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("LOTID", typeof(string));
                            inData.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                            inData.Columns.Add("BR_TYPE", typeof(string));

                            DataRow row = inData.NewRow();
                            row["LOTID"] = sLotID;
                            row["BLOCK_TYPE_CODE"] = "SHIP_PRODUCT";    //NEW HOLD Type 변수
                            row["BR_TYPE"] = "P_PACKING";               //OLD BR Search 변수

                            indataSet.Tables["INDATA"].Rows.Add(row);
                            loadingIndicator.Visibility = Visibility.Visible;

                            //BR_PRD_CHK_QMS_FOR_PACKING -> BR_PRD_CHK_QMS_FOR_PACKING_NEW 변경
                            //신규 HOLD 적용을 위해 변경 작업
                            new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_QMS_FOR_PACKING_NEW", "INDATA", "OUTDATA", (result, Exception) =>
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (Exception != null)
                                {
                                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception.Message), null, "Information", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                                    Util.MessageException(Exception);
                                    return;
                                }

                                Search_Pancake(sLotID);

                                txtLotID.SelectAll();
                                txtLotID.Focus();

                            }, indataSet);
                        }
                        else
                        {
                            // 품질결과 Skip
                            Search_Pancake(sLotID);

                            txtLotID.SelectAll();
                            txtLotID.Focus();
                        }
                    }
                    else
                    {
                        Search_QMS_Validation(sLotID);
                        // 품질결과 Skip
                        Search_Pancake(sLotID);

                        txtLotID.SelectAll();
                        txtLotID.Focus();
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return;
            }
        }
    }
}

