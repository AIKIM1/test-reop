/*************************************************************************************
 Created Date : 2020.04.02
      Creator : 
   Decription : Tag Print
--------------------------------------------------------------------------------------
 [Change History]
  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2020.04.02  0.1   이현호       21700 자동포장기 실적처리 및 포장출고 신규화면 개발.
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
    public partial class CMM_FORM_OUTBOX_TAG_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        DataTable _dtTagPrint;
        private string _palletID = string.Empty;      // Pallet, Inbox ID
        private string _wipSeq = string.Empty;        // WipSeq

        public string DefectPalletYN { get; set; }
        public string RemainPalletYN { get; set; }
        public string HoldPalletYN { get; set; }
        //QMS 검사의뢰 
        public string QMSRequestPalletYN { get; set; }
        public string returnPalletYN { get; set; }
        public string PrintCount { get; set; }

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
        public CMM_FORM_OUTBOX_TAG_PRINT()
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

            _palletID = tmps[0] as string;
            _wipSeq = tmps[1] as string;

            SetTagPrintData();

            PrintView();

            // 미리보기
         
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

        #region Method

        #region 팔레트 정보 조회 :SetTagPrintData(), SetPallet()
        private void SetTagPrintData()
        {
            SetPallet();

            if (_dtTagPrint == null || _dtTagPrint.Rows.Count == 0)
            {
                // 출력 Lot 정보가 없습니다.
                Util.MessageValidation("SFU4025");
                return;
            }
        }

        private void SetPallet()
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

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_AUTO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 미리보기 : PrintView()
        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintView()
        {
            try
            {
                string ReportNamePath = string.Empty;
                string ReportName = string.Empty;

                // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                string paperSize = string.Empty;
                if (LoginInfo.CFG_ETC.Columns.Contains(CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE))
                {
                    paperSize = LoginInfo.CFG_ETC.Rows[0][CustomConfig.CONFIGTABLE_ETC_PAPER_SIZE].ToString();
                }

                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_OutboxPallet.xml";
                ReportName = "FORM_OutboxPallet";

                cr = new C1.C1Report.C1Report();
                //cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

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
                    
                    txtPageCount.Text = string.IsNullOrWhiteSpace(PrintCount) ? "1": PrintCount;

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

                #region 용지 설정(A4,A5)
                if (paperSize.Equals("A4")) 
                {
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                }
                else if (paperSize.Equals("A5"))
                {
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A5;
                    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                }
                else
                {
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
                    cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                }
                #endregion

                c1DocumentViewer.Document = cr.FixedDocumentSequence;
                c1DocumentViewer.FitToWidth();
                c1DocumentViewer.FitToHeight();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #endregion

        #region 출력 : TagPrint(), SetLabelPrtHist()
        /// <summary>
        /// 출력
        /// </summary>
        private void TagPrint()
        {
            try
            {
                string sMsgID = string.Empty;

                if (LoginInfo.CFG_SHOP_ID == "A010")
                {
                    sMsgID = "SFU2802"; // 포장출고를 하시겠습니까?
                }
                else
                {
                    sMsgID = "SFU2873"; // 발행하시겠습니까?
                }

                //SFU2802	포장출고를 하시겠습니까?
                Util.MessageConfirm(sMsgID, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        var pm = new C1.C1Preview.C1PrintManager();
                        pm.Document = cr;
                        System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                    //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
                        pm.Print(ps, ps.DefaultPageSettings);

                        SetLabelPrtHist();

                        this.DialogResult = MessageBoxResult.OK;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetLabelPrtHist()
        {
            try
            {
                DataTable inTable = _bizDataSet.GetBR_PRD_REG_LABEL_HIST();

                ////DataRow newRow = inTable.NewRow();
                ////newRow["LABEL_PRT_COUNT"] = 1;                                                    // 발행 수량
                ////newRow["PRT_ITEM01"] = _palletID;
                ////newRow["PRT_ITEM02"] = _wipSeq;
                ////newRow["PRT_ITEM04"] = _rePrint;                                                   // 재발행 여부
                ////newRow["INSUSER"] = LoginInfo.USERID;
                ////newRow["LOTID"] = _palletID;

                ////inTable.Rows.Add(newRow);

                DataRow newRow;
                foreach (DataRow row in _dtTagPrint.Rows)
                {
                    newRow = inTable.NewRow();
                    newRow["LABEL_PRT_COUNT"] = 1;                                                    // 발행 수량
                    newRow["PRT_ITEM01"] = row["PALLETID"];
                    newRow["PRT_ITEM02"] = _dtTagPrint.Columns.Contains("WIPSEQ") ? row["WIPSEQ"] : _wipSeq;
                    newRow["PRT_ITEM04"] = "";                                                   // 재발행 여부
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = row["PALLETID"];
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
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
