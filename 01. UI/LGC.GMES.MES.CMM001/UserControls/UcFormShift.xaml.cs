using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.UserControls
{
    /// <summary>
    /// UC_SHIFT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcFormShift
    {
        public TextBox TextShift { get; set; }
        public TextBox TextShiftDateTime { get; set; }
        public TextBox TextWorker { get; set; }
        public TextBox TextShiftStartTime { get; set; }
        public TextBox TextShiftEndTime { get; set; }

        public Button ButtonShift { get; set; }

        public UcFormShift()
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
            ButtonShift = btnShift;
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
        }
    }
}