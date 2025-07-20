/*************************************************************************************
 Created Date : 2017.09.26
      Creator : J.S HONG
   Decription : 활성화 비용처리 등록/취소 < 특이작업
--------------------------------------------------------------------------------------
 [Change History]
  2017.09.26  J.S HONG : Initial Created.

 
**************************************************************************************/

using C1.WPF;
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
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;


namespace LGC.GMES.MES.COM001
{
    public partial class COM001_116 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        public COM001_116()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;

            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
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

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show("시작일자 이전 날짜는 선택할 수 없습니다.", null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            #region [Combo]비용처리 대상 등록  

            //동
            _combo.SetCombo(cboAreaRegister, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            string[] sFilter1 = { "CHARGE_PROD_LOT" };
            _combo.SetCombo(cboResnCode, CommonCombo.ComboStatus.SELECT, sCase: "ACTIVITIREASON", sFilter: sFilter1);

            string[] sFilter2 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboCostSloc, CommonCombo.ComboStatus.SELECT, sCase: "COST_SLOC", sFilter: sFilter2);

            string[] sFilter3 = { LoginInfo.CFG_SHOP_ID };
            _combo.SetCombo(cboSlocByGmes, CommonCombo.ComboStatus.SELECT, sCase: "SLOC_BY_GMES_USE", sFilter: sFilter3);



            #endregion

            #region [Combo]비용처리 완료 등록

            //동
            _combo.SetCombo(cboAreaComplete, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            #endregion

            #region [Combo]비용처리 대상 등록 취소

            //동
            _combo.SetCombo(cboAreaCancel, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            #endregion

            #region [Combo]비용처리 대상 이력 조회

            //등록일시
            dtpDateFrom.SelectedDateTime = DateTime.Now;
            dtpDateTo.SelectedDateTime = DateTime.Now;

            //동
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            #endregion
        }
        #endregion

        #region Event

        #region 버튼권한
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;

                DataTable dtInfo = ary.GetValue(0) as DataTable;

                foreach (DataRow dr in dtInfo.Rows)
                {
                    txtLotidRegister.Text = Util.NVC(dr["PALLETID"]);
                    GetLotList_Register(false);
                }
            }

            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSaveRegister);
            listAuth.Add(btnSaveComplete);
            listAuth.Add(btnSaveCancel);
            listAuth.Add(btnSaveHistory);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회&저장]비용처리 대상 등록
        private void btnSearchRegister_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Register();
        }
      
        private void btnSaveRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Register())
                {
                    return;
                }
                //비용처리 대상 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4086"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Register();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]비용처리 완료 등록 
        private void btnSearchComplete_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Complete();
        }

        //비용처리 완료 등록
        private void btnSaveComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Complete())
                {
                    return;
                }
                //비용처리 완료 등록 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4087"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Complete();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [조회&저장]비용처리 대상 등록 취소
        private void btnSearchCancel_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_Cancel();
        }

        private void btnSaveCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_Cancel())
                {
                    return;
                }
                //비용처리 등록 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4088"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_Cancel();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [조회&저장]비용처리 대상 이력 조회
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetLotList_History();
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_History())
                {
                    return;
                }
                //비용처리 대상등록 완료 취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU4133"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                REQ_History();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region [대상 선택하기]

        #region 비용처리 대상 등록
        private void CheckBoxRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListRegister.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgSelectRegister.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);                    
                    dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectRegister.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectRegister.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 등록 완료
        private void CheckBoxComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListComplete.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgSelectComplete.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND HIST_SEQNO = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "HIST_SEQNO")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectComplete.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 등록 취소
        private void CheckBoxCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListCancel.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgSelectCancel.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND HIST_SEQNO = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "HIST_SEQNO")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectCancel.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 이력 조회
        private void CheckBoxHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListHistory.Rows[idx].DataItem;

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if (DataTableConverter.Convert(dgSelectHistory.ItemsSource).Select("LOTID = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")) + "' AND HIST_SEQNO = '" + Util.NVC(DataTableConverter.GetValue(objRowIdx, "HIST_SEQNO")) + "'").Length == 1)
                    {
                        Util.MessageValidation("SFU2840", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));   //LOT이 이미 선택되었습니다.
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }
                }

                if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                {
                    if(!Util.NVC(DataTableConverter.GetValue(objRowIdx, "FCS_PRDT_REQ_PROG_CODE")).Equals("COMPLETE"))
                    {
                        Util.MessageValidation("SFU4138", Util.NVC(DataTableConverter.GetValue(objRowIdx, "LOTID")));//해당LOTID[%1]는 완료 등록된 LOTID가 아닙니다.
                        DataTableConverter.SetValue(objRowIdx, "CHK", 0);
                        return;
                    }

                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    DataRow dr = dtTo.NewRow();
                    foreach (DataColumn dc in dtTo.Columns)
                    {
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(objRowIdx, dc.ColumnName));
                    }
                    dtTo.Rows.Add(dr);
                    dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                }
                else
                {
                    DataTable dtTo = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                    if (dtTo.Rows.Count > 0)
                    {
                        if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'").Length > 0)
                        {
                            dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(objRowIdx, "LOTID") + "'")[0]);
                            dgSelectHistory.ItemsSource = DataTableConverter.Convert(dtTo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #endregion

        #region BizWF 처리 담당자
        private void txtPrscUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        private void btnPrscUser_Click(object sender, RoutedEventArgs e)
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
                Parameters[0] = txtPrscUser.Text;
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
                txtPrscUser.Text = wndPerson.USERNAME;
                txtPrscUser.Tag = wndPerson.USERID;

                if (string.IsNullOrEmpty(txtDept.Text))
                {
                    txtDept.Text = wndPerson.DEPTNAME;
                    txtDept.Tag = wndPerson.DEPTID;
                }
                else
                {
                    txtDept.Text = string.Empty;
                    txtDept.Tag = string.Empty;
                }
            }
        }
        #endregion

        #region BizWF 부서
        private void txtDept_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetDepartmentWindow();
            }
        }

        private void btnDept_Click(object sender, RoutedEventArgs e)
        {
            GetDepartmentWindow();
        }

        private void GetDepartmentWindow()
        {
            CMM_DEPARTMENT wndDept = new CMM_DEPARTMENT();
            wndDept.FrameOperation = FrameOperation;

            if (wndDept != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtDept.Text;
                C1WindowExtension.SetParameters(wndDept, Parameters);

                wndDept.Closed += new EventHandler(wndDept_Closed);
                grdMain.Children.Add(wndDept);
                wndDept.BringToFront();
            }
        }
        private void wndDept_Closed(object sender, EventArgs e)
        {
            CMM_DEPARTMENT wndDept = sender as CMM_DEPARTMENT;
            if (wndDept.DialogResult == MessageBoxResult.OK)
            {
                txtDept.Text = wndDept.DEPTNAME;
                txtDept.Tag = wndDept.DEPTID;
            }
        }

        #endregion

        #region 입력란 표시 - 요청수량 Grid 색상변경
        private void dgSelectRegister_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Column.Name == "ACTQTY2")
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightYellow);
                }));
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [대상목록 가져오기]

        #region 비용처리 대상 등록 조회

        public void GetLotList_Register(bool bButton = true)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaRegister, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdRegister.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlRegister.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtRegister.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidRegister.Text.Trim());
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_COST_PRDT_REQ_HIST_WIP", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0 && bButton == true)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    return;
                }
                else if (dtRslt.Rows.Count == 0 && bButton == false)
                {
                    Util.MessageConfirm("SFU1905", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            txtLotidRegister.Focus();
                            txtLotidRegister.Text = string.Empty;
                        }
                    });
                    return;
                }

                if (dtRslt.Rows.Count > 0 && bButton == false)
                {
                    if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgListRegister.ItemsSource);
                        dtSource.Merge(dtRslt);
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtSource, FrameOperation, true);
                        DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }
                    else
                    {
                        Util.gridClear(dgListRegister);
                        Util.GridSetData(dgListRegister, dtRslt, FrameOperation, true); DataTable dtSelect = GetDtRegister();
                        Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                        txtLotidRegister.Text = string.Empty;
                        txtLotidRegister.Focus();
                    }


                }
                else
                {
                    Util.GridSetData(dgListRegister, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectRegister);

                    DataTable dtSelect = GetDtRegister();
                    Util.GridSetData(dgSelectRegister, dtSelect, FrameOperation, false);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 완료 등록 조회
        public void GetLotList_Complete()
        {
            try
            {
                const string sFCS_PRDT_REQ_PROG_CODE = "CREATE";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaComplete, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidComplete.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdComplete.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlComplete.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtComplete.Text.Trim());
                dr["FCS_PRDT_REQ_PROG_CODE"] = sFCS_PRDT_REQ_PROG_CODE;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FCS_PRDT_REQ_HIST_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectComplete);

                    return;
                }

                Util.GridSetData(dgListComplete, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectComplete);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectComplete, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 등록 취소 조회
        private void GetLotList_Cancel()
        {
            try
            {
                const string sFCS_PRDT_REQ_PROG_CODE = "CREATE";
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaCancel, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidCancel.Text.Trim());
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdCancel.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlCancel.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtCancel.Text.Trim());
                dr["FCS_PRDT_REQ_PROG_CODE"] = sFCS_PRDT_REQ_PROG_CODE;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FCS_PRDT_REQ_HIST_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectCancel);

                    return;
                }

                Util.GridSetData(dgListCancel, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectCancel);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectCancel, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 비용처리 대상 이력 조회 조회
        public void GetLotList_History()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));
                dtRqst.Columns.Add("FROMDATE", typeof(string));
                dtRqst.Columns.Add("TODATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHistory, "SFU3203");//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["PRODID"] = Util.ConvertEmptyToNull(txtProdHistory.Text.Trim());
                dr["MODLID"] = Util.ConvertEmptyToNull(txtModlHistory.Text.Trim());
                dr["PRJT_NAME"] = Util.ConvertEmptyToNull(txtPrjtHistory.Text.Trim());
                dr["LOTID"] = Util.ConvertEmptyToNull(txtLotidHistory.Text.Trim());
                dr["FCS_PRDT_REQ_PROG_CODE"] = null;
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_FCS_PRDT_REQ_HIST_LIST", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."

                    Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                    Util.gridClear(dgSelectHistory);

                    return;
                }

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, false);
                Util.gridClear(dgSelectHistory);

                DataTable dtSelect = GetDtComplete();
                Util.GridSetData(dgSelectHistory, dtSelect, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 선택목록 DataTable
        private DataTable GetDtRegister()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("EQSGID", typeof(string));
            dtSelect.Columns.Add("EQSGNAME", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("MODLID", typeof(string));
            dtSelect.Columns.Add("PRODNAME", typeof(string));
            dtSelect.Columns.Add("WIPQTY2", typeof(string));
            dtSelect.Columns.Add("ACTQTY2", typeof(string));
            dtSelect.Columns.Add("UNIT_CODE", typeof(string));

            return dtSelect;
        }

        private DataTable GetDtComplete()
        {
            DataTable dtSelect = new DataTable();

            dtSelect.Columns.Add("AREANAME", typeof(string));
            dtSelect.Columns.Add("LOTID", typeof(string));
            dtSelect.Columns.Add("HIST_SEQNO", typeof(decimal));
            dtSelect.Columns.Add("RESNCODE", typeof(string));
            dtSelect.Columns.Add("PRODID", typeof(string));
            dtSelect.Columns.Add("PRJT_NAME", typeof(string));
            dtSelect.Columns.Add("MODLID", typeof(string));
            dtSelect.Columns.Add("PRODNAME", typeof(string));
            dtSelect.Columns.Add("RESNQTY", typeof(string));
            dtSelect.Columns.Add("UNIT_CODE", typeof(string));
            dtSelect.Columns.Add("RESNNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_DEPTNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_USERNAME", typeof(string));
            dtSelect.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
            dtSelect.Columns.Add("REG_USERNAME", typeof(string));
            dtSelect.Columns.Add("REG_DTTM", typeof(string));
            dtSelect.Columns.Add("CMPL_USERNAME", typeof(string));
            dtSelect.Columns.Add("CMPL_DTTM", typeof(string));
            dtSelect.Columns.Add("PRDT_REQ_NOTE", typeof(string));
            dtSelect.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));

            return dtSelect;
        }
        #endregion
        #endregion

        #region [선택목록 등록]

        #region 비용처리 대상 등록
        private void REQ_Register()
        {
            try
            {
                DataSet inData = new DataSet();

                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("SLOC_ID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_DEPTID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_USERID", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_SCHD_CMPL_DATE", typeof(string));
                inDataTable.Columns.Add("COST_PRCS_SLOC_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["SLOC_ID"] =(string)cboSlocByGmes.SelectedValue;
                row["BIZ_WF_DEPTID"] = txtDept.Tag;
                row["BIZ_WF_PRCS_USERID"] = txtPrscUser.Tag;
                row["BIZ_WF_PRCS_SCHD_CMPL_DATE"] = ldpCmplDateRegister.SelectedDateTime.ToString("yyyyMMdd");
                row["COST_PRCS_SLOC_ID"] = (string)cboCostSloc.SelectedValue; ;
                row["USERID"] = LoginInfo.USERID;
                row["NOTE"] = txtNoteRegister.Text;
                inDataTable.Rows.Add(row);

                DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);

                //INRESN
                DataTable inResn = inData.Tables.Add("INRESN");
                inResn.Columns.Add("LOTID", typeof(string));
                inResn.Columns.Add("RESNCODE", typeof(string));
                inResn.Columns.Add("RESNQTY", typeof(decimal));
                inResn.Columns.Add("RESNNOTE", typeof(string));
                
                for (int i = 0; i < dtSelectRegister.Rows.Count; i++)
                {
                    row = inResn.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectRegister.Rows[i]["LOTID"]);
                    row["RESNCODE"] = (string)cboResnCode.SelectedValue;
                    row["RESNQTY"] = Convert.ToString(dtSelectRegister.Rows[i]["ACTQTY2"]);
                    row["RESNNOTE"] = txtNoteRegister.Text;
                    inResn.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_FCS_STOCK", "INDATA,INRESN", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Register();

                        cboResnCode.SelectedIndex = 0;
                        cboSlocByGmes.SelectedIndex = 0;
                        cboCostSloc.SelectedIndex = 0;
                        txtDept.Text = string.Empty;
                        txtDept.Tag = string.Empty;
                        txtPrscUser.Text = string.Empty;
                        txtPrscUser.Tag = string.Empty;
                        ldpCmplDateRegister.SelectedDateTime = DateTime.Now;
                        txtNoteRegister.Text = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_FCS_STOCK", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region 비용처리 완료 등록
        private void REQ_Complete()
        {
            try
            {
                const string sFCS_PRDT_REQ_PROG_CODE = "COMPLETE";
                DataSet inData = new DataSet();
                
                //INDATA
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("BIZ_WF_PRCS_END_DATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["FCS_PRDT_REQ_PROG_CODE"] = sFCS_PRDT_REQ_PROG_CODE;
                row["NOTE"] = txtNoteComplete.Text;
                row["BIZ_WF_PRCS_END_DATE"] = ldpCmplDateComplete.SelectedDateTime.ToString("yyyyMMdd");
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HIST_SEQNO", typeof(string));

                DataTable dtSelectComplete = DataTableConverter.Convert(dgSelectComplete.ItemsSource);
                for (int i = 0; i < dtSelectComplete.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectComplete.Rows[i]["LOTID"]);
                    row["HIST_SEQNO"] = Convert.ToString(dtSelectComplete.Rows[i]["HIST_SEQNO"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Complete();

                        ldpCmplDateComplete.SelectedDateTime = DateTime.Now;
                        txtNoteComplete.Text = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 비용처리 대상 등록 취소
        private void REQ_Cancel()
        {
            try
            {
                const string sFCS_PRDT_REQ_PROG_CODE = "CANCEL";
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["FCS_PRDT_REQ_PROG_CODE"] = sFCS_PRDT_REQ_PROG_CODE;
                row["NOTE"] = txtNoteCancel.Text;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HIST_SEQNO", typeof(decimal));

                DataTable dtSelectCancel = DataTableConverter.Convert(dgSelectCancel.ItemsSource);
                for (int i = 0; i < dtSelectCancel.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectCancel.Rows[i]["LOTID"]);
                    row["HIST_SEQNO"] = Convert.ToString(dtSelectCancel.Rows[i]["HIST_SEQNO"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_Cancel();

                        txtNoteCancel.Text = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #region 비용처리 대상 이력 조회
        private void REQ_History()
        {
            try
            {
                const string sFCS_PRDT_REQ_PROG_CODE = "CREATE";
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("FCS_PRDT_REQ_PROG_CODE", typeof(string));
                inDataTable.Columns.Add("NOTE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["IFMODE"] = "OFF";
                row["FCS_PRDT_REQ_PROG_CODE"] = sFCS_PRDT_REQ_PROG_CODE;
                row["NOTE"] = txtNoteHistory.Text;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HIST_SEQNO", typeof(decimal));

                DataTable dtSelectHistory = DataTableConverter.Convert(dgSelectHistory.ItemsSource);
                for (int i = 0; i < dtSelectHistory.Rows.Count; i++)
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Convert.ToString(dtSelectHistory.Rows[i]["LOTID"]);
                    row["HIST_SEQNO"] = Convert.ToString(dtSelectHistory.Rows[i]["HIST_SEQNO"]);
                    inLot.Rows.Add(row);
                }

                loadingIndicator.Visibility = Visibility.Visible;

                //비용처리 대상 완료등록 처리
                new ClientProxy().ExecuteService_Multi("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU3532");//저장되었습니다.

                        GetLotList_History();

                        txtNoteHistory.Text = string.Empty;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_ACT_REG_COST_PRDT_REQ_FCS_STATUS", ex.Message, ex.ToString());
            }
        }

        #endregion

        #endregion

        #region [Validation]

        #region 비용처리 대상 등록
        private bool Validation_Register()
        {
            if (dgSelectRegister.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            DataTable dtSelectRegister = DataTableConverter.Convert(dgSelectRegister.ItemsSource);

            int iACTQTY2 = dtSelectRegister.Select("ACTQTY2 = '' ").Count();

            if (iACTQTY2 > 0)
            {
                Util.MessageValidation("SFU4135"); //요청수량을 입력 하세요.
                return false;
            }

            if (cboResnCode.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU1593"); //사유를 선택하세요.
                return false;
            }

            if (cboSlocByGmes.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4136"); //저장위치를 선택해 주세요.
                return false;
            }

            if (cboCostSloc.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4137"); //비용창고를 선택해 주세요.
                return false;
            }

            //if (String.IsNullOrEmpty((String)txtDept.Tag))
            //{
            //    Util.MessageValidation("SFU4105"); //BizWF 처리 부서를 선택해 주세요.
            //    return false;
            //}

            if (String.IsNullOrEmpty((String)txtPrscUser.Tag))
            {
                Util.MessageValidation("SFU4106"); //BizWF 처리 담당자를 선택해 주세요.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 완료 등록
        private bool Validation_Complete()
        {
            if (dgSelectComplete.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 등록 취소
        private bool Validation_Cancel()
        {
            if (dgSelectCancel.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region 비용처리 대상 이력조회
        private bool Validation_History()
        {
            if (dgSelectHistory.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                return false;
            }

            return true;
        }


        #endregion

        #endregion

        #endregion

        private void txtLotidRegister_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sLotid = txtLotidRegister.Text.Trim();
                    if (dgListRegister.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListRegister.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListRegister.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {

                                dgListRegister.SelectedIndex = i;
                                dgListRegister.ScrollIntoView(i, dgListRegister.Columns["CHK"].Index);
                                txtLotidRegister.Focus();
                                txtLotidRegister.Text = string.Empty;
                                return;
                            }
                        }
                    }
                    GetLotList_Register(false);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
