/*************************************************************************************
 Created Date : 2023.02.23
      Creator : 홍석원
   Decription : 작업 중 Lot 불량 현황 메뉴 불량 클릭 시 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.02.23  홍석원 : Initial Created.
  2023.03.13  조영대 : 생산실적 레포트와 최대한 같게 하기 위하여 비즈룰에서 직행과 재작업을 따로 만들어 머지함.
  2023.03.30  조영대 : 라인 ALL 추가, LOT 유형 조건 추가
  2024.07.03  Meilia : E20240613-001554 Improvement in the number and list of defect cells(GDC)
**************************************************************************************/


using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Controls;


namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// FCS001_152_DEFECT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_152_DEFECT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string popupType = string.Empty;
        private string calDateFrom = string.Empty;
        private string calDateTo = string.Empty;
        private string eqsgId = string.Empty;
        private string lotType = string.Empty;
        private string eqptId = string.Empty;
        private bool checkDirect = false;
        private bool checkRework = false;
        private string shiftID = string.Empty; // 2024.07.03 Meilia Add ShiftID

        string bizDirect = "DA_SEL_PROD_DFCT_SUMMARY_BY_EQPT";
        string bizRework = string.Empty;
        string sWRKLOG_TYPE_CODE = string.Empty;

        public FCS001_152_DEFECT()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
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
            checkDirect = (bool)parameters[6];
            checkRework = (bool)parameters[7];
            shiftID = Util.NVC(parameters[8]).Equals(string.Empty) ? null : Util.NVC(parameters[8]); // 2024.07.03 Meilia add ShiftID

            switch (popupType)
            {
                case "CHARGE1ST":
                    sWRKLOG_TYPE_CODE = "A";
                    break;

                case "CHARGE2ND":
                    sWRKLOG_TYPE_CODE = "B";
                    break;

                case "LOWVOLT":
                    sWRKLOG_TYPE_CODE = "G";
                    break;

                case "DEGAS":
                    sWRKLOG_TYPE_CODE = "D";
                    break;

                case "EOL":
                    sWRKLOG_TYPE_CODE = "Q";
                    break;

                default:
                    return;
            }

            GetList();
        }

        private void dgDefect_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null) return;

                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.ToString() == "SUBLOT_QTY")
                        {
                            e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Blue;
                        }
                        else
                        {
                            e.Cell.Presenter.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgDefect.GetCellFromPoint(pnt);

                if (cell == null) return;
                if (Convert.ToInt32(cell.Value) <= 0) return;

                if (cell.Column.Name == "SUBLOT_QTY")
                {
                    string areaId = Util.NVC(dgDefect.GetValue(cell.Row.Index, "AREAID"));
                    string dfctGrTypeCode = Util.NVC(dgDefect.GetValue(cell.Row.Index, "DFCT_GR_TYPE_CODE"));
                    string dfctCode = Util.NVC(dgDefect.GetValue(cell.Row.Index, "DFCT_CODE"));
                    string workTypeCode = string.Empty;

                    if (Util.NVC(dgDefect.GetValue(cell.Row.Index, "WORK_TYPE_CODE")).Equals("1"))
                        workTypeCode = "RWK";
                    else
                        workTypeCode = "FRST";

                    ShowCellListPopup(areaId, dfctGrTypeCode, dfctCode, workTypeCode);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgDefect_ExecuteDataModify(object sender, UcBaseDataGrid.ExecuteDataEventArgs e)
        {
            if (checkDirect && checkRework)
            {
                //조회 후 쓸데없이 한번 더 조회하여 MERGE 해서 데이터 중복 출력되어 제거
                //DataTable dtRqst = e.Arguments as DataTable;
                //DataTable dtRework = new ClientProxy().ExecuteServiceSync(bizDirect, "RQSTDT", "RSLTDT", dtRqst);

                //DataTable dtResult = e.ResultData as DataTable;
                //dtResult.Merge(dtRework, true, MissingSchemaAction.Ignore);
                //dtRework.AcceptChanges();
            }
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
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("LOTTYPE", typeof(string));
                dtRqst.Columns.Add("WRKLOG_TYPE_CODE", typeof(string));
                dtRqst.Columns.Add("WRK_TYPE", typeof(string)); //오타 수정
                dtRqst.Columns.Add("EQPTID", typeof(string));
                //dtRqst.Columns.Add("SHFT_ID", typeof(string)); //2024.07.03 Meilia Add ShiftID 소스 반영 미적용

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = calDateFrom;
                dr["TO_DATE"] = calDateTo;
                dr["EQSGID"] = eqsgId;
                dr["LOTTYPE"] = lotType;
                dr["WRKLOG_TYPE_CODE"] = sWRKLOG_TYPE_CODE;
                //dr["EQPTID"] = Util.NVC(eqptId).Equals("NO_EQP") ? null : eqptId;
                dr["EQPTID"] = eqptId;
                //dr["SHFT_ID"] = shiftID; //2024.06.20 Meilia add SHIFTID 소스 반영 미적용
                dtRqst.Rows.Add(dr);

                bizDirect = "DA_SEL_PROD_DFCT_SUMMARY_BY_EQPT";

                if (checkDirect && checkRework)
                {
                    //dr["WRK_TYPE"] = "FRST"; //오타 수정, 주석 처리
                    dgDefect.ExecuteService(bizDirect, "RQSTDT", "RSLTDT", dtRqst, false, true, dtRqst); 

                }
                else if (checkDirect)
                {
                    dr["WRK_TYPE"] = "FRST"; //오타 수정
                    dgDefect.ExecuteService(bizDirect, "RQSTDT", "RSLTDT", dtRqst, false, true, dtRqst);
                }
                else if (checkRework)
                {
                    dr["WRK_TYPE"] = "RWK"; //오타 수정
                    dgDefect.ExecuteService(bizDirect, "RQSTDT", "RSLTDT", dtRqst, false, true, dtRqst);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void ShowCellListPopup(string areaId, string dfctGrTypeCode, string dfctCode, string workTypeCode)
        {
            FCS001_054_DFCT_CELL_LIST cellListPopup = new FCS001_054_DFCT_CELL_LIST();
            cellListPopup.FrameOperation = FrameOperation;

            if (cellListPopup != null)
            {
                object[] Parameters = new object[16]; 
                // 2024.07.03 Meilia add ShiftID
                //Parameters[0] = popupType;
                //Parameters[1] = calDateFrom;
                //Parameters[2] = calDateTo;
                //Parameters[3] = eqsgId;
                //Parameters[4] = lotType;
                //Parameters[5] = eqptId;
                //Parameters[6] = areaId;
                //Parameters[7] = dfctGrTypeCode;
                //Parameters[8] = dfctCode;
                //Parameters[9] = workTypeCode;
                //Parameters[10] = shiftID; // 2024.07.03 Meilia add ShiftID
                //Parameters[0] = calDateFrom; //WORK_DATE
                Parameters[1] = shiftID; //SHFT_ID
                Parameters[3] = eqsgId; //EQSGID
                Parameters[4] = eqptId; //EQPTID
                Parameters[6] = lotType; // LOTTYPE
                Parameters[7] = dfctCode; // DFCT_CODE
                Parameters[8] = sWRKLOG_TYPE_CODE;
                Parameters[9] = workTypeCode; //"FRST"; //0 or 1

                Parameters[12] = calDateFrom;
                Parameters[13] = calDateTo;

                //if (checkDirect && checkRework)
                //{
                //    //Parameters[9] = "FRST";
                //}
                //else if (checkDirect)
                //{
                //    Parameters[9] = "FRST";
                //}
                //else if (checkRework)
                //{
                //    Parameters[9] = "RWK";
                //}

                C1WindowExtension.SetParameters(cellListPopup, Parameters);

                //this.Dispatcher.BeginInvoke(new Action(() => { cellListPopup.ShowModal(); }));

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => cellListPopup.ShowModal()));
            }
        }

        #endregion

    }
}
