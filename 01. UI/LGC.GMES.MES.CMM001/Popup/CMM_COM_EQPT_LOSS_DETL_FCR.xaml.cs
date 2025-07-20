/*************************************************************************************
 Created Date : 2024.03.04
      Creator : 이병윤
   Decription : 상호참조불가능으로 COM001_014_LOSS_DETL_FCR.xaml[집합번호: 60386],
                                   COM001_014_LOSS_DETL_FCR.xaml.cs[집합번호 : 58946]
                해당파일을 복사해서 처리
--------------------------------------------------------------------------------------
 [Change History]
  2024.03.04  이병윤 : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_COM_EQPT_LOSS_DETL_FCR : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        string _AREAID = string.Empty;
        string _PROCID = string.Empty;
        string _EQPTID = string.Empty;
        string _LOSSCODE = string.Empty; // 2022.08.23 YJH

        string LossCode = string.Empty;
        string LossDetlCode = string.Empty;
        string LossDetlName = string.Empty;
        string CausedEqptChk = string.Empty;  // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
        string _OCCUREQPT = string.Empty;  // 2023.05.30 윤지해 CSR ID E20230330-001442 추가

        // 2023.07.18 윤지해 CSR ID E20230703-000158 추가
        string FailCode = string.Empty;
        string CauseCode = string.Empty;
        string ResolCode = string.Empty;
        string FailName = string.Empty;
        string CauseName = string.Empty;
        string ResolName = string.Empty;
        string[] sFCR = new string[3];

        public CMM_COM_EQPT_LOSS_DETL_FCR()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string _LOSS_CODE
        {
            set { LossCode = value; }
            get { return LossCode; }
        }
        public string _LOSS_DETL_CODE
        {
            set { LossDetlCode = value; }
            get { return LossDetlCode; }
        }
        public string _LOSS_DETL_NAME
        {
            set { LossDetlName = value; }
            get { return LossDetlName; }
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 추가
        public string _FAIL_CODE
        {
            set { FailCode = value; }
            get { return FailCode; }
        }

        public string _CAUSE_CODE
        {
            set { CauseCode = value; }
            get { return CauseCode; }
        }

        public string _RESOL_CODE
        {
            set { ResolCode = value; }
            get { return ResolCode; }
        }

        public string _FAIL_NAME
        {
            set { FailName = value; }
            get { return FailName; }
        }

        public string _CAUSE_NAME
        {
            set { CauseName = value; }
            get { return CauseName; }
        }

        public string _RESOL_NAME
        {
            set { ResolName = value; }
            get { return ResolName; }
        }
        #endregion

        #region Initialize


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AREAID = Util.NVC(tmps[0]);
                _PROCID = Util.NVC(tmps[1]);
                _EQPTID = Util.NVC(tmps[2]);
                _LOSSCODE = Util.NVC(tmps[3]);
                CausedEqptChk = Util.NVC(tmps[4]);  // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                _OCCUREQPT = Util.NVC(tmps[5]);     // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
            }

            SetDataGrid();
        }

        private void SetDataGrid()
        {
            try
            {
                // 2023.07.18 윤지해 CSR ID E20230703-000158 - 초기화
                txtLossDetlCode.Text = null;
                txtLossDetlName.Text = null;
                txtLossCode.Text = null;
                txtFailCode.Text = null;
                txtFailName.Text = null;
                txtCauseCode.Text = null;
                txtCauseName.Text = null;
                txtResolCode.Text = null;
                txtResolName.Text = null;
                Util.gridClear(dgFive);
                Util.gridClear(dgSix);
                Util.gridClear(dgSeven);

                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("LOSSCODE", typeof(string));
                dt.Columns.Add("LOSS_DETL_NAME_PART", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("OCCUREQPTFLAG", typeof(string));    // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                dt.Columns.Add("CAUSE_EQPTID", typeof(string));    // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                dt.Columns.Add("TMP_IDX", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = _AREAID;
                row["PROCID"] = _PROCID;
                row["EQPTID"] = _EQPTID;
                row["LOSSCODE"] = _LOSSCODE;
                row["LOSS_DETL_NAME_PART"] = txtLossFilter.Text.Length == 0 ? "" : txtLossFilter.Text;
                row["OCCUREQPTFLAG"] = CausedEqptChk;   // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                row["CAUSE_EQPTID"] = _OCCUREQPT;   // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                row["USERID"] = LoginInfo.USERID;

                dt.Rows.Add(row);

                // 2023.05.30 윤지해 CSR ID E20230330-001442 biz 조건에 따라 분할하여 LOSS LV3 LIST-UP
                string bizRuleName = "DA_EQP_SEL_EQPTLOSS_LOSS_DETL_CODE";

                if (CausedEqptChk.Equals("Y") && !_OCCUREQPT.Equals(""))
                {
                    bizRuleName = "DA_EQP_SEL_EQPTLOSS_OCCUREQP_LOSS_DETL_CODE";
                }
                else
                {
                    bizRuleName = "DA_EQP_SEL_EQPTLOSS_LOSS_DETL_CODE";
                }

                DataTable tmpResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
                if (tmpResult.Rows.Count == 0) { return; }

                DataTable result = SetDataTable(tmpResult);

                dgOne.ItemsSource = result.Select("UPPR_LOSS_CODE = 30000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 30000").CopyToDataTable()); //비가동
                dgTwo.ItemsSource = result.Select("UPPR_LOSS_CODE = 20000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 20000").CopyToDataTable()); //비부하
                dgThree.ItemsSource = result.Select("UPPR_LOSS_CODE = 10000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 10000").CopyToDataTable()); //비조업
                // 2023.07.18 윤지해 CSR ID E20230703-000158 FCR 코드 입력 방식 개선 - 미사용 컬럼 주석처리
                //dgFour.ItemsSource = result.Select("UPPR_LOSS_CODE = 40000").Length == 0 ? null : DataTableConverter.Convert(result.Select("UPPR_LOSS_CODE = 40000").CopyToDataTable());//무효가동

                #region C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                // 즐겨찾기 dgZero 생성
                dt.Clear();
                row.Delete();

                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = _AREAID;
                row["PROCID"] = _PROCID;
                row["EQPTID"] = _EQPTID;
                row["LOSSCODE"] = _LOSSCODE; // 2022.08.23 YJH
                row["LOSS_DETL_NAME_PART"] = txtLossFilter.Text.Length == 0 ? "" : txtLossFilter.Text; // 2022.08.23 YJH
                row["OCCUREQPTFLAG"] = CausedEqptChk;   // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                row["CAUSE_EQPTID"] = _OCCUREQPT;   // 2023.05.30 윤지해 CSR ID E20230330-001442 추가
                row["USERID"] = LoginInfo.USERID; // 2022.08.23 YJH
                row["TMP_IDX"] = "2"; // 2022.08.23 YJH

                dt.Rows.Add(row);

                // 2023.07.03 윤지해 CSR ID E20230330-001442 즐겨찾기 조회 비즈 수정
                DataTable tmpFavResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", dt);
                if (tmpFavResult.Rows.Count == 0) { return; }

                DataTable favResult = SetDataTable(tmpFavResult);
                dgZero.ItemsSource = favResult.Select("TMP_IDX = 2").Length == 0 ? null : DataTableConverter.Convert(favResult.Select("TMP_IDX = 2").CopyToDataTable()); //즐겨찾기

                #endregion
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
        private void dgZero_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);
        }

        private void dgOne_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);
        }

        private void dgTwo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);
        }

        private void dgThree_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);
        }

        private void SetRowColor(C1.WPF.DataGrid.C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                // C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TMP_IDX")).Equals("2"))
                {
                    // 즐겨찾기 설정된 내역
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FAF0F0"));
                }

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"))))
                {
                    // Loss lv2
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6699"));
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFFFFF"));
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#000000"));
                }
            }));
        }

        private DataTable SetDataTable(DataTable tmpResult)
        {
            DataTable result = new DataTable();
            result.Columns.Add("UPPR_LOSS_CODE", typeof(string));
            result.Columns.Add("LOSS_CODE", typeof(string));
            result.Columns.Add("LOSS_DETL_CODE", typeof(string));
            result.Columns.Add("LOSS_DETL_NAME", typeof(string));
            result.Columns.Add("TMP_IDX", typeof(string));

            int diffIndex = 0;
            DataRow row = null;
            string pre_loss_code = "";

            while (true)
            {

                row = result.NewRow();

                row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["UPPR_LOSS_CODE"]);
                row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_NAME"]);
                row["TMP_IDX"] = Convert.ToString(tmpResult.Rows[diffIndex]["TMP_IDX"]);
                result.Rows.Add(row);

                pre_loss_code = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);

                for (int i = diffIndex; i < tmpResult.Rows.Count; i++)
                {

                    if (!Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]).Equals(pre_loss_code))
                    {
                        diffIndex = i;
                        break;
                    }
                    row = result.NewRow();

                    row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[i]["UPPR_LOSS_CODE"]);
                    row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]);
                    row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[i]["LOSS_DETL_CODE"]);
                    row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[i]["LOSS_DETL_NAME"]);
                    row["TMP_IDX"] = Convert.ToString(tmpResult.Rows[i]["TMP_IDX"]);

                    result.Rows.Add(row);
                    pre_loss_code = Convert.ToString(tmpResult.Rows[i]["LOSS_CODE"]);

                    if (i == tmpResult.Rows.Count - 1)
                    {
                        return result;
                    }

                }
            }
        }

        // 미사용
        private void dgOne_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;


            int index = (sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.Index;
            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[index].DataItem, "LOSS_DETL_CODE"))))
            {
                return;
            }

            _LOSS_CODE = Util.NVC(DataTableConverter.GetValue((sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.DataItem, "LOSS_CODE"));
            _LOSS_DETL_CODE = Util.NVC(DataTableConverter.GetValue((sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.DataItem, "LOSS_DETL_CODE"));
            _LOSS_DETL_NAME = Util.NVC(DataTableConverter.GetValue((sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.DataItem, "LOSS_DETL_NAME"));

            this.DialogResult = MessageBoxResult.OK;

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) { return; }
            this.DialogResult = MessageBoxResult.OK;
        }

        // C20220627-000350 부동내역명 필터를 위한 돋보기 버튼 클릭
        private void btnLossFilter_Click(object sender, RoutedEventArgs e)
        {
            SetDataGrid();
        }

        // C20220627-000350 key event(enter 버튼 클릭)
        private void txtLossFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetDataGrid();
            }
        }

        // C20220627-000350 즐겨찾기 
        private void imgFav_Click(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;

            if (img.DataContext == null) return;

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4947"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("USERID", typeof(string));
                        dt.Columns.Add("LOSSCODE", typeof(string));
                        dt.Columns.Add("LOSS_DETL_CODE", typeof(string));
                        dt.Columns.Add("USE_FLAG", typeof(string));

                        DataRow row = dt.NewRow();
                        row["USERID"] = LoginInfo.USERID;
                        row["LOSSCODE"] = (img.DataContext as DataRowView).Row["LOSS_CODE"].ToString();
                        row["LOSS_DETL_CODE"] = (img.DataContext as DataRowView).Row["LOSS_DETL_CODE"].ToString();
                        row["USE_FLAG"] = img.Source.ToString().Contains("unselected") ? 'Y' : 'N';

                        dt.Rows.Add(row);

                        DataTable tmpResult = new ClientProxy().ExecuteServiceSync("DA_EQP_INS_EQPTLOSS_LOSS_DETL_CODE_USER_BMK", "RQSTDT", "RSLTDT", dt);

                        SetDataGrid();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        // C20220627-000350 즐겨찾기 이미지 세팅
        private void SetImage(C1.WPF.DataGrid.C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Image img = e.Cell?.Presenter?.Content as Image;
                    Uri uriSource = null;

                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"))))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "TMP_IDX")).Equals("2"))
                        {
                            uriSource = new Uri(@"/LGC.GMES.MES.COM001;component/Images/icon_favorite.png", UriKind.Relative);
                        }
                        else
                        {
                            uriSource = new Uri(@"/LGC.GMES.MES.COM001;component/Images/icon_favorite_unselected.png", UriKind.Relative);
                        }
                    }

                    if (img != null && uriSource != null)
                    {
                        img.Visibility = Visibility.Visible;
                        img.Source = new BitmapImage(uriSource);
                    }
                    else if (img != null && uriSource == null)
                    {
                        img.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    Util.Alert(ex.Message);
                }
            }));
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 초기화 이벤트 추가 - 부동내용
        private void btnRunCancelLv3_Click(object sender, RoutedEventArgs e)
        {
            SetDataGrid();
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 초기화 이벤트 추가 - 현상
        private void btnRunCancelFail_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgFive);
            Util.gridClear(dgSix);
            Util.gridClear(dgSeven);
            sFCR[0] = string.Empty;
            sFCR[1] = string.Empty;
            sFCR[2] = string.Empty;
            GetFcrList("F", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 초기화 이벤트 추가 - 원인
        private void btnRunCancelCause_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSix);
            Util.gridClear(dgSeven);
            sFCR[1] = string.Empty;
            sFCR[2] = string.Empty;
            GetFcrList("C", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 초기화 이벤트 추가 - 조치
        private void btnRunCancelResol_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgSeven);
            sFCR[2] = string.Empty;
            GetFcrList("R", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - 부동내용 선택 이벤트
        private void dataGrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;
            // 선택된 그리드에 데이터가 없을 때
            if (datagrid.Rows.Count() < 1) return;

            int tmpIndex = (sender as C1.WPF.DataGrid.C1DataGrid).CurrentCell.Row.Index;

            // 선택된 그리드에 데이터가 있지만 선택된 row 자체에 데이터가 없을 때
            if (tmpIndex < 1) return;

            // 부동내용 내 Loss Lv2(소제목)을 선택했을 때
            if (Util.NVC(DataTableConverter.GetValue(datagrid.Rows[tmpIndex].DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(datagrid.Rows[tmpIndex].DataItem, "LOSS_DETL_CODE"))))
            {
                return;
            }

            // 초기화
            txtLossDetlCode.Text = null;
            txtLossDetlName.Text = null;
            txtLossCode.Text = null;

            dgZero.SelectedItem = null;
            dgOne.SelectedItem = null;
            dgTwo.SelectedItem = null;
            dgThree.SelectedItem = null;

            Util.gridClear(dgFive);
            Util.gridClear(dgSix);
            Util.gridClear(dgSeven);

            sFCR[0] = string.Empty;
            sFCR[1] = string.Empty;
            sFCR[2] = string.Empty;

            datagrid.SelectedIndex = tmpIndex;

            txtLossDetlCode.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "LOSS_DETL_CODE"));
            txtLossDetlName.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "LOSS_DETL_NAME"));
            txtLossCode.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "LOSS_CODE"));

            // 현상(F) List-up
            GetFcrList("F", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - 현상(F) 선택변경 이벤트
        private void dgFive_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;

            txtFailCode.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_CODE"));
            txtFailName.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_NAME"));
            txtCauseCode.Text = string.Empty;
            txtCauseName.Text = string.Empty;
            txtResolCode.Text = string.Empty;
            txtResolName.Text = string.Empty;

            // 원인(C) List-up
            Util.gridClear(dgSix);
            Util.gridClear(dgSeven);
            sFCR[0] = txtFailCode.Text;
            sFCR[1] = string.Empty;
            GetFcrList("C", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - 원인(C) 선택변경 이벤트
        private void dgSix_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;

            txtCauseCode.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_CODE"));
            txtCauseName.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_NAME"));
            txtResolCode.Text = string.Empty;
            txtResolName.Text = string.Empty;

            // 원인(C) List-up
            Util.gridClear(dgSeven);
            sFCR[1] = txtCauseCode.Text;
            GetFcrList("R", sFCR);
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - 조치(R) 선택변경 이벤트
        private void dgSeven_SelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = sender as C1.WPF.DataGrid.C1DataGrid;

            txtResolCode.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_CODE"));
            txtResolName.Text = Util.NVC(DataTableConverter.GetValue(datagrid.SelectedItem, "CBO_NAME"));
        }

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - 최종 선택 이벤트
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (Util.IsNVC(txtLossCode.Text) && Util.IsNVC(txtLossDetlCode.Text) && Util.IsNVC(txtLossDetlName.Text))
            {
                Util.MessageInfo("SFU3631");    // 부동내용을 입력하세요.
                return;
            }
            _LOSS_CODE = txtLossCode.Text;
            _LOSS_DETL_CODE = txtLossDetlCode.Text;
            _LOSS_DETL_NAME = txtLossDetlName.Text;
            _FAIL_CODE = txtFailCode.Text;
            _FAIL_NAME = txtFailName.Text;
            _CAUSE_CODE = txtCauseCode.Text;
            _CAUSE_NAME = txtCauseName.Text;
            _RESOL_CODE = txtResolCode.Text;
            _RESOL_NAME = txtResolName.Text;

            this.DialogResult = MessageBoxResult.OK;
        }
        #endregion

        #region Method

        // 2023.07.18 윤지해 CSR ID E20230703-000158 - FCR List 조회
        private void GetFcrList(string fcrTypeCode, string[] sFCRCode)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("LOSS_CODE", typeof(string));
                INDATA.Columns.Add("LOSS_DETL_CODE", typeof(string));
                INDATA.Columns.Add("FCR_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("FAIL_CODE", typeof(string));
                INDATA.Columns.Add("CAUSE_CODE", typeof(string));
                INDATA.Columns.Add("RESOL_CODE", typeof(string));
                INDATA.Columns.Add("OCCUREQPTFLAG", typeof(string));
                INDATA.Columns.Add("EQPTID", typeof(string));
                INDATA.Columns.Add("CAUSE_EQPTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = _PROCID;
                dr["LOSS_CODE"] = txtLossCode.Text;
                dr["LOSS_DETL_CODE"] = txtLossDetlCode.Text;
                dr["FCR_TYPE_CODE"] = fcrTypeCode.ToString();
                dr["FAIL_CODE"] = Util.ConvertEmptyToNull(sFCRCode[0]);
                dr["CAUSE_CODE"] = Util.ConvertEmptyToNull(sFCRCode[1]);
                dr["RESOL_CODE"] = Util.ConvertEmptyToNull(sFCRCode[2]);
                dr["OCCUREQPTFLAG"] = CausedEqptChk;
                dr["EQPTID"] = _EQPTID;
                dr["CAUSE_EQPTID"] = _OCCUREQPT;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_FCRCODE_LOSS_LV3", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count == 0)
                {
                    return;
                }

                if (fcrTypeCode.Equals("F"))
                {
                    Util.GridSetData(dgFive, dtResult, FrameOperation, false);
                }
                else if (fcrTypeCode.Equals("C"))
                {
                    Util.GridSetData(dgSix, dtResult, FrameOperation, false);
                }
                else
                {
                    Util.GridSetData(dgSeven, dtResult, FrameOperation, false);
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

    }
}
