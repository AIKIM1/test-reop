/*************************************************************************************
 Created Date : 2020.09.16
      Creator : 서동현
   Decription : 포장 Pallet 구성 (개별 Cell/Carrier) - Pallet 실적확정 PopUp
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.16  서동현 책임 : Initial Created.

**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_251_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo combo = new CommonCombo();

        // 데이터 넘겨받기 위해서 필요함 : Tray / Magazine
        private static DataTable dtBOX = new DataTable();
        private static DataTable dtCELL = new DataTable();
        private static DataTable dtASSYLOT = new DataTable();

        string sSHOPID = string.Empty;
        string sAREAID = string.Empty;
        string sLINEID = string.Empty;
        string sPRODID = string.Empty;
        string sWOID = string.Empty;
        string sWO_DETL_ID = string.Empty;
        string sUSERID = string.Empty;

        private bool bTop_ProdID_Use_Flag = false;

        // 팝업 호출한 폼으로 리턴함.
        private DataTable _RetDT = null;
        public DataTable retDT
        {
            get { return _RetDT; }
        }

        string sBOX_CELL_QTY = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_251_CONFIRM()
        {
            InitializeComponent();
            Loaded += BOX001_251_CONFIRM_Loaded;
        }

        private void BOX001_251_CONFIRM_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_251_CONFIRM_Loaded;
            InitSet();
        }
        #endregion

        #region Initialize
        private void InitSet()
        {
            //object[] Parameters = new object[9];
            //Parameters[0] = Util.NVC(cboModel.SelectedValue);
            //Parameters[1] = Util.NVC(cboEquipmentSegment.SelectedValue);
            //Parameters[2] = Util.NVC(cboEquipmentSegment.Text);
            //Parameters[3] = txtProdID.Text.Trim();
            //Parameters[4] = txtBoxCellQty.Value.ToString();
            //Parameters[5] = totalqty;
            //Parameters[6] = lsDataTable;
            //Parameters[7] = cellCheck == true ? "SKIP" : "NO SKIP";
            //Parameters[8] = Util.NVC(cboPackOut_Go.SelectedValue);
            //Parameters[9] = txtWO.Text.Trim();
            //Parameters[10] = sWO_DETL_ID;
            //Parameters[11] = sSHOPID;
            //Parameters[12] = sAREAID;

            object[] tmps = C1WindowExtension.GetParameters(this);
            txtLotid.Text = string.Empty;
            dtCELL = tmps[6] as DataTable;
            sWOID = tmps[9] as string;
            sWO_DETL_ID = tmps[10] as string;

            sSHOPID = tmps[11] as string;
            sAREAID = tmps[12] as string;
            txtModel.Text = tmps[0] as string;
            sBOX_CELL_QTY = tmps[4] as string;
            txtOutputqty.Text = tmps[5] as string;  // totalqty
            txtProdID.Text = tmps[3] as string;
            sPRODID = tmps[3] as string;
            txtLine.Text = tmps[2] as string;
            sLINEID = tmps[1] as string;
            txtSkip.Text = tmps[7] as string;
            dtShipDate.Text = DateTime.Now.ToShortDateString();
            dtUserDate.Text = DateTime.Now.ToShortDateString();
            dtUserTime.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
            //txtUser.Text = LoginInfo.USERNAME;
            sUSERID = tmps[13] as string;
            txtProcUser.Text = tmps[13] as string;
            txtProcUser.Tag = tmps[14] as string;
            txtCstid.Text = tmps[16] as string;     // Carrier ID

            //출하처 Combo Set.
            string[] sFilter3 = { sSHOPID, sLINEID, sAREAID };
            combo.SetCombo(cboComp, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
            cboComp.SelectedValue = tmps[8] as string;
            cboComp.IsEnabled = false;

            combo.SetCombo(cboExpDom, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            setExpDom();

            txtConbineseq.AllowNull = true;
            txtShipqty.AllowNull = true;
            txtConbineseq.Value = double.NaN;
            txtShipqty.Value = double.NaN;
            //작업자 Combo Set.
            //String[] sFilter2 = { sSHOPID, sAREAID, Process.CELL_BOXING };
            //combo.SetCombo(cboProcUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "PROC_USER");

            //cboProcUser.SelectedValue = sUSERID;
            //txtProcUser.Text = LoginInfo.USERNAME;

            //공통코드에 등록된 Shop은 UNCODE 입력 TextBox Visible 처리
            if (UseCommoncodePlant())
            {
                txtUnCode.Visibility = Visibility.Visible;
                tbUnCode.Visibility = Visibility.Visible;
            }

            ChkUseTopProdID();
        }
        #endregion

        #region Event
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            #region # 필수 입력값 확인하는 부분
            if (Util.NVC(cboComp.SelectedValue) == "" || Util.NVC(cboComp.SelectedValue) == "SELECT")
            {
                // 출하처를 선택해주세요. >> 출하처 아이디를 선택해 주세요.
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0062"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("MMD0062");
                return;
            }
            else if (double.IsNaN(txtShipqty.Value) || txtShipqty.Value == 0)
            {
                //출하 수량을 입력해주세요.
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3174"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("SFU3174");
                txtShipqty.Focus();
                return;
            }
            else if (double.IsNaN(txtConbineseq.Value) || txtConbineseq.Value == 0)
            {
                //구성 차수 관리 번호를 입력해주세요.
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3146"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                Util.MessageValidation("SFU3146");
                txtConbineseq.Focus();
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
            else if (string.IsNullOrWhiteSpace(txtCstid.Text))
            {
                //%1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", txtCstName.Text);
                return;
            }

            //string sUser = Util.NVC(cboProcUser.Text);

            //if (sUser == "" || sUser == "SELECT")
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업자를 선택해주세요."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
            //    return;
            //}
            #endregion

            //#region 입력값 확인 후 수행되는 로직
            //DataTable resultAssyLotIDs = new DataTable();

            // 매거진 정보들을 가지고, 관련 실적을 생성하는 함수 호출
            //string palletID = InsertCELLTOPALLETData(DateTime.Now, Convert.ToInt32(txtOutputqty.Text.Trim()), sUser);
            string palletID = InsertCELLTOPALLETData();

            // 리턴 받은 ID가 널이라면 함수 종료
            if (palletID == null || palletID == "")
            {
                return;
            }

            // 조립LotID 구하는 함수 호출 : 결과값을 DataTable 로 리턴
            dtASSYLOT = SearchAssyLot(palletID);
            txtLotid.Text = palletID;

            #region 완제품 ID 존재 여부 체크
            // TOP_PRODID 조회.
            string sTopProdID = "";

            if (bTop_ProdID_Use_Flag)
            {
                sTopProdID = GetTopProdID(palletID);

                if (sTopProdID.Equals(""))
                {
                    // [%1]에 완제품 정보(TOP_PRODID)가 없습니다.
                    Util.MessageValidation("SFU5208", palletID);
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                    return;
                }
            }
            #endregion

            //Pallet Tag 정보Set
            //_RetDT = setPalletTag(cboProcUser.Text);
            _RetDT = setPalletTag(sTopProdID);

            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Mehod
        private void setExpDom()
        {
            // BR_PRD_GET_EXP_DOM_TYPE
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
                inData["DATA_TYPE"] = "SUBLOT";
                inData["PROCID"] = Process.CELL_BOXING;
                inData["EQSGID"] = sLINEID;

                inDataTable.Rows.Add(inData);

                foreach (DataRow dr in dtCELL.Rows)
                {
                    DataRow inLot = inLotTable.NewRow();
                    inLot["LOTID"] = dr["CELLID"];
                    inLotTable.Rows.Add(inLot);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_EXP_DOM_TYPE", "INDATA,INLOT", "OUTDATA", indataSet);
                string sValue = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["EXP_DOM_TYPE_CODE"]);
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
        /// CELL 정보들을 가지고, 관련 실적을 생성하는 함수
        /// </summary>
        /// <returns></returns>
        private string InsertCELLTOPALLETData()
        {
            // GMPACK_CELL_LOT_v02
            try
            {
                string sNote = "";
                if (dtCELL.Rows.Count > 0)
                {
                    sNote = Util.NVC(dtCELL.Rows[0]["PROD_KIND"]) + " " + txtNote.Text;      //개발품, 양산품여부는 NOTE FIELD에 저장함 2014.02.25 배포 후 DATA부터는...
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("INSPECT_SKIP", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO_ADDR", typeof(string));
                inDataTable.Columns.Add("PACKQTY", typeof(Decimal));
                inDataTable.Columns.Add("SHIPQTY", typeof(Decimal));
                inDataTable.Columns.Add("SHIPDATE", typeof(string));
                inDataTable.Columns.Add("PACKDATE", typeof(DateTime));
                inDataTable.Columns.Add("COMBINESEQ", typeof(Decimal));
                inDataTable.Columns.Add("BOXCELLQTY", typeof(Decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTUSER", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("UNCODE", typeof(string));
                inDataTable.Columns.Add("CSTID", typeof(string));

                DataTable inDataCellTable = indataSet.Tables.Add("INDATA_SUBLOT");
                inDataCellTable.Columns.Add("PACKCELLSEQ", typeof(decimal));
                inDataCellTable.Columns.Add("SUBLOTID", typeof(string));
                inDataCellTable.Columns.Add("BOXID", typeof(string));
                inDataCellTable.Columns.Add("LOTID", typeof(string));

                TimeSpan spn = ((TimeSpan)dtUserTime.Value);
                DateTime dtPackDateTime = new DateTime(dtUserDate.SelectedDateTime.Year, dtUserDate.SelectedDateTime.Month, dtUserDate.SelectedDateTime.Day,
                    spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);

                DataRow inData = inDataTable.NewRow();
                inData["EQSGID"] = sLINEID;
                inData["PROCID"] = Process.CELL_BOXING;
                inData["MODLID"] = txtModel.Text.Trim(); 
                inData["PRODID"] = sPRODID;

                //포장구성확인 해제 (포장검사 Skip 여부??)
                if (txtSkip.Text == "SKIP")
                {
                    inData["INSPECT_SKIP"] = "Y";
                }
                else
                {
                    inData["INSPECT_SKIP"] = "N";
                }

                inData["PACKOUT_GO"] = Util.NVC(cboComp.SelectedValue);
                inData["PACKOUT_GO_ADDR"] = txtOutgo.Text.Trim();
                inData["PACKQTY"] = txtOutputqty.Text;
                inData["SHIPQTY"] = txtShipqty.Value;
                inData["SHIPDATE"] = dtShipDate.SelectedDateTime.ToString("yyyyMMdd");
                inData["PACKDATE"] = dtPackDateTime;
                inData["COMBINESEQ"] = txtConbineseq.Value;
                inData["BOXCELLQTY"] = sBOX_CELL_QTY;
                inData["USERID"] = txtProcUser.Tag as string;
                inData["ACTUSER"] = txtProcUser.Text;
                inData["NOTE"] = txtNote.Text.Trim();
                inData["MKT_TYPE_CODE"] = cboExpDom.SelectedValue;
                inData["UNCODE"] = !string.IsNullOrWhiteSpace(txtUnCode.Text.Trim()) ? txtUnCode.Text.Trim() : null;
                inData["CSTID"] = txtCstid.Text.Trim();
                inDataTable.Rows.Add(inData);

                if (dtCELL != null)
                {
                    for (int lsCount = 0; lsCount < dtCELL.Rows.Count; lsCount++)
                    {
                        DataRow inDataCell = inDataCellTable.NewRow();
                        inDataCell["PACKCELLSEQ"] = Util.NVC_Decimal(dtCELL.Rows[lsCount]["PACKCELLSEQ"]);
                        inDataCell["SUBLOTID"] =  Util.NVC(dtCELL.Rows[lsCount]["CELLID"]);
                        inDataCell["BOXID"] = Util.NVC(dtCELL.Rows[lsCount]["BOXID"]);
                        inDataCell["LOTID"] = Util.NVC(dtCELL.Rows[lsCount]["LOTID"]);
                        inDataCellTable.Rows.Add(inDataCell);
                    }
                }
                else
                {
                    return null;
                }

                loadingIndicator.Visibility = Visibility.Visible;
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PACKING_BY_USER_CP_MAPPING_CSTID", "INDATA,INDATA_SUBLOT", "OUTDATA", indataSet);

                if (dsResult.Tables["OUTDATA"].Rows.Count <= 0)
                {
                    // 조회된 결과가 없을 경우 함수 종료
                    return null;
                }
                else
                {
                    return Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["BOXID"]);                 // PalletID 리턴
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 포장 출고 ID 별 조립LotID, 해당 Lot 별 수량 조회
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

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 result값에 null 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                return null;
            }
        }

        private DataTable setPalletTag(string sTopProdID)
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                //return null;
            }

            //Lot 편차 구하기... 2014.02.20 Add By Airman
            string sLotTerm = GetLotTerm2PalletID(txtLotid.Text.Trim());

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

            DataRow dr = dtPalletHisCard.NewRow();
            dr["PALLETID"] = txtLotid.Text.Trim();
            dr["BARCODE1"] = txtLotid.Text.Trim();
            dr["CONBINESEQ1"] = txtConbineseq.Value;

            //dr["SHIP_LOC"] = Util.NVC(cboComp.SelectedValue);
            dr["SHIP_LOC"] = Util.NVC(cboComp.Text);
            dr["SHIPDATE"] = dtShipDate.SelectedDateTime.ToString("yyyy-MM-dd");//.Text.Substring(0, 10);
            dr["OUTGO"] = txtOutgo.Text.Trim();
            dr["LOTTERM"] = sLotTerm;
            dr["NOTE"] = txtNote.Text.Trim();
            dr["UNCODE"] = txtUnCode.Text.Trim();

            //string sModel = sProjectName;
            //if (cboComp.Text.ToString() == "HLGP")
            //{
            //    if (sModel == "" || sModel == "N/A")
            //    {
            //        sModel = txtModel.Text.Trim();              
            //    }
            //}
            //else
            //{
            //    sModel = txtModel.Text.Trim();
            //}

            string sUserDate = dtUserDate.SelectedDateTime.ToString("yyyy-MM-dd");
            dr["PACKDATE"] = sUserDate + " " + dtUserTime.Value;

            dr["LINE"] = txtLine.Text.Trim();
            dr["MODEL"] = txtModel.Text.Trim() + " (" + sProjectName + ")";
            if (bTop_ProdID_Use_Flag)
                dr["PRODID"] = sTopProdID.Equals("") ? "" : sTopProdID;// txtProdID.Text.Trim();
            else
                dr["PRODID"] = txtProdID.Text.Trim();
            dr["SHIPQTY"] = string.Format("{0:#,###}", txtShipqty.Value.ToString());
            dr["PARTNO"] = string.Empty;
            dr["OUTQTY"] = string.Format("{0:#,###}", txtOutputqty.Text.Trim());
            dr["USERID"] = txtProcUser.Text;
            dr["CONBINESEQ2"] = txtConbineseq.Value;
            dr["SKIPYN"] = txtSkip.Text.Trim();

            for (int i = 0; i < dtBOX.Rows.Count && i <= 40; i++)
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
        #endregion
    }
}
