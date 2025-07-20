/*************************************************************************************
 Created Date : 2016.09.30
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 노칭 공정진척 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.30  INS 김동일K : Initial Created.
  2019.02.25  INS 오화백K : RF_ID 작업시작 추가.
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_001_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_RUNSTART : C1Window, IWorkArea
    {        
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoID = string.Empty;
        private string _WoDetail = string.Empty;
        private string _ElecType = string.Empty;
        private string _RfID = string.Empty;
        private bool bSave = false;
        
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
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

        public ASSY001_001_RUNSTART()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            //ldpDatePicker.Text = System.DateTime.Now.ToLongDateString();
            //teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ElecType = Util.NVC(tmps[2]);
                // 2019-02-25 RF_ID 파라미터 추가 by 오화백
                _RfID = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ElecType = "";
                // 2019-02-25 RF_ID 파라미터 추가 by 오화백
                _RfID = "";
            }
            ApplyPermissions();
            InitializeControls();
            GetEIOInfo();

            // 투입위치 코드
            CommonCombo _combo = new CommonCombo();
            String[] sFilter2 = { _EqptID, "PROD" };
            _combo.SetCombo(cboMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            //GetEIOInfo();
            //InitCombo();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSave_Clicked(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;


            if (_RfID == "RF_ID")
            {
                ASSY001_001_RF_ID_RUN_START RF_ID_Run = new ASSY001_001_RF_ID_RUN_START();
                RF_ID_Run.FrameOperation = FrameOperation;

                if (RF_ID_Run != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = _LineID.ToString();
                    Parameters[1] = _EqptID.ToString();
                    Parameters[2] = _ElecType.ToString();
                    Parameters[3] = cboMountPstsID.SelectedValue.ToString();
                    Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK")].DataItem, "LOTID"));
                    C1WindowExtension.SetParameters(RF_ID_Run, Parameters);

                    RF_ID_Run.Closed += new EventHandler(RF_ID_Run_Closed);
                    //팝업의 팝업에서 호출
                    foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                    {
                        if (tmp.Name == "grdMain")
                        {
                            tmp.Children.Add(RF_ID_Run);
                            RF_ID_Run.BringToFront();
                            break;
                        }
                    }
                }
            }
            else
            {
                Util.MessageConfirm("SFU1240", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        //string sNewLot = GetNewLotid();
                        string sNewLot = Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK")].DataItem, "LOTID"));
                        if (sNewLot.Equals(""))
                            return;

                        StartRun(sNewLot);
                    }
                });
            }
       }
        //2019.02.25 RF_ID 작업시작 팝업닫기
        private void RF_ID_Run_Closed(object sender, EventArgs e)
        {
            ASSY001_001_RF_ID_RUN_START window = sender as ASSY001_001_RF_ID_RUN_START;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                btnSave.IsEnabled = false;
                bSave = true;

                Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                //GetWaitList();
                this.DialogResult = MessageBoxResult.OK;
            }
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(window);
                    break;
                }
            }
            //this.grdRunStart.Children.Remove(window);
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitList();
        }

        private void dgWaitList_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    txtInputLot.Text = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "LOTID"));

                                    //row 색 바꾸기
                                    dgWaitList.SelectedIndex = e.Cell.Row.Index;
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    txtInputLot.Text = "";
                                }

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                                break;
                        }

                        // Checked Event 사용 불가로 인해 CurrentCellChanged 사용함으로 발생하는 동일 Cell Cheked  후 Unchecked 시 Event 안타는 문제로 처리..
                        dgWaitList.CurrentCell = dgWaitList.GetCell(dgWaitList.CurrentCell.Row.Index, dgWaitList.Columns.Count - 1);
                    }
                }
            }));
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetEIOInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WORKORDER_PLAN_DETAIL_BYEQPTID();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = _EqptID;

                inTable.Rows.Add(searchCondition);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WORKORER_PLAN_DETAIL_BYEQPTID", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (searchResult == null || searchResult.Rows.Count < 1)
                            return;

                        txtWorkorder.Text = Util.NVC(searchResult.Rows[0]["WOID"]);

                        GetWaitList();
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

        private void GetWaitList()
        {
            try
            {
                Util.gridClear(dgWaitList);
                txtInputLot.Text = "";

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LIST_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQSGID"] = _LineID;
                newRow["WOID"] = txtWorkorder.Text;
                newRow["PRDT_CLSS_CODE"] = _ElecType;
                newRow["LOTID"] = txtLot.Text.Trim().Equals("") ? null : txtLot.Text;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgWaitList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitList, searchResult, null, true);

                        if (dgWaitList.Rows.Count > 0)
                        {
                            dgWaitList.CurrentCell = dgWaitList.GetCell(0, dgWaitList.Columns.Count - 1);
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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInputLotValid(out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_IN_LOT_VALID_NT();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_INPUT_LOT_NT_UI", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sRet = Util.NVC(dtResult.Rows[0][0]);
                    sMsg = Util.NVC(dtResult.Rows[0][1]);
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881"; // "존재하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetNewLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_GET_NEW_LOT_NT();

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _EqptID;
                newRow["LOTID_MO"] = Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_LOTID_NT", "INDATA", "OUTDATA", inTable);

                string sNewLot = string.Empty;
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dtResult.Rows[0]["LOTID"]);
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void StartRun(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_START_LOT_NT();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                //newRow["LOTID"] = sNewLot;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                newRow = inMtrlTable.NewRow();
                newRow["INPUT_LOTID"] = sNewLot;
                //newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[_Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK")].DataItem, "PRODID"));
                newRow["EQPT_MOUNT_PSTN_ID"] = cboMountPstsID.SelectedValue.ToString();
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                //newRow["LOT_TYPE_CODE"] = "INPUT";

                inMtrlTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_NT_EIF", "IN_EQP,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        btnSave.IsEnabled = false;
                        bSave = true;

                        Util.MessageInfo("SFU1275");//정상 처리 되었습니다.

                        //GetWaitList();
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
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]
        private bool CanStartRun()
        {
            bool bRet = false;

            if (txtWorkorder.Text.Trim().Equals(""))
            {
                //Util.Alert("해당 설비에 선택된 작업지시가 없습니다.");
                Util.MessageValidation("SFU2018");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgWaitList, "CHK");

            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgWaitList.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
            {
                //Util.Alert("대기 LOT이 아닙니다.\n대기 LOT을 선택해 주세요.");
                Util.MessageValidation("SFU1494");
                return bRet;
            }

            if (cboMountPstsID.SelectedValue == null || cboMountPstsID.SelectedValue.ToString().Equals(""))
            {
                //Util.Alert("투입위치를 선택하세요.");
                Util.MessageValidation("SFU1981");
                return bRet;
            }

            // 착공시 validation은 투입 biz에서 처리 하므로 주석.
            //string sRet = string.Empty;
            //string sMsg = string.Empty;
            //GetInputLotValid(out sRet, out sMsg);

            //if (sRet.Equals("NG"))
            //{
            //    Util.Alert(sMsg);
            //    return bRet;
            //}

            bRet = true;

            return bRet;
        }
        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

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


        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
