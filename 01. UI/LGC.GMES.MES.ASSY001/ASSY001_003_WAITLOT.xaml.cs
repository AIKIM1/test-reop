/*************************************************************************************
 Created Date : 2017.12.12
      Creator : 이진선
   Decription : VD예약 - 대기list조회
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_003_WAITLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_003_WAITLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID;
        private string _Unit;
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
        public ASSY001_003_WAITLOT()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _LineID = Util.NVC(tmps[0]);
                _Unit = Util.NVC(tmps[1]);
            }


            if (!_Unit.Equals("LOT"))
            {
                SKIDID.Visibility = Visibility.Visible;
            }
            else
            {
                SKIDID.Visibility = Visibility.Collapsed;
            }

            ApplyPermissions();

            GetWaitLot();
        }

        
        private void rdoAllType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoCType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void rdoAType_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            if ((sender as RadioButton).IsChecked.HasValue && (bool)(sender as RadioButton).IsChecked)
                GetWaitLot();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                GetWaitLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetWaitLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetWaitLot()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ELEC", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _LineID;
                dr["PROCID"] = Process.VD_LMN;
                dr["ELEC"] = rdoAllType.IsChecked == true ? null : rdoAType.IsChecked == true ? "A" : "C";
                dr["LOTID"] = txtWaitPancakeLot == null ? null : txtWaitPancakeLot.Text.Equals("") ? null : txtWaitPancakeLot.Text;

                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RESERVE_LOT_LIST", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, searchResult, null, true);

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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btn);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #endregion

   




    }
}
