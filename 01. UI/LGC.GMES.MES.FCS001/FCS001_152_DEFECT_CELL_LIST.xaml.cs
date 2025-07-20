/*************************************************************************************
 Created Date : 2023.02.23
      Creator : 홍석원
   Decription : 작업 중 Lot 불량 현황 메뉴 팝업 수량 클릭 시 Cell List 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.23  홍석원 : Initial Created.
  2023.03.13  조영대 : 비즈연결
  2023.03.30  조영대 : 라인 ALL 추가, LOT 유형 조건 추가
  2023.12.28  최윤호 : E20231227-000291 EOL투입시간 추가 
  2024.07.03  Meilia : E20240613-001554 Improvement in the number and list of defect cells(GDC)
**************************************************************************************/


using System;
using System.Data;
using System.Linq;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_152_DEFECT_CELL_LIST.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_152_DEFECT_CELL_LIST : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private string popupType = string.Empty;
        private string calDateFrom = string.Empty;
        private string calDateTo = string.Empty;
        private string eqsgId = string.Empty;
        private string lotType = string.Empty;
        private string eqptId = string.Empty;
        private string areaId = string.Empty;
        private string dfctGrTypeCode = string.Empty;
        private string dfctCode = string.Empty;
        private string workTypeCode = string.Empty;
        private string shiftID = string.Empty; // 2024.07.03 Meilia add shift ID


        public FCS001_152_DEFECT_CELL_LIST()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length < 1) return;

            popupType = Util.NVC(parameters[0]);
            calDateFrom = Util.NVC(parameters[1]);
            calDateTo = Util.NVC(parameters[2]);
            eqsgId = Util.NVC(parameters[3]).Equals(string.Empty) ? null : Util.NVC(parameters[3]);
            lotType = Util.NVC(parameters[4]).Equals(string.Empty) ? null : Util.NVC(parameters[4]);
            eqptId = Util.NVC(parameters[5]).Equals(string.Empty) ? null : Util.NVC(parameters[5]);
            areaId = Util.NVC(parameters[6]);
            dfctGrTypeCode = Util.NVC(parameters[7]);
            dfctCode = Util.NVC(parameters[8]);
            workTypeCode = Util.NVC(parameters[9]);
            shiftID = Util.NVC(parameters[10]).Equals(string.Empty) ? null : Util.NVC(parameters[10]); //2024.07.03 Meilia add SHIFTID
            GetList();
        }

        #endregion

        #region Event

        private void dgDefectCellList_ExecuteDataCompleted(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            this.IsActive = true;
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CALDATE_FROM", typeof(string));
                dtRqst.Columns.Add("CALDATE_TO", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("WORK_TYPE", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("DFCT_GR_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("DFCT_CODE", typeof(string));
                dtRqst.Columns.Add("SHFT_ID", typeof(string)); //2024.07.03 Meilia add SHIFTID

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CALDATE_FROM"] = calDateFrom;
                dr["CALDATE_TO"] = calDateTo;
                dr["EQSGID"] = eqsgId;
                dr["LOTTYPE"] = lotType;
                dr["WORK_TYPE"] = "DEFECT_CELL_LIST";
                dr["EQPTID"] = eqptId;
                dr["AREAID"] = areaId;
                dr["DFCT_GR_TYPE_CODE"] = dfctGrTypeCode;
                dr["DFCT_CODE"] = dfctCode;
                dr["SHFT_ID"] = shiftID; //2024.07.03 Meilia add SHIFTID
                dtRqst.Rows.Add(dr);

                string bizDirect = string.Empty;
                string bizRework = string.Empty;

                switch (popupType)
                {
                    case "CHARGE1ST":
                        bizDirect = "DA_SEL_PERF_FORMATION_DIRECT_EQPT";
                        bizRework = "DA_SEL_PERF_FORMATION_DIRECT_REWORK_EQPT";
                        break;
                    case "CHARGE2ND":
                        bizDirect = "DA_SEL_PERF_FORMATION_DIRECT_SECOND_EQPT";
                        bizRework = "DA_SEL_PERF_FORMATION_DIRECT_REWORK_SECOND_EQPT";
                        break;
                    case "LOWVOLT":
                        bizDirect = "DA_SEL_PERF_LOWVOLT_EQPT";
                        bizRework = "DA_SEL_PERF_LOWVOLT_REWORK_EQPT";
                        break;
                    case "DEGAS":
                        bizDirect = "DA_SEL_WORKSHEET_EQPT";
                        bizRework = "DA_SEL_WORKSHEET_REWORK_EQPT";
                        break;
                    case "EOL":
                        bizDirect = "DA_SEL_WORKSHEET_EOL_EQPT";
                        bizRework = "DA_SEL_WORKSHEET_EOL_REWORK_EQPT";

                        dgDefectCellList.Columns["EOL_INPUT_DT"].Visibility = Visibility;           //EOL 일경우 투입시간 컬럼 SHOW
                        break;
                    default:
                        return;
                }

                switch (workTypeCode)
                {
                    case "0":
                        dgDefectCellList.ExecuteService(bizDirect, "RQSTDT", "RSLTDT", dtRqst, false, true, dtRqst);
                        break;
                    case "1":
                        dgDefectCellList.ExecuteService(bizRework, "RQSTDT", "RSLTDT", dtRqst, false, true, dtRqst);
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

    }
}
