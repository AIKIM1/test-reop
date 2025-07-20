/*************************************************************************************
 Created Date : 2019.08.07
      Creator : 염규범
  Description :
--------------------------------------------------------------------------------------
 [Change History]
  2019.08.07  염규범    Initialize
  2021.12.24  정용석    부모 Form에서 넘어온 데이터 가지고 Grid Data 표출 (기존 순서도 호출 제외시킴)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_044_POPUP_TERM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_044_POPUP_TERM()
        {
            InitializeComponent();
        }
        #endregion

        #region Member Function List
        private void ShowUnwholesomenessLOTList(object obj)
        {
            try
            {
                if (!obj.GetType().Equals(typeof(DataTable)))
                {
                    return;
                }

                DataTable dtNewLOTList = (DataTable)obj;
                if (!CommonVerify.HasTableRow(dtNewLOTList))
                {
                    return;
                }

                var query = dtNewLOTList.AsEnumerable().Select(x => new
                {
                    LOTID = x.Field<string>("LOTID"),
                    PRODID = x.Field<string>("PRODID"),
                    PRODNAME = x.Field<string>("PRODNAME"),
                    PROCNAME = x.Field<string>("PROCNAME"),
                    WIPSNAME = x.Field<string>("WIPSNAME"),
                    NOTE =
                    (
                        x.Field<string>("WIPHOLD").Equals("Y") ? MessageDic.Instance.GetMessage("SFU8455") :            // 해당 재공은 홀드 상태입니다
                        x.Field<string>("LOTSTAT").Equals("SHIPPING") ? MessageDic.Instance.GetMessage("SFU8456") :     // 해당 재공은 이미 출고 되었습니다.
                        x.Field<string>("LOTSTAT").Equals("SHIPPED") ? MessageDic.Instance.GetMessage("SFU8456") :      // 해당 재공은 이미 출고 되었습니다.
                        x.Field<string>("LOTSTAT").Equals("ASSEMBLED") ? MessageDic.Instance.GetMessage("SFU8457") :    // 해당 재공은 이미 결합된 상태입니다.
                        x.Field<string>("WIPSTAT").Equals("TERM") ? MessageDic.Instance.GetMessage("SFU8458") :         // 해당 재공은 Term 상태입니다.
                        !string.IsNullOrEmpty(x.Field<string>("BOXID")) ? MessageDic.Instance.GetMessage("SFU8459") :   // 해당 재공은 포장 상태입니다.

                        // As-Is : PR000 / PS000 공정 LOT 만 처리 가능
                        // To-Be :
                        // 2021-11-15 : 재공상태가 BIZWF인 LOT일 경우 재공 종료 처리 불가
                        // 2021-11-15 : 재공상태가 MOVING인 LOT일 경우 재공 종료 처리 불가
                        // 2021-11-15 : 공정이 PB000 (반품후 대기) LOT인 경우 재공 종료 처리 불가
                        // 2021-11-15 : 공정이 PD000 (등외품 관리) LOT인 경우 재공 종료 처리 불가
                        // 2021-11-15 : ERP 창고가 NULL 이 아닌 LOT일 경우
                        // 2021-12-24 : 공정이 P1뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
                        // 2021-12-24 : 공정이 P5뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
                        // 2021-12-24 : 공정이 P9뭐시깽이인것들은 재공 종료 처리 불가 (ERP 창고가 NULL인 아닌 것들은 재공 종료 처리 불가 걷어냄)
                        // 2021-12-24 : 반품이력이 있는 LOT에 대해서 재공 삭제 불가 INTERLOCK 적용
                        x.Field<string>("WIPSTAT").Equals("BIZWF") ? MessageDic.Instance.GetMessage("SFU8460", x.Field<string>("WIPSTAT")) :    // %1 상태의 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("WIPSTAT").Equals("MOVING") ? MessageDic.Instance.GetMessage("SFU8460", x.Field<string>("WIPSTAT")) :   // %1 상태의 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("PROCID").Equals("PB000") ? MessageDic.Instance.GetMessage("SFU8461", x.Field<string>("PROCNAME")) :    // %1 공정 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("PROCID").Equals("PD000") ? MessageDic.Instance.GetMessage("SFU8461", x.Field<string>("PROCNAME")) :    // %1 공정 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("PROCID").StartsWith("P1") ? MessageDic.Instance.GetMessage("SFU8461", x.Field<string>("PROCNAME")) :   // %1 공정 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("PROCID").StartsWith("P5") ? MessageDic.Instance.GetMessage("SFU8461", x.Field<string>("PROCNAME")) :   // %1 공정 재공은 재공종료 처리가 불가합니다.
                        x.Field<string>("PROCID").StartsWith("P9") ? MessageDic.Instance.GetMessage("SFU8461", x.Field<string>("PROCNAME")) :   // %1 공정 재공은 재공종료 처리가 불가합니다.
                        // 2022-02-07 반픔이력 관련 재공종료 불가 INTERLOCK ROLLBACK
                        //!string.IsNullOrEmpty(x.Field<string>("RCV_ISS_PRODID")) ? MessageDic.Instance.GetMessage("SFU8462") :                // 반품 이력이 있는 재공은 재공종료 처리가 불가합니다. 
                        MessageDic.Instance.GetMessage("SFU8463")                                                                               // 시스템 관리자에게 확인이 필요한 LOT 입니다.
                    )
                });

                DataTable dtResult = DataTableConverter.Convert(this.dgExceptLotList.ItemsSource).AsEnumerable().Union(PackCommon.queryToDataTable(query.ToList()).AsEnumerable()).CopyToDataTable();
                Util.GridSetData(this.dgExceptLotList, dtResult, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] objParam = C1WindowExtension.GetParameters(this);
            if (objParam == null || objParam.Length <= 0)
            {
                return;
            }
            this.ShowUnwholesomenessLOTList(objParam[0]);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}