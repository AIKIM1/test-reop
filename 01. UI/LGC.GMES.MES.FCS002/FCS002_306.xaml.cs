/*************************************************************************************
 Created Date : 2016.07.19
      Creator : 오화백
   Decription : Pallet 병햡/분할/수량변경
--------------------------------------------------------------------------------------
 [Change History]
  2016.07.19  오화백 : Initial Created.
  2018.01.20  이상훈   C20180109_77053 [CSR ID:3577053] 고객 반품 구분 관리 건
  2023.03.13  LEEHJ    SI              소형활성화 MES 복사
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_306 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();
        string LOTID_RT = string.Empty;
        string LOTID_RT_SPLIT = string.Empty;
        string PALLETID_SPLIT = string.Empty;
        string PALLETID_SELECT = string.Empty;
        string PLT_TYPE_SPLIT = string.Empty;
        string PLT_TYPE_CHANGE = string.Empty;
        string PLT_TYPE_SELECT = string.Empty;

        public FCS002_306()
        {
            InitializeComponent();
            Initialize();
            InitCombo();
            InitHistorySpread();

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        private void Initialize()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("A010")) //소형조립일때만 보여줌
            {
                Tab_MergerMix.Visibility = Visibility.Visible;
            }
            else
            {
                Tab_MergerMix.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            #region Pallet병합(LOT혼합)  
            //동
            C1ComboBox[] cboAreaChildMix = { cboEquipmentSegmentMergerMix };
            _combo.SetCombo(cboAreaMergerMix, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChildMix);

            //라인
            C1ComboBox[] cboEquipmentSegmentParentMix = { cboAreaMergerMix };
            C1ComboBox[] cboEquipmentSegmentChildMix = { cboProcessMergerMix };
            _combo.SetCombo(cboEquipmentSegmentMergerMix, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChildMix, cbParent: cboEquipmentSegmentParentMix);

            //공정
            C1ComboBox[] cbProcessParentMix = { cboEquipmentSegmentMergerMix };
            String[] sFilterProcessMix = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessMergerMix, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcessMix, sCase: "PROCESS_SORT", cbParent: cbProcessParentMix);

            //구분
            String[] sFilterQmsRequestMix = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltyMergerMix, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequestMix, sCase: "COMMCODES");

            #endregion

            #region Pallet 병합  
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegmentMerger };
            _combo.SetCombo(cboAreaMerger, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaMerger };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessMerger };
            _combo.SetCombo(cboEquipmentSegmentMerger, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);


            //공정
            C1ComboBox[] cbProcessParent = { cboEquipmentSegmentMerger };
            String[] sFilterProcess = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessMerger, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcess, sCase: "PROCESS_SORT", cbParent: cbProcessParent);

            //구분
            String[] sFilterQmsRequest = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltyMerger, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest, sCase: "COMMCODES");

            #endregion

            #region Pallet 분할
            //동
            C1ComboBox[] cboAreaSplicChild = { cboEquipmentSegmentSplit };
            _combo.SetCombo(cboAreaSplit, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaSplicChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentSplitParent = { cboAreaSplit };
            C1ComboBox[] cboEquipmentSegmentSplitChild = { cboProcessSplit };
            _combo.SetCombo(cboEquipmentSegmentSplit, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentSplitChild, cbParent: cboEquipmentSegmentSplitParent);



            //공정
            C1ComboBox[] cbProcessSplitParent = { cboEquipmentSegmentSplit };
            String[] sFilterProcessSplit = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessSplit, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcessSplit, sCase: "PROCESS_SORT", cbParent: cbProcessSplitParent);


            //구분
            //String[] sFilterQmsRequest_Split = { "", "WIP_QLTY_TYPE_CODE" };
            //_combo.SetCombo(cboQltySplit, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest_Split, sCase: "COMMCODES");
            #endregion

            #region Pallet 수량 변경
            //동
            C1ComboBox[] cboAreaChangeChild = { cboEquipmentSegmentChange };
            _combo.SetCombo(cboAreaChange, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaChangeChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentChangeParent = { cboAreaChange };
            C1ComboBox[] cboEquipmentSegmentChangeChild = { cboProcessChange };
            _combo.SetCombo(cboEquipmentSegmentChange, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentChangeChild, cbParent: cboEquipmentSegmentChangeParent);

            //공정
            C1ComboBox[] cbProcessChangeParent = { cboEquipmentSegmentChange };
            String[] sFilterProcessChange = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessChange, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcessChange, sCase: "PROCESS_SORT", cbParent: cbProcessChangeParent);

            //구분
            String[] sFilterQmsRequest_Change = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltyChange, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest_Change, sCase: "COMMCODES");

            // 변경사유
            String[] sFilterReason = { "MODIFY_WIPQTY" };
            _combo.SetCombo(cboChangeReason, CommonCombo.ComboStatus.SELECT, sFilter: sFilterReason, sCase: "ACTIVITIREASON");

            #endregion

            #region Pallet 병합/분할/수량 변경이력
            //동
            C1ComboBox[] cboAreaHistoryChild = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaHistoryChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentHistoryParent = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentHistoryChild = { cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentHistoryChild, cbParent: cboEquipmentSegmentHistoryParent);

            //공정
            //C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            //_combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.SELECT, sCase: "PROCESS", cbParent: cbProcessHistoryParent);

            C1ComboBox[] cbProcessHistoryParent = { cboEquipmentSegmentHistory };
            String[] sFilterProcessHistory = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilterProcessHistory, sCase: "PROCESS_SORT", cbParent: cbProcessHistoryParent);



            // 구분
            String[] sFilterElectype = { "PALLET_RECONFIGURE_TYPE", "Y" };
            _combo.SetCombo(cboWorkHistory, CommonCombo.ComboStatus.NONE, sFilter: sFilterElectype, sCase: "COMMCODEATTR");

            #endregion

            #region Pallet 선별
            //동
            C1ComboBox[] cboAreaSplicChild_Select = { cboEquipmentSegmentSplit_Select };
            _combo.SetCombo(cboAreaSplit_Select, CommonCombo.ComboStatus.SELECT, sCase: "AREA", cbChild: cboAreaSplicChild_Select);

            //라인
            C1ComboBox[] cboEquipmentSegmentSplitParent_Select = { cboAreaSplit_Select };
            C1ComboBox[] cboEquipmentSegmentSplitChild_Select = { cboProcessSplit_Select };
            _combo.SetCombo(cboEquipmentSegmentSplit_Select, CommonCombo.ComboStatus.SELECT, sCase: "EQUIPMENTSEGMENT_FORM", cbChild: cboEquipmentSegmentSplitChild_Select, cbParent: cboEquipmentSegmentSplitParent_Select);



            //공정
            C1ComboBox[] cbProcessSplitParent_Select = { cboEquipmentSegmentSplit_Select };
            String[] sFilterProcessSplit_Select = { "SEARCH", string.Empty, string.Empty, string.Empty };
            _combo.SetCombo(cboProcessSplit_Select, CommonCombo.ComboStatus.SELECT, sFilter: sFilterProcessSplit_Select, sCase: "PROCESS_SORT", cbParent: cbProcessSplitParent_Select);


            //구분
            String[] sFilterQmsRequest_Split_Select = { "", "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltySplit_Select, CommonCombo.ComboStatus.ALL, sFilter: sFilterQmsRequest_Split_Select, sCase: "COMMCODES");
            #endregion
        }

        private void InitHistorySpread()
        {
            //이력Spread  셋팅
            #region Pallet 병합 
            dgListMergerHistory.Columns["CHK"].DisplayIndex = 0;
            dgListMergerHistory.Columns["TO_LOTID"].DisplayIndex = 1;
            dgListMergerHistory.Columns["WIPQTY2"].DisplayIndex = 2;
            dgListMergerHistory.Columns["LOTID_RT"].DisplayIndex = 3;
            dgListMergerHistory.Columns["PRJT_NAME"].DisplayIndex = 4;
            dgListMergerHistory.Columns["PRODID"].DisplayIndex = 5;
            dgListMergerHistory.Columns["MKT_TYPE_NAME"].DisplayIndex = 6;
            dgListMergerHistory.Columns["ACTDTTM"].DisplayIndex = 7;
            dgListMergerHistory.Columns["FROM_LOTID"].DisplayIndex = 8;
            dgListMergerHistory.Columns["QTY"].DisplayIndex = 9;
            dgListMergerHistory.Columns["WIPSEQ"].DisplayIndex = 10;
            dgListMergerHistory.Columns["PROCID"].DisplayIndex = 11;

            #endregion

            #region Pallet 분할 
            dgListSplitHistory.Columns["CHK"].DisplayIndex = 0;
            dgListSplitHistory.Columns["TO_LOTID"].DisplayIndex = 1;
            dgListSplitHistory.Columns["WIPQTY2"].DisplayIndex = 2;
            dgListSplitHistory.Columns["LOTID_RT"].DisplayIndex = 3;
            dgListSplitHistory.Columns["PRJT_NAME"].DisplayIndex = 4;
            dgListSplitHistory.Columns["PRODID"].DisplayIndex = 5;
            dgListSplitHistory.Columns["MKT_TYPE_NAME"].DisplayIndex = 6;
            dgListSplitHistory.Columns["ACTDTTM"].DisplayIndex = 7;
            dgListSplitHistory.Columns["FROM_LOTID"].DisplayIndex = 8;
            dgListSplitHistory.Columns["QTY"].DisplayIndex = 9;
            dgListSplitHistory.Columns["WIPSEQ"].DisplayIndex = 10;
            dgListSplitHistory.Columns["PROCID"].DisplayIndex = 11;

            #endregion
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnPalletMerger);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }

        #region Pallet 병합 
        private void btnSearchMerger_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Merger(true);
        }
        //Pallet ID 엔터시
        private void txtPalletIdMerger_KeyDown(object sender, KeyEventArgs e)
        {

            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdMerger.Text.Trim();
                    if (dgListInput.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInput.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInput.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListInput.ScrollIntoView(i, dgListInput.Columns["CHK"].Index);
                                dgListInput.SelectedIndex = i;
                                txtPalletIdMerger.Focus();
                                txtPalletIdMerger.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Merger(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            //try
            //{
            //    if (e.Key == Key.Enter)
            //    {
            //        GetLotList_Merger(true);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            //}
        }
        //Pallet 병합
        private void btnPalletMerger_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation())
                {
                    return;
                }
                //병합 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4174"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                PalletMerger();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReSetMerger_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInput);
            Util.gridClear(dgSelectInput);
            txtTotalLotIDMerger.Text = string.Empty;
            txtWipQtyMerger.Text = string.Empty;
            txtSumWipQtyMerger.Text = string.Empty;
            txtWipQtyMerger_Box.Text = string.Empty;
            txtSumWipQtyMerger_Box.Text = string.Empty;
        }
        #endregion

        #region Pallet 분할 
        private void btnSearchSplit_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Split(true);
        }
        //Pallet ID 엔터시
        private void txtPalletIdSplit_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdSplit.Text.Trim();
                    if (dgListSplit.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListSplit.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListSplit.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListSplit.SelectedIndex = i;
                                dgListSplit.ScrollIntoView(i, dgListSplit.Columns["CHK"].Index);
                                txtPalletIdSplit.Focus();
                                txtPalletIdSplit.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Split(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPalletSplit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PLT_TYPE_SPLIT == "SHIP_PLT")
                {
                    OpenSplitShipPltWindow("SPLIT_LOT");
                }
                else
                {
                    if (!Validation_Split())
                    {
                        return;
                    }
                    //분할하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU4175"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    PalletSprit();
                                }
                            });

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void txtAfterSplitQty_Box_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtAfterSplitQty_Box.Text, 0))
            {
                txtAfterSplitQty_Box.Focus();
                return;
            }
            if (dgSelectSplit.Rows.Count == 0)
            {
                return;
            }
        }
        private void btnReSetSplit_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListSplit);
            Util.gridClear(dgSelectSplit);
            txtTotalLotIDSplit.Text = string.Empty;
            txtBeforeSplitQty.Text = string.Empty;
            txtAfterSplitQty.Text = string.Empty;
            txtBeforeSplitQty_Box.Text = string.Empty;
            txtAfterSplitQty_Box.Text = string.Empty;
        }


        private void btnAdd_Split_Click(object sender, RoutedEventArgs e)
        {
            if (PLT_TYPE_SPLIT == "SHIP_PLT")
            {
                OpenSplitShipPltWindow("SPLIT_LOT");
            }
            else
            {
                DataGridRowAdd_Split(dgSelectSplit);
            }
        }

        public void OpenSplitShipPltWindow(string pAct)
        {
            try
            {
                FCS002_306_SPLIT_SHIP_PLT splitShipPltWindow = new FCS002_306_SPLIT_SHIP_PLT();
                splitShipPltWindow.FrameOperation = this.FrameOperation;

                object[] parameters = new object[3];
                if (pAct == "SPLIT_LOT_SELECT")
                {
                    parameters[0] = PALLETID_SELECT;
                    parameters[1] = LOTID_RT;
                }
                else
                {
                    parameters[0] = PALLETID_SPLIT;
                    parameters[1] = LOTID_RT_SPLIT;
                }
                parameters[2] = pAct;

                C1WindowExtension.SetParameters(splitShipPltWindow, parameters);

                splitShipPltWindow.Closed += new EventHandler(splitShipPltWindow_Closed);
                grdMain.Children.Add(splitShipPltWindow);
                splitShipPltWindow.BringToFront();

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }

        private void splitShipPltWindow_Closed(object sender, EventArgs e)
        {
            try
            {
                DataTable result = null;
                string act = string.Empty;

                FCS002_306_SPLIT_SHIP_PLT splitShipPltWindow = sender as FCS002_306_SPLIT_SHIP_PLT;

                if (splitShipPltWindow.DialogResult == MessageBoxResult.OK)
                {
                    result = splitShipPltWindow.RESULT;
                    act = splitShipPltWindow.ACT;
                }

                this.grdMain.Children.Remove(splitShipPltWindow);

                if (result != null)
                {
                    if (act == "SPLIT_LOT_SELECT")
                    {
                        GetLotList_Split_Select_Tag(result);
                    }
                    else
                    {
                        GetLotList_Split_Tag(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DataGridRowAdd_Split(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["LOTID_RT"] = LOTID_RT_SPLIT;
                dr2["WIPQTY"] = string.Empty;
                dr2["INBOX_QTY"] = string.Empty;
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                //dg.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dg, dt, FrameOperation, true);


                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDel_Split_Click(object sender, RoutedEventArgs e)
        {
            if (PLT_TYPE_SPLIT == "SHIP_PLT")
            {
                OpenSplitShipPltWindow("SPLIT_LOT");
            }
            else
            {
                DataGridRowRemove_Split(dgSelectSplit);
            }
        }

        private void txtInboxQty_Split_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

            if (!Util.CheckDecimal(txtBox.Text == string.Empty ? "0" : txtBox.Text, 0))
            {
                //txtBox.Text = string.Empty;
                DataTableConverter.SetValue(txtBox.DataContext, "INBOX_QTY", string.Empty);
                return;
            }

            Double InputBoxQty = Convert.ToDouble(txtBox.Text == string.Empty ? "0" : txtBox.Text);
            Double AllBoxQty = 0;
            if (dgSelectSplit.Rows.Count > 0)
            {

                for (int i = 0; i < dgSelectSplit.Rows.Count; i++)
                {
                    InputBoxQty = InputBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString());
                    AllBoxQty = AllBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString());
                }
            }
            if (Convert.ToDouble(txtBeforeSplitQty_Box.Text.Replace(",", "")) - InputBoxQty > 0)
            {
                txtAfterSplitQty_Box.Text = String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Box.Text.Replace(",", "")) - InputBoxQty);
            }
            else
            {
                txtAfterSplitQty_Box.Text = "0";

            }
            if (txtAfterSplitQty_Box.Text == txtBeforeSplitQty_Box.Text)
            {
                txtAfterSplitQty_Box.Text = string.Empty;
            }
        }

        #endregion

        #region Pallet 수량변경 
        private void btnSearchChange_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Change(true);
        }
        private void txtPalletIdChange_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdChange.Text.Trim();
                    if (dgListChange.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListChange.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListChange.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListChange.SelectedIndex = i;
                                dgListChange.ScrollIntoView(i, dgListChange.Columns["CHK"].Index);

                                txtPalletIdChange.Focus();
                                txtPalletIdChange.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Change(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void txtMQtyChange_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtMQtyChange.Text, 0))
            {
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Text = string.Empty;
                txtMQtyChange.Focus();
                return;
            }
            if (dgSelectChange.Rows.Count == 0)
            {
                return;
            }
            txtAfterChangeQty.Text = String.Format("{0:#,##0}", Convert.ToDouble(txtMQtyChange.Text) - Convert.ToDouble(txtWipQtyChange.Text));
        }
        private void txtAfterChangeQty_Box_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtMQtyChange_Box.Text, 0))
            {
                txtMQtyChange_Box.Text = string.Empty;
                txtAfterChangeQty_Box.Text = string.Empty;
                txtMQtyChange_Box.Focus();
                return;
            }
            if (dgSelectChange.Rows.Count == 0)
            {
                return;
            }
            txtAfterChangeQty_Box.Text = String.Format("{0:#,##0}", Convert.ToDouble(txtMQtyChange_Box.Text) - Convert.ToDouble(txtWipQtyChange_Box.Text));
        }
        private void btnPalletChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Change())
                {
                    return;
                }
                //수량변경하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4177"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                PalletChange();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnReSetChange_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListChange);
            Util.gridClear(dgSelectChange);
            txtTotalLotIDChange.Text = string.Empty;
            txtWipQtyChange.Text = string.Empty;
            txtMQtyChange.Text = string.Empty;
            txtAfterChangeQty.Text = string.Empty;
            txtChangeEtc.Text = string.Empty;
        }

        #endregion

        #region Pallet 선별 
        private void btnSearchSplit_Select_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Split_Select(true);
        }
        //Pallet ID 엔터시
        private void txtPalletIdSplit_Select_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdSplit_Select.Text.Trim();
                    if (dgListSplit_Select.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListSplit_Select.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListSplit_Select.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListSplit_Select.SelectedIndex = i;
                                dgListSplit_Select.ScrollIntoView(i, dgListSplit_Select.Columns["CHK"].Index);
                                txtPalletIdSplit_Select.Focus();
                                txtPalletIdSplit_Select.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Split_Select(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnPalletSplit_Select_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PLT_TYPE_SELECT == "SHIP_PLT")
                {
                    OpenSplitShipPltWindow("SPLIT_LOT_SELECT");
                }
                else
                {
                    if (!Validation_Split_Select())
                    {
                        return;
                    }
                    //선별하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU4231"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    PalletSprit_Select();
                                }
                            });

                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void txtAfterSplitQty_Box_Select_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Util.CheckDecimal(txtAfterSplitQty_Box_Select.Text, 0))
            {
                txtAfterSplitQty_Box_Select.Focus();
                return;
            }
            if (dgSelectSplit_Select.Rows.Count == 0)
            {
                return;
            }
        }
        private void btnReSetSplit_Select_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListSplit_Select);
            Util.gridClear(dgSelectSplit_Select);
            dgSelectSplit_Select.ItemsSource = null;
            txtTotalLotIDSplit_Select.Text = string.Empty;
            txtBeforeSplitQty_Select.Text = string.Empty;
            txtAfterSplitQty_Select.Text = string.Empty;
            txtBeforeSplitQty_Box_Select.Text = string.Empty;
            txtAfterSplitQty_Box_Select.Text = string.Empty;
        }


        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (PLT_TYPE_SELECT == "SHIP_PLT")
            {
                OpenSplitShipPltWindow("SPLIT_LOT_SELECT");
            }
            else
            {
                DataGridRowAdd(dgSelectSplit_Select);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (PLT_TYPE_SELECT == "SHIP_PLT")
            {
                OpenSplitShipPltWindow("SPLIT_LOT_SELECT");
            }
            else
            {
                DataGridRowRemove(dgSelectSplit_Select);
            }
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();

                if (dg.ItemsSource == null || dg.Rows.Count < 0)
                {
                    return;
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                dt = DataTableConverter.Convert(dg.ItemsSource);
                DataRow dr2 = dt.NewRow();
                dr2["WIPQTY"] = string.Empty;
                dr2["INBOX_QTY"] = string.Empty;
                dt.Rows.Add(dr2);
                dt.AcceptChanges();

                //dg.ItemsSource = DataTableConverter.Convert(dt);
                Util.GridSetData(dg, dt, FrameOperation, true);


                // 스프레드 스크롤 하단으로 이동
                dg.ScrollIntoView(dg.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }


        private void DataGridRowRemove(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    //dg.ItemsSource = DataTableConverter.Convert(dt);
                    Util.GridSetData(dg, dt, FrameOperation, true);
                    if (dgSelectSplit_Select.Rows.Count == 0)
                    {
                        txtAfterSplitQty_Select.Text = string.Empty;
                        txtAfterSplitQty_Box_Select.Text = string.Empty;
                        return;
                    }

                    Double AllWipQty = 0;
                    Double AllBoxQty = 0;
                    if (dgSelectSplit_Select.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgSelectSplit_Select.Rows.Count; i++)
                        {
                            AllWipQty = AllWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString());
                            AllBoxQty = AllBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString());
                        }
                    }
                    if (AllWipQty != 0)
                    {
                        txtAfterSplitQty_Select.Text = (Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) - AllWipQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) - AllWipQty);
                    }

                    if (AllBoxQty != 0)
                    {
                        if (Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - AllBoxQty > 0)
                        {
                            txtAfterSplitQty_Box_Select.Text = (Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - AllBoxQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - AllBoxQty);
                        }
                        else
                        {
                            txtAfterSplitQty_Box_Select.Text = "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }



        private void txtLotIDRT_Select_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;
                if (txtBox.Text != string.Empty)
                {

                    Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]+$");
                    Boolean ismatch = regex.IsMatch(txtBox.Text);
                    if (!ismatch)
                    {
                        txtBox.Focus();
                        Util.MessageValidation("SFU3674"); // 숫자와 영문대문자만 입력가능합니다.
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWipQty_Select_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

            if (!Util.CheckDecimal(txtBox.Text == string.Empty ? "0" : txtBox.Text, 0))
            {
                // txtBox.Text = string.Empty;
                DataTableConverter.SetValue(txtBox.DataContext, "WIPQTY", string.Empty);
                return;
            }

            Double InputWipQty = Convert.ToDouble(txtBox.Text == string.Empty ? "0" : txtBox.Text);
            Double AllWipQty = 0;
            if (dgSelectSplit_Select.Rows.Count > 0)
            {

                for (int i = 0; i < dgSelectSplit_Select.Rows.Count; i++)
                {
                    InputWipQty = InputWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString());
                    AllWipQty = AllWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY").ToString());
                }
            }


            if (Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) < InputWipQty)
            {
                Util.MessageValidation("SFU4230"); //입력Cell 수량의 합이 기존 Cell 수량보다 큽니다
                txtBox.Text = string.Empty;
                txtAfterSplitQty_Select.Text = (Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) - AllWipQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) - AllWipQty);

                if (txtAfterSplitQty_Select.Text == txtBeforeSplitQty_Select.Text)
                {
                    txtAfterSplitQty_Select.Text = string.Empty;
                }
                DataTableConverter.SetValue(txtBox.DataContext, "WIPQTY", string.Empty);
                return;
            }
            else
            {

                txtAfterSplitQty_Select.Text = String.Format("{0:#,##0}", Convert.ToDouble(Convert.ToDouble(txtBeforeSplitQty_Select.Text.Replace(",", "")) - InputWipQty));
                if (txtAfterSplitQty_Select.Text == txtBeforeSplitQty_Select.Text)
                {
                    txtAfterSplitQty_Select.Text = string.Empty;
                }

            }
        }

        private void txtInboxQty_Select_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

            if (!Util.CheckDecimal(txtBox.Text == string.Empty ? "0" : txtBox.Text, 0))
            {
                //txtBox.Text = string.Empty;
                DataTableConverter.SetValue(txtBox.DataContext, "INBOX_QTY", string.Empty);
                return;
            }

            Double InputBoxQty = Convert.ToDouble(txtBox.Text == string.Empty ? "0" : txtBox.Text);
            Double AllBoxQty = 0;
            if (dgSelectSplit_Select.Rows.Count > 0)
            {

                for (int i = 0; i < dgSelectSplit_Select.Rows.Count; i++)
                {
                    InputBoxQty = InputBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString());
                    AllBoxQty = AllBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY").ToString());
                }
            }
            if (Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - InputBoxQty > 0)
            {
                txtAfterSplitQty_Box_Select.Text = String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - InputBoxQty);
            }
            else
            {
                txtAfterSplitQty_Box_Select.Text = "0";

            }
            if (txtAfterSplitQty_Box_Select.Text == txtBeforeSplitQty_Box_Select.Text)
            {
                txtAfterSplitQty_Box_Select.Text = string.Empty;
            }

            //if (Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) < InputBoxQty)
            //{
            //    Util.MessageValidation("입력Box 수량의 합이 기존 Box 수량보다 큽니다"); //입력Cell 수량의 합이 기존 Cell 수량보다 큽니다
            //    txtBox.Text = string.Empty;
            //    txtAfterSplitQty_Box_Select.Text = (Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - AllBoxQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - AllBoxQty);

            //    if (txtAfterSplitQty_Box_Select.Text == txtBeforeSplitQty_Box_Select.Text)
            //    {
            //        txtAfterSplitQty_Box_Select.Text = string.Empty;
            //    }
            //    DataTableConverter.SetValue(txtBox.DataContext, "INBOX_QTY", string.Empty);
            //    return;
            //}
            //else
            //{

            //    txtAfterSplitQty_Box_Select.Text = String.Format("{0:#,##0}", Convert.ToDouble(Convert.ToDouble(txtBeforeSplitQty_Box_Select.Text.Replace(",", "")) - InputBoxQty));
            //    if (txtAfterSplitQty_Box_Select.Text == txtBeforeSplitQty_Box_Select.Text)
            //    {
            //        txtAfterSplitQty_Box_Select.Text = string.Empty;
            //    }

            //}
        }
        #endregion

        #region Pallet 이력 
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            if (cboWorkHistory.SelectedValue.ToString() == "MODIFY_WIPQTY")
            {
                GetLotList_Change_History();
            }
            else if (cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT" || cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT_MIX")
            {
                GetLotList_Merger_History();
            }
            else
            {
                GetLotList_Split_History();
            }
        }
        private void cboWorkHistory_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (cboWorkHistory.SelectedValue.ToString() == "MODIFY_WIPQTY")
                {
                    dgListMergerHistory.Visibility = Visibility.Collapsed;
                    dgListSplitHistory.Visibility = Visibility.Collapsed;
                    dgListChangeHistory.Visibility = Visibility.Visible;
                }
                else if (cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT" || cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT_MIX")
                {
                    dgListMergerHistory.Visibility = Visibility.Visible;
                    dgListSplitHistory.Visibility = Visibility.Collapsed;
                    dgListChangeHistory.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgListMergerHistory.Visibility = Visibility.Collapsed;
                    dgListSplitHistory.Visibility = Visibility.Visible;
                    dgListChangeHistory.Visibility = Visibility.Collapsed;
                }
            }));
        }



        #endregion



        #region [대상 선택하기]
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        #region Pallet 병합

        private void dgLotChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
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
                        dgSelectInput.SelectedIndex = idx;
                        txtTotalLotIDMerger.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[idx].DataItem, "PALLETID"));
                        txtWipQtyMerger.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[idx].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : string.Format("{0:#,##0}", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[idx].DataItem, "WIPQTY"))));
                        txtWipQtyMerger_Box.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[idx].DataItem, "INBOX_QTY")) == string.Empty ? Util.NVC(0) : string.Format("{0:#,##0}", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[idx].DataItem, "INBOX_QTY"))));

                        double MergerQty = 0;
                        double MergerQty_Box = 0;
                        for (int i = 0; i < dgSelectInput.Rows.Count; i++)
                        {
                            MergerQty = MergerQty + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")).ToString());
                            MergerQty_Box = MergerQty_Box + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INBOX_QTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "INBOX_QTY")).ToString());
                        }
                        txtSumWipQtyMerger.Text = String.Format("{0:#,##0}", MergerQty);
                        txtSumWipQtyMerger_Box.Text = String.Format("{0:#,##0}", MergerQty_Box);
                    }
                }
                else
                {
                    txtTotalLotIDMerger.Text = string.Empty;
                    txtWipQtyMerger.Text = string.Empty;
                    txtSumWipQtyMerger.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void dgListInput_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgListInput.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListInput.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgListInput.CurrentRow.Index;
            ////원각특성/Grade 공정일 경우 저항등급 보이게 하기
            //if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PROCID").ToString() == Process.CircularCharacteristicGrader)
            //{
            //    dgListInput.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            //    dgSelectInput.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    dgListInput.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            //    dgSelectInput.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            //}

            if (DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "CHK").Equals(1))
            {
                Seting_dgSelectInput(seleted_row);
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
                if (dtTo.Rows.Count > 0)
                {
                    if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0)
                    {
                        dtTo.Rows.Remove(dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                        dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);
                        //머지할 Pallet 재조정
                        DgSelectInputCheck();
                    }
                }
            }


        }




        private void txtSumWipQtyMerger_Box_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtSumWipQtyMerger.Text, 0))
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

        #region Pallet 분할
        private void dgLotSplitChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
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

                        //원각특성/Grade 공정일 경우 저항등급 보이게 하기
                        //if (DataTableConverter.GetValue(dgListSplit.Rows[idx].DataItem, "PROCID").ToString() == Process.CircularCharacteristicGrader)
                        //{
                        //    dgListSplit.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                        //    dgSelectSplit.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                        //}
                        //else
                        //{
                        //    dgListSplit.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                        //    dgSelectSplit.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                        //}


                        //row 색 바꾸기
                        dgListSplit.SelectedIndex = idx;
                        DataTable dtTo = new DataTable();
                        //dtTo.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                        dtTo.Columns.Add("PALLETID", typeof(string));
                        dtTo.Columns.Add("LOTID_RT", typeof(string));
                        dtTo.Columns.Add("PJT", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("INBOX_QTY", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(string));
                        dtTo.Columns.Add("UNIT", typeof(string));
                        dtTo.Columns.Add("LOTTYPE", typeof(string));
                        //dtTo.Columns.Add("SOC_VALUE", typeof(string));
                        //dtTo.Columns.Add("CAPA_GRD_CODE", typeof(string));
                        //dtTo.Columns.Add("VLTG_GRD_CODE", typeof(string));
                        //dtTo.Columns.Add("RSST_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("MKT_TYPE_NAME", typeof(string));
                        //dtTo.Columns.Add("WIPSEQ", typeof(string));
                        //dtTo.Columns.Add("WIPHOLD", typeof(string));
                        //dtTo.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                        dtTo.Columns.Add("EQPTID", typeof(string));
                        //dtTo.Columns.Add("PLT_TYPE", typeof(string));

                        DataRow dr = dtTo.NewRow();
                        foreach (DataColumn dc in dtTo.Columns)
                        {
                            dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListSplit.Rows[idx].DataItem, dc.ColumnName));
                        }
                        dtTo.Rows.Add(dr);
                        //dgSelectSplit.ItemsSource = DataTableConverter.Convert(dtTo);
                        //dgSelectSplit.SelectedIndex = 0;
                        Init_Split();
                        LOTID_RT_SPLIT = Util.NVC(dtTo.Rows[0]["LOTID_RT"]);
                        PALLETID_SPLIT = Util.NVC(dtTo.Rows[0]["PALLETID"]);
                        //PLT_TYPE_SPLIT = Util.NVC(dtTo.Rows[0]["PLT_TYPE"]);
                        //DataGridRowAdd_Split(dgSelectSplit);

                        DataTable dt = GetInitOutboxList(PALLETID_SPLIT);

                        // if (PLT_TYPE_SPLIT == "SHIP_PLT" || (dt != null && PALLETID_SPLIT.StartsWith("R")))
                        //{
                        //    OpenSplitShipPltWindow("SPLIT_LOT");
                        //}
                        //else
                        //{
                        //    DataGridRowAdd_Split(dgSelectSplit);
                        //}

                        OpenSplitShipPltWindow("SPLIT_LOT");

                        txtTotalLotIDSplit.Text = Util.NVC(dtTo.Rows[0]["PALLETID"]).ToString();
                        txtBeforeSplitQty.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(dtTo.Rows[0]["WIPQTY"]) == string.Empty ? Util.NVC(0) : Util.NVC_Int(dtTo.Rows[0]["WIPQTY"]).ToString()));
                        txtBeforeSplitQty_Box.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(dtTo.Rows[0]["INBOX_QTY"]) == string.Empty ? Util.NVC(0) : Util.NVC_Int(dtTo.Rows[0]["INBOX_QTY"]).ToString()));
                        txtAfterSplitQty.Text = string.Empty;
                        txtAfterSplitQty_Box.Text = string.Empty;
                    }
                }
                else
                {
                    txtTotalLotIDSplit.Text = string.Empty;
                    txtBeforeSplitQty.Text = string.Empty;
                    txtAfterSplitQty.Text = string.Empty;
                    txtBeforeSplitQty_Box.Text = string.Empty;
                    txtAfterSplitQty_Box.Text = string.Empty;
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

        #region Pallet 수량변경
        private void dgLotChangeChk_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
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

                        string sPallet = Util.NVC(DataTableConverter.GetValue(dgListChange.Rows[idx].DataItem, "PALLETID"));

                        DataTable dt = GetInitOutboxList(sPallet);

                            PLT_TYPE_CHANGE = Util.NVC(DataTableConverter.GetValue(dgListChange.Rows[idx].DataItem, "PLT_TYPE"));

                        // OUTBOX 매핑 된 박스 수량 변경 불가
                        if (PLT_TYPE_CHANGE == "SHIP_PLT" || (dt != null && sPallet.StartsWith("R")))
                        {
                            //출하 팔레트는 수량을 변경할 수 없습니다.
                            Util.MessageValidation("SFU3767");

                            txtTotalLotIDChange.Text = string.Empty;
                            txtWipQtyChange.Text = string.Empty;
                            txtMQtyChange.Text = string.Empty;
                            txtAfterChangeQty.Text = string.Empty;
                            txtChangeEtc.Text = string.Empty;
                            txtMQtyChange.Text = string.Empty;
                            txtAfterChangeQty.Text = string.Empty;
                            txtChangeEtc.Text = string.Empty;

                            return;
                        }

                        ////원각특성/Grade 공정일 경우 저항등급 보이게 하기
                        //if (DataTableConverter.GetValue(dgListChange.Rows[idx].DataItem, "PROCID").ToString() == Process.CircularCharacteristicGrader)
                        //{
                        //    dgListChange.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                        //    dgSelectChange.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
                        //}
                        //else
                        //{
                        //    dgListChange.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                        //    dgSelectChange.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
                        //}
                        //row 색 바꾸기
                        dgListChange.SelectedIndex = idx;
                        DataTable dtTo = new DataTable();
                        dtTo.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                        dtTo.Columns.Add("PALLETID", typeof(string));
                        dtTo.Columns.Add("LOTID_RT", typeof(string));
                        dtTo.Columns.Add("PJT", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("INBOX_QTY", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(string));
                        dtTo.Columns.Add("UNIT", typeof(string));
                        dtTo.Columns.Add("LOTTYPE", typeof(string));
                        dtTo.Columns.Add("SOC_VALUE", typeof(string));
                        dtTo.Columns.Add("CAPA_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("VLTG_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("RSST_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("MKT_TYPE_NAME", typeof(string));
                        dtTo.Columns.Add("WIPSEQ", typeof(string));
                        dtTo.Columns.Add("WIPHOLD", typeof(string));
                        dtTo.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                        dtTo.Columns.Add("EQPTID", typeof(string));
                        dtTo.Columns.Add("PROCID", typeof(string));
                        DataRow dr = dtTo.NewRow();
                        foreach (DataColumn dc in dtTo.Columns)
                        {
                            dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListChange.Rows[idx].DataItem, dc.ColumnName));
                        }
                        dtTo.Rows.Add(dr);
                        dgSelectChange.ItemsSource = DataTableConverter.Convert(dtTo);
                        dgSelectChange.SelectedIndex = 0;
                        txtTotalLotIDChange.Text = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "PALLETID")).ToString();
                        txtWipQtyChange.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "WIPQTY")).ToString()));
                        txtWipQtyChange_Box.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "INBOX_QTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "INBOX_QTY")).ToString()));
                        txtMQtyChange.Text = string.Empty;
                        txtAfterChangeQty.Text = string.Empty;
                        txtChangeEtc.Text = string.Empty;
                        txtMQtyChange_Box.Text = string.Empty;
                        txtAfterChangeQty_Box.Text = string.Empty;

                    }
                }
                else
                {
                    txtTotalLotIDChange.Text = string.Empty;
                    txtWipQtyChange.Text = string.Empty;
                    txtMQtyChange.Text = string.Empty;
                    txtAfterChangeQty.Text = string.Empty;
                    txtChangeEtc.Text = string.Empty;
                    txtMQtyChange.Text = string.Empty;
                    txtAfterChangeQty.Text = string.Empty;
                    txtChangeEtc.Text = string.Empty;

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

        #region Pallet 선별
        private void dgLotSplitChk_Select_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //(grdResult.Children[0] as UcAssyResult).dtpCaldate.SelectedDataTimeChanged -= OndtpCaldate_SelectedDataTimeChanged;
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
                        ////원각특성/Grade 공정일 경우 저항등급 보이게 하기
                        //if (DataTableConverter.GetValue(dgListSplit_Select.Rows[idx].DataItem, "PROCID").ToString() == Process.CircularCharacteristicGrader)
                        //{
                        //    dgListSplit_Select.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;

                        //}
                        //else
                        //{
                        //    dgListSplit_Select.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;

                        //}

                        //row 색 바꾸기
                        dgListSplit_Select.SelectedIndex = idx;
                        DataTable dtTo = new DataTable();
                        dtTo.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                        dtTo.Columns.Add("PALLETID", typeof(string));
                        dtTo.Columns.Add("LOTID_RT", typeof(string));
                        dtTo.Columns.Add("PJT", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("INBOX_QTY", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(string));
                        dtTo.Columns.Add("UNIT", typeof(string));
                        dtTo.Columns.Add("LOTTYPE", typeof(string));
                        dtTo.Columns.Add("SOC_VALUE", typeof(string));
                        dtTo.Columns.Add("CAPA_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("VLTG_GRD_CODE", typeof(string));
                        dtTo.Columns.Add("WIPSEQ", typeof(string));
                        dtTo.Columns.Add("WIPHOLD", typeof(string));
                        dtTo.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                        dtTo.Columns.Add("EQPTID", typeof(string));
                        dtTo.Columns.Add("PLT_TYPE", typeof(string));

                        DataRow dr = dtTo.NewRow();
                        foreach (DataColumn dc in dtTo.Columns)
                        {
                            dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListSplit_Select.Rows[idx].DataItem, dc.ColumnName));
                        }
                        dtTo.Rows.Add(dr);
                        //dgSelectSplit_Select.ItemsSource = DataTableConverter.Convert(dtTo);
                        //dgSelectSplit_Select.SelectedIndex = 0;
                        Init();

                        LOTID_RT = Util.NVC(dtTo.Rows[0]["LOTID_RT"]);
                        PALLETID_SELECT = Util.NVC(dtTo.Rows[0]["PALLETID"]);
                        PLT_TYPE_SELECT = Util.NVC(dtTo.Rows[0]["PLT_TYPE"]);

                        DataTable dt = GetInitOutboxList(PALLETID_SELECT);

                        if (PLT_TYPE_SELECT == "SHIP_PLT" || (dt != null && PALLETID_SELECT.StartsWith("R")))
                        {
                            OpenSplitShipPltWindow("SPLIT_LOT_SELECT");
                        }

                        txtTotalLotIDSplit_Select.Text = Util.NVC(dtTo.Rows[0]["PALLETID"]).ToString();
                        txtBeforeSplitQty_Select.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(dtTo.Rows[0]["WIPQTY"]) == string.Empty ? Util.NVC(0) : Util.NVC_Int(dtTo.Rows[0]["WIPQTY"]).ToString()));
                        txtBeforeSplitQty_Box_Select.Text = String.Format("{0:#,##0}", Convert.ToDouble(Util.NVC(dtTo.Rows[0]["INBOX_QTY"]) == string.Empty ? Util.NVC(0) : Util.NVC_Int(dtTo.Rows[0]["INBOX_QTY"]).ToString()));
                        txtAfterSplitQty_Select.Text = string.Empty;
                        txtAfterSplitQty_Box_Select.Text = string.Empty;
                    }
                }
                else
                {
                    dgSelectSplit_Select.ItemsSource = null;
                    txtTotalLotIDSplit_Select.Text = string.Empty;
                    txtBeforeSplitQty_Select.Text = string.Empty;
                    txtAfterSplitQty_Select.Text = string.Empty;
                    txtBeforeSplitQty_Box_Select.Text = string.Empty;
                    txtAfterSplitQty_Box_Select.Text = string.Empty;
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

        #endregion



        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region Pallet 병합
        public void GetLotList_Merger(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                if (Util.GetCondition(txtPalletIdMerger).Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentMerger, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessMerger, bAllNull: true);

                    dr["PJT_NAME"] = string.IsNullOrWhiteSpace(txtPrjtNameMerger.Text) ? null : txtPrjtNameMerger.Text;
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdidMerger.Text) ? null : txtProdidMerger.Text;//txtProdidMerger.Text;
                    dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotRTDMerger.Text) ? null : txtLotRTDMerger.Text;//txtLotRTDMerger.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    //dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQltyMerger, bAllNull: true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.GetCondition(txtPalletIdMerger);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    txtTotalLotIDMerger.Text = string.Empty;
                    txtWipQtyMerger.Text = string.Empty;
                    txtSumWipQtyMerger.Text = string.Empty;
                    txtWipQtyMerger_Box.Text = string.Empty;
                    txtSumWipQtyMerger_Box.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdMerger.Focus();
                            txtPalletIdMerger.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListInput.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInput);
                        Util.GridSetData(dgListInput, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInput.Rows[dgListInput.Rows.Count - 1].DataItem, "CHK", 1);
                        Seting_dgSelectInput(dgListInput.Rows.Count - 1);
                        txtPalletIdMerger.Text = string.Empty;
                        txtPalletIdMerger.Focus();
                    }
                    else
                    {

                        Util.gridClear(dgSelectInput);
                        Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    }

                    txtTotalLotIDMerger.Text = string.Empty;
                    txtWipQtyMerger.Text = string.Empty;
                    txtSumWipQtyMerger.Text = string.Empty;
                    txtWipQtyMerger_Box.Text = string.Empty;
                    txtSumWipQtyMerger_Box.Text = string.Empty;
                }
                else
                {
                    Util.GridSetData(dgListInput, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInput);
                    txtTotalLotIDMerger.Text = string.Empty;
                    txtWipQtyMerger.Text = string.Empty;
                    txtSumWipQtyMerger.Text = string.Empty;
                    txtWipQtyMerger_Box.Text = string.Empty;
                    txtSumWipQtyMerger_Box.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Merger_Tag()
        {

            try
            {
                DataTable Result = new DataTable();
                Result.Columns.Add("SPLIT_LOTID", typeof(string));



                FCS002_306_SPLIT popupTagPrint = new FCS002_306_SPLIT();
                popupTagPrint.FrameOperation = this.FrameOperation;

                DataRow drnewrow = Result.NewRow();
                drnewrow["SPLIT_LOTID"] = txtTotalLotIDMerger.Text;
                Result.Rows.Add(drnewrow);
                object[] parameters = new object[1];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = Result;
                }


                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                popupTagPrint.Closed += new EventHandler(popupTagPrint_Merge_Closed);
                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }


        }
        private void popupTagPrint_Merge_Closed(object sender, EventArgs e)
        {
            FCS002_306_SPLIT popupTagPrint = new FCS002_306_SPLIT();

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Util.gridClear(dgListInput);
                    Util.gridClear(dgSelectInput);
                    txtTotalLotIDMerger.Text = string.Empty;
                    txtWipQtyMerger.Text = string.Empty;
                    txtSumWipQtyMerger.Text = string.Empty;
                    GetLotList_Merger(true);
                }
            });
            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            this.grdMain.Children.Remove(popupTagPrint);
        }


        public void DgSelectInputCheck()
        {
            double MergerQty = 0;
            string CheckYN = "N";
            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "CHK")) == "True")
                {
                    CheckYN = "Y";
                    dgSelectInput.SelectedIndex = i;
                    break;
                }
            }
            if (CheckYN == "Y")
            {
                for (int i = 0; i < dgSelectInput.Rows.Count; i++)
                {
                    MergerQty = MergerQty + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "WIPQTY")).ToString());
                }
                txtSumWipQtyMerger.Text = String.Format("{0:#,##0}", MergerQty);
            }
            else
            {
                txtTotalLotIDMerger.Text = string.Empty;
                txtWipQtyMerger.Text = string.Empty;
                txtSumWipQtyMerger.Text = string.Empty;
            }
        }
        public void Seting_dgSelectInput(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInput.ItemsSource);
            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {

                dtTo.Columns.Add("CHK", typeof(Boolean));
                dtTo.Columns.Add("PALLETID", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("PJT", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("INBOX_QTY", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("LOTTYPE", typeof(string));
                dtTo.Columns.Add("MKT_TYPE_CODE", typeof(string));
                dtTo.Columns.Add("EQPTID", typeof(string));
                dtTo.Columns.Add("PROCID", typeof(string));
                dtTo.Columns.Add("S04", typeof(string));

            }
            if (dtTo.Rows.Count > 0)
            {
                if (dtTo.Select("LOTID_RT = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "LOTID_RT") + "'").Length == 0) //동일한 조립LOT
                {

                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4146"); //동일한 조립LOT이 아닙니다.
                    return;
                }
                if (dtTo.Select("PRODID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PRODID") + "'").Length == 0) //동일한 제품 체크
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4178"); //동일한 제품이 아닙니다.
                    return;
                }
                if (dtTo.Select("LOTTYPE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "LOTTYPE") + "'").Length == 0) //동일한 LOTTYPE이 아닙니다. 
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4179"); //동일한 LOTTYPE이 아닙니다.
                    return;
                }

                //if (dtTo.Select("WIP_QLTY_TYPE_NAME = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "WIP_QLTY_TYPE_NAME") + "'").Length == 0) //동일한 구분이 아닙니다. 
                //{
                //    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4180 "); //동일한 구분이 아닙니다.
                //    return;
                //}

                //if (dtTo.Select("SOC_VALUE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "SOC_VALUE") + "'").Length == 0) //동일한 SOC 정보가 아닙니다.
                //{
                //    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4181"); //동일한 SOC 정보가 아닙니다.
                //    return;
                //}

                //if (dtTo.Select("CAPA_GRD_CODE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "CAPA_GRD_CODE") + "'").Length == 0) //동일한 등급이 아닙니다. 
                //{
                //    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4182"); //동일한 등급이 아닙니다.
                //    return;
                //}

                //if (dtTo.Select("VLTG_GRD_CODE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "VLTG_GRD_CODE") + "'").Length == 0) //동일한 전압등급이 아닙니다
                //{
                //    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4183 "); //동일한 전압등급이 아닙니다
                //    return;
                //}
                //if (dtTo.Select("RSST_GRD_CODE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "RSST_GRD_CODE") + "'").Length == 0) //동일한 저항등급이 아닙니다
                //{
                //    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4270"); //동일한 저항등급이 아닙니다
                //    return;
                //}
                if (dtTo.Select("MKT_TYPE_CODE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "MKT_TYPE_CODE") + "'").Length == 0) //동일한 내수/수출이 아닙니다.
                {
                    DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4271"); //동일한 내수/수출이 아닙니다.
                    return;
                }
                if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0) //중복체크
                {
                    return;
                }

                //C20180109_77053  고객반품 인경우 체크
                //if ((DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE") != null &&
                //     DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE").Equals("FORM_WORK_CR")) ||
                //     dtTo.Select("FORM_WRK_TYPE_CODE = 'FORM_WORK_CR'").Length > 0)
                //{
                //    if (dtTo.Select("FORM_WRK_TYPE_CODE = '" + DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE") + "'").Length == 0) //
                //    {
                //        DataTableConverter.SetValue(dgListInput.Rows[seleted_row].DataItem, "CHK", 0);
                //        Util.MessageValidation("SFU4496"); //고객 반품 재작업 이외의 작업 유형과 병합 할 수 없습니다.
                //        return;
                //    }
                //}
            }

            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Boolean)))
                {
                    //dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    dr[dc.ColumnName] = 0;
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInput.Rows[seleted_row].DataItem, dc.ColumnName));
                }
            }

            dtTo.Rows.Add(dr);
            dgSelectInput.ItemsSource = DataTableConverter.Convert(dtTo);
            //머지할 Pallet 재조정
            DgSelectInputCheck();
        }






        #endregion

        #region Pallet 분할

        public void GetLotList_Split(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                //dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdSplit).Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentSplit, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessSplit, bAllNull: true);

                    dr["PJT_NAME"] = string.IsNullOrWhiteSpace(txtPrjtNameSplit.Text) ? null : txtPrjtNameSplit.Text;
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdidSplit.Text) ? null : txtProdidSplit.Text;
                    dr["LOTID_RT"] = string.IsNullOrWhiteSpace(txtLotRTDSplit.Text) ? null : txtLotRTDSplit.Text;
                    //dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQltySplit, bAllNull: true);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.GetCondition(txtPalletIdSplit);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListSplit, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectSplit);
                    txtTotalLotIDSplit.Text = string.Empty;
                    txtBeforeSplitQty.Text = string.Empty;
                    txtAfterSplitQty.Text = string.Empty;
                    txtBeforeSplitQty_Box.Text = string.Empty;
                    txtAfterSplitQty_Box.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdSplit.Focus();
                            txtPalletIdSplit.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListSplit.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListSplit);
                        //dgListSplit.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgListSplit, dtSource, FrameOperation, true);
                        txtPalletIdSplit.Text = string.Empty;
                        txtPalletIdSplit.Focus();
                    }
                    else
                    {

                        Util.gridClear(dgSelectSplit);
                        Util.GridSetData(dgListSplit, dtRslt, FrameOperation, true);
                    }

                    txtTotalLotIDSplit.Text = string.Empty;
                    txtBeforeSplitQty.Text = string.Empty;
                    txtAfterSplitQty.Text = string.Empty;
                    txtBeforeSplitQty_Box.Text = string.Empty;
                    txtAfterSplitQty_Box.Text = string.Empty;
                }
                else
                {
                    Util.GridSetData(dgListSplit, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectSplit);
                    txtTotalLotIDSplit.Text = string.Empty;
                    txtBeforeSplitQty.Text = string.Empty;
                    txtAfterSplitQty.Text = string.Empty;
                    txtBeforeSplitQty_Box.Text = string.Empty;
                    txtAfterSplitQty_Box.Text = string.Empty;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Split_Tag(DataTable Result)
        {
            try
            {
                FCS002_306_SPLIT popupTagPrint = new FCS002_306_SPLIT();
                popupTagPrint.FrameOperation = this.FrameOperation;

                DataRow drnewrow = Result.NewRow();
                drnewrow["SPLIT_LOTID"] = txtTotalLotIDSplit.Text;
                Result.Rows.Add(drnewrow);
                object[] parameters = new object[1];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = Result;
                }


                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        private void popupTagPrint_Closed(object sender, EventArgs e)
        {
            FCS002_306_SPLIT popupTagPrint = new FCS002_306_SPLIT();

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    Util.gridClear(dgListSplit);
                    Util.gridClear(dgSelectSplit);
                    txtTotalLotIDSplit.Text = string.Empty;
                    txtBeforeSplitQty.Text = string.Empty;
                    txtAfterSplitQty.Text = string.Empty;
                    txtBeforeSplitQty_Box.Text = string.Empty;
                    txtAfterSplitQty_Box.Text = string.Empty;
                    GetLotList_Split(true);
                }
            });
            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            this.grdMain.Children.Remove(popupTagPrint);
        }
        private void DataGridRowRemove_Split(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 기존 저장자료는 제외
                if (dg.SelectedIndex > -1)
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    dt.Rows[dg.SelectedIndex].Delete();
                    dt.AcceptChanges();
                    //dg.ItemsSource = DataTableConverter.Convert(dt);
                    Util.GridSetData(dg, dt, FrameOperation, true);
                    if (dgSelectSplit.Rows.Count == 0)
                    {
                        txtAfterSplitQty.Text = string.Empty;
                        txtAfterSplitQty_Box.Text = string.Empty;
                        return;
                    }

                    Double AllWipQty = 0;
                    Double AllBoxQty = 0;
                    if (dgSelectSplit.Rows.Count > 0)
                    {
                        for (int i = 0; i < dgSelectSplit.Rows.Count; i++)
                        {
                            AllWipQty = AllWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString());
                            AllBoxQty = AllBoxQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY").ToString());
                        }
                    }
                    if (AllWipQty != 0)
                    {
                        txtAfterSplitQty.Text = (Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) - AllWipQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) - AllWipQty);
                    }

                    if (AllBoxQty != 0)
                    {
                        if (Convert.ToDouble(txtBeforeSplitQty_Box.Text.Replace(",", "")) - AllBoxQty > 0)
                        {
                            txtAfterSplitQty_Box.Text = (Convert.ToDouble(txtBeforeSplitQty_Box.Text.Replace(",", "")) - AllBoxQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty_Box.Text.Replace(",", "")) - AllBoxQty);
                        }
                        else
                        {
                            txtAfterSplitQty_Box.Text = "0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void txtWipQty_Split_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;

            if (!Util.CheckDecimal(txtBox.Text == string.Empty ? "0" : txtBox.Text, 0))
            {
                // txtBox.Text = string.Empty;
                DataTableConverter.SetValue(txtBox.DataContext, "WIPQTY", string.Empty);
                return;
            }

            Double InputWipQty = Convert.ToDouble(txtBox.Text == string.Empty ? "0" : txtBox.Text);
            Double AllWipQty = 0;
            if (dgSelectSplit.Rows.Count > 0)
            {
                for (int i = 0; i < dgSelectSplit.Rows.Count; i++)
                {
                    InputWipQty = InputWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString());
                    AllWipQty = AllWipQty + Convert.ToDouble(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString() == string.Empty ? "0" : DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY").ToString());
                }
            }


            if (Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) < InputWipQty)
            {
                Util.MessageValidation("SFU4230"); //입력Cell 수량의 합이 기존 Cell 수량보다 큽니다
                txtBox.Text = string.Empty;
                txtAfterSplitQty.Text = (Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) - AllWipQty).ToString() == "0" ? string.Empty : String.Format("{0:#,##0}", Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) - AllWipQty);

                if (txtAfterSplitQty.Text == txtBeforeSplitQty.Text)
                {
                    txtAfterSplitQty.Text = string.Empty;
                }
                DataTableConverter.SetValue(txtBox.DataContext, "WIPQTY", string.Empty);
                return;
            }
            else
            {
                txtAfterSplitQty.Text = String.Format("{0:#,##0}", Convert.ToDouble(Convert.ToDouble(txtBeforeSplitQty.Text.Replace(",", "")) - InputWipQty));
                if (txtAfterSplitQty.Text == txtBeforeSplitQty.Text)
                {
                    txtAfterSplitQty.Text = string.Empty;
                }

            }
        }
        void Init_Split()
        {
            dgSelectSplit.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("LOTID_RT", typeof(string));
            emptyTransferTable.Columns.Add("WIPQTY", typeof(string));
            emptyTransferTable.Columns.Add("INBOX_QTY", typeof(string));
            dgSelectSplit.ItemsSource = DataTableConverter.Convert(emptyTransferTable);


        }

        #endregion

        #region Pallet 수량변경
        public void GetLotList_Change(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdChange).Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentChange, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessChange, bAllNull: true);

                    dr["PJT_NAME"] = txtPrjtNameChange.Text;
                    dr["PRODID"] = txtProdidChange.Text;
                    dr["LOTID_RT"] = txtLotRTDChange.Text;
                    dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQltyChange, bAllNull: true);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.GetCondition(txtPalletIdChange);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListChange, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectChange);
                    txtTotalLotIDChange.Text = string.Empty;
                    txtWipQtyChange.Text = string.Empty;
                    txtMQtyChange.Text = string.Empty;
                    txtAfterChangeQty.Text = string.Empty;
                    txtWipQtyChange_Box.Text = string.Empty;
                    txtMQtyChange_Box.Text = string.Empty;
                    txtAfterChangeQty_Box.Text = string.Empty;
                    txtChangeEtc.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdChange.Focus();
                            txtPalletIdChange.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListChange.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListChange);
                        //dgListSplit.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgListChange, dtSource, FrameOperation, true);
                        txtPalletIdChange.Text = string.Empty;
                        txtPalletIdChange.Focus();
                    }
                    else
                    {
                        Util.gridClear(dgSelectChange);
                        Util.GridSetData(dgListChange, dtRslt, FrameOperation, true);
                    }
                    txtTotalLotIDChange.Text = string.Empty;
                    txtWipQtyChange.Text = string.Empty;
                    txtMQtyChange.Text = string.Empty;
                    txtAfterChangeQty.Text = string.Empty;
                    txtWipQtyChange_Box.Text = string.Empty;
                    txtMQtyChange_Box.Text = string.Empty;
                    txtAfterChangeQty_Box.Text = string.Empty;
                    txtChangeEtc.Text = string.Empty;
                }
                else
                {
                    Util.GridSetData(dgListChange, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectChange);
                    txtTotalLotIDChange.Text = string.Empty;
                    txtWipQtyChange.Text = string.Empty;
                    txtMQtyChange.Text = string.Empty;
                    txtAfterChangeQty.Text = string.Empty;
                    txtWipQtyChange_Box.Text = string.Empty;
                    txtMQtyChange_Box.Text = string.Empty;
                    txtAfterChangeQty_Box.Text = string.Empty;
                    txtChangeEtc.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Change_Tag()
        {
            try
            {
                CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
                popupTagPrint.QMSRequestPalletYN = "Y";
                // SET PARAMETER

                object[] parameters = new object[8];
                parameters[0] = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "PROCID"));
                parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "EQPTID"));
                parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "PALLETID"));
                parameters[3] = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "WIPSEQ")).ToString();
                parameters[4] = txtMQtyChange.Text.ToString();
                parameters[5] = "N";      // 디스패치 처리
                parameters[6] = Util.NVC(DataTableConverter.GetValue(dgSelectChange.Rows[0].DataItem, "PROC_LABEL_PRT_FLAG"));
                parameters[7] = "Y";      // Direct 출력 여부

                C1WindowExtension.SetParameters(popupTagPrint, parameters);
                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Util.gridClear(dgListChange);
                Util.gridClear(dgSelectChange);
                txtTotalLotIDChange.Text = string.Empty;
                txtWipQtyChange.Text = string.Empty;
                txtMQtyChange.Text = string.Empty;
                txtAfterChangeQty.Text = string.Empty;
                txtChangeEtc.Text = string.Empty;
                GetLotList_Change(true);
            }
        }
        #endregion

        #region Pallet 선별
        public void GetLotList_Split_Select(bool bButton)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                if (Util.GetCondition(txtPalletIdSplit_Select).Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentSplit_Select, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessSplit_Select, bAllNull: true);

                    dr["PJT_NAME"] = txtPrjtNameSplit_Select.Text;
                    dr["PRODID"] = txtProdidSplit_Select.Text;
                    dr["LOTID_RT"] = txtLotRTDSplit_Select.Text;
                    dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQltySplit_Select, bAllNull: true);
                    dr["LANGID"] = LoginInfo.LANGID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.GetCondition(txtPalletIdSplit_Select);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListSplit_Select, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectSplit_Select);
                    txtTotalLotIDSplit_Select.Text = string.Empty;
                    txtBeforeSplitQty_Select.Text = string.Empty;
                    txtAfterSplitQty_Select.Text = string.Empty;
                    txtBeforeSplitQty_Box_Select.Text = string.Empty;
                    txtAfterSplitQty_Box_Select.Text = string.Empty;
                    dgSelectSplit_Select.ItemsSource = null;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    //Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdSplit_Select.Focus();
                            txtPalletIdSplit_Select.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListSplit_Select.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListSplit_Select);
                        //dgListSplit.ItemsSource = DataTableConverter.Convert(dtSource);
                        Util.GridSetData(dgListSplit_Select, dtSource, FrameOperation, true);
                        txtPalletIdSplit_Select.Text = string.Empty;
                        txtPalletIdSplit_Select.Focus();
                    }
                    else
                    {

                        Util.gridClear(dgSelectSplit_Select);
                        Util.GridSetData(dgListSplit_Select, dtRslt, FrameOperation, true);
                    }

                    txtTotalLotIDSplit_Select.Text = string.Empty;
                    txtBeforeSplitQty_Select.Text = string.Empty;
                    txtAfterSplitQty_Select.Text = string.Empty;
                    txtBeforeSplitQty_Box_Select.Text = string.Empty;
                    txtAfterSplitQty_Box_Select.Text = string.Empty;
                    dgSelectSplit_Select.ItemsSource = null;
                }
                else
                {
                    Util.GridSetData(dgListSplit_Select, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectSplit_Select);
                    txtTotalLotIDSplit_Select.Text = string.Empty;
                    txtBeforeSplitQty_Select.Text = string.Empty;
                    txtAfterSplitQty_Select.Text = string.Empty;
                    txtBeforeSplitQty_Box_Select.Text = string.Empty;
                    txtAfterSplitQty_Box_Select.Text = string.Empty;
                    dgSelectSplit_Select.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Split_Select_Tag(DataTable Result)
        {
            try
            {
                FCS002_306_SELECT_TAG popupTagPrint = new FCS002_306_SELECT_TAG();
                popupTagPrint.FrameOperation = this.FrameOperation;

                DataRow drnewrow = Result.NewRow();
                drnewrow["SPLIT_LOTID"] = txtTotalLotIDSplit_Select.Text.Trim();
                Result.Rows.Add(drnewrow);
                object[] parameters = new object[1];

                if (Result.Rows.Count > 0)
                {
                    parameters[0] = Result;
                }


                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                popupTagPrint.Closed += new EventHandler(popupTagPrint_Select_Closed);
                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {

            }
        }
        private void popupTagPrint_Select_Closed(object sender, EventArgs e)
        {
            FCS002_306_SELECT_TAG popupTagPrint = new FCS002_306_SELECT_TAG();

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    Util.gridClear(dgListSplit_Select);
                    Util.gridClear(dgSelectSplit_Select);
                    txtTotalLotIDSplit_Select.Text = string.Empty;
                    txtBeforeSplitQty_Select.Text = string.Empty;
                    txtAfterSplitQty_Select.Text = string.Empty;
                    txtBeforeSplitQty_Box_Select.Text = string.Empty;
                    txtAfterSplitQty_Box_Select.Text = string.Empty;
                    GetLotList_Split_Select(true);
                }
            });
            //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            this.grdMain.Children.Remove(popupTagPrint);
        }

        void Init()
        {
            dgSelectSplit_Select.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("LOTID_RT", typeof(string));
            emptyTransferTable.Columns.Add("WIPQTY", typeof(string));
            emptyTransferTable.Columns.Add("INBOX_QTY", typeof(string));
            dgSelectSplit_Select.ItemsSource = DataTableConverter.Convert(emptyTransferTable);


        }
        #endregion

        #endregion

        #region [이력 가져오기]

        public void GetLotList_Merger_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(DateTime));
                dtRqst.Columns.Add("TO_DATE", typeof(DateTime));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("TYPE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Convert.ToDateTime(ldpDateFromHist.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00");
                dr["TO_DATE"] = Convert.ToDateTime(ldpDateToHist.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59");
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["TYPE"] = Util.GetCondition(cboWorkHistory, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory.Text;
                dr["PRODID"] = txtProdID.Text;
                dr["LOTID_RT"] = txtLotHistory.Text;
                dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_MERGER_SPLIT_HISTORY_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgListMergerHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }
                //dgListMergerHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListMergerHistory, dtRslt, FrameOperation, true);

                //string[] sColumnName = new string[] { "ACTDTTM", "WRK_SUPPLIERID", "SHIFT", "WRK_USER_NAME", "ACTNAME", "LOTID_RT", "PRJT_NAME", "PRODID",  "FROM_LOTID","QTY", "TO_LOTID","WIPQTY2" };
                //_Util.SetDataGridMergeExtensionCol(dgListMergerHistory, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                string[] sColumnName = new string[] { "ACTDTTM", "LOTID_RT", "PRJT_NAME", "PRODID", "MKT_TYPE_NAME", "CHK", "TO_LOTID", "WIPQTY2", "FROM_LOTID", "QTY" };
                _Util.SetDataGridMergeExtensionCol(dgListMergerHistory, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                dgListMergerHistory.Columns["CHK"].DisplayIndex = 0;
                dgListMergerHistory.Columns["TO_LOTID"].DisplayIndex = 1;
                dgListMergerHistory.Columns["WIPQTY2"].DisplayIndex = 2;
                dgListMergerHistory.Columns["LOTID_RT"].DisplayIndex = 3;
                dgListMergerHistory.Columns["PRJT_NAME"].DisplayIndex = 4;
                dgListMergerHistory.Columns["PRODID"].DisplayIndex = 5;
                dgListMergerHistory.Columns["MKT_TYPE_NAME"].DisplayIndex = 6;
                dgListMergerHistory.Columns["ACTDTTM"].DisplayIndex = 7;
                dgListMergerHistory.Columns["FROM_LOTID"].DisplayIndex = 8;
                dgListMergerHistory.Columns["QTY"].DisplayIndex = 9;
                dgListMergerHistory.Columns["WIPSEQ"].DisplayIndex = 10;
                dgListMergerHistory.Columns["PROCID"].DisplayIndex = 11;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetLotList_Split_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("TYPE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["TYPE"] = Util.GetCondition(cboWorkHistory, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory.Text;
                dr["PRODID"] = txtProdID.Text;
                dr["LOTID_RT"] = txtLotHistory.Text;
                dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_SPLIT_HISTORY", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgListSplitHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }
                //dgListSplitHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgListSplitHistory, dtRslt, FrameOperation, true);
                //string[] sColumnName = new string[] { "ACTDTTM", "WRK_SUPPLIERID", "SHIFT", "WRK_USER_NAME", "ACTNAME", "LOTID_RT", "PRJT_NAME", "PRODID",  "FROM_LOTID","QTY", "TO_LOTID","WIPQTY2" };
                //_Util.SetDataGridMergeExtensionCol(dgListMergerHistory, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                string[] sColumnName = new string[] { "ACTDTTM", "LOTID_RT", "PRJT_NAME", "PRODID", "MKT_TYPE_NAME", "CHK", "FROM_LOTID", "QTY", "TO_LOTID", "WIPQTY2" };
                _Util.SetDataGridMergeExtensionCol(dgListSplitHistory, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

                dgListSplitHistory.Columns["CHK"].DisplayIndex = 0;
                dgListSplitHistory.Columns["FROM_LOTID"].DisplayIndex = 1;
                dgListSplitHistory.Columns["QTY"].DisplayIndex = 2;
                dgListSplitHistory.Columns["LOTID_RT"].DisplayIndex = 3;
                dgListSplitHistory.Columns["PRJT_NAME"].DisplayIndex = 4;
                dgListSplitHistory.Columns["PRODID"].DisplayIndex = 5;
                dgListSplitHistory.Columns["MKT_TYPE_NAME"].DisplayIndex = 6;
                dgListSplitHistory.Columns["ACTDTTM"].DisplayIndex = 7;
                dgListSplitHistory.Columns["TO_LOTID"].DisplayIndex = 8;
                dgListSplitHistory.Columns["WIPQTY2"].DisplayIndex = 9;
                dgListSplitHistory.Columns["WIPSEQ"].DisplayIndex = 10;
                dgListSplitHistory.Columns["PROCID"].DisplayIndex = 11;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        public void GetLotList_Change_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("FROM_DATE", typeof(String));
                dtRqst.Columns.Add("TO_DATE", typeof(String));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("TYPE", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                //ldpDateFromHist.SelectedDateTime.ToShortDateString();
                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return;
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                dr["TYPE"] = Util.GetCondition(cboWorkHistory, bAllNull: true);
                dr["PJT_NAME"] = txtPjtHistory.Text;
                dr["PRODID"] = txtProdID.Text;
                dr["LOTID_RT"] = txtLotHistory.Text;
                dr["LOTID"] = Util.GetCondition(txtPalletHistory);
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_CHANGE_QTY_HISTORY", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                    dgListChangeHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                    return;
                }
                //dgListChangeHistory.ItemsSource = DataTableConverter.Convert(dtRslt);
                Util.GridSetData(dgListChangeHistory, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion

        #region 병합/분할/변경/선별/이력 정보 셋팅

        #region [Pallet 병합]
        private void PalletMerger()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("BOXID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["USERID"] = LoginInfo.USERID;
            row["BOXID"] = txtTotalLotIDMerger.Text;

            inDataTable.Rows.Add(row);

            //병합PalletLot
            DataTable inLot = inData.Tables.Add("INPALLET");
            inLot.Columns.Add("BOXID", typeof(string));


            for (int i = 0; i < dgSelectInput.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "CHK")) == "False")
                {
                    row = inLot.NewRow();
                    row["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInput.Rows[i].DataItem, "PALLETID"));
                    inLot.Rows.Add(row);
                }
            }
            try
            {
                //Pallet병합처리
                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_PALLET", "INDATA,INLOT", null, (Result, ex) =>
                //{
                //    if (ex != null)
                //    {
                //        Util.MessageException(ex);
                //        return;
                //    }
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                //    {
                //        GetLotList_Merger_Tag();
                //    });
                //    return;

                // }, inData);
                //Pallet 재구성 Merge
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_PALLET_MB", "INDATA,INPALLET", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    GetLotList_Merger_Tag();


                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MERGE_PALLET", ex.Message, ex.ToString());

            }
        }

        #endregion

        #region [Pallet 분할]
        private void PalletSprit()
        {
            //DataSet inData = new DataSet();

            ////마스터 정보
            //DataTable inDataTable = inData.Tables.Add("INDATA");
            //inDataTable.Columns.Add("SRCTYPE", typeof(string));
            //inDataTable.Columns.Add("IFMODE", typeof(string));
            //inDataTable.Columns.Add("PALLETID", typeof(string));
            //inDataTable.Columns.Add("SPLIT_WIPQTY", typeof(decimal));
            //inDataTable.Columns.Add("FROM_INBOX_QTY", typeof(decimal));
            //inDataTable.Columns.Add("INBOX_QTY", typeof(decimal));
            //inDataTable.Columns.Add("USERID", typeof(string));

            //DataRow row = null;

            //row = inDataTable.NewRow();
            //row["SRCTYPE"] = "UI";
            //row["IFMODE"] = "OFF";
            //row["PALLETID"] = txtTotalLotIDSplit.Text;
            ////row["SPLIT_WIPQTY"] = Convert.ToDecimal(txtSplitQty.Text);
            //if (txtAfterSplitQty_Box.Text != string.Empty)
            //{
            //    row["FROM_INBOX_QTY"] = Convert.ToDecimal(txtAfterSplitQty_Box.Text);
            //}
            ////if (txtSplitQty_Box.Text != string.Empty)
            ////{
            ////    row["INBOX_QTY"] = Convert.ToDecimal(txtSplitQty_Box.Text);
            ////}
            //row["USERID"] = LoginInfo.USERID;

            //inDataTable.Rows.Add(row);


            //try
            //{
            //    //Pallet병합처리
            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_PALLET", "INDATA", "OUTDATA", (Result, ex) =>
            //    {
            //        if (ex != null)
            //        {
            //            Util.MessageException(ex);
            //            return;
            //        }
            //        GetLotList_Split_Tag(Result.Tables[1]);

            //    }, inData);



            //}
            //catch (Exception ex)
            //{
            //    Util.AlertByBiz("BR_PRD_REG_SPLIT_PALLET", ex.Message, ex.ToString());

            //}
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PALLETID", typeof(string));
            inDataTable.Columns.Add("FROM_INBOX_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PALLETID"] = txtTotalLotIDSplit.Text;
            if (txtAfterSplitQty_Box.Text != string.Empty)
            {
                row["FROM_INBOX_QTY"] = Convert.ToDecimal(txtAfterSplitQty_Box.Text.Replace(",", ""));
            }
            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);
            //선별 PalletLot
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("ACTQTY", typeof(decimal));
            inLot.Columns.Add("INBOX_QTY", typeof(decimal));


            for (int i = 0; i < dgSelectSplit.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "WIPQTY")));
                if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY")).ToString() != string.Empty)
                {
                    row["INBOX_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectSplit.Rows[i].DataItem, "INBOX_QTY")));
                }

                inLot.Rows.Add(row);
            }
            try
            {
                //Pallet 재구성 Split
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_PALLET", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //2020-02-03 최상민 수정 Tables[1] -> Tables["OUTDATA"]
                    GetLotList_Split_Tag(Result.Tables["OUTDATA"]);

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SPLIT_PALLET", ex.Message, ex.ToString());

            }
        }

        #endregion

        #region [Pallet 수량변경]
        private void PalletChange()
        {
            DataSet inData = new DataSet();

            //마스터 정보

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("PALLETID", typeof(string));
            inDataTable.Columns.Add("WIPQTY", typeof(decimal));
            inDataTable.Columns.Add("INBOX_QTY", typeof(decimal));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PROCID"] = cboProcessChange.SelectedValue.ToString();
            row["PALLETID"] = txtTotalLotIDChange.Text;
            row["WIPQTY"] = Convert.ToDecimal(txtMQtyChange.Text);
            if (txtMQtyChange_Box.Text != string.Empty)
            {
                row["INBOX_QTY"] = Convert.ToDecimal(txtMQtyChange_Box.Text);
            }
            row["RESNCODE"] = cboChangeReason.SelectedValue.ToString();
            row["WIPNOTE"] = txtChangeEtc.Text;
            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);


            try
            {
                //Pallet수량변경
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_WIPQTY_PALLET", "INDATA", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetLotList_Change_Tag();
                    });
                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MODIFY_WIPQTY_PALLE", ex.Message, ex.ToString());

            }
        }

        #endregion

        #region [Pallet 선별]
        private void PalletSprit_Select()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PALLETID", typeof(string));
            inDataTable.Columns.Add("FROM_INBOX_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACTID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PALLETID"] = txtTotalLotIDSplit_Select.Text;
            if (txtAfterSplitQty_Box_Select.Text != string.Empty)
            {
                row["FROM_INBOX_QTY"] = Convert.ToDecimal(txtAfterSplitQty_Box_Select.Text.Replace(",", ""));
            }
            row["USERID"] = LoginInfo.USERID;
            row["ACTID"] = "SPLIT_LOT_SELECT";

            inDataTable.Rows.Add(row);
            //선별 PalletLot
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("ACTQTY", typeof(decimal));
            inLot.Columns.Add("INBOX_QTY", typeof(decimal));
            inLot.Columns.Add("ASSY_LOTID", typeof(string));

            for (int i = 0; i < dgSelectSplit_Select.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY")));
                if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY")).ToString() != string.Empty)
                {
                    row["INBOX_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "INBOX_QTY")));
                }
                row["ASSY_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "LOTID_RT"));
                inLot.Rows.Add(row);
            }
            try
            {
                //Pallet선별처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_ASSY_LOT", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //2020-02-03 최상민 수정 Tables[1] -> Tables["OUTDATA"]
                    GetLotList_Split_Select_Tag(Result.Tables["OUTDATA"]);

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_SPLIT_ASSY_LOT", ex.Message, ex.ToString());

            }
        }

        #endregion

        #endregion


        #endregion

        #region [Validation]

        #region [Pallet병합]
        private bool Validation()
        {
            if (txtTotalLotIDMerger.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU4232"); //대표Lot을 선택하세요.
                return false;
            }

            if (txtSumWipQtyMerger.Text.Trim() == "0")
            {
                Util.MessageValidation("SFU4233"); //병합 Cell 수량이 0 입니다.
                return false;
            }
            if (dgSelectInput.Rows.Count == 1)
            {
                Util.MessageValidation("SFU4234"); //병합할 대상은 2개 이상이어야 합니다.
                return false;
            }
            //if (txtSumWipQtyMerger_Box.Text.Trim().Length == 0)
            //{
            //    Util.MessageValidation("병합 BOX 수량을 입력하세요."); //병합 BOX 수량을 입력하세요.
            //return false;
            //}
            if (txtSumWipQtyMerger_Box.Text.Trim() == "0")
            {
                Util.MessageValidation("SFU4235"); //병합 BOX 수량에 0을 입력할 수 없습니다.
                return false;
            }
            return true;
        }
        #endregion

        #region [Pallet분할]
        private bool Validation_Split()
        {

            //if (txtSplitQty.Text.Trim().Length == 0)
            //{
            //    Util.MessageValidation("SFU4184"); //분할수량을 입력하세요.
            //    return false;
            //}
            //if (txtSplitQty.Text.Trim() == "0")
            //{
            //    Util.MessageValidation("SFU4185"); //분할수량을 입력하세요.
            //    return false;
            //}
            //if (txtSplitQty_Box.Text.Trim().Length == 0)
            //{
            //    Util.MessageValidation("분할 BOX 수량을 입력하세요"); //분할 BOX 수량을 입력하세요.
            //    return false;
            //}
            //if (txtAfterSplitQty_Box.Text.Trim().Length == 0)
            //{
            //    Util.MessageValidation("분할후  BOX 수량을 입력하세요"); //분할 BOX 수량을 입력하세요.
            //    return false;
            //}
            if (txtAfterSplitQty_Box.Text.Trim() == "0")
            {
                Util.MessageValidation("SFU4186"); //분할후  BOX 수량은 0 이상입니다.
                return false;
            }
            return true;
        }
        #endregion

        #region [Pallet수량변경]
        private bool Validation_Change()
        {

            if (PLT_TYPE_CHANGE == "SHIP_PLT")
            {
                Util.MessageValidation("SFU3767"); //출하 팔레트는 수량을 변경할 수 없습니다.
                return false;
            }

            if (txtMQtyChange.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU4187"); //변경수량을 입력하세요.
                return false;
            }
            //if (txtMQtyChange.Text.Trim() == "0")
            //{
            //    Util.MessageValidation("SFU4188"); //변경수량은 0 이상입니다.
            //    return false;
            //}
            if (cboChangeReason.SelectedValue.ToString() == "SELECT")
            {
                Util.MessageValidation("SFU4189"); //변경사유를 선택하세요.
                return false;
            }
            //if (txtMQtyChange_Box.Text.Trim().Length == 0)
            //{
            //    Util.MessageValidation("변경 Box수량 입력하세요"); //변경 Boxx수량을 입력하세요.
            //    return false;
            //}
            //if (txtMQtyChange_Box.Text.Trim() == "0")
            //{
            //    Util.MessageValidation("SFU4190"); //"변경 Box수량은 0 이상입니다."
            //    return false;
            //}
            return true;
        }


        #endregion

        #region [Pallet선별]
        private bool Validation_Split_Select()
        {

            if (dgSelectSplit_Select.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4163"); //대상목록정보가 없습니다.
                return false;
            }

            for (int i = 0; i < dgSelectSplit_Select.Rows.Count; i++)
            {
                int DubLot = 0;
                if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "LOTID_RT")).ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU4225"); //조립LOT 정보가 없는 데이터가 존재합니다.
                    dgSelectSplit_Select.SelectedIndex = i;
                    return false;
                }
                for (int j = 0; j < dgSelectSplit_Select.Rows.Count; j++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "LOTID_RT")).ToString() == Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[j].DataItem, "LOTID_RT").ToString()))
                    {
                        DubLot = DubLot + 1;
                    }
                }

                if (DubLot > 1) //중복LOT Check 
                {
                    Util.MessageValidation("SFU4226"); //조립LOT 정보가 없는 데이터가 존재합니다.
                    return false;
                }


                if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "WIPQTY")).ToString() == string.Empty)
                {
                    Util.MessageValidation("SFU4227"); //조립LOT 정보가 없는 데이터가 존재합니다.
                    dgSelectSplit_Select.SelectedIndex = i;
                    return false;
                }
                if (LOTID_RT.Length == 8) //원형
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "LOTID_RT")).ToString().Trim().Length != 8)
                    {
                        Util.MessageValidation("SFU4228"); //조립LOT 정보는 8자리 입니다.
                        dgSelectSplit_Select.SelectedIndex = i;
                        return false;
                    }
                }
                else if (LOTID_RT.Length == 10) // 초소형
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSelectSplit_Select.Rows[i].DataItem, "LOTID_RT")).ToString().Trim().Length != 10)
                    {
                        Util.MessageValidation("SFU4229"); //조립LOT 정보는 10자리 입니다.
                        dgSelectSplit_Select.SelectedIndex = i;
                        return false;
                    }
                }
                // 중복LOT 체크

            }




            return true;
        }
        #endregion

        #endregion

        private void btnPrintHistory_Click(object sender, RoutedEventArgs e)
        {


            if (cboWorkHistory.SelectedValue.ToString() == "MODIFY_WIPQTY")
            {
                DataRow[] drSelectChange = DataTableConverter.Convert(dgListChangeHistory.ItemsSource).Select("CHK = 'True'");

                if (drSelectChange.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                foreach (DataRow drPrint in drSelectChange)
                {
                    TagPrint(drPrint);
                }
            }
            else if (cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT" || cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT_MIX")
            {
                DataRow[] drSelectHistory = DataTableConverter.Convert(dgListMergerHistory.ItemsSource).Select("CHK = 'True'");

                if (drSelectHistory.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                foreach (DataRow drPrint in drSelectHistory)
                {
                    TagPrint(drPrint);
                }
            }
            else
            {
                DataRow[] drSelectSplit = DataTableConverter.Convert(dgListSplitHistory.ItemsSource).Select("CHK = 'True'");

                if (drSelectSplit.Length == 0)
                {
                    Util.MessageValidation("SFU1651");
                    return;
                }

                foreach (DataRow drPrint in drSelectSplit)
                {
                    TagPrint(drPrint);
                }
            }
        }


        private void TagPrint(DataRow dr)
        {
            CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;

            if (dr["WIP_QLTY_TYPE_CODE"].ToString() == "N")
            {
                popupTagPrint.DefectPalletYN = "Y";
            }
            else
            {
                //남경일경우
                if (LoginInfo.CFG_SHOP_ID == "G182")
                {
                    //특성 Grader
                    if (dr["PROCID"].Equals(Process.CircularCharacteristicGrader))
                    {
                        popupTagPrint.QMSRequestPalletYN = "Y";
                    }
                    else
                    {
                        // 초소형 일경우 오창이랑 동일함
                        if (dr["CLSS3_CODE"].ToString() == "MCS")
                        {
                            popupTagPrint.QMSRequestPalletYN = "Y";
                        }
                        else
                        {
                            popupTagPrint.returnPalletYN = "Y";
                        }

                    }

                }
                else
                {
                    popupTagPrint.QMSRequestPalletYN = "Y";
                }
            }
            object[] parameters = new object[8];
            parameters[0] = dr["PROCID"];
            parameters[1] = null;              // 설비ID
            parameters[3] = dr["WIPSEQ"].ToString();
            if (cboWorkHistory.SelectedValue.ToString() == "SPLIT_LOT")
            {
                parameters[2] = dr["FROM_LOTID"];
                parameters[4] = dr["QTY"].ToString();
            }
            else if (cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT" || cboWorkHistory.SelectedValue.ToString() == "MERGE_LOT_MIX")
            {
                parameters[2] = dr["TO_LOTID"];
                parameters[4] = dr["WIPQTY2"].ToString();

            }
            else
            {
                parameters[2] = dr["LOTID"];
                parameters[4] = dr["WIPQTY2"].ToString();
            }
            parameters[5] = "N";                                         // 디스패치 처리
            parameters[6] = "Y";                                         // 출력 여부
            parameters[7] = "N";     // Direct 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            //popupTagPrint.Closed += new EventHandler(popupTagPrint_Closed);
            grdMain.Children.Add(popupTagPrint);
            popupTagPrint.BringToFront();
        }

        private void btnReSetMergerMix_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgListInputMix);
            Util.gridClear(dgSelectInputMix);
            txtTotalLotIDMergerMix.Text = string.Empty;
            txtWipQtyMergerMix.Text = string.Empty;
            txtSumWipQtyMergerMix.Text = string.Empty;
            txtWipQtyMergerMix_Box.Text = string.Empty;
            txtSumWipQtyMergerMix_Box.Text = string.Empty;
        }

        private void txtPalletIdMergerMix_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtPalletIdMergerMix.Text.Trim();
                    if (dgListInputMix.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListInputMix.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListInputMix.Rows[i].DataItem, "PALLETID").ToString() == sLotid)
                            {
                                dgListInputMix.ScrollIntoView(i, dgListInputMix.Columns["CHK"].Index);
                                dgListInputMix.SelectedIndex = i;
                                txtPalletIdMergerMix.Focus();
                                txtPalletIdMergerMix.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_MergerMix(false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetLotList_MergerMix(bool bButton, string sPalletID = "")
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PJT_NAME", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("LOTID_RT", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));
                //dtRqst.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                if (Util.GetCondition(txtPalletIdMergerMix).Equals("") && sPalletID.Equals("")) //PalletID 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentMergerMix, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return;

                    dr["PROCID"] = Util.GetCondition(cboProcessMergerMix, bAllNull: true);
                    dr["PJT_NAME"] = txtPrjtNameMergerMix.Text;
                    dr["PRODID"] = txtProdidMergerMix.Text;
                    dr["LOTID_RT"] = txtLotRTDMergerMix.Text;
                    dr["LANGID"] = LoginInfo.LANGID;
                    //dr["WIP_QLTY_TYPE_CODE"] = Util.GetCondition(cboQltyMergerMix, bAllNull: true);
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sPalletID.Equals("") ? Util.GetCondition(txtPalletIdMergerMix) : sPalletID;
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLTE_MERGER_SPLIT_QTY_LIST_MB", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListInputMix, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInputMix);
                    txtTotalLotIDMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix.Text = string.Empty;
                    txtSumWipQtyMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix_Box.Text = string.Empty;
                    txtSumWipQtyMergerMix_Box.Text = string.Empty;
                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1905"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtPalletIdMergerMix.Focus();
                            txtPalletIdMergerMix.Text = string.Empty;
                        }
                    });
                    return;
                }
                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgListInputMix.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListInputMix);
                        Util.GridSetData(dgListInputMix, dtSource, FrameOperation, true);
                        DataTableConverter.SetValue(dgListInputMix.Rows[dgListInputMix.Rows.Count - 1].DataItem, "CHK", 1);
                        Seting_dgSelectInputMix(dgListInputMix.Rows.Count - 1);
                        txtPalletIdMergerMix.Text = string.Empty;
                        txtPalletIdMergerMix.Focus();
                    }
                    else
                    {

                        Util.gridClear(dgSelectInputMix);
                        Util.GridSetData(dgListInputMix, dtRslt, FrameOperation, true);
                    }

                    txtTotalLotIDMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix.Text = string.Empty;
                    txtSumWipQtyMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix_Box.Text = string.Empty;
                    txtSumWipQtyMergerMix_Box.Text = string.Empty;
                }
                else
                {
                    Util.GridSetData(dgListInputMix, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectInputMix);
                    txtTotalLotIDMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix.Text = string.Empty;
                    txtSumWipQtyMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix_Box.Text = string.Empty;
                    txtSumWipQtyMergerMix_Box.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearchMergerMix_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_MergerMix(true);
        }

        private void dgListInputMix_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (dgListInputMix.GetRowCount() == 0)
            {
                return;
            }

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgListInputMix.GetCellFromPoint(pnt);

            if (cell == null || cell.Value == null)
            {
                return;
            }
            if (cell.Column.Name != "CHK")
            {
                return;
            }

            int seleted_row = dgListInputMix.CurrentRow.Index;

            if (DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK").Equals(1))
            {
                Seting_dgSelectInputMix(seleted_row);
            }
            else
            {
                DataTable dtTo = DataTableConverter.Convert(dgSelectInputMix.ItemsSource);
                if (dtTo.Rows.Count > 0)
                {
                    if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0)
                    {
                        dtTo.Rows.Remove(dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "PALLETID") + "'")[0]);
                        dgSelectInputMix.ItemsSource = DataTableConverter.Convert(dtTo);
                        //머지할 Pallet 재조정
                        DgSelectInputCheckMix();
                    }
                }
            }
        }

        public void Seting_dgSelectInputMix(int seleted_row)
        {
            DataTable dtTo = DataTableConverter.Convert(dgSelectInputMix.ItemsSource);
            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Boolean));
                dtTo.Columns.Add("WIP_QLTY_TYPE_NAME", typeof(string));
                dtTo.Columns.Add("PALLETID", typeof(string));
                dtTo.Columns.Add("LOTID_RT", typeof(string));
                dtTo.Columns.Add("PJT", typeof(string));
                dtTo.Columns.Add("PRODID", typeof(string));
                dtTo.Columns.Add("INBOX_QTY", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(string));
                dtTo.Columns.Add("UNIT", typeof(string));
                dtTo.Columns.Add("LOTTYPE", typeof(string));
                dtTo.Columns.Add("SOC_VALUE", typeof(string));
                dtTo.Columns.Add("CAPA_GRD_CODE", typeof(string));
                dtTo.Columns.Add("VLTG_GRD_CODE", typeof(string));
                dtTo.Columns.Add("RSST_GRD_CODE", typeof(string));
                dtTo.Columns.Add("MKT_TYPE_NAME", typeof(string));
                dtTo.Columns.Add("WIPSEQ", typeof(string));
                dtTo.Columns.Add("WIPHOLD", typeof(string));
                dtTo.Columns.Add("PROC_LABEL_PRT_FLAG", typeof(string));
                dtTo.Columns.Add("EQPTID", typeof(string));
                dtTo.Columns.Add("WIP_QLTY_TYPE_CODE", typeof(string));
                dtTo.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string)); // C20180109_77053
            }

            if (dtTo.Rows.Count > 0)
            {
                if (dtTo.Select("LOTID_RT LIKE '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "LOTID_RT").ToString().Substring(0, 5) + "%'").Length == 0) //조립LOT 앞 5자리가 동일한지 확인
                {
                    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4146"); //동일한 조립LOT이 아닙니다.
                    return;
                }
                if (dtTo.Select("PRODID = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "PRODID") + "'").Length == 0) //동일한 제품 체크
                {
                    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4178"); //동일한 제품이 아닙니다.
                    return;
                }
                //if (dtTo.Select("LOTTYPE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "LOTTYPE") + "'").Length == 0) //동일한 LOTTYPE이 아닙니다. 
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4179"); //동일한 LOTTYPE이 아닙니다.
                //    return;
                //}

                if (dtTo.Select("WIP_QLTY_TYPE_NAME = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "WIP_QLTY_TYPE_NAME") + "'").Length == 0) //동일한 구분이 아닙니다. 
                {
                    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                    Util.MessageValidation("SFU4180 "); //동일한 구분이 아닙니다.
                    return;
                }

                //if (dtTo.Select("SOC_VALUE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "SOC_VALUE") + "'").Length == 0) //동일한 SOC 정보가 아닙니다.
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4181"); //동일한 SOC 정보가 아닙니다.
                //    return;
                //}

                //if (dtTo.Select("CAPA_GRD_CODE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "CAPA_GRD_CODE") + "'").Length == 0) //동일한 등급이 아닙니다. 
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4182"); //동일한 등급이 아닙니다.
                //    return;
                //}

                //if (dtTo.Select("VLTG_GRD_CODE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "VLTG_GRD_CODE") + "'").Length == 0) //동일한 전압등급이 아닙니다
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4183 "); //동일한 전압등급이 아닙니다
                //    return;
                //}

                //if (dtTo.Select("RSST_GRD_CODE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "RSST_GRD_CODE") + "'").Length == 0) //동일한 저항등급이 아닙니다
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4270"); //동일한 저항등급이 아닙니다
                //    return;
                //}

                //if (dtTo.Select("MKT_TYPE_NAME = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "MKT_TYPE_NAME") + "'").Length == 0) //동일한 내수/수출이 아닙니다.
                //{
                //    DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                //    Util.MessageValidation("SFU4271"); //동일한 내수/수출이 아닙니다.
                //    return;
                //}

                if (dtTo.Select("PALLETID = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "PALLETID") + "'").Length > 0) //중복체크
                {
                    return;
                }

                //C20180109_77053  고객반품 인경우 체크
                if ((DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE") != null &&
                     DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE").Equals("FORM_WORK_CR")) ||
                     dtTo.Select("FORM_WRK_TYPE_CODE = 'FORM_WORK_CR'").Length > 0)
                {
                    if (dtTo.Select("FORM_WRK_TYPE_CODE = '" + DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, "FORM_WRK_TYPE_CODE") + "'").Length == 0) //
                    {
                        DataTableConverter.SetValue(dgListInputMix.Rows[seleted_row].DataItem, "CHK", 0);
                        Util.MessageValidation("SFU4496"); //고객 반품 재작업 이외의 작업 유형과 병합 할 수 없습니다.
                        return;
                    }
                }
            }

            DataRow dr = dtTo.NewRow();
            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Boolean)))
                {
                    dr[dc.ColumnName] = 0;
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(dgListInputMix.Rows[seleted_row].DataItem, dc.ColumnName));
                }
            }
            dtTo.Rows.Add(dr);
            dgSelectInputMix.ItemsSource = DataTableConverter.Convert(dtTo);
            //머지할 Pallet 재조정
            DgSelectInputCheckMix();
        }

        public void DgSelectInputCheckMix()
        {
            double MergerQty = 0;
            string CheckYN = "N";
            for (int i = 0; i < dgSelectInputMix.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "CHK")) == "True")
                {
                    CheckYN = "Y";
                    dgSelectInputMix.SelectedIndex = i;
                    break;
                }
            }
            if (CheckYN == "Y")
            {
                for (int i = 0; i < dgSelectInputMix.Rows.Count; i++)
                {
                    MergerQty = MergerQty + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "WIPQTY")).ToString());
                }
                txtSumWipQtyMergerMix.Text = String.Format("{0:#,##0}", MergerQty);
            }
            else
            {
                txtTotalLotIDMergerMix.Text = string.Empty;
                txtWipQtyMergerMix.Text = string.Empty;
                txtSumWipQtyMergerMix.Text = string.Empty;
            }
        }

        private void dgLotChkMix_Checked(object sender, RoutedEventArgs e)
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
                        dgSelectInputMix.SelectedIndex = idx;
                        txtTotalLotIDMergerMix.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[idx].DataItem, "PALLETID"));
                        txtWipQtyMergerMix.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[idx].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : string.Format("{0:#,##0}", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[idx].DataItem, "WIPQTY"))));
                        txtWipQtyMergerMix_Box.Text = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[idx].DataItem, "INBOX_QTY")) == string.Empty ? Util.NVC(0) : string.Format("{0:#,##0}", Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[idx].DataItem, "INBOX_QTY"))));

                        double MergerQty = 0;
                        double MergerQty_Box = 0;
                        for (int i = 0; i < dgSelectInputMix.Rows.Count; i++)
                        {
                            MergerQty = MergerQty + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "WIPQTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "WIPQTY")).ToString());
                            MergerQty_Box = MergerQty_Box + Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "INBOX_QTY")) == string.Empty ? Util.NVC(0) : Util.NVC_Int(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "INBOX_QTY")).ToString());
                        }
                        txtSumWipQtyMergerMix.Text = String.Format("{0:#,##0}", MergerQty);
                        txtSumWipQtyMergerMix_Box.Text = String.Format("{0:#,##0}", MergerQty_Box);
                    }
                }
                else
                {
                    txtTotalLotIDMergerMix.Text = string.Empty;
                    txtWipQtyMergerMix.Text = string.Empty;
                    txtSumWipQtyMergerMix.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnPalletMergerMix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationMix())
                {
                    return;
                }
                //병합 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4174"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                PalletMergerMix();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void PalletMergerMix()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("PROD_PALLETID", typeof(string));
            inDataTable.Columns.Add("INBOX_QTY", typeof(decimal));
            inDataTable.Columns.Add("PALLET_FLAG", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["IFMODE"] = "OFF";
            row["PROD_PALLETID"] = txtTotalLotIDMergerMix.Text;
            if (txtSumWipQtyMergerMix_Box.Text != string.Empty)
            {
                row["INBOX_QTY"] = Convert.ToDecimal(txtSumWipQtyMergerMix_Box.Text);
            }
            //양, 불 구분
            if (Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[0].DataItem, "WIP_QLTY_TYPE_CODE")) == "G")
            {
                row["PALLET_FLAG"] = "G";
            }
            else
            {
                row["PALLET_FLAG"] = "N";
            }

            row["USERID"] = LoginInfo.USERID;

            inDataTable.Rows.Add(row);

            //병합PalletLot
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("PALLETID", typeof(string));


            for (int i = 0; i < dgSelectInputMix.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "CHK")) == "False")
                {
                    row = inLot.NewRow();
                    row["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "PALLETID"));
                    inLot.Rows.Add(row);
                }
            }
            try
            {
                //Pallet병합처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MERGE_PALLET_MIX", "INDATA,INLOT", null, (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1275"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        GetLotList_MergerMix_Tag();
                    });
                    return;

                }, inData);



            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_MERGE_PALLET_MIX", ex.Message, ex.ToString());

            }
        }

        public void GetLotList_MergerMix_Tag()
        {
            try
            {
                CMM_FORM_TAG_PRINT popupTagPrint = new CMM_FORM_TAG_PRINT();
                popupTagPrint.FrameOperation = this.FrameOperation;
                popupTagPrint.QMSRequestPalletYN = "Y";
                // SET PARAMETER

                object[] parameters = new object[8];
                for (int i = 0; i < dgSelectInputMix.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "CHK")) == "True")
                    {
                        parameters[0] = cboProcessMergerMix.SelectedValue.ToString();
                        parameters[1] = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "EQPTID"));
                        parameters[2] = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "PALLETID"));
                        parameters[3] = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "WIPSEQ"));
                        parameters[4] = txtSumWipQtyMergerMix.Text.ToString();
                        parameters[5] = "N";     // 디스패치 처리
                        parameters[6] = Util.NVC(DataTableConverter.GetValue(dgSelectInputMix.Rows[i].DataItem, "PROC_LABEL_PRT_FLAG"));
                        parameters[7] = "Y";      // Direct 출력 여부
                    }
                }

                C1WindowExtension.SetParameters(popupTagPrint, parameters);

                grdMain.Children.Add(popupTagPrint);
                popupTagPrint.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Util.gridClear(dgListInputMix);
                Util.gridClear(dgSelectInputMix);
                txtTotalLotIDMergerMix.Text = string.Empty;
                txtWipQtyMergerMix.Text = string.Empty;
                txtSumWipQtyMergerMix.Text = string.Empty;
                GetLotList_MergerMix(true);
            }
        }

        private bool ValidationMix()
        {
            if (txtTotalLotIDMergerMix.Text.Trim().Length == 0)
            {
                Util.MessageValidation("SFU4232"); //대표Lot을 선택하세요.
                return false;
            }

            if (txtSumWipQtyMergerMix.Text.Trim() == "0")
            {
                Util.MessageValidation("SFU4233"); //병합 Cell 수량이 0 입니다.
                return false;
            }
            if (dgSelectInputMix.Rows.Count == 1)
            {
                Util.MessageValidation("SFU4234"); //병합할 대상은 2개 이상이어야 합니다.
                return false;
            }

            if (txtSumWipQtyMergerMix_Box.Text.Trim() == "0")
            {
                Util.MessageValidation("SFU4235"); //병합 BOX 수량에 0을 입력할 수 없습니다.
                return false;
            }
            return true;
        }

        private void txtSumWipQtyMergerMix_Box_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtSumWipQtyMergerMix.Text, 0))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletIdMergerMix_PreviewKeyDown(object sender, KeyEventArgs e)
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

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;
                        }

                        if (dgListInputMix.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgListInputMix.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgListInputMix.Rows[idx].DataItem, "PALLETID").ToString() == sPasteStrings[i])
                                {
                                    dgListInputMix.ScrollIntoView(i, dgListInputMix.Columns["CHK"].Index);
                                    dgListInputMix.SelectedIndex = i;
                                    dgListInputMix.Focus();
                                    txtPalletIdMergerMix.Text = string.Empty;
                                    return;
                                }
                            }
                        }

                        GetLotList_MergerMix(false, sPasteStrings[i]);
                        System.Windows.Forms.Application.DoEvents();
                    }

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

        private DataTable GetInitOutboxList(string sPalletID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("OUTER_BOXID", typeof(string));
                dtRqst.Columns.Add("PKG_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID"] = sPalletID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUTER_BOX_FOR_SPLIT_MB", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    return dtRslt;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

    }
}
