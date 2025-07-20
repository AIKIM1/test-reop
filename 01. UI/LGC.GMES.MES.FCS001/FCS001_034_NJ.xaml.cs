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
  2023.09.25  김용식 : ESNJ 동별코드 등급변경 추가
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
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    
    /// </summary>
    public partial class FCS001_034_NJ : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        int addRows;
        int iBadCnt;
        int iCurrCnt;
        Util _Util = new Util();
        bool bBGradeUseFlag = false;
        public FCS001_034_NJ()
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
            bBGradeUseFlag = _Util.IsAreaCommonCodeUse("PROGRAM_BY_FUNC_USE_FLAG", "FCS001_034_NJ"); // 2023.09.25 동별코드 등급변경

            InitCombo();
            //  InitControl();
            //  SetEvent();
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter = { "COMBO_SUBLOTJUDGE_CHANGE", "Y", null, null, null, null };
            string sCase = "CMN_WITH_OPTION";
            //_combo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.SELECT, sCase: "CMN_WITH_OPTION", sFilter: sFilter);

            if (bBGradeUseFlag)
            {
                // 2023.09.25 동별 Grade 사용
                sFilter[0] = "SUBLOTJUDGE_CHANGE";
                sCase = "AREA_COMMON_CODE";
            };
            _combo.SetCombo(cboGrade, CommonCombo_Form.ComboStatus.SELECT, sCase: sCase, sFilter: sFilter);
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
                if (cboGrade.GetBindValue() == null)
                {
                    //등급을 선택해주세요.
                    cboGrade.SetValidation(MessageDic.Instance.GetMessage("FM_ME_0122"));
                    return;
                }

                // 2023.09.25 ESNJ 동별 기준정보에 From 등급 정보가 있을경우 Check
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

                        // Check Plan1 Only A, B
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FINL_JUDG_CODE").ToString().Equals(sChkFrGrade) == false)
                            {
                                Util.MessageValidation("SFU3747", sToGraede, sChkFrGrade); // {%1}등급 변경은 {%2}등급만 가능합니다.
                                return;
                            }
                        }

                        // Check Plan2 All Grade
                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            if (LstFrGrade.Contains(DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "FINL_JUDG_CODE").ToString()) == false)
                            {
                                Util.MessageValidation("SFU3747", sToGraede, sChkFrGrade); // {%1}등급 변경은 {%2}등급만 가능합니다.
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
                        DataTable dtRqst = new DataTable();
                        dtRqst.TableName = "RQSTDT";
                        dtRqst.Columns.Add("SUBLOTID", typeof(string));
                        dtRqst.Columns.Add("SUBLOTJUDGE", typeof(string));
                        dtRqst.Columns.Add("USERID", typeof(string));

                        for (int i = 0; i < dgCellList.Rows.Count; i++)
                        {
                            DataRow dr = dtRqst.NewRow();
                            dr["SUBLOTID"] = DataTableConverter.GetValue(dgCellList.Rows[i].DataItem, "SUBLOTID");
                            dr["SUBLOTJUDGE"] = Util.GetCondition(cboGrade);
                            dr["USERID"] = LoginInfo.USERID;

                            if (string.IsNullOrEmpty(dr["SUBLOTJUDGE"].ToString()))
                                return;

                            dtRqst.Rows.Add(dr);
                        }

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_SET_CELL_GRADE_CHANGE", "RQSTDT", "RSLTDT", dtRqst);
                        Util.MessageInfo("FM_ME_0136"); //변경완료하였습니다.
                        GetDataFromGrid(dgCellList);
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
        #endregion

        private string GetChkFrGrade(string sToGrade)
        {
            // 2023.09.25 변경 가능한 From 등급 조회
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
    }
}
