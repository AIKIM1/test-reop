/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2020.12.10  김동일 : C20201104-000359 CWA 셀-팩 동간 Intransit 정합성 개선을 위한 셀 출고 기능 변경 건 관련 수정




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_011_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public BOX001_011_CONFIRM()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string sRCV_ISS_ID = string.Empty;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                txtShiptoName.Text = tmps[0] as string;
                txtToSlocName.Text = tmps[1] as string;
                txtModel.Text = tmps[2] as string;
                txtProdID.Text = tmps[3] as string;
                txtPalletQty.Value = int.Parse(tmps[4] as string);
                txtCellQty.Value = int.Parse(tmps[5] as string);

                if (tmps.Length > 6)
                {
                    bool bShipConfigMode = Util.NVC(tmps[6]).Equals("") ? false : (bool)tmps[6];

                    if (bShipConfigMode)
                    {
                        tbTitle.Text = ObjectDic.Instance.GetObjectName("SHIP_CONFIG_FINISH_MSG");
                        this.Header = ObjectDic.Instance.GetObjectName("SHIP_CONFIG");
                    }
                }
            }

            this.Loaded -= Window_Loaded;

        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //BOX001_CONFIRM wndConfirm = new BOX001_CONFIRM();
                //wndConfirm.FrameOperation = FrameOperation;
                //wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                //this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));  
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
              //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            BOX001_CONFIRM window = sender as BOX001_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        #endregion

        #region Mehod

        #endregion


    }
}
