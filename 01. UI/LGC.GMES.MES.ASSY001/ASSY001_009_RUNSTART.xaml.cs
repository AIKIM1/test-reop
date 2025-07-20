/*************************************************************************************
 Created Date : 2017.02.08
      Creator : INS 정문교C
   Decription : 전지 5MEGA-GMES 구축 - STP 공정진척 화면 - 착공 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.08  INS 정문교C : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_009_RUNSTART : C1Window, IWorkArea
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
        public ASSY001_009_RUNSTART()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
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
            lblProd.Text = string.Empty;

            ApplyPermissions();

            ClearDataGrid();

            GetEqptInfo();
            GetInputMountInfo();

            ////dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;
            dgInput.CommittedEdit += dgInput_CommittedEdit;
            dgMGZ.CommittingEdit += dgMGZ_CommittingEdit;
            dgMGZ.CommittedEdit += dgMGZ_CommittedEdit;

        }
        #endregion

        #region [작업시작, 닫기]
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

                    ////txtLot.Text = sNewLot;
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
        #endregion

        #region [dgInput 그리드]
        private void dgInput_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgInput.ItemsSource);

            string sMountPstnid = dt.Rows[e.Cell.Row.Index]["EQPT_MOUNT_PSTN_ID"].ToString();
            string sClssCode = dt.Rows[e.Cell.Row.Index]["BICELL_LEVEL3_CODE"].ToString();

            foreach (DataRow row in dt.Rows)
            {
                if (row["EQPT_MOUNT_PSTN_ID"].Equals(sMountPstnid))
                    continue;
                else
                    row["CHK"] = "0";
            }

            dt.AcceptChanges();
            Util.GridSetData(dgInput, dt, null, false);

            GetWaitMazList(dgMGZ, sClssCode);

        }
  
        #endregion

        #region [투입 LOT]
        private void txtMTRL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    dgInput.CommittedEdit -= dgInput_CommittedEdit;

                    if (txtMTRL.Text.Trim().Equals(""))
                        return;

                    int iChkRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
                    int iSelRow = -1;

                    if (iChkRow < 0)
                    {
                        //Util.Alert("투입위치를 선택하세요.");
                        Util.MessageValidation("SFU1981");
                        return;
                    }
                    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iChkRow].DataItem, "INPUT_LOTID")).Trim().Equals(""))
                    {
                        //Util.Alert("해당 위치는 이미 투입 정보가 존재하여 투입할 수 없습니다.");
                        Util.MessageValidation("SFU2021");
                        return;
                    }

                    if (_Util.GetDataGridRowIndex(dgInput, "INPUT_LOTID", txtMTRL.Text) >= 0)
                    {
                        //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                        Util.MessageValidation("SFU1967");
                        return;
                    }

                    if (_Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", txtMTRL.Text) >= 0)
                    {
                        //Util.Alert("선택한 LOT에 동일한 LOT이 있습니다.");
                        Util.MessageValidation("SFU1657");
                        return;
                    }

                    // 매거진 Row 
                    iSelRow = _Util.GetDataGridRowIndex(dgMGZ, "LOTID", txtMTRL.Text);

                    if (iSelRow < 0)
                    {
                        //Util.Alert("선택된 LOT 이 존재하지 않습니다.");
                        Util.MessageValidation("SFU1137");
                        return;
                    }

                    AddInMaz((dgMGZ.Rows[iSelRow].DataItem as DataRowView).Row, iChkRow);
                    txtMTRL.Text = string.Empty;

                    DataTableConverter.SetValue(dgInput.Rows[iSelRow].DataItem, "CHK", false);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    dgInput.CommittedEdit += dgInput_CommittedEdit;
                }

            }
        }
        #endregion

        #region [dgMGZ 그리드]
        private void dgMGZ_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            try
            {
                dgInput.CommittedEdit -= dgInput_CommittedEdit;

                int idx = _Util.GetDataGridFirstRowIndexByCheck(dgInput, "CHK");

                if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(1))
                {
                    if (idx < 0)
                    {
                        //Util.Alert("투입 위치를 선택하세요.");
                        Util.MessageValidation("SFU1957");
                        DataTableConverter.SetValue(dgMGZ.Rows[dgMGZ.CurrentRow.Index].DataItem, "CHK", false);
                        dgMGZ.SelectedIndex = -1;
                        return;
                    }

                    if (CanAddMagin(dgMGZ, e.Row.Index))
                    {
                        AddInMaz((dgMGZ.Rows[e.Row.Index].DataItem as DataRowView).Row, idx);
                    }
                }
                else
                {
                    RemoveInMaz((dgMGZ.Rows[e.Row.Index].DataItem as DataRowView).Row);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgInput.CommittedEdit += dgInput_CommittedEdit;
            }

        }
        private void dgMGZ_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            //SetSelectMGZ(false);
        }

        #endregion

        #endregion

        #region Mehod

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
                    //if (_WoDetail.Equals(""))
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
        /// <summary>
        /// 투입정보 조회 Biz
        /// </summary>
        private void GetInputMountInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQPT_MOUNT_INFO_STP();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;
                newRow["MOUNT_MTRL_TYPE_CODE"] = null;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO_STP", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgInput, searchResult, null);

                        if (dgInput.CurrentCell != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                        else if (dgInput.Rows.Count > 0)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);

                        //dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;
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

        /// <summary>
        /// 선택 W/O의 대기 매거진 정보 조회 Biz
        /// </summary>
        private void GetWaitMazList(C1.WPF.DataGrid.C1DataGrid datagrid, string sClssCode)
        {
            try
            {
                // 같은 W/O 이고 대기 매거진이 있다면 
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_WAIT_MAG_STP();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.STP;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["PRODID"] = null;
                newRow["LOTID"] = null; //BI-CELL
                newRow["BICELL_LEVEL3_CODE"] = sClssCode;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_MAG_STP", "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(datagrid, bizResult, null);

                        if (datagrid.CurrentCell != null)
                            datagrid.CurrentCell = datagrid.GetCell(datagrid.CurrentCell.Row.Index, datagrid.Columns.Count - 1);
                        else if (datagrid.Rows.Count > 0 && datagrid.GetCell(datagrid.Rows.Count, datagrid.Columns.Count - 1) != null)
                            datagrid.CurrentCell = datagrid.GetCell(datagrid.Rows.Count, datagrid.Columns.Count - 1);

                        SetSelectMGZ(true);
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
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetNewLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_GET_NEW_PROD_LOTID_STP();

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

                ////DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_FD", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_STP", "IN_EQP,IN_INPUT", "OUT_LOT", indataSet);

                string sNewLot = string.Empty;
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUT_LOT"] != null && dsResult.Tables["OUT_LOT"].Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dsResult.Tables["OUT_LOT"].Rows[0]["PROD_LOTID"]);
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
        /// <summary>
        /// 작업시작 Biz
        /// </summary>
        private void RunStart(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();

                dgInput.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_START_PROD_LOT_STP();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = sNewLot;
                newRow["LANGID"] = LoginInfo.LANGID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        newRow = input_LOT.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID"));

                        input_LOT.Rows.Add(newRow);
                    }
                    else
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
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_PROD_LOT_STP", "IN_EQP,IN_INPUT", null, indataSet);

                dgInput.IsReadOnly = true;
                btnOK.IsEnabled = false;
                bSave = true;

                NEW_PROD_LOT = sNewLot;

                //tbSplash.Text = "[" + sNewLot + "] LOT이 생성 되었습니다.";
                tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", sNewLot);
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

        #region [Validation]

        /// <summary>
        /// 작업시작시 체크
        /// </summary>
        private bool CanRun()
        {
            bool bRet = false;

            // 착공시 수량 관련 validation 모두 주석. (설비 실제 투입 위치랑 제품에 투입되어야 되는 갯수가 다를 수 있으므로..)
            //if (int.Parse(txtWaitC.Text) != int.Parse(txtSelC.Text))
            //{
            //    Util.Alert("CType 매거진이 모두 선택되지 않았습니다.");
            //    return bRet;
            //}

            //if (int.Parse(txtWaitA.Text) != int.Parse(txtSelA.Text))
            //{
            //    Util.Alert("AType 매거진이 모두 선택되지 않았습니다.");
            //    return bRet;
            //}

            ////if (_Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK") < 0)
            ////{
            ////    Util.Alert("기준이 될 CType 매거진을 하나 선택 하세요.");
            ////    return bRet;
            ////}
            ////else
            ////{
            ////    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[_Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK")].DataItem, "MAG_TYPE")).Equals("C"))
            ////    {
            ////        Util.Alert("기준이 될 CType 매거진을 하나 선택 하세요.");
            ////        return bRet;
            ////    }
            ////}

            ////for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
            ////{
            ////    if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID")).Equals(""))
            ////    {
            ////        Util.Alert("투입 위치를 모두 입력 하세요.");
            ////        return bRet;
            ////    }
            ////}

            //for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
            //{
            //    string inLot = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID"));
            //    if (inLot.Equals(""))
            //    {
            //        if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
            //        {
            //            Util.Alert("{0}이 입력되지 않았습니다.", Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));
            //            return bRet;
            //        }
            //    }
            //}


            ////if (!MagazineValidation())
            ////    return bRet;

            bRet = true;
            return bRet;
        }

        /// <summary>
        /// dgInput에 대기매거진 추가 체크
        /// </summary>
        private bool CanAddMagin(C1.WPF.DataGrid.C1DataGrid dgReady, int iRedSelRow)
        {
            bool bRet = false;

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

                if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(sTmpLot))
                {
                    //Util.Alert("선택한 LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1657");
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
            listAuth.Add(btnOK);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 그리드 Clear
        /// </summary>
        private void ClearDataGrid()
        {
            Util.gridClear(dgInput);
            Util.gridClear(dgMGZ);
        }

        /// <summary>
        /// dgInput 그리드에 대기 매거진 Setting
        /// </summary>
        private void AddInMaz(DataRow addRow, int iInputRow)
        {
            try
            {
                if (iInputRow < 0)
                    return;

                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

                ////if (!dtTmp.Columns.Contains("PRDT_CLSS_CODE"))
                ////    dtTmp.Columns.Add("PRDT_CLSS_CODE", typeof(string));
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
                        if (dtTmp.Columns[i].ColumnName.Equals("PRDT_CLSS_CODE"))
                            continue;

                        if (dtTmp.Columns[i].ColumnName.Equals(addRow.Table.Columns[j].ColumnName))
                        {
                            if (dtTmp.Columns[i].ColumnName.Equals("WOID") || dtTmp.Columns[i].ColumnName.Equals("WO_DETL_ID"))
                                continue;

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
                        else if (dtTmp.Columns[i].ColumnName.Equals("SEL_LOTID") && addRow.Table.Columns[j].ColumnName.Equals("LOTID"))
                        {
                            dtTmp.Rows[iInputRow][i] = Util.NVC(addRow[j]);
                        }

                    }
                }

                dtTmp.AcceptChanges();
                dgInput.BeginEdit();
                dgInput.ItemsSource = DataTableConverter.Convert(dtTmp);
                dgInput.EndEdit();

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

        /// <summary>
        /// dgInput 그리드에 대기 매거진 Setting 취소
        /// </summary>
        private void RemoveInMaz(DataRow removeRow)
        {
            try
            {
                int idx = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", Util.NVC(removeRow["LOTID"]));
                if (idx < 0)
                    return;

                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "SEL_LOTID", "");
                ////DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRDT_CLSS_CODE", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PR_LOTID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "WIPQTY", 0);
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODID", "");
                DataTableConverter.SetValue(dgInput.Rows[idx].DataItem, "PRODNAME", "");

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

        /// <summary>
        /// dgInput 그리드 W/On변경시 대기 매거진 조회후 Setting된 대기 매거진 체크
        /// </summary>
        private void SetSelectMGZ(bool bSearch)
        {
            // 선택 되어진 대기 메거진 표시 
            if (bSearch)
            {
                // 조회후 체크
                for (int nrow = 0; nrow < dgInput.Rows.Count; nrow++)
                {
                    int idx = _Util.GetDataGridRowIndex(dgMGZ, "LOTID", DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SEL_LOTID").ToString());

                    if (idx > -1)
                    {
                        DataTableConverter.SetValue(dgMGZ.Rows[idx].DataItem, "CHK", 1);
                    }
                }
            }
            else
            {
                // 그리드 Click시 
                for (int nrow = 0; nrow < dgMGZ.Rows.Count; nrow++)
                {
                    if (DataTableConverter.GetValue(dgMGZ.Rows[nrow].DataItem, "CHK").Equals(1))
                    {
                        int idx = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", DataTableConverter.GetValue(dgMGZ.Rows[nrow].DataItem, "LOTID").ToString());

                        if (idx < 0)
                        {
                            DataTableConverter.SetValue(dgMGZ.Rows[nrow].DataItem, "CHK", 0);
                        }
                    }
                }
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
            //Makes the thread wait for 5s before exiting.
            Thread.Sleep(2000);
        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #endregion

    }
}
