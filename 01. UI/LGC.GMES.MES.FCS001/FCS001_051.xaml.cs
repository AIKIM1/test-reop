/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : 저전압 Cell 등급 변경
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2021.04.13  KDH : 화면간 이동 시 초기화 현상 제거
  2022.01.01  LJM : LOTID -> PROD_LOTID로 변경
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
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_051 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        int addRows;

        public FCS001_051()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lblHelp.Text = ObjectDic.Instance.GetObjectName("UC_0016").Replace(@"\r\n", "\r\n");  //매거진 적재 전 등급 변경 가능.\r\n적재 된 이력이 있는 셀은 Magazine ID가 조회 됨

            AddRow(txtRow.Text);

            SettingCellCnt();

            this.Loaded -= UserControl_Loaded; //2021.04.13 화면간 이동 시 초기화 현상 제거
        }

        private void SettingCellCnt()
        {
            try
            {
                int iCell = 0;
                for (int i = 0; i < dgCellList.Rows.Count; i++)
                {
                    string sCellID = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID"));
                    if (!string.IsNullOrEmpty(sCellID))
                    {
                        iCell++;
                    }
                }
                txtCellCnt.Text = iCell.ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                for (int i = 0; i < dgCellList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID")) != string.Empty)
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["SUBLOTID"] = DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID");
                        dtRqst.Rows.Add(dr);
                    }
                }

                if (dtRqst.Rows.Count == 0) return;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_LOW_VOLT", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgCellList, dtRslt, this.FrameOperation, false);
                AddRow(txtRow.Text);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void AddRow(string sAddRow )
        {
            int iAddRow = Convert.ToInt16(sAddRow);

            DataTable dtCell = Util.MakeDataTable(dgCellList, true);
            DataTable dtTemp = dtCell.Copy();

            if (dtCell.Columns.Count == 0)
            {
                return;
            }

            Util.gridClear(dgCellList);
            dtTemp.Rows.Clear();

            for (int i = 0; i < iAddRow; i++)
            {
                if (i < dtCell.Rows.Count)
                {
                    string sSubLotID = Util.NVC(dtCell.Rows[i]["SUBLOTID"]);

                    if (!string.IsNullOrEmpty(sSubLotID))
                    {
                        DataRow dr = dtTemp.NewRow();
                        dr["SUBLOTID"] = Util.NVC(dtCell.Rows[i]["SUBLOTID"]);
                        dr["PROD_LOTID"] = Util.NVC(dtCell.Rows[i]["PROD_LOTID"]);
                        dr["ROUTID"] = Util.NVC(dtCell.Rows[i]["ROUTID"]);
                        dr["FINL_JUDG_CODE"] = Util.NVC(dtCell.Rows[i]["FINL_JUDG_CODE"]);
                        dtTemp.Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dtTemp.NewRow();
                        dr["SUBLOTID"] = string.Empty;
                        dr["PROD_LOTID"] = string.Empty;
                        dr["ROUTID"] = string.Empty;
                        dr["FINL_JUDG_CODE"] = string.Empty;
                        dtTemp.Rows.Add(dr);
                    }
                }
                else
                {
                    DataRow dr = dtTemp.NewRow();
                    dr["SUBLOTID"] = string.Empty;
                    dr["PROD_LOTID"] = string.Empty;
                    dr["ROUTID"] = string.Empty;
                    dr["FINL_JUDG_CODE"] = string.Empty;
                    dtTemp.Rows.Add(dr);
                }
            }
            Util.GridSetData(dgCellList, dtTemp, this.FrameOperation, false);
        }

        #endregion

        #region [Event]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellList);
            AddRow(txtRow.Text);
            SettingCellCnt();
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            GetList();
            SettingCellCnt();
        }

        private void btnGrade_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("FM_ME_0020", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "INDATA";
                        dtRqst.Columns.Add("SUBLOTID", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            string sSubLotID = Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID"));
                            if (!string.IsNullOrEmpty(sSubLotID))
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SUBLOTID"] = sSubLotID;
                                dr["USERID"] = LoginInfo.USERID;
                                dtRqst.Rows.Add(dr);
                            }
                        }

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CELL_CHANGE_GRADE", "INDATA", "OUTDATA", dtRqst);
                        Util.MessageInfo("FM_ME_0136"); //변경완료하였습니다.

                        GetList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });            
        }

        private void txtRow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!Char.IsDigit((char)KeyInterop.VirtualKeyFromKey(e.Key)) & e.Key != Key.Back | e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void txtRow_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRow.Text) || !Util.isNumber(txtRow.Text))
            {
                return;
            }

            AddRow(txtRow.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                buttonAccess(sender);
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void buttonAccess(object sender)
        {
            try
            {
                Button btnDelCell = sender as Button;
                DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
                if (btnDelCell != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;

                    if (string.Equals(btnDelCell.Name, "DELETE"))
                    {
                        DataGridCellPresenter presenter = (sender as Button).Parent as DataGridCellPresenter;

                        if (presenter == null)
                            return;

                        int clickedIndex = presenter.Row.Index;
                        dt.Rows.RemoveAt(clickedIndex);
                        Util.GridSetData(presenter.DataGrid, dt, this.FrameOperation);

                        txtRow.Text = dt.Rows.Count.ToString();
                        SettingCellCnt();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgCellList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("DELETE") || e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                    }
                }
            }));
        }

        #endregion

        private void dgCellList_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (dgCellList.Rows.Count > 0 && dgCellList.CurrentColumn.Name == "SUBLOTID")
            {
                if (dgCellList.CurrentRow != null && dgCellList.CurrentColumn != null)
                {
                    SettingCellCnt();
                }
            }
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
    }
}
