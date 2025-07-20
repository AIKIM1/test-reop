/*************************************************************************************
 Created Date : 2021.12.30
      Creator : 오화백
   Decription : Sorting Cell 엑셀 업로드
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using System.Windows.Media.Animation;


namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_320_SORTING_CELL_UPLOAD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        string _holdTrgtCode = string.Empty;

        //private double maxCell = 0;
      
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

        public BOX001_320_SORTING_CELL_UPLOAD()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tempParameters = C1WindowExtension.GetParameters(this);
            //maxHoldCell = Double.Parse(tempParameters[1].ToString());
            //cellDivideCnt = Double.Parse(tempParameters[2].ToString());
            //tempFixCnt = Convert.ToInt32(cellDivideCnt);

            //xTextMaxCnt.Text = "Max Cell : " + maxHoldCell.ToString();

            //if (tempParameters != null && tempParameters.Length >= 1)
            //{

            //}

            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
         
            DataTable dt = new DataTable();
            for (int i = 0; i < dgCell.Columns.Count; i++)
            {
                dt.Columns.Add(dgCell.Columns[i].Name);
            }

            Util.GridSetData(dgCell, dt, FrameOperation);

            // 팔레트 바코드 표시 여부
            isVisibleBCD(LoginInfo.CFG_AREA_ID);

        }

        private void dgCell_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgCell.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion
    
        #region Event
      
        #region 엑셀 양식 다운로드 : btnDownLoad_Click()
        /// <summary>
        /// 엑셀 업로드 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Sorting_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "PalletID";
                    sheet[1, 0].Value = "PBRFB3001";


                    sheet[0, 1].Value = "Cell ID";
                    sheet[1, 1].Value = "RTJNF11947";

                    sheet[0, 0].Style = sheet[0, 1].Style = styel;
                    sheet.Columns[0].Width = sheet.Columns[1].Width = 1500;


                    c1XLBook1.Save(od.FileName);
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 엑셀 업로드 : btnUpLoad_Click()
        /// <summary>
        /// 엑셀 업로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgCell.ItemsSource);

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

                        if (sheet.GetCell(0, 0).Text != "PalletID" || sheet.GetCell(0, 1).Text != "Cell ID")
                        {
                            //SFU4424	형식에 맞는 EXCEL파일을 선택해 주세요.
                            Util.MessageValidation("SFU4424");
                            return;
                        }


                        // Pallet BCD 컬럼 활성화 상태일 경우만 getPalletBCD 호출 (boxid, pallet bcd 값 적용)
                        bool _chkPalletbcd = false;
                        if (dgCell.Columns["PLLT_BCD_ID"].Visibility == Visibility.Visible)
                            _chkPalletbcd = true;

                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            // LOTID 미입력시 return;
                            if (sheet.GetCell(rowInx, 0) == null || sheet.GetCell(rowInx, 1) == null)
                                return;

                            DataRow dr = dtInfo.NewRow();
                            dr["CHK"] = true;
                            dr["OUTER_BOXID"] = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                            dr["SUBLOTID"] = sheet.GetCell(rowInx, 1).Text.Replace("\r", "").Replace("\n", "").Trim();
                            // Pallet BCD 조회
                            if (_chkPalletbcd)
                            {
                                string _boxid = sheet.GetCell(rowInx, 0).Text.Replace("\r", "").Replace("\n", "").Trim();
                                dr["PLLT_BCD_ID"] = getPalletInfo(_boxid);
                            }

                            dtInfo.Rows.Add(dr);

                            //if (sheet.Rows.Count > Convert.ToInt32(maxHoldCell))
                            //    break;
                        }

                        //if (sheet.Rows.Count > Convert.ToInt32(maxHoldCell))
                        //{
                        //    Util.MessageValidation("SFU8217", maxHoldCell);
                        //}

                        if (dtInfo.Rows.Count > 0)
                            dtInfo = dtInfo.DefaultView.ToTable(true);

                        Util.GridSetData(dgCell, dtInfo, FrameOperation);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 개별 Cell Row 추가 : btnAdd_Click()
        /// <summary>
        /// CELL 리스트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);
            DataRow dr = dt.NewRow();
            dt.Rows.Add(dr);

            Util.GridSetData(dgCell, dt, FrameOperation);
            dgCell.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        #endregion

        #region  개별 Cell Row 삭제 : btnDelete_Click()






        /// <summary>
        /// CELL 리스트 제외
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgCell.ItemsSource);

            List<DataRow> drList = dt.Select("CHK = 'True'")?.ToList();
            if (drList.Count > 0)
            {
                foreach (DataRow dr in drList)
                {
                    dt.Rows.Remove(dr);
                }
                Util.GridSetData(dgCell, dt, FrameOperation);
                chkAll.IsChecked = false;
            }
        }
        #endregion

        #region 저장 버튼 이벤트 : btnSave_Click()
        /// <summary>
        /// CELL 등록
        /// BIZ : BR_PRD_REG_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgCell.ItemsSource);
            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }
            if (dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("OUTER_BOXID"))).ToList().Count > 0)
            {
                //SFU4351		미입력된 항목이 존재합니다.	
                Util.MessageValidation("SFU4351");
                return;
            }
            if (dtInfo.AsEnumerable().Where(c => string.IsNullOrWhiteSpace(c.Field<string>("SUBLOTID"))).ToList().Count > 0)
            {
                //SFU4351		미입력된 항목이 존재합니다.	
                Util.MessageValidation("SFU4351");
                return;
            }

            if (dtInfo.AsEnumerable().Where(c => (string.IsNullOrWhiteSpace(c.Field<string>("SUBLOTID"))
                                        )).ToList().Count > 0)
            {
                //SFU1209		Cell 정보가 없습니다.	
                Util.MessageValidation("SFU1209");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtRequester.Text) || txtRequester.Tag == null)
            {
                //등록자를 입력하세요
                Util.MessageValidation("FM_ME_0123", (result) =>
                {
                    txtRequester.Focus();
                });
               return ;
            }
           
            try
            {
                // DATA SET 
                DataTable dtCell = DataTableConverter.Convert(dgCell.ItemsSource).Select("CHK = True").CopyToDataTable();
                if (dtCell == null || dtCell.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("INSDTTM", typeof(DateTime));
                inTable.Columns.Add("UPDUSER", typeof(string));
                inTable.Columns.Add("UPDDTTM", typeof(DateTime));
                inTable.Columns.Add("TYPE", typeof(string));
                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["INSUSER"] = txtRequester.Tag;
                newRow["INSDTTM"] = System.DateTime.Now;
                newRow["UPDUSER"] = txtRequester.Tag;
                newRow["UPDDTTM"] = System.DateTime.Now;
                newRow["TYPE"] = "INS";
                inTable.Rows.Add(newRow);

                // XML string 500건당 1개 생성
                int rowCount = 0;
                string sXML = string.Empty;


                for (int row = 0; row < dtCell.Rows.Count; row++)
                {
                    if (row == 0 || row % 250 == 0)
                    {
                        sXML = "<root>";
                    }
                    sXML += "<DT><OUTER_BOXID>" + dtCell.Rows[row]["OUTER_BOXID"] + "</OUTER_BOXID><SUBLOTID>" + dtCell.Rows[row]["SUBLOTID"] + "</SUBLOTID></DT>";
                    if ((row + 1) % 250 == 0 || row + 1 == dtCell.Rows.Count)
                    {
                        sXML += "</root>";

                        newRow = inSubLot.NewRow();
                        newRow["SUBLOTID"] = sXML;
                        inSubLot.Rows.Add(newRow);
                    }
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SORTING_CELL_INS", "INDATA,INSUBLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        HiddenLoadingIndicator();

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1275");
                        Util.gridClear(dgCell);
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 닫기 버튼 이벤트 : btnClose_Click()

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region 사용자 조회 및 등록 : txtRequester_KeyDown(), btnRequester_Click(), wndFindUser_Closed()

        /// <summary>
        /// Text Box KeyDown 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtRequester_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FindUserWindow();
            }
        }

        /// <summary>
        /// 사용자 찾기 팝업 호출
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRequester_Click(object sender, RoutedEventArgs e)
        {
            FindUserWindow();
        }

        /// <summary>
        /// 사용자 찾기 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wndFindUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRequester.Text = wndPerson.USERNAME;
                txtRequester.Tag = wndPerson.USERID;
            }
        }


        #endregion

        #endregion

        #region Method

        #region 사용자 찾기 팝업 : FindUserWindow()
        /// <summary>
        /// 사용자 찾기 팝업
        /// </summary>
        private void FindUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtRequester.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndFindUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        #endregion


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

        #endregion

        private void dgCell_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            dgCell.EndEdit();
            dgCell.EndEditRow(true);

            string sBoxID = Util.NVC(dgCell.GetCell(e.Cell.Row.Index, dgCell.Columns["OUTER_BOXID"].Index)?.Value);

            for (int i = 0; i < dgCell.GetRowCount(); i++)
            {
                if (i == e.Cell.Row.Index || dgCell.GetCell(i, dgCell.Columns["OUTER_BOXID"].Index).Value == null)
                    continue;
                if (dgCell.GetCell(i, dgCell.Columns["OUTER_BOXID"].Index).Value.Equals(sBoxID))
                {
                    //이미 조회된 Pallet ID 입니다.
                    Util.MessageValidation("SFU3165");
                    return;
                }
            }

            DataTable dtRslt = GetPalletInfo(sBoxID);

            if (dtRslt.Rows.Count < 1)
            {
                //입력한 Pallet가 존재하지 않습니다. 
                Util.MessageValidation("SFU3394");

                if (e.Cell.Row.DataItem != null)
                {
                    DataTableConverter.SetValue(dgCell.CurrentRow.DataItem, "OUTER_BOXID", string.Empty);
                    DataTableConverter.SetValue(dgCell.CurrentRow.DataItem, "PLLT_BCD_ID", string.Empty);
                    DataTableConverter.SetValue(dgCell.CurrentRow.DataItem, "SUBLOTID", string.Empty);
                }
                return;
            }
            try
            {
                if (e.Cell.Row.DataItem != null)
                {
                    DataTableConverter.SetValue(dgCell.CurrentRow.DataItem, "PLLT_BCD_ID", dtRslt.Rows[0]["PLLT_BCD_ID"].ToString());
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            dgCell.UpdateLayout();

        }
        /// <summary>
        /// 팔레트ID -> BOXID
        /// </summary>
        /// <param name="sBoxID"></param>
        /// <returns></returns>
        private DataTable GetPalletInfo(string sBoxID)
        {
            try
            {
                DataTable dtResult = new DataTable();

                if (sBoxID.Trim().Equals(""))
                {
                    dtResult.Clear();
                    return dtResult;
                }
                DataTable inTable = new DataTable();
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("BOXTYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["BOXID"] = sBoxID;
                dr["BOXTYPE"] = "PLT";
                inTable.Rows.Add(dr);

                dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", inTable);

                if (dtResult?.Rows?.Count > 0 && dtResult.Rows[0]["BOXTYPE"].Equals("PLT"))
                {
                    return dtResult;
                }

                dtResult.Clear();
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 팔레트 바코드 표시 여부
        /// </summary>
        /// <param name="sAreaID"></param>
        private void isVisibleBCD(string sAreaID)
        {
            // 팔레트 바코드 표시 설정
            if (_Util.IsCommonCodeUseAttr("CELL_PLT_BCD_USE_AREA", sAreaID))
            {
                if (dgCell.Columns.Contains("PLLT_BCD_ID"))
                    dgCell.Columns["PLLT_BCD_ID"].Visibility = Visibility.Visible;
            }
            else
            {
                if (dgCell.Columns.Contains("PLLT_BCD_ID"))
                    dgCell.Columns["PLLT_BCD_ID"].Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// 팔레트바코드ID -> BoxID
        /// </summary>
        private string getPalletInfo(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("BOXTYPE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = palletid;
                dr["BOXTYPE"] = "PLT";

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CP", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["PLLT_BCD_ID"]);
                }
                return "";
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return "";
            }
        }
    }
}
