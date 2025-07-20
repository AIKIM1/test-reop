/*************************************************************************************
 Created Date : 2023.04.28
      Creator : 유재홍
   Decription : FOIL적치대 현황 조회
--------------------------------------------------------------------------------------
 [Change History]
 2023.04.28   유재홍   최초생성
 2023.06.19   강성묵   권치방향 설정 기능 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
//추가
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.Windows.Media;
using System.Linq;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_376.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_376 : UserControl, IWorkArea
    {
        CommonCombo combo = new CommonCombo();
        Util _Util = new Util();


        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();
        private bool _isSelectedAutoTime = false;
        private bool _isLoaded = false;
        

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_376()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }
        

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= UserControl_Loaded;
            initcombo();
            TimerSetting();
            
            _isLoaded = true;
        }





        private void initcombo()
        {
            // 동
            SetAreaCombo(cboArea);


             //유형
            SetRackTypeCombo(cboRacktype);

            // 극성
            SetPolarityCombo(cboPolarity);

            // 적치대 ID
            SetRackIDCombo(cboRackID);

        }




        private void SetRackIDCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MHS_SEL_FOIL_RACKID_CBO";
            string[] arrColumn = { "LANGID", "AREAID","RACK_TYPE","ELEC_TYPE_CODE"};
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue?.ToString(), cboRacktype.SelectedValue?.ToString(),cboPolarity.SelectedValue?.ToString() };//적치대 리스트 biz*수정필요*
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText);
        }

        private void SetPolarityCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELEC_TYPE" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText);
        }

        private void SetRackTypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR";
            string[] arrColumn = { "LANGID","AREAID","COM_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, cboArea.SelectedValue.ToString(),"FOIL_WAIT_STAGE_TYPE" };//적치대 타입 입력 공통코드 *수정필요*
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private static void SetAreaCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_AREA_BY_AREATYPE_CBO";
            string[] arrColumn = { "LANGID", "AREAID","AREA_TYPE_CODE" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "E" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_AREA_ID);
        }






        //미완성
        private void SearchData()
        {
           


            ShowLoadingIndicator();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("RACK_TYPE", typeof(string));
            inDataTable.Columns.Add("ELEC_TYPE_CODE", typeof(string));
            inDataTable.Columns.Add("RACKID", typeof(string));


            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = cboArea.SelectedValue?.ToString();
            newRow["RACK_TYPE"] = cboRacktype.SelectedValue?.ToString(); 
            newRow["ELEC_TYPE_CODE"] = cboPolarity.SelectedValue?.ToString();
            newRow["RACKID"] = cboRackID.SelectedValue?.ToString();

            inDataTable.Rows.Add(newRow);




            new ClientProxy().ExecuteService("DA_MHS_SEL_FOIL_RACK_MTRL_INFO",  "INDATA", "RSLTDT", inDataTable, (searchResult, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    Util.GridSetData(dgLotInfo, searchResult, FrameOperation, false);

                    //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ATTR1")) == "EMPTY"
                    //&& Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MTRL_CSTID")) != "")
                    //{
                    //    if (e.Row.Presenter != null)
                    //    {
                    //        e.Row.GetCellPresenter.Background = (new SolidColorBrush(Colors.Red));
                    //        e.Row.Presenter.Background = (new SolidColorBrush(Colors.Red));
                    //    }
                    //}

                    //for (int i = 0; i < dgLotInfo.Rows.Count; i++)
                    //{
                    //    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "ATTR1")) == "EMPTY"
                    //    && Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "MTRL_CSTID")) != "")
                    //    {
                    //        for(int j = 0; j < dgLotInfo.Columns.Count; j++)
                    //        {
                    //            dgLotInfo.GetCell(i, j).Presenter.Background = new SolidColorBrush(Colors.Red);
                    //        }
                    //        //if (dgLotInfo.Rows[i].Presenter != null)
                    //        //{
                    //        //    dgLotInfo.Rows[i].Presenter.Backgroundbrush = new SolidColorBrush();
                    //        //}
                    //    }
                    //}

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    
                    HiddenLoadingIndicator();
                }
            }
            );
        }

        private void SetEMSectionRollDirctn(string sWindingSide)
        {
            // 2023.06.19   강성묵   권치방향 설정 기능 추가

            if (dgLotInfo.CurrentRow == null || dgLotInfo.CurrentRow.DataItem == null || dgLotInfo.CurrentRow.Index < 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return;
            }

            string sMtrlCstId = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentRow.DataItem, "MTRL_CSTID"));

            if (sMtrlCstId == "")
            {
                // 캐리어 정보가 없습니다.
                Util.MessageValidation("SFU4564");
                return;
            }

            string sCstStat = Util.NVC(DataTableConverter.GetValue(dgLotInfo.CurrentRow.DataItem, "CSTSTAT"));

            if (sCstStat != "U")
            {
                object[] parameters = new object[1];
                parameters[0] = sMtrlCstId;

                // %1은 이미 Empty 상태인 Carrier 입니다.
                Util.MessageValidation("SFU4893", parameters);
                return;
            }

            // [%1]의 상태를 변경하시겠습니까?
            object[] oMtrlCstId = { sMtrlCstId };
            Util.MessageConfirm("SFU8195", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        loadingIndicator.Visibility = Visibility.Visible;

                        DataTable dtInDataTable = new DataTable();
                        dtInDataTable.Columns.Add("MTRL_CSTID", typeof(string));
                        dtInDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                        dtInDataTable.Columns.Add("UPDUSER", typeof(string));
                        dtInDataTable.Columns.Add("UPDDTTM", typeof(DateTime));

                        dtInDataTable.Clear();

                        DataRow drParam = dtInDataTable.NewRow();
                        drParam["MTRL_CSTID"] = sMtrlCstId;
                        drParam["EM_SECTION_ROLL_DIRCTN"] = sWindingSide;
                        drParam["UPDUSER"] = LoginInfo.USERID;
                        drParam["UPDDTTM"] = DateTime.Now;

                        dtInDataTable.Rows.Add(drParam);

                        new ClientProxy().ExecuteServiceSync("DA_MHS_UPD_TB_MHS_MTRL_CST", "INDATA", null, dtInDataTable);

                        Util.MessageInfo("SFU1166");

                        // Refresh
                        SearchData();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
            }, oMtrlCstId);
        }

        private void Record_blue(int i)
        {
            dgLotInfo.Rows[i].Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#87CEEB"));
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }



        private void COM_RACKTYPE_CHANGED(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRackIDCombo(cboRackID);
        }

        private void COM_AREA_CHANGED(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRackIDCombo(cboRackID);
        }

        private void COM_ELECTYPE_CHANGED(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRackIDCombo(cboRackID);
        }



        #region  타이머 콤보박스 이벤트 : cboTimer_SelectedValueChanged()
        /// <summary>
        /// 타이머 콤보박스 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        #endregion


        #region 타이머 셋팅 : TimerSetting()
        /// <summary>
        /// 타이머 셋팅
        /// </summary>
        private void TimerSetting()
        {
            CommonCombo combo = new CommonCombo();
            string[] filter = { "INTERVAL_MIN" };
            combo.SetCombo(cboTimer, CommonCombo.ComboStatus.NA, sFilter: filter, sCase: "COMMCODE");

            //if (cboTimer != null && cboTimer.Items.Count > 0)
            //    cboTimer.SelectedIndex = 3;

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


        #region 타이머 실행 이벤트 : _dispatcherTimer_Tick()
        /// <summary>
        /// 타이머 실행 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


                    Time_Re_Search();
                    
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

        #region 타이머 재조회 : Time_Re_Search()
        /// <summary>
        /// 창고 적재현황 조회
        /// </summary>
        private void Time_Re_Search()
        {
            SearchData();
        }




        #endregion

        private void dgLotInfo_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ATTR1")) == "EMPTY"
            //&& Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MTRL_CSTID")) != "")
            //{
            //    if (e.Row.Presenter != null)
            //    {
            //        e.Row.GetCellPresenter.Background = (new SolidColorBrush(Colors.Red));
            //        e.Row.Presenter.Background = (new SolidColorBrush(Colors.Red));
            //    }
            //}

            ////for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            ////{
            ////    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "ATTR1")) == "EMPTY"
            ////    && Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "MTRL_CSTID")) != "")
            ////    {
            ////        if (dgLotInfo.Rows[i].Presenter != null)
            ////        {
            ////            dgLotInfo.Rows[i].Presenter.Backgroundbrush = new SolidColorBrush();
            ////        }
            ////    }
            ////}
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "ATTR1")) == "EMPTY"
                    && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_CSTID")) != "")
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("LightSkyBlue");
                        if (convertFromString != null)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                        }
                    }
                }
            }));

            //if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "ATTR1")) == "EMPTY"
            //&& Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "MTRL_CSTID")) != "")
            //{
            //    if (e.Row.Presenter != null)
            //    {
            //        e.Row.GetCellPresenter.Background = (new SolidColorBrush(Colors.Red));
            //        e.Row.Presenter.Background = (new SolidColorBrush(Colors.Red));
            //    }
            //}

            //for (int i = 0; i < dgLotInfo.Rows.Count; i++)
            //{
            //    if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "ATTR1")) == "EMPTY"
            //    && Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[i].DataItem, "MTRL_CSTID")) != "")
            //    {
            //        for(int j = 0; j < dgLotInfo.Columns.Count; j++)
            //        {
            //            dgLotInfo.GetCell(i, j).Presenter.Background = new SolidColorBrush(Colors.Red);
            //        }
            //        //if (dgLotInfo.Rows[i].Presenter != null)
            //        //{
            //        //    dgLotInfo.Rows[i].Presenter.Backgroundbrush = new SolidColorBrush();
            //        //}
            //    }
            //}
        }

        private void btnEMSectionRollDirctn_U_Click(object sender, RoutedEventArgs e)
        {
            // 2023.06.19   강성묵   권치방향 설정 기능 추가
            
            try
            {
                SetEMSectionRollDirctn("U");
            }
            catch
            {
                // NoAction
            }
        }

        private void btnEMSectionRollDirctn_D_Click(object sender, RoutedEventArgs e)
        {
            // 2023.06.19   강성묵   권치방향 설정 기능 추가

            try
            {
                SetEMSectionRollDirctn("D");
            }
            catch
            {
                // NoAction
            }
        }
    }
}
