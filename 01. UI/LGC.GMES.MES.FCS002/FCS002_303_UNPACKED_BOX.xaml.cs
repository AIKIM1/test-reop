/*************************************************************************************
 Created Date : 2024.02.07
      Creator : 이홍주
   Decription : 소형 미포장박스 조회 POPUP 생성
--------------------------------------------------------------------------------------
 [Change History]
  2024.02.07  이홍주 : NFF 양산라인 최초생성
  

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;


namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_303_UNPACKED_BOX : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        public FCS002_303_UNPACKED_BOX()
        {
            InitializeComponent();
            Loaded += FCS002_303_UNPACKED_BOX_Loaded;
        }

        private void FCS002_303_UNPACKED_BOX_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

   
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

     
    
        private void GetCellList(string pCellID)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["SUBLOTID"] = pCellID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_MB", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    dtSource.Merge(dtResult);
                    Util.gridClear(dgPalletInfo);
                    Util.GridSetData(dgPalletInfo, dtSource, FrameOperation, true);
                }

                if(dgPalletInfo.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU2951");    //조회결과가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
        
       
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSearchData();
        }

        #endregion

        #region Mehod

        private void GetSearchData()
        {

            try
            {
                DataTable RQSTDT = new DataTable("");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("BOXTYPE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXTYPE"] = "TN_INBOX,T_OUTBOX";
                dr["BOXID"] = string.IsNullOrEmpty(txtoutbox.Text) ? "": txtoutbox.Text;
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNPACKED_BOX_MB", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtoutbox.Text = string.Empty;
            }
        }
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

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgPalletInfo);

            txtoutbox.Text = string.Empty;
            txtoutbox.SelectAll();
            txtoutbox.Focus();
        }

        #endregion



    }
}
