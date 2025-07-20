/*************************************************************************************
 Created Date : 2023.01.04
      Creator : 심찬보
   Decription : 오창 IT 3동 자동차 고전압 활성화 JIG 충방전기 현황
--------------------------------------------------------------------------------------
 [Change History]
  2023.01.04  DEVELOPER : Initial Created.
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
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;
using System.IO;
using System.Windows.Data;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_313 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _LANEID = string.Empty;
        private DataTable _dtColor;
        private DataTable _dtDATA;
        private DataTable _dtCopy;
        private string _MENUID = string.Empty;
        private bool _LoadChk = false;
        private string NO_EQP = string.Empty;

        DispatcherTimer _timer = new DispatcherTimer();
        private int sec = 0;

        private bool bUseFlag = false; //2023.09.21 NA1동 JGF 설비명 설비 설명 표시
        public FCS001_313()
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
            try
            {
                _LoadChk = false;
                string sLaneID = string.Empty;

                if (string.IsNullOrEmpty(_MENUID))
                    _MENUID = LoginInfo.CFG_MENUID;

                sLaneID = GetLaneIDForMenu(_MENUID);

                if (string.IsNullOrEmpty(sLaneID))
                    _LANEID = "1";
                else
                    _LANEID = sLaneID;

                //Combo Setting
                InitCombo();
                //Legend Setting
                InitLegend();

                rdoTrayId.Checked += rdo_CheckedChanged;
                rdoTrayId.Unchecked += rdo_CheckedChanged;
                rdoLotId.Checked += rdo_CheckedChanged;
                rdoLotId.Unchecked += rdo_CheckedChanged;
                rdoOpStart.Checked += rdo_CheckedChanged;
                rdoOpStart.Unchecked += rdo_CheckedChanged;
                rdoTime.Checked += rdo_CheckedChanged;
                rdoTime.Unchecked += rdo_CheckedChanged;
                rdoTempPress.Checked += rdo_CheckedChanged;
                rdoTempPress.Unchecked += rdo_CheckedChanged;
                rdoRouteNextOp.Checked += rdo_CheckedChanged;
                rdoRouteNextOp.Unchecked += rdo_CheckedChanged;

                cboLane.SelectedValue = _LANEID;

                bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_002_JGF_NAME"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

                _LoadChk = true;
                btnSearch_Click(null, null);

                NO_EQP = ObjectDic.Instance.GetObjectName("NO_EQP");

                cboLane.SelectedValueChanged += cboLane_SelectedValueChanged;

                _timer = new DispatcherTimer();

                this.Loaded -= UserControl_Loaded;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (chkTimer.IsChecked == true)
            {
                chkTimer.IsChecked = false;
            }
        }

        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilterLane = { "", "1" };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilterLane);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            sec++;
            if (sec >= 10)
            {
                btnSearch_Click(null, null);
                sec = 0;
            }
        }

        #endregion

        #region Event

        private void cboLane_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = (RadioButton)sender;
            if (rdo.IsChecked == true) //2019-09-17 scpark 두번식 조회되는 증상 때문에 체크되는 radio 에 대해서만 처리되도록.
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!_LoadChk) return;

            ClearALL();
            GetFormationStatus();
        }

        private void dgFormation_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    bool bRH = false;
                    bool bCH = false;

                    #region Header Setting
                    //ROW Header 설정
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "1")).Equals(string.Empty))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        bRH = true;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //e.Cell.Presenter.FontStretch = FontStretches.Condensed;
                        if (e.Cell.Column.Index != 0 && e.Cell.Row.Index != 0)
                        {
                            //설비없음
                            e.Cell.Presenter.Background = new BrushConverter().ConvertFromString("Gray") as SolidColorBrush;
                            e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("White") as SolidColorBrush;
                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), "설비없음");
                        }

                        bRH = false;
                    }

                    //Column Header 설정
                    if (e.Cell.Column.Index == 0)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightGray);
                        e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString("Black") as SolidColorBrush;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        bCH = true;
                    }
                    else
                    {
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        //e.Cell.Presenter.FontStretch = FontStretches.Condensed;
                        bCH = false;
                    }
                    #endregion

                    if (bRH.Equals(true) || bCH.Equals(true))
                    {
                        return;
                    }
                    else
                    {
                        string ROWNUM = _dtCopy.Rows[e.Cell.Row.Index][(e.Cell.Column.Index + 1).ToString()].ToString();

                        if (!string.IsNullOrEmpty(ROWNUM))
                        {
                            string BCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "BCOLOR"));
                            string FCOLOR = Util.NVC(GetDtRowValue(ROWNUM, "FCOLOR"));
                            string TEXT = Util.NVC(GetDtRowValue(ROWNUM, "TEXT"));
                            string BOLD = Util.NVC(GetDtRowValue(ROWNUM, "BOLD"));

                            if (!string.IsNullOrEmpty(BCOLOR))
                                e.Cell.Presenter.Background = new BrushConverter().ConvertFromString(BCOLOR) as SolidColorBrush;

                            if (!string.IsNullOrEmpty(FCOLOR))
                                e.Cell.Presenter.Foreground = new BrushConverter().ConvertFromString(FCOLOR) as SolidColorBrush;

                            DataTableConverter.SetValue(e.Cell.Row.DataItem, (e.Cell.Column.Index + 1).ToString(), TEXT);

                            if (BOLD.Equals("Y"))
                                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                            else if (BOLD.Equals("N"))
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                        }

                        e.Cell.Presenter.Padding = new Thickness(0);
                        e.Cell.Presenter.Margin = new Thickness(0);
                        e.Cell.Presenter.BorderBrush = Brushes.DarkGray;
                        e.Cell.Presenter.BorderThickness = new Thickness(0, 1, 1, 0);
                    }
                }));

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgColor_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name) == "CBO_NAME")
                    {
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Background = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE1").ToString()) as SolidColorBrush;
                        e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.Foreground = new BrushConverter().ConvertFromString(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "ATTRIBUTE2").ToString()) as SolidColorBrush;
                    }
                }
            }));
        }

        private void dgFormation_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    // 값이 아니라 헤더 영역인 경우 종료
                    if (cell.Row.Index.Equals(0) || cell.Column.Index.Equals(0)) return;

                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    if (string.IsNullOrEmpty(ROWNUM)) return;
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));
                    string ROW = Util.NVC(GetDtRowValue(ROWNUM, "ROW"));
                    string COL = Util.NVC(GetDtRowValue(ROWNUM, "COL"));
                    string STG = Util.NVC(GetDtRowValue(ROWNUM, "STG"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string COLHEADER = _dtCopy.Rows[0][cell.Column.Index].ToString();
                    string ROWHEADER = _dtCopy.Rows[cell.Row.Index][0].ToString();

                    //if (string.IsNullOrEmpty(CSTID))
                    //    return;

                    if (rdoTrayInfo.IsChecked == true)
                    {
                        if (string.IsNullOrEmpty(CSTID))
                            return;

                        FCS001_021 wndRunStart = new FCS001_021();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[6];
                            Parameters[0] = CSTID;

                            this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                        }
                    }
                    else if (rdoEqpControl.IsChecked == true)
                    {
                        if (string.IsNullOrEmpty(EQPTID))
                            return;

                        FCS001_002_DETAIL wndRunStart = new FCS001_002_DETAIL();
                        wndRunStart.FrameOperation = FrameOperation;

                        if (wndRunStart != null)
                        {
                            object[] Parameters = new object[7];
                            Parameters[0] = ROW;
                            Parameters[1] = COL;
                            Parameters[2] = STG;
                            Parameters[3] = EQPTID;
                            Parameters[4] = _LANEID;
                            Parameters[5] = COLHEADER;
                            Parameters[6] = ROWHEADER;

                            C1WindowExtension.SetParameters(wndRunStart, Parameters);

                            wndRunStart.ShowModal();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgFormation_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgFormation.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    ClearControl();

                    //설비 정보 가져오기
                    string ROWNUM = _dtCopy.Rows[cell.Row.Index][(cell.Column.Index + 1).ToString()].ToString();
                    if (string.IsNullOrEmpty(ROWNUM)) return;

                    int i = 0;
                    if (!int.TryParse(ROWNUM, out i)) return;

                    string ROW_ID = Util.NVC(GetDtRowValue(ROWNUM, "ROW_ID"));
                    string COL_ID = Util.NVC(GetDtRowValue(ROWNUM, "COL_ID"));
                    string FORMSTATUS = Util.NVC(GetDtRowValue(ROWNUM, "FORMSTATUS"));
                    string EQPTID = Util.NVC(GetDtRowValue(ROWNUM, "EQPTID"));
                    string CSTID = Util.NVC(GetDtRowValue(ROWNUM, "CSTID"));

                    //TextBox Set
                    txtSelUnit.Text = ROW_ID;
                    txtSelNum.Text = COL_ID;

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "INDATA";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("EQPTID", typeof(string));
                    dtRqst.Columns.Add("CSTID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQPTID"] = EQPTID;
                    dr["CSTID"] = CSTID;
                    dtRqst.Rows.Add(dr);

                    DataSet dsRqst = new DataSet();
                    dsRqst.Tables.Add(dtRqst);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_GET_FORMATION_TRAY_EQP_MAINT", "INDATA", "TRAY,EQPMAINT,MAINT", dsRqst);

                    // Box 상태정보
                    if (dsRslt.Tables["TRAY"].Rows.Count > 0 && !FORMSTATUS.Equals("16")) //Trouble이 아닐경우
                    {
                        if (string.IsNullOrEmpty(CSTID))
                        {
                            txtStatus.Text = string.Empty;
                        }
                        else
                        {
                            txtStatus.Text = dsRslt.Tables["TRAY"].Rows[0]["CSTID"].ToString() + "\r\n"
                                     + dsRslt.Tables["TRAY"].Rows[0]["PROCID"].ToString() + "\r\n"
                                     + dsRslt.Tables["TRAY"].Rows[0]["WIPDTTM_ST"].ToString();
                        }
                    }
                    else if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0 && FORMSTATUS.Equals("16")) //Trouble
                    {
                        txtStatus.Text = ObjectDic.Instance.GetObjectName("TROUBLE");
                    }
                    else if (cell.Text.Length > 0)
                    {
                        txtStatus.Text = cell.Text;
                    }
                    else
                    {
                        txtStatus.Text = string.Empty;
                    }

                    // Box Trouble 정보
                    if (dsRslt.Tables["EQPMAINT"].Rows.Count > 0 && (FORMSTATUS.Equals("16") || FORMSTATUS.Equals("17"))) //Trouble 또는 일시정지
                    {
                        txtTroubleName.Text = dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_NAME"].ToString();
                        txtTroubleRepairWay.Text = dsRslt.Tables["EQPMAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString();
                    }
                    else if (dsRslt.Tables["MAINT"].Rows.Count > 0)
                    {
                        txtTroubleRepairWay.Text = dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_WAY"].ToString();

                        /* 2014.10.28 정종덕D // [CSR ID:2581928] FCS상 충방전기 부동 전환 Logic 변경 및 비가동 Box율 정보 표시 기능 추가 
                             * 부동전환시 박스 표시 변경, 조건 변경
                             * */
                        if (dsRslt.Tables["MAINT"].Rows[0]["MAINT_STAT_CODE"].ToString().Equals("C"))
                        {
                            txtRemark.Text = dsRslt.Tables["MAINT"].Rows[0]["TROUBLE_REPAIR_NAME"].ToString();
                        }
                    }
                    else
                    {
                        txtTroubleName.Text = string.Empty;
                        txtTroubleRepairWay.Text = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void chkTimer_Checked(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromTicks(10000000);  //1초
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();
        }

        private void chkTimer_Unchecked(object sender, RoutedEventArgs e)
        {
            _timer.Tick -= new EventHandler(timer_Tick);
            _timer.Stop();
        }

        #endregion

        #region Method

        private string GetLaneIDForMenu(string sMenuID)
        {
            string sLaneID = string.Empty;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_JIG_CHARGE_MENU_ID";
                dr["CMCODE"] = _MENUID;
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    sLaneID = dtRslt.Rows[0]["ATTRIBUTE1"].ToString();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
            return sLaneID;
        }

        private void InitLegend()
        {
            try
            {
                C1ComboBoxItem cbItemTiTle = new C1ComboBoxItem { Content = ObjectDic.Instance.GetObjectName("- LEGEND -") };
                cboColorLegend.Items.Add(cbItemTiTle);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "FORM_JIGFORMSTATUS";
                RQSTDT.Rows.Add(dr);

                ShowLoadingIndicator();
                _dtColor = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_ALL_ITEMS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in _dtColor.Rows)
                {
                    if (row["ATTRIBUTE1"].ToString().IsNullOrEmpty())
                    {
                        continue;
                    }

                    C1ComboBoxItem cbItem = new C1ComboBoxItem
                    {
                        Content = row["CBO_NAME"].ToString(),
                        Background = new BrushConverter().ConvertFromString(row["ATTRIBUTE1"].ToString()) as SolidColorBrush,
                        Foreground = new BrushConverter().ConvertFromString(row["ATTRIBUTE2"].ToString()) as SolidColorBrush,
                    };
                    cboColorLegend.Items.Add(cbItem);
                }

                cboColorLegend.SelectedIndex = 0;

                //-----------------------------------------------------
                CommonCombo_Form _combo = new CommonCombo_Form();

                _combo.SetCombo(cboOperLegend, CommonCombo_Form.ComboStatus.NONE, sCase: "CMN", sFilter: new string[] { "JIGFORMATION_STATUS_NEXT_PROC" });

                //------------------------------------------------------
                //Util.GridSetData(dgColor, _dtColor, FrameOperation, true);
                dgColor.ItemsSource = DataTableConverter.Convert(_dtColor);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void GetFormationStatus()
        {
            try
            {
                Util.gridClear(dgFormation);

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                //20220419_에러 수정(개체 참조가 개체의 인스턴스로 설정되지 않았습니다.) START
                //if (!string.IsNullOrEmpty(cboLane.SelectedValue.ToString()))
                //    dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                if (!string.IsNullOrEmpty(Util.NVC(cboLane.SelectedValue)))
                {
                    dr["LANE_ID"] = Util.GetCondition(cboLane, bAllNull: true);
                }
                else
                {
                    dr["LANE_ID"] = DBNull.Value;
                }
                //20220419_에러 수정(개체 참조가 개체의 인스턴스로 설정되지 않았습니다.) END

                dtRqst.Rows.Add(dr);

                _dtDATA = new ClientProxy().ExecuteServiceSync("DA_SEL_JIG_FORMATION_VIEW", "RQSTDT", "RSLTDT", dtRqst);

                if (_dtDATA.Rows.Count == 0)
                {
                    ////임시
                    ////GetTestData1(ref _dtDATA);
                    ////조회된 값이 없습니다.
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0232"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    //{
                    //    if (result == MessageBoxResult.OK)
                    //    {
                    //    }
                    //});
                    //return;

                    //Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                }
                else
                {
                    dgFormation.Columns.Clear();
                    dgFormation.Refresh();

                    SetfpsFormationData(_dtDATA);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetfpsFormationData(DataTable dtRslt)
        {
            try
            {
                DataTable dt = new DataTable();
                DataTable dtCol = new DataTable();
                dtCol.TableName = "RQSTDT";
                dtCol.Columns.Add("COL_ID", typeof(string));
                dtCol.Columns.Add("INDEX", typeof(string));
                int iCOLINDEX = 0;
                string sCOL_ID = string.Empty;
                string sStatus = string.Empty;
                bool bBold = false;

                //GRID에 Binding할 Datatable Setting하기
                //MAX ROW
                int iMaxRow = Convert.ToInt16(dtRslt.Compute("MAX(ROW_IDX)", string.Empty));

                //Row Height get
                //double dRowHeight = (dgFormation.ActualHeight) / iMaxRow - 1.6;
                double dRowHeight = (dgFormation.ActualHeight) / iMaxRow - 10;

                //MAX COL
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    sCOL_ID = dtRslt.Rows[i]["COL_ID"].ToString();

                    if (i == 0)
                    {
                        DataRow row = dtCol.NewRow();
                        row["COL_ID"] = sCOL_ID;
                        row["INDEX"] = iCOLINDEX;
                        dtCol.Rows.Add(row);

                        iCOLINDEX++;
                    }
                    else
                    {
                        if (sCOL_ID != dtRslt.Rows[i - 1]["COL_ID"].ToString())
                        {
                            DataRow row = dtCol.NewRow();
                            row["COL_ID"] = sCOL_ID;
                            row["INDEX"] = iCOLINDEX;
                            dtCol.Rows.Add(row);

                            iCOLINDEX++;
                        }
                    }
                }

                //MAX COL
                int iMaxCol = dtCol.Rows.Count + 1; //Header 포함

                //Column 생성
                for (int i = 0; i < iMaxCol; i++)
                {
                    double dWidth;

                    if (i == 0)
                        dWidth = 200;
                    else
                        dWidth = 280;

                    //GRID Column Create
                    SetGridHeaderSingle((i + 1).ToString(), dgFormation, dWidth);

                    dt.Columns.Add((i + 1).ToString(), typeof(string));
                }

                //Row 생성
                for (int i = 0; i < iMaxRow; i++)
                {
                    DataRow row = dt.NewRow();
                    dt.Rows.Add(row);
                }

                //Column Header 생성
                DataRow Header = dt.NewRow();
                for (int i = 0; i < dtCol.Rows.Count + 1; i++)
                {
                    if (i != 0)
                    {
                        Header[i] = dtCol.Rows[i - 1]["COL_ID"].ToString();
                    }
                }

                //Row Header 
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow[] dr = dtRslt.Select("ROW_IDX = '" + (i + 1) + "'");

                    if (dr.Length > 0)
                    {
                        /// NA 1동에서 J/F 설비명이 맞지 않아 설비 설명으로 대체 표시
                        if (bUseFlag)
                        {
                            if (dr[0]["EQPTDESC"] != null)
                            {
                                string[] LaneId = dr[0]["EQPTDESC"].ToString().Split(' ');
                                if (LaneId.Length < 3)
                                {
                                    dt.Rows[i]["1"] = LaneId[0].ToString();
                                }
                                else
                                {
                                    dt.Rows[i]["1"] = LaneId[2].ToString();

                                }
                            }
                        }
                        else
                        {
                            dt.Rows[i]["1"] = dr[0]["ROW_ID"].ToString();
                        }



                    }
                }

                //dtRslt Column Add
                dtRslt.Columns.Add("BCOLOR", typeof(string));
                dtRslt.Columns.Add("FCOLOR", typeof(string));
                dtRslt.Columns.Add("TEXT", typeof(string));
                dtRslt.Columns.Add("BOLD", typeof(string));

                int COLPOS;
                int iROW;
                int iCOL;
                int iSTG;
                int iROW_IDX;
                string COL_ID = string.Empty;
                string FORMSTATUS = string.Empty;
                string CSTID = string.Empty;
                string LOTID = string.Empty;
                string PROD_LOTID = string.Empty; //2021.06.03 추가
                string OP_START_TIME = string.Empty;
                string OP_OVER_TIME = string.Empty;
                string TEMP_VAL = string.Empty;
                string PRESS_VAL = string.Empty;
                string LAST_RUN_TIME = string.Empty;
                string ATCALIB_TYPE_CODE = string.Empty; //20211018 Auto Calibration Lot표시 추가
                string ROUTID = string.Empty;
                string NEXT_PROCID = string.Empty;

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    iROW = int.Parse(dtRslt.Rows[i]["ROW"].ToString());
                    iCOL = int.Parse(dtRslt.Rows[i]["COL"].ToString());
                    iSTG = int.Parse(dtRslt.Rows[i]["STG"].ToString());
                    iROW_IDX = int.Parse(dtRslt.Rows[i]["ROW_IDX"].ToString());
                    COL_ID = dtRslt.Rows[i]["COL_ID"].ToString();
                    FORMSTATUS = Util.NVC(dtRslt.Rows[i]["FORMSTATUS"].ToString());
                    CSTID = Util.NVC(dtRslt.Rows[i]["CSTID"].ToString());
                    LOTID = Util.NVC(dtRslt.Rows[i]["LOTID"].ToString());
                    PROD_LOTID = Util.NVC(dtRslt.Rows[i]["PROD_LOTID"]);
                    OP_START_TIME = Util.NVC(dtRslt.Rows[i]["OP_START_TIME"].ToString());
                    OP_OVER_TIME = Util.NVC(dtRslt.Rows[i]["OP_OVER_TIME"].ToString());
                    TEMP_VAL = Util.NVC(dtRslt.Rows[i]["TEMP_VAL"].ToString());
                    PRESS_VAL = Util.NVC(dtRslt.Rows[i]["PRESS_VAL"].ToString());
                    LAST_RUN_TIME = Util.NVC(dtRslt.Rows[i]["LAST_RUN_TIME"].ToString());
                    ATCALIB_TYPE_CODE = Util.NVC(dtRslt.Rows[i]["ATCALIB_TYPE_CODE"].ToString()); //20211018 Auto Calibration Lot표시 추가
                    ROUTID = Util.NVC(dtRslt.Rows[i]["ROUTID"].ToString());
                    NEXT_PROCID = Util.NVC(dtRslt.Rows[i]["NEXT_PROCID"].ToString());

                    COLPOS = GetColPos(COL_ID, dtCol);

                    dt.Rows[iROW_IDX - 1][COLPOS + 1] = i;

                    string BCOLOR = "Black";
                    string FCOLOR = "White";
                    string TEXT = string.Empty;

                    DataRow[] drColor = _dtColor.Select("CBO_CODE = '" + FORMSTATUS + "'");

                    if (drColor.Length > 0)
                    {
                        BCOLOR = drColor[0]["ATTRIBUTE1"].ToString();
                        FCOLOR = drColor[0]["ATTRIBUTE2"].ToString();
                    }

                    if (rdoTrayId.IsChecked == true)
                        sStatus = CSTID;
                    else if (rdoLotId.IsChecked == true)
                    {
                        //20211018 Auto Calibration Lot표시 추가 START
                        //sStatus = PROD_LOTID; //2021.06.03 PSM : TRAY LOT ID -> PKG LOT ID로 변경
                        if (!string.IsNullOrEmpty(ATCALIB_TYPE_CODE))
                        {
                            sStatus = PROD_LOTID + " [Auto Calib]";
                        }
                        else
                        {
                            sStatus = PROD_LOTID;
                        }
                        //20211018 Auto Calibration Lot표시 추가 END
                    }
                    else if (rdoTime.IsChecked == true)
                        sStatus = OP_OVER_TIME;
                    else if (rdoOpStart.IsChecked == true)
                        sStatus = OP_START_TIME;
                    else if (rdoTempPress.IsChecked == true)
                        sStatus = string.Format("{0} / {1}", string.IsNullOrEmpty(TEMP_VAL) ? "" : (Convert.ToDouble(TEMP_VAL) / 10).ToString("F1"), PRESS_VAL);
                    else if (rdoRouteNextOp.IsChecked == true)
                        sStatus = ROUTID + (NEXT_PROCID.Equals(string.Empty) ? "" : " [" + NEXT_PROCID + "]");

                    switch (FORMSTATUS)
                    {
                        case "01": // 통신두절
                            sStatus = ObjectDic.Instance.GetObjectName("COMM_LOSS");
                            break;
                        case "19": //Power Off
                            sStatus = "Power Off";
                            break;
                        case "21": //정비중
                            sStatus = ObjectDic.Instance.GetObjectName("MAINTENANCE") + ")" + LAST_RUN_TIME; //200611 KJE : 정비중 시간 추가
                            break;
                        case "25": //수리중
                            sStatus = ObjectDic.Instance.GetObjectName("REPAIR") + ")" + LAST_RUN_TIME; //200611 KJE : 수리중 시간 추가
                            break;
                        case "26": //임시예약
                            sStatus = ObjectDic.Instance.GetObjectName("TMP_RESV");
                            break;
                        case "36": //화재
                            sStatus = ObjectDic.Instance.GetObjectName("FIRE");
                            break;
                    }

                    dtRslt.Rows[i]["BCOLOR"] = BCOLOR;
                    dtRslt.Rows[i]["FCOLOR"] = FCOLOR;
                    dtRslt.Rows[i]["TEXT"] = sStatus;
                    dtRslt.Rows[i]["BOLD"] = (bBold == true) ? "Y" : "N";

                }

                //DataTable Header Insert
                dt.Rows.InsertAt(Header, 0);

                //DataTable Copy
                _dtCopy = dt.Copy();

                dgFormation.ItemsSource = DataTableConverter.Convert(dt);

                //dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(20, DataGridUnitType.Pixel);
                dgFormation.RowHeight = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                dgFormation.UpdateLayout();
                //dgFormation.ItemsSource = DataTableConverter.Convert(dt);

                //for (int i = 0; i < dgFormation.Rows.Count; i++)
                //{
                //    //dgFormation.RowHeight[i] = C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                //    //dgFormation.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(dRowHeight, DataGridUnitType.Pixel);
                //    dgFormation.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(50, DataGridUnitType.Pixel);
                //    //dgFormation.Rows[i]
                //}
                //dgFormation.UpdateLayout();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private string getMaintName(string sEqpId, string sLastRunTime) //작업자 이름 가져오기, 부동유형 가져오기
        {
            string sReturn = string.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQP_ID"] = sEqpId;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_FORMATION_TRAY_EQP_MAINT", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                    sReturn = ObjectDic.Instance.GetObjectName("MANUAL") + " " + sLastRunTime;  //수동
                else
                    sReturn = dtRslt.Rows[0]["TROUBLE_REPAIR_CD2"].ToString() + " " + dtRslt.Rows[0]["TROUBLE_REPAIR_TIME"].ToString();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sReturn;
        }

        private int GetColPos(string sVal, DataTable dt)
        {
            int iRtnVal = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (dt.Rows[i]["COL_ID"].ToString() == sVal)
                    {
                        iRtnVal = int.Parse(dt.Rows[i]["INDEX"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return iRtnVal;
        }

        private void ClearALL()
        {
            dgFormation.ItemsSource = null;
            //dgFormation.Columns.Clear();
            //dgFormation.Refresh();
            _dtCopy = null;
            _dtDATA = null;

            ClearControl();
        }

        private void ClearControl()
        {
            txtSelNum.Text = string.Empty;
            txtSelUnit.Text = string.Empty;
            txtStatus.Text = string.Empty;
            txtTroubleName.Text = string.Empty;
            txtTroubleRepairWay.Text = string.Empty;
            txtRemark.Text = string.Empty;
        }

        private void SetGridHeaderSingle(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)

            });
        }

        private string GetDtRowValue(string iRowNum, string sFindCol)
        {
            string sRtnValue = string.Empty;
            try
            {
                if (int.Parse(iRowNum) >= _dtDATA.Rows.Count)
                    return sRtnValue;

                sRtnValue = _dtDATA.Rows[int.Parse(iRowNum)][sFindCol].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sRtnValue;
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
