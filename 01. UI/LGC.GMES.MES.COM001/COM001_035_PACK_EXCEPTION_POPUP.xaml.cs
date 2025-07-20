/*************************************************************************************
 Created Date : 2022.06.10
      Creator : 염규범
  Description : 출고 인터락 해체 요청(Pack)
--------------------------------------------------------------------------------------
 [Change History]
  2022.06.10 염규범 : Initial Created.
  2023.02.22 정용석 : Exception Lot List 항목 추가 (LOT Release 요청, 물품청구 요청, 전공정 Loss 요청 등등)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_035_PACK_EXCEPTION_POPUP : C1Window
    {
        #region Member Variable Lists...
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public COM001_035_PACK_EXCEPTION_POPUP()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function Lists...
        // 예외유형별 LOT List Data Binding
        private void GetExceptionLOTList()
        {
            object[] arrObject = C1WindowExtension.GetParameters(this);
            if (arrObject == null || arrObject.Length < 1)
            {
                return;
            }

            DataTable dt = (DataTable)arrObject[0];
            string exceptionType = arrObject[1].ToString();
            this.SetVisibleColumnHeader(exceptionType);
            this.MakeExceptionData(dt, exceptionType);
        }

        // 에외유형별 Column 숨기기
        private void SetVisibleColumnHeader(string exceptionType)
        {
            /***************************************************************************************
                ColumnHeaderIndex=0 : Header="INPUT_LOTID" Binding="{Binding INPUT_LOTID}"
                ColumnHeaderIndex=1 : Header="LOTID"       Binding="{Binding LOTID}"
                ColumnHeaderIndex=2 : Header="요청번호"    Binding="{Binding REQ_NO}"
                ColumnHeaderIndex=3 : Header="HOLD 여부"   Binding="{Binding WIPHOLD}"
                ColumnHeaderIndex=4 : Header="WIP상태"     Binding="{Binding WIPSTAT}"
                ColumnHeaderIndex=5 : Header="제품ID"      Binding="{Binding PRODID}"
                ColumnHeaderIndex=6 : Header="등급품코드"  Binding="{Binding GRD_PRODID}"
                ColumnHeaderIndex=7 : Header="홀드사유"    Binding="{Binding RESNCODE}"
                ColumnHeaderIndex=8 : Header="공정"        Binding="{Binding PROCID}"
                ColumnHeaderIndex=9 : Header="사유"        Binding="{Binding NOTE}"
             ***************************************************************************************/
            switch (exceptionType.ToUpper())
            {
                case "RELEASE_LOT":                 // COM001_035_RELEASE_PACK - HOLD RELEASE 요청 화면에서 조회시
                case "MATERIAL_REQUEST_LOT":        // COM001_035_RELEASE1_PACK - 물품청구 요청화면에서 LOTID 입력시
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                case "HOLD_LOT":                    // PACK001_060 - Lot 홀드 요청 화면
                    this.dgExceptionLOTList.Columns["REQ_NO"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["WIPHOLD"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                case "BATCH_PROCESSING_OFFGRADE":   // PACK001_075 - 일괄처리 화면
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["REQ_NO"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["WIPHOLD"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                case "BATCH_PROCESSING_RETURN":     // PACK001_075 - 일괄처리 화면
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["REQ_NO"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["WIPSTAT"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    break;
                case "APPR_BIZ":                    // 각종 승인 요청 Popup 화면
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                case "MTOM":                        // PACK001_100 - 제품변경 화면
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                case "GROUP_MENAGEMENT":            // PACK003_031 - 그룹 관리 Popup 화면 
                    this.dgExceptionLOTList.Columns["INPUT_LOTID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["REQ_NO"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["WIPHOLD"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["WIPSTAT"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["GRD_PRODID"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["RESNCODE"].Visibility = Visibility.Collapsed;
                    this.dgExceptionLOTList.Columns["PROCID"].Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }

            // Header명 변경
            switch (exceptionType.ToUpper())
            {
                case "RELEASE_LOT":                 // COM001_035_RELEASE_PACK - HOLD RELEASE 요청 화면에서 조회시
                case "MATERIAL_REQUEST_LOT":        // COM001_035_RELEASE1_PACK - 물품청구 요청화면에서 LOTID 입력시
                case "HOLD_LOT":                    // PACK001_060 - Lot 홀드 요청 화면
                case "BATCH_PROCESSING_OFFGRADE":   // PACK001_075 - 일괄처리 화면
                case "BATCH_PROCESSING_RETURN":     // PACK001_075 - 일괄처리 화면
                case "MTOM":                        // PACK001_100 - 제품변경 화면
                case "GROUP_MENAGEMENT":            // PACK003_031 - 그룹관리 화면
                    break;
                case "APPR_BIZ":                    // 각종 승인 요청 Popup 화면
                    this.dgExceptionLOTList.Columns["WIPHOLD"].Header = ObjectDic.Instance.GetObjectName("요청구분");
                    this.dgExceptionLOTList.Columns["WIPSTAT"].Header = ObjectDic.Instance.GetObjectName("진행상태");
                    break;
                default:
                    break;
            }
        }

        // 예외유형별 Data 조작하기
        private void MakeExceptionData(DataTable dt, string exceptionType)
        {
            switch (exceptionType.ToUpper())
            {
                case "RELEASE_LOT":                 // COM001_035_RELEASE_PACK - HOLD RELEASE 요청 화면에서 조회시
                    var queryExceptionListReleaseLOT = dt.AsEnumerable().Select(x => new
                    {
                        LOTID = x.Field<string>("LOTID"),
                        REQ_NO = x.Field<string>("REQ_NO"),
                        WIPHOLD = x.Field<string>("HOLD_YN"),
                        WIPSTAT = x.Field<string>("WIPSTAT"),
                        NOTE =
                        (
                            string.IsNullOrEmpty(x.Field<string>("CHECK_LOTID")) ? MessageDic.Instance.GetMessage("SFU1386") :                  // LOT정보가 없습니다.
                            x.Field<string>("HOLD_YN").Equals("N") ? ObjectDic.Instance.GetObjectName("해당 재공은 홀드 상태가 아닙니다.") :
                            x.Field<string>("WIPSTAT").Equals("TERM") ? MessageDic.Instance.GetMessage("SFU8119") :                             // 해당 LOT은 재공종료 상태입니다.
                            !string.IsNullOrEmpty(x.Field<string>("REQ_NO")) ? MessageDic.Instance.GetMessage("SFU5133") :                      // 이미 요청중이거나 승인중인 LOT입니다.
                            string.Empty
                        )
                    });
                    Util.GridSetData(this.dgExceptionLOTList, Util.queryToDataTable(queryExceptionListReleaseLOT.ToList()), FrameOperation);
                    break;
                case "MATERIAL_REQUEST_LOT":        // COM001_035_RELEASE1_PACK - 물품청구 요청화면에서 LOTID 입력시
                    var queryExceptionListMaterialRequestLOT = dt.AsEnumerable().Select(x => new
                    {
                        LOTID = x.Field<string>("LOTID"),
                        REQ_NO = x.Field<string>("REQ_NO"),
                        WIPHOLD = x.Field<string>("WIPHOLD"),
                        WIPSTAT = x.Field<string>("WIPSTAT"),
                        PROCID = x.Field<string>("PROCID"),
                        PRODID = x.Field<string>("PRODID"),
                        NOTE =
                        (
                            string.IsNullOrEmpty(x.Field<string>("CHECK_LOTID")) ? MessageDic.Instance.GetMessage("SFU1386") :                  // LOT정보가 없습니다.
                            x.Field<string>("WIPHOLD").Equals("Y") ? MessageDic.Instance.GetMessage("SFU1340") :                                // HOLD된 LOT ID입니다.
                            !x.Field<string>("PROCID").Equals("PR000") ? MessageDic.Instance.GetMessage("SFU8901") :                            // 수리공정에 있는 LOT이 아닙니다. (NEW)
                            !("WAIT,PROC").Contains(x.Field<string>("WIPSTAT")) ? MessageDic.Instance.GetMessage("SFU8902") :                   // 재공상태가 대기 또는 진행중 상태가 아닌 LOT ID입니다. (NEW)
                            !string.IsNullOrEmpty(x.Field<string>("REQ_NO")) ? MessageDic.Instance.GetMessage("SFU5133") :                      // 이미 요청중이거나 승인중인 LOT입니다.
                            string.Empty
                        )
                    });
                    Util.GridSetData(this.dgExceptionLOTList, Util.queryToDataTable(queryExceptionListMaterialRequestLOT.ToList()), FrameOperation);
                    break;
                case "MTOM":                        // PACK001_100 - 제품변경 화면
                    var queryExceptionListMTOM = dt.AsEnumerable().Select(x => new
                    {
                        LOTID = x.Field<string>("INPUT_LOTID"),
                        REQ_NO = x.Field<string>("REQ_NO"),
                        WIPHOLD = x.Field<string>("WIPHOLD"),
                        WIPSTAT = x.Field<string>("WIPSTAT"),
                        PROCID = x.Field<string>("PROCID"),
                        PRODID = x.Field<string>("TO_PRODID"),
                        NOTE =
                        (
                            string.IsNullOrEmpty(x.Field<string>("LOTID")) ? MessageDic.Instance.GetMessage("SFU1386") :                                         // LOT정보가 없습니다.
                            x.Field<string>("CHK_VALID") == "LOTID NOT EXISTS" ? MessageDic.Instance.GetMessage("SFU1386") :                                     // LOT정보가 없습니다.
                            x.Field<string>("CHK_VALID") == "NOT PR000" ? MessageDic.Instance.GetMessage("SFU8901") :                                            // 수리공정에 있는 LOT이 아닙니다. (NEW)
                            x.Field<string>("CHK_VALID") == "NOT WIPSTAT" ? MessageDic.Instance.GetMessage("SFU8902") :                                          // 재공상태가 대기 또는 진행중 상태가 아닌 LOT ID입니다. (NEW)
                            x.Field<string>("CHK_VALID") == "WIP HOLD" ? MessageDic.Instance.GetMessage("SFU1340") :                                             // HOLD된 LOT ID입니다.
                            x.Field<string>("CHK_VALID") == "QMS HOLD" ? MessageDic.Instance.GetMessage("96176", x.Field<string>("INPUT_LOTID")) :               // [%1] 에 QMS HOLD인 상태입니다.
                            x.Field<string>("CHK_VALID") == "APPROVE REQUEST" ? MessageDic.Instance.GetMessage("SFU5133") :                                      // 이미 요청중이거나 승인중인 LOT입니다.
                            x.Field<string>("CHK_VALID") == "INVALID FROM PRODUCTID" ? MessageDic.Instance.GetMessage("SFU1897") :                               // 제품코드가 틀립니다.
                            x.Field<string>("CHK_VALID") == "INVALID TO PRODUCTID" ? MessageDic.Instance.GetMessage("100116", x.Field<string>("TO_PRODID")) :    // 존재하지 않는 (반)제품[%1] 입니다.
                            x.Field<string>("CHK_VALID") == "NOT TO PRODUCTID" ? MessageDic.Instance.GetMessage("SFU8675") :                                     // 포장 제품으로 변경할 수 없습니다.
                            x.Field<string>("CHK_VALID") == "NOT EQSGID" ? MessageDic.Instance.GetMessage("SFU4618") :                                           // 라인정보가 틀립니다.
                            string.Empty
                        )
                    });
                    Util.GridSetData(this.dgExceptionLOTList, Util.queryToDataTable(queryExceptionListMTOM.ToList()), FrameOperation);
                    break;
                case "GROUP_MENAGEMENT":            // PACK003_031 - 그룹관리 화면
                    var queryExceptionListGroupManagement = dt.AsEnumerable().Select(x => new
                    {
                        LOTID = x.Field<string>("LOTID"),
                        NOTE =
                        (
                            x.Field<string>("MESSAGEID").Equals("SFU1384") ? MessageDic.Instance.GetMessage("SFU1384") :                             //LOT이 이미 있습니다.
                            x.Field<string>("MESSAGEID").Equals("SFU8473") ? MessageDic.Instance.GetMessage("SFU8473", x.Field<string>("LOTID")) :   //이미 다른 Group에 포함된 LOT입니다. 
                            x.Field<string>("MESSAGEID").Equals("SFU4957") ? MessageDic.Instance.GetMessage("SFU4957", x.Field<string>("LOTID")) :   //이전에 스캔한 LOT과 LOT[%1]의 공정이 다릅니다.
                            x.Field<string>("MESSAGEID").Equals("SFU4956") ? MessageDic.Instance.GetMessage("SFU4956", x.Field<string>("LOTID")) :   //이전에 스캔한 LOT과 LOT[%1]의 제품ID가 같지 않습니다.
                            x.Field<string>("MESSAGEID").Equals("SFU5950") ? MessageDic.Instance.GetMessage("SFU5950") :                             //초과수량
                            MessageDic.Instance.GetMessage("SFU2816")                                                                                //조회 결과가 없습니다.
                        )
                    });
                    Util.GridSetData(this.dgExceptionLOTList, Util.queryToDataTable(queryExceptionListGroupManagement.ToList()), FrameOperation);
                    break;

                case "HOLD_LOT":                    // PACK001_060 - Lot 홀드 요청 화면
                case "BATCH_PROCESSING_OFFGRADE":   // PACK001_075 - 일괄처리 화면
                case "BATCH_PROCESSING_RETURN":     // PACK001_075 - 일괄처리 화면
                    Util.GridSetData(this.dgExceptionLOTList, dt, FrameOperation);
                    break;
                case "APPR_BIZ":
                    var queryExceptionListApproveBusiness = dt.AsEnumerable().Select(x => new
                    {
                        LOTID = x.Field<string>("LOTID"),
                        REQ_NO = x.Field<string>("REQ_NO"),
                        WIPHOLD = x.Field<string>("APPR_BIZ_NAME"),             // Column Mapping : WIPHOLD = x.Field<string>("APPR_BIZ_NAME")
                        WIPSTAT = x.Field<string>("REQ_RSLT_NAME"),             // Column Mapping : WIPSTAT = x.Field<string>("REQ_RSLT_NAME")
                        NOTE = MessageDic.Instance.GetMessage("SFU5133")        // 이미 요청중이거나 승인중인 LOT입니다.
                    });
                    Util.GridSetData(this.dgExceptionLOTList, Util.queryToDataTable(queryExceptionListApproveBusiness.ToList()), FrameOperation);
                    break;
                default:
                    Util.GridSetData(this.dgExceptionLOTList, dt, FrameOperation);
                    break;
            }
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.GetExceptionLOTList();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
    }
}
