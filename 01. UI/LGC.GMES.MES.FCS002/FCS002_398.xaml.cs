/*************************************************************************************
 Created Date : 2024.05.27
      Creator : 이홍주
   Decription : 소형(NFF) CELL ReCheck 처리
--------------------------------------------------------------------------------------
 [Change History]
  2024.05.27     이홍주   Initial Created
  2024.06.28     이홍주   NFF 양산           BOX위치 정보 칼럼 추가
  2024.08.12     이홍주   E20240820-000556   RECHECK 대상 선택 후 선택 취소기능 추가
                                             VEND-->VENT오타수정  
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
    public partial class FCS002_398 : UserControl, IWorkArea
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
          DataTable dtOri = new DataTable();

        DataView _dvLEVEL1 { get; set; }
        DataView _dvLEVEL2 { get; set; }
        DataView _dvLEVEL3 { get; set; }

        Stack<string> reChcekstack = new Stack<string>();

        #endregion

        #region [Initialize]
        public FCS002_398()
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
            //listAuth.Add(btnReCheckProc);
            //listAuth.Add(btnScrapStandbyProc);
            //listAuth.Add(btnScrapProc);

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
                ///txtGrpLotID.Text = Util.NVC(parameters[0]);
                ///GetLotInfo();
            }

            IsLoading = false;
            this.Loaded -= UserControl_Loaded;


            /////
           
            /////

        }


        /// <summary>
        /// Setting Combo Items
        /// </summary>
        private void InitCombo()
        {
            // 동
            ComCombo.SetCombo(cboArea, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "ALLAREA");

            // Line
            SetLine();


            //cboLine.SelectedItems.Clear();

            //C1ComboBox[] cboLineChild = { cboLot };
            //ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "LINE", cbChild: cboLineChild);

            //C1ComboBox[] cboTypeChild = { cboLine };
            //string[] sFilter3 = { "FORM_CELL_TYPE_MB" };
            //ComCombo.SetCombo(cboType, CommonCombo_Form_MB.ComboStatus.NONE, sCase: "AREA_COMMON_CODE", cbChild: cboTypeChild, sFilter: sFilter3);

            //C1ComboBox[] cboLineParent = { cboType };
            //C1ComboBox[] cboLineChild = { cboProcGrpCode };
            //ComCombo.SetCombo(cboLine, CommonCombo_Form_MB.ComboStatus.ALL, sCase: "LINE_BY_CELLTYPE", cbChild: cboLineChild, cbParent: cboLineParent);

            // 공정 그룹
            //C1ComboBox[] cboProcGrParent = { cboLine };
            ///C1ComboBox[] cboProcGrChild = { cboProcess };

            //string ProcGR = GetProcGR();
            // string[] sFilterProcGR = { "PROC_GR_CODE_MB", Util.NVC(cboArea.SelectedValue), ProcGR };
            //string[] sFilterProcGR = { "PROC_GR_CODE_MB", Util.NVC(cboArea.SelectedValue) };
            ///ComCombo.SetCombo(cboProcGrpCode, CommonCombo_Form_MB.ComboStatus.SELECT, cbChild: cboProcGrChild, cbParent: cboProcGrParent, sFilter: sFilterProcGR, sCase: "PROCGRP_BY_LINE");

            // 공정
            //C1ComboBox[] cboProcParent = { cboArea, cboLine, cboProcGrpCode };
            ///C1ComboBox[] cboProcChild = { cboEquipment };
            ///ComCombo.SetCombo(cboProcess, CommonCombo_Form_MB.ComboStatus.SELECT, cbChild: cboProcChild, cbParent: cboProcParent, sCase: "PROC_BY_PROCGRP");

            // 설비 
            ///C1ComboBox[] cboEqpParent = { cboArea, cboLine, cboProcGrpCode, cboProcess };
            //string[] sFilterEqp = { "M" };
            ///ComCombo.SetCombo(cboEquipment, CommonCombo_Form_MB.ComboStatus.ALL, cbParent: cboEqpParent,sFilter: sFilterEqp, sCase: "EQP_BY_PROC");

            // Lot 유형
            //string[] sFilter1 = { "LOT_DETL_TYPE_CODE" };
            /// ComCombo.SetCombo(cboLotType, CommonCombo_Form_MB.ComboStatus.ALL, sFilter: sFilter1, sCase: "FORM_CMN");
        }

        private void InitControl()
        {
            dtpFromDateSearch.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpToDateSearch.SelectedDateTime = DateTime.Now.AddDays(0);

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
            Util.gridClear(dgReCheckLotList);
        }

        #endregion

        #region [Method]

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            
            
            
            try
            {
                btnSearch_Click(null, null);

                //    if (rdoNOREAD.IsChecked  == true)
                //    {
                //        sNgType = "NOREAD";
                //    }

                //    else if (rdoNG.IsChecked == true)
                //    {
                //        sNgType = "NG";
                //    }
                //    else { sNgType = ""; }

                //    //DataTable dtScrap = DataTableConverter.Convert(dgReCheckLotList.ItemsSource);

                //    DataTable dtTemp = dtOri.Copy();

                //    DataRow[] drSelectList;

                //    if (sNgType == "") //ALL
                //    {
                //        drSelectList = dtTemp.Select();
                //    }
                //    else //NG,NOREAD
                //    {
                //        drSelectList = dtTemp.Select(string.Format("NG_TYPE = '{0}'", sNgType));
                //    }

                //    DataTable dtFilter = dtTemp.Clone(); //DataTableConverter.Convert(dtScrap.Select(string.Format("NG_TYPE = '{0}'", "NG")));

                //    foreach (DataRow drSelect in drSelectList)
                //    {
                //        dtFilter.Rows.Add(drSelect.ItemArray);
                //    }


                //    if (dtFilter.Rows.Count > 0)
                //    {

                //        Util.GridSetData(dgReCheckLotList, dtFilter, FrameOperation, true);

                //        if (chkMerge.IsChecked == true)
                //        {
                //            string[] sColumnName = new string[] { "EQSGID", "PROC_NAME", "EQPTID", "LOTID", "CSTID", "LOT_DETL_TYPE_CODE", "LOTTYPE", "PROD_LOTID", "PRODID", "PRJT_NAME", "ROUTID", "WIPQTY", "WIPSTAT", "WIPDTTM_ED", "SUBLOTJUDGE", "UPDDTTM" };
                //            _Util.SetDataGridMergeExtensionCol(dgReCheckLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                //        }

                //        if (dtFilter.Rows.Count == 1)
                //        {
                //            string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[0].DataItem, "LOTID"));
                //            dgReCheckLotList.SelectedIndex = 0;
                //        }

                //        if (!string.IsNullOrEmpty(txtCellID.Text))
                //        {
                //            txtCellID.SelectAll();
                //            txtCellID.Focus();
                //        }



                //    }
            }
            catch(Exception ex)
            {

            }

        }


        //Recheck 변경 대상 조회 (EOL 이면서 GOOD, END)
        private void SetRecheckLot()
        {
            try
            {
                string str = string.Empty;
                cboLot.SelectedValue = "";               

                initScreen();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCGRID", typeof(string));
                RQSTDT.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                
                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID.ToString();                
                dr["EQSGID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["PROCGRID"] = "5"; //EOL
                dr["LOT_DETL_TYPE_CODE"] = "G"; //GOOD 양품
                dr["FROM_DATE"] = Convert.ToDateTime(dtpFromDateSearch.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00");
                dr["TO_DATE"] = Convert.ToDateTime(dtpToDateSearch.SelectedDateTime.AddDays(1).ToString("yyyy-MM-dd") + " 23:59:59");
                
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_WIP_RECHECK_PROD_LOT_MB", "RQSTDT", "RSLTDT", RQSTDT);

                cboLot.DisplayMemberPath = "PROD_LOTID";
                cboLot.SelectedValuePath = "PROD_LOTID";

                cboLot.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                

            }
            catch (Exception ex)
            {
            }

        }

        private void SetLine()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_LINE_MB", "RQSTDT", "RSLTDT", RQSTDT);

                cboLine.DisplayMemberPath = "CBO_NAME";
                cboLine.SelectedValuePath = "CBO_CODE";
                cboLine.ItemsSource = DataTableConverter.Convert(dtResult);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

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
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("LOT_DETL_TYPE_CODE", typeof(string));
                inTable.Columns.Add("FROM_DATE", typeof(string));
                inTable.Columns.Add("TO_DATE", typeof(string));
                inTable.Columns.Add("NG_TYPE", typeof(string));

                string sNGTYPE = "";

      
                

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.GetCondition(cboLine, sMsg: "SFU1223");  //라인을 선택해주세요.
                if (string.IsNullOrEmpty(newRow["EQSGID"].ToString())) return;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID.ToString();
                newRow["PROCGRID"] = "5"; //EOL
                newRow["PROD_LOTID"] = Util.GetCondition(cboLot, sMsg: "SFU4613");  //조립LOT을 선택하세요.;
                if (string.IsNullOrEmpty(newRow["PROD_LOTID"].ToString())) return;
                newRow["LOT_DETL_TYPE_CODE"] = "G"; //GOOD 양품
                newRow["FROM_DATE"] = Convert.ToDateTime(dtpFromDateSearch.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00");
                newRow["TO_DATE"] = Convert.ToDateTime(dtpToDateSearch.SelectedDateTime.AddDays(1).ToString("yyyy-MM-dd") + " 23:59:59");


                if (rdoNG.IsChecked == true)
                {
                    newRow["NG_TYPE"] = "NG";
                }
                else if (rdoNOREAD.IsChecked == true)
                {
                    newRow["NG_TYPE"] = "NOREAD";
                }
                else
                {
                    newRow["NG_TYPE"] = null;
                }


                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_SEL_WIP_RECHECK_PROD_LOT_DETAIL_MB", "RQSTDT", "RSLTDT", inTable, (result, Exception) =>
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

                            dtOri = result.Copy();


                            Util.GridSetData(dgReCheckLotList, result, FrameOperation, true);

                            if (chkMerge.IsChecked == true)
                            {
                                string[] sColumnName = new string[] { "EQSGID", "PROC_NAME", "EQPTID", "LOTID", "CSTID", "LOT_DETL_TYPE_CODE", "LOTTYPE", "PROD_LOTID", "PRODID", "PRJT_NAME", "ROUTID", "WIPQTY", "WIPSTAT", "WIPDTTM_ST", "SUBLOTJUDGE", "UPDDTTM" };
                                _Util.SetDataGridMergeExtensionCol(dgReCheckLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                            }

                            if (result.Rows.Count == 1)
                            {
                                string sLotID = Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[0].DataItem, "LOTID"));
                                dgReCheckLotList.SelectedIndex = 0;
                            }
                           
                            if (!string.IsNullOrEmpty(txtCellID.Text))
                            {
                                txtCellID.SelectAll();
                                txtCellID.Focus();
                            }
                        }

                        reChcekstack.Clear();
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
                            dtTemp.Columns.Add("TRAY_SUBLOT_POSITION", typeof(string));
                            dtTemp.Columns.Add("BOX_SUBLOT_POSITION", typeof(string));

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
                                drIn["TRAY_SUBLOT_POSITION"] = Util.NVC(drSubLot["TRAY_SUBLOT_POSITION"]);
                                drIn["BOX_SUBLOT_POSITION"] = Util.NVC(drSubLot["BOX_SUBLOT_POSITION"]);

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

                            ///Util.GridSetData(dgReCheckLotList, dtTemp, FrameOperation, true);
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

        //ReCheck 처리
        private void SaveScrapSubLotProcess(string sLotDetlTypeCode = "R")
        {
            try
            {
                //DATA SET
                DataTable dtScrap = DataTableConverter.Convert(dgReCheckLotList.ItemsSource).Select("CHK = True").ToDataTable();
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
                newRow["REMARKS_CNTT"] = ""; //변경사유
                newRow["MENUID"] = LoginInfo.CFG_MENUID;
                inTable.Rows.Add(newRow);

                DataTable inSubLot = inDataSet.Tables.Add("IN_SUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));
                inSubLot.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                inSubLot.Columns.Add("DFCT_CODE", typeof(string));

                for (int i = 0; i < dgReCheckLotList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {

                        DataRow newRowSubLot = inSubLot.NewRow();

                        string sSubLotID = Util.Convert_CellID(Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "SUBLOTID")));
                        string sNGTYPE = Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "NG_TYPE"));

                        string sDFCT_CODE = ""; // ING : NOREAD, INO : NG

                        if (sNGTYPE == "NG")
                        {
                            sDFCT_CODE = "INO"; //NG
                        }
                        else
                        {
                            sDFCT_CODE = "ING";  //NOREAD
                        }

                        newRowSubLot["SUBLOTID"] = sSubLotID;
                        newRowSubLot["DFCT_GR_TYPE_CODE"] = "E"; //EOL? (BIZ에서 입력값만 확인 후 사용안함 )
                        newRowSubLot["DFCT_CODE"] = sDFCT_CODE;  //(BIZ에서 입력값만 확인 후 사용안함) 
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
                        Util.MessageInfo("SFU1275");//정상처리되었습니다.
                        // 재조회
                        btnSearch_Click(null,null);
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
              

        #endregion
        
        #region [Event]
        // 체크박스 관련
        #region
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        //CheckBox chkAll = new CheckBox()
        //{
        //    IsChecked = false,
        //    Background = new SolidColorBrush(Colors.Transparent),
        //    VerticalAlignment = System.Windows.VerticalAlignment.Center,
        //    HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        //};
        private void dgReCheckLotList_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgReCheckLotList.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void dgReCheckLotList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgReCheckLotList.Dispatcher.BeginInvoke(new Action(() =>
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

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetRecheckLot();
        }

        private void cboLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (!IsLoading && cboLot.HasItems && cboLot.SelectedIndex != -1)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnClearCellClear_Click(object sender, RoutedEventArgs e)
        {
            initScreen();
        }

        private void dtpFromDateSearch_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetRecheckLot();
        }

        private void cboArea_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgReCheckLotList);
        }

  
        
        private void btnSearch_Click(object sender, EventArgs e)
        {                
             GetList();
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util _Util = new Util();

                if (dgReCheckLotList.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }

                int iRow = _Util.GetDataGridRowIndex(dgReCheckLotList, "CHK", "True");

                if (iRow < 0)
                {
                    Util.MessageValidation("FM_ME_0165");  //선택된 데이터가 없습니다.
                    return;
                }

                if (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[iRow].DataItem, "LOT_DETL_TYPE_CODE")).Equals("G"))
                {
                    string sTrayNo = Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[iRow].DataItem, "LOTID")).ToString();

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
                    parameters[0] = Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[iRow].DataItem, "LOTID"));   //TRAYNO
                    this.FrameOperation.OpenMenu("SFU010710320", true, parameters);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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

                dgReCheckLotList.SelectedIndex = idx;
                
            }
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

     
        private void dgReCheckLotList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
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

   
        
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {


            bool bSublotId = false;
            bool bVentId = false;
            bool bCanId = false;

            string sCellId = "";

            if ((!string.IsNullOrEmpty(txtCellID.Text)) && (e.Key == Key.Enter))
            {
                sCellId = txtCellID.Text.Trim();

                if (dgReCheckLotList.ItemsSource == null) return;

               
                for (int i = 0; i < dgReCheckLotList.GetRowCount(); i++)
                {
                    bSublotId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "SUBLOTID")).ToUpper().Equals(sCellId));
                    bVentId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "VENTID")).ToUpper().Equals(sCellId));
                    bCanId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "CANID")).ToUpper().Equals(sCellId));

                    if(bSublotId == true || bVentId == true || bCanId == true)
                    {
                        DataTableConverter.SetValue(dgReCheckLotList.Rows[i].DataItem, "CHK", true);

                        //선택 취소를 위해서 STACK에 선택값 추가
                        reChcekstack.Push(sCellId);
                    }
                }

                txtCellID.Text = "";
                txtCellID.Focus();

            }
        }

        #region 텍스트 박스 포커스 : text_GotFocus()
        /// <summary>
        /// 텍스트 박스 포커스
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }



        private void chkMerge_Checked(object sender, RoutedEventArgs e)
        {

            if (IsLoading) return;

            if (chkMerge.IsChecked == true)
            {
                string[] sColumnName = new string[] { "EQSGID", "PROC_NAME", "EQPTID", "LOTID", "CSTID", "LOT_DETL_TYPE_CODE", "LOTTYPE", "PROD_LOTID", "PRODID", "PRJT_NAME", "ROUTID", "WIPQTY", "WIPSTAT", "WIPDTTM_ST", "SUBLOTJUDGE", "SUBLOTJUDGE", "UPDDTTM" };
                _Util.SetDataGridMergeExtensionCol(dgReCheckLotList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            else
            {
                string[] sColumnName = new string[] { "EQSGID", "PROC_NAME", "EQPTID", "LOTID", "CSTID", "LOT_DETL_TYPE_CODE", "LOTTYPE", "PROD_LOTID", "PRODID", "PRJT_NAME", "ROUTID", "WIPQTY", "WIPSTAT", "WIPDTTM_ST", "SUBLOTJUDGE", "SUBLOTJUDGE", "UPDDTTM" };
                _Util.SetDataGridMergeExtensionCol(dgReCheckLotList, sColumnName, DataGridMergeMode.NONE);
            }
        }

                
        #endregion

        //};
        private void btnCancelTheSelection_Click(object sender, RoutedEventArgs e)
        {
            string sCellId = "";
            bool bSublotId = false;
            bool bVentId = false;
            bool bCanId = false;

            if (chkAll.IsChecked == true)
            {
                reChcekstack.Clear();

                //..전체 삭제
                for (int i = 0; i < dgReCheckLotList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgReCheckLotList.Rows[i].DataItem, "CHK", false);
                }

                txtCellID.Text = "";
                txtCellID.Focus();
            }
            else
            {
                if (dgReCheckLotList.ItemsSource == null || reChcekstack.Count() == 0) return;

                sCellId = reChcekstack.Pop();

                for (int i = 0; i < dgReCheckLotList.GetRowCount(); i++)
                {
                    bSublotId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "SUBLOTID")).ToUpper().Equals(sCellId));
                    bVentId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "VENTID")).ToUpper().Equals(sCellId));
                    bCanId = (Util.NVC(DataTableConverter.GetValue(dgReCheckLotList.Rows[i].DataItem, "CANID")).ToUpper().Equals(sCellId));

                    if (bSublotId == true || bVentId == true || bCanId == true)
                    {
                        DataTableConverter.SetValue(dgReCheckLotList.Rows[i].DataItem, "CHK", false);
                    }
                }

                txtCellID.Text = "";
                txtCellID.Focus();

            }




        }
        //private void dgReCheckLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new Action(() =>
        //    {
        //        if (string.IsNullOrEmpty(e.Column.Name) == false)
        //        {
        //            if (e.Column.Name.Equals("CHK"))
        //            {
        //                //pre.Content = chkAll;
        //                //e.Column.HeaderPresenter.Content = pre;
        //                //chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
        //                //chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
        //                //chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
        //                //chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
        //            }
        //        }
        //    }));
        //}

        //void checkAll_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    DataTable dt = DataTableConverter.Convert(dgReCheckLotList.ItemsSource);
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        dr["CHK"] = false;
        //    }
        //    dgReCheckLotList.ItemsSource = DataTableConverter.Convert(dt);
        //}

        


        #endregion


    }
}
