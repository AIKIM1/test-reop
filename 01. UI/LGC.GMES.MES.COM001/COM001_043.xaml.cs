/*************************************************************************************
 Created Date : 2017.08.31
      Creator : 
   Decription : 조립용 합권취 화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.08.31  DEVELOPER : Initial Created.
  2024.07.02  김영택(ytkim29) : 합권취소 탭, 합권된 lot 컬럼 오류 수정 
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

    public partial class COM001_043 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util util = new Util();

        int MAXMERGECOUNT = 10;
        int cnt = 0;
        int checkIndex; //합권취소에서 그리드 클릭시 index저장
        string temp = string.Empty;
        private string selectedElecType = string.Empty;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_043()
        {
            InitializeComponent();
            SetVisibility(Visibility.Collapsed);
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

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //string[] sFilter = { Convert.ToString(cboEquipmentSegment.SelectedValue), Convert.ToString( cboProcess.SelectedValue), null};
            //combo.SetCombo(cboModel, CommonCombo.ComboStatus.SELECT, sFilter:sFilter, sCase: "cboModelMerge");
        }
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
                MergeSum(Convert.ToString(cboLot.SelectedValue));

            }





        }
        private void cboLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboLot.Items.Count > 1)
            {
                MergeSum(Convert.ToString(cboLot.SelectedValue));

            }
        }
        private void MergeSum(string lotid)
        {
            List<DataRow> QtyList = DataTableConverter.Convert(dgMergeInfo.ItemsSource).AsEnumerable().Where(c => !string.IsNullOrWhiteSpace(c.Field<string>("WIPQTY"))).ToList();

            txtMergeQty.Text = Convert.ToString(QtyList.AsEnumerable().Sum(c => Double.Parse(c.Field<string>("WIPQTY"))));


            DataTable dt = DataTableConverter.Convert(dgMergeInfo.ItemsSource).Select("LOTID <> '" + lotid + "'").CopyToDataTable();
            txtLastQty.Text = Convert.ToString(dt.AsEnumerable().Sum(c => double.Parse(c.Field<string>("WIPQTY"))));
        }



        private void btnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgMergeInfo.ItemsSource);
                if (dt.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU3218"); //합권 할 LOT이 없습니다.
                    return;
                }
                if (dt.Rows.Count == 1)
                {
                    Util.MessageValidation("SFU1140");  //LOT은 2개 이상 이어야 합니다.
                    return;
                }

                MergeButtonClickProcess();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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
            //합권취소 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2010"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    try
                    {

                        DataTable dt = new DataTable();
                        dt.Columns.Add("SRCTYPE", typeof(string));
                        dt.Columns.Add("LOTID", typeof(string));
                        dt.Columns.Add("NOTE", typeof(string));
                        dt.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgCLotList.Rows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCLotList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgCLotList.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow row = dt.NewRow();
                                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgCLotList.Rows[i].DataItem, "LOTID"));
                                row["NOTE"] = "";
                                row["USERID"] = LoginInfo.USERID;
                                dt.Rows.Add(row);
                            }
                        }

                        new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CANCEL_MERGE_LOT", "INDATA", null, dt);
                        Util.AlertInfo("SFU2008");  //합권 취소 완료

                        SelectProcess();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
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


            ////합권취소 콤보 셋팅
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


        }
        private void SetLotListCombo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CBO_NAME", typeof(string));
            dt.Columns.Add("CBO_CODE", typeof(string));

            DataRow row = null;

            for (int i = 0; i < dgMergeInfo.Rows.Count; i++)
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

            if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "CHK")).Equals("1"))
            {
                if (cnt == MAXMERGECOUNT)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK", false);
                    Util.MessageValidation("SFU1221");  //더 이상 추가 할 수 없습니다.
                    return;
                }

                for (int i = 0; i < dgMergeInfo.Rows.Count; i++)
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

                //if (DataTableConverter.Convert(dgMergeInfo.ItemsSource).Rows.Count >= 1)
                //{
                //    if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "VERSION")).Equals(Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[0].DataItem, "VERSION"))))
                //    {
                //        Util.MessageValidation("SFU1501");  //동일 버전이 아닙니다.
                //        DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK" , false);
                //        return;
                //    }

                //    if (!Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRODID")).Equals(Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[0].DataItem, "PRODID"))))
                //    {
                //        Util.MessageValidation("SFU1502");  //동일 제품이 아닙니다.
                //        DataTableConverter.SetValue(dgLotList.Rows[index].DataItem, "CHK", false);
                //        return;
                //    }


                //}

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
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "UNIT", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "UNIT")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRODID", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRODID")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRODNAME", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRODNAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "STATUS", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "WIPSNAME")));
                DataTableConverter.SetValue(dgMergeInfo.CurrentRow.DataItem, "PRJT_NAME", Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[index].DataItem, "PRJT_NAME")));

                //dgmerge에 추가

                dgMergeInfo.IsReadOnly = true;
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
                    txtLastQty.Text = "";
                    txtMergeQty.Text = "";
                    txtComment.Document.Blocks.Clear();

                }
            }

            SetLotListCombo();

            if (txtCSTID.Visibility == Visibility.Visible)
                txtCSTID.Text = String.Empty;
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

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["LOTID"] = Convert.ToString(cboLot.SelectedValue);
                        row["NOTE"] = new TextRange(txtComment.Document.ContentStart, txtComment.Document.ContentEnd).Text;
                        row["USERID"] = LoginInfo.USERID;
                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable formLotID = indataSet.Tables.Add("IN_FROMLOT");
                        formLotID.Columns.Add("FROM_LOTID", typeof(string));


                        for (int i = 0; i < dgMergeInfo.Rows.Count; i++)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID")).Equals(Util.NVC(Convert.ToString(cboLot.SelectedValue))))
                            {
                                row = formLotID.NewRow();
                                row["FROM_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMergeInfo.Rows[i].DataItem, "LOTID"));
                                indataSet.Tables["IN_FROMLOT"].Rows.Add(row);
                            }

                        }



                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MERGE_LOT", "INDATA,IN_FROMLOT", null, indataSet);


                        Util.AlertInfo("SFU2009");  //합권되었습니다.
                        SearchData();

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
                SetGridVisibility_CSTcolumn();
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
                //if (txtLotID.Text.Equals("-SELECT-"))
                //{
                //    Util.MessageValidation("SFU1225");  //모델을 선택하세요.
                //    return;
                //}

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
                Util.GridSetData(dgLotList, result, FrameOperation);

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

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["SHOPID"] = Convert.ToString(cboCancelShop.SelectedValue).Equals("") ? null : Convert.ToString(cboCancelShop.SelectedValue);
                row["AREAID"] = Convert.ToString(AREA.SelectedValue).Equals("") ? null : Convert.ToString(AREA.SelectedValue);
                row["EQSGID"] = Convert.ToString(EQUIPMENTSEGMENT.SelectedValue).Equals("") ? null : Convert.ToString(EQUIPMENTSEGMENT.SelectedValue);
                row["PROCID"] = Convert.ToString(PROCESS.SelectedValue).Equals("") ? null : Convert.ToString(PROCESS.SelectedValue);
                row["FROM"] = ldpDatePickerFrom.SelectedDateTime.ToShortDateString();
                row["TO"] = ldpDatePickerTo.SelectedDateTime.ToShortDateString();
                dt.Rows.Add(row);


                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_MERGE_CANCEL", "RQSTDT", "RSLTDT", dt);
                Util.GridSetData(dgCLotList, result, FrameOperation);

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
                    //e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, 0), dgCLotList.GetCell((p + arr[j] - 1), 0)));
                    //e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, 7), dgCLotList.GetCell((p + arr[j] - 1), 7)));
                    //p += arr[j];

                    // 2024-07-02 병합된 lot 정보 로딩 수정 
                    for (int i = 0; i < dgCLotList.Columns.Count; i++)
                    {
                        if (!dgCLotList.Columns[i].Name.Equals("FROM_LOTID"))
                        {
                            e.Merge(new DataGridCellsRange(dgCLotList.GetCell(p, i), dgCLotList.GetCell((p + arr[j] - 1), i)));
                        }

                    }
                    p += arr[j];
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(new System.Diagnostics.StackFrame(0, true).GetMethod().Name, null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #endregion

        private void ClearData()
        {
            dgMergeInfo.ItemsSource = null;

            txtMergeQty.Text = "";
            txtLastQty.Text = "";
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


        #region [SET Visibility CSTID ]
        /// <summary>
        /// CASSETTE 컬럼 추가설정 2017.08.30 Add by Choi seung hyeok 
        /// 카세트 ID사용하는 라인의 합권취 시에 CSTID컬럼 추가
        /// </summary>
        private void SetGridVisibility_CSTcolumn()
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dtRQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = Util.NVC(cboProcess.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue.ToString());
                dtRQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "RQSTDT", "RSLDT", dtRQSTDT);

                if (dtRslt.Rows.Count > 0)
                {
                    switch (Util.NVC(cboProcess.SelectedValue))
                    {
                        case "A8000":
                            if (dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString().Trim().ToUpper().Equals("CST_ID"))
                            {
                                SetVisibility(Visibility.Visible);
                            }
                            else
                            {

                                SetVisibility(Visibility.Collapsed);
                            }
                            break;
                        case "A9000":
                            if (dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString().Trim().ToUpper().Equals("CST_ID"))
                            {
                                SetVisibility(Visibility.Visible);
                            }
                            else
                            {
                                SetVisibility(Visibility.Collapsed);
                            }
                            break;
                        default:
                            SetVisibility(Visibility.Collapsed);
                            break;
                    }
                }
                else
                {
                    SetVisibility(Visibility.Collapsed);
                }



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        /// <summary>
        /// SET GRID VISIBILITY  2017.08.30 Add by Choi seung hyeok  
        /// </summary>
        private void SetVisibility(Visibility val)
        {
            try
            {
                dgLotList.Columns["CSTID"].Visibility = val;
                dgMergeInfo.Columns["CSTID"].Visibility = val;
                dgCLotList.Columns["CSTID"].Visibility = val;

                txtCSTID.Visibility = val;

                lblTXT.Visibility = val;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// CSTID 입력시 GRID에 존재하는 LOT ADD  2017.08.30 Add by Choi seung hyeok  
        /// </summary>
        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            string sCSTid = string.Empty;
            if (e.Key == Key.Enter)
            {                
                sCSTid = txtCSTID.Text.Trim();
            }

            if (string.IsNullOrEmpty(sCSTid)) return;

            for (int i = 0; i < dgLotList.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[i].DataItem, "CSTID")) == sCSTid)
                {
                    DataTableConverter.SetValue(dgLotList.Rows[i].DataItem, "CHK", true);
                    CheckBoxClickProcess(i);

                    return;
                }

            }


        }
        #endregion
    }
}
