/*************************************************************************************
 Created Date : 2021.04.29
      Creator : 김민석
   Decription : CELL 공급 프로젝트 조립 쪽 요청 포기 팝업
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Documents;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_079_CANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public bool bClick = false;
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_079_CANCEL()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                InitCombo();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            try
            {
                String[] sFiltercboAreaRslt = { "CELL_SPLY_RSPN_CNCL_RESN" };
                _combo.SetCombo(cboCancelReason, CommonCombo.ComboStatus.SELECT, sFilter: sFiltercboAreaRslt, sCase: "COMMCODE_WITHOUT_CODE");

            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Event
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bClick = false;

                if (bClick == false)
                {
                    bClick = true;
                    if (bClick == true)
                    {
                        this.DialogResult = MessageBoxResult.Cancel;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                //HiddenLoadingIndicator();

                bClick = false;
            }
        }

        private void btnDisclaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sSelectedCancelReason = ((System.Data.DataRowView)cboCancelReason.SelectedItem).Row.ItemArray[1].ToString();

                if (string.IsNullOrEmpty(cboCancelReason.SelectedValue.ToString()) || cboCancelReason.SelectedValue.ToString() == "SELECT")
                {
                    ms.AlertWarning("SFU1594"); //사유를 입력하세요.
                    return;
                }
                else
                {
                    this.DataContext = sSelectedCancelReason;
                    this.DialogResult = MessageBoxResult.OK;

                    this.Close();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        #endregion

        #region Mehod

        #endregion

        #region Grid

        #endregion

        private void cboCancelReason_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {

        }
    }
}
