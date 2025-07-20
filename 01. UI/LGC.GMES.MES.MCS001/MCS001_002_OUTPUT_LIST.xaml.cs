/*************************************************************************************
 Created Date : 2019.07.01
      Creator : 오화백
   Decription : 수동출고 리스트
--------------------------------------------------------------------------------------
  
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_002_OUTPUT_LIST : C1Window, IWorkArea
	{

        #region Declaration & Constructor 
          
        public MCS001_002_OUTPUT_LIST() {
			InitializeComponent();
		}
		public IFrameOperation FrameOperation {
			get;
			set;
		}
		private void C1Window_Loaded( object sender, RoutedEventArgs e ) {
			ApplyPermissions();
            InitCombo();
            this.Loaded -= C1Window_Loaded;

        }
        public bool IsUpdated;
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions() {
			List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOutPut);
            Util.pageAuth( listAuth, FrameOperation.AUTHORITY );
		}
        /// <summary>
        /// 콤보박스 
        /// </summary>
        private void InitCombo()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //설비 정보 조회
                 _combo.SetCombo(cboEqp, CommonCombo.ComboStatus.SELECT,  sCase: "CWAMCSEQUIPMENT_MEB");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Event

        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region 조회 : btnSearch()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch(object sender, RoutedEventArgs e)
        {
            SeachData();
        }
        #endregion

        #region 설비 콤보 이벤트 : cboEqp_SelectedItemChanged()
        /// <summary>
        /// 설비 콤보 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboEqp_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboEqp.Items.Count > 0 && cboEqp.SelectedValue != null && !cboEqp.SelectedValue.Equals(""))
            {
                if (cboEqp.SelectedIndex == 0)
                {
                    //워크오더
                    txtWorkOrder.Text = string.Empty;
                    txtWorkOrder.Tag = null;
                    //자재
                    txtMtrl.Text = string.Empty;
                    txtMtrl.Tag = null;
                    //목적지
                    txtPort.Text = string.Empty;
                    txtPort.Tag = null;
                }
                else
                {
                    WorkOredr(); //워크오더
                    Mtrl();   //자재
                    Port();  //목적지
                }

            }
            else
            {
                cboEqp.SelectedIndex = 0;

                //워크오더
                txtWorkOrder.Text = string.Empty;
                txtWorkOrder.Tag = null;
                //자재
                txtMtrl.Text = string.Empty;
                txtMtrl.Tag = null;
                //목적지
                txtPort.Text = string.Empty;
                txtPort.Tag = null;
            }
        }
        #endregion

        #region 수동출고 처리 : btnOutPut_Click()
        private void btnOutPut_Click(object sender, RoutedEventArgs e)
        {
            if (!OutPutaveValidation())
            {
                return;
            }
            //저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    OutPut();
                }
            });
        }
        #endregion

        #endregion

        #region Mehod

        #region 조회 : SeachData()
        /// <summary>
        /// 조회
        /// </summary>
        private void SeachData()
        {
            loadingIndicator.Visibility = Visibility.Visible;
            DataTable RQSTDT = new DataTable("RQSTDT");
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("MTRLID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = cboEqp.SelectedValue.ToString() == string.Empty ? null : cboEqp.SelectedValue;
            dr["MTRLID"] = txtMtrl.Tag == null ? null : txtMtrl.Tag.ToString();

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteService("DA_MCS_SEL_MEB_OUTPUT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (result, exception) => {
                try
                {
                    if (exception != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(exception);
                        return;
                    }

                    Util.GridSetData(dgList, result, FrameOperation, true);
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

        #endregion

        #region  WOID 셋팅 : WorkOredr()
        /// <summary>
        /// WOID 셋팅
        /// </summary>
        private void WorkOredr()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["EQPTID"] = cboEqp.SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_WORKORDER", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult != null && dtResult.Rows.Count >0)
            {
                txtWorkOrder.Text = dtResult.Rows[0]["CBO_NAME"].ToString();
                txtWorkOrder.Tag = dtResult.Rows[0]["CBO_CODE"].ToString();
            }
            else
            {
                txtWorkOrder.Text = string.Empty;
                txtWorkOrder.Tag = null;
            }
        }
        #endregion
        
        #region 자재 셋팅 :   Mtrl()
        /// <summary>
        /// 자재 셋팅
        /// </summary>
        private void Mtrl()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANG", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANG"] = LoginInfo.LANGID;
            dr["EQPTID"] = cboEqp.SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_MTRL", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                txtMtrl.Text = dtResult.Rows[0]["CBO_NAME"].ToString();
                txtMtrl.Tag = dtResult.Rows[0]["CBO_CODE"].ToString();
            }
            else
            {
                txtMtrl.Text = string.Empty;
                txtMtrl.Tag = null;
            }
        }
        #endregion

        #region 목적지 Port 셋팅 : Port()
        /// <summary>
        /// 목적지 Port 셋팅
        /// </summary>
        private void Port()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANG", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANG"] = LoginInfo.LANGID;
            dr["EQPTID"] = cboEqp.SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_MEB_SEL_PORT", "RQSTDT", "RSLTDT", RQSTDT);
            if (dtResult != null && dtResult.Rows.Count > 0)
            {
                txtPort.Text = dtResult.Rows[0]["CBO_NAME"].ToString();
                txtPort.Tag = dtResult.Rows[0]["CBO_CODE"].ToString();
            }
            else
            {
                txtPort.Text = string.Empty;
                txtPort.Tag = null;
            }
        }
        #endregion

        #region 수동출고 명령 : OutPut()
        /// <summary>
        /// 출고명령 생성
        /// </summary>
        private void OutPut()
        {

            try
            {
                const string bizRuleName = "BR_MCS_REG_LOGIS_CMD_MGV";

                DataSet ds = new DataSet();
                DataTable inDataTable = ds.Tables.Add("INLOT");
                inDataTable.Columns.Add("LOTID", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True")
                    {
                        DataRow dr = inDataTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(row.DataItem, "LOTID");
                        inDataTable.Rows.Add(dr);
                    }
                }
                DataTable inCst = ds.Tables.Add("INDATA");
                inCst.Columns.Add("FROM_PORT_ID", typeof(string));
                inCst.Columns.Add("TO_PORT_ID", typeof(string));
                inCst.Columns.Add("UPDUSER", typeof(string));

                foreach (C1.WPF.DataGrid.DataGridRow row in dgList.Rows)
                {
                    if (row.Type == DataGridRowType.Item && DataTableConverter.GetValue(row.DataItem, "CHK").GetString() == "True")
                    {
                        DataRow drData = inDataTable.NewRow();
                        drData["FROM_PORT_ID"] = DataTableConverter.GetValue(row.DataItem, "PORT_ID");
                        drData["TO_PORT_ID"] = txtPort.Tag.ToString();
                        drData["UPDUSER"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(drData);
                    }
                }

                new ClientProxy().ExecuteService_Multi(bizRuleName, "INLOT,INDATA", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                        IsUpdated = true;

                        // 재조회 처리
                        SeachData();
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                }, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 출고 명령 Validation : OutPutaveValidation()
        /// <summary>
        ///  출고 명령생성 Validation
        /// </summary>
        /// <returns></returns>
        private bool OutPutaveValidation()
        {
            if (cboEqp.SelectedValue.ToString() == string.Empty)
            {
                Util.AlertInfo("SFU1673"); //"설비정보를 선택하세요."
                return false;
            }
            if (txtPort.Text == string.Empty && txtPort.Tag == null)
            {
                Util.AlertInfo("SFU7024"); //"목적지 정보가 없습니다."
                return false;
            }
            if (dgList.Rows.Count == 0)
            {
                Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                return false;
            }
            DataRow[] drchk = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 1");
            if (drchk.Length == 0)
            {
                // 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651");
                return false;
            }

            return true;
        }

        #endregion

        #endregion

    }
}