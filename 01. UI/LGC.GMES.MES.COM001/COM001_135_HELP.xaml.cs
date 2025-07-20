/*************************************************************************************
 Created Date : 2020.04.24
      Creator : INS 김동일K 
   Decription : 수불정보 이상 Data - 도움말 팝업 [C20200406-000377]
--------------------------------------------------------------------------------------
 [Change History]
  2020.04.24  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_135_HELP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_135_HELP : C1Window, IWorkArea
    {
        string _AREAID = string.Empty;

        public COM001_135_HELP()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps.Length >= 1)
                {
                    _AREAID = Util.NVC(tmps[0]);
                }
                
                GetHelpInfo();
                
                this.Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetHelpInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = _AREAID;              
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SBL_ABNORM_BAS_INFO", "INDATA", "OUTDATA", inTable, (searchRslt, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgList, searchRslt, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}
