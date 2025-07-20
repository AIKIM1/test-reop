/*************************************************************************************
 Created Date : 2017.07.03
      Creator : 이대영D
   Decription : Winding(초소형) Winding Tray 위치 정보 수정
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.03   INS 이대영D : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
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

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_WINDING_TRAY_LOCATION_ADJUSTMENT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WINDING_TRAY_LOCATION_ADJUSTMENT : C1Window, IWorkArea
    {
        private readonly Util _util = new Util();
        UcBaseAssy _baseForm;
        BizDataSet _bizRule = new BizDataSet();

        private DataTable dt = null;

        private DataTable selectDt = null;
        private string prodID = string.Empty;
        private string trayID = string.Empty;
        private string sFlag = string.Empty;
        private string lineID = string.Empty;
        private string procID = string.Empty;
        private string eqptID = string.Empty;
        private string eqptName = string.Empty;
        private string lotID = string.Empty;
        private string outlotID = string.Empty;
        private string trayTag = string.Empty;
        private DataRow workOrder = null;
        private string wipqty2 = string.Empty;
        private string gubun = string.Empty;
        private bool isSmallType = false;

        public DataTable SELECTHALFPRODUCT
        {
            get { return selectDt; }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_WINDING_TRAY_LOCATION_ADJUSTMENT()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            if (!isSmallType) // 원각
                gubun = "CR";
            else // 초소형
                gubun = "CS";

            String[] sFilter = { LoginInfo.CFG_AREA_ID, gubun, procID };
            _combo.SetCombo(cboEquipmentSegmentAssy, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            cboEquipmentSegmentAssy.SelectedValue = lineID;
        }

        private void CMM_WINDING_TRAY_LOCATION_ADJUSTMENT_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            procID = tmps[0] as string;     
            lineID = tmps[1] as string;    
            eqptID = tmps[2] as string;     
            eqptName = tmps[3] as string;  
            lotID = tmps[4] as string;      
            outlotID = tmps[5] as string;
            trayID = tmps[6] as string;    
            trayTag = tmps[7] as string;
            workOrder = tmps[8] as DataRow;
            prodID = tmps[9] as string;   
            sFlag = tmps[10] as string;
            isSmallType = bool.Parse(Util.NVC(tmps[11]));

            txtTrayId.Text = trayID;

            // INIT COMBO
            InitCombo();
            // SET DATA
            GetHalfProductList();

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
            GetHalfProductList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetHalfProductList()
        {
            // SELECT HALF PRODUCT
            try
            {
                Util.gridClear(dgInputProduct);

                string sBizName = "";

                if (string.Equals(procID, Process.WINDING) || (string.Equals(procID, Process.ASSEMBLY)))
                    sBizName = "DA_PRD_SEL_WINDING_TRAY_WAIT_LIST";

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("TRAYID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = cboEquipmentSegmentAssy.SelectedValue;
                //Indata["PROCID"] = procID;
                Indata["PROCID"] = "A3000";

                Indata["TRAYID"] = Util.NVC(txtTrayId.Text.Trim());

                IndataTable.Rows.Add(Indata);

                dt = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "RSLTDT", IndataTable);

                if (dt.Rows.Count == 0)
                {
                    //Util.AlertInfo("조회 가능한 반제품 정보가 없습니다.");
                    Util.MessageValidation("SFU1901");
                    return;
                }

                dt.Columns.Add("CONTENTFLAG");
                dt.Columns.Add("FOREGROUNDFLAG");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["CONTENTFLAG"] = ObjectDic.Instance.GetObjectName("수정");
                    dt.Rows[i]["FOREGROUNDFLAG"] = "Black";
                }

                Util.GridSetData(dgInputProduct, dt, FrameOperation, false);
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

        private void InputHalfProduct_Closed(object sender, EventArgs e)
        {
            // HALF PRODUCT RESULT
            CMM_WINDING_TRAY_LOCATION_ADJUSTMENT _InputProduct = sender as CMM_WINDING_TRAY_LOCATION_ADJUSTMENT;    // Winding Tray 위치 조정

            if (_InputProduct.DialogResult == MessageBoxResult.OK)
                _baseForm.SetHalfProdAddGrid(_InputProduct.SELECTHALFPRODUCT);
        }

        private void TrayCellInfo_Closed(object sender, EventArgs e)
        {
            // HALF PRODUCT RESULT
            CMM_TRAY_CELL_INFO _InputProduct = sender as CMM_TRAY_CELL_INFO;

            if (_InputProduct.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void dgInputProduct_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (string.Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOCATION_NG")), "OK"))
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("White"));
                    else
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#ffc0cb"));
                }
            }));
        }

        private void btnGridTraySearch_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.DataGridCellPresenter presenter = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)((System.Windows.Input.MouseDevice)e.Device).Captured).Parent) as C1.WPF.DataGrid.DataGridCellPresenter;

            string sTrayID = string.Empty;
            string sTrayTag = string.Empty;
            int sTrayTagLength = 0;
            string sOutLotID = string.Empty;

            wipqty2 = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "WIPQTY2"));

            if (presenter == null)
                return;

            sTrayID = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "TRAYID"));
            sTrayTag = presenter.Content.ToString();
            sTrayTagLength = sTrayTag.Length;
            sTrayTag = sTrayTag.Substring(sTrayTagLength - 2);

            if (string.Equals(sTrayTag, "수정"))
            {
                sTrayTag = "U";
                sOutLotID = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "LOTID"));
            }

            GetTrayFormLoad(sTrayID, sTrayTag, sOutLotID);
        }
        
        private void GetTrayFormLoad(string sTrayID, string sTrayTag, string sOutlotID)
        {
            try
            {
                CMM_TRAY_CELL_INFO _trayCell = new CMM_TRAY_CELL_INFO();
                _trayCell.FrameOperation = FrameOperation;

                if (_trayCell != null)
                {
                    // SET PARAMETER
                    object[] parameters = new object[10];
                    parameters[0] = procID;
                    parameters[1] = lineID;
                    parameters[2] = eqptID;
                    parameters[3] = eqptName;
                    parameters[4] = lotID;
                    parameters[5] = sOutlotID;
                    parameters[6] = sTrayID;
                    parameters[7] = sTrayTag;
                    parameters[8] = workOrder;

                    int sIndex = wipqty2.IndexOf(".");
                    string resultWipqty2 = wipqty2.Substring(0, sIndex);
                    parameters[9] = resultWipqty2;

                    C1WindowExtension.SetParameters(_trayCell, parameters);

                    _trayCell.Closed += new EventHandler(TrayCellInfo_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => _trayCell.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // SEARCH
                GetHalfProductList();
            }
        }
    }
}
