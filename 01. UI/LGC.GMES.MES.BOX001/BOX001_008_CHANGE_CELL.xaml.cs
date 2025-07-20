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
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_008_CHANGE_CELL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string sAREAID = string.Empty;

        CommonCombo combo = new CommonCombo();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public BOX001_008_CHANGE_CELL()
        {
            InitializeComponent();
            Loaded += BOX001_008_CHANGE_CELL_Loaded;
        }

        private void BOX001_008_CHANGE_CELL_Loaded(object sender, RoutedEventArgs e)
        {

            Loaded -= BOX001_008_CHANGE_CELL_Loaded;

            ////작업자 Combo Set.
            //String[] sFilter2 = { LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, Process.CELL_BOXING };
            //combo.SetCombo(cboProcUser, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "PROC_USER");
        }


        #endregion

        #region Initialize

        #endregion

        #region Event


        private void chkExtraction_Click(object sender, RoutedEventArgs e)
        {
            if (chkExtraction.IsChecked == true)
            {
                txtAfterCell.Text = "";
                txt2D.Text = "";
                txtAfterCell.IsEnabled = false;
                txt2D.IsEnabled = false;
                ClearAfterCellInfo();
            }
            else
            {
                txtAfterCell.IsEnabled = true;
                txt2D.IsEnabled = true;
            }
        }


        /// <summary>
        /// 불량 Cell 입력란에 스캔 혹은 입력 시 동작하는 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBeforeCell_KeyDown(object sender, KeyEventArgs e)
        {
            // 입력한 값이 Enter 일 경우만 아래 로직 수행
            if (e.Key == Key.Enter)
            {
                // 입력한 값이 널이나 공백이 아닐 경우만 아래 로직 수행
                if (string.IsNullOrEmpty(txtBeforeCell.Text))
                {
                    return;
                }

                // 체크박스 활성화 & 체크 안 되어 있는 상태로 변경.
                chkSkip.IsChecked = false;
                chkSkip.IsEnabled = true;

                string msg = "";

                // 셀 정보 조회 함수 호출
                int result = SearchCellInfo(0, txtBeforeCell.Text, out msg);

                switch (result)
                {
                    case 1:
                        Util.MessageValidation("SFU1209");  //"셀 정보가 존재하지 않습니다" >>Cell 정보가 없습니다.

                        // 기존 정보 초기화
                        ClearBeforeCellInfo();

                        // 포커스 이동
                        txtBeforeCell.Focus();
                        txtBeforeCell.SelectAll();
                        break;

                    case 2:
                        Util.AlertInfo(msg);

                        // 기존 정보 초기화
                        ClearBeforeCellInfo();

                        // 포커스 이동
                        txtBeforeCell.Focus();
                        txtBeforeCell.SelectAll();
                        break;

                    default:

                        // [CSR ID : 1893050] MLB UI System 개별 Cell 구성 시 '검사조건 Skip' 으로 구성된 제품 Cell 교체 기능 구현
                        // 기존에는 교체 시에 정합성 체크에서 문제가 발생하면 교체를 하지 못했는데 이 부분을 Pallet의 검사조건 Skip 여부에 따라 선택할 수 있도록 수정해달라고 한 사항임.
                        // 해당 셀 ID가 속한 Pallet 이 검사조건을 Skip했는지 Skip하지 않았는지 확인하는 함수 호출 : 2011 06 02 홍광표S
                        SelectSkipYNCheck(txtBeforeCell.Text);

                        // 포커스 이동
                        txtAfterCell.Focus();
                        txtAfterCell.SelectAll();
                        break;
                }
            }
        }

        /// <summary>
        /// 교체 Cell 입력란에 스캔 혹은 입력 시 동작하는 이벤트 핸들러
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtAfterCell_KeyDown(object sender, KeyEventArgs e)
        {
            // 입력한 값이 Enter 일 경우만 아래 로직 수행
            if (e.Key == Key.Enter)
            {
                // 입력한 값이 널이나 공백이 아닐 경우만 아래 로직 수행
                if (string.IsNullOrEmpty(txtAfterCell.Text))
                {
                    return;
                }

                #region ## 타공장 Cell 교체 처리하기 위함. [CSR ID:2487240] -- 사용안함.
                string sPlant = "";
                if (chkPlant_Check.IsChecked == true)
                {
                    sPlant = Search2Plant();
                    if (sPlant.Equals("NG"))
                    {
                        Util.MessageValidation("SFU1209");  //"셀의 정보를 찾을 수 없습니다." >>Cell 정보가 없습니다.                        
                        return;
                    }

                    string sFactory = LoginInfo.CFG_AREA_ID;    // A1 or A2
                    //sFactory = sFactory.Substring(1, 1);

                    if (sPlant != sFactory)     //타공장 Cell 내공장 DB에 저장.
                    {
                        string sResult = Save2CellData();

                        if (sResult.Equals("NG"))
                        {
                            Util.MessageValidation("SFU1940"); //"타 공장 Cell 정보 이관 오류"
                            return;
                        }
                    }
                }
                else
                {
                    //sPlant = MLBUtil.ReadReg("FACTORY", "").Substring(1, 1);
                    sPlant = LoginInfo.CFG_AREA_ID;    // A1 or A2

                }
                #endregion


                string msg = "";

                // 셀 정보 조회 함수 호출
                int result = SearchCellInfo(1, txtAfterCell.Text, out msg);

                // 
                switch (result)
                {
                    case 1:
                        Util.MessageValidation("SFU1209");  //"셀 정보가 존재하지 않습니다" >>Cell 정보가 없습니다.

                        // 기존 정보 초기화
                        ClearAfterCellInfo();

                        // 포커스 이동
                        txtAfterCell.Focus();
                        txtAfterCell.SelectAll();
                        break;

                    case 2:
                        Util.AlertInfo(msg);

                        // 기존 정보 초기화
                        ClearAfterCellInfo();

                        // 포커스 이동
                        txtAfterCell.Focus();
                        txtAfterCell.SelectAll();
                        break;

                    default:
                        // 체크박스 선택되어 있는지 확인.
                        string skip = "";
                        if (chkSkip.IsChecked == true)
                        {
                            skip = "Y";
                        }
                        else
                        {
                            skip = "N";
                        }

                        // 교체될 CELL 은 활성화 특성치 정보 가져오는 BizRule 호출
                        // 활성화 BizActor Server 로 접속
                        //MLBUtil.InitializeBizConfigFormation(sPlant);

                        try
                        {

                            //DataTable RQSTDT = new DataTable();
                            //RQSTDT.Columns.Add("CELLID", typeof(string));
                            //RQSTDT.Columns.Add("COND_SKIP", typeof(string));

                            //DataRow dr = RQSTDT.NewRow();
                            //dr["CELLID"] = txtAfterCell.Text.Trim();
                            //dr["COND_SKIP"] = skip;
                            //RQSTDT.Rows.Add(dr);

                            //DataTable dtRslt = null;
                            //if (sAREAID.Equals("A1"))
                            //{
                            //    dtRslt = new ClientProxy2007("AF1").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_GET", "INDATA", "OUTDATA", RQSTDT);
                            //}
                            //else if (sAREAID.Equals("A2") || sAREAID.Equals("S2"))
                            //{
                            //    dtRslt = new ClientProxy2007("AF2").ExecuteServiceSync("SET_GMES_SHIPMENT_CELL_INFO_GET", "INDATA", "OUTDATA", RQSTDT);
                            //}
                            //else
                            //{
                            //    return;
                            //}

                            DataSet indataSet = new DataSet();
                            DataTable RQSTDT = indataSet.Tables.Add("INDATA");

                            RQSTDT.Columns.Add("SUBLOTID", typeof(string));
                            RQSTDT.Columns.Add("SHOPID", typeof(string));
                            RQSTDT.Columns.Add("AREAID", typeof(string));
                            RQSTDT.Columns.Add("EQSGID", typeof(string));
                            RQSTDT.Columns.Add("MDLLOT_ID", typeof(string));
                            RQSTDT.Columns.Add("SUBLOT_CHK_SKIP_FLAG", typeof(string));
                            RQSTDT.Columns.Add("INSP_SKIP_FLAG", typeof(string));
                            RQSTDT.Columns.Add("2D_BCR_SKIP_FLAG", typeof(string));
                            RQSTDT.Columns.Add("USERID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["SUBLOTID"] = txtAfterCell.Text.Trim();
                            dr["SHOPID"] = string.Empty;
                            dr["AREAID"] = string.Empty;
                            dr["EQSGID"] = string.Empty;
                            dr["MDLLOT_ID"] = string.Empty;
                            dr["SUBLOT_CHK_SKIP_FLAG"] = "N";
                            dr["INSP_SKIP_FLAG"] = skip;
                            dr["2D_BCR_SKIP_FLAG"] = "Y";
                            dr["USERID"] = LoginInfo.USERID;
                            RQSTDT.Rows.Add(dr);

                            // ClientProxy2007
                            DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_FCS_VALIDATION", "INDATA", "OUTDATA", indataSet);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);

                            // 기존 정보 초기화 -- 단위 TEST를 위해 주석처리함.
                            //ClearAfterCellInfo();

                            // 포커스 이동
                            txtAfterCell.Focus();
                            txtAfterCell.SelectAll();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 교체처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            // 기본적인 Validation
            if (string.IsNullOrEmpty(txtBeforeCell.Text.Trim()))
            {
                Util.MessageValidation("SFU1286"); //[불량] 교체될 CELL ID가 미입력 상태입니다.

                // 포커스 이동
                txtBeforeCell.Focus();
                txtBeforeCell.SelectAll();
                return;
            }
            else if (string.IsNullOrEmpty(txtRemark.Text.Trim()))
            {
                Util.MessageValidation("SFU1252"); //"교체 사유는 필수 입력사항입니다." >>교체 사유는 필수 입니다.

                // 포커스 이동
                txtRemark.Focus();
                txtRemark.SelectAll();
                return;
            }
            else if (string.IsNullOrEmpty(txtBePRODID.Text.Trim()))
            {
                string sCarriageReturn = Convert.ToString((char)13) + Convert.ToString((char)10);
                Util.MessageValidation("SFU1464", new object[] { sCarriageReturn }); //"교체 처리를 하시기 전에 CELL ID를 조회하시기 바랍니다. {0} ※ 직접 KeyIn하신 경우는 필히 엔터키를 입력하십시오."
                // 포커스 이동
                txtBeforeCell.Focus();
                txtBeforeCell.SelectAll();
                return;
            }

            if (chkExtraction.IsChecked == false && string.IsNullOrEmpty(txtAfterCell.Text.Trim()))
            {
                Util.MessageValidation("SFU1466"); //"교체할 CELL ID가 미입력 상태입니다."

                // 포커스 이동
                txtAfterCell.Focus();
                txtAfterCell.SelectAll();
                return;
            }
            if (chkExtraction.IsChecked == false && string.IsNullOrEmpty(txtAfPRODID.Text.Trim()))
            {
                string sCarriageReturn = Convert.ToString((char)13) + Convert.ToString((char)10);
                Util.MessageValidation("SFU1464", new object[] { sCarriageReturn }); //"교체 처리를 하시기 전에 CELL ID를 조회하시기 바랍니다. {0} ※ 직접 KeyIn하신 경우는 필히 엔터키를 입력하십시오."
                // 포커스 이동
                txtAfterCell.Focus();
                txtAfterCell.SelectAll();
                return;
            }

            //string sUser = Util.NVC(cboProcUser.SelectedValue);
            //if (sUser == "" || sUser == "SELECT")
            //{
            //    Util.AlertInfo("작업자를 선택해주세요.");
            //    return;
            //}

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("FROM_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("TO_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("BCR2D", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));

                

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["FROM_SUBLOTID"] = txtBeforeCell.Text.Trim();
                dr["TO_SUBLOTID"] = chkExtraction.IsChecked == true ? null : txtAfterCell.Text.Trim();
                dr["BCR2D"] = chkExtraction.IsChecked == true ? null : txt2D.Text.Trim();
                dr["USERID"] = LoginInfo.USERID;    // sUser;
                dr["NOTE"] = txtRemark.Text.Trim();
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_REPLACE_CELL", "RQSTDT", null, RQSTDT);

                // 메시지 출력
                if (chkExtraction.IsChecked == true)
                {
                    Util.MessageInfo("SFU1891"); //"정상적으로 Cell 추출 되었습니다."
                }
                else
                {
                    Util.MessageInfo("SFU1890"); //"정상적으로 Cell 정보 변경되었습니다."
                }
                

                // 초기화
                ClearBeforeCellInfo();
                ClearAfterCellInfo();
                txtBeforeCell.Text = "";
                //btnBigo.Text = "";
                txtAfterCell.Text = "";
                txt2D.Text = "";

                // 체크박스 활성화 & 체크 안 되어 있는 상태로 변경. 2011 06 02 홍광표S
                chkSkip.IsEnabled = true;
                chkSkip.IsChecked = false;

                // 포커스 이동
                txtBeforeCell.Focus();
                txtBeforeCell.SelectAll();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        #endregion


        #region Mehod

        /// <summary>
        /// 불량 Cell 정보 초기화
        /// </summary>
        private void ClearBeforeCellInfo()
        {
            txtBePalletID.Text = "";
            txtBeTrayID.Text = "";
            txtBeCellPos.Text = "";
            txtBeLotID.Text = "";
            txtBeRELSID.Text = "";
            txtBePRODID.Text = "";
            //txtBeShipID.Text = "";
        }

        /// <summary>
        /// 교체 Cell 정보 초기화
        /// </summary>
        private void ClearAfterCellInfo()
        {
            txtAfPalletID.Text = "";
            txtAfTrayID.Text = "";
            txtAfCellPos.Text = "";
            txtAfLotID.Text = "";
            txtAfRELSID.Text = "";
            txtAfPRODID.Text = "";
            //txtAfShipID.Text = "";
        }

        /// <summary>
        /// 셀 정보 조회 함수
        /// </summary>
        /// <param name="cellID"></param>
        private int SearchCellInfo(int cellseq, string cellID, out string msg)
        {

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("FROM_SUBLOTID", typeof(string));
                RQSTDT.Columns.Add("TO_SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_SUBLOTID"] = (cellseq == 0 ? cellID : null);
                dr["TO_SUBLOTID"] = (cellseq == 0 ? null : cellID);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_INFO_FOR_REPLACE", "RQSTDT", "RSLTDT", RQSTDT);
                // 데이터테이블에 값이 없다면 result값에 false 대입하고 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    msg = "";
                    return 1;
                }
                else if (Util.NVC(dtResult.Rows[0]["RESULT_YN"]) == "N")
                {
                    msg = "";
                    return 1;
                }
                else
                {
                    // Before Cell 에 해당하면
                    if (cellseq == 0)
                    {
                        txtBePalletID.Text = Util.NVC(dtResult.Rows[0]["FROM_OUTER_BOXID"]);         // PalletID
                        txtBeTrayID.Text = Util.NVC(dtResult.Rows[0]["FROM_INNER_BOXID"]);              // BoxID : 포장 공정 내 Tray
                        txtBeCellPos.Text = Util.NVC(dtResult.Rows[0]["FROM_BOX_PSTN_NO"]);       // 포장 트레이 내 셀 위치
                        txtBeLotID.Text = Util.NVC(dtResult.Rows[0]["FROM_LOTID"]);              // 조립 LotID
                        txtBeRELSID.Text = Util.NVC(dtResult.Rows[0]["FROM_RCV_ISS_ID"]);            // 포장 출고 ID
                        txtBePRODID.Text = Util.NVC(dtResult.Rows[0]["FROM_PRODID"]);
                        //INSP_SKIP_FLAG = Util.NVC(dtResult.Rows[0]["FROM_INSP_SKIP_FLAG"]);                         
                    }
                    // After Cell 에 해당하면
                    else
                    {
                        txtAfPalletID.Text = Util.NVC(dtResult.Rows[0]["TO_OUTER_BOXID"]);         // PalletID
                        txtAfTrayID.Text = Util.NVC(dtResult.Rows[0]["TO_INNER_BOXID"]);              // BoxID : 포장 공정 내 Tray
                        txtAfCellPos.Text = Util.NVC(dtResult.Rows[0]["TO_BOX_PSTN_NO"]);       // 포장 트레이 내 셀 위치
                        txtAfLotID.Text = Util.NVC(dtResult.Rows[0]["TO_LOTID"]);              // 조립 LotID
                        txtAfRELSID.Text = Util.NVC(dtResult.Rows[0]["TO_RCV_ISS_ID"]);            // 포장 출고 ID
                        txtAfPRODID.Text = Util.NVC(dtResult.Rows[0]["TO_PRODID"]);
                        //txtAfShipID.Text = Util.NVC(dtResult.Rows[0]["SHIPID"]);            // 출하 ID      
                        sAREAID = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                    }

                    msg = "";
                    return 0;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return 2;
            }
        }

        /// <summary>
        /// 해당 셀 ID가 속한 Pallet 이 검사조건을 Skip했는지 Skip하지 않았는지 확인하는 함수
        /// </summary>
        /// <param name="cellID"></param>
        /// <returns></returns>
        private void SelectSkipYNCheck(string cellID)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = cellID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INSP_SKIP_CHK_CP", "RQSTDT", "RSLTDT", RQSTDT);

                // 데이터테이블에 값이 없다면 함수 중단함.
                if (dtResult.Rows.Count <= 0)
                {
                    //return false;
                }
                else if (Util.NVC(dtResult.Rows[0]["INSPECT_SKIP"]) == "N")
                {
                    // 체크박스 선택하지 못하도록 변경 : 검사조건 No Skip
                    chkSkip.IsEnabled = false;
                }
                else if (Util.NVC(dtResult.Rows[0]["INSPECT_SKIP"]) == "Y")
                {
                    // 체크박스 선택된 상태로 변경 : 검사조건 Skip
                    chkSkip.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        /// <summary>
        /// 타공장 Cell 교체 처리 위한 함수 - 어떤 공장 Cell인지 조회.
        /// </summary>
        /// <returns></returns>
        private string Search2Plant()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = txtAfterCell.Text.Trim();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? BR_GET_SHOPID_BY_CELLID", "RQSTDT", "RSLTDT", RQSTDT);

                if (Util.NVC(dtResult.Rows[0][0]) != "NG")
                {
                    string sPlant = Util.NVC(dtResult.Rows[0][0]);
                    //return (sPlant == "O1P" ? "1" : "2");
                    return sPlant;
                }
                else
                {
                    return "NG";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "NG";
            }
        }

        /// <summary>
        /// 타공장 Cell 교체 처리 위한 함수 - 타공장 Cell 내공장 DB에 저장
        /// </summary>
        /// <returns></returns>
        private string Save2CellData()
        {
            DataSet ds = new DataSet();
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("CELLID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CELLID"] = txtAfterCell.Text.Trim();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("??? BR_INS_CELL_O1P_O2P", "RQSTDT", "RSLTDT", RQSTDT);
                if ( Util.NVC(dtResult.Rows[0][0]) != "NG")
                {
                    return "OK";
                }
                else
                {
                    return "NG";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "NG";
            }
        }



        #endregion


    }
}
