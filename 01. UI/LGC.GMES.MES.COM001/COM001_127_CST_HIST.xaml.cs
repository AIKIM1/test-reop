/*************************************************************************************
 Created Date : 2018.07.04
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 캐리어 이력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.04  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_127_CST_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_127_CST_HIST : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private string _CSTID = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_127_CST_HIST()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= C1Window_Loaded;

            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 1)
            {
                _CSTID = Util.NVC(tmps[0]);                
            }
            else
            {
                _CSTID = "";
            }

            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-30);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;
            
            GetActHistory();

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GetActHistory();
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
                GetActHistory();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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

        private void GetActHistory()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_CSTID)) return;

                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("FRDT", typeof(string));
                inTable.Columns.Add("TODT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CSTID"] = _CSTID;
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

                        Util.GridSetData(dgDetailList, searchRslt, FrameOperation, false);
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
