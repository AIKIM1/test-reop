/*************************************************************************************
 Created Date : 2023.12.04
      Creator : 
   Decription : 
                
--------------------------------------------------------------------------------------
 [Change History]
  2023.12.04  DEVELOPER : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Configuration;
using System.Linq;
using System.Windows.Threading;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_393 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        bool bFinishLoading = false;
        bool bFirstTimeShowSecondTab = true;

        //SolidColorBrush _colExtraBatch = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush _colExtraBatch = new SolidColorBrush(Color.FromRgb(193, 196, 201));
        SolidColorBrush _colNormal = new SolidColorBrush(Colors.Black);
        SolidColorBrush _colEnableClick = new SolidColorBrush(Colors.Blue);

        DispatcherTimer _SecondTabInitTimer = null;

        public COM001_393()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        CommonCombo _combo = new CommonCombo();


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //일반 콤보박스
            _combo.SetCombo(cboAreaSupply, CommonCombo.ComboStatus.SELECT, sCase: "AREA");
            _combo.SetCombo(cboAreaUse, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

        }

        private void InitMulticombo()
        {
            //멀티셀렉트 콤보박스
            //1번탭
            SetEquipmentCombo(cboAreaSupply, cboEqptSupply);
            SetCommonCodeMultiCombo("ROLLMAP_SLURRY_MOUNT_PSTN_ID", cboEqptMeasrPstnSupply);

            //2번탭
            //여기서 하는건 실제로 타지 않음. 그냥 구성 맞출려고 넣어놓은것임.
            //SecondInitControl 여기에서 실제로 데이터를 가져옴.
            //PJT 일반 TEXT BOX로 변경 하여 주석처리함
            SetEquipmentCombo(cboAreaUse, cboEqptUse);
            //SetCommonCodeMultiCombo("ROLLMAP_SLURRY_MOUNT_PSTN_ID", cboPJTUse);

        }

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitMulticombo();

            foreach (C1.WPF.DataGrid.DataGridColumn col in dgSlurrySupply.Columns)
            {
                if (col.Name.ToString() == "BATCH_LOTID_2" || col.Name.ToString() == "BATCH_LOTID_3")
                {
                    Style styleHeader = new Style(typeof(DataGridColumnHeaderPresenter));
                    styleHeader.Setters.Add(new Setter { Property = ForegroundProperty, Value = _colExtraBatch });
                    col.HeaderStyle = styleHeader;
                }
            }

            bFinishLoading = true;   //이건 항상 마지막에..
        }

        private void btnSearchSupply_Click(object sender, RoutedEventArgs e)
        {
            searchSlurrySupply();
        }

        private void btnSearchUse_Click(object sender, RoutedEventArgs e)
        {
            searchSlurryUse();
        }

        private void cboAreaSupply_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return; //로딩전에는 여러번 탈 수 있기에 건너뛰어주기
            SetEquipmentCombo(cboAreaSupply, cboEqptSupply);
        }

        private void cboAreaUse_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (bFinishLoading == false) return; //로딩전에는 여러번 탈 수 있기에 건너뛰어주기
            SetEquipmentCombo(cboAreaUse, cboEqptUse);
        }

        private void tabSlurry_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabSlurry.SelectedIndex == 1 && bFirstTimeShowSecondTab == true)
            {
                bFirstTimeShowSecondTab = false;
                if (_SecondTabInitTimer == null)
                {
                    _SecondTabInitTimer = new DispatcherTimer();
                    _SecondTabInitTimer.Tick += SecondInitControl;
                }

                _SecondTabInitTimer.Interval = TimeSpan.FromMilliseconds(100);
                _SecondTabInitTimer.Start();
            }
        }

        private void btnSlurryMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_ELEC_COATER_MANUAL_DRANE popup = new CMM_ELEC_COATER_MANUAL_DRANE { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = Util.GetCondition(cboAreaSupply);
                    //Parameters[1] = "A1ECOT001";
                    //popupSlurryMove.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSlurryInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // RollMap 실적 수정 Popup Call
                CMM_ELEC_COATER_MANUAL_INPUT popup = new CMM_ELEC_COATER_MANUAL_INPUT { FrameOperation = FrameOperation };

                if (popup != null)
                {
                    object[] Parameters = new object[10];
                    Parameters[0] = Util.GetCondition(cboAreaSupply);
                    //Parameters[1] = "A1ECOT001";
                    //popupSlurryMove.Closed += new EventHandler(PopupRollMapUpdate_Closed);
                    C1WindowExtension.SetParameters(popup, Parameters);

                    if (popup != null)
                    {
                        popup.ShowModal();
                        popup.CenterOnScreen();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSlurrySupply_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            if (e.Cell.Presenter == null) return;

            if (e.Cell.Column.Name == "BATCH_LOTID_1")
            {
                e.Cell.Presenter.Foreground = _colEnableClick;
                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                e.Cell.Presenter.Cursor = Cursors.Hand;
            }
            else if (e.Cell.Column.Name.ToString() == "BATCH_LOTID_2" || e.Cell.Column.Name.ToString() == "BATCH_LOTID_3")
            {
                e.Cell.Presenter.Foreground = _colExtraBatch;
                e.Cell.Presenter.FontWeight = FontWeights.Bold;
                e.Cell.Presenter.Cursor = Cursors.Hand;
            }
            else
            {
                e.Cell.Presenter.Foreground = _colNormal;
                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                e.Cell.Presenter.Cursor = Cursors.Arrow;
            }
        }

        private void dgSlurryUse_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null) return;
                if (dg.CurrentRow == null) return;
                if (dg.CurrentCell.Row == null) return;


                if (dg.CurrentCell.Column.Name == "SLURRY_LOT_ID_USER" || dg.CurrentCell.Column.Name == "SLURRY_LOT_ID_AUTO")
                {
                    string INPUT_LOTID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dg.CurrentCell.Column.Name));

                    if (INPUT_LOTID == "") return;

                    COM001_393_HISTORY popup = new COM001_393_HISTORY { FrameOperation = FrameOperation };

                    if (popup != null)
                    {
                        object[] Parameters = new object[10];
                        Parameters[0] = INPUT_LOTID;
                        Parameters[1] = "";
                        C1WindowExtension.SetParameters(popup, Parameters);

                        if (popup != null)
                        {
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }                
                else if(dg.CurrentCell.Column.Name == "LOTID")
                {
                    string INPUT_LOTID = Util.NVC(DataTableConverter.GetValue(dgSlurryUse.CurrentRow.DataItem, dg.CurrentCell.Column.Name));

                    if (INPUT_LOTID == "") return;

                    COM001_393_HISTORY popup = new COM001_393_HISTORY { FrameOperation = FrameOperation };

                    if (popup != null)
                    {
                        object[] Parameters = new object[10];
                        Parameters[0] = "";
                        Parameters[1] = INPUT_LOTID;
                        C1WindowExtension.SetParameters(popup, Parameters);

                        if (popup != null)
                        {
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgSlurryUse_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dg = sender as C1DataGrid;
            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null) return;

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "SLURRY_LOT_ID_USER" || e.Cell.Column.Name == "SLURRY_LOT_ID_AUTO" || e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = _colEnableClick;
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = _colNormal;
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;                 
                        e.Cell.Presenter.Cursor = Cursors.Arrow;
                    }
                }
            }));
        }

        private void dgSlurrySupply_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null) return;
                if (dg.CurrentRow == null) return;
                if (dg.CurrentCell.Row == null) return;


                if (dg.CurrentCell.Column.Name == "BATCH_LOTID_1" || dg.CurrentCell.Column.Name == "BATCH_LOTID_2" || dg.CurrentCell.Column.Name == "BATCH_LOTID_3")
                {
                    string INPUT_LOTID = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, dg.CurrentCell.Column.Name));

                    if (INPUT_LOTID == "") return;

                    COM001_393_HISTORY popup = new COM001_393_HISTORY { FrameOperation = FrameOperation };

                    if (popup != null)
                    {
                        object[] Parameters = new object[10];
                        Parameters[0] = INPUT_LOTID;
                        Parameters[1] = "";
                        C1WindowExtension.SetParameters(popup, Parameters);

                        if (popup != null)
                        {
                            popup.ShowModal();
                            popup.CenterOnScreen();
                        }
                    }
                }                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod
        private void searchSlurrySupply()
        {
            // 시스템 부하 방지를 위한 조회 조건 31일 제한
            if ((dtpDateToSupply.SelectedDateTime - dtpDateFromSupply.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31"); //기간은 %1일 이내 입니다.
                return;
            }

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                RQSTDT.Columns.Add("FROMDT", typeof(string));
                RQSTDT.Columns.Add("TODT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaSupply, "SFU3203"); //동은필수입니다.
                dr["EQPTID"] = cboEqptSupply.SelectedItemsToString;
                dr["EQPT_MOUNT_PSTN_ID"] = cboEqptMeasrPstnSupply.SelectedItemsToString;

                dr["FROMDT"] = Util.GetCondition(dtpDateFromSupply);
                dr["TODT"] = Util.GetCondition(dtpDateToSupply);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_MOVE_RPT_HIST_RM", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgSlurrySupply, dtRslt, FrameOperation);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void searchSlurryUse()
        {
            // 시스템 부하 방지를 위한 조회 조건 31일 제한
            if ((dtpDateToUse.SelectedDateTime - dtpDateFromUse.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31"); //기간은 %1일 이내 입니다.
                return;
            }

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("PRJ_NAME", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaUse, "SFU3203"); //동은필수입니다.
                dr["EQPTID"] = cboEqptUse.SelectedItemsToString;
                dr["PRJ_NAME"] = cboPJTUse.Text.Equals("") ? null : cboPJTUse.Text; 
                dr["LOTID"] = txtLotIdUse.Text.Equals("") ? null : txtLotIdUse.Text;

                dr["FROM_DATE"] = Util.GetCondition(dtpDateFromUse);
                dr["TO_DATE"] = Util.GetCondition(dtpDateToUse);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_USE_SITUATION_RM", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgSlurryUse, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SetEquipmentCombo(C1ComboBox cbArea, MultiSelectionBox mcb)
        {
            try
            {
                if (mcb.IsVisible == false) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = "E2000";     //코터 공정으로 고정
                dr["AREAID"] = Util.GetCondition(cbArea, "SFU3203");

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCommonCodeMultiCombo(String CMCDTYPE, MultiSelectionBox mcb)
        {
            try
            {
                if (mcb.IsVisible == false) return; // 컨트롤 생성되기 전에는 건너뛰기

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = CMCDTYPE;
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                    mcb.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        mcb.Check(-1);
                    }
                    else
                    {
                        mcb.isAllUsed = true;
                        mcb.CheckAll();
                    }
                }
                else
                {
                    mcb.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private void SecondInitControl(object sender, EventArgs e)
        {
            _SecondTabInitTimer.Stop();

            SetEquipmentCombo(cboAreaUse, cboEqptUse);
            //PJT 일반 TEXT BOX로 변경 하여 주석처리함
            //SetCommonCodeMultiCombo("ROLLMAP_SLURRY_MOUNT_PSTN_ID", cboPJTUse);

        }



        #endregion

        
    }
}
