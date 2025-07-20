/*************************************************************************************
 Created Date : 2018.01.02
      Creator : 
   Decription : 활성화 대차 재공관리
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.08  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using C1.WPF;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_210 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

       

        public COM001_210()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
        
            listAuth.Add(btnDetail);
            listAuth.Add(btnMove);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            CommonCombo _combo = new CommonCombo();

            //공정
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboAreaChild = { cboEquipment };
            _combo.SetCombo(cboProcid, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, cbChild: cboAreaChild, sCase: "POLYMER_PROCESS");

            //설비
            C1ComboBox[] cboEquipmentParent = {cboProcid};
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "POLYMER_EQUIPMENT");

            //대차상태
            String[] sFilter2 = { "", "CTNR_STAT_CODE" };
            _combo.SetCombo(cboCartStat, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

            //INBOX상태
            _combo.SetCombo(cboInboxStat, CommonCombo.ComboStatus.ALL,  sCase: "INBOX_STAT");

            // 재공구분
            String[] sFilterQLTY = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cbowipType, CommonCombo.ComboStatus.ALL, sFilter: sFilterQLTY, sCase: "COMMCODES");

            // 등급
            String[] sFilterGRD = { "", "CAPA_GRD_CODE" };
            _combo.SetCombo(cboCapaGrd, CommonCombo.ComboStatus.ALL, sFilter: sFilterGRD, sCase: "COMMCODES");
        }
        #endregion

        #region Event

        #region 활성화 대차 재공관리 조회 : btnSearch_Click()
        //조회

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCART_Info();

        }
        #endregion
      
        #region 활성화 대차 재공관리 대차ID 조회  : txtCartID_KeyDown()
        //대차ID 조회
        private void txtCartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetCART_Info();
            }
        }
        #endregion

        #region 활성화 대차 재공관리 클립보다 대차조회 : txtCartID_PreviewKeyDown()
        private void txtCartID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);
                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    txtCartID.Text = sPasteString.Replace("\r\n", ",");
                    GetCART_Info();
                    txtCartID.Text = string.Empty;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        #endregion

        #region 활성화 대차 재공관리 INBOX ID 조회 : txtInbox_KeyDown()

        //INBXOID 조회
        private void txtInbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetCART_Info();
            }
        }
        #endregion
   
        #region 활성화 대차 재공관리 대차 상세 팝업 : btnDetail_Click(), popupCartDetail_Closed()

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }

                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");

                CMM_POLYMER_FORM_CART_DETAIL popupCartDetail = new CMM_POLYMER_FORM_CART_DETAIL();
                popupCartDetail.QueryCall = true;
                popupCartDetail.FrameOperation = this.FrameOperation;

                object[] parameters = new object[5];

                parameters[0] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["PROCID"].Index).Value);   //ProcessCode;
                parameters[1] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["PROCNAME"].Index).Value); //ProcessName;
                parameters[2] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CURR_EQPTID"].Index).Value); //EquipmentCode;
                parameters[3] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["EQPTNAME"].Index).Value); //EquipmentName;
                parameters[4] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CTNR_ID"].Index).Value);  //ProdCartId;    // ButtonCertSelect.Tag;

                C1WindowExtension.SetParameters(popupCartDetail, parameters);
                popupCartDetail.Closed += new EventHandler(popupCartDetail_Closed);
                //grdMain.Children.Add(popupCartDetail);
                //popupCartDetail.BringToFront();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupCartDetail);
                        popupCartDetail.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


        }
        //상세 팝업 닫기
        private void popupCartDetail_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_DETAIL popup = sender as CMM_POLYMER_FORM_CART_DETAIL;
            //if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            //{
            GetCART_Info();
            //}
            //this.grdMain.Children.Remove(popup);
            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        #endregion

        #region 활성화 대차 재공관리 대차 이동 팝업 : btnMove_Click(), popupCartMove_Closed()
        //대차이동
        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Move_Validation())
                {
                    return;
                }
                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");
                CMM_POLYMER_FORM_RE_CART_MOVE popupCartMove = new CMM_POLYMER_FORM_RE_CART_MOVE();
                popupCartMove.FrameOperation = this.FrameOperation;

                object[] parameters = new object[5];

                parameters[0] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["PROCID"].Index).Value);   //ProcessCode;
                parameters[1] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["PROCNAME"].Index).Value); //ProcessName;
                parameters[2] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CURR_EQPTID"].Index).Value); //EquipmentCode;
                parameters[3] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["EQPTNAME"].Index).Value); //EquipmentName;
                parameters[4] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CTNR_ID"].Index).Value);  //ProdCartId;    // ButtonCertSelect.Tag;

                C1WindowExtension.SetParameters(popupCartMove, parameters);
                popupCartMove.Closed += new EventHandler(popupCartMove_Closed);
                //grdMain.Children.Add(popupCartDetail);
                //popupCartDetail.BringToFront();
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupCartMove);
                        popupCartMove.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        //대차이동 팝업 닫기
        private void popupCartMove_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_RE_CART_MOVE popup = sender as CMM_POLYMER_FORM_RE_CART_MOVE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetCART_Info();
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }
        #endregion

        #region 활성화 대차 재공관리 대차 선택 : dgLotDetail_Checked()
        //대차 선택
        private void dgLotDetail_Checked(object sender, RoutedEventArgs e)
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

                        for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                        {
                            DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                        }
                        //row 색 바꾸기
                        dgSearch.SelectedIndex = idx;
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
        #endregion

        #region 활성화 대차 재공관리 대차 보관 취소 : btnCancel_Click()
        //보관 취소 
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Cancel_Validation())
                {
                    return;
                }

                //보관취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4459"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Cancel();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 활성화 대차 재공관리 스프레드 색깔 : dgSearch_LoadedCellPresenter(), dgSearch_UnloadedCellPresenter()
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgSearch.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if(e.Cell.Column.Name == "")
                    if (DataTableConverter.GetValue(e.Cell.Row.DataItem, "CTNR_ID2").ToString() == string.Empty)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.OrangeRed);
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D8BFD8"));

                    }
                    else
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dataGrid = sender as C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF000000");
                        if (convertFromString != null)
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }));
        }
        #endregion

        #region 활성화 대차 재공관리 Cell 등록 : btnCellInbput_Click(), popupCell_Closed()
        /// <summary>
        /// Cell 등록
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCellInbput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");
                CMM_POLYMER_CELL_INPUT popupCell = new CMM_POLYMER_CELL_INPUT();

                popupCell.CTNR_DEFC_LOT_CHK = "Y";  //대차ID/불량그룹 체크
                popupCell.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CTNR_ID"].Index).Value);
                C1WindowExtension.SetParameters(popupCell, parameters);
                popupCell.Closed += new EventHandler(popupCell_Closed);
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupCell);
                        popupCell.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Cell 등록 팝업 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void popupCell_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_CELL_INPUT popup = sender as CMM_POLYMER_CELL_INPUT;

            GetCART_Info();

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(popup);
                    break;
                }
            }

        }

        #endregion

        #endregion

        #region Method

        #region 활성화 대차 재공관리 조회 : GetCART_Info()
        // 조회
        private void GetCART_Info()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CTNR_STAT", typeof(string));
                inTable.Columns.Add("INBOX_STAT", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("INBOX_ID", typeof(string));
                inTable.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                inTable.Columns.Add("CAPA_GRD_CODE", typeof(string));
                DataRow newRow = inTable.NewRow();

                if (!string.IsNullOrEmpty(txtCartID.Text.Trim()))
                {
                    newRow["PROCID"] = null;
                    newRow["EQPTID"] = null;
                    newRow["CTNR_STAT"] = null;
                    newRow["INBOX_STAT"] = null;
                    newRow["PJT_NAME"] = txtPjt.Text;
                    newRow["PRODID"] = txtProdid.Text;
                    if (txtCartID.Text != string.Empty)
                    {
                        newRow["CTNR_ID"] = txtCartID.Text;
                    }
                    else
                    {
                        newRow["CTNR_ID"] = null;
                    }
                       

                    newRow["LOTID_RT"] = txtLot_RT.Text;
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    //newRow["INBOX_ID"] = txtInbox.Text;
                    if (txtInbox.Text != string.Empty)
                    {
                        newRow["INBOX_ID"] = txtInbox.Text;
                    }
                    else
                    {
                        newRow["INBOX_ID"] = null;
                    }
                    inTable.Rows.Add(newRow);
                }
                else if (!string.IsNullOrEmpty(txtInbox.Text.Trim()))
                {

                    newRow["PROCID"] = null;
                    newRow["EQPTID"] = null;
                    newRow["CTNR_STAT"] = null;
                    newRow["INBOX_STAT"] = null;
                    newRow["PJT_NAME"] = txtPjt.Text;
                    newRow["PRODID"] = txtProdid.Text;
                    if (txtCartID.Text != string.Empty)
                    {
                        newRow["CTNR_ID"] = txtCartID.Text;
                    }
                    else
                    {
                        newRow["CTNR_ID"] = null;
                    }
                    newRow["LOTID_RT"] = txtLot_RT.Text;
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    newRow["INBOX_ID"] = txtInbox.Text;
                    inTable.Rows.Add(newRow);

                }
                else
                {
                    newRow["PROCID"] = Util.GetCondition(cboProcid, "SFU1459"); //공정을 선택하세요
                    if (newRow["PROCID"].Equals("")) return;

                    newRow["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                    if (cboCartStat.SelectedValue.ToString() == "WORKING")
                    {
                        newRow["CTNR_STAT"] = null;
                    }
                    else if (cboCartStat.SelectedValue.ToString() == "MOVING")
                    {
                        newRow["CTNR_STAT"] = null;
                    }
                    else
                    {
                        newRow["CTNR_STAT"] = Util.GetCondition(cboCartStat, bAllNull: true);
                    }
                    newRow["INBOX_STAT"] = Util.GetCondition(cboInboxStat, bAllNull: true);
                    newRow["PJT_NAME"] = txtPjt.Text;
                    newRow["PRODID"] = txtProdid.Text;
                    if (txtCartID.Text != string.Empty)
                    {
                        newRow["CTNR_ID"] = txtCartID.Text;
                    }
                    else
                    {
                        newRow["CTNR_ID"] = null;
                    }
                    newRow["LOTID_RT"] = txtLot_RT.Text;
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    if (txtInbox.Text != string.Empty)
                    {
                        newRow["INBOX_ID"] = txtInbox.Text;
                    }
                    else
                    {
                        newRow["INBOX_ID"] = null;
                    }
                    newRow["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cbowipType, bAllNull: true);
                    newRow["CAPA_GRD_CODE"] = Util.GetCondition(cboCapaGrd, bAllNull: true);
                    inTable.Rows.Add(newRow);
                }

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_WIP_INFO", "INDATA", "OUTDATA", inTable);

                for (int i = 0; i < dtMain.Rows.Count; i++)
                {

                    if (dtMain.Rows[i]["CTNR_STAT_CODE"].ToString() == "CREATED" && dtMain.Rows[i]["CURR_EQPTID"].ToString() != string.Empty)
                    {
                        dtMain.Rows[i]["CTNR_STAT_NAME"] = ObjectDic.Instance.GetObjectName("작업중");
                        dtMain.Rows[i]["CTNR_STAT_CODE"] = "WORKING";
                    }

                    if (dtMain.Rows[i]["MOVE_ORD_STAT_CODE"].ToString() == "MOVING" && dtMain.Rows[i]["INBOX_STAT"].ToString() != string.Empty)
                    {
                        dtMain.Rows[i]["CTNR_STAT_NAME"] = ObjectDic.Instance.GetObjectName("이동중");
                        dtMain.Rows[i]["CTNR_STAT_CODE"] = "MOVING";
                    }

                }
                DataTable dtBindMain = new DataTable();
                dtBindMain = dtMain.Clone();
                if (cboCartStat.SelectedIndex != 0)
                {

                    //작업중은 CTNR_STAT_CODE가 CREATED이고 CURR_EQPTID가 존재하면 작업중으로 표시된다.
                    //아래와 같이 한 이유는 조회조건의 CTNR_STAT_CODE 가 작업중일 경우 상태가 WORKING 이므로  
                    //작업중을 선택후 조회를 하게되면 위에서 정의한 작업중 값으로 조회를 해서  바인딩하기 위하여 아래 로직 추가
                    DataRow[] drInbox = dtMain.Select("CTNR_STAT_CODE ='" + cboCartStat.SelectedValue.ToString() + "'");
                    foreach (DataRow dr in drInbox)
                    {
                        DataRow drBindInBox = dtBindMain.NewRow();
                        drBindInBox["CTNR_ID2"] = dr["CTNR_ID2"];
                        drBindInBox["CHK"] = dr["CHK"];
                        drBindInBox["CTNR_ID"] = dr["CTNR_ID"];
                        drBindInBox["PRJT_NAME"] = dr["PRJT_NAME"];
                        drBindInBox["PRODID"] = dr["PRODID"];
                        drBindInBox["MKT_TYPE_NAME"] = dr["MKT_TYPE_NAME"];
                        drBindInBox["FORM_WRK_TYPE_NAME"] = dr["FORM_WRK_TYPE_NAME"];
                        drBindInBox["PROCNAME"] = dr["PROCNAME"];
                        drBindInBox["CTNR_STAT_NAME"] = dr["CTNR_STAT_NAME"];
                        drBindInBox["EQPTNAME"] = dr["EQPTNAME"];
                        drBindInBox["INSDTTM"] = dr["INSDTTM"];
                        drBindInBox["PRE_PROCNAME"] = dr["PRE_PROCNAME"];
                        drBindInBox["ACTDTTM"] = dr["ACTDTTM"];
                        drBindInBox["LOTID_RT"] = dr["LOTID_RT"];
                        drBindInBox["CAPA_GRD_CODE"] = dr["CAPA_GRD_CODE"];
                        drBindInBox["SHIPTO_NAME"] = dr["SHIPTO_NAME"];
                        drBindInBox["INBOX_STAT_NAME"] = dr["INBOX_STAT_NAME"];
                        drBindInBox["INBOX_QTY"] = dr["INBOX_QTY"];
                        drBindInBox["CELL_QTY"] = dr["CELL_QTY"];
                        drBindInBox["CELL_IN_QTY"] = dr["CELL_IN_QTY"];
                        drBindInBox["INBOX_STAT"] = dr["INBOX_STAT"];
                        drBindInBox["MKT_TYPE_CODE"] = dr["MKT_TYPE_CODE"];
                        drBindInBox["CTNR_STAT_CODE"] = dr["CTNR_STAT_CODE"];
                        drBindInBox["PROCID"] = dr["PROCID"];
                        drBindInBox["FORM_WRK_TYPE_CODE"] = dr["FORM_WRK_TYPE_CODE"];
                        drBindInBox["CURR_EQPTID"] = dr["CURR_EQPTID"];
                        drBindInBox["UPDDTTM"] = dr["UPDDTTM"];
                        drBindInBox["PRE_PROCID"] = dr["PRE_PROCID"];
                        drBindInBox["HOLD_FLAG"] = dr["HOLD_FLAG"];

                        dtBindMain.Rows.Add(drBindInBox);

                    }

                    //여러개의 같은데이터를 GROUP BY 
                    DataTable LinQ = new DataTable();
                    DataRow Linqrow = null;
                    LinQ = dtBindMain.Clone();
                    decimal CountCtnr = 0; //대차수
                    decimal SumInboxQty = 0;  //INBOX 수
                    decimal SumCellQty = 0; //Cell 수

                    for (int i = 0; i < dtBindMain.Rows.Count; i++)
                    {
                        Linqrow = LinQ.NewRow();

                        Linqrow["CTNR_ID"] = dtBindMain.Rows[i]["CTNR_ID"];
                        LinQ.Rows.Add(Linqrow);

                        SumInboxQty = SumInboxQty + Convert.ToDecimal(dtBindMain.Rows[i]["INBOX_QTY"].ToString());
                        SumCellQty = SumCellQty + Convert.ToDecimal(dtBindMain.Rows[i]["CELL_QTY"].ToString());
                    }

                    //var summarydata = from SUMrow in LinQ.AsEnumerable()
                    //                  group SUMrow by new
                    //                  {
                    //                      CTNR_ID = SUMrow.Field<string>("CTNR_ID")

                    //                  } into grp
                    //                  select new
                    //                  {

                    //                      CTNR_ID = grp.Key.CTNR_ID
                    //                  };

                    // 집계한 수량을 집계 DataTable에 적재
                    //foreach (var data in summarydata)
                    //{

                    //    CountCtnr = CountCtnr + 1;

                    //}

                    // 대차의 Inbox수 산출
                    var summarydata = from row in dtMain.AsEnumerable()
                                      group row by new
                                      {
                                          CartID = row.Field<string>("CTNR_ID"),
                                      } into grp
                                      select new
                                      {
                                          CartID = grp.Key.CartID,
                                          CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))
                                      };

                    foreach (var row in summarydata)
                    {
                        dtMain.Select("CTNR_ID = '" + row.CartID + "'").ToList<DataRow>().ForEach(r => r["CART_CELL_QTY"] = row.CellSum);
                    }
                    dtMain.AcceptChanges();

                    //DataTable SumDT = new DataTable();
                    //SumDT = dtBindMain.Clone();
                    //DataRow row = null;
                    //row = SumDT.NewRow();
                    //row["CHK"] = "Collapsed";
                    //row["CTNR_ID2"] = string.Empty;
                    //row["CTNR_ID"] = CountCtnr;
                    //row["INBOX_QTY"] = SumInboxQty;
                    //row["CELL_QTY"] = SumCellQty;
                    //SumDT.Rows.Add(row);

                    ////집계한 데이터를 바인딩 데이터에 머지시킴
                    //dtBindMain.Merge(SumDT);

                    Util.gridClear(dgSearch);
                    Util.GridSetData(dgSearch, dtBindMain, FrameOperation);

                    // 대차 개수
                    int CtnrCount = dtMain.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                    DataGridAggregate.SetAggregateFunctions(dgSearch.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });

                }
                else
                {
                    //여러개의 같은데이터를 GROUP BY 
                    DataTable LinQ = new DataTable();
                    DataRow Linqrow = null;
                    LinQ = dtMain.Clone();
                    decimal CountCtnr = 0; //대차수
                    decimal SumInboxQty = 0;  //INBOX 수
                    decimal SumCellQty = 0; //Cell 수

                    for (int i = 0; i < dtMain.Rows.Count; i++)
                    {
                        Linqrow = LinQ.NewRow();

                        Linqrow["CTNR_ID"] = dtMain.Rows[i]["CTNR_ID"];
                        LinQ.Rows.Add(Linqrow);

                        SumInboxQty = SumInboxQty + Convert.ToDecimal(dtMain.Rows[i]["INBOX_QTY"].ToString());
                        SumCellQty = SumCellQty + Convert.ToDecimal(dtMain.Rows[i]["CELL_QTY"].ToString());
                    }

                    //var summarydata = from SUMrow in LinQ.AsEnumerable()
                    //                  group SUMrow by new
                    //                  {
                    //                      CTNR_ID = SUMrow.Field<string>("CTNR_ID")

                    //                  } into grp
                    //                  select new
                    //                  {

                    //                      CTNR_ID = grp.Key.CTNR_ID
                    //                  };

                    // 집계한 수량을 집계 DataTable에 적재
                    //foreach (var data in summarydata)
                    //{

                    //    CountCtnr = CountCtnr + 1;

                    //}

                    // 대차의 Inbox수 산출
                    var summarydata = from row in dtMain.AsEnumerable()
                                      group row by new
                                      {
                                          CartID = row.Field<string>("CTNR_ID"),
                                      } into grp
                                      select new
                                      {
                                          CartID = grp.Key.CartID,
                                          CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))
                                      };

                    foreach (var row in summarydata)
                    {
                        dtMain.Select("CTNR_ID = '" + row.CartID + "'").ToList<DataRow>().ForEach(r => r["CART_CELL_QTY"] = row.CellSum);
                    }
                    dtMain.AcceptChanges();


                    //DataTable SumDT = new DataTable();
                    //SumDT = dtMain.Clone();
                    //DataRow row = null;
                    //row = SumDT.NewRow();
                    //row["CHK"] = "Collapsed";
                    //row["CTNR_ID2"] = string.Empty;
                    //row["CTNR_ID"] = CountCtnr;
                    //row["INBOX_QTY"] = SumInboxQty;
                    //row["CELL_QTY"] = SumCellQty;
                    //SumDT.Rows.Add(row);

                    ////집계한 데이터를 바인딩 데이터에 머지시킴
                    //dtMain.Merge(SumDT);

                    Util.gridClear(dgSearch);

                    Util.GridSetData(dgSearch, dtMain, FrameOperation);

                    // 대차 개수
                    int CtnrCount = dtMain.DefaultView.ToTable(true, "CTNR_ID").Rows.Count;
                    DataGridAggregate.SetAggregateFunctions(dgSearch.Columns["CTNR_ID"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = ObjectDic.Instance.GetObjectName("대차") + ": " + CtnrCount.ToString("###,###") } });

                }

                //dgSearch.MergingCells -= dgSearch_MergingCells;
                //string[] sColumnName = new string[] { "CTNR_ID2", "CHK", "CTNR_ID", "PRJT_NAME", "PRODID", "MKT_TYPE_NAME", "FORM_WRK_TYPE_NAME", "PROCNAME", "CTNR_STAT_NAME", "EQPTNAME", "LOTID_RT", "INSDTTM", "INBOX_STAT_NAME", "INBOX_QTY", "CELL_QTY", "PRE_PROCNAME", "ACTDTTM" };
                //_Util.SetDataGridMergeExtensionCol(dgSearch, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                dgSearch.Columns["CTNR_ID2"].Width = new C1.WPF.DataGrid.DataGridLength(0);
                dgSearch.Columns["CHK"].Width = new C1.WPF.DataGrid.DataGridLength(40);
                dgSearch.Columns["CTNR_ID"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgSearch.Columns["PRJT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgSearch.Columns["PRODID"].Width = new C1.WPF.DataGrid.DataGridLength(120);
                dgSearch.Columns["MKT_TYPE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                dgSearch.Columns["FORM_WRK_TYPE_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                dgSearch.Columns["PROCNAME"].Width = new C1.WPF.DataGrid.DataGridLength(170);
                dgSearch.Columns["CTNR_STAT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgSearch.Columns["EQPTNAME"].Width = new C1.WPF.DataGrid.DataGridLength(170);
                dgSearch.Columns["LOTID_RT"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgSearch.Columns["INBOX_STAT_NAME"].Width = new C1.WPF.DataGrid.DataGridLength(100);
                dgSearch.Columns["INBOX_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                dgSearch.Columns["INSDTTM"].Width = new C1.WPF.DataGrid.DataGridLength(140);
                dgSearch.Columns["UPDDTTM"].Width = new C1.WPF.DataGrid.DataGridLength(140);
                dgSearch.Columns["CELL_QTY"].Width = new C1.WPF.DataGrid.DataGridLength(90);
                dgSearch.Columns["PRE_PROCNAME"].Width = new C1.WPF.DataGrid.DataGridLength(170);
                dgSearch.Columns["ACTDTTM"].Width = new C1.WPF.DataGrid.DataGridLength(140);


                //HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 활성화 대차 재공관리 보관취소 : Cancel()
        //보관취소
        private void Cancel()
        {

            int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");
            DataSet inData = new DataSet();
            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string)); // 공정ID
            inDataTable.Columns.Add("USERID", typeof(string)); // 사용자ID
            inDataTable.Columns.Add("LANGID", typeof(string)); // 다국어

            DataRow row = null;

            row = inDataTable.NewRow();
            row["IFMODE"] = "OFF";
            row["SRCTYPE"] = "UI";
            row["PROCID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[idxchk].DataItem, "PROCID"));
            row["USERID"] = LoginInfo.USERID;
            row["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(row);

            DataTable inCtnr = inData.Tables.Add("INCTNR");
            inCtnr.Columns.Add("CTNR_ID", typeof(string));
            row = inCtnr.NewRow();
            row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[idxchk].DataItem, "CTNR_ID"));
            inCtnr.Rows.Add(row);

            //보관취소
            try
            {
                //보관취소 실행
                //new ClientProxy().ExecuteService_Multi("BR_ACT_REG_CANCEL_STORE_CONTAINER", "INDATA,INCTNR", null, (Result, ex) =>
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_STORE_CTNR", "INDATA,INCTNR", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");  //정상 처리 되었습니다.
                    GetCART_Info();

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_CANCEL_STORE_CONTAINER", ex.Message, ex.ToString());

            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// 대차상세
        /// </summary>
        /// <returns></returns>
        private bool Validation()
        {


            if (dgSearch.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); // 조회된 데이터가 없습니다
                return false;
            }

            DataRow[] drSelect = DataTableConverter.Convert(dgSearch.ItemsSource).Select("CHK = '1'");


            int CheckCount = 0;

            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }


            return true;
        }

        /// <summary>
        /// 대차이동
        /// </summary>
        /// <returns></returns>
        private bool Move_Validation()
        {


            if (dgSearch.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");
                return false;
            }
            DataRow[] drSelect = DataTableConverter.Convert(dgSearch.ItemsSource).Select("CHK = '1'");
            int CheckCount = 0;

            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }
            if (CheckCount > 1)
            {
                Util.MessageValidation("SFU4159");//한건만 선택하세요.
                return false;
            }
            int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");
            if (Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CTNR_STAT_CODE"].Index).Value) == "WORKING")
            {
                Util.MessageValidation("SFU4431");//작업중인 대차정보입니다.
                return false;
            }
            if (Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["INBOX_STAT"].Index).Value) == "MOVING")
            {
                Util.MessageValidation("SFU4510");//이동중인 대차입니다.
                return false;
            }
            return true;
        }

        /// <summary>
        /// 대차보관취소
        /// </summary>
        /// <returns></returns>
        private bool Cancel_Validation()
        {


            if (dgSearch.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");
                return false;
            }
            DataRow[] drSelect = DataTableConverter.Convert(dgSearch.ItemsSource).Select("CHK = '1'");
            int CheckCount = 0;

            for (int i = 0; i < dgSearch.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[i].DataItem, "CHK")) == "1")
                {
                    CheckCount = CheckCount + 1;
                }
            }

            if (CheckCount == 0)
            {
                Util.MessageValidation("SFU1651");//선택된 항목이 없습니다.
                return false;
            }

            int idxchk = _Util.GetDataGridCheckFirstRowIndex(dgSearch, "CHK");
            if (Util.NVC(dgSearch.GetCell(idxchk, dgSearch.Columns["CTNR_STAT_CODE"].Index).Value) != "STORED")
            {
                Util.MessageValidation("SFU4458");//보관상태가 아닙니다.
                return false;
            }


            return true;
        }








        #endregion

        #endregion
      
        #region Funct
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



        #endregion

        private void btnRemarkInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CMM_CART_REMARK_INPUT popupRemark = new CMM_CART_REMARK_INPUT();

                popupRemark.FrameOperation = this.FrameOperation;

                object[] parameters = new object[1];
                parameters[0] = "";


                C1WindowExtension.SetParameters(popupRemark, parameters);
                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(popupRemark);
                        popupRemark.BringToFront();
                        break;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

