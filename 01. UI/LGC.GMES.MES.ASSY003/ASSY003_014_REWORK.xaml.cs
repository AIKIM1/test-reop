/*************************************************************************************
 Created Date : 2017.08.15
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - C 생산 관리 화면 - 재작업 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.15  INS 김동일K : Initial Created.
  2017.09.30  CNS 고현영S : DA,BR 추가 및 재작업
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_REWORK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_REWORK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _CProdLotId = string.Empty;
        private string _PjtName = string.Empty;
        private string _ProdId = string.Empty;
        private string _AreaId = string.Empty;
        private string _EqsgId = string.Empty;
        private string _EqsgName = string.Empty;
        private string _EqptId = string.Empty;
        private string _EqptName = string.Empty;
        private string _LotState = string.Empty;
        private string _DfctQty = string.Empty;
        private string _SendDttm = string.Empty;
        private string _SendUser = string.Empty;
        private string _MktTypeCode = string.Empty;
        private string _MktTypeName = string.Empty;
        private string _CProdWrkPstnId = string.Empty;
        private string _CProdWrkPstnName = string.Empty;
        private string _workerId = string.Empty;
        private string _workerName = string.Empty;

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

        public ASSY003_014_REWORK()
        {
            InitializeComponent();
        }

        private void InitTextBox()
        {
            tbCProdLot.Text = _CProdLotId;
            tbPjtName.Text = _PjtName;
            tbProdID.Text = _ProdId;
            tbDfctQty.Text = _DfctQty;
            tbTransferLine.Text = _EqsgName;
            tbMktTypeName.Text = _MktTypeName;
        }

        private void GetDgBCList()
        {
            try
            {
                if (_LotState.Equals("IN"))
                {
                    DataTable inTable = _Biz.GetDA_PRD_SEL_BC_MTRL();

                    DataRow newRow = inTable.NewRow();
                    newRow["PRODID"] = _ProdId;
                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_BC_MTRL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            Util.GridSetData(dgBCList, searchResult, FrameOperation, false);
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
                else if (_LotState.Equals("PROC"))
                {
                    DataTable BCDATA = new DataTable("BCDATA");
                    BCDATA.Columns.Add("CPROD_LOTID");
                    BCDATA.Columns.Add("PRODID");

                    DataRow newRow = BCDATA.NewRow();
                    newRow["CPROD_LOTID"] = _CProdLotId;
                    newRow["PRODID"] = _ProdId;
                    BCDATA.Rows.Add(newRow);

                    DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BC_CPROD_WRK_RSLT", "INDATA", "OUTDATA", BCDATA);

                    //BC에 대해서 입력이 없던 C생산 LOT
                    if (searchResult.Rows.Count <= 0)
                    {
                        searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BC_MTRL", "INDATA", "OUTDATA", BCDATA);
                    }

                    Util.GridSetData(dgBCList, searchResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }

        private void GetDgFCList()
        {
            try
            {
                if (_LotState.Equals("IN"))
                {
                    DataTable inTable = _Biz.GetDA_PRD_SEL_BC_MTRL();

                    DataRow newRow = inTable.NewRow();
                    newRow["PRODID"] = _ProdId;
                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_FC_PROD", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                    {
                        try
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }
                            Util.GridSetData(dgFCList, searchResult, FrameOperation, true);
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
                else if (_LotState.Equals("PROC"))
                {
                    DataTable FCDATA = new DataTable("FCDATA");
                    FCDATA.Columns.Add("CPROD_LOTID");
                    FCDATA.Columns.Add("PRODID");

                    DataRow newRow = FCDATA.NewRow();
                    newRow["CPROD_LOTID"] = _CProdLotId;
                    newRow["PRODID"] = _ProdId;
                    FCDATA.Rows.Add(newRow);

                    DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FC_CPROD_WRK_RSLT", "INDATA", "OUTDATA", FCDATA);

                    //FC
                    if (searchResult.Rows.Count <= 0)
                    {
                        searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FC_PROD", "INDATA", "OUTDATA", FCDATA);
                    }

                    Util.GridSetData(dgFCList, searchResult, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length == 18)
                {
                    _CProdLotId = Util.NVC(tmps[0]);
                    _DfctQty = Util.NVC_Int(tmps[1]).ToString();
                    _PjtName = Util.NVC(tmps[2]);
                    _ProdId = Util.NVC(tmps[3]);
                    _AreaId = Util.NVC(tmps[4]);
                    _EqsgId = Util.NVC(tmps[5]);
                    _EqsgName = Util.NVC(tmps[6]);
                    _EqptId = Util.NVC(tmps[7]);
                    _EqptName = Util.NVC(tmps[8]);
                    _LotState = Util.NVC(tmps[9]);
                    _MktTypeCode = Util.NVC(tmps[10]);
                    _MktTypeName = Util.NVC(tmps[11]);
                    _SendDttm = Util.NVC(tmps[12]);
                    _SendUser = Util.NVC(tmps[13]);
                    _CProdWrkPstnId = Util.NVC(tmps[14]);
                    _CProdWrkPstnName = Util.NVC(tmps[15]);
                    _workerId = Util.NVC(tmps[16]);
                    _workerName = Util.NVC(tmps[17]);
                }
                else
                {
                    _CProdLotId = "";
                    _DfctQty = "";
                    _PjtName = "";
                    _ProdId = "";
                    _AreaId = "";
                    _EqsgId = "";
                    _EqsgName = "";
                    _EqptId = "";
                    _EqptName = "";
                    _LotState = "";
                    _MktTypeCode = "";
                    _MktTypeName = "";
                    _SendDttm = "";
                    _SendUser = "";
                    _CProdWrkPstnId = "";
                    _CProdWrkPstnName = "";
                    _workerId = "";
                    _workerName = "";
                }

                InitTextBox();

                GetDgBCList();

                GetDgFCList();

                GetDgBoxList();

                ApplyPermissions();

                //this
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(this.DialogResult == MessageBoxResult.OK))
            {
                DataSet INDATA_Set = new DataSet("INDATASET");

                DataTable INDATA_Table = INDATA_Set.Tables.Add("INDATA");
                INDATA_Table.Columns.Add("USERID", typeof(string));
                INDATA_Table.Columns.Add("CPROD_LOTID", typeof(string));
                INDATA_Table.Columns.Add("RECYC_QTY_FC", typeof(decimal));
                INDATA_Table.Columns.Add("MKT_TYPE_CODE", typeof(string));

                DataRow newRow = INDATA_Table.NewRow();
                newRow["USERID"] = _workerId;
                newRow["CPROD_LOTID"] = tbCProdLot.Text;
                newRow["RECYC_QTY_FC"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgFCList.Rows[0].DataItem, "RECYC_QTY"));
                newRow["MKT_TYPE_CODE"] = _MktTypeCode;
                INDATA_Table.Rows.Add(newRow);

                DataTable IN_WRK_Table = INDATA_Set.Tables.Add("IN_WRK");

                IN_WRK_Table.Columns.Add("PRODID", typeof(string));
                IN_WRK_Table.Columns.Add("RECYC_QTY", typeof(decimal));
                IN_WRK_Table.Columns.Add("SCRP_QTY", typeof(decimal));
                IN_WRK_Table.Columns.Add("ADD_INPUT_QTY", typeof(decimal));

                foreach (var row in dgBCList.Rows)
                {
                    newRow = IN_WRK_Table.NewRow();
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                    newRow["RECYC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "RECYC_QTY"));
                    newRow["SCRP_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "SCRP_QTY"));
                    newRow["ADD_INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "ADD_INPUT_QTY"));

                    IN_WRK_Table.Rows.Add(newRow);
                }

                foreach (var row in dgFCList.Rows)
                {
                    newRow = IN_WRK_Table.NewRow();
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                    newRow["RECYC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(row.DataItem, "RECYC_QTY"));
                    newRow["SCRP_QTY"] = Util.NVC_Decimal(0);
                    newRow["ADD_INPUT_QTY"] = Util.NVC_Decimal(0);

                    IN_WRK_Table.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_WRK_RSLT", "INDATA,IN_LOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
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
                }, INDATA_Set
                );
            }
        }

        private void dgBCList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (Util.NVC_Int(e.Cell.Value) <= 0)
                e.Cell.Value = null;
        }


        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgBCList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                else
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }));
        }

        private void dgFCList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Column.Name.Equals("RECYC_QTY"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE9E9E9"));
                }
                else if (e.Cell.Presenter == null)
                {

                    return;
                }
                else
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }));
        }

        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnBoxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!CanCreateBox())
                return;

            Util.MessageConfirm("SFU1621", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateOutBox(GetNewOutLotid());
                }
            });
        }


        private void DeleteOutBox()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_DELETE_BOX_ST();

                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptId;
                newRow["PROD_LOTID"] = _CProdLotId;
                newRow["USERID"] = _workerId;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable input_LOT = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgBoxList.Rows.Count - dgBoxList.BottomRows.Count; i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgBoxList, "CHK", i)) continue;

                    newRow = input_LOT.NewRow();
                    newRow["OUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID"));

                    input_LOT.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_OUT_LOT_FD", "INDATA,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        GetDgBoxList();

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

        private void btnBoxDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CanDeleteBox())
                {
                    DeleteOutBox();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region Method

        #region [BizCall]

        private void GetDgBoxList()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID");
                inTable.Columns.Add("PR_LOTID");

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _CProdLotId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_BOX_LOT_LIST_CPROD", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchResult != null)
                        {
                            Util.GridSetData(dgBoxList, searchResult, FrameOperation, false);

                            DataTableConverter.SetValue(dgFCList.Rows[0].DataItem, "RECYC_QTY", DataTableConverter.Convert(dgBoxList.ItemsSource).AsEnumerable().Sum(row => Util.NVC_Int(row.Field<object>("WIPQTY"))));
                        }

                        C1.WPF.DataGrid.Summaries.DataGridAggregate.SetAggregateFunctions(dgBoxList.Columns["WIPQTY"]
                            , new C1.WPF.DataGrid.Summaries.DataGridAggregatesCollection { new C1.WPF.DataGrid.Summaries.DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate } });

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
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

        private void CreateOutBox(string sNewOutLot)
        {

            try
            {
                if (sNewOutLot.Equals(""))
                    return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOTID");
                inTable.Columns.Add("AREAID");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("CPROD_LOTID");
                inTable.Columns.Add("WIPQTY");
                inTable.Columns.Add("PRODID");
                inTable.Columns.Add("MKT_TYPE_CODE");
                inTable.Columns.Add("USERID");

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = sNewOutLot;
                newRow["AREAID"] = _AreaId;
                newRow["PROCID"] = Process.CPROD;
                newRow["EQSGID"] = _EqsgId;
                newRow["CPROD_LOTID"] = _CProdLotId;
                newRow["WIPQTY"] = Util.NVC_Int(txtOutBoxQty.Text);
                newRow["PRODID"] = _ProdId;
                newRow["MKT_TYPE_CODE"] = _MktTypeCode;
                newRow["USERID"] = _workerId;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_CREATE_OUT_LOT_CPROD", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //DataTa


                        AutoThermalPrint(sNewOutLot, _CProdWrkPstnName, _MktTypeName, _PjtName, _ProdId, Util.NVC_Int(txtOutBoxQty.Text), _workerName);
                        GetDgBoxList();

                        //this.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
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
        private void Move()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_REG_END_CPROD_LOT();

                DataTable INDATA_Table = indataSet.Tables["INDATA"];

                DataRow newRow = INDATA_Table.NewRow();
                newRow["USERID"] = _workerId;
                newRow["CPROD_LOTID"] = tbCProdLot.Text;
                newRow["MKT_TYPE_CODE"] = _MktTypeCode;
                newRow["RECYC_QTY_FC"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgFCList.Rows[0].DataItem, "RECYC_QTY"));

                INDATA_Table.Rows.Add(newRow);

                DataTable IN_BC_Table = indataSet.Tables["IN_BC"];
                foreach (var dr in dgBCList.Rows)
                {
                    newRow = IN_BC_Table.NewRow();
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "PRODID"));
                    newRow["RECYC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dr.DataItem, "RECYC_QTY"));
                    newRow["SCRP_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dr.DataItem, "SCRP_QTY"));
                    newRow["ADD_INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dr.DataItem, "ADD_INPUT_QTY"));
                    IN_BC_Table.Rows.Add(newRow);
                }

                DataTable IN_FC_Table = indataSet.Tables["IN_FC"];
                foreach (var dr in dgFCList.Rows)
                {
                    newRow = IN_FC_Table.NewRow();
                    newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dr.DataItem, "PRODID"));
                    newRow["RECYC_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dr.DataItem, "RECYC_QTY"));
                    newRow["SCRP_QTY"] = 0;
                    newRow["ADD_INPUT_QTY"] = 0;
                    IN_FC_Table.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_CPROD_LOT", "INDATA,IN_BC,IN_FC", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetNewOutLotid()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("inTable");
                inTable.Columns.Add("AREAID");
                inTable.Columns.Add("PROCID");
                inTable.Columns.Add("EQSGID");
                inTable.Columns.Add("CPROD_LOTID");
                inTable.Columns.Add("PRODID");

                DataRow newRow = inTable.NewRow();

                newRow["AREAID"] = _AreaId;
                newRow["PROCID"] = Process.CPROD;
                newRow["EQSGID"] = _EqsgId;
                newRow["CPROD_LOTID"] = _CProdLotId;
                newRow["PRODID"] = _ProdId;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_NEW_LOTID_CPROD", "inTable", "outTable", inTable);

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

        #endregion

        #region [Validation]

        private bool CanMove()
        {
            bool bRet = false;

            try
            {
                int fcRecycSum = DataTableConverter.Convert(dgFCList.ItemsSource).AsEnumerable().Sum(row => Util.NVC_Int(row.Field<object>("RECYC_QTY")));
                if (fcRecycSum > Util.NVC_Int(_DfctQty))
                {
                    //Util.Alert("재생FC수량은 불량수량을 초과할수 없습니다.");
                    Util.MessageValidation("SFU4239");
                    return bRet;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bRet;
            }


            bRet = true;
            return bRet;
        }

        private bool CanDeleteBox()
        {
            bool bRet = false;

            int idx = _Util.GetDataGridCheckFirstRowIndex(dgBoxList, "CHK");
            if (idx < 0)
            {
                //Util.Alert("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return bRet;
            }

            // ERP 전송 여부 확인.
            for (int i = 0; i < dgBoxList.Rows.Count - dgBoxList.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgBoxList, "CHK", i)) continue;

                if (!GetErpSendInfo(Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID")),
                                    Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "WIPSEQ"))))
                {
                    //Util.Alert("[{0}] 은 ERP 전송 중 입니다.\n잠시 후 다시 시도하세요.", Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID")));
                    Util.MessageValidation("SFU1283", Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID")));
                    return bRet;
                }
            }

            bRet = true;

            return bRet;
        }

        private bool CanCreateBox()
        {
            bool bRet = false;

            if (txtOutBoxQty.Text.Trim().Equals(""))
            {
                //Util.Alert("수량을 입력 하세요.");
                Util.MessageValidation("SFU1684");
                txtOutBoxQty.Focus();
                return bRet;
            }

            if (Convert.ToDecimal(txtOutBoxQty.Text) <= 0)
            {
                //Util.Alert("수량이 0보다 작습니다.");
                Util.MessageValidation("SFU1232");
                return bRet;
            }

            int sum = DataTableConverter.Convert(dgBoxList.ItemsSource).AsEnumerable().Sum(row => Util.NVC_Int(row.Field<object>("WIPQTY")));
            if (sum + Util.NVC_Int(txtOutBoxQty.Text) > Util.NVC_Int(_DfctQty))
            {
                //Util.Alert("F/C 재생수량은 입고수량를 초과할 수 없습니다.");
                Util.MessageValidation("SFU4297");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]

        private void AutoThermalPrint(string _LotId, string _cprodWrkPstnName, string _mktTypeName, string _tbPjtName, string _tbProdID, decimal _qty, string _userName)
        {
            try
            {
                // 발행..
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                dicParam.Add("LOTID", _LotId);
                dicParam.Add("CPROD_WRK_PSTN_NAME", _cprodWrkPstnName);
                dicParam.Add("MKT_TYPE_NAME", _mktTypeName);
                dicParam.Add("PJT", _tbPjtName);
                dicParam.Add("PRODID", _tbProdID);
                dicParam.Add("QTY", Util.NVC(_qty));
                dicParam.Add("USERID", _userName);
                dicParam.Add("PRINTQTY", "1");  // 발행 수                
                dicParam.Add("RE_PRT_YN", "N"); // 재발행 여부.

                LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD(dicParam);
                print.FrameOperation = FrameOperation;

                if (print != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = dicParam;
                    Parameters[1] = Process.STACKING_FOLDING;
                    Parameters[2] = _EqsgId;
                    Parameters[3] = _EqptId;
                    Parameters[4] = "N";   // 완료 메시지 표시 여부.
                    Parameters[5] = "M";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                    C1WindowExtension.SetParameters(print, Parameters);

                    print.Closed += new EventHandler(print_Closed);

                    print.ShowModal();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add();

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

        private void btnMove_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanMove())
                    return;

                // 작업완료 하시겠습니까?\r\n※작업완료후 취소가 불가합니다.
                Util.MessageConfirm("SFU4292", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Move();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool IsDataEmpty()
        {
            bool bRet = false;

            foreach (var row in dgBCList.Rows)
            {
                if (Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "RECYC_QTY")) != 0
                    || Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "ADD_INPUT_QTY")) != 0)
                    return bRet;
            }

            foreach (var row in dgFCList.Rows)
            {
                if (Util.NVC_Int(DataTableConverter.GetValue(row.DataItem, "RECYC_QTY")) != 0)
                    return bRet;
            }

            bRet = true;
            return bRet;
        }

        private string GetCProdStat()
        {
            DataTable dt = new DataTable("INDATA");
            dt.Columns.Add("CPROD_LOTID");

            DataRow dr = dt.NewRow();
            dr["CPROD_LOTID"] = _CProdLotId;
            dt.Rows.Add(dr);

            DataTable tmpTable = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_LOT_LIST", "INDATA", "OUTDATA", dt);
            return Util.NVC(tmpTable.Rows[0]["CPROD_LOT_STAT"].ToString());
        }


        private void btnBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgBoxList.Rows.Count - dgBoxList.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgBoxList, "CHK", i)) continue;

                AutoThermalPrint(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID").ToString(), _CProdWrkPstnName, _MktTypeName, _PjtName, _ProdId, Util.NVC_Int(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "WIPQTY")),
                    Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "INSUSER_NAME")));
            }
        }

     
    }
}
