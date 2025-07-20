/*********************************************************************************************************************************
 Created Date : 2024.03.13
      Creator : 유명환
   Decription : 자재LOT 잔량재공 조회 재공삭제
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2024.03.13                               유명환           Initial Created.
***********************************************************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using System.Globalization;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_203_WIP_DELETE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private string _scrapType = string.Empty;
        private string _resnGubun = string.Empty;
        private string _area = string.Empty;
        private object[] tmps = null;
        private Util _Util = new Util();
        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public MTRL001_203_WIP_DELETE()
        {
            InitializeComponent();

            //InitCombo();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

        }

        private void ClearList()
        {
            Util.gridClear(dgDeleteList);
            Util.gridClear(dgUnDeleteList);
            Util.gridClear(dgRequest);
        }
        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //파라메터 등록
            tmps = C1WindowExtension.GetParameters(this);

            setGridHeader();
            DeleteListData(tmps);
            UnDeleteListData(tmps);

        }

        private void txtScanId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ClearList();

                DeleteListData(tmps);
                UnDeleteListData(tmps);
                chkAll.IsChecked = false;
            }
        }

        #region [조회클릭]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            ClearList();

            DeleteListData(tmps);
            UnDeleteListData(tmps);
            chkAll.IsChecked = false;
        }
        #endregion

        private void btnReqDelete_Click(object sender, RoutedEventArgs e)
        {
            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgRequest, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }
            if (string.IsNullOrEmpty(Util.NVC(txtReason.Text)))
            {
                // 사유를 입력 하세요.
                Util.MessageValidation("SFU1594");
                return;
            }
            if (string.IsNullOrEmpty(Util.NVC(txtUserName.Text)) || string.IsNullOrEmpty(Util.NVC(txtUserName.Tag)))
            {
                // 요청자를 입력 하세요.
                Util.MessageValidation("SFU3451");
                return;
            }

            //삭제 하시겠습니까?
            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    deleteRemainLot();
                }
            });
        }

        private void C1Window_Closed(object sender, EventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        //조용수 추가
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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgDeleteList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgDeleteList.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgDeleteList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgDeleteList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void dgDeleteList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void chkDeleteList_Click(object sender, RoutedEventArgs e)
        {
            dgDeleteList.Selection.Clear();

            CheckBox cb = sender as CheckBox;
            DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

            if (dtTo.Columns.Count == 0) //최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Int32));
                dtTo.Columns.Add("MLOTID", typeof(string));
                dtTo.Columns.Add("MTRLID", typeof(string));
                dtTo.Columns.Add("ORIG_UNIT_CODE", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(decimal));
                dtTo.Columns.Add("WIPQTY_M", typeof(decimal));
                dtTo.Columns.Add("ORIG_WIPQTY", typeof(decimal));
                dtTo.Columns.Add("WIPHOLD", typeof(string));
                dtTo.Columns.Add("EQSGNAME", typeof(string));
                dtTo.Columns.Add("PROCNAME", typeof(string));
                dtTo.Columns.Add("WIPSTAT", typeof(string));
                dtTo.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtTo.Columns.Add("WIPSDTTM", typeof(DateTime));
                dtTo.Columns.Add("MTRLNAME", typeof(string));
                dtTo.Columns.Add("MTRLDESC", typeof(string));
                dtTo.Columns.Add("TCK", typeof(decimal));
                dtTo.Columns.Add("WIDTH", typeof(decimal));
                dtTo.Columns.Add("CONV_RATE", typeof(decimal));
                dtTo.Columns.Add("INPUTSEQNO", typeof(string));
                dtTo.Columns.Add("MTRL_ISS_QTY", typeof(decimal));
                dtTo.Columns.Add("ORIG_MTRL_ISS_QTY", typeof(decimal));
                dtTo.Columns.Add("MTRL_ISS_QTY_M", typeof(decimal));
            }

            if (dtTo.Select("MLOTID = '" + DataTableConverter.GetValue(cb.DataContext, "MLOTID") + "'").Length > 0) //중복조건 체크
            {
                return;
            }

            DataRow dr = dtTo.NewRow();

            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Int32)) || dc.DataType.Equals(typeof(decimal)))
                {
                    dr[dc.ColumnName] = Util.NVC_Int(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                }
            }

            dtTo.Rows.Add(dr);
            dgRequest.ItemsSource = DataTableConverter.Convert(dtTo);

            DataTable dtTo2 = DataTableConverter.Convert(dgDeleteList.ItemsSource);
            dtTo2.Rows.Remove(dtTo2.Select("MLOTID = '" + DataTableConverter.GetValue(cb.DataContext, "MLOTID") + "'")[0]);
            dgDeleteList.ItemsSource = DataTableConverter.Convert(dtTo2);
        }

        private void chkUnDeleteList_Click(object sender, RoutedEventArgs e)
        {
            dgRequest.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            DataTable dtTo = DataTableConverter.Convert(dgDeleteList.ItemsSource);

            if (dtTo.Columns.Count == 0) //최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("CHK", typeof(Int32));
                dtTo.Columns.Add("MLOTID", typeof(string));
                dtTo.Columns.Add("MTRLID", typeof(string));
                dtTo.Columns.Add("ORIG_UNIT_CODE", typeof(string));
                dtTo.Columns.Add("WIPQTY", typeof(decimal));
                dtTo.Columns.Add("WIPQTY_M", typeof(decimal));
                dtTo.Columns.Add("ORIG_WIPQTY", typeof(decimal));
                dtTo.Columns.Add("WIPHOLD", typeof(string));
                dtTo.Columns.Add("EQSGNAME", typeof(string));
                dtTo.Columns.Add("PROCNAME", typeof(string));
                dtTo.Columns.Add("WIPSTAT", typeof(string));
                dtTo.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                dtTo.Columns.Add("WIPSDTTM", typeof(DateTime));
                dtTo.Columns.Add("MTRLNAME", typeof(string));
                dtTo.Columns.Add("MTRLDESC", typeof(string));
                dtTo.Columns.Add("TCK", typeof(decimal));
                dtTo.Columns.Add("WIDTH", typeof(decimal));
                dtTo.Columns.Add("CONV_RATE", typeof(decimal));
                dtTo.Columns.Add("INPUTSEQNO", typeof(string));
                dtTo.Columns.Add("MTRL_ISS_QTY", typeof(decimal));
                dtTo.Columns.Add("ORIG_MTRL_ISS_QTY", typeof(decimal));
                dtTo.Columns.Add("MTRL_ISS_QTY_M", typeof(decimal));
            }
            else
            {
                dtTo.Columns.Remove("CHK");
                dtTo.Columns.Add("CHK", typeof(Int32));
            }

            if (dtTo.Select("MLOTID = '" + DataTableConverter.GetValue(cb.DataContext, "MLOTID") + "'").Length > 0) //중복조건 체크
            {
                return;
            }

            DataRow dr = dtTo.NewRow();

            foreach (DataColumn dc in dtTo.Columns)
            {
                if (dc.DataType.Equals(typeof(Int32)) || dc.DataType.Equals(typeof(decimal)))
                {
                    dr[dc.ColumnName] = Util.NVC_Int(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                }
                else
                {
                    dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                }
            }

            dtTo.Rows.Add(dr);
            dgDeleteList.ItemsSource = DataTableConverter.Convert(dtTo);

            DataTable dtTo2 = DataTableConverter.Convert(dgRequest.ItemsSource);
            dtTo2.Rows.Remove(dtTo2.Select("MLOTID = '" + DataTableConverter.GetValue(cb.DataContext, "MLOTID") + "'")[0]);
            dgRequest.ItemsSource = DataTableConverter.Convert(dtTo2);
        }

        private void txtUserName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnReqUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUserName.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }

        #endregion

        #region Mehod

        private void setGridHeader()
        {
            if (LoginInfo.CFG_AREA_ID.Substring(0, 1).Equals("E"))
            {
                dgDeleteList.Columns["WIPQTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Collapsed;

                dgDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Collapsed;

                dgDeleteList.Columns["TCK"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["WIDTH"].Visibility = Visibility.Visible;
                dgDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Visible;

                dgUnDeleteList.Columns["WIPQTY"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Collapsed;

                dgUnDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Collapsed;

                dgUnDeleteList.Columns["TCK"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["WIDTH"].Visibility = Visibility.Visible;
                dgUnDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Visible;

                dgRequest.Columns["WIPQTY"].Visibility = Visibility.Visible;
                dgRequest.Columns["ORIG_WIPQTY"].Visibility = Visibility.Visible;
                dgRequest.Columns["WIPQTY_M"].Visibility = Visibility.Collapsed;

                dgRequest.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgRequest.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Visible;
                dgRequest.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Collapsed;

                dgRequest.Columns["TCK"].Visibility = Visibility.Visible;
                dgRequest.Columns["WIDTH"].Visibility = Visibility.Visible;
                dgRequest.Columns["CONV_RATE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgDeleteList.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Visible;

                dgDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Visible;

                dgDeleteList.Columns["TCK"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["WIDTH"].Visibility = Visibility.Collapsed;
                dgDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Collapsed;

                dgUnDeleteList.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["ORIG_WIPQTY"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["WIPQTY_M"].Visibility = Visibility.Visible;

                dgUnDeleteList.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Visible;

                dgUnDeleteList.Columns["TCK"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["WIDTH"].Visibility = Visibility.Collapsed;
                dgUnDeleteList.Columns["CONV_RATE"].Visibility = Visibility.Collapsed;

                dgRequest.Columns["WIPQTY"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["ORIG_WIPQTY"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["WIPQTY_M"].Visibility = Visibility.Visible;

                dgRequest.Columns["MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["ORIG_MTRL_ISS_QTY"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["MTRL_ISS_QTY_M"].Visibility = Visibility.Visible;

                dgRequest.Columns["TCK"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["WIDTH"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["CONV_RATE"].Visibility = Visibility.Collapsed;
            }
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgDeleteList.ItemsSource);
            dtSelect = dtTo.Copy();

            dgRequest.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }

        private void DeleteListData(object[] tmps)
        {
            setGridListData(dgDeleteList, tmps, "WAIT");            
        }

        private void UnDeleteListData(object[] tmps)
        {
            setGridListData(dgUnDeleteList, tmps, "NotWAIT");
        }

        private void setGridListData(C1.WPF.DataGrid.C1DataGrid Grid, object[] tmps, string sWipStat)
        {
            try
            {
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQSGID", typeof(string));
                dtINDATA.Columns.Add("PROCID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("MLOTID", typeof(string));
                dtINDATA.Columns.Add("INWAITWIPSTAT", typeof(Boolean));
                dtINDATA.Columns.Add("NOTINWAITWIPSTAT", typeof(Boolean));

                DataRow Indata = dtINDATA.NewRow();
                Indata["LANGID"] = tmps[0].ToString();
                Indata["AREAID"] = tmps[1].ToString();

                if (!String.IsNullOrEmpty(tmps[2].ToString()))
                    Indata["EQSGID"] = tmps[2].ToString();

                if (!String.IsNullOrEmpty(tmps[3].ToString()))
                    Indata["PROCID"] = tmps[3].ToString();

                if (!String.IsNullOrEmpty(tmps[4].ToString()))
                    Indata["MTRLID"] = tmps[4].ToString();

                if (!String.IsNullOrEmpty(txtScanId.Text))
                    Indata["MLOTID"] = txtScanId.Text;

                if (sWipStat.Equals("WAIT"))
                    Indata["INWAITWIPSTAT"] = true;
                else if(sWipStat.Equals("NotWAIT"))
                    Indata["NOTINWAITWIPSTAT"] = true;

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_BAS_SEL_MLOT_WIP_REMAIN_DETAIL";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "RQSTDT", "RSLTDT", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if ((dtResult == null || dtResult.Rows.Count == 0) && sWipStat.Equals("WAIT"))
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(Grid, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void deleteRemainLot()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inMLot = indataSet.Tables.Add("IN_LOT");
                inMLot.Columns.Add("MLOTID", typeof(string));
                inMLot.Columns.Add("INPUTSEQNO", typeof(string));

                for (int i = 0; i < dgRequest.GetRowCount(); i++)
                {
                    if (!_Util.GetDataGridCheckValue(dgRequest, "CHK", i)) continue;

                    DataRow newMLotRow = inMLot.NewRow();
                    newMLotRow["MLOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MLOTID"));
                    newMLotRow["INPUTSEQNO"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "INPUTSEQNO"));

                    inMLot.Rows.Add(newMLotRow);
                }

                DataTable inRequestUser = indataSet.Tables.Add("IN_REQUEST_USER");
                inRequestUser.Columns.Add("RUSERID", typeof(string));
                inRequestUser.Columns.Add("RUSERNAME", typeof(string));
                inRequestUser.Columns.Add("RESON", typeof(string));

                DataRow newRequestUserRow = inRequestUser.NewRow();
                newRequestUserRow["RUSERID"] = Util.NVC(txtUserName.Tag);
                newRequestUserRow["RUSERNAME"] = Util.NVC(txtUserName.Text);
                newRequestUserRow["RESON"] = Util.NVC(txtReason.Text);

                inRequestUser.Rows.Add(newRequestUserRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAIN_DELETE_LOT", "IN_LOT,IN_REQUEST_USER", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ClearList();
                                DeleteListData(tmps);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally { }
        }

        #endregion
    }
}
