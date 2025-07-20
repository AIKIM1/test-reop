/*************************************************************************************
 Created Date : 2019.09. 25
      Creator : LG CNS 김대근
   Decription : 금형사용 상세이력 화면
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.25  LG CNS 김대근 : 금형사용 상세이력 화면
  2023.07.13  김도형          [E20230626-000053] [생산PI] Tool ID 사용이력 및 연마 관리 내 수정 요청
  2023.12.06  정재홍        : E20231211-000182 - 신규 표준 공구 Tool ID 추가
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_314_DETAIL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_314_DETAIL : C1Window, IWorkArea
    {
        #region [Variables & Constructor]
        private string _toolID;
        private string _StdtoolID;

        public COM001_314_DETAIL()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get; set;
        }
        #endregion

        #region [Init Method]
        private void InitValueFromParent()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            int paramLen = 1;

            if (parameters == null || parameters.Length < paramLen)
                return;

            _toolID = Util.NVC(parameters[0]);
            _StdtoolID = Util.NVC(parameters[1]);
        }

        private void InitControls()
        {
            tbToolID.Text = Util.NVC(_toolID);
            tbStdToolID.Text = Util.NVC(_StdtoolID);

            //해당 월 1일을 시작시점으로 설정
            dtpFrom.SelectedDateTime = DateTime.Today.AddDays(-1 * (DateTime.Today.Day - 1));
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitValueFromParent();
            InitControls();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if(dtpFrom.SelectedDateTime == null || dtpTo.SelectedDateTime == null)
            {
                //선택된 날짜가 없습니다.
                return;
            }

            if (string.IsNullOrEmpty(_toolID))
            {
                //toolID가 없습니다.
                return;
            }

            ShowLoadingIndicator();
            GetToolUsageHistory();
        }
        #endregion

        #region [BizCall]
        private void GetToolUsageHistory()
        {
            try
            {
                DataTable inData = new DataTable();
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("FROM_DT", typeof(DateTime));
                inData.Columns.Add("TO_DT", typeof(DateTime));
                inData.Columns.Add("PRCS_TYPE_CODE", typeof(string));

                DataRow row = inData.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["TOOL_ID"] = _toolID;
                row["FROM_DT"] = dtpFrom.SelectedDateTime;
                row["TO_DT"] = dtpTo.SelectedDateTime;
                inData.Rows.Add(row);

                //[E20230626-000053] [생산PI] Tool ID 사용이력 및 연마 관리 내 수정 요청
                new ClientProxy().ExecuteService("DA_PRD_SEL_TOOL_USAGE_HIST_DETL_UI", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgToolUsageHistory);
                        Util.GridSetData(dgToolUsageHistory, result, this.FrameOperation);
                    }
                    catch(Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Util Method]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Visible)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator.Visibility != Visibility.Collapsed)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}
