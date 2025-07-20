/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : Tool 상태 변경 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Windows;
using System.Data;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_314_STAT_HIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_314_STAT_HIST : C1Window, IWorkArea
    {
        private string _Tool_ID = "";

        public COM001_314_STAT_HIST()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }

        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _Tool_ID = Util.NVC(tmps[0]);
            }
            else
            {
                _Tool_ID = "";
            }

            InitControls();

            GetStatChgHist();
        }

        private void InitControls()
        {
            //해당 월 1일을 시작시점으로 설정
            dtpFrom.SelectedDateTime = DateTime.Today.AddDays(-1 * (DateTime.Today.Day - 1));
        }

        private void btnSearch_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (dtpFrom.SelectedDateTime == null || dtpTo.SelectedDateTime == null)
                {
                    //선택된 날짜가 없습니다.
                    return;
                }

                if ((dtpTo.SelectedDateTime - dtpFrom.SelectedDateTime).TotalDays > 31)
                {
                    //기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                GetStatChgHist();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void GetStatChgHist()
        {
            try
            {
                if (_Tool_ID == "") return;

                DataTable inData = new DataTable();

                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));
                inData.Columns.Add("LANGID", typeof(string));

                DataRow row = inData.NewRow();
                row["TOOL_ID"] = _Tool_ID;
                row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                row["TO_DTTM"] = dtpTo.SelectedDateTime;
                row["LANGID"] = LoginInfo.LANGID;
                inData.Rows.Add(row);

                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgToolHist);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TOOL_INFO_STAT_CHG_HIST", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }
                                                
                        Util.GridSetData(dgToolHist, result, null, true);
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
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
