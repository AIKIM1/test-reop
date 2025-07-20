using C1.WPF;
using C1.WPF.DataGrid;
using System;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// BaseUserControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BaseUserControl : UcBase
    {
        public Button SearchButton { get; set; }
        public Button StartButton { get; set; }
        public Button StartCancelButton { get; set; }
        public Button ConfirmButton { get; set; }
        public C1ComboBox EquipmentSegmentCombo { get; set; }
        public C1ComboBox EquipmentCombo { get; set; }
        public CheckBox RunCheck { get; set; }
        public CheckBox EndCheck { get; set; }
        public C1DataGrid ProdLotGrid { get; set; }
        public C1DataGrid DefectGrid { get; set; }
        public Button DefectSaveButton { get; set; }
        public C1DataGrid QualityGrid { get; set; }
        public Button QualitySaveButton { get; set; }
        public C1DataGrid RemarkGrid { get; set; }
        public Button RemarkSaveButton { get; set; }

        string processID;
        public string PROCID
        {
            get
            {
                return processID;
            }
            set
            {
                processID = value;
                //(grdSearch.Children[0] as UcSearch).PROCID = value;
            }
        }

        string lotId;
        public string LOTID
        {
            get
            {
                return lotId;
            }
            set
            {
                lotId = value;
            }
        }

        string wipStatus;
        public string WIPSTATUS
        {
            get
            {
                return wipStatus;
            }
            set
            {
                wipStatus = value;
            }
        }

        Action saveDefect;
        public Action SaveDefect
        {
            get
            {
                return saveDefect;
            }
            set
            {
                saveDefect = value;
            }
        }

        Action saveQuality;
        public Action SaveQuality
        {
            get
            {
                return saveQuality;
            }
            set
            {
                saveQuality = value;
            }
        }

        public BaseUserControl()
        {
            InitializeComponent();
            SetUserControls();
        }

        void SetUserControls()
        {
            StartButton = (grdCommand.Children[0] as UcCommand).btnStart;
            StartCancelButton = (grdCommand.Children[0] as UcCommand).btnCancel;
            ConfirmButton = (grdCommand.Children[0] as UcCommand).btnConfirm;
            SearchButton = (grdSearch.Children[0] as UcSearch).btnSearch;
            EquipmentSegmentCombo = (grdSearch.Children[0] as UcSearch).cboEquipmentSegment;
            EquipmentCombo = (grdSearch.Children[0] as UcSearch).cboEquipment;
            RunCheck = (grdSearch.Children[0] as UcSearch).chkRun;
            EndCheck = (grdSearch.Children[0] as UcSearch).chkEqpEnd;
            ProdLotGrid = (grdProdLot.Children[0] as UcProdLot).dgProductLot;
            DefectGrid = (grdDataCollect.Children[0] as UcDataCollect).dgDefect;
            QualityGrid = (grdDataCollect.Children[0] as UcDataCollect).dgQuality;
            RemarkGrid = (grdDataCollect.Children[0] as UcDataCollect).dgRemark;
            RemarkSaveButton = (grdDataCollect.Children[0] as UcDataCollect).btnSaveRemark;
        }
    }
}