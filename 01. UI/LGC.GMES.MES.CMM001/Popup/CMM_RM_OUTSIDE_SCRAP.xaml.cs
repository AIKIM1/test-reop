/*************************************************************************************
 Created Date : 2024.05.07
      Creator : 신광희
   Decription : 최외각 폐기 팝업 (롤맵 팝업)  CMM_ROLLMAP_OUTSIDE_SCRAP 참조 변경된 BizRule 적용 필요 (코터 롤맵 화면에서 호출 함.)
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.07  : Initial Created.
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_RM_OUTSIDE_SCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public bool IsUpdated { get; set; }

        private string _processCode;
        private string _equipmentCode;
        private string _lotId;
        private string _wipSeq;
        private string _equipmentMeasurementCode;

        public CMM_RM_OUTSIDE_SCRAP()
        {
            InitializeComponent();
        }

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

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitializeControl()
        {
            InitializeComboBox();
        }

        private void InitializeComboBox()
        {
            SetFaultytypeCombo(cboFaultytype);
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                _processCode = Util.NVC(parameters[0]);
                _equipmentCode = Util.NVC(parameters[1]);
                _lotId = Util.NVC(parameters[2]);
                _wipSeq = Util.NVC(parameters[3]);
                _equipmentMeasurementCode = Util.NVC(parameters[4]);
            }

            InitializeControl();
            GetOutSideScrapInfo();
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }

        private void txtWidth_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtWidth.Value < txtEnd.Value)
                txtStart.Value = txtEnd.Value - txtWidth.Value;
        }

        private void txtWidth_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEnd.Value < txtWidth.Value)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtWidth.Focus();
                            e.Handled = true;
                        }
                    }, ObjectDic.Instance.GetObjectName("길이"));
                }
                else
                {
                    txtStart.Value = txtEnd.Value - txtWidth.Value;
                    SelectProductQty();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            loadingIndicator.Visibility = Visibility.Visible;

            //string bizRuleName = string.Equals(_processCode, Process.COATING) ? "BR_PRD_REG_DATACOLLECT_DEFECT_CT" : "BR_PRD_REG_DATACOLLECT_DEFECT_RP";
            const string bizRuleName = "BR_PRD_REG_RM_ADJ_DEFECT_CT";

            DataSet inDataSet = new DataSet();
            DataTable dtInEquipment = inDataSet.Tables.Add("IN_EQP");
            dtInEquipment.Columns.Add("SRCTYPE", typeof(string));
            dtInEquipment.Columns.Add("IFMODE", typeof(string));
            dtInEquipment.Columns.Add("EQPTID", typeof(string));
            dtInEquipment.Columns.Add("USERID", typeof(string));

            DataRow dr = dtInEquipment.NewRow();
            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            dr["IFMODE"] = IFMODE.IFMODE_OFF;
            dr["EQPTID"] = _equipmentCode;
            dr["USERID"] = LoginInfo.USERID;
            dtInEquipment.Rows.Add(dr);

            DataTable dtInLot = inDataSet.Tables.Add("IN_LOT");
            dtInLot.Columns.Add("LOTID", typeof(string));
            dtInLot.Columns.Add("WIPSEQ", typeof(Int32));
            dtInLot.Columns.Add("USER_EQPT_MEASR_PSTN_ID", typeof(string));
            dtInLot.Columns.Add("USER_SCRAP_LEN", typeof(decimal));
            dtInLot.Columns.Add("USER_ROLLMAP_CLCT_TYPE", typeof(string));

            DataRow newRow = dtInLot.NewRow();
            newRow["LOTID"] = _lotId;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["USER_EQPT_MEASR_PSTN_ID"] = _equipmentMeasurementCode;
            newRow["USER_SCRAP_LEN"] = txtWidth.Value;
            newRow["USER_ROLLMAP_CLCT_TYPE"] = cboFaultytype.SelectedValue;
            dtInLot.Rows.Add(newRow);

            new ClientProxy().ExecuteService_Multi(bizRuleName, "IN_EQP,IN_LOT", null, (bizResult, bizException) =>
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                try
                {
                    if (bizException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(bizException);
                        return;
                    }
                    //정상 처리 되었습니다.
                    Util.MessageInfo("SFU1275");
                    IsUpdated = true;
                }
                catch (Exception ex)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    IsUpdated = false;
                    Util.MessageException(ex);
                }
            }, inDataSet);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }


        #endregion

        #region Mehod

        private void GetOutSideScrapInfo()
        {
            string bizRuleName = "DA_PRD_SEL_RM_RPT_OUTSIDE_SCRAP";

            DataTable inTable = new DataTable { TableName = "RQSTDT" };
            inTable.Columns.Add("ADJ_LOTID", typeof(string));
            inTable.Columns.Add("ADJ_WIPSEQ", typeof(decimal));
            inTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["ADJ_LOTID"] = _lotId;
            dr["ADJ_WIPSEQ"] = _wipSeq;
            dr["EQPT_MEASR_PSTN_ID"] = _equipmentMeasurementCode;
            inTable.Rows.Add(dr);

            DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if(CommonVerify.HasTableRow(dt))
            {
                txtStart.Value = dt.Rows[0]["ADJ_STRT_PSTN"].GetDouble();
                txtEnd.Value = dt.Rows[0]["ADJ_END_PSTN"].GetDouble();
                txtWidth.Value = dt.Rows[0]["WND_LEN"].GetDouble();

                if (!string.IsNullOrEmpty(dt.Rows[0]["ROLLMAP_CLCT_TYPE"].GetString()))
                    cboFaultytype.SelectedValue = dt.Rows[0]["ROLLMAP_CLCT_TYPE"].GetString();
            }
            else
            {
                txtWidth.Value = 0;
                txtStart.Value = 0;
                txtEnd.Value = 0;
            }
        }

        private void SelectProductQty()
        {
            //Top, Back 양품량, Foil 양
            string bizRuleName = "DA_PRD_SEL_RM_RPT_PROD_QTY";
            DataTable inTable = new DataTable();
            inTable.TableName = "RQSTDT";
            inTable.Columns.Add("EQPTID", typeof(string));
            inTable.Columns.Add("LOTID", typeof(string));
            inTable.Columns.Add("WIPSEQ", typeof(int));
            inTable.Columns.Add("CLCT_SEQNO", typeof(int));
            inTable.Columns.Add("ADJ_STRT_PSTN", typeof(decimal));
            inTable.Columns.Add("ADJ_END_PSTN", typeof(decimal));
            DataRow newRow = inTable.NewRow();
            newRow["EQPTID"] = _equipmentCode;
            newRow["LOTID"] = _lotId;
            newRow["WIPSEQ"] = _wipSeq;
            newRow["ADJ_STRT_PSTN"] = txtStart.Value;
            newRow["ADJ_END_PSTN"] = txtEnd.Value;

            inTable.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                txtTopQty.Value = dtResult.Rows[0]["TOP_PROD_QTY"].GetDouble();
                txtBackQty.Value = dtResult.Rows[0]["BACK_PROD_QTY"].GetDouble();
                txtFoilQty.Value = dtResult.Rows[0]["FOIL_QTY"].GetDouble();
            }
            else
            {
                txtTopQty.Value = double.NaN;
                txtBackQty.Value = double.NaN;
                txtFoilQty.Value = double.NaN;
            }
        }

        private void SetFaultytypeCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_RM_RPT_DEFECT_CBO";
            string[] arrColumn = { "PROCID", "EQPTID", "EQPT_MEASR_PSTN_ID", "LANGID" };
            string[] arrCondition = { _processCode, _equipmentCode, _equipmentMeasurementCode, LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.NONE, selectedValueText, displayMemberText, string.Empty);
        }

        private bool ValidationSearch()
        {
            if (txtWidth.Value > txtEnd.Value)
            {
                Util.MessageInfo("SFU8116", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtWidth.Focus();
                    }
                }, ObjectDic.Instance.GetObjectName("길이"));
                return false;
            }

            if (cboFaultytype.SelectedIndex < 0)
            {
                Util.MessageInfo("SFU8116", ObjectDic.Instance.GetObjectName("DFCT_TYPE"));
                return false;
            }

            return true;
        }

        #endregion
    }
}
