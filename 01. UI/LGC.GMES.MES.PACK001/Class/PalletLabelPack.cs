/*************************************************************************************
   Created Date : 2017-01-25
   Creator      : srcadm01
   Description  : Pack 포장 정보 레포트 발행
--------------------------------------------------------------------------------------
   [Change History]
   2022.09.27  정용석   : C20220923-000277 Pallet포장 라벨 출력 기능 개선 요청
   2024.11.19  김준형   : ST Stellantis 포장라벨 추가
   2025.05.13  최평부   : E20250110-001962 모듈 Pallet Tag 양식 수정(PALLET_BOXID 추가)
**************************************************************************************/
using C1.C1Preview;
using C1.C1Report;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Xml;

namespace LGC.GMES.MES.PACK001
{
    #region C1Report Pallet Label Class List...
    // Base Class
    internal class PalletLabel
    {
        #region Member Variable List
        protected string reportDesignFileName = string.Empty;
        protected string resourceStreamName = string.Empty;
        protected List<string> lstReportName = new List<string>();
        protected DataSet ds = new DataSet();
        protected C1Report c1Report = new C1Report();
        #endregion

        #region Constructor
        public PalletLabel()
        {
            this.reportDesignFileName = "NEW_PALLET_TAG_" + this.GetType().Name.ToUpper() + ".xml";
            this.resourceStreamName = this.GetType().Namespace + ".Report." + this.reportDesignFileName;
            this.lstReportName.Add("NEW_PALLET_TAG_" + this.GetType().Name.ToUpper());
        }

        public PalletLabel(DataSet ds)
        {
            this.reportDesignFileName = "NEW_PALLET_TAG_" + this.GetType().Name.ToUpper() + ".xml";
            this.resourceStreamName = this.GetType().Namespace + ".Report." + this.reportDesignFileName;
            this.lstReportName.Add("NEW_PALLET_TAG_" + this.GetType().Name.ToUpper());
            if (CommonVerify.HasTableInDataSet(ds))
            {
                this.ds = ds;
            }
        }
        #endregion

        #region Member Function Lists...
        public C1Report LoadC1Report()
        {
            // Declarations...
            C1Report returnReport = new C1Report();
            XmlDocument xmlDocument = new XmlDocument();

            try
            {
                using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.resourceStreamName))
                {
                    if (stream == null)
                    {
                        Util.MessageValidation("SFU3637");
                        return null;
                    }

                    using (StreamReader streamReader = new StreamReader(stream))
                    {
                        xmlDocument.Load(stream);
                    }
                }

