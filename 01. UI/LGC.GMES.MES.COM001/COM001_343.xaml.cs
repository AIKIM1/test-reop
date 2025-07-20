/*************************************************************************************
 Created Date : 2020.12.18
      Creator : 조영대
   Decription : 공상평 ECM 전지 GMES 구축 DRB - LOT HOLD
--------------------------------------------------------------------------------------
 [Change History]
  2020.12.18  조영대 : Initial Created.
  2023.09.13  정재홍 : [E20230508-000004] - [ESGM] 전극,조립간 인수인계 완료된 롤정보 실적 수정 및 홀드 불가
  2024.09.03  이샛별 : [E20240731-000866] - 유럽식 날짜 입력에 대해 Data Conversion Error 발생하여 Util.GetCondition() 활용
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
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
    public partial class COM001_343 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util util = new Util();

        string ValueToLot = string.Empty;
        DateTime dCalDate;

        List<string> LotList = new List<string>();
        List<string> CSTList = new List<string>();

        public COM001_343()
        {
            InitializeComponent();
            InitCombo();

            // 전극만 해당 조건 보이게 설정 [2018-01-24]
            if (string.Equals(GetAreaType(), "E"))
            {
                txtElecType.Visibility = Visibility.Visible;
                cboElecType.Visibility = Visibility.Visible;
                dgListHold.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
                dgSelectHold.Columns["PROD_VER_CODE"].Visibility = Visibility.Visible;
            }

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preListHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preSelectHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAllListHold = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0)
        };

        CheckBox chkAllSelectHold = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            Margin = new Thickness(6, 0, 0, 0)
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
            CommonCombo _combo = new CommonCombo();
            
            //동
            C1ComboBox[] cboAreaChildHold = { cboEquipmentSegmentHold, cboHoldType };
            _combo.SetCombo(cboAreaHold, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHold, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHold = { cboAreaHold };
            C1ComboBox[] cboEquipmentSegmentChildHold = { cboProcessHold };
            _combo.SetCombo(cboEquipmentSegmentHold, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildHold, cbParent: cboEquipmentSegmentParentHold, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHold = { cboEquipmentSegmentHold };
            _combo.SetCombo(cboProcessHold, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHold, sCase: "PROCESS_PCSGID");

            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcessHistory};
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL,  cbParent: cboProcessParentHistory, sCase: "PROCESS");

            //HOLD 사유
            C1ComboBox[] cboHoldTypeParentHold = { cboAreaHold };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, cbParent: cboHoldTypeParentHold, sCase: "CBO_AREA_ACTIVITIREASON", sFilter: new string[] { "HOLD_LOT" });

            // 극성
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);

            dCalDate = GetComSelCalDate();
            dtCalDate.SelectedDateTime = dCalDate;

            // Selection Hold UI 활성화 여부 [2019-09-03]
            if (IsCommonCode("HOLD_SELECT_EXCEPT_AREA", LoginInfo.CFG_AREA_ID) == true)
            {
                HoldSelection.Visibility = Visibility.Collapsed;
            }

            this.Loaded -= UserControl_Loaded;
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

        private void btnSearchHold_Click(object sender, RoutedEventArgs e)
        {
            SearchLotList();
        }


        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetHoldHistory();
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (dgSelectHold.Rows.Count < 2)
                {
                    Util.MessageValidation("SFU1261");
                    return;
                }
                                
                if (cboHoldType.SelectedIndex < 1)
                {
                    // %1 (을)를 입력해 주세요.
                    Util.MessageValidation("SFU8275", lblHoldType.Text);
                    return;
                }
                
                if (Util.IsNVC(txtPerson.Text))
                {
                    // %1 (을)를 입력해 주세요.
                    Util.MessageValidation("SFU8275", lblPerson.Text);
                    return;
                }

                //HOLD 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
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

        private void txtLotHold_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    if (txtLotHold.Text.Trim() == string.Empty)
                        return;

                    string sLotid = txtLotHold.Text.Trim();
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

        private void txtLotHold_PreviewKeyDown(object sender, KeyEventArgs e)
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

                    string _ValueToFind = string.Empty;

                    LotList.Clear();

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList(sPasteStrings[i], false, false) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        _ValueToFind = string.Join(",", LotList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
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
                    txtLotHold.Text = "";
                    txtLotHold.Focus();
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

        #region [Check ALL]
        private void dgListHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preListHold.Content = chkAllListHold;
                        e.Column.HeaderPresenter.Content = preListHold;                        
                        chkAllListHold.Checked -= new RoutedEventHandler(chkAllListHold_Checked);
                        chkAllListHold.Unchecked -= new RoutedEventHandler(chkAllListHold_Unchecked);
                        chkAllListHold.Checked += new RoutedEventHandler(chkAllListHold_Checked);
                        chkAllListHold.Unchecked += new RoutedEventHandler(chkAllListHold_Unchecked);
                    }
                }
            }));
        }
        
        private void chkAllListHold_Checked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }

        private void chkAllListHold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgListHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgListHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }

        private void dgSelectHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        preSelectHold.Content = chkAllSelectHold;
                        e.Column.HeaderPresenter.Content = preSelectHold;
                        chkAllSelectHold.Checked -= new RoutedEventHandler(chkAllSelectHold_Checked);
                        chkAllSelectHold.Unchecked -= new RoutedEventHandler(chkAllSelectHold_Unchecked);
                        chkAllSelectHold.Checked += new RoutedEventHandler(chkAllSelectHold_Checked);
                        chkAllSelectHold.Unchecked += new RoutedEventHandler(chkAllSelectHold_Unchecked);
                    }
                }
            }));
        }

        private void chkAllSelectHold_Checked(object sender, RoutedEventArgs e)
        {
            if (dgSelectHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgSelectHold.ItemsSource).Table;

            dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            dt.AcceptChanges();
        }

        private void chkAllSelectHold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgSelectHold.ItemsSource == null) return;

            DataTable dt = ((DataView)dgSelectHold.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = 0);
            dt.AcceptChanges();
        }
        #endregion

        #region [대상 선택하기]
        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            if (util.GetDataGridCheckCnt(dgListHold, "CHK") > 0)
            {
                util.GetDataGridCheckRowIndex(dgListHold, "CHK").OrderByDescending(r => r).ToList().ForEach(r => MoveRow(r, true));
            }
            else
            {
                if (dgListHold.CurrentRow == null || dgListHold.CurrentRow.Index < 0 ||
                    dgListHold.SelectedIndex < 0 || dgListHold.CurrentRow.DataItem == null)
                {
                    //선택된 LOT이 없습니다
                    Util.Alert("9151");
                    return;
                }

                MoveRow(dgListHold.CurrentRow.Index, true);
            }
            chkAllListHold.IsChecked = false;
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (util.GetDataGridCheckCnt(dgSelectHold, "CHK") > 0)
            {
                util.GetDataGridCheckRowIndex(dgSelectHold, "CHK").OrderByDescending(r => r).ToList().ForEach(r => MoveRow(r, false));
            }
            else
            {
                if (dgSelectHold.CurrentRow == null || dgSelectHold.CurrentRow.Index < 0 ||
                    dgSelectHold.SelectedIndex < 0 || dgSelectHold.CurrentRow.DataItem == null)
                {
                    //선택된 LOT이 없습니다
                    Util.Alert("9151");
                    return;
                }

                MoveRow(dgSelectHold.CurrentRow.Index, false);
            }
            chkAllSelectHold.IsChecked = false;
        }

        private void MoveRow(int row, bool isRight)
        {
            C1.WPF.DataGrid.C1DataGrid gridSource = null;
            C1.WPF.DataGrid.C1DataGrid gridTarget = null;

            if (isRight)
            {
                gridSource = dgListHold;
                gridTarget = dgSelectHold;
            }
            else
            {
                gridSource = dgSelectHold;
                gridTarget = dgListHold;
            }

            DataTable dtSourceHold = DataTableConverter.Convert(gridSource.ItemsSource);
            DataTable dtTargetHold = DataTableConverter.Convert(gridTarget.ItemsSource);

            object[] array = dtSourceHold.Rows[row].ItemArray;
            array[0] = 0;
            dtTargetHold.Rows.Add(array);
            gridTarget.ItemsSource = DataTableConverter.Convert(dtTargetHold);

            dtSourceHold.Rows.RemoveAt(row);
            gridSource.ItemsSource = DataTableConverter.Convert(dtSourceHold);
            
        }


        #endregion

        #region [담당자]
        private void txtPerson_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    if (txtPerson.Text.Trim() == string.Empty)
                        return;

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
                    Util.MessageException(ex); Util.MessageException(ex);
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

        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCSTID.Text.Trim() == string.Empty)
                        return;

                    string sCSTID = txtCSTID.Text.Trim();
                    if (dgListHold.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "CSTID").ToString() == sCSTID)
                            {
                                Util.Alert("SFU1504");   //동일한 LOT이 스캔되었습니다.
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

        private void txtCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
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

                    string _ValueToFind = string.Empty;

                    CSTList.Clear();

                    if (sPasteStrings.Length > 1)
                    {
                        for (int i = 0; i < sPasteStrings.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sPasteStrings[i]) && GetLotList("", false, false, sPasteStrings[i]) == false)
                                break;

                            System.Windows.Forms.Application.DoEvents();
                        }

                        _ValueToFind = string.Join(",", CSTList);

                        if (_ValueToFind != "")
                        {
                            Util.MessageValidation("SFU4306", _ValueToFind);  // 입력한 LOTID[%1] 정보를 확인하십시오.
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
                    txtCSTID.Text = "";
                    txtCSTID.Focus();
                }
            }
        }


        private void dgListHold_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgListHold.CurrentRow != null)
            {
                dgListHold.SetValue(dgListHold.CurrentRow.Index, "CHK", true);
            }
        }

        private void dgSelectHold_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgSelectHold.CurrentRow != null)
            {
                dgSelectHold.SetValue(dgSelectHold.CurrentRow.Index, "CHK", true);
            }
        }

        private void dgHoldGroup1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgHoldGroup1.CurrentRow != null)
            {
                dgHoldGroup1.SetValue(dgHoldGroup1.CurrentRow.Index, "CHK", true);
            }
        }

        private void dgHoldGroup2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgHoldGroup2.CurrentRow != null)
            {
                dgHoldGroup2.SetValue(dgHoldGroup2.CurrentRow.Index, "CHK", true);
            }
        }

        private void dgPersonSelect_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgPersonSelect.CurrentRow != null)
            {
                dgPersonSelect.SetValue(dgPersonSelect.CurrentRow.Index, "CHK", true);
            }
        }
        #endregion

        #region Mehod
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>
            {
                btnHold
            };

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void SearchLotList()
        {
            if (!Util.IsNVC(txtLotHold.Text) && txtLotHold.Text.Length < 4)
            {
                //[%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");

                txtLotHold.Focus();
                txtLotHold.SelectAll();
                return;
            }

            if (!Util.IsNVC(txtCSTID.Text) && txtCSTID.Text.Length < 4)
            {
                //[%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");

                txtCSTID.Focus();
                txtCSTID.SelectAll();
                return;
            }

            if (!Util.IsNVC(txtModlId.Text) && txtModlId.Text.Length < 4)
            {
                //[%1] 자리수 이상 입력하세요.
                Util.MessageValidation("SFU4342", "3");

                txtModlId.Focus();
                txtModlId.SelectAll();
                return;
            }

            if (!string.Equals(txtLotHold.Text, ""))
                GetLotList(txtLotHold.Text.Trim(), true, true);
            else
                GetLotList("", true, true, txtCSTID.Text);
        }

        #region [작업대상 가져오기]
        public bool GetLotList( string sLotId, bool bButton, bool vOnlyOne, string sCSTID = "")
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

                if (sLotId.Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHold, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return false;

                    dr["PROCID"] = Util.GetCondition(cboProcessHold, bAllNull: true);

                    dr["MODLID"] = txtModlId.Text;
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                    // 극성
                    if (cboElecType.Visibility == Visibility.Visible && cboElecType.SelectedIndex > 0)
                        dr["ELEC_TYPE"] = Util.GetCondition(cboElecType, bAllNull: true);

                    //유럽식 날짜 입력 시 오류가 발생하는 구문 주석처리 후, Util.GenCondition() 활용 (2024-09-03)
                    //dr["CALDATE_FROM"] = ldpCaldateFrom.SelectedDateTime.ToShortDateString(); 
                    //dr["CALDATE_TO"] = ldpCaldateTo.SelectedDateTime.ToShortDateString();

                    dr["CALDATE_FROM"] = Util.GetCondition(ldpCaldateFrom);
                    dr["CALDATE_TO"] = Util.GetCondition(ldpCaldateTo);

                    if (!string.Equals(sCSTID, "")) dr["CSTID"] = sCSTID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = sLotId;
                }


                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_FIX_WIPSTAT", "INDATA", "OUTDATA", dtRqst);

                if (vOnlyOne)
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.AlertInfo("SFU1905"); //"조회된 Data가 없습니다."
                        return false; 
                    }
                    else if (dtRslt.Rows[0]["WIPHOLD"].ToString() == "Y")
                    {
                        Util.AlertInfo("SFU1340"); //"HOLD 된 LOT ID 입니다."
                        return false;
                    }
                }
                else
                {
                    if (dtRslt.Rows.Count == 0)
                    {
                        LotList.Add(sLotId);
                        return true;
                    }
                    else if (dtRqst.Rows[0]["WIPHOLD"].ToString() == "Y")
                    {
                        LotList.Add(sLotId);
                        return true;
                    }
                }
                chkAllListHold.Unchecked -= new RoutedEventHandler(chkAllListHold_Unchecked);
                chkAllListHold.IsChecked = false;
                chkAllListHold.Unchecked += new RoutedEventHandler(chkAllListHold_Unchecked);

                if (dgListHold.GetRowCount() > 0 && bButton == false)
                {
                    if (dgListHold.GetRowCount() == 0)
                    {
                        dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);                        
                        dgSelectHold.ItemsSource = DataTableConverter.Convert(dtRslt.Clone());
                    }
                    else
                    {
                        for (int i = 0; i < dgListHold.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgListHold.Rows[i].DataItem, "LOTID").ToString() == dtRslt.Rows[0]["LOTID"].ToString())
                            {
                                LotList.Add(sLotId);
                                return true;
                            }
                        }
                        DataTable dtSource = DataTableConverter.Convert(dgListHold.ItemsSource);
                        dtSource.Merge(dtRslt);
                        
                        dgListHold.SetItemsSource(dtSource, FrameOperation, true);
                        dgSelectHold.SetItemsSource(dtSource.Clone(), FrameOperation, true);
                    }
                }
                else
                {
                    dgListHold.SetItemsSource(dtRslt, FrameOperation, true);
                    dgSelectHold.SetItemsSource(dtRslt.Clone(), FrameOperation, true);
                    dgSelectHold.ClearRows();
                }

                if (!bButton)
                    chkAllListHold.IsChecked = true;

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        public DataTable UnionDataTable(DataTable dt1, DataTable dt2 )
        {
            DataTable dtResult = new DataTable();
            dtResult = dt1;

            dtResult.PrimaryKey = new DataColumn[] { dtResult.Columns["LOTID"] };
            dtResult.Merge(dt2, true);
            
            return dtResult;
        }
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private string GetAreaType()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREATYPE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return Util.NVC(dtResult.Rows[0]["AREA_TYPE_CODE"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            return "";
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

                if (!string.IsNullOrEmpty(txtLotHistory.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotHistory);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                    dr["CSTID"] = string.IsNullOrWhiteSpace(txtCSTHistory.Text) ? null : txtCSTHistory.Text;
                }
                
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTHOLDHISTORY_DRB", "INDATA", "OUTDATA", dtRqst);                
                dgListHistory.SetItemsSource(dtRslt, FrameOperation);
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

            try
            {
                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACTION_USERID", typeof(string));
                inDataTable.Columns.Add("CALDATE", typeof(DateTime)); // 2024.10.17. 김영국 - Indata 형식 변경. string -> DateTime

                DataRow row = null;

                row = inDataTable.NewRow();
                row["SRCTYPE"] = "UI";
                row["LANGID"] = LoginInfo.LANGID;
                row["IFMODE"] = "OFF";
                row["USERID"] = LoginInfo.USERID;
                row["ACTION_USERID"] = Util.GetCondition(txtPersonId);
                row["CALDATE"] = dtCalDate.SelectedDateTime.ToString("yyyy-MM-dd");

                inDataTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOLD_NOTE", typeof(string));
                inLot.Columns.Add("RESNCODE", typeof(string));
                inLot.Columns.Add("HOLD_CODE", typeof(string));
                inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));

                //라벨 발행용
                DataRow row1 = null;
                DataTable dtLabel = new DataTable();
                dtLabel.Columns.Add("LOTID", typeof(string));
                dtLabel.Columns.Add("RESNNAME", typeof(string));
                dtLabel.Columns.Add("MODELID", typeof(string));
                dtLabel.Columns.Add("WIPQTY", typeof(string));
                dtLabel.Columns.Add("PERSON", typeof(string));

                //Shop 이동 Validaiont 확인
                DataRow drMove = null;
                DataTable dtMove = new DataTable();
                dtMove.Columns.Add("LOTID", typeof(String));
                
                for (int i = 0; i < dgSelectHold.Rows.Count - 1; i++)
                {
                    if (!dgSelectHold.Rows[i].Type.Equals(C1.WPF.DataGrid.DataGridRowType.Item)) continue;

                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    row["HOLD_NOTE"] = Util.GetCondition(txtHold);
                    row["RESNCODE"] = Util.GetCondition(cboHoldType);
                    row["HOLD_CODE"] = Util.GetCondition(cboHoldType);
                    row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);


                    inLot.Rows.Add(row);

                    row1 = dtLabel.NewRow();
                    row1["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    row1["RESNNAME"] = cboHoldType.Text;
                    row1["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "MODELID"));
                    row1["WIPQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "WIPQTY"))).ToString("###,###,##0.##");
                    row1["PERSON"] = Util.GetCondition(txtPerson);
                    dtLabel.Rows.Add(row1);

                    drMove = dtMove.NewRow();
                    drMove["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    dtMove.Rows.Add(dtMove);
                }

                // [E20230508-000004] 
                if (string.Equals(GetAreaType(), "E"))
                {
                    DataTable dtDalv = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_SHOP_MOVE_LOT_VALD", "RQSTDT", "RSLTDT", inLot);

                    if (dtDalv.Rows[0]["RSLT_MSG"].ToString() == "NG")
                    {
                        Util.MessageValidation("SFU8913", Util.NVC(dtDalv.Rows[0]["LOTID"].ToString())); // 이동중인 Lot [1%]입니다. 확인 후 진행하세요
                        return;
                    }
                }

                //hold 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_HOLD_LOT", "INDATA,INLOT", null, inData);

                //담당자에게 메일 보내기
                //MailSend mail = new CMM001.Class.MailSend();
                //mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, txtPersonId.Text, "", "HOLD 처리", "HOLD 내용");

                //라벨발행
                if (chkPrint.IsChecked.Equals(true))
                {
                    PrintHoldLabel(dtLabel);
                }

                Util.AlertInfo("SFU1344");  //HOLD 완료
                Util.gridClear(dgListHold);
                Util.gridClear(dgSelectHold);

                chkAllListHold.Unchecked -= new RoutedEventHandler(chkAllListHold_Unchecked);
                chkAllListHold.IsChecked = false;
                chkAllListHold.Unchecked += new RoutedEventHandler(chkAllListHold_Unchecked);

                txtHold.Text = "";
                txtPerson.Text = "";
                txtPersonId.Text = "";
                txtPersonDept.Text = "";

                if (chkHoldLevel.IsChecked.Equals(true))
                {
                    chkHoldLevel.IsChecked = false;
                }
                cboHoldType.SelectedIndex = 0;

                SearchLotList();
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_HOLD_LOT", ex.Message, ex.ToString());

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
                        
                        if (!util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                        return;

                        if (PrintLabel(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo) == false)
                        {
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

        #endregion


        // 동별 Level Hold 코드 추가로 하단에 Method
        #region CWA Level Hold 코드 선택
        private void chkHoldLevel_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (cboHoldType.Items.Count > 1)
                cboHoldType.SelectedIndex = 0;

            if (checkBox.IsChecked == true)
            {
                GetAreaDefectCode();
                cboHoldType.IsEnabled = false;
            }
            else
            {
                Util.gridClear(dgHoldGroup1);
                Util.gridClear(dgHoldGroup2);
                cboHoldType.IsEnabled = true;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.DataContext == null)
                return;

            if ((bool)radioButton.IsChecked)
            {
                C1.WPF.DataGrid.DataGridCellPresenter cellPresenter = (C1.WPF.DataGrid.DataGridCellPresenter)radioButton.Parent;
                if (cellPresenter != null)
                {
                    C1.WPF.DataGrid.C1DataGrid dataGrid = cellPresenter.DataGrid;
                    int rowIdx = cellPresenter.Row.Index;

                    if (string.Equals(radioButton.GroupName, "radHoldGroup1"))
                    {
                        if (cboHoldType.Items.Count > 1)
                            cboHoldType.SelectedIndex = 0;

                        GetAreaDefectDetailCode(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "DFCT_CODE")));
                    }
                    else if (string.Equals(radioButton.GroupName, "radHoldGroup2"))
                    {
                        cboHoldType.SelectedValue = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[rowIdx].DataItem, "RESNCODE"));
                    }
                }
            }
        }

        private void GetAreaDefectCode()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = Util.NVC(cboAreaHold.SelectedValue);
                dataRow["ACTID"] = "HOLD_LOT";

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_CODE", "INDATA", "RSLTDT", dt);

                dgHoldGroup1.SetItemsSource(result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetAreaDefectDetailCode(string sDefectCode)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("ACTID", typeof(string));
                dt.Columns.Add("DFCT_CODE", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["AREAID"] = Util.NVC(cboAreaHold.SelectedValue);
                dataRow["ACTID"] = "HOLD_LOT";
                dataRow["DFCT_CODE"] = sDefectCode;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_HOLD_DFCT_DETL_CODE", "INDATA", "RSLTDT", dt);

                dgHoldGroup2.SetItemsSource(result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private bool IsCommonCode(string sCodeType, string sCodeName)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sCodeType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                foreach (DataRow row in dtResult.Rows)
                    if (Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { Util.MessageException(ex); }

            return false;
        }





        #endregion

 
    }
}
