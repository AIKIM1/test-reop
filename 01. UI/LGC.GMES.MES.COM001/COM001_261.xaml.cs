/*************************************************************************************
 Created Date : 2019.08.22
      Creator : INS 김동일K
   Decription : CWA 전극 배포계획
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.22  INS 김동일K : Initial Created.

**************************************************************************************/

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
using C1.WPF.DataGrid;
using C1.WPF;
using System.Globalization;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_261.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_261 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private string selectedShop = string.Empty;
        private string selectedArea = string.Empty;
        private string selectedEquipmentSegmant = string.Empty;
        private string selectedProcess = string.Empty;
        private string selectedEquipment = string.Empty;
        private string RecipeNo = string.Empty;

        private DataTable DtColumns = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_261()
        {
            InitializeComponent();
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            dtpTo.SelectedDateTime = System.DateTime.Now;
            dtpFrom.SelectedDateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            GetCategoryCombo(); //CATEGORY
            GetMeasureCombo(); //MEASURE
            SettingColumns();
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
            CommonCombo _combo = new CommonCombo();
            
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild, sCase: "cboShopFrom");
            
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            C1ComboBox[] cboAreaParent = { cboShop };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, cbParent: cboAreaParent, sCase: "cboArea");

            //C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessParent = { cboArea };
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "cboProcessByAreaid");

            DataTable dtTmp = new DataTable();
            dtTmp.Columns.Add("CBO_NAME", typeof(string));
            dtTmp.Columns.Add("CBO_CODE", typeof(string));

            DataRow dRow = dtTmp.NewRow();
            dRow["CBO_NAME"] = "E2000 : Coating";
            dRow["CBO_CODE"] = "E2000";
            dtTmp.Rows.Add(dRow);

            cboProcess.ItemsSource = DataTableConverter.Convert(dtTmp);
            cboProcess.SelectedIndex = 0;

            String[] sFilter3 = { Process.COATING };
            C1ComboBox[] cboLineChild = { cboEquipment };
            C1ComboBox[] cboLineParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboLineChild, cbParent: cboLineParent, sFilter: sFilter3, sCase: "EQUIPMENTSEGMENT_PROC");

            String[] sFilter0 = { Process.COATING, null };
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sFilter: sFilter0, sCase: "EQUIPMENT");


            String[] sFilter2 = { "DEMAND_TYPE" };
            _combo.SetCombo(cboDEMAND_TYPE, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            // 극성            
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

           // _combo.SetCombo(cboCategory, CommonCombo.ComboStatus.ALL, sCase: "PRODUCTION_PLAN_CATEGORY");
          
            //String[] sFilter5 = { "PRODUCTION_PLAN_MEASR" };
            //_combo.SetCombo(cboMeasure, CommonCombo.ComboStatus.ALL, sFilter: sFilter5, sCase: "COMMCODE");
        }
        #endregion

        #region Event       

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }

        private void dgList_Loaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.dgList.Loaded -= dgList_Loaded;
                SetGridDate();
            }));
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                string _Day = string.Empty;
                string _colName = string.Empty;
                string _dayofWeek = string.Empty;
                string _date = string.Empty;
                string ValueToFind = string.Empty;
                int iYear;
                int iMon;
                int iDay;
                DateTime DateValue;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Top && Util.NVC(e.Cell.Column.Name).Contains("PLAN_QTY_"))
                    {
                        string[] splitColHeader = e.Cell.Column.ActualGroupHeader.ToString().Split(',');

                        iYear = int.Parse(splitColHeader[0].Trim().Replace("[#]", "").Trim().Substring(0, 4));
                        iMon = int.Parse(splitColHeader[1].Trim().Replace("[#]", "").Replace("(Today)", "").Trim().Substring(0, 2));
                        iDay = int.Parse(splitColHeader[1].Trim().Replace("[#]", "").Replace("(Today)", "").Trim().Substring(3, 2));
                        DateValue = new DateTime(iYear, iMon, iDay);

                        _dayofWeek = Convert.ToString((int)DateValue.DayOfWeek);

                        if (e.Cell.Presenter.Content.GetType() == typeof(C1.WPF.DataGrid.DataGridColumnHeaderPresenter))
                        {
                            System.Windows.Controls.ContentControl cc = (e.Cell.Presenter.Content as System.Windows.Controls.ContentControl);
                            switch (_dayofWeek.Trim())
                            {
                                case "6": //토
                                    cc.Foreground = new SolidColorBrush(Colors.Blue);
                                    break;
                                case "0": //일
                                    cc.Foreground = new SolidColorBrush(Colors.Red);
                                    break;
                                default:
                                    cc.Foreground = new SolidColorBrush(Colors.Black);
                                    break;
                            }
                        }
                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 62)
                {
                    Util.MessageValidation("SFU5033", new object[] { "2" });   // 기간은 {}달 이내 입니다.
                    dtpTo.SelectedDateTime = dtpFrom.SelectedDateTime.AddDays(60);
                    //SetGridDate();
                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
            }
        }

        private void dtpTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpFrom.SelectedDateTime.Year > 1 && dtpTo.SelectedDateTime.Year > 1)
            {
                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 62)
                {
                    Util.MessageValidation("SFU5033", new object[] { "2" });  // 기간은 {}달 이내 입니다.
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime.AddDays(-60);
                    //SetGridDate();
                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                    //SetGridDate();
                    return;
                }
            }
        }

        /// <summary>
        /// 2019 10 17 오화백 히든컬럼 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboCloumSetting_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                for (int i = 0; i < DtColumns.Rows.Count; i++)
                {
                    if (dgList.Columns.Contains(DtColumns.Rows[i]["CBO_CODE"].ToString()))
                        dgList.Columns[DtColumns.Rows[i]["CBO_CODE"].ToString()].Visibility = Visibility.Visible;
                }
                System.Collections.Generic.IList<object> list = cboCloumSetting.SelectedItems;
                foreach (string str in list)
                {

                    if (dgList.Columns.Contains(str))
                        dgList.Columns[str].Visibility = Visibility.Collapsed;
                }
            }
            catch
            {
            }
        }
        /// <summary>
        /// 2019 10 17 오화백 컬럼 머지 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                //기준 컬럼의 변수
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                #region 각 컬럼 머지에 대한 변수 선언

                // 기준컬럼을 기준으로 머지 시킬때 한번의 For문으로 각 컬럼을 머지키시기 위해 컬럼별로 변수를 선언
                //Areaname
                int AreaidxS = 0;
                int AreaidxE = 0;
                bool bAreaStrt = false;
                string sTmpArea = string.Empty;

                //PROCNAME
                int ProcidxS = 0;
                int ProcidxE = 0;
                bool bProcStrt = false;
                string sTmpProc = string.Empty;

                //EQSGNAME
                int EqsgidxS = 0;
                int EqsgidxE = 0;
                bool bEqsgStrt = false;
                string sTmpEqsg = string.Empty;

                //EQPTNAME
                int EqptidxS = 0;
                int EqptidxE = 0;
                bool bEqptStrt = false;
                string sTmpEqpt = string.Empty;

                //PRODID
                int ProdidxS = 0;
                int ProdidxE = 0;
                bool bProdStrt = false;
                string sTmpProd = string.Empty;

                //PRJT_NAME
                int PrjtidxS = 0;
                int PrjtidxE = 0;
                bool bPrjtStrt = false;
                string sTmpPrjt = string.Empty;

                //DEMAND_TYPE
                int DemandidxS = 0;
                int DemandidxE = 0;
                bool bDemandStrt = false;
                string sTmpDemand = string.Empty;

                //MKT_TYPE_NAME
                int MktidxS = 0;
                int MktidxE = 0;
                bool bMktStrt = false;
                string sTmpMkt = string.Empty;


                //ELTR_TYPE
                int EltridxS = 0;
                int EltridxE = 0;
                bool bEltrStrt = false;
                string sTmpEltr = string.Empty;


                //MODLID
                int ModlidxS = 0;
                int ModlidxE = 0;
                bool bModlStrt = false;
                string sTmpMold = string.Empty;

                //PRE_DISPERSION_ID
                int Dispersion_ididxS = 0;
                int Dispersion_ididxE = 0;
                bool bDispersion_idStrt = false;
                string sTmpDispersion_id = string.Empty;

                //PRE_DISPERSION_NAME
                int Dispersion_nameidxS = 0;
                int Dispersion_nameidxE = 0;
                bool bDispersion_nameStrt = false;
                string sTmpDispersion_name = string.Empty;

                //ACTIVE_MTRL1
                int Mtrl1idxS = 0;
                int Mtrl1idxE = 0;
                bool bMtrl1Strt = false;
                string sTmpMtrl1 = string.Empty;

                //ACTIVE_MTRL2
                int Mtrl2idxS = 0;
                int Mtrl2idxE = 0;
                bool bMtrl2Strt = false;
                string sTmpMtrl2 = string.Empty;

                //ACTIVE_MTRL3
                int Mtrl3idxS = 0;
                int Mtrl3idxE = 0;
                bool bMtrl3Strt = false;
                string sTmpMtrl3 = string.Empty;

                // BINDER1
                int Binder1idxS = 0;
                int Binder1idxE = 0;
                bool bBinder1Strt = false;
                string sTmpBinder1 = string.Empty;

                // BINDER2
                int Binder2idxS = 0;
                int Binder2idxE = 0;
                bool bBinder2Strt = false;
                string sTmpBinder2 = string.Empty;

                // BINDER3
                int Binder3idxS = 0;
                int Binder3idxE = 0;
                bool bBinder3Strt = false;
                string sTmpBinder3 = string.Empty;

                //CONDUCTION_MTRL1
                int Conduction1idxS = 0;
                int Conduction1idxE = 0;
                bool bConduction1Strt = false;
                string sTmpConduction1 = string.Empty;

                //CONDUCTION_MTRL2
                int Conduction2idxS = 0;
                int Conduction2idxE = 0;
                bool bConduction2Strt = false;
                string sTmpConduction2 = string.Empty;

                //CMC
                int CmcidxS = 0;
                int CmcidxE = 0;
                bool bCmcStrt = false;
                string sTmpCmc = string.Empty;

                //FOIL
                int FoilidxS = 0;
                int FoilidxE = 0;
                bool bFoilStrt = false;
                string sTmpFoil = string.Empty;

                //DAILY_CAPA_PPM
                int CapaidxS = 0;
                int CapaidxE = 0;
                bool bCapaStrt = false;
                string sTmpCapa = string.Empty;

                //PROD_VER_CODE
                int VeridxS = 0;
                int VeriddxE = 0;
                bool bVerStrt = false;
                string sTmpVer = string.Empty;

                //CATEGORY_DISP_NAME
                int CategoryidxS = 0;
                int CategoryidxE = 0;
                bool bCategoryStrt = false;
                string sTmpCategory = string.Empty;

                //MEASR_NAME
                int MeasridxS = 0;
                int MeasridxE = dgList.TopRows.Count;
                bool bMeasrStrt = false;
                string sTmpMeasr = string.Empty;
                #endregion

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count - 1; i++)
                {
                    if (dgList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PLAN_GRP_ID")).Equals(""))
                        {
                            if (bStrt)
                            {
                                #region 기준 컬럼(PLAN_GRP_ID)에 대한 각 컬럼 머지 
                                for (int m = idxS; m <= idxE; m++)
                                {
                                    //AreaName
                                    if (!bAreaStrt)
                                    {
                                        bAreaStrt = true;
                                        sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                        AreaidxS = m;
                                        //if (sTmpArea.Equals(""))
                                        //    bAreaStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME")).Equals(sTmpArea))
                                            AreaidxE = m;
                                        else
                                        {
                                            if (AreaidxS > AreaidxE)
                                            {
                                                AreaidxE = AreaidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                            bAreaStrt = true;
                                            sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                            AreaidxS = m;
                                            //if (sTmpArea.Equals(""))
                                            //    bAreaStrt = false;
                                        }
                                    }
                                    //PROCNAME
                                    if (!bProcStrt)
                                    {
                                        bProcStrt = true;
                                        sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                        ProcidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bProcStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME")).Equals(sTmpProc))
                                            ProcidxE = m;
                                        else
                                        {
                                            if (ProcidxS > ProcidxE)
                                            {
                                                ProcidxE = ProcidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                            bProcStrt = true;
                                            sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                            ProcidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bProcStrt = false;
                                        }
                                    }

                                    //EQSGNAME
                                    if (!bEqsgStrt)
                                    {
                                        bEqsgStrt = true;
                                        sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                        EqsgidxS = m;
                                        //if (sTmpEqsg.Equals(""))
                                        //    bEqsgStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME")).Equals(sTmpEqsg))
                                            EqsgidxE = m;
                                        else
                                        {
                                            if (EqsgidxS > EqsgidxE)
                                            {
                                                EqsgidxE = EqsgidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                            bEqsgStrt = true;
                                            sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                            EqsgidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEqsgStrt = false;
                                        }
                                    }
                                    //EQPTNAME
                                    if (!bEqptStrt)
                                    {
                                        bEqptStrt = true;
                                        sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                        EqptidxS = m;
                                        //if (sTmpEqpt.Equals(""))
                                        //    bEqptStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME")).Equals(sTmpEqpt))
                                            EqptidxE = m;
                                        else
                                        {
                                            if (EqptidxS > EqptidxE)
                                            {
                                                EqptidxE = EqptidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                            bEqptStrt = true;
                                            sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                            EqptidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEqptStrt = false;
                                        }
                                    }

                                    //PRODID
                                    if (!bProdStrt)
                                    {
                                        bProdStrt = true;
                                        sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                        ProdidxS = m;
                                        //if (sTmpProd.Equals(""))
                                        //    bProdStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID")).Equals(sTmpProd))
                                            ProdidxE = m;
                                        else
                                        {
                                            if (ProdidxS > ProdidxE)
                                            {
                                                ProdidxE = ProdidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                            bProdStrt = true;
                                            sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                            ProdidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bProdStrt = false;
                                        }
                                    }

                                    //PRJT_NAME
                                    if (!bPrjtStrt)
                                    {
                                        bPrjtStrt = true;
                                        sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                        PrjtidxS = m;
                                        //if (sTmpPrjt.Equals(""))
                                        //    bPrjtStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME")).Equals(sTmpPrjt))
                                            PrjtidxE = m;
                                        else
                                        {
                                            if (PrjtidxS > PrjtidxE)
                                            {
                                                PrjtidxE = PrjtidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                            bPrjtStrt = true;
                                            sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                            PrjtidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bPrjtStrt = false;
                                        }
                                    }

                                    //DEMAND_TYPE
                                    if (!bDemandStrt)
                                    {
                                        bDemandStrt = true;
                                        sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                        DemandidxS = m;
                                        //if (sTmpDemand.Equals(""))
                                        //    bDemandStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE")).Equals(sTmpDemand))
                                            DemandidxE = m;
                                        else
                                        {
                                            if (DemandidxS > DemandidxE)
                                            {
                                                DemandidxE = DemandidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                            bDemandStrt = true;
                                            sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                            DemandidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDemandStrt = false;
                                        }
                                    }
                                    //MKT_TYPE_NAME
                                    if (!bMktStrt)
                                    {
                                        bMktStrt = true;
                                        sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                        MktidxS = m;
                                        //if (sTmpMkt.Equals(""))
                                        //    bMktStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME")).Equals(sTmpMkt))
                                            MktidxE = m;
                                        else
                                        {
                                            if (MktidxS > MktidxE)
                                            {
                                                MktidxE = MktidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                            bMktStrt = true;
                                            sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                            MktidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMktStrt = false;
                                        }
                                    }
                                    //ELTR_TYPE
                                    if (!bEltrStrt)
                                    {
                                        bEltrStrt = true;
                                        sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                        EltridxS = m;
                                        //if (sTmpEltr.Equals(""))
                                        //    bEltrStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE")).Equals(sTmpEltr))
                                            EltridxE = m;
                                        else
                                        {
                                            if (EltridxS > EltridxE)
                                            {
                                                EltridxE = EltridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                            bEltrStrt = true;
                                            sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                            EltridxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEltrStrt = false;
                                        }
                                    }

                                    //MODLID
                                    if (!bModlStrt)
                                    {
                                        bModlStrt = true;
                                        sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                        ModlidxS = m;
                                        //if (sTmpMold.Equals(""))
                                        //    bModlStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID")).Equals(sTmpMold))
                                            ModlidxE = m;
                                        else
                                        {
                                            if (ModlidxS > ModlidxE)
                                            {
                                                ModlidxE = ModlidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                            bModlStrt = true;
                                            sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                            ModlidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bModlStrt = false;
                                        }
                                    }

                                    //PRE_DISPERSION_ID
                                    if (!bDispersion_idStrt)
                                    {
                                        bDispersion_idStrt = true;
                                        sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                        Dispersion_ididxS = m;
                                        //if (sTmpDispersion_id.Equals(""))
                                        //    bDispersion_idStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID")).Equals(sTmpDispersion_id))
                                            Dispersion_ididxE = m;
                                        else
                                        {
                                            if (Dispersion_ididxS > Dispersion_ididxE)
                                            {
                                                Dispersion_ididxE = Dispersion_ididxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                            bDispersion_idStrt = true;
                                            sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                            Dispersion_ididxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDispersion_idStrt = false;
                                        }
                                    }

                                    //PRE_DISPERSION_NAME
                                    if (!bDispersion_nameStrt)
                                    {
                                        bDispersion_nameStrt = true;
                                        sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                        Dispersion_nameidxS = m;
                                        //if (sTmpDispersion_name.Equals(""))
                                        //    bDispersion_nameStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME")).Equals(sTmpDispersion_name))
                                            Dispersion_nameidxE = m;
                                        else
                                        {
                                            if (Dispersion_nameidxS > Dispersion_nameidxE)
                                            {
                                                Dispersion_nameidxE = Dispersion_nameidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                            bDispersion_nameStrt = true;
                                            sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                            Dispersion_nameidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDispersion_nameStrt = false;
                                        }
                                    }
                                    //ACTIVE_MTRL1
                                    if (!bMtrl1Strt)
                                    {
                                        bMtrl1Strt = true;
                                        sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                        Mtrl1idxS = m;
                                        //if (sTmpMtrl1.Equals(""))
                                        //    bMtrl1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1")).Equals(sTmpMtrl1))
                                            Mtrl1idxE = m;
                                        else
                                        {
                                            if (Mtrl1idxS > Mtrl1idxE)
                                            {
                                                Mtrl1idxE = Mtrl1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                            bMtrl1Strt = true;
                                            sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                            Mtrl1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl1Strt = false;
                                        }
                                    }

                                    //ACTIVE_MTRL2
                                    if (!bMtrl2Strt)
                                    {
                                        bMtrl2Strt = true;
                                        sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                        Mtrl2idxS = m;
                                        //if (sTmpMtrl2.Equals(""))
                                        //    bMtrl2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2")).Equals(sTmpMtrl2))
                                            Mtrl2idxE = m;
                                        else
                                        {
                                            if (Mtrl2idxS > Mtrl2idxE)
                                            {
                                                Mtrl2idxE = Mtrl2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                            bMtrl2Strt = true;
                                            sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                            Mtrl2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl2Strt = false;
                                        }
                                    }
                                    //ACTIVE_MTRL3
                                    if (!bMtrl3Strt)
                                    {
                                        bMtrl3Strt = true;
                                        sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                        Mtrl3idxS = m;
                                        //if (sTmpMtrl3.Equals(""))
                                        //    bMtrl3Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3")).Equals(sTmpMtrl3))
                                            Mtrl3idxE = m;
                                        else
                                        {
                                            if (Mtrl3idxS > Mtrl3idxE)
                                            {
                                                Mtrl3idxE = Mtrl3idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                            bMtrl3Strt = true;
                                            sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                            Mtrl3idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl3Strt = false;
                                        }
                                    }
                                    //BINDER1
                                    if (!bBinder1Strt)
                                    {
                                        bBinder1Strt = true;
                                        sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                        Binder1idxS = m;
                                        //if (sTmpBinder1.Equals(""))
                                        //    bBinder1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1")).Equals(sTmpBinder1))
                                            Binder1idxE = m;
                                        else
                                        {
                                            if (Binder1idxS > Binder1idxE)
                                            {
                                                Binder1idxE = Binder1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                            bBinder1Strt = true;
                                            sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                            Binder1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder1Strt = false;
                                        }
                                    }

                                    //BINDER2
                                    if (!bBinder2Strt)
                                    {
                                        bBinder2Strt = true;
                                        sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                        Binder2idxS = m;
                                        //if (sTmpBinder2.Equals(""))
                                        //    bBinder2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2")).Equals(sTmpBinder2))
                                            Binder2idxE = m;
                                        else
                                        {
                                            if (Binder2idxS > Binder2idxE)
                                            {
                                                Binder2idxE = Binder2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["BINDER2"].Index)));
                                            bBinder2Strt = true;
                                            sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                            Binder2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder2Strt = false;
                                        }
                                    }
                                    //BINDER3
                                    if (!bBinder3Strt)
                                    {
                                        bBinder3Strt = true;
                                        sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                        Binder3idxS = m;
                                        //if (sTmpBinder3.Equals(""))
                                        //    bBinder3Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3")).Equals(sTmpBinder3))
                                            Binder3idxE = m;
                                        else
                                        {
                                            if (Binder3idxS > Binder3idxE)
                                            {
                                                Binder3idxE = Binder3idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                            bBinder3Strt = true;
                                            sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                            Binder3idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder3Strt = false;
                                        }
                                    }
                                    //CONDUCTION_MTRL1
                                    if (!bConduction1Strt)
                                    {
                                        bConduction1Strt = true;
                                        sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                        Conduction1idxS = m;
                                        //if (sTmpConduction1.Equals(""))
                                        //    bConduction1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1")).Equals(sTmpConduction1))
                                            Conduction1idxE = m;
                                        else
                                        {
                                            if (Conduction1idxS > Conduction1idxE)
                                            {
                                                Conduction1idxE = Conduction1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                            bConduction1Strt = true;
                                            sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                            Conduction1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bConduction1Strt = false;
                                        }
                                    }

                                    //CONDUCTION_MTRL2
                                    if (!bConduction2Strt)
                                    {
                                        bConduction2Strt = true;
                                        sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                        Conduction2idxS = m;
                                        //if (sTmpConduction2.Equals(""))
                                        //    bConduction2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2")).Equals(sTmpConduction2))
                                            Conduction2idxE = m;
                                        else
                                        {
                                            if (Conduction2idxS > Conduction2idxE)
                                            {
                                                Conduction2idxE = Conduction2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                            bConduction2Strt = true;
                                            sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                            Conduction2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bConduction2Strt = false;
                                        }
                                    }

                                    //CMC
                                    if (!bCmcStrt)
                                    {
                                        bCmcStrt = true;
                                        sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                        CmcidxS = m;
                                        //if (sTmpCmc.Equals(""))
                                        //    bCmcStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC")).Equals(sTmpCmc))
                                            CmcidxE = m;
                                        else
                                        {
                                            if (CmcidxS > CmcidxE)
                                            {
                                                CmcidxE = CmcidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                            bCmcStrt = true;
                                            sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                            CmcidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bCmcStrt = false;
                                        }
                                    }

                                    //FOIL
                                    if (!bFoilStrt)
                                    {
                                        bFoilStrt = true;
                                        sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                        FoilidxS = m;
                                        //if (sTmpFoil.Equals(""))
                                        //    bFoilStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL")).Equals(sTmpFoil))
                                            FoilidxE = m;
                                        else
                                        {
                                            if (FoilidxS > FoilidxE)
                                            {
                                                FoilidxE = FoilidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                            bFoilStrt = true;
                                            sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                            FoilidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bFoilStrt = false;
                                        }
                                    }
                                    //DAILY_CAPA_PPM
                                    if (!bCapaStrt)
                                    {
                                        bCapaStrt = true;
                                        sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                        CapaidxS = m;
                                        //if (sTmpCapa.Equals(""))
                                        //    bCapaStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM")).Equals(sTmpCapa))
                                            CapaidxE = m;
                                        else
                                        {
                                            if (CapaidxS > CapaidxE)
                                            {
                                                CapaidxE = CapaidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                            bCapaStrt = true;
                                            sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                            CapaidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bCapaStrt = false;
                                        }
                                    }
                                    //PROD_VER_CODE
                                    if (!bVerStrt)
                                    {
                                        bVerStrt = true;
                                        sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                        VeridxS = m;
                                        //if (sTmpVer.Equals(""))
                                        //    bVerStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE")).Equals(sTmpVer))
                                            VeriddxE = m;
                                        else
                                        {
                                            if (VeridxS > VeriddxE)
                                            {
                                                VeriddxE = VeridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                            bVerStrt = true;
                                            sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                            VeridxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bVerStrt = false;
                                        }
                                    }
                                    //CATEGORY_DISP_NAME
                                    if (!bCategoryStrt)
                                    {
                                        bCategoryStrt = true;
                                        sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                        CategoryidxS = m;
                                        //if (sTmpCategory.Equals(""))
                                        //    bCategoryStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME")).Equals(sTmpCategory))
                                            CategoryidxE = m;
                                        else
                                        {
                                            if (CategoryidxS > CategoryidxE)
                                            {
                                                CategoryidxE = CategoryidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                            bCategoryStrt = true;
                                            sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                            CategoryidxS = m;
                                            //if (sTmpCategory.Equals(""))
                                            //    bCategoryStrt = false;
                                        }
                                    }
                                    //MEASR_NAME
                                    if (!bMeasrStrt)
                                    {
                                        bMeasrStrt = true;
                                        sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                        MeasridxS = m;
                                        //if (sTmpMeasr.Equals(""))
                                        //    bMeasrStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME")).Equals(sTmpMeasr))
                                            MeasridxE = m;
                                        else
                                        {
                                            if (MeasridxS > MeasridxE)
                                            {
                                                MeasridxE = MeasridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                            bMeasrStrt = true;
                                            sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                            MeasridxS = m;
                                            //if (sTmpMeasr.Equals(""))
                                            //    bMeasrStrt = false;
                                        }
                                    }
                                }
                                //AREANAME
                                if (bAreaStrt)
                                {
                                    if (AreaidxS > AreaidxE)
                                    {
                                        AreaidxE = AreaidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                    bAreaStrt = false;
                                    sTmpArea = string.Empty;
                                    AreaidxE = 0;
                                    AreaidxS = 0;

                                }
                                //PROCNAME
                                if (bProcStrt)
                                {
                                    if (ProcidxS > ProcidxE)
                                    {
                                        ProcidxE = ProcidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                    ProcidxS = 0;
                                    ProcidxE = 0;
                                    bProcStrt = false;
                                    sTmpProc = string.Empty;
                                }
                                //EQSGNAME
                                if (bEqsgStrt)
                                {
                                    if (EqsgidxS > EqsgidxE)
                                    {
                                        EqsgidxE = EqsgidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                    EqsgidxS = 0;
                                    EqsgidxE = 0;
                                    bEqsgStrt = false;
                                    sTmpEqsg = string.Empty;
                                }
                                //EQPTNAME
                                if (bEqptStrt)
                                {
                                    if (EqptidxS > EqptidxE)
                                    {
                                        EqptidxE = EqptidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                    EqptidxS = 0;
                                    EqptidxE = 0;
                                    bEqptStrt = false;
                                    sTmpEqpt = string.Empty;
                                }
                                //PRODID
                                if (bProdStrt)
                                {
                                    if (ProdidxS > ProdidxE)
                                    {
                                        ProdidxE = ProdidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                    ProdidxS = 0;
                                    ProdidxE = 0;
                                    bProdStrt = false;
                                    sTmpProd = string.Empty;
                                }
                                //PRJT_NAME
                                if (bPrjtStrt)
                                {
                                    if (PrjtidxS > PrjtidxE)
                                    {
                                        PrjtidxE = PrjtidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                    PrjtidxS = 0;
                                    PrjtidxE = 0;
                                    bPrjtStrt = false;
                                    sTmpPrjt = string.Empty;
                                }
                                //DEMAND_TYPE
                                if (bDemandStrt)
                                {
                                    if (DemandidxS > DemandidxE)
                                    {
                                        DemandidxE = DemandidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                    DemandidxS = 0;
                                    DemandidxE = 0;
                                    bDemandStrt = false;
                                    sTmpDemand = string.Empty;
                                }
                                //MKT_TYPE_NAME
                                if (bMktStrt)
                                {
                                    if (MktidxS > MktidxE)
                                    {
                                        MktidxE = MktidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                    MktidxS = 0;
                                    MktidxE = 0;
                                    bMktStrt = false;
                                    sTmpMkt = string.Empty;
                                }

                                //ELTR_TYPE
                                if (bEltrStrt)
                                {
                                    if (EltridxS > EltridxE)
                                    {
                                        EltridxE = EltridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                    EltridxS = 0;
                                    EltridxE = 0;
                                    bEltrStrt = false;
                                    sTmpEltr = string.Empty;
                                }

                                //MODLID
                                if (bModlStrt)
                                {
                                    if (ModlidxS > ModlidxE)
                                    {
                                        ModlidxE = ModlidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                    ModlidxS = 0;
                                    ModlidxE = 0;
                                    bModlStrt = false;
                                    sTmpMold = string.Empty;
                                }
                                //PRE_DISPERSION_ID
                                if (bDispersion_idStrt)
                                {
                                    if (Dispersion_ididxS > Dispersion_ididxE)
                                    {
                                        Dispersion_ididxE = Dispersion_ididxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                    Dispersion_ididxS = 0;
                                    Dispersion_ididxE = 0;
                                    bDispersion_idStrt = false;
                                    sTmpDispersion_id = string.Empty;
                                }
                                //PRE_DISPERSION_NAME
                                if (bDispersion_nameStrt)
                                {
                                    if (Dispersion_nameidxS > Dispersion_nameidxE)
                                    {
                                        Dispersion_nameidxE = Dispersion_nameidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                    Dispersion_nameidxS = 0;
                                    Dispersion_nameidxE = 0;
                                    bDispersion_nameStrt = false;
                                    sTmpDispersion_name = string.Empty;
                                }
                                //ACTIVE_MTRL1
                                if (bMtrl1Strt)
                                {
                                    if (Mtrl1idxS > Mtrl1idxE)
                                    {
                                        Mtrl1idxE = Mtrl1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                    Mtrl1idxS = 0;
                                    Mtrl1idxE = 0;
                                    bMtrl1Strt = false;
                                    sTmpMtrl1 = string.Empty;
                                }
                                //ACTIVE_MTRL2
                                if (bMtrl2Strt)
                                {
                                    if (Mtrl2idxS > Mtrl2idxE)
                                    {
                                        Mtrl2idxE = Mtrl2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                    Mtrl2idxS = 0;
                                    Mtrl2idxE = 0;
                                    bMtrl2Strt = false;
                                    sTmpMtrl2 = string.Empty;
                                }
                                //ACTIVE_MTRL3
                                if (bMtrl3Strt)
                                {
                                    if (Mtrl3idxS > Mtrl3idxE)
                                    {
                                        Mtrl3idxE = Mtrl3idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                    Mtrl3idxS = 0;
                                    Mtrl3idxE = 0;
                                    bMtrl3Strt = false;
                                    sTmpMtrl3 = string.Empty;
                                }
                                // BINDER1
                                if (bBinder1Strt)
                                {
                                    if (Binder1idxS > Binder1idxE)
                                    {
                                        Binder1idxE = Binder1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                    Binder1idxS = 0;
                                    Binder1idxE = 0;
                                    bBinder1Strt = false;
                                    sTmpBinder1 = string.Empty;
                                }
                                //BINDER2
                                if (bBinder2Strt)
                                {
                                    if (Binder2idxS > Binder2idxE)
                                    {
                                        Binder2idxE = Binder2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Binder2idxE, dgList.Columns["BINDER2"].Index)));
                                    Binder2idxS = 0;
                                    Binder2idxE = 0;
                                    bBinder2Strt = false;
                                    sTmpBinder2 = string.Empty;
                                }
                                // BINDER3
                                if (bBinder3Strt)
                                {
                                    if (Binder3idxS > Binder3idxE)
                                    {
                                        Binder3idxE = Binder3idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                    Binder3idxS = 0;
                                    Binder3idxE = 0;
                                    bBinder3Strt = false;
                                    sTmpBinder3 = string.Empty;
                                }
                                //CONDUCTION_MTRL1
                                if (bConduction1Strt)
                                {
                                    if (Conduction1idxS > Conduction1idxE)
                                    {
                                        Conduction1idxE = Conduction1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                    Conduction1idxS = 0;
                                    Conduction1idxE = 0;
                                    bConduction1Strt = false;
                                    sTmpConduction1 = string.Empty;
                                }
                                //CONDUCTION_MTRL2
                                if (bConduction2Strt)
                                {
                                    if (Conduction2idxS > Conduction2idxE)
                                    {
                                        Conduction2idxE = Conduction2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                    Conduction2idxS = 0;
                                    Conduction2idxE = 0;
                                    bConduction2Strt = false;
                                    sTmpConduction2 = string.Empty;
                                }
                                //CMC
                                if (bCmcStrt)
                                {
                                    if (CmcidxS > CmcidxE)
                                    {
                                        CmcidxE = CmcidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                    CmcidxS = 0;
                                    CmcidxE = 0;
                                    bCmcStrt = false;
                                    sTmpCmc = string.Empty;
                                }
                                //FOIL
                                if (bFoilStrt)
                                {
                                    if (FoilidxS > FoilidxE)
                                    {
                                        FoilidxE = FoilidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                    FoilidxS = 0;
                                    FoilidxE = 0;
                                    bFoilStrt = false;
                                    sTmpFoil = string.Empty;
                                }
                                //DAILY_CAPA_PPM
                                if (bCapaStrt)
                                {
                                    if (CapaidxS > CapaidxE)
                                    {
                                        CapaidxE = CapaidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                    CapaidxS = 0;
                                    CapaidxE = 0;
                                    bCapaStrt = false;
                                    sTmpCapa = string.Empty;
                                }
                                //PROD_VER_CODE
                                if (bVerStrt)
                                {
                                    if (VeridxS > VeriddxE)
                                    {
                                        VeriddxE = VeridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                    VeridxS = 0;
                                    VeriddxE = 0;
                                    bVerStrt = false;
                                    sTmpVer = string.Empty;
                                }
                                //CATEGORY_DISP_NAME
                                if (bCategoryStrt)
                                {
                                    if (CategoryidxS > CategoryidxE)
                                    {
                                        CategoryidxE = CategoryidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                    CategoryidxS = 0;
                                    CategoryidxE = 0;
                                    bCategoryStrt = false;
                                    sTmpCategory = string.Empty;
                                }
                                //MEASR
                                if (bMeasrStrt)
                                {
                                    if (MeasridxS > MeasridxE)
                                    {
                                        MeasridxE = MeasridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                    bMeasrStrt = false;
                                    MeasridxS = 0;
                                    MeasridxE = dgList.TopRows.Count;
                                    sTmpMeasr = string.Empty;
                                }
                                #endregion

                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PLAN_GRP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PLAN_GRP_ID")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                #region 기준 컬럼(PLAN_GRP_ID)에 대한 각 컬럼 머지 
                                for (int m = idxS; m <= idxE; m++)
                                {
                                    //AreaName
                                    if (!bAreaStrt)
                                    {
                                        bAreaStrt = true;
                                        sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                        AreaidxS = m;
                                        //if (sTmpArea.Equals(""))
                                        //    bAreaStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME")).Equals(sTmpArea))
                                            AreaidxE = m;
                                        else
                                        {
                                            if (AreaidxS > AreaidxE)
                                            {
                                                AreaidxE = AreaidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                            bAreaStrt = true;
                                            sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                            AreaidxS = m;
                                            //if (sTmpArea.Equals(""))
                                            //    bAreaStrt = false;
                                        }
                                    }
                                    //PROCNAME
                                    if (!bProcStrt)
                                    {
                                        bProcStrt = true;
                                        sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                        ProcidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bProcStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME")).Equals(sTmpProc))
                                            ProcidxE = m;
                                        else
                                        {
                                            if (ProcidxS > ProcidxE)
                                            {
                                                ProcidxE = ProcidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                            bProcStrt = true;
                                            sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                            ProcidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bProcStrt = false;
                                        }
                                    }

                                    //EQSGNAME
                                    if (!bEqsgStrt)
                                    {
                                        bEqsgStrt = true;
                                        sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                        EqsgidxS = m;
                                        //if (sTmpEqsg.Equals(""))
                                        //    bEqsgStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME")).Equals(sTmpEqsg))
                                            EqsgidxE = m;
                                        else
                                        {
                                            if (EqsgidxS > EqsgidxE)
                                            {
                                                EqsgidxE = EqsgidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                            bEqsgStrt = true;
                                            sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                            EqsgidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEqsgStrt = false;
                                        }
                                    }
                                    //EQPTNAME
                                    if (!bEqptStrt)
                                    {
                                        bEqptStrt = true;
                                        sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                        EqptidxS = m;
                                        //if (sTmpEqpt.Equals(""))
                                        //    bEqptStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME")).Equals(sTmpEqpt))
                                            EqptidxE = m;
                                        else
                                        {
                                            if (EqptidxS > EqptidxE)
                                            {
                                                EqptidxE = EqptidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                            bEqptStrt = true;
                                            sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                            EqptidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEqptStrt = false;
                                        }
                                    }

                                    //PRODID
                                    if (!bProdStrt)
                                    {
                                        bProdStrt = true;
                                        sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                        ProdidxS = m;
                                        //if (sTmpProd.Equals(""))
                                        //    bProdStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID")).Equals(sTmpProd))
                                            ProdidxE = m;
                                        else
                                        {
                                            if (ProdidxS > ProdidxE)
                                            {
                                                ProdidxE = ProdidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                            bProdStrt = true;
                                            sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                            ProdidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bProdStrt = false;
                                        }
                                    }

                                    //PRJT_NAME
                                    if (!bPrjtStrt)
                                    {
                                        bPrjtStrt = true;
                                        sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                        PrjtidxS = m;
                                        //if (sTmpPrjt.Equals(""))
                                        //    bPrjtStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME")).Equals(sTmpPrjt))
                                            PrjtidxE = m;
                                        else
                                        {
                                            if (PrjtidxS > PrjtidxE)
                                            {
                                                PrjtidxE = PrjtidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                            bPrjtStrt = true;
                                            sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                            PrjtidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bPrjtStrt = false;
                                        }
                                    }

                                    //DEMAND_TYPE
                                    if (!bDemandStrt)
                                    {
                                        bDemandStrt = true;
                                        sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                        DemandidxS = m;
                                        //if (sTmpDemand.Equals(""))
                                        //    bDemandStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE")).Equals(sTmpDemand))
                                            DemandidxE = m;
                                        else
                                        {
                                            if (DemandidxS > DemandidxE)
                                            {
                                                DemandidxE = DemandidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                            bDemandStrt = true;
                                            sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                            DemandidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDemandStrt = false;
                                        }
                                    }
                                    //MKT_TYPE_NAME
                                    if (!bMktStrt)
                                    {
                                        bMktStrt = true;
                                        sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                        MktidxS = m;
                                        //if (sTmpMkt.Equals(""))
                                        //    bMktStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME")).Equals(sTmpMkt))
                                            MktidxE = m;
                                        else
                                        {
                                            if (MktidxS > MktidxE)
                                            {
                                                MktidxE = MktidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                            bMktStrt = true;
                                            sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                            MktidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMktStrt = false;
                                        }
                                    }
                                    //ELTR_TYPE
                                    if (!bEltrStrt)
                                    {
                                        bEltrStrt = true;
                                        sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                        EltridxS = m;
                                        //if (sTmpEltr.Equals(""))
                                        //    bEltrStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE")).Equals(sTmpEltr))
                                            EltridxE = m;
                                        else
                                        {
                                            if (EltridxS > EltridxE)
                                            {
                                                EltridxE = EltridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                            bEltrStrt = true;
                                            sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                            EltridxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bEltrStrt = false;
                                        }
                                    }

                                    //MODLID
                                    if (!bModlStrt)
                                    {
                                        bModlStrt = true;
                                        sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                        ModlidxS = m;
                                        //if (sTmpMold.Equals(""))
                                        //    bModlStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID")).Equals(sTmpMold))
                                            ModlidxE = m;
                                        else
                                        {
                                            if (ModlidxS > ModlidxE)
                                            {
                                                ModlidxE = ModlidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                            bModlStrt = true;
                                            sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                            ModlidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bModlStrt = false;
                                        }
                                    }

                                    //PRE_DISPERSION_ID
                                    if (!bDispersion_idStrt)
                                    {
                                        bDispersion_idStrt = true;
                                        sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                        Dispersion_ididxS = m;
                                        //if (sTmpDispersion_id.Equals(""))
                                        //    bDispersion_idStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID")).Equals(sTmpDispersion_id))
                                            Dispersion_ididxE = m;
                                        else
                                        {
                                            if (Dispersion_ididxS > Dispersion_ididxE)
                                            {
                                                Dispersion_ididxE = Dispersion_ididxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                            bDispersion_idStrt = true;
                                            sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                            Dispersion_ididxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDispersion_idStrt = false;
                                        }
                                    }

                                    //PRE_DISPERSION_NAME
                                    if (!bDispersion_nameStrt)
                                    {
                                        bDispersion_nameStrt = true;
                                        sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                        Dispersion_nameidxS = m;
                                        //if (sTmpDispersion_name.Equals(""))
                                        //    bDispersion_nameStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME")).Equals(sTmpDispersion_name))
                                            Dispersion_nameidxE = m;
                                        else
                                        {
                                            if (Dispersion_nameidxS > Dispersion_nameidxE)
                                            {
                                                Dispersion_nameidxE = Dispersion_nameidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                            bDispersion_nameStrt = true;
                                            sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                            Dispersion_nameidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bDispersion_nameStrt = false;
                                        }
                                    }
                                    //ACTIVE_MTRL1
                                    if (!bMtrl1Strt)
                                    {
                                        bMtrl1Strt = true;
                                        sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                        Mtrl1idxS = m;
                                        //if (sTmpMtrl1.Equals(""))
                                        //    bMtrl1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1")).Equals(sTmpMtrl1))
                                            Mtrl1idxE = m;
                                        else
                                        {
                                            if (Mtrl1idxS > Mtrl1idxE)
                                            {
                                                Mtrl1idxE = Mtrl1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                            bMtrl1Strt = true;
                                            sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                            Mtrl1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl1Strt = false;
                                        }
                                    }

                                    //ACTIVE_MTRL2
                                    if (!bMtrl2Strt)
                                    {
                                        bMtrl2Strt = true;
                                        sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                        Mtrl2idxS = m;
                                        //if (sTmpMtrl2.Equals(""))
                                        //    bMtrl2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2")).Equals(sTmpMtrl2))
                                            Mtrl2idxE = m;
                                        else
                                        {
                                            if (Mtrl2idxS > Mtrl2idxE)
                                            {
                                                Mtrl2idxE = Mtrl2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                            bMtrl2Strt = true;
                                            sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                            Mtrl2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl2Strt = false;
                                        }
                                    }
                                    //ACTIVE_MTRL3
                                    if (!bMtrl3Strt)
                                    {
                                        bMtrl3Strt = true;
                                        sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                        Mtrl3idxS = m;
                                        //if (sTmpMtrl3.Equals(""))
                                        //    bMtrl3Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3")).Equals(sTmpMtrl3))
                                            Mtrl3idxE = m;
                                        else
                                        {
                                            if (Mtrl3idxS > Mtrl3idxE)
                                            {
                                                Mtrl3idxE = Mtrl3idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                            bMtrl3Strt = true;
                                            sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                            Mtrl3idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bMtrl3Strt = false;
                                        }
                                    }
                                    //BINDER1
                                    if (!bBinder1Strt)
                                    {
                                        bBinder1Strt = true;
                                        sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                        Binder1idxS = m;
                                        //if (sTmpBinder1.Equals(""))
                                        //    bBinder1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1")).Equals(sTmpBinder1))
                                            Binder1idxE = m;
                                        else
                                        {
                                            if (Binder1idxS > Binder1idxE)
                                            {
                                                Binder1idxE = Binder1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                            bBinder1Strt = true;
                                            sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                            Binder1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder1Strt = false;
                                        }
                                    }

                                    //BINDER2
                                    if (!bBinder2Strt)
                                    {
                                        bBinder2Strt = true;
                                        sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                        Binder2idxS = m;
                                        //if (sTmpBinder2.Equals(""))
                                        //    bBinder2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2")).Equals(sTmpBinder2))
                                            Binder2idxE = m;
                                        else
                                        {
                                            if (Binder2idxS > Binder2idxE)
                                            {
                                                Binder2idxE = Binder2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["BINDER2"].Index)));
                                            bBinder2Strt = true;
                                            sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                            Binder2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder2Strt = false;
                                        }
                                    }
                                    //BINDER3
                                    if (!bBinder3Strt)
                                    {
                                        bBinder3Strt = true;
                                        sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                        Binder3idxS = m;
                                        //if (sTmpBinder3.Equals(""))
                                        //    bBinder3Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3")).Equals(sTmpBinder3))
                                            Binder3idxE = m;
                                        else
                                        {
                                            if (Binder3idxS > Binder3idxE)
                                            {
                                                Binder3idxE = Binder3idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                            bBinder3Strt = true;
                                            sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                            Binder3idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bBinder3Strt = false;
                                        }
                                    }
                                    //CONDUCTION_MTRL1
                                    if (!bConduction1Strt)
                                    {
                                        bConduction1Strt = true;
                                        sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                        Conduction1idxS = m;
                                        //if (sTmpConduction1.Equals(""))
                                        //    bConduction1Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1")).Equals(sTmpConduction1))
                                            Conduction1idxE = m;
                                        else
                                        {
                                            if (Conduction1idxS > Conduction1idxE)
                                            {
                                                Conduction1idxE = Conduction1idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                            bConduction1Strt = true;
                                            sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                            Conduction1idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bConduction1Strt = false;
                                        }
                                    }

                                    //CONDUCTION_MTRL2
                                    if (!bConduction2Strt)
                                    {
                                        bConduction2Strt = true;
                                        sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                        Conduction2idxS = m;
                                        //if (sTmpConduction2.Equals(""))
                                        //    bConduction2Strt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2")).Equals(sTmpConduction2))
                                            Conduction2idxE = m;
                                        else
                                        {
                                            if (Conduction2idxS > Conduction2idxE)
                                            {
                                                Conduction2idxE = Conduction2idxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                            bConduction2Strt = true;
                                            sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                            Conduction2idxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bConduction2Strt = false;
                                        }
                                    }

                                    //CMC
                                    if (!bCmcStrt)
                                    {
                                        bCmcStrt = true;
                                        sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                        CmcidxS = m;
                                        //if (sTmpCmc.Equals(""))
                                        //    bCmcStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC")).Equals(sTmpCmc))
                                            CmcidxE = m;
                                        else
                                        {
                                            if (CmcidxS > CmcidxE)
                                            {
                                                CmcidxE = CmcidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                            bCmcStrt = true;
                                            sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                            CmcidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bCmcStrt = false;
                                        }
                                    }

                                    //FOIL
                                    if (!bFoilStrt)
                                    {
                                        bFoilStrt = true;
                                        sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                        FoilidxS = m;
                                        //if (sTmpFoil.Equals(""))
                                        //    bFoilStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL")).Equals(sTmpFoil))
                                            FoilidxE = m;
                                        else
                                        {
                                            if (FoilidxS > FoilidxE)
                                            {
                                                FoilidxE = FoilidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                            bFoilStrt = true;
                                            sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                            FoilidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bFoilStrt = false;
                                        }
                                    }
                                    //DAILY_CAPA_PPM
                                    if (!bCapaStrt)
                                    {
                                        bCapaStrt = true;
                                        sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                        CapaidxS = m;
                                        //if (sTmpCapa.Equals(""))
                                        //    bCapaStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM")).Equals(sTmpCapa))
                                            CapaidxE = m;
                                        else
                                        {
                                            if (CapaidxS > CapaidxE)
                                            {
                                                CapaidxE = CapaidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                            bCapaStrt = true;
                                            sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                            CapaidxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bCapaStrt = false;
                                        }
                                    }
                                    //PROD_VER_CODE
                                    if (!bVerStrt)
                                    {
                                        bVerStrt = true;
                                        sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                        VeridxS = m;
                                        //if (sTmpVer.Equals(""))
                                        //    bVerStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE")).Equals(sTmpVer))
                                            VeriddxE = m;
                                        else
                                        {
                                            if (VeridxS > VeriddxE)
                                            {
                                                VeriddxE = VeridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                            bVerStrt = true;
                                            sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                            VeridxS = m;
                                            //if (sTmpProc.Equals(""))
                                            //    bVerStrt = false;
                                        }
                                    }
                                    //CATEGORY_DISP_NAME
                                    if (!bCategoryStrt)
                                    {
                                        bCategoryStrt = true;
                                        sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                        CategoryidxS = m;
                                        //if (sTmpCategory.Equals(""))
                                        //    bCategoryStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME")).Equals(sTmpCategory))
                                            CategoryidxE = m;
                                        else
                                        {
                                            if (CategoryidxS > CategoryidxE)
                                            {
                                                CategoryidxE = CategoryidxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                            bCategoryStrt = true;
                                            sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                            CategoryidxS = m;
                                            //if (sTmpCategory.Equals(""))
                                            //    bCategoryStrt = false;
                                        }
                                    }
                                    //MEASR_NAME
                                    if (!bMeasrStrt)
                                    {
                                        bMeasrStrt = true;
                                        sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                        MeasridxS = m;
                                        //if (sTmpMeasr.Equals(""))
                                        //    bMeasrStrt = false;
                                    }
                                    else
                                    {
                                        if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME")).Equals(sTmpMeasr))
                                            MeasridxE = m;
                                        else
                                        {
                                            if (MeasridxS > MeasridxE)
                                            {
                                                MeasridxE = MeasridxS;
                                            }
                                            e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                            bMeasrStrt = true;
                                            sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                            MeasridxS = m;
                                            //if (sTmpMeasr.Equals(""))
                                            //    bMeasrStrt = false;
                                        }
                                    }
                                }
                                //AREANAME
                                if (bAreaStrt)
                                {
                                    if (AreaidxS > AreaidxE)
                                    {
                                        AreaidxE = AreaidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                    bAreaStrt = false;
                                    sTmpArea = string.Empty;
                                    AreaidxE = 0;
                                    AreaidxS = 0;

                                }
                                //PROCNAME
                                if (bProcStrt)
                                {
                                    if (ProcidxS > ProcidxE)
                                    {
                                        ProcidxE = ProcidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                    ProcidxS = 0;
                                    ProcidxE = 0;
                                    bProcStrt = false;
                                    sTmpProc = string.Empty;
                                }
                                //EQSGNAME
                                if (bEqsgStrt)
                                {
                                    if (EqsgidxS > EqsgidxE)
                                    {
                                        EqsgidxE = EqsgidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                    EqsgidxS = 0;
                                    EqsgidxE = 0;
                                    bEqsgStrt = false;
                                    sTmpEqsg = string.Empty;
                                }
                                //EQPTNAME
                                if (bEqptStrt)
                                {
                                    if (EqptidxS > EqptidxE)
                                    {
                                        EqptidxE = EqptidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                    EqptidxS = 0;
                                    EqptidxE = 0;
                                    bEqptStrt = false;
                                    sTmpEqpt = string.Empty;
                                }
                                //PRODID
                                if (bProdStrt)
                                {
                                    if (ProdidxS > ProdidxE)
                                    {
                                        ProdidxE = ProdidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                    ProdidxS = 0;
                                    ProdidxE = 0;
                                    bProdStrt = false;
                                    sTmpProd = string.Empty;
                                }
                                //PRJT_NAME
                                if (bPrjtStrt)
                                {
                                    if (PrjtidxS > PrjtidxE)
                                    {
                                        PrjtidxE = PrjtidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                    PrjtidxS = 0;
                                    PrjtidxE = 0;
                                    bPrjtStrt = false;
                                    sTmpPrjt = string.Empty;
                                }
                                //DEMAND_TYPE
                                if (bDemandStrt)
                                {
                                    if (DemandidxS > DemandidxE)
                                    {
                                        DemandidxE = DemandidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                    DemandidxS = 0;
                                    DemandidxE = 0;
                                    bDemandStrt = false;
                                    sTmpDemand = string.Empty;
                                }
                                //MKT_TYPE_NAME
                                if (bMktStrt)
                                {
                                    if (MktidxS > MktidxE)
                                    {
                                        MktidxE = MktidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                    MktidxS = 0;
                                    MktidxE = 0;
                                    bMktStrt = false;
                                    sTmpMkt = string.Empty;
                                }

                                //ELTR_TYPE
                                if (bEltrStrt)
                                {
                                    if (EltridxS > EltridxE)
                                    {
                                        EltridxE = EltridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                    EltridxS = 0;
                                    EltridxE = 0;
                                    bEltrStrt = false;
                                    sTmpEltr = string.Empty;
                                }

                                //MODLID
                                if (bModlStrt)
                                {
                                    if (ModlidxS > ModlidxE)
                                    {
                                        ModlidxE = ModlidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                    ModlidxS = 0;
                                    ModlidxE = 0;
                                    bModlStrt = false;
                                    sTmpMold = string.Empty;
                                }
                                //PRE_DISPERSION_ID
                                if (bDispersion_idStrt)
                                {
                                    if (Dispersion_ididxS > Dispersion_ididxE)
                                    {
                                        Dispersion_ididxE = Dispersion_ididxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                    Dispersion_ididxS = 0;
                                    Dispersion_ididxE = 0;
                                    bDispersion_idStrt = false;
                                    sTmpDispersion_id = string.Empty;
                                }
                                //PRE_DISPERSION_NAME
                                if (bDispersion_nameStrt)
                                {
                                    if (Dispersion_nameidxS > Dispersion_nameidxE)
                                    {
                                        Dispersion_nameidxE = Dispersion_nameidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                    Dispersion_nameidxS = 0;
                                    Dispersion_nameidxE = 0;
                                    bDispersion_nameStrt = false;
                                    sTmpDispersion_name = string.Empty;
                                }
                                //ACTIVE_MTRL1
                                if (bMtrl1Strt)
                                {
                                    if (Mtrl1idxS > Mtrl1idxE)
                                    {
                                        Mtrl1idxE = Mtrl1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                    Mtrl1idxS = 0;
                                    Mtrl1idxE = 0;
                                    bMtrl1Strt = false;
                                    sTmpMtrl1 = string.Empty;
                                }
                                //ACTIVE_MTRL2
                                if (bMtrl2Strt)
                                {
                                    if (Mtrl2idxS > Mtrl2idxE)
                                    {
                                        Mtrl2idxE = Mtrl2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                    Mtrl2idxS = 0;
                                    Mtrl2idxE = 0;
                                    bMtrl2Strt = false;
                                    sTmpMtrl2 = string.Empty;
                                }
                                //ACTIVE_MTRL3
                                if (bMtrl3Strt)
                                {
                                    if (Mtrl3idxS > Mtrl3idxE)
                                    {
                                        Mtrl3idxE = Mtrl3idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                    Mtrl3idxS = 0;
                                    Mtrl3idxE = 0;
                                    bMtrl3Strt = false;
                                    sTmpMtrl3 = string.Empty;
                                }
                                // BINDER1
                                if (bBinder1Strt)
                                {
                                    if (Binder1idxS > Binder1idxE)
                                    {
                                        Binder1idxE = Binder1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                    Binder1idxS = 0;
                                    Binder1idxE = 0;
                                    bBinder1Strt = false;
                                    sTmpBinder1 = string.Empty;
                                }
                                //BINDER2
                                if (bBinder2Strt)
                                {
                                    if (Binder2idxS > Binder2idxE)
                                    {
                                        Binder2idxE = Binder2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Binder2idxE, dgList.Columns["BINDER2"].Index)));
                                    Binder2idxS = 0;
                                    Binder2idxE = 0;
                                    bBinder2Strt = false;
                                    sTmpBinder2 = string.Empty;
                                }
                                // BINDER3
                                if (bBinder3Strt)
                                {
                                    if (Binder3idxS > Binder3idxE)
                                    {
                                        Binder3idxE = Binder3idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                    Binder3idxS = 0;
                                    Binder3idxE = 0;
                                    bBinder3Strt = false;
                                    sTmpBinder3 = string.Empty;
                                }
                                //CONDUCTION_MTRL1
                                if (bConduction1Strt)
                                {
                                    if (Conduction1idxS > Conduction1idxE)
                                    {
                                        Conduction1idxE = Conduction1idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                    Conduction1idxS = 0;
                                    Conduction1idxE = 0;
                                    bConduction1Strt = false;
                                    sTmpConduction1 = string.Empty;
                                }
                                //CONDUCTION_MTRL2
                                if (bConduction2Strt)
                                {
                                    if (Conduction2idxS > Conduction2idxE)
                                    {
                                        Conduction2idxE = Conduction2idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                    Conduction2idxS = 0;
                                    Conduction2idxE = 0;
                                    bConduction2Strt = false;
                                    sTmpConduction2 = string.Empty;
                                }
                                //CMC
                                if (bCmcStrt)
                                {
                                    if (CmcidxS > CmcidxE)
                                    {
                                        CmcidxE = CmcidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                    CmcidxS = 0;
                                    CmcidxE = 0;
                                    bCmcStrt = false;
                                    sTmpCmc = string.Empty;
                                }
                                //FOIL
                                if (bFoilStrt)
                                {
                                    if (FoilidxS > FoilidxE)
                                    {
                                        FoilidxE = FoilidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                    FoilidxS = 0;
                                    FoilidxE = 0;
                                    bFoilStrt = false;
                                    sTmpFoil = string.Empty;
                                }
                                //DAILY_CAPA_PPM
                                if (bCapaStrt)
                                {
                                    if (CapaidxS > CapaidxE)
                                    {
                                        CapaidxE = CapaidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                    CapaidxS = 0;
                                    CapaidxE = 0;
                                    bCapaStrt = false;
                                    sTmpCapa = string.Empty;
                                }
                                //PROD_VER_CODE
                                if (bVerStrt)
                                {
                                    if (VeridxS > VeriddxE)
                                    {
                                        VeriddxE = VeridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                    VeridxS = 0;
                                    VeriddxE = 0;
                                    bVerStrt = false;
                                    sTmpVer = string.Empty;
                                }
                                //CATEGORY_DISP_NAME
                                if (bCategoryStrt)
                                {
                                    if (CategoryidxS > CategoryidxE)
                                    {
                                        CategoryidxE = CategoryidxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                    CategoryidxS = 0;
                                    CategoryidxE = 0;
                                    bCategoryStrt = false;
                                    sTmpCategory = string.Empty;
                                }
                                //MEASR
                                if (bMeasrStrt)
                                {
                                    if (MeasridxS > MeasridxE)
                                    {
                                        MeasridxE = MeasridxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                    bMeasrStrt = false;
                                    MeasridxS = 0;
                                    MeasridxE = dgList.TopRows.Count;
                                    sTmpMeasr = string.Empty;
                                }
                                #endregion

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PLAN_GRP_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            #region 기준 컬럼(PLAN_GRP_ID)에 대한 각 컬럼 머지 
                            for (int m = idxS; m <= idxE; m++)
                            {
                                //AreaName
                                if (!bAreaStrt)
                                {
                                    bAreaStrt = true;
                                    sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                    AreaidxS = m;
                                    //if (sTmpArea.Equals(""))
                                    //    bAreaStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME")).Equals(sTmpArea))
                                        AreaidxE = m;
                                    else
                                    {
                                        if (AreaidxS > AreaidxE)
                                        {
                                            AreaidxE = AreaidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                        bAreaStrt = true;
                                        sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                        AreaidxS = m;
                                        //if (sTmpArea.Equals(""))
                                        //    bAreaStrt = false;
                                    }
                                }
                                //PROCNAME
                                if (!bProcStrt)
                                {
                                    bProcStrt = true;
                                    sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                    ProcidxS = m;
                                    //if (sTmpProc.Equals(""))
                                    //    bProcStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME")).Equals(sTmpProc))
                                        ProcidxE = m;
                                    else
                                    {
                                        if (ProcidxS > ProcidxE)
                                        {
                                            ProcidxE = ProcidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                        bProcStrt = true;
                                        sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                        ProcidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bProcStrt = false;
                                    }
                                }

                                //EQSGNAME
                                if (!bEqsgStrt)
                                {
                                    bEqsgStrt = true;
                                    sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                    EqsgidxS = m;
                                    //if (sTmpEqsg.Equals(""))
                                    //    bEqsgStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME")).Equals(sTmpEqsg))
                                        EqsgidxE = m;
                                    else
                                    {
                                        if (EqsgidxS > EqsgidxE)
                                        {
                                            EqsgidxE = EqsgidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                        bEqsgStrt = true;
                                        sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                        EqsgidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bEqsgStrt = false;
                                    }
                                }
                                //EQPTNAME
                                if (!bEqptStrt)
                                {
                                    bEqptStrt = true;
                                    sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                    EqptidxS = m;
                                    //if (sTmpEqpt.Equals(""))
                                    //    bEqptStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME")).Equals(sTmpEqpt))
                                        EqptidxE = m;
                                    else
                                    {
                                        if (EqptidxS > EqptidxE)
                                        {
                                            EqptidxE = EqptidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                        bEqptStrt = true;
                                        sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                        EqptidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bEqptStrt = false;
                                    }
                                }

                                //PRODID
                                if (!bProdStrt)
                                {
                                    bProdStrt = true;
                                    sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                    ProdidxS = m;
                                    //if (sTmpProd.Equals(""))
                                    //    bProdStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID")).Equals(sTmpProd))
                                        ProdidxE = m;
                                    else
                                    {
                                        if (ProdidxS > ProdidxE)
                                        {
                                            ProdidxE = ProdidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                        bProdStrt = true;
                                        sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                        ProdidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bProdStrt = false;
                                    }
                                }

                                //PRJT_NAME
                                if (!bPrjtStrt)
                                {
                                    bPrjtStrt = true;
                                    sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                    PrjtidxS = m;
                                    //if (sTmpPrjt.Equals(""))
                                    //    bPrjtStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME")).Equals(sTmpPrjt))
                                        PrjtidxE = m;
                                    else
                                    {
                                        if (PrjtidxS > PrjtidxE)
                                        {
                                            PrjtidxE = PrjtidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                        bPrjtStrt = true;
                                        sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                        PrjtidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bPrjtStrt = false;
                                    }
                                }

                                //DEMAND_TYPE
                                if (!bDemandStrt)
                                {
                                    bDemandStrt = true;
                                    sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                    DemandidxS = m;
                                    //if (sTmpDemand.Equals(""))
                                    //    bDemandStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE")).Equals(sTmpDemand))
                                        DemandidxE = m;
                                    else
                                    {
                                        if (DemandidxS > DemandidxE)
                                        {
                                            DemandidxE = DemandidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                        bDemandStrt = true;
                                        sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                        DemandidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bDemandStrt = false;
                                    }
                                }
                                //MKT_TYPE_NAME
                                if (!bMktStrt)
                                {
                                    bMktStrt = true;
                                    sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                    MktidxS = m;
                                    //if (sTmpMkt.Equals(""))
                                    //    bMktStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME")).Equals(sTmpMkt))
                                        MktidxE = m;
                                    else
                                    {
                                        if (MktidxS > MktidxE)
                                        {
                                            MktidxE = MktidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                        bMktStrt = true;
                                        sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                        MktidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bMktStrt = false;
                                    }
                                }
                                //ELTR_TYPE
                                if (!bEltrStrt)
                                {
                                    bEltrStrt = true;
                                    sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                    EltridxS = m;
                                    //if (sTmpEltr.Equals(""))
                                    //    bEltrStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE")).Equals(sTmpEltr))
                                        EltridxE = m;
                                    else
                                    {
                                        if (EltridxS > EltridxE)
                                        {
                                            EltridxE = EltridxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                        bEltrStrt = true;
                                        sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                        EltridxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bEltrStrt = false;
                                    }
                                }

                                //MODLID
                                if (!bModlStrt)
                                {
                                    bModlStrt = true;
                                    sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                    ModlidxS = m;
                                    //if (sTmpMold.Equals(""))
                                    //    bModlStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID")).Equals(sTmpMold))
                                        ModlidxE = m;
                                    else
                                    {
                                        if (ModlidxS > ModlidxE)
                                        {
                                            ModlidxE = ModlidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                        bModlStrt = true;
                                        sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                        ModlidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bModlStrt = false;
                                    }
                                }

                                //PRE_DISPERSION_ID
                                if (!bDispersion_idStrt)
                                {
                                    bDispersion_idStrt = true;
                                    sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                    Dispersion_ididxS = m;
                                    //if (sTmpDispersion_id.Equals(""))
                                    //    bDispersion_idStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID")).Equals(sTmpDispersion_id))
                                        Dispersion_ididxE = m;
                                    else
                                    {
                                        if (Dispersion_ididxS > Dispersion_ididxE)
                                        {
                                            Dispersion_ididxE = Dispersion_ididxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                        bDispersion_idStrt = true;
                                        sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                        Dispersion_ididxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bDispersion_idStrt = false;
                                    }
                                }

                                //PRE_DISPERSION_NAME
                                if (!bDispersion_nameStrt)
                                {
                                    bDispersion_nameStrt = true;
                                    sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                    Dispersion_nameidxS = m;
                                    //if (sTmpDispersion_name.Equals(""))
                                    //    bDispersion_nameStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME")).Equals(sTmpDispersion_name))
                                        Dispersion_nameidxE = m;
                                    else
                                    {
                                        if (Dispersion_nameidxS > Dispersion_nameidxE)
                                        {
                                            Dispersion_nameidxE = Dispersion_nameidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                        bDispersion_nameStrt = true;
                                        sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                        Dispersion_nameidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bDispersion_nameStrt = false;
                                    }
                                }
                                //ACTIVE_MTRL1
                                if (!bMtrl1Strt)
                                {
                                    bMtrl1Strt = true;
                                    sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                    Mtrl1idxS = m;
                                    //if (sTmpMtrl1.Equals(""))
                                    //    bMtrl1Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1")).Equals(sTmpMtrl1))
                                        Mtrl1idxE = m;
                                    else
                                    {
                                        if (Mtrl1idxS > Mtrl1idxE)
                                        {
                                            Mtrl1idxE = Mtrl1idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                        bMtrl1Strt = true;
                                        sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                        Mtrl1idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bMtrl1Strt = false;
                                    }
                                }

                                //ACTIVE_MTRL2
                                if (!bMtrl2Strt)
                                {
                                    bMtrl2Strt = true;
                                    sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                    Mtrl2idxS = m;
                                    //if (sTmpMtrl2.Equals(""))
                                    //    bMtrl2Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2")).Equals(sTmpMtrl2))
                                        Mtrl2idxE = m;
                                    else
                                    {
                                        if (Mtrl2idxS > Mtrl2idxE)
                                        {
                                            Mtrl2idxE = Mtrl2idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                        bMtrl2Strt = true;
                                        sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                        Mtrl2idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bMtrl2Strt = false;
                                    }
                                }
                                //ACTIVE_MTRL3
                                if (!bMtrl3Strt)
                                {
                                    bMtrl3Strt = true;
                                    sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                    Mtrl3idxS = m;
                                    //if (sTmpMtrl3.Equals(""))
                                    //    bMtrl3Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3")).Equals(sTmpMtrl3))
                                        Mtrl3idxE = m;
                                    else
                                    {
                                        if (Mtrl3idxS > Mtrl3idxE)
                                        {
                                            Mtrl3idxE = Mtrl3idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                        bMtrl3Strt = true;
                                        sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                        Mtrl3idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bMtrl3Strt = false;
                                    }
                                }
                                //BINDER1
                                if (!bBinder1Strt)
                                {
                                    bBinder1Strt = true;
                                    sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                    Binder1idxS = m;
                                    //if (sTmpBinder1.Equals(""))
                                    //    bBinder1Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1")).Equals(sTmpBinder1))
                                        Binder1idxE = m;
                                    else
                                    {
                                        if (Binder1idxS > Binder1idxE)
                                        {
                                            Binder1idxE = Binder1idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                        bBinder1Strt = true;
                                        sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                        Binder1idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bBinder1Strt = false;
                                    }
                                }

                                //BINDER2
                                if (!bBinder2Strt)
                                {
                                    bBinder2Strt = true;
                                    sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                    Binder2idxS = m;
                                    //if (sTmpBinder2.Equals(""))
                                    //    bBinder2Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2")).Equals(sTmpBinder2))
                                        Binder2idxE = m;
                                    else
                                    {
                                        if (Binder2idxS > Binder2idxE)
                                        {
                                            Binder2idxE = Binder2idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["BINDER2"].Index)));
                                        bBinder2Strt = true;
                                        sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                        Binder2idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bBinder2Strt = false;
                                    }
                                }
                                //BINDER3
                                if (!bBinder3Strt)
                                {
                                    bBinder3Strt = true;
                                    sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                    Binder3idxS = m;
                                    //if (sTmpBinder3.Equals(""))
                                    //    bBinder3Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3")).Equals(sTmpBinder3))
                                        Binder3idxE = m;
                                    else
                                    {
                                        if (Binder3idxS > Binder3idxE)
                                        {
                                            Binder3idxE = Binder3idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                        bBinder3Strt = true;
                                        sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                        Binder3idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bBinder3Strt = false;
                                    }
                                }
                                //CONDUCTION_MTRL1
                                if (!bConduction1Strt)
                                {
                                    bConduction1Strt = true;
                                    sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                    Conduction1idxS = m;
                                    //if (sTmpConduction1.Equals(""))
                                    //    bConduction1Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1")).Equals(sTmpConduction1))
                                        Conduction1idxE = m;
                                    else
                                    {
                                        if (Conduction1idxS > Conduction1idxE)
                                        {
                                            Conduction1idxE = Conduction1idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                        bConduction1Strt = true;
                                        sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                        Conduction1idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bConduction1Strt = false;
                                    }
                                }

                                //CONDUCTION_MTRL2
                                if (!bConduction2Strt)
                                {
                                    bConduction2Strt = true;
                                    sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                    Conduction2idxS = m;
                                    //if (sTmpConduction2.Equals(""))
                                    //    bConduction2Strt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2")).Equals(sTmpConduction2))
                                        Conduction2idxE = m;
                                    else
                                    {
                                        if (Conduction2idxS > Conduction2idxE)
                                        {
                                            Conduction2idxE = Conduction2idxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                        bConduction2Strt = true;
                                        sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                        Conduction2idxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bConduction2Strt = false;
                                    }
                                }

                                //CMC
                                if (!bCmcStrt)
                                {
                                    bCmcStrt = true;
                                    sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                    CmcidxS = m;
                                    //if (sTmpCmc.Equals(""))
                                    //    bCmcStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC")).Equals(sTmpCmc))
                                        CmcidxE = m;
                                    else
                                    {
                                        if (CmcidxS > CmcidxE)
                                        {
                                            CmcidxE = CmcidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                        bCmcStrt = true;
                                        sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                        CmcidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bCmcStrt = false;
                                    }
                                }

                                //FOIL
                                if (!bFoilStrt)
                                {
                                    bFoilStrt = true;
                                    sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                    FoilidxS = m;
                                    //if (sTmpFoil.Equals(""))
                                    //    bFoilStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL")).Equals(sTmpFoil))
                                        FoilidxE = m;
                                    else
                                    {
                                        if (FoilidxS > FoilidxE)
                                        {
                                            FoilidxE = FoilidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                        bFoilStrt = true;
                                        sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                        FoilidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bFoilStrt = false;
                                    }
                                }
                                //DAILY_CAPA_PPM
                                if (!bCapaStrt)
                                {
                                    bCapaStrt = true;
                                    sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                    CapaidxS = m;
                                    //if (sTmpCapa.Equals(""))
                                    //    bCapaStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM")).Equals(sTmpCapa))
                                        CapaidxE = m;
                                    else
                                    {
                                        if (CapaidxS > CapaidxE)
                                        {
                                            CapaidxE = CapaidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                        bCapaStrt = true;
                                        sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                        CapaidxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bCapaStrt = false;
                                    }
                                }
                                //PROD_VER_CODE
                                if (!bVerStrt)
                                {
                                    bVerStrt = true;
                                    sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                    VeridxS = m;
                                    //if (sTmpVer.Equals(""))
                                    //    bVerStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE")).Equals(sTmpVer))
                                        VeriddxE = m;
                                    else
                                    {
                                        if (VeridxS > VeriddxE)
                                        {
                                            VeriddxE = VeridxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                        bVerStrt = true;
                                        sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                        VeridxS = m;
                                        //if (sTmpProc.Equals(""))
                                        //    bVerStrt = false;
                                    }
                                }
                                //CATEGORY_DISP_NAME
                                if (!bCategoryStrt)
                                {
                                    bCategoryStrt = true;
                                    sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                    CategoryidxS = m;
                                    //if (sTmpCategory.Equals(""))
                                    //    bCategoryStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME")).Equals(sTmpCategory))
                                        CategoryidxE = m;
                                    else
                                    {
                                        if (CategoryidxS > CategoryidxE)
                                        {
                                            CategoryidxE = CategoryidxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                        bCategoryStrt = true;
                                        sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                        CategoryidxS = m;
                                        //if (sTmpCategory.Equals(""))
                                        //    bCategoryStrt = false;
                                    }
                                }
                                //MEASR_NAME
                                if (!bMeasrStrt)
                                {
                                    bMeasrStrt = true;
                                    sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                    MeasridxS = m;
                                    //if (sTmpMeasr.Equals(""))
                                    //    bMeasrStrt = false;
                                }
                                else
                                {
                                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME")).Equals(sTmpMeasr))
                                        MeasridxE = m;
                                    else
                                    {
                                        if (MeasridxS > MeasridxE)
                                        {
                                            MeasridxE = MeasridxS;
                                        }
                                        e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                        bMeasrStrt = true;
                                        sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                        MeasridxS = m;
                                        //if (sTmpMeasr.Equals(""))
                                        //    bMeasrStrt = false;
                                    }
                                }
                            }
                            //AREANAME
                            if (bAreaStrt)
                            {
                                if (AreaidxS > AreaidxE)
                                {
                                    AreaidxE = AreaidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                bAreaStrt = false;
                                sTmpArea = string.Empty;
                                AreaidxE = 0;
                                AreaidxS = 0;

                            }
                            //PROCNAME
                            if (bProcStrt)
                            {
                                if (ProcidxS > ProcidxE)
                                {
                                    ProcidxE = ProcidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                ProcidxS = 0;
                                ProcidxE = 0;
                                bProcStrt = false;
                                sTmpProc = string.Empty;
                            }
                            //EQSGNAME
                            if (bEqsgStrt)
                            {
                                if (EqsgidxS > EqsgidxE)
                                {
                                    EqsgidxE = EqsgidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                EqsgidxS = 0;
                                EqsgidxE = 0;
                                bEqsgStrt = false;
                                sTmpEqsg = string.Empty;
                            }
                            //EQPTNAME
                            if (bEqptStrt)
                            {
                                if (EqptidxS > EqptidxE)
                                {
                                    EqptidxE = EqptidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                EqptidxS = 0;
                                EqptidxE = 0;
                                bEqptStrt = false;
                                sTmpEqpt = string.Empty;
                            }
                            //PRODID
                            if (bProdStrt)
                            {
                                if (ProdidxS > ProdidxE)
                                {
                                    ProdidxE = ProdidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                ProdidxS = 0;
                                ProdidxE = 0;
                                bProdStrt = false;
                                sTmpProd = string.Empty;
                            }
                            //PRJT_NAME
                            if (bPrjtStrt)
                            {
                                if (PrjtidxS > PrjtidxE)
                                {
                                    PrjtidxE = PrjtidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                PrjtidxS = 0;
                                PrjtidxE = 0;
                                bPrjtStrt = false;
                                sTmpPrjt = string.Empty;
                            }
                            //DEMAND_TYPE
                            if (bDemandStrt)
                            {
                                if (DemandidxS > DemandidxE)
                                {
                                    DemandidxE = DemandidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                DemandidxS = 0;
                                DemandidxE = 0;
                                bDemandStrt = false;
                                sTmpDemand = string.Empty;
                            }
                            //MKT_TYPE_NAME
                            if (bMktStrt)
                            {
                                if (MktidxS > MktidxE)
                                {
                                    MktidxE = MktidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                MktidxS = 0;
                                MktidxE = 0;
                                bMktStrt = false;
                                sTmpMkt = string.Empty;
                            }

                            //ELTR_TYPE
                            if (bEltrStrt)
                            {
                                if (EltridxS > EltridxE)
                                {
                                    EltridxE = EltridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                EltridxS = 0;
                                EltridxE = 0;
                                bEltrStrt = false;
                                sTmpEltr = string.Empty;
                            }

                            //MODLID
                            if (bModlStrt)
                            {
                                if (ModlidxS > ModlidxE)
                                {
                                    ModlidxE = ModlidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                ModlidxS = 0;
                                ModlidxE = 0;
                                bModlStrt = false;
                                sTmpMold = string.Empty;
                            }
                            //PRE_DISPERSION_ID
                            if (bDispersion_idStrt)
                            {
                                if (Dispersion_ididxS > Dispersion_ididxE)
                                {
                                    Dispersion_ididxE = Dispersion_ididxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                Dispersion_ididxS = 0;
                                Dispersion_ididxE = 0;
                                bDispersion_idStrt = false;
                                sTmpDispersion_id = string.Empty;
                            }
                            //PRE_DISPERSION_NAME
                            if (bDispersion_nameStrt)
                            {
                                if (Dispersion_nameidxS > Dispersion_nameidxE)
                                {
                                    Dispersion_nameidxE = Dispersion_nameidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                Dispersion_nameidxS = 0;
                                Dispersion_nameidxE = 0;
                                bDispersion_nameStrt = false;
                                sTmpDispersion_name = string.Empty;
                            }
                            //ACTIVE_MTRL1
                            if (bMtrl1Strt)
                            {
                                if (Mtrl1idxS > Mtrl1idxE)
                                {
                                    Mtrl1idxE = Mtrl1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                Mtrl1idxS = 0;
                                Mtrl1idxE = 0;
                                bMtrl1Strt = false;
                                sTmpMtrl1 = string.Empty;
                            }
                            //ACTIVE_MTRL2
                            if (bMtrl2Strt)
                            {
                                if (Mtrl2idxS > Mtrl2idxE)
                                {
                                    Mtrl2idxE = Mtrl2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                Mtrl2idxS = 0;
                                Mtrl2idxE = 0;
                                bMtrl2Strt = false;
                                sTmpMtrl2 = string.Empty;
                            }
                            //ACTIVE_MTRL3
                            if (bMtrl3Strt)
                            {
                                if (Mtrl3idxS > Mtrl3idxE)
                                {
                                    Mtrl3idxE = Mtrl3idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                Mtrl3idxS = 0;
                                Mtrl3idxE = 0;
                                bMtrl3Strt = false;
                                sTmpMtrl3 = string.Empty;
                            }
                            // BINDER1
                            if (bBinder1Strt)
                            {
                                if (Binder1idxS > Binder1idxE)
                                {
                                    Binder1idxE = Binder1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                Binder1idxS = 0;
                                Binder1idxE = 0;
                                bBinder1Strt = false;
                                sTmpBinder1 = string.Empty;
                            }
                            //BINDER2
                            if (bBinder2Strt)
                            {
                                if (Binder2idxS > Binder2idxE)
                                {
                                    Binder2idxE = Binder2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Binder2idxE, dgList.Columns["BINDER2"].Index)));
                                Binder2idxS = 0;
                                Binder2idxE = 0;
                                bBinder2Strt = false;
                                sTmpBinder2 = string.Empty;
                            }
                            // BINDER3
                            if (bBinder3Strt)
                            {
                                if (Binder3idxS > Binder3idxE)
                                {
                                    Binder3idxE = Binder3idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                Binder3idxS = 0;
                                Binder3idxE = 0;
                                bBinder3Strt = false;
                                sTmpBinder3 = string.Empty;
                            }
                            //CONDUCTION_MTRL1
                            if (bConduction1Strt)
                            {
                                if (Conduction1idxS > Conduction1idxE)
                                {
                                    Conduction1idxE = Conduction1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                Conduction1idxS = 0;
                                Conduction1idxE = 0;
                                bConduction1Strt = false;
                                sTmpConduction1 = string.Empty;
                            }
                            //CONDUCTION_MTRL2
                            if (bConduction2Strt)
                            {
                                if (Conduction2idxS > Conduction2idxE)
                                {
                                    Conduction2idxE = Conduction2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                Conduction2idxS = 0;
                                Conduction2idxE = 0;
                                bConduction2Strt = false;
                                sTmpConduction2 = string.Empty;
                            }
                            //CMC
                            if (bCmcStrt)
                            {
                                if (CmcidxS > CmcidxE)
                                {
                                    CmcidxE = CmcidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                CmcidxS = 0;
                                CmcidxE = 0;
                                bCmcStrt = false;
                                sTmpCmc = string.Empty;
                            }
                            //FOIL
                            if (bFoilStrt)
                            {
                                if (FoilidxS > FoilidxE)
                                {
                                    FoilidxE = FoilidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                FoilidxS = 0;
                                FoilidxE = 0;
                                bFoilStrt = false;
                                sTmpFoil = string.Empty;
                            }
                            //DAILY_CAPA_PPM
                            if (bCapaStrt)
                            {
                                if (CapaidxS > CapaidxE)
                                {
                                    CapaidxE = CapaidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                CapaidxS = 0;
                                CapaidxE = 0;
                                bCapaStrt = false;
                                sTmpCapa = string.Empty;
                            }
                            //PROD_VER_CODE
                            if (bVerStrt)
                            {
                                if (VeridxS > VeriddxE)
                                {
                                    VeriddxE = VeridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                VeridxS = 0;
                                VeriddxE = 0;
                                bVerStrt = false;
                                sTmpVer = string.Empty;
                            }
                            //CATEGORY_DISP_NAME
                            if (bCategoryStrt)
                            {
                                if (CategoryidxS > CategoryidxE)
                                {
                                    CategoryidxE = CategoryidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                CategoryidxS = 0;
                                CategoryidxE = 0;
                                bCategoryStrt = false;
                                sTmpCategory = string.Empty;
                            }
                            //MEASR
                            if (bMeasrStrt)
                            {
                                if (MeasridxS > MeasridxE)
                                {
                                    MeasridxE = MeasridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                bMeasrStrt = false;
                                MeasridxS = 0;
                                MeasridxE = dgList.TopRows.Count;
                                sTmpMeasr = string.Empty;
                            }
                            #endregion

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PLAN_GRP_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    #region 기준 컬럼(PLAN_GRP_ID)에 대한 각 컬럼 머지 
                    for (int m = idxS; m <= idxE; m++)
                    {
                        //AreaName
                        if (!bAreaStrt)
                        {
                            bAreaStrt = true;
                            sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                            AreaidxS = m;
                            //if (sTmpArea.Equals(""))
                            //    bAreaStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME")).Equals(sTmpArea))
                                AreaidxE = m;
                            else
                            {
                                if (AreaidxS > AreaidxE)
                                {
                                    AreaidxE = AreaidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                                bAreaStrt = true;
                                sTmpArea = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "AREANAME"));
                                AreaidxS = m;
                                //if (sTmpArea.Equals(""))
                                //    bAreaStrt = false;
                            }
                        }
                        //PROCNAME
                        if (!bProcStrt)
                        {
                            bProcStrt = true;
                            sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                            ProcidxS = m;
                            //if (sTmpProc.Equals(""))
                            //    bProcStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME")).Equals(sTmpProc))
                                ProcidxE = m;
                            else
                            {
                                if (ProcidxS > ProcidxE)
                                {
                                    ProcidxE = ProcidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                                bProcStrt = true;
                                sTmpProc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROCNAME"));
                                ProcidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bProcStrt = false;
                            }
                        }

                        //EQSGNAME
                        if (!bEqsgStrt)
                        {
                            bEqsgStrt = true;
                            sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                            EqsgidxS = m;
                            //if (sTmpEqsg.Equals(""))
                            //    bEqsgStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME")).Equals(sTmpEqsg))
                                EqsgidxE = m;
                            else
                            {
                                if (EqsgidxS > EqsgidxE)
                                {
                                    EqsgidxE = EqsgidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                                bEqsgStrt = true;
                                sTmpEqsg = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQSGNAME"));
                                EqsgidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bEqsgStrt = false;
                            }
                        }
                        //EQPTNAME
                        if (!bEqptStrt)
                        {
                            bEqptStrt = true;
                            sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                            EqptidxS = m;
                            //if (sTmpEqpt.Equals(""))
                            //    bEqptStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME")).Equals(sTmpEqpt))
                                EqptidxE = m;
                            else
                            {
                                if (EqptidxS > EqptidxE)
                                {
                                    EqptidxE = EqptidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                                bEqptStrt = true;
                                sTmpEqpt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "EQPTNAME"));
                                EqptidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bEqptStrt = false;
                            }
                        }

                        //PRODID
                        if (!bProdStrt)
                        {
                            bProdStrt = true;
                            sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                            ProdidxS = m;
                            //if (sTmpProd.Equals(""))
                            //    bProdStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID")).Equals(sTmpProd))
                                ProdidxE = m;
                            else
                            {
                                if (ProdidxS > ProdidxE)
                                {
                                    ProdidxE = ProdidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                                bProdStrt = true;
                                sTmpProd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRODID"));
                                ProdidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bProdStrt = false;
                            }
                        }

                        //PRJT_NAME
                        if (!bPrjtStrt)
                        {
                            bPrjtStrt = true;
                            sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                            PrjtidxS = m;
                            //if (sTmpPrjt.Equals(""))
                            //    bPrjtStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME")).Equals(sTmpPrjt))
                                PrjtidxE = m;
                            else
                            {
                                if (PrjtidxS > PrjtidxE)
                                {
                                    PrjtidxE = PrjtidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                                bPrjtStrt = true;
                                sTmpPrjt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRJT_NAME"));
                                PrjtidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bPrjtStrt = false;
                            }
                        }

                        //DEMAND_TYPE
                        if (!bDemandStrt)
                        {
                            bDemandStrt = true;
                            sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                            DemandidxS = m;
                            //if (sTmpDemand.Equals(""))
                            //    bDemandStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE")).Equals(sTmpDemand))
                                DemandidxE = m;
                            else
                            {
                                if (DemandidxS > DemandidxE)
                                {
                                    DemandidxE = DemandidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                                bDemandStrt = true;
                                sTmpDemand = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DEMAND_TYPE"));
                                DemandidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bDemandStrt = false;
                            }
                        }
                        //MKT_TYPE_NAME
                        if (!bMktStrt)
                        {
                            bMktStrt = true;
                            sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                            MktidxS = m;
                            //if (sTmpMkt.Equals(""))
                            //    bMktStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME")).Equals(sTmpMkt))
                                MktidxE = m;
                            else
                            {
                                if (MktidxS > MktidxE)
                                {
                                    MktidxE = MktidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                                bMktStrt = true;
                                sTmpMkt = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MKT_TYPE_NAME"));
                                MktidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bMktStrt = false;
                            }
                        }
                        //ELTR_TYPE
                        if (!bEltrStrt)
                        {
                            bEltrStrt = true;
                            sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                            EltridxS = m;
                            //if (sTmpEltr.Equals(""))
                            //    bEltrStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE")).Equals(sTmpEltr))
                                EltridxE = m;
                            else
                            {
                                if (EltridxS > EltridxE)
                                {
                                    EltridxE = EltridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                                bEltrStrt = true;
                                sTmpEltr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ELTR_TYPE"));
                                EltridxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bEltrStrt = false;
                            }
                        }

                        //MODLID
                        if (!bModlStrt)
                        {
                            bModlStrt = true;
                            sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                            ModlidxS = m;
                            //if (sTmpMold.Equals(""))
                            //    bModlStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID")).Equals(sTmpMold))
                                ModlidxE = m;
                            else
                            {
                                if (ModlidxS > ModlidxE)
                                {
                                    ModlidxE = ModlidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                                bModlStrt = true;
                                sTmpMold = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MODLID"));
                                ModlidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bModlStrt = false;
                            }
                        }

                        //PRE_DISPERSION_ID
                        if (!bDispersion_idStrt)
                        {
                            bDispersion_idStrt = true;
                            sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                            Dispersion_ididxS = m;
                            //if (sTmpDispersion_id.Equals(""))
                            //    bDispersion_idStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID")).Equals(sTmpDispersion_id))
                                Dispersion_ididxE = m;
                            else
                            {
                                if (Dispersion_ididxS > Dispersion_ididxE)
                                {
                                    Dispersion_ididxE = Dispersion_ididxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                                bDispersion_idStrt = true;
                                sTmpDispersion_id = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_ID"));
                                Dispersion_ididxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bDispersion_idStrt = false;
                            }
                        }

                        //PRE_DISPERSION_NAME
                        if (!bDispersion_nameStrt)
                        {
                            bDispersion_nameStrt = true;
                            sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                            Dispersion_nameidxS = m;
                            //if (sTmpDispersion_name.Equals(""))
                            //    bDispersion_nameStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME")).Equals(sTmpDispersion_name))
                                Dispersion_nameidxE = m;
                            else
                            {
                                if (Dispersion_nameidxS > Dispersion_nameidxE)
                                {
                                    Dispersion_nameidxE = Dispersion_nameidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                                bDispersion_nameStrt = true;
                                sTmpDispersion_name = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PRE_DISPERSION_NAME"));
                                Dispersion_nameidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bDispersion_nameStrt = false;
                            }
                        }
                        //ACTIVE_MTRL1
                        if (!bMtrl1Strt)
                        {
                            bMtrl1Strt = true;
                            sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                            Mtrl1idxS = m;
                            //if (sTmpMtrl1.Equals(""))
                            //    bMtrl1Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1")).Equals(sTmpMtrl1))
                                Mtrl1idxE = m;
                            else
                            {
                                if (Mtrl1idxS > Mtrl1idxE)
                                {
                                    Mtrl1idxE = Mtrl1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                                bMtrl1Strt = true;
                                sTmpMtrl1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL1"));
                                Mtrl1idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bMtrl1Strt = false;
                            }
                        }

                        //ACTIVE_MTRL2
                        if (!bMtrl2Strt)
                        {
                            bMtrl2Strt = true;
                            sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                            Mtrl2idxS = m;
                            //if (sTmpMtrl2.Equals(""))
                            //    bMtrl2Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2")).Equals(sTmpMtrl2))
                                Mtrl2idxE = m;
                            else
                            {
                                if (Mtrl2idxS > Mtrl2idxE)
                                {
                                    Mtrl2idxE = Mtrl2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                                bMtrl2Strt = true;
                                sTmpMtrl2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL2"));
                                Mtrl2idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bMtrl2Strt = false;
                            }
                        }
                        //ACTIVE_MTRL3
                        if (!bMtrl3Strt)
                        {
                            bMtrl3Strt = true;
                            sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                            Mtrl3idxS = m;
                            //if (sTmpMtrl3.Equals(""))
                            //    bMtrl3Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3")).Equals(sTmpMtrl3))
                                Mtrl3idxE = m;
                            else
                            {
                                if (Mtrl3idxS > Mtrl3idxE)
                                {
                                    Mtrl3idxE = Mtrl3idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                                bMtrl3Strt = true;
                                sTmpMtrl3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "ACTIVE_MTRL3"));
                                Mtrl3idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bMtrl3Strt = false;
                            }
                        }
                        //BINDER1
                        if (!bBinder1Strt)
                        {
                            bBinder1Strt = true;
                            sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                            Binder1idxS = m;
                            //if (sTmpBinder1.Equals(""))
                            //    bBinder1Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1")).Equals(sTmpBinder1))
                                Binder1idxE = m;
                            else
                            {
                                if (Binder1idxS > Binder1idxE)
                                {
                                    Binder1idxE = Binder1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                                bBinder1Strt = true;
                                sTmpBinder1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER1"));
                                Binder1idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bBinder1Strt = false;
                            }
                        }

                        //BINDER2
                        if (!bBinder2Strt)
                        {
                            bBinder2Strt = true;
                            sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                            Binder2idxS = m;
                            //if (sTmpBinder2.Equals(""))
                            //    bBinder2Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2")).Equals(sTmpBinder2))
                                Binder2idxE = m;
                            else
                            {
                                if (Binder2idxS > Binder2idxE)
                                {
                                    Binder2idxE = Binder2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["BINDER2"].Index)));
                                bBinder2Strt = true;
                                sTmpBinder2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER2"));
                                Binder2idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bBinder2Strt = false;
                            }
                        }
                        //BINDER3
                        if (!bBinder3Strt)
                        {
                            bBinder3Strt = true;
                            sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                            Binder3idxS = m;
                            //if (sTmpBinder3.Equals(""))
                            //    bBinder3Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3")).Equals(sTmpBinder3))
                                Binder3idxE = m;
                            else
                            {
                                if (Binder3idxS > Binder3idxE)
                                {
                                    Binder3idxE = Binder3idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                                bBinder3Strt = true;
                                sTmpBinder3 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "BINDER3"));
                                Binder3idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bBinder3Strt = false;
                            }
                        }
                        //CONDUCTION_MTRL1
                        if (!bConduction1Strt)
                        {
                            bConduction1Strt = true;
                            sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                            Conduction1idxS = m;
                            //if (sTmpConduction1.Equals(""))
                            //    bConduction1Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1")).Equals(sTmpConduction1))
                                Conduction1idxE = m;
                            else
                            {
                                if (Conduction1idxS > Conduction1idxE)
                                {
                                    Conduction1idxE = Conduction1idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                                bConduction1Strt = true;
                                sTmpConduction1 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL1"));
                                Conduction1idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bConduction1Strt = false;
                            }
                        }

                        //CONDUCTION_MTRL2
                        if (!bConduction2Strt)
                        {
                            bConduction2Strt = true;
                            sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                            Conduction2idxS = m;
                            //if (sTmpConduction2.Equals(""))
                            //    bConduction2Strt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2")).Equals(sTmpConduction2))
                                Conduction2idxE = m;
                            else
                            {
                                if (Conduction2idxS > Conduction2idxE)
                                {
                                    Conduction2idxE = Conduction2idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                                bConduction2Strt = true;
                                sTmpConduction2 = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CONDUCTION_MTRL2"));
                                Conduction2idxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bConduction2Strt = false;
                            }
                        }

                        //CMC
                        if (!bCmcStrt)
                        {
                            bCmcStrt = true;
                            sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                            CmcidxS = m;
                            //if (sTmpCmc.Equals(""))
                            //    bCmcStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC")).Equals(sTmpCmc))
                                CmcidxE = m;
                            else
                            {
                                if (CmcidxS > CmcidxE)
                                {
                                    CmcidxE = CmcidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                                bCmcStrt = true;
                                sTmpCmc = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CMC"));
                                CmcidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bCmcStrt = false;
                            }
                        }

                        //FOIL
                        if (!bFoilStrt)
                        {
                            bFoilStrt = true;
                            sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                            FoilidxS = m;
                            //if (sTmpFoil.Equals(""))
                            //    bFoilStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL")).Equals(sTmpFoil))
                                FoilidxE = m;
                            else
                            {
                                if (FoilidxS > FoilidxE)
                                {
                                    FoilidxE = FoilidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                                bFoilStrt = true;
                                sTmpFoil = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "FOIL"));
                                FoilidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bFoilStrt = false;
                            }
                        }
                        //DAILY_CAPA_PPM
                        if (!bCapaStrt)
                        {
                            bCapaStrt = true;
                            sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                            CapaidxS = m;
                            //if (sTmpCapa.Equals(""))
                            //    bCapaStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM")).Equals(sTmpCapa))
                                CapaidxE = m;
                            else
                            {
                                if (CapaidxS > CapaidxE)
                                {
                                    CapaidxE = CapaidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                                bCapaStrt = true;
                                sTmpCapa = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "DAILY_CAPA_PPM"));
                                CapaidxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bCapaStrt = false;
                            }
                        }
                        //PROD_VER_CODE
                        if (!bVerStrt)
                        {
                            bVerStrt = true;
                            sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                            VeridxS = m;
                            //if (sTmpVer.Equals(""))
                            //    bVerStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE")).Equals(sTmpVer))
                                VeriddxE = m;
                            else
                            {
                                if (VeridxS > VeriddxE)
                                {
                                    VeriddxE = VeridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                                bVerStrt = true;
                                sTmpVer = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "PROD_VER_CODE"));
                                VeridxS = m;
                                //if (sTmpProc.Equals(""))
                                //    bVerStrt = false;
                            }
                        }
                        //CATEGORY_DISP_NAME
                        if (!bCategoryStrt)
                        {
                            bCategoryStrt = true;
                            sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                            CategoryidxS = m;
                            //if (sTmpCategory.Equals(""))
                            //    bCategoryStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME")).Equals(sTmpCategory))
                                CategoryidxE = m;
                            else
                            {
                                if (CategoryidxS > CategoryidxE)
                                {
                                    CategoryidxE = CategoryidxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                                bCategoryStrt = true;
                                sTmpCategory = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "CATEGORY_DISP_NAME"));
                                CategoryidxS = m;
                                //if (sTmpCategory.Equals(""))
                                //    bCategoryStrt = false;
                            }
                        }
                        //MEASR_NAME
                        if (!bMeasrStrt)
                        {
                            bMeasrStrt = true;
                            sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                            MeasridxS = m;
                            //if (sTmpMeasr.Equals(""))
                            //    bMeasrStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME")).Equals(sTmpMeasr))
                                MeasridxE = m;
                            else
                            {
                                if (MeasridxS > MeasridxE)
                                {
                                    MeasridxE = MeasridxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                                bMeasrStrt = true;
                                sTmpMeasr = Util.NVC(DataTableConverter.GetValue(dgList.Rows[m].DataItem, "MEASR_NAME"));
                                MeasridxS = m;
                                //if (sTmpMeasr.Equals(""))
                                //    bMeasrStrt = false;
                            }
                        }
                    }
                    //AREANAME
                    if (bAreaStrt)
                    {
                        if (AreaidxS > AreaidxE)
                        {
                            AreaidxE = AreaidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(AreaidxS, dgList.Columns["AREANAME"].Index), dgList.GetCell(AreaidxE, dgList.Columns["AREANAME"].Index)));
                        bAreaStrt = false;
                        sTmpArea = string.Empty;
                        AreaidxE = 0;
                        AreaidxS = 0;

                    }
                    //PROCNAME
                    if (bProcStrt)
                    {
                        if (ProcidxS > ProcidxE)
                        {
                            ProcidxE = ProcidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(ProcidxS, dgList.Columns["PROCNAME"].Index), dgList.GetCell(ProcidxE, dgList.Columns["PROCNAME"].Index)));
                        ProcidxS = 0;
                        ProcidxE = 0;
                        bProcStrt = false;
                        sTmpProc = string.Empty;
                    }
                    //EQSGNAME
                    if (bEqsgStrt)
                    {
                        if (EqsgidxS > EqsgidxE)
                        {
                            EqsgidxE = EqsgidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(EqsgidxS, dgList.Columns["EQSGNAME"].Index), dgList.GetCell(EqsgidxE, dgList.Columns["EQSGNAME"].Index)));
                        EqsgidxS = 0;
                        EqsgidxE = 0;
                        bEqsgStrt = false;
                        sTmpEqsg = string.Empty;
                    }
                    //EQPTNAME
                    if (bEqptStrt)
                    {
                        if (EqptidxS > EqptidxE)
                        {
                            EqptidxE = EqptidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(EqptidxS, dgList.Columns["EQPTNAME"].Index), dgList.GetCell(EqptidxE, dgList.Columns["EQPTNAME"].Index)));
                        EqptidxS = 0;
                        EqptidxE = 0;
                        bEqptStrt = false;
                        sTmpEqpt = string.Empty;
                    }
                    //PRODID
                    if (bProdStrt)
                    {
                        if (ProdidxS > ProdidxE)
                        {
                            ProdidxE = ProdidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(ProdidxS, dgList.Columns["PRODID"].Index), dgList.GetCell(ProdidxE, dgList.Columns["PRODID"].Index)));
                        ProdidxS = 0;
                        ProdidxE = 0;
                        bProdStrt = false;
                        sTmpProd = string.Empty;
                    }
                    //PRJT_NAME
                    if (bPrjtStrt)
                    {
                        if (PrjtidxS > PrjtidxE)
                        {
                            PrjtidxE = PrjtidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(PrjtidxS, dgList.Columns["PRJT_NAME"].Index), dgList.GetCell(PrjtidxE, dgList.Columns["PRJT_NAME"].Index)));
                        PrjtidxS = 0;
                        PrjtidxE = 0;
                        bPrjtStrt = false;
                        sTmpPrjt = string.Empty;
                    }
                    //DEMAND_TYPE
                    if (bDemandStrt)
                    {
                        if (DemandidxS > DemandidxE)
                        {
                            DemandidxE = DemandidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(DemandidxS, dgList.Columns["DEMAND_TYPE"].Index), dgList.GetCell(DemandidxE, dgList.Columns["DEMAND_TYPE"].Index)));
                        DemandidxS = 0;
                        DemandidxE = 0;
                        bDemandStrt = false;
                        sTmpDemand = string.Empty;
                    }
                    //MKT_TYPE_NAME
                    if (bMktStrt)
                    {
                        if (MktidxS > MktidxE)
                        {
                            MktidxE = MktidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(MktidxS, dgList.Columns["MKT_TYPE_NAME"].Index), dgList.GetCell(MktidxE, dgList.Columns["MKT_TYPE_NAME"].Index)));
                        MktidxS = 0;
                        MktidxE = 0;
                        bMktStrt = false;
                        sTmpMkt = string.Empty;
                    }

                    //ELTR_TYPE
                    if (bEltrStrt)
                    {
                        if (EltridxS > EltridxE)
                        {
                            EltridxE = EltridxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(EltridxS, dgList.Columns["ELTR_TYPE"].Index), dgList.GetCell(EltridxE, dgList.Columns["ELTR_TYPE"].Index)));
                        EltridxS = 0;
                        EltridxE = 0;
                        bEltrStrt = false;
                        sTmpEltr = string.Empty;
                    }

                    //MODLID
                    if (bModlStrt)
                    {
                        if (ModlidxS > ModlidxE)
                        {
                            ModlidxE = ModlidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(ModlidxS, dgList.Columns["MODLID"].Index), dgList.GetCell(ModlidxE, dgList.Columns["MODLID"].Index)));
                        ModlidxS = 0;
                        ModlidxE = 0;
                        bModlStrt = false;
                        sTmpMold = string.Empty;
                    }
                    //PRE_DISPERSION_ID
                    if (bDispersion_idStrt)
                    {
                        if (Dispersion_ididxS > Dispersion_ididxE)
                        {
                            Dispersion_ididxE = Dispersion_ididxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_ididxS, dgList.Columns["PRE_DISPERSION_ID"].Index), dgList.GetCell(Dispersion_ididxE, dgList.Columns["PRE_DISPERSION_ID"].Index)));
                        Dispersion_ididxS = 0;
                        Dispersion_ididxE = 0;
                        bDispersion_idStrt = false;
                        sTmpDispersion_id = string.Empty;
                    }
                    //PRE_DISPERSION_NAME
                    if (bDispersion_nameStrt)
                    {
                        if (Dispersion_nameidxS > Dispersion_nameidxE)
                        {
                            Dispersion_nameidxE = Dispersion_nameidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Dispersion_nameidxS, dgList.Columns["PRE_DISPERSION_NAME"].Index), dgList.GetCell(Dispersion_nameidxE, dgList.Columns["PRE_DISPERSION_NAME"].Index)));
                        Dispersion_nameidxS = 0;
                        Dispersion_nameidxE = 0;
                        bDispersion_nameStrt = false;
                        sTmpDispersion_name = string.Empty;
                    }
                    //ACTIVE_MTRL1
                    if (bMtrl1Strt)
                    {
                        if (Mtrl1idxS > Mtrl1idxE)
                        {
                            Mtrl1idxE = Mtrl1idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl1idxS, dgList.Columns["ACTIVE_MTRL1"].Index), dgList.GetCell(Mtrl1idxE, dgList.Columns["ACTIVE_MTRL1"].Index)));
                        Mtrl1idxS = 0;
                        Mtrl1idxE = 0;
                        bMtrl1Strt = false;
                        sTmpMtrl1 = string.Empty;
                    }
                    //ACTIVE_MTRL2
                    if (bMtrl2Strt)
                    {
                        if (Mtrl2idxS > Mtrl2idxE)
                        {
                            Mtrl2idxE = Mtrl2idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl2idxS, dgList.Columns["ACTIVE_MTRL2"].Index), dgList.GetCell(Mtrl2idxE, dgList.Columns["ACTIVE_MTRL2"].Index)));
                        Mtrl2idxS = 0;
                        Mtrl2idxE = 0;
                        bMtrl2Strt = false;
                        sTmpMtrl2 = string.Empty;
                    }
                    //ACTIVE_MTRL3
                    if (bMtrl3Strt)
                    {
                        if (Mtrl3idxS > Mtrl3idxE)
                        {
                            Mtrl3idxE = Mtrl3idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Mtrl3idxS, dgList.Columns["ACTIVE_MTRL3"].Index), dgList.GetCell(Mtrl3idxE, dgList.Columns["ACTIVE_MTRL3"].Index)));
                        Mtrl3idxS = 0;
                        Mtrl3idxE = 0;
                        bMtrl3Strt = false;
                        sTmpMtrl3 = string.Empty;
                    }
                    // BINDER1
                    if (bBinder1Strt)
                    {
                        if (Binder1idxS > Binder1idxE)
                        {
                            Binder1idxE = Binder1idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder1idxS, dgList.Columns["BINDER1"].Index), dgList.GetCell(Binder1idxE, dgList.Columns["BINDER1"].Index)));
                        Binder1idxS = 0;
                        Binder1idxE = 0;
                        bBinder1Strt = false;
                        sTmpBinder1 = string.Empty;
                    }
                    //BINDER2
                    if (bBinder2Strt)
                    {
                        if (Binder2idxS > Binder2idxE)
                        {
                            Binder2idxE = Binder2idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder2idxS, dgList.Columns["BINDER2"].Index), dgList.GetCell(Binder2idxE, dgList.Columns["BINDER2"].Index)));
                        Binder2idxS = 0;
                        Binder2idxE = 0;
                        bBinder2Strt = false;
                        sTmpBinder2 = string.Empty;
                    }
                    // BINDER3
                    if (bBinder3Strt)
                    {
                        if (Binder3idxS > Binder3idxE)
                        {
                            Binder3idxE = Binder3idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Binder3idxS, dgList.Columns["BINDER3"].Index), dgList.GetCell(Binder3idxE, dgList.Columns["BINDER3"].Index)));
                        Binder3idxS = 0;
                        Binder3idxE = 0;
                        bBinder3Strt = false;
                        sTmpBinder3 = string.Empty;
                    }
                    //CONDUCTION_MTRL1
                    if (bConduction1Strt)
                    {
                        if (Conduction1idxS > Conduction1idxE)
                        {
                            Conduction1idxE = Conduction1idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction1idxS, dgList.Columns["CONDUCTION_MTRL1"].Index), dgList.GetCell(Conduction1idxE, dgList.Columns["CONDUCTION_MTRL1"].Index)));
                        Conduction1idxS = 0;
                        Conduction1idxE = 0;
                        bConduction1Strt = false;
                        sTmpConduction1 = string.Empty;
                    }
                    //CONDUCTION_MTRL2
                    if (bConduction2Strt)
                    {
                        if (Conduction2idxS > Conduction2idxE)
                        {
                            Conduction2idxE = Conduction2idxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(Conduction2idxS, dgList.Columns["CONDUCTION_MTRL2"].Index), dgList.GetCell(Conduction2idxE, dgList.Columns["CONDUCTION_MTRL2"].Index)));
                        Conduction2idxS = 0;
                        Conduction2idxE = 0;
                        bConduction2Strt = false;
                        sTmpConduction2 = string.Empty;
                    }
                    //CMC
                    if (bCmcStrt)
                    {
                        if (CmcidxS > CmcidxE)
                        {
                            CmcidxE = CmcidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(CmcidxS, dgList.Columns["CMC"].Index), dgList.GetCell(CmcidxE, dgList.Columns["CMC"].Index)));
                        CmcidxS = 0;
                        CmcidxE = 0;
                        bCmcStrt = false;
                        sTmpCmc = string.Empty;
                    }
                    //FOIL
                    if (bFoilStrt)
                    {
                        if (FoilidxS > FoilidxE)
                        {
                            FoilidxE = FoilidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(FoilidxS, dgList.Columns["FOIL"].Index), dgList.GetCell(FoilidxE, dgList.Columns["FOIL"].Index)));
                        FoilidxS = 0;
                        FoilidxE = 0;
                        bFoilStrt = false;
                        sTmpFoil = string.Empty;
                    }
                    //DAILY_CAPA_PPM
                    if (bCapaStrt)
                    {
                        if (CapaidxS > CapaidxE)
                        {
                            CapaidxE = CapaidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(CapaidxS, dgList.Columns["DAILY_CAPA_PPM"].Index), dgList.GetCell(CapaidxE, dgList.Columns["DAILY_CAPA_PPM"].Index)));
                        CapaidxS = 0;
                        CapaidxE = 0;
                        bCapaStrt = false;
                        sTmpCapa = string.Empty;
                    }
                    //PROD_VER_CODE
                    if (bVerStrt)
                    {
                        if (VeridxS > VeriddxE)
                        {
                            VeriddxE = VeridxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(VeridxS, dgList.Columns["PROD_VER_CODE"].Index), dgList.GetCell(VeriddxE, dgList.Columns["PROD_VER_CODE"].Index)));
                        VeridxS = 0;
                        VeriddxE = 0;
                        bVerStrt = false;
                        sTmpVer = string.Empty;
                    }
                    //CATEGORY_DISP_NAME
                    if (bCategoryStrt)
                    {
                        if (CategoryidxS > CategoryidxE)
                        {
                            CategoryidxE = CategoryidxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(CategoryidxS, dgList.Columns["CATEGORY_DISP_NAME"].Index), dgList.GetCell(CategoryidxE, dgList.Columns["CATEGORY_DISP_NAME"].Index)));
                        CategoryidxS = 0;
                        CategoryidxE = 0;
                        bCategoryStrt = false;
                        sTmpCategory = string.Empty;
                    }
                    //MEASR
                    if (bMeasrStrt)
                    {
                        if (MeasridxS > MeasridxE)
                        {
                            MeasridxE = MeasridxS;
                        }
                        e.Merge(new DataGridCellsRange(dgList.GetCell(MeasridxS, dgList.Columns["MEASR_NAME"].Index), dgList.GetCell(MeasridxE, dgList.Columns["MEASR_NAME"].Index)));
                        bMeasrStrt = false;
                        MeasridxS = 0;
                        MeasridxE = dgList.TopRows.Count;
                        sTmpMeasr = string.Empty;
                    }
                    #endregion
                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region Mehod

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

                List<string> sHeader = new List<string>() { dtFrom.AddDays(i - 1).Year.ToString() + "W" + GetWeekOfYear(dtFrom.AddDays(i - 1), null).ToString(), dtFrom.AddDays(i - 1).ToString("MM") + "/" + dtFrom.AddDays(i - 1).ToString("dd") };

                if (DateTime.Now.ToString("yyyyMMdd") == dtFrom.AddDays(i - 1).Year.ToString() + dtFrom.AddDays(i - 1).Month.ToString("00") + dtFrom.AddDays(i - 1).ToString("dd"))
                {
                    List<string> sHeader_Today = new List<string>() { dtFrom.AddDays(i - 1).Year.ToString() + "W" + GetWeekOfYear(dtFrom.AddDays(i - 1), null).ToString(), dtFrom.AddDays(i - 1).ToString("MM") + "/" + dtFrom.AddDays(i - 1).ToString("dd") + " (Today)" };
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader_Today, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
                else
                {
                    Util.SetGridColumnText(dgList, dayColumnName, sHeader, i.ToString(), false, false, false, true, new C1.WPF.DataGrid.DataGridLength(), HorizontalAlignment.Right, Visibility.Visible);
                }
            }
        }

        private void SearchData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                for (int i = dgList.Columns.Count; i-- > 32;) //Grid Head 삭제
                    dgList.Columns.RemoveAt(i);

                //Grid Head 재생성
                SetGridDate();

                string _ValueFrom = string.Format("{0:yyyyMMdd}", dtpFrom.SelectedDateTime);
                string _ValueTo = string.Format("{0:yyyyMMdd}", dtpTo.SelectedDateTime);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("FROM_DATE", typeof(string));
                IndataTable.Columns.Add("TO_DATE", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("DEMAND_TYPE", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PRJT_NAME", typeof(string));
                IndataTable.Columns.Add("MODLID", typeof(string));
                IndataTable.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                IndataTable.Columns.Add("CATEGORY_CODE", typeof(string));
                IndataTable.Columns.Add("MEASR_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["FROM_DATE"] = _ValueFrom;
                Indata["TO_DATE"] = _ValueTo;
                Indata["SHOPID"] = Util.GetCondition(cboShop);
                Indata["AREAID"] = Util.GetCondition(cboArea);
                Indata["PROCID"] = Util.GetCondition(cboProcess);
                Indata["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                Indata["EQPTID"] = Util.GetCondition(cboEquipment);
                Indata["DEMAND_TYPE"] = Util.GetCondition(cboDEMAND_TYPE);
                Indata["PRODID"] = Util.NVC(txtProduct.Text);
                Indata["PRJT_NAME"] = Util.NVC(txtProject.Text);
                Indata["MODLID"] = Util.NVC(txtModel.Text);
                Indata["PRDT_CLSS_CODE"] = Util.GetCondition(cboElecType);
                //Indata["CATEGORY_CODE"] = Util.GetCondition(cboCategory);
                if (cboCategory.SelectedItemsToString == string.Empty)
                {
                    Indata["CATEGORY_CODE"] = null;
                }
                else
                {
                    Indata["CATEGORY_CODE"] = cboCategory.SelectedItemsToString;  //Category
                }
                //Indata["MEASR_CODE"] = Util.GetCondition(cboMeasure);
                if (cboMeasure.SelectedItemsToString == string.Empty)
                {
                    Indata["MEASR_CODE"] = null;
                }
                else
                {
                    Indata["MEASR_CODE"] = cboMeasure.SelectedItemsToString;  //Measure
                }
                IndataTable.Rows.Add(Indata);

                // 2019-08-28 오화백 DA_PRD_SEL_ELEC_FP_DPL_PLAN_INFO 에서 DA_PRD_SEL_FP_DPL_PLAN_CT으로 변경
                new ClientProxy().ExecuteService("DA_PRD_SEL_FP_DPL_PLAN_CT", "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Util.GridSetData(dgList, result, FrameOperation, false);

                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            Scrolling_Into_Today_Col();
                        }
                        dgList.MergingCells -= dgList_MergingCells;
                        dgList.MergingCells += dgList_MergingCells;
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

        private void Scrolling_Into_Today_Col()
        {
            string today_Col_Name = "PLAN_QTY_" + DateTime.Now.ToString("dd");

            if (dgList.Columns[today_Col_Name] != null)
            {
                if (DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpFrom.SelectedDateTime) ||
                    DateTime.Now.ToString("yyyyMM") == string.Format("{0:yyyyMM}", dtpTo.SelectedDateTime))
                {
                    dgList.ScrollIntoView(2, dgList.Columns[today_Col_Name].Index);
                }
            }
        }

        public int GetWeekOfYear(DateTime sourceDate, CultureInfo cultureInfo)
        {
            if (sourceDate == null) return 0;

            if (cultureInfo == null)
                cultureInfo = CultureInfo.CurrentCulture;

            CalendarWeekRule calendarWeekRule = cultureInfo.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday; //cultureInfo.DateTimeFormat.FirstDayOfWeek;
            return cultureInfo.Calendar.GetWeekOfYear(sourceDate, calendarWeekRule, firstDayOfWeek);
        }


        //2019 10 17 오화백 CATEGORY, MEASURE 에 대한 멀티콤보형식으로 변경 
        private void GetCategoryCombo()
        {
           
                try
                {
                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["CMCDTYPE"] = "PRODUCTION_PLAN_CATEGORY";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTION_PLAN_CATEGORY_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                   if (dtResult.Rows.Count != 0)
                    {
                        cboCategory.isAllUsed = false;
                        if (dtResult.Rows.Count == 1)
                        {
                            cboCategory.ItemsSource = DataTableConverter.Convert(dtResult);
                            cboCategory.Check(-1);
                        }
                        else
                        {
                            cboCategory.isAllUsed = true;
                            cboCategory.ItemsSource = DataTableConverter.Convert(dtResult);
                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                               if(dtResult.Rows[i]["CBO_CODE"].ToString() == "DPP_IN")
                                {
                                    cboCategory.Check(i);
                                }
                               
                            }
                        }
                    }
                    else
                    {
                        cboCategory.ItemsSource = null;
                    }
                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
          
        }

        private void GetMeasureCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PRODUCTION_PLAN_MEASR";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    cboMeasure.isAllUsed = false;
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMeasure.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMeasure.Check(-1);
                    }
                    else
                    {
                        cboMeasure.isAllUsed = true;
                        cboMeasure.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (dtResult.Rows[i]["CBO_CODE"].ToString() == "BTCH"|| dtResult.Rows[i]["CBO_CODE"].ToString() == "JR"|| dtResult.Rows[i]["CBO_CODE"].ToString() == "CELL")
                            {
                                cboMeasure.Check(i);
                            }

                        }
                    }
                }
                else
                {
                    cboMeasure.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        //2019 10 17 오화백 셋팅할 컬럼 멀티콤보 셋팅
        private void SettingColumns()
        {
            try
            {
                DtColumns = new DataTable();
                DtColumns.Columns.Add("CBO_CODE", typeof(string));
                DtColumns.Columns.Add("CBO_NAME", typeof(string));
            
                #region Hidden 컬럼 데이터 테이블 Rows
                DataRow row = null;
                row = DtColumns.NewRow();
                row["CBO_CODE"] = "AREANAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("동");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PROCNAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("공정");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "EQSGNAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("라인");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "EQPTNAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("설비");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PRODID";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("제품코드");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PRJT_NAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("프로젝트명");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "DEMAND_TYPE";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("생산유형");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "MKT_TYPE_NAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("시장유형");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "ELTR_TYPE";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("ELEC");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "MODLID";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("모델");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PRE_DISPERSION_ID";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("PRE_DISPERSION_ID");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PRE_DISPERSION_NAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("PRE_DISPERSION_NAME");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "ACTIVE_MTRL1";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("ACTIVE_MATERIAL_1");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "ACTIVE_MTRL2";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("ACTIVE_MATERIAL_2");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "ACTIVE_MTRL3";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("ACTIVE_MATERIAL_3");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "BINDER1";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("BINDER_1");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "BINDER2";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("BINDER_2");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "BINDER3";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("BINDER_3");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "CONDUCTION_MTRL1";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("CONDUCTION_MATERIAL_1");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "CONDUCTION_MTRL2";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("CONDUCTION_MATERIAL_2");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "CMC";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("CMC");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "FOIL";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("FOIL");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "DAILY_CAPA_PPM";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("DAILY_CAPA_PPM");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "PROD_VER_CODE";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("VERSION");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "CATEGORY_DISP_NAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("CATEGORY");
                DtColumns.Rows.Add(row);

                row = DtColumns.NewRow();
                row["CBO_CODE"] = "MEASR_NAME";
                row["CBO_NAME"] = ObjectDic.Instance.GetObjectName("MEASURE");
                DtColumns.Rows.Add(row);
                #endregion

                cboCloumSetting.isAllUsed = true;
                cboCloumSetting.ItemsSource = DataTableConverter.Convert(DtColumns);
            
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

      
    }
}
