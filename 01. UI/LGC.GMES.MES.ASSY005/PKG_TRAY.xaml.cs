/*************************************************************************************
 Created Date : 2020.11.20
      Creator : 신 광희
   Decription : CNB2동 증설 - Packaging 공정진척 화면 - Tray 디자인 (ASSY0004.PKG_TRAY 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.20  신 광희 : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF;
using System.Data;
using C1.WPF.DataGrid;
using System.Reflection;

namespace LGC.GMES.MES.ASSY005
{
    /// <summary>
    /// PKG_TRAY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PKG_TRAY : UserControl, IWorkArea
    {
        //public static class TRAY_TYPE
        //{
        //    public static readonly string TRAY_TYPE_20 = "20";
        //    public static readonly string TRAY_TYPE_25 = "25";
        //    public static readonly string TRAY_TYPE_50 = "50";

        //    public static readonly string TRAY_TYPE_43 = "43";
        //    public static readonly string TRAY_TYPE_64 = "64";
        //    public static readonly string TRAY_TYPE_66 = "66";
        //    public static readonly string TRAY_TYPE_81 = "81";
        //    public static readonly string TRAY_TYPE_88 = "88";
        //    public static readonly string TRAY_TYPE_108 = "108";
        //    public static readonly string TRAY_TYPE_110 = "110";
        //    public static readonly string TRAY_TYPE_128 = "128";
        //    public static readonly string TRAY_TYPE_132 = "132";
        //    public static readonly string TRAY_TYPE_151 = "151";
        //}

        public static class TRAY_SHAPE
        {
            public static string CELL_TYPE = string.Empty;  // CELL TYPE
            public static int ROW_NUM = 0;  // 총 ROW 수
            public static int COL_NUM = 0;  // 총 COL 수
            public static bool EMPTY_SLOT = false;  // 빈 슬롯 존재 여부
            public static bool ZIGZAG = false;  // COL 별 지그재그 배치 여부
            public static string[] EMPTY_SLOT_LIST = null;  // 빈 슬롯 컬럼 LIST
            public static int MERGE_START_COL_NUM = 0; // 머지 시작 컬럼 넘버
            public static string[] DISPLAY_LIST = null; // Cell 영역에 표시할 Data List
            public static char[] DISP_SEPARATOR; // 표시 영역 구분자
        }

        public static void SetTrayShape(string sCellType, int iRowCnt, int iColCnt,
                                        bool bEmptySlot, bool bZigZag, string[] emptySlotList,
                                        int iMergeStartCol = 0, string[] displayList = null, char[] dispSeparator = null)
        {
            TRAY_SHAPE.CELL_TYPE = sCellType;  // CELL TYPE
            TRAY_SHAPE.ROW_NUM = bZigZag ? iRowCnt * 2 : iRowCnt;  // 총 ROW 수 (zigzag이면 Merge를 위해 2배로 생성)
            TRAY_SHAPE.COL_NUM = iColCnt;  // 총 COL 수
            TRAY_SHAPE.EMPTY_SLOT = bEmptySlot;  // 빈 슬롯 존재 여부
            TRAY_SHAPE.ZIGZAG = bZigZag;  // COL 별 지그재그 배치 여부
            TRAY_SHAPE.EMPTY_SLOT_LIST = emptySlotList;  // 빈 슬롯 번호 LIST
            TRAY_SHAPE.MERGE_START_COL_NUM = iMergeStartCol;    // 머지 시작 컬럼 번호.
            TRAY_SHAPE.DISPLAY_LIST = displayList == null ? new string[] { "CELLID" } : displayList;  // Cell 영역에 표시할 Data List
            TRAY_SHAPE.DISP_SEPARATOR = dispSeparator == null ? new char[] { ',' } : dispSeparator; // 표시 영역 구분자
        }

        public void SetTrayDisplayList(string[] strList)
        {
            TRAY_SHAPE.DISPLAY_LIST = strList == null ? new string[] { "CELLID" } : strList;
        }

        #region Declaration & Constructor  
        public C1Window _Parent;     // Caller 

        public bool bExistLayOutInfo = false;
        private bool bViewAll = false;

        private string _LOT = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _TRAY = string.Empty;
        private string _TRAYQTY = string.Empty;
        private string _OUT_LOT = string.Empty;
        private string _EQPTID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public PKG_TRAY()
        {
            InitializeComponent();
        }

        public PKG_TRAY(string sLot, string sWipSeq, string sTray, string sTrayQty, string sOutLot, string sEqptID)
        {
            InitializeComponent();

            _LOT = sLot;
            _WIPSEQ = sWipSeq;
            _TRAY = sTray;
            _TRAYQTY = sTrayQty;
            _OUT_LOT = sOutLot;
            _EQPTID = sEqptID;

            // Tray Layout 정보 조회.
            GetTrayLayoutInfo();

            #region Tray Layout Setting 주석..
            //switch (_TRAYQTY)
            //{
            //    case "20":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_20, 20, 1, false, false, null);
            //        break;
            //    case "25":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_25, 25, 1, false, false, null);
            //        break;
            //    case "50":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_50, 25, 2, false, false, null);
            //        break;

            //    case "43":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_43, 22, 2, true, true, new string[] { "88", "131" }, iMergeStartCol: 1);
            //        break;
            //    case "64":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_64, 22, 3, true, true, new string[] { "44", "87", "132", "175" }, iMergeStartCol: 2, displayList: new string[] { "LOCATION", "CELLID", "EL"  });
            //        break;
            //    case "66":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_66, 22, 3, false, false, null, displayList: new string[] { "CELLID", "EL" });
            //        break;
            //    case "81": // 모양 특이..
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_81, 23, 5, true, true, new string[] { "6", "8", "20", "22", "34", "36", "52", "54", "66", "68", "80", "82", "92", "99", "101", "111", "113", "115", "127", "129", "137", "144", "146", "158", "160", "172", "174", "184", "191", "193", "203", "205", "207", "219", "221", "229", "236", "238", "250", "252", "264", "266" }, iMergeStartCol: 1, displayList: new string[] { "CELLID", "EL" });
            //        break;
            //    case "88":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_88, 22, 4, false, false, null);
            //        break;
            //    case "108":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_108, 22, 5, true, true, new string[] { "88", "131", "176", "219" }, iMergeStartCol: 1, displayList: new string[] { "CELLID", "EL" });
            //        break;
            //    case "110":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_110, 22, 5, false, false, null);
            //        break;
            //    case "128":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_128, 22, 6, true, false, new string[] { "22", "43", "132", "153" });
            //        break;
            //    case "132":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_132, 22, 6, false, false, null);
            //        break;
            //    case "151":
            //        SetTrayShape(TRAY_TYPE.TRAY_TYPE_151, 22, 7, true, true, new string[] { "88", "131", "176", "219", "264", "307"  }, iMergeStartCol: 1, displayList: new string[] { "CELLID", "EL" });
            //        break;
            //    default:
            //        break;
            //}
            #endregion
        }


        #endregion

        #region Event
        private void UserControl_Initialized(object sender, EventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            InitializeGrid();

            SetCellInfo(true, false, true);

            if (_Parent != null && _Parent.GetType() == typeof(ASSY005_007_CELL_LIST) && (_Parent as ASSY005_007_CELL_LIST).IsShowSlotNo)
            {
                ShowSlotNoColumns(true);
            }

            //폴란드 셀리스트 컬럼 역순적용 라인
            SetColumnReverse();
        }

        private void SetColumnReverse()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EQPTID;
                inTable.Rows.Add(newRow);

                DataTable DA_BAS_SEL_CELL_LIST_COLUMN_REVERSE_EQSGID = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CELL_LIST_COLUMN_REVERSE_EQSGID", "INDATA", "OUTDATA", inTable);

                if (DA_BAS_SEL_CELL_LIST_COLUMN_REVERSE_EQSGID.Rows.Count > 0)
                {
                    int firstIndex = dgCell.Columns.Count - 5;
                    int tmpCount = 0;
                    for (int i = 1; i < dgCell.Columns.Count; i++)
                    {
                        dgCell.Columns[i].DisplayIndex = firstIndex + tmpCount;
                        tmpCount++;
                        if (tmpCount >= 5)
                        {
                            tmpCount = 0;
                            firstIndex -= 5;
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

            finally
            {
                HideParentLoadingIndicator();
            }
        }

        private void dgCell_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    if (e.Cell.Row.Index >= 0 && e.Cell.Column.Index > 0)
                    {
                        if (Util.NVC(e.Cell.Column.Name).IndexOf("_") >= 0 || Util.NVC(e.Cell.Column.Name).IndexOf("NO") >= 0) // Hidden Column 인 경우.
                            return;

                        if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_JUDGE")).Equals("EMPT_SLOT")) // 빈 슬롯인 경우..
                            return;

                        string sCell = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_CELLID")); //dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value == null ? "" : dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, Util.NVC(e.Cell.Column.Name) + "_LOC"));


                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetParentCellInfo(sCell.Equals(sLoc) ? "" : sCell, e.Cell.Row.Index, e.Cell.Column.Index, sLoc);
                    }
                }
            }));
        }

        private void dgCell_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name.Equals("NO"))
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        return;
                    }

                    if (e.Cell.Column.Name.IndexOf("_SLOTNO") > 0)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        string sOrgView = Util.NVC(e.Cell.Column.Name).Replace("_SLOTNO", "");
                        if (dgCell.Columns.Contains(sOrgView + "_JUDGE"))
                        {
                            string sTmpJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sOrgView + "_JUDGE"));

                            if (sTmpJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }

                        return;
                    }

                    if (!dataGrid.Columns.Contains(e.Cell.Column.Name + "_JUDGE"))
                        return;

                    string sJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_JUDGE"));

                    if (sJudge.Equals("SC")) // SC : 특수문자 포함 (Include Special Character) => Cell ID 형식 오류
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NR")) // NR : 읽을 수 없음 (No Read)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("DL")) // DL : 자리수 상이 (Different ID Length)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("ID")) // ID : ID 중복 (ID Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("PD")) // PD : Tray Location 중복 (Position Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("NI")) // NI : 주액량 정보 없음 (No Information)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else if (sJudge.Equals("EMPT_SLOT"))    // Tray 내 빈 슬롯
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5D5D5D"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;

                        // 아무것도 없는 경우에는 기본 Base 폰트 색 변경.
                        if (e.Cell.Column.Name.IndexOf("_") < 0 && dataGrid.Columns.Contains(e.Cell.Column.Name + "_LOC") && !e.Cell.Column.Name.Equals("NO"))
                        {
                            string sLocVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_LOC"));
                            string sCellVal = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name));

                            if (!sLocVal.Equals("EMPT_SLOT") && sCellVal.Trim().Equals(sLocVal)) //sCellVal.Trim().Equals(""))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#B0B0B0"));
                                //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                                e.Cell.Presenter.FontStyle = FontStyles.Italic;
                            }
                            else if (sLocVal.Equals("EMPT_SLOT"))
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                e.Cell.Presenter.FontStyle = FontStyles.Normal;
                            }
                        }
                    }
                }
            }));
        }

        private void dgCell_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        e.Cell.Presenter.FontStyle = FontStyles.Normal;
                    }
                }
            }));
        }
        #endregion

        #region Mehod

        #region [BizRule]
        public DataTable GetTrayCellList()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_TRAY_CELL_LIST();

                DataRow newRow = inTable.NewRow();
                newRow["PROD_LOTID"] = _LOT;
                newRow["OUT_LOTID"] = _OUT_LOT;
                newRow["EQPTID"] = _EQPTID;
                newRow["TRAYID"] = _TRAY;

                inTable.Rows.Add(newRow);

                return new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_LIST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HideParentLoadingIndicator();
            }
        }

        private void GetTrayLayoutInfo()
        {
            try
            {
                ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();

                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CST_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["CST_TYPE_CODE"] = _TRAY.Length > 4 ? _TRAY.Substring(0, 4) : _TRAY;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CST_LAYOUT_INFO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    bExistLayOutInfo = true;

                    int iRowNum = Util.NVC(dtRslt.Rows[0]["ROW_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_NUM"]));
                    int iColNum = Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["CELL_COL_NUM"]));
                    string sRowTypeCode = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]);
                    string[] sEmptySlotList = Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["EMPTY_SLOT_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    int iRowMergeStrtCol = Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]).Equals("") ? 0 : int.Parse(Util.NVC(dtRslt.Rows[0]["ROW_MRG_STRT_COL_NO"]));
                    string[] sDispInfo = Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]).Equals("") ? null : System.Text.RegularExpressions.Regex.Split(Util.NVC(dtRslt.Rows[0]["SCRN_CELL_DISP_INFO_LIST"]), ",").Where(p => !string.IsNullOrEmpty(p)).Select(p => p.Trim()).ToArray();
                    char[] cDispDelimeter = Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).Equals("") ? null : Util.NVC(dtRslt.Rows[0]["SCRN_DISP_DELIMT"]).ToCharArray();
                    bool bEmptySlot = sEmptySlotList != null && sEmptySlotList.Length > 0 ? true : false;
                    bool bZigZag = Util.NVC(dtRslt.Rows[0]["TRAY_ROW_TYPE_CODE"]).Equals("Z") ? true : false;

                    SetTrayShape(Util.NVC(dtRslt.Rows[0]["CST_CELL_QTY"]), iRowNum, iColNum, bEmptySlot, bZigZag, sEmptySlotList, iMergeStartCol: iRowMergeStrtCol, displayList: sDispInfo, dispSeparator: cDispDelimeter);
                }
                else
                {
                    bExistLayOutInfo = false;

                    SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                    // 데이터 오류 [캐리어 레이아웃 기준정보 누락 - PI팀에 데이터 확인 요청]
                    Util.MessageValidation("SFU3630");
                }
            }
            catch (Exception ex)
            {
                bExistLayOutInfo = false;

                SetTrayShape(string.Empty, 0, 0, false, false, null, iMergeStartCol: 0, displayList: null, dispSeparator: null);

                Util.MessageException(ex);
            }
            finally
            {
                HideParentLoadingIndicator();
            }
        }
        #endregion

        #region [Func]
        /// <summary>
        /// 권한 부여 
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowParentLoadingIndicator()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("ShowLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void HideParentLoadingIndicator()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("HideLoadingIndicator");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                for (int i = 0; i < parameterArrys.Length; i++)
                {
                    parameterArrys[i] = null;
                }

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitializeGrid()
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                DataTable dtTemp = new DataTable();

                if (TRAY_SHAPE.ZIGZAG) // zigzag 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 80;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 115;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);


                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // 빈 Cell 정보 Set.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // alternating row color 삭제
                    dgCell.RowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);
                    dgCell.AlternatingRowBackground = new System.Windows.Media.SolidColorBrush(Colors.White);

                    SetZigZagGridInfo();

                    dgCell.MergingCells -= dgCell_MergingCells;
                    dgCell.MergingCells += dgCell_MergingCells;

                    //MergingCells();
                }
                else // 정상 모양
                {
                    int width = 200;

                    if (TRAY_SHAPE.COL_NUM > 6)
                        width = 95;
                    else if (TRAY_SHAPE.COL_NUM > 5)
                        width = 100;
                    else if (TRAY_SHAPE.COL_NUM > 4)
                        width = 125;
                    else if (TRAY_SHAPE.COL_NUM > 3)
                        width = 150;

                    dtTemp.Columns.Add("NO");

                    int ascii = 65; // ascii => "A"

                    for (int i = 0; i < TRAY_SHAPE.COL_NUM; i++)
                    {
                        int iSBN = (ascii + i);

                        string sTmp = Char.ConvertFromUtf32(iSBN);

                        if (!dgCell.Columns.Contains(sTmp + "_SLOTNO"))
                            Util.SetGridColumnText(dgCell, sTmp + "_SLOTNO", null, "NO.", true, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 20, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp))
                            Util.SetGridColumnText(dgCell, sTmp, null, sTmp, false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, width, HorizontalAlignment.Center, Visibility.Visible);
                        if (!dgCell.Columns.Contains(sTmp + "_JUDGE"))
                            Util.SetGridColumnText(dgCell, sTmp + "_JUDGE", null, sTmp + "_JUDGE", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_LOC"))
                            Util.SetGridColumnText(dgCell, sTmp + "_LOC", null, sTmp + "_LOC", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);
                        if (!dgCell.Columns.Contains(sTmp + "_CELLID"))
                            Util.SetGridColumnText(dgCell, sTmp + "_CELLID", null, sTmp + "_CELLID", false, false, false, true, C1.WPF.DataGrid.DataGridLength.Auto, 50, HorizontalAlignment.Center, Visibility.Collapsed);

                        dtTemp.Columns.Add(sTmp + "_SLOTNO");
                        dtTemp.Columns.Add(sTmp);
                        dtTemp.Columns.Add(sTmp + "_JUDGE");
                        dtTemp.Columns.Add(sTmp + "_LOC");
                        dtTemp.Columns.Add(sTmp + "_CELLID");

                        dgCell.Columns[sTmp].MaxWidth = 220;
                    }

                    // Row Add.
                    for (int i = 0; i < TRAY_SHAPE.ROW_NUM; i++)
                    {
                        DataRow dtRow = dtTemp.NewRow();

                        dtRow["NO"] = (i + 1).ToString();

                        dtTemp.Rows.Add(dtRow);
                    }

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();

                    // LOCATION 정보 SET.
                    int iLocIdx = 1;    // 실제 Cell Location Number 변수.
                    int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        for (int i = 0; i < dgCell.Rows.Count; i++)
                        {
                            // 빈 슬롯 번호 확인.
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                            {
                                if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                                {
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_JUDGE"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_JUDGE", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC", "EMPT_SLOT");
                                    if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_CELLID"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_CELLID", "");
                                }
                                else
                                {
                                    if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                                }
                            }
                            else
                            {
                                if (!Util.NVC(dgCell.Columns[j].Name).Equals("NO") &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // Location 정보 설정
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_LOC") >= 0 &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                            {
                                // Cell 에 Location 값 처리.
                                string sOrgView = Util.NVC(dgCell.Columns[j].Name).Replace("_LOC", "");
                                if (dgCell.Columns.Contains(sOrgView))
                                {
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView, iLocIdx.ToString());
                                }

                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, iLocIdx.ToString());
                                iLocIdx++;


                                // Location 정보 Set (View 용)
                                if (dgCell.Columns.Contains(sOrgView + "_SLOTNO") &&
                                    dgCell.Columns.Contains(sOrgView + "_LOC"))
                                {
                                    string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, sOrgView + "_LOC"));

                                    if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, sOrgView + "_SLOTNO", sTmpLocValue);
                                }
                            }
                            else if (Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                            {

                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name)).Equals("EMPT_SLOT"))
                                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                            }

                            // View 컬럼 기준으로 슬롯 넘버링 처리.
                            if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                                iTmpIdx++;

                            //// 25 ROW 넘어가는 경우 ROW HEIGHT 조정
                            //if (dgCell.Rows.Count > 25)
                            //    dgCell.Rows[i].Height = new C1.WPF.DataGrid.DataGridLength(22);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void dgCell_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            try
            {
                C1DataGrid dg = sender as C1DataGrid;

                if (dg == null)
                    return;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dg.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if ((Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 || Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) && !Util.NVC(dg.Columns[idxCol].Name).Equals("NO"))
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                        }
                    }
                    //if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                    //{
                    //    sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                    //    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                    //    {
                    //        iAsciiIdx++;
                    //    }
                    //}

                    for (int idxRow = 0; idxRow < dg.Rows.Count; idxRow++)
                    {
                        // Row Number 설정
                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") >= 0)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                            }
                            else
                            {
                                if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    //iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Cell Merge 처리.
                                if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                                else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                {
                                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                }
                            }
                            else
                            {
                                // View Column Cell Merge 처리.
                                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 ||
                                    Util.NVC(dg.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0) // View 용 slot no 머지 처리.
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                                    {
                                        e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                                    }
                                }
                            }
                        }

                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0)
                        {
                            iTmpIdx++;
                        }

                        dg.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }

                #region [빈슬롯 cell number 변경 전 로직 주석..]
                //C1DataGrid dg = sender as C1DataGrid;

                //if (dg == null)
                //    return;

                //int iRowNum = 1;

                //// LOCATION 정보 SET.
                //int iLocIdx = 1; // 실제 Loc Number
                //int iAsciiIdx = 0; // 실제 Col Index
                //string sTmpColName = "";

                //for (int idxCol = 0; idxCol < dg.Columns.Count; idxCol++)
                //{
                //    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                //    if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") < 0)
                //    {
                //        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                //        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                //            iAsciiIdx++;
                //    }

                //    for (int idxRow = 0; idxRow < dg.Rows.Count; idxRow++)
                //    {
                //        // Row Number 설정
                //        if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("NO") >= 0)
                //        {
                //            DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, "NO", (iRowNum).ToString());

                //            if (idxRow % 2 == 0 && idxRow != dg.Rows.Count - 1)
                //                e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            else
                //                iRowNum = iRowNum + 1;                           
                //        }
                //        else
                //        {
                //            if (!Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, "");


                //            int iTmp = 0;

                //            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                //            {
                //                int.TryParse(TRAY_SHAPE.EMPTY_SLOT_LIST[0].ToString(), out iTmp);
                //            }

                //            if (iTmp % 2 != 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                //            {
                //                // Location Number 설정.
                //                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                //                        !Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                {
                //                    if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                    else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                }

                //                // Cell Merge 처리.
                //                if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //                else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //            }
                //            else
                //            {
                //                // Location Number 설정.
                //                if (Util.NVC(dg.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                //                        !Util.NVC(DataTableConverter.GetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                //                {
                //                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                    {
                //                        DataTableConverter.SetValue(dg.Rows[idxRow].DataItem, dg.Columns[idxCol].Name, iLocIdx.ToString());
                //                        iLocIdx++;
                //                    }
                //                }

                //                // Cell Merge 처리.
                //                if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //                else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //                {
                //                    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //                }
                //            }

                //            //// Cell Merge 처리.
                //            //if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)
                //            //{
                //            //    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            //}
                //            //else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dg.Rows.Count - 1)
                //            //{
                //            //    e.Merge(new DataGridCellsRange(dg.GetCell(idxRow, idxCol), dg.GetCell(idxRow + 1, idxCol)));
                //            //}
                //        }

                //        dg.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                //    }
                //}
                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetZigZagGridInfo()
        {
            try
            {
                if (dgCell == null)
                    return;

                int iRowNum = 1;

                // LOCATION 정보 SET.
                int iLocIdx = 1; // 실제 Loc Number
                int iAsciiIdx = 0; // 실제 Col Index
                int iTmpIdx = 0;    // 빈 슬롯 번호를 포함 한 전체 넘버 변수.
                int iMergIdx = string.IsNullOrEmpty(TRAY_SHAPE.MERGE_START_COL_NUM.ToString()) ? 1 : TRAY_SHAPE.MERGE_START_COL_NUM;   // 0 row 부터 머지 처리할 index 변수.
                string sTmpColName = "";


                for (int idxCol = 0; idxCol < dgCell.Columns.Count; idxCol++)
                {
                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iAsciiIdx++;
                            //iMergIdx++;
                        }
                    }

                    for (int idxRow = 0; idxRow < dgCell.Rows.Count; idxRow++)
                    {
                        // 빈 슬롯 번호 확인하여 Empty Slot 설정.
                        if (TRAY_SHAPE.EMPTY_SLOT_LIST != null)
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))
                            {
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_JUDGE", "EMPT_SLOT");
                                if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC"))
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name) + "_LOC", "EMPT_SLOT");
                            }
                            else
                            {
                                if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                    !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                                {
                                    // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                        )
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                }
                            }
                        }
                        else
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT") &&
                                !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                            {
                                // Location 정보 다음 Cell index 값 Set 후 아래 로직에서 Reset 되는 문제로...
                                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_LOC") >= 0 &&
                                    !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", ""))).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name))) &&
                                    Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).IndexOf("_SLOTNO") < 0
                                   )
                                    DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                            }
                        }


                        // Row Number 설정
                        if (Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            if (TRAY_SHAPE.EMPTY_SLOT_LIST != null && TRAY_SHAPE.EMPTY_SLOT_LIST.Contains(iTmpIdx.ToString()))  // Empty slot
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), "");
                            }
                            else
                            {
                                DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, Util.NVC(dgCell.Columns[idxCol].Name), (iRowNum).ToString());

                                if (idxRow % 2 != 0 && idxRow != dgCell.Rows.Count - 1)
                                {
                                    iRowNum = iRowNum + 1;
                                }
                            }
                        }
                        else
                        {
                            if (iMergIdx % 2 == 0) // zigzag이면서 처음 빈 슬롯이 홀수이면 Merge를 1번 부터...
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 != 0 && idxRow % 2 == 0)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 == 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1)
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                            else
                            {
                                // Location Number 설정.
                                if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_LOC") >= 0 &&
                                        !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals("EMPT_SLOT"))
                                {
                                    if (iAsciiIdx % 2 == 0 && idxRow % 2 == 0)  // A Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }


                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }
                                    else if (iAsciiIdx % 2 != 0 && idxRow % 2 != 0 && idxRow < dgCell.Rows.Count - 1) // B Column
                                    {
                                        DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());

                                        // Cell 에 Location 값 처리.
                                        string sOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");
                                        if (dgCell.Columns.Contains(sOrgView) &&
                                            !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name)).Equals(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sOrgView))))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sOrgView, iLocIdx.ToString());

                                            // Zigzag 이면 다음 Row 도 동일 값 설정
                                            if (TRAY_SHAPE.ZIGZAG)
                                            {
                                                if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                                {
                                                    DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, sOrgView, iLocIdx.ToString());
                                                }
                                            }
                                        }

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > idxRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[idxRow + 1].DataItem, dgCell.Columns[idxCol].Name, iLocIdx.ToString());
                                            }
                                        }

                                        iLocIdx++;
                                    }


                                    // Location 정보 Set (View 용)
                                    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_LOC", "");

                                    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                    {
                                        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", sTmpLocValue);
                                        else
                                            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_SLOTNO", "");
                                    }
                                }
                                //// Location 정보 Set (View 용)
                                //else if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_SLOTNO") >= 0)
                                //{
                                //    string sTmpOrgView = Util.NVC(dgCell.Columns[idxCol].Name).Replace("_SLOTNO", "");

                                //    if (dgCell.Columns.Contains(sTmpOrgView + "_SLOTNO") &&
                                //        dgCell.Columns.Contains(sTmpOrgView + "_LOC"))
                                //    {
                                //        string sTmpLocValue = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[idxRow].DataItem, sTmpOrgView + "_LOC"));

                                //        if (!sTmpLocValue.Equals("EMPT_SLOT"))
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, sTmpLocValue);
                                //        else
                                //            DataTableConverter.SetValue(dgCell.Rows[idxRow].DataItem, dgCell.Columns[idxCol].Name, "");
                                //    }
                                //}
                            }
                        }


                        // View 컬럼 기준으로 슬롯 넘버링 처리.
                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[idxCol].Name).Equals("NO"))
                        {
                            iTmpIdx++;
                        }

                        dgCell.Rows[idxRow].Height = new C1.WPF.DataGrid.DataGridLength(15);
                    }

                    // 실제 View 컬럼인 경우에만 Index 증가. (zigzag 처리를 위함)
                    if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("_") < 0 && Util.NVC(dgCell.Columns[idxCol].Name).IndexOf("NO") < 0)
                    {
                        sTmpColName = Char.ConvertFromUtf32(65 + iAsciiIdx);    // 65 => A

                        if (Util.NVC(dgCell.Columns[idxCol].Name).IndexOf(sTmpColName) < 0)
                        {
                            iMergIdx++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void SetCellInfo(bool bLoad, bool bSameLoc, bool bChgNexRow)
        {
            try
            {
                if (string.IsNullOrEmpty(TRAY_SHAPE.CELL_TYPE))
                    return;

                ShowParentLoadingIndicator();

                int iRow = 0;
                int iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;

                if (!bLoad)
                    GetNextCellPos(out iRow, out iCol, bSameLoc: bSameLoc, bChgNexRow: bChgNexRow);

                ClearCellInfo();

                // Cell List 조회.
                DataTable dtResult = GetTrayCellList();

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (!Util.NVC(dtResult.Rows[i]["LOCATION"]).Equals(""))
                        {
                            int iTmpLoc = 0;
                            if (int.TryParse(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iTmpLoc))
                            {
                                // Grid 내에 해당 Location 좌표 조회
                                int iFndRow, iFndCol;
                                string sViewColName;

                                //FindLocXY(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);
                                FindLocXYByLinq(Util.NVC(dtResult.Rows[i]["LOCATION"]), out iFndRow, out iFndCol, out sViewColName);

                                if (!sViewColName.Equals("") &&
                                    dgCell.Columns.Contains(sViewColName) &&
                                    dgCell.Columns.Contains(sViewColName + "_JUDGE") &&
                                    iFndRow > -1)
                                {
                                    // OK 가 아닌 경우에는 DATA SET 후 화면 색 표시 처리.
                                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("") ||
                                        Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE")).Equals("OK"))
                                    {
                                        string sTmpDisp = ""; // Util.NVC(dtResult.Rows[i]["CELLID"]);


                                        if (TRAY_SHAPE.DISPLAY_LIST != null)
                                        {
                                            for (int iDsp = 0; iDsp < TRAY_SHAPE.DISPLAY_LIST.Length; iDsp++)
                                            {
                                                if (dtResult.Columns.Contains(Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])))
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(dtResult.Rows[i][Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp])]);
                                                }
                                                else
                                                {
                                                    if (sTmpDisp.Equals(""))
                                                        sTmpDisp = Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                    else
                                                        sTmpDisp = sTmpDisp + new string(TRAY_SHAPE.DISP_SEPARATOR) + Util.NVC(TRAY_SHAPE.DISPLAY_LIST[iDsp]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (dtResult.Columns.Contains("CELLID"))
                                                sTmpDisp = Util.NVC(dtResult.Rows[i]["CELLID"]);
                                        }

                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName, sTmpDisp);
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                        DataTableConverter.SetValue(dgCell.Rows[iFndRow].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));

                                        // Zigzag 이면 다음 Row 도 동일 값 설정
                                        if (TRAY_SHAPE.ZIGZAG)
                                        {
                                            if (dgCell.Rows.Count > iFndRow + 1)    // 다음 Row
                                            {
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName, sTmpDisp);
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_JUDGE", Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                                DataTableConverter.SetValue(dgCell.Rows[iFndRow + 1].DataItem, sViewColName + "_CELLID", Util.NVC(dtResult.Rows[i]["CELLID"]));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!bLoad)
                {
                    DataTable dtTemp = DataTableConverter.Convert(dgCell.ItemsSource);

                    dgCell.BeginEdit();
                    dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
                    dgCell.EndEdit();
                }

                if (dgCell.Rows.Count > iRow && dgCell.Columns.Count > iCol && iRow > -1 && iCol > -1)
                {
                    Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                    dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);
                    dgCell.ScrollIntoView(iRow, iCol);

                    if (Util.NVC(dgCell.Columns[iCol].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[iCol].Name).Equals("NO") &&
                        dgCell.Columns.Contains(Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"))
                    {
                        //loadcellpresenter 콜을 위해 itemsouce 다시 set 시 current cell 오류로.. index로 직접 콜 하도록 변경.
                        string sCell = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_CELLID")); //dgCell.GetCell(iRow, iCol).Value == null ? "" : dgCell.GetCell(iRow, iCol).Value.ToString();
                        string sLoc = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iRow].DataItem, Util.NVC(dgCell.Columns[iCol].Name) + "_LOC"));


                        if (sCell.IndexOf(new string(TRAY_SHAPE.DISP_SEPARATOR)) >= 0)
                        {
                            string[] sSplList = sCell.Split(TRAY_SHAPE.DISP_SEPARATOR);

                            if (TRAY_SHAPE.DISPLAY_LIST != null && TRAY_SHAPE.DISPLAY_LIST.Length > 0)
                            {
                                int index = Array.FindIndex(TRAY_SHAPE.DISPLAY_LIST, s => s.Contains("CELLID"));
                                if (index >= 0 && index < sSplList.Length)
                                {
                                    sCell = sSplList[index];
                                }
                                else
                                {
                                    sCell = sSplList[0];
                                }
                            }
                            else
                            {
                                sCell = sSplList[0];
                            }
                        }

                        GetParentCellInfo(sCell.Equals(sLoc) ? "" : sCell, iRow, iCol, sLoc);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HideParentLoadingIndicator();
            }
        }


        public C1DataGrid GetTrayGrdInfo()
        {
            try
            {
                if (dgCell == null)
                    return null;
                else
                    return dgCell;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void GetParentCellInfo(string sCellID, int iRow, int iCol, string sLoc)
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("GetCellInfo");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];
                if (parameterArrys.Length > 0)
                    parameterArrys[0] = sCellID;
                if (parameterArrys.Length > 1)
                    parameterArrys[1] = iRow;
                if (parameterArrys.Length > 2)
                    parameterArrys[2] = iCol;
                if (parameterArrys.Length > 3)
                    parameterArrys[3] = sLoc;

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ParentClearInfo()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("ClearInfo");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetNextCellPos(out int iRow, out int iCol, bool bSameLoc = false, bool bChgNexRow = true)
        {
            if (dgCell.CurrentCell != null)
            {
                if (dgCell.CurrentCell.Row != null)
                {
                    if (bSameLoc)   // 동일 로케이션 
                    {
                        iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index;
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    }
                    else
                    {
                        string sTmpColName = Util.NVC(dgCell.CurrentCell.Column.Name);

                        if (sTmpColName.IndexOf("_") < 0 && !sTmpColName.Equals("NO") && dgCell.Columns.Contains(sTmpColName + "_LOC"))
                        {
                            string sPrvLoc = Util.NVC(DataTableConverter.GetValue(dgCell.CurrentCell.Row.DataItem, sTmpColName + "_LOC"));

                            int iTmp = 0;
                            //int iFndRow, iFndCol;
                            string sViewColName;

                            int.TryParse(sPrvLoc, out iTmp);

                            //FindLocXY((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);
                            FindLocXYByLinq((iTmp + 1).ToString(), out iRow, out iCol, out sViewColName);

                            if (!sViewColName.Equals(""))
                                iCol = dgCell.Columns[sViewColName].Index < 0 ? iCol : dgCell.Columns[sViewColName].Index;

                            // 동일 Row 유지 확인
                            if (!bChgNexRow)
                            {
                                int iTmpPrvRow = 0;
                                int iTmpPrvCol = 0;
                                string sTmpViewColName = "";

                                FindLocXYByLinq((iTmp).ToString(), out iTmpPrvRow, out iTmpPrvCol, out sTmpViewColName);

                                iRow = iTmpPrvRow;
                            }
                        }
                        else
                        {
                            iRow = 0;
                            iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1;
                        }
                    }
                }
                else
                {
                    iRow = 0;

                    if (dgCell.CurrentCell.Column != null)
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    else
                        iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
                }
            }
            else
            {
                iRow = 0;
                iCol = dgCell.Columns.Contains("A") ? dgCell.Columns["A"].Index : 1; ;
            }
        }

        public void ShowHideAllColumns(bool bShowSlotNo)
        {
            if (dgCell == null)
                return;

            if (bViewAll)
                bViewAll = false;
            else
                bViewAll = true;

            for (int i = 0; i < dgCell.Columns.Count; i++)
            {
                if (bViewAll)
                {
                    dgCell.Columns[i].Visibility = Visibility.Visible;
                }
                else
                {
                    if (Util.NVC(dgCell.Columns[i].Name).Length > 2)
                    {
                        if (dgCell.Columns.Contains("NO"))
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                dgCell.Columns["NO"].Visibility = Visibility.Visible;
                            }
                        }

                        if (dgCell.Columns[i].Name.IndexOf("_SLOTNO") >= 0)
                        {
                            if (bShowSlotNo)
                            {
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                            }
                            else
                            {
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            }
                        }
                        else
                        {
                            if (dgCell.Columns[i].Visibility == Visibility.Visible)
                                dgCell.Columns[i].Visibility = Visibility.Collapsed;
                            else if (dgCell.Columns[i].Visibility == Visibility.Collapsed)
                                dgCell.Columns[i].Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        public void ShowSlotNoColumns(bool bShowSlotNo)
        {
            if (dgCell == null)
                return;

            for (int i = 0; i < dgCell.Columns.Count; i++)
            {
                if (Util.NVC(dgCell.Columns[i].Name).IndexOf("_SLOTNO") >= 0)
                {
                    if (bShowSlotNo)
                    {
                        dgCell.Columns[i].Visibility = Visibility.Visible;

                        if (dgCell.Columns.Contains("NO"))
                            dgCell.Columns["NO"].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        dgCell.Columns[i].Visibility = Visibility.Collapsed;

                        if (dgCell.Columns.Contains("NO"))
                            dgCell.Columns["NO"].Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ClearCellInfo()
        {
            try
            {
                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    for (int j = 0; j < dgCell.Columns.Count; j++)
                    {
                        if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_") < 0 && !Util.NVC(dgCell.Columns[j].Name).Equals("NO"))
                        {
                            if (dgCell.Columns.Contains(Util.NVC(dgCell.Columns[j].Name) + "_LOC") &&
                                !Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name) + "_LOC")));
                            else
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_JUDGE") >= 0)  // 판정 컬럼 초기화
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, Util.NVC(dgCell.Columns[j].Name))).Equals("EMPT_SLOT"))
                                DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                        else if (Util.NVC(dgCell.Columns[j].Name).IndexOf("_CELLID") >= 0)  // CELL ID 컬럼 초기화
                        {
                            DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                        }
                    }
                }

                //ParentClearInfo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void FindLocXY(string sFindText, out int iFndRow, out int iFndCol, out string sViewColName)
        {
            iFndRow = -1;
            iFndCol = -1;
            sViewColName = "";

            for (int col = 0; col < dgCell.Columns.Count; col++)
            {
                if (Util.NVC(dgCell.Columns[col].Name).IndexOf("_LOC") < 0) continue;
                for (int row = 0; row < dgCell.Rows.Count; row++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[row].DataItem, Util.NVC(dgCell.Columns[col].Name))).Equals(sFindText))
                    {
                        iFndRow = row;
                        iFndCol = col;

                        sViewColName = Util.NVC(dgCell.Columns[col].Name).Replace("_LOC", "");
                        return;
                    }
                }
            }
        }

        private void FindLocXYByLinq(string sFindText, out int iFndRow, out int iFndCol, out string sViewColName)
        {
            iFndRow = -1;
            iFndCol = -1;
            sViewColName = "";

            DataTable dt = ((DataView)dgCell.ItemsSource).Table;
            DataRow row;

            for (int col = 0; col < dgCell.Columns.Count; col++)
            {
                if (Util.NVC(dgCell.Columns[col].Name).IndexOf("_LOC") < 0) continue;

                row = (from t in dt.AsEnumerable()
                       where (t.Field<string>(Util.NVC(dgCell.Columns[col].Name)) == sFindText)
                       select t).FirstOrDefault();

                if (row != null)
                {
                    //idx = dt.Rows.IndexOf(row) + 1;
                    iFndRow = dt.Rows.IndexOf(row);
                    iFndCol = dgCell.Columns[col].Index;

                    sViewColName = Util.NVC(dgCell.Columns[col].Name).Replace("_LOC", "");
                    return;
                }
            }
        }
        #endregion

        #region [Validation]
        private bool chkReadOnly()
        {
            try
            {
                if (_Parent != null && _Parent.GetType() == typeof(ASSY005_007_CELL_LIST) && (_Parent as ASSY005_007_CELL_LIST).IsOnlyViewMode)
                    return true;
                else
                    return false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion

        #endregion

        private void dgCell_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.D)
                {
                    C1.WPF.DataGrid.C1DataGrid grd = (sender as C1.WPF.DataGrid.C1DataGrid);
                    if (grd != null &&
                        grd.CurrentCell != null &&
                        grd.CurrentCell.Column != null &&
                        !grd.CurrentCell.Column.Name.Equals("NO") &&
                        grd.CurrentCell.Column.Name.IndexOf("_") < 0)
                    {
                        //남경 로직.. OP 사용 로직인지 확인 필요..
                        //DeleteBtnCall();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteBtnCall()
        {
            if (_Parent == null)
                return;

            try
            {
                if (chkReadOnly()) return;

                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("DeleteBtnCall");
                ParameterInfo[] parameters = methodInfo.GetParameters();

                object[] parameterArrys = new object[parameters.Length];

                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}


