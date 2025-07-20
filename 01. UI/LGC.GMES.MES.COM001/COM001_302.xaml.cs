/*************************************************************************************
 Created Date : 2019.05.13
      Creator : 정문교
   Decription : 공정별 생산실적 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.13  정문교 : Initial Created.   폴란드3동 & 빈강용 조립 공정에서 사용
  2019.09.18  정문교 : 1. "투입" → "공급" 으로 명칭 변경
                       2. "전공정수량" → "공급량" 명칭 변경
                       3. 전공정수량(공급량) 단위 변경 (M / EA) ->노칭 공정
                       4. 불량(미반영) 항목 Notching공정 제외하고 삭제
                       5. 불량/LOSS/물청 불량 항목 중 장비 판단 불량 회색 음영 표시 (ex. Winding, Heeater Time Out, Thickness etc...)
                       6. 잔량 Column 추가  (Lot 조회 화면 및 투입 이력 조회 화면)
 2019.09.25   정문교 : 1.Notching공정일 경우 공급량(M), 공급량(EA)로 표기 및 합계 "전공정 LOSS", "고정LOSS", "잔량" 칼럼의 M를 소숫점 2자리까지 표시
                       2.Carrier ID 칼럼 추가
 2019.10.08   정문교 : 불량/Loss/물품청구 탭 그리드(dgDefect)에 
                       불량 수량 변경 차단 여부 Y인 경우 Background 색상 처리를 주석 처리 함
 2019.10.17   정문교 : 1. NND공정 우측 하단 "불량/LOSS/물품청구" 탭에서 "장비불량수량" 열 숨김처리
                       2. 그리드  "Loss" 명칭을 대문자 "LOSS" 로 변경
                       3. 좌측하단 "Loss" 는 "LOSS (Total)" 명칭으로 변경하여 총 수량을 보여주고 "미확인LOSS"는 바로 밑에 표시
                       4. PKG 공정 조회시 투입수량 다음에 재투입수량 나오도록 칼럼 위치 변경
                       5. NND 불량(미반영) 열 삭제 하고 불량(반영) -> 불량으로 명칭 변경
                       6. 좌측하단 투입이력 탭의 투입 명칭을 공급으로 변경 (투입이력, 투입LOT, 투입시간 -> 공급이력, 공급LOT, 공급시간)
                       7. 좌측하단 미확인LOSS 항목 숨김처리
 2019.10.24   정문교 : 1.사용량 칼럼 변경 : TB_SFC_WIP_INPUT_MTRL_HIST의 INPUT_QTY -> WIPHISTORYATTR의 EQPT_INPUT_QTY로 변경
                       2.칼럼 추가 : 전극 Cut수 WIPHISTORYATTR의 EQPT_END_QTY 칼럼 추가
                                     공급량 WIPHISTORY의 WIPQTY_IN 칼럼 추가
                       3.좌측 하단 공급 이력 칼럼 순서 변경 : 단위, 공급량, 소진량, 전극Cut수, 3Loss, 잔량, 공급시간 순으로 변경
 2019.10.31   정문교 : 칼람 명칭 변경 : 왼쪽 하단 실적확인 탭의 “LOSS(Total)” => “LOSS” 로 변경
                       칼럼 추가      : Notching 공정일 경우 Lot 리스트에서 자공정 LOSS (공급부) 칼람 추가 
 2019.11.05   정문교 : 칼람 추가 : WipAttr 작업방식(재검사,재작업) 칼럼 추가
 2019.11.12   정문교 : 재투입수 칼람위치 작업조 앞으로 변경
                       노칭 공정 타발전, 타발후 칼럼 추가
 2019.11.19   정문교 : 설비불량명, 불량그룹명 좌측 정렬
                       공급이력탭 "Cut 수량" => "전극 Cut 수량" 으로 수정
                       완공시간 칼럼 추가
                       시작유형(IFMODE_ST), 완료유형(IFMODE_ED) 칼럼 추가
 2019.11.26   정문교 : 전공정LOSS 칼럼 제거
                       PKG 공급 Carrier 탭 추가
                       공급 이력 탭에서 전극 Cut 수 칼럼 제거
 2019.12.05   정문교 : 작업타입을 작업유형으로 명칭 변경
                       투입 LOT을 공급 LOT으로 명칭 변경
                       실적확정일시와 장비완료일시 배치 순서 변경
                       모델 탭에서 완성 쪽에 재투입 칼럼 삭제
 2019.12.11   정문교 : REWINDER TAB COUNT 칼럼 설비 앞으로 위치 변경
                       조회 조건에 생산 구분 콤보 추가
 2019.12.18   정문교 : 불량/Loss/물품청구 Tab에 재투입수량 컬럼 추가
                       패키지 생산실적 재투입 정보 Tab에 불량명 컬럼 추가
 2019.12.31   정문교 : 조회 그리드 작업유형 칼럼에 filter 기능 오류 수정
 2020.01.27   정문교 : 재투입 탭 칼럼 순서 변경 As-Is 설비Unit, 위치, 수량, 최근수집일시, 불량명 
                                                To-be 설비Unit, 불량명, 수량, 위치, 최근수집일시
 2020.03.11   김동일   재공이력 정보 조회 기능 추가.
 2020.04.13   정문교   CSR[C20200406-000204] Notching공정일 경우 Vision Count Qty 조회 칼럼 추가.
 2020.05.07   김동일   C20200502-000001 Notching 공정 vision count, tab count 컬럼 합계 중복 계산 오류 수정
 2020.05.14   정문교   CSR[C20200219-000381] 노칭 공정 설비 구간 파단 탭 추가
                       CSR[C20200513-000318] 라미 공정 투입 LOSS 관리 탭 추가
 2020.07.23   정문교   설비수집TAG수 칼럼 추가.
 2020.09.25   신광희   C20200625-000215 - 코팅라인 컬럼 추가
 2021.02.02   신광희   동별 공통코드 항목에 RE_INPUT_DFCT_CODE 등록된 동만 불량/Loss/물품청구 그리드 재투입 수량 및 재투입 정보의 불량 항목 보이도록 수정 함.
 2021.03.15   정문교   LOT 리스트 다국어 처리 오류 수정
 2021.07.15   김지은 : [GM JV Proj.]시험 생산 구분 코드 추가로 인한 수정
 2021.11.04   권상민 : CT검사 공정 코드 추가로 인한 수정
 2022.03.10   정재홍   [C20211203-000387] - STORAGE_PERIOD_TIME 컬럼 추가
 2022.09.01   신광희 : LOT 리스트 데이터 그리드 생산량 컬럼 수정 및 실적 확인 영역 투입TextBlock 생산으로 변경
 2023.03.07   윤지해 : C20230109-000084 완성 생산량, 양품량, 불량량, LOSS량, 물품청구 부분 A5000(노칭)일 경우 M/EA 구분 추가
 2023.03.08   김용군 : ESHM 증설 - A7400 공정 구분정보 'EA 표기, A84000 공정구분정보 음극/양극 표기, Stack별 불량정보, Stack 생산정보 Tab_Page추가
 2023.03.14   윤지해 : C20230109-000084 노칭 공정만 Summary 적용되도록 수정
 2023.03.21   김용군 : ESHM 증설 - 설비불량정보Tab과 동일하게 Stack Machine별 불량그룹, 설비불량명, 불량수량, 최근수집일시 정보로 변경
 2023.05.30   강성묵 : 생산실적 조회 (생산Lot 기준) - AZS 공정 생산실적 조회 (A7400, A8400 추가)
 2023.08.21   강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
 2023.11.24   안유수 : E20231006-001025 패키지공정 특별관리 Tray Group ID - SPCL_LOT_GR_CODE 컬럼 추가
 2024.01.08   남재현 : STK 특별 TRAY 설정 추가에 따른 공통 코드 조회 추가
 2024.02.14   김용군 : E20240221-000898 ESMI1동(A4) 6Line증설관련 화면별 라인ID 콤보정보에 조회될 Line정보와 제외될 Line정보 처리
 2024.06.19   남재현 : E20240619-000957 E-Cuter : 자공정 Loss, 고정 Loss 컬럼 Visible , AZS Stacking : 자공정 Loss 컬럼 Visible.
 2025.03.18   김선영 : 완성LOT 탭 : PKG일때 완성LOTID 보이게 함
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using System.Linq;
using LGC.GMES.MES.CMM001.Popup;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_302 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();
        private BizDataSet _Biz = new BizDataSet();

        bool _bModelLoad = true;
        bool _bLoad = true;

        private const string _assembly = "ASSY";

        DataTable _dtLotListHeader;
        DataTable _dtModelListHeader;
        DataTable _dtLotListColumnVisible;
        DataTable _dtModelListColumnVisible;
        DataTable _dtDefectColumnVisible;
        DataTable _dtSubLotColumnVisible;
        DataTable _dtInputHistColumnVisible;

        DataRow _drSelectRow;

        public COM001_302()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize

        #region # Combo Setting
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };

            //ESMI 1동(A4) 6 Line 만 조회
            //_combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            if (IsCmiExceptLine())
            {                
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "ESMI_A4_FIRST_PRIORITY_LINEID");
            }
            else
            {
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);
            }

            //공정
            C1ComboBox[] cboProcessParent = { cboArea, cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "PROCESSWITHAREA");

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent);

            //경로흐름
            string[] sFilter = { "FLOWTYPE" };
            _combo.SetCombo(cboFlowType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            //생산구분
            sFilter = new string[] { "PRODUCT_DIVISION" };
            _combo.SetCombo(cboProductDiv, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);

            // 생산구분 Default 정상생산
            if (cboProductDiv.Items.Count > 1)
                cboProductDiv.SelectedIndex = 1;

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
        #endregion

        #region # dgLotList의 Header Text, Column Visibility
        private void InitColumnsList()
        {
            #region [ dgLotList의 칼럼 Header ]
            _dtLotListHeader = new DataTable();
            _dtLotListHeader.Columns.Add("COLUMN");
            _dtLotListHeader.Columns.Add("PROCESS");
            _dtLotListHeader.Columns.Add("OBJECTID");
            _dtLotListHeader.Columns.Add("OBJECTNAME");

            //_dtLotListHeader.Rows.Add("PRE_PROC_INPUT_QTY", _assembly, null, ObjectDic.Instance.GetObjectName("공급") + "," + ObjectDic.Instance.GetObjectName("공급량"));
            #endregion

            #region [ dgLotList의 칼럼 Header ]
            _dtModelListHeader = new DataTable();
            _dtModelListHeader.Columns.Add("COLUMN");
            _dtModelListHeader.Columns.Add("PROCESS");
            _dtModelListHeader.Columns.Add("TITLE");

            //_dtModelListHeader.Rows.Add("CURR_PROC_LOSS_QTY", _assembly, ObjectDic.Instance.GetObjectName("공급") + "," + ObjectDic.Instance.GetObjectName("자공정LOSS"));
            //_dtModelListHeader.Rows.Add("CURR_PROC_LOSS_QTY", Process.NOTCHING, ObjectDic.Instance.GetObjectName("공급") + "," + ObjectDic.Instance.GetObjectName("자공정LOSS"));
            #endregion

            #region [ dgLotList의 칼럼 Visibility ]
            _dtLotListColumnVisible = new DataTable();
            _dtLotListColumnVisible.Columns.Add("COLUMN");
            _dtLotListColumnVisible.Columns.Add("PROCESS");
            _dtLotListColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtLotListColumnVisible.Rows.Add("PR_LOTID", Process.NOTCHING + "," + Process.VD_LMN, null);                                                // 투입LOT
            _dtLotListColumnVisible.Rows.Add("EQPT_END_PSTN_ID", Process.NOTCHING, null);                                                               // REWINDER
            _dtLotListColumnVisible.Rows.Add("CSTID", Process.NOTCHING, null);                                                                          // Carrier ID
            _dtLotListColumnVisible.Rows.Add("COATING_LINE", Process.NOTCHING, null);                                                                   // 코팅 라인

            _dtLotListColumnVisible.Rows.Add("DFCT_TAG_QTY", Process.NOTCHING, null);                                                                   // 불량태그수
            _dtLotListColumnVisible.Rows.Add("EQPT_DFCT_TAG_QTY", Process.NOTCHING, null);                                                              // 설비수집TAG수
            _dtLotListColumnVisible.Rows.Add("PRE_PROC_INPUT_QTY", _assembly, null);                                                                    // 공급량
            _dtLotListColumnVisible.Rows.Add("EQPT_INPUT_QTY", _assembly, Process.RWK_LNS);                                                             // 소진량
            // CSR : [C20211203-000387]
            _dtLotListColumnVisible.Rows.Add("STORAGE_PERIOD_TIME", Process.NOTCHING);                                                                  // 창고보관기간
            _dtLotListColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.PACKAGING + "," + Process.CT_INSP, null);                    // 전공정LOSS
            _dtLotListColumnVisible.Rows.Add("FIX_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.AZS_ECUTTER, null);                                        // 고정LOSS
            _dtLotListColumnVisible.Rows.Add("CURR_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.AZS_ECUTTER + "," + Process.STACKING_FOLDING + "," + Process.AZS_STACKING, null); // 자공정LOSS
            _dtLotListColumnVisible.Rows.Add("JAMMING_COUNT", Process.LAMINATION);                                                                      // JAMMING
            _dtLotListColumnVisible.Rows.Add("OCCR_COUNT", Process.LAMINATION);                                                                         // 파단총횟수
            _dtLotListColumnVisible.Rows.Add("RMN_QTY", _assembly, null);                                                                               // 잔량
            _dtLotListColumnVisible.Rows.Add("PRE_PUNCH_CUTOFF_COUNT", Process.NOTCHING, null);                                                         // 타발 전 파단수
            _dtLotListColumnVisible.Rows.Add("AFTER_PUNCH_CUTOFF_COUNT", Process.NOTCHING, null);                                                       // 타발 후 파단수
            _dtLotListColumnVisible.Rows.Add("RE_INPUT_QTY", Process.PACKAGING, null);                                                                  // 재투입
            _dtLotListColumnVisible.Rows.Add("REWINDER_TAB_COUNT", Process.NOTCHING, null);                                                             // Rewinder Tab Count
            _dtLotListColumnVisible.Rows.Add("VISION_COUNT_QTY", Process.NOTCHING, null);                                                               // Vision Count Qty
            _dtLotListColumnVisible.Rows.Add("RWK_TYPE_NAME", Process.RWK_LNS, null);                                                                   // 작업방식
            _dtLotListColumnVisible.Rows.Add("WOID", _assembly, Process.RWK_LNS);                                                                       // W/O
            _dtLotListColumnVisible.Rows.Add("WO_DETL_ID", _assembly, Process.RWK_LNS);                                                                 // W/O상세

            #endregion

            #region [ dgModelList 칼럼 Visibility ]
            _dtModelListColumnVisible = new DataTable();
            _dtModelListColumnVisible.Columns.Add("COLUMN");
            _dtModelListColumnVisible.Columns.Add("PROCESS");
            _dtModelListColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtModelListColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.PACKAGING + "," + Process.CT_INSP, null);             // 전공정 LOSS
            _dtModelListColumnVisible.Rows.Add("FIX_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.AZS_ECUTTER, null);                                         // 고정LOSS
            _dtModelListColumnVisible.Rows.Add("CURR_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.AZS_ECUTTER + "," + Process.STACKING_FOLDING + "," + Process.AZS_STACKING, null);  // 자공정   

            #endregion

            #region [ 불량/Loss/물품청구 칼럼 Visibility ]
            _dtDefectColumnVisible = new DataTable();
            _dtDefectColumnVisible.Columns.Add("COLUMN");
            _dtDefectColumnVisible.Columns.Add("PROCESS");
            _dtDefectColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtDefectColumnVisible.Rows.Add("EQP_DFCT_QTY", _assembly, Process.NOTCHING);                                                            // 장비불량수량
            #endregion

            #region [ dgSubLot 칼럼 Visibility ]
            _dtSubLotColumnVisible = new DataTable();
            _dtSubLotColumnVisible.Columns.Add("COLUMN");
            _dtSubLotColumnVisible.Columns.Add("PROCESS");
            _dtSubLotColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtSubLotColumnVisible.Rows.Add("CSTID", _assembly, Process.NOTCHING);                                                                    // Carrier ID

            // 2024.01.08   남재현 : STK 특별 TRAY 설정 추가
            if (IsCommonCodeUse("STK_BOX_SPCL_MNG_AREA", LoginInfo.CFG_AREA_ID))
            {
                _dtSubLotColumnVisible.Rows.Add("SPECIALYN",Process.STACKING_FOLDING + "," + Process.PACKAGING + "," + Process.CT_INSP, null);                                                                // 특이    
                _dtSubLotColumnVisible.Rows.Add("SPECIALDESC", Process.STACKING_FOLDING + "," + Process.PACKAGING + "," + Process.CT_INSP, null);                                                              // 특이사항
            }
            else
            {
                _dtSubLotColumnVisible.Rows.Add("SPECIALYN", Process.PACKAGING + "," + Process.CT_INSP, null);                                                                    // 특이      
                _dtSubLotColumnVisible.Rows.Add("SPECIALDESC", Process.PACKAGING + "," + Process.CT_INSP, null);                                                                  // 특이사항
            }
            _dtSubLotColumnVisible.Rows.Add("SPCL_RSNCODE", Process.PACKAGING + "," + Process.CT_INSP, null);                                                                    // 사유       
            _dtSubLotColumnVisible.Rows.Add("SPCL_LOT_GR_CODE", Process.PACKAGING, null);                                                                                          // 특별 Tray 그룹 ID
            _dtSubLotColumnVisible.Rows.Add("FORM_MOVE_STAT_CODE_NAME", Process.PACKAGING + "," + Process.CT_INSP, null);                                                     // 상태
            //_dtSubLotColumnVisible.Rows.Add("LOTID", _assembly, Process.PACKAGING + "," + Process.CT_INSP);                                                                   // 완성ID
            _dtSubLotColumnVisible.Rows.Add("LOTID", _assembly, Process.CT_INSP);                                                                                                       // 완성ID(PKG 완성LOTID 보이게 함, 2025.03.18, 김선영)
            _dtSubLotColumnVisible.Rows.Add("PRINT_YN", _assembly, Process.PACKAGING + "," + Process.CT_INSP);                                                                // 발행
            _dtSubLotColumnVisible.Rows.Add("DISPATCH_YN", _assembly, Process.PACKAGING + "," + Process.CT_INSP);                                                             // DISPATCH
            _dtSubLotColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", _assembly, Process.NOTCHING + "," + Process.PACKAGING + "," + Process.CT_INSP);                              // 전공정 LOSS

            #region 2023.08.21  강성묵 : ESHM 증설 - 완성 LOT => Remarks 컬럼 추가
            try
            {
                DataTable dtInTable = new DataTable("RQSTDT");
                dtInTable.Columns.Add("AREAID", typeof(string));
                dtInTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                dtInTable.Columns.Add("COM_CODE", typeof(string));

                DataRow drNewRow = dtInTable.NewRow();
                drNewRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                drNewRow["COM_TYPE_CODE"] = "OUTLOT_REMARK_COL_USE";
                dtInTable.Rows.Add(drNewRow);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", dtInTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    string sProcIdList = "";
                    for(int iIdx = 0; iIdx < dtResult.Rows.Count; iIdx++)
                    {
                        if(string.IsNullOrEmpty(sProcIdList) == false)
                        {
                            sProcIdList += ",";
                        }

                        sProcIdList += dtResult.Rows[iIdx]["COM_CODE"];
                    }

                    if (string.IsNullOrEmpty(sProcIdList) == false)
                    {
                        _dtSubLotColumnVisible.Rows.Add("REMARK", sProcIdList, null);
                    }
                }
            }
            catch
            {
                // NoAction
            }
            #endregion

            #endregion

            #region [ 투입이력 칼럼 Visibility ]
            _dtInputHistColumnVisible = new DataTable();
            _dtInputHistColumnVisible.Columns.Add("COLUMN");
            _dtInputHistColumnVisible.Columns.Add("PROCESS");
            _dtInputHistColumnVisible.Columns.Add("EXCEPTION_PROCESS");

            _dtInputHistColumnVisible.Rows.Add("CSTID", _assembly, Process.NOTCHING);                                                                 // Carrier ID
            //_dtInputHistColumnVisible.Rows.Add("CUT_QTY", Process.LAMINATION, null);                                                                  // Cut 수량
            _dtInputHistColumnVisible.Rows.Add("PRE_PROC_LOSS_QTY", Process.NOTCHING + "," + Process.PACKAGING + "," + Process.CT_INSP, null);                                // 전공정 LOSS
            _dtInputHistColumnVisible.Rows.Add("FIX_LOSS_QTY", Process.NOTCHING + "," + Process.LAMINATION + "," + Process.AZS_ECUTTER, null);                                    // 고정LOSS
            _dtInputHistColumnVisible.Rows.Add("CURR_PROC_LOSS_QTY", Process.LAMINATION + "," + Process.AZS_ECUTTER + "," + Process.STACKING_FOLDING + "," + Process.AZS_STACKING, null);                      // 자공정   

            #endregion

        }
        #endregion

        #region # Tab, TextBox, TextBlock, ComboBox Visibility.Collapsed
        private void InitTabControlVisible()
        {
            InitializeDataGridAttribute();

            // Tab
            cTabHalf.Visibility = Visibility.Collapsed;                       // 완성LOT
            cTabTrayTime.Visibility = Visibility.Collapsed;                   // 시간별생산실적
            CTabReInput.Visibility = Visibility.Collapsed;                    // 재투입
            cTabPkgSupplyCarrier.Visibility = Visibility.Collapsed;           // PKG 공급 Carrier
            cTabEqptCut.Visibility = Visibility.Collapsed;                    // 설비구간파단
            cTabInputLoss.Visibility = Visibility.Collapsed;                  // 투입LOSS관리

            // TextBlock
            tbPreSectionQty.Visibility = Visibility.Collapsed;                // 구간잔량(검사전)
            tbAfterSectionQty.Visibility = Visibility.Collapsed;              // 구간잔량(검사후)
            tbReInputQty.Visibility = Visibility.Collapsed;                   // 재투입수량

            // C1NumericBox
            txtPreSectionQty.Visibility = Visibility.Collapsed;
            txtAfterSectionQty.Visibility = Visibility.Collapsed;
            txtReInputQty.Visibility = Visibility.Collapsed;

            // Button
            btnQualityInfo.Visibility = Visibility.Collapsed;
        }

        private void InitializeDataGridAttribute()
        {

            dgLotList.Columns["INPUT_TAB_COUNT"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["END_TAB_COUNT"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["FIX_LOSS_QTY_VD"].Visibility = Visibility.Collapsed;
            dgLotList.Columns["WRK_TYPE"].Visibility = Visibility.Collapsed;

            dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Visible;
            dgLotList.Columns["IRREGL_PROD_LOT_TYPE_NAME"].Visibility = Visibility.Visible;
            dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Visible;
            dgLotList.Columns["WOID"].Visibility = Visibility.Visible;
            dgLotList.Columns["WO_DETL_ID"].Visibility = Visibility.Visible;
            dgLotList.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["RMN_QTY"].Visibility = Visibility.Visible;
            dgLotList.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Visible;

            //dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { "완성", "생산량", "생산량" };
            //dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { "완성", "양품량", "양품량" };
            //dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { "완성", "불량량", "불량량" };
            //dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { "완성", "LOSS량", "LOSS량" };
            //dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { "완성", "물품청구", "물품청구" };

            if (_bLoad == false)
            {
                dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("생산량"), ObjectDic.Instance.GetObjectName("생산량") };
                dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("양품량"), ObjectDic.Instance.GetObjectName("양품량") };
                dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("불량량"), ObjectDic.Instance.GetObjectName("불량량") };
                dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("LOSS량"), ObjectDic.Instance.GetObjectName("LOSS량") };
                dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("완성"), ObjectDic.Instance.GetObjectName("물품청구"), ObjectDic.Instance.GetObjectName("물품청구") };
            }

            tbWorkOrder.Visibility = Visibility.Visible;
            txtWorkorder.Visibility = Visibility.Visible;

            // 김용군 AZS_STACKING 공정의 경우 Stacking불량정보TAB, Stacking생산정보TAB 활성화
            if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING)
            {
                cTabStackDefect.Visibility = Visibility.Visible;
                cTabStackInputEndQty.Visibility = Visibility.Visible;
            }
            else
            {
                cTabStackDefect.Visibility = Visibility.Collapsed;
                cTabStackInputEndQty.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region # Control Clear
        private void InitUsrControl()
        {
            txtSelectLot.Text = string.Empty;
            txtWorkorder.Text = string.Empty;
            txtWorkorderDetail.Text = string.Empty;
            txtWorkdate.Text = string.Empty;
            txtShift.Text = string.Empty;
            txtShift.Tag = string.Empty;
            txtStartTime.Text = string.Empty;
            txtWorker.Text = string.Empty;
            txtEndTime.Text = string.Empty;

            txtInputQty.Value = 0;
            txtOutQty.Value = 0;
            txtPreSectionQty.Value = 0;
            txtAfterSectionQty.Value = 0;
            txtDefectQty.Value = 0;
            txtLossQty.Value = 0;
            txtPrdtReqQty.Value = 0;
            //txtCurrProcLossQty.Value = 0;
            txtReInputQty.Value = 0;
            txtUnidentifiedQty.Value = 0;

            txtIrreglProdLotTyep.Text = string.Empty;
            txtNote.Text = string.Empty;

            btnQualityInfo.Visibility = Visibility.Collapsed;

            Util.gridClear(dgDefect);                 // 불량/Loss/물품청구
            Util.gridClear(dgEqpFaulty);              // 설비불량정보
            Util.gridClear(dgOutPkgSupply);           // PKG 공급 Carrier
            Util.gridClear(dgSubLot);                 // 완성LOT
            Util.gridClear(dgReInput);                // 재투입 정보
            Util.gridClear(dgTrayInfo);               // 시간별생산실적
            Util.gridClear(dgInputHist);              // 투입이력
            Util.gridClear(dgEqptCut);                // 설비구간파단
            Util.gridClear(dgInputLoss);              // 투입LSSS관리

            // 김용군
            Util.gridClear(dgStackDefect);            // Stacking별 불량정보
            Util.gridClear(dgStackInputEndQty);       // Stacking별 생산정보

            _drSelectRow = null;
        }

        private void InitGridControl()
        {
            Util.gridClear(dgLotList);
            Util.gridClear(dgModelList);

            // 김용군
            //if (cboProcess.SelectedValue != null && (cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS))
            if (cboProcess.SelectedValue != null && (cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS || cboProcess.SelectedValue.ToString() == Process.AZS_ECUTTER))
            {
                dgLotList.AlternatingRowBackground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF9F9F9"));
                dgLotList.Rows[dgLotList.Rows.Count - 1].Visibility = Visibility.Collapsed;
            }
            else
            {
                dgLotList.AlternatingRowBackground = null;
                dgLotList.Rows[dgLotList.Rows.Count - 1].Visibility = Visibility.Visible;
            }

            DataGridAggregate.SetAggregateFunctions(dgLotList.Columns["PRDT_CLSS_CODE"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });

            for (int col = dgLotList.Columns["PRE_PROC_INPUT_QTY"].Index; col < dgLotList.Columns["SHFT_NAME"].Index; col++)
            {
                DataGridAggregate.SetAggregateFunctions(dgLotList.Columns[dgLotList.Columns[col].Name], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = string.Empty } });
            }
        }

        private void SetControl()
        {
            dgLotList.TopRows[0].Height = new C1.WPF.DataGrid.DataGridLength(35);
            dgLotList.TopRows[1].Height = new C1.WPF.DataGrid.DataGridLength(35);

            cboArea.SelectedItemChanged += cboArea_SelectedItemChanged;
            cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;

            //dgDefect.AlternatingRowBackground = null;
            dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
        }

        #endregion

        #endregion

        #region Event

        #region # Form Load
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            InitCombo();
            InitColumnsList();
            SetControl();
            SetAreaGridVisibility();
            SetSearchControl();

            GetCaldate();
            SetControlVisibility(cboProcess.SelectedValue.ToString());
            SetShopGridVisibility();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region # 조회
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSearch())
                return;

            GetLotList();
        }
        #endregion

        #region # 탭 선택 변경 : Lot, 모델
        private void tbcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("cTabLot"))
                chkModel.IsChecked = false;
            else
                chkModel.IsChecked = true;

        }
        #endregion

        #region # 조회조건 : 동 변경
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboArea.Items.Count > 0 && cboArea.SelectedValue != null && !cboArea.SelectedValue.Equals("SELECT"))
            {
                SetAreaGridVisibility();
            }
        }
        #endregion

        #region # 조회조건 : 라인 변경
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            //{
            //    SetProcess();
            //}
        }
        #endregion

        #region # 조회조건 : 공정 변경
        private void CboProcess_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboProcess.Items.Count > 0 && cboProcess.SelectedValue != null && !cboProcess.SelectedValue.Equals("SELECT"))
            {
                //SetEquipment();
                InitUsrControl();
                InitGridControl();
                SetAreaGridVisibility();
                SetSearchControl();

                SetControlVisibility(cboProcess.SelectedValue.ToString());
                SetShopGridVisibility();

                SetCoatingLineColumnVisibility();
            }
            else
            {
                dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Collapsed;
                dgInputHist.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region # 조회조건 : 작업일 변경
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtpDateFrom.SelectedDateTime.Year > 1 && dtpDateTo.SelectedDateTime.Year > 1)
            {
                LGCDatePicker LGCdp = sender as LGCDatePicker;

                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays < 0)
                {
                    dtpDateFrom.SelectedDateTime = dtpDateTo.SelectedDateTime;
                    return;
                }
            }
        }
        #endregion

        #region # 조회조건 : Lot KeyDown
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region # 조회조건 : 모델 KeyDown
        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {
            // 모델 AutoComplete
            if (_bModelLoad)
            {
                GetModel();
                _bModelLoad = false;
            }
        }
        #endregion

        #region # 조회조건 : 프로젝트 KeyDown
        private void txtPrjtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetLotList();
            }
        }
        #endregion

        #region # 모델 Check
        private void chkModel_Checked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 1;
            SetSearchControl();
        }

        private void chkModel_Unchecked(object sender, RoutedEventArgs e)
        {
            tbcList.SelectedIndex = 0;
            SetSearchControl();
        }
        #endregion

        #region # 생산 Lot 선택
        private void dgProductLotChoice_Checked(object sender, RoutedEventArgs e)
        {
            dgLotList.Selection.Clear();

            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            //if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0) || DataTableConverter.GetValue(rb.DataContext, "CHK").ToString() == "0") // 2024.11.12. 김영국 - DataType의 값이 long으로 들어와서 String 비교함.
            {
                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                InitUsrControl();

                // 선택 Row 
                _drSelectRow = _util.GetDataGridFirstRowBycheck(dgLotList, "CHK");

                // 실적확정 탭 조회
                SetValue();

                // 불량/Loss/물품청구 조회
                GetDefectInfo();

                // 설비불량정보 탭 조회
                GetEqpFaultyData();

                // PKG 공급 Carrier 조회
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.STACKING_FOLDING))
                {
                    GetPkgSupplyProduct();
                }

                // 완성LOT 탭 조회
                if (!Util.NVC(_drSelectRow["PROCID"]).Equals(Process.NOTCHING))
                {
                    GetSubLot();
                }

                // 재투입 정보탭, 시간별생산실적 탭 조회
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.PACKAGING))
                {
                    GetReInputInfo();
                    GetTrayLotByTime();
                }

                // 투입이력 탭 조회
                GetInputHistory();

                // 설비 구간 파단 조회  
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.NOTCHING))
                {
                    GetEqptSection(dgEqptCut);
                }

                // 투입LOSS 조회
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.LAMINATION))
                {
                    GetEqptSection(dgInputLoss);
                }

                // 김용군 Stack별 불량정보& Stack별 생산정보 조회
                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.AZS_STACKING))
                {
                    GetStackDefect();
                    GetStackInputEndInfo();
                }

                SetControlVisibility(cboProcess.SelectedValue.ToString());
                SetShopGridVisibility();
            }
        }
        #endregion

        /// <summary>
        /// Lot List Merge
        /// </summary>
        private void dgLotList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            // 김용군
            //if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS) return;
            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS || cboProcess.SelectedValue.ToString() == Process.AZS_ECUTTER) return;

            C1DataGrid dg = sender as C1DataGrid;

            int rowCount = 0;
            for (int row = 0; row < (dg.Rows.Count - dg.TopRows.Count) - 1; row++)
            {
                rowCount++;

                if (rowCount % 2 != 0)
                {
                    for (int col = 0; col < dg.Columns.Count; col++)
                    {
                        if(cboProcess.SelectedValue.ToString() == Process.NOTCHING)
                        {
                            #region 2023.03.07   윤지해 : C20230109-000084 완성 생산량, 양품량, 불량량, LOSS량, 물품청구 부분 A5000(노칭)일 경우 M/EA 구분 추가
                            // 2023.03.07 C20230109-000084 INPUT_QTY, WIPQTY_ED, CNFM_DFCT_QTY, CNFM_LOSS_QTY, CNFM_PRDT_REQ_QTY 추가
                            // 2023.03.14 C20230109-000084 노칭공정만 적용
                            if (
                                dg.Columns[col].Name.Equals("PRDT_CLSS_CODE") ||
                                dg.Columns[col].Name.Equals("PRE_PROC_INPUT_QTY") ||
                                dg.Columns[col].Name.Equals("EQPT_INPUT_QTY") ||
                                dg.Columns[col].Name.Equals("PRE_PROC_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("FIX_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("CURR_PROC_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("RMN_QTY") ||
                                dg.Columns[col].Name.Equals("INPUT_QTY") ||
                                dg.Columns[col].Name.Equals("WIPQTY_ED") ||
                                dg.Columns[col].Name.Equals("CNFM_DFCT_QTY") ||
                                dg.Columns[col].Name.Equals("CNFM_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("CNFM_PRDT_REQ_QTY"))
                            #endregion
                            {
                                // 공급, 소진량, 전공정, 고정, 자공정, 잔량
                                continue;
                            }
                            else
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(row + dg.TopRows.Count, col), dg.GetCell(row + dg.TopRows.Count + 1, col)));
                            }
                        }
                        else
                        {
                            if (
                                dg.Columns[col].Name.Equals("PRDT_CLSS_CODE") ||
                                dg.Columns[col].Name.Equals("PRE_PROC_INPUT_QTY") ||
                                dg.Columns[col].Name.Equals("EQPT_INPUT_QTY") ||
                                dg.Columns[col].Name.Equals("PRE_PROC_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("FIX_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("CURR_PROC_LOSS_QTY") ||
                                dg.Columns[col].Name.Equals("RMN_QTY"))
                            {
                                // 공급, 소진량, 전공정, 고정, 자공정, 잔량
                                continue;
                            }
                            else
                            {
                                e.Merge(new DataGridCellsRange(dg.GetCell(row + dg.TopRows.Count, col), dg.GetCell(row + dg.TopRows.Count + 1, col)));
                            }

                        }
                    }
                }

            }
        }

        /// <summary>
        /// Lot List LoadedCellPresenter(
        /// </summary>
        private void dgLotList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 칼럼 Width 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name == "WRK_USER_NAME")
                    {
                        e.Cell.Column.Width = new C1.WPF.DataGrid.DataGridLength(100);
                    }
                }

                if (e.Cell.Row.Type == DataGridRowType.Bottom)
                {
                    StackPanel panel = e.Cell.Presenter.Content as StackPanel;

                    if (panel == null && panel.Children == null && panel.Children.Count < 1) return;

                    ContentPresenter presenter = panel.Children[0] as ContentPresenter;

                    if (e.Cell.Column.Index == dg.Columns["PRDT_CLSS_CODE"].Index)
                    {
                        if (e.Cell.Row.Index == (dg.Rows.Count - 2))
                        {
                            if (cboProcess.SelectedValue == null)
                                presenter.Content = string.Empty;
                            else if (cboProcess.SelectedValue.ToString() == Process.NOTCHING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("M");
                            else if (cboProcess.SelectedValue.ToString() == Process.LAMINATION)
                                presenter.Content = ObjectDic.Instance.GetObjectName("음극");
                            // 김용군 AZS_STACKING 구분 - 음극 표시
                            else if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("음극");
                            else if (cboProcess.SelectedValue.ToString() == Process.STACKING_FOLDING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("Mono/A-Type");
                            else
                                presenter.Content = ObjectDic.Instance.GetObjectName("EA");
                        }
                        else if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                        {
                            if (cboProcess.SelectedValue == null)
                                presenter.Content = string.Empty;
                            else if (cboProcess.SelectedValue.ToString() == Process.NOTCHING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("EA");
                            else if (cboProcess.SelectedValue.ToString() == Process.LAMINATION)
                                presenter.Content = ObjectDic.Instance.GetObjectName("양극");
                            // 김용군 AZS_STACKING 구분 - 양극 표시
                            else if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("양극");
                            else if (cboProcess.SelectedValue.ToString() == Process.STACKING_FOLDING)
                                presenter.Content = ObjectDic.Instance.GetObjectName("Half/C-Type");
                        }
                    }
                    else if (e.Cell.Column.Index >= dg.Columns["PRE_PROC_INPUT_QTY"].Index && e.Cell.Column.Index <= dg.Columns["VISION_COUNT_QTY"].Index || e.Cell.Column.Index == dg.Columns["DFCT_TAG_QTY"].Index)
                    {
                        // 김용군
                        //if (cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS) return;
                        if (cboProcess.SelectedValue.ToString() == Process.PACKAGING || cboProcess.SelectedValue.ToString() == Process.CT_INSP || cboProcess.SelectedValue.ToString() == Process.VD_LMN || cboProcess.SelectedValue.ToString() == Process.RWK_LNS || cboProcess.SelectedValue.ToString() == Process.AZS_ECUTTER) return;

                        if (dg.GetRowCount() > 0)
                        {
                            if (e.Cell.Row.Index == (dg.Rows.Count - 2))
                            {
                                if (e.Cell.Column.Name == "PRE_PUNCH_CUTOFF_COUNT" || e.Cell.Column.Name == "AFTER_PUNCH_CUTOFF_COUNT" || e.Cell.Column.Name == "JAMMING_COUNT" || e.Cell.Column.Name == "OCCR_COUNT" || e.Cell.Column.Name == "DFCT_TAG_QTY")
                                    presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnInt(e.Cell.Column.Name, 1));
                                else
                                    presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnDecimal(e.Cell.Column.Name, 1));
                            }
                            else if (e.Cell.Row.Index == (dg.Rows.Count - 1))
                            {
                                if (e.Cell.Column.Name == "PRE_PUNCH_CUTOFF_COUNT" || e.Cell.Column.Name == "AFTER_PUNCH_CUTOFF_COUNT" || e.Cell.Column.Name == "JAMMING_COUNT" || e.Cell.Column.Name == "OCCR_COUNT" || e.Cell.Column.Name == "DFCT_TAG_QTY")
                                    presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnInt(e.Cell.Column.Name, 2));
                                else
                                    presenter.Content = SetGridFormatBottomRow(e.Cell.Column.Name, SumGridColumnDecimal(e.Cell.Column.Name, 2));
                            }
                        }

                    }
                }

            }));
        }

        #region # 공정이 PACKAGING인 경우  품질정보조회 버튼 활성화 (팝업)
        private void btnQualityInfo_Click(object sender, RoutedEventArgs e)
        {
            popupQuality();
        }
        #endregion

        #region # 불량/Loss/물품청구 탭
        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    //if (!e.Cell.Column.Name.Equals("ACTNAME"))
                    //{
                    //    //if (Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "RSLT_EXCL_FLAG")).Equals("Y"))
                    //    //{
                    //    //    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
                    //    //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //    //}
                    //    //else
                    //    //{
                    //        string sFlag = Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "DFCT_QTY_CHG_BLOCK_FLAG"));
                    //        //if (sFlag == "Y" || Util.NVC(DataTableConverter.GetValue(dgDefect.Rows[e.Cell.Row.Index].DataItem, "PRCS_ITEM_CODE")).Equals("UNIDENTIFIED_QTY"))
                    //        if (sFlag == "Y")
                    //        {
                    //            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                    //            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //        }
                    //        else
                    //        {
                    //            e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                    //            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    //        }
                    //    //}
                    //}
                }
            }));
        }

        private void dgDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }
        #endregion

        #region # 설비불량정보 탭
        private void dgEqpFaulty_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    //Grid Data Binding 이용한 Background 색 변경
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") &&
                            Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        private void dgEqpFaulty_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgEqpFaulty_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupEqptDfctCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgEqpFaulty_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgEqpFaulty.TopRows.Count; i < dgEqpFaulty.Rows.Count; i++)
                {
                    if (dgEqpFaulty.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") ||
                           Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgEqpFaulty.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgEqpFaulty.GetCell(idxS, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgEqpFaulty.GetCell(idxE, dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성LOT 탭
        private void dgSubLot_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if ((cboProcess.SelectedValue.Equals(Process.CT_INSP) || cboProcess.SelectedValue.Equals(Process.PACKAGING)) && e.Cell.Column.Name == "CSTID")
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));

        }
        private void dgSubLot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CSTID")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupAssyCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "LOTID")),
                                          Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "CSTID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            String sBoxID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));

            if (!sBoxID.Equals(""))
            {
                // 발행..
                DataTable dtRslt = GetThermalPaperPrintingInfo(sBoxID);

                if (dtRslt == null || dtRslt.Rows.Count < 1)
                    return;

                List<Dictionary<string, string>> dicList = new List<Dictionary<string, string>>();
                Dictionary<string, string> dicParam = new Dictionary<string, string>();

                if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.LAMINATION))
                {
                    dicParam.Add("reportName", "Lami");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("CASTID", Util.NVC(dtRslt.Rows[0]["CSTID"])); // 카세트 ID 컬럼은??
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("CELLTYPE", Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]));
                    dicParam.Add("TITLEX", "MAGAZINE ID");
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    CMM_THERMAL_PRINT_LAMI printlami = new CMM_THERMAL_PRINT_LAMI();
                    printlami.FrameOperation = FrameOperation;

                    if (printlami != null)
                    {
                        object[] Parameters = new object[7];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.LAMINATION;
                        Parameters[2] = Util.NVC(_drSelectRow["EQSGID"]);
                        Parameters[3] = Util.NVC(_drSelectRow["EQPTID"]);
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.
                        Parameters[6] = "MAGAZINE";   // 발행 Type M:Magazine, B:Folded Box, R:Remain Pancake, N:매거진재구성(Folding공정)

                        C1WindowExtension.SetParameters(printlami, Parameters);
                        printlami.Show();
                    }

                }
                else if (Util.NVC(_drSelectRow["PROCID"]).Equals(Process.STACKING_FOLDING))
                {
                    int iCopys = 2;

                    if (LoginInfo.CFG_THERMAL_COPIES > 0)
                    {
                        iCopys = LoginInfo.CFG_THERMAL_COPIES;
                    }

                    dicParam.Add("reportName", "Fold");
                    dicParam.Add("LOTID", Util.NVC(dtRslt.Rows[0]["LOTID_RT"]));
                    dicParam.Add("QTY", Convert.ToDouble(Util.NVC(dtRslt.Rows[0]["WIPQTY"])).ToString());
                    dicParam.Add("MAGID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("MAGIDBARCODE", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("LARGELOT", Util.NVC(dtRslt.Rows[0]["CAL_DATE"]));  // 폴딩 LOT의 생성시간(공장시간기준)
                    dicParam.Add("MODEL", Util.NVC(dtRslt.Rows[0]["MODLID"]));
                    dicParam.Add("REGDATE", Util.NVC(dtRslt.Rows[0]["LOTDTTM_CR"]));
                    dicParam.Add("EQPTNO", Util.NVC(dtRslt.Rows[0]["EQPTSHORTNAME"]));
                    dicParam.Add("TITLEX", "BASKET ID");
                    dicParam.Add("PRINTQTY", iCopys.ToString());  // 발행 수
                    dicParam.Add("B_LOTID", Util.NVC(dtRslt.Rows[0]["LOTID"]));
                    dicParam.Add("B_WIPSEQ", Util.NVC(dtRslt.Rows[0]["WIPSEQ"]));
                    dicParam.Add("RE_PRT_YN", "Y"); // 재발행 여부.
                    dicList.Add(dicParam);

                    LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD printfold = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_FOLD(dicParam);
                    printfold.FrameOperation = FrameOperation;

                    if (printfold != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = dicList;
                        Parameters[1] = Process.STACKING_FOLDING;
                        Parameters[2] = Util.NVC(_drSelectRow["EQSGID"]);
                        Parameters[3] = Util.NVC(_drSelectRow["EQPTID"]);
                        Parameters[4] = "Y";   // 완료 메시지 표시 여부.
                        Parameters[5] = "Y";   // 디스패치 처리.

                        C1WindowExtension.SetParameters(printfold, Parameters);
                        printfold.ShowModal();
                    }

                }

            }
        }
        #endregion

        #endregion

        #region Mehod

        #region # 실적 일자 조회
        public void GetCaldate()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CALDATE", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    dtpDateFrom.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
                    dtpDateTo.SelectedDateTime = Convert.ToDateTime(dtRslt.Rows[0]["CALDATE"].ToString());
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
        }
        #endregion

        #region # 공정 콤보
        private void SetProcess()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_BY_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.DisplayMemberPath = "CBO_NAME";
                cboProcess.SelectedValuePath = "CBO_CODE";

                //DataRow drIns = dtResult.NewRow();
                //drIns["CBO_NAME"] = "-SELECT-";
                //drIns["CBO_CODE"] = "SELECT";
                //dtResult.Rows.InsertAt(drIns, 0);

                cboProcess.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_PROC_ID.Equals(""))
                {
                    cboProcess.SelectedValue = LoginInfo.CFG_PROC_ID;

                    if (cboProcess.SelectedIndex < 0)
                        cboProcess.SelectedIndex = 0;
                }
                else
                {
                    if (cboProcess.Items.Count > 0)
                        cboProcess.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비 콤보
        private void SetEquipment()
        {
            try
            {
                // 동을 선택하세요.
                string sArea = Util.GetCondition(cboArea);
                if (string.IsNullOrWhiteSpace(sArea))
                    return;

                string sProc = Util.GetCondition(cboProcess);
                if (string.IsNullOrWhiteSpace(sProc))
                {
                    cboEquipment.ItemsSource = null;
                    return;
                }

                string sEquipmentSegment = Util.GetCondition(cboEquipmentSegment);

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;
                dr["EQSGID"] = string.IsNullOrWhiteSpace(sEquipmentSegment) ? null : sEquipmentSegment;
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_AREA_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-ALL-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();

                if (!LoginInfo.CFG_EQPT_ID.Equals(""))
                {
                    cboEquipment.SelectedValue = LoginInfo.CFG_EQPT_ID;

                    if (cboEquipment.SelectedIndex < 0)
                        cboEquipment.SelectedIndex = 0;
                }
                else
                {
                    cboEquipment.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 조회 조건 모델 조회 : AutoCompleteTextBox 
        private void GetModel()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("PRODID", typeof(string));

                DataRow newRow = inTable.NewRow();

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_MODEL", "INDATA", "OUTDATA", inTable);

                foreach (DataRow r in dtRslt.Rows)
                {
                    string displayString = r["MODLID"].ToString(); //표시 텍스트
                    string keywordString;

                    keywordString = displayString;

                    txtModlId.AddItem(new CMM001.AutoCompleteEntry(displayString, keywordString)); //표시 텍스트와 검색어 텍스트(배열)를 AutoCompleteTextBox의 Item에 추가한다.
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
        }
        #endregion

        #region # 생산 Lot 리스트 조회
        /// <summary>
        /// 생산 Lot 리스트 조회
        /// </summary>
        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));
                dtRqst.Columns.Add("FLOWTYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("RUNYN", typeof(string));
                //dtRqst.Columns.Add("NORMAL", typeof(string));
                //dtRqst.Columns.Add("PILOT", typeof(string));
                dtRqst.Columns.Add("PILOT_PROD_DIVS_CODE", typeof(string)); // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
                dr["EQSGID"] = string.IsNullOrWhiteSpace(cboEquipmentSegment.SelectedValue.ToString()) ? null : cboEquipmentSegment.SelectedValue.ToString();
                dr["PROCID"] = cboProcess.SelectedValue.ToString();
                dr["EQPTID"] = string.IsNullOrWhiteSpace(cboEquipment.SelectedValue.ToString()) ? null : cboEquipment.SelectedValue.ToString();
                dr["FROM_DATE"] = Util.GetCondition(dtpDateFrom);
                dr["TO_DATE"] = Util.GetCondition(dtpDateTo);
                dr["FLOWTYPE"] = string.IsNullOrWhiteSpace(cboFlowType.SelectedValue.ToString()) ? null : cboFlowType.SelectedValue.ToString();

                if (!string.IsNullOrWhiteSpace(txtPRLOTID.Text))
                    dr["PR_LOTID"] = txtPRLOTID.Text;
                else if (!string.IsNullOrWhiteSpace(txtLotId.Text))
                    dr["LOTID"] = txtLotId.Text;
                else if (!string.IsNullOrWhiteSpace(txtModlId.Text))
                    dr["MODLID"] = txtModlId.Text;
                else if (!string.IsNullOrWhiteSpace(txtProdId.Text))
                    dr["PRODID"] = txtProdId.Text;
                else if (!string.IsNullOrWhiteSpace(txtPrjtName.Text))
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                if (chkProc.IsChecked == false)
                    dr["RUNYN"] = "Y";

                // 2021.07.15 : 시험 생산 구분 코드 추가로 인한 수정
                //if (cboProductDiv.SelectedValue.ToString() == "P")
                //    dr["NORMAL"] = cboProductDiv.SelectedValue.ToString();
                //else if (cboProductDiv.SelectedValue.ToString() == "X")
                //    dr["PILOT"] = cboProductDiv.SelectedValue.ToString();
                dr["PILOT_PROD_DIVS_CODE"] = Util.GetCondition(cboProductDiv, bAllNull: true);

                // 극성
                if (cboElecType.IsEnabled == true && ((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("cTabLot"))
                {
                    dtRqst.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                    dr["PRDT_CLSS_CODE"] = cboElecType.SelectedValue.ToString() == "" ? null : cboElecType.SelectedValue.ToString();
                }

                dtRqst.Rows.Add(dr);

                string bizRuleName = string.Empty;

                if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("cTabLot"))
                {
                    // 김용군
                    //bizRuleName = "DA_PRD_SEL_LOT_LIST_L";
                    //InitUsrControl();
                    // 2023.05.30   강성묵 : 생산실적 조회 (생산Lot 기준) - AZS 공정 생산실적 조회 (A7400, A8400 추가) BIZ 통합으로 AZS 특화 DA 주석
                    //if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING || cboProcess.SelectedValue.ToString() == Process.AZS_ECUTTER)
                    //{
                    //    bizRuleName = "DA_PRD_SEL_AZS_STACKING_LOT_LIST_L";
                    //    InitUsrControl();
                    //}
                    //else
                    //{
                        bizRuleName = "DA_PRD_SEL_LOT_LIST_L";
                        InitUsrControl();
                    //}
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_LOT_LIST_MODEL_L";
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    // Clear
                    //InitUsrControl();

                    if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("cTabLot"))
                    {
                        dgLotList.MergingCells -= dgLotList_MergingCells;

                        Util.GridSetData(dgLotList, bizResult, FrameOperation, true);

                        for (int col = dgLotList.Columns["PRDT_CLSS_CODE"].Index; col < dgLotList.Columns["EQPTNAME"].Index; col++)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgLotList.Columns[dgLotList.Columns[col].Name], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});
                        }

                        int col2 = dgLotList.Columns["DFCT_TAG_QTY"].Index;
                        DataGridAggregate.SetAggregateFunctions(dgLotList.Columns[dgLotList.Columns[col2].Name], new DataGridAggregatesCollection {
                                            new DataGridAggregateSum { ResultTemplate = this.Resources["ResultTemplate"] as DataTemplate  }});


                        dgLotList.MergingCells += dgLotList_MergingCells;
                    }
                    else
                    {
                        Util.GridSetData(dgModelList, bizResult, FrameOperation, true);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 불량/Loss/물품청구 탭 조회
        /// <summary>
        /// 불량/Loss/물품청구 탭 조회
        /// </summary>
        private void GetDefectInfo()
        {
            try
            {
                // 김용군 AZS Staking설비불량 별도 DA호출
                string BizNAme = string.Empty;
                // BizNAme = "DA_QCA_SEL_WIPRESONCOLLECT_L";

                if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING)
                {
                    // 김용군 AZS Staking설비불량 Main설비 불량 집계하게 Biz수정하여 기존 DA호출하는거로 변경
                    //BizNAme = "DA_QCA_SEL_WIPRESONCOLLECT_AZS_L";
                    BizNAme = "DA_QCA_SEL_WIPRESONCOLLECT_L";
                }
                else
                {
                    BizNAme = "DA_QCA_SEL_WIPRESONCOLLECT_L";
                }

                DataTable inTable = _Biz.GetDA_QCA_SEL_WIPRESONCOLLECT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = Util.NVC(_drSelectRow["AREAID"]);
                newRow["PROCID"] = Util.NVC(_drSelectRow["PROCID"]);
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                newRow["ACTID"] = "DEFECT_LOT,LOSS_LOT,CHARGE_PROD_LOT";
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService(BizNAme, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgDefect, searchResult, null);

                    SumDefectQty();
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 설비불량정보 탭 조회
        private void GetEqpFaultyData() //string sLot, string sWipSeq)
        {
            try
            {
                DataTable inTable = _Biz.GetDA_EQP_SEL_EQPTDFCT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                // 김용군 AZS Staking설비불량 별도 DA호출
                string bizName = string.Empty;

                if (cboProcess.SelectedValue.ToString() == Process.AZS_STACKING)
                {
                    // 김용군 AZS Staking설비불량 Main설비 불량 집계하게 Biz수정하여 기존 DA호출하는거로 변경
                    //bizName = "DA_EQP_SEL_EQPTDFCTSTACKING_INFO_L";
                    bizName = "DA_EQP_SEL_EQPTDFCT_INFO_L";
                }
                else
                {
                    bizName = "DA_EQP_SEL_EQPTDFCT_INFO_L";
                }

                //new ClientProxy().ExecuteService("DA_EQP_SEL_EQPTDFCT_INFO_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                new ClientProxy().ExecuteService(bizName, "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgEqpFaulty, searchResult, null);

                    dgEqpFaulty.MergingCells -= dgEqpFaulty_MergingCells;
                    dgEqpFaulty.MergingCells += dgEqpFaulty_MergingCells;

                    if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                    else
                        dgEqpFaulty.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # PKG 공급 Carrier 탭 조회
        private void GetPkgSupplyProduct()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_OUT_LOT_LIST_UNLOADER_FD_L", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgOutPkgSupply, searchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private bool CheckUnldrShrFlag()
        {
            try
            {
                if (_drSelectRow == null) return false;

                bool bRet = false;

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow dr = IndataTable.NewRow();
                dr["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);

                IndataTable.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTATTR", "INDATA", "OUTDATA", IndataTable);

                if (dtRslt?.Rows?.Count > 0)
                {
                    if (Util.NVC(dtRslt.Rows[0]["UNLDR_SHR_FLAG"]).Equals("Y"))
                    {
                        cTabPkgSupplyCarrier.Visibility = Visibility.Visible;
                        bRet = true;
                    }
                    else
                        cTabPkgSupplyCarrier.Visibility = Visibility.Collapsed;
                }
                else
                    cTabPkgSupplyCarrier.Visibility = Visibility.Collapsed;

                return bRet;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool CheckReInputDefectCode()
        {
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
            inTable.Columns.Add("COM_CODE", typeof(string));

            DataRow newRow = inTable.NewRow();
            newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            newRow["COM_TYPE_CODE"] = "RE_INPUT_DFCT_CODE";
            inTable.Rows.Add(newRow);
            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_FOR_AREA", "RQSTDT", "RSLTDT", inTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        #endregion

        #region # 완성LOT 탭 조회
        private void GetSubLot()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EDIT_SUBLOT_LIST_SM_L", "INDATA", "OUTDATA", dtRqst, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgSubLot, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 투입이력 탭 조회
        private void GetInputHistory()
        {
            try
            {
                if (!cTabInputHalf.Visibility.Equals(Visibility.Visible)) return;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("PROD_WIPSEQ", typeof(int));
                inTable.Columns.Add("INPUT_LOTID", typeof(string));
                inTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                newRow["PROD_WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                inTable.Rows.Add(newRow);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUT_MTRL_HIST_END_L", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgInputHist, searchResult, null, true);

                    if (dgInputHist.CurrentCell != null)
                        dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.CurrentCell.Row.Index, dgInputHist.Columns.Count - 1);
                    else if (dgInputHist.Rows.Count > 0)
                        dgInputHist.CurrentCell = dgInputHist.GetCell(dgInputHist.Rows.Count, dgInputHist.Columns.Count - 1);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 재투입 정보 탭 조회
        private void GetReInputInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                newRow["PROD_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_BAS_SEL_LOT_EQPT_RE_INPUT_HIST", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgReInput, searchResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 시간별생산실적 탭 조회
        private void GetTrayLotByTime()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PR_LOTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PR_LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_EDIT_SUBLOT_QTY_BY_TIME", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dgTrayInfo, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region # 완성 LOT 탭의 재발행
        private DataTable GetThermalPaperPrintingInfo(string sLotID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_THERMAL_PAPER_PRT_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = Util.NVC(_drSelectRow["AREAID"]);
                newRow["LOTID"] = sLotID;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_THERMAL_PAPER_PRT_INFO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetLabelPrtHist(string sZPL, DataRow drPrtInfo, string sLot, string sWipseq)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_LABEL_HIST();

                DataRow newRow = inTable.NewRow();
                //newRow["LABEL_CODE"] = "LBL0001";
                //newRow["LABEL_ZPL_CNTT"] = sZPL;
                newRow["LABEL_PRT_COUNT"] = "2";
                newRow["PRT_ITEM01"] = sLot;
                newRow["PRT_ITEM02"] = sWipseq;
                newRow["INSUSER"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region # 시간별생산실적 탭 조회
        private void GetEqptSection(C1DataGrid dg)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LANGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                dr["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                dr["LANGID"] = LoginInfo.LANGID;
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_EQPT_SECTION_PRD_RSLT", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    Util.GridSetData(dg, searchResult, null, true);
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region Function

        #region [Validation]
        private bool ValidationSearch(bool isLot = false)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                // 기간은 {0}일 이내 입니다.
                Util.MessageValidation("SFU2042", "31");
                return false;
            }

            if (cboProcess.SelectedValue == null || cboProcess.SelectedValue.ToString().Equals("SELECT"))
            {
                // 공정을선택하세요
                Util.MessageValidation("SFU1459");
                return false;
            }

            if (!isLot)
            {
                if (cboArea.SelectedValue == null || cboArea.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 동을선택하세요
                    Util.MessageValidation("SFU1499");
                    return false;
                }
            }

            return true;
        }

        private bool ValidationPopupQuality()
        {
            if (string.IsNullOrWhiteSpace(Util.NVC(_drSelectRow["LOTID"])))
            {
                // 생산 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4014");
                return false;
            }

            return true;
        }

        #endregion

        #region [Popup]

        #region # 품질정보 조회 팝업
        private void popupQuality()
        {
            if (!ValidationPopupQuality()) return;

            CMM_ASSY_QUALITY_PKG popQuality = new CMM_ASSY_QUALITY_PKG();
            popQuality.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popQuality.Name.ToString()) == false)
                return;

            object[] Parameters = new object[5];
            Parameters[0] = Util.NVC(_drSelectRow["EQSGID"]);
            Parameters[1] = Util.NVC(_drSelectRow["PROCID"]);
            Parameters[2] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[3] = Util.NVC(_drSelectRow["EQSGNAME"]);
            Parameters[4] = Util.NVC(_drSelectRow["EQPTNAME"]);

            C1WindowExtension.SetParameters(popQuality, Parameters);

            popQuality.Closed += new EventHandler(popQuality_Closed);

            // 팝업 화면 숨겨지는 문제 수정.
            this.Dispatcher.BeginInvoke(new Action(() => popQuality.ShowModal()));
            //foreach (System.Windows.UIElement child in grdMain.Children)
            //{
            //    if (child.GetType() == typeof(CMM_ASSY_QUALITY_PKG))
            //    {
            //        grdMain.Children.Remove(child);
            //        break;
            //    }
            //}

            //grdMain.Children.Add(wndPopup);
            //wndPopup.BringToFront();
        }

        private void popQuality_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_QUALITY_PKG popup = sender as CMM_ASSY_QUALITY_PKG;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
            this.grdMain.Children.Remove(popup);
        }

        #endregion

        #region # 설비불량정보 조회 팝업
        private void popupEqptDfctCell(string sPortID)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popEqptDfctCell = new CMM_ASSY_EQPT_DFCT_CELL_INFO();
            popEqptDfctCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popEqptDfctCell.Name.ToString()) == false)
                return;

            object[] Parameters = new object[3];
            Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[1] = Util.NVC(_drSelectRow["EQPTID"]);
            Parameters[2] = sPortID;
            C1WindowExtension.SetParameters(popEqptDfctCell, Parameters);

            popEqptDfctCell.Closed += new EventHandler(popEqptDfctCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popEqptDfctCell.ShowModal()));
        }

        private void popEqptDfctCell_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_EQPT_DFCT_CELL_INFO popup = sender as CMM_ASSY_EQPT_DFCT_CELL_INFO;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        #endregion

        #region # 완성LOT Cell 조회 팝업
        private void popupAssyCell(string sLotID, string CstID)
        {
            COM001_302_CELL popAssyCell = new COM001_302_CELL();
            popAssyCell.FrameOperation = FrameOperation;

            if (ValidationGridAdd(popAssyCell.Name.ToString()) == false)
                return;

            object[] Parameters = new object[4];
            Parameters[0] = Util.NVC(_drSelectRow["LOTID"]);
            Parameters[1] = sLotID;
            Parameters[2] = CstID;

            C1WindowExtension.SetParameters(popAssyCell, Parameters);

            popAssyCell.Closed += new EventHandler(popAssyCell_Closed);

            this.Dispatcher.BeginInvoke(new Action(() => popAssyCell.ShowModal()));
        }

        private void popAssyCell_Closed(object sender, EventArgs e)
        {
            COM001_302_CELL popup = sender as COM001_302_CELL;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
            }
        }

        #endregion

        #endregion

        #region [Func]
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }

            return true;
        }
        // 2024.01.08 남재현 : STK 특별 TRAY 설정 추가에 따른 공통 코드 조회 추가
        private bool IsCommonCodeUse(string sCmcdType, string sAreaid)
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(String));
                RQSTDT.Columns.Add("CMCODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = sCmcdType;
                dr["CMCODE"] = sAreaid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                    bFlag = true;

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #region # 그리드, 탭, Text Header Text 및 칼럼 Visibility
        private void SetControlVisibility(string sProcID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sProcID) || sProcID.Equals("SELECT"))
                    return;

                // Visibility.Collapsed
                InitTabControlVisible();

                #region ## 공정별 Header Text 변경
                DataRow[] drHeader;

                ////////////////////////////////////// dgLotList
                // 조립 전체
                drHeader = _dtLotListHeader.Select("PROCESS = '" + _assembly + "'");

                foreach (DataRow dr in drHeader)
                {
                    if (dgLotList.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["OBJECTNAME"].ToString().Split(',').ToList<string>();

                        if (_bLoad && !string.IsNullOrWhiteSpace(dr["OBJECTID"].ToString()))
                        {
                            dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["OBJECTID"].ToString().Split(',').ToList<string>();
                        }
                    }
                }

                // 공정별 
                drHeader = _dtLotListHeader.Select("PROCESS = '" + sProcID + "'");
                foreach (DataRow dr in drHeader)
                {
                    if (dgLotList.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["OBJECTNAME"].ToString().Split(',').ToList<string>();

                        if (_bLoad && !string.IsNullOrWhiteSpace(dr["OBJECTID"].ToString()))
                        {
                            dgLotList.Columns[dr["COLUMN"].ToString()].Header = dr["OBJECTID"].ToString().Split(',').ToList<string>();
                        }
                    }
                }
                _bLoad = false;

                ////////////////////////////////////// dgModelList
                // 조립 전체
                drHeader = _dtModelListHeader.Select("PROCESS = '" + _assembly + "'");

                foreach (DataRow dr in drHeader)
                {
                    if (dgModelList.Columns.Contains(dr["COLUMN"].ToString()))
                        dgModelList.Columns[dr["COLUMN"].ToString()].Header = dr["TITLE"].ToString().Split(',').ToList<string>();
                }

                // 공정별 
                drHeader = _dtModelListHeader.Select("PROCESS = '" + sProcID + "'");
                foreach (DataRow dr in drHeader)
                {
                    if (dgModelList.Columns.Contains(dr["COLUMN"].ToString()))
                        dgModelList.Columns[dr["COLUMN"].ToString()].Header = dr["TITLE"].ToString().Split(',').ToList<string>();
                }

                #endregion

                #region ## 공정별 칼럼 Visibility
                ////////////////////////////////////// dgLotList
                SetGridVisibility(_dtLotListColumnVisible, dgLotList, sProcID);

                ////////////////////////////////////// dgModelList
                SetGridVisibility(_dtModelListColumnVisible, dgModelList, sProcID);

                ////////////////////////////////////// 불량/Loss/물품청구 탭 : dgDefect
                SetGridVisibility(_dtDefectColumnVisible, dgDefect, sProcID);

                ////////////////////////////////////// 완성LOT 탭 : dgSubLot
                SetGridVisibility(_dtSubLotColumnVisible, dgSubLot, sProcID);

                ////////////////////////////////////// 투입이력 탭 : dgInputHist
                SetGridVisibility(_dtInputHistColumnVisible, dgInputHist, sProcID);

                #endregion

                // Format 
                SetGridFormat(dgLotList, sProcID);

                SetInputHistGridFormat(sProcID);

                //tbCurrProcLossQty.Visibility = Visibility.Collapsed;
                //txtCurrProcLossQty.Visibility = Visibility.Collapsed;
                tbInputQty.Text = ObjectDic.Instance.GetObjectName("생산");

                if (sProcID.Equals(Process.PACKAGING))
                {
                    // 완성LOT 탭
                    CTabReInput.Visibility = Visibility.Visible;
                    cTabHalf.Visibility = Visibility.Visible;
                    cTabTrayTime.Visibility = Visibility.Visible;
                    
                    // 재투입
                    tbReInputQty.Visibility = Visibility.Visible;
                    txtReInputQty.Visibility = Visibility.Visible;

                    // 실적확인 품질정보 조회
                    btnQualityInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    if (sProcID.Equals(Process.NOTCHING))
                    {
                        //tbCurrProcLossQty.Visibility = Visibility.Visible;
                        //txtCurrProcLossQty.Visibility = Visibility.Visible;

                        cTabEqptCut.Visibility = Visibility.Visible;
                    }
                    else if (sProcID.Equals(Process.VD_LMN))
                    {
                        dgLotList.Columns["INPUT_TAB_COUNT"].Visibility = Visibility.Visible;
                        dgLotList.Columns["END_TAB_COUNT"].Visibility = Visibility.Visible;
                        dgLotList.Columns["FIX_LOSS_QTY_VD"].Visibility = Visibility.Visible;
                        dgLotList.Columns["WRK_TYPE"].Visibility = Visibility.Visible;

                        //dgLotList.Columns["WRK_TYPE"].Visibility = Visibility.Visible;
                        dgLotList.Columns["PR_LOTID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["IRREGL_PROD_LOT_TYPE_NAME"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WOID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["WO_DETL_ID"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["PRE_PROC_INPUT_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["PRE_PROC_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["FIX_LOSS_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["EQPT_INPUT_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["RMN_QTY"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["PRDT_CLSS_CODE"].Visibility = Visibility.Collapsed;

                        //dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { "투입량", "투입량", "투입량" };
                        //dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { "양품량", "양품량", "양품량" };
                        //dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { "불량량", "불량량", "불량량" };
                        //dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { "LOSS량", "LOSS량", "LOSS량" };
                        //dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { "물품청구", "물품청구", "물품청구" };

                        dgLotList.Columns["INPUT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("투입량"), ObjectDic.Instance.GetObjectName("투입량") };
                        dgLotList.Columns["WIPQTY_ED"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("양품량"), ObjectDic.Instance.GetObjectName("양품량"), ObjectDic.Instance.GetObjectName("양품량") };
                        dgLotList.Columns["CNFM_DFCT_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("불량량"), ObjectDic.Instance.GetObjectName("불량량"), ObjectDic.Instance.GetObjectName("불량량") };
                        dgLotList.Columns["CNFM_LOSS_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("LOSS량"), ObjectDic.Instance.GetObjectName("LOSS량"), ObjectDic.Instance.GetObjectName("LOSS량") };
                        dgLotList.Columns["CNFM_PRDT_REQ_QTY"].Header = new List<string>() { ObjectDic.Instance.GetObjectName("물품청구"), ObjectDic.Instance.GetObjectName("물품청구"), ObjectDic.Instance.GetObjectName("물품청구") };

                        tbWorkOrder.Visibility = Visibility.Collapsed;
                        txtWorkorder.Visibility = Visibility.Collapsed;

                        cTabInputHalf.Visibility = Visibility.Collapsed;
                        cTabHalf.Visibility = Visibility.Collapsed;
                        tbInputQty.Text = ObjectDic.Instance.GetObjectName("투입");
                    }
                    else
                    {
                        if (sProcID.Equals(Process.STACKING_FOLDING))
                        {
                            CheckUnldrShrFlag();
                        }
                        else if (sProcID.Equals(Process.LAMINATION))
                        {
                            cTabInputLoss.Visibility = Visibility.Visible;
                        }

                        // 완성LOT 탭
                        cTabHalf.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetGridVisibility(DataTable dt, C1DataGrid dg, string ProcID)
        {
            dg.UpdateLayout();

            foreach (DataRow dr in dt.Rows)
            {
                string[] sProcess = dr["PROCESS"].ToString().Split(',');
                string[] sExceptionProcess = dr["EXCEPTION_PROCESS"].ToString().Split(',');
                int nExceptionindex;
                int nindex = -1;

                // 예외 공정 검색 
                nExceptionindex = Array.IndexOf(sExceptionProcess, ProcID);
                if (nExceptionindex >= 0)
                {
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // 대상공정 여부 체크
                    nindex = Array.IndexOf(sProcess, ProcID);

                    // 검색 공정이 없으면 조립 구분자로 다시 검색
                    if (nindex < 0)
                    {
                        nindex = Array.IndexOf(sProcess, _assembly);
                    }

                    // 공정별 칼럼 Visibility
                    if (dg.Columns.Contains(dr["COLUMN"].ToString()))
                    {
                        if (nindex < 0)
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Collapsed;
                        else
                            dg.Columns[dr["COLUMN"].ToString()].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void SetGridFormat(C1DataGrid dg, string ProcID)
        {
            string sFormat = string.Empty;

            if (ProcID.Equals(Process.NOTCHING))
            {
                sFormat = "###,##0.##";
            }
            else
            {
                sFormat = "###,##0";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["PRE_PROC_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["FIX_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["RMN_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CURR_PROC_LOSS_QTY"])).Format = sFormat;

            #region 2023.03.07   윤지해 : C20230109-000084 완성 생산량, 양품량, 불량량, LOSS량, 물품청구 부분 A5000(노칭)일 경우 M/EA 구분 추가
            // 2023.03.07 C20230109-000084 CNFM_DFCT_QTY, CNFM_LOSS_QTY, CNFM_PRDT_REQ_QTY 추가
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_DFCT_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_LOSS_QTY"])).Format = sFormat;
            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgLotList.Columns["CNFM_PRDT_REQ_QTY"])).Format = sFormat;
            #endregion
        }

        private void SetInputHistGridFormat(string ProcID)
        {
            string sFormat = string.Empty;

            if (ProcID.Equals(Process.NOTCHING))
            {
                sFormat = "###,##0.##";
            }
            else
            {
                sFormat = "###,##0";
            }

            ((C1.WPF.DataGrid.DataGridBoundColumn)(dgInputHist.Columns["RMN_QTY"])).Format = sFormat;
        }

        private string SetGridFormatBottomRow(string ColumnName, object obj)
        {
            string sFormat = string.Empty;
            double dFormat = 0;

            if (cboProcess.SelectedValue != null && cboProcess.SelectedValue.ToString() == Process.NOTCHING)
            {
                #region 2023.03.07   윤지해 : C20230109-000084 완성 생산량, 양품량, 불량량, LOSS량, 물품청구 부분 A5000(노칭)일 경우 M/EA 구분 추가
                // 2023.03.07 C20230109-000084 CNFM_DFCT_QTY, CNFM_LOSS_QTY, CNFM_PRDT_REQ_QTY 추가
                //if (ColumnName == "PRE_PROC_LOSS_QTY" || ColumnName == "FIX_LOSS_QTY" || ColumnName == "RMN_QTY")
                //    sFormat = "{0:###,##0.##}";
                if (ColumnName == "PRE_PROC_LOSS_QTY" || ColumnName == "FIX_LOSS_QTY" || ColumnName == "RMN_QTY" || ColumnName == "CNFM_DFCT_QTY" || ColumnName == "CNFM_LOSS_QTY" || ColumnName == "CNFM_PRDT_REQ_QTY")
                    sFormat = "{0:###,##0.##}";
                #endregion
                else
                    sFormat = "{0:###,##0}";
            }
            else
            {
                sFormat = "{0:###,##0}";
            }

            if (Double.TryParse(Util.NVC(obj), out dFormat))
                return String.Format(sFormat, dFormat);

            return string.Empty;
        }

        private void SetShopGridVisibility()
        {
            if (dgLotList.Columns["EQPT_DFCT_TAG_QTY"].Visibility == Visibility.Visible)
            {
                // 폴란드 조립인 경우만 보임
                if (LoginInfo.CFG_SHOP_ID != "G481")
                {
                    dgLotList.Columns["EQPT_DFCT_TAG_QTY"].Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SetCoatingLineColumnVisibility()
        {

            const string bizRuleName = "DA_BAS_SEL_TB_MMD_EQSG_ELTR_LINE_BAS";
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["AREAID"] = cboArea.SelectedValue;
                dr["PROCID"] = cboProcess.SelectedValue;
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

                if (CommonVerify.HasTableRow(dtResult))
                {
                    dgInputHist.Columns["COATING_LINE"].Visibility = Visibility.Visible;

                    if (cboProcess.SelectedValue.GetString() == "A5000")
                    {
                        dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Visible;
                        dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                        dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                    dgLotList.Columns["COATING_LINE_NOTCHING"].Visibility = Visibility.Visible;
                    dgInputHist.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                dgLotList.Columns["COATING_LINE"].Visibility = Visibility.Collapsed;

                Util.MessageException(ex);
            }

        }

        #endregion

        #region # 선택 생산 Lot의 실적 확인(textBiox) Setting
        private void SetValue()
        {
            txtSelectLot.Text = Util.NVC(_drSelectRow["LOTID"]);
            txtWorkorder.Text = Util.NVC(_drSelectRow["WOID"]);
            txtWorkorderDetail.Text = Util.NVC(_drSelectRow["WO_DETL_ID"]);
            txtWorkdate.Text = Util.NVC(_drSelectRow["CALDATE"]);

            txtShift.Text = Util.NVC(_drSelectRow["SHFT_NAME"]);
            txtShift.Tag = Util.NVC(_drSelectRow["SHIFT"]);
            txtStartTime.Text = Util.NVC(_drSelectRow["STARTDTTM"]);
            txtWorker.Text = Util.NVC(_drSelectRow["WRK_USER_NAME"]);
            txtWorker.Tag = Util.NVC(_drSelectRow["WRK_USERID"]);
            txtEndTime.Text = Util.NVC(_drSelectRow["ENDDTTM"]);
            txtInputQty.Value = Double.Parse(Util.NVC(_drSelectRow["INPUT_QTY"]));
            txtOutQty.Value = Double.Parse(Util.NVC(_drSelectRow["WIPQTY_ED"]));
            txtDefectQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_DFCT_QTY"]));
            txtLossQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_LOSS_QTY"]));
            txtPrdtReqQty.Value = Double.Parse(Util.NVC(_drSelectRow["CNFM_PRDT_REQ_QTY"]));
            //txtCurrProcLossQty.Value = Double.Parse(Util.NVC(_drSelectRow["CURR_PROC_LOSS_QTY"]));
            txtPreSectionQty.Value = Double.Parse(Util.NVC(_drSelectRow["PRE_SECTION_QTY"]));
            txtAfterSectionQty.Value = Double.Parse(Util.NVC(_drSelectRow["AFTER_SECTION_QTY"]));
            txtReInputQty.Value = Double.Parse(Util.NVC(_drSelectRow["RE_INPUT_QTY"]));
            txtNote.Text = GetWipNote();

            txtIrreglProdLotTyep.Text = Util.NVC(_drSelectRow["IRREGL_PROD_LOT_TYPE_NAME"]);

            CalculateQty();
        }
        #endregion

        #region # 특이사항 Split
        private string GetWipNote()
        {
            string sReturn;
            string[] sWipNote = Util.NVC(_drSelectRow["WIP_NOTE"]).Split('|');

            if (sWipNote.Length == 0)
            {
                sReturn = Util.NVC(_drSelectRow["WIP_NOTE"]);
            }
            else
            {
                sReturn = sWipNote[0];
            }
            return sReturn;
        }





        #endregion

        #region # 불량 합산        

        private void SumDefectQty()
        {
            try
            {
                DataTable dtDefect = DataTableConverter.Convert(dgDefect.ItemsSource);

                //double dDefect = double.Parse(Util.NVC(dtDefect.Compute("sum(RESNQTY)", "ACTID = 'DEFECT_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'")));
                //double dLoss = double.Parse(Util.NVC(dtDefect.Compute("sum(RESNQTY)", "ACTID = 'LOSS_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'")));
                //double dChargeProd = double.Parse(Util.NVC(dtDefect.Compute("sum(RESNQTY)", "ACTID = 'CHARGE_PROD_LOT' AND RSLT_EXCL_FLAG = 'N' AND ISNULL(PRCS_ITEM_CODE, '') <> 'UNIDENTIFIED_QTY'")));
                double dDefect = 0;
                double dLoss = 0;
                double dChargeProd = 0;

                foreach (DataRow dr in dtDefect.Rows)
                {
                    if (dr["RSLT_EXCL_FLAG"].Equals("N") && !dr["PRCS_ITEM_CODE"].Equals("UNIDENTIFIED_QTY"))
                    {
                        if (dr["ACTID"].Equals("DEFECT_LOT"))
                            dDefect += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else if (dr["ACTID"].Equals("LOSS_LOT"))
                            dLoss += double.Parse(Util.NVC(dr["RESNQTY"]));
                        else
                            dChargeProd += double.Parse(Util.NVC(dr["RESNQTY"]));
                    }
                }

                txtDefectQty.Value = dDefect;
                //txtLossQty.Value = dLoss;
                txtPrdtReqQty.Value = dChargeProd;

                CalculateQty();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CalculateQty()
        {
            try
            {
                // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) - 구간잔량(전+후) + 재투입수
                //double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - txtDefectQty.Value - txtLossQty.Value - txtPrdtReqQty.Value - (txtPreSectionQty.Value + txtAfterSectionQty.Value) + txtReInputQty.Value;
                // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외) + 재투입수
                //double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - txtDefectQty.Value - txtLossQty.Value - txtPrdtReqQty.Value + txtReInputQty.Value;
                // 미확인 Loss = 투입수 - 양품수 - 불량Sum(실적제외 및 미확인LOSS 항목 제외)
                double dUnidentifiedQty = txtInputQty.Value - txtOutQty.Value - txtDefectQty.Value - txtLossQty.Value - txtPrdtReqQty.Value;

                txtUnidentifiedQty.Value = dUnidentifiedQty;

                /////////////////////////// LOSS(Total) 수량에 미확인Loss 수량까지 합산하여 보이도록 수정 
                //txtLossQty.Value = txtLossQty.Value + dUnidentifiedQty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private decimal SumGridColumnDecimal(string ColumnName, int SortNo)
        {
            DataRow[] dr = DataTableConverter.Convert(dgLotList.ItemsSource).Select("SORT_NO = " + SortNo + "");

            if (dr.Length == 0) return 0;

            return dr.AsEnumerable().Sum(r => r.GetValue(ColumnName).GetDecimal());     // 2024.11.11 천진수 형변환개선 
        }
        private long SumGridColumnInt(string ColumnName, int SortNo)
        {
            DataRow[] dr = DataTableConverter.Convert(dgLotList.ItemsSource).Select("SORT_NO = " + SortNo + "");

            if (dr.Length == 0) return 0;

            return dr.AsEnumerable().Sum(r => r.GetValue(ColumnName).GetInt());     // 2024.11.11 천진수 형변환개선
        }
        private void SetAreaGridVisibility()
        {
            if (cboArea.SelectedValue == null) return;
            if (cboProcess.SelectedValue == null) return;

            // 재투입 수량은 패키징 공정진척 인 경우에 한하여 조건 처리 함.
            dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;


            if (cboProcess.SelectedValue.ToString() == Process.PACKAGING)
            {
                // 동별 공통코드 항목에 RE_INPUT_DFCT_CODE 등록된 동만 보이도록 처리
                if (CheckReInputDefectCode())
                {
                    dgReInput.Columns["RESNNAME"].Visibility = Visibility.Visible;
                    dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgReInput.Columns["RESNNAME"].Visibility = Visibility.Collapsed;
                    dgDefect.Columns["RE_INPUT_QTY"].Visibility = Visibility.Collapsed;
                }
            }

        }

        private void SetSearchControl()
        {
            cboElecType.IsEnabled = false;

            if (((System.Windows.FrameworkElement)tbcList.SelectedItem).Name.Equals("cTabLot"))
            {
                if (cboProcess.SelectedValue.ToString() == Process.NOTCHING)
                {
                    cboElecType.IsEnabled = true;
                }
            }
        }


        #endregion

        #endregion

        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                CMM001.Popup.CMM_COM_WIPHIST_HIST wndHist = new CMM001.Popup.CMM_COM_WIPHIST_HIST();
                wndHist.FrameOperation = FrameOperation;

                if (wndHist != null)
                {
                    object[] Parameters = new object[2];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                    Parameters[1] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPSEQ"));

                    C1WindowExtension.SetParameters(wndHist, Parameters);

                    wndHist.Closed += new EventHandler(wndHist_Closed);

                    this.Dispatcher.BeginInvoke(new Action(() => wndHist.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndHist_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_COM_WIPHIST_HIST window = sender as CMM001.Popup.CMM_COM_WIPHIST_HIST;
            if (window.DialogResult == MessageBoxResult.OK)
            {

            }
        }

        // 김용군 Stack별 불량 정보 조회
        private void GetStackDefect()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                // 김용군
                dt.Columns.Add("WIPSEQ", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                dr["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                // 김용군
                dr["WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_EQP_SEL_LOTSTKDFCT_INFO_L", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    // 김용군
                    //Util.GridSetData(dgStackDefect, searchResult, null, true);
                    Util.GridSetData(dgStackDefect, searchResult, null);


                    // 김용군
                    dgStackDefect.MergingCells -= dgStackDefect_MergingCells;
                    dgStackDefect.MergingCells += dgStackDefect_MergingCells;

                    if (searchResult?.Rows?.Count > 0 && searchResult?.Select("EQPT_DFCT_GR_SUM_YN = 'Y'")?.Length > 0)
                        dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Visible;
                    else
                        dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Visibility = Visibility.Collapsed;

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        // 김용군 생산 정보 조회
        private void GetStackInputEndInfo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("PROD_WIPSEQ", typeof(int));
                dt.Columns.Add("INPUT_LOTID", typeof(string));
                dt.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Util.NVC(_drSelectRow["LOTID"]);
                dr["EQPTID"] = Util.NVC(_drSelectRow["EQPTID"]);
                dr["PROD_WIPSEQ"] = Util.NVC(_drSelectRow["WIPSEQ"]);
                dt.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_EQP_SEL_LOTSTKINOUT_INFO_L", "INDATA", "OUTDATA", dt, (searchResult, searchException) =>
                {
                    HiddenLoadingIndicator();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    // 김용군
                    //Util.GridSetData(dgStackInputEndQty, searchResult, null, true);
                    Util.GridSetData(dgStackInputEndQty, searchResult, null);

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        // 김용군
        private void dgStackDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;
            if (sender == null) return;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        // 김용군
                        //if (!Util.NVC(e.Cell.Column.Name).Equals("MACHINE"))
                        //{
                        //    string flag = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "EQPT_DFCT_GR_SUM_YN"));
                        //    if (flag == "Y")
                        //    {
                        //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //    }
                        //    else
                        //    {
                        //        e.Cell.Presenter.Background = null;
                        //        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //    }
                        //}
                        if (Util.NVC(e.Cell.Column.Name).Equals("PORT_NAME"))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        // 합계 컬럼 색 변경.
                        if (!Util.NVC(e.Cell.Column.Name).Equals("MACHINE") && !Util.NVC(e.Cell.Column.Name).Equals("EQPT_DFCT_SUM_GR_NAME") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "EQPT_DFCT_GR_SUM_YN")).Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA648"));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }));
        }

        // 김용군
        private void dgStackDefect_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        // 김용군
        private void dgStackDefect_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;

                for (int i = dgStackDefect.TopRows.Count; i < dgStackDefect.Rows.Count; i++)
                {
                    if (dgStackDefect.Rows[i].DataItem.GetType() == typeof(DataRowView))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("") || Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals("-"))
                        {
                            if (bStrt)
                            {
                                e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));
                                bStrt = false;
                            }
                        }

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE")).Equals(sTmpLvCd))
                                idxE = i;
                            else
                            {
                                e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                                idxS = i;

                                if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                    bStrt = false;
                            }
                        }
                    }
                    else
                    {
                        if (bStrt)
                        {
                            e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgStackDefect.Rows[i].DataItem, "EQPT_DFCT_SUM_GR_CODE"));
                            idxS = i;

                            if (sTmpLvCd.Equals("") || sTmpLvCd.Equals("-"))
                                bStrt = false;
                        }
                    }
                }

                if (bStrt)
                {
                    e.Merge(new DataGridCellsRange(dgStackDefect.GetCell(idxS, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index), dgStackDefect.GetCell(idxE, dgStackDefect.Columns["EQPT_DFCT_SUM_GR_NAME"].Index)));

                    bStrt = false;
                }
            }
            catch (Exception)
            {
                //Util.MessageException(ex);
            }
        }

        // 김용군
        private void dgStackDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dataGrid.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "PORT_NAME")
                    {
                        if (!string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, cell.Column.Name))))
                        {
                            popupEqptDfctCell(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[cell.Row.Index].DataItem, "PORT_ID")));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //ESMI-A4동 1~5 Line 제외처리
        private bool IsCmiExceptLine()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("COM_TYPE_CODE", typeof(string));
                inTable.Columns.Add("USE_FLAG", typeof(string));
                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "UI_FIRST_PRIORITY_LINE_ID";
                dr["USE_FLAG"] = "Y";
                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_USE", "INDATA", "OUTDATA", inTable);

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
    }
}