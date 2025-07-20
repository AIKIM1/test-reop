/*************************************************************************************
 Created Date : 2021.12.07
      Creator : INS 김동일K
   Decription : C20211207-000117
--------------------------------------------------------------------------------------
 [Change History]
  2021.12.07  INS 김동일K : Initial Created.

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
    /// COM001_015_TEST_LOSS_PRV_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_015_TEST_LOSS_PRV_INFO : C1Window, IWorkArea
    {
        public COM001_015_TEST_LOSS_PRV_INFO()
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
            object[] tmps = C1WindowExtension.GetParameters(this);

            string sAreaID = string.Empty;
            string sEqsgID = string.Empty;
            string sProcID = string.Empty;
            string sEqptID = string.Empty;
            string sShftID = string.Empty;
            string sStDttm = string.Empty;
            string sEdDttm = string.Empty;

            DateTime sShftStDttm = DateTime.Now;
            DateTime sShftEdDttm = DateTime.Now;
            bool bShft = false;
 
            if (tmps.Length >= 9)
            {
                sAreaID = Util.NVC(tmps[0]);
                sEqsgID = Util.NVC(tmps[1]);
                sProcID = Util.NVC(tmps[2]);
                sEqptID = Util.NVC(tmps[3]);                
                sStDttm = Util.NVC(tmps[4]);
                sEdDttm = Util.NVC(tmps[5]);
                sShftID = Util.NVC(tmps[6]);

                if (DateTime.TryParse(Util.NVC(tmps[7]), out sShftStDttm) && DateTime.TryParse(Util.NVC(tmps[8]), out sShftEdDttm))
                {
                    bShft = true;
                }   
            }

            GetTestLossInfo(sAreaID, sEqsgID, sProcID, sEqptID, sShftID, sStDttm, sEdDttm, bShft, sShftStDttm, sShftEdDttm);
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void GetTestLossInfo(string sAreaID, string sEqsgID, string sProcID, string sEqptID, string sShftID, string sStDttm, string sEdDttm, bool bShft, DateTime sShftStDttm, DateTime sShftEdDttm)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("SHFTID", typeof(string));
                inDataTable.Columns.Add("FRDT", typeof(string));
                inDataTable.Columns.Add("TODT", typeof(string));
                inDataTable.Columns.Add("SHFT_STDT", typeof(DateTime));
                inDataTable.Columns.Add("SHFT_EDDT", typeof(DateTime));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = Util.NVC(sAreaID);
                newRow["PROCID"] = Util.NVC(sProcID).Equals("") ? null : sProcID;
                newRow["EQSGID"] = Util.NVC(sEqsgID).Equals("") ? null : sEqsgID;
                newRow["EQPTID"] = Util.NVC(sEqptID).Equals("") ? null : sEqptID;
                newRow["FRDT"] = sStDttm;
                newRow["TODT"] = sEdDttm;

                if (bShft)
                {
                    newRow["SHFTID"] = Util.NVC(sShftID).Equals("") ? null : sShftID;
                    newRow["SHFT_STDT"] = sShftStDttm;
                    newRow["SHFT_EDDT"] = sShftEdDttm;
                }
                
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPTLOSS_TEST_LOSS_PRV_INFO", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgList, searchResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}
