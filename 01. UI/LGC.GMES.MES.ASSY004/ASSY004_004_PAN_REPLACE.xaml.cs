/*************************************************************************************
 Created Date : 2019.05.15
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 라미 공정진척 화면 - 잔량처리 팝업 (ASSY0001.ASSY001_004_PAN_REPLACE 2019/05/15 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.15  INS 김동일K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_004_PAN_REPLACE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_004_PAN_REPLACE : C1Window, IWorkArea
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
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _CSTID = string.Empty;

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
        public ASSY004_004_PAN_REPLACE()
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
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _WipQty = Util.NVC(tmps[4]).Equals("") ? 0 : double.Parse(Util.NVC(tmps[4]));
                _Position = Util.NVC(tmps[5]);
                _ProcID = Util.NVC(tmps[6]);
                _Input_type_code = Util.NVC(tmps[7]);

                if (tmps.Length > 8)
                    _LDR_LOT_IDENT_BAS_CODE = Util.NVC(tmps[8]);

                if (tmps.Length > 9)
                    _CSTID = Util.NVC(tmps[9]);

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

                _LDR_LOT_IDENT_BAS_CODE = "";
                _CSTID = "";
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

            if (_ProcID.Equals(Process.STACKING_FOLDING))
            {
                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);
                grdContents.RowDefinitions[6].Height = new GridLength(0);
                grdContents.RowDefinitions[7].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;
            }

            if (!_Input_type_code.Equals("PROD"))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);

                grdContents.RowDefinitions[4].Height = new GridLength(0);
                grdContents.RowDefinitions[5].Height = new GridLength(0);

                txtChangeQty.IsReadOnly = false;

                grdStats.Visibility = Visibility.Collapsed;

                grdContents.RowDefinitions[6].Height = new GridLength(0);
                grdContents.RowDefinitions[7].Height = new GridLength(0);
                grdContents.RowDefinitions[8].Height = new GridLength(0);
                grdContents.RowDefinitions[9].Height = new GridLength(0);
                grdContents.RowDefinitions[10].Height = new GridLength(0);
                grdContents.RowDefinitions[11].Height = new GridLength(0);
                grdContents.RowDefinitions[12].Height = new GridLength(0);
                grdContents.RowDefinitions[13].Height = new GridLength(0);
            }

            if (!_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                grdContents.RowDefinitions[2].Height = new GridLength(0);
                grdContents.RowDefinitions[3].Height = new GridLength(0);
            }

            btnSaveAndPrint.Visibility = Visibility.Collapsed;
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

                //잔량처리 하시겠습니까?
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
                //처리 하시겠습니까?
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

        private void Save(bool bPrint = false)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_REMAIN_INPUT_IN_LOT();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["INPUT_LOT_TYPE"] = _Input_type_code;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputLot = indataSet.Tables["IN_INPUT"];
                newRow = inInputLot.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = _Position;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = txtLotId.Text;
                newRow["WIPNOTE"] = txtChangeReason.Text.Trim();
                newRow["ACTQTY"] = Util.NVC(txtChangeQty.Text).Equals("") ? 0 : Convert.ToDecimal(txtChangeQty.Text);
                if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    newRow["CSTID"] = txtCSTId.Text.Trim();

                inInputLot.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_REMAIN_INPUT_IN_LOT", "INDATA,IN_INPUT", null, indataSet);

                //// 라미 공정 잔량 처리 시 감열지 발행.
                //if (_ProcID.Equals(Process.LAMINATION) && _Input_type_code.Equals("PROD")
                //    && chkAutoPrint.IsChecked.HasValue && (bool)chkAutoPrint.IsChecked)
                //{
                //    PrintThermalPrint(txtChangeReason.Text.Trim(), txtTotalQty.Text);
                //}

                // 잔량처리 및 발행 버튼 Click 시 발행 처리.
                if (_ProcID.Equals(Process.STACKING_FOLDING) && bPrint)
                {
                    if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
                    {
                        using (ThermalPrint thmlPrt = new ThermalPrint())
                        {
                            thmlPrt.Print(sEqsgID: Util.NVC(_LineID),
                                          sEqptID: Util.NVC(_EqptID),
                                          sProcID: Process.STACKING_FOLDING,
                                          inData: GetGroupPrintInfo(txtLotId.Text),
                                          iType: THERMAL_PRT_TYPE.COM_OUT_RFID_GRP,
                                          iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                          bSavePrtHist: true,
                                          bDispatch: false);
                        }
                    }
                    else
                    {
                        using (ThermalPrint thmlPrt = new ThermalPrint())
                        {
                            thmlPrt.Print(sEqsgID: Util.NVC(_LineID),
                                          sEqptID: Util.NVC(_EqptID),
                                          sProcID: Process.STACKING_FOLDING,
                                          inData: GetThermalPaperPrintingInfo(txtLotId.Text),
                                          iType: THERMAL_PRT_TYPE.FOL_IN_REMAIN_MAGAZINE,
                                          iPrtCnt: LoginInfo.CFG_THERMAL_COPIES < 1 ? 1 : LoginInfo.CFG_THERMAL_COPIES,
                                          bSavePrtHist: true,
                                          bDispatch: false);
                        }
                    }
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
                //LANG ID 추가 : 2017.11.29
                newRow["LANGID"] = LoginInfo.LANGID;
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

        private DataTable GetGroupPrintInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID; // sTmp;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_GRP_PRT_INFO_FD", "INDATA", "OUTDATA", inTable);
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

        private bool CheckCSTState()
        {
            try
            {
                bool bRet = false;
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("CSTID", typeof(string));

                DataRow dtRow = inData.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["CSTID"] = txtCSTId.Text;
                inData.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_BASIC_INFO_RFID", "INDATA", "OUTDATA", inData);

                if (dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("CSTSTAT"))
                {
                    if (Util.NVC(dtRslt.Rows[0]["CSTSTAT"]).Equals("E"))
                        bRet = true;
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
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

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                if (txtCSTId.Text.Trim().Length < 1)
                {
                    Util.MessageValidation("SFU6051");
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

            if (_LDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                txtCSTId.Text = _CSTID;

                //if (!CheckCSTState())
                //{
                //    txtCSTId.Text = "";
                //    txtCSTId.Focus();
                //}
            }
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

                //프로젝트명, 극성 추가 : 2017.11.28
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

        private void txtCSTId_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCSTId == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCSTId, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSaveAndPrint_Click(object sender, RoutedEventArgs e)
        {
            try
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
                            Save(bPrint: true);
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
