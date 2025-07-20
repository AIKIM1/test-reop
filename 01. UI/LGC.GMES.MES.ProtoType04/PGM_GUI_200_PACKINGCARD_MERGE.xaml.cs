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
    public partial class PGM_GUI_200_PACKINGCARD_MERGE : UserControl, IWorkArea
    {
        public PGM_GUI_400 PGM_GUI_400;

        private string m_Mode = string.Empty;
        private string m_WC_CODE = string.Empty;
        private string m_LotID = string.Empty;
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
        private string m_Status = string.Empty; //현재상태m_Status (출고대기N ,출고 O, 출하구성L ,실물확인C , 출하S)
        private string m_TwinFlag = string.Empty; //1 Cut 2가대일 경우 Y
        private Boolean m_bDataBinding = false; //데이터 바인딩여부. 포장카드 생성여부
        private Boolean m_ReprintFlag;
        private string m_Prod_Create_Seq = string.Empty;
        private string m_Prod_Code = string.Empty;
        private double m_dPattern_Conv = 0; //변환율
        private Boolean m_SelectedDataChange = true;
        private string m_ToLoc = string.Empty;

        #region Declaration & Constructor 
        public PGM_GUI_200_PACKINGCARD_MERGE()
        {
            InitializeComponent();

            Initialize();
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
            //List<Button> listAuth = new List<Button>();
            ////listAuth.Add(btnInReplace);

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            Initialize();
        }

        #endregion

        #region Initialize
        private void Initialize()
        {

            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };
            String[] sFilter3 = { "WH_TYPE" };
            _combo.SetCombo(cboPackWay, CommonCombo.ComboStatus.NONE, sFilter: sFilter3, sCase: "COMMCODE");




            #region Test 임시 데이터


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

            dgPancakeListRemain01.ItemsSource = DataTableConverter.Convert(dtADD);
            dgPancakeListRemain02.ItemsSource = DataTableConverter.Convert(dtADD);

            #endregion


        }

        #endregion

        #region Event
        private void btnPackCard_Click(object sender, RoutedEventArgs e)
        {
            //포장카드 구성 버튼 클릭시 Validation
            if (Prevalidation() == true)
            {
                //포장카드 특이사항 내용 출력:체크박스에 선택된 내용만 선택하여 출력
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
            if (cboPackWay.SelectedItem.ToString() == "가대")
            {
                m_PkgWay = "G";

                txtBlock1.Text = "가대 Lane #1";
                txtBlock2.Text = "가대 Lane #2";
            }
            else
            {
                m_PkgWay = "B";

                txtBlock1.Text = "BOX Lane #1";
                txtBlock2.Text = "BOX Lane #2";
            }

            InitLaneList();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

        }

        ////private void btnPrintCancel_Click(object sender, RoutedEventArgs e)
        ////{
        ////    this.Close();
        ////}

        private void btnWMSCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string slitter_lot_id1 = string.Empty;
                string slitter_lot_id2 = string.Empty;
                string packing_no1 = string.Empty;
                string packing_no2 = string.Empty;




                //WMS로 전송되지 않은 예약 정보가 있는지 확인
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



                // 기존 : 1. 실적 정보를 수량 체크 / 0 => 실적 정보가 없는 LOT 입니다.
                //        2. Status 체크 / IC => WMS 입고 완료되어 취소할수 없습니다. WMS 에서 입고취소 후 취소 가능합니다.
                //        3. P_LOT_PACKING_CANCEL_NEW 호출


                // 포장 실적정보 확인
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
            AddToPosition(1, dgPancakeListSelected01, dgPancakeListRemain01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnAddAllToPosition01_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected01, dgPancakeListRemain01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnDelFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeListRemain01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnDelAllFromPosition01_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected01, dgPancakeListRemain01);

            numLaneQty1.Value = dgPancakeListSelected01.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected01, "CUT_M");
            txtSelectedM1.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected01, "CELL_M");
            txtSelectedCell1.Text = Convert.ToString(dCell);
        }

        private void btnAddToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected02, dgPancakeListRemain02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnAddAllToPosition02_Click(object sender, RoutedEventArgs e)
        {
            AddToPosition(1, dgPancakeListSelected02, dgPancakeListRemain02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnDelFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeListRemain02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }

        private void btnDelAllFromPosition02_Click(object sender, RoutedEventArgs e)
        {
            DelFromPosition(0, dgPancakeListSelected02, dgPancakeListRemain02);

            numLaneQty2.Value = dgPancakeListSelected02.Rows.Count;

            double dM = GetPancakeSum(dgPancakeListSelected02, "CUT_M");
            txtSelectedM2.Text = Convert.ToString(dM);

            double dCell = GetPancakeSum(dgPancakeListSelected02, "CELL_M");
            txtSelectedCell2.Text = Convert.ToString(dCell);
        }


        #endregion

        #region Mehod
        private void InitLaneList()
        {
            int iStrPos = 0;

            Util.gridClear(dgPancakeListSelected01);
            Util.gridClear(dgPancakeListSelected02);

            numLaneQty1.Value = 0;
            numLaneQty2.Value = 0;

            for (int i = 0; i < dgPancakeListRemain01.Rows.Count - dgPancakeListRemain01.BottomRows.Count; i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain01.Rows[i].DataItem, "CHK", false);
                DataTableConverter.SetValue(dgPancakeListRemain01.Rows[i].DataItem, "POSITION", iStrPos);
            }

            for (int i = 0; i < dgPancakeListRemain02.Rows.Count - dgPancakeListRemain02.BottomRows.Count; i++)
            {
                DataTableConverter.SetValue(dgPancakeListRemain02.Rows[i].DataItem, "CHK", false);
                DataTableConverter.SetValue(dgPancakeListRemain02.Rows[i].DataItem, "POSITION", iStrPos);
            }

        }


        private void AddToPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid dgSel, C1.WPF.DataGrid.C1DataGrid dgRemain)
        {
            if (dgSel.Rows.Count == 0)
            {
                DataTable dtAdd = new DataTable();
                dtAdd.Columns.Add("CHK", typeof(bool));
                dtAdd.Columns.Add("PANCAKE", typeof(string));
                dtAdd.Columns.Add("M", typeof(string));
                dtAdd.Columns.Add("CELL", typeof(string));

                dgSel.ItemsSource = DataTableConverter.Convert(dtAdd);

                //DataRow newRow = null;
                //DataRow newRow = dtAdd.NewRow();

                for (int i = 0; i < dgRemain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CHK").ToString() == "True" &&
                            //string.IsNullOrEmpty(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()))
                            Convert.ToInt32(DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "POSITION").ToString()) == 0)
                    {

                        //newRow = dtAdd.NewRow();
                        //newRow["CHK"] = true;
                        //newRow["PANCAKE"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "PANCAKE").ToString();
                        //newRow["M"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CUT_M").ToString();
                        //newRow["CELL"] = DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "CELL_M").ToString();
                        //dtAdd.Rows.Add(newRow);

                        //cell Lock 또는 Hidden
                        DataTableConverter.SetValue(dgRemain.Rows[i].DataItem, "POSITION", iStrPos);


                        dgSel.IsReadOnly = false;
                        dgSel.BeginNewRow();
                        dgSel.EndNewRow(true);
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "PANCAKE", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "PANCAKE").ToString());
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "M", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CUT_M").ToString());
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "CELL", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CELL_M").ToString());
                        dgSel.IsReadOnly = true;


                    }
                }

                //DataGrid.BeginEdit();
                //DataGrid.ItemsSource = DataTableConverter.Convert(dtAdd);
                //DataGrid.EndEdit();
            }
            else
            {
                for (int i = 0; i < dgRemain.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CHK").ToString() == "True" &&
                            //string.IsNullOrEmpty(DataTableConverter.GetValue(dgPancakeListRemain.Rows[i].DataItem, "POSITION").ToString()))
                            Convert.ToInt32(DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "POSITION").ToString()) == 0)
                    {
                        dgSel.IsReadOnly = false;
                        dgSel.BeginNewRow();
                        dgSel.EndNewRow(true);
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "CHK", true);
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "PANCAKE", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "PANCAKE").ToString());
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "M", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CUT_M").ToString());
                        DataTableConverter.SetValue(dgSel.CurrentRow.DataItem, "CELL", DataTableConverter.GetValue(dgRemain.Rows[i].DataItem, "CELL_M").ToString());
                        dgSel.IsReadOnly = true;

                        DataTableConverter.SetValue(dgRemain.Rows[i].DataItem, "POSITION", iStrPos);

                    }
                }
            }
        }

        private void DelFromPosition(int iStrPos, C1.WPF.DataGrid.C1DataGrid dgSel, C1.WPF.DataGrid.C1DataGrid dgRemain)
        {
            string sPancake_ID = string.Empty;

            if (dgSel.Rows.Count == 0)
            {
                return;
            }

            for (int i = 0; i < dgSel.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgSel.Rows[i].DataItem, "CHK").ToString() == "True")
                {
                    sPancake_ID = DataTableConverter.GetValue(dgSel.Rows[i].DataItem, "PANCAKE").ToString();

                    for (int j = 0; j < dgRemain.Rows.Count; j++)
                    {
                        if (sPancake_ID == DataTableConverter.GetValue(dgRemain.Rows[j].DataItem, "PANCAKE").ToString())
                        {

                            dgSel.IsReadOnly = false;
                            //dgData.EndNewRow(true);
                            //DataGrid.RemoveRow(DataGrid.CurrentRow.Index);
                            dgSel.RemoveRow(i);
                            dgSel.IsReadOnly = true;

                            DataTableConverter.SetValue(dgRemain.Rows[j].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgRemain.Rows[j].DataItem, "POSITION", iStrPos);

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

        #endregion

        private bool Prevalidation()
        {
            bool bResult = true;

            // 슬리터 실적 기준으로 Lane 선택 범위 제한

            if (Convert.ToInt32(numLaneQty1.Value) == 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("#1 Box/가대에 선택된 Lane 이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bResult = false;
            }

            if (Convert.ToInt32(numLaneQty2.Value) == 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("#2 Box/가대에 선택된 Lane 이 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bResult = false;
            }

            if (Convert.ToInt32(numLaneQty1.Value) > Convert.ToInt32(txtLaneQty1.Text))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택 가능한 범위를 초과하였습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bResult = false;
            }

            if (Convert.ToInt32(numLaneQty2.Value) > Convert.ToInt32(txtLaneQty2.Text))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("선택 가능한 범위를 초과하였습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bResult = false;
            }

            if (rdoFrame1.IsChecked.Value == false)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장번호 생성을 위해 적용 가대 수를 선택해주세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                bResult = false;
            }

            if (Convert.ToInt32(numLaneQty1.Value) == 1 || Convert.ToInt32(numLaneQty2.Value) == 1)
            {
                string sMsg = "하나의 가대(BOX)에 한개 LANE으로 구성한 포장입니다.  \r\n\r\n" + "수정할려면 [취소] / 계속진행은 [확인]";

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(sMsg), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.Cancel)
                        bResult = false;
                });
            }

            return bResult;
        }

        private void InitPackingNo()
        {
            m_PackingRemark = string.Empty;

            if ((bool)chkRemark1.IsChecked)
                m_PackingRemark = chkRemark1.Content.ToString();

            if ((bool)chkRemark2.IsChecked)
                m_PackingRemark = m_PackingRemark + chkRemark2.Content.ToString();

            if ((bool)chkRemark3.IsChecked)
                m_PackingRemark = m_PackingRemark + chkRemark3.Content.ToString();

            if ((bool)chkRemark4.IsChecked)
                m_PackingRemark = m_PackingRemark + chkRemark4.Content.ToString();

            m_iFrameLane1 = numLaneQty1.Value;
            m_iFrameLane2 = numLaneQty2.Value;

            m_PackingNo1 = txtLotID1.Text.Substring(0, 9) + "0";
            m_PackingNo2 = txtLotID1.Text.Substring(0, 9) + "0";

            m_dCutM1 = Convert.ToDouble(txtSelectedM1.Text.ToString());
            m_dCutM2 = Convert.ToDouble(txtSelectedM2.Text.ToString());

            m_dCell1 = Convert.ToDouble(txtSelectedCell1.Text.ToString());
            m_dCell2 = Convert.ToDouble(txtSelectedCell2.Text.ToString());

            m_TwinFlag = "N";
        }

        private void PackingCardDataBinding_Config()
        {

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int iPosition = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "POSITION").ToString()));

            if (!(bool)(sender as CheckBox).IsChecked && iPosition != 0)
                (sender as CheckBox).IsChecked = true;
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            int iPosition = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "POSITION").ToString()));

            if (!(bool)(sender as CheckBox).IsChecked && iPosition != 0)
                (sender as CheckBox).IsChecked = true;
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}