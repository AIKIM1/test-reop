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
    public partial class PACK003_048_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_048_POPUP()
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
                    PRODNAME = x.Field<string>("PRODNAME"),
                    WIPSNAME = x.Field<string>("WIPSNAME"),
                    NOTE =
                    (
                        x.Field<string>("RESNCODE").Equals("NOT_EXISTS") ? MessageDic.Instance.GetMessage("SFU8485") :                          // 해당 재공은 존재하지 않습니다.
                        x.Field<string>("RESNCODE").Equals("NOT_TERM") ? MessageDic.Instance.GetMessage("SFU8486") :                            // 해당 재공은 삭제되지 않은 상태입니다.
                        x.Field<string>("RESNCODE").Equals("NOT_DEFINE_ACTID") ? MessageDic.Instance.GetMessage("SFU8487") :                    // 재공 생성 대상이 아닙니다.
                        x.Field<string>("RESNCODE").Equals("LAST_ACT") ? MessageDic.Instance.GetMessage("SFU8488", x.Field<string>("ACTID")) :  // 재공 활동 이력 중 재공 생성 처리에 맞지 않는 이력이 존재합니다. %1
                        x.Field<string>("RESNCODE").Equals("BOXING") ? MessageDic.Instance.GetMessage("SFU8459") :                              // 해당 재공은 포장 상태입니다.
                        x.Field<string>("RESNCODE").Equals("ALREADY_ATTACH") ? MessageDic.Instance.GetMessage("SFU8457") :                      // 해당 재공은 이미 결합이 되었습니다.
                        x.Field<string>("RESNCODE").Equals("NOT_EXISTS_TERM_LAST_ACTID") ? MessageDic.Instance.GetMessage("SFU8489") :          // 재공 생성으로 적용된 활동 이력이 없습니다.
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