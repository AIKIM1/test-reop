/*************************************************************************************
 Created Date : 2017.07.01
      Creator : 이대영D
   Decription : Winding 공정진척(초소형) 대기Pancake조회
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.01   INS 이대영D : Initial Created.
   2019.03.22   INS 강민준C : IN LINE 정보 선택시 GRID 재정의 CheckInline()
   2024.02.22        백광영 : 설비 재작업 모드 -> 대기 반제품 조회 공정 AC003 적용
   2024.07.16        백광영 : 설비 재작업 모드 기능 제거
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Data;
using System.Diagnostics.Eventing.Reader;


namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private DataTable selectDt = null;
        public DataRow DrSelectedDataRow = null;
        private string woID = string.Empty;
        private string lineID = string.Empty;
        private string procID = string.Empty;  // 공정코드
        private string electordeType = string.Empty;
        private string limitCount = string.Empty;
        private string lotID = string.Empty;
        private string sFlag = string.Empty;
        private string eqptID = string.Empty;
        private string prodID = string.Empty;
        private string windingRuncardID = string.Empty;
        private string SearchCheck = string.Empty; //조회여부 체크 Y : 엔터키 N: 버튼키
        private string _equipmentCode = string.Empty;


        public string ELECTRODETYPE
        {
            get { return electordeType; }
        }

        public DataTable SELECTHALFPRODUCT
        {
            get { return selectDt; }
        }

        public DataRow SELECTEDROW
        {
            get { return DrSelectedDataRow; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
        
        public CMM_ASSY_WG_WAITING_HALF_PRODUCT_SEARCH()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            const string gubun = "CR"; // 초소형 : CS

            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, gubun, procID };
            _combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter1);
            cboEquipmentSegmentAssy.SelectedValue = lineID;

            //인라인정보 확인
            CheckInline(cboEquipmentSegmentAssy.SelectedValue.ToString());
        }

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;            // 공정코드
            lineID = tmps[1] as string;            // Line
            lotID = tmps[2] as string;             // LOT ID
            SearchCheck = tmps[3] as string;       //조회여부 체크 Y : 엔터키 N: 버튼키
            _equipmentCode = tmps[4] as string;
            //windingRuncardID = tmps[3] as string;  // 이력카드 ID

            // INIT COMBO
            InitCombo();

            //메뉴에서 호출하는 경우는 그리드의 라디오 버튼과 하단의 선택 버튼은 보여주지 않는다.
            if (string.Equals(sFlag, "A"))
            {
                rdoChk.Visibility = Visibility.Hidden;
                btnSelect.Visibility = Visibility.Hidden;
            }

            // INIT TEXTBOX
            if (SearchCheck == "Y") //조회여부 체크 Y : 엔터키 N: 버튼키
            {
                if (!string.IsNullOrEmpty(lotID))
                {
                    txtLotId.Text = lotID;
                    txtLotId.Focus();
                    txtLotId.SelectAll();
                }
            }
            else
            {
                txtLotId.Text = string.Empty;
            }

            if (string.Equals(procID, Process.WASHING))
            {
                gdInputProduct.Columns["WIP_TRAY"].Visibility = Visibility.Collapsed;
                StackEquipmentSegmentAssy.Visibility = Visibility.Collapsed;
            }

            //if (!string.IsNullOrEmpty(windingRuncardID))
            //    txtWindingRuncardId.Text = windingRuncardID;

            // SET DATA
            GetSearchGrid();

            this.BringToFront();
        }
        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtLotId.Text.Length != 0 && txtLotId.Text.Length < 3)
            {
                Util.MessageValidation("SFU3624"); // LOTID는 3자리 이상 입력하세요.
                return;
            }
            // SEARCH
            GetSearchGrid();

            //인라인정보 확인             
            CheckInline(cboEquipmentSegmentAssy.SelectedValue.ToString());
        }

        #region [선택]

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (gdInputProduct.Rows.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (selectDt != null)
            {
                for (int i = 0; i < selectDt.Rows.Count; i++)
                {
                    if (string.Equals(selectDt.Rows[i]["CHK"].ToString(), "True") || string.Equals(selectDt.Rows[i]["CHK"].ToString(), "1"))
                    {
                        selectDt.Rows[i]["CHK"] = 1;
                    }
                    else
                    {
                        selectDt.Rows[i]["CHK"] = 0;
                    }
                }
            }

            

            DataRow[] dr = Util.gridGetChecked(ref gdInputProduct, "CHK");

            if (dr.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (dr == null || dr.Length < 1)
                DrSelectedDataRow = null;
            else
                DrSelectedDataRow = dr[0];

            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region [닫기]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        private void GetSearchGrid()
        {
            // SELECT HALF PRODUCT
            try
            {
                Util.gridClear(gdInputProduct);

                string sBizName;
                if (string.Equals(procID, Process.WASHING))
                    sBizName = "DA_PRD_SEL_WASHING_RW_WAIT_LIST";
                else
                    sBizName = "DA_PRD_SEL_WINDING_LOT_WAIT_LIST";

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));

                if (string.Equals(procID, Process.WASHING))
                {
                    inDataTable.Columns.Add("EQPTID", typeof(string));
                }

                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataRow["PROCID"] = Util.NVC(procID);
                inDataRow["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;
                inDataRow["LOTID"] = Util.NVC(txtLotId.Text.Trim());
                if (string.Equals(procID, Process.WASHING))
                {
                    inDataRow["EQPTID"] = _equipmentCode;
                }
                    
                //inDataRow["WINDING_RUNCARD_ID"] = Util.NVC(txtWindingRuncardId.Text.Trim());

                inDataTable.Rows.Add(inDataRow);

                DataSet ds = new DataSet();
                inDataTable.TableName = "RQSTDT";
                ds.Tables.Add(inDataTable);
                
                selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inDataTable);

                if (selectDt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }
                
                Util.GridSetData(gdInputProduct, selectDt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Func]

        private void CheckInline(string strline)
        {
            const string bizRuleName = "DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT_LDR_FLAG";

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["EQSGID"] = strline;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            if (searchResult.Rows.Count > 0)
            {
                gdInputProduct.Columns["INLINE"].Visibility = Visibility.Visible;
            }
            else
            {
                gdInputProduct.Columns["INLINE"].Visibility = Visibility.Collapsed;
            }

        }
        #endregion

        #endregion

        private void rdoChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        gdInputProduct.SelectedIndex = idx;

                        selectDt = dtRow.Table;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        private void gdInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null && gdInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = gdInputProduct.SelectedIndex;

                    for (int row = 0; row < selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                        {
                            if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                            {
                                gdInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
        }

        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string inputLotId = txtLotId.Text.Trim();

                if (inputLotId.Length < 3)
                {
                    Util.MessageValidation("SFU3624"); // LOTID는 3자리 이상 입력하세요.
                    return;
                }
                // SEARCH
                GetSearchGrid();
            }
        }

        private void gdInputProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int selectedIdx = gdInputProduct.SelectedIndex;

                for (int row = 0; row < selectDt.Rows.Count; row++)
                {
                    if (string.Equals(selectDt.Rows[row]["CHK"].ToString(), "True"))
                    {
                        if (string.Equals(selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                        {
                            gdInputProduct.SelectedIndex = row;
                        }
                    }
                }
            }));
        }
    }
}
