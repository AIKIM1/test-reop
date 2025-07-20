/*************************************************************************************
 Created Date : 2017.06.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - 대기LOT조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.15  INS 김동일K : Initial Created.
  
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
    /// ASSY003_007_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_WAITLOT : C1Window, IWorkArea
    {   
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _processCode = string.Empty;

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

        public ASSY003_007_WAITLOT()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _processCode = Util.NVC(tmps[2]);
            }
            else
            {
                _LineID = "";
            }

            ApplyPermissions();

            CommonCombo _combo = new CommonCombo();

            string[] sFilter2 = { LoginInfo.CFG_AREA_ID, Process.PACKAGING, null, _LineID };
            _combo.SetCombo(cboOtherEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2, sCase: "EQUIPMENTSEGMENT_WITHOUT_SEL_EQSGID");


            GetWaitBox();

            // HOLD 사유 코드
            //CommonCombo _combo = new CommonCombo();

            //HOLD 사유
            string[] sFilter = { "HOLD_LOT" };
            _combo.SetCombo(cboHoldReason, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            if (!CanExecute())
                return;

            HoldProcess();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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

                btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
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
                GetWaitBox();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = chkOtherLine?.IsChecked != null && (bool)chkOtherLine?.IsChecked ? cboOtherEquipmentSegment.SelectedValue : _LineID;
                newRow["LOTID"] = txtWaitLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_BY_LINE_CL_NJ", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitLot.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitLot, searchResult, null, true);

                        if (dgWaitLot.CurrentCell != null)
                            dgWaitLot.CurrentCell = dgWaitLot.GetCell(dgWaitLot.CurrentCell.Row.Index, dgWaitLot.Columns.Count - 1);
                        else if (dgWaitLot.Rows.Count > 0 && dgWaitLot.GetCell(dgWaitLot.Rows.Count, dgWaitLot.Columns.Count - 1) != null)
                            dgWaitLot.CurrentCell = dgWaitLot.GetCell(dgWaitLot.Rows.Count, dgWaitLot.Columns.Count - 1);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void HoldProcess()
        {
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("HOLD 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1345", result =>
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
                        {
                            HiddenLoadingIndicator();
                            return;
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, (bizResult, ex1) =>
                        {
                            try
                            {
                                if (ex1 != null)
                                {
                                    Util.MessageException(ex1);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                GetWaitBox();

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
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            });
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
            listAuth.Add(btnExecute);

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

        private void txtWaitLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    GetWaitBox();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnWaitBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                String sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

                if (!sBoxID.Equals(""))
                {
                    //List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                    // 발행..
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    Dictionary<string, string> dicParam = new Dictionary<string, string>();

                    //폴딩
                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                    {
                        dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                    }

                    //dicList.Add(dicParam);


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.PACKAGING;
                        Parameters[2] = _LineID;
                        Parameters[3] = _EqptID;
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
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

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO_NJ", "INDATA", "OUTDATA", RQSTDT);
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
    }
}
