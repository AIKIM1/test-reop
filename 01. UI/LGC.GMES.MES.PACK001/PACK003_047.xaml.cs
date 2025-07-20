/************************************************************************************
  Created Date : 2024.07.01
       Creator : 최평부
   Description : 포장기별 제품설정
 ------------------------------------------------------------------------------------
  [Change History]
    2024.07.01  최평부 : Initial Created (from 포장기설정 및 자동반송-PACK003_021xaml 복사 후 수정)    
    2024.11.08  Adira  :  화면 개선 및 라인 정보 매핑 기능 추가 
    2024.12.11  최평부 :  포장기 제품설정 validation 기능 추가 
    2025.01.16  최평부 :  bizRule return data 수집 방법 변경
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_047 : UserControl, IWorkArea
    {
        #region Member Variable Lists...
        private const string PACK_EQUIPMENT_SET = "PACK_EQUIPMENTSET";
        private const string PACK_EQSG_SET = "PACK_EQSGSET";

        private PACK003_047_DataHelper dataHelper = new PACK003_047_DataHelper();
        private List<GridProperty> lstGridElement = new List<GridProperty>();
        private List<C1DataGrid> lstGrid = new List<C1DataGrid>();

        //2024-11-08 Adira S
        Util _Util = new Util();
        public bool _MCSEditFlag;
        public bool _EqptEditFlag;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public PACK003_047()
        {
            InitializeComponent();
            PackCommon.SetPopupDraggable(this.popupBaseInfo, this.pnlTitleBaseInfo);
        }
        #endregion

        #region Member Function Lists...
        // Initialize
        private void Initialize()
        {
            this.SetTagControl();
            List<Button> listAuth = new List<Button>();
            listAuth.Add(this.btnSaveEquipment);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            PackCommon.SearchRowCount(ref this.txtPackEquipmentProductBaseSet, 0);
            this.SetComboBox();

            this.CreateGridProperty();
            this.CreateGrid();
        }

        // Initialize - Control에 Tag값 설정
        private void SetTagControl()
        {
            this.dgEquipment.Tag = PACK_EQUIPMENT_SET;
            this.cboPackEqptID1.Tag = PACK_EQUIPMENT_SET;
            this.btnSearch1.Tag = PACK_EQUIPMENT_SET;
            this.txtPackEquipmentProductBaseSet.Tag = PACK_EQUIPMENT_SET;
            this.btnPlus1.Tag = PACK_EQUIPMENT_SET + "|" + "ADD";
            this.btnMinus1.Tag = PACK_EQUIPMENT_SET + "|" + "MINUS";
            this.btnSaveEquipment.Tag = PACK_EQUIPMENT_SET;

            //2024-11-08 Adira S
            this.dgEQSG.Tag = PACK_EQSG_SET;
            this.btnSaveEQSG.Tag = PACK_EQSG_SET;
        }

        // Popup - Grid 속성 설정
        private void CreateGridProperty()
        {
            //2024-11-08 Adira S
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PACK_EQUIPMENT", DATA_PROPERTIES = "PACK_EQUIPMENT", CODE_COLUMN = "PACK_EQPTID", CODENAME_COLUMN = "PACK_EQPTNAME", LINKED_VALUEPATH_COLUMN = "PACK_EQPTID", LINKED_DISPLAYPATH_COLUMN = "PACK_EQPTID", GRID_NAME = "dgPackEqpt", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false, MAIN_EQPTNAME = "MAIN_EQPTNAME", LINK_MAIN_EQPTNAME = "MAIN_EQPTNAME" });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "PRODUCT", DATA_PROPERTIES = "PRODUCT", CODE_COLUMN = "PRODID", CODENAME_COLUMN = "PRODNAME", LINKED_VALUEPATH_COLUMN = "PRODID", LINKED_DISPLAYPATH_COLUMN = "PRODNAME", GRID_NAME = "dgProduct", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false, MAIN_EQPTNAME = "MAIN_EQPTNAME", LINK_MAIN_EQPTNAME = "MAIN_EQPTNAME" });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "USE_FLAG", DATA_PROPERTIES = "USE_FLAG", CODE_COLUMN = "USE_FLAG", CODENAME_COLUMN = "USE_FLAG_NAME", LINKED_VALUEPATH_COLUMN = "USE_FLAG", LINKED_DISPLAYPATH_COLUMN = "USE_FLAG_NAME", GRID_NAME = "dgUseFlag", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false, MAIN_EQPTNAME = "MAIN_EQPTNAME", LINK_MAIN_EQPTNAME = "MAIN_EQPTNAME" });
            this.lstGridElement.Add(new GridProperty() { CATEGORY = "MIX_FLAG", DATA_PROPERTIES = "MIX_FLAG", CODE_COLUMN = "MIX_FLAG", CODENAME_COLUMN = "MIX_FLAG_NAME", LINKED_VALUEPATH_COLUMN = "MIX_FLAG", LINKED_DISPLAYPATH_COLUMN = "MIX_FLAG_NAME", GRID_NAME = "dgMixFlag", IS_LASTGRID = true, ROW_INDEX = 3, COLUMN_INDEX = 1, MULTI_CHECK = false, MAIN_EQPTNAME = "MAIN_EQPTNAME", LINK_MAIN_EQPTNAME = "MAIN_EQPTNAME" });
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

                PackCommon.AddGridColumn(c1DataGrid, "TEXT", gridProperty.MAIN_EQPTNAME, false);

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

                       MAIN_EQPTNAME = GRID_PROPERTY.MAIN_EQPTNAME,
                       LINK_MAIN_EQPTNAME = GRID_PROPERTY.LINK_MAIN_EQPTNAME,

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
                if (dataRowView == null || !Convert.ToBoolean(dataRowView["CHK"]))
                {
                    return;
                }

                // 클릭질한 Row가 조회버튼 눌러서 가져온 DataRow라면 Key Column인 경우는 Popup 띄우지 못하게 하기
                if ((dataRowView.Row.RowState.Equals(DataRowState.Unchanged) || dataRowView.Row.RowState.Equals(DataRowState.Modified)) &&
                    c1DataGrid.Name.ToUpper().Equals("DGEQUIPMENT") &&
                    dataGridCell.Column.Name.Equals("PACK_EQPTID")
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
                    case "PRODNAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("CHOICE") + " - " + ObjectDic.Instance.GetObjectName("PRODID");
                        break;
                    case "PACK_EQPTNAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("CHOICE") + " - " + ObjectDic.Instance.GetObjectName("PACK_EQPTNAME");
                        break;
                    case "USE_FLAG_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("CHOICE") + " - " + ObjectDic.Instance.GetObjectName("USE_FLAG");
                        break;
                    case "MIX_FLAG_NAME":
                        this.txtPopupTitle.Text = ObjectDic.Instance.GetObjectName("CHOICE") + " - " + ObjectDic.Instance.GetObjectName("MIX_FLAG_NAME");//CHOICE : 선택, MIX_FLAG_NAME : MICA 물류 제품 혼입여부
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

        // Popup - Close Popup
        private void HidePopUp()
        {
            this.popupBaseInfo.IsOpen = false;
            this.popupBaseInfo.HorizontalOffset = 0;
            this.popupBaseInfo.VerticalOffset = 0;
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
                    if (item.CATEGORY.Equals("PRODUCT")) dt = this.dataHelper.GetProductInfo();                             // 제품 Grid
                    if (item.CATEGORY.Equals("PACK_EQUIPMENT")) dt = this.dataHelper.GetPackEquipmentInfo();                // 포장기 Grid
                    if (item.CATEGORY.Equals("USE_FLAG")) dt = this.dataHelper.GetUseFlagInfo();                            // 사용여부 Grid
                    if (item.CATEGORY.Equals("MIX_FLAG")) dt = this.dataHelper.GetMixFlagInfo();                            // MIX FLAG Grid
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

            if (gridCategory.Equals("PRODUCT")) this.DataBindProductInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("PACK_EQUIPMENT")) this.DataBindPackEquipmentInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("USE_FLAG")) this.DataBindUseFlagInfo(lstAddGrid, selectedCodeValue);
            if (gridCategory.Equals("MIX_FLAG")) this.DataBindMixFlagInfo(lstAddGrid, selectedCodeValue);
        }

        // Popup - Grid Data Binding 제품
        private void DataBindProductInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtProductInfo = this.dataHelper.GetProductInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("PRODUCT") && CommonVerify.HasTableRow(dtProductInfo))
                {
                    //2024-11-08 Adira S          
                    Util.GridSetData(item.GRID, dtProductInfo, FrameOperation, true);
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
                    //2024-11-08 Adira S
                    Util.GridSetData(item.GRID, dtPackEquipmentInfo, FrameOperation, true);
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

        // Popup - Grid Data Binding 포장기 혼입여부
        private void DataBindMixFlagInfo(IEnumerable<dynamic> lstAddGrid, List<string> selectedCodeValue)
        {
            DataTable dtMixFlagInfo = this.dataHelper.GetMixFlagInfo(selectedCodeValue);

            foreach (var item in lstAddGrid)
            {
                if (item.DATA_PROPERTIES.Contains("MIX_FLAG") && CommonVerify.HasTableRow(dtMixFlagInfo))
                {
                    Util.GridSetData(item.GRID, dtMixFlagInfo, FrameOperation);
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
                    this.SearchPackEquipmentProductBaseSet();
                    this.SearchPackEQSGBaseSet();
                    break;
                //2024-11-08 Adira S
                case PACK_EQSG_SET:
                    this.SearchPackEquipmentProductBaseSet();
                    this.SearchPackEQSGBaseSet();
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

                if (CommonVerify.HasTableRow(dt))
                {
                    PackCommon.SearchRowCount(ref this.txtPackEquipmentProductBaseSet, dt.Rows.Count);
                    Util.GridSetData(this.dgEquipment, dt, FrameOperation);
                }

                string[] sColumnName = new string[] { "MAIN_EQPTNAME" };
                _Util.SetDataGridMergeExtensionCol(dgEquipment, sColumnName, DataGridMergeMode.VERTICAL);
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

        private void SearchPackEQSGBaseSet()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                DataTable dt = this.dataHelper.GetEQSGInfo();

                Util.GridSetData(this.dgEQSG, dt, FrameOperation);
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
                //2024-11-08 Adira S
                case PACK_EQSG_SET:
                    this.SavePackLineSet(button);
                    break;
                //case TRANSFER_SET:
                //this.SaveTransferMixSet(button);
                //break;
                default:
                    break;
            }
        }

        // 포장기 설정 정보 저장
        private void SavePackEquipmentSet(Button button, DataTable dt, string messageID)
        {
            Util.MessageConfirm(messageID, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //2024-11-08 Adira S
                    if (this.dataHelper.SavePackEquipmentSet(dt, LoginInfo.USERID, _MCSEditFlag, _EqptEditFlag))
                    {
                        this.SearchProcess(button);
                    }
                }
            });
        }

        // MICA 포장기 설정 정보 저장  (반송 정보 완료 Update 포함)
        private void SavePackEquipmentSet_MICA(DataTable dtGridData, DataTable dtEIOOperMode)
        {
            string messageID = "SFU3533";

            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }

            this.SavePackEquipmentSet(this.btnSearch1, (DataTable)dtGridData, messageID);
        }

        // 포장기 설정 정보 저장
        private void SavePackEquipmentSet(Button button)
        {
            dgEquipment.EndEdit();

            // Declarations...
            string messageID = "SFU3533";
            string transferGenerteTypeCode = string.Empty;
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
                        //MIX_FLAG = x.Field<string>("MIX_FLAG"),
                        USE_FLAG = x.Field<string>("USE_FLAG")
                    }).Except(dtCheck.AsEnumerable().Select(y => new
                    {
                        PACK_EQPTID = y.Field<string>("PACK_EQPTID"),
                        PRODID = y.Field<string>("PRODID"),
                        //MIX_FLAG = y.Field<string>("MIX_FLAG"),
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
            //DataTable dtTransferRequest = new DataTable();
            //foreach (DataRowView drv in dtTarget.AsDataView())
            //{
            //    DataTable dt = this.dataHelper.GetModuleTransferHistoryList(drv["PACK_EQPTID"].ToString(), string.Empty, transferRequestStatusCode, transferGenerteTypeCode);
            //    if (CommonVerify.HasTableRow(dt))
            //    {
            //        // Schema Copy & Import Row & Commit
            //        if (dtTransferRequest.Columns.Count <= 0)
            //        {
            //            dtTransferRequest = dt.Clone();
            //        }
            //        foreach (DataRowView drvTransferRequest in dt.AsDataView())
            //        {
            //            dtTransferRequest.ImportRow(drvTransferRequest.Row);
            //        }
            //        dtTransferRequest.AcceptChanges();
            //    }
            //}

            // Step 3 : 자동 반송 요청정보가 없는 경우에는 그냥 저장.
            //if (!CommonVerify.HasTableRow(dtTransferRequest))
            //{
            messageID = "SFU3533";
            // ByPass Mode이면 Message 뿌려주기
            if (CommonVerify.HasTableRow(dtEIOOperMode))
            {
                foreach (DataRowView drvEIOOperMode in dtEIOOperMode.AsDataView())
                {
                    messageID = drvEIOOperMode["EQPT_OPER_MODE"].ToString().ToUpper().Equals("MANUAL") ? "SFU8394" : "SFU3533";
                }
            }

            //2024-11-08 Adira S
            dtGridData.TableName = "EQPT";
            this.SavePackEquipmentSet(button, dtGridData, messageID);
            return;
            //}

            // Step 4 : 포장기 설정의 중요 속성 데이터가 변경되었고, 반송 요청정보가 있는 경우에는 Popup 표출후에 누르는 버튼에 따라 포장기 설정 정보만 저장할지 둘다 저장할지 결정함.
            if (CommonVerify.HasTableRow(dtTarget))
            {
                //MICA 설비의 경우 무조건 종료 기능 추가
                if (chkEqsg(dtTarget.Rows[0]["PACK_EQPTID"].ToString()))
                {
                    this.SavePackEquipmentSet_MICA(dtGridData, dtEIOOperMode);
                }
                return;
            }
        }

        //2024-11-08 Adira S
        private void SavePackLineSet(Button button)
        {
            string messageID = "SFU3533";

            string PROD_PACK_LINE_LIST = null;

            var selectedEqsg = DataTableConverter.Convert(dgEQSG.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
            if (selectedEqsg.Any())
            {
                DataTable dtGridData = selectedEqsg.CopyToDataTable();
                PROD_PACK_LINE_LIST = string.Join(", ", dtGridData.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>("EQSGID")).Distinct().ToList());

            }

            var eqpt = DataTableConverter.Convert(dgEquipment.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK"));
            if (eqpt.Any())
            {
                DataTable dtGridDataEquipment = eqpt.CopyToDataTable();

                dtGridDataEquipment.Rows[0]["PROD_PACK_LINE_LIST"] = PROD_PACK_LINE_LIST;
                dtGridDataEquipment.TableName = "EQPT_EQSG";
                dtGridDataEquipment.AcceptChanges();

                this.SavePackEquipmentSet(button, dtGridDataEquipment, messageID);
            }
        }

        // Validation Check
        private bool ValidationCheckPackEquipmentProductBaseSet(C1DataGrid c1DataGrid)
        {
            if (c1DataGrid == null || c1DataGrid.ItemsSource == null || c1DataGrid.Rows.Count < 0)
            {
                Util.Alert("9059");     // 데이터를 조회 하십시오.
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
            ////MMD 메세지 신규 생성 완료
            //if (queryValidationCheck.Where(x => string.IsNullOrEmpty(x.Field<string>("MIX_FLAG"))).Count() > 0)
            //{
            //    Util.Alert("SFU9025");  // 혼입여부를 선택하세요.
            //    c1DataGrid.Focus();
            //    return false;
            //}

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

        // Popup에서 선택한 항목들을 호출한 Grid Cell에다가 집어넣기
        private void SelectItemProcess()
        {
            //2024-11-08 Adira S
            _EqptEditFlag = true;
            string resultCode = string.Empty;
            string resultCodeName = string.Empty;

            //최평부
            string resultMainEqptName = string.Empty;

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

                    //2024-11-08 Adira S
                    if (codeNameColumnName == "PRODNAME")
                        codeNameColumnName = "DISPLAY";
                    else if (codeNameColumnName == "PACK_EQPTNAME")
                        codeNameColumnName = "PACK_EQPTID";


                    //최평부
                    string mainEqptName = item.MAIN_EQPTNAME;


                    DataTable dt = DataTableConverter.Convert(item.GRID.ItemsSource);
                    resultCode = string.Join(",", dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>(codeColumnName)).ToList());
                    resultCodeName = string.Join(Environment.NewLine, dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>(codeNameColumnName)).ToList());

                    if (item.CATEGORY.Equals("PACK_EQUIPMENT"))
                    {
                        //최평부
                        resultMainEqptName = string.Join(Environment.NewLine, dt.AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => x.Field<string>(mainEqptName)).ToList());
                    }

                    // Grid 사용자 정의 속성중에 MULTI_CHECK 여부가 FALSE인 것들은 부모그리드에 있는 값을 그대로 보존함.
                    if (string.IsNullOrEmpty(resultCode) && !Convert.ToBoolean(item.MULTI_CHECK))
                    {
                        continue;
                    }

                    if (this.ValidationCheckDup(c1DataGrid, dataGridCell.Row.Index, item.LINKED_VALUEPATH_COLUMN, resultCode))
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, resultCode);
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, resultCodeName);
                        //최평부
                        if (item.CATEGORY.Equals("PACK_EQUIPMENT"))
                        {
                            DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINK_MAIN_EQPTNAME, resultMainEqptName);
                        }
                    }
                    else
                    {
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_VALUEPATH_COLUMN, string.Empty);
                        DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINKED_DISPLAYPATH_COLUMN, string.Empty);
                        //최평부
                        if (item.CATEGORY.Equals("PACK_EQUIPMENT"))
                        {
                            DataTableConverter.SetValue(c1DataGrid.Rows[dataGridCell.Row.Index].DataItem, item.LINK_MAIN_EQPTNAME, resultMainEqptName);
                        }

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
                var query = DataTableConverter.Convert(this.dgEquipment.ItemsSource).AsEnumerable().Where(x => x.Field<bool>("CHK")).Select(x => new
                {
                    PACK_EQPTID = x.Field<string>("PACK_EQPTID"),
                    PRODID = x.Field<string>("PRODID")
                });

                if (query.Count() <= 0)
                {
                    return;
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.SearchProcess((Button)sender);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.SaveProcess((Button)sender);
        }

        //private void chkAll_Checked(object sender, RoutedEventArgs e)
        //{
        //    this.SetGridRowChecked((CheckBox)sender, true);
        //}

        //private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    this.SetGridRowChecked((CheckBox)sender, false);
        //}

        private void c1DataGrid_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            this.GridDataBinding((C1DataGrid)sender, e);
        }

        private void dgEquipment_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            try
            {
                e.Item.SetValue("CHK", true);

                foreach (DataRowView dataRowView in this.dataHelper.GetUseFlagInfo(new List<string> { "Y" }, false).AsDataView())
                {
                    e.Item.SetValue("USE_FLAG", dataRowView["USE_FLAG"]);
                    e.Item.SetValue("USE_FLAG_NAME", dataRowView["USE_FLAG_NAME"]);
                }

                //2024-11-08 Adira S
                //foreach (DataRowView dataRowView in this.dataHelper.GetMixFlagInfo(new List<string> { "N" }, false).AsDataView())
                //{
                //    e.Item.SetValue("MIX_FLAG", dataRowView["MIX_FLAG"]); //MIX FLAG 공통코드 USE_FLAG 사용
                //    e.Item.SetValue("MIX_FLAG_NAME", dataRowView["MIX_FLAG_NAME"]);
                //}

                e.Item.SetValue("INSUSER", LoginInfo.USERID);
                //e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                e.Item.SetValue("UPDUSER", LoginInfo.USERID);
                e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipment_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
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

        private void dgEquipment_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dataRowView = (DataRowView)e.Cell.Row.DataItem;

            //if (e.Cell.Column.Name.ToUpper().Equals("NEXT_REQ_BAS_QTY") ||
            //    e.Cell.Column.Name.ToUpper().Equals("WAIT_ENABLE_TIME") ||
            //    e.Cell.Column.Name.ToUpper().Equals("TRF_LOT_QTY"))
            //{
            //    return;
            //}

            //2024-11-12 Adira S
            if (e.Cell.Column.Name.ToUpper().Equals("MAX_TRF_QTY"))
            {
                _MCSEditFlag = true;
            }

            // 반송내역 조회시에 Check 표시가 해제되었다던가 신규 Row인 경우에는 조회안하게 함.
            if (!dataRowView.Row["CHK"].SafeToBoolean() ||
                dataRowView.Row.RowState.Equals(DataRowState.Detached) ||
                dataRowView.Row.RowState == DataRowState.Added)
            {
                return;
            }
        }

        private void dgEquipment_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "TRF_CMD_CNT")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEquipment_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell == null || datagrid.CurrentRow == null)
            {
                return;
            }

            //2024-11-08 Adira S
            if (cell != null)
            {
                if (cell.Column.Name == "TRF_CMD_CNT")
                {
                    //2024-11-18 Adira S
                    PACK003_047_TRF_CMD_STATUS_LIST wndPopup = new PACK003_047_TRF_CMD_STATUS_LIST();
                    wndPopup.FrameOperation = FrameOperation;

                    if (wndPopup != null)
                    {
                        object[] Parameters = new object[1];
                        string sPortid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["PORT_ID"].Index).Value);
                        Parameters[0] = sPortid;

                        C1WindowExtension.SetParameters(wndPopup, Parameters);

                        this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                    }
                }
                else
                {
                    this.ShowPopUp((C1DataGrid)sender, e);
                }
            }
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearProcess();
        }

        private void btnPlus1_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (this.dgEquipment.ItemsSource == null)
            {
                Util.Alert("9059");     // 데이터를 조회하십시오.
                return;
            }


            for (int i = 0; i <= this.dgEquipment.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(this.dgEquipment.Rows[i].DataItem, "CHK").SafeToBoolean())
                {
                    DataTableConverter.SetValue(this.dgEquipment.Rows[i].DataItem, "CHK", false);
                }
            }
            Util.DataGridRowAdd(this.dgEquipment, 1);
        }

        private void btnMinus1_Click(object sender, RoutedEventArgs e)
        {
            Util.DataGridRowDelete(this.dgEquipment, 1);
        }

        private void dgEquipmentCheckBoxColumn_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (this.dgEquipment.CurrentColumn == null || this.dgEquipment.CurrentRow == null)
                {
                    return;
                }
                if (!this.dgEquipment.CurrentColumn.Name.ToUpper().Equals("CHK"))
                {
                    return;
                }
                if (this.dgEquipment.GetRowCount() <= 0)
                {
                    return;
                }
                int currentRowIndex = this.dgEquipment.CurrentRow.Index;

                // 체크값이 True인 경우 다른 Row의 있는 내용은 False
                for (int i = 0; i < this.dgEquipment.GetRowCount(); i++)
                {
                    if (!i.Equals(currentRowIndex))
                    {
                        DataTableConverter.SetValue(this.dgEquipment.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        //2024-11-08 Adira S
        private void btnRadioEquipment_Checked(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)((DataGridCellPresenter)((sender as Control).Parent)).Row.DataItem;

            if (((DataGridCellPresenter)((sender as Control).Parent)).Column.Name.ToUpper().Equals("CHK"))
            {
                if (dataRowView.Row["CHK"].SafeToBoolean())
                {
                    List<string> lines = dataRowView.Row["PROD_PACK_LINE_LIST"].ToString().Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    setSelectedLine(lines);
                }
            }

            dgEquipment.EndEdit();
        }

        //2024-11-08 Adira S
        private void setSelectedLine(List<string> lines)
        {
            DataTable dt = DataTableConverter.Convert(dgEQSG.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;

                if (lines.IndexOf(dr["EQSGID"].ToString()) != -1)
                {
                    dr["CHK"] = true;
                }
            }

            dt.AcceptChanges();
            dgEQSG.ItemsSource = DataTableConverter.Convert(dt);
        }
    }

    public class PACK003_047_DataHelper
    {
        #region Member Variable Lists...
        private DataTable dtProductInfo = new DataTable();              // 제품
        private DataTable dtPackEquipmentInfo = new DataTable();        // 포장기
        private DataTable dtUseFlagInfo = new DataTable();              // Use Flag
        private DataTable dtMixFlagInfo = new DataTable();              // Mix Flag
        #endregion

        #region Constructor
        public PACK003_047_DataHelper()
        {
            this.GetProductInfo(ref this.dtProductInfo);
            this.GetPackEquipmentInfo(ref this.dtPackEquipmentInfo);
            this.GetCommonCodeInfo(ref this.dtUseFlagInfo, "USE_FLAG");
            this.GetCommonCodeInfo(ref this.dtMixFlagInfo, "USE_FLAG");//MIX_FLAG 공통코드 만들지 않고 USE_FLAG 꺼 사용
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

        //new
        //2024-08-17 최평부 추가 - 공통코드 조회
        public DataTable GetCommonCode(string codeType, string code)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = codeType;
                dr["CBO_CODE"] = code;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return new DataTable();
        }

        // BizRule 호출 - 포장기
        private void GetPackEquipmentInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_BAS_SEL_LOGIS_PACK_EQPT_PORT_CBO";
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

        // BizRule 호출 - 제품
        private void GetProductInfo(ref DataTable dtReturn)
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_SHOP_PRDT_PORT_ROUT_MODULE_CBO";
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
            string bizRuleName = "DA_PRD_SEL_PACK_EQPT_PRDT_PORT_MIX_SET_LIST";
            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));
            dtRQSTDT.Columns.Add("PRODID", typeof(string));
            //2024-11-08 Adira S
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow drRQSTDT = dtRQSTDT.NewRow();
            drRQSTDT["LANGID"] = LoginInfo.LANGID;
            drRQSTDT["PACK_EQPTID"] = string.IsNullOrEmpty(Util.NVC(objPackEquipment)) ? null : Util.NVC(objPackEquipment);
            //2024-11-08 Adira S
            drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRQSTDT.Rows.Add(drRQSTDT);

            return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        }

        // BizRule 호출 - 포장기 속성정보 저장
        //2024-11-08 Adira S
        public bool SavePackEquipmentSet(DataTable dt, string updUser, bool MCSEditFlag = false, bool EqptEditFlag = false)
        {
            bool returnValue = true;
            string bizRuleName = "BR_PRD_REG_PACK_EQPT_PRDT_MIX_BAS_SET";

            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));

            DataTable dtEQPT_PRDT = new DataTable("EQPT_PRDT");
            DataTable dtEQPT_EQSG = new DataTable("EQPT_EQSG");
            DataTable dtPORT_QTY = new DataTable("PORT_QTY");

            try
            {
                //2024-11-08 Adira S
                if (dt.TableName == "EQPT")
                {
                    if (EqptEditFlag)
                    {
                        dtEQPT_PRDT.Columns.Add("LANGID", typeof(string));
                        dtEQPT_PRDT.Columns.Add("PACK_EQPTID", typeof(string));
                        dtEQPT_PRDT.Columns.Add("PRODID", typeof(string));
                        //dtEQPT_PRDT.Columns.Add("MIX_FLAG", typeof(string));
                        dtEQPT_PRDT.Columns.Add("USE_FLAG", typeof(string));
                        dtEQPT_PRDT.Columns.Add("INSUSER", typeof(string));
                        //dtEQPT_PRDT.Columns.Add("INSDTTM", typeof(DateTime));
                        dtEQPT_PRDT.Columns.Add("UPDUSER", typeof(string));
                        dtEQPT_PRDT.Columns.Add("UPDDTTM", typeof(DateTime));

                        foreach (var item in query)
                        {
                            DataRow drINDATA = dtEQPT_PRDT.NewRow();
                            drINDATA["LANGID"] = LoginInfo.LANGID;
                            drINDATA["PACK_EQPTID"] = item.Field<string>("PACK_EQPTID");
                            drINDATA["PRODID"] = item.Field<string>("PRODID");
                            //drINDATA["MIX_FLAG"] = item.Field<string>("MIX_FLAG");
                            drINDATA["USE_FLAG"] = item.Field<string>("USE_FLAG");
                            drINDATA["INSUSER"] = item.Field<string>("INSUSER");
                            drINDATA["UPDUSER"] = updUser;

                            dtEQPT_PRDT.Rows.Add(drINDATA);
                        }

                    }

                    if (MCSEditFlag)
                    {
                        dtPORT_QTY.Columns.Add("MAX_TRF_QTY", typeof(string));
                        dtPORT_QTY.Columns.Add("PORT_ID", typeof(string));
                        dtPORT_QTY.Columns.Add("UPDUSER", typeof(string));
                        dtPORT_QTY.Columns.Add("POLARITY", typeof(string));
                        dtPORT_QTY.Columns.Add("BUF_QTY", typeof(string));

                        foreach (var item in query)
                        {
                            //2024-11-12 Adira S
                            if (!SaveValidation(dt)) return false;

                            DataRow newRow = dtPORT_QTY.NewRow();
                            newRow["PORT_ID"] = item.Field<string>("PORT_ID");
                            newRow["UPDUSER"] = updUser;
                            newRow["MAX_TRF_QTY"] = item.Field<string>("MAX_TRF_QTY"); //2025.01.22 형변환 오류 대응
                            newRow["POLARITY"] = "";
                            newRow["BUF_QTY"] = "";

                            dtPORT_QTY.Rows.Add(newRow);
                        }

                    }
                }
                if (dt.TableName == "EQPT_EQSG")
                {
                    dtEQPT_EQSG.Columns.Add("PROD_PACK_LINE_LIST", typeof(string));
                    dtEQPT_EQSG.Columns.Add("PACK_EQPTID", typeof(string));
                    dtEQPT_EQSG.Columns.Add("UPDUSER", typeof(string));

                    foreach (var item in query)
                    {
                        DataRow newRow = dtEQPT_EQSG.NewRow();
                        newRow["PROD_PACK_LINE_LIST"] = item.Field<string>("PROD_PACK_LINE_LIST");
                        newRow["PACK_EQPTID"] = item.Field<string>("PACK_EQPTID");
                        newRow["UPDUSER"] = updUser;

                        dtEQPT_EQSG.Rows.Add(newRow);
                    }

                }

                dsINDATA.Tables.Add(dtEQPT_PRDT);
                dsINDATA.Tables.Add(dtPORT_QTY);
                dsINDATA.Tables.Add(dtEQPT_EQSG);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());


                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, "OUTDATA", dsINDATA);
                if (!CommonVerify.HasTableInDataSet(dsOUTDATA))
                {
                    Util.MessageInfo("SFU1270");        // 저장되었습니다.

                }
                else
                {
                    //2025-01-16 최평부 dsOUTDATA.Tables[0]; >> dsOUTDATA.Tables["OUTDATA"]; 으로 수정
                    DataTable temp = dsOUTDATA.Tables["OUTDATA"];

                    if (temp.Rows.Count > 0)
                    {
                        if (temp.Rows[0]["RESULT"].ToString().Equals("N"))
                        {
                            Util.MessageInfo("SFU10032"); // 선택한 포장기는 진행중인 Batch 가 있어 제품변경이 불가합니다.
                        }
                    }
                    else
                    {
                        Util.MessageInfo("SFU1270");        // 저장되었습니다.
                    }
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
        //public DataTable GetModuleTransferHistoryList(object objPackEqptID, object objProdID, object objTrfReqStatusCode, string objTrfReqGenrateTypeCode)
        //{
        //    string bizRuleName = "DA_PRD_SEL_LOGIS_TRF_REQ_HISTORY";
        //    DataTable dtRQSTDT = new DataTable("RQSTDT");
        //    DataTable dtRSLTDT = new DataTable("RSLTDT");

        //    dtRQSTDT.Columns.Add("LANGID", typeof(string));
        //    dtRQSTDT.Columns.Add("FROM_REQ_DATE", typeof(DateTime));
        //    dtRQSTDT.Columns.Add("TO_REQ_DATE", typeof(DateTime));
        //    dtRQSTDT.Columns.Add("PRODID", typeof(string));
        //    dtRQSTDT.Columns.Add("TRF_REQ_STAT_CODE", typeof(string));
        //    dtRQSTDT.Columns.Add("TRF_REQ_NO", typeof(string));
        //    dtRQSTDT.Columns.Add("TRF_REQ_GNRT_TYPE_CODE", typeof(string));
        //    dtRQSTDT.Columns.Add("PACK_EQPTID", typeof(string));

        //    DataRow drRQSTDT = dtRQSTDT.NewRow();
        //    drRQSTDT["LANGID"] = LoginInfo.LANGID;
        //    drRQSTDT["PRODID"] = string.IsNullOrEmpty(Util.NVC(objProdID)) ? null : Util.NVC(objProdID);
        //    drRQSTDT["TRF_REQ_STAT_CODE"] = objTrfReqStatusCode;
        //    drRQSTDT["TRF_REQ_GNRT_TYPE_CODE"] = string.IsNullOrEmpty(Util.NVC(objTrfReqGenrateTypeCode)) ? null : Util.NVC(objTrfReqGenrateTypeCode);
        //    drRQSTDT["PACK_EQPTID"] = objPackEqptID;
        //    dtRQSTDT.Rows.Add(drRQSTDT);

        //    return new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
        //}

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
                PACK_EQPTNAME = x.Field<string>("CBO_NAME"),
                MAIN_EQPTNAME = x.Field<string>("MAIN_EQPTNAME")
            });

            return PackCommon.queryToDataTable(query.ToList());
        }

        //2024-11-12 Adira S
        public bool SaveValidation(DataTable dt)
        {
            var query = dt.AsEnumerable().Where(x => x.Field<bool>("CHK"));
            // Port가 극성을 가지는 경우 Buffer별 수량변경시 Max수량을 Buffer별 수량의 합으로 한다.
            Dictionary<string, int> dicPortMaxTrfQty = new Dictionary<string, int>();

            string strPortId = string.Empty;
            int nTransferQty = 0;

            foreach (var item in query)
            {
                strPortId = item.Field<string>("PORT_ID");

                nTransferQty = Util.NVC_Int(item.Field<string>("MAX_TRF_QTY"));  //2025.01.22 형변환 오류 대응

                if (dicPortMaxTrfQty.ContainsKey(strPortId))
                {
                    dicPortMaxTrfQty[strPortId] += nTransferQty;
                }
                else
                {
                    dicPortMaxTrfQty.Add(strPortId, nTransferQty);
                }
            }

            foreach (var item in query)
            {
                if (!item["MAX_SET_ENABLE_TRF_QTY"].IsNullOrEmpty())
                {
                    strPortId = item.Field<string>("PORT_ID");

                    if (dicPortMaxTrfQty.ContainsKey(strPortId))
                    {
                        if (dicPortMaxTrfQty[strPortId] > Util.NVC_Int(item.Field<string>("MAX_SET_ENABLE_TRF_QTY"))) //2025.01.22 형변환 오류 대응
                        {
                            object[] errMsgParam = new object[2];
                            errMsgParam[0] = item.Field<string>("MAX_SET_ENABLE_TRF_QTY"); //2025.01.22 형변환 오류 대응
                            errMsgParam[1] = strPortId.ToString();
                            Util.MessageValidation("100000210", errMsgParam);  //최대 설정 가능 반송 수량(%1)보다 변경값이 큽니다. \\nPORT ID[%2]
                            return false;
                        }
                    }
                }
            }
            return true;
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
                                 PACK_EQPTNAME = x.BASEDATA.Field<string>("PACK_EQPTNAME"),
                                 //최평부
                                 MAIN_EQPTNAME = x.BASEDATA.Field<string>("MAIN_EQPTNAME")
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
                //2024-11-08 Adira S
                PRODNAME = x.Field<string>("PRODID") + " (" + x.Field<string>("RVW_PRE_PARA_VALUE") + ") : " + x.Field<string>("PRODNAME"),
                DISPLAY = x.Field<string>("PRODID") + " (" + x.Field<string>("RVW_PRE_PARA_VALUE") + ")"
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
                                 PRODNAME = x.BASEDATA.Field<string>("PRODNAME"),
                                 //2024-11-08 Adira S
                                 DISPLAY = x.BASEDATA.Field<string>("DISPLAY")
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

        // Mix Flag
        public DataTable GetMixFlagInfo()
        {
            if (!CommonVerify.HasTableRow(this.dtMixFlagInfo))
            {
                return null;
            }

            var query = this.dtMixFlagInfo.AsEnumerable().Select(x => new
            {
                CHK = false,
                MIX_FLAG = x.Field<string>("CBO_CODE"),
                MIX_FLAG_NAME = x.Field<string>("CBO_NAME")
            });


            return PackCommon.queryToDataTable(query.ToList());
        }

        // MIX Flag
        public DataTable GetMixFlagInfo(List<string> lstSelect, bool isGridMapping = true)
        {
            DataTable dt = this.GetMixFlagInfo();
            DataTable dtReturn = new DataTable();

            if (isGridMapping)
            {
                var query = (from d1 in dt.AsEnumerable()
                             join d2 in lstSelect on d1.Field<string>("MIX_FLAG") equals d2 into outerJoin
                             from d3 in outerJoin.DefaultIfEmpty()
                             select new
                             {
                                 BASEDATA = d1,
                                 SELECTEDDATA = d3
                             }).Select(x => new
                             {
                                 CHK = x.SELECTEDDATA == null ? false : true,
                                 MIX_FLAG = x.BASEDATA.Field<string>("MIX_FLAG"),
                                 MIX_FLAG_NAME = x.BASEDATA.Field<string>("MIX_FLAG_NAME")
                             });

                dtReturn = PackCommon.queryToDataTable(query.ToList());
            }
            else
            {
                dtReturn = dt.AsEnumerable().Where(x => lstSelect.Where(y => y.Equals(x.Field<string>("MIX_FLAG"))).Any()).CopyToDataTable();
            }

            return dtReturn;
        }

        //2024-11-08 Adira S
        public DataTable GetEQSGInfo()
        {
            string bizRuleName = "DA_PRD_SEL_PACK_EQSG_WO_BAS_SET_LIST";

            DataTable dtRQSTDT = new DataTable("RQSTDT");

            dtRQSTDT.Columns.Add("LANGID", typeof(string));
            dtRQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;

            dtRQSTDT.Rows.Add(dr);

            DataTable dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, "RSLTDT", dtRQSTDT);

            if (CommonVerify.HasTableRow(dtRSLTDT))
            {
                return dtRSLTDT;
            }

            return null;
        }
        #endregion


    }
}