/*************************************************************************************
 Created Date : 2022.03.16
      Creator : KDH
   Decription : Tray특별관리

--------------------------------------------------------------------------------------
 [Change History]
  2022.03.16  DEVELOPER : Initial Created.
  2022.07.12  조영대    : 최대 로우수 200 Validation 수정   
  2023.02.13  주훈종    : [E20230213-000118] 특별관리등록 기존 200개 제한에서 1000개 처리로 변경, 200개씩 나눠서 비즈룰 호출하는 방식으로 변경
  2023.09.20  조영대    : IWorkArea 추가
  2023.12.05  손동혁    : 특별 관리 등록 된 트레이 특별변경 가능하게 수정
  2024.05.03  임정훈    : 특별관리등록 버튼 특별관리로 변경 및 클릭 시 팝업창 메시지 변경
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// </summary>
    public partial class FCS001_123 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        int addRows;
        int iBadCnt;
        int iCurrCnt;
        public FCS001_123()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region [Method]
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();

                int rowCount = int.Parse(txtRowCntCell.Value.ToString());

                if (Math.Abs(rowCount) > 0)
                {
                    if (rowCount + dg.Rows.Count > 1000) //E20230213-000118
                    {
                        // 최대 ROW수는 200입니다.
                        //Util.MessageValidation("SFU8338");
                        Util.MessageValidation("SFU5015", "1000");
                        return;
                    }
                    else
                    {
                        addRows = rowCount;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel()
        {
            DataTable dtInfo = DataTableConverter.Convert(dgCellList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("CSTID", typeof(string));
                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["CSTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dgCellList, dataTable, FrameOperation);
                }
            }
        }

        private void GetDataFromGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            string sTrayID = null;
            for (int iRow = 0; iRow < dgCellList.Rows.Count; iRow++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "CSTID")) == string.Empty)
                    continue;

                sTrayID += DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "CSTID") + ",";
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("LANGID", typeof(string));
            dtRqst.Columns.Add("TRAY_LIST", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["TRAY_LIST"] = sTrayID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_SPECIAL_INFO", "RQSTDT", "RSLTDT", dtRqst);
            Util.GridSetData(dgCellList, dtRslt, this.FrameOperation, false);

            iBadCnt = 0;
            for (int i = 0; i < dgCellList.Rows.Count; i++)
            {
                if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SPCL_FLAG")))
                     && !Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SPCL_FLAG")).Equals("N"))
                {
                    iBadCnt++;
                }
            }

            iCurrCnt = dgCellList.Rows.Count;
            txtCellSum.Text = iCurrCnt.ToString();
            txtInfoErr.Text = iBadCnt.ToString();
        }
        #endregion

        #region [Event]
        // 체크박스 관련
        #region
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

        private void txtRowCntCell_KeyDown(object sender, KeyEventArgs e)
        {
            if ((!string.IsNullOrEmpty(txtRowCntCell.Value.ToString())) && (e.Key == Key.Enter))
            {
                btnClear_Click(null, null);
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellList);
            iBadCnt = 0;
            txtCellSum.Text = string.Empty;
            txtInfoErr.Text = string.Empty;
            DataGridRowAdd(dgCellList);
            dgCellList.Columns["CSTID"].IsReadOnly = false;
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel();
            GetDataFromGrid(dgCellList);
        }
        private void btnSpecial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sTrayList = string.Empty;


                for (int i = 0; i < dgCellList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CHK")).Equals("True") ||
                        Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CHK")).Equals("1"))
                    {
                        sTrayList += DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "CSTID") + "|";
                    }
                }

                if (sTrayList.Length == 0)
                {
                    //Tray를 선택해주세요.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("FM_ME_0081"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        { }
                    });
                    return;
                }
                else if (dgCellList.Rows.Count > 1000)//E20230213-000118
                {
                    // 최대 ROW수는 200입니다.
                    Util.MessageValidation("SFU5015", "1000");
                    return;
                }

                Util.MessageConfirm("SFU9219", (result) =>  //특별관리 등록 시 단 적재 된 Tray도 같이 등록 됩니다. 
                {
                    if (result == MessageBoxResult.OK)
                    {
                        FCS001_021_SPECIAL_MANAGEMENT specialManagement = new FCS001_021_SPECIAL_MANAGEMENT();
                        specialManagement.FrameOperation = FrameOperation;

                        if (specialManagement != null)
                        {
                            object[] Parameters = new object[2];

                            Parameters[0] = sTrayList.Substring(0, sTrayList.Length - 1);
                            Parameters[1] = "Y"; //적재된 Tray 특별관리 진행

                            C1WindowExtension.SetParameters(specialManagement, Parameters);
                            specialManagement.Closed += new EventHandler(specialManagement_Closed);

                            this.Dispatcher.BeginInvoke(new Action(() => specialManagement.ShowModal()));
                            specialManagement.BringToFront();
                        }
                    }
                    else
                    {
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void specialManagement_Closed(object sender, EventArgs e)
        {
            FCS001_021_SPECIAL_MANAGEMENT window = sender as FCS001_021_SPECIAL_MANAGEMENT;

            //if (window.DialogResult == MessageBoxResult.Yes)
            //{
            //    this.btnSearch_Click(null, null);
            //}
            if (window.sResultReturn == "Y")
            {
                this.btnSearch_Click(null, null);
            }

            this.grdMain.Children.Remove(window);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromGrid(dgCellList);
        }

        private void dgCellList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    ////////////////////////////////////////////  default 색상 및 Cursor
                    e.Cell.Presenter.Cursor = Cursors.Arrow;

                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontSize = 12;
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    ///////////////////////////////////////////////////////////////////////////////////

                    if (!string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG"))))
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "SPCL_FLAG")).Equals("N"))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = true;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                        }
                        else
                        {
                            //dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = false;
                            dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = true;
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                        }
                    }
                    else
                    {
                        dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = true;
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Transparent);
                    }
                }
            }));
        }

        private void dgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgCellList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        private void dgCellList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

                    if (e.Column.Name.Equals("CSTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }

                }
            }));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                //if (string.IsNullOrEmpty(Util.NVC(dr["SPCL_FLAG"])) || Util.NVC(dr["SPCL_FLAG"]).Equals("N"))
                //{
                    dr["CHK"] = true;
                //}
            }
            dgCellList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                //if (string.IsNullOrEmpty(Util.NVC(dr["SPCL_FLAG"])) || Util.NVC(dr["SPCL_FLAG"]).Equals("N"))
                //{
                    dr["CHK"] = false;
                //}
            }
            dgCellList.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }
        #endregion

    }
}
