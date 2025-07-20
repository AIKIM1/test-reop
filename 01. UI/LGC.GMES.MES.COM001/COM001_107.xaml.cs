/*************************************************************************************
 Created Date : 2017.09.07
      Creator : 
   Decription : 합권취 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.31  DEVELOPER : Initial Created.
  2022.01.04  정재홍      [C20211213-000126] 합권-합권수량 개선
  2022.01.11  정재홍      [C20210921-000040] LOT 예약 홀드 등록된 생산 LOT은 합권취 불가 Validation 추가
  2023.09.07  김도형      [E20230807-000061] speical work-LOT MERGE improvement
  2024.03.12  황재원      [E20240226-000300] 소형 3동 전극 합권취 이후 전산 수량 오류 발생 건 수량 검증 수정
  2025.02.24  이민형      : 날짜 함수 Util.GetConfition 으로 형변환 함수 변경
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{

    public partial class COM001_107 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util util = new Util();

        int MAXMERGECOUNT = 10;
        int cnt = 0;
        int checkIndex; //합권취소에서 그리드 클릭시 index저장
        string temp = string.Empty;
        private string selectedElecType = string.Empty;

        private string _ProIDMerge = string.Empty;   //[E20230807-000061] speical work-LOT MERGE improvement
        private string _ProNameMerge = string.Empty; //[E20230807-000061] speical work-LOT MERGE improvement

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_107()
        {
            InitializeComponent();
            ldpDatePickerFrom.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }



        #endregion

        #region EVENT
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }



        private void UserControl_Initialized(object sender, EventArgs e)
        {
            initCombo();
            ldpDatePickerFrom.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }
        #region[MERGE EVENT]

     
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            CheckBoxClickProcess(index);

        }

        private void cboLotList_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboLot.Items.Count > 1)
            {
                MergeSum();

            }
        }

        private void cboLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLot.Items.Count > 1)
            {
                MergeSum();

            }
        }

        private void MergeSum()
        {
            List<DataRow> QtyList = DataTableConverter.Convert(dgMergeInfo.ItemsSource).AsEnumerable().Where(c => !string.IsNullOrWhiteSpace(c.Field<string>("MERGEQTY"))).ToList();
            List<DataRow> WipQty = DataTableConverter.Convert(dgMergeInfo.ItemsSource).AsEnumerable().Where(c => !string.IsNullOrWhiteSpace(c.Field<string>("WIPQTY"))).ToList();
            
            txtMergeQty.Text = Convert.ToString(QtyList.AsEnumerable().Sum(c => Double.Parse(c.Field<string>("MERGEQTY"))));
          //  txtLastQty.Text = Convert.ToString(WipQty.AsEnumerable().Sum(c => Double.Parse(c.Field<string>("WIPQTY"))) - QtyList.AsEnumerable().Sum(c => Double.Parse(c.Field<string>("MERGEQTY"))));
        }

        private void dgMergeInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.Equals("MERGEQTY"))
            {
                if (DataTableConverter.GetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "LOTID") != null)
                {
                    DataTable dt = DataTableConverter.Convert(dgMergeInfo.ItemsSource);

                    decimal reaminqty = Decimal.Parse(Util.NVC(dt.Select("LOTID = '" + DataTableConverter.GetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "LOTID") + "'")[0]["WIPQTY"])) - Decimal.Parse(Util.NVC(dt.Select("LOTID = '" + DataTableConverter.GetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "LOTID") + "'")[0]["MERGEQTY"])); //- Util.NVC_Decimal(dt.Select("LOTID = '" + DataTableConverter.GetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "LOTID") + "'")[0]["MERGEQTY"]);

                    //C20211213-000126 - 합권-합권수량 개선
                    decimal MergeQTY = Decimal.Parse(Util.NVC(dt.Select("LOTID = '" + DataTableConverter.GetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "LOTID") + "'")[0]["WIPQTY"]));
                    decimal ResetQty = Decimal.Parse("0.00000");
                    if (reaminqty < 0)
                    {
                        Util.MessageValidation("SFU8452");
                        DataTableConverter.SetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "MERGEQTY", MergeQTY);
                        DataTableConverter.SetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "REAMIN_QTY", ResetQty);
                        MergeSum();
                        return;
                    }
                    /////////////////////////////////////////////
                    DataTableConverter.SetValue(dgMergeInfo.Rows[e.Cell.Row.Index].DataItem, "REAMIN_QTY", reaminqty);
                }

                MergeSum();
            }
        }
       

        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            string sLotid = "F";                    // [E20230807-000061] speical work-LOT MERGE improvement
            string sProceChkMsg = string.Empty;     // [E20230807-000061] speical work-LOT MERGE improvement 

            try
            {
                DataTable dt = DataTableConverter.Convert(dgMergeInfo.ItemsSource);
                if (dt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3218"); //합권 할 LOT이 없습니다.
                    return;
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgMergeInfo.Rows[0].DataItem, "LANEQTY").ToString() != DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LANEQTY").ToString())
                    {
                        Util.MessageValidation("SFU5081"); //레인수가 다릅니다.
                        return;
                    }
                    if (DataTableConverter.GetValue(dgMergeInfo.Rows[0].DataItem, "MKT_TYPE_CODE").ToString() != DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "MKT_TYPE_CODE").ToString())
                    {
                        Util.MessageValidation("SFU4271"); //레인수가 다릅니다.
                        return;
                    }

                    // [E20230807-000061] speical work-LOT MERGE improvement
                    if (Util.NVC(sLotid).Equals("F"))
                    {
                        sLotid = DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID").ToString();
                    }
                    else
                    {
                        sLotid = sLotid + "," + DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID").ToString();
                    }

                }
                if (dt.Rows.Count == 1)
                {
                    Util.MessageValidation("SFU1140");  //LOT은 2개 이상 이어야 합니다.
                    return;
                }

                if ( Decimal.Parse(Convert.ToString(dt.Select("LOTID = '" + Convert.ToString(cboLot.SelectedValue) + "'")[0]["MERGEQTY"])) != Decimal.Parse(Convert.ToString(dt.Select("LOTID = '" + Convert.ToString(cboLot.SelectedValue) + "'")[0]["WIPQTY"])))
                {
                    Util.MessageValidation("SFU4114"); // 대표LOT은 잔량을 남길 수 없습니다. 
                    return;
                }

                if (dgMergeInfo.CurrentCell.IsEditing)
                {
                    //수량 입력을 마무리하고 다시 진행해주세요.
                    Util.MessageValidation("SFU8239");
                    return;

                }

                // [E20230807-000061] speical work-LOT MERGE improvement
                sProceChkMsg = GetMergeLotProcIDChk(_ProIDMerge, sLotid);

                if (!Util.NVC(sProceChkMsg).Equals("Y"))
                {
                    if (!Util.NVC(sProceChkMsg).Equals("E"))
                    {
                        Util.MessageValidation("SFU9205", _ProNameMerge, sProceChkMsg); //  동일공정의 LOT이 아닙니다.
                        SearchData();
                    }
                    return;
                }

                DataTable merRsult = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_MERGE_LOT_QTY_CHK", "RQSTDT", "RSLTDT", dt);
 
                if (merRsult.Rows[0]["MERGE_QTY_FLAG"].Equals("Y"))
                {
                    Util.MessageConfirm("SFU8209", (result) =>
                    {
						//[E20240226-000300] 소형 3동 전극 합권취 이후 전산 수량 오류 발생 건 수량 검증 수정
						if (result == MessageBoxResult.OK)
                        {
							SearchData();
                        }
                    });
                }
                else
                {
					MergeButtonClickProcess();
				}
            
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // [E20230807-000061] speical work-LOT MERGE improvement
        private string GetMergeLotProcIDChk(string sProcid, string sLotid)
        {
            string sChkResult = "Y";

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID_MERGE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string)); 

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; 
                dr["PROCID_MERGE"] = sProcid ;
                dr["LOTID"] = sLotid ;
               
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PROD_SEL_MERGE_LOT_PROCID_CHK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _ProNameMerge = Util.NVC(dtResult.Rows[0]["PROCNAME_MERGE"].ToString());

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {   
                        if (Util.NVC(sChkResult).Equals("Y"))
                        {
                            sChkResult = Util.NVC(dtResult.Rows[i]["CHK_MSG"].ToString());
                        }
                        else
                        {
                            sChkResult = sChkResult + "\r\n" + Util.NVC(dtResult.Rows[i]["CHK_MSG"].ToString());
                        }
                    }
                }
                else
                {
                    sChkResult = "Y";
                }
              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                sChkResult = "E";
            }

            return sChkResult;
        }
        #endregion

        #region[CANCEL EVENT]

        private void btnCSearch_Click(object sender, RoutedEventArgs e)
        {
            dgCLotList.ItemsSource = null;
            if (cboCancelShop.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1424");  //SHOP을 선택하세요.
                return;
            }
            if (AREA.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1499");  //동을 선택하세요.
                return;
            }
            if (EQUIPMENTSEGMENT.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }
            if (PROCESS.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return;
            }
            SelectProcess();
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            if (dgCLotList.GetRowCount() < 0)
            {
                return;
            }

            checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as RadioButton).Parent)).Row.Index;
            DataTableConverter.SetValue(dgCLotList.Rows[checkIndex].DataItem, "CHK", 1);

            for (int i = 0; i < dgCLotList.Rows.Count; i++)
            {
                if (i != checkIndex)
                {
                    DataTableConverter.SetValue(dgCLotList.Rows[i].DataItem, "CHK", 0);
                }
            }

        }
        private void btnMergeCancel_Click(object sender, RoutedEventArgs e)
        {

            COM001_107_MERGE_CANCEL_INFO wndPopup = new COM001_107_MERGE_CANCEL_INFO();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] =  DataTableConverter.Convert(dgCLotList.ItemsSource).Select("CHK = 1 or CHK = True")[0]["LOTID"] ;

                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndPopup_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void wndPopup_Closed(object sender, EventArgs e)
        {
             COM001_107_MERGE_CANCEL_INFO window = sender as COM001_107_MERGE_CANCEL_INFO;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SelectProcess();
            }
        }

        #endregion

        #region[MERGE_HIST_EVENT]
        private void btnHistSearch_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            dgMergeHistList.ItemsSource = null;

            if (cboHistShop.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1424");  //SHOP을 선택하세요.
                return;
            }
            if (cboHistAREA.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1499");  //동을 선택하세요.
                return;
            }
            if (cboHistEquipmentSegment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                return;
            }
            if (cboHistProcess.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                return;
            }

            MergeHistSearchData();
        }

        private void dgMergeHistList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgMergeHistList.GetRowCount() == 0) return;

                List<System.Data.DataRow> list = DataTableConverter.Convert(dgMergeHistList.ItemsSource).Select().ToList();
                string tmp_lotid = Util.NVC(list[0]["LOTID"]);
                string tmp_actdttm = Util.NVC(list[0]["ACTDTTM"]);
                int cnt = 0;

                for (int i = 0; i < list.Count; i++)
                {
                    if (tmp_lotid.Equals(Util.NVC(list[i]["LOTID"])) && tmp_actdttm.Equals(Util.NVC(list[i]["ACTDTTM"])))
                    {
                        cnt++;
                        //가장 마지막 Row는 자신을 포함하여 cnt개 만큼 Merge해야 함
                        if (i == list.Count - 1)
                        {
                            for (int j = 0; j < dgMergeHistList.Columns.Count; j++)
                            {
                                if (!(dgMergeHistList.Columns[j].Name.Equals("FROM_LOTID") || dgMergeHistList.Columns[j].Name.Equals("FROM_LOTQTY")))
                                {
                                    e.Merge(new DataGridCellsRange(dgMergeHistList.GetCell(i - cnt + 1, j), dgMergeHistList.GetCell((i), j)));
                                }

                            }
                        }
                    }
                    else
                    {
                        //전과 다음이 다를 때까지 이동함.
                        if (cnt > 1)
                        {
                            for (int j = 0; j < dgMergeHistList.Columns.Count; j++)
                            {
                                if (!(dgMergeHistList.Columns[j].Name.Equals("FROM_LOTID") || dgMergeHistList.Columns[j].Name.Equals("FROM_LOTQTY")))
                                {
                                    e.Merge(new DataGridCellsRange(dgMergeHistList.GetCell(i - cnt, j), dgMergeHistList.GetCell((i - 1), j)));
                                }

                            }
                        }
                        //초기화
                        tmp_lotid = Util.NVC(list[i]["LOTID"]);
                        tmp_actdttm = Util.NVC(list[i]["ACTDTTM"]);
                        cnt = 1;
                    }
                }


            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #endregion

        #region Method
        #region [Merge]
        private void initCombo()
        {

            C1ComboBox[] cboShopChild = { cboArea, cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboShop, CommonCombo.ComboStatus.NONE, cbChild: cboShopChild);

            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment, cboProcess };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbParent: cboAreaParent, cbChild: cboAreaChild);

            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboShop };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

            String[] sFilter1 = { "", "ELEC_TYPE" };
            combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODES");


            //합권취소 콤보박스 
            C1ComboBox[] cboCShopChild = { AREA, EQUIPMENTSEGMENT, PROCESS };
            combo.SetCombo(cboCancelShop, CommonCombo.ComboStatus.NONE, cbChild: cboCShopChild, sCase: "cboShop");

            C1ComboBox[] cboCAreaParent = { cboCancelShop };
            C1ComboBox[] cboCAreaChild = { EQUIPMENTSEGMENT };
            combo.SetCombo(AREA, CommonCombo.ComboStatus.SELECT, cbParent: cboCAreaParent, cbChild: cboCAreaChild);

            C1ComboBox[] cboCEquipmentSegmentParent = { AREA, cboCancelShop };
            C1ComboBox[] cboCEquipmentSegmentChild = { PROCESS };
            combo.SetCombo(EQUIPMENTSEGMENT, CommonCombo.ComboStatus.SELECT, cbParent: cboCEquipmentSegmentParent, cbChild: cboCEquipmentSegmentChild);

            C1ComboBox[] cboCProcessParent = { EQUIPMENTSEGMENT, AREA, cboCancelShop };
            combo.SetCombo(PROCESS, CommonCombo.ComboStatus.SELECT, cbParent: cboCProcessParent);


            //합권이력조회 콤보박스
            C1ComboBox[] cboHistShopChild = { cboHistAREA, cboHistEquipmentSegment, cboHistProcess };
            combo.SetCombo(cboHistShop, CommonCombo.ComboStatus.NONE, cbChild: cboHistShopChild, sCase: "cboShop");

            C1ComboBox[] cboHistAreaParent = { cboHistShop };
            C1ComboBox[] cboHistAreaChild = { cboHistEquipmentSegment };
            combo.SetCombo(cboHistAREA, CommonCombo.ComboStatus.SELECT, cbParent: cboHistAreaParent, cbChild: cboHistAreaChild, sCase: "cboArea");

            C1ComboBox[] cboHistEquipmentSegmentParent = { cboHistAREA, cboHistShop };
            C1ComboBox[] cboHistEquipmentSegmentChild = { cboHistProcess };
            combo.SetCombo(cboHistEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboHistEquipmentSegmentParent, cbChild: cboHistEquipmentSegmentChild, sCase:"cboEquipmentSegment");

            C1ComboBox[] cboHistProcessParent = { cboHistEquipmentSegment, cboHistAREA, cboHistShop };
            combo.SetCombo(cboHistProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboHistProcessParent, sCase: "cboProcess");



        }
        private void SetLotListCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = null;

            for (int i = 0; i < dgMergeInfo.GetRowCount(); i++)
            {
                row = dt.NewRow();
                row["CBO_NAME"] = Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID"));
                row["CBO_CODE"] = Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID"));
                dt.Rows.Add(row);
            }

            cboLot.DisplayMemberPath = "CBO_CODE";
            cboLot.SelectedValuePath = "CBO_NAME";
            cboLot.ItemsSource = DataTableConverter.Convert(dt);

            cboLot.SelectedIndex = 0;

           
        }
        private void CheckBoxClickProcess(int index)
        {
            string lotid = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "LOTID"));
            string wh_id = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WH_ID"));

            if (!string.IsNullOrEmpty(wh_id))
            {
                Util.MessageValidation("SFU2963");
                DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK", false);
                return;
            }

            if (!ValidLotHoldRSV(lotid))
            {
                Util.MessageValidation("SFU8464"); //LOT 예약 홀드에 등록된 LOT 입니다.
                DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK", false);
                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "CHK")).Equals("1"))
            {
                if (cnt == MAXMERGECOUNT)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK", false);
                    Util.MessageValidation("SFU1221");  //더 이상 추가 할 수 없습니다.
                    return;
                }

                

                //if (!string.IsNullOrEmpty(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WH_ID").ToString()))
                //{
                //    Util.MessageValidation("SFU2963");
                //    return;
                //}

                for (int i = 0; i < dgMergeInfo.GetRowCount(); i++)
                {
                    if (lotid.Equals(Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID"))))
                    {
                        Util.MessageValidation("SFU1508");  //동일한 LOT ID가 있습니다.
                        return;
                    }

                }

                if (DataTableConverter.Convert(dgMergeInfo.ItemsSource) == null)
                {
                    return;
                }


                dgMergeInfo.IsReadOnly = false;
                dgMergeInfo.BeginNewRow();
                dgMergeInfo.EndNewRow(true);

                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "LOTID_RT", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "LOTID_RT")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "VERSION", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "VERSION")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "LOTID", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "LOTID")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "CSTID", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "CSTID")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PROCNAME", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PROCNAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "MODLID", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "MODLID")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "WIPQTY", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WIPQTY")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "MERGEQTY", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "MERGEQTY")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "REAMIN_QTY", Util.NVC(Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WIPQTY"))) - Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "MERGEQTY")))));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "UNIT", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "UNIT")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRODID")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRODNAME", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRODNAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "STATUS", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WIPSNAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRJT_NAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "LANEQTY", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "LANEQTY")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "MKT_TYPE_CODE", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "MKT_TYPE_CODE")));

                //dgmerge에 추가

                foreach (C1.WPF.DataGrid.DataGridColumn col in dgMergeInfo.Columns)
                {
                    if (!col.Name.Equals("MERGEQTY"))
                    {
                        col.IsReadOnly = true;
                    }
                }
          
                cnt++;
            }
            else
            {
                if (dgMergeInfo.CurrentRow != null)
                {
                    dgMergeInfo.IsReadOnly = false;

                    for (int i = 0; i < dgMergeInfo.Rows.Count; i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID")).Equals(lotid))
                        {
                            dgMergeInfo.RemoveRow(i);
                        }
                    }
                    dgMergeInfo.IsReadOnly = true;
                    cnt--;
                    //text박스들 초기화
                    //txtLastQty.Text = "";
                    txtMergeQty.Text = "";
                    txtComment.Document.Blocks.Clear();

                }
            }

            SetLotListCombo();

        }

        private bool ValidLotHoldRSV(string LOTID)
        {
            DataTable indata = new DataTable();
            indata.Columns.Add("LOTID", typeof(string));

            DataRow row = indata.NewRow();
            row["LOTID"] = LOTID;
            indata.Rows.Add(row);

            //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_RSV_USE_CHK", "RQSTDT", "RSLTDT", indata);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_RSV_CHK_FOR_OCAP", "RQSTDT", "RSLTDT", indata);

            if (dtResult.Rows.Count > 0)
                return false;

            return true;
        }

        private void initGridTable()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgMergeInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgMergeInfo.BeginEdit();
            dgMergeInfo.ItemsSource = DataTableConverter.Convert(dt);
            dgMergeInfo.EndEdit();
        }

        private void MergeButtonClickProcess()
        {
            if (txtMergeQty.Text.ToString().Equals(0))
            {
                Util.MessageValidation("SFU1251");  //MERGE량을 입력하지 않았습니다.
                return;
            }

            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("LOTID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));
                        inData.Columns.Add("OUTPUT_QTY", typeof(decimal));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["LOTID"] = Convert.ToString(cboLot.SelectedValue);
                        row["NOTE"] = new TextRange(txtComment.Document.ContentStart, txtComment.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;
                        row["OUTPUT_QTY"] = Decimal.Parse(txtMergeQty.Text);
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable formLotID = indataSet.Tables.Add("IN_FROMLOT");
                        formLotID.Columns.Add("LOTID", typeof(string));
                        formLotID.Columns.Add("INPUT_WIPQTY", typeof(decimal));


                        for (int i = 0; i < dgMergeInfo.GetRowCount(); i++)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID")).Equals(Util.NVC(Convert.ToString(cboLot.SelectedValue))))
                            {
                                row = formLotID.NewRow();
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID"));
                                row["INPUT_WIPQTY"] = Decimal.Parse(Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "MERGEQTY")));
                                indataSet.Tables["IN_FROMLOT"].Rows.Add(row);
                            }

                        }

                        loadingIndicator.Visibility = Visibility.Visible;

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_LOT_QTY", "INDATA,IN_FROMLOT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU2009");  //합권되었습니다.
                                SearchData();
                            }
                            catch (Exception ex)
                            {

                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);



                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });

        }
        private void SearchData()
        {
            try
            {
                ClearData();
                initGridTable();

                DataTable indata = new DataTable();
                indata.Columns.Add("LANGID", typeof(string));
                indata.Columns.Add("SHOPID", typeof(string));
                indata.Columns.Add("AREAID", typeof(string));
                indata.Columns.Add("EQSGID", typeof(string));
                indata.Columns.Add("PROCID", typeof(string));
                indata.Columns.Add("PRJT_NAME", typeof(string));
                indata.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                if (cboArea.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1499");  //동을 선택하세요.
                    return;
                }
                if (cboEquipmentSegment.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1223");  //라인을 선택하세요.
                    return;
                }
                if (cboProcess.Text.Equals("-SELECT-"))
                {
                    Util.MessageValidation("SFU1459");  //공정을 선택하세요.
                    return;
                }

                _ProIDMerge = Convert.ToString(cboProcess.SelectedValue); // [E20230807-000061] speical work-LOT MERGE improvement                

                DataRow row = indata.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SHOPID"] = Convert.ToString(cboShop.SelectedValue);
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["PRJT_NAME"] = Convert.ToString(txtLotID.Text.Trim().Equals("") ? null : Convert.ToString(txtLotID.Text.Trim()));
                row["PRDT_CLSS_CODE"] = Convert.ToString(cboElecType.SelectedValue).Equals("") ? null : Convert.ToString(cboElecType.SelectedValue);

                indata.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_LIST", "RQSTDT", "RSLTDT", indata);
                Util.GridSetData(dgLotList, result, FrameOperation, true);

                if (cboLot.Items.Count > 0)
                {
                    SetLotListCombo();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region[CANCEL]
        private void SelectProcess()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("SHOPID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FROM", typeof(string));
                dt.Columns.Add("TO", typeof(string));
                dt.Columns.Add("WIPSTAT", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SHOPID"] = Convert.ToString(cboCancelShop.SelectedValue).Equals("") ? null : Convert.ToString(cboCancelShop.SelectedValue);
                row["AREAID"] = Convert.ToString(AREA.SelectedValue).Equals("") ? null : Convert.ToString(AREA.SelectedValue);
                row["EQSGID"] = Convert.ToString(EQUIPMENTSEGMENT.SelectedValue).Equals("") ? null : Convert.ToString(EQUIPMENTSEGMENT.SelectedValue);
                row["PROCID"] = Convert.ToString(PROCESS.SelectedValue).Equals("") ? null : Convert.ToString(PROCESS.SelectedValue);
                row["FROM"] = Util.GetCondition(ldpDatePickerFrom);
                row["TO"] = Util.GetCondition(ldpDatePickerTo);
                row["WIPSTAT"] = Wip_State.TERM;
                dt.Rows.Add(row);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTRACE_MERGE_CANCEL", "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgCLotList, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgCLotList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                if (dgCLotList.GetRowCount() == 0) return;

                List<System.Data.DataRow> list = DataTableConverter.Convert(dgCLotList.ItemsSource).Select().ToList();
                List<int> arr = list.GroupBy(c => c["LOTID"]).Select(group => group.Count()).ToList();


                int p = 0;
                for (int j = 0; j < arr.Count; j++)
                {
                    for (int i = 0; i < dgCLotList.Columns.Count; i++)
                    {
                        if (!dgCLotList.Columns[i].Name.Equals("FROM_LOTID"))
                        {
                            e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, i), dgCLotList.GetCell((p + arr[j] - 1), i)));
                        }
                       
                    }

                    p += arr[j];
                    //e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, 0), dgCLotList.GetCell((p + arr[j] - 1), 0)));
                    //e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, 8), dgCLotList.GetCell((p + arr[j] - 1), 8)));
                    //p += arr[j];
                }


            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name, null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region[MERGE_HIST]

        private void MergeHistSearchData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                //dt.Columns.Add("SHOPID", typeof(string));
                //dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("FROM", typeof(string));
                dt.Columns.Add("TO", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                //row["SHOPID"] = Convert.ToString(cboHistShop.SelectedValue).Equals("") ? null : Convert.ToString(cboHistShop.SelectedValue);
                //row["AREAID"] = Convert.ToString(cboHistAREA.SelectedValue).Equals("") ? null : Convert.ToString(cboHistAREA.SelectedValue);
                row["EQSGID"] = Convert.ToString(cboHistEquipmentSegment.SelectedValue).Equals("") ? null : Convert.ToString(cboHistEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboHistProcess.SelectedValue).Equals("") ? null : Convert.ToString(cboHistProcess.SelectedValue);
                row["FROM"] = Util.GetCondition(ldpDatePickerHistFrom);
                row["TO"] = Util.GetCondition(ldpDatePickerHistTo);
                dt.Rows.Add(row);

                //DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTRACE_MERGE_CANCEL", "RQSTDT", "RSLTDT", dt);
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MERGE_LOT_HIST_V01", "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgMergeHistList, result, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        private void ClearData()
        {
            dgMergeInfo.ItemsSource = null;
            cboLot.ItemsSource = null;

            txtMergeQty.Text = "";
            //txtLastQty.Text = "";
            txtComment.Document.Blocks.Clear();

            cnt = 0;
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void cboElecType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedElecType = Convert.ToString(cboElecType.SelectedValue);
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sLotid = string.Empty;
                sLotid = txtLotID.Text.Trim();
            }
        }

     
    }
}
