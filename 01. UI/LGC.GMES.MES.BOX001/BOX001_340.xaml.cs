/*************************************************************************************
 Created Date : 2024.07.02
      Creator : 
   Decription : 입출고관리 > Cell입고(NJ포장)
--------------------------------------------------------------------------------------
 [Change History]
       





**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.BOX001
{    
    public partial class BOX001_340 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;

        public BOX001_340()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
        }


        #endregion

        #region Event

        /// <summary>
        /// 출하그리드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgTagetListCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                    return;
                if (dgTargetList == null)
                    return;

                if (e.Cell.Column.Name == "RECEIVABLE_FLAG")
                {
                    C1.WPF.DataGrid.DataGridCell chkCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["CHK"].Index);
                    C1.WPF.DataGrid.DataGridCell issIdCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["RCV_ISS_ID"].Index);
                    C1.WPF.DataGrid.DataGridCell plltIdCell = dgTargetList.GetCell(e.Cell.Row.Index, dgTargetList.Columns["PALLETID"].Index);


                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        DataTableConverter.SetValue(dgTargetList.Rows[e.Cell.Row.Index].DataItem, "CHK", 0);

                        if (!(dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, chkCell.Column.Index).Presenter.IsEnabled = false;
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, issIdCell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        }
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, plltIdCell.Column.Index).Presenter.FontWeight = FontWeights.Bold;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// 사용자 직접입력 PALLETID 입력 후 엔터
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    dgTargetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                    dgTargetList.LoadedCellPresenter += dgTagetListCellPresenter;
                    getTagetPalletInfo();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTargetList.Rows.Count <= 0)
            {
                Util.MessageInfo("SFU2093"); //취소 대상이 없습니다. PALLET ID를 입력해주세요.
                return;
            }

            //전체 취소 하시겠습니까?
            Util.MessageConfirm("SFU1885", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgTargetList);

                    txtPalletID.Text = "";
                }
            });
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                if (Util.NVC(dr["RECEIVABLE_FLAG"]).Equals("Y"))
                {
                    dr["CHK"] = true;
                }
            }
            dgTargetList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList);
        }

        /// <summary>
        /// 입고버튼클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTargetList.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU1796"); //입고 대상이 없습니다. PALLETID를 입력 하세요.
                    return;
                }

                int row = (from C1.WPF.DataGrid.DataGridRow rows in dgTargetList.Rows
                           where rows.DataItem != null
                                 && rows.Type == DataGridRowType.Item
                                 && DataTableConverter.GetValue(rows.DataItem, "CHK").GetString() == "True"
                           select rows).Count();

                if (row <= 0)
                {
                    Util.MessageInfo("SFU1636");
                    return;
                }

                if (this.ChkRcvable())
                {
                    Util.MessageInfo("SFU3722");     // 입고 가능한 상태가 아닙니다.
                    return;
                }

                // OCV DATA가 없는 CELL이 존재합니다. 입고하시겠습니까?"
                Util.MessageConfirm("SFU2073", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        setWarehousingUnitPallet(); //입고처리
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// 이력조회 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dtDateCompare()) return;

                getWareHousingData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// 이력 에셀 다운로더
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion



        #region Method
        /// <summary>
        /// iWMS 출고 조회
        /// </summary>
        private void getTagetPalletInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["PALLETID"] = txtPalletID.Text;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_RECEIVE_PRODUCT_NJ", "INDATA", "OUTDATA", RQSTDT);
                    
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["RECEIVABLE_FLAG"].Equals("Y"))
                    {
                        dtResult.Rows[i]["CHK"] = "True";
                    }
                }
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                dtBefore.Merge(dtResult);

                dgTargetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                txtPalletID.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setWarehousingUnitPallet()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                var serWareHousingList = dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true);

                DataTable dtPallet = serWareHousingList.CopyToDataTable();

                loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                for (int i = 0; i < dtPallet.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["PALLETID"])) || string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"])))
                    {
                        Util.MessageInfo("SFU3256");
                        return;
                    }

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("SRCTYPE", typeof(string));
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    INDATA.Columns.Add("EQSGID", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                    DataRow drINDATA = null;
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["AREAID"] = Util.NVC(dtPallet.Rows[i]["TO_AREAID"]);
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);

                    INDATA.Rows.Add(drINDATA);

                    DataTable RCV_ISS = new DataTable();
                    RCV_ISS.TableName = "RCV_ISS";
                    RCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                    RCV_ISS.Columns.Add("PALLETID", typeof(string));

                    DataRow drRCV_ISS = null;
                    drRCV_ISS = RCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);
                    drRCV_ISS["PALLETID"] = Util.NVC(dtPallet.Rows[i]["PALLETID"]);
                    RCV_ISS.Rows.Add(drRCV_ISS);

                    DataSet dsIndata = new DataSet();
                    dsIndata.Tables.Add(INDATA);
                    dsIndata.Tables.Add(RCV_ISS);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_CELL_PLLT", "INDATA,RCV_ISS", "OUTDATA", dsIndata);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        dt.AcceptChanges();

                        foreach (DataRow drDel in dt.Rows)
                        {
                            if (drDel["PALLETID"].ToString() == dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].GetString())
                            {
                                drDel.Delete();
                                break;
                            }
                        }

                        dt.AcceptChanges();

                        Util.GridSetData(dgTargetList, dt, FrameOperation);

                    }

                }

                Util.MessageInfo("SFU1412");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                loadingIndicator.Visibility = Visibility.Collapsed;

            }
        }

        /// <summary>
        /// 입고 이력조회
        /// </summary>
        private void getWareHousingData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["PALLETID"] = Util.IsNVC(txtHisPalletID.Text)? null : Util.NVC(txtHisPalletID.Text);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_PRODUCT_END_NJ", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Funct

        /// <summary>
        /// 입고일자 Validate
        /// </summary>
        /// <returns></returns>
        private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+7);
                    //조회기간은 7일을 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3567");
                    return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ChkRcvable()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("RECEIVABLE_FLAG = 'N' AND CHK = 'True'").Length > 0) // 입고 가능 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        #endregion

        /***********************************************************************************************/

        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void cboTagetRoute_PreviewTouchMove(object sender, TouchEventArgs e)
        {

        }

        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgTagetList_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgTargetList_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void Ocv_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cboSearchProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }

        private void dgSearchResultList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        
    }
}

