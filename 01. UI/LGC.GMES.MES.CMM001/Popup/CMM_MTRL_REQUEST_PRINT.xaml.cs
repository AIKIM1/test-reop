/*************************************************************************************
 Created Date : 2019.07.11
      Creator : 
   Decription : 원자재 관리 - 요청서 출력
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
    public partial class CMM_MTRL_REQUEST_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        DataTable _dtRequest;
        private string _requestID = string.Empty;       
        private string _directPrint = string.Empty;
        private string _paperSize = string.Empty;
        private string _SettingPrintName = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        public CMM_MTRL_REQUEST_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _requestID = tmps[0] as string;
            _directPrint = tmps[1] as string;

            if (LoginInfo.CFG_GENERAL_PRINTER != null
                && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0
                && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null
                && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
            {
                _SettingPrintName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();
            }

            if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
            {
                _paperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
            }
        }
        #endregion

        #region Event
        /// <summary>
        /// Form Load
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= Window_Loaded;

            SetControl();
            SearchProcess();

            if (_dtRequest == null || _dtRequest.Rows.Count == 0)
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            else
            {
                PrintView();
            }
        }

        /// <summary>
        /// 출력 
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintProcess();
        }

        /// <summary>
        /// 닫기 
        /// </summary>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        /// <summary>
        /// SizeChanged(
        /// </summary>
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();
        }
        #endregion

        #region Method

        private void SearchProcess()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MTRL_SPLY_REQ_ID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MTRL_SPLY_REQ_ID"] = _requestID;

                inTable.Rows.Add(newRow);

                _dtRequest = new DataTable();
                _dtRequest = new ClientProxy().ExecuteServiceSync("DA_MTR_SEL_MATERIAL_REQUEST_PRINT", "INDATA", "OUTDATA", inTable);
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
                string ReportNamePath = "LGC.GMES.MES.CMM001.Report.MTRL_Request.xml";
                string ReportName = "MTRL_Request";

                cr = new C1.C1Report.C1Report();
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
                    }

                    // Data Binding
                    for (int row = 0; row < _dtRequest.Rows.Count; row++)
                    {
                        for (int col = 0; col < _dtRequest.Columns.Count; col++)
                        {
                            string strColName = _dtRequest.Columns[col].ColumnName;
                            string strTagName = _dtRequest.Columns[col].ColumnName;
                            double dValue = 0;

                            if (strColName.Equals("MTRL_CLSS3_CODE") ||
                                strColName.Equals("MTRLID") ||
                                strColName.Equals("MTRLDESC") ||
                                strColName.Equals("MTRL_SPLY_REQ_QTY") ||
                                strColName.Equals("REQ_UNIT_CODE") ||
                                strColName.Equals("CONV_QTY") ||
                                strColName.Equals("STCK_UNIT_CODE"))
                            {
                                strTagName = strTagName + (row + 1).ToString();
                            }

                            if (cr.Fields.Contains(strTagName))
                            {
                                if (strColName.Equals("MTRL_SPLY_REQ_QTY") || strColName.Equals("CONV_QTY"))
                                {
                                    if (double.TryParse(Util.NVC(_dtRequest.Rows[row][strColName]), out dValue))
                                    {
                                        cr.Fields[strTagName].Text = dValue.ToString("N0");
                                    }
                                }
                                else
                                {
                                    cr.Fields[strTagName].Text = _dtRequest.Rows[row][strColName].ToString();
                                }
                            }

                        }
                    }
                }

                //// 용지 설정(A4,A5)
                //if (string.IsNullOrWhiteSpace(_paperSize) || _paperSize.Equals("A4"))
                //{
                //    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                //    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                //}
                //else
                //{
                //    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A5;
                //    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Portrait;
                //}

                c1DocumentViewer.Document = cr.FixedDocumentSequence;

                if (_directPrint.Equals("Y"))
                {
                    PrintProcess();
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
        private void PrintProcess()
        {
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            pm.Print(ps, ps.DefaultPageSettings);
        }

        #endregion



    }
}
