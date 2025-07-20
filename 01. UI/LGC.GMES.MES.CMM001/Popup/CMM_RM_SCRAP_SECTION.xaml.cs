/*************************************************************************************
 Created Date : 2025.04.19
      Creator : 조성근
   Decription : ROLLMAP SCRAP SECTION 등록 수정 삭제 
--------------------------------------------------------------------------------------
 [Change History]
  2025.04.19  : Initial Created.
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
using System.Linq;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_RM_SCRAP_SECTION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public struct WinParams
        {
            public string processCode;
            public string equipmentCode;
            public string lotId;
            public string wipSeq;
            public string equipmentName;
            public string eqpt_measr_pstn_id;
            public string clctSeqNo;
            public string mode; //1 Search, 2 Edit, 3 Add
        }
        public WinParams wParams;

        private DataTable _dtReason;
        private Boolean updateFlag = false;

        private string _lotId = string.Empty;
        private string _wipseq = string.Empty;
        private int _collectSeq = 0;

        public bool IsUpdated { get; set; }

        public CMM_RM_SCRAP_SECTION()
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


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null && parameters.Length > 0)
            {
                wParams.processCode = Util.NVC(parameters[0]);
                wParams.equipmentCode = Util.NVC(parameters[1]);
                wParams.lotId = Util.NVC(parameters[2]);
                wParams.wipSeq = Util.NVC(parameters[3]);
                wParams.eqpt_measr_pstn_id = Util.NVC(parameters[4]);
                wParams.clctSeqNo = Util.NVC(parameters[5]);
                wParams.mode = Util.NVC(parameters[6]); // 1 조회, 2 수정, 3 신규                 
            }

            Header = ObjectDic.Instance.GetObjectName("SCRAP_SECTION");

            if (wParams.mode == "1") //조회만 가능한 모드
            {
                //cboScrapSection.IsEnabled = false;   // 얘는 바꿀 수 있게..
                cboClctType.IsEnabled = false;
                cboReasonCode.IsEnabled = false;
                btnSave.Visibility = Visibility.Collapsed;
                btnDel.Visibility = Visibility.Collapsed;
                btnNew.Visibility = Visibility.Collapsed;
                txtStartPosition.IsReadOnly = true;
                txtWidth.IsReadOnly = true;
                txtEndPosition.IsReadOnly = true;

            }

            bool bRollmapSBL = SelectRollMapLot();
            if (bRollmapSBL == false)
            {
                ReasonBox.Visibility = Visibility.Collapsed;
            }

            InitializeCombo();

        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void InitializeCombo()
        {
            DataTable dtResult = GetEqptClctTypeCombo("SCRAP_SECTION");
            cboClctType.ItemsSource = dtResult.Copy().AsDataView();
            cboClctType.SelectedIndex = 0;

            GetReasonCode();

            DataTable dtResn = GetEqptDefectCode("SCRAP_SECTION", null);
            List<string> strList = new List<string>();
            for (int i = 0; i < dtResn.Rows.Count; i++)
            {
                string strData = dtResn.Rows[i]["RESNCODE"].ToString();
                strList.Insert(i, strData);
            }
            var query = _dtReason.AsEnumerable().Where(o => strList.Contains(o.Field<string>("RESNCODE"))).ToList();
            DataTable dtResn2 = query.Any() ? query.CopyToDataTable() : _dtReason.Clone();
            cboReasonCode.ItemsSource = dtResn2.Copy().AsDataView();
            cboReasonCode.SelectedIndex = 0;

            SetScrapSectionCombo();
        }

        private DataTable GetEqptClctTypeCombo(string strPstnID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = wParams.equipmentCode;
            dr["PROCID"] = wParams.processCode;
            dr["EQPT_MEASR_PSTN_ID"] = strPstnID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_DEFECT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        private DataTable GetRollmapClctData(string strPstnID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("ADJ_LOTID", typeof(string));
            RQSTDT.Columns.Add("ADJ_WIPSEQ", typeof(decimal));
            RQSTDT.Columns.Add("PROCID", typeof(string));
            RQSTDT.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            RQSTDT.Columns.Add("DEL_FLAG", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["ADJ_LOTID"] = wParams.lotId;
            dr["ADJ_WIPSEQ"] = wParams.wipSeq;
            dr["PROCID"] = wParams.processCode;
            dr["EQPT_MEASR_PSTN_ID"] = strPstnID;
            dr["DEL_FLAG"] = "N";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RM_RPT_CLCT_DATA_COMBO", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult;
        }

        private void GetReasonCode()
        {
            const string bizRuleName = "BR_PRD_SEL_WIPRESONCOLLECT_INFO";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WIPSEQ", typeof(decimal));
            inDataTable.Columns.Add("ACTID", typeof(string));
            inDataTable.Columns.Add("RESNPOSITION", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["AREAID"] = LoginInfo.CFG_AREA_ID;
            inData["PROCID"] = wParams.processCode;
            inData["EQPTID"] = wParams.equipmentCode;
            inData["LOTID"] = wParams.lotId;
            inData["WIPSEQ"] = wParams.wipSeq;
            inData["ACTID"] = "LOSS_LOT,DEFECT_LOT";
            inDataTable.Rows.Add(inData);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            string[] exceptionCode = new string[] { "LENGTH_LACK", "LENGTH_EXCEED" };
            var queryResult = dtResult.AsEnumerable().Where(o => !exceptionCode.Contains(o.Field<string>("PRCS_ITEM_CODE"))).ToList();

            _dtReason = queryResult.Any() ? queryResult.CopyToDataTable() : dtResult.Clone();
        }

        private DataTable GetEqptDefectCode(string strPstnID, string strResnCode)
        {
            const string bizRuleName = "DA_PRD_SEL_RM_COM_EQPT_INSP_DFCT_CODE";
            DataTable inDataTable = new DataTable("RQSTDT");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));
            inDataTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("DFCT_AUTO_APPLY_FLAG", typeof(string));

            DataRow inData = inDataTable.NewRow();
            inData["LANGID"] = LoginInfo.LANGID;
            inData["PROCID"] = wParams.processCode;
            inData["EQPTID"] = wParams.equipmentCode;
            inData["USE_FLAG"] = "Y";
            inData["DFCT_AUTO_APPLY_FLAG"] = "Y";
            inData["EQPT_MEASR_PSTN_ID"] = strPstnID;
            inData["RESNCODE"] = strResnCode;


            inDataTable.Rows.Add(inData);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

            return dtResult;
        }

        private void SetScrapSectionCombo()
        {
            DataTable dtResult = GetRollmapClctData("SCRAP_SECTION");

            if (CommonVerify.HasTableRow(dtResult) == false)
            {
                SetNewMode();
                return;
            }

            cboScrapSection.ItemsSource = dtResult.Copy().AsDataView();

            for (int i = 0; i < dtResult.Rows.Count; i++)
            {
                if (Util.NVC_Int(dtResult.Rows[i]["CLCT_SEQNO"].ToString()) == Util.NVC_Int(wParams.clctSeqNo))
                {
                    cboScrapSection.SelectedIndex = i;
                    DisplayControlByTagAutoFlag(dtResult.Rows[i]["TAG_AUTO_FLAG"].GetString());
                    return;
                }
            }

            cboScrapSection.SelectedIndex = 0;
            DisplayControlByTagAutoFlag(CommonVerify.HasTableRow(dtResult) ? dtResult.Rows[0]["TAG_AUTO_FLAG"].GetString() : string.Empty);

        }

        private void SetNewMode()
        {
            txtStartPosition.Value = 0;
            txtEndPosition.Value = 0;
            txtWidth.Value = 0;
            btnDel.IsEnabled = false;
            btnNew.IsEnabled = false;
            cboScrapSection.SelectedValue = "";
            updateFlag = true;
            cboScrapSection.IsEnabled = false;
            _lotId = string.Empty;
            _wipseq = string.Empty;
        }

        private bool SelectRollMapLot()
        {
            try
            {
                const string bizRuleName = "BR_PRD_CHK_RM_RPT_LOT";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataRow dr = inTable.NewRow();
                dr["LOTID"] = wParams.lotId;
                dr["WIPSEQ"] = wParams.wipSeq;
                inTable.Rows.Add(dr);

                DataTable dt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dt))
                {
                    if (dt.Rows[0]["ROLLMAP_LOT_YN"].Equals("Y"))
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void cboScrapSection_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            try
            {
                updateFlag = false;

                DataTable dt = DataTableConverter.Convert(cboScrapSection.ItemsSource);

                txtStartPosition.Value = 0;
                txtEndPosition.Value = 0;
                txtWidth.Value = 0;

                if (!CommonVerify.HasTableRow(dt) || cboScrapSection.SelectedIndex < 0)
                {
                    DisplayControlByTagAutoFlag(string.Empty);
                    return;
                }

                DataRow dr = dt.Rows[cboScrapSection.SelectedIndex];
                txtStartPosition.Value = dr["ADJ_STRT_PSTN2"].GetDouble();
                txtEndPosition.Value = dr["ADJ_END_PSTN2"].GetDouble();
                txtWidth.Value = dr["ADJ_END_PSTN2"].GetDouble() - dr["ADJ_STRT_PSTN2"].GetDouble();

                _lotId = dr["LOTID"].ToString();
                _wipseq = dr["WIPSEQ"].ToString();
                _collectSeq = (int)dr["CLCT_SEQNO"].GetInt();

                if (string.IsNullOrEmpty(dr["ROLLMAP_CLCT_TYPE"].ToString()) == false) cboClctType.SelectedValue = dr["ROLLMAP_CLCT_TYPE"].ToString();
                if (string.IsNullOrEmpty(dr["BACK_RESNCODE"].ToString()) == false) cboReasonCode.SelectedValue = dr["BACK_RESNCODE"].ToString();

                DisplayControlByTagAutoFlag(dr["TAG_AUTO_FLAG"].GetString());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("SFU1230", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveDefectForRollMap("D");
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Util.NVC_Decimal(txtStartPosition.Value) > Util.NVC_Decimal(txtEndPosition.Value) || Util.NVC_Decimal(txtStartPosition.Value) < 0)
                {
                    Util.MessageInfo("SFU8116", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtEndPosition.Focus();
                        }
                    }, ObjectDic.Instance.GetObjectName("위치"));
                    return;
                }

                // 기존 좌표랑 하나라도 동일하면 업데이트 아닐시 신규 등록
                if (updateFlag == true)
                {
                    //신규
                    SaveDefectForRollMap("N");
                }
                else if (updateFlag == false)
                {
                    //수정
                    SaveDefectForRollMap("U");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveDefectForRollMap(string sFlag)
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_RM_RPT_DEFECT_LIST";

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USER", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));

                DataTable inRollmapTable = inDataSet.Tables.Add("INROLLMAP");
                inRollmapTable.Columns.Add("LOTID", typeof(string));
                inRollmapTable.Columns.Add("WIPSEQ", typeof(decimal));
                inRollmapTable.Columns.Add("EQPT_MEASR_PSTN_ID", typeof(string));
                inRollmapTable.Columns.Add("ROLLMAP_CLCT_TYPE", typeof(string));
                inRollmapTable.Columns.Add("CLCT_SEQNO", typeof(int));
                inRollmapTable.Columns.Add("ACTID", typeof(string));
                inRollmapTable.Columns.Add("ADJ_STRT_PSTN2", typeof(decimal));  //절대값 전송, 상대좌표는 NULL 상태로 보냄
                inRollmapTable.Columns.Add("ADJ_END_PSTN2", typeof(decimal));  //절대값 전송, 상대좌표는 NULL 상태로 보냄
                inRollmapTable.Columns.Add("DEFECT_LEN", typeof(decimal));
                inRollmapTable.Columns.Add("METHOD", typeof(string));     //N 신규, U 수정, D 삭제
                inRollmapTable.Columns.Add("RESNCODE", typeof(string));   //활동유형코드

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = wParams.equipmentCode;
                newRow["LOTID"] = wParams.lotId;
                newRow["WIPSEQ"] = wParams.wipSeq;
                newRow["USER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);


                DataRow newRollRow = inRollmapTable.NewRow();
                newRollRow["LOTID"] = string.IsNullOrEmpty(_lotId) ? wParams.lotId : _lotId;
                newRollRow["WIPSEQ"] = string.IsNullOrEmpty(_wipseq) ? wParams.wipSeq : _wipseq;
                newRollRow["EQPT_MEASR_PSTN_ID"] = wParams.eqpt_measr_pstn_id;
                newRollRow["ROLLMAP_CLCT_TYPE"] = cboClctType.SelectedValue.ToString();

                if (ReasonBox.Visibility == Visibility.Visible)
                {
                    newRollRow["RESNCODE"] = cboReasonCode.SelectedValue.ToString();
                }


                if (sFlag == "N")
                {
                    //신규
                    newRollRow["CLCT_SEQNO"] = _collectSeq + 1;
                }
                else
                {
                    // 수정, 삭제
                    newRollRow["CLCT_SEQNO"] = _collectSeq;
                }
                newRollRow["ACTID"] = "ADJ_USER_ROLLMAP";
                newRollRow["ADJ_STRT_PSTN2"] = txtStartPosition.Value;
                newRollRow["ADJ_END_PSTN2"] = txtEndPosition.Value;
                newRollRow["DEFECT_LEN"] = txtWidth.Value;

                //if (sFlag == "N")
                //{
                //    newRollRow["STRT_PSTN"] = txtStartPosition.Value;
                //    newRollRow["END_PSTN"] = txtEndPosition.Value;
                //}

                newRollRow["METHOD"] = sFlag;
                inRollmapTable.Rows.Add(newRollRow);


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INROLLMAP", null, (result, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (sFlag == "D")
                        {  // 삭제
                            Util.MessageInfo("SFU1273"); // 삭제되었습니다.
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                        else
                        { // 저장
                            Util.MessageInfo("SFU1270"); // 저장되었습니다.
                            IsUpdated = true;
                            DialogResult = MessageBoxResult.OK;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtStartPosition_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEndPosition.Value > 0 && txtEndPosition.Value > txtStartPosition.Value)
                {
                    txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
                }
            }
        }

        private void txtStartPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEndPosition.Value > 0 && txtEndPosition.Value > txtStartPosition.Value)
            {
                txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
            }
        }

        private void txtEndPosition_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                if (txtEndPosition.Value > 0 && txtEndPosition.Value > txtStartPosition.Value)
                {
                    txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
                }
            }
        }

        private void txtEndPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtEndPosition.Value > 0 && txtEndPosition.Value > txtStartPosition.Value)
            {
                txtWidth.Value = txtEndPosition.Value - txtStartPosition.Value;
            }
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            SetNewMode();
        }

        private void DisplayControlByTagAutoFlag(string tagAutoFlag)
        {
            //if (tagAutoFlag == "Y")
            //{
            //    chkStartPosition.IsEnabled = false;
            //    txtStartPosition.IsEnabled = false;
            //    txtWidth.IsEnabled = false;
            //    chkEndPosition.IsEnabled = false;
            //    txtEndPosition.IsEnabled = false;
            //}
            //else
            //{
            //    txtStartPosition.IsEnabled = true;
            //    txtWidth.IsEnabled = true;
            //    chkEndPosition.IsEnabled = true;
            //    txtEndPosition.IsEnabled = true;
            //    if (_isLoaded == false) txtStartPosition.IsEnabled = false;
            //}
        }
    }
}
