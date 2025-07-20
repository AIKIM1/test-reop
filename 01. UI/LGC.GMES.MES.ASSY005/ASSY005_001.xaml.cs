/*************************************************************************************
 Created Date : 2020.10.16
      Creator : �ű���
   Decription : ���� ������ô (CNB2 ��) ����
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.12  ������ : �û�����ü���/���� ��� �߰�
 2021.09.01  ������ : Process.STACKING_FOLDING �� ��� ����׷� �޺� �߰�
 2021.10.25  ������ : �����ι��⼳�� �߰�(�����ڵ� ������ ���� ��/�� �˾� �����Ͽ� ���)
 2021.10.27  ������ : ���� ���� ���� �˾� �߰�
 2022.12.27  �ű��� : GM NND ������ ��� �������� ���� UI ǥ�� ��� �߰�
 2023.02.24  ��뱺 : ESHM ����-AZS E-Cuttor ���� �� AZS Stacking ���� ����  
 2023.03.20  ������ : ������ ���� ��ȣ �߰�
 2023.06.29  �ֵ��� : NND Unwinder ������/���� ���� ����
 2023.08.24  �念ö : GM2 ���ܷ��� �߰� (G673 ���� �߰�)
 2023.11.21  ������ : E20231115-000860 ��� ���� �ҷ�/LOSS/��ǰû�� ���� ��ư ���� ó��
 2024.01.02  �念ö : �û��� ����/���� ������ LOT Validation ������ �ݿ���
 2024.01.23  ������ : Package ���� �� Ư�� ���� ���� �÷� SPCL_MNGT_FLAG �̿�.
 2024.01.31  �ڼ��� : E20240124-000934 ����������ô ���� ������ ��ȸ �ؼ� ���� ���� ���� �� �ؼ� �޺��ڽ� �� �����Ҷ����� �ش��ϴ� �� ��ȸ�ϵ��� ���� 
 2024.02.20  ������ : E20240130-000700 ��Ī ������ ��� ����/NND ���� NG TAG�� �� �߰�
 2024.02.21  ��뱺 : E20240221-000898 ESMI1��(A4) 6Line�������� ȭ�麰 ����ID �޺������� ��ȸ�� Line������ ���ܵ� Line���� ó��
 2024.06.26  ��뱺 : E20240626-000751 ESMI1�� ����(A4) �� ��� ���� �����ڵ� üũ�Ͽ� MES SFU Configurtion���� ���õ� ���������� �ƴ� '����������ô-6Line' ȭ�鿡 ���� �޺��ڽ��� ���õ� ���������� ��ȸ�ǰ� ����
 2024.09.04  �赿�� : E20240806-000371 Lamination ������ ������ �޽��� ���� �߰�
 2025.05.20  õ���� : ESHG ���� ����������ô DNC�����߰� 
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
using LGC.GMES.MES.ASSY005.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.ASSY005
{
    public partial class ASSY005_001 : IWorkArea
    {
        #region Declaration


        public UcAssemblyCommand UcAssemblyCommand { get; set; }
        public UcAssemblyEquipment UcAssemblyEquipment { get; set; }
        public UcAssemblyProductLot UcAssemblyProductLot { get; set; }
        public UcAssemblyProductionResult UcAssemblyProductionResult { get; set; }

        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private readonly Util _util = new Util();

        private C1DataGrid DgEquipment { get; set; }
        public C1DataGrid DgProductLot { get; set; }

        private string _equipmentSegmentCode;
        private string _equipmentSegmentName;
        private string _processCode;
        private string _processName;
        private string _equipmentCode;
        private string _equipmentName;
        private string _treeEquipmentCode;
        private string _treeEquipmentName;
        private string _productLot;
        private string _equipmentMountPositionCode;
        private string sAttr2;

        private string _ldrLotIdentBasCode;
        private string _unldrLotIdentBasCode;
        private string _labelPassHoldFlag = string.Empty;
        private bool _isPopupConfirmJobEnd = true;  //������ �ߺ����� Ÿ�� ���� �߻��� �����ϱ� ����
        private bool _isAutoSelectTime = false;
        private List<string> _selectUserModeAreaList = new List<string>(new string[] { "A7", "A9" });   // �۾���,�۾��� ��� ȭ�� ���� ��û �� [C20200511-000024]
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherRackRateTimer = new System.Windows.Threading.DispatcherTimer();

        DataRowView _dvProductLot;
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);

        private bool _isRackRateMode = false;

        //SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);


        private enum SetEquipmentType
        {
            SearchButton,
            EquipmentComboChange,
            EquipmentTreeClick,
            ProductLotClick
        }

        private enum SetRackState
        {
            Normal,
            Warning,
            Danger
        }

        private SetRackState _rackState;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public ASSY005_001()
        {
            InitializeComponent();
        }

        private void Initialize()
        {
            SetComboBox();
            InitializeUserControls();
            SetEventInUserControls();
            SetProcessInUserControls();

            // Setting ������ ������ ����, ����, ���� ������ ȭ�� ���¿� ���� �ʱ� ���� 
            GetButtonPermissionGroup();

            // NND ������ô�� ��츸 �ؼ��� ������.
            SetPolarityByProcess();

            if (_dispatcherMainTimer != null)
            {
                int second = 0;

                if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.GetString()))
                    second = int.Parse(cboAutoSearch.SelectedValue.ToString());

                _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
                _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            }

            if (_dispatcherRackRateTimer != null)
            {
                _dispatcherRackRateTimer.Tick -= DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Tick += DispatcherRackRateTimer_Tick;
                _dispatcherRackRateTimer.Interval = new TimeSpan(0, 0, 60);
                _dispatcherRackRateTimer.Start();
            }
        }

        private void SetComboBox()
        {
            // ����
            SetEquipmentSegmentCombo(cboEquipmentSegment);

            // ����
            SetProcessCombo(cboProcess);

            // ���� �׷�
            SetEquipmentGroup();

            // �ؼ�
            SetPolarityCombo(cboPolarity);

            // ����
            SetEquipmentCombo(cboEquipment);

            // �ڵ���ȸ
            SetAutoSearchCombo(cboAutoSearch);

            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;
            cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
            cboPolarity.SelectedValueChanged += cboPolarity_SelectedValueChanged;
            cboEqpGrp.SelectedValueChanged += cboEqpGrp_SelectedValueChanged;
            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            if ((string.Equals(cboProcess.SelectedValue?.GetString(), Process.NOTCHING) || string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) && (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673")))   //GM2�߰� 2023-08-24  �念ö
            {
                SelectRackRate();
            }
        }

        private void InitializeUserControls()
        {
            UcAssemblyCommand = grdCommand.Children[0] as UcAssemblyCommand;
            UcAssemblyEquipment = grdEquipment.Children[0] as UcAssemblyEquipment;
            UcAssemblyProductLot = grdProduct.Children[0] as UcAssemblyProductLot;
            UcAssemblyProductionResult = grdProductionResult.Children[0] as UcAssemblyProductionResult;

            SetIdentInfo();

            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.FrameOperation = FrameOperation;
                UcAssemblyCommand.ProcessCode = _processCode;
                UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);
                UcAssemblyCommand.ApplyPermissions();
            }

            if (UcAssemblyEquipment != null)
            {
                UcAssemblyEquipment.UcParentControl = this;
                UcAssemblyEquipment.FrameOperation = FrameOperation;
                UcAssemblyEquipment.DgEquipment.MouseLeftButtonUp += DgEquipment_MouseLeftButtonUp;
                UcAssemblyEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
                UcAssemblyEquipment.ProcessCode = _processCode;
                UcAssemblyEquipment.EquipmentCode = _equipmentCode;
                UcAssemblyEquipment.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                DgEquipment = UcAssemblyEquipment.dgEquipment;
                SetEquipmentTree();
            }

            if (UcAssemblyProductLot != null)
            {
                UcAssemblyProductLot.UcParentControl = this;
                UcAssemblyProductLot.FrameOperation = FrameOperation;
                UcAssemblyProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
                UcAssemblyProductLot.ProcessCode = _processCode;
                UcAssemblyProductLot.EquipmentCode = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipmentName.Text = _equipmentName;
                UcAssemblyProductLot.SetControlVisibility();
                UcAssemblyProductLot.cboColor.SelectedIndex = 0;

                UcAssemblyProductLot.dgProductLot.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLotCommon.PreviewMouseDoubleClick += DgProductLot_PreviewMouseDoubleClick;
                UcAssemblyProductLot.dgProductLot.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;
                UcAssemblyProductLot.dgProductLotCommon.MouseLeftButtonUp += DgProductLot_MouseLeftButtonUp;

                DgProductLot = _processCode == Process.NOTCHING ? UcAssemblyProductLot.dgProductLot : UcAssemblyProductLot.dgProductLotCommon;

                UcAssemblyProductLot.LdrLotIdentBasCode = _ldrLotIdentBasCode;
                UcAssemblyProductLot.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
                SetProductLotList();

                // ������ : 20230404 ���ι�ȣ �߰�
                GetSlittingLaneNo();
            }

            if (UcAssemblyProductionResult != null)
            {
                UcAssemblyProductionResult.FrameOperation = FrameOperation;
                UcAssemblyProductionResult.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING ������ ���� �׷� �߰�
                UcAssemblyProductionResult.ApplyPermissions();
            }
        }

        private void InitializeButtonPermissionGroup()
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnRunCancel.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnRunCompleteCancel.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC�����ż�
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.btnDefectSave.Visibility = Visibility.Collapsed; // 2023.11.21 E20231115-000860 ������ �߰�
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.STACKING_FOLDING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.PACKAGING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutPackagingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdSpclTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }

                // ��뱺 ESHM AZS_ECUTTER, AZS_STACKING ���� �߰��� ���� ����
                else if (_processCode == Process.AZS_ECUTTER)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }
                else if (_processCode == Process.AZS_STACKING)
                {
                    UcAssemblyCommand.btnRunStart.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Collapsed;
                    UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Collapsed;
                    UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Collapsed;
                }

                UcAssemblyEquipment.IsInputUseAuthority = false;
                UcAssemblyEquipment.IsWaitUseAuthority = false;
                UcAssemblyEquipment.IsInputHistoryAuthority = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEventInUserControls()
        {
            if (UcAssemblyCommand != null)
            {
                UcAssemblyCommand.btnWaitLot.Click += ButtonWaitLot_Click;                       //���LOT��ȸ
                UcAssemblyCommand.btnRemarkHistory.Click += ButtonRemarkHistory_Click;           //Ư�̻����̷�(������)
                UcAssemblyCommand.btnEqptCondSearch.Click += ButtonEqptCondSearch_Click;         //�۾�������ȸ
                UcAssemblyCommand.btnEqptCond.Click += ButtonEqptCond_Click;                     //�۾����ǵ��
                UcAssemblyCommand.btnEqptCondMain.Click += ButtonEqptCond_Click;                 //�۾����ǵ��
                UcAssemblyCommand.btnWipNote.Click += ButtonWipNote_Click;                       //Ư�̻��װ���
                UcAssemblyCommand.btnReworkMove.Click += ButtonReworkMove_Click;                 //���۾����LOT�̵�
                UcAssemblyCommand.btnEqptIssue.Click += ButtonEqptIssue_Click;                   //�μ��ΰ��Ʈ
                UcAssemblyCommand.btnWorkHalfSlitSide.Click += ButtonWorkHalfSlitSide_Click;     //������ ���⼳��
                UcAssemblyCommand.btnEmSectionRollDirctn.Click += ButtonEmSectionRollDirctn_Click; //������⺯��

                UcAssemblyCommand.btnRunStart.Click += ButtonRunStart_Click;                     //�۾�����
                UcAssemblyCommand.btnRunCancel.Click += ButtonRunCancel_Click;                   //�������
                UcAssemblyCommand.btnRunComplete.Click += ButtonRunComplete_Click;               //���Ϸ�
                UcAssemblyCommand.btnRunCompleteCancel.Click += ButtonRunCompleteCancel_Click;   //���Ϸ����
                UcAssemblyCommand.btnCancelConfirm.Click += ButtonCancelConfirm_Click;           //Ȯ�����
                UcAssemblyCommand.btnSpclProdMode.Click += ButtonSpclProdMode_Click;             //Ư����������/����
                UcAssemblyCommand.btnMerge.Click += ButtonMerge_Click;                           //LOT Merge
                UcAssemblyCommand.btnChgCarrier.Click += ButtonChgCarrier_Click;                 //Carrier ��ü
                UcAssemblyCommand.btnTrayInfo.Click += ButtonTrayInfo_Click;                     //Ȱ��ȭ Ʈ���� ��ȸ
                UcAssemblyCommand.btnPilotProdMode.Click += ButtonPilotProdMode_Click;           //�û��꼳��/����
                UcAssemblyCommand.btnPilotProdSPMode.Click += ButtonPilotProdSPMode_Click;       //�û�����ü���/����
                UcAssemblyCommand.btnRework.Click += ButtonRework_Click;                         //���۾����LOT�̵�

                UcAssemblyCommand.btnQualityInput.Click += ButtonQualityInput_Click;                 //ǰ�������Է�
                UcAssemblyCommand.btnProductLot.Click += ButtonProductLot_Click;                     //Product Lot
                UcAssemblyCommand.btnConfirm.Click += ButtonConfirm_Click;                           //����Ȯ��
                UcAssemblyCommand.btnPrint.Click += ButtonPrint_Click;                               //���ڵ����

                UcAssemblyCommand.btnReturnCondition.Click += ButtonbtnReturnCondition_Click;        // �����ݼ������˾�
                // UcAssemblyCommand.btnUpdateLaneNo.Click += ButtonBtnUpdateLaneNo_Click;              // ������ ���� ��ȣ ���� �˾�
            }
        }

        private void SetProcessInUserControls()
        {
        }

        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            //this.RegisterName("redBrush", redBrush);
            //this.RegisterName("yellowBrush", yellowBrush);
            //HideRackRateMode();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _equipmentSegmentCode = LoginInfo.CFG_EQSG_ID;
            _equipmentSegmentName = LoginInfo.CFG_EQSG_NAME;
            _processCode = LoginInfo.CFG_PROC_ID;
            _processName = LoginInfo.CFG_PROC_NAME;
            _equipmentCode = LoginInfo.CFG_EQPT_ID;
            _equipmentName = LoginInfo.CFG_EQPT_NAME;

            ApplyPermissions();
            Initialize();

            //ESMI 1�� ����(A4) �� ��� ���� �����ڵ� üũ�Ͽ� MES SFU Configurtion���� ���õ� ���������� �ƴ� '����������ô-6Line' ȭ�鿡 ���� �޺��ڽ��� ���õ� ���������� ��ȸ�ǰ� ����
            if (IsEsmiLineCheck())
            {
                cboEquipmentSegment_SelectedValueChanged(null, null);
            }

            this.RegisterName("redBrush", redBrush);
            this.RegisterName("yellowBrush", yellowBrush);

            Loaded -= UserControl_Loaded;
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //UcPolymerFormProductionResult.DispatcherTimer?.Stop();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlEquipmentSegment();
            SetIdentInfo();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // ���� 
            SetProcessCombo(cboProcess);
            // Clear
            SetControlClear();

            HideAllRackRateMode();
        }

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && string.IsNullOrEmpty(cboProcess.SelectedValue?.GetString())) return;

            if (cboProcess?.SelectedValue == null || string.IsNullOrEmpty(cboProcess.SelectedValue.GetString()))
            {
                SetControlClearByProcess();
                return;
            }

            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            SetUserControlProcess();
            SetIdentInfo();

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;

            // Process.STACKING_FOLDING �����϶� ���� �׷켳��
            SetEquipmentGroup();

            // ���� 
            SetEquipmentCombo(cboEquipment);

            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

            // ProductLot ������ ��Ʈ�� Visibility �Ӽ� ����
            UcAssemblyProductLot.SetControlVisibility();

            // ���� ���濡 ���� ��ư ���� ��ȸ
            GetButtonPermissionGroup();

            // NND ������ô�� ��� �ؼ��� �������� ����.
            SetPolarityByProcess();

            // Clear
            SetControlClear();

            HideAllRackRateMode();

            if ((string.Equals(cboProcess.SelectedValue?.GetString(), Process.NOTCHING) || string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) && (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673")))   //GM2�߰� 2023-08-24  �念ö
            {
                SelectRackRate();
            }

            // ������ : 20230404 ���ι�ȣ �߰�
            GetSlittingLaneNo();

            // 2023.11.21 E20231115-000860 ������ : ��� ���� �ҷ�/LOSS/��ǰû�� ���� ��ư ����
            if (!string.Equals(cboProcess.SelectedValue?.GetString(), Process.LAMINATION)) UcAssemblyProductionResult.btnDefectSave.Visibility = Visibility.Visible;
        }

        private void cboPolarity_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            // �Ʒ� SelectRackRate ȣ�� �� �ִϸ��̼� �ʱ�ȭ
            // ShowRackRateMode �Լ� �� gridrowIndex ���������� ���Ͽ� idx < 4 ����
            for (int idx = 0; idx < 4; idx++)
                UcAssemblyProductLot.ProductLotContents.RowDefinitions[idx].BeginAnimation(RowDefinition.HeightProperty, null);

            // �ؼ� �޺��ڽ� �� �����Ҷ����� ��ȸ�Ǵ� �� ���� load
            SelectRackRate();

            // ���� cboPolarity
            SetEquipmentCombo(cboEquipment);

            // Clear
            SetControlClear();

            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        private void cboEqpGrp_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            cboEquipment.SelectionChanged -= cboEquipment_SelectionChanged;

            // ����
            SetEquipmentCombo(cboEquipment);

            // Clear
            SetControlClear();

            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);

            cboEquipment.SelectionChanged += cboEquipment_SelectionChanged;
        }

        private void cboEquipment_SelectionChanged(object sender, EventArgs e)
        {
            SetControlClear();
            SetEquipment(SetEquipmentType.EquipmentComboChange);
            ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
        }

        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals(""))
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // �ڵ���ȸ�� ������� �ʵ��� ���� �Ǿ����ϴ�.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // �ڵ���ȸ  %1�ʷ� ���� �Ǿ����ϴ�.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.IsEnabled = false;
            UcAssemblyProductLot.IsSearchResult = false;

            SetIdentInfo();
            SetEquipment(SetEquipmentType.SearchButton);

            Task<bool> task = WaitCallback();
            task.ContinueWith(_ =>
            {
                Thread.Sleep(500);
                btnSearch.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void DgEquipment_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            if (DgEquipment == null || DgEquipment.CurrentCell == null) return;
            if (grdProduct.Visibility == Visibility.Collapsed) return;

            string equipment = DgEquipment.CurrentCell.Row.DataItem.ToString();

            if (string.IsNullOrWhiteSpace(equipment) || equipment.Split(':').Length < 2)
            {
                _treeEquipmentCode = string.Empty;
                _treeEquipmentName = string.Empty;
            }
            else
            {
                _treeEquipmentCode = equipment.Split(':')[0].Trim();
                _treeEquipmentName = equipment.Split(':')[1].Trim();

                SetEquipment(SetEquipmentType.EquipmentTreeClick);

                DgProductLot = _processCode == Process.NOTCHING ? UcAssemblyProductLot.dgProductLot : UcAssemblyProductLot.dgProductLotCommon;

                // Product Lot ����
                DataTable dt = DataTableConverter.Convert(DgProductLot.ItemsSource);

                if (dt == null || dt.Rows.Count == 0) return;

                DataRow[] drSelect = dt.Select("EQPTID = '" + _treeEquipmentCode + "'");

                //�ش� ���� ���� Lot ������ ���� ���
                if (drSelect.Length == 0)
                {
                    if (_processCode == Process.NOTCHING)
                    {
                        DgProductLot.Refresh();
                        return;
                    }
                    else
                    {
                        // Sort ó��
                        dt = (from t in dt.AsEnumerable()
                              orderby t.Field<string>("EQPTID") ascending, t.Field<string>("WIPSTAT") descending, t.Field<string>("PR_LOTID") descending
                              select t).CopyToDataTable();

                        dt.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                        dt.AcceptChanges();

                        UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = false;
                        Util.GridSetData(DgProductLot, dt, null, true);
                        UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = true;
                        return;
                    }
                }

                var queryNoneEquipment = dt.AsEnumerable().Where(o => string.IsNullOrEmpty(o.Field<string>("EQPTID"))).ToList();
                DataTable dtNoneEquipment = queryNoneEquipment.Any() ? queryNoneEquipment.CopyToDataTable() : dt.Clone();

                var querySelect = dt.AsEnumerable().Where(o => o.Field<string>("EQPTID") == _treeEquipmentCode).ToList();
                DataTable dtSelect = querySelect.Any() ? querySelect.OrderBy(y => y.Field<string>("EQPTID")).ThenByDescending(z => z.Field<string>("WIPSTAT")).CopyToDataTable() : dt.Clone();

                var queryNoneSelect = dt.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("EQPTID")) && x.Field<string>("EQPTID") != _treeEquipmentCode).ToList();
                DataTable dtNoneSelect = queryNoneSelect.Any() ? queryNoneSelect.OrderBy(y => y.Field<string>("EQPTID")).ThenByDescending(z => z.Field<string>("WIPSTAT")).CopyToDataTable() : dt.Clone();

                DataTable dtSort = dt.Clone();
                dtSort.Merge(dtSelect);
                dtSort.Merge(dtNoneSelect);
                dtSort.Merge(dtNoneEquipment);
                dtSort.Select("CHK = 1").ToList().ForEach(r => r["CHK"] = 0);
                dtSort.AcceptChanges();

                /*
                DataRow[] drNoneSelectEquipmentNull = dt.Select(" ISNULL(EQPTID,'') = '' ");
                DataRow[] drNoneSelect = dt.Select(" EQPTID <> '" + _treeEquipmentCode + "' AND ISNULL(EQPTID,'') <> '' ");

                DataTable dtSort = dt.Clone();

                for (int row = 0; row < drSelect.Length; row++)
                {
                    dtSort.ImportRow(drSelect[row]);
                }

                for (int row = 0; row < drNoneSelect.Length ; row++)
                {
                    dtSort.ImportRow(drNoneSelect[row]);
                }

                for (int row = 0; row < drNoneSelectEquipmentNull.Length; row++)
                {
                    dtSort.ImportRow(drNoneSelectEquipmentNull[row]);
                }

                // Equipment TreeClick �� üũ�� ���� ��ư ��Ȱ��ȭ ó��
                dtSort.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
                dtSort.AcceptChanges();
                */

                UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = false;
                Util.GridSetData(DgProductLot, dtSort, null, true);
                UcAssemblyProductLot.IsProductLotChoiceRadioButtonEnable = true;
            }
        }

        private void DgProductLot_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dg != null && (dg.CurrentCell == null || dg.CurrentCell.Row == null)) return;

            // +, - ���ÿ� ���� ������ư üũ�� ������ �۵��� �ϴ� ��찡 �߻��Ͽ� TOGGLEKEY �÷� ���� ���ýÿ� ���� ��ư üũ ���� ó�� ��.
            if (dg.CurrentCell == null || dg.CurrentCell.Column.Name == "TOGGLEKEY") return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            RadioButton rb = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as RadioButton;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            if (!string.Equals(_processCode, Process.NOTCHING) && string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);

            if (dg.CurrentCell.Column.Name == "LOTID" && !string.IsNullOrEmpty(drv["LOTID"].GetString()))
            {
                //this.Cursor = Cursors.Wait;
                dg.CurrentCell.Presenter.Cursor = Cursors.Wait;

                HideAllRackRateMode();
                UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductionResult : UcAssemblyCommand.ButtonVisibilityType.CommonProductionResult);

                GetButtonPermissionGroup();

                grdProduct.Visibility = Visibility.Collapsed;
                grdProductionResult.Visibility = Visibility.Visible;

                SetUserControlProductionResult();
                UcAssemblyProductionResult.SelectProductionResult();

                dg.CurrentCell.Presenter.Cursor = Cursors.Hand;
            }
        }

        private void DgProductLot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (dg?.CurrentCell == null || dg.CurrentCell.Row == null) return;
            if (dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns["CHK"].Index).Presenter == null) return;

            // +, - ���ÿ� ���� ������ư üũ�� ������ �۵��� �ϴ� ��찡 �߻��Ͽ� TOGGLEKEY �÷� ���� ���ýÿ� ���� ��ư üũ ���� ó�� ��.
            if (dg.CurrentCell.Column.Name == "TOGGLEKEY") return;

            int rowIdx = dg.CurrentCell.Row.Index;
            DataRowView drv = dg.Rows[rowIdx].DataItem as DataRowView;
            if (drv == null) return;

            if (!string.Equals(_processCode, Process.NOTCHING) && string.IsNullOrEmpty(drv["LOTID"].GetString())) return;

            if (dg.Name == "dgProductLot")
                DgProductLot = UcAssemblyProductLot.dgProductLot;
            else if (dg.Name == "dgProductLotCommon")
                DgProductLot = UcAssemblyProductLot.dgProductLotCommon;

            RadioButton rb = DgProductLot.GetCell(DgProductLot.CurrentCell.Row.Index, DgProductLot.Columns["CHK"].Index).Presenter.Content as RadioButton;

            if (rb?.DataContext == null) return;
            SetProductLotSelect(rb);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;

            // ���õ� LOT�� �۾� ����Ͻðڽ��ϱ�?
            Util.MessageConfirm("SFU3151", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelProcess();
                }
            });
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UcAssemblyCommand.btnConfirm.IsEnabled = false;

                // WorkCalander üũ.
                GetWorkCalander();

                if (!ValidationConfirm()) return;
                if (!CheckEquipmentConfirmType("CONFIRM_W")) return;

                if (_processCode == Process.NOTCHING)
                {
                    // �۾�����/ǰ������ �Է� ���θ� Ȯ���Ͽ� ���Է� �� ��� ȭ�� ȣ�� ó��
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // �۾����� ��� ����
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCondSearch_Click(null, null);
                        return;
                    }

                    // ǰ������ ��� ����
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }

                    // ���� Loss ��� ���� üũ
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);
                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string info = string.Empty;
                        string lossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            info = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            lossInfo = lossInfo + "\n" + info;
                        }

                        object[] param = new object[] { lossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);

                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
                else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)        // 20250428 ESHG DNC�����ż�
                {
                    #region �ҷ� ���� üũ
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    #region �۾�����/ǰ������ �Է� ����
                    //string _ValueToLotID = Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgProductLot, "CHK")].DataItem, "LOTID"));
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // �۾����� ��� ����
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCond_Click(null, null);
                        return;
                    }
                    // ǰ������ ��� ����
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }
                    #endregion

                    // ���� Loss ��� ���� üũ
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string info = string.Empty;
                        string lossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            info = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            lossInfo = lossInfo + "\n" + info;
                        }

                        object[] param = new object[] { lossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);
                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
                else if (_processCode == Process.STACKING_FOLDING)
                {
                    #region �ҷ� ���� üũ
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // ���� Loss ��� ���� üũ
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string sInfo = string.Empty;
                        string sLossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            sLossInfo = sLossInfo + "\n" + sInfo;
                        }

                        object[] param = new object[] { sLossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);
                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
                else if (_processCode == Process.PACKAGING)
                {
                    #region �ҷ� ���� üũ
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    #region �۾�����/ǰ������ �Է� ����
                    string valueToLotId = _dvProductLot["LOTID"].GetString();

                    // �۾����� ��� ����
                    if (Util.EQPTCondition(_processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonEqptCond_Click(null, null);
                        return;
                    }
                    // ǰ������ ��� ����
                    if (Util.EDCCondition(LoginInfo.CFG_AREA_ID, _processCode, _equipmentSegmentCode, _equipmentCode, valueToLotId))
                    {
                        ButtonQualityInput_Click(null, null);
                        return;
                    }

                    #endregion

                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string sInfo = string.Empty;
                        string sLossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            sLossInfo = sLossInfo + "\n" + sInfo;
                        }

                        object[] param = new object[] { sLossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);
                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
                // ��뱺 ESHM AZS_ECUTTER, AZS_STACKING ���� �߰��� ���� ����
                else if (_processCode == Process.AZS_ECUTTER)
                {
                    #region �ҷ� ���� üũ
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // ���� Loss ��� ���� üũ
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string sInfo = string.Empty;
                        string sLossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            sLossInfo = sLossInfo + "\n" + sInfo;
                        }

                        object[] param = new object[] { sLossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);
                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
                else if (_processCode == Process.AZS_STACKING)
                {
                    #region �ҷ� ���� üũ
                    foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                    {
                        double dRsn, dOrgRsn = 0;

                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                        double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                        if (dRsn != dOrgRsn)
                        {
                            // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                            Util.MessageValidation("SFU1878", (action) =>
                            {
                                if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                    UcAssemblyProductionResult.tabDefect.IsSelected = true;
                            });
                            return;
                        }
                    }
                    #endregion

                    // ���� Loss ��� ���� üũ
                    DataTable dtEqpLossInfo = Util.Get_EqpLossInfo(_equipmentCode, _processCode);

                    if (dtEqpLossInfo != null && dtEqpLossInfo.Rows.Count > 0 && dtEqpLossInfo.Columns.Contains("NOINPUT_CNT") && Util.NVC_Int(dtEqpLossInfo.Rows[0]["NOINPUT_CNT"]) > 0)
                    {
                        string sInfo = string.Empty;
                        string sLossInfo = string.Empty;

                        for (int iCnt = 0; iCnt < dtEqpLossInfo.Rows.Count; iCnt++)
                        {
                            sInfo = dtEqpLossInfo.Rows[iCnt]["JOBDATE"].ToString() + " : " + dtEqpLossInfo.Rows[iCnt]["NOINPUT_CNT"].ToString();
                            sLossInfo = sLossInfo + "\n" + sInfo;
                        }

                        object[] param = new object[] { sLossInfo };

                        // ���Է��� ���� Loss�� �����մϴ�. Ȯ���Ͻðڽ��ϱ�? %1
                        Util.MessageConfirm("SFU3501", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Confirm_Process();
                            }
                        }, param);
                    }
                    else
                    {
                        Confirm_Process();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                UcAssemblyCommand.btnConfirm.IsEnabled = true;
            }
        }

        private void ButtonWaitLot_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidationWaitLot()) return;

            PopupWaitLot();
        }

        private void ButtonRemarkHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRemarkHistory()) return;

            PopupRemarkHistory();
        }

        private void ButtonEqptCondSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptCondSearch()) return;

            PopupEqptCondSearch();
        }

        private void ButtonEqptCond_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationEqptCond()) return;

            PopupEqptCond();
        }

        private void ButtonWipNote_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWipNote()) return;

            PopupWipNote();
        }

        private void ButtonReworkMove_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonEqptIssue_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return;
            }

            int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");


            DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

            CMM_COM_EQPCOMMENT popupEquipmentComment = new CMM_COM_EQPCOMMENT();
            popupEquipmentComment.FrameOperation = FrameOperation;

            object[] parameters = new object[10];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = idx < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID"));//_dvProductLot["LOTID"].GetString();
            parameters[4] = idx < 0 ? "" : Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSEQ"));//_dvProductLot["WIPSEQ"].GetString();
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            parameters[6] = Util.NVC(drShift[0]["VAL001"]);     //txtShift.Text; // �۾�����
            parameters[7] = Util.NVC(drShift[0]["SHFT_ID"]);    //txtShift.Tag; // �۾����ڵ�
            parameters[8] = Util.NVC(drShift[0]["VAL002"]);     //txtWorker.Text; // �۾��ڸ�
            parameters[9] = Util.NVC(drShift[0]["WRK_USERID"]); //txtWorker.Tag; // �۾��� ID

            C1WindowExtension.SetParameters(popupEquipmentComment, parameters);
            //popupEquipmentComment.Closed += new EventHandler(popupEquipmentComment_Closed);
            Dispatcher.BeginInvoke(new Action(() => popupEquipmentComment.ShowModal()));
        }

        /// <summary>
        /// ������ ���⼳��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonWorkHalfSlitSide_Click(object sender, RoutedEventArgs e)
        {
            PopupWorkHalfSlitSide();
        }

        private void PopupWorkHalfSlitSide_Closed(object sender, EventArgs e)
        {
            // ������/���� ���� 2���� ��� ����ϴ� AREA �� ���
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popup = sender as CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN;
            
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // ������ ��ȸ
                    GetWorkHalfSlittingSide();
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popup = sender as CMM_ELEC_WORK_HALF_SLITTING;
                if (popup != null && popup.DialogResult == MessageBoxResult.OK)
                {
                    // ������ ��ȸ
                    GetWorkHalfSlittingSide();
                    UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                }
            }
        }

        /// <summary>
        /// ������⺯��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonEmSectionRollDirctn_Click(object sender, RoutedEventArgs e)
        {
            PopupEmSectionRollDirctn();
        }

        private void PopupEmSectionRollDirctn_Closed(object sender, EventArgs e)
        {
            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popup = sender as CMM_ELEC_EM_SECTION_ROLL_DIRCTN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
            }
        }

        private void ButtonRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunStart()) return;

            PopupRunStart();
        }

        //������� ��ư�� NND ������ô�� ��쿡�� ���� ��.
        private void ButtonRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRunCancel()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelRun();
                }
            });

        }

        private void ButtonRunComplete_Click(object sendeer, RoutedEventArgs e)
        {
            if (!ValidationRunComplete()) return;
            if (!CheckEquipmentConfirmType("EQPT_END_W")) return;

            PopupEquipmentEnd();
        }

        private void ButtonRunCompleteCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCompleteCancelRun()) return;

            Util.MessageConfirm("SFU1243", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunCompleteCancel();
                }
            });
        }

        private void ButtonCancelConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancelConfirm()) return;

            PopupCancelConfirm();
        }

        private void ButtonSpclProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSpclProdMode()) return;

            bool isMode = GetSpclProdMode();

            string messageCode;

            if (isMode)
                messageCode = "SFU8309";    //[%1]�� Ư������ �����Ͻðڽ��ϱ�?
            else
                messageCode = "SFU8308";    //[%1]�� Ư������ �����Ͻðڽ��ϱ�?

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetSpclProdMode(!isMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, UcAssemblyProductLot.txtSelectEquipmentName.Text);

        }

        private void ButtonMerge_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLogMarge()) return;

            PopupLotMerge();
        }

        private void ButtonChgCarrier_Click(object sender, RoutedEventArgs e)
        {
            PopupChangeCarrier();
        }

        private void ButtonTrayInfo_Click(object sender, RoutedEventArgs e)
        {
            PopupTrayInfo();
        }

        private void ButtonPilotProdMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            //bool isMode = GetPilotProdMode();
            string sFromMode = GetPilotProdMode();
            string messageCode;
            string sToMode = string.Empty;

            if (sFromMode.Equals("PILOT"))
            {
                messageCode = "SFU8304";    //[%1]�� �û��� �����Ͻðڽ��ϱ�?
                sToMode = string.Empty;
            }
            else
            {
                messageCode = "SFU8303";    //[%1]�� �û��� �����Ͻðڽ��ϱ�?
                sToMode = "PILOT";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetPilotProdMode(sToMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, UcAssemblyProductLot.txtSelectEquipmentName.Text);
        }

        private void ButtonPilotProdSPMode_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPilotProdMode()) return;

            //bool isMode = GetPilotProdMode();
            string sFromMode = GetPilotProdMode();
            string messageCode;
            string sToMode = string.Empty;

            if (sFromMode.Equals("PILOT_S"))
            {
                messageCode = "SFU8188";    //[%1]�� �û������ �����Ͻðڽ��ϱ�?
                sToMode = string.Empty;
            }
            else
            {
                messageCode = "SFU8189";    //[%1]�� �û������ �����Ͻðڽ��ϱ�?
                sToMode = "PILOT_S";
            }

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (SetPilotProdMode(sToMode) == false)
                    {
                        return;
                    }
                    Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
                }
            }, UcAssemblyProductLot.txtSelectEquipmentName.Text);
        }

        private void ButtonRework_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationRework()) return;

            PopupReWork();
        }

        private void ButtonQualityInput_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationQualityInput()) return;

            PopupQualityInput();
        }

        private void ButtonProductLot_Click(object sender, RoutedEventArgs e)
        {
            if (UcAssemblyProductionResult.IsProductLotRefreshFlag)
            {
                if (_dvProductLot != null)
                {
                    UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString());
                }
                else
                {
                    UcAssemblyProductLot.SelectProductList();
                }

            }


            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

            // ���� ���濡 ���� ��ư ���� ��ȸ
            GetButtonPermissionGroup();

            grdProduct.Visibility = Visibility.Visible;
            grdProductionResult.Visibility = Visibility.Collapsed;

            //if (UcAssemblyProductionResult.IsProductLotRefreshFlag)
            //    UcAssemblyProductLot.SelectProductList(_dvProductLot["LOTID"].GetString());

            UcAssemblyProductionResult.IsProductLotRefreshFlag = false;
            UcAssemblyProductionResult.InitializeControls();
        }

        private void ButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;
            PopupPrint();
        }

        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0���̸� skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    // ���� Tree ��ȸ
                    SetUserControlEquipment();

                    if (cboEquipment.SelectedItems.Any())
                    {
                        UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                    }

                    // Product Lot ���� ��ȸ
                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        SetProductLotList();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        private void DispatcherRackRateTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0���̸� skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;

                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   //GM2�߰� 2023-08-24  �念ö
                        {
                            HideRackRateMode();
                            SelectRackRate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));

        }

        // <summary>
        /// �����ݼ����Ǽ��� �˾�
        /// </summary>
        protected void ButtonbtnReturnCondition_Click(object sender, EventArgs e)
        {

            try
            {
                CMM_WORKORDER_DRB WO = new CMM_WORKORDER_DRB();

                WO._EqptSegment = _equipmentSegmentCode;
                WO._ProcID = _processCode;
                WO._EqptID = _equipmentCode;

                WO.GetWorkOrdeCallBack(() =>
                {
                    if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
                    {
                        Util.MessageValidation("SFU1673");     // ���� ���� �ϼ���.
                        return;
                    }
                    DataTable dt = WO.WOTable;

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        // %1�� �����ϴ�.
                        Util.MessageValidation("SFU1444", ObjectDic.Instance.GetObjectName("Work Order"));
                        return;
                    }

                    CMM_CONDITION_RETURN popCondition = new CMM_CONDITION_RETURN() { FrameOperation = FrameOperation }; ;
                    if (popCondition != null)
                    {
                        UcAssemblyCommand.btnExtra.IsDropDownOpen = false;
                        object[] Parameters = new object[7];
                        Parameters[0] = _equipmentCode.ToString(); //�����ڵ�
                        Parameters[1] = UcAssemblyProductLot.txtSelectEquipmentName.Text; // �����ڵ��
                        Parameters[2] = dt.Rows[0]["PRJT_NAME"].ToString(); //PJT
                        Parameters[3] = dt.Rows[0]["PROD_VER_CODE"].ToString(); //Version
                        Parameters[4] = _processCode; //����
                        Parameters[5] = "A"; //���� ȭ�鿡�� ȣ��
                        Parameters[6] = dt.Rows[0]["WOID"].ToString();
                        C1WindowExtension.SetParameters(popCondition, Parameters);

                        popCondition.Closed += new EventHandler(popCondition_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => popCondition.ShowModal()));
                    }

                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        //// <summary>
        ///// ������ ���� ��ȣ ���� �˾�
        ///// </summary>
        //protected void ButtonBtnUpdateLaneNo_Click(object sender, EventArgs e)
        //{
        //    if (!ValidationUpdateLaneNo()) return;

        //    PopupUpdateLaneNo();
        //}

        private void popCondition_Closed(object sender, EventArgs e)
        {
            CMM_CONDITION_RETURN popup = sender as CMM_CONDITION_RETURN;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                // ������ : 20230404 ���ι�ȣ �߰�
                GetSlittingLaneNo();

                // ������ ��ȸ
                GetWorkHalfSlittingSide();
                UcAssemblyEquipment.ChangeEquipment(_processCode, cboEquipment.SelectedItemsToString);
                //string processLotId = SelectProcessLotId();

                ////SetProductLotList();
                //// ���� Lot ����ȸ
                //SelectProductLot(processLotId);
                //SetProductLotList(processLotId);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private static void SetEquipmentSegmentCombo(C1ComboBox cbo)
        {
            //ESMI-A4�� 1~5 Line ����ó��
            //const string bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
            string bizRuleName = string.Empty;

            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_FIRST_PRIORITY_LINE_CBO";
                }
                else
                {
                    bizRuleName = "DA_BAS_SEL_EQUIPMENTSEGMENT_CBO";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, LoginInfo.CFG_EQSG_ID);
        }

        private void SetProcessCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_PROCESS_CBO";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["LANGID"] = LoginInfo.LANGID;
            dtRow["EQSGID"] = cboEquipmentSegment.SelectedValue ?? null;
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            //A5000 : Notching,  A7000	A7000 : Lamination, A8000	A8000 : Stacking & Folding, A9000	A9000 : Packaging
            // string[] assemblyProcess = new string[] { "A5000", "A7000", "A7400", "A8400", "A8000", "A9000" };
            string[] assemblyProcess = new string[] { "A5000", "A5700", "A7000", "A7400", "A8400", "A8000", "A9000" };  // 20250428 ESHG DNC�����ż�

            if (CommonVerify.HasTableRow(dtResult))
            {
                dtResult.AsEnumerable().Where(r => !assemblyProcess.Contains(r.Field<string>("CBO_CODE"))).ToList().ForEach(row => row.Delete());
                dtResult.AcceptChanges();
            }

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (!string.IsNullOrEmpty(LoginInfo.CFG_PROC_ID))
            {
                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;

                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            else
            {
                cbo.SelectedIndex = 0;
            }
            //cboProcess.SelectedValueChanged += cboProcess_SelectedValueChanged;
        }

        private void SetPolarityCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "ELEC_TYPE" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NA, selectedValueText, displayMemberText, string.Empty);
        }

        private void SetEquipmentCombo(MultiSelectionBox mcb)
        {
            try
            {
                // ��뱺 E-Cuttor ����׷� ���ý� ���� ����
                if (_processCode == Process.AZS_ECUTTER)
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQSGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                    dr["PROCID"] = cboProcess.SelectedValue?.ToString();

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ECUTTER_CBO", "RQSTDT", "RSLTDT", inTable);

                    if (dtResult.Rows.Count != 0)
                    {
                        mcb.isAllUsed = false;
                        if (dtResult.Rows.Count == 1)
                        {
                            mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                        }
                        else
                        {
                            mcb.isAllUsed = true;
                            mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                            for (int row = 0; row < dtResult.Rows.Count; row++)
                            {
                                if (dtResult.Rows[row]["CBO_CODE"].ToString() == LoginInfo.CFG_EQPT_ID)
                                {
                                    mcb.Check(row);
                                }
                            }
                        }
                    }
                    else
                    {
                        mcb.ItemsSource = null;
                    }
                }
                else
                {
                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.TableName = "RQSTDT";
                    inTable.Columns.Add("LANGID", typeof(string));
                    inTable.Columns.Add("EQSGID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                    inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                    inTable.Columns.Add("EQGRID", typeof(string));

                    DataRow dr = inTable.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue?.ToString();
                    //dr["PROCID"] = cboProcess.SelectedValue == null ? null : cboProcess.SelectedValue.ToString();
                    dr["PROCID"] = cboProcess.SelectedValue?.ToString();

                    if (_processCode == Process.NOTCHING)
                    {
                        dr["ELTR_TYPE_CODE"] = string.IsNullOrWhiteSpace(cboPolarity.SelectedValue?.ToString()) ? null : cboPolarity.SelectedValue.ToString();
                    }

                    if (_processCode == Process.STACKING_FOLDING && cboEqpGrp.GetBindValue() != null)
                    {
                        dr["EQGRID"] = cboEqpGrp.GetBindValue();
                    }

                    inTable.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_BY_ELECTYPE_CBO", "RQSTDT", "RSLTDT", inTable);

                    if (dtResult.Rows.Count != 0)
                    {
                        mcb.isAllUsed = false;
                        if (dtResult.Rows.Count == 1)
                        {
                            mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                            mcb.Check(-1);
                        }
                        else
                        {
                            mcb.isAllUsed = true;
                            mcb.ItemsSource = DataTableConverter.Convert(dtResult);
                            for (int row = 0; row < dtResult.Rows.Count; row++)
                            {
                                if (dtResult.Rows[row]["CBO_CODE"].ToString() == LoginInfo.CFG_EQPT_ID)
                                {
                                    mcb.Check(row);
                                }
                            }
                        }
                    }
                    else
                    {
                        mcb.ItemsSource = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEquipmentGroup()
        {
            try
            {
                if (cboProcess.SelectedValue?.GetString() == Process.STACKING_FOLDING)
                {
                    cboEqpGrp.SelectedValueChanged -= cboEqpGrp_SelectedValueChanged;

                    string[] arrColumn = { "LANGID", "AREAID", "EQSGID", "PROCID" };
                    string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID,
                    cboEquipmentSegment.GetStringValue(), cboProcess.GetStringValue() };
                    cboEqpGrp.SetDataComboItem("DA_BAS_SEL_EQUIPMENTGROUP_BY_PROCID_CBO", arrColumn, arrCondition);

                    cboEqpGrp.SelectedValueChanged += cboEqpGrp_SelectedValueChanged;

                    if (cboEqpGrp.Items.Count > 1)
                    {
                        tbEqpGrp.Visibility = Visibility.Visible;
                        cboEqpGrp.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        tbEqpGrp.Visibility = Visibility.Collapsed;
                        cboEqpGrp.Visibility = Visibility.Collapsed;
                    }

                    tbEqpGrp.Visibility = Visibility.Visible;
                    cboEqpGrp.Visibility = Visibility.Visible;
                }
                else
                {
                    cboEqpGrp.ItemsSource = null;

                    tbEqpGrp.Visibility = Visibility.Collapsed;
                    cboEqpGrp.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetIdentInfo()
        {
            const string bizRuleName = "DA_EQP_SEL_LOT_IDENT_BAS_CODE";
            try
            {
                _ldrLotIdentBasCode = string.Empty;
                _unldrLotIdentBasCode = string.Empty;

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _processCode;
                newRow["EQSGID"] = _equipmentSegmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    _ldrLotIdentBasCode = dtRslt.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                    _unldrLotIdentBasCode = dtRslt.Rows[0]["UNLDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CheckSelectWorkOrderInfo()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SET_WORKORDER_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (string.IsNullOrEmpty(dtRslt.Rows[0]["WO_DETL_ID"].GetString()))
                    {
                        //���õ� W/O�� �����ϴ�.
                        Util.MessageValidation("SFU1635");
                        return false;
                    }

                    if (dtRslt.Rows[0]["WO_STAT_CHK"].GetString() == "N")
                    {
                        //���õ� W/O�� �����ϴ�.
                        Util.MessageValidation("SFU1635");
                        return false;
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetButtonPermissionGroup()
        {
            try
            {
                InitializeButtonPermissionGroup();

                if (!Util.pageAuthCheck(FrameOperation.AUTHORITY))
                    return;

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID; //"CNSSOBAKTOP"; //
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = _processCode;
                dtRow["EQGRID"] = _processCode == Process.STACKING_FOLDING ? "STK" : null;  // STACKING, FOLDING ������ô�� ��� ������ PROCID�� ���������, EQGRID �� �б�ó���� �ʿ� ��.
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtResult.Rows)
                        {
                            if (drTmp == null) continue;

                            if (_processCode == Process.NOTCHING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ���� TODO : CNB2�� ����������ô�� ��� ����ȭ��� �����Ͽ� ����, ���, �����̷¿� ���� ������ �ʿ� ��.
                                        //SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        //grdOutTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunCancel.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_EQPT_END_W": // ���Ϸ� ��� ����
                                        UcAssemblyCommand.btnRunCompleteCancel.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }

                            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC�����ż�
                            {
                                UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;   // 20250428 ESHG DNC�����ż�
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ����
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"])); //TODO : AS-IS �󿡴� ���� ��Ʈ���� ���� �ο��� �Ͽ����� ����� �׸��忡 ��������� ���� ���� �۾��� �ʿ� ��.
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // Ȯ����� ��� ����
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // ���� ������
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.STACKING_FOLDING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ����
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // Ȯ����� ��� ����
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // ���� ������
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.PACKAGING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ����
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        UcAssemblyProductionResult.grdOutPackagingTranBtn.Visibility = Visibility.Visible;
                                        UcAssemblyProductionResult.grdSpclTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // Ȯ����� ��� ����
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            // ��뱺 AZS_ECUTTER, AZS_STACKING ���� ����
                            else if (_processCode == Process.AZS_ECUTTER)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ����
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"])); //TODO : AS-IS �󿡴� ���� ��Ʈ���� ���� �ο��� �Ͽ����� ����� �׸��忡 ��������� ���� ���� �۾��� �ʿ� ��.
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        UcAssemblyProductionResult.grdOutLaminationTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // Ȯ����� ��� ����
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // ���� ������
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutLaminationTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
                            }
                            else if (_processCode == Process.AZS_STACKING)
                            {
                                switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                                {
                                    case "INPUT_W": // ���� ��� ����
                                    case "WAIT_W": // ��� ��� ����
                                    case "INPUTHIST_W": // �����̷� ��� ����
                                        SetPermissionPerInputButton(Util.NVC(drTmp["BTN_PMS_GRP_CODE"]));
                                        break;
                                    case "OUTPUT_W": // �������ǰ ��� ����
                                        UcAssemblyProductionResult.grdOutStackingTranBtn.Visibility = Visibility.Visible;
                                        break;
                                    case "LOTSTART_W": // �۾����� ��� ����
                                        UcAssemblyCommand.btnRunStart.Visibility = Visibility.Visible;
                                        break;
                                    case "CANCEL_CONFIRM_W":    // Ȯ����� ��� ����
                                        UcAssemblyCommand.btnCancelConfirm.Visibility = Visibility.Visible;
                                        break;
                                    case "OUTPUT_W_C1":         // ���� ������
                                        UcAssemblyProductionResult.IsMagzinePrintVisible = true;
                                        UcAssemblyProductionResult.grdOutStackingTranPrint.Visibility = Visibility.Visible;
                                        break;
                                    case "PILOT_W":    // �û��� ���� ���� ����
                                        UcAssemblyCommand.btnPilotProdSPMode.Visibility = Visibility.Visible;
                                        break;
                                }
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

        private void SetPermissionPerInputButton(string sBtnPermissionGrpCode)
        {
            if (UcAssemblyEquipment == null)
                return;

            UcAssemblyEquipment.SetPermissionPerButton(sBtnPermissionGrpCode);
        }

        private string GetMaxChildGRPSeq(string parentLot)
        {
            if (string.IsNullOrEmpty(parentLot)) return string.Empty;

            try
            {
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PR_LOTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["PR_LOTID"] = parentLot;
                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MAX_CHILDGRSEQ", "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(searchResult))
                {
                    return Util.NVC(searchResult.Rows[0]["CHILD_GR_SEQNO"]);
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private void GetWorkCalander()
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["PROCID"] = _processCode; //Process.NOTCHING;
                inTable.Rows.Add(newRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORK_CALENDAR_WRKR_INFO", "RQSTDT", "RSLTDT", inTable);

                if (result.Rows.Count > 0)
                {
                    //DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                    dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                    {
                        r["SHFT_ID"] = result.Rows[0]["SHFT_ID"];
                        r["VAL001"] = result.Rows[0]["SHFT_NAME"];
                        r["WRK_USERID"] = result.Rows[0]["WRK_USERID"];
                        r["VAL002"] = result.Rows[0]["WRK_USERNAME"];
                        r["WRK_STRT_DTTM"] = result.Rows[0]["WRK_STRT_DTTM"];
                        r["WRK_END_DTTM"] = result.Rows[0]["WRK_END_DTTM"];
                    });
                    dt.AcceptChanges();

                }
                else
                {
                    DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                    dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                    {
                        r["SHFT_ID"] = string.Empty;
                        r["VAL001"] = string.Empty;
                        r["WRK_USERID"] = string.Empty;
                        r["VAL002"] = string.Empty;
                        r["WRK_STRT_DTTM"] = string.Empty;
                        r["WRK_END_DTTM"] = string.Empty;
                    });
                    dt.AcceptChanges();

                    #region �۾���,�۾��� ��� ȭ�� ���� ��û �� [C20200511-000024]
                    if (_selectUserModeAreaList.Contains(LoginInfo.CFG_AREA_ID))
                    {
                        GetEquipmentWorkInfo();
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                //HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SelectEquipmentMountPosition()
        {
            _equipmentMountPositionCode = string.Empty;

            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_CURR_MOUNT_MTRL_CBO_L";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("MOUNT_MTRL_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["MOUNT_MTRL_TYPE_CODE"] = "PROD";
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    _equipmentMountPositionCode = dtResult.Rows[0]["CBO_CODE"].GetString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 2021.08.12 : �û�����ü���/���� ��� �߰�
        /// param bool���� string���� ����
        /// </summary>
        /// <param name="sMode"></param>
        /// <returns></returns>
        private bool SetPilotProdMode(string sMode)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PILOT_PRDC_MODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["PILOT_PRDC_MODE"] = sMode; //isMode ? "PILOT" : "";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_PILOT_PRODUCTION_MODE", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097"); // ����Ǿ����ϴ�.
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void CancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("OUT_CSTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));

                ///////////////////////////////////////////////////////////// Process.SLITTING || Process.SRS_SLITTING ���
                DataTable InPrLot = inDataSet.Tables.Add("IN_PRLOT");
                InPrLot.Columns.Add("PR_LOTID", typeof(string));
                InPrLot.Columns.Add("CUT_ID", typeof(string));
                InPrLot.Columns.Add("CSTID", typeof(string));

                ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // Lot ���� �׸��� ��ŭ Loop ??

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].ToString();
                if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
                {
                    if (_dvProductLot["CSTID"] != null)
                    {
                        //CSTID�� NULL�� ��쿡�� ������ �߻��Ͽ� Util.NVC�� �����.
                        newRow["CSTID"] = Util.NVC(_dvProductLot["CSTID"]);
                    }
                }
                if (_unldrLotIdentBasCode == "CST_ID" || _unldrLotIdentBasCode == "RF_ID")
                {
                    if (_dvProductLot["OUT_CSTID"] != null)
                    {
                        newRow["OUT_CSTID"] = Util.NVC(_dvProductLot["OUT_CSTID"]);
                    }
                }
                newRow["USERID"] = LoginInfo.USERID;
                newRow["INPUT_LOTID"] = Util.NVC(_dvProductLot["LOTID_PR"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_MX", "INDATA", null, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    // ���� ó�� �Ǿ����ϴ�.
                    Util.MessageInfo("SFU1275");

                    // ���� Lot ����ȸ
                    SelectProductLot(string.Empty);
                    SetProductLotList();

                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void CancelRun()
        {
            try
            {
                ShowLoadingIndicator();

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("IN_EQP");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inInput = indataSet.Tables.Add("IN_INPUT");
                inInput.Columns.Add("INPUT_LOTID", typeof(string));

                DataTable inOutput = indataSet.Tables.Add("IN_OUTPUT");
                inOutput.Columns.Add("OUTPUT_LOTID", typeof(string));
                inOutput.Columns.Add("OUTPUT_CSTID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataTable.Rows.Add(newRow);

                newRow = inInput.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_LOTID"));
                inInput.Rows.Add(newRow);

                newRow = inOutput.NewRow();
                newRow["OUTPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "LOTID"));
                newRow["OUTPUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "CSTID"));
                inOutput.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_START_LOT_NT_CSTID", "IN_EQP,IN_INPUT,IN_OUTPUT", null, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                        Util.MessageInfo("SFU1275");	//���� ó�� �Ǿ����ϴ�.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void RunCompleteCancel()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_CANCEL_EQPT_END_LOT_NT_L";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["INPUT_LOTID"] = _dvProductLot["PR_LOTID"].GetString();
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

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
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

        private void Confirm_Process()
        {

            if (CheckModelChange() && !CheckInputEqptCond())
            {
                //�ش� LOT�� �۾������� ��ϵ��� �ʾҽ��ϴ�.\n����Ȯ�� �Ͻðڽ��ϱ�?
                Util.MessageConfirm("SFU2817", (result2) =>
                {
                    if (result2 == MessageBoxResult.OK)
                    {
                        //ConfirmProcess();
                        CheckNgTag();   // 2024.02.20 ������ E20240130-000700 ����/��Ī(NND) ���� NG TAG ���� �� �˾�
                    }
                });
            }
            else
            {
                //ConfirmProcess();
                CheckNgTag();   // 2024.02.20 ������ E20240130-000700 ����/��Ī(NND) ���� NG TAG ���� �� �˾�
            }
        }

        /// <summary>
        /// 2021.08.12 : �û�����ü���/���� ��� �߰�
        /// return bool���� string���� ����
        /// </summary>
        /// <returns></returns>
        private string GetPilotProdMode()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_UI_TESTMODE_INFO", "INDATA", "OUTDATA", inTable);

                //if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("PILOT_PROD_MODE"))
                if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("EQPT_OPER_MODE"))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"])))
                    {
                        //ShowPilotProdMode();
                        return Util.NVC(dtRslt.Rows[0]["EQPT_OPER_MODE"]);
                    }
                    else
                    {
                        //HidePilotProdMode();
                        return string.Empty;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private bool GetSpclProdMode()
        {
            try
            {
                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                    return false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dr);
                //2024.01.23  ������: Package ���� �� Ư�� ���� ���� �÷� SPCL_MNGT_FLAG �̿�.
                if (_processCode == Process.PACKAGING)
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR_SPCL_MNGT_FLAG", "INDATA", "OUTDATA", inTable);
                    if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("SPCL_MNGT_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["SPCL_MNGT_FLAG"]).Equals("Y"))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EIOATTR", "INDATA", "OUTDATA", inTable);

                    if (CommonVerify.HasTableRow(dtRslt) && dtRslt.Columns.Contains("SPCL_LOT_GNRT_FLAG"))
                    {
                        if (Util.NVC(dtRslt.Rows[0]["SPCL_LOT_GNRT_FLAG"]).Equals("Y"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SetSpclProdMode(bool bMode)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SPCL_LOT_GNRT_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                newRow["SPCL_LOT_GNRT_FLAG"] = bMode ? "Y" : "N";
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_EIOATTR_SPCL_LOT_INPUT", "INDATA", null, inTable);

                Util.MessageInfo("PSS9097");    // ����Ǿ����ϴ�.
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetEquipmentWorkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["PROCID"] = _processCode;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = result.Rows[0]["SHFT_ID"];
                                r["VAL001"] = result.Rows[0]["SHFT_NAME"];
                                r["WRK_USERID"] = result.Rows[0]["WRK_USERID"];
                                r["VAL002"] = result.Rows[0]["WRK_USERNAME"];
                                r["WRK_STRT_DTTM"] = result.Rows[0]["WRK_STRT_DTTM"];
                                r["WRK_END_DTTM"] = result.Rows[0]["WRK_END_DTTM"];
                            });
                            dt.AcceptChanges();
                        }
                        else
                        {
                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = string.Empty;
                                r["VAL001"] = string.Empty;
                                r["WRK_USERID"] = string.Empty;
                                r["VAL002"] = string.Empty;
                                r["WRK_STRT_DTTM"] = string.Empty;
                                r["WRK_END_DTTM"] = string.Empty;
                            });
                            dt.AcceptChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AutoPrint(bool bQASample)
        {
            try
            {
                string equipmentCellPrintFlag = string.Empty;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_PRINT_YN();
                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["WIPSEQ"] = string.IsNullOrEmpty(_dvProductLot["WIPSEQ"].GetString()) ? 1 : _dvProductLot["WIPSEQ"].GetInt();
                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_PRT_CHK", "INDATA", "OUTDATA", inTable);

                //  ���� ���� �б� ó��
                DataTable inTable2 = new DataTable();
                inTable2.Columns.Add("EQPTID", typeof(string));

                DataRow newRow2 = inTable2.NewRow();
                newRow2["EQPTID"] = _equipmentCode;
                inTable2.Rows.Add(newRow2);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR_CELL_ID_PRT_FLAG", "INDATA", "OUTDATA", inTable2);

                if (CommonVerify.HasTableRow(dtResult))
                    equipmentCellPrintFlag = dtResult.Rows[0]["CELL_ID_PRT_FLAG"].ToString();

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (!Util.NVC(dtRslt.Rows[0]["PROC_LABEL_PRT_FLAG"]).Equals("Y"))
                    {
                        // ������ ���� ��ȸ
                        string sPrt = string.Empty;
                        string sRes = string.Empty;
                        string sCopy = string.Empty;
                        string sXpos = string.Empty;
                        string sYpos = string.Empty;
                        string sDark = string.Empty;
                        string sLBCD = string.Empty;    // ���� �� Ÿ�� �ڵ�
                        string sEqpt = string.Empty;
                        DataRow drPrtInfo = null;

                        // 2017-07-04 Lee. D. R
                        // Line�� �� ���� ���� ���
                        if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU2003"); //����Ʈ ȯ�� �������� �����ϴ�.
                            return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                        {
                            if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                                return;
                        }
                        else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                        {
                            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                            {
                                if (Util.NVC(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT]).Equals(_equipmentCode))
                                {
                                    sPrt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE].ToString();
                                    sRes = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                                    sCopy = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_COPIES].ToString();
                                    sXpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_X].ToString();
                                    sYpos = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_Y].ToString();
                                    sDark = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DARKNESS].ToString();
                                    sEqpt = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_EQUIPMENT].ToString();
                                    drPrtInfo = dr;
                                }
                            }

                            if (sEqpt.Equals(""))
                            {
                                Util.MessageValidation("SFU3615"); //������ ȯ�漳���� ���� ������ Ȯ���ϼ���.
                                return;
                            }
                        }

                        if (bQASample)
                            sCopy = Convert.ToString(Convert.ToInt32(sCopy) + 1);

                        string zplCode = GetPrintInfo(_dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString(), sPrt, sRes, sCopy, sXpos, sYpos, sDark, out sLBCD);
                        string sLot = _dvProductLot["LOTID"].GetString();


                        if (zplCode.Equals(""))
                        {
                            Util.MessageValidation("SFU1498");
                            return;
                        }

                        if (equipmentCellPrintFlag.Equals("Y"))
                        {
                            Util.SendZplBarcode(sLot, zplCode);
                        }
                        else
                        {
                            if (zplCode.StartsWith("0,"))  // ZPL ���� �ڵ� Ȯ��.
                            {
                                if (PrintLabel(zplCode.Substring(2), drPrtInfo))
                                    SetLabelPrtHist(zplCode.Substring(2), drPrtInfo, _dvProductLot["LOTID"].GetString(), _dvProductLot["WIPSEQ"].GetString(), sLBCD);
                            }
                            else
                            {
                                Util.MessageValidation(zplCode.Substring(2));
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

        private string GetPrintInfo(string sLot, string sWipSeq, string sPrt, string sRes, string sCopy, string sXpos, string sYpos, string sDark, out string sOutLBCD)
        {
            sOutLBCD = "";
            try
            {

                DataTable inTable = _bizDataSet.GetBR_PRD_GET_PROCESS_LOT_LABEL_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.NOTCHING;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = sLot;
                newRow["WIPSEQ"] = sWipSeq;
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness            
                newRow["LBCD"] = LoginInfo.CFG_LABEL_TYPE; // LABEL CODE
                newRow["NT_WAIT_YN"] = "N"; // ��� ������ ����� ����.
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PROCESS_LOT_LABEL_NT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("MMDLBCD"))
                        sOutLBCD = Util.NVC(dtResult.Rows[0]["MMDLBCD"]);

                    return Util.NVC(dtResult.Rows[0]["LABELCD"]);
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }

        private void SetLabelPrtHist(string zplCode, DataRow drPrtInfo, string sLot, string wipseq, string labelCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                newRow["LABEL_CODE"] = labelCode;
                newRow["LABEL_ZPL_CNTT"] = zplCode;
                newRow["LABEL_PRT_COUNT"] = Util.NVC(drPrtInfo["COPIES"]).Equals("") ? "0" : Util.NVC(drPrtInfo["COPIES"]);
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = wipseq;
                newRow["PRT_ITEM03"] = "NOTCHED PANCAKE";
                //newRow["PRT_ITEM04"] = "";
                //newRow["PRT_ITEM05"] = "";
                //newRow["PRT_ITEM06"] = "";
                //newRow["PRT_ITEM07"] = "";
                //newRow["PRT_ITEM08"] = "";
                //newRow["PRT_ITEM09"] = "";
                //newRow["PRT_ITEM10"] = "";
                //newRow["PRT_ITEM11"] = "";
                //newRow["PRT_ITEM12"] = "";
                //newRow["PRT_ITEM13"] = "";
                //newRow["PRT_ITEM14"] = "";
                //newRow["PRT_ITEM15"] = "";
                //newRow["PRT_ITEM16"] = "";
                //newRow["PRT_ITEM17"] = "";
                //newRow["PRT_ITEM18"] = "";
                //newRow["PRT_ITEM19"] = "";
                //newRow["PRT_ITEM20"] = "";
                //newRow["PRT_ITEM21"] = "";
                //newRow["PRT_ITEM22"] = "";
                //newRow["PRT_ITEM23"] = "";
                //newRow["PRT_ITEM24"] = "";
                //newRow["PRT_ITEM25"] = "";
                //newRow["PRT_ITEM26"] = "";
                //newRow["PRT_ITEM27"] = "";
                //newRow["PRT_ITEM28"] = "";
                //newRow["PRT_ITEM29"] = "";
                //newRow["PRT_ITEM30"] = "";
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["LOTID"] = sLot;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private bool CheckModelChange()
        {
            bool bRet = true;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQSGID"] = _equipmentSegmentCode;
                newRow["EQPTID"] = _equipmentCode;
                newRow["PROCID"] = _processCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LAST_EQP_PROD_INFO", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("PRJT_NAME"))
                    {
                        string productProjectName = _dvProductLot["PRJT_NAME"].GetString();
                        string searchProjectName = Util.NVC(dtResult.Rows[0]["PRJT_NAME"]);
                        if (searchProjectName.Length > 1 && searchProjectName.Equals(productProjectName))
                            bRet = false;
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

        private bool CheckInputEqptCond()
        {
            try
            {
                bool bRet = false;

                DataTable inTable = _bizDataSet.GetDA_PRD_SEL_EQPT_CLCT_CNT();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_CNT", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("CNT"))
                    {
                        string sCnt = Util.NVC(dtResult.Rows[0]["CNT"]);
                        int iCnt = 0;
                        int.TryParse(sCnt, out iCnt);

                        if (iCnt > 0)
                        {
                            bRet = true;
                        }
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

        private bool CheckProdQtyChangePermission()
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = _processCode;
                dtRow["EQGRID"] = _processCode == Process.STACKING_FOLDING ? "STK" : null;  // STACKING, FOLDING ������ô�� ��� ������ PROCID�� ���������, EQGRID �� �б�ó���� �ʿ� ��.
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (dtRslt.Columns.Contains("BTN_PMS_GRP_CODE"))
                    {
                        foreach (DataRow drTmp in dtRslt.Rows)
                        {
                            if (drTmp == null) continue;

                            switch (Util.NVC(drTmp["BTN_PMS_GRP_CODE"]))
                            {
                                case "PROD_QTY_CHG_W": // ���� ���� ����
                                    bRet = true;
                                    break;
                            }
                        }
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

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }

        // 2024.02.20 ������ E20240130-000700 ����/��Ī(NND) ���� NG TAG ���� �� �˾�
        private void CheckNgTag()
        {
            if (string.Equals(_processCode, Process.NOTCHING))
            {
                const string bizRuleName = "BR_PRD_CHK_NG_TAG_CNT";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("OUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["OUT_LOTID"] = _dvProductLot["LOTID"].GetString();
                dr["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                object[] parameters = new object[2];
                parameters[0] = dtRslt.Rows[0]["NND_DFCT_QTY"];
                parameters[1] = dtRslt.Rows[0]["ELEC_DFCT_QTY"];

                if (dtRslt.Rows[0]["RSLT_FLAG"].Equals("Y"))
                {
                    Util.MessageValidation("SFU3676", (action) =>
                    {
                        ConfirmProcess();
                    }, parameters);
                }
                else
                {
                    ConfirmProcess();
                }
            }
            else
            {
                ConfirmProcess();
            }
        }

        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            /*
            if (UcElectrodeCommand != null)
            {
                List<Button> listAuth = new List<Button>
                {
                };

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            }
            */
        }

        private void SetPolarityByProcess()
        {
            if (cboProcess.SelectedValue?.GetString() == Process.NOTCHING)
            {
                tbPolarity.Visibility = Visibility.Visible;
                cboPolarity.Visibility = Visibility.Visible;
                UcAssemblyProductLot.chkWait.Visibility = Visibility.Visible;
                UcAssemblyProductLot.chkWait.IsChecked = false;

            }
            else
            {
                tbPolarity.Visibility = Visibility.Collapsed;
                cboPolarity.Visibility = Visibility.Collapsed;
                UcAssemblyProductLot.chkWait.Visibility = Visibility.Collapsed;
            }

            if (UcAssemblyProductLot.cboColor.Items.Count > 0)
                UcAssemblyProductLot.cboColor.SelectedIndex = 0;
        }

        private void SetControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            _productLot = string.Empty;
            _equipmentMountPositionCode = string.Empty;
            _dvProductLot = null;

            Util.gridClear(UcAssemblyEquipment.DgEquipment);

            //Util.gridClear(UcAssemblyProductLot.DgProductLot);
            //Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            //Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);

            //UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            //UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            //UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
        }

        private void SetControlClearByProcess()
        {
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
            Util.gridClear(UcAssemblyProductLot.dgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgProductLotCommon);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);
            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
            UcAssemblyProductionResult.SetControlClear();

            // ������ : 20230404 ���ι�ȣ �߰�
            UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
        }

        private void SetEquipmentTreeControlClear()
        {
            _treeEquipmentCode = string.Empty;
            _treeEquipmentName = string.Empty;
            Util.gridClear(UcAssemblyEquipment.DgEquipment);
        }

        private void SetEquipmentProductLotControlClear()
        {
            Util.gridClear(UcAssemblyProductLot.DgProductLot);
            Util.gridClear(UcAssemblyProductLot.dgInputHistory);
            Util.gridClear(UcAssemblyProductLot.dgInputHistoryDetail);

            UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
            UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
            UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

            // ������ : 20230404 ���ι�ȣ �߰�
            UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
        }

        private void SetUserControlEquipmentSegment()
        {
            _equipmentSegmentCode = cboEquipmentSegment.SelectedValue.ToString();
            _equipmentSegmentName = cboEquipmentSegment.Text;

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SetUserControlProcess()
        {


            _processCode = cboProcess?.SelectedValue?.ToString();
            if (cboProcess != null) _processName = cboProcess.Text;

            UcAssemblyCommand.ProcessCode = string.IsNullOrEmpty(_processCode) ? LoginInfo.CFG_PROC_ID : _processCode;
            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductLot : UcAssemblyCommand.ButtonVisibilityType.CommonProductLot);

            // ���� ���濡 ���� ��ư ���� ��ȸ
            GetButtonPermissionGroup();

            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SetUserControlEquipment()
        {
            UcAssemblyEquipment.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyEquipment.ProcessCode = _processCode;
            UcAssemblyEquipment.ProcessName = _processName;
            UcAssemblyEquipment.EquipmentCode = cboEquipment.SelectedItemsToString;
            UcAssemblyEquipment.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING ������ ���� �׷� �߰�

            if (_dvProductLot != null)
            {
                UcAssemblyEquipment.ProductLotID = _dvProductLot["LOTID"].GetString();
                UcAssemblyEquipment.SelectEquipmentCode = _dvProductLot["EQPTID"].GetString();
            }
            else
            {
                UcAssemblyEquipment.ProductLotID = string.Empty;
                UcAssemblyEquipment.SelectEquipmentCode = string.Empty;
            }
        }

        private void SetUserControlProductLot()
        {
            UcAssemblyProductLot.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductLot.ProcessCode = _processCode;
            UcAssemblyProductLot.ProcessName = _processName;
            UcAssemblyProductLot.EquipmentCode = cboEquipment.SelectedItemsToString;
            // ���񿵿� ���ÿ� ���� UcAssemblyProductLot UserControl�� EquipmentCode ���� ���� ��.
            //UcAssemblyProductLot.EquipmentCode = _equipmentCode;
            UcAssemblyProductLot.LdrLotIdentBasCode = _ldrLotIdentBasCode;
            UcAssemblyProductLot.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
        }

        public void SetUserControlProductionResult()
        {
            if (string.IsNullOrWhiteSpace(_equipmentCode) || _equipmentCode.Split(',').Length > 1) return;

            DataTable dt = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "'").CopyToDataTable();

            UcAssemblyProductionResult.EquipmentSegmentCode = _equipmentSegmentCode;
            UcAssemblyProductionResult.EquipmentSegmentName = _equipmentSegmentName;
            UcAssemblyProductionResult.ProcessCode = _processCode;
            UcAssemblyProductionResult.ProcessName = _processName;
            UcAssemblyProductionResult.EquipmentCode = _equipmentCode;
            UcAssemblyProductionResult.EquipmentName = _equipmentName;
            UcAssemblyProductionResult.DtEquipment = dt;
            UcAssemblyProductionResult.DvProductLot = _dvProductLot;
            UcAssemblyProductionResult.LdrLotIdentBasCode = _ldrLotIdentBasCode;
            UcAssemblyProductionResult.UnldrLotIdentBasCode = _unldrLotIdentBasCode;
            UcAssemblyProductionResult.EquipmentGroupCode = cboEqpGrp.GetStringValue(); // STACKING, FOLDING ������ ���� �׷� �߰�
        }

        public void SetProductLotSelect(RadioButton rb)
        {
            int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;

            for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
            {
                DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
            }

            DgProductLot = UcAssemblyProductLot.DgProductLot;

            DgProductLot.SelectedIndex = idx;

            // ���� ��ư ���ÿ� ���� DataRowView ����
            _dvProductLot = DgProductLot.Rows[idx].DataItem as DataRowView;
            if (_dvProductLot == null) return;

            if (_processCode == Process.NOTCHING)
            {
                if (string.IsNullOrEmpty(_dvProductLot["EQPTID"].GetString()) && !string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                {
                    SelectEquipment(UcAssemblyProductLot.txtSelectEquipment.Text, _dvProductLot["EQPTNAME"].ToString());
                }
                else
                {
                    SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
                }
            }
            else
            {
                SelectEquipment(_dvProductLot["EQPTID"].ToString(), _dvProductLot["EQPTNAME"].ToString());
            }

            SelectProductLot(_dvProductLot["LOTID"].ToString());
        }

        private void SetEquipment(SetEquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case SetEquipmentType.SearchButton:

                    UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

                    if (string.IsNullOrEmpty(cboEquipment.SelectedItemsToString))
                    {
                        UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                        UcAssemblyProductLot.IsSearchResult = true;
                        break;
                    }

                    SelectEquipment(cboEquipment.SelectedItemsToString,
                        cboEquipment.SelectedItems.Count == 1 ? GetEquipmentName(cboEquipment) : null);

                    // ���� Tree ��ȸ
                    SetEquipmentTree();

                    // 1. grdProduct Ȱ��ȭ ��  
                    if (grdProductionResult.Visibility == Visibility.Visible)
                    {
                        btnSearch.IsEnabled = true;
                        UcAssemblyProductLot.IsSearchResult = true;

                        if (!IsCurrentProcessByLotId())
                        {
                            Util.MessageInfo("SFU7352", (result) =>
                                {
                                    // Product Lot ȭ�� ��ȯ �� ��ȸ
                                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                                    SetProductLotList();
                                }
                                , _dvProductLot["LOTID"].GetString());
                        }
                        else
                        {
                            // UcAssemblyProductionResult ��ȸ
                            UcAssemblyCommand.SetButtonVisibility(_processCode == Process.NOTCHING ? UcAssemblyCommand.ButtonVisibilityType.NotchingProductionResult : UcAssemblyCommand.ButtonVisibilityType.CommonProductionResult);

                            // ���� ���濡 ���� ��ư ���� ��ȸ
                            GetButtonPermissionGroup();

                            grdProduct.Visibility = Visibility.Collapsed;
                            grdProductionResult.Visibility = Visibility.Visible;
                            _equipmentCode = _dvProductLot["EQPTID"].GetString();
                            //SetUserControlProductionResult();
                            UcAssemblyProductionResult.SelectProductionResult();
                        }
                    }
                    else
                    {
                        // Product Lot ��ȸ
                        SetProductLotList();
                    }

                    break;

                case SetEquipmentType.EquipmentComboChange:

                    SetEquipmentProductLotControlClear();

                    if (string.IsNullOrEmpty(cboEquipment.SelectedItemsToString))
                    {
                        UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                        UcAssemblyProductLot.txtSelectLot.Text = string.Empty;
                        break;
                    }

                    //SelectEquipment(cboEquipment.SelectedItemsToString, null);
                    SelectEquipment(cboEquipment.SelectedItemsToString,
                        cboEquipment.SelectedItems.Count == 1 ? GetEquipmentName(cboEquipment) : null);

                    // ���� Tree ��ȸ
                    SetEquipmentTree();

                    // Product Lot ��ȸ
                    SetProductLotList();
                    break;

                case SetEquipmentType.EquipmentTreeClick:

                    DgEquipment.SelectedBackground = new SolidColorBrush(Colors.Transparent);
                    _equipmentCode = _treeEquipmentCode;
                    _equipmentName = _treeEquipmentName;
                    SelectEquipment(_equipmentCode, _equipmentName);

                    if (grdProduct.Visibility == Visibility.Visible)
                    {
                        SelectProductLot(string.Empty);
                    }
                    break;

                case SetEquipmentType.ProductLotClick:

                    break;
            }
        }

        private void SetEquipmentTree()
        {
            // Clear
            //SetControlClear();
            SetEquipmentTreeControlClear();

            if (cboEquipment.SelectedItems.Any())
            {
                UcAssemblyEquipment.ChangeEquipment(_processCode, _equipmentCode);
            }
        }

        private void SetProductLotList(string processLotId = null)
        {
            //if (grdProduct.Visibility == Visibility.Collapsed) return;

            UcAssemblyProductLot.SetControlVisibility();

            if (cboEquipment.SelectedItems.Any())
            {
                SetUserControlProductLot();
                UcAssemblyProductLot.SelectProductList(processLotId);
            }
        }

        public void SelectProductLotList()
        {
            if (grdProduct.Visibility == Visibility.Collapsed)
                UcAssemblyProductionResult.IsProductLotRefreshFlag = true;
            else
            {
                SetProductLotList();
            }
        }

        private void SelectEquipment(string equipmentCode, string equipmentName)
        {
            _equipmentCode = equipmentCode;
            _equipmentName = equipmentName;

            if (equipmentCode.Split(',').Length == 1)
            {
                UcAssemblyProductLot.txtSelectEquipment.Text = _equipmentCode;
                UcAssemblyProductLot.txtSelectEquipmentName.Text = _equipmentName;

                if (string.IsNullOrEmpty(_equipmentName))
                {
                    equipmentName = DgEquipment?.CurrentCell?.Row?.DataItem?.GetString();

                    if (!string.IsNullOrEmpty(equipmentName))
                    {
                        equipmentName = equipmentName.Replace("System.Data.DataRowView", string.Empty);

                        if (equipmentName.Split(':').Length == 2)
                        {
                            equipmentName = equipmentName.Split(':')[1].Trim();
                            UcAssemblyProductLot.txtSelectEquipmentName.Text = equipmentName;
                        }
                    }
                }

                // ������ ���� ��ȸ(���� ���� : MNG_SLITTING_SIDE_AREA)
                if (UcAssemblyCommand.IsManageSlittingSide)
                {
                    GetWorkHalfSlittingSide();
                }

                SelectEquipmentMountPosition();

                // ������ : 20230404 ���ι�ȣ �߰�
                GetSlittingLaneNo();
            }
            else
            {
                if (grdProduct.Visibility == Visibility.Visible)
                {
                    UcAssemblyProductLot.txtSelectEquipment.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectEquipmentName.Text = string.Empty;
                    UcAssemblyProductLot.txtSelectLot.Text = string.Empty;

                    // ������ : 20230404 ���ι�ȣ �߰�
                    UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;
                }
            }
            SetUserControlEquipment();
            SetUserControlProductLot();
        }

        private void SelectProductLot(string productLotId)
        {
            if (string.IsNullOrEmpty(productLotId))
            {
                if (_processCode == Process.NOTCHING)
                {
                    if (_util.GetDataGridFirstRowIndexByCheck(DgProductLot, "CHK") < 0)
                        _dvProductLot = null;
                }
                else
                {
                    if (string.IsNullOrEmpty(productLotId)) _dvProductLot = null;
                }
            }

            _productLot = productLotId;
            UcAssemblyProductLot.txtSelectLot.Text = _productLot;
        }

        private string SelectProcessLotId()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                newRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_EQPT_PROC_LOT_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    return dtResult.Rows[0]["LOTID"].ToString();
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return string.Empty;
            }
        }

        private static string GetEquipmentName(MultiSelectionBox msb)
        {
            DataTable dt = ((DataView)msb.ItemsSource).Table;

            var query = (from t in dt.AsEnumerable()
                         where t.Field<string>("CBO_CODE") == msb.SelectedItemsToString
                         select new
                         {
                             EquipmentName = t.Field<string>("CBO_NAME")
                         }).FirstOrDefault();

            return query?.EquipmentName;
        }

        private string GetFormIdByProcess(string processCode)
        {
            string returnFormId = string.Empty;

            switch (processCode)
            {
                case "A5000":
                    returnFormId = "ASSY004_001";
                    break;
                case "A7000":
                    returnFormId = "ASSY004_004";
                    break;
                case "A8000":
                    returnFormId = "ASSY004_006";
                    break;
                case "A9000":
                    returnFormId = "ASSY004_007";
                    break;
                default:
                    returnFormId = "ASSY004_001";
                    break;
            }

            return returnFormId;
        }

        private void ConfirmProcess()
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    ASSY005_001_CONFIRM popupConfirm = new ASSY005_001_CONFIRM();
                    popupConfirm.FrameOperation = FrameOperation;

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    object[] parameters = new object[14];
                    parameters[0] = Process.NOTCHING;
                    parameters[1] = _equipmentSegmentCode;
                    parameters[2] = _equipmentCode;
                    parameters[3] = _dvProductLot["LOTID"].GetString();
                    parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    parameters[5] = _ldrLotIdentBasCode;
                    parameters[6] = _unldrLotIdentBasCode;
                    parameters[7] = _equipmentMountPositionCode;

                    parameters[8] = Util.NVC(drShift[0]["VAL001"]);         //Util.NVC(txtShift.Text);
                    parameters[9] = Util.NVC(drShift[0]["SHFT_ID"]);        //Util.NVC(txtShift.Tag);
                    parameters[10] = Util.NVC(drShift[0]["VAL002"]);        //Util.NVC(txtWorker.Text);
                    parameters[11] = Util.NVC(drShift[0]["WRK_USERID"]);    //Util.NVC(txtWorker.Tag);
                    parameters[12] = CheckProdQtyChangePermission();
                    parameters[13] = _dvProductLot["QA_INSP_TRGT_FLAG"].GetString();

                    C1WindowExtension.SetParameters(popupConfirm, parameters);

                    popupConfirm.Closed -= popupConfirm_Closed;
                    popupConfirm.Closed += popupConfirm_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupConfirm.ShowModal()));
                }
                else
                {

                    ASSY005_COM_CONFIRM popupConfirm = new ASSY005_COM_CONFIRM { FrameOperation = FrameOperation };

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    object[] parameters = new object[13];
                    parameters[0] = _processCode;
                    parameters[1] = _equipmentSegmentCode;
                    parameters[2] = _equipmentCode;
                    parameters[3] = _dvProductLot["LOTID"].GetString();
                    parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                    parameters[5] = _ldrLotIdentBasCode;
                    parameters[6] = _unldrLotIdentBasCode;
                    parameters[7] = Util.NVC(drShift[0]["VAL001"]);         //Util.NVC(txtShift.Text);
                    parameters[8] = Util.NVC(drShift[0]["SHFT_ID"]);        //Util.NVC(txtShift.Tag);
                    parameters[9] = Util.NVC(drShift[0]["VAL002"]);         //Util.NVC(txtWorker.Text);
                    parameters[10] = Util.NVC(drShift[0]["WRK_USERID"]);    //Util.NVC(txtWorker.Tag);
                    parameters[11] = CheckProdQtyChangePermission();
                    parameters[12] = "N";

                    C1WindowExtension.SetParameters(popupConfirm, parameters);
                    popupConfirm.Closed += popupConfirm_Closed;
                    Dispatcher.BeginInvoke(new Action(() => popupConfirm.ShowModal()));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool PrintLabel(string zplCode, DataRow drPrtInfo)
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
                    if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 1)
                    {
                        brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zplCode);
                    }
                    else if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count > 1)
                    {
                        brtndefault = FrameOperation.PrintUsbBarcodeEquipment(zplCode, _equipmentCode);
                    }

                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zplCode);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zplCode);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
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

        private void SelectRackRate()
        {
            if (string.Equals(_processCode, Process.NOTCHING))
            {
                //if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;

                const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_ELTR_TYPE_STO";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "NPW";   //��Ī�ϼ�â��
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        if (cboPolarity.SelectedIndex != 2)
                        {
                            //NND ��� : NND-LNS (+) STO #1
                            const string cathodeEquipmentCode = "U1ASTO13201";

                            var querycathode = (from t in bizResult.AsEnumerable()
                                                where t.Field<string>("EQPTID") == cathodeEquipmentCode
                                                select new
                                                {
                                                    EquipmentCode = t.Field<string>("EQPTID"),
                                                    RackRate = t.Field<long>("RACK_RATE"),
                                                    EquipmentName = t.Field<string>("EQPTNAME")
                                                }).FirstOrDefault();

                            if (querycathode != null)
                            {
                                _rackState = RackRateDifferenceValue(querycathode.EquipmentCode, querycathode.RackRate);
                                string rackRateText = RackRateDifferenceValueText(querycathode.EquipmentCode, querycathode.RackRate);

                                ShowRackRateMode(_rackState, querycathode.EquipmentName, rackRateText + "%", "C");
                            }
                        }
                        if (cboPolarity.SelectedIndex != 1)
                        {
                            //NND ���� : NND - LNS(-) STO #1
                            const string anodeEquipmentCode = "U1ASTO13101";

                            var queryAnode = (from t in bizResult.AsEnumerable()
                                              where t.Field<string>("EQPTID") == anodeEquipmentCode
                                              select new
                                              {
                                                  EquipmentCode = t.Field<string>("EQPTID"),
                                                  RackRate = t.Field<long>("RACK_RATE"),
                                                  EquipmentName = t.Field<string>("EQPTNAME"),
                                              }).FirstOrDefault();

                            // querycathode -> queryAnode ����
                            if (queryAnode != null)
                            {
                                _rackState = RackRateDifferenceValue(queryAnode.EquipmentCode, queryAnode.RackRate);
                                string rackRateText = RackRateDifferenceValueText(queryAnode.EquipmentCode, queryAnode.RackRate);

                                ShowRackRateMode(_rackState, queryAnode.EquipmentName, rackRateText + "%", "A");
                            }
                        }
                    }
                });
            }
            else if (string.Equals(_processCode, Process.DNC) || string.Equals(_processCode, Process.LAMINATION))       // 20250428 ESHG DNC�����ż� 
            {
                HideAllRackRateMode();

                const string bizRuleName = "BR_MHS_SEL_STO_INVENT_SUMMARY_PER_ELTR_TYPE_STO";
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = "LWW";   //��̿ϼ�â��
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        string sDangerMsgEqptName = string.Empty;
                        string sDangerMsgRackRate = string.Empty;
                        string sWarningMsgEqptName = string.Empty;
                        string sWarningMsgRackRate = string.Empty;

                        for (int idx = 0; idx < bizResult.Rows.Count; idx++)
                        {
                            if (bizResult.Columns.Contains("EQPTID") && bizResult.Columns.Contains("EQPTNAME") && bizResult.Columns.Contains("RACK_RATE"))
                            {
                                string sEqptCode = Util.NVC(bizResult.Rows[idx]["EQPTID"]);
                                string sEqptName = Util.NVC(bizResult.Rows[idx]["EQPTNAME"]);
                                decimal dRackRate = Util.NVC_Decimal(bizResult.Rows[idx]["RACK_RATE"]);

                                _rackState = RackRateDifferenceValue(sEqptCode, dRackRate);
                                string rackRateText = RackRateDifferenceValueText(sEqptCode, dRackRate);
                                if (_rackState == SetRackState.Danger)
                                {
                                    sDangerMsgEqptName = string.IsNullOrEmpty(sDangerMsgEqptName) ? sEqptName : sDangerMsgEqptName + ", " + sEqptName;
                                    sDangerMsgRackRate = string.IsNullOrEmpty(sDangerMsgRackRate) ? rackRateText + "%" : sDangerMsgRackRate + ", " + rackRateText + "%";
                                }
                                else if (_rackState == SetRackState.Warning)
                                {
                                    sWarningMsgEqptName = string.IsNullOrEmpty(sWarningMsgEqptName) ? sEqptName : sWarningMsgEqptName + ", " + sEqptName;
                                    sWarningMsgRackRate = string.IsNullOrEmpty(sWarningMsgRackRate) ? rackRateText + "%" : sWarningMsgRackRate + ", " + rackRateText + "%";
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(sDangerMsgEqptName))
                            ShowRackRateMode(SetRackState.Danger, sDangerMsgEqptName, sDangerMsgRackRate, "C");

                        if (!string.IsNullOrEmpty(sWarningMsgEqptName))
                            ShowRackRateMode(SetRackState.Warning, sWarningMsgEqptName, sWarningMsgRackRate, "A");
                    }
                });
            }
        }

        private SetRackState RackRateDifferenceValue(string equipmentCode, decimal rackRate)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_PRDT_WH";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("WH_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["WH_ID"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(bizResult))
            {
                if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5 <= rackRate)
                {
                    return SetRackState.Danger;
                }
                else if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10 <= rackRate)
                {
                    return SetRackState.Warning;
                }
                else
                {
                    return SetRackState.Normal;
                }
            }

            return SetRackState.Normal;
        }

        private string RackRateDifferenceValueText(string equipmentCode, decimal rackRate)
        {
            const string bizRuleName = "DA_BAS_SEL_TB_MMD_PRDT_WH";

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("WH_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["WH_ID"] = equipmentCode;
            inTable.Rows.Add(dr);

            DataTable bizResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);
            if (CommonVerify.HasTableRow(bizResult))
            {
                if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5 <= rackRate)
                {
                    return $"{(bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 5):###,###,###,##0.##}";
                }
                else if (bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10 <= rackRate)
                {
                    return $"{(bizResult.Rows[0]["OPTM_LOAD_RATE"].GetDecimal() - 10):###,###,###,##0.##}";
                }
                else
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }


        private void HideAllRackRateMode()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   //GM2�߰� 2023-08-24  �念ö
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 3; i > 1; i--)
                    {
                        if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].Height.Value <= 0) continue;

                        GridLengthAnimation gla = new GridLengthAnimation
                        {
                            From = new GridLength(0.1, GridUnitType.Star),
                            To = new GridLength(0, GridUnitType.Star),
                            AccelerationRatio = 0.8,
                            DecelerationRatio = 0.2,
                            Duration = new TimeSpan(0, 0, 0, 0, 500)
                        };
                        gla.Completed += HideTestAnimationCompleted;
                        UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].BeginAnimation(RowDefinition.HeightProperty, gla);
                    }
                    _isRackRateMode = false;
                }));
            }
        }

        private void HideRackRateMode()
        {
            if (LoginInfo.CFG_SHOP_ID.Equals("G671") || LoginInfo.CFG_SHOP_ID.Equals("G673"))   // GM2 �߰�   2023-08-24  �念ö
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 3; i > 1; i--)
                    {
                        if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[i].Height.Value > 0)
                        {
                            GridLengthAnimation gla = new GridLengthAnimation
                            {
                                From = new GridLength(0.1, GridUnitType.Star),
                                To = new GridLength(0, GridUnitType.Star),
                                AccelerationRatio = 0.8,
                                DecelerationRatio = 0.2,
                                Duration = new TimeSpan(0, 0, 0, 0, 500)
                            };
                            gla.Completed += HideTestAnimationCompleted;
                            UcAssemblyProductLot.ProductLotContents.RowDefinitions[3].BeginAnimation(RowDefinition.HeightProperty, gla);
                        }
                    }
                    _isRackRateMode = false;
                }));
            }
        }

        private void HideTestAnimationCompleted(object sender, EventArgs e)
        {

        }

        private void HideRackRateAnimationCompleted(int i)
        {
            if (i == 3)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcAssemblyProductLot.txtCathodeRackRateMode.Text = string.Empty;
            }
            else
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                UcAssemblyProductLot.txtAnodeRackRateMode.Text = string.Empty;
            }
        }

        private void ShowRackRateMode(SetRackState rackState, string equipmentName, string rackRate, string polarityCode)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int gridrowIndex;
                if (polarityCode.Equals("C"))
                    gridrowIndex = 2;
                else
                    gridrowIndex = 3;

                if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0)
                {
                    //�׸��� Row Height ���� ����
                    GridLengthAnimation lengthAnimation = new GridLengthAnimation
                    {
                        From = new GridLength(0.1, GridUnitType.Star),
                        To = new GridLength(0, GridUnitType.Star),
                        AccelerationRatio = 0.8,
                        DecelerationRatio = 0.2,
                        Duration = new TimeSpan(0, 0, 0, 0, 500)
                    };
                    UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, lengthAnimation);
                }

                if (rackState == SetRackState.Normal) return;
                //if (UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].Height.Value > 0) return;

                string messageCode = string.Empty;

                if (rackState == SetRackState.Danger)
                    messageCode = "SFU8891";
                else if (rackState == SetRackState.Warning)
                    messageCode = "SFU8890";

                object[] parameters = new object[2];
                parameters[0] = equipmentName;
                parameters[1] = rackRate;

                if (polarityCode.Equals("C"))
                {
                    UcAssemblyProductLot.txtCathodeRackRateMode.Text = GetMessage(messageCode, parameters);
                }
                else
                {
                    UcAssemblyProductLot.txtAnodeRackRateMode.Text = GetMessage(messageCode, parameters);
                }

                GridLengthAnimation gla = new GridLengthAnimation
                {
                    From = new GridLength(0, GridUnitType.Star),
                    To = new GridLength(0.1, GridUnitType.Star),
                    AccelerationRatio = 0.8,
                    DecelerationRatio = 0.2,
                    Duration = new TimeSpan(0, 0, 0, 0, 500)
                };

                if (polarityCode.Equals("C"))
                {
                    gla.Completed += (sender, e) => { ShowCathodeAnimationCompleted(rackState); };
                }
                else
                {
                    gla.Completed += (sender, e) => { ShowAnodeAnimationCompleted(rackState); };
                }

                UcAssemblyProductLot.ProductLotContents.RowDefinitions[gridrowIndex].BeginAnimation(RowDefinition.HeightProperty, gla);

            }));

            _isRackRateMode = true;
        }

        private void ShowCathodeAnimationCompleted(SetRackState rackState)
        {
            string name;
            if (rackState == SetRackState.Danger)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcAssemblyProductLot.recCathodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                name = string.Empty;
                return;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                //Duration = TimeSpan.FromSeconds(0.8),
                Duration = TimeSpan.FromSeconds(2.0),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, name);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void ShowAnodeAnimationCompleted(SetRackState rackState)
        {
            string name;
            if (rackState == SetRackState.Danger)
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = redBrush;
                name = "redBrush";
            }
            else if (rackState == SetRackState.Warning)
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = yellowBrush;
                name = "yellowBrush";
            }
            else
            {
                UcAssemblyProductLot.recAnodeRackRateMode.Fill = new SolidColorBrush(Colors.Transparent);
                name = string.Empty;
                return;
            }

            DoubleAnimation opacityAnimation = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                //Duration = TimeSpan.FromSeconds(0.8),
                Duration = TimeSpan.FromSeconds(2.0),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTargetName(opacityAnimation, name);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(Brush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);
            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private string GetMessage(string messageId, params object[] parameters)
        {
            if (string.IsNullOrEmpty(messageId)) return string.Empty;

            string message = MessageDic.Instance.GetMessage(messageId);
            message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
            if (message.IndexOf("%1", StringComparison.Ordinal) > -1)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    message = message.Replace("%" + (i + 1), parameters[i].ToString());
                }
            }
            else
            {
                message = MessageDic.Instance.GetMessage(messageId, parameters);
            }
            return message;
        }


        #endregion

        #region[[Validation]

        private bool ValidationWaitLot()
        {
            if (string.IsNullOrEmpty(_processCode))
            {
                // ������ �����ϼ���.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationRemarkHistory()
        {

            // NND ������ô������ ��� ��.
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //���õ� �׸��� �����ϴ�.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationEqptCondSearch()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationEqptCond()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIP_TYPE_CODE")).Equals("OUT"))
                {
                    Util.MessageValidation("SFU3086", Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID")));
                    return false;
                }
            }

            return true;
        }

        private bool ValidationWipNote()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationRunStart()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            if (!CheckSelectWorkOrderInfo())
            {
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (string.IsNullOrEmpty(_equipmentSegmentCode))
                {
                    // ������ ���� �ϼ���.
                    Util.MessageValidation("SFU1255");
                    return false;
                }
                /*
                if (_dvProductLot == null)
                {
                    // ���õ� �׸��� �����ϴ�.
                    Util.MessageValidation("SFU1651");
                    return false;
                }
                
                if (_dvProductLot["WIPSTAT"].GetString() != "WAIT")
                {
                    // ��� LOT�� �������ּ���
                    Util.MessageValidation("SFU1492");
                    return false;
                }
                */

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("WAIT"))
                {
                    Util.MessageValidation("SFU1492");
                    return false;
                }


                if (string.IsNullOrEmpty(_equipmentMountPositionCode))
                {
                    //Util.Alert("����ǰ ������ġ ���������� �����ϴ�.");
                    Util.MessageValidation("SFU1543");
                    return false;
                }
            }
            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC�����ż�
            {
                DataTable dt = ((DataView)DgProductLot.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("EQPTID") == UcAssemblyProductLot.txtSelectEquipment.Text
                                   && t.Field<string>("WIPSTAT") == "PROC"
                             select t).ToList();

                if (query.Any())
                {
                    //"��� ���� �� �� LOT�� ���� �մϴ�."
                    Util.MessageValidation("SFU1863");
                    return false;
                }
            }

            else if (_processCode == Process.STACKING_FOLDING)
            {
            }
            else if (_processCode == Process.PACKAGING)
            {
            }

            return true;
        }

        private bool ValidationRunCancel()
        {
            //if (_dvProductLot == null)
            //{
            //    Util.MessageValidation("SFU1651");
            //    return false;
            //}

            //if (_dvProductLot["WIPSTAT"].GetString() != "PROC")
            //{
            //    Util.MessageValidation("SFU1846");
            //    return false;
            //}

            //string parentLot = _dvProductLot["PR_LOTID"].GetString();
            //string childSeq = _dvProductLot["CHILD_GR_SEQNO"].GetString();

            int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");
            if (idx < 0)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (!Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPSTAT")).Equals("PROC"))
            {
                //Util.Alert("�۾����� LOT�� �ƴմϴ�.");
                Util.MessageValidation("SFU1846");
                return false;
            }

            string parentLot = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID"));
            string childSeq = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "CHILD_GR_SEQNO"));

            int cut = 0;

            if (!int.TryParse(childSeq, out cut))
            {
                //Util.Alert("CUT�� ���ڰ� �ƴմϴ�.");
                //return bRet;
            }

            // ���� �۾� lot ���� ��� ����.
            for (int i = 0; i < UcAssemblyProductLot.dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(parentLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > cut)
                        {
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "LOTID")));
                            return false;
                        }
                    }
                }
            }

            // Max CUT DB Ȯ��.
            string maxSeq = string.Empty;
            int tempMinSeq = 0;

            maxSeq = GetMaxChildGRPSeq(parentLot);
            int.TryParse(maxSeq, out tempMinSeq);

            if (tempMinSeq > 0 && tempMinSeq > cut)
            {
                //Util.Alert("���� CUT�� �����Ͽ� ����� �� �����ϴ�.");
                Util.MessageValidation("SFU1790");
                return false;
            }

            return true;
        }

        private bool ValidationRunComplete()
        {
            // ���Ϸ� Validation
            if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
            {   //���õ� �׸��� �����ϴ�.
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "PROC")
            {
                //���Ϸ� �� �� �ִ� LOT���°� �ƴմϴ�.
                Util.MessageValidation("SFU1866");
                return false;
            }

            if (_processCode == Process.NOTCHING)
            {
                if (_selectUserModeAreaList.Contains(LoginInfo.CFG_AREA_ID))
                {
                    GetWorkCalander();

                    DataRow[] drShift = DataTableConverter.Convert(DgEquipment.ItemsSource).Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'");

                    if (string.IsNullOrEmpty(drShift[0]["VAL001"].GetString()))
                    {
                        Util.MessageValidation("SFU3752"); // �Է¿��� : �Էµ� �۾��� ������ �����ϴ�. ���������� ��� �ϰų� �۾��ڸ� ���� �ϼ���.
                        return false;
                    }

                    if (!string.IsNullOrEmpty(drShift[0]["WRK_END_DTTM"].GetString()))
                    {
                        DateTime shiftStartDateTime = Convert.ToDateTime(drShift[0]["WRK_STRT_DTTM"].GetString());
                        DateTime shiftEndDateTime = Convert.ToDateTime(drShift[0]["WRK_END_DTTM"].GetString());
                        DateTime systemDateTime = GetSystemTime();

                        int prevCheck = DateTime.Compare(systemDateTime, shiftStartDateTime);
                        int nextCheck = DateTime.Compare(systemDateTime, shiftEndDateTime);

                        if (prevCheck < 0 || nextCheck > 0)
                        {
                            Util.MessageValidation("SFU3752"); // �Է¿��� : �Էµ� �۾��� ������ �����ϴ�. ���������� ��� �ϰų� �۾��ڸ� ���� �ϼ���.

                            DataTable dt = ((DataView)DgEquipment.ItemsSource).Table;
                            dt.Select("EQPTID = '" + _equipmentCode + "' And SEQ = '2'").ToList<DataRow>().ForEach(r =>
                            {
                                r["SHFT_ID"] = string.Empty;            //txtShift.Text        
                                r["VAL001"] = string.Empty;             //txtShift.Tag
                                r["WRK_USERID"] = string.Empty;         //txtWorker.Text
                                r["VAL002"] = string.Empty;             //txtWorker.Tag
                                r["WRK_STRT_DTTM"] = string.Empty;      //txtShiftStartTime.Text
                                r["WRK_END_DTTM"] = string.Empty;       //txtShiftEndTime.Text
                            });
                            dt.AcceptChanges();

                            return false;
                        }
                    }
                }
            }
            else if (_processCode == Process.DNC || _processCode == Process.LAMINATION)     // 20250428 ESHG DNC�����ż�
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.STACKING_FOLDING)
            {
                // ������ġ �� mono �Ű��� ������ ���� ���� üũ. ������ AS-IS�� ������
                // SHOPID ��â �ڵ��� ����(A040), ��â ESS ����(A050)���� �ش�Ǹ�, CNB2���� ���谡 �����Ƿ� �ش� ������ �ݿ����� ����. CanStkConfirm

                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            // ��뱺 ESHM AZS_ECUTTER, AZS_STACKING ���� �߰��� ���� ����
            else if (_processCode == Process.AZS_ECUTTER)
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.AZS_STACKING)
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }
            else if (_processCode == Process.PACKAGING)
            {
                //���� �ٱ��� �Ϸ� ���� üũ AS-IS winInput.CanPkgConfirm
                if (!ValidationPackagingConfirm(_dvProductLot["LOTID"].GetString()))
                    return false;

                foreach (C1.WPF.DataGrid.DataGridRow row in UcAssemblyProductionResult.dgDefect.Rows)
                {
                    double dRsn, dOrgRsn = 0;

                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "RESNQTY")), out dRsn);
                    double.TryParse(Util.NVC(DataTableConverter.GetValue(row.DataItem, "ORG_RESNQTY")), out dOrgRsn);

                    if (dRsn != dOrgRsn)
                    {
                        // �������� ���� �ҷ� ������ �ֽ��ϴ�.
                        Util.MessageValidation("SFU1878", (action) =>
                        {
                            if (UcAssemblyProductionResult.tabDefect.Visibility == Visibility.Visible)
                                UcAssemblyProductionResult.tabDefect.IsSelected = true;
                        });
                        return false;
                    }
                }
            }

            return true;
        }

        private bool ValidationCompleteCancelRun()
        {
            if (_dvProductLot == null)
            {
                Util.MessageValidation("SFU1651");
                return false;
            }

            if (_dvProductLot["WIPSTAT"].GetString() != "EQPT_END")
            {
                // ���Ϸ� ������ LOT�� �ƴմϴ�.
                Util.MessageValidation("SFU1864");
                return false;
            }

            string parentLot = _dvProductLot["PR_LOTID"].GetString();
            string childSeq = _dvProductLot["CHILD_GR_SEQNO"].GetString();

            int cut = 0;

            if (!int.TryParse(childSeq, out cut))
            {
                //Util.Alert("CUT�� ���ڰ� �ƴմϴ�.");
                //return bRet;
            }

            // ���� �۾� lot ���� ��� ����.
            for (int i = 0; i < UcAssemblyProductLot.dgProductLot.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "PR_LOTID")).Equals(parentLot))
                {
                    if (!Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO")).Equals(""))
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "CHILD_GR_SEQNO"))) > cut)
                        {
                            //Util.Alert("���� CUT�� �����Ͽ� ����� �� �����ϴ�.\n( LOT : {0} )", Util.NVC(DataTableConverter.GetValue(dgProductLot.Rows[i].DataItem, "LOTID")));
                            Util.MessageValidation("SFU1791", Util.NVC(DataTableConverter.GetValue(UcAssemblyProductLot.dgProductLot.Rows[i].DataItem, "LOTID")));
                            return false;
                        }
                    }
                }
            }

            string maxSeq = string.Empty;
            int tempMinSeq = 0;

            maxSeq = GetMaxChildGRPSeq(parentLot);
            int.TryParse(maxSeq, out tempMinSeq);

            if (tempMinSeq > 0 && tempMinSeq > cut)
            {
                //Util.Alert("���� CUT�� �����Ͽ� ����� �� �����ϴ�.");
                Util.MessageValidation("SFU1790");
                return false;
            }

            return true;
        }

        private bool ValidationCancelConfirm()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationSpclProdMode()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageValidation("SFU1673");
                return false;
            }

            string processLotId = string.Empty;
            if (ValidationProcessWip(out processLotId))
            {
                Util.MessageValidation("SFU3199", processLotId); // �������� LOT�� �ֽ��ϴ�. LOT ID : {% 1}
                return false;
            }

            string equipmentDisplayName = string.Empty;
            processLotId = string.Empty;

            if (ValidationLoaderEquipmentProcessWip(out equipmentDisplayName, out processLotId))
            {
                Util.MessageValidation("SFU3747", equipmentDisplayName, processLotId); // �۾����� : �δ� ���� �����ϴ� [%1] ���� �������� LOT [%2] �� �ֽ��ϴ�.
                return false;
            }

            return true;
        }

        private bool ValidationPilotProdMode()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                // ���� ���� �ϼ���.
                Util.MessageValidation("SFU1673");
                return false;
            }

            //if (_processCode == Process.STACKING_FOLDING) --> ������ ������ LOT Validation (GM2 ���� Pjt �念ö)
            //{
                string processLotId;
                if (CheckProcessWip(out processLotId))
                {
                    Util.MessageValidation("SFU3199", processLotId); // �������� LOT�� �ֽ��ϴ�. LOT ID : {% 1}
                    return false;
                }
            //}

            return true;
        }

        private bool ValidationRework()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            return true;
        }

        private bool ValidationQualityInput()
        {
            if (_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK") < 0)
            {
                //Util.Alert("���õ� �׸��� �����ϴ�.");
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        private bool ValidationPrint()
        {
            if (LoginInfo.CFG_SERIAL_PRINT == null || LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                //Util.Alert("����Ʈ ȯ�� �������� �����ϴ�.");
                Util.MessageValidation("SFU2003");
                return false;
            }

            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                //Util.Alert("������ ���� �ϼ���.");
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                //Util.Alert("���� ���� �ϼ���.");
                Util.MessageInfo("SFU1673");
                return false;
            }

            return true;
        }

        private bool ValidationLogMarge()
        {
            if (string.IsNullOrEmpty(_equipmentSegmentCode))
            {
                // ������ ���� �ϼ���.
                Util.MessageValidation("SFU1255");
                return false;
            }

            return true;
        }

        private bool ValidationCancel()
        {
            /*
            if (_dvProductLot == null)
            {
                // ����� LOT�� �����ϼ���.
                Util.MessageValidation("SFU1938"); 
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "WAIT")
            {
                return false;
            }

            if (_dvProductLot["WIPSTAT"].ToString() == "EQPT_END")
            {
                // ���� �ϰ� LOT�� ����� �� �����ϴ�.
                Util.MessageValidation("SFU1671");  
                return false;
            }
            */

            return true;
        }

        private bool ValidationConfirm()
        {
            try
            {

                if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
                {
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _equipmentCode;
                newRow["LOTID"] = _dvProductLot["LOTID"].GetString();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_AUTO_CONFIRM_LOT", "INDATA", "OUTDATA", inDataTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    if (Util.NVC(dtRslt.Rows[0]["RESULT_VALUE"]).Equals("OK"))
                    {
                        return true;
                    }
                    else
                    {
                        Util.MessageValidation(Util.NVC(dtRslt.Rows[0]["RESULT_CODE"]));
                        return false;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationLoaderEquipmentProcessWip(out string equipmentDisplayName, out string processLotId)
        {
            equipmentDisplayName = string.Empty;
            processLotId = string.Empty;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["LANGID"] = LoginInfo.LANGID;
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_CHK_WIP_BY_LDR_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    equipmentDisplayName = Util.NVC(dtRslt.Rows[0]["EQPTDSPNAME"]);
                    processLotId = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool ValidationPackagingConfirm(string lotId)
        {
            const string bizRuleName = "DA_PRD_SEL_CURR_IN_LOT_LIST_L";
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = lotId;
            inTable.Rows.Add(newRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (dtResult.Rows[i]["MOUNT_MTRL_TYPE_CODE"].GetString() == "PROD")
                    {
                        if (dtResult.Rows[i]["WIPSTAT"].GetString() == "PROC" && dtResult.Rows[i]["PROD_LOTID"].GetString() == lotId)
                        {
                            object[] parameters = new object[2];
                            parameters[0] = dtResult.Rows[i]["EQPT_MOUNT_PSTN_NAME"].GetString();
                            parameters[1] = dtResult.Rows[i]["INPUT_LOTID"].GetString();
                            //[{0}] ��ġ�� ���ԿϷ���� ���� �ٱ���[{1}]�� ���� �մϴ�.
                            Util.MessageValidation("SFU1282", parameters);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool ValidationProcessWip(out string processLotId)
        {
            processLotId = string.Empty;

            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    processLotId = Util.NVC(dtRslt.Rows[0]["LOTID"]);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }


        }

        //private bool ValidationUpdateLaneNo()
        //{
        //    return true;
        //}

        private bool CheckProcessWip(out string processLotId)
        {
            processLotId = string.Empty;

            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtRow["WIPSTAT"] = Wip_State.PROC;
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    processLotId = Util.NVC(dtResult.Rows[0]["LOTID"]);
                    bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckEquipmentConfirmType(string buttonGroupId)
        {
            try
            {
                if (_dvProductLot == null || string.IsNullOrEmpty(_dvProductLot["LOTID"].GetString()))
                {
                    //���õ� �׸��� �����ϴ�.
                    Util.MessageValidation("SFU1651");
                    return false;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _equipmentCode;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    if (dtResult.Columns.Contains("RSLT_CNFM_TYPE") && Util.NVC(dtResult.Rows[0]["RSLT_CNFM_TYPE"]).Equals("A"))
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupId(buttonGroupId))  // ��ư ���� üũ.
                        {
                            //�ش� ����� �ڵ�����Ȯ�� ���� �Դϴ�.
                            Util.MessageValidation("SFU6034");
                            return false;
                        }
                    }
                    else
                    {
                        if (!CheckButtonPermissionGroupByBtnGroupId(buttonGroupId))  // ��ư ���� üũ.
                        {
                            Util.MessageValidation("SFU3520", LoginInfo.USERID, buttonGroupId.Equals("EQPT_END_W") ? ObjectDic.Instance.GetObjectName("���Ϸ�") : ObjectDic.Instance.GetObjectName("����Ȯ��")); // �ش� USER[%1]�� ����[%2]�� ������ ���� �ʽ��ϴ�.
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckButtonPermissionGroupByBtnGroupId(string buttonGroupId)
        {
            try
            {
                bool bRet = false;
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["PROCID"] = _processCode;
                dtRow["EQGRID"] = _processCode == Process.STACKING_FOLDING ? "STK" : null;  // STACKING, FOLDING ������ô�� ��� ������ PROCID�� ���������, EQGRID �� �б�ó���� �ʿ� ��.
                inTable.Rows.Add(dtRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_DRB", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    DataRow[] drs = dtResult.Select("BTN_PMS_GRP_CODE = '" + buttonGroupId + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool IsCurrentProcessByLotId()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LOTID", typeof(string));

            DataRow dtRow = inTable.NewRow();
            dtRow["LOTID"] = _dvProductLot["LOTID"].GetString();
            inTable.Rows.Add(dtRow);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                if (dtResult.Rows[0]["PROCID"].GetString() == _dvProductLot["PROCID"].ToString())
                {
                    return _dvProductLot["PROCID"].ToString() == dtResult.Rows[0]["PROCID"].ToString();
                }
            }
            return false;
        }

        private async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (UcAssemblyProductLot.IsSearchResult) succeeded = true;
                await System.Threading.Tasks.Task.Delay(500);
            }
            return true;
        }

        #endregion

        #region [�˾�]


        private void PopupWaitLot()
        {
            ASSY005_COM_WAITLOT popWaitLot = new ASSY005_COM_WAITLOT();
            popWaitLot.FrameOperation = FrameOperation;

            if (_processCode == Process.STACKING_FOLDING)
            {
                object[] parameters = new object[5];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = Process.STACKING_FOLDING;
                //parameters[3] = _unldrLotIdentBasCode;
                parameters[3] = _ldrLotIdentBasCode;
                // stacking�� folding�� �ϳ��� ProcID�� ���յǾ� ����.
                //�׷��� ���� �и��� ���Ѿ� �ϱ� ������ �����Ͽ� ���� ���� �ش�.
                parameters[4] = EquipmentGroup.STACKING;
                C1WindowExtension.SetParameters(popWaitLot, parameters);
                popWaitLot.Closed += popWaitLot_Closed;
                Dispatcher.BeginInvoke(new Action(() => popWaitLot.ShowModal()));
            }
            else
            {
                object[] parameters = new object[4];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                //parameters[3] = _unldrLotIdentBasCode;
                parameters[3] = _ldrLotIdentBasCode;
                C1WindowExtension.SetParameters(popWaitLot, parameters);

                popWaitLot.Closed += popWaitLot_Closed;
                Dispatcher.BeginInvoke(new Action(() => popWaitLot.ShowModal()));
            }
        }

        private void popWaitLot_Closed(object sender, EventArgs e)
        {
            ASSY005_COM_WAITLOT popup = sender as ASSY005_COM_WAITLOT;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        private void PopupRemarkHistory()
        {
            ASSY005_001_LOTCOMMENTHIST popLotCommentHistory = new ASSY005_001_LOTCOMMENTHIST();
            popLotCommentHistory.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "PR_LOTID"));//_dvProductLot["PR_LOTID"].ToString();
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));//_dvProductLot["WIPSEQ"].ToString();
            C1WindowExtension.SetParameters(popLotCommentHistory, parameters);

            popLotCommentHistory.Closed += popLotCommentHistory_Closed;
            Dispatcher.BeginInvoke(new Action(() => popLotCommentHistory.ShowModal()));
        }

        private void popLotCommentHistory_Closed(object sender, EventArgs e)
        {
            ASSY005_001_LOTCOMMENTHIST popup = sender as ASSY005_001_LOTCOMMENTHIST;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupEqptCondSearch()
        {
            CMM_ASSY_EQPT_COND_SEARCH popEqptCondSearch = new CMM_ASSY_EQPT_COND_SEARCH();
            popEqptCondSearch.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = _processCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = _equipmentSegmentName;
            parameters[4] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popEqptCondSearch, parameters);

            popEqptCondSearch.Closed += popEqptCondSearch_Closed;
            Dispatcher.BeginInvoke(new Action(() => popEqptCondSearch.ShowModal()));
        }

        private void popEqptCondSearch_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND_SEARCH popup = sender as CMM_ASSY_EQPT_COND_SEARCH;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupEqptCond()
        {
            CMM_ASSY_EQPT_COND popEqptCond = new CMM_ASSY_EQPT_COND();
            popEqptCond.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));//_dvProductLot["LOTID"].ToString();
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ")); //_dvProductLot["WIPSEQ"].ToString();
            parameters[5] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popEqptCond, parameters);

            popEqptCond.Closed += popEqptCond_Closed;
            Dispatcher.BeginInvoke(new Action(() => popEqptCond.ShowModal()));
        }

        private void popEqptCond_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_COND popup = sender as CMM_ASSY_EQPT_COND;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupWipNote()
        {
            CMM_COM_WIP_NOTE popWipNote = new CMM_COM_WIP_NOTE();
            popWipNote.FrameOperation = FrameOperation;

            object[] parameters = new object[3];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            C1WindowExtension.SetParameters(popWipNote, parameters);

            popWipNote.Closed += popWipNote_Closed;
            Dispatcher.BeginInvoke(new Action(() => popWipNote.ShowModal()));
        }

        private void popWipNote_Closed(object sender, EventArgs e)
        {
            CMM_COM_WIP_NOTE popup = sender as CMM_COM_WIP_NOTE;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupRunStart()
        {
            if (_processCode == Process.NOTCHING)
            {
                string equipmentName = UcAssemblyProductLot.txtSelectEquipmentName.Text;

                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipmentName.Text))
                {
                    equipmentName = DgEquipment?.CurrentCell?.Row?.DataItem?.GetString();

                    if (!string.IsNullOrEmpty(equipmentName))
                    {
                        equipmentName = equipmentName.Replace("System.Data.DataRowView", string.Empty);

                        if (equipmentName.Split(':').Length == 2)
                        {
                            equipmentName = equipmentName.Split(':')[1].Trim();
                        }
                    }
                }

                int idx = _util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK");

                ASSY005_001_RUNSTART popupRunStart = new ASSY005_001_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[10];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                parameters[3] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_LOTID"));//_dvProductLot["PR_LOTID"].ToString();
                parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "PR_CSTID"));//_dvProductLot["PR_CSTID"].ToString();
                parameters[5] = _equipmentMountPositionCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "MO_PRODID"));//_dvProductLot["MO_PRODID"].ToString();
                parameters[8] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[idx].DataItem, "WIPQTY_M_EA")); //_dvProductLot["WIPQTY_M_EA"].ToString();
                parameters[9] = equipmentName;// UcAssemblyProductLot.txtSelectEquipmentName.Text;
                C1WindowExtension.SetParameters(popupRunStart, parameters);

                popupRunStart.Closed += popupRunStart_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
            else
            {
                ASSY005_COM_RUNSTART popupRunStart = new ASSY005_COM_RUNSTART { FrameOperation = FrameOperation };

                object[] parameters = new object[3];
                parameters[0] = _equipmentSegmentCode;
                parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[2] = _processCode;
                C1WindowExtension.SetParameters(popupRunStart, parameters);
                popupRunStart.Closed += popupRunStart_Closed;

                Dispatcher.BeginInvoke(new Action(() => popupRunStart.ShowModal()));
            }
        }

        private void popupRunStart_Closed(object sender, EventArgs e)
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_RUNSTART pop = sender as ASSY005_001_RUNSTART;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    string processLotId = SelectProcessLotId();

                    //SetProductLotList();
                    // ���� Lot ����ȸ
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
            else
            {
                ASSY005_COM_RUNSTART pop = sender as ASSY005_COM_RUNSTART;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    //ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    //SetProductLotList();
                    string processLotId = SelectProcessLotId();

                    //SetProductLotList();
                    // ���� Lot ����ȸ
                    SelectProductLot(processLotId);
                    SetProductLotList(processLotId);
                }
            }
        }

        private void PopupCancelConfirm()
        {
            CMM_ASSY_CANCEL_CONFIRM_PROD popupCancelConfrim = new CMM_ASSY_CANCEL_CONFIRM_PROD();
            popupCancelConfrim.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = UcAssemblyProductLot.txtSelectEquipmentName.Text;

            C1WindowExtension.SetParameters(popupCancelConfrim, parameters);

            popupCancelConfrim.Closed += popupCancelConfrim_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCancelConfrim.ShowModal()));
        }

        private void popupCancelConfrim_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_CANCEL_CONFIRM_PROD pop = sender as CMM_ASSY_CANCEL_CONFIRM_PROD;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                //SetProductLotList();
            }
            if (string.IsNullOrEmpty(_equipmentSegmentCode) || string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;
            SetProductLotList();
        }

        private void PopupLotMerge()
        {
            ASSY005_OUTLOT_MERGE popLotMerge = new ASSY005_OUTLOT_MERGE();
            popLotMerge.FrameOperation = FrameOperation;

            object[] parameters = new object[5];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = _processCode;
            parameters[3] = _equipmentSegmentName;
            parameters[4] = _ldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popLotMerge, parameters);
            popLotMerge.Closed += popLotMerge_Closed;
            Dispatcher.BeginInvoke(new Action(() => popLotMerge.ShowModal()));
        }

        private void popLotMerge_Closed(object sender, EventArgs e)
        {
            ASSY005_OUTLOT_MERGE pop = sender as ASSY005_OUTLOT_MERGE;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }

            if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text)) return;

            Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
        }

        private void PopupChangeCarrier()
        {
            CMM_COM_CHG_CARRIER popupChangeCarrier = new CMM_COM_CHG_CARRIER();
            popupChangeCarrier.FrameOperation = FrameOperation;

            object[] parameters = new object[2];
            parameters[0] = _processCode; //Process.STACKING_FOLDING;
            parameters[1] = "N";

            C1WindowExtension.SetParameters(popupChangeCarrier, parameters);
            popupChangeCarrier.Closed += popupChangeCarrier_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupChangeCarrier.ShowModal()));
        }

        private void popupChangeCarrier_Closed(object sender, EventArgs e)
        {
            CMM_COM_CHG_CARRIER pop = sender as CMM_COM_CHG_CARRIER;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupTrayInfo()
        {
            CMM_PKG_TRAY_INFO popupTrayInfo = new CMM_PKG_TRAY_INFO();
            popupTrayInfo.FrameOperation = FrameOperation;

            object[] parameters = new object[1];
            parameters[0] = _processCode; //Process.PACKAGING;
            C1WindowExtension.SetParameters(popupTrayInfo, parameters);

            popupTrayInfo.Closed += popupTrayInfo_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupTrayInfo.ShowModal()));
        }

        private void popupTrayInfo_Closed(object sender, EventArgs e)
        {
            CMM_PKG_TRAY_INFO pop = sender as CMM_PKG_TRAY_INFO;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void PopupReWork()
        {
            ASSY005_001_REWORK popupRework = new ASSY005_001_REWORK();
            popupRework.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _equipmentSegmentCode;
            parameters[1] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[2] = Process.VD_LMN;
            parameters[3] = _unldrLotIdentBasCode;

            C1WindowExtension.SetParameters(popupRework, parameters);
            popupRework.Closed += popupRework_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupRework.ShowModal()));
        }

        private void popupRework_Closed(object sender, EventArgs e)
        {
            ASSY005_001_REWORK pop = sender as ASSY005_001_REWORK;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(btnSearch, null)));
            }
        }

        private void PopupQualityInput()
        {
            CMM_COM_SELF_INSP popupQualityInput = new CMM_COM_SELF_INSP();
            popupQualityInput.FrameOperation = FrameOperation;

            object[] parameters = new object[6];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
            parameters[3] = UcAssemblyProductLot.txtSelectEquipmentName.Text;
            parameters[4] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "LOTID"));
            parameters[5] = Util.NVC(DataTableConverter.GetValue(DgProductLot.Rows[_util.GetDataGridCheckFirstRowIndex(DgProductLot, "CHK")].DataItem, "WIPSEQ"));

            C1WindowExtension.SetParameters(popupQualityInput, parameters);
            popupQualityInput.Closed += popupQualityInput_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupQualityInput.ShowModal()));
        }

        private void popupQualityInput_Closed(object sender, EventArgs e)
        {
            CMM_COM_SELF_INSP pop = sender as CMM_COM_SELF_INSP;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {
                // TODO : AS-IS Packaging ������ô���� ���� �����ڵ忡 ���尭���Է�üũ���θ� �����ϴ� ������ ������,
                // ��â�ڵ���1��, ��â�ڵ���2���� ���ѵǾ� ������, CNB2�� ����������ô���� �ش������ ���������� �ǴܵǾ� �ش� ������ �߰����� �ʴ´�. 
            }
        }

        private void PopupEquipmentEnd()
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_EQPTEND popupEquipmentEnd = new ASSY005_001_EQPTEND { FrameOperation = FrameOperation };

                object[] parameters = new object[9];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = UcAssemblyProductLot.txtSelectEquipment.Text;
                parameters[3] = _dvProductLot["LOTID"].GetString();
                parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                parameters[5] = _ldrLotIdentBasCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = _equipmentMountPositionCode;
                parameters[8] = CheckProdQtyChangePermission();

                C1WindowExtension.SetParameters(popupEquipmentEnd, parameters);
                popupEquipmentEnd.Closed += popupEquipmentEnd_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupEquipmentEnd.ShowModal()));
            }
            else
            {
                ASSY005_COM_EQPTEND popupEquipmentEnd = new ASSY005_COM_EQPTEND { FrameOperation = FrameOperation };

                object[] parameters = new object[9];
                parameters[0] = _processCode;
                parameters[1] = _equipmentSegmentCode;
                parameters[2] = _equipmentCode;
                parameters[3] = _dvProductLot["LOTID"].GetString();
                parameters[4] = _dvProductLot["WIPSEQ"].GetString();
                parameters[5] = _ldrLotIdentBasCode;
                parameters[6] = _unldrLotIdentBasCode;
                parameters[7] = CheckProdQtyChangePermission();
                parameters[8] = "N";
                C1WindowExtension.SetParameters(popupEquipmentEnd, parameters);
                popupEquipmentEnd.Closed += popupEquipmentEnd_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupEquipmentEnd.ShowModal()));
            }
        }

        private void popupEquipmentEnd_Closed(object sender, EventArgs e)
        {
            if (_processCode == Process.NOTCHING)
            {
                ASSY005_001_EQPTEND pop = sender as ASSY005_001_EQPTEND;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    SetProductLotList();
                }
            }
            else
            {
                ASSY005_COM_EQPTEND pop = sender as ASSY005_COM_EQPTEND;
                if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                {
                    ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                    SetProductLotList();
                }
            }
        }

        private void PopupPrint()
        {

            ASSY005_001_HIST popupPrint = new ASSY005_001_HIST();
            popupPrint.FrameOperation = FrameOperation;

            object[] parameters = new object[4];
            parameters[0] = _processCode;
            parameters[1] = _equipmentSegmentCode;
            parameters[2] = _equipmentCode;
            //_UNLDR�ڵ带 popupPrint ������.
            parameters[3] = _unldrLotIdentBasCode;
            C1WindowExtension.SetParameters(popupPrint, parameters);
            popupPrint.Closed += popupPrint_Closed;

            Dispatcher.BeginInvoke(new Action(() => popupPrint.ShowModal()));
        }

        private void popupPrint_Closed(object sender, EventArgs e)
        {
            ASSY005_001_HIST pop = sender as ASSY005_001_HIST;
            if (pop != null && pop.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        private void popupConfirm_Closed(object sender, EventArgs e)
        {
            try
            {
                if (_processCode == Process.NOTCHING)
                {
                    ASSY005_001_CONFIRM pop = sender as ASSY005_001_CONFIRM;
                    if (pop != null && pop.DialogResult == MessageBoxResult.OK && _isPopupConfirmJobEnd)
                    {
                        // ���� ���� üũ �� �̹��� �� �ڵ� ���� ó��
                        AutoPrint(pop.CHECKED);

                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }
                else
                {
                    ASSY005_COM_CONFIRM pop = sender as ASSY005_COM_CONFIRM;
                    if (pop != null && pop.DialogResult == MessageBoxResult.OK)
                    {
                        ButtonProductLot_Click(UcAssemblyCommand.btnProductLot, null);
                        SetProductLotList();
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _isPopupConfirmJobEnd = true;
            }
        }

        /// <summary>
        /// ������ ���⼳��
        /// </summary>
        private void PopupWorkHalfSlitSide()
        {
            if (!ValidationWorkHalfSlitSide()) return;

            // ������/���� ���� 2���� ��� ����ϴ� AREA �� ���
            if (_util.IsCommonCodeUse("NON_COATED_WINDING_DIRCTN_USE_AREA", LoginInfo.CFG_AREA_ID))
            {
                CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING_ROLL_DIRCTN { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[3];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;
                    Parameters[2] = _processCode;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
            else
            {
                CMM_ELEC_WORK_HALF_SLITTING popupWorkHalfSlitSide = new CMM_ELEC_WORK_HALF_SLITTING { FrameOperation = FrameOperation };
                if (popupWorkHalfSlitSide != null)
                {
                    popupWorkHalfSlitSide.FrameOperation = FrameOperation;
                    UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                    object[] Parameters = new object[2];
                    Parameters[0] = _equipmentCode;
                    Parameters[1] = UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag;

                    C1WindowExtension.SetParameters(popupWorkHalfSlitSide, Parameters);

                    popupWorkHalfSlitSide.Closed += new EventHandler(PopupWorkHalfSlitSide_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => popupWorkHalfSlitSide.ShowModal()));
                }
            }
        }

        private bool ValidationWorkHalfSlitSide()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // ���� ���� �ϼ���.
                return false;
            }

            return true;
        }

        private void GetWorkHalfSlittingSide()
        {
            try
            {
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _equipmentCode;
                inDataTable.Rows.Add(dr);

                string sBizrule = string.Empty;
                
                string sWorkHalfSlittingSideName = string.Empty;
                string sWorkHalfSlittingSide = string.Empty;
                sAttr2 = string.Empty;

                DataTable dtUWND = GetCommonUWND();
                sAttr2 = (dtUWND != null && dtUWND.Rows.Count > 0) ? dtUWND.Rows[0]["ATTR2"].ToString() : "N";

                if (sAttr2 == "Y")
                    sBizrule = "DA_PRD_SEL_CURR_MOUNT_MTRL_WRK_HALF_SLIT_SIDE";
                else
                    sBizrule = "DA_PRD_SEL_WRK_HALF_SLIT_SIDE";

                new ClientProxy().ExecuteService(sBizrule, "RQSTDT", "RSLTDT", inDataTable, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(bizResult))
                    {
                        if (sAttr2 == "Y")
                        {
                            for (int i = 0; i <= dtUWND.Rows.Count; i++)
                            {
                                sWorkHalfSlittingSideName += i > 0 ? ", " : string.Empty;
                                sWorkHalfSlittingSide += i > 0 ? "," : string.Empty;

                                sWorkHalfSlittingSideName += bizResult.Rows[i]["WRK_HALF_SLIT_SIDE_NAME"].ToString();
                                sWorkHalfSlittingSide += bizResult.Rows[i]["WRK_HALF_SLIT_SIDE"].ToString();
                            }

                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(sWorkHalfSlittingSideName);
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(sWorkHalfSlittingSide);
                        }
                        else
                        {
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE_NAME"]);
                            UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = Util.NVC(bizResult.Rows[0]["WRK_HALF_SLIT_SIDE"]);
                        }
                    }
                    else
                    {
                        UcAssemblyProductLot.txtWorkHalfSlittingSide.Text = string.Empty;
                        UcAssemblyProductLot.txtWorkHalfSlittingSide.Tag = null;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// �����ڵ忡 ��ϵ� �����ι��� �׸� �ҷ�����
        /// </summary>
        /// <returns></returns>
        private DataTable GetCommonUWND()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "MNG_SLITTING_SIDE_AREA";
                dr["COM_CODE"] = _processCode;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        /// <summary>
        /// ���� ���� ����
        /// </summary>
        private void PopupEmSectionRollDirctn()
        {
            if (!ValidationEmSectionRollDirctn()) return;

            CMM_ELEC_EM_SECTION_ROLL_DIRCTN popupEmSectionRollDirctn = new CMM_ELEC_EM_SECTION_ROLL_DIRCTN { FrameOperation = FrameOperation };
            if (popupEmSectionRollDirctn != null)
            {
                popupEmSectionRollDirctn.FrameOperation = FrameOperation;
                UcAssemblyCommand.btnExtra.IsDropDownOpen = false;

                object[] Parameters = new object[1];
                Parameters[0] = _equipmentCode;

                C1WindowExtension.SetParameters(popupEmSectionRollDirctn, Parameters);

                popupEmSectionRollDirctn.Closed += new EventHandler(PopupEmSectionRollDirctn_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => popupEmSectionRollDirctn.ShowModal()));
            }
        }

        private bool ValidationEmSectionRollDirctn()
        {
            if (string.IsNullOrWhiteSpace(UcAssemblyProductLot.txtSelectEquipment.Text))
            {
                Util.MessageValidation("SFU1673");     // ���� ���� �ϼ���.
                return false;
            }

            return true;
        }

        private void GetSlittingLaneNo()
        {
            try
            {
                UcAssemblyProductLot.txtSlittingLaneNo.Text = string.Empty;

                if (UcAssemblyProductLot.txtSlittingLaneNo.Visibility != Visibility.Visible)
                {
                    return;
                }

                if (string.IsNullOrEmpty(UcAssemblyProductLot.txtSelectEquipment.Text))
                {
                    return;
                }

                DataTable dtInDataTable = new DataTable("INDATA");
                dtInDataTable.Columns.Add("LANGID", typeof(string));
                dtInDataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtInDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = UcAssemblyProductLot.txtSelectEquipment.Text;
                dtInDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_INPUT_LANE_INFO", "RQSTDT", "RSLTDT", dtInDataTable, (dtBizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(dtBizResult))
                    {
                        if (dtBizResult.Columns["LANE_ID"] != null)
                        {
                            string sLineNo = "";
                            foreach (DataRow drLaneNo in dtBizResult.Rows)
                            {
                                if (drLaneNo != null && drLaneNo["LANE_ID"] != null)
                                {
                                    if (sLineNo.Length > 0)
                                    {
                                        sLineNo += ", ";
                                    }

                                    sLineNo += Util.NVC(drLaneNo["LANE_ID"]);
                                }
                            }

                            UcAssemblyProductLot.txtSlittingLaneNo.Text = sLineNo;
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void PopupUpdateLaneNo()
        //{
        //    CMM_COM_UPDATE_LANENO popUpdateLaneNo = new CMM_COM_UPDATE_LANENO();
        //    popUpdateLaneNo.FrameOperation = FrameOperation;

        //    object[] parameters = new object[1];
        //    parameters[0] = _processCode;
        //    C1WindowExtension.SetParameters(popUpdateLaneNo, parameters);

        //    popUpdateLaneNo.Closed += popUpdateLaneNo_Closed;
        //    Dispatcher.BeginInvoke(new Action(() => popUpdateLaneNo.ShowModal()));
        //}

        //private void popUpdateLaneNo_Closed(object sender, EventArgs e)
        //{
        //    CMM_COM_UPDATE_LANENO popup = sender as CMM_COM_UPDATE_LANENO;
        //    if (popup != null && popup.DialogResult == MessageBoxResult.OK)
        //    {
        //    }
        //}

        //ESMI 1�� ����(A4) �� ��� ���� �����ڵ� üũ�Ͽ� MES SFU Configurtion���� ���õ� ���������� �ƴ� '����������ô-6Line' ȭ�鿡 ���� �޺��ڽ��� ���õ� ���������� ��ȸ�ǰ� ����
        private bool IsEsmiLineCheck()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #endregion

    }
}
