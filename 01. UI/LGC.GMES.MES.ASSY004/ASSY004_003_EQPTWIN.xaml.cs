/*************************************************************************************
 Created Date : 2019.05.09
      Creator : INS 김동일K
   Decription : CWA3동 증설 - VD QA 대상LOT조회 화면 (ASSY0001.ASSY001_043_EQPTWIN 2019/05/09 소스 카피 후 작성)
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.09  INS 김동일K : Initial Created.
  2020.08.27  정문교      : QaType = 'Y'인 경우 검사,확정 버튼 안보이게 수정
  2022.04.29  INS 안유수S : 확정 처리 시 로딩 화면 처리 LoadingIndicator 추가   CSR ID : C20220411-000163
  2022.08.22  신광희      : 검사대상, 대상변경 컬럼 추가
  2023.03.14  유재기      : LOT COUNT -> 검사대상 COUNT,AUTO HOLD 기능 추가
  2023.03.21  유재기      : AUTO HOLD 기능 개선, Area 별 기능 적용
  2023.03.28  유재기      : AUTO HOLD 기능 개선, Area 별 기능 적용
  2024.08.19  안유수 E20240720-001675 사용자 설정 컬럼 기능 적용(UcBaseDataGrid 적용)
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_003_EQPTWIN.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_003_EQPTWIN : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]

        UserControl _UCParent = null;
        Util _Util = new Util();
        string LDR_LOT_IDENT_BAS_CODE = string.Empty;
        string UNLDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string sHoldCode = "";
        private string sReleaseDay = "";

        public IFrameOperation FrameOperation { get; set; }
        public ASSY004_003_EQPTWIN(UserControl uc)
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
            SetUCParent(uc);
        }

        private string Eqptname { get; set; } = string.Empty;

        private string Eqptid { get; set; } = string.Empty;

        private string Eleccheck { get; set; } = string.Empty;

        private string EqptElec { get; set; } = string.Empty;

        private string Eqsgid { get; set; } = string.Empty;

        private bool Finishflag { get; set; } = false;

        private string VdQaInspCond { get; set; } = string.Empty;

        private string QaType { get; set; } = string.Empty;

        private bool LotQAInsp { get; set; } = false;

        private string Area { get; set; } = string.Empty;

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

        public bool IsInspectionTarget { get; set; } = false;

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        #endregion

        #region[Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            //2019.03.05 RF_ID 여부 : RF_ID일 경우 CST 컬럼 보여줌
            if (_UNLDR_LOT_IDENT_BAS_CODE.Equals("RF_ID"))
            {
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Visible;
            }
            else
            {
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
            }

            if (UseCommoncodePlant())
            {
                dgFinishLot.Columns["CSTID"].Visibility = Visibility.Visible;
                dgFinishLot.Columns["SMPL_CLCT_CMPL_FLAG"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgFinishLot.Columns["CSTID"].Visibility.Equals(Visibility.Visible))
                {
                    // RF_ID 이므로 CSTID 변경하지 않는다.
                }
                else
                {
                    dgFinishLot.Columns["CSTID"].Visibility = Visibility.Collapsed;
                }

                dgFinishLot.Columns["SMPL_CLCT_CMPL_FLAG"].Visibility = Visibility.Collapsed;
            }

            ApplyPermissions();
        }

        //검사 버튼
        private void btnInspectionConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInsception())
                return;

            //QA_INSP_TRGT_FLAG이 Y인 LOT을 개별 검사하면 해당 Batch는 모두 LOT별 검사로 처리해야 하기 때문에 Confirm추가
            if (LotQAInsp && Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "QA_INSP_TRGT_FLAG")).Equals("Y"))
            {
                string[] p = new string[1];
                p[0] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));
                Util.MessageConfirm("SFU7022", (msg) =>
                {
                    if (msg == MessageBoxResult.OK)
                    {
                        InspectionConfirm();
                    }
                }, p);
            }
            else
            {
                InspectionConfirm();
            }
        }

        //확정 버튼
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
                        if (!LotQAInsp)
                        {
                            string selBatchNo = DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO") as string;
                            for (int j = i; j < dgFinishLot.GetRowCount(); j++)
                            {
                                if (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[j].DataItem, "EQPT_BTCH_WRK_NO")).Equals(selBatchNo))
                                {
                                    row2 = inLot.NewRow();
                                    row2["LOTID"] = DataTableConverter.GetValue(dgFinishLot.Rows[j].DataItem, "LOTID");
                                    inLot.Rows.Add(row2);
                                }
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
            }

            //검사확정 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3368"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    ShowLoadingIndicator();

                    new ClientProxy().ExecuteService_Multi("BR_RPD_REG_QA_CONFIRM_L", "INDATA, IN_LOT", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.AlertByBiz("BR_RPD_REG_QA_CONFIRM_L", bizException.Message, bizException.ToString());
                                return;
                            }

                            Util.MessageInfo("SFU3369");//확정완료

                            foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
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
                        finally
                        {
                            HiddenLoadingIndicator();
                        }
                    }, ds);
                }
            });
        }

        private void dgFinishLot_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dataGrid = sender as C1DataGrid;

            if (e.Cell.Presenter == null || dgFinishLot.GetRowCount() == 0)
            {
                return;
            }

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
                    else
                    {
                        string judgValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "JUDG_VALUE").GetString();
                        string QAValue = DataTableConverter.GetValue(dgFinishLot.Rows[e.Cell.Row.Index].DataItem, "QA_INSP_TRGT_FLAG").GetString();

                        if (string.IsNullOrEmpty(judgValue))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("black"));
                            return;
                        }

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

        private void dgFinishLot_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {

            if (e.Column.HeaderPresenter == null)
                return;

            /*
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        //전체체크하는 체크박스를 ColumnHeader에 삽입
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        //전체체크 활성, 비활성 이벤트 설정
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));
            */
        }

        private void dgFinishLot_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            /*
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
                            Util.MessageValidation("검사대기(두께불량) 먼저 판정해주세요");
                            return;
                        }
                    }
                }
            }));
            */
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
                int btchIdx = dgFinishLot.Columns["EQPT_BTCH_WRK_NO"].Index < 0 ? 7 : dgFinishLot.Columns["EQPT_BTCH_WRK_NO"].Index;
                int chkIdx = dgFinishLot.Columns["CHK"].Index < 0 ? 0 : dgFinishLot.Columns["CHK"].Index;

                int x1 = 0;

                for (int i = 0; i < dgFinishLot.GetRowCount(); i++)
                {
                    if (!tmp.Equals(Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[i].DataItem, "EQPT_BTCH_WRK_NO"))))
                    {
                        e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, btchIdx), dgFinishLot.GetCell((int)(i - 1), btchIdx)));
                        if (!LotQAInsp)
                            e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, chkIdx), dgFinishLot.GetCell((int)(i - 1), chkIdx)));
                        x1 = i;
                        tmp = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[x1].DataItem, "EQPT_BTCH_WRK_NO"));
                    }
                }

                e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, btchIdx), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), btchIdx)));
                if (!LotQAInsp)
                    e.Merge(new DataGridCellsRange(dgFinishLot.GetCell((int)x1, chkIdx), dgFinishLot.GetCell((int)(dgFinishLot.GetRowCount() - 1), chkIdx)));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgFinishLot_CurrentCellChanged(object sender, DataGridCellEventArgs e)
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

                                //row 색 바꾸기
                                dg.SelectedIndex = e.Cell.Row.Index;
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
                            break;
                    }

                    if (dg.CurrentCell != null)
                        dg.CurrentCell = dg.GetCell(dg.CurrentCell.Row.Index, dg.Columns.Count - 1);
                    else if (dg.Rows.Count > 0)
                        dg.CurrentCell = dg.GetCell(dg.Rows.Count, dg.Columns.Count - 1);
                }
            }
        }

        private void btnTargetChange_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGridCellPresenter presenter = ((DataGridCellPresenter)((FrameworkElement)((System.Windows.Input.MouseDevice)e.Device).Captured).Parent) as DataGridCellPresenter;
            if (presenter == null) return;

            string messageCode = "SFU2875";

            Util.MessageConfirm(messageCode, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    string equipmentBatchWorkNo = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "EQPT_BTCH_WRK_NO"));
                    string lotId = Util.NVC(DataTableConverter.GetValue(presenter.Row.DataItem, "LOTID"));

                    const string bizRuleName = "BR_PRD_REG_NND_SMPL_LOT_CHANGE_UI";
                    DataTable inDataTable = new DataTable("INDATA");
                    inDataTable.Columns.Add("EQPT_BTCH_WRK_NO", typeof(string));
                    inDataTable.Columns.Add("LOTID", typeof(string));

                    DataRow newRow = inDataTable.NewRow();
                    newRow["EQPT_BTCH_WRK_NO"] = equipmentBatchWorkNo;
                    newRow["LOTID"] = lotId;
                    inDataTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
                                {
                                    win.REFRESH = true;
                                    return;
                                }
                                //return;
                            }

                            //HOLD 처리
                            DataTable dtResult = presenter.DataGrid.GetDataTable();
                            LotHold(dtResult, equipmentBatchWorkNo, lotId);


                            //저장 되었습니다.
                            Util.MessageInfo("SFU1270");

                            foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
                            {
                                win.REFRESH = true;
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            //Util.MessageException(ex);
                            foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
                            {
                                win.REFRESH = true;
                                return;
                            }
                        }
                    });
                }
            });

            //catch (Exception)
            //{
            //    foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
            //    {
            //        win.REFRESH = true;
            //        return;
            //    }

            //}

        }

        private void wndQA_Closed(object sender, EventArgs e)
        {
            ASSY004_003_QAJUDG window = sender as ASSY004_003_QAJUDG;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SetGrid();

                foreach (ASSY004_003 win in Util.FindVisualChildren<ASSY004_003>(Application.Current.MainWindow))
                {
                    win.REFRESH = true;
                    return;
                }

                System.Diagnostics.Debug.WriteLine("--clear: {0} Bytes.", GC.GetTotalMemory(false));
            }
        }
        #endregion

        #region[Method]
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
            VdQaInspCond = Util.NVC(parameters[6]);
            QaType = Util.NVC(parameters[7]);
            IsInspectionTarget = (bool)parameters[8];
            Area = Util.NVC(parameters[9]);

            SetGrid();

            ApplyPermissions();
        }

        public void SetLotQAInsp()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters == null) return;

            LotQAInsp = (bool)parameters[0];

            DataTable dt = DataTableConverter.Convert(dgFinishLot.ItemsSource);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if ((dt.Rows[i]["CHK"] as bool?).HasValue && (dt.Rows[i]["CHK"] as bool?).Value)
                    dt.Rows[i]["CHK"] = false;
            }
            dgFinishLot.ItemsSource = DataTableConverter.Convert(dt);
            dgFinishLot.Refresh();
        }

        private void SetGrid()
        {
            tbEqptName.Text = Eqptname;
            tbLotCount.Text = "0";

            if (dgFinishLot.GetDataTable() != null)
            {
                DataTable dtMachineData = null;
                try {
                    dtMachineData = dgFinishLot.GetDataTable().Select("QA_INSP_TRGT_FLAG = 'Y'").CopyToDataTable();

                    if (dtMachineData != null && dtMachineData.Rows.Count > 0)
                    {
                        tbLotCount.Text = dtMachineData.Rows.Count.ToString();
                    }
                }
                catch
                {
                    // SELECT 값 없으므로 처리하지 않음.
                }
            }


            dgFinishLot.Visibility = Visibility.Visible;

            grTime.Visibility = Visibility.Collapsed;

            if (QaType == "Y")
            {
                btnInspectionConfirm.Visibility = Visibility.Collapsed;
                btnConfirm.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnInspectionConfirm.Visibility = Visibility.Visible;
                btnConfirm.Visibility = Visibility.Visible;
            }

            if (IsInspectionTarget)
            {
                dgFinishLot.Columns["QA_INSP_TRGT_FLAG"].Visibility = Visibility.Visible;
                targetChange.Visibility = Visibility.Visible;
            }
            else
            {
                dgFinishLot.Columns["QA_INSP_TRGT_FLAG"].Visibility = Visibility.Collapsed;
                targetChange.Visibility = Visibility.Collapsed;
            }

            CHK_FINISH.Visibility = Visibility.Visible;

            QATYPE_FINISH.Visibility = Visibility.Collapsed;
        }

        private bool ValidateInsception()
        {
            try
            {
                if (dgFinishLot.ItemsSource == null || _Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632"); // 선택된 LOT이 없습니다.
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

        private void SetUCParent(UserControl uc)
        {
            _UCParent = uc;
        }

        private DataTable GetSelectedLot()
        {
            DataTable lotTable = new DataTable();
            lotTable.Columns.Add("CBO_NAME");
            lotTable.Columns.Add("CBO_CODE");

            DataRow row = null;
            foreach (DataRowView drv in DataTableConverter.Convert(dgFinishLot.ItemsSource).DefaultView)
            {
                if ((DataTableConverter.GetValue(drv, "CHK") as bool?).HasValue && (DataTableConverter.GetValue(drv, "CHK") as bool?).Value)
                {
                    row = lotTable.NewRow();
                    row["CBO_NAME"] = DataTableConverter.GetValue(drv, "LOTID") as string;
                    row["CBO_CODE"] = DataTableConverter.GetValue(drv, "LOTID") as string;
                    lotTable.Rows.Add(row);
                }
            }
            return lotTable;
        }

        private bool CanInspection()
        {
            bool bRet = true;

            //if (!LotQAInsp) //LOT별 검사가 아니면 해당 배치에 있는 모든 랏들의 WIPSTAT 체크
            //{
            //DataTable dt = DataTableConverter.Convert(dgFinishLot.ItemsSource);
            //string btch = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));
            //foreach (DataRowView drv in dt.DefaultView)
            //{
            //    if ((DataTableConverter.GetValue(drv, "EQPT_BTCH_WRK_NO") as string).Equals(btch))
            //    {
            //        if(!(DataTableConverter.GetValue(drv, "WIPSTAT") as string).Equals("END"))
            //        {
            //            bRet = false;
            //            break;
            //        }
            //    }
            //}
            //}
            //else //LOT별 검사면 선택된 것들만 WIPSTAT 체크
            //{
            //    DataTable dt = DataTableConverter.Convert(dgFinishLot.ItemsSource);
            //    foreach (DataRowView drv in dt.DefaultView)
            //    {
            //        if ((DataTableConverter.GetValue(drv, "CHK") as bool?).HasValue && (DataTableConverter.GetValue(drv, "CHK") as bool?).Value)
            //        {
            //            if (!(DataTableConverter.GetValue(drv, "WIPSTAT") as string).Equals("END"))
            //            {
            //                bRet = false;
            //                break;
            //            }
            //        }
            //    }
            //}
            return bRet;
        }

        private void InspectionConfirm()
        {
            ASSY004_003_QAJUDG wndQA = new ASSY004_003_QAJUDG();
            wndQA.FrameOperation = FrameOperation;

            if (wndQA != null)
            {
                object[] Parameters = new object[11];
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID"));
                Parameters[1] = "";//Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE"));
                Parameters[2] = Eqptid;
                Parameters[3] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "LOTID_RT"));
                Parameters[4] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQPT_BTCH_WRK_NO"));
                Parameters[5] = Eqsgid;
                Parameters[6] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "VD_QA_INSP_COND_CODE"));
                Parameters[7] = LotQAInsp == false ? (Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "JUDG_VALUE")).Equals("WAIT_RE_VD") ? true : false) : LotQAInsp; //2017.08.13 Add By Kim Joonphil
                Parameters[8] = GetSelectedLot();
                Parameters[9] = CanInspection();
                Parameters[10] = Util.NVC(DataTableConverter.GetValue(dgFinishLot.Rows[_Util.GetDataGridCheckFirstRowIndex(dgFinishLot, "CHK")].DataItem, "EQGRID"));

                C1WindowExtension.SetParameters(wndQA, Parameters);

                wndQA.Closed += new EventHandler(wndQA_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                this.Dispatcher.BeginInvoke(new Action(() => wndQA.ShowModal()));
                wndQA.BringToFront();
            }
        }

        private bool UseCommoncodePlant()
        {
            sHoldCode = "";
            sReleaseDay = "";

            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "VD_QA_TRGT_CHG_AUTO_HOLD_AREA";
            dr["CMCODE"] = Area;

            inTable.Rows.Add(dr);

            DataTable dtRslt = null;
            try
            {
                dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception)
            {
                // null 처리
            }

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                sHoldCode = dtRslt.Rows[0].GetValue("ATTRIBUTE1").ToString();
                sReleaseDay = dtRslt.Rows[0].GetValue("ATTRIBUTE2").ToString();

                return true;
            }
            else
            {
                return false;
            }
        }

        private void LotHold(DataTable dtResult, string sBatchLotId, string sLotId)
        {
            if (UseCommoncodePlant() == false)
                return;

            if (sHoldCode.IsNullOrEmpty())
                return;

            DateTime dtNow = GetCurrentTime();

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACTION_USERID", typeof(string));
            inDataTable.Columns.Add("CALDATE", typeof(string));

            DataRow row = null;

            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["LANGID"] = LoginInfo.LANGID;
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["ACTION_USERID"] = "";
            row["CALDATE"] = dtNow.ToShortDateString();

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("HOLD_NOTE", typeof(string));
            inLot.Columns.Add("RESNCODE", typeof(string));
            inLot.Columns.Add("HOLD_CODE", typeof(string));
            inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
            inLot.Columns.Add("SPCL_HOLD_STCK_FLAG", typeof(string));

            //라벨 발행용
            DataRow row1 = null;
            DataTable dtLabel = new DataTable();
            dtLabel.Columns.Add("LOTID", typeof(string));
            dtLabel.Columns.Add("RESNNAME", typeof(string));
            dtLabel.Columns.Add("MODELID", typeof(string));
            dtLabel.Columns.Add("WIPQTY", typeof(string));
            dtLabel.Columns.Add("PERSON", typeof(string));

            DataTable dtBatchLotList = null;
            try
            {
                dtBatchLotList = dtResult.Select(String.Format("EQPT_BTCH_WRK_NO = '{0}'", sBatchLotId)).CopyToDataTable();
            } catch
            {
                // null 처리
            }

            if(dtBatchLotList != null)
            {
                for (int i = 0; i < dtBatchLotList.Rows.Count; i++)
                {
                    string sRowLotId = dtBatchLotList.Rows[i].GetValue("LOTID").ToString();

                    if (sLotId.Equals(sRowLotId))
                    {
                        break;
                    }
                    else
                    {
                        int iAddDays = 30;
                        if (sReleaseDay.IsNullOrEmpty() == false)
                        {
                            if(sReleaseDay.IsNumeric())
                            {
                                iAddDays = Int32.Parse(sReleaseDay);
                            }
                        }

                        inLot.Rows.Clear();

                        row = inLot.NewRow();
                        row["LOTID"] = sRowLotId;
                        row["HOLD_NOTE"] = MessageDic.Instance.GetMessage("SFU8545");
                        row["RESNCODE"] = sHoldCode;
                        row["HOLD_CODE"] = sHoldCode;
                        row["UNHOLD_SCHD_DATE"] = dtNow.AddDays(iAddDays).ToString("yyyMMdd");
                        row["SPCL_HOLD_STCK_FLAG"] = "";

                        inLot.Rows.Add(row);

                        try
                        {
                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, inData);
                        }
                        catch (Exception ex)
                        {
                            // HOLD CHECK 에러 발생 (이미 HOLD걸려있음)  다음 LOT HOLD
                        }

                    }
                }
            }
        }

        private DateTime GetCurrentTime()
        {
            try
            {
                DataTable dt = new ClientProxy().ExecuteServiceSync("BR_CUS_GET_SYSTIME", null, "OUTDATA", null);
                return (DateTime)dt.Rows[0]["SYSTIME"];
            }
            catch (Exception ex) { }

            return DateTime.Now;
        }
        #endregion

        #region [Init & Util Method]
        public static class GridBackColor
        {
            public static readonly System.Drawing.Color FAIL = System.Drawing.Color.FromArgb(255, 105, 105); //FAIL
            public static readonly System.Drawing.Color PASS = System.Drawing.Color.FromArgb(169, 209, 142); // PASS
            public static readonly System.Drawing.Color WAIT = System.Drawing.Color.FromArgb(255, 192, 0); //WAIT
            public static readonly System.Drawing.Color HOLD = System.Drawing.Color.FromArgb(89, 89, 89); //HOLD
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

            tbLotCount.Text = string.Empty;
        }
        #endregion

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;

        }


    }
}
