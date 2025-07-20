/*************************************************************************************
 Created Date : 2019.11.06
      Creator : 이상준C
   Decription : 전지 5MEGA-GMES 구축 - ZZS 공정진척 화면 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.06  이상준C : Initial Created.
  
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_023_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_023_RUNSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _WoID = string.Empty;
        private string _WoDetail = string.Empty;
        private bool bSave = false;
        private bool isCellDetlNeed = false;

        private string EqptElcType = "";

        private string _Max_Pre_Proc_End_Day = string.Empty;
        private string _PROD_LV1_CODE = "";
        private string _PROD_LV2_CODE = "";
        private string _PROD_LV3_CODE = "";
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

        public ASSY003_023_RUNSTART()
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
            SetCombo();

            grdMsg.Visibility = Visibility.Collapsed;

            ApplyPermissions();

            GetEqptInfo();
            GetInputMountInfo();
            GetProductLvCode();
            // 선입선출 기준일 조회.
            //GetProcMtrlInputRule();

            SetCellDetlClss();
        }

        private void SetCombo()
        {
            CommonCombo cbo = new CommonCombo();
            string[] sFilter1 = { "CELL_DETL_CLSS_CODE" };
            cbo.SetCombo(cboCellClass, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");
        }

        private void SetCellDetlClss()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable();
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["PROCID"] = Process.ZZS;
                dr["EQSGID"] = _LineID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_PROCESSEQUIPMENTSEGMENT", "INDATA", "OUTDATA", dt, (result, exception) =>
                {
                    try
                    {
                        if(result.Rows.Count > 0 && result.Rows[0]["CELL_DETL_CLSS_MNGT_FLAG"].Equals("Y"))
                        {
                            stpCellDetl.Visibility = Visibility.Visible;
                            isCellDetlNeed = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
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

        private void C1Window_Initialized(object sender, EventArgs e)
        {
            //GetEqptInfo();
            //InitCombo();
        }

        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (dgList.CurrentRow == null || dgList.CurrentRow.DataItem == null || dgList.CurrentRow.Index < 0)
            //    return;


            //string sRet = string.Empty;
            //string sMsg = string.Empty;

            //SetInput(Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID")),
            //            //sLoc,
            //            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "WIPQTY")),
            //            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_CODE")),
            //            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_NAME")),
            //            "PROD");
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSearch())
                return;

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInput, "CHK");
            if (iRow < 0)
                return;

            GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dgInput.Rows[iRow].DataItem, "EQPT_MOUNT_PSTN_ID")));
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
                                        GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_MOUNT_PSTN_ID")));
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
                                    GetWaitLotList_ByMtrlClssCode(Util.NVC(DataTableConverter.GetValue(dg.Rows[e.Cell.Row.Index].DataItem, "EQPT_MOUNT_PSTN_ID")));
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
            //this.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    C1DataGrid dg = sender as C1DataGrid;
            //    if (e.Cell != null &&
            //        e.Cell.Presenter != null &&
            //        e.Cell.Presenter.Content != null)
            //    {
            //        CheckBox chk = e.Cell.Presenter.Content as CheckBox;
            //        if (chk != null)
            //        {
            //            switch (Convert.ToString(e.Cell.Column.Name))
            //            {
            //                case "CHK":
            //                    if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                       dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
            //                       (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                       !(bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
            //                    {
            //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
            //                        chk.IsChecked = true;

            //                        for (int idx = 0; idx < dg.Rows.Count; idx++)
            //                        {
            //                            if (e.Cell.Row.Index != idx)
            //                            {
            //                                if (dg.GetCell(idx, e.Cell.Column.Index).Presenter != null &&
            //                                    dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content != null &&
            //                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox) != null)
            //                                {
            //                                    (dg.GetCell(idx, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked = false;
            //                                }
            //                                DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
            //                            }
            //                        }
            //                    }
            //                    else if (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
            //                             dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
            //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox) != null &&
            //                             (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                             (bool)(dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox).IsChecked)
            //                    {
            //                        DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
            //                        chk.IsChecked = false;
            //                    }
            //                    break;
            //            }
            //        }
            //        else if (e.Cell.Column.Index != dg.Columns.Count - 1) // 선택 후 Curr.Col.idx를 맨뒤로 보내므로.. 다시타는 문제.
            //        {
            //            if (!dg.Columns.Contains("CHK"))
            //                return;

            //            CheckBox chk2 = dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox;

            //            if (chk2 != null)
            //            {
            //                if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
            //                       dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
            //                       (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
            //                       (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                       !(bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
            //                {
            //                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", true);
            //                    chk2.IsChecked = true;

            //                    for (int idx = 0; idx < dg.Rows.Count; idx++)
            //                    {
            //                        if (e.Cell.Row.Index != idx)
            //                        {
            //                            if (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter != null &&
            //                                dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content != null &&
            //                                (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null)
            //                            {
            //                                (dg.GetCell(idx, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = false;
            //                            }
            //                            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", false);
            //                        }
            //                    }
            //                }
            //                else if (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter != null &&
            //                         dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content != null &&
            //                         (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox) != null &&
            //                         (dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked.HasValue &&
            //                         (bool)(dg.GetCell(e.Cell.Row.Index, dg.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked)
            //                {
            //                    DataTableConverter.SetValue(dg.Rows[e.Cell.Row.Index].DataItem, "CHK", false);
            //                    chk2.IsChecked = false;
            //                }
            //            }
            //        }

            //        if (dg.CurrentCell != null)
            //            dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
            //        else if (dg.Rows.Count > 0)
            //            dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
            //    }
            //}));
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

        private void dgList_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            SetSelectPancake(false);
        }

        private void dgList_CommittingEdit(object sender, DataGridEndingEditEventArgs e)
        {
            try
            {
                if (dgList.CurrentRow == null || dgList.CurrentRow.DataItem == null || dgList.CurrentRow.Index < 0)
                    return;


                string sRet = string.Empty;
                string sMsg = string.Empty;

                if (DataTableConverter.GetValue(e.Row.DataItem, "CHK").Equals(1))
                {
                    SetInput(Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID")),
                            //sLoc,
                            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "WIPQTY")),
                            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_CODE")),
                            Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "PRDT_CLSS_NAME")),
                            "PROD");
                }
                else
                {
                    RemoveInput(Util.NVC(DataTableConverter.GetValue(dgList.CurrentRow.DataItem, "LOTID")));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {

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
                        Util.GridSetData(dgInput, searchResult, null, false);

                        dgInput.CurrentCellChanged += dgInput_CurrentCellChanged;

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

        
        private void GetWaitLotList_ByMtrlClssCode(string sEqptMountPstnID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_READY_LOT_ZZS();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.ZZS;
                newRow["EQPTID"] = _EqptID;
                newRow["IN_LOTID"] = txtPancake.Text;
                newRow["EQPT_MOUNT_PSTN_ID"] = sEqptMountPstnID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_ZZS", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
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

                        SetSelectPancake(true);
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

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_INPUT_LOT_ZZS", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

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

                if (isCellDetlNeed)
                    newRow["LAMI_CELL_TYPE"] = cboCellClass.SelectedValue.ToString();

                inTable.Rows.Add(newRow);
                newRow = null;

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_NEW_PROD_LOTID_ZZS", "IN_EQP,IN_INPUT", "OUTDATA", indataSet);

                string sNewLot = string.Empty;
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    sNewLot = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["PROD_LOTID"]);
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
                if (isCellDetlNeed)
                    newRow["CELL_DETL_CLSS_CODE"] = cboCellClass.SelectedValue.ToString();

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

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_PROD_LOT_ZZS", "IN_EQP,IN_INPUT", "OUT_LOT", (searchResult, searchException) =>
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
                newRow["PROCID"] = Process.ZZS;

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

        private void GetProductLvCode()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCT_LEVEL_CODE_BY_EQPTID", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    _PROD_LV1_CODE = Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL1_CODE"]);
                    _PROD_LV2_CODE = Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL2_CODE"]);
                    _PROD_LV3_CODE = Util.NVC(dtRslt.Rows[0]["PRODUCT_LEVEL3_CODE"]);
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

            // MC 인 경우 AN/CA 위치관계없이 1개씩 선택 여부 체크
            if (_PROD_LV2_CODE.Equals("MC"))
            {
                DataTable dtTmp = DataTableConverter.Convert(dgInput.ItemsSource);
                if (dtTmp != null)
                {
                    DataRow[] drAn = dtTmp.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND INPUT_MTRL_CLSS_CODE = 'AN' AND (ISNULL(INPUT_LOTID, '') <> '' OR ISNULL(SEL_LOTID, '') <> '')");
                    DataRow[] drCa = dtTmp.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND INPUT_MTRL_CLSS_CODE IN ('CA', 'CS') AND (ISNULL(INPUT_LOTID, '') <> '' OR ISNULL(SEL_LOTID, '') <> '')");
                    DataRow[] drAnOrg = dtTmp.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND INPUT_MTRL_CLSS_CODE = 'AN'");
                    DataRow[] drCaOrg = dtTmp.Select("MOUNT_MTRL_TYPE_CODE = 'PROD' AND INPUT_MTRL_CLSS_CODE IN ('CA', 'CS')");

                    // AN 체크
                    if (drAn.Length < 1)
                    {
                        if (drAnOrg.Length > 0)
                        {
                            Util.MessageValidation("SFU3689", Util.NVC(drAnOrg[0]["INPUT_MTRL_CLSS_CODE"]));    // Notched Roll 음극(%1)이 투입되지 않았습니다.
                            return bRet;
                        }
                    }

                    // CA 체크
                    if (drCa.Length < 1)
                    {
                        if (drCaOrg.Length > 0)
                        {
                            Util.MessageValidation("SFU3688", Util.NVC(drCaOrg[0]["INPUT_MTRL_CLSS_CODE"]));    // Notched Roll 양극(%1)이 투입되지 않았습니다.
                            return bRet;
                        }
                    }

                    // 1건 이상 입력 체크
                    if (drAn.Length != 1 || drCa.Length != 1)
                    {
                        Util.MessageValidation("SFU3690"); // Mono Cell은 음/양극 1개의 팬케익을 선택 해야 합니다.
                        return bRet;
                    }
                }                
            }
            else
            {
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
            }

            if (isCellDetlNeed)
            {
                if(cboCellClass.SelectedIndex <= 0)
                {
                    //셀 상세분류를 선택하세요.
                    Util.MessageValidation("SFU4587");
                    return bRet;
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

        private void RemoveInput(string sLot)
        {
            if (dgInput.ItemsSource == null || dgInput.Rows.Count <= 0)
                return;

            int iRow = -1;

            iRow = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", sLot);

            if (iRow < 0)
            {
                return;
            }
            else
            {
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

                dtTmp.Rows[iRow]["SEL_LOTID"] = "";
                dtTmp.Rows[iRow]["INPUT_QTY"] = 0;
                dtTmp.Rows[iRow]["PRDT_CLSS_CODE"] = "";
                dtTmp.Rows[iRow]["PRDT_CLSS_NAME"] = "";

                dgInput.BeginEdit();
                dgInput.ItemsSource = DataTableConverter.Convert(dtTmp);
                dgInput.EndEdit();
            }
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

        private void SetSelectPancake(bool bSearch)
        {
            // 선택 되어진 대기 메거진 표시 
            if (bSearch)
            {
                // 조회후 체크
                for (int nrow = 0; nrow < dgInput.Rows.Count; nrow++)
                {
                    if (!String.IsNullOrWhiteSpace(DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SEL_LOTID").ToString()))
                    {
                        int idx = _Util.GetDataGridRowIndex(dgList, "LOTID", DataTableConverter.GetValue(dgInput.Rows[nrow].DataItem, "SEL_LOTID").ToString());

                        if (idx > -1)
                        {
                            DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "CHK", 1);
                        }
                    }
                }
            }
            else
            {
                // 그리드 Click시 
                for (int nrow = 0; nrow < dgList.Rows.Count; nrow++)
                {
                    if (DataTableConverter.GetValue(dgList.Rows[nrow].DataItem, "CHK").Equals(1))
                    {
                        int idx = _Util.GetDataGridRowIndex(dgInput, "SEL_LOTID", DataTableConverter.GetValue(dgList.Rows[nrow].DataItem, "LOTID").ToString());

                        if (idx < 0)
                        {
                            DataTableConverter.SetValue(dgList.Rows[nrow].DataItem, "CHK", 0);
                        }
                    }
                }
            }

        }
        #endregion

        #endregion


    }
}
