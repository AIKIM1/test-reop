/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 포장 Pallet 구성 (BOX) - Pallet 실적확정 PopUp
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.07.18  이제섭    : UNCODE (수출인증코드) 추가 (공통코드 등록된 Plant는 필수입력)
  2023.04.21  권혜정    : UNCODE 수량 제한 및 유효 기간 설정 추가
  2023.05.16  권혜정    : BOX 구성 USERNAME 변경
  2023.12.27  박나연    : UN CODE 사용 수량 및 HISTORY 생성 시 오류 발생하여 OUTDATA 'PALLETID' -> 'BOXID'로 수정 





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_311_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _SavePalletBizName = "BR_SET_END_PALLET_BY_BOX_BX";

        string _sBoxList = string.Empty;
        string _sSHOPID = string.Empty;
        string _sAREAID = string.Empty;
        string _sLINEID = string.Empty;
        string _sPRODID = string.Empty;      
        string _sUSERID = string.Empty;
        string _sSkip = string.Empty;
        string _sUSERNAME = string.Empty;
        string[] _sUNCODE = new string[3];

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


        public BOX001_311_CONFIRM()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            //InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _sBoxList = Util.NVC(tmps[0]);
            txtBoxQty.Text = Util.NVC(tmps[1]);  //박스수량
            txtCellqty.Text = Util.NVC(tmps[2]); //셀수량
            txtModel.Text = Util.NVC(tmps[3]); //모델
            txtProdID.Text = Util.NVC(tmps[4]); //제품
            _sSkip = Util.NVC(tmps[5]); //검사여부
            txtSkip.Text = _sSkip == "Y"? "SKIP" : "NO SKIP";
            _sLINEID = Util.NVC(tmps[6]); //라인
            txtLine.Text = Util.NVC(tmps[7]); //라인
            _sAREAID = Util.NVC(tmps[9]);
            _sSHOPID = Util.NVC(tmps[10]);
            _sUSERNAME = Util.NVC(tmps[12]);

            dtShipDate.Text = DateTime.Now.ToShortDateString();
            dtUserDate.Text = DateTime.Now.ToShortDateString();
            dtUserTime.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0);
          
            //출하처 Combo Set.
            CommonCombo combo = new CommonCombo();
            string[] sFilter3 = { _sSHOPID, _sLINEID, _sAREAID };
            combo.SetCombo(cboComp, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
            cboComp.SelectedValue = Util.NVC(tmps[8]);
            cboComp.IsEnabled = false;
            
            combo.SetCombo(cboExpDom,CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");
            setExpDom();
            txtConbineseq.AllowNull = true;
            txtShipqty.AllowNull = true;
            txtConbineseq.Value = double.NaN;
            txtShipqty.Value = double.NaN;

            //   txtProcUser.Text = LoginInfo.USERNAME;        
            txtProcUser.Text = tmps[11] as string;
            txtProcUser.Tag = tmps[12] as string;

            //공통코드에 등록된 Shop은 UNCODE 입력 TextBox Visible 처리
            if (UseCommoncodePlant())
            {
                txtUnCode.Visibility = Visibility.Visible;
                tbUnCode.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region Mehod
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
                inData["DATA_TYPE"] = "BOX";
                inData["PROCID"] = Process.CELL_BOXING;
                inData["EQSGID"] = _sLINEID;

                inDataTable.Rows.Add(inData);

                foreach (string boxid in _sBoxList.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    DataRow inLot = inLotTable.NewRow();
                    inLot["LOTID"] = boxid;
                    inLotTable.Rows.Add(inLot);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_EXP_DOM_TYPE_BX", "INDATA,INLOT", "OUTDATA", indataSet);
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
        #endregion

        #region Event


        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            #region # 필수 입력값 확인하는 부분
            if (double.IsNaN(txtShipqty.Value) || txtShipqty.Value == 0)
            {
                //출하 수량을 입력해주세요.
                Util.MessageValidation("SFU3174");
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3174"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                txtShipqty.Focus();
                return;
            }
            else if (double.IsNaN(txtConbineseq.Value) || txtConbineseq.Value == 0)
            {
                //구성 차수 관리 번호를 입력해주세요.
                Util.MessageValidation("SFU3146");
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3146"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                txtConbineseq.Focus();
                return;
            }

            else if (Util.NVC(cboExpDom.SelectedValue) == "" || Util.NVC(cboExpDom.SelectedValue) == "SELECT")
            {
                Util.MessageValidation("SFU3606"); //'수출/내수'를 선택해주세요
                return;
            }
            //UNCODE 입력여부 체크
            else if (UseCommoncodePlant() && string.IsNullOrWhiteSpace(txtUnCode.Text))
            {
                //%1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", "UNCODE");
                return;
            }
            else if (UseCommoncodePlant())
            {
                _sUNCODE = GetUncodeUseDate(txtUnCode.Text);
                DateTime dtUncodeDate = DateTime.ParseExact(_sUNCODE[2], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None); // 유효기간

                if (_sUNCODE == null)
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
                else if (Convert.ToInt32(_sUNCODE[1]) <= 0)
                {
                    // UNCODE 사용 수량 초과하였습니다.
                    Util.MessageValidation("SFU8907");
                    return;
                }
            }

            #endregion

            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("MODLID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("INSPECT_SKIP", typeof(string));
                inDataTable.Columns.Add("PACKOUT_GO", typeof(string));
                inDataTable.Columns.Add("SHIPDATE", typeof(string));
                inDataTable.Columns.Add("SHIPQTY", typeof(Decimal));
                inDataTable.Columns.Add("PACKQTY", typeof(Decimal));
                inDataTable.Columns.Add("COMBINESEQ", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTUSER", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("UNCODE", typeof(string));

                DataTable inDataCellTable = indataSet.Tables.Add("INDATA_BOX");
                inDataCellTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["EQSGID"] = _sLINEID;
                inData["PROCID"] = Process.CELL_BOXING;
                inData["MODLID"] = txtModel.Text;
                inData["PRODID"] = txtProdID.Text;
                inData["INSPECT_SKIP"] = _sSkip;
                inData["SHIPQTY"] = txtShipqty.Value;
                inData["SHIPDATE"] = dtShipDate.SelectedDateTime.ToString("yyyyMMdd");
                inData["PACKOUT_GO"] = Util.NVC(cboComp.SelectedValue);
                inData["PACKQTY"] = txtCellqty.Text;
                inData["COMBINESEQ"] = txtConbineseq.Value;
                inData["USERID"] = LoginInfo.USERID;
                inData["ACTUSER"] = txtProcUser.Text;
                inData["NOTE"] = txtNote.Text;
                inData["MKT_TYPE_CODE"] = cboExpDom.SelectedValue;
                inData["UNCODE"] = !string.IsNullOrWhiteSpace(txtUnCode.Text.Trim()) ? txtUnCode.Text.Trim() : null;

                inDataTable.Rows.Add(inData);

                foreach (string boxid in _sBoxList.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    DataRow inDataCell = inDataCellTable.NewRow();
                    inDataCell["BOXID"] = boxid;
                    inDataCellTable.Rows.Add(inDataCell);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(_SavePalletBizName, "INDATA,INDATA_BOX", "OUTDATA", indataSet);

                // 리턴 받은 ID가 널이라면 함수 종료
                if (dsResult == null || dsResult.Tables["OUTDATA"] == null || dsResult.Tables["OUTDATA"].Rows.Count <= 0)
                {
                    return;
                }

                _RetDT = dsResult.Tables["OUTDATA"];

                // UN CODE 사용 수량 및 HISTORY 생성
                if (UseCommoncodePlant())
                {
                    UpdateUncode(_sUNCODE[0], _sUNCODE[1], _sUSERNAME, _RetDT.Rows[0]["BOXID"].ToString());
                }

                this.DialogResult = MessageBoxResult.OK;
                //    Print_Box();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
            this.Close();
        }

       
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion        
    }
}
