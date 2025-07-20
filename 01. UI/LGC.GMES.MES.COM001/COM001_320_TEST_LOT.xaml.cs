/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.02.20  정용석 : 결재요청 순서도 OUTPUT Parameter 추가로 인한 수정




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Windows.Media;
using C1.WPF.DataGrid;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_320_TEST_LOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _reqNo = string.Empty;
        private string _reqType = string.Empty;
        private Util _Util = new Util();
        private string _holdYN = string.Empty;
        private string _reqResncode = string.Empty;
        public COM001_320_TEST_LOT()
        {
            InitializeComponent();
            InitCombo();
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

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
          
            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TEST_LOTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            cboReqLottype.DisplayMemberPath = "CBO_NAME";
            cboReqLottype.SelectedValuePath = "CBO_CODE";

            DataRow dataRow = dtResult.NewRow();
            dataRow["CBO_CODE"] = string.Empty;
            dataRow["CBO_NAME"] = "-SELECT-";
            dtResult.Rows.InsertAt(dataRow, 0);

            cboReqLottype.ItemsSource = dtResult.Copy().AsDataView();
            cboReqLottype.SelectedIndex = 0;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            CommonCombo _combo = new CommonCombo();
            if (tmps != null && tmps.Length >= 1)
            {
                _reqNo = Util.NVC(tmps[0]);
                _reqType = Util.NVC(tmps[1]);
            }
            if (!_reqNo.Equals("NEW"))
            {
                SetModify();
                btnReq.Visibility = Visibility.Collapsed;
                btnSearch.Visibility = Visibility.Collapsed;
                grdSearch.Visibility = Visibility.Collapsed;
                txtGrator.Visibility = Visibility.Collapsed;
                txtNotice.Visibility = Visibility.Collapsed;
                dgList.Columns["CHK"].Visibility = Visibility.Collapsed;
                dgRequest.Columns["CHK"].Visibility = Visibility.Collapsed;
            
            }
            else
            {
                btnReqCancel.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Event

        #region [조회 - 버튼] : btnSearch_Click()
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLotList();
            chkAll.IsChecked = false;
        }
        #endregion

        #region [조회 - LOT Text박스 및 CST Text박스] : txtLot_KeyDown()
        private void txtLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetLotList();
                    chkAll.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [조회 - 여러건 조회] : txtLot_PreviewKeyDown()
        private void txtLot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    string lotList = string.Empty;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (i == 0)
                        {
                            lotList = sPasteStrings[i];
                        }
                        else
                        {
                            lotList = lotList + "," + sPasteStrings[i];
                        }

                        GetLotList(lotList);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }
        #endregion

        #region [대상 선택하기] - chk_Click()
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        private void chk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;


                if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(true))//체크되는 경우
                {

                    //상태체크
                    if (!DataTableConverter.GetValue(cb.DataContext, "APPR_NAME").IsNullOrEmpty())
                    {
                        // [%1]중인 LOT 입니다.
                        Util.MessageValidation("SFU5136", DataTableConverter.GetValue(cb.DataContext, "APPR_NAME").ToString());
                        DataTableConverter.SetValue(cb.DataContext, "CHK", false);
                        return;
                    }

                    //VD품질이상여부 
                    if (DataTableConverter.GetValue(cb.DataContext, "AUTO_VD_FLAG").ToString() == "Y")
                    {
                        // [%1]중인 LOT 입니다.
                        Util.MessageValidation("SFU5137");
                        DataTableConverter.SetValue(cb.DataContext, "CHK", false);
                        return;
                    }

                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK", typeof(Boolean));
                        dtTo.Columns.Add("EQSGNAME", typeof(string));
                        dtTo.Columns.Add("LOTID", typeof(string));
                        dtTo.Columns.Add("CSTID", typeof(string));
                        dtTo.Columns.Add("PRODID", typeof(string));
                        dtTo.Columns.Add("PRODNAME", typeof(string));
                        dtTo.Columns.Add("MODELID", typeof(string));
                        dtTo.Columns.Add("WIPQTY", typeof(string));
                        dtTo.Columns.Add("WIPHOLD", typeof(string));
                        dtTo.Columns.Add("APPR_NAME", typeof(string));
                        dtTo.Columns.Add("AUTO_VD_FLAG", typeof(string));
                    }

                    if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0) //중복조건 체크
                    {
                        return;
                    }

                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }

                    dtTo.Rows.Add(dr);
                    Util.GridSetData(dgRequest, dtTo, FrameOperation, true);
                }
                else//체크 풀릴때
                {
                    DataTable dtTo = DataTableConverter.Convert(dgRequest.ItemsSource);

                    dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);
                    Util.GridSetData(dgRequest, dtTo, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회된 LOT스프레드의 컬럼 이벤트 - 전체선택이벤트 설정] - dgList_LoadedColumnHeaderPresenter()
        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
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
                }
                catch (Exception ex)
                {

                }
            }));
        }
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        #endregion

        #region [전체선택] - checkAll_Checked()
        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = False AND APPR_NAME IS NULL AND AUTO_VD_FLAG = 'N'").ToList<DataRow>().ForEach(r => r["CHK"] = "True");
            dt.AcceptChanges();

            chkAllSelect();
        }

        #endregion

        #region [전체해제] - checkAll_Unchecked()
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            dt.Select("CHK = True").ToList<DataRow>().ForEach(r => r["CHK"] = "False");
            dt.AcceptChanges();

            chkAllClear();
        }
        #endregion
        
        #region [WIP HOLD 여부에 따른 색변경] - dgList_LoadedCellPresenter(), dgRequest_LoadedCellPresenter(), dgList_UnloadedCellPresenter()
        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter != null)
                    {
                        //HOLD 여부
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                        {
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                        }
                        else
                        {
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
                        }
                    }
                 
                }
            }));
        }

        private void dgRequest_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter != null)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                        {
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                        }
                        else
                        {
                            dg.GetCell(e.Cell.Row.Index, dg.Columns["WIPHOLD"].Index).Presenter.Background = new SolidColorBrush(Colors.White);
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
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            //link 색변경
                            if (e.Cell.Column.Name.Equals("WIPHOLD"))
                            {

                                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPHOLD")).Equals("Y"))
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                }
                                else
                                {
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                }
                            }
                            else
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            }
                        }
                    }
                }
            }));
        }

        #endregion

        #region [변경LOTTYPE 콤보박스 이벤트] - cboReqLottype_SelectedValueChanged()

        /// <summary>
        /// 변경 LOTTYPE  선택시 RESNCODE 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboReqLottype_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("LOTTYPE", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["LOTTYPE"] = cboReqLottype.SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TEST_LOTTYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            if (dtResult.Rows.Count > 0)
            {
                _reqResncode = dtResult.Rows[0]["UNHOLD_RESNCODE"].ToString();
            }
            //else
            //{
            //    _reqResncode = "R001";
            //}
        }

        #endregion
        
        #region [승인자 입력] : txtGrator_KeyDown()
        private void txtGrator_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtGrator.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("APPR_SEQS", typeof(string));
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }

                        dtTo.Rows.Add(drFrom);
                        for (int i = 0; i < dtTo.Rows.Count; i++)
                        {
                            dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
                        }
                        //dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                        Util.gridClear(dgGrator);
                        Util.GridSetData(dgGrator, dtTo, FrameOperation, true);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);
                        //dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        Util.GridSetData(dgGratorSelect, dtRslt, FrameOperation, true);

                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [승인자 검색결과 여러개일경우] : dgGratorChoice_Checked()
        private void dgGratorChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgGrator.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("APPR_SEQS", typeof(string));
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgGratorSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            for (int i = 0; i < dtTo.Rows.Count; i++)
            {
                dtTo.Rows[i]["APPR_SEQS"] = (i + 1);
            }


            //dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);
            Util.GridSetData(dgGrator, dtTo, FrameOperation, true);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
        }

        #endregion

        #region [참조자 입력] : txtNotice_KeyDown()
        private void txtNotice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtNotice.Text;
                    dr["LANGID"] = txtNotice.Text;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("SFU1780"); // 이미 추가 된 참조자 입니다.
                            return;
                        }

                        DataRow drFrom = dtTo.NewRow();
                        foreach (DataColumn dc in dtRslt.Columns)
                        {
                            if (dtTo.Columns.IndexOf(dc.ColumnName) > -1)
                            {
                                drFrom[dc.ColumnName] = dtRslt.Rows[0][dc.ColumnName];
                            }
                        }
                        dtTo.Rows.Add(drFrom);
                        Util.GridSetData(dgNotice, dtTo, FrameOperation, true);
                        txtNotice.Text = string.Empty;
                    }
                    else
                    {
                        dgNoticeSelect.Visibility = Visibility.Visible;
                        Util.gridClear(dgNoticeSelect);
                        Util.GridSetData(dgNoticeSelect, dtRslt, FrameOperation, true);
                        this.Focus();
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [참조자 검색결과 여러개일경우] : dgNoticeChoice_Checked()
        private void dgNoticeChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgNotice.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgNoticeSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);
            Util.GridSetData(dgNotice, dtTo, FrameOperation, true);
            dgNoticeSelect.Visibility = Visibility.Collapsed;

            txtNotice.Text = "";
        }
        #endregion

        #region [승인자 및 참조자 Spread 숨기기 : txtNotice_GotFocus(), txtGrator_GotFocus()
        /// <summary>
        /// 승인자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtNotice_GotFocus(object sender, RoutedEventArgs e)
        {
            dgGratorSelect.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 참조자
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtGrator_GotFocus(object sender, RoutedEventArgs e)
        {
            dgNoticeSelect.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region [승인자 및 참조자 제거 처리] : delete_Button_Click()
        private void delete_Button_Click(object sender, RoutedEventArgs e)
        {
            Button bt = sender as Button;

            C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;

            try
            {

                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;

                dg.IsReadOnly = false;
                dg.RemoveRow(dg.SelectedIndex);
                dg.IsReadOnly = true;

                //승인자 차수 정리
                if (dg.Name.Equals("dgGrator"))
                {
                    DataTable dt = DataTableConverter.Convert(dg.ItemsSource);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i]["APPR_SEQS"] = (i + 1);
                    }

                    Util.gridClear(dg);
                    Util.GridSetData(dg, dt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [요청클릭 - btnReq_Click()
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgGrator.Rows.Count == 0)
                {
                    Util.Alert("SFU1692");  //승인자가 필요합니다.
                    return;
                }
                if (dgRequest.Rows.Count == 0)
                {
                    Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                    return;
                }
                if (cboReqLottype.SelectedValue.ToString() == string.Empty)
                {
                    Util.MessageInfo("SFU5135", (result) => //선택된 LOTTYPE이 없습니다. LOTTYPE을 선택하세요.
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            cboReqLottype.Focus();
                        }
                    });
                    return;
                }
                int HoldCount = 0;
                for (int i = 0; i < dgRequest.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPHOLD")) == "Y")
                    {
                        HoldCount = HoldCount + 1;
                    }
                }

                if (HoldCount > 0)
                {
                    _holdYN = "Y";

                    //HOLD LOT이 존재합니다. 요청하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU5138"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Request();
                                }
                            });
                }
                else
                {
                    //요청하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                            "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    Request();
                                }
                            });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [요청취소] : btnReqCancel_Click()
        private void btnReqCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReqCancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [LOT 조회] - GetLotList()
        public void GetLotList(string lotLists = "")
        {
            try
            {
                if (string.IsNullOrEmpty(lotLists) && string.IsNullOrEmpty(txtLot.Text) && string.IsNullOrEmpty(txtCSTID.Text))
                    return;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(lotLists))
                    dr["LOTID"] = lotLists;
                if (!Util.GetCondition(txtLot).Equals(""))
                    dr["LOTID"] = Util.GetCondition(txtLot);
                if (!Util.GetCondition(txtCSTID).Equals(""))
                    dr["CSTID"] = Util.GetCondition(txtCSTID);
           
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_CHANGE_LOTTYPE", "INDATA", "OUTDATA", dtRqst);
                
                if (dtRslt.Rows.Count == 0)
                {
                    //조회된 데이터가 없습니다.
                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU1905"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {

                            if(string.IsNullOrEmpty(lotLists) && txtLot.Text == string.Empty && txtCSTID.Text != string.Empty)
                            {
                                txtCSTID.Text = string.Empty;
                                txtCSTID.Focus();
                            }
                            else
                            {
                                txtLot.Text = string.Empty;
                                txtLot.Focus();
                            }
                         
                        }
                    });
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgList.ItemsSource);
                    if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                    {
                        dtTo.Columns.Add("CHK",          typeof(Boolean));
                        dtTo.Columns.Add("EQSGNAME",     typeof(string));
                        dtTo.Columns.Add("LOTID",        typeof(string));
                        dtTo.Columns.Add("CSTID",        typeof(string));
                        dtTo.Columns.Add("LOTTYPE",      typeof(string));
                        dtTo.Columns.Add("LOTYNAME",     typeof(string));
                        dtTo.Columns.Add("PRODID",       typeof(string));
                        dtTo.Columns.Add("PRODNAME",     typeof(string));
                        dtTo.Columns.Add("PRJT_NAME",    typeof(string));
                        dtTo.Columns.Add("MODELID",      typeof(string));
                        dtTo.Columns.Add("WIPQTY",       typeof(decimal));
                        dtTo.Columns.Add("WIPHOLD",      typeof(string));
                        dtTo.Columns.Add("APPR_NAME",    typeof(string));
                        dtTo.Columns.Add("AUTO_VD_FLAG", typeof(string));
                    }
                    DataRow Add_dr = null;
                    for (int i=0; i< dtRslt.Rows.Count; i++ )
                    {
                        if (!dtRslt.Rows[i]["APPR_NAME"].IsNullOrEmpty()) return;
                        if (dtRslt.Rows[i]["AUTO_VD_FLAG"].ToString() == "Y") return;

                        if (dtTo.Select("LOTID = '" + dtRslt.Rows[i]["LOTID"].ToString() + "'").Length == 0) //중복조건 체크
                        {
                            Add_dr = dtTo.NewRow();
                            Add_dr["CHK"]          = dtRslt.Rows[i]["CHK"];
                            Add_dr["EQSGNAME"]     = dtRslt.Rows[i]["EQSGNAME"];
                            Add_dr["LOTID"]        = dtRslt.Rows[i]["LOTID"];
                            Add_dr["CSTID"]        = dtRslt.Rows[i]["CSTID"];
                            Add_dr["LOTTYPE"]      = dtRslt.Rows[i]["LOTTYPE"];
                            Add_dr["LOTYNAME"]     = dtRslt.Rows[i]["LOTYNAME"];
                            Add_dr["PRODID"]       = dtRslt.Rows[i]["PRODID"];
                            Add_dr["PRODNAME"]     = dtRslt.Rows[i]["PRODNAME"];
                            Add_dr["PRJT_NAME"]    = dtRslt.Rows[i]["PRJT_NAME"];
                            Add_dr["MODELID"]      = dtRslt.Rows[i]["MODELID"];
                            Add_dr["WIPQTY"]       = dtRslt.Rows[i]["WIPQTY"];
                            Add_dr["WIPHOLD"]      = dtRslt.Rows[i]["WIPHOLD"];
                            Add_dr["APPR_NAME"]    = dtRslt.Rows[i]["APPR_NAME"];
                            Add_dr["AUTO_VD_FLAG"] = dtRslt.Rows[i]["AUTO_VD_FLAG"];
                            dtTo.Rows.Add(Add_dr);
                        }
                     }
                    Util.GridSetData(dgList, dtTo, FrameOperation, true);

                    if (txtLot.Text == string.Empty && txtCSTID.Text != string.Empty)
                    {
                        txtCSTID.Text = string.Empty;
                        txtCSTID.Focus();
                    }
                    else
                    {
                        int idx = _Util.GetDataGridRowIndex(dgList, "LOTID", txtLot.Text);

                        if (idx >= 0)
                        {
                            //row 색 바꾸기
                            dgList.SelectedIndex = idx;
                            dgList.ScrollIntoView(idx, dgList.Columns["LOTID"].Index);
                        }

                        txtLot.Text = string.Empty;
                        txtLot.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
      
        #region [수정모드일 경우 조회] - SetModify()
        public void SetModify()
        {
            try
            {
                DataSet inData = new DataSet();
                DataTable dtRqst = inData.Tables.Add("INDATA");

                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = _reqNo;

                dtRqst.Rows.Add(dr);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_APPR_REQUEST", "INDATA", "OUTDATA,OUTPROG,OUTREF,OUTLOT", inData);

                Util.gridClear(dgRequest);
                Util.GridSetData(dgRequest, dsRslt.Tables["OUTLOT"], FrameOperation, true);

                Util.gridClear(dgGrator);
                Util.GridSetData(dgGrator, dsRslt.Tables["OUTPROG"], FrameOperation, true);

                Util.gridClear(dgNotice);
                Util.GridSetData(dgNotice, dsRslt.Tables["OUTREF"], FrameOperation, true);

                txtNote.Text = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NOTE"].ToString();
                cboReqLottype.SelectedValue = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_LOTTYPE"].ToString();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [요청] - Request()
        private void Request()
        {
            string sTo = "";
            string sCC = "";
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));
            inDataTable.Columns.Add("RESNCODE", typeof(string));
            inDataTable.Columns.Add("COST_CNTR_ID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("REQ_LOTTYPE", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = "LOTTYPE_CHANGE";//_reqType;// Util.GetCondition(cboReqType);
            row["REQ_NOTE"] = Util.GetCondition(txtNote);
            if (_holdYN == "Y")
            {
                if(_reqResncode == string.Empty)
                {
                     Util.MessageValidation("SFU5140");//사유코드 정보가 없습니다.
                    return;
                }
                row["RESNCODE"] = _reqResncode;
            }
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["REQ_LOTTYPE"] = cboReqLottype.SelectedValue.ToString();
            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("WIPQTY", typeof(decimal));
            inLot.Columns.Add("WIPQTY2", typeof(decimal));
            inLot.Columns.Add("PRODID", typeof(string));
            inLot.Columns.Add("PRODNAME", typeof(string));
            inLot.Columns.Add("MODELID", typeof(string));
            for (int i = 0; i < dgRequest.Rows.Count; i++)
            {
                row = inLot.NewRow();
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "LOTID"));
                row["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "WIPQTY"));
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODID"));
                row["PRODNAME"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "PRODNAME"));
                row["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[i].DataItem, "MODELID"));
                inLot.Rows.Add(row);
            }

            //승인자
            DataTable inProg = inData.Tables.Add("INPROG");
            inProg.Columns.Add("APPR_SEQS", typeof(string));
            inProg.Columns.Add("APPR_USERID", typeof(string));

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                row = inProg.NewRow();
                row["APPR_SEQS"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "APPR_SEQS"));
                row["APPR_USERID"] = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                inProg.Rows.Add(row);

                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgNotice.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_APPR_REQUEST", "INDATA,INLOT,INPROG,INREF", "OUTDATA,OUTDATA_LOT", inData);
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    GetLotList();
                    Util.gridClear(dgRequest);
                    Util.gridClear(dgGrator);
                    Util.gridClear(dgNotice);
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " " + this.Header;

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote), inLot));
                }
                Util.AlertInfo("SFU1747");  //요청되었습니다.
                _reqResncode = string.Empty;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [요청 취소] - ReqCancel()
        private void ReqCancel()
        {
            string sTo = "";
            string sCC = "";

            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("REQ_NO", typeof(string));
            dtRqst.Columns.Add("USERID", typeof(string));
            dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["REQ_NO"] = _reqNo;
            dr["USERID"] = LoginInfo.USERID;
            dr["REQ_RSLT_CODE"] = "DEL";


            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_APPR_REQ", "INDATA", "OUTDATA", dtRqst);

            for (int i = 0; i < dgGrator.Rows.Count; i++)
            {
                if (i == 0)//최초 승인자만 메일 가도록
                {
                    sTo = Util.NVC(DataTableConverter.GetValue(dgGrator.Rows[i].DataItem, "USERID"));
                }
            }

            //참조자

            for (int i = 0; i < dgNotice.Rows.Count; i++)
            {
                sCC += Util.NVC(DataTableConverter.GetValue(dgNotice.Rows[i].DataItem, "USERID")) + ";";
            }


            MailSend mail = new CMM001.Class.MailSend();
            string sMsg = ObjectDic.Instance.GetObjectName("요청취소"); //승인요청취소
            string sTitle = _reqNo + " " + this.Header;

            mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sCC, sMsg, mail.makeBodyApp(sTitle, Util.GetCondition(txtNote)));

            Util.AlertInfo("SFU1937");  //취소되었습니다.
            this.DialogResult = MessageBoxResult.OK;
            this.Close();
        }
        #endregion

        #region [전체 선택 및 전체 해제] - chkAll(), chkAllSelect(), chkAllClear()
        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        private void chkAllSelect()
        {
            Util.gridClear(dgRequest);

            DataTable dtSelect = new DataTable();
            DataTable dtTo = DataTableConverter.Convert(dgList.ItemsSource);
            dtTo.Select("CHK = False").ToList<DataRow>().ForEach(row => row.Delete());
            dtSelect = dtTo.Copy();
            Util.GridSetData(dgRequest, dtSelect, FrameOperation, true);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgRequest);
        }


        #endregion

        #endregion

      
    }
}
