using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_336.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_336 : UserControl, IWorkArea
    {
        #region Declare and Constructor
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

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
        public COM001_336()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            initCombo();

            dtpRcvIssDateFrom.SelectedDateTime = System.DateTime.Now.AddMonths(-1);
            dtpRcvIssDateTo.SelectedDateTime = System.DateTime.Now.AddDays(1);

            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddMonths(-1);
            dtpDateTo.SelectedDateTime = System.DateTime.Now.AddDays(1);
        }

        private void initCombo()
        {
            combo.SetCombo(cboMTGR, CommonCombo.ComboStatus.ALL, sCase: "cboMTGRID", sFilter:null);
            combo.SetCombo(cboRcvIssMTGR, CommonCombo.ComboStatus.ALL, sCase: "cboMTGRID", sFilter: null);
        }
        #endregion

        #region 입고상태 자재 검색
        /// <summary>
        /// Dry Room에 입고되어 있는 자재를 검색한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgCurrWH.ItemsSource = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("MTGRID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));
                dt.Columns.Add("END_DTTM", typeof(string));
                dt.Columns.Add("MTRLID", typeof(string));
                dt.Columns.Add("ISOVERTIME", typeof(string));


                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["MTGRID"] = Convert.ToString(cboRcvIssMTGR.SelectedValue).Equals("") ? null : Convert.ToString(cboRcvIssMTGR.SelectedValue); 
                row["STRT_DTTM"] = dtpRcvIssDateFrom.SelectedDateTime.ToShortDateString();
                row["END_DTTM"] = dtpRcvIssDateTo.SelectedDateTime.ToShortDateString();
                row["MTRLID"] = txtMtrlID2.Text.Equals("") ? null : txtMtrlID2.Text;
                row["ISOVERTIME"] = "N";

                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_DRYROOM_STOCK_LIST", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_DRYROOM_STOCK_LIST", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.GridSetData(dgCurrWH, bizResult, FrameOperation, false);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        #endregion 

        #region 입고 처리 대상 검색
        /// <summary>
        /// 입력받은 입고처리 대상 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRcvSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtRcvMtrlID.Equals(null) || txtRcvMtrlID.Equals(""))
                    return;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SLOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SLOTID"] = txtRcvMtrlID.Text.Equals("") ? null : txtRcvMtrlID.Text;

                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_DRYROOM_RCV_TARGET", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_DRYROOM_RCV_TARGET", bizException.Message, bizException.ToString());
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2060");
                            return;
                        }

                        DataTable mergeDt = DataTableConverter.Convert(dgRcvList.ItemsSource);

                        if (bizResult.Rows.Count > 1)
                        {
                            if (mergeDt != null && mergeDt.Rows.Count > 0)
                            {
                                foreach (DataRow dr in bizResult.Rows)
                                {
                                    if (mergeDt.Select("S_BOX_ID = '" + dr["S_BOX_ID"].ToString() + "' AND PLLT_ID = '" + dr["PLLT_ID"].ToString() + "'").Count() == 0)
                                    {
                                        DataRow newRow = mergeDt.NewRow();

                                        foreach (DataColumn dc in mergeDt.Columns)
                                        {
                                            newRow[dc.ColumnName] = dr[dc.ColumnName];
                                        }

                                        mergeDt.Rows.Add(newRow);
                                    }
                                }
                            }
                            else
                            {
                                mergeDt.Merge(bizResult);
                            }
                        }
                        else
                        {
                            if (mergeDt != null)
                            {
                                foreach (DataRow dr in mergeDt.Rows)
                                {
                                    if (dr["S_BOX_ID"].ToString() == bizResult.Rows[0]["S_BOX_ID"].ToString() && dr["PLLT_ID"].ToString() == bizResult.Rows[0]["PLLT_ID"].ToString())
                                    {
                                        Util.MessageValidation("SFU1914");
                                        return;
                                    }
                                }
                                mergeDt.Merge(bizResult);
                            }
                        }

                        Util.GridSetData(dgRcvList, mergeDt, FrameOperation, false);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                txtRcvMtrlID.Clear();
            }
        }
        #endregion

        #region 출고 처리 대상 검색
        /// <summary>
        /// 입력받은 출고처리 대상 검색
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIssSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtIssMtrlID.Equals(null) || txtIssMtrlID.Equals(""))
                    return;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SLOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SLOTID"] = txtIssMtrlID.Text.Equals("") ? null : txtIssMtrlID.Text;

                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_DRYROOM_ISS_TARGET", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_DRYROOM_ISS_TARGET", bizException.Message, bizException.ToString());
                            return;
                        }

                        if (bizResult.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2060");
                            return;
                        }

                        DataTable mergeDt = DataTableConverter.Convert(dgIssList.ItemsSource);

                        if (bizResult.Rows.Count > 1)
                        {
                            if (mergeDt != null && mergeDt.Rows.Count > 0)
                            {
                                foreach (DataRow dr in bizResult.Rows)
                                {
                                    if (mergeDt.Select("S_BOX_ID = '" + dr["S_BOX_ID"].ToString() + "' AND PLLT_ID = '" + dr["PLLT_ID"].ToString() + "'").Count() == 0)
                                    {
                                        DataRow newRow = mergeDt.NewRow();

                                        foreach (DataColumn dc in mergeDt.Columns)
                                        {
                                            newRow[dc.ColumnName] = dr[dc.ColumnName];
                                        }

                                        mergeDt.Rows.Add(newRow);
                                    }
                                }
                            }
                            else
                            {
                                mergeDt.Merge(bizResult);
                            }
                        }
                        else
                        {
                            if (mergeDt != null)
                            {
                                foreach (DataRow dr in mergeDt.Rows)
                                {
                                    if (dr["S_BOX_ID"].ToString() == bizResult.Rows[0]["S_BOX_ID"].ToString() && dr["PLLT_ID"].ToString() == bizResult.Rows[0]["PLLT_ID"].ToString())
                                    {
                                        Util.MessageValidation("SFU1914");
                                        return;
                                    }
                                }
                                mergeDt.Merge(bizResult);
                            }
                        }

                        Util.GridSetData(dgIssList, mergeDt, FrameOperation, false);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
            finally
            {
                txtIssMtrlID.Clear();
            }
        }
        #endregion

        #region 입고 처리 자재 입력 키 이벤트
        /// <summary>
        /// 입고 처리 자재 입력 키 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRcvMtrlId_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
                this.btnRcvSearch_Click(null, null);
        }
        #endregion

        #region 출고 처리 자재 입력 키 이벤트
        /// <summary>
        /// 출고 처리 자재 입력 키 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtIssMtrlId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.btnIssSearch_Click(null, null);
        }
        #endregion

        #region 입고 처리 리스트 초기화
        /// <summary>
        /// 입고처리 리스트를 초기화 한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRcvClear_Click(object sender, RoutedEventArgs e)
        {
            dgRcvList.ItemsSource = null;
        }
        #endregion

        #region 출고 처리 리스트 초기화
        /// <summary>
        /// 출고처리 리스트를 초기화 한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIssClear_Click(object sender, RoutedEventArgs e)
        {
            dgIssList.ItemsSource = null;
        }
        #endregion

        #region 입고 처리 리스트 건별 삭제
        /// <summary>
        /// 입고처리 리스트의 체크되어 있는 항목을 제거 후 Grid에 바인딩한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRcvDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable gvDt = DataTableConverter.Convert(dgRcvList.ItemsSource);
                DataTable newGridView = null;
                if (gvDt.Select("CHK = '0'").Count() != 0)
                {
                    newGridView = gvDt.Select("CHK = '0'").CopyToDataTable();
                    Util.GridSetData(dgRcvList, newGridView, FrameOperation, false);
                }
                else
                {
                    dgRcvList.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        #region 출고 처리 리스트 건별 삭제
        /// <summary>
        /// 출고처리 리스트의 체크되어 있는 항목을 제거 후 Grid에 바인딩한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIssDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable gvDt = DataTableConverter.Convert(dgIssList.ItemsSource);
                DataTable newGridView = null;
                if (gvDt.Select("CHK = '0'").Count() != 0)
                {
                    newGridView = gvDt.Select("CHK = '0'").CopyToDataTable();
                    Util.GridSetData(dgIssList, newGridView, FrameOperation, false);
                }
                else
                {
                    dgIssList.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 출고 처리
        /// <summary>
        /// 출고 처리 리스트의 자재를 Dry Room에서 출고처리한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnIssProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("S_BOX_ID", typeof(string));
                RQSTDT.Columns.Add("PLLT_ID", typeof(string));

                DataTable gvDt = DataTableConverter.Convert(dgIssList.ItemsSource);

                foreach (DataRow dr in gvDt.Rows)
                {
                    DataRow newdr = RQSTDT.NewRow();
                    newdr["S_BOX_ID"] = dr["S_BOX_ID"].ToString();
                    newdr["PLLT_ID"] = dr["PLLT_ID"].ToString();
                    RQSTDT.Rows.Add(newdr);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_DRYROOM_ISS", "RQSTDT", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1270");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.btnIssClear_Click(null, null);
                        this.btnSearch_Click(null, null);
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 입고 처리
        /// <summary>
        /// 입고 대상 리스트의 자재를 Dry Room에 입고처리한다.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRcvProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("S_BOX_ID", typeof(string));
                RQSTDT.Columns.Add("PLLT_ID", typeof(string));

                DataTable gvDt = DataTableConverter.Convert(dgRcvList.ItemsSource);

                foreach (DataRow dr in gvDt.Rows)
                {
                    DataRow newdr = RQSTDT.NewRow();
                    newdr["S_BOX_ID"] = dr["S_BOX_ID"].ToString();
                    newdr["PLLT_ID"] = dr["PLLT_ID"].ToString();
                    RQSTDT.Rows.Add(newdr);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_DRYROOM_RCV", "RQSTDT", null, RQSTDT, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        Util.MessageInfo("SFU1270");
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.btnRcvClear_Click(null, null);
                        this.btnSearch_Click(null, null);
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 입출고 이력 조회 버튼 이벤트
        /// <summary>
        /// 입출고 이력 조회 버튼 이벤트 발생 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dgRcvIssHist.ItemsSource = null;

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("MTGRID", typeof(string));
                dt.Columns.Add("STRT_DTTM", typeof(string));
                dt.Columns.Add("END_DTTM", typeof(string));
                dt.Columns.Add("MTRLID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["MTGRID"] = Convert.ToString(cboMTGR.SelectedValue).Equals("") ? null : Convert.ToString(cboMTGR.SelectedValue);
                row["STRT_DTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                row["END_DTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                row["MTRLID"] = txtMtrlID.Text.Equals("") ? null : txtMtrlID.Text;
                dt.Rows.Add(row);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_PRD_SEL_DRYROOM_STOCK_HIST", "RQSTDT", "RSLTDT", dt, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.AlertByBiz("DA_PRD_SEL_DRYROOM_STOCK_HIST", bizException.Message, bizException.ToString());
                            return;
                        }

                        Util.GridSetData(dgRcvIssHist, bizResult, FrameOperation, false);

                    }
                    catch (Exception ex)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(ex.Message, ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion
    }
}
