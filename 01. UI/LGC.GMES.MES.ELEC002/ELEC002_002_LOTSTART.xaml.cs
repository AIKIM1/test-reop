using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC002
{
    public partial class ELEC002_002_LOTSTART : C1Window, IWorkArea
    {
        #region Initialize
        private static string sEqptID = string.Empty;
        private static string sElectrode = string.Empty;
        private static string sWOID = string.Empty;
        private static string sLargeLot = string.Empty;
        private static string sWO_DETL_ID = string.Empty;
        private string sEqsgID = string.Empty;
        private string PRDT_CLSS_CODE = string.Empty;
        private string sLotID = string.Empty;
        private string sSlittingFlag = string.Empty;
        string coatSide;
        private int RowIndex = 0;
        DataSet inDataSet = null;
        DataSet _MaterialDataSet = null;
        Util _Util = new Util();

        // --- 2019-07-18 김승재 
        private string _UNLDR_LOT_IDENT_BAS_CODE = string.Empty; // RF_ID 배출부
        // ---

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public bool? IsSingleCoater { get; set; }

        Dictionary<string, string> dicParam = new Dictionary<string, string>();

        public ELEC002_002_LOTSTART()
        {
            ApplyPermissions();
            InitializeComponent();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetHalfSlitterProcess();    // H/S 지정 BIZ

            // --- 2019-07-18 김승재
            GetLotIdentBasCode();
            // ---

            // CSR : C20220614-000596 , 코터 설비의 GMES UI 상 수당 자랏 생성 기능 수정 요청건 (기존내역 주석후 신규 추가)
            //if (!string.Equals(sSlittingFlag, "Y"))
            //{
            //    if (!GetCurrentLot())
            //    {
            //        this.DialogResult = MessageBoxResult.Cancel;
            //        return;
            //    }
            //}

            if (!string.Equals(sSlittingFlag, "Y"))
            {
                if (IsAreaCommonCodeUse("ELEC_CHK_LARGE_LOT_FOR_PROC_LOT", Process.COATING))
                {
                    if (!GetCurrentLot_RT())
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        return;
                    }
                }
                else
                {
                    if (!GetCurrentLot())
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        return;
                    }
                }
            }


            if (!GetWorkorder())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            SetElectrode();
            GetCurrentMount();
            txtLotID.Text = sLargeLot;

            // UI에서 SLURRY 미장착시 작업시작 못하게 막는 기능 요청으로 추가 ( 2017-02-11 ) => SRS SLURRY 도 동일 적용
            bool isInputSlurry = true;
            for (int i = 0; i < dgSlurry.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSlurry.Rows[i].DataItem, "INPUT_LOTID"))))
                {
                    isInputSlurry = false;
                    break;
                }
            }

            if (dgSlurry.GetRowCount() == 0 || isInputSlurry == false)
            {
                Util.MessageValidation("SFU3198");  //해당 설비에 Slurry Batch가 지정되지 않아 작업시작을 할 수 없습니다.
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }


            // CNA 코터 + H/S 설비는 SIDE TYPE을 선택하게 해줘야 하여 현재와 같이 생성 [2017-07-04]
            if (!string.IsNullOrEmpty(sLargeLot) && string.Equals(sSlittingFlag, "Y"))
            {
                lblSideType.Visibility = Visibility.Visible;
                cboSideType.Visibility = Visibility.Visible;
            }

            // --- 2019-07-18 김승재 H/S 여부, 대랏 생성 여부, 배출부 RF_ID 체크
            lblRWCARRIERID.Visibility = Visibility.Collapsed;
            txtRWCARRIERID.Visibility = Visibility.Collapsed;

            lblRWCARRIERID_L.Visibility = Visibility.Collapsed;
            txtRWCARRIERID_L.Visibility = Visibility.Collapsed;
            lblRWCARRIERID_R.Visibility = Visibility.Collapsed;
            txtRWCARRIERID_R.Visibility = Visibility.Collapsed;

            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (sSlittingFlag.Equals("Y"))
                {
                    // 1) RFID(O) + Slitting(O) + 대랏생성(O)
                    if (string.IsNullOrEmpty(sLargeLot))
                    {
                        lblRWCARRIERID_L.Visibility = Visibility.Visible;
                        txtRWCARRIERID_L.Visibility = Visibility.Visible;
                        lblRWCARRIERID_R.Visibility = Visibility.Visible;
                        txtRWCARRIERID_R.Visibility = Visibility.Visible;
                    }
                    // 2) RFID(O) + Slitting(O) + 대랏생성(X)
                    else
                    {
                        lblRWCARRIERID.Visibility = Visibility.Visible;
                        txtRWCARRIERID.Visibility = Visibility.Visible;
                    }
                }
                // 3) Slitting(X)
                else
                {
                    lblRWCARRIERID.Visibility = Visibility.Visible;
                    txtRWCARRIERID.Visibility = Visibility.Visible;
                }
            }
            // ---

            if (string.IsNullOrEmpty(sLargeLot))
                chkFirstFlag.IsChecked = true;
        }



        public ELEC002_002_LOTSTART(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("EQPTID")) sEqptID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) sEqsgID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("LARGELOT")) sLargeLot = dicParam["LARGELOT"];
                if (dicParam.ContainsKey("WODETIL")) sWO_DETL_ID = dicParam["WODETIL"];
                if (dicParam.ContainsKey("COATSIDE")) coatSide = dicParam["COATSIDE"];
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidData())
                return;
            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LotStart();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private bool GetWorkorder()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);
                //설비별 workorder 
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요
                    rslt = false;
                }
                else
                {
                    txtWorkOrder.Text = Util.NVC(dtMain.Rows[0]["WO_DETL_ID"]);
                    txtWOID.Text = Util.NVC(dtMain.Rows[0]["WOID"]);
                    txtPRODID.Text = Util.NVC(dtMain.Rows[0]["PRODID"]);
                    txtMODELID.Text = Util.NVC(dtMain.Rows[0]["MODLID"]);
                    rslt = true;
                }

                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetElectrode()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                    sElectrode = dtMain.Rows[0]["PRDT_CLSS_CODE"].ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private bool GetCurrentLot()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_PROC_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    Util.MessageValidation("SFU3199", new object[] { Util.NVC(dtMain.Rows[0]["LOTID"]) }); //진행중인 LOT이 있습니다.\r\nLOT ID : {%1}
                    rslt = false;
                }
                else
                {
                    rslt = true;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsHalfSlittingValid()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("SIDETYPE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["SIDETYPE"] = Util.NVC(cboSideType.Text);
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_PROC_LOT_WIDE", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.MessageValidation("SFU3694", Util.NVC(dtMain.Rows[0]["LOTID"]), Util.NVC(dtMain.Rows[0]["HALF_SLIT_SIDE"]));   //Lot[%1]이 해당 Side[%2]로 작업 진행중 입니다.
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SetHalfSlitterProcess()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_SLIT_INFO", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                    sSlittingFlag = Util.NVC(dtMain.Rows[0]["HALF_SLIT_FLAG"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetLotIdentBasCode()
        {
            try
            {
                bool rslt = false;

                _UNLDR_LOT_IDENT_BAS_CODE = "";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["PROCID"] = Process.COATING;
                dtRow["EQSGID"] = sEqsgID;
                inTable.Rows.Add(dtRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", inTable);

                if (dtMain.Rows.Count > 0)
                {
                    _UNLDR_LOT_IDENT_BAS_CODE = Util.NVC(dtMain.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"]);
                    if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        rslt = true;
                    }
                    else
                    {
                        rslt = false;
                    }
                }

                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetRWCarrierID()
        {
            // 캐리어 앙디ㅣ 가져오기 -> getprodecutlot() 참고
        }

        private void GetCurrentMount()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                if (!string.IsNullOrEmpty(coatSide))
                    IndataTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = sEqptID;

                if (!string.IsNullOrEmpty(coatSide))
                    Indata["COATING_SIDE_TYPE_CODE"] = null;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL_CT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    DataTable Foildt = dtMain.Clone();

                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE <> 'ASL' AND MTRL_CLSS_CODE = 'MFL'"))
                        Foildt.ImportRow(_iRow);

                    dgFoil.ItemsSource = DataTableConverter.Convert(Foildt);

                    DataTable Slurrydt = dtMain.Clone();

                    foreach (DataRow _iRow in dtMain.Select("PRDT_CLSS_CODE = 'ASL'"))
                        Slurrydt.ImportRow(_iRow);

                    dgSlurry.ItemsSource = DataTableConverter.Convert(Slurrydt);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidData()
        {
            if (txtWOID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1443");  //작업지시를 선택하세요.
                return false;
            }

            if (txtLotID.Text.Trim().Equals("") && !Convert.ToBoolean(chkFirstFlag.IsChecked))
            {
                //Modify : 2016.12.10  단면코터 Back 작업일때 제외 
                Util.MessageValidation("SFU1487");  //대LOT 생성 체크 후 진행하십시오.
                return false;
            }

            // CSR : C20220614-000596 , 코터 설비의 GMES UI 상 수당 자랏 생성 기능 수정 요청건(기존내역 주석후 신규 추가)
            //if (string.Equals(sSlittingFlag, "Y"))
            //    if (IsHalfSlittingValid() == false)
            //        return false;
            if (string.Equals(sSlittingFlag, "Y"))
            {
                if (IsAreaCommonCodeUse("ELEC_CHK_LARGE_LOT_FOR_PROC_LOT", Process.COATING))
                {
                    if (IsHalfSlittingValid_RT() == false)
                        return false;
                }
                else
                {
                    if (IsHalfSlittingValid() == false)
                        return false;
                }
            }


            // 확정 후 재적용
            if (Convert.ToBoolean(chkFirstFlag.IsChecked))
            {
                if (!txtLotID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1491");  //대LOT이 선택이 되어 있습니다. 선택 해제 후 진행하십시오.
                    return false;
                }
            }
            else
            {
                if (IsSingleCoater == true)
                {
                    if (coatSide.Equals("B"))
                    {
                        DataTable dt = DataTableConverter.Convert(dgFoil.ItemsSource);
                        string sInputLot = string.Empty;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (string.IsNullOrEmpty(Util.NVC(dt.Rows[i]["INPUT_LOTID"])) && !string.Equals(dt.Rows[i]["INPUT_STATE_CODE"], "A"))
                                continue;

                            sInputLot = Util.NVC(dt.Rows[i]["INPUT_LOTID"]);
                        }


                        if (string.IsNullOrEmpty(sInputLot))
                        {
                            Util.MessageValidation("SFU1966");  //투입Lot(Top 완료Lot)을 선택하십시오.
                            return false;
                        }
                    }
                }
                else
                {
                    if (txtLotID.Text.Trim() == "")
                    {
                        Util.MessageValidation("SFU1836");  //작업대상 대LOT을 선택 후 시작하십시오.
                        return false;
                    }
                }
            }

            // 신규 체크 RULE [2017-07-04]
            if (cboSideType.Visibility == Visibility.Visible && string.IsNullOrEmpty(Util.NVC(cboSideType.Text)))
            {
                Util.MessageValidation("SFU3622");  //H/S설비에서는 H/S Side를 선택 후 진행바랍니다. 
                return false;
            }

            // CSTID 체크 RULE [2019-05-29]
            if (txtRWCARRIERID.Visibility == Visibility.Visible && string.IsNullOrEmpty(Util.NVC(txtRWCARRIERID.Text)) ||
                txtRWCARRIERID_L.Visibility == Visibility.Visible && string.IsNullOrEmpty(Util.NVC(txtRWCARRIERID_L.Text)) ||
                txtRWCARRIERID_R.Visibility == Visibility.Visible && string.IsNullOrEmpty(Util.NVC(txtRWCARRIERID_R.Text)))
            {
                Util.MessageValidation("SFU7006"); // CarrierID를 입력하십시오.
                return false;
            }

            return true;
        }

        private bool CheckSlurryID(TextBox tBox)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQPTID"] = sEqptID;
                Indata["LOTID"] = tBox.Text;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_LOT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1910"); //존재하지 않는 Slurry(Batch) 입니다.
                    tBox.Focus();
                    return false;
                }
                else
                {
                    string sResult = dtMain.Rows[0]["RESULT"].ToString();

                    if (sResult == "OK")
                    {
                        return true;
                    }
                    else
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sResult), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        private void LotStart()
        {

            inDataSet = new DataSet();
            _MaterialDataSet = new DataSet();
            #region Lot Info
            DataTable inLotDataTable = inDataSet.Tables.Add("INDATA");
            inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
            inLotDataTable.Columns.Add("IFMODE", typeof(string));
            //inLotDataTable.Columns.Add("LANGID", typeof(string));
            inLotDataTable.Columns.Add("EQPTID", typeof(string));
            inLotDataTable.Columns.Add("USERID", typeof(string));
            inLotDataTable.Columns.Add("FIRST_FLAG", typeof(string));
            inLotDataTable.Columns.Add("LOTID", typeof(string));  // 대LOT 정보
            inLotDataTable.Columns.Add("COAT_SIDE_TYPE", typeof(string)); // 단면코터 코팅면
            inLotDataTable.Columns.Add("HALF_SLIT_SIDE", typeof(string)); // HALF_SLIT 사이드

            DataRow inLotDataRow = null;

            inLotDataRow = inLotDataTable.NewRow();
            inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //inLotDataRow["LANGID"] = LoginInfo.LANGID;
            inLotDataRow["EQPTID"] = sEqptID;
            inLotDataRow["USERID"] = LoginInfo.USERID;
            inLotDataRow["FIRST_FLAG"] = chkFirstFlag.IsChecked == true ? "Y" : "N";    // Y: 대LOT 발번, N: CUT Start
            inLotDataRow["LOTID"] = chkFirstFlag.IsChecked == true ? null : sLargeLot;  // 대LOT 정보
            inLotDataRow["COAT_SIDE_TYPE"] = coatSide; //단면코터 코팅면

            if (cboSideType.Visibility == Visibility.Visible)
                inLotDataRow["HALF_SLIT_SIDE"] = Util.NVC(cboSideType.Text);

            inLotDataTable.Rows.Add(inLotDataRow);
            #endregion

            #region Material
            DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
            InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inMtrlDataRow = null;

            DataTable dtFoil = DataTableConverter.Convert(dgFoil.ItemsSource);

            int iIdx = 0;
            foreach (DataRow _iRow in dtFoil.Rows)
            {
                if (!string.IsNullOrEmpty(Util.NVC(_iRow["INPUT_LOTID"])))
                {
                    inMtrlDataRow = InMtrldataTable.NewRow();
                    inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = _iRow["EQPT_MOUNT_PSTN_ID"];
                    inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = _iRow["INPUT_STATE_CODE"];
                    inMtrlDataRow["INPUT_LOTID"] = _iRow["INPUT_LOTID"];
                    InMtrldataTable.Rows.Add(inMtrlDataRow);
                    iIdx++;
                }
            }

            DataTable dtSlurry = DataTableConverter.Convert(dgSlurry.ItemsSource);

            foreach (DataRow _iRow in dtSlurry.Rows)
            {
                if (!string.IsNullOrEmpty(Util.NVC(_iRow["INPUT_LOTID"])))
                {
                    inMtrlDataRow = InMtrldataTable.NewRow();
                    inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = _iRow["EQPT_MOUNT_PSTN_ID"];
                    inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = _iRow["INPUT_STATE_CODE"];
                    inMtrlDataRow["INPUT_LOTID"] = _iRow["INPUT_LOTID"];
                    InMtrldataTable.Rows.Add(inMtrlDataRow);
                }
            }
            #endregion

            #region CST

            DataTable InCSTdataTable = inDataSet.Tables.Add("IN_OUTPUT");
            InCSTdataTable.Columns.Add("CSTID", typeof(string));

            DataRow inCSTDataRow = null;


            if (txtRWCARRIERID.Visibility == Visibility.Visible)
            {
                inCSTDataRow = InCSTdataTable.NewRow();
                inCSTDataRow["CSTID"] = Util.NVC(txtRWCARRIERID.Text);
                InCSTdataTable.Rows.Add(inCSTDataRow);
            }

            if (txtRWCARRIERID_L.Visibility == Visibility.Visible && txtRWCARRIERID_R.Visibility == Visibility.Visible)
            {
                inCSTDataRow = InCSTdataTable.NewRow();
                inCSTDataRow["CSTID"] = Util.NVC(txtRWCARRIERID_L.Text);
                InCSTdataTable.Rows.Add(inCSTDataRow);
                inCSTDataRow = InCSTdataTable.NewRow();
                inCSTDataRow["CSTID"] = Util.NVC(txtRWCARRIERID_R.Text);
                InCSTdataTable.Rows.Add(inCSTDataRow);
            }
            #endregion

            try
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

                new ClientProxy().ExecuteServiceSync_Multi(IsSingleCoater == true ? "BR_PRD_REG_CREATE_START_LOT_CT_SINGLE" : "BR_PRD_REG_CREATE_START_LOT_CT", "INDATA,IN_INPUT,IN_OUTPUT", null, inDataSet);
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                //정상처리되었습니다.
                Util.MessageInfo("SFU1275", (result) =>
                {
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                });
            }
            catch (Exception ex)
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
                //LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("SFU1838"), ex.Message, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);  //작업시작 정보 확인
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_ELEC_SLURRY _Slurry = new CMM001.Popup.CMM_ELEC_SLURRY();  // CMM으로 이동
            _Slurry.FrameOperation = FrameOperation;
            RowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

            if (_Slurry != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = Process.COATING;
                Parameters[1] = sEqsgID;
                Parameters[2] = Util.NVC(txtWorkOrder.Text);

                // Parameter에 EQPTID 추가 ( 2017-01-06 ) => TOP/BACK 동시적용
                Parameters[3] = sEqptID;

                C1WindowExtension.SetParameters(_Slurry, Parameters);

                _Slurry.Closed += new EventHandler(Slurry_Closed);
                _Slurry.ShowModal();
                _Slurry.CenterOnScreen();
            }
        }

        private void Slurry_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_ELEC_SLURRY _Slurry = sender as CMM001.Popup.CMM_ELEC_SLURRY;

            if (_Slurry.DialogResult == MessageBoxResult.OK)
            {
                // Top/Back Slurry 동일하게 적용 : 2016.12.06  ***************************************************
                DataTable dt = DataTableConverter.Convert(dgSlurry.ItemsSource);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(dgSlurry.Rows[i].DataItem, "INPUT_LOTID", _Slurry._ReturnLotID);
                }
                //***********************************************************************************************
                PRDT_CLSS_CODE = _Slurry._ReturnCLSSCODE;
            }
        }

        private void OnLoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            Button btn = e.Cell.Presenter.Content as Button;
            DataTable dt = DataTableConverter.Convert(dgFoil.ItemsSource);

            if (btn != null && e.Cell.Row.Index == 1)
            {
                btn.Visibility = Visibility.Collapsed;
            }
        }

        private void chkFirstFlag_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.IsChecked == false && string.Equals(sSlittingFlag, "Y"))
            {
                lblSideType.Visibility = Visibility.Visible;
                cboSideType.Visibility = Visibility.Visible;
            }
            else
            {
                lblSideType.Visibility = Visibility.Collapsed;
                cboSideType.Visibility = Visibility.Collapsed;
            }

            // --- 김승재 2019-07-18
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (sSlittingFlag.Equals("Y")) // 양극
                {
                    if (checkBox != null && checkBox.IsChecked == false)
                    {
                        lblRWCARRIERID.Visibility = Visibility.Visible;
                        txtRWCARRIERID.Visibility = Visibility.Visible;

                        lblRWCARRIERID_L.Visibility = Visibility.Collapsed;
                        txtRWCARRIERID_L.Visibility = Visibility.Collapsed;
                        lblRWCARRIERID_R.Visibility = Visibility.Collapsed;
                        txtRWCARRIERID_R.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        lblRWCARRIERID.Visibility = Visibility.Collapsed;
                        txtRWCARRIERID.Visibility = Visibility.Collapsed;

                        lblRWCARRIERID_L.Visibility = Visibility.Visible;
                        txtRWCARRIERID_L.Visibility = Visibility.Visible;
                        lblRWCARRIERID_R.Visibility = Visibility.Visible;
                        txtRWCARRIERID_R.Visibility = Visibility.Visible;
                    }
                }
                else // 음극
                {
                    lblRWCARRIERID_L.Visibility = Visibility.Collapsed;
                    txtRWCARRIERID_L.Visibility = Visibility.Collapsed;
                    lblRWCARRIERID_R.Visibility = Visibility.Collapsed;
                    txtRWCARRIERID_R.Visibility = Visibility.Collapsed;

                    lblRWCARRIERID.Visibility = Visibility.Visible;
                    txtRWCARRIERID.Visibility = Visibility.Visible;
                }
            }
            // --- 
        }

        // CSR : C20220614-000596 , 코터 설비의 GMES UI 상 수당 자랏 생성 기능 수정 요청건
        private bool GetCurrentLot_RT()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID_RT", typeof(string));
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID_RT"] = sLargeLot;
                Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                Indata["PROCID"] = Process.COATING;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_PR_PROC_LOT_CT", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0 && !string.IsNullOrEmpty(dtMain.Rows[0]["LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU3199", new object[] { Util.NVC(dtMain.Rows[0]["LOTID"]) }); //진행중인 LOT이 있습니다.\r\nLOT ID : {%1}
                    rslt = false;
                }
                else
                {
                    rslt = true;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // CSR : C20220614-000596 , 코터 설비의 GMES UI 상 수당 자랏 생성 기능 수정 요청건
        private bool IsHalfSlittingValid_RT()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID_RT", typeof(string));
                IndataTable.Columns.Add("SRCTYPE", typeof(string));
                IndataTable.Columns.Add("SIDETYPE", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID_RT"] = sLargeLot;
                Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                Indata["SIDETYPE"] = Util.NVC(cboSideType.Text);
                Indata["PROCID"] = Process.COATING;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTID_PR_PROC_LOT_CT_WIDE", "INDATA", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0 && !string.IsNullOrEmpty(dtMain.Rows[0]["LOTID"].ToString()))
                {
                    Util.MessageValidation("SFU3694", Util.NVC(dtMain.Rows[0]["LOTID"]), Util.NVC(dtMain.Rows[0]["HALF_SLIT_SIDE"]));   //Lot[%1]이 해당 Side[%2]로 작업 진행중 입니다.
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        public bool IsAreaCommonCodeUse(string sComeCodeType, string sComeCode)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                dr["COM_CODE"] = sComeCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (string.Equals("Y", row["ATTR1"])) 
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }
    }
}