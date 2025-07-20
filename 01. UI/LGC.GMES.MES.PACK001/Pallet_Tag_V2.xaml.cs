/*************************************************************************************
Created Date : 2017-01-25
Creator      : srcadm01
Description  : Pack 포장 정보 레포트 발행
--------------------------------------------------------------------------------------
[Change History]
 2017.01.25  srcadm01 : Initial Created.
 2017.04.06  장만철   :
 2017.07.13  장만철   :
 2018.04.18  손우석   : CSR ID 3665550 팩 11호기 BMW12V 35UP Pallet 인쇄양식 변경요청의건 요청번호 C20180418_65550
 2018.09.27  손우석   : CSR ID 3794696 CSR요구사항 정의서 - 팩11호 팔렛트ID 출력 시 출력정보 오류 개선 및 변경 요청번호 C20180915_94696
 2018.10.15  손우석   : CSR ID 3794696 CSR요구사항 정의서 - 팩11호 팔렛트ID 출력 시 출력정보 오류 개선 및 변경 요청번호 C20180915_94696
 2018.11.08  손우석   : CSR ID 3835126 오창 팩11호 BMW12V Pallet 정보 Sheet 출력 양식 개선 요청 요청번호 20181105_35126
 2018.12.06  손우석   : CSR ID 3835126 오창 팩11호 BMW12V Pallet 정보 Sheet 출력 양식 개선 요청 요청번호 20181105_35126
 2018.12.13  손우석   : CSR ID 3859317 [G.MES] Audi BEV C-sample G.MES G/BT 모듈 출하 식별 기능 추가 요청의 건 [요청번호] C20181201_59317
 2019.02.26  손우석   : CSR ID 3932728 GMES Pallet 발행 식별표 추가 요청_신규모델 [요청번호] C20190225_32728
 2019.11.05  손우석   : CSR ID 1840 PALLET Tag 양식 변경 요청 [요청번호] C20191101-000145 [서비스 번호] 1840
 2019.11.18  염규범   : 오류건 수정
 2019.12.05  손우석   : CSR ID 10840 MOKA Pallet출력용지 오류개선 요청 건 [요청번호] C20191206-000023
 2020.06.05  김민석   : CSR ID 61625 GMES 시스템의 전진검사 효율화를 위한 바코드 출력 기능 변경(건) [요청번호] C20200519-000003
 2020.06.23  최우석   : CSR ID 61613, 라인 입고 후 Cell 동간 이동 처리 팔레트 조회 및 성능 개선 [요청번호] C20200518-000493
 2020.07.03  최우석   : CSR ID 61613, 라인 입고 후 Cell 동간 이동 처리 팔레트 조회 및 성능 개선 [요청번호] C20200518-000493 Decimal 형변환 오류 수정
 2020.10.14  염규범   : Pallet 용지 출력시, ZPL 라벨지 추가적으로 인쇄 처리 여부 PACK_UI_PALLET_ZPL_PRINT 에 LoginInfo.CFG_SHOP_ID 구분 처리
 2021.08.27  정용석   : MEB 7단 포장 라벨의 경우 최초 발행은 못하게 INTERLOCK
 2021.11.15  김길용   : Pack 3동 포장기 라벨 추가(CarrierID 추가)
 2021.11.23  김길용   : 배포 이슈로인해 이전 버전으로 롤백처리
 2022.03.24  정용석   : RSA BT6향 포장라벨 추가
 2022.08.04  임성운   : NJ ISUZU용 포장라벨 추가
 2022.09.21  임성운   : 양식 자동이동 기능 개발(tagname을 Parameter로 받아와서 해당 양식으로 이동)
 2022.09.27  정용석   : C20220923-000277 Pallet포장 라벨 출력 기능 개선 요청
 2024.11.19  김준형   : ST Stellantis 포장라벨 추가
**************************************************************************************/
using C1.C1Report;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;

using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class Pallet_Tag_V2 : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private bool isShowPalletLabelType = false;
        private PalletLabel palletLabel = new PalletLabel();
        private string currentPalletLabelType = string.Empty;
        private string equipmentSegmentID = string.Empty;
        #endregion

        #region Constructor
        public Pallet_Tag_V2()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Member Function Lists...
        // 초기화
        private void Initialize()
        {
            try
            {
                object[] obj = C1WindowExtension.GetParameters(this);
                if (obj.Length > 0)
                {
                    //this.txtPalletID.Text = "PP1Q02QF16MD006";                 // For Test
                    this.txtPalletID.Text = Convert.ToString(obj[1]);            // 구루마 ID
                    this.equipmentSegmentID = Convert.ToString(obj[2]);          // 라인 ID
                }

                // Label 유형 Grid에 뿌리기
                DataTable dt = this.GetCommonCodeInfo("PACK_PALLETAG_LIST");
                if (!CommonVerify.HasTableRow(dt))
                {
                    dt = new DataTable();
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dr = null;
                    dr = dt.NewRow(); dr["CBO_CODE"] = "CMA"; dr["CBO_NAME"] = "CMA"; dt.Rows.Add(dr);                  // 공통
                    dr = dt.NewRow(); dr["CBO_CODE"] = "X09CMA"; dr["CBO_NAME"] = "X09CMA"; dt.Rows.Add(dr);            // 오창 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "B10CMA"; dr["CBO_NAME"] = "B10CMA"; dt.Rows.Add(dr);            // 오창 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "PORSCHE12V"; dr["CBO_NAME"] = "PORSCHE12V"; dt.Rows.Add(dr);    // 오창 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "BMW12V"; dr["CBO_NAME"] = "BMW12V"; dt.Rows.Add(dr);            // 오창 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "FORD48V"; dr["CBO_NAME"] = "FORD48V"; dt.Rows.Add(dr);          // 오창 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "MEBCMA"; dr["CBO_NAME"] = "MEBCMA"; dt.Rows.Add(dr);            // 폴란드 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "C727EOL"; dr["CBO_NAME"] = "C727EOL"; dt.Rows.Add(dr);          // 폴란드 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "BT6"; dr["CBO_NAME"] = "BT6"; dt.Rows.Add(dr);                  // 폴란드 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "ISUZU"; dr["CBO_NAME"] = "ISUZU"; dt.Rows.Add(dr);              // 중공
                    dr = dt.NewRow(); dr["CBO_CODE"] = "MAN_TRUCK"; dr["CBO_NAME"] = "MAN_TRUCK"; dt.Rows.Add(dr);      // 폴란드 
                    dr = dt.NewRow(); dr["CBO_CODE"] = "ST"; dr["CBO_NAME"] = "ST"; dt.Rows.Add(dr);                    // 캐나다
                    dt.AcceptChanges();
                }
                Util.GridSetData(this.dgList, dt, FrameOperation, true);

                this.ShowPalletLabelType();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.Loaded -= C1Window_Loaded;
            }
        }

        // 조회
        private void SearchProcess()
        {
            // Declarations...
            DataSet ds = new DataSet();
            if (string.IsNullOrEmpty(this.txtPalletID.Text))
            {
                return;
            }

            try
            {
                // Data 가져오기
                ds = this.GetPalletLabelInfo(this.txtPalletID.Text, this.currentPalletLabelType);
                if (!CommonVerify.HasTableInDataSet(ds) || !CommonVerify.HasTableRow(ds.Tables["OUTDATA"]))
                {
                    Util.MessageValidation("SFU3637");
                    return;
                }

                // Report 객체 생성
                this.loadingIndicator.Visibility = Visibility.Visible;
                PackCommon.DoEvents();

                // 입력한 Pallet의 제품 ID가 Pallet Label Type 기준정보에 등록된 제품 ID라고 하면 왼쪽에 Pallet Label Type Grid 숨기기 - (From 폴란드 )
                //this.isShowPalletLabelType = ds.Tables["OUTDATA_PALLET_LABEL_TYPE"].AsEnumerable().Select(x => x.Field<bool>("PALLET_LABEL_TYPE_VIEW_FLAG")).FirstOrDefault();
                this.isShowPalletLabelType = ds.Tables["OUTDATA_PALLET_LABEL_TYPE"].AsEnumerable().Select(x => x.Field<string>("PALLET_LABEL_TYPE_VIEW_FLAG")).FirstOrDefault().Equals("True") ? true : false;
                string palletLabelType = ds.Tables["OUTDATA_PALLET_LABEL_TYPE"].AsEnumerable().Select(x => x.Field<string>("PALLET_LABEL_TYPE")).FirstOrDefault();

                this.ShowPalletLabelType();
                if (this.CreateC1ReportClass(palletLabelType, ds))
                {
                    C1Report c1Report = this.palletLabel.LoadC1Report();
                    this.c1DocumentViewer.Document = c1Report.FixedDocumentSequence;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowPalletLabelType()
        {
            // isShowPalletlabelType
            if (!this.isShowPalletLabelType)
            {
                this.gridColumnPalletLabelType1.Width = new GridLength(120, GridUnitType.Pixel);
                this.gridColumnPalletLabelType2.Width = new GridLength(8, GridUnitType.Pixel);
            }
            else
            {
                this.gridColumnPalletLabelType1.Width = new GridLength(0, GridUnitType.Pixel);
                this.gridColumnPalletLabelType2.Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        // Create Report Class
        private bool CreateC1ReportClass(string palletLabelType, DataSet ds)
        {
            bool returnValue = true;
            try
            {
                this.palletLabel = null;
                Type type = Type.GetType(this.GetType().Namespace + "." + palletLabelType.ToUpper());
                object obj = Activator.CreateInstance(type, ds);
                if (obj == null)
                {
                    returnValue = false;
                }
                else
                {
                    this.palletLabel = (PalletLabel)obj;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                returnValue = false;
            }

            return returnValue;
        }

        // Report 출력
        private void PrintProcess()
        {
            this.c1DocumentViewer.Print();
            this.PrintPalletLabelZebraPrinter(this.txtPalletID.Text, this.equipmentSegmentID);  // TODO : PalletID, EquipmentSegmentID
        }

        // Zebra Printer로의 출력여부 체크 (현재 사용설정은 안되어 있음.)
        private bool CheckPrintPalletLabelZebraPrinter(string equipmentSegmentID)
        {
            return CommonVerify.HasTableRow(this.GetCommonCodeInfo("PACK_UI_PALLET_ZPL_PRINT", equipmentSegmentID)) ? true : false;
        }

        // Zebra Printer로의 출력 (현재는 이거로 출력할 일이 없어보임.)
        private void PrintPalletLabelZebraPrinter(string palletID, string equipmentSegmentID)
        {
            if (!this.CheckPrintPalletLabelZebraPrinter(equipmentSegmentID))
            {
                return;
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("LABEL_CODE", typeof(string));
            dt.Columns.Add("ITEM001", typeof(string));
            dt.Columns.Add("ITEM002", typeof(string));

            DataRow dr = dt.NewRow();
            dr["LOTID"] = palletID;
            dr["LABEL_CODE"] = "LBL0247";
            dr["ITEM001"] = palletID;
            dr["ITEM002"] = palletID;
            dt.Rows.Add(dr);

            Util.labelPrint(FrameOperation, loadingIndicator, dt);
        }

        // 순서도 호출 - CommonCode 정보
        private DataTable GetCommonCodeInfo(string cmcdType, string cmCode = null)
        {
            DataTable dtReturn = new DataTable();
            try
            {
                string bizRuleName = "DA_BAS_SEL_COMMONCODE_ATTRIBUTE";
                DataTable dtRQSTDT = new DataTable("RQSTDT");
                DataTable dtRSLTDT = new DataTable("RSLTDT");

                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                dtRQSTDT.Columns.Add("CBO_CODE", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE2", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE3", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));
                dtRQSTDT.Columns.Add("ATTRIBUTE5", typeof(string));

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["CMCDTYPE"] = cmcdType;
                drRQSTDT["CBO_CODE"] = string.IsNullOrEmpty(cmCode) ? null : cmCode;
                drRQSTDT["ATTRIBUTE1"] = null;
                drRQSTDT["ATTRIBUTE2"] = null;
                drRQSTDT["ATTRIBUTE3"] = null;
                drRQSTDT["ATTRIBUTE4"] = null;
                drRQSTDT["ATTRIBUTE5"] = null;
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (CommonVerify.HasTableRow(dtRSLTDT))
                {
                    // E20240226-000090 : 공통코드 정보를 참조하여 Pallet Tag 발행 화면 내 해당 Site에 해당하는 Project 만 조회되도록 개선
                    dtReturn = dtRSLTDT.AsEnumerable().Where(x => string.IsNullOrEmpty(x.Field<string>("ATTRIBUTE1")) || (!string.IsNullOrEmpty(x.Field<string>("ATTRIBUTE1")) && x.Field<string>("ATTRIBUTE1").Contains(LoginInfo.CFG_AREA_ID))).CopyToDataTable();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dtReturn;
        }

        // 순서도 호출 - Pallet Label Data 가져오기
        private DataSet GetPalletLabelInfo(string palletID, string labelType)
        {
            string bizRuleName = "BR_PRD_GET_PALLET_LABEL_DATA";
            DataSet dsINDATA = new DataSet();
            DataSet dsOUTDATA = new DataSet();
            string outDataSetName = "OUTDATA_PALLET_LABEL_TYPE,OUTDATA";

            try
            {
                DataTable dtINDATA = new DataTable("INDATA");
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("PALLETID", typeof(string));
                dtINDATA.Columns.Add("PALLET_LABEL_TYPE", typeof(string));

                DataRow drINDATA = dtINDATA.NewRow();
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PALLETID"] = palletID;
                drINDATA["PALLET_LABEL_TYPE"] = labelType;
                dtINDATA.Rows.Add(drINDATA);
                dsINDATA.Tables.Add(dtINDATA);

                string inDataTableNameList = string.Join(",", dsINDATA.Tables.OfType<DataTable>().Select(x => x.TableName).ToList());
                dsOUTDATA = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, inDataTableNameList, outDataSetName, dsINDATA);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return dsOUTDATA;
        }
        #endregion

        #region Event Lists...
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.SearchProcess();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            this.PrintProcess();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgList_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            System.Windows.Point point = e.GetPosition(null);
            DataGridCell dataGridCell = c1DataGrid.GetCellFromPoint(point);

            if (dataGridCell == null || dataGridCell.Value == null || string.IsNullOrEmpty(dataGridCell.Value.ToString()))
            {
                return;
            }

            this.currentPalletLabelType = dataGridCell.Value.ToString();
            this.SearchProcess();
        }

        private void txtPalletID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            this.SearchProcess();
        }
        #endregion
    }
}
