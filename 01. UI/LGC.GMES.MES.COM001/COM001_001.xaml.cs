/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 생산계획 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2016.01.11  DEVELOPER : 조회조건 변경(C/Roll, S/Roll 선택 추가), 콤보박스, 조회 비즈 변경, 등록 버튼 삭제
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_001 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private string RecipeNo = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_001()
        {
            InitializeComponent();
            Initialize();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;            
        }

        private void Initialize()
        {
            selectedShop = LoginInfo.CFG_SHOP_ID;
            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;
            InitCombo();
            InitGrid();
            rdoEquipment.IsChecked = true;
            rdoSRoll.IsChecked = true;

            if (!LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals(Area_Type.ELEC))
            {
                lblRollMandMark.Visibility = Visibility.Collapsed;
                lblRollType.Visibility = Visibility.Collapsed;
                rdoCRoll.Visibility = Visibility.Collapsed;
                rdoSRoll.Visibility = Visibility.Collapsed;
            }
            //dgList.GroupBy(args.AddedItems[0] as C1.WPF.DataGrid.DataGridColumn);
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            Set_Combo_Shop(cboShop);
            //Set_Combo_Type(cboType);
            Set_Combo_WOType(cboDEMAND_TYPE);

            // 극성
            CommonCombo _combo = new CommonCombo();
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

        }
        #endregion

        #region Event
        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.dgList.Loaded -= dgList_Loaded;
                SetGridDate();
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //if (dgList.Rows.Count <= 3)
                //    return;

                //if (dgList.CurrentColumn.Name == "T_WOID")
                //{
                //CMM001.CMM_ELECRECIPE _RecipeNo = new CMM001.CMM_ELECRECIPE();
                //_RecipeNo.FrameOperation = FrameOperation;

                //if (_RecipeNo != null)
                //{
                //    //object[] Parameters = new object[1];
                //    //Parameters[0] = "";

                //    C1WindowExtension.SetParameters(_RecipeNo, null);

                //    _RecipeNo.Closed += new EventHandler(RecipeNo_Closed);
                //    _RecipeNo.ShowModal();
                //    _RecipeNo.CenterOnScreen();
                //}
                //}

                if (dgList == null || dgList.CurrentRow == null || dgList.CurrentRow.Index < 3 || !dgList.Columns.Contains("NOTE_FLAG"))
                    return;

                if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "NOTE_FLAG")).ToUpper().Equals("Y"))
                {
                    COM001_001_NOTE wndNote = new COM001_001_NOTE();
                    wndNote.FrameOperation = FrameOperation;

                    if (wndNote != null)
                    {
                        object[] Parameters = new object[5];
                        Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODNAME"));
                        Parameters[1] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "PRODID"));
                        Parameters[2] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "MODLID"));
                        Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "NOTE"));
                        //Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[dgList.CurrentRow.Index].DataItem, "POUCH_PROD_CHG_NOTE"));
                        C1WindowExtension.SetParameters(wndNote, Parameters);

                        wndNote.Closed += new EventHandler(wndNote_Closed);
                        
                        this.Dispatcher.BeginInvoke(new Action(() => wndNote.ShowModal()));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndNote_Closed(object sender, EventArgs e)
        {
            COM001_001_NOTE window = sender as COM001_001_NOTE;
            if (window.DialogResult == MessageBoxResult.OK)
            {
            }
        }        

        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboShop.SelectedIndex > -1)
                {
                    selectedShop = Convert.ToString(cboShop.SelectedValue);
                    Set_Combo_Area(cboArea);
                }
                else
                {
                    selectedShop = string.Empty;
                }
            }));
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_Process(cboProcess);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                }
                else
                {
                    selectedArea = string.Empty;
                }
            }));
        }

        private void cboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboProcess.SelectedIndex > -1)
                {
                    selectedProcess = Convert.ToString(cboProcess.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboEquipmentSegment.SelectedIndex > -1)
                {
                    selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
                    Set_Combo_Equipment(cboEquipment);
                    //Set_Combo_Process(cboProcess);
                }
            }));
        }


        private void cboEquipment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (cboEquipment.SelectedIndex > -1)
            //    {
            //        selectedEquipment = Convert.ToString(cboEquipment.SelectedValue);
            //        if (selectedEquipment == "")
            //        {
            //            dgList.Columns["EQPTID"].Visibility = Visibility.Collapsed;
            //        }
            //        else
            //        {
            //            dgList.Columns["EQPTID"].Visibility = Visibility.Visible;
            //        }                    
            //    }
            //    else
            //    {
            //        dgList.Columns["EQPTID"].Visibility = Visibility.Collapsed;
            //    }
            //}));
        }
        //private void btnLoad_Click(object sender, RoutedEventArgs e)
        //{
        //    //sSHOPID = Util.NVC(tmps[0]);
        //    //sAREAID = Util.NVC(tmps[1]);
        //    //sLINEID = Util.NVC(tmps[2]);
        //    //sPROCID = Util.NVC(tmps[3]);

        //    COM001_001_LOAD _xlsLoad = new COM001_001_LOAD();
        //    _xlsLoad.FrameOperation = FrameOperation;

        //    if (_xlsLoad != null)
        //    {
        //        object[] Parameters = new object[4];
        //        Parameters[0] = Util.NVC(cboShop.SelectedValue);
        //        Parameters[1] = Util.NVC(cboArea.SelectedValue);
        //        Parameters[2] = Util.NVC(cboEquipmentSegment.SelectedValue);
        //        Parameters[3] = Util.NVC(cboProcess.SelectedValue);

        //        C1WindowExtension.SetParameters(_xlsLoad, Parameters);

        //        _xlsLoad.Closed += new EventHandler(xlsLoad_Closed);
        //        _xlsLoad.ShowModal();
        //        _xlsLoad.CenterOnScreen();
        //    }
        //}
        private void rdoProcess_Checked(object sender, RoutedEventArgs e)
        {
            //cboEquipment.IsEnabled = true;
            //dgList.Columns["EQPTID"].Visibility = Visibility.Collapsed;
        }

        private void rdoEquipment_Checked(object sender, RoutedEventArgs e)
        {
            //cboEquipment.IsEnabled = false;
            //dgList.Columns["EQPTID"].Visibility = Visibility.Visible;
        }

        //private void txtProject_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        SearchData();
        //    }
        //}

        //private void txtModel_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        SearchData();
        //    }
        //}
        //private void txtProduct_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        SearchData();
        //    }
        //}


        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                string _Day = string.Empty;
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    ////오늘날짜 셀구분
                    //if(e.Cell.Column.Name.Length <= 1)
                    //{
                    //    _Day ="0" + e.Cell.Column.Name.ToString();
                    //}
                    //else
                    //{
                    //    _Day = e.Cell.Column.Name.ToString();
                    //}
                    //if (DateTime.Now.ToString("dd") == _Day && DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime))
                    //{
                    //    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightSteelBlue);
                    //}
                    //오늘날짜 셀구분
                    if (e.Cell.Column.Name.Contains("PLAN_QTY_"))
                    {
                        string[] splitDay = e.Cell.Column.Name.Split('_');
                        _Day = splitDay[2];
                    }
                    else
                    {
                        _Day = e.Cell.Column.Name.ToString();
                    }
                    if (DateTime.Now.ToString("dd") == _Day && DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Colors.LightSteelBlue);
                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Mehod
        private void Set_Combo_Shop(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("SYSID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["SYSID"] = LGC.GMES.MES.Common.LoginInfo.SYSID + "_" + LGC.GMES.MES.Common.Common.APP_System;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_SHOP_BY_USERID_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedShop) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedShop;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboShop_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Set_Combo_Area(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["SYSTEM_ID"] = LoginInfo.SYSID;
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = selectedShop;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    //DataRow dRow = result.NewRow();
                    //dRow["CBO_NAME"] = "-ALL-";
                    //dRow["CBO_CODE"] = "";
                    //result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedArea) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedArea;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboArea_SelectedItemChanged(cbo, null);
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_EquipmentSegmant(C1ComboBox cbo)
        {
            try
            {
                #region 변경 DA_BAS_SEL_EQUIPMENTSEGMENT_CBO -> DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR
                //DataTable dtRQSTDT = new DataTable();
                //dtRQSTDT.TableName = "RQSTDT";
                //dtRQSTDT.Columns.Add("LANGID", typeof(string));
                //dtRQSTDT.Columns.Add("AREAID", typeof(string));

                //DataRow drnewrow = dtRQSTDT.NewRow();
                //drnewrow["LANGID"] = LoginInfo.LANGID;
                //drnewrow["AREAID"] = selectedArea;
                //dtRQSTDT.Rows.Add(drnewrow);

                //new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        return;
                //    }
                //    DataRow dRow = result.NewRow();
                //    dRow["CBO_NAME"] = "-ALL-";
                //    dRow["CBO_CODE"] = "";
                //    result.Rows.InsertAt(dRow, 0);

                //    cbo.ItemsSource = DataTableConverter.Convert(result);
                //    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                //    {
                //        cbo.SelectedValue = selectedEquipmentSegmant;
                //    }
                //    else if (result.Rows.Count > 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //    else if (result.Rows.Count == 0)
                //    {
                //        cbo.SelectedItem = null;
                //    }
                //    cboEquipmentSegment_SelectedItemChanged(cbo, null);
                //});
                #endregion

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = selectedArea;
                drnewrow["PROCID"] = selectedProcess; ;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();
                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipmentSegmant) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedEquipmentSegmant;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                    cboEquipmentSegment_SelectedItemChanged(cbo, null);
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Process(C1ComboBox cbo)
        {
            try
            {
                #region 변경 DA_BAS_SEL_PROCESS_CBO -> DA_BAS_SEL_PROCESS_BY_AREA_CBO
                //DataTable dtRQSTDT = new DataTable();
                //dtRQSTDT.TableName = "RQSTDT";
                //dtRQSTDT.Columns.Add("LANGID", typeof(string));
                //dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                //DataRow drnewrow = dtRQSTDT.NewRow();
                //drnewrow["LANGID"] = LoginInfo.LANGID;
                //drnewrow["EQSGID"] = selectedEquipmentSegmant;
                //dtRQSTDT.Rows.Add(drnewrow);

                //new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                //{
                //    if (Exception != null)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        return;
                //    }
                //    DataRow dRow = result.NewRow();

                //    dRow["CBO_NAME"] = "-ALL-";
                //    dRow["CBO_CODE"] = "";
                //    result.Rows.InsertAt(dRow, 0);

                //    cbo.ItemsSource = DataTableConverter.Convert(result);

                //    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                //    {
                //        cbo.SelectedValue = selectedProcess;
                //    }
                //    else if (result.Rows.Count > 0)
                //    {
                //        cbo.SelectedIndex = 0;
                //    }
                //    else if (result.Rows.Count == 0)
                //    {
                //        cbo.SelectedItem = null;
                //    }
                //});
                #endregion

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = selectedArea; ;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);

                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedProcess) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedProcess;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Equipment(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                drnewrow["PROCID"] = selectedProcess;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedEquipment;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void Set_Combo_Product(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["SHOPID"] = cboShop.SelectedValue.ToString();
                drnewrow["AREAID"] = cboArea.SelectedValue.ToString();
                if(cboEquipmentSegment.SelectedValue.ToString() == "")
                {
                    drnewrow["EQSGID"] = null;
                }
                else
                {
                    drnewrow["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();
                }

                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PRODUCT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();

                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    if ((from DataRow dr in result.Rows where dr["CBO_CODE"].Equals(selectedEquipment) select dr).Count() > 0)
                    {
                        cbo.SelectedValue = selectedEquipment;
                    }
                    else if (result.Rows.Count > 0)
                    {
                        cbo.SelectedIndex = 0;
                    }
                    else if (result.Rows.Count == 0)
                    {
                        cbo.SelectedItem = null;
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        //private void Set_Combo_Type(C1ComboBox cbo) //사용안함 숨김처리
        //{
        //    DataTable dtRQSTDT = new DataTable();
        //    dtRQSTDT.TableName = "RQSTDT";
        //    dtRQSTDT.Columns.Add("LANGID", typeof(string));
        //    dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

        //    DataRow drnewrow = dtRQSTDT.NewRow();
        //    drnewrow["LANGID"] = LoginInfo.LANGID;
        //    drnewrow["CMCDTYPE"] = "PLAN_TYPE";
        //    dtRQSTDT.Rows.Add(drnewrow);

        //    new ClientProxy().ExecuteService("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
        //    {
        //        if (Exception != null)
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
        //            return;
        //        }
        //        DataRow dRow = result.NewRow();
        //        dRow["CBO_NAME"] = "-ALL-";
        //        dRow["CBO_CODE"] = "";
        //        result.Rows.InsertAt(dRow, 0);

        //        cbo.ItemsSource = DataTableConverter.Convert(result);
        //        cbo.SelectedIndex = 0;
        //    }
        //    );
        //}
        private void Set_Combo_WOType(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["CMCDTYPE"] = "DEMAND_TYPE";
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    DataRow dRow = result.NewRow();
                    dRow["CBO_NAME"] = "-ALL-";
                    dRow["CBO_CODE"] = "";
                    result.Rows.InsertAt(dRow, 0);

                    cbo.ItemsSource = DataTableConverter.Convert(result);
                    cbo.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        private void SetGridDate()
        {
            System.DateTime dtNow = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, 1); //new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (int i = 1; i <= dtNow.AddMonths(1).AddDays(-1).Day; i++)
            {
                string dayColumnName = string.Empty;

                if (i < 10)
                {
                    dayColumnName = "PLAN_QTY_0" + i.ToString();
                }
                else
                {
                    dayColumnName = "PLAN_QTY_" + i.ToString();
                }

                List<string> sHeader = new List<string>() { "[*]" + dtNow.Year.ToString() + "." + dtNow.Month.ToString(), "[*]" + dtNow.AddDays(i - 1).ToString("ddd"), "[*]" + i.ToString() };

                //현재 날짜 컬럼 색 변경
                if (DateTime.Now.ToString("yyyyMMdd") == dtNow.Year.ToString() + dtNow.Month.ToString("00") + dtNow.AddDays(i - 1).ToString("dd")) //dtNow.AddMonths(1).AddDays(-i).Day.ToString()) //dgList.Columns[i].Name
                {
                    List<string> sHeader_Today = new List<string>() { "[*]" +  dtNow.Year.ToString() + "." + dtNow.Month.ToString(), "[*]" + dtNow.AddDays(i - 1).ToString("ddd")+" (Today)", "[*]" + i.ToString() };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
            }

            Util.SetGridColumnText(dgList, "NOTE", new List<string>() { "비고", "비고", "비고" }, "", true, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Left, Visibility.Visible);
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent)
        };
        
        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                for (int i = dgList.Columns.Count; i-- > 15;) //컬럼수
                    dgList.Columns.RemoveAt(i);

                SetGridDate();

                if (rdoProcess.IsChecked.Value)
                    dgList.Columns["EQPTNAME"].Visibility = Visibility.Collapsed;
                else if (rdoEquipment.IsChecked.Value)
                    dgList.Columns["EQPTNAME"].Visibility = Visibility.Visible;

                string _ValueToMonth = string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime);
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("LVL", typeof(string));
                IndataTable.Columns.Add("ROLL_TYPE", typeof(string));
                IndataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = _ValueToMonth;
                Indata["SHOPID"] = Util.NVC(cboShop.SelectedValue);
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["DEMAND_TYPE"] = Util.NVC(cboDEMAND_TYPE.SelectedValue);
                Indata["PRJT_NAME"] = Util.NVC(txtProject.Text.Trim());
                Indata["PRODID"] = Util.NVC(txtProduct.Text.Trim());
                Indata["MODLID"] = Util.NVC(txtModel.Text.Trim());

                if (rdoProcess.IsChecked.Value)
                    Indata["LVL"] = "PROC";
                else if (rdoEquipment.IsChecked.Value)
                    Indata["LVL"] = "EQPT";

                if (rdoCRoll.IsChecked.Value)
                    Indata["ROLL_TYPE"] = "CROLL";
                else if (rdoSRoll.IsChecked.Value)
                    Indata["ROLL_TYPE"] = "SROLL";

                Indata["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType, bAllNull: true);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_DAILY_PLAN", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    //if (result.Rows.Count <= 0)
                    //{
                    //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("조회데이터가 없습니다."), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                    //}
                    //dgList.ItemsSource = DataTableConverter.Convert(result);
                    Util.GridSetData(dgList, result, FrameOperation, true);

                    for (int i = 0; i < dgList.GetRowCount(); i++)
                    {
                        Scrolling_Into_Today_Col();
                    }
                });
            }
            catch(Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }

        private void Sorting_Closed(object sender, EventArgs e)
        {
            COM001_001_PRIORITY runStartWindow = sender as COM001_001_PRIORITY;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                
            }
        }
        //private void xlsLoad_Closed(object sender, EventArgs e)
        //{
        //    COM001_001_LOAD runStartWindow = sender as COM001_001_LOAD;
        //    if (runStartWindow.DialogResult == MessageBoxResult.OK)
        //    {

        //    }
        //}

        private void Scrolling_Into_Today_Col()
        {
            string today_Col_Name = "PLAN_QTY_" + DateTime.Now.ToString("dd");
            if (dgList.Columns[today_Col_Name] != null)
            {
                if (DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime))
                {
                    dgList.ScrollIntoView(2, dgList.Columns[today_Col_Name].Index);
                }
            }
        }
         
        //private void setGridFormat(C1.WPF.DataGrid.DataGridRow row)
        //{
        //    if (dgList.GetRowCount() < 4 || row.Index < 3)
        //        return;

        //    //C1.WPF.DataGrid.DataGridTextColumn fColumn = (C1.WPF.DataGrid.DataGridTextColumn)dgList.Columns["PLAN_QTY"];
        //    string pointValue = dgList[row.Index, dgList.Columns["PROCNAME"].Index].ToString();
        //    string demicalPointNum = string.Empty;
        //    if (demicalPointNum.Equals("Slitting"))
        //        demicalPointNum = "2";
        //    else
        //        demicalPointNum = "0";

        //    string sFormat = string.Empty;
        //    if (demicalPointNum.Equals("0"))
        //    {
        //        sFormat = "#,##0";
        //    }
        //    else if (demicalPointNum.Equals("1"))
        //    {
        //        sFormat = "#,##0.0";
        //    }
        //    else if (demicalPointNum.Equals("2"))
        //    {
        //        sFormat = "#,##0.00";
        //    }
        //    else if (demicalPointNum.Equals("3"))
        //    {
        //        sFormat = "#,##0.000";
        //    }
        //    //(dgList[row.Index, dgList.Columns["PLAN_QTY"].Index].Presenter.Content as C1NumericBox).Format = sFormat;
        //    C1.WPF.DataGrid.DataGridNumericColumn sNumericCol = (C1.WPF.DataGrid.DataGridNumericColumn)dgList.Columns["PLAN_QTY"];
        //    sNumericCol.Format = sFormat;
        //}
        #endregion
    }
}
