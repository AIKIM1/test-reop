/*************************************************************************************
 Created Date : 2020.12.10
      Creator : Kang Dong Hee
   Decription : 자주검사 등록/조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.10  NAME   : Initial Created
  2021.03.31  KDH    : 자주검사 등록 Parameter 수정 및 정보 조회/출력 부분 수정.
                       이미지 조회 및 출력 함수 수정 대응
  2021.04.08  KDH    : 화면간 이동 시 초기화 현상 제거 및 이미지 조회(BR_GET_WORKINSP_IMG) BR 호출 파라미터 INDATA, OUTDATA로 변경
                       조회 데이타가 없는 경우에도 Cell Merge 되도록 수정.(조회 Tab)
  2022.06.20  이정미 : 조회시 컬럼명이 없는 비정상 컬럼 생성되는 오류 수정
  2022.07.06  조영대 : 저장시 NullReference 오류 수정.
  2022.07.28  이정미 : 조회시 이미지 없을 경우, 메시지 출력 변경
  2022.10.20  강동희 : 저장 시 파라미터 값 추가(PROD_LOTID)
  2023.01.10  형준우 : 폴란드어 수치 표기 시 이상한 수치가 표기되는 현상 수정
  2023.01.25  형준우 : 폴란드어 수치 표기 시 이상한 수치가 표기되는 현상 누락된 부분 수정
  2023.03.01  배준호 : 폴란드어 수치 저장 시 소수점 ','를 '.'로 변환
  2023.03.14  최도훈 : 작성이력보기(FCS001_080_HIST_VIEW) 조회 안되는 부분 수정
  2023.08.15  손동혁 : DA 방식 조회 시 측정포인트 자리수 오류발생하여 수정 및 DA 방식에 TITLE1에 설비 이름이 아닌 챔버 이름으로 변경 
  2023.10.29  손동혁 : 조회탭에서 조회 시 설비구분 변경을 해도 검사방식이 이전 설비구분 검사방식이 존재하여 수정
  2023.11.16  주훈종 : 자주검사 입력 시 입력항목을 모두 필수로 입력할지? 아니면 일부만 입력해도 저장가능할지? 선택적으로 동작하도록 변경,  MMD 동별공통코드: SELF_INSP_ENTER_LESS
  2023.12.05  손동혁 : 자주검사 조회 후 저장 시 동별공통코드를 통하여 천단위 이상 ','를 공백처리 수정
  2024.01.15  조영대 : 상하/한 값을 벗어날 경우 오류 발생 수정.
  2024.02.05  조영대 : 조회 조건에 작업자, 작업일자 정렬 순서 추가
  2024.02.19  권순범 : WA3 요청 - MMD 자주검사 값이 NULL이면 0으로 나타내지말고 NULL로 요청
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_080 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        Util _Util = new Util();

        private int _iBaseUnit;
        private int _iUnit = 1;
        private int _iRow = 0;
        private int _iCol = 0;
        private int _iUnitColorRow = 0;
        private int _iUnitColorCol = 0;
        
        private bool bUseFlag = false;

        public FCS001_080()
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
            InitCombo();
            //Control Setting
            InitControl();

            dgSfInsBasInput_AddRow();
            //dgInspList_AddRow();

            txtWorker.Text = LoginInfo.USERNAME;

            bUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_080_NUMERIC_USE"); //동별공통코드에 기준정보 등록되어 있고, Attr1 속성의 값이 "Y"인 경우 기능 사용 가능.

            //기본 Unit 개수 설정
            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("AREAID", typeof(string));
            dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
            dtRqst.Columns.Add("COM_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "FORM_SITE_BASE_INFO";
            dr["COM_CODE"] = "WORKINSP_UNIT_DEFAULT";
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtRqst);
            if (dtRslt.Rows.Count > 0) //20210408 데이타가 있는 경우 아래 로직 타도록 수정
            {
                _iBaseUnit = Convert.ToInt32(dtRslt.Rows[0]["ATTR1"].ToString());
            }

            this.Loaded -= UserControl_Loaded; //20210408 화면간 이동 시 초기화 현상 제거
        }

        private void InitCombo()
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();

            ComCombo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.NONE, sCase: "MODEL");

            string[] sFilterEqpType = { "EQPT_GR_TYPE_CODE", "5,D" };
            ComCombo.SetCombo(cboEqpKind, CommonCombo_Form.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterEqpType);

            string[] sFilter = { "SELF_INSP_CHECK_TIME" };
            ComCombo.SetCombo(cboTime, CommonCombo_Form.ComboStatus.NONE, sCase: "FORM_CMN", sFilter: sFilter);

            ComCombo.SetCombo(cboSelModel, CommonCombo_Form.ComboStatus.NONE, sCase: "MODEL");

            ComCombo.SetCombo(cboSelEqpKind, CommonCombo_Form.ComboStatus.SELECT, sCase: "FORM_CMN", sFilter: sFilterEqpType);

            cboEqpKind.SelectedValueChanged += cboEqpKind_SelectedValueChanged;
            cboSelEqpKind.SelectedValueChanged += cboEqpKind_SelectedValueChanged;

        }

        private void InitControl()
        {
            dtpWorkDate.SelectedDateTime = DateTime.Now;
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }

        private void dgSfInsBasInput_AddRow()
        {
            Util.gridClear(dgSfInsBasInput);

            dgSfInsBasInput.ItemsSource = null;
            dgSfInsBasInput.Columns.Clear();
            dgSfInsBasInput.Refresh();

            DataTable dt = new DataTable();
            dt.Columns.Add("SUBLOTID", typeof(string));
            dt.Columns.Add("TITLE1", typeof(string));
            dt.Columns.Add("TITLE2", typeof(string));
            dt.Columns.Add("TITLE3", typeof(string));
            dt.Columns.Add("CLCT_TYPE", typeof(string));

            //DataTable dt = Util.MakeDataTable(dgSfInsBasInput, true);

            for (int i = 0; i < 6; i++)
            {
                DataRow dr = dt.NewRow();
                dr["SUBLOTID"] = ObjectDic.Instance.GetObjectName("CELL_ID");
                switch (i.ToString())
                {
                    case "0":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("INSP_TYPE");
                        break;
                    case "1":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("FIX_VAL");
                        break;
                    case "2":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("UPPER_LIMIT");
                        break;
                    case "3":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("LOWER_LIMIT");
                        break;
                    case "4":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("MEAS_POINT");
                        break;
                    case "5":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("CLCT_MTHD_TYPE_CODE");
                        break;
                }
                dt.Rows.Add(dr);

               
            }
            SetGridHeader(dt, dgSfInsBasInput);

            Util.GridSetData(dgSfInsBasInput, dt, this.FrameOperation, true);

            nudUnit.Visibility = Visibility.Collapsed;
            btnUnitPlus.Visibility = Visibility.Collapsed;
            btnUnitMinus.Visibility = Visibility.Collapsed;
        }

        private void dgInspList_AddRow()
        {
   
            Util.gridClear(dgInspList);
            dgInspList.ItemsSource = null;
            dgInspList.Columns.Clear();
            dgInspList.Refresh();
          
            // DataTable dt = Util.MakeDataTable(dgInspList, true);

            DataTable dt = new DataTable();
            dt.Columns.Add("WORK_USER", typeof(string));
            dt.Columns.Add("WORK_DATE", typeof(string));
            dt.Columns.Add("SUBLOTID", typeof(string));
            dt.Columns.Add("TITLE1", typeof(string));
            dt.Columns.Add("TITLE2", typeof(string));
            dt.Columns.Add("TITLE3", typeof(string));

            for (int i = 0; i < 6; i++) //20210331 5 -> 6 변경
            {
                DataRow dr = dt.NewRow();
                dr["WORK_USER"] = ObjectDic.Instance.GetObjectName("WORK_USER");
                dr["WORK_DATE"] = ObjectDic.Instance.GetObjectName("WORK_DATE");
                dr["SUBLOTID"] = ObjectDic.Instance.GetObjectName("CELL_ID");
                switch (i.ToString())
                {
                    case "0":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("INSP_TYPE");
                        break;
                    case "1":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("FIX_VAL");
                        break;
                    case "2":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("UPPER_LIMIT");
                        break;
                    case "3":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("LOWER_LIMIT");
                        break;
                    case "4":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("MEAS_POINT");
                        break;
                    case "5":
                        dr["TITLE1"] = ObjectDic.Instance.GetObjectName("CLCT_MTHD_TYPE_CODE");
                        break;
                }
                dt.Rows.Add(dr);
            }
            SetGridInspHeader(dt, dgInspList);

            Util.GridSetData(dgInspList, dt, this.FrameOperation, true);

            nudUnit.Visibility = Visibility.Collapsed;
            btnUnitPlus.Visibility = Visibility.Collapsed;
            btnUnitMinus.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                this.ClearValidation();
                if (cboEqpKind.GetBindValue() == null)
                {
                    cboEqpKind.SetValidation("SFU4925", tbEqpKind.Text);
                    return;
                }

                dgSfInsBasInput_AddRow();

                _iUnitColorRow = 0;
                _iUnitColorCol = 0;
                _iUnit = 1;
                #region [임시]
                string _sCA = "";
                string _sAN = "";
                string _sCA2 = "";
                string _sAN2 = "";
                #endregion

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                string sWorkDate = Util.GetCondition(dtpWorkDate) + int.Parse(Util.GetCondition(cboTime)).ToString("00"); //20210331 데이터 형식 변경
            
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROC_GR_CODE"] = Util.GetCondition(cboEqpKind);
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dr["SELF_INSP_TYPE_CODE"] = "D";
                dr["FROM_DATE"] = sWorkDate + "0000"; //20210331 데이터 형식 변경
                dr["TO_DATE"] = sWorkDate + "0000";   //20210331 데이터 형식 변경
                dr["EQPTID"] = Util.GetCondition(cboEqp);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_INPUT2", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0095");  //검사기준 정보가 없습니다.
                    //pbView.items = null;
                    return;
                }

                //검사방식 DISTINCT
                DataTable dtType = dtRslt.DefaultView.ToTable(true, "CLCT_MTHD_TYPE_CODE");
                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    int nRow = dgSfInsBasInput.Rows.Count;
                    string _sType = dtType.Rows[i]["CLCT_MTHD_TYPE_CODE"].ToString();
                    switch (_sType)
                    {
                        case "DA": //선택된 설비의 챔버 셋팅
                            DataTable dtRqst1 = new DataTable();
                            dtRqst1.TableName = "RQSTDT";
                            dtRqst1.Columns.Add("LANGID", typeof(string));
                            dtRqst1.Columns.Add("EQPTID", typeof(string));
                            dtRqst1.Columns.Add("FACILITY_CODE", typeof(string));

                            DataRow dr1 = dtRqst1.NewRow();

                            dr1["LANGID"] = LoginInfo.LANGID;
                            dr1["EQPTID"] = Util.GetCondition(cboEqp);
                            dr1["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;
                            dtRqst1.Rows.Add(dr1);

                            DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CHAMBER", "RQSTDT", "RSLTDT", dtRqst1);

                            DataTable dtA = Util.MakeDataTable(dgSfInsBasInput, true);

                            for (int j = 0; j < dtRslt1.Rows.Count; j++)
                            {
                                DataRow drA = dtA.NewRow();
                                ///2023.08.15 DA방식일 경우 설비명이 아닌 챔버명 표시
                                //drA["TITLE1"] = Util.NVC(dtRslt1.Rows[j]["EQP_NAME"]);
                                //drA["TITLE2"] = Util.NVC(dtRslt1.Rows[j]["CHAMBER_NAME"]);
                                drA["TITLE1"] = Util.NVC(dtRslt1.Rows[j]["CHAMBER_NAME"]);
                                drA["TITLE2"] = Util.NVC(dtRslt1.Rows[j]["EQP_NAME"]);
                                drA["TITLE3"] = Util.NVC(dtRslt1.Rows[j]["CHAMBER_ID"]);
                                drA["CLCT_TYPE"] = _sType;
                                dtA.Rows.Add(drA);
                            }
                            Util.GridSetData(dgSfInsBasInput, dtA, this.FrameOperation, true);

                            break;
                        case "DB": //검사방식 - 공통
                            DataTable dtB = Util.MakeDataTable(dgSfInsBasInput, true);

                            DataRow drB = dtB.NewRow();

                            drB["TITLE1"] = ObjectDic.Instance.GetObjectName("CMN_UNIT");
                            drB["TITLE3"] = "CO";
                            drB["CLCT_TYPE"] = _sType;
                            dtB.Rows.Add(drB);

                            Util.GridSetData(dgSfInsBasInput, dtB, this.FrameOperation, true);
                            break;
                        case "DC": //검사방식 - Unit
                            addRowForUnit(nRow, _iBaseUnit, _sType); //Unit setting
                            nudUnit.Visibility = Visibility.Visible;
                            btnUnitPlus.Visibility = Visibility.Visible;
                            btnUnitMinus.Visibility = Visibility.Visible;
                            break;
                    }
                }

                Util _Util = new Util();

                //검사항목만큼 컬럼 추가하기
                DataTable dtCol = Util.MakeDataTable(dgSfInsBasInput, true);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    string sClctItem = Util.NVC(dtRslt.Rows[i]["CLCTITEM"]);

                    int iCol = _Util.GetDataGridColIndex(dgSfInsBasInput, sClctItem);

                    if (iCol < 0)
                    {
                        SetGridHeaderTextColumnAdd(Util.NVC(sClctItem), dgSfInsBasInput, 80);
                        dtCol.Columns.Add(Util.NVC(sClctItem), typeof(string));
                    }
                }
                Util.GridSetData(dgSfInsBasInput, dtCol, this.FrameOperation, true);

                //필요값써주기
                int iClctCol = 0;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    string sClctItem = dtRslt.Rows[i]["CLCTITEM"].ToString();

                    decimal sTrgtValue = string.IsNullOrEmpty(dtRslt.Rows[i]["TRGT_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["TRGT_VALUE"].ToString());
                    decimal sMaxValue = string.IsNullOrEmpty(dtRslt.Rows[i]["MAX_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["MAX_VALUE"].ToString());
                    decimal sMinValue = string.IsNullOrEmpty(dtRslt.Rows[i]["MIN_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["MIN_VALUE"].ToString());

                    //24.02.19 WA3 요청 - MMD에 값이 NULL이면 똑같이 NULL로 보이게 요청
                    string sTrgtValue2 = null;
                    string sMaxValue2  = null;
                    string sMinValue2  = null;

                    if (sTrgtValue != 0)
                        sTrgtValue2 = Convert.ToDecimal(Util.NVC_Decimal(sTrgtValue)).ToString("#,##0.#0");

                    if (sMaxValue != 0)
                        sMaxValue2 = Convert.ToDecimal(Util.NVC_Decimal(sMaxValue)).ToString("#,##0.#0");

                    if (sMinValue != 0)
                        sMinValue2 = Convert.ToDecimal(Util.NVC_Decimal(sMinValue)).ToString("#,##0.#0");

                    iClctCol = dgSfInsBasInput.Columns[Util.NVC(dtRslt.Rows[i]["CLCTITEM"])].Index;

                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[0].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["CLCT_NAME"].ToString());
                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[1].DataItem, Util.NVC(sClctItem), sTrgtValue2);
                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[2].DataItem, Util.NVC(sClctItem), sMaxValue2);
                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[3].DataItem, Util.NVC(sClctItem), sMinValue2);
                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[4].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["MEASR_PSTN_NAME"].ToString());
                    DataTableConverter.SetValue(dgSfInsBasInput.Rows[5].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["CLCT_MTHD_TYPE_CODE"].ToString());
                }

                DataTable dtAgain = Util.MakeDataTable(dgSfInsBasInput, true);
                Util.GridSetData(dgSfInsBasInput, dtAgain, this.FrameOperation, true);

                //이전 검사결과 매칭하기
                DataTable dtRsltVal = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_VALUE", "RQSTDT", "RSLTDT", dtRqst);
                //이전 검사결과 ChamNum distinct 하여 조회

                #region [임시]
                for (int i = 0; i < dtRsltVal.Rows.Count; i++)
                {
                    if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("CA")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN1"; _sCA = "CA"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("AN")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN2"; _sAN = "AN"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("CA2")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN3"; _sCA2 = "CA2"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("AN2")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN4"; _sAN2 = "AN2"; }
                }
                #endregion

                DataTable dtChamNum = dtRsltVal.DefaultView.ToTable(true, "CLCT_MEASR_PONT_CODE");
                int unitCntSearch = 0;
                //등록된 UNIT ROW 수 체크
                ///2023.08.15  
                /// Substring -> Contains 로 변경
                for (int i = 0; i < dtChamNum.Rows.Count; i++)
                {
                    if (dtChamNum.Rows[i]["CLCT_MEASR_PONT_CODE"].ToString().Contains("UN"))
                    {
                        unitCntSearch++;
                    }
                }

                if (unitCntSearch >= 3)
                {
                    int startRow = dgSfInsBasInput.Rows.Count;
                    addRowForUnit(startRow, unitCntSearch - _iBaseUnit, "DC");
                    _iUnitColorRow = startRow;
                    _iUnitColorCol = dgSfInsBasInput.Columns["CLCT_TYPE"].Index + 1;

                }

                for (int i = 0; i < dtRsltVal.Rows.Count; i++)
                {
                    string sSerach = dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].ToString();

                    int iRow = _Util.GetDataGridRowIndex(dgSfInsBasInput, "TITLE3", sSerach); //20210331 Grid명 변경

                    if (iRow > 0)
                    {
                        string sClctItem = dtRsltVal.Rows[i]["CLCTITEM"].ToString();
                        //string sClctValue = dtRsltVal.Rows[i]["CLCT_VALUE"].ToString();
                        decimal sClctValue = string.IsNullOrEmpty(dtRsltVal.Rows[i]["CLCT_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRsltVal.Rows[i]["CLCT_VALUE"].ToString());

                        //24.02.19 WA3 요청 - 값이 NULL이면 NULL로 보이게 요청
                        string sClctValue2 = null;

                        if (sClctValue != 0)
                            sClctValue2 = Convert.ToDecimal(Util.NVC_Decimal(sClctValue)).ToString("#,##0.#0");

                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[iRow].DataItem, "SUBLOTID", dtRsltVal.Rows[i]["SUBLOTID"].ToString());
                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[iRow].DataItem, Util.NVC(sClctItem), sClctValue2);
                    }
                }
      
                SelectImagePreview();
                #region [임시]
                for (int i = 6; i < dgSfInsBasInput.Rows.Count; i++)
                {
                    string sValue = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3"));

                    if (!string.IsNullOrEmpty(_sCA) && sValue.Equals("UN1"))
                    {
                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3", _sCA);
                    }
                    else if (!string.IsNullOrEmpty(_sAN) && sValue.Equals("UN2"))
                    {
                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3", _sAN);
                    }
                    else if (!string.IsNullOrEmpty(_sCA2) && sValue.Equals("UN3"))
                    {
                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3", _sCA2);
                    }
                    else if (!string.IsNullOrEmpty(_sAN2) && sValue.Equals("UN4"))
                    {
                        DataTableConverter.SetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3", _sAN2);
                    }
                }
                #endregion

                CheckValidation();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void addRowForUnit(int rowNum, int loop, string _sType)
        {
            if (loop > 0)
            {
                DataTable dt = Util.MakeDataTable(dgSfInsBasInput, true);

                for (int i = 0; i < loop; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["TITLE1"] = _iUnit + ObjectDic.Instance.GetObjectName("UNIT");
                    dr["TITLE3"] = "UN" + _iUnit;
                    dr["CLCT_TYPE"] = _sType;
                    _iUnit++;
                    dt.Rows.Add(dr);
                }
                Util.GridSetData(dgSfInsBasInput, dt, this.FrameOperation, false);
            }
        }

        private void SelectImagePreview() //20210331 이미지 조회 및 출력 함수 추가 수정
        {
            try
            {
                pbView.Source = null;

                loadingIndicator.Visibility = Visibility.Visible;

                tbNoImage.Visibility = Visibility.Collapsed;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["MDLLOT_ID"] = Util.GetCondition(cboModel);
                dr["PROC_GR_CODE"] = Util.GetCondition(cboEqpKind);
                dtRqst.Rows.Add(dr);

                //20210408 INDATA, OUTDATA로 변경
                new ClientProxy().ExecuteService("BR_GET_WORKINSP_IMG", "INDATA", "OUTDATA", dtRqst, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        tbNoImage.Text = MessageDic.Instance.GetMessage("FM_ME_0440");
                        tbNoImage.Visibility = Visibility.Visible;
                        return;
                    }

                    DataTable dtFormSelfInspImg = result;

                    if (dtFormSelfInspImg.Rows.Count > 0)
                    {
                        //_sTransactionType = Convert.ToString(CommonDataSet.TransactionType.Modified);
                    }

                    if (dtFormSelfInspImg.Rows.Count == 0 || string.IsNullOrEmpty(dtFormSelfInspImg.Rows[0]["SELF_INSP_IMG1"].ToString()) == true)
                    {
                        return;
                    }

                    //byte[] byteFile = dtFormSelfInspImg.Rows[0]["SELF_INSP_IMG"] as byte[];

                    string base64String = string.Empty;
                    int iBasAttr81 = 1;
                    string sBasAttr = string.Empty;
                    for (int i = 0; i < 10; i++)
                    {
                        sBasAttr = string.Format("SELF_INSP_IMG{0}", (iBasAttr81 + i).ToString());
                        base64String = base64String + dtFormSelfInspImg.Rows[0][sBasAttr].ToString();
                    }

                    byte[] imageBytes = Convert.FromBase64String(base64String);

                    if (!(imageBytes.Length > 0))
                    {
                        return;
                    }

                    var bi = new BitmapImage();
                    bi.BeginInit();
                    if (imageBytes != null) bi.StreamSource = new MemoryStream(imageBytes);
                    bi.EndInit();

                    pbView.Source = bi;

                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GetListPeriod()
        {
            try
            {
                cboSelEqpKind.ClearValidation();
                if (cboSelEqpKind.GetBindValue() == null)
                {
                    cboSelEqpKind.SetValidation("SFU4925", tbSelEqpKind.Text);
                    return;
                }

                Util _Util = new Util(); //20210408 조회 데이타가 없는 경우에도 Cell Merge 되도록 수정.(조회 Tab)
                dgInspList_AddRow();

                //검사항목 셋팅
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("PROC_GR_CODE", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROC_GR_CODE"] = Util.GetCondition(cboSelEqpKind);
                dr["MDLLOT_ID"] = Util.GetCondition(cboSelModel);
                dr["SELF_INSP_TYPE_CODE"] = "D";
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate) + "000000"; //20210331 데이터 형식 변경
                dr["TO_DATE"] = Util.GetCondition(dtpToDate) + "235959";     //20210331 데이터 형식 변경
                dr["EQPTID"] = Util.GetCondition(cboSelEqp);
                dtRqst.Rows.Add(dr);

                //ROW 반복을 위에 먼저 쿼리
                DataTable dtRsltVal = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_VALUE", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRsltVal.Rows.Count == 0)
                {
                    string[] sColName = new string[] { "WORK_USER", "WORK_DATE" }; //20210408 조회 데이타가 없는 경우에도 Cell Merge 되도록 수정.(조회 Tab)
                    _Util.SetDataGridMergeExtensionCol(dgInspList, sColName, DataGridMergeMode.VERTICALHIERARCHI); //20210408 조회 데이타가 없는 경우에도 Cell Merge 되도록 수정.(조회 Tab)

                    Util.MessageInfo("FM_ME_0232");  //조회된 값이 없습니다.
                    return;
                }
                #region [임시]
                for (int i = 0; i < dtRsltVal.Rows.Count; i++)
                {
                    if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("CA")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN1"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("AN")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN2"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("CA2")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN3"; }
                    else if (dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].Equals("AN2")) { dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"] = "UN4"; }
                }
                #endregion
                //distinct 하여 필요한 날짜 시간 뽑아내고
                DataTable dtDate = dtRsltVal.DefaultView.ToTable(true, "WRKR_NAME", "WRK_DTTM");
                //선택된 설비의 챔버 셋팅
                DataTable dtRqst1 = new DataTable();
                dtRqst1.TableName = "RQSTDT";
                dtRqst1.Columns.Add("LANGID", typeof(string));
                dtRqst1.Columns.Add("EQPTID", typeof(string));
                dtRqst1.Columns.Add("FACILITY_CODE", typeof(string));

                DataRow dr1 = dtRqst1.NewRow();

                dr1["LANGID"] = LoginInfo.LANGID;
                dr1["EQPTID"] = Util.GetCondition(cboSelEqp);
                dr1["FACILITY_CODE"] = LoginInfo.CFG_AREA_ID;

                dtRqst1.Rows.Add(dr1);
              
                //검사결과 CHAMBR NUM distinct 조회
                DataTable dtChmNum = dtRsltVal.DefaultView.ToTable(true, "CLCT_MEASR_PONT_CODE", "WRK_DTTM");
                //해당 설비의 chamber조회
                 DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CHAMBER", "RQSTDT", "RSLTDT", dtRqst1);
                //DataTable dtRslt1 = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_CHAMBER_NA", "RQSTDT", "RSLTDT", dtRqst1);
                
                //날짜별로 검사type check
                for (int j = 0; j < dtDate.Rows.Count; j++)
                {
                    bool chmFlg = false;
                    bool coFlg = false;
                    int unitCntSearch = 0;
                    ///2023.08.15  
                    //Chamber 측정포인트는 1자리수 이므로 위의 SubString 에서 Contains로 변경 기존에는 Substring (0,2) 에서 오류 발생
                    for (int i = 0; i < dtChmNum.Rows.Count; i++)
                    {
                        string _sChmNum = dtChmNum.Rows[i]["CLCT_MEASR_PONT_CODE"].ToString();
                        if (dtDate.Rows[j]["WRK_DTTM"].ToString().Equals(dtChmNum.Rows[i]["WRK_DTTM"].ToString()))
                        {
                            if (_sChmNum.Contains("UN"))
                            {
                                unitCntSearch++;
                            }
                            else if (_sChmNum.Contains("CO"))
                            {
                                coFlg = true;
                            }
                            else
                            {
                                chmFlg = true;
                            }
                        }
                    }

                    DateTime dtWOrkDate = DateTime.Parse(dtDate.Rows[j]["WRK_DTTM"].ToString());      
                    if (chmFlg) //검사타입 - chamber
                    {
                        DataTable dtA = Util.MakeDataTable(dgInspList, true);

                        for (int i = 0; i < dtRslt1.Rows.Count; i++)
                        {
                            DataRow drA = dtA.NewRow();

                            drA["WORK_USER"] = Util.NVC(dtDate.Rows[j]["WRKR_NAME"]);
                            drA["WORK_DATE"] = dtWOrkDate.ToString("yyyy-MM-dd HH");

                             drA["TITLE1"] = Util.NVC(dtRslt1.Rows[i]["CHAMBER_NAME"]);
                             drA["TITLE2"] = Util.NVC(dtRslt1.Rows[i]["EQP_NAME"]);
                            // drA["TITLE1"] = Util.NVC(dtRslt1.Rows[i]["EQP_NAME"]);
                            //  drA["TITLE2"] = Util.NVC(dtRslt1.Rows[i]["CHAMBER_NAME"]);


                            drA["TITLE3"] = Util.NVC(dtDate.Rows[j]["WRK_DTTM"]) + Util.NVC(dtRslt1.Rows[i]["CHAMBER_ID"]);
                            dtA.Rows.Add(drA);
                        }
                        Util.GridSetData(dgInspList, dtA, this.FrameOperation, true);

                    }
                    if (coFlg) //검사타입 - 공통
                    {
                        DataTable dtB = Util.MakeDataTable(dgInspList, true);

                        DataRow drB = dtB.NewRow();

                        drB["TITLE1"] = ObjectDic.Instance.GetObjectName("CMN_UNIT");
                        drB["TITLE3"] = dtDate.Rows[j]["WRK_DTTM"].ToString() + "CO";
                        drB["WORK_USER"] = dtDate.Rows[j]["WRKR_NAME"].ToString();
                        drB["WORK_DATE"] = dtWOrkDate.ToString("yyyy-MM-dd HH");
                        dtB.Rows.Add(drB);

                        Util.GridSetData(dgInspList, dtB, this.FrameOperation, true);
                    }

                    DataTable dtC = Util.MakeDataTable(dgInspList, true);

                    for (int i = 0; i < unitCntSearch; i++) //검사타입- UNIT
                    {
                        DataRow drC = dtC.NewRow();

                        drC["TITLE1"] = (i + 1) + ObjectDic.Instance.GetObjectName("UNIT");
                        drC["TITLE3"] = dtDate.Rows[j]["WRK_DTTM"].ToString() + "UN" + (i + 1);
                        drC["WORK_USER"] = dtDate.Rows[j]["WRKR_NAME"].ToString();
                        drC["WORK_DATE"] = dtWOrkDate.ToString("yyyy-MM-dd HH");
                        dtC.Rows.Add(drC);
                    }
                    Util.GridSetData(dgInspList, dtC, this.FrameOperation, true);
                }
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_QA_INPUT2", "RQSTDT", "RSLTDT", dtRqst);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageInfo("FM_ME_0095");  //검사기준 정보가 없습니다.
                    return;
                }

                //검사항목만큼 컬럼 추가하기
                DataTable dtCol = Util.MakeDataTable(dgInspList, true);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    string sClctItem = Util.NVC(dtRslt.Rows[i]["CLCTITEM"]);

                    int iCol = _Util.GetDataGridColIndex(dgInspList, sClctItem);

                    if (iCol < 0)
                    {
                        SetGridHeaderTextColumnAdd(Util.NVC(sClctItem), dgInspList, 100);
                        dtCol.Columns.Add(Util.NVC(sClctItem), typeof(string));
                    }
                }
                Util.GridSetData(dgInspList, dtCol, this.FrameOperation, true);

                //필요값써주기
                int iClctCol = 0;
                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    string sClctItem = dtRslt.Rows[i]["CLCTITEM"].ToString();
                    //string sTrgtValue = dtRslt.Rows[i]["TRGT_VALUE"].ToString();
                    //string sMaxValue  = dtRslt.Rows[i]["MAX_VALUE"].ToString();
                    //string sMinValue  = dtRslt.Rows[i]["MIN_VALUE"].ToString();

                    decimal sTrgtValue = string.IsNullOrEmpty(dtRslt.Rows[i]["TRGT_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["TRGT_VALUE"].ToString());
                    decimal sMaxValue = string.IsNullOrEmpty(dtRslt.Rows[i]["MAX_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["MAX_VALUE"].ToString());
                    decimal sMinValue = string.IsNullOrEmpty(dtRslt.Rows[i]["MIN_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRslt.Rows[i]["MIN_VALUE"].ToString());

                    iClctCol = dgInspList.Columns[Util.NVC(dtRslt.Rows[i]["CLCTITEM"])].Index;

                    //24.02.19 WA3 요청 - MMD에 값이 NULL이면 똑같이 NULL로 보이게 요청
                    string sTrgtValue2 = null;
                    string sMaxValue2 = null;
                    string sMinValue2 = null;

                    if(sTrgtValue != 0)
                        sTrgtValue2 = Convert.ToDecimal(Util.NVC_Decimal(sTrgtValue)).ToString("#,##0.#0");

                    if(sMaxValue != 0)
                        sMaxValue2 = Convert.ToDecimal(Util.NVC_Decimal(sMaxValue)).ToString("#,##0.#0");

                    if(sMinValue != 0)
                        sMinValue2 = Convert.ToDecimal(Util.NVC_Decimal(sMinValue)).ToString("#,##0.#0");

                    DataTableConverter.SetValue(dgInspList.Rows[0].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["CLCT_NAME"].ToString());
                    DataTableConverter.SetValue(dgInspList.Rows[1].DataItem, Util.NVC(sClctItem), sTrgtValue2);
                    DataTableConverter.SetValue(dgInspList.Rows[2].DataItem, Util.NVC(sClctItem), sMaxValue2);
                    DataTableConverter.SetValue(dgInspList.Rows[3].DataItem, Util.NVC(sClctItem), sMinValue2);
                    DataTableConverter.SetValue(dgInspList.Rows[4].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["MEASR_PSTN_NAME"].ToString());
                    DataTableConverter.SetValue(dgInspList.Rows[5].DataItem, Util.NVC(sClctItem), dtRslt.Rows[i]["CLCT_MTHD_TYPE_CODE"].ToString());
                }

                for (int i = 0; i < dtRsltVal.Rows.Count; i++)
                {
                    string sSerach = dtRsltVal.Rows[i]["WRK_DTTM"].ToString() + dtRsltVal.Rows[i]["CLCT_MEASR_PONT_CODE"].ToString();

                    int iRow = _Util.GetDataGridRowIndex(dgInspList, "TITLE3", sSerach);

                    if (iRow > 0)
                    {
                        string sClctItem = dtRsltVal.Rows[i]["CLCTITEM"].ToString();
                        //string sClctValue = dtRsltVal.Rows[i]["CLCT_VALUE"].ToString();
                        decimal sClctValue = string.IsNullOrEmpty(dtRsltVal.Rows[i]["CLCT_VALUE"].ToString()) ? 0 : Convert.ToDecimal(dtRsltVal.Rows[i]["CLCT_VALUE"].ToString());

                        ///24.02.19 WA3 요청 - 값이 NULL이면 NULL로 보이게 요청
                        string sClctValue2 = null;

                        if (sClctValue != 0)
                            sClctValue2 = Convert.ToDecimal(Util.NVC_Decimal(sClctValue)).ToString("#,##0.#0");

                        DataTableConverter.SetValue(dgInspList.Rows[iRow].DataItem, "SUBLOTID", dtRsltVal.Rows[i]["SUBLOTID"].ToString());
                        DataTableConverter.SetValue(dgInspList.Rows[iRow].DataItem, Util.NVC(sClctItem), sClctValue2);
                    }
                }

                DataTable dtAgain = Util.MakeDataTable(dgInspList, true);

                #region 정렬

                DataTable dtHeader = dtAgain.Clone();
                for (int row = 0; row < 6; row++)
                {
                    // MES 2.0 ItemArray 위치 오류 Patch
                    //dtHeader.Rows.Add(dtAgain.Rows[row].ItemArray);
                    dtHeader.AddDataRow(dtAgain.Rows[row]);
                }
                DataTable dtData = dtAgain.Copy();                
                for (int row = 0; row < 6; row++)
                {
                    dtData.Rows.RemoveAt(0);
                }

                DataView dvSort = dtData.DefaultView;
                if (rdoWorkUser.IsChecked.Equals(true))
                {
                    dvSort.Sort = "WORK_USER, WORK_DATE";
                    dgInspList.Columns["WORK_USER"].DisplayIndex = 0;
                    dgInspList.Columns["WORK_DATE"].DisplayIndex = 1;
                }
                else
                {
                    dvSort.Sort = "WORK_DATE, WORK_USER";
                    dgInspList.Columns["WORK_DATE"].DisplayIndex = 0;
                    dgInspList.Columns["WORK_USER"].DisplayIndex = 1;
                }
                
                dtData = dvSort.ToTable();

                for (int row = 0; row < 6; row++)
                {
                    DataRow addRow = dtData.NewRow();
                    addRow.ItemArray = dtHeader.Rows[row].ItemArray;
                    dtData.Rows.InsertAt(addRow, row);
                }
                dtData.AcceptChanges();
             
                #endregion 정렬


                Util.GridSetData(dgInspList, dtData, this.FrameOperation, true);

                string[] sColumnName = new string[] { "WORK_USER", "WORK_DATE" };
                _Util.SetDataGridMergeExtensionCol(dgInspList, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetChkRangeClctValue(string clctItem, decimal cellvalue)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("CLCT_VALUE", typeof(decimal));

                DataRow dr = dtRqst.NewRow();
                dr["CLCTITEM"] = clctItem;
                dr["CLCT_VALUE"] = cellvalue;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CHK_CLCT_VALUE_FIT_PERIOD", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void CheckValidation()
        {
            try
            {
                dgSfInsBasInput.ClearValidation();

                for (int row = 6; row < dgSfInsBasInput.Rows.Count; row++)
                {
                    for(int col = 5;col < dgSfInsBasInput.Columns.Count; col++)
                    {
                        string columnName = dgSfInsBasInput.Columns[col].Name;
                        CheckValidation(row, columnName);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckValidation(int rowIndex, string colName)
        {
            try
            {
                dgSfInsBasInput.RemoveCellValidation(rowIndex, colName);

                string columnName = dgSfInsBasInput.Columns[colName].Name;
                string value = Util.NVC(dgSfInsBasInput.GetValue(rowIndex, columnName));
                if (!string.IsNullOrEmpty(value))
                {
                    if (!GetChkRangeClctValue(columnName, Convert.ToDecimal(value)))
                    {
                        //입력한 값이 상,하한 범위에 속하지 않습니다.
                        dgSfInsBasInput.SetCellValidation(rowIndex, colName, "FM_ME_0380");
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [조회]
        private bool IsOnlyPartOfTheInputPossible()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow drNew = RQSTDT.NewRow();
                drNew["LANGID"] = LoginInfo.LANGID;
                drNew["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNew["COM_TYPE_CODE"] = "SELF_INSP_ENTER_LESS";
                drNew["USE_FLAG"] = "Y";
                RQSTDT.Rows.Add(drNew);

                string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_USE";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (dtResult.Rows[0]["ATTR1"].Equals("Y")) return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return false;
        }
        #endregion

        #region Header 생성
        private void SetGridHeader(DataTable dt, C1DataGrid dg)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "CELL_ID",
                Binding = new Binding() { Path = new PropertyPath("SUBLOTID"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE1",
                Binding = new Binding() { Path = new PropertyPath("TITLE1"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE2",
                Binding = new Binding() { Path = new PropertyPath("TITLE2"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE3",
                Binding = new Binding() { Path = new PropertyPath("TITLE3"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "CLCT_TYPE",
                Binding = new Binding() { Path = new PropertyPath("CLCT_TYPE"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }

        private void SetGridInspHeader(DataTable dt, C1DataGrid dg)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "WORK_USER",
                Binding = new Binding() { Path = new PropertyPath("WORK_USER"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "WORK_DATE",
                Binding = new Binding() { Path = new PropertyPath("WORK_DATE"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "CELL_ID",
                Binding = new Binding() { Path = new PropertyPath("SUBLOTID"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE1",
                Binding = new Binding() { Path = new PropertyPath("TITLE1"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE2",
                Binding = new Binding() { Path = new PropertyPath("TITLE2"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = "TITLE3",
                Binding = new Binding() { Path = new PropertyPath("TITLE3"), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
        }
        #endregion

        #endregion

        #region [Event]

        #region [등록 Tab]

        private void btnSearch_Click(object sender, EventArgs e)
        {
            GetList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sWorkDate = Util.GetCondition(dtpWorkDate) + int.Parse(Util.GetCondition(cboTime)).ToString("00");
                string sEqpId = Util.GetCondition(cboEqp);

                bool partInput = IsOnlyPartOfTheInputPossible();

                DataTable dtRqst = new DataTable();

                dtRqst.Columns.Add("SRCTYPE", typeof(string));
                dtRqst.Columns.Add("IFMODE", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("WRK_DTTM", typeof(DateTime));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("CLCTITEM", typeof(string));
                dtRqst.Columns.Add("WRKR_NAME", typeof(string));
                dtRqst.Columns.Add("CLCT_MEASR_PONT_CODE", typeof(string));
                dtRqst.Columns.Add("CLCT_VALUE", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string)); //저장 시 파라미터 값 추가(PROD_LOTID)
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("SELF_INSP_TYPE_CODE", typeof(string));

                for (int i = 6; i < dgSfInsBasInput.Rows.Count; i++)
                {
                    for (int j = dgSfInsBasInput.Columns["CLCT_TYPE"].Index + 1; j < dgSfInsBasInput.Columns.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, "SUBLOTID")).ToString()))
                        {
                            if (dgSfInsBasInput.GetCell(i, j) != null)
                            {
                                string sValue = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, dgSfInsBasInput.Columns[j].Name)).ToString();
                                
                                if (string.IsNullOrEmpty(sValue))
                                {
                                    if (partInput) //빈값MMD 동별공통코드(SELF_INSP_ENTER_LESS ) 설정이 Y 일 경우, 꼭 전체 항목을 입력하지 않아도 된다.
                                    {
                                        continue;
                                    }else
                                    {
                                        Util.MessageValidation("FM_ME_0256");  //항목에 빈값이 있습니다.
                                        return;
                                    }                                    
                                }

                                DateTime dtWrkDttm = DateTime.ParseExact(sWorkDate + "0000", "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

                                DataRow dr = dtRqst.NewRow();
                                dr["SRCTYPE"] = "UI";
                                dr["IFMODE"] = "OFF";
                                dr["USERID"] = LoginInfo.USERID;
                                dr["WRK_DTTM"] = dtWrkDttm;
                                dr["EQPTID"] = sEqpId;
                                dr["CLCTITEM"] = dgSfInsBasInput.Columns[j].Name;
                                dr["WRKR_NAME"] = txtWorker.Text;
                                dr["CLCT_MEASR_PONT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, "TITLE3")).ToString();
                                dr["CLCT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, dgSfInsBasInput.Columns[j].Name)).ToString().Replace(',', '.');
                                if (bUseFlag)
                                {
                                    dr["CLCT_VALUE"] = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, dgSfInsBasInput.Columns[j].Name)).ToString().Replace(",", "");
                                }
                                dr["PROD_LOTID"] = Util.NVC(dgSfInsBasInput.GetCell(dgSfInsBasInput.Rows[i].Index, 0).Presenter.Tag); //저장 시 파라미터 값 추가(PROD_LOTID)
                                dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[i].DataItem, "SUBLOTID"));
                                dr["SELF_INSP_TYPE_CODE"] = "D";
                                dtRqst.Rows.Add(dr);
                            }
                        }
                    }
                }

                if (dtRqst.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU2052");  //입력된 항목이 없습니다.
                    return;
                }

                string saveMessage = MessageDic.Instance.GetMessage("FM_ME_0214");
                if (dgSfInsBasInput.IsValidation())
                {
                    saveMessage = MessageDic.Instance.GetMessage("FM_ME_0380") + "\r\n" + MessageDic.Instance.GetMessage("FM_ME_0214");
                }

                //저장하시겠습니까?
                Util.MessageConfirm("[*]" + saveMessage, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_WORKINSP_DATA", "INDATA", "OUTDATA", dtRqst); // BR 개발필요
                        GetList();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void nudUnit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnUnitPlus_Click(null, null);
            }
        }

        private void btnUnitPlus_Click(object sender, EventArgs e)
        {
            int startRow = dgSfInsBasInput.Rows.Count;
            addRowForUnit(startRow, Convert.ToInt32(nudUnit.Value), "DC");
            _iUnitColorRow = startRow;
            _iUnitColorCol = 4;
        }

        private void btnUnitMinus_Click(object sender, EventArgs e)
        {
            if (_iUnit - Convert.ToInt32(nudUnit.Value) > 2)
            {
                if (nudUnit.Value > 0)
                {
                    DataTable dt = Util.MakeDataTable(dgSfInsBasInput, true);

                    for (int i = 0; i < nudUnit.Value; i++)
                    {
                        dt.Rows.RemoveAt(dt.Rows.Count - 1);
                    }
                    Util.GridSetData(dgSfInsBasInput, dt, this.FrameOperation, false);
                }
                _iUnit = _iUnit - Convert.ToInt32(nudUnit.Value);
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            FCS001_080_HIST_VIEW HistView = new FCS001_080_HIST_VIEW();
            HistView.FrameOperation = FrameOperation;

            if (HistView != null)
            {
                object[] Parameters = new object[3];
                Parameters[0] = Util.GetCondition(dtpWorkDate);
                Parameters[1] = Util.GetCondition(cboEqp);
                Parameters[2] = "D";

                C1WindowExtension.SetParameters(HistView, Parameters);

                HistView.Closed += new EventHandler(HistView_Closed);
                HistView.ShowModal();
                HistView.CenterOnScreen();
            }

        }

        private void cboEqpKind_SelectedValueChanged(object sender, EventArgs e)
        {
            CommonCombo_Form ComCombo = new CommonCombo_Form();
            C1ComboBox cb = (C1ComboBox)sender;
            string sEqpKind = cb.SelectedValue.ToString();
            string[] sFilter = { sEqpKind };
            ComCombo.SetCombo((cb == cboEqpKind) ? cboEqp : cboSelEqp, CommonCombo_Form.ComboStatus.NONE, sCase: "EQPSUBSTRING", sFilter: sFilter);  //DEGAS/EOL 라인
        }

        private void HistView_Closed(object sender, EventArgs e)
        {
            FCS001_080_HIST_VIEW runStartWindow = sender as FCS001_080_HIST_VIEW;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                //btnSearch_Click(null, null);
            }
        }

        private void dgSfInsBasInput_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                    if (e.Cell.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }

                    if (e.Cell.Column.Name.Equals("TITLE3") || e.Cell.Column.Name.Equals("CLCT_TYPE"))
                    {
                        e.Cell.Column.Visibility = Visibility.Collapsed;
                    }

                    if (dgSfInsBasInput.IsValidation(e.Cell.Row.Index, e.Cell.Column.Index)) 
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Color.FromArgb(50, Colors.Red.R, Colors.Red.G, Colors.Red.B));
                    }

                    if (e.Cell.Row.Index < 6)
                    {
                        if (e.Cell.Row.Index.Equals(5))
                        {
                            dgSfInsBasInput.Rows[5].Visibility = Visibility.Collapsed;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        if (e.Cell.Column.Index > 4)
                        {
                            string sClctType = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[e.Cell.Row.Index].DataItem, "CLCT_TYPE"));
                            string sClctMthdType = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[5].DataItem, e.Cell.Column.Name));
                            if (!sClctType.Equals(sClctMthdType))
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Gray);
                                e.Cell.Presenter.Tag = "LOCK";
                                e.Cell.Presenter.IsEnabled = false;
                            }
                        }

                        //20221020_저장 시 파라미터 값 추가(PROD_LOTID) START
                        if (_iRow.Equals(0) && _iCol.Equals(0) && e.Cell.Column.Index == 0)
                        {
                            string sSubLotID = Util.NVC(DataTableConverter.GetValue(dgSfInsBasInput.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name));

                            if (!string.IsNullOrEmpty(sSubLotID))
                            {
                                DataTable dtRqst = new DataTable();
                                dtRqst.TableName = "RQSTDT";
                                dtRqst.Columns.Add("AREAID", typeof(string));
                                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                                DataRow dr = dtRqst.NewRow();
                                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                                dr["SUBLOTID"] = sSubLotID;
                                dtRqst.Rows.Add(dr);

                                DataTable dtRsltVal = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_CHECK_INFO", "RQSTDT", "RSLTDT", dtRqst);

                                if (dtRsltVal.Rows.Count > 0)
                                {
                                    string sModelID = Util.NVC(dtRsltVal.Rows[0]["MDLLOT_ID"]);

                                    if (!sModelID.Equals(Util.NVC(cboModel.SelectedValue)))
                                    {
                                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, dg.Columns[e.Cell.Column.Index].Name, string.Empty);
                                        e.Cell.Presenter.Tag = string.Empty;
                                        Util.MessageValidation("FM_ME_0459", new string[] { Util.NVC(cboModel.SelectedValue), sSubLotID });  //모델[%1]이 다른 Cell ID[%2]를 입력하셨습니다.
                                        return;
                                    }

                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, dg.Columns[e.Cell.Column.Index].Name, Util.NVC(dtRsltVal.Rows[0]["SUBLOTID"]));
                                    if (e.Cell.Presenter != null) e.Cell.Presenter.Tag = Util.NVC(dtRsltVal.Rows[0]["PROD_LOTID"]);
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, dg.Columns[e.Cell.Column.Index].Name, string.Empty);
                                    e.Cell.Presenter.Tag = string.Empty;
                                    Util.MessageValidation("FM_ME_0003", new string[] { sSubLotID });  //[Cell ID : %1]의 정보가 존재하지 않습니다.
                                    return;
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Tag = string.Empty;
                            }
                        }
                        //20221020_저장 시 파라미터 값 추가(PROD_LOTID) END
                    }

                    if (e.Cell.Row.Index > 5 && e.Cell.Column.Index > 4)
                    {
                        if (!_iUnitColorRow.Equals(0) && !_iUnitColorCol.Equals(0))
                        {
                            if ((e.Cell.Row.Index >= _iUnitColorRow && e.Cell.Row.Index < dg.Rows.Count) && (e.Cell.Column.Index >= _iUnitColorCol && e.Cell.Column.Index < dg.Columns.Count))
                            {
                                e.Cell.Presenter.Background = dg.GetCell(e.Cell.Row.Index - 1, e.Cell.Column.Index).Presenter.Background;

                                if (e.Cell.Presenter.Background != new SolidColorBrush(Colors.Gray))
                                {
                                    e.Cell.Presenter.IsEditing = false; // Lock기능 동일 여부 확인 필요
                                }
                            }

                            if (e.Cell.Row.Index.Equals(dg.Rows.Count - 1))
                            {
                                _iUnitColorRow = 0;
                            }

                            if (e.Cell.Column.Index.Equals(dg.Columns.Count - 1))
                            {
                                _iUnitColorCol = 0;
                            }
                        }
                    }
                }
            }));
        }

        private void dgSfInsBasInput_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
            {
                var _mergeList = new List<DataGridCellsRange>();

                _mergeList.Add(new DataGridCellsRange(grid.GetCell(0, 0), grid.GetCell(5, 0)));
                for (int j = 0; j < grid.GetRowCount(); j++)
                {
                 _mergeList.Add(new DataGridCellsRange(grid.GetCell(j, 1), grid.GetCell(j, 3)));
                }

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }
            }
        }

        private void dgSfInsBasInput_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                if (dataGrid.CurrentRow.Index < 6)
                {
                    e.Cancel = true;
                }

                if (string.Equals(e.Column.Name, "TITLE1"))
                {
                    e.Cancel = true;
                }

                if (!e.Column.Name.Equals("SUBLOTID"))
                {
                    string subLotID = Util.NVC(dataGrid.GetValue(e.Row.Index, "SUBLOTID"));
                    if (string.IsNullOrEmpty(subLotID)) e.Cancel = true;
                }
            }
        }

        private void dgSfInsBasInput_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg.CurrentRow.Index > 5)
                {
                    if (!dg.CurrentColumn.Name.Equals("SUBLOTID") && !dg.CurrentColumn.Name.Equals("TITLE1"))
                    {
                        string sValue = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name));
                        double iValue;

                        if (!string.IsNullOrEmpty(sValue))
                        {
                            if (!string.IsNullOrWhiteSpace(sValue) && !double.TryParse(sValue, out iValue))
                            {
                                DataTableConverter.SetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name, string.Empty);
                                //SFU3435	숫자만 입력해주세요
                                Util.MessageInfo("SFU3435");
                            }
                            else
                            {
                                ((C1.WPF.DataGrid.DataGridBoundColumn)(dgSfInsBasInput.Columns[dg.CurrentCell.Column.Name])).Format = "###,##0.#0";
                                string sDBValue = Convert.ToDouble(DataTableConverter.GetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name)).ToString("###,##0.#0");
                                DataTableConverter.SetValue(dg.CurrentCell.Row.DataItem, dg.CurrentCell.Column.Name, Convert.ToDecimal(sDBValue));
                                _iRow = dg.CurrentRow.Index;
                                _iCol = dg.CurrentColumn.Index;

                                CheckValidation(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Name);
                            }
                        }
                        else
                        {
                            CheckValidation(dg.CurrentCell.Row.Index, dg.CurrentCell.Column.Name);
                        }
                    }
                    //20221020_저장 시 파라미터 값 추가(PROD_LOTID) START
                    else if (dg.CurrentColumn.Name.Equals("SUBLOTID"))
                    {
                        string sSubLotID = Util.NVC(DataTableConverter.GetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name));

                        if (!string.IsNullOrEmpty(sSubLotID))
                        {

                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "RQSTDT";
                            dtRqst.Columns.Add("AREAID", typeof(string));
                            dtRqst.Columns.Add("SUBLOTID", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                            dr["SUBLOTID"] = sSubLotID;
                            dtRqst.Rows.Add(dr);

                            DataTable dtRsltVal = new ClientProxy().ExecuteServiceSync("DA_SEL_SUBLOT_CHECK_INFO", "RQSTDT", "RSLTDT", dtRqst);

                            if (dtRsltVal.Rows.Count > 0)
                            {
                                string sModelID = Util.NVC(dtRsltVal.Rows[0]["MDLLOT_ID"]);

                                if (!sModelID.Equals(Util.NVC(cboModel.SelectedValue)))
                                {
                                    DataTableConverter.SetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name, string.Empty);
                                    dg.GetCell(dg.CurrentRow.Index, dg.CurrentColumn.Index).Presenter.Tag = string.Empty;
                                    Util.MessageValidation("FM_ME_0459", new string[] { Util.NVC(cboModel.SelectedValue), sSubLotID });  //모델[%1]이 다른 Cell ID[%2]를 입력하셨습니다.
                                    return;
                                }

                                DataTableConverter.SetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name, Util.NVC(dtRsltVal.Rows[0]["SUBLOTID"]));
                                dg.GetCell(dg.CurrentRow.Index, dg.CurrentColumn.Index).Presenter.Tag = Util.NVC(dtRsltVal.Rows[0]["PROD_LOTID"]);
                            }
                            else
                            {
                                DataTableConverter.SetValue(dg.Rows[dg.CurrentRow.Index].DataItem, dg.CurrentColumn.Name, string.Empty);
                                dg.GetCell(dg.CurrentRow.Index, dg.CurrentColumn.Index).Presenter.Tag = string.Empty;
                                Util.MessageValidation("FM_ME_0003", new string[] { sSubLotID });  //[Cell ID : %1]의 정보가 존재하지 않습니다.
                                return;
                            }
                        }
                        else
                        {
                            dg.GetCell(dg.CurrentRow.Index, dg.CurrentColumn.Index).Presenter.Tag = string.Empty;
                        }
                    }
                    //20221020_저장 시 파라미터 값 추가(PROD_LOTID) END
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [조회 Tab]

        private void cboSelSearch_Click(object sender, RoutedEventArgs e)
        {
            GetListPeriod();
        }

        private void dgInspList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    e.Cell.Presenter.Column.IsReadOnly = true;
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (e.Cell.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }

                    if (e.Cell.Column.Name.Equals("TITLE3"))
                    {
                        e.Cell.Column.Visibility = Visibility.Collapsed;
                    }

                    if (e.Cell.Row.Index < 6)
                    {
                        if (e.Cell.Row.Index.Equals(5))
                        {
                            dgInspList.Rows[5].Visibility = Visibility.Collapsed;
                        }
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        e.Cell.Presenter.IsEnabled = false;
                    }
                }
            }));
        }

        private void dgInspList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null && grid.ItemsSource != null && grid.Rows.Count > 0)
            {
                var _mergeList = new List<DataGridCellsRange>();

                _mergeList.Add(new DataGridCellsRange(grid.GetCell(0, 2), grid.GetCell(5, 2)));
                for (int j = 0; j < grid.GetRowCount(); j++)
                {
                    _mergeList.Add(new DataGridCellsRange(grid.GetCell(j, 3), grid.GetCell(j, 5)));
                }

                foreach (var range in _mergeList)
                {
                    e.Merge(range);
                }
            }
        }

        private void dgInspList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (dataGrid != null)
            {
                e.Cancel = true;
            }
        }

        private void SetGridHeaderNumericColumnAdd(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridNumericColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                IsReadOnly = true,
                Format = "#,##0.##",
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel)
            });
        }

        private void SetGridHeaderTextColumnAdd(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.OneWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                IsReadOnly = false,
                Format = "#,##0.##",
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Pixel),
                MinWidth = 100
            });
        }
        
        #endregion

        #endregion


        private void GetTestData(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["CLCTITEM"] = "C0000068";
            row1["CLCT_MEASR_PONT_CODE"] = "UN1";
            row1["CLCT_VALUE"] = "261";
            row1["WRK_DTTM"] = "2021-03-26 03:00:00";
            row1["WRKR_NAME"] = "张军辉";
            row1["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["CLCTITEM"] = "C0000068";
            row2["CLCT_MEASR_PONT_CODE"] = "UN2";
            row2["CLCT_VALUE"] = "260";
            row2["WRK_DTTM"] = "2021-03-26 03:00:00";
            row2["WRKR_NAME"] = "张军辉";
            row2["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["CLCTITEM"] = "C0000068";
            row3["CLCT_MEASR_PONT_CODE"] = "UN3";
            row3["CLCT_VALUE"] = "263";
            row3["WRK_DTTM"] = "2021-03-26 03:00:00";
            row3["WRKR_NAME"] = "张军辉";
            row3["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["CLCTITEM"] = "C0000068";
            row4["CLCT_MEASR_PONT_CODE"] = "UN4";
            row4["CLCT_VALUE"] = "261";
            row4["WRK_DTTM"] = "2021-03-26 03:00:00";
            row4["WRKR_NAME"] = "张军辉";
            row4["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["CLCTITEM"] = "C0000068";
            row5["CLCT_MEASR_PONT_CODE"] = "UN1";
            row5["CLCT_VALUE"] = "259";
            row5["WRK_DTTM"] = "2021-03-26 10:00:00";
            row5["WRKR_NAME"] = "刘银玲";
            row5["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["CLCTITEM"] = "C0000068";
            row6["CLCT_MEASR_PONT_CODE"] = "UN2";
            row6["CLCT_VALUE"] = "256";
            row6["WRK_DTTM"] = "2021-03-26 10:00:00";
            row6["WRKR_NAME"] = "刘银玲";
            row6["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["CLCTITEM"] = "C0000068";
            row7["CLCT_MEASR_PONT_CODE"] = "UN3";
            row7["CLCT_VALUE"] = "258";
            row7["WRK_DTTM"] = "2021-03-26 10:00:00";
            row7["WRKR_NAME"] = "刘银玲";
            row7["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["CLCTITEM"] = "C0000068";
            row8["CLCT_MEASR_PONT_CODE"] = "UN4";
            row8["CLCT_VALUE"] = "256";
            row8["WRK_DTTM"] = "2021-03-26 10:00:00";
            row8["WRKR_NAME"] = "刘银玲";
            row8["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["CLCTITEM"] = "C0000069";
            row9["CLCT_MEASR_PONT_CODE"] = "UN1";
            row9["CLCT_VALUE"] = "257";
            row9["WRK_DTTM"] = "2021-03-26 10:00:00";
            row9["WRKR_NAME"] = "刘银玲";
            row9["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["CLCTITEM"] = "C0000069";
            row10["CLCT_MEASR_PONT_CODE"] = "UN2";
            row10["CLCT_VALUE"] = "255";
            row10["WRK_DTTM"] = "2021-03-26 10:00:00";
            row10["WRKR_NAME"] = "刘银玲";
            row10["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["CLCTITEM"] = "C0000069";
            row11["CLCT_MEASR_PONT_CODE"] = "UN3";
            row11["CLCT_VALUE"] = "259";
            row11["WRK_DTTM"] = "2021-03-26 10:00:00";
            row11["WRKR_NAME"] = "刘银玲";
            row11["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row11);
            DataRow row12 = dt.NewRow();
            row12["CLCTITEM"] = "C0000069";
            row12["CLCT_MEASR_PONT_CODE"] = "UN4";
            row12["CLCT_VALUE"] = "255";
            row12["WRK_DTTM"] = "2021-03-26 10:00:00";
            row12["WRKR_NAME"] = "刘银玲";
            row12["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row12);
            DataRow row13 = dt.NewRow();
            row13["CLCTITEM"] = "C0000069";
            row13["CLCT_MEASR_PONT_CODE"] = "UN1";
            row13["CLCT_VALUE"] = "262";
            row13["WRK_DTTM"] = "2021-03-26 03:00:00";
            row13["WRKR_NAME"] = "张军辉";
            row13["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row13);
            DataRow row14 = dt.NewRow();
            row14["CLCTITEM"] = "C0000069";
            row14["CLCT_MEASR_PONT_CODE"] = "UN2";
            row14["CLCT_VALUE"] = "258";
            row14["WRK_DTTM"] = "2021-03-26 03:00:00";
            row14["WRKR_NAME"] = "张军辉";
            row14["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row14);
            DataRow row15 = dt.NewRow();
            row15["CLCTITEM"] = "C0000069";
            row15["CLCT_MEASR_PONT_CODE"] = "UN3";
            row15["CLCT_VALUE"] = "262";
            row15["WRK_DTTM"] = "2021-03-26 03:00:00";
            row15["WRKR_NAME"] = "张军辉";
            row15["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row15);
            DataRow row16 = dt.NewRow();
            row16["CLCTITEM"] = "C0000069";
            row16["CLCT_MEASR_PONT_CODE"] = "UN4";
            row16["CLCT_VALUE"] = "259";
            row16["WRK_DTTM"] = "2021-03-26 03:00:00";
            row16["WRKR_NAME"] = "张军辉";
            row16["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row16);
            DataRow row17 = dt.NewRow();
            row17["CLCTITEM"] = "C0000070";
            row17["CLCT_MEASR_PONT_CODE"] = "UN1";
            row17["CLCT_VALUE"] = "264";
            row17["WRK_DTTM"] = "2021-03-26 03:00:00";
            row17["WRKR_NAME"] = "张军辉";
            row17["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row17);
            DataRow row18 = dt.NewRow();
            row18["CLCTITEM"] = "C0000070";
            row18["CLCT_MEASR_PONT_CODE"] = "UN2";
            row18["CLCT_VALUE"] = "262";
            row18["WRK_DTTM"] = "2021-03-26 03:00:00";
            row18["WRKR_NAME"] = "张军辉";
            row18["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row18);
            DataRow row19 = dt.NewRow();
            row19["CLCTITEM"] = "C0000070";
            row19["CLCT_MEASR_PONT_CODE"] = "UN3";
            row19["CLCT_VALUE"] = "265";
            row19["WRK_DTTM"] = "2021-03-26 03:00:00";
            row19["WRKR_NAME"] = "张军辉";
            row19["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row19);
            DataRow row20 = dt.NewRow();
            row20["CLCTITEM"] = "C0000070";
            row20["CLCT_MEASR_PONT_CODE"] = "UN4";
            row20["CLCT_VALUE"] = "260";
            row20["WRK_DTTM"] = "2021-03-26 03:00:00";
            row20["WRKR_NAME"] = "张军辉";
            row20["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row20);
            DataRow row21 = dt.NewRow();
            row21["CLCTITEM"] = "C0000070";
            row21["CLCT_MEASR_PONT_CODE"] = "UN1";
            row21["CLCT_VALUE"] = "257";
            row21["WRK_DTTM"] = "2021-03-26 10:00:00";
            row21["WRKR_NAME"] = "刘银玲";
            row21["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row21);
            DataRow row22 = dt.NewRow();
            row22["CLCTITEM"] = "C0000070";
            row22["CLCT_MEASR_PONT_CODE"] = "UN2";
            row22["CLCT_VALUE"] = "256";
            row22["WRK_DTTM"] = "2021-03-26 10:00:00";
            row22["WRKR_NAME"] = "刘银玲";
            row22["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row22);
            DataRow row23 = dt.NewRow();
            row23["CLCTITEM"] = "C0000070";
            row23["CLCT_MEASR_PONT_CODE"] = "UN3";
            row23["CLCT_VALUE"] = "256";
            row23["WRK_DTTM"] = "2021-03-26 10:00:00";
            row23["WRKR_NAME"] = "刘银玲";
            row23["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row23);
            DataRow row24 = dt.NewRow();
            row24["CLCTITEM"] = "C0000070";
            row24["CLCT_MEASR_PONT_CODE"] = "UN4";
            row24["CLCT_VALUE"] = "256";
            row24["WRK_DTTM"] = "2021-03-26 10:00:00";
            row24["WRKR_NAME"] = "刘银玲";
            row24["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row24);
            DataRow row25 = dt.NewRow();
            row25["CLCTITEM"] = "C0000071";
            row25["CLCT_MEASR_PONT_CODE"] = "UN1";
            row25["CLCT_VALUE"] = "1.5";
            row25["WRK_DTTM"] = "2021-03-26 10:00:00";
            row25["WRKR_NAME"] = "刘银玲";
            row25["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row25);
            DataRow row26 = dt.NewRow();
            row26["CLCTITEM"] = "C0000071";
            row26["CLCT_MEASR_PONT_CODE"] = "UN2";
            row26["CLCT_VALUE"] = "1.5";
            row26["WRK_DTTM"] = "2021-03-26 10:00:00";
            row26["WRKR_NAME"] = "刘银玲";
            row26["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row26);
            DataRow row27 = dt.NewRow();
            row27["CLCTITEM"] = "C0000071";
            row27["CLCT_MEASR_PONT_CODE"] = "UN3";
            row27["CLCT_VALUE"] = "1.6";
            row27["WRK_DTTM"] = "2021-03-26 10:00:00";
            row27["WRKR_NAME"] = "刘银玲";
            row27["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row27);
            DataRow row28 = dt.NewRow();
            row28["CLCTITEM"] = "C0000071";
            row28["CLCT_MEASR_PONT_CODE"] = "UN4";
            row28["CLCT_VALUE"] = "1.5";
            row28["WRK_DTTM"] = "2021-03-26 10:00:00";
            row28["WRKR_NAME"] = "刘银玲";
            row28["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row28);
            DataRow row29 = dt.NewRow();
            row29["CLCTITEM"] = "C0000071";
            row29["CLCT_MEASR_PONT_CODE"] = "UN1";
            row29["CLCT_VALUE"] = "2";
            row29["WRK_DTTM"] = "2021-03-26 03:00:00";
            row29["WRKR_NAME"] = "张军辉";
            row29["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row29);
            DataRow row30 = dt.NewRow();
            row30["CLCTITEM"] = "C0000071";
            row30["CLCT_MEASR_PONT_CODE"] = "UN2";
            row30["CLCT_VALUE"] = "2.1";
            row30["WRK_DTTM"] = "2021-03-26 03:00:00";
            row30["WRKR_NAME"] = "张军辉";
            row30["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row30);
            DataRow row31 = dt.NewRow();
            row31["CLCTITEM"] = "C0000071";
            row31["CLCT_MEASR_PONT_CODE"] = "UN3";
            row31["CLCT_VALUE"] = "1.9";
            row31["WRK_DTTM"] = "2021-03-26 03:00:00";
            row31["WRKR_NAME"] = "张军辉";
            row31["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row31);
            DataRow row32 = dt.NewRow();
            row32["CLCTITEM"] = "C0000071";
            row32["CLCT_MEASR_PONT_CODE"] = "UN4";
            row32["CLCT_VALUE"] = "2";
            row32["WRK_DTTM"] = "2021-03-26 03:00:00";
            row32["WRKR_NAME"] = "张军辉";
            row32["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row32);
            DataRow row33 = dt.NewRow();
            row33["CLCTITEM"] = "C0000072";
            row33["CLCT_MEASR_PONT_CODE"] = "UN1";
            row33["CLCT_VALUE"] = "2";
            row33["WRK_DTTM"] = "2021-03-26 03:00:00";
            row33["WRKR_NAME"] = "张军辉";
            row33["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row33);
            DataRow row34 = dt.NewRow();
            row34["CLCTITEM"] = "C0000072";
            row34["CLCT_MEASR_PONT_CODE"] = "UN2";
            row34["CLCT_VALUE"] = "2";
            row34["WRK_DTTM"] = "2021-03-26 03:00:00";
            row34["WRKR_NAME"] = "张军辉";
            row34["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row34);
            DataRow row35 = dt.NewRow();
            row35["CLCTITEM"] = "C0000072";
            row35["CLCT_MEASR_PONT_CODE"] = "UN3";
            row35["CLCT_VALUE"] = "2";
            row35["WRK_DTTM"] = "2021-03-26 03:00:00";
            row35["WRKR_NAME"] = "张军辉";
            row35["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row35);
            DataRow row36 = dt.NewRow();
            row36["CLCTITEM"] = "C0000072";
            row36["CLCT_MEASR_PONT_CODE"] = "UN4";
            row36["CLCT_VALUE"] = "1.9";
            row36["WRK_DTTM"] = "2021-03-26 03:00:00";
            row36["WRKR_NAME"] = "张军辉";
            row36["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row36);
            DataRow row37 = dt.NewRow();
            row37["CLCTITEM"] = "C0000072";
            row37["CLCT_MEASR_PONT_CODE"] = "UN1";
            row37["CLCT_VALUE"] = "1.5";
            row37["WRK_DTTM"] = "2021-03-26 10:00:00";
            row37["WRKR_NAME"] = "刘银玲";
            row37["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row37);
            DataRow row38 = dt.NewRow();
            row38["CLCTITEM"] = "C0000072";
            row38["CLCT_MEASR_PONT_CODE"] = "UN2";
            row38["CLCT_VALUE"] = "1.5";
            row38["WRK_DTTM"] = "2021-03-26 10:00:00";
            row38["WRKR_NAME"] = "刘银玲";
            row38["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row38);
            DataRow row39 = dt.NewRow();
            row39["CLCTITEM"] = "C0000072";
            row39["CLCT_MEASR_PONT_CODE"] = "UN3";
            row39["CLCT_VALUE"] = "1.6";
            row39["WRK_DTTM"] = "2021-03-26 10:00:00";
            row39["WRKR_NAME"] = "刘银玲";
            row39["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row39);
            DataRow row40 = dt.NewRow();
            row40["CLCTITEM"] = "C0000072";
            row40["CLCT_MEASR_PONT_CODE"] = "UN4";
            row40["CLCT_VALUE"] = "1.6";
            row40["WRK_DTTM"] = "2021-03-26 10:00:00";
            row40["WRKR_NAME"] = "刘银玲";
            row40["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row40);
            DataRow row41 = dt.NewRow();
            row41["CLCTITEM"] = "C0000073";
            row41["CLCT_MEASR_PONT_CODE"] = "UN1";
            row41["CLCT_VALUE"] = "1.5";
            row41["WRK_DTTM"] = "2021-03-26 10:00:00";
            row41["WRKR_NAME"] = "刘银玲";
            row41["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row41);
            DataRow row42 = dt.NewRow();
            row42["CLCTITEM"] = "C0000073";
            row42["CLCT_MEASR_PONT_CODE"] = "UN2";
            row42["CLCT_VALUE"] = "1.6";
            row42["WRK_DTTM"] = "2021-03-26 10:00:00";
            row42["WRKR_NAME"] = "刘银玲";
            row42["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row42);
            DataRow row43 = dt.NewRow();
            row43["CLCTITEM"] = "C0000073";
            row43["CLCT_MEASR_PONT_CODE"] = "UN3";
            row43["CLCT_VALUE"] = "1.5";
            row43["WRK_DTTM"] = "2021-03-26 10:00:00";
            row43["WRKR_NAME"] = "刘银玲";
            row43["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row43);
            DataRow row44 = dt.NewRow();
            row44["CLCTITEM"] = "C0000073";
            row44["CLCT_MEASR_PONT_CODE"] = "UN4";
            row44["CLCT_VALUE"] = "1.5";
            row44["WRK_DTTM"] = "2021-03-26 10:00:00";
            row44["WRKR_NAME"] = "刘银玲";
            row44["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row44);
            DataRow row45 = dt.NewRow();
            row45["CLCTITEM"] = "C0000073";
            row45["CLCT_MEASR_PONT_CODE"] = "UN1";
            row45["CLCT_VALUE"] = "2";
            row45["WRK_DTTM"] = "2021-03-26 03:00:00";
            row45["WRKR_NAME"] = "张军辉";
            row45["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row45);
            DataRow row46 = dt.NewRow();
            row46["CLCTITEM"] = "C0000073";
            row46["CLCT_MEASR_PONT_CODE"] = "UN2";
            row46["CLCT_VALUE"] = "2";
            row46["WRK_DTTM"] = "2021-03-26 03:00:00";
            row46["WRKR_NAME"] = "张军辉";
            row46["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row46);
            DataRow row47 = dt.NewRow();
            row47["CLCTITEM"] = "C0000073";
            row47["CLCT_MEASR_PONT_CODE"] = "UN3";
            row47["CLCT_VALUE"] = "2.1";
            row47["WRK_DTTM"] = "2021-03-26 03:00:00";
            row47["WRKR_NAME"] = "张军辉";
            row47["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row47);
            DataRow row48 = dt.NewRow();
            row48["CLCTITEM"] = "C0000073";
            row48["CLCT_MEASR_PONT_CODE"] = "UN4";
            row48["CLCT_VALUE"] = "2";
            row48["WRK_DTTM"] = "2021-03-26 03:00:00";
            row48["WRKR_NAME"] = "张军辉";
            row48["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row48);
            DataRow row49 = dt.NewRow();
            row49["CLCTITEM"] = "C0000074";
            row49["CLCT_MEASR_PONT_CODE"] = "UN1";
            row49["CLCT_VALUE"] = "1.9";
            row49["WRK_DTTM"] = "2021-03-26 03:00:00";
            row49["WRKR_NAME"] = "张军辉";
            row49["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row49);
            DataRow row50 = dt.NewRow();
            row50["CLCTITEM"] = "C0000074";
            row50["CLCT_MEASR_PONT_CODE"] = "UN2";
            row50["CLCT_VALUE"] = "2";
            row50["WRK_DTTM"] = "2021-03-26 03:00:00";
            row50["WRKR_NAME"] = "张军辉";
            row50["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row50);
            DataRow row51 = dt.NewRow();
            row51["CLCTITEM"] = "C0000074";
            row51["CLCT_MEASR_PONT_CODE"] = "UN3";
            row51["CLCT_VALUE"] = "2";
            row51["WRK_DTTM"] = "2021-03-26 03:00:00";
            row51["WRKR_NAME"] = "张军辉";
            row51["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row51);
            DataRow row52 = dt.NewRow();
            row52["CLCTITEM"] = "C0000074";
            row52["CLCT_MEASR_PONT_CODE"] = "UN4";
            row52["CLCT_VALUE"] = "1.9";
            row52["WRK_DTTM"] = "2021-03-26 03:00:00";
            row52["WRKR_NAME"] = "张军辉";
            row52["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row52);
            DataRow row53 = dt.NewRow();
            row53["CLCTITEM"] = "C0000074";
            row53["CLCT_MEASR_PONT_CODE"] = "UN1";
            row53["CLCT_VALUE"] = "1.5";
            row53["WRK_DTTM"] = "2021-03-26 10:00:00";
            row53["WRKR_NAME"] = "刘银玲";
            row53["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row53);
            DataRow row54 = dt.NewRow();
            row54["CLCTITEM"] = "C0000074";
            row54["CLCT_MEASR_PONT_CODE"] = "UN2";
            row54["CLCT_VALUE"] = "1.5";
            row54["WRK_DTTM"] = "2021-03-26 10:00:00";
            row54["WRKR_NAME"] = "刘银玲";
            row54["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row54);
            DataRow row55 = dt.NewRow();
            row55["CLCTITEM"] = "C0000074";
            row55["CLCT_MEASR_PONT_CODE"] = "UN3";
            row55["CLCT_VALUE"] = "1.5";
            row55["WRK_DTTM"] = "2021-03-26 10:00:00";
            row55["WRKR_NAME"] = "刘银玲";
            row55["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row55);
            DataRow row56 = dt.NewRow();
            row56["CLCTITEM"] = "C0000074";
            row56["CLCT_MEASR_PONT_CODE"] = "UN4";
            row56["CLCT_VALUE"] = "1.5";
            row56["WRK_DTTM"] = "2021-03-26 10:00:00";
            row56["WRKR_NAME"] = "刘银玲";
            row56["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row56);
            DataRow row57 = dt.NewRow();
            row57["CLCTITEM"] = "C0000434";
            row57["CLCT_MEASR_PONT_CODE"] = "UN1";
            row57["CLCT_VALUE"] = "9.5";
            row57["WRK_DTTM"] = "2021-03-26 10:00:00";
            row57["WRKR_NAME"] = "刘银玲";
            row57["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row57);
            DataRow row58 = dt.NewRow();
            row58["CLCTITEM"] = "C0000434";
            row58["CLCT_MEASR_PONT_CODE"] = "UN2";
            row58["CLCT_VALUE"] = "9.5";
            row58["WRK_DTTM"] = "2021-03-26 10:00:00";
            row58["WRKR_NAME"] = "刘银玲";
            row58["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row58);
            DataRow row59 = dt.NewRow();
            row59["CLCTITEM"] = "C0000434";
            row59["CLCT_MEASR_PONT_CODE"] = "UN3";
            row59["CLCT_VALUE"] = "9.5";
            row59["WRK_DTTM"] = "2021-03-26 10:00:00";
            row59["WRKR_NAME"] = "刘银玲";
            row59["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row59);
            DataRow row60 = dt.NewRow();
            row60["CLCTITEM"] = "C0000434";
            row60["CLCT_MEASR_PONT_CODE"] = "UN4";
            row60["CLCT_VALUE"] = "9.5";
            row60["WRK_DTTM"] = "2021-03-26 10:00:00";
            row60["WRKR_NAME"] = "刘银玲";
            row60["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row60);
            DataRow row61 = dt.NewRow();
            row61["CLCTITEM"] = "C0000434";
            row61["CLCT_MEASR_PONT_CODE"] = "UN1";
            row61["CLCT_VALUE"] = "9.5";
            row61["WRK_DTTM"] = "2021-03-26 03:00:00";
            row61["WRKR_NAME"] = "张军辉";
            row61["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row61);
            DataRow row62 = dt.NewRow();
            row62["CLCTITEM"] = "C0000434";
            row62["CLCT_MEASR_PONT_CODE"] = "UN2";
            row62["CLCT_VALUE"] = "9.5";
            row62["WRK_DTTM"] = "2021-03-26 03:00:00";
            row62["WRKR_NAME"] = "张军辉";
            row62["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row62);
            DataRow row63 = dt.NewRow();
            row63["CLCTITEM"] = "C0000434";
            row63["CLCT_MEASR_PONT_CODE"] = "UN3";
            row63["CLCT_VALUE"] = "9.5";
            row63["WRK_DTTM"] = "2021-03-26 03:00:00";
            row63["WRKR_NAME"] = "张军辉";
            row63["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row63);
            DataRow row64 = dt.NewRow();
            row64["CLCTITEM"] = "C0000434";
            row64["CLCT_MEASR_PONT_CODE"] = "UN4";
            row64["CLCT_VALUE"] = "9.5";
            row64["WRK_DTTM"] = "2021-03-26 03:00:00";
            row64["WRKR_NAME"] = "张军辉";
            row64["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row64);
            DataRow row65 = dt.NewRow();
            row65["CLCTITEM"] = "C0000435";
            row65["CLCT_MEASR_PONT_CODE"] = "UN1";
            row65["CLCT_VALUE"] = "9.5";
            row65["WRK_DTTM"] = "2021-03-26 03:00:00";
            row65["WRKR_NAME"] = "张军辉";
            row65["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row65);
            DataRow row66 = dt.NewRow();
            row66["CLCTITEM"] = "C0000435";
            row66["CLCT_MEASR_PONT_CODE"] = "UN2";
            row66["CLCT_VALUE"] = "9.5";
            row66["WRK_DTTM"] = "2021-03-26 03:00:00";
            row66["WRKR_NAME"] = "张军辉";
            row66["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row66);
            DataRow row67 = dt.NewRow();
            row67["CLCTITEM"] = "C0000435";
            row67["CLCT_MEASR_PONT_CODE"] = "UN3";
            row67["CLCT_VALUE"] = "9.5";
            row67["WRK_DTTM"] = "2021-03-26 03:00:00";
            row67["WRKR_NAME"] = "张军辉";
            row67["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row67);
            DataRow row68 = dt.NewRow();
            row68["CLCTITEM"] = "C0000435";
            row68["CLCT_MEASR_PONT_CODE"] = "UN4";
            row68["CLCT_VALUE"] = "9.5";
            row68["WRK_DTTM"] = "2021-03-26 03:00:00";
            row68["WRKR_NAME"] = "张军辉";
            row68["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row68);
            DataRow row69 = dt.NewRow();
            row69["CLCTITEM"] = "C0000435";
            row69["CLCT_MEASR_PONT_CODE"] = "UN1";
            row69["CLCT_VALUE"] = "9.5";
            row69["WRK_DTTM"] = "2021-03-26 10:00:00";
            row69["WRKR_NAME"] = "刘银玲";
            row69["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row69);
            DataRow row70 = dt.NewRow();
            row70["CLCTITEM"] = "C0000435";
            row70["CLCT_MEASR_PONT_CODE"] = "UN2";
            row70["CLCT_VALUE"] = "9.5";
            row70["WRK_DTTM"] = "2021-03-26 10:00:00";
            row70["WRKR_NAME"] = "刘银玲";
            row70["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row70);
            DataRow row71 = dt.NewRow();
            row71["CLCTITEM"] = "C0000435";
            row71["CLCT_MEASR_PONT_CODE"] = "UN3";
            row71["CLCT_VALUE"] = "9.5";
            row71["WRK_DTTM"] = "2021-03-26 10:00:00";
            row71["WRKR_NAME"] = "刘银玲";
            row71["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row71);
            DataRow row72 = dt.NewRow();
            row72["CLCTITEM"] = "C0000435";
            row72["CLCT_MEASR_PONT_CODE"] = "UN4";
            row72["CLCT_VALUE"] = "9.5";
            row72["WRK_DTTM"] = "2021-03-26 10:00:00";
            row72["WRKR_NAME"] = "刘银玲";
            row72["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row72);
            DataRow row73 = dt.NewRow();
            row73["CLCTITEM"] = "C0000436";
            row73["CLCT_MEASR_PONT_CODE"] = "UN1";
            row73["CLCT_VALUE"] = "9.5";
            row73["WRK_DTTM"] = "2021-03-26 10:00:00";
            row73["WRKR_NAME"] = "刘银玲";
            row73["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row73);
            DataRow row74 = dt.NewRow();
            row74["CLCTITEM"] = "C0000436";
            row74["CLCT_MEASR_PONT_CODE"] = "UN2";
            row74["CLCT_VALUE"] = "9.5";
            row74["WRK_DTTM"] = "2021-03-26 10:00:00";
            row74["WRKR_NAME"] = "刘银玲";
            row74["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row74);
            DataRow row75 = dt.NewRow();
            row75["CLCTITEM"] = "C0000436";
            row75["CLCT_MEASR_PONT_CODE"] = "UN3";
            row75["CLCT_VALUE"] = "9.5";
            row75["WRK_DTTM"] = "2021-03-26 10:00:00";
            row75["WRKR_NAME"] = "刘银玲";
            row75["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row75);
            DataRow row76 = dt.NewRow();
            row76["CLCTITEM"] = "C0000436";
            row76["CLCT_MEASR_PONT_CODE"] = "UN4";
            row76["CLCT_VALUE"] = "9.5";
            row76["WRK_DTTM"] = "2021-03-26 10:00:00";
            row76["WRKR_NAME"] = "刘银玲";
            row76["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row76);
            DataRow row77 = dt.NewRow();
            row77["CLCTITEM"] = "C0000436";
            row77["CLCT_MEASR_PONT_CODE"] = "UN1";
            row77["CLCT_VALUE"] = "9.5";
            row77["WRK_DTTM"] = "2021-03-26 03:00:00";
            row77["WRKR_NAME"] = "张军辉";
            row77["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row77);
            DataRow row78 = dt.NewRow();
            row78["CLCTITEM"] = "C0000436";
            row78["CLCT_MEASR_PONT_CODE"] = "UN2";
            row78["CLCT_VALUE"] = "9.5";
            row78["WRK_DTTM"] = "2021-03-26 03:00:00";
            row78["WRKR_NAME"] = "张军辉";
            row78["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row78);
            DataRow row79 = dt.NewRow();
            row79["CLCTITEM"] = "C0000436";
            row79["CLCT_MEASR_PONT_CODE"] = "UN3";
            row79["CLCT_VALUE"] = "9.5";
            row79["WRK_DTTM"] = "2021-03-26 03:00:00";
            row79["WRKR_NAME"] = "张军辉";
            row79["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row79);
            DataRow row80 = dt.NewRow();
            row80["CLCTITEM"] = "C0000436";
            row80["CLCT_MEASR_PONT_CODE"] = "UN4";
            row80["CLCT_VALUE"] = "9.5";
            row80["WRK_DTTM"] = "2021-03-26 03:00:00";
            row80["WRKR_NAME"] = "张军辉";
            row80["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row80);
            DataRow row81 = dt.NewRow();
            row81["CLCTITEM"] = "C0000437";
            row81["CLCT_MEASR_PONT_CODE"] = "UN1";
            row81["CLCT_VALUE"] = "9.5";
            row81["WRK_DTTM"] = "2021-03-26 03:00:00";
            row81["WRKR_NAME"] = "张军辉";
            row81["SUBLOTID"] = "UACN213211";
            dt.Rows.Add(row81);
            DataRow row82 = dt.NewRow();
            row82["CLCTITEM"] = "C0000437";
            row82["CLCT_MEASR_PONT_CODE"] = "UN2";
            row82["CLCT_VALUE"] = "9.5";
            row82["WRK_DTTM"] = "2021-03-26 03:00:00";
            row82["WRKR_NAME"] = "张军辉";
            row82["SUBLOTID"] = "UACN213214";
            dt.Rows.Add(row82);
            DataRow row83 = dt.NewRow();
            row83["CLCTITEM"] = "C0000437";
            row83["CLCT_MEASR_PONT_CODE"] = "UN3";
            row83["CLCT_VALUE"] = "9.5";
            row83["WRK_DTTM"] = "2021-03-26 03:00:00";
            row83["WRKR_NAME"] = "张军辉";
            row83["SUBLOTID"] = "UACN213213";
            dt.Rows.Add(row83);
            DataRow row84 = dt.NewRow();
            row84["CLCTITEM"] = "C0000437";
            row84["CLCT_MEASR_PONT_CODE"] = "UN4";
            row84["CLCT_VALUE"] = "9.5";
            row84["WRK_DTTM"] = "2021-03-26 03:00:00";
            row84["WRKR_NAME"] = "张军辉";
            row84["SUBLOTID"] = "UACN213193";
            dt.Rows.Add(row84);
            DataRow row85 = dt.NewRow();
            row85["CLCTITEM"] = "C0000437";
            row85["CLCT_MEASR_PONT_CODE"] = "UN1";
            row85["CLCT_VALUE"] = "9.5";
            row85["WRK_DTTM"] = "2021-03-26 10:00:00";
            row85["WRKR_NAME"] = "刘银玲";
            row85["SUBLOTID"] = "UACN220648";
            dt.Rows.Add(row85);
            DataRow row86 = dt.NewRow();
            row86["CLCTITEM"] = "C0000437";
            row86["CLCT_MEASR_PONT_CODE"] = "UN2";
            row86["CLCT_VALUE"] = "9.5";
            row86["WRK_DTTM"] = "2021-03-26 10:00:00";
            row86["WRKR_NAME"] = "刘银玲";
            row86["SUBLOTID"] = "UACN220650";
            dt.Rows.Add(row86);
            DataRow row87 = dt.NewRow();
            row87["CLCTITEM"] = "C0000437";
            row87["CLCT_MEASR_PONT_CODE"] = "UN3";
            row87["CLCT_VALUE"] = "9.5";
            row87["WRK_DTTM"] = "2021-03-26 10:00:00";
            row87["WRKR_NAME"] = "刘银玲";
            row87["SUBLOTID"] = "UACN220665";
            dt.Rows.Add(row87);
            DataRow row88 = dt.NewRow();
            row88["CLCTITEM"] = "C0000437";
            row88["CLCT_MEASR_PONT_CODE"] = "UN4";
            row88["CLCT_VALUE"] = "9.5";
            row88["WRK_DTTM"] = "2021-03-26 10:00:00";
            row88["WRKR_NAME"] = "刘银玲";
            row88["SUBLOTID"] = "UACN220647";
            dt.Rows.Add(row88);
            #endregion

        }

        private void GetTestData_1(ref DataTable dt)
        {
            dt.TableName = "RQSTDT";
            //dt.Columns.Add("TRAY_OP_STATUS_CD", typeof(string));

            #region ROW ADD
            DataRow row1 = dt.NewRow();
            row1["CLCTITEM"] = "C0000068";
            row1["CLCT_NAME"] = "Sealing 두께";
            row1["MDLLOT_ID"] = "UA3";
            row1["PROC_GR_CODE"] = "D";
            row1["CLCT_MTHD_TYPE_CODE"] = "C";
            row1["CLCTTYPE"] = "0";
            row1["TRGT_VALUE"] = "270.00";
            row1["MAX_VALUE"] = "290";
            row1["MIN_VALUE"] = "250";
            row1["MEASR_PSTN_NAME"] = "T1";
            dt.Rows.Add(row1);
            DataRow row2 = dt.NewRow();
            row2["CLCTITEM"] = "C0000069";
            row2["CLCT_NAME"] = "Sealing 두께";
            row2["MDLLOT_ID"] = "UA3";
            row2["PROC_GR_CODE"] = "D";
            row2["CLCT_MTHD_TYPE_CODE"] = "C";
            row2["CLCTTYPE"] = "0";
            row2["TRGT_VALUE"] = "270.00";
            row2["MAX_VALUE"] = "290";
            row2["MIN_VALUE"] = "250";
            row2["MEASR_PSTN_NAME"] = "T2";
            dt.Rows.Add(row2);
            DataRow row3 = dt.NewRow();
            row3["CLCTITEM"] = "C0000070";
            row3["CLCT_NAME"] = "Sealing 두께";
            row3["MDLLOT_ID"] = "UA3";
            row3["PROC_GR_CODE"] = "D";
            row3["CLCT_MTHD_TYPE_CODE"] = "C";
            row3["CLCTTYPE"] = "0";
            row3["TRGT_VALUE"] = "270.00";
            row3["MAX_VALUE"] = "290";
            row3["MIN_VALUE"] = "250";
            row3["MEASR_PSTN_NAME"] = "T3";
            dt.Rows.Add(row3);
            DataRow row4 = dt.NewRow();
            row4["CLCTITEM"] = "C0000436";
            row4["CLCT_NAME"] = "Sealing width（인쇄면）";
            row4["MDLLOT_ID"] = "UA3";
            row4["PROC_GR_CODE"] = "D";
            row4["CLCT_MTHD_TYPE_CODE"] = "C";
            row4["CLCTTYPE"] = "1";
            row4["TRGT_VALUE"] = "0.00";
            row4["MAX_VALUE"] = "9.5";
            row4["MIN_VALUE"] = "9";
            row4["MEASR_PSTN_NAME"] = "T2";
            dt.Rows.Add(row4);
            DataRow row5 = dt.NewRow();
            row5["CLCTITEM"] = "C0000437";
            row5["CLCT_NAME"] = "Sealing width（인쇄면）";
            row5["MDLLOT_ID"] = "UA3";
            row5["PROC_GR_CODE"] = "D";
            row5["CLCT_MTHD_TYPE_CODE"] = "C";
            row5["CLCTTYPE"] = "1";
            row5["TRGT_VALUE"] = "0.00";
            row5["MAX_VALUE"] = "9.5";
            row5["MIN_VALUE"] = "9";
            row5["MEASR_PSTN_NAME"] = "T1";
            dt.Rows.Add(row5);
            DataRow row6 = dt.NewRow();
            row6["CLCTITEM"] = "C0000434";
            row6["CLCT_NAME"] = "Sealing width（인쇄뒷면）";
            row6["MDLLOT_ID"] = "UA3";
            row6["PROC_GR_CODE"] = "D";
            row6["CLCT_MTHD_TYPE_CODE"] = "C";
            row6["CLCTTYPE"] = "2";
            row6["TRGT_VALUE"] = "0.00";
            row6["MAX_VALUE"] = "9.5";
            row6["MIN_VALUE"] = "9";
            row6["MEASR_PSTN_NAME"] = "T2";
            dt.Rows.Add(row6);
            DataRow row7 = dt.NewRow();
            row7["CLCTITEM"] = "C0000435";
            row7["CLCT_NAME"] = "Sealing width（인쇄뒷면）";
            row7["MDLLOT_ID"] = "UA3";
            row7["PROC_GR_CODE"] = "D";
            row7["CLCT_MTHD_TYPE_CODE"] = "C";
            row7["CLCTTYPE"] = "2";
            row7["TRGT_VALUE"] = "0.00";
            row7["MAX_VALUE"] = "9.5";
            row7["MIN_VALUE"] = "9";
            row7["MEASR_PSTN_NAME"] = "T1";
            dt.Rows.Add(row7);
            DataRow row8 = dt.NewRow();
            row8["CLCTITEM"] = "C0000071";
            row8["CLCT_NAME"] = "Cup~Sealing Gap";
            row8["MDLLOT_ID"] = "UA3";
            row8["PROC_GR_CODE"] = "D";
            row8["CLCT_MTHD_TYPE_CODE"] = "C";
            row8["CLCTTYPE"] = "C";
            row8["TRGT_VALUE"] = "2.00";
            row8["MAX_VALUE"] = "2.5";
            row8["MIN_VALUE"] = "1.5";
            row8["MEASR_PSTN_NAME"] = "阳极正面";
            dt.Rows.Add(row8);
            DataRow row9 = dt.NewRow();
            row9["CLCTITEM"] = "C0000072";
            row9["CLCT_NAME"] = "Cup~Sealing Gap";
            row9["MDLLOT_ID"] = "UA3";
            row9["PROC_GR_CODE"] = "D";
            row9["CLCT_MTHD_TYPE_CODE"] = "C";
            row9["CLCTTYPE"] = "C";
            row9["TRGT_VALUE"] = "2.00";
            row9["MAX_VALUE"] = "2.5";
            row9["MIN_VALUE"] = "1.5";
            row9["MEASR_PSTN_NAME"] = "阳极反面";
            dt.Rows.Add(row9);
            DataRow row10 = dt.NewRow();
            row10["CLCTITEM"] = "C0000073";
            row10["CLCT_NAME"] = "Cup~Sealing Gap";
            row10["MDLLOT_ID"] = "UA3";
            row10["PROC_GR_CODE"] = "D";
            row10["CLCT_MTHD_TYPE_CODE"] = "C";
            row10["CLCTTYPE"] = "C";
            row10["TRGT_VALUE"] = "2.00";
            row10["MAX_VALUE"] = "2.5";
            row10["MIN_VALUE"] = "1.5";
            row10["MEASR_PSTN_NAME"] = "阴极正面";
            dt.Rows.Add(row10);
            DataRow row11 = dt.NewRow();
            row11["CLCTITEM"] = "C0000074";
            row11["CLCT_NAME"] = "Cup~Sealing Gap";
            row11["MDLLOT_ID"] = "UA3";
            row11["PROC_GR_CODE"] = "D";
            row11["CLCT_MTHD_TYPE_CODE"] = "C";
            row11["CLCTTYPE"] = "C";
            row11["TRGT_VALUE"] = "2.00";
            row11["MAX_VALUE"] = "2.5";
            row11["MIN_VALUE"] = "1.5";
            row11["MEASR_PSTN_NAME"] = "阴极反面";
            dt.Rows.Add(row11);

            #endregion

        }

    }
}


