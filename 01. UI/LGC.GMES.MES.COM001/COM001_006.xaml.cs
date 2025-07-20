/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : Lot Hold
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2021.01.22 cnskmaru  C20210104-000732 Hold 전기일 항목 이력확인 가능기능 요청 (이력 탭에 HOLD_CALDATE 컬럼 추가함)
  2024.09.03  이샛별 : [E20240731-000866] - 유럽식 날짜 입력에 대해 Data Conversion Error 발생하여 Util.GetCondition() 활용
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
    public partial class COM001_006 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        string ValueToLot = string.Empty;
        DateTime dCalDate;

        List<string> LotList = new List<string>();
        List<string> CSTList = new List<string>();

        private double MAX_HOLD_CELL = 1000;
        private double CELL_DIVIDE_CNT = 100;

        public COM001_006()
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

            if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_SHOP_ID == "G184")
            {
                cTabZZSCellInterlock.Visibility = Visibility.Visible;
            }else
            {
                cTabZZSCellInterlock.Visibility = Visibility.Collapsed;
            }

            this.Loaded += UserControl_Loaded;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preCellHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        CheckBox chkAllCellHold = new CheckBox()
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


            //동
            C1ComboBox[] cboAreaChildHold = { cboEquipmentSegmentHold, cboHoldType };
            _combo.SetCombo(cboAreaHold, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHold, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHold = { cboAreaHold };
            C1ComboBox[] cboEquipmentSegmentChildHold = { cboProcessHold };
            _combo.SetCombo(cboEquipmentSegmentHold, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChildHold, cbParent: cboEquipmentSegmentParentHold, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHold = { cboEquipmentSegmentHold };
            //C1ComboBox[] cboProcessChildHold = { cboEquipmentHold };
            _combo.SetCombo(cboProcessHold, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHold, sCase: "PROCESS_PCSGID");

            //설비
            //C1ComboBox[] cboEquipmentParentHold = { cboEquipmentSegmentHold, cboProcessHold };
            //_combo.SetCombo(cboEquipmentHold, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentHold, sCase: "EQUIPMENT");

            //동
            C1ComboBox[] cboAreaChildHistory = { cboEquipmentSegmentHistory };
            _combo.SetCombo(cboAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");
            _combo.SetCombo(cboMngAreaHistory, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChildHistory, sCase: "AREA");

            //라인
            C1ComboBox[] cboEquipmentSegmentParentHistory = { cboAreaHistory };
            C1ComboBox[] cboEquipmentSegmentChildHistory = { cboProcessHistory};
            _combo.SetCombo(cboEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");
            _combo.SetCombo(cboMngEquipmentSegmentHistory, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChildHistory, cbParent: cboEquipmentSegmentParentHistory, sCase: "EQUIPMENTSEGMENT");

            //공정
            C1ComboBox[] cboProcessParentHistory = { cboEquipmentSegmentHistory };
            //C1ComboBox[] cboProcessChildHistory = { cboEquipmentHistory };
            _combo.SetCombo(cboProcessHistory, CommonCombo.ComboStatus.ALL,  cbParent: cboProcessParentHistory, sCase: "PROCESS");
            _combo.SetCombo(cboMngProcessHistory, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParentHistory, sCase: "PROCESS");

            //설비
            //C1ComboBox[] cboEquipmentParentHistory = { cboEquipmentSegmentHistory, cboProcessHistory };
            //_combo.SetCombo(cboEquipmentHistory, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParentHistory, sCase: "EQUIPMENT");

            //HOLD 사유
            C1ComboBox[] cboHoldTypeParentHold = { cboAreaHold };
            _combo.SetCombo(cboHoldType, CommonCombo.ComboStatus.SELECT, cbParent: cboHoldTypeParentHold, sCase: "CBO_AREA_ACTIVITIREASON", sFilter: new string[] { "HOLD_LOT" });

            // 극성 추가 [2018-01-24]
            String[] sFilter1 = { "ELEC_TYPE" };
            _combo.SetCombo(cboElecType, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            _combo.SetCombo(cboAreaCellHold, CommonCombo.ComboStatus.ALL, sCase: "ALLAREA");

            string[] sFilter = { "HOLD_YN" };
            _combo.SetCombo(cboHoldYNCellHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: "COMMCODE");
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

            // Selection Hold UI 활성화 여부 [2019-09-03]
            if (IsCommonCode("HOLD_SELECT_EXCEPT_AREA", LoginInfo.CFG_AREA_ID) == true)
            {
                HoldSelection.Visibility = Visibility.Collapsed;
                spltGrid.Visibility = Visibility.Collapsed;
            }

            DataTable dt = new DataTable();
            for (int i = 0; i < dgCellHold.Columns.Count; i++)
            {
                dt.Columns.Add(dgCellHold.Columns[i].Name);
            }

            Util.GridSetData(dgCellHold, dt, FrameOperation);

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
            if (!string.Equals(txtLotHold.Text, ""))
                GetLotList(txtLotHold.Text.Trim(), true, true);
            else
                GetLotList("", true, true, txtCSTID.Text);
        }
        
        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetHoldHistory();
        }

        private void btnMngSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            GetHoldHistoryManager();
        }

        private void btnMngSaveHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgMngListHistory.Rows.Count == 0)
                {
                    Util.AlertInfo("SFU2816");  //조회 결과가 없습니다.
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                DataSave();
                            }
                        });

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        #endregion

        #region Mehod

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

                //if (Util.GetCondition(txtLotHold).Equals("")) //lot id 가 없는 경우
                if (sLotId.Equals("")) //lot id 가 없는 경우
                {
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHold, "SFU1223"); // 라인을선택하세요. >> 라인을 선택 하세요.
                    if (dr["EQSGID"].Equals("")) return false;

                    //dr["PROCID"] = Util.GetCondition(cboProcessHold, "SFU3207"); //공정을선택하세요. >> 선택해주세요.
                    //if (dr["PROCID"].Equals("")) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessHold, bAllNull: true);

                    //dr["EQPTID"] = Util.GetCondition(cboEquipmentHold, bAllNull:true);
                    // dr["EQPTID"] = null;

                    dr["MODLID"] = txtModlId.Text;
                    dr["PRJT_NAME"] = txtPrjtName.Text;

                    // 극성 추가 [2018-01-24]
                    if (cboElecType.Visibility == Visibility.Visible && cboElecType.SelectedIndex > 0)
                        dr["ELEC_TYPE"] = Util.GetCondition(cboElecType, bAllNull: true);

                    //유럽식 날짜 입력 시 오류가 발생하는 구문 주석처리 후, Util.GenCondition() 활용 (2024-09-03)
                    //dr["CALDATE_FROM"] = ldpCaldateFrom.SelectedDateTime.ToShortDateString(); 
                    //dr["CALDATE_TO"] = ldpCaldateTo.SelectedDateTime.ToShortDateString();

                    dr["CALDATE_FROM"] = Util.GetCondition(ldpCaldateFrom);
                    dr["CALDATE_TO"] = Util.GetCondition(ldpCaldateTo);

                    #region CSTID
                    if (!string.Equals(sCSTID, ""))
                        dr["CSTID"] = sCSTID;
                    #endregion
                }
                else //lot id 가 있는경우 다른 조건 모두 무시
                {
                    dr["LOTID"] = sLotId; // Util.GetCondition(sLotId);
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
                //                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCARRIERID.Text.Trim()))
                {
                    string sLotid = SearhCarrierIDHistory(txtCARRIERID.Text.Trim());
                    dr["LOTID"] = sLotid;
                }
                else if (!string.IsNullOrEmpty(txtLotHistory.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtLotHistory);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull: true);
                    //if (dr["EQSGID"].Equals("") ) return;
                    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                    //if (dr["PROCID"].Equals("")) return;
                    //dr["EQPTID"] = Util.GetCondition(cboEquipmentHistory, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                    dr["CSTID"] = string.IsNullOrWhiteSpace(txtCSTHistory.Text) ? null : txtCSTHistory.Text;
                }

                //if (Util.GetCondition(txtLotHistory).Equals("")) //lot id 가 없는 경우
                //{
                //    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromHist);
                //    dr["TO_DATE"] = Util.GetCondition(ldpDateToHist);
                //    dr["EQSGID"] = Util.GetCondition(cboEquipmentSegmentHistory, bAllNull:true);
                //    //if (dr["EQSGID"].Equals("") ) return;
                //    dr["PROCID"] = Util.GetCondition(cboProcessHistory, bAllNull: true);
                //    //if (dr["PROCID"].Equals("")) return;
                //    //dr["EQPTID"] = Util.GetCondition(cboEquipmentHistory, bAllNull: true);
                //    dr["PRODID"] = string.IsNullOrWhiteSpace(txtProdID.Text) ? null : txtProdID.Text;
                //    dr["CSTID"] = string.IsNullOrWhiteSpace(txtCSTHistory.Text) ? null : txtCSTHistory.Text;
                //}
                //else //lot id 가 있는경우 다른 조건 모두 무시
                //{
                //    dr["LOTID"] = Util.GetCondition(txtLotHistory);

                //}

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHOLDHISTORY", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgListHistory);
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgListHistory, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        public void GetHoldHistoryManager()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                //                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtMngCARRIERID_01.Text.Trim()))
                {
                    string sLotid = SearhCarrierIDHistory(txtMngCARRIERID_01.Text.Trim());
                    dr["LOTID"] = sLotid;
                }
                else if (!string.IsNullOrEmpty(txtMngLotHistory.Text))
                {
                    dr["LOTID"] = Util.GetCondition(txtMngLotHistory);
                }
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpMngDateFromHist);
                    dr["TO_DATE"] = Util.GetCondition(ldpMngDateToHist);
                    dr["EQSGID"] = Util.GetCondition(cboMngEquipmentSegmentHistory, bAllNull: true);
                    //if (dr["EQSGID"].Equals("") ) return;
                    dr["PROCID"] = Util.GetCondition(cboMngProcessHistory, bAllNull: true);
                    //if (dr["PROCID"].Equals("")) return;
                    //dr["EQPTID"] = Util.GetCondition(cboEquipmentHistory, bAllNull: true);
                    dr["PRODID"] = string.IsNullOrWhiteSpace(txtMngProdID.Text) ? null : txtProdID.Text;
                    dr["CSTID"] = string.IsNullOrWhiteSpace(txtMngCSTHistory.Text) ? null : txtCSTHistory.Text;
                }
               
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPHOLDHISTORY_MANAGER", "INDATA", "OUTDATA", dtRqst);

                //Util.gridClear(dgListHistory);
                //dgListHistory.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgMngListHistory, dtRslt, FrameOperation);
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
            
            string sHoldType = Util.GetCondition(cboHoldType, "SFU1342"); //"사유를 선택하세요" >> HOLD 사유를 선택 하세요.
            if (sHoldType.Equals(""))
            {
                return;
            }

            //C20221218 - 00000[ESGM PI]HIGH NG ratio NG mark Electode Auto Hold
            String sNG_Mark_HOLD_YN = String.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                //                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAreaHold.SelectedValue); ;
                dr["COM_TYPE_CODE"] = "NND_QA_INSP_NG_MARK";
                dr["COM_CODE"] = "AUTO_HOLD_YN";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgMngListHistory, dtRslt, FrameOperation);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["ATTR1"].ToString().Length > 0)
                {
                    //if (sHoldType.Equals("PH01H103"))
                    sNG_Mark_HOLD_YN = dtRslt.Rows[0]["ATTR1"].ToString();

                }

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", ex.Message, ex.ToString());
            }

            // ROLLBACK -- JN 문제 발생 -- 다시 수정해서 올릴 것 
            //NG Mark HOLD 코드 BIZ CALL --AUTO HOLD 관련 추가 - C20221218 - 00000[ESGM PI]HIGH NG ratio NG mark Electode Auto Hold
            String sNG_Mark_HOLD_CODE = String.Empty;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("COM_TYPE_CODE", typeof(string));
                //                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("COM_CODE", typeof(string));


                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboAreaHold.SelectedValue); ;
                dr["COM_TYPE_CODE"] = "NND_QA_INSP_NG_MARK";
                dr["COM_CODE"] = "RESNCODE";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgMngListHistory, dtRslt, FrameOperation);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Rows[0]["ATTR1"].ToString().Length > 1)
                {
                    //if (sHoldType.Equals("PH01H103"))
                    sNG_Mark_HOLD_CODE = dtRslt.Rows[0]["ATTR1"].ToString();

                    //if (sHoldType.Equals(sNG_Mark_HOLD_CODE))
                    //{
                    //    Util.MessageValidation("SFU9015");   //"NG Mark HOLD" [H01H103]는 선택 불가합니다.  'V/D 검사'  공정 AUTO HOLD 입니다.
                    //    return;
                    //}
                }
                //else
                //{
                //    Util.MessageValidation("SFU9016");   //MMD - 동별 공통코드관리 - NND_QA_INSP_NG_MARK - RESNCODE 등록 바랍니다.
                //    return;
                //}

            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", ex.Message, ex.ToString());
            }

            //C20221218-00000[ESGM PI]HIGH NG ratio NG mark Electode Auto Hold
            if (sNG_Mark_HOLD_YN.Length > 0 && sNG_Mark_HOLD_CODE.Length > 0 && sNG_Mark_HOLD_YN.Equals("Y"))
            {
                //AUTO HOLD를 사용 등록하고, NG_Mark_HOLD_CODE 사용등록 했으면, NG Mark 선택불가
                if (sHoldType.Equals(sNG_Mark_HOLD_CODE))
                {
                    Util.MessageValidation("SFU9015");   //"NG Mark HOLD" [H01H103]는 선택 불가합니다.  'V/D 검사'  공정 AUTO HOLD 입니다.
                    return;
                }
            }

            string sPerson = Util.GetCondition(txtPersonId, "SFU4011"); //담당자를 입력 하세요.
            if (sPerson.Equals(""))
            {
                return;
            }

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

            inDataTable.Rows.Add(row);

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INLOT");
            inLot.Columns.Add("LOTID", typeof(string));
            inLot.Columns.Add("HOLD_NOTE", typeof(string));
            inLot.Columns.Add("RESNCODE", typeof(string));
            inLot.Columns.Add("HOLD_CODE", typeof(string));
            inLot.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
            inLot.Columns.Add("SPCL_HOLD_STCK_FLAG", typeof(string));

            //라벨 발행용
            DataRow row1 = null;
            DataTable dtLabel = new DataTable();
            dtLabel.Columns.Add("LOTID", typeof(string));
            dtLabel.Columns.Add("RESNNAME", typeof(string));
            dtLabel.Columns.Add("MODELID", typeof(string));
            dtLabel.Columns.Add("WIPQTY", typeof(string));
            dtLabel.Columns.Add("PERSON", typeof(string));


            for (int i = 0; i < dgSelectHold.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    row = inLot.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    row["HOLD_NOTE"] = Util.GetCondition(txtHold);
                    row["RESNCODE"] = sHoldType;
                    row["HOLD_CODE"] = sHoldType;
                    row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);
                    row["SPCL_HOLD_STCK_FLAG"] = (chkSpecialStock.IsChecked == true ? "Y" : "");


                    inLot.Rows.Add(row);

                    row1 = dtLabel.NewRow();
                    row1["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "LOTID"));
                    row1["RESNNAME"] = cboHoldType.Text;
                    row1["MODELID"] = Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "MODELID"));
                    row1["WIPQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgSelectHold.Rows[i].DataItem, "WIPQTY"))).ToString("###,###,##0.##");
                    row1["PERSON"] = txtPerson.Text;
                    dtLabel.Rows.Add(row1);
                }
            }

            

            try
            {
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
                Util.AlertByBiz("BR_PRD_REG_HOLD_LOT", ex.Message, ex.ToString());

            }
        }
        #endregion

        private void DataSave()
        {

            DataSet RqstDt = new DataSet();

            //마스터 정보
            DataTable RqstDataTable = RqstDt.Tables.Add("RQSTDT");
            RqstDataTable.Columns.Add("LOTID", typeof(string));
            RqstDataTable.Columns.Add("WIPSEQ", typeof(string));
            RqstDataTable.Columns.Add("HOLD_DTTM", typeof(string));
            RqstDataTable.Columns.Add("SPCL_HOLD_STCK_FLAG", typeof(string));
            RqstDataTable.Columns.Add("UPDUSER", typeof(string));
            RqstDataTable.Columns.Add("UPDDTTM", typeof(string));

            DataRow row = null;

            for (int i = 0; i < dgMngListHistory.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "CHK")).Equals("True") ||
                    Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "CHK")).Equals("1"))
                {
                    string strFlag = Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "SPCL_HOLD_STCK_FLAG"));
                    
                    row = RqstDataTable.NewRow();
                    row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "LOTID"));
                    row["WIPSEQ"] = Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "WIPSEQ"));
                    row["HOLD_DTTM"] = Util.NVC(DataTableConverter.GetValue(dgMngListHistory.Rows[i].DataItem, "HOLD_DTTM_REAL"));
                    row["SPCL_HOLD_STCK_FLAG"] = (strFlag == "True" ? "Y" : "N");
                    row["UPDUSER"] = LoginInfo.USERID;
                    row["UPDDTTM"] = System.DateTime.Now;

                    RqstDataTable.Rows.Add(row);
                }
            }
            
            if (RqstDataTable == null || RqstDataTable.Rows.Count == 0)
            {
                Util.AlertInfo("SFU1566");  //변경된 데이터가 없습니다.
                return;
            }

            try
            {
                //저장 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("DA_BAS_UPD_WIPHOLDHISTORY_MANAGER", "RQSTDT", "RSLTDT", RqstDt);
                
                Util.AlertInfo("SFU1270");  //저장되었습니다.
                GetHoldHistoryManager();    //데이터 조회
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("DA_BAS_UPD_WIPHOLDHISTORY_MANAGER", ex.Message, ex.ToString());
            }
        }

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
                        
                        if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
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
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        #endregion

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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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

        private void txtCARRIERID_01_KeyDown(object sender, KeyEventArgs e)
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
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        // 동별 Level Hold 코드 추가로 하단에 Method 추가 [2019-07-11]
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

        private void btnSearchCellHold_Click(object sender, RoutedEventArgs e)
        {
            SearchCellHold();
        }

        private void SearchCellHold()
        {
            try
            {

                TimeSpan timeSpan = dtpDateToCellHold.SelectedDateTime.Date - dtpDateFromCellHold.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    Util.MessageValidation("SFU3569");  //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    return;
                }

                if (timeSpan.Days > 30)
                {
                    Util.MessageValidation("SFU4466");  //조회기간은 30일을 초과 할 수 없습니다.
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("HOLD_FLAG");
                RQSTDT.Columns.Add("FROM_HOLD_DTTM");
                RQSTDT.Columns.Add("TO_HOLD_DTTM");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("ASSY_LOTID");
                RQSTDT.Columns.Add("SUBLOTID");
                RQSTDT.Columns.Add("HOLD_TRGT_CODE");
                RQSTDT.Columns.Add("HOLD_TYPE_CODE");
                RQSTDT.Columns.Add("PRODID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (!string.IsNullOrEmpty(txtCellIDCellHold.Text))
                {
                    dr["SUBLOTID"] = string.IsNullOrEmpty(txtCellIDCellHold.Text) ? null : txtCellIDCellHold.Text;
                }
                else
                {

                    dr["FROM_HOLD_DTTM"] = dtpDateFromCellHold.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                    dr["TO_HOLD_DTTM"] = dtpDateToCellHold.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";
                    dr["AREAID"] = (string)cboAreaCellHold.SelectedValue == "" ? null : (string)cboAreaCellHold.SelectedValue;
                    dr["HOLD_TRGT_CODE"] = "SUBLOT";
                    dr["HOLD_TYPE_CODE"] = "ZZS_CELL_HOLD";
                    dr["HOLD_FLAG"] = (string)cboHoldYNCellHold.SelectedValue == "" ? null : (string)cboHoldYNCellHold.SelectedValue;
                    dr["PRODID"] = string.IsNullOrEmpty(txtProdIDCellHold.Text) ? null : txtProdIDCellHold.Text;
                    dr["ASSY_LOTID"] = string.IsNullOrEmpty(txtLotIDCellHold.Text) ? null : txtLotIDCellHold.Text;

                }

                RQSTDT.Rows.Add(dr);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_ASSY_HOLD_HIST", "INDATA", "OUTDATA", RQSTDT);

                if (!dtResult.Columns.Contains("CHK"))
                {
                    dtResult = _Util.gridCheckColumnAdd(dtResult, "CHK");
                }

                Util.GridSetData(dgCellHold, dtResult, FrameOperation, true);
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

        private void btnCellHold_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_006_HOLD_CELL puHold = new COM001_006_HOLD_CELL();
                puHold.FrameOperation = FrameOperation;

                if (puHold != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = "SUBLOT";
                    Parameters[1] = MAX_HOLD_CELL;
                    Parameters[2] = CELL_DIVIDE_CNT;
                    C1WindowExtension.SetParameters(puHold, Parameters);

                    puHold.Closed += new EventHandler(puHold_Closed);
                    grdMain.Children.Add(puHold);
                    puHold.BringToFront();
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

        private void puHold_Closed(object sender, EventArgs e)
        {
            COM001_006_HOLD_CELL window = sender as COM001_006_HOLD_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SearchCellHold();
            }
            this.grdMain.Children.Remove(window);
        }

        private void btnCellRelease_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgCellHold.ItemsSource);

                List<DataRow> drListNotY = dtInfo.Select("CHK = 'True' AND HOLD_FLAG <> 'Y'").ToList();
                if (drListNotY.Count > 0)
                {
                    Util.MessageValidation("SFU5145", new object[] { drListNotY[0]["STRT_SUBLOTID"], "HOLD해제" });    //해당 Lot[%1]은 %2 처리할 수 없습니다.
                    return;
                }


                COM001_006_RELEASE_CELL puRelease = new COM001_006_RELEASE_CELL();
                puRelease.FrameOperation = FrameOperation;

                if (puRelease != null)
                {
                    object[] Parameters = new object[4];
                    Parameters[0] = "SUBLOT";
                    Parameters[1] = MAX_HOLD_CELL;
                    Parameters[2] = CELL_DIVIDE_CNT;

                    List<DataRow> drListY = dtInfo.Select("CHK = 'True' AND HOLD_FLAG = 'Y'").ToList();
                    if (drListY != null && drListY.Count > 0)
                    {
                        Parameters[3] = drListY.CopyToDataTable();
                    }
                    else
                    {
                        Parameters[3] = null;
                    }

                    C1WindowExtension.SetParameters(puRelease, Parameters);

                    puRelease.Closed += new EventHandler(puRelease_Closed);
                    grdMain.Children.Add(puRelease);
                    puRelease.BringToFront();
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

        private void puRelease_Closed(object sender, EventArgs e)
        {
            COM001_006_RELEASE_CELL window = sender as COM001_006_RELEASE_CELL;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                SearchCellHold();
            }
            this.grdMain.Children.Remove(window);
        }

        private void dgCellHold_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            preCellHold.Content = chkAllCellHold;
                            e.Column.HeaderPresenter.Content = preCellHold;
                            chkAllCellHold.Checked -= new RoutedEventHandler(checkAllCellHold_Checked);
                            chkAllCellHold.Unchecked -= new RoutedEventHandler(checkAllCellHold_Unchecked);
                            chkAllCellHold.Checked += new RoutedEventHandler(checkAllCellHold_Checked);
                            chkAllCellHold.Unchecked += new RoutedEventHandler(checkAllCellHold_Unchecked);
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

        void checkAllCellHold_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAllCellHold.IsChecked)
            {
                for (int i = 0; i < dgCellHold.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgCellHold.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgCellHold.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgCellHold.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgCellHold.Rows[i].DataItem, "CHK", true);
                }
            }
        }

        private void checkAllCellHold_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAllCellHold.IsChecked)
            {
                for (int i = 0; i < dgCellHold.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCellHold.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;
            
            DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.DataItem, "CHK", "True");
        }
    }
}
