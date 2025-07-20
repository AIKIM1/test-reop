/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 포장 Pallet 구성 (BOX) - Pallet 실적확정 PopUp
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.07.18 이제섭 UNCODE (수출인증코드) 추가 (공통코드 등록된 Plant는 필수입력)
  2023.07.11 안유수 E20230404-000532   UNCODE 수량관리하는 PLANT 분기 처리




 
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

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_026_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _SavePalletBizName = "BR_PRD_REG_PACKING_PALLET_FOR_BOX";

        string _sBoxList = string.Empty;
        string _sSHOPID = string.Empty;
        string _sAREAID = string.Empty;
        string _sLINEID = string.Empty;
        string _sPRODID = string.Empty;      
        string _sUSERID = string.Empty;
        string _sSkip = string.Empty;

        // 팝업 호출한 폼으로 리턴함.
        private DataTable _RetDT = null;

        private DataTable CommDT = null;
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


        public BOX001_026_CONFIRM()
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
            CommDT = UseCommoncodePlant();
            if (CommonVerify.HasTableRow(CommDT))
            {
                txtUnCode.Visibility = Visibility.Visible;
                tbUnCode.Visibility = Visibility.Visible;
            }
        }

        #endregion

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
        /// UNCODE 필수입력 Plant 조회
        /// </summary>
        /// <returns></returns>
        private DataTable UseCommoncodePlant()
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

            return dtRslt;
        }


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
            else if (CommonVerify.HasTableRow(CommDT) && string.IsNullOrWhiteSpace(txtUnCode.Text))
            {
                if (Util.NVC(CommDT.Rows[0]["ATTRIBUTE1"]) != "Y")
                {
                    //%1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", "UNCODE");
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
