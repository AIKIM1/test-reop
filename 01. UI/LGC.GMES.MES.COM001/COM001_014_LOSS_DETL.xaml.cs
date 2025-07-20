/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2022.08.23 윤지해 CSR ID C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
  2023.05.28 윤지해 CSR ID E20230330-001442 조건에 따라 BM/PD 출력 로직 수정
  2023.07.03 윤지해 CSR ID E20230330-001442 즐겨찾기 조회 비즈 수정
  2023.07.18 윤지해 CSR ID E20230703-000158 무효가동 관련 로직 삭제

 
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_014_LOSS_DETL : C1Window, IWorkArea
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

        public COM001_014_LOSS_DETL()
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

            //DataTable dt = new DataTable();
            //dt.Columns.Add("UPPR_LOSS_CODE", typeof(string));
            //dt.Columns.Add("LOSS_CODE", typeof(string));
            //dt.Columns.Add("LOSS_DETL_CODE", typeof(string));
            //dt.Columns.Add("LOSS_DETL_NAME", typeof(string));

            //DataRow row = dt.NewRow();
            //row["UPPR_LOSS_CODE"] = "3000";
            //row["LOSS_CODE"] = "EL001";
            //row["LOSS_DETL_CODE"] = "TEST1";
            //row["LOSS_DETL_NAME"] = "TEST1";

            //dt.Rows.Add(row);

            //dgOne.ItemsSource = DataTableConverter.Convert(dt);

            //DataTable dt2 = new DataTable();
            //dt2.Columns.Add("UPPR_LOSS_CODE", typeof(string));
            //dt2.Columns.Add("LOSS_CODE", typeof(string));
            //dt2.Columns.Add("LOSS_DETL_CODE", typeof(string));
            //dt2.Columns.Add("LOSS_DETL_NAME", typeof(string));

            //DataRow row2 = dt2.NewRow();
            //row2["UPPR_LOSS_CODE"] = "2000";
            //row2["LOSS_CODE"] = "EL002";
            //row2["LOSS_DETL_CODE"] = "TEST2";
            //row2["LOSS_DETL_NAME"] = "TEST2";

            //dt2.Rows.Add(row2);

            //dgTwo.ItemsSource = DataTableConverter.Convert(dt2);

            SetDataGrid();
        }

        private void SetDataGrid()
        {
            try
            {
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

                if(CausedEqptChk.Equals("Y") && !_OCCUREQPT.Equals(""))
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

                // 2023.07.18 윤지해 CSR ID E20230703-000158 무효가동 관련 로직 삭제
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

        #region C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
        private void dgZero_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);
        }
        #endregion
        private void dgOne_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            SetRowColor(dataGrid, e);
            SetImage(dataGrid, e);

            //dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (e.Cell.Presenter == null)
            //    {
            //        return;
            //    }

            //    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"))))
            //    {
            //        //색깔바꾸기
            //        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
            //    }
            //}));
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

        // 2023.07.18 윤지해 CSR ID E20230703-000158 무효가동 관련 로직 삭제
        //private void dgFour_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        //{
        //    C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
        //    SetRowColor(dataGrid, e);
        //    SetImage(dataGrid, e);
        //}

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
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("lavenderblush"));
                }

                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_CODE")).Equals(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "LOSS_DETL_CODE"))))
                {
                    //색깔바꾸기
                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("pink"));
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

            //row = result.NewRow();
            //row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[0]["UPPR_LOSS_CODE"]);
            //row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[0]["LOSS_CODE"]);
            //row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[0]["LOSS_CODE"]);
            //row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[0]["LOSS_NAME"]);
            //result.Rows.Add(row);

            //pre_loss_code = Convert.ToString(tmpResult.Rows[0]["LOSS_CODE"]);

            while (true)
            {
                //if (diffIndex != 0)
                //{
                    row = result.NewRow();

                    row["UPPR_LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["UPPR_LOSS_CODE"]);
                    row["LOSS_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                    row["LOSS_DETL_CODE"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);
                    row["LOSS_DETL_NAME"] = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_NAME"]);
                    row["TMP_IDX"] = Convert.ToString(tmpResult.Rows[diffIndex]["TMP_IDX"]);
                    result.Rows.Add(row);

                    pre_loss_code = Convert.ToString(tmpResult.Rows[diffIndex]["LOSS_CODE"]);

            //    }


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

        #region C20220627-000350 [생산PI] GMES 시스템 개선을 통한 PD Loss Code 입력 편의성 개선 건
        // 부동내역명 필터를 위한 돋보기 버튼 클릭
        private void btnLossFilter_Click(object sender, RoutedEventArgs e)
        {
            SetDataGrid();
        }

        // key event(enter 버튼 클릭)
        private void txtLossFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SetDataGrid();
            }
        }

        // 즐겨찾기 
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

        // 즐겨찾기 이미지 세팅
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
        #endregion

        #endregion
    }
}
