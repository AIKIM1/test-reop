/*************************************************************************************
 Created Date : 2020.06.03
      Creator : INS 김동일K
   Decription : 조립 공정진척 화면 - 활성화 트레이 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.03  INS 김동일K : Initial Created. [C20200602-000207]
  
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_PKG_TRAY_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_PKG_TRAY_INFO : C1Window, IWorkArea
    {
        public CMM_PKG_TRAY_INFO()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            Loaded -= C1Window_Loaded;
            txtTrayID.Focus();
        }

        private void txtTrayID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetTrayInfo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtTrayID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetTrayInfo();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void InitializeControls()
        {
            Util.gridClear(dgInfo);
        }

        private void GetTrayInfo()
        {
            try
            {
                if (txtTrayID.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU4975"); // TRAY ID를 입력하세요.
                    return;
                }

                InitializeControls();

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = txtTrayID.Text.Trim();
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_PKG_INFO_OUTLOT", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        
                        Util.GridSetData(dgInfo, bizResult, FrameOperation, true);                        
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
