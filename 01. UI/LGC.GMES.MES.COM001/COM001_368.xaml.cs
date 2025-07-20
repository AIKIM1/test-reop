/*************************************************************************************
 Created Date : 2022.04.18
      Creator : 이춘우
   Decription : 특이사항 LOT 등록 관리
--------------------------------------------------------------------------------------
 [Change History]
  2022.04.18  최초착성.
 
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
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_368 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

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

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preHist = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllHist = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public COM001_368()
        {
            InitializeComponent();
            InitCombo();
            InitControl();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion Declaration & Constructor

        #region Initialize

        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            // LOT 지정 Tab
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_PCSGID");

            cboProcess.SelectedValue = "E7000";

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            // LOT 지정 해제 Tab
            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcess };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHistory, sCase: "PROCESS_PCSGID");

            cboProcessHistory.SelectedValue = "E7000";

            // 극성
            String[] sFilter2 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecTypeHistory, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");

            SetGridCboItem(dgListHistory.Columns["USE_FLAG"], "USE_FLAG");
        }

        private void InitControl()
        {
            dgListSelect.Columns["BTCH_NOTE"].IsReadOnly = false;

            rdoReg.IsChecked = true;
            rdoRegAll.IsChecked = false;

            btnInputAll.IsEnabled = false;

            txtNote.Text = "";
            txtNote.IsEnabled = false;
        }

        #endregion Initialize

        #region Event

        private void UserControl_Loaded(object sender, EventArgs e)
        {
            ldpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            ldpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;

            ldpDateFromHist.SelectedDataTimeChanged += dtpDateFromHist_SelectedDataTimeChanged;
            ldpDateToHist.SelectedDataTimeChanged += dtpDateToHist_SelectedDataTimeChanged;
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                ldpDateTo.SelectedDateTime = dtPik.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                ldpDateFrom.SelectedDateTime = dtPik.SelectedDateTime;
                return;
            }
        }

        private void dtpDateFromHist_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateToHist.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                ldpDateToHist.SelectedDateTime = dtPik.SelectedDateTime;
                return;
            }
        }

        private void dtpDateToHist_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(ldpDateFromHist.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                ldpDateFromHist.SelectedDateTime = dtPik.SelectedDateTime;
                return;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetListHistory();
        }

        private void txtMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetList();
            }
        }

        private void txtHistory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetListHistory();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgListSelect.Rows.Count == 0)
                    return;

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LotSave();
                        GetList();

                        if (dgListHistory.Rows.Count > 0)
                        {
                            GetListHistory();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgListHistory.Rows.Count == 0)
                    return;

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LotHistorySave();
                        GetListHistory();

                        // 삭제 후 LOT지정 탭 재조회
                        if (dgListMain.Rows.Count > 0)
                            GetList();
                    }
                });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnInputAll_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtNote.Text))
                return;

            for (int i = 0; i < dgListSelect.Rows.Count; i++)
            {
                if (DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "CHK").ToString() == "0" ||
                    DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "CHK").ToString() == "False")
                    continue;

                DataTableConverter.SetValue(dgListSelect.Rows[i].DataItem, "BTCH_NOTE", txtNote.Text);
            }
        }

        private void rdoReg_Checked(object sender, RoutedEventArgs e)
        {
            dgListSelect.Columns["BTCH_NOTE"].IsReadOnly = false;

            btnInputAll.IsEnabled = false;

            txtNote.Text = "";
            txtNote.IsEnabled = false;
        }

        private void rdoRegAll_Checked(object sender, RoutedEventArgs e)
        {
            dgListSelect.Columns["BTCH_NOTE"].IsReadOnly = true;

            btnInputAll.IsEnabled = true;
            txtNote.IsEnabled = true;

            if (dgListSelect.Rows.Count == 0)
                return;

            if (dgListSelect.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListSelect.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }

        #endregion Event

        #region Check ALL
        private void dgListMain_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListMain.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListMain.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListMain.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListMain.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgListSelect);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListMain.ItemsSource);
            dtSelect = dtTo.Copy();

            dgListSelect.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgListSelect);
        }

        private void dgListHistory_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preHist.Content = chkAllHist;
                        e.Column.HeaderPresenter.Content = preHist;
                        chkAllHist.Checked -= new RoutedEventHandler(checkAllHist_Checked);
                        chkAllHist.Unchecked -= new RoutedEventHandler(checkAllHist_Unchecked);
                        chkAllHist.Checked += new RoutedEventHandler(checkAllHist_Checked);
                        chkAllHist.Unchecked += new RoutedEventHandler(checkAllHist_Unchecked);
                    }
                }
            }));
        }

        void checkAllHist_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListHistory.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHistory.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }

        private void checkAllHist_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHistory.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHistory.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }

        #endregion Check ALL

        #region 대상 선택하기
        private void chkMain_Click(object sender, RoutedEventArgs e)
        {
            dgListMain.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1)) //체크되는 경우
            {
                DataTable dtTo = DataTableConverter.Convert(dgListSelect.ItemsSource);

                if (dtTo.Columns.Count == 0) //최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                    dtTo.Columns.Add("EQSGNAME", typeof(string));
                    dtTo.Columns.Add("LOTID", typeof(string));
                    dtTo.Columns.Add("CSTID", typeof(string));
                    dtTo.Columns.Add("PRODID", typeof(string));
                    dtTo.Columns.Add("PRODNAME", typeof(string));
                    dtTo.Columns.Add("MODELID", typeof(string));
                    dtTo.Columns.Add("WIPQTY", typeof(string));
                    dtTo.Columns.Add("UNIT_CODE", typeof(string));
                    dtTo.Columns.Add("WIPDTTM", typeof(string));
                    dtTo.Columns.Add("PRJT_NAME", typeof(string));
                    dtTo.Columns.Add("PROD_VER_CODE", typeof(string));
                    dtTo.Columns.Add("LOTID_RT", typeof(string));
                    dtTo.Columns.Add("BTCH_NOTE", typeof(string));
                }

                if (dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'").Length > 0) //중복조건 체크
                {
                    return;
                }

                DataRow dr = dtTo.NewRow();

                foreach (DataColumn dc in dtTo.Columns)
                {
                    if (dc.DataType.Equals(typeof(Boolean)))
                    {
                        dr[dc.ColumnName] = DataTableConverter.GetValue(cb.DataContext, dc.ColumnName);
                    }
                    else
                    {
                        dr[dc.ColumnName] =  Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                    }
                }

                dtTo.Rows.Add(dr);
                dgListSelect.ItemsSource = DataTableConverter.Convert(dtTo);

                DataRow[] drUnchk = DataTableConverter.Convert(dgListMain.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }

            }
            else //체크 풀릴 때
            {
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                DataTable dtTo = DataTableConverter.Convert(dgListSelect.ItemsSource);

                dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);

                dgListSelect.ItemsSource = DataTableConverter.Convert(dtTo);
            }
        }

        private void chkHist_Click(object sender, RoutedEventArgs e)
        {
            dgListMain.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))
            {
                DataTable dt = DataTableConverter.Convert(dgListHistory.ItemsSource);

                DataRow[] drUnchk = DataTableConverter.Convert(dgListHistory.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAllHist.Checked -= new RoutedEventHandler(checkAllHist_Checked);
                    chkAllHist.IsChecked = true;
                    chkAllHist.Checked += new RoutedEventHandler(checkAllHist_Checked);
                }
            }
            else
            {
                chkAllHist.Unchecked -= new RoutedEventHandler(checkAllHist_Unchecked);
                chkAllHist.IsChecked = false;
                chkAllHist.Unchecked += new RoutedEventHandler(checkAllHist_Unchecked);
            }
        }

        #endregion 대상 선택하기

        #region Mehod

        public bool GetList()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgListMain);
                Util.gridClear(dgListSelect);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("ELEC_TYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("DATE_FROM", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223"); // 라인을 선택 하세요.
                dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);

                if (cboElecType.Visibility == Visibility.Visible && cboElecType.SelectedIndex > 0)
                    dr["ELEC_TYPE"] = Util.GetCondition(cboElecType, bAllNull: true);

                if (txtLotID.Text != string.Empty)
                {
                    dr["LOTID"] = txtLotID.Text;
                }

                if (txtCSTID.Text != string.Empty)
                {
                    dr["CSTID"] = txtCSTID.Text;
                }

                if (txtProdID.Text != string.Empty)
                {
                    dr["PRODID"] = txtProdID.Text;
                }

                if (txtPrjtName.Text != string.Empty)
                {
                    dr["PRJT_NAME"] = txtPrjtName.Text;
                }

                dr["DATE_FROM"] = ldpDateFrom.SelectedDateTime.ToShortDateString();
                dr["DATE_TO"] = ldpDateTo.SelectedDateTime.ToShortDateString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_RMK_LOT_REG_MNG", "INDATA", "OUTDATA", dtRqst);

                HiddenLoadingIndicator();

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905");  //조회된 Data가 없습니다.
                    return false;
                }

                Util.GridSetData(dgListMain, dtRslt, FrameOperation, true);
                InitControl();

                //chkAll.IsChecked = false;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

                return false;
            }
        }

        public bool GetListHistory()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                Util.gridClear(dgListHistory);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("ELEC_TYPE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("DATE_FROM", typeof(string));
                dtRqst.Columns.Add("DATE_TO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, "SFU1223"); // 라인을 선택 하세요.
                dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);

                if (cboElecTypeHistory.Visibility == Visibility.Visible && cboElecTypeHistory.SelectedIndex > 0)
                    dr["ELEC_TYPE"] = Util.GetCondition(cboElecTypeHistory, bAllNull: true);

                if (txtLotIDHistory.Text != string.Empty)
                {
                    dr["LOTID"] = txtLotIDHistory.Text;
                }

                if (txtCSTIDHistory.Text != string.Empty)
                {
                    dr["CSTID"] = txtCSTIDHistory.Text;
                }

                if (txtProdIDHistory.Text != string.Empty)
                {
                    dr["PRODID"] = txtProdIDHistory.Text;
                }

                if (txtPrjtNameHistory.Text != string.Empty)
                {
                    dr["PRJT_NAME"] = txtPrjtNameHistory.Text;
                }

                dr["DATE_FROM"] = ldpDateFromHist.SelectedDateTime.ToShortDateString();
                dr["DATE_TO"] = ldpDateToHist.SelectedDateTime.ToShortDateString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_ELTR_VISUAL_MNGT_LOT", "INDATA", "OUTDATA", dtRqst);

                HiddenLoadingIndicator();

                if (dtRslt.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU1905"); //조회된 Data가 없습니다.
                    return false;
                }

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);

                //chkAllHist.IsChecked = true;

                //for (int i = 0; i < dgListHistory.Rows.Count; i++)
                //{
                //    DataTableConverter.SetValue(dgListHistory.Rows[i].DataItem, "CHK", 1);
                //}

                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);

                return false;
            }
        }

        private void LotSave()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("BTCH_NOTE", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("PACK_ALARM_FLAG", typeof(string));

                for (int i = 0; i < dgListSelect.Rows.Count; i++)
                {
                    if ((DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "CHK").ToString() == "1" ||
                        DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "CHK").ToString() == "True") &&
                        !string.IsNullOrEmpty(DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "BTCH_NOTE").ToString()))
                    {
                        DataRow dr = inTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "LOTID");
                        dr["PRODID"] = DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "PRODID");
                        dr["BTCH_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgListSelect.Rows[i].DataItem, "BTCH_NOTE"));
                        dr["DEL_FLAG"] = "N";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["PACK_ALARM_FLAG"] = "Y";

                        inTable.Rows.Add(dr);
                    }
                }

                if (inTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ELTR_VISUAL_MNGT_LOT", "INDATA", null, inTable);

                    inTable.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void LotHistorySave()
        {
            try
            {
                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("BTCH_NOTE", typeof(string));
                inTable.Columns.Add("DEL_FLAG", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("PACK_ALARM_FLAG", typeof(string));

                for (int i = 0; i < dgListHistory.Rows.Count; i++)
                {
                    if (DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK").ToString() == "1" ||
                        DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "CHK").ToString() == "True")
                    {
                        DataRow dr = inTable.NewRow();
                        dr["LOTID"] = DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "LOTID");
                        dr["PRODID"] = DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "PRODID");
                        dr["BTCH_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "BTCH_NOTE"));
                        dr["DEL_FLAG"] = "N";
                        dr["INSUSER"] = LoginInfo.USERID;
                        dr["UPDUSER"] = LoginInfo.USERID;
                        dr["PACK_ALARM_FLAG"] = Util.NVC(DataTableConverter.GetValue(dgListHistory.Rows[i].DataItem, "USE_FLAG"));

                        inTable.Rows.Add(dr);
                    }
                }

                if (inTable.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync("BR_PRD_REG_ELTR_VISUAL_MNGT_LOT", "INDATA", null, inTable);

                    inTable.Rows.Clear();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private bool SetGridCboItem(C1.WPF.DataGrid.DataGridColumn col, string sClassId, bool bCodeDisplay = false, string sCmnCd = null)
        {
            try
            {
                bool rtn = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sClassId;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CMN_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    rtn = false;
                }
                else
                {
                    rtn = true;
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).DisplayMemberPath = "CBO_NAME";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).SelectedValuePath = "CBO_CODE";
                    (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtResult);
                }

                return rtn;
            }
            catch (Exception ex)
            {
                throw ex;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }




        #endregion Mehod

        
    }
}
