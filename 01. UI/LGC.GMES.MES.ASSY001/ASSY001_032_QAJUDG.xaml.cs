/*************************************************************************************
 Created Date : 2017.01.14
      Creator : 이진선
   Decription : VD QA대상LOT조회 설비 내용
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
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

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_032_QAJUDG : C1Window, IWorkArea
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
        string vd_qa_insp_cond = string.Empty;
        string resncode = string.Empty;
        string number = string.Empty;
        string cost_center = string.Empty;
        DataTable dtRslt = null;
        string resolve = string.Empty;
        bool mbRework = false;
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

        public ASSY001_032_QAJUDG()
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
                    mbRework = !string.IsNullOrEmpty(tmps[7].ToString());
                }

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID_RT", typeof(string));
                dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID_RT"] = vd_qa_insp_cond.Equals("VD_QA_INSP_RULE_02") ? lotid_rt : null;
                row["EQPT_BTCH_WRK_NO"] = eqpt_btch_wrk_no;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA", "INDATA", "OUTDATA", dt);

                cboLotID.ItemsSource = DataTableConverter.Convert(result);

                cboLotID.DisplayMemberPath = "CBO_NAME";
                cboLotID.SelectedValuePath = "CBO_CODE";



                //DataTable dtRqst = new DataTable();
                //dtRqst.Columns.Add("USERNAME", typeof(string));
                //dtRqst.Columns.Add("LANGID", typeof(string));

                //DataRow dr = dtRqst.NewRow();
                //dr["USERNAME"] = null;// txtPerson.Text;
                //dr["LANGID"] = LoginInfo.LANGID;

                //dtRqst.Rows.Add(dr);
                //dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                //foreach (DataRow r in dtRslt.Rows)
                //{
                //    string displayString = r["USERNAME"].ToString();
                //    string[] keywordString = new string[r["USERNAME"].ToString().Length];

                //    for (int i = 0; i < displayString.Length - 1; i++)
                //        keywordString[i] = displayString.Substring(i, txtPerson.Threshold);

                //    keywordString[keywordString.Length - 1] = r["USERID"].ToString();

                //    txtPerson.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString));

                //}

                //           txtPerson.DataContextChanged += txtPerson_DataContextChanged;
                // txtPerson.MouseEnter += txtPerson_MouseEnter;
                //txtPerson.IsKeyboardFocusedChanged += txtPerson_IsKeyboardFocusedChanged;
                //txtPerson.Loaded += txtPerson_Loaded;
                //txtPerson.KeyDown += KeyDown;


                dt = new DataTable();
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("DFCT_GR_CODE", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("RESNCODE", typeof(string));

                row = dt.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["DFCT_GR_CODE"] = "ALL";
                row["PROCID"] = Process.VD_LMN;
                row["ACTID"] = "CHARGE_PROD_LOT";
                row["RESNCODE"] = "PS01S04";

                dt.Rows.Add(row);
                result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_MMD_AREA_PROC_DFCT_CODE", "INDATA", "RSLT", dt);
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
       // private void txtPerson_
        //private void txtPerson_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (sender == null)
        //    {
        //        return;
        //    }
        //}
        //private void txtPerson_MouseEnter(object sender , MouseEventArgs e )
        //{
        //    if (sender == null)
        //    {
        //        return;
        //    }
        //}
        private void txtPerson_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;
        }
     
      
        //private void KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (sender == null) return;
        //    if (e.Key == Key.Enter)
        //    {
        //        txtPersonId.Text = "";
        //    }
        //}
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

                        //foreach (DataRow r in dtRslt.Rows)
                        //{
                        //    string displayString = r["USERNAME"].ToString();
                        //    string[] keywordString = new string[r["USERNAME"].ToString().Length - 1];

                        //    for (int i = 0; i < displayString.Length - 2; i++)
                        //        keywordString[i] = displayString.Substring(i, txtPerson.Threshold);

                        //    txtPerson.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString));
                        //}
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
            if (txtNumber.Text.Equals("0"))
            {
              //  return;
            }

            //if (txtQAUSER.Text.Equals(string.Empty))
            //{
            //    return;
            //}
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
                            VDReworkLot(Convert.ToString(cboLotID.SelectedItems[0]));
                        }
                        else if (Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") && resolve.Equals("HOLD"))
                        {
                            //해당LOT과 배치ID가 같은 LOT Hold처리
                            LotHold(Convert.ToString(cboLotID.SelectedItems[0]));
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
                //재작업 대상LOT조회
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = lotid;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_REWORK_LOT", "RQST", "RSLT", dt); //해당LOT과 같은 배치ID를 가진 LOT조회 (VD재작업 대상)

                if (result.Rows.Count == 0)
                {
                    return;
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

                row = dt.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["USERID"] = LoginInfo.USERID;
                row["PCSGID"] = "A"; // 조립
                row["WIPNOTE"] = "";
                row["PROCID_TO"] = Process.VD_LMN;
                row["EQSGID_TO"] = eqsgid;

                dt.Rows.Add(row);

                row = null;

                DataTable inLot = ds.Tables.Add("IN_LOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("TREATTYPE", typeof(string));
                inLot.Columns.Add("PROCID", typeof(string));

                for (int i = 0; i < result.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = result.Rows[i]["LOTID"];
                    row["TREATTYPE"] = "V";
                    row["PROCID"] = Process.VD_LMN;

                    inLot.Rows.Add(row);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_MOVE_LOT_WAIT_VD", "INDATA, IN_LOT", null, ds);

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

                ////해당LOT이 배치ID기준인지/ 대LOT단위 기준인지 판별
                //DataTable dt = new DataTable();
                //dt.Columns.Add("LOTID", typeof(string));

                //DataRow row = dt.NewRow();
                //row["LOTID"] = lotid;
                //dt.Rows.Add(row);

                ////DataTable result_insp_cond = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_INSP_COND", "RQST", "RSLT", dt);

                //if (result_insp_cond.Rows.Count == 0)
                //{
                //    //기준정보 없음
                //}


                //Hold대상LOT조회

                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LOTID"] = lotid;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_VD_REWORK_LOT", "RQST", "RSLT", dt); //해당LOT과 같은 배치ID를 가진 LOT조회 (VD재작업 대상)


                if (vd_qa_insp_cond.Equals("VD_QA_INSP_RULE_02")) //대LOT기준일때만
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

                //if (result.Rows.Count == 0)
                //{
                //    return;
                //}


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
                    Util.AlertByBiz("BR_PRD_REG_HOLD_LOT", ex.Message, ex.ToString());

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
            inDataTable.Columns.Add("REWORKFLAG", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["EQPTID"] = eqptid;
            row["USERID"] = LoginInfo.USERID;
            row["INSP_DTTM"] = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            row["INSP_USERID"] = LoginInfo.USERID;
            row["REWORKFLAG"] = mbRework;
            //DataTable person = new DataTable();
            //person.Columns.Add("USERNAME", typeof(string));
            //person.Columns.Add("LANGID", typeof(string));

            //DataRow dr = person.NewRow();
            //dr["USERNAME"] = txtQAUSER.Text;
            //dr["LANGID"] = LoginInfo.LANGID;
            //person.Rows.Add(dr);
            //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", person);

            //row["INSP_USERID"] = dtRslt.Rows.Count == 0 ? dtRslt.Rows[0][""]

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
                row["DFCT_CODE"] = Convert.ToString(cboQAJUDG.SelectedValue).Equals("F") ? FailCode : null;

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
                dr["ACTID"] = "CHARGE_PROD_LOT";
                dr["RESNCODE"] = resncode;
                dr["RESNQTY"] = txtNumber.Text;
                dr["DFCT_TAG_QTY"] = 0;
                dr["LANE_QTY"] = 1;
                dr["LANE_PTN_QTY"] = 1;
                dr["COST_CNTR_ID"] = cost_center;
                IndataTable.Rows.Add(dr);
            }

            //BR_PRD_REG_VDQA_QAJUDGE
            //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_QAJUDGE_VDQA", "INDATA,IN_LOT, INRSEN", null, inData);
            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_VDQA_QAJUDGE", "INDATA,IN_LOT, INRSEN", null, inData);

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

























        #endregion
    }
}