/*************************************************************************************
 Created Date : 2020.04.21
      Creator : 
   Decription : 슬러리 재고 조회
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.30  나순민 사원 : [E20240428-001997] Slurry 재고 조회 슬러리 Validation 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media;


namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_037 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo _combo = new CommonCombo();

        DataSet inDataSet = null;

        bool Inbool = false;

        Util _Util = new Util();


        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_037()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            SetEvent();
            GridColumnVisible();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void InitCombo()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            //설비
            string[] Filter = new string[] { LoginInfo.CFG_EQSG_ID, Process.MIXING, null };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sFilter: Filter);

            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_LOTTYPE_CBO", new string[] { }, new string[] { }, CommonCombo.ComboStatus.NONE, dgResultSum.Columns["REG_LOTTYPE_NAME"], "CBO_CODE", "CBO_NAME");
            CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENT_BY_EQPTID_CBO", new string[] { "EQSGID", "PROCID", "LANGID" }, new string[] { LoginInfo.CFG_EQSG_ID, Process.MIXING, LoginInfo.LANGID }, CommonCombo.ComboStatus.NONE, dgResultSum.Columns["EQPTID"], "CBO_CODE", "CBO_NAME");
        }
        #endregion

        #region Event
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("tSlurry"))
                SearchData();
            else if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("tHistory"))
                SearchDataHistory();
            else
                SearchDataSummary();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            Inbool = true;
            DataGridRowAdd(dgResultSum);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            //if (dgSampling.ItemsSource == null || dgSampling.Rows.Count < 0)
            //    return;

            //DataTable dt = ((DataView)dgSampling.ItemsSource).Table;

            //for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            //    if (Convert.ToBoolean(dt.Rows[i]["CHK"]) && string.Equals(dt.Rows[i]["DELETEYN"], "Y"))
            //        dt.Rows[i].Delete();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResultSum.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU8276");  //슬러리 재고가 없습니다.
                    return;
                }

                int selChk = _Util.GetDataGridCheckFirstRowIndex(dgResultSum, "CHK");

                if (selChk < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        //비즈 내부에서 UPD / INS 여부 확인하여 저장처리 -BR_MAT_REG_SLURRY_STCK_SUM
                        SlurryStchkSum_Insert();
                        //if (sresult == MessageBoxResult.OK)
                        //{
                        //    if (Inbool == true) //추가시
                        //    {
                        //        SlurryStchkSum_Insert();
                        //    }
                        //    else
                        //    {
                        //        SlurryStchkSum_Update();
                        //    }
                        //}
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("LANGID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                Indata["LANGID"] = LoginInfo.LANGID;

                IndataTable.Rows.Add(Indata);

                //가져오기를 실행합니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8292"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService("BR_MAT_REG_SLURRY_STCK_SUM_IMPORT", "INDATA", null, IndataTable, (result, ex) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            if (ex != null)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                return;
                            }

                            //Util.AlertInfo("SFU1270");  //저장되었습니다.
                            SearchDataSummary();
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
            }
        }

        private void dgResultSum_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                DataRowView drv = e.Row.DataItem as DataRowView;
                bool bchk = Convert.ToBoolean(DataTableConverter.GetValue(e.Row.DataItem, "CHK"));

                if (bchk == true)
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "RSLT_SLURRY_TOTL_QTY")
                        {
                            e.Cancel = false;
                        }
                    }
                }
                else
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridNumericColumn)
                    {
                        if (Convert.ToString(e.Column.Name) == "RSLT_SLURRY_TOTL_QTY")
                        {
                            e.Cancel = true;    // Editing 불가능
                        }
                    }
                    //il.MessageValidation("SFU8276");  //슬러리 재고를 선택하세요
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgResultSum_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHK")).ToString().Equals("ERROR"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
                }

            }));
        }


        private void dgResultSum_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (!dg.CurrentCell.IsEditing)
                {
                    switch (dg.CurrentCell.Column.Name)
                    {
                        case "EQPTID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPTID")?.ToString().Length > 0)
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_EQUIPMENT_BY_EQPTID_CBO", new string[] { "EQSGID", "PROCID", "LANGID" }, new string[] { LoginInfo.CFG_EQSG_ID, Process.MIXING, LoginInfo.LANGID }, CommonCombo.ComboStatus.NONE, dgResultSum.Columns["EQPTID"], "CBO_CODE", "CBO_NAME");
                            break;

                        case "REG_LOTTYPE_NAME":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "REG_LOTTYPE_NAME")?.ToString().Length > 0)
                                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_LOTTYPE_CBO", new string[] {}, new string[] { DataTableConverter.GetValue(e.Cell.Row.DataItem, "REG_LOTTYPE_NAME") as string }, CommonCombo.ComboStatus.NONE, dgResultSum.Columns["REG_LOTTYPE_NAME"], "CBO_CODE", "CBO_NAME");
                            break;

                        case "PRODID":
                            if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString().Length > 0)
                            {
                                DataTableConverter.SetValue(e.Cell.Row.DataItem, "PRODID", DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID").ToString().ToUpper());
                                ProdChecked(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString());
                                SetProdPrjtName(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PRODID")?.ToString());
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //[E20240428-001997]Slurry 재고 조회 입력코드
        void ProdChecked(string prodId)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("PRODID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["PRODID"] = prodId;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTATTR", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4029"); //제품 정보가 없습니다.
                DataTableConverter.SetValue(dgResultSum.Rows[dgResultSum.CurrentCell.Row.Index].DataItem, "PRODID", null);
                return;
            }
            else if (!dt.Rows[0]["CLSS2_CODE"].ToString().Equals("ASL"))
            {
                Util.MessageValidation("SFU9512"); //해당 제품은 슬러리 제품이 아닙니다.
                DataTableConverter.SetValue(dgResultSum.Rows[dgResultSum.CurrentCell.Row.Index].DataItem, "PRODID", null);
                return;
            }

        }

        void SetProdPrjtName(string prodId)
        {
            DataTable IndataTable = new DataTable("INDATA");
            IndataTable.Columns.Add("PRODID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["PRODID"] = prodId;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_VW_PRODUCT_ADJUST", "INDATA", "RSLTDT", IndataTable);

            if (dt == null || dt.Rows.Count == 0)
                DataTableConverter.SetValue(dgResultSum.Rows[dgResultSum.CurrentCell.Row.Index].DataItem, "REG_PRJT_NAME", "");
            else
                DataTableConverter.SetValue(dgResultSum.Rows[dgResultSum.CurrentCell.Row.Index].DataItem, "REG_PRJT_NAME", Util.NVC(dt.Rows[0]["PRJT_NAME"]));

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgResultSum.ItemsSource == null || dgResultSum.Rows.Count == 0)
                    return;

                if (dgResultSum.CurrentRow.DataItem == null)
                    return;
                int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

                DataTable dt = ((DataView)dgResultSum.ItemsSource).Table;

                if (Convert.ToBoolean(DataTableConverter.GetValue(dgResultSum.Rows[rowIndex].DataItem, "CHK")) == true)
                {
                    dgResultSum.SelectedIndex = rowIndex;
                    dgResultSum.Columns["CHK"].IsReadOnly = true;
                    dgResultSum.Columns["CLCT_YMD"].IsReadOnly = false;
                    dgResultSum.Columns["EQPTID"].IsReadOnly = false;
                    dgResultSum.Columns["PRODID"].IsReadOnly = false;
                    dgResultSum.Columns["REG_LOTTYPE_NAME"].IsReadOnly = false;
                    dgResultSum.Columns["REG_PROD_VER_CODE"].IsReadOnly = false;
                    dgResultSum.Columns["REG_PRJT_NAME"].IsReadOnly = false;
                    dgResultSum.Columns["STORAGE_TANK_QTY"].IsReadOnly = false;
                    dgResultSum.Columns["TRANSFER_TANK_QTY"].IsReadOnly = false;
                    dgResultSum.Columns["SPLY_TANK_QTY"].IsReadOnly = false;
                    dgResultSum.Columns["EQPT_SLURRY_TOTL_QTY"].IsReadOnly = false;
                    dgResultSum.Columns["RSLT_SLURRY_TOTL_QTY"].IsReadOnly = false;
                }
                else
                {
                    dgResultSum.Columns["CHK"].IsReadOnly = true;
                    dgResultSum.Columns["CLCT_YMD"].IsReadOnly = true;
                    dgResultSum.Columns["EQPTID"].IsReadOnly = true;
                    dgResultSum.Columns["PRODID"].IsReadOnly = true;
                    dgResultSum.Columns["REG_LOTTYPE_NAME"].IsReadOnly = true;
                    dgResultSum.Columns["REG_PROD_VER_CODE"].IsReadOnly = true;
                    dgResultSum.Columns["REG_PRJT_NAME"].IsReadOnly = true;
                    dgResultSum.Columns["STORAGE_TANK_QTY"].IsReadOnly = true;
                    dgResultSum.Columns["TRANSFER_TANK_QTY"].IsReadOnly = true;
                    dgResultSum.Columns["SPLY_TANK_QTY"].IsReadOnly = true;
                    dgResultSum.Columns["EQPT_SLURRY_TOTL_QTY"].IsReadOnly = true;
                    dgResultSum.Columns["RSLT_SLURRY_TOTL_QTY"].IsReadOnly = true;
                    return;
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region Mehod
        private void SearchData()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgResult);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DAILY_MIXER_TANK_SLURRY_STCK", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgResult, dtMain, FrameOperation);
                    string[] sColumnName = new string[] { "CLCT_YMD", "EQPTID", "EQPTNAME", "PRJT_NAME", "LOTTYPENAME", "PROD_VER_CODE" };
                    _Util.SetDataGridMergeExtensionCol(dgResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void SearchDataHistory()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgIFResult);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DAILY_MIXER_TANK_SLURRY_STCK_HIST", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgIFResult, dtMain, FrameOperation);
                    string[] sColumnName = new string[] { "CLCT_YMD", "EQPTID", "EQPTNAME" };
                    _Util.SetDataGridMergeExtensionCol(dgIFResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void SearchDataSummary()
        {
            try
            {
                if (loadingIndicator != null)
                    loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                Util.gridClear(dgResultSum);

                dgResultSum.Columns["CHK"].IsReadOnly = true;
                dgResultSum.Columns["CLCT_YMD"].IsReadOnly = true;
                dgResultSum.Columns["EQPTID"].IsReadOnly = true;
                dgResultSum.Columns["PRODID"].IsReadOnly = true;
                dgResultSum.Columns["REG_LOTTYPE_NAME"].IsReadOnly = true;
                dgResultSum.Columns["REG_PROD_VER_CODE"].IsReadOnly = true;
                dgResultSum.Columns["REG_PRJT_NAME"].IsReadOnly = true;
                dgResultSum.Columns["STORAGE_TANK_QTY"].IsReadOnly = true;
                dgResultSum.Columns["TRANSFER_TANK_QTY"].IsReadOnly = true;
                dgResultSum.Columns["SPLY_TANK_QTY"].IsReadOnly = true;
                dgResultSum.Columns["EQPT_SLURRY_TOTL_QTY"].IsReadOnly = true;
                dgResultSum.Columns["RSLT_SLURRY_TOTL_QTY"].IsReadOnly = true;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("FDATE", typeof(string));
                IndataTable.Columns.Add("TDATE", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["FDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                Indata["TDATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DAILY_MIXER_TANK_SLURRY_STCK_SUM", "RQSTDT", "RSLTDT", IndataTable);

                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgResultSum, dtMain, FrameOperation);
                    //string[] sColumnName = new string[] { "CLCT_YMD", "EQPTID", "EQPTNAME", "PRODID", "REG_LOTTYPE_NAME", "LOTTYPENAME", "REG_PROD_VER_CODE", "REG_PRJT_NAME" };
                    //_Util.SetDataGridMergeExtensionCol(dgResultSum, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageValidation("SFU1498"); // 데이터가 없습니다.
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void GridColumnVisible()
        {
            try
            {
                //Grid
                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "SLURRY_TANK_COL_VISIBLE";
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    foreach (DataRow drTmp in dtRslt.Rows)
                    {
                        if (drTmp == null) continue;

                        if (dgResult.Columns.Contains(Util.NVC(drTmp["CBO_CODE"])))
                            dgResult.Columns[Util.NVC(drTmp["CBO_CODE"])].Visibility = Visibility.Visible;
                    }
                }

                //Button
                DataTable inTableBtn = new DataTable();
                inTableBtn.TableName = "RQSTDT";
                inTableBtn.Columns.Add("LANGID", typeof(string));
                inTableBtn.Columns.Add("CMCDTYPE", typeof(string));

                DataRow drBtn = inTableBtn.NewRow();
                drBtn["LANGID"] = LoginInfo.LANGID;
                drBtn["CMCDTYPE"] = "SLURRY_STCK_BTN_VISIBILITY";
                inTableBtn.Rows.Add(drBtn);

                DataTable dtRslt2 = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTableBtn);

                if (dtRslt2 != null && dtRslt2.Rows.Count > 0)
                {
                    foreach (DataRow drTmp2 in dtRslt2.Rows)
                    {
                        if (drTmp2 == null) continue;

                        if (Util.NVC(drTmp2["CMCODE"]) == LoginInfo.USERID)
                            btnImport.Visibility = Visibility.Visible;
                    }
                }

            }
            catch (Exception ex) { }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                //데이터 Null 이면
                if (dg.ItemsSource == null || dg.Rows.Count == 0)
                {
                    dt.Columns.Add("CHK", typeof(bool));
                    dt.Columns.Add("CLCT_YMD", typeof(string));
                    dt.Columns.Add("EQPTID", typeof(string));
                    dt.Columns.Add("PRODID", typeof(string));
                    dt.Columns.Add("REG_LOTTYPE_NAME", typeof(string));
                    dt.Columns.Add("REG_PROD_VER_CODE", typeof(string));
                    dt.Columns.Add("REG_PRJT_NAME", typeof(string));
                    dt.Columns.Add("STORAGE_TANK_QTY", typeof(string));
                    dt.Columns.Add("TRANSFER_TANK_QTY", typeof(string));
                    dt.Columns.Add("SPLY_TANK_QTY", typeof(string));
                    dt.Columns.Add("EQPT_SLURRY_TOTL_QTY", typeof(string));
                    dt.Columns.Add("RSLT_SLURRY_TOTL_QTY", typeof(string));
                    dt.Columns.Add("DEL_FLAG", typeof(string));
                    dt.Columns.Add("INSUSER", typeof(string));
                    dt.Columns.Add("INSDTTM", typeof(string));
                    dt.Columns.Add("UPDUSER", typeof(string));
                    dt.Columns.Add("UPDDTTM", typeof(string));
                }
                else
                {
                    foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                    {
                        dt.Columns.Add(Convert.ToString(col.Name));
                    }

                    dt = DataTableConverter.Convert(dg.ItemsSource);
                }
                
                DataRow dr = dt.NewRow();
                dr["CHK"] = 1;
                dr["CLCT_YMD"] = DateTime.Now.ToString("yyyyMMdd");
                dr["EQPTID"] = string.Empty;                                
                dr["PRODID"] = string.Empty;                       
                dr["REG_LOTTYPE_NAME"] = string.Empty;             
                dr["REG_PROD_VER_CODE"] = string.Empty;
                dr["REG_PRJT_NAME"] = string.Empty;                
                dr["STORAGE_TANK_QTY"] = 0.0;             
                dr["TRANSFER_TANK_QTY"] = 0.0;
                dr["SPLY_TANK_QTY"] = 0.0;
                dr["EQPT_SLURRY_TOTL_QTY"] = 0.0;
                dr["RSLT_SLURRY_TOTL_QTY"] = 0.0;
                dr["DEL_FLAG"] = 'N';
                dr["INSUSER"] = LoginInfo.USERID;

                //20210310 저장버튼 누른 시점의 시간이 저장되도록 로직 변경
                //dr["INSDTTM"] = DateTime.Now;                    
                dr["UPDUSER"] = LoginInfo.USERID;
                //dr["UPDDTTM"] = DateTime.Now;

                dt.Rows.Add(dr);
                dt.AcceptChanges();

                dg.ItemsSource = DataTableConverter.Convert(dt);

                //// 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void SlurryStchkSum_Insert()
        {
            try
            {
                dgResultSum.EndEdit();
                inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("CLCT_YMD", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("REG_LOTTYPE_NAME", typeof(string));
                inDataTable.Columns.Add("REG_PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("REG_PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("STORAGE_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("TRANSFER_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("SPLY_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("EQPT_SLURRY_TOTL_QTY", typeof(string));
                inDataTable.Columns.Add("RSLT_SLURRY_TOTL_QTY", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("INSUSER", typeof(string));
                inDataTable.Columns.Add("INSDTTM", typeof(string));

                DataRow inDataRow = null;

                for (int i = 0; i < dgResultSum.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "CHK")) == true)
                    {
                        inDataRow = inDataTable.NewRow();
                        inDataRow["CLCT_YMD"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "CLCT_YMD").ToString().Replace("-",""));
                        inDataRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "EQPTID"));
                        inDataRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "PRODID"));
                        inDataRow["REG_LOTTYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_LOTTYPE_NAME"));
                        inDataRow["REG_PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_PROD_VER_CODE"));
                        inDataRow["REG_PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_PRJT_NAME"));
                        inDataRow["STORAGE_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "STORAGE_TANK_QTY"));
                        inDataRow["TRANSFER_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "TRANSFER_TANK_QTY"));
                        inDataRow["SPLY_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "SPLY_TANK_QTY"));
                        inDataRow["EQPT_SLURRY_TOTL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "EQPT_SLURRY_TOTL_QTY"));
                        inDataRow["RSLT_SLURRY_TOTL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "RSLT_SLURRY_TOTL_QTY"));
                        inDataRow["DEL_FLAG"] = "N";
                        inDataRow["INSUSER"] = LoginInfo.USERID;
                        inDataRow["INSDTTM"] = null;//Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "INSDTTM"));
                        inDataTable.Rows.Add(inDataRow);
                    }
                }

                if (!Validayion(inDataTable)) return;

                if (inDataTable.Rows.Count != 0)
                {
                    new ClientProxy().ExecuteService("BR_MAT_REG_SLURRY_STCK_SUM", "INDATA", null, inDataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                            return;
                        }

                        Inbool = false; //등록 False처리
                        Util.AlertInfo("SFU1270");  //저장되었습니다.
                        SearchDataSummary();
                    });
                    Inbool = false;
                }
                else
                {
                    Util.Alert("SFU1498");  //처리 할 항목이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private void SlurryStchkSum_Update()
        {
            try
            {
                dgResultSum.EndEdit();
                inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("CLCT_YMD", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("REG_LOTTYPE_NAME", typeof(string));
                inDataTable.Columns.Add("REG_PROD_VER_CODE", typeof(string));
                inDataTable.Columns.Add("REG_PRJT_NAME", typeof(string));
                inDataTable.Columns.Add("STORAGE_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("TRANSFER_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("SPLY_TANK_QTY", typeof(string));
                inDataTable.Columns.Add("EQPT_SLURRY_TOTL_QTY", typeof(string));
                inDataTable.Columns.Add("RSLT_SLURRY_TOTL_QTY", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow inDataRow = null;

                for (int i = 0; i < dgResultSum.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "CHK")) == true)
                    {
                        inDataRow = inDataTable.NewRow();
                        inDataRow["CLCT_YMD"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "CLCT_YMD").ToString().Replace("-", ""));
                        inDataRow["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "EQPTID"));
                        inDataRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "PRODID"));
                        inDataRow["REG_LOTTYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_LOTTYPE_NAME"));
                        inDataRow["REG_PROD_VER_CODE"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_PROD_VER_CODE"));
                        inDataRow["REG_PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "REG_PRJT_NAME"));
                        inDataRow["STORAGE_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "STORAGE_TANK_QTY"));
                        inDataRow["TRANSFER_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "TRANSFER_TANK_QTY"));
                        inDataRow["SPLY_TANK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "SPLY_TANK_QTY"));
                        inDataRow["EQPT_SLURRY_TOTL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "EQPT_SLURRY_TOTL_QTY"));
                        inDataRow["RSLT_SLURRY_TOTL_QTY"] = Util.NVC(DataTableConverter.GetValue(dgResultSum.Rows[i].DataItem, "RSLT_SLURRY_TOTL_QTY"));
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(inDataRow);
                    }
                }
                
                if (!Validayion(inDataTable)) return;

                if (inDataTable.Rows.Count != 0)
                {
                    new ClientProxy().ExecuteService("DA_PRD_UPD_SLURRY_STCK_SUM", "INDATA", null, inDataTable, (result, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                            return;
                        }

                        
                        Util.AlertInfo("SFU1270");  //저장되었습니다.
                        SearchDataSummary();
                    });
                }
                else
                {
                    Util.Alert("SFU1498");  //처리 할 항목이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { loadingIndicator.Visibility = Visibility.Collapsed; }
        }

        private bool Validayion(DataTable dt)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (string.IsNullOrEmpty(dt.Rows[i]["EQPTID"].ToString()))
                {
                    Util.MessageValidation("SFU3555"); //설비ID
                    return false;
                    
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["PRODID"].ToString()))
                {
                    Util.MessageValidation("SFU2949"); //제품ID
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["REG_LOTTYPE_NAME"].ToString()))
                {
                    Util.MessageValidation("SFU4068"); //Lot유형
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["REG_PROD_VER_CODE"].ToString())) 
                {
                    Util.MessageValidation("SFU8277"); //버전코드
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["STORAGE_TANK_QTY"].ToString()) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dt.Rows[i], "STORAGE_TANK_QTY")) < 0)
                {
                    Util.MessageValidation("SFU1154");
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["TRANSFER_TANK_QTY"].ToString()) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dt.Rows[i], "TRANSFER_TANK_QTY")) < 0)
                {
                    Util.MessageValidation("SFU1154");
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["SPLY_TANK_QTY"].ToString()) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dt.Rows[i], "SPLY_TANK_QTY")) < 0)
                {
                    Util.MessageValidation("SFU1154");
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["EQPT_SLURRY_TOTL_QTY"].ToString()) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dt.Rows[i], "EQPT_SLURRY_TOTL_QTY")) < 0)
                {
                    Util.MessageValidation("SFU1154");
                    return false;
                }

                if (string.IsNullOrEmpty(dt.Rows[i]["RSLT_SLURRY_TOTL_QTY"].ToString()) ||
                    Util.NVC_Int(DataTableConverter.GetValue(dt.Rows[i], "RSLT_SLURRY_TOTL_QTY")) < 0)
                {
                    Util.MessageValidation("SFU1154");
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
