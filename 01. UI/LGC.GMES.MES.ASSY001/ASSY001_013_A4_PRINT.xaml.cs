using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_013_A4_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_013_A4_PRINT : C1Window, IWorkArea
    {
        private List<Dictionary<string, string>> _List = null;
        private String _Page = null;
        private int _PageQty = 1;
        private string _ReportFormat = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY001_013_A4_PRINT()
        {
            InitializeComponent();
        }

        public ASSY001_013_A4_PRINT(List<Dictionary<string, string>> listParam)
        {
            try
            {
                InitializeComponent();

                //object[] tmps = C1WindowExtension.GetParameters(this);
                //dicParam = tmps[0] as Dictionary<string, string>;
                _List = listParam;

                //최초 보기용
                
                _Page = _List[0]["PAGE"]; //dic["PAGE"];

                C1.C1Report.C1Report[] crList = new C1.C1Report.C1Report[(_List.Count + 1) / 2];

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();                

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                System.Drawing.Imaging.Metafile[] Pages = new System.Drawing.Imaging.Metafile[crList.Length];

                if (LoginInfo.LANGID == "ko-KR")
                {
                    _ReportFormat = "LGC.GMES.MES.ASSY001.Report.CELL_PALLET.xml";
                }
                else
                {
                    _ReportFormat = "LGC.GMES.MES.ASSY001.Report.CELL_PALLET_EN.xml";
                }

                using (Stream stream = a.GetManifestResourceStream(_ReportFormat))
                {
                    crList[0] = new C1.C1Report.C1Report();
                    crList[0].Load(stream, _Page); //cr.Load(@"C:\c1report.xml", "test");
                    crList[0].Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    for (int i=1; i< crList.Length; i++)
                    {
                        crList[i] = new C1.C1Report.C1Report();
                        crList[i].CopyFrom(crList[0]);
                    }

                    int page = 0;

                    for (int i = 0; i < _List.Count; i++)
                    {
                        page = i / 2;

                        string postfix = string.Empty;
                        if (i % 2 == 1)
                            postfix = "1";

                        foreach (var pair in _List[i])
                        {
                            if (crList[page].Fields.Contains(pair.Key + postfix)) crList[page].Fields[pair.Key + postfix].Text = pair.Value;                            
                        }
                    }                                     

                    page = 0;
                    foreach (var cr in crList)
                    {  
                        if (_Page == "PALLET_LOT")
                        {    
                            //팔레트별 lot 정보
                          
                            DataTable dtRqst = new DataTable();
                            dtRqst.Columns.Add("BOXID", typeof(string));
                            dtRqst.Columns.Add("SHOPID", typeof(string));

                            DataRow dr = dtRqst.NewRow();
                            dr["BOXID"] = _List[0]["txtPallet"];
                            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                            dtRqst.Rows.Add(dr);

                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "INDATA", "OUTDATA", dtRqst);

                            for (int i = 0; i < 22; i++)
                            {
                                cr.Fields["txtLot" + (i + 1).ToString()].Text = "";
                                cr.Fields["txtQty" + (i + 1).ToString()].Text = "";
                            }

                            for (int i = 0; i < dtRslt.Rows.Count; i++)
                            {
                                cr.Fields["txtLot" + (i + 1).ToString()].Text = dtRslt.Rows[i]["LOTID"].ToString();
                                cr.Fields["txtQty" + (i + 1).ToString()].Text = dtRslt.Rows[i]["QTY"].ToString();
                            }
                        }

                        page++;

                        cr.Render();

                        crList[0].C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                        crList[0].C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                        crList[0].C1Document.Body.Children.Add(new C1.C1Preview.RenderImage(cr.GetPageImage(0)));                        
                        crList[0].C1Document.Reflow();

                    }
                }

                c1DocumentViewer.Document = crList[0].FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                _PageQty = Util.NVC_Int(tmps[0]);

                button.IsEnabled = false;

                if (_Page == "PALLET_LOT")
                {
                    PrintQueue pq1 = LocalPrintServer.GetDefaultPrintQueue();

                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                        pq1 = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());
                    var writer1 = PrintQueue.CreateXpsDocumentWriter(pq1);

                    var paginator1 = c1DocumentViewer.Document.DocumentPaginator;
                    writer1.Write(paginator1);
                }
                else
                {
                    C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                    using (Stream stream = a.GetManifestResourceStream(_ReportFormat))
                    {
                        cr.Load(stream, _Page); //cr.Load(@"C:\c1report.xml", "test");

                        int cnt = 0;
                        foreach (Dictionary<string, string> dic in _List)
                        {
                            cnt++;

                            if (cnt % 2 == 0)
                            {
                                foreach (var pair in dic)
                                {
                                    //if (cr.Fields.Contains(pair.Key)) cr.Fields[pair.Key].Text = pair.Value;
                                    if (cr.Fields.Contains(pair.Key + "1")) cr.Fields[pair.Key + "1"].Text = pair.Value;
                                }
                            }
                            else
                            {
                                foreach (var pair in dic)
                                {
                                    if (cr.Fields.Contains(pair.Key)) cr.Fields[pair.Key].Text = pair.Value;
                                    if (cr.Fields.Contains(pair.Key + "1")) cr.Fields[pair.Key + "1"].Text = "";
                                }
                            }

                            c1DocumentViewer.Document = cr.FixedDocumentSequence;
                            //c1DocumentViewer.Document.Print();

                            if (cnt.Equals(_List.Count) || cnt % 2 == 0)
                            {
                                //var pq = LocalPrintServer.GetDefaultPrintQueue();

                                //if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                                //    pq = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());

                                //var writer = PrintQueue.CreateXpsDocumentWriter(pq);


                                //var paginator = c1DocumentViewer.Document.DocumentPaginator;
                                //writer.Write(paginator);

                                // Print수량만큼 출력함...
                                for (int iPrint = 0; iPrint < _PageQty; iPrint++)
                                {
                                    var pq = LocalPrintServer.GetDefaultPrintQueue();

                                    if (LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                                        pq = new PrintQueue(new PrintServer(), LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString());

                                    var writer = PrintQueue.CreateXpsDocumentWriter(pq);

                                    var paginator = c1DocumentViewer.Document.DocumentPaginator;
                                    writer.Write(paginator);
                                }
                            }
                        }
                    }

                    C1.C1Report.C1Report cr1 = new C1.C1Report.C1Report();
                    cr1.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    System.Reflection.Assembly a1 = System.Reflection.Assembly.GetExecutingAssembly();

                    using (Stream stream = a1.GetManifestResourceStream(_ReportFormat))
                    {
                        cr1.Load(stream, "PALLET_LOT");
                        foreach (Dictionary<string, string> dic in _List)
                        {

                            if (dic["LIST"].Equals("Y"))
                            {
                                //팔레트별 lot 정보
                                cr1.Fields["txtVol"].Text = "(" + dic["txtVol"] + ")";
                                cr1.Fields["txtCarType"].Text = dic["txtCarType"];
                                DataTable dtRqst = new DataTable();
                                dtRqst.Columns.Add("BOXID", typeof(string));
                                dtRqst.Columns.Add("SHOPID", typeof(string));

                                DataRow dr = dtRqst.NewRow();
                                dr["BOXID"] = dic["txtPallet"];
                                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                                dtRqst.Rows.Add(dr);

                                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_LOT_BY_PALLET", "INDATA", "OUTDATA", dtRqst);

                                for (int i = 0; i < 22; i++)
                                {
                                    cr1.Fields["txtLot" + (i + 1).ToString()].Text = "";
                                    cr1.Fields["txtQty" + (i + 1).ToString()].Text = "";
                                }

                                for (int i = 0; i < dtRslt.Rows.Count; i++)
                                {
                                    cr1.Fields["txtLot" + (i + 1).ToString()].Text = dtRslt.Rows[i]["LOTID"].ToString();
                                    cr1.Fields["txtQty" + (i + 1).ToString()].Text = dtRslt.Rows[i]["QTY"].ToString();
                                }

                                c1DocumentViewer.Document = cr1.FixedDocumentSequence;
                                //c1DocumentViewer.Document.Print();

                                // Print수량만큼 출력함...
                                for (int iPrint = 0; iPrint < _PageQty; iPrint++)
                                {
                                    var pm = new C1.C1Preview.C1PrintManager();
                                    pm.Document = cr1;
                                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                                    if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                                        ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                                    pm.Print(ps);
                                }
                            }
                        }
                    }
                }
                Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
