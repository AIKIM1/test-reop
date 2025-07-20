/*************************************************************************************
 Created Date : 2023.05.17
      Creator : 조영대
   Decription : 단적재 자동설정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.17  조영대 : Initial Created.
  2023.12.01  최석준 : cboInputEqpt 에서 GetList 호출하도록 변경, GetList() INDATA로 EQPTID 추가, 동별공통코드 추가
**************************************************************************************/


using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;


namespace LGC.GMES.MES.FCS001
{

    public partial class FCS001_149_AUTO_CONF : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        string SHIP_LOC = string.Empty;
        string sComTypeCode = "FORM_STACK_SET_INFO";

        public FCS001_149_AUTO_CONF()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length < 1) return;

            SHIP_LOC = Util.NVC(parameters[0]);

            InitCombo();

        }

        private void cboShipLoc_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            cboShipLoc.ClearValidation();
            SHIP_LOC = cboShipLoc.GetStringValue();

            if (!string.IsNullOrEmpty(SHIP_LOC))
            {
                string[] arrColum = { "LANGID", "AREAID", "COM_TYPE_CODE", "COM_CODE" };
                string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, sComTypeCode, SHIP_LOC };

                cboInputEqpt.ClearValidation();
                cboInputEqpt.SetDataComboItem("DA_SEL_SELECTOR_FOR_LOAD_ISS", arrColum, arrCondition, CommonCombo.ComboStatus.NONE);
            }
        }

        private void cboInputEqpt_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            string bufferQty = cboInputEqpt.GetStringValue("BUF_QTY");
            if (string.IsNullOrEmpty(bufferQty))
            {
                txtBufferCount.Text = "2";
            }
            else
            {
                txtBufferCount.Text = cboInputEqpt.GetStringValue("BUF_QTY");
            }

            if (!string.IsNullOrEmpty(cboInputEqpt.GetStringValue())) GetList();
            
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveStackShip();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgWorkLine_ExecuteCustomBinding(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (e.ResultData != null)
            {
                DataSet dsBind = e.ResultData as DataSet;

                // cboInputEqpt.DisplayMemberPath = "STACK_EQPTNAME";
                // cboInputEqpt.SelectedValuePath = "STACK_EQPTID";
                // cboInputEqpt.SetDataComboItem(dsBind.Tables["RET_STACK_SET"].Copy(), CommonCombo.ComboStatus.SELECT);

                if (dsBind.Tables.Count > 0 && dsBind.Tables.Contains("RET_STACK_SET") && 
                    dsBind.Tables["RET_STACK_SET"].Rows.Count > 0)
                {
                    // cboInputEqpt.SelectedValue = dsBind.Tables["RET_STACK_SET"].Rows[0]["STACK_EQPTID"];
                    txtBufferCount.Text = Util.NVC(dsBind.Tables["RET_STACK_SET"].Rows[0]["BUF_QTY"]);
                    cboShipMode.SelectedValue = dsBind.Tables["RET_STACK_SET"].Rows[0]["USE_FLAG"];
                }

                if (!dsBind.Tables["RET_TRGT"].Columns.Contains("CHK")) dsBind.Tables["RET_TRGT"].Columns.Add("CHK", typeof(bool));
                dsBind.Tables["RET_TRGT"].AsEnumerable().ToList().ForEach(f => f["CHK"] = f["USE_YN"].Equals("Y"));
                dgWorkLine.SetItemsSource(dsBind.Tables["RET_TRGT"], FrameOperation);
            }
        }

        private void dgWorkLine_ExecuteDataCompleted(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (e.ResultData != null)
            {
                
            }
        }
        #endregion

        #region Method
        private void InitCombo()
        {
            string[] arrColum = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            cboShipLoc.SetDataComboItem("DA_SEL_AUTO_STACK_SHIP_LOC", arrColum, arrCondition, CommonCombo.ComboStatus.SELECT, SHIP_LOC);

            cboShipMode.SetAreaCommonCode("FORM_STACK_SHIP_MODE", CommonCombo.ComboStatus.SELECT, false);
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SRCTYPE"] = "UI";
                dr["IFMODE"] = "OFF";
                dr["LANE_ID"] = SHIP_LOC;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQPTID"] = cboInputEqpt.GetBindValue();
                dtRqst.Rows.Add(dr);

                dgWorkLine.ExecuteService("BR_GET_STACK_SET", "INDATA", "OUTDATA,RET_STACK_SET,RET_TRGT,RET_SELECTOR_EQPTID", dtRqst, false, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveStackShip()
        {
            try
            {
                this.ClearValidation();

                bool isValidation = false;

                if (cboShipLoc.GetBindValue() == null)
                {
                    // 출고 위치를 입력해 주세요.
                    cboShipLoc.SetValidation("SFU4925", tbShipLoc.Text);
                    isValidation = true;
                }

                if (cboShipMode.GetBindValue() == null)
                {
                    // 적재출고 모드를 입력해 주세요.
                    cboShipMode.SetValidation("SFU4925", tbShipMode.Text);
                    isValidation = true;
                }

                if (cboInputEqpt.GetBindValue() == null)
                {
                    // 투입 설비를 입력해 주세요.
                    cboInputEqpt.SetValidation("SFU4925", tbInputEqpt.Text);
                    isValidation = true;
                }

                //if (cboBufferCount.GetBindValue() == null)
                //{
                //    // Buffer 수량을 입력해 주세요.
                //    cboBufferCount.SetValidation("SFU4925", tbBufferCount.Text);
                //    isValidation = true;
                //}

                if (!dgWorkLine.IsCheckedRow("CHK"))
                {
                    // 작업 가능 라인을 선택하세요.
                    Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("WORK_ABLE_LINE"));
                    isValidation = true;
                }

                if (isValidation) return;


                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LANE_ID", typeof(string));
                dtRqst.Columns.Add("USE_FLAG", typeof(string));
                dtRqst.Columns.Add("BUF_QTY", typeof(string));
                dtRqst.Columns.Add("TRGT_EQSGID", typeof(string));
                dtRqst.Columns.Add("USER_ID", typeof(string));
                dtRqst.Columns.Add("STACK_EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANE_ID"] = cboShipLoc.GetBindValue();
                dr["USE_FLAG"] = cboShipMode.GetBindValue();
                dr["BUF_QTY"] = txtBufferCount.Text;
                
                string eqsgList = string.Join(",", dgWorkLine.GetCheckedDataRow("CHK").Select(s => s["TRGT_EQSGID"]));
                dr["TRGT_EQSGID"] = eqsgList;

                dr["USER_ID"] = LoginInfo.USERID;
                dr["STACK_EQPTID"] = cboInputEqpt.GetBindValue();
                dtRqst.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_SET_STACK_SET", "INDATA", "OUTDATA", dtRqst);
                if (dtResult != null && dtResult.Rows.Count > 0 && dtResult.Rows[0]["RETVAL"].Equals("0"))
                {
                    this.DialogResult = MessageBoxResult.OK;
                    Util.MessageInfo("SFU1889");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }


        #endregion

    }
}
