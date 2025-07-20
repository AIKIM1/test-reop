/*************************************************************************************
 Created Date : 2017.06.30
      Creator : 이대영D
   Decription : Asssembly 대기반제품조회(초소형)
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.03   INS 이대영D : Initial Created.
   2017.08.07   신광희C 대기반제품 조회(초소형) 그리드 컬럼 수정 수정 팝업 호출 저장 후 재조회 처리 불필요한 이벤트 삭제 처리 
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
using LGC.GMES.MES.CMM001.Extensions;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// ASSY002_005_WAITING_HALF_PRODUCT_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        private DataTable _selectDt = null;
        public DataRow _selectRow = null;
        private string _procID = string.Empty;           // 공정코드
        private string _eqsgID = string.Empty;           // Line
        private string _eqptID = string.Empty;           // 설비코드
        private string _eqptName = string.Empty;         // 
        private string _trayID = string.Empty;           // TrayID
        private string _lotID = string.Empty;            // LOTID
        private string _windingRuncardID = string.Empty; // 이력카드ID
        private DataRow workOrder = null;
        private string wipqty2 = string.Empty;           // 수량
        private string SearchCheck = string.Empty; //조회여부 체크 Y : 엔터키 N: 버튼키
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

        public CMM_ASSY_CSH_WAITING_HALF_PRODUCT_SEARCH()
        {
            InitializeComponent();
        }

        private void InitControl()
        {

            // INIT TEXTBOX
            if (SearchCheck == "Y")
            {
                if (!string.IsNullOrEmpty(_trayID))
                {
                    txtTrayId.Text = _trayID;
                    txtTrayId.Focus();
                    txtTrayId.SelectAll();
                }
            }
            else
            {
                txtTrayId.Text = string.Empty;
            }

            CommonCombo combo = new CommonCombo();
            const string gubun = "CS"; // 초소형, 원각 : CR

            // Combo
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID, gubun, _procID };
            combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter1); // Line
            cboEquipmentSegmentAssy.SelectedValue = _eqsgID;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqsgID = tmps[1] as string;
            _trayID = tmps[2] as string;
            SearchCheck = tmps[3] as string; //조회여부 체크 Y : 엔터키 N: 버튼키
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
                Util.MessageValidation("SFU3623"); // TRAYID는 3자리 이상 입력하세요.
                return;
            }

            // SEARCH
            GetSearch();
        }

        private void GetSearch()
        {
            // SELECT HALF PRODUCT
            try
            {
                ShowLoadingIndicator();
                
                Util.gridClear(dgInputProduct);

                const string sBizName = "DA_PRD_SEL_WINDING_TRAY_WAIT_LIST"; // 초소형

                DataTable inDataTable = new DataTable();  // InputItems
                DataRow inDataRow = inDataTable.NewRow(); // InputItems Data

                inDataTable.Columns.Add("LANGID", typeof(string));             // 다국어
                inDataTable.Columns.Add("PROCID", typeof(string));             // 공정코드
                inDataTable.Columns.Add("EQSGID", typeof(string));             // Line
                inDataTable.Columns.Add("TRAYID", typeof(string));             // TrayID
                inDataTable.Columns.Add("WINDING_RUNCARD_ID", typeof(string)); // 이력카드ID

                inDataRow["LANGID"] = LoginInfo.LANGID;                        // 다국어
                inDataRow["PROCID"] = Util.NVC(_procID);                       // 공정코드
                inDataRow["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;   // Line
                inDataRow["TRAYID"] = Util.NVC(txtTrayId.Text);                // TrayID
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
                    _selectDt.Rows[i]["CONTENTFLAG"] = ObjectDic.Instance.GetObjectName("수정");
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
            try
            {
                if (!CommonVerify.HasDataGridRow(dgInputProduct))
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                int rowIndex = _Util.GetDataGridFirstRowIndexByCheck(dgInputProduct, "CHK", true);
                if (rowIndex < 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataRow[] dr = DataTableConverter.Convert(dgInputProduct.ItemsSource).Select("CHK = True");
                _selectRow = dr.Length < 1 ? null : dr[0];
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

                if (!CommonVerify.HasDataGridRow(dgInputProduct)) return;
                if (rb?.DataContext == null) return;

                if (rb.IsChecked != null)
                {
                    int previewrowIdx = _Util.GetDataGridFirstRowIndexByCheck(dgInputProduct, "CHK", true);

                    DataRowView drv = rb.DataContext as DataRowView;

                    if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                    {
                        if (drv.Row["LOCATION_NG"].GetString() == "NG")
                        {
                            if (previewrowIdx <= 0)
                            {
                                foreach (DataGridRow item in ((DataGridCellPresenter) rb.Parent).DataGrid.Rows)
                                {
                                    DataTableConverter.SetValue(item.DataItem, "CHK", false);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < ((DataGridCellPresenter) rb.Parent).DataGrid.Rows.Count; i++)
                                {
                                    DataTableConverter.SetValue(((DataGridCellPresenter) rb.Parent).DataGrid.Rows[i].DataItem, "CHK",previewrowIdx == i);
                                }
                                //row 색 바꾸기
                                dgInputProduct.SelectedIndex = previewrowIdx;
                            }
                        }
                        else
                        {
                            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

                            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                            {
                                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                            }
                            //row 색 바꾸기
                            dgInputProduct.SelectedIndex = idx;
                        }

                        /*
                        int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                        DataRow dtRow = (rb.DataContext as DataRowView).Row;

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgInputProduct.SelectedIndex = idx;
                        _selectDt = dtRow.Table;
                        */
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
            C1.WPF.DataGrid.DataGridCellPresenter presenter = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)((System.Windows.Input.MouseDevice)e.Device).Captured).Parent) as C1.WPF.DataGrid.DataGridCellPresenter;

            string sTrayID = string.Empty;
            string sTrayTag = string.Empty;
            int sTrayTagLength = 0;
            string sOutLotID = string.Empty;

            wipqty2 = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "WIPQTY2"));
            sTrayID = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "TRAYID"));

            sTrayTag = presenter.Content.ToString();
            sTrayTagLength = sTrayTag.Length;
            sTrayTag = sTrayTag.Substring(sTrayTagLength - 2);

            if (string.Equals(sTrayTag, ObjectDic.Instance.GetObjectName("수정")))
            {
                sTrayTag = "U";
                sOutLotID = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "LOTID"));
            }

            GetTrayFormLoad(sTrayID, sTrayTag, sOutLotID);
        }

        private void GetTrayFormLoad(string trayId, string trayTag, string outlotId)
        {
            try
            {
                CMM_TRAY_CELL_INFO popupCellInfo = new CMM_TRAY_CELL_INFO();
                popupCellInfo.FrameOperation = FrameOperation;

                // SET PARAMETER
                object[] parameters = new object[10];
                parameters[0] = _procID;
                parameters[1] = _eqsgID;
                parameters[2] = _eqptID;
                parameters[3] = _eqptName;
                parameters[4] = _lotID;
                parameters[5] = outlotId;
                parameters[6] = trayId;
                parameters[7] = trayTag;
                parameters[8] = workOrder;

                int sIndex = wipqty2.IndexOf(".", StringComparison.Ordinal);
                string resultWipqty2 = wipqty2.Substring(0, sIndex);
                parameters[9] = resultWipqty2;

                C1WindowExtension.SetParameters(popupCellInfo, parameters);

                popupCellInfo.Closed += new EventHandler(TrayCellInfo_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupCellInfo.ShowModal()));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TrayCellInfo_Closed(object sender, EventArgs e)
        {
            // HALF PRODUCT RESULT
            CMM_TRAY_CELL_INFO popup = sender as CMM_TRAY_CELL_INFO;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetSearch();
            }
        }



        private void txtTrayId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if (txtTrayId.Text.Length != 0 && txtTrayId.Text.Length < 3)
            //{
            //    Util.MessageValidation("SFU3623"); // TRAYID는 3자리 이상 입력하세요.
            //    return;
            //}

            //// SEARCH
            //GetSearch();
        }

        private void dgInputProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_NG")), "OK"))
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("White");
                        if (convertFromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)convertFromString);
                    }
                    else
                    {
                        var fromString = System.Windows.Media.ColorConverter.ConvertFromString("#ffc0cb");
                        if (fromString != null)
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)fromString);
                    }
                }
            }));
        }
    }
}
