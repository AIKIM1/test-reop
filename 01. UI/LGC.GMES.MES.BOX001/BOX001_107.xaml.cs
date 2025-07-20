/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이슬아
   Decription : 출고취소
               
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.18  이상훈 : C20180929_04640 Initial Created.
                       다중 취소 기능으로 권한에 따라 처리 할 수 있도록 하기 위해 화면 구성으로 함
  2023.04.05  이병윤 : C20221110-000111_Ctrl C + V로 처리할 수 있게 기능 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;



namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_107 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
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

        public BOX001_107()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            GetEqptWrkInfo();
        }

        #endregion



        #region 기간 Event
        #endregion
        #region Event

        /// <summary>
        /// C20180929_04640 엑셀 upload 이벤트 처리
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcelOpen_Click(object sender, RoutedEventArgs e)
        {
            ExcelMng exl = new ExcelMng();

            try
            {

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

                        if (sheet.GetCell(0, 0).Text != "PALLETID")
                        {
                            Util.MessageValidation("SFU4424");
                            return;
                        }


                        DataTable dtINLOT = getPalletidFileToDataTable();
                        DataRow newRow = null;


                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (sheet.GetCell(rowInx, 0) == null || string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text))
                                break;

                            newRow = dtINLOT.NewRow();

                            newRow["PALLETID"] = sheet.GetCell(rowInx, 0).Text;
                            dtINLOT.Rows.Add(newRow);
                        }

                        setgrListCreatePalletID_DataSet(dtINLOT);
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
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
            }
        }

        /// <summary>
        /// Tray 엑셀파일 양식 다운로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExcelDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog od = new SaveFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    od.InitialDirectory = @"\\Client\C$";
                }
                od.Filter = "Excel Files (.xlsx)|*.xlsx";
                od.FileName = "Shipping_Cancel_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "PALLETID";

                    sheet[0, 0].Style = styel;
                    sheet.Columns[0].Width = 1500;

                    c1XLBook1.Save(od.FileName);

                    //   if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] != "SBC")
                    System.Diagnostics.Process.Start(od.FileName);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtTo = DataTableConverter.Convert(dgPalletID.ItemsSource);
            if (dtTo.Rows.Count > 0)
            {
                DataRow[] drInfo = dtTo.Select("CHK = True");
                foreach (DataRow dr in drInfo)
                {
                    dtTo.Rows.Remove(dr);
                    dgPalletID.ItemsSource = DataTableConverter.Convert(dtTo);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (dgPalletID.GetRowCount() <= 0)
            {
                Util.MessageValidation("SFU3425"); //선택된 Pallet가 없습니다.
                return;
            }
            if (string.IsNullOrEmpty(txtWorker.Text) || string.IsNullOrEmpty(Util.NVC(txtWorker.Tag)))
            {
                // SFU1843 작업자를 입력 해주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (string.IsNullOrEmpty(txtShift.Text) || string.IsNullOrEmpty(Util.NVC(txtShift.Tag)))
            {
                // SFU1844 작업조를 입력 해주세요.
                Util.MessageValidation("SFU1844");
                return;
            }

            DataTable dtCheck = DataTableConverter.Convert(dgPalletID.ItemsSource);
            if (dtCheck.Rows.Count > 0)
            {
                DataRow[] drInfo = dtCheck.Select("isnull(ERROR_MSG,'') <> '' ");
                if (drInfo.Length > 0 )
                {
                    Util.MessageValidation("SFU5045"); //삭제 대상 Pallet ID 가 존재합니다.
                    return;
                }

            }

            //취소 하시겠습니까?
            Util.MessageConfirm("SFU1243", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    CancelShip();
                }
            });
        }

        /// <summary>
        /// C20221110-000111 :
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPalletId_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    ShowLoadingIndicator();

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    //if (sPasteStrings.Count() > 100)
                    //{
                    //    Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                    //    return;
                    //}

                    DataTable dtINLOT = getPalletidFileToDataTable();
                    DataRow newRow = null;

                    DataTable dt = DataTableConverter.Convert(dgPalletID.ItemsSource);
                    dtINLOT.Merge(dt);
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (dt.Rows.Count > 0)
                        {   
                            DataRow[] row = dt.Select("PALLETID = '" + sPasteStrings[i] + "'"); //중복조건 체크
                            if (row == null || row.Length == 0)
                            {
                                newRow = dtINLOT.NewRow();
                                newRow["PALLETID"] = sPasteStrings[i];
                                dtINLOT.Rows.Add(newRow);
                            }
                        }
                        else
                        {
                            newRow = dtINLOT.NewRow();
                            newRow["PALLETID"] = sPasteStrings[i];
                            dtINLOT.Rows.Add(newRow);

                        }
                        
                    }
                    setgrListCreatePalletID_DataSet(dtINLOT);

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

        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {

                    DataTable dtINLOT = getPalletidFileToDataTable();
                    DataRow newRow = null;
                    //리스트에 있는지 체크
                    DataTable dt = DataTableConverter.Convert(dgPalletID.ItemsSource);
                    

                    if (dt.Rows.Count > 0 && dt.Select("PALLETID = '" + txtPalletId.Text + "'").Length > 0) //중복조건 체크
                    {
                        Util.Alert("SFU1781"); //"이미추가된팔레트입니다."
                        return;
                    }
                    dtINLOT.Merge(dt);
                    newRow = dtINLOT.NewRow();
                    newRow["PALLETID"] = txtPalletId.Text;
                    dtINLOT.Rows.Add(newRow);
                    setgrListCreatePalletID_DataSet(dtINLOT);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgPalletID_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
        #endregion

        #region Method

        /// <summary>
        /// 데이터 테이블 셋
        /// </summary>
        /// <returns></returns>
        private DataTable getPalletidFileToDataTable()
        {
            DataTable inLot = new DataTable();
            inLot.Columns.Add("PALLETID", typeof(string));
            return inLot;
        }

        /// <summary>
        /// 엑셀 목록 일괄 데이터 조회 데이터 셋
        /// </summary>
        /// <returns></returns>
        private DataSet getPalletIDFileShechIndataSet()
        {
            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inTable = inData.Tables.Add("INDATA");
            inTable.Columns.Add("LANGID", typeof(string));

            //대상 LOT
            DataTable inLot = inData.Tables.Add("INPALLET");
            inLot.Columns.Add("PALLETID", typeof(string));
            return inData;
        }

        private void setgrListCreatePalletID_DataSet(DataTable dtList)
        {

            try
            {
                if (dtList == null || dtList.Rows.Count <= 0)
                    return;

                DataSet inDataSet = getPalletIDFileShechIndataSet();
                DataTable inTable = inDataSet.Tables["INDATA"];
                string sBIZName = "BR_PRD_CHK_CANCEL_SHIP_FM_MULTI";

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                inTable.Rows.Add(dr);

                DataTable inLot = inDataSet.Tables["INPALLET"];
                DataRow newRow = null;

                for (int rowInx = 0; rowInx < dtList.Rows.Count; rowInx++)
                {
                    if (string.IsNullOrEmpty(dtList.Rows[rowInx]["PALLETID"].ToString()) )
                    {
                        Util.MessageValidation("SFU4979"); //필수입력항목을 모두 입력하십시오.
                        return;
                    }
                    newRow = inLot.NewRow();

                    newRow["PALLETID"] = dtList.Rows[rowInx]["PALLETID"].ToString();
                    inLot.Rows.Add(newRow);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBIZName, "INDATA,INPALLET", "OUTDATA", inDataSet);
                if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"] != null && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                {
                    Util.GridSetData(dgPalletID, dsResult.Tables["OUTDATA"], FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        
        private void CancelShip()
        {
            //if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            //{
            //    // SFU3350  입력오류 : PALLETID 를 입력해 주세요.	
            //    Util.MessageValidation("SFU3350");
            //    return;
            //}

            loadingIndicator.Visibility = Visibility.Visible;

            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID");
            inDataTable.Columns.Add("SHFT_ID");
            inDataTable.Columns.Add("WRK_SUPPLIERID");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("NOTE");

            DataRow inDataRow = inDataTable.NewRow();
            inDataRow["USERID"] = txtWorker.Tag;
            inDataRow["SHFT_ID"] = txtShift.Tag;
            inDataRow["LANGID"] = LoginInfo.LANGID;
            inDataRow["NOTE"] = string.Empty;
            inDataTable.Rows.Add(inDataRow);

            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
            inBoxTable.Columns.Add("BOXID");

            for (int inx = 0; inx < dgPalletID.GetRowCount(); inx++)
            {
                DataRow newRow = inBoxTable.NewRow();
                newRow["BOXID"] = Util.NVC(DataTableConverter.GetValue(dgPalletID.Rows[inx].DataItem, "PALLETID"));
                inBoxTable.Rows.Add(newRow);
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_SHIP_INPALLET_FM", "INDATA,INBOX", null, (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }, indataSet);

        }


        #endregion

        #region [PopUp Event]
        private void btnShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = new CMM_SHIFT_USER2();
            shiftPopup.FrameOperation = this.FrameOperation;

            if (shiftPopup != null)
            {
                object[] Parameters = new object[8];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQSG_ID;
                Parameters[3] = Process.CELL_BOXING;
                Parameters[4] = Util.NVC(txtShift.Tag);
                Parameters[5] = Util.NVC(txtWorker.Tag);
                Parameters[6] = LoginInfo.CFG_EQPT_ID;
                Parameters[7] = "Y"; // 저장 플로그 "Y" 일때만 저장.
                C1WindowExtension.SetParameters(shiftPopup, Parameters);

                shiftPopup.Closed += new EventHandler(shift_Closed);

                // 팝업 화면 숨겨지는 문제 수정.
                //this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
                grdMain.Children.Add(shiftPopup);
                shiftPopup.BringToFront();
            }
        }
        private void shift_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 shiftPopup = sender as CMM_SHIFT_USER2;

            if (shiftPopup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.grdMain.Children.Remove(shiftPopup);
        }

        private void wndShift_Closed(object sender, EventArgs e)
        {
            GMES.MES.CMM001.Popup.CMM_SHIFT_USER window = sender as GMES.MES.CMM001.Popup.CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtWorker.Text = window.USERNAME;
                txtWorker.Tag = window.USERID;
            }
            this.grdMain.Children.Remove(window);
        }
        #endregion

        #region [EQPT_WRK_INFO Event]
        private void GetEqptWrkInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable IndataTable = new DataTable("RQSTDT");
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WIPSTAT", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                Indata["PROCID"] = Process.CELL_BOXING;

                //Indata["LOTID"] = sLotID;
                //Indata["PROCID"] = procId;
                //Indata["WIPSTAT"] = WIPSTATUS;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                            {
                                txtShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                            }
                            else
                            {
                                txtShiftStartTime.Text = string.Empty;
                            }

                            if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                            {
                                txtShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                            }
                            else
                            {
                                txtShiftEndTime.Text = string.Empty;
                            }

                            if (!string.IsNullOrEmpty(txtShiftStartTime.Text) && !string.IsNullOrEmpty(txtShiftEndTime.Text))
                            {
                                txtShiftDateTime.Text = txtShiftStartTime.Text + " ~ " + txtShiftEndTime.Text;
                            }
                            else
                            {
                                txtShiftDateTime.Text = string.Empty;
                            }

                            if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                            {
                                txtWorker.Text = string.Empty;
                                txtWorker.Tag = string.Empty;
                            }
                            else
                            {
                                txtWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                txtWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                            }

                            if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                            {
                                txtShift.Tag = string.Empty;
                                txtShift.Text = string.Empty;
                            }
                            else
                            {
                                txtShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                txtShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                            }
                        }
                        else
                        {
                            txtWorker.Text = string.Empty;
                            txtWorker.Tag = string.Empty;
                            txtShift.Text = string.Empty;
                            txtShift.Tag = string.Empty;
                            txtShiftStartTime.Text = string.Empty;
                            txtShiftEndTime.Text = string.Empty;
                            txtShiftDateTime.Text = string.Empty;
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
                });
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletID.GetRowCount(); i++)
                {
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgPalletID.Rows[i].DataItem, "CHK")))
                        || Util.NVC(DataTableConverter.GetValue(dgPalletID.Rows[i].DataItem, "CHK")).Equals("0")
                        || Util.NVC(DataTableConverter.GetValue(dgPalletID.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgPalletID.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgPalletID.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgPalletID.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        #endregion

        

        
    }
}
