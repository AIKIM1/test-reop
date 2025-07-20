/*************************************************************************************
 Created Date : 2022.01.24
      Creator : 이정미
   Decription : 조립확정/활성화 입고 수량
--------------------------------------------------------------------------------------
 [Change History]
  2022.01.24  이정미 : Initial Created
  2022.04.12  이정미 : 콤보박스 수정 - 생산라인 선택시 모델ID 동기화
  2022.06.03  이정미 : 콤보박스 수정 - ALL 표시
  2022.07.20  이정미 : 동 콤보박스 추가 및 수정, 조회 조건 추가
  2022.07.26  이정미 : dgGMESDetailList Summary 추가
  2024.02.20  이한학 : Tab 메뉴 전환 및 신규 Tab 메뉴 조립확정/활성화 입고수량 (모델/PKG Lot별) 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;


namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_124 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        private string strConfirmDate = "";
        private string strLine = "";
        private string strModel = "";
        Util _Util = new Util();

        public FCS001_124()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
                  
            //동
            C1ComboBox[] cboAreaChild = { cboLine };
            _combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.SELECT, sCase: "ALLAREA", cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboLineParent = { cboArea };
            C1ComboBox[] cboLineChild = { cboModel }; 
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_LINE", cbParent: cboLineParent,cbChild: cboLineChild);
           
            //모델
            C1ComboBox[] cboModelParent = { cboLine };  
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);
        }

        #endregion


        #region [Method]

        private void GetList()
        {
            try
            {

                Util.gridClear(dgGMES);
                Util.gridClear(dgGMESDetailList);
                txtAssemConfirmDate.Text = string.Empty;
                txtProdLine.Text = string.Empty;
                txtModelId.Text = string.Empty;

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("START_DATE", typeof(string));
                dtRqst.Columns.Add("END_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));  //LINEID
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string)); //MODELID
                dtRqst.Columns.Add("FORM_EQSGID", typeof(string));
                dtRqst.Columns.Add("MODEL_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["START_DATE"] = Util.GetCondition(dtpScFromDate);
                dr["END_DATE"] = Util.GetCondition(dtpScToDate);
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["FORM_EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["MODEL_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_GET_ASSY_CNFM_FCS_INPUT_CNT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgGMES, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "EQSGID", "MDLLOT_ID" };
                _Util.SetDataGridMergeExtensionCol(dgGMES, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
         }

        private void GetListTabSum()
        {
            try
            {

                Util.gridClear(dgGMESTabSum);

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("START_DATE", typeof(string));
                dtRqst.Columns.Add("END_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["START_DATE"] = Util.GetCondition(dtpScFromDateTab);
                dr["END_DATE"] = Util.GetCondition(dtpScToDateTab);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSY_CNFM_FCS_INPUT_LIST_CNT_UI", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgGMESTabSum, dtRslt, FrameOperation, true);

                string[] sColumnName = new string[] { "PRODID" };
                _Util.SetDataGridMergeExtensionCol(dgGMESTabSum, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetListTab()
        {
            try
            {

                Util.gridClear(dgGMESTab);

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("START_DATE", typeof(string));
                dtRqst.Columns.Add("END_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["START_DATE"] = Util.GetCondition(dtpScFromDateTab);
                dr["END_DATE"] = Util.GetCondition(dtpScToDateTab);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSY_CNFM_FCS_INPUT_LIST_UI", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgGMESTab, dtRslt, FrameOperation, true);

            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// DataTable에서 특정 컬럼의 Value값을 Total로 리턴함.
        /// </summary>
        /// <param name="dt">Target DataTable</param>
        /// <param name="sColumnName">값을 바꾸고자 하는 Column명</param>
        /// <returns></returns>
        private DataTable ChangeValueToTotal(DataTable dt, string sColumnName)
        {
            if (dt == null || dt.Rows.Count < 1) { return dt; }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i][sColumnName] = ObjectDic.Instance.GetObjectName("TOTAL");
            }

            return dt;
        }

        #endregion


        #region [Event]
        private void dgGMES_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgGMES.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgGMESDetailList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgGMESDetailList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearch_Click_Tab(object sender, RoutedEventArgs e)
        {
            GetListTabSum();
            GetListTab();
        }        

        private void dgGMES_CellDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Util.gridClear(dgGMESDetailList);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgGMES.GetCellFromPoint(pnt);


                if(!cell.Column.Name.Equals("GAP_CNT"))
                {
                    return;
                }

                strConfirmDate = Util.NVC(DataTableConverter.GetValue(dgGMES.Rows[cell.Row.Index].DataItem, "ASSY_CNFM_DATE")).ToString(); 
                strLine = Util.NVC(DataTableConverter.GetValue(dgGMES.Rows[cell.Row.Index].DataItem, "EQSGID")).ToString(); 
                strModel = Util.NVC(DataTableConverter.GetValue(dgGMES.Rows[cell.Row.Index].DataItem, "MDLLOT_ID")).ToString(); 

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("ASSY_CNFM_DATE", typeof(string)); 
                dtRqst.Columns.Add("MODEL_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string)); 

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                dr["ASSY_CNFM_DATE"] = strConfirmDate;
                dr["MODEL_ID"] = strModel;
                dr["EQSGID"] = strLine;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSY_CNFM_TRAY_NOT_INPUT_UI", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgGMESDetailList, dtRslt, FrameOperation, true);

                txtAssemConfirmDate.Text = strConfirmDate;
                txtProdLine.Text = strLine;
                txtModelId.Text = strModel;

                // Summary 추가
                if (dtRslt.Rows.Count > 0)
                {
                    DataTable dtsum = new DataTable();

                    string[] grCode = new string[1] { "ASSY_CNFM_DATE" };
                    string[] colName = dtRslt.Columns.Cast<DataColumn>()     
                                                                            .Where(x => ! (x.ColumnName.Contains("ASSY_CNFM_DATE")) && !(x.ColumnName.Contains("CONFIRM_TIME")) 
                                                                                     && !(x.ColumnName.Contains("EQSGID")) && !(x.ColumnName.Contains("LOT_ID")) && !(x.ColumnName.Contains("PROD_CD"))
                                                                                     && !(x.ColumnName.Contains("CSTID")) &&  !(x.ColumnName.Contains("SPCL_FLAG")) && !(x.ColumnName.Contains("SPCL_NOTE"))
                                                                                     && !(x.ColumnName.Contains("SPCL_INSUSER")) && !(x.ColumnName.Contains("MODEL_ID"))
                                                                                     && !(x.ColumnName.Contains("SPCL_RSNCODE")) && !(x.ColumnName.Contains("SPCL_GR_ID")))
                                                                        .Select(x => x.ColumnName)
                                                                        .ToArray();
                    dtsum = Util.GetGroupBySum(dtRslt, colName, grCode, true);
                    ChangeValueToTotal(dtsum, "ASSY_CNFM_DATE");
                    dtRslt.Merge(dtsum);

                    Util.GridSetData(dgGMESDetailList, dtRslt, FrameOperation, true);     
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }

        private void dgGMES_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
           {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    
                    if (e.Cell.Row.Index == dgGMES.Rows.Count - 1)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                    }

                    else
                    {
                        if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "GAP_CNT").ToString() != "0")
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }

                        else
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                }));   
          }

            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGMESDetailList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;
                    try
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        if (e.Cell.Row.Index == dgGMESDetailList.Rows.Count - 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        }

                        else
                        {
                            e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }


                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgGMESTabSum_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;
                    try
                    {
                        if (e.Cell.Presenter == null)
                        {
                            return;
                        }

                        if (e.Cell.Row.Index == dgGMESTabSum.Rows.Count - 1)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Aquamarine);
                        }

                        else
                        {
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "GRP_PILOT_PROD_DIVS_CODE").ToString() != "0")
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Azure);
                                //e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 2).Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                //e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 2).Presenter.Foreground = new SolidColorBrush(Colors.Red);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }


                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

    }
}


