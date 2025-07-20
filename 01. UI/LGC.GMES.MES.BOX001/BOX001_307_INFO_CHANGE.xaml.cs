/*************************************************************************************
 Created Date : 2024.01.02
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.01.02  DEVELOPER : Initial Created. BOX001_015_INFO_CHANGE COPY.
  2024.01.03  최경아 : 출하처 변경 저장 시 BR_PRD_REG_CHANGE_SHIPTO_CP -> BR_SET_CHANGE_SHIPTO_CP 변경
  2024.08.01  임정훈 : UNCODE 변경 로직 추가(E20240731-000840)
**************************************************************************************/

using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;


namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_307_INFO_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo _combo = new CMM001.Class.CommonCombo();

        string sCHG_TYPE = string.Empty;
        string sSHOPID = string.Empty;
        string sAreaID = string.Empty;

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

        CheckBox chkAll = new CheckBox()
        {
            Content = "",
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public BOX001_307_INFO_CHANGE()
        {
            InitializeComponent();
            Loaded += BOX001_307_INFO_CHANGE_Loaded;
        }

        private void BOX001_307_INFO_CHANGE_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sCHG_TYPE = tmps[0] as string;
            sSHOPID = tmps[1] as string;
            DataTable dtPallet = tmps[2] as DataTable;
            sAreaID = tmps[3] as string;
            //dgPalletInfo.ItemsSource = DataTableConverter.Convert(dtPallet);

            dtpShipDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            if (sCHG_TYPE == "SHIPDATE")
            {
                lblNewShipDate.Visibility = Visibility.Visible;
                dtpShipDate.Visibility = Visibility.Visible;
                lblPalletID.Visibility = Visibility.Visible;
                txtPalletid.Visibility = Visibility.Visible;
                lblNewPackOut_Go.Visibility = Visibility.Hidden;
                cboPackOut_Go.Visibility = Visibility.Hidden;
                lblUnCode.Visibility = Visibility.Hidden;
                cboUnCode.Visibility = Visibility.Hidden;
                lblPalletID2.Visibility = Visibility.Hidden;
                txtPalletid2.Visibility = Visibility.Hidden;
            }
            else if (sCHG_TYPE == "PACKOUT_GO" || sCHG_TYPE == "SHIPPING_PACK")
            {
                lblNewShipDate.Visibility = Visibility.Hidden;
                dtpShipDate.Visibility = Visibility.Hidden;
                lblPalletID.Visibility = Visibility.Visible;
                txtPalletid.Visibility = Visibility.Visible;
                lblNewPackOut_Go.Visibility = Visibility.Visible;
                cboPackOut_Go.Visibility = Visibility.Visible;
                lblUnCode.Visibility = Visibility.Hidden;
                cboUnCode.Visibility = Visibility.Hidden;
                lblPalletID2.Visibility = Visibility.Hidden;
                txtPalletid2.Visibility = Visibility.Hidden;

                //출하처 Combo Set.
                string[] sFilter3 = { sSHOPID, null, sAreaID };
                _combo.SetCombo(cboPackOut_Go, CommonCombo.ComboStatus.SELECT, sFilter: sFilter3, sCase: "SHIPTO_CP");
            }
            else if (sCHG_TYPE == "UNCODE")
            {
                lblUnCode.Visibility = Visibility.Visible;
                //cboUnCode.Visibility = Visibility.Visible;
                cboUnCode.Visibility = Visibility.Hidden;
                txtUnCode.Visibility = Visibility.Visible;
                lblPalletID2.Visibility = Visibility.Visible;
                txtPalletid2.Visibility = Visibility.Visible;
                lblNewShipDate.Visibility = Visibility.Hidden;
                dtpShipDate.Visibility = Visibility.Hidden;
                lblPalletID.Visibility = Visibility.Hidden;
                txtPalletid.Visibility = Visibility.Hidden;
                lblNewPackOut_Go.Visibility = Visibility.Hidden;
                cboPackOut_Go.Visibility = Visibility.Hidden;
            }

            txtSelQty.Value = 0;
            txtPalletQty.Value = dgPalletInfo.GetRowCount();

        }


        #endregion

        #region Initialize

        #endregion

        #region Event

        private void dgPalletInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgPalletInfo.CurrentRow == null || dgPalletInfo.SelectedIndex == -1)
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left") && dgPalletInfo.CurrentColumn.Name == "CHK")
                {
                    if (Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(dgPalletInfo.CurrentRow.Index, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgPalletInfo.Rows[dgPalletInfo.CurrentRow.Index].DataItem, "CHK", true);
                    }
                }

                Chkcbcnt();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                dgPalletInfo.CurrentRow = null;
            }
        }

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            dgPalletInfo.ItemsSource = null;
            txtSelQty.Value = 0;
            txtPalletQty.Value = 0;
            txtPalletid.Text = "";
            if(cboUnCode != null) { cboUnCode.Clear(); cboUnCode.Text = string.Empty; }
        }

        private void btnPltExcel_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = System.Windows.Input.Cursors.Wait;
            OpenExcel();
            this.Cursor = System.Windows.Input.Cursors.Arrow;
            txtPalletQty.Value = dgPalletInfo.GetRowCount();
            Chkcbcnt();
        }

        private void txtPalletid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtPalletid.Text.Trim() != "")
                {
                    DataTable dtPALLET = new DataTable();
                    dtPALLET.Columns.Add("PALLETID", typeof(string));
                    dtPALLET.Columns.Add("AREAID", typeof(string));
                    DataRow inData = dtPALLET.NewRow();
                    inData["PALLETID"] = txtPalletid.Text.Trim();
                    //inData["AREAID"] = sAreaID;
                    dtPALLET.Rows.Add(inData);
                    // Pallet 정보 조회 후 추가...
                    GetPallet_Info(dtPALLET);
                    txtPalletQty.Value = dgPalletInfo.GetRowCount();
                    txtPalletid.SelectAll();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                bool bChk = false;
                switch (sCHG_TYPE)
                {
                    case "SHIPDATE":
                        bChk = Save2ShipDate();
                        break;
                    case "PACKOUT_GO":
                        bChk = Save2Pack_OutGo();
                        break;
                    case "SHIPPING_PACK":
                        bChk = SaveShippingPack();
                        break;
                    case "UNCODE":
                        SaveUnCode();
                        break;
                }

                //if (bChk)
                //{
                //    //this.DialogResult = MessageBoxResult.OK;
                //    //Close();
                //}

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }

        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }





        #endregion


        #region Mehod

        private void OpenExcel()
        {

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
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];
                //XLSheet sheet = null;

                if (sheet == null)
                {
                    Util.AlertInfo("sheet not exists!"); //"Sheet not exists!"
                    return;
                }

                // extract data
                DataTable dataTable = new DataTable();
                Int32 colCnt = 0;
                for (int colInx = 0; colInx < sheet.Columns.Count; colInx++)
                {
                    //col width setting
                    if (sheet.GetCell(0, colInx) != null && !sheet.GetCell(0, colInx).Text.Equals(""))
                    {
                        dataTable.Columns.Add("C" + colInx, typeof(string));
                        colCnt++;
                    }
                }

                Int32 rowCnt = 0;
                for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                {
                    if (sheet.GetCell(rowInx, 0) != null && !sheet.GetCell(rowInx, 0).Text.Equals(""))
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int colInx = 0; colInx < colCnt; colInx++)
                        {
                            XLCell cell = sheet.GetCell(rowInx, colInx);
                            Point cellPoint = new Point(rowInx, colInx);

                            XLRow row = sheet.Rows[1];

                            if (cell != null)
                            {
                                dataRow["C" + colInx] = cell.Text;
                                rowCnt++;
                            }

                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }


                if (dataTable == null)
                {
                    Util.MessageInfo("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                    return;
                }

                GetPallet_Info(dataTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetPallet_Info(DataTable dtResult)
        {
            try
            {
                string sProdid = string.Empty;
                string sLineid = string.Empty;

                int iData_Cnt = dtResult.Rows.Count;
                if (iData_Cnt < 1)
                {
                    Util.MessageInfo("SFU1331"); //"Data가 없습니다" >>Data가 존재하지 않습니다.
                    return;
                }


                for (int i = 0; i < iData_Cnt; i++)
                {

                    string sPalletid = dtResult.Columns.Count >= 1 ? Util.NVC(dtResult.Rows[i][0]) : string.Empty;                   
                    if (sPalletid.Equals(""))
                    {                        
                        int nCount;
                        string sCount;
                        nCount = i + 1;
                        sCount = Convert.ToString(nCount);
                        //Util.AlertInfo("PALLET 정보 중 [" + (i + 1) + "] 행의 PALLET ID가 공백 입니다. \r\n확인 후 다시 하십시오");
                        Util.MessageValidation("SFU1409", new object[] { sCount }); //"PALLET 정보 중 {0} 행의 PALLET ID가 공백 입니다. \r\n확인 후 다시 하십시오."
                        return;
                    }

                    // 중복이면 Skip 처리함..
                    bool bDupYN = false;
                    for (int iRow =0; iRow < dgPalletInfo.GetRowCount(); iRow++)
                    {
                        if (sPalletid == Util.NVC(dgPalletInfo.GetCell(iRow, dgPalletInfo.Columns["PALLETID"].Index).Value))
                        {

                            bDupYN = true;
                            break;
                        }
                    }

                    if (bDupYN)
                    {
                        continue;
                    }

                    // Pallet 정보 조회

                    DataTable dt = Search2Data(sPalletid);

                    if (dt == null)
                        return;
               
                    if (sCHG_TYPE == "UNCODE")
                    {
                        sProdid = dt.Rows[0]["PRODID"].ToString();
                        sLineid = dt.Rows[0]["LINEID"].ToString();

                        if (!string.IsNullOrEmpty(Util.NVC(dt.Rows[0]["RELSID"])))
                        {
                            Util.MessageValidation("SFU3018"); //출고 이력이 존재합니다.
                            return;
                        }

                        //for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                        for (int iRow = 0; iRow < dgPalletInfo.Rows.Count; iRow++)
                        {
                        
                            if (dt.Rows[0]["PRODID"].ToString() != Util.NVC((((DataView)dgPalletInfo.ItemsSource).Table.Rows[iRow]["PRODID"])))//dt.Rows[iRow]["PRODID"])
                            {
                                Util.MessageValidation("SFU1896"); //제품코드가 같은 Pallet만 선택해주세요
                                return;
                            }

                            if (dt.Rows[0]["LINEID"].ToString() != Util.NVC((((DataView)dgPalletInfo.ItemsSource).Table.Rows[iRow]["LINEID"]))) //dt.Rows[iRow]["LINEID"])
                            {
                                Util.MessageValidation("SFU4645"); //동일한 라인이 아닙니다.
                                return;
                            }
                        }
                       
                        //cboUnCode.ItemsSource = DataTableConverter.Convert(GetUnCodeCbo(sProdid, sLineid));
                    }

                    if (dgPalletInfo.GetRowCount() == 0)
                    {
                        dgPalletInfo.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        
                        if (dt.Rows.Count > 0)
                        {
                            //전송정보 로우 수 체크(테이블 결합 루프용)
                            DataTable DTResult = DataTableConverter.Convert(dgPalletInfo.ItemsSource);

                            DataRow drGet = dt.Rows[0];
                            DataRow newDr = DTResult.NewRow();
                            foreach (DataColumn col in dt.Columns)
                            {
                                newDr[col.ColumnName] = drGet[col.ColumnName];
                            }
                            DTResult.Rows.Add(newDr);

                            dgPalletInfo.ItemsSource = DataTableConverter.Convert(DTResult);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private DataTable Search2Data(string sPalletid)
        {
            try
            {
                sPalletid = ConvertBarcodeId(sPalletid);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sPalletid;
                dr["AREAID"] = sAreaID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_INFO_PLT_CP", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count < 1)
                {
                    Util.MessageInfo("SFU1905"); //"조회된 Data가 없습니다."
                    return null;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        /// <summary>
        /// 출하예정일 변경
        /// </summary>
        /// <returns></returns>
        private bool Save2ShipDate()
        {
            try
            {
                string sShipDate = dtpShipDate.SelectedDateTime.ToString("yyyyMMdd"); //Convert.ToDateTime(dtpShipDate.Text).ToString("yyyyMMdd");

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("ISS_SCHD_DATE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["ISS_SCHD_DATE"] = sShipDate;
                inData["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inData);

                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    { 
                        iSelCnt = iSelCnt + 1;
                        DataRow inDataBox = inBoxTable.NewRow();
                        inDataBox["BOXID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        inBoxTable.Rows.Add(inDataBox);
                    }
                }

                if (iSelCnt > 0)
                {
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CHANGE_ISS_SCHD_DATE_CP", "INDATA,INBOX", null, indataSet);
                    Util.MessageInfo("SFU1934"); //"출하 예정일이 변경되었습니다."

                    for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                            || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "SHIPDATE_SCHEDULE", dtpShipDate.SelectedDateTime.ToString("yyyy-MM-dd"));
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;

            }
        }

        /// <summary>
        /// 출하처 변경
        /// </summary>
        /// <returns></returns>
        private bool Save2Pack_OutGo()
        {
            try
            {
                string sPackOut_Go = Util.NVC(cboPackOut_Go.SelectedValue);
                if (sPackOut_Go == "" || sPackOut_Go == "SELECT")
                {
                    Util.MessageValidation("SFU3173");   //출하처를 선택 하십시오
                    return false;
                }


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("TO_SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["SRCTYPE"] = "UI";
                inData["TO_SHIPTO_ID"] = sPackOut_Go;
                inData["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inData);

                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        DataRow inDataBox = inBoxTable.NewRow();
                        inDataBox["BOXID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        inBoxTable.Rows.Add(inDataBox);
                    }
                }

                if (iSelCnt > 0)
                {
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CHANGE_SHIPTO_CP", "INDATA,INBOX", null, indataSet);
                    Util.MessageInfo("SFU1935"); //"출하처가 변경되었습니다."

                    for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                            || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "SHIPTO_NAME", Util.NVC(cboPackOut_Go.Text));
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private bool SaveShippingPack()
        {
            try
            {
                string sPackOut_Go = Util.NVC(cboPackOut_Go.SelectedValue);
                if (sPackOut_Go == "" || sPackOut_Go == "SELECT")
                {
                    Util.MessageValidation("SFU3173");   //출하처를 선택 하십시오
                    return false;
                }

                string sBizRule = string.Empty;

                // 활성화 GMES
                if (LoginInfo.CFG_SYSTEM_TYPE_CODE == "F")
                {
                    sBizRule = "BR_INF_INS_IFS_GMES_RCV_ISS_FOR_PLANT_FORM";
                }
                // 조립 GMES
                else
                {
                    sBizRule = "BR_INF_INS_IFS_GMES_RCV_ISS_FOR_PLANT";
                }


                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("FROM_AREAID", typeof(string));
                inDataTable.Columns.Add("SHIPTO_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("RCV_ISS_ID", typeof(string));
                inBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;
                inData["SHIPTO_ID"] = sPackOut_Go;
                inData["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(inData);

                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        DataRow inDataBox = inBoxTable.NewRow();
                        if (String.IsNullOrEmpty(Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["RELSID"].Index).Value)))
                        {
                            Util.MessageValidation("SFU2963");
                            return false;
                        }

                        // 출고대기 상태 전송 불가.
                        if (Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[i].DataItem, "BOX_RCV_ISS_STAT_CODE")).Equals("SHIP_WAIT"))
                        {
                            Util.MessageValidation("SFU2963");
                            return false;
                        }

                        inDataBox["RCV_ISS_ID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["RELSID"].Index).Value);
                        inDataBox["BOXID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        inBoxTable.Rows.Add(inDataBox);
                    }
                }

                if (iSelCnt > 0)
                {
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX", null, indataSet);
                    Util.MessageInfo("SFU1275"); //"출하처가 변경되었습니다."

                    for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                    {
                        if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                            || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                        {
                            DataTableConverter.SetValue(dgPalletInfo.Rows[i].DataItem, "SHIPTO_NAME", Util.NVC(cboPackOut_Go.Text));
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        // 바코드 ID ==> Pallet ID 입력 변환
        private string ConvertBarcodeId(string lotId)
        {
            DataTable dtRqst = new DataTable();
            dtRqst.Columns.Add("CSTID", typeof(string));

            DataRow drRqst = dtRqst.NewRow();
            drRqst["CSTID"] = lotId;
            dtRqst.Rows.Add(drRqst);

            DataTable dtPallet = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", dtRqst);
            if (dtPallet != null && dtPallet.Rows.Count > 0)
            {
                return Util.NVC(dtPallet.Rows[0]["CURR_LOTID"]);
            }
            return lotId;
        }

        #endregion

        private void dgPalletInfo_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgPalletInfo.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            Chkcbcnt();
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgPalletInfo.GetRowCount(); idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgPalletInfo.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);    
            }

            Chkcbcnt();
            //DataRow[] chkRow = Util.gridGetChecked(ref dgPalletInfo, "CHK");

            //for (int i = 0; i < chkRow.Length; i++)
            //{
            //    int iSelCnt = 0;

            //    if (chkRow[i]["CHK"].Equals(1))
            //    {
            //        iSelCnt = iSelCnt + 1;
            //    }

            //    txtSelQty.Value = iSelCnt;
            //}
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            //DataTable dt = null;
            //삭제하시겠습니까?
          //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
          Util.MessageConfirm("SFU1230", (result) =>
          {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                    DataView dv = (DataView)dgPalletInfo.ItemsSource;
                    DataTable dt = dv.Table;
                    dt.Rows[index].Delete();
                    dgPalletInfo.ItemsSource = DataTableConverter.Convert(dt);
                    txtPalletQty.Value = dgPalletInfo.GetRowCount();
                    Chkcbcnt();
                  //dgPalletInfo.IsReadOnly = false;
                  //dgPalletInfo.RemoveRow(index);
                  //dgPalletInfo.IsReadOnly = true;

              }
            });
        }

        private void txtPalletid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string sArea = string.Empty;

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    DataTable dtPALLET = new DataTable();
                    dtPALLET.Columns.Add("PALLETID", typeof(string));
                    dtPALLET.Columns.Add("AREAID", typeof(string));

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]))
                        { 
                            DataRow inData = dtPALLET.NewRow();
                            inData["PALLETID"] = ConvertBarcodeId(sPasteStrings[i].Trim());
                            dtPALLET.Rows.Add(inData);
                        }
                    }

                    GetPallet_Info(dtPALLET);

                    txtPalletQty.Value = dgPalletInfo.GetRowCount();
                    txtPalletid.SelectAll();                    
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                   // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }

        private void txtPalletid2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtPalletid2.Text.Trim() != "")
                {
                    DataTable dtPALLET = new DataTable();
                    dtPALLET.Columns.Add("PALLETID", typeof(string));
                    DataRow inData = dtPALLET.NewRow();
                    inData["PALLETID"] = txtPalletid2.Text.Trim();
                    dtPALLET.Rows.Add(inData);
                    GetPallet_Info(dtPALLET);
                    txtPalletQty.Value = dgPalletInfo.GetRowCount();
                    txtPalletid.SelectAll();
                }
            }
        }

        private void SaveUnCode()
        {
            try
            {
                string[] sUNCODE = new string[3];

                sUNCODE = GetUncodeUseDate(txtUnCode.Text);
                if (sUNCODE == null)
                {
                    // MMD에 입력되지 않은 UNCODE 입니다. 
                    Util.MessageValidation("SFU8905");
                    return;
                }
                DateTime dtUncodeDate = DateTime.ParseExact(sUNCODE[2], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None); // 유효기간
                if (dtUncodeDate < DateTime.Now)
                {
                    // UNCODE 유효기간을 확인해주세요.
                    Util.MessageValidation("SFU8906");
                    return;
                }                  
            
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("UNCODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("USE_QTY", typeof(string));

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["SRCTYPE"] = "UI";
                inData["UNCODE"] = sUNCODE[0];
                inData["USERID"] = LoginInfo.USERID;
                inData["USE_QTY"] = sUNCODE[1];
                inDataTable.Rows.Add(inData);

                int iSelCnt = 0;
                for (int i = 0; i < dgPalletInfo.GetRowCount(); i++)
                {
                    if (Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value) == "1"
                        || Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["CHK"].Index).Value).ToUpper() == "TRUE")
                    {
                        iSelCnt = iSelCnt + 1;
                        DataRow inDataBox = inBoxTable.NewRow();
                        inDataBox["BOXID"] = Util.NVC(dgPalletInfo.GetCell(i, dgPalletInfo.Columns["PALLETID"].Index).Value);
                        inBoxTable.Rows.Add(inDataBox);
                    }
                }

                if (iSelCnt > 0)
                {
                    if (Convert.ToInt32(sUNCODE[1])-iSelCnt < 0)
                    {
                        // UNCODE 사용 수량 초과하였습니다.
                        Util.MessageValidation("SFU8907");
                        return;
                    }
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_SET_CHANGE_UNCODE_PLLT", "INDATA,INBOX", null, indataSet);
                    Util.MessageInfo("SFU9023"); //"UNCODE가 변경되었습니다."

                }

                
                
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_SET_CHANGE_UNCODE_PLLT", ex.Message, ex.ToString());
            }
        }

        private DataTable GetUnCodeCbo(string sProdid, string sLineid)
        {
            try
            {
            
                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = sProdid;
                dr["EQSGID"] = sLineid;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);
            
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
        private void Chkcbcnt()
        {
            //DataRow[] chkRow = Util.gridGetChecked(ref dgPalletInfo, "CHK");

            //int iSelCnt = 0;
            //if (chkRow != null)
            //{
            //    for (int i = 0; i < chkRow.Length; i++)
            //    {
            //        if (chkRow[i]["CHK"].Equals(1))
            //        {
            //            iSelCnt = iSelCnt + 1;
            //        }
            //    }
            //}

            int iSelCnt = 0;

            for (int lsCount = 0; lsCount < dgPalletInfo.GetRowCount(); lsCount++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[lsCount].DataItem, "CHK")).Equals("1")
                    || Util.NVC(DataTableConverter.GetValue(dgPalletInfo.Rows[lsCount].DataItem, "CHK")).ToUpper().Equals("TRUE"))
                {
                    iSelCnt = iSelCnt + 1;
                }
            }

            txtSelQty.Value = iSelCnt;
            txtPalletQty.Value = dgPalletInfo.GetRowCount();
        }

        private void txtPalletid2_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    string sArea = string.Empty;

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    DataTable dtPALLET = new DataTable();
                    dtPALLET.Columns.Add("PALLETID", typeof(string));
                    dtPALLET.Columns.Add("AREAID", typeof(string));

                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            DataRow inData = dtPALLET.NewRow();
                            inData["PALLETID"] = ConvertBarcodeId(sPasteStrings[i].Trim());
                            dtPALLET.Rows.Add(inData);
                        }
                    }

                    GetPallet_Info(dtPALLET);

                    txtPalletQty.Value = dgPalletInfo.GetRowCount();
                    txtPalletid2.SelectAll();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                    return;
                }

                e.Handled = true;
            }
        }
        private string[] GetUncodeUseDate(string sUNCODE)
        {
            try
            {
                string[] sReturn = new string[3];

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("UN_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["UN_CODE"] = sUNCODE;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNCODE", "RQSTDT", "RSLTDT", RQSTDT);
                if (dtResult.Rows.Count > 0)
                {
                    sReturn[0] = Util.NVC(dtResult.Rows[0]["UN_CODE"]);
                    sReturn[1] = Util.NVC(dtResult.Rows[0]["USE_QTY"]);
                    sReturn[2] = Util.NVC(dtResult.Rows[0]["VLD_PERIOD"]);
                    return sReturn;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }
    }
}
