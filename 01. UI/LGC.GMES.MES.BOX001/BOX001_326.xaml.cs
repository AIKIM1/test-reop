/*************************************************************************************
 Created Date : 2023.08.08
      Creator : 
   Decription : 포장완료 Sorting Cell선별요청
--------------------------------------------------------------------------------------
 [Change History]
  2023.08.08  DEVELOPER : Initial Created.
  2023.08.17  박나연    : 다국어 처리 추가
  2024.07.23  최석준    : 반품여부 컬럼 추가 (2025년 적용예정, 수정 시 연락 부탁드립니다)
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using Microsoft.Win32;
using System.IO;
using System.Configuration;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_326.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_326 : UserControl, IWorkArea
    {
        Util _util = new Util();
        CommonCombo _combo = new CommonCombo();
        CommonCombo_Form _comboF = new CommonCombo_Form();

        private List<String> lotList_tpSaveCell = null;
        private int cnt_sum;
        private int cnt_error;
        private int cnt_model;
        

        public BOX001_326()
        {
            InitializeComponent();

            Initialize();

            Loaded += BOX001_326_Loaded;
        }

        private void BOX001_326_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= BOX001_326_Loaded;

            initSet();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region [Initialize]
        private void initSet()
        {
            // Area 셋팅
            _combo.SetCombo(cboAreaR, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            _combo.SetCombo(cboAreaA, CommonCombo.ComboStatus.SELECT, sCase: "AREA_CP");
            // 처리구분
            string[] sFilterC = { "FORM_PROCESS_CATEGORY_CODE" };
            _comboF.SetCombo(cboProcessCodeR, CommonCombo_Form.ComboStatus.SELECT, sCase: "SORTING", sFilter: sFilterC);
            _comboF.SetCombo(cboProcessCodeRH, CommonCombo_Form.ComboStatus.ALL, sCase: "SORTING", sFilter: sFilterC);
            _comboF.SetCombo(cboProcessCodeAH, CommonCombo_Form.ComboStatus.ALL, sCase: "SORTING", sFilter: sFilterC);
            //sorting상태
            string[] sFilterS = { "FORM_SORTING_RSLT_CODE" };
            _comboF.SetCombo(cboSortingRsltRH, CommonCombo_Form.ComboStatus.ALL, sCase: "SORTING", sFilter: sFilterS);
            //요청서 상태
            string[] sFilterR = { "REQ_RSLT_CODE", "Y" };
            _comboF.SetCombo(cboReqRsltAH, CommonCombo_Form.ComboStatus.ALL, sCase: "SORTING", sFilter: sFilterR);
            //지연여부
            string[] sFilterD = { "FORM_DELAY_RSLT_CODE" };
            _comboF.SetCombo(cboDelayRsltRH, CommonCombo_Form.ComboStatus.ALL, sCase: "SORTING", sFilter: sFilterD);

            txtSortingBetch.Text = string.Empty;
        }

        private void Initialize()
        {
            // 사외반품여부 컬럼 숨김여부
            if (GetOcopRtnPsgArea())
            {
                dgInputList.Columns["RTN_FLAG"].Visibility = Visibility.Visible;
            }

        }
        #endregion

        #region [요청화면 검색, 엑셀]
        private void btnCellSearch_Click(object sender, RoutedEventArgs e)
        {
            GetCellData();
        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            string sCellID = LoadExcel();
            GetCellData(sCellID);
        }

        private void GetCellData(string sCellID = "")
        {
            try
            {
                cnt_sum = 0;
                cnt_error = 0;
                cnt_model = 0;

                txtInsertCellCnt.Text = string.Empty;
                txtBadInsertRow.Text = string.Empty;

                
                //SUBLOTID로 검색
                if (string.IsNullOrEmpty(sCellID))
                {
                    //SUBLOTID - XADDA14223,XADDA14224,XADDA14225 ...
                    for (int iRow = 0; iRow < dgInputList.Rows.Count; iRow++)
                    {
                        string sublot = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "SUBLOTID"));
                        if (string.IsNullOrEmpty(sublot))
                            continue;
                        sCellID += sublot;
                        sCellID += ",";
                    }
                }

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SUBLOTID"] = sCellID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SORTING_SUBLOT_BX", "RQSTDT", "RSLTDT", dtRqst, menuid: FrameOperation.MENUID);
                if (dtRslt.Rows.Count == 0)
                {
                    Util.MessageValidation("FM_ME_0021");  //Cell 정보가 존재하지않습니다.
                    return;
                }
                cnt_sum = dtRslt.Rows.Count;
                dtRslt.Columns.Add("E_MESSAGE", typeof(string));
                Util.GridSetData(dgInputList, dtRslt, this.FrameOperation);
                lotList_tpSaveCell = new List<String>();
                //첫 셀
                if (lotList_tpSaveCell.Count == 0)
                {
                    lotList_tpSaveCell.Add(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[0].DataItem, "MDLLOT_ID")));
                }
                
                for (int iRow = 0; iRow < dgInputList.GetRowCount(); iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "SORTING_ENABLE_FLAG")).Equals("N"))
                    {
                        cnt_error++;
                        dgInputList.SetValue(iRow, "E_MESSAGE", MessageDic.Instance.GetMessage("FM_ME_0024")); // CELL 공정을 확인해주세요.
                    }

                    // 다른 모델 입력 불가
                    if (!lotList_tpSaveCell[0].ToString().Equals(Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[iRow].DataItem, "MDLLOT_ID").ToString())))
                    {
                        cnt_model++;
                        
                    }
                }

                if(cnt_error > 0) btnReq.IsEnabled = false;
                if(cnt_model > 0)
                {
                    Util.MessageValidation("FM_ME_0130");  //모델이 다른 Cell을 추가할 수 없습니다.
                    btnReq.IsEnabled = false;
                    return;
                }

                txtInsertCellCnt.Text = cnt_sum.ToString();
                txtBadInsertRow.Text = cnt_error.ToString();
               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private string LoadExcel()
        {

            DataTable dtInfo = DataTableConverter.Convert(dgInputList.ItemsSource);

            dtInfo.Clear();

            OpenFileDialog fd = new OpenFileDialog();

            if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
            {
                fd.InitialDirectory = @"\\Client\C$";
            }

            fd.Filter = "Excel Files (.xlsx)|*.xlsx";

            string sColData = string.Empty;

            if (fd.ShowDialog() == true)
            {
                using (Stream stream = fd.OpenFile())
                {
                    C1XLBook book = new C1XLBook();
                    book.Load(stream, FileFormat.OpenXml);
                    XLSheet sheet = book.Sheets[0];

                    for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                    {
                        // CELL ID;
                        if (sheet.GetCell(rowInx, 0) == null)
                            return sColData;
                        sColData += Util.NVC(sheet.GetCell(rowInx, 0).Text);
                        sColData += ",";
                    }
                }
            }

            return sColData;
        }
        #endregion

        #region [요청화면 초기화버튼]
        private void btnCellClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearValidation();

            dgInputList.ClearRows();
            txtInsertCellCnt.Text = "";
            txtBadInsertRow.Text = "";
            btnReq.IsEnabled = true;
            DataGridRowAdd(dgInputList, Convert.ToInt32(txtRowCntInsertCell.Text));
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg, int iRowcount)
        {
            try
            {
                DataTable dt = new DataTable();

                if (Math.Abs(iRowcount) > 0)
                {
                    if (iRowcount > 900)
                    {
                        //최대 ROW수는 900입니다.
                        Util.MessageValidation("SFU4648", "900");
                        return;
                    }
                }

                foreach (C1.WPF.DataGrid.DataGridColumn col in dg.Columns)
                {
                    dt.Columns.Add(Convert.ToString(col.Name));
                }

                if (dg.ItemsSource != null)
                {
                    for (int i = 0; i < iRowcount; i++)
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
                    for (int i = 0; i < iRowcount; i++)
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
        #endregion

        #region [요청화면 승인자검색]
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


                        dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtGrator.Text = "";
                    }
                    else
                    {
                        dgGratorSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgGratorSelect);

                        dgGratorSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

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

        #region [요청화면 승인자 검색결과 여러개일경우]
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


            dgGrator.ItemsSource = DataTableConverter.Convert(dtTo);

            dgGratorSelect.Visibility = Visibility.Collapsed;

            txtGrator.Text = "";
        }

        #endregion

        #region [요청화면 담당자검색]
        private void txtManager_KeyDown(object sender, KeyEventArgs e)   
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    DataTable dtRqst = new DataTable();
                    dtRqst.Columns.Add("USERNAME", typeof(string));
                    dtRqst.Columns.Add("LANGID", typeof(string));

                    DataRow dr = dtRqst.NewRow();
                    dr["USERNAME"] = txtManager.Text;
                    dr["LANGID"] = txtManager.Text;

                    dtRqst.Rows.Add(dr);

                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_NAME", "INDATA", "OUTDATA", dtRqst);

                    if (dtRslt.Rows.Count == 0)
                    {
                        Util.Alert("SFU1592");  //사용자 정보가 없습니다.
                    }
                    else if (dtRslt.Rows.Count == 1)
                    {

                        DataTable dtTo = DataTableConverter.Convert(dgManager.ItemsSource);

                        if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
                        {
                            dtTo.Columns.Add("USERID", typeof(string));
                            dtTo.Columns.Add("USERNAME", typeof(string));
                            dtTo.Columns.Add("DEPTNAME", typeof(string));
                        }

                        if (dtTo.Select("USERID = '" + dtRslt.Rows[0]["USERID"].ToString() + "'").Length > 0) //중복조건 체크
                        {
                            Util.Alert("이미 추가된 담당자입니다.");
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

                        dgManager.ItemsSource = DataTableConverter.Convert(dtTo);

                        txtManager.Text = "";
                    }
                    else
                    {
                        dgManagerSelect.Visibility = Visibility.Visible;

                        Util.gridClear(dgManagerSelect);

                        dgManagerSelect.ItemsSource = DataTableConverter.Convert(dtRslt);

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

        #region [요청화면 담당자 검색결과 여러개일경우]
        private void dgManagerChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            DataTable dtTo = DataTableConverter.Convert(dgManager.ItemsSource);

            if (dtTo.Columns.Count == 0)//최초 바인딩이 안된 상태라서 컬럼들이 없음. 추가후 처리
            {
                dtTo.Columns.Add("USERID", typeof(string));
                dtTo.Columns.Add("USERNAME", typeof(string));
                dtTo.Columns.Add("DEPTNAME", typeof(string));
            }

            if (dtTo.Select("USERID = '" + DataTableConverter.GetValue(rb.DataContext, "USERID").ToString() + "'").Length > 0) //중복조건 체크
            {
                Util.Alert("SFU1779");  //이미 추가 된 승인자 입니다.
                dgManagerSelect.Visibility = Visibility.Collapsed;
                return;
            }

            DataRow drFrom = dtTo.NewRow();
            drFrom["USERID"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERID"));
            drFrom["USERNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "USERNAME"));
            drFrom["DEPTNAME"] = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "DEPTNAME"));

            dtTo.Rows.Add(drFrom);


            dgManager.ItemsSource = DataTableConverter.Convert(dtTo);

            dgManagerSelect.Visibility = Visibility.Collapsed;

            txtManager.Text = "";
        }
        #endregion

        #region [요청화면 요청버튼]
        private void btnReq_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //요청하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2924"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Req();
                            }
                        });

            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
            }
        }

        private void Req()
        {
            string sTo = "";
            string sMa = "";

            if (dgGrator.Rows.Count == 0)
            {
                Util.Alert("SFU1692");  //승인자가 필요합니다.
                return;
            }

            if (dgManager.Rows.Count == 0)
            {
                Util.Alert("SFU4011");  //담당자가 필요합니다.
                return;
            }

            if (dgInputList.Rows.Count == 0)
            {
                Util.Alert("SFU1748");  //요청 목록이 필요합니다.
                return;
            }

            string sPrC = Util.NVC(cboProcessCodeR.SelectedValue);
            if (sPrC.Equals("SELECT") || sPrC.Equals(""))
            {
                Util.Alert("SFU4650");  //처리구분을 선택하세요.
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text)|| string.IsNullOrWhiteSpace(txtNote.Text))
            {
                Util.Alert("SFU4651");  //요청명을 입력하세요.
                return;
            }

            
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("APPR_BIZ_CODE", typeof(string)); // LOT_SORTING
            inDataTable.Columns.Add("USERID", typeof(string));        //요청자
            inDataTable.Columns.Add("REQ_NOTE", typeof(string));      // 요청명
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("PRIS_DIVS_CODE", typeof(string));      //처리구분
            inDataTable.Columns.Add("REQ_CMPL_DATE", typeof(string)); //요청완료일

            DataRow row = null;
            row = inDataTable.NewRow();
            row["APPR_BIZ_CODE"] = "LOT_SORTING";
            row["USERID"] = LoginInfo.USERID;
            row["REQ_NOTE"] = Util.NVC(txtNote.Text); 
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["PRIS_DIVS_CODE"] = Util.NVC(cboProcessCodeR.SelectedValue);
            row["REQ_CMPL_DATE"] = dtCalDate.SelectedDateTime.ToString("yyyyMMdd");

            inDataTable.Rows.Add(row);

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

            //담당자
            DataTable inRef = inData.Tables.Add("INREF");
            inRef.Columns.Add("REF_USERID", typeof(string));

            for (int i = 0; i < dgManager.Rows.Count; i++)
            {
                row = inRef.NewRow();
                row["REF_USERID"] = Util.NVC(DataTableConverter.GetValue(dgManager.Rows[i].DataItem, "USERID"));
                inRef.Rows.Add(row);

                sMa += Util.NVC(DataTableConverter.GetValue(dgManager.Rows[i].DataItem, "USERID")) + ";";
            }

            //대상 CELL
            DataTable inSublot = inData.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("SUBLOTID", typeof(string));
            inSublot.Columns.Add("LOTID", typeof(string));
            for (int i = 0; i < dgInputList.Rows.Count; i++)
            {
                row = inSublot.NewRow();
                row["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "SUBLOTID"));
                row["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputList.Rows[i].DataItem, "LOTID"));
                inSublot.Rows.Add(row);
            }

            try
            {
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SORTING_APPR_REQUEST", "INDATA,INPROG,INREF,INSUBLOT", "OUTDATA", inData);
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                {
                    Util.gridClear(dgInputList);
                    Util.gridClear(dgGrator);
                    Util.gridClear(dgManager);
                    MailSend mail = new CMM001.Class.MailSend();
                    string sMsg = "["+ dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + "]"+ ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = dsRslt.Tables["OUTDATA"].Rows[0]["REQ_NO"].ToString() + " Pallet Cell " + ObjectDic.Instance.GetObjectName("PICK_APPR_REQUEST");
                    string mailBody = MessageDic.Instance.GetMessage("SFU8575"); 

                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, sTo, sMa, sMsg, mail.makeBodyApp(sTitle, mailBody));
                }
                Util.AlertInfo("SFU1747");  //요청되었습니다.
                txtInsertCellCnt.Text = string.Empty;
                txtBadInsertRow.Text = string.Empty;
                txtNote.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [요청화면 테이블]
        private void dgInputList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("SUBLOTID"))
                    {
                        e.Column.HeaderPresenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Column.HeaderPresenter.FontWeight = FontWeights.Bold;
                        e.Column.HeaderPresenter.Cursor = Cursors.Hand;
                    }
                }
            }));
        }
        #endregion

        

        

        #region [공통  ROW삭제]
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

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion
        
        #region [요청이력 검색]
        private void btnSearchRH_Click(object sender, RoutedEventArgs e)
        {
            try
            {   
                string areaIDR = string.Empty;
                txtSortingBetch.Text = string.Empty;

                string sTemp = Util.NVC(cboAreaR.SelectedValue);
                if (sTemp == "" || sTemp == "SELECT")
                {
                    areaIDR = "";
                    Util.MessageValidation("SFU3203"); 
                    return;
                }
                else
                {
                    string[] sArry = sTemp.Split('^');
                    areaIDR = sArry[0];
                }

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("PRCS_DIVS_CODE", typeof(string));
                dtRqst.Columns.Add("PICK_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("DELAY_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["APPR_BIZ_CODE"] = "LOT_SORTING";
                dr["AREAID"] = areaIDR;
                dr["FROM_DATE"] = ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = ldpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["LOTID"] = Util.NVC(txtLotID.Text)=="" ? null : txtLotID.Text;
                dr["SUBLOTID"] = string.IsNullOrWhiteSpace(txtSubLotID.Text) ? null : txtSubLotID.Text;
                dr["USERNAME"] = string.IsNullOrWhiteSpace(txtReqUser.Text) ? null : txtReqUser.Text;  //요청자
                dr["PRCS_DIVS_CODE"] = Util.NVC(cboProcessCodeRH.SelectedValue) == "" ? null : cboProcessCodeRH.SelectedValue;
                dr["PICK_RSLT_CODE"] = Util.NVC(cboSortingRsltRH.SelectedValue) == "" ? null : cboSortingRsltRH.SelectedValue;
                dr["DELAY_RSLT_CODE"] = Util.NVC(cboDelayRsltRH.SelectedValue) == "" ? null : cboDelayRsltRH.SelectedValue;
                dr["MDLLOT_ID"] = string.IsNullOrWhiteSpace(textMDLotIDH.Text) ? null : textMDLotIDH.Text;
                
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SORTING_APPROVAL_REQ_HIST", "RQSTDT", "RSLTDT", dtRqst);
                dtRslt.Columns.Add("PICK", typeof(string));
                Util.GridSetData(dgReqHList, dtRslt, FrameOperation);

                for (int iRow = 0; iRow < dgReqHList.GetRowCount(); iRow++)
                {
                    dgReqHList.SetValue(iRow, "PICK", Util.NVC(DataTableConverter.GetValue(dgReqHList.Rows[iRow].DataItem, "PICK_RATE")) + "% (" + Util.NVC(DataTableConverter.GetValue(dgReqHList.Rows[iRow].DataItem, "PICK_COUNT")) + "/" + Util.NVC(DataTableConverter.GetValue(dgReqHList.Rows[iRow].DataItem, "SUBLOT_CNT"))+")");
                }

                if (dgReqHList.Rows.Count > 0)
                {
                    DataTable dtBRqst = new DataTable();
                    dtBRqst.Columns.Add("LANGID", typeof(string));

                    DataRow drB = dtBRqst.NewRow();
                    drB["LANGID"] = LoginInfo.LANGID;

                    dtBRqst.Rows.Add(drB);
                    DataTable dtBRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LAST_SORTING_BTCH_TIME", "RQSTDT", "RSLTDT", dtBRqst);
                    txtSortingBetch.Text = "★ " + ObjectDic.Instance.GetObjectName("LAST_BTCH_TIME") + " " + dtBRslt.Rows[0]["LAST_BTCH_TIME"].ToString();
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [요청이력 테이블]
        private void dgRegHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {

            dgReqHList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //선별률 색변경
                if (e.Cell.Column.Name == "PICK")
                {
                    string colorF = dgReqHList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index+1).Text;
                    if (colorF == "R" )  
                    {
                        dgReqHList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                    }
                    else if (colorF == "G")
                    {
                        dgReqHList.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                    }
                }
            }));
        }
        #endregion

        #region [요청이력 선별률클릭]
        private void dgList_SortingDoubleClick(object sender, RoutedEventArgs e)
        {
            if ((dgReqHList.CurrentRow != null && dgReqHList.CurrentColumn.Name.Equals("PICK")))
            {
                BOX001_326_SORTING_LIST wndPopup = new BOX001_326_SORTING_LIST();
                wndPopup.FrameOperation = FrameOperation;

                if (wndPopup != null)
                {
                    object[] Parameters = new object[1];

                    Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgReqHList.CurrentRow.DataItem, "REQ_NO"));
                    C1WindowExtension.SetParameters(wndPopup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                }

            }
        }
        #endregion

        #region [승인 검색]
        private void btnSearchA_Click(object sender, RoutedEventArgs e)
        {
            GetApprReqList();
        }
        #endregion

        #region [승인 작업대상]
        private void GetApprReqList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));
                dtRqst.Columns.Add("USERNAME", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERNAME"] = string.IsNullOrWhiteSpace(txtReqUserA.Text) ? null : txtReqUserA.Text;
                dr["USERID"] = LoginInfo.USERID;
                dr["APPR_BIZ_CODE"] = "LOT_SORTING";

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SORTING_APPROVAL_TARGET_LIST", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgApprReqList, dtRslt, FrameOperation);
                Util.gridClear(dgAccept);

                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [승인 요청자keydown]
        private void txtReqUserA_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetApprReqList();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [승인 테이블]
        private void dgAppr_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            dgApprReqList.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                //link 색변경
                if (e.Cell.Column.Name.Equals("REQ_NO"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }

            }));
        }
        #endregion
        

        #region [승인 요청번호 클릭]
        private void dgApprReqList_RequestDoubleClick(object sender, RoutedEventArgs e)
        {
             BOX001_326_SORTING_REQUEST wndPopup = new BOX001_326_SORTING_REQUEST();
             wndPopup.FrameOperation = FrameOperation;
             
             if (wndPopup != null)
             {
                 object[] Parameters = new object[1];
                 Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgApprReqList.CurrentRow.DataItem, "REQ_NO"));
                
                 C1WindowExtension.SetParameters(wndPopup, Parameters);
             
                 this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
             }
        }
        #endregion

        #region [승인 승인차수별내용 검색]
        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            //최초 체크시에만 로직 타도록 구현
            if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals(0))
            {
                //체크시 처리될 로직
                string sReqNo = DataTableConverter.GetValue(rb.DataContext, "REQ_NO").ToString();

                //승인내용 조회
                GetApprovalList(sReqNo);

                foreach (C1.WPF.DataGrid.DataGridRow row in ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows)
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", 0);
                }

                DataTableConverter.SetValue(rb.DataContext, "CHK", 1);
                //row 색 바꾸기
                ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.SelectedIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Cell.Row.Index;

                txtRemark.Text = "";
        }
    }

        private void GetApprovalList(string sReqNo)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_APPROVAL_DETAIL_LIST", "RQSTDT", "RSLTDT", dtRqst);

                //Util.gridClear(dgAccept);
                //dgAccept.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.GridSetData(dgAccept, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [승인 승인버튼]
        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] drChk = Util.gridGetChecked(ref dgApprReqList, "CHK");
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                string sMsg = "SFU2878"; //승인하시겠습니까?
                string sLot = string.Empty;
                

                Util.MessageConfirm(sMsg, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Accept();
                    }
                }, sLot);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Accept()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgApprReqList, "CHK");

            try
            {
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                DataSet dsRqst = new DataSet();

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "INDATA";

                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "APP";
                dr["APPR_NOTE"] = Util.GetCondition(txtRemark);

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SORTING_APPR", "INDATA", "OUTDATA", dsRqst);

                Util.AlertInfo("SFU1690");  //승인되었습니다.

                MailSend mail = new CMM001.Class.MailSend();
                if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                { //다음차수 안내메일
                    string sMsg = "[" + drChk[0]["REQ_NO"].ToString() + "]" + ObjectDic.Instance.GetObjectName("승인요청");
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " Pallet Cell " + ObjectDic.Instance.GetObjectName("PICK_APPR_REQUEST");
                    string mailBody = MessageDic.Instance.GetMessage("SFU8575");
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, dsRslt.Tables["OUTDATA"].Rows[0]["APPR_USERID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, mailBody));
                }
                else
                {  //완료메일
                    string sMsg = "[" + drChk[0]["REQ_NO"].ToString() + "]" + ObjectDic.Instance.GetObjectName("완료"); //승인완료
                    string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                    string mailBody = MessageDic.Instance.GetMessage("SUF9017");
                    mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, GetCC(drChk[0]["REQ_NO"].ToString()), drChk[0]["REQ_USER_ID"].ToString(), sMsg, mail.makeBodyApp(sTitle, mailBody));
                }

                GetApprReqList();
                Util.gridClear(dgAccept);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [승인 반려버튼]
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            //반려하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU2866"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                Reject();
                            }
                        });
        }

        private void Reject()
        {
            DataRow[] drChk = Util.gridGetChecked(ref dgApprReqList, "CHK");

            try
            {
                if (drChk.Length == 0)
                {
                    Util.AlertInfo("SFU1654");  //선택된 요청이 없습니다.
                    return;
                }

                DataSet dsRqst = new DataSet();

                DataTable dtRqst = new DataTable();

                dtRqst.TableName = "INDATA";

                dtRqst.Columns.Add("REQ_NO", typeof(string));
                dtRqst.Columns.Add("APPR_USERID", typeof(string));
                dtRqst.Columns.Add("APPR_RSLT_CODE", typeof(string));
                dtRqst.Columns.Add("APPR_NOTE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["REQ_NO"] = drChk[0]["REQ_NO"].ToString();
                dr["APPR_USERID"] = LoginInfo.USERID;
                dr["APPR_RSLT_CODE"] = "REJ";
                dr["APPR_NOTE"] = txtRemark.Text;

                dtRqst.Rows.Add(dr);

                dsRqst.Tables.Add(dtRqst);

                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_SORTING_APPR", "INDATA", "OUTDATA", dsRqst);

                MailSend mail = new CMM001.Class.MailSend();
                string sMsg = "[" + drChk[0]["REQ_NO"].ToString() + "]" + ObjectDic.Instance.GetObjectName("반려");
                string sTitle = drChk[0]["REQ_NO"].ToString() + " " + sMsg;
                string mailBody = MessageDic.Instance.GetMessage("SFU1541"); 

                mail.SendMail(LoginInfo.USERID, LoginInfo.USERNAME, drChk[0]["REQ_USER_ID"].ToString(), GetCC(drChk[0]["REQ_NO"].ToString()), sMsg, mail.makeBodyApp(sTitle, mailBody));
                Util.AlertInfo("SFU1541");  //반려되었습니다.


                GetApprReqList();
                Util.gridClear(dgAccept);
                txtRemark.Text = "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [승인 참조자 가져오기]
        private string GetCC(string sReqNo)
        {
            string sCC = "";
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("REQ_NO", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["REQ_NO"] = sReqNo;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_APPR_REF", "INDATA", "OUTDATA", dtRqst);

                for (int i = 0; i < dtRslt.Rows.Count; i++)
                {
                    sCC += dtRslt.Rows[i]["USERID"].ToString() + ";";
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            return sCC;
        }
        #endregion

        #region [승인이력 검색]
        private void btnSearchAH_Click(object sender, RoutedEventArgs e)
        {
            GetApprHistList();
        }

        public void GetApprHistList()
        {
            try
            {

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("APPR_BIZ_CODE", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));  
                dtRqst.Columns.Add("FROM_DATE", typeof(string)); 
                dtRqst.Columns.Add("TO_DATE", typeof(string));   
                dtRqst.Columns.Add("REQ_USERNAME", typeof(string));     //요청자
                dtRqst.Columns.Add("APPR_USERNAME", typeof(string));    //요청자
                dtRqst.Columns.Add("PRCS_DIVS_CODE", typeof(string));   //처리구분
                dtRqst.Columns.Add("REQ_RSLT_CODE", typeof(string));    //요청서 상태
                dtRqst.Columns.Add("MDLLOT_ID", typeof(string));
                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;

                DataTable dtRslt = new DataTable();
                
                string areaIDS = string.Empty;
                string sTemp = Util.NVC(cboAreaA.SelectedValue);
                if (sTemp == "" || sTemp == "SELECT")
                {
                    areaIDS = "";
                    Util.MessageValidation("SFU3203");
                    return;
                }
                else
                {
                    string[] sArry = sTemp.Split('^');
                    areaIDS = sArry[0];
                }

                dr["AREAID"] = areaIDS;//동은필수입니다.
                if (dr["AREAID"].Equals("")) return;
                dr["FROM_DATE"] = ldpDateFromAH.SelectedDateTime.ToString("yyyyMMdd");
                dr["TO_DATE"] = ldpDateToAH.SelectedDateTime.ToString("yyyyMMdd");
                dr["REQ_USERNAME"] = string.IsNullOrWhiteSpace(txtReqUserAH.Text) ? null : txtReqUserAH.Text;
                dr["APPR_USERNAME"] = string.IsNullOrWhiteSpace(txtApprUserAH.Text) ? null : txtApprUserAH.Text;
                dr["APPR_BIZ_CODE"] = "LOT_SORTING";
                dr["PRCS_DIVS_CODE"] = Util.NVC(cboProcessCodeAH.SelectedValue) == "" ? null : cboProcessCodeAH.SelectedValue;
                dr["REQ_RSLT_CODE"] = Util.NVC(cboReqRsltAH.SelectedValue) == "" ? null : cboReqRsltAH.SelectedValue;
                dr["MDLLOT_ID"] = string.IsNullOrWhiteSpace(textMDLotIDAH.Text) ? null : textMDLotIDAH.Text;

                dtRqst.Rows.Add(dr);

                dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SORTING_APPROVAL_APPR_HIST", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgApprHistList, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [승인이력 테이블]
        private void dgApprHist_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1DataGrid dataGrid = sender as C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "REQ_NO")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [승인이력 요청번호 클릭]
        private void dgApprHistList_RequestDoubleClick(object sender, RoutedEventArgs e)
        {
            BOX001_326_SORTING_REQUEST wndPopup = new BOX001_326_SORTING_REQUEST();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[1];
                
                Parameters[0] = Util.NVC(DataTableConverter.GetValue(dgApprHistList.CurrentRow.DataItem, "REQ_NO"));
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }
        #endregion


        #region [공통코드 조회]
        /// <summary>
        /// 활성화 사외 반품 처리 여부 사용 Area 조회
        /// </summary>
        /// <returns></returns>
        private bool GetOcopRtnPsgArea()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("CMCDTYPE", typeof(string));
            inTable.Columns.Add("CMCODE", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "FORM_OCOP_RTN_PSG_YN";
            dr["CMCODE"] = LoginInfo.CFG_AREA_ID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", inTable);

            if (dtRslt.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

    }
}