                // 선택한 ReportType에 Report가 두개 이상 있을 경우 : 각 Report별로 Report 작성 후에 Image 떠서 리턴
                if (this.lstReportName.Count > 1)
                {
                    returnReport.C1Document.PageLayout.PageSettings.LeftMargin = "0.0in";
                    returnReport.C1Document.PageLayout.PageSettings.TopMargin = "0.0in";
                    returnReport.C1Document.PageLayout.PageSettings.BottomMargin = "1.0in";

                    foreach (string reportName in this.lstReportName)
                    {
                        this.c1Report = new C1Report();
                        this.c1Report.Load(xmlDocument, reportName);    // 1. Report File 가져와서
                        this.TranslateLabelFieldValue();                // 2. Label Field 다국어처리
                        this.BindingFieldCalculate();                   // 3. Detail 영역에 데이터 Binding 되는 Field Calculated = true 치고
                        this.BindingHeaderData();                       // 4. Header Data Binding
                        this.BindingDetailData();                       // 5. Detail Data Binding
                        this.c1Report.Render();
                        foreach (Metafile metafile in this.c1Report.GetPageImages())
                        {
                            RenderImage renderImage = new RenderImage(metafile);
                            returnReport.C1Document.Body.Children.Add(renderImage);
                            returnReport.C1Document.Reflow();
                        }
                    }
                }
                // 선택한 ReportType에 Report가 한개만 경우 : 그냥 리턴
                else
                {
                    foreach (string reportName in this.lstReportName)
                    {
                        this.c1Report = new C1Report();
                        this.c1Report.Load(xmlDocument, reportName);    // 1. Report File 가져와서
                        this.TranslateLabelFieldValue();                // 2. Label Field 다국어처리
                        this.BindingFieldCalculate();                   // 3. Detail 영역에 데이터 Binding 되는 Field Calculated = true 치고
                        this.BindingHeaderData();                       // 4. Header Data Binding
                        this.BindingDetailData();                       // 5. Detail Data Binding
                        returnReport = this.c1Report;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return returnReport;
        }

        // 다국어 처리
        protected virtual void TranslateLabelFieldValue()
        {
            foreach (Field field in this.c1Report.Fields)
            {
                field.Font.Name = "돋움체";
                if (field.Name.ToUpper().StartsWith("LBL") && field.Text != null)
                {
                    List<string> lstFieldText = field.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    field.Text = lstFieldText.Select(y => ObjectDic.Instance.GetObjectName(y)).Aggregate((current, next) => current + Environment.NewLine + next);
                }
            }
        }

        // Report Field Calculated True
        protected virtual void BindingFieldCalculate()
        {
            foreach (Field field in this.c1Report.Fields)
            {
                if (field.Name.ToUpper().StartsWith("TXT"))
                {
                    field.Calculated = true;
                }
            }
        }

        // Header Data Binding
        protected virtual void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "BCDPALLETID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETID")).FirstOrDefault().ToString();
                        break;
                    case "TXTPALLETID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETID")).FirstOrDefault().ToString();
                        break;
                    case "TXTWORKINGDATE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<DateTime>("DATE")).FirstOrDefault().ToString();
                        break;
                    case "TXTTOTALBOXQTY":
                        field.Text = dt.Rows.Count.ToString();
                        break;
                    case "TXTTOTALQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString();
                        break;
                    case "TXTWORKER":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("USERID")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODID1":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PRODID")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODID2":
                        field.Text = string.Empty;
                        break;
                    case "TXTPRODIDQTY1":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODIDQTY2":
                        field.Text = string.Empty;
                        break;
                    default:
                        break;
                }
            }
        }

        // Detail Data Binding
        protected virtual void BindingDetailData()
        {

        }

        // Report Rendering
        protected virtual void C1ReportRender(C1Report c1Report)
        {
            this.c1Report.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
            this.c1Report.Render();
        }
        #endregion
    }

    // 오창, 중국, 미국, 폴란드 (Default)
    internal class CMA : PalletLabel
    {
        // 한페이지에 출력되는 최대 BoxID 갯수 40개 -> Detail Row 갯수 20개
        protected int maxDetailRowCountPerPage;
        protected int maxBoxIDCountPerPage;
        protected int offset;

        public CMA(DataSet ds) : base(ds)
        {
            this.maxDetailRowCountPerPage = 20;
            this.maxBoxIDCountPerPage = 40;
            this.offset = 500;
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            var query = dt.AsEnumerable().Select((x, index) => new
            {
                INDEX = ++index,
                PAGE_INDEX = index / maxBoxIDCountPerPage + Math.Sign(index % maxBoxIDCountPerPage),
                COLUMN_INDEX = index / maxDetailRowCountPerPage + Math.Sign(index % maxDetailRowCountPerPage),
                ROW_INDEX = (index % maxDetailRowCountPerPage == 0) ? maxDetailRowCountPerPage : index % maxDetailRowCountPerPage,

                PALLETID = x.Field<string>("PALLETID"),
                BOXID = x.Field<string>("BOXID"),
                BOXCNT = x.Field<string>("BOXCNT"),
                GBT = string.IsNullOrEmpty(x.Field<string>("GBT_CNT")) ? -1 : Convert.ToInt32(x.Field<string>("GBT_CNT")),
                MBOM = string.IsNullOrEmpty(x.Field<string>("MBOM_CNT")) ? -1 : Convert.ToInt32(x.Field<string>("MBOM_CNT"))
            }).GroupBy(grp => grp.INDEX).Select(x => new
            {
                PAGE_INDEX = x.Max(y => y.PAGE_INDEX),
                ROW_INDEX = x.Max(y => y.ROW_INDEX),
                BOXID_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.BOXID) : string.Empty,
                BOXCNT_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.BOXCNT) : string.Empty,
                GBTID_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.GBT) : -1,
                MBOM_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.MBOM) : -1,
                BOXID_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.BOXID) : string.Empty,
                BOXCNT_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.BOXCNT) : string.Empty,
                GBTID_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.GBT) : -1,
                MBOM_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.MBOM) : -1
            }).GroupBy(grp => new
            {
                grp.PAGE_INDEX,
                grp.ROW_INDEX
            }).Select((x, index) => new
            {
                INDEX = ++index,
                TXTBOXID_01 = x.Max(y => y.BOXID_01),
                TXTGBTID_01 = x.Max(y => y.GBTID_01),
                TXTMBOM_01 = x.Max(y => y.MBOM_01),
                TXTBOXLOTCOUNT_01 = x.Max(y => y.BOXCNT_01),
                TXTBOXID_02 = x.Max(y => y.BOXID_02),
                TXTGBTID_02 = x.Max(y => y.GBTID_02),
                TXTMBOM_02 = x.Max(y => y.MBOM_02),
                TXTBOXLOTCOUNT_02 = x.Max(y => y.BOXCNT_02)
            }).Select(x => new
            {
                x.TXTBOXID_01,
                TXTGBTID_01 = (x.TXTGBTID_01 > 0 && x.TXTGBTID_01 == x.TXTMBOM_01) ? "GB/T" : string.Empty,       // 2018-12-13
                x.TXTBOXLOTCOUNT_01,
                x.TXTBOXID_02,
                TXTGBTID_02 = (x.TXTGBTID_02 > 0 && x.TXTGBTID_02 == x.TXTMBOM_02) ? "GB/T" : string.Empty,       // 2018-12-13
                x.TXTBOXLOTCOUNT_02
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = 0;
            if (dtDetailData.Rows.Count % maxDetailRowCountPerPage != 0)
            {
                emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            }
            for (int i = 0; i < emptyDataRowCount; i++)
            {
                dtDetailData.Rows.Add();
            }

            // C1Report Detail 영역의 특성때문에, RecordSet을 이용한 Binding 처리를 하지 않고, 동적으로 Field 만들고 Report에 직접 그림.
            this.DrawHorizontalLine(dtDetailData);      // 가로 라인 그리고
            this.DrawVerticalLine(dtDetailData);        // 세로 라인 그리고
            this.DrawDetailData(dtDetailData);          // 항목 Binding 해서 그리기
        }

        protected virtual void DrawHorizontalLine(DataTable dtDetailData)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle();
            bool isTransparentBorder = false;
            for (int i = 0; i <= dtDetailData.Rows.Count; i++)
            {
                if (i != 0 && (i % this.maxDetailRowCountPerPage == 0))
                {
                    rectangle = new System.Drawing.Rectangle(300, i * this.offset, 10725, 1);
                }
                else
                {
                    rectangle = new System.Drawing.Rectangle(1735, i * this.offset, 9290, 1);
                }
                this.CreateFieldInDetailSection(this.c1Report.Sections[SectionTypeEnum.Detail], rectangle, "hLineDetail" + "_" + i.ToString(), string.Empty, isTransparentBorder);
            }
        }

        protected virtual void DrawVerticalLine(DataTable dtDetailData)
        {
            for (int i = 0; i < 6; i++)
            {
                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle();
                bool isTransparentBorder = false;
                switch (i)
                {
                    case 0:
                        rectangle = new System.Drawing.Rectangle(300, 0, 1, 500);
                        break;
                    case 1:
                        rectangle = new System.Drawing.Rectangle(1735, 0, 1, 500);
                        break;
                    case 2:
                        rectangle = new System.Drawing.Rectangle(5875, 0, 1, 500);
                        break;
                    case 3:
                        rectangle = new System.Drawing.Rectangle(6375, 0, 1, 500);
                        break;
                    case 4:
                        rectangle = new System.Drawing.Rectangle(10515, 0, 1, 500);
                        break;
                    case 5:
                        rectangle = new System.Drawing.Rectangle(11015, 0, 1, 500);
                        break;
                }

                for (int j = 0; j < dtDetailData.Rows.Count; j++)
                {
                    this.CreateFieldInDetailSection(this.c1Report.Sections[SectionTypeEnum.Detail], rectangle, "vLineDetail" + "_" + j.ToString(), string.Empty, isTransparentBorder);
                    rectangle.Offset(0, this.offset);
                }
            }
        }

        protected virtual void DrawDetailData(DataTable dtDetailData)
        {
            foreach (DataColumn dc in dtDetailData.Columns)
            {
                System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle();
                bool isTransparentBorder = false;
                switch (dc.ColumnName.ToUpper())
                {
                    case "TXTBOXID_01":
                        rectangle = new System.Drawing.Rectangle(1815, 0, 3400, 500); isTransparentBorder = true;
                        break;
                    case "TXTBOXID_02":
                        rectangle = new System.Drawing.Rectangle(6455, 0, 3400, 500); isTransparentBorder = true;
                        break;
                    case "TXTGBTID_01":
                        rectangle = new System.Drawing.Rectangle(5295, 0, 500, 500); isTransparentBorder = true;
                        break;
                    case "TXTGBTID_02":
                        rectangle = new System.Drawing.Rectangle(9935, 0, 500, 500); isTransparentBorder = true;
                        break;
                    case "TXTBOXLOTCOUNT_01":
                        rectangle = new System.Drawing.Rectangle(5925, 0, 400, 500); isTransparentBorder = true;
                        break;
                    case "TXTBOXLOTCOUNT_02":
                        rectangle = new System.Drawing.Rectangle(10565, 0, 400, 500); isTransparentBorder = true;
                        break;
                    default:
                        break;
                }
                List<string> lstData = new List<string>();
                lstData = dtDetailData.AsEnumerable().Select(x => x.Field<string>(dc.ColumnName)).ToList();

                for (int i = 0; i < lstData.Count; i++)
                {
                    this.CreateFieldInDetailSection(this.c1Report.Sections[SectionTypeEnum.Detail], rectangle, dc.ColumnName + "_" + i.ToString(), lstData[i], isTransparentBorder);
                    rectangle.Offset(0, this.offset);
                }
            }
        }

        protected virtual void CreateFieldInDetailSection(Section detailSection
                                                , System.Drawing.Rectangle rectangle
                                                , string fieldNamePrefix, string fieldValue, bool isTransparentBorder)
        {
            Field field = detailSection.Fields.Add(fieldNamePrefix, fieldValue, rectangle);
            field.Calculated = true;
            field.ForeColor = Colors.Black;
            field.Align = FieldAlignEnum.CenterMiddle;
            field.BorderColor = isTransparentBorder ? Colors.Transparent : Colors.Black;
            field.BorderStyle = BorderStyleEnum.Solid;
        }
    }

    // 오창
    internal class X09CMA : PalletLabel
    {
        public X09CMA(DataSet ds) : base(ds)
        {

        }

        protected override void TranslateLabelFieldValue()
        {
            // 이 Class는 다국어 처리 안함.
        }

        protected override void BindingFieldCalculate()
        {
            // 이 Class는 Detail 영역의 Binding 처리 안함.
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTPACKAGINGDATE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("DATE")).FirstOrDefault().ToString();
                        break;
                    case "TXTPARTNO":
                        field.Text = "295B93949R";
                        break;
                    case "TXTPARTNAME":
                        field.Text = "X09 CMA";
                        break;
                    case "TXTSUPPIER":
                        field.Text = "LGChem (273107)";
                        break;
                    case "TXTOUTERBOXID2":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OUTER_BOXID2")).FirstOrDefault().ToString();
                        break;
                    case "TXTQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString();
                        break;
                    default:
                        break;
                }
            }

            base.BindingHeaderData();
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 한페이지에 출력되는 최대 LOTID 갯수 63개 -> Detail Row 갯수 21개
            int maxDetailRowCountPerPage = 21;
            var query = dt.AsEnumerable().Select((x, index) => new
            {
                INDEX = index++,
                BOXID_01 = x.Field<string>("BOXID_01"),
                LOTID_01 = x.Field<string>("LOTID_01"),
                BOXID_02 = x.Field<string>("BOXID_02"),
                LOTID_02 = x.Field<string>("LOTID_02"),
                BOXID_03 = x.Field<string>("BOXID_03"),
                LOTID_03 = x.Field<string>("LOTID_03")
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            for (int i = 0; i < emptyDataRowCount; i++)
            {
                DataRow drDetailData = dtDetailData.NewRow();
                drDetailData["INDEX"] = query.Count() + i;
                drDetailData["BOXID_01"] = string.Empty;
                drDetailData["BOXID_02"] = string.Empty;
                drDetailData["BOXID_03"] = string.Empty;
                dtDetailData.Rows.Add(drDetailData);
            }

            dtDetailData.AcceptChanges();

            // C1Report Detail 영역의 특성때문에, RecordSet을 이용한 Binding 처리를 하지 않고, 동적으로 Field 만들고 Report에 직접 그림.
            // Create BOXID field
            foreach (DataColumn dc in dtDetailData.Columns)
            {
                if (!dc.ColumnName.StartsWith("BOXID"))
                {
                    continue;
                }

                int leftPosition = 0; int topPosition = 0; int width = 0; int height = 0;
                switch (dc.ColumnName.ToUpper())
                {
                    case "BOXID_01":
                        leftPosition = 300; topPosition = 0; width = 1300; height = 1590;
                        break;
                    case "BOXID_02":
                        leftPosition = 3873; topPosition = 0; width = 1300; height = 1590;
                        break;
                    case "BOXID_03":
                        leftPosition = 7446; topPosition = 0; width = 1300; height = 1590;
                        break;
                    default:
                        break;
                }
                List<string> lstData = dtDetailData.AsEnumerable().Where(x => x.Field<int>("INDEX") % 3 == 0).OrderBy(x => x.Field<int>("INDEX")).Select(x => x.Field<string>(dc.ColumnName)).ToList();
                this.CreateFieldInDetailSection(this.c1Report.Sections[SectionTypeEnum.Detail], leftPosition, topPosition, width, height, dc.ColumnName, lstData);
            }

            // Create LOTID Field
            foreach (DataColumn dc in dtDetailData.Columns)
            {
                if (!dc.ColumnName.StartsWith("LOTID"))
                {
                    continue;
                }

                int leftPosition = 0; int topPosition = 0; int width = 0; int height = 0;
                switch (dc.ColumnName.ToUpper())
                {
                    case "LOTID_01":
                        leftPosition = 1600; topPosition = 0; width = 2273; height = 530;
                        break;
                    case "LOTID_02":
                        leftPosition = 5173; topPosition = 0; width = 2273; height = 530;
                        break;
                    case "LOTID_03":
                        leftPosition = 8746; topPosition = 0; width = 2274; height = 530;
                        break;
                    default:
                        break;
                }
                List<string> lstData = dtDetailData.AsEnumerable().Select(x => x.Field<string>(dc.ColumnName)).ToList();
                this.CreateFieldInDetailSection(this.c1Report.Sections[SectionTypeEnum.Detail], leftPosition, topPosition, width, height, dc.ColumnName, lstData);
            }
        }

        private void CreateFieldInDetailSection(Section detailSection
                                              , int leftPosition, int topPosition, int width, int height
                                              , string fieldNamePrefix, List<string> lstData)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(leftPosition, topPosition, width, height);
            for (int i = 0; i < lstData.Count; i++)
            {
                Field field = detailSection.Fields.Add(fieldNamePrefix + "_" + i.ToString(), lstData[i], rectangle);
                field.Calculated = false;
                field.ForeColor = Colors.Black;
                field.Align = FieldAlignEnum.CenterMiddle;
                field.BorderColor = Colors.Black;
                field.BorderStyle = BorderStyleEnum.Solid;
                rectangle.Offset(0, rectangle.Height);
            }
        }
    }

    // 오창
    internal class B10CMA : PalletLabel
    {
        public B10CMA(DataSet ds) : base(ds)
        {

        }

        protected override void TranslateLabelFieldValue()
        {
            // 이 Class는 다국어 처리 안함.
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTPACKAGINGDATE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("DATE")).FirstOrDefault().ToString();
                        break;
                    case "TXTPARTNO":
                        field.Text = "295B93949R";
                        break;
                    case "TXTPARTNAME":
                        field.Text = "B10 CMA";
                        break;
                    case "TXTSUPPIER":
                        field.Text = "LGChem (273107)";
                        break;
                    case "TXTOUTERBOXID2":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OUTER_BOXID2")).FirstOrDefault().ToString();
                        break;
                    case "TXTQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString() + " EA";
                        break;
                    default:
                        break;
                }
            }

            base.BindingHeaderData();
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 한페이지에 출력되는 최대 BoxID 갯수 24개 -> Detail Row 갯수 24개
            int maxDetailRowCountPerPage = 24;
            List<string> lstBoxID = dt.AsEnumerable().GroupBy(grp => grp.Field<string>("BOXID")).Select(x => x.Key).ToList();
            var query = dt.AsEnumerable().Select((x, index) => new
            {
                TXTBOXID = string.Empty,
                TXTNO = ++index,
                TXTCMAID = x.Field<string>("LOTID")
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            if (emptyDataRowCount != maxDetailRowCountPerPage)
            {
                for (int i = 0; i < emptyDataRowCount; i++)
                {
                    dtDetailData.Rows.Add();
                }
            }

            // BoxID Column에 Box List 집어넣기
            int startIndexforInsertBoxID = (maxDetailRowCountPerPage - lstBoxID.Count) / 2;
            int endIndexForInsertBoxID = startIndexforInsertBoxID + lstBoxID.Count - 1;
            int rowIndex = 0;
            int currentBoxIDIndex = 0;
            while (true)
            {
                int checkIndex = rowIndex % maxDetailRowCountPerPage;
                if (checkIndex <= 0)
                {
                    currentBoxIDIndex = 0;
                }

                if (rowIndex >= dtDetailData.Rows.Count)
                {
                    break;
                }

                if (checkIndex < startIndexforInsertBoxID || checkIndex > endIndexForInsertBoxID)
                {
                    rowIndex++;
                    continue;
                }

                dtDetailData.Rows[rowIndex++]["TXTBOXID"] = lstBoxID[currentBoxIDIndex++];
            }

            dtDetailData.AcceptChanges();

            this.c1Report.DataSource.Recordset = dtDetailData;
        }
    }

    // 오창
    internal class PORSCHE12V : PalletLabel
    {
        public PORSCHE12V(DataSet ds) : base(ds)
        {

        }

        protected override void TranslateLabelFieldValue()
        {
            // 이 Class는 다국어 처리 안함.
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            base.BindingHeaderData();

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTPACKAGINGDATE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("DATE")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PRODID")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODUCTINDEX":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PRODUCT_INDEX")).FirstOrDefault().SafeToString();
                        break;
                    case "TXTSUPPIER":
                        field.Text = "LGChem (273107)";
                        break;
                    case "TXTOUTERBOXID2":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OUTER_BOXID2")).FirstOrDefault().SafeToString();
                        break;
                    case "TXTQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString() + " EA";
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 한페이지에 출력되는 최대 BoxID 갯수 24개 -> Detail Row 갯수 24개
            int maxDetailRowCountPerPage = 24;
            List<string> lstBoxID = dt.AsEnumerable().GroupBy(grp => grp.Field<string>("BOXID")).Select(x => x.Key).ToList();
            var query = dt.AsEnumerable().Select((x, index) => new
            {
                TXTBOXID = string.Empty,
                TXTNO = ++index,
                TXTCMAID = x.Field<string>("LOTID")
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            if (emptyDataRowCount != maxDetailRowCountPerPage)
            {
                for (int i = 0; i < emptyDataRowCount; i++)
                {
                    dtDetailData.Rows.Add();
                }
            }

            // BoxID Column에 Box List 집어넣기
            int startIndexforInsertBoxID = (maxDetailRowCountPerPage - lstBoxID.Count) / 2;
            int endIndexForInsertBoxID = startIndexforInsertBoxID + lstBoxID.Count - 1;
            int rowIndex = 0;
            int currentBoxIDIndex = 0;
            while (true)
            {
                int checkIndex = rowIndex % maxDetailRowCountPerPage;
                if (checkIndex <= 0)
                {
                    currentBoxIDIndex = 0;
                }

                if (rowIndex >= dtDetailData.Rows.Count)
                {
                    break;
                }

                if (checkIndex < startIndexforInsertBoxID || checkIndex > endIndexForInsertBoxID)
                {
                    rowIndex++;
                    continue;
                }

                dtDetailData.Rows[rowIndex++]["TXTBOXID"] = lstBoxID[currentBoxIDIndex++];
            }

            dtDetailData.AcceptChanges();

            this.c1Report.DataSource.Recordset = dtDetailData;
        }
    }

    // 오창
    internal class BMW12V : PalletLabel
    {
        public BMW12V(DataSet ds) : base(ds)
        {

        }

        protected override void TranslateLabelFieldValue()
        {
            // 이 Class는 다국어 처리 안함.
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTPACKAGINGDATE":
                        field.Text = Convert.ToDateTime(dt.AsEnumerable().Select(x => x.Field<string>("DATE")).FirstOrDefault()).ToString("yyyyMMdd hh:mm:ss");
                        break;
                    case "TXTPRODID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PRODID")).FirstOrDefault().ToString();
                        break;
                    case "TXTPRODUCTINDEX":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PRODUCT_INDEX")).FirstOrDefault().SafeToString();
                        break;
                    case "TXTSUPPIER":
                        field.Text = "LGChem (273107)";
                        break;
                    case "TXTOUTERBOXID2":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OUTER_BOXID2")).FirstOrDefault().ToString();
                        break;
                    case "TXTQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString() + " EA";
                        break;
                    default:
                        break;
                }
            }

            base.BindingHeaderData();
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 한페이지에 출력되는 최대 BoxID 갯수 48개 -> Detail Row 갯수 24개
            int maxDetailRowCountPerPage = 24;
            int maxBoxIDCountPerPage = 48;
            List<string> lstBoxID = dt.AsEnumerable().GroupBy(grp => grp.Field<string>("BOXID")).Select(x => x.Key).ToList();

            var query = dt.AsEnumerable().Select((x, index) => new
            {
                INDEX = ++index,
                PAGE_INDEX = index / maxBoxIDCountPerPage + Math.Sign(index % maxBoxIDCountPerPage),
                COLUMN_INDEX = index / maxDetailRowCountPerPage + Math.Sign(index % maxDetailRowCountPerPage),
                ROW_INDEX = (index % maxDetailRowCountPerPage == 0) ? maxDetailRowCountPerPage : index % maxDetailRowCountPerPage,

                NO = index.ToString(),
                BMAID = x.Field<string>("LOTID")
            }).GroupBy(grp => grp.INDEX).Select(x => new
            {
                PAGE_INDEX = x.Max(y => y.PAGE_INDEX),
                ROW_INDEX = x.Max(y => y.ROW_INDEX),

                NO_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.NO) : string.Empty,
                BMAID_01 = (x.Max(y => y.COLUMN_INDEX) % 2 == 1) ? x.Max(y => y.BMAID) : string.Empty,

                NO_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.NO) : string.Empty,
                BMAID_02 = (x.Max(y => y.COLUMN_INDEX) % 2 == 0) ? x.Max(y => y.BMAID) : string.Empty,
            }).GroupBy(grp => new
            {
                grp.PAGE_INDEX,
                grp.ROW_INDEX
            }).Select((x, index) => new
            {
                INDEX = ++index,
                TXTBOXID = string.Empty,
                TXTNO01 = x.Max(y => y.NO_01),
                TXTBMAID01 = x.Max(y => y.BMAID_01),
                TXTNO02 = x.Max(y => y.NO_02),
                TXTBMAID02 = x.Max(y => y.BMAID_02)
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            if (emptyDataRowCount != maxDetailRowCountPerPage)
            {
                for (int i = 0; i < emptyDataRowCount; i++)
                {
                    dtDetailData.Rows.Add();
                }
            }

            // BoxID Column에 Box List 집어넣기
            int startIndexforInsertBoxID = (maxDetailRowCountPerPage - lstBoxID.Count) / 2;
            int endIndexForInsertBoxID = startIndexforInsertBoxID + lstBoxID.Count - 1;
            int rowIndex = 0;
            int currentBoxIDIndex = 0;
            while (true)
            {
                int checkIndex = rowIndex % maxDetailRowCountPerPage;
                if (checkIndex <= 0)
                {
                    currentBoxIDIndex = 0;
                }

                if (rowIndex >= dtDetailData.Rows.Count)
                {
                    break;
                }

                if (checkIndex < startIndexforInsertBoxID || checkIndex > endIndexForInsertBoxID)
                {
                    rowIndex++;
                    continue;
                }

                dtDetailData.Rows[rowIndex++]["TXTBOXID"] = lstBoxID[currentBoxIDIndex++];
            }

            dtDetailData.AcceptChanges();

            this.c1Report.DataSource.Recordset = dtDetailData;
        }
    }

    // 오창
    internal class FORD48V : PalletLabel
    {
        public FORD48V(DataSet ds) : base(ds)
        {

        }

        protected override void TranslateLabelFieldValue()
        {
            // 이 Class는 다국어 처리 안함.
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTPACKAGINGDATE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("DATE")).FirstOrDefault().ToString();
                        break;
                    case "TXTPARTNO":
                        field.Text = string.IsNullOrEmpty(dt.AsEnumerable().Select(x => x.Field<string>("PARTNO")).FirstOrDefault()) ? string.Empty : dt.AsEnumerable().Select(x => x.Field<string>("PARTNO")).FirstOrDefault().ToString();
                        break;
                    case "TXTPARTNAME":
                        field.Text = "Ford 48V";
                        break;
                    case "TXTOUTERBOXID2":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OUTER_BOXID2")).FirstOrDefault().ToString();
                        break;
                    case "TXTQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLETCNT")).FirstOrDefault().ToString() + " EA";
                        break;
                    default:
                        break;
                }
            }

            base.BindingHeaderData();
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 한페이지에 출력되는 최대 BoxID 갯수 24개 -> Detail Row 갯수 24개
            int maxDetailRowCountPerPage = 24;
            List<string> lstBoxID = dt.AsEnumerable().GroupBy(grp => grp.Field<string>("BOXID")).Select(x => x.Key).ToList();
            var query = dt.AsEnumerable().Select((x, index) => new
            {
                TXTBOXID = string.Empty,
                TXTNO = ++index,
                TXTCMAID = x.Field<string>("LOTID")
            });

            DataTable dtDetailData = PackCommon.queryToDataTable(query.ToList());

            // 마지막 페이지 여백 처리
            int emptyDataRowCount = (dtDetailData.Rows.Count <= maxDetailRowCountPerPage) ? maxDetailRowCountPerPage - dtDetailData.Rows.Count : maxDetailRowCountPerPage - (dtDetailData.Rows.Count % maxDetailRowCountPerPage);
            if (emptyDataRowCount != maxDetailRowCountPerPage)
            {
                for (int i = 0; i < emptyDataRowCount; i++)
                {
                    dtDetailData.Rows.Add();
                }
            }

            // BoxID Column에 Box List 집어넣기
            int startIndexforInsertBoxID = (maxDetailRowCountPerPage - lstBoxID.Count) / 2;
            int endIndexForInsertBoxID = startIndexforInsertBoxID + lstBoxID.Count - 1;
            int rowIndex = 0;
            int currentBoxIDIndex = 0;
            while (true)
            {
                int checkIndex = rowIndex % maxDetailRowCountPerPage;
                if (checkIndex <= 0)
                {
                    currentBoxIDIndex = 0;
                }

                if (rowIndex >= dtDetailData.Rows.Count)
                {
                    break;
                }

                if (checkIndex < startIndexforInsertBoxID || checkIndex > endIndexForInsertBoxID)
                {
                    rowIndex++;
                    continue;
                }

                dtDetailData.Rows[rowIndex++]["TXTBOXID"] = lstBoxID[currentBoxIDIndex++];
            }

            dtDetailData.AcceptChanges();

            this.c1Report.DataSource.Recordset = dtDetailData;
        }
    }

    // 폴란드
    internal class MEBCMA : CMA
    {
        public MEBCMA(DataSet ds) : base(ds)
        {
            this.maxDetailRowCountPerPage = 17;
            this.maxBoxIDCountPerPage = 34;
            this.offset = 500;
        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            // 하양색 바탕에 검은색 글자 색깔로 단표시 박스 깔고, BOXSEQ에 해당하는 BOX만 음영처리
            for (int i = 1; i <= 7; i++)
            {
                string columnName = "txtLayer" + i.ToString();
                this.c1Report.Fields[columnName].BackColor = Colors.White;
                this.c1Report.Fields[columnName].ForeColor = Colors.Black;
            }

            string boxSeqence = dt.AsEnumerable().GroupBy(x => x.Field<string>("BOXSEQ")).Select(x => x.Key).FirstOrDefault();
            if (!string.IsNullOrEmpty(boxSeqence) && !boxSeqence.Equals("0"))
            {
                string reverseColorFieldName = "txtLayer" + boxSeqence;
                if (!string.IsNullOrEmpty(reverseColorFieldName))
                {
                    this.c1Report.Fields[reverseColorFieldName].BackColor = Colors.Black;
                    this.c1Report.Fields[reverseColorFieldName].ForeColor = Colors.White;
                }
            }

            base.BindingHeaderData();
        }
    }

    // 폴란드
    internal class BT6 : CMA
    {
        public BT6(DataSet ds) : base(ds)
        {

        }

        protected override void BindingDetailData()
        {
            DataTable dtBindData = new DataTable();
            DataTable dt = this.ds.Tables["OUTDATA"];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dtBindData.Columns.Add("TXTBOXID_" + (i + 1).ToString("D2"), typeof(string));
                dtBindData.Columns.Add("TXTGBTID_" + (i + 1).ToString("D2"), typeof(string));
                dtBindData.Columns.Add("TXTBOXLOTCOUNT_" + (i + 1).ToString("D2"), typeof(string));
                dtBindData.Columns.Add("EOLDATABCR" + (i + 1).ToString("D2"), typeof(Array));
                dtBindData.Columns.Add("EOLDATA" + (i + 1).ToString("D2"), typeof(string));
            }

            // N개 들어가는거...
            int index = 0;
            DataRow drBindData = dtBindData.NewRow();
            foreach (DataRowView dataRowView in dt.AsDataView())
            {
                string columnName = string.Empty;
                columnName = "TXTBOXID_" + (index + 1).ToString("D2");
                drBindData[columnName] = dataRowView["BOXID"].ToString();
                columnName = "TXTBOXLOTCOUNT_" + (index + 1).ToString("D2");
                drBindData[columnName] = dataRowView["BOXCNT"].ToString();
                columnName = "EOLDATABCR" + (index + 1).ToString("D2");
                drBindData[columnName] = PackCommon.GetQRCode(dataRowView["EOLDATA"].ToString());
                columnName = "EOLDATA" + (index + 1).ToString("D2");
                drBindData[columnName] = dataRowView["BOXID"].ToString();
                if (Convert.ToInt32(dataRowView["GBT_CNT"].ToString()).Equals(Convert.ToInt32(dataRowView["MBOM_CNT"].ToString())))
                {
                    columnName = "TXTGBTID_" + (index + 1).ToString("D2");
                    drBindData[columnName] = "GB/T";
                }
                index++;
            }

            dtBindData.Rows.Add(drBindData);

            for (int columnIndex = 0; columnIndex < dtBindData.Columns.Count; columnIndex++)
            {
                string columnName = dtBindData.Columns[columnIndex].ColumnName;
                if (this.c1Report.Fields.Contains(columnName))
                {
                    if (columnName.Contains("EOLDATABCR"))
                    {
                        ImageConverter imageConverter = new System.Drawing.ImageConverter();
                        Image image = (Image)imageConverter.ConvertFrom(dtBindData.Rows[0][columnName]);
                        this.c1Report.Fields[columnName].Picture = (image == null) ? (Image)imageConverter.ConvertFrom("") : image;
                    }
                    else
                    {
                        this.c1Report.Fields[columnName].Text = dtBindData.Rows[0][columnName] == null ? "" : dtBindData.Rows[0][columnName].ToString();
                    }
                }
            }
        }
    }

    // 폴란드
    internal class C727EOL : BT6
    {
        public C727EOL(DataSet ds) : base(ds)
        {

        }
    }

    // 중국 - ISUZU
    internal class ISUZU : PalletLabel
    {
        public ISUZU(DataSet ds) : base(ds)
        {

        }

        protected override void BindingHeaderData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];

            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTMODEL":
                        field.Text = "ISUZU MODULE" + "(" + dt.AsEnumerable().Select(x => x.Field<string>("PRODID")).FirstOrDefault().ToString() + ")";
                        break;
                    case "TXTBOXID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("BOXID")).FirstOrDefault().ToString();
                        break;
                    case "TXTQUANTITY":
                        field.Text = dt.Rows.Count.ToString();
                        break;
                    case "TXTNETWEIGHT":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("ITEM01")).FirstOrDefault().ToString();
                        break;
                    case "TXTGROSSWEIGHT":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("ITEM02")).FirstOrDefault().ToString();
                        break;
                    case "TXTMINENERGY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("ITEM03")).FirstOrDefault().ToString();
                        break;
                    case "TXTVOLTAGE":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("ITEM05")).FirstOrDefault().ToString();
                        break;
                    case "TXTPACKINGDATE":
                        field.Text = DateTime.Parse(dt.AsEnumerable().Select(x => x.Field<string>("PACKDTTM")).FirstOrDefault().ToString()).ToString("yyyy.MM.dd");
                        break;
                    default:
                        break;
                }
            }

            base.BindingHeaderData();
        }

        protected override void BindingDetailData()
        {
            DataTable dt = this.ds.Tables["OUTDATA"];
            var query = dt.AsEnumerable().Select((x, index) => new
            {
                TXTNO = ++index,
                TXTMODULEID = x.Field<string>("LOTID"),
                TXTCELLLOT = x.Field<string>("PKG_LOTID")
            });

            this.c1Report.DataSource.Recordset = PackCommon.queryToDataTable(query.ToList());
        }
    }

    // 폴란드 : Report Design이 CMA와 유사한 관계로 CMA Class로부터 상속
    internal class CMAEV2020 : CMA
    {
        public CMAEV2020(DataSet ds) : base(ds)
        {
            this.maxDetailRowCountPerPage = 18;
            this.maxBoxIDCountPerPage = 36;
            this.offset = 500;
        }

        protected override void BindingHeaderData()
        {
            base.BindingHeaderData();

            DataTable dt = this.ds.Tables["OUTDATA"];
            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TXTCARRIERID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("CSTID")).FirstOrDefault().ToString();
                        break;
                    case "TXTTOTALBOXQTY":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("BOXCNT")).FirstOrDefault().ToString();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    // 캐나다 : Report Design이 CMA와 유사한 관계로 CMA Class로부터 상속
    internal class ST : CMA
    {
        public ST(DataSet ds) : base(ds)
        {

        }

        protected override void BindingHeaderData()
        {
            base.BindingHeaderData();

            DataTable dt = this.ds.Tables["OUTDATA"];
            foreach (Field field in this.c1Report.Fields)
            {
                switch (field.Name.ToUpper())
                {
                    case "TAG_ID":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("TAG_ID")).FirstOrDefault().ToString();
                        break;
                    case "OQC_REQ_FLAG":
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("OQC_REQ_FLAG")).FirstOrDefault().ToString();
                        break;
                    case "PALLET_BOXID": //2025.05.13  최평부   : E20250110-001962 모듈 Pallet Tag 양식 수정
                        field.Text = dt.AsEnumerable().Select(x => x.Field<string>("PALLET_BOXID")).FirstOrDefault().ToString();
                        break;
                    default:
                        break;
                }
            }
        }
    }
    #endregion
}