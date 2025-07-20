using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcAssyShift
    {
        public TextBox TextShift { get; set; }
        public TextBox TextShiftDateTime { get; set; }
        public TextBox TextWorker { get; set; }
        public TextBox TextShiftStartTime { get; set; }
        public TextBox TextShiftEndTime { get; set; }
        public TextBox TextLossCnt { get; set; }
        public Button ButtonShift { get; set; }

        public string ProcessCode { get; set; }

        public UcAssyShift()
        {
            InitializeComponent();
            SetControl();
        }

        private void SetControl()
        {
            TextShift = txtShift;
            TextShiftDateTime = txtShiftDateTime;
            TextWorker = txtWorker;
            TextShiftStartTime = txtShiftStartTime;
            TextShiftEndTime = txtShiftEndTime;
            TextLossCnt = txtLossCnt;
            ButtonShift = btnShift;
        }

        public void SetControlProperties()
        {
            if (string.Equals(ProcessCode, Process.XRAY_REWORK))
            {
                tbLoss.Visibility = Visibility.Collapsed;
                txtLossCnt.Visibility = Visibility.Collapsed;

                grdContents.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Auto);
                grdContents.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Auto);
                grdContents.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
                grdContents.ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Auto);
                grdContents.ColumnDefinitions[5].Width = new GridLength(2, GridUnitType.Star);
                grdContents.ColumnDefinitions[6].Width = new GridLength(1, GridUnitType.Auto);

                grdContents.ColumnDefinitions[7].Width = new GridLength(0);
                grdContents.ColumnDefinitions[8].Width = new GridLength(0);
                grdContents.ColumnDefinitions[9].Width = new GridLength(0);
            }
        }

        public void ClearShiftControl()
        {
            txtWorker.Text = string.Empty;
            txtWorker.Tag = string.Empty;
            txtShift.Text = string.Empty;
            txtShift.Tag = string.Empty;
            txtShiftStartTime.Text = string.Empty;
            txtShiftEndTime.Text = string.Empty;
            txtShiftDateTime.Text = string.Empty;
            
            if (txtLossCnt.Visibility == Visibility.Visible && tbLoss.Visibility == Visibility.Visible)
            {
                txtLossCnt.Text = string.Empty;
                txtLossCnt.Background = new SolidColorBrush(Colors.WhiteSmoke);
            }
        }
    }
}