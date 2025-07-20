/*************************************************************************************
 Created Date : 2019.02.15
      Creator : �ű���C
   Decription : ���δ� ������ô (In-Line)
                CNJ ���� 9ȣ�� Tray ���� �����̾ ������ �������� ���� �̿� ���� ������ �޸� Tray ���� �ϰ� �� ������ �̷�� ���� �Ǿ� �ű� ������ô ȭ������ �����Ͽ� ����
--------------------------------------------------------------------------------------
 [Change History]
   2019.02.15   �ű���C   : Initial Created.    
   2019.11.21   �̻���C   : ����Ȯ�� Validation���� ��ǰ����0 �� ��쿡�� ó�� �ǵ��� ������. 
                            ����Ȯ�� �� ��ǰ�� 0�ΰ�� Winding �̷�ī�� ��µ��� �ʵ��� ��.
   2023.02.07   ���ν�    : C20230109-000394 ���� ���� ���� ���� �̷� ��ȸ ��ư �̺�Ʈ �߰�
   2023.03.18   ���ν�    : C20221206-000611 �ڵ� ���� Ȯ�� �� PRODLOT �̼��� ���� ����
   2023.03.18   ���ν�    : C20221206-000611 UI���� �ڵ� ���� Ȯ�� �� �߰� �ҷ� ���� ���� ��ó�� ���� -> Biz���� ����
   2023.06.25   ������    : ���� Loss Level 2 Code ��� üũ �� ��ȯ
   2024.01.24   ������    : E20230901-001504 UcAssyResultDetail�� ChangeEquipment()�Լ� ȣ�� �κ� �߰�
   2024.01.30   ������    : ����Ȯ�� �� �������� ���� ��ǰ ��û��, ���� �� ���� DB ���� 
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using System.Linq;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY002
{
    public partial class ASSY002_010 : IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        public UC_WORKORDER_LINE Ucworkorder = new UC_WORKORDER_LINE();
        public UcAssyCommand UcAssyCommand { get; set; }
        public UcAssySearch UcAssySearch { get; set; }
        public UcAssyProdLot UcAssyProdLot { get; set; }
        public UcAssyResultDetail UcAssyResultDetail { get; set; }
        public UcAssyDataCollectInline UcAssyDataCollectInline { get; set; }
        public UcAssyShift UcAssyShift { get; set; }

        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }
        public C1DataGrid DgDefect { get; set; }
        public C1DataGrid DgDefectDetail { get; set; }

        private string _processCode;
        private string _InlineFlag = "Y";
        private bool _isSmallType;
        private bool _isLoaded;
        private bool _isTestMode;
        private bool _isAutoConfirmMode;        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        private bool _isSucAutoConfirm;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);

        //������ �ʰ� ����
        DataTable _dtRet_Data = new DataTable();
        string _sUserID = string.Empty;
        string _sDepID = string.Empty;
        bool _bInputErpRate = false;
        string _WipSeq = string.Empty;
        string _LOTID = string.Empty;

        public string ProcessCode
        {
            get { return _processCode; }
            set
            {
                _processCode = value;
            }
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY002_010()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void Initialize()
        {
            InitializeUserControls();
            InitializeUserControlsGrid();
            SetComboBox();
            SetEventInUserControls();

            string equipmentCode = string.Empty;
            string equipmentSegmentCode = string.Empty;

            _isSucAutoConfirm = false;

            if (ComboEquipment?.SelectedValue != null)
            {
                equipmentCode = ComboEquipment.SelectedValue.GetString();
            }

            if (ComboEquipmentSegment?.SelectedValue != null)
            {
                equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
            }
            UcAssyResultDetail?.ChangeEquipment(equipmentCode, equipmentSegmentCode);// E20230901-001504 2024.01.24 �߰�
            UcAssyDataCollectInline?.ChangeEquipment(equipmentCode, equipmentSegmentCode);

            if (LoginInfo.CFG_PROC_ID.Equals("A2000"))
            {
                Ucworkorder.CheckInline(ComboEquipmentSegment.SelectedValue.GetString());
            }
            //=====================================================================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            //=====================================================================================================================
            // �ڵ�����Ȯ�� ��ư ���� ó�� (OC�������� - OC��������2�� - ����������#09,#10 ȣ)
            //if (ProcessCode.Equals(Process.WINDING) && LoginInfo.CFG_SHOP_ID.Equals("A010") && LoginInfo.CFG_AREA_ID.Equals("M2") && (LoginInfo.CFG_EQSG_ID.Equals("M2C09") || LoginInfo.CFG_EQSG_ID.Equals("M2C10")))
            if (LoginInfo.CFG_SHOP_ID.Equals("A010") && LoginInfo.CFG_AREA_ID.Equals("M2") && (LoginInfo.CFG_EQSG_ID.Equals("M2C09") || LoginInfo.CFG_EQSG_ID.Equals("M2C10")) && LoginInfo.CFG_PROC_ID.Equals("A2000") )
            {
                UcAssyCommand.ButtonAutoRsltCnfmMode.Visibility = Visibility.Visible;
            }
            else
            {
                UcAssyCommand.ButtonAutoRsltCnfmMode.Visibility = Visibility.Collapsed; 
            }
        }

        private void InitializeUserControls()
        {
            UcAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            UcAssySearch = grdSearch.Children[0] as UcAssySearch;
            UcAssyProdLot = grdProductLot.Children[0] as UcAssyProdLot;
            UcAssyResultDetail = grdResult.Children[0] as UcAssyResultDetail;
            UcAssyDataCollectInline = grdDataCollect.Children[0] as UcAssyDataCollectInline;
            UcAssyShift = grdShift.Children[0] as UcAssyShift;
            Ucworkorder.InlineFlag = _InlineFlag;

            if (UcAssyCommand != null)
            {
                UcAssyCommand.ProcessCode = _processCode;
                UcAssyCommand.InlineFlag = _InlineFlag;
                UcAssyCommand.IsSmallType = _isSmallType;
                UcAssyCommand.SetButtonVisibility();
                UcAssyCommand.SetButtonInline();
            }

            if (UcAssyProdLot != null)
            {
                UcAssyProdLot.UcParentControl = this;
                UcAssyProdLot.ProcessCode = _processCode;
                UcAssyProdLot.IsSmalltype = _isSmallType;
                UcAssyProdLot.SetDataGridColumnVisibility();
            }

            if (UcAssyResultDetail != null)
            {
                UcAssyResultDetail.UcParentControl = this;
                UcAssyResultDetail.FrameOperation = FrameOperation;
                UcAssyResultDetail.ProcessCode = _processCode;
                UcAssyResultDetail.IsSmallType = _isSmallType;
                UcAssyResultDetail.SetControlProperties();
            }

            if (UcAssyDataCollectInline != null)
            {
                UcAssyDataCollectInline.UcParentControl = this;
                UcAssyDataCollectInline.ProcessCode = _processCode;
                UcAssyDataCollectInline.IsSmallType = _isSmallType;
                UcAssyDataCollectInline.FrameOperation = FrameOperation;
                UcAssyDataCollectInline.SetControlProperties();
                UcAssyDataCollectInline.SetInputHistButtonControls();
            }

            if (UcAssySearch != null)
            {
                UcAssySearch.ProcessCode = _processCode;
                ComboEquipmentSegment = UcAssySearch.ComboEquipmentSegment;
                ComboEquipment = UcAssySearch.ComboEquipment;
            }

            UcAssyCommand.SetInlineBtn();
        }

        private void InitializeUserControlsGrid()
        {
            DgProductLot = UcAssyProdLot.DgProductLot;
            DgDefect = UcAssyDataCollectInline.DgDefect;
            DgDefectDetail = UcAssyResultDetail.DgDefectDetail;
        }

        private void SetEventInUserControls()
        {
            if (UcAssyCommand != null)
            {
                UcAssyCommand.ButtonExtra.MouseLeave += ButtonExtra_MouseLeave;
                UcAssyCommand.ButtonEqptCond.Click += ButtonEqptCond_Click;
                UcAssyCommand.ButtonWaitPancake.Click += ButtonWaitPancake_Click;
                UcAssyCommand.ButtonCancelTerm.Click += ButtonCancelTerm_Click;
                UcAssyCommand.ButtonCancelTermSepa.Click += ButtonCancelTermSepa_Click;
                UcAssyCommand.ButtonWindingTrayLocation.Click += ButtonWindingTrayLocation_Click;
                UcAssyCommand.ButtonTestMode.Click += ButtonTestMode_Click;
                UcAssyCommand.ButtonScheduledShutdown.Click += ButtonScheduledShutdown_Click;
                UcAssyCommand.ButtonQualityInput.Click += ButtonQualityInput_Click;
                UcAssyCommand.ButtonSelfInspection.Click += ButtonSelfInspection_Click;
                UcAssyCommand.ButtonSelfInspectionNew.Click += ButtonSelfInspectionNew_Click;
                UcAssyCommand.ButtonEqptIssue.Click += ButtonEqptIssue_Click;
                UcAssyCommand.ButtonStart.Click += ButtonStart_Click;                   //�۾�����
                UcAssyCommand.ButtonCancel.Click += ButtonCancel_Click;                 //�������
                UcAssyCommand.ButtonEqptEnd.Click += ButtonEqptEnd_Click;               //���Ϸ�
                UcAssyCommand.ButtonEqptEndCancel.Click += ButtonEqptEndCancel_Click;   //���Ϸ����
                UcAssyCommand.ButtonConfirm.Click += ButtonConfirm_Click;               //����Ȯ��
                UcAssyCommand.ButtonHistoryCard.Click += ButtonHistoryCard_Click;
                UcAssyCommand.ButtonRemarkHist.Click += ButtonRemarkHist_Click;
                UcAssyCommand.ButtonEditEqptQty.Click += ButtonEditEqptQty_Click;
                UcAssyCommand.ButtonQualitySearch.Click += ButtonQualitySearch_Click;
                UcAssyCommand.ButtonEqptQtyHist.Click += ButtonEqptQtyHist_Click;
                //=====================================================================================================================
                // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                //=====================================================================================================================
                UcAssyCommand.ButtonAutoRsltCnfmMode.Click += ButtonAutoConfirm_Click;        //�ڵ�����Ȯ�� ��� ON/OFF
                UcAssySearch.ComboEquipmentSegment.MouseLeave += ComboEquipmentSegment_MouseLeave;//�ڵ�����Ȯ�� DISPLAY ON/OFF
            }

            if (UcAssySearch != null)
            {
                UcAssySearch.ComboEquipment.SelectedValueChanged += ComboEquipment_SelectedValueChanged;
                UcAssySearch.ButtonSearch.Click += ButtonSearch_Click;
            }

            if (UcAssyShift != null)
            {
                UcAssyShift.ButtonShift.Click += ButtonShift_Click;
            }

            if (UcAssyResultDetail != null)
            {
                UcAssyResultDetail.ButtonSaveWipHistory.Click += ButtonSaveWipHistory_Click;
            }
        }
        //=====================================================================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� - ��ɹ�ư DISPLAY
        //=====================================================================================================================
        private void ComboEquipmentSegment_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ComboEquipmentSegment.SelectedValue.ToString().Equals("M2C09") || ComboEquipmentSegment.SelectedValue.ToString().Equals("M2C10"))
            {
                UcAssyCommand.ButtonAutoRsltCnfmMode.Visibility = Visibility.Visible;
            }
            else
            {
                UcAssyCommand.ButtonAutoRsltCnfmMode.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            RegisterName("redBrush", redBrush);
            RegisterName("yellowBrush", yellowBrush);
            HideTestMode();
            //=====================================================================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            //=====================================================================================================================
            HideAutoConfirmMode();
        }

        /// <summary>
        /// Description     :   ASSY002_010.xaml Loaded �̺�Ʈ(Process Code �� �ʼ��� ���� ����, ���Ѻ� ��ư ����, UserControl ���� �� �̺�Ʈ ����)
        /// Author          :   �� ����
        /// Create Date     :   2017-06-07
        /// Update date     :   
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _processCode = Process.WINDING;
            _isSmallType = true;

            ApplyPermissions();
            SetWorkOrderWindow();
            Initialize();

            if (_isLoaded == false)
            {
                if(!ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
                    ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
            }
            _isLoaded = true;
            Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                UcAssyProduction.DispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            */
        }

        private void ButtonEqptQtyHist_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptQtyHist()) return;

            try
            {

                int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] parameters = new object[2];

                    parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
                    parameters[1] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));


                    C1WindowExtension.SetParameters(wndHist, parameters);
                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool ValidationEqptQtyHist()
        {
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex == -1)
            {
                Util.MessageValidation("PSS9073"); //���õ� LOT�� �����ϴ�.
                return false;
            }

            if (String.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"))))
            {
                Util.MessageValidation("PSS9073"); //���õ� LOT�� �����ϴ�.
                return false;
            }
            else
                return true;
        }

        private void ButtonExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            C1DropDownButton btn = sender as C1DropDownButton;
            if (btn != null) btn.IsDropDownOpen = false;
        }

        private void ButtonEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentCondition())
                return;

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_ASSY_PU_EQPT_COND popupEqptCond = new CMM_ASSY_PU_EQPT_COND { FrameOperation = FrameOperation};
            object[] parameters = new object[6];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = ComboEquipment.SelectedValue;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            parameters[5] = ComboEquipment.Text;

            C1WindowExtension.SetParameters(popupEqptCond, parameters);

            popupEqptCond.Closed += popupEqptCond_Closed;

            // �˾� ȭ�� �������� ���� ����.
            //Dispatcher.BeginInvoke(new Action(() => wndEqptCond.ShowModal()));
            grdMain.Children.Add(popupEqptCond);
            popupEqptCond.BringToFront();
        }

        private void popupEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_PU_EQPT_COND popup = sender as CMM_ASSY_PU_EQPT_COND;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonWaitPancake_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.Equals("") || ComboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            string workOrder;

            if (GetSelectWorkOrderRow() == null || _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y") == null)
            {
                workOrder = string.Empty;
            }
            else
            {
                workOrder = _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y").Field<string>("WOID").GetString();
            }

            // A����, C���
            string electrodeCode = string.Empty;
            string limitCount = string.Empty;
            string flag = string.Empty;
            string inputLotId = string.Empty;

            if (btn.Name == "btnWaitPancake")
            {
                flag = "A";
            }
            else if (btn.Name == "btnProductWaitLot")
            {
                flag = "C";
            }

            CMM_WAITING_PANCAKE_SEARCH popupWaitingPancake = new CMM_WAITING_PANCAKE_SEARCH { FrameOperation = FrameOperation };
            object[] parameters = new object[11];
            parameters[0] = workOrder;
            parameters[1] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[2] = _processCode;
            parameters[3] = electrodeCode;
            parameters[4] = limitCount;
            parameters[5] = inputLotId;
            parameters[6] = flag;
            parameters[7] = ComboEquipment.SelectedValue.GetString();
            parameters[8] = string.Empty;
            parameters[9] = _isSmallType;
            parameters[10] = "N";
            C1WindowExtension.SetParameters(popupWaitingPancake, parameters);
            popupWaitingPancake.Closed += popupWaitingPancake_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupWaitingPancake.ShowModal()));
        }

        private void popupWaitingPancake_Closed(object sender, EventArgs e)
        {
            CMM_WAITING_PANCAKE_SEARCH popup = sender as CMM_WAITING_PANCAKE_SEARCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonCancelTerm_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CANCEL_TERM popupCanCelTerm = new CMM_ASSY_CANCEL_TERM {FrameOperation = FrameOperation};

            object[] parameters = new object[1];
            parameters[0] = _processCode;
            C1WindowExtension.SetParameters(popupCanCelTerm, parameters);

            popupCanCelTerm.Closed += popupCanCelTerm_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCanCelTerm.ShowModal()));
        }

        private void popupCanCelTerm_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM window = sender as CMM_ASSY_CANCEL_TERM;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
        }



        private void ButtonCancelTermSepa_Click(object sender, RoutedEventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_SEPA popupCancelTermSepa = new CMM_ASSY_CANCEL_TERM_SEPA { FrameOperation = FrameOperation };

            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.Equals("") || ComboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1153");    //���� �������ּ���
                return;
            }

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = ComboEquipment.SelectedValue.GetString();
            parameters[2] = "Winding";
            parameters[3] = ComboEquipment.Text.GetString();
            C1WindowExtension.SetParameters(popupCancelTermSepa, parameters);
            popupCancelTermSepa.Closed += new EventHandler(popupCancelTermSepa_Closed);
            this.Dispatcher.BeginInvoke(new Action(() => popupCancelTermSepa.ShowModal()));
            //grdMain.Children.Add(popupCancelTermSepa);
            //popupCancelTermSepa.BringToFront();
        }

        private void popupCancelTermSepa_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_TERM_SEPA window = sender as CMM_ASSY_CANCEL_TERM_SEPA;
            if (window != null && window.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(window);
        }




        private void ButtonWindingTrayLocation_Click(object sender, RoutedEventArgs e)
        {
            
            var btnTrayPosition = sender as Button;
            if (btnTrayPosition == null) return;
            if (!ValidationTrayLocation()) return;

            UcAssyDataCollectInline.DispatcherTimer?.Stop();

            CMM_WINDING_TRAY_LOCATION_ADJUSTMENT popupWindingTrayPosition = new CMM_WINDING_TRAY_LOCATION_ADJUSTMENT
            {
                FrameOperation = FrameOperation
            };

            string lotId = string.Empty;
            string outLotId = string.Empty;
            string trayId = string.Empty;
            string trayTag = "C";
            DataRow drWorkOrder = null;
            string prodId = string.Empty;
            string flag = string.Empty;

            if (btnTrayPosition.Name == "btnCellWindingLocation")
            {
                int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                if (idx >= 0)
                {
                    lotId = DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString();
                }

                C1DataGrid dg = UcAssyDataCollectInline.DgProdCellWinding;

                int rowIdx = _util.GetDataGridCheckFirstRowIndex(dg, "CHK");
                if (rowIdx < 0) return;
                outLotId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "OUT_LOTID")).GetString();
                trayId = Util.NVC(DataTableConverter.GetValue(dg.Rows[rowIdx].DataItem, "TRAYID")).Replace("\0", "");
                trayTag = "U";
                drWorkOrder = Ucworkorder.GetSelectWorkOrderRow();
                prodId = Util.NVC(drWorkOrder["PRODID"]);
                flag = string.Empty;
            }

            object[] parameters = new object[12];
            parameters[0] = _processCode;
            parameters[1] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[2] = ComboEquipment.SelectedValue.GetString();
            parameters[3] = ComboEquipment.Text;
            parameters[4] = lotId;
            parameters[5] = outLotId;
            parameters[6] = trayId;
            parameters[7] = trayTag;
            parameters[8] = drWorkOrder;
            parameters[9] = prodId;
            parameters[10] = flag;
            parameters[11] = _isSmallType;

            C1WindowExtension.SetParameters(popupWindingTrayPosition, parameters);
            popupWindingTrayPosition.Closed += popupWindingTrayPosition_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupWindingTrayPosition.ShowModal()));
        }

        private void popupWindingTrayPosition_Closed(object sender, EventArgs e)
        {
            CMM_WINDING_TRAY_LOCATION_ADJUSTMENT popup = sender as CMM_WINDING_TRAY_LOCATION_ADJUSTMENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);

            if (UcAssyDataCollectInline.DispatcherTimer != null && UcAssyDataCollectInline.DispatcherTimer.Interval.TotalSeconds > 0)
                UcAssyDataCollectInline.DispatcherTimer.Start();
        }

        private void ButtonTestMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationTestMode()) return;

            if (_isTestMode)
            {
                SetTestMode(false);
                GetTestMode();
            }
            else
            {
                Util.MessageConfirm("SFU3411", (result) => // �׽�Ʈ Run�� �Ǹ� ����ó���� ���� �ʽ��ϴ�. �׽�Ʈ Run �Ͻðڽ��ϱ�?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("�׽�Ʈ�������");

                        SetTestMode(true);
                        GetTestMode();
                    }
                });
            }
        }

        private void ButtonScheduledShutdown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationScheduledShutdownMode()) return;

                if (_isTestMode)
                {
                    SetTestMode(false, bShutdownMode: true);
                    GetTestMode();
                }
                else
                {
                    Util.MessageConfirm("SFU4460", (result) => // ��ȹ������ �Ͻðڽ��ϱ�?
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("��ȹ����");

                            SetTestMode(true, bShutdownMode: true);
                            GetTestMode();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ButtonQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInfo())
                return;

            CMM_ASSY_QUALITY_INPUT_LOT_TIME popupQualityInput = new CMM_ASSY_QUALITY_INPUT_LOT_TIME
            {
                FrameOperation = FrameOperation
            };

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = ComboEquipment.SelectedValue;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WO_DETL_ID"));

            C1WindowExtension.SetParameters(popupQualityInput, parameters);
            popupQualityInput.Closed += popupQualityInput_Closed;

            // �˾� ȭ�� �������� ���� ����.
            Dispatcher.BeginInvoke(new Action(() => popupQualityInput.ShowModal()));

        }

        private void popupQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_INPUT_LOT_TIME popup = sender as CMM_ASSY_QUALITY_INPUT_LOT_TIME;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);
            //===================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
            //      3. <ǰ���˻� �Է�>
            //===================================================================
            AutoConfirm_Call();
        }

        private void ButtonSelfInspection_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelfInspection()) return;

            CMM_COM_QUALITY popupSelfInspection = new CMM_COM_QUALITY
            {
                FrameOperation = FrameOperation
            };

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupSelfInspection, parameters);
            popupSelfInspection.Closed += popupSelfInspection_Closed;

            // �˾� ȭ�� �������� ���� ����.
            Dispatcher.BeginInvoke(new Action(() => popupSelfInspection.ShowModal()));
        }

        private void popupSelfInspection_Closed(object sender, EventArgs e)
        {
            CMM_COM_QUALITY popup = sender as CMM_COM_QUALITY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
            //===================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
            //      4.1 	<���ְ˻� �Է�>
            //===================================================================
            AutoConfirm_Call();
        }

        private void ButtonSelfInspectionNew_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSelfInspection()) return;

            CMM_COM_SELF_INSP popupSelfInspectionNew = new CMM_COM_SELF_INSP { FrameOperation = FrameOperation };
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupSelfInspectionNew, parameters);
            popupSelfInspectionNew.Closed += popupSelfInspectionNew_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupSelfInspectionNew.ShowModal()));
        }

        private void popupSelfInspectionNew_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP popup = sender as CMM_COM_SELF_INSP;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //===================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
            //      4.2 	<���ְ˻� �Է�>
            //===================================================================
            AutoConfirm_Call();
        }

        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (ComboEquipment.SelectedValue == null || ComboEquipment.SelectedValue.Equals("") || ComboEquipment.SelectedValue.Equals("SELECT"))
            {
                Util.MessageValidation("SFU1673");
                return;
            }

            int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_COM_EQPCOMMENT popupEqpComment = new CMM_COM_EQPCOMMENT {FrameOperation = FrameOperation};

            object[] parameters = new object[10];
            parameters[0] = ComboEquipmentSegment.SelectedValue.GetString();
            parameters[1] = ComboEquipment.SelectedValue.ToString();
            parameters[2] = _processCode;
            parameters[3] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
            parameters[4] = iRow < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
            parameters[5] = ComboEquipment.Text;
            parameters[6] = UcAssyShift.TextShift.Text;     // �۾�����
            parameters[7] = UcAssyShift.TextShift.Tag;      // �۾����ڵ�
            parameters[8] = UcAssyShift.TextWorker.Text;    // �۾��ڸ�
            parameters[9] = UcAssyShift.TextWorker.Tag;     // �۾��� ID

            C1WindowExtension.SetParameters(popupEqpComment, parameters);
            popupEqpComment.Closed += popupEqpComment_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupEqpComment.ShowModal()));
        }

        private void popupEqpComment_Closed(object sender, EventArgs e)
        {
            CMM_COM_EQPCOMMENT popup = sender as CMM_COM_EQPCOMMENT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart())
                return;

            ASSY002_010_RUNSTART popupRunStart = new ASSY002_010_RUNSTART {FrameOperation = FrameOperation};

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[3] = Util.NVC(ComboEquipment.Text);
            parameters[4] = string.Empty;
            // Set Work Order Parameter
            parameters[5] = _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
            C1WindowExtension.SetParameters(popupRunStart, parameters);

            // �������� 
            //popupRunStart.Closed += popupRunStart_Closed;
            popupRunStart.Closed += new EventHandler(popupRunStart_Closed);
            //Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            this.Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            ASSY002_010_RUNSTART popup = sender as ASSY002_010_RUNSTART;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                ClearControls();
                GetProductLot();
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });
        }

        private void ButtonEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEnd()) return;

            CMM_ASSY_EQUIPMENT_END popupEqpEnd = new CMM_ASSY_EQUIPMENT_END { FrameOperation = FrameOperation };
            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            object[] parameters = new object[7];
            parameters[0] = _processCode;
            parameters[1] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPDTTM_ST_ORG"));
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "DTTM_NOW"));
            parameters[5] = _isSmallType;
            parameters[6] = false;  // ���۾� ����
            C1WindowExtension.SetParameters(popupEqpEnd, parameters);

            //popupEqpEnd.Closed += popupEqpEnd_Closed;
            //��������
            popupEqpEnd.Closed += new EventHandler(popupEqpEnd_Closed);
            grdMain.Children.Add(popupEqpEnd);
            popupEqpEnd.BringToFront();

        }        

        private void popupEqpEnd_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQUIPMENT_END popup = sender as CMM_ASSY_EQUIPMENT_END;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();

                //===================================================================
                // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
                //      5. <���Ϸ�>
                //===================================================================
                System.Threading.Thread.Sleep(5000);
                AutoConfirm_Call();
            }
            grdMain.Children.Remove(popup);
            
        }

        private void ButtonEqptEndCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEquipmentEndCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EquipmentEndCancel();
                }
            });

        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            DgDefectDetail.EndEdit();
            DgDefectDetail.EndEditRow(true);

            if (!ValidationConfirm()) return;

            // �ҷ�, ��������, ���� ����ǰ ���忩�� üũ
            //if (!ValidationDataCollect())return;

            if (!ValidationInspectionLot()) return;

            if (!ValidationInspectionTime()) return;

            if (DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() == 0)
            {   //��ǰ���� 0�Դϴ�. �׷��� ����Ȯ�� �Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU4497", result1 =>
                {
                    if (result1 == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                });
            }
            else
            {   //���� Ȯ�� �Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU1706", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ConfirmProcess();
                    }
                });
            }
        }

        //====================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 4.[�ڵ�����Ȯ�����] ��ư ����
        //====================================================================
        private void ButtonAutoConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationAutoConfirmMode()) return; //�߰� �ʿ� --�׽�Ʈ ���ϰ�� �׽�Ʈ �� ����, ��ȹ���� ���ϰ�� ��ȹ ���� ���� �ʿ�


             if (_isAutoConfirmMode)
            {
                SetAutoConfirMode(false);
                GetAutoConfirmMode();
            }
            else
            {
                //1. �׽�Ʈ �� ü�� �ʿ� --����� Ȯ�� �޽��� 

                //2. ��ȹ ���� üũ �ʿ� --����� Ȯ�� �޽��� 

                Util.MessageConfirm("SFU5204", (result) => // �ڵ�����Ȯ����忡���� �׽�Ʈ Run, ��ȹ������ �۵� ���� �ʽ��ϴ�. �ڵ�����Ȯ�� Run ���� �Ͻðڽ��ϱ�?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        // �׽�Ʈ �� ����
                        // ��ȹ ���� ���� 
                        txtEqptMode.Text = ObjectDic.Instance.GetObjectName("�ڵ�����Ȯ���������");

                        SetAutoConfirMode(true);
                        GetAutoConfirmMode();
                    }
                });
            }
        }
        //===================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
        //      1. �����ӽ�����( Ư�̻����Է�, �߰��ҷ�����)
        //      2. �۾���/�۾��� �Է�, 4. ǰ���˻� �Է�, 5.���ְ˻��Է�
        //      6. ���Ϸ�
        //=====================================================================
        public void AutoConfirm_Call()
        {
            if (!_isAutoConfirmMode) return;

            DgDefectDetail.EndEdit();
            DgDefectDetail.EndEditRow(true);

            if (!ValidationAutoConfirm()) return;

            // �ҷ�, ��������, ���� ����ǰ ���忩�� üũ
            //if (!ValidationDataCollect())return;

            if (!ValidationInspectionLot()) return;

            if (!ValidationInspectionTime()) return;

            if (DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() == 0)
            {   //��ǰ���� 0�Դϴ�. �׷��� ����Ȯ�� �Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU4497", result1 =>
                {
                    if (result1 == MessageBoxResult.OK)
                    {
                        AutoConfirmProcess();
                    }
                });
            }
            else
            {  
                // ����� ����Ȯ�� ���� Ȯ�� �޽��� �˾� ���� => �����䱸���� 20221129
                ////���� Ȯ�� �Ͻðڽ��ϱ�?
                //Util.MessageConfirm("SFU1706", (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                        AutoConfirmProcess();
                    //}
                //});
            }
        }

        private void ButtonHistoryCard_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationHistoryCard()) return;

            CMM_ASSY_HISTORYCARD popupHistoryCard = new CMM_ASSY_HISTORYCARD { FrameOperation = FrameOperation};

            object[] parameters = new object[6];
            parameters[0] = DataTableConverter.Convert(ComboEquipment.ItemsSource);
            parameters[1] = Util.NVC(ComboEquipment.SelectedIndex);
            parameters[2] = DataTableConverter.Convert(ComboEquipmentSegment.ItemsSource);
            parameters[3] = Util.NVC(ComboEquipmentSegment.SelectedIndex);
            parameters[4] = _processCode;
            parameters[5] = _isSmallType;

            C1WindowExtension.SetParameters(popupHistoryCard, parameters);
            popupHistoryCard.Closed += popupHistoryCard_Closed;
            grdMain.Children.Add(popupHistoryCard);
            popupHistoryCard.BringToFront();
        }

        private void popupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_HISTORYCARD popup = sender as CMM_ASSY_HISTORYCARD;
            // �̷� �˾� ������ ó��
            grdMain.Children.Remove(popup);
        }

        private void ButtonRemarkHist_Click(object sender, RoutedEventArgs e)
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return;
            }

            CMM_ASSY_LOTCOMMENTHIST popupLotCommentHistory = new CMM_ASSY_LOTCOMMENTHIST { FrameOperation = FrameOperation};
            object[] parameters = new object[1];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
            C1WindowExtension.SetParameters(popupLotCommentHistory, parameters);

            popupLotCommentHistory.Closed += popupLotCommentHistory_Closed;

            grdMain.Children.Add(popupLotCommentHistory);
            popupLotCommentHistory.BringToFront();

        }

        private void popupLotCommentHistory_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_LOTCOMMENTHIST popup = sender as CMM_ASSY_LOTCOMMENTHIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            grdMain.Children.Remove(popup);
        }

        private void ButtonEditEqptQty_Click(object sender, RoutedEventArgs e)
        {
            if (!CommonVerify.HasDataGridRow(DgProductLot))
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            CMM_ASSY_EQPT_INPUT_QTY popupEqutInputQty = new CMM_ASSY_EQPT_INPUT_QTY { FrameOperation = FrameOperation };

            object[] parameters = new object[3];
            parameters[0] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "LOTID"));
            parameters[1] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY"));
            parameters[2] = ComboEquipmentSegment.SelectedValue;
            C1WindowExtension.SetParameters(popupEqutInputQty, parameters);
            popupEqutInputQty.Closed += popupEqutInputQty_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupEqutInputQty.ShowModal()));
            //grdMain.Children.Add(popupEqutInputQty);
            //popupEqutInputQty.BringToFront();

        }

        private void popupEqutInputQty_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_INPUT_QTY popup = sender as CMM_ASSY_EQPT_INPUT_QTY;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetProductLot();
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            GetTestMode();
            GetWorkOrder();
            GetProductLot();
            GetEqptWrkInfo();
            //=========================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� 
            // 3.[�ڵ�����Ȯ�����] ��ȸ
            GetAutoConfirmMode();
            //=========================================================
        }

        private void ButtonQualitySearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualitySearch()) return;

            CMM_ASSY_QUALITY_PKG popQuality = new CMM_ASSY_QUALITY_PKG { FrameOperation = FrameOperation };

            object[] parameters = new object[5];
            parameters[0] = ComboEquipmentSegment.SelectedValue;
            parameters[1] = _processCode;
            parameters[2] = ComboEquipment.SelectedValue;
            parameters[3] = ComboEquipmentSegment.Text;
            parameters[4] = ComboEquipment.Text;

            C1WindowExtension.SetParameters(popQuality, parameters);

            popQuality.Closed += popQuality_Closed;

            // �˾� ȭ�� �������� ���� ����.
            Dispatcher.BeginInvoke(new Action(() => popQuality.ShowModal()));
        }

        private void popQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG pop = sender as CMM_ASSY_QUALITY_PKG;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(pop);
        }

        private void ButtonSaveWipHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSaveWipHistory()) return;
            SaveWipHistory();
            //Util.MessageConfirm("SFU1241", (result) =>
            //{
            //    if (result == MessageBoxResult.OK)
            //    {
            //        SaveWipHistory();
            //    }
            //});
        }

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 {FrameOperation = FrameOperation};

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = Util.NVC(ComboEquipmentSegment.SelectedValue);
            parameters[3] = _processCode;
            parameters[4] = Util.NVC(UcAssyShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcAssyShift.TextWorker.Tag);
            parameters[6] = Util.NVC(ComboEquipment.SelectedValue);
            parameters[7] = "Y"; // ���� Flag "Y" �϶��� ����.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += popupShiftUser_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupShiftUser.ShowModal()));
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

                //===================================================================
                // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
                //      2. <�۾���/�۾��� �Է�>
                //===================================================================
                bool isDataShiftOrWorker = GetEqptWrkInfo();
                //if(isDataShiftOrWorker)
                //{
                AutoConfirm_Call();
                //}
            }          
        }

        private void ComboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                //GetTestMode();
                ClearControls();
                UcAssyShift.ClearShiftControl();

                if (Ucworkorder != null)
                {
                    Ucworkorder.EQPTSEGMENT = ComboEquipmentSegment.SelectedValue.GetString();
                    Ucworkorder.EQPTID = ComboEquipment.SelectedValue.GetString();
                    Ucworkorder.PROCID = _processCode;
                    if (_processCode.Equals("A2000"))
                    {
                        Ucworkorder.CheckInline(ComboEquipmentSegment.SelectedValue.GetString());
                    }
                }

                string equipmentCode = string.Empty;
                string equipmentSegmentCode = string.Empty;

                if (ComboEquipment?.SelectedValue != null)
                {
                    equipmentCode = ComboEquipment.SelectedValue.GetString();
                }
                    
                if (ComboEquipmentSegment?.SelectedValue != null)
                {
                    equipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                }

                UcAssyResultDetail?.ChangeEquipment(equipmentCode, equipmentSegmentCode);// E20230901-001504 2024.01.24 �߰�
                UcAssyDataCollectInline?.ChangeEquipment(equipmentCode,equipmentSegmentCode);

                // ���� ���� �� �ڵ� ��ȸ ó��
                if (ComboEquipment != null && (ComboEquipment.SelectedIndex > 0 && ComboEquipment.Items.Count > ComboEquipment.SelectedIndex))
                {
                    if(ComboEquipment.SelectedValue.GetString() != "SELECT")
                    {
                        Dispatcher.BeginInvoke(new Action(() => ButtonSearch_Click(UcAssySearch.ButtonSearch, null)));
                    }
                }

                //===================================================================
                // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                // --�����Ī�� ���� ��� �ڵ�����Ȯ����� �����
                //===================================================================
                else
                {
                    if(!_isAutoConfirmMode)
                    {
                        HideAutoConfirmMode();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        
        #endregion

        #region Mehod

        private void ConfirmProcess()
        {
            try
            {
                SaveDefectBeforeConfirm();

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                //Rate ���� �˾�
                //�Ϸ� ó�� �ϱ� ���� �˾� ǥ��
                _bInputErpRate = false;
                _dtRet_Data.Clear();
                _sUserID = string.Empty;
                _sDepID = string.Empty;
                if (PERMIT_RATE_input(_LOTID, _WipSeq))
                {
                    return;
                }
                ///////////////////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                // ����9,10ȣ ����                
                //string bizRuleName = _isSmallType ? "BR_PRD_REG_END_LOT_WNS" : "BR_PRD_REG_END_LOT_WN";
                string bizRuleName = "BR_PRD_REG_END_LOT_WNS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcAssyShift.TextWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                            //���� ó�� �Ǿ����ϴ�.
                            Util.MessageInfo("SFU1275");
                        System.Threading.Thread.Sleep(500);

                            //===================================================================
                            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                            // --> �̷�ī�� ������
                            //===================================================================
                            //�̷�ī�� ������ 20221108
                            //if (string.Equals(_processCode, Process.WINDING) && DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() > 0)
                            //{   // Winding �̷�ī�� ���
                            //    //PrintHistoryCard(UcAssyProduction.ProdLotId);
                            //    PrintHistoryCard(_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString());
                            //}
                        }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        UcAssyResultDetail.ClearResultDetailControl();
                        ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void Confirm_Real()
        {
            try
            {
                ShowLoadingIndicator();

                // ����9,10ȣ ����                
                //string bizRuleName = _isSmallType ? "BR_PRD_REG_END_LOT_WNS" : "BR_PRD_REG_END_LOT_WN";
                string bizRuleName = "BR_PRD_REG_END_LOT_WNS";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_END_LOT_WN();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["INPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY").GetDouble();
                newRow["OUTPUTQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDouble();
                newRow["RESNQTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY").GetDouble();
                newRow["SHIFT"] = UcAssyShift.TextShift.Tag.GetString();
                newRow["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
                newRow["WRK_USERID"] = UcAssyShift.TextWorker.Tag;
                newRow["WRK_USER_NAME"] = UcAssyShift.TextWorker.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = Convert.ToDateTime(GetConfirmDate());
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // ERP �ҷ� ���� Rate ����
                        if (_bInputErpRate && _dtRet_Data.Rows.Count > 0)
                        {
                            BR_PRD_REG_PERMIT_RATE_OVER_HIST();
                        }

                        //���� ó�� �Ǿ����ϴ�.
                        Util.MessageInfo("SFU1275");
                        System.Threading.Thread.Sleep(500);

                        //===================================================================
                        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                        // --> �̷�ī�� ������
                        //===================================================================
                        //�̷�ī�� ������ 20221108
                        //if (string.Equals(_processCode, Process.WINDING) && DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() > 0)
                        //{   // Winding �̷�ī�� ���
                        //    //PrintHistoryCard(UcAssyProduction.ProdLotId);
                        //    PrintHistoryCard(_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString());
                        //}
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        UcAssyResultDetail.ClearResultDetailControl();
                        ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private double GetLotINPUTQTY(string sLotID)
        {
            double dInputQty = 0;
            if (DgDefectDetail != null && DgDefectDetail.Rows.Count == 3)
            {
                if (DgDefectDetail.Rows.Count > 0)
                {
                    dInputQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[2].DataItem, "OUTPUTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[2].DataItem, "OUTPUTQTY")));
                }
            }
            return dInputQty;
        }

        private bool PERMIT_RATE_input(string sLotID, string sWipSeq)
        {
            bool bFlag = false;
            try
            {
                DgDefect.EndEdit();
                //��ǰ ������ ������ �´�.
                //double goodQty = GetLotGoodQty(sLotID);
                ////////��������� ������ �´�
                double goodQty = GetLotINPUTQTY(sLotID);

                DataTable data = new DataTable();
                data.Columns.Add("LOTID", typeof(string));
                data.Columns.Add("WIPSEQ", typeof(string));
                data.Columns.Add("ACTID", typeof(string));
                data.Columns.Add("ACTNAME", typeof(string));
                data.Columns.Add("RESNCODE", typeof(string));
                data.Columns.Add("RESNNAME", typeof(string));
                data.Columns.Add("DFCT_CODE_DETL_NAME", typeof(string));
                data.Columns.Add("RESNQTY", typeof(string));
                data.Columns.Add("PERMIT_RATE", typeof(string));
                data.Columns.Add("OVER_QTY", typeof(string));
                data.Columns.Add("SPCL_RSNCODE", typeof(string));
                data.Columns.Add("SPCL_RSNCODE_NAME", typeof(string));
                data.Columns.Add("RESNNOTE", typeof(string));


                decimal dRate = 0;
                decimal dQty = 0;
                decimal dAllowQty = 0;
                decimal OverQty = 0;

                for (int j = 0; j < DgDefect.Rows.Count - DgDefect.BottomRows.Count; j++)
                {
                    // double dRate = 0;
                    dRate = Util.NVC_Decimal(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "PERMIT_RATE"));
                    //��ϵ� Rate�� 0���� ū���� �͸� ����
                    if (dRate > 0)
                    {
                        dQty = Util.NVC_Decimal(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNQTY")); //�������� ��� ���?                        
                                                                                                                    //dAllowQty = Math.Truncate(goodQty * dRate / 100); //���� 
                        dAllowQty = Convert.ToDecimal(goodQty) * dRate / 100;
                        if (dAllowQty < dQty)
                        {
                            OverQty = dQty - dAllowQty;
                            OverQty = Math.Ceiling(OverQty); //�Ҽ��� ù�ڸ� �ø�

                            DataRow newRow = data.NewRow();

                            newRow["LOTID"] = sLotID; //�ʼ�
                            newRow["WIPSEQ"] = sWipSeq; //�ʼ�
                            newRow["ACTID"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "ACTID")); //�ʼ�
                            newRow["ACTNAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "ACTNAME")); //�ʼ�
                            newRow["RESNCODE"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNCODE")); //�ʼ�
                            newRow["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "RESNNAME")); //�ʼ�
                            newRow["DFCT_CODE_DETL_NAME"] = Util.NVC(DataTableConverter.GetValue(DgDefect.Rows[j].DataItem, "DFCT_CODE_DETL_NAME"));


                            newRow["RESNQTY"] = dQty.ToString("G29"); //�ʼ�
                            newRow["PERMIT_RATE"] = dRate.ToString("0.00");  //�ʼ� 0���� 
                            newRow["OVER_QTY"] = OverQty.ToString("G29"); //(dQty - dAllowQty).ToString("0.000"); //�Ҽ��� 3�ڸ�

                            newRow["SPCL_RSNCODE"] = "";
                            newRow["SPCL_RSNCODE_NAME"] = "";
                            newRow["RESNNOTE"] = "";
                            data.Rows.Add(newRow);
                        }
                    }
                }


                //��� �� ������ ������ 
                if (data.Rows.Count > 0)
                {

                    CMM_PERMIT_RATE popRermitRate = new CMM_PERMIT_RATE { FrameOperation = FrameOperation };
                    object[] parameters = new object[2];
                    parameters[0] = sLotID;
                    parameters[1] = data;
                    C1WindowExtension.SetParameters(popRermitRate, parameters);

                    popRermitRate.Closed += popupPermitRate_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popRermitRate.ShowModal()));

                    bFlag = true;
                }

                return bFlag;
                ///////////////////////////////////////////////                  
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bFlag;
            }

        }

        private void popupPermitRate_Closed(object sender, EventArgs e)
        {
            try
            {
                CMM_PERMIT_RATE popRermitRate = sender as CMM_PERMIT_RATE;
                if (popRermitRate != null && popRermitRate.DialogResult == MessageBoxResult.OK)
                {
                    _dtRet_Data.Clear();
                    _dtRet_Data = popRermitRate.PERMIT_RATE.Copy();
                    _sUserID = popRermitRate.UserID;
                    _sDepID = popRermitRate.DeptID;
                    _bInputErpRate = true;

                    //////////////////////////////
                    //Ȯ�� ó��
                    Confirm_Real();

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void BR_PRD_REG_PERMIT_RATE_OVER_HIST()
        {
            try
            {
                DataTable lotTab = _dtRet_Data.DefaultView.ToTable(true, new string[] { "LOTID", "WIPSEQ" });
                string sLot = "";


                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataTable inRESN = indataSet.Tables.Add("IN_RESN");
                inRESN.Columns.Add("PERMIT_RATE", typeof(decimal));
                inRESN.Columns.Add("ACTID", typeof(string));
                inRESN.Columns.Add("RESNCODE", typeof(string));
                inRESN.Columns.Add("RESNQTY", typeof(decimal));
                inRESN.Columns.Add("OVER_QTY", typeof(decimal));
                inRESN.Columns.Add("REQ_USERID", typeof(string));
                inRESN.Columns.Add("REQ_DEPTID", typeof(string));
                inRESN.Columns.Add("DIFF_RSN_CODE", typeof(string));
                inRESN.Columns.Add("NOTE", typeof(string));

                for (int j = 0; j < lotTab.Rows.Count; j++)
                {
                    inTable.Rows.Clear();
                    inRESN.Rows.Clear();

                    sLot = lotTab.Rows[j]["LOTID"].ToString();

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["LOTID"] = sLot;
                    newRow["WIPSEQ"] = lotTab.Rows[j]["WIPSEQ"].ToString();
                    inTable.Rows.Add(newRow);
                    newRow = null;


                    for (int i = 0; i < _dtRet_Data.Rows.Count; i++)
                    {
                        if (sLot.Equals(_dtRet_Data.Rows[i]["LOTID"].ToString()))
                        {
                            newRow = inRESN.NewRow();
                            newRow["PERMIT_RATE"] = Convert.ToDecimal(_dtRet_Data.Rows[i]["PERMIT_RATE"].ToString());
                            newRow["ACTID"] = _dtRet_Data.Rows[i]["ACTID"].ToString();
                            newRow["RESNCODE"] = _dtRet_Data.Rows[i]["RESNCODE"].ToString();
                            newRow["RESNQTY"] = _dtRet_Data.Rows[i]["RESNQTY"].ToString();
                            newRow["OVER_QTY"] = _dtRet_Data.Rows[i]["OVER_QTY"].ToString();
                            newRow["REQ_USERID"] = _sUserID;
                            newRow["REQ_DEPTID"] = _sDepID;
                            newRow["DIFF_RSN_CODE"] = _dtRet_Data.Rows[i]["SPCL_RSNCODE"].ToString();
                            newRow["NOTE"] = _dtRet_Data.Rows[i]["RESNNOTE"].ToString();
                            inRESN.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERMIT_RATE_OVER_HIST", "INDATA,IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            //Util.AlertInfo("���� ó�� �Ǿ����ϴ�.");
                            //Util.MessageInfo("SFU1275");
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {

                        }
                    }, indataSet);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }

        //=====================================================================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) : �۾���/�۾��� �Է�, Ư�̻����Է�, ǰ���˻� �Է�, ���ְ˻��Է�, ���Ϸ�
        // 7.1 [�ڵ�����Ȯ��]	üũ
        // 7.2 [�ڵ�����Ȯ��]	���� (�ڵ����� Ȯ�� ���μ���
        //=====================================================================================================================
        private void AutoConfirmProcess()
        {

            try
            {
                // �ּ�ó���� - �ڵ�Ȯ��ó���ô� UI ���� �̺�Ʈ �߻��� ȣ���-- ���� ���� �ʿ�
                //2023.03.22 ���� Ȯ�� ������ �ÿ��� �߰� �ҷ� ���� �������� ���� ���� ������ ������ �ּ�ó�� -> Biz���� ����
                //SaveDefectBeforeAutoConfirm();
                ShowLoadingIndicator();

                // ����9,10ȣ ����                
                //string bizRuleName = _isSmallType ? "BR_PRD_REG_END_LOT_WNS" : "BR_PRD_REG_END_LOT_WN";
                string bizRuleName = "BR_PRD_REG_WINDING_AUTO_RSLT_CNFM";
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_WINDING_AUTO_RSLT_CNFM();
                DataTable inTable = indataSet.Tables["INDATA"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["PROD_LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        //���� ó�� �Ǿ����ϴ�.
                        _isSucAutoConfirm = true;
                        Util.MessageInfo("SFU1275");
                        System.Threading.Thread.Sleep(500);

                        //�̷�ī�� ������ 20221108
                        //if (string.Equals(_processCode, Process.WINDING) && DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() > 0)
                        //{   // Winding �̷�ī�� ���
                        //    //PrintHistoryCard(UcAssyProduction.ProdLotId);
                        //    PrintHistoryCard(_util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString());
                        //}
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                        UcAssyResultDetail.ClearResultDetailControl();
                        ButtonSearch_Click(UcAssySearch.ButtonSearch, null);
                    }

                }, indataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveDefectBeforeConfirm()
        {
            string selectedLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            UcAssyDataCollectInline.SaveDefectBeforeConfirm();

            if (string.Equals(_processCode, Process.WINDING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                if (_isSmallType)
                {
                    UcAssyDataCollectInline.GetProductCellList(UcAssyDataCollectInline.DgProdCellWinding, false);
                }
                else
                {
                    CalculateDefectQty();
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                CalculateDefectQty();
            }
            else if (string.Equals(_processCode, Process.WASHING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                //GetOutTray();
            }
        }

        private void SaveDefectBeforeAutoConfirm()
        {
            string selectedLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            string selectedWipSeq = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<decimal>("WIPSEQ").GetString();

            //C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            //[�ҷ�/LOSS/��û] ������ ���� �� - �߰��ҷ� ���� ��� ���� (�����) 
            if (string.Equals(_processCode, Process.WINDING) && _isAutoConfirmMode)
            {
                int iAutoRsltCnfmAddDfctQty = 0;
                int.TryParse(UcAssyResultDetail.TextAssyResultQty.Text, out iAutoRsltCnfmAddDfctQty);

                // 2022.11.30-���� �߰��ҷ������� 0�ΰ�쿡�� �ڵ�����ó�����ɿ���, ����(0)�� ��쿡�� �߰��ҷ��������� �� �ڵ�����Ȯ��ó�� ���� 
                //if (iAutoRsltCnfmAddDfctQty > 0)
                //{
                    //--UcAssyDataCollectInline.SetAutoRsltCnfmAddDefectQtyBeforeConfirm(iAutoRsltCnfmAddDfctQty);
                    UcAssyDataCollectInline.SaveDefectBeforeAutoConfirm(iAutoRsltCnfmAddDfctQty);
                //}
                //else
                //{
                //    UcAssyDataCollectInline.SaveDefectBeforeConfirm();
                //}
            }

            

            if (string.Equals(_processCode, Process.WINDING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                if (_isSmallType)
                {
                    UcAssyDataCollectInline.GetProductCellList(UcAssyDataCollectInline.DgProdCellWinding, false);
                }
                else
                {
                    CalculateDefectQty();
                }
            }
            else if (string.Equals(_processCode, Process.ASSEMBLY))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                CalculateDefectQty();
            }
            else if (string.Equals(_processCode, Process.WASHING))
            {
                SetDefectDetail();
                GetDefectInfo(selectedLotId, selectedWipSeq);
                //GetOutTray();
            }
        }

        private void SaveWipHistory()
        {
            ShowLoadingIndicator();

            const string bizRuleName = "BR_ACT_REG_SAVE_LOT";

            DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PROD_VER_CODE", typeof(string));
            inDataTable.Columns.Add("SHIFT", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("WRK_USER_NAME", typeof(string));
            inDataTable.Columns.Add("WRK_USERID", typeof(string));
            inDataTable.Columns.Add("LANE_PTN_QTY", typeof(decimal));
            inDataTable.Columns.Add("LANE_QTY", typeof(decimal));
            inDataTable.Columns.Add("PROD_QTY", typeof(decimal));
            inDataTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            dr["PROD_VER_CODE"] = null;
            dr["SHIFT"] = UcAssyShift.TextShift.Tag;
            dr["WIPNOTE"] = new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text;
            dr["WRK_USERID"] = Util.NVC(UcAssyShift.TextWorker.Tag);
            dr["WRK_USER_NAME"] = Util.NVC(UcAssyShift.TextWorker.Text);
            //dr["LANE_PTN_QTY"] = 0;
            //dr["LANE_QTY"] = 0;
            dr["PROD_QTY"] = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal();
            dr["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(dr);

            //DataSet ds = new DataSet();
            //ds.Tables.Add(inDataTable);
            //string xml = ds.GetXml();

            new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizException) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    GetProductLot();
                    //Util.MessageInfo("SFU1275");    //���� ó�� �Ǿ����ϴ�.
                    //===================================================================
                    // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                    // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
                    //      1. <�����ӽ�����> (Ư�̻��� �Է� üũ, �߰��ҷ����� �ڵ����
                    //===================================================================
                    AutoConfirm_Call();

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
            });
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CANCEL_START_LOT_WN";
                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    GetProductLot();
                    Util.MessageInfo("SFU1275");	//���� ó�� �Ǿ����ϴ�.
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void EquipmentEndCancel()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_WN";

                int iRow = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = ComboEquipment.SelectedValue.GetString();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                newRow["INPUT_LOTID"] = null; // Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "PR_LOTID"));
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        GetProductLot();

                        Util.MessageInfo("SFU1275");	//���� ó�� �Ǿ����ϴ�.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SetWorkOrderWindow()
        {
            if (grdWorkOrder.Children.Count == 0)
            {
                Ucworkorder.FrameOperation = FrameOperation;
                Ucworkorder._UCParent = this;
                Ucworkorder.PROCID = _processCode;
                grdWorkOrder.Children.Add(Ucworkorder);
            }
        }

        public void GetWorkOrder()
        {
            if (Ucworkorder == null)
                return;

            Ucworkorder.EQPTSEGMENT = ComboEquipmentSegment.SelectedValue.GetString();
            Ucworkorder.EQPTID = ComboEquipment.SelectedValue.GetString();
            Ucworkorder.PROCID = _processCode;

            Ucworkorder.GetWorkOrder();
        }

        public void GetProductLot()
        {
            try
            {
                string selectedLot = string.Empty;

                if (CommonVerify.HasDataGridRow(DgProductLot))
                {
                    int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

                    if (rowIdx >= 0)
                    {
                        selectedLot = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    }
                }

                const string wipState = "PROC,EQPT_END";
                UcAssyDataCollectInline.ClearDataCollectControl(); 
                ShowLoadingIndicator();

                // DA_PRD_SEL_PRODUCTLOT_WNS �ʼ��� , DA_PRD_SEL_PRODUCTLOT_WN ����
                string bizRuleName = _isSmallType ? "DA_PRD_SEL_PRODUCTLOT_WNS" : "DA_PRD_SEL_PRODUCTLOT_WN";

                DataTable indataTable = _bizDataSet.GetDA_PRD_SEL_PRODUCTLOT_ASSY();
                DataRow newRow = indataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                newRow["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                newRow["WIPSTAT"] = wipState;
                newRow["PROCID"] = _processCode;
                newRow["WIPTYPECODE"] = "PROD";

                indataTable.Rows.Add(newRow);
                //DataSet ds = new DataSet();
                //ds.Tables.Add(indataTable);
                //string xml = ds.GetXml();

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                    }
                    else
                    {
                        Util.GridSetData(DgProductLot, result, FrameOperation, true);

                        if (string.IsNullOrEmpty(selectedLot) || _isSucAutoConfirm)
                        {
                            if (result.Rows.Count > 0)
                            {
                                //int iRowRun = _util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC");
                                int iRowRun = _util.GetDataGridFirstRowIndexByEquiptmentEnd(DgProductLot, "WIPSTAT", "EQPT_END");
                                if (iRowRun < 0)
                                {
                                    iRowRun = 0;
                                    if (DgProductLot.TopRows.Count > 0)
                                        iRowRun = DgProductLot.TopRows.Count;

                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row �� �ٲٱ�
                                    DgProductLot.SelectedIndex = iRowRun;
                                    DgProductLot.Selection.Clear();
                                    ProdListClickedProcess(iRowRun);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(DgProductLot.Rows[iRowRun].DataItem, "CHK", true);

                                    //row �� �ٲٱ�
                                    DgProductLot.SelectedIndex = iRowRun;
                                    DgProductLot.Selection.Clear();
                                    ProdListClickedProcess(iRowRun);
                                }
                                _isSucAutoConfirm = false;
                            }
                            else
                            {
                                // ���� ���� ���� ���� ��ȸ ó��.
                                GetCurrInputLotList();
                                GetWaitPancakeList();
                            }
                        }
                        else
                        {
                            int idx = _util.GetDataGridRowIndex(DgProductLot, "LOTID", selectedLot);
                            if (idx >= 0)
                            {
                                DataTableConverter.SetValue(DgProductLot.Rows[idx].DataItem, "CHK", true);

                                //row �� �ٲٱ�
                                DgProductLot.SelectedIndex = idx;
                                ProdListClickedProcess(idx);

                                // Checked Event ��� �Ұ��� ���� CurrentCellChanged ��������� �߻��ϴ� ���� Cell Cheked  �� Unchecked �� Event ��Ÿ�� ������ ó��..
                                DgProductLot.CurrentCell = DgProductLot.GetCell(idx, DgProductLot.Columns.Count - 1);
                            }
                            else
                            {
                                // ���� ���� ���� ���� ��ȸ ó��.
                                GetCurrInputLotList();
                                GetWaitPancakeList();
                            }
                        }
                    }
                    GetLossCount();
                    HiddenLoadingIndicator();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        public DataRow GetSelectProductRow()
        {
            DataRow row = null;

            try
            {
                row = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK");
                return row;
            }
            catch (Exception)
            {
                return row;
            }
        }

        private void GetInputSelectInfo(int row)
        {
            if (UcAssyDataCollectInline != null)
            {
                UcAssyDataCollectInline.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollectInline.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollectInline.ProdLotId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "LOTID").GetString();
                UcAssyDataCollectInline.ProdWorkInProcessSequence = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WIPSEQ").GetString();
                UcAssyDataCollectInline.ProdWorkOrderId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WOID").GetString();
                UcAssyDataCollectInline.ProdWorkOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[row].DataItem, "WO_DETL_ID").GetString();
                UcAssyDataCollectInline.SearchAllDataCollect();
            }
        }

        private void SumDefectQty()
        {
            try
            {
                DataTable dtTmp = DataTableConverter.Convert(DgDefect.ItemsSource);
               
                if(CommonVerify.HasTableRow(dtTmp))
                {
                    //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT'").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG <> 'Y' ").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT'").GetString()));
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString().Equals("") ? 0 : double.Parse(dtTmp.Compute("SUM(RESNQTY)", string.Empty).GetString()));
                }
                else
                {
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD", 0);
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY", 0);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetTrayFormLoad(string trayId, string trayTag, string outLotId, string wipQty)
        {
            try
            {
                int rowIdx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                if (rowIdx >= 0)
                {
                    string selectedLotId = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "LOTID").GetString();
                    string workOrderDetailId = DataTableConverter.GetValue(DgProductLot.Rows[rowIdx].DataItem, "WO_DETL_ID").GetString();

                    CMM_TRAY_CELL_INFO popupTrayCellInfo = new CMM_TRAY_CELL_INFO
                    {
                        FrameOperation = FrameOperation
                    };

                    // SET PARAMETER
                    object[] parameters = new object[10];
                    parameters[0] = _processCode;
                    parameters[1] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                    parameters[2] = Util.NVC(ComboEquipment.SelectedValue);
                    parameters[3] = Util.NVC(ComboEquipment.Text);
                    parameters[4] = selectedLotId;
                    parameters[5] = outLotId;
                    parameters[6] = trayId;
                    parameters[7] = trayTag;
                    parameters[8] = workOrderDetailId;
                    parameters[9] = wipQty;

                    C1WindowExtension.SetParameters(popupTrayCellInfo, parameters);

                    popupTrayCellInfo.Closed += TrayCellInfo_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupTrayCellInfo.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void TrayCellInfo_Closed(object sender, EventArgs e)
        {
            CMM_TRAY_CELL_INFO popup = sender as CMM_TRAY_CELL_INFO;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetWorkOrder();
                GetProductLot();
            }
            grdMain.Children.Remove(popup);

            if (UcAssyDataCollectInline.DispatcherTimer != null && UcAssyDataCollectInline.DispatcherTimer.Interval.TotalSeconds > 0)
                UcAssyDataCollectInline.DispatcherTimer.Start();
        }

        public bool GetSearchConditions(out string processCode, out string equipmentSegmentCode, out string equipmentCode)
        {
            try
            {
                processCode = _processCode;
                equipmentSegmentCode = ComboEquipmentSegment.SelectedIndex >= 0 ? ComboEquipmentSegment.SelectedValue.ToString() : string.Empty;
                equipmentCode = ComboEquipment.SelectedIndex >= 0 ? ComboEquipment.SelectedValue.ToString() : string.Empty;

                return true;
            }
            catch (Exception)
            {
                processCode = string.Empty;
                equipmentSegmentCode = string.Empty;
                equipmentCode = string.Empty;
                return false;
            }
        }

        public bool ClearControls()
        {
            try
            {
                Util.gridClear(DgProductLot);
                //UcAssyResultDetail.ClearResultDetailControl();
                UcAssyDataCollectInline.ClearDataCollectControl();
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public void ClearResultCollectControls()
        {
            UcAssyDataCollectInline.ClearDataCollectControl();
        }

        public void GetAllInfoFromChild()
        {
            GetProductLot();
        }

        public void ProdListClickedProcess(int iRow)
        {
            try
            {
                if (iRow < 0 || !_util.GetDataGridCheckValue(DgProductLot, "CHK", iRow)) return;
                string lotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                string wipSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));

                UcAssyDataCollectInline.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollectInline.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollectInline.EquipmentCodeName = ComboEquipment.Text;
                UcAssyDataCollectInline.ProcessCode = _processCode;
                UcAssyDataCollectInline.ProdWorkOrderId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WOID"));
                UcAssyDataCollectInline.ProdWorkOrderDetailId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WO_DETL_ID"));
                UcAssyDataCollectInline.ProdLotState = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSTAT"));
                UcAssyDataCollectInline.ProdLotId = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "LOTID"));
                UcAssyDataCollectInline.ProdWorkInProcessSequence = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "WIPSEQ"));
                //UcAssyDataCollectInline.CellManagementTypeCode = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[iRow].DataItem, "CELL_MNGT_TYPE_CODE"));
                UcAssyDataCollectInline.ProdSelectedCheckRowIdx = iRow;

                // UcAssyDataCollectInline ���� ��ȸ
                GetInputSelectInfo(iRow);

                // ������� UcAssyResultDetail ���� ��Ʈ�� ���ε�
                UcAssyResultDetail.ClearResultDetailControl();
                UcAssyResultDetail.SetResultDetailControl(DgProductLot.Rows[iRow].DataItem);

                // ������� UcAssyResultDetail ���� �׸���(DgDefectDetail) �ʱ�ȭ 
                SetDefectDetail();
                // UcAssyDataCollectInline ���� �ҷ�/LOSS/��ǰû�� ��ȸ
                GetDefectInfo(lotId, wipSeq);
                // ���� ��ǰ �ҷ� ���� ���
                // CalculateDefectQty();

                //Permit Rate Added
                _LOTID = lotId;
                _WipSeq = wipSeq;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetCurrInputLotList()
        {
            if (UcAssyDataCollectInline != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyDataCollectInline.ProdLotId = string.Empty;
                    UcAssyDataCollectInline.ProdWorkInProcessSequence = string.Empty;
                    UcAssyDataCollectInline.ProdWorkOrderId = string.Empty;
                    UcAssyDataCollectInline.ProdWorkOrderDetailId = string.Empty;
                    UcAssyDataCollectInline.ProdLotState = string.Empty;
                }
                UcAssyDataCollectInline.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollectInline.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollectInline.ProcessCode = _processCode;
                UcAssyDataCollectInline.GetMaterialInputList();
            }
        }

        private void GetWaitPancakeList()
        {
            if (UcAssyDataCollectInline != null)
            {
                if (!CommonVerify.HasDataGridRow(DgProductLot))
                {
                    UcAssyDataCollectInline.ProdLotId = string.Empty;
                    UcAssyDataCollectInline.ProdWorkInProcessSequence = string.Empty;
                    UcAssyDataCollectInline.ProdWorkOrderId = string.Empty;
                    UcAssyDataCollectInline.ProdWorkOrderDetailId = string.Empty;
                    UcAssyDataCollectInline.ProdLotState = string.Empty;
                }
                UcAssyDataCollectInline.EquipmentSegmentCode = ComboEquipmentSegment.SelectedValue.GetString();
                UcAssyDataCollectInline.EquipmentCode = ComboEquipment.SelectedValue.GetString();
                UcAssyDataCollectInline.ProcessCode = _processCode;
                UcAssyDataCollectInline.GetWaitPancake();
            }
        }


        //===================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
        //      2. <�۾���/�۾��� �Է�>
        //===================================================================
        private bool GetEqptWrkInfo()
        {
            //�۾��� ����� �ڵ�����Ȯ�� üũ��
            bool is_DataShiftOrWorker = false;
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable indataTable = new DataTable("RQSTDT");
                indataTable.Columns.Add("LANGID", typeof(string));
                indataTable.Columns.Add("EQPTID", typeof(string));
                indataTable.Columns.Add("SHOPID", typeof(string));
                indataTable.Columns.Add("AREAID", typeof(string));
                indataTable.Columns.Add("EQSGID", typeof(string));
                indataTable.Columns.Add("PROCID", typeof(string));

                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["EQPTID"] = Util.NVC(ComboEquipment.SelectedValue);
                indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                indata["EQSGID"] = Util.NVC(ComboEquipmentSegment.SelectedValue);
                indata["PROCID"] = _processCode;

                indataTable.Rows.Add(indata);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", indataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcAssyShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcAssyShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcAssyShift.TextShiftEndTime.Text = string.Empty;
                                }
                                
                                if (!string.IsNullOrEmpty(UcAssyShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcAssyShift.TextShiftEndTime.Text))
                                {
                                    UcAssyShift.TextShiftDateTime.Text = UcAssyShift.TextShiftStartTime.Text + " ~ " + UcAssyShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcAssyShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcAssyShift.TextWorker.Text = string.Empty;
                                    UcAssyShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcAssyShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                    //=====================================================================================================================
                                    // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                                    //=====================================================================================================================
                                    //�۾��� ����� �ڵ�����Ȯ�� üũ��
                                    is_DataShiftOrWorker = true;
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcAssyShift.TextShift.Tag = string.Empty;
                                    UcAssyShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcAssyShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcAssyShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                    //=====================================================================================================================
                                    // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
                                    //=====================================================================================================================
                                    //�۾��� ����� �ڵ�����Ȯ�� üũ��
                                    is_DataShiftOrWorker = true;
                                }
                            }
                            else
                            {
                                UcAssyShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
                //�۾��� ����� �ڵ�����Ȯ�� üũ��
                return is_DataShiftOrWorker;
            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();
                Util.MessageException(ex);
                //�۾��� ����� �ڵ�����Ȯ�� üũ��
                return is_DataShiftOrWorker;
            }

        }

        public void GetDefectInfo(string lotId, string wipSeq)
        {
            try
            {
                const string bizRuleName = "BR_PRD_GET_WIPRESONCOLLECT_BY_MNGT_TYPE";
                DataTable inTable = _bizDataSet.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _processCode;
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                newRow["LOTID"] = lotId;
                newRow["WIPSEQ"] = wipSeq;
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";

                inTable.Rows.Add(newRow);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTTYPE,OUTDATA", (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableInDataSet(bizResult))
                    {
                        //'AP' ���� / ������
                        //'LP' ���� / ������
                        DgDefect.Columns["CLSS_NAME1"].Visibility = bizResult.Tables["OUTTYPE"].Rows[0]["DFCT_CODE_MNGT_TYPE"].GetString() == "LP" ? Visibility.Visible : Visibility.Collapsed;
                        DgDefect.ItemsSource = DataTableConverter.Convert(bizResult.Tables["OUTDATA"]);
                    }

                    CalculateDefectQty();

                }, ds, FrameOperation.MENUID);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void CalculateDefectQty()
        {
            // ��ǰ����
            Decimal totalCellQty = 0;

            if (CommonVerify.HasDataGridRow(UcAssyDataCollectInline.DgProdCellWinding))
            {
                DataTable dt = ((DataView)UcAssyDataCollectInline.DgProdCellWinding.ItemsSource).Table;
                totalCellQty = dt.AsEnumerable().Sum(s => s.Field<Decimal>("CELLQTY"));
            }

            SumDefectQty();

            if (CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                if (DgDefectDetail.Rows.Count - DgDefectDetail.TopRows.Count > 0)
                {
                    double defect = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_DEFECT").GetDouble();
                    double loss = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_LOSS").GetDouble();
                    double chargeprd = DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DTL_CHARGEPRD").GetDouble();
                    //double defectQty = Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "DEFECTQTY")));
                    double defectQty = defect + loss + chargeprd;

                    double goodQty = (double)totalCellQty;
                    double outputQty = goodQty + defectQty;
                    double eqptQty = 0;
                    double inputQtyByType = GetInputQtyByApplyTypeCode().GetDouble();

                    int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
                    if (rowIndex > 0)
                    {
                        eqptQty = DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY").GetDouble();
                        DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", eqptQty + inputQtyByType);
                        //DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "EQPTQTY", DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "EQPT_END_QTY"));
                    }

                    //��ǰ����
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY", goodQty);
                    //�������
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "OUTPUTQTY", outputQty);
                    //���Լ���
                    DataTableConverter.SetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "INPUTQTY", outputQty);

                    //�߰��ҷ�
                    if (Math.Abs((eqptQty + inputQtyByType) - (defectQty + goodQty)) > 0)
                    {
                        UcAssyResultDetail.TextAssyResultQty.Text = ((eqptQty + inputQtyByType) - (defectQty + goodQty)).ToString("##,###");
                        UcAssyResultDetail.TextAssyResultQty.FontWeight = FontWeights.Bold;
                        UcAssyResultDetail.TextAssyResultQty.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else
                    {
                        UcAssyResultDetail.TextAssyResultQty.Text = "0";
                        UcAssyResultDetail.TextAssyResultQty.FontWeight = FontWeights.Normal;
                        var convertFromString = System.Windows.Media.ColorConverter.ConvertFromString("#FF4D4C4C");
                        if (convertFromString != null)
                            UcAssyResultDetail.TextAssyResultQty.Foreground = new SolidColorBrush((Color)convertFromString);
                    }
                }
            }
        }

        private decimal GetEtcDefect()
        {
            decimal defectDecimal = 0;

            if (CommonVerify.HasDataGridRow(DgDefect))
            {
                //�߰��ҷ� = �������Լ��� - (��ǰ���� + LOSS + ��ǰû��) - (�ҷ� - ��Ÿ�ҷ�)
                DataTable dt = ((DataView)DgDefect.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("RESNCODE") == "P999000AJR"
                             select new
                             {
                                 ResnQty = t.Field<decimal>("RESNQTY")
                             }).FirstOrDefault();
                if (query != null)
                {
                    defectDecimal = query.ResnQty;
                }
            }
            return defectDecimal;
        }

        private decimal GetInputQtyByApplyTypeCode()
        {
            decimal qty = 0;

            if (CommonVerify.HasDataGridRow(DgDefect))
            {
                DataTable dt = ((DataView)DgDefect.ItemsSource).Table;
                decimal qtyPlus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "PLUS").Sum(s => s.Field<decimal>("RESNQTY"));
                decimal qtyMinus = dt.AsEnumerable().Where(s => s.Field<string>("INPUT_QTY_APPLY_TYPE_CODE") == "MINUS").Sum(s => s.Field<decimal>("RESNQTY"));

                return qtyPlus - qtyMinus;
            }
            return qty;
        }

        private string GetConfirmDate()
        {
            string confirmDate;

            const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO";
            string prodLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = prodLotId;
            dr["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            confirmDate = string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).Trim()) ? Util.NVC(searchResult.Rows[0]["NOW_CALDATE"]).GetString() : Util.NVC(searchResult.Rows[0]["CALDATE_LOT"]).GetString();

            return confirmDate;
        }

        private string GetConfirmLonInfo()
        {
            string confirmLotWipStat;

            const string bizRuleName = "DA_PRD_SEL_CONFIRM_LOT_INFO";
            string prodLotId = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();

            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            DataRow dr = inDataTable.NewRow();
            dr["LOTID"] = prodLotId;
            dr["LANGID"] = LoginInfo.LANGID;
            inDataTable.Rows.Add(dr);

            DataTable searchResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            confirmLotWipStat = string.IsNullOrEmpty(Util.NVC(searchResult.Rows[0]["WIPSTAT"]).Trim()) ? "": Util.NVC(searchResult.Rows[0]["WIPSTAT"]).GetString();

            return confirmLotWipStat;
        }

        public DataRow GetSelectWorkOrderRow()
        {
            return _util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y");
        }

        private void PrintHistoryCard(string lotId)
        {
            try
            {
                string bizRuleName = _isSmallType ? "BR_PRD_SEL_WINDING_RUNCARD_WNS" : "BR_PRD_SEL_WINDING_RUNCARD_WN";
                DataSet ds = _bizDataSet.GetBR_PRD_SEL_WINDING_RUNCARD_WN();
                DataTable indataTable = ds.Tables["IN_DATA"];
                DataRow indata = indataTable.NewRow();
                indata["LANGID"] = LoginInfo.LANGID;
                indata["LOTID"] = lotId;
                indataTable.Rows.Add(indata);
                //ds.Tables.Add(indataTable);

                new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_DATA", "OUT_DATA,OUT_ELEC,OUT_DFCT,OUT_SEPA,OUT_TRAY", (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        CMM_ASSY_WINDERCARD_PRINT poopupHistoryCard = new CMM_ASSY_WINDERCARD_PRINT { FrameOperation = FrameOperation };
                        object[] parameters = new object[5];
                        parameters[0] = result;
                        parameters[1] = _isSmallType;
                        parameters[2] = lotId;
                        parameters[3] = _processCode;
                        parameters[4] = true;
                        C1WindowExtension.SetParameters(poopupHistoryCard, parameters);
                        poopupHistoryCard.Closed += poopupHistoryCard_Closed;
                        Dispatcher.BeginInvoke(new Action(() => poopupHistoryCard.ShowModal()));
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, ds);
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void poopupHistoryCard_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_WINDERCARD_PRINT popup = sender as CMM_ASSY_WINDERCARD_PRINT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private bool ValidationSearch()
        {

            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationRunStart()
        {
            /*
            if (_util.GetDataGridRowIndex(DgProductLot, "WIPSTAT", "PROC") > -1)
            {
                //Util.Alert("��� ���� �� �� LOT�� ���� �մϴ�.");
                Util.MessageValidation("SFU1863");
                return false;
            }
            */

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.GetString().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetWorkOrderGridSelectedRow(Ucworkorder.DgWorkOrder, "EIO_WO_SEL_STAT", "Y") == null)
            {
                Util.MessageValidation("SFU1635");
                return false;
            }

            if (!CheckSelectWorkOrderInfo())
            {
                return false;
            }

            return true;
        }

        private bool ValidationEquipmentEnd()
        {
            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            string wipState = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK")].DataItem, "WIPSTAT"));

            if (!wipState.Equals("PROC"))
            {
                Util.MessageValidation("SFU1866");
                return false;
            }

            return true;
        }

        private bool ValidationConfirm()
        {

            //===================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) :  
            //      6. <����Ȯ��>����
            //===================================================================
            // [�ڵ�����Ȯ��] Run �߿��� ���� [����Ȯ��] ó�� �Ұ� 
            if (_isAutoConfirmMode)
            {
                // �ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�Ȯ����� ���� �� �õ� �ٶ��ϴ�
                Util.MessageValidation("SFU5201");
                return false;
                //�ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�����Ȯ���� ���� �Ͻðڽ��ϱ�?SFU5201 ����� Ȯ��
                //Util.MessageConfirm("SFU5200", result1 =>
                //{
                //    if (result1 == MessageBoxResult.OK)
                //    {
                //        SetAutoConfirMode(false);
                //    }
                //});
                //return !_isAutoConfirmMode; //ON==  false, OFF ==true
            }

            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //����Ȯ�� �� LOT�� ���õ��� �ʾҽ��ϴ�.
                Util.MessageValidation("SFU1717");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                //Util.Alert("Ȯ�� �� �� �ִ� LOT���°� �ƴմϴ�.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextShift.Text))
            {
                //�۾����� �Է� �� �ּ���.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //�۾��ڸ� �Է� �� �ּ���.
                Util.MessageValidation("SFU1842");
                return false;
            }

            if (Math.Abs(UcAssyResultDetail.TextAssyResultQty.Text.GetDecimal() ) > 0)
            {
                //�߰��ҷ� ���� : �߰��ҷ� ������ �� Ȯ���Ͻ� �� �ݿ�(����)�� �ּ���.
                Util.MessageValidation("SFU3665");
                return false;
            }


            return true;
        }
        //=====================================================================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) : �۾���/�۾��� �Է�, Ư�̻����Է�, ǰ���˻� �Է�, ���ְ˻��Է�, ���Ϸ�
        // 7.1 [�ڵ�����Ȯ��]	üũ
        //=====================================================================================================================
        private bool ValidationAutoConfirm()
        {
            //MMD �ڵ�����Ȯ�� Ÿ�� Ȯ��
            if (!ValidationAutoResultConfrimTypeEQPT())
            {
                //MMD > ���� -[���� Ȯ�� ����] �� 'M : ����'���� �Ǿ� �ֽ��ϴ�. 'A : �ڵ�'���� ���� �� �ڵ����� ���� �ٶ��ϴ�.
                //Util.MessageValidation("SFU5205"); --
                return false;
            }

            //UI �ڵ�����Ȯ�����(����) ON Ȯ��
            if (!ValidationAutoResultConfrimFlagEio())
            {
                //�ڵ�����Ȯ�� ������ OFF (STOP) ���Դϴ�.
                //Util.MessageValidation("SFU5206"); --
                return false;
            }

            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (string.IsNullOrEmpty(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID").GetString()))
            {
                //����Ȯ�� �� LOT�� ���õ��� �ʾҽ��ϴ�.
                Util.MessageValidation("SFU1717");
                return false;
            }
            //[���Ϸ�] �� WIPSTAT ='PROC' , ���� DB Search
            //if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            if(GetConfirmLonInfo().Equals("EQPT_END") == false)
            {
                //Util.Alert("Ȯ�� �� �� �ִ� LOT���°� �ƴմϴ�.");
                Util.MessageValidation("SFU2045");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextShift.Text))
            {
                //�۾����� �Է� �� �ּ���.
                Util.MessageValidation("SFU1845");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssyShift.TextWorker.Text))
            {
                //�۾��ڸ� �Է� �� �ּ���.
                Util.MessageValidation("SFU1842");
                return false;
            }

            //���� ó�� �� ==> �����䱸 ����  20221129
            //if (Math.Abs(UcAssyResultDetail.TextAssyResultQty.Text.GetDecimal()) < 0) //0���� ū ���� (Loss-��Ÿ-�߰��ҷ� ���� �ڵ���� �� ���� ��)
            //{
            //    //�߰��ҷ� ���� : �߰��ҷ� ������ �� Ȯ���Ͻ� �� �ݿ�(����)�� �ּ���.
            //    Util.MessageValidation("SFU3665");
            //    return false;
            //}

            //��ǰ ������ 0�� ��� Ư�̻��� �Է� �ʼ� üũ 
            if (DataTableConverter.GetValue(DgDefectDetail.Rows[DgDefectDetail.TopRows.Count].DataItem, "GOODQTY").GetDecimal() == 0)
            {
                if (string.IsNullOrEmpty(new TextRange(UcAssyResultDetail.TextRemark.Document.ContentStart, UcAssyResultDetail.TextRemark.Document.ContentEnd).Text.Trim()))
                {
                    Util.MessageValidation("SFU1993");  //Ư�̻����� �Է��ϼ���.
                    return false;
                }
            }
            //���� ó�� �� ==> �����䱸 ����  20221129
            //double AddDefectQty = 0;
            //double.TryParse(UcAssyResultDetail.TextAssyResultQty.Text, out AddDefectQty);
            //if (AddDefectQty < 0)
            //{
            //    Util.MessageValidation("SFU3665");  //�߰��ҷ� ���� : �߰��ҷ� ������ �� Ȯ���Ͻ� �� �ݿ�(����)�� �ּ���.
            //    return false;
            //}


            return true;
        }

        private bool ValidationInspectionLot()
        {
            const string bizRuleName = "DA_QCA_CHK_MAND_INSP_ITEM_RESULT_LOT";

            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                Util.MessageInfo("SFU3669", dtResult.Rows[0]["CLCTNAME"].GetString());
                return false;
            }
            else
            {
                return true;
            }
        }

        private bool ValidationInspectionTime()
        {

            const string bizRuleName = "BR_QCA_CHK_MAND_INSP_ITEM_RESULT_TIME";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow dr = inDataTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["PROCID"] = _processCode;
            dr["EQPTID"] = ComboEquipment.SelectedValue;
            dr["LOTID"] = _util.GetDataGridFirstRowBycheck(DgProductLot, "CHK").Field<string>("LOTID").GetString();
            inDataTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);
            if (CommonVerify.HasTableRow(dtResult))
            {
                //Util.MessageInfo("SFU1605", dtResult.Rows[0]["CLCTNAME"].GetString());
                //return dtResult.Rows[0]["CLCTNAME"].GetString();
                String stnMsg = dtResult.Rows[0]["CLCTNAME1"].GetString();
                if (String.IsNullOrEmpty(dtResult.Rows[0]["CLCTNAME2"].GetString()) == false)
                {
                    stnMsg = stnMsg + "-" + dtResult.Rows[0]["CLCTNAME2"].GetString();
                }
                if (String.IsNullOrEmpty(dtResult.Rows[0]["CLCTNAME3"].GetString()) == false)
                {
                    stnMsg = stnMsg + "-" + dtResult.Rows[0]["CLCTNAME3"].GetString();
                }
                Util.MessageInfo("SFU5079", stnMsg); //Time ���� �ʼ� �˻� �׸� �Է� �ʿ� : [%1]
                return false;
            }
            else
            {
                //return string.Empty;
                return true;
            }
        }

        private bool ValidationCancelRun()
        {

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");

            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[rowIndex].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("�۾����� LOT�� �ƴմϴ�.");
                Util.MessageValidation("SFU1846");
                return false;
            }


            return true;
        }

        private bool ValidationEquipmentCondition()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationQualityInfo()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationSelfInspection()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return false;
            }
            return true;
        }

        private bool ValidationEquipmentEndCancel()
        {
            int idx = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("EQPT_END"))
            {
                Util.MessageValidation("SFU1864");  // ���Ϸ� ������ LOT�� �ƴմϴ�.
                return false;
            }
            return true;
        }

        private bool ValidationTrayLocation()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }


        private bool ValidationQualitySearch()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }
            return true;
        }

        private bool ValidationTestMode()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }
            //=====================================================================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.  [�ڵ�����Ȯ��]ȣ��(��ưx,Ÿ �̺�Ʈ0) : �۾���/�۾��� �Է�, Ư�̻����Է�, ǰ���˻� �Է�, ���ְ˻��Է�, ���Ϸ�
            // 7.1 [�ڵ�����Ȯ��]	üũ
            //=====================================================================================================================
            if (_isAutoConfirmMode)
            {
                // �ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�Ȯ����� ���� �� �õ� �ٶ��ϴ�
                Util.MessageValidation("SFU5201");
                return false;
                //�ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�����Ȯ���� ���� �Ͻðڽ��ϱ�?SFU5201 ����� Ȯ��
                //Util.MessageConfirm("SFU5200", result1 =>
                //{
                //    if (result1 == MessageBoxResult.OK)
                //    {
                //        SetAutoConfirMode(false);
                //    }
                //});
                //return !_isAutoConfirmMode; //ON==  false, OFF ==true
            }
            return true;
        }

        //====================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 4.   [�ڵ�����Ȯ�����] ��ư ����
        // 4.1. [�ڵ�����Ȯ�����] üũ
        //====================================================================
        private bool ValidationAutoConfirmMode()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            //MMD ���� üũ DA_PRD_SEL_EQUIPMENT_STDINFO_AUTO_CNFM --���� (�ڵ�����Ȯ�� BIZ ���� üũ ��. (BIZ Ȯ�� �� UI üũ �ʿ�
            if (!ValidationAutoResultConfrimTypeEQPT())
            {
                //Util.Alert("MMD > ���� -[���� Ȯ�� ����] �� 'M : ����'���� �Ǿ� �ֽ��ϴ�. 'A : �ڵ�'���� ���� �� �ڵ����� ���� �ٶ��ϴ�.");
                //Util.MessageValidation("SFU5205");
                return false;
            }
            return true;
        }

        private bool ValidationScheduledShutdownMode()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }
            //=====================================================================================================================
            // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
            // 7.1 [�ڵ�����Ȯ��]���-	��ȹ���� üũ
            //=====================================================================================================================
            if (_isAutoConfirmMode)
            {
                // �ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�Ȯ����� ���� �� �õ� �ٶ��ϴ�
                Util.MessageValidation("SFU5201");
                return false;
                //�ڵ�����Ȯ�� Run ���Դϴ�. �ڵ�����Ȯ���� ���� �Ͻðڽ��ϱ�?SFU5201 ����� Ȯ��
                //Util.MessageConfirm("SFU5200", result1 =>
                //{
                //    if (result1 == MessageBoxResult.OK)
                //    {
                //        SetAutoConfirMode(false);
                //    }
                //});
                //return !_isAutoConfirmMode; //ON==  false, OFF ==true
            }

            return true;
        }

        //=====================================================================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // MMD  [����Ȯ������]	üũ
        //=====================================================================================================================
        private bool ValidationAutoResultConfrimTypeEQPT()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_EQUIPMENT_STDINFO";
                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SET_WORKORDER_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (!Util.NVC(dtRslt.Rows[0]["RSLT_CNFM_TYPE"]).Equals("A"))
                    {
                        //Util.Alert("MMD > ���� -[���� Ȯ�� ����] �� 'M : ����'���� �Ǿ� �ֽ��ϴ�. 'A : �ڵ�'���� ���� �� �ڵ����� ���� �ٶ��ϴ�.");
                        Util.MessageValidation("SFU5205");
                    }
                    else
                    {
                        bRet = true;
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        //=====================================================================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // UI [�ڵ�����Ȯ�����]	üũ
        //=====================================================================================================================
        private bool ValidationAutoResultConfrimFlagEio()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_EIOATTR_AUTO_RSLT_CNFM";
                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SET_WORKORDER_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (!Util.NVC(dtRslt.Rows[0]["AUTO_RSLT_CNFM_FLAG"]).Equals("Y"))
                    {
                        //Util.Alert("�ڵ�����Ȯ�� ������ OFF (STOP) ���Դϴ�.");
                        Util.MessageValidation("SFU5206");
                    }
                    else
                    {
                        bRet = true;
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckSelectWorkOrderInfo()
        {
            try
            {
                const string bizRuleName = "DA_PRD_SEL_SET_WORKORDER_INFO";
                bool bRet = false;
                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_SET_WORKORDER_INFO();
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = ComboEquipment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]).Equals(""))
                    {
                        //Util.Alert("���õ� W/O�� �����ϴ�.");
                        Util.MessageValidation("SFU1635");
                    }
                    else if (Util.NVC(dtRslt.Rows[0]["WO_STAT_CHK"]).Equals("N"))
                    {
                        Util.MessageValidation("SFU1635");
                    }
                    else
                    {
                        bRet = true;
                    }
                }
                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationHistoryCard()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationSaveWipHistory()
        {
            if (ComboEquipmentSegment.SelectedIndex < 0 || ComboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (ComboEquipment.SelectedIndex < 0 || ComboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            int rowIndex = _util.GetDataGridFirstRowIndexWithTopRow(DgProductLot, "CHK");
            if (rowIndex < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!CommonVerify.HasDataGridRow(DgDefectDetail))
            {
                Util.MessageValidation("SFU3552");
                return false;
            }

            return true;
        }

        private void GetLossCount()
        {
            try
            {
                DataTable dtLossCount = Util.Get_EqpLossCnt(ComboEquipment.SelectedValue.GetString());
                if (CommonVerify.HasTableRow(dtLossCount))
                {
                    UcAssyShift.TextLossCnt.Text = Util.NVC(dtLossCount.Rows[0]["CNT"]);
                    UcAssyShift.TextLossCnt.Background = dtLossCount.Rows[0]["CNT"].GetInt() > 0 ? new SolidColorBrush(Colors.LightPink) : new SolidColorBrush(Colors.WhiteSmoke);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetDefectDetail()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("INPUTQTY", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_P", typeof(int));
            inDataTable.Columns.Add("ALPHAQTY_M", typeof(int));
            inDataTable.Columns.Add("OUTPUTQTY", typeof(int));
            inDataTable.Columns.Add("GOODQTY", typeof(int));
            inDataTable.Columns.Add("EQPTQTY", typeof(int));
            inDataTable.Columns.Add("DTL_DEFECT", typeof(int));
            inDataTable.Columns.Add("DTL_LOSS", typeof(int));
            inDataTable.Columns.Add("DTL_CHARGEPRD", typeof(int));
            inDataTable.Columns.Add("DEFECTQTY", typeof(int));

            DataRow dtRow = inDataTable.NewRow();
            dtRow["INPUTQTY"] = 0;
            dtRow["ALPHAQTY_P"] = 0;
            dtRow["ALPHAQTY_M"] = 0;
            dtRow["OUTPUTQTY"] = 0;
            dtRow["GOODQTY"] = 0;
            dtRow["EQPTQTY"] = 0;
            dtRow["DTL_DEFECT"] = 0;
            dtRow["DTL_LOSS"] = 0;
            dtRow["DTL_CHARGEPRD"] = 0;
            dtRow["DEFECTQTY"] = 0;

            inDataTable.Rows.Add(dtRow);
            DgDefectDetail.ItemsSource = DataTableConverter.Convert(inDataTable);
        }

        private void SetComboBox()
        {
            CommonCombo combo = new CommonCombo();

            // 2017.03.05 ������(���δ� IN-LINE)
            // CNJ ����9,10ȣ ȭ�δ��� �ʼ����� ����(CST_ID)  //���� : CR , �ʼ��� : CS
            const string gubun = "CR";
            
            //String[] sFilter = { LoginInfo.CFG_AREA_ID, gubun, _processCode};
            String[] sFilter = { LoginInfo.CFG_AREA_ID , gubun, _processCode, "CST_ID"};
            C1ComboBox[] cboLineChild = { ComboEquipment };
            combo.SetCombo(ComboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, sFilter: sFilter);

            String[] sFilter2 = { _processCode};
            C1ComboBox[] cboEquipmentParent = { ComboEquipmentSegment };
            combo.SetCombo(ComboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent, sFilter: sFilter2);            
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void ApplyPermissions()
        {
            var ucAssyCommand = grdCommand.Children[0] as UcAssyCommand;
            if (ucAssyCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                    ucAssyCommand.ButtonStart,
                    ucAssyCommand.ButtonCancel,
                    ucAssyCommand.ButtonConfirm
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
        }

        private void ColorAnimationInredRectangle()
        {
            recTestMode.Fill = redBrush;
            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromSeconds(0.8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void HideTestMode()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value <= 0) return;

                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isTestMode = false;

        }
        //=========================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� 
        // 3.[�ڵ�����Ȯ�����] ��ȸ
        //    3.1.�ڵ�����Ȯ�����(ȭ��ǥ��) �����
        //=========================================================
        private void HideAutoConfirmMode()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value <= 0) return;

                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isAutoConfirmMode = false;

        }

        private void ShowTestMode()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value > 0) return;

                //MainContents.RowDefinitions[1].Height = new GridLength(8);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.3, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += ShowTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            _isTestMode = true;
        }

        //=========================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� 
        // 3.[�ڵ�����Ȯ�����] ��ȸ
        //    3.1.�ڵ�����Ȯ�����(ȭ��ǥ��) �����
        //=========================================================
        private void ShowAutoConfirmMode()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //C20221206-000611 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� - �ϴ� �ڵ�����Ȯ�� ǥ�� SIZE ����
                //if (MainContents.RowDefinitions[3].Height.Value > 0) return;

                //C20221206-000611 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� - �ϴ� �ڵ�����Ȯ�� ǥ�� SIZE ����
                MainContents.RowDefinitions[3].Height = new GridLength(0.1);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    //C20221206-000611 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� - �ϴ� �ڵ�����Ȯ�� ǥ�� SIZE ����
                    //To = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0.1, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += ShowTestAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);

                //ColorAnimationInredRectangle();
            }));

            _isAutoConfirmMode = true;
        }

        private void ShowTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInredRectangle();
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void SetTestMode(bool bOn, bool bShutdownMode = false)
        {
            try
            {
                string bizRuleName;
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_MODE", typeof(string));
                inTable.Columns.Add("UI_LOSS_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bShutdownMode)
                {
                    bizRuleName = "BR_EQP_REG_EQPT_OPMODE_LOSS";

                    newRow["IFMODE"] = "ON";
                    newRow["UI_LOSS_MODE"] = bOn ? "ON" : "OFF";
                    newRow["UI_LOSS_CODE"] = bOn ? Util.ConvertEqptLossLevel2Change("LC003") : ""; // ��ȹ���� loss �ڵ�.
                }
                else
                {
                    bizRuleName = "BR_EQP_REG_EQPT_OPMODE";
                    newRow["IFMODE"] = bOn ? "TEST" : "ON";
                }

                newRow["EQPTID"] = ComboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync(bizRuleName, "IN_EQP", null, inTable);

                Util.MessageInfo("PSS9097");    // ����Ǿ����ϴ�.
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

        //====================================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ����
        // 4.  [�ڵ�����Ȯ�����] ��ư ����
        // 4.2.[�ڵ�����Ȯ�����] ����/����
        //====================================================================
        private void SetAutoConfirMode(bool bOn)
        {
            try
            {
                string bizRuleName = "BR_PRD_REG_EIOATTR_AUTO_RSLT_CNFM_FLAG";
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string)); //�� �ʿ��Ѱ� Ȯ�� �ʿ�
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AUTO_RSLT_CNFM_FLAG", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                if (bOn)
                {

                    newRow["IFMODE"] = "ON"; // �� �ʿ��Ѱ� Ȯ�� �ʿ�
                    newRow["AUTO_RSLT_CNFM_FLAG"] = bOn ? "Y" : "N";
                }
                else
                {
                    newRow["IFMODE"] = bOn ? "ON" : "TEST";// �� �ʿ��Ѱ� Ȯ�� �ʿ�
                    newRow["AUTO_RSLT_CNFM_FLAG"] = bOn ? "Y" : "N";
                }

                newRow["EQPTID"] = ComboEquipment.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
                Util.MessageInfo("PSS9097");    // ����Ǿ����ϴ�.
                #region ���� �ּ� ó�� �� --GetAutoConfirmMode() ���� ó�� �� (OUT_RSLT 
                //if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("OUT_RSLT"))
                //{
                //    if (Util.NVC(dtRslt.Rows[0]["OUT_RSLT"]).Equals("-1")==false && ((bOn && Util.NVC(dtRslt.Rows[0]["OUT_RSLT"]).Equals("Y")) || (bOn ==false && Util.NVC(dtRslt.Rows[0]["OUT_RSLT"]).Equals("N"))))
                //    {
                //        //�ڵ�����Ȯ����� ON ���� ����� 
                //        if((bOn && Util.NVC(dtRslt.Rows[0]["OUT_RSLT"]).Equals("Y")))
                //        {
                //            ShowAutoConfirmMode();
                //            Util.MessageInfo("SFU5202");    // �ڵ�����Ȯ����尡 ON (RUN) ���� ���� �Ǿ����ϴ�.
                //        }

                //        //�ڵ�����Ȯ����� OFF ���� ����� 
                //        if ((!bOn && Util.NVC(dtRslt.Rows[0]["OUT_RSLT"]).Equals("N")))
                //        {
                //            HideAutoConfirmMode();
                //            Util.MessageInfo("SFU5203");    // �ڵ�����Ȯ����尡 OFF (STOP)���� ���� �Ǿ����ϴ�.
                //        }
                //        // ��� ����
                //        //Util.MessageInfo("PSS9097");    // ����Ǿ����ϴ�.
                //        System.Threading.Thread.Sleep(500);
                //    }
                //    else
                //    {
                //        Util.MessageInfo("10011");    // ����ȳ����� �����ϴ�.
                //    }
                //}
                #endregion
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

        private void GetTestMode()
        {
            try
            {
                if (ComboEquipment?.SelectedValue == null) return;
                if (Util.NVC(ComboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideTestMode();
                    return;
                }

                DataTable inTable = _bizDataSet.GetDA_EQP_SEL_TESTMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = ComboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO_S", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN"))
                {
                    Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        ShowTestMode();
                    }
                    else
                    {
                        //HideTestMode();

                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            ShowScheduledShutdown();
                        }
                        else
                        {
                            HideScheduledShutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //=========================================================
        // C20220822-000365 ������ 9,10ȣ ���� �ڵ� Ȯ�� ��� ���� 
        // 3.[�ڵ�����Ȯ�����] ��ȸ
        //=========================================================
        private void GetAutoConfirmMode()
        {
            try
            {
                if (ComboEquipment?.SelectedValue == null) return;
                if (Util.NVC(ComboEquipment?.SelectedValue).Trim().Equals("SELECT"))
                {
                    HideAutoConfirmMode();
                    return;
                }

                DataTable inTable = _bizDataSet.GetDA_EQP_SEL_AUTO_CONFIRMMODE();

                DataRow searchCondition = inTable.NewRow();
                searchCondition["EQPTID"] = ComboEquipment.SelectedValue;

                inTable.Rows.Add(searchCondition);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_AUTO_RSLT_CNFM", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("TEST_MODE") && dtRslt.Columns.Contains("MODE_TYPE") && dtRslt.Columns.Contains("SCHEDULED_SHUTDOWN") && dtRslt.Columns.Contains("AUTO_RSLT_CNFM_FLAG"))
                {
                    Util.NVC(dtRslt.Rows[0]["MODE_TYPE"]);

                    if (Util.NVC(dtRslt.Rows[0]["TEST_MODE"]).Equals("Y"))
                    {
                        //�׽�Ʈ ��忡���� [�ڵ� ���� Ȯ��]�� ��� �� �� �����ϴ�.
                        //if (Util.NVC(dtRslt.Rows[0]["AUTO_RSLT_CNFM_FLAG"]).Equals("Y"))
                        //{
                        //    SetAutoConfirMode(false);
                        //}
                    }
                    else
                    {
                        if (Util.NVC(dtRslt.Rows[0]["SCHEDULED_SHUTDOWN"]).Equals("Y"))
                        {
                            // ��ȹ ���� ��忡����[�ڵ� ���� Ȯ��]�� ��� �� �� �����ϴ�.
                            //if (Util.NVC(dtRslt.Rows[0]["AUTO_RSLT_CNFM_FLAG"]).Equals("Y"))
                            //{
                            //    SetAutoConfirMode(false);
                            //}

                        }
                        else
                        {
                            if (Util.NVC(dtRslt.Rows[0]["AUTO_RSLT_CNFM_FLAG"]).Equals("Y"))
                            {
                                txtEqptMode.Text = ObjectDic.Instance.GetObjectName("�ڵ�����Ȯ���������");
                                //ShowMode �ϰ�� �� showMod�� ȣ���ϸ� ȭ�鿡�� ������� ������ ���� ? ��� �ٲ�� �����.
                                //if (_isAutoConfirmMode) return;

                                ShowAutoConfirmMode();

                            }
                            else
                            {
                                HideAutoConfirmMode();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowScheduledShutdown()
        {
            txtEqptMode.Text = ObjectDic.Instance.GetObjectName("��ȹ����");

            Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bScheduledShutdown) return;
                if (MainContents.RowDefinitions[3].Height.Value > 0)
                {
                    ColorAnimationInRectangle(false);
                    return;
                }

                MainContents.RowDefinitions[1].Height = new GridLength(8);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.3, GridUnitType.Star),
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += ShowScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);

            }));

            _isTestMode = true;
        }

        private void HideScheduledShutdown()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainContents.RowDefinitions[3].Height.Value <= 0) return;

                MainContents.RowDefinitions[1].Height = new GridLength(0);
                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0.3, GridUnitType.Star),
                    To = new GridLength(0, GridUnitType.Star),
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };
                gla.Completed += HideScheduledShutdownAnimationCompleted;
                MainContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));

            _isTestMode = false;

        }

        private void ShowScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInRectangle(false);
        }

        private void HideScheduledShutdownAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void ColorAnimationInRectangle(bool isTest)
        {
            try
            {
                string name;
                if (isTest)
                {
                    recTestMode.Fill = redBrush;
                    name = "redBrush";
                }
                else
                {
                    recTestMode.Fill = yellowBrush;
                    name = "yellowBrush";
                }

                DoubleAnimation opacityAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = TimeSpan.FromSeconds(0.8),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };
                Storyboard.SetTargetName(opacityAnimation, name);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
                Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
                mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

                mouseLeftButtonDownStoryboard.Begin(this);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }

}
