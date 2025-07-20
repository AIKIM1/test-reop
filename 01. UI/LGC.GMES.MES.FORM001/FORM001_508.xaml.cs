/*************************************************************************************
 Created Date : 2018.11.06
      Creator : 이제섭
   Decription : FCS I/F Error 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.11.06  이제섭 : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Data;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Collections.Generic;


namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_508 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
       // private MessageBoxResult DialogResult;

        public FORM001_508()
        {
            InitializeComponent();                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public MessageBoxResult DialogResult { get; private set; }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region 콤보박스

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { "SUBLOT_STAT_CODE" };
            _combo.SetCombo(cbosublotstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "AREA");

        }
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
        }

        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }

        #endregion


        #region LoadingIndicator
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

        #region Event

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetData();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            RunProcedure();
        }

        #endregion

        #region BizCall
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_FCS_ERROR_HIST_AUTO
        /// </summary>
        private void GetData()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable intable = new DataTable("RQSTDT");
                intable.Columns.Add("LANGID", typeof(string));
                intable.Columns.Add("SUBLOT_STAT_CODE", typeof(string));
                intable.Columns.Add("AREAID", typeof(string));
                intable.Columns.Add("FROMDATE", typeof(string));
                intable.Columns.Add("TODATE", typeof(string));

                DataRow dr = intable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOT_STAT_CODE"] = string.IsNullOrWhiteSpace(cbosublotstat.SelectedValue.ToString()) ? null : cbosublotstat.SelectedValue.ToString();
                dr["AREAID"] = string.IsNullOrWhiteSpace(cboArea.SelectedValue.ToString()) ? null : cboArea.SelectedValue.ToString();
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd");

                intable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_FCS_ERROR_HIST_AUTO", "RQSTDT", "RSLTDT", intable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgSearhResult, bizResult, FrameOperation, true);

                        txtRowCount.Value = bizResult.Rows.Count;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Update  
        /// </summary>
        private void RunProcedure()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SUBLOTID");
                inTable.Columns.Add("USERID");

                for (int i = 0; i < dgSearhResult.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "SUBLOTID")).Trim();
                        newRow["USERID"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_LOT_GNRT_FLAG_AUTO", "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            // 정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                            GetData();
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
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            
        }

        #endregion


        private void dgSearchResult_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearhResult.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgSearhResult.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgSearhResult.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearhResult.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearhResult.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            dgSearhResult.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            {
                DataTable dtTo = DataTableConverter.Convert(dgSearhResult.ItemsSource);

                if (dtTo.Columns.Count == 0)
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                }

                DataRow dr = dtTo.NewRow();
                foreach (DataColumn dc in dtTo.Columns)
                {
                    if (dc.DataType.Equals(typeof(Boolean)))
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    else
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                    }
                }

                dtTo.Rows.Add(dr);
                dgSearhResult.ItemsSource = DataTableConverter.Convert(dtTo);

                DataRow[] drUnchk = DataTableConverter.Convert(dgSearhResult.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }

            }
            else//체크 풀릴때
            {
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                DataTable dtTo = DataTableConverter.Convert(dgSearhResult.ItemsSource);

                dgSearhResult.ItemsSource = DataTableConverter.Convert(dtTo);
            }
        }
    }
 }
