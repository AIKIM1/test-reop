/*************************************************************************************
 Created Date : 2017.07.04
      Creator : 이슬아
   Decription : 포장 공정실적
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.04  이슬아 : Initial Created.
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Globalization;
using System.Reflection;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media.Animation;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_100.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_205 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        private Util _util = new Util();
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        string _sPGM_ID = "BOX001_205";

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_205()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: new C1ComboBox[] { cboLine }, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "ALL" }, sCase: "AREA_NO_AUTH");
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea }, cbChild: new C1ComboBox[] { cboEqpt }, sFilter: new string[] { "B" }, sCase: "PROCESSSEGMENTLINE");
            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { Process.CELL_BOXING }, sCase: "EQUIPMENT");
            _combo.SetCombo(cboLottype, CommonCombo.ComboStatus.ALL, sCase: "LOTTYPE");

            _combo.SetCombo(cboBoxType, CommonCombo.ComboStatus.ALL, sFilter: new string[] { "BOXTYPE_NJ" }, sCase: "COMMCODE_WITHOUT_CODE");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }

        #endregion

        #region Event

        #region [Main Window]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }


        private void dgReturn_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                //if (e.Cell.Row.Type == DataGridRowType.Item)
                //{
                //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblShipping.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblShipping.Background;
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblFinish.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblFinish.Background;
                //    }
                //    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "RCV_ISS_STAT_CODE")).Equals(lblCancel.Tag))
                //    {
                //        e.Cell.Presenter.Background = lblCancel.Background;
                //    }
                //    else
                //    {
                //        e.Cell.Presenter.Background = null;
                //    }
                //}
            }));
        }

        private void dgReturnResultChoice_Checked(object sender, RoutedEventArgs e)
        {

        }
        #endregion
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void btnRegCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxBoxList = _util.GetDataGridCheckFirstRowIndex(dgBox, "CHK");

                BOX001_201_CELL_DETL puCellDetl = new BOX001_201_CELL_DETL();
                puCellDetl.FrameOperation = FrameOperation;

                if (puCellDetl != null)
                {
                    string sBoxID = idxBoxList < 0 ? string.Empty : Util.NVC(dgBox.GetCell(idxBoxList, dgBox.Columns["BOXID"].Index).Value);
                 

                    object[] Parameters = new object[6];
                    Parameters[0] = sBoxID;
                    Parameters[1] = LoginInfo.USERID;
                    //Parameters[2] = sProdID;
                    //Parameters[3] = sPkgLotID;
                    //Parameters[4] = sProject;
                    //Parameters[5] = sProdName;
                    C1WindowExtension.SetParameters(puCellDetl, Parameters);

                    puCellDetl.Closed += new EventHandler(puCellDetl_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puCellDetl);
                    puCellDetl.BringToFront();
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

        private void puCellDetl_Closed(object sender, EventArgs e)
        {
            BOX001_201_CELL_DETL popup = sender as BOX001_201_CELL_DETL;

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgBox, "CHK");

            //미 선택 후 팝업 작업하는 경우가 있음.
            if (idxPallet >= 0)
            {
                GetCellInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        private void btnLabelPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                    return;

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

                string sPalletId = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);
                string sExpdom = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["EXP_DOM_TYPE_NAME"].Index).Value);
                string sExpdomc = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["EXP_DOM_TYPE_CODE"].Index).Value);

                BOX001_205_ADD_LABEL pulabel = new BOX001_205_ADD_LABEL();
                pulabel.FrameOperation = FrameOperation;

                if (pulabel != null)
                {

                    object[] Parameters = new object[6];
                    Parameters[0] = sPalletId;
                    //Parameters[1] = LoginInfo.USERID;
                    Parameters[2] = sExpdom;
                    Parameters[3] = sExpdomc;
                    //Parameters[4] = sProject;
                    //Parameters[5] = sProdName;
                    C1WindowExtension.SetParameters(pulabel, Parameters);

                    pulabel.Closed += new EventHandler(pulabel_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(pulabel);
                    pulabel.BringToFront();
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

        private void pulabel_Closed(object sender, EventArgs e)
        {
            BOX001_205_ADD_LABEL popup = sender as BOX001_205_ADD_LABEL;

            this.grdMain.Children.Remove(popup);
        }

        #region [Button]

        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_PACKED_PALLET_HIST_NJ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
            Util.gridClear(dgBox);
        }
        #endregion

        #region [CheckBox]

        #endregion


        #endregion

        #region Mehod

        #region [BizCall]
        /// <summary>
        /// BR_PRD_GET_PACKED_PALLET_HIST_NJ
        /// </summary>
        private void GetPalletInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("EQPTID", typeof(String));
                RQSTDT.Columns.Add("PROJECT", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("BOXTYPE", typeof(String));
                RQSTDT.Columns.Add("SUBLOTID", typeof(String));
                RQSTDT.Columns.Add("LOTTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                {
                    dr["BOXID"] = txtPalletID.Text;
                }
                if (!string.IsNullOrWhiteSpace(txtCellID.Text))
                {
                    dr["SUBLOTID"] = txtCellID.Text;
                }

                else
                {
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["AREAID"] = string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) ? null : cboArea.SelectedValue;
                    dr["EQSGID"] = string.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue)) ? null : cboLine.SelectedValue;
                    dr["EQPTID"] = string.IsNullOrWhiteSpace(Util.NVC(cboEqpt.SelectedValue)) ? null : cboEqpt.SelectedValue;
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtLotID.Text) ? null : txtLotID.Text;
                    dr["BOXTYPE"] = string.IsNullOrEmpty(cboBoxType.SelectedValue.ToString()) ? null : cboBoxType.SelectedValue;
                    dr["LOTTYPE"] = string.IsNullOrEmpty(cboLottype.SelectedValue.ToString()) ? null : cboLottype.SelectedValue;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PACKED_PALLET_HIST_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgPallet, dtResult, FrameOperation, true);

                if (dgPallet.GetRowCount() > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        /// <summary>
        /// BR_PRD_GET_PACKED_BOX_HIST_NJ
        /// </summary>
        private void GetBoxInfo()
        {
            try
            {

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sPalletId = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sPalletId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PACKED_BOX_HIST_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgBox, dtResult, FrameOperation, true);

                if (dgBox.GetRowCount() > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgBox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgBox.Columns["SUBLOT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        /// <summary>
        /// BR_PRD_GET_PACKED_SUBLOT_HIST_NJ
        /// </summary>
        private void GetCellInfo()
        {
            try
            {

                int idxBox = _util.GetDataGridCheckFirstRowIndex(dgBox, "CHK");

                if (idxBox < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sBoxId = Util.NVC(dgBox.GetCell(idxBox, dgBox.Columns["BOXID"].Index).Value);
                string sBoxtype = Util.NVC(dgBox.GetCell(idxBox, dgBox.Columns["BOXTYPE"].Index).Value);

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("BOXTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = sBoxId;
                dr["BOXTYPE"] = sBoxtype;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PACKED_SUBLOT_HIST_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgCell, dtResult, FrameOperation, true);
                if (dtResult.Rows.Count > 0)
                    btnExpandFrame.IsChecked = false;
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

        #region [Validation]
        private bool Validation()
        {
            int idxBoxList = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

            if (idxBoxList < 0)
            {
                //Util.Alert("선택된 Pallet가 없습니다.");
                Util.MessageValidation("SFU3425");
                return false;
            }
            return true;
        }

        #endregion

        #region [PopUp Event]

        #endregion

        #region [Func]
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                System.Threading.Thread.Sleep(300);
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion


        #endregion

        private void btnCellDetail_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {

            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                return;

            int idxBox = _util.GetDataGridCheckFirstRowIndex(dgBox, "CHK");
            if (idxBox < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }
            string boxType = Util.NVC(dgBox.GetCell(idxBox, dgBox.Columns["BOXTYPE"].Index).Value);
            string boxID = Util.NVC(dgBox.GetCell(idxBox, dgBox.Columns["BOXID"].Index).Value);
            try
            {
                string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("LANGID");
                dtInData.Columns.Add("USERID");
                dtInData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                dtInData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용
                DataRow dr = dtInData.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["PGM_ID"] = _sPGM_ID;
                dr["BZRULE_ID"] = sBizRule;
                dtInData.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INBOX");
                dtInbox.Columns.Add("BOXID");
                dr = dtInbox.NewRow();
                dr["BOXID"] = boxID;
                dtInbox.Rows.Add(dr);

                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");
                dr = dtInPrint.NewRow();
                dr["PRMK"] = _sPrt;
                dr["RESO"] = _sRes;
                dr["PRCN"] = _sCopy;
                dr["MARH"] = _sXpos;
                dr["MARV"] = _sYpos;
                dr["DARK"] = _sDark;
                dtInPrint.Rows.Add(dr);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable dtResult = dsResult.Tables["OUTDATA"];
                    string zplCode = string.Empty;
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                    }
                    PrintLabel(zplCode, _drPrtInfo);
                }

                //if (boxType.Equals("OUTBOX"))
                //{
                //    DataSet ds = new DataSet();
                //    DataTable dtInData = ds.Tables.Add("INDATA");
                //    dtInData.Columns.Add("BOXID");
                //    dtInData.Columns.Add("LANGID");
                //    DataRow dr = dtInData.NewRow();
                //    dr["BOXID"] = boxID;
                //    dr["LANGID"] = LoginInfo.LANGID;
                //    dtInData.Rows.Add(dr);
                //    DataTable dtInPrint = ds.Tables.Add("INPRINT");
                //    dtInPrint.Columns.Add("PRMK");
                //    dtInPrint.Columns.Add("RESO");
                //    dtInPrint.Columns.Add("PRCN");
                //    dtInPrint.Columns.Add("MARH");
                //    dtInPrint.Columns.Add("MARV");
                //    dtInPrint.Columns.Add("DARK");
                //    dr = dtInPrint.NewRow();
                //    dr["PRMK"] = _sPrt;
                //    dr["RESO"] = _sRes;
                //    dr["PRCN"] = _sCopy;
                //    dr["MARH"] = _sXpos;
                //    dr["MARV"] = _sYpos;
                //    dr["DARK"] = _sDark;
                //    dtInPrint.Rows.Add(dr);
                //    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INPRINT", "OUTDATA", ds);
                //    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count>0)
                //    {
                //        DataTable dtResult = dsResult.Tables["OUTDATA"];
                //        string zplCode = string.Empty;
                //        for (int i = 0; i < dtResult.Rows.Count; i++)
                //        {
                //            zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                //        }
                //        PrintLabel(zplCode, _drPrtInfo);
                //    }
                //}
                //else if (boxType.Equals("INBOX"))
                //{
                //    DataSet ds = new DataSet();
                //    DataTable dtInData = ds.Tables.Add("INDATA");
                //    dtInData.Columns.Add("BOXID");
                //    dtInData.Columns.Add("LANGID");
                //    DataRow dr = dtInData.NewRow();
                //    dr["BOXID"] = boxID;
                //    dr["LANGID"] = LoginInfo.LANGID;
                //    dtInData.Rows.Add(dr);
                //    DataTable dtInPrint = ds.Tables.Add("INPRINT");
                //    dtInPrint.Columns.Add("PRMK");
                //    dtInPrint.Columns.Add("RESO");
                //    dtInPrint.Columns.Add("PRCN");
                //    dtInPrint.Columns.Add("MARH");
                //    dtInPrint.Columns.Add("MARV");
                //    dtInPrint.Columns.Add("DARK");
                //    dr = dtInPrint.NewRow();
                //    dr["PRMK"] = _sPrt;
                //    dr["RESO"] = _sRes;
                //    dr["PRCN"] = _sCopy;
                //    dr["MARH"] = _sXpos;
                //    dr["MARV"] = _sYpos;
                //    dr["DARK"] = _sDark;
                //    dtInPrint.Rows.Add(dr);
                //    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_INBOX_REPRT_NJ", "INDATA,INPRINT", "OUTDATA", ds);
                //    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                //    {
                //        DataTable dtResult = dsResult.Tables["OUTDATA"];
                //        string zplCode = string.Empty;
                //        for (int i = 0; i < dtResult.Rows.Count; i++)
                //        {
                //            zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                //        }
                //        PrintLabel(zplCode, _drPrtInfo);
                //    }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }

        //private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        //{
        //    DetailArea.ColumnDefinitions[4].Width = new GridLength(0);
        //    LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
        //    gla.From = new GridLength(1, GridUnitType.Star);
        //    gla.To = new GridLength(0, GridUnitType.Star); 0
        //    gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        //    DetailArea.ColumnDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
        //}

        //private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    DetailArea.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Star);
        //    LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
        //    gla.From = new GridLength(0, GridUnitType.Star);
        //    gla.To = new GridLength(1, GridUnitType.Star);
        //    gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
        //    DetailArea.ColumnDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
        //}
        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM001.GridLengthAnimation gla = new CMM001.GridLengthAnimation();
                gla.From = DetailArea.ColumnDefinitions[2].Width; //new GridLength(0.5, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.AccelerationRatio = 0.3;
                gla.DecelerationRatio = 0.7;
                DetailArea.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
            }
            catch (Exception ex)
            {

            }
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                DetailArea.ColumnDefinitions[0].Width = new GridLength(2, GridUnitType.Star);
                DetailArea.ColumnDefinitions[1].Width = new GridLength(8);
                CMM001.GridLengthAnimation gla = new CMM001.GridLengthAnimation();
                gla.From = new GridLength(0);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 300);
                gla.AccelerationRatio = 0.7;
                gla.DecelerationRatio = 0.3;

                DetailArea.ColumnDefinitions[2].BeginAnimation(ColumnDefinition.WidthProperty, gla);
                //   DetailArea.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            catch (Exception ex)
            {
                // CommonUtil.MessageError(ex);
            }
        }

        private void dgPalletChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgPallet.SelectedIndex = idx;

                GetBoxInfo();
            }
        }

        private void dgBoxChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }

                //row 색 바꾸기
                dgBox.SelectedIndex = idx;

                GetCellInfo();
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");
                string palletID = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);
                string palletType = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXTYPE"].Index).Value);
                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("LANGID");
                DataRow inDataRow = inDataTable.NewRow();
                inDataRow["BOXID"] = palletID;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inDataTable.Rows.Add(inDataRow);

                if (palletType.Equals("PLT") || palletType.Equals("OFFGRD_PLT")) //1차 팔레트 태그 -. 2019.08.15 이제섭 등외품 팔레트도 출력될 수 있도록 추가
                {
                    //DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_INPALLET_NJ", "INDATA", "OUTPALLET,OUTBOX,OUTLOT", inDataSet);

                    //int prtCount = 0;
                    //if (resultDs != null)
                    //{
                    //    DataTable dtOutbox = resultDs.Tables["OUTBOX"];
                    //    DataTable dtOutdata = resultDs.Tables["OUTDATA"];

                    //    if (dtOutbox.Rows.Count > 60)
                    //    {
                    //        prtCount = dtOutbox.Rows.Count % 60 == 0 ? dtOutbox.Rows.Count / 60 : (dtOutbox.Rows.Count / 60) + 1;
                    //        DataSet[] dsArr = new DataSet[prtCount];

                    //        for (int cnt = 0; cnt < prtCount; cnt++)
                    //        {
                    //            DataTable dtBox = null;
                    //            dsArr[cnt] = new DataSet();
                    //            dtBox = dtOutbox.Select("BOXSEQ >= " + (60 * cnt + 1) + " AND BOXSEQ <=" + (cnt + 1) * 60).CopyToDataTable();
                    //            dtBox.TableName = "OUTBOX";
                    //            dsArr[cnt].Tables.Add(dtOutdata.Copy());
                    //            dsArr[cnt].Tables.Add(dtBox);

                    //            //dtData = dtOutdata.Copy();
                    //            //  dtBox = dtOutbox.Select("BOXSEQ >= " + (prtCount + 1) + " AND BOXSEQ <=" + (prtCount + 1) * 60).CopyToDataTable();

                    //        }
                    //        for (int prt = dsArr.Length - 1; prt > -1; prt--)
                    //        {
                    //            TagPrint_1st(dsArr[prt]);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        TagPrint_1st(resultDs);
                    //    }
                    //}
                    TagPrint_1st();
                }
                else if (palletType.Equals("2ND_PLT") || palletType.Equals("SHIP_PLT")|| palletType.Equals("TESLA_PLT")) // 2차팔레트 태그
                {
                    //DataSet resultDs = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_TAG_SHIP_PALLET_NJ", "INDATA", "OUTBOX,OUTLOT", inDataSet);
                    //int prtCount = 0;
                    //if (resultDs != null)
                    //{
                    //    DataTable dtOutbox = resultDs.Tables["OUTBOX"];
                    //    DataTable dtOutlot = resultDs.Tables["OUTLOT"];

                    //    if (dtOutlot.Rows.Count > 20)
                    //    {
                    //        prtCount = dtOutlot.Rows.Count % 20 == 0 ? dtOutlot.Rows.Count / 20 : (dtOutlot.Rows.Count / 20) + 1;
                    //        DataSet[] dsArr = new DataSet[prtCount];

                    //        for (int cnt = 0; cnt < prtCount; cnt++)
                    //        {
                    //            DataTable dtLot = null;
                    //            dsArr[cnt] = new DataSet();
                    //            dtLot = dtOutlot.Select("SEQ >= " + (20 * cnt + 1) + " AND SEQ <=" + (cnt + 1) * 20).CopyToDataTable();
                    //            dtLot.TableName = "OUTLOT";
                    //            dsArr[cnt].Tables.Add(dtOutbox.Copy());
                    //            dsArr[cnt].Tables.Add(dtLot);

                    //            //dtData = dtOutdata.Copy();
                    //            //  dtBox = dtOutbox.Select("BOXSEQ >= " + (prtCount + 1) + " AND BOXSEQ <=" + (prtCount + 1) * 60).CopyToDataTable();

                    //        }
                    //        for (int prt = dsArr.Length - 1; prt > -1; prt--)
                    //        {
                    //            TagPrint_2nd(dsArr[prt]);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        TagPrint_2nd(resultDs);
                    //    }
                    //}
                    TagPrint_2nd();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void TagPrint_1st()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            string sPalletId = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);

            Report_1st_Boxing popup = new Report_1st_Boxing();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(popup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        //private void TagPrint_2nd(DataSet ds)
        //{
        //    Report_2nd_Boxing popup = new Report_2nd_Boxing();
        //    popup.FrameOperation = this.FrameOperation;
        //    //  DataSet ds = GetPalletDataSet();
        //    if (popup != null)
        //    {
        //        object[] Parameters = new object[3];

        //        Parameters[0] = ds;
        //        C1WindowExtension.SetParameters(popup, Parameters);

        //        popup.Closed += new EventHandler(popup_Closed);
        //        grdMain.Children.Add(popup);
        //        popup.BringToFront();
        //    }
        //}

        private void TagPrint_2nd()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            string sPalletId = Util.NVC(dgPallet.GetCell(idxPallet, dgPallet.Columns["BOXID"].Index).Value);

            Report_2nd_Boxing popup = new Report_2nd_Boxing();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[3];

                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(popup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }
        private void popup_Closed(object sender, EventArgs e)
        {

        }

        private void btnCellInfo_Click(object sender, RoutedEventArgs e)
        {

            List<int> chkList = _util.GetDataGridCheckRowIndex(dgPallet, "CHK");
            object[] param = new object[chkList.Count];
            for (int i = 0; i < param.Length; i++)
            {
                param[i] = dgPallet.GetCell(chkList[i], dgPallet.Columns["BOXID"].Index).Value;
            }
            this.FrameOperation.OpenMenu("SFU010160570", true, param);


        }


        private void btnRePrintAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                return;

            int idxPlt = _util.GetDataGridCheckFirstRowIndex(dgPallet, "CHK");

            if (idxPlt < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            string pltType = Util.NVC(dgPallet.GetCell(idxPlt, dgPallet.Columns["BOXTYPE"].Index).Value);
            string boxID = string.Empty;

            try
            {
                string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("LANGID");
                dtInData.Columns.Add("USERID");
                dtInData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                dtInData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow dr = null;
                dr = dtInData.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["PGM_ID"] = _sPGM_ID;
                dr["BZRULE_ID"] = sBizRule;
                dtInData.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INBOX");
                dtInbox.Columns.Add("BOXID");
                for (int i = 0; i < dgBox.Rows.Count-1; i++)
                {
                    dr = dtInbox.NewRow();
                    boxID = Util.NVC(dgBox.GetCell(i, dgBox.Columns["BOXID"].Index).Value);
                    dr["BOXID"] = boxID;
                    dtInbox.Rows.Add(dr);
                }
                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");

                dr = dtInPrint.NewRow();
                dr["PRMK"] = _sPrt;
                dr["RESO"] = _sRes;
                dr["PRCN"] = _sCopy;
                dr["MARH"] = _sXpos;
                dr["MARV"] = _sYpos;
                dr["DARK"] = _sDark;
                dtInPrint.Rows.Add(dr);

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    DataTable dtResult = dsResult.Tables["OUTDATA"];
                    string zplCode = string.Empty;
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                    }
                    PrintLabel(zplCode, _drPrtInfo);
                }
                //DataTable dt = new DataTable();
                //dt.Columns.Add("BOXID");
                //dt.Columns.Add("USERID");
                //DataRow newRow = null;
                //for (int i = 0; i < dgBox.Rows.Count; i++)
                //{
                //    boxID = Util.NVC(dgBox.GetCell(idxPlt, dgBox.Columns["BOXID"].Index).Value);
                //    newRow = dt.NewRow();
                //    newRow["BOXID"] = boxID;
                //    newRow["USERID"] = txtWorker.Tag;

                //}
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA", "OUTDATA", dt);
                //if (dtResult != null && dtResult.Rows.Count > 0)
                //{
                //    string zplCode = string.Empty;
                //    for (int i = 0; i < dtResult.Rows.Count; i++)
                //    {
                //        zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                //    }
                //    PrintLabel(zplCode, _drPrtInfo);
                //}
                //}

                //    else if (pltType.Equals("2ND_PLT") || pltType.Equals("SHIP_PLT"))
                //{
                //    DataTable dt = new DataTable();
                //    dt.Columns.Add("BOXID");
                //    dt.Columns.Add("USERID");
                //    DataRow newRow = null;
                //    for (int i = 0; i < dgBox.Rows.Count; i++)
                //    {
                //        boxID = Util.NVC(dgBox.GetCell(idxPlt, dgBox.Columns["BOXID"].Index).Value);
                //        newRow = dt.NewRow();
                //        newRow["BOXID"] = boxID;
                //        newRow["USERID"] = txtWorker.Tag;
                //    }
                //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA", "OUTDATA", dt);
                //    if (dtResult != null && dtResult.Rows.Count > 0)
                //    {
                //        string zplCode = string.Empty;
                //        for (int i = 0; i < dtResult.Rows.Count; i++)
                //        {
                //            zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                //        }
                //        PrintLabel(zplCode, _drPrtInfo);
                //    }
                //}
                //else
                //    return;
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
    }
}
