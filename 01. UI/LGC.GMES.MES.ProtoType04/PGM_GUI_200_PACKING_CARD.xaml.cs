/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ProtoType04
{
    public partial class PGM_GUI_200_PACKING_CARD : C1Window, IWorkArea
    {
        private string m_Mode = string.Empty;
        private string m_LotID = string.Empty;

        private string m_WC_CODE = string.Empty;
        private string m_ProdType = string.Empty;
        private string m_PackingNo1 = string.Empty;
        private string m_PackingNo2 = string.Empty;
        private string m_TrasferLoc = string.Empty;
        private string m_PkgWay = string.Empty;
        private double m_iLane = 0;
        private double m_dCutMAvg = 0;
        private double m_dCellAvg = 0;
        private double m_iFrameLane1 = 0;
        private double m_iFrameLane2 = 0;
        private double m_dCutM1 = 0;
        private double m_dCutM2 = 0;
        private double m_dCell1 = 0;
        private double m_dCell2 = 0;
        private string m_PackingRemark = string.Empty;
        private string m_ProdFlag = string.Empty;
        private string m_Status = string.Empty; //�������m_Status (�����N ,��� O, ���ϱ���L ,�ǹ�Ȯ��C , ����S)
        private string m_TwinFlag = "N"; //1 Cut 2������ ��� Y
        private Boolean m_bDataBinding = false; //������ ���ε�����. ����ī�� ��������
        private Boolean m_ReprintFlag;
        private string m_Prod_Create_Seq = string.Empty;
        private string m_Prod_Code = string.Empty;
        private double m_dPattern_Conv = 0; //��ȯ��
        private Boolean m_SelectedDataChange = true;
        private string m_ToLoc = string.Empty;

        #region Declaration & Constructor 
        public PGM_GUI_200_PACKING_CARD()
        {
            InitializeComponent();

            Initialize();

            //public PGM_GUI_200_PACKING_CARD(string sLotid, string sMode)
            //m_LotID = sLotid;
            //m_Mode = sMode;
        }

        /// <summary>
        ///  Frame�� ��ȣ�ۿ��ϱ� ���� ��ü
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize
        //private void Initialize()
        //{

        //    CommonCombo _combo = new CommonCombo();

        //    String[] sFilter = { LoginInfo.CFG_AREA_ID };
        //    String[] sFilter3 = { "WH_TYPE" };
        //    _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");

        //    // ShowDialog
        //    if (m_Mode == "SCAN")
        //    {


        //        #region test�� / ���� �ʿ�
        //        DataTable dtADD = new DataTable();
        //        DataRow newRow = null;

        //        //dtADD = new DataTable();
        //        dtADD.Columns.Add("CHK", typeof(bool));
        //        dtADD.Columns.Add("PANCAKE", typeof(string));
        //        dtADD.Columns.Add("CUT_M", typeof(int));
        //        dtADD.Columns.Add("CELL_M", typeof(int));
        //        dtADD.Columns.Add("POSITION", typeof(int));

        //        List<object[]> list_RAN = new List<object[]>();

        //        for (int i = 1; i < 6; i++)
        //        {
        //            list_RAN.Add(new object[] { false, "PANCAKE01", i, i+5, 0 });
        //        }

        //        foreach (object[] item in list_RAN)
        //        {
        //            newRow = dtADD.NewRow();
        //            newRow.ItemArray = item;
        //            dtADD.Rows.Add(newRow);
        //        }

        //        dgPancakeListRemain.ItemsSource = DataTableConverter.Convert(dtADD);

        //        #endregion


        //    }
        //    else if (m_Mode == "SCANOTHERPOP")
        //    {

        //    }

        //}

        private void Initialize()
        {
            DataTable dtADD = new DataTable();
            DataRow newRow = null;

            //dtADD = new DataTable();
            dtADD.Columns.Add("CHK", typeof(bool));
            dtADD.Columns.Add("PANCAKE", typeof(string));
            dtADD.Columns.Add("CUT_M", typeof(int));
            dtADD.Columns.Add("CELL_M", typeof(int));
            dtADD.Columns.Add("POSITION", typeof(int));

            List<object[]> list_RAN = new List<object[]>();

            for (int i = 1; i < 6; i++)
            {
                list_RAN.Add(new object[] { false, "PANCAKE" + i.ToString("00"), i, i + 5, 0 });
            }

            foreach (object[] item in list_RAN)
            {
                newRow = dtADD.NewRow();
                newRow.ItemArray = item;
                dtADD.Rows.Add(newRow);
            }

            dgPancakeListRemain.ItemsSource = DataTableConverter.Convert(dtADD);
        }

        #endregion

        #region Event
        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            //����ī�� ���� ��ư Ŭ���� Validation
            if (Prevalidation() == true)
            {
                //����ī�� Ư�̻��� ���� ���:üũ�ڽ��� ���õ� ���븸 �����Ͽ� ���
                InitPackingNo();

                //If Me.m_ProdType = gsPolymer Then
                //    Me.m_dCell1 = 0
                //    Me.m_dCell2 = 0
                //End If

                PackingCardDataBinding_Config();

            }
        }

        private void cboPackWay_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboPackWay.SelectedItem.ToString() == "����")
            {
                m_PkgWay = "G";
                rdoFrame1.IsEnabled = true;
                rdoFrame2.IsEnabled = true;
                rdoFrame1.IsChecked = true;
                rdoFrame2.IsChecked = false;
                numLaneQty1.IsEnabled = false;
                numLaneQty2.IsEnabled = false;
                txtBlock1.Text = "1���� Lane";
                txtBlock2.Text = "2���� Lane";
            }
            else //Box
            {
                m_PkgWay = "B";
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
            btnAddToPosition02.IsEnabled = false;
            btnAddAllToPosition02.IsEnabled = false;
            btnDelFromPosition02.IsEnabled = false;
            btnDelAllFromPosition02.IsEnabled = false;
        }

        /// <summary>
        /// �̷� ī�� ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPrintCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// WMS ���� ���
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnWMSCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string slitter_lot_id1 = string.Empty;
                string slitter_lot_id2 = string.Empty;
                string packing_no1 = string.Empty;
                string packing_no2 = string.Empty;




                //WMS�� ���۵��� ���� ���� ������ �ִ��� Ȯ��
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr[""] = packing_no1;
                dr[""] = packing_no2;


                DataTable TransResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);

                if (TransResult.Rows.Count > 0)
                {

                }


                RQSTDT.Clear();



                // ���� : 1. ���� ������ ���� üũ / 0 => ���� ������ ���� LOT �Դϴ�.
                //        2. Status üũ / IC => WMS �԰� �Ϸ�Ǿ� ����Ҽ� �����ϴ�. WMS ���� �԰���� �� ��� �����մϴ�.
                //        3. P_LOT_PACKING_CANCEL_NEW ȣ��


                // ���� �������� Ȯ��
                //DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("", typeof(String));
                RQSTDT.Columns.Add("", typeof(String));

                dr = RQSTDT.NewRow();
                //DataRow dr = RQSTDT.NewRow();
                dr[""] = packing_no1;
                dr[""] = packing_no2;

                DataTable PackInfoResult = new ClientProxy().ExecuteServiceSync("", "RQSTDT", "RSLTDT", RQSTDT);






            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnAddToPosition01_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnAddAllToPosition01_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnDelFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnDelAllFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnAddToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(2, dgPancakeListSelected02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnAddAllToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(2, dgPancakeListSelected02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnDelFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnDelAllFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
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

        #endregion

        #region Mehod
        private void AddToPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            if (DataGrid.Rows.Count == 0)
            {
                DataTable dtAdd = new DataTable();
                dtAdd.Columns.Add("CHK", typeof(bool));
                dtAdd.Columns.Add("PANCAKE", typeof(string));
                dtAdd.Columns.Add("M", typeof(string));
                dtAdd.Columns.Add("CELL", typeof(string));

                DataGrid.ItemsSource = DataTableConverter.Convert(dtAdd);

                //DataRow newRow = null;
                //DataRow newRow = dtAdd.NewRow();

                for (int i = 0; i < dgPancakeListRemain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK").ToString() == "True" &&
                            //string.IsNullOrEmpty(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()))
                            Convert.ToInt32(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()) == 0)
                    {

                        //newRow = dtAdd.NewRow();
                        //newRow["CHK"] = true;
                        //newRow["PANCAKE"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PANCAKE").ToString();
                        //newRow["M"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CUT_M").ToString();
                        //newRow["CELL"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_M").ToString();
                        //dtAdd.Rows.Add(newRow);

                        //cell Lock �Ǵ� Hidden
                        DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION", iStrPos);


                        DataGrid.IsReadOnly = false;
                        DataGrid.BeginNewRow();
                        DataGrid.EndNewRow(true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PANCAKE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PANCAKE").ToString());
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "M", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CUT_M").ToString());
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CELL", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_M").ToString());
                        DataGrid.IsReadOnly = true;


                    }
                }

                //DataGrid.BeginEdit();
                //DataGrid.ItemsSource = DataTableConverter.Convert(dtAdd);
                //DataGrid.EndEdit();
            }
            else
            {
                for (int i = 0; i < dgPancakeListRemain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CHK").ToString() == "True" &&
                            //string.IsNullOrEmpty(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()))
                            Convert.ToInt32(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()) == 0)
                    {
                        DataGrid.IsReadOnly = false;
                        DataGrid.BeginNewRow();
                        DataGrid.EndNewRow(true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "PANCAKE", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PANCAKE").ToString());
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "M", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CUT_M").ToString());
                        DataTableConverter.SetValue(DataGrid.CurrentRow.DataItem, "CELL", DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_M").ToString());
                        DataGrid.IsReadOnly = true;

                        DataTableConverter.SetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION", iStrPos);

                    }
                }
            }
        }

        private void DelFromPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid DataGrid)
        {
            string sPancake_ID = string.Empty;

            if (DataGrid.Rows.Count == 0)
            {
                return;
            }

            for (int i = 0; i < DataGrid.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    sPancake_ID = DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "PANCAKE").ToString();

                    for (int j = 0; j < dgPancakeListRemain.Rows.Count; j++)
                    {
                        if (sPancake_ID == DataTableConverter.GetValue(dgPancakeListRemain.Rows[j].DataItem, "PANCAKE").ToString())
                        {

                            DataGrid.IsReadOnly = false;
                            //dgData.EndNewRow(true);
                            //DataGrid.RemoveRow(DataGrid.CurrentRow.Index);
                            DataGrid.RemoveRow(i);
                            DataGrid.IsReadOnly = true;

                            DataTableConverter.SetValue(dgPancakeListRemain.Rows[j].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgPancakeListRemain.Rows[j].DataItem, "POSITION", iStrPos);

                            i = i - 1;
                        }
                    }
                }
            }
        }

        private double GetPancakeSum(C1DataGrid DataGrid, string sName)
        {
            double dSum = 0;
            double dTotal = 0;

            for (int i = 0; i < DataGrid.Rows.Count; i++)
            {
                if (sName == "CUT_M")
                {
                    dSum = Convert.ToDouble(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "M").ToString());
                }
                else if (sName == "CELL_M")
                {
                    dSum = Convert.ToDouble(DataTableConverter.GetValue(DataGrid.Rows[i].DataItem, "CELL").ToString());
                }

                dTotal = dTotal + dSum;
            }

            return dTotal;
        }

        private bool Prevalidation()
        {
            bool bResult = true;

            if (Convert.ToInt32(numLaneQty1.Value) == 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("#1 Box/���뿡 ���õ� Lane �� �����ϴ�."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Convert.ToInt32(numLaneQty2.Value) == 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("#2 Box/���뿡 ���õ� Lane �� �����ϴ�."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (Convert.ToInt32(numLaneQty1.Value) + Convert.ToInt32(numLaneQty2.Value) > Convert.ToInt32(txtLaneQty.Text.ToString()))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("���� ������ Lane ������ �ʰ��Ǿ����ϴ�.(������� Lane�� Ȯ�����ּ���)"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (rdoFrame1.IsChecked.Value == false && rdoFrame2.IsChecked.Value == false)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("�����ȣ ������ ���� ���� Box/���� ���� �������ּ���."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return false;
            }

            if ((Convert.ToInt32(numLaneQty1.Value) == 1) || (rdoFrame2.IsChecked.Value == true && Convert.ToInt32(numLaneQty2.Value) == 1))
            {
                string sMsg = "�ϳ��� ����(BOX)�� �Ѱ� LANE���� ������ �����Դϴ�.  \r\n\r\n" + "�����ҷ��� [���] / ��������� [Ȯ��]";

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.Cancel)
                    {
                        bResult = false;
                    }
                });
            }

            return bResult;
        }

        /// <summary>
        /// Packing Card ���� ����
        /// </summary>
        private void InitPackingNo()
        {
            m_PackingRemark = string.Empty;

            if ((bool)chkRemark1.IsChecked)
            {
                m_PackingRemark = chkRemark1.Content.ToString();
            }

            if ((bool)chkRemark2.IsChecked)
            {
                m_PackingRemark = m_PackingRemark + chkRemark2.Content.ToString();
            }

            if ((bool)chkRemark3.IsChecked)
            {
                m_PackingRemark = m_PackingRemark + chkRemark3.Content.ToString();
            }

            if ((bool)chkRemark4.IsChecked)
            {
                m_PackingRemark = m_PackingRemark + chkRemark4.Content.ToString();
            }

            if (m_PkgWay == "G" && rdoFrame2.IsChecked == true)
            {
                m_iFrameLane1 = numLaneQty1.Value;
                m_iFrameLane2 = numLaneQty2.Value;

                switch (txtLotID.Text.ToString().Substring(8, 1))
                {
                    case "1":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "A";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "B";
                        break;
                    case "2":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "C";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "D";
                        break;
                    case "3":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "E";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "F";
                        break;
                    case "4":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "G";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "H";
                        break;
                    case "5":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "I";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "J";
                        break;
                    case "6":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "K";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "L";
                        break;
                    case "7":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "M";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "N";
                        break;
                    case "8":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "O";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "P";
                        break;
                    case "":
                        m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 8) + "Q";
                        m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 8) + "S";
                        break;
                }

                m_dCutM1 = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
                m_dCutM2 = GetPancakeSum(dgPancakeListSelected02, "CUT_M");

                m_dCell1 = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
                m_dCell2 = GetPancakeSum(dgPancakeListSelected02, "CELL_M");

                m_TwinFlag = "Y";
            }
            else
            {
                m_iFrameLane1 = numLaneQty1.Value;
                m_iFrameLane2 = 0;

                m_PackingNo1 = txtLotID.Text.ToString().Substring(0, 9);
                m_PackingNo2 = txtLotID.Text.ToString().Substring(0, 9);

                m_dCutM1 = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
                m_dCutM2 = 0;

                m_dCell1 = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
                m_dCell2 = 0;

                m_TwinFlag = "N";
            }
        }

        /// <summary>
        /// ������ ȭ�� Loading
        /// </summary>
        private void PackingCardDataBinding_Config()
        {

        }


        #endregion
    }
}