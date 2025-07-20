/*************************************************************************************
 Created Date : 2017-01-25
      Creator : 
  Description : Pack 포장 정보 레포트 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.25 srcadm01 : Initial Created.
  2022.05.04 srcadm01 : ReCreated.
**************************************************************************************/
using C1.C1Report;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace LGC.GMES.MES.PACK001
{
    public partial class CST_Prt : Window, IWorkArea
    {
        #region Member Variable Lists...
        private DataTable dtHeader = new DataTable();
        private DataTable dtDetail = new DataTable();
        string REPORT_NAME = "Pallet_Tag_CST";
        private bool isLOTInclude = true;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Properties...


        public DataTable HEADERDATA
        {
            get
            {
                return dtHeader;
            }
            set
            {
                dtHeader = value;
            }
        }

        public DataTable DETAILDATA
        {
            get
            {
                return dtDetail;
            }
            set
            {
                dtDetail = value;
            }
        }

        public bool ISLOTINCLUDE
        {
            get
            {
                return isLOTInclude;
            }
            set
            {
                isLOTInclude = value;
            }
        }
        #endregion

        #region Constructor
        public CST_Prt()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Member Function Lists..
        private void Initialize()
        {
            object[] obj = C1WindowExtension.GetParameters(this);
            if (obj == null)
            {
                this.Close();
                return;
            }

            this.isLOTInclude = (bool)obj[0];
            this.dtHeader = (DataTable)obj[1];
            this.dtDetail = (DataTable)obj[2];

            // LOT 리스트 포함으로 보냈는데 실제 LOT 리스트가 없으면 헤더만 출력하기
            if (this.isLOTInclude && !CommonVerify.HasTableRow(this.dtDetail))
            {
                this.isLOTInclude = false;
            }
        }

        // Report 만들기
        private void SetReport()
        {
            try
            {
                C1Report c1Report = new C1Report();
                string reportFileName = "Pallet_Tag_CST.xml";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + reportFileName))
                {
                    if (stream != null)
                    {
                        c1Report.Load(stream, this.REPORT_NAME);        // Load Report Definition
                        this.TranslateObjectID(ref c1Report);           // 다국어 처리
                        this.SetReportEntityVisible(ref c1Report);      // Field Visible / Invisible
                        this.BindingHeaderData(ref c1Report);           // Binding Header Data
                        if (this.isLOTInclude)                          // Binding Detail Data
                        {
                            this.BindingDetailData(ref c1Report);
                        }
                        c1Report.Layout.PaperSize = PaperKind.A4;       // Define Paper Size
                        c1Report.Render();                              // Rendering
                        this.c1DocumentViewer_CST.Document = c1Report.C1Document.FixedDocumentSequence;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // 다국어 처리
        private void TranslateObjectID(ref C1Report c1Report)
        {
            c1Report.Fields["lblTitle"].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields["lblTitle"].Text);
            c1Report.Fields["lblCassetteID1"].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields["lblCassetteID1"].Text);
            c1Report.Fields["lblPrintDate"].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields["lblPrintDate"].Text);
            c1Report.Fields["lblLOTQty1"].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields["lblLOTQty1"].Text);
        }

        // LOT 포함 여부에 따른 Field의 보이기 숨기기 설정
        private void SetReportEntityVisible(ref C1Report c1Report)
        {
            // Header
            c1Report.Fields["vLineHeader04"].Visible = this.isLOTInclude; c1Report.Fields["vLineHeader04"].Calculated = true;
            c1Report.Fields["vLineHeader05"].Visible = this.isLOTInclude; c1Report.Fields["vLineHeader05"].Calculated = true;
            c1Report.Fields["vLineHeader06"].Visible = this.isLOTInclude; c1Report.Fields["vLineHeader06"].Calculated = true;
            c1Report.Fields["vHeaderLine07"].Visible = this.isLOTInclude; c1Report.Fields["vHeaderLine07"].Calculated = true;
            c1Report.Fields["lblLOTID_01"].Visible = this.isLOTInclude; c1Report.Fields["lblLOTID_01"].Calculated = true;
            c1Report.Fields["lblLOTID_02"].Visible = this.isLOTInclude; c1Report.Fields["lblLOTID_02"].Calculated = true;

            // Detail
            c1Report.Fields["hLineDetail01"].Visible = this.isLOTInclude; c1Report.Fields["hLineDetail01"].Calculated = true;
            c1Report.Fields["hLineDetail02"].Visible = this.isLOTInclude; c1Report.Fields["hLineDetail02"].Calculated = true;
            c1Report.Fields["vLineDetail01"].Visible = this.isLOTInclude; c1Report.Fields["vLineDetail01"].Calculated = true;
            c1Report.Fields["vLineDetail02"].Visible = this.isLOTInclude; c1Report.Fields["vLineDetail02"].Calculated = true;
            c1Report.Fields["vLineDetail03"].Visible = this.isLOTInclude; c1Report.Fields["vLineDetail03"].Calculated = true;
            c1Report.Fields["vLineDetail04"].Visible = this.isLOTInclude; c1Report.Fields["vLineDetail04"].Calculated = true;
            c1Report.Fields["LOTID_01"].Visible = this.isLOTInclude; c1Report.Fields["LOTID_01"].Calculated = true;
            c1Report.Fields["LOTID_02"].Visible = this.isLOTInclude; c1Report.Fields["LOTID_02"].Calculated = true;
        }

        // Header 부분의 Data Binding
        private void BindingHeaderData(ref C1Report c1Report)
        {
            try
            {
                // Binding Field
                foreach (Field field in c1Report.Fields)
                {
                    switch (field.Name.ToUpper())
                    {
                        case "TXTCASSETTEID":
                            field.Text = dtHeader.AsEnumerable().Select(x => x.Field<string>("PANCAKE_GR_ID")).FirstOrDefault().ToString();
                            break;
                        case "TXTLABELPRINTDATE":
                            field.Text = DateTime.Now.ToString();
                            break;
                        case "BCRCASSETTEID":
                            field.Text = dtHeader.AsEnumerable().Select(x => x.Field<string>("PANCAKE_GR_ID")).FirstOrDefault().ToString();
                            break;
                        case "TXTLOTQTY":
                            field.Text = dtHeader.AsEnumerable().Select(x => x.Field<int>("LOT_MAPPING_COUNT")).FirstOrDefault().ToString();
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Detail 부분의 Data Binding
        private void BindingDetailData(ref C1Report c1Report)
        {
            try
            {
                if (!this.isLOTInclude)
                {
                    return;
                }

                var query = this.dtDetail.AsEnumerable().Select((x, index) => new
                {
                    INDEX = ++index,
                    LOTID = x.Field<string>("LOTID")
                }).GroupBy(grp => grp.INDEX).Select(y => new
                {
                    INDEX = (y.Key % 2 != 0) ? (y.Key / 2) + 1 : y.Key / 2,
                    LOTID_01 = y.Where(grp => grp.INDEX % 2 != 0).Max(grp => grp.LOTID),
                    LOTID_02 = y.Where(grp => grp.INDEX % 2 == 0).Max(grp => grp.LOTID)
                }).GroupBy(grp => grp.INDEX).Select(y => new
                {
                    LOTID_01 = y.Max(z => z.LOTID_01),
                    LOTID_02 = y.Max(z => z.LOTID_02)
                });

                c1Report.DataSource.Recordset = PackCommon.queryToDataTable(query.ToList()); ;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        // Print Report
        private void PrintProcess()
        {
            this.c1DocumentViewer_CST.Print();

            if (this.DialogResult == null)
            {
                return;
            }

            this.DialogResult = true;
        }

        // Print Report (Only Print)
        public C1Report PrintBatchProcess()
        {
            // Declarations..
            C1Report c1Report = new C1Report();
            try
            {
                // LOT 리스트 포함으로 보냈는데 실제 LOT 리스트가 없으면 헤더만 출력하기
                if (this.isLOTInclude && !CommonVerify.HasTableRow(this.dtDetail))
                {
                    this.isLOTInclude = false;
                }

                string reportFileName = "Pallet_Tag_CST.xml";
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (Stream stream = assembly.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + reportFileName))
                {
                    if (stream != null)
                    {
                        c1Report.Load(stream, this.REPORT_NAME);        // Load Report Definition
                        this.SetReportEntityVisible(ref c1Report);      // Field Visible / Invisible
                        this.BindingHeaderData(ref c1Report);           // Binding Header Data
                        if (this.isLOTInclude)                          // Binding Detail Data
                        {
                            this.BindingDetailData(ref c1Report);
                        }
                        c1Report.Layout.PaperSize = PaperKind.A4;       // Define Paper Size
                        c1Report.Render();                              // Rendering
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return c1Report;
        }
        #endregion

        #region Event Lists...
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.SetReport();
            this.Loaded -= Window_Loaded;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            PrintProcess();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}