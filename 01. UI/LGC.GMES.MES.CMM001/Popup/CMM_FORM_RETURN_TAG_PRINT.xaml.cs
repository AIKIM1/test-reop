/*************************************************************************************
 Created Date : 2017.07.21
      Creator : 
   Decription : Tag Print
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
    public partial class CMM_FORM_RETURN_TAG_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        DataTable _dtTagPrint;
        private string _procID = string.Empty;        // 공정코드
        private string _prodID = string.Empty;        // 설비코드
        private string _palletID = string.Empty;      // Pallet, Inbox ID
        private string _wipSeq = string.Empty;        // WipSeq
        private string _cellQty = string.Empty;       // 수량
        private string _dispatch = string.Empty;
        private string _rePrint = string.Empty;
        private string _directPrint = string.Empty;

        public string DefectPalletYN { get; set; }

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
        public CMM_FORM_RETURN_TAG_PRINT()
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
            _prodID = tmps[1] as string;
            _palletID = tmps[2] as string;
            _wipSeq = tmps[3] as string;
            _cellQty = tmps[4] as string;
            _dispatch = tmps[5] as string;
            _rePrint = tmps[6] as string;
            _directPrint = tmps[7] as string;

            SetTagPrintData();

            // 미리보기
            PrintView();
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

        private void SetTagPrintData()
        {
            if (string.Equals(DefectPalletYN, "Y"))
            {
                SetPalletDefect();
            }
            else
            {
                SetPallet();
            }

            if (_dtTagPrint == null || _dtTagPrint.Rows.Count == 0)
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("출력 Lot 정보가 없습니다.");
                return;
            }

        }

        /// <summary>
        /// 불량 Pallet 정보 조회
        /// </summary>
        private void SetPalletDefect()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _palletID;
                newRow["WIPSEQ"] = _wipSeq;

                inTable.Rows.Add(newRow);

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_DEFECT_FO", "INDATA", "OUTDATA", inTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Pallet 정보 조회
        /// </summary>
        private void SetPallet()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));
               // inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = _palletID;
              //  newRow["WIPSEQ"] = _wipSeq;

                inTable.Rows.Add(newRow);

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_FM", "INDATA", "OUTDATA", inTable);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintView()
        {
            try
            {
                string ReportNamePath = string.Empty;
                string ReportName = string.Empty;

                if (string.Equals(DefectPalletYN, "Y"))
                {
                    if (string.Equals(_procID, Process.CircularCharacteristic))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteDefect.xml";
                        ReportName = "FORM_GraderPaletteDefect";
                    }
                    else
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect.xml";
                        ReportName = "FORM_GraderPaletteSmallTypeDefect";
                    }
                }
                else
                {
                    string strProdID = _prodID.Substring(0, 3);
                    if (strProdID.Equals("MAC"))
                    {
                        strProdID = "MCC";
                    }
                    _procID = strProdID == "MCC" ? Process.CircularCharacteristic : _procID;

                    if (string.Equals(_procID, Process.CircularCharacteristic))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_ReturnPalette.xml";
                        ReportName = "FORM_ReturnPalette";
                    }
                    else
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_ReturnPaletteSmallType.xml";
                        ReportName = "FORM_ReturnPaletteSmallType";
                    }
                }

                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

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

                        ////// Binding Data Clear
                        ////if (cr.Fields[cnt].Name.IndexOf("LOTID", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("GROUP", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("WINDER", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("GRADE", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("CAPA", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("VLTG", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("SOC", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("QTY", StringComparison.Ordinal) > -1 ||
                        ////    cr.Fields[cnt].Name.IndexOf("BOX", StringComparison.Ordinal) > -1)
                        ////{
                        ////    cr.Fields[cnt].Text = "";
                        ////}


                        // 초소형 공정이구 불량 Pallet인 경우 공정명 표시
                        if (string.Equals(DefectPalletYN, "Y") && _procID != Process.CircularCharacteristic)
                        {
                            if (cr.Fields[cnt].Name.Equals("TagName"))
                            {
                                cr.Fields[cnt].Text = cr.Fields[cnt].Text + " (" + _dtTagPrint.Rows[0]["PROCNAME"].ToString() + ")";
                            }
                        }

                    }

                    // Data Binding
                    for (int col = 0; col < _dtTagPrint.Columns.Count; col++)
                    {
                        string strColName = _dtTagPrint.Columns[col].ColumnName;
                        double dValue = 0;

                        if (cr.Fields.Contains(strColName))
                        {
                            if (strColName.Equals("QTY") || strColName.Equals("BOX"))
                            {
                                if (double.TryParse(Util.NVC(_dtTagPrint.Rows[0][strColName]), out dValue))
                                    cr.Fields[strColName].Text = dValue.ToString("N0");
                            }
                            else
                            {
                                cr.Fields[strColName].Text = _dtTagPrint.Rows[0][strColName].ToString();
                            }
                        }

                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                if (_directPrint.Equals("Y"))
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                    pm.Print(ps, ps.DefaultPageSettings);
                    this.DialogResult = MessageBoxResult.OK;
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

        /// <summary>
        /// 출력
        /// </summary>
        private void TagPrint()
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
            pm.Print(ps, ps.DefaultPageSettings);

            SetLabelPrtHist();

            if (string.Equals(_dispatch, "Y"))
            {
                SetDispatch();
            }

            this.DialogResult = MessageBoxResult.OK;
        }

        private void SetLabelPrtHist()
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                DataRow newRow = inDataTable.NewRow();
                inDataTable.Columns.Add("USERID");

                newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);

                newRow = null;

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                newRow = inBoxTable.NewRow();
                newRow["BOXID"] = _palletID;
                inBoxTable.Rows.Add(newRow);


                newRow = null;

                DataTable inItemTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST(); //indataSet.Tables.Add("INITEM");
                inItemTable.TableName = "INITEM";
                
                newRow = inItemTable.NewRow();
                newRow["PRT_ITEM01"] = _palletID;
                newRow["PRT_ITEM02"] = _wipSeq;
                newRow["PRT_ITEM04"] = _rePrint;                                                   // 재발행 여부
                inItemTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_TAG_PRINT_FOR_BOX_FM", "INDATA,INBOX,INITEM", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {

                    }
                },indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //Util.MessageException(ex);
            }
        }

        private void SetDispatch()
        {
            try
            {
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //newRow["LANGID"] = LoginInfo.LANGID;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = string.Empty;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["REWORK"] = "N";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                newRow = inDataTable.NewRow();
                newRow["LOTID"] = _palletID;
                newRow["ACTQTY"] = Util.NVC_Decimal(_cellQty);
                newRow["ACTUQTY"] = 0;
                newRow["WIPNOTE"] = "";

                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion



    }
}
