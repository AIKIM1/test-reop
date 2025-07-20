using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace LGC.GMES.MES.ASSY002
{
    /// <summary>
    /// ASSY002_005_WAITING_HALF_PRODUCT_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY002_005_WAITING_HALF_PRODUCT_SEARCH : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        private DataTable _selectDt = null;
        public DataRow _selectRow = null;
        private string _procID = string.Empty;           // 공정코드
        private string _eqsgID = string.Empty;           // Line
        private string _trayID = string.Empty;           // TrayID
        private string _lotID = string.Empty;            // LOTID
        private string _windingRuncardID = string.Empty; // 이력카드ID

        #endregion

        public DataRow SELECTEDROW
        {
            get { return _selectRow; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY002_005_WAITING_HALF_PRODUCT_SEARCH()
        {
            InitializeComponent();
        }

        private void InitControl()
        {
            // INIT TEXTBOX
            if (!string.IsNullOrEmpty(_trayID))
            {
                txtTrayId.Text = _trayID;
                txtTrayId.Focus();
                txtTrayId.SelectAll();
            }

            CommonCombo _combo = new CommonCombo();

            const string gubun = "CS"; // 초소형, 원각 : CR

            // Combo
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, gubun };
            _combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter1); // Line
            cboEquipmentSegmentAssy.SelectedValue = _eqsgID;
        }

        private void ASSY002_005_WAITING_HALF_PRODUCT_SEARCH_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqsgID = tmps[1] as string;
            _trayID = tmps[2] as string;

            // Control 초기화
            InitControl();

            // SET DATA
            GetSearch();

            this.BringToFront();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtTrayId.Text.Length != 0 && txtTrayId.Text.Length < 3)
            {
                Util.AlertInfo("TRAYID는 3자리 이상 입력하세요.");
                return;
            }

            // SEARCH
            GetSearch(); // 초소형 or 원각 그리드 조회
        }

        private void GetSearch()
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();
                
                Util.gridClear(dgInputProduct);

                string sBizName = string.Empty;

                sBizName = "DA_PRD_SEL_WINDING_TRAY_WAIT_LIST"; // 초소형

                DataTable inDataTable = new DataTable();  // InputItems
                DataRow inDataRow = inDataTable.NewRow(); // InputItems Data

                inDataTable.Columns.Add("LANGID", typeof(string)); // 다국어
                inDataTable.Columns.Add("PROCID", typeof(string)); // 공정코드
                inDataTable.Columns.Add("EQSGID", typeof(string)); // Line
                inDataTable.Columns.Add("TRAYID", typeof(string)); // TrayID
                inDataTable.Columns.Add("WINDING_RUNCARD_ID", typeof(string));  // 이력카드ID

                inDataRow["LANGID"] = LoginInfo.LANGID;          // 다국어
                inDataRow["PROCID"] = Util.NVC(_procID);         // 공정코드
                inDataRow["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;     // Line
                inDataRow["TRAYID"] = Util.NVC(txtTrayId.Text);  // TrayID
                //inDataRow["WINDING_RUNCARD_ID"] = Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "WINDING_RUNCARD_ID")); 
                inDataRow["WINDING_RUNCARD_ID"] = dgInputProduct.Rows.Count == 0 ? string.Empty : Util.NVC(DataTableConverter.GetValue(dgInputProduct.Rows[0].DataItem, "WINDING_RUNCARD_ID")); // 이력카드ID

                inDataTable.Rows.Add(inDataRow);

                _selectDt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", inDataTable);

                if (_selectDt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }

                _selectDt.Columns.Add("CONTENTFLAG");
                _selectDt.Columns.Add("FOREGROUNDFLAG");

                for (int i = 0; i < _selectDt.Rows.Count; i++)
                {
                    _selectDt.Rows[i]["CONTENTFLAG"] = "수정";
                    _selectDt.Rows[i]["FOREGROUNDFLAG"] = "Black";
                }

                Util.GridSetData(dgInputProduct, _selectDt, FrameOperation, false); // 초소형
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
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

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            DataRow[] dr = null;

            if (dgInputProduct.Rows.Count == 0)
            {
                //Util.AlertInfo("선택된 항목이 없습니다.");
                Util.MessageValidation("SFU1651");
                return;
            }

            if (_selectDt != null)
            {
                for (int i = 0; i < _selectDt.Rows.Count; i++)
                {
                    if (string.Equals(_selectDt.Rows[i]["CHK"].ToString(), "True"))
                    {
                        _selectDt.Rows[i]["CHK"] = 1;
                    }
                    else
                    {
                        _selectDt.Rows[i]["CHK"] = 0;
                    }
                }
            }

            dr = Util.gridGetChecked(ref dgInputProduct, "CHK");

            if (dr.Length == 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (dr == null || dr.Length < 1)
                _selectRow = null;
            else
                _selectRow = dr[0];

            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

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
                        dgInputProduct.SelectedIndex = idx;

                        _selectDt = dtRow.Table;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally { }
        }

        private void btnGridTraySearch_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 대기 반제품 조회(초소형) 그리드의 위치정보칼럼에 있는 수정 버튼 클릭 이벤트 : 개발중
        }

        private void dgInputProduct_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int rowIndex = 0;
                int colIndex = 0;

                if (e.Cell != null && e.Cell.Presenter != null && e.Cell.Presenter.Content != null && dgInputProduct.SelectedIndex >= 0)
                {
                    rowIndex = e.Cell.Row.Index;
                    colIndex = e.Cell.Column.Index;

                    int selectedIdx = dgInputProduct.SelectedIndex;

                    for (int row = 0; row < _selectDt.Rows.Count; row++)
                    {
                        if (string.Equals(_selectDt.Rows[row]["CHK"].ToString(), "True"))
                        {
                            if (string.Equals(_selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                            {
                                dgInputProduct.SelectedIndex = row;
                            }
                        }
                    }
                }
            }));
        }

        private void dgInputProduct_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;

                int selectedIdx = dgInputProduct.SelectedIndex;

                for (int row = 0; row < _selectDt.Rows.Count; row++)
                {
                    if (string.Equals(_selectDt.Rows[row]["CHK"].ToString(), "True"))
                    {
                        if (string.Equals(_selectDt.Rows[selectedIdx]["CHK"].ToString(), "False"))
                        {
                            dgInputProduct.SelectedIndex = row;
                        }
                    }
                }
            }));
        }
    }
}
