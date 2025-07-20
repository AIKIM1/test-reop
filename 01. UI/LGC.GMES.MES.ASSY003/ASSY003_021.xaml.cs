/*************************************************************************************
 Created Date : 2017.12.21
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - C 생산 공정진척 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.21     : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_021.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor

        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;

        private Util _Util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        ASSY003_021_RUNSTART wndRunStart;
        ASSY003_021_CONFIRM wndConfirm;
        ASSY003_021_WAITLOT wndWaitLot;
        ASSY003_TEST_PRINT wndTestPrint;
        CMM_SHIFT_USER2 wndShiftUser;
        ASSY003_OUTLOT_MERGE wndMerge;

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

        public ASSY003_021()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitCombo();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region Popup 처리 로직 변경
            if (wndRunStart != null)
                wndRunStart.BringToFront();

            if (wndConfirm != null)
                wndConfirm.BringToFront();

            if (wndWaitLot != null)
                wndWaitLot.BringToFront();

            if (wndTestPrint != null)
                wndTestPrint.BringToFront();

            if (wndShiftUser != null)
                wndShiftUser.BringToFront();

            if (wndMerge != null)
                wndMerge.BringToFront();
            #endregion

            ApplyPermissions();
        }
        #endregion

        #region Method

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //라인 Combo
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, null, Process.CPROD };
            C1ComboBox[] cboLineChild1 = { cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild1, sCase: "cboEquipmentSegmentAssy", sFilter: sFilter1);

            //설비 Combo
            String[] sFilter2 = { Process.CPROD };
            C1ComboBox[] cboEquipmentParent1 = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent1, sCase: "EQUIPMENT_MAIN_LEVEL", sFilter: sFilter2);

            String[] sFilter3 = { "CPROD_WRK_TYPE_CODE" };
            _combo.SetCombo(cboCPROD_WRK_TYPE_CODE, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreateBOX);
            listAuth.Add(btnDeleteBOX);
            listAuth.Add(btnOutPrint);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #region Button Event
        private void txtCurrInLotID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    // 권한 없으면 Skip.
                    if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                        return;

                    if (!CanCurrAutoInputLot())
                        return;

                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    Util.MessageConfirm("SFU1248", result =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            CProdInput();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CanCurrAutoInputLot()
        {
            bool bRet = false;

            if (txtCurrInLotID.Text.Trim().Equals(""))
            {
                Util.MessageValidation("SFU1379");
                return bRet;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")).Equals(""))
            {
                //Util.Alert("선택한 작업대상 LOT이 없어 투입할 수 없습니다.");
                Util.MessageValidation("SFU1664");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void btnCurrIn_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCurrInComplete())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1248", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CProdInput();
                }
            });
        }

        private void CProdInput()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable2 = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow2 = inTable2.NewRow();
                newRow2["LANGID"] = LoginInfo.LANGID;
                newRow2["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow2["MOUNT_MTRL_TYPE_CODE"] = "PROD"; // 바구니 투입위치만 조회.

                inTable2.Rows.Add(newRow2);

                DataTable dtEqptInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable2);


                DataSet indataSet = _Biz.GetBR_PRD_REG_MTRL_INPUT_FD();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inInputTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(dtEqptInfo.Rows[0]["EQPT_MOUNT_PSTN_ID"]); //MTRL_INPUT
                //newRow["EQPT_MOUNT_PSTN_ID"] = "MTRL_INPUT";
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = txtCurrInLotID.Text.Trim();

                inInputTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_CPROD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot(GetSelectWorkOrderInfo());
                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanCurrInComplete()
        {
            bool bRet = false;

            if (txtCurrInLotID.Text == "")
                return bRet;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (iRow < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void btnCurrInCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanCurrInCancel())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입취소 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                        if (iRow < 0)
                            return;

                        DataTable inTable2 = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                        DataRow Row = inTable2.NewRow();
                        Row["LANGID"] = LoginInfo.LANGID;
                        Row["EQPTID"] = cboEquipment.SelectedValue.ToString();
                        Row["MOUNT_MTRL_TYPE_CODE"] = "PROD"; // 바구니 투입위치만 조회.

                        inTable2.Rows.Add(Row);

                        DataTable dtEqptInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable2);

                        string sEQPT_MOUNT_PSTN_ID = Util.NVC(dtEqptInfo.Rows[0]["EQPT_MOUNT_PSTN_ID"]);

                        DataSet indataSet = new DataSet();

                        DataTable inLot = indataSet.Tables.Add("INDATA");
                        inLot.Columns.Add("SRCTYPE", typeof(string));
                        inLot.Columns.Add("IFMODE", typeof(string));
                        inLot.Columns.Add("EQPTID", typeof(string));
                        inLot.Columns.Add("USERID", typeof(string));
                        inLot.Columns.Add("PROD_LOTID", typeof(string));

                        DataRow newRow2 = inLot.NewRow();

                        newRow2["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        newRow2["IFMODE"] = IFMODE.IFMODE_OFF;
                        newRow2["EQPTID"] = cboEquipment.SelectedValue.ToString();
                        newRow2["USERID"] = LoginInfo.USERID;
                        newRow2["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")); // biz에서 찾음.

                        indataSet.Tables["INDATA"].Rows.Add(newRow2);

                        DataTable inData = indataSet.Tables.Add("IN_INPUT");
                        inData.Columns.Add("WIPNOTE", typeof(string));
                        inData.Columns.Add("INPUT_LOTID", typeof(string));
                        inData.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        inData.Columns.Add("ACTQTY", typeof(int));
                        inData.Columns.Add("INPUT_SEQNO", typeof(Int64));

                        DataRow newRow = null;
                        for (int i = 0; i < dgCurrIn.Rows.Count - dgCurrIn.BottomRows.Count; i++)
                        {
                            if (!_Util.GetDataGridCheckValue(dgCurrIn, "CHK", i)) continue;

                            if (!Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                            {
                                newRow = inData.NewRow();
                                newRow["WIPNOTE"] = "";
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["EQPT_MOUNT_PSTN_ID"] = sEQPT_MOUNT_PSTN_ID;
                                newRow["ACTQTY"] = Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_QTY")));
                                newRow["INPUT_SEQNO"] = Convert.ToInt64(Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[i].DataItem, "INPUT_SEQNO")));

                                //inData.Rows.Add(newRow);
                                indataSet.Tables["IN_INPUT"].Rows.Add(newRow);
                            }
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_LOT_CPROD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                        {
                            try
                            {
                                if (searchException != null)
                                {
                                    Util.MessageException(searchException);
                                    return;
                                }

                                GetProductLot();

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HiddenLoadingIndicator();
                            }
                        }, indataSet
                        );
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void OnCurrInCancel(DataTable inMtrl)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                    return;

                ShowLoadingIndicator();
                DataSet indataSet = _Biz.GetBR_PRD_REG_INPUT_CANCEL_FD();

                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")); // biz에서 찾음.

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                foreach (DataRow sourcerow in inMtrl.Rows)
                {
                    DataRow destRow = inInputTable.NewRow();
                    foreach (DataColumn colname in inInputTable.Columns)
                    {
                        if (sourcerow[colname.ColumnName] != null)
                            destRow[colname.ColumnName] = sourcerow[colname.ColumnName];
                    }
                    inInputTable.Rows.Add(destRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CanCurrInCancel()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCurrIn.Rows[_Util.GetDataGridCheckFirstRowIndex(dgCurrIn, "CHK")].DataItem, "INPUT_LOTID")).Equals(""))
            {
                //Util.Alert("투입 LOT이 없습니다.");
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCurrInComplete())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1248", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CProdCheckInput();
                }
            });
        }

        private void CProdCheckInput()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable2 = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow2 = inTable2.NewRow();
                newRow2["LANGID"] = LoginInfo.LANGID;
                newRow2["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow2["MOUNT_MTRL_TYPE_CODE"] = "PROD"; 

                inTable2.Rows.Add(newRow2);

                DataTable dtEqptInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_INFO", "INDATA", "OUTDATA", inTable2);


                //DataSet indataSet = _Biz.GetBR_PRD_REG_MTRL_INPUT_FD();
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));


                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                //inTable.Rows.Add(newRow);
                indataSet.Tables["INDATA"].Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                inInputTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                inInputTable.Columns.Add("INPUT_LOTID", typeof(string));
                inInputTable.Columns.Add("MTRLID", typeof(string));

                //DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgWaitInput.Rows.Count - dgWaitInput.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgWaitInput, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(dtEqptInfo.Rows[0]["EQPT_MOUNT_PSTN_ID"]); //MTRL_INPUT                                                                
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitInput.Rows[i].DataItem, "LOTID"));
                    newRow["MTRLID"] = Util.NVC(DataTableConverter.GetValue(dgWaitInput.Rows[i].DataItem, "PRODID"));

                    //inInputTable.Rows.Add(newRow);
                    indataSet.Tables["IN_INPUT"].Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_INPUT_LOT_CPROD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //GetProductLot(GetSelectWorkOrderInfo());
                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            GetProductLot();
        }

        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex <= 0)
            {
                //라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            if (cboEquipment.SelectedIndex <= 0)
            {
                //설비를 선택하세요
                Util.MessageValidation("SFU1153");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void btnDeleteBOX_Click(object sender, RoutedEventArgs e)
        {
            if (!CanDeleteBox())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("삭제 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteOutBox();
                }
            });
        }

        private bool CanDeleteBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 작업대상이 없습니다.");
                Util.MessageValidation("SFU1645");
                return bRet;
            }

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));


                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                //inTable.Rows.Add(newRow);
                indataSet.Tables["INDATA"].Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("LOTID", typeof(string));

                //DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));

                    //inInputTable.Rows.Add(newRow);
                    indataSet.Tables["IN_INPUT"].Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_CPROD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool GetErpSendInfo(string sLotID, string sWipSeq)
        {
            try
            {
                ShowLoadingIndicator();

                bool bRet = false;
                DataTable inTable = _Biz.GetDA_PRD_SEL_ERP_SEND_INFO();

                DataRow newRow = inTable.NewRow();

                newRow["LOTID"] = sLotID;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_ERP_SEND", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    // 'S' 가 아닌 경우는 삭제 가능.
                    if (!Util.NVC(dtRslt.Rows[0]["ERP_TRNF_FLAG"]).Equals("S")) // S : ERP 전송 중 , P : ERP 전송 대기, Y : ERP 전송 완료
                    {
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnModifyBox_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveOutBox())
                return;

            Util.MessageConfirm("SFU1241", (result) =>
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("저장 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //OutLotSpclSave();

                    SaveOutBox();
                }
            });
        }

        private bool CanSaveOutBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                string sQty = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY"));
                double dTmp = 0;
                double.TryParse(sQty, out dTmp);
                if (dTmp < 1)
                {
                    //Util.Alert("수량은 0보다 커야 합니다.");
                    Util.MessageValidation("SFU1683");
                    return bRet;
                }


                //string specYN = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALYN"));
                //string SpecDesc = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALDESC"));
                //if (specYN.Equals("Y"))
                //{
                //    if (SpecDesc == "")
                //    {
                //        //Util.Alert("특별관리내역을 입력하세요.");
                //        Util.MessageValidation("SFU1990");
                //        return bRet;
                //    }
                //}
                //else if (specYN.Equals("N"))
                //{
                //    if (SpecDesc != "")
                //    {
                //        //Util.Alert("특별관리내역을 삭제하세요.");
                //        Util.MessageValidation("SFU1989");
                //        return bRet;
                //    }
                //}
            }

            bRet = true;
            return bRet;
        }

        private void OutLotSpclSave()
        {
            try
            {
                ShowLoadingIndicator();

                dgOutProduct.EndEdit();

                DataSet indataSet = new DataSet();

                DataTable inEQP = indataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("PROD_LOTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));

                DataTable inLOT = indataSet.Tables.Add("IN_LOT");
                inLOT.Columns.Add("OUT_LOTID", typeof(string));
                inLOT.Columns.Add("CSTID", typeof(string));
                inLOT.Columns.Add("WIPQTY", typeof(int));
                inLOT.Columns.Add("SPCL_CST_GNRT_FLAG", typeof(string));
                inLOT.Columns.Add("SPCL_CST_NOTE", typeof(string));
                inLOT.Columns.Add("SPCL_CST_RSNCODE", typeof(string));

                DataRow newRow = inEQP.NewRow();

                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inEQP.Rows.Add(newRow);
                newRow = null;

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = inLOT.NewRow();

                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["CSTID"] = null;
                    newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));
                    //newRow["SPCL_CST_GNRT_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALYN"));
                    //newRow["SPCL_CST_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "SPECIALDESC"));
                    newRow["SPCL_CST_RSNCODE"] = "";

                    inLOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UPD_OUT_LOT_SPCL", "IN_EQP,IN_LOT", null, indataSet);
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

        private void SaveOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));


                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                //inTable.Rows.Add(newRow);
                indataSet.Tables["INDATA"].Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables.Add("IN_INPUT");
                inInputTable.Columns.Add("LOTID", typeof(string));
                inInputTable.Columns.Add("WIPQTY_ED", typeof(string));

                //DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID"));
                    newRow["WIPQTY_ED"] = Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")));

                    //inInputTable.Rows.Add(newRow);
                    indataSet.Tables["IN_INPUT"].Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UPD_OUT_LOT_CPROD", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            GetOutProduct();
                            return;
                        }

                        GetProductLot();
                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!CanPrint())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("인쇄 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    BoxIDPrint();
                }
            });
        }

        private void BoxIDPrint(string sBoxID = "", decimal dQty = 0)
        {
            try
            {
                int iCopys = 2;

                if (LoginInfo.CFG_THERMAL_COPIES > 0)
                {
                    iCopys = LoginInfo.CFG_THERMAL_COPIES;
                }

                btnOutPrint.IsEnabled = false;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();

                if (!sBoxID.Equals(""))
                {
                    // 발행..
                    DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                    if (dtRslt == null || dtRslt.Rows.Count < 1)
                        return;

                    // 인계설비 정보.
                    string sEqptName = "";
                    if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") >= 0)
                    {
                        sEqptName = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CPROD_RWK_LOT_EQSGNAME"));
                    }

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
                    dicParam.Add("EQPTNO", sEqptName.Equals("") ? Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]) : sEqptName);
                    dicParam.Add("TITLEX", "BASKET ID");

                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                    dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                    if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                    {
                        dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                    }

                    dicList.Add(dicParam);


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = null;
                        Parameters[1] = Process.CPROD;
                        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboEquipment.SelectedValue.ToString();
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
                        Parameters[5] = "N";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(print, Parameters);

                        print.Closed += new EventHandler(print_Closed);

                        print.ShowModal();
                    }
                }
                else
                {
                    // 인계설비 정보.
                    string sEqptName = "";
                    if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") >= 0)
                    {
                        sEqptName = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CPROD_RWK_LOT_EQSGNAME"));
                    }

                    for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
                    {
                        if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;


                        DataTable dtRslt = GetThermalPaperPrintingInfo(Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "LOTID")));

                        if (dtRslt == null || dtRslt.Rows.Count < 1) continue;


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
                        dicParam.Add("EQPTNO", sEqptName.Equals("") ? Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]) : sEqptName);
                        dicParam.Add("TITLEX", "BASKET ID");

                        dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수

                        dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                        dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));

                        dicParam.Add("RE_PRT_YN", Util.NVC(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "PRINT_YN")).Equals("Y") ? "Y" : "N"); // 재발행 여부.

                        if (LoginInfo.CFG_SHOP_ID.Equals("G182"))
                        {
                            dicParam.Add("MKT_TYPE_CODE", Util.NVC(dtRslt.Rows[0]["MKT_TYPE_CODE"]));
                        }

                        dicList.Add(dicParam);
                    }


                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD();
                    print.FrameOperation = FrameOperation;

                    if (print != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.CPROD;
                        Parameters[2] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[3] = cboEquipment.SelectedValue.ToString();
                        Parameters[4] = sBoxID.Equals("") ? "Y" : "N";   // 완료 메시지 표시 여부.
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
            finally
            {
                btnOutPrint.IsEnabled = true;
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

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_THERMAL_PRT_INFO", "INDATA", "OUTDATA", RQSTDT);
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

        private void print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_FOLD window = sender as CMM_THERMAL_PRINT_FOLD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
            GetOutProduct();
        }

        private bool CanPrint()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgOutProduct, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            for (int i = 0; i < dgOutProduct.Rows.Count - dgOutProduct.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgOutProduct, "CHK", i)) continue;

                if (Util.NVC_Int(DataTableConverter.GetValue(dgOutProduct.Rows[i].DataItem, "WIPQTY")) < 1)
                {
                    // 수량이 없는 반제품은 발행할 수 없습니다.
                    Util.MessageValidation("SFU3510");
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private void btnCreateBOX_Click(object sender, RoutedEventArgs e)
        {            
            if (!CanCreateBox())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOut(GetNewOutLotId());
                }
            });
        }

        private bool CanCreateBox()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK") < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (txtBoxCellQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtBoxCellQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtBoxCellQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                txtBoxCellQty.SelectAll();
                return bRet;
            }            

            bRet = true;
            return bRet;
        }

        private void btnCellResultSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE");
                inTable.Columns.Add("IFMODE");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("PROD_LOTID");
                inTable.Columns.Add("WRK_USERID");
                inTable.Columns.Add("USERID");

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["WRK_USERID"] = LoginInfo.USERID;
                newRow["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(newRow);

                DataTable inLot = indataSet.Tables.Add("IN_WRK");
                inLot.Columns.Add("PRODID", typeof(string));
                inLot.Columns.Add("RECYC_QTY", typeof(Decimal));
                inLot.Columns.Add("SCRP_QTY", typeof(Decimal));
                inLot.Columns.Add("ADD_INPUT_QTY", typeof(Decimal));

                DataRow newRow2 = inLot.NewRow();

                for (int i = 0; i < dgLamiCell.Rows.Count - dgLamiCell.BottomRows.Count; i++)
                {
                    newRow2["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgLamiCell.Rows[i].DataItem, "PRODID"));
                    newRow2["RECYC_QTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLamiCell.Rows[i].DataItem, "RECYC_QTY")));
                    newRow2["SCRP_QTY"] = "0";
                    newRow2["ADD_INPUT_QTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgLamiCell.Rows[i].DataItem, "ADD_INPUT_QTY")));                  

                    indataSet.Tables["IN_WRK"].Rows.Add(newRow2);

                    newRow2 = inLot.NewRow();
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CPROD_WRK_RSLT", "INDATA,IN_WRK", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
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

        private void btnSaveDfct_Click(object sender, RoutedEventArgs e)
        {
            if (dgDefect.ItemsSource == null || dgDefect.Rows.Count < 1)
            {
                //Util.Alert("불량항목이 없습니다.");
                Util.MessageValidation("SFU1588");
                return;
            }
            //불량정보를 저장 하시겠습니까?
            Util.MessageConfirm("SFU1587", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveDfct();
                }
            });
        }

        private void btnWaitLot_Click(object sender, RoutedEventArgs e)
        {
            if (wndWaitLot != null)
                wndWaitLot = null;

            wndWaitLot = new ASSY003_021_WAITLOT();
            wndWaitLot.FrameOperation = FrameOperation;

            if (wndWaitLot != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = Util.GetCondition(cboEquipmentSegment);
                C1WindowExtension.SetParameters(wndWaitLot, Parameters);

                wndWaitLot.Closed += new EventHandler(wndWaitLot_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndWaitLot.ShowModal()));
            }
        }

        private void wndWaitLot_Closed(object sender, EventArgs e)
        {
            wndWaitLot = null;
            ASSY003_021_WAITLOT window = sender as ASSY003_021_WAITLOT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgProductLot.SelectedIndex = idx;

                GetCurrIn();
                GetWaitInput();
                GetOutProduct();
                GetCellResult();
                GetDfctResult();

            }
        }

        private void btnShift_Click(object sender, RoutedEventArgs e)
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
                Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Parameters[3] = Process.CPROD;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = Util.NVC(cboEquipment.SelectedValue);
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(wndShiftUser, Parameters);

                wndShiftUser.Closed += new EventHandler(wndShift_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndShiftUser.ShowModal()));
            }
        }
        #endregion

        #region Grid Search
        public void GetProductLot(bool bSelPrv = true, string sNewLot = "")
        {
            try
            {
                // 다른화면 갔다가 다시 오는 경우.. combobox 등 모두 Reset 되는 문제로 조회 가능 여부 체크...
                if (!CanSearch())
                {
                    HiddenLoadingIndicator();
                    return;
                }

                string sPrvLot = string.Empty;
                if (dgProductLot.ItemsSource != null && dgProductLot.Rows.Count > 0)
                {
                    int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                    if (idx >= 0)
                        sPrvLot = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));

                    // 착공 후 조회 시..
                    if (sNewLot != null && !sNewLot.Equals(""))
                        sPrvLot = sNewLot;
                }

                Clear();

                ShowLoadingIndicator();                

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("PROCID");
                //dt.Columns.Add("EQSGID");
                dt.Columns.Add("CPROD_WRK_TYPE_CODE");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dr["PROCID"] = Process.CPROD;
                //dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                dr["CPROD_WRK_TYPE_CODE"] = Util.GetCondition(cboCPROD_WRK_TYPE_CODE, bAllNull: true);

                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_PROD_LIST", "INDATA", "OUTDATA", dt, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.GridSetData(dgProductLot, result, FrameOperation, false);

                        if (!sPrvLot.Equals("") && bSelPrv)
                        {
                            int idx = _Util.GetDataGridRowIndex(dgProductLot, "LOTID", sPrvLot);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgProductLot.SelectedIndex = idx;

                                ProdListClickedProcess(idx);
                            }
                            else
                            {
                                GetCurrIn();
                                GetWaitInput();
                                //GetOutProduct();
                                //GetCellResult();
                                //GetDfctResult();

                            }
                        }
                        else
                        {
                            if (result.Rows.Count > 0)
                            {
                                int iRowRun = _Util.GetDataGridRowIndex(dgProductLot, "WIPSTAT", "PROC");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (dgProductLot.TopRows.Count > 0)
                                        iRowRun = dgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row 색 바꾸기
                                    dgProductLot.SelectedIndex = iRowRun;

                                    ProdListClickedProcess(iRowRun);
                                }
                            }
                            else
                            {
                                GetCurrIn();
                                GetWaitInput();
                                //GetOutProduct();
                                //GetCellResult();
                                //GetDfctResult();
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ProdListClickedProcess(int iRow)
        {
            if (iRow < 0)
                return;

            Util.gridClear(dgOutProduct);

            if (!_Util.GetDataGridCheckValue(dgProductLot, "CHK", iRow))
            {
                return;
            }

            GetCurrIn();
            GetWaitInput();
            GetOutProduct();
            GetCellResult();
            GetDfctResult();
        }

        private void GetCurrIn()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("LOTID");

                DataRow dr = dt.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INPUT_MTRL_HIST_CPROD", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_WIP_INPUT_MTRL_HIST_CPROD", "INDATA", "OUTDATA", dt, (result, exception) =>
                //{
                //    try
                //    {
                //        if (exception != null)
                //        {
                //            Util.MessageException(exception);
                //            return;
                //        }

                //        Util.GridSetData(dgCurrIn, result, FrameOperation, false);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
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

        private void GetWaitInput()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("CPROD_WRK_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("MKT_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));

                DataRow newRow = RQSTDT.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.CPROD;
                newRow["CPROD_WRK_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "CPROD_WRK_TYPE_CODE"));
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODID"));
                newRow["MKT_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "MKT_TYPE_CODE"));
                newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);

                RQSTDT.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_WAIT_LOT_LIST", "INDATA", "OUTDATA", RQSTDT);

                Util.GridSetData(dgWaitInput, searchResult, FrameOperation);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_WAIT_LOT_LIST", "INDATA", "OUTDATA", RQSTDT, (result, exception) =>
                //{
                //    try
                //    {
                //        if (exception != null)
                //        {
                //            Util.MessageException(exception);
                //            return;
                //        }

                //        Util.GridSetData(dgWaitInput, result, FrameOperation, false);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
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

        private void GetOutProduct()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("PR_LOTID");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("AREAID");

                DataRow dr = dt.NewRow();
                dr["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CPROD;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_OUT_LOT_LIST", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgOutProduct, searchResult, FrameOperation);

                //new ClientProxy().ExecuteService("DA_PRD_SEL_CPROD_OUT_LOT_LIST", "INDATA", "OUTDATA", dt, (result, exception) =>
                //{
                //    try
                //    {
                //        ShowLoadingIndicator();

                //        Util.GridSetData(dgOutProduct, result, FrameOperation, false);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
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

        private void GetCellResult()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("PROD_LOTID");
                dt.Columns.Add("PRODID");
                //dt.Columns.Add("PRODUCT_LEVEL2_CODE");

                DataRow dr = dt.NewRow();
                dr["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                 dr["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "PRODID"));
                //dr["PRODUCT_LEVEL2_CODE"] = "FC,ST";  //구분자 ','
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CPROD_CELL_RSLT", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgLamiCell, searchResult, FrameOperation);


                //new ClientProxy().ExecuteService("DA_BAS_SEL_CPROD_CELL_RSLT", "INDATA", "OUTDATA", dt, (result, exception) =>
                //{
                //    try
                //    {
                //        ShowLoadingIndicator();

                //        Util.GridSetData(dgLamiCell, result, FrameOperation, false);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
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

        private void GetDfctResult()
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                if (iRow < 0)
                    return;

                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("LOTID");
                dt.Columns.Add("WIPSEQ");
                dt.Columns.Add("AREAID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("ACTID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Process.CPROD;
                dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                dr["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboEquipment.SelectedValue.ToString();
                dr["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                dt.Rows.Add(dr);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgDefect, searchResult, FrameOperation);

                //new ClientProxy().ExecuteService("DA_QCA_SEL_WIPRESONCOLLECT", "INDATA", "OUTDATA", dt, (result, exception) =>
                //{
                //    try
                //    {
                //        ShowLoadingIndicator();
                //        if (exception != null)
                //        {
                //            Util.MessageException(exception);
                //            return;
                //        }

                //        Util.GridSetData(dgDefect, result, FrameOperation, false);
                //    }
                //    catch (Exception ex)
                //    {
                //        Util.MessageException(ex);
                //    }
                //    finally
                //    {
                //        HiddenLoadingIndicator();
                //    }
                //});
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

        #region Popup Event
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!CanStartRun())
                return;

            if (wndRunStart != null)
                wndRunStart = null;

            wndRunStart = new ASSY003_021_RUNSTART();
            wndRunStart.FrameOperation = FrameOperation;

            if (wndRunStart != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = cboEquipment.SelectedValue.ToString();
                Parameters[2] = Process.CPROD;

                C1WindowExtension.SetParameters(wndRunStart, Parameters);

                wndRunStart.Closed += new EventHandler(wndRunStart_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndRunStart.ShowModal()));
            }
        }

        private bool CanStartRun()
        {
            bool bRet = false;

            if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("설비를 선택 하세요.");
                Util.MessageValidation("SFU1673");
                return bRet;
            }

            //for (int i = 0; i < dgProductLot.Rows.Count - dgProductLot.BottomRows.Count; i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "WIPSTAT")).Equals("PROC"))
            //    {
            //        //Util.Alert("작업중인 LOT이 존재 합니다.");
            //        Util.MessageValidation("SFU1847");
            //        return bRet;
            //    }
            //}

            bRet = true;
            return bRet;
        }

        private void wndRunStart_Closed(object sender, EventArgs e)
        {
            wndRunStart = null;
            ASSY003_021_RUNSTART window = sender as ASSY003_021_RUNSTART;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot(true, window.NEW_PROD_LOT);
            }

            //if (wndRunStart.DialogResult == MessageBoxResult.OK)
            //{
            //    btnSearch_Click(sender, null);
            //}
        }

        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            //if (!CanCancelRun())
            //    return;

            //if (wndRunCancel != null)
            //    wndRunCancel = null;

            //wndRunCancel = new ASSY003_021_RUN_CANCEL();
            //wndRunCancel.FrameOperation = FrameOperation;

            //if (wndRunCancel != null)
            //{
            //    object[] Parameters = new object[3];
            //    Parameters[0] = "작업장ID";
            //    Parameters[1] = "작업장NAME";
            //    Parameters[2] = "LOTID";

            //    C1WindowExtension.SetParameters(wndRunCancel, Parameters);

            //    this.Dispatcher.BeginInvoke(new Action(() => wndRunCancel.ShowModal()));
            //}

            if (!CanCancelRun())
                return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private bool CanCancelRun()
        {
            bool bRet = false;
            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("작업중인 LOT이 아닙니다.");
                Util.MessageValidation("SFU1846");
                return bRet;
            }

            string sLotid = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "LOTID"));
            string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSEQ"));

            // 투입 이력 정보 존재여부 확인            
            if (ChkInputHistCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU3437");   //투입이력이 존재하여 취소할 수 없습니다.
                return bRet;
            }

            // 완성 이력 정보 존재여부 확인
            if (ChkOutTrayCnt(sLotid, sWipSeq))
            {
                Util.MessageValidation("SFU3438");   // 생산Tray가 존재하여 취소할 수 없습니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_START_LOT_CPROD", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//정상 처리 되었습니다.
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

        private bool ChkInputHistCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["LOTID"] = sLotid;
                newRow["WIPSEQ"] = sWipSeq;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_MTRL_HIST_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool ChkOutTrayCnt(string sLotid, string sWipSeq)
        {
            try
            {
                bool bRet = false;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = Process.CPROD;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["EQPTID"] = cboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = sLotid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    if (dtRslt.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtRslt.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!CanConfirm())
                return;

            Util.MessageConfirm("SFU1706", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (wndConfirm != null)
                        wndConfirm = null;

                    DataTable dtInfo = ((DataView)dgLamiCell.ItemsSource).Table.Copy();

                    wndConfirm = new ASSY003_021_CONFIRM();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        object[] Parameters = new object[12];
                        Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                        Parameters[1] = cboEquipment.SelectedValue.ToString();
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSEQ"));
                        Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "WIPSTAT"));

                        Parameters[5] = Util.NVC(txtShift.Text); //= Util.NVC(wndPopup.SHIFTNAME);
                        Parameters[6] = Util.NVC(txtShift.Tag); //= Util.NVC(wndPopup.SHIFTCODE);
                        Parameters[7] = Util.NVC(txtWorker.Text); //= Util.NVC(wndPopup.USERNAME);
                        Parameters[8] = Util.NVC(txtWorker.Tag); //= Util.NVC(wndPopup.USERID);                            
                        Parameters[9] = Util.NVC(txtShiftStartTime.Text); //= Util.NVC(wndPopup.WRKSTRTTIME);
                        Parameters[10] = Util.NVC(txtShiftEndTime.Text); //= Util.NVC(wndPopup.WRKENDTTIME);

                        Parameters[11] = dtInfo;

                        C1WindowExtension.SetParameters(wndConfirm, Parameters);

                        wndConfirm.Closed += new EventHandler(wndConfirm_Closed);

                        this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    }

                }
            });            
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            wndConfirm = null;
            ASSY003_021_CONFIRM window = sender as ASSY003_021_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }

            GetEqptWrkInfo();
        }

        private bool CanConfirm()
        {
            bool bRet = false;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            //if (!Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            //{
            //    //Util.Alert("확정 할 수 있는 LOT상태가 아닙니다.");
            //    Util.MessageValidation("SFU2045");
            //    return bRet;
            //}

            // DISPATCH 여부 체크.            
            //if (GetNotDispatchCnt() > 0)
            //{
            //    //Util.Alert("발행처리 하지 않은 생산 반제품이 존재 합니다.");
            //    Util.MessageInfo("SFU1558");
            //    return bRet;
            //}

            if (string.IsNullOrEmpty(txtShift.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");
                return bRet;
            }

            if (string.IsNullOrEmpty(txtShiftDateTime.Text.Trim()))
            {
                Util.MessageValidation("SFU1845");
                return bRet;
            }

            if (string.IsNullOrEmpty(txtWorker.Text.Trim()))
            {
                Util.MessageValidation("SFU1843");
                return bRet;
            }

            if (!string.IsNullOrEmpty(txtShiftEndTime.Text))
            {
                DateTime shiftStartDateTime = Convert.ToDateTime(txtShiftStartTime.Text);
                DateTime shiftEndDateTime = Convert.ToDateTime(txtShiftEndTime.Text);
                DateTime systemDateTime = GetSystemTime();

                // 공통코드에 등록된 시간 내의 경우에는 초기화 하지 않도록..
                double dHour = 0;
                if (GetExcptTime(out dHour))
                {
                    if (dHour > 0)
                    {
                        shiftStartDateTime = shiftStartDateTime.AddHours(-dHour);
                        shiftEndDateTime = shiftEndDateTime.AddHours(-dHour);
                    }
                }

                int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                if (prevCheck < 0 || nextCheck > 0)
                {
                    Util.MessageValidation("10012", ObjectDic.Instance.GetObjectName("작업자"));
                    txtWorker.Text = string.Empty;
                    txtWorker.Tag = string.Empty;
                    txtShift.Text = string.Empty;
                    txtShift.Tag = string.Empty;
                    txtShiftStartTime.Text = string.Empty;
                    txtShiftEndTime.Text = string.Empty;
                    txtShiftDateTime.Text = string.Empty;
                    return bRet;
                }
            }

            bRet = true;
            return bRet;
        }

        private bool GetExcptTime(out double dHour)
        {
            dHour = 0;

            try
            {
                bool bRet = false;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = Process.CPROD;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_WORK_EXCP_TIME", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("EXCP_TIME"))
                {
                    if (!double.TryParse(Util.NVC(dtRslt.Rows[0]["EXCP_TIME"]), out dHour))
                        dHour = 0;

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

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private int GetNotDispatchCnt()
        {
            try
            {
                int iCnt = 0;

                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_NOT_DISPATCH_CNT();

                DataRow newRow = inTable.NewRow();

                newRow["PR_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_NOT_DISPATCH_CNT", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    iCnt = Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["NOT_DISPATCH_CNT"]));
                }

                return iCnt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return 0;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            wndShiftUser = null;

            CMM_SHIFT_USER2 wndPopup = sender as CMM_SHIFT_USER2;

            if (wndPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
        }

        #endregion

        #region [### 설비별 작업조, 작업자, 작업시간 조회 ###]
        private void GetEqptWrkInfo()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Process.CPROD;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void SaveDfct()
        {
            try
            {
                ShowLoadingIndicator();

                dgDefect.EndEdit();

                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("선택된 항목이 없습니다.");
                    Util.MessageValidation("SFU1651");
                }

                if (Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID")).Equals(""))
                {
                    //Util.Alert("투입 LOT이 없습니다.");
                    Util.MessageValidation("SFU1945");
                }

                DataSet indataSet = _Biz.GetBR_PRD_REG_DEFECT_ALL();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable inDEFECT_LOT = indataSet.Tables["INRESN"];

                //for (int i = 0; i < dgDefect.Columns.Count; i++)
                //{
                //    if (dgDefect.Columns[i].Name.ToString().StartsWith("DEFECTQTY"))
                //    {
                //        string sLot = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "LOTID"));
                //        string sWipSeq = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridRowIndex(dgProductLot, "PR_LOTID", sLot)].DataItem, "WIPSEQ"));
                //        string sColName = dgDefect.Columns[i].Name.ToString();

                //        for (int j = 0; j < dgDefect.Rows.Count - dgDefect.BottomRows.Count; j++)
                //        {
                //            newRow = null;

                //            newRow = inDEFECT_LOT.NewRow();
                //            newRow["LOTID"] = sLot;
                //            newRow["WIPSEQ"] = sWipSeq;
                //            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTID"));
                //            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "RESNCODE"));
                //            newRow["RESNQTY"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, sColName)).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, sColName)));
                //            newRow["RESNCODE_CAUSE"] = "";
                //            newRow["PROCID_CAUSE"] = "";
                //            newRow["RESNNOTE"] = "";
                //            //newRow["DFCT_TAG_QTY"] = 0;
                //            newRow["LANE_QTY"] = 1;
                //            newRow["LANE_PTN_QTY"] = 1;

                //            if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "ACTID")).Equals("CHARGE_PROD_LOT"))
                //                newRow["COST_CNTR_ID"] = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[j].DataItem, "COST_CNTR_ID"));
                //            else
                //                newRow["COST_CNTR_ID"] = "";

                //            newRow["A_TYPE_DFCT_QTY"] = 0;
                //            newRow["C_TYPE_DFCT_QTY"] = 0;

                //            inDEFECT_LOT.Rows.Add(newRow);
                //        }
                //    }
                //}

                for (int i = 0; i < dgDefect.Rows.Count - dgDefect.BottomRows.Count; i++)
                {
                    newRow = null;
                    newRow = inDEFECT_LOT.NewRow();
                    newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "LOTID"));
                    newRow["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
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

                if (inDEFECT_LOT.Rows.Count < 1)
                {
                    HiddenLoadingIndicator();
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_QCA_REG_WIPREASONCOLLECT_ALL", "INDATA,INRESN", null, indataSet);

                //GetDfctResult();
                GetProductLot();

                //Util.AlertInfo("정상 처리 되었습니다.");
                Util.MessageInfo("SFU1275");
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

        private string GetNewOutLotId()
        {
            try
            {
                string lotId = "";

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("EQPTID");

                DataRow dr = dt.NewRow();
                dr["EQPTID"] = cboEquipment.SelectedValue;
                dt.Rows.Add(dr);

                DataTable result = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_BOX_LOTID_CPROD", "INDATA", "OUTDATA", dt);

                lotId = result.Rows[0]["LOTID"].ToString();

                return lotId;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void CreateOut(string OutLotId)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE");
                inTable.Columns.Add("IFMODE");
                inTable.Columns.Add("EQPTID");
                inTable.Columns.Add("PROD_LOTID");
                inTable.Columns.Add("USERID");
                
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(newRow);

                DataTable inLot = indataSet.Tables.Add("IN_LOT");
                inLot.Columns.Add("WIPQTY", typeof(Decimal));
                inLot.Columns.Add("CSTID", typeof(string));

                DataRow newRow2 = inLot.NewRow();

                newRow2["WIPQTY"] = Convert.ToDecimal(txtBoxCellQty.Text);
                newRow2["CSTID"] = txtBoxid.Text.Trim().ToUpper();

                indataSet.Tables["IN_LOT"].Rows.Add(newRow2);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_OUT_LOT_CPROD", "INDATA,IN_LOT", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        string sNewLot = Util.NVC(searchResult.Tables["OUTDATA"].Rows[0]["LOTID"]);

                        BoxIDPrint(sNewLot, Convert.ToDecimal(txtBoxCellQty.Text));

                        GetProductLot();

                        int idx = _Util.GetDataGridRowIndex(dgOutProduct, "LOTID", sNewLot);
                        if (idx >= 0)
                            DataTableConverter.SetValue(dgOutProduct.Rows[idx].DataItem, "CHK", true);

                        txtBoxid.Text = "";
                        txtBoxid.Focus();
                        //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );               
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

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboEquipment.SelectedIndex <= 0)
                {
                    Clear();
                    return;
                }

                GetProductLot();

                if (cboEquipment.SelectedIndex > 0 && cboEquipment.Items.Count > cboEquipment.SelectedIndex)
                {
                    if (Util.NVC((cboEquipment.Items[cboEquipment.SelectedIndex] as DataRowView).Row["CBO_NAME"]).IndexOf("SELECT") < 0)
                    {
                        ShowLoadingIndicator();

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Clear()
        {
            Util.gridClear(dgProductLot);
            Util.gridClear(dgCurrIn);
            Util.gridClear(dgWaitInput);
            Util.gridClear(dgOutProduct);
            Util.gridClear(dgLamiCell);
            Util.gridClear(dgDefect);
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (btnOutPrint != null)
                    btnOutPrint.IsEnabled = true;

                if (e.RightButton == MouseButtonState.Released &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    btnTESTPrint_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }        
        }

        private void btnTESTPrint_Click(object sender, RoutedEventArgs e)
        {
            if (wndTestPrint != null)
                wndTestPrint = null;

            wndTestPrint = new ASSY003_TEST_PRINT();
            wndTestPrint.FrameOperation = FrameOperation;

            wndTestPrint.Closed += new EventHandler(wndTestPrint_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => wndTestPrint.ShowModal()));
        }

        private void wndTestPrint_Closed(object sender, EventArgs e)
        {
            wndTestPrint = null;
            ASSY003_TEST_PRINT window = sender as ASSY003_TEST_PRINT;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void txtBoxCellQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtBoxCellQty.Text, 0))
                {
                    txtBoxCellQty.Text = "";
                    txtBoxCellQty.Focus();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgOutProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("Y"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRINT_YN")).Equals("N"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                }
            }));
        }

        private void dgOutProduct_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }
        
        private void txtBoxid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!CanCreateBox())
                    return;

                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("생성 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                Util.MessageConfirm("SFU1621", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateOut(GetNewOutLotId());
                        txtBoxid.Focus();
                    }
                });
            }
        }

        #region [### Lot Merge 체크###] - 추가기능
        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!CanMerge())
                return;

            if (wndMerge != null)
                wndMerge = null;

            wndMerge = new ASSY003_OUTLOT_MERGE();
            wndMerge.FrameOperation = FrameOperation;

            if (wndMerge != null)
            {
                object[] Parameters = new object[4];
                Parameters[0] = cboEquipmentSegment.SelectedValue;
                Parameters[1] = cboEquipment.SelectedValue;
                Parameters[2] = Process.CPROD;
                Parameters[3] = cboEquipmentSegment.Text;

                C1WindowExtension.SetParameters(wndMerge, Parameters);

                wndMerge.Closed += new EventHandler(wndMerge_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndMerge.ShowModal()));
            }
        }
        
        private bool CanMerge()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            //if (cboEquipment.SelectedIndex < 0 || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    //Util.Alert("설비를 선택 하세요.");
            //    Util.MessageValidation("SFU1673");
            //    return bRet;
            //}

            bRet = true;
            return bRet;
        }

        private void wndMerge_Closed(object sender, EventArgs e)
        {
            wndMerge = null;

            ASSY003_OUTLOT_MERGE window = sender as ASSY003_OUTLOT_MERGE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                
            }
            GetProductLot();
        }
        #endregion

        private void dgCurrIn_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
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

                                SetChkBoxControls(e, dgCurrIn);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetChkBoxControls(DataGridCellEventArgs e, C1DataGrid dg)
        {
            try
            {
                int preValue = (int)e.Cell.Value;

                Util.DataGridCheckAllUnChecked(dg);

                if (preValue > 0) e.Cell.Value = true;
                else e.Cell.Value = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}