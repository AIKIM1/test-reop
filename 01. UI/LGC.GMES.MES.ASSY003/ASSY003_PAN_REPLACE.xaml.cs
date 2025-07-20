/*************************************************************************************
 Created Date : 2017.06.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 교체처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.14  INS 김동일K : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_PAN_REPLACE : C1Window, IWorkArea
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
        private string _Prod_LotID = string.Empty;
        private string _CstID = string.Empty;

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

        public ASSY003_PAN_REPLACE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 10)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _WipQty = Util.NVC(tmps[4]).Equals("") ? 0 : double.Parse(Util.NVC(tmps[4]));
                _Position = Util.NVC(tmps[5]);
                _ProcID = Util.NVC(tmps[6]);
                _Input_type_code = Util.NVC(tmps[7]);
                _Prod_LotID = Util.NVC(tmps[8]);
                _CstID = Util.NVC(tmps[9]);
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
                _Prod_LotID = "";
                _CstID = "";
            }
            ApplyPermissions();
            SetInfo();

            //// HOLD 사유 코드
            CommonCombo _combo = new CommonCombo();

            string[] sFilter2 = { "CORE_RADIUS" };
            _combo.SetCombo(cboCoreRds, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "CORE_RADIUS");

            //HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            txtChangeQty.IsReadOnly = false;    // ea 직접 입력 가능하도록 기능 변경 요청으로 수정.

            if (!_ProcID.Equals(Process.LAMINATION))
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

            if (_ProcID.Equals(Process.LAMINATION) && _Input_type_code.Equals("PROD"))
            {
                chkAutoPrint.Visibility = Visibility.Visible;
            }
            else
            {
                chkAutoPrint.Visibility = Visibility.Collapsed;
            }
        }

        private void txtInChangeQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtQtyChange();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtInChangeQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                txtQtyChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (txtInChangeQty.Text.Length <= 0)
            {
                Util.MessageValidation("SFU3587");  //제품반경을 입력 하세요.
                return;
            }

            GetConvertQty();
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
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("처리 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

                    if (cboCoreRds == null || cboCoreRds.SelectedIndex < 0 || cboCoreRds.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU3276"); // 코어반지름을 선택하세요.
                        cboCoreRds.Focus();
                        return;
                    }


                    double iCoreRadius = 0;
                    double iPancakelength = double.Parse(txtInChangeQty.Text);
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

                    if (!double.TryParse(cboCoreRds.Text.ToString(), out iCoreRadius))//Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["CORE_RADIUS"]));
                    {
                        Util.MessageValidation("SFU3277");   // 코어 반지름 기준정보가 잘못 되었습니다.
                        return;
                    }

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

                DataSet indataSet = new DataSet();
                string sBizName = string.Empty;

                if (_ProcID.Equals(Process.SRC))
                {
                    indataSet = _Biz.GetBR_PRD_REG_REMAIN_LOT_SRC();
                    sBizName = "BR_PRD_REG_REMAIN_LOT_SRC";
                }
                else if (_ProcID.Equals(Process.STP) || _ProcID.Equals(Process.SSC_FOLDED_BICELL))
                {
                    indataSet = _Biz.GetBR_PRD_REG_REMAIN_INPUT_IN_LOT();
                    sBizName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT";
                }
                else if (_ProcID.Equals(Process.SSC_BICELL))
                {
                    // SSC Bi-Cell
                    indataSet = _Biz.GetBR_PRD_REG_REMAIN_LOT_SSCBI();
                    sBizName = "BR_PRD_REG_REMAIN_LOT_SSCBI";
                }
                else if (_ProcID.Equals(Process.PACKAGING) || _ProcID.Equals(Process.TAPING_AFTER_FOLDING))
                {
                    indataSet = _Biz.GetBR_PRD_REG_REMAIN_INPUT_IN_LOT();
                    sBizName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT_S";
                }
                else
                {
                    indataSet = _Biz.GetBR_PRD_REG_REMAIN_INPUT_IN_LOT();
                    sBizName = "BR_PRD_REG_REMAIN_INPUT_IN_LOT";
                }

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                if (_ProcID.Equals(Process.SRC) || _ProcID.Equals(Process.STP) ||
                    _ProcID.Equals(Process.SSC_BICELL) || _ProcID.Equals(Process.SSC_FOLDED_BICELL) ||
                    _ProcID.Equals(Process.PACKAGING) || _ProcID.Equals(Process.TAPING_AFTER_FOLDING))
                {
                    newRow["PROD_LOTID"] = _Prod_LotID; 
                }
                else
                {
                    newRow["PROD_LOTID"] = "";
                }
                
                newRow["INPUT_LOT_TYPE"] = _Input_type_code;

                if (_ProcID.Equals(Process.SRC) || _ProcID.Equals(Process.SSC_BICELL))
                {
                    newRow["LANGID"] = LoginInfo.LANGID;
                }

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputLot = indataSet.Tables["IN_INPUT"];
                newRow = inInputLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _Position;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = txtLotId.Text;
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                
                if (_ProcID.Equals(Process.SRC) || _ProcID.Equals(Process.SSC_BICELL))
                {
                    newRow["REMAIN_QTY"] = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : Convert.ToDecimal(txtChangeQty.Text);
                    newRow["OUTPUT_QTY"] = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : _WipQty - Convert.ToDouble(txtChangeQty.Text);
                }
                else
                {
                    newRow["ACTQTY"] = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : Convert.ToDecimal(txtChangeQty.Text);
                }

                if (_ProcID.Equals(Process.SRC) || _ProcID.Equals(Process.STP) || _ProcID.Equals(Process.PACKAGING))
                {
                    newRow["CSTID"] = _CstID;
                }

                inInputLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,IN_INPUT", null, indataSet);

                // 라미 공정 잔량 처리 시 감열지 발행.
                if (_ProcID.Equals(Process.LAMINATION) && _Input_type_code.Equals("PROD")
                    && chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                {
                    PrintThermalPrint(txtChangeReason.Text.Trim(), txtTotalQty.Text);
                }


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

                inDataTable.Rows.Add(inRow);
                inRow = null;

                inRow = inLot.NewRow();
                inRow["LOTID"] = txtLotId.Text;
                inRow["HOLD_NOTE"] = txtChangeReason.Text;
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
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAMI_REMAIN_PRT_INFO", "INDATA", "OUTDATA", inTable);

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

            if ( Convert.ToDecimal(txtChangeQty.Text.Trim()) < 1 )
            {
                //Util.Alert("잔량은 1이상이어야 합니다.");
                Util.MessageValidation("SFU4207");
                return bRet;
            }

            if ( Convert.ToInt32(Math.Floor(Convert.ToDecimal(txtChangeQty.Text.Trim())))
              == Convert.ToInt32(Math.Floor(Convert.ToDecimal(txtTotalQty.Text.Trim()))))
            {
                //Util.Alert("잔량이 투입량과 같습니다.");
                Util.MessageValidation("SFU4208");
                return bRet;
            }

            if (txtChangeReason.Text.Trim().Equals(""))
            {
                //Util.Alert("특이사항을 입력하세요.");
                Util.MessageValidation("SFU1992");
                return bRet;
            }

            double dTot, dChg;
            double.TryParse(txtTotalQty.Text, out dTot);
            double.TryParse(txtChangeQty.Text, out dChg);

            if (dTot < dChg)
            {
                //Util.Alert("잔량이 총 수량보다 많습니다.");
                Util.MessageValidation("SFU1861");
                return bRet;
            }

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
            listAuth.Add(btnConvert);
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void txtQtyChange()
        {
            if (!Util.CheckDecimal(txtInChangeQty.Text, 0))
            {
                txtInChangeQty.Text = "";
                return;
            }
        }

        private void SetInfo()
        {
            txtLotId.Text = _LotID;
            txtTotalQty.Text = _WipQty.ToString();
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

                // 2018-01-12 Lee. D. R
                // 남경 라미 잔량처리시 PRODID 추가 요청 (PJT명 밑)
                dicParam.Add("PRDT_CLSS_NAME", Util.NVC(dtRslt.Rows[0]["PRDT_CLSS_NAME"]));

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

        private void txtChangeQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtChangeQty.Text, 0))
                {
                    txtChangeQty.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //Util.AlertInfo("오늘이후날짜만지정가능합니다.");
                Util.MessageValidation("SFU1740");
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }
    }
}
