/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 두잇 이선규K
   Decription : 전지 5MEGA-GMES 구축 - DSF 공정진척 - 실적확인 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  두잇 이선규K : Initial Created.
  2017.09.18  INS  김동일K : 조립 Prj 에서 활성화 Prj 로 소스코드 이동
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FORM001
{
    /// <summary>
    /// FORM001_052_CONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FORM001_052_CONFIRM : C1Window, IWorkArea
    {   
        #region Declaration & Constructor

        private string _BaseProcess = Process.DSF;

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private bool bSave = false;
        private System.DateTime dtNow;
        private bool bEndSetTime = false;

        private string sCaldate = string.Empty;
        private System.DateTime dtCaldate;

        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;

        private BizDataSet _Biz = new BizDataSet();

        #region Popup 처리 로직 변경

        CMM_SHIFT_USER2 wndShiftUser;

        #endregion

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

        public FORM001_052_CONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            dtNow = System.DateTime.Now;
        }

        private void InitializeDfctDtl()
        {
            DataTable dtTmp = _Biz.GetDA_PRD_SEL_DEFECT_DTL();

            DataRow dtRow = dtTmp.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            dtTmp.Rows.Add(dtRow);

            dgDfctDTL.ItemsSource = DataTableConverter.Convert(dtTmp);
        }

        #endregion

        #region [메인 윈도우 이벤트]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경

            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

            #endregion

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 11)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipSeq = Util.NVC(tmps[3]);
                _WipStat = Util.NVC(tmps[4]);
                _RetShiftName = Util.NVC(tmps[5]);
                _RetShiftCode = Util.NVC(tmps[6]);
                _RetUserName = Util.NVC(tmps[7]);
                _RetUserID = Util.NVC(tmps[8]);
                _RetWrkStrtTime = Util.NVC(tmps[9]);
                _RetWrkEndTime = Util.NVC(tmps[10]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipSeq = "";
                _WipStat = "";
                _RetShiftName = "";
                _RetShiftCode = "";
                _RetUserName = "";
                _RetUserID = "";
                _RetWrkStrtTime = "";
                _RetWrkEndTime = "";
            }

            #region [탭 설정]

            tbDefect.Visibility = Visibility.Collapsed;    // 현재 사용치 않음. 기능만 구현
            tbEqpDefect.Visibility = Visibility.Collapsed; // 현재 사용치 않음. 기능만 구현

            #endregion [탭 설정]

            ApplyPermissions();
            InitializeControls();
            GetAllData();

            bEndSetTime = true;

            txtShift.Text = _RetShiftName;
            txtShift.Tag = _RetShiftCode;
            txtWorker.Text = _RetUserName;
            txtWorker.Tag = _RetUserID;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            InitializeDfctDtl();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;

            string parameterText = string.Empty;
            string messageCode = _WipStat.Equals("PROC") ? "SFU1915" : "SFU2039";

            double dAlphaQty_P = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P")));
            double dAlphaQty_M = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M")));
            double dAlphaQty = dAlphaQty_P + dAlphaQty_M;

            if (dAlphaQty > 0)
            {
                messageCode = "SFU1874";
                parameterText = Math.Abs(dAlphaQty_P).GetString();
            }

            else if (dAlphaQty < 0)
            {
                messageCode = "SFU1571";
                parameterText = Math.Abs(dAlphaQty_M).GetString();
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }

            }, parameterText);

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion [메인 윈도우 이벤트]

        #region [Lot 정보 이벤트

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (sender == null)
                {
                    this.Focus();
                    return;
                }

                LGCDatePicker dtPik = (sender as LGCDatePicker);

                if (sCaldate.Equals(""))
                {
                    this.Focus();
                    return;
                }

                if ((Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) - 1 > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))) ||
                    (Convert.ToDecimal(dtCaldate.ToString("yyyyMMdd")) + 1 < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd"))))
                {
                    dtPik.Text = dtCaldate.ToLongDateString();
                    dtPik.SelectedDateTime = dtCaldate;

                    Util.MessageValidation("SFU1669");  // 선택할 수 없습니다.
                    //e.Handled = false;
                    return;
                }

                this.Focus();
            }));
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            if (wndShiftUser != null)
                wndShiftUser = null;

            wndShiftUser = new CMM_SHIFT_USER2();
            wndShiftUser.FrameOperation = this.FrameOperation;

            if (wndShiftUser != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = Util.NVC(_LineID);
                Parameters[3] = _BaseProcess;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(_EqptID);  //EQPTID 추가
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.

                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShiftUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));
            }
        }

        private void dgDfctDTL_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
            }));
        }

        private void dgDfctDTL_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        #endregion [Lot 정보 이벤트

        #region [탭 - 불량정보 이벤트]

        private void btnDefectSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveDefect())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("불량정보를 저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1587", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetDefect();
                }
            });
        }

        private void dgDefect_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            SumDefectQty();

            if (dgDfctDTL.Rows.Count - dgDfctDTL.TopRows.Count > 0)
            {
                double dInputQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")));
                double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                double dDefectQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                double dOutQty = dGoodQty + dDefectQty;
                double dAlphaQty = dOutQty - dInputQty;

                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dOutQty);

                if (dAlphaQty > 0)
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P", dAlphaQty);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M", 0);
                }
                else
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M", dAlphaQty);
                }
            }

        }

        #endregion [탭 - 불량정보 이벤트]

        #region [탭 - 투입자재 이벤트]
        #endregion [탭 - 투입자재 이벤트]

        #region [탭 - 설비불량 이벤트]

        private void btnEqpDefectSearch_Click(object sender, RoutedEventArgs e)
        {
            GetEqpDefectInfo();
        }

        #endregion [탭 - 설비불량 이벤트]

        #region [BizRule]

        private void GetPkgLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WIP_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _LotID;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CONFIRM_LOT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult != null && searchResult.Rows.Count > 0)
                        {
                            txtLotId.Text = Util.NVC(searchResult.Rows[0]["LOTID"]);
                            txtProdId.Text = Util.NVC(searchResult.Rows[0]["PRODID"]);
                            txtWorkOrder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);
                            txtStartTime.Text = Util.NVC(searchResult.Rows[0]["WIPDTTM_ST"]);
                            txtEndTime.Text = Util.NVC(searchResult.Rows[0]["EQPT_END_DTTM"]);
                            txtRemark.Text = Util.NVC(searchResult.Rows[0]["WIP_NOTE"]);

                            // Caldate Lot의 Caldate로...
                            if (Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).Trim().Equals(""))
                            {
                                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["NOW_CALDATE"])).ToLongDateString();
                                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]));

                                sCaldate = Util.NVC(searchResult.Rows[0]["NOW_CALDATE_YMD"]);
                                dtCaldate = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]));
                            }
                            else
                            {
                                dtpCaldate.Text = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"])).ToLongDateString();
                                dtpCaldate.SelectedDateTime = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]));

                                sCaldate = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"])).ToString("yyyyMMdd");
                                dtCaldate = Convert.ToDateTime(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]));
                            }
                        }
                        else
                        {
                            Util.MessageValidation("SFU1331"); // Data가 존재하지 않습니다.
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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetTrayInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_OUT_LOT_LIST_DSF();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = _BaseProcess;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["TRAYID"] = null;    // 전체 리스트 조회 처리.

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_DSF", "INDATA", "OUTDATA", inTable);
                Util.GridSetData(dgTray, dtRslt, null, true);

                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY", dtRslt.Compute("SUM(CELLQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtRslt.Compute("SUM(CELLQTY)", string.Empty).ToString()));
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", 0);

                HiddenLoadingIndicator();
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

        private void GetInputInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_MTRL_LIST_DSF();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq.Equals("") ? 1 : Convert.ToDecimal(_WipSeq);


                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_MTRL_LIST_DSF", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInput, searchResult, null, true);
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

        private void GetDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _BaseProcess;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", inTable);

                if (searchResult != null)
                {
                    //dgDefect.ItemsSource = DataTableConverter.Convert(searchResult);
                    Util.GridSetData(dgDefect, searchResult, null, true);

                    SumDefectQty();

                    if (dgDfctDTL != null)
                    {
                        if (dgDfctDTL.Rows.Count - dgDfctDTL.TopRows.Count > 0)
                        {
                            double dInputQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY")));
                            double dGoodQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "GOODQTY")));
                            double dDefectQty = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY")));
                            double dOutQty = dGoodQty + dDefectQty;
                            double dAlphaQty = dOutQty - dInputQty;

                            DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "OUTPUTQTY", dOutQty);
                            //DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "INPUTQTY", dOutQty);

                            if (dAlphaQty > 0)
                            {
                                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P", dAlphaQty);
                                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M", 0);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P", 0);
                                DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M", dAlphaQty);
                            }
                        }
                    }
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

        private void GetEqpDefectInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["WIPSEQ"] = _WipSeq;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgEqpDefect, searchResult, null, true);
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

        private void Save()
        {
            try
            {
                // 투입 = 양품 + 불량 같을 경우만 확정 처리 VALIDATION을 위해 VALIDATION 함수내에서 저장 처리.
                // 자동 불량 저장 처리.
                SaveDefectAllBeforeConfirm();

                ShowLoadingIndicator();

                DateTime dtTime;

                dtTime = new DateTime(dtpCaldate.SelectedDateTime.Year, dtpCaldate.SelectedDateTime.Month, dtpCaldate.SelectedDateTime.Day);

                double dAlphQty_P = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_P")));
                double dAlphQty_M = Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "ALPHAQTY_M")));

                DataTable inTable = _Biz.GetBR_PRD_REG_END_LOT_DSF();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = _LotID;
                newRow["INPUTQTY"] = 0;
                newRow["OUTPUTQTY"] = 0;
                newRow["RESNQTY"] = 0;
                newRow["INPUT_DIFF_QTY"] = dAlphQty_P + dAlphQty_M;
                newRow["SHIFT"] = txtShift.Tag.ToString();
                newRow["WIPDTTM_ED"] = dtTime;
                newRow["WIPNOTE"] = txtRemark.Text.Trim();
                newRow["WRK_USERID"] = txtWorker.Tag;
                newRow["WRK_USER_NAME"] = txtWorker.Text.Trim();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_END_LOT_DSF", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        btnDefectSave.IsEnabled = false;
                        btnSave.IsEnabled = false;

                        Util.MessageInfo("SFU1275"); // 정상 처리 되었습니다.

                        bSave = true;

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
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetDefect(bool bMsgShow = true)
        {
            try
            {
                if (dgDefect == null || dgDefect.Rows.Count < 1)
                    return;

                if (dgDefect.Rows.Count - dgDefect.FrozenTopRowsCount - dgDefect.FrozenBottomRowsCount < 1)
                    return;

                ShowLoadingIndicator();

                dgDefect.EndEdit();

                DataRow newRow = null;
                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();

                DataTable inTable = indataSet.Tables["INDATA"];

                newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;

                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = txtLotId.Text.Trim();
                    newRow["WIPSEQ"] = _WipSeq;
                    newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID"));
                    newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNCODE"));
                    newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "RESNQTY")));
                    newRow["RESNCODE_CAUSE"] = "";
                    newRow["PROCID_CAUSE"] = "";
                    newRow["RESNNOTE"] = "";
                    newRow["DFCT_TAG_QTY"] = 0;
                    newRow["LANE_QTY"] = 1;
                    newRow["LANE_PTN_QTY"] = 1;

                    if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                    {
                        newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[i].DataItem, "COST_CNTR_ID"));
                    }
                    else
                    {
                        newRow["COST_CNTR_ID"] = "";
                    }

                    newRow["A_TYPE_DFCT_QTY"] = 0;
                    newRow["C_TYPE_DFCT_QTY"] = 0;

                    inDEFECT_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                if (bMsgShow)
                    Util.MessageInfo("SFU1275"); // 정상 처리 되었습니다.

                GetDefectInfo();
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

        private void SaveDefectAllBeforeConfirm()
        {
            try
            {
                SetDefect(false);
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region [Validation]

        private bool CanSaveDefect()
        {
            bool bRet = false;

            if (dgDefect.ItemsSource == null || dgDefect.Rows.Count < 1)
            {
                Util.MessageValidation("SFU1578"); // 불량 항목이 없습니다.
                return bRet;
            }

            if (txtLotId.Text.Trim().Length < 1)
            {
                Util.MessageValidation("SFU1195"); // LOT 정보가 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;

            if (txtLotId.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1364"); // LOT ID가 선택되지 않았습니다."
                return bRet;
            }

            if (txtShift.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1844"); // 작업조를 선택 하세요.
                return bRet;
            }

            if (txtWorker.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1842"); // 작업자를 선택 하세요.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [PopUp Event]

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;

            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                txtShift.Text = Util.NVC(wndPopup.SHIFTNAME);
                txtShift.Tag = Util.NVC(wndPopup.SHIFTCODE);
                txtWorker.Text = Util.NVC(wndPopup.USERNAME);
                txtWorker.Tag = Util.NVC(wndPopup.USERID);
                _RetWrkStrtTime = Util.NVC(wndPopup.WRKSTRTTIME);
                _RetWrkEndTime = Util.NVC(wndPopup.WRKENDTTIME);
                _RetShiftCode = Util.NVC(wndPopup.SHIFTCODE);
                _RetShiftName = Util.NVC(wndPopup.SHIFTNAME);
                _RetUserID = Util.NVC(wndPopup.USERID);
                _RetUserName = Util.NVC(wndPopup.USERNAME);
            }
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            listAuth.Add(btnDefectSave);

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

        private void ClearControls()
        {
            Util.gridClear(dgDfctDTL);
            Util.gridClear(dgTray);
            Util.gridClear(dgInput);
            Util.gridClear(dgDefect);
            Util.gridClear(dgEqpDefect);
        }

        private void GetAllData()
        {
            ClearControls();

            InitializeDfctDtl();
            GetPkgLotInfo();
            GetTrayInfo();
            GetInputInfo();
            GetDefectInfo();
            GetEqpDefectInfo();
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(dgDefect.ItemsSource);

                if (dtTmp != null && dtTmp.Rows.Count > 0)
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").ToString()));
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).ToString()));
                }
                else
                {
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(dgDfctDTL.Rows[dgDfctDTL.TopRows.Count].DataItem, "DEFECTQTY", 0);
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
