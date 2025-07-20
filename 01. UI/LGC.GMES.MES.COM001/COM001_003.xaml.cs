/*************************************************************************************
 Created Date : 2017.01.18
      Creator : 
   Decription : 믹서생산계획 조회
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.18  DEVELOPER : Initial Created.
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
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_003 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedElecType = string.Empty;
        private string selectedEquipment = string.Empty;
        

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_003()
        {
            InitializeComponent();
            Initialize();

            this.Loaded += UserControl_Loaded;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            int deltaMDay = DayOfWeek.Monday - System.DateTime.Now.DayOfWeek;
            DateTime thisMon = System.DateTime.Now.AddDays(deltaMDay);
            dtpFrom.SelectedDateTime = thisMon;
            int deltaSDay = DayOfWeek.Saturday - System.DateTime.Now.DayOfWeek;
            DateTime nextSat = System.DateTime.Now.AddDays(deltaSDay + 7);
            dtpTo.SelectedDateTime = nextSat;

            ApplyPermissions();

            this.Loaded -= UserControl_Loaded;
        }

        private void Initialize()
        {
            selectedShop = LoginInfo.CFG_SHOP_ID;
            selectedArea = LoginInfo.CFG_AREA_ID;
            selectedEquipmentSegmant = LoginInfo.CFG_EQSG_ID;
            selectedProcess = LoginInfo.CFG_PROC_ID;
            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            Set_Combo_Area(cboArea);
            Set_Combo_WOType(cboDEMAND_TYPE);
        }
        #endregion

        #region Event
        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                SetGridDate();
            }));
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void dtpFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.Alert("SFU2042", new object[] { "31" });   //기간은 {0}일 이내 입니다.
                    dtpTo.SelectedDateTime = dtpFrom.SelectedDateTime.AddDays(30);
                    //SetGridDate();
                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    SetGridDate();
                    return;
                }
            }
            //SetGridDate();
        }

        private void dtpTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.Alert("SFU2042", new object[] { "31" });  //기간은 {0}일 이내 입니다.
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime.AddDays(-30);
                    SetGridDate();
                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    SetGridDate();
                    return;
                }
            }
            //SetGridDate();
        }

        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboArea.SelectedIndex > -1)
                {
                    selectedArea = Convert.ToString(cboArea.SelectedValue);
                    Set_Combo_EquipmentSegmant(cboEquipmentSegment);
                    Set_Combo_Process(cboProcess);
                    CommonCombo _combo = new CommonCombo();
                    String[] sFilter1 = { "", "ELEC_TYPE" };
                    _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");
                }
                else
                {
                    selectedArea = string.Empty;
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
                    Set_Combo_Process(cboProcess);
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
                    Set_Combo_Equipment(cboEquipment);
                }
                else
                {
                    selectedProcess = string.Empty;
                }
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                string _Year = string.Empty;
                string _Month = string.Empty;
                string _Day = string.Empty;
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    //오늘날짜 셀구분
                    string[] splitColHeader = e.Cell.Column.ActualGroupHeader.ToString().Split(',');
                    if (!splitColHeader[0].Trim().Equals(splitColHeader[1].Trim()))
                    {
                        string[] splitYearMonth = splitColHeader[0].Split('.');
                        _Year = splitYearMonth[0].Trim();
                        _Month = splitYearMonth[1].Trim();
                        _Day = splitColHeader[2].Trim();
                    }
                    else
                    {
                        _Day = e.Cell.Column.Name.ToString();
                    }

                    if (DateTime.Now.ToString("yyyy") == _Year && DateTime.Now.Month.ToString() == _Month && DateTime.Now.ToString("dd") == _Day)
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
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["AREAID"] = selectedArea;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_MIXING_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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
                dtRQSTDT.Columns.Add("ELTR_TYPE_CODE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = selectedEquipmentSegmant;
                drnewrow["PROCID"] = selectedProcess;
                if(!selectedElecType.Equals(""))
                    drnewrow["ELTR_TYPE_CODE"] = selectedElecType;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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
            System.DateTime dtFrom = dtpFrom.SelectedDateTime;
            System.DateTime dtTo = dtpTo.SelectedDateTime;

            int i = 0;
            for (i = 1; i <= dtTo.Subtract(dtFrom).Days + 1; i++)
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
                List<string> sHeader = new List<string>() { dtFrom.AddDays(i-1).Year.ToString() + "." + dtFrom.AddDays(i-1).Month.ToString(), dtFrom.AddDays(i-1).ToString("ddd"), dtFrom.AddDays(i-1).Day.ToString() };


                if (DateTime.Now.ToString("yyyyMMdd") == dtFrom.AddDays(i-1).Year.ToString() + dtFrom.AddDays(i-1).Month.ToString("00") + dtFrom.AddDays(i-1).ToString("dd"))
                {
                    List<string> sHeader_Today = new List<string>() { dtFrom.AddDays(i-1).Year.ToString() + "." + dtFrom.AddDays(i-1).Month.ToString(), dtFrom.AddDays(i-1).ToString("ddd") + " (Today)", dtFrom.AddDays(i-1).Day.ToString() };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible, "#,##0.00");
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible, "#,##0.00");
                }
            }

            Util.SetGridColumnText(dgList, "PLAN_QTY", new List<string>() { "SUM", "SUM", "SUM" }, (i + 1).ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(80), HorizontalAlignment.Right, Visibility.Visible, "#,##0.00");
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

                for (int col = dgList.Columns.Count; col-- > 22;) //고정컬럼수
                    dgList.Columns.RemoveAt(col);

                SetGridDate();

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PLANSDATE", typeof(string));
                IndataTable.Columns.Add("PLANEDATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PLANSDATE"] = _ValueFrom;
                Indata["PLANEDATE"] = _ValueTo;
                Indata["SHOPID"] = selectedShop;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                Indata["DEMAND_TYPE"] = Util.NVC(cboDEMAND_TYPE.SelectedValue);
                Indata["PRJT_NAME"] = Util.NVC(txtProject.Text.Trim());
                Indata["PRODID"] = Util.NVC(txtProduct.Text.Trim());
                Indata["MODLID"] = Util.NVC(txtModel.Text.Trim());
                Indata["PRDT_CLSS_CODE"] = Util.NVC(cboElecType.SelectedValue);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_DAILY_PLAN_MIXER", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgList, result, FrameOperation, false);

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

        private void Scrolling_Into_Today_Col()
        {
            string _Year = string.Empty;
            string _Month = string.Empty;
            string _Day = string.Empty;

            for (int i = 0; i < dgList.Columns.Count; i++)
            {
                //오늘날짜 셀구분
                string[] splitColHeader = dgList.Columns[i].ActualGroupHeader.ToString().Split(',');
                if (!splitColHeader[0].Trim().Equals(splitColHeader[1].Trim()))
                {
                    string[] splitYearMonth = splitColHeader[0].Split('.');
                    _Year = splitYearMonth[0].Trim();
                    _Month = splitYearMonth[1].Trim();
                    _Day = splitColHeader[2].Trim();
                }

                if (DateTime.Now.ToString("yyyy") == _Year && DateTime.Now.Month.ToString() == _Month && DateTime.Now.ToString("dd") == _Day)
                {
                    dgList.ScrollIntoView(2, dgList.Columns[i].Index);
                }
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnLoad);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        private void cboElecType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedElecType = Convert.ToString(cboElecType.SelectedValue);
            Set_Combo_Equipment(cboEquipment);
            //Set_Combo_Process(cboProcess);
        }
    }
}
