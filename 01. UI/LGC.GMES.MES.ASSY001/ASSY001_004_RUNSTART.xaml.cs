/*************************************************************************************
 Created Date : 2016.09.03
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Lamination 공정진척 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.03  INS 김동일K : Initial Created.
  
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
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.ComponentModel;
using System.Threading;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_004_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_004_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoID = string.Empty;
        private string _WoDetail = string.Empty;
        private bool bSave = false;

        private string EqptElcType = "";

        private string _Max_Pre_Proc_End_Day = string.Empty;
        private DateTime _Min_Valid_Date;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_004_RUNSTART()
        {
            InitializeComponent();
        }        
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
            }

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            GetEqptInfo();            
            GetInputMountInfo();

            // 선입선출 기준일 조회.
            //GetProcMtrlInputRule();

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            //GetEqptInfo();
            //InitCombo();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgList.CurrentRow == null || dgList.CurrentRow.DataItem == null || dgList.CurrentRow.Index < 0)
                return;

                        
            string sRet = string.Empty;
            string sMsg = string.Empty;
            
            SetInput(Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID")),
                        //sLoc,
                        Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "WIPQTY")),
                        Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_CODE")),
                        Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_NAME")),
                        "PROD");            
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
            if (iRow < 0)
                return;
            
            GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "INPUT_MTRL_CLSS_CODE")));
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("Pancake을 선택 하세요.");                
                    Util.MessageValidation("SFU1415");
                    return;
                }

                string sRet = string.Empty;
                string sMsg = string.Empty;
                
                SetInput(Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "LOTID")),
                            //sLoc,
                            Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "WIPQTY")),
                            Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRDT_CLSS_CODE")),
                            Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PRDT_CLSS_NAME")),
                            "PROD");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (!CanRunStart())
                return;

            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("작업시작 하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string sNewLot = GetNewLotId();
                    if (sNewLot.Equals(""))
                        return;
                    txtLOT.Text = sNewLot;
                    RunStart(sNewLot);
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtMTRL_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
                if (iRow < 0)
                {
                    //Util.Alert("자재 투입위치를 선택하세요.");
                    Util.MessageValidation("SFU1820");
                    return;
                }

                SetInput(Util.NVC(txtMTRL.Text.Trim()),
                            //Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_ID")),
                            "",
                            "",
                            "",
                            "MTRL");                

                txtMTRL.Text = "";
            }
        }

        private void dgInput_CurrentCellChanged(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }

                                    // 선택한 위치에 투입 가능 자재 조회
                                    if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                                        GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "INPUT_MTRL_CLSS_CODE")));
                                    else
                                        Util.gridClear(dgList);
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;

                                    Util.gridClear(dgList);
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }

                                // 선택한 위치에 투입 가능 자재 조회
                                if (Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                                    GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "INPUT_MTRL_CLSS_CODE")));
                                else
                                    Util.gridClear(dgList);
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;

                                Util.gridClear(dgList);
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void dgList_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                C1DataGrid dg = sender as C1DataGrid;
                if (e.Cell != null &&
                    e.Cell.Presenter != null &&
                    e.Cell.Presenter.Content != null)
                {
                    CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                    if (chk != null)
                    {
                        switch (Convert.ToString(e.Cell.Column.Name))
                        {
                            case "CHK":
                                if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                    chk.IsChecked = true;

                                    for (int idx = 0; idx < dg.Rows.Count; idx++)
                                    {
                                        if (e.Cell.Row.Index != idx)
                                        {
                                            if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
                                                dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
                                            {
                                                (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
                                            }
                                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                        }
                                    }
                                }
                                else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
                                         (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                         (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
                                {
                                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                    chk.IsChecked = false;
                                }
                                break;
                        }
                    }
                    else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
                    {
                        if (!dg.Columns.Contains("CHK"))
                            return;

                        CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

                        if (chk2 != null)
                        {
                            if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                   dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                   (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                   !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
                                chk2.IsChecked = true;

                                for (int idx = 0; idx < dg.Rows.Count; idx++)
                                {
                                    if (e.Cell.Row.Index != idx)
                                    {
                                        if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
                                            dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
                                        {
                                            (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
                                        }
                                        DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
                                    }
                                }
                            }
                            else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
                                     dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
                                     (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
                                     (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
                            {
                                DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
                                chk2.IsChecked = false;
                            }
                        }
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }));
        }

        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals("") || txtPancake.Text.Trim().Length > 0)
                            {
                                e.Cell.Presenter.Background = null;
                                //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFFFF"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                    //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                            //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void txtPancake_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    btnSearch_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]
        private void GetEqptInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO();

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_ELTYPE_IFNO", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    txtWorkorder.Text = Util.NVC(dtRslt.Rows[0]["WOID"]);
                    txtWODetail.Text = Util.NVC(dtRslt.Rows[0]["WO_DETL_ID"]);

                    EqptElcType = Util.NVC(dtRslt.Rows[0]["PRDT_CLSS_CODE"]);
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

        private void GetInputMountInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUT_POS_INFO();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_MOUNT_INFO_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        dgInput.CurrentCellChanged -= dgInput_CurrentCellChanged;
                        //dgInput.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInput, searchResult, null, true);

                        dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;

                        //// 투입위치 유형이 PROD 인대 INPUT_MTRL_CLSS_CODE 가 없는 경우 DISABLE 처리.
                        //for (int i = 0; i < dgInput.Rows.Count - dgInput.Rows.BottomRows.Count; i++)
                        //{
                        //    if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD"))
                        //    {
                        //        if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_MTRL_CLSS_CODE")).Equals(""))
                        //        {
                        //            dgInput.Rows[i].IsSelectable = false; //.GetCell(i, 0)..IsEditable = false; // dgInput.Rows[i]["CHK"].IsEditable = false;
                        //        }
                        //    }
                        //}

                        if (dgInput.CurrentCell != null)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.CurrentCell.Row.Index, dgInput.Columns.Count - 1);
                        else if (dgInput.Rows.Count > 0)
                            dgInput.CurrentCell = dgInput.GetCell(dgInput.Rows.Count, dgInput.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        //private void GetWaitLotList_Center(string sElec)
        //{
        //    try
        //    {
        //        ShowLoadingIndicator();

        //        DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

        //        DataRow newRow = inTable.NewRow();
        //        newRow["LANGID"] = LoginInfo.LANGID;
        //        newRow["EQSGID"] = _LineID;
        //        newRow["EQPTID"] = _EqptID;
        //        newRow["PROCID"] = Process.LAMINATION;
        //        newRow["WOID"] = txtWorkorder.Text;
        //        //newRow["POSITION"] = sPos;
        //        //newRow["PRDT_CLSS_CODE"] = sElec; -- 설비 조건으로 PROD_CLASS_CODE 조회 함..
        //        newRow["IN_LOTID"] = txtPancake.Text;

        //        inTable.Rows.Add(newRow);

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
        //        {
        //            try
        //            {
        //                if (searchException != null)
        //                {
        //                    Util.MessageException(searchException);
        //                    return;
        //                }

        //                dgList.ItemsSource = DataTableConverter.Convert(searchResult);

        //                if (dgList.CurrentCell != null)
        //                    dgList.CurrentCell = dgList.GetCell(dgList.CurrentCell.Row.Index, dgList.Columns.Count - 1);
        //                else if (dgList.Rows.Count > 0)
        //                    dgList.CurrentCell = dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1);
        //            }
        //            catch (Exception ex)
        //            {
        //                Util.MessageException(ex);
        //            }
        //            finally
        //            {
        //                HiddenLoadingIndicator();
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}

        //private void GetWaitLotList_UL()
        //{
        //    try
        //    {
        //        ShowLoadingIndicator();

        //        DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

        //        DataRow newRow = inTable.NewRow();
        //        newRow["LANGID"] = LoginInfo.LANGID;
        //        newRow["EQSGID"] = _LineID;
        //        newRow["EQPTID"] = _EqptID;
        //        newRow["PROCID"] = Process.LAMINATION;
        //        newRow["WOID"] = txtWorkorder.Text;
        //        newRow["IN_LOTID"] = txtPancake.Text;

        //        inTable.Rows.Add(newRow);

        //        new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_UL_LM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
        //        {
        //            try
        //            {
        //                if (searchException != null)
        //                {
        //                    Util.MessageException(searchException);
        //                    return;
        //                }

        //                dgList.ItemsSource = DataTableConverter.Convert(searchResult);

        //                if (dgList.CurrentCell != null)
        //                    dgList.CurrentCell = dgList.GetCell(dgList.CurrentCell.Row.Index, dgList.Columns.Count - 1);
        //                else if (dgList.Rows.Count > 0)
        //                    dgList.CurrentCell = dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1);
        //            }
        //            catch (Exception ex)
        //            {
        //                Util.MessageException(ex);
        //            }
        //            finally
        //            {
        //                HiddenLoadingIndicator();
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        HiddenLoadingIndicator();
        //        Util.MessageException(ex);
        //    }
        //}


        private void GetWaitLotList_ByMtrlClssCode(string sMtrlClssCode)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_LM();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQSGID"] = _LineID;
                newRow["EQPTID"] = _EqptID;
                newRow["PROCID"] = Process.LAMINATION;
                newRow["WOID"] = txtWorkorder.Text;
                newRow["IN_LOTID"] = txtPancake.Text;
                newRow["INPUT_MTRL_CLSS_CODE"] = sMtrlClssCode;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_LM_BY_LV3_CODE", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }

                        dgList.CurrentCellChanged -= dgList_CurrentCellChanged;
                        //dgList.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgList, searchResult, null, false);
                        dgList.CurrentCellChanged += dgList_CurrentCellChanged;

                        if (dgList.CurrentCell != null)
                            dgList.CurrentCell = dgList.GetCell(dgList.CurrentCell.Row.Index, dgList.Columns.Count - 1);
                        else if (dgList.Rows.Count > 0 && dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1) != null)
                            dgList.CurrentCell = dgList.GetCell(dgList.Rows.Count, dgList.Columns.Count - 1);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetPancakeValid(string sLot, string sLoc, out string sRet, out string sMsg)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_SEL_IN_LOT_VALID_LM();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;                
                newRow["PROD_LOTID"] = sLot;
                newRow["USERID"] = LoginInfo.USERID;
                
                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];
                newRow = inTable.NewRow();
                newRow["EQPT_MOUNT_PSTN_ID"] = sLoc;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                newRow["INPUT_LOTID"] = sLot;                

                inTable.Rows.Add(newRow);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_LM", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sRet = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["REQCODE"]);
                    sMsg = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["REQMSG"]);
                }
                else
                {
                    sRet = "NG";
                    sMsg = "SFU2881";// "존재하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                sRet = "NG";
                sMsg = ex.Message;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetNewLotId()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = _Biz.GetBR_PRD_GET_NEW_LOT_LM();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;                
                //newRow["NEXTDAY"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;
                
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_LM", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sNewLot =  Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);
                }

                return sNewLot;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void RunStart(string sNewLot)
        {
            try
            {
                ShowLoadingIndicator();
                
                dgInput.EndEdit();

                DataSet indataSet = _Biz.GetBR_PRD_REG_LOTSTART_LM();

                DataTable inTable = indataSet.Tables["IN_EQP"];
                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["WO_DETL_ID"] = null;
                newRow["PROD_LOTID"] = sNewLot;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inInputTable = indataSet.Tables["IN_INPUT"];

                for (int i = 0; i < dgInput.Rows.Count; i++)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        newRow = inInputTable.NewRow();
                        newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                        newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                        newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID"));

                        inInputTable.Rows.Add(newRow);
                    }
                    else
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID")).Equals(""))
                        {
                            newRow = inInputTable.NewRow();
                            newRow["EQPT_MOUNT_PSTN_ID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_ID"));
                            newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                            newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID"));

                            inInputTable.Rows.Add(newRow);
                        }
                    }
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_LOT_LM", "IN_EQP,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        lbMsg.Text = MessageDic.Instance.GetMessage("SFU1275"); // 정상 처리 되었습니다.

                        dgInput.IsReadOnly = true;
                        dgList.IsReadOnly = true;
                        btnOK.IsEnabled = false;

                        //Util.AlertInfo("정상 처리 되었습니다.");

                        bSave = true;

                        tbSplash.Text = MessageDic.Instance.GetMessage("SFU3044", sNewLot); // [%1] LOT이 생성 되었습니다.

                        grdMsg.Visibility = Visibility.Visible;

                        AsynchronousClose();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetProcMtrlInputRule()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = Process.LAMINATION;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MTRL_INPUT_RULE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _Max_Pre_Proc_End_Day = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                //HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]
        private bool CanSearch()
        {
            bool bRet = false;
            
            if (_Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK") < 0)
            {
                //Util.Alert("투입 위치를 선택 하세요.");
                Util.MessageValidation("SFU1957");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanRunStart()
        {
            bool bRet = false;
            
            for (int i = 0; i < dgInput.Rows.Count - dgInput.BottomRows.Count; i++)
            {
                if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "MOUNT_MTRL_TYPE_CODE")).Equals("PROD")) continue;

                string inLot = Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_LOTID"));
                if (inLot.Equals(""))
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "SEL_LOTID")).Equals(""))
                    {
                        if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "INPUT_MTRL_CLSS_CODE")).Equals(""))
                        {
                            //Util.Alert("{0}이 입력되지 않았습니다.", Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                            Util.MessageValidation("SFU1299", Util.NVC(DataTableConverter.GetValue(dgInput.Rows[i].DataItem, "EQPT_MOUNT_PSTN_NAME")));
                            return bRet;
                        }                        
                    }
                }
            }

            bRet = true;
            return bRet;
        }
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

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnOK);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SetInput(string sLot, string sQty, string sType, string sTypeName, string sMtgrid)
        {
            if (dgInput.ItemsSource == null || dgInput.Rows.Count <= 0)
                return;
            
            int iRow = -1;

            if (!sLot.Equals(""))
            {
                iRow = _Util.GetDataGridRowIndex(dgInput, "INPUT_LOTID", sLot);
                if (iRow >= 0)
                {
                    //Util.Alert("투입LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1967");
                    return;
                }
            }

            if (!sLot.Equals(""))
            {
                iRow = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", sLot);
                if (iRow >= 0)
                {
                    //Util.Alert("선택한 LOT에 동일한 LOT이 있습니다.");
                    Util.MessageValidation("SFU1657");
                    return;
                }
            }

            iRow = -1;
            
            iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");

            if (iRow < 0)
            {
                //Util.Alert("투입위치를 선택 하세요.");
                Util.MessageValidation("SFU1981");
                return;
            }

            if (!Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "INPUT_LOTID")).Trim().Equals(""))
            {
                //Util.Alert("해당 위치는 이미 투입 정보가 존재하여 투입할 수 없습니다.");
                Util.MessageValidation("SFU2021");
                return;
            }
            
            DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);

            if (!dtTmp.Columns.Contains("INPUT_LOTID"))
                dtTmp.Columns.Add("INPUT_LOTID", typeof(string));
            if (!dtTmp.Columns.Contains("INPUT_QTY"))
                dtTmp.Columns.Add("INPUT_QTY", typeof(int));
            if (!dtTmp.Columns.Contains("PRDT_CLSS_CODE"))
                dtTmp.Columns.Add("PRDT_CLSS_CODE", typeof(string));
            if (!dtTmp.Columns.Contains("PRDT_CLSS_NAME"))
                dtTmp.Columns.Add("PRDT_CLSS_NAME", typeof(string));
            if (!dtTmp.Columns.Contains("SEL_LOTID"))
                dtTmp.Columns.Add("SEL_LOTID", typeof(string));

            dtTmp.Rows[iRow]["SEL_LOTID"] = sLot;
            dtTmp.Rows[iRow]["INPUT_QTY"] = sQty.Equals("") ? 0 : Convert.ToDecimal(sQty);
            dtTmp.Rows[iRow]["PRDT_CLSS_CODE"] = sType;
            dtTmp.Rows[iRow]["PRDT_CLSS_NAME"] = sTypeName;

            dgInput.BeginEdit();
            dgInput.ItemsSource = DataTableConverter.Convert(dtTmp);
            dgInput.EndEdit();            
        }

        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #endregion
        
    }
}
