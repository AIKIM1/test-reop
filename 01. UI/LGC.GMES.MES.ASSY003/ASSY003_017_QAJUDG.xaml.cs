/*************************************************************************************
 Created Date : 2017.10.11
      Creator : 이진선
   Decription : VD QA대상LOT조회 설비 내용
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
  2023.10.20  이병윤 : E20230401-002084_샘플수량 > 재공수량일때 인터락처리
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_017_QAJUDG : C1Window, IWorkArea
    {
        #region Declaration & Constructor        

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        string lotid = string.Empty;
        string qaJudg = string.Empty;
        string eqptid = string.Empty;
        string lotid_rt = string.Empty;
        string eqpt_btch_wrk_no = string.Empty;
        string eqsgid = string.Empty;
        string FailCode = string.Empty;
        string _PRODID;
        string resncode = string.Empty;
        string number = string.Empty;
        string cost_center = string.Empty;
        string actID = string.Empty;
        DataTable dtRslt = null;
        string resolve = string.Empty;
        string qa_trgt_flag = string.Empty;
        string _Unit;
        string _PROCID = string.Empty;

        bool gbLotQAInsp = false;       //2017.08.13 Add By Kim Joonphil Lot ID별 QA검사 여부 Flag

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public string LOTID
        {
            get { return lotid; }
            set { lotid = value; }
        }
        public string QAJUDG
        {
            get { return qaJudg; }
            set { qaJudg = value; }
        }

        public ASSY003_017_QAJUDG()
        {
            InitializeComponent();
        }

        #endregion

        #region[Event]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    LOTID = Util.NVC(tmps[0]);
                    QAJUDG = "";//Util.NVC(tmps[1]);
                    eqptid = Util.NVC(tmps[2]);
                    lotid_rt = Util.NVC(tmps[3]);
                    eqpt_btch_wrk_no = Util.NVC(tmps[4]);
                    eqsgid = Util.NVC(tmps[5]);
                    _PRODID = Util.NVC(tmps[6]);
                    _Unit = Util.NVC(tmps[7]);
                    _PROCID = Util.NVC(tmps[8]);
                }


                DataTable dt = new DataTable();
                dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
                dt.Columns.Add("PRODID", typeof(string));
                if (!_Unit.Equals("LOT"))
                {
                    dt.Columns.Add("SKIDID", typeof(string));
                }

                DataRow row = dt.NewRow();
                row["EQPT_BTCH_WRK_NO"] = eqpt_btch_wrk_no;
                row["PRODID"] = _PRODID;

                if (!_Unit.Equals("LOT"))
                {
                    row["SKIDID"] = LOTID;
                }

                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA_NJ", "INDATA", "OUTDATA", dt);

                cboLotID.ItemsSource = DataTableConverter.Convert(result);

                cboLotID.DisplayMemberPath = "CBO_NAME";
                cboLotID.SelectedValuePath = "CBO_CODE";

                //if (result.Rows.Count != 0)
                //{
                //    qa_trgt_flag = Convert.ToString(result.Rows[0]["QA_INSP_TRGT_FLAG"]);
                //}


                dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("DFCT_GR_CODE", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("RESNCODE", typeof(string));

                row = dt.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["DFCT_GR_CODE"] = "ALL";
                row["PROCID"] = _PROCID;
                row["ACTID"] = "CHARGE_PROD_LOT,LOSS_LOT";
                //row["RESNCODE"] = _PROCID.Equals("A6000") ?  "PS01S04" : "PS01S12";

                dt.Rows.Add(row);
                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_PROC_DFCT_CODE_NJ", "INDATA", "RSLT", dt);
                txtNumber.Text = result.Rows.Count == 0 ? 0.ToString() : result.Rows[0]["BAS_DFCT_QTY"].ToString().Equals("") ? 0.ToString() : Convert.ToString((int)((decimal)result.Rows[0]["BAS_DFCT_QTY"]));

                //txtQAUSER.Text = LoginInfo.USERNAME;


                initCombo();

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["CMCDTYPE"] = "VD_RESN_CODE";
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMCODE_VD_RESN", "INDATA", "RSLT", dt);

                Util.GridSetData(dgResn, result, null, false);

                string[] sFilter = { "HOLD_LOT" };
                combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

                GetLossInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
       
        private void txtPerson_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;
        }
     
      
    
        private void txtPerson_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null)
            {
                txtPersonId.Text = "";
            }
        }
        private void cboQAJUDG_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null)
                return;

            dgResn.IsEnabled = false;
            grHold.IsEnabled = false;

            if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F"))
                dgResn.IsEnabled = true;
        }

        private void dgResnChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            //grHold.Visibility = Visibility.Collapsed;
            grHold.IsEnabled = false;

            // 부모 조회 없으므로 로직 수정..
            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                FailCode = Convert.ToString(dtRow["CMCODE"]);
                resolve = Convert.ToString(dtRow["RESOLVE"]);

                if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["RESOLVE"].Equals("REWORK"))
                    grHold.IsEnabled = false;//grHold.Visibility = Visibility.Collapsed; 

                if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["CMCODE"].Equals("DF"))
                {
                    grHold.IsEnabled = true;
                    //grHold.Visibility = Visibility.Visible;
                    cboHoldType.SelectedValue = "PH01H02";
                    txtHoldResn.Visibility = Visibility.Collapsed;
                }

                else if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["CMCODE"].Equals("ETC"))
                {
                    grHold.IsEnabled = true;
                    //cboHoldType.IsEnabled = false;
                    cboHoldType.SelectedValue = "PH99H99";
                    txtHoldResn.Visibility = Visibility.Visible;
                   // cboHoldType.SelectedIndex = 0;
                }
                    
            }
        }

        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtPerson.Text.Equals(""))
                    {
                        txtPersonDept.Text = string.Empty;
                        txtPersonId.Text = string.Empty;
                        return;
                    }

                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtPerson.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                        txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                        txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                    }
                    else
                    {
                        dgPersonSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgPersonSelect);

                        dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;

                     
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

       
        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }
        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void btnInspectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (cboLotID.SelectedItems.Count == 0)
            {
                Util.MessageValidation("SFU3370"); //샘플LOT을 선택해주세요
                return;
            }
            if (int.Parse(txtNumber.Text) == 0)
            {
                Util.MessageValidation("SFU3371"); //수량이 0이상이어야 합니다.
                return;
            }
            if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("E") || Convert.ToString(cboQAJUDG.SelectedValue).Equals("SELECT"))
            {
                Util.MessageValidation("SFU3372"); //판정결과를 선택해주세요
                return;
            }

            if(Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && FailCode == "")
            {
                Util.MessageValidation("SFU1585"); //불량정보가 없습니다.
                return;
            }

            // 수량 체크
            string lots = String.Empty;
            for (int i = 0; i < cboLotID.SelectedItems.Count; i++)
            {
                lots += Convert.ToString(cboLotID.SelectedItems[i]) + ",";
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("LOTS", typeof(string));
            dt.Columns.Add("SAMPLE", typeof(string));

            DataRow row = dt.NewRow();
            row["LOTS"] = lots;
            row["SAMPLE"] = txtNumber.Text;
            dt.Rows.Add(row);

            DataTable rsltQty = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA_QTY_NJ", "RQST", "RSLT", dt);
            if (rsltQty.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3370"); //샘플LOT을 선택해주세요
                return;
            }
            // If the sample QTY is more than the LOT QTY, it should be alarm and op can’t do completion.
            if (rsltQty.Rows[0]["RSLT"].ToString().Equals("NG"))
            {
                // 샘플LOT 수량이 LOT 수량보다 클 수는 없습니다.
                Util.MessageValidation("SFU8920");
                return;
            }


            //검사결과를 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2811"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        
                        ////QA 저장 + 물청
                        SaveQaJudg();

                        Util.AlertInfo("SFU1449"); //검사 결과 저장 완료


                        this.DialogResult = MessageBoxResult.OK;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });




        }
        #endregion

        #region[Method]
        private void initCombo()
        {
            string[] sFilter = { "QAJUDGE_POPUP" };
            combo.SetCombo(cboQAJUDG, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");
        
        }
       
        private void SaveQaJudg()
        {

            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("INSP_DTTM", typeof(string));
            inDataTable.Columns.Add("INSP_USERID", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["EQPTID"] = eqptid;
            row["USERID"] = LoginInfo.USERID;
            row["INSP_DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            row["INSP_USERID"] = LoginInfo.USERID;

            inData.Tables["INDATA"].Rows.Add(row);
            DataTable inLot = inData.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("LOTID_RT", typeof(string));
            inLot.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
            inLot.Columns.Add("JUDG_VALUE", typeof(string));
            inLot.Columns.Add("DFCT_CODE", typeof(string));

            for (int i = 0; i < cboLotID.SelectedItems.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = cboLotID.SelectedItems[i];
                row["LOTID_RT"] = lotid_rt;
                row["EQPT_BTCH_WRK_NO"] = eqpt_btch_wrk_no;
                row["JUDG_VALUE"] = Convert.ToString(cboQAJUDG.SelectedValue);
                row["DFCT_CODE"] = Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") ? FailCode : null; //20170816 삭제되어 추가

                inData.Tables["IN_LOT"].Rows.Add(row);
            }


            DataTable IndataTable = inData.Tables.Add("INRESN");

            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(Int32));
            IndataTable.Columns.Add("ACTID", typeof(string));
            IndataTable.Columns.Add("RESNCODE", typeof(string));
            IndataTable.Columns.Add("RESNQTY", typeof(double));
            IndataTable.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_QTY", typeof(Int32));
            IndataTable.Columns.Add("LANE_PTN_QTY", typeof(Int32));
            IndataTable.Columns.Add("COST_CNTR_ID", typeof(string));

            for (int i = 0; i < cboLotID.SelectedItems.Count; i++)
            {
                DataRow dr = IndataTable.NewRow();

                dr["LOTID"] = Convert.ToString(cboLotID.SelectedItems[i]);
                dr["WIPSEQ"] = GetWipSeq(Convert.ToString(cboLotID.SelectedItems[i]));
                dr["ACTID"] = actID;
                dr["RESNCODE"] = resncode;
                dr["RESNQTY"] = txtNumber.Text;
                dr["DFCT_TAG_QTY"] = 0;
                dr["LANE_QTY"] = 1;
                dr["LANE_PTN_QTY"] = 1;
                dr["COST_CNTR_ID"] = cost_center;
                IndataTable.Rows.Add(dr);
            }

            if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && !FailCode.Equals("WF"))//두께불량/ETC
            {
                string sHoldType = Convert.ToString(cboHoldType.SelectedValue);//Util.GetCondition(cboHoldType, "사유를 선택하세요");
                DataTable inHold = inData.Tables.Add("IN_HOLD");


                inHold.Columns.Add("HOLD_NOTE", typeof(string));
                inHold.Columns.Add("RESNCODE", typeof(string));
                inHold.Columns.Add("HOLD_CODE", typeof(string));
                inHold.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
                inHold.Columns.Add("ACTION_USERID", typeof(string));

                DataRow dr2 = inHold.NewRow();
                dr2["HOLD_NOTE"] = txtHoldResn.Text.Equals("") ? null : txtHoldResn.Text;
                dr2["RESNCODE"] = sHoldType;
                dr2["HOLD_CODE"] = sHoldType;
                dr2["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);
                dr2["ACTION_USERID"] = txtPersonId.Text;
                inHold.Rows.Add(dr2);

            }

            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_QAJUDGE_VDQA_NJ", "INDATA,IN_LOT,INRESN,IN_HOLD", null, inData);

        }
        
        private Decimal GetWipSeq(string lotid)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));

            DataRow row = dt.NewRow();
            row["LOTID"] = lotid;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "RSLT", dt);
            if (result.Rows.Count == 0)
            {
                return Convert.ToDecimal(0);
            }

            return Convert.ToDecimal(result.Rows[0]["WIPSEQ"]);

        }
        private void GetLossInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("AREAID", typeof(string));
            dt.Columns.Add("PROCID", typeof(string));
            dt.Columns.Add("ACTID", typeof(string));
            dt.Columns.Add("DFCT_GR_CODE", typeof(string));

            DataRow row = dt.NewRow();
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["PROCID"] = _PROCID;// _Unit.Equals("LOT") ? Process.VD_LMN : "A1000";//LoginInfo.CFG_PROC_ID; //Process.VD_LMN;
            row["ACTID"] = "CHARGE_PROD_LOT,LOSS_LOT";
            row["DFCT_GR_CODE"] = "ALL";
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_PROC_DFCT_CODE_NJ", "INDATA", "RSLT", dt);
            if (result.Rows.Count == 0)
            {
                Util.MessageValidation("SFU4223"); //공장별 Activity reson code에 물품청구 기준정보가 없습니다. 등록 후 검사해주세요
                return;
            }

            resncode = Convert.ToString(result.Rows[0]["RESNCODE"]);
            number = Convert.ToString(result.Rows[0]["PRCS_ITEM_CODE"]);
            cost_center = Convert.ToString(result.Rows[0]["COST_CNTR_ID"]);
            actID = Util.NVC(result.Rows[0]["ACTID"]);

        }

























        #endregion
    }
}