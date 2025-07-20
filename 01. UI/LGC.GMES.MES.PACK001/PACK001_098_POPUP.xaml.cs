/*************************************************************************************
 Created Date : 2023.05.17
      Creator : 김진수
   Decription : 착/완공에 대한 결과 Popup창
--------------------------------------------------------------------------------------
 [Change History]
  2023.06.02  김진수S : Initial Created.
  2023.06.30  김진수S : 최신버전 재 업로드
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
    public partial class PACK001_098_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_098_POPUP()
        {
            InitializeComponent();
        }
        #endregion
        
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
                        txtTitle.Text = Util.NVC(dtText.Rows[0]["TITLE"]);// ObjectDic.Instance.GetObjectName();
                        txtLotID.Text = Util.NVC(dtText.Rows[0]["LOTID"]);

                        //if (!(txtLotTitle.Text.Length > 0))
                        //{
                        //    GRIDROW_LOTID.Height = new GridLength(0);
                        //    this.Height = this.Height - 50;
                        //}
                        //if (!(txtScanTitle.Text.Length > 0))
                        //{
                        //    GRIDROW_SCANID.Height = new GridLength(0);
                        //    this.Height = this.Height - 50;
                        //}
                    }
                }
                TimerSetting();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region Mehod
        private void TimerSetting()
        {
            timer.Interval = new TimeSpan(0, 0, 0, 3);
            timer.Tick += new EventHandler(timer_Tick);
            timer.IsEnabled = true;
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


    }
}
