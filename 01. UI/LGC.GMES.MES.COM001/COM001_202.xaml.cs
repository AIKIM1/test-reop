/*************************************************************************************
 Created Date : 2017.11.13
      Creator : 장만철C
   Decription : 조립재공생성화면
--------------------------------------------------------------------------------------
 [Change History]
  2017.11.13  DEVELOPER : Initial Created.
  2020.05.25  정문교 : C20200526-000379
                       1. 데이터 그리드 위 텍스트 박스 제거					
                       2. 엔터 키 다운 시 validation 제거					
                       3. 제품 입력 항목 상단 메뉴로 이동 및 제품 ID 조회 기능 추가					
                       4. Lot 유형 항목 상단 메뉴로 이동					
                       5. 수출/내수 항목 상단 메뉴로 이동					
  2023.11.07   고재영 변경내용 없으며 본 화면에 대한 근거 설명을 위한 주석
                                 본 화면은 자동차 조립 시생산을 위해 투입 LOT을 발행하기 위해 필요하다고 하여 추가됨
                                 화면 추가와 함께 시생산 유형의 랏만 생성 가능하도록 변경됨
                                 LOT ID채번규칙도 없고, 재고 불일치 문제점이 있음을 설명하였음에도 추가해달라고 함
                                 향후 본 화면을 통해 발생하는 장애에 대해서는 운영팀 귀책이 없음
                                 회의 일자 - 2023년 11월 06일
                                 요청자 -  김광희 책임
                                 근거 자료 - ECM\Shared\전지.GMES\09. GMES 운영업무\897. 기타자료\조립재공생성 화면 근거자료.png
  2023.11.10   장영철 : E20230811-001361
                        LotType 시생산 고정, 유효일자 입력 달력선택 으료 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_202 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        public COM001_202()
        {
            InitializeComponent();
            this.Loaded += COM001_202_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        private void InitializeControls()
        {
        }

        private void InitializeGrid()
        {
            DataTable dtTable = new DataTable();
            dtTable.Columns.Add("LOTID", typeof(string));
            dtTable.Columns.Add("PROD_VER_CODE", typeof(string));
            dtTable.Columns.Add("VLD_DATE", typeof(string));
            dtTable.Columns.Add("WIPQTY", typeof(Int32));

            Util.GridSetData(dgResult, dtTable, null);
        }

        private void InitializeCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";

            #region 재공생성 TAB
            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessRout };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //공정
            C1ComboBox[] cboProcessRoutParent = { cboAreaByAreaType, cboEquipmentSegment };
            _combo.SetCombo(cboProcessRout, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessRoutParent);

            //LotType
            _combo.SetCombo(cboLotType, CommonCombo.ComboStatus.SELECT);
            cboLotType.SelectedValue = "X"; cboLotType.IsEnabled = false;   // 시생산으로 고정

            //COMMCODE
            string[] sFilter = { "MKT_TYPE_CODE" };
            _combo.SetCombo(cboMKType, CommonCombo.ComboStatus.SELECT, sCase: "COMMCODE", sFilter: sFilter);

            #endregion

            #region 재공생성이력 TAB
            //라인            
            C1ComboBox[] cboEquipmentSegmentParent_HIST = { cboAreaByAreaType };
            C1ComboBox[] cboEquipmentSegmentChild_HIST = { cboProcessRout };
            _combo.SetCombo(cboEquipmentSegment_HIST, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent_HIST, cbChild: cboEquipmentSegmentChild_HIST, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessRoutParent_HIST = { cboAreaByAreaType, cboEquipmentSegment };
            _combo.SetCombo(cboProcessRout_HIST, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessRoutParent_HIST, sCase: "PROCESSROUT");
            #endregion

            cboProcessRout.SelectedValueChanged += cboProcessRout_SelectedValueChanged;
        }

        #endregion

        #region Event
        private void COM001_202_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreat);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitializeControls();
            InitializeGrid();
            InitializeCombo();

            //제품 
            SearchProduct();

            this.Loaded -= COM001_202_Loaded;
        }

        private void cboProcessRout_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboProcessRout.SelectedValue == null) return;

            ////제품 
            //SearchProduct();
        }

        private void popSearchProdID_ValueChanged(object sender, EventArgs e)
        {
            //_SelectProdID = Util.NVC(popSearchProdID.SelectedValue);
        }

        /// <summary>
        /// 행추가
        /// </summary>
        private void btnAddRow_Click(object sender, RoutedEventArgs e)
        {
            for (int addrow = 0; addrow < txtWipQty.Value; addrow++)
            {
                dgResult.BeginNewRow();
                dgResult.EndNewRow(true);
            }
        }

        /// <summary>
        /// 행삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button dg = sender as Button;
                if (dg != null &&
                    dg.DataContext != null &&
                    (dg.DataContext as DataRowView).Row != null)
                {
                    dgResult.RemoveRow(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index);
                    dgResult.EndNewRow(true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 생성
        /// </summary>
        private void btnCreat_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            // % 1(을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Create();
                }
            }, ObjectDic.Instance.GetObjectName("재공생성"));
        }

        /// <summary>
        /// 초기화
        /// </summary>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            InitializeGrid();
        }

        /// <summary>
        /// 재공생성이력 조회
        /// </summary>
        private void btnSearch_HIST_Click(object sender, RoutedEventArgs e)
        {
            SearchHistory();
        }

        #endregion


        #region Mehod

        #region [BizCall]
        /// <summary>
        /// 반제품 조회
        /// </summary>
        private void SearchProduct()
        {
            try
            {
                if (cboProcessRout.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    popSearchProdID.ItemsSource = null;
                    return;
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SHOPID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_VW_PRODUCT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        popSearchProdID.ItemsSource = DataTableConverter.Convert(searchResult);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// PCSGID 조회
        /// </summary>
        private string SearchPCSGID()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID; ;
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESSSEGMENT_BY_EQSGID_CBO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0]["CBO_CODE"].ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 재공생성
        /// </summary>
        private void Create()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                /////////////////////////////////////////////////////////////////  Table 생성
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PCSGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTTYPE", typeof(string));
                inTable.Columns.Add("MKT_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("ACTQTY", typeof(decimal));
                inLot.Columns.Add("ACTQTY2", typeof(decimal));
                inLot.Columns.Add("VLD_DATE", typeof(string));
                inLot.Columns.Add("PROD_VER_CODE", typeof(string));

                /////////////////////////////////////////////////////////////////
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PCSGID"] = SearchPCSGID();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["PROCID"] = cboProcessRout.SelectedValue;
                newRow["PRODID"] = Util.NVC(popSearchProdID.SelectedValue);
                newRow["LOTTYPE"] = cboLotType.SelectedValue;
                newRow["MKT_TYPE_CODE"] = cboMKType.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

                foreach (DataRow row in dt.Rows)
                {
                    if(string.IsNullOrEmpty(row["LOTID"].ToString()) || row["LOTID"].ToString().Length < 9)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3608"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None);
                        return;
                    }

                    newRow = inLot.NewRow();
                    newRow["LOTID"] = Util.NVC(row["LOTID"]);
                    newRow["ACTQTY"] = Util.NVC(row["WIPQTY"]);
                    newRow["ACTQTY2"] = Util.NVC(row["WIPQTY"]);
                    newRow["VLD_DATE"] = Util.NVC(DateTime.Parse(row["VLD_DATE"].ToString()).ToString("yyyyMMdd")); //DateTime yyyyMMdd 형식으로 변경 , 2023-11-10 장영철
                    newRow["PROD_VER_CODE"] = Util.NVC(row["PROD_VER_CODE"]);
                    inLot.Rows.Add(newRow);
                }
                /////////////////////////////////////////////////////////////////

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_MTRL_FOR_NJ", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // [%1] LOT이 생성 되었습니다.
                        Util.MessageInfo("SFU3044", ObjectDic.Instance.GetObjectName("재공"));

                        // Clear
                        InitializeControls();
                        InitializeGrid();
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

        /// <summary>
        /// 재공생성 이력 조회
        /// </summary>
        private void SearchHistory()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment_HIST) == "" ? null : Util.GetCondition(cboEquipmentSegment_HIST);
                newRow["PROCID"] = null; // Util.GetCondition(cboProcessRout_HIST) == "" ? null : Util.GetCondition(cboProcessRout_HIST);
                newRow["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TODATE"] = Util.GetCondition(dtpDateTo);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_WIPCREATE_HIST", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgResult_HIST, bizResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        private bool ValidationCreate()
        {
            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 라인을 선택하세요.
                Util.MessageValidation("SFU1223");
                return false;
            }

            if (cboProcessRout.SelectedIndex < 0 || cboProcessRout.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // 공정을 선택하세요.
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (popSearchProdID.SelectedValue == null || string.IsNullOrWhiteSpace(popSearchProdID.SelectedValue.ToString()))
            {
                // %1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("제품 ID"));
                return false;
            }

            //if (cboLotType.SelectedIndex < 0 || cboLotType.SelectedValue.ToString().Trim().Equals("SELECT"))
            //{
            //    // %1이 입력되지 않았습니다.
            //    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("LOTTYPE"));
            //    return false;
            //}

            if (cboMKType.SelectedIndex < 0 || cboMKType.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                // %1이 입력되지 않았습니다.
                Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("수출/입 구분"));
                return false;
            }

            // Grid 체크
            DataTable dt = DataTableConverter.Convert(dgResult.ItemsSource);

            if (dt == null || dt.Rows.Count == 0)
            {
                // 데이터가 없습니다.
                Util.MessageValidation("SFU1498");
                return false;
            }

            foreach (DataRow row in dt.Rows)
            {
                if (string.IsNullOrWhiteSpace(Util.NVC(row["LOTID"])))
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("LOTID"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Util.NVC(row["PROD_VER_CODE"])))
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("전극버전"));
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Util.NVC(row["VLD_DATE"])))
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("유효기간"));
                    return false;
                }

                string sWipQty = Util.NVC(row["WIPQTY"]);
                double dWipQty = 0;
                double.TryParse(sWipQty, out dWipQty);

                if (dWipQty == 0)
                {
                    // %1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", ObjectDic.Instance.GetObjectName("수량(Cell)"));
                    return false;
                }

            }

            return true;
        }
        #endregion

        #region [Function]

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

        #endregion


    }

}
