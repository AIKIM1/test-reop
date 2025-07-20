/*************************************************************************************
 Created Date : 2016.10.29
      Creator : Jeong Hyeon Sik
   Decription : Pack ID발행 화면 - 자동발행선택시 팝업으로 라벨발행여부 체크하도록
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.
 
**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_002_PRINT_YN_SELECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private bool bPrintYnFlag = false;
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public bool PRINTYN_FLAG
        {
            get
            {
                return bPrintYnFlag;
            }

            set
            {
                bPrintYnFlag = value;
            }
        }

        public PACK001_002_PRINT_YN_SELECT()
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
                TEXT1.Text = MessageDic.Instance.GetMessage("SFU1540"); //바코드를발행하시겠습니까?
                TEXT2.Text = MessageDic.Instance.GetMessage("SFU3603"); //※주의! 현장라벨발행PC인경우발행체크하세요.
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkPrint_Y_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkPrint_Y.IsChecked)
                {
                    chkPrint_N.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void chkPrint_Y_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(bool)chkPrint_Y.IsChecked)
                {
                    chkPrint_N.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void chkPrint_N_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((bool)chkPrint_N.IsChecked)
                {
                    chkPrint_Y.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void chkPrint_N_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!(bool)chkPrint_N.IsChecked)
                {
                    chkPrint_Y.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PRINTYN_FLAG = (bool)chkPrint_Y.IsChecked ? true : false;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Mehod

        #endregion


    }
}
