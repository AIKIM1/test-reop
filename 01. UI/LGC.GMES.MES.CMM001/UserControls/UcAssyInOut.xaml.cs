/*************************************************************************************
 Created Date : 2017.05.17
      Creator : 신광희C
   Decription : [조립 - 원각 및 초소형 투입 영역 UserControl]
--------------------------------------------------------------------------------------
 [Change History]
   2017.05.17   신광희C   : 최초생성
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UcAssyInOut.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyInOut
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public UserControl _UcParent;     // Caller
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();
        

        public TextBox TextCurrInLotId { get; set; }

        public TextBox TextInputPancakeLotId { get; set; }

        public Button ButtonCurrInCancel { get; set; }

        public Button ButtonCurrInReplace { get; set; }

        public Button ButtonCurrInComplete { get; set; }


        public string Eqptsegment { get; set; }

        public string Eqptid { get; set; }

        public string ProcId { get; set; }

        public string ProdLotId { get; set; }

        public string ProdWipSeq { get; set; }

        public string ProdWorkOrderId { get; set; }

        public string ProdWorkOrderDetailId { get; set; }

        public string ProdLotState { get; set; }

        private struct PRV_VALUES
        {
            public string sPrvOutTray;
            public string sPrvCurrIn;
            public string sPrvOutBox;

            public PRV_VALUES(string sTray, string sIn, string sBox)
            {
                this.sPrvOutTray = sTray;
                this.sPrvCurrIn = sIn;
                this.sPrvOutBox = sBox;
            }
        }

        private PRV_VALUES _PRV_VLAUES = new PRV_VALUES("", "", "");

        private DateTime _MinValidDate;
        private bool _StackingYN = false;

        public UcAssyInOut()
        {
            InitializeComponent();
        }


        private void InitializeControls()
        {
            try
            {
                CommonCombo combo = new CommonCombo();

                // 자재 투입위치 코드
                String[] sFilter1 = { Eqptid, "PROD" };
                String[] sFilter2 = { Eqptid, null }; // 자재,제품 전체

                //combo.SetCombo(cboPancakeMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboMagMountPstnID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboBoxMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
                combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");

                if (ProdLotId.Equals(Process.WINDING))
                {
                    tbCurrIn.Visibility = Visibility.Visible;
                    tbPancake.Visibility = Visibility.Visible;
                    tbInBox.Visibility = Visibility.Visible;
                    tbMagazine.Visibility = Visibility.Collapsed;
                    tbBox.Visibility = Visibility.Collapsed;
                    tbHist.Visibility = Visibility.Collapsed;

                    grdMagTypeCntInfo.Visibility = Visibility.Collapsed;
                }
                //else if (PROCID.Equals(Process.STACKING_FOLDING))
                //{
                //    tbCurrIn.Visibility = Visibility.Visible;
                //    tbPancake.Visibility = Visibility.Collapsed;
                //    tbMagazine.Visibility = Visibility.Visible;
                //    tbBox.Visibility = Visibility.Collapsed;
                //    tbHist.Visibility = Visibility.Visible;
                //    tbInBox.Visibility = Visibility.Collapsed;

                //    grdMagTypeCntInfo.Visibility = Visibility.Visible;

                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ClearDataGrid()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null)
                    Util.gridClear(dgCurrIn);
                if (txtCurrInLotID != null)
                    txtCurrInLotID.Text = string.Empty;

                //Pancake투입
                if (dgWaitPancake != null)
                    Util.gridClear(dgWaitPancake);

                if (txtInputPancakeLot != null)
                    txtInputPancakeLot.Text = string.Empty;

                //대기매거진
                if (dgWaitMagazine != null)
                    Util.gridClear(dgWaitMagazine);

                if (txtWaitMazID != null)
                    txtWaitMazID.Text = string.Empty;

                //바구니투입
                if (dgWaitBox != null)
                    Util.gridClear(dgWaitBox);

                //투입 바구니(PKG INPUT)
                if (dgInputBox != null)
                    Util.gridClear(dgInputBox);

                //투입이력
                if (dgInputHist != null)
                    Util.gridClear(dgInputHist);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SearchAll()
        {
            try
            {
                ClearAll();

                if (tbCurrIn.Visibility == Visibility.Visible)
                {
                    GetCurrInList();
                }
                if (tbPancake.Visibility == Visibility.Visible)
                {
                    GetWaitPancake();
                }
                if (tbMagazine.Visibility == Visibility.Visible)
                {
                    GetWaitMagazine();
                }
                if (tbBox.Visibility == Visibility.Visible)
                {
                    GetWaitBox();
                }
                if (tbHist.Visibility == Visibility.Visible)
                {
                    //GetInputHistory();
                }
                if (tbInBox.Visibility == Visibility.Visible)
                {
                    GetInBoxList();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ClearAll()
        {
            try
            {
                //투입현황
                if (dgCurrIn != null)
                    Util.gridClear(dgCurrIn);
                //if (tbATypeCnt != null)
                //    tbATypeCnt.Text = "0";
                //if (tbCTypeCnt != null)
                //    tbCTypeCnt.Text = "0";

                if (txtCurrInLotID != null) txtCurrInLotID.Text = string.Empty;

                //Pancake투입
                if (dgWaitPancake != null) Util.gridClear(dgWaitPancake);
                if (txtInputPancakeLot != null) txtInputPancakeLot.Text = string.Empty;

                //대기매거진
                if (dgWaitMagazine != null) Util.gridClear(dgWaitMagazine);
                if (txtWaitMazID != null) txtWaitMazID.Text = string.Empty;

                //바구니투입
                if (dgWaitBox != null) Util.gridClear(dgWaitBox);

                //투입 바구니(PKG INPUT)
                if (dgInputBox != null) Util.gridClear(dgInputBox);

                //투입이력
                if (dgInputHist != null) Util.gridClear(dgInputHist);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetCurrInList()
        {
            try
            {
                // 메인 LOT이 없는 경우 disable 처리..
                //if (ProdLotId PROD_LOTID.Equals(""))
                if(string.IsNullOrEmpty(ProdLotId))
                {
                    if (ProcId.Equals(Process.PACKAGING))
                        btnCurrInCancel.IsEnabled = true;
                    else
                        btnCurrInCancel.IsEnabled = false;

                    btnCurrInComplete.IsEnabled = false;
                }
                else
                {
                    btnCurrInCancel.IsEnabled = true;
                    btnCurrInComplete.IsEnabled = true;
                }

                string bizRuleName;

                if (ProcId.Equals(Process.LAMINATION))
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_LM";
                else
                    bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_CURR_IN_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Eqptid;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCurrIn, searchResult, FrameOperation);

                        if (!_PRV_VLAUES.sPrvCurrIn.Equals(""))
                        {
                            int idx = _util.GetDataGridRowIndex(dgCurrIn, "EQPT_MOUNT_PSTN_ID", _PRV_VLAUES.sPrvCurrIn);

                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(dgCurrIn.Rows[idx].DataItem, "CHK", true);

                                //row 색 바꾸기
                                dgCurrIn.SelectedIndex = idx;
                                dgCurrIn.ScrollIntoView(idx, dgCurrIn.Columns["CHK"].Index);
                            }
                        }

                        // 라미의 경우 컬럼 다르게 보이도록 수정.
                        if (ProcId.Equals(Process.LAMINATION))
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Collapsed;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Collapsed;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Visible;
                        }
                        else
                        {
                            dgCurrIn.Columns["MOUNT_MTRL_TYPE_NAME"].Visibility = Visibility.Visible;
                            //dgCurrIn.Columns["WIPSNAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["MTRLNAME"].Visibility = Visibility.Visible;
                            dgCurrIn.Columns["INPUT_MTRL_CLSS_NAME"].Visibility = Visibility.Collapsed;
                        }

                        if (dgCurrIn.CurrentCell != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.CurrentCell.Row.Index, dgCurrIn.Columns.Count - 1);
                        else if (dgCurrIn.Rows.Count > 0 && dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1) != null)
                            dgCurrIn.CurrentCell = dgCurrIn.GetCell(dgCurrIn.Rows.Count, dgCurrIn.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetWaitPancake()
        {
            try
            {
                string sInMtrlClssCode = GetInputMtrlClssCode();


                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Eqptsegment;
                newRow["EQPTID"] = Eqptid;
                newRow["PROCID"] = ProcId;
                newRow["WOID"] = ProdWorkOrderId;
                newRow["IN_LOTID"] = txtInputPancakeLot.Text;   // txtWaitPancakeLot.Text;
                //newRow["PRDT_CLSS_CODE"] = null;  -- 설비 조건으로 PROD_CLASS_CODE 조회 함..
                newRow["INPUT_MTRL_CLSS_CODE"] = sInMtrlClssCode;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _MinValidDate);
                        }

                        //dgWaitPancake.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgWaitPancake, searchResult, FrameOperation);

                        //lblSelWaitPancakeCnt.Text = (dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count).ToString();

                        if (dgWaitPancake.CurrentCell != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.CurrentCell.Row.Index, dgWaitPancake.Columns.Count - 1);
                        else if (dgWaitPancake.Rows.Count > 0 && dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1) != null)
                            dgWaitPancake.CurrentCell = dgWaitPancake.GetCell(dgWaitPancake.Rows.Count, dgWaitPancake.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetWaitMagazine(string sLot = "")
        {
            try
            {
                string sElec = string.Empty;

                if (rdoMagAType.IsChecked.HasValue && (bool)rdoMagAType.IsChecked)
                {
                    sElec = rdoMagAType.Tag.ToString();
                }
                else if (rdoMagCtype.IsChecked.HasValue && (bool)rdoMagCtype.IsChecked)
                {
                    sElec = rdoMagCtype.Tag.ToString();
                }
                else
                    return;

                string sBizName = "";

                if (_StackingYN)
                    sBizName = "DA_PRD_SEL_WAIT_MAG_ST";
                else
                    sBizName = "DA_PRD_SEL_WAIT_MAG_FD";

                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WAIT_LOT_LIST_FD();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Eqptsegment;
                newRow["PROCID"] = ProcId;
                newRow["EQPTID"] = Eqptid;
                newRow["PRODUCT_LEVEL2_CODE"] = _StackingYN ? sElec : "BC"; //BI-CELL, Stacking 경우 Lv2가 Lv3와 동일 코드.
                if (!_StackingYN)
                    newRow["PRODUCT_LEVEL3_CODE"] = sElec;

                newRow["WOID"] = ProdWorkOrderId;
                if (!sLot.Equals(""))
                    newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", inTable, (bizResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        // FIFO 기준 Date
                        try
                        {
                            if (bizResult != null && bizResult.Rows.Count > 0 && bizResult.Columns.Contains("VALID_DATE_YMDHMS"))
                            {
                                DataRow row = (from t in bizResult.AsEnumerable()
                                               where (t.Field<string>("VALID_DATE_YMDHMS") != null)
                                               select t).FirstOrDefault();


                                if (row != null)
                                {
                                    DateTime.TryParse(Util.NVC(row["VALID_DATE_YMDHMS"]), out _MinValidDate);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        //dgWaitMagazine.ItemsSource = DataTableConverter.Convert(bizResult);
                        Util.GridSetData(dgWaitMagazine, bizResult, FrameOperation);

                        if (dgWaitMagazine.CurrentCell != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.CurrentCell.Row.Index, dgWaitMagazine.Columns.Count - 1);
                        else if (dgWaitMagazine.Rows.Count > 0 && dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1) != null)
                            dgWaitMagazine.CurrentCell = dgWaitMagazine.GetCell(dgWaitMagazine.Rows.Count, dgWaitMagazine.Columns.Count - 1);
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public void GetWaitBox()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = ProcId != Process.SSC_FOLDED_BICELL ? Process.PACKAGING : Process.SSC_FOLDED_BICELL;
                newRow["EQSGID"] = Eqptsegment;
                newRow["EQPTID"] = Eqptid;
                newRow["WO_DETL_ID"] = ProdWorkOrderDetailId;

                inTable.Rows.Add(newRow);

                string bizRuleName = ProcId == Process.SSC_FOLDED_BICELL ? "DA_PRD_SEL_WAIT_LOT_LIST_SSC_FD" : "DA_PRD_SEL_WAIT_LOT_LIST_CL";

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _MinValidDate);
                        }


                        Util.GridSetData(dgWaitBox, searchResult, FrameOperation);

                        if (dgWaitBox.CurrentCell != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.CurrentCell.Row.Index, dgWaitBox.Columns.Count - 1);
                        else if (dgWaitBox.Rows.Count > 0 && dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1) != null)
                            dgWaitBox.CurrentCell = dgWaitBox.GetCell(dgWaitBox.Rows.Count, dgWaitBox.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetInBoxList()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_IN_BOX_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = ProdLotId;
                newRow["WIPSEQ"] = ProdWipSeq.Equals("") ? 1 : Convert.ToDecimal(ProdWipSeq);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_IN_BOX_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgInputBox, searchResult, FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenParentLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetInputMtrlClssCode()
        {
            try
            {
                //if (cboPancakeMountPstnID == null || cboPancakeMountPstnID.SelectedValue == null)
                //{
                //    return "";
                //}



                string sInputMtrlClssCode = "";
                ShowParentLoadingIndicator();

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_INPUT_MTRL_CLSS_CODE();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = Eqptid;
                newRow["EQPT_MOUNT_PSTN_ID"] = string.Empty; //cboPancakeMountPstnID.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPUT_PRDT_CLSS_CODE_BY_MOUNT_PSTN_ID", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sInputMtrlClssCode = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                    //txtWaitPancakeInputClssCode.Text = Util.NVC(dtResult.Rows[0]["INPUT_MTRL_CLSS_CODE"]);
                }
                return sInputMtrlClssCode;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }
        }

        private void PancakeInput()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_MTRL_INPUT_LM();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Eqptid;
                newRow["PROD_LOTID"] = ProdLotId;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                for (int i = 0; i < dgWaitPancake.Rows.Count - dgWaitPancake.BottomRows.Count; i++)
                {
                    if (!_util.GetDataGridCheckValue(dgWaitPancake, "CHK", i)) continue;

                    newRow = inInputTable.NewRow();
                    newRow["EQPT_MOUNT_PSTN_ID"] = string.Empty;// cboPancakeMountPstnID.SelectedValue.ToString();
                    newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitPancake.Rows[i].DataItem, "LOTID"));

                    inInputTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_INPUT_IN_LOT", "INDATA,IN_INPUT", null, (searchResult, searchException) =>
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
                        HiddenParentLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        protected virtual void GetProductLot()
        {
            if (_UcParent == null)
                return;

            try
            {
                Type type = _UcParent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetProductLot");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UcParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void ChangeEquipment(string equipmentSegmentId, string equipmentId)
        {
            //UcAssyInOut?.ChangeEquipment(ComboEquipmentSegment.SelectedValue.GetString(), ComboEquipment.SelectedValue.GetString());

            try
            {

                Eqptsegment = equipmentSegmentId;
                Eqptid = equipmentId;

                ProdLotId = string.Empty;
                ProdWipSeq = string.Empty;
                ProdWorkOrderId = string.Empty;
                ProdWorkOrderDetailId = string.Empty;
                ProdLotState = string.Empty;


                InitializeControls();

                ClearAll();

                // 현재 설비 투입 자재 조회 처리.
                //GetCurrInList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowParentLoadingIndicator()
        {
            if (_UcParent == null)
                return;

            try
            {
                Type type = _UcParent.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UcParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrInCancel(DataTable inMtrl)
        {
            if (_UcParent == null)
                return;

            try
            {
                Type type = _UcParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInCancel");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UcParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        protected virtual void OnCurrInComplete(DataTable inMtrl)
        {
            if (_UcParent == null)
                return;

            try
            {
                Type type = _UcParent.GetType();
                MethodInfo methodInfo = type.GetMethod("OnCurrInComplete");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    if (i == 0) parameterArrys[i] = inMtrl;
                    else parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UcParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HiddenParentLoadingIndicator()
        {
            if (_UcParent == null)
                return;

            try
            {
                Type type = _UcParent.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_UcParent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCurrIn_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null && dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null && checkBox.IsChecked.HasValue && !(bool)checkBox.IsChecked))
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    _PRV_VLAUES.sPrvCurrIn = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_MOUNT_PSTN_ID"));

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                var box = dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox;
                                                if (box !=null)
                                                    box.IsChecked = false;
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

                                    _PRV_VLAUES.sPrvCurrIn = "";
                                }
                                break;
                        }

                        if (dg.CurrentCell != null)
                            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                        else if (dg.Rows.Count > 0 && dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1) != null)
                            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);

                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
