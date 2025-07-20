/*************************************************************************************
 Created Date : 2018.10.31
      Creator : 
   Decription : 자동차 활성화 후공정 - 외관 불량 양품화 처리
--------------------------------------------------------------------------------------
 [Change History]


 
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
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.UserControls;
using System.Configuration;
using System.IO;
using C1.WPF.Excel;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_507 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private UcPolymerFormShift UcPolymerFormShift { get; set; }

        private Util _Util = new Util();
        private CheckBoxHeaderType _HeaderTypeCell;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        public FORM001_507()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        private void InitializeUserControls()
        {
            if (((System.Windows.FrameworkElement)tabGood.SelectedItem).Name.Equals("ctbGood"))
            {
                txtScanQty.Value = 0;
                Util.gridClear(dgDefectSubLot);
            }
        }
        private void SetControl()
        {
            _HeaderTypeCell = CheckBoxHeaderType.Zero;
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

        }

        #endregion

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            InitializeUserControls();
            SetControl();
            InitCombo();

            txtDefectSubLot.Focus();

            this.Loaded -= UserControl_Loaded;
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtDefectSubLot_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        txtDefectSubLot.Text = sPasteStrings[i].Trim();
                        if (!string.IsNullOrEmpty(txtDefectSubLot.Text))
                            txtDefectSubLot_KeyDown(txtDefectSubLot, null);

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtDefectSubLot_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e == null || e.Key == System.Windows.Input.Key.Enter)
                {
                    string sSubLotID = txtDefectSubLot.Text.Trim();

                    DataTable dtSubLot = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);

                    if (dtSubLot.Rows.Count > 0)
                    {
                        DataRow[] drDup = dtSubLot.Select("SUBLOTID = '" + sSubLotID + "'");

                        if (drDup.Length > 0)
                        {
                            // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                            ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                            {
                                if (result == MessageBoxResult.OK)
                                {
                                    txtDefectSubLot.Focus();
                                    txtDefectSubLot.Text = string.Empty;
                                }
                            });

                            txtDefectSubLot.Text = string.Empty;
                            return;
                        }
                    }

                    // Sublot 정보 조회
                    DataTable RQSTDT = new DataTable("INDATA");
                    RQSTDT.Columns.Add("LANGID");
                    RQSTDT.Columns.Add("SUBLOTID");
                    RQSTDT.Columns.Add("OPERATION");

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["SUBLOTID"] = sSubLotID;
                    dr["OPERATION"] = "G";
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_SUBLOT_AUTO", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null)
                    {
                        if (dtResult.Rows.Count > 0)
                        {
                            if (dtSubLot.Rows.Count > 0)
                            {
                                // 중복 체크
                                DataRow[] drList = dtSubLot.Select("SUBLOTID = '" + dtResult.Rows[0]["SUBLOTID"] + "'");

                                if (drList.Length > 0)
                                {
                                    // SFU3159 아래쪽 List에 이미 존재하는 CELL ID입니다.
                                    ControlsLibrary.MessageBox.ShowNoEnter(MessageDic.Instance.GetMessage("SFU3099"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning, (result) =>
                                    {
                                        if (result == MessageBoxResult.OK)
                                        {
                                            txtDefectSubLot.Focus();
                                            txtDefectSubLot.Text = string.Empty;
                                        }
                                    });

                                    txtDefectSubLot.Text = string.Empty;
                                    return;
                                }

                                // Row 추가
                                dtResult.Merge(dtSubLot);
                            }

                            Util.GridSetData(dgDefectSubLot, dtResult, null, true);
                            txtScanQty.Value = dtResult.Rows.Count;
                        }
                    }

                    txtDefectSubLot.Text = string.Empty;
                    txtDefectSubLot.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageExceptionNoEnter(ex, msgResult =>
                {
                    if (msgResult == MessageBoxResult.OK)
                    {
                        txtDefectSubLot.Text = string.Empty;
                        txtDefectSubLot.Focus();
                    }
                });
            }
            finally
            {
            }
        }
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetHistory();
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetHistory();
            }
        }

        private void tbCheckHeaderAllDefect_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgDefectSubLot;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_HeaderTypeCell)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_HeaderTypeCell)
            {
                case CheckBoxHeaderType.Zero:
                    _HeaderTypeCell = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _HeaderTypeCell = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        /// <summary>
        /// 엑셀 파일 오픈
        /// </summary>
        private void btnOpenExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {
                txtScanQty.Value = 0;

                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        // DataTable dtResult = exl.GetSheetData(str[0]);

                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable inSubLot = new DataTable();
                        inSubLot.Columns.Add("CHK", typeof(Boolean));
                        inSubLot.Columns.Add("SUBLOTID", typeof(string));

                        DataRow newRow = null;

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = inSubLot.NewRow();
                            newRow["CHK"] = 0;
                            newRow["SUBLOTID"] = sheet.GetCell(rowInx, 0).Text;
                            inSubLot.Rows.Add(newRow);
                        }

                        Util.GridSetData(dgDefectSubLot, inSubLot, null, true);
                        txtScanQty.Value = inSubLot.Rows.Count;

                    }
                }
            }
            catch (Exception ex)
            {
                if (exl != null)
                {
                    //이전 연결 해제
                    exl.Conn_Close();
                }
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationDelete())
                return;

            DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);
            dt.Select("CHK = 1").ToList<DataRow>().ForEach(row => row.Delete());
            dt.AcceptChanges();

            Util.GridSetData(dgDefectSubLot, dt, null, true);

            txtScanQty.Value = dt.Rows.Count;
        }

        /// <summary>
        /// 저장 : 양품화
        /// </summary>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationSave())
                return;

            // %1 (을)를 하시겠습니까?
            Util.MessageConfirm("SFU4329", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            }, ObjectDic.Instance.GetObjectName("양품화처리"));
        }

        #endregion


        #region Mehod

        #region [BizCall]

        /// <summary>
        /// 저장 : 양품화
        /// </summary>
        private void SaveProcess()
        {
            try
            {
                // DATA SET 
                DataTable dtGood = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);
                if (dtGood == null || dtGood.Rows.Count == 0)
                {
                    // 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inSubLot = inDataSet.Tables.Add("INSUBLOT");
                inSubLot.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                // XML string 500건당 1개 생성
                int rowCount = 0;
                string sXML = string.Empty;

                for (int row = 0; row < dtGood.Rows.Count; row++)
                {
                    if (row == 0 || row % 500 == 0)
                    {
                        sXML = "<root>";
                    }

                    sXML += "<DT><L>" + dtGood.Rows[row]["SUBLOTID"] + "</L></DT>";

                    if ((row + 1) % 500 == 0 || row + 1 == dtGood.Rows.Count)
                    {
                        sXML += "</root>";

                        newRow = inSubLot.NewRow();
                        newRow["SUBLOTID"] = sXML;
                        inSubLot.Rows.Add(newRow);
                    }

                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_DEFECT_SUBLOT_GOOD_AUTO", "INDATA,INSUBLOT", "OUTDATA", (bizResult, bizException) =>
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

                        // 재조회
                        InitializeUserControls();

                        bizResult.Tables["OUTDATA"].Columns.Add("CHK", typeof(Boolean));
                        Util.GridSetData(dgDefectSubLot, bizResult.Tables["OUTDATA"], null, true);
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
        /// <summary>
        /// 양품화 이력 조회
        /// </summary>
        private void GetHistory()
        {
            try
            {
                if (!ValidationSearch())
                    return;

                if ((dtpDateToHis.SelectedDateTime - dtpDateFromHis.SelectedDateTime).TotalDays > 31)
                {
                    // 기간은 {0}일 이내 입니다.
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }

                //ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;

                dr["SUBLOTID"] = txtCellID.Text == "" ? null : txtCellID.Text;
                dr["BOXID"] = txtPalletID.Text == "" ? null : txtPalletID.Text;
                dr["FROMDATE"] = dtpDateFromHis.SelectedDateTime.ToString("yyyy-MM-dd"); 
                dr["TODATE"] = dtpDateToHis.SelectedDateTime.ToString("yyyy-MM-dd");

                inTable.Rows.Add(dr);

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("DA_PRD_SEL_GOOD_SUBLOT_HIST_AUTO", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgHistory, bizResult, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        private bool ValidationDelete()
        {
            int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectSubLot, "CHK", true);

            if (rowIndex < 0 || dgDefectSubLot.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            //int rowIndex = _Util.GetDataGridFirstRowIndexWithTopRow(dgDefectSubLot, "CHK", true);

            //if (rowIndex < 0 || dgDefectSubLot.GetRowCount() == 0)
            //{
            //    // 선택된 작업대상이 없습니다.
            //    Util.MessageValidation("SFU1645");
            //    return false;
            //}

            if (dgDefectSubLot.GetRowCount() == 0)
            {
                // 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }


            // 중복 CELL 체크
            DataTable dt = DataTableConverter.Convert(dgDefectSubLot.ItemsSource);
            var dupSublot = dt.AsEnumerable().GroupBy(x => x["SUBLOTID"]).Where(x => x.Count() > 1);

            if (dupSublot.Count() > 0)
            {
                foreach (var data in dupSublot)
                {
                    // 중복 데이터가 존재 합니다. % 1
                    Util.MessageValidation("SFU2051", data.Key);
                    return false;
                }
            }

            return true;
        }

        private bool ValidationSearch()
        {
            if(cboArea.SelectedValue.ToString() == "SELECT" || cboArea.SelectedValue.ToString() == "" )
            {
                //동을 선택하세요.
                Util.MessageInfo("SFU1499");
                return false;
            }

            return true;
        }

        #endregion

        #region [Func]

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

        #endregion

        private void btnSearchHis_Click(object sender, RoutedEventArgs e)
        {
            GetHistory();
        }


    }
}
