/*************************************************************************************
 Created Date : 2024.04.02
      Creator : 정용석
  Description : PACK UI에서 예외 Message 공통 Popup
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.02 정용석 : Initial Created.
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class EXCEPTION_POPUP : C1Window
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
        public EXCEPTION_POPUP()
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
            this.AddGridColumn(exceptionType);
            this.MakeExceptionData(dt, exceptionType);
        }

        private void AddGridColumn(string exceptionType)
        {
            switch (exceptionType.ToUpper())
            {
                case "PACK001_051":                 // 재작업/폐기 일괄작업 UI에서의 예외발생 LOT DATA 표출
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "INPUT_LOTID", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "LOTID", false);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "PROCID_CAUSE", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "EQSGID", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "NOTE", true);
                    break;

                case "PACK001_067":                 // Rack포장(Pack) UI에서 Mapping LOT 입력시 예외발생 LOT DATA 표출
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "SCANID", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "LOTID", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "NOTE", true);
                    break;

                default:
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "LOTID", true);
                    PackCommon.AddGridColumn(this.dgExceptionLOTList, "TEXT", "NOTE", true);
                    break;
            }
        }

        // 예외유형별 Data 조작하기
        private void MakeExceptionData(DataTable dt, string exceptionType)
        {
            switch (exceptionType.ToUpper())
            {
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
