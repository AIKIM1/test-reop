/*************************************************************************************
 Created Date : 2020.09.18
      Creator : 안인효
   Decription : 라벨 발행 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.18  안인효 : Initial Created.   
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_334.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_334 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        
        DataTable dtMain = new DataTable();

        public COM001_334()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
            dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime.AddDays(-7);
        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
        }

        private void dtp_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                if (sender == dtpTo)
                {
                    dtpFrom.SelectedDateTime = dtpTo.SelectedDateTime;
                }
                else
                {
                    dtpTo.SelectedDateTime = dtpFrom.SelectedDateTime;
                }
            }
        }

        private void txtLotId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidationSearch())
                {
                    return;
                }

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("ACTFROM", typeof(string));
                inTable.Columns.Add("ACTTO", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                if (txtLotId.Text.Trim() == string.Empty)
                {
                    newRow["LOTID"] = null;
                    newRow["ACTFROM"] = dtpFrom.SelectedDateTime.ToString("yyyyMMdd");
                    newRow["ACTTO"] = dtpTo.SelectedDateTime.AddDays(1).ToString("yyyyMMdd");
                }
                else
                {
                    newRow["LOTID"] = Util.NVC(txtLotId.Text.Trim());
                    newRow["ACTFROM"] = null;
                    newRow["ACTTO"] = null;
                }
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_LABEL_PRT_HISTORY", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgList, searchResult, FrameOperation, false);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private bool ValidationSearch()
        {
            if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            return true;
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

        #endregion

    }
}
