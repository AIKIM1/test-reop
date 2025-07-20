/*************************************************************************************
 Created Date : 2020.08.24
      Creator : 신 광희
   Decription : QA 품질검사결과 등록 (조립) COM001_006 COPY 
--------------------------------------------------------------------------------------
 [Change History]
  2020.08.24  신 광희 : Initial Created.
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
using System.Windows.Threading;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_330 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private readonly Util _util = new Util();

        DateTime dCalDate;

        List<string> LotList = new List<string>();
        List<string> CSTList = new List<string>();

        public COM001_330()
        {
            InitializeComponent();
            InitCombo();
            
            this.Loaded += UserControl_Loaded;
        }

        DataGridRowHeaderPresenter pre = new DataGridRowHeaderPresenter()
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


        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo combo = new CommonCombo();
            
            //동
            C1ComboBox[] cboAreaChildHold = { cboEquipmentSegmentHold };
            combo.SetCombo(cboAreaHold, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHold, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHold = { cboAreaHold };
            C1ComboBox[] cboEquipmentSegmentChildHold = { cboProcessHold };
            combo.SetCombo(cboEquipmentSegmentHold, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildHold, cbParent: cboEquipmentSegmentParentHold, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHold = { cboEquipmentSegmentHold };
            //C1ComboBox[] cboProcessChildHold = { cboEquipmentHold };
            combo.SetCombo(cboProcessHold, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHold, sCase: "PROCESS_PCSGID");
            //cboProcessHold.SelectedValue = "E7000"; //전극창고 대기

            //설비
            //string[] sFilterEqpt = { cboEquipmentSegmentHold.SelectedValue.ToString(), "E4000", null };
            //_combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, sCase: "EQUIPMENT", sFilter : sFilterEqpt);

            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcessHistory};
            combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL,  cbParent: cboProcessParentHistory, sCase: "PROCESS");

            //HOLD 사유 LVL1
            //string[] sFilter = {"HOLD_LOT"};
            //_combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, sCase: "HOLD_CODE_LVL1", sFilter: sFilter);

            //Level 1
            GetLevel1List();


        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHold);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

            dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);

            dCalDate = GetComSelCalDate();
            dtCalDate.SelectedDateTime = dCalDate;
            this.Loaded -= UserControl_Loaded;
        }

        private void popSearchLevel1_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "HOLD_LOT";
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DFCT_CODE"] = popSearchLevel1.SelectedValue;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOT_HOLD_DFCT_CODE_LVL2_CBO", "INDATA", "OUTDATA", RQSTDT);

                popSearchLevel2.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        //private void cboHoldType_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        //{
        //    CommonCombo _combo = new CommonCombo();

        //    //HOLD 사유 LVL2
        //    string[] sFilter = { "HOLD_LOT",LoginInfo.CFG_AREA_ID,cboHoldType.SelectedValue.ToString() };
        //    _combo.SetCombo(cboHoldType2, CommonCombo.ComboStatus.SELECT, sCase: "HOLD_CODE_LVL2", sFilter: sFilter);
        //}

        //private void txtSearchLot_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == System.Windows.Input.Key.Enter)
        //    {
        //        if (!string.IsNullOrEmpty(txtSearchLot.Text.Trim()))
        //        {
        //            SearchLot(txtSearchLot.Text.Trim());
        //        }
        //    }
        //}

        private void btnSearchHold_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLotIdHold.Text))
            {
                GetLotList("", true, true, txtCSTIdHold.Text);
            }
            else
            {
                GetLotList(txtLotIdHold.Text.Trim(), true, true);
            }
        }
        
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetHoldHistory();
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //저장하시겠습니까?
                Util.MessageConfirm("SFU1241", result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LotHold();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.AlertInfo("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }

        private void dtCalDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("DATE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ERP_CLOSING_FLAG", "RQSTDT", "RSLTDT", RQSTDT);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                dtCalDate.SelectedDateTime = dCalDate;
            }
        }

        #region [Check ALL]
        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();

            chkAllSelect();
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();

            chkAllClear();
        }

        private void chkAllSelect()
        {
            Util.gridClear(dgSelectHold);

            DataTable dtSelect = new DataTable();

            DataTable dtTo = DataTableConverter.Convert(dgListHold.ItemsSource);
            dtSelect = dtTo.Copy();

            dgSelectHold.ItemsSource = DataTableConverter.Convert(dtSelect);

        }

        private void chkAllClear()
        {
            Util.gridClear(dgSelectHold);
        }

        #endregion

        #region [대상 선택하기]
        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            dgListHold.Selection.Clear();

            CheckBox cb = sender as CheckBox;
            
                Logger.Instance.WriteLine("COM001_330_N", "LOT_ID: " + DataTableConverter.GetValue(cb.DataContext, "LOTID"), LogCategory.FRAME);

            //if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
            if (Util.NVC_Int(DataTableConverter.GetValue(cb.DataContext, "CHK")) == 1)
            {                
                DataTable dtTo = DataTableConverter.Convert(dgSelectHold.ItemsSource);

                if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                {
                    dtTo.Columns.Add("CHK", typeof(Boolean));
                    dtTo.Columns.Add("EQSGNAME", typeof(string));
                    //dtTo.Columns.Add("EQPTNAME", typeof(string));
                    dtTo.Columns.Add("LOTID", typeof(string));
                    dtTo.Columns.Add("PRODID", typeof(string));
                    dtTo.Columns.Add("PRODNAME", typeof(string));
                    dtTo.Columns.Add("MODELID", typeof(string));
                    dtTo.Columns.Add("UNIT_CODE", typeof(string));
                    dtTo.Columns.Add("WIPQTY", typeof(string));
                    dtTo.Columns.Add("PROD_VER_CODE", typeof(string));
                    dtTo.Columns.Add("CSTID", typeof(string));
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
                        dr[dc.ColumnName] = Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
                    }
                }

                dtTo.Rows.Add(dr);
                dgSelectHold.ItemsSource = DataTableConverter.Convert(dtTo);

                DataRow[] drUnchk = DataTableConverter.Convert(dgListHold.ItemsSource).Select("CHK = 0");

                if (drUnchk.Length == 0)
                {
                    chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                    chkAll.IsChecked = true;
                    chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                }

            }
            else//체크 풀릴때
            {             
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                DataTable dtTo = DataTableConverter.Convert(dgSelectHold.ItemsSource);

                dtTo.Rows.Remove(dtTo.Select("LOTID = '" + DataTableConverter.GetValue(cb.DataContext, "LOTID") + "'")[0]);

                dgSelectHold.ItemsSource = DataTableConverter.Convert(dtTo);
            }
        }

        #endregion

        #region [담당자]
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtPerson.Text;
                    dr["LANGID"] = LoginInfo.LANGID;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {
                        txtPerson.Text = dtRslt.Rows[0]["USERNAME"].ToString();
                        txtPersonId.Text = dtRslt.Rows[0]["USERID"].ToString();
                        txtPersonDept.Text = dtRslt.Rows[0]["DEPTNAME"].ToString();
                    }
                    else
                    {
                        dgPersonSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgPersonSelect);

                        dgPersonSelect.ItemsSource = DataTableConverter.Convert(dtRslt);
                        this.Focusable = true;
                        this.Focus();
                        this.Focusable = false;
                    }

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }
        #endregion

        #region [담당자 검색결과 여러개일경우]
        private void dgPersonSelect_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            txtPersonId.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID").ToString());
            txtPerson.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME").ToString());
            txtPersonDept.Text = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dgPersonSelect.Visibility = Visibility.Collapsed;

        }
        #endregion
        
        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
        }

        private void txtLotIdHold_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    if (txtLotIdHold.Text.Trim() == string.Empty)
                        return;

                    string sLotid = txtLotIdHold.Text.Trim();
                    if (dgListHold.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                Util.MessageInfo("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                return;
                            }
                        }
                    }
                    GetLotList(sLotid, false, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCSTIdHold_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCSTIdHold.Text.Trim() == string.Empty)
                        return;

                    string sCSTID = txtCSTIdHold.Text.Trim();
                    if (dgListHold.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "CSTID").ToString() == sCSTID)
                            {
                                Util.MessageValidation("SFU1504");    //동일한 LOT이 스캔되었습니다.
                                return;
                            }
                        }
                    }
                    GetLotList("", false, true, sCSTID);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCarrierIdHold_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCarrierIdHold.Text.Trim() == string.Empty)
                        return;

                    string sLotid = SearhCarrierId(txtCarrierIdHold.Text.Trim());

                    if (string.IsNullOrEmpty(sLotid))
                        return;

                    if (dgListHold.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "LOTID").ToString() == sLotid)
                            {
                                Util.Alert("SFU1504");   //동일한 LOT이 스캔되었습니다.
                                return;
                            }
                        }
                    }
                    GetLotList(sLotid, false, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotIdHold_PreviewKeyDown(object sender, KeyEventArgs e)
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

                    string valueToFind = string.Empty;

                    LotList.Clear();

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList(sPasteStrings[i], false, false) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        valueToFind = string.Join(",", LotList);

                        if (valueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", valueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }

                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtLotIdHold.Text = string.Empty;
                    txtLotIdHold.Focus();
                }
            }
        }

        private void txtCSTIdHold_PreviewKeyDown(object sender, KeyEventArgs e)
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

                    string valueToFind = string.Empty;

                    CSTList.Clear();

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList("", false, false, sPasteStrings[i]) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        valueToFind = string.Join(",", CSTList);

                        if (valueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", valueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
                        }

                        e.Handled = true;
                    }
                    else
                        e.Handled = false;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    txtCSTIdHold.Text = string.Empty;
                    txtCSTIdHold.Focus();
                }
            }
        }

        private void txtLotHistory_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetHoldHistory();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        private void GetLevel1List()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = "HOLD_LOT";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_DFCT_CODE_CBO", "INDATA", "OUTDATA", dtRqst);

                popSearchLevel1.ItemsSource = DataTableConverter.Convert(dtRslt);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void checkBoxHandle(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e, string sColumeName, string sLotIDColumnName)
        {
            C1DataGrid dg = sender as C1DataGrid;

            CheckBox chk = e.Cell?.Presenter?.Content as CheckBox;

            if (e.Cell.Column.Name.Equals(sColumeName))
            {
                var checkBox = dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content as CheckBox;
                if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                         dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                         checkBox.IsChecked.HasValue &&
                                         (bool)checkBox.IsChecked))
                {
                    chk.IsEnabled = false;


                }
                else if (checkBox != null && (dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter != null &&
                                              dg.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Content != null &&
                                              string.IsNullOrEmpty(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sLotIDColumnName)))))
                {
                    chk.IsEnabled = false;
                }

            }
        }

        private void ForegroundColorChange(C1.WPF.DataGrid.DataGridCellEventArgs e, string sColumeName)
        {
            if (e.Cell.Column.Name.Equals(sColumeName))
            {
                if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sColumeName)).Equals("NG"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("RED"));
                }
                else if ((Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sColumeName)).Equals("OK")) ||
                        (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, sColumeName)).Equals("Release")))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("BLUE"));
                }
            }
        }

        private DateTime GetComSelCalDate()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_CALDATE", "RQSTDT", "RSLTDT", RQSTDT);

                DateTime dCalDate;

                if (dtResult != null && dtResult.Rows.Count > 0)
                    dCalDate = Convert.ToDateTime(Util.NVC(dtResult.Rows[0]["CALDATE"]));
                else
                    dCalDate = DateTime.Now;

                return dCalDate;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return DateTime.Now;
            }
        }

        #region [작업대상 가져오기]
        private bool GetLotList()
        {
            try
            {
                dgListHold.ItemsSource = null;
                dgSelectHold.ItemsSource = null;

                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("CALDATE_FROM", typeof(string));
                dtRqst.Columns.Add("CALDATE_TO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHold, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                if (dr["EQSGID"].Equals("")) return false;
                //dr["EQPTID"] = Util.GetCondition(cboEquipment, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessHold, bAllNull: true);
                dr["CALDATE_FROM"] = ldpCaldateFrom.SelectedDateTime.ToShortDateString();
                dr["CALDATE_TO"] = ldpCaldateTo.SelectedDateTime.ToShortDateString();

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_FOR_QA_HOLD", "INDATA", "OUTDATA", dtRqst);

                
                //chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                //chkAll.IsChecked = false;
                //chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                if (dgListHold.GetRowCount() > 0)
                {
                    dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                }
                else
                {
                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    //Util.gridClear(dgSelectHold);
                }

                return true;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return false;
            }
        }

        private bool GetLotList(string lotId, bool bButton, bool vOnlyOne, string carrierId = "")
        {
            try
            {
                DoEvents();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("WIPHOLD", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PRJT_NAME", typeof(string));
                dtRqst.Columns.Add("ELEC_TYPE", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("CALDATE_FROM", typeof(string));
                dtRqst.Columns.Add("CALDATE_TO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["WIPHOLD"] = "N"; //-현재 HOLD 아닌것들

                //if (Util.GetCondition(txtLotHold).Equals("")) //lot id 가 없는 경우
                if (lotId.Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHold, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return false;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessHold, bAllNull: true);

                    //dr["EQPTID"] = Util.GetCondition(cboEquipmentHold, bAllNull:true);
                    // dr["EQPTID"] = null;

                    dr["MODLID"] = txtModelIdHold.Text;
                    dr["PRJT_NAME"] = txtProject.Text;
                    dr["CALDATE_FROM"] = ldpCaldateFrom.SelectedDateTime.ToShortDateString();
                    dr["CALDATE_TO"] = ldpCaldateTo.SelectedDateTime.ToShortDateString();

                    #region CSTID
                    if (!string.Equals(carrierId, ""))
                        dr["CSTID"] = carrierId;
                    #endregion
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = lotId; // Util.GetCondition(sLotId);
                }


                dtRqst.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(dtRqst);
                string xml = ds.GetXml();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_FIX_WIPSTAT", "INDATA", "OUTDATA", dtRqst);

                if (vOnlyOne)
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.MessageInfo("SFU1905");    //"조회된 Data가 없습니다."
                        return false;
                    }
                    else if (dtRslt.Rows[0]["WIPHOLD"].ToString() == "Y")
                    {
                        Util.MessageInfo("SFU1340");    //"HOLD 된 LOT ID 입니다."
                        return false;
                    }
                }
                else
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        LotList.Add(lotId);
                        return true;
                    }
                    else if (dtRqst.Rows[0]["WIPHOLD"].ToString() == "Y")
                    {
                        LotList.Add(lotId);
                        return true;
                    }
                }
                chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                chkAll.IsChecked = false;
                chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                if (dgListHold.GetRowCount() > 0 && bButton == false)
                {
                    if (dgListHold.GetRowCount() == 0)
                    {
                        dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                    }
                    else
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "LOTID").ToString() == dtRslt.Rows[0]["LOTID"].ToString())
                            {
                                //Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                                LotList.Add(lotId);
                                return true;
                            }
                        }
                        DataTable dtSource = DataTableConverter.Convert(dgListHold.ItemsSource);
                        dtSource.Merge(dtRslt);

                        Util.gridClear(dgListHold);
                        dgListHold.ItemsSource = DataTableConverter.Convert(dtSource);
                    }
                }
                else
                {
                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                    Util.gridClear(dgSelectHold);
                }

                if (!bButton)
                    chkAll.IsChecked = true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        
        #endregion

        #region [이력 가져오기]
        public void GetHoldHistory()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCarrierIdHistory.Text.Trim()))
                {
                    string sLotid = SearhCarrierIdHistory(txtCarrierIdHistory.Text.Trim());
                    dr["LOTID"] = sLotid;
                }
                else if (!string.IsNullOrEmpty(txtLotIdHistory.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotIdHistory);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProductId.Text) ? null : txtProductId.Text;
                    dr["CSTID"] = string.IsNullOrWhiteSpace(txtCSTIdHistory.Text) ? null : txtCSTIdHistory.Text;
                }
                dtRqst.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(dtRqst);
                string xml = ds.GetXml();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHOLDHISTORY", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgListHistory, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [hold 처리]
        private void LotHold()
        {
            string holdType = string.Empty;

            if (popSearchLevel2.SelectedValue == null)
            {
                //HOLD 사유를 선택 하세요.
                Util.MessageValidation("SFU1342");
                return;
            }

            holdType = popSearchLevel2.SelectedValue.ToString();

            string sPerson = Util.GetCondition(txtPersonId);

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("ACTION_USERID", typeof(string));
            inDataTable.Columns.Add("CALDATE", typeof(string));

            DataRow row = null;
            
            row = inDataTable.NewRow();
            row["SRCTYPE"] = "UI";
            row["LANGID"] = LoginInfo.LANGID;
            row["IFMODE"] = "OFF";
            row["USERID"] = LoginInfo.USERID;
            row["ACTION_USERID"] = txtPersonId.Text;
            row["CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");
            //row["CALDATE"] = dtCalDate.SelectedDateTime;
            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("HOLD_NOTE", typeof(string));
            inLot.Columns.Add("RESNCODE", typeof(string));
            inLot.Columns.Add("HOLD_CODE", typeof(string));
            inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
            inLot.Columns.Add("QA_INSP_JUDG_VALUE", typeof(string));

            DataTable dtSelectHold = DataTableConverter.Convert(dgSelectHold.ItemsSource);

            for (int i = 0; i < dtSelectHold.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    row["HOLD_NOTE"] = Util.GetCondition(txtHold);
                    row["RESNCODE"] = holdType;
                    row["HOLD_CODE"] = holdType;
                    row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);
                    row["QA_INSP_JUDG_VALUE"] = "NG";
                    inLot.Rows.Add(row);
                }
            }

            if (inLot.Rows.Count == 0)
            {
                //선택된 LOT이 없습니다.
                Util.Alert("SFU1261");
                return;
            }

            string xml = inData.GetXml();
            
            try
            {
                //HOLD 처리
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_HOLD_LOT_INSP_QA", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        else
                        {
                            dgSelectHold.ItemsSource = null;
                            
                            //정상 처리 되었습니다.
                            Util.MessageValidation("SFU1275", (action) =>
                            {
                                txtHold.Text = string.Empty;
                                txtPerson.Text = string.Empty;
                                txtPersonId.Text = string.Empty;
                                txtPersonDept.Text = string.Empty;

                                popSearchLevel1.SelectedValue = string.Empty;
                                popSearchLevel2.SelectedValue = string.Empty;

                                //다시 조회
                                //GetLotList();
                                btnSearchHold_Click(btnSearchHold, null);

                            });
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inData
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [이력에서 출력]
        private void print_Button_Click(object sender, RoutedEventArgs e)
        {
            //라벨 발행용
            DataRow row1 = null;
            DataTable dtLabel = new DataTable();
            dtLabel.Columns.Add("LOTID", typeof(string));
            dtLabel.Columns.Add("RESNNAME", typeof(string));
            dtLabel.Columns.Add("MODELID", typeof(string));
            dtLabel.Columns.Add("WIPQTY", typeof(string));
            dtLabel.Columns.Add("PERSON", typeof(string));

            try
            {
                Button bt = sender as Button;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).DataGrid;


                dg.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)bt.Parent).Cell.Row.Index;


                row1 = dtLabel.NewRow();
                row1["LOTID"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "LOTID"));
                row1["RESNNAME"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "HOLDCODENAME"));
                row1["MODELID"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MODELID"));
                row1["WIPQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "WIPQTY"))).ToString("###,###,##0.##");
                row1["PERSON"] = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ACTIONUSERNAME")); ;
                dtLabel.Rows.Add(row1);

                PrintHoldLabel(dtLabel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }
        #endregion

        #region [출력]
        private void PrintHoldLabel(DataTable inData)
        {
            try
            {
                //x,y 가져오기
                DataTable dt = LoginInfo.CFG_SERIAL_PRINT;

                string startX = "0";
                string startY = "0";
                if (dt.Rows.Count > 0)
                {
                    startX = dt.Rows[0]["X"].ToString();
                    startY = dt.Rows[0]["Y"].ToString();
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));
                dtRqst.Columns.Add("ATTVAL003", typeof(string));
                dtRqst.Columns.Add("ATTVAL004", typeof(string));
                dtRqst.Columns.Add("ATTVAL005", typeof(string));
                dtRqst.Columns.Add("ATTVAL006", typeof(string));
                dtRqst.Columns.Add("ATTVAL007", typeof(string));
                dtRqst.Columns.Add("ATTVAL008", typeof(string));
                dtRqst.Columns.Add("ATTVAL009", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                for (int i = 0; i < inData.Rows.Count; i++)
                {

                    dtRqst.Rows[0]["LBCD"] = "LBL0013";
                    dtRqst.Rows[0]["PRMK"] = "Z";
                    dtRqst.Rows[0]["RESO"] = "203";
                    dtRqst.Rows[0]["PRCN"] = "1";
                    dtRqst.Rows[0]["MARH"] = startX;
                    dtRqst.Rows[0]["MARV"] = startY;
                    dtRqst.Rows[0]["ATTVAL001"] = inData.Rows[i]["MODELID"].ToString();
                    dtRqst.Rows[0]["ATTVAL002"] = inData.Rows[i]["LOTID"].ToString();
                    dtRqst.Rows[0]["ATTVAL003"] = inData.Rows[i]["WIPQTY"].ToString();
                    dtRqst.Rows[0]["ATTVAL004"] = inData.Rows[i]["RESNNAME"].ToString();
                    dtRqst.Rows[0]["ATTVAL005"] = DateTime.Now.ToString("yyyy.MM.dd");
                    dtRqst.Rows[0]["ATTVAL006"] = dtExpected.SelectedDateTime.ToString("yyyy.MM.dd"); 
                    dtRqst.Rows[0]["ATTVAL007"] = inData.Rows[i]["PERSON"].ToString();
                    dtRqst.Rows[0]["ATTVAL008"] = "";
                    dtRqst.Rows[0]["ATTVAL009"] = "";

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);



                    try
                    {
                        //CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(dtRslt.Rows[0]["LABELCD"].ToString());
                        //wndPopup.Show();

                        // 프린터 정보 조회
                        string sPrt = string.Empty;
                        string sRes = string.Empty;
                        string sCopy = string.Empty;
                        string sXpos = string.Empty;
                        string sYpos = string.Empty;
                        string sDark = string.Empty;
                        DataRow drPrtInfo = null;
                        
                        if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                        return;

                        if (PrintLabel(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo) == false)
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라벨 발행중 문제가 발생하였습니다. IT 담당자에게 문의하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            Util.MessageValidation("SFU1309"); //Barcode Print 실패.
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private bool PrintLabel(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo == null || drPrtInfo.Table == null)
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030")); //프린터 환경설정 정보가 없습니다.

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                //if (drPrtInfo["PORTNAME"].ToString().IndexOf("USB") >= 0)
                if (drPrtInfo["PORTNAME"].ToString().ToUpper().Equals("USB"))
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().ToUpper().IndexOf("COM") >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    Util.MessageValidation("SFU3031"); //프린터 환경설정에 포트명 항목이 없습니다.
                }
            }
            else
            {
                loadingIndicator.Visibility = System.Windows.Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031")); // 프린터 환경설정에 포트명 항목이 없습니다.

                Util.MessageValidation("SFU3031"); // 프린터 환경설정에 포트명 항목이 없습니다.
            }

            return brtndefault;

        }


        #endregion

        private string SearhCarrierId(string carrierId)
        {
            string sLotID = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(carrierId))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return sLotID;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = carrierId.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    //재공 정보가 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1870"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                        }
                    });
                    return sLotID;
                }
                else
                {
                    sLotID = dtLot.Rows[0]["LOTID"].ToString();
                }

                return sLotID;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private string SearhCarrierIdHistory(string carrierId)
        {
            string lotId = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(carrierId))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return lotId;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = carrierId.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
                    return lotId;
                }
                else
                {
                    lotId = dtLot.Rows[0]["LOTID"].ToString();
                }

                return lotId;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }


        #endregion



    }
}
