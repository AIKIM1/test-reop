/*************************************************************************************
 Created Date : 2021.06.07
      Creator : 신광희
   Decription : ESNB 재와인딩 투입,투입취소 DRB Copy 후 ESWA 적용
--------------------------------------------------------------------------------------
 [Change History]
 2021.06.07  정문교 : Initial Created.
 2021.09.13  조영대 : Carrier ID Label 이름변경   
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.ELEC002
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC002_006_INPUT_CANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
        private string _prodLotId = string.Empty;
        private string _prodWipseq = string.Empty;
        private bool _isInput;
        private string _ldrLotIdentBasCode = string.Empty;

        Util _Util = new Util();

        DateTime lastKeyPress = DateTime.Now;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC002_006_INPUT_CANCEL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            _processCode = parameters[0] as string;
            _equipmentSegmentCode = parameters[1] as string;
            _equipmentCode = parameters[2] as string;
            _equipmentName = parameters[3] as string;
            _prodLotId = parameters[4] as string;
            _prodWipseq = parameters[5] as string;

            _isInput = (parameters[6] as string) == "Y" ? true : false;

            _ldrLotIdentBasCode = parameters[7] as string;

            txtEquipment.Text = "[" + _equipmentCode + "]" + _equipmentName;

            if (_isInput)
            {
                this.Header = ObjectDic.Instance.GetObjectName("투입");
                btnInupt.Content = ObjectDic.Instance.GetObjectName("투입");
                txtCstID.Focus();
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("투입취소");
                btnInupt.Content = ObjectDic.Instance.GetObjectName("투입취소");

                // 투입취소 LOT 검색 
                GetCancelLotInfo();
                txtCstID.IsEnabled = false;
            }

            SetIdentInfo();
            if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
            {
                lblCstID.Text = ObjectDic.Instance.GetObjectName("CARRIER ID");
            }
            else
            {
                lblCstID.Text = ObjectDic.Instance.GetObjectName("CARRIER ID/LOT ID");
            }
        }

        /// <summary>
        /// 투입, 투입취소
        /// </summary>
        private void btnInupt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCstID()) return;

            // 투입처리 하시겠습니까?, 투입취소 하시겠습니까?
            string sMessageID = _isInput ? "SFU1987" : "SFU1988";

            Util.MessageConfirm(sMessageID, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if (_isInput)
                    {
                        InputProcess();
                    }
                    else
                    {
                        InputCancelProcess();
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 취소 대상LOT 조회
        /// </summary>
        private void GetCancelLotInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("EQPTID", typeof(string));
            dt.Columns.Add("INPUT_LOT_STAT_CODE", typeof(string));
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(decimal));
            dt.Columns.Add("INPUT_LOT_YN", typeof(string));

            DataRow row = dt.NewRow();
            row["PROCID"] = _processCode;
            row["EQPTID"] = _equipmentCode;
            row["INPUT_LOT_STAT_CODE"] = "PROC";
            row["LOTID"] = _prodLotId;
            row["WIPSEQ"] = _prodWipseq;
            row["INPUT_LOT_YN"] = "Y";
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_REWINDING_WRK_HIST_INPUT_DISTINCT", "INDATA", "OUTDATA", dt);

            if (CommonVerify.HasTableRow(result))
            {
                txtCstID.Text = result.Rows[0]["INPUT_LOTID"].ToString();
            }
            else
            {
                txtCstID.Text = string.Empty;
            }
        }

        /// <summary>
        /// 투입 
        /// </summary>
        private void InputProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable InEqp = inDataSet.Tables.Add("IN_EQP");
                InEqp.Columns.Add("SRCTYPE", typeof(string));
                InEqp.Columns.Add("IFMODE", typeof(string));
                InEqp.Columns.Add("EQPTID", typeof(string));
                InEqp.Columns.Add("USERID", typeof(string));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow row = InEqp.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _equipmentCode;
                row["USERID"] = LoginInfo.USERID;
                InEqp.Rows.Add(row);

                // 투입자재 장착위치 가져오기
                DataTable dt = GetEqptMountPstn();

                if (dt == null || dt.Rows.Count == 0) return;

                row = InInput.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["INPUT_LOTID"] = txtCstID.Text;
                InInput.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_LOT_RW_DRB", "INDATA,IN_INPUT", "OUT_INPUT", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");     // 정상처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 투입 취소
        /// </summary>
        private void InputCancelProcess()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable InEqp = inDataSet.Tables.Add("IN_EQP");
                InEqp.Columns.Add("SRCTYPE", typeof(string));
                InEqp.Columns.Add("IFMODE", typeof(string));
                InEqp.Columns.Add("EQPTID", typeof(string));
                InEqp.Columns.Add("USERID", typeof(string));
                InEqp.Columns.Add("PROCID", typeof(string));
                InEqp.Columns.Add("LOTID", typeof(string));
                InEqp.Columns.Add("WIPSEQ", typeof(decimal));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                ////////////////////////////////////////////////////////////////////////////////////////////////////////////
                DataRow row = InEqp.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["EQPTID"] = _equipmentCode;
                row["USERID"] = LoginInfo.USERID;
                row["PROCID"] = _processCode;
                row["LOTID"] = _prodLotId;
                row["WIPSEQ"] = _prodWipseq;
                InEqp.Rows.Add(row);

                // 투입자재 장착위치 가져오기
                DataTable dt = GetEqptMountPstn();

                if (dt == null || dt.Rows.Count == 0) return;

                row = InInput.NewRow();
                row["EQPT_MOUNT_PSTN_ID"] = dt.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                row["EQPT_MOUNT_PSTN_STATE"] = "A";
                row["INPUT_LOTID"] = txtCstID.Text;
                InInput.Rows.Add(row);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_LOT_CANCEL_RW_DRB", "INDATA,IN_INPUT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");     // 정상처리되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable GetEqptMountPstn()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow tmprow = dt.NewRow();
                tmprow["EQPTID"] = _equipmentCode;
                dt.Rows.Add(tmprow);

                DataTable dtEqptMount = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", dt);
                if (dtEqptMount == null || dtEqptMount.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU2019");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                    return null;
                }

                return dtEqptMount;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(_processCode) || string.IsNullOrEmpty(_equipmentSegmentCode))
                {
                    _ldrLotIdentBasCode = "";
                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = _processCode;
                row["EQSGID"] = _equipmentSegmentCode;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _ldrLotIdentBasCode = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); };
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

        #region[Validation]
        private bool ValidationCstID()
        {
            if (string.IsNullOrWhiteSpace(txtCstID.Text))
            {
                Util.MessageValidation("SFU7006"); // CarrierID를 입력하십시오.
                return false;
            }

            return true;
        }


        #endregion

        #endregion

    }
}
