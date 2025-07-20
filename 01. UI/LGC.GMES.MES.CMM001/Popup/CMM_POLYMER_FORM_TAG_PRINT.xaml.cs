/*************************************************************************************
 Created Date : 2017.12.06
      Creator : 
   Decription : 활성화 후공정 - 폴리머 Tag Print
--------------------------------------------------------------------------------------
 [Change History]
**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.IO;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_POLYMER_FORM_TAG_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        //string _sPGM_ID = nameof(Class);
        string _sPGM_ID = "CMM_POLYMER_FORM_TAG_PRINT";

        DataTable _dtCartSheetPrint;
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _cartID = string.Empty;        // Cart ID
        private string _directPrint = string.Empty;
        private string _tempPrint = string.Empty;
        private string _SettingPrintName = string.Empty;

        public string CART_MERGE { get; set; }
        public string PrintCount { get; set; }
        public DataRow[] DataRowCartSheet { get; set; }
        public string CART_RE{ get; set; } // 활성화 대차 재공관리에서 호출
        public string DefectCartYN { get; set; }
        public string DefectGroupLotYN { get; set; }

        public string NonWipYN { get; set; }  //비재공 폐기 화면에서 호출
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
        public CMM_POLYMER_FORM_TAG_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Window_Loaded;

            SetControl();
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[1] as string;
            _cartID = tmps[2] as string;
            _directPrint = tmps[3] as string;
            _tempPrint = tmps[4] as string;

            if (LoginInfo.CFG_GENERAL_PRINTER != null 
                && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 
                && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null
                && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
            {
                _SettingPrintName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
            }
            //else
            //{
            //    // Setting에서 기본 프린트 설정을 먼저 하세요.
            //    Util.MessageValidation("SFU4428");
            //    this.DialogResult = MessageBoxResult.Cancel;
            //}

            if (SetCartSheetPrintData())
            {
                // 미리보기 또는 바로 출력
                PrintView();
            }
            else
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }

        }

        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private bool SetCartSheetPrintData()
        {
            if (DataRowCartSheet != null && DataRowCartSheet.Length > 0)
            {
                _dtCartSheetPrint = DataRowCartSheet.CopyToDataTable();
                return true;
            }

            // 대차 정보 조회
            if (CART_MERGE == "Y")
            {
                SetCart_MERGE();
            }
            else if(CART_RE == "Y")
            {
                SetCart_RE();
            }
            else if (DefectGroupLotYN != null && DefectGroupLotYN.Equals("Y"))
            {
                SetDefectGroupLot();
            }
            else if (NonWipYN != null && NonWipYN.Equals("Y")) //비재공 화면에서 호출
            {
                SetNonWip();
            }

            else
            {
                SetCart();
            }

            if (_dtCartSheetPrint == null || _dtCartSheetPrint.Rows.Count == 0)
            {
                // 대차에 Inbox 정보가 없습니다.
                Util.MessageValidation("SFU4375");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 대차 정보 조회
        /// </summary>
        private void SetCart()
        {
            try
            {
                string bizRuleName = string.Empty;

                if (string.IsNullOrWhiteSpace(_tempPrint) || _tempPrint.Equals("N"))
                {
                    if (string.Equals(_procID, Process.CELL_BOXING_RETURN) || string.Equals(_procID, Process.CELL_BOXING))
                        bizRuleName = "DA_PRD_SEL_CART_SHEET_NJ";
                    else
                        bizRuleName = "DA_PRD_SEL_CART_SHEET_PC";
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_CART_SHEET_TEMP";
                }

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                if (string.IsNullOrWhiteSpace(_tempPrint) || _tempPrint.Equals("N"))
                {
                    inTable.Columns.Add("EQPTID", typeof(string));
                    inTable.Columns.Add("PROCID", typeof(string));
                }
                inTable.Columns.Add("CART_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                if (string.IsNullOrWhiteSpace(_tempPrint) || _tempPrint.Equals("N"))
                {
                    newRow["EQPTID"] = _eqptID;
                    newRow["PROCID"] = _procID;
                }
                newRow["CART_ID"] = _cartID;

                inTable.Rows.Add(newRow);

                _dtCartSheetPrint = new DataTable();
                _dtCartSheetPrint = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetCart_MERGE()
        {
            try
            {
                string bizRuleName = string.Empty;
                bizRuleName = "DA_PRD_SEL_CART_SHEET_MERGE";
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = _cartID;

                inTable.Rows.Add(newRow);

                _dtCartSheetPrint = new DataTable();
                _dtCartSheetPrint = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetCart_RE()
        {
            try
            {
                string bizRuleName = string.Empty;
                bizRuleName = "DA_PRD_SEL_CART_SHEET_RE";
                       

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CART_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["CART_ID"] = _cartID;

                inTable.Rows.Add(newRow);

                _dtCartSheetPrint = new DataTable();
                _dtCartSheetPrint = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 불량 그룹LOT 정보 조회
        /// </summary>
        private void SetDefectGroupLot()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_DEFECT_GROUPLOT_SHEET_PC";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _cartID;

                inTable.Rows.Add(newRow);

                _dtCartSheetPrint = new DataTable();
                _dtCartSheetPrint = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }




        /// <summary>
        /// 비재공 정보 조회
        /// </summary>
        private void SetNonWip()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_POLYMER_NON_WIP_SCRAP_SHEET";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("NON_WIP_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["NON_WIP_ID"] = _cartID;

                inTable.Rows.Add(newRow);

                _dtCartSheetPrint = new DataTable();
                _dtCartSheetPrint = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #region 공통 : 미리보기
        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintView()
        {
            try
            {
                string ReportNamePath = string.Empty;
                string ReportName = string.Empty;

                #region Print 태그 설정
                //ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart.xml";
                //ReportName = "POLYMERFORM_GoodCart";

                String PaperSize = string.Empty;
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
                {
                    PaperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
                }

                if (DefectCartYN != null && DefectCartYN.Equals("Y"))
                {
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_DefectCart.xml";
                    ReportName = "POLYMERFORM_DefectCart";
                }
                else if (DefectGroupLotYN != null && DefectGroupLotYN.Equals("Y"))
                {
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_DefectGroupLot.xml";
                    ReportName = "POLYMERFORM_DefectGroupLot";
                }
                else if (NonWipYN != null && NonWipYN.Equals("Y"))  // 비재공 폐기 화면에서 호출
                {
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_NonWip.xml";
                    ReportName = "POLYMERFORM_NonWip";
                }

                else
                {
                    if (string.IsNullOrWhiteSpace(PaperSize) || PaperSize.Equals("A4"))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart.xml";
                        ReportName = "POLYMERFORM_GoodCart";
                    }
                    else
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.POLYMERFORM_GoodCart_A5.xml";
                        ReportName = "POLYMERFORM_GoodCart_A5";
                    }
                }
                #endregion

                cr = new C1.C1Report.C1Report();
                ////if (string.IsNullOrWhiteSpace(PaperSize) || PaperSize.Equals("A4"))
                ////    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                ////else
                ////    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A5;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream(ReportNamePath))
                {
                    cr.Load(stream, ReportName);

                    // 다국어 처리 및 Clear
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        // Title
                        if (cr.Fields[cnt].Name.IndexOf("txTitle", StringComparison.Ordinal) > -1)
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                        }

                        // 공정명 표시
                        if (cr.Fields[cnt].Name.Equals("TagName"))
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                            cr.Fields[cnt].Text = _dtCartSheetPrint.Rows[0]["PROCNAME"].ToString() + " " + cr.Fields[cnt].Text;
                        }

                    }

                    txtPageCount.Text = string.IsNullOrWhiteSpace(PrintCount) ? "1" : PrintCount;

                    // Data Binding
                    for (int row = 0; row < _dtCartSheetPrint.Rows.Count; row++)
                    {
                        for (int col = 0; col < _dtCartSheetPrint.Columns.Count; col++)
                        {
                            string strColName = _dtCartSheetPrint.Columns[col].ColumnName;
                            string strTagName = _dtCartSheetPrint.Columns[col].ColumnName;
                            double dValue = 0;

                            if (strColName.Equals("ASSY_LOTID") || 
                                strColName.Equals("GRADE") || 
                                strColName.Equals("INBOX_QTY") || 
                                strColName.Equals("CELL_QTY") || 
                                strColName.Equals("GRADE") ||
                                strColName.Equals("RESNGRNAME"))
                                strTagName = strTagName + (row + 1).ToString();

                            if (cr.Fields.Contains(strTagName))
                            {
                                if (strColName.Equals("INBOX_QTY") || strColName.Equals("CELL_QTY") || strColName.Equals("TOTAL_INBOX_QTY") || strColName.Equals("TOTAL_CELL_QTY"))
                                {
                                    if (double.TryParse(Util.NVC(_dtCartSheetPrint.Rows[row][strColName]), out dValue))
                                    {
                                        cr.Fields[strTagName].Text = dValue.ToString("N0");
                                    }
                                }
                                else
                                {
                                    cr.Fields[strTagName].Text = _dtCartSheetPrint.Rows[row][strColName].ToString();
                                }
                            }

                        }
                    }
                }

                #region >>>>> 용지 설정(A4,A5)
                ////String PaperSize = string.Empty; 
                ////if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
                ////{
                ////    PaperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
                ////}

                if (string.IsNullOrWhiteSpace(PaperSize) || PaperSize.Equals("A4"))
                {
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                }
                else
                {
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A5;
                    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Portrait;
                }
                #endregion

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                if (_directPrint.Equals("Y"))
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                    pm.Print(ps, ps.DefaultPageSettings);

                    //ps.PrinterName = _SettingPrintName;
                    //pm.Print(ps);

                    SetLabelPrtHist();
                }
                else
                {
                    c1DocumentViewer.FitToWidth();
                    c1DocumentViewer.FitToHeight();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// 출력
        /// </summary>
        private void TagPrint()
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
            //pm.Print(ps, ps.DefaultPageSettings);

            //ps.PrinterName = _SettingPrintName;
            //pm.Print(ps);

            pm.Print(ps, ps.DefaultPageSettings);

            SetLabelPrtHist();
        }

        private void SetLabelPrtHist()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("LABEL_PRT_COUNT", typeof(string));
                inTable.Columns.Add("PRT_ITEM01", typeof(string));
                inTable.Columns.Add("PRT_ITEM02", typeof(string));
                inTable.Columns.Add("PRT_ITEM04", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("PGM_ID", typeof(string));
                inTable.Columns.Add("BZRULE_ID", typeof(string));

                DataRow newRow;

                foreach (DataRow row in _dtCartSheetPrint.Rows)
                {
                    newRow = inTable.NewRow();
                    newRow["CTNR_ID"] = row["CART_ID"];
                    newRow["LABEL_PRT_COUNT"] = 1;                                                    // 발행 수량
                    newRow["PRT_ITEM01"] = row["CART_ID"];
                    //newRow["PRT_ITEM02"] = _dtCartSheetPrint.Columns.Contains("WIPSEQ") ? row["WIPSEQ"] : _wipSeq;
                    //newRow["PRT_ITEM04"] = _dtCartSheetPrint;                                                   // 재발행 여부
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = "BR_PRD_REG_LABEL_PRINT_HIST_CTNR";

                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST_CTNR", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
        }

        #endregion



    }
}
