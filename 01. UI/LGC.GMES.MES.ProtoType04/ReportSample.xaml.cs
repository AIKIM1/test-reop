using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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

namespace LGC.GMES.MES.ProtoType04
{
    /// <summary>
    /// ReportSample.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ReportSample : Window
    {
        C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
        public ReportSample()
        {
            InitializeComponent();

            
            cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
            //cr.Layout.Orientation = C1.C1Report.OrientationEnum.Landscape;
                        
            string filename = "c1report_land.xml";
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

            using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.ProtoType04.Report." + filename))
            {  
                cr.Load(stream, "RptHistoryCard"); //cr.Load(@"C:\c1report.xml", "test");

                //레포트 라벨 다국어처리
                //foreach (C1.C1Report.Field f in cr.Fields)
                //{
                //    f.Text = ObjectDic.Instance.GetObjectName(f.Text);
                //}

                #region 테스트를 위한 가상 테이블		
                DataSet dsRptHistoryCard = new DataSet();
                              
                #region dtCoater            
                DataTable dtCoater = new DataTable("dtCoater");
                dtCoater.Columns.Add("COAT_LOTID");
                dtCoater.Columns.Add("COAT_WORKDATE");
                dtCoater.Columns.Add("COAT_WORKER");
                dtCoater.Columns.Add("COAT_PRODLEN");
                dtCoater.Columns.Add("COAT_PRODCELL");
                dtCoater.Columns.Add("COAT_REMARK");
                dtCoater.Columns.Add("COAT_EQPTORDER");
                dtCoater.Columns.Add("COAT_TOP_LOADING");
                dtCoater.Columns.Add("COAT_TOP_COATING");
                dtCoater.Columns.Add("COAT_TOP_UNCOATING");
                dtCoater.Columns.Add("COAT_BACK_LOADING");
                dtCoater.Columns.Add("COAT_BACK_COATING");
                dtCoater.Columns.Add("COAT_BACK_UNCOATING");
                dtCoater.Columns.Add("COAT_TOP_FOIL_L");
                dtCoater.Columns.Add("COAT_TOP_FOIL_M");
                dtCoater.Columns.Add("COAT_TOP_FOIL_R");
                dtCoater.Columns.Add("COAT_BACK_FOIL_L");
                dtCoater.Columns.Add("COAT_BACK_FOIL_M");
                dtCoater.Columns.Add("COAT_BACK_FOIL_R");
                dsRptHistoryCard.Tables.Add(dtCoater);
                #endregion
                             
                #region dtHead
                DataTable dtHead = new DataTable("dtHead");
                dtHead.Columns.Add("HEAD_LOTID");
                dtHead.Columns.Add("HEAD_BARCODE");
                dtHead.Columns.Add("HEAD_VERSION");
                dtHead.Columns.Add("HEAD_ELECTRODE");
                dtHead.Columns.Add("HEAD_PRODID");
                dtHead.Columns.Add("HEAD_PRINTDATE");
                dtHead.Columns.Add("HEAD_EXPIREDATE");
                dtHead.Columns.Add("HEAD_MODELGRPNAME");
                dtHead.Columns.Add("HEAD_QTY");
                dtHead.Columns.Add("HEAD_QTY_BARCODE");
                dsRptHistoryCard.Tables.Add(dtHead);
                #endregion
                
                #region dtMixer
                DataTable dtMixer = new DataTable("dtMixer");
                dtMixer.Columns.Add("MIX1_BATCH");
                dtMixer.Columns.Add("MIX1_MTRL1");
                dtMixer.Columns.Add("MIX1_MTRL2");
                dtMixer.Columns.Add("MIX1_DATE1");
                dtMixer.Columns.Add("MIX1_DATE2");
                dtMixer.Columns.Add("MIX1_WORKER");
                dtMixer.Columns.Add("MIX1_JUMDO");
                dtMixer.Columns.Add("MIX1_GOHYUNG");
                dtMixer.Columns.Add("MIX1_PRODQTY");
                dtMixer.Columns.Add("MIX1_REMARK");            
                dtMixer.Columns.Add("MIX_EQPTORDER");
                dsRptHistoryCard.Tables.Add(dtMixer);
                #endregion

                #region dtRollPress
                DataTable dtRollPress = new DataTable("dtRollPress");
                dtRollPress.Columns.Add("RP_LOTID");
                dtRollPress.Columns.Add("RP_WORKDATE");
                dtRollPress.Columns.Add("RP_WORKER");
                dtRollPress.Columns.Add("RP_PRODLEN");
                dtRollPress.Columns.Add("RP_PRODCELL");
                dtRollPress.Columns.Add("RP_REMARK");
                dtRollPress.Columns.Add("RP_EQPTORDER");
                dtRollPress.Columns.Add("RP_ROLLTHICK");
                dtRollPress.Columns.Add("RP_RED");
                dtRollPress.Columns.Add("RP_BLUE");
                dtRollPress.Columns.Add("RP_WHITE");
                dtRollPress.Columns.Add("RP_YELLOW");
                dtRollPress.Columns.Add("RP_ORANGE");
                dtRollPress.Columns.Add("RP_PAPER");
                dsRptHistoryCard.Tables.Add(dtRollPress);
                #endregion

                #region dtSlitter
                DataTable dtSlitter = new DataTable("dtSlitter");
                dtSlitter.Columns.Add("SLIT_LOTID");
                dtSlitter.Columns.Add("SLIT_EQPTORDER");
                dtSlitter.Columns.Add("SLIT_WORKDATE");
                dtSlitter.Columns.Add("SLIT_WORKER");
                dtSlitter.Columns.Add("SLIT_CONNECT");
                dtSlitter.Columns.Add("SLIT_GOODCNT_TOT");
                dtSlitter.Columns.Add("SLIT_GOODCNT_1");
                dtSlitter.Columns.Add("SLIT_GOODCNT_2");
                dtSlitter.Columns.Add("SLIT_GOODCNT_3");
                dtSlitter.Columns.Add("SLIT_GOODCNT_4");
                dtSlitter.Columns.Add("SLIT_REMARK");
                dtSlitter.Columns.Add("SLIT_REMARK_1");
                dtSlitter.Columns.Add("SLIT_REMARK_2");
                dtSlitter.Columns.Add("SLIT_REMARK_3");
                dtSlitter.Columns.Add("SLIT_REMARK_4");
                dtSlitter.Columns.Add("SLIT_CUT");
                dsRptHistoryCard.Tables.Add(dtSlitter);
                #endregion

                #endregion
                                
                #region  테스트용 출력
                foreach (DataTable dtData in dsRptHistoryCard.Tables)
                {
                    for (int col = 0; col < dtData.Columns.Count; col++)
                    {
                        string strColName = dtData.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = col.ToString();//strColName;
                    }
                }
                #endregion                

                //cr.Fields["testF"].Text = "test";
                //cr.Fields["barF"].Text = "1";
            }

            c1DocumentViewer.Document = cr.FixedDocumentSequence;

            //cr.Render();
            //http://our.componentone.com/groups/topic/need-help-getting-c1reports-to-work/
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //c1DocumentViewer.Print();
            var pm = new C1.C1Preview.C1PrintManager();
            pm.Document = cr;
            System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //ps.DefaultPageSettings.Landscape = true;
            
            //ps.DefaultPageSettings.PrinterSettings.Copies = (short)iCopies;
            pm.Print(ps, ps.DefaultPageSettings);
        }
    }
}
