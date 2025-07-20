/*************************************************************************************
 Created Date : 2017-01-25
 Creator :
 Decription : Pack 포장 정보 레포트 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.25  srcadm01 : Initial Created.
  2017.04.06  장만철
  2017.07.13  장만철
  2018.04.18  손우석   CSR ID 3665550 팩 11호기 BMW12V 35UP Pallet 인쇄양식 변경요청의건 요청번호 C20180418_65550
  2018.09.27  손우석   CSR ID 3794696 CSR요구사항 정의서 - 팩11호 팔렛트ID 출력 시 출력정보 오류 개선 및 변경 요청번호 C20180915_94696
  2018.10.15  손우석   CSR ID 3794696 CSR요구사항 정의서 - 팩11호 팔렛트ID 출력 시 출력정보 오류 개선 및 변경 요청번호 C20180915_94696
  2018.11.08  손우석   CSR ID 3835126 오창 팩11호 BMW12V Pallet 정보 Sheet 출력 양식 개선 요청 요청번호 20181105_35126
  2018.12.06  손우석   CSR ID 3835126 오창 팩11호 BMW12V Pallet 정보 Sheet 출력 양식 개선 요청 요청번호 20181105_35126
  2018.12.13  손우석   CSR ID 3859317 [G.MES] Audi BEV C-sample G.MES G/BT 모듈 출하 식별 기능 추가 요청의 건 [요청번호] C20181201_59317
  2019.02.26  손우석   CSR ID 3932728 GMES Pallet 발행 식별표 추가 요청_신규모델 [요청번호] C20190225_32728
  2019.11.05  손우석   CSR ID 1840 PALLET Tag 양식 변경 요청 [요청번호] C20191101-000145 [서비스 번호] 1840
  2019.11.18  염규범   오류건 수정
  2019.12.05  손우석   CSR ID 10840 MOKA Pallet출력용지 오류개선 요청 건 [요청번호] C20191206-000023
  2020.06.05  김민석   CSR ID 61625 GMES 시스템의 전진검사 효율화를 위한 바코드 출력 기능 변경(건) [요청번호] C20200519-000003
  2020.06.23  최우석   CSR ID 61613, 라인 입고 후 Cell 동간 이동 처리 팔레트 조회 및 성능 개선 [요청번호] C20200518-000493
  2020.07.03  최우석   CSR ID 61613, 라인 입고 후 Cell 동간 이동 처리 팔레트 조회 및 성능 개선 [요청번호] C20200518-000493 Decimal 형변환 오류 수정
  2020.10.14  염규범   Pallet 용지 출력시, ZPL 라벨지 추가적으로 인쇄 처리 여부 PACK_UI_PALLET_ZPL_PRINT 에 LoginInfo.CFG_SHOP_ID 구분 처리
  2021.08.27  정용석   MEB 7단 포장 라벨의 경우 최초 발행은 못하게 INTERLOCK
  2021.11.15  김길용   Pack 3동 포장기 라벨 추가(CarrierID 추가)
  2021.11.23  김길용   배포 이슈로인해 이전 버전으로 롤백처리
  2022.03.24  정용석   RSA BT6향 포장라벨 추가
  2022.08.04  임성운   NJ ISUZU용 포장라벨 추가
  2022.09.21  임성운   양식 자동이동 기능 개발(tagname을 Parameter로 받아와서 해당 양식으로 이동) ESWA 테스트 Holding
  2024.09.04  최평부   SI ESST용 포장라벨 추가
  2025.06.10  임성운   CX727 MI에서도 보이도록 수정 

**************************************************************************************/
using C1.WPF;
using C1.WPF.C1Report;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using QRCoder;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class Pallet_Tag : Window, IWorkArea
    {
        MESSAGE_PARAM messageParam = new MESSAGE_PARAM();
        object[] tmps;
        string[] reports_name;
        string window_name = string.Empty;
        string palletID = string.Empty;
        C1DocumentViewer param_c1DocumentViewer; //전달 받은 내용
        C1DocumentViewer seleted_c1DocumentViewer; //선택한 tab 내용
        string eqsgid = string.Empty;
        string model = string.Empty;
        //        string tagname = string.Empty; ESWA 테스트 Holding

        private bool strChkZplPrint = false;        // ZPL 프린트 확인 여부
        private Util _Util = new Util();
        string strDpi = string.Empty;               //임시 DPI
        DataSet dsPalletData = new DataSet();
        DataTable dtPalletHistory = new DataTable();
        DataTable dtBindData = new DataTable();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Constructor
        public Pallet_Tag()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        // 2021.08.27  정용석   MEB 7단 포장 라벨의 경우 최초 발행은 못하게 INTERLOCK
        private bool CheckMEBPalletPrintYN(string palletID)
        {
            bool returnValue = true;
            DataSet dsINDATA = new DataSet();
            DataTable dtINDATA = new DataTable("INDATA");
            dtINDATA.Columns.Add("LANGID", typeof(string));
            dtINDATA.Columns.Add("SHOPID", typeof(string));
            dtINDATA.Columns.Add("AREAID", typeof(string));
            dtINDATA.Columns.Add("PALLETID", typeof(string));
            dtINDATA.Rows.Add(new object[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID, LoginInfo.CFG_AREA_ID, palletID });
            dsINDATA.Tables.Add(dtINDATA);
            DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_GET_PLT_TAG_FOR_LOGIS", "INDATA", "OUTDATA", dsINDATA);
            if (CommonVerify.HasTableInDataSet(ds))
            {
                if (CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                {
                    returnValue = ds.Tables["OUTDATA"].Rows[0]["ISPRINTYN"].ToString().Equals("Y") ? true : false;
                }
            }
            return returnValue;
        }

        private void setReport(string gubun = "CMA")
        {
            C1.C1Report.C1Report c1Report = new C1.C1Report.C1Report();
            c1Report.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;
            string filename = string.Empty;
            string reportname = string.Empty;
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            this.param_c1DocumentViewer = new C1.WPF.C1Report.C1DocumentViewer();
            reportname = "Pallet_Tag_" + gubun;

            if (this.dtBindData == null || this.dtBindData.Rows.Count == 0)
            {
                return;
            }

            switch (gubun)
            {
                case "CMA":
                    param_c1DocumentViewer = c1DocumentViewer_CMA;
                    if (Convert.ToInt32(this.dtPalletHistory.Rows.Count) > 40)
                    {
                        reportname = "Pallet_Tag_" + gubun + "_ADDROW";
                    }
                    break;
                case "X09CMA":
                    param_c1DocumentViewer = c1DocumentViewer_X09CMA;
                    break;
                case "YFCMA":
                    break;
                case "B10CMA":
                    param_c1DocumentViewer = c1DocumentViewer_B10CMA;
                    if (Convert.ToInt32(this.dtPalletHistory.Rows.Count) > 24)
                    {
                        reportname = "Pallet_Tag_" + gubun + "_ADDROW";
                    }
                    break;
                case "313HBMA":
                    break;
                case "315HCMA":
                    break;
                case "PL65":
                    param_c1DocumentViewer = c1DocumentViewer_PL65;
                    break;
                case "Porsche12V":
                    param_c1DocumentViewer = c1DocumentViewer_BMW_PORCHE;
                    reportname = "Pallet_Tag_BMW_PORCHE";
                    break;
                case "BMW12V":
                    param_c1DocumentViewer = c1DocumentViewer_BMW12V;
                    reportname = "Pallet_Tag_BMW_ADDROW";
                    break;
                case "Ford48V":
                    param_c1DocumentViewer = c1DocumentViewer_Ford48V;
                    reportname = "Pallet_Tag_Ford48";
                    break;
                case "C727EOL":
                    param_c1DocumentViewer = c1DocumentViewer_C727EOL;
                    reportname = "Pallet_Tag_C727EOL";
                    break;
                case "MEBCMA":
                    param_c1DocumentViewer = c1DocumentViewer_MEBCMA;
                    if (Convert.ToInt32(this.dtPalletHistory.Rows.Count) > 34)
                    {
                        reportname = "Pallet_Tag_" + gubun + "_ADDROW";
                    }
                    break;
                case "BT6":
                    param_c1DocumentViewer = c1DocumentViewer_BT6;
                    reportname = "Pallet_Tag_BT6";
                    break;
                case "ISUZU":
                    param_c1DocumentViewer = c1DocumentViewer_ISUZU;
                    reportname = "Pallet_Tag_ISUZU";
                    break;
                case "ST": // 2024.09.04  최평부   SI ESST용 포장라벨 추가
                    param_c1DocumentViewer = c1DocumentViewer_ST;
                    reportname = "Pallet_Tag_ST";
                    break;
            }

            filename = reportname + ".xml";

            // Report별 Data Binding
            using (Stream stream = assembly.GetManifestResourceStream("LGC.GMES.MES.PACK001.Report." + filename))
            {
                if (stream != null)
                {
                    c1Report.Load(stream, reportname);
                    //레포트에 Value Binding
                    for (int col = 0; col < this.dtBindData.Columns.Count; col++)
                    {
                        string strColName = this.dtBindData.Columns[col].ColumnName;
                        if (c1Report.Fields.Contains(strColName))
                        {
                            if (strColName.Contains("EOLDATABCR"))
                            {
                                ImageConverter imgcvt = new System.Drawing.ImageConverter();
                                Image image = (Image)imgcvt.ConvertFrom(this.dtBindData.Rows[0][strColName]);
                                c1Report.Fields[strColName].Picture = image == null ? (Image)imgcvt.ConvertFrom("") : image;
                            }
                            else
                            {
                                c1Report.Fields[strColName].Text = this.dtBindData.Rows[0][strColName] == null ? "" : this.dtBindData.Rows[0][strColName].ToString();
                            }
                        }
                    }
                    // 2021-07-26 MEBCMA 7단포장위치 음영표시
                    if (gubun.Equals("MEBCMA"))
                    {
                        // 하양색 바탕에 검은색 글자 색깔로 단표시 박스 깔고,  BOXSEQ에 해당하는 BOX만 음영처리
                        for (int i = 1; i <= 7; i++)
                        {
                            string columnName = "txtRow" + i.ToString();
                            c1Report.Fields[columnName].BackColor = Colors.White;
                            c1Report.Fields[columnName].ForeColor = Colors.Black;
                        }
                        // select top 1 * from this.dtBindData;
                        foreach (var item in this.dtBindData.AsEnumerable().Take(1))
                        {
                            if (!string.IsNullOrEmpty(item.Field<string>("BOXSEQ")))
                            {
                                string reverseColorColumnName = "txtRow" + item.Field<string>("BOXSEQ");
                                c1Report.Fields[reverseColorColumnName].BackColor = Colors.Black;
                                c1Report.Fields[reverseColorColumnName].ForeColor = Colors.White;
                            }
                        }
                    }
                    //Language Binding : 다국어 처리
                    for (int fieldIndex = 0; fieldIndex < c1Report.Fields.Count; fieldIndex++)
                    {
                        if (c1Report.Fields[fieldIndex].Text != null)
                        {
                            switch (gubun)
                            {
                                case "CMA":
                                    if (c1Report.Fields[fieldIndex].Name == null || c1Report.Fields[fieldIndex].Name == "1" || c1Report.Fields[fieldIndex].Name == "11" ||
                                    c1Report.Fields[fieldIndex].Name == "11" || c1Report.Fields[fieldIndex].Name == "12" || c1Report.Fields[fieldIndex].Name == "13" ||
                                    c1Report.Fields[fieldIndex].Name == "14" || c1Report.Fields[fieldIndex].Name == "15" || c1Report.Fields[fieldIndex].Name == "16" ||
                                    c1Report.Fields[fieldIndex].Name == "18" || c1Report.Fields[fieldIndex].Name == "19" || c1Report.Fields[fieldIndex].Name == "111" ||
                                    c1Report.Fields[fieldIndex].Name == "112" || c1Report.Fields[fieldIndex].Name == "114" || c1Report.Fields[fieldIndex].Name == "115" ||
                                    c1Report.Fields[fieldIndex].Name == "118" || c1Report.Fields[fieldIndex].Name == "17" || c1Report.Fields[fieldIndex].Name == "110" || c1Report.Fields[fieldIndex].Name == "124" || c1Report.Fields[fieldIndex].Name == "125"
                                    )
                                    {
                                        if (c1Report.Fields[fieldIndex].Name == "1")
                                        {
                                            string[] temp = c1Report.Fields[fieldIndex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                            if (temp.Length <= 1)
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                        }
                                    }
                                    break;
                                case "X09CMA":
                                    break;
                                case "YFCMA":
                                    break;
                                case "B10CMA":
                                    break;
                                case "313HBMA":
                                    break;
                                case "315HCMA":
                                    break;
                                case "PL65":
                                    if (c1Report.Fields[fieldIndex].Name == null)
                                    {
                                        c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                    }
                                    break;
                                case "PORSCHE12V":
                                    if (eqsgid == "P2Q11")
                                    {
                                        if (c1Report.Fields[fieldIndex].Name == "22" || c1Report.Fields[fieldIndex].Name == "OUTER_BOXID2")
                                        {
                                            c1Report.Fields[fieldIndex].Visible = true;
                                        }
                                        if (model.Length > 0)
                                        {
                                            if (model == "PORSHCE")
                                            {
                                                if (c1Report.Fields[fieldIndex].Name == "TITLE")
                                                {
                                                    c1Report.Fields[fieldIndex].Text = "PORSCHE 12V BMA ID Packaging Sheet";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (c1Report.Fields[fieldIndex].Name == "TITLE")
                                            {
                                                c1Report.Fields[fieldIndex].Text = "PORSCHE 12V BMA ID Packaging Sheet";
                                            }
                                        }
                                    }
                                    break;
                                case "BMW12V":
                                    if (c1Report.Fields[fieldIndex].Name == "TITLE")
                                    {
                                        c1Report.Fields[fieldIndex].Text = "BMW 12V BMA ID Packaging Sheet";
                                    }
                                    break;
                                case "Ford48V":
                                    break;
                                case "C727EOL":
                                    if (c1Report.Fields[fieldIndex].Name == null || c1Report.Fields[fieldIndex].Name == "1" || c1Report.Fields[fieldIndex].Name == "2" ||
                                        c1Report.Fields[fieldIndex].Name == "3" || c1Report.Fields[fieldIndex].Name == "4" || c1Report.Fields[fieldIndex].Name == "5" ||
                                        c1Report.Fields[fieldIndex].Name == "6" || c1Report.Fields[fieldIndex].Name == "7" || c1Report.Fields[fieldIndex].Name == "8" ||
                                        c1Report.Fields[fieldIndex].Name == "9" || c1Report.Fields[fieldIndex].Name == "10" || c1Report.Fields[fieldIndex].Name == "11" ||
                                        c1Report.Fields[fieldIndex].Name == "12" || c1Report.Fields[fieldIndex].Name == "13" || c1Report.Fields[fieldIndex].Name == "14" ||
                                        c1Report.Fields[fieldIndex].Name == "15" || c1Report.Fields[fieldIndex].Name == "16" || c1Report.Fields[fieldIndex].Name == "17" ||
                                        c1Report.Fields[fieldIndex].Name == "18" || c1Report.Fields[fieldIndex].Name == "19" || c1Report.Fields[fieldIndex].Name == "20" ||
                                        c1Report.Fields[fieldIndex].Name == "21" || c1Report.Fields[fieldIndex].Name == "22" || c1Report.Fields[fieldIndex].Name == "23" ||
                                        c1Report.Fields[fieldIndex].Name == "24" || c1Report.Fields[fieldIndex].Name == "25" || c1Report.Fields[fieldIndex].Name == "26" ||
                                        c1Report.Fields[fieldIndex].Name == "27"
                                        )
                                    {
                                        if (c1Report.Fields[fieldIndex].Name == "1")
                                        {
                                            string[] temp = c1Report.Fields[fieldIndex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                            if (temp.Length <= 1)
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                        }
                                    }
                                    break;
                                case "MEBCMA":
                                    if (c1Report.Fields[fieldIndex].Name.ToUpper().Contains("LABEL"))
                                    {
                                        string strResult = string.Empty;
                                        switch (c1Report.Fields[fieldIndex].Name.ToUpper())
                                        {
                                            case "LABEL1":
                                                string[] temp = c1Report.Fields[fieldIndex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                                for (int i = 0; i < temp.Length; i++)
                                                {
                                                    string item = temp[i];
                                                    strResult = strResult + ObjectDic.Instance.GetObjectName(item) + ";";
                                                }
                                                c1Report.Fields[fieldIndex].Text = strResult.Substring(0, strResult.Length - 1).Replace(";", Environment.NewLine);
                                                break;
                                            default:
                                                strResult = c1Report.Fields[fieldIndex].Text;
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(strResult);
                                                break;
                                        }
                                    }
                                    break;

                                case "BT6":
                                    if (c1Report.Fields[fieldIndex].Name == null || c1Report.Fields[fieldIndex].Name.Contains("LBL"))
                                    {
                                        if (c1Report.Fields[fieldIndex].Name.Contains("LBLPALLETID"))
                                        {
                                            string[] temp = c1Report.Fields[fieldIndex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                            if (temp.Length <= 1)
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                        }
                                    }
                                    break;

                                case "ISUZU":
                                    if (c1Report.Fields[fieldIndex].Name == null || c1Report.Fields[fieldIndex].Name == "1" || c1Report.Fields[fieldIndex].Name == "2" ||
                                        c1Report.Fields[fieldIndex].Name == "3" || c1Report.Fields[fieldIndex].Name == "4" || c1Report.Fields[fieldIndex].Name == "5" ||
                                        c1Report.Fields[fieldIndex].Name == "6" || c1Report.Fields[fieldIndex].Name == "7" || c1Report.Fields[fieldIndex].Name == "8" ||
                                        c1Report.Fields[fieldIndex].Name == "9" || c1Report.Fields[fieldIndex].Name == "10" || c1Report.Fields[fieldIndex].Name == "11" ||
                                        c1Report.Fields[fieldIndex].Name == "12" || c1Report.Fields[fieldIndex].Name == "0")
                                    {
                                        c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                    }
                                    break;
                                case "ST": //2024.09.04  최평부 SI ESST용 포장라벨 추가
                                    if (c1Report.Fields[fieldIndex].Name == null || c1Report.Fields[fieldIndex].Name == "1" || c1Report.Fields[fieldIndex].Name == "11" ||
                                    c1Report.Fields[fieldIndex].Name == "11" || c1Report.Fields[fieldIndex].Name == "12" || c1Report.Fields[fieldIndex].Name == "13" ||
                                    c1Report.Fields[fieldIndex].Name == "14" || c1Report.Fields[fieldIndex].Name == "15" || c1Report.Fields[fieldIndex].Name == "16" ||
                                    c1Report.Fields[fieldIndex].Name == "18" || c1Report.Fields[fieldIndex].Name == "19" || c1Report.Fields[fieldIndex].Name == "111" ||
                                    c1Report.Fields[fieldIndex].Name == "112" || c1Report.Fields[fieldIndex].Name == "114" || c1Report.Fields[fieldIndex].Name == "115" ||
                                    c1Report.Fields[fieldIndex].Name == "118" || c1Report.Fields[fieldIndex].Name == "17" || c1Report.Fields[fieldIndex].Name == "110" ||
                                    c1Report.Fields[fieldIndex].Name == "124" || c1Report.Fields[fieldIndex].Name == "125" || c1Report.Fields[fieldIndex].Name == "20" ||
                                    c1Report.Fields[fieldIndex].Name == "21"
                                    )
                                    {
                                        if (c1Report.Fields[fieldIndex].Name == "1")
                                        {
                                            string[] temp = c1Report.Fields[fieldIndex].Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                            if (temp.Length <= 1)
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]);
                                            }
                                            else
                                            {
                                                c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(temp[0]) + "\r\n" + ObjectDic.Instance.GetObjectName(temp[1]);
                                            }
                                        }
                                        else
                                        {
                                            c1Report.Fields[fieldIndex].Text = ObjectDic.Instance.GetObjectName(c1Report.Fields[fieldIndex].Text);
                                        }
                                    }
                                    break;
                            }
                        }
                        c1Report.Fields[fieldIndex].Font.Name = "돋움체";
                    }
                    setDocumentView(param_c1DocumentViewer, c1Report);
                }
            }
        }

        private void setDataTable(string reportname = null)
        {
            try
            {
                // Reportname 별 DATA 받아오기
                DataSet dsIndata = new DataSet();
                DataTable dtIndata = new DataTable("INDATA");
                dtIndata.Columns.Add("PALLETID", typeof(string));
                dtIndata.Columns.Add("PLT_TYPE", typeof(string));
                dtIndata.Rows.Add(new object[] { palletID, reportname });
                dsIndata.Tables.Add(dtIndata);
                //2024.09.04  최평부   SI ESST용 포장라벨 추가
                this.dsPalletData = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_PLT_TAG", "INDATA", "CMA,X09CMA,PORSCHE12V,MBW12V,FORD48V,C727EOL,CMAEV,MEBCMA,BT6,ISUZU,ST", dsIndata);
                reportname = string.IsNullOrWhiteSpace(reportname) ? "CMA" : reportname;
                switch (reportname)
                {
                    case "CMA":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("CMA") ? this.dsPalletData.Tables["CMA"] : null;
                        break;
                    case "X09CMA":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("X09CMA") ? this.dsPalletData.Tables["X09CMA"] : null;
                        break;
                    case "YFCMA":
                        break;
                    case "B10CMA":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("X09CMA") ? this.dsPalletData.Tables["X09CMA"] : null;
                        break;
                    case "313HBMA":
                        break;
                    case "315HCMA":
                        break;
                    case "PL65":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("X09CMA") ? this.dsPalletData.Tables["X09CMA"] : null;
                        break;
                    case "PORSCHE12V":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("PORSCHE12V") ? this.dsPalletData.Tables["PORSCHE12V"] : null;
                        break;
                    case "BMW12V":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("BMW12V") ? this.dsPalletData.Tables["BMW12V"] : null;
                        break;
                    case "FORD48V":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("FORD48V") ? this.dsPalletData.Tables["FORD48V"] : null;
                        break;
                    case "C727EOL":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("C727EOL") ? this.dsPalletData.Tables["C727EOL"] : null;
                        break;
                    case "MEBCMA":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("MEBCMA") ? this.dsPalletData.Tables["MEBCMA"] : null;
                        break;
                    case "BT6":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("BT6") ? this.dsPalletData.Tables["BT6"] : null;
                        break;
                    case "ISUZU":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("ISUZU") ? this.dsPalletData.Tables["ISUZU"] : null;
                        break;
                    case "ST":
                        this.dtPalletHistory = this.dsPalletData.Tables.Contains("ST") ? this.dsPalletData.Tables["ST"] : null; // 2024.09.04  최평부   SI ESST용 포장라벨 추가
                        break;
                }
                SetReportData();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetReportData(string reportname = "CMA")
        {
            #region Reportname 별 Binding Data Setting
            if (this.dtPalletHistory == null || this.dtPalletHistory.Rows.Count == 0)
            {
                messageParam.AlertWarning("SFU3637");//선택한 PALLETID가 없거나 포장해제된 PALLET입니다.
                return;
            }
            else
            {
                switch (reportname)
                {
                    case "CMA":
                        this.dtBindData = setBindData_CMA();
                        break;
                    case "X09CMA":
                        if (eqsgid == "P2Q02")
                        {
                            this.dtBindData = setBindData_X09CMA();
                        }
                        break;
                    case "YFCMA":
                        break;
                    case "B10CMA":
                        if (eqsgid == "P2Q04")
                        {
                            this.dtBindData = setBindData_B10CMA();
                        }
                        break;
                    case "313HBMA":
                        if (eqsgid == "P6Q07")
                        {
                            this.dtBindData = setBindData_313HBMA();
                        }
                        break;
                    case "315HCMA":
                        break;
                    case "PL65":
                        break;
                    case "PORSCHE12V":
                        if (eqsgid == "P2Q11")
                        {
                            this.dtBindData = setBindData_BMW_PORCHE();
                        }
                        break;
                    case "BMW12V":
                        if (eqsgid == "P2Q11")
                        {
                            this.dtBindData = setBindData_BMW12V();
                        }
                        break;
                    case "FORD48V":
                        if (eqsgid == "P6Q12")
                        {
                            this.dtBindData = setBindData_Ford48V();
                        }
                        break;
                    case "C727EOL":
                        if (eqsgid == "P8Q21" || eqsgid == "P8Q30" || eqsgid == "P8Q31" || eqsgid == "P8Q24" || eqsgid == "P8Q13" || LoginInfo.CFG_AREA_ID.Equals("PJ"))
                        {
                            this.dtBindData = setBindData_C727EOL();
                        }
                        break;
                    case "MEBCMA":
                        this.dtBindData = setBindData_MEBCMA();
                        break;
                    case "BT6":
                        this.dtBindData = setBindData_BT6();
                        break;
                    case "ISUZU":
                        this.dtBindData = setBindData_ISUZU();
                        break;
                    case "ST": // 최평부 추가
                        this.dtBindData = setBindData_ST();
                        break;
                }
            }
            #endregion
        }

        private DataTable setBindData_CMA()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                int index = 2;
                string boxid = "BOXID";
                string boxLotCnt = "BOXLOTCNT";
                //2018.12.13
                int i_GBT = 0;
                int i_MBOM = 0;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("PRODCNT", typeof(string));
                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(boxLotCnt + j.ToString(), typeof(string));
                    //2018.12.13
                    this.dtBindData.Columns.Add("GBT" + j, typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["BOXCNT"] = this.dtPalletHistory.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["BOXID1"] = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]).ToString();
                //2018.12.13
                i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[0]["MBOM_CNT"].ToString());
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[boxid + index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        drBindData[boxLotCnt + index.ToString()] = Convert.ToInt32(this.dtPalletHistory.Rows[i]["BOXCNT"]).ToString();
                        //2018.12.13
                        i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[i]["GBT_CNT"].ToString());
                        i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[i]["MBOM_CNT"].ToString());
                        if (i_GBT > 0)
                        {
                            if (i_GBT == i_MBOM)
                            {
                                drBindData["GBT" + index.ToString()] = "GB/T";
                            }
                        }
                        index++;
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_X09CMA()
        {
            try
            {
                this.dtBindData = null;
                int palletlotcnt = this.dtPalletHistory.Rows.Count; //총 lot의 갯수
                int boxcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]); //box 갯수
                int box_index = 2;
                int lot_index = 2;
                string pre_boxid = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                string seleted_boxid = string.Empty;
                string seleted_cmaid = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                int pre_boxlotcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXLOTCNT"]);
                string boxid = "BOXID";
                string cmaid = "CMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("PARTNO", typeof(string));
                this.dtBindData.Columns.Add("PARTNAME", typeof(string));
                this.dtBindData.Columns.Add("SUPPIER", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                }
                for (int k = 1; k <= boxcnt * 3; k++)
                {
                    this.dtBindData.Columns.Add(cmaid + k.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["PARTNO"] = "295B93949R";
                drBindData["PARTNAME"] = "X09 CMA";
                drBindData["SUPPIER"] = "LGChem (273107)";
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["BOXID1"] = pre_boxid;
                drBindData["CMAID1"] = seleted_cmaid;
                if (palletlotcnt > 1)
                {
                    for (int i = 1; i < palletlotcnt; i++)
                    {
                        if (i == 53)
                        {
                        }
                        seleted_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        if (lot_index > 3 && (lot_index % 3) == 1)
                        {
                            drBindData[boxid + box_index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                            box_index++;
                            lot_index++;
                        }
                        else
                        {
                            if (pre_boxid == seleted_boxid)
                            {
                                drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                                lot_index++;
                            }
                            else
                            {
                                drBindData[boxid + box_index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                                if ((lot_index % 3) == 0)
                                {
                                    lot_index += 1;
                                }
                                else if ((lot_index % 3) == 1)
                                {
                                    lot_index += 3;
                                }
                                else
                                {
                                    lot_index += 2;
                                }
                                drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                                box_index++;
                                lot_index++;
                                pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                            }
                        }
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_B10CMA()
        {
            try
            {
                this.dtBindData = null;
                int palletlotcnt = this.dtPalletHistory.Rows.Count; //총 lot의 갯수
                int boxcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]); //box 갯수
                int lot_index = 2;
                string pre_boxid = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                string seleted_boxid = string.Empty;
                string seleted_cmaid = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                int pre_boxlotcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXLOTCNT"]);
                string NO = "NO";
                string cmaid = "CMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("PARTNO", typeof(string));
                this.dtBindData.Columns.Add("PARTNAME", typeof(string));
                this.dtBindData.Columns.Add("SUPPIER", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("OUTER_BOXID2", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("BOXID1", typeof(string));
                for (int j = 1; j < palletlotcnt + 1; j++)
                {
                    this.dtBindData.Columns.Add(NO + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(cmaid + j.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["PARTNO"] = "295B93949R";
                drBindData["PARTNAME"] = "B10 CMA";
                drBindData["SUPPIER"] = "LGChem (273107)";
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["OUTER_BOXID2"] = this.dtPalletHistory.Rows[0]["OUTER_BOXID2"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString() + " EA";
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["NO1"] = 1.ToString();
                drBindData["BOXID1"] = pre_boxid;
                drBindData["CMAID1"] = seleted_cmaid;
                if (palletlotcnt > 1)
                {
                    for (int i = 1; i < palletlotcnt; i++)
                    {
                        seleted_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        if (pre_boxid == seleted_boxid)
                        {
                            if (i == 23)
                            {
                            }
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            lot_index++;
                        }
                        else
                        {
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            drBindData["BOXID1"] = drBindData["BOXID1"].ToString() + "\n" + seleted_boxid;
                            lot_index++;
                            pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        }
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_313HBMA()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                int index = 2;
                string no = "CNT";
                string bmaid = "BMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("PRODNAME", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER", typeof(string));
                for (int j = 1; j < 24; j++)
                {
                    this.dtBindData.Columns.Add(no + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(bmaid + j.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PRODNAME"] = "Volvo 313H BMA";// this.dtPalletHistory.Rows[0]["PRODNAME"].ToString();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["CNT1"] = "1";
                drBindData["BMAID1"] = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[no + index.ToString()] = "1";//index.ToString();
                        drBindData[bmaid + index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                        index++;
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_BMW_PORCHE()
        {
            try
            {
                this.dtBindData = null;
                int palletlotcnt = this.dtPalletHistory.Rows.Count; //총 lot의 갯수
                int boxcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]); //box 갯수
                int box_index = 2;
                int lot_index = 2;
                string pre_boxid = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                string seleted_boxid = string.Empty;
                string seleted_cmaid = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                int pre_boxlotcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXLOTCNT"]);
                string NO = "NO";
                string cmaid = "CMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                //2018.09.27
                //this.dtBindData.Columns.Add("PARTNO", typeof(string));
                //this.dtBindData.Columns.Add("PARTNAME", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("PRODTYPE", typeof(string));
                this.dtBindData.Columns.Add("SUPPIER", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("OUTER_BOXID2", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("BOXID1", typeof(string));
                //2018.10.15
                this.dtBindData.Columns.Add("PRODUCTINDEX", typeof(string));
                for (int j = 1; j < palletlotcnt + 1; j++)
                {
                    this.dtBindData.Columns.Add(NO + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(cmaid + j.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODUCTINDEX"] = this.dtPalletHistory.Rows[0]["PRODUCT_INDEX"].ToString();
                drBindData["SUPPIER"] = "LGChem (273107)";
                drBindData["DATE"] = Convert.ToDateTime(this.dtPalletHistory.Rows[0]["DATE"]).ToString("yyyyMMdd hh:mm:ss");
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["OUTER_BOXID2"] = this.dtPalletHistory.Rows[0]["OUTER_BOXID2"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString() + " EA";
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["NO1"] = 1.ToString();
                drBindData["BOXID1"] = pre_boxid;
                drBindData["CMAID1"] = seleted_cmaid;
                if (palletlotcnt > 1)
                {
                    for (int i = 1; i < palletlotcnt; i++)
                    {
                        seleted_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        if (pre_boxid == seleted_boxid)
                        {
                            if (i == 23)
                            {
                            }
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            lot_index++;
                        }
                        else
                        {
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            drBindData["BOXID1"] = drBindData["BOXID1"].ToString() + "\n" + seleted_boxid;
                            lot_index++;
                            pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        }
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_BMW12V()
        {
            try
            {
                this.dtBindData = null;
                int palletlotcnt = this.dtPalletHistory.Rows.Count; //총 lot의 갯수
                int boxcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]); //box 갯수
                int lot_index = 2;
                string pre_boxid = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                string seleted_boxid = string.Empty;
                string seleted_cmaid = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                int pre_boxlotcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXLOTCNT"]);
                string NO = "NO";
                string cmaid = "CMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("PRODTYPE", typeof(string));
                this.dtBindData.Columns.Add("SUPPIER", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("OUTER_BOXID2", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("BOXID1", typeof(string));
                this.dtBindData.Columns.Add("PRODUCTINDEX", typeof(string));
                for (int j = 1; j < palletlotcnt + 1; j++)
                {
                    this.dtBindData.Columns.Add(NO + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(cmaid + j.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODUCTINDEX"] = this.dtPalletHistory.Rows[0]["PRODUCT_INDEX"].ToString();
                drBindData["SUPPIER"] = "LGChem (273107)";
                drBindData["DATE"] = Convert.ToDateTime(this.dtPalletHistory.Rows[0]["DATE"]).ToString("yyyyMMdd hh:mm:ss");
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["OUTER_BOXID2"] = this.dtPalletHistory.Rows[0]["OUTER_BOXID2"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString() + " EA";
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["NO1"] = 1.ToString();
                drBindData["BOXID1"] = pre_boxid;
                drBindData["CMAID1"] = seleted_cmaid;
                if (palletlotcnt > 1)
                {
                    for (int i = 1; i < palletlotcnt; i++)
                    {
                        seleted_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        if (pre_boxid == seleted_boxid)
                        {
                            if (i == 23)
                            {
                            }
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            lot_index++;
                        }
                        else
                        {
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            drBindData["BOXID1"] = drBindData["BOXID1"].ToString() + "\n" + seleted_boxid;
                            lot_index++;
                            pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        }
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_Ford48V()
        {
            try
            {
                this.dtBindData = null;
                int palletlotcnt = this.dtPalletHistory.Rows.Count; //총 lot의 갯수
                int boxcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]); //box 갯수
                int box_index = 2;
                int lot_index = 2;
                string pre_boxid = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                string seleted_boxid = string.Empty;
                string seleted_cmaid = this.dtPalletHistory.Rows[0]["LOTID"].ToString();
                int pre_boxlotcnt = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXLOTCNT"]);
                string NO = "NO";
                string cmaid = "CMAID";
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("PARTNO", typeof(string));
                this.dtBindData.Columns.Add("PARTNAME", typeof(string));
                this.dtBindData.Columns.Add("SUPPIER", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("OUTER_BOXID2", typeof(string));
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("BOXID1", typeof(string));
                for (int j = 1; j < palletlotcnt + 1; j++)
                {
                    this.dtBindData.Columns.Add(NO + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(cmaid + j.ToString(), typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["PARTNO"] = this.dtPalletHistory.Rows[0]["PARTNO"].ToString();
                drBindData["PARTNAME"] = "Ford 48V";
                drBindData["SUPPIER"] = "";
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["OUTER_BOXID2"] = this.dtPalletHistory.Rows[0]["OUTER_BOXID2"].ToString();
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString() + " EA";
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["NO1"] = 1.ToString();
                drBindData["BOXID1"] = pre_boxid;
                drBindData["CMAID1"] = seleted_cmaid;
                if (palletlotcnt > 1)
                {
                    for (int i = 1; i < palletlotcnt; i++)
                    {
                        seleted_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        if (pre_boxid == seleted_boxid)
                        {
                            if (i == 23)
                            {
                            }
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            lot_index++;
                        }
                        else
                        {
                            drBindData[NO + lot_index.ToString()] = lot_index.ToString();
                            drBindData[cmaid + lot_index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                            drBindData["BOXID1"] = drBindData["BOXID1"].ToString() + "\n" + seleted_boxid;
                            lot_index++;
                            pre_boxid = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        }
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBindData_C727EOL()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                int index = 2;
                int eolIndex = 1;
                string boxid = "BOXID";
                string boxLotCnt = "BOXLOTCNT";
                string eoldata = "EOLDATA";
                string eoldatabcr = "EOLDATABCR";
                int i_GBT = 0;
                int i_MBOM = 0;
                string url = string.Empty;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("PRODCNT", typeof(string));
                this.dtBindData.Columns.Add("BARCODE1", typeof(string));
                this.dtBindData.Columns.Add("PALLETID1", typeof(string));
                this.dtBindData.Columns.Add("DATE1", typeof(string));
                this.dtBindData.Columns.Add("BOXCNT1", typeof(string)); //BOX 갯수
                this.dtBindData.Columns.Add("LOTTOTALCNT1", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER1", typeof(string));
                this.dtBindData.Columns.Add("PRODID1", typeof(string));
                this.dtBindData.Columns.Add("PRODCNT1", typeof(string));
                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(boxLotCnt + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add("GBT" + j, typeof(string));
                    this.dtBindData.Columns.Add(eoldata + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(eoldatabcr + j.ToString(), typeof(Array));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["BARCODE1"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID1"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["DATE1"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["BOXCNT"] = this.dtPalletHistory.Rows.Count;
                drBindData["BOXCNT1"] = this.dtPalletHistory.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["LOTTOTALCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["USER1"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODID1"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["PRODCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["BOXID1"] = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]).ToString();
                i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[0]["MBOM_CNT"].ToString());
                drBindData["EOLDATA1"] = this.dtPalletHistory.Rows[0]["EOLDATA"].ToString().Substring(0, 30);
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[boxid + index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        drBindData[boxLotCnt + index.ToString()] = Convert.ToInt32(this.dtPalletHistory.Rows[i]["BOXCNT"]).ToString();
                        i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[i]["GBT_CNT"].ToString());
                        i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[i]["MBOM_CNT"].ToString());
                        drBindData[eoldata + index.ToString()] = this.dtPalletHistory.Rows[i]["EOLDATA"].ToString().Substring(0, 30);
                        if (i_GBT > 0)
                        {
                            if (i_GBT == i_MBOM)
                            {
                                drBindData["GBT" + index.ToString()] = "GB/T";
                            }
                        }
                        index++;
                    }
                }
                for (int l = 0; l < boxcnt; l++)
                {
                    drBindData[eoldatabcr + eolIndex.ToString()] = GetQRCode(this.dtPalletHistory.Rows[l]["EOLDATA"].ToString()); //.Replace("&", "%26")
                    eolIndex++;
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }
        }

        private DataTable setBindData_MEBCMA()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                int index = 2;
                string boxid = "BOXID";
                string boxLotCnt = "BOXLOTCNT";
                //2018.12.13
                int i_GBT = 0;
                int i_MBOM = 0;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("BOXSEQ", typeof(string)); //포장 단수
                this.dtBindData.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));
                this.dtBindData.Columns.Add("PRODCNT", typeof(string));
                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(boxLotCnt + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add("GBT" + j, typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["BOXSEQ"] = this.dtPalletHistory.Rows[0]["BOXSEQ"].ToString();
                drBindData["BOXCNT"] = this.dtPalletHistory.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString();
                drBindData["PRODCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["BOXID1"] = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]).ToString();
                //2018.12.13
                i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[0]["MBOM_CNT"].ToString());
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        if (i == 32)
                        {
                        }
                        drBindData[boxid + index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        drBindData[boxLotCnt + index.ToString()] = Convert.ToInt32(this.dtPalletHistory.Rows[i]["BOXCNT"]).ToString();
                        //2018.12.13
                        i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[i]["GBT_CNT"].ToString());
                        i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[i]["MBOM_CNT"].ToString());
                        if (i_GBT > 0)
                        {
                            if (i_GBT == i_MBOM)
                            {
                                drBindData["GBT" + index.ToString()] = "GB/T";
                            }
                        }
                        index++;
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }
        }

        private DataTable setBindData_BT6()
        {
            try
            {
                this.dtBindData = null;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";

                // Column 만들기 Series...
                this.AddColumnsForBT6BindingSchema("BARCODE");
                this.AddColumnsForBT6BindingSchema("PALLETID");
                this.AddColumnsForBT6BindingSchema("DATE");
                this.AddColumnsForBT6BindingSchema("BOXTOTALQTY");
                this.AddColumnsForBT6BindingSchema("LOTTOTALQTY");
                this.AddColumnsForBT6BindingSchema("USER");
                this.AddColumnsForBT6BindingSchema("PRODID1_");
                this.AddColumnsForBT6BindingSchema("PRODQTY1_");
                this.AddColumnsForBT6BindingSchema("PRODID2_");
                this.AddColumnsForBT6BindingSchema("PRODQTY2_");

                for (int i = 0; i < this.dtPalletHistory.Rows.Count; i++)
                {
                    this.dtBindData.Columns.Add("BOXID" + (i + 1).ToString(), typeof(string));
                    this.dtBindData.Columns.Add("GBT" + (i + 1).ToString(), typeof(string));
                    this.dtBindData.Columns.Add("BOXLOTCNT" + (i + 1).ToString(), typeof(string));
                    this.dtBindData.Columns.Add("EOLDATABCR" + (i + 1).ToString(), typeof(Array));
                    this.dtBindData.Columns.Add("EOLDATA" + (i + 1).ToString(), typeof(string));
                }

                // Insert Data
                // 한개만 들어가는거.
                DataRow drBindData = this.dtBindData.NewRow();
                this.MappingDatForBT6BindingSchema("BARCODE", ref drBindData, this.dtPalletHistory.Rows[0]["PALLETID"].ToString());
                this.MappingDatForBT6BindingSchema("PALLETID", ref drBindData, this.dtPalletHistory.Rows[0]["PALLETID"].ToString());
                this.MappingDatForBT6BindingSchema("DATE", ref drBindData, this.dtPalletHistory.Rows[0]["DATE"].ToString());
                this.MappingDatForBT6BindingSchema("BOXTOTALQTY", ref drBindData, this.dtPalletHistory.Rows.Count.ToString());
                this.MappingDatForBT6BindingSchema("LOTTOTALQTY", ref drBindData, this.dtPalletHistory.Rows[0]["PALLETCNT"].ToString());
                this.MappingDatForBT6BindingSchema("USER", ref drBindData, this.dtPalletHistory.Rows[0]["USERID"].ToString());
                this.MappingDatForBT6BindingSchema("PRODID1_", ref drBindData, this.dtPalletHistory.Rows[0]["PRODID"].ToString());
                this.MappingDatForBT6BindingSchema("PRODQTY1_", ref drBindData, this.dtPalletHistory.Rows[0]["PALLETCNT"].ToString());
                //this.MappingDatForBT6BindingSchema("PRODID2_", ref drBindData, this.dtPalletHistory.Rows[0]["PRODID"].ToString());        // 제품 1개인 경우에는 안찍는거 같음.
                //this.MappingDatForBT6BindingSchema("PRODQTY2_", ref drBindData, this.dtPalletHistory.Rows[0]["PALLETCNT"].ToString());    // 제품 1개인 경우에는 안찍는거 같음.

                // N개 들어가는거...
                int index = 0;
                foreach (DataRowView dataRowView in this.dtPalletHistory.AsDataView())
                {
                    string columnName = string.Empty;
                    columnName = "BOXID" + (index + 1).ToString();
                    drBindData[columnName] = dataRowView["BOXID"].ToString();
                    columnName = "BOXLOTCNT" + (index + 1).ToString();
                    drBindData[columnName] = dataRowView["BOXCNT"].ToString();
                    columnName = "EOLDATABCR" + (index + 1).ToString();
                    drBindData[columnName] = this.GetQRCode(dataRowView["EOLDATA"].ToString());
                    columnName = "EOLDATA" + (index + 1).ToString();
                    drBindData[columnName] = dataRowView["BOXID"].ToString();
                    if (Convert.ToInt32(dataRowView["GBT_CNT"].ToString()).Equals(Convert.ToInt32(dataRowView["MBOM_CNT"].ToString())))
                    {
                        columnName = "GBT" + (index + 1).ToString();
                        drBindData[columnName] = "GB/T";
                    }
                    index++;
                }

                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }
        }


        private DataTable setBindData_ISUZU()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                string url = string.Empty;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("MODELID", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("BOXID", typeof(string));
                this.dtBindData.Columns.Add("QUANTITY", typeof(string));        // Quantity of Shipping
                this.dtBindData.Columns.Add("NETWEIGHT", typeof(string));       // Net Weight
                this.dtBindData.Columns.Add("GROSSWEIGHT", typeof(string));     // Gross Weight
                this.dtBindData.Columns.Add("MINENERGY", typeof(string));      // Rated Power + Capacity
                this.dtBindData.Columns.Add("VOLTAGE", typeof(string));         // Voltage
                this.dtBindData.Columns.Add("PACKINGDATE", typeof(string));     // Packing Date

                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add("NO" + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add("MODULEID" + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add("PKGLOTID" + j.ToString(), typeof(string));
                }

                string resultDT = "";
                try
                {
                    resultDT = DateTime.Parse(this.dtPalletHistory.Rows[0]["PACKDTTM"].ToString()).ToString("yyyy.MM.dd");
                }
                catch (Exception e)
                {
                    resultDT = this.dtPalletHistory.Rows[0]["PACKDTTM"].ToString();
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["MODELID"] = "ISUZU MODULE" + "(" + this.dtPalletHistory.Rows[0]["PRODID"].ToString() + ")";
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["BOXID"] = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                drBindData["QUANTITY"] = this.dtPalletHistory.Rows.Count;                        // Quantity of Shipping       
                drBindData["NETWEIGHT"] = this.dtPalletHistory.Rows[0]["ITEM01"].ToString();      // Net Weight
                drBindData["GROSSWEIGHT"] = this.dtPalletHistory.Rows[0]["ITEM02"].ToString();      // Gross Weight
                drBindData["MINENERGY"] = this.dtPalletHistory.Rows[0]["ITEM03"].ToString();      // Min Energy : 기존 Rated Power,Capacity 병합
                drBindData["VOLTAGE"] = this.dtPalletHistory.Rows[0]["ITEM05"].ToString();      // Voltage
                drBindData["PACKINGDATE"] = resultDT;    // Packing Date

                if (boxcnt > 0)
                {
                    for (int i = 0; i < boxcnt; i++)
                    {
                        int index = i + 1;
                        drBindData["NO" + index.ToString()] = index.ToString();
                        drBindData["MODULEID" + index.ToString()] = this.dtPalletHistory.Rows[i]["LOTID"].ToString();
                        drBindData["PKGLOTID" + index.ToString()] = this.dtPalletHistory.Rows[i]["PKG_LOTID"].ToString();
                    }
                }

                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                throw ex;
            }

        }

        private DataTable setBindData_ST()
        {
            try
            {
                this.dtBindData = null;
                int boxcnt = this.dtPalletHistory.Rows.Count;
                int index = 2;
                string boxid = "BOXID";
                string boxLotCnt = "BOXLOTCNT";
                //2018.12.13
                int i_GBT = 0;
                int i_MBOM = 0;
                this.dtBindData = new DataTable();
                this.dtBindData.TableName = "RQSTDT";
                this.dtBindData.Columns.Add("BARCODE", typeof(string));
                this.dtBindData.Columns.Add("PALLETID", typeof(string));
                this.dtBindData.Columns.Add("DATE", typeof(string));
                this.dtBindData.Columns.Add("BOXCNT", typeof(string)); //BOX 갯수
                this.dtBindData.Columns.Add("LOTTOTALCNT", typeof(string)); //LOT의 총 갯수
                this.dtBindData.Columns.Add("USER", typeof(string));
                this.dtBindData.Columns.Add("PRODID", typeof(string));

                this.dtBindData.Columns.Add("TAG_ID", typeof(string));//1단 트레이ID
                this.dtBindData.Columns.Add("OQC_REQ_FLAG", typeof(string));//auto oqc 의려 여부               

                this.dtBindData.Columns.Add("PRODCNT", typeof(string));
                for (int j = 1; j <= boxcnt; j++)
                {
                    this.dtBindData.Columns.Add(boxid + j.ToString(), typeof(string));
                    this.dtBindData.Columns.Add(boxLotCnt + j.ToString(), typeof(string));
                    //2018.12.13
                    this.dtBindData.Columns.Add("GBT" + j, typeof(string));
                }
                DataRow drBindData = this.dtBindData.NewRow();
                drBindData["BARCODE"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["PALLETID"] = this.dtPalletHistory.Rows[0]["PALLETID"].ToString();
                drBindData["DATE"] = this.dtPalletHistory.Rows[0]["DATE"].ToString();
                drBindData["BOXCNT"] = this.dtPalletHistory.Rows.Count;
                drBindData["LOTTOTALCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();
                drBindData["USER"] = this.dtPalletHistory.Rows[0]["USERID"].ToString();
                drBindData["PRODID"] = this.dtPalletHistory.Rows[0]["PRODID"].ToString(); drBindData["PRODCNT"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["PALLETCNT"]).ToString();

                drBindData["TAG_ID"] = this.dtPalletHistory.Rows[0]["TAG_ID"].ToString();
                drBindData["OQC_REQ_FLAG"] = this.dtPalletHistory.Rows[0]["OQC_REQ_FLAG"].ToString();

                drBindData["BOXID1"] = this.dtPalletHistory.Rows[0]["BOXID"].ToString();
                drBindData["BOXLOTCNT1"] = Convert.ToInt32(this.dtPalletHistory.Rows[0]["BOXCNT"]).ToString();
                //2018.12.13
                i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[0]["GBT_CNT"].ToString());
                i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[0]["MBOM_CNT"].ToString());
                if (boxcnt > 1)
                {
                    for (int i = 1; i < boxcnt; i++)
                    {
                        drBindData[boxid + index.ToString()] = this.dtPalletHistory.Rows[i]["BOXID"].ToString();
                        drBindData[boxLotCnt + index.ToString()] = Convert.ToInt32(this.dtPalletHistory.Rows[i]["BOXCNT"]).ToString();
                        //2018.12.13
                        i_GBT = Convert.ToInt32(this.dtPalletHistory.Rows[i]["GBT_CNT"].ToString());
                        i_MBOM = Convert.ToInt32(this.dtPalletHistory.Rows[i]["MBOM_CNT"].ToString());
                        if (i_GBT > 0)
                        {
                            if (i_GBT == i_MBOM)
                            {
                                drBindData["GBT" + index.ToString()] = "GB/T";
                            }
                        }
                        index++;
                    }
                }
                this.dtBindData.Rows.Add(drBindData);
                return this.dtBindData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void AddColumnsForBT6BindingSchema(string prefix)
        {
            // Make Column...
            for (int i = 0; i < 4; i++)
            {
                string columnName = prefix + (i + 1).ToString();
                this.dtBindData.Columns.Add(columnName, typeof(string));
            }
        }

        private void MappingDatForBT6BindingSchema(string prefix, ref DataRow dr, string sourceData)
        {
            for (int i = 0; i < 4; i++)
            {
                string columnName = prefix + (i + 1).ToString();
                dr[columnName] = sourceData;
            }
        }

        private byte[] GetQRCode(string strQRString)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrData = qrGen.CreateQrCode(strQRString, QRCodeGenerator.ECCLevel.Q);
            QRCode qr = new QRCode(qrData);
            System.Drawing.Image fullsizeImage = qr.GetGraphic(20);
            System.Drawing.Image newImage = fullsizeImage.GetThumbnailImage(100, 100, null, IntPtr.Zero);
            System.IO.MemoryStream myResult = new System.IO.MemoryStream();
            newImage.Save(myResult, System.Drawing.Imaging.ImageFormat.Bmp);
            return myResult.ToArray();
        }

        private void setDocumentView(C1DocumentViewer c1DocumentViewer, C1.C1Report.C1Report cr)
        {
            c1DocumentViewer.Document = cr.FixedDocumentSequence;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            C1TabItem seleted_item = tcMain.SelectedItem as C1.WPF.C1TabItem;
            string item_header = seleted_item.Header.ToString();
            switch (item_header)
            {
                case "CMA":
                    seleted_c1DocumentViewer = c1DocumentViewer_CMA;
                    break;
                case "X09CMA":
                    seleted_c1DocumentViewer = c1DocumentViewer_X09CMA;
                    break;
                case "YFCMA":
                    break;
                case "B10CMA":
                    seleted_c1DocumentViewer = c1DocumentViewer_B10CMA;
                    break;
                case "313HBMA":
                    break;
                case "315HCMA":
                    break;
                case "PL65":
                    seleted_c1DocumentViewer = c1DocumentViewer_PL65;
                    break;
                case "Porsche12V":
                    seleted_c1DocumentViewer = c1DocumentViewer_BMW_PORCHE;
                    break;
                case "BMW12V":
                    seleted_c1DocumentViewer = c1DocumentViewer_BMW12V;
                    break;
                case "Ford48V":
                    seleted_c1DocumentViewer = c1DocumentViewer_Ford48V;
                    break;
                case "C727EOL":
                    seleted_c1DocumentViewer = c1DocumentViewer_C727EOL;
                    break;
                case "MEBCMA":
                    seleted_c1DocumentViewer = c1DocumentViewer_MEBCMA;
                    break;
                case "BT6":
                    seleted_c1DocumentViewer = c1DocumentViewer_BT6;
                    break;
                case "ISUZU":
                    seleted_c1DocumentViewer = c1DocumentViewer_ISUZU;
                    break;
                case "ST": //2024.09.04  최평부   SI ESST용 포장라벨 추가
                    seleted_c1DocumentViewer = c1DocumentViewer_ST;
                    break;
            }
            seleted_c1DocumentViewer.Print();
            if (strChkZplPrint)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("LABEL_CODE", typeof(string));
                dt.Columns.Add("ITEM001", typeof(string));
                dt.Columns.Add("ITEM002", typeof(string));
                DataRow drLbl = dt.NewRow();
                drLbl["LOTID"] = palletID;
                drLbl["LABEL_CODE"] = "LBL0247";
                drLbl["ITEM001"] = palletID;
                drLbl["ITEM002"] = palletID;
                dt.Rows.Add(drLbl);
                Util.labelPrint(FrameOperation, loadingIndicator, dt);
            }
            if (this.DialogResult == null)
            {
                return;
            }
            this.DialogResult = true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (this.DialogResult == null)
            {
            }
            else
            {
                this.DialogResult = false;
            }
            this.Close();
        }

        private void tcMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CommonVerify.HasTableInDataSet(this.dsPalletData))
            {
                string reportname = ((C1.WPF.C1TabItem)tcMain.SelectedItem).Header.ToString();
                string gubun = reportname;
                reportname = this.dsPalletData.Tables.Contains(reportname.Trim().ToUpper()) ? reportname.Trim().ToUpper() : "CMA";
                this.dtPalletHistory = this.dsPalletData.Tables[reportname];
                SetReportData(reportname);
                setReport(gubun);
            }
        }

        private Boolean chkPalletZplYn(string strEqsgId)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_PALLET_ZPL_PRINT";
                dr["CMCODE"] = strEqsgId;
                RQSTDT.Rows.Add(dr);
                DataTable dtAuth = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "INDATA", "OUTDATA", RQSTDT);
                if (dtAuth.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #region Event Lists...
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //            int MovePageIdx = 0; ESWA 테스트 Holding
            reports_name = new string[] { "CMA", "X09CMA", "PORSCHE12V", "BMW12V", "FORD48V", "C727EOL", "MEBCMA", "BT6", "ISUZU", "ST" };//최평부 추가
            tmps = C1WindowExtension.GetParameters(this);
            window_name = tmps[0] as string;
            palletID = tmps[1] as string;   // palletID
            eqsgid = tmps[2] as string;     // eqsgid
                                            //             tagname = tmps[3] as string;    // 호출 프로그램으로 부터 전달 받은 이동할 양식이름 ESWA 테스트 Holding

            //  2021.08.27  정용석   MEB 7단 포장 라벨의 경우 최초 발행은 못하게 INTERLOCK
            if (!this.CheckMEBPalletPrintYN(palletID))
            {
                // %1 라벨은 재발행만 가능합니다.\r\n물류관리->Pallet 라벨 일괄 발행 화면에서 최초 라벨 발행을 해주세요.
                Util.MessageInfo("SFU8402", (result) =>
                {
                    this.Close();
                }, palletID);
                return;
            }
            strChkZplPrint = chkPalletZplYn(eqsgid);
            this.Loaded -= Window_Loaded;
            setDataTable();
            setReport();

            /* ESWA 테스트 Holding
            // 양식명을 전달받았다면, reports_name과 비교하여 위치 정보를 찾고, 해당 양식으로 이동시킨다.
            if (tagname != "") {
                for (int i = 0; i < tcMain.Items.Count; i++) {
                    if (tagname == ((C1.WPF.C1TabItem)tcMain.Items[i]).Header.ToString()) {
                        MovePageIdx = i;
                    }
                }
                tcMain.SelectedIndex = MovePageIdx;
            }
            */
            //http://our.componentone.com/groups/topic/need-help-getting-c1reports-to-work/
            //임시 DPI
            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
            {
                if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()) == true)
                {
                    strDpi = dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DPI].ToString();
                }
            }
        }
        #endregion
    }
}