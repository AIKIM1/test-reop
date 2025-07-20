/************************************************************************************
  Created Date : 2021.04.06
       Creator : 정용석
   Description : 포장기별 투입 및 혼입설정
 ------------------------------------------------------------------------------------
  [Change History]
    2021.04.06  정용석 : Initial Created
    2021.05.10  정용석 : 포장기 라인 설정 + 투입 및 혼입 설정 기능 추가
    2021.09.25  정용석 : 투입 및 혼입 설정에서 BOX_LOT_SET_DATE Column 추가
    2022.01.19  정용석 : 투입 및 혼입 설정에서 LOGIS_PACK_TYPE Column 추가
    2023.08.01  김진수 : MICA전용 공통코드 확인하여 기존 반송정보 무조건 종료 처리
 ************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_021 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private const string PACK_EQUIPMENT_SET = "PACK_EQUIPMENTSET";
        private const string TRANSFER_SET = "PACK_TRANSFER_SET";

        private PACK003_021_DataHelper dataHelper = new PACK003_021_DataHelper();
        private List<GridProperty> lstGridElement = new List<GridProperty>();
        private List<C1DataGrid> lstGrid = new List<C1DataGrid>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_021()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupBaseInfo, this.pnlTitleBaseInfo);
            PackCommon.SetPopupDraggable(this.popupDate, this.pnlTitleDate);
            PackCommon.SetPopupDraggable(this.popupTransferConfirm, this.pnlTitleTransferConfirm);
        }
        #endregion

        #region Member Function Lists...
        // Initialize
        private void Initialize()
        {
            this.SetTagControl();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(this.btnSave1);
            listAuth.Add(this.btnSave2);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            PackCommon.SearchRowCount(ref this.txtPackEquipmentProductBaseSet, 0);
            PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
            this.SetComboBox();
            this.txtRemark.Text = MessageDic.Instance.GetMessage("SFU8389");                // 선택 내용이 없으면 전체 선택(으)로 처리되며, 조립동, 전극설비, 조립설비는 투입혼입설정여부가 CHK_ALLOW로 설정됩니다.
            this.txtPopupRemark.Text = MessageDic.Instance.GetMessage("SFU8369");           // 선택 내용이 없으면 전체 선택으로 처리됩니다.
            this.txtMessageTransferCancel.Text = MessageDic.Instance.GetMessage("SFU8371"); // 아래와 같이 투입 및 혼입설정 정보가 변경되기 전에 해당 포장기로 반송 또는 반송요청 중인 항목이 존재합니다. 투입 및 혼입설정 정보 저장 및 아래의 반송내역을 종료 또는 취소하시겠습니까?

            this.dtpBoxLotSetDate.ApplyTemplate();
            this.dtpBoxLotSetDate.SelectedDateTime = DateTime.Now;

            this.CreateGridProperty();
            this.CreateGrid();
        }

        // Initialize - Control에 Tag값 설정
        private void SetTagControl()
        {
            this.dgGrid1.Tag = PACK_EQUIPMENT_SET;
            this.cboPackEqptID1.Tag = PACK_EQUIPMENT_SET;
            this.btnSearch1.Tag = PACK_EQUIPMENT_SET;
            this.txtPackEquipmentProductBaseSet.Tag = PACK_EQUIPMENT_SET;
            this.btnPlus1.Tag = PACK_EQUIPMENT_SET + "|" + "ADD";
            this.btnMinus1.Tag = PACK_EQUIPMENT_SET + "|" + "MINUS";
            this.btnSave1.Tag = PACK_EQUIPMENT_SET;
            //this.chkAll1.Tag = PACK_EQUIPMENT_SET;

            this.dgGrid2.Tag = TRANSFER_SET;
            this.txtGridRowCount2.Tag = TRANSFER_SET;
            this.numericCount2.Tag = TRANSFER_SET;
            this.btnPlus2.Tag = TRANSFER_SET + "|" + "ADD";
            this.btnMinus2.Tag = TRANSFER_SET + "|" + "MINUS";
            this.btnSave2.Tag = TRANSFER_SET;
            this.chkAll2.Tag = TRANSFER_SET;
        }

        // Popup - Grid 속성 설정
        private void CreateGridProperty()
        {
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_AREA", DATA_PROPERTIES = "SHOP", CODE_COLUMN = "SHOPID", CODENAME_COLUMN = "SHOPNAME", LINKED_VALUEPATH_COLUMN = "PROD_AREAID_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_AREAID_LIST_NAME", GRID_NAME = "dgAssyShop", IS_LASTGRID = false, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_AREA", DATA_PROPERTIES = "AREA", CODE_COLUMN = "AREAID", CODENAME_COLUMN = "AREANAME", LINKED_VALUEPATH_COLUMN = "PROD_AREAID_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_AREAID_LIST_NAME", GRID_NAME = "dgAssyArea", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 3, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ASSY_LINE", DATA_PROPERTIES = "SHOP", CODE_COLUMN = "SHOPID", CODENAME_COLUMN = "SHOPNAME", LINKED_VALUEPATH_COLUMN = "PROD_ASSY_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ASSY_LINE_LIST_NAME", GRID_NAME = "dgAssyLineShop", IS_LASTGRID = false, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ASSY_LINE", DATA_PROPERTIES = "AREA", CODE_COLUMN = "AREAID", CODENAME_COLUMN = "AREANAME", LINKED_VALUEPATH_COLUMN = "PROD_ASSY_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ASSY_LINE_LIST_NAME", GRID_NAME = "dgAssyLineArea", IS_LASTGRID = false, ROW_INDEX = 3, COLUMN_INDEX = 3, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ASSY_LINE", DATA_PROPERTIES = "EQUIPMENT", CODE_COLUMN = "EQPTID", CODENAME_COLUMN = "EQPTNAME", LINKED_VALUEPATH_COLUMN = "PROD_ASSY_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ASSY_LINE_LIST_NAME", GRID_NAME = "dgAssyLineEqpt", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 5, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ELTR_LINE", DATA_PROPERTIES = "SHOP", CODE_COLUMN = "SHOPID", CODENAME_COLUMN = "SHOPNAME", LINKED_VALUEPATH_COLUMN = "PROD_ELTR_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ELTR_LINE_LIST_NAME", GRID_NAME = "dgEltrLineShop", IS_LASTGRID = false, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ELTR_LINE", DATA_PROPERTIES = "AREA", CODE_COLUMN = "AREAID", CODENAME_COLUMN = "AREANAME", LINKED_VALUEPATH_COLUMN = "PROD_ELTR_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ELTR_LINE_LIST_NAME", GRID_NAME = "dgEltrLineArea", IS_LASTGRID = false, ROW_INDEX = 3, COLUMN_INDEX = 3, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_ELTR_LINE", DATA_PROPERTIES = "EQUIPMENT", CODE_COLUMN = "EQPTID", CODENAME_COLUMN = "EQPTNAME", LINKED_VALUEPATH_COLUMN = "PROD_ELTR_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_ELTR_LINE_LIST_NAME", GRID_NAME = "dgEltrLineEqpt", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 5, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PROD_PACK_LINE", DATA_PROPERTIES = "EQUIPMENTSEGMENT", CODE_COLUMN = "EQSGID", CODENAME_COLUMN = "EQSGNAME", LINKED_VALUEPATH_COLUMN = "PROD_PACK_LINE_LIST", LINKED_DISPLAYPATH_COLUMN = "PROD_PACK_LINE_LIST_NAME", GRID_NAME = "dgPackLineEqsg", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = true });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PRODUCT", DATA_PROPERTIES = "PRODUCT", CODE_COLUMN = "PRODID", CODENAME_COLUMN = "PRODNAME", LINKED_VALUEPATH_COLUMN = "PRODID", LINKED_DISPLAYPATH_COLUMN = "PRODNAME", GRID_NAME = "dgProduct", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PACK_EQUIPMENT", DATA_PROPERTIES = "PACK_EQUIPMENT", CODE_COLUMN = "PACK_EQPTID", CODENAME_COLUMN = "PACK_EQPTNAME", LINKED_VALUEPATH_COLUMN = "PACK_EQPTID", LINKED_DISPLAYPATH_COLUMN = "PACK_EQPTNAME", GRID_NAME = "dgPackEqpt", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "USE_FLAG", DATA_PROPERTIES = "USE_FLAG", CODE_COLUMN = "USE_FLAG", CODENAME_COLUMN = "USE_FLAG_NAME", LINKED_VALUEPATH_COLUMN = "USE_FLAG", LINKED_DISPLAYPATH_COLUMN = "USE_FLAG_NAME", GRID_NAME = "dgUseFlag", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "AUTO_TRF_FLAG", DATA_PROPERTIES = "AUTO_TRF_FLAG", CODE_COLUMN = "AUTO_TRF_FLAG", CODENAME_COLUMN = "AUTO_TRF_FLAG_NAME", LINKED_VALUEPATH_COLUMN = "AUTO_TRF_FLAG", LINKED_DISPLAYPATH_COLUMN = "AUTO_TRF_FLAG_NAME", GRID_NAME = "dgAutoTrfFlag", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "INPUT_MIX_CHK_MTHD_CODE", DATA_PROPERTIES = "INPUT_MIX_CHK_MTHD_CODE", CODE_COLUMN = "INPUT_MIX_CHK_MTHD_CODE", CODENAME_COLUMN = "INPUT_MIX_CHK_MTHD_CODE_NAME", LINKED_VALUEPATH_COLUMN = "INPUT_MIX_CHK_MTHD_CODE", LINKED_DISPLAYPATH_COLUMN = "INPUT_MIX_CHK_MTHD_CODE_NAME", GRID_NAME = "dgInputMixCheckMethodCode", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PACK_MIX_TYPE_CODE", DATA_PROPERTIES = "PACK_MIX_TYPE_CODE", CODE_COLUMN = "PACK_MIX_TYPE_CODE", CODENAME_COLUMN = "PACK_MIX_TYPE_CODE_NAME", LINKED_VALUEPATH_COLUMN = "PACK_MIX_TYPE_CODE", LINKED_DISPLAYPATH_COLUMN = "PACK_MIX_TYPE_CODE_NAME", GRID_NAME = "dgPackMixTypeCode", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "LOGIS_PACK_TYPE", DATA_PROPERTIES = "LOGIS_PACK_TYPE", CODE_COLUMN = "LOGIS_PACK_TYPE", CODENAME_COLUMN = "LOGIS_PACK_TYPE_NAME", LINKED_VALUEPATH_COLUMN = "LOGIS_PACK_TYPE", LINKED_DISPLAYPATH_COLUMN = "LOGIS_PACK_TYPE_NAME", GRID_NAME = "dgLogisPackType", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false });
        }

        // Popup - Grid 만들기
        private void CreateGrid()
        {
            foreach (GridProperty gridProperty in this.lstGridElement)
            {
                C1DataGrid c1DataGrid = new C1DataGrid();
                c1DataGrid.Name = gridProperty.GRID_NAME;
                c1DataGrid.Width = 350;
                c1DataGrid.SelectionMode = C1.WPF.DataGrid.DataGridSelectionMode.SingleRow;
                c1DataGrid.CommittedEdit += c1DataGrid_CommittedEdit;

                // Column 추가
                PackCommon.AddGridColumn(c1DataGrid, "CHECKBOX", "CHK", true);
                PackCommon.AddGridColumn(c1DataGrid, "TEXT", gridProperty.CODE_COLUMN, false);
                PackCommon.AddGridColumn(c1DataGrid, "TEXT", gridProperty.CODENAME_COLUMN, true);
                this.lstGrid.Add(c1DataGrid);
            }
        }

        // Popup - Grid 조회 (DataGrid Control과 사전에 정의한 DataGrid 속성값들 리턴)
        private IEnumerable<dynamic> GetGridInfo()
        {
            return from GRID_PROPERTY in this.lstGridElement.OfType<GridProperty>()
                   join GRID in this.lstGrid.OfType<C1DataGrid>() on GRID_PROPERTY.GRID_NAME equals GRID.Name
                   select new
                   {
                       CATEGORY = GRID_PROPERTY.CATEGORY,
                       DATA_PROPERTIES = GRID_PROPERTY.DATA_PROPERTIES,
                       CODE_COLUMN = GRID_PROPERTY.CODE_COLUMN,
                       CODENAME_COLUMN = GRID_PROPERTY.CODENAME_COLUMN,
                       DISPLAYPATH_COLUMN = GRID_PROPERTY.LINKED_DISPLAYPATH_COLUMN,
                       LINKED_VALUEPATH_COLUMN = GRID_PROPERTY.LINKED_VALUEPATH_COLUMN,
                       LINKED_DISPLAYPATH_COLUMN = GRID_PROPERTY.LINKED_DISPLAYPATH_COLUMN,
                       GRID_NAME = GRID_PROPERTY.GRID_NAME,
                       IS_LASTGRID = GRID_PROPERTY.IS_LASTGRID,
                       ROW_INDEX = GRID_PROPERTY.ROW_INDEX,
                       COLUMN_INDEX = GRID_PROPERTY.COLUMN_INDEX,
                       MULTI_CHECK = GRID_PROPERTY.MULTI_CHECK,
                       GRID = GRID
                   };
        }

        // Popup - Popup 표출
        private void ShowPopUp(C1DataGrid c1DataGrid, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                // Declareations...
                C1.WPF.DataGrid.DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(e.GetPosition(null));
                if (dataGridCell == null)
                {
                    return;
                }

                // 클릭질한 Row의 선택표시가 안되어 있으면 Return
                DataRowView dataRowView = (DataRowView)c1DataGrid.Rows[dataGridCell.Row.Index].DataItem;
                if (!Convert.ToBoolean(dataRowView["CHK"]))
                {
                    return;
                }

                // Box Lot Set Date 컬럼인 경우 별도 분리
                if (c1DataGrid.Name.ToUpper().Equals("DGGRID2") && dataGridCell.Column.Name.Equals("BOX_LOT_SET_DATE"))
                {
                    this.SetPopupDateControlTag(dataGridCell);

                    // 열려진 Popup 닫고 Popup 제목 땡기기
                    this.HidePopUp();
                    this.txtPopupTitleDate.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("BOX_LOT_SET_DATE");

                    string selectedDate = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "BOX_LOT_SET_DATE")?.ToString();

                    if (!string.IsNullOrEmpty(selectedDate) && !selectedDate.ToUpper().Equals("ALL"))
                    {
                        this.dtpBoxLotSetDate.SelectedDateTime = DateTime.ParseExact(selectedDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                    }
                    else
                    {
                        this.dtpBoxLotSetDate.SelectedDateTime = DateTime.Now;
                    }

                    // Popup 표출 위치 (정가운데)
                    this.popupDate.Placement = PlacementMode.Center;
                    this.popupDate.IsOpen = true;
                }

                // 클릭질한 Row가 조회버튼 눌러서 가져온 DataRow라면 Key Column인 경우는 Popup 띄우지 못하게 하기
                if ((dataRowView.Row.RowState.Equals(DataRowState.Unchanged) || dataRowView.Row.RowState.Equals(DataRowState.Modified)) &&
                    c1DataGrid.Name.ToUpper().Equals("DGGRID1") &&
                    dataGridCell.Column.Name.Equals("PACK_EQPTNAME")
                    )
                {
                    return;
                }
                if ((dataRowView.Row.RowState.Equals(DataRowState.Unchanged) || dataRowView.Row.RowState.Equals(DataRowState.Modified)) &&
                    c1DataGrid.Name.ToUpper().Equals("DGGRID2") &&
                    (dataGridCell.Column.Name.Equals("PACK_EQPTNAME") || dataGridCell.Column.Name.Equals("PRODNAME"))
                    )
                {
                    return;
                }

                // 투입 혼입 설정 Grid 에서 클릭질한 Row가 신규 Row이고 Column이 포장기인 경우 Popup 띄우지 못하게 하기
                if (dataRowView.Row.RowState.Equals(DataRowState.Added) &&
                    c1DataGrid.Name.ToUpper().Equals("DGGRID2") &&
                    (dataGridCell.Column.Name.Equals("PACK_EQPTNAME"))
                    )
                {
                    return;
                }

                // 클릭질한 Column명 Validation
                string displayPathColumn = c1DataGrid.Columns[dataGridCell.Column.Index].Name;
                string valuePathColumn = this.GetGridInfo().Where(x => x.LINKED_DISPLAYPATH_COLUMN.Equals(displayPathColumn)).Select(x => x.LINKED_VALUEPATH_COLUMN).FirstOrDefault();
                if (string.IsNullOrEmpty(valuePathColumn))
                {
                    return;
                }

                List<string> selectedCodeValue = DataTableConverter.GetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, valuePathColumn)?.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                this.SetPopupControlTag(dataGridCell);

                // 열려진 Popup 닫고 Popup 제목 땡기기
                this.HidePopUp();
                switch (displayPathColumn)
                {
                    case "PROD_AREAID_LIST_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("AREA_ASSY");
                        break;
                    case "PROD_ELTR_LINE_LIST_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("전극설비");
                        break;
                    case "PROD_ASSY_LINE_LIST_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("조립설비");
                        break;
                    case "PROD_PACK_LINE_LIST_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("PACK_LINE");
                        break;
                    case "PRODNAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("PRODID");
                        break;
                    case "PACK_EQPTNAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("포장기");
                        break;
                    case "USE_FLAG_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("USE_FLAG");
                        break;
                    case "AUTO_TRF_FLAG_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("자동반송여부");
                        break;
                    case "INPUT_MIX_CHK_MTHD_CODE_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("투입혼입설정");
                        break;
                    case "PACK_MIX_TYPE_CODE_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("포장혼입유형");
                        break;
                    case "LOGIS_PACK_TYPE_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("선택") + " - " + ObjectDic.Instance.GetObjectName("LOGIS_PACK_TYPE_NAME");
                        break;
                    default:
                        this.txtPopupTitle.Text = string.Empty;
                        break;
                }

                // 기존 Grid 삭제
                List<C1DataGrid> lstCurrentGrid = this.grdPopup.Children.OfType<C1DataGrid>().ToList();
                for (int i = lstCurrentGrid.Count() - 1; i >= 0; i--)
                {
                    Util.gridClear(lstCurrentGrid[i]);
                    this.grdPopup.Children.Remove(lstCurrentGrid[i]);
                }

                // 선택한 컬럼에 해당하는 Grid 추가
                var lstAddGrid = this.GetGridInfo().Where(x => x.LINKED_DISPLAYPATH_COLUMN.Equals(displayPathColumn.ToUpper()));
                foreach (var item in lstAddGrid)
                {
                    if (lstAddGrid.Count() == 1)
                    {
                        item.GRID.Width = 400;
                    }
                    Grid.SetRow(item.GRID, item.ROW_INDEX);
                    Grid.SetColumn(item.GRID, item.COLUMN_INDEX);
                    this.grdPopup.Children.Add(item.GRID);
                }

                // 추가한 Grid 갯수만큼 Popup 폭조절
                switch (lstAddGrid.Count())
                {
                    case 1:
                        this.defColumnWhiteSpaceLeft.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridShop.Width = new GridLength(400, GridUnitType.Pixel);
                        this.defColumnWhiteSpace1.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnGridArea.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnWhiteSpace2.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnGridEquipment.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnWhiteSpaceRight.Width = new GridLength(8, GridUnitType.Pixel);
                        break;
                    case 2:
                        this.defColumnWhiteSpaceLeft.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridShop.Width = new GridLength(350, GridUnitType.Pixel);
                        this.defColumnWhiteSpace1.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridArea.Width = new GridLength(350, GridUnitType.Pixel);
                        this.defColumnWhiteSpace2.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnGridEquipment.Width = new GridLength(0, GridUnitType.Pixel);
                        this.defColumnWhiteSpaceRight.Width = new GridLength(8, GridUnitType.Pixel);
                        break;
                    default:
                        this.defColumnWhiteSpaceLeft.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridShop.Width = new GridLength(350, GridUnitType.Pixel);
                        this.defColumnWhiteSpace1.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridArea.Width = new GridLength(350, GridUnitType.Pixel);
                        this.defColumnWhiteSpace2.Width = new GridLength(8, GridUnitType.Pixel);
                        this.defColumnGridEquipment.Width = new GridLength(350, GridUnitType.Pixel);
                        this.defColumnWhiteSpaceRight.Width = new GridLength(8, GridUnitType.Pixel);
                        break;
                }

                // 제목 영역, 버튼 영역 Column Span
                Grid.SetColumnSpan(this.grdTitle, lstAddGrid.Count() * 2 - 1);
                Grid.SetColumnSpan(this.grdBottom, lstAddGrid.Count() * 2 - 1);

                // Grid DataBinding
                this.DataBindingProcess(lstAddGrid, selectedCodeValue);

                // Popup 표출 위치 (정가운데)
                this.popupBaseInfo.Placement = PlacementMode.Center;
                this.popupBaseInfo.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - 반송 완료 여부 Confirm Popup 표출
        private void ShowPopupTransferConfirm(List<object> lstParam)
        {
            try
            {
                this.popupTransferConfirm.Tag = null;
                this.popupTransferConfirm.Tag = lstParam;
                DataTable dtChangedTrfMixSet = (DataTable)lstParam[1];
                DataTable dtTransferRequest = (DataTable)lstParam[2];
                DataTable dtEIOOperMode = (DataTable)lstParam[3];

                // Data Binding
                if (!CommonVerify.HasTableRow(dtTransferRequest))
                {
                    return;
                }
                Util.GridSetData(this.dgTransferRequestHistory, dtTransferRequest, FrameOperation);

                // Popup 표출 위치 (정가운데)
                this.popupTransferConfirm.Placement = PlacementMode.Center;
                this.popupTransferConfirm.IsOpen = true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupBaseInfo.IsOpen = false;
            this.popupBaseInfo.HorizontalOffset = 0;
            this.popupBaseInfo.VerticalOffset = 0;

            this.popupDate.IsOpen = false;
            this.popupDate.HorizontalOffset = 0;
            this.popupDate.VerticalOffset = 0;

            this.popupTransferConfirm.IsOpen = false;
            this.popupTransferConfirm.HorizontalOffset = 0;
            this.popupTransferConfirm.VerticalOffset = 0;
        }

        // Popup - Popup에 표출된 Grid에서 선택표시를 하느냐 마느냐에 따른 Data Binding 처리
        private void GridDataBinding(C1DataGrid c1DataGrid, DataGridCellEventArgs e)
        {
            try
            {
                var currentGridCategory = this.GetGridInfo().Where(x => x.GRID_NAME.Equals(c1DataGrid.Name)).FirstOrDefault();
                if (currentGridCategory == null)
                {
                    return;
                }

                // 다음 Grid 찾기
                var nextGridInfo = this.GetGridInfo().Where(x => x.CATEGORY.Equals(currentGridCategory.CATEGORY)).SkipWhile(x => !x.GRID_NAME.Equals(c1DataGrid.Name)).Skip(1).FirstOrDefault();
                if (nextGridInfo == null)
                {
                    // 다음 Grid가 없으면 Popup에는 한개의 Grid만 표출됨
                    // Multi Check여부가 True인 경우에는 Skip, 단일 Check 여부인 경우에는 CheckBox Check표시가 하나만 보이게끔 처리
                    if (!Convert.ToBoolean(currentGridCategory.MULTI_CHECK))
                    {
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
                    return;
                }

                // 현재 Grid에 선택된 항목이 없으면 다음 Grid 데이터 Clear 시켜버리고 종료
                var query = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
                if (query.Count() <= 0)
                {
                    Util.gridClear(nextGridInfo.GRID);
                    return;
                }

                // Grid Binding Data 가져오기
                DataTable dtNewData = new DataTable();
                if (nextGridInfo.DATA_PROPERTIES.Equals("AREA") && nextGridInfo.CATEGORY.Equals("PROD_AREA")) dtNewData = this.dataHelper.GetAssyAreaInfo(query);
                if (nextGridInfo.DATA_PROPERTIES.Equals("AREA") && nextGridInfo.CATEGORY.Equals("PROD_ASSY_LINE")) dtNewData = this.dataHelper.GetAssyAreaInfo(query);
                if (nextGridInfo.DATA_PROPERTIES.Equals("AREA") && nextGridInfo.CATEGORY.Equals("PROD_ELTR_LINE")) dtNewData = this.dataHelper.GetElecAreaInfo(query);
                if (nextGridInfo.DATA_PROPERTIES.Equals("EQUIPMENT") && nextGridInfo.CATEGORY.Equals("PROD_ELTR_LINE")) dtNewData = this.dataHelper.GetElecEquipmentInfo(query);
                if (nextGridInfo.DATA_PROPERTIES.Equals("EQUIPMENT") && nextGridInfo.CATEGORY.Equals("PROD_ASSY_LINE")) dtNewData = this.dataHelper.GetAssyEquipmentInfo(query);

                // 다음 Grid에 Binding된 데이터가 없으면 신규 Data Binding 하고 종료
                if (nextGridInfo.GRID.ItemsSource == null)
                {
                    Util.GridSetData(nextGridInfo.GRID, dtNewData, FrameOperation);
                    return;
                }

                // 다음 Grid에 Binding된 데이터가 있으면 신규데이터랑 Bindng 데이터랑 경우에 맞는 쪼인질해서 선택표시를 유지하게끔 처리
                DataTable dtCurrentData = DataTableConverter.Convert(nextGridInfo.GRID.ItemsSource);
                string codeColumnName = nextGridInfo.CODE_COLUMN;
                string codeNameColumnName = nextGridInfo.CODENAME_COLUMN;

                DataTable dt = new DataTable();
                if (dtCurrentData.Rows.Count < dtNewData.Rows.Count)
                {
                    var checkQuery = (from newData in dtNewData.AsEnumerable()
                                      join currentData in dtCurrentData.AsEnumerable() on newData.Field<string>(codeColumnName) equals currentData.Field<string>(codeColumnName) into outerJoin
                                      from d1 in outerJoin.DefaultIfEmpty()
                                      select new
                                      {
                                          NEWDATA = newData,
                                          CURRENTDATA = d1
                                      }).Select(x => new
                                      {
                                          CHK = x.CURRENTDATA == null ? x.NEWDATA.Field<bool>("CHK") : x.CURRENTDATA.Field<bool>("CHK"),
                                          CODE = x.NEWDATA.Field<string>(codeColumnName),
                                          CODENAME = x.NEWDATA.Field<string>(codeNameColumnName)
                                      });
                    dt = PackCommon.queryToDataTable(checkQuery.ToList());
                }
                else
                {
                    var checkQuery = from newData in dtNewData.AsEnumerable()
                                     join currentData in dtCurrentData.AsEnumerable() on newData.Field<string>(codeColumnName) equals currentData.Field<string>(codeColumnName)
                                     select new
                                     {
                                         CHK = currentData.Field<bool>("CHK"),
                                         CODE = currentData.Field<string>(codeColumnName),
                                         CODENAME = currentData.Field<string>(codeNameColumnName)
                                     };
                    dt = PackCommon.queryToDataTable(checkQuery.ToList());
                }
                dt.Columns["CODE"].ColumnName = codeColumnName;
                dt.Columns["CODENAME"].ColumnName = codeNameColumnName;
                dt.AcceptChanges();

                Util.GridSetData(nextGridInfo.GRID, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup - Popup Open시에 부모 Grid에 넘어온 값에 따른 Data Binding 처리
        private void DataBindingProcess(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue = null)
        {
            if (selectedCodeValue == null)
            {
                this.DataBindTopLevel(lstAddGrid);
            }
            else
            {
                this.DataBindAllLevel(lstAddGrid, selectedCodeValue);
            }
        }

        // Popup - 부모 Grid에서 넘어온 값이 없을때 Grid Data Binding - 가장 상단 레벨것만 Binding
        private void DataBindTopLevel(IEnumerable<dynamic> lstAddGrid)
        {
            DataTable dt = new DataTable();

            foreach (var item in lstAddGrid)
            {
                if (lstAddGrid.Count().Equals(1) || item.DATA_PROPERTIES.Contains("SHOP"))
                {
                    if (item.CATEGORY.Equals("PROD_AREA")) dt = this.dataHelper.GetAssyShopInfo();                          // 조립동 Shop Grid
                    if (item.CATEGORY.Equals("PROD_ELTR_LINE")) dt = this.dataHelper.GetElecShopInfo();                     // 전극설비 Shop Grid
                    if (item.CATEGORY.Equals("PROD_ASSY_LINE")) dt = this.dataHelper.GetAssyShopInfo();                     // 조립설비 Shop Grid
                    if (item.CATEGORY.Equals("PROD_PACK_LINE")) dt = this.dataHelper.GetPackLineInfo();                     // 팩동 EquipmentSegment Grid
                    if (item.CATEGORY.Equals("PRODUCT")) dt = this.dataHelper.GetProductInfo();                             // 제품 Grid
                    if (item.CATEGORY.Equals("PACK_EQUIPMENT")) dt = this.dataHelper.GetPackEquipmentInfo();                // 포장기 Grid
                    if (item.CATEGORY.Equals("USE_FLAG")) dt = this.dataHelper.GetUseFlagInfo();                            // 사용여부 Grid
                    if (item.CATEGORY.Equals("AUTO_TRF_FLAG")) dt = this.dataHelper.GetAutoTransferFlagInfo();              // 자동반송여부 Grid
                    if (item.CATEGORY.Equals("INPUT_MIX_CHK_MTHD_CODE")) dt = this.dataHelper.GetInputMixCheckMethodInfo(); // Check Allow, Check Forbidden Grid
                    if (item.CATEGORY.Equals("PACK_MIX_TYPE_CODE")) dt = this.dataHelper.GetPackMixTypeCodeInfo();          // 포장혼입유형코드
                    if (item.CATEGORY.Equals("LOGIS_PACK_TYPE")) dt = this.dataHelper.GetLogisPackType();                   // 물류포장유형코드
                    if (CommonVerify.HasTableRow(dt)) Util.GridSetData(item.GRID, dt, FrameOperation);
                }
            }
        }

        // Popup - 부모 Grid에서 넘어온 값이 있을때 Grid Data Binding - 전체 Grid Binding
        private void DataBindAllLevel(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue = null)
        {
            var gridCategory = lstAddGrid.Select(x => x.CATEGORY).FirstOrDefault();
            if (gridCategory == null)
            {
                return;
            }

            if (gridCategory.Equals("PROD_AREA")) this.DataBindAssyArea(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PROD_ELTR_LINE")) this.DataBindElecEquipment(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PROD_ASSY_LINE")) this.DataBindAssyEquipment(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PROD_PACK_LINE")) this.DataBindPackLineInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PRODUCT")) this.DataBindProductInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PACK_EQUIPMENT")) this.DataBindPackEquipmentInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("USE_FLAG")) this.DataBindUseFlagInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("AUTO_TRF_FLAG")) this.DataBindAutoTransferFlagInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("INPUT_MIX_CHK_MTHD_CODE")) this.DataBindInputMixCheckMethodCodeInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PACK_MIX_TYPE_CODE")) this.DataBindPackMixTypeCodeInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("LOGIS_PACK_TYPE")) this.DataBindLogisPackType(lstAddGrid, selectedCodeValue);
        }

        // Popup - Grid Data Binding 조립동
        private void DataBindAssyArea(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtShopInfo = new DataTable();
            DataTable dtAreaInfo = new DataTable();
            this.dataHelper.GetAssyAreaInfo(selectedCodeValue, ref dtShopInfo, ref dtAreaInfo);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("SHOP") && CommonVerify.HasTableRow(dtShopInfo))
                {
                    Util.GridSetData(item.GRID, dtShopInfo, FrameOperation);
                }
                if (item.DATA_PROPERTIES.Contains("AREA") && CommonVerify.HasTableRow(dtAreaInfo))
                {
                    Util.GridSetData(item.GRID, dtAreaInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 조립설비
        private void DataBindAssyEquipment(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtShopInfo = new DataTable();
            DataTable dtAreaInfo = new DataTable();
            DataTable dtEquipmentInfo = new DataTable();
            this.dataHelper.GetAssyEquipmentInfo(selectedCodeValue, ref dtShopInfo, ref dtAreaInfo, ref dtEquipmentInfo);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("SHOP") && CommonVerify.HasTableRow(dtShopInfo))
                {
                    Util.GridSetData(item.GRID, dtShopInfo, FrameOperation);
                }
                if (item.DATA_PROPERTIES.Contains("AREA") && CommonVerify.HasTableRow(dtAreaInfo))
                {
                    Util.GridSetData(item.GRID, dtAreaInfo, FrameOperation);
                }
                if (item.DATA_PROPERTIES.Contains("EQUIPMENT") && CommonVerify.HasTableRow(dtEquipmentInfo))
                {
                    Util.GridSetData(item.GRID, dtEquipmentInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 전극설비
        private void DataBindElecEquipment(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtShopInfo = new DataTable();
            DataTable dtAreaInfo = new DataTable();
            DataTable dtEquipmentInfo = new DataTable();
            this.dataHelper.GetElecEquipmentInfo(selectedCodeValue, ref dtShopInfo, ref dtAreaInfo, ref dtEquipmentInfo);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("SHOP") && CommonVerify.HasTableRow(dtShopInfo))
                {
                    Util.GridSetData(item.GRID, dtShopInfo, FrameOperation);
                }
                if (item.DATA_PROPERTIES.Contains("AREA") && CommonVerify.HasTableRow(dtAreaInfo))
                {
                    Util.GridSetData(item.GRID, dtAreaInfo, FrameOperation);
                }
                if (item.DATA_PROPERTIES.Contains("EQUIPMENT") && CommonVerify.HasTableRow(dtEquipmentInfo))
                {
                    Util.GridSetData(item.GRID, dtEquipmentInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 팩라인
        private void DataBindPackLineInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtPackLineInfo = this.dataHelper.GetPackLineInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("EQUIPMENTSEGMENT") && CommonVerify.HasTableRow(dtPackLineInfo))
                {
                    Util.GridSetData(item.GRID, dtPackLineInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 제품
        private void DataBindProductInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtProductInfo = this.dataHelper.GetProductInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("PRODUCT") && CommonVerify.HasTableRow(dtProductInfo))
                {
                    Util.GridSetData(item.GRID, dtProductInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 포장기
        private void DataBindPackEquipmentInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtPackEquipmentInfo = this.dataHelper.GetPackEquipmentInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("PACK_EQUIPMENT") && CommonVerify.HasTableRow(dtPackEquipmentInfo))
                {
                    Util.GridSetData(item.GRID, dtPackEquipmentInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 사용여부
        private void DataBindUseFlagInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtUseFlagInfo = this.dataHelper.GetUseFlagInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("USE_FLAG") && CommonVerify.HasTableRow(dtUseFlagInfo))
                {
                    Util.GridSetData(item.GRID, dtUseFlagInfo, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 포장기 자동반송여부
        private void DataBindAutoTransferFlagInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtAutoTransferFlag = this.dataHelper.GetAutoTransferFlagInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("AUTO_TRF_FLAG") && CommonVerify.HasTableRow(dtAutoTransferFlag))
                {
                    Util.GridSetData(item.GRID, dtAutoTransferFlag, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding Input Mix Check Method Code
        private void DataBindInputMixCheckMethodCodeInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtInputMixCheckMethodCode = this.dataHelper.GetInputMixCheckMethodInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("INPUT_MIX_CHK_MTHD_CODE") && CommonVerify.HasTableRow(dtInputMixCheckMethodCode))
                {
                    Util.GridSetData(item.GRID, dtInputMixCheckMethodCode, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 포장혼입유형
        private void DataBindPackMixTypeCodeInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtInputMixCheckMethodCode = this.dataHelper.GetPackMixTypeCodeInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("PACK_MIX_TYPE_CODE") && CommonVerify.HasTableRow(dtInputMixCheckMethodCode))
                {
                    Util.GridSetData(item.GRID, dtInputMixCheckMethodCode, FrameOperation);
                }
            }
        }

        // Popup - Grid Data Binding 물류포장유형
        private void DataBindLogisPackType(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtLogisPackType = this.dataHelper.GetLogisPackType(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("LOGIS_PACK_TYPE") && CommonVerify.HasTableRow(dtLogisPackType))
                {
                    Util.GridSetData(item.GRID, dtLogisPackType, FrameOperation);
                }
            }
        }

        // Popup - Popup Control에 현재 선택된 Grid의 Cell 정보를 저장
        private void SetPopupControlTag(C1.WPF.DataGrid.DataGridCell dataGridCell)
        {
            C1.WPF.DataGrid.DataGridCell dataGridCellCurrent = (C1.WPF.DataGrid.DataGridCell)this.popupBaseInfo.Tag;
            this.popupBaseInfo.Tag = dataGridCell;
            if (dataGridCellCurrent == null)
            {
                return;
            }

            if (dataGridCell.Equals(dataGridCellCurrent))
            {
                return;
            }
        }

        // Popup - Popup Control에 현재 선택된 Grid의 Cell 정보를 저장
        private void SetPopupDateControlTag(C1.WPF.DataGrid.DataGridCell dataGridCell)
        {
            C1.WPF.DataGrid.DataGridCell dataGridCellCurrent = (C1.WPF.DataGrid.DataGridCell)this.popupDate.Tag;
            this.popupDate.Tag = dataGridCell;
            if (dataGridCellCurrent == null)
            {
                return;
            }

            if (dataGridCell.Equals(dataGridCellCurrent))
            {
                return;
            }
        }

        // 조회영역 ComboBox Binding
        private void SetComboBox()
        {
            this.SetComboBox(this.dataHelper.GetPackEquipmentInfo(), this.cboPackEqptID1);
        }

        // 조회영역 ComboBox Binding
        private void SetComboBox(DataTable dtSource, C1ComboBox c1ComboBox)
        {
            if (!CommonVerify.HasTableRow(dtSource))
            {
                return;
            }

            DataTable dtBinding = dtSource.Copy();
            string codeColumnName = dtBinding.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Contains("ID")).Select(x => x.ColumnName).FirstOrDefault();
            string codeValueColumnName = dtBinding.Columns.OfType<DataColumn>().Where(x => x.ColumnName.Contains("NAME")).Select(x => x.ColumnName).FirstOrDefault();
            if (codeColumnName == null || codeValueColumnName == null)
            {
                return;
            }

            DataRow drBinding = dtBinding.NewRow();
            drBinding[codeColumnName] = null;
            drBinding[codeValueColumnName] = "-ALL-";
            dtBinding.Rows.InsertAt(drBinding, 0);

            c1ComboBox.DisplayMemberPath = codeValueColumnName;
            c1ComboBox.SelectedValuePath = codeColumnName;
            c1ComboBox.ItemsSource = dtBinding.AsDataView();
            c1ComboBox.SelectedIndex = 0;
        }

        // 조회
        private void SearchProcess(Button button)
        {
            this.popupBaseInfo.IsOpen = false;
            switch (button.Tag.ToString())
            {
                case PACK_EQUIPMENT_SET:
                    PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
                    Util.gridClear(this.dgGrid2);
                    this.SearchPackEquipmentProductBaseSet();
                    this.numericCount2.IsEnabled = false;
                    this.btnPlus2.IsEnabled = false;
                    this.btnMinus2.IsEnabled = false;
                    this.btnSave2.IsEnabled = false;
                    break;
                default:
                    break;
            }
        }

        // 조회
        private void SearchPackEquipmentProductBaseSet()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtPackEquipmentProductBaseSet, 0);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackEquipmentProductBaseSet(this.cboPackEqptID1.SelectedValue);
                PackCommon.SearchRowCount(ref this.txtPackEquipmentProductBaseSet, dt.Rows.Count);
                Util.GridSetData(this.dgGrid1, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 조회
        private void SearchPackTransferMixSet()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
                PackCommon.DoEvents();

                DataTable dt = this.dataHelper.GetPackTransferMixSet(this.dgGrid1, "CHK_MIX_BOX_CMA", "", "");
                if (CommonVerify.HasTableRow(dt))
                {
                    // CR/LF 변환
                    foreach (DataRow dr in dt.Rows)
                    {
                        dr["PROD_AREAID_LIST_NAME"] = dr["PROD_AREAID_LIST_NAME"].ToString().Replace(",", Environment.NewLine);
                        dr["PROD_ELTR_LINE_LIST_NAME"] = dr["PROD_ELTR_LINE_LIST_NAME"].ToString().Replace(",", Environment.NewLine);
                        dr["PROD_ASSY_LINE_LIST_NAME"] = dr["PROD_ASSY_LINE_LIST_NAME"].ToString().Replace(",", Environment.NewLine);
                        dr["PROD_PACK_LINE_LIST_NAME"] = dr["PROD_PACK_LINE_LIST_NAME"].ToString().Replace(",", Environment.NewLine);
                    }
                    dt.AcceptChanges();

                    PackCommon.SearchRowCount(ref this.txtGridRowCount2, dt.Rows.Count);
                }
                else
                {
                    PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
                }
                Util.GridSetData(this.dgGrid2, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        // 저장
        private void SaveProcess(Button button)
        {
            switch (button.Tag.ToString())
            {
                case PACK_EQUIPMENT_SET:
                    this.SavePackEquipmentSet(button);
                    break;
                case TRANSFER_SET:
                    this.SaveTransferMixSet(button);
                    break;
                default:
                    break;
            }
        }

        // 포장기 설정 정보 저장
        private void SavePackEquipmentSet(Button button, DataTable dt, string messageID, bool isCompleted)
        {
            Util.MessageConfirm(messageID, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (this.dataHelper.SavePackEquipmentSet(dt, this.ucPersonInfo.UserID, isCompleted))
                    {
                        this.SearchProcess(button);
                    }
                }
            });
        }

        // 포장기 설정 정보 저장 (반송 정보 완료 Update 포함)
        private void SavePackEquipmentSet(bool isCompleted)
        {
            if (this.popupTransferConfirm.Tag == null)
            {
                return;
            }
            this.popupTransferConfirm.IsOpen = false;
            this.popupTransferConfirm.HorizontalOffset = 0;
            this.popupTransferConfirm.VerticalOffset = 0;

            List<object> lstParam = (List<object>)this.popupTransferConfirm.Tag;

            // ByPass Mode이면 Message 뿌려주기
            string messageID = "SFU3533";
            DataTable dtEIOOperMode = (DataTable)lstParam[3];
            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }

            this.SavePackEquipmentSet(this.btnSearch1, (DataTable)lstParam[1], messageID, isCompleted);
        }
        // MICA 포장기 설정 정보 저장  (반송 정보 완료 Update 포함)
        private void SavePackEquipmentSet_MICA(bool isCompleted, DataTable dtGridData, DataTable dtEIOOperMode)
        {
            string messageID = "SFU3533";

            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }

            this.SavePackEquipmentSet(this.btnSearch1, (DataTable)dtGridData, messageID, isCompleted);
        }

        // 포장기 설정 정보 저장
        private void SavePackEquipmentSet(Button button)
        {
            // Declarations...
            string messageID = "SFU3533";
            string transferGenerteTypeCode = string.Empty;
            string transferRequestStatusCode = "REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS";
            DataTable dtEIOOperMode = new DataTable();

            C1DataGrid c1DataGrid = PackCommon.FindChildControl<C1DataGrid>(this.grdContent, "TAG", button.Tag.ToString());
            if (c1DataGrid == null)
            {
                return;
            }

            if (!this.ValidationCheckPackEquipmentProductBaseSet(c1DataGrid))
            {
                return;
            }

            DataTable dtGridData = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
            DataTable dtTarget = dtGridData.Clone();         // Target Table...

            // Step 0 : 포장기의 EIO Operation Mode 체크
            foreach (DataRowView drv in dtGridData.AsDataView())
            {
                dtEIOOperMode = this.dataHelper.GetEIOOperMode(drv["PACK_EQPTID"].ToString());
            }

            // Step 1 : 포장기 설정 속성값이 변경이 되었는지 조회 (변경 속성값 : 제품ID, 자동반송여부, 사용여부)
            foreach (DataRowView drvGridData in dtGridData.AsDataView())
            {
                DataTable dtCheck = this.dataHelper.GetPackEquipmentProductBaseSet(drvGridData["PACK_EQPTID"].ToString());
                if (CommonVerify.HasTableRow(dtCheck))
                {
                    var checkQuery = dtGridData.AsEnumerable().Where(x => x.Field<string>("PACK_EQPTID").Equals(drvGridData["PACK_EQPTID"].ToString())).Select(x => new
                    {
                        PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                        PRODID = x.Field<string>("PRODID"),
                        AUTO_TRF_FLAG = x.Field<string>("AUTO_TRF_FLAG"),
                        USE_FLAG = x.Field<string>("USE_FLAG")
                    }).Except(dtCheck.AsEnumerable().Select(y => new
                    {
                        PACK_EQPTID = y.Field<string>("PACK_EQPTID"),
                        PRODID = y.Field<string>("PRODID"),
                        AUTO_TRF_FLAG = y.Field<string>("AUTO_TRF_FLAG"),
                        USE_FLAG = y.Field<string>("USE_FLAG")
                    }));
                    if (checkQuery.Count() > 0)
                    {
                        if (dtTarget.Columns.Count <= 0)
                        {
                            dtTarget = drvGridData.Row.Table.Clone();
                        }
                        dtTarget.ImportRow(drvGridData.Row);
                        dtTarget.AcceptChanges();
                    }
                }
                else
                {
                    if (dtTarget.Columns.Count <= 0)
                    {
                        dtTarget = drvGridData.Row.Table.Clone();
                    }
                    dtTarget.ImportRow(drvGridData.Row);
                    dtTarget.AcceptChanges();
                }
            }

            // Step 2 :Step 1에서 구한 포장기 설정 변경데이터에 상응하는 반송 요청 데이터가 있는지 조회
            DataTable dtTransferRequest = new DataTable();
            foreach (DataRowView drv in dtTarget.AsDataView())
            {
                DataTable dt = this.dataHelper.GetModuleTransferHistoryList(drv["PACK_EQPTID"].ToString(), string.Empty, transferRequestStatusCode, transferGenerteTypeCode);
                if (CommonVerify.HasTableRow(dt))
                {
                    // Schema Copy & Import Row & Commit
                    if (dtTransferRequest.Columns.Count <= 0)
                    {
                        dtTransferRequest = dt.Clone();
                    }
                    foreach (DataRowView drvTransferRequest in dt.AsDataView())
                    {
                        dtTransferRequest.ImportRow(drvTransferRequest.Row);
                    }
                    dtTransferRequest.AcceptChanges();
                }
            }

            // Step 3 : 자동 반송 요청정보가 없는 경우에는 그냥 저장.
            if (!CommonVerify.HasTableRow(dtTransferRequest))
            {
                messageID = "SFU3533";
                // ByPass Mode이면 Message 뿌려주기
                if (CommonVerify.HasTableRow(dtEIOOperMode))
                {
                    foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                    {
                        messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                    }
                }
                this.SavePackEquipmentSet(button, dtGridData, messageID, false);
                return;
            }
            
            // Step 4 : 포장기 설정의 중요 속성 데이터가 변경되었고, 반송 요청정보가 있는 경우에는 Popup 표출후에 누르는 버튼에 따라 포장기 설정 정보만 저장할지 둘다 저장할지 결정함.
            if (CommonVerify.HasTableRow(dtTarget) && CommonVerify.HasTableRow(dtTransferRequest))
            {
                //MICA 설비의 경우 무조건 종료 기능 추가
                if (chkEqsg(dtTarget.Rows[0]["PACK_EQPTID"].ToString()))
                {
                    this.SavePackEquipmentSet_MICA(true, dtGridData, dtEIOOperMode);
                }
                else
                {
                    List<object> lstParam = new List<object>();
                    lstParam.Add(PACK_EQUIPMENT_SET);
                    lstParam.Add(dtGridData);
                    lstParam.Add(dtTransferRequest);
                    lstParam.Add(dtEIOOperMode);
                    this.ShowPopupTransferConfirm(lstParam);
                }
                return;
            }
        }

        // 투입 및 혼입 설정 정보 저장
        private void SaveTransferMixSet(Button button)
        {
            // Declarations...
            string messageID = "SFU3533";
            string transferGenerteTypeCode = "AUTO";
            string transferRequestStatusCode = "REQUEST_LOGIS,RECEIVING_LOGIS,CLOSED_LOGIS";
            DataTable dtEIOOperMode = new DataTable();

            C1DataGrid c1DataGrid = PackCommon.FindChildControl<C1DataGrid>(this.grdContent, "TAG", button.Tag.ToString());
            if (!this.ValidationCheckPackTransferMixSet(c1DataGrid))
            {
                return;
            }
            DataTable dtGridData = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).CopyToDataTable();
            DataTable dtTarget = dtGridData.Clone();         // Target Table...

            // Step 0 : 포장기의 EIO Operation Mode 체크
            var query = dtGridData.AsEnumerable().GroupBy(x => new
            {
                PACK_EQPTID = x.Field<string>("PACK_EQPTID")
            }).Select(grp => new
            {
                PACK_EQPTID = grp.Key.PACK_EQPTID
            });
            foreach (var item in query)
            {
                dtEIOOperMode = this.dataHelper.GetEIOOperMode(item.PACK_EQPTID);
            }

            // Step 1 : 투입 및 혼입 설정 속성값이 변경이 되었는지 조회 - (값을 변경한게 없으면 저장하는 것에서 빼버리기)
            foreach (DataRowView drvGridData in dtGridData.AsDataView())
            {
                DataTable dtCheck = this.dataHelper.GetPackTransferMixSet(drvGridData["INPUT_MIX_TYPE_CODE"].ToString(), drvGridData["PACK_EQPTID"].ToString(), drvGridData["PRODID"].ToString());
                if (CommonVerify.HasTableRow(dtCheck))
                {
                    var checkQuery = dtGridData.AsEnumerable().Where(x => x.Field<string>("INPUT_MIX_TYPE_CODE").Equals(drvGridData["INPUT_MIX_TYPE_CODE"].ToString()) &&
                                                                          x.Field<string>("PACK_EQPTID").Equals(drvGridData["PACK_EQPTID"].ToString()) &&
                                                                          x.Field<string>("PRODID").Equals(drvGridData["PRODID"].ToString())).Select(x => new
                                                                          {
                                                                              INPUT_MIX_TYPE_CODE = x.Field<string>("INPUT_MIX_TYPE_CODE"),
                                                                              PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                                                                              PRODID = x.Field<string>("PRODID"),
                                                                              INPUT_MIX_CHK_MTHD_CODE = x.Field<string>("INPUT_MIX_CHK_MTHD_CODE"),
                                                                              PROD_AREAID_LIST = x.Field<string>("PROD_AREAID_LIST"),
                                                                              PROD_ELTR_LINE_LIST = x.Field<string>("PROD_ELTR_LINE_LIST"),
                                                                              PROD_ASSY_LINE_LIST = x.Field<string>("PROD_ASSY_LINE_LIST"),
                                                                              PROD_PACK_LINE_LIST = x.Field<string>("PROD_PACK_LINE_LIST"),
                                                                              USE_FLAG = x.Field<string>("USE_FLAG"),
                                                                              PACK_MIX_TYPE_CODE = x.Field<string>("PACK_MIX_TYPE_CODE"),
                                                                              BOX_LOT_SET_DATE = string.IsNullOrEmpty(x.Field<string>("BOX_LOT_SET_DATE")) ? "ALL" : x.Field<string>("BOX_LOT_SET_DATE"),
                                                                              LOGIS_PACK_TYPE = string.IsNullOrEmpty(x.Field<string>("LOGIS_PACK_TYPE")) ? "PACK" : x.Field<string>("LOGIS_PACK_TYPE")
                                                                          }).Except(dtCheck.AsEnumerable().Select(y => new
                                                                          {
                                                                              INPUT_MIX_TYPE_CODE = y.Field<string>("INPUT_MIX_TYPE_CODE"),
                                                                              PACK_EQPTID = y.Field<string>("PACK_EQPTID"),
                                                                              PRODID = y.Field<string>("PRODID"),
                                                                              INPUT_MIX_CHK_MTHD_CODE = y.Field<string>("INPUT_MIX_CHK_MTHD_CODE"),
                                                                              PROD_AREAID_LIST = y.Field<string>("PROD_AREAID_LIST"),
                                                                              PROD_ELTR_LINE_LIST = y.Field<string>("PROD_ELTR_LINE_LIST"),
                                                                              PROD_ASSY_LINE_LIST = y.Field<string>("PROD_ASSY_LINE_LIST"),
                                                                              PROD_PACK_LINE_LIST = y.Field<string>("PROD_PACK_LINE_LIST"),
                                                                              USE_FLAG = y.Field<string>("USE_FLAG"),
                                                                              PACK_MIX_TYPE_CODE = y.Field<string>("PACK_MIX_TYPE_CODE"),
                                                                              BOX_LOT_SET_DATE = y.Field<string>("BOX_LOT_SET_DATE"),
                                                                              LOGIS_PACK_TYPE = y.Field<string>("LOGIS_PACK_TYPE")
                                                                          }));
                    if (checkQuery.Count() > 0)
                    {
                        if (dtTarget.Columns.Count <= 0)
                        {
                            dtTarget = drvGridData.Row.Table.Clone();
                        }
                        dtTarget.ImportRow(drvGridData.Row);
                        dtTarget.AcceptChanges();
                    }
                }
                else
                {
                    if (dtTarget.Columns.Count <= 0)
                    {
                        dtTarget = drvGridData.Row.Table.Clone();
                    }
                    dtTarget.ImportRow(drvGridData.Row);
                    dtTarget.AcceptChanges();
                }
            }

            if (!CommonVerify.HasTableRow(dtTarget))
            {
                Util.Alert("SFU1566");      // 변경된 데이터가 없습니다.
                return;
            }

            // Step 2 :Step 1에서 구한 투입 혼입 설정 변경데이터에 상응하는 자동 반송 요청 데이터가 있는지 조회
            DataTable dtTransferRequest = new DataTable();
            foreach (DataRowView drv in dtTarget.AsDataView())
            {
                DataTable dt = this.dataHelper.GetModuleTransferHistoryList(drv["PACK_EQPTID"].ToString(), drv["PRODID"].ToString(), transferRequestStatusCode, transferGenerteTypeCode);
                if (CommonVerify.HasTableRow(dt))
                {
                    // Schema Copy & Import Row & Commit
                    if (dtTransferRequest.Columns.Count <= 0)
                    {
                        dtTransferRequest = dt.Clone();
                    }
                    foreach (DataRowView drvTransferRequest in dt.AsDataView())
                    {
                        dtTransferRequest.ImportRow(drvTransferRequest.Row);
                    }
                    dtTransferRequest.AcceptChanges();
                }
            }

            // Step 3 : 자동 반송 요청정보가 없는 경우에는 그냥 저장.
            if (!CommonVerify.HasTableRow(dtTransferRequest))
            {
                messageID = "SFU3533";
                // ByPass Mode이면 Message 뿌려주기
                if (CommonVerify.HasTableRow(dtEIOOperMode))
                {
                    foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                    {
                        messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                    }
                }
                this.SaveTransferMixSet(button, dtGridData, messageID, false);
                return;
            }

            // Step 4 : 자동 반송 요청정보가 있는 경우에는 Popup 표출후에 누르는 버튼에 따라 투입 혼입 설정 정보만 저장할지 둘다 저장할지 결정함.
            if (CommonVerify.HasTableRow(dtTarget) && CommonVerify.HasTableRow(dtTransferRequest))
            {
                if (chkEqsg(dtTarget.Rows[0]["PACK_EQPTID"].ToString()))
                {
                    this.SaveTrasnferMixSet_MICA(true, dtTarget, dtEIOOperMode);
                }
                else
                {
                    List<object> lstParam = new List<object>();
                    lstParam.Add(TRANSFER_SET);
                    lstParam.Add(dtTarget);
                    lstParam.Add(dtTransferRequest);
                    lstParam.Add(dtEIOOperMode);
                    this.ShowPopupTransferConfirm(lstParam);
                }
            }
        }

        // 투입 및 혼입 설정 정보 저장
        private void SaveTransferMixSet(Button button, DataTable dt, string messageID, bool isCompleted)
        {
            Util.MessageConfirm(messageID, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (this.dataHelper.SaveTransferMixSet(dt, this.ucPersonInfo.UserID, isCompleted))
                    {
                        this.SearchPackTransferMixSet();
                    }
                }
            });
        }

        // 투입 및 혼입 설정 정보 저장 (반송 정보 완료 Update 포함)
        private void SaveTrasnferMixSet(bool isCompleted)
        {
            if (this.popupTransferConfirm.Tag == null)
            {
                return;
            }
            this.popupTransferConfirm.IsOpen = false;
            this.popupTransferConfirm.HorizontalOffset = 0;
            this.popupTransferConfirm.VerticalOffset = 0;

            List<object> lstParam = (List<object>)this.popupTransferConfirm.Tag;

            // ByPass Mode이면 Message 뿌려주기
            string messageID = "SFU3533";
            DataTable dtEIOOperMode = (DataTable)lstParam[3];
            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }

            this.SaveTransferMixSet(this.btnSearch1, (DataTable)lstParam[1], messageID, isCompleted);
        }

        // 투입 및 혼입 설정 정보 저장 (반송 정보 완료 Update 포함)
        private void SaveTrasnferMixSet_MICA(bool isCompleted, DataTable dtTarget, DataTable dtEIOOperMode)
        {
            string messageID = "SFU3533";

            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }
            this.SaveTransferMixSet(this.btnSearch1, dtTarget, messageID, isCompleted);
        }
        
        // Validation Check
        private bool ValidationCheckPackEquipmentProductBaseSet(C1DataGrid c1DataGrid)
        {
            if (c1DataGrid == null || c1DataGrid.ItemsSource == null || c1DataGrid.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            // Validation Check
            DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
            var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));

            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("PACK_EQPTID"))).Count() > 0)
            {
                Util.Alert("9080");  // 설비를 선택하여 주십시오.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("PRODID"))).Count() > 0)
            {
                Util.Alert("SFU1895");  // 제품을 선택하세요.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("USE_FLAG"))).Count() > 0)
            {
                Util.Alert("SFU8354");  // 사용여부를 선택하세요.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("AUTO_TRF_FLAG"))).Count() > 0)
            {
                Util.Alert("SFU8355");  // 자동 또는 수동반송여부를 선택하세요.
                c1DataGrid.Focus();
                return false;
            }

            if (queryValidationCheck.Where(x => x.Field<decimal>("NEXT_REQ_BAS_QTY") <= 0).Count() > 0)
            {
                Util.Alert("SFU8388");  // 반송요청도달수량은 0보다 커야 합니다.
                c1DataGrid.Focus();
                return false;
            }

            if (queryValidationCheck.Where(x => x.Field<decimal>("WAIT_ENABLE_TIME") <= 0).Count() > 0)
            {
                Util.Alert("SFU8383");  // 대기경과시간은은 0보다 커야 합니다.
                c1DataGrid.Focus();
                return false;
            }

            if (queryValidationCheck.Where(x => x.Field<decimal>("TRF_LOT_QTY") <= 0).Count() > 0)
            {
                Util.Alert("SFU8384");  // 반송 LOT수량은 0보다 커야 합니다.
                c1DataGrid.Focus();
                return false;
            }
            return true;
        }

        // Validation Check
        private bool ValidationCheckPackTransferMixSet(C1DataGrid c1DataGrid)
        {
            if (c1DataGrid == null || c1DataGrid.ItemsSource == null || c1DataGrid.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
                return false;
            }
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return false;
            }

            // Validation Check
            DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
            var queryValidationCheck = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));

            if (queryValidationCheck.Count() <= 0)
            {
                Util.Alert("10008");  // 선택된 데이터가 없습니다.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("PACK_EQPTID"))).Count() > 0)
            {
                Util.Alert("9080");  // 설비를 선택하여 주십시오.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("PRODID"))).Count() > 0)
            {
                Util.Alert("SFU1895");  // 제품을 선택하세요.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("USE_FLAG"))).Count() > 0)
            {
                Util.Alert("SFU8354");  // 사용여부를 선택하세요.
                c1DataGrid.Focus();
                return false;
            }
            if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("INPUT_MIX_CHK_MTHD_CODE"))).Count() > 0)
            {
                Util.Alert("SFU8356");  // 투입/혼입 방법을 선택하세요.
                c1DataGrid.Focus();
                return false;
            }

            return true;
        }

        // 각 Grid에서 Row 추가시에 Key Validation Check
        private bool ValidationCheckDup(C1DataGrid c1DataGrid, int rowIndex, string keyColumn, string keyValue)
        {
            bool returnValue = true;
            switch (c1DataGrid.Tag.ToString())
            {
                case PACK_EQUIPMENT_SET:
                    returnValue = this.ValidationCheckDupPackEquipmentSet(c1DataGrid, keyColumn, keyValue);
                    break;
                case TRANSFER_SET:
                    returnValue = this.ValidationCheckDupTransferSet(c1DataGrid, rowIndex, keyColumn, keyValue);
                    break;
                default:
                    break;
            }

            return returnValue;
        }

        // 포장기 설정 추가시에 Key Validation Check
        private bool ValidationCheckDupPackEquipmentSet(C1DataGrid c1DataGrid, string keyColumn, string keyValue)
        {
            bool returnValue = true;

            // keyColumn Validation
            if (!keyColumn.Equals("PACK_EQPTID"))
            {
                return returnValue;
            }

            // Grid에 표출된 Key Dup Check
            DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
            if (CommonVerify.HasTableRow(dt))
            {
                if (dt.AsEnumerable().Where(x => x.Field<string>("PACK_EQPTID") != null && x.Field<string>("PACK_EQPTID").Equals(keyValue)).Count() > 0)
                {
                    returnValue = false;
                    Util.Alert("SFU2051", keyValue);      // 중복 데이터가 존재 합니다. %1
                    return returnValue;
                }
            }

            // DB단의 Dup Check
            if (!string.IsNullOrEmpty(keyValue))
            {
                DataTable dtReturnCheck = this.dataHelper.GetPackEquipmentProductBaseSet(keyValue);
                if (CommonVerify.HasTableRow(dtReturnCheck))
                {
                    returnValue = false;
                    Util.Alert("SFU2051", keyValue);      // 중복 데이터가 존재 합니다. %1
                    return returnValue;
                }
            }

            return returnValue;
        }

        // 투입 및 혼입 설정 추가시에 Key Validation Check
        private bool ValidationCheckDupTransferSet(C1DataGrid c1DataGrid, int rowIndex, string keyColumn, string keyValue)
        {
            bool returnValue = true;

            // keyColumn Validation
            if (!keyColumn.Equals("INPUT_MIX_TYPE_CODE") && !keyColumn.Equals("PACK_EQPTID") && !keyColumn.Equals("PRODID"))
            {
                return returnValue;
            }

            string inputMixTypeCode = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "INPUT_MIX_TYPE_CODE"));
            string packEqptID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "PACK_EQPTID"));
            string prodID = Util.NVC(DataTableConverter.GetValue(c1DataGrid.Rows[rowIndex].DataItem, "PRODID"));
            switch (keyColumn)
            {
                case "INPUT_MIX_TYPE_CODE":
                    inputMixTypeCode = Util.NVC(keyValue);
                    break;
                case "PACK_EQPTID":
                    packEqptID = Util.NVC(keyValue);
                    break;
                case "PRODID":
                    prodID = Util.NVC(keyValue);
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(inputMixTypeCode) && !string.IsNullOrEmpty(packEqptID) && !string.IsNullOrEmpty(prodID))
            {
                // Grid에 표출된 Key Dup Check
                DataTable dt = DataTableConverter.Convert(c1DataGrid.ItemsSource);
                if (CommonVerify.HasTableRow(dt))
                {
                    var query = dt.AsEnumerable().Where(x => x.Field<string>("INPUT_MIX_TYPE_CODE") != null && x.Field<string>("INPUT_MIX_TYPE_CODE").Equals(inputMixTypeCode) &&
                                                             x.Field<string>("PACK_EQPTID") != null && x.Field<string>("PACK_EQPTID").Equals(packEqptID) &&
                                                             x.Field<string>("PRODID") != null && x.Field<string>("PRODID").Equals(prodID)).Count();
                    if (query > 0)
                    {
                        returnValue = false;
                        Util.Alert("SFU2051", string.Empty);      // 중복 데이터가 존재 합니다. %1
                        return returnValue;
                    }
                }

                // DB단의 Dup Check
                DataTable dtReturnCheck = this.dataHelper.GetPackTransferMixSet(inputMixTypeCode, packEqptID, prodID);
                if (CommonVerify.HasTableRow(dtReturnCheck))
                {
                    returnValue = false;
                    Util.Alert("SFU2051", string.Empty);      // 중복 데이터가 존재 합니다. %1
                    return returnValue;
                }
            }

            return returnValue;
        }

        // Popup에서 선택한 항목들을 호출한 Grid Cell에다가 집어넣기
        private void SelectItemProcess()
        {
            string resultCode = string.Empty;
            string resultCodeName = string.Empty;

            var query = this.GetGridInfo().Where(x => this.grdPopup.Children.OfType<C1DataGrid>().Where(y => y.Equals(x.GRID) && x.IS_LASTGRID).Any());
            if (query.Count() <= 0)
            {
                return;
            }

            C1.WPF.DataGrid.DataGridCell dataGridCell = (C1.WPF.DataGrid.DataGridCell)this.popupBaseInfo.Tag;
            if (dataGridCell == null)
            {
                return;
            }
            C1DataGrid c1DataGrid = dataGridCell.DataGrid;

            foreach (var item in query)
            {
                if (item.GRID.ItemsSource == null)
                {
                    DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, string.Empty);
                    DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, string.Empty);
                    c1DataGrid.Refresh();
                    this.popupBaseInfo.IsOpen = false;
                    return;
                }
                DataTable dt = DataTableConverter.Convert(item.GRID.ItemsSource);
                if (!CommonVerify.HasTableRow(dt))
                {
                    DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, string.Empty);
                    DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, string.Empty);
                    c1DataGrid.Refresh();
                    this.popupBaseInfo.IsOpen = false;
                    return;
                }
            }

            try
            {
                foreach (var item in query)
                {
                    string codeColumnName = item.CODE_COLUMN;
                    string codeNameColumnName = item.CODENAME_COLUMN;
                    DataTable dt = DataTableConverter.Convert(item.GRID.ItemsSource);
                    resultCode = string.Join(",", dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>(codeColumnName)).ToList());
                    resultCodeName = string.Join(Environment.NewLine, dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>(codeNameColumnName)).ToList());

                    // Grid 사용자 정의 속성중에 MULTI_CHECK 여부가 FALSE인 것들은 부모그리드에 있는 값을 그대로 보존함.
                    if (string.IsNullOrEmpty(resultCode) && !Convert.ToBoolean(item.MULTI_CHECK))
                    {
                        continue;
                    }

                    if (this.ValidationCheckDup(c1DataGrid, dataGridCell.Row.Index, item.LINKED_VALUEPATH_COLUMN, resultCode))
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, resultCode);
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, resultCodeName);
                    }
                    else
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, string.Empty);
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, string.Empty);
                    }
                }
                c1DataGrid.Refresh();
                foreach (var item in query)
                {
                    if (item.CATEGORY.Equals("PRODUCT") || item.CATEGORY.Equals("PACK_EQUIPMENT"))
                    {
                        this.SetGridRowColor();
                    }
                }

                this.popupBaseInfo.IsOpen = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Popup안에 있는 Grid에서 선택표시된거 싸그리 지우기
        private void ClearProcess()
        {
            try
            {
                var query = this.GetGridInfo().Where(x => this.grdPopup.Children.OfType<C1DataGrid>().Where(y => y.Equals(x.GRID)).Any());
                foreach (var item in query)
                {
                    if (item.GRID.ItemsSource == null)
                    {
                        continue;
                    }
                    DataTable dt = DataTableConverter.Convert(item.GRID.ItemsSource);
                    if (!CommonVerify.HasTableRow(dt))
                    {
                        continue;
                    }

                    foreach (DataRow dr in dt.Rows)
                    {
                        if (query.Count() == 1 || item.DATA_PROPERTIES.Contains("SHOP"))
                        {
                            if (dr["CHK"].SafeToBoolean())
                            {
                                dr["CHK"] = false;
                            }

                            dt.AcceptChanges();
                            Util.GridSetData(item.GRID, dt, FrameOperation);
                        }
                        else
                        {
                            Util.gridClear(item.GRID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 저장영역 더하기 빼기 버튼 눌렀을때 처리
        private void DataGridRowOperation(Button button)
        {
            string[] arrCheck = button.Tag.ToString().Split('|');

            // Find Control
            C1DataGrid c1DataGrid = PackCommon.FindChildControl<C1DataGrid>(this.grdContent, "TAG", arrCheck[0]);
            C1NumericBox c1NumericBox = PackCommon.FindChildControl<C1NumericBox>(this.grdContent, "TAG", arrCheck[0]);

            if (c1DataGrid == null || c1NumericBox == null)
            {
                return;
            }

            // Validation
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return;
            }

            if (c1DataGrid.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회하십시오.
                return;
            }

            // numeric Control.isEnabled = false이고 TAG = TRANSFER_SET이며 더하기 Button 눌러제꼈을경우
            if (c1NumericBox.IsEnabled && c1NumericBox.Tag.ToString().Equals(TRANSFER_SET))
            {
                switch (arrCheck[1].ToUpper())
                {
                    case "ADD":
                        // 기본적으로 투입 및 혼입설정 Grid에 신규 Row 추가시 포장기와 제품 ID값을 복사함.
                        var queryPackEquipmentInfo = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => new
                        {
                            PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                            PACK_EQPTNAME = x.Field<string>("PACK_EQPTNAME"),
                            PRODID = x.Field<string>("PRODID"),
                            PRODNAME = x.Field<string>("PRODNAME")
                        });

                        // Validation Check (Grid Data)
                        var queryValidation = DataTableConverter.Convert(this.dgGrid2.ItemsSource).AsEnumerable()
                                             .Where(x => queryPackEquipmentInfo.Where(y => y.PACK_EQPTID.Equals(x.Field<string>("PACK_EQPTID")) &&
                                                                                           y.PRODID.Equals(x.Field<string>("PRODID"))).Any());

                        // Validation Check (DB Data)
                        int queryValidationDB = 0;
                        foreach (var item in queryPackEquipmentInfo)
                        {
                            DataTable dt = this.dataHelper.GetPackTransferMixSet("CHK_MIX_BOX_CMA", item.PACK_EQPTID, string.Empty);
                            if (CommonVerify.HasTableRow(dt))
                            {
                                queryValidationDB = dt.Rows.Count;
                            }
                        }

                        foreach (var item in queryPackEquipmentInfo)
                        {
                            c1DataGrid.BeginNewRow();
                            c1DataGrid.EndNewRow(true);
                            DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "INPUT_MIX_TYPE_CODE", "CHK_MIX_BOX_CMA");
                            DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "PACK_EQPTID", item.PACK_EQPTID);
                            DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "PACK_EQPTNAME", item.PACK_EQPTNAME);
                        }
                        break;
                    case "MINUS":
                        Util.DataGridRowDelete(c1DataGrid, 1);
                        break;
                    default:
                        break;
                }
            }
        }

        // New Row Event가 발생했을 때
        private void BeginningNewRowEventFireProcess(C1DataGrid c1DataGrid, DataGridBeginningNewRowEventArgs e)
        {
        }

        // Grid의 Header CheckBox Check 또는 Uncheck했을때
        private void SetGridRowChecked(CheckBox checkBox, bool check)
        {
            C1DataGrid c1DataGrid = PackCommon.FindChildControl<C1DataGrid>(this.grdContent, "TAG", checkBox.Tag.ToString());
            if (c1DataGrid == null)
            {
                return;
            }

            PackCommon.GridCheckAllFlag(c1DataGrid, check, "CHK");
        }

        // 포장기 설정 Grid에서 선택된 항목에 상응하는 투입 혼입 설정 Grid에 Row가 존재하면 색깔 바꾸기
        private void SetGridRowColor()
        {
            try
            {
                var query = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => new
                {
                    PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                    PRODID = x.Field<string>("PRODID")
                });

                if (query.Count() <= 0)
                {
                    return;
                }

                for (int i = 0; i < this.dgGrid2.GetRowCount(); i++)
                {
                    var packEquipmentID = DataTableConverter.GetValue(this.dgGrid2.Rows[i].DataItem, "PACK_EQPTID");
                    var productID = DataTableConverter.GetValue(this.dgGrid2.Rows[i].DataItem, "PRODID");
                    var checkQuery = query.Where(x => x.PACK_EQPTID.Equals(packEquipmentID) && x.PRODID.Equals(productID)).Count();
                    if (this.dgGrid2.Rows[i].Presenter == null)
                    {
                        continue;
                    }
                    this.dgGrid2.Rows[i].Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    this.dgGrid2.Rows[i].Presenter.FontWeight = checkQuery > 0 ? FontWeights.Bold : FontWeights.Normal;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
        }

        private void btnPlusMinus_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string[] arrCheck = button.Tag.ToString().Split('|');

            // Find Control
            C1DataGrid c1DataGrid = PackCommon.FindChildControl<C1DataGrid>(this.grdContent, "TAG", arrCheck[0]);
            C1NumericBox c1NumericBox = PackCommon.FindChildControl<C1NumericBox>(this.grdContent, "TAG", arrCheck[0]);

            if (c1DataGrid == null || c1NumericBox == null)
            {
                return;
            }

            // Validation
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return;
            }

            if (c1DataGrid.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회하십시오.
                return;
            }

            // numeric Control.isEnabled = false이고 TAG = TRANSFER_SET이며 더하기 Button 눌러제꼈을경우
            if (c1NumericBox.IsEnabled && c1NumericBox.Tag.ToString().Equals(TRANSFER_SET))
            {
                switch (arrCheck[1].ToUpper())
                {
                    case "ADD":
                        // 기본적으로 투입 및 혼입설정 Grid에 신규 Row 추가시 포장기값을 복사함.
                        var queryPackEquipmentInfo = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => new
                        {
                            PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                            PACK_EQPTNAME = x.Field<string>("PACK_EQPTNAME")
                        });


                        foreach (var item in queryPackEquipmentInfo)
                        {
                            if (string.IsNullOrEmpty(item.PACK_EQPTID))
                            {
                                continue;
                            }

                            for (int i = 0; i < this.numericCount2.Value.SafeToInt32(); i++)
                            {
                                c1DataGrid.BeginNewRow();
                                c1DataGrid.EndNewRow(true);
                                DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "INPUT_MIX_TYPE_CODE", "CHK_MIX_BOX_CMA");
                                DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "PACK_EQPTID", item.PACK_EQPTID);
                                DataTableConverter.SetValue(c1DataGrid.CurrentRow.DataItem, "PACK_EQPTNAME", item.PACK_EQPTNAME);
                            }
                        }
                        break;
                    case "MINUS":
                        Util.DataGridRowDelete(c1DataGrid, 1);
                        break;
                    default:
                        break;
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess((Button)sender);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess((Button)sender);
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked((CheckBox)sender, true);
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            this.SetGridRowChecked((CheckBox)sender, false);
        }

        private void c1DataGrid_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.GridDataBinding((C1DataGrid)sender, e);
        }

        private void dgGrid1_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            try
            {
                e.Item.SetValue("CHK", true);
                foreach (DataRowView dataRowView in this.dataHelper.GetAutoTransferFlagInfo(new List<string> { "Y" }, false).AsDataView())
                {
                    e.Item.SetValue("AUTO_TRF_FLAG", dataRowView["AUTO_TRF_FLAG"]);
                    e.Item.SetValue("AUTO_TRF_FLAG_NAME", dataRowView["AUTO_TRF_FLAG_NAME"]);
                }
                e.Item.SetValue("NEXT_REQ_BAS_QTY", 0);
                e.Item.SetValue("WAIT_ENABLE_TIME", 0);
                e.Item.SetValue("TRF_LOT_QTY", 0);

                foreach (DataRowView dataRowView in this.dataHelper.GetUseFlagInfo(new List<string> { "Y" }, false).AsDataView())
                {
                    e.Item.SetValue("USE_FLAG", dataRowView["USE_FLAG"]);
                    e.Item.SetValue("USE_FLAG_NAME", dataRowView["USE_FLAG_NAME"]);
                }
                e.Item.SetValue("INSUSER", LoginInfo.USERID);
                e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                e.Item.SetValue("UPDUSER", this.ucPersonInfo.UserID);
                e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGrid1_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Row.DataItem;

            if (!e.Column.Name.ToUpper().Equals("CHK") && !dataRowView.Row["CHK"].SafeToBoolean())
            {
                e.Cancel = true;
            }

            switch (dataRowView.Row.RowState)
            {
                case DataRowState.Added:
                case DataRowState.Detached:
                    break;
                default:
                    break;
            }
        }

        private void dgGrid1_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgGrid1_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Cell.Row.DataItem;

            if (e.Cell.Column.Name.ToUpper().Equals("NEXT_REQ_BAS_QTY") ||
                e.Cell.Column.Name.ToUpper().Equals("WAIT_ENABLE_TIME") ||
                e.Cell.Column.Name.ToUpper().Equals("TRF_LOT_QTY"))
            {
                return;
            }

            // 반송내역 조회시에 Check 표시가 해제되었다던가 신규 Row인 경우에는 조회안하게 함.
            if (!dataRowView.Row["CHK"].SafeToBoolean() ||
                dataRowView.Row.RowState.Equals(DataRowState.Detached) ||
                dataRowView.Row.RowState == DataRowState.Added)
            {
                this.numericCount2.IsEnabled = false;
                this.btnPlus2.IsEnabled = false;
                this.btnMinus2.IsEnabled = false;
                this.btnSave2.IsEnabled = false;
                Util.gridClear(this.dgGrid2);
                PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
                return;
            }

            this.numericCount2.IsEnabled = true;
            this.btnPlus2.IsEnabled = true;
            this.btnMinus2.IsEnabled = true;
            this.btnSave2.IsEnabled = true;
            this.SearchPackTransferMixSet();
        }

        private void dgGrid1_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

        }

        private void dgGrid2_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            try
            {
                e.Item.SetValue("CHK", true);
                foreach (DataRowView dataRowView in this.dataHelper.GetInputMixCheckMethodInfo(new List<string> { "CHK_ALLOW" }, false).AsDataView())
                {
                    e.Item.SetValue("INPUT_MIX_CHK_MTHD_CODE", dataRowView["INPUT_MIX_CHK_MTHD_CODE"].ToString());
                    e.Item.SetValue("INPUT_MIX_CHK_MTHD_CODE_NAME", dataRowView["INPUT_MIX_CHK_MTHD_CODE_NAME"].ToString());
                }

                foreach (DataRowView dataRowView in this.dataHelper.GetUseFlagInfo(new List<string> { "Y" }, false).AsDataView())
                {
                    e.Item.SetValue("USE_FLAG", dataRowView["USE_FLAG"]);
                    e.Item.SetValue("USE_FLAG_NAME", dataRowView["USE_FLAG_NAME"]);
                }

                e.Item.SetValue("INSUSER", LoginInfo.USERID);
                e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                e.Item.SetValue("UPDUSER", this.ucPersonInfo.UserID);
                e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGrid2_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Row.DataItem;

            if (!e.Column.Name.ToUpper().Equals("CHK") && !dataRowView.Row["CHK"].SafeToBoolean())
            {
                e.Cancel = true;
            }

            switch (dataRowView.Row.RowState)
            {
                case DataRowState.Added:
                case DataRowState.Detached:
                    break;
                default:
                    break;
            }
        }

        private void dgGrid2_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {

        }

        private void dgGrid2_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name.ToUpper().Equals("BOX_LOT_SET_DATE"))
            {
                Util.MessageInfo(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOX_LOT_SET_DATE").ToString());
            }
        }

        private void dgGrid2_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
        }

        private void dgGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.ShowPopUp((C1DataGrid)sender, e);
        }

        private void btnHide_Click(object sender, RoutedEventArgs e)
        {
            this.HidePopUp();
        }

        private void popupBaseInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            this.popupBaseInfo.StaysOpen = true;
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HidePopUp();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            this.SelectItemProcess();
        }

        private void btnSelectDate_Click(object sender, RoutedEventArgs e)
        {
            C1.WPF.DataGrid.DataGridCell dataGridCell = (C1.WPF.DataGrid.DataGridCell)this.popupDate.Tag;
            if (dataGridCell == null)
            {
                return;
            }
            C1DataGrid c1DataGrid = dataGridCell.DataGrid;

            var selectedDate = this.dtpBoxLotSetDate.SelectedDateTime.ToString("yyyyMMdd");
            DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "BOX_LOT_SET_DATE", selectedDate);
            c1DataGrid.Refresh();
            this.popupDate.IsOpen = false;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearProcess();
        }

        private void btnClearDate_Click(object sender, RoutedEventArgs e)
        {
            C1.WPF.DataGrid.DataGridCell dataGridCell = (C1.WPF.DataGrid.DataGridCell)this.popupDate.Tag;
            if (dataGridCell == null)
            {
                return;
            }
            C1DataGrid c1DataGrid = dataGridCell.DataGrid;

            DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, "BOX_LOT_SET_DATE", "ALL");
            c1DataGrid.Refresh();
            this.popupDate.IsOpen = false;
        }

        private void btnSaveConfirmWithTransferComplete_Click(object sender, RoutedEventArgs e)
        {
            List<object> objParam = (List<object>)this.popupTransferConfirm.Tag;

            switch (objParam[0].ToString())
            {
                case PACK_EQUIPMENT_SET:
                    this.SavePackEquipmentSet(true);
                    break;
                case TRANSFER_SET:
                    this.SaveTrasnferMixSet(true);
                    break;
                default:
                    break;
            }
        }

        private void btnSaveConfirm_Click(object sender, RoutedEventArgs e)
        {
            List<object> objParam = (List<object>)this.popupTransferConfirm.Tag;

            switch (objParam[0].ToString())
            {
                case PACK_EQUIPMENT_SET:
                    this.SavePackEquipmentSet(false);
                    break;
                case TRANSFER_SET:
                    this.SaveTrasnferMixSet(false);
                    break;
                default:
                    break;
            }
        }

        private void btnHideConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.popupTransferConfirm.IsOpen = false;
            this.popupTransferConfirm.HorizontalOffset = 0;
            this.popupTransferConfirm.VerticalOffset = 0;
        }

        private void btnPlus1_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrEmpty(this.ucPersonInfo.UserID))
            {
                Util.Alert("SFU4591");  // 작업자를 입력하세요
                this.ucPersonInfo.Focus();
                return;
            }

            if (this.dgGrid1.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회하십시오.
                return;
            }


            for (int i = 0; i <= this.dgGrid1.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(this.dgGrid1.Rows[i].DataItem, "CHK").SafeToBoolean())
                {
                    DataTableConverter.SetValue(this.dgGrid1.Rows[i].DataItem, "CHK", false);
                }
            }
            Util.gridClear(this.dgGrid2);
            PackCommon.SearchRowCount(ref this.txtGridRowCount2, 0);
            this.numericCount2.IsEnabled = false;
            this.btnPlus2.IsEnabled = false;
            this.btnMinus2.IsEnabled = false;
            Util.DataGridRowAdd(this.dgGrid1, 1);
        }

        private void btnMinus1_Click(object sender, RoutedEventArgs e)
        {
            Util.DataGridRowDelete(this.dgGrid1, 1);
        }

        private void dgGrid1CheckBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (this.dgGrid1.CurrentColumn == null || this.dgGrid1.CurrentRow == null)
                {
                    return;
                }
                if (!this.dgGrid1.CurrentColumn.Name.ToUpper().Equals("CHK"))
                {
                    return;
                }
                if (this.dgGrid1.GetRowCount() <= 0)
                {
                    return;
                }
                int currentRowIndex = this.dgGrid1.CurrentRow.Index;

                // 체크값이 True인 경우 다른 Row의 있는 내용은 False
                for (int i = 0; i < this.dgGrid1.GetRowCount(); i++)
                {
                    if (!i.Equals(currentRowIndex))
                    {
                        DataTableConverter.SetValue(this.dgGrid1.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgGrid2_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)e.Row.DataItem;

            if (dataRowView == null)
            {
                return;
            }

            string packEquipmentID = Util.NVC(DataTableConverter.GetValue(dataRowView, "PACK_EQPTID"));
            string productID = Util.NVC(DataTableConverter.GetValue(dataRowView, "PRODID"));
            var query = DataTableConverter.Convert(this.dgGrid1.ItemsSource).AsEnumerable()
                        .Where(x => x.Field<bool>("CHK") &&
                                    x.Field<string>("PACK_EQPTID").Equals(packEquipmentID) &&
                                    x.Field<string>("PRODID").Equals(productID));

            if (query.Count() <= 0)
            {
                return;
            }
            e.Row.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            e.Row.Presenter.FontWeight = FontWeights.Bold;
        }
        #endregion

        //MICA 설비 유무 확인
        public Boolean chkEqsg(string strBoxID)
        {
            bool bResult = false;
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LOGIS_PACK_UI_MICA_YN";
                dr["CMCODE"] = strBoxID;

                RQSTDT.Rows.Add(dr);

                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);
                //bResult = true;
                if (dtAuth.Rows.Count > 0)
                {
                    bResult = true;
                }
                return bResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
    }

    public class PACK003_021_DataHelper
    {
        #region Member Variable Lists...
        private DataTable dtAssyLineInfo = new DataTable();             // 조립설비
        private DataTable dtElecLineInfo = new DataTable();             // 전극설비
        private DataTable dtPackLineInfo = new DataTable();             // Pack Line
        private DataTable dtProductInfo = new DataTable();              // 제품
        private DataTable dtPackEquipmentInfo = new DataTable();        // 포장기
        private DataTable dtUseFlagInfo = new DataTable();              // Use Flag
        private DataTable dtAutoTransferFlagInfo = new DataTable();     // Auto Transfer Flag
        private DataTable dtInputMixCheckMethodInfo = new DataTable();  // Check Allow, Check Forbidden
        private DataTable dtPackMixTypeCodeInfo = new DataTable();      // 포장혼입유형코드
        private DataTable dtLogisPackType = new DataTable();            // 물류포장유형
        #endregion

        #region Constructor
        public PACK003_021_DataHelper()
        {
            this.GetBasicInfo(ref this.dtAssyLineInfo, "A");
            this.GetBasicInfo(ref this.dtElecLineInfo, "E");
            this.GetPackLineInfo(ref this.dtPackLineInfo);
            this.GetProductInfo(ref this.dtProductInfo);
            this.GetPackEquipmentInfo(ref this.dtPackEquipmentInfo);
            this.GetCommonCodeInfo(ref this.dtUseFlagInfo, "USE_FLAG");
            this.GetCommonCodeInfo(ref this.dtAutoTransferFlagInfo, "AUTO_TRF_FLAG");
            this.GetCommonCodeInfo(ref this.dtInputMixCheckMethodInfo, "PACK_UI_INPUT_MIX_CHK_MTHD_CODE");
            this.GetCommonCodeInfo(ref this.dtPackMixTypeCodeInfo, "PACK_MIX_TYPE_CODE");
            this.GetCommonCodeInfo(ref this.dtLogisPackType, "LOGIS_PACK_TYPE");
        }
        #endregion

        #region Member Function Lists...
        // BizRule 호출 - CommonCode
        private void GetCommonCodeInfo(ref DataTable dtReturn, string cmcdType)
        {
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
        }

        // BizRule 호출 - 포장기
        private void GetPackEquipmentInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_BOX_LINE_FLAG"] = "Y";
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    this.dtPackEquipmentInfo = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // BizRule 호출 - 조립동 및 전극동 Shop, Line, Equipment
        private void GetBasicInfo(ref DataTable dtReturn, string areaTypeCode)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_LOGIS_BASICINFO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = null;
                drRQSTDT["AREAID"] = null;
                drRQSTDT["AREA_TYPE_CODE"] = areaTypeCode;
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
        }

        // BizRule 호출 - Pack Line
        private void GetPackLineInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_LINE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("PACK_CELL_AUTO_LOGIS_FLAG", typeof(string));  // 반송여부(물류타는라인)
                dtRQSTDT.Columns.Add("PACK_MEB_LINE_FLAG", typeof(string));         // MEB 라인 여부
                dtRQSTDT.Columns.Add("PACK_BOX_LINE_FLAG", typeof(string));         // 자동 포장 라인 여부

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["PACK_CELL_AUTO_LOGIS_FLAG"] = DBNull.Value;
                drRQSTDT["PACK_MEB_LINE_FLAG"] = "Y";
                drRQSTDT["PACK_BOX_LINE_FLAG"] = DBNull.Value;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    dtPackLineInfo = dtRSLTDT.Copy();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // BizRule 호출 - 제품
        private void GetProductInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_ROUT_MODULE_CBO";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["EQSGID"] = null;
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
        }

        // BizRule 호출 - 포장기 속성정보 조회
        public DataTable GetPackEquipmentProductBaseSet(object objPackEquipment)
        {
            string bizRuleName = "DA_PRD_SEL_PACK_EQPT_PRDT_BAS_SET_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["PACK_EQPTID"] = string.IsNullOrEmpty(Util.NVC(objPackEquipment)) ? null : Util.NVC(objPackEquipment);
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // BizRule 호출 - 포장기 반송설정 정보 조회 (투입 및 혼입 설정 정보)
        public DataTable GetPackTransferMixSet(object objInputMixTypeCode, object objPackEquipmentID, object objProdID)
        {
            string bizRuleName = "DA_PRD_SEL_PACK_TRF_MIX_SET_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["INPUT_MIX_TYPE_CODE"] = Util.NVC(objInputMixTypeCode);
            drRQSTDT["PACK_EQPTID"] = string.IsNullOrEmpty(Util.NVC(objPackEquipmentID)) ? string.Empty : Util.NVC(objPackEquipmentID);
            drRQSTDT["PRODID"] = string.IsNullOrEmpty(Util.NVC(objProdID)) ? null : Util.NVC(objProdID);
            dtRQSTDT.Rows.Add(drRQSTDT);

            dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);

            return dtRSLTDT;
        }

        // BizRule 호출 - 포장기 속성정보 저장
        public bool SavePackEquipmentSet(DataTable dt, string updUser, bool isCompleted)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_PACK_EQPT_PRDT_BAS_SET";

            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("PACK_EQPTID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("AUTO_TRF_FLAG", typeof(string));
                dtINDATA.Columns.Add("NEXT_REQ_BAS_QTY", typeof(int));
                dtINDATA.Columns.Add("WAIT_ENABLE_TIME", typeof(int));
                dtINDATA.Columns.Add("TRF_LOT_QTY", typeof(int));
                dtINDATA.Columns.Add("USE_FLAG", typeof(string));
                dtINDATA.Columns.Add("INSUSER", typeof(string));
                dtINDATA.Columns.Add("INSDTTM", typeof(DateTime));
                dtINDATA.Columns.Add("UPDUSER", typeof(string));
                dtINDATA.Columns.Add("UPDDTTM", typeof(DateTime));
                dtINDATA.Columns.Add("IS_COMPLETED", typeof(bool));

                var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));
                foreach (var item in query)
                {
                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["PACK_EQPTID"] = item.Field<string>("PACK_EQPTID");
                    drINDATA["PRODID"] = item.Field<string>("PRODID");
                    drINDATA["AUTO_TRF_FLAG"] = item.Field<string>("AUTO_TRF_FLAG");
                    drINDATA["NEXT_REQ_BAS_QTY"] = Convert.ToInt32(item.Field<decimal>("NEXT_REQ_BAS_QTY"));
                    drINDATA["WAIT_ENABLE_TIME"] = Convert.ToInt32(item.Field<decimal>("WAIT_ENABLE_TIME"));
                    drINDATA["TRF_LOT_QTY"] = Convert.ToInt32(item.Field<decimal>("TRF_LOT_QTY"));
                    drINDATA["USE_FLAG"] = item.Field<string>("USE_FLAG");
                    drINDATA["INSUSER"] = item.Field<string>("INSUSER");
                    drINDATA["UPDUSER"] = updUser;
                    drINDATA["IS_COMPLETED"] = isCompleted;
                    dtINDATA.Rows.Add(drINDATA);
                }
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                if (!CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    Util.MessageInfo("SFU1270");        // 저장되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // BizRule 호출 - 포장기 반송설정 정보 저장 (투입 및 혼입 설정 정보)
        public bool SaveTransferMixSet(DataTable dt, string updUser, bool isCompleted)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_PACK_TRF_MIX_SET";

            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("SRCTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("SHOPID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("INPUT_MIX_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("PACK_EQPTID", typeof(string));
                dtINDATA.Columns.Add("PRODID", typeof(string));
                dtINDATA.Columns.Add("INPUT_MIX_CHK_MTHD_CODE", typeof(string));
                dtINDATA.Columns.Add("PROD_AREAID_LIST", typeof(string));
                dtINDATA.Columns.Add("PROD_ELTR_LINE_LIST", typeof(string));
                dtINDATA.Columns.Add("PROD_ASSY_LINE_LIST", typeof(string));
                dtINDATA.Columns.Add("PROD_PACK_LINE_LIST", typeof(string));
                dtINDATA.Columns.Add("USE_FLAG", typeof(string));
                dtINDATA.Columns.Add("PACK_MIX_TYPE_CODE", typeof(string));
                dtINDATA.Columns.Add("BOX_LOT_SET_DATE", typeof(string));
                dtINDATA.Columns.Add("LOGIS_PACK_TYPE", typeof(string));
                dtINDATA.Columns.Add("INSUSER", typeof(string));
                dtINDATA.Columns.Add("INSDTTM", typeof(DateTime));
                dtINDATA.Columns.Add("UPDUSER", typeof(string));
                dtINDATA.Columns.Add("UPDDTTM", typeof(DateTime));
                dtINDATA.Columns.Add("IS_COMPLETED", typeof(bool));

                foreach (DataRowView drv in dt.AsDataView())
                {
                    DataRow drINDATA = dtINDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drINDATA["INPUT_MIX_TYPE_CODE"] = drv["INPUT_MIX_TYPE_CODE"].ToString();
                    drINDATA["PACK_EQPTID"] = drv["PACK_EQPTID"].ToString();
                    drINDATA["PRODID"] = drv["PRODID"].ToString();
                    drINDATA["INPUT_MIX_CHK_MTHD_CODE"] = drv["INPUT_MIX_CHK_MTHD_CODE"].ToString();
                    drINDATA["PROD_AREAID_LIST"] = string.IsNullOrEmpty(drv["PROD_AREAID_LIST"].ToString()) ? "ALL" : drv["PROD_AREAID_LIST"].ToString();
                    drINDATA["PROD_ELTR_LINE_LIST"] = string.IsNullOrEmpty(drv["PROD_ELTR_LINE_LIST"].ToString()) ? "ALL" : drv["PROD_ELTR_LINE_LIST"].ToString();
                    drINDATA["PROD_ASSY_LINE_LIST"] = string.IsNullOrEmpty(drv["PROD_ASSY_LINE_LIST"].ToString()) ? "ALL" : drv["PROD_ASSY_LINE_LIST"].ToString();
                    drINDATA["PROD_PACK_LINE_LIST"] = string.IsNullOrEmpty(drv["PROD_PACK_LINE_LIST"].ToString()) ? "ALL" : drv["PROD_PACK_LINE_LIST"].ToString();
                    drINDATA["USE_FLAG"] = drv["USE_FLAG"].ToString();
                    drINDATA["PACK_MIX_TYPE_CODE"] = string.IsNullOrEmpty(drv["PACK_MIX_TYPE_CODE"].ToString()) ? "ALL" : drv["PACK_MIX_TYPE_CODE"].ToString();
                    drINDATA["BOX_LOT_SET_DATE"] = string.IsNullOrEmpty(drv["BOX_LOT_SET_DATE"].ToString()) ? "ALL" : drv["BOX_LOT_SET_DATE"].ToString();
                    drINDATA["LOGIS_PACK_TYPE"] = string.IsNullOrEmpty(drv["LOGIS_PACK_TYPE"].ToString()) ? "PACK" : drv["LOGIS_PACK_TYPE"].ToString();
                    drINDATA["INSUSER"] = drv["INSUSER"].ToString();
                    drINDATA["UPDUSER"] = updUser;
                    drINDATA["IS_COMPLETED"] = isCompleted;
                    dtINDATA.Rows.Add(drINDATA);
                }
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, null, dsINDATA);
                if (!CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    Util.MessageInfo("SFU1270");        // 저장되었습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // BizRule 호출 - 반송 요청 이력 조회
        public DataTable GetModuleTransferHistoryList(object objPackEqptID, object objProdID, object objTrfReqStatusCode, string objTrfReqGenrateTypeCode)
        {
            string bizRuleName = "DA_PRD_SEL_LOGIS_TRF_REQ_HISTORY";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("FROM_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("TO_REQ_DATE", typeof(DateTime));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
            dtRQSTDT.Columns.Add("TRF_REQ_GNRT_TYPE_CODE", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["PRODID"] = string.IsNullOrEmpty(Util.NVC(objProdID)) ? null : Util.NVC(objProdID);
            drRQSTDT["TRF_REQ_STAT_CODE"] = objTrfReqStatusCode;
            drRQSTDT["TRF_REQ_GNRT_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(objTrfReqGenrateTypeCode)) ? null : Util.NVC(objTrfReqGenrateTypeCode);
            drRQSTDT["PACK_EQPTID"] = objPackEqptID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // BizRule 호출 - EIO Oper Mode 조회
        public DataTable GetEIOOperMode(string packEquipmentID)
        {
            string bizRuleName = "DA_BAS_SEL_EIOATTR_EQPT_OPER_MODE";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["EQPTID"] = packEquipmentID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // 조립Shop
        public DataTable GetAssyShopInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().GroupBy(x => new
            {
                SHOPID = x.Field<string>("SHOPID")
            }).Select(grp => new
            {
                CHK = false,
                SHOPID = grp.Key.SHOPID,
                SHOPNAME = grp.Key.SHOPID + " : " + grp.Max(x => x.Field<string>("SHOPNAME"))
            }).OrderBy(x => x.SHOPID);

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 전극Shop
        public DataTable GetElecShopInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                return null;
            }

            var query = this.dtElecLineInfo.AsEnumerable().GroupBy(x => new
            {
                SHOPID = x.Field<string>("SHOPID")
            }).Select(grp => new
            {
                CHK = false,
                SHOPID = grp.Key.SHOPID,
                SHOPNAME = grp.Key.SHOPID + " : " + grp.Max(x => x.Field<string>("SHOPNAME"))
            }).OrderBy(x => x.SHOPID);

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립동
        public DataTable GetAssyAreaInfo(IEnumerable<dynamic> lstShopID)
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().Where(x => lstShopID.OfType<DataRow>().Where(y => y.Field<string>("SHOPID").Equals(x.Field<string>("SHOPID"))).Any()).GroupBy(x => new
            {
                AREAID = x.Field<string>("AREAID")
            }).Select(grp => new
            {
                CHK = false,
                AREAID = grp.Key.AREAID,
                AREANAME = grp.Key.AREAID + " : " + grp.Max(x => x.Field<string>("AREANAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립동
        public void GetAssyAreaInfo(List<string> lstSelect, ref DataTable dtShopInfo, ref DataTable dtAreaInfo)
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                dtShopInfo = null;
                dtAreaInfo = null;
                return;
            }

            var baseInfo = (from d1 in this.dtAssyLineInfo.AsEnumerable()
                            join d2 in lstSelect on d1.Field<string>("AREAID") equals d2 into outerJoin
                            from d3 in outerJoin.DefaultIfEmpty()
                            select new
                            {
                                BASEDATA = d1,
                                SELECTEDDATA = d3
                            }).Select(x => new
                            {
                                SHOPCHK = x.SELECTEDDATA == null ? false : true,
                                AREACHK = x.SELECTEDDATA == null ? false : true,
                                SHOPID = x.BASEDATA.Field<string>("SHOPID"),
                                SHOPNAME = x.BASEDATA.Field<string>("SHOPNAME"),
                                AREAID = x.BASEDATA.Field<string>("AREAID"),
                                AREANAME = x.BASEDATA.Field<string>("AREANAME")
                            });
            DataTable dtBaseInfo = PackCommon.queryToDataTable(baseInfo.ToList());
            var shopInfo = baseInfo.GroupBy(x => x.SHOPID).Select(grp => new
            {
                CHK = grp.Max(x => x.SHOPCHK),
                SHOPID = grp.Key,
                SHOPNAME = grp.Key + " : " + grp.Max(x => x.SHOPNAME)
            });
            dtShopInfo = PackCommon.queryToDataTable(shopInfo.ToList());

            var areaInfo = from d1 in baseInfo
                           join d2 in shopInfo on d1.SHOPID equals d2.SHOPID
                           where d2.CHK
                           group d1 by d1.AREAID into grp
                           select new
                           {
                               CHK = grp.Max(x => x.AREACHK),
                               AREAID = grp.Key,
                               AREANAME = grp.Key + " : " + grp.Max(x => x.AREANAME)
                           };
            dtAreaInfo = PackCommon.queryToDataTable(areaInfo.ToList());
        }

        // 전극동
        public DataTable GetElecAreaInfo(IEnumerable<dynamic> lstShopID)
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                return null;
            }

            var query = this.dtElecLineInfo.AsEnumerable().Where(x => lstShopID.OfType<DataRow>().Where(y => y.Field<string>("SHOPID").Equals(x.Field<string>("SHOPID"))).Any()).GroupBy(x => new
            {
                AREAID = x.Field<string>("AREAID")
            }).Select(grp => new
            {
                CHK = false,
                AREAID = grp.Key.AREAID,
                AREANAME = grp.Key.AREAID + " : " + grp.Max(x => x.Field<string>("AREANAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립설비
        public DataTable GetAssyEquipmentInfo(IEnumerable<dynamic> lstAreaID)
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                return null;
            }

            var query = this.dtAssyLineInfo.AsEnumerable().Where(x => lstAreaID.OfType<DataRow>().Where(y => y.Field<string>("AREAID").Equals(x.Field<string>("AREAID"))).Any()).GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                CHK = false,
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 조립설비
        public void GetAssyEquipmentInfo(List<string> lstSelect, ref DataTable dtShopInfo, ref DataTable dtAreaInfo, ref DataTable dtEqptInfo)
        {
            if (!CommonVerify.HasTableRow(this.dtAssyLineInfo))
            {
                dtShopInfo = null;
                dtAreaInfo = null;
                dtEqptInfo = null;
                return;
            }

            var baseInfo = (from d1 in this.dtAssyLineInfo.AsEnumerable()
                            join d2 in lstSelect on d1.Field<string>("EQPTID") equals d2 into outerJoin
                            from d3 in outerJoin.DefaultIfEmpty()
                            select new
                            {
                                BASEDATA = d1,
                                SELECTEDDATA = d3
                            }).Select(x => new
                            {
                                SHOPCHK = x.SELECTEDDATA == null ? false : true,
                                AREACHK = x.SELECTEDDATA == null ? false : true,
                                EQPTCHK = x.SELECTEDDATA == null ? false : true,
                                SHOPID = x.BASEDATA.Field<string>("SHOPID"),
                                SHOPNAME = x.BASEDATA.Field<string>("SHOPNAME"),
                                AREAID = x.BASEDATA.Field<string>("AREAID"),
                                AREANAME = x.BASEDATA.Field<string>("AREANAME"),
                                EQPTID = x.BASEDATA.Field<string>("EQPTID"),
                                EQPTNAME = x.BASEDATA.Field<string>("EQPTNAME")
                            });

            var shopInfo = baseInfo.GroupBy(x => x.SHOPID).Select(grp => new
            {
                CHK = grp.Max(x => x.SHOPCHK),
                SHOPID = grp.Key,
                SHOPNAME = grp.Key + " : " + grp.Max(x => x.SHOPNAME)
            });
            dtShopInfo = PackCommon.queryToDataTable(shopInfo.ToList());

            var areaInfo = from d1 in baseInfo
                           join d2 in shopInfo on d1.SHOPID equals d2.SHOPID
                           where d2.CHK
                           group d1 by d1.AREAID into grp
                           select new
                           {
                               CHK = grp.Max(x => x.AREACHK),
                               AREAID = grp.Key,
                               AREANAME = grp.Key + " : " + grp.Max(x => x.AREANAME)
                           };
            dtAreaInfo = PackCommon.queryToDataTable(areaInfo.ToList());

            var eqptInfo = from d1 in baseInfo
                           join d2 in areaInfo on d1.AREAID equals d2.AREAID
                           where d2.CHK
                           group d1 by d1.EQPTID into grp
                           select new
                           {
                               CHK = grp.Max(x => x.EQPTCHK),
                               EQPTID = grp.Key,
                               EQPTNAME = grp.Key + " : " + grp.Max(x => x.EQPTNAME)
                           };
            dtEqptInfo = PackCommon.queryToDataTable(eqptInfo.ToList());
        }

        // 전극설비
        public DataTable GetElecEquipmentInfo(IEnumerable<dynamic> lstAreaID)
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                return null;
            }

            var query = this.dtElecLineInfo.AsEnumerable().Where(x => lstAreaID.OfType<DataRow>().Where(y => y.Field<string>("AREAID").Equals(x.Field<string>("AREAID"))).Any()).GroupBy(x => new
            {
                EQPTID = x.Field<string>("EQPTID")
            }).Select(grp => new
            {
                CHK = false,
                EQPTID = grp.Key.EQPTID,
                EQPTNAME = grp.Key.EQPTID + " : " + grp.Max(x => x.Field<string>("EQPTNAME"))
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 전극설비
        public void GetElecEquipmentInfo(List<string> lstSelect, ref DataTable dtShopInfo, ref DataTable dtAreaInfo, ref DataTable dtEqptInfo)
        {
            if (!CommonVerify.HasTableRow(this.dtElecLineInfo))
            {
                dtShopInfo = null;
                dtAreaInfo = null;
                dtEqptInfo = null;
                return;
            }

            var baseInfo = (from d1 in this.dtElecLineInfo.AsEnumerable()
                            join d2 in lstSelect on d1.Field<string>("EQPTID") equals d2 into outerJoin
                            from d3 in outerJoin.DefaultIfEmpty()
                            select new
                            {
                                BASEDATA = d1,
                                SELECTEDDATA = d3
                            }).Select(x => new
                            {
                                SHOPCHK = x.SELECTEDDATA == null ? false : true,
                                AREACHK = x.SELECTEDDATA == null ? false : true,
                                EQPTCHK = x.SELECTEDDATA == null ? false : true,
                                SHOPID = x.BASEDATA.Field<string>("SHOPID"),
                                SHOPNAME = x.BASEDATA.Field<string>("SHOPNAME"),
                                AREAID = x.BASEDATA.Field<string>("AREAID"),
                                AREANAME = x.BASEDATA.Field<string>("AREANAME"),
                                EQPTID = x.BASEDATA.Field<string>("EQPTID"),
                                EQPTNAME = x.BASEDATA.Field<string>("EQPTNAME")
                            });

            var shopInfo = baseInfo.GroupBy(x => x.SHOPID).Select(grp => new
            {
                CHK = grp.Max(x => x.SHOPCHK),
                SHOPID = grp.Key,
                SHOPNAME = grp.Key + " : " + grp.Max(x => x.SHOPNAME)
            });
            dtShopInfo = PackCommon.queryToDataTable(shopInfo.ToList());

            var areaInfo = from d1 in baseInfo
                           join d2 in shopInfo on d1.SHOPID equals d2.SHOPID
                           where d2.CHK
                           group d1 by d1.AREAID into grp
                           select new
                           {
                               CHK = grp.Max(x => x.AREACHK),
                               AREAID = grp.Key,
                               AREANAME = grp.Key + " : " + grp.Max(x => x.AREANAME)
                           };
            dtAreaInfo = PackCommon.queryToDataTable(areaInfo.ToList());

            var eqptInfo = from d1 in baseInfo
                           join d2 in areaInfo on d1.AREAID equals d2.AREAID
                           where d2.CHK
                           group d1 by d1.EQPTID into grp
                           select new
                           {
                               CHK = grp.Max(x => x.EQPTCHK),
                               EQPTID = grp.Key,
                               EQPTNAME = grp.Key + " : " + grp.Max(x => x.EQPTNAME)
                           };
            dtEqptInfo = PackCommon.queryToDataTable(eqptInfo.ToList());
        }

        // Pack Line
        public DataTable GetPackLineInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackLineInfo))
            {
                return null;
            }

            var query = this.dtPackLineInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                EQSGID = x.Field<string>("CBO_CODE"),
                EQSGNAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // Pack Line
        public DataTable GetPackLineInfo(List<string> lstSelect)
        {
            DataTable dt = this.GetPackLineInfo();

            var query = (from d1 in dt.AsEnumerable()
                         join d2 in lstSelect on d1.Field<string>("EQSGID") equals d2 into outerJoin
                         from d3 in outerJoin.DefaultIfEmpty()
                         select new
                         {
                             BASEDATA = d1,
                             SELECTEDDATA = d3
                         }).Select(x => new
                         {
                             CHK = x.SELECTEDDATA == null ? false : true,
                             EQSGID = x.BASEDATA.Field<string>("EQSGID"),
                             EQSGNAME = x.BASEDATA.Field<string>("EQSGNAME")
                         });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 포장기
        public DataTable GetPackEquipmentInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackEquipmentInfo))
            {
                return null;
            }

            var query = this.dtPackEquipmentInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_EQPTID = x.Field<string>("CBO_CODE"),
                PACK_EQPTNAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 포장기
        public DataTable GetPackEquipmentInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetPackEquipmentInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("PACK_EQPTID") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 PACK_EQPTID = x.BASEDATA.Field<string>("PACK_EQPTID"),
                                 PACK_EQPTNAME = x.BASEDATA.Field<string>("PACK_EQPTNAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("PACK_EQPTID"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // 제품
        public DataTable GetProductInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtProductInfo))
            {
                return null;
            }

            var query = this.dtProductInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PRODID = x.Field<string>("PRODID"),
                PRODNAME = x.Field<string>("PRODID") + " : " + x.Field<string>("PRODNAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // 제품
        public DataTable GetProductInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetProductInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("PRODID") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 PRODID = x.BASEDATA.Field<string>("PRODID"),
                                 PRODNAME = x.BASEDATA.Field<string>("PRODNAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("PRODID"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // Use Flag
        public DataTable GetUseFlagInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtUseFlagInfo))
            {
                return null;
            }

            var query = this.dtUseFlagInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                USE_FLAG = x.Field<string>("CBO_CODE"),
                USE_FLAG_NAME = x.Field<string>("CBO_NAME")
            });


            return PackCommon.queryToDataTable(query.ToList());
        }

        // Use Flag
        public DataTable GetUseFlagInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetUseFlagInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("USE_FLAG") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 USE_FLAG = x.BASEDATA.Field<string>("USE_FLAG"),
                                 USE_FLAG_NAME = x.BASEDATA.Field<string>("USE_FLAG_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("USE_FLAG"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // Auto Transfer Flag
        public DataTable GetAutoTransferFlagInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtAutoTransferFlagInfo))
            {
                return null;
            }

            var query = this.dtAutoTransferFlagInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                AUTO_TRF_FLAG = x.Field<string>("CBO_CODE"),
                AUTO_TRF_FLAG_NAME = x.Field<string>("CBO_NAME")
            });


            return PackCommon.queryToDataTable(query.ToList());
        }

        // Auto Transfer Flag
        public DataTable GetAutoTransferFlagInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetAutoTransferFlagInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("AUTO_TRF_FLAG") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 AUTO_TRF_FLAG = x.BASEDATA.Field<string>("AUTO_TRF_FLAG"),
                                 AUTO_TRF_FLAG_NAME = x.BASEDATA.Field<string>("AUTO_TRF_FLAG_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("AUTO_TRF_FLAG"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // CheckAllow, CheckForbidden
        public DataTable GetInputMixCheckMethodInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtInputMixCheckMethodInfo))
            {
                return null;
            }

            var query = this.dtInputMixCheckMethodInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                INPUT_MIX_CHK_MTHD_CODE = x.Field<string>("CBO_CODE"),
                INPUT_MIX_CHK_MTHD_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        // CheckAllow, CheckForbidden
        public DataTable GetInputMixCheckMethodInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetInputMixCheckMethodInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("INPUT_MIX_CHK_MTHD_CODE") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 INPUT_MIX_CHK_MTHD_CODE = x.BASEDATA.Field<string>("INPUT_MIX_CHK_MTHD_CODE"),
                                 INPUT_MIX_CHK_MTHD_CODE_NAME = x.BASEDATA.Field<string>("INPUT_MIX_CHK_MTHD_CODE_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("INPUT_MIX_CHK_MTHD_CODE"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // 포장혼입유형코드
        public DataTable GetPackMixTypeCodeInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtPackMixTypeCodeInfo))
            {
                return null;
            }

            var query = this.dtPackMixTypeCodeInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                PACK_MIX_TYPE_CODE = x.Field<string>("CBO_CODE"),
                PACK_MIX_TYPE_CODE_NAME = x.Field<string>("CBO_NAME")
            });

            DataTable dt = PackCommon.queryToDataTable(query.ToList());

            // All값 집어넣기
            DataRow dr = dt.NewRow();
            dr["CHK"] = false;
            dr["PACK_MIX_TYPE_CODE"] = "ALL";
            dr["PACK_MIX_TYPE_CODE_NAME"] = "ALL";
            dt.Rows.InsertAt(dr, 0);
            dt.AcceptChanges();
            return dt;
        }

        // 포장혼입유형코드
        public DataTable GetPackMixTypeCodeInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetPackMixTypeCodeInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("PACK_MIX_TYPE_CODE") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 PACK_MIX_TYPE_CODE = x.BASEDATA.Field<string>("PACK_MIX_TYPE_CODE"),
                                 PACK_MIX_TYPE_CODE_NAME = x.BASEDATA.Field<string>("PACK_MIX_TYPE_CODE_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("PACK_MIX_TYPE_CODE"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // 물류포장유형
        public DataTable GetLogisPackType()
        {
            if (!CommonVerify.HasTableRow(this.dtLogisPackType))
            {
                return null;
            }

            var query = this.dtLogisPackType.AsEnumerable().Select(x => new
            {
                CHK = false,
                LOGIS_PACK_TYPE = x.Field<string>("CBO_CODE"),
                LOGIS_PACK_TYPE_NAME = x.Field<string>("CBO_NAME")
            });

            DataTable dt = PackCommon.queryToDataTable(query.ToList());
            return dt;
        }

        // 물류포장유형
        public DataTable GetLogisPackType(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetLogisPackType();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("LOGIS_PACK_TYPE") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 LOGIS_PACK_TYPE = x.BASEDATA.Field<string>("LOGIS_PACK_TYPE"),
                                 LOGIS_PACK_TYPE_NAME = x.BASEDATA.Field<string>("LOGIS_PACK_TYPE_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("LOGIS_PACK_TYPE"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        // 포장기 반송설정 정보 조회 (투입 및 혼입 설정 정보)
        public DataTable GetPackTransferMixSet(C1DataGrid c1DataGrid, object objInputMixTypeCode, object packEquipmentID, object productID)
        {
            DataTable dt = new DataTable();

            // 포장기 설정 그리드에 선택된 값이 있으면 그 포장기로 조회
            List<string> lstSelectedPackEquipment = DataTableConverter.Convert(c1DataGrid.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK") && x.Field<string>("PACK_EQPTID") != null).Select(x => x.Field<string>("PACK_EQPTID")).ToList();
            if (lstSelectedPackEquipment.Count() > 0)
            {
                packEquipmentID = string.Join(",", lstSelectedPackEquipment);
            }

            dt = this.GetPackTransferMixSet(objInputMixTypeCode, packEquipmentID, productID);

            return dt;
        }
        #endregion

       
    }
}