/*************************************************************************************
 Created Date : 2017.01.25
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - SRC 공정진척 화면 - 대기LOT 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.25  INS 정문교C : Initial Created.
   
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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_008_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_008_WAITLOT : C1Window, IWorkArea
    {   
        #region Declaration & Constructor

        private string _ProcID = string.Empty;
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _util = new Util();
        #endregion

        #region Initialize

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY003_008_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event       

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _ProcID = Util.NVC(tmps[0]);
                _LineID = Util.NVC(tmps[1]);
            }
            else
            {
                _ProcID = "";
                _LineID = "";
            }
            ApplyPermissions();


            //HOLD 사유
            CommonCombo _combo = new CommonCombo();
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

            string[] sFilter2 = { "BICELL_LEVEL3_CODE" };
            _combo.SetCombo(cboBicellType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { LoginInfo.CFG_AREA_ID, _ProcID, null, _LineID };
            _combo.SetCombo(cboOtherEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "EQUIPMENTSEGMENT_WITHOUT_SEL_EQSGID");


            if (_ProcID.Equals(Process.STP))
            {
                dgWaitLot.Columns["PRDT_PSTN_NAME"].Visibility = Visibility.Collapsed;
                dgWaitLot.Columns["BICELL_LEVEL3_NAME"].Visibility = Visibility.Collapsed;
            }
            else if (_ProcID.Equals(Process.SRC))
            {
                dgWaitLot.Columns["PRDT_PSTN_NAME"].Visibility = Visibility.Collapsed;
                dgWaitLot.Columns["BICELL_LEVEL3_NAME"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region [HOLD - 저장]
        private void btnExecute_Click(object Sender, RoutedEventArgs e)
        {
            if (!CanExecute())
                return;

            HoldProcess();
        }
        #endregion

        #region [종료]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [BI CELL TYPE 변경]
        private void cboBicellType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetWaitMagazine();
        }
        #endregion

        #region 예상해제일
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //Util.AlertInfo("오늘이후날짜만지정가능합니다.");
                Util.MessageValidation("SFU1740");
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }
        #endregion

        private void chkOtherLine_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkOtherLine?.IsChecked == null)
                    return;

                if ((bool)chkOtherLine.IsChecked)
                    cboOtherEquipmentSegment.IsEnabled = true;
                else
                    cboOtherEquipmentSegment.IsEnabled = false;

                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void chkOtherLine_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chkOtherLine?.IsChecked == null)
                    return;

                if ((bool)chkOtherLine.IsChecked)
                    cboOtherEquipmentSegment.IsEnabled = true;
                else
                    cboOtherEquipmentSegment.IsEnabled = false;

                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboOtherEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboOtherEquipmentSegment?.SelectedValue == null || Util.NVC(cboOtherEquipmentSegment?.SelectedValue).Equals("") || Util.NVC(cboOtherEquipmentSegment?.SelectedValue).Equals("SELECT"))
                    return;

                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitMagazine()
        {
            try
            {
                if (cboBicellType == null || cboBicellType.SelectedValue == null) return;

                Util.gridClear(dgWaitLot);

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("BICELL_LEVEL3_CODE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _ProcID;
                newRow["EQSGID"] = chkOtherLine?.IsChecked != null && (bool)chkOtherLine?.IsChecked ? cboOtherEquipmentSegment.SelectedValue : _LineID;
                newRow["BICELL_LEVEL3_CODE"] = cboBicellType.SelectedValue.ToString().Equals("") && Util.NVC(cboBicellType.Text).IndexOf("ALL") >= 0 ? "ALL" : cboBicellType.SelectedValue.ToString();
                newRow["LOTID"] = txtWaitLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_3D", "INDATA", "OUTDATA", inTable, (bizResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, bizResult, null, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
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

        private void HoldProcess()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1345", (result) =>
            {
                if (result == MessageBoxResult.OK)
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

                        for (int i = 0; i < dgWaitLot.Rows.Count; i++)
                        {
                            if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                            inRow = inLot.NewRow();
                            inRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "LOTID"));
                            inRow["HOLD_NOTE"] = txtRemark.Text;
                            inRow["RESNCODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["HOLD_CODE"] = cboHoldReason.SelectedValue.ToString();
                            inRow["UNHOLD_SCHD_DATE"] = dtExpected.SelectedDateTime.ToString("yyyyMMdd");

                            inLot.Rows.Add(inRow);
                        }

                        if (inLot.Rows.Count < 1)
                            return;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, (bizResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                //Util.AlertInfo("정상 처리 되었습니다.");
                                Util.MessageInfo("SFU1275");
                                GetWaitMagazine();

                                txtRemark.Text = "";
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, inDataSet);
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
            });
        }

        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow newRow = RQSTDT.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["LOTID"] = sLotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_REWRK_NJ", "INDATA", "OUTDATA", RQSTDT);

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
        private bool CanExecute()
        {
            bool bRet = false;

            int iRow = _util.GetDataGridCheckFirstRowIndex(dgWaitLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (cboHoldReason == null || cboHoldReason.SelectedValue == null || cboHoldReason.SelectedValue.ToString().Equals("SELECT"))
            {
                //Util.Alert("HOLD 사유를 선택 하세요.");
                Util.MessageValidation("SFU1342");
                return bRet;
            }

            for (int i = 0; i < dgWaitLot.Rows.Count; i++)
            {
                if (!_util.GetDataGridCheckValue(dgWaitLot, "CHK", i)) continue;

                if (Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[i].DataItem, "WIPHOLD")).Equals("Y"))
                {
                    //Util.Alert("이미 HOLD상태의 LOT이 선택 되었습니다.");
                    Util.MessageValidation("SFU1773");
                    return bRet;
                }
            }

            if (txtRemark.Text.Trim().Equals(""))
            {
                //Util.Alert("LOT HOLD 에 대한 설명을 입력해 주세요.");
                Util.MessageValidation("SFU1214");
                return bRet;
            }

            bRet = true;

            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHoldRelease);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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


        #endregion

        #endregion

        private void btnWaitPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                String sMazID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sMazID.Equals(""))
                {
                    List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    DataTable dtRslt = GetThermalPaperPrintingInfo(sMazID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //라미
                    dicParam.Add("reportName", "Lami"); //dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "MAGAZINE ID");

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "F"); // 재발행 여부.

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    {
                        dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                    }
                    
                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = null;
                        Parameters[1] = _ProcID;
                        Parameters[2] = _LineID;
                        Parameters[3] = _EqptID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.
                        Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(printWait_Closed);

                        print.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void printWait_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_LAMI;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void txtWaitLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    GetWaitMagazine();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetWaitMagazine();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
