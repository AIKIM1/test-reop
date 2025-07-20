/*************************************************************************************
 Created Date : 2024.03.13
      Creator : 최동훈
   Decription : 활성화 포장 재작업 Remark 입력 Popup 생성
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.13  최초착성.
 
**************************************************************************************/

using System.Data;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_303_REMARK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _Remark = string.Empty;
        private string _Area = string.Empty;

        public BOX001_303_REMARK()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string Remark
        {
            get { return _Remark; }
        }

        public string Area
        {
            get { return _Area; }
        }

        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            if (tmps.Length > 0)
            {
                _Area = Convert.ToString(tmps[0]);
            }

        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtRemark.Text == null || string.IsNullOrEmpty(txtRemark.Text.Trim()))
                {
                    ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1590"), null, "Info", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    //비고를 입력하세요.
                    //Util.MessageValidation("SFU1590");
                    return;
                }

                // 저장하시겠습니까?
                //Util.MessageConfirm("SFU2070", (result) =>
                //{
                //    if (result == MessageBoxResult.OK)
                //    {
                //        _Remark = txtRemark.Text;
                //        this.DialogResult = MessageBoxResult.OK;
                //        this.Close();
                //    }
                //});

                _Remark = txtRemark.Text;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCencel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #endregion


        #region Mehod

        #endregion
    }
}
