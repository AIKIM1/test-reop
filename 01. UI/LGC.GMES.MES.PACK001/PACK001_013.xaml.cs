/*************************************************************************************
  Created Date : 2016.06.16
      Creator : 
   Decription : 불량유형 변경
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2019.11.19  손우석    : 불량유형 변경 개선
  2022.06.22  정용석    : 프로그램 재편
  2024.06.10  강송모    : E20240401-000283 불량유형선택방법 변경
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_013 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        BackgroundWorker backgroundWorker = new BackgroundWorker();
        private int causeProcessIDMultiComboBindDataCount = 0;
        PACK001_013_DataHelper dataHelper = new PACK001_013_DataHelper();
        private DataTable dtSearch = new DataTable();

        //불량유형 code 선택을 위한 전역 변수 선언
        private DataTable dtCodeResult = new DataTable();
        private DataTable defectResult = new DataTable();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK001_013()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupReasonCode, this.pnlTitleReasonCode);
            PackCommon.SetPopupDraggable(this.popupErrorCode, this.pnlTitleErrorCode);
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            try
            {
                this.Loaded -= UserControl_Loaded;
                // BackGroundWorker
                this.backgroundWorker = new BackgroundWorker();
                this.backgroundWorker.WorkerReportsProgress = true;
                this.backgroundWorker.WorkerSupportsCancellation = true;
                this.backgroundWorker.DoWork += BackgroundWorker_DoWork;
                this.backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
                this.backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                PackCommon.SearchRowCount(ref this.txtRowCount, 0);

                // 달력
                this.dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
                this.dtpToDate.SelectedDateTime = DateTime.Now;

                // 상위 ComboBox Event에 관련되어 있지 않은 ComboBox Control
                PackCommon.SetC1ComboBox(this.dataHelper.GetEquipmentSegmentInfo(LoginInfo.CFG_AREA_ID), this.cboEquipmentSegment, true, "ALL");
                PackCommon.SetC1ComboBox(this.dataHelper.GetCommonCodeInfo("PRDT_CLSS_CODE_PACK"), this.cboProductClassCode, true, "ALL");
                PackCommon.SetC1ComboBox(this.dataHelper.GetSearchRowCount(), this.cboListCount);

                // Popup Grid
                this.InitializePopupGrid();

                // 사용자권한별로 메뉴 숨기기
                List<Button> lstButton = new List<Button>();
                lstButton.Add(btnSave);
                Util.pageAuth(lstButton, FrameOperation.AUTHORITY);

                object[] arrParameters = FrameOperation.Parameters;
                if (arrParameters != null && arrParameters.Length > 0)
                {
                    string LOTID = arrParameters[0].ToString();
                    string procIDCause = arrParameters[1].ToString();
                    decimal wipSEQ = Util.NVC_Decimal(arrParameters[2]);
                    this.SearchProcessByLOT(LOTID, procIDCause, wipSEQ);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btnSave.IsEnabled = true;
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DataTable dt = (DataTable)e.Argument;
            // 조회내역 Copy
            var query = dt.AsEnumerable().Select(x => new
            {
                LOTID = x.Field<string>("LOTID"),
                WIPSEQ = x.Field<string>("WIPSEQ"),
                RESNCODE = x.Field<string>("RESNCODE")
            });

            this.dtSearch = PackCommon.queryToDataTable(query.ToList());
        }

        // LOT ID별로 조회
        private void SearchProcessByLOT(string LOTID, string causeProcID, decimal wipSEQ)
        {
            // Search Process
            PackCommon.SearchRowCount(ref this.txtRowCount, 0);
            Util.gridClear(this.dgLOTList);
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                DataTable dt = this.dataHelper.GetRepairDefectLOT(null, null, null, null, null, null, causeProcID, null, LOTID, wipSEQ);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgLOTList, dt, FrameOperation);

                    if (!this.backgroundWorker.IsBusy)
                    {
                        this.backgroundWorker.RunWorkerAsync(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        // 조회
        private void SearchProcess(params object[] obj)
        {
            this.btnSave.IsEnabled = false;
            this.HidePopupReasonCode();
            // Validation Check...
            if (this.dtpFromDate.SelectedDateTime.Date > this.dtpToDate.SelectedDateTime.Date)
            {
                Util.MessageValidation("SFU3569");  // 조회 시작일자는 종료일자를 초과 할 수 없습니다.
                this.dtpFromDate.Focus();
                return;
            }

            if (string.IsNullOrEmpty(Convert.ToString(this.cboMultiCauseProcessID.SelectedItemsToString)))
            {
                Util.MessageValidation("SFU3300");   // 선택오류 : 불량발생 공정(필수조건) 콤보를 선택하지 않았습니다. [콤보선택]
                this.cboMultiCauseProcessID.Focus();
                return;
            }

            // Search Process
            PackCommon.SearchRowCount(ref this.txtRowCount, 0);
            Util.gridClear(this.dgLOTList);
            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();
            try
            {
                DateTime dteFromDate = this.dtpFromDate.SelectedDateTime.Date;
                DateTime dteToDate = this.dtpToDate.SelectedDateTime.Date;
                string eqsgID = this.cboEquipmentSegment.SelectedValue.ToString();
                string productModel = this.cboProductModel.SelectedValue.ToString();
                string productClassCode = this.cboProductClassCode.SelectedValue.ToString();
                string productID = this.cboProductID.SelectedValue.ToString();
                string causeProcID = this.cboMultiCauseProcessID.SelectedItems.Count().Equals(this.causeProcessIDMultiComboBindDataCount) ? string.Empty : Convert.ToString(this.cboMultiCauseProcessID.SelectedItemsToString);
                int selectCount = Convert.ToInt32(this.cboListCount.SelectedValue);

                //obj[0] = DateTime dteFromDate
                //obj[1] = DateTime dteToDate
                //obj[2] = string eqsgID
                //obj[3] = string productModel
                //obj[4] = string productClassCode
                //obj[5] = string productID
                //obj[6] = string causeProcID
                //obj[7] = int selectCount
                //obj[8] = string LOTID
                //obj[9] = decimal wipseq
                DataTable dt = this.dataHelper.GetRepairDefectLOT(dteFromDate, dteToDate, eqsgID, productModel, productClassCode, productID, causeProcID, selectCount, null, null);
                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtRowCount, dt.Rows.Count);
                    Util.GridSetData(this.dgLOTList, dt, FrameOperation);
                    this.loadingIndicator.Visibility = Visibility.Collapsed;

                    if (!this.backgroundWorker.IsBusy)
                    {
                        this.backgroundWorker.RunWorkerAsync(dt);
                    }
                }
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 저장
        private bool SaveProcess()
        {
            // Declarations...
            int maxCount = 500;
            bool returnValue = false;

            // Transaction Data 추출
            DataTable dtEditData = DataTableConverter.Convert(this.dgLOTList.ItemsSource);

            // Validation Check...
            if (dtEditData.AsEnumerable().Where(x => x.Field<string>("CHK").ToUpper().Equals("TRUE")).Count() > maxCount)
            {
                Util.MessageValidation("SFU8217", maxCount);     // 최대 500개 까지 가능합니다.
                return false;
            }

            var query = from d1 in dtEditData.AsEnumerable()
                        join d2 in this.dtSearch.AsEnumerable() on new { LOTID = d1.Field<string>("LOTID"), WIPSEQ = d1.Field<string>("WIPSEQ") } equals new { LOTID = d2.Field<string>("LOTID"), WIPSEQ = d2.Field<string>("WIPSEQ") }
                        where d1.Field<string>("CHK").ToUpper().Equals("TRUE") && !d1.Field<string>("RESNCODE").Equals(d2.Field<string>("RESNCODE"))
                        select new
                        {
                            LOTID = d1.Field<string>("LOTID"),
                            WIPSEQ = d1.Field<string>("WIPSEQ"),
                            RESNCODE = d1.Field<string>("RESNCODE"),            // 변경할 불량코드
                            RESNCODE_OLD = d2.Field<string>("RESNCODE"),        // 변경전 불량코드
                            ACTID = d1.Field<string>("ACTID"),
                            ACTDTTM = string.Format("{0:yyyy-MM-dd HH:mm:ss.fff}", d1.Field<DateTime>("ACTDTTM")),
                            PROCID_CAUSE = d1.Field<string>("PROCID_CAUSE")
                        };

            if (query.Count() <= 0)
            {
                Util.Alert("SFU1651");  //선택된 항목이 없습니다.
                return false;
            }

            DataTable dtResult = this.dataHelper.SetLOTDefectCode(query.ToList());
            if (!CommonVerify.HasTableRow(dtResult))
            {
                returnValue = true;
            }
            else
            {
                // 결과값이 있으면 Error Message 뿌려주기...
                this.HidePopupErrorCode();
                Util.gridClear(this.dgErrorCode);
                Util.GridSetData(this.dgErrorCode, dtResult, FrameOperation);
                this.ShowPopupErrorCode();
                returnValue = false;
            }

            return returnValue;
        }

        // Popup - CreateGrid
        private void InitializePopupGrid()
        {
            // 제목땡기기
            this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("불량");

            // Grid 설정
            this.dgReasonCode.Width = 350;
            this.dgReasonCode.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
            this.dgReasonCode.CommittedEdit += c1DataGrid_CommittedEdit;

            // Column 추가
            PackCommon.AddGridColumn(this.dgReasonCode, "CHECKBOX", "CHK", true);
            PackCommon.AddGridColumn(this.dgReasonCode, "TEXT", "RESNCODE", false);
            PackCommon.AddGridColumn(this.dgReasonCode, "TEXT", "RESNNAME", true);
        }

        // Popup - Popup 표출
        private void ShowPopupReasonCode(C1DataGrid c1DataGrid, System.Windows.Input.MouseButtonEventArgs e)
        {
            Util.gridClear(dgReasonCode0);
            Util.gridClear(dgReasonCode1);
            Util.gridClear(dgReasonCode2);
            Util.gridClear(dgReasonCode3);
            Util.gridClear(dgReasonCode4);

            try
            {
                // Declareations...
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }

                // 현재 선택된 dataGridCell 정보 저장
                this.popupReasonCode.Tag = null;
                this.popupReasonCode.Tag = dataGridCell;
                // Popup Grid DataBinding
                string eqsgID = DataTableConverter.GetValue((DataRowView)dataGridCell.Row.DataItem, "EQSGID").ToString();
                string causeProcID = DataTableConverter.GetValue((DataRowView)dataGridCell.Row.DataItem, "PROCID_CAUSE").ToString();
                string actID = "DEFECT_LOT";
                string resnCode = DataTableConverter.GetValue((DataRowView)dataGridCell.Row.DataItem, "RESNCODE").ToString();
                bool chkResult = this.DataBindingProcess(this.dgReasonCode, actID, eqsgID, causeProcID, resnCode);

                if (chkResult)
                {
                    // Popup 표출 위치 (정가운데)
                    if (!this.popupReasonCode.IsOpen)
                    {
                        this.popupReasonCode.Placement = PlacementMode.Center;
                        this.popupReasonCode.IsOpen = true;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - Popup 표출
        private void ShowPopupErrorCode()
        {
            if (!this.popupErrorCode.IsOpen)
            {
                this.popupErrorCode.Placement = PlacementMode.Center;
                this.popupErrorCode.IsOpen = true;
            }
        }

        // Popup - Close Popup
        private void HidePopupReasonCode()
        {
            Util.gridClear(dgReasonCode0);
            Util.gridClear(dgReasonCode1);
            Util.gridClear(dgReasonCode2);
            Util.gridClear(dgReasonCode3);
            Util.gridClear(dgReasonCode4);
            this.popupReasonCode.IsOpen = false;
            this.popupReasonCode.HorizontalOffset = 0;
            this.popupReasonCode.VerticalOffset = 0;
        }

        // Popup - Close Popup
        private void HidePopupErrorCode()
        {
            this.popupErrorCode.IsOpen = false;
            this.popupErrorCode.HorizontalOffset = 0;
            this.popupErrorCode.VerticalOffset = 0;
        }

        // Popup - Grid Data Binding Process
        private bool DataBindingProcess(C1DataGrid c1DataGrid, string actID, string eqsgID, string procID, string resnCode)
        {
            bool resultChk = false;

            Util.gridClear(c1DataGrid);

            //DataTable dt = this.dataHelper.GetDefectActivityReasonCode(actID, eqsgID, procID, resnCode);
            //var query = dt.AsEnumerable().Select(x => new
            //{
            //    CHK = x.Field<string>("RESNCODE").Equals(resnCode) ? true : false,
            //    RESNCODE = x.Field<string>("RESNCODE"),
            //    RESNNAME = x.Field<string>("RESNNAME")
            //});

            //Util.GridSetData(c1DataGrid, PackCommon.queryToDataTable(query.ToList()), FrameOperation);
            dtCodeResult = this.dataHelper.GetDefectActivityReasonCode(actID, eqsgID, procID, resnCode);
            setDefectResult();

            if (defectResult != null && defectResult.Rows.Count > 0)
            {
                Util.GridSetData(dgReasonCode0, getNextRESNCODE(new string[0]), FrameOperation, true);
                resultChk = true;
            }
            else
            {
                Util.AlertInfo("SFU5062"); //불량코드가 존재하지 않습니다.
                resultChk = false;
            }

            return resultChk;
        }

        // Popup - Selected Item Process
        private void SelectedItemProcess()
        {
            try
            {
                C1.WPF.DataGrid.DataGridCell dataGridCell = (C1.WPF.DataGrid.DataGridCell)this.popupReasonCode.Tag;
                if (dataGridCell == null)
                {
                    return;
                }
                int changedRowIndex = dataGridCell.Row.Index;

                var query = DataTableConverter.Convert(this.dgReasonCode.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
                foreach (var item in query)
                {
                    var currentResnCode = DataTableConverter.GetValue(this.dgLOTList.Rows[changedRowIndex].DataItem, "RESNCODE");
                    var changedResnCode = item.Field<string>("RESNCODE");
                    if (currentResnCode.ToString().Equals(changedResnCode.ToString()))
                    {
                        continue;
                    }
                    DataTableConverter.SetValue(this.dgLOTList.Rows[changedRowIndex].DataItem, "RESNCODE", item.Field<string>("RESNCODE"));
                    DataTableConverter.SetValue(this.dgLOTList.Rows[changedRowIndex].DataItem, "FAILNAME", item.Field<string>("RESNNAME"));
                    DataTableConverter.SetValue(this.dgLOTList.Rows[changedRowIndex].DataItem, "CHK", "True");
                    DataTableConverter.SetValue(this.dgLOTList.Rows[changedRowIndex].DataItem, "CHK", true);
                    this.dgLOTList.Refresh(false, true, true, false, false);
                    //var isChecked = DataTableConverter.GetValue(this.dgLOTList.Rows[dataGridCell.Row.Index].DataItem, "CHK");
                    //if (!Convert.ToBoolean(isChecked))
                    //{
                    //    DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "CHK", "True");
                    //}
                }
                this.popupReasonCode.Tag = null;
                this.HidePopupReasonCode();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - Committed Event가 발생되었을 때
        private void CommittedEditProcess(C1DataGrid c1DataGrid, DataGridCellEventArgs e)
        {
            // Popup의 Grid는 단일 체크
            DataRowView dataRowView = (DataRowView)e.Cell.Row.DataItem;
            var checkedColumn = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));

            // 체크값이 True인 경우 다른 Row의 있는 내용은 False
            if (dataRowView.Row["CHK"].SafeToBoolean())
            {
                for (int i = 0; i < c1DataGrid.GetRowCount(); i++)
                {
                    if (!i.Equals(e.Cell.Row.Index))
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string eqsgID = e.NewValue.ToString();
            DataTable dtProductModel = this.dataHelper.GetProjectModelInfo(LoginInfo.CFG_AREA_ID, eqsgID);
            PackCommon.SetC1ComboBox(dtProductModel, this.cboProductModel, true, "ALL");

            DataTable dtProcessPack = this.dataHelper.GetProcessPackInfo(Area_Type.PACK, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, eqsgID);
            PackCommon.SetMultiSelectionComboBox(dtProcessPack, this.cboMultiCauseProcessID, ref causeProcessIDMultiComboBindDataCount);
        }

        private void cboProductModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string eqsgID = (this.cboEquipmentSegment.SelectedValue == null) ? string.Empty : this.cboEquipmentSegment.SelectedValue.ToString();
            string productClassCode = (this.cboProductClassCode.SelectedValue == null) ? string.Empty : this.cboProductClassCode.SelectedValue.ToString();
            string projectModel = e.NewValue.ToString();
            DataTable dtProduct = this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, eqsgID, projectModel, productClassCode);
            PackCommon.SetC1ComboBox(dtProduct, this.cboProductID, true, "ALL");
        }

        private void cboProductClassCode_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string eqsgID = (this.cboEquipmentSegment.SelectedValue == null) ? string.Empty : this.cboEquipmentSegment.SelectedValue.ToString();
            string productClassCode = e.NewValue.ToString();
            string projectModel = (this.cboProductModel.SelectedValue == null) ? string.Empty : this.cboProductModel.SelectedValue.ToString();
            DataTable dtProduct = this.dataHelper.GetProductByProjectModelOrProjectClassCodeInfo(LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, eqsgID, projectModel, productClassCode);
            PackCommon.SetC1ComboBox(dtProduct, this.cboProductID, true, "ALL");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(this.dgLOTList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.SaveProcess())
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1270"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        this.SearchProcess();
                        return;
                    }
                });
            }
        }

        private void dgLOTList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgLOTList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
            if (dataGridCell == null)
            {
                return;
            }

            // 클릭질한 Column명에 따른 분기
            switch (dataGridCell.Column.Name.ToUpper())
            {
                case "LOTID":
                    this.FrameOperation.OpenMenu("SFU010090090", true, dataGridCell.Text);
                    this.HidePopupReasonCode();
                    break;

                case "RESNCODE":
                case "FAILNAME":
                    this.ShowPopupReasonCode(c1DataGrid, e);
                    break;
                default:
                    this.HidePopupReasonCode();
                    break;
            }
        }

        private void c1DataGrid_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.CommittedEditProcess((C1DataGrid)sender, e);
        }

        private void popupReasonCode_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupReasonCode.StaysOpen = true;
        }

        private void popupErrorCode_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupErrorCode.StaysOpen = true;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopupReasonCode();
        }

        private void imgCloseErrorCode_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.HidePopupErrorCode();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItemProcess();
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopupReasonCode();
        }

        private void btnHideErrorCode_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopupErrorCode();
        }

        private void dgReasonCode0_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgReasonCode0.SelectedItem as DataRowView;

            string[] prefix = new string[1];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];

            Util.gridClear(dgReasonCode);
            Util.gridClear(dgReasonCode1);
            Util.gridClear(dgReasonCode2);
            Util.gridClear(dgReasonCode3);
            Util.gridClear(dgReasonCode4);

            Util.GridSetData(dgReasonCode1, getNextRESNCODE(prefix), FrameOperation, true);
            dgReasonCode1.AllColumnsWidthAuto();
        }

        private void dgReasonCode1_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgReasonCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgReasonCode1.SelectedItem as DataRowView;

            String[] prefix = new string[2];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];

            Util.gridClear(dgReasonCode);
            Util.gridClear(dgReasonCode2);
            Util.gridClear(dgReasonCode3);
            Util.gridClear(dgReasonCode4);

            Util.GridSetData(dgReasonCode2, getNextRESNCODE(prefix), FrameOperation, true);
            dgReasonCode2.AllColumnsWidthAuto();
        }

        private void dgReasonCode2_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgReasonCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgReasonCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgReasonCode2.SelectedItem as DataRowView;

            String[] prefix = new string[3];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];

            Util.gridClear(dgReasonCode);
            Util.gridClear(dgReasonCode3);
            Util.gridClear(dgReasonCode4);

            Util.GridSetData(dgReasonCode3, getNextRESNCODE(prefix), FrameOperation, true);
            dgReasonCode3.AllColumnsWidthAuto();
        }

        private void dgReasonCode3_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgReasonCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgReasonCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgReasonCode2.SelectedItem as DataRowView;
            DataRowView r3 = dgReasonCode3.SelectedItem as DataRowView;

            String[] prefix = new string[4];
            prefix[0] = r0.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[1] = r1.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[2] = r2.Row.ItemArray[0].ToString().Split(':')[0];
            prefix[3] = r3.Row.ItemArray[0].ToString().Split(':')[0];

            Util.gridClear(dgReasonCode);
            Util.gridClear(dgReasonCode4);

            Util.GridSetData(dgReasonCode4, getNextRESNCODE(prefix), FrameOperation, true);
            dgReasonCode4.AllColumnsWidthAuto();
        }

        private void dgReasonCode4_SelectionChanged(object sender, DataGridSelectionChangedEventArgs args)
        {
            if (args.AddedRanges.Count < 1) return;

            DataRowView r0 = dgReasonCode0.SelectedItem as DataRowView;
            DataRowView r1 = dgReasonCode1.SelectedItem as DataRowView;
            DataRowView r2 = dgReasonCode2.SelectedItem as DataRowView;
            DataRowView r3 = dgReasonCode3.SelectedItem as DataRowView;
            DataRowView r4 = dgReasonCode4.SelectedItem as DataRowView;

            String code = r0.Row.ItemArray[0].ToString().Split(':')[0] + r1.Row.ItemArray[0].ToString().Split(':')[0] + r2.Row.ItemArray[0].ToString().Split(':')[0] + r3.Row.ItemArray[0].ToString().Split(':')[0] + r4.Row.ItemArray[0].ToString().Split(':')[0];

            var query = dtCodeResult.AsEnumerable()
                .Where(x => x.Field<string>("RESNCODE") == code)
                .Select(x =>
                    new {
                        CHK = true,
                        RESNCODE = x.Field<string>("RESNCODE"),
                        RESNNAME = x.Field<string>("RESNNAME")
                    });

            Util.GridSetData(dgReasonCode, PackCommon.queryToDataTable(query.ToList()), FrameOperation);
        }

        private void setDefectResult()
        {
            try
            {
                if (dtCodeResult == null || dtCodeResult.Rows.Count < 1)
                {
                    return;
                }

                DataTable INDATA = dtCodeResult.Copy().DefaultView.ToTable(false, new string[] { "RESNCODE" });
                INDATA.TableName = "INDATA";


                DataColumn col = new DataColumn();

                col.DataType = typeof(string);
                col.ColumnName = "LANGID";
                col.DefaultValue = LoginInfo.LANGID;

                INDATA.Columns.Add(col);

                defectResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DFCT_INFO", "RQSTDT", "RSLTDT", INDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 불량유형코드선택창 각 유형 grid에 들어갈 데이터 선별 함수
        /// <para>
        /// dsResult.Tables["ACTIVITYREASON"], defectResult 조회가 선행되어야 함
        /// </para>
        /// <param name="pre">이전까지 선택된 모든 code string[].</param>
        /// </summary>
        private DataTable getNextRESNCODE(string[] pre)
        {
            if (pre == null) pre = new string[0];

            if (defectResult != null)
            {
                if (defectResult.Rows.Count > 0)
                {
                    DataTable dt = defectResult.Copy();
                    SortedSet<String> set = new SortedSet<String>();

                    int k = -1;
                    for (int i = 0; i < pre.Length; i++)
                    {
                        DataView dv = dt.DefaultView;
                        dv.RowFilter = dt.Columns[i * 2].ColumnName + " = '" + pre[i] + "' ";
                        dt = dv.ToTable();
                        k = i;
                    }

                    k = (k + 1) * 2;
                    if (dt.Columns.Count <= k + 1)
                    {
                        return null;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        set.Add(Convert.ToString(dr[k]) + ":" + Convert.ToString(dr[k + 1]));
                    }

                    DataTable resdt = new DataTable();
                    resdt.Columns.Add("RESNCODE", typeof(string));

                    foreach (String val in set)
                    {
                        resdt.Rows.Add(val);
                    }

                    return resdt;
                }
            }

            return null;
        }
        #endregion
    }

    internal class PACK001_013_DataHelper
    {
        #region Member Variable Lists...
        #endregion

        #region Constructor
        internal PACK001_013_DataHelper()
        {

        }
        #endregion

        #region Member Function Lists...
        // 순서도 호출 - CommonCode 정보
        internal DataTable GetCommonCodeInfo(string cmcdType)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTES";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["ATTRIBUTE1"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE2"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE3"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE4"] = DBNull.Value;
                drRQSTDT["ATTRIBUTE5"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - EquipmentSegment 정보
        internal DataTable GetEquipmentSegmentInfo(string areaID)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EXCEPT_GROUP", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EXCEPT_GROUP"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtReturn = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Project Model 정보
        internal DataTable GetProjectModelInfo(string areaID, string eqsgID)
        {
            string bizRuleName = "DA_BAS_SEL_PRJMODEL_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Product Info currespond with Project Model or Project Class Code
        internal DataTable GetProductByProjectModelOrProjectClassCodeInfo(string shopID, string areaID, string eqsgID, string projectModel, string productClassCode)
        {
            string bizRuleName = "DA_BAS_SEL_PRJPRODUCT_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROJECT_MODEL", typeof(string));
                dtRQSTDT.Columns.Add("PRDCLASS", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["SHOPID"] = shopID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                drRQSTDT["PROJECT_MODEL"] = (string.IsNullOrEmpty(projectModel) || projectModel.Equals("ALL")) ? null : projectModel;
                drRQSTDT["PRDCLASS"] = (string.IsNullOrEmpty(productClassCode) || productClassCode.Equals("ALL")) ? null : productClassCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - Pack 공정 정보 (원인공정)
        internal DataTable GetProcessPackInfo(string areaTypeCode, string shopID, string areaID, string eqsgID)
        {
            string bizRuleName = "DA_BAS_SEL_PROCESS_PACK_CBO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PCSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREA_TYPE_CODE"] = areaTypeCode;
                drRQSTDT["SHOPID"] = shopID;
                drRQSTDT["AREAID"] = areaID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                drRQSTDT["PCSGID"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 불량발생정보 조회
        internal DataTable GetRepairDefectLOT(params object[] obj)
        {
            string bizRuleName = "DA_PRD_SEL_REPAIR_DEFECT_SEARCH_INFO";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("MODLID", typeof(string));
                dtRQSTDT.Columns.Add("PRODCLASS", typeof(string));
                dtRQSTDT.Columns.Add("PRODID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("PROCTYPE", typeof(string));
                dtRQSTDT.Columns.Add("PROCID_CAUSE", typeof(string));
                dtRQSTDT.Columns.Add("SHIFT", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COUNT", typeof(Int64));
                dtRQSTDT.Columns.Add("FROMDATE", typeof(string));
                dtRQSTDT.Columns.Add("TODATE", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("WIPSEQ", typeof(string));

                //obj[0] = DateTime dteFromDate
                //obj[1] = DateTime dteToDate
                //obj[2] = string eqsgID
                //obj[3] = string productModel
                //obj[4] = string productClassCode
                //obj[5] = string productID
                //obj[6] = string causeProcID
                //obj[7] = int selectCount
                //obj[8] = string LOTID
                //obj[9] = decimal wipseq
                DateTime dteFromDate = obj[0] == null ? new DateTime(1900, 1, 1) : Convert.ToDateTime(obj[0]);
                DateTime dteToDate = obj[1] == null ? new DateTime(2199, 1, 1) : Convert.ToDateTime(obj[1]);
                string eqsgID = obj[2] == null ? string.Empty : Convert.ToString(obj[2]);
                string productModel = obj[3] == null ? string.Empty : Convert.ToString(obj[3]);
                string productClassCode = obj[4] == null ? string.Empty : Convert.ToString(obj[4]);
                string productID = obj[5] == null ? string.Empty : Convert.ToString(obj[5]);
                string causeProcID = obj[6] == null ? string.Empty : Convert.ToString(obj[6]);
                int selectCount = obj[7] == null ? 50000 : Convert.ToInt32(obj[7]);
                string LOTID = obj[8] == null ? string.Empty : Convert.ToString(obj[8]);
                decimal? wipseq = obj[9] == null ? 0 : Convert.ToDecimal(obj[9]);

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                drRQSTDT["MODLID"] = (string.IsNullOrEmpty(productModel) || productModel.Equals("ALL")) ? null : productModel;
                drRQSTDT["PRODCLASS"] = (string.IsNullOrEmpty(productClassCode) || productClassCode.Equals("ALL")) ? null : productClassCode;
                drRQSTDT["PRODID"] = (string.IsNullOrEmpty(productID) || productID.Equals("ALL")) ? null : productID;
                drRQSTDT["PROCID"] = null;
                drRQSTDT["ACTID"] = "DEFECT_LOT";
                drRQSTDT["PROCTYPE"] = null;
                drRQSTDT["PROCID_CAUSE"] = (string.IsNullOrEmpty(causeProcID) || causeProcID.Equals("ALL")) ? null : causeProcID;
                drRQSTDT["SHIFT"] = null;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["COUNT"] = selectCount;
                drRQSTDT["FROMDATE"] = (dteFromDate == null) ? null : dteFromDate.ToString("yyyyMMdd");
                drRQSTDT["TODATE"] = (dteToDate == null) ? null : dteToDate.ToString("yyyyMMdd");
                drRQSTDT["LOTID"] = (string.IsNullOrEmpty(LOTID) || LOTID.Equals("ALL")) ? null : LOTID;
                if (wipseq <= 0)
                {
                    drRQSTDT["WIPSEQ"] = null;
                }
                else
                {
                    drRQSTDT["WIPSEQ"] = wipseq;
                }
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 라인, 공정별 불량코드정보 조회
        internal DataTable GetDefectActivityReasonCode(string actID, string eqsgID, string procID, string resnCode)
        {
            string bizRuleName = "DA_PRD_SEL_ACTIVITYREASON_BY_EQSGID_PROCID";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID", typeof(string));
                dtRQSTDT.Columns.Add("RESNCODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["ACTID"] = actID;
                drRQSTDT["EQSGID"] = (string.IsNullOrEmpty(eqsgID) || eqsgID.Equals("ALL")) ? null : eqsgID;
                drRQSTDT["PROCID"] = (string.IsNullOrEmpty(procID) || procID.Equals("ALL")) ? null : procID;
                drRQSTDT["RESNCODE"] = resnCode;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 순서도 호출 - 불량코드변경 데이터 저장
        internal DataTable SetLOTDefectCode(IEnumerable<dynamic> query)
        {
            string bizRuleName = "BR_PRD_REG_TB_SFC_WIPREASONCOLLECT_HIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            DataTable dt = PackCommon.queryToDataTable(query.ToList());
            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("LOTID", typeof(string));
                dtRQSTDT.Columns.Add("WIPSEQ", typeof(string));
                dtRQSTDT.Columns.Add("RESNCODE", typeof(string));
                dtRQSTDT.Columns.Add("RESNCODE_OLD", typeof(string));
                dtRQSTDT.Columns.Add("ACTID", typeof(string));
                dtRQSTDT.Columns.Add("ACTDTTM", typeof(string));
                dtRQSTDT.Columns.Add("USERID", typeof(string));
                dtRQSTDT.Columns.Add("PROCID_CAUSE", typeof(string));
                dtRQSTDT.Columns.Add("RESNNOTE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["LOTID"] = dt.AsEnumerable().Select(x => x.Field<string>("LOTID")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["WIPSEQ"] = dt.AsEnumerable().Select(x => Convert.ToString(x.Field<string>("WIPSEQ"))).Aggregate((current, next) => current + "," + next);
                drRQSTDT["RESNCODE"] = dt.AsEnumerable().Select(x => x.Field<string>("RESNCODE")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["RESNCODE_OLD"] = dt.AsEnumerable().Select(x => x.Field<string>("RESNCODE_OLD")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["ACTID"] = dt.AsEnumerable().Select(x => x.Field<string>("ACTID")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["ACTDTTM"] = dt.AsEnumerable().Select(x => x.Field<string>("ACTDTTM")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["USERID"] = LoginInfo.USERID;
                drRQSTDT["PROCID_CAUSE"] = dt.AsEnumerable().Select(x => x.Field<string>("PROCID_CAUSE")).Aggregate((current, next) => current + "," + next);
                drRQSTDT["RESNNOTE"] = "Changed Defect from UI";

                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtRSLTDT;
        }

        // 데이터 조회건수
        internal DataTable GetSearchRowCount()
        {
            List<Tuple<string, int>> list = new List<Tuple<string, int>>();

            for (int i = 1000; i > 0; i -= 100)
            {
                Tuple<string, int> tuple = new Tuple<string, int>(i.Equals(1000) ? "ALL" : i.ToString(), i.Equals(1000) ? 50000 : i);
                list.Add(tuple);
            }

            var query = list.Select(x => new
            {
                CODE = x.Item2,
                NAME = x.Item1
            }).OrderByDescending(x => x.NAME);

            return PackCommon.queryToDataTable(query.ToList());
        }
        #endregion
    }
}