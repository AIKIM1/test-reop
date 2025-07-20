/*************************************************************************************
 Created Date : 2024.05.28
      Creator : 신광희
   Decription : ROLLMAP Rewinder 폐기 팝업 CMM_ROLLMAP_RW_SCRAP Copy 
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.28  : Initial Created.
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

    public partial class CMM_RM_RW_SCRAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _lotId;
        private string _wipSeq;
        private string _processCode;
        private string _equipmentCode;
        private string _equipmentMeasurementCode;
        
        private string _startPosition;
        private string _endPosition;
        private string _length;
        private string _collectSeq;
        private string _collectType;

        public bool IsUpdated { get; set; }

        public CMM_RM_RW_SCRAP()
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
            tbRewinderScrapWidth.Text = ObjectDic.Instance.GetObjectName("M 추가 제거");
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
                _lotId = Util.NVC(parameters[0]);
                _wipSeq = Util.NVC(parameters[1]);
                _processCode = Util.NVC(parameters[2]);
                _equipmentCode = Util.NVC(parameters[3]);
                _equipmentMeasurementCode = Util.NVC(parameters[4]);
                _startPosition = Util.NVC(parameters[5]);
                _endPosition = Util.NVC(parameters[6]);
                _length = Util.NVC(parameters[7]);
                _collectSeq = Util.NVC(parameters[8]);
                _collectType = Util.NVC(parameters[9]);

                txtEndPosition.Value = _endPosition.GetDouble();
                if (!string.IsNullOrEmpty(_startPosition))
                    txtStart.Value = _startPosition.GetDouble();
                else
                    txtStart.Value = double.NaN;

                txtEnd.Value = _endPosition.GetDouble();
                txtLength.Value = _length.GetDouble();
            }
            InitializeControl();
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch()) return;

            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                string bizRuleName = "BR_PRD_REG_DATACOLLECT_DEFECT_RP";

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
                dtInLot.Columns.Add("CLCT_SEQNO", typeof(Int32));

                DataRow newRow = dtInLot.NewRow();
                newRow["LOTID"] = _lotId;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["USER_EQPT_MEASR_PSTN_ID"] = _equipmentMeasurementCode;
                newRow["USER_SCRAP_LEN"] = txtLength.Value;
                newRow["USER_ROLLMAP_CLCT_TYPE"] = cboFaultytype.SelectedValue;
                newRow["CLCT_SEQNO"] = _collectSeq;
                dtInLot.Rows.Add(newRow);

                //string xml = inDataSet.GetXml();

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
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLength_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {       
                if (txtEndPosition.Value - txtLength.Value < 0)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEndPosition.Focus();
                        }
                    }, ObjectDic.Instance.GetObjectName("길이"));
                }
            }
        }

        private void txtLength_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEnd.Value < txtLength.Value)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLength.Focus();
                            e.Handled = true;
                        }
                    }, ObjectDic.Instance.GetObjectName("길이"));
                }
                else
                    txtStart.Value = txtEnd.Value - txtLength.Value;
            }
        }

        private void txtLength_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEnd.Value < txtLength.Value)
            {

            }
            else
            {
                txtStart.Value = txtEnd.Value - txtLength.Value;
            }
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.OK;
        }


        #endregion

        #region Mehod


        private void InitializeClear()
        {
            txtEndPosition.Value = 0;
            txtLength.Value = 0;

            txtLength.IsEnabled = true;
            txtEndPosition.IsEnabled = true;
        }

        private bool ValidationSearch()
        {
            if (txtEndPosition.Value - txtLength.Value < 0)
            {
                Util.MessageInfo("SFU8116", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtEndPosition.Focus();
                    }
                }, ObjectDic.Instance.GetObjectName("길이"));
                return false;
            }

            return true;
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



        #endregion


    }
}
