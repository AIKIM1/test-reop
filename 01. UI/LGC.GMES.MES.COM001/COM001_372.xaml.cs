
/*************************************************************************************
 Created Date : 2022.09.08
      Creator : 신광희
   Decription : 자동차 전극 Roll Map - Roll Map 생성률, 기준점 인식률 조회(가칭) 
--------------------------------------------------------------------------------------
 [Change History]
  2022.09.08  신광희 : Initial Created
  2022.10.13  신광희 : 검색조건 Plant, 동 콤보박스 추가 및 설비 MultiSelectionBox로 변경 
  2023.05.25  신광희 : 기준점 인식률 코터 공정 추가에 따른 소스 수정
  2024.06.20  박학철 : NFF 와인더 기준점 인식률/생성률 로직 추가
  2025.07.07  김현진 : DNC 기준점 인식률/생성률 로직 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Linq;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_372 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        private string _queryResultProcessCode = string.Empty;
        private string _queryResultSearchType = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_372()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize        
        private void InitializeCombo()
        {
            // Plant 
            SetRollMapPlantCombo(cboPlant);

            // 동
            SetRollMapAreaCombo(cboArea);

            // 공정
            SetRollMapProcessCombo(cboProcess);

            // 설비
            SetRollMapEquipmentMultiSelectionBox(msbEquipment);
        }

        private void InitializeControl()
        {
            DateTime systemDateTime = GetSystemTime();

            if (dtpDateFrom != null)
            {
                dtpDateFrom.SelectedDateTime = systemDateTime.AddDays(-7);
                dtpDateFrom.Text = systemDateTime.AddDays(-7).ToShortDateString();
            }

            if (dtpDateTo != null)
            {
                dtpDateTo.SelectedDateTime = systemDateTime;
                dtpDateTo.Text = systemDateTime.ToShortDateString();
            }

            InitializeCombo();
        }


        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControl();

            Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            SelectRollMapDataList();
        }

        private void cboPlant_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRollMapAreaCombo(cboArea);
        }

        private void cboArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRollMapEquipmentMultiSelectionBox(msbEquipment);
        }

        

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRollMapEquipmentMultiSelectionBox(msbEquipment);
        }

        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.dgList.Loaded -= dgList_Loaded;
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            if (e.Cell.Presenter == null) return;

            C1DataGrid dg = sender as C1DataGrid;

            string prefixProcess = GetPreFixProcess(cboProcess.SelectedValue.GetString());

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //string projectName = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRJT_NAME"));
                    if (e.Cell.Column.Name.GetString() == "PRJT_NAME"
                     || e.Cell.Column.Name.GetString() == "CT_LOTID"
                     || e.Cell.Column.Name.GetString() == "RP_LOTID"
                     || e.Cell.Column.Name.GetString() == "SL_LOTID"
                     || e.Cell.Column.Name.GetString() == "NT_LOTID"
                     || e.Cell.Column.Name.GetString() == "LM_LOTID"
                     || e.Cell.Column.Name.GetString() == "HS_LOTID"    //20231013 조성진, 소형 2170 조회 추가
                     || e.Cell.Column.Name.GetString() == "WN_LOTID"    //20231013 조성진, 소형 2170 조회 추가
                     || e.Cell.Column.Name.GetString() == "DNC_LOTID"    //20250707 김현진, DNC 조회 추가
                     )
                    {
                        e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Left;
                    }
                    else
                    {
                        e.Cell.Presenter.HorizontalContentAlignment = HorizontalAlignment.Right;
                    }
                }

                if (rdoCreate.IsChecked != null && (bool)rdoCreate.IsChecked)
                {
                    if(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, prefixProcess+ "LOTID")).Contains("Total"))
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    //if (e.Cell.Row.Type == DataGridRowType.Item)
                    //{
                    //    e.Cell.Presenter.Background = null;
                    //}
                }
            }));
        }

        private void dgList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (!CommonVerify.HasDataGridRow(dg) || dg?.CurrentRow == null) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(pnt);

                if (cell == null) return;

                int rowIdx = cell.Row.Index;
                DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
                if (drv == null) return;

                // 1. 생성률, 2. 기준점 인식률, Default. 롤프레스 UW엔코더 슬립
                if (_queryResultSearchType == "1")
                {
                    if (string.Equals(_queryResultProcessCode,Process.ROLL_PRESSING))
                    {
                        if (cell.Column.Name.Equals("CT_RM_CREATE"))
                        {
                            if(DataTableConverter.GetValue(drv, "CT_RM_CREATE").GetString() == "N")
                            {
                                //Util.MessageInfo(DataTableConverter.GetValue(drv, "CT_ROLLMAP_REASON").GetString());
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "CT_ROLLMAP_REASON").GetString());
                            }
                            //
                        }
                        else if (cell.Column.Name.Equals("RP_RM_CREATE"))
                        {
                            if(DataTableConverter.GetValue(drv, "RP_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "RP_ROLLMAP_REASON").GetString());
                            }
                        }
                        else return;
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.SLITTING))
                    {
                        if (cell.Column.Name.Equals("CT_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "CT_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "CT_ROLLMAP_REASON").GetString());
                            }
                        }
                        else if(cell.Column.Name.Equals("RP_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "RP_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "RP_ROLLMAP_REASON").GetString());
                            }
                        }
                        else if (cell.Column.Name.Equals("SL_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "SL_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "SL_ROLLMAP_REASON").GetString());
                            }
                        }
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.NOTCHING))
                    {
                        if (cell.Column.Name.Equals("NT_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "NT_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "NT_ROLLMAP_REASON").GetString());
                            }
                        }
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.LAMINATION))
                    {
                        if (cell.Column.Name.Equals("NT_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "NT_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "NT_ROLLMAP_REASON").GetString());
                            }
                        }
                        else if (cell.Column.Name.Equals("LM_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "LM_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "LM_ROLLMAP_REASON").GetString());
                            }
                        }
                    }
                    else if (string.Equals(_queryResultProcessCode, Process.DNC))
                    {
                        if (cell.Column.Name.Equals("DNC_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "DNC_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "DNC_ROLLMAP_REASON").GetString());
                            }
                        }
                        else if (cell.Column.Name.Equals("RP_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "RP_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "RP_ROLLMAP_REASON").GetString());
                            }
                        }
                        else if (cell.Column.Name.Equals("SL_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "SL_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "SL_ROLLMAP_REASON").GetString());
                            }
                        }
                    }
                    else if (string.Equals(_queryResultProcessCode, Process.HALF_SLITTING))    //20231013 조성진, 소형 2170 조회 추가
                    {
                        if (cell.Column.Name.Equals("CT_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "CT_RM_CREATE").GetString() == "N")
                            {
                                //Util.MessageInfo(DataTableConverter.GetValue(drv, "CT_ROLLMAP_REASON").GetString());
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "CT_ROLLMAP_REASON").GetString());
                            }
                            //
                        }
                        else if (cell.Column.Name.Equals("HS_RM_CREATE"))
                        {
                            if (DataTableConverter.GetValue(drv, "HS_RM_CREATE").GetString() == "N")
                            {
                                PopupRollMapReason(DataTableConverter.GetValue(drv, "HS_ROLLMAP_REASON").GetString());
                            }
                        }
                        else return;
                    }
                    else
                    {
                        return;
                    }

                }
                else if(_queryResultSearchType == "2")
                {
                    //기준점 인식율인 경우 코터 마킹 기준점 개수, 롤프레스 인식 기준점 개수 더블 클릭 시 Lane 번호, 롤맵 수집유형 팝업 호출 함.
                    if (string.Equals(_queryResultProcessCode, Process.COATING))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (string.Equals(_queryResultProcessCode, Process.ROLL_PRESSING))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if(cell.Column.Name.Equals("RP_MARK_COUNT") && DataTableConverter.GetValue(drv, "RP_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "RP_LOTID").GetString(), "1");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.SLITTING))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if (cell.Column.Name.Equals("RP_MARK_COUNT") && DataTableConverter.GetValue(drv, "RP_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "RP_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("SL_MARK_COUNT") && DataTableConverter.GetValue(drv, "SL_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "SL_LOTID").GetString(), "1");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.NOTCHING))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if (cell.Column.Name.Equals("RP_MARK_COUNT") && DataTableConverter.GetValue(drv, "RP_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "RP_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("SL_MARK_COUNT") && DataTableConverter.GetValue(drv, "SL_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "SL_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("NT_MARK_COUNT") && DataTableConverter.GetValue(drv, "NT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "NT_LOTID").GetString(), "1");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if(string.Equals(_queryResultProcessCode, Process.LAMINATION))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if (cell.Column.Name.Equals("RP_MARK_COUNT") && DataTableConverter.GetValue(drv, "RP_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "RP_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("SL_MARK_COUNT") && DataTableConverter.GetValue(drv, "SL_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "SL_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("NT_MARK_COUNT") && DataTableConverter.GetValue(drv, "NT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "NT_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("LM_MARK_COUNT") && DataTableConverter.GetValue(drv, "LM_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "LM_LOTID").GetString(), DataTableConverter.GetValue(drv, "WIPSEQ").GetString());
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (string.Equals(_queryResultProcessCode, Process.DNC))
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if (cell.Column.Name.Equals("RP_MARK_COUNT") && DataTableConverter.GetValue(drv, "RP_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "RP_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("SL_MARK_COUNT") && DataTableConverter.GetValue(drv, "SL_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "SL_LOTID").GetString(), "1");
                        }
                        else if (cell.Column.Name.Equals("DNC_MARK_COUNT") && DataTableConverter.GetValue(drv, "DNC_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "DNC_LOTID").GetString(), DataTableConverter.GetValue(drv, "LOT_SUM_NO").GetString());
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (string.Equals(_queryResultProcessCode, Process.HALF_SLITTING))    //20231013 조성진, 소형 2170 조회 추가
                    {
                        if (cell.Column.Name.Equals("CT_MARK_COUNT") && DataTableConverter.GetValue(drv, "CT_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "CT_LOTID").GetString(), "1", "Y");
                        }
                        else if (cell.Column.Name.Equals("HS_MARK_COUNT") && DataTableConverter.GetValue(drv, "HS_MARK_COUNT").GetInt() > 0)
                        {
                            PopupRollMapMark(DataTableConverter.GetValue(drv, "HS_LOTID").GetString(), "1");
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            /*
            if (rdoCreate.IsChecked != null && (bool)rdoCreate.IsChecked && cboProcess.SelectedValue?.GetString() == "E3000")
            {
                if (e.Column.Header.GetString() == ObjectDic.Instance.GetObjectName("RP_RM_CREATE"))
                {
                    Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                    style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Color.FromArgb(255, 255, 204, 204)) });
                    style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Green });
                    style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                    style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                    style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
                    dgList.Columns[e.Column.Name].HeaderStyle = style;
                    //e.Column.HeaderPresenter.Background = new SolidColorBrush(Colors.Green);
                }
            }
            */
        }

        private void DatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }

        private void rdoUwslip_Click(object sender, RoutedEventArgs e)
        {
        }

        private void rdoCognition_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rdoCreate_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Mehod

        private void SelectRollMapDataList()
        {
            try
            {
                Util.gridClear(dgList);

                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_SEL_ROLLMAP_CREATE_MARK_RATE";
                const string outParameterTable = "OUT_CREATE_CT,OUT_CREATE_RP,OUT_CREATE_SL,OUT_CREATE_NT,OUT_CREATE_LM,OUT_CREATE_WN"
                                                + ",OUT_MARK_CT,OUT_MARK_RP,OUT_MARK_SL,OUT_MARK_NT,OUT_MARK_LM,OUT_MARK_WN"
                                                + ",OUT_SLIP,OUT_HS_YN"
                                                + ",OUT_CREATE_CT_CY,OUT_CREATE_HS_CY,OUT_CREATE_RP_CY,OUT_CREATE_SL_CY,OUT_CREATE_WN_CY"
                                                + ",OUT_MARK_CT_CY,OUT_MARK_HS_CY,OUT_MARK_RP_CY,OUT_MARK_SL_CY,OUT_MARK_WN_CY,OUT_CREATE_DNC,OUT_MARK_DNC";

                DataSet ds = new DataSet();
                DataTable inTable = ds.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SELECT_TYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("DT_START", typeof(string));
                inTable.Columns.Add("DT_END", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));


                string searchType = string.Empty;

                foreach (RadioButton rdoButton in Util.FindVisualChildren<RadioButton>(grdSearch))
                {
                    if((bool)rdoButton.IsChecked)
                    {
                        searchType = rdoButton.Tag.GetString();
                        break;
                    }
                }

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SELECT_TYPE"] = searchType;             //1. 생성률, 2. 기준점 인식률, Default. 롤프레스 UW엔코더 슬립
                dr["EQPTID"] = msbEquipment.SelectedItemsToString;
                dr["PROCID"] = cboProcess.SelectedValue;
                dr["DT_START"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DT_END"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["AREAID"] = cboArea.SelectedValue;
                inTable.Rows.Add(dr);

                //string xml = ds.GetXml();//
                _queryResultProcessCode = cboProcess.SelectedValue.GetString();
                _queryResultSearchType = searchType;

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", outParameterTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        DataTable result = GetDataTableColumnByProcessCode(searchType, cboProcess.SelectedValue.GetString(), searchResult);

                        if (result == null)
                        {
                            return;
                        }


                        Util.GridSetData(dgList, result, FrameOperation, true);

                        string cyyn = searchResult.Tables["OUT_HS_YN"].Rows.Count == 0 ? "N" : "Y";     //20231013 조성진, 소형인지 여부
                        string hsyn = string.Equals(cyyn, "N") ? "N" : searchResult.Tables["OUT_HS_YN"].Rows[0]["HSYN"].ToString();     //20231013 조성진, 소형이면서 하프슬리터 유무
                                                
                        if (_queryResultSearchType == "1")
                        {
                            if(string.Equals(_queryResultProcessCode,Process.ROLL_PRESSING))
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.SLITTING))
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["SL_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.NOTCHING))
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("NT_ROLLMAP_REASON")) dgList.Columns["NT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.LAMINATION))
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("NT_ROLLMAP_REASON")) dgList.Columns["NT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("LM_ROLLMAP_REASON")) dgList.Columns["LM_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.WINDING))
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("WN_ROLLMAP_REASON")) dgList.Columns["WN_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.HALF_SLITTING))    //20231013 조성진, 소형 2170 조회 추가
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }
                            else if (string.Equals(_queryResultProcessCode, Process.DNC)) //20250708 김현진 DNC 추가
                            {
                                if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["SL_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                if (dgList.Columns.Contains("DNC_ROLLMAP_REASON")) dgList.Columns["DNC_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                            }

                            if (string.Equals(cyyn, "Y"))     //20231013 조성진, 소형 2170 조회 추가
                            {
                                if(string.Equals(hsyn, "N"))
                                {
                                    if (!string.Equals(_queryResultProcessCode, Process.COATING))
                                    {
                                        if (dgList.Columns.Contains("HS_LOTID")) { dgList.Columns["HS_LOTID"].Visibility = Visibility.Collapsed; }
                                        if (dgList.Columns.Contains("HS_CREATE_YN")) { dgList.Columns["HS_CREATE_YN"].Visibility = Visibility.Collapsed; }
                                        if (dgList.Columns.Contains("HS_RM_CREATE")) { dgList.Columns["HS_RM_CREATE"].Visibility = Visibility.Collapsed; }
                                        if (dgList.Columns.Contains("HS_RM_CREATE_RATE")) { dgList.Columns["HS_RM_CREATE_RATE"].Visibility = Visibility.Collapsed; }
                                        if (dgList.Columns.Contains("HS_ROLLMAP_REASON")) { dgList.Columns["HS_ROLLMAP_REASON"].Visibility = Visibility.Collapsed; }
                                        
                                    }
                                }
                                else
                                {
                                    if (!string.Equals(_queryResultProcessCode, Process.COATING))
                                    {
                                        if (dgList.Columns.Contains("HS_ROLLMAP_REASON")) dgList.Columns["HS_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                    }

                                    if (string.Equals(_queryResultProcessCode, Process.WINDING))    //20231013 조성진, 소형 2170 조회 추가
                                    {
                                        if (dgList.Columns.Contains("CT_ROLLMAP_REASON")) dgList.Columns["CT_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                        //if (dgList.Columns.Contains("HS_ROLLMAP_REASON")) dgList.Columns["HS_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                        if (dgList.Columns.Contains("RP_ROLLMAP_REASON")) dgList.Columns["RP_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                        if (dgList.Columns.Contains("SL_ROLLMAP_REASON")) dgList.Columns["SL_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                        if (dgList.Columns.Contains("WN_ROLLMAP_REASON")) dgList.Columns["WN_ROLLMAP_REASON"].Visibility = Visibility.Collapsed;
                                    }
                                }
                            }
                        }
                        else if(_queryResultSearchType == "2")
                        {
                            if(string.Equals(_queryResultProcessCode, Process.LAMINATION))
                            {
                                if (dgList.Columns.Contains("WIPSEQ")) dgList.Columns["WIPSEQ"].Visibility = Visibility.Collapsed;
                            }
                            if (string.Equals(_queryResultProcessCode, Process.DNC))
                            {
                                if (dgList.Columns.Contains("LOT_SUM_NO")) dgList.Columns["LOT_SUM_NO"].Visibility = Visibility.Collapsed;
                            }

                            if (string.Equals(cyyn, "Y"))     //20231013 조성진, 소형 2170 조회 추가
                            {
                                if (string.Equals(hsyn, "N"))    //20231013 조성진, 소형 2170 조회 추가
                                {
                                    if (dgList.Columns.Contains("HS_LOTID")) { dgList.Columns["HS_LOTID"].Visibility = Visibility.Collapsed; }
                                    if (dgList.Columns.Contains("HS_EQPT_NAME")) { dgList.Columns["HS_EQPT_NAME"].Visibility = Visibility.Collapsed; }
                                    if (dgList.Columns.Contains("HS_CREATE_YN")) { dgList.Columns["HS_CREATE_YN"].Visibility = Visibility.Collapsed; }
                                    if (dgList.Columns.Contains("HS_MARK_COUNT")) { dgList.Columns["HS_MARK_COUNT"].Visibility = Visibility.Collapsed; }
                                    if (dgList.Columns.Contains("HS_MISSING_MARK")) { dgList.Columns["HS_MISSING_MARK"].Visibility = Visibility.Collapsed; }
                                    if (dgList.Columns.Contains("HS_MARK_DETECT_RATE")) { dgList.Columns["HS_MARK_DETECT_RATE"].Visibility = Visibility.Collapsed; }
                                }
                            }
                        }

                        //GetDataGridHeaderStyle(dgList);

                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetDataTableColumnByProcessCode(string searchType, string processCode, DataSet ds)
        {
            DataTable dtReturn = null;

            if (!CommonVerify.HasTableInDataSet(ds)) return null;

            //20231013 조성진, 오창 소형 조회 관련 수정(하프슬리터 추가 및 소형 조회 분기 등)
            if (searchType == "1")
            {
                if(ds.Tables["OUT_HS_YN"].Rows.Count == 0)
                {
                    //생성률
                    if (string.Equals(Process.COATING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_CT")) dtReturn = ds.Tables["OUT_CREATE_CT"];
                    }
                    else if (string.Equals(Process.ROLL_PRESSING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_RP")) dtReturn = ds.Tables["OUT_CREATE_RP"];
                    }
                    else if (string.Equals(Process.SLITTING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_SL")) dtReturn = ds.Tables["OUT_CREATE_SL"];
                    }
                    else if (string.Equals(Process.NOTCHING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_NT")) dtReturn = ds.Tables["OUT_CREATE_NT"];
                    }
                    else if (string.Equals(Process.WINDING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_WN")) dtReturn = ds.Tables["OUT_CREATE_WN"];
                    }
                    else if (string.Equals(Process.DNC, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_DNC")) dtReturn = ds.Tables["OUT_CREATE_DNC"];
                    }
                    else //if (string.Equals(Process.LAMINATION, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_LM")) dtReturn = ds.Tables["OUT_CREATE_LM"];
                    }
                }
                else    // 소형인 경우
                {
                    //생성률
                    if (string.Equals(Process.COATING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_CT_CY")) dtReturn = ds.Tables["OUT_CREATE_CT_CY"];
                    }
                    else if (string.Equals(Process.HALF_SLITTING, processCode))   //20231013 조성진, 소형 2170 조회 추가
                    {
                        if (ds.Tables.Contains("OUT_CREATE_HS_CY")) dtReturn = ds.Tables["OUT_CREATE_HS_CY"];
                    }
                    else if (string.Equals(Process.ROLL_PRESSING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_RP_CY")) dtReturn = ds.Tables["OUT_CREATE_RP_CY"];
                    }
                    else if (string.Equals(Process.SLITTING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_SL_CY")) dtReturn = ds.Tables["OUT_CREATE_SL_CY"];
                    }
                    else if (string.Equals(Process.WINDING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_CREATE_WN_CY")) dtReturn = ds.Tables["OUT_CREATE_WN_CY"];
                    }
                }                
            }
            else if(searchType == "2")
            {
                if (ds.Tables["OUT_HS_YN"].Rows.Count == 0)
                {
                    //기준점 인식율
                    if (string.Equals(Process.COATING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_CT")) dtReturn = ds.Tables["OUT_MARK_CT"];
                    }
                    else if (string.Equals(Process.ROLL_PRESSING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_RP")) dtReturn = ds.Tables["OUT_MARK_RP"];
                    }
                    else if (string.Equals(Process.SLITTING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_SL")) dtReturn = ds.Tables["OUT_MARK_SL"];
                    }
                    else if (string.Equals(Process.NOTCHING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_NT")) dtReturn = ds.Tables["OUT_MARK_NT"];
                    }
                    else if (string.Equals(Process.HALF_SLITTING, processCode))   //20231013 조성진, 소형 2170 조회 추가
                    {
                        if (ds.Tables.Contains("OUT_MARK_HS")) dtReturn = ds.Tables["OUT_MARK_HS"];
                    }
                    else if (string.Equals(Process.WINDING, processCode))   //20231013 조성진, 소형 2170 조회 추가
                    {
                        if (ds.Tables.Contains("OUT_MARK_WN")) dtReturn = ds.Tables["OUT_MARK_WN"];
                    }
                    else if (string.Equals(Process.DNC, processCode))   //20250707 김현진 DNC 추가
                    {
                        if (ds.Tables.Contains("OUT_MARK_DNC")) dtReturn = ds.Tables["OUT_MARK_DNC"];
                    }
                    else //if (string.Equals(Process.LAMINATION, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_LM")) dtReturn = ds.Tables["OUT_MARK_LM"];
                    }
                }
                else    // 소형인 경우   //20231013 조성진, 소형 2170 조회 추가
                {
                    //기준점 인식율
                    if (string.Equals(Process.COATING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_CT_CY")) dtReturn = ds.Tables["OUT_MARK_CT_CY"];
                    }
                    else if (string.Equals(Process.HALF_SLITTING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_HS_CY")) dtReturn = ds.Tables["OUT_MARK_HS_CY"];
                    }
                    else if (string.Equals(Process.ROLL_PRESSING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_RP_CY")) dtReturn = ds.Tables["OUT_MARK_RP_CY"];
                    }
                    else if (string.Equals(Process.SLITTING, processCode))
                    {
                        if (ds.Tables.Contains("OUT_MARK_SL_CY")) dtReturn = ds.Tables["OUT_MARK_SL_CY"];
                    }                    
                    else if (string.Equals(Process.WINDING, processCode)) 
                    {
                        if (ds.Tables.Contains("OUT_MARK_WN_CY")) dtReturn = ds.Tables["OUT_MARK_WN_CY"];
                    }
                }
            }
            else
            {
                //롤프레스 UW엔코더 슬립
                dtReturn = ds.Tables["OUT_SLIP"];
            }

            return dtReturn;
        }

        private void GetDataGridHeaderStyle(C1DataGrid dg)
        {
            if (!CommonVerify.HasDataGridRow(dg)) return;

            foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
            {
                if (rdoCreate.IsChecked != null && (bool)rdoCreate.IsChecked && cboProcess.SelectedValue?.GetString() == "E3000")
                {
                    //CT_CREATE_YN 코터 롤맵 데이터 존재 여부
                    //RP_CREATE_YN 롤프레스 롤맵 데이터 존재 여부
                    //CT_RM_CREATE 코터 롤맵 생성 LOT 수
                    //RP_RM_CREATE 롤프레스 롤맵 생성 LOT 수

                    string[] columnHeaderStyles = new string[] { "CT_CREATE_YN", "RP_CREATE_YN", "CT_RM_CREATE", "RP_RM_CREATE"};

                    if(columnHeaderStyles.Any(x => x.Contains(col.Name)))
                    {
                        Style style = new Style(typeof(DataGridColumnHeaderPresenter));
                        style.Setters.Add(new Setter { Property = BackgroundProperty, Value = new SolidColorBrush(Colors.LightGreen) });
                        style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Black });
                        style.Setters.Add(new Setter { Property = HorizontalAlignmentProperty, Value = HorizontalAlignment.Stretch });
                        style.Setters.Add(new Setter { Property = HorizontalContentAlignmentProperty, Value = HorizontalAlignment.Center });
                        style.Setters.Add(new Setter { Property = FontWeightProperty, Value = FontWeights.Bold });
                        col.HeaderStyle = style;
                    }
                }
            }
        }

        private string GetPreFixProcess(string processCode)
        {
            if (string.Equals(Process.COATING, processCode))
                return "CT_";
            else if (string.Equals(Process.ROLL_PRESSING, processCode))
                return "RP_";
            else if (string.Equals(Process.SLITTING, processCode))
                return "SL_";
            else if (string.Equals(Process.NOTCHING, processCode))
                return "NT_";
            else if (string.Equals(Process.LAMINATION, processCode))
                return "LM_";
            else if (string.Equals(Process.HALF_SLITTING, processCode))    //20231013 조성진, 소형 2170 조회 추가
                return "HS_";
            else if (string.Equals(Process.WINDING, processCode))    //20231013 조성진, 소형 2170 조회 추가
                return "WN_";
            else if (string.Equals(Process.DNC, processCode))    //20250707 김현진 DNC 추가
                return "DNC_";
            else
                return string.Empty;
            
        }

        private void PopupRollMapReason(string processReason)
        {
            COM001_372_REASON popRollMapReason = new COM001_372_REASON();
            popRollMapReason.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = processReason;
            C1WindowExtension.SetParameters(popRollMapReason, parameters);
            popRollMapReason.Closed += popRollMapReason_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapReason.ShowModal()));
        }

        private void popRollMapReason_Closed(object sender, EventArgs e)
        {
            COM001_372_REASON popup = sender as COM001_372_REASON;
            if (popup != null)
            {

            }
        }

        private void PopupRollMapMark(string lotId, string wipseq, string coaterLotYn = "N")
        {
            COM001_372_TYPE popRollMapMark = new COM001_372_TYPE();
            popRollMapMark.FrameOperation = FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = lotId;
            parameters[1] = wipseq;
            parameters[2] = coaterLotYn;
            C1WindowExtension.SetParameters(popRollMapMark, parameters);
            popRollMapMark.Closed += popRollMapMark_Closed;
            Dispatcher.BeginInvoke(new Action(() => popRollMapMark.ShowModal()));
        }

        private void popRollMapMark_Closed(object sender, EventArgs e)
        {
            COM001_372_TYPE popup = sender as COM001_372_TYPE;
            if (popup != null)
            {

            }
        }

        private static void SetRollMapPlantCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ROLLMAP_PLANT_CBO";

            string[] arrColumn = { "LANGID","USERID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.USERID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_SHOP_ID);
        }

        private void SetRollMapAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ROLLMAP_AREA_CBO";

            string[] arrColumn = { "LANGID", "USERID","SHOPID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.USERID,cboPlant.SelectedValue.GetString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }

        private void SetRollMapProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_ROLLMAP_PROCESS_CBO";

            string[] arrColumn = { "LANGID"};
            string[] arrCondition = { LoginInfo.LANGID};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void SetRollMapEquipmentMultiSelectionBox(MultiSelectionBox msb)
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_ROLLMAP_EQUIPMENT_CBO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["PROCID"] = cboProcess.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
                msb.ItemsSource = DataTableConverter.Convert(dtResult);
                if (dtResult.Rows.Count > 0)
                {
                    msb.CheckAll();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        private bool ValidationSearch()
        {
            if (cboProcess?.SelectedValue == null || cboProcess.SelectedValue.GetString() == "SELECT")
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return false;
            }


            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            // 설비코드 체크 
            if(string.IsNullOrEmpty(msbEquipment.SelectedItemsToString.Trim()))
            {
                //설비정보가 없습니다.
                Util.MessageValidation("SFU2911");
                return false;
            }

            return true;
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