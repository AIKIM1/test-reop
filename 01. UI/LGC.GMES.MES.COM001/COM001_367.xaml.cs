/*************************************************************************************
 Created Date : 2021.12.10
      Creator : 
   Decription : Lot 예약 홀드
--------------------------------------------------------------------------------------
 [Change History]
 2021.12.10  DEVELOPER : Initial Created.

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
    public partial class COM001_367 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        string ValueToLot = string.Empty;

        List<string> LotList = new List<string>();
        List<string> CSTList = new List<string>();

        public COM001_367()
        {
            InitializeComponent();
            InitCombo();

            this.Loaded += UserControl_Loaded;
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
        #endregion

        #region Initialize

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            #region 예약홀드
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

            //HOLD 사유
            C1ComboBox[] cboHoldTypeParentHold = { cboAreaHold };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, cbParent: cboHoldTypeParentHold, sCase: "CBO_AREA_ACTIVITIREASON", sFilter: new string[] { "HOLD_LOT" });

            // 극성 
            string[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");
            #endregion

            #region 예약홀드해제
            //동
            C1ComboBox[] cboAreaChildRelease = { cboEquipmentSegmentRelease };
            _combo.SetCombo(cboAreaRelease, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildRelease, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentRelease = { cboAreaRelease };
            //C1ComboBox[] cboEquipmentSegmentChildRelease = { cboProcessRelease};
            _combo.SetCombo(cboEquipmentSegmentRelease, CommonCombo.ComboStatus.ALL, cbChild: null, cbParent: cboEquipmentSegmentParentRelease, sCase: "EQUIPMENTSEGMENT");

            // 보류유형 
            string[] sFilter2 = { "HOLD_PRCS_TYPE_CODE" };
            _combo.SetCombo(cboHoldTypeCode, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODE");
            #endregion

            #region 예약홀드History
            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcessHistory };
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            //C1ComboBox[] cboProcessChildHistory = { cboEquipmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHistory, sCase: "PROCESS");

            cboProcessHistory.SelectedIndex = 0;

            // 보류유형 
            string[] sFilter3 = { "HOLD_PRCS_TYPE_CODE" };
            _combo.SetCombo(cboHoldTypeCode2, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");
            #endregion
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnHold);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dtpCaldateFrom.SelectedDateTime = DateTime.Now;
            dtpCaldateTo.SelectedDateTime = DateTime.Now;
            dtpDateFromRels.Text = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
            dtpDateFromRels.SelectedDateTime = DateTime.Now.AddDays(-30);
            dtpDateToRels.SelectedDateTime = DateTime.Now;
            dtpDateToHist.SelectedDateTime = DateTime.Now;
            dtpDateFromHist.SelectedDateTime = DateTime.Now;
            
            
            // Selection Hold UI 활성화 여부
            if (IsCommonCode("HOLD_SELECT_EXCEPT_AREA", LoginInfo.CFG_AREA_ID) == true)
            {
                HoldSelection.Visibility = Visibility.Collapsed;
                spltGrid.Visibility = Visibility.Collapsed;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearchHold_Click(object sender, RoutedEventArgs e)
        {
            if (!string.Equals(txtLotHold.Text, ""))
                GetLotList(txtLotHold.Text.Trim(), true, true);
            else
                GetLotList("", true, true, "");
        }
        
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetRsvHoldHistory();
        }

        private void btnSearchRelease_Click(object sender, RoutedEventArgs e)
        {
            GetRsvHoldRelease();
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //HOLD 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1345"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                LotHold();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

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
        //checked, unchecked 이벤트 사용하면 화면에서 사라지고 나타날때도
        //이벤트가 발생함 click 이벤트 잡아서 사용할것
        //WPF 그지같애
        private void chkHold_Click(object sender, RoutedEventArgs e)
        {
            dgListHold.Selection.Clear();

            CheckBox cb = sender as CheckBox;

            if (DataTableConverter.GetValue(cb.DataContext, "CHK").Equals(1))//체크되는 경우
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
                        dr[dc.ColumnName] =  Util.NVC(DataTableConverter.GetValue(cb.DataContext, dc.ColumnName));
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
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void txtPerson_GotFocus(object sender, RoutedEventArgs e)
        {
            dgPersonSelect.Visibility = Visibility.Collapsed;
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

        #region Mehod
        #region [작업대상 가져오기]
        public bool GetLotList( string sLotId, bool bButton, bool vOnlyOne, string sCSTID = "")
        {
            try
            {
                DoEvents();

                Util.gridClear(dgListHold);
                Util.gridClear(dgSelectHold);

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
                    
                    if (cboElecType.Visibility == Visibility.Visible && cboElecType.SelectedIndex > 0)
                        dr["ELEC_TYPE"] = Util.GetCondition(cboElecType, bAllNull: true);

                    dr["CALDATE_FROM"] = dtpCaldateFrom.SelectedDateTime.ToShortDateString(); 
                    dr["CALDATE_TO"] = dtpCaldateTo.SelectedDateTime.ToShortDateString();
                    
                    if (!string.Equals(sCSTID, ""))
                        dr["CSTID"] = sCSTID;
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = sLotId; 
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_RSV", "INDATA", "OUTDATA", dtRqst);

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
                                LotList.Add(sLotId);
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

        public DataTable UnionDataTable(DataTable dt1, DataTable dt2 )
        {
            //DataColumn[] dc = new DataColumn[1];
            //dc[0] = dt1.Columns["LOTID"];
            //dt1.PrimaryKey = dc;
            //dt1.AcceptChanges();
            DataTable dtResult = new DataTable();
            dtResult = dt1;
            // PrimaryKey 셋팅(Merge할때 기본키값을 설정하지 않으면 테이블 Merge시 중복된 데이터가 들어갈수 있습니다.

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
            catch (Exception ex) { }
            return "";
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

                    //HiddenLoadingIndicator();
                }
            }
        }

        private void txtCARRIERID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtCARRIERID.Text.Trim() == string.Empty)
                        return;

                    string sLotid = SearhCarrierID(txtCARRIERID.Text.Trim());

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [예약홀드 해제]
        public void GetRsvHoldRelease()
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
                dtRqst.Columns.Add("HOLD_PRCS_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCarrieridRels.Text.Trim()))
                {
                    string sLotid = SearhCarrierID(txtCarrieridRels.Text.Trim());
                    dr["LOTID"] = sLotid;
                }
                else if (!string.IsNullOrEmpty(txtLotRelease.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotRelease);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromRels);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToRels);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentRelease, bAllNull: true);
                    //dr["PROCID"] = Util.GetCondition(cboProcessRelease, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdIDRelease.Text) ? null : txtProdIDRelease.Text;
                    dr["HOLD_PRCS_TYPE_CODE"] = Util.GetCondition(cboHoldTypeCode, bAllNull: true);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_RSV_RELS", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgListHistory);
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListRelease, dtRslt, FrameOperation, true);
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotRelease_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                   GetRsvHoldRelease();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtCarrieridRels_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetRsvHoldRelease();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgListRelease.ItemsSource == null || dgListRelease.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1905");  //조회된 Data가 없습니다.
                    return;
                }

                //HOLD 해제 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4046"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LotReleaseHold();
                    }
                });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [예약홀드 History]
        public void GetRsvHoldHistory()
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
                dtRqst.Columns.Add("HOLD_PRCS_TYPE_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCARRIERID.Text.Trim()))
                {
                    string sLotid = SearhCarrierID(txtCARRIERID.Text.Trim());
                    dr["LOTID"] = sLotid;
                }
                else if (!string.IsNullOrEmpty(txtLotHistory.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotHistory);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(dtpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(dtpDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull: true);
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                    dr["HOLD_PRCS_TYPE_CODE"] = Util.GetCondition(cboHoldTypeCode2, bAllNull: true);
                }

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_RSV_HIST", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgListHistory);
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtLotHistory_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetRsvHoldHistory();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void txtCarrierHistory_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetRsvHoldHistory();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        
        #endregion

        #region [hold 처리]
        private void LotHold()
        {
            try
            {
                int iProc = 0;

                string sHoldType = Util.GetCondition(cboHoldType, "SFU1342"); //"사유를 선택하세요" >> HOLD 사유를 선택 하세요.
                if (sHoldType.Equals(""))
                {
                    return;
                }

                string sPerson = Util.GetCondition(txtPersonId, "SFU4011"); //담당자를 입력 하세요.
                if (sPerson.Equals(""))
                {
                    return;
                }

                //공정 정보
                DataTable dtProc = new DataTable();
                dtProc.Columns.Add("SEQ", typeof(string));
                dtProc.Columns.Add("PROCID", typeof(string));

                DataRow drProc = null;
                if (chkProcRoll.IsChecked == true)
                {
                    drProc = dtProc.NewRow();
                    drProc["SEQ"] = iProc + 1;
                    drProc["PROCID"] = Process.ROLL_PRESSING;
                    dtProc.Rows.Add(drProc);

                    iProc++;
                }

                if (chkProcSlitter.IsChecked == true)
                {
                    drProc = dtProc.NewRow();
                    drProc["SEQ"] = iProc + 1;
                    drProc["PROCID"] = Process.SLITTING;
                    dtProc.Rows.Add(drProc);

                    iProc++;
                }

                if (dtProc.Rows.Count == 0)
                {
                    Util.MessageValidation("SFU1459");   //공정을 선택하세요.
                    return;
                }
                ///////////////////////////////////////////////////////////////////

                //대상 LOT
                DataTable dtLot = new DataTable();
                dtLot.Columns.Add("LOTID", typeof(string));
                dtLot.Columns.Add("PROCID", typeof(string));
                dtLot.Columns.Add("HOLD_CODE", typeof(string));
                dtLot.Columns.Add("HOLD_USERID", typeof(string));
                dtLot.Columns.Add("ACTION_USERID", typeof(string));
                dtLot.Columns.Add("HOLD_NOTE", typeof(string));
                dtLot.Columns.Add("USE_FLAG", typeof(string));
                dtLot.Columns.Add("ACTID", typeof(string));
                dtLot.Columns.Add("HOLD_PRCS_TYPE_CODE", typeof(string));

                DataRow dr = dtProc.NewRow();

                for (int i = 0; i < dgSelectHold.Rows.Count; i++)
                {
                    for (int j = 0; j < dtProc.Rows.Count; j++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("True") ||
                            Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("1"))
                        {
                            dr = dtLot.NewRow();
                            dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                            dr["PROCID"] = dtProc.Rows[j]["PROCID"].ToString();
                            dr["HOLD_CODE"] = sHoldType;
                            dr["HOLD_USERID"] = Util.GetCondition(txtPersonId);
                            dr["ACTION_USERID"] = null; 
                            dr["HOLD_NOTE"] = Util.GetCondition(txtHold);
                            dr["USE_FLAG"] = "Y";
                            dr["ACTID"] = "HOLD_LOT_RSV";
                            dr["HOLD_PRCS_TYPE_CODE"] = "GMES";
                            dtLot.Rows.Add(dr);
                        }
                    }
                }

                //hold 처리
                new ClientProxy().ExecuteService("BR_PRD_REG_HOLD_LOT_RSV", "INDATA", null, dtLot, (Result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.AlertInfo("SFU1344");  //HOLD 완료
                        Util.gridClear(dgListHold);
                        Util.gridClear(dgSelectHold);

                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.IsChecked = false;
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);

                        txtHold.Text = "";
                        txtPerson.Text = "";
                        txtPersonId.Text = "";
                        txtPersonDept.Text = "";
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_HOLD_LOT_RSV", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region [hold해제 처리]
        private void LotReleaseHold()
        {
            try
            {
                bool isChk = false;

                DataTable dtRels = DataTableConverter.Convert(dgListRelease.ItemsSource);

                for (int i = 0; i < dtRels.Rows.Count; i++)
                {
                    if (dtRels.Rows[i]["CHK"].ToString() == "1" || dtRels.Rows[i]["CHK"].ToString() == "CHK")
                    {
                        if (dtRels.Rows[i]["HOLD_RSN_NOTE"].ToString() == "")
                        {
                            Util.MessageValidation("SFU4301");  //HOLD 해제사유를 입력하세요
                            return;
                        }

                        isChk = true;
                    }
                }

                if (isChk == false)
                {
                    Util.MessageValidation("SFU1636");  //선택된 대상이 없습니다.
                    return;
                }

                //공정 정보
                DataTable dtProc = new DataTable();
                dtProc.Columns.Add("PROCID", typeof(string));

                DataRow drProc = null;
                drProc = dtProc.NewRow();
                drProc["PROCID"] = Process.ROLL_PRESSING;
                dtProc.Rows.Add(drProc);
                
                drProc = dtProc.NewRow();
                drProc["PROCID"] = Process.SLITTING;
                dtProc.Rows.Add(drProc);

                //대상 LOT
                DataTable dtLot = new DataTable();
                dtLot.Columns.Add("LOTID", typeof(string));
                dtLot.Columns.Add("ACTID", typeof(string));
                dtLot.Columns.Add("PROCID", typeof(string));
                dtLot.Columns.Add("HOLD_USERID", typeof(string));
                dtLot.Columns.Add("ACTION_USERID", typeof(string));
                dtLot.Columns.Add("HOLD_RSN_NOTE", typeof(string));
                dtLot.Columns.Add("USE_FLAG", typeof(string));
                dtLot.Columns.Add("HOLD_PRCS_TYPE_CODE", typeof(string));

                DataRow dr = dtLot.NewRow();

                for (int j = 0; j < dtRels.Rows.Count; j++)
                {
                    for (int k = 0; k < dtProc.Rows.Count; k++)
                    {
                        if (dtRels.Rows[j]["CHK"].ToString() == "1" || dtRels.Rows[j]["CHK"].ToString() == "CHK")
                        {
                            dr = dtLot.NewRow();
                            dr["LOTID"] = dtRels.Rows[j]["LOTID"].ToString();
                            dr["ACTID"] = "UNHOLD_LOT_RSV";
                            dr["PROCID"] = dtProc.Rows[k]["PROCID"].ToString();
                            dr["HOLD_USERID"] = dtRels.Rows[j]["HOLD_USERID"].ToString();
                            dr["ACTION_USERID"] = LoginInfo.USERID;
                            dr["HOLD_RSN_NOTE"] = dtRels.Rows[j]["HOLD_RSN_NOTE"].ToString();
                            dr["USE_FLAG"] = "N";
                            dr["HOLD_PRCS_TYPE_CODE"] = "GMES";

                            dtLot.Rows.Add(dr);
                        }
                    }
                }
                
                //hold해제 처리
                new ClientProxy().ExecuteService("BR_PRD_REG_HOLD_LOT_RSV_MERGE", "INDATA", null, dtLot, (Result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.AlertInfo("SFU1268");  //HOLD 완료
                        GetRsvHoldRelease();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_REG_HOLD_LOT_RSV_MERGE", ex.Message, ex.ToString());
            }
        }
        #endregion

        #region [Carrier 조회]
        private string SearhCarrierID(string sCarrierID)
        {
            string sLotID = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return sLotID;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }

        /*
        private string SearhCarrierIDHistory(string sCarrierID)
        {
            string sLotID = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sCarrierID))
                {
                    Util.MessageValidation("SFU2060"); //스캔한 데이터가 없습니다.
                    return sLotID;
                }

                DataTable dt = new DataTable();
                dt.TableName = "RQSTDT";
                dt.Columns.Add("CSTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CSTID"] = sCarrierID.Trim();

                dt.Rows.Add(dr);

                DataTable dtLot = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_LOTID_FOR_RFID", "RQSTDT", "RSLTDT", dt);

                if (dtLot.Rows.Count == 0)
                {
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return null;
            }
        }
        */

        #endregion
        #endregion
        
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
                    //dataGrid.SelectedIndex = rowIdx;

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

                Util.GridSetData(dgHoldGroup1, result, FrameOperation, true);
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

                Util.GridSetData(dgHoldGroup2, result, FrameOperation, true);
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
                    if (string.Equals(sCodeName, row["CBO_CODE"]))
                        return true;
            }
            catch (Exception ex) { }

            return false;
        }
        #endregion
        private void TabHold_GotFocus(object sender, RoutedEventArgs e)
        {
            TabHold.GotFocus -= TabHold_GotFocus;
            TabRelease.GotFocus += TabRelease_GotFocus;
            TabHistory.GotFocus += TabHistory_GotFocus;
            dgClear(dgListHold, dgSelectHold);
        }

        private void TabRelease_GotFocus(object sender, RoutedEventArgs e)
        {
            TabHold.GotFocus += TabHold_GotFocus;
            TabRelease.GotFocus -= TabRelease_GotFocus;
            TabHistory.GotFocus += TabHistory_GotFocus;
            dgClear(dgListRelease, dgListRelease);
        }

        private void TabHistory_GotFocus(object sender, RoutedEventArgs e)
        {
            TabHold.GotFocus += TabHold_GotFocus;
            TabRelease.GotFocus += TabRelease_GotFocus;
            TabHistory.GotFocus -= TabHistory_GotFocus;
            dgClear(dgListHistory, dgListHistory);
        }        

        private void dgClear(C1DataGrid dg1, C1DataGrid dg2)
        {
            Util.gridClear(dg1);
            Util.gridClear(dg2);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            if (dgListRelease.Rows.Count <= 0) return;

            CheckBox cb = sender as CheckBox;
            if (cb?.DataContext == null) return;

            int idx = ((DataGridCellPresenter)cb.Parent).Row.Index;

            if (string.Equals(DataTableConverter.GetValue(dgListRelease.Rows[idx].DataItem, "HOLD_PRCS_TYPE_CODE"), "SPC"))
            {
                if (cb.IsChecked != null)
                    DataTableConverter.SetValue(dgListRelease.Rows[idx].DataItem, "CHK", false);

                Util.MessageValidation("SFU8496");
                return;
            }
        }

        private void dgListRelease_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell.Presenter == null) return;

            if (dgListRelease.Rows.Count <= 0) return;

            dgListRelease.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name != null)
                {
                    //link 색변경
                    if (e.Cell.Column.Name.Equals("LOTID") && Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "HOLD_PRCS_TYPE_CODE")).ToString() == "SPC")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }
            }));
        }

        private void dgListRelease_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string pLotid = string.Empty;

                if (dgListRelease.ItemsSource == null || dgListRelease.Rows.Count <= 0)
                    return;

                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                if (dg.CurrentColumn.Name.Equals("LOTID") && DataTableConverter.GetValue(dg.CurrentRow.DataItem, "HOLD_PRCS_TYPE_CODE").ToString() == "SPC")
                {
                    pLotid = Util.NVC(DataTableConverter.GetValue(dg.CurrentRow.DataItem, "LOTID"));

                    COM001_367_SPC_DETL SPCPopup = new COM001_367_SPC_DETL();
                    SPCPopup.FrameOperation = FrameOperation;

                    if (SPCPopup != null)
                    {
                        object[] Parameters = new object[1];
                        Parameters[0] = pLotid;
                        C1WindowExtension.SetParameters(SPCPopup, Parameters);

                        SPCPopup.ShowModal();
                    }
                }
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
    }
}
