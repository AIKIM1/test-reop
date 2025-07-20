/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.07.13  김도형     : [E20230306-000128]전극수 소수점 개선
  2024.01.10  김도형     : [E20231122-001031] [NA PI]전극MES시스템의 포장화면 개선건 (BOX001_330->BOX001_330)
 
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

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_330_PACKINGCARD_MERGE : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        public BOX001_330 BOX001_330;

        public DataTable dtPackingCard_Merge;
        public DataTable dtBasicInfo;
        public DataTable dtSel01;
        public DataTable dtSel02;
        public DataTable dtLot;

        Util _Util = new Util();

        private string m_PkgWay = string.Empty;
        private string m_PackingRemark = string.Empty;
        private string m_SkidRemark = string.Empty;
        private double m_iFrameLane1 = 0;
        private double m_iFrameLane2 = 0;
        private double m_dCutM1 = 0;
        private double m_dCutM2 = 0;
        private double m_dCell1 = 0;
        private double m_dCell2 = 0;
        private string m_PackingNo1 = string.Empty;
        private string m_PackingNo2 = string.Empty;
        private string m_TrasferLoc = string.Empty;
        private string m_ToLoc = string.Empty;
        private string m_LotID1 = string.Empty;
        private string m_LotID2 = string.Empty;
        private string m_EltrValueDecimalApplyFlag = string.Empty;  //[E20230306-000128]전극수 소수점 개선

        bool bFormLoad = false;

        public BOX001_330_PACKINGCARD_MERGE()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_EltrValueDecimalApplyFlag = GetEltrValueDecimalApplyFlag(); //[E20230306-000128]전극수 소수점 개선 

            if (bFormLoad == false)
            {
                init_Form();
            }

            if (BOX001_330.bNew_Load_Pack == true)
            {
                init_Form();
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            bFormLoad = true;
            BOX001_330.bNew_Load_Pack = false;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter2 = { "WH_SHIPMENT" };
            String[] sFilter3 = { "WH_TYPE" };

            _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");
        }

        private void init_Form()
        {
            try
            {
                Util.gridClear(dgPancakeListRemain01);
                Util.gridClear(dgPancakeListRemain02);
                Util.gridClear(dgPancakeListSelected01);
                Util.gridClear(dgPancakeListSelected02);

                m_LotID1 = BOX001_330.sTempLot_1_Pack.ToString();
                m_LotID2 = BOX001_330.sTempLot_2_Pack.ToString();

                Search_Lot_List(m_LotID1, dgPancakeListRemain01);

                Search_Lot_List(m_LotID2, dgPancakeListRemain02);

                txtLotID1.Text = m_LotID1;
                txtLotID2.Text = m_LotID2;

                txtLaneQty1.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[0].DataItem, "LANE_QTY"));
                txtLaneQty2.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[1].DataItem, "LANE_QTY"));

                txtCutM1.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[0].DataItem, "M_WIPQTY"));
                txtCutM2.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[1].DataItem, "M_WIPQTY"));

                txtCellM1.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[0].DataItem, "CELL_WIPQTY"));
                txtCellM2.Text = Util.NVC(DataTableConverter.GetValue(BOX001_330.dgOut_Pack.Rows[1].DataItem, "CELL_WIPQTY"));

                numLane_Qty1.Value = dgPancakeListRemain01.GetRowCount();
                numLane_Qty2.Value = dgPancakeListRemain02.GetRowCount();

                txtLocation.Text = BOX001_330.cboTransLoc2_Pack.Text;

                txtSelectedM1.Text = "0";
                txtSelectedM2.Text = "0";

                txtSelectedCell1.Text = "0";
                txtSelectedCell2.Text = "0";

                chkRemark1.IsChecked = false;
                chkRemark2.IsChecked = false;
                chkRemark3.IsChecked = false;
                chkRemark4.IsChecked = false;

                numLaneQty1.Value = 0;
                numLaneQty2.Value = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        
        private void Search_Lot_List(string sLotid, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = sLotid;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELEC_PACKING", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                DataGrid.ItemsSource = DataTableConverter.Convert(SearchResult);

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
            if (cboPackWay.SelectedValue.ToString() == "CRT")
            {
                m_PkgWay = "CRT";
                txtBlock1.Text = "Lane #1";
                txtBlock2.Text = "Lane #1";
            }
            else //Box
            {
                m_PkgWay = "BOX";
                txtBlock1.Text = "BoxLane #1";
                txtBlock2.Text = "BoxLane #1";
            }

            InitLaneList();

            for (int i = 0; i < dgPancakeListRemain01.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain01.Rows[i].DataItem, "POSITION", 0);
                DataTableConverter.SetValue(dgPancakeListRemain01.Rows[i].DataItem, "CHK", true);
            }

            for (int i = 0; i < dgPancakeListRemain02.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain02.Rows[i].DataItem, "POSITION", 0);
                DataTableConverter.SetValue(dgPancakeListRemain02.Rows[i].DataItem, "CHK", true);
            }
        }

        private void InitLaneList()
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeListRemain01);
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeListRemain02);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();
            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            numLane_Qty1.Value = dgPancakeListRemain01.GetRowCount();
            numLane_Qty2.Value = dgPancakeListRemain02.GetRowCount();

            txtSelectedM1.Text = "0";
            txtSelectedM2.Text = "0";

            txtSelectedCell1.Text = "0";
            txtSelectedCell2.Text = "0";
            
        }

        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                //포장카드 구성 버튼 클릭시 Validation
                if (Prevalidation() == true)
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
                else
                {
                    return;
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
            AddToPosition(1, dgPancakeListSelected01, dgPancakeListRemain01, 1);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();
            numLane_Qty1.Value = dgPancakeListRemain01.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);           
        }

        private void btnDelAllFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeListRemain01);

            numLane_Qty1.Value = dgPancakeListRemain01.GetRowCount();
            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);            
        }

        private void btnAddAllToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(2, dgPancakeListSelected02, dgPancakeListRemain02, 2);

            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();
            numLane_Qty2.Value = dgPancakeListRemain02.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected02, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_WIPQTY");
            txtSelectedCell2.Text = Convert.ToString(dCell);            

        }

        private void btnDelAllFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeListRemain02);

            numLane_Qty2.Value = dgPancakeListRemain02.GetRowCount();
            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected02, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_WIPQTY");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int iPosition = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "POSITION").ToString()));

            if (!(bool)(sender as CheckBox).IsChecked && iPosition != 0)
            {
                (sender as CheckBox).IsChecked = true;
            }
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int iPosition = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "POSITION").ToString()));

            if (!(bool)(sender as CheckBox).IsChecked && iPosition != 0)
            {
                (sender as CheckBox).IsChecked = true;
            }
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
        }

        private void CheckBox_Click_3(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
        }

        #endregion

        #region Mehod
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
                dtAdd.Columns.Add("EQSG_NAME", typeof(string));

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
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "EQSG_NAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "EQSG_NAME"));
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
                        DataTableConverter.SetValue(Selected.CurrentRow.DataItem, "EQSG_NAME", DataTableConverter.GetValue(Remain.Rows[i].DataItem, "EQSG_NAME"));
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
            /*
            decimal dSum = 0;

            for (int i = 0; i < DataGrid.Rows.Count; i++)
                if (string.Equals(sName, "M_WIPQTY"))
                    dSum += Math.Round(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)), 0, MidpointRounding.AwayFromZero);
                else if (string.Equals(sName, "CELL_WIPQTY"))
                    dSum += Math.Round(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)), 0, MidpointRounding.AwayFromZero);

            return Convert.ToDouble(dSum);
            */
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

        private void DelFromPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid Selected, C1.WPF.DataGrid.C1DataGrid Remain)
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
                dtAdd.Columns.Add("EQSG_NAME", typeof(string));

                Remain.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < Selected.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CHK").ToString() == "True" &&
                        DataTableConverter.GetValue(Selected.Rows[i].DataItem, "POSITION").ToString() != "ADD")
                    {

                        Remain.IsReadOnly = false;
                        Remain.BeginNewRow();
                        Remain.EndNewRow(true);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CHK", true);
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
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "EQSG_NAME", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "EQSG_NAME"));
                        Remain.IsReadOnly = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < Selected.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(Selected.Rows[i].DataItem, "CHK").ToString() == "True" &&
                        DataTableConverter.GetValue(Selected.Rows[i].DataItem, "POSITION").ToString() != "ADD")
                    {
                        Remain.IsReadOnly = false;
                        Remain.BeginNewRow();
                        Remain.EndNewRow(true);
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "CHK", true);
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
                        DataTableConverter.SetValue(Remain.CurrentRow.DataItem, "EQSG_NAME", DataTableConverter.GetValue(Selected.Rows[i].DataItem, "EQSG_NAME"));
                        Remain.IsReadOnly = true;

                    }
                }
            }

            for (int i = Selected.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;
                if (DataTableConverter.GetValue(Selected.Rows[k].DataItem, "CHK").ToString() == "True" &&
                    DataTableConverter.GetValue(Selected.Rows[k].DataItem, "POSITION").ToString() != "ADD")
                {
                    Selected.IsReadOnly = false;
                    Selected.RemoveRow(k);
                    Selected.IsReadOnly = true;
                }
            }
        }

        private bool Prevalidation()
        {
            bool bResult = true;

            if (rdoFrame1.IsChecked.Value == false)
            {
                Util.MessageValidation("SFU3020"); //포장번호 생성을 위해 적용 Box/가대 수를 선택해주세요.
                return false;
            }

            if (Convert.ToInt32(numLaneQty1.Value) <= 0)
            {
                //%1 Box/가대에 선택된 Lane 이 없습니다.
                Util.MessageValidation("SFU3019", "#1"); //%1 Box/가대에 선택된 Lane 이 없습니다.
                return false;
            }

            if (Convert.ToInt32(numLaneQty2.Value) <= 0)
            {
                Util.MessageValidation("SFU3019", "#2"); //%1 Box/가대에 선택된 Lane 이 없습니다.
                return false;
            }
            return bResult;
        }

        private void InitPackingNo()
        {
            m_PackingRemark = string.Empty;
            m_SkidRemark = string.Empty;

            if (!m_LotID1.Equals(""))
            {
                InitPackSkidRemake(m_LotID1);
            }

            if (!m_LotID2.Equals(""))
            {
                InitPackSkidRemake(m_LotID2);
            }
            if (!m_LotID1.Equals("") || !m_LotID2.Equals(""))
            {
                int n = m_SkidRemark.Length;
                m_SkidRemark = string.Empty;
                m_SkidRemark = "---------  " + m_SkidRemark.PadRight(n, '-') + "\n";
                m_PackingRemark = m_SkidRemark + m_PackingRemark;
                m_PackingRemark = "Skid ID          Lot ID(Cut ID)\n" + m_PackingRemark +"\n";
                if(!BOX001_330.sNote2_Pack.ToString().Equals(""))
                    m_PackingRemark += BOX001_330.sNote2_Pack.ToString() + ", ";
            }

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

            m_PackingNo1 = txtLotID1.Text.ToString().Substring(0, 9) + "0";
            m_PackingNo2 = txtLotID1.Text.ToString().Substring(0, 9) + "0";

            m_dCutM1 = GetPancakeSum2(dgPancakeListSelected01, "M_WIPQTY");
            m_dCutM2 = GetPancakeSum2(dgPancakeListSelected02, "M_WIPQTY");

            m_dCell1 = GetPancakeSum2(dgPancakeListSelected01, "CELL_WIPQTY");
            m_dCell2 = GetPancakeSum2(dgPancakeListSelected02, "CELL_WIPQTY");

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
                if(m_SkidRemark.Length < SlocIDResult.Rows[0]["CUTIDS"].ToString().Length)
                    m_SkidRemark = SlocIDResult.Rows[0]["CUTIDS"].ToString();
            }
        }

        private void PackingCardDataBinding_Config()
        {
            try
            {  
                string PkgWayTemp = string.Empty;
                string sPackageWay = string.Empty;
                string sOWMS_Code = string.Empty;
                string procRemark = "";

                m_TrasferLoc = txtLocation.Text;

                PkgWayTemp = m_PkgWay;

                if (!Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    m_dCutM1 = Math.Floor(m_dCutM1);
                    m_dCutM2 = Math.Floor(m_dCutM2);
                    m_dCell1 = Math.Floor(m_dCell1);
                    m_dCell2 = Math.Floor(m_dCell2);
                }

                sPackageWay = ObjectDic.Instance.GetObjectName("전극포장카드");

                if (m_PkgWay == "CRT")
                {
                    sPackageWay = sPackageWay + " " + ObjectDic.Instance.GetObjectName("가대");
                    sOWMS_Code = "EG";
                }
                else if (m_PkgWay == "BOX")
                {
                    sPackageWay = sPackageWay + " " + "BOX";
                    sOWMS_Code = "EB";
                }

                if ("CNA".Equals(m_ToLoc))
                {
                    sPackageWay = sPackageWay + " " + "CNA";
                }

                string sUnitCode = string.Empty;
                string sOper_Desc = string.Empty;
                string sAbbrCode = string.Empty;

                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("CSTID", typeof(String));
                RQSTDT3.Columns.Add("LANGID", typeof(String));
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["CSTID"] = m_LotID1;
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
                decimal m_dConvCutC1 = 0;
                decimal m_dConvCutC2 = 0;
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
                            m_dConvCutC1 = Util.NVC_Decimal(m_dCell1) * sConvLength;
                            m_dConvCutC2 = Util.NVC_Decimal(m_dCell2) * sConvLength;
                        }
                        else
                        {
                            m_dConvCutM1 = Math.Floor(Util.NVC_Decimal(m_dCutM1) * sConvLength);
                            m_dConvCutM2 = Math.Floor(Util.NVC_Decimal(m_dCutM2) * sConvLength);
                            m_dConvCutC1 = Math.Floor(Util.NVC_Decimal(m_dCell1) * sConvLength);
                            m_dConvCutC2 = Math.Floor(Util.NVC_Decimal(m_dCell2) * sConvLength);
                        }
                    }
                }
                else
                {
                    m_dConvCutC1 = Util.NVC_Decimal(m_dCell1);
                    m_dConvCutC2 = Util.NVC_Decimal(m_dCell2);
                }

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    if (m_dConvCutM1 > 0 || m_dConvCutM2 > 0)
                    {
                        m_CutStr1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2) * 10) / 10) ) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dConvCutM1 + m_dConvCutM2) * 10) / 10) ) + "M";
                        s_CutStr2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCell1 + m_dCell2) * 10) / 10) ) + "/" + String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dConvCutC1 + m_dConvCutC2) * 10) / 10) ) + "M";
                    }
                    else
                    {
                        m_CutStr1 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2) * 10) / 10) );
                        s_CutStr2 = String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( (m_dCell1 + m_dCell2) * 10) / 10) );
                    }
                }
                else
                {
                    if (m_dConvCutM1 > 0 || m_dConvCutM2 > 0)
                    {
                        m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1 + m_dCutM2)) + "/" + String.Format("{0:#,##0}", (m_dConvCutM1 + m_dConvCutM2)) + "M";
                        s_CutStr2 = String.Format("{0:#,##0}", (m_dCell1 + m_dCell2)) + "/" + String.Format("{0:#,##0}", (m_dConvCutC1 + m_dConvCutC2)) + "M";
                    }
                    else
                    {
                        m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1 + m_dCutM2));
                        s_CutStr2 = String.Format("{0:#,##0}", (m_dCell1 + m_dCell2));
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
                //열처리 체크

                DataTable result = new DataTable();
                result.TableName = "RQSTDT";

                result.Columns.Add("LANGID", typeof(String));
                result.Columns.Add("CSTID", typeof(String));
                result.Columns.Add("AREAID", typeof(String));

                DataRow dr4 = result.NewRow();

                dr4["LANGID"] = LoginInfo.LANGID;
                dr4["CSTID"] = m_LotID1 + "," + m_LotID2;
                dr4["AREAID"] = LoginInfo.CFG_AREA_ID;
                result.Rows.Add(dr4);

                DataTable chkResult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_PACK_PROC_ROUTE_CHK", "RQSTDT", "RSLTDT", result);

                if (!chkResult.Rows[0]["PROC_REMARK"].ToString().Equals(""))
                {
                    procRemark = ObjectDic.Instance.GetObjectName(chkResult.Rows[0]["PROC_REMARK"].ToString());
                }
                else
                {
                    procRemark = "";
                }


                string sVld_date = string.Empty;
                string sVld = string.Empty;
                string sVld_date2 = string.Empty;
                string sVld2 = string.Empty;
                string sProdDate = string.Empty;
                string sProdDate2 = string.Empty;

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE")) == "")
                {
                    sVld = null;
                }
                else
                {
                    DataTable dtPancakeListSelected01 = ((DataView)dgPancakeListSelected01.ItemsSource).ToTable();
                    object vdate = dtPancakeListSelected01.Compute("MIN(VLD_DATE)", string.Empty);                    
                    sVld_date = vdate.ToString();
                    //sVld_date = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE"));
                    sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                }

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "VLD_DATE")) == "")
                {
                    sVld2 = null;
                }
                else
                {
                    DataTable dtPancakeListSelected02 = ((DataView)dgPancakeListSelected02.ItemsSource).ToTable();
                    object vdate2 = dtPancakeListSelected02.Compute("MIN(VLD_DATE)", string.Empty);
                    sVld_date2 = vdate2.ToString();
                    //sVld_date2 = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "VLD_DATE"));
                    sVld2 = sVld_date2.ToString().Substring(0, 4) + "-" + sVld_date2.ToString().Substring(4, 2) + "-" + sVld_date2.ToString().Substring(6, 2);
                }

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "WIPSDTTM")) == "")
                {
                    sProdDate = null;
                }
                else
                {
                    DateTime ProdDate = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "WIPSDTTM")));
                    sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");
                }

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "WIPSDTTM")) == "")
                {
                    sProdDate2 = null;
                }
                else
                {
                    DateTime ProdDate2 = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "WIPSDTTM")));
                    sProdDate2 = ProdDate2.Year.ToString() + "-" + ProdDate2.Month.ToString("00") + "-" + ProdDate2.Day.ToString("00");  
                }

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                string sPKG_DATE = DateTime.Now.ToString("yyyy-MM-dd");

                dtPackingCard_Merge = new DataTable();

                dtPackingCard_Merge.Columns.Add("Title", typeof(string));
                dtPackingCard_Merge.Columns.Add("MODEL_NAME", typeof(string));
                dtPackingCard_Merge.Columns.Add("PACK_NO", typeof(string));
                dtPackingCard_Merge.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard_Merge.Columns.Add("Transfer", typeof(string));
                dtPackingCard_Merge.Columns.Add("Total_M", typeof(string));
                dtPackingCard_Merge.Columns.Add("Total_Cell", typeof(string));
                dtPackingCard_Merge.Columns.Add("No1", typeof(string));
                dtPackingCard_Merge.Columns.Add("No2", typeof(string));
                dtPackingCard_Merge.Columns.Add("Lot1", typeof(string));
                dtPackingCard_Merge.Columns.Add("Lot2", typeof(string));
                dtPackingCard_Merge.Columns.Add("VLD_DATE1", typeof(string));
                dtPackingCard_Merge.Columns.Add("VLD_DATE2", typeof(string));
                dtPackingCard_Merge.Columns.Add("REG_DATE1", typeof(string));
                dtPackingCard_Merge.Columns.Add("REG_DATE2", typeof(string));
                dtPackingCard_Merge.Columns.Add("V1", typeof(string));
                dtPackingCard_Merge.Columns.Add("V2", typeof(string));
                dtPackingCard_Merge.Columns.Add("L1", typeof(string));
                dtPackingCard_Merge.Columns.Add("L2", typeof(string));
                dtPackingCard_Merge.Columns.Add("M1", typeof(string));
                dtPackingCard_Merge.Columns.Add("M2", typeof(string));
                dtPackingCard_Merge.Columns.Add("C1", typeof(string));
                dtPackingCard_Merge.Columns.Add("C2", typeof(string));
                dtPackingCard_Merge.Columns.Add("D1", typeof(string));
                dtPackingCard_Merge.Columns.Add("D2", typeof(string));
                dtPackingCard_Merge.Columns.Add("REMARK", typeof(string));
                dtPackingCard_Merge.Columns.Add("UNIT_CODE", typeof(string));
                dtPackingCard_Merge.Columns.Add("V_DATE", typeof(string));
                dtPackingCard_Merge.Columns.Add("P_DATE", typeof(string));
                dtPackingCard_Merge.Columns.Add("OFFER_DESC", typeof(string));
                dtPackingCard_Merge.Columns.Add("PRODID", typeof(string));
                dtPackingCard_Merge.Columns.Add("EQSG_NAME", typeof(string));
                dtPackingCard_Merge.Columns.Add("PROC_REMARK", typeof(string));
                dtPackingCard_Merge.Columns.Add("PKG_DATE", typeof(string));
                DataRow drCrad = null;

                drCrad = dtPackingCard_Merge.NewRow();

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drCrad.ItemArray = new object[] { sPackageWay,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "MODLID")),
                                                  m_PackingNo1,
                                                  "*" + m_PackingNo1 + "*",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSGNAME")) + " -> " + m_TrasferLoc,
                                                  m_CutStr1,
                                                  s_CutStr2,
                                                  "1",
                                                  "2",
                                                  m_LotID1,
                                                  m_LotID2,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PROD_VER_CODE")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "PROD_VER_CODE")),
                                                  m_iFrameLane1,
                                                  m_iFrameLane2,
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dCutM1 * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dCutM2 * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dCell1 * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dCell2 * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dConvCutC1 * 10) / 10) ),
                                                  String.Format("{0:#,##0.#}", (decimal)(Math.Truncate( m_dConvCutC2 * 10) / 10) ),
                                                  m_PackingRemark,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSG_NAME")),
                                                  procRemark,
                                                  sPKG_DATE
                                               };
                }
                else
                {
                    drCrad.ItemArray = new object[] { sPackageWay,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "MODLID")),
                                                  m_PackingNo1,
                                                  "*" + m_PackingNo1 + "*",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSGNAME")) + " -> " + m_TrasferLoc,
                                                  m_CutStr1,
                                                  s_CutStr2,
                                                  "1",
                                                  "2",
                                                  m_LotID1,
                                                  m_LotID2,
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate2,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PROD_VER_CODE")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "PROD_VER_CODE")),
                                                  m_iFrameLane1,
                                                  m_iFrameLane2,
                                                  String.Format("{0:#,##0}", m_dCutM1),
                                                  String.Format("{0:#,##0}", m_dCutM2),
                                                  String.Format("{0:#,##0}", m_dCell1),
                                                  String.Format("{0:#,##0}", m_dCell2),
                                                  String.Format("{0:#,##0}", m_dConvCutC1),
                                                  String.Format("{0:#,##0}", m_dConvCutC2),
                                                  m_PackingRemark,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSG_NAME")),
                                                  procRemark,
                                                  sPKG_DATE
                                               };
                }
                dtPackingCard_Merge.Rows.Add(drCrad);

                string sSHIPTO_ID = BOX001_330.cboTransLoc2_Pack.SelectedValue.ToString();
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

                sPackNo1 = m_PackingNo1 + "_01";
                sPackNo2 = m_PackingNo1 + "_02";

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
                dtBasicInfo.Columns.Add("REMARK", typeof(string));
                dtBasicInfo.Columns.Add("PKGWAY", typeof(string));
                dtBasicInfo.Columns.Add("TOTAL_QTY", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY2", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY3", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY4", typeof(Decimal));
                dtBasicInfo.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                dtBasicInfo.Columns.Add("TO_SHOPID", typeof(string));
                dtBasicInfo.Columns.Add("TYPE", typeof(string));
                dtBasicInfo.Columns.Add("PROCID", typeof(string));
                dtBasicInfo.Columns.Add("SKID_NOTE", typeof(string));

                DataRow drInfo = dtBasicInfo.NewRow();
                //drInfo["QTY1"] = Convert.ToDecimal(txtSelectedM1.Text.ToString()) + Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString()) + Convert.ToDecimal(txtSelectedCell2.Text.ToString());

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drInfo["QTY1"] = (decimal)(Math.Truncate( (m_dCutM1 + m_dCutM2) * 10) / 10);
                    drInfo["QTY2"] = (decimal)(Math.Truncate( (m_dCell1 + m_dCell2) * 10) / 10);
                }
                else
                {
                    drInfo["QTY1"] = m_dCutM1 + m_dCutM2;
                    drInfo["QTY2"] = m_dCell1 + m_dCell2;
                }

                drInfo["PRODID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID");
                drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                drInfo["FROM_AREAID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_AREAID");
                drInfo["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_SLOC_ID");
                drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                drInfo["PACKING_NO"] = m_PackingNo1;
                drInfo["PACK_NO1"] = sPackNo1;
                drInfo["PACK_NO2"] = sPackNo2;
                drInfo["REMARK"] = m_PackingRemark;
                drInfo["PKGWAY"] = m_PkgWay;
                //drInfo["TOTAL_QTY"] = Convert.ToDecimal(txtSelectedM1.Text.ToString());
                //drInfo["TOTAL_QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString());
                //drInfo["TOTAL_QTY3"] = Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["TOTAL_QTY4"] = Convert.ToDecimal(txtSelectedCell2.Text.ToString());

                if (Util.NVC(m_EltrValueDecimalApplyFlag).Equals("Y"))  //[E20230306-000128]전극수 소수점 개선
                {
                    drInfo["TOTAL_QTY"]  = (decimal)(Math.Truncate( m_dCutM1 * 10) / 10);
                    drInfo["TOTAL_QTY2"] = (decimal)(Math.Truncate( m_dCell1 * 10) / 10);
                    drInfo["TOTAL_QTY3"] = (decimal)(Math.Truncate( m_dCutM2 * 10) / 10);
                    drInfo["TOTAL_QTY4"] = (decimal)(Math.Truncate( m_dCell2 * 10) / 10);
                }
                else
                {
                    drInfo["TOTAL_QTY"] = m_dCutM1;
                    drInfo["TOTAL_QTY2"] = m_dCell1;
                    drInfo["TOTAL_QTY3"] = m_dCutM2;
                    drInfo["TOTAL_QTY4"] = m_dCell2;
                }

                drInfo["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;
                drInfo["TO_SHOPID"] = sShopID;
                drInfo["TYPE"] = "SHIP";
                drInfo["PROCID"] = "E7000";
                drInfo["SKID_NOTE"] = BOX001_330.txtComment_Pack.Text;

                dtBasicInfo.Rows.Add(drInfo);

                // dgPancakeListSelected01 정보 DataTable
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
                else
                {
                    dtSel02 = null;
                }

                // C20210826-000420 [LGESNJ 生?PI]??ElectrodePack'g Card 改善
                DataTable RQSTDT2 = new DataTable();
                RQSTDT2.TableName = "RQSTDT";
                RQSTDT2.Columns.Add("LANGID", typeof(string));
                RQSTDT2.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT2.Columns.Add("CMCODE", typeof(string));

                DataRow dr2 = RQSTDT2.NewRow();
                dr2["LANGID"] = LoginInfo.LANGID;
                dr2["CMCDTYPE"] = "ELEC_PACKING_CARD";
                dr2["CMCODE"] = LoginInfo.CFG_SHOP_ID;
                RQSTDT2.Rows.Add(dr2);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT2);

                string sReportCardName = string.Empty;

                if (dtResult.Rows.Count > 0)
                {
                    sReportCardName = "PackingCard_New_NJ";
                }
                else
                {
                    sReportCardName = "PackingCard_New";
                }
                ///////////////////////////////////////////////////////////////

                LGC.GMES.MES.BOX001.Report_Packing rs = new LGC.GMES.MES.BOX001.Report_Packing();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    // C20210826-000420 [LGESNJ 生?PI]??ElectrodePack'g Card 改善
                    //Parameters[0] = "PackingCard_New";
                    ///////////////////////////////////////////////////////////////
                    Parameters[0] = sReportCardName;
                    Parameters[1] = dtPackingCard_Merge;
                    Parameters[2] = dtBasicInfo;
                    Parameters[3] = dtSel01;
                    Parameters[4] = dtSel02;

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
                LGC.GMES.MES.BOX001.Report_Packing wndPopup = sender as LGC.GMES.MES.BOX001.Report_Packing;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    Util.gridClear(BOX001_330.dgOut_Pack);
                    BOX001_330.dgSub_Pack.Children.Clear();

                    BOX001_330.dgOut_Pack.IsEnabled = true;
                    BOX001_330.txtLotID_Pack.IsReadOnly = false;
                    BOX001_330.btnPackOut_Pack.IsEnabled = true;
                    BOX001_330.txtLotID_Pack.Text = "";
                    BOX001_330.txtLotID_Pack.Focus();
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
