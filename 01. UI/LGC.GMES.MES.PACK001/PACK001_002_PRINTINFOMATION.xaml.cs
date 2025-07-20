/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack ID발행 화면 - 발행된 라벨ID 표시 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_002_PRINTINFOMATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_002_PRINTINFOMATION()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        txtPrintLot.Text = Util.NVC(dtText.Rows[0]["LOTID"]);
                        txtPrintScanid.Text = Util.NVC(dtText.Rows[0]["SCANID"]);
                        txtLotTitle.Text = ObjectDic.Instance.GetObjectName(Util.NVC(dtText.Rows[0]["LOTID_TITLE"]));
                        txtScanTitle.Text = ObjectDic.Instance.GetObjectName(Util.NVC(dtText.Rows[0]["SCANID_TITLE"]));

                        if (!(txtLotTitle.Text.Length > 0))
                        {
                            GRIDROW_LOTID.Height = new GridLength(0);
                            this.Height = this.Height - 50;
                        }
                        if (!(txtScanTitle.Text.Length > 0))
                        {
                            GRIDROW_SCANID.Height = new GridLength(0);
                            this.Height = this.Height - 50;
                        }
                    }
                }
                TimerSetting();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.IsEnabled = false;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        #region Mehod
        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);
            timer.IsEnabled = true;
        }
        #endregion


    }
}
