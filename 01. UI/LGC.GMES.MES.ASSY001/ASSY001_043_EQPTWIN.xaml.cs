/*************************************************************************************
 Created Date : 2018.11.20
      Creator : 이동우
   Decription : VD QA대상LOT조회 설비 내용(R to R) //
--------------------------------------------------------------------------------------
 [Change History]
    2019.03.05  오화백 : RF_ID일경우 CST 정보 보여주기
    2019.05.09  이동우 : 확정 호출 비즈 변경
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

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_043_EQPTWIN : UserControl, IWorkArea
    {
        #region Declaration & Constructor        

        Util _Util = new Util();

        //2019.03.05 RF_ID 여부
        string LDR_LOT_IDENT_BAS_CODE = string.Empty;
        string UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
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

        private string VdQaInspCond { get; set; } = string.Empty;
        private string QaType { get; set; } = string.Empty;

        private bool LotQAInsp { get; set; } = false;

        public string _LDR_LOT_IDENT_BAS_CODE
        {
            get { return LDR_LOT_IDENT_BAS_CODE; }
            set { LDR_LOT_IDENT_BAS_CODE = value; }
        }
        public string _UNLDR_LOT_IDENT_BAS_CODE
        {
            get { return UNLDR_LOT_IDENT_BAS_CODE; }
            set { UNLDR_LOT_IDENT_BAS_CODE = value; }
        }
        public bool ROUTE_CHANGE_YN { get; set; } = false;
        

        public ASSY001_043_EQPTWIN()
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
            
            //2019.03.05 RF_ID 여부 : RF_ID일 경우 CST 컬럼 보여줌
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgRunLot.Columns["CSTID"].Visibility = Visibility.Visible;
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgRunLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            GetRouteProdEqptLine();
            if (ROUTE_CHANGE_YN)
            {
                dgFinishLot.Columns["NT_LOTID"].Visibility = Visibility.Visible;
                dgFinishLot.Columns["NT_STAT"].Visibility = Visibility.Visible;
            }

            else
            {
                dgFinishLot.Columns["NT_LOTID"].Visibility = Visibility.Collapsed;
                dgFinishLot.Columns["NT_STAT"].Visibility = Visibility.Collapsed;
            }
            ApplyPermissions();

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
            // Parent = (ASSY001_024)parameters[6];
            VdQaInspCond = Util.NVC(parameters[6]);
            QaType = Util.NVC(parameters[7]);

            SetGrid();

            ApplyPermissions();

        }

        /// <summary>
        /// Lot ID별로 검사 선택 시.. 폴란드 사용 2017.08.13 Add By Kim Joonphil
        /// </summary>
        public void SetLotQAInsp()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters == null) return;

            LotQAInsp = (bool)parameters[0];
        }

        private void btnInspectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInsception())
                return;

            ASSY001_043_QAJUDG wndQA = new ASSY001_043_QAJUDG();
            wndQA.FrameOperation = FrameOperation;

            if (wndQA != null)
            {
                //object[] Parameters = new object[7];
                object[] Parameters = new object[9]; //2017.08.13 Edited By Kim Joonphil

                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID"));
                Parameters[1] = "";//Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE"));
                Parameters[2] = Eqptid;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID_RT"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));
                Parameters[5] = Eqsgid;
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "VD_QA_INSP_COND_CODE"));
                Parameters[7] = LotQAInsp == false ? Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE")).Equals("WAIT_RE_VD") ? true : false : LotQAInsp; //2017.08.13 Add By Kim Joonphil
                Parameters[8] = ROUTE_CHANGE_YN; //2019-05-21 윤세진 추가
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
            ASSY001_043_QAJUDG window = sender as ASSY001_043_QAJUDG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SetGrid();

                foreach (ASSY001_043 win in Util.FindVisualChildren<ASSY001_043>(Application.Current.MainWindow))
                {
                    win.REFRESH = true;
                    return;
                }

                // parent.REFRESH = true;
                //parent = null;
                System.Diagnostics.Debug.WriteLine("--clear: {0} Bytes.", GC.GetTotalMemory(false));
                //parent.REFRESH = true;
                //parent = null;
            }
        }
        private void wndEND_Closed(object sender, EventArgs e)
        {
            ASSY001_024_ENDTIME window = sender as ASSY001_024_ENDTIME;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SetGrid();

                foreach (ASSY001_024 win in Util.FindVisualChildren<ASSY001_024>(Application.Current.MainWindow))
                {
                    win.REFRESH = true;
                    return;
                }

                // parent.REFRESH = true;
                //parent = null;
                System.Diagnostics.Debug.WriteLine("--clear: {0} Bytes.", GC.GetTotalMemory(false));
                //parent.REFRESH = true;
                //parent = null;
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
                CHK_STANBY.Visibility = Visibility.Collapsed;

                btnInspectionConfirm.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;

                tbLotCount.Text = dgFinishLot.GetRowCount().ToString();

                if (QaType.Equals("Y")) // 자동
                {
                    btnConfirm.Visibility = Visibility.Collapsed;
                    btnInspectionConfirm.Visibility = Visibility.Collapsed;
                    CHK_FINISH.Visibility = Visibility.Collapsed;
                    //btnInspEndTime.Visibility = Visibility.Collapsed;
                    btnMovetoStanby.Visibility = Visibility.Collapsed;
                    QATYPE_FINISH.Visibility = Visibility.Visible;
                }
                else //수동
                {
                    btnConfirm.Visibility = Visibility.Visible;
                    btnInspectionConfirm.Visibility = Visibility.Visible;
                    CHK_FINISH.Visibility = Visibility.Visible;
                    //btnInspEndTime.Visibility = Visibility.Collapsed;
                    btnMovetoStanby.Visibility = Visibility.Collapsed;
                    QATYPE_FINISH.Visibility = Visibility.Collapsed;
                }
            }
            else //가동중
            {
                if (QaType.Equals("Y"))
                {
                    QATYPE.Visibility = Visibility.Visible;
                    QATYPE_FINISH.Visibility = Visibility.Visible;
                }
                else
                {
                    QATYPE.Visibility = Visibility.Collapsed;
                    QATYPE_FINISH.Visibility = Visibility.Collapsed;
                }
                dgRunLot.Visibility = Visibility.Visible;
                dgFinishLot.Visibility = Visibility.Collapsed;
                CHK_STANBY.Visibility = Visibility.Visible;
                btnInspectionConfirm.Visibility = Visibility.Collapsed;
                btnConfirm.Visibility = Visibility.Collapsed;
                grTime.Visibility = Visibility.Visible;
                //btnInspEndTime.Visibility = Visibility.Visible;
                btnMovetoStanby.Visibility = Visibility.Visible;
                tbLotCount.Text = dgRunLot.GetRowCount().ToString();
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
                                    //수분불량 LOT을 검사 해주세요
                                    Util.MessageValidation("SFU6021");
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
                    else
                    {
                        string judgValue = DataTableConverter.GetValue(dgRunLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();

                        if (string.IsNullOrEmpty(judgValue)) return;

                        //if (judgValue.Equals("WAIT") || judgValue.Equals("WAIT_WF") || judgValue.Equals("WAIT_DF") || judgValue.Equals("WAIT_RE_VD"))
                        //{
                        //    System.Drawing.Color color = GridBackColor.WAIT;
                        //    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        //}
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
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("White"));

                    if (e.Cell.Column.Name.Equals("JUDG_NAME"))
                    {
                        string judgValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();
                        string QAValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "QA_INSP_TRGT_FLAG").GetString();
                       // string FinalCutValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "FINAL_CUT_FLAG").GetString();
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
                        //if (QAValue.Equals("Y"))
                        //{
                        //    System.Drawing.Color color = GridBackColor.WAIT;
                        //    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        //}
                    }
                    else
                    {
                        string judgValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();
                        string QAValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "QA_INSP_TRGT_FLAG").GetString();
                        //string FinalCutValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "FINAL_CUT_FLAG").GetString();

                        if (string.IsNullOrEmpty(judgValue))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                            return;
                        }

                        //if (judgValue.Equals("WAIT") || judgValue.Equals("WAIT_WF") || judgValue.Equals("WAIT_DF") || judgValue.Equals("WAIT_RE_VD"))
                        //{
                        //    System.Drawing.Color color = GridBackColor.WAIT;
                        //    e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                        //    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                        //}
                        if (QAValue.Equals("Y"))
                        {
                            System.Drawing.Color color = GridBackColor.WAIT;
                            e.Cell.Presenter.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
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
                    if (DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE").Equals("WAIT") || DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE").Equals("WAIT_WF") || DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "JUDG_VALUE").Equals("WAIT_DF"))
                    {
                        if (VdQaInspCond.Equals("VD_QA_INSP_RULE_02"))
                        {
                            Util.MessageValidation("SFU3366", DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID_RT")); //대LOT %1 은 확정 할 수 없는 상태 입니다.
                        }
                        else
                        {
                            Util.MessageValidation("SFU3367", DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID")); //LOT [%1] 은 확정 할 수 없는 상태 입니다. 
                        }

                        return;
                    }

                }
            }

            DataTable tmpdt = DataTableConverter.Convert(dgFinishLot.ItemsSource).Select("CHK = 1").Count() == 0 ? null : DataTableConverter.Convert(dgFinishLot.ItemsSource).Select("CHK = 1").CopyToDataTable();
            for (int i = 0; i < tmpdt.Rows.Count; i++)
            {
                if (int.Parse(tmpdt.Rows[i]["REWORKCNT"].ToString()) != 0)
                {


                    for (int j = 0; j < dgFinishLot.GetRowCount(); j++)
                    {
                        if (!tmpdt.Rows[i]["LOTID"].Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[j].DataItem, "LOTID"))))
                        {
                            if (tmpdt.Rows[i]["EQPT_BTCH_WRK_NO"].ToString().Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[j].DataItem, "EQPT_BTCH_WRK_NO"))))
                            {

                                if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[j].DataItem, "JUDG_VALUE")).Equals("WAIT_WF"))
                                {
                                    Util.MessageValidation("LOT[%1]는 같은 챔버에 수분불량으로 인한 재작업LOT이 있어 확정할 수 없습니다.", tmpdt.Rows[i]["LOTID"].ToString());
                                    return;
                                }

                            }
                        }

                    }
                }
            }

            DataSet ds = new DataSet();

            DataTable inData = ds.Tables.Add("INDATA");
            inData.Columns.Add("USERID", typeof(string));
            inData.Columns.Add("EQSGID", typeof(string));
            inData.Columns.Add("EQPTID", typeof(string));

            DataRow dr = inData.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["EQSGID"] = Eqsgid;
            dr["EQPTID"] = Eqptid;
            inData.Rows.Add(dr);

            DataTable inLot = ds.Tables.Add("IN_LOT");
            inLot.Columns.Add("LOTID", typeof(string));

            DataRow row2 = null;

            for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "CHK")).Equals("True"))
                {

                    if (VdQaInspCond.Equals("VD_QA_INSP_RULE_02")) //설비의 판별 기준정보
                    {
                        if (int.Parse(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "COUNT"))) > 1)
                        {
                            DataTable dt = new DataTable();
                            dt.Columns.Add("LOTID_RT", typeof(string));
                            dt.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));

                            DataRow row = dt.NewRow();
                            row["LOTID_RT"] = VdQaInspCond.Equals("VD_QA_INSP_RULE_02") ? DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID_RT") : null;
                            row["EQPT_BTCH_WRK_NO"] = DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO");
                            dt.Rows.Add(row);

                            DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_FOR_VDQA", "INDATA", "OUTDATA", dt);

                            for (int j = 0; j < result.Rows.Count; j++)
                            {
                                row2 = inLot.NewRow();
                                row2["LOTID"] = Convert.ToString(result.Rows[j]["CBO_CODE"]);
                                inLot.Rows.Add(row2);
                            }
                        }
                        else
                        {
                            row2 = inLot.NewRow();
                            row2["LOTID"] = DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID");
                            inLot.Rows.Add(row2);
                        }

                    }
                    else
                    {
                        row2 = inLot.NewRow();
                        row2["LOTID"] = DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID");
                        inLot.Rows.Add(row2);
                    }

                }
            }

            //검사확정 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3368"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    //   loadingIndicator.Visibility = Visibility.Visible;
                    //  BR_RPD_REG_QA_CONFIRM_FOR_VD ->BR_RPD_REG_QA_CONFIRM_FOR_VD_PANCAKE 변경 05.09
                    new ClientProxy().ExecuteService_Multi("BR_RPD_REG_QA_CONFIRM_FOR_VD_PANCAKE", "INDATA, IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_RPD_REG_QA_CONFIRM_FOR_VD_PANCAKE", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.MessageInfo("SFU3369");//확정완료

                            foreach (ASSY001_043 win in Util.FindVisualChildren<ASSY001_043>(Application.Current.MainWindow))
                            {
                                win.REFRESH = true;
                                return;
                            }

                            //   parent.REFRESH = true;
                            //parent.SearchData();
                            //parent = null;
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
        //private void btnInspEndTime_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender == null)
        //        return;

        //    ASSY001_024_ENDTIME wndEND = new ASSY001_024_ENDTIME();
        //    wndEND.FrameOperation = FrameOperation;

        //    if (wndEND != null)
        //    {
        //        object[] Parameters = new object[1];

        //        Parameters[0] = Eqptid;

        //        C1WindowExtension.SetParameters(wndEND, Parameters);

        //        wndEND.Closed += new EventHandler(wndEND_Closed);

        //        // 팝업 화면 숨겨지는 문제 수정.
        //        this.Dispatcher.BeginInvoke(new Action(() => wndEND.ShowModal()));
        //        wndEND.BringToFront();
        //    }
        //}

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
                        e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)8), dgRunLot.GetCell((int)(i - 1), (int)8)));
                        e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)9), dgRunLot.GetCell((int)(i - 1), (int)9)));
                        //     e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)10), dgRunLot.GetCell((int)(i - 1), (int)10)));
                        x1 = i;
                        tmp = Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[x1].DataItem, "EQPT_BTCH_WRK_NO"));
                    }


                }

                e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)8), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)8)));
                e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)9), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)9)));
                //      e.Merge(new DataGridCellsRange(dgRunLot.GetCell((int)x1, (int)10), dgRunLot.GetCell((int)(dgRunLot.GetRowCount() - 1), (int)10)));

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
                            Util.MessageValidation("검사대기(두께불량) 먼저 판정해주세요");

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

            Eqptname = string.Empty;
            Eqptid = string.Empty;
            Eleccheck = string.Empty;
            EqptElec = string.Empty;
            Eqsgid = string.Empty;
            Finishflag = false;
            VdQaInspCond = string.Empty;

            dgFinishLot.ItemsSource = null;
            dgRunLot.ItemsSource = null;

            tbLotCount.Text = string.Empty;

        }

        private void btnMovetoStanby_Click(object sender, RoutedEventArgs e)
        {

            if (_Util.GetDataGridCheckFirstRowIndex(dgRunLot, "CHK") == -1)
            {
                //Util.Alert("선택 된 LOT이 없습니다.");
                Util.MessageValidation("SFU1661");
                return;
            }

            Util.MessageConfirm("SFU5080", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("IN_EQP");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));
                    inData.Columns.Add("FINAL_CUT_FLAG", typeof(string));
                    DataRow row = inData.NewRow();

                    row["SRCTYPE"] = "UI";
                    row["IFMODE"] = "OFF";
                    row["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[0].DataItem, "EQPTID"));
                    row["USERID"] = LoginInfo.USERID;
                    row["FINAL_CUT_FLAG"] = "Y";
                    indataSet.Tables["IN_EQP"].Rows.Add(row);

                    DataTable input = indataSet.Tables.Add("IN_INPUT");
                    input.Columns.Add("LOTID", typeof(string));

                    DataTable dtProductLot = DataTableConverter.Convert(dgRunLot.ItemsSource);
                    foreach (DataRow _iRow in dtProductLot.Rows)
                    {
                        string a = _iRow["CHK"].ToString();
                        if (_iRow["CHK"].ToString().Equals("True") || _iRow["CHK"].ToString().Equals("1"))
                        {
                            row = input.NewRow();

                            row["LOTID"] = _iRow["LOTID"];
                            indataSet.Tables["IN_INPUT"].Rows.Add(row);
                        }
                    }

                    //DataRow row_2 = input.NewRow();
                    //row_2["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgRunLot, "CHK")].DataItem, "LOTID"));
                    //indataSet.Tables["IN_INPUT"].Rows.Add(row_2);

                    try
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MOVE_LOT_QA_WAIT_VD_PANCAKE", "INDATA,IN_INPUT", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                                foreach (ASSY001_043 win in Util.FindVisualChildren<ASSY001_043>(Application.Current.MainWindow))
                                {
                                    win.REFRESH = true;
                                    return;
                                }
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void dgRunLot_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {

            if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
            {
                C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                CheckBox cb = cell.Presenter.Content as CheckBox;

                int idx1 = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index; //선택한거
                int idx2 = _Util.GetDataGridCheckFirstRowIndex(dgRunLot, "CHK"); //이미 선택된거

                if (idx2 != -1 && !Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[idx1].DataItem, "PRODID")).Equals(Util.NVC(DataTableConverter.GetValue(dgRunLot.Rows[idx2].DataItem, "PRODID"))))
                {
                    _Util.SetDataGridUncheck(dgRunLot, "CHK", idx1);
                    (dgRunLot.GetCell(idx1, dgRunLot.Columns["CHK"].Index).Presenter.Content as CheckBox).IsChecked = true;
                }
            }
            }

        private void dgFinishLot_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {

                if (dgFinishLot.GetRowCount() == 0)
                {
                    return;
                }
                string tmp = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[0].DataItem, "EQPT_BTCH_WRK_NO"));
                string lottmp = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[0].DataItem, "LOTID"));

                int x1 = 0;
                int x2 = 0;
                for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                {

                    if (!tmp.Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO"))))
                    {
                        //e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)8), dgFinishLot.GetCell((int)(i - 1), (int)8)));
                       // e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)6), dgFinishLot.GetCell((int)(i - 1), (int)6)));
                       //if(lottmp.Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID"))))
                       // e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)2), dgFinishLot.GetCell((int)(i - 1), (int)2)));

                        e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)14), dgFinishLot.GetCell((int)(i - 1), (int)14)));
                        x1 = i;
                        tmp = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[x1].DataItem, "EQPT_BTCH_WRK_NO"));
                    }
                }


                for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                {

                    if (!lottmp.Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "LOTID"))))
                    {
                        e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x2, (int)2), dgFinishLot.GetCell((int)(i - 1), (int)2)));
                        x2 = i;
                        lottmp = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[x2].DataItem, "LOTID"));
                    }
                }

                //e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)8), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), (int)8)));
                // e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)6), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), (int)6)));
                e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x2, (int)2), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), (int)2)));
                e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, (int)14), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), (int)14)));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void GetRouteProdEqptLine()
        {
            try
            {
                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CMCDTYPE", typeof(string));
                inTable.Columns.Add("ATTRIBUTE2", typeof(string));

                DataRow dtRow = inTable.NewRow();
                dtRow["CMCDTYPE"] = "VD_NTC_ROUTE_PROD_EQPT_LINE";
                dtRow["ATTRIBUTE2"] = Eqptid;

                inTable.Rows.Add(dtRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTES", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    ROUTE_CHANGE_YN = true;
                }
                else
                    ROUTE_CHANGE_YN = false;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                //HiddenLoadingIndicator();
            }
        }
    }
}