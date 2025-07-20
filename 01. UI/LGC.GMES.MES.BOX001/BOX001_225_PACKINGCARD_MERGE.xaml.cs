/*************************************************************************************
 Created Date : 2018.07.12
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.12  DEVELOPER : Initial Created.
  2023.07.13  김도형    : [E20230306-000128]전극수 소수점 개선
 
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Linq;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_225_PACKINGCARD_MERGE : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public BOX001_225 BOX001_225;        
        public DataTable dtPackingCard_Merge;
        public DataTable dtBasicInfo;
        public DataTable dtSel01;
        public DataTable dtSel02;
        public DataTable dtSel03;
        public DataTable dtLot;
        public DataView PancakeList;
        public string SkidID = string.Empty;
        private Util _Util = new Util();
        private string m_PkgWay = string.Empty;
        private string m_PackingRemark = string.Empty;
        private string m_SkidRemark = string.Empty;
        private double m_iFrameLane1 = 0;
        private double m_iFrameLane2 = 0;
        private double m_iFrameLane3 = 0;
        private double m_dCutM1 = 0;
        private double m_dCutM2 = 0;
        private double m_dCutM3 = 0;
        private double m_dCell1 = 0;
        private double m_dCell2 = 0;
        private double m_dCell3 = 0;
        private string m_PackingNo1 = string.Empty;
        private string m_PackingNo2 = string.Empty;
        private string m_PackingNo3 = string.Empty;
        private string m_TrasferLoc = string.Empty;
        private string m_ToLoc = string.Empty;        
        private string m_LotID1 = string.Empty;
        private string m_LotID2 = string.Empty;
        private string m_LotID3 = string.Empty;
        private string PackageWay = string.Empty;
        private string OWMS_Code = string.Empty;
        private double TotalLaneQty = 0.0d;
        private string LotQtyCheckMessage = string.Empty;
        private string m_EltrValueDecimalApplyFlag = string.Empty;  //[E20230306-000128]전극수 소수점 개선

        public BOX001_225_PACKINGCARD_MERGE()
        {
            InitializeComponent();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //if (bFormLoad == false)
            //{
            //    init_Form();
            //}

            m_EltrValueDecimalApplyFlag = GetEltrValueDecimalApplyFlag(); //[E20230306-000128]전극수 소수점 개선

            if (BOX001_225.bNew_Load == true)
            {
                init_Form();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //bFormLoad = true;
            BOX001_225.bNew_Load = false;
        }

        #endregion


        #region Initialize

        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();                        
            String[] sFilter1 = { "WH_TYPE2" };

            _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "PACKWAY");
        }

        private void init_Form()
        {
            try
            {
                Util.gridClear(dgPancakeList);                                
                Util.gridClear(dgPancakeListSelected01);
                Util.gridClear(dgPancakeListSelected02);
                Util.gridClear(dgPancakeListSelected03);

                //m_LotID1 = BOX001_225.sTempLot_1.ToString();
                //m_LotID2 = BOX001_225.sTempLot_2.ToString();
                //m_LotID3 = BOX001_225.sTempLot_3.ToString();

                txtLocation.Text = BOX001_225.cboTransLoc2.Text;

                txtSelectedM1.Text = "0";
                txtSelectedM2.Text = "0";
                txtSelectedM3.Text = "0";

                txtSelectedCell1.Text = "0";
                txtSelectedCell2.Text = "0";
                txtSelectedCell3.Text = "0";

                chkRemark1.IsChecked = false;
                chkRemark2.IsChecked = false;
                chkRemark3.IsChecked = false;
                chkRemark4.IsChecked = false;

                numLaneQty1.Value = 0;
                numLaneQty2.Value = 0;
                numLaneQty3.Value = 0;

                /// PancakeList ///                                
                DataTable ScanPancakeList = DataTableConverter.Convert(BOX001_225.dgOut.ItemsSource);
                
                for (int i = 0; i < ScanPancakeList.Rows.Count; i++)
                {
                    Search_Lot_List(ScanPancakeList.Rows[i]["LotID"].ToString());
                }

                // Total //
                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    txtCutM1.Text = string.Format("{0:###,##.#}", PancakeList.ToTable().Compute("SUM(M_WIPQTY)", ""));
                    txtCellM1.Text = string.Format("{0:###,###.#}", PancakeList.ToTable().Compute("SUM(CELL_WIPQTY)", ""));
                }
                else
                {
                    txtCutM1.Text = string.Format("{0:###,###}", PancakeList.ToTable().Compute("SUM(M_WIPQTY)", ""));
                    txtCellM1.Text = string.Format("{0:###,###}", PancakeList.ToTable().Compute("SUM(CELL_WIPQTY)", ""));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        
        #endregion


        #region Event

        private void cboPackWay_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            PackageWay = ObjectDic.Instance.GetObjectName("전극포장카드");
            OWMS_Code = (cboPackWay.SelectedItem as DataRowView)["ATTRIBUTE1"].ToString();
            m_PkgWay = cboPackWay.SelectedValue.ToString();

            switch (cboPackWay.SelectedValue.ToString())
            {                
                case "CRT":                    
                    PackageWay = PackageWay + " " + ObjectDic.Instance.GetObjectName("가대");

                    txtBlock1.Text = "Lane #1";
                    txtBlock2.Text = "Lane #2";
                    txtBlock3.Text = "Lane #3";
                    break;

                case "BOX":                    
                    PackageWay = PackageWay + " " + ObjectDic.Instance.GetObjectName("BOX");

                    txtBlock1.Text = "BoxLane #1";
                    txtBlock2.Text = "BoxLane #2";
                    txtBlock3.Text = "BoxLane #3";
                    break;

                case "STEELCASE":                    
                    PackageWay = PackageWay + " " + ObjectDic.Instance.GetObjectName("Steel Case");

                    txtBlock1.Text = "Lane #1";
                    txtBlock2.Text = "Lane #2";
                    txtBlock3.Text = "Lane #3";
                    break;

                case "STEELRACK":                    
                    PackageWay = PackageWay + " " + ObjectDic.Instance.GetObjectName("Steel Rack");

                    txtBlock1.Text = "Lane #1";
                    txtBlock2.Text = "Lane #2";
                    txtBlock3.Text = "Lane #3";
                    break;

                case "JUMBOCASE":                    
                    PackageWay = PackageWay + " " + ObjectDic.Instance.GetObjectName("Jumbo Case");

                    txtBlock1.Text = "Lane #1";
                    txtBlock2.Text = "Lane #2";
                    txtBlock3.Text = "Lane #3";
                    break;
            }

            InitLaneList();
        }
        
        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;

                if (TotalLaneQty == 0)
                {
                    Util.MessageValidation("SFU4986"); //선택된 Lane이 없습니다.
                    return;
                }

                double BaseTotalLane = Util.StringToDouble((cboPackWay.SelectedItem as DataRowView)["ATTRIBUTE2"].ToString());
                double WarningTotalLane = BaseTotalLane - 1;
                double UnableTotalLnae = BaseTotalLane + 1;

                switch (cboPackWay.SelectedValue.ToString())
                {
                    // 가대 //
                    case "CRT":
                        string LotID = string.Empty;

                        if (!CheckLotQty(ref LotID))
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(LotID + MessageDic.Instance.GetMessage("SFU5012"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                                , (result2) =>
                                {
                                    if (result2 == MessageBoxResult.OK)
                                    {
                                        PrintPackingList();
                                    }
                                }
                          );
                        }
                        else
                        {
                            PrintPackingList();
                        }
                        break;
                    
                    case "BOX":
                    case "STEELCASE":
                    case "STEELRACK":
                    case "JUMBOCASE":
                        if (TotalLaneQty <= WarningTotalLane)
                        {
                            //완포장이 아닙니다. 진행하시겠습니까?
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4987"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                                , (result) =>
                                {
                                    if (result == MessageBoxResult.OK)
                                    {
                                        LotID = string.Empty;

                                        if(!CheckLotQty(ref LotID))
                                        {
                                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(LotID + MessageDic.Instance.GetMessage("SFU5012"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                                            , (result2) =>
                                            {
                                                if (result2 == MessageBoxResult.OK)
                                                {
                                                    PrintPackingList();
                                                }                                                
                                            }
                                          );
                                        }
                                        else
                                        {
                                            PrintPackingList();
                                        }                                        
                                    }
                                });
                        }
                        else if (TotalLaneQty >= UnableTotalLnae)
                        {
                            Util.MessageValidation("SFU4988"); //Lane 수 초과하여 포장이 불가합니다.                            
                        }
                        else
                        {
                            LotID = string.Empty;

                            if (!CheckLotQty(ref LotID))
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(LotID + MessageDic.Instance.GetMessage("SFU5012"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None
                                , (result2) =>
                                {
                                    if (result2 == MessageBoxResult.OK)
                                    {
                                        PrintPackingList();
                                    }
                                }
                              );
                            }
                            else
                            {
                                PrintPackingList();
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnAddAllToPosition01_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected01, dgPancakeList, 1);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();            

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnDelAllFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeList);
            
            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnAddAllToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(2, dgPancakeListSelected02, dgPancakeList, 2);

            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected02, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_WIPQTY");
            txtSelectedCell2.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnDelAllFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeList);
            
            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected02, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_WIPQTY");
            txtSelectedCell2.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnAddAllToPosition03_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(3, dgPancakeListSelected03, dgPancakeList, 3);

            numLaneQty3.Value = dgPancakeListSelected03.GetRowCount();            

            double dM = GetPancakeSum(dgPancakeListSelected03, "M_WIPQTY");
            txtSelectedM3.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected03, "CELL_WIPQTY");
            txtSelectedCell3.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnDelAllFromPosition03_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected03, dgPancakeList);
            
            numLaneQty3.Value = dgPancakeListSelected03.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected03, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected03, "CELL_WIPQTY");
            txtSelectedCell3.Text = Convert.ToString(dCell);

            TotalLaneQty = (double)numLaneQty1.Value + (double)numLaneQty2.Value + (double)numLaneQty3.Value;
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgPancakeList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgPancakeList.Rows[i].DataItem, "CHK", true);
            }
        }

        private void btnUnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgPancakeList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgPancakeList.Rows[i].DataItem, "CHK", false);
            }
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
        }

        #endregion


        #region Mehod

        private void InitLaneList()
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeList, true);
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeList, true);
            DelFromPosition(0, dgPancakeListSelected03, dgPancakeList, true);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();
            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();
            numLaneQty3.Value = dgPancakeListSelected03.GetRowCount();

            txtSelectedM1.Text = "0";
            txtSelectedM2.Text = "0";
            txtSelectedM3.Text = "0";

            txtSelectedCell1.Text = "0";
            txtSelectedM2.Text = "0";
            txtSelectedM3.Text = "0";
        }

        private void Search_Lot_List(string Lotid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = Lotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PACKING_PALLTE_FALES", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }
                
                if (dgPancakeList.ItemsSource == null || dgPancakeList.GetRowCount() == 0)
                {
                    dgPancakeList.ItemsSource = DataTableConverter.Convert(SearchResult);
                }
                else
                {
                    DataTable temp = (dgPancakeList.ItemsSource as DataView).ToTable().Copy();
                    temp.Merge(SearchResult);

                    dgPancakeList.ItemsSource = temp.AsDataView();
                }            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void AddToPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid Selected, C1.WPF.DataGrid.C1DataGrid Remain, int iPosition)
        {
            if (Selected.Rows.Count == 0)
            {
                DataTable dtAdd = new DataTable();
                dtAdd.Columns.Add("CHK", typeof(bool));
                dtAdd.Columns.Add("LOTID", typeof(string));
                dtAdd.Columns.Add("M_WIPQTY", typeof(string));
                dtAdd.Columns.Add("CELL_WIPQTY", typeof(string));
                dtAdd.Columns.Add("PRODID", typeof(string));
                dtAdd.Columns.Add("POSITION", typeof(string));
                dtAdd.Columns.Add("EQSGNAME", typeof(string));
                dtAdd.Columns.Add("WIPSDTTM", typeof(string));
                dtAdd.Columns.Add("PROD_VER_CODE", typeof(string));
                dtAdd.Columns.Add("VLD_DATE", typeof(string));
                dtAdd.Columns.Add("FROM_AREAID", typeof(string));
                dtAdd.Columns.Add("FROM_SHOPID", typeof(string));
                dtAdd.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtAdd.Columns.Add("MODLID", typeof(string));
                dtAdd.Columns.Add("CSTID", typeof(string));

                Selected.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < Remain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        Selected.IsReadOnly = false;
                        Selected.BeginNewRow();
                        Selected.EndNewRow(true);
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CSTID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CSTID"));
                        Selected.IsReadOnly = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Remain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        Selected.IsReadOnly = false;
                        Selected.BeginNewRow();
                        Selected.EndNewRow(true);
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "CSTID", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "CSTID"));
                        Selected.IsReadOnly = true;
                    }
                }
            }

            for (int i = Remain.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;

                if (DataTableConverter.GetValue(Remain.Rows[k].DataItem, "CHK").ToString() == "True")
                {
                    Remain.IsReadOnly = false;
                    Remain.RemoveRow(k);
                    Remain.IsReadOnly = true;
                }
            }
        }

        private void DelFromPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid Selected, C1.WPF.DataGrid.C1DataGrid Remain, bool AllClear = false)
        {
            if (Selected.Rows.Count == 0)
            {
                return;
            }

            if (Remain.Rows.Count == 0)
            {
                DataTable dtAdd = new DataTable();
                dtAdd.Columns.Add("CHK", typeof(bool));
                dtAdd.Columns.Add("LOTID", typeof(string));
                dtAdd.Columns.Add("M_WIPQTY", typeof(string));
                dtAdd.Columns.Add("CELL_WIPQTY", typeof(string));
                dtAdd.Columns.Add("PRODID", typeof(string));
                dtAdd.Columns.Add("POSITION", typeof(string));
                dtAdd.Columns.Add("EQSGNAME", typeof(string));
                dtAdd.Columns.Add("WIPSDTTM", typeof(string));
                dtAdd.Columns.Add("PROD_VER_CODE", typeof(string));
                dtAdd.Columns.Add("VLD_DATE", typeof(string));
                dtAdd.Columns.Add("FROM_AREAID", typeof(string));
                dtAdd.Columns.Add("FROM_SHOPID", typeof(string));
                dtAdd.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtAdd.Columns.Add("MODLID", typeof(string));
                dtAdd.Columns.Add("CSTID", typeof(string));

                Remain.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < Selected.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CHK").ToString() == "True" || AllClear)
                    //if (DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CHK").ToString() == "True" && DataTableConverter.GetValue(Selected.Rows[i].DataItem, "POSITION").ToString() != "ADD")
                    {
                        Remain.IsReadOnly = false;
                        Remain.BeginNewRow();
                        Remain.EndNewRow(true);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CHK", false);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CSTID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CSTID"));
                        Remain.IsReadOnly = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Selected.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CHK").ToString() == "True" || AllClear)                        
                    {
                        Remain.IsReadOnly = false;
                        Remain.BeginNewRow();
                        Remain.EndNewRow(true);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CHK", false);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CSTID", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CSTID"));
                        Remain.IsReadOnly = true;
                    }
                }
            }

            for (int i = Selected.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;
                if (DataTableConverter.GetValue(Selected.Rows[k].DataItem, "CHK").ToString() == "True" || AllClear)
                {
                    Selected.IsReadOnly = false;
                    Selected.RemoveRow(k);
                    Selected.IsReadOnly = true;
                }
            }
        }

        private double GetPancakeSum(C1DataGrid DataGrid, string sName)
        {
            double dSum = 0;
            double dTotal = 0;

            for (int i = 0; i < DataGrid.Rows.Count; i++)
            {
                if (sName == "M_WIPQTY")
                {
                    dSum = Convert.ToDouble(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "M_WIPQTY").ToString()) * 10;
                }
                else if (sName == "CELL_WIPQTY")
                {
                    dSum = Convert.ToDouble(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CELL_WIPQTY").ToString()) * 10;
                }

                dTotal = dTotal + dSum;
            }

            return dTotal / 10;
        }

        private double GetPancakeSum2(C1DataGrid DataGrid, string sName)
        {
            decimal dSum = 0;

            for (int i = 0; i < DataGrid.Rows.Count; i++)
            {
                if (string.Equals(sName, "M_WIPQTY"))
                {
                    if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                    {
                        dSum += Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName));
                    }
                    else
                    {
                        dSum += Math.Floor(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)));
                    }
                }
                else if (string.Equals(sName, "CELL_WIPQTY"))
                {
                    if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                    {
                        dSum += Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName));
                    }
                    else
                    {
                        dSum += Math.Floor(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)));
                    }
                }                  
            }

            return Convert.ToDouble(dSum);
        }

        private void PrintPackingList()
        {            
            //포장카드 특이사항 내용 출력:체크박스에 선택된 내용만 선택하여 출력
            InitPackingNo();

            string sSiteID = string.Empty;

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            RQSTDT.Rows.Add(dr);

            DataTable SResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SITE_INFO", "RQSTDT", "RSLTDT", RQSTDT);

            if (SResult.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3026"); //SITE ID 기준정보가 존재하지 않습니다.
                return;
            }
            else
            {
                sSiteID = SResult.Rows[0]["SITEID"].ToString();
            }

            PackingCardDataBinding_Config();            
        }

        private bool CheckLotQty(ref string LotID)
        {
            bool returnValue = true;

            for (int i = 0; i < dgPancakeListSelected01.Rows.Count; i++)
            {
                double Qty = double.Parse(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "M_WIPQTY").ToString());

                if(Qty <= 100)
                {
                    if (LotID.Equals(string.Empty))
                    {
                        LotID += DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "LOTID").ToString();
                        returnValue = false;
                    }
                    else
                    {
                        LotID += ", " + DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "LOTID").ToString();
                    }                    
                }
            }

            LotID = "PanCake : " + LotID + Environment.NewLine + Environment.NewLine;
            return returnValue;
        }

        private void InitPackingNo()
        {
            m_PackingRemark = string.Empty;
            m_SkidRemark = string.Empty;

            //if (!m_LotID1.Equals(""))
            //{
            //    InitPackSkidRemake(m_LotID1);
            //}

            //if (!m_LotID2.Equals(""))
            //{
            //    InitPackSkidRemake(m_LotID2);
            //}
            //if (!m_LotID3.Equals(""))
            //{
            //    InitPackSkidRemake(m_LotID3);
            //}

            //if (!m_LotID1.Equals("") || !m_LotID2.Equals("") || !m_LotID3.Equals(""))
            //{
            //    int n = m_SkidRemark.Length;
            //    m_SkidRemark = string.Empty;
            //    m_SkidRemark = "---------  " + m_SkidRemark.PadRight(n, '-') + "\n";
            //    m_PackingRemark = m_SkidRemark + m_PackingRemark;
            //    m_PackingRemark = "Skid ID          Lot ID(Cut ID)\n" + m_PackingRemark + "\n";

            //    if (!BOX001_225.sNote2.ToString().Equals(""))
            //        m_PackingRemark += BOX001_225.sNote2.ToString() + ", ";
            //}

            /// 특이사항 내역 ///
            if ((bool)chkRemark1.IsChecked)
            {
                m_PackingRemark += chkRemark1.Content.ToString();
            }

            if ((bool)chkRemark2.IsChecked)
            {
                if (m_PackingRemark == null)
                {
                    m_PackingRemark += chkRemark2.Content.ToString();
                }
                else
                {
                    m_PackingRemark = m_PackingRemark + ", " + chkRemark2.Content.ToString();
                }
            }

            if ((bool)chkRemark3.IsChecked)
            {
                if (m_PackingRemark == null)
                {
                    m_PackingRemark += chkRemark3.Content.ToString();
                }
                else
                {
                    m_PackingRemark = m_PackingRemark + ", " + chkRemark3.Content.ToString();
                }
            }

            if ((bool)chkRemark4.IsChecked)
            {
                if (m_PackingRemark == null)
                {
                    m_PackingRemark += chkRemark4.Content.ToString();
                }
                else
                {
                    m_PackingRemark = m_PackingRemark + ", " + chkRemark4.Content.ToString();
                }
            }

            m_iFrameLane1 = numLaneQty1.Value;
            m_iFrameLane2 = numLaneQty2.Value;
            m_iFrameLane3 = numLaneQty3.Value;            

            m_dCutM1 = GetPancakeSum2(dgPancakeListSelected01, "M_WIPQTY");
            m_dCutM2 = GetPancakeSum2(dgPancakeListSelected02, "M_WIPQTY");
            m_dCutM3 = GetPancakeSum2(dgPancakeListSelected03, "M_WIPQTY");

            m_dCell1 = GetPancakeSum2(dgPancakeListSelected01, "CELL_WIPQTY");
            m_dCell2 = GetPancakeSum2(dgPancakeListSelected02, "CELL_WIPQTY");
            m_dCell3 = GetPancakeSum2(dgPancakeListSelected03, "CELL_WIPQTY");
        }

        private void InitPackSkidRemake(string sLOTID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SKIDID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SKIDID"] = sLOTID;

            RQSTDT.Rows.Add(dr);

            DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SKID_CUTIDS", "RQSTDT", "RSLTDT", RQSTDT);

            if (SlocIDResult.Rows.Count == 0)
            {
                Util.MessageValidation("SFU2071"); //To_Location 기준정보가 존재하지 않습니다.
                return;
            }
            else
            {
                m_PackingRemark += sLOTID + " " + SlocIDResult.Rows[0]["CUTIDS"].ToString() + "\n";

                if (m_SkidRemark.Length < SlocIDResult.Rows[0]["CUTIDS"].ToString().Length)
                    m_SkidRemark = SlocIDResult.Rows[0]["CUTIDS"].ToString();
            }
        }

        private void PackingCardDataBinding_Config()
        {
            try
            {
                //string PkgWayTemp = string.Empty;
                
                m_TrasferLoc = txtLocation.Text;
                //PkgWayTemp = m_PkgWay;

                if (!Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    m_dCutM1 = Math.Floor(m_dCutM1);
                    m_dCutM2 = Math.Floor(m_dCutM2);
                    m_dCutM3 = Math.Floor(m_dCutM3);
                    m_dCell1 = Math.Floor(m_dCell1);
                    m_dCell2 = Math.Floor(m_dCell2);
                    m_dCell3 = Math.Floor(m_dCell3);
                }
                                
                if ("CNA".Equals(m_ToLoc))
                {
                    PackageWay = PackageWay + " " + "CNA";                    
                }                

                string sUnitCode = string.Empty;
                string sOper_Desc = string.Empty;
                string sAbbrCode = string.Empty;

                // Selected Grid 1,2,3 Merge //
                DataTable dt1 = DataTableConverter.Convert(dgPancakeListSelected01.ItemsSource);
                DataTable dt2 = DataTableConverter.Convert(dgPancakeListSelected02.ItemsSource);
                DataTable dt3 = DataTableConverter.Convert(dgPancakeListSelected03.ItemsSource);
                dt1.Merge(dt2);
                dt1.Merge(dt3);

                /// 유효일자 ///
                DataRow[] Rows = dt1.Select("", "VLD_DATE ASC");
                DateTime TempDate = DateTime.ParseExact(Rows[0]["VLD_DATE"].ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                string ValidateDate = TempDate.ToString("yyyy-MM-dd");

                /// 생산일자 ///
                Rows = dt1.Select("", "WIPSDTTM ASC");
                string WipDate = Convert.ToDateTime(Rows[0]["WIPSDTTM"]).ToString("yyyy-MM-dd");

                // LotID //                                
                DataTable dtMerge = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_CONV_QTY", "RQSTDT", "RSLTDT", dt1, RowSequenceNo:true);

                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("CSTID", typeof(String));
                RQSTDT3.Columns.Add("LANGID", typeof(String));
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));
                
                DataRow dr3 = RQSTDT3.NewRow();
                dr3["CSTID"] = dt1.Rows[0]["CSTID"].ToString();
                dr3["LANGID"] = LoginInfo.LANGID;
                dr3["CMCDTYPE"] = "PRDT_ABBR_CODE";
                RQSTDT3.Rows.Add(dr3);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNITCODE_BY_CUTID", "RQSTDT", "RSLTDT", RQSTDT3);

                if (SearchResult.Rows[0]["UNIT_CODE"].ToString() == "")
                {
                    sUnitCode = null;
                }
                else
                {
                    sUnitCode = SearchResult.Rows[0]["UNIT_CODE"].ToString();
                }

                if (SearchResult.Rows[0]["PRDT_ABBR_CODE"].ToString() == "")
                {
                    sAbbrCode = null;
                }
                else
                {
                    sAbbrCode = SearchResult.Rows[0]["PRDT_ABBR_CODE"].ToString();
                }

                decimal m_dConvCutM1 = 0;
                decimal m_dConvCutM2 = 0;
                decimal m_dConvCutM3 = 0;
                decimal m_dConvCutC1 = 0;
                decimal m_dConvCutC2 = 0;
                decimal m_dConvCutC3 = 0;
                string m_CutStr1 = string.Empty;
                string s_CutStr2 = string.Empty;

                if (string.Equals(sUnitCode, "EA"))
                {
                    string sProdID = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID"));

                    if (!string.IsNullOrEmpty(sProdID))
                    {
                        decimal sConvLength = Util.NVC_Decimal(GetPatternLength(sProdID, m_LotID1));

                        if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                        {

                            m_dConvCutM1 = Util.NVC_Decimal(m_dCutM1) * sConvLength;
                            m_dConvCutM2 = Util.NVC_Decimal(m_dCutM2) * sConvLength;
                            m_dConvCutM3 = Util.NVC_Decimal(m_dCutM3) * sConvLength;
                            m_dConvCutC1 = Util.NVC_Decimal(m_dCell1) * sConvLength;
                            m_dConvCutC2 = Util.NVC_Decimal(m_dCell2) * sConvLength;
                            m_dConvCutC3 = Util.NVC_Decimal(m_dCell3) * sConvLength;
                        }
                        else
                        {
                            m_dConvCutM1 = Math.Floor(Util.NVC_Decimal(m_dCutM1) * sConvLength);
                            m_dConvCutM2 = Math.Floor(Util.NVC_Decimal(m_dCutM2) * sConvLength);
                            m_dConvCutM3 = Math.Floor(Util.NVC_Decimal(m_dCutM3) * sConvLength);
                            m_dConvCutC1 = Math.Floor(Util.NVC_Decimal(m_dCell1) * sConvLength);
                            m_dConvCutC2 = Math.Floor(Util.NVC_Decimal(m_dCell2) * sConvLength);
                            m_dConvCutC3 = Math.Floor(Util.NVC_Decimal(m_dCell3) * sConvLength);
                        }
                    }
                }
                else
                {
                    m_dConvCutC1 = Util.NVC_Decimal(m_dCell1);
                    m_dConvCutC2 = Util.NVC_Decimal(m_dCell2);
                    m_dConvCutC3 = Util.NVC_Decimal(m_dCell3);
                }

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    if (m_dConvCutM1 > 0 || m_dConvCutM2 > 0 || m_dConvCutM3 > 0)
                    {
                        m_CutStr1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2 + m_dCutM3) * 10) / 10) ) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dConvCutM1 + m_dConvCutM2 + m_dConvCutM3) * 10) / 10) ) + "M";
                        s_CutStr2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCell1 + m_dCell2 + m_dCell3) * 10) / 10) ) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dConvCutC1 + m_dConvCutC2 + m_dConvCutC3) * 10) / 10) ) + "M";
                    }
                    else
                    {
                        m_CutStr1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2 + m_dCutM3) * 10) / 10) );
                        s_CutStr2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCell1 + m_dCell2 + m_dCell3) * 10) / 10) );
                    }
                }
                else
                {
                    if (m_dConvCutM1 > 0 || m_dConvCutM2 > 0 || m_dConvCutM3 > 0)
                    {
                        m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1 + m_dCutM2 + m_dCutM3)) + "/" + String.Format("{0:#,##0}", (m_dConvCutM1 + m_dConvCutM2 + m_dConvCutM3)) + "M";
                        s_CutStr2 = String.Format("{0:#,##0}", (m_dCell1 + m_dCell2 + m_dCell3)) + "/" + String.Format("{0:#,##0}", (m_dConvCutC1 + m_dConvCutC2 + m_dConvCutC3)) + "M";
                    }
                    else
                    {
                        m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1 + m_dCutM2 + m_dCutM3));
                        s_CutStr2 = String.Format("{0:#,##0}", (m_dCell1 + m_dCell2 + m_dCell3));
                    }
                }

                if (SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                {
                    sOper_Desc = null;
                }
                else
                {
                    sOper_Desc = SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString();
                }
                
                string sVld = string.Empty;
                string sProdDate = string.Empty;                                          
                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");
                string PackingNo = IsDupplicatedPakcingNo(dt1.Rows[0]["CSTID"].ToString() + "0");
                double TotalLane = numLaneQty1.Value + numLaneQty2.Value + numLaneQty3.Value;
                double TotalCRoll = m_dCutM1 + m_dCutM2 + m_dCutM3;
                double TotalSRoll = m_dCell1 + m_dCell2 + m_dCell3;
                double TotalM = (double)(m_dConvCutC1 + m_dConvCutC2 + m_dConvCutC3);

                dtPackingCard_Merge = new DataTable();
                dtPackingCard_Merge.Columns.Add("Title", typeof(string));
                dtPackingCard_Merge.Columns.Add("MODEL_NAME", typeof(string));
                dtPackingCard_Merge.Columns.Add("PACK_NO", typeof(string));
                dtPackingCard_Merge.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard_Merge.Columns.Add("Transfer", typeof(string));
                dtPackingCard_Merge.Columns.Add("Total_M", typeof(string));
                dtPackingCard_Merge.Columns.Add("Total_Cell", typeof(string));            
                dtPackingCard_Merge.Columns.Add("VLD_DATE", typeof(string));                
                dtPackingCard_Merge.Columns.Add("REG_DATE", typeof(string));                
                dtPackingCard_Merge.Columns.Add("V", typeof(string));                
                dtPackingCard_Merge.Columns.Add("L", typeof(string));                
                dtPackingCard_Merge.Columns.Add("M", typeof(string));                
                dtPackingCard_Merge.Columns.Add("C", typeof(string));                
                dtPackingCard_Merge.Columns.Add("D", typeof(string));                
                dtPackingCard_Merge.Columns.Add("REMARK", typeof(string));
                dtPackingCard_Merge.Columns.Add("UNIT_CODE", typeof(string));
                dtPackingCard_Merge.Columns.Add("V_DATE1", typeof(string));
                dtPackingCard_Merge.Columns.Add("P_DATE1", typeof(string));
                dtPackingCard_Merge.Columns.Add("V_DATE2", typeof(string));
                dtPackingCard_Merge.Columns.Add("P_DATE2", typeof(string));
                dtPackingCard_Merge.Columns.Add("OFFER_DESC", typeof(string));
                dtPackingCard_Merge.Columns.Add("PRODID", typeof(string));                

                DataRow drCrad = null;
                drCrad = dtPackingCard_Merge.NewRow();

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drCrad.ItemArray = new object[] { PackageWay,
                                                  Util.NVC(dt1.Rows[0]["MODLID"]),
                                                  PackingNo,
                                                  "*" + PackingNo + "*",
                                                  Util.NVC(dt1.Rows[0]["EQSGNAME"]) + '\n' +" -> " + m_TrasferLoc,
                                                  (decimal)(Math.Truncate( TotalCRoll * 10) / 10),
                                                  (decimal)(Math.Truncate( TotalSRoll * 10) / 10),
                                                  ValidateDate,
                                                  WipDate,
                                                  Util.NVC(dt1.Rows[0]["PROD_VER_CODE"]),
                                                  String.Format("{0:#,##0}", TotalLane),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalCRoll * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalSRoll * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( TotalM * 10) / 10) ),
                                                  m_PackingRemark,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(dt1.Rows[0]["PRODID"])  + " [ " + sAbbrCode + " ]"
                                               };
                }
                else
                {
                    drCrad.ItemArray = new object[] { PackageWay,
                                                  Util.NVC(dt1.Rows[0]["MODLID"]),
                                                  PackingNo,
                                                  "*" + PackingNo + "*",
                                                  Util.NVC(dt1.Rows[0]["EQSGNAME"]) + '\n' +" -> " + m_TrasferLoc,
                                                  TotalCRoll,
                                                  TotalSRoll,
                                                  ValidateDate,
                                                  WipDate,
                                                  Util.NVC(dt1.Rows[0]["PROD_VER_CODE"]),
                                                  String.Format("{0:#,##0}", TotalLane),
                                                  String.Format("{0:#,##0}", TotalCRoll),
                                                  String.Format("{0:#,##0}", TotalSRoll),
                                                  String.Format("{0:#,##0}", TotalM),
                                                  m_PackingRemark,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(dt1.Rows[0]["PRODID"])  + " [ " + sAbbrCode + " ]"
                                               };
                }

                dtPackingCard_Merge.Rows.Add(drCrad);

                string sSHIPTO_ID = BOX001_225.cboTransLoc2.SelectedValue.ToString();
                string sTO_SLOC_ID = string.Empty;
                string sShopID = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("FROM_AREAID", typeof(String));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SHIPTO_ID"] = sSHIPTO_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_SLOC_ID", "RQSTDT", "RSLTDT", RQSTDT);

                if (SlocIDResult.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2071"); //To_Location 기준정보가 존재하지 않습니다.
                    return;
                }
                else
                {
                    sTO_SLOC_ID = SlocIDResult.Rows[0]["TO_SLOC_ID"].ToString();
                    sShopID = SlocIDResult.Rows[0]["SHOPID"].ToString();
                }

                string sPackNo1 = string.Empty;
                string sPackNo2 = string.Empty;
                string sPackNo3 = string.Empty;

                sPackNo1 = PackingNo + "_01";
                sPackNo2 = PackingNo + "_02";
                sPackNo3 = PackingNo + "_03";

                // 기본 정보 DataTable
                dtBasicInfo = new DataTable();
                dtBasicInfo.TableName = "dtBasicInfo";
                dtBasicInfo.Columns.Add("QTY1", typeof(Decimal));
                dtBasicInfo.Columns.Add("QTY2", typeof(Decimal));
                dtBasicInfo.Columns.Add("PRODID", typeof(string));
                dtBasicInfo.Columns.Add("SHIPTO_ID", typeof(string));
                dtBasicInfo.Columns.Add("FROM_AREAID", typeof(string));
                dtBasicInfo.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtBasicInfo.Columns.Add("TO_SLOC_ID", typeof(string));
                dtBasicInfo.Columns.Add("PACKING_NO", typeof(string));
                dtBasicInfo.Columns.Add("PACK_NO1", typeof(string));
                dtBasicInfo.Columns.Add("PACK_NO2", typeof(string));
                dtBasicInfo.Columns.Add("PACK_NO3", typeof(string));
                dtBasicInfo.Columns.Add("REMARK", typeof(string));
                dtBasicInfo.Columns.Add("PKGWAY", typeof(string));
                dtBasicInfo.Columns.Add("TOTAL_QTY", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY2", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY3", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY4", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY5", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY6", typeof(Decimal));
                dtBasicInfo.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                dtBasicInfo.Columns.Add("TO_SHOPID", typeof(string));
                dtBasicInfo.Columns.Add("TYPE", typeof(string));
                dtBasicInfo.Columns.Add("PROCID", typeof(string));

                DataRow drInfo = dtBasicInfo.NewRow();

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drInfo["QTY1"] = (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2 + m_dCutM3) * 10) / 10);
                    drInfo["QTY2"] = (decimal)(Math.Truncate( (m_dCell1 + m_dCell2 + m_dCell3) * 10) / 10);
                }
                else
                {
                    drInfo["QTY1"] = m_dCutM1 + m_dCutM2 + m_dCutM3;
                    drInfo["QTY2"] = m_dCell1 + m_dCell2 + m_dCell3;
                }

                drInfo["PRODID"] = Util.NVC(dt1.Rows[0]["PRODID"]);
                drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                drInfo["FROM_AREAID"] = Util.NVC(dt1.Rows[0]["FROM_AREAID"]);
                drInfo["FROM_SLOC_ID"] = Util.NVC(dt1.Rows[0]["FROM_SLOC_ID"]);
                drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                drInfo["PACKING_NO"] = PackingNo;
                drInfo["PACK_NO1"] = sPackNo1;
                drInfo["PACK_NO2"] = sPackNo2;
                drInfo["PACK_NO3"] = sPackNo3;
                drInfo["REMARK"] = m_PackingRemark;
                drInfo["PKGWAY"] = m_PkgWay;
                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drInfo["TOTAL_QTY"]  = (decimal)(Math.Truncate( m_dCutM1 * 10) / 10);
                    drInfo["TOTAL_QTY2"] = (decimal)(Math.Truncate( m_dCell1 * 10) / 10);
                    drInfo["TOTAL_QTY3"] = (decimal)(Math.Truncate( m_dCutM2 * 10) / 10);
                    drInfo["TOTAL_QTY4"] = (decimal)(Math.Truncate( m_dCell2 * 10) / 10);
                    drInfo["TOTAL_QTY5"] = (decimal)(Math.Truncate( m_dCutM3 * 10) / 10);
                    drInfo["TOTAL_QTY6"] = (decimal)(Math.Truncate( m_dCell3 * 10) / 10);
                }
                else
                {
                    drInfo["TOTAL_QTY"] = m_dCutM1;
                    drInfo["TOTAL_QTY2"] = m_dCell1;
                    drInfo["TOTAL_QTY3"] = m_dCutM2;
                    drInfo["TOTAL_QTY4"] = m_dCell2;
                    drInfo["TOTAL_QTY5"] = m_dCutM3;
                    drInfo["TOTAL_QTY6"] = m_dCell3;
                }
                drInfo["OWMS_BOX_TYPE_CODE"] = OWMS_Code;
                drInfo["TO_SHOPID"] = sShopID;
                drInfo["TYPE"] = "SHIP";
                drInfo["PROCID"] = "E7000";
                dtBasicInfo.Rows.Add(drInfo);

                // dgPancakeListSelected01 정보 DataTable
                if (dgPancakeListSelected01.GetRowCount() > 0)
                {
                    dtSel01 = new DataTable();
                    dtSel01.TableName = "dtSel01";
                    dtSel01.Columns.Add("LOTID", typeof(string));
                    dtSel01.Columns.Add("M_WIPQTY", typeof(string));
                    dtSel01.Columns.Add("CELL_WIPQTY", typeof(string));

                    for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
                    {
                        DataRow drSel01 = dtSel01.NewRow();

                        drSel01["LOTID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "LOTID");
                        drSel01["M_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "M_WIPQTY");
                        drSel01["CELL_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "CELL_WIPQTY");

                        dtSel01.Rows.Add(drSel01);
                    }
                }

                // dgPancakeListSelected02 정보 DataTable
                if (dgPancakeListSelected02.GetRowCount() > 0)
                {
                    dtSel02 = new DataTable();
                    dtSel02.TableName = "dtSel02";
                    dtSel02.Columns.Add("LOTID", typeof(string));
                    dtSel02.Columns.Add("M_WIPQTY", typeof(string));
                    dtSel02.Columns.Add("CELL_WIPQTY", typeof(string));

                    for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
                    {
                        DataRow drSel02 = dtSel02.NewRow();
                        drSel02["LOTID"] = DataTableConverter.GetValue(dgPancakeListSelected02.Rows[i].DataItem, "LOTID");
                        drSel02["M_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected02.Rows[i].DataItem, "M_WIPQTY");
                        drSel02["CELL_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected02.Rows[i].DataItem, "CELL_WIPQTY");
                        dtSel02.Rows.Add(drSel02);
                    }
                }

                // dgPancakeListSelected03 정보 DataTable
                if (dgPancakeListSelected03.GetRowCount() > 0)
                {
                    dtSel03 = new DataTable();
                    dtSel03.TableName = "dtSel03";
                    dtSel03.Columns.Add("LOTID", typeof(string));
                    dtSel03.Columns.Add("M_WIPQTY", typeof(string));
                    dtSel03.Columns.Add("CELL_WIPQTY", typeof(string));

                    for (int i = 0; i < dgPancakeListSelected03.GetRowCount(); i++)
                    {
                        DataRow drSel03 = dtSel03.NewRow();
                        drSel03["LOTID"] = DataTableConverter.GetValue(dgPancakeListSelected03.Rows[i].DataItem, "LOTID");
                        drSel03["M_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected03.Rows[i].DataItem, "M_WIPQTY");
                        drSel03["CELL_WIPQTY"] = DataTableConverter.GetValue(dgPancakeListSelected03.Rows[i].DataItem, "CELL_WIPQTY");
                        dtSel03.Rows.Add(drSel03);
                    }
                }

                LGC.GMES.MES.BOX001.Report_Packing_Three rs = new LGC.GMES.MES.BOX001.Report_Packing_Three();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[7];
                    Parameters[0] = "PackingCard_New_Four";
                    Parameters[1] = dtPackingCard_Merge;
                    Parameters[2] = dtBasicInfo;
                    Parameters[3] = dtSel01;
                    Parameters[4] = dtSel02;
                    Parameters[5] = dtSel03;
                    Parameters[6] = dtMerge;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private string IsDupplicatedPakcingNo(string sBoxID)
        {
            try
            {
                string sTO_SLOC_ID = string.Empty;
                string sShopID = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sBoxID;

                RQSTDT.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CANCEL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dt.Rows.Count == 0)
                {
                    return sBoxID;
                }
                else
                {
                    return sBoxID.Substring(0, 9) + "Z";
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }            
        }

        private decimal GetPatternLength(string prodID, string sLotID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["PRODID"] = prodID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["LOTID"] = sLotID;
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CONV_RATE_PTN_BY_LOT", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["PTN_LEN"])))
                        return Util.NVC_Decimal(result.Rows[0]["PTN_LEN"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return 1;
        }

        private void Print_Result(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Packing_Three wndPopup = sender as LGC.GMES.MES.BOX001.Report_Packing_Three;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    Util.gridClear(BOX001_225.dgOut);
                    BOX001_225.dgSub.Children.Clear();

                    BOX001_225.dgOut.IsEnabled = true;
                    BOX001_225.txtLotID.IsReadOnly = false;
                    BOX001_225.btnPackOut.IsEnabled = true;
                    BOX001_225.txtLotID.Text = "";
                    BOX001_225.txtLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        //[E20230306-000128]전극수 소수점 개선
        private string GetEltrValueDecimalApplyFlag()
        {

            string sEltrValueDecimalApplyFlag = "N";
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_VALUE_DECIMAL_APPLY_FLAG";
            sCmCode = null;

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrValueDecimalApplyFlag = Util.NVC(dtResult.Rows[0]["ATTR1"].ToString());

                }
                else
                {
                    sEltrValueDecimalApplyFlag = "N";
                }

                return sEltrValueDecimalApplyFlag;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return sEltrValueDecimalApplyFlag;
            }
        }


        #endregion
    }
}
