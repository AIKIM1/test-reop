/*************************************************************************************
 Created Date : 2020.03.10
      Creator : 이제섭
   Decription : CNJ 원형 9 ~ 14라인 증설 Pjt - 자동 포장 구성 (원/각형) - 정보 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.03.10  이제섭 : 최초생성
  2022.06.03  장희만 : C20220516-000309  CELLID 동일하게 OUTBOX 기능 추가 (다중복사, EXCEL Import)
  2024.05.08  이병윤 : E20240320-000992  pallet 검색조건 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Configuration;
using C1.WPF.Excel;
using Microsoft.Win32;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_240_INFO : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CommonCombo();

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        public BOX001_240_INFO()
        {
            InitializeComponent();
            Loaded += BOX001_240_INFO_Loaded;
        }

        private void BOX001_240_INFO_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

   
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void txtsublot_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    ShowLoadingIndicator();

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;

                        }

                        bool isExist = false;

                        if (dgPalletInfo.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgPalletInfo.Rows[idx].DataItem, "SUBLOTID").ToString() == sPasteStrings[i])
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetCellList(sPasteStrings[i]);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void txtsublot_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtsublot.Text.ToString().Trim()))
                    {
                        // CELLID를 스캔 또는 입력하세요.
                        Util.MessageInfo("SFU1323");
                        return;
                    }

                    ShowLoadingIndicator();

                    GetCellList(txtsublot.Text.ToString().Trim());
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtsublot.Text = string.Empty;
                    txtsublot.SelectAll();
                    txtsublot.Focus();

                    HiddenLoadingIndicator();
                }
            }
        }

        private void GetCellList(string pCellID)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["SUBLOTID"] = pCellID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    dtSource.Merge(dtResult);
                    Util.gridClear(dgPalletInfo);
                    Util.GridSetData(dgPalletInfo, dtSource, FrameOperation, true);
                }

                if(dgPalletInfo.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU2951");    //조회결과가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }


        private void GetOutboxList(string pOutboxID)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("OUTBOXID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["OUTBOXID"] = pOutboxID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    dtSource.Merge(dtResult);
                    Util.gridClear(dgPalletInfo);
                    Util.GridSetData(dgPalletInfo, dtSource, FrameOperation, true);
                }

                if (dgPalletInfo.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU2951");    //조회결과가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }


        private void txtinbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(txtinbox.Text.ToString().Trim()))
                    {
                        // Inbox ID를 입력 하세요.
                        Util.MessageInfo("SFU4517");
                        return;
                    }

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("INBOXID", typeof(string));

                    DataRow newRow = inTable.NewRow();

                    newRow["INBOXID"] = string.IsNullOrWhiteSpace(txtinbox.Text.ToString().Trim()) ? null : txtinbox.Text.ToString().Trim();
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, false);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtinbox.Text = string.Empty;
                    txtinbox.SelectAll();
                    txtinbox.Focus();
                }
            }
        }

        private void txtoutbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(txtoutbox.Text.ToString().Trim()))
                    {
                        // OUTBOX를 입력하세요.
                        Util.MessageInfo("SFU5008");
                        return;
                    }

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("OUTBOXID", typeof(string));

                    DataRow newRow = inTable.NewRow();

                    newRow["OUTBOXID"] = string.IsNullOrWhiteSpace(txtoutbox.Text.ToString().Trim()) ? null : txtoutbox.Text.ToString().Trim();
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, false);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtoutbox.Text = string.Empty;
                    txtoutbox.SelectAll();
                    txtoutbox.Focus();
                }
            }
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetSearchData();
        }

        #endregion


        #region Mehod

        private void GetSearchData()
        {

            try
            {
                if (string.IsNullOrWhiteSpace(txtsublot.Text.ToString().Trim()) 
                    && string.IsNullOrWhiteSpace(txtinbox.Text.ToString().Trim()) 
                    && string.IsNullOrWhiteSpace(txtoutbox.Text.ToString().Trim())
                    && string.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()))
                {
                    // 입력 데이터가 존재하지 않습니다.
                    Util.MessageInfo("SFU1801");
                    return;
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("INBOXID", typeof(string));
                inTable.Columns.Add("OUTBOXID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["SUBLOTID"] = string.IsNullOrWhiteSpace(txtsublot.Text.ToString().Trim()) ? null : txtsublot.Text.ToString().Trim();
                newRow["INBOXID"] = string.IsNullOrWhiteSpace(txtinbox.Text.ToString().Trim()) ? null : txtinbox.Text.ToString().Trim();
                newRow["OUTBOXID"] = string.IsNullOrWhiteSpace(txtoutbox.Text.ToString().Trim()) ? null : txtoutbox.Text.ToString().Trim();
                newRow["PALLETID"] = string.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()) ? null : txtPallet.Text.ToString().Trim();
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtsublot.Text = string.Empty;
                txtinbox.Text = string.Empty;
                txtoutbox.Text = string.Empty;
                txtPallet.Text = string.Empty;
                txtsublot.SelectAll();
                txtsublot.Focus();
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

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtExcelData = GetExcelData();

                if (dtExcelData != null)
                {
                    ShowLoadingIndicator();

                    for (int inx = 0; inx < dtExcelData.Rows.Count; inx++)
                    {
                        string sCell = Util.NVC(dtExcelData.Rows[inx][0]);
                        if (string.IsNullOrEmpty(sCell))
                        {
                            return;
                        }

                        bool isExist = false;

                        if (dgPalletInfo.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgPalletInfo.Rows[idx].DataItem, "SUBLOTID").ToString() == sCell)
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetCellList(sCell);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();

                txtsublot.Text = string.Empty;
                txtinbox.Text = string.Empty;
                txtoutbox.Text = string.Empty;
                txtsublot.SelectAll();
                txtsublot.Focus();
            }
        }

        private DataTable GetExcelData()
        {
            DataTable dtExcelData = null;

            try
            {
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
                        dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        
           return dtExcelData;
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgPalletInfo);

            txtsublot.Text = string.Empty;
            txtinbox.Text = string.Empty;
            txtoutbox.Text = string.Empty;
            txtPallet.Text = string.Empty; 
            txtsublot.SelectAll();
            txtsublot.Focus();
        }

        private void txtoutbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    ShowLoadingIndicator();

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;

                        }

                        bool isExist = false;

                        if (dgPalletInfo.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgPalletInfo.Rows[idx].DataItem, "OUTBOX").ToString() == sPasteStrings[i])
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetOutboxList(sPasteStrings[i]);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void btnExcel1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
                DataTable dtExcelData = GetExcelData();

                if (dtExcelData != null)
                {
                    ShowLoadingIndicator();

                    for (int inx = 0; inx < dtExcelData.Rows.Count; inx++)
                    {
                        string sCell = Util.NVC(dtExcelData.Rows[inx][0]);
                        if (string.IsNullOrEmpty(sCell))
                        {
                            return;
                        }

                        bool isExist = false;

                        if (dgPalletInfo.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgPalletInfo.Rows[idx].DataItem, "OUTBOX").ToString() == sCell)
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetOutboxList(sCell);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();

                txtsublot.Text = string.Empty;
                txtinbox.Text = string.Empty;
                txtoutbox.Text = string.Empty;
                txtoutbox.SelectAll();
                txtoutbox.Focus();
            }
        }

        private void txtPallet_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()))
                    {
                        // PALLETID를 입력하세요.
                        Util.MessageValidation("SFU1411");
                        return;
                    }

                    DataTable inTable = new DataTable("RQSTDT");
                    inTable.Columns.Add("PALLETID", typeof(string));

                    DataRow newRow = inTable.NewRow();

                    newRow["PALLETID"] = string.IsNullOrWhiteSpace(txtPallet.Text.ToString().Trim()) ? null : txtPallet.Text.ToString().Trim();
                    inTable.Rows.Add(newRow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                    Util.GridSetData(dgPalletInfo, dtResult, FrameOperation, false);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    txtPallet.Text = string.Empty;
                    txtPallet.SelectAll();
                    txtPallet.Focus();
                }
            }
        }

        private void txtPallet_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Length > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }

                    ShowLoadingIndicator();

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            break;

                        }

                        bool isExist = false;

                        if (dgPalletInfo.GetRowCount() > 0)
                        {
                            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
                            {
                                if (DataTableConverter.GetValue(dgPalletInfo.Rows[idx].DataItem, "PALLET").ToString() == sPasteStrings[i])
                                {
                                    isExist = true;
                                    break;
                                }
                            }
                        }

                        if (isExist == false)
                        {
                            GetPalletList(sPasteStrings[i]);
                        }

                        System.Windows.Forms.Application.DoEvents();
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

                e.Handled = true;
            }
        }

        private void GetPalletList(string pPalletID)
        {
            try
            {
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("PALLETID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["PALLETID"] = pPalletID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PALLET_INFO_NJ", "INDATA", "OUTDATA", inTable);

                if (dtResult.Rows.Count > 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgPalletInfo.ItemsSource);
                    dtSource.Merge(dtResult);
                    Util.gridClear(dgPalletInfo);
                    Util.GridSetData(dgPalletInfo, dtSource, FrameOperation, true);
                }

                if (dgPalletInfo.Rows.Count <= 0)
                {
                    Util.MessageInfo("SFU2951");    //조회결과가 없습니다.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
    }
}
