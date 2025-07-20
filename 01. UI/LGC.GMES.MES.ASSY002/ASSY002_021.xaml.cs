/*************************************************************************************
 Created Date : 2017.02.22
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_021 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _util = new Util();
        public ASSY002_021()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine });
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboArea });
            //_combo.SetCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { _PROCID }, sCase: "cboEquipment");
            //_combo.SetCombo(cboWorkType_Search, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { string.Empty, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
            //_combo.SetCombo(cboInBox, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "INBOX_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            //_combo.SetCombo(cboWorkType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "PACK_WRK_TYPE_CODE1" }, sCase: "COMMCODE_WITHOUT_CODE");

            //SetGridColumnCombo(dgInbox, "PRDT_GRD_CODE", "PRDT_GRD_CODE");
            //SetGridColumnCombo(dgInbox, "PRDT_GRD_LEVEL", "PRDT_GRD_LEVEL");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Now;
        }        
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }        

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void dgSearchResult_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "FI_GAP"
                      && Util.NVC_Int(e.Cell.Text) != 0)
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : DA_PRD_SEL_INPALLET_FM
        /// </summary>
        private void Search()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(txtLotID.Text) && txtLotID.Text.Length < 8)
                {
                    //SFU4074  LOTID 8자리 이상 입력시 조회 가능합니다.
                    Util.MessageValidation("SFU4074", new object[] { 8 });
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;

                }

                if (Util.NVC(cboLine.SelectedValue) == "SELECT")
                {
                    //SFU1223	라인을 선택하세요.
                    Util.MessageValidation("SFU1223");
                    return;
                }

                    DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;               

                if (!string.IsNullOrWhiteSpace(txtLotID.Text))
                {
                    dr["LOTID"] = txtLotID.Text;
                }
                //else if (!string.IsNullOrWhiteSpace(txtProject.Text))
                //{
                //    dr["PRJT_NAME"] = txtProject.Text;
                //}
                else
                {
                    dr["AREAID"] = (string)cboArea.SelectedValue;
                    dr["EQSGID"] = (string)cboLine.SelectedValue;
                    dr["PRJT_NAME"] = string.IsNullOrWhiteSpace(txtProject.Text)? null : txtProject.Text;
                    //dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    //dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_FORM_MOVE", "RQSTDT", "RSLTDT", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                {
                    DataColumn dc = new DataColumn("CHK");
                    dc.DefaultValue = 0;
                    dtResult.Columns.Add(dc);
                }

                Util.GridSetData(dgSearchResult, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        #endregion
        
        private void btn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iSelRow = _util.GetDataGridCheckFirstRowIndex(dgSearchResult, "CHK");

                if (iSelRow < 0)
                {
                    // 선택된 LOT이 없습니다.
                    Util.MessageValidation("SFU1261");
                    return;
                }                

                ASSY002_021_TRAY_HIST NCR = new ASSY002_021_TRAY_HIST();
                NCR.FrameOperation = this.FrameOperation;

                object[] parameters = new object[5];
                parameters[0] = Util.NVC(dgSearchResult.GetCell(iSelRow, dgSearchResult.Columns["LOTID"].Index).Text);
                parameters[1] = Util.NVC(dgSearchResult.GetCell(iSelRow, dgSearchResult.Columns["CALDATE"].Index).Text);
                parameters[2] = Util.NVC(dgSearchResult.GetCell(iSelRow, dgSearchResult.Columns["EQSGNAME"].Index).Text);
                parameters[3] = Util.NVC(dgSearchResult.GetCell(iSelRow, dgSearchResult.Columns["PRJT_NAME"].Index).Text);
                parameters[4] = Util.NVC(dgSearchResult.GetCell(iSelRow, dgSearchResult.Columns["PRODID"].Index).Text);

                C1WindowExtension.SetParameters(NCR, parameters);

                grdMain.Children.Add(NCR);
                NCR.BringToFront();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                if (rb?.DataContext == null) return;

                if (rb.IsChecked == null)
                    return;

                DataRowView drv = rb.DataContext as DataRowView;

                if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
                {
                    int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                    DataRow dtRow = (rb.DataContext as DataRowView).Row;

                    for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                    {
                        DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                    }
                    dgSearchResult.SelectedIndex = idx;
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
    }
}
