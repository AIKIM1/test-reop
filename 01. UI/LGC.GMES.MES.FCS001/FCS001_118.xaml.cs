/*************************************************************************************
 Created Date : 2020.12.30
      Creator : PSM
   Decription : 공정 검사 의뢰
--------------------------------------------------------------------------------------
 [Change History]
  2021.09.22  NAME   : Initial Created
  2022.03.10  KDH    : 공정 검사 의뢰 취소 기능 추가
  2022.11.15  조영대 : 멀티 Cell 입력기능 추가
  2023.01.12  LHR    : Sample 조회 Tab 추가
  2023.03.09  임근영 : LQC 의뢰 취소 조회 TAB 추가
  2024.12.20  최도훈 : 검사조회 탭 요청사항 표시되도록 수정 
  2025.04.09  이현승 : Catch-Up [E20240911-001061] 공정 검사 의뢰 및 해제시 Cell 등급 변경 추적성 향상을 위한 MENUID INDATA 추가
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_118 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        public FCS001_118()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion


        #region [Initialize]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Combo Setting
            InitCombo();
            //Control Setting
            InitControl();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }
        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            #region [Cell 저장 Tabpage]
            C1ComboBox[] cboCellGroupChild = { cboCellUser };
            ComCombo.SetCombo(cboCellGroup, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCDEPT", cbChild: cboCellGroupChild);

            C1ComboBox[] cboCellUserParent = { cboCellGroup };
            ComCombo.SetCombo(cboCellUser, CommonCombo_Form.ComboStatus.NONE, sCase: "PQCUSER", cbParent: cboCellUserParent);

            string[] sFilter = { "LQC_REQ_STEP_CODE" };
            ComCombo.SetCombo(cboCellStep, CommonCombo_Form.ComboStatus.NONE, sFilter: sFilter, sCase: "CMN");
            #endregion

            #region [검사조회 Tabpage]
            C1ComboBox[] cboLineSearchChild = { cboModelSearch };
            ComCombo.SetCombo(cboLineSearch, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineSearchChild);

            C1ComboBox[] cboModelSearchParent = { cboLineSearch };
            ComCombo.SetCombo(cboModelSearch, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelSearchParent);

            ComCombo.SetCombo(cboReqUser, CommonCombo_Form.ComboStatus.ALL, sCase: "PQCUSER");

            string[] sFilter2 = { "PQC_RSLT_CODE" };
            ComCombo.SetCombo(cboStatus, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter2, sCase: "CMN");  //검사 의뢰 결과

            string[] sFilter1 = { "LAST_JUDG_VALUE" };
            ComCombo.SetCombo(cboResult, CommonCombo_Form.ComboStatus.ALL, sFilter: sFilter1, sCase: "CMN");  //최종 판정 결과
            #endregion

            #region [조회 Tabpage]

            string[] sSampleFilter = { "FORM_SMPL_TYPE_CODE", "Y", null, null, null, null };
            ComCombo.SetCombo(cboSearchSampleType, CommonCombo_Form.ComboStatus.ALL, sFilter: sSampleFilter, sCase: "CMN_WITH_OPTION");

            //20220329_조회조건추가-생산라인,모델,LotType START
            ComCombo.SetCombo(cboLineSample, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE");

            C1ComboBox[] cboModelParentSample = { cboLineSample };
            ComCombo.SetCombo(cboModelSample, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParentSample);

            // Lot 유형
            ComCombo.SetCombo(cboLotType, CommonCombo_Form.ComboStatus.ALL, sCase: "LOTTYPE"); // 2021.08.19 Lot 유형 검색조건 추가
                                                                                               //20220329_조회조건추가-생산라인,모델,LotType END
            #endregion

            #region [해제이력 Tabpage]
            C1ComboBox[] cboLineCancelChild = { cboModelSelect };
            ComCombo.SetCombo(cboLineSelect, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineCancelChild); //생산라인 

            C1ComboBox[] cboModelCancelParent = { cboLineSelect };
            ComCombo.SetCombo(cboModelSelect, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelCancelParent); //모델명
            #endregion

        }

        private void InitControl()
        {
            dtpFromDateSearch.SelectedDateTime = DateTime.Now;
            dtpToDateSearch.SelectedDateTime = DateTime.Now;

            dtpFromDateSample.SelectedDateTime = DateTime.Now;
            dtpToDateSample.SelectedDateTime = DateTime.Now;

            dtpDateFromSearch.SelectedDateTime = DateTime.Now;
            dtpDateToSearch.SelectedDateTime = DateTime.Now;
        }

        private void Init_tpSaveCell()
        {
            txtCellCnt.Text = string.Empty;
            txtLotId.Text = string.Empty;
            txtReqDesc.Text = string.Empty;
            txtCellId.Text = string.Empty;

            cboCellStep.SelectedIndex = 0;

            dgCellList.ClearRows();
        }

        /* private void Init_tpInsObj()
         {
             DataSet dsInData = new DataSet();
             DataSet dsOutData = new DataSet();

             try
             {
                 DataTable dtRqst = new DataTable();
                 dtRqst.TableName = "RQSTDT";
                 dtRqst.Columns.Add("LANGID", typeof(string)); //2021.04.09 검사의뢰 공정 데이터 중문 출력 처리.

                 DataRow dr = dtRqst.NewRow();
                 dr["LANGID"] = LoginInfo.LANGID; //2021.04.09 검사의뢰 공정 데이터 중문 출력 처리.
                 dtRqst.Rows.Add(dr);

                 DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_REQ_OP", "RQSTDT", "RSLTDT", dtRqst);

                 iOpStepCount = dtRslt.Rows.Count;

                 int iRowCnt = dtRslt.Rows.Count * 2 + 2;
                 int iColCnt = tpReqInsW_day_endColumn;
                 double dColumnWidth = 0;

                 //상단 Datatable, 하단 Datatable 정의
                 DataTable Udt = new DataTable();
                 DataTable Ddt = new DataTable();

                 DataRow UrowHeader = Udt.NewRow();
                 DataRow DrowHeader = Ddt.NewRow();

                 for (int i = 0; i < iColCnt; i++)
                 {
                     switch (i.ToString())
                     {
                         case "0":
                             dColumnWidth = 90;
                             break;
                         case "1":
                             dColumnWidth = 130;
                             break;
                         case "2":
                             dColumnWidth = 160;
                             break;
                         default:
                             dColumnWidth = 55;
                             break;
                     }
                     //GRID Column Create
                     SetGridHeaderSingle((i + 1).ToString(), dgTarget, dColumnWidth);
                     Udt.Columns.Add((i + 1).ToString(), typeof(string));
                     Ddt.Columns.Add((i + 1).ToString(), typeof(string));
                 }

                 //Row 생성
                 for (int i = 0; i < dtRslt.Rows.Count + 1; i++)
                 {
                     DataRow Urow = Udt.NewRow();
                     Udt.Rows.Add(Urow);

                     DataRow Drow = Ddt.NewRow();
                     Ddt.Rows.Add(Drow);
                 }

                 DateTime dt = dtpTarget.SelectedDateTime;

                 //2021.04.20 Lot ID 생성 로직 수정 START
                 string year = Convert.ToChar((dt.Year - 2001) % 20 + 65).ToString();  //2021.01.13  jinmingfei
                 if (dt.Year < 2010) return;
                 string month = Convert.ToChar(dt.Month + 64).ToString();
                 //2021.04.20 Lot ID 생성 로직 수정 END

                 //마지막 일자 구하기
                 DateTime dt2 = dt.AddMonths(1);
                 dt2 = dt2.AddDays(-dt2.Day);

                 for (int k = 0; k < dtRslt.Rows.Count; k++)
                 {
                     if (k == 0)
                     {
                         Udt.Rows[k][0] = ObjectDic.Instance.GetObjectName("LINE_ID");
                         Udt.Rows[k][1] = ObjectDic.Instance.GetObjectName("MODEL");
                         Udt.Rows[k][2] = ObjectDic.Instance.GetObjectName("REQ_OP_NAME");
                         for (int iDay = 1; iDay <= 16; iDay++)
                         {
                             Udt.Rows[k][iDay + tpReqInsW_day_startColumn - 1] = Util.NVC(year + month + string.Format("{0:D2}", iDay));
                         }

                         Ddt.Rows[k][0] = ObjectDic.Instance.GetObjectName("LINE_ID");
                         Ddt.Rows[k][1] = ObjectDic.Instance.GetObjectName("MODEL");
                         Ddt.Rows[k][2] = ObjectDic.Instance.GetObjectName("REQ_OP_NAME");

                         int lastCol = 0;
                         for (int iDay = 17; iDay <= dt2.Day; iDay++)
                         {
                             Ddt.Rows[k][iDay + tpReqInsW_day_startColumn - 1 - 16] = Util.NVC(year + month + string.Format("{0:D2}", iDay));
                             lastCol = iDay + tpReqInsW_day_startColumn - 1 - 16;
                         }

                         //마지막 부분 지우기
                         for (int iDay = lastCol + 1; iDay < 19; iDay++)
                         {
                             Ddt.Rows[k][iDay] = string.Empty;
                         }
                     }

                     Udt.Rows[k + 1][0] = Util.NVC(cboLineTarget.SelectedValue);
                     Udt.Rows[k + 1][1] = Util.NVC(cboModelTarget.SelectedValue);
                     Udt.Rows[k + 1][2] = Util.NVC(dtRslt.Rows[k]["CMN_CD_NAME"]);
                     Udt.Rows[k + 1][3] = Util.NVC(dtRslt.Rows[k]["CMN_CD"]);

                     Ddt.Rows[k + 1][0] = Util.NVC(cboLineTarget.SelectedValue);
                     Ddt.Rows[k + 1][1] = Util.NVC(cboModelTarget.SelectedValue);
                     Ddt.Rows[k + 1][2] = Util.NVC(dtRslt.Rows[k]["CMN_CD_NAME"]);
                     Ddt.Rows[k + 1][3] = Util.NVC(dtRslt.Rows[k]["CMN_CD"]);
                 }

                 //상,하 Merge
                 Udt.Merge(Ddt, false, MissingSchemaAction.Add);

                 //dgTarget.ItemsSource = DataTableConverter.Convert(Udt);
                 Util.GridSetData(dgTarget, Udt, FrameOperation, false);

                 dgTarget.Columns[3].Visibility = Visibility.Collapsed;
             }
             catch (Exception ex)
             {
                 Util.MessageException(ex);
             }
         }
         */
        private bool SetGridCboItem_CommonCode(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE_LIST", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;
                if (!string.IsNullOrEmpty(sCmnCd))
                {
                    dr["CMCODE_LIST"] = sCmnCd;
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMBO_CMN_FORM", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region [조회 Method]

        #region [Cell 저장 Tabpage]
        #endregion

        #region [검사조회 Tabpage]
        private void GetReqList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                dtRqst.Columns.Add("LQC_REQ_ID", typeof(string));
                //  dtRqst.Columns.Add("LQC_REQ_GR_ID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LQC_REQ_USERID", typeof(string));
                dtRqst.Columns.Add("LQC_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("LAST_JUDGE_VALUE", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                if (!string.IsNullOrEmpty(txtSaveID.Text))
                {
                    dr["LQC_REQ_ID"] = Util.NVC(txtSaveID.Text);
                }
                else
                {
                    dr["FROM_TIME"] = Util.GetCondition(dtpFromDateSearch);
                    dr["TO_TIME"] = Util.GetCondition(dtpToDateSearch);
                    dr["MDLLOT_ID"] = Util.GetCondition(cboModelSearch, bAllNull: true);
                    dr["EQSGID"] = Util.GetCondition(cboLineSearch, bAllNull: true);
                    dr["LQC_REQ_USERID"] = Util.GetCondition(cboReqUser, bAllNull: true);
                    dr["LAST_JUDGE_VALUE"] = Util.GetCondition(cboResult, bAllNull: true);
                    dr["LQC_RSLT_CODE"] = Util.GetCondition(cboStatus, bAllNull: true);
                }
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgReqList, dtRslt, FrameOperation, true);

                Util.gridClear(dgReqCellInfo);
                txtReject.Text = string.Empty;
            }
            catch (Exception e)
            {

            }
        }
        #endregion

        //20220310_공정 검사 의뢰 취소 기능 추가 START
        #region [공정검사 해제 Tabpage]
        /// <summary>
        /// 공정검사 해제 대상 Cell ID 조회
        /// </summary>
        /// <param name="cell"></param>
        private void ScanClearId(string cell)
        {
            Util _util = new Util();
            if (string.IsNullOrEmpty(cell)) return;

            if (cell.Length < 10) return;

            //스프레드에 있는지 확인
            int iRow = -1;

            iRow = _util.GetDataGridRowIndex(dgCancel, dgCancel.Columns["SUBLOTID"].Name, cell);
            if (iRow > -1)
            {
                Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                return;
            }

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = cell;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_CHK_SAVE_CELL", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    DataTable preTable = DataTableConverter.Convert(dgCancel.ItemsSource);
                    DataTable dtTemp = new DataTable();

                    if (preTable.Columns.Count == 0)
                    {
                        preTable = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCancel.Columns)
                        {
                            preTable.Columns.Add(Convert.ToString(col.Name));
                        }

                        dtTemp = preTable.Copy();
                    }
                    else
                    {
                        dtTemp = new DataTable();
                        foreach (C1.WPF.DataGrid.DataGridColumn col in dgCancel.Columns)
                        {
                            dtTemp.Columns.Add(Convert.ToString(col.Name));
                        }
                    }

                    for (int i = 0; i < dtRslt.Rows.Count; i++)
                    {
                        DataRow row = dtTemp.NewRow();
                        row["LOTID"] = Util.NVC(dtRslt.Rows[i]["LOTID"]);
                        row["SUBLOTID"] = Util.NVC(dtRslt.Rows[i]["SUBLOTID"]);
                        row["CREATE_TIME"] = Util.NVC(dtRslt.Rows[i]["CREATE_TIME"]);
                        row["EQSGNAME"] = Util.NVC(dtRslt.Rows[i]["EQSGNAME"]);
                        row["MDLLOT_ID"] = Util.NVC(dtRslt.Rows[i]["MDLLOT_ID"]);
                        row["LQC_REQ_USER"] = Util.NVC(dtRslt.Rows[i]["LQC_REQ_USERID"]);
                        dtTemp.Rows.Add(row);
                    }

                    dtTemp.Merge(preTable);

                    Util.GridSetData(dgCancel, dtTemp, FrameOperation, true);
                }
                else
                {
                    Util.MessageValidation("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        //20220310_공정 검사 의뢰 취소 기능 추가 END

        #region [조회 Tabpage]
        private void GetSampleList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SMPL_TYPE_CODE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType START
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                //20220329_조회조건추가-생산라인,모델,LotType END

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpFromDateSample.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpToDateSample.SelectedDateTime.ToString("yyyyMMdd");
                if (!string.IsNullOrEmpty(txtSearchCellId.Text))
                {
                    dr["SUBLOTID"] = Util.GetCondition(txtSearchCellId, bAllNull: true);
                }

                dr["SMPL_TYPE_CODE"] = Util.GetCondition(cboSearchSampleType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType START
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = Util.GetCondition(cboLineSample, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelSample, bAllNull: true);
                dr["LOTTYPE"] = Util.GetCondition(cboLotType, bAllNull: true);
                //20220329_조회조건추가-생산라인,모델,LotType END
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SAMPLE_CELL_LQC", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation, true);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region [기타 Method]

        #region [Cell 저장 Tabpage]


        //private void SetCellInfo(string cell)
        //{
        //    try
        //    {
        //        DataTable dtRqst = new DataTable();
        //        dtRqst.TableName = "RQSTDT";
        //        dtRqst.Columns.Add("LANGID", typeof(string));
        //        dtRqst.Columns.Add("AREAID", typeof(string));
        //        dtRqst.Columns.Add("SUBLOTID", typeof(string));

        //        DataRow dr = dtRqst.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
        //        dr["SUBLOTID"] = cell;
        //        dtRqst.Rows.Add(dr);

        //        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_SCAN_FOR_LQC", "RQSTDT", "RSLTDT", dtRqst);

        //        if (dtRslt.Rows.Count == 0)
        //        {
        //            Util.MessageValidation("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
        //            return;
        //        }
        //        else if (dtRslt.Rows.Count == 1)
        //        {

        //            DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
        //            DataTable dtTemp = new DataTable();

        //            if (dt.Columns.Count == 0)
        //            {
        //                dt = new DataTable();
        //                foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
        //                {
        //                    dt.Columns.Add(Convert.ToString(col.Name));
        //                }

        //                dtTemp = dt.Copy();
        //            }
        //            else
        //            {
        //                dtTemp = new DataTable();
        //                foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
        //                {
        //                    dtTemp.Columns.Add(Convert.ToString(col.Name));
        //                }
        //            }
        //            DataRow drRow = dtTemp.NewRow();

        //            if (dgCellList.Rows.Count == 0)
        //            {
        //                drRow["LQC_REQ_GROUP"] = 1;
        //            }
        //            else
        //            {
        //                drRow["LQC_REQ_GROUP"] = Util.NVC_Int(DataTableConverter.GetValue(dgCellList.Rows[dgCellList.Rows.Count - 1].DataItem, "LQC_REQ_GROUP").ToString()) + 1;
        //            }

        //            drRow["SUBLOTID"] = Util.NVC(dtRslt.Rows[0]["SUBLOTID"]);
        //            drRow["CSTID"] = Util.NVC(dtRslt.Rows[0]["CSTID"]);
        //            drRow["LINE_NAME"] = Util.NVC(dtRslt.Rows[0]["LINE_NAME"]);
        //            drRow["MODEL"] = Util.NVC(dtRslt.Rows[0]["MODEL"]);
        //            drRow["PROD_LOTID"] = Util.NVC(dtRslt.Rows[0]["PROD_LOTID"]);
        //            drRow["REQ_STEP"] = Util.NVC(cboCellStep.Text);
        //            drRow["SUBLOTJUDGE"] = Util.NVC(dtRslt.Rows[0]["SUBLOTJUDGE"]);

        //            dtTemp.Rows.Add(drRow);
        //            dtTemp.Merge(dt);

        //            dgCellList.ItemsSource = DataTableConverter.Convert(dtTemp);

        //            txtCellCnt.Text = dgCellList.Rows.Count.ToString();
        //        }
        //    }


        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}

        private void SetCellInfo(string cell, bool isMulti)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = cell;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_SEL_CELL_SCAN_FOR_LQC_SUBLOT", "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count == 0)
                    {
                        //포장대기 Cell 확인
                        bizResult = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_SCAN_FOR_LQC_WAIT_BOX", "RQSTDT", "RSLTDT", dtRqst);

                        if (bizResult.Rows.Count == 0)
                        {
                            if (!isMulti) Util.MessageValidation("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                            return;
                        }
                    }

                    if (bizResult.Rows.Count == 1)
                    {

                        DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
                        DataTable dtTemp = new DataTable();

                        if (dt.Columns.Count == 0)
                        {
                            dt = new DataTable();
                            foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
                            {
                                dt.Columns.Add(Convert.ToString(col.Name));
                            }

                            dtTemp = dt.Copy();
                        }
                        else
                        {
                            dtTemp = new DataTable();
                            foreach (C1.WPF.DataGrid.DataGridColumn col in dgCellList.Columns)
                            {
                                dtTemp.Columns.Add(Convert.ToString(col.Name));
                            }
                        }
                        DataRow drRow = dtTemp.NewRow();

                        if (dgCellList.Rows.Count == 0)
                        {
                            drRow["LQC_REQ_GROUP"] = 1;
                        }
                        else
                        {
                            drRow["LQC_REQ_GROUP"] = Util.NVC_Int(DataTableConverter.GetValue(dgCellList.Rows[0].DataItem, "LQC_REQ_GROUP").ToString()) + 1;
                        }

                        drRow["SUBLOTID"] = Util.NVC(bizResult.Rows[0]["SUBLOTID"]);
                        drRow["CSTID"] = Util.NVC(bizResult.Rows[0]["CSTID"]);
                        drRow["LINE_NAME"] = Util.NVC(bizResult.Rows[0]["LINE_NAME"]);
                        drRow["MODEL"] = Util.NVC(bizResult.Rows[0]["MODEL"]);
                        drRow["PROD_LOTID"] = Util.NVC(bizResult.Rows[0]["PROD_LOTID"]);
                        drRow["REQ_STEP"] = Util.NVC(cboCellStep.Text);
                        drRow["SUBLOTJUDGE"] = Util.NVC(bizResult.Rows[0]["SUBLOTJUDGE"]);

                        dtTemp.Rows.Add(drRow);
                        dtTemp.Merge(dt);

                        dgCellList.ItemsSource = DataTableConverter.Convert(dtTemp);

                        txtCellCnt.Text = dgCellList.Rows.Count.ToString();
                    }
                });
            }


            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ScanCellId(string cell)
        {
            try
            {
                Util _util = new Util();
                if (string.IsNullOrEmpty(cell)) return;

                if (cell.Length < 10) return;

                // 콤마 여부 체크후 다중 처리와 싱글처리로 분기
                if (cell.Contains(","))
                {
                    bool isScanCheck = false;
                    string[] cells = cell.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string itemCell in cells)
                    {
                        int iRow = _util.GetDataGridRowIndex(dgCellList, dgCellList.Columns["SUBLOTID"].Name, itemCell);
                        if (iRow > -1)
                        {
                            isScanCheck = true;
                            continue;
                        }

                        SetCellInfo(itemCell.Trim(), true);
                    }

                    if (isScanCheck)
                    {
                        Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                    }
                }
                else
                {
                    //스프레드에 있는지 확인
                    int iRow = -1;
                    iRow = _util.GetDataGridRowIndex(dgCellList, dgCellList.Columns["SUBLOTID"].Name, cell);

                    if (iRow > -1)
                    {
                        Util.MessageValidation("FM_ME_0193");  //이미 스캔한 ID 입니다.
                        return;
                    }

                    DataTable dtRslt1 = TcReqDetailByCellID(cell);

                    if (dtRslt1.Rows.Count > 0)
                    {
                        Util.MessageConfirm("FM_ME_0261", (result2) =>  //해당 구성 Cell ID는 기존에 제품검사 구성이력이 존재합니다. 의뢰 하시겠습니까?
                        {
                            if (result2 != MessageBoxResult.OK)
                            {
                                return;
                            }
                            SetCellInfo(cell, false);
                        });
                    }
                    else
                    {
                        SetCellInfo(cell, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable TcReqDetailByCellID(string CellID)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("SUBLOTID", typeof(string));

            try
            {
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SUBLOTID"] = CellID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_REQ_DETAIL_BY_CELLID", "RQSTDT", "RSLTDT", RQSTDT);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return RQSTDT;
            }
        }

        #endregion

        #region [조회 Tabpage]
        private void GetReqCellList(string sRqcReqId)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LQC_REQ_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LQC_REQ_ID"] = sRqcReqId;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_DETAIL", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgReqCellInfo, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion


        #region [Event]

        #region [Cell 저장 Tabpage]
        private void btnCellSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtCellId.Text))
            {
                Util.MessageValidation("FM_ME_0019");  //Cell ID를 입력해주세요.
                return;
            }

            ScanCellId(txtCellId.Text);
            txtCellId.Text = string.Empty;
            txtCellId.SelectAll();
        }


        private void txtCellId_ClipboardPasted(object sender, DataObjectPastingEventArgs e, string text)
        {
            if (text.Contains(","))
            {
                txtCellId.Text = string.Empty;
                ScanCellId(text);
            }
        }

        private void txtCellId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtCellId.Text))
                    return;
                try
                {
                    ScanCellId(txtCellId.Text);
                    txtCellId.Text = string.Empty;
                    txtCellId.SelectAll();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnCellClear_Click(object sender, RoutedEventArgs e)
        {
            Init_tpSaveCell();
        }

        private void btnCellSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgCellList.Rows.Count == 0)
                    return;
                Util.MessageConfirm("FM_ME_0429", (result) =>  //LQC ID를 발번하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(txtReqDesc.Text))
                        {
                            Util.MessageValidation("FM_ME_0251", new string[] { ObjectDic.Instance.GetObjectName("REQ_DESC") });  //필수항목누락 : {0}
                            return;
                        }

                        DataSet dsRqst = new DataSet();

                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SRCTYPE", typeof(string));
                        dtRqst.Columns.Add("IFMODE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("AREAID", typeof(string));
                        dtRqst.Columns.Add("REQ_REMARKS_CNTT", typeof(string));
                        dtRqst.Columns.Add("LQC_REQ_STEP_CODE", typeof(string));
                        dtRqst.Columns.Add("LQC_REQ_USERID", typeof(string));
                        dtRqst.Columns.Add("MENUID", typeof(string)); // 2025.04.09 이현승 : Cell 등급 변경 이력 추적성 향상을 위한 MENUID 추가


                        DataRow dr = dtRqst.NewRow();
                        dr["SRCTYPE"] = "UI";
                        dr["IFMODE"] = "OFF";
                        dr["USERID"] = LoginInfo.USERID;
                        dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                        dr["REQ_REMARKS_CNTT"] = cboCellStep.Text + "\r\n" + txtReqDesc.Text;
                        dr["LQC_REQ_STEP_CODE"] = Util.GetCondition(cboCellStep);
                        dr["LQC_REQ_USERID"] = Util.GetCondition(cboCellUser);
                        dr["MENUID"] = LoginInfo.CFG_MENUID;

                        dtRqst.Rows.Add(dr);
                        txtReqDesc.Text = cboCellStep.Text + "\r\n" + txtReqDesc.Text;

                        dsRqst.Tables.Add(dtRqst);

                        DataTable dtCell = new DataTable();
                        dtCell.TableName = "IN_SUBLOT";
                        dtCell.Columns.Add("SUBLOTID", typeof(string));

                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            DataRow dr2 = dtCell.NewRow();
                            dr2["SUBLOTID"] = DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID").ToString();

                            dtCell.Rows.Add(dr2);
                        }

                        dsRqst.Tables.Add(dtCell);

                        new ClientProxy().ExecuteService_Multi("BR_SET_LQC_SUBLOT_REQ_PRCS", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                if (bizResult.Tables["OUTDATA"].Rows[0]["RETVAL"].ToString().Equals("0"))
                                {
                                    Util.MessageInfo("FM_ME_0427");  //LQC ID 발행을 완료하였습니다.
                                    Init_tpSaveCell();
                                }
                                else
                                {
                                    Util.MessageInfo("FM_ME_0428");  //LQC ID 발행에 실패하였습니다.
                                }
                            }
                            catch (Exception ex)
                            {
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            }
                        }, dsRqst);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboCellStep_SelectionCommitted(object sender, PropertyChangedEventArgs<object> e)
        {
            /*if (lotMixReqTab1)
            {
                Util.gridClear(dgCellList);
                lotList_tpSaveCell = new List<String>();
                cellCnt_tpSaveCell = new List<int>();
            }

            for (int row = 0; row < dgCellList.Rows.Count; row++)
            {
                DataTableConverter.SetValue(row, "REQ_STEP", cboCellStep.Text);
            }

            lotMixReqTab1 = false;
            //안전성 LOT 혼용검사시 periodReq_tab1 표시

            DataTable dt = ((DataView)cboCellStep.ItemsSource).Table;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // LOT 혼합 검사
                if (cboCellStep.SelectedValue.ToString() == dt.Rows[i]["CBO_CODE"].ToString()
                    && int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString()) > 1)
                {
                    lotMixReqTab1 = true;
                    lotMixReqDayTab1 = int.Parse(dt.Rows[i]["PERIOD_REQ_DAY"].ToString());
                }
            }*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccess(object sender)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgCellList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("DELETE"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        #region [검사조회 Tabpage]
        private void btnReqSearch_Click(object sender, RoutedEventArgs e)
        {
            GetReqList();
        }

        private void dgReqList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
                        //e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Blue);
                        //e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        //e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END
                    }
                }
            }));
        }

        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 START
        private void dgReqList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("PQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        //2021.04.09 링크 기능을 Head -> Cell 로 전환 및 Bold 제거 END

        private void dgReqList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgReqList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (!cell.Column.Name.Equals("LQC_REQ_ID"))
                    {
                        return;
                    }

                    if (cell.Column.Name.Equals("LQC_REQ_ID"))
                    {
                        GetReqCellList(Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "LQC_REQ_ID")));

                        if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
                        {
                            if (chkResult.IsChecked == true)
                            {
                                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_RCV"));
                            }
                            else
                            {
                                txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_SND"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void chkResult_Checked(object sender, RoutedEventArgs e)
        {
            /* dgReqList.Columns[dgReqList.Columns["SAMPLE_ISSUE_DAY"].Index].Visibility = Visibility.Collapsed;
             dgReqList.Columns[dgReqList.Columns["REQ_OP"].Index].Visibility = Visibility.Collapsed;
             dgReqList.Columns[dgReqList.Columns["LQC_REQ_QTY"].Index].Visibility = Visibility.Collapsed;
             dgReqList.Columns[dgReqList.Columns["LQC_REQ_USER"].Index].Visibility = Visibility.Collapsed;
             dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_SND"].Index].Visibility = Visibility.Collapsed;
             dgReqList.Columns[dgReqList.Columns["LQC_RSLT_CODE"].Index].Visibility = Visibility.Visible;
             dgReqList.Columns[dgReqList.Columns["LAST_JUDGE_VALUE"].Index].Visibility = Visibility.Visible;
             dgReqList.Columns[dgReqList.Columns["LAST_JUDG_DTTM"].Index].Visibility = Visibility.Visible;
             dgReqList.Columns[dgReqList.Columns["LAST_JUDG_USERID"].Index].Visibility = Visibility.Visible;
             dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_RCV"].Index].Visibility = Visibility.Visible;

             lblDesc.Text = ObjectDic.Instance.GetObjectName("REMARK_REQ_REJ");  //비고(요청거부사항)

             if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
             {
                 txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_RCV"));
             }*/
        }

        private void chkResult_Unchecked(object sender, RoutedEventArgs e)
        {
            /*   dgReqList.Columns[dgReqList.Columns["SAMPLE_ISSUE_DAY"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["REQ_OP"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["PQC_REQ_QTY"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["DEFECT_CNT"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["PQC_REQ_USER"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_SND"].Index].Visibility = Visibility.Visible;
               dgReqList.Columns[dgReqList.Columns["PQC_CYCL_INSP_FLAG"].Index].Visibility = Visibility.Collapsed;
               dgReqList.Columns[dgReqList.Columns["PQC_RSLT_CODE"].Index].Visibility = Visibility.Collapsed;
               dgReqList.Columns[dgReqList.Columns["LAST_JUDGE_VALUE"].Index].Visibility = Visibility.Collapsed;
               dgReqList.Columns[dgReqList.Columns["LAST_JUDG_DTTM"].Index].Visibility = Visibility.Collapsed;
               dgReqList.Columns[dgReqList.Columns["LAST_JUDG_USERID"].Index].Visibility = Visibility.Collapsed;
               dgReqList.Columns[dgReqList.Columns["EXTRA_INFORM_RCV"].Index].Visibility = Visibility.Collapsed;

               lblDesc.Text = ObjectDic.Instance.GetObjectName("REQ_DESC");  //비고(요청거부사항)

               if (dgReqList.CurrentCell != null && dgReqList.CurrentRow.Index > -1)
               {
                   txtReject.Text = Util.NVC(DataTableConverter.GetValue(dgReqList.CurrentRow.DataItem, "EXTRA_INFORM_SND"));
               }*/
        }
        #endregion

        //20220310_공정 검사 의뢰 취소 기능 추가 START
        #region [공정검사 해제 Tabpage]
        private void txtClearId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txtClearId.Text))
                    return;

                ScanClearId(txtClearId.Text);
            }
        }

        private void btnClearCellClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCancel);
        }

        private void btnCellClearSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Util.MessageConfirm("FM_ME_0436", (result) =>  //선택한 Cell의 공정검사의뢰를 해제하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("TRAY_NO", typeof(string));
                        dtRqst.Columns.Add("CELL_ID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));
                        dtRqst.Columns.Add("GLOT_FLAG", typeof(string));
                        dtRqst.Columns.Add("MENUID", typeof(string)); // 2025.04.09 이현승 : Cell 등급 변경 이력 추적성 향상을 위한 MENUID 추가

                        //기존 Tray로 복구하겠습니까? (버튼을 OK / NO로 생성함)
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0482"), null, "Information", MessageBoxButton.YesNo, MessageBoxIcon.None, (result_restore) =>
                        {
                            if (result_restore == MessageBoxResult.OK) // 기존TRAY로 복구
                            {
                                for (int i = 0; i < dgCancel.Rows.Count; i++)
                                {
                                    DataRow dr = dtRqst.NewRow();

                                    dr["TRAY_NO"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "LOTID").ToString();
                                    dr["CELL_ID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "SUBLOTID").ToString();
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["GLOT_FLAG"] = "N";
                                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                                    dtRqst.Rows.Add(dr);
                                }

                                new ClientProxy().ExecuteService("BR_SET_REQ_LQC_CANCEL", "INDATA", null, dtRqst, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        Util.MessageInfo("FM_ME_0437");  //공정검사 해제를 완료하였습니다.
                                        Util.gridClear(dgCancel);
                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                });
                            }

                            if (result_restore == MessageBoxResult.No) //가상 G LOT 발번하여 복구
                            {
                                for (int i = 0; i < dgCancel.Rows.Count; i++)
                                {
                                    DataRow dr = dtRqst.NewRow();

                                    dr["TRAY_NO"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "LOTID").ToString();
                                    dr["CELL_ID"] = DataTableConverter.GetValue(dgCancel.Rows[i].DataItem, "SUBLOTID").ToString();
                                    dr["USERID"] = LoginInfo.USERID;
                                    dr["GLOT_FLAG"] = "Y";
                                    dr["MENUID"] = LoginInfo.CFG_MENUID;
                                    dtRqst.Rows.Add(dr);
                                }

                                new ClientProxy().ExecuteService("BR_SET_REQ_LQC_CANCEL", "INDATA", null, dtRqst, (bizResult, bizException) =>
                                {
                                    try
                                    {
                                        if (bizException != null)
                                        {
                                            Util.MessageException(bizException);
                                            return;
                                        }

                                        Util.MessageInfo("FM_ME_0437");  //공정검사 해제를 완료하였습니다.
                                        Util.gridClear(dgCancel);
                                    }
                                    catch (Exception ex)
                                    {
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                                    }
                                });
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
        //20220310_공정 검사 의뢰 취소 기능 추가 END

        #region 조회 Tabpage
        private void btnSearchSample_Click(object sender, RoutedEventArgs e)
        {
            GetSampleList();
        }
        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Equals("CSTID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

                if (e.Cell.Column.Name.Equals("SMPL_STAT"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SMPL_STAT")).Equals(ObjectDic.Instance.GetObjectName("RESTORE")))
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }

                if (e.Cell.Column.Name.Equals("CHK"))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "DFCT_SLOC_SMPL_FLAG")).Equals("Y")) //불량창고의 셀일 경우
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Cell.Row.Index, 0);
                        cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

                if (cell == null || datagrid.CurrentRow == null)
                {
                    return;
                }

                if (datagrid.CurrentColumn.Name.Equals("CSTID"))
                {
                    FCS001_021 wndRunStart = new FCS001_021();
                    wndRunStart.FrameOperation = FrameOperation;

                    if (wndRunStart != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cell.Text;

                        this.FrameOperation.OpenMenu("SFU010710010", true, Parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        #endregion

        private void dgSearch_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgSearch.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void btnCancelSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCancelList();
        }

        private void dgCancelSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);

                    if (e.Cell.Column.Name.Equals("LQC_REQ_ID"))
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }

        private void dgCancelSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e) //더블클릭
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);

            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = datagrid.GetCellFromPoint(pnt);

            if (cell != null && cell.Column.Name.Equals("LQC_REQ_ID"))
            {
                string sLQCID = cell.Text;

                try
                {

                    DataTable dtRqst = new DataTable();
                    dtRqst.TableName = "RQSTDT";
                    dtRqst.Columns.Add("LANGID", typeof(string));
                    dtRqst.Columns.Add("AREAID", typeof(string));
                    dtRqst.Columns.Add("FROM_DATE", typeof(string));
                    dtRqst.Columns.Add("TO_DATE", typeof(string));
                    dtRqst.Columns.Add("LQC_REQ_ID", typeof(string));//의뢰ID

                    DataRow dr = dtRqst.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["FROM_DATE"] = dtpDateFromSearch.SelectedDateTime.ToString("yyyyMMdd");
                    dr["TO_DATE"] = dtpDateToSearch.SelectedDateTime.ToString("yyyyMMdd");
                    dr["LQC_REQ_ID"] = sLQCID;


                    dtRqst.Rows.Add(dr);

                    DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_CANCEL_DETL", "RQSTDT", "RSLTDT", dtRqst);


                    Util.GridSetData(dgReqDetl, dtRsltAll, FrameOperation, true);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void GetCancelList()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["FROM_DATE"] = dtpDateFromSearch.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = dtpDateToSearch.SelectedDateTime.ToString("yyyyMMdd");
                dr["EQSGID"] = Util.GetCondition(cboLineSelect, bAllNull: true);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModelSelect, bAllNull: true);

                dtRqst.Rows.Add(dr);

                DataTable dtRsltAll = new ClientProxy().ExecuteServiceSync("DA_SEL_LQC_CANCEL", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgCancelSearch, dtRsltAll, FrameOperation, true);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


    }
}


