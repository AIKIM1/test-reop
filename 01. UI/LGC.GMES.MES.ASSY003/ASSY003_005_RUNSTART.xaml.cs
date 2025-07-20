/*************************************************************************************
 Created Date : 2017.06.15
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Folding 공정진척 화면 - 착공 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.06.15  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_005_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_005_RUNSTART : C1Window, IWorkArea
    {   
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private bool bSave = false;

        public string NEW_PROD_LOT = string.Empty;

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();
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
        public ASSY003_005_RUNSTART()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            GetEqptInfo();

            ClearDataGrid();
            
            GetInputMountInfo();
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter3 = { "BICELL_TYPE_FD" };
            _combo.SetCombo(cboCellType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "COMMCODE");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRun())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업시작 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sNewLot = GetNewLotId();
                    if (sNewLot.Equals(""))
                        return;

                    txtLot.Text = sNewLot;
                    RunStart(sNewLot);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboProd_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            ClearDataGrid();
        }

        private void dgWait_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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
                                    int iPutRow = -1;

                                    if (CanAddMagin(dg, e.Cell.Row.Index, out iPutRow))
                                    {

                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                        chk.IsChecked = true;

                                        AddInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row, iPutRow);
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    RemoveInMaz((dg.Rows[e.Cell.Row.Index].DataItem as DataRowView).Row);
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }
            }));
        }

        private void dgInput_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                            dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                            (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                            (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void txtMTRL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtMTRL.Text.Trim().Equals(""))
                    return;

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("투입위치를 선택하세요.");
                    Util.MessageValidation("SFU1981");
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    //Util.Alert("반제품 투입위치는 대기 매거진에서 선택하여 투입하세요.");
                    Util.MessageValidation("SFU1544");
                    return;
                }

                if (_Util.GetDataGridRowIndex(dgInput, "INPUT_LOTID", txtMTRL.Text) >= 0)
                {
                    //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1967");
                    return;
                }
                
                if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "INPUT_LOTID")).Trim().Equals(""))
                {
                    //Util.Alert("해당 위치는 이미 투입 정보가 존재하여 투입할 수 없습니다.");
                    Util.MessageValidation("SFU2021");
                    return;
                }

                DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "INPUT_LOTID", txtMTRL.Text.Trim());

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                if (dgInput.Columns.Contains("PRDT_CLSS_CODE") && dtTmp.Columns.Contains("PRDT_CLSS_CODE"))
                    DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "PRDT_CLSS_CODE", "");
                if (dgInput.Columns.Contains("PR_LOTID") && dtTmp.Columns.Contains("PR_LOTID"))
                    DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "PR_LOTID", "");
                if (dgInput.Columns.Contains("WIPQTY") && dtTmp.Columns.Contains("WIPQTY"))
                    DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "WIPQTY", 0);
                if (dgInput.Columns.Contains("PRODID") && dtTmp.Columns.Contains("PRODID"))
                    DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "PRODID", "");
                if (dgInput.Columns.Contains("PRODNAME") && dtTmp.Columns.Contains("PRODNAME"))
                    DataTableConverter.SetValue(dgInput.Rows[iRow].DataItem, "PRODNAME", "");
            }
        }

        #endregion

        #region Mehod

        #region [Validation]
        private bool CanRun()
        {
            bool bRet = false;

            bRet = true;
            return bRet;
        }

        private bool CanAddMagin(C1.WPF.DataGrid.C1DataGrid dgReady, int iRedSelRow, out int iPutRow)
        {
            bool bRet = false;

            iPutRow = -1;

            string sTmpLot = Util.NVC(DataTableConverter.GetValue(dgReady.Rows[iRedSelRow].DataItem, "LOTID"));
            // 투입LOT 중복 체크
            for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(sTmpLot))
                {
                    //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1967");
                    return bRet;
                }
                
                // 투입 위치가 제품 이고 투입 가능 타입과 동일한 Row
                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                {
                    // 투입LOT이 없고 선택 Lot이 없는 Row.
                    if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                    {
                        if (iPutRow < 0) iPutRow = i;
                    }
                }
            }

            if (iPutRow < 0)
            {
                //Util.Alert("더이상 투입할 수 없습니다.");
                Util.MessageValidation("SFU1222");
                return bRet;
            }


            // 최상위 라미랏과 비교..
            string sTopLamiLot = GetTopLamiLot(dgReady, iRedSelRow);
            string sLamiLot = Util.NVC(DataTableConverter.GetValue(dgReady.Rows[iRedSelRow].DataItem, "PR_LOTID"));
            if (sTopLamiLot != sLamiLot)
            {
                //Util.Alert("최상단의 LAMI LOT 과 다른 LOT 입니다.");
                Util.MessageValidation("SFU1242");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [BizCall]

        private void GetEqptInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    txtWODetail.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);
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

        private void GetInputMountInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInput.CurrentCellChanged -= dgInput_CurrentCellChanged;
                        //dgInput.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInput, searchResult, null, false);
                        dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;
                        
                        if (dgInput.CurrentCell != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                        else if (dgInput.Rows.Count > 0)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
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

        private void GetWaitMazList()
        {
            try
            {
                ShowLoadingIndicator();

                string sType = string.Empty;
                if (cboCellType != null && cboCellType.SelectedValue.Equals("") || cboCellType.SelectedValue.Equals("SELECT"))
                    sType = "";
                else
                    sType = cboCellType.SelectedValue.ToString();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["PROCID"] = Process.STACKING_FOLDING;
                newRow["PRODUCT_LEVEL2_CODE"] = null; //BI-CELL
                newRow["PRODUCT_LEVEL3_CODE"] = sType;
                newRow["WOID"] = txtWorkorder.Text;
                //newRow["PRODID"] = cboProd.SelectedValue.ToString();


                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_MAG_FD_S", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                                                
                        dgWait.CurrentCellChanged -= dgWait_CurrentCellChanged;
                        
                        // 이미 투입된 Data Check 처리.
                        DataTable dt = DataTableConverter.Convert(dgInput.ItemsSource);

                        foreach (DataRow row in dt.Rows)
                        {
                            string sInputLot = dt.Columns.Contains("INPUT_LOTID") ? Util.NVC(row["INPUT_LOTID"]) : "";

                            DataRow[] dr = bizResult.Select("LOTID = '" + sInputLot + "'");

                            foreach (DataRow tmpRow in dr)
                            {
                                tmpRow["CHK"] = "1";
                            }
                        }

                        bizResult.AcceptChanges();

                        Util.GridSetData(dgWait, bizResult, null, false);

                        dgWait.CurrentCellChanged += dgWait_CurrentCellChanged;

                        if (dgWait.CurrentCell != null)
                            dgWait.CurrentCell = dgWait.GetCell(dgWait.CurrentCell.Row.Index, dgWait.Columns.Count - 1);
                        else if (dgWait.Rows.Count > 0 && dgWait.GetCell(dgWait.Rows.Count, dgWait.Columns.Count - 1) != null)
                            dgWait.CurrentCell = dgWait.GetCell(dgWait.Rows.Count, dgWait.Columns.Count - 1);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
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

        private string GetNewLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_GET_NEW_LOT_FD();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                //newRow["NEXTDAY"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                //DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                //newRow = input_LOT.NewRow();
                //newRow["EQPT_MOUNT_PSTN_ID"] = "";
                //newRow["EQPT_MOUNT_PSTN_STATE"] = "";
                //newRow["INPUT_LOTID"] = "";

                //input_LOT.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_FD_S", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);
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

        private void RunStart(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();

                dgInput.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_LOTSTART_FD();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["WO_DETL_ID"] = null;
                newRow["PROD_LOTID"] = sNewLot;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                    {
                        newRow = input_LOT.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID"));

                        input_LOT.Rows.Add(newRow);
                    }
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_PROD_LOT_FD_S", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);


                dgInput.IsReadOnly = true;
                dgWait.IsReadOnly = true;
                btnOK.IsEnabled = false;
                bSave = true;

                NEW_PROD_LOT = sNewLot;

                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", sNewLot); // [%1] LOT이 생성 되었습니다.
                grdMsg.Visibility = Visibility.Visible;

                AsynchronousClose();

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

        #endregion

        #region [PopUp Event]

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
            listAuth.Add(btnOK);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private string GetTopLamiLot(C1.WPF.DataGrid.C1DataGrid datagrid, int rowIdx)
        {
            string sRet = "";
            for (int i = 0; i < rowIdx; i++)
            {
                if (!_Util.GetDataGridCheckValue(datagrid, "CHK", i))
                {
                    sRet = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[i].DataItem, "PR_LOTID"));
                    break;
                }
            }

            if (sRet.Equals(""))
                sRet = Util.NVC(DataTableConverter.GetValue(datagrid.Rows[rowIdx].DataItem, "PR_LOTID"));

            return sRet;
        }

        private void ClearDataGrid()
        {
            Util.gridClear(dgWait);
            Util.gridClear(dgInput);
        }

        private void AddInMaz(DataRow addRow, int iInputRow)
        {
            try
            {
                if (iInputRow < 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                if (!dtTmp.Columns.Contains("PRDT_CLSS_CODE"))
                    dtTmp.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                if (!dtTmp.Columns.Contains("PR_LOTID"))
                    dtTmp.Columns.Add("PR_LOTID", typeof(string));
                if (!dtTmp.Columns.Contains("PRODID"))
                    dtTmp.Columns.Add("PRODID", typeof(string));
                if (!dtTmp.Columns.Contains("PRODNAME"))
                    dtTmp.Columns.Add("PRODNAME", typeof(string));
                if (!dtTmp.Columns.Contains("WIPQTY"))
                    dtTmp.Columns.Add("WIPQTY", typeof(int));

                for (int i = 0; i < dtTmp.Columns.Count; i++)
                {
                    for (int j = 0; j < addRow.Table.Columns.Count; j++)
                    {
                        if (dtTmp.Columns[i].ColumnName.Equals(addRow.Table.Columns[j].ColumnName))
                        {
                            if (addRow[j].GetType() == typeof(string))
                            {
                                dtTmp.Rows[iInputRow][i] = Util.NVC(addRow[j]);
                            }
                            else if (addRow.Table.Columns[j].ColumnName.Equals("CHK"))
                            {
                                dtTmp.Rows[iInputRow][i] = false;
                            }
                            else
                            {
                                dtTmp.Rows[iInputRow][i] = addRow[j];
                            }
                        }
                        else if (dtTmp.Columns[i].ColumnName.Equals("INPUT_LOTID") && addRow.Table.Columns[j].ColumnName.Equals("LOTID"))
                        {
                            dtTmp.Rows[iInputRow][i] = Util.NVC(addRow[j]);
                        }
                    }
                }

                dtTmp.AcceptChanges();
                dgInput.BeginEdit();
                dgInput.ItemsSource = DataTableConverter.Convert(dtTmp);
                dgInput.EndEdit();

                dgInput.ScrollIntoView(iInputRow, dgInput.Columns["INPUT_LOTID"].Index);

                //if (sType.Equals("AT"))
                //    txtSelA.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();
                //else
                //    txtSelC.Text = dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'") == null ? "0" : dtTmp.Select("PRDT_CLSS_CODE = '" + sType + "'").Length.ToString();

                if (dgInput.CurrentCell != null)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                else if (dgInput.Rows.Count > 0)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void RemoveInMaz(DataRow removeRow)
        {
            try
            {
                int idx = _Util.GetDataGridRowIndex(dgInput, "INPUT_LOTID", Util.NVC(removeRow["LOTID"]));
                if (idx < 0)
                    return;

                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "INPUT_LOTID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRDT_CLSS_CODE", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PR_LOTID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "WIPQTY", 0);
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODNAME", "");

                dgInput.ScrollIntoView(idx, dgInput.Columns["INPUT_LOTID"].Index);

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                if (dtTmp == null || dtTmp.Rows.Count <= 0)
                    return;

                if (dgInput.CurrentCell != null)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                else if (dgInput.Rows.Count > 0)
                    dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #endregion

        private void cboCellType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                GetWaitMazList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
                
    }
}
