/*************************************************************************************
 Created Date : 2022.12.22
      Creator : INS 정재홍
   Decription : Tool 비고 등록 및 이력
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.22 DEVELOPER : Initial Created [C20221128-000143]
  2023.12.06 정재홍    : E20231211-000182 - 신규 표준 공구 Tool ID 추가
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
    /// COM001_314_REMARK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_314_REMARK : C1Window, IWorkArea
    {
        #region [Variables & Constructor]
        private string _toolID;
        private string _remark;
        private string _StdtoolID;

        public COM001_314_REMARK()
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
            _remark = Util.NVC(parameters[1]);
            _StdtoolID = Util.NVC(parameters[2]);
        }

        private void InitControls()
        {
            txtToolID.Text = Util.NVC(_toolID);
            txtRemark.Text = Util.NVC(_remark);
            txtStdToolID.Text = Util.NVC(_StdtoolID);

            //해당 월 1일을 시작시점으로 설정
            dtpFrom.SelectedDateTime = DateTime.Today.AddDays(-1 * (DateTime.Today.Day - 1));
        }
        #endregion

        #region [Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitValueFromParent();
            InitControls();
            GetToolUsageHistory();
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
                
                inData.Columns.Add("TOOL_ID", typeof(string));
                inData.Columns.Add("FROM_DTTM", typeof(DateTime));
                inData.Columns.Add("TO_DTTM", typeof(DateTime));
                inData.Columns.Add("LANGID", typeof(string));

                DataRow row = inData.NewRow();
                row["TOOL_ID"] = _toolID;
                row["FROM_DTTM"] = dtpFrom.SelectedDateTime;
                row["TO_DTTM"] = dtpTo.SelectedDateTime;
                row["LANGID"] = LoginInfo.LANGID;
                inData.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_TOOL_INFO_HIST", "INDATA", "OUTDATA", inData, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                            throw exception;

                        Util.gridClear(dgToolHis);
                        Util.GridSetData(dgToolHis, result, this.FrameOperation);
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //저장 하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    GetToolRemark();
                }
            });
        }

        private void GetToolRemark()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("TOOL_ID", typeof(string));
                RQSTDT.Columns.Add("REMARKS", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["TOOL_ID"] = txtToolID.Text;
                dr["REMARKS"] = txtRemark.Text;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_REG_TOOL_INFO", "RQSTDT", null, RQSTDT, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    Util.AlertInfo("SFU1270");  //저장되었습니다.
                    GetToolUsageHistory();
                });
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
    }
}
