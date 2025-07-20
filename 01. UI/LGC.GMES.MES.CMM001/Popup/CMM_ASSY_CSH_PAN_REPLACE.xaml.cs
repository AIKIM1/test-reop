/*************************************************************************************
 Created Date : 2017.07.03
      Creator : INS 이대영D
   Decription : Assembly 잔량처리(초소형)
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.03  INS 이대영D : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CSH_PAN_REPLACE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private double _WipQty = 0;
        private string _Position = string.Empty;
        private string _ProcID = string.Empty;
        private string _Input_type_code = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSY_CSH_PAN_REPLACE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 8)
            {
                _LineID = Util.NVC(tmps[0]);                                                    // Line
                _EqptID = Util.NVC(tmps[1]);                                                    // 설비코드
                _LotID = Util.NVC(tmps[2]);                                                     // 투입LOT
                _WipSeq = Util.NVC(tmps[3]);                                                    // 재공 일련번호
                _WipQty = Util.NVC(tmps[4]).Equals("") ? 0 : double.Parse(Util.NVC(tmps[4]));   // 투입량
                _Position = Util.NVC(tmps[5]);                                                  // 위치
                _ProcID = Util.NVC(tmps[6]);                                                    // Process
                _Input_type_code = Util.NVC(tmps[7]);                                           // 투입제품타입
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _WipQty = 0;
                _Position = "";
                _ProcID = "";
                _Input_type_code = "";
            }
            ApplyPermissions();
            DataTable dt = new DataTable();
            GetRemainInfo(ref dt);
            if (dt != null)
                SetInfo(dt);

            InitControl(); // Control 초기화
        }

        private void InitControl()
        {
            //// HOLD 사유 코드
            CommonCombo _combo = new CommonCombo();

            //HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            txtChangeQty.IsReadOnly = false;    // ea 직접 입력 가능하도록 기능 변경 요청으로 수정.

            if (!_ProcID.Equals(Process.ASSEMBLY))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);
                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;
            }

            if (!_Input_type_code.Equals("PROD"))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;

                grdStats.Visibility = Visibility.Collapsed;

                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);
                grdContents.RowDefinitions[6].Height = new GridLength(0);
                grdContents.RowDefinitions[7].Height = new GridLength(0);
                grdContents.RowDefinitions[8].Height = new GridLength(0);
                grdContents.RowDefinitions[9].Height = new GridLength(0);
                grdContents.RowDefinitions[10].Height = new GridLength(0);
                grdContents.RowDefinitions[11].Height = new GridLength(0);
            }
        }

        private DataTable GetRemainInfo(ref DataTable selectDt)
        {
            string sBizName = string.Empty;
            sBizName = "DA_PRD_SEL_WINDING_REMAIN_INFO_AS";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("INPUT_LOTID", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["EQPTID"] = Util.NVC(_EqptID);
            inData["EQPT_MOUNT_PSTN_ID"] = Util.NVC(_Position);
            inData["INPUT_LOTID"] = Util.NVC(_LotID);

            inDataTable.Rows.Add(inData);

            selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

            return selectDt;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Input_type_code.Equals("PROD"))
            {
                if (!CanSave())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("잔량처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1862", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {                        
                        Save();
                    }
                });
            }
            else
            {
                Util.MessageConfirm("SFU1925", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Save();
                    }
                });
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetConvertQty()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_CONVERT_UNIT_QTY();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = txtLotId.Text;
                //newRow["QTY"] = txtInChangeQty.Text;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONVERT_UNIT_QTY", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bool bOC14 = false;

                    if (Util.NVC(dtRslt.Rows[0]["PTN_LEN"]).Equals(""))
                    {
                        //Util.Alert("변환을 위한 패턴길이 기준정보가 없습니다.");
                        Util.MessageValidation("SFU1570");
                        return;
                    }

                    if (Util.NVC(dtRslt.Rows[0]["ELTR_TCK"]).Equals(""))
                    {
                        // 전극두께 컬럼 이동으로 인한 처리 로직으로 14라인 운영 중이므로 14라인 관련 반제품 코드인 경우는 하드코딩 처리... 4/1일 오픈 전까지.. (최종엽C 요청)
                        if (_LineID.Equals("A2A14") && int.Parse(System.DateTime.Now.ToString("yyyyMMdd")) < 20170401)
                        {
                            if (Util.NVC(dtRslt.Rows[0]["PRODID"]).Equals("ANPAN0486A") || Util.NVC(dtRslt.Rows[0]["PRODID"]).Equals("ANPCA0488A")) //E63 제품 코드...
                            {
                                bOC14 = true;
                            }
                            else
                            {
                                //Util.Alert("변환을 위한 전극두께 기준정보가 없습니다.");
                                Util.MessageValidation("SFU1568");
                                return;
                            }
                        }
                        else
                        {
                            //Util.Alert("변환을 위한 전극두께 기준정보가 없습니다.");
                            Util.MessageValidation("SFU1568");
                            return;
                        }

                        ////Util.Alert("변환을 위한 전극두께 기준정보가 없습니다.");
                        //Util.MessageValidation("SFU1568");
                        //return;
                    }

                    //if (Util.NVC(dtRslt.Rows[0]["CORE_RADIUS"]).Equals(""))
                    //{
                    //    //Util.Alert("변환을 위한 코어반지름 기준정보가 없습니다.");
                    //    Util.MessageValidation("SFU1569");
                    //    return;
                    //}

                    double iCoreRadius = 0;
                    double iPancakelength = double.Parse(txtChangeQty.Text);
                    double dElecThckness = 0;
                    double dPitch = 0;
                    double Qty = 0;

                    dPitch = Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["PTN_LEN"]));
                    if (bOC14)
                    {
                        if (Util.NVC(dtRslt.Rows[0]["PRODID"]).Equals("ANPAN0486A"))
                            dElecThckness = 180;
                        else if (Util.NVC(dtRslt.Rows[0]["PRODID"]).Equals("ANPCA0488A"))
                            dElecThckness = 169;
                    }
                    else
                        dElecThckness = Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["ELTR_TCK"]));

                    Qty = Math.PI * (Math.Pow(((iPancakelength) + iCoreRadius), 2) - Math.Pow((iCoreRadius), 2)) / dElecThckness / dPitch;

                    //Qty = Math.PI * (Math.Pow((iPancakelength), 2) - Math.Pow((iCoreRadius), 2)) / dElecThckness / dPitch;  // CMI 요청으로 계산식 변경.

                    if (double.IsInfinity(Qty))
                    {
                        //변환값이 무한대 입니다.\r\n기준 정보를 확인하세요.
                        Util.MessageValidation("SFU3085");
                        return;
                    }

                    txtChangeQty.Text = Qty.ToString("####"); //Convert.ToString(Qty).ToString("####");
                }
                else
                {
                    //Util.Alert("변환을 위한 기준정보가 없습니다.");
                    Util.MessageValidation("SFU1567");
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                //DataSet inDataSet = _Biz.GetBR_PRD_REG_REMAIN_INPUT_IN_LOT();
                const string bizRuleName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT_AS";

                DataSet inDataSet = new DataSet();
                //DataTable inTable = inDataSet.Tables["INDATA"];
                DataTable inData = inDataSet.Tables.Add("INDATA");
                DataTable inInput = inDataSet.Tables.Add("IN_INPUT");

                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("INPUT_LOT_TYPE", typeof(string));

                inInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInput.Columns.Add("INPUT_LOTID", typeof(string));
                inInput.Columns.Add("WIPNOTE", typeof(string));
                inInput.Columns.Add("ACTQTY", typeof(Decimal));

                DataRow newRow = inData.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = txtLotId.Text.Trim();
                newRow["INPUT_LOT_TYPE"] = _Input_type_code;

                inData.Rows.Add(newRow);
                
                newRow = inInput.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(_Position);
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = _LotID;
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                newRow["ACTQTY"] = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : Convert.ToDecimal(txtChangeQty.Text);

                inInput.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA,IN_INPUT", null, inDataSet);
                
                if (_Input_type_code.Equals("PROD") && rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
                {
                    if (!HoldProcess())
                    {
                        return;
                    }
                }

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");

                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool HoldProcess()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTION_USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));


                DataRow inRow = inDataTable.NewRow();
                inRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inRow["LANGID"] = LoginInfo.LANGID;
                inRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inRow["USERID"] = LoginInfo.USERID;
                inRow["ACTION_USERID"] = string.Empty;

                inDataTable.Rows.Add(inRow);

                inRow = inLot.NewRow();
                inRow["LOTID"] = _LotID;
                inRow["HOLD_NOTE"] = txtChangeReason.Text.Trim();
                inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                inLot.Rows.Add(inRow);
                
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, inDataSet);

                return true;   
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
                return false;
            }
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK", "INDATA", "OUTDATA", inTable);

                return dtRslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region [Validation]
        private bool CanSave()
        {
            bool bRet = false;
            
            if (txtChangeQty.Text.Trim().Equals("") || txtChangeQty.Text.Trim().Equals("0"))
            {
                //Util.Alert("잔량이 없습니다.");
                Util.MessageValidation("SFU1859");
                return bRet;
            }
            if (txtChangeReason.Text.Trim().Equals(""))
            {
                //Util.Alert("특이사항을 입력하세요.");
                Util.MessageValidation("SFU1992");
                return bRet;
            }

            double dTot, dChg;
            double.TryParse(txtChangeQty.Text, out dChg);

            if (rdoHold.IsChecked.HasValue && !(bool)rdoHold.IsChecked &&
                rdoWait.IsChecked.HasValue && !(bool)rdoWait.IsChecked)
            {
                //Util.Alert("상태변경 여부를 선택 하세요.");
                Util.MessageValidation("SFU1600");
                return bRet;
            }


            if (rdoHold.IsChecked.HasValue && (bool)rdoHold.IsChecked)
            {
                if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
                {
                    //Util.Alert("HOLD 사유를 선택 하세요.");
                    Util.MessageValidation("SFU1342");
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Func]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnConvert);
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void txtQtyChange()
        {
            if (!Util.CheckDecimal(txtChangeQty.Text, 0))
            {
                txtChangeQty.Text = "";
                return;
            }
        }

        private void SetInfo(DataTable dt)
        {
            int iWipQty = 0;                  // 투입수량
            int iChangeQty = 0;               // 잔량
            string sChangeQty = string.Empty; // 투입수량 콤마 삽입을 위한 변수
            string sWipQty = string.Empty;    // 잔량 콤마 삽입을 위한 변수

            if (dt.Rows.Count != 0) // 조회했을 때 데이터가있을경우 조회해온 dt에서 값 바인딩
            {
                txtLotId.Text = Util.NVC(dt.Rows[0]["PROD_LOTID"]); // Lot ID
                txtTrayId.Text = Util.NVC(dt.Rows[0]["CSTID"]);     // Tray ID
                iWipQty = Util.NVC_Int(dt.Rows[0]["INPUT_QTY"]);    // 투입수량
            }
            else                   // 조회했을 때 데이터가없을경우 메인에서 넘겨받은 파라미터 값 바인딩
            {
                txtLotId.Text = Util.NVC(_LotID); // Lot ID
            }
            sWipQty = $"{iWipQty:#,###}";
            txtWipQty.Text = iWipQty != 0 ? Util.NVC(sWipQty) : Util.NVC(0);
            sChangeQty = $"{iChangeQty:#,###}";
            txtChangeQty.Text = iChangeQty != 0 ? Util.NVC(sChangeQty) : Util.NVC(0);
        }

        private void PrintThermalPrint(string sNote, string sTotQty)
        {
            try
            {
                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                DataTable dtRslt = GetThermalPaperPrintingInfo(txtLotId.Text.Trim());

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;


                Dictionary<string, string> dicParam = new Dictionary<string, string>();
                
                
                dicParam.Add("PANCAKEID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                dicParam.Add("TOT_QTY", Convert.ToDouble(Util.NVC(sTotQty)).ToString());
                dicParam.Add("REMAIN_QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                dicParam.Add("NOTE", Util.NVC(sNote));
                dicParam.Add("PRINTQTY", "1");  // 발행 수

                dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                dicList.Add(dicParam);

                CMM_THERMAL_PRINT_LAMI_REMAIN print = new CMM_THERMAL_PRINT_LAMI_REMAIN();
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicList;
                    Parameters[1] = Process.LAMINATION;
                    Parameters[2] = _LineID;
                    Parameters[3] = _EqptID;
                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "N";   // 디스패치 처리.

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.Show();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_LAMI_REMAIN window = sender as CMM_THERMAL_PRINT_LAMI_REMAIN;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }
        #endregion

        #endregion


        private void rdoHold_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (cboHoldReason != null)
                    cboHoldReason.IsEnabled = true;

                if (dtExpected != null)
                    dtExpected.IsEnabled = true;
            }
        }

        private void rdoWait_Checked(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                if (cboHoldReason != null)
                    cboHoldReason.IsEnabled = false;

                if (dtExpected != null)
                    dtExpected.IsEnabled = false;
            }
        }

        private void txtChangeQty_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    e.Handled = true;
                    break;
                }
            }
        }
    }
}
