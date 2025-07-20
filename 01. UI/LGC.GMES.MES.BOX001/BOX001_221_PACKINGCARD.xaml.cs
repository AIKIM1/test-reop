/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
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
    public partial class BOX001_221_PACKINGCARD : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        public BOX001_221 BOX001_221;

        public DataTable dtPackingCard;
        public DataTable dtBasicInfo;
        public DataTable dtSel01;
        public DataTable dtSel02;
        public DataTable dtLot;

        Util _Util = new Util();

        private string m_LotID = string.Empty;
        private string m_PackingNo = string.Empty;      //포장NO : BOXID
        //private string m_PackingNo2 = string.Empty;
        private string m_TrasferLoc = string.Empty;
        private string m_PkgWay = string.Empty;

        private double m_iFrameLane1 = 0;               //1가대의 Lane수
        private double m_iFrameLane2 = 0;               //2가대의 Lane수
        private double m_dCutM1 = 0;                    //1개대의 M_WIPQTY 의 SUM
        private double m_dCutM2 = 0;                    //2가대의 M_WIPQTY 의 SUM
        private double m_dCell1 = 0;                    //1가대의 CELL_WIPQTY 의 SUM
        private double m_dCell2 = 0;                    //2가대의 CELL_WIPQTY 의 SUM
        private string m_PackingRemark1 = string.Empty; //1가대의 REMARK
        private string m_PackingRemark2 = string.Empty; //2가대의 REMARK    
           
        private string m_Status = string.Empty; 
        private string m_ToLoc = string.Empty;

        private string m_ProcID = string.Empty;

        bool bFormLoad = false;


        public BOX001_221_PACKINGCARD()
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
            if (bFormLoad == false)
            {
                init_Form();
            }

            if(BOX001_221.bNew_Load == true)
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
            BOX001_221.bNew_Load = false;
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

                Util.gridClear(dgPancakeListRemain);
                Util.gridClear(dgPancakeListSelected01);
                Util.gridClear(dgPancakeListSelected02);

                m_ProcID = BOX001_221.sProcid.ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("PROCID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LOTID"] = BOX001_221.sTempLot_1.ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROCID"] = m_ProcID;
                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_PACKING", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");   //재공 정보가 없습니다.
                    return;
                }

                numLaneQty.Value = SearchResult.Rows.Count;

                dgPancakeListRemain.ItemsSource = DataTableConverter.Convert(SearchResult);

                //txtLotID.Text = BOX001_221.sTempLot_1.ToString(); //필요없음

                txtLocation.Text = BOX001_221.cboTransLoc2.Text;

                txtLaneQty.Text = Util.NVC(BOX001_221.dgOut.GetRowCount()); //양품 LANE

                double m_wipqty = 0;
                double cell_wipqty = 0;

                for(int i = 0; i < BOX001_221.dgOut.GetRowCount(); i++)
                {
                    m_wipqty += Convert.ToDouble(DataTableConverter.GetValue(BOX001_221.dgOut.Rows[i].DataItem, "M_WIPQTY"));
                    cell_wipqty += Convert.ToDouble(DataTableConverter.GetValue(BOX001_221.dgOut.Rows[i].DataItem, "CELL_WIPQTY"));
                }

                txtCutM.Text = m_wipqty.ToString("###,###.##"); // C/ROLL

                txtCellM.Text = cell_wipqty.ToString("###,###.##"); // S/ROLL

                txtSelectedM1.Text = "0";
                txtSelectedM2.Text = "0";

                txtSelectedCell1.Text = "0";
                txtSelectedCell2.Text = "0";

                //chkRemark1.IsChecked = false;
                //chkRemark2.IsChecked = false;
                //chkRemark3.IsChecked = false;
                //chkRemark4.IsChecked = false;

                numLaneQty1.Value = 0;
                numLaneQty2.Value = 0;

                rdoFrame1.IsChecked = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        #endregion

        #region Event
        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //포장카드 구성 버튼 클릭시 Validation
                if (Prevalidation() == true)
                {
                    //BOXNO 발번 호출
                    m_PackingNo = getBoxID();

                    if (m_PackingNo.Length == 0)
                    {
                        Util.MessageValidation(""); //BOXID 발번이 정상적으로 이뤄지지 않았습니다.
                        return;
                    }

                    //포장카드 Remark, 수량 
                    InitPackingRemark();

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

                    if (m_PkgWay == "CRT" && rdoFrame2.IsChecked == true)
                    {
                        PackingCardDataBinding_2CRT();
                    }
                    else
                    {
                        PackingCardDataBinding_Config();
                    }
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

        private void InitLaneList()
        {
            DelFromPosition(0, dgPancakeListSelected01);
            DelFromPosition(0, dgPancakeListSelected02);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();
            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            txtSelectedM1.Text = "0";
            txtSelectedM2.Text = "0";

            txtSelectedCell1.Text = "0";
            txtSelectedCell2.Text = "0";

            numLaneQty.Value = dgPancakeListRemain.GetRowCount();
        }

        private void rdoFrame1_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListSelected01.Rows[i].DataItem, "CHK", true);
            }

            for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListSelected02.Rows[i].DataItem, "CHK", true);
            }


            InitLaneList();

            for (int i = 0; i < dgPancakeListRemain.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION", 0);
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK", true);
            }

            dgPancakeListSelected02.IsEnabled = false;
            btnAddAllToPosition02.IsEnabled = false;
            btnDelAllFromPosition02.IsEnabled = false;
        }

        private void rdoFrame2_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListSelected01.Rows[i].DataItem, "CHK", true);
            }

            for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListSelected02.Rows[i].DataItem, "CHK", true);
            }

            InitLaneList();

            for (int i = 0; i < dgPancakeListRemain.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION", 0);
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK", true);
            }

            dgPancakeListSelected02.IsEnabled = true;
            btnAddAllToPosition02.IsEnabled = true;
            btnDelAllFromPosition02.IsEnabled = true;
        }

        private void btnAddAllToPosition01_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected01, 1);

            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);

            numLaneQty.Value = dgPancakeListRemain.GetRowCount();
        }

        private void btnDelAllFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01);

            numLaneQty.Value = dgPancakeListRemain.GetRowCount();
            numLaneQty1.Value = dgPancakeListSelected01.GetRowCount();            

            double dM = GetPancakeSum(dgPancakeListSelected01, "M_WIPQTY");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_WIPQTY");
            txtSelectedCell1.Text = Convert.ToString(dCell);            
        }

        private void btnAddAllToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(2, dgPancakeListSelected02, 2);

            numLaneQty2.Value = dgPancakeListSelected02.GetRowCount();

            double dM = GetPancakeSum(dgPancakeListSelected02, "M_WIPQTY");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_WIPQTY");
            txtSelectedCell2.Text = Convert.ToString(dCell);
            
            numLaneQty.Value = dgPancakeListRemain.GetRowCount();
        }

        private void btnDelAllFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02);

            numLaneQty.Value = dgPancakeListRemain.GetRowCount();
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
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
        }

        private void cboPackWay_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboPackWay.SelectedValue.ToString() == "CRT")
            {
                m_PkgWay = "CRT";
                rdoFrame1.IsEnabled = true;
                rdoFrame2.IsEnabled = true;
                rdoFrame1.IsChecked = true;
                rdoFrame2.IsChecked = false;
                numLaneQty1.IsEnabled = false;
                numLaneQty2.IsEnabled = false;
                txtBlock1.Text = "#1 Lane";
                txtBlock2.Text = "#2 Lane";
            }
            else //Box
            {
                m_PkgWay = "BOX";
                rdoFrame1.IsEnabled = false;
                rdoFrame2.IsEnabled = false;
                rdoFrame1.IsChecked = true;
                rdoFrame2.IsChecked = false;
                numLaneQty1.IsEnabled = false;
                numLaneQty2.IsEnabled = false;
                txtBlock1.Text = "BOX Lane";
                txtBlock2.Text = "BOX Lane";
            }

            numLaneQty1.IsEnabled = false;
            dgPancakeListSelected02.IsEnabled = false;
            btnAddAllToPosition02.IsEnabled = false;
            btnDelAllFromPosition02.IsEnabled = false;

            InitLaneList();

            for (int i = 0; i < dgPancakeListRemain.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION", 0);
                DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK", true);
            }
        }

        #endregion

        #region Mehod
        private void AddToPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid DataGrid, int iPosition)
        {
            if (DataGrid.Rows.Count == 0)
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
                dtAdd.Columns.Add("PRJT_NAME", typeof(string));
                dtAdd.Columns.Add("ELEC", typeof(string));

                DataGrid.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < dgPancakeListRemain.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK").ToString() == "True")
                    {

                        DataGrid.IsReadOnly = false;
                        DataGrid.BeginNewRow();
                        DataGrid.EndNewRow(true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PRJT_NAME", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PRJT_NAME"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "ELEC", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "ELEC"));
                        DataGrid.IsReadOnly = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < dgPancakeListRemain.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataGrid.IsReadOnly = false;
                        DataGrid.BeginNewRow();
                        DataGrid.EndNewRow(true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PRJT_NAME", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PRJT_NAME"));
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "ELEC", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "ELEC"));
                        DataGrid.IsReadOnly = true;

                    }
                }
            }

            for (int i = dgPancakeListRemain.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;
                if (DataTableConverter.GetValue(dgPancakeListRemain.Rows[k].DataItem, "CHK").ToString() == "True")
                {
                    dgPancakeListRemain.IsReadOnly = false;
                    dgPancakeListRemain.RemoveRow(k);
                    dgPancakeListRemain.IsReadOnly = true;
                }
            }
        }

        private void DelFromPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
             if (DataGrid.Rows.Count == 0)
            {
                return;
            }

            if (dgPancakeListRemain.Rows.Count == 0)
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
                dtAdd.Columns.Add("PRJT_NAME", typeof(string));
                dtAdd.Columns.Add("ELEC", typeof(string));

                dgPancakeListRemain.ItemsSource = DataTableConverter.Convert(dtAdd);

                for (int i = 0; i < DataGrid.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CHK").ToString() == "True" &&
                        DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "POSITION").ToString() != "ADD")
                    {

                        dgPancakeListRemain.IsReadOnly = false;
                        dgPancakeListRemain.BeginNewRow();
                        dgPancakeListRemain.EndNewRow(true);
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PRJT_NAME", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PRJT_NAME"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "ELEC", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "ELEC"));
                        dgPancakeListRemain.IsReadOnly = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < DataGrid.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CHK").ToString() == "True" &&
                        DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "POSITION").ToString() != "ADD")
                    {
                        dgPancakeListRemain.IsReadOnly = false;
                        dgPancakeListRemain.BeginNewRow();
                        dgPancakeListRemain.EndNewRow(true);
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "LOTID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "LOTID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "M_WIPQTY", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "M_WIPQTY"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "CELL_WIPQTY", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CELL_WIPQTY"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PRODID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PRODID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "POSITION", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "POSITION"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "EQSGNAME", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "EQSGNAME"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "WIPSDTTM", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "WIPSDTTM"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PROD_VER_CODE", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PROD_VER_CODE"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "VLD_DATE", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "VLD_DATE"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_AREAID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_AREAID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_SHOPID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_SHOPID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "FROM_SLOC_ID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "FROM_SLOC_ID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "MODLID", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "MODLID"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "PRJT_NAME", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PRJT_NAME"));
                        DataTableConverter.SetValue(dgPancakeListRemain.CurrentRow.DataItem, "ELEC", DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "ELEC"));
                        dgPancakeListRemain.IsReadOnly = true;

                    }
                }
            }

            for (int i = DataGrid.GetRowCount(); i > 0; i--)
            {
                int k = 0;
                k = i - 1;
                if (DataTableConverter.GetValue(DataGrid.Rows[k].DataItem, "CHK").ToString() == "True" &&
                    DataTableConverter.GetValue(DataGrid.Rows[k].DataItem, "POSITION").ToString() != "ADD")
                {
                    DataGrid.IsReadOnly = false;
                    DataGrid.RemoveRow(k);
                    DataGrid.IsReadOnly = true;
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
                if (string.Equals(sName, "M_WIPQTY"))
                    dSum += Math.Floor(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)));
                else if (string.Equals(sName, "CELL_WIPQTY"))
                    dSum += Math.Floor(Util.NVC_Decimal(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, sName)));

            return Convert.ToDouble(dSum);
        }

        private bool Prevalidation()
        {
            bool bResult = true;

            if (Convert.ToInt32(numLaneQty1.Value) == 0)
            {
                Util.MessageValidation("SFU3019", "#1"); //%1 Box/가대에 선택된 Lane 이 없습니다.
                return false;
            }

            if (rdoFrame2.IsChecked == true && Convert.ToInt32(numLaneQty2.Value) == 0)
            {
                Util.MessageValidation("SFU3019", "#2"); //%1 Box/가대에 선택된 Lane 이 없습니다.
                return false;
            }

            if (rdoFrame1.IsChecked.Value == false && rdoFrame2.IsChecked.Value == false)
            {
                Util.MessageValidation("SFU3020"); //포장번호 생성을 위해 적용 Box/가대 수를 선택해주세요.
                return false;
            }

            return bResult;
        }

        /// <summary>
        /// Packing Card 변수 정리
        /// </summary>
        private void InitPackingRemark()
        {
            m_PackingRemark1 = string.Empty;
            m_PackingRemark2 = string.Empty;
            string m_gubun = string.Empty;

            m_gubun = "-------------  -----------  ";

            if (dgPancakeListSelected01.GetRowCount() > 0) //1가대의 REMARK
            {
                m_PackingRemark1 = m_gubun + m_PackingRemark1;
                m_PackingRemark1 = "   Box ID                 Lot ID\n" + m_PackingRemark1 + "\n";

                for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
                {
                    m_PackingRemark1 = m_PackingRemark1 + m_PackingNo + "_01  " + DataTableConverter.GetValue(dgPancakeListSelected01.Rows[i].DataItem, "LOTID").ToString() + "\n";
                }
            }

            if (dgPancakeListSelected02.GetRowCount() > 0) //2가대의 REMARK
            {
                m_PackingRemark2 = m_gubun + m_PackingRemark2;
                m_PackingRemark2 = "   Box ID                 Lot ID\n" + m_PackingRemark2 + "\n";

                for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
                {
                    m_PackingRemark2 = m_PackingRemark2 + m_PackingNo + "_02  " + DataTableConverter.GetValue(dgPancakeListSelected02.Rows[i].DataItem, "LOTID").ToString() + "\n";
                }
            }

            if (m_PkgWay == "CRT" && rdoFrame2.IsChecked == true) //2가대 포장의 LANE, 수량
            {
                //1가대
                m_iFrameLane1 = numLaneQty1.Value;
                m_dCutM1 = GetPancakeSum2(dgPancakeListSelected01, "M_WIPQTY");
                m_dCell1 = GetPancakeSum2(dgPancakeListSelected01, "CELL_WIPQTY");

                //2가대
                m_iFrameLane2 = numLaneQty2.Value;
                m_dCutM2 = GetPancakeSum2(dgPancakeListSelected02, "M_WIPQTY");
                m_dCell2 = GetPancakeSum2(dgPancakeListSelected02, "CELL_WIPQTY");
            }
            else //1가대만 포장의 LANE, 수량
            {
                //1가대
                m_iFrameLane1 = numLaneQty1.Value;
                m_dCutM1 = GetPancakeSum2(dgPancakeListSelected01, "M_WIPQTY");
                m_dCell1 = GetPancakeSum2(dgPancakeListSelected01, "CELL_WIPQTY");

                //2가대
                m_iFrameLane2 = 0;
                m_dCutM2 = 0;
                m_dCell2 = 0;
            }
        }

        private string getBoxID()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("SHOPID", typeof(String));
            RQSTDT.Columns.Add("AREAID", typeof(String));
            RQSTDT.Columns.Add("USERID", typeof(String));

            DataRow dr = RQSTDT.NewRow();
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["USERID"] = LoginInfo.USERID;

            RQSTDT.Rows.Add(dr);

            DataTable SlocIDResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_BOXID", "RQSTDT", "RSLTDT", RQSTDT);
           
            return SlocIDResult.Rows[0][0] == null ? "" : SlocIDResult.Rows[0][0].ToString();
        }

        private bool IsDupplicatedPakcingNo(string sBoxID)
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
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void PackingCardDataBinding_Config()
        {
            try
            {
                string PkgWayTemp = string.Empty;
                string sPackageWay = string.Empty;
                string sOWMS_Code = string.Empty;

                //m_TrasferLoc = cboTransLoc.Text;
                m_TrasferLoc = txtLocation.Text;

                PkgWayTemp = m_PkgWay;

                m_Status = "O";

                m_dCutM1 = Math.Floor(m_dCutM1);
                m_dCutM2 = Math.Floor(m_dCutM2);
                m_dCell1 = Math.Floor(m_dCell1);
                m_dCell2 = Math.Floor(m_dCell2);

                m_LotID = BOX001_221.sRepre_Lotid;

                sPackageWay = ObjectDic.Instance.GetObjectName("조립포장카드");

                if (m_PkgWay == "CRT")
                {
                    if (rdoFrame1.IsChecked == true)
                    {
                        sPackageWay = sPackageWay + " " + ObjectDic.Instance.GetObjectName("1가대");
                        sOWMS_Code = "EG";
                    }
                    else if (rdoFrame2.IsChecked == true)
                    {
                        sPackageWay = sPackageWay + " " + ObjectDic.Instance.GetObjectName("2가대");
                        sOWMS_Code = "EG";
                    }
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
                RQSTDT3.Columns.Add("LOTID", typeof(String));
                RQSTDT3.Columns.Add("LANGID", typeof(String));
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["LOTID"] = m_LotID;
                dr3["LANGID"] = LoginInfo.LANGID;
                dr3["CMCDTYPE"] = "PRDT_ABBR_CODE";

                RQSTDT3.Rows.Add(dr3);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNITCODE_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT3);

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
                        decimal sConvLength = Util.NVC_Decimal(GetPatternLength(sProdID));

                        m_dConvCutM1 = Math.Floor((Util.NVC_Decimal(m_dCutM1) * sConvLength) / 1000);
                        m_dConvCutM2 = Math.Floor((Util.NVC_Decimal(m_dCutM2) * sConvLength) / 1000);
                        m_dConvCutC1 = Math.Floor((Util.NVC_Decimal(m_dCell1) * sConvLength) / 1000);
                        m_dConvCutC2 = Math.Floor((Util.NVC_Decimal(m_dCell2) * sConvLength) / 1000);
                    }
                }
                else
                {
                    m_dConvCutC1 = Util.NVC_Decimal(m_dCell1);
                    m_dConvCutC2 = Util.NVC_Decimal(m_dCell2);
                }

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

                if (SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                {
                    sOper_Desc = null;
                }
                else
                {
                    sOper_Desc = SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString();
                }

                string sVld_date = string.Empty;
                string sVld = string.Empty;
                string sProdDate = string.Empty;

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE")) == "")
                {
                    sVld = null;
                }
                else
                {
                    sVld_date = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE"));
                    sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
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

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                dtPackingCard = new DataTable();

                dtPackingCard.Columns.Add("Title", typeof(string));
                dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                dtPackingCard.Columns.Add("PRJT_NAME", typeof(string));
                dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard.Columns.Add("Transfer", typeof(string));
                dtPackingCard.Columns.Add("Total_M", typeof(string));
                dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                dtPackingCard.Columns.Add("No1", typeof(string));
                dtPackingCard.Columns.Add("No2", typeof(string));
                dtPackingCard.Columns.Add("Lot1", typeof(string));
                dtPackingCard.Columns.Add("Lot2", typeof(string));
                dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                dtPackingCard.Columns.Add("V1", typeof(string));
                dtPackingCard.Columns.Add("V2", typeof(string));
                dtPackingCard.Columns.Add("L1", typeof(string));
                dtPackingCard.Columns.Add("L2", typeof(string));
                dtPackingCard.Columns.Add("M1", typeof(string));
                dtPackingCard.Columns.Add("M2", typeof(string));
                dtPackingCard.Columns.Add("C1", typeof(string));
                dtPackingCard.Columns.Add("C2", typeof(string));
                dtPackingCard.Columns.Add("D1", typeof(string));
                dtPackingCard.Columns.Add("D2", typeof(string));
                dtPackingCard.Columns.Add("REMARK", typeof(string));
                dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                dtPackingCard.Columns.Add("V_DATE", typeof(string));
                dtPackingCard.Columns.Add("P_DATE", typeof(string));
                dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                dtPackingCard.Columns.Add("ELEC", typeof(string));
                dtPackingCard.Columns.Add("PRODID", typeof(string));

                DataRow drCrad = null;

                drCrad = dtPackingCard.NewRow();

                drCrad.ItemArray = new object[] { sPackageWay,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "MODLID")),
                                                  "/" + Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRJT_NAME")),
                                                  m_PackingNo,
                                                  "*" + m_PackingNo + "*",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSGNAME")) + " -> " + m_TrasferLoc,
                                                  m_CutStr1 ,
                                                  s_CutStr2,
                                                  "1",
                                                  "",
                                                  m_PackingNo + "_01",
                                                  "",
                                                  sVld,
                                                  "",
                                                  sProdDate,
                                                  "",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PROD_VER_CODE")),
                                                  "",
                                                  m_iFrameLane1 + m_iFrameLane2,
                                                  "",
                                                  String.Format("{0:#,##0}", m_dCutM1 + m_dCutM2),
                                                  "",
                                                  String.Format("{0:#,##0}", m_dCell1 + m_dCell2),
                                                  "",
                                                  String.Format("{0:#,##0}", m_dConvCutC1 + m_dConvCutC2),
                                                  "",
                                                  m_PackingRemark1,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "ELEC")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                                };

                dtPackingCard.Rows.Add(drCrad);

                string sSHIPTO_ID = BOX001_221.cboTransLoc2.SelectedValue.ToString();
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

                sPackNo1 = m_PackingNo + "_01";
                //sPackNo2 = m_PackingNo + "_02";

                // 기본 정보 DataTable
                dtBasicInfo = new DataTable();
                dtBasicInfo.TableName = "dtBasicInfo";
                dtBasicInfo.Columns.Add("INBOXID", typeof(string));
                dtBasicInfo.Columns.Add("OUTBOXID", typeof(string));
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

                DataRow drInfo = dtBasicInfo.NewRow();
                drInfo["INBOXID"] = sPackNo1;
                drInfo["OUTBOXID"] = m_PackingNo;
                //drInfo["QTY1"] = Convert.ToDecimal(txtSelectedM1.Text.ToString()) + Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString()) + Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                drInfo["QTY1"] = m_dCutM1 + m_dCutM2;
                drInfo["QTY2"] = m_dCell1 + m_dCell2;
                drInfo["PRODID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID");
                drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                drInfo["FROM_AREAID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_AREAID");
                drInfo["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_SLOC_ID");
                drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                drInfo["PACKING_NO"] = m_PackingNo;
                drInfo["PACK_NO1"] = sPackNo1;
                drInfo["PACK_NO2"] = sPackNo2;
                drInfo["REMARK"] = m_PackingRemark1;
                drInfo["PKGWAY"] = m_PkgWay;
                //drInfo["TOTAL_QTY"] = Convert.ToDecimal(txtSelectedM1.Text.ToString());
                //drInfo["TOTAL_QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString());
                //drInfo["TOTAL_QTY3"] = Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["TOTAL_QTY4"] = Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                drInfo["TOTAL_QTY"] = m_dCutM1;
                drInfo["TOTAL_QTY2"] = m_dCell1;
                drInfo["TOTAL_QTY3"] = m_dCutM2;
                drInfo["TOTAL_QTY4"] = m_dCell2;
                drInfo["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;
                drInfo["TO_SHOPID"] = sShopID;
                drInfo["TYPE"] = "SHIP";
                drInfo["PROCID"] = m_ProcID;

                dtBasicInfo.Rows.Add(drInfo);

                dtSel01 = new DataTable();
                dtSel01.TableName = "dtSel01";
                dtSel01.Columns.Add("INBOXID", typeof(string));
                dtSel01.Columns.Add("LOTID", typeof(string));
                dtSel01.Columns.Add("M_WIPQTY", typeof(string));
                dtSel01.Columns.Add("CELL_WIPQTY", typeof(string));


                for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
                {
                    DataRow drSel01 = dtSel01.NewRow();

                    drSel01["INBOXID"] = sPackNo1;
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
                    dtSel02.Columns.Add("INBOXID", typeof(string));
                    dtSel02.Columns.Add("LOTID", typeof(string));
                    dtSel02.Columns.Add("M_WIPQTY", typeof(string));
                    dtSel02.Columns.Add("CELL_WIPQTY", typeof(string));

                    for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
                    {
                        DataRow drSel02 = dtSel02.NewRow();

                        drSel02["INBOXID"] = sPackNo2;
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

                LGC.GMES.MES.BOX001.Report_Packing rs = new LGC.GMES.MES.BOX001.Report_Packing();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = "PackingCard_Assy";
                    Parameters[1] = dtPackingCard;
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }


        private void PackingCardDataBinding_2CRT()
        {
            try
            {
                string PkgWayTemp = string.Empty;
                string sPackageWay = string.Empty;
                string sPackageWay2 = string.Empty;
                string sOWMS_Code = string.Empty;

                m_TrasferLoc = txtLocation.Text;

                PkgWayTemp = m_PkgWay;

                m_Status = "O";

                m_dCutM1 = Math.Floor(m_dCutM1);
                m_dCutM2 = Math.Floor(m_dCutM2);
                m_dCell1 = Math.Floor(m_dCell1);
                m_dCell2 = Math.Floor(m_dCell2);

                m_LotID = BOX001_221.sRepre_Lotid.ToString();

                string sPackage = ObjectDic.Instance.GetObjectName("조립포장카드");

                if (m_PkgWay == "CRT")
                {
                    sPackageWay = sPackage + " " + ObjectDic.Instance.GetObjectName("1가대");
                    sPackageWay2 = sPackage + " " + ObjectDic.Instance.GetObjectName("2가대");
                    sOWMS_Code = "EG";
                }
                else if (m_PkgWay == "BOX")
                {
                    sPackageWay = sPackage + " " + "BOX";
                    sOWMS_Code = "EB";
                }

                if ("CNA".Equals(m_ToLoc))
                {
                    sPackageWay = sPackage + " " + "CNA";
                }

                string sUnitCode = string.Empty;
                string sOper_Desc = string.Empty;
                string sAbbrCode = string.Empty;

                DataTable RQSTDT3 = new DataTable();
                RQSTDT3.TableName = "RQSTDT";
                RQSTDT3.Columns.Add("LOTID", typeof(String));
                RQSTDT3.Columns.Add("LANGID", typeof(String));
                RQSTDT3.Columns.Add("CMCDTYPE", typeof(String));

                DataRow dr3 = RQSTDT3.NewRow();
                dr3["LOTID"] = m_LotID;
                dr3["LANGID"] = LoginInfo.LANGID;
                dr3["CMCDTYPE"] = "PRDT_ABBR_CODE";

                RQSTDT3.Rows.Add(dr3);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNITCODE_BY_LOTID", "RQSTDT", "RSLTDT", RQSTDT3);

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
                string m_CutStr2 = string.Empty;
                string s_CutStr1 = string.Empty;
                string s_CutStr2 = string.Empty;

                if (string.Equals(sUnitCode, "EA"))
                {
                    string sProdID = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID"));

                    if (!string.IsNullOrEmpty(sProdID))
                    {
                        decimal sConvLength = Util.NVC_Decimal(GetPatternLength(sProdID));

                        m_dConvCutM1 = Math.Floor((Util.NVC_Decimal(m_dCutM1) * sConvLength)/100);
                        m_dConvCutM2 = Math.Floor((Util.NVC_Decimal(m_dCutM2) * sConvLength)/100);
                        m_dConvCutC1 = Math.Floor((Util.NVC_Decimal(m_dCell1) * sConvLength)/100);
                        m_dConvCutC2 = Math.Floor((Util.NVC_Decimal(m_dCell2) * sConvLength)/100);
                    }
                }
                else
                {
                    m_dConvCutM1 = Util.NVC_Decimal(m_dCutM1);
                    m_dConvCutM2 = Util.NVC_Decimal(m_dCutM2);
                    m_dConvCutC1 = Util.NVC_Decimal(m_dCell1);
                    m_dConvCutC2 = Util.NVC_Decimal(m_dCell2);
                }

                if (string.Equals(sUnitCode, "EA"))
                {
                    m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1)) + "/" + String.Format("{0:#,##0}", (m_dConvCutM1)) + "M";
                    s_CutStr1 = String.Format("{0:#,##0}", (m_dCell1)) + "/" + String.Format("{0:#,##0}", (m_dConvCutC1)) + "M";
                    m_CutStr2 = String.Format("{0:#,##0}", (m_dCutM2)) + "/" + String.Format("{0:#,##0}", (m_dConvCutM2)) + "M";
                    s_CutStr2 = String.Format("{0:#,##0}", (m_dCell2)) + "/" + String.Format("{0:#,##0}", (m_dConvCutC2)) + "M";
                }
                else
                {
                    m_CutStr1 = String.Format("{0:#,##0}", (m_dCutM1));
                    s_CutStr1 = String.Format("{0:#,##0}", (m_dCell1));
                    m_CutStr2 = String.Format("{0:#,##0}", (m_dCutM2));
                    s_CutStr2 = String.Format("{0:#,##0}", (m_dCell2));
                }

                if (SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString() == "")
                {
                    sOper_Desc = null;
                }
                else
                {
                    sOper_Desc = SearchResult.Rows[0]["OFFER_SHEET_DESCRIPTION"].ToString();
                }

                string sVld_date = string.Empty;
                string sVld_date2 = string.Empty;
                string sVld = string.Empty;
                string sVld2 = string.Empty;

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE")) == "")
                {
                    sVld = null;
                }
                else
                {
                    sVld_date = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "VLD_DATE"));
                    sVld = sVld_date.ToString().Substring(0, 4) + "-" + sVld_date.ToString().Substring(4, 2) + "-" + sVld_date.ToString().Substring(6, 2);
                }

                if (Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "VLD_DATE")) == "")
                {
                    sVld2 = null;
                }
                else
                {
                    sVld_date2 = Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected02.Rows[0].DataItem, "VLD_DATE"));
                    sVld2 = sVld_date2.ToString().Substring(0, 4) + "-" + sVld_date2.ToString().Substring(4, 2) + "-" + sVld_date2.ToString().Substring(6, 2);
                }

                DateTime ProdDate = Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "WIPSDTTM")));
                string sProdDate = ProdDate.Year.ToString() + "-" + ProdDate.Month.ToString("00") + "-" + ProdDate.Day.ToString("00");

                string sV_DATE = ObjectDic.Instance.GetObjectName("유효기간");
                string sP_DATE = ObjectDic.Instance.GetObjectName("생산일자");

                dtPackingCard = new DataTable();

                dtPackingCard.Columns.Add("Title", typeof(string));
                dtPackingCard.Columns.Add("Title1", typeof(string));
                dtPackingCard.Columns.Add("MODEL_NAME", typeof(string));
                dtPackingCard.Columns.Add("MODEL_NAME1", typeof(string));
                dtPackingCard.Columns.Add("PRJT_NAME", typeof(string));
                dtPackingCard.Columns.Add("PRJT_NAME1", typeof(string));
                dtPackingCard.Columns.Add("PACK_NO", typeof(string));
                dtPackingCard.Columns.Add("PACK_NO1", typeof(string));
                dtPackingCard.Columns.Add("HEAD_BARCODE", typeof(string));
                dtPackingCard.Columns.Add("HEAD_BARCODE1", typeof(string));
                dtPackingCard.Columns.Add("Transfer", typeof(string));
                dtPackingCard.Columns.Add("Transfer1", typeof(string));
                dtPackingCard.Columns.Add("Total_M", typeof(string));
                dtPackingCard.Columns.Add("Total_Cell", typeof(string));
                dtPackingCard.Columns.Add("Total_M1", typeof(string));
                dtPackingCard.Columns.Add("Total_Cell1", typeof(string));
                dtPackingCard.Columns.Add("No1", typeof(string));
                dtPackingCard.Columns.Add("No2", typeof(string));
                dtPackingCard.Columns.Add("Lot1", typeof(string));
                dtPackingCard.Columns.Add("Lot2", typeof(string));
                dtPackingCard.Columns.Add("VLD_DATE1", typeof(string));
                dtPackingCard.Columns.Add("VLD_DATE2", typeof(string));
                dtPackingCard.Columns.Add("REG_DATE1", typeof(string));
                dtPackingCard.Columns.Add("REG_DATE2", typeof(string));
                dtPackingCard.Columns.Add("V1", typeof(string));
                dtPackingCard.Columns.Add("V2", typeof(string));
                dtPackingCard.Columns.Add("L1", typeof(string));
                dtPackingCard.Columns.Add("L2", typeof(string));
                dtPackingCard.Columns.Add("M1", typeof(string));
                dtPackingCard.Columns.Add("M2", typeof(string));
                dtPackingCard.Columns.Add("C1", typeof(string));
                dtPackingCard.Columns.Add("C2", typeof(string));
                dtPackingCard.Columns.Add("D1", typeof(string));
                dtPackingCard.Columns.Add("D2", typeof(string));
                dtPackingCard.Columns.Add("REMARK", typeof(string));
                dtPackingCard.Columns.Add("REMARK1", typeof(string));
                dtPackingCard.Columns.Add("UNIT_CODE", typeof(string));
                dtPackingCard.Columns.Add("UNIT_CODE1", typeof(string));
                dtPackingCard.Columns.Add("V_DATE", typeof(string));
                dtPackingCard.Columns.Add("P_DATE", typeof(string));
                dtPackingCard.Columns.Add("V_DATE1", typeof(string));
                dtPackingCard.Columns.Add("P_DATE1", typeof(string));
                dtPackingCard.Columns.Add("OFFER_DESC", typeof(string));
                dtPackingCard.Columns.Add("OFFER_DESC1", typeof(string));
                dtPackingCard.Columns.Add("ELEC", typeof(string));
                dtPackingCard.Columns.Add("ELEC1", typeof(string));
                dtPackingCard.Columns.Add("PRODID", typeof(string));
                dtPackingCard.Columns.Add("PRODID1", typeof(string));

                DataRow drCrad = null;

                drCrad = dtPackingCard.NewRow();

                drCrad.ItemArray = new object[] { sPackageWay,
                                                  sPackageWay2,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "MODLID")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "MODLID")),
                                                  "/" + Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRJT_NAME")),
                                                  "/" + Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRJT_NAME")),
                                                  m_PackingNo,
                                                  m_PackingNo,
                                                  "*" + m_PackingNo + "*",
                                                  "*" + m_PackingNo + "*",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSGNAME")) + " -> " + m_TrasferLoc,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "EQSGNAME")) + " -> " + m_TrasferLoc,
                                                  m_CutStr1,
                                                  s_CutStr1,
                                                  m_CutStr2,
                                                  s_CutStr2,
                                                  "1",
                                                  "1",
                                                  m_PackingNo + "_01",
                                                  m_PackingNo + "_02",
                                                  sVld,
                                                  sVld2,
                                                  sProdDate,
                                                  sProdDate,
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
                                                  m_PackingRemark1,
                                                  m_PackingRemark2,
                                                  sUnitCode,
                                                  sUnitCode,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sV_DATE,
                                                  sP_DATE,
                                                  sOper_Desc,
                                                  sOper_Desc,
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "ELEC")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "ELEC")),
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]",
                                                  Util.NVC(DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID")) + " [ " + sAbbrCode + " ]"
                                               };

                dtPackingCard.Rows.Add(drCrad);

                string sSHIPTO_ID = BOX001_221.cboTransLoc2.SelectedValue.ToString();
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

                sPackNo1 = m_PackingNo + "_01";
                sPackNo2 = m_PackingNo + "_02";

                // 기본 정보 DataTable
                dtBasicInfo = new DataTable();
                dtBasicInfo.TableName = "dtBasicInfo";
                dtBasicInfo.Columns.Add("INBOXID", typeof(string)); 
                dtBasicInfo.Columns.Add("OUTBOXID", typeof(string)); 
                dtBasicInfo.Columns.Add("QTY1", typeof(Decimal));
                dtBasicInfo.Columns.Add("QTY2", typeof(Decimal));
                dtBasicInfo.Columns.Add("PRODID", typeof(string));
                dtBasicInfo.Columns.Add("SHIPTO_ID", typeof(string));
                dtBasicInfo.Columns.Add("FROM_AREAID", typeof(string));
                dtBasicInfo.Columns.Add("FROM_SLOC_ID", typeof(string));
                dtBasicInfo.Columns.Add("TO_SLOC_ID", typeof(string));
                dtBasicInfo.Columns.Add("PACKING_NO", typeof(string));
                dtBasicInfo.Columns.Add("PACK_NO1", typeof(string));
                dtBasicInfo.Columns.Add("REMARK", typeof(string));
                dtBasicInfo.Columns.Add("PKGWAY", typeof(string));
                dtBasicInfo.Columns.Add("TOTAL_QTY", typeof(Decimal));
                dtBasicInfo.Columns.Add("TOTAL_QTY2", typeof(Decimal));
                dtBasicInfo.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));
                dtBasicInfo.Columns.Add("TO_SHOPID", typeof(string));
                dtBasicInfo.Columns.Add("TYPE", typeof(string));
                dtBasicInfo.Columns.Add("PROCID", typeof(string));

                DataRow drInfo = dtBasicInfo.NewRow();
                drInfo["INBOXID"] = sPackNo1;
                drInfo["OUTBOXID"] = m_PackingNo;
                //drInfo["QTY1"] = Convert.ToDecimal(txtSelectedM1.Text.ToString()) + Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString()) + Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                drInfo["QTY1"] = m_dCutM1 + m_dCutM2;
                drInfo["QTY2"] = m_dCell1 + m_dCell2;
                drInfo["PRODID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID");
                drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                drInfo["FROM_AREAID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_AREAID");
                drInfo["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_SLOC_ID");
                drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                drInfo["PACKING_NO"] = m_PackingNo;
                drInfo["PACK_NO1"] = m_PackingNo;
                drInfo["REMARK"] = m_PackingRemark1;
                drInfo["PKGWAY"] = m_PkgWay;
                //drInfo["TOTAL_QTY"] = Convert.ToDecimal(txtSelectedM1.Text.ToString());
                //drInfo["TOTAL_QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString());
                drInfo["TOTAL_QTY"] = m_dCutM1;
                drInfo["TOTAL_QTY2"] = m_dCell1;
                drInfo["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;
                drInfo["TO_SHOPID"] = sShopID;
                drInfo["TYPE"] = "SHIP";
                drInfo["PROCID"] = m_ProcID;

                dtBasicInfo.Rows.Add(drInfo);

                drInfo = dtBasicInfo.NewRow();
                drInfo["INBOXID"] = sPackNo2;
                drInfo["OUTBOXID"] = m_PackingNo;
                //drInfo["QTY1"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString()) + Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                //drInfo["QTY2"] = Convert.ToDecimal(txtSelectedCell1.Text.ToString()) + Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                drInfo["QTY1"] = m_dCell1 + m_dCell2;
                drInfo["QTY2"] = m_dCell1 + m_dCell2;
                drInfo["PRODID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "PRODID");
                drInfo["SHIPTO_ID"] = sSHIPTO_ID;
                drInfo["FROM_AREAID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_AREAID");
                drInfo["FROM_SLOC_ID"] = DataTableConverter.GetValue(dgPancakeListSelected01.Rows[0].DataItem, "FROM_SLOC_ID");
                drInfo["TO_SLOC_ID"] = sTO_SLOC_ID;
                drInfo["PACKING_NO"] = m_PackingNo;
                drInfo["PACK_NO1"] = m_PackingNo;
                drInfo["REMARK"] = m_PackingRemark2;
                drInfo["PKGWAY"] = m_PkgWay;
                //drInfo["TOTAL_QTY"] = Convert.ToDecimal(txtSelectedM2.Text.ToString());
                //drInfo["TOTAL_QTY2"] = Convert.ToDecimal(txtSelectedCell2.Text.ToString());
                drInfo["TOTAL_QTY"] = m_dCutM2;
                drInfo["TOTAL_QTY2"] = m_dCell2;
                drInfo["OWMS_BOX_TYPE_CODE"] = sOWMS_Code;
                drInfo["TO_SHOPID"] = sShopID;
                drInfo["TYPE"] = "SHIP";
                drInfo["PROCID"] = m_ProcID;

                dtBasicInfo.Rows.Add(drInfo);

                // dgPancakeListSelected01 정보 DataTable
                dtSel01 = new DataTable();
                dtSel01.TableName = "dtSel01";
                dtSel01.Columns.Add("INBOXID", typeof(string));
                dtSel01.Columns.Add("LOTID", typeof(string));
                dtSel01.Columns.Add("M_WIPQTY", typeof(string));
                dtSel01.Columns.Add("CELL_WIPQTY", typeof(string));


                for (int i = 0; i < dgPancakeListSelected01.GetRowCount(); i++)
                {
                    DataRow drSel01 = dtSel01.NewRow();

                    drSel01["INBOXID"] = sPackNo1;
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
                    dtSel02.Columns.Add("INBOXID", typeof(string));
                    dtSel02.Columns.Add("LOTID", typeof(string));
                    dtSel02.Columns.Add("M_WIPQTY", typeof(string));
                    dtSel02.Columns.Add("CELL_WIPQTY", typeof(string));

                    for (int i = 0; i < dgPancakeListSelected02.GetRowCount(); i++)
                    {
                        DataRow drSel02 = dtSel02.NewRow();

                        drSel02["INBOXID"] = sPackNo2;
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

                LGC.GMES.MES.BOX001.Report_Packing_Assy rs = new LGC.GMES.MES.BOX001.Report_Packing_Assy();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[5];
                    Parameters[0] = "PackingCard_2CRT_Assy";
                    Parameters[1] = dtPackingCard;
                    Parameters[2] = dtBasicInfo;
                    Parameters[3] = dtSel01;
                    Parameters[4] = dtSel02;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(Print_Result1);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private decimal GetPatternLength(string prodID)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));               

                DataRow Indata = IndataTable.NewRow();
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["PRODID"] = prodID;
               
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_MBOM_FOR_PRODID", "INDATA", "RSLTDT", IndataTable);

                if (result.Rows.Count > 0)
                    if (!string.IsNullOrEmpty(Util.NVC(result.Rows[0]["MTRL_QTY"])))
                        return Util.NVC_Decimal(result.Rows[0]["MTRL_QTY"]);
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

                    Util.gridClear(BOX001_221.dgOut);
                    BOX001_221.dgSub.Children.Clear();

                    BOX001_221.dgOut.IsEnabled = true;
                    BOX001_221.txtLotID.IsReadOnly = false;
                    BOX001_221.btnPackOut.IsEnabled = true;
                    BOX001_221.txtLotID.Text = "";
                    BOX001_221.txtLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        private void Print_Result1(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.BOX001.Report_Packing_Assy wndPopup = sender as LGC.GMES.MES.BOX001.Report_Packing_Assy;
                if (wndPopup.DialogResult == MessageBoxResult.OK)
                {
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.

                    Util.gridClear(BOX001_221.dgOut);
                    BOX001_221.dgSub.Children.Clear();

                    BOX001_221.dgOut.IsEnabled = true;
                    BOX001_221.txtLotID.IsReadOnly = false;
                    BOX001_221.btnPackOut.IsEnabled = true;
                    BOX001_221.txtLotID.Text = "";
                    BOX001_221.txtLotID.Focus();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion
    }
}
