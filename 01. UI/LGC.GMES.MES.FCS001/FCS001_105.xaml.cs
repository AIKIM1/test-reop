/*************************************************************************************
 Created Date : 2021.03.09
      Creator : 신광희
   Decription : 외부 보관 Box 구성
--------------------------------------------------------------------------------------
 [Change History]
  2021.03.09  신광희 : Initial Created
  2021.03.22  조영대 : 장기 보류 체크박스 및 인수 추가
  2021.03.29  조영대 : 모델별 Pallet 구성 수량 기준 버튼
  2023.10.04  조영대 : 선택 없이 추가버튼 시 Validation 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_105 : UserControl,IWorkArea
    {
        #region [Declaration & Constructor]
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly string _lotDetailTypeCode = "R,N";

        private string _workReSetTime = string.Empty;
        private string _workEndTime = string.Empty;
        private DataTable dtsubLotList = new DataTable();

        #endregion

        #region [Initialize]
        public FCS001_105()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetWorkResetTime();
            //Combo Setting            
            InitializeCombo();
            //Control Setting
            InitializeControl();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitializeCombo()
        {
            CommonCombo_Form combo = new CommonCombo_Form();

            // 동
            combo.SetCombo(cboArea, CommonCombo_Form.ComboStatus.NONE, sCase: "AREA");

            // 라인
            SetEquipmentSegmentCombo(cboLine);

            // 공정 그룹
            cboProcGroup.SetCommonCode("PROC_GR_CODE", CommonCombo.ComboStatus.ALL, false);

            SetProcessCombo(cboProcess);

            // Lot 유형
            string[] sFilter = { "LOT_DETL_TYPE_CODE", "R,N" };
            combo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "FORM_CMN", sFilter: sFilter);

            // 불량
            cboDefect.SetCommonCode("OUTER_WH_DFCT_GR_CODE", CommonCombo.ComboStatus.NA, false);

            // 수리
            cboRepair.SetCommonCode("OUTER_WH_REP_GR_CODE", "CBO_CODE = 'NONE'", CommonCombo.ComboStatus.NA, false);
        }

        private void InitializeControl()
        {
            // Util 에 해당 함수 추가 필요.
            dtpFromDate.SelectedDateTime = GetJobDateFrom();
            dtpToDate.SelectedDateTime = GetJobDateTo();

            //dgSelectedList.ItemsSource = DataTableConverter.Convert(Util.MakeDataTable(dgSelectedList, true));
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_SCAN_CELL_FOR_REG_BOX_UI";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("MDLLOT_ID", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = cboLine.SelectedValue;
                newRow["PROCID"] = cboProcess.SelectedValue;    //cboProcess.SelectedValue;    //"FF3101";
                newRow["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();
                newRow["PROD_LOTID"] = !string.IsNullOrEmpty(txtPkgLotID.Text) ? txtPkgLotID.Text : null;
                newRow["LOT_DETL_TYPE_CODE"] = string.IsNullOrEmpty(cboLotType.SelectedValue.GetString()) ? null : cboLotType.SelectedValue.GetString();
                newRow["MDLLOT_ID"] = !string.IsNullOrEmpty(txtModelLotId.Text) ? txtModelLotId.Text : null;
                newRow["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd");
                inTable.Rows.Add(newRow);

                //DataSet ds = new DataSet();
                //ds.Tables.Add(inTable);
                //string xml = ds.GetXml();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        Util.GridSetData(dgTargetList, result, FrameOperation, true);
                        Util.GridSetData(dgSelectedList, result.Clone(), FrameOperation, true);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetSubLotList()
        {

            DataTable dt = ((DataView)dgTargetList.ItemsSource).Table;
            string selectedLotIdList = string.Join(",", dt.Rows.OfType<DataRow>().Where(x => x["CHK"].ToString().Equals("True")).Select(y => y["LOTID"].ToString()).ToArray());

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTID"] = selectedLotIdList;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SCAN_CELL_FOR_REG_BOX_SUBLOT_LIST", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtsubLotList))
            {
                // 2. SUB LOT 테이블의 데이터 삭제
                dtsubLotList.AsEnumerable()
                    .Where(s => dtResult.AsEnumerable().Where(x => s.Field<string>("LOTID").ToString().Equals(x.Field<string>("LOTID")) ).Any()).ToList<DataRow>()
                    .ForEach(row => row.Delete());
                //dtsubLotList.AcceptChanges();

                dtsubLotList.Merge(dtResult.Copy());
                dtsubLotList.AcceptChanges();

            }
            else
            {
                dtsubLotList = dtResult.Copy();
            }
        }

        private void GetBoxCount()
        {
            txtCellCount.Value = 0;
            txtBoxCount.Value = 0;
            txtBoxCellCount.Value = 0;

            // 1.Cell 수량 선택 목록 수량 합계
            txtCellCount.Value = ((DataView)dgSelectedList.ItemsSource).Table.AsEnumerable().Sum(s => s.Field<Int32>("CNT"));

            DataRow drSelect = dgTargetList.GetCheckedDataRow("CHK").FirstOrDefault();
            if (drSelect != null && !Util.IsNVC(drSelect["MAX_QTY"].ToString()))
            {
                // 2. Box구성 Cell 수량기준
                txtBoxCellCount.Value = Convert.ToInt32(drSelect["MAX_QTY"]);
                
                // 3. 구성박스 수
                txtBoxCount.Value = Convert.ToInt32(Math.Truncate(txtCellCount.Value / txtBoxCellCount.Value)) + 1;
            }
        }

        private void CreateBox()
        {

            // 1.박스 구성 Cell 수량 기준으로 BizRule 호출 함.(N 번 호출도 가능) 
            const string bizRuleName = "BR_PRD_REG_FORM_OUTER_WH_BOX";

            DataSet ds = new DataSet();
            
            DataTable inTable = ds.Tables.Add("INDATA");
            inTable.Columns.Add("USERID", typeof(string));
            inTable.Columns.Add("MDLLOT_ID", typeof(string));
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("LONG_TERM_HOLD_FLAG", typeof(string));
            inTable.Columns.Add("BOX_OUTER_WH_DFCT_GR_CODE", typeof(string));
            inTable.Columns.Add("BOX_OUTER_WH_REPAIR_GR_CODE", typeof(string));

            DataRow row = inTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            row["MDLLOT_ID"] = dtsubLotList.Rows[0]["MDLLOT_ID"].GetString();
            row["LANGID"] = LoginInfo.LANGID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["LONG_TERM_HOLD_FLAG"] = chkLongHold.IsChecked.Equals(true) ? "Y" : null;
            row["BOX_OUTER_WH_DFCT_GR_CODE"] = cboDefect.GetBindValue();
            row["BOX_OUTER_WH_REPAIR_GR_CODE"] = cboRepair.GetBindValue();
            inTable.Rows.Add(row);

            DataTable inCellTable = ds.Tables.Add("INCELL");
            inCellTable.Columns.Add("SUBLOTID", typeof(string));

            for (int i = 0; i < dtsubLotList.Rows.Count; i++)
            {
                DataRow newRow = inCellTable.NewRow();
                newRow["SUBLOTID"] = dtsubLotList.Rows[i]["SUBLOTID"];
                inCellTable.Rows.Add(newRow);
            }

            //string xml = ds.GetXml();

            new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCELL", "OUTDATA", (result, bizException) =>
            {
                if (bizException != null)
                {
                    Util.MessageException(bizException);
                    return;
                }
                else
                {
                    // 포장구성이 완료되었습니다.
                    Util.MessageValidation("SFU2980", (message) =>
                    {
                        BoxBarCodeAutoPrint(result?.Tables["OUTDATA"]);
                        btnSearch_Click(btnSearch, null);
                    });
                }

            }, ds);

        }

        private void BoxBarCodeAutoPrint(DataTable dt)
        {
            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  // LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     // Model Code
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     // 생성일자
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     // BOX ID BCD
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     // BOX ID
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     // PJT
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     // 수량
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     // 박스/Pallet 보관 유형 명
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM010", typeof(string));     //
            dtLabelItem.Columns.Add("ITEM011", typeof(string));     //

            DataRow dr = dtLabelItem.NewRow();
            dr["LABEL_CODE"] = "LBL0283";
            dr["ITEM001"] = dt.Rows[0]["MODLID"].GetString();
            dr["ITEM002"] = dt.Rows[0]["BOX_DATE"].GetString();
            dr["ITEM003"] = dt.Rows[0]["OUTER_WH_BOX_ID"].GetString();
            dr["ITEM004"] = dt.Rows[0]["OUTER_WH_BOX_ID"].GetString();
            dr["ITEM005"] = dt.Rows[0]["PRJT_NAME"].GetString();
            dr["ITEM006"] = dt.Rows[0]["CNT"].GetString();

            //dr["ITEM007"] = dt.Rows[0]["DFCT_NAME"].GetString();
            if (!Util.IsNVC(cboDefect.GetBindValue()) || !Util.IsNVC(cboRepair.GetBindValue()))
            {
                dr["ITEM007"] = Util.NVC(cboDefect.Text) + "-" + Util.NVC(cboRepair.Text);
            }
            else
            {
                dr["ITEM007"] = string.Empty;
            }
            
            dtLabelItem.Rows.Add(dr);

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);

            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다.
                Util.MessageValidation("SFU3243");
            }
            else
            {

            }
        }

        private void SetControlClear()
        {
            Util.gridClear(dgTargetList);
            Util.gridClear(dgSelectedList);
            dtsubLotList.Clear();
            txtCellCount.Value = 0;
            txtBoxCount.Value = 0;
            txtBoxCellCount.Value = 0;
        }

        private void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_LINE";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_PROC_BY_LINE";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "S26" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, null,
                cboProcGroup.GetStringValue().IsNullOrEmpty() ? null : cboProcGroup.GetStringValue() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_PROC_ID);
        }

        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMBO_FORM_EQP_BY_PROC";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboLine.SelectedValue == null ? null : cboLine.SelectedValue.ToString(), cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString() };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(_workReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + _workReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(_workEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + _workEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            _workReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            _workEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }

        private void SetDataGridCheckHeaderInitialize(C1DataGrid dg)
        {
            C1.WPF.DataGrid.DataGridCheckBoxColumn allColumn = dg.Columns["CHK"] as C1.WPF.DataGrid.DataGridCheckBoxColumn;
            StackPanel allPanel = allColumn?.Header as StackPanel;
            CheckBox allCheck = allPanel?.Children[0] as CheckBox;
            if (allCheck?.IsChecked == true)
            {
                allCheck.Unchecked -= chkHeaderAll_Unchecked;
                allCheck.IsChecked = false;
                allCheck.Unchecked += chkHeaderAll_Unchecked;
            }
        }

        private bool ValidationGrid(DataRow drCheck)
        {
            if (!CommonVerify.HasDataGridRow(dgTargetList))
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (_util.GetDataGridRowCountByCheck(dgTargetList, "CHK", true) < 1)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }

            if (drCheck == null) return true;

            DataTable dtData = new DataTable();

            if (dgSelectedList.ItemsSource != null)
            {
                dtData = DataTableConverter.Convert(dgSelectedList.ItemsSource);
            }

            if (dtData.Rows.Count > 0)
            {
                //양품 BOX, 불량 BOX 혼입 불가
                if (string.IsNullOrEmpty(dtData.Rows[0]["DFCT_CODE"].ToString()))
                {
                    if (!string.IsNullOrEmpty(drCheck["DFCT_CODE"].ToString()))
                    {
                        //양품과 불량은 동일 PALLET로 구성 할 수 없습니다.
                        Util.MessageValidation(MessageDic.Instance.GetMessage("FM_ME_0406", new object[] { drCheck["LOTID"] }));
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(dtData.Rows[0]["DFCT_CODE"].ToString()))
                {
                    if (string.IsNullOrEmpty(drCheck["DFCT_CODE"].ToString()))
                    {
                        //양품과 불량은 동일 PALLET로 구성 할 수 없습니다.
                        Util.MessageValidation(MessageDic.Instance.GetMessage("FM_ME_0406", new object[] { drCheck["LOTID"] }));
                        return false;
                    }
                }

                //동일 PKG LOT만 PLLT 구성 가능
                if (dtData.Rows[0]["PROD_LOTID"].ToString() != drCheck["PROD_LOTID"].ToString())
                {
                    //동일 Pakage Lot만 동일 PLLT로 구성 가능 합니다.
                    Util.MessageValidation(MessageDic.Instance.GetMessage("FM_ME_0407", new object[] { drCheck["LOTID"] }));
                    return false;
                }

                //동일 모델만 PLLT 구성 가능 - 동일 Pakage LOT Check를 먼저 하기 때문에, 필요 없지만..
                if (dtData.Rows[0]["MDLLOT_ID"].ToString() != drCheck["MDLLOT_ID"].ToString())
                {
                    //동일 모델만 동일 PALLET로 구성 가능합니다.
                    Util.MessageValidation(MessageDic.Instance.GetMessage("FM_ME_0408", new object[] { drCheck["LOTID"] }));
                    return false;
                }
            }

            return true;
        }

        private bool ValidationCreateBox()
        {
            if (!CommonVerify.HasTableRow(dtsubLotList) || !CommonVerify.HasDataGridRow(dgSelectedList))
            {
                Util.MessageValidation("SFU2812");
                return false;
            }

            if (cboDefect.GetBindValue() != null && cboRepair.GetBindValue() == null)
            {
                Util.MessageValidation("SFU4925", lblRepair.Text);
                return false;
            }
            return true;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region [Event]
        private void dgTargetList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 공정 
            //SetProcessCombo(cboProcess);
            // Clear
            Util.gridClear(dgTargetList);
        }

        private void cboProcGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetProcessCombo(cboProcess);
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 설비 
            SetEquipmentCombo(cboEquipment);
        }

        private void txtPkgLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtPkgLotID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(btnSearch, null);
        }

        private void txtModelLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtModelLotId.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(btnSearch, null);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SetDataGridCheckHeaderInitialize(dgTargetList);
            SetDataGridCheckHeaderInitialize(dgSelectedList);
            SetControlClear();
            GetList();
        }


        private void dgTargetList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                /*
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
                */
            }));
        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgTargetList, true);
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList, true);
        }

        private void chkSelectedHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllChecked(dgSelectedList, true);
        }

        private void chkSelectedHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgSelectedList, true);
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validation 
            if (!ValidationGrid(null)) return;

            DataTable dtTargetList = ((DataView)dgTargetList.ItemsSource).Table;
            DataTable dtSelectedtList = ((DataView)dgSelectedList.ItemsSource).Table;

            // 2. SUB LOT 테이블의 데이터 삭제
            dtsubLotList.AsEnumerable()
                .Where(s => dtSelectedtList.AsEnumerable().Where(x => s.Field<string>("LOTID").ToString().Equals(x.Field<string>("LOTID")) && x.Field<string>("CHK").GetString() == "True").Any()).ToList<DataRow>()
                .ForEach(row => row.Delete());
            dtsubLotList.AcceptChanges();

            // 3. dgTargetList 그리드 데이터 추가 
            //foreach (DataRow row in dtSelectedtList.Select("CHK = True"))
            //{
            //    dtTargetList.ImportRow(row);
            //}
            //dtTargetList.AcceptChanges();

            // 4. dgSelectedList 그리드 데이터 삭제
            //dtSelectedtList.Select("CHK = True").ToList<DataRow>().ForEach(row => row.Delete());
            //dtSelectedtList.AcceptChanges();

            dtSelectedtList.AsEnumerable()
                .Where(s => dtTargetList.AsEnumerable().Where(x => s.Field<string>("LOTID").ToString().Equals(x.Field<string>("LOTID")) && x.Field<string>("CHK").GetString() == "True").Any()).ToList<DataRow>()
                .ForEach(row => row.Delete());
            dtSelectedtList.AcceptChanges();

            // 5. Cell 수량 구성 박스 수 계산
            GetBoxCount();

            // 전체 선택인 경우 그리드 Check박스 헤더 제거
            if (dgSelectedList.GetRowCount() < 1)
                SetDataGridCheckHeaderInitialize(dgSelectedList);
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CommonVerify.HasDataGridRow(dgTargetList))
                {
                    Util.MessageValidation("SFU1636");
                    return;
                }

                if (_util.GetDataGridRowCountByCheck(dgTargetList, "CHK", true) < 1)
                {
                    Util.MessageValidation("SFU1636");
                    return;
                }

                DataTable dtTargetList = ((DataView)dgTargetList.ItemsSource).Table;
                DataTable dtSelectedtList = ((DataView)dgSelectedList.ItemsSource).Table;

                foreach (DataRow drCheck in dgTargetList.GetCheckedDataRow("CHK"))
                {
                    if (!ValidationGrid(drCheck)) continue;

                        var query = (from t in dtSelectedtList.AsEnumerable()
                                     where t.Field<string>("LOTID") == drCheck["LOTID"].GetString()
                                     select t).ToList();

                        if (!query.Any())
                        {
                            dtSelectedtList.ImportRow(drCheck);
                        }
                }

                dtSelectedtList.AcceptChanges();

                dgSelectedList.SetItemsSource(dtSelectedtList, FrameOperation, true);
                
                GetSubLotList();

                GetBoxCount();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void cboDefect_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboDefect.GetBindValue() != null)
            {
                if (cboDefect.GetStringValue().Equals("ETC"))
                {
                    cboRepair.SetCommonCode("OUTER_WH_REP_GR_CODE", "CBO_CODE = 'ETC'", CommonCombo.ComboStatus.NA, false);
                }
                else
                {
                    cboRepair.SetCommonCode("OUTER_WH_REP_GR_CODE", "CBO_CODE <> 'ETC'", CommonCombo.ComboStatus.NA, false);
                }
            }
            else
            {
                cboRepair.SetCommonCode("OUTER_WH_REP_GR_CODE", "CBO_CODE = 'NONE'", CommonCombo.ComboStatus.NA, false);
            }
        }

        private void btnBoxCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreateBox()) return;

            Util.MessageConfirm("SFU6014", (result) =>// 포장구성 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    CreateBox();
                }
            });
        }

        private void btnLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_105_LABEL_PRINT wndLabelPrint = new FCS001_105_LABEL_PRINT();
                wndLabelPrint.FrameOperation = FrameOperation;

                object[] parameters = new object[6];
                parameters[0] = cboArea.SelectedValue;
                parameters[1] = cboLine.SelectedValue;
                parameters[2] = cboProcGroup.SelectedValue;
                parameters[3] = cboProcess.SelectedValue;
                parameters[4] = dtpFromDate.SelectedDateTime;
                parameters[5] = dtpToDate.SelectedDateTime;
                C1WindowExtension.SetParameters(wndLabelPrint, parameters);

                if (wndLabelPrint != null)
                {
                    Dispatcher.BeginInvoke(new Action(() => wndLabelPrint.ShowModal()));
                    wndLabelPrint.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnBoxCreateByModel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FCS001_105_MAX_TRF_QTY popup = new FCS001_105_MAX_TRF_QTY();
                popup.FrameOperation = FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = cboArea.GetStringValue();

                    C1WindowExtension.SetParameters(popup, Parameters);

                    Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                    popup.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
