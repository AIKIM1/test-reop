/*************************************************************************************
 Created Date : 2018.06.26
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 캐리어 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.26  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_129.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_129 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        private bool bLoadHistTab = false;

        Util _Util = new Util();

        public COM001_129()
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
        private void Initialize()
        {
           
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSearch);
           
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-30);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;
            
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
            
            this.Loaded -= UserControl_Loaded;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();                               
        }

        private void txtCSTid_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    int cnt = 6;

                    if (txtCSTid.Text.Trim() != "")
                    {
                        this.Dispatcher.BeginInvoke(new Action(() => btnSearch_Click(null, null)));
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTid_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCSTid == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCSTid, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                if (txtCSTid.Text.Trim().Equals(""))
                {
                    Util.MessageValidation("SFU1244");
                    return;
                }

                GetActHistory(txtCSTid.Text);
                GetMappingHistory(txtCSTid.Text);
                GetCurrentStatus(txtCSTid.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        

        private void C1TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                C1.WPF.C1TabControl c1TabControl = sender as C1.WPF.C1TabControl;
                if (c1TabControl.IsLoaded)
                {
                    if (tbMappingHist.IsSelected)
                    {
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        #endregion

        #region Funct
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


        private void GetActHistory(string sCstID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sCstID)) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(string));
                inTable.Columns.Add("TODT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = sCstID;
                newRow["FRDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER_ACT_HIST", "INDATA", "OUTDATA", inTable, (searchRslt, searchException) => 
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgActHistList, searchRslt, FrameOperation, false);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        
        private void GetMappingHistory(string sCstID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sCstID)) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                //inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(string));
                inTable.Columns.Add("TODT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                //newRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString);
                newRow["CSTID"] = sCstID;
                newRow["FRDT"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                newRow["TODT"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER_LOT_MAPPING_HIST", "INDATA", "OUTDATA", inTable, (searchRslt, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgMappingList, searchRslt, FrameOperation, false);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetCurrentStatus(string sCstID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sCstID)) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = sCstID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER", "INDATA", "OUTDATA", inTable, (searchRslt, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgCurrentStatus, searchRslt, FrameOperation, false);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
