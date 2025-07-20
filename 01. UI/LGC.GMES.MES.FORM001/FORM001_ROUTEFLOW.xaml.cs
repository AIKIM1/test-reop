/*************************************************************************************
 Created Date : 2017.08.16
      Creator : 
   Decription : X-Ray 검사 공정 ROUTE FLOW 변경
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_ROUTEFLOW : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private bool _load = true;

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

        public FORM001_ROUTEFLOW()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
            }
        }

        private void InitializeUserControls()
        {
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[1] as string;
            txtRouteFlow.Text = tmps[4] as string;
            txtProcName.Text = tmps[5] as string;
            txtProcID.Text = tmps[6] as string;

            // 변경경로콤보 설정
            GetRouteFlowNextCombo();

            cboNXRouteFlow.SelectedValueChanged += cboNXRouteFlow_SelectedValueChanged;
        }
        #endregion

        #region [작업구분 변경 cboFormWorkType_SelectedValueChanged]
        private void cboNXRouteFlow_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable dt = DataTableConverter.Convert(cboNXRouteFlow.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                txtNXProcName.Text = "";
                txtNXProcID.Text = "";
                return;
            }

            txtNXProcName.Text = dt.Rows[cboNXRouteFlow.SelectedIndex]["PROCNAME_TO"].ToString();
            txtNXProcID.Text = dt.Rows[cboNXRouteFlow.SelectedIndex]["PROCID_TO"].ToString();
        
        }
        #endregion

        #region [변경]
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateChange())
                return;

            // 변경하시겠습니까?
            Util.MessageConfirm("SFU2875", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RouteFlowChange();
                }
            });

        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]

        #region 변경경로콤보 설정
        private void GetRouteFlowNextCombo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = _procID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ROUTEPATH_FO", "INDATA", "OUTDATA", inTable);

                DataRow dr = dtResult.NewRow();
                dr[cboNXRouteFlow.SelectedValuePath] = "SELECT";
                dr[cboNXRouteFlow.DisplayMemberPath] = "- SELECT -";
                dtResult.Rows.InsertAt(dr, 0);

                cboNXRouteFlow.ItemsSource = dtResult.Copy().AsDataView();
                cboNXRouteFlow.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        /// <summary>
        /// 경로 변경
        /// </summary>
        private void RouteFlowChange()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_PRD_REG_TB_SFC_PROC_FLOW_SET";

                DataTable dt = DataTableConverter.Convert(cboNXRouteFlow.ItemsSource);

                // DATA Table
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("ROUTID", typeof(string));
                inTable.Columns.Add("FLOWID", typeof(string));
                inTable.Columns.Add("TO_FLOWID", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["ROUTID"] = dt.Rows[cboNXRouteFlow.SelectedIndex]["ROUTID"].ToString();
                newRow["FLOWID"] = dt.Rows[cboNXRouteFlow.SelectedIndex]["FLOWID"].ToString();
                newRow["TO_FLOWID"] = dt.Rows[cboNXRouteFlow.SelectedIndex]["FLOWID_TO"].ToString();
                newRow["USE_FLAG"] = "Y";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                });


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
        #endregion

        #region[[Validation]
        private bool ValidateChange()
        {
            if (string.IsNullOrWhiteSpace(txtNXProcName.Text) || txtNXProcName.Equals("SELECT"))
            {
                // 변경내용이 없습니다.
                Util.MessageValidation("SFU1226");
                return false;
            }

            if (txtNXProcID.Text == txtProcID.Text)
            {
                // 현재설정결로와 변경경로가 동일 합니다.
                Util.MessageValidation("SFU4032");
                return false;
            }

            return true;
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





        #endregion

        #endregion

    }
}
