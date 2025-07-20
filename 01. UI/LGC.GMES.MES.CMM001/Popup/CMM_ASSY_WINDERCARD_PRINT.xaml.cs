using System;
using System.Windows;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using C1.C1Preview;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_WINDERCARD_PRINT : C1Window, IWorkArea
    {
        #region Declaration
        DataSet runcard = null;
        private bool _isSmallType;
        private string _lotId;
        private bool _isAutoPrint;
        private string _processCode;
        private string _windingRuncardId; //WINDING_RUNCARD_ID
        C1.C1Report.C1Report cr = null;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public CMM_ASSY_WINDERCARD_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            // Runcard 발행 DataSet
            runcard = tmps[0] as DataSet;
            _isSmallType = (bool) tmps[1];
            _lotId = tmps[2].GetString();
            _processCode = tmps[3].GetString();
            _isAutoPrint = (bool) tmps[4];

            //자동발행시 발행버튼을 없애는 과정
            if (LoginInfo.CFG_CARD_AUTO == "Y" ? true : false)
            {
                if (_isAutoPrint)
                {
                    btnPrint.Visibility = Visibility.Collapsed;
                }
            }

            this.Loaded -= Window_Loaded;

            if (_isSmallType)
            {
                PrintViewSmallType();
            }
            else
            {
                PrintView();
            }


        }
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            btnPrint.IsEnabled = false; //C20180308_28552 연속 클릭 제어
            SetHistoryCardPrintCount();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
            //this.Close();
        }
        #endregion

        #region SizeChanged
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();

        }
        #endregion

        #region User Method
        private void PrintView()
        {
            try
            {
                cr = new C1.C1Report.C1Report {Layout = {PaperSize = System.Drawing.Printing.PaperKind.Custom }};
                
                
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
                //using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.CMM001.Report.Winder_HistoryCard.xml"))
                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.CMM001.Report.Winder_HistoryCard_Ver2.xml"))
                {
                    //cr.Load(stream, "Winder_HistoryCard");
                    cr.Load(stream, "Winder_HistoryCard_Ver2");

                    // 임시 다국어 처리
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        if (cr.Fields[cnt].Name.IndexOf("Text", StringComparison.Ordinal) > -1)
                        {
                            if(string.IsNullOrEmpty(cr.Fields[cnt].Text)) continue;
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }
                    }

                    // 1.OUT_DATA
                    DataTable dt1 = runcard.Tables["OUT_DATA"];
                    for (int col = 0; col < dt1.Columns.Count; col++)
                    {
                        string strColName = dt1.Columns[col].ColumnName;

                        if (cr.Fields.Contains(strColName))
                        {
                            double dValue = 0;
                            int nValue = 0;

                            if (strColName.Equals("INPUT_QTY") || strColName.Equals("DFCT_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");
                            }
                            else if (strColName.Equals("PRD_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["PRD_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.Equals("WIP_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["WIP_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.IndexOf("BARCODE", StringComparison.Ordinal) > -1)
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                                cr.Fields[strColName + "_TXT"].Text = dt1.Rows[0][strColName].ToString();
                            }
                            else
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                                _windingRuncardId = dt1.Rows[0]["WINDING_RUNCARD_ID"].GetString();
                            }

                        }
                    }

                    // 2.OUT_ELEC
                    DataTable dt2 = runcard.Tables["OUT_ELEC"];
                    for (int col = 0; col < dt2.Columns.Count; col++)
                    {
                        string strColName = dt2.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dt2.Rows[0][strColName].ToString();
                    }

                    /*
                    // 3.OUT_DFCT
                    DataTable dt3 = runcard.Tables["OUT_DFCT"];
                    for (int col = 0; col < dt3.Columns.Count; col++)
                    {
                        string strColName = dt3.Columns[col].ColumnName;

                        //for (int row = 0; row < dt3.Rows.Count; row++)
                        for (int row = 0; row < 30; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                cr.Fields[strColName + strRowName].Text = "";

                                if (row < dt3.Rows.Count)
                                {
                                    double dValue = 0;

                                    if (strColName.Equals("DFCT_QTY"))
                                    {
                                        if (double.TryParse(Util.NVC(dt3.Rows[row][strColName]), out dValue))
                                        {
                                            if (dValue > 0)
                                                cr.Fields[strColName + strRowName].Text = " " + dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr.Fields[strColName + strRowName].Text = dt3.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }

                    // 4.OUT_SEPA
                    DataTable dt4 = runcard.Tables["OUT_SEPA"];
                    for (int col = 0; col < dt4.Columns.Count; col++)
                    {
                        string strColName = dt4.Columns[col].ColumnName;

                        //for (int row = 0; row < dt4.Rows.Count; row++)
                        for (int row = 0; row < 8; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                if (row < dt4.Rows.Count)
                                    cr.Fields[strColName + strRowName].Text = ReplaceHexadecimalSymbols(ReplaceExceptionCode(dt4.Rows[row][strColName].ToString()));
                                else
                                    cr.Fields[strColName + strRowName].Text = "";

                            }
                        }
                    }
                    */
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                Task<bool> task = WaitCallback();
                task.ContinueWith(_ =>
                {
                    HiddenLoadingIndicator();
                    Thread.Sleep(2000);
                    PrintAuto(LoginInfo.CFG_CARD_AUTO == "Y" ? true : false);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.Close();
            }
        }

        async Task<bool> WaitCallback()
        {
            bool succeeded = false;
            while (!succeeded)
            {
                if (
                    c1DocumentViewer.Document != null
                    ) succeeded = true;
                await Task.Delay(500);
            }
            return true;
        }

        private static string ReplaceExceptionCode(string text)
        {
            return Regex.Replace(text, @"[^0-9a-zA-Z]", "");
        }

        private static string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }




        private void PrintViewSmallType()
        {
            try
            {
                cr = new C1.C1Report.C1Report {Layout = {PaperSize = System.Drawing.Printing.PaperKind.A4}};
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.CMM001.Report.Winder_HistoryCardSmallType.xml"))
                {
                    cr.Load(stream, "Winder_HistoryCard");

                    // 임시 다국어 처리
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        if (cr.Fields[cnt].Name.IndexOf("Text", StringComparison.Ordinal) > -1)
                        {
                            if (string.IsNullOrEmpty(cr.Fields[cnt].Text)) continue;
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }
                    }

                    // 1.OUT_DATA
                    DataTable dt1 = runcard.Tables["OUT_DATA"];
                    for (int col = 0; col < dt1.Columns.Count; col++)
                    {
                        string strColName = dt1.Columns[col].ColumnName;

                        if (cr.Fields.Contains(strColName))
                        {
                            double dValue = 0;
                            int nValue = 0;

                            if (strColName.Equals("INPUT_QTY") || strColName.Equals("DFCT_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");
                            }
                            else if (strColName.Equals("PRD_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["PRD_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.Equals("WIP_QTY"))
                            {
                                if (double.TryParse(Util.NVC(dt1.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");

                                if (int.TryParse(Util.NVC(dt1.Rows[0]["WIP_TRAY"]), out nValue))
                                {
                                    if (nValue > 0)
                                        cr.Fields[strColName].Text = dValue.ToString("N0") + " (" + nValue.ToString("N0") + ")";
                                }
                            }
                            else if (strColName.IndexOf("BARCODE", StringComparison.Ordinal) > -1)
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                                cr.Fields[strColName + "_TXT"].Text = dt1.Rows[0][strColName].ToString();
                            }
                            else
                            {
                                cr.Fields[strColName].Text = dt1.Rows[0][strColName].ToString();
                            }

                        }
                    }

                    // 2.OUT_ELEC
                    DataTable dt2 = runcard.Tables["OUT_ELEC"];
                    for (int col = 0; col < dt2.Columns.Count; col++)
                    {
                        string strColName = dt2.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = dt2.Rows[0][strColName].ToString();
                    }

                    // 3.OUT_DFCT
                    DataTable dt3 = runcard.Tables["OUT_DFCT"];
                    for (int col = 0; col < dt3.Columns.Count; col++)
                    {
                        string strColName = dt3.Columns[col].ColumnName;

                        //for (int row = 0; row < dt3.Rows.Count; row++)
                        for (int row = 0; row < 30; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                cr.Fields[strColName + strRowName].Text = "";

                                if (row < dt3.Rows.Count)
                                {
                                    double dValue = 0;

                                    if (strColName.Equals("DFCT_QTY"))
                                    {
                                        if (double.TryParse(Util.NVC(dt3.Rows[row][strColName]), out dValue))
                                        {
                                            if (dValue > 0)
                                                cr.Fields[strColName + strRowName].Text = " " + dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr.Fields[strColName + strRowName].Text = dt3.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }

                    // 4.OUT_SEPA
                    DataTable dt4 = runcard.Tables["OUT_SEPA"];
                    for (int col = 0; col < dt4.Columns.Count; col++)
                    {
                        string strColName = dt4.Columns[col].ColumnName;

                        //for (int row = 0; row < dt4.Rows.Count; row++)
                        for (int row = 0; row < 8; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                if (row < dt4.Rows.Count)
                                    cr.Fields[strColName + strRowName].Text = ReplaceHexadecimalSymbols(ReplaceExceptionCode(dt4.Rows[row][strColName].ToString()));
                                else
                                    cr.Fields[strColName + strRowName].Text = "";

                            }
                        }
                    }

                    // 5.OUT_DFCT
                    DataTable dt5 = runcard.Tables["OUT_TRAY"];
                    for (int col = 0; col < dt5.Columns.Count; col++)
                    {
                        string strColName = dt5.Columns[col].ColumnName;

                        //for (int row = 0; row < dt5.Rows.Count; row++)
                        for (int row = 0; row < 15; row++)
                        {
                            string strRowName = (row + 1).ToString();

                            if (cr.Fields.Contains(strColName + strRowName))
                            {
                                cr.Fields[strColName + strRowName].Text = "";

                                if (row < dt5.Rows.Count)
                                {
                                    double dValue = 0;

                                    if (strColName.Equals("CELL_QTY"))
                                    {
                                        if (double.TryParse(Util.NVC(dt5.Rows[row][strColName]), out dValue))
                                        {
                                            if (dValue > 0)
                                                cr.Fields[strColName + strRowName].Text = " " + dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr.Fields[strColName + strRowName].Text = dt5.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                Task<bool> task = WaitCallback();
                task.ContinueWith(_ =>
                {
                    HiddenLoadingIndicator();
                    Thread.Sleep(2000);
                    PrintAuto(LoginInfo.CFG_CARD_AUTO == "Y" ? true : false);
                }, TaskScheduler.FromCurrentSynchronizationContext());

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.Close();
            }
        }

        private void PrintAuto(bool isAutoConfig)
        {
            if (isAutoConfig)
            {
                if (_isAutoPrint)
                {
                    btnPrint.Visibility = Visibility.Collapsed;
                    SetHistoryCardPrintCount();
                }
            }
        }

        private void SetHistoryCardPrintCount()
        {
            try
            {
                ShowLoadingIndicator();
                string bizRuleName = _isSmallType ? "BR_PRD_REG_WINDING_RUNCARD_PRINT_CNT_WNS" : "BR_PRD_REG_WINDING_RUNCARD_PRINT_CNT_WN";

                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add(_isSmallType ? "PROD_LOTID" : "WINDING_LOT_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();

                if (_isSmallType)
                {
                    dr["PROD_LOTID"] = _lotId;
                }
                else
                {
                    dr["WINDING_LOT_ID"] = _lotId;
                }

                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);
                DataSet ds = new DataSet();
                ds.Tables.Add(inDataTable);
                string xml = ds.GetXml();

                //new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", null, inDataTable);
                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inDataTable, (result, bizEx) =>
                {
                    try
                    {
                        if (bizEx != null)
                            return;

                        //감열지 출력은 오창 소형 조립만 적용함
                        if(!_isSmallType &&
                            (LoginInfo.CFG_AREA_ID.ToString().Equals ("M1") || LoginInfo.CFG_AREA_ID.ToString().Equals("M2")) &&
                            LoginInfo.CFG_THERMAL_PRINT.Rows.Count > 0 && 
                            !string.IsNullOrEmpty(LoginInfo.CFG_THERMAL_PRINT.Rows[0]?[CustomConfig.CONFIGTABLE_THERMALPRINTER_NAME].ToString()))
                        {
                            //감열지 출력 처리 기능
                            LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_Winding thermal_print = new LGC.GMES.MES.CMM001.CMM_THERMAL_PRINT_Winding();
                            thermal_print.FrameOperation = FrameOperation;

                            if (thermal_print != null)
                            {
                                object[] Parameters = new object[6];
                                Parameters[0] = runcard;

                                C1WindowExtension.SetParameters(thermal_print, Parameters);

                                thermal_print.Closed += new EventHandler(thermal_print_Closed);
                                this.Dispatcher.BeginInvoke(new Action(() => thermal_print.ShowModal()));
                            }

                        }
                        else
                        {
                            //일반 A4용지 출력 기능
                            var pm = new C1.C1Preview.C1PrintManager { Document = cr };
                            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                            pm.Print(ps, ps.DefaultPageSettings);
                            Dispatcher.BeginInvoke(new Action(() => loadingIndicator.Visibility = Visibility.Collapsed));
                        }
                        HiddenLoadingIndicator();
                        this.DialogResult = MessageBoxResult.OK;


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

        private void thermal_print_Closed(object sender, EventArgs e)
        {
            CMM_THERMAL_PRINT_Winding popup = sender as CMM_THERMAL_PRINT_Winding;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }
            //this.grdMain.Children.Remove(popup);
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

        #endregion


    }
}
