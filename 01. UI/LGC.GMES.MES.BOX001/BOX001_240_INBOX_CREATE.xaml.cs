/*************************************************************************************
 Created Date : 2024.08.02
      Creator : 
   Decription : INBOX 생성
--------------------------------------------------------------------------------------
 [Change History]
 2024.08.02  이병윤    E20240606-0001246 최초 생성
 **************************************************************************************/

using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Microsoft.Win32;

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;



using System.Windows.Media;
using System.Configuration;
using System.Collections.Generic;




namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_240_INBOX_CREATE : C1Window, IWorkArea
    {
        string sEQSGID = string.Empty;
        string sUSERID = string.Empty;
        bool bVirtual = false; // 가상Lot 여부

        Util _Util = new Util();

        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region [Initialize]
        public BOX001_240_INBOX_CREATE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sEQSGID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);

            InitControl();
        }


        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
        }
        #endregion [Initialize]

        #region [EVENT]

        #region 텍스트박스 포커스 
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region [CELL ID] 입력
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sCellID = Util.NVC(txtCellID.Text);

                if (string.IsNullOrEmpty(sCellID))
                {
                    // CELL IS NOT EXIST
                    Util.MessageValidation("SFU9921");
                    return;
                }

                // 단순 엑셀 130개 업로드
                int iRowCnt = dgInbox.GetRowCount();

                // 최대 130초과 할 수 없습니다.
                if (iRowCnt > 129)   //헤더는 데이터 개수에서 제외
                {
                    // CELL 업로드 오류입니다. EXCEL을 다시 확인해 주세요.
                    Util.MessageValidation("SFU9922");
                    return;
                }


                DataTable dtSource = DataTableConverter.Convert(dgInbox.ItemsSource);
                
                var query = (from t in dtSource.AsEnumerable()
                             where t.Field<string>("SUBLOTID") == sCellID
                             select t.Field<string>("SUBLOTID")).ToList();
                if (query.Any())
                {
                    Util.MessageInfo("SFU9919"); // Duplicate Cell ID/point
                    return;

                }
                DataTable dtTempTag = new DataTable();
                dtTempTag.Columns.Add("SUBLOTID", typeof(string));
                dtTempTag.Columns.Add("SUBLOT_PSTN_NO", typeof(string));
                dtTempTag.Columns.Add("SEQ", typeof(string));

                DataRow dr = dtTempTag.NewRow();
                dr["SUBLOTID"] = Util.NVC(sCellID);
                dr["SUBLOT_PSTN_NO"] = "";
                dr["SEQ"] = iRowCnt + 1;

                //추가
                dtTempTag.Rows.Add(dr);

                // Cell 조회
                AddCell(dtTempTag);
            }

            txtCellID.Focus();
            txtCellID.Text = string.Empty;

        }

        private void txtCellID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {

                    string[] stringSeparators = new string[] { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    // 단순 엑셀 130개 업로드
                    int iRowCnt = dgInbox.GetRowCount();

                    // 최대 130초과 할 수 없습니다.
                    if (iRowCnt + sPasteStrings.Count() > 129)   //헤더는 데이터 개수에서 제외
                    {
                        Util.MessageValidation("SFU9922");
                        return;
                    }

                    // HEAD 제외
                    DataTable dtTempTag = new DataTable();
                    dtTempTag.Columns.Add("SUBLOTID", typeof(string));
                    dtTempTag.Columns.Add("SUBLOT_PSTN_NO", typeof(string));
                    dtTempTag.Columns.Add("SEQ", typeof(string));
                    int iSeq = iRowCnt + 1;
                    for (int i = 0; i < sPasteStrings.Length; i++)
                    {
                        if (string.IsNullOrEmpty(sPasteStrings[i]))
                        {
                            // CELL IS NOT EXIST
                            Util.MessageValidation("SFU9921");
                            return;
                        }

                        DataRow dr = dtTempTag.NewRow();
                        dr["SUBLOTID"] = Util.NVC(sPasteStrings[i]);
                        dr["SUBLOT_PSTN_NO"] = "";
                        dr["SEQ"] = iSeq + i;
                        dtTempTag.Rows.Add(dr);

                        System.Windows.Forms.Application.DoEvents();

                    }

                    // IF : CELL ID/포인트 중복 체크
                    DataTable dtSrc = DataTableConverter.Convert(dgInbox.ItemsSource);
                    dtSrc.Merge(dtTempTag);
                    var query = dtSrc.AsEnumerable()
                                .Select(row => new {
                                    SUBLOTID = row.Field<string>("SUBLOTID")
                                    })
                               .Distinct().Count();

                    if (dtSrc.Rows.Count != query)
                    {
                        // Duplicate Cell ID/point
                        Util.MessageValidation("SFU9919");
                        return;
                    }

                    // Cell 조회
                    AddCell(dtTempTag);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                    return;
                }
            }

            e.Handled = true;
            txtCellID.Focus();
            txtCellID.Text = string.Empty;

        }
        #endregion

        #region [Excel 등록] 버튼 클릭
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dgInbox.ItemsSource);

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

                        // 단순 엑셀 130개 업로드
                        int iRowCnt = dgInbox.GetRowCount();

                        // 최대 130초과 할 수 없습니다.
                        if (iRowCnt + sheet.Rows.Count > 130)   //헤더는 데이터 개수에서 제외
                        {
                            // CELL 업로드 오류입니다. EXCEL을 다시 확인해 주세요.
                            //Util.MessageValidation("SFU9918", $"The number of cells > 130");
                            string SFU9922 = MessageDic.Instance.GetMessage("SFU9922");
                            Util.MessageValidation("SFU9918", SFU9922);
                            return;
                        }

                        // HEAD 제외
                        DataTable dtTempTag = new DataTable();
                        dtTempTag.Columns.Add("SUBLOTID", typeof(string));
                        dtTempTag.Columns.Add("SUBLOT_PSTN_NO", typeof(string));
                        dtTempTag.Columns.Add("SEQ", typeof(string));
                        for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (string.IsNullOrEmpty(sheet.GetCell(rowInx, 0).Text) || string.IsNullOrEmpty(sheet.GetCell(rowInx, 1).Text))
                            {
                                // CELL 업로드 오류입니다. EXCEL을 다시 확인해 주세요.
                                //Util.MessageValidation("SFU9918", "CELL ID not exist");
                                string SFU9921 = MessageDic.Instance.GetMessage("SFU9921");
                                Util.MessageValidation("SFU9918", SFU9921+"["+ rowInx + "]");
                                return;
                            }

                            DataRow dr = dtTempTag.NewRow();
                            dr["SUBLOTID"] = Util.NVC(sheet.GetCell(rowInx, 0).Text);
                            dr["SUBLOT_PSTN_NO"] = Util.NVC(sheet.GetCell(rowInx, 1).Text);
                            dr["SEQ"] = iRowCnt + rowInx;

                            //iRowCnt
                            dtTempTag.Rows.Add(dr);
                        }

                        // IF : CELL ID/포인트 중복 체크
                        DataTable dtSrc = DataTableConverter.Convert(dgInbox.ItemsSource);
                        dtSrc.Merge(dtTempTag);
                        var query = dtSrc.AsEnumerable()
                                    .Select(row => new {
                                        SUBLOTID = row.Field<string>("SUBLOTID")
                                    })
                                   .Distinct().Count();

                        if (dtSrc.Rows.Count != query)
                        {
                            // CELL 업로드 오류입니다. EXCEL을 다시 확인해 주세요.
                            //Util.MessageValidation("SFU9918", "Duplicate Cell ID/point");
                            string SFU9919 = MessageDic.Instance.GetMessage("SFU9919");
                            Util.MessageValidation("SFU9918", SFU9919);
                            return;
                        }

                        // Cell 조회
                        AddCell(dtTempTag);

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [엑셀 양식 다운] 버튼 클릭
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
                od.FileName = "Inbox_Create_Cell_Upload_Sample.xlsx";

                if (od.ShowDialog() == true)
                {
                    C1XLBook c1XLBook1 = new C1XLBook();
                    XLSheet sheet = c1XLBook1.Sheets[0];
                    XLStyle styel = new XLStyle(c1XLBook1);
                    styel.AlignHorz = XLAlignHorzEnum.Center;

                    sheet[0, 0].Value = "CELLID";

                    sheet[0, 1].Value = "SUBLOT_PSTN_NO";

                    sheet[0, 0].Style = styel;
                    sheet.Columns[0].Width = 1500;

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

        #region [초기화] 버튼 클릭
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (dgInbox.GetRowCount() > 0)
            {
                //SFU3440 : 초기화 하시겠습니까?
                Util.MessageConfirm("SFU3440", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        reSet();

                    }
                });
                return;
            }
        }
        #endregion

        #region [생성] 버튼 클릭
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 인터락
                if (!ValidationINBOX_Create())
                {
                    return;
                }

                if (dgInbox.GetRowCount() < 130)
                {
                    //SFU9929 : 초기화 하시겠습니까?
                    Util.MessageConfirm("SFU9929", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            // INBOX 생성
                            CreateInbox();
                        }
                    });
                    return;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [발행] 버튼 클릭
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    reSet();
                    return;
                }

                DataTable dtLog = new DataTable();
                dtLog.Columns.Add("BOXID", typeof(string));
                dtLog.Columns.Add("LABEL_CODE", typeof(string));
                dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
                dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                dtLog.Columns.Add("PRT_ITEM01", typeof(string));
                dtLog.Columns.Add("PRT_ITEM02", typeof(string));
                dtLog.Columns.Add("PRT_ITEM03", typeof(string));
                dtLog.Columns.Add("PRT_ITEM04", typeof(string));
                dtLog.Columns.Add("INSUSER", typeof(string));

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
                

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);
                dtRqst.Rows[0]["LBCD"] = "LBL0367";
                dtRqst.Rows[0]["PRMK"] = sPrt;
                dtRqst.Rows[0]["RESO"] = sRes;
                dtRqst.Rows[0]["PRCN"] = txtInputQty.Value ;
                dtRqst.Rows[0]["MARH"] = sXpos;
                dtRqst.Rows[0]["MARV"] = sYpos;
                dtRqst.Rows[0]["ATTVAL001"] = txtBoxID.Text;
                dtRqst.Rows[0]["ATTVAL002"] = txtBoxID.Text;
                dtRqst.Rows[0]["ATTVAL003"] = txtLotID.Text;
                dtRqst.Rows[0]["ATTVAL004"] = txtGrade.Text;

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                if (PrintLabel(dtRslt.Rows[0]["LABELCD"].ToString(), drPrtInfo))
                {
                    // 라벨 출력 후 로그 저장
                    DataRow drLog = dtLog.NewRow();

                    drLog["BOXID"] = Util.NVC(txtBoxID.Text);
                    drLog["LABEL_CODE"] = dtRqst.Rows[0]["LBCD"];
                    drLog["LABEL_ZPL_CNTT"] = dtRslt.Rows[0]["LABELCD"].ToString();
                    drLog["LABEL_PRT_COUNT"] = dtRqst.Rows[0]["PRCN"];
                    drLog["PRT_ITEM01"] = dtRqst.Rows[0]["ATTVAL001"];
                    drLog["PRT_ITEM02"] = dtRqst.Rows[0]["ATTVAL002"];
                    drLog["PRT_ITEM03"] = dtRqst.Rows[0]["ATTVAL003"];
                    drLog["PRT_ITEM04"] = dtRqst.Rows[0]["ATTVAL004"];
                    drLog["INSUSER"] = sUSERID;

                    dtLog.Rows.Add(drLog);

                    DataTable dtRsltLog = new ClientProxy().ExecuteServiceSync("DA_PRD_INS_LABEL_HIST", "INDATA", null, dtLog);
                }
                reSet();
            }
            catch (Exception ex)
            {
                reSet();
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion [EVENT]


        #region [Method]

        private bool ValidationINBOX_Create()
        {
            // Whether the Grader level is consistent
            DataTable dtSrc = DataTableConverter.Convert(dgInbox.ItemsSource);
            var grader = dtSrc.AsEnumerable()
                        .Select(row => new {
                            GRADE = row.Field<string>("GRADE")
                        })
                       .Distinct().Count();

            if(grader > 1)
            {
                Util.MessageValidation("SFU9923");
                return false;
            }

            // Whether the model is consistent
            var model = dtSrc.AsEnumerable()
                        .Select(row => new {
                            PRODID = row.Field<string>("PRODID")
                        })
                       .Distinct().Count();
            if (model > 1)
            {
                Util.MessageValidation("SFU9924");
                return false;
            }

            // Whether the production line is consistent (LOT ID 8th bit); PROD_LINE
            var prod = dtSrc.AsEnumerable()
                        .Select(row => new {
                            PROD_LINE = row.Field<string>("PROD_LINE")
                        })
                       .Distinct().Count();
            if(prod > 1)
            {
                Util.MessageValidation("SFU9925");
                return false;
            }

            // Whether the year and month of production of LOT are consistent (LOT ID 4~5 digits);
            var yymm = dtSrc.AsEnumerable()
                        .Select(row => new {
                            YYYYMM = row.Field<string>("YYYYMM")
                        })
                       .Distinct().Count();
            if(yymm > 1)
            {
                Util.MessageValidation("SFU9926");
                return false;
            }

            // Whether the production LOT span is within 7 days (LOT ID 6~7 digits);
            List<int> list = new List<int>();
            for (int r = 0; r < dtSrc.Rows.Count; r++)
            {
                DataRow row = dtSrc.Rows[r];
                string sDd = (String)row["DAY"];
                int nDD = Util.NVC_Int(sDd);
                list.Add(nDD);
            }

            int dMax = list.Max();
            int dMin = list.Min();

            if(dMax - dMin > 6)
            {
                Util.MessageValidation("SFU9927");
                return false;
            }

            // Whether the production LOT quantity is ≤5ea;
            var lotCnt = dtSrc.AsEnumerable()
                        .Select(row => new {
                            LOTID = row.Field<string>("LOTID")
                        })
                       .Distinct().Count();
            if(lotCnt > 5)
            {
                Util.MessageValidation("SFU9928");
                return false;
            }
            if(lotCnt > 1)
            {
                bVirtual = true;
            }
            else
            {
                bVirtual = false;
            }

            return true;
        }

        #region 매칭 Cell 체크 후 Grid에 Cell 추가
        private void AddCell(DataTable dtCell)
        {            
            try
            {
                ShowLoadingIndicator();
                DataSet inDataSet = new DataSet();
                DataTable dtIndata = inDataSet.Tables.Add("INDATA");
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("IFMODE", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQSGID"] = sEQSGID;
                dr["USERID"] = sUSERID;

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = inDataSet.Tables.Add("IN_SUBLOT");
                dtInbox.Columns.Add("SUBLOTID");
                dtInbox.Columns.Add("SUBLOT_PSTN_NO");
                dtInbox.Columns.Add("SEQ");
                dtInbox.Clear();
                dtInbox.Merge(dtCell);

                new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_CELL_DATA_M50L_NJ", "INDATA,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            DataTable dtSrc = DataTableConverter.Convert(dgInbox.ItemsSource);
                            dtSrc.Merge(bizResult.Tables["OUTDATA"]);
                            Util.GridSetData(dgInbox, dtSrc, FrameOperation, false);
                            
                        }
                        HiddenLoadingIndicator();
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

        #region 매칭 Cell 체크 후 Grid에 Cell 추가
        private void CreateInbox()
        {
            try
            {
                ShowLoadingIndicator();
                DataSet inDataSet = new DataSet();
                DataTable dtIndata = inDataSet.Tables.Add("IN_EQPT");
                dtIndata.Columns.Add("SRCTYPE", typeof(string));
                dtIndata.Columns.Add("IFMODE", typeof(string));
                dtIndata.Columns.Add("EQSGID", typeof(string));
                dtIndata.Columns.Add("USERID", typeof(string));

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["EQSGID"] = sEQSGID;
                dr["USERID"] = sUSERID;

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = inDataSet.Tables.Add("IN_SUBLOT");
                dtInbox.Columns.Add("SUBLOTID");
                dtInbox.Columns.Add("SUBLOT_PSTN_NO");
                dtInbox.Clear();
                DataTable dtSrc = DataTableConverter.Convert(dgInbox.ItemsSource);
                dtInbox.Merge(dtSrc);
                HiddenLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INBOX_M50L_UI_NJ", "IN_EQPT,IN_SUBLOT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            HiddenLoadingIndicator();
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            HiddenLoadingIndicator();
                            string inbox = Util.NVC(bizResult.Tables["OUTDATA"].Rows[0]["BOXID"]);
                            txtBoxID.Text = inbox;
                            
                            string lotId = string.Empty;
                            lotId = Util.NVC(dtSrc.Rows[0]["LOTID"]);
                            // 단일 lot 여부
                            if (bVirtual)
                            {
                                // 가상 lot 번호 생성
                                lotId = lotId.Substring(0,5) + "00" + lotId.Substring(7,1) + "N1";
                            }
                            txtLotID.Text = lotId;

                            txtGrade.Text = Util.NVC(dtSrc.Rows[0]["GRADE"]);
                            // 라벨 출력창
                            grCellList.Visibility = Visibility.Visible;
                        }
                        
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

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        //loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }

                //System.Threading.Thread.Sleep(200);
            }
            else
            {
                //loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void reSet()
        {
            Util.gridClear(dgInbox);
            txtCellID.Focus();
            txtCellID.Text = string.Empty;
            grCellList.Visibility = Visibility.Collapsed;
            bVirtual = false;
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
        #endregion [Method]
    }
}
