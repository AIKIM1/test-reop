/*************************************************************************************
 Created Date : 2019.05.09
      Creator : INS 김동일K
   Decription : CWA3동 증설 - VD QA 대상LOT조회 화면 (ASSY0001.ASSY001_043_QAJUDG 2019/05/09 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.09  INS 김동일K : Initial Created.
  2020.09.25  이종원 [CNB 조립]에 해당하며 판정결과 "불합격"인 경우만 적용되는 "HOLD사유" 추가.
  2021.10.06  김지은 : 불합격 상세정보가 기본과 다를경우 동별 공통코드로 관리하도록 설정함. 2020.09.25 수정건 주석처리
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
using System.Collections.Generic;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_003_QAJUDG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_003_QAJUDG : C1Window, IWorkArea
    {
        #region Declaration & Constructor        

        Util _Util = new Util();
        CommonCombo combo = new CommonCombo();
        DataTable lotTable = null;
        string lotid = string.Empty;
        string qaJudg = string.Empty;
        string eqptid = string.Empty;
        string lotid_rt = string.Empty;
        string eqpt_btch_wrk_no = string.Empty;
        string eqsgid = string.Empty;
        string FailCode = string.Empty;
        string vd_qa_insp_cond = string.Empty;
        string resncode = string.Empty;
        string number = string.Empty;
        string cost_center = string.Empty;
        DataTable dtRslt = null;
        string resolve = string.Empty;
        string qa_trgt_flag = string.Empty;
        bool canInsp = true;
        string eqgrid = string.Empty;

        bool gbLotQAInsp = false;       //2017.08.13 Add By Kim Joonphil Lot ID별 QA검사 여부 Flag

        private DataTable _DT_BTCH_LOT_INFO = null;
        #endregion

        #region Initialize        
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

        public ASSY004_003_QAJUDG()
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
                    vd_qa_insp_cond = Util.NVC(tmps[6]);
                    gbLotQAInsp = (bool)tmps[7]; //2017.08.13 Add By Kim Joonphil
                    lotTable = tmps[8] as DataTable;
                    canInsp = (bool)tmps[9];
                    eqgrid = Util.NVC(tmps[10]);
                }

                DataTable dt = new DataTable();
                DataRow row = null;
                DataTable result = null;

                if (!gbLotQAInsp)
                {
                    dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

                    row = dt.NewRow();
                    row["LOTID"] = vd_qa_insp_cond.Equals("VD_QA_INSP_RULE_02") ? LOTID : null;
                    row["EQPT_BTCH_WRK_NO"] = eqpt_btch_wrk_no;

                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA_L", "INDATA", "OUTDATA", dt);

                    DataRow[] drTmp = result.Select("ISNULL(VD_QA_INSP_TYPE_CODE, '') <> 'Y'");
                    txtInspPancakeNum.Text = drTmp?.Length < 1 ? "0" : drTmp.Length.ToString();
                    
                    DataRow[] drTmp2 = result.Select("QA_INSP_TRGT_FLAG = 'Y' AND ISNULL(VD_QA_INSP_TYPE_CODE, '') <> 'Y'");

                    DataTable dtTmpLot = null;
                    if (drTmp2?.Length > 0)
                        dtTmpLot = drTmp2.CopyToDataTable();
                    else
                    {
                        DataRow[] drTmp3 = result.Select("ISNULL(VD_QA_INSP_TYPE_CODE, '') <> 'Y'");

                        if (drTmp3?.Length > 0)
                            dtTmpLot = drTmp3.CopyToDataTable();
                        else
                            dtTmpLot = result.Copy();
                    }

                    DataRow dr = dtTmpLot.NewRow();
                    dr["CBO_NAME"] = " - SELECT-";
                    dr["CBO_CODE"] = "SELECT";
                    dtTmpLot.Rows.InsertAt(dr, 0);

                    cboLotID.ItemsSource = DataTableConverter.Convert(dtTmpLot);
                    cboLotID.DisplayMemberPath = "CBO_NAME";
                    cboLotID.SelectedValuePath = "CBO_CODE";
                    cboLotID.SelectedIndex = dtTmpLot.Rows.Count == 2 ? 1 : 0;

                    // 검사 수량
                    DataRow[] drTmp4 = result.Select("CBO_CODE = '" + Util.NVC(cboLotID.SelectedValue) + "'");
                    txtNumber.Text = drTmp4?.Length > 0 ? double.Parse(drTmp4[0]["SMPL_QTY"].ToString()).ToString() : "0"; // double.Parse(result.Compute("SUM(SMPL_QTY)", "").ToString()).ToString();

                    if (result.Rows.Count != 0)
                    {
                        qa_trgt_flag = Convert.ToString(result.Rows[0]["QA_INSP_TRGT_FLAG"]);
                    }

                    _DT_BTCH_LOT_INFO = result.Copy();
                }
                else
                {
                    txtInspPancakeNum.Text = Convert.ToString((int)lotTable.Rows.Count);
                    cboLotID.ItemsSource = DataTableConverter.Convert(lotTable);

                    cboLotID.DisplayMemberPath = "CBO_NAME";
                    cboLotID.SelectedValuePath = "CBO_CODE";
                    cboLotID.SelectedIndex = 0;


                    dt = new DataTable();
                    dt.Columns.Add("LOTID", typeof(string));

                    row = dt.NewRow();
                    row["LOTID"] = LOTID;

                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA_L", "INDATA", "OUTDATA", dt);

                    DataRow[] drTmp4 = result.Select("CBO_CODE = '" + Util.NVC(cboLotID.SelectedValue) + "'");
                    txtNumber.Text = drTmp4?.Length > 0 ? double.Parse(drTmp4[0]["SMPL_QTY"].ToString()).ToString() : "0"; // double.Parse(result.Compute("SUM(SMPL_QTY)", "").ToString()).ToString();

                    _DT_BTCH_LOT_INFO = result.Copy();
                }                
                                
                ////2019.10.18 NND설비는 샘플채취량 초기 수량을 0으로 하기 위해 수정
                //if (!eqgrid.Equals(EquipmentGroup.NND))
                //{
                //    dt = new DataTable();
                //    dt.Columns.Add("AREAID", typeof(string));
                //    dt.Columns.Add("DFCT_GR_CODE", typeof(string));
                //    dt.Columns.Add("PROCID", typeof(string));
                //    dt.Columns.Add("ACTID", typeof(string));
                //    dt.Columns.Add("RESNCODE", typeof(string));

                //    row = dt.NewRow();
                //    row["AREAID"] = LoginInfo.CFG_AREA_ID;
                //    row["DFCT_GR_CODE"] = "ALL";
                //    row["PROCID"] = Process.VD_LMN;
                //    row["ACTID"] = "CHARGE_PROD_LOT";
                //    row["RESNCODE"] = "PS01S04";

                //    dt.Rows.Add(row);
                //    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_PROC_DFCT_CODE", "INDATA", "RSLT", dt);
                //    //BAS_DFCT_QTY는 검사할 수량을 나타낸다.
                //    txtNumber.Text = result.Rows.Count == 0 ? 0.ToString() : (result.Rows[0]["BAS_DFCT_QTY"].ToString().Equals("") ? 0.ToString() : Convert.ToString((int)((decimal)result.Rows[0]["BAS_DFCT_QTY"])));
                //}
                //else
                //{
                //    txtNumber.Text = 0.ToString();
                //}

                //판정결과 cbo초기화
                initCombo();

                dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));  // 2021.10.06 : 동별 특이사항 확인을 위하여 파라미터 추가

                row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["CMCDTYPE"] = "VD_RESN_CODE";
                row["AREAID"] = LoginInfo.CFG_AREA_ID;  // 2021.10.06 : 동별 특이사항 확인을 위하여 파라미터 추가
                dt.Rows.Add(row);

                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMCODE_VD_RESN", "INDATA", "RSLT", dt);

                // 2021.10.06 : 동별 공통코드 관리로 변경함
                ///* 20200924 이종원 */
                ///* CSRID : C20200326-000371 */
                ///* [CNB 조립] 경우만 적용, QA검사 불량사유 중 "수분불량" REWORK -> HOLD로 변경하여 조회되도록 수정 */
                ///* DB데이터 또는 DA를 수정할 경우 그것들을 사용하는 타 사업장에 영향이 있으므로 소스에서 수정 */
                //if (LoginInfo.CFG_SHOP_ID == "G631")         // CNB 조립(G631), 전극(G632)
                //{
                //    for (int i = 0; i < result.Rows.Count; i++)
                //    {
                //        if (result.Rows[i]["CMCODE"].Equals("WF") && result.Rows[i]["RESOLVE"].Equals("REWORK"))
                //            result.Rows[i]["RESOLVE"] = "HOLD";
                //    }
                //}

                //불량사유 목록
                Util.GridSetData(dgResn, result, null, false);

                //HOLD 사유
                string[] sFilter = { "HOLD_LOT" };
                combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter);

                GetLossInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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

                //if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["RESOLVE"].Equals("REWORK"))
                //    grHold.IsEnabled = false;//grHold.Visibility = Visibility.Collapsed; 
                if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["RESOLVE"].Equals("HOLD"))
                    grHold.IsEnabled = true;

                // 20200925 추가
                // [CNB 조립] 경우만 적용
                if (LoginInfo.CFG_SHOP_ID == "G631")
                {
                    if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["CMCODE"].Equals("WF"))
                    {
                        //grHold.IsEnabled = true;
                        //grHold.Visibility = Visibility.Visible;
                        cboHoldType.SelectedValue = "PH01H31";
                        txtHoldResn.Visibility = Visibility.Collapsed;
                    }
                }

                if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["CMCODE"].Equals("DF"))
                {
                    //grHold.IsEnabled = true;
                    //grHold.Visibility = Visibility.Visible;
                    cboHoldType.SelectedValue = "PH01H02";
                    txtHoldResn.Visibility = Visibility.Collapsed;
                }

                else if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && dtRow["CMCODE"].Equals("ETC"))
                {
                    //grHold.IsEnabled = true;
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
            if (Util.NVC(cboLotID.SelectedValue).ToUpper().IndexOf("SELECT") >= 0 || cboLotID.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU3370"); //샘플LOT을 선택해주세요
                return;
            }

            //2019.10.18 NND설비에서 완공된 LOT이면 수량이 0이어도 상관없음.
            if (int.Parse(txtNumber.Text) == 0 && !eqgrid.Equals(EquipmentGroup.NND))
            {
                Util.MessageValidation("SFU3371"); //수량이 0이상이어야 합니다.
                return;
            }
            if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("E") || Convert.ToString(cboQAJUDG.SelectedValue).Equals("SELECT"))
            {
                Util.MessageValidation("SFU3372"); //판정결과를 선택해주세요
                return;
            }

            if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && FailCode == "")
            {
                Util.MessageValidation("SFU1585"); //불량정보가 없습니다.
                return;
            }

            if (!canInsp)
            {
                Util.MessageValidation("SFU7021"); //완공 상태가 아닌 LOT이 존재합니다.
                return;
            }

            // 20200925추가
            // [CNB 조립] 경우만 적용, "HOLD사유" 선택 필수
            // "판정결과" 불합격인 경우만.
            if (LoginInfo.CFG_SHOP_ID == "G631")
            {
                if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F"))
                {
                    if (Convert.ToString(cboHoldType.SelectedValue).Equals("") || Convert.ToString(cboHoldType.SelectedValue).Equals("SELECT"))
                    {
                        Util.MessageValidation("SFU1593"); //사유를 선택하세요
                        return;
                    }
                }
            }


            //1. 물청저장
            //2. 판정결과저장
            //3. 두께불량/ 기타일경우 hold처리
            //3. 수분불량일 경우 재작업처리


            //물청저장
            //검사결과를 저장하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2811"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //QA 저장 + 물청
                        SaveQaJudg();

                        if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && resolve.Equals("REWORK"))
                        {
                            //수분검사일 경우 설비배치ID가 같은 LOT들을 VD예약 상태로 돌림
                            VDReworkLot(Util.NVC(cboLotID.SelectedValue));
                        }
                        else if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && resolve.Equals("HOLD"))
                        {
                            //해당LOT과 배치ID가 같은 LOT Hold처리
                            LotHold(Util.NVC(cboLotID.SelectedValue));
                        }

                        //검사결과저장
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

        private void VDReworkLot(string lotid)
        {
            try
            {
                DataTable result = null;
                DataTable dt = new DataTable();
                DataRow row = null;

                if (gbLotQAInsp == false)   //2017.08.13 Edited By Kim Joonphil
                {
                    //재작업 대상LOT조회

                    dt.Columns.Add("LOTID", typeof(string));

                    row = dt.NewRow();
                    row["LOTID"] = lotid;
                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_REWORK_LOT_L", "RQST", "RSLT", dt); //해당LOT과 같은 배치ID를 가진 LOT조회 (VD재작업 대상)

                    if (result.Rows.Count == 0)
                    {
                        return;
                    }
                }

                DataSet ds = new DataSet();
                dt = ds.Tables.Add("INDATA");
                dt.Columns.Add("SRCTYPE", typeof(string));
                dt.Columns.Add("IFMODE", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("PCSGID", typeof(string));
                dt.Columns.Add("WIPNOTE", typeof(string));
                dt.Columns.Add("PROCID_TO", typeof(string));
                dt.Columns.Add("EQSGID_TO", typeof(string));
                dt.Columns.Add("RE_VD_TYPE_CODE", typeof(string));

                row = dt.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["PCSGID"] = "A"; // 조립
                row["WIPNOTE"] = "";
                row["PROCID_TO"] = Process.VD_LMN;
                row["EQSGID_TO"] = eqsgid;
                row["RE_VD_TYPE_CODE"] = "WF";

                dt.Rows.Add(row);

                row = null;

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("TREATTYPE", typeof(string));
                inLot.Columns.Add("PROCID", typeof(string));

                if (gbLotQAInsp == false)   //2017.08.13 Edited By Kim Joonphil
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = result.Rows[i]["LOTID"];
                        row["TREATTYPE"] = "V";
                        row["PROCID"] = Process.VD_LMN;

                        inLot.Rows.Add(row);
                    }
                }
                else
                {
                    row = inLot.NewRow();
                    row["LOTID"] = lotid;
                    row["TREATTYPE"] = "V";
                    row["PROCID"] = Process.VD_LMN;

                    inLot.Rows.Add(row);
                }
                //BR_PRD_REG_MOVE_LOT_WAIT_VD -> BR_PRD_REG_MOVE_LOT_WAIT_VD_PANCAKE 으로 변경
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_LOT_WAIT_FOR_RW_L", "INDATA, IN_LOT", null, ds);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void LotHold(string lotid)
        {
            try
            {
                if (Convert.ToString(cboHoldType.SelectedValue).Equals(""))
                {
                    Util.MessageValidation("SFU1593"); //사유를 선택하세요
                }
                string sHoldType = Convert.ToString(cboHoldType.SelectedValue);//Util.GetCondition(cboHoldType, "사유를 선택하세요");

                ////해당LOT이 배치ID기준인지/ 대LOT단위 기준인지 판별/ 두번째 검사 LOT인지 아닌지 판별
                DataTable dt_hold = new DataTable();
                dt_hold.Columns.Add("LOTID", typeof(string));

                DataRow row_hold = dt_hold.NewRow();
                row_hold["LOTID"] = lotid;
                dt_hold.Rows.Add(row_hold);

                DataTable result_qa_insp_cond = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_QA_INSP_COND", "RQST", "RSLT", dt_hold);

                if (result_qa_insp_cond.Rows.Count == 0)
                {
                    //기준정보 없음
                }

                DataTable dt = new DataTable();
                DataRow row = null;
                DataTable result = null;

                if (gbLotQAInsp == false)        //2017.08.13 Edited By Kim Joonphil
                {
                    //Hold대상LOT조회

                    dt.Columns.Add("LOTID", typeof(string));
                    dt.Columns.Add("LOTID_RT", typeof(string));

                    row = dt.NewRow();


                    row["LOTID"] = lotid;
                    row["LOTID_RT"] = (!Convert.ToString(result_qa_insp_cond.Rows[0]["QA_INSP_TRGT_FLAG"]).Equals("Y") && Convert.ToString(result_qa_insp_cond.Rows[0]["VD_QA_INSP_COND_CODE"]).Equals("VD_QA_INSP_RULE_02")) ? Convert.ToString(result_qa_insp_cond.Rows[0]["LOTID_RT"]) : null;

                    dt.Rows.Add(row);

                    result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_REWORK_LOT_L", "RQST", "RSLT", dt); //해당LOT과 같은 배치ID를 가진 LOT조회 (VD재작업 대상)


                    if (vd_qa_insp_cond.Equals("VD_QA_INSP_RULE_02") && qa_trgt_flag.Equals("Y")) //대LOT기준일때만
                    {
                        dt = new DataTable();
                        dt.Columns.Add("EQPTID", typeof(string));
                        dt.Columns.Add("WIPSTAT", typeof(string));
                        dt.Columns.Add("PROCID", typeof(string));
                        dt.Columns.Add("LOTID", typeof(string));

                        row = dt.NewRow();
                        row["EQPTID"] = eqptid;
                        row["WIPSTAT"] = Wip_State.END;
                        row["PROCID"] = Process.VD_LMN;
                        row["LOTID"] = lotid;

                        dt.Rows.Add(row);
                        DataTable tmp = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_VD_QA_COATING_LOT", "RQST", "RSLT", dt);

                        row = null;

                        if (tmp.Rows.Count != 0)
                        {
                            for (int i = 0; i < tmp.Rows.Count; i++)
                            {
                                row = result.NewRow();
                                row["LOTID"] = tmp.Rows[i]["LOTID"];
                                result.Rows.Add(row);
                            }
                        }
                    }
                }

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTION_USERID", typeof(string));

                row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["LANGID"] = LoginInfo.LANGID;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["ACTION_USERID"] = txtPersonId.Text;

                inDataTable.Rows.Add(row);

                //대상 LOT

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                if (gbLotQAInsp == false)       //2017.08.13 Edited By Kim Joonphil
                {
                    for (int i = 0; i < result.Rows.Count; i++)
                    {
                        row = inLot.NewRow();
                        row["LOTID"] = result.Rows[i]["LOTID"];
                        row["HOLD_NOTE"] = txtHoldResn.Text.Equals("") ? null : txtHoldResn.Text;
                        row["RESNCODE"] = sHoldType;
                        row["HOLD_CODE"] = sHoldType;
                        row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);

                        inLot.Rows.Add(row);
                    }
                }
                else
                {
                    row = inLot.NewRow();
                    row["LOTID"] = lotid;
                    row["HOLD_NOTE"] = txtHoldResn.Text.Equals("") ? null : txtHoldResn.Text;
                    row["RESNCODE"] = sHoldType;
                    row["HOLD_CODE"] = sHoldType;
                    row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);

                    inLot.Rows.Add(row);
                }

                try
                {
                    //hold 처리
                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, inData);

                    txtPerson.Text = "";
                    txtPersonId.Text = "";
                    txtPersonDept.Text = "";

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
            catch (Exception ex)
            {

            }

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
            row["INSP_DTTM"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            row["INSP_USERID"] = LoginInfo.USERID;
            inData.Tables["INDATA"].Rows.Add(row);

            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("LOTID_RT", typeof(string));
            inLot.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
            inLot.Columns.Add("JUDG_VALUE", typeof(string));
            inLot.Columns.Add("DFCT_CODE", typeof(string));
            inLot.Columns.Add("QA_INSP_COND_LOT_CHECK", typeof(string)); //2017.08.13 Add By Kim Joonphil

            inLot.Columns.Add("ACTID", typeof(string));
            inLot.Columns.Add("RESNCODE", typeof(string));
            inLot.Columns.Add("RESNQTY", typeof(double));
            inLot.Columns.Add("DFCT_TAG_QTY", typeof(Int32));
            inLot.Columns.Add("COST_CNTR_ID", typeof(string));

            row = inLot.NewRow();
            row["LOTID"] = Util.NVC(cboLotID.SelectedValue);
            row["LOTID_RT"] = lotid_rt;
            row["EQPT_BTCH_WRK_NO"] = eqpt_btch_wrk_no;
            row["JUDG_VALUE"] = Convert.ToString(cboQAJUDG.SelectedValue);
            row["DFCT_CODE"] = Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") ? FailCode : null; //20170816 삭제되어 추가
            row["QA_INSP_COND_LOT_CHECK"] = gbLotQAInsp ? "Y" : "N";    //2017.08.13 Add By Kim Joonphil LOT별검사인지 체크

            row["ACTID"] = "CHARGE_PROD_LOT";
            row["RESNCODE"] = resncode;
            row["RESNQTY"] = txtNumber.Text;
            row["DFCT_TAG_QTY"] = 0;
            row["COST_CNTR_ID"] = cost_center;

            inData.Tables["INLOT"].Rows.Add(row);

            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_QAJUDGE_L", "INDATA,INLOT", null, inData);
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
            row["PROCID"] = Process.VD_LMN;
            row["ACTID"] = "CHARGE_PROD_LOT";
            row["DFCT_GR_CODE"] = "ALL";
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MMD_AREA_DFCT_CODE", "INDATA", "RSLT", dt);
            if (result.Rows.Count == 0)
            {
                return;
            }

            resncode = Convert.ToString(result.Rows[0]["RESNCODE"]);
            number = Convert.ToString(result.Rows[0]["PRCS_ITEM_CODE"]);
            cost_center = Convert.ToString(result.Rows[0]["COST_CNTR_ID"]);
        }

        private bool CanConfirm()
        {
            bool bRet = true;
            string lotID = Convert.ToString(cboLotID.SelectedValue);

            DataTable dt = new DataTable();
            DataRow row = null;
            dt.Columns.Add("LOTID", typeof(string));

            row = dt.NewRow();
            row["LOTID"] = lotid;
            dt.Rows.Add(row);

            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_REWORK_LOT_L", "RQST", "RSLT", dt); //해당LOT과 같은 배치ID를 가진 LOT조회 (VD재작업 대상)

            foreach (DataRowView drv in result.DefaultView)
            {
                string stat = DataTableConverter.GetValue(drv, "WIPSTAT") as string;
                if (!stat.Equals("END"))
                {
                    bRet = false;
                    break;
                }
            }

            return bRet;
        }
        #endregion

        private void cboLotID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (_DT_BTCH_LOT_INFO == null) return;

            // 검사 수량
            DataRow[] drTmp4 = _DT_BTCH_LOT_INFO.Select("CBO_CODE = '" + Util.NVC(cboLotID.SelectedValue) + "'");
            txtNumber.Text = drTmp4?.Length > 0 ? double.Parse(drTmp4[0]["SMPL_QTY"].ToString()).ToString() : "0"; // double.Parse(result.Compute("SUM(SMPL_QTY)", "").ToString()).ToString();

        }

        private void txtNumber_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtNumber.Text, 0))
                {
                    txtNumber.Text = "";
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
 