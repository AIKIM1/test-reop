/*************************************************************************************
 Created Date : 2017.10.13
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - C 생산 관리 화면 - 재작업 상세보기
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.13  CNS 고현영S : 생성
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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_014_HIST_DETAIL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_014_HIST_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CProdLot = String.Empty;
        private string _PjtName = String.Empty;
        private string _ProdID = String.Empty;
        private string _EqsgId = string.Empty;
        private string _EqsgName = string.Empty;
        private string _DfctQty = string.Empty;
        private string _MktTypeName = string.Empty;

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
        private void InitTextBox()
        {
            tbCProdLot.Text = _CProdLot;
            tbPjtName.Text = _PjtName;
            tbTransferLine.Text = _EqsgName;
            tbDfctQty.Text = _DfctQty;
            tbProdID.Text = _ProdID;
            tbMktTypeName.Text = _MktTypeName;
        }

        public ASSY003_014_HIST_DETAIL()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if ( tmps != null && tmps.Length == 6 )
                {
                    _CProdLot = Util.NVC(tmps[0]);
                    _PjtName = Util.NVC(tmps[1]);
                    _EqsgName = Util.NVC(tmps[2]);
                    _DfctQty = Util.NVC(tmps[3]);
                    _ProdID = Util.NVC(tmps[4]);
                    _MktTypeName = Util.NVC(tmps[5]);
                }
                else
                {
                    _CProdLot = "";
                    _PjtName = "";
                    _ProdID = "";
                    _EqsgName = "";
                    _DfctQty = "";
                    _ProdID = "";
                    _MktTypeName = "";
                }

                InitTextBox();

                InitDataGridBCList();

                InitDataGridFCList();

                GetDgBoxList();

                ApplyPermissions();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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
                newRow["PR_LOTID"] = _CProdLot;

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

        private void InitDataGridBCList()
        {
            try
            {
                DataTable BCDATA = new DataTable("BCDATA");
                BCDATA.Columns.Add("CPROD_LOTID");
                BCDATA.Columns.Add("PRODID");

                DataRow newRow = BCDATA.NewRow();
                newRow["CPROD_LOTID"] = _CProdLot;
                newRow["PRODID"] = _ProdID;
                BCDATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_BC_CPROD_WRK_RSLT", "INDATA", "OUTDATA", BCDATA, (searchResult, searchException) =>
                {
                    try
                    {
                        if ( searchException != null )
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgBCList, searchResult, FrameOperation, false);

                    }
                    catch ( Exception ex )
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
            catch ( Exception ex )
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void InitDataGridFCList()
        {

            try
            {
                DataTable FCDATA = new DataTable("FCDATA");
                FCDATA.Columns.Add("CPROD_LOTID");
                FCDATA.Columns.Add("PRODID");

                DataRow newRow = FCDATA.NewRow();
                newRow["CPROD_LOTID"] = _CProdLot;
                newRow["PRODID"] = _ProdID;
                FCDATA.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FC_CPROD_WRK_RSLT", "INDATA", "OUTDATA", FCDATA, (searchResult, searchException) =>
                {
                    try
                    {
                        if ( searchException != null )
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgFCList, searchResult, FrameOperation, false);

                    }
                    catch ( Exception ex )
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
            catch ( Exception ex )
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

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
                    Parameters[3] = string.Empty;
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


        private void print_Closed(object sender, EventArgs e)
        {
            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD window = sender as LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_BOX_CPROD;

            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void btnBoxPrint_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgBoxList.Rows.Count - dgBoxList.BottomRows.Count; i++)
            {
                if (!_Util.GetDataGridCheckValue(dgBoxList, "CHK", i)) continue;
                AutoThermalPrint(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "LOTID").ToString(),
                                 DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "CPROD_WRK_PSTN_NAME").ToString(),
                                 _MktTypeName,
                                 _PjtName,
                                 _ProdID,
                                 Util.NVC_Int(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "WIPQTY")),
                                 Util.NVC(DataTableConverter.GetValue(dgBoxList.Rows[i].DataItem, "WRK_USERNAME")));
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

      
    }
}
