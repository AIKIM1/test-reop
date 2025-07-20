using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.IO;
using System.Printing;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Report : Window, IWorkArea
    {
        string tmp = string.Empty;
        object[] tmps ;
        string tmmp01 = string.Empty;
        DataTable tmmp02 ;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public Report()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable;

            this.Loaded -= Window_Loaded;

            C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

            string filename = string.Empty;
            string reportname = string.Empty;

            //reportname = tmmp02.Rows[0]["Name"].ToString();
            //filename = tmmp02.Rows[0]["Name"].ToString() + ".xml";

            reportname = tmmp01;
            filename = tmmp01 + ".xml";

            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + filename))
            {
                cr.Load(stream, reportname); 

                for (int col = 0; col < tmmp02.Columns.Count; col++)
                {
                    string strColName = tmmp02.Columns[col].ColumnName;
                    if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[0][strColName].ToString();
                }
            }

            c1DocumentViewer.Document = cr.FixedDocumentSequence;

            //cr.Render();
            //http://our.componentone.com/groups/topic/need-help-getting-c1reports-to-work/
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            //c1DocumentViewer.Print();
            var pq = LocalPrintServer.GetDefaultPrintQueue();
            var writer = PrintQueue.CreateXpsDocumentWriter(pq);
            var paginator = c1DocumentViewer.Document.DocumentPaginator;
            writer.Write(paginator);

            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
