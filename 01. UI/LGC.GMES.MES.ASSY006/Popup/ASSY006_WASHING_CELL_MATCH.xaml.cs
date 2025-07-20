/*************************************************************************************
 Created Date : 2023.11.30
      Creator : 배현우
   Decription : 오창 2산단 NFF MP 라인 구축 - CANID or VentID  바코드 리딩검사 
--------------------------------------------------------------------------------------
 [Change History]
  2023.11.30  배현우 : Initial Created.
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary; 
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY006.Popup
{
    /// <summary>
    /// ASSY006_WASHING_CELL_MATCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY006_WASHING_CELL_MATCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor         

        private string _previewQtyValue = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private bool _load = true;

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY006_WASHING_CELL_MATCH()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_load)
                {
                    SetControl();
                    _load = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControl()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (!Util.NVC(tmps[0]).Equals(""))
                {
                    txtCanID.Text = Util.NVC(tmps[0]);
                }

                if (!Util.NVC(tmps[1]).Equals(""))
                {
                    txtVentID.Text = Util.NVC(tmps[1]);
                }
                txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfirmMatch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void ConfirmMatch()
        {
            try
            {
                ShowLoadingIndicator();
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (txtCanID.Text == null  || txtCanID.Text == "")
                {
                    if (txtVentID.Text == null || txtVentID.Text == "")
                    {
                        // 비교 할 DATA가 없습니다.
                        Util.MessageValidation("SFU4579");
                        return;
                    }
                }

                
                if (txtCanID.Text.Equals(txtBarcode.Text))
                {
                    Util.MessageInfo("SFU4577"); //CANID가 일치합니다.
                }
                else if (txtVentID.Text.Equals(txtBarcode.Text))
                {
                    Util.MessageInfo("SFU4578");// VENTID가 일치합니다.
                }
                else
                {
                    string msg = string.Empty;
                    msg += Util.NVC(tmps[2]);
                    if (Util.NVC(tmps[3]) != "" && !string.IsNullOrEmpty(Util.NVC(tmps[3])))
                        msg += ", " + tmps[3];
                    if (Util.NVC(tmps[4])!= "" && !string.IsNullOrEmpty(Util.NVC(tmps[4])))
                        msg += ", " + tmps[4];

                    AlertMessage("SFU4576", msg);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        #region [Validation]


        #endregion

        #region [Func]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }





        #endregion

        #endregion

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.LeftCtrl)
                {
                    Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
                    return;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void txtBarcode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
            txtBarcode.IsEnabled = false;
            txtBarcode.IsEnabled = true;
            return;
        }

        private void txtBarcode_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
            txtBarcode.IsEnabled = false;
            txtBarcode.IsEnabled = true;
            return;

        }

        private void AlertMessage(string msg,string tok)
        {
            Util.MessageValidation(msg, tok);
            return;
        }
    }
}
