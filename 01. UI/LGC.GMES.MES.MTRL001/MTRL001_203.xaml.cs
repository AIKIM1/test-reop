/*********************************************************************************************************************************
 Created Date : 2024.03.13
      Creator : 유명환
   Decription : 자재LOT 잔량재공 조회
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2024.03.13                               유명환           Initial Created.
***********************************************************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using System.Windows.Media;
using C1.WPF;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_203 : UserControl, IWorkArea
    {
        #region Declaration
        private CommonCombo combo = new CommonCombo();
        private Util _Util = new Util();

        private string CSTStatus = string.Empty;
        private string sMtrlid = "";

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        public MTRL001_203()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            cboMatlLossProcess.SelectedIndex = 0;

            this.Loaded -= UserControl_Loaded;
        }

        #region [ Event ] - Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SummaryData();
        }
        #endregion

        #region [ Method ] - Search
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaHisChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaHisChild, sCase: "AREA");

            //공정
            C1ComboBox[] cboProcessParent = { cboArea };
            C1ComboBox[] cboProcessChild = { cboEquipmentSegment };
            _combo.SetCombo(cboMatlLossProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea, cboMatlLossProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent, sCase: "cboMatlLossEquipmentSegment");

            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboMatlLossProcess.SelectedItemChanged += cboMatlLossProcess_SelectedItemChanged;            

            setGridHeader();
        }

        private void init()
        {
            Util.gridClear(dgSummary);
            Util.gridClear(dgDetail);
        }


        private void SummaryData()
        {
            try
            {
                init();
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                //Indata["MTRLTYPE"] = cboMTRLTYPE.SelectedValue.ToString(); // cboMTRLTYPE.SelectedValue;
                Indata["AREAID"] = cboArea.SelectedValue.ToString();

                if (!String.IsNullOrEmpty(txtMtrlID.Text))
                {
                    Indata["MTRLID"] = txtMtrlID.Text;
                }
                else
                {
                    if (!String.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()))
                        Indata["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                    if (!String.IsNullOrEmpty(cboMatlLossProcess.SelectedValue.ToString()))
                        Indata["PROCID"] = cboMatlLossProcess.SelectedValue.ToString();
                }

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_BAS_SEL_MLOT_WIP_REMAIN";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgSummary, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void DetailData(string MtrlID)
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = cboArea.SelectedValue.ToString();

                if (!String.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()))
                    Indata["EQSGID"] = cboEquipmentSegment.SelectedValue.ToString();

                if (!String.IsNullOrEmpty(cboMatlLossProcess.SelectedValue.ToString()))
                    Indata["PROCID"] = cboMatlLossProcess.SelectedValue.ToString();

                if (!String.IsNullOrEmpty(MtrlID))
                    Indata["MTRLID"] = MtrlID;

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_BAS_SEL_MLOT_WIP_REMAIN_DETAIL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgDetail, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void setGridHeader()
        {
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E"))
            {
                dgSummary.Columns["WAIT_LOT_WIP"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["WAIT_LOT_WIP_M"].Visibility = Visibility.Visible;
                dgSummary.Columns["WAIT_LOT_WIP_KG"].Visibility = Visibility.Visible;

                dgSummary.Columns["MOUNT_LOT_WIP"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["MOUNT_LOT_WIP_M"].Visibility = Visibility.Visible;
                dgSummary.Columns["MOUNT_LOT_WIP_KG"].Visibility = Visibility.Visible;

                dgSummary.Columns["PROC_LOT_WIP"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["PROC_LOT_WIP_M"].Visibility = Visibility.Visible;
                dgSummary.Columns["PROC_LOT_WIP_KG"].Visibility = Visibility.Visible;

                dgSummary.Columns["EQPT_END_LOT_WIP"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["EQPT_END_LOT_WIP_M"].Visibility = Visibility.Visible;
                dgSummary.Columns["EQPT_END_LOT_WIP_KG"].Visibility = Visibility.Visible;

                dgSummary.Columns["HOLD_LOT_WIP"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["HOLD_LOT_WIP_M"].Visibility = Visibility.Visible;
                dgSummary.Columns["HOLD_LOT_WIP_KG"].Visibility = Visibility.Visible;


                dgDetail.Columns["WIPQTY"].Visibility = Visibility.Visible;
                dgDetail.Columns["ORIG_WIPQTY"].Visibility = Visibility.Visible;
                dgDetail.Columns["WIPQTY_M"].Visibility = Visibility.Collapsed;

                dgDetail.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDetail.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDetail.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Collapsed;

                dgDetail.Columns["TCK"].Visibility = Visibility.Visible;
                dgDetail.Columns["WIDTH"].Visibility = Visibility.Visible;
                dgDetail.Columns["CONV_RATE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgSummary.Columns["WAIT_LOT_WIP"].Visibility = Visibility.Visible;
                dgSummary.Columns["WAIT_LOT_WIP_M"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["WAIT_LOT_WIP_KG"].Visibility = Visibility.Collapsed;

                dgSummary.Columns["MOUNT_LOT_WIP"].Visibility = Visibility.Visible;
                dgSummary.Columns["MOUNT_LOT_WIP_M"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["MOUNT_LOT_WIP_KG"].Visibility = Visibility.Collapsed;

                dgSummary.Columns["PROC_LOT_WIP"].Visibility = Visibility.Visible;
                dgSummary.Columns["PROC_LOT_WIP_M"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["PROC_LOT_WIP_KG"].Visibility = Visibility.Collapsed;

                dgSummary.Columns["EQPT_END_LOT_WIP"].Visibility = Visibility.Visible;
                dgSummary.Columns["EQPT_END_LOT_WIP_M"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["EQPT_END_LOT_WIP_KG"].Visibility = Visibility.Collapsed;

                dgSummary.Columns["HOLD_LOT_WIP"].Visibility = Visibility.Visible;
                dgSummary.Columns["HOLD_LOT_WIP_M"].Visibility = Visibility.Collapsed;
                dgSummary.Columns["HOLD_LOT_WIP_KG"].Visibility = Visibility.Collapsed;

                dgDetail.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["ORIG_WIPQTY"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["WIPQTY_M"].Visibility = Visibility.Visible;

                dgDetail.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Visible;

                dgDetail.Columns["TCK"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["WIDTH"].Visibility = Visibility.Collapsed;
                dgDetail.Columns["CONV_RATE"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion


        #region [ Util ]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region [동] - 조회 조건
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            init();
        }
        #endregion

        #region [라인] - 조회 조건
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            init();
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {

            }
        }
        #endregion

        #region [공정] - 조회 조건
        private void cboMatlLossProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboMatlLossProcess.Items.Count > 0 && cboMatlLossProcess.SelectedValue != null && !cboMatlLossProcess.SelectedValue.Equals("SELECT"))
            {
                init();
            }
        }
        #endregion

        private void btnLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgDetail.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU4961");  //상세 데이터가 없습니다.
                    return;
                }

                DataTable dtDetail = DataTableConverter.Convert(dgDetail.ItemsSource);
                MTRL001_203_LABEL popup = new MTRL001_203_LABEL();
                popup.FrameOperation = this.FrameOperation;

                string sLangid = LoginInfo.LANGID;
                string sAreaid = cboArea.SelectedValue.ToString();
                string sEqsgid = cboEquipmentSegment.SelectedValue.ToString();
                string sProcid = cboMatlLossProcess.SelectedValue.ToString();
                sMtrlid = dtDetail.Rows[0]["MTRLID"].ToString();

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = sLangid;
                    Parameters[1] = sAreaid;
                    Parameters[2] = sEqsgid;
                    Parameters[3] = sProcid;
                    Parameters[4] = sMtrlid;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                }

                popup.Closed += new EventHandler(popup_Closed);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtMtrlID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SummaryData();
            }
        }
        
        private void btnWipDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (dgDetail.GetRowCount() == 0)
                {
                    Util.MessageValidation("SFU4961");  //상세 데이터가 없습니다.
                    return;
                }

                DataTable dtDetail = DataTableConverter.Convert(dgDetail.ItemsSource);
                MTRL001_203_WIP_DELETE popup = new MTRL001_203_WIP_DELETE();
                popup.FrameOperation = this.FrameOperation;

                string sLangid = LoginInfo.LANGID;
                string sAreaid = cboArea.SelectedValue.ToString();
                string sEqsgid = cboEquipmentSegment.SelectedValue.ToString();
                string sProcid = cboMatlLossProcess.SelectedValue.ToString();
                sMtrlid = dtDetail.Rows[0]["MTRLID"].ToString();

                if (popup != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = sLangid;
                    Parameters[1] = sAreaid;
                    Parameters[2] = sEqsgid;
                    Parameters[3] = sProcid;
                    Parameters[4] = sMtrlid;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                }

                popup.Closed += new EventHandler(popup_Closed);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popup_Closed(object sender, EventArgs e)
        {
            init();
            SummaryData();
            DetailData(sMtrlid);
        }

        private void dgSummary_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn != null && dg.CurrentColumn.Name.Equals("MTRLID") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    DetailData(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "MTRLID")));
                }               
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

        private void dgSummary_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSummary.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("MTRLID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }
    }
}

