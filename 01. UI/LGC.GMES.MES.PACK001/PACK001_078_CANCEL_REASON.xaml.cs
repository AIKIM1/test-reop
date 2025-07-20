/*************************************************************************************
 Created Date : 2021.03.22
      Creator : 김민석
   Decription : CELL 공급 프로젝트 PACK 팝업 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using C1.WPF.DataGrid;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Documents;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_078_CANCEL_REASON : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public bool bClick = false;
        string strAreaID = string.Empty;
        string strMtrlID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        public PACK001_078_CANCEL_REASON()
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
                if (tmps != null)
                {

                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //string sCancelReason = sender as string;
                //C1DataGrid dg = sender as C1DataGrid;
                //RichTextBox rtb = sender as RichTextBox;

                TextRange textRange = new TextRange(txtCancelReason.Document.ContentStart, txtCancelReason.Document.ContentEnd);

                if (textRange.Text.Equals("\r\n") || textRange.Text.Equals("") || textRange.Text.Trim().Equals(""))
                {
                    ms.AlertWarning("SFU1594"); //사유를 입력하세요.
                    return;
                }else
                {
                    this.DataContext = textRange;
                    this.DialogResult = MessageBoxResult.OK;

                    this.Close();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }


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

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Mehod

        #region Refresh
        private void Refresh()
        {
            try
            {
                //그리드 clear
                //Util.gridClear(dgCellReq);
                //Util.gridClear(dgCellConf);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Refresh

        #endregion

        #region Grid
        private void dgCellReq_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                //2020.06.26
                C1DataGrid dataGrid = (sender as C1DataGrid);

                Action act = () =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                };


            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator();

                //bClick = false;
            }
        }
        #endregion
    }
}
