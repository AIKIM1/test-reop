/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 자재입고 화면 - 입고정보 변경 팝업
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
    public partial class PACK001_022_RETURN_PALLETINFO : C1Window, IWorkArea
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

        public PACK001_022_RETURN_PALLETINFO()
        {
            InitializeComponent();
        }

        Util util = new Util();

        
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
                    string sPalletId = Util.NVC(tmps[0]);
                    string sTrayId = Util.NVC(tmps[1]);
                    if (!string.IsNullOrEmpty(sTrayId))
                    {
                        this.tbTray.Visibility = Visibility.Visible;  
                        this.txtTray.Visibility = Visibility.Visible;
                        this.txtTray.Text = sTrayId;
                    }
                    else
                    {
                        this.tbTray.Visibility = Visibility.Collapsed;
                        this.txtTray.Visibility = Visibility.Collapsed;
                        this.txtTray.Text = string.Empty;
                    }

                    this.txtPallet.Text = sPalletId;
                    if (txtPallet.Text.Length > 0)
                    {
                        getPalletInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnEcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //if (txtPallet.Text.Length > 0)
                //{
                //    getPalletInfo();
                //}
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if(e.Key == Key.Enter)
                {
                    if (txtPallet.Text.Length > 0)
                    {
                        getPalletInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Mehod
        
        private void getPalletInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = txtPallet.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_ID_DTL", "RQSTDT", "RSLTDT", RQSTDT);

                dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch(Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RETURN_ID_DTL", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }

        }

        #endregion


    }
}
