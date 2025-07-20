/*************************************************************************************
 Created Date : 2018.06.08
      Creator : 
   Decription : 장기 미사용자 LOCK 해제
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.08  DEVELOPER : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Windows.Media;
using System.Linq;
using C1.WPF;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_241 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        List<string> LotList = new List<string>();

        public COM001_241()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            txtUserID.Text = string.Empty;
            txtUserName.Text = string.Empty;
            Util.gridClear(dgSearch);
            Util.gridClear(dgSearchList);
        }
        #endregion

        #region Funct
        private void saveLongTerm()
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("ACT_USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["ACT_USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                DataTable inHIST = indataSet.Tables.Add("INHIST");
                inHIST.Columns.Add("USERID", typeof(string));

                DataTable dt = ((DataView)dgSearch.ItemsSource).Table;

                foreach (DataRow inRow in dt.Rows)
                {
                    newRow = null;
                    if (Convert.ToBoolean(inRow["CHK"]))
                    {
                        newRow = inHIST.NewRow();
                        newRow["USERID"] = Util.NVC(inRow["USERID"]);
                        inHIST.Rows.Add(newRow);
                    }
                }
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_PERSON_LONG_TERM", "INDATA,INHIST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        getLongTermList();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void getLongTermList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrWhiteSpace(txtUserID.Text))
                    dr["USERID"] = txtUserID.Text;
                if (!string.IsNullOrWhiteSpace(txtUserName.Text))
                    dr["USERNAME"] = txtUserName.Text;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_LONG_TERM", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSearch, dtRslt, FrameOperation);

                Util.gridClear(dgSearchList);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void getLongTermDetailList(string sUserID)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = sUserID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_LONG_TERM_DETL", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgSearchList, dtRslt, FrameOperation);
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

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            getLongTermList();
        }

        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowLoadingIndicator();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgSearch.GetRowCount() == 0)
            {
                HiddenLoadingIndicator();
                Util.MessageInfo("SFU3536");    //조회된 결과가 없습니다.
                return;
            }               

            Util.MessageConfirm("SFU4946", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    saveLongTerm();
                }
                HiddenLoadingIndicator();
            });

        }

        private void txtUserID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                getLongTermList();
            }
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                getLongTermList();
            }

        }

        private void dgSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgSearch.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("USERNAME"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
                if (dg.CurrentColumn.Name.Equals("USERNAME") && dg.GetRowCount() > 0 && dg.CurrentRow != null)
                {
                    getLongTermDetailList(Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "USERID")));
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

        #endregion
    }
}

