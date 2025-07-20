using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_270.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_270 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool _ViewText = false;
        private DataTable _DTORG = new DataTable();

        //private string selectedShop = string.Empty;
        //private string selectedArea = string.Empty;
        //private string selectedElecType = string.Empty;
        //private string selectedProcess = string.Empty;
        //private string selectedEquipment = string.Empty;
        //private string selectedEquipmentSegmant = string.Empty;

        Util _Util = new Util();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_270()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            Initialize();
            this.dtpDate.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
        }

        private void Initialize()
        {
            //selectedShop = LoginInfo.CFG_SHOP_ID;
            //selectedArea = LoginInfo.CFG_AREA_ID;
            //selectedProcess = LoginInfo.CFG_PROC_ID;
            dtpDate.SelectedDateTime = System.DateTime.Now;
            InitCombo();
            ApplyPermissions();
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            C1ComboBox[] cboAreaChild = { cboProcess };
            SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            C1ComboBox[] cboProcessParent = { cboArea };
            SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent);

            C1ComboBox[] cboLineParent = { cboArea, cboProcess };
            SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent);
            

            String[] sFilter3 = { "ELEC_TYPE" };
            //C1ComboBox[] cboElecChild = { cboEquipment };
            SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter3);
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        //private void cboArea_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (cboArea.SelectedIndex > -1)
        //        {
        //            selectedArea = Convert.ToString(cboArea.SelectedValue);
        //            SetComboProcess(cboProcess);                    
        //        }
        //        else
        //        {
        //            selectedArea = string.Empty;
        //        }
        //    }));
        //}

        //private void cboProcess_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (cboProcess.SelectedIndex > -1)
        //        {
        //            selectedProcess = Convert.ToString(cboProcess.SelectedValue);
        //            SetComboEquipmentSegmant(cboEquipmentSegment);
        //            //SetComboEquipment(cboEquipment);
        //        }
        //        else
        //        {
        //            selectedProcess = string.Empty;
        //        }
        //    }));
        //}

        //private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (cboEquipmentSegment.SelectedIndex > -1)
        //        {
        //            selectedEquipmentSegmant = Convert.ToString(cboEquipmentSegment.SelectedValue);
        //            SetComboEquipment(cboEquipment);
        //        }
        //        else
        //        {
        //            selectedEquipmentSegmant = string.Empty;
        //        }
        //    }));
        //}

        //private void cboElecType_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (cboElecType.SelectedIndex > -1)
        //        {
        //            selectedElecType = Convert.ToString(cboElecType.SelectedValue);
        //            SetComboEquipment(cboEquipment);
        //        }
        //        else
        //        {
        //            selectedElecType = string.Empty;
        //        }
        //    }));
        //}

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Util.NVC(cboEquipment.SelectedItemsToString) == "")
            {
                Util.MessageValidation("SFU1673");      //설비를 선택하세요.
                return;
            }

            SearchData();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSave())
                return;
            
            // 저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                try
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveOffTime();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            });           
        }

        //private void txtFromHour_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (Util.CheckDecimal(txtFromHour.Text, 0))
        //        {
        //            Util.MessageValidation("SFU3435");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        //private void txtToHour_KeyUp(object sender, KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (Util.CheckDecimal(txtToHour.Text, 0))
        //        {
        //            Util.MessageValidation("SFU3435");
        //            return;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void btnOn_Click(object sender, RoutedEventArgs e)
        {
            SetRange(true);
        }

        private void btnOff_Click(object sender, RoutedEventArgs e)
        {
            SetRange(false);
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(e.Cell.Column.Name).StartsWith("H"))
                    {
                        int iHr = 0;
                        if (int.TryParse(Util.NVC(e.Cell.Column.Name).Replace("H", ""), out iHr))
                        {
                            //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("N"))
                            //{
                            //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#56AA1C"));
                            //    //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                            //}
                            //else 
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("Y"))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#955090"));

                                if (_ViewText)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                else
                                    e.Cell.Presenter.Foreground = null;
                            }
                            else //if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name)).Equals("N"))
                            {
                                e.Cell.Presenter.Background = null;

                                if (_ViewText)
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                else
                                    e.Cell.Presenter.Foreground = null;
                            }
                        }
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void TextBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                        (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
                        (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt &&
                        (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                _ViewText = !_ViewText;

                dgList.Refresh();
            }
        }

        private void cboProcess_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void cboElecType_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    SetCboEquipment();
                }));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Cb_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            C1ComboBox cb = sender as C1ComboBox;
            Hashtable hashCbo = cb.Tag as Hashtable;

            C1ComboBox[] cbChildArray = hashCbo["child_cbo"] as C1ComboBox[];

            if (cb.SelectedValue != null)
            {
                foreach (C1ComboBox cbChild in cbChildArray)
                {
                    SetCombo(cbChild);
                }
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            Util.gridClear(dgList);
        }

        #endregion

        #region Mehod

        public void SetCombo(C1ComboBox cb, CommonCombo.ComboStatus cs, C1ComboBox[] cbChild = null, C1ComboBox[] cbParent = null, String[] sFilter = null, String sCase = null)
        {
            Hashtable hashTag = new Hashtable();
            if (sCase == null)
            {
                hashTag.Add("combo_case", cb.Name);
            }
            else
            {
                hashTag.Add("combo_case", sCase);
            }
            hashTag.Add("all_status", cs);
            hashTag.Add("child_cbo", cbChild);
            hashTag.Add("parent_cbo", cbParent);
            hashTag.Add("filter", sFilter);
            cb.Tag = hashTag;

            SetCombo(cb);

            if (hashTag.Contains("child_cbo") && hashTag["child_cbo"] != null)
            {
                cb.SelectedItemChanged -= Cb_SelectedItemChanged;
                cb.SelectedItemChanged += Cb_SelectedItemChanged;
            }
        }

        private void SetCombo(C1ComboBox cb)
        {
            try
            {
                Hashtable hashTag = cb.Tag as Hashtable;
                CommonCombo.ComboStatus cs = (CommonCombo.ComboStatus)Enum.Parse(typeof(CommonCombo.ComboStatus), hashTag["all_status"].ToString());
                String[] sFilter = new String[10];
                

                if (hashTag.Contains("parent_cbo") && hashTag["parent_cbo"] != null)
                {
                    C1ComboBox[] cbParentArray = hashTag["parent_cbo"] as C1ComboBox[];
                    int i = 0;
                    for (i = 0; i < cbParentArray.Length; i++)
                    {
                        if (cbParentArray[i].SelectedValue != null)
                        {
                            sFilter[i] = cbParentArray[i].SelectedValue.ToString();
                        }
                        else
                        {
                            sFilter[i] = "";
                        }
                    }

                    if (hashTag.Contains("filter") && hashTag["filter"] != null)
                    {
                        String[] sFilter1 = hashTag["filter"] as String[];
                        foreach (string s in sFilter1) {
                            sFilter[i] = s;
                            i++;
                        }
                        
                    }
                }                
                else if (hashTag.Contains("filter") && hashTag["filter"] != null)
                {
                    sFilter = hashTag["filter"] as String[];
                }
                else
                {
                    sFilter[0] = "";
                }

                switch (hashTag["combo_case"].ToString())
                {
                    case "cboArea":
                        SetComboArea(cb, cs, sFilter);
                        break;
                    case "cboProcess":
                        SetComboProcess(cb, cs, sFilter);
                        break;
                    case "cboEquipmentSegment":
                        SetComboEquipmentSegmant(cb, cs, sFilter);
                        break;
                    case "cboElecType":
                        SetComboEltr(cb, cs, sFilter);
                        break;
                    case "cboEquipment":
                        SetComboEquipment(cb, cs, sFilter);
                        break;
                    default:
                        SetDefaultCbo(cb, cs);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        

        private void SetComboArea(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
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
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["USERID"] = LoginInfo.USERID;
                drnewrow["USE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AUTH_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboProcess(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0]; ;
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataTable result = result2.Clone();
                var drTmp = result2?.Select("CBO_CODE IN ('" + Process.ROLL_PRESSING + "','" + Process.SLITTING + "','" + Process.NOTCHING + "')");
                if (drTmp.Length > 0)
                    result = drTmp.CopyToDataTable();

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipmentSegmant(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("PROD_GROUP", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = sFilter[0];
                drnewrow["PROCID"] = sFilter[1];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO_CR", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEltr(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sFilter[0];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetComboEquipment(C1ComboBox cbo, CommonCombo.ComboStatus cs, String[] sFilter)
        {
            try
            {
                Util.gridClear(dgList);

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string)); 

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = sFilter[1];
                drnewrow["PROCID"] = sFilter[0]; ;
                drnewrow["ELTR_TYPE_CODE"] = Util.NVC(sFilter[2]).Equals("") ? null : sFilter[2];
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(result, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SetCboEquipment()
        {
            try
            {

                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = Util.GetCondition(cboArea);
                drnewrow["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment);
                drnewrow["PROCID"] = Util.GetCondition(cboProcess);
                drnewrow["ELTR_TYPE_CODE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_RTS_SEL_EQUIPMENT_BY_ELECTRODETYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                cboEquipment.ItemsSource = DataTableConverter.Convert(result);
                cboEquipment.CheckAll();
            }
            catch (Exception ex)
            {

            }
        }

        private void SetDefaultCbo(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable dtResult = new DataTable();

                dtResult.Columns.Add("CBO_CODE", typeof(string));
                dtResult.Columns.Add("CBO_NAME", typeof(string));

                DataRow newRow = dtResult.NewRow();

                newRow = dtResult.NewRow();
                newRow.ItemArray = new object[] { "NA", "구현안됨" };
                dtResult.Rows.Add(newRow);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SearchData()
        {
            try
            {
                Util.gridClear(dgList);

                loadingIndicator.Visibility = Visibility.Visible;

                string _Date = string.Format("{0:yyyyMMdd}", dtpDate.SelectedDateTime);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("DATE_YMD", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = IndataTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["DATE_YMD"] = _Date;
                dtRow["AREAID"] = Util.GetCondition(cboArea);
                dtRow["PROCID"] = Util.GetCondition(cboProcess);
                dtRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment).Equals("") ? null : Util.GetCondition(cboEquipmentSegment);
                dtRow["ELTR_TYPE_CODE"] = Util.GetCondition(cboElecType).Equals("") ? null : Util.GetCondition(cboElecType);
                dtRow["EQPTID"] = cboEquipment.SelectedItemsToString; //Util.GetCondition(cboEquipment).Equals("") ? null : Util.GetCondition(cboEquipment);

                IndataTable.Rows.Add(dtRow);

                new ClientProxy().ExecuteService("DA_RTS_SEL_MES_OFFTIME_LIST", "RQSTDT", "RSLTDT", IndataTable, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgList, result, FrameOperation, false);
                        _DTORG = result.Copy();
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void SetRange(bool bOn)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgList)) return;
                
                DataTable dtSel = GetSelectedRange(dgList, bOn);

                if (!ValidationSelectedRange(dtSel))
                    return;

                DataTable dtTrgt = DataTableConverter.Convert(dgList.ItemsSource);

                dtTrgt.PrimaryKey = new DataColumn[] { dtTrgt.Columns["EQPTID"], dtTrgt.Columns["OFF_DATE_YMD"] };
                dtSel.PrimaryKey = new DataColumn[] { dtSel.Columns["EQPTID"], dtSel.Columns["OFF_DATE_YMD"] };
                dtTrgt.Merge(dtSel);

                Util.GridSetData(dgList, dtTrgt, FrameOperation, false);
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

        private DataTable GetSelectedRange(C1DataGrid view, bool bOn)
        {
            try
            {
                string sVal = bOn ? "N" : "Y";

                DataTable dtTrgt = new DataTable();
                DataTable dtSrc = null;

                var rowSel = view.Selection.SelectedRows.ToList();
                var colSel = view.Selection.SelectedColumns.ToList();

                if (view.ItemsSource is DataView)
                    dtSrc = ((DataView)view.ItemsSource).Table;

                if (dtSrc != null)
                {
                    for (int col = 0; col < colSel.Count; col++)
                    {
                        dtTrgt.Columns.Add(new DataColumn(colSel[col].Name));
                    }
                                        
                    dtTrgt.Columns.Add("EQPTID", typeof(string));
                    dtTrgt.Columns.Add("OFF_DATE_YMD", typeof(string));
                    dtTrgt.Columns.Add("CHK", typeof(string));

                    for (int row = 0; row < rowSel.Count; row++)
                    {
                        ArrayList values = new ArrayList();
                        for (int col = 0; col < colSel.Count; col++)
                        {
                            //values.Add(colSel[col].GetCellValue(rowSel[row]));
                            values.Add(sVal);
                        }

                        values.Add(Util.NVC(DataTableConverter.GetValue(view.Rows[rowSel[row].Index].DataItem, "EQPTID")));
                        values.Add(Util.NVC(DataTableConverter.GetValue(view.Rows[rowSel[row].Index].DataItem, "OFF_DATE_YMD")));
                        values.Add(true);

                        dtTrgt.Rows.Add(values.ToArray());
                    }

                    dtTrgt.AcceptChanges();
                }

                return dtTrgt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private bool ValidationSelectedRange(DataTable inDataTable)
        {
            try
            {
                bool bRet = false;
                bool bChkHour = false;

                if (!CommonVerify.HasTableRow(inDataTable)) return bRet;

                DateTime _SysDttm = GetSystemTime();

                // 과거 일자 여부 체크.
                var query = (from dr in inDataTable.AsEnumerable()
                            orderby dr.Field<string>("OFF_DATE_YMD") ascending
                            select new
                            {
                                OFF_DATE_YMD = dr.Field<string>("OFF_DATE_YMD")
                            }).Distinct();

                foreach (var x in query)
                {
                    DateTime dttmTmp;

                    if (DateTime.TryParseExact(Util.NVC(x.OFF_DATE_YMD)
                        , "yyyyMMdd"
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.None
                        , out dttmTmp))
                    {
                        if (dttmTmp.Date < _SysDttm.Date)
                        {
                            // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 일자 확인)
                            Util.MessageValidation("SFU3744");
                            return bRet;
                        }

                        if (dttmTmp.Date == _SysDttm.Date)
                            bChkHour = true;
                    }
                    else
                    {
                        if (dtpDate.SelectedDateTime < _SysDttm.Date)
                        {
                            // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 일자 확인)
                            Util.MessageValidation("SFU3744");
                            return bRet;
                        }
                    }
                }

                for (int col = 0; col < inDataTable.Columns.Count; col++)
                {
                    if (inDataTable.Columns[col].ColumnName.Equals("EQPTID") || inDataTable.Columns[col].ColumnName.Equals("OFF_DATE_YMD") || inDataTable.Columns[col].ColumnName.Equals("CHK")) continue;
                    if (!inDataTable.Columns[col].ColumnName.StartsWith("H"))
                    {
                        // 선택오류 : 시간외 항목을 선택했습니다. (시간 항목만 선택)
                        Util.MessageValidation("SFU3742");
                        return bRet;
                    }

                    int iHr = 0;
                    if (!int.TryParse(inDataTable.Columns[col].ColumnName.Replace("H", ""), out iHr))
                    {
                        // 선택오류 : 시간외 항목을 선택했습니다. (시간 항목만 선택)
                        Util.MessageValidation("SFU3742");
                        return bRet;
                    }
                    
                    if (bChkHour && iHr < _SysDttm.Hour)
                    {
                        // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 시간 확인)
                        Util.MessageValidation("SFU3743");
                        return bRet;
                    }
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CanSave()
        {
            try
            {
                bool bRet = false;
                bool bChkHour = false;

                if (_Util.GetDataGridCheckCnt(dgList, "CHK") < 1)
                {
                    Util.MessageValidation("SFU1651");
                    return bRet;
                }
                
                DateTime _SysDttm = GetSystemTime();

                if (dtpDate.SelectedDateTime < _SysDttm.Date)
                {
                    // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 일자 확인)
                    Util.MessageValidation("SFU3744");
                    return bRet;
                }

                for (int row = 0; row < dgList.GetRowCount(); row++)
                {
                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", row)) continue;

                    #region [저장 시간 기준 이전 일자 변경 여부 체크]
                    DateTime dttmTmp;

                    if (DateTime.TryParseExact(Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "OFF_DATE_YMD"))
                        , "yyyyMMdd"
                        , System.Globalization.CultureInfo.InvariantCulture
                        , System.Globalization.DateTimeStyles.None
                        , out dttmTmp))
                    {
                        if (dttmTmp.Date < _SysDttm.Date)
                        {
                            // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 일자 확인)
                            Util.MessageValidation("SFU3744");
                            return bRet;
                        }

                        if (dttmTmp.Date == _SysDttm.Date)
                            bChkHour = true;
                    }
                    else
                    {
                        if (dtpDate.SelectedDateTime < _SysDttm.Date)
                        {
                            // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 일자 확인)
                            Util.MessageValidation("SFU3744");
                            return bRet;
                        }
                    }
                    #endregion

                    if (bChkHour)
                    {
                        #region [저장 시간 기준 이전 시간 변경 여부 체크]                     
                        string sCond = "";
                        string sColName = string.Empty;
                        for (int idx = 0; idx < _SysDttm.Hour; idx++)
                        {
                            sColName = "H" + idx.ToString("00");

                            sCond = sCond.Equals("") ? sColName + "='" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, sColName)) + "'"
                                                     : sCond + " AND " + sColName + "='" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, sColName)) + "'";
                        }

                        DataRow[] drTmp = _DTORG.Select("OFF_DATE_YMD = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "OFF_DATE_YMD"))
                                                      + "' AND EQPTID = '" + Util.NVC(DataTableConverter.GetValue(dgList.Rows[row].DataItem, "EQPTID"))
                                                      + "' AND " + sCond);

                        if (drTmp?.Length < 1)
                        {
                            // 선택오류 : 현재 시간 이전은 변경 불가 합니다. (선택 시간 확인)
                            Util.MessageValidation("SFU3743");
                            return bRet;
                        }
                        #endregion
                    }
                }

                bRet = true;
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void SaveOffTime()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                string _Date = string.Format("{0:yyyyMMdd}", dtpDate.SelectedDateTime);

                DataSet indataSet = new DataSet();
                DataTable inHDRTable = indataSet.Tables.Add("IN_HDR");
                inHDRTable.Columns.Add("SRCTYPE", typeof(string));
                inHDRTable.Columns.Add("IFMODE", typeof(string));                
                inHDRTable.Columns.Add("AREAID", typeof(string));
                inHDRTable.Columns.Add("PROCID", typeof(string));
                inHDRTable.Columns.Add("USERID", typeof(string));

                DataTable inDataTable = indataSet.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("OFF_DATE_YMD", typeof(string));
                inDataTable.Columns.Add("H00_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H01_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H02_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H03_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H04_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H05_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H06_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H07_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H08_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H09_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H10_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H11_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H12_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H13_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H14_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H15_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H16_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H17_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H18_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H19_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H20_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H21_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H22_OFF_YN", typeof(string));
                inDataTable.Columns.Add("H23_OFF_YN", typeof(string));
                
                DataRow newRow = inHDRTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["AREAID"] = Util.GetCondition(cboArea);
                newRow["PROCID"] = Util.GetCondition(cboProcess);
                newRow["USERID"] = LoginInfo.USERID;

                inHDRTable.Rows.Add(newRow);
                
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    newRow = null;

                    if (!_Util.GetDataGridCheckValue(dgList, "CHK", i)) continue;

                    newRow = inDataTable.NewRow();
                    newRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "EQPTID"));
                    newRow["OFF_DATE_YMD"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OFF_DATE_YMD")).Equals("") ? _Date : Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OFF_DATE_YMD"));

                    newRow["H00_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H00")).Equals("Y") ? "N" : "Y";
                    newRow["H01_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H01")).Equals("Y") ? "N" : "Y";
                    newRow["H02_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H02")).Equals("Y") ? "N" : "Y";
                    newRow["H03_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H03")).Equals("Y") ? "N" : "Y";
                    newRow["H04_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H04")).Equals("Y") ? "N" : "Y";
                    newRow["H05_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H05")).Equals("Y") ? "N" : "Y";
                    newRow["H06_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H06")).Equals("Y") ? "N" : "Y";
                    newRow["H07_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H07")).Equals("Y") ? "N" : "Y";
                    newRow["H08_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H08")).Equals("Y") ? "N" : "Y";
                    newRow["H09_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H09")).Equals("Y") ? "N" : "Y";
                    newRow["H10_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H10")).Equals("Y") ? "N" : "Y";
                    newRow["H11_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H11")).Equals("Y") ? "N" : "Y";
                    newRow["H12_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H12")).Equals("Y") ? "N" : "Y";
                    newRow["H13_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H13")).Equals("Y") ? "N" : "Y";
                    newRow["H14_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H14")).Equals("Y") ? "N" : "Y";
                    newRow["H15_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H15")).Equals("Y") ? "N" : "Y";
                    newRow["H16_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H16")).Equals("Y") ? "N" : "Y";
                    newRow["H17_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H17")).Equals("Y") ? "N" : "Y";
                    newRow["H18_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H18")).Equals("Y") ? "N" : "Y";
                    newRow["H19_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H19")).Equals("Y") ? "N" : "Y";
                    newRow["H20_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H20")).Equals("Y") ? "N" : "Y";
                    newRow["H21_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H21")).Equals("Y") ? "N" : "Y";
                    newRow["H22_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H22")).Equals("Y") ? "N" : "Y";
                    newRow["H23_OFF_YN"] = !Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "H23")).Equals("Y") ? "N" : "Y";

                    inDataTable.Rows.Add(newRow);
                }

                if (inDataTable.Rows.Count < 1)
                {
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService_Multi("BR_RTS_REG_MES_OFFTIME", "IN_HDR,IN_DATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다.
                        Util.MessageInfo("SFU1275");

                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        this.loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion
        
    }
}
