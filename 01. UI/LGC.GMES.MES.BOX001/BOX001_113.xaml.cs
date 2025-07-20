/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이슬아
   Decription : 포장출고
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.06  이슬아 : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_113 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private string RETURN_CONFIRM_OK = "Y";
        private string RETURN_CONFIRM_CANCEL = "N";

        Util _util = new Util();
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

        public BOX001_113()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //GetEqptWrkInfo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine }, sCase: "ALLAREA");

            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea }, cbChild: new C1ComboBox[] { cboEqpt });

            _combo.SetCombo(cboEqpt, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine }, sFilter: new string[] { "F6000" }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");
                  
            _combo.SetCombo(cboArea2, CommonCombo.ComboStatus.SELECT, cbChild: new C1ComboBox[] { cboLine2 }, sCase: "ALLAREA");

            _combo.SetCombo(cboLine2, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboArea2 }, cbChild: new C1ComboBox[] { cboEqpt2 }, sCase: "cboLine");

            _combo.SetCombo(cboEqpt2, CommonCombo.ComboStatus.ALL, cbParent: new C1ComboBox[] { cboLine2 }, sFilter: new string[] { "F6000" }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");




            DataTable dtCombo = new DataTable();
            dtCombo.TableName = "RQSTDT";
            dtCombo.Columns.Add("CBO_CODE", typeof(string));
            dtCombo.Columns.Add("CBO_NAME", typeof(string));

            DataRow dr = dtCombo.NewRow();
            dr["CBO_CODE"] = null;
            dr["CBO_NAME"] = "ALL";
            dtCombo.Rows.Add(dr);

            dr = dtCombo.NewRow();
            dr["CBO_CODE"] = "UNHOLD";
            dr["CBO_NAME"] = "UNHOLD";
            dtCombo.Rows.Add(dr);

            dr = dtCombo.NewRow();
            dr["CBO_CODE"] = "HOLD";
            dr["CBO_NAME"] = "HOLD";
            dtCombo.Rows.Add(dr);

            cboReturnStatus.DisplayMemberPath = "CBO_NAME";
            cboReturnStatus.SelectedValuePath = "CBO_CODE";
            cboReturnStatus.ItemsSource = DataTableConverter.Convert(dtCombo);
            cboReturnStatus.SelectedIndex = 0;
    }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom2.SelectedDateTime = DateTime.Today;
            dtpDateTo2.SelectedDateTime = DateTime.Today;
        }
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            dtpDateFrom2.SelectedDataTimeChanged += dtpDateFrom2_SelectedDataTimeChanged;
            dtpDateTo2.SelectedDataTimeChanged += dtpDateTo2_SelectedDataTimeChanged;
        }

        #region 기간
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }
        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }


        private void dtpDateFrom2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo2.SelectedDateTime;
                return;
            }
        }
        private void dtpDateTo2_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom2.SelectedDateTime;
                return;
            }
        }
        

        #endregion


        private void btnSearch_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

    
        private void dgSearhResultChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearhResult.ItemsSource)
                {
                    if (drv["BOXID"].ToString().Equals(item["BOXID"].ToString()))
                    {
                        item["CHK"] = true;
                    }
                }
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void puCancel_Closed(object sender, EventArgs e)
        {
            BOX001_106_CANCEL popUp = sender as BOX001_106_CANCEL;
            if (popUp.DialogResult == MessageBoxResult.OK)
            {
                Search();
            }
            this.grdMain.Children.Remove(popUp);
        }


        private void dgSearhResultChoice_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearhResult.ItemsSource)
                {
                    if (drv["BOXID"].ToString().Equals(item["BOXID"].ToString()))
                    {
                        item["CHK"] = false;
                    }
                }
                //drv["CHK"] = false;
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void btnReturnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (!Validation(btn.Name))
                    return;

                // 메세지처리 출고 하시겠습니까?
                Util.MessageConfirm("고객반품에 대한 포장출고 처리 가능 상태로 변경 하시겠습니까?", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        DataRow inDataRow = inDataTable.NewRow();
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("RETURN_CONFIRM_FLAG");

                        inDataRow["USERID"] = txtUserName.Tag.ToString();
                        inDataRow["RETURN_CONFIRM_FLAG"] = RETURN_CONFIRM_OK;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
                        var query = (from t in dt.AsEnumerable()
                                     where t.Field<bool>("CHK")
                                     select t).ToList();

                        foreach (DataRow dr in query)
                        {
                            DataRow newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = dr["BOXID"].ToString();
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_CONFIRM_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                Search();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {

                            }
                        }, indataSet);

                    }
                });              
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_INPALLET_FOR_SHIP_FM
        /// </summary>
        private void Search()
        {
            try
            {

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROJECT", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(string));
                RQSTDT.Columns.Add("TO_DTTM", typeof(string));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("CHR_EQPTID", typeof(string));
                RQSTDT.Columns.Add("ASSY_EQSGID", typeof(string));

                RQSTDT.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("RETURN_CONFIRM_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_STAT_CODE"] = "READY_SHIP";
                dr["FORM_WRK_TYPE_CODE"] = "FORM_WORK_CR"; //고객반품
                dr["RETURN_CONFIRM_FLAG"] = "HOLD"; //미확인 상태 대상 조회
                
                if (!string.IsNullOrWhiteSpace(txtBoxID.Text))
                {
                    dr["BOXID"] = txtBoxID.Text;
                }
               else
                {
                    dr["FROM_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                    dr["TO_DTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
                    dr["CHR_EQPTID"] = string.IsNullOrWhiteSpace(Util.NVC(cboEqpt.SelectedValue)) ? null : cboEqpt.SelectedValue;
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtPkgLotID.Text)? null : txtPkgLotID.Text;
                    dr["PROJECT"] = string.IsNullOrWhiteSpace(txtProject.Text) ? null : txtProject.Text;
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProd.Text) ? null : txtProd.Text;
                    dr["AREAID"] = String.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue)) ? null : cboArea.SelectedValue;
                    dr["EQSGID"] = String.IsNullOrWhiteSpace(Util.NVC(cboLine.SelectedValue)) ? null : cboLine.SelectedValue;
                }             

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_INPALLET_FOR_SHIP_FM", "INDATA", "OUTDATA", RQSTDT);
                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgSearhResult, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void GetHistory()
        {
            try
            {
                if (Util.NVC(cboArea2.SelectedValue) == string.Empty || Util.NVC(cboArea2.SelectedValue) == "SELECT")
                {
                    // SFU1499 동을 선택해주십시오.
                    Util.MessageValidation("SFU1499");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("CHR_EQPTID", typeof(String));
                RQSTDT.Columns.Add("EQSGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("PROJECT", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("PKG_LOTID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_FROM_DTTM", typeof(String));
                RQSTDT.Columns.Add("RCV_TO_DTTM", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(String));

                RQSTDT.Columns.Add("FORM_WRK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("RETURN_CONFIRM_FLAG", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;



                dr["FROM_DTTM"] = dtpDateFrom2.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
                dr["TO_DTTM"] = dtpDateTo2.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");

                dr["PROJECT"] = String.IsNullOrWhiteSpace(txtProject2.Text) ? null : txtProject2.Text;
                dr["PRODID"] = String.IsNullOrWhiteSpace(txtProdId2.Text) ? null : txtProdId2.Text;
                dr["PKG_LOTID"] = String.IsNullOrWhiteSpace(txtPkgLotID2.Text) ? null : txtPkgLotID2.Text;
                dr["BOXID"] = String.IsNullOrWhiteSpace(txtBoxID2.Text) ? null : txtBoxID2.Text;

                dr["CHR_EQPTID"] = string.IsNullOrWhiteSpace(Util.NVC(cboEqpt2.SelectedValue)) ? null : cboEqpt2.SelectedValue;
                dr["AREAID"] = String.IsNullOrWhiteSpace(Util.NVC(cboArea2.SelectedValue)) ? null : cboArea2.SelectedValue;
                dr["EQSGID"] = String.IsNullOrWhiteSpace(Util.NVC(cboLine2.SelectedValue)) ? null : cboLine2.SelectedValue;

                dr["RCV_ISS_STAT_CODE"] = null; //출고대기 상태
                dr["FORM_WRK_TYPE_CODE"] = null; //고객반품
                dr["RETURN_CONFIRM_FLAG"] = String.IsNullOrWhiteSpace(Util.NVC(cboReturnStatus.SelectedValue)) ? null : cboReturnStatus.SelectedValue ; 


                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INPALLET_LIST_FOR_SHIP_FM_RETURN", "INDATA", "OUTDATA", RQSTDT);
                if (!SearchResult.Columns.Contains("CHK"))
                     SearchResult = _util.gridCheckColumnAdd(SearchResult, "CHK");

                Util.GridSetData(dgSearhResult2, SearchResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetHistory();
        }

      
        private bool Validation(string btnName)
        {

            if (string.IsNullOrEmpty(txtUserName.Text) || string.IsNullOrEmpty(Util.NVC(txtUserName.Tag)))
            {
                // SFU1843 작업자를 입력 해주세요.
                Util.MessageValidation("SFU4011");
                return false;
            }

            switch (btnName)
            {
                case "btnReturnConfirm":
                    if (dgSearhResult.ItemsSource == null || dgSearhResult.Rows.Count <= 0)
                    {
                        // SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return false;
                    }

                    //bool result = true;
                    DataTable dt = ((DataView)dgSearhResult.ItemsSource).Table;
                    var query = (from t in dt.AsEnumerable()
                                 where t.Field<bool>("CHK") //t.Field<string>("CHK") == bool.TrueString
                                 select t).ToList();

                    if (!query.Any())
                    {
                        // SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return false;
                    }

                    break;

                case "btnReturnConfirmCancel":
                    if (dgSearhResult2.ItemsSource == null || dgSearhResult2.Rows.Count <= 0)
                    {
                        // SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return false;
                    }

                    //bool result = true;
                    DataTable dt2 = ((DataView)dgSearhResult2.ItemsSource).Table;
                    var query2 = (from t in dt2.AsEnumerable()
                                  where t.Field<bool>("CHK") //t.Field<string>("CHK") == bool.TrueString
                                  select t).ToList();

                    if (!query2.Any())
                    {
                        // SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return false;
                    }
                    break;


                default:
                    return false;
            }

           
            
            return true;
        }

        #endregion

 

      

        private void dgSearhResult2_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {

            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearhResult2.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    // if (Util.NVC(DataTableConverter.GetValue(dgSearhResult2.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgSearhResult2.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                    if (Util.NVC(DataTableConverter.GetValue(dgSearhResult2.Rows[i].DataItem, "RCV_ISS_STAT_CODE")).Equals("READY_SHIP") &&
                        Util.NVC(DataTableConverter.GetValue(dgSearhResult2.Rows[i].DataItem, "RETURN_CONFIRM_FLAG")).Equals("UNHOLD") &&
                        !Util.NVC(DataTableConverter.GetValue(dgSearhResult2.Rows[i].DataItem, "WIPSTAT")).Equals("TERM") )
                     {
                        DataTableConverter.SetValue(dgSearhResult2.Rows[i].DataItem, "CHK", true);
                    }
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgSearhResult2.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgSearhResult2.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        /// <summary>
        /// BR_PRD_REG_CARRIED_OVER_SHIP_INPALLET_FM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReturnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (!Validation(btn.Name))
                    return;

                Util.MessageConfirm("고객반품 포장출고 확인 완료를 취소 하시겠습니까?", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("USERID");
                        inDataTable.Columns.Add("RETURN_CONFIRM_FLAG");

                        DataRow inDataRow = inDataTable.NewRow();
                        inDataRow["USERID"] = txtUserName.Tag.ToString();
                        inDataRow["RETURN_CONFIRM_FLAG"] = RETURN_CONFIRM_CANCEL;
                        inDataTable.Rows.Add(inDataRow);

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataTable dt = DataTableConverter.Convert(dgSearhResult2.ItemsSource);
                        var query = (from t in dt.AsEnumerable()
                                     where t.Field<bool>("CHK")  //t.Field<string>("CHK") == bool.TrueString
                                     select t).ToList();

                        foreach (DataRow dr in query)
                        {
                            DataRow newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = dr["BOXID"].ToString();
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_CONFIRM_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                                GetHistory();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
                    }
                
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void dgSearhResultChoice2_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                if (Util.NVC(drv["RCV_ISS_STAT_CODE"].ToString()).Equals("READY_SHIP") &&
                    Util.NVC(drv["RETURN_CONFIRM_FLAG"].ToString()).Equals("UNHOLD") &&
                   !Util.NVC(drv["WIPSTAT"].ToString()).Equals("TERM"))
                {
                    cb.IsChecked = true;
                }
                else
                {
                    cb.IsChecked = false;
                }
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void dgSearhResultChoice2_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = sender as CheckBox;
                if (cb?.DataContext == null) return;
                if (cb.IsChecked == null) return;

                DataRowView drv = cb.DataContext as DataRowView;

                foreach (DataRowView item in dgSearhResult2.ItemsSource)
                {
                    if (drv["BOXID"].ToString().Equals(item["BOXID"].ToString()))
                    {
                        item["CHK"] = false;
                    }
                }
                //drv["CHK"] = false;
                //row색 변경부분 추가

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
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

        #region [확인자]
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
    }
}
