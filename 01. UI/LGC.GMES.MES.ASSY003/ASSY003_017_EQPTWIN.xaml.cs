/*************************************************************************************
 Created Date : 2017.10.11
      Creator : 이진선
   Decription : VD QA대상LOT조회 설비 내용
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  이진선 : Initial Created.
  2022.04.20  장희만 : C20220410-000011- 이송 Biz 호출 시 문제가 있는 경우 Biz Error Message 나오도록 수정 
**************************************************************************************/

using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;

namespace LGC.GMES.MES.ASSY003
{
    public partial class ASSY003_017_EQPTWIN : UserControl, IWorkArea
    {
        #region Declaration & Constructor        

        Util _Util = new Util();

        #endregion

        #region Initialize        
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }


        private string Eqptname { get; set; } = string.Empty;

        private string Eqptid { get; set; } = string.Empty;

        private string Eleccheck { get; set; } = string.Empty;

        private string EqptElec { get; set; } = string.Empty;

        private string Eqsgid { get; set; } = string.Empty;

        private bool Finishflag { get; set; } = false;

        private string _PRODID { get; set; } = string.Empty;

        private string _UNIT { get; set; } = string.Empty;

        private string _PROCID { get; set; } = string.Empty;

        public ASSY003_017_EQPTWIN()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        #endregion

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
          //  ApplyPermissions();

        }

        public void SetValue()
        {
            

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters == null) return;

            Finishflag = (bool)parameters[0];
            Eleccheck = Util.NVC(parameters[1]);
            EqptElec = Util.NVC(parameters[2]);
            Eqptname = Util.NVC(parameters[3]);
            Eqptid = Util.NVC(parameters[4]);
            Eqsgid = Util.NVC(parameters[5]);
            _UNIT = Util.NVC(parameters[6]);
            _PROCID = Util.NVC(parameters[7]);

            SetGrid();

            ApplyPermissions();

        }


        private void btnInspectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInsception())
                return;

            ASSY003_017_QAJUDG wndQA = new ASSY003_017_QAJUDG();
            wndQA.FrameOperation = FrameOperation;

            if (wndQA != null)
            {
                object[] Parameters = new object[9];

                Parameters[0] = _UNIT.Equals("LOT") ?  Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID")) : Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "CSTID"));
                Parameters[1] = "";//Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE"));
                Parameters[2] = Eqptid;
                Parameters[3] = "";//Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID_RT")).Equals("");
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));
                Parameters[5] = Eqsgid;
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "PRODID"));
                Parameters[7] = _UNIT;
                Parameters[8] = _PROCID;

                C1WindowExtension.SetParameters(wndQA, Parameters);

                wndQA.Closed += new EventHandler(wndQA_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQA.ShowModal()));
                wndQA.BringToFront();

            }

        }
        #endregion

        #region[Method]
        private void wndQA_Closed(object sender, EventArgs e)
        {
            ASSY003_017_QAJUDG window = sender as ASSY003_017_QAJUDG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SetGrid();

                foreach (ASSY003_017 win in Util.FindVisualChildren<ASSY003_017>(Application.Current.MainWindow))
                {
                    win.REFRESH = true;
                    return;
                }

                System.Diagnostics.Debug.WriteLine("--clear: {0} Bytes.", GC.GetTotalMemory(false));
            }
        }
        


        private void SetGrid()
        {
            tbEqptName.Text = Eqptname;
            
            if (Finishflag == true) //VD완료, 검사대기
            {
                dgRunLot.Visibility = Visibility.Collapsed;
                dgFinishLot.Visibility = Visibility.Visible;
                grTime.Visibility = Visibility.Collapsed;


                btnInspectionConfirm.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;

                tbWIPSTAT.Text = ObjectDic.Instance.GetObjectName("검사대기현황");
            }
            else //가동중
            {
                dgRunLot.Visibility = Visibility.Visible;
                dgFinishLot.Visibility = Visibility.Collapsed;

                btnInspectionConfirm.Visibility = Visibility.Collapsed;
                btnConfirm.Visibility = Visibility.Collapsed;
                grTime.Visibility = Visibility.Visible;

                tbWIPSTAT.Text = ObjectDic.Instance.GetObjectName("VD가동현황");
            }


        }

        private bool ValidateInsception()
        {
            try
            {
                if (dgFinishLot.ItemsSource == null)
                {
                    return false;
                }

                if (_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK") == -1)
                {
                    Util.Alert("SFU1632"); // 선택된 LOT이 없습니다.
                    return false;
                }

                DataTable dt = DataTableConverter.Convert(dgFinishLot.ItemsSource);
                if (dt.Rows.Count != 0)
                {
                    if (dt.Select("CHK = 1").Count() > 1)
                    {
                        Util.MessageValidation("SFU3364"); //샘플검사LOT 한개만 선택해주세요
                        return false;
                    }
                }


                int idx = _Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK");
                if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "REWORKCNT"))) != 0)
                {

                    for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                    {
                        if (i != idx)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO")).Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO"))))
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "JUDG_VALUE")).Equals("WAIT") && Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE")).Equals("WAIT_WF"))
                                {
                                    Util.MessageValidation("SFU4252");  // 수분불량 LOT을 검사 해주세요
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static class GridBackColor
        {

            public static readonly System.Drawing.Color FAIL = System.Drawing.Color.FromArgb(255, 105, 105); //FAIL

            public static readonly System.Drawing.Color PASS = System.Drawing.Color.FromArgb(169, 209, 142); // PASS
            public static readonly System.Drawing.Color WAIT = System.Drawing.Color.FromArgb(255, 192, 0); //WAIT
            public static readonly System.Drawing.Color HOLD = System.Drawing.Color.FromArgb(89, 89, 89); //HOLD



        }



        #endregion
        private void dgRunLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Cell.Presenter == null || dgRunLot.GetRowCount() == 0)
            {
                return;
            }

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Cell.Column.Name.Equals("JUDG_NAME"))
                    {
                        string judgValue = DataTableConverter.GetValue(dgRunLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();

                        if (string.IsNullOrEmpty(judgValue)) return;

                        if (judgValue.Equals("WAIT") || judgValue.Equals("WAIT_WF") || judgValue.Equals("WAIT_DF") || judgValue.Equals("WAIT_RE_VD"))
                        {
                            System.Drawing.Color color = GridBackColor.WAIT;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        }
                        else if (judgValue.Equals("PASS"))
                        {
                            System.Drawing.Color color = GridBackColor.PASS;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        }
                        else if (judgValue.Equals("FAIL"))
                        {
                            System.Drawing.Color color = GridBackColor.FAIL;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                        }
                    }
                }
                catch (Exception ex)
                {

                }


            }));
        }

        private void dgFinishLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            if (e.Cell.Presenter == null || dgFinishLot.GetRowCount() == 0)
            {
                return;
            }

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));

                    if (e.Cell.Column.Name.Equals("JUDG_NAME"))
                    {
                        string judgValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();
                        if (string.IsNullOrEmpty(judgValue))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                            return;
                        }

                        if (judgValue.Equals("WAIT") || judgValue.Equals("WAIT_WF") || judgValue.Equals("WAIT_DF") || judgValue.Equals("WAIT_RE_VD"))
                        {
                            System.Drawing.Color color = GridBackColor.WAIT;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        }
                        else if (judgValue.Equals("PASS"))
                        {
                            System.Drawing.Color color = GridBackColor.PASS;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        }
                        else if (judgValue.Equals("FAIL"))
                        {
                            System.Drawing.Color color = GridBackColor.FAIL;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                        }
                        else if (judgValue.Equals("HOLD"))
                        {
                            System.Drawing.Color color = GridBackColor.HOLD;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("white"));
                        }
                    }
                }
                catch (Exception ex)
                {

                }


            }));

        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK") == -1)
            {
                Util.MessageValidation("SFU3365"); //확정할 LOT을 선택하세요
                return;
            }

            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("True"))

                {
                    if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE")).Equals("WAIT") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE")).Equals("HOLD"))
                    {
                        
                        Util.MessageValidation("SFU3367", DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID")); //LOT [%1] 은 확정 할 수 없는 상태 입니다. 
                        return;
                    }

                }
            }

            //검사확정 하시겠습니까?
            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라미로 이송하시겠습니까?"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            Util.MessageConfirm("SFU4257", (result) => // 다음 공정으로 이송 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    //   loadingIndicator.Visibility = Visibility.Visible;

                    DataSet ds = new DataSet();
                    DataTable inData = ds.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("SHOPID", typeof(string));

                    DataRow dr = inData.NewRow();
                    dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    dr["IFMODE"] = IFMODE.IFMODE_OFF;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    inData.Rows.Add(dr);

                    DataTable inLot = ds.Tables.Add("IN_LOT");
                    inLot.Columns.Add("LOTID", typeof(string));
                    inLot.Columns.Add("PRODID", typeof(string));

                    if (_UNIT.Equals("LOT"))
                    {

                        for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow dr2 = inLot.NewRow();
                                dr2["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID"));
                                dr2["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "PRODID"));
                                inLot.Rows.Add(dr2);
                            }
                        }

                    }else
                    {
                        DataSet ds2 = new DataSet();
                        DataTable inData2 = ds2.Tables.Add("INDATA");
                        inData2.Columns.Add("SKIDID", typeof(string));
                        inData2.Columns.Add("WIPSTAT", typeof(string));

                        for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("True"))
                            {
                                DataRow tmpdr = inData2.NewRow();
                                tmpdr["SKIDID"] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CSTID"));
                                tmpdr["WIPSTAT"] = "END";
                                inData2.Rows.Add(tmpdr);
                            } 
                        }

                       // new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);
                        DataSet out_ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_SKIDID2PANCAKE_NJ", "INDATA", "OUTDATA", ds2);

                        for (int i = 0; i < out_ds.Tables["OUTDATA"].Rows.Count; i++)
                        {
                          
                            DataRow dr2 = inLot.NewRow();
                            dr2["LOTID"] = Convert.ToString(out_ds.Tables["OUTDATA"].Rows[i]["LOTID"]);
                            dr2["PRODID"] = Convert.ToString(out_ds.Tables["OUTDATA"].Rows[i]["PRODID"]);
                            inLot.Rows.Add(dr2);
                        }

                        // BR_PRD_CHK_SKIDID2PANCAKE
                    }

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_LOT_LINE_NJ", "INDATA, IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                //Util.MessageException(bizException); Biz Error Message 대체
                                Util.ShowExceptionMessages(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU3369");//확정완료

                            foreach (ASSY003_017 win in Util.FindVisualChildren<ASSY003_017>(Application.Current.MainWindow))
                            {
                                win.REFRESH = true;
                                return;
                            }


                            System.Diagnostics.Debug.WriteLine("inspect ok: {0} Bytes.", GC.GetTotalMemory(false));


                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, ds);
                }
            });



        }

        private void dgFinishLot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }
   
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgFinishLot.Rows[i].DataItem, "CHK", true);
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                DataTableConverter.SetValue(dgFinishLot.Rows[i].DataItem, "CHK", false);
            }
        }

        private void dgRunLot_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {

                if (dgRunLot.GetRowCount() == 0)
                {
                    return;
                }
                string tmp = Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[0].DataItem, "EQPT_BTCH_WRK_NO"));
                int x1 = 0;

                for (int i = 0; i < dgRunLot.GetRowCount(); i++)
                {
                    if (!tmp.Equals(Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO"))))
                    {
                        e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)6), dgRunLot.GetCell((int)(i - 1), (int)6)));
                        e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)7), dgRunLot.GetCell((int)(i - 1), (int)7)));
                        e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)8), dgRunLot.GetCell((int)(i - 1), (int)8)));
                        x1 = i;
                        tmp = Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[x1].DataItem, "EQPT_BTCH_WRK_NO"));
                    }


                }

                e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)6), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)6)));
                e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)7), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)7)));
                e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)8), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)8)));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

     
  

        private void dgFinishLot_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    string EQPT_BTCH_WRK_NO = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "EQPT_BTCH_WRK_NO"));

                    if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[idx].DataItem, "JUDG_VALUE")).Equals("WAIT"))
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

                        DataRow row = dt.NewRow();
                        row["EQPT_BTCH_WRK_NO"] = EQPT_BTCH_WRK_NO;
                        dt.Rows.Add(row);

                        DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA", "INDATA", "OUTDATA", dt);

                        if (result.Select("QA_INSP_TRGT_FLAG = 'D' and QA_INSP_CMPL_FLAG = 'N'").Count() > 0)
                        {
                            //DataTableConverter.SetValue(dgFinishLot.Rows[idx].DataItem, "CHK", 1);
                            Util.MessageValidation("SFU4253");  // 검사대기(두께불량) 먼저 판정해주세요

                            return;
                        }
                    }


                  

                  

                }
            }));

        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            listAuth.Add(btnInspectionConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        public void ClearData()
        {

            tbEqptName.Text = string.Empty;
            tbWIPSTAT.Text = string.Empty;

            Eqptname = string.Empty;
            Eqptid = string.Empty;
            Eleccheck = string.Empty;
            EqptElec = string.Empty;
            Eqsgid = string.Empty;
            Finishflag = false;

            dgFinishLot.ItemsSource = null;
            dgRunLot.ItemsSource = null;
           
       
    }

    }

}