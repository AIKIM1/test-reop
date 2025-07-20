/*************************************************************************************
 Created Date : 2022.04.28
      Creator : 신광희
   Decription : 재와인딩 작업시작 DRB ELEC003_LOTSTART_REWINDING Copy 후 작성
--------------------------------------------------------------------------------------
 [Change History]
 2021.06.07  정문교 : Initial Created.
 2022.04.28  신광희 : ESWA 재와인딩 공정진척 신규 화면에 ESNB 기능으로 추가 함
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
    public partial class ELEC002_006_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _processCode = string.Empty;
        private string _equipmentSegmentCode = string.Empty;
        private string _equipmentCode = string.Empty;
        private string _equipmentName = string.Empty;
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

        public ELEC002_006_LOTSTART()
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

            SetIdentInfo();
            if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
            {
                lblOutCstID.Visibility = Visibility.Visible;
                txtOutCstID.Visibility = Visibility.Visible;
                lblInputCstID.Text = ObjectDic.Instance.GetObjectName("투입 CARRIER ID");
            }
            else
            {
                lblOutCstID.Visibility = Visibility.Collapsed;
                txtOutCstID.Visibility = Visibility.Collapsed;
                lblInputCstID.Text = ObjectDic.Instance.GetObjectName("CARRIER ID/LOT ID");
            }

            txtEquipment.Text = "[" + _equipmentCode + "]" + _equipmentName;
            txtInputCstID.Focus();
        }

        /// <summary>
        /// 작업시작
        /// </summary>
        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLotStart()) return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    LotStart();
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

        private void LotStart()
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

                DataTable InOutput = inDataSet.Tables.Add("IN_OUTPUT");
                InOutput.Columns.Add("CSTID", typeof(string));
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
                row["INPUT_LOTID"] = txtInputCstID.Text;
                InInput.Rows.Add(row);

                row = InOutput.NewRow();
                row["CSTID"] = txtOutCstID.Text;
                InOutput.Rows.Add(row);

                //string xml = inDataSet.GetXml();

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_RW_DRB", "IN_EQP,IN_INPUT,IN_OUTPUT", "OUT_LOT", (bizResult, bizException) =>
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

                if (!CommonVerify.HasTableRow(dtEqptMount))
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
                    _ldrLotIdentBasCode = string.Empty;
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
        private bool ValidationLotStart()
        {
            if (_ldrLotIdentBasCode == "CST_ID" || _ldrLotIdentBasCode == "RF_ID")
            {
                if (string.IsNullOrWhiteSpace(txtInputCstID.Text) || string.IsNullOrWhiteSpace(txtOutCstID.Text))
                {
                    Util.MessageValidation("SFU7006"); // CarrierID를 입력하십시오.
                    return false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtInputCstID.Text))
                {
                    Util.MessageValidation("SFU8275", ObjectDic.Instance.GetObjectName("CARRIER ID/LOT ID")); // %1 (을)를 입력해 주세요.
                    return false;
                }
            }
            return true;
        }
        #endregion

        #endregion

    }
}
