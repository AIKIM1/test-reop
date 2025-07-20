using System.Windows;
using C1.WPF;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssySearch
    {
        public C1ComboBox ComboEquipmentSegment { get; set; }

        //public TextBlock TextBlock { get; set; }

        public Grid GridLine { get; set; }

        public C1ComboBox ComboEquipment { get; set; }

        public Button ButtonSearch { get; set; }

        public string ProcessCode { get; set; }

        public UcAssySearch()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            ComboEquipmentSegment = cboEquipmentSegmentAssy;
            ComboEquipment = cboEquipmentAssy;
            //TextBlock = cboLine;
            GridLine = grdLine;
            ButtonSearch = btnSearch;
        }

    }
}