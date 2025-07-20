/*************************************************************************************
 Created Date : 2020.11.23
      Creator : Kang Dong Hee
   Decription : 활성화 Lot 관리
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.23  NAME   : Initial Created
  2021.03.31  KDH    : 변경사유 추가
  2021.04.01  KDH    : CommonCombo 참조에서 CommonCombo_Form 참조로 변경
  2021.04.06  KDH    : Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID)
  2021.04.18  KDH    : 검색조건 제거(작업일) 및 컬럼 추가(CST ID, 재공상태)
  2021.04.20  KDH    : 공통 Line에 대해 공정 List Setting되게 로직 추가
  2021.04.26  KDH    : 검색조건 추가 및 자동 체크 기능 추가 (Cell ID)
  2022.05.27  이정미 : 동 콤보박스 수정 
  2022.07.25  KDH    : C20220603-000198_버튼별 권한 체크 로직 추가
  2022.08.09  조영대 : 설비콤보 기본설정(Setting) 안되게 수정.
  2022.08.26  조영대 : Cell ID 입력 조회 시 선택된 Lot 을 최상위로 이동  
  2022.08.31  강동희 : Cell ID 입력/조회 후 Cell ID Block 설정
  2022.09.01  조영대 : 공정콤보박스 ALL 추가
  2022.09.02  김령호 : Aging 공정시에 Line 조건 무시하도록 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_096 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        private DataTable dtTemp = new DataTable();
        Util _Util = new Util();
        private string sWorkReSetTime = string.Empty;
        private string sWorkEndTime = string.Empty;
        private string LOTID = string.Empty;
        private string LOTTYPE = string.Empty;
        private string DFCT_TYPE_CODE = string.Empty;
        private string DFCT_GR_CODE = string.Empty;
        private string PROC_DETL_CODE = string.Empty;

        private bool IsLoading = true;
        CommonCombo_Form_MB ComCombo = new CommonCombo_Form_MB();

        DataView _dvLEVEL1 { get; set; }
        DataView _dvLEVEL2 { get; set; }
        DataView _dvLEVEL3 { get; set; }

        #endregion

        #region [Initialize]
        public FCS002_096()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnReCheckProc);
            listAuth.Add(btnScrapStandbyProc);
            listAuth.Add(btnScrapProc);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            SetWorkResetTime();
            //Combo Setting            
            InitCombo();
            //Control Setting
            InitControl();

            //다른 화면에서 넘어온 경우
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                txtGrpLotID.Text = Util.NVC(parameters[0]);
                GetLotInfo();
            }

            IsLoading = false;
            this.Loaded -= UserControl_Loaded;
        }

        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");

            C1ComboBox[] cboLineChild = { cboProcGrpCode };
            ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);

            //C1ComboBox[] cboTypeChild = { cboLine };
            //string[] sFilter3 = { "FORM_CELL_TYPE_MB" };
            //ComCombo.SetCombo(cboType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", cbChild: cboTypeChild, sFilter: sFilter3);

            //C1ComboBox[] cboLineParent = { cboType };
            //C1ComboBox[] cboLineChild = { cboProcGrpCode };
            //ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE_BY_CELLTYPE", cbChild: cboLineChild, cbParent: cboLineParent);
                       
            // 공정 그룹
            C1ComboBox[] cboProcGrParent = { cboLine };
            C1ComboBox[] cboProcGrChild = { cboProcess };

            string ProcGR = GetProcGR();
           // string[] sFilterProcGR = { "PROC_GR_CODE_MB", Util.NVC(cboArea.SelectedValue), ProcGR };
            string[] sFilterProcGR = { "PROC_GR_CODE_MB", Util.NVC(cboArea.SelectedValue) };
            ComCombo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.SELECT, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sFilterProcGR, sCase: "PROCGRP_BY_LINE");

            // 공정
            C1ComboBox[] cboProcParent = { cboArea, cboLine, cboProcGrpCode };
            C1ComboBox[] cboProcChild = { cboEquipment };
            ComCombo.SetCombo(cboProcess, CommonCombo_Form_MB.ComboStatus.SELECT, cbChild: cboProcChild, cbParent: cboProcParent, sCase: "PROC_BY_PROCGRP");

            // 설비 
            C1ComboBox[] cboEqpParent = { cboArea, cboLine, cboProcGrpCode, cboProcess };
            string[] sFilterEqp = { "M" };
            ComCombo.SetCombo(cboEquipment, CommonCombo_Form_MB.ComboStatus.ALL, cbParent: cboEqpParent,sFilter: sFilterEqp, sCase: "EQP_BY_PROC");

            // Lot 유형
            string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
           ComCombo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter1, sCase: "FORM_CMN");
        }

        private void InitControl()
        {
            // Util 에 해당 함수 추가 필요.
            //dtpFromDate.SelectedDateTime = GetJobDateFrom(); //2021.04.18 검색조건 제거(작업일)
            //dtpToDate.SelectedDateTime = GetJobDateTo(); //2021.04.18 검색조건 제거(작업일)
        }

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
        private bool IsAreaCommoncodeAttrUse(string sCodeType, string sCodeName, string[] sAttribute)
        {
            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = !string.IsNullOrEmpty(sCodeName) ? sCodeName : null;
                dr["USE_FLAG"] = 'Y';
                for (int i = 0; i < sAttribute.Length; i++)
                    dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
        private bool CheckButtonPermissionGroupByBtnGroupID(string sBtnGrpID, string sFormID)
        {
            try
            {
                bool bRet = false;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("FORMID", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["USERID"] = LoginInfo.USERID;
                dtRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRow["FORMID"] = sFormID;
                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_BTN_PERMISSION_GRP_FORM", "INDATA", "OUTDATA", inTable);

                if (CommonVerify.HasTableRow(dtRslt))
                {
                    DataRow[] drs = dtRslt.Select("BTN_PMS_GRP_CODE = '" + sBtnGrpID + "'");
                    if (drs?.Length > 0)
                        bRet = true;
                }

                if (bRet == false)
                {
                    string objectmessage = string.Empty;

                    if (sBtnGrpID == "SCRAP_PROC_W")
                        objectmessage = ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL");

                    Util.MessageValidation("SFU3520", LoginInfo.USERID, objectmessage);     // 해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END
        private void initScreen()
        {
            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
            rdoReCheck.IsEnabled = false;
            rdoScpStdb.IsEnabled = false;
            rdoScrap.IsEnabled = false;
            rdoReCheck.IsChecked = false;
            rdoScpStdb.IsChecked = false;
            rdoScrap.IsChecked = false;
            rdoReCheck.Foreground = new SolidColorBrush(Colors.Black);
            rdoScpStdb.Foreground = new SolidColorBrush(Colors.Black);
            rdoScrap.Foreground = new SolidColorBrush(Colors.Black);
            rdoReCheck.FontWeight = FontWeights.Normal;
            rdoScpStdb.FontWeight = FontWeights.Normal;
            rdoScrap.FontWeight = FontWeights.Normal;
            btnReCheckProc.IsEnabled = false;
            btnScrapStandbyProc.IsEnabled = false;
            btnScrapProc.IsEnabled = false;
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                initScreen();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCGRID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("FROM_WIPDTTM_ST", typeof(string));
                inTable.Columns.Add("TO_WIPDTTM_ST", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = cboArea.GetBindValue();
                //상온, 고온, 출하 Aging 선택인 경우 무시.
                //if (cboProcGrpCode.GetStringValue() == "3" || cboProcGrpCode.GetStringValue() == "4" || cboProcGrpCode.GetStringValue() == "7" || cboProcGrpCode.GetStringValue() == "9")
                //{
                //    newRow["EQSGID"] = null;
                //}
                //else
                //{
                newRow["EQSGID"] = Util.GetCondition(cboLine, sMsg: "SFU1223");  //라인을 선택해주세요.
                if (string.IsNullOrEmpty(newRow["EQSGID"].ToString())) return;
                //}

                newRow["PROCGRID"] = cboProcGrpCode.GetBindValue();
                newRow["PROCID"] = cboProcess.GetBindValue();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboEquipment);
                newRow["PROD_LOTID"] = txtPkgLotID.Text == string.Empty ? null : txtPkgLotID.Text;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(cboLotType.SelectedValue).Equals(string.Empty) ? null : Util.GetCondition(cboLotType);
                newRow["PRODID"] = txtProd.Text == string.Empty ? null : txtProd.Text;
                //newRow["FROM_WIPDTTM_ST"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd"); //2021.04.18 검색조건 제거(작업일)
                //newRow["TO_WIPDTTM_ST"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd"); //2021.04.18 검색조건 제거(작업일)

                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RECHECK_NG_DRB_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            List<DataRow> irows = result.AsEnumerable().Where(r => r["CHK"].Equals(true)).ToList();
                            if (irows.Count > 0)
                            {
                                for (int i = 0; i < irows.Count; i++)
                                {
                                    irows[i]["CHK"] = false;
                                    result = result.DefaultView.ToTable();
                                }
                            }

                            #region 선택된 로우 최상단으로 이동
                            if (!string.IsNullOrEmpty(txtGrpLotID.Text.Trim()))
                            {
                                List<DataRow> rows = result.AsEnumerable().Where(r => r["LOTID"].Equals(Util.NVC(txtGrpLotID.Text))).ToList();
                                if (rows.Count > 0)
                                {
                                    rows[0]["CHK"] = true;
                                    result.DefaultView.Sort = "CHK DESC";
                                    result = result.DefaultView.ToTable();
                                }
                            }
                            else if (!string.IsNullOrEmpty(txtTrayID.Text.Trim()))
                            {
                                List<DataRow> rows = result.AsEnumerable().Where(r => r["CSTID"].Equals(Util.NVC(txtTrayID.Text))).ToList();
                                if (rows.Count > 0)
                                {
                                    rows[0]["CHK"] = true;
                                    result.DefaultView.Sort = "CHK DESC";
                                    result = result.DefaultView.ToTable();
                                }
                            }

                            #endregion

                            Util.GridSetData(dgReCheckNGLotList, result, FrameOperation, true);
                            if (result.Rows.Count == 1)
                            {
                                string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[0].DataItem, "LOTID"));
                                dgReCheckNGLotList.SelectedIndex = 0;
                            }

                            if (!string.IsNullOrEmpty(txtSubLotID.Text))
                            {
                                txtSubLotID.SelectAll();
                                txtSubLotID.Focus();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private string GetProcGR()
        {
            try
            {
                string sProcGR = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "FORM_LOT_MGR_COMBO_MB";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_BY_TYPE_CODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count>0)
                {
                    for(int i=0; i<dtResult.Rows.Count; i++)
                    {
                        if (i != 0)
                            sProcGR = sProcGR + ",";
                        sProcGR = sProcGR + dtResult.Rows[i]["CBO_CODE"].ToString();
                    }

                    return sProcGR;
                }

                return sProcGR;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return string.Empty;
            }
        }


        private void GetDetailInfo(string LOTID)
        {
            try
            {
                _dvLEVEL1 = null;
                Util.gridClear(dgCellIDDetail);
                txtResnNoteSubLot.Text = string.Empty;

                _dvLEVEL1 = SetGridCboItem_CommonCodeLevel1(dgCellIDDetail.Columns["DFCT_GR_TYPE_CODE_LV1"], "FORM_DFCT_GR_TYPE_CODE", "FORM_DFCT_GR_TYPE_CODE_MB", Util.NVC(DFCT_GR_CODE)).DefaultView;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LOTID"] = Util.NVC(LOTID);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_FCS_RN_CELL_DETAIL_DRB_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            DataTable dtTemp = new DataTable();
                            dtTemp.TableName = "IN_REQ_TRF_INFO";
                            dtTemp.Columns.Add("ROW_NUM", typeof(string));
                            dtTemp.Columns.Add("CHK", typeof(string));
                            dtTemp.Columns.Add("SUBLOTID", typeof(string));
                            dtTemp.Columns.Add("VENTID", typeof(string));
                            dtTemp.Columns.Add("CANID", typeof(string));
                            dtTemp.Columns.Add("SUBLOTJUDGE", typeof(string));
                            dtTemp.Columns.Add("UPDDTTM", typeof(string));
                            dtTemp.Columns.Add("DFCT_GR_TYPE_CODE_LV1", typeof(string));
                            dtTemp.Columns.Add("DFCT_ITEM_CODE_LV2", typeof(string));
                            dtTemp.Columns.Add("DEFECT_ID_LV3", typeof(string));
                            dtTemp.Columns.Add("SUBLOT_GRD_CODE", typeof(string));
                            dtTemp.Columns.Add("SUBLOT_POSITION", typeof(string));

                            foreach (DataRow drSubLot in result.Rows)
                            {
                                DataRow drIn = dtTemp.NewRow();
                                drIn["ROW_NUM"] = Util.NVC(drSubLot["ROW_NUM"]);
                                drIn["CHK"] = Util.NVC(drSubLot["CHK"]);
                                drIn["SUBLOTID"] = Util.NVC(drSubLot["SUBLOTID"]);
                                drIn["VENTID"] = Util.NVC(drSubLot["VENTID"]);
                                drIn["CANID"] = Util.NVC(drSubLot["CANID"]);
                                drIn["SUBLOTJUDGE"] = Util.NVC(drSubLot["SUBLOTJUDGE"]);
                                drIn["UPDDTTM"] = Util.NVC(drSubLot["UPDDTTM"]);
                                drIn["DFCT_GR_TYPE_CODE_LV1"] = Util.NVC(drSubLot["DFCT_GR_TYPE_CODE_LV1"]);
                                drIn["DFCT_ITEM_CODE_LV2"] = Util.NVC(drSubLot["DFCT_ITEM_CODE_LV2"]);
                                drIn["DEFECT_ID_LV3"] = Util.NVC(drSubLot["DEFECT_ID_LV3"]);
                                drIn["SUBLOT_GRD_CODE"] = Util.NVC(drSubLot["SUBLOT_GRD_CODE"]);
                                drIn["SUBLOT_POSITION"] = Util.NVC(drSubLot["SUBLOT_POSITION"]);

                                //if (!string.IsNullOrEmpty(Util.NVC(drSubLot["CSTSLOT"])))
                                //{
                                //    int cellNo = Convert.ToInt32(Util.NVC(drSubLot["CSTSLOT"]));

                                //    int i = 65 + cellNo / 16;
                                //    if (cellNo % 16 == 0)
                                //    {
                                //        cellNo = 16;
                                //        i--;
                                //    }
                                //    else
                                //    {
                                //        cellNo = cellNo % 16;
                                //    }
                                //    char unicode = (char)i;
                                //    drIn["SUBLOT_POSITION"] = Util.NVC(String.Format("{0}{1:0#}", unicode, cellNo));
                                //}
                                //else
                                //{
                                //    drIn["SUBLOT_POSITION"] = string.Empty;
                                //}
                                dtTemp.Rows.Add(drIn);
                            }

                            Util.GridSetData(dgCellIDDetail, dtTemp, FrameOperation, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        // 공통함수로 뺄지 확인 필요 START
        private DateTime GetJobDateFrom(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkReSetTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkReSetTime, "yyyyMMdd HHmmss", null);
            return dJobDate;
        }

        private DateTime GetJobDateTo(int iDays = 0)
        {
            string sJobDate = string.Empty;

            if (Convert.ToInt32(DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString()) < Convert.ToInt32(sWorkEndTime.Substring(0, 4)))
            {
                sJobDate = DateTime.Now.ToString("yyyyMMdd");
            }
            else
            {
                sJobDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
            }

            DateTime dJobDate = DateTime.ParseExact(sJobDate + " " + sWorkEndTime, "yyyyMMdd HHmmss", null).AddSeconds(-1);
            return dJobDate;
        }

        private void SetWorkResetTime()
        {
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREAATTR_FOR_OPENTIME", "RQSTDT", "RSLTDT", dtRqst);
            sWorkReSetTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
            sWorkEndTime = dtRslt.Rows[0]["OPEN_TIME"].ToString();
        }
        // 공통함수로 뺄지 확인 필요 END

        private void SaveScrapSubLotProcess(string sLotDetlTypeCode)
        {
            try
            {
                // DATA SET 
                DataTable dtScrap = DataTableConverter.Convert(dgCellIDDetail.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtScrap == null || dtScrap.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string)); //20210331 변경사유 추가
                inTable.Columns.Add("REMARKS_CNTT", typeof(string));
                inTable.Columns.Add("MENUID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["IFMODE"] = "OFF";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOT_DETL_TYPE_CODE"] = Util.NVC(sLotDetlTypeCode);
                newRow["REMARKS_CNTT"] = Util.NVC(txtResnNoteSubLot.Text); //20210331 변경사유 추가
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                inTable.Rows.Add(newRow);

                DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_ITEM_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_CODE", typeof(string));

                for (int i = 0; i < dgCellIDDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("1"))
                    {

                        if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DFCT_GR_TYPE_CODE_LV1")) == string.Empty ||
                            Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DFCT_ITEM_CODE_LV2")) == string.Empty ||
                            Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DEFECT_ID_LV3")) == string.Empty )
                        {
                            // 선택된 작업대상이 없습니다.
                            Util.MessageValidation("FM_ME_0149");
                            return;
                        }
                        DataRow newRowSubLot = inSubLot.NewRow();

                        string sSubLotID = Util.Convert_CellID(Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "SUBLOTID")));

                        newRowSubLot["SUBLOTID"] = sSubLotID;
                        newRowSubLot["DFCT_GR_TYPE_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DFCT_GR_TYPE_CODE_LV1"));
                        newRowSubLot["DFCT_ITEM_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DFCT_ITEM_CODE_LV2"));
                        newRowSubLot["DFCT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "DEFECT_ID_LV3"));
                        inSubLot.Rows.Add(newRowSubLot);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_SET_SUBLOT_TRANSFER_LOT_DETL_TYPE_MB", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        GetDetailInfo(LOTID);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet, menuid: FrameOperation.MENUID);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveScrapLotProcess()
        {
            try
            {
                // DATA SET 
                DataTable dtScrap = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtScrap == null || dtScrap.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataTable dtScrapLot = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                DataTable dtDetalALLCell = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);

                DataTable dtDetalChkCell = DataTableConverter.Convert(dgCellIDDetail.ItemsSource).Select("CHK = True").ToDataTable();
                if (dtDetalChkCell == null || dtDetalChkCell.Rows.Count == 0 || dtDetalALLCell.Rows.Count != dtDetalChkCell.Rows.Count)
                {
                    // 실물 폐기 처리시 전체 Cell을 선택해야 합니다.
                    Util.MessageValidation("FM_ME_0422");
                    return;
                }

                int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("MENUID", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID"));
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                inTable.Rows.Add(newRow);

                for (int i = 0; i < dgCellIDDetail.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        DataRow dr = inSubLot.NewRow();
                        string sSubLotID = Util.Convert_CellID(Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[i].DataItem, "SUBLOTID")));

                        dr["SUBLOTID"] = sSubLotID;
                        inSubLot.Rows.Add(dr);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_SET_SCRAP_SUBLOT_MB", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        // 재조회
                        GetList();

                        DataTable dt = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                        dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
                        dt.AcceptChanges();
                        Util.GridSetData(dgReCheckNGLotList, dt, null, true);
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
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private DataTable SetGridCboItem_CommonCodeLevel1(C1.WPF.DataGrid.DataGridColumn col, string CMCDTYPE, string COM_TYPE_CODE, string sCmnCd = null)
        {
            try
            {
                DataTable RSLTDT = new DataTable();
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = Util.NVC(COM_TYPE_CODE);
                dr["CMCDTYPE"] = Util.NVC(CMCDTYPE);
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_1LEVEL_DFCT_CODE_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetGridCboItem_CommonCodeLevel2(C1.WPF.DataGrid.DataGridColumn col, string sCmcdtype, string sLotID, string sDfcttypeCode = null, string sDfctGrtypeCode = null)
        {
            try
            {
                DataTable RSLTDT = new DataTable();
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCmcdtype;
                dr["DFCT_TYPE_CODE"] = sDfcttypeCode;
                if (!string.IsNullOrEmpty(sDfctGrtypeCode))
                {
                    dr["DFCT_GR_TYPE_CODE"] = sDfctGrtypeCode;
                }
                if (!string.IsNullOrEmpty(sLotID))
                {
                    dr["LOTID"] = sLotID;
                }

                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_FORM_2LEVEL_DFCT_CODE_CBO_MB", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable SetDfctCode(string sDfctGrpTypeCode, string sDfctItemCode, string sDfctTypeCode)
        {
            try
            {
                DataTable RSLTDT = new DataTable();
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_ITEM_CODE", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["DFCT_GR_TYPE_CODE"] = sDfctGrpTypeCode;
                dr["DFCT_ITEM_CODE"] = sDfctItemCode;
                dr["DFCT_TYPE_CODE"] = sDfctTypeCode;
                RQSTDT.Rows.Add(dr);

                RSLTDT = new ClientProxy().ExecuteServiceSync("DA_SEL_COMBO_FORM_DFCT_CODE_MB", "RQSTDT", "RSLTDT", RQSTDT);

                return RSLTDT;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void GetLotInfo()
        {
            try
            {
                initScreen();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTID"] = txtTrayID.Text == string.Empty ? null : Util.NVC(txtTrayID.Text);
                newRow["LOTID"] = txtGrpLotID.Text == string.Empty ? null : Util.NVC(txtGrpLotID.Text);

                string sGrpLotID = txtGrpLotID.Text; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                string sTrayID = txtTrayID.Text; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                inTable.Rows.Add(newRow);

                //ShowLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                new ClientProxy().ExecuteService("DA_SEL_GROUP_LOT_INFO_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (result.Rows[0]["PROC_GRP"] == null)
                            {
                                //Util.MessageInfo("FM_ME_0504");
                                return;
                            }

                            cboArea.SelectedValue = Util.NVC(result.Rows[0]["AREAID"]);
                            cboLine.SelectedValue = Util.NVC(result.Rows[0]["EQSGID"]);
                            cboProcGrpCode.SelectedValue = Util.NVC(result.Rows[0]["PROC_GRP"]);
                            cboProcess.SelectedValue = Util.NVC(result.Rows[0]["PROCID"]);
                            //dtpFromDate.SelectedDateTime = DateTime.Parse(Util.NVC(result.Rows[0]["CALDATE"])); //2021.04.18 검색조건 제거(작업일)
                            txtGrpLotID.Text = sGrpLotID; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                            txtTrayID.Text = sTrayID; //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)

                            GetList();
                        }
                    }
                    catch (Exception ex)
                    {
                        //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                    }
                });

            }
            catch (Exception ex)
            {
                //HiddenLoadingIndicator(); //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID)
                Util.MessageException(ex);
            }
        }

        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
        private void GetSubLotInfo()
        {
            try
            {
                string sSubLotID = Util.Convert_CellID(txtSubLotID.Text == string.Empty ? null : Util.NVC(txtSubLotID.Text));

                initScreen();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SUBLOTID"] = sSubLotID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_SEL_SUBLOT_INFO", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (result.Rows[0]["PROC_GRP"] == null)
                            {
                               // Util.MessageInfo("FM_ME_0504");
                                return;
                            }

                            cboArea.SelectedValue = Util.NVC(result.Rows[0]["AREAID"]);
                            cboLine.SelectedValue = Util.NVC(result.Rows[0]["EQSGID"]);
                            cboProcGrpCode.SelectedValue = Util.NVC(result.Rows[0]["PROC_GRP"]);
                            cboProcess.SelectedValue = Util.NVC(result.Rows[0]["PROCID"]);
                            txtGrpLotID.Text = Util.NVC(result.Rows[0]["LOTID"]);
                            txtSubLotID.Text = Util.NVC(sSubLotID);

                         
                            GetList();
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END



        //private string Convert_CellID(string sInputID)
        //{

        //    string sCellID = string.Empty;

        //    if (string.IsNullOrEmpty(sInputID))
        //    {
        //        return string.Empty;
        //    }


        //    DataTable dtRqst = new DataTable();
        //    dtRqst.TableName = "RQSTDT";
        //    dtRqst.Columns.Add("INPUTID", typeof(string));

        //    DataRow dr = dtRqst.NewRow();
        //    dr["INPUTID"] = sInputID;
        //    dtRqst.Rows.Add(dr);

        //    ShowLoadingIndicator();
        //    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CONV_SUBLOTID_MB", "RQSTDT", "RSLTDT", dtRqst);

        //    if (dtRslt.Rows.Count == 0)
        //        return string.Empty;

        //    sCellID = dtRslt.Rows[0]["SUBLOTID"].ToString();

        //    return sCellID;
        //}
        #endregion

        #region [Event]
        // 체크박스 관련
        #region
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        private void dgReCheckNGLotList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgReCheckNGLotList.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgReCheckNGLotList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgReCheckNGLotList.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        #endregion

        private void cboArea_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            // Clear
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;

            if (cboLine.Items.Count == 0)
            {
                cboLine.Text = string.Empty;
            }

            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
        }

        private void cboLine_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            // Clear
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;

            if (cboProcGrpCode.Items.Count == 0)
            {
                cboProcGrpCode.Text = string.Empty;
            }

            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
        }

        private void cboProcGrpCode_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            // Clear
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;

            if (cboProcess.Items.Count == 0)
            {
                cboProcess.Text = string.Empty;
            }

            DFCT_GR_CODE = cboProcGrpCode.GetStringValue("DFCT_GR_CODE");

            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
        }

        private void cboProcess_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            // Clear
            txtProd.Text = string.Empty;
            txtPkgLotID.Text = string.Empty;
            txtGrpLotID.Text = string.Empty;
            txtSubLotID.Text = string.Empty;

            if (cboEquipment.Items.Count == 0)
            {
                cboEquipment.Text = string.Empty;
            }

            PROC_DETL_CODE = cboProcess.GetStringValue("PROC_DETL_CODE");

            if (!string.IsNullOrEmpty(Util.NVC(cboProcGrpCode.SelectedValue)) && Util.NVC(cboProcGrpCode.SelectedValue).Equals("6"))
            {
                if (!string.IsNullOrEmpty(PROC_DETL_CODE) && PROC_DETL_CODE.Equals("6C"))
                {
                    DFCT_GR_CODE = "L";
                }
                else
                {
                    DFCT_GR_CODE = cboProcGrpCode.GetStringValue("DFCT_GR_CODE");
                }
            }

            Util.gridClear(dgReCheckNGLotList);
            Util.gridClear(dgCellIDDetail);
        }

        private void txtPkgLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtPkgLotID.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        private void cboLotType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!IsLoading)
            {
                btnSearch_Click(null, null);
            }
        }

        private void txtProd_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtProd.Text)) && (e.Key == Key.Enter))
            {
                btnSearch_Click(null, null);
            }
        }

        //2021.04.06 Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID) START
        private void txtGrpLotID_KeyDown(object sender, KeyEventArgs e)
        {
            //if ((!string.IsNullOrEmpty(txtGrpLotID.Text)) && (e.Key == Key.Enter))
            //{
            //    GetLotInfo();
            //}
        }
        //2021.04.06 Line별 공정그룹 Set 수정 및 검색조건 추가(Group Lot ID) END

        private void txtGrpLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtGrpLotID.Text)) && (e.Key == Key.Enter))
            {
                GetLotInfo();
            }
        }

        private void txtTrayID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtTrayID.Text)) && (e.Key == Key.Enter))
            {
                GetLotInfo();
            }
        }

        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
        private void txtSubLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtSubLotID.Text)) && (e.Key == Key.Enter))
            {
                GetSubLotInfo();
            }
        }
        //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
            if (!string.IsNullOrEmpty(txtSubLotID.Text))
            {
                GetSubLotInfo();
            }
            else if (!string.IsNullOrEmpty(txtGrpLotID.Text) || !string.IsNullOrEmpty(txtTrayID.Text))
            {
                GetLotInfo();
            }
            else
            {
                GetList();
            }
            //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util _Util = new Util();

                if (dgReCheckNGLotList.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }

                int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                if (iRow < 0)
                {
                    Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOT_DETL_TYPE_CODE")).Equals("G"))
                {
                    string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID")).ToString();

                    // 프로그램 ID 확인 후 수정
                    object[] parameters = new object[6];
                    parameters[0] = string.Empty;
                    parameters[1] = sTrayNo;
                    parameters[2] = string.Empty;
                    parameters[3] = string.Empty;
                    parameters[4] = string.Empty;
                    parameters[5] = "Y";
                    this.FrameOperation.OpenMenu("SFU010710300", true, parameters); //Tray 정보조회 연계
                }
                else
                {
                    object[] parameters = new object[1];
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[iRow].DataItem, "LOTID"));   //TRAYNO
                    this.FrameOperation.OpenMenu("SFU010710320", true, parameters);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgReCheckNGLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReCheckNGLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("LOTID"))
                    {
                        string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.CurrentRow.DataItem, "LOTID"));

                        DataTable dt = DataTableConverter.Convert(dgReCheckNGLotList.ItemsSource);
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (dr["LOTID"].Equals(sLotID))
                            {
                                dr["CHK"] = true;
                            }
                            else
                            {
                                dr["CHK"] = false;
                            }
                        }
                        dgReCheckNGLotList.ItemsSource = DataTableConverter.Convert(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }

        }

        private void dgReCheckNGLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.Cursor = Cursors.Hand;

                        int iRow = _Util.GetDataGridRowIndex(dgReCheckNGLotList, "CHK", "True");

                        if (iRow < 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgReCheckNGLotList.Rows[e.Cell.Row.Index].DataItem, "LOTID")).Equals(Util.NVC(txtGrpLotID.Text)))
                            {
                                DataTableConverter.SetValue(dgReCheckNGLotList.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }
                }
            }));
        }

        private void dgReCheckNGLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            LOTID = string.Empty;
            LOTTYPE = string.Empty;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                LOTID = Util.NVC((rb.DataContext as DataRowView).Row["LOTID"]);
                LOTTYPE = Util.NVC((rb.DataContext as DataRowView).Row["LOT_DETL_TYPE_CODE"]);
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                if (dg != null)
                {

                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                        {
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        }
                        else
                        {
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                        }
                    }
                }

                dgReCheckNGLotList.SelectedIndex = idx;

                rdoReCheck.Foreground = new SolidColorBrush(Colors.Black);
                rdoScpStdb.Foreground = new SolidColorBrush(Colors.Black);
                rdoScrap.Foreground = new SolidColorBrush(Colors.Black);

                rdoReCheck.FontWeight = FontWeights.Normal;
                rdoScpStdb.FontWeight = FontWeights.Normal;
                rdoScrap.FontWeight = FontWeights.Normal;

                if (LOTTYPE.Equals("G"))
                {
                    btnScrapProc.IsEnabled = false;

                    rdoReCheck.IsChecked = true;
                    rdoReCheck.IsEnabled = true;
                    rdoReCheck.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    rdoReCheck.FontWeight = FontWeights.Bold;

                    rdoScpStdb.IsChecked = false;
                    rdoScpStdb.IsEnabled = true;
                    rdoScpStdb.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    rdoScpStdb.FontWeight = FontWeights.Bold;

                    rdoScrap.IsChecked = false;
                    rdoScrap.IsEnabled = false;
                }
                else if (LOTTYPE.Equals("R"))
                {
                    btnScrapProc.IsEnabled = false;

                    rdoReCheck.IsChecked = false;
                    rdoReCheck.IsEnabled = false;

                    rdoScpStdb.IsChecked = true;
                    rdoScpStdb.IsEnabled = true;
                    rdoScpStdb.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    rdoScpStdb.FontWeight = FontWeights.Bold;

                    rdoScrap.IsChecked = false;
                    rdoScrap.IsEnabled = false;
                }
                else if (LOTTYPE.Equals("N"))
                {
                    btnScrapProc.IsEnabled = true;

                    rdoReCheck.IsChecked = false;
                    rdoReCheck.IsEnabled = false;

                    rdoScpStdb.IsChecked = false;
                    rdoScpStdb.IsEnabled = false;

                    rdoScrap.IsChecked = true;
                    rdoScrap.IsEnabled = true;
                    rdoScrap.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    rdoScrap.FontWeight = FontWeights.Bold;
                }
                else if (LOTTYPE.Equals("H"))
                {
                    btnScrapProc.IsEnabled = false;

                    rdoReCheck.IsChecked = false;
                    rdoReCheck.IsEnabled = false;

                    rdoScpStdb.IsChecked = true;
                    rdoScpStdb.IsEnabled = true;
                    rdoScpStdb.Foreground = new SolidColorBrush(Colors.LimeGreen);
                    rdoScpStdb.FontWeight = FontWeights.Bold;

                    rdoScrap.IsChecked = false;
                    rdoScrap.IsEnabled = false;
                }
                else
                {
                    btnReCheckProc.IsEnabled = false;
                    btnScrapStandbyProc.IsEnabled = false;
                    btnScrapProc.IsEnabled = false;

                    rdoReCheck.IsChecked = false;
                    rdoReCheck.IsEnabled = false;

                    rdoScpStdb.IsChecked = false;
                    rdoScpStdb.IsEnabled = false;

                    rdoScrap.IsChecked = false;
                    rdoScrap.IsEnabled = false;
                }

                GetDetailInfo(LOTID);
            }
        }

        private void dgCellIDDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;

                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
        }

        private void dgCellIDDetail_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) START
                    if (e.Cell.Column.Name != null && e.Cell.Column.Name.ToString() == "SUBLOTID")
                    {
                        int iRow = _Util.GetDataGridRowIndex(dgCellIDDetail, "CHK", "True");

                        if (iRow < 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[e.Cell.Row.Index].DataItem, "SUBLOTID")).Equals(Util.NVC(txtSubLotID.Text)))
                            {
                                DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                            }
                        }
                    }
                    //2021.04.26 검색조건 추가 및 자동 체크 기능 추가(Cell ID) END

                    if (e.Cell.Column.Name.Equals("DFCT_GR_TYPE_CODE_LV1") || e.Cell.Column.Name.Equals("DFCT_ITEM_CODE_LV2") || e.Cell.Column.Name.Equals("DEFECT_ID_LV3"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                    }
                }
            }));
        }

        private void dgCellIDDetail_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1.WPF.C1ComboBox cbo = e.EditingElement as C1.WPF.C1ComboBox;
            DataRowView drv = e.Row.DataItem as DataRowView;

            string sSelectedValue = drv[e.Column.Name].SafeToString();

            string sDFCT_GR_TYPE_CODE = (string)DataTableConverter.GetValue(e.Row.DataItem, "DFCT_GR_TYPE_CODE_LV1");
            string sDFCT_ITEM_CODE = (string)DataTableConverter.GetValue(e.Row.DataItem, "DFCT_ITEM_CODE_LV2");

            if (cbo != null)
            {
                if (Convert.ToString(e.Column.Name) == "DFCT_GR_TYPE_CODE_LV1")
                {
                    cbo.ItemsSource = DataTableConverter.Convert(_dvLEVEL1.ToTable());
                    cbo.SelectedIndex = -1;
                }

                if (Convert.ToString(e.Column.Name) == "DFCT_ITEM_CODE_LV2")
                {
                    _dvLEVEL2 = null;
                    string sDfctGrpTypeCode = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "DFCT_GR_TYPE_CODE_LV1"));

                    if (string.IsNullOrEmpty(sDfctGrpTypeCode))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_GR_TYPE_CODE_LV1"));
                        return;
                    }

                    _dvLEVEL2 = SetGridCboItem_CommonCodeLevel2(dgCellIDDetail.Columns["DFCT_ITEM_CODE_LV2"], "FORM_DFCT_ITEM_CODE", LOTID, DFCT_TYPE_CODE, Util.NVC(DFCT_GR_CODE)).DefaultView;
                    _dvLEVEL2.RowFilter = "DFCT_GR_TYPE_CODE_LV1 = '" + sDfctGrpTypeCode + "'";

                    cbo.ItemsSource = DataTableConverter.Convert(_dvLEVEL2.ToTable());
                    cbo.SelectedIndex = -1;
                    _dvLEVEL2.RowFilter = null;
                }

                if (Convert.ToString(e.Column.Name) == "DEFECT_ID_LV3")
                {
                    if (string.IsNullOrEmpty(sDFCT_GR_TYPE_CODE))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_GR_TYPE_CODE_LV1"));
                        return;
                    }

                    if (string.IsNullOrEmpty(sDFCT_ITEM_CODE))
                    {
                        Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("DFCT_ITEM_CODE_LV2"));
                        return;
                    }
                    _dvLEVEL3 = SetDfctCode(sDFCT_GR_TYPE_CODE, sDFCT_ITEM_CODE, DFCT_TYPE_CODE).DefaultView;

                    cbo.ItemsSource = DataTableConverter.Convert(_dvLEVEL3.ToTable());
                    cbo.SelectedIndex = -1;

                    DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Row.Index].DataItem, "SUBLOT_GRD_CODE", string.Empty);
                }

                cbo.EditCompleted += delegate (object sender1, EventArgs e1)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        this.UpdateRowView2(e.Row, e.Column);
                    }));

                };
            }
        }

        private bool _manualCommit2 = false;
        private void dgCellIDDetail_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            if (_manualCommit2 == false)
            {
                this.UpdateRowView2(e.Row, e.Column);
            }

            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                C1ComboBox cbo = e.EditingElement as C1ComboBox;

                if (dg.CurrentColumn.Name == "DEFECT_ID_LV3")
                {
                    if (_dvLEVEL3 != null && _dvLEVEL3.Count > 0)
                    {
                        string sDfctID = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "DEFECT_ID_LV3"));
                        DataRow[] dr = _dvLEVEL3.ToTable().Select("CBO_CODE = '" + sDfctID + "'");

                        if (dr.Length > 0)
                        {
                            string sValue = Util.NVC(dr[0]["SUBLOT_GRD_CODE"]);
                            DataTableConverter.SetValue(dgCellIDDetail.Rows[e.Row.Index].DataItem, "SUBLOT_GRD_CODE", sValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void UpdateRowView2(C1.WPF.DataGrid.DataGridRow dgr, C1.WPF.DataGrid.DataGridColumn dgc)
        {
            try
            {
                DataRowView drv = dgr.DataItem as DataRowView;

                if (drv != null && Convert.ToString(dgc.Name) == "DFCT_GR_TYPE_CODE_LV1")
                {
                    _manualCommit2 = true;
                    drv["DFCT_ITEM_CODE_LV2"] = string.Empty;
                    drv["DEFECT_ID_LV3"] = string.Empty;
                    this.dgCellIDDetail.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "DFCT_ITEM_CODE_LV2")
                {
                    _manualCommit2 = true;
                    drv["DEFECT_ID_LV3"] = string.Empty;
                    this.dgCellIDDetail.EndEditRow(true);
                }

                if (drv != null && Convert.ToString(dgc.Name) == "DEFECT_ID_LV3")
                {
                    _manualCommit2 = true;
                    this.dgCellIDDetail.EndEditRow(true);
                }
            }
            finally
            {
                _manualCommit2 = false;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgCellIDDetail.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgCellIDDetail.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }

        private void btnReCheckProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapSubLotProcess("R");
                    }
                }, ObjectDic.Instance.GetObjectName("RECHECK_PROC"));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnScrapStandbyProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapSubLotProcess("N");
                    }
                }, ObjectDic.Instance.GetObjectName("SCRAP_STANDBY_PROC"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnScrapProc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!FrameOperation.AUTHORITY.Equals("W"))
                {
                    return;
                }

                // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 START
                // 1. 해당 동 적용 여부 체크
                // 2. 적용 동 버튼 적용 여부 체크
                string[] sAttrbute = { null, "FCS002_096" };

                if (IsAreaCommoncodeAttrUse("PERMISSIONS_PER_BUTTON_FORM", "", sAttrbute))
                    if (!CheckButtonPermissionGroupByBtnGroupID("SCRAP_PROC_W", "FCS002_096")) return;
                // 20220725_C20220603-000198_버튼별 권한 체크 로직 추가 END

                // %1 (을)를 하시겠습니까?
                Util.MessageConfirm("SFU4329", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveScrapLotProcess(); //임시로 막아둠 2021.04.16
                    }
                }, ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL"));

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnAllCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sCheck = string.Empty;
                string sDfctGrpTypeCode = string.Empty;
                string sDfctTypeCode = string.Empty;
                string sDfctCode = string.Empty;
                string sSubLotGrdCode = string.Empty;

                for (int idx = 0; idx < dgCellIDDetail.Rows.Count; idx++)
                {
                    sCheck = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "CHK"));
                    sDfctGrpTypeCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DFCT_GR_TYPE_CODE_LV1"));
                    sDfctTypeCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DFCT_ITEM_CODE_LV2"));
                    sDfctCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "DEFECT_ID_LV3"));
                    sSubLotGrdCode = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[idx].DataItem, "SUBLOT_GRD_CODE"));

                    if (sCheck.Equals("True") || sCheck.Equals("1"))
                    {
                        break;
                    }
                }

                DataTable dt = DataTableConverter.Convert(dgCellIDDetail.ItemsSource);
                foreach (DataRow dr in dt.Rows)
                {
                    if (Util.NVC(dr["CHK"]).Equals("True") || Util.NVC(dr["CHK"]).Equals("1"))
                    {
                        dr["DFCT_GR_TYPE_CODE_LV1"] = sDfctGrpTypeCode;
                        dr["DFCT_ITEM_CODE_LV2"] = sDfctTypeCode;
                        dr["DEFECT_ID_LV3"] = sDfctCode;
                        dr["SUBLOT_GRD_CODE"] = sSubLotGrdCode;
                    }
                }
                Util.GridSetData(dgCellIDDetail, dt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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

        private void btnScrapByCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                FCS002_113 FCS002_113 = new FCS002_113();
                FCS002_113.FrameOperation = FrameOperation;
                this.FrameOperation.OpenMenuFORM("FCS002_113", "FCS002_113", "LGC.GMES.MES.FCS002", ObjectDic.Instance.GetObjectName("PHYSICAL_DISPOSAL_SUBLOT"), true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        private void dgReCheckNGLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }

        //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 START
        private void dgCellIDDetail_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentColumn.Name == "DEFECT_ID_LV3")
                {
                    if (dtTemp != null && dtTemp.Rows.Count > 0)
                    {
                        string sDfctID = Util.NVC(DataTableConverter.GetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "DEFECT_ID_LV3"));
                        DataRow[] dr = dtTemp.Select("CBO_CODE = '" + sDfctID + "'");

                        if (dr.Length > 0)
                        {
                            string sValue = Util.NVC(dr[0]["SUBLOT_GRD_CODE"]);
                            DataTableConverter.SetValue(dgCellIDDetail.Rows[dg.CurrentRow.Index].DataItem, "SUBLOT_GRD_CODE", sValue);
                            if (dg.CurrentRow.Index == (dg.Rows.Count - 1))
                            {
                                dg.SelectRow(dg.CurrentRow.Index - 1);
                            }
                            else
                            {
                                dg.SelectRow(dg.CurrentRow.Index + 1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //20220704_불량코드 선택 후 코드값 셋팅 시 무한루프 돔 수정 END

        private void rdoReCheck_Checked(object sender, RoutedEventArgs e)
        {
            DFCT_TYPE_CODE = "A";
            btnReCheckProc.IsEnabled = true;
            btnScrapStandbyProc.IsEnabled = false;
            btnScrapProc.IsEnabled = false;

            GetDetailInfo(LOTID);
        }

        private void rdoScpStdb_Checked(object sender, RoutedEventArgs e)
        {
            DFCT_TYPE_CODE = "B";
            btnReCheckProc.IsEnabled = false;
            btnScrapStandbyProc.IsEnabled = true;
            btnScrapProc.IsEnabled = false;

            GetDetailInfo(LOTID);
        }

        private void rdoScrap_Checked(object sender, RoutedEventArgs e)
        {
            DFCT_TYPE_CODE = "B";
            btnReCheckProc.IsEnabled = false;
            btnScrapStandbyProc.IsEnabled = false;
            btnScrapProc.IsEnabled = true;

            GetDetailInfo(LOTID);
        }
        #endregion

    }
}
