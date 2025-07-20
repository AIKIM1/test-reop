/*************************************************************************************
 Created Date : 2016.09.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 패키지 공정 Tray Cell 관리 화면의 25 Cell Tray 
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.22  INS 김동일K : Initial Created.
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF;
using System.Data;
using C1.WPF.DataGrid;
using System.Reflection;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// PKG_TRAY_25.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PKG_TRAY_25 : UserControl, IWorkArea
    {
        #region Declaration & Constructor  
        public C1Window _Parent;     // Caller 
        
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

        public PKG_TRAY_25()
        {
            InitializeComponent();
        }

        public PKG_TRAY_25(string sLot, string sWipSeq, string sTray, string sTrayQty, string sOutLot, string sEqptID)
        {
            InitializeComponent();

            _LOT = sLot;
            _WIPSEQ = sWipSeq;
            _TRAY = sTray;
            _TRAYQTY = sTrayQty;
            _OUT_LOT = sOutLot;
            _EQPTID = sEqptID;
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

            SetCellInfo(true, false);
        }

        private void dgCell_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                        string sCell = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value == null ? "" : dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Value.ToString();
                        GetParentCellInfo(sCell, e.Cell.Row.Index, e.Cell.Column.Index);
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

                        return;
                    }

                    string sJudge = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, e.Cell.Column.Name + "_JUDGE"));

                    if (sJudge.Equals("SC")) // SC : 특수문자 포함 (Include Special Character)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#9253EB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (sJudge.Equals("NR")) // NR : 읽을 수 없음 (No Read)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else if (sJudge.Equals("DL")) // DL : 자리수 상이 (Different ID Length)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D941C5"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (sJudge.Equals("ID")) // ID : ID 중복 (ID Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#86E57F"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else if (sJudge.Equals("PD")) // PD : Tray Location 중복 (Position Duplication)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFA500"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                    }
                    else if (sJudge.Equals("NI")) // NI : 주액량 정보 없음 (No Information)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFF612"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
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
                HiddenParentLoadingIndicator();
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

        private void HiddenParentLoadingIndicator()
        {
            if (_Parent == null)
                return;

            try
            {
                Type type = _Parent.GetType();
                MethodInfo methodInfo = type.GetMethod("HiddenLoadingIndicator");
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
            if (_TRAYQTY.Equals(""))
                return;

            DataTable dtTemp = new DataTable();

            if (_TRAYQTY.Equals("25") || _TRAYQTY.Equals("20"))
            {
                if (!dgCell.Columns.Contains("A"))
                    Util.SetGridColumnText(dgCell, "A", null, "A", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Visible);
                if (!dgCell.Columns.Contains("A_JUDGE"))
                    Util.SetGridColumnText(dgCell, "A_JUDGE", null, "A_JUDGE", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Collapsed);

                dtTemp.Columns.Add("NO");
                dtTemp.Columns.Add("A");
                dtTemp.Columns.Add("A_JUDGE");
            }
            else if (_TRAYQTY.Equals("50"))
            {
                if (!dgCell.Columns.Contains("A"))
                    Util.SetGridColumnText(dgCell, "A", null, "A", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Visible);
                if (!dgCell.Columns.Contains("B"))
                    Util.SetGridColumnText(dgCell, "B", null, "B", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Visible);

                if (!dgCell.Columns.Contains("A_JUDGE"))
                    Util.SetGridColumnText(dgCell, "A_JUDGE", null, "A_JUDGE", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Collapsed);
                if (!dgCell.Columns.Contains("B_JUDGE"))
                    Util.SetGridColumnText(dgCell, "B_JUDGE", null, "B_JUDGE", false, false, false, true, 200, HorizontalAlignment.Center, Visibility.Collapsed);

                dtTemp.Columns.Add("NO");
                dtTemp.Columns.Add("A");
                dtTemp.Columns.Add("B");

                dtTemp.Columns.Add("A_JUDGE");
                dtTemp.Columns.Add("B_JUDGE");
            }

            // 빈 Cell 정보 Set.
            int iTotRow = 25;

            if (_TRAYQTY.Equals("20"))
                iTotRow = 20;

            for (int i = 0; i < iTotRow; i++)
            {
                DataRow dtRow = dtTemp.NewRow();
                dtRow["NO"] = (i + 1).ToString();
                if (dtTemp.Columns.Contains("A"))
                    dtRow["A"] = "";
                if (dtTemp.Columns.Contains("B"))
                    dtRow["B"] = "";
                if (dtTemp.Columns.Contains("A_JUDGE"))
                    dtRow["A_JUDGE"] = "";
                if (dtTemp.Columns.Contains("B_JUDGE"))
                    dtRow["B_JUDGE"] = "";
                dtTemp.Rows.Add(dtRow);
            }

            dgCell.BeginEdit();
            dgCell.ItemsSource = DataTableConverter.Convert(dtTemp);
            dgCell.EndEdit();
        }

        public void SetCellInfo(bool bLoad, bool bSameLoc)
        {
            try
            {
                ShowParentLoadingIndicator();

                int iRow = 0;
                int iCol = 1;

                if (!bLoad)
                    GetNextCellPos(out iRow, out iCol, bSameLoc);

                //Util.gridClear(dgCell);

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
                                string sTmpCol = "A";
                                string sTmpCol_Judge = "A_JUDGE";

                                if (iTmpLoc > 25)
                                {
                                    if (dgCell.Columns.Count > 2)
                                    {
                                        sTmpCol = "B";
                                        sTmpCol_Judge = "B_JUDGE";
                                    }

                                    if (dgCell.Columns.Contains(sTmpCol) && dgCell.Columns.Contains(sTmpCol_Judge))
                                    {
                                        // OK 가 아닌 경우에는 DATA SET 후 화면 색 표시 처리.
                                        if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iTmpLoc - 26].DataItem, sTmpCol_Judge)).Equals("") ||
                                            Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iTmpLoc - 26].DataItem, sTmpCol_Judge)).Equals("OK"))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[iTmpLoc - 26].DataItem, sTmpCol, Util.NVC(dtResult.Rows[i]["CELLID"]));
                                            DataTableConverter.SetValue(dgCell.Rows[iTmpLoc - 26].DataItem, sTmpCol_Judge, Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                            //dtTemp.Rows[iTmpLoc - 26].BeginEdit();
                                            //dtTemp.Rows[iTmpLoc - 26][sTmpCol] = Util.NVC(dtResult.Rows[i]["CELLID"]);
                                            //dtTemp.Rows[iTmpLoc - 26][sTmpCol_Judge] = Util.NVC(dtResult.Rows[i]["JUDGE"]);
                                            //dtTemp.Rows[iTmpLoc - 26].EndEdit();
                                        }
                                    }
                                }
                                else
                                {
                                    if (dgCell.Columns.Contains(sTmpCol) && dgCell.Columns.Contains(sTmpCol_Judge))
                                    {
                                        // OK 가 아닌 경우에는 DATA SET 후 화면 색 표시 처리.
                                        if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iTmpLoc - 1].DataItem, sTmpCol_Judge)).Equals("") ||
                                            Util.NVC(DataTableConverter.GetValue(dgCell.Rows[iTmpLoc - 1].DataItem, sTmpCol_Judge)).Equals("OK"))
                                        {
                                            DataTableConverter.SetValue(dgCell.Rows[iTmpLoc - 1].DataItem, sTmpCol, Util.NVC(dtResult.Rows[i]["CELLID"]));
                                            DataTableConverter.SetValue(dgCell.Rows[iTmpLoc - 1].DataItem, sTmpCol_Judge, Util.NVC(dtResult.Rows[i]["JUDGE"]));
                                            //dtTemp.Rows[iTmpLoc - 1].BeginEdit();
                                            //dtTemp.Rows[iTmpLoc - 1][sTmpCol] = Util.NVC(dtResult.Rows[i]["CELLID"]);
                                            //dtTemp.Rows[iTmpLoc - 1][sTmpCol_Judge] = Util.NVC(dtResult.Rows[i]["JUDGE"]);
                                            //dtTemp.Rows[iTmpLoc - 1].EndEdit();
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

                if (dgCell.Rows.Count > iRow && dgCell.Columns.Count > iCol)
                {
                    Util.SetDataGridCurrentCell(dgCell, dgCell[iRow, iCol]);

                    dgCell.CurrentCell = dgCell.GetCell(iRow, iCol);


                    //loadcellpresenter 콜을 위해 itemsouce 다시 set 시 current cell 오류로.. index로 직접 콜 하도록 변경.
                    string sCell = dgCell.GetCell(iRow, iCol).Value == null ? "" : dgCell.GetCell(iRow, iCol).Value.ToString();
                    GetParentCellInfo(sCell, iRow, iCol);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenParentLoadingIndicator();
            }            
        }

        private void ClearCellInfo()
        {
            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                for (int j = 1; j < dgCell.Columns.Count; j++)
                {
                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, dgCell.Columns[j].Name, "");
                }
            }
        }

        private void GetParentCellInfo(string sCellID, int iRow, int iCol)
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
                
                methodInfo.Invoke(_Parent, parameters.Length == 0 ? null : parameterArrys);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetNextCellPos(out int iRow, out int iCol, bool bSameLoc = false)
        {
            if (dgCell.CurrentCell != null)
            {
                if (dgCell.CurrentCell.Row != null)
                {
                    if (dgCell.CurrentCell.Row.Index < 24)
                    {
                        if (bSameLoc)   // 동일 로케이션 
                            iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index;
                        else
                            iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index + 1;

                        if (dgCell.CurrentCell.Column != null)
                        {
                            iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                        }
                        else
                        {
                            iCol = 1;
                        }
                    }
                    else
                    {                        
                        if (dgCell.CurrentCell.Column != null)
                        {
                            int iMinus = 1 + dgCell.Columns.Count / 2; // Number 컬럼, 판정 컬럼
                            if (dgCell.Columns.Count - iMinus > dgCell.CurrentCell.Column.Index)
                            {
                                int iTmpCol;

                                if (bSameLoc)   // 동일 로케이션 
                                    iTmpCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index + 1;
                                else
                                    iTmpCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;

                                if (dgCell.Columns.Count > iTmpCol && dgCell.Columns[iTmpCol].Name.EndsWith("_JUDGE"))
                                {
                                    iRow = dgCell.CurrentCell.Row.Index < 0 ? 0 : dgCell.CurrentCell.Row.Index;
                                    iCol = iTmpCol - 1;                                    
                                }
                                else
                                {
                                    iRow = 0;
                                    iCol = iTmpCol;
                                }
                            }
                            else
                            {
                                iRow = 0;
                                iCol = 1;
                            }
                        }
                        else
                        {
                            iRow = 0;
                            iCol = 1;
                        }
                    }

                }
                else
                {
                    iRow = 0;

                    if (dgCell.CurrentCell.Column != null)
                        iCol = dgCell.CurrentCell.Column.Index < 0 ? 0 : dgCell.CurrentCell.Column.Index;
                    else
                        iCol = 1;
                }
            }
            else
            {
                iRow = 0;
                iCol = 1;
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

        #endregion

        #endregion
        
    }
}
