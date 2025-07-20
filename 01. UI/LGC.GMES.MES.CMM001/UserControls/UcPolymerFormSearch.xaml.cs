using System.Windows;
using C1.WPF;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001.UserControls
{
    public partial class UcPolymerFormSearch
    {
        public C1ComboBox ComboEquipmentSegment { get; set; }
        public C1ComboBox ComboEquipment { get; set; }
        public C1ComboBox ComboProcess { get; set; }
        public C1ComboBox ComboInspector { get; set; }
        public PopupFindControl PopupFindControlShipto { get; set; }

        public Button ButtonSearch { get; set; }

        public string ProcessCode { get; set; }

        public string DivisionCode { get; set; }

        public UcPolymerFormSearch()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            ComboEquipmentSegment = cboEquipmentSegment;
            ComboEquipment = cboEquipment;
            ComboProcess = cboProcess;
            ComboInspector = cboInspector;
            PopupFindControlShipto = popShipto;
            ButtonSearch = btnSearch;
        }

        public void SetControlVisibility()
        {
            if (string.Equals(ProcessCode, Process.PolymerFinalExternalDSF)
                || string.Equals(ProcessCode, Process.PolymerFinalExternal)
                || string.Equals(ProcessCode, Process.PolymerFairQuality))
            {
                grShipto.Visibility = Visibility.Collapsed;
                grdContents.ColumnDefinitions[0].Width = new GridLength(13, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[2].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[6].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[8].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[9].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[10].Width = new GridLength(1, GridUnitType.Star);
            }
            else if (string.Equals(ProcessCode, Process.PolymerDSF))
            {
                grProcess.Visibility = Visibility.Collapsed;
                grdContents.ColumnDefinitions[0].Width = new GridLength(13, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[2].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[6].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[8].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[9].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[10].Width = new GridLength(1, GridUnitType.Star);
            }
            else if (string.Equals(ProcessCode, Process.CELL_BOXING_RETURN) || string.Equals(ProcessCode, Process.CELL_BOXING))
            {
                grInspectUser.Visibility = Visibility.Collapsed;
                grProcess.Visibility = Visibility.Collapsed;
                grShipto.Visibility = Visibility.Collapsed;
                grdContents.ColumnDefinitions[0].Width = new GridLength(13, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[6].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[8].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[9].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[10].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                grProcess.Visibility = Visibility.Collapsed;
                grShipto.Visibility = Visibility.Collapsed;
                grdContents.ColumnDefinitions[0].Width = new GridLength(13, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[2].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[4].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[6].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[7].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[8].Width = new GridLength(10, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[9].Width = new GridLength(0, GridUnitType.Pixel);
                grdContents.ColumnDefinitions[10].Width = new GridLength(1, GridUnitType.Star);
            }
        }

    }
}