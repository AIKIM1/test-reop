/*************************************************************************************
 Created Date : 2023.07.14
      Creator : 장영철
   Decription : Lot별 불량현황 (동적구성)
--------------------------------------------------------------------------------------
 [Change History]
 --------------------------------------------------------------------------------------
     날 짜     이 름      CSR 번호                      내용
--------------------------------------------------------------------------------------
  2020.12.22  장영철 : Initial Created.  
  2024.08.23  김동일 : [E20240717-000992] - 조회 시 TEST Lot 유형 제외 조건 추가
  2025.07.04  김선영 : ESHG - DNC_5700 신규공정 -> LAMI 패널에 보여지도록 처리, ESHG는 LAMI 공정 없음.  
**************************************************************************************/
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Linq;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_347_Dynamic : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private decimal _yieldYellowReferenceValue = 0;
        private decimal _yieldRedReferenceValue = 0;

        private COM001_347_UC_PROCYIELD[] ucProc = null;

        public COM001_347_Dynamic()
        {
            InitializeComponent();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }
        #endregion

        #region Initialize
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetMultiCboEqsg();
            SetProcStackArea();

            TimerSetting();
            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // 2022.12.20 C20221001-000004 [ESWA PI] FMCS 시스템 상 양품률 (OK rate) 현황 모니터링 기능 추가_사용하지 않음
            //SetYieldColorReferenceValue(); 

            if(AREAID.Text != "")
            {
                //동적항목 초기화

                ucProc = null;
                proc_A7000.Children.Clear();
                proc_A8000.Children.Clear();
                proc_A9000.Children.Clear();
                proc_A7000.RowDefinitions.Clear();
                proc_A8000.RowDefinitions.Clear();
                proc_A9000.RowDefinitions.Clear();

                SetProcStackArea();
            }

            SetContents();
        }

        private void cboTimer_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!_isLoaded) return;
            try
            {
                if (_monitorTimer != null)
                {
                    _monitorTimer.Stop();

                    int second = 0;
                    if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.GetString()))
                    {
                        second = int.Parse(cboTimer.SelectedValue.ToString());
                        _isSelectedAutoTime = true;
                    }
                    else
                    {
                        _isSelectedAutoTime = false;
                    }

                    if (second == 0 && !_isSelectedAutoTime)
                    {
                        Util.MessageValidation("SFU8310");
                        return;
                    }
                    _monitorTimer.Interval = new TimeSpan(0, 0, second);
                    _monitorTimer.Start();

                    if (_isSelectedAutoTime)
                    {
                        if (cboTimer != null)
                            Util.MessageInfo("SFU8311", Convert.ToString(Convert.ToInt32(cboTimer.SelectedValue) / 60));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    SetContents();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }
        #endregion

        #region Method
        private void SetMultiCboEqsg()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                 dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EXCEPT_GROUP"] = "VD";
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT, (bizResult, exception) => {

                    if (exception != null)
                    {
                        throw exception;
                    }

                    try
                    {
                        multiCboEqsg.ItemsSource = DataTableConverter.Convert(bizResult);
                        multiCboEqsg.CheckAll();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetucGrid(Grid ucGr, COM001_347_UC_PROCYIELD ucProc, string eqgrname, string eqgrid) {

            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(1, GridUnitType.Star);

            RowDefinition rd1 = new RowDefinition();
            rd1.Height = new GridLength(1, GridUnitType.Auto);

            GridSplitter gsp = new GridSplitter();
            gsp.Height = 8;
            gsp.HorizontalAlignment = HorizontalAlignment.Stretch;
            gsp.ResizeDirection = GridResizeDirection.Rows;

            ucGr.RowDefinitions.Add(rd);
            ucGr.RowDefinitions.Add(rd1);

            int gridRowMax = ucGr.RowDefinitions.Count();

            Grid.SetRow(ucProc, gridRowMax-2);
            Grid.SetRow(gsp, gridRowMax-1);

            ucGr.Children.Add(ucProc);
            ucGr.Children.Add(gsp);

            ucProc.SetTitle(eqgrname.SafeToString());
            ucProc.SetEqgrid(eqgrid.SafeToString());

        }

        private void SetProcStackArea()
        {

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID_STR", typeof(string));

            LoginInfo.CFG_AREA_ID = AREAID.Text == "" ? LoginInfo.CFG_AREA_ID : AREAID.Text;

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID_STR"] = AREAID.Text == "" ? string.Join(",", multiCboEqsg.SelectedItems) : null;

            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_EQUIPMENTGROUP_BY_PROC";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt.Rows.Count > 0)
                {
                    ucProc = new COM001_347_UC_PROCYIELD[dtRslt.Rows.Count];

                    int ucIdx = 0;

                    foreach (DataRow row in dtRslt.Rows)
                    {
                        string procid = row["PROCID_GR"].SafeToString();
                        string eqgrid = row["EQGRID"].SafeToString();
                        string eqgrname = row["EQGRNAME"].SafeToString();
                        
                        if (row["PROCID_GR"].Equals("A7")) {

                            ucProc[ucIdx] = new COM001_347_UC_PROCYIELD();
                            ucProc[ucIdx].SetProcId(procid);
                            SetucGrid(proc_A7000, ucProc[ucIdx], eqgrname, eqgrid);
                            ucIdx++;
                        }
                        else if (row["PROCID_GR"].Equals("A8"))
                        {
                            ucProc[ucIdx] = new COM001_347_UC_PROCYIELD();
                            ucProc[ucIdx].SetProcId(procid);
                            SetucGrid(proc_A8000, ucProc[ucIdx], eqgrname, eqgrid);
                            ucIdx++;
                        }
                        else if (row["PROCID_GR"].Equals("A9"))
                        {
                            ucProc[ucIdx] = new COM001_347_UC_PROCYIELD();
                            ucProc[ucIdx].SetProcId(procid);
                            SetucGrid(proc_A9000, ucProc[ucIdx], eqgrname, eqgrid);
                            ucIdx++;
                        }
                        else if (row["PROCID_GR"].Equals("A5"))     //2025.07.04  김선영 : ESHG - DNC_5700 신규공정 -> LAMI 패널에 보여지도록 처리, ESHG는 LAMI 공정 없음 
                        {
                            if(LoginInfo.CFG_AREA_ID.Equals("AK"))  //ESHG
                            {
                                ucProc[ucIdx] = new COM001_347_UC_PROCYIELD();
                                ucProc[ucIdx].SetProcId(procid);
                                SetucGrid(proc_A7000, ucProc[ucIdx], eqgrname, eqgrid);
                                ucIdx++;
                            } 
                        }
                    }

                    

                }
            }
            catch (Exception ex)
            {
                 Util.MessageException(ex);
            }
        }

        private void SetContents()
        {
            try
            {
                if(ucProc == null) { return;  }

                for (int i = 0; i < ucProc.Length; i++)
                {
                    if (ucProc[i] != null) ucProc[i].SetContents(multiCboEqsg, chkTestLotExclude?.IsChecked == true ? "Y" : "N");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            if (cboTimer != null && cboTimer.Items.Count > 0)
                cboTimer.SelectedIndex = 1;

            if (_monitorTimer != null)
            {
                int second = 0;

                if (!string.IsNullOrEmpty(cboTimer?.SelectedValue?.ToString()))
                    second = int.Parse(cboTimer.SelectedValue.ToString());

                _monitorTimer.Tick += _dispatcherTimer_Tick;
                _monitorTimer.Interval = new TimeSpan(0, 0, second);

                _monitorTimer.Start();

            }
        }

        #endregion

        #region [ Util ] - DA
        private void SetYieldColorReferenceValue()
        {
            _yieldYellowReferenceValue = 0;
            _yieldRedReferenceValue = 0;

            DataTable inTable = new DataTable();
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "REAL_TIME_YIELD_MONITORING_INFO";
            dr["COM_CODE"] = "YIELD_COLOR_REFERENCE_VALUE";

            inTable.Rows.Add(dr);

            string bizName = "DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA";

            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", inTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    decimal decAttr1 = 0;
                    string strAttr1 = dtRslt.Rows[0]["ATTR1"]?.ToString();

                    if (decimal.TryParse(strAttr1, out decAttr1))
                    {
                        _yieldYellowReferenceValue = decAttr1;
                    }

                    decimal decAttr2 = 0;
                    string strAttr2 = dtRslt.Rows[0]["ATTR2"]?.ToString();

                    if (decimal.TryParse(strAttr2, out decAttr2))
                    {
                        _yieldRedReferenceValue = decAttr2;
                    }
                }
            }
            catch (Exception ex)
            {
                // Util.MessageException(ex);
            }
        }
        #endregion

        private void multiCboEqsg_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                //동적항목 초기화

                ucProc = null;
                proc_A7000.Children.Clear();
                proc_A8000.Children.Clear();
                proc_A9000.Children.Clear();
                proc_A7000.RowDefinitions.Clear();
                proc_A8000.RowDefinitions.Clear();
                proc_A9000.RowDefinitions.Clear();

                //동적항목 새로그림
                SetProcStackArea();
            }
            catch (Exception ex) {

            }
        }
    }
}
