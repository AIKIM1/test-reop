/*************************************************************************************
 Created Date : 2020.12.08
      Creator : 
   Decription : Cell 등급 변경

--------------------------------------------------------------------------------------
 [Change History]
  2020.12.08  DEVELOPER : Initial Created.
  2023.01.07  배준호 : FCS에서 제한 없다가 GMES 최대 제한이 생겨 필요한 ESWA1동만 적용
  2023.01.31  조영대 : Validation 수정
  2023.02.14  권혜정 : 전사 최대제한 2000으로 적용
  2023.02.16  권혜정 : 전사 최대제한 3000으로 적용(ESGM 장현석 책임님 요청/CSR:E20230213-000097)
  2023.09.08  조영대 : IWorkArea 추가
  2023.10.09  김용식 : ESNJ B등급 변경 기능 추가
  2023.12.29  이정미 : Cell 변경 가능 등급 동별로 관리될 수 있도록 수정
  2024.02.20  남형희 : Cell 등급 변경 이력 조회 탭 화면 추가 (CSR : E20240219-000042)
  2024.04.24  지광현 : MessageConfirm Callback 함수에 예외 처리 없어 오류 메시지 표시되지 않아 try-catch문 추가(CSR:E20240424-000854)
  2024.06.11  조영대 : BizWF 처리중일때 등급변경 금지 처리
  2025.01.06  배준호 : 조회기간 날짜 처리(00:00:00 ~ 23:59:59)로 수정
  2025.04.01  이현승 : Catch-Up [E20240911-001061] Cell 등급 변경 시 MENUID INDATA 추가, Cell 등급 변경 이력 조회 탭 적용 UI 필터 및 적용 UI 컬럼 추가
                       선적용   [E20250227-001096] Cell변경이력 탭의 적용 UI 콤보 박스 리스트를 공통코드(COMBO_SUBLOTJUDGE_CHANGE_MENUID)에서 조회할 수 있도록 수정
**************************************************************************************/
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_034 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        int addRows;
        int iBadCnt;
        int iCurrCnt;
        Util _Util = new Util();
        bool bBGradeUseFlag = false;

        //SUBLOT 변경 가능 등급 동별로 관리 가능
        private bool bGradeChange = false; 

        public FCS001_034()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bBGradeUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "BGradeUse"); //2023.10.09 B 등급 사용 추가
            
            //SUBLOT 변경 가능 등급 동별로 관리 가능
            bGradeChange = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_034_GRADE_CHANGE");

            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter = { "COMBO_SUBLOTJUDGE_CHANGE", "Y", null, null, null, null };
            string sCase = string.Empty;

            if (bGradeChange)
            {
                IsAreaComTypeCodeUse(cboGrade,"COMBO_SUBLOTJUDGE_CHANGE", CommonCombo.ComboStatus.SELECT, sFilter);
            }

            else
            {
                sCase = "CMN_WITH_OPTION"; // 2024.01.05 위치 변경

                if (bBGradeUseFlag)
                {
                    sFilter[0] = "SUBLOTJUDGE_CHANGE";
                    sCase = "AREA_COMMON_CODE";
                };
                
                _combo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.SELECT, sCase: sCase, sFilter: sFilter);
            }

            // 2025.02.27 홍석원 : Cell 변경이력 적용 UI 콤보박스 로딩
            SetActMenuID();
        }

        #endregion

        #region [Method]
        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                // 여러건 추가 시 안되는 부분 확인
                DataTable dt = new DataTable();

                int rowCount = int.Parse(rowCnt.Text);

                if (Math.Abs(rowCount) > 0)
                {
                    /*if (LoginInfo.CFG_AREA_ID != "A5" && rowCount + dg.Rows.Count > 576)
                    {
                        // 최대 ROW수는 576입니다.
                        Util.MessageValidation("SFU4264");
                        return;
                    }
                    else if (LoginInfo.CFG_AREA_ID == "A5" && rowCount + dg.Rows.Count > 2000)
                    {
                        //GMES 최대 제한이 생겨 필요한 ESWA1동만 적용
                        //최대 ROW수는 2000입니다.
                        Util.MessageValidation("SFU4646");
                        return;
                    }*/
                    if (rowCount + dg.Rows.Count > 3000)
                    {
                        //전사 최대 제한 3000으로 변경
                        //최대 ROW수는 3000입니다.
                        Util.MessageValidation("SFU4647");
                        return;
                    }
                    else
                    {
                        addRows = rowCount;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }
                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        dt = DataTableConverter.Convert(dg.ItemsSource);
                        DataRow dr2 = dt.NewRow();
                        dt.Rows.Add(dr2);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
                else
                {
                    for (int i = 0; i < addRows; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dt.Rows.Add(dr);
                        dg.BeginEdit();
                        dg.ItemsSource = DataTableConverter.Convert(dt);
                        dg.EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel()
        {
            DataTable dtInfo = DataTableConverter.Convert(dgCellList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";


            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("SUBLOTID", typeof(string));
                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return;

                        string CELL_ID = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        DataRow dataRow = dataTable.NewRow();
                        dataRow["SUBLOTID"] = CELL_ID;
                        dataTable.Rows.Add(dataRow);
                    }

                    if (dataTable.Rows.Count > 0)
                        dataTable = dataTable.DefaultView.ToTable(true);

                    Util.GridSetData(dgCellList, dataTable, FrameOperation);
                }
            }
        }

        private void GetDataFromGrid(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dgCellList.Columns["Delete"].Visibility = Visibility.Visible;

            string sCellID = null;
            for (int iRow = 0; iRow < dgCellList.Rows.Count; iRow++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID")) == string.Empty)
                    continue;

                sCellID += DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID") + ",";
            }

            DataTable dtRqst = new DataTable();
            dtRqst.TableName = "RQSTDT";
            dtRqst.Columns.Add("SUBLOTID", typeof(string));

            DataRow dr = dtRqst.NewRow();
            dr["SUBLOTID"] = sCellID;
            dtRqst.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_CELL_INFO_RJUDG", "RQSTDT", "RSLTDT", dtRqst);
            Util.GridSetData(dgCellList, dtRslt, this.FrameOperation, false);

            iBadCnt = 0;
            dgCellList.Columns["SUBLOTID"].IsReadOnly = true;
            iCurrCnt = dgCellList.Rows.Count;
            txtCellCnt.Text = iCurrCnt.ToString();
        }

        //2024.02.20  남형희 : Cell 등급 변경 이력 조회 탭 화면 추가
        private void GetDataFromGrid2()
        {
            try
            {
                Util.gridClear(dgCellListHist);

                string sCellID = null;
                string sPkgID = null;
                string sFromDate = null;
                string sToDate = null;
                string sBizRuleName = null;

                sCellID = txtCellId.Text;
                sPkgID = txtLotId.Text;
                sFromDate = dtpFromDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 00:00:00";
                sToDate = dtpToDate.SelectedDateTime.ToString("yyyy-MM-dd") + " 23:59:59";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                DataRow dr = null;

                if (sCellID.Length > 0 || sPkgID.Length > 0)
                {
                    dtRqst.Columns.Add("SUBLOTID", typeof(string));
                    dtRqst.Columns.Add("PKGLOTID", typeof(string));

                    dr = dtRqst.NewRow();

                    sBizRuleName = "DA_SEL_CELL_INFO_RJUDG_CELL_HISTORY";

                    dr["SUBLOTID"] = string.IsNullOrEmpty(sCellID) ? null : sCellID;
                    dr["PKGLOTID"] = string.IsNullOrEmpty(sPkgID) ? null : sPkgID;
                }
                else
                {
                    if (dtpFromDate.SelectedDateTime.AddDays(7) < dtpToDate.SelectedDateTime)
                    {
                        //조회기간은 7일을 초과할 수 없습니다.
                        Util.MessageValidation("FM_ME_0231");
                        return;
                    }

                    dtRqst.Columns.Add("FROMDATE", typeof(string));
                    dtRqst.Columns.Add("TODATE", typeof(string));
                    dtRqst.Columns.Add("ACT_PROGRAM_ID", typeof(string));

                    dr = dtRqst.NewRow();

                    sBizRuleName = "DA_SEL_CELL_INFO_RJUDG_DATE_HISTORY";

                    object oActMenu = cboActMenu.GetBindValue();
                    string sActMenu = cboActMenu.SelectedItemsToString; // 2024.11.01 이현승 : Cell 등급 변경 이력 조회 조건 추가

                    if ((oActMenu == null && sActMenu != "") || (sActMenu == "" || sActMenu == "SELECT"))
                    {
                        sActMenu = null;
                    }

                    dr["FROMDATE"] = sFromDate;
                    dr["TODATE"] = sToDate;
                    dr["ACT_PROGRAM_ID"] = sActMenu;
                }

                dtRqst.Columns.Add("LANGID", typeof(string));
                dr["LANGID"] = LoginInfo.LANGID;

                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService(sBizRuleName, "RQSTDT", "RSLTDT", dtRqst, (result, Exception) =>
                {
                    try
                    {
                        if (Exception != null)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgCellListHist, result, FrameOperation, false);
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
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetBadCellInfo()
        {
            if (iBadCnt > 0)
            {
                btnGradeChange.IsEnabled = false;
                Util.MessageInfo("FM_ME_0218");  //정보가 없는 Cell ID가 있습니다. 확인 후 다시 시도해주세요.
            }
            else
            {
                dgCellList.Columns["Delete"].Visibility = Visibility.Collapsed;
            }
        }

        public void IsAreaComTypeCodeUse(C1.WPF.C1ComboBox cbo, string sComeCodeType, CommonCombo.ComboStatus cs, string[] sFilter)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("ATTR1", typeof(string));
                RQSTDT.Columns.Add("ATTR2", typeof(string));
                RQSTDT.Columns.Add("ATTR3", typeof(string));
                RQSTDT.Columns.Add("ATTR4", typeof(string));
                RQSTDT.Columns.Add("ATTR5", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sComeCodeType;
                if (sFilter.Length > 1)
                {
                    dr["ATTR1"] = (string.IsNullOrEmpty(sFilter[1])) ? null : sFilter[1];
                    dr["ATTR2"] = (string.IsNullOrEmpty(sFilter[2])) ? null : sFilter[2];
                    dr["ATTR3"] = (string.IsNullOrEmpty(sFilter[3])) ? null : sFilter[3];
                    dr["ATTR4"] = (string.IsNullOrEmpty(sFilter[4])) ? null : sFilter[4];
                    dr["ATTR5"] = (string.IsNullOrEmpty(sFilter[5])) ? null : sFilter[5];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_COM_CODE_CBO_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, cs, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                if (cbo.Items.Count > 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        // 2024.11.01 이현승 : Cell 등급 변경 이력 조회 조건 추가 
        private void SetActMenuID()
        {
            try
            {
                // 2025.02.27 홍석원 : 적용 UI 콤보박스 조회 시 공통코드 조회로 변경 하여 불필요한 Input Param 삭제

                cboActMenu.ItemsSource = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_ACT_MENUID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboActMenu.DisplayMemberPath = "CBO_NAME";
                cboActMenu.SelectedValuePath = "CBO_CODE";

                cboActMenu.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
            }
            catch (Exception ex)
            {
            }

        }

        #endregion

        #region [Event]
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgCellList);
            txtCellCnt.Clear();
            iBadCnt = 0;
            DataGridRowAdd(dgCellList);
            dgCellList.Columns["SUBLOTID"].IsReadOnly = false;
            dgCellList.Columns["Delete"].Visibility = Visibility.Collapsed;
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel();
            GetDataFromGrid(dgCellList);
        }
        private void btnGradeChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ClearValidation();

                for (int iRow = 0; iRow < dgCellList.Rows.Count; iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID")) == string.Empty) continue;

                    // BizWF 처리중일때 중단.
                    if (BizWF_Check(Util.NVC(DataTableConverter.GetValue(dgCellList.Rows[iRow].DataItem, "SUBLOTID"))) != 0) return;
                }

                if (cboGrade.GetBindValue() == null)
                {
                    //등급을 선택해주세요.
                    cboGrade.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0122"));
                    return;
                }

                // 2023.10.09 From 등급 Check
                if (bBGradeUseFlag)
                {
                    string sFromGrade = string.Empty;
                    string sToGraede = string.Empty;
                    string sChkFrGrade = string.Empty;

                    sToGraede = Util.GetCondition(cboGrade);
                    sChkFrGrade = GetChkFrGrade(cboGrade.GetBindValue().ToString());

                    if (string.IsNullOrEmpty(sChkFrGrade) == false)
                    {
                        List<string> LstFrGrade = sChkFrGrade.Split(',').ToList<string>();

                        // Check Enable Grade
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (LstFrGrade.Contains(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FINL_JUDG_CODE").ToString()) == false)
                            {
                                Util.MessageValidation("SFU2100", sToGraede, sChkFrGrade); // {%1}등급 변경은 {%2}등급만 가능합니다.
                                return;
                            }
                        }
                    }
                }

                Util.MessageConfirm("FM_ME_0020", (result) => //Cell 등급을 변경하시겠습니까?
                {
                    if (result != MessageBoxResult.OK)
                    {
                        return;
                    }
                    else
                    {
                        try // 2024.04.24 지광현 MessageConfirm Callback 함수에 예외 처리 없어 오류 메시지 표시되지 않아 예외처리 추가
                        {
                            DataTable dtRqst = new DataTable();
                            dtRqst.TableName = "RQSTDT";
                            dtRqst.Columns.Add("SUBLOTID", typeof(string));
                            dtRqst.Columns.Add("SUBLOTJUDGE", typeof(string));
                            dtRqst.Columns.Add("USERID", typeof(string));
                            dtRqst.Columns.Add("MENUID", typeof(string));

                            for (int i = 0; i < dgCellList.Rows.Count; i++)
                            {
                                DataRow dr = dtRqst.NewRow();
                                dr["SUBLOTID"] = DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID");
                                dr["SUBLOTJUDGE"] = Util.GetCondition(cboGrade);
                                dr["USERID"] = LoginInfo.USERID;
                                dr["MENUID"] = LoginInfo.CFG_MENUID; // 2025.04.01 이현승 : 적용 UI 추적성 향상을 위한 MENUID 추가

                                if (string.IsNullOrEmpty(dr["SUBLOTJUDGE"].ToString()))
                                    return;

                                dtRqst.Rows.Add(dr);
                            }

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CELL_GRADE_CHANGE", "RQSTDT", "RSLTDT", dtRqst);
                            Util.MessageInfo("FM_ME_0136"); //변경완료하였습니다.
                            GetDataFromGrid(dgCellList);
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btnDelete = sender as Button;

                if (btnDelete != null)
                {
                    DataRowView dataRow = ((FrameworkElement)sender).DataContext as DataRowView;
                    int rowidx = -1;

                    for (int i = 0; i < dgCellList.Rows.Count; i++)
                    {
                        if (DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID").Equals(dataRow.Row[0]))
                        {
                            rowidx = i;
                        }
                    }

                    DataTable dt = DataTableConverter.Convert(dgCellList.ItemsSource);
                    dt.Rows.RemoveAt(rowidx);
                    Util.GridSetData(dgCellList, dt, this.FrameOperation);

                    iBadCnt--;
                    iCurrCnt--;
                    txtCellCnt.Text = iCurrCnt.ToString();
                    if (iBadCnt == 0) dgCellList.Columns["Delete"].Visibility = Visibility.Collapsed;

                    //txtBadRow.Text = iBadCnt.ToString(); NB 불량 ROW 수량 확인 안함

                }
                if (iBadCnt == 0)
                {
                    btnGradeChange.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromGrid(dgCellList);
        }

        //2024.02.20  남형희 : Cell 등급 변경 이력 조회 탭 화면 추가
        private void btnSearch2_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromGrid2();
        }

        private void dgCellList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //삭제 버튼 Control
                if (e.Cell.Column.Name.Equals("Delete") && e.Cell.DataGrid.GetCell(e.Cell.Row.Index, 1) != null)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "PROD_LOTID")).Equals("CELL ID CHECK"))
                    {
                        iBadCnt++;
                        Button btn = e.Cell.Presenter.Content as Button;
                        dataGrid.GetCell(e.Cell.Row.Index, 6).Presenter.IsEnabled = true;
                        //if (btn != null) btn.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        dataGrid.GetCell(e.Cell.Row.Index, 6).Presenter.IsEnabled = false;
                        Button btn = e.Cell.Presenter.Content as Button;
                        // if(btn!=null) btn.Visibility = Visibility.Collapsed;
                    }
                    if (e.Cell.Row.Index == dataGrid.Rows.Count - 1)
                    {
                        SetBadCellInfo();
                    }
                }


            }));
        }

        private void dgCellList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            tb.Text = (e.Row.Index + 1 - dgCellList.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }

        // 2024.11.01 이현승 : Cell 등급 변경 이력 조회 조건 추가 
        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            SetActMenuID();
        }

        #endregion

        private string GetChkFrGrade(string sToGrade)
        {
            // 2023.10.09 김용식 동별 등급 변경 코드 사용
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "SUBLOTJUDGE_CHANGE";
                dr["COM_CODE"] = sToGrade;
                dr["USE_FLAG"] = 'Y';
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                return dtResult.Rows[0]["ATTR1"].ToString();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        
        //2024.02.20  남형희 : Cell 등급 변경 이력 조회 탭 화면 추가
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        //2024.02.20  남형희 : Cell 등급 변경 이력 조회 탭 화면 추가
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private int BizWF_Check(string SubLotID)
        {
            int RetVal = -1;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "INDATA";
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = SubLotID;
                dr["LANGID"] = LoginInfo.LANGID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SMPL_COM_CHK", "INDATA", "OUTDATA", dtRqst);

                RetVal = Convert.ToInt16(dtRslt.Rows[0]["RETVAL"]);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return RetVal;
            }

            return RetVal;
        }

    }
}
