/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면 - 입고정보선택 확인 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_019_RECEIVEPRODUCT_SELECTCHECK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private DataSet dsIndataParam = new DataSet();

        public PACK001_019_RECEIVEPRODUCT_SELECTCHECK()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            DataTable dtText = tmps[0] as DataTable;
            dsIndataParam = tmps[1] as DataSet;
            if (dtText.Rows.Count > 0)
            {
                txtModelName.Text = Util.NVC(dtText.Rows[0]["MODELNAME"]);
                txtProductName.Text = Util.NVC(dtText.Rows[0]["PRODID"]);
                txtRouteName.Text = Util.NVC(dtText.Rows[0]["ROUTENAME"]);
                //txtLotType.Text = Util.NVC(dtText.Rows[0]["LOTTYPE"]);
            }
            chkProduct.IsChecked = true;
        }
        private void C1Window_Closed(object sender, EventArgs e)
        {
            //loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if((bool)chkModel.IsChecked && (bool)chkProduct.IsChecked && (bool)chkRoute.IsChecked)// && (bool)chkLotType.IsChecked)
                {
                    //setWarehousing();

                    this.DialogResult = MessageBoxResult.OK;

                    this.Close();

                }
                else
                {
                    //입고 정보를 확인 후 체크 표시 하세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1797"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                txtModelName.Style = (Style)FindResource("Content_InputForm_LabelStyle");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            
            try
            {
                txtModelName.Style = (Style)FindResource("Content_InputForm_TextBlockStyle");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkProduct_Checked(object sender, RoutedEventArgs e)
        {
            
            try
            {           
                if(txtProductName!=null)
                {
                    txtProductName.Style = (Style)FindResource("Content_InputForm_LabelStyle");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void chkProduct_Unchecked(object sender, RoutedEventArgs e)
        {
            
            try
            {
                if (txtProductName != null)
                {
                    txtProductName.Style = (Style)FindResource("Content_InputForm_TextBlockStyle");
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkRoute_Checked(object sender, RoutedEventArgs e)
        {
            
            try
            {
                txtRouteName.Style = (Style)FindResource("Content_InputForm_LabelStyle");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkRoute_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                txtRouteName.Style = (Style)FindResource("Content_InputForm_TextBlockStyle");
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtProductName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)chkProduct.IsChecked)
                {
                    chkProduct.IsChecked = false;
                }
                else
                {
                    chkProduct.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtModelName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)chkModel.IsChecked)
                {
                    chkModel.IsChecked = false;
                }
                else
                {
                    chkModel.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtRouteName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((bool)chkRoute.IsChecked)
                {
                    chkRoute.IsChecked = false;
                }
                else
                {
                    chkRoute.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void setWarehousing()
        {
            try
            {
                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_PACK", "INDATA,RCV_ISS", "OUTDATA", dsIndataParam, null);
                

                if (dsResult != null && dsResult.Tables.Count > 0)
                {

                    if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                    {
                        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            //PALLET을입고하였습니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1412"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.DialogResult = MessageBoxResult.OK;

                                this.Close();
                            });

                            
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_REG_RECEIVE_PRODUCT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }


        //private void chkLotType_Checked(object sender, RoutedEventArgs e)
        //{
        //    txtLotType.Style = (Style)FindResource("Content_InputForm_LabelStyle");
        //}

        //private void chkLotType_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    txtLotType.Style = (Style)FindResource("Content_InputForm_TextBlockStyle");
        //}

        #endregion

        #region Mehod

        #endregion


    }
}
