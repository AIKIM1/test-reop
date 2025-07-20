/*************************************************************************************
 Created Date : 2018.01.18
      Creator : 
   Decription : 메시지 그룹별 에러 건수
--------------------------------------------------------------------------------------
 [Change History]
  
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

namespace LGC.GMES.MES.MON001
{
    public partial class MON001_017 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        private string selectedProcess = string.Empty;

        Util _util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public MON001_017()
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
            selectedProcess = LoginInfo.CFG_PROC_ID;
            dtpDateMonth.SelectedDateTime = System.DateTime.Now;
            InitCombo();
            InitGrid();
 
        }

        private void InitGrid()
        {
            Util.gridClear(dgList);
        }

        private void InitCombo()
        {
            Set_Combo_Shop(cboShop);
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

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.Rows.Count <= 3)
                return;

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
                    //Set_Combo_Equipment(cboEquipment);
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
                string _Day = string.Empty;
                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //오늘날짜 셀구분
                    if (e.Cell.Column.Name.Contains("ERR_CNT_"))
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

        private void Set_Combo_Process(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                //drnewrow["EQSGID"] = "";
                drnewrow["EQSGID"] = null; // 2024.10.06 김영국 - DA 호출 시 변수값을 ""으로 넘기면 에러가 발생하여 null 처리함. 
                drnewrow["AREAID"] = selectedArea;
                dtRQSTDT.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESS_CBO_WITH_AREA", "RQSTDT", "RSLTDT", dtRQSTDT, (result, Exception) =>
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

        private void SetGridDate()
        {
            System.DateTime dtNow = new DateTime(dtpDateMonth.SelectedDateTime.Year, dtpDateMonth.SelectedDateTime.Month, 1); //new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (int i = 1; i <= dtNow.AddMonths(1).AddDays(-1).Day; i++)
            {
                string dayColumnName = string.Empty;

                if (i < 10)
                {
                    dayColumnName = "ERR_CNT_0" + i.ToString();
                }
                else
                {
                    dayColumnName = "ERR_CNT_" + i.ToString();
                }

                List<string> sHeader = new List<string>() { dtNow.Year.ToString() + "." + dtNow.Month.ToString(), dtNow.AddDays(i - 1).ToString("ddd"), i.ToString() };

                //현재 날짜 컬럼 색 변경
                if (DateTime.Now.ToString("yyyyMMdd") == dtNow.Year.ToString() + dtNow.Month.ToString("00") + dtNow.AddDays(i - 1).ToString("dd")) //dtNow.AddMonths(1).AddDays(-i).Day.ToString()) //dgList.Columns[i].Name
                {
                    List<string> sHeader_Today = new List<string>() { dtNow.Year.ToString() + "." + dtNow.Month.ToString(), dtNow.AddDays(i - 1).ToString("ddd") + " (Today)", i.ToString() };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }

            }
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

                for (int i = dgList.Columns.Count; i-- > 3;) //컬럼수
                    dgList.Columns.RemoveAt(i);

                SetGridDate();

                string _ValueToMonth = string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime);
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("MONTH", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["MONTH"] = _ValueToMonth;
                Indata["AREAID"] = Util.NVC(cboArea.SelectedValue);
                Indata["PROCID"] = Util.NVC(cboProcess.SelectedValue);

                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MSGGR_ERR_CNT", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                        return;

                    Util.GridSetData(dgList, result, FrameOperation, true);
                    string[] sColumnName = new string[] { "AREANAME", "PROCNAME" };
                    _util.SetDataGridMergeExtensionCol(dgList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); 

                    //for (int i = 0; i < dgList.GetRowCount(); i++)
                    //{
                    //    Scrolling_Into_Today_Col();
                    //}
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
            string today_Col_Name = "ERR_CNT_" + DateTime.Now.ToString("dd");
            if (dgList.Columns[today_Col_Name] != null)
            {
                if (DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpDateMonth.SelectedDateTime))
                {
                    dgList.ScrollIntoView(2, dgList.Columns[today_Col_Name].Index);
                }
            }
        }
         

        #endregion
    }
}
