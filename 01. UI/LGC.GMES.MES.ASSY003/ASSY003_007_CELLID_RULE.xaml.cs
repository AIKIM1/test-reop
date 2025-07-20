/*************************************************************************************
 Created Date : 2017.12.14
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging CELL ID RULE 참조 (HIDDEN 기능)
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.14  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_007_CELLID_RULE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_007_CELLID_RULE : C1Window, IWorkArea
    {
        private string _lotid = string.Empty;
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_007_CELLID_RULE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length >= 1)
                {
                    _lotid = Util.NVC(tmps[0]);
                }
                else
                {
                    _lotid = "";
                }

                SetCellIDRule();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void SetCellIDRule()
        {
            try
            {
                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    string str = GetYMWDDigit();

                    DataTable dtTmp = new DataTable();
                    dtTmp.Columns.Add("GUBUN", typeof(string));
                    dtTmp.Columns.Add("N1", typeof(string));
                    dtTmp.Columns.Add("N2", typeof(string));
                    dtTmp.Columns.Add("N3", typeof(string));
                    dtTmp.Columns.Add("N4", typeof(string));
                    dtTmp.Columns.Add("N5", typeof(string));
                    dtTmp.Columns.Add("N6", typeof(string));
                    dtTmp.Columns.Add("N7", typeof(string));
                    dtTmp.Columns.Add("N8", typeof(string));
                    dtTmp.Columns.Add("N9", typeof(string));
                    dtTmp.Columns.Add("N10", typeof(string));
                    dtTmp.Columns.Add("N11", typeof(string));
                    dtTmp.Columns.Add("N12", typeof(string));
                    dtTmp.Columns.Add("N13", typeof(string));
                    dtTmp.Columns.Add("N14", typeof(string));
                    dtTmp.Columns.Add("N15", typeof(string));
                    dtTmp.Columns.Add("N16", typeof(string));
                    dtTmp.Columns.Add("N17", typeof(string));
                    dtTmp.Columns.Add("N18", typeof(string));
                    dtTmp.Columns.Add("N19", typeof(string));
                    dtTmp.Columns.Add("N20", typeof(string));

                    DataRow dtRow = dtTmp.NewRow();
                    dtRow["GUBUN"] = "Digit";
                    dtRow["N1"] = "1";
                    dtRow["N2"] = "2";
                    dtRow["N3"] = "3";
                    dtRow["N4"] = "4";
                    dtRow["N5"] = "5";
                    dtRow["N6"] = "6";
                    dtRow["N7"] = "7";
                    dtRow["N8"] = "8";
                    dtRow["N9"] = "9";
                    dtRow["N10"] = "10";
                    dtRow["N11"] = "11";
                    dtRow["N12"] = "12";
                    dtRow["N13"] = "13";
                    dtRow["N14"] = "14";
                    dtRow["N15"] = "15";
                    dtRow["N16"] = "16";
                    dtRow["N17"] = "17";
                    dtRow["N18"] = "18";
                    dtRow["N19"] = "19";
                    dtRow["N20"] = "20";

                    dtTmp.Rows.Add(dtRow);

                    DataRow dtLot = dtTmp.NewRow();
                    dtLot["GUBUN"] = "LOT ID";

                    for (int i = 0; i < _lotid.Length; i++)
                    {
                        dtLot["N" + (i + 1).ToString()] = _lotid.Substring(i, 1);
                    }

                    dtTmp.Rows.Add(dtLot);

                    dtRow = dtTmp.NewRow();
                    dtRow["GUBUN"] = "CELL ID";
                    dtRow["N1"] = "L";
                    dtRow["N2"] = "1";
                    dtRow["N3"] = "1";
                    dtRow["N4"] = "1";

                    if (str.Length > 3)
                    {
                        dtRow["N5"] = str.Substring(0, 1);
                        dtRow["N6"] = str.Substring(1, 1);
                        dtRow["N7"] = str.Substring(2, 1);
                        dtRow["N8"] = str.Substring(3, 1);
                    }
                    else
                    {
                        dtRow["N5"] = "";
                        dtRow["N6"] = "";
                        dtRow["N7"] = "";
                        dtRow["N8"] = "";
                    }
                    
                    dtRow["N9"] = "A";
                    dtRow["N10"] = "0";
                    dtRow["N11"] = "0";
                    dtRow["N12"] = "0";
                    dtRow["N13"] = "0";
                    dtRow["N14"] = "1";
                    dtRow["N15"] = "";
                    dtRow["N16"] = "";
                    dtRow["N17"] = "";
                    dtRow["N18"] = "";
                    dtRow["N19"] = "";
                    dtRow["N20"] = "";

                    dtTmp.Rows.Add(dtRow);

                    dgCellIDRule.ItemsSource = DataTableConverter.Convert(dtTmp);
                                        
                    dgCellIDRule.Columns["N15"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N16"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N17"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N18"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N19"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N20"].Visibility = Visibility.Collapsed;
                }
                else
                {
                    DataTable dtTmp = new DataTable();
                    dtTmp.Columns.Add("GUBUN", typeof(string));
                    dtTmp.Columns.Add("N1", typeof(string));
                    dtTmp.Columns.Add("N2", typeof(string));
                    dtTmp.Columns.Add("N3", typeof(string));
                    dtTmp.Columns.Add("N4", typeof(string));
                    dtTmp.Columns.Add("N5", typeof(string));
                    dtTmp.Columns.Add("N6", typeof(string));
                    dtTmp.Columns.Add("N7", typeof(string));
                    dtTmp.Columns.Add("N8", typeof(string));
                    dtTmp.Columns.Add("N9", typeof(string));
                    dtTmp.Columns.Add("N10", typeof(string));
                    dtTmp.Columns.Add("N11", typeof(string));
                    dtTmp.Columns.Add("N12", typeof(string));
                    dtTmp.Columns.Add("N13", typeof(string));
                    dtTmp.Columns.Add("N14", typeof(string));
                    dtTmp.Columns.Add("N15", typeof(string));
                    dtTmp.Columns.Add("N16", typeof(string));
                    dtTmp.Columns.Add("N17", typeof(string));
                    dtTmp.Columns.Add("N18", typeof(string));
                    dtTmp.Columns.Add("N19", typeof(string));
                    dtTmp.Columns.Add("N20", typeof(string));

                    DataRow dtRow = dtTmp.NewRow();
                    dtRow["GUBUN"] = "Digit";
                    dtRow["N1"] = "1";
                    dtRow["N2"] = "2";
                    dtRow["N3"] = "3";
                    dtRow["N4"] = "4";
                    dtRow["N5"] = "5";
                    dtRow["N6"] = "6";
                    dtRow["N7"] = "7";
                    dtRow["N8"] = "8";
                    dtRow["N9"] = "9";
                    dtRow["N10"] = "10";
                    dtRow["N11"] = "11";
                    dtRow["N12"] = "12";
                    dtRow["N13"] = "13";
                    dtRow["N14"] = "14";
                    dtRow["N15"] = "15";
                    dtRow["N16"] = "16";
                    dtRow["N17"] = "17";
                    dtRow["N18"] = "18";
                    dtRow["N19"] = "19";
                    dtRow["N20"] = "20";

                    dtTmp.Rows.Add(dtRow);

                    DataRow dtLot = dtTmp.NewRow();
                    dtLot["GUBUN"] = "LOT ID";

                    for (int i = 0; i < _lotid.Length; i++)
                    {
                        dtLot["N" + (i + 1).ToString()] = _lotid.Substring(i, 1);
                    }

                    dtTmp.Rows.Add(dtLot);

                    dtRow = dtTmp.NewRow();
                    dtRow["GUBUN"] = "CELL ID";
                    dtRow["N1"] = dtLot["N4"];
                    dtRow["N2"] = dtLot["N5"];
                    dtRow["N3"] = dtLot["N6"];
                    dtRow["N4"] = dtLot["N7"];
                    dtRow["N5"] = dtLot["N8"];
                    dtRow["N6"] = dtLot["N10"];
                    dtRow["N7"] = "0";
                    dtRow["N8"] = "0";
                    dtRow["N9"] = "0";
                    dtRow["N10"] = "1";
                    dtRow["N11"] = "";
                    dtRow["N12"] = "";
                    dtRow["N13"] = "";
                    dtRow["N14"] = "";
                    dtRow["N15"] = "";
                    dtRow["N16"] = "";
                    dtRow["N17"] = "";
                    dtRow["N18"] = "";
                    dtRow["N19"] = "";
                    dtRow["N20"] = "";

                    dtTmp.Rows.Add(dtRow);

                    dgCellIDRule.ItemsSource = DataTableConverter.Convert(dtTmp);

                    dgCellIDRule.Columns["N11"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N12"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N13"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N14"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N15"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N16"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N17"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N18"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N19"].Visibility = Visibility.Collapsed;
                    dgCellIDRule.Columns["N20"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string GetYMWDDigit()
        {
            try
            {
                string sRet = "";
                DataTable searchConditionTable = new DataTable();
                searchConditionTable.Columns.Add("LOTID", typeof(string));

                DataRow searchCondition = searchConditionTable.NewRow();
                searchCondition["LOTID"] = _lotid;

                searchConditionTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELLID_Y_DW_WD_RULE_BY_LOT_TMP", "INDATA", "OUTDATA", searchConditionTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("RET_CELL_STR"))
                {
                    sRet = Util.NVC(dtRslt.Rows[0]["RET_CELL_STR"]);
                }
                return sRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void dgCellIDRule_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    if (e.Cell.Row.Index == 1)
                    {
                        if (e.Cell.Column.Index == 4 ||
                           e.Cell.Column.Index == 5 ||
                           e.Cell.Column.Index == 6
                          )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                    else if (e.Cell.Row.Index == 2)
                    {
                        if (e.Cell.Column.Index == 5 ||
                           e.Cell.Column.Index == 6 ||
                           e.Cell.Column.Index == 7 ||
                           e.Cell.Column.Index == 8
                          )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else if (e.Cell.Column.Index == 1 ||
                                e.Cell.Column.Index == 2 ||
                                e.Cell.Column.Index == 3 ||
                                e.Cell.Column.Index == 4
                                )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#B8B8B8"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else if (e.Cell.Column.Index == 9
                                )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#B8B8B8"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                }
                else
                {
                    if (e.Cell.Row.Index == 1)
                    {
                        if (e.Cell.Column.Index == 4 ||
                           e.Cell.Column.Index == 5 ||
                           e.Cell.Column.Index == 6 ||
                           e.Cell.Column.Index == 7 ||
                           e.Cell.Column.Index == 8
                          )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else if (e.Cell.Column.Index == 10)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                    else if (e.Cell.Row.Index == 2)
                    {
                        if (e.Cell.Column.Index == 1 ||
                           e.Cell.Column.Index == 2 ||
                           e.Cell.Column.Index == 3 ||
                           e.Cell.Column.Index == 4 ||
                           e.Cell.Column.Index == 5
                          )
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                        else if (e.Cell.Column.Index == 6)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        }
                    }
                }
            }));
        }

        private void dgCellIDRule_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnCopyCellID_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellIDRule.Rows.Count >= 2)
                {
                    string sCell = "";
                    for (int i = 1; i < dgCellIDRule.Columns.Count; i++)
                    {
                        sCell = sCell + Util.NVC(DataTableConverter.GetValue(dgCellIDRule.Rows[2].DataItem, dgCellIDRule.Columns[i].Name));
                    }

                    Clipboard.SetText(sCell);

                    this.DialogResult = MessageBoxResult.OK;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellIDRule_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                {
                    if (e.Row.Index != 2 ||
                        e.Column.Index == 5 ||
                        e.Column.Index == 6 ||
                        e.Column.Index == 7 ||
                        e.Column.Index == 8)
                    {
                        e.Cancel = true;
                    }
                }
                else
                {
                    if (e.Row.Index != 2 ||
                        e.Column.Index > 7)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
