/*************************************************************************************
 Created Date : 2023.11.29
      Creator : 이정미
   Decription : 공Tray 현황 (MULTI_BLDG 전용)
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.29  NAME   : Initial Created
  2023.12.28  이정미 : 셀감지, 거리감지 추가
**************************************************************************************/
#define SAMPLE_DEV
//#undef SAMPLE_DEV

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Linq;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_017_MULTI_BLDG : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        public FCS001_017_MULTI_BLDG()
        {
            InitializeComponent();
        }
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
            //Combo Setting
            InitCombo();

            //Spread Setting
            InitializeDataGrid();

            //조회함수
            GetList();

            this.Loaded -= UserControl_Loaded;
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE"); //콤보셋팅 공통함수 추가 수정 필요
        }

        private void InitializeDataGrid()
        {
            try
            {
                //빌딩개수조회
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["CMCDTYPE"] = "BLDG_CODE";

                dtRqst.Rows.Add(Indata);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_MUTI_BLDG_INFO_UI", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            for (int i = 0; i < result.Rows.Count; i++)
                            {
                                string sBldgName = "[*]" + result.Rows[i]["CBO_NAME"].ToString();
                                string sBldgCode = result.Rows[i]["CBO_CODE"].ToString();
                                string SBldgNo = string.IsNullOrEmpty(result.Rows[i]["ATTRIBUTE3"].ToString()) ? "3" : result.Rows[i]["ATTRIBUTE3"].ToString();

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "RUN_TRAY_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "RUN_TRAY", "RUN_TRAY" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("RUN_TRAY_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_ON_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "ONLINE" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_ON_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_OFF_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "OFFLINE" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_OFF_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_ONCLEAN_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "CLEANING" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_ONCLEAN_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_CELLDETECT_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "CELLDETECT" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_CELLDETECT_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_INNERDETECT_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "INNERDETECT" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_INNERDETECT_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });


                                dgTrayCnt.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
                                {
                                    Name = "EMPTY_TRAY_DISUSE_CNT_" + SBldgNo,
                                    Header = new string[] { sBldgName, "EMPTY_TRAY", "SCRAP" }.ToList<string>(),
                                    Binding = new Binding()
                                    {
                                        Path = new PropertyPath("EMPTY_TRAY_DISUSE_CNT_" + SBldgNo),
                                        Mode = BindingMode.TwoWay
                                    },
                                    TextWrapping = TextWrapping.Wrap,
                                    IsReadOnly = true,
                                    Format = "#,##0"
                                });

                                dgTrayCnt.TopRowHeaderMerge();
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

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnChart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetChart();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgTrayCnt_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (dgTrayCnt == null || dgTrayCnt.CurrentRow == null || dgTrayCnt.CurrentRow.Index < 1)
                {
                    return;
                }

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTrayCnt.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sTrayType = Util.NVC(DataTableConverter.GetValue(dgTrayCnt.Rows[cell.Row.Index].DataItem, "TRAY_TYPE_CD")).ToString();

                    string sOnOff = string.Empty;
                    string sColTag = cell.Column.Name.ToString();
                    string[] sColTagList = sColTag.Split('_');

                    if ((sColTag.Equals("EMPTY_TRAY_ON_CNT_3") || sColTag.Equals("EMPTY_TRAY_OFF_CNT_3")) ||
                        (!sColTag.Contains("EMPTY_TRAY_ON_CNT") && !sColTag.Contains("EMPTY_TRAY_OFF_CNT")))
                    {
                        return;
                    }

                    if (sColTag.Contains("EMPTY_TRAY_ON_CNT"))
                    {
                        sOnOff = "ON";
                    }
                    else if (sColTag.Contains("EMPTY_TRAY_OFF_CNT"))
                    {
                        sOnOff = "OFF";
                    }

                    object[] parameters = new object[5];
                    parameters[0] = sOnOff;
                    parameters[1] = sTrayType;
                    parameters[2] = string.Empty;

                    if (!sColTagList[4].Equals("3"))
                    {
                        parameters[3] = sColTagList[4];
                    }
                    else
                    {
                        parameters[3] = string.Empty;
                    }
                    parameters[4] = string.Empty;

                    this.FrameOperation.OpenMenu("SFU010710061", true, parameters); //공 Tray 관리 화면 연계
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgTrayCnt_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if ((!e.Cell.Column.Name.ToString().Equals("EMPTY_TRAY_ON_CNT_PKG") && !e.Cell.Column.Name.ToString().Equals("EMPTY_TRAY_OFF_CNT_PKG"))
                     && (e.Cell.Column.Name.ToString().Contains("EMPTY_TRAY_ON_CNT") || e.Cell.Column.Name.ToString().Contains("EMPTY_TRAY_OFF_CNT")))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Cell.Presenter.FontWeight = FontWeights.Bold; //2021.04.01 링크 기능을 Head -> Cell 로 전환 및 Bold 제거
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        #region Method

        private void GetList()
        {
            try
            {
                Util.gridClear(dgTrayCnt);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CMCDTYPE"] = "BLDG_CODE";
                dr["TRAY_TYPE_CODE"] = Util.GetCondition(cboTrayType, bAllNull: true);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService("DA_SEL_EMPT_TRAY_TOTAL_MULTI_BLDG", "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgTrayCnt, result, FrameOperation, true);
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
            catch (Exception e)
            {
                Util.MessageException(e);
            }
        }

        private void GetChart()
        {
            FCS001_017_MULTI_BLDG_CHART popup = new FCS001_017_MULTI_BLDG_CHART { FrameOperation = FrameOperation };

            object[] parameters = new object[4];
            parameters[0] = "";         // inTable;
            parameters[1] = "";         // cboArea.SelectedValue.ToString();
            parameters[2] = null;
            parameters[3] = "Port";

            C1WindowExtension.SetParameters(popup, parameters);

            Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
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
    }
}
