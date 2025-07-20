/*************************************************************************************
 Created Date : 2018.04.28
      Creator : 
   Decription : 활성화 폴리머 재공 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.04.28  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_232 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
      

        public COM001_232()
        {
            InitializeComponent();
          
          
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
       

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
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnRoute);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitCombo();

            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }


        //화면내 combo 셋팅
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcess, cboPkgLine };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboEquipmentSegment};
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, sCase: "POLYMER_PROCESS", cbChild: cboLineChild, cbParent: cboLineParent);
            cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

            //라인
            C1ComboBox[] cboProcessParent = { cboArea, cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");


            //PKGLINE
            C1ComboBox[] cboPKGLINEParent = { cboArea};
            String[] sFilterProcess = { "A9000"};
            _combo.SetCombo(cboPkgLine, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcess, cbParent: cboPKGLINEParent, sCase: "PKG_LINE");

            String[] sFilterQmsRequest = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest, sCase: "COMMCODES");


        }

        #endregion

        #region Event

        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 마스터 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetStock();
        }

        #endregion

        #region 마스터 스프레드 색 변경 : dgSummary_LoadedCellPresenter()
        /// <summary>
        /// 색변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {


                    //link 색변경
                    if (e.Cell.Column.Name.Equals("PRODID") || e.Cell.Column.Name.Equals("PROCNAME"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIP_QLTY_TYPE_CODE")).Equals("N"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            //e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));

                        }


                    }
                }





            }));
        }
        #endregion

        #region 상세조회  : dgSummary_MouseDoubleClick()

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                GetStockDetail(dg);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region 조회시 프로그래스바 실행 : btnSearch_PreviewMouseDown()
        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #region 상세조회시 프로그래스바 : dgSummary_PreviewMouseDoubleClick()
        private void dgSummary_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }
        #endregion

        #endregion

        #region Method

        #region 조회 : GetStock()
        /// <summary>
        /// 조회 Method
        /// </summary>
        private void GetStock()
        {
            try
            {

                string sBizName = "DA_PRD_SEL_WIP_LIST_PC";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQSGID_PKG", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTRT", typeof(string));
               
                DataRow dr = dtRqst.NewRow();
                DataTable dtRslt = new DataTable();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].ToString().Equals("")) return;
                dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboProduct);
                dr["PROCID"] = Util.GetCondition(cboProcess, "SFU1459");//공정은 필수입니다.
                if (dr["PROCID"].ToString().Equals("")) return;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["EQSGID_PKG"] = Util.GetCondition(cboPkgLine, bAllNull: true);  
                dr["PRJT_NAME"] = txtPrjtName.Text == string.Empty ? null : txtPrjtName.Text;  
                dr["PRODID"] = txtProdId.Text == string.Empty ? null : txtProdId.Text;  
                dr["LOTRT"] = txtLotRt.Text == string.Empty ? null : txtLotRt.Text;  

                dtRqst.Rows.Add(dr);
                dtRslt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSummary, dtRslt, FrameOperation, true);
                Util.gridClear(dgLotList);
                if (dtRslt.Rows.Count > 0)
                {
                    string[] sColumnName = new string[] { "PROCNAME", "PRJT_NAME" };
                    _Util.SetDataGridMergeExtensionCol(dgSummary, sColumnName, DataGridMergeMode.VERTICAL);

                    dgSummary.GroupBy(dgSummary.Columns["PROCNAME"], DataGridSortDirection.None);
                    dgSummary.GroupRowPosition = DataGridGroupRowPosition.AboveData;

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRJT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("합계") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PRODID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MKT_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WIP_QLTY_TYPE_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WIP_UNIT_NAME"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("") } });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["WAIT_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["PROC_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["END_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["MOVE_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["HOLD_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSummary.Columns["SUM_CELL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });

                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 상세조회 : GetStockDetail(), GetDetailLot()
        /// <summary>
        /// 상세조회 파라미터 셋팅
        /// </summary>
        /// <param name="dg"></param>
        private void GetStockDetail(C1DataGrid dg)
        {
            if (dg.CurrentColumn.Name.Equals("PRODID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCNAME")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRJT_NAME")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PRODID")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "MKT_TYPE_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WIP_QLTY_TYPE_CODE")),
                             Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "WIP_UNIT")),
                             dg.CurrentRow.Index
                             );

            }
            else if (dg.CurrentColumn.Name.Equals("PROCNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
            {
                GetDetailLot(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "PROCID")), Util.NVC(DataTableConverter.GetValue(dgSummary.CurrentRow.DataItem, "PROCNAME")), "","", "", "", "", dg.CurrentRow.Index);

            }
        }
        /// <summary>
        /// 상세 조회 
        /// </summary>
        /// <param name="sProcId"></param>
        /// <param name="sProcname"></param>
        /// <param name="sprjt"></param>
        /// <param name="sProdId"></param>
        /// <param name="sMkttype"></param>
        /// <param name="sQlty"></param>
        /// <param name="sUnit"></param>
        /// <param name="Row"></param>
        private void GetDetailLot(string sProcId, string sProcname, string sprjt,  string sProdId, string sMkttype, string sQlty, string sUnit,  int Row)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("WIP_UNIT", typeof(string));
   
                DataRow dr = dtRqst.NewRow();
                if (sProcname == string.Empty && Row != 2)
                {
                    //공정 전체 - # 클릭시
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSummary.Rows[Row + 1].DataItem, "PROCID"));
                }
                else if (sProcname != string.Empty && sProdId == string.Empty)
                {
                    //공정 전체 - 공정명 클릭시
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROCID"] = sProcId;
                }
                else
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PROCID"] = sProcId;
                    dr["PRJT_NAME"] = sprjt == string.Empty? null : sprjt;
                    dr["PRODID"] = sProdId == string.Empty ? null : sProdId;
                    dr["MKT_TYPE_CODE"] = sMkttype == string.Empty ? null : sMkttype;
                    dr["WIP_QLTY_TYPE_CODE"] = sQlty == string.Empty ? null : sQlty;
                    dr["WIP_UNIT"] = sUnit == string.Empty ? null : sUnit;
                }
               dtRqst.Rows.Add(dr);
                DataTable dtRslt = new DataTable();
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_LIST_DETAIL_PC", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgLotList, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion



      


    }
}
