/*************************************************************************************
 Created Date : 2020.12.16
      Creator : 
   Decription : 포장 Pallet 구성 (자동) - Pallet 실적확정 PopUp
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.16 이제섭 : Initial Created.
  2023.03.01 이제섭 : 자동 출고 사용 동 조회하여 자동출고여부 CheckBox Visible처리, 
                      실적확정 비즈 호출 시, SHIP_YN 파라미터 추가
  2023.04.21 권혜정 : UNCODE 수량 제한 및 유효 기간 설정 추가




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_300_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        // 데이터 넘겨받기 위해서 필요함 : Tray / Magazine
        private static DataTable dtBOX = new DataTable();
        private static DataTable dtASSYLOT = new DataTable();

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sLINEID = string.Empty;
        string sPROCID = string.Empty;
        string sEQPTID = string.Empty;
        string sPRODID = string.Empty;
        string sUSERNAME = string.Empty;
        string[] sUNCODE = new string[3];
        int iTOTALQTY = 0;
        //bool bShipWait = false;

        private Util _Util = new Util();
        bool _palletBCD = false;

        // 팝업 호출한 폼으로 리턴함.
        private DataTable _RetDT = null;
        public DataTable retDT
        {
            get { return _RetDT; }
        }


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_300_CONFIRM()
        {
            InitializeComponent();
            Loaded += BOX001_300_CONFIRM_Loaded;

        }

        private void BOX001_300_CONFIRM_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_300_CONFIRM_Loaded;
            InitSet();
           
        }


        #endregion

        #region Initialize

        private void InitSet()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);
            
            txtLotid.Text = tmps[0] as string;
            dtBOX = tmps[10] as DataTable;
            dtASSYLOT = tmps[11] as DataTable;

            txtOutputqty.Text = tmps[12] as string;
            iTOTALQTY = Util.NVC_Int(txtOutputqty.Text);
            txtModel.Text = tmps[5] as string;
            txtProdID.Text = tmps[9] as string;
            sPRODID = tmps[9] as string;
            txtLine.Text = tmps[7] as string;
            sSHOPID = tmps[13] as string;
            sAREAID = tmps[14] as string;
            sLINEID = tmps[6] as string;
            sPROCID = tmps[3] as string;
            sEQPTID = tmps[8] as string;
            sUSERNAME = tmps[14] as string;
            txtProcUser.Text = tmps[15] as string;
            txtProcUser.Tag = tmps[16] as string;
            txtSkip.Text = "NO SKIP";
            dtUserDate.Text = DateTime.Now.ToShortDateString();
            dtUserTime.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            dtShipDate.Text = tmps[17].ToString();

            


            //출하처 Combo Set.
            string[] sFilter3 = { sSHOPID, sLINEID, sAREAID };
            combo.SetCombo(cboComp, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");

            combo.SetCombo(cboExpDom, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            setExpDom();

            txtConbineseq.AllowNull = true;
            txtShipqty.AllowNull = true;

            txtConbineseq.Value = double.NaN;
            txtShipqty.Value = double.NaN;

            //공통코드에 등록된 Shop은 UNCODE 입력 TextBox Visible 처리
            if(UseCommoncodePlant())
            {
                txtUnCode.Visibility = Visibility.Visible;
                tbUnCode.Visibility = Visibility.Visible;
            }

            // Pallet BCD 표시여부
            isVisibleBCD(sAREAID);

            //// 자동출고 사용동이면, 자동출고여부 체크박스 Visible 처리
            //if(UseCommoncodeArea())
            //{
            //    chkShipWait.Visibility = Visibility.Visible;
            //}
        }

        #endregion


        #region Event


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //필수 입력값 확인하는 부분
            if (Util.NVC(cboComp.SelectedValue) == "" || Util.NVC(cboComp.SelectedValue) == "SELECT")
            {
                // 출하처를 선택해주세요. >> 출하처 아이디를 선택해 주세요.
                Util.MessageValidation("MMD0062");
                return;
            }
            else if (double.IsNaN(txtShipqty.Value) || txtShipqty.Value == 0)
            {
                //출하 수량을 입력해주세요.
                Util.MessageValidation("SFU3174");
                txtShipqty.Focus();
                return;
            }
            else if (double.IsNaN(txtConbineseq.Value) || txtConbineseq.Value == 0)
            {
                //구성 차수 관리 번호를 입력해주세요.
                Util.MessageValidation("SFU3146");
                txtConbineseq.Focus();
                return;
            }
            else if (string.IsNullOrEmpty(txtProcUser.Text))
            {
                Util.MessageValidation("SFU1843");              
                return;
            }

            else if (Util.NVC(cboExpDom.SelectedValue) == "" || Util.NVC(cboExpDom.SelectedValue) == "SELECT")
            {
                Util.MessageValidation("SFU3606"); //'수출/내수'를 선택해주세요
                return;
            }    
            else if (UseCommoncodePlant() && string.IsNullOrWhiteSpace(txtUnCode.Text))
            {
                //%1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", "UNCODE");
                return;
            }
            else if (UseCommoncodePlant())
            {
                sUNCODE = GetUncodeUseDate(txtUnCode.Text);
                DateTime dtUncodeDate = DateTime.ParseExact(sUNCODE[2], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None); // 유효기간

                if (sUNCODE == null)
                {
                    // MMD에 입력되지 않은 UNCODE 입니다. 
                    Util.MessageValidation("SFU8905");
                    return;
                }
                else if (dtUncodeDate < DateTime.Now)
                {
                    // UNCODE 유효기간을 확인해주세요.
                    Util.MessageValidation("SFU8906");
                    return;
                }
                else if (Convert.ToInt32(sUNCODE[1]) <= 0)
                {
                    // UNCODE 사용 수량 초과하였습니다.
                    Util.MessageValidation("SFU8907");
                    return;
                }
            }

            // Pallet BCD 입력 체크
            if (_Util.IsCommonCodeUseAttr("FORM_PLLT_BCD_MAN_LINK_AREA", sAREAID, "Y"))
            {
                if (string.IsNullOrEmpty(txtPalletBCD.Text.Trim()))
                {
                    // [%1]을(를) 스캔하거나 입력하십시오.
                    Util.MessageValidation("SFU8896", result =>
                    {
                        txtPalletBCD.Focus();
                    }, "Pallet BCD");
                    return;
                }
                if (!getPalletBCD(txtPalletBCD.Text.Trim()))
                {
                    return;
                }
            }

            // 실적확인 처리
            Boolean endResult = ProcPackEnd();

            if (endResult == true)
            {
                //_RetDT = setPalletTag(cboProcUser.Text);
                _RetDT = setPalletTag(txtProcUser.Text);

                // UN CODE 사용 수량 및 HISTORY 생성
                if (UseCommoncodePlant())
                {
                    UpdateUncode(sUNCODE[0], sUNCODE[1], sUSERNAME, _RetDT.Rows[0]["PALLETID"].ToString());
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            else
            {
                //실적 확정 처리가 비정상 종료되었습니다.
                Util.MessageInfo("SFU3155");
                return;
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }


        #endregion

        #region Mehod
        /// <summary>
        /// 수출 / 내수 유형 조회
        /// </summary>
        private void setExpDom()
        {
            // BR_GET_EXP_DOM_TYPE_BX
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("DATA_TYPE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("LOTID", typeof(string));                 

                DataRow inData = inDataTable.NewRow();
                inData["DATA_TYPE"] = "PLT";
                inData["PROCID"] = Process.CELL_BOXING;
                inData["EQSGID"] = sLINEID;

                inDataTable.Rows.Add(inData);

                DataRow inLot = inLotTable.NewRow();
                inLot["LOTID"] = txtLotid.Text;
                inLotTable.Rows.Add(inLot);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_EXP_DOM_TYPE_BX", "INDATA,INLOT", "OUTDATA", indataSet);
                string sValue = Util.NVC( dsResult.Tables["OUTDATA"].Rows[0]["EXP_DOM_TYPE_CODE"]);
                cboExpDom.SelectedValue = sValue;
                if (string.IsNullOrWhiteSpace(cboExpDom.Text)) cboExpDom.SelectedIndex = 0;
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
        ///  실적확정
        /// </summary>
        /// <returns></returns>
        private Boolean ProcPackEnd()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO_ADDR", typeof(string));
                inDataTable.Columns.Add("SHIPDATE", typeof(string));
                inDataTable.Columns.Add("PACKDATE", typeof(DateTime));
                inDataTable.Columns.Add("COMBINESEQ", typeof(Decimal));
                inDataTable.Columns.Add("SHIP_QTY", typeof(Decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTUSER", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("UNCODE", typeof(string));
                inDataTable.Columns.Add("SHIP_YN", typeof(string));

                DataTable inLotTable = indataSet.Tables.Add("INLOT");
                inLotTable.Columns.Add("BOXID", typeof(string));
                inLotTable.Columns.Add("TOTAL_QTY", typeof(decimal));
                inLotTable.Columns.Add("NOTE", typeof(string));
                inLotTable.Columns.Add("PLLT_BCD_ID", typeof(string));

                TimeSpan spn = ((TimeSpan)dtUserTime.Value);
                DateTime dtPackDateTime = new DateTime(dtUserDate.SelectedDateTime.Year, dtUserDate.SelectedDateTime.Month, dtUserDate.SelectedDateTime.Day,
                    spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inData["EQSGID"] = sLINEID;
                inData["PROCID"] = sPROCID;
                inData["EQPTID"] = sEQPTID;
                inData["PACKOUT_GO"] = Util.NVC(cboComp.SelectedValue);
                inData["PACKOUT_GO_ADDR"] = txtOutgo.Text.Trim();
                inData["SHIPDATE"] = dtShipDate.SelectedDateTime.ToString("yyyyMMdd");
                inData["PACKDATE"] = dtPackDateTime;
                inData["COMBINESEQ"] = txtConbineseq.Value;
                inData["SHIP_QTY"] = txtShipqty.Value;
                inData["USERID"] = txtProcUser.Tag as string;
                inData["ACTUSER"] = txtProcUser.Text;
                inData["MKT_TYPE_CODE"] = cboExpDom.SelectedValue;
                inData["UNCODE"] = !string.IsNullOrWhiteSpace(txtUnCode.Text.Trim()) ? txtUnCode.Text.Trim() : null;
                //inData["SHIP_YN"] = bShipWait ? "Y" : "N";
                inData["SHIP_YN"] ="N";
                inDataTable.Rows.Add(inData);

                DataRow inLot = inLotTable.NewRow();
                inLot["BOXID"] = txtLotid.Text.Trim();
                inLot["TOTAL_QTY"] = txtOutputqty.Text.Trim();
                inLot["NOTE"] = txtNote.Text.Trim();
                inLot["PLLT_BCD_ID"] = !string.IsNullOrWhiteSpace(txtPalletBCD.Text.Trim()) ? txtPalletBCD.Text.Trim() : null; 
                inLotTable.Rows.Add(inLot);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_END_PALLET_BY_EQ_BX", "INDATA,INLOT", null, indataSet);
                return true;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        private DataTable setPalletTag(string sUserName)
        {
            string sProjectName = string.Empty;
            //고객 모델 조회
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PRODID", typeof(string));

                DataRow drMomel = RQSTDT.NewRow();
                drMomel["PRODID"] = txtProdID.Text.Trim();
                RQSTDT.Rows.Add(drMomel);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR_FOR_PROJECTNAME", "RQSTDT", "RSLTDT", RQSTDT);
                sProjectName = Util.NVC(dtResult.Rows[0]["PROJECTNAME"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


            //Lot 편차 구하기... 2014.02.20 Add By Airman
            string sLotTerm = GetLotTerm2PalletID(txtLotid.Text.Trim());
            //TAG발행시 영문화면 추가용
            DataTable dtinfo = SelectScanPalletInfo(txtLotid.Text.Trim());

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

            dtPalletHisCard.Columns.Add("SHIP_LOC_EN", typeof(string));
            dtPalletHisCard.Columns.Add("LINE_EN", typeof(string));

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

            DataRow dr = dtPalletHisCard.NewRow();
            dr["PALLETID"] = txtLotid.Text.Trim();
            dr["BARCODE1"] = txtLotid.Text.Trim();
            dr["CONBINESEQ1"] = txtConbineseq.Value;

            dr["SHIP_LOC"] = Util.NVC(cboComp.Text);
            dr["SHIPDATE"] = dtShipDate.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["OUTGO"] = txtOutgo.Text.Trim();
            dr["LOTTERM"] = sLotTerm;
            dr["NOTE"] = txtNote.Text.Trim();
            dr["UNCODE"] = txtUnCode.Text.Trim();

            string sUserDate = string.Empty;          
            sUserDate = dtUserDate.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["PACKDATE"] = sUserDate + " " + dtUserTime.Value;
            
            dr["LINE"] = txtLine.Text.Trim();
            dr["MODEL"] = txtModel.Text.Trim() + " (" + sProjectName + ")";
            dr["PRODID"] = txtProdID.Text.Trim();
            dr["SHIPQTY"] = string.Format("{0:#,###}", txtShipqty.Value.ToString());
            dr["PARTNO"] = string.Empty;
            dr["OUTQTY"] = string.Format("{0:#,###}", txtOutputqty.Text.Trim());
            dr["USERID"] = sUserName;
            dr["CONBINESEQ2"] = txtConbineseq.Value;
            dr["SKIPYN"] = txtSkip.Text.Trim();

            dr["SHIP_LOC_EN"] = Util.NVC(dtinfo.Rows[0]["SHIPTO_NAME_EN"]);
            dr["LINE_EN"] = Util.NVC(dtinfo.Rows[0]["EQSGNAME_EN"]);


            for (int i = 0; i < dtBOX.Rows.Count && i<40; i++)
            {
                dr["TRAY_" + i.ToString()] = Util.NVC(dtBOX.Rows[i]["TRAY_MAGAZINE"]);
                dr["T_" + i.ToString()] = Util.NVC(dtBOX.Rows[i]["QTY"]);
            }

            dtPalletHisCard.Rows.Add(dr);

            for (int cnt = 0; cnt < (dtASSYLOT.Rows.Count + 1) / 20; cnt++)
            {
                DataTable dtNew = dtPalletHisCard.Copy();
                dtPalletHisCard.Merge(dtNew);
            }

            for (int i = 0; i < dtASSYLOT.Rows.Count; i++)
            {
                int cnt = i / 20;
                dtPalletHisCard.Rows[cnt]["LOTID_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtASSYLOT.Rows[i]["LOTID"]);
                dtPalletHisCard.Rows[cnt]["L_" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC_Int(dtASSYLOT.Rows[i]["CELLQTY"]).ToString();
                dtPalletHisCard.Rows[cnt]["BCD" + (i < 20 ? i : i - (20 * cnt)).ToString()] = Util.NVC(dtASSYLOT.Rows[i]["LOTID"]) + " " + Util.NVC_Int(dtASSYLOT.Rows[i]["CELLQTY"]).ToString();
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
                return "0";
            }
        }

        private DataTable SelectScanPalletInfo(string palletID)
        {
            DataTable dtResult = new DataTable();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
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

        /// <summary>
        /// 자동 출고 사용 동 조회
        /// </summary>
        /// <returns></returns>
        private bool UseCommoncodeArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "AUTO_ISS_AREA";
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

        /// <summary>
        /// Pallet BCD 표시 여부
        /// </summary>
        /// <param name="sAreaID"></param>
        private void isVisibleBCD(string sAreaID)
        {
            Border _BCD1 = FindName("brdBCD_1") as Border;
            Border _BCD2 = FindName("brdBCD_2") as Border;
            Border _BCD3 = FindName("brdBCD_3") as Border;
            Border _BCD4 = FindName("brdBCD_4") as Border;

            if (_Util.IsCommonCodeUseAttr("FORM_PLLT_BCD_MAN_LINK_AREA", sAreaID))
            {
                if (_BCD1 != null)
                    _BCD1.Visibility = Visibility.Visible;
                if (_BCD2 != null)
                    _BCD2.Visibility = Visibility.Visible;
                if (_BCD3 != null)
                    _BCD3.Visibility = Visibility.Visible;
                if (_BCD4 != null)
                    _BCD4.Visibility = Visibility.Visible;
            }
            else
            {
                if (_BCD1 != null)
                    _BCD1.Visibility = Visibility.Collapsed;
                if (_BCD2 != null)
                    _BCD2.Visibility = Visibility.Collapsed;
                if (_BCD3 != null)
                    _BCD3.Visibility = Visibility.Collapsed;
                if (_BCD4 != null)
                    _BCD4.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// UNCODE MMD 입력 여부 및 유효기간 조회
        /// </summary>
        private string[] GetUncodeUseDate(string sUNCODE)
        {
            try
            {
                string[] sReturn = new string[3];

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("UN_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["UN_CODE"] = sUNCODE;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNCODE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    sReturn[0] = Util.NVC(dtResult.Rows[0]["UN_CODE"]);
                    sReturn[1] = Util.NVC(dtResult.Rows[0]["USE_QTY"]);
                    sReturn[2] = Util.NVC(dtResult.Rows[0]["VLD_PERIOD"]);
                    return sReturn;
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

        /// <summary>
        /// UNCODE 사용 수량 -1 차감
        /// </summary>
        private void UpdateUncode(string sUNCODE, string sUSEQTY, string sUSER, string sPalletID)
        {
            try
            {
                int iUseqty = Convert.ToInt32(sUSEQTY) - 1;
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("UN_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_QTY", typeof(string));
                RQSTDT.Columns.Add("PLLT_ID", typeof(string));
                RQSTDT.Columns.Add("INSUSER", typeof(string));
                RQSTDT.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["UN_CODE"] = sUNCODE;
                dr["USE_QTY"] = iUseqty.ToString();
                dr["PLLT_ID"] = sPalletID;
                dr["INSUSER"] = sUSER;
                dr["UPDUSER"] = sUSER;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_UNCODE", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletBCD_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                getPalletBCD(txtPalletBCD.Text.Trim());
            }
        }
        /// <summary>
        /// Pallet BCD 할당여부
        /// </summary>
        /// <param name="palletid"></param>
        /// <returns></returns>
        private bool getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));
                RQSTDT.Columns.Add("USERID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;
                dr["USERID"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PLLT_BCD_INF", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    if (SearchResult.Columns.Contains("CSTID") && SearchResult.Columns.Contains("RACK_ID"))
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(SearchResult.Rows[0]["RACK_ID"])))
                        {
                            // 팔레트[%1]은 현재 Location[%2]에 입고 되어 있습니다.
                            Util.MessageValidation("100000199", result =>
                            {
                                txtPalletBCD.Clear();
                                txtPalletBCD.Focus();
                            }, Util.NVC(SearchResult.Rows[0]["CSTID"]), Util.NVC(SearchResult.Rows[0]["RACK_ID"]));
                            return false;
                        }
                    }
                    //return true;
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    txtPalletBCD.Clear();
                    txtPalletBCD.Focus();
                });
                return false;
            }
            return true;
        }

        #endregion

        //private void chkShipWait_Checked(object sender, RoutedEventArgs e)
        //{
        //    bShipWait = true;

        //}

        //private void chkShipWait_UnChecked(object sender, RoutedEventArgs e)
        //{
        //    bShipWait = false;
        //}
    }
}
