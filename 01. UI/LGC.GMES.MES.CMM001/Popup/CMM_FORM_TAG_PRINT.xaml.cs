/*************************************************************************************
 Created Date : 2017.07.21
      Creator : 
   Decription : Tag Print
--------------------------------------------------------------------------------------
 [Change History]
 
 2019.03.26 0.1 이상훈 C20190318_50754 활성화 pallet tag 용량/lot 표기 건
 2019.04.10 0.2 이상훈 C20190312_46085 초소형 inbox pallet 라벨 출력 용지 size A5 추가
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
    public partial class CMM_FORM_TAG_PRINT : C1Window, IWorkArea
    {
        #region Declaration

        private readonly BizDataSet _bizDataSet = new BizDataSet();

        C1.C1Report.C1Report cr = null;

        DataTable _dtTagPrint;
        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _palletID = string.Empty;      // Pallet, Inbox ID
        private string _wipSeq = string.Empty;        // WipSeq
        private string _cellQty = string.Empty;       // 수량
        private string _dispatch = string.Empty;
        private string _rePrint = string.Empty;
        private string _directPrint = string.Empty;
        private string _MCC_MCS_MCR_CHK = string.Empty;

        string _sPGM_ID = "CMM_FORM_TAG_PRINT";

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
        public CMM_FORM_TAG_PRINT()
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
            _eqptID = tmps[1] as string;
            _palletID = tmps[2] as string;
            _wipSeq = tmps[3] as string;
            _cellQty = tmps[4] as string;
            _dispatch = tmps[5] as string;
            _rePrint = tmps[6] as string;
            _directPrint = tmps[7] as string;

            SetTagPrintData();

            if(LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_AREA_ID.Equals("S5"))
            {
                if (_dtTagPrint != null && _dtTagPrint.Rows.Count > 0)
                {
                    _MCC_MCS_MCR_CHK = MCC_MCS_MCR_CHK(_dtTagPrint.Rows[0]["PRODID"].ToString());
                }
                if (_MCC_MCS_MCR_CHK != "MCS")
                {
                    if (_procID.Equals(Process.CircularCharacteristicGrader) || _procID.Equals(Process.CircularReTubing) || _procID.Equals(Process.CELL_BOXING))
                    {
                        PrintViewNJ();
                    }
                    else
                    {
                        PrintView();
                    }
                }
                else
                {
                    PrintView();
                }
            }
            else
            {

                PrintView();
            }

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

        #region User Method

        private void SetTagPrintData()
        {
            if (_procID.Equals(Process.CircularCharacteristicGrader) || _procID.Equals(Process.CircularReTubing))
            {
                // 남경 특성/Grading -> HoldPalletYN, RemainPalletYN 추가
                if (string.Equals(DefectPalletYN, "Y"))
                {
                    SetPalletDefect();
                }
                else if (string.Equals(QMSRequestPalletYN, "Y"))
                {
                    //[C20190318_50754] 변경
                    if (LoginInfo.CFG_SHOP_ID.Equals("A010") && _procID.Equals(Process.CircularCharacteristicGrader))
                    {
                        SetPallet_QmsRequest();
                    }
                    else
                    {
                        SetPallet_QmsRequest_NJ();
                    }
                }
                else if (string.Equals(HoldPalletYN, "Y"))
                {
                    SetPallet();
                }
                else if (string.Equals(RemainPalletYN, "Y"))
                {
                    if (LoginInfo.CFG_SHOP_ID.Equals("A010") && _procID.Equals(Process.CircularCharacteristicGrader))
                    {
                        SetPallet_QmsRequest();
                    }
                    else
                    {
                        SetPallet_QmsRequest_NJ();
                    }
                }
                else
                {
                    //[20181204_01] 소형 특성/Grader 라우터 추가로 인한 로직 변경
                    if (LoginInfo.CFG_SHOP_ID.Equals("A010") && _procID.Equals(Process.CircularCharacteristicGrader))
                    {
                        SetPallet();
                    }
                    else
                    {
                        SetPalletNJ();
                    }
                }
            }
            else
            {
                if (string.Equals(DefectPalletYN, "Y"))
                {
                    SetPalletDefect();
                }
                else if (string.Equals(QMSRequestPalletYN, "Y"))
                {
                    SetPallet_QmsRequest();
                }
                else if (string.Equals(returnPalletYN, "Y"))
                {
                    SetPallet_QmsRequest_NJ();
                }
                else
                {
                    SetPallet();
                }

                if (_dtTagPrint == null || _dtTagPrint.Rows.Count == 0)
                {
                    // 출력 Lot 정보가 없습니다.
                    Util.MessageValidation("SFU4025");
                    return;
                }
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
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _palletID;
                newRow["WIPSEQ"] = _wipSeq;

                inTable.Rows.Add(newRow);

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_FO", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// Pallet 정보 조회(남경)
        /// </summary>
        private void SetPalletNJ()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _palletID;
                newRow["PROCID"] = _procID;
                newRow["WIPSEQ"] = _wipSeq;

                inTable.Rows.Add(newRow);

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_NJ", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void SetPallet_QmsRequest()
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

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_FO_QMS_REQUEST", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        private void SetPallet_QmsRequest_NJ()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _palletID;
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                _dtTagPrint = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TAG_PRINT_FO_QMS_REQUEST_NJ", "INDATA", "OUTDATA", inTable);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #region 공통 : 미리보기
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


                if (string.Equals(QMSRequestPalletYN, "Y"))
                {
                    if (_dtTagPrint != null && _dtTagPrint.Rows.Count > 0)
                    {
                        // 확인 필요 공정이 F5000으로 나오는 경우가 있음
                        // 쿼리 확인이 필요함 
                        if (_procID != "F6000")
                        {
                            _procID = _dtTagPrint.Rows[0]["PROCID"].ToString();
                        }
                        HoldPalletYN = _dtTagPrint.Rows[0]["WIPHOLD"].ToString();

                        if (_dtTagPrint.Rows[0]["GRADE"].ToString().Equals("N"))
                            DefectPalletYN = "Y";
                    }
                }

                #region Print 태그 설정
                if (string.Equals(DefectPalletYN, "Y"))
                {
                    // 불량 Pallet
                    if (_procID == Process.SELECTING)
                    {
                        if (_MCC_MCS_MCR_CHK == "MCS")
                        {
                            #region A5 용지 설정
                            if (paperSize.Equals("A4"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect";
                            }
                            else if (paperSize.Equals("A5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect_A5.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect_A5";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect";
                            }
                            #endregion
                        }
                        else 
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteDefect.xml";
                            ReportName = "FORM_GraderPaletteDefect";
                        }
                      
                    }
                    else
                    {
                        if (string.Equals(_procID, Process.CircularCharacteristic) || string.Equals(_procID, Process.CircularReTubing))
                        {
                            if (LoginInfo.CFG_SHOP_ID.Equals("G182") || LoginInfo.CFG_AREA_ID.Equals("S5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteDefect_NJ.xml";
                                ReportName = "FORM_GraderPaletteDefect_NJ";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteDefect.xml";
                                ReportName = "FORM_GraderPaletteDefect";
                            }
                        }
                        else
                        {
                            #region A5 용지 설정
                            if (paperSize.Equals("A4"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect";
                            }
                            else if (paperSize.Equals("A5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect_A5.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect_A5";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallTypeDefect.xml";
                                ReportName = "FORM_GraderPaletteSmallTypeDefect";
                            }
                            #endregion
                        }
                    }
                }
                else if (string.Equals(RemainPalletYN, "Y"))
                {
                    // 잔량 Pallet
                    if (string.Equals(_procID, Process.CircularCharacteristic) || string.Equals(_procID, Process.CircularReTubing) || string.Equals(_procID, Process.CircularVoltage))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette.xml";
                        ReportName = "FORM_GraderPalette";
                    }
                    else if (string.Equals(_procID, Process.SmallXray))
                    {
                        #region A5 용지 설정
                        if (paperSize.Equals("A4"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                            ReportName = "FORM_GraderPaletteSmallType";
                        }
                        else if (paperSize.Equals("A5"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType_A5.xml";
                            ReportName = "FORM_GraderPaletteSmallType_A5";
                        }
                        else
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                            ReportName = "FORM_GraderPaletteSmallType";
                        }
                        #endregion
                    }
                    //[C20181121_50523] 소형 특성/Grader 라우터 추가로 인한 로직 변경
                    else if (LoginInfo.CFG_SHOP_ID == "A010" && string.Equals(_procID, Process.CircularCharacteristicGrader))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette.xml";
                        ReportName = "FORM_GraderPalette";
                    }
                    else
                    {
                        #region A5 용지 설정
                        // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                        if (paperSize.Equals("A4"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                            ReportName = "FORM_InboxPaletteSmallType";
                        }
                        else if (paperSize.Equals("A5"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType_A5.xml";
                            ReportName = "FORM_InboxPaletteSmallType_A5";
                        }
                        else
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                            ReportName = "FORM_InboxPaletteSmallType";
                        }
                        #endregion

                    }
                }
                else if (string.Equals(HoldPalletYN, "Y"))
                {
                    // Hold Pallet
                    if (string.Equals(_procID, Process.CircularGrader))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette.xml";
                        ReportName = "FORM_GraderPalette";
                    }
                    else if (string.Equals(_procID, Process.CircularCharacteristic) || string.Equals(_procID, Process.CircularReTubing))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteHold.xml";
                        ReportName = "FORM_InboxPaletteHold";
                    }
                    else if (string.Equals(_procID, Process.SmallGrader))
                    {
                        #region A5 용지 설정
                        // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                        if (paperSize.Equals("A4"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                            ReportName = "FORM_GraderPaletteSmallType";
                        }
                        else if (paperSize.Equals("A5"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType_A5.xml";
                            ReportName = "FORM_GraderPaletteSmallType_A5";
                        }
                        else
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                            ReportName = "FORM_GraderPaletteSmallType";
                        }
                        #endregion
                    }
                    //[C20181121_50523] 소형 특성/Grader 라우터 추가로 인한 로직 변경
                    else if (LoginInfo.CFG_SHOP_ID == "A010" && string.Equals(_procID, Process.CircularCharacteristicGrader))
                    {
                        ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteHold.xml";
                        ReportName = "FORM_InboxPaletteHold";
                    }
                    else
                    {
                        #region A5 용지 설정
                        if (paperSize.Equals("A4"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallTypeHold.xml";
                            ReportName = "FORM_InboxPaletteSmallTypeHold";
                        }
                        else if (paperSize.Equals("A5"))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallTypeHold_A5.xml";
                            ReportName = "FORM_InboxPaletteSmallTypeHold_A5";
                        }
                        else
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallTypeHold.xml";
                            ReportName = "FORM_InboxPaletteSmallTypeHold";
                        }
                        #endregion
                    }

                }

                else
                {
                    //조회값이 B1000으로 넘어오는 경우 초소형으로 나타남
                    if (_procID == "B1000")
                    {
                        if (_MCC_MCS_MCR_CHK == "MCS")
                        {
                            #region A5 용지 설정
                            // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                            if (paperSize.Equals("A4"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                                ReportName = "FORM_InboxPaletteSmallType";
                            }
                            else if (paperSize.Equals("A5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType_A5.xml";
                                ReportName = "FORM_InboxPaletteSmallType_A5";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                                ReportName = "FORM_InboxPaletteSmallType";
                            }
                            #endregion
                        }
                        else 
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPalette.xml";
                            ReportName = "FORM_InboxPalette";
                        }
                        
                    }
                    else
                    {
                        //[20181204_01] 소형 특성/Grader 라우터 추가로 인한 로직 변경
                        if (LoginInfo.CFG_SHOP_ID == "A010" && string.Equals(_procID, Process.CircularCharacteristicGrader)) 
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPalette.xml";
                            ReportName = "FORM_InboxPalette";
                        }
                        else if (string.Equals(_procID, Process.CircularGrader) || (LoginInfo.CFG_SHOP_ID != "A010" &&  string.Equals(_procID, Process.CircularCharacteristicGrader)))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette.xml";
                            ReportName = "FORM_GraderPalette";
                        }
                        else if (string.Equals(_procID, Process.CircularCharacteristic) || string.Equals(_procID, Process.CircularReTubing) || string.Equals(_procID, Process.CircularVoltage))
                        {
                            ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPalette.xml";
                            ReportName = "FORM_InboxPalette";
                        }
                        else if (string.Equals(_procID, Process.SmallGrader))
                        {
                            #region A5 용지 설정
                            // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                            if (paperSize.Equals("A4"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                                ReportName = "FORM_GraderPaletteSmallType";
                            }
                            else if (paperSize.Equals("A5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType_A5.xml";
                                ReportName = "FORM_GraderPaletteSmallType_A5";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteSmallType.xml";
                                ReportName = "FORM_GraderPaletteSmallType";
                            }
                            #endregion

                        }
                        else
                        {

                            #region A5 용지 설정
                            // C20190312_46085 GMES 환경 설정의 기타 항목의 용지 SIZE 값에 따른 출력 변경 처리
                            if (paperSize.Equals("A4"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                                ReportName = "FORM_InboxPaletteSmallType";
                            }
                            else if (paperSize.Equals("A5"))
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType_A5.xml";
                                ReportName = "FORM_InboxPaletteSmallType_A5";
                            }
                            else
                            {
                                ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_InboxPaletteSmallType.xml";
                                ReportName = "FORM_InboxPaletteSmallType";
                            }
                            #endregion
                        }
                    }

                }
                #endregion

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
                            if (string.Equals(HoldPalletYN, "Y") && (ReportName.Equals("FORM_GraderPalette") || ReportName.Equals("FORM_GraderPaletteSmallType") || ReportName.Equals("FORM_GraderPaletteSmallType_A5")))
                            {
                                if (cr.Fields[cnt].Name.Equals("txTitle_1"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("HOLD"));
                                else if (cr.Fields[cnt].Name.Equals("txTitle_2"))
                                    cr.Fields[cnt].Text = "";
                                else if (cr.Fields[cnt].Name.Equals("txTitle_3"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("Hold 사유"));
                                else if (cr.Fields[cnt].Name.Equals("txTitle_4"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("비고"));
                                else
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));

                            }
                            else
                            {
                                cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                            }

                        }

                        // 초소형 공정이구 불량 Pallet인 경우 공정명 표시
                        if (cr.Fields[cnt].Name.Equals("TagName"))
                        {
                            cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));

                            //[20181204_01] 소형 특성/Grader 라우터 추가로 인한 로직 변경
                            if (_procID != Process.CircularGrader &&
                                _procID != Process.CircularCharacteristic &&
                                _procID != Process.SmallGrader &&
                                ! (_procID == Process.CircularCharacteristicGrader && LoginInfo.CFG_SHOP_ID == "A010")
                                )
                            {
                                cr.Fields[cnt].Text += " (" + _dtTagPrint.Rows[0]["PROCNAME"].ToString() + ")";
                            }
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

                #region >>>>> [C20190312_46085] 용지 설정(A4,A5)
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
                

                if (_directPrint.Equals("Y"))
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                    pm.Print(ps, ps.DefaultPageSettings);

                    SetLabelPrtHist();

                    if (_dispatch.Equals("Y"))
                    {
                        SetDispatch();
                    }

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
        #endregion

        #region 남경(특성/Grading) : 미리보기
        /// <summary>
        /// 미리보기
        /// </summary>
        private void PrintViewNJ()
        {
            try
            {
                string ReportNamePath = string.Empty;
                string ReportName = string.Empty;

                if (string.Equals(QMSRequestPalletYN, "Y"))
                {
                    if (_dtTagPrint != null && _dtTagPrint.Rows.Count > 0)
                    {
                        // 확인 필요 공정이 F5000으로 나오는 경우가 있음
                        // 쿼리 확인이 필요함 
                        if (_procID != "F6000")
                        {
                            _procID = _dtTagPrint.Rows[0]["PROCID"].ToString();
                        }
                        HoldPalletYN = _dtTagPrint.Rows[0]["WIPHOLD"].ToString();

                        if (_dtTagPrint.Rows[0]["GRADE"].ToString().Equals("N"))
                            DefectPalletYN = "Y";
                    }
                }

                #region Print 태그 설정
                if (string.Equals(DefectPalletYN, "Y"))
                {
                    // 불량 Pallet
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPaletteDefect_NJ.xml";
                    ReportName = "FORM_GraderPaletteDefect_NJ";
                }
                else if (string.Equals(RemainPalletYN, "Y"))
                {
                    // 잔량 Pallet
                    //ReportNamePath = "LGC.GMES.MES.CMM001.Report.FOiRM_GraderPalette.xml";
                    //ReportName = "FORM_GraderPalette";
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette_NJ.xml";
                    ReportName = "FORM_GraderPalette_NJ";
                }
                else if (string.Equals(HoldPalletYN, "Y"))
                {
                    // Hold Pallet
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette.xml";
                    ReportName = "FORM_GraderPalette";
                }
                else
                {
                    ReportNamePath = "LGC.GMES.MES.CMM001.Report.FORM_GraderPalette_NJ.xml";
                    ReportName = "FORM_GraderPalette_NJ";
                }
                #endregion

                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream(ReportNamePath))
                {
                    cr.Load(stream, ReportName);

                    // 다국어 처리 및 Clear
                    for (int cnt = 0; cnt < cr.Fields.Count; cnt++)
                    {
                        if (cr.Fields[cnt].Name.Equals("TagName"))
                        {
                            if ((_procID.Equals(Process.CircularCharacteristicGrader) || _procID.Equals(Process.CircularReTubing)) && ReportName.Equals("FORM_GraderPalette_NJ"))
                            {
                                cr.Fields[cnt].Text += " (" + _dtTagPrint.Rows[0]["EQPTNAME"].ToString() + ")";
                            }
                            else
                            {
                                cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                            }
                        }

                        // 남경 Pallet, 불량 Pallet 인 경우 다국어 처리 제외
                        if (ReportName.Equals("FORM_GraderPalette_NJ") || ReportName.Equals("FORM_GraderPaletteDefect_NJ"))
                            continue;

                        // Title
                        if (cr.Fields[cnt].Name.IndexOf("txTitle", StringComparison.Ordinal) > -1)
                        {
                            if (string.Equals(HoldPalletYN, "Y") && ReportName.Equals("FORM_GraderPalette"))
                            {
                                if (cr.Fields[cnt].Name.Equals("txTitle_1"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("HOLD"));
                                else if (cr.Fields[cnt].Name.Equals("txTitle_2"))
                                    cr.Fields[cnt].Text = "";
                                else if (cr.Fields[cnt].Name.Equals("txTitle_3"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("Hold 사유"));
                                else if (cr.Fields[cnt].Name.Equals("txTitle_4"))
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC("비고"));
                                else
                                    cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));

                            }
                            else
                            {
                                cr.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(cr.Fields[cnt].Text));
                            }

                        }

                    }

                    txtPageCount.Text = string.IsNullOrWhiteSpace(PrintCount) ? "1" : PrintCount;

                    // Data Binding
                    if (ReportName.Equals("FORM_GraderPalette_NJ"))
                    {
                        for (int row = 0; row < _dtTagPrint.Rows.Count; row++)
                        {
                            for (int col = 0; col < _dtTagPrint.Columns.Count; col++)
                            {
                                string strColName = _dtTagPrint.Columns[col].ColumnName;
                                string strTagName = _dtTagPrint.Columns[col].ColumnName;
                                double dValue = 0;

                                if (strColName.Equals("QTY") || strColName.Equals("BOX") || strColName.Equals("PALLETID_BARCODE") || strColName.Equals("PALLETID") || strColName.Equals("GRADE"))
                                    strTagName = strTagName + (row + 1).ToString();

                                if (cr.Fields.Contains(strTagName))
                                {
                                    if (strColName.Equals("QTY") || strColName.Equals("BOX") || strColName.Equals("TOTAL_BOX") || strColName.Equals("TOTAL_QTY"))
                                    {
                                        if (double.TryParse(Util.NVC(_dtTagPrint.Rows[row][strColName]), out dValue))
                                        {
                                            cr.Fields[strTagName].Text = dValue.ToString("N0");
                                        }
                                    }
                                    else
                                    {
                                        cr.Fields[strTagName].Text = _dtTagPrint.Rows[row][strColName].ToString();
                                    }
                                }

                            }
                        }
                    }
                    else
                    {
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
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;


                if (_directPrint.Equals("Y"))
                {
                    var pm = new C1.C1Preview.C1PrintManager();
                    pm.Document = cr;
                    System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                    pm.Print(ps, ps.DefaultPageSettings);

                    SetLabelPrtHist();

                    if (_dispatch.Equals("Y"))
                    {
                        SetDispatch();
                    }

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
        #endregion

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
                string sBizRuleName = "BR_PRD_REG_LABEL_PRINT_HIST";

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
                    newRow["PRT_ITEM04"] = _rePrint;                                                   // 재발행 여부
                    newRow["INSUSER"] = LoginInfo.USERID;
                    newRow["LOTID"] = row["PALLETID"];
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = sBizRuleName;
                    inTable.Rows.Add(newRow);
                }

                //new ClientProxy().ExecuteService("BR_PRD_REG_LABEL_PRINT_HIST", "INDATA", null, inTable, (result, searchException) =>
                new ClientProxy().ExecuteService(sBizRuleName, "INDATA", null, inTable, (result, searchException) =>
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

        private void SetDispatch()
        {
            try
            {
                DataSet indataSet = _bizDataSet.GetBR_PRD_REG_DISPATCH_LOT_FD();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["REWORK"] = "N";
                newRow["EQPTID"] = _eqptID;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inDataTable = indataSet.Tables["INLOT"];

                ////newRow = inDataTable.NewRow();
                ////newRow["LOTID"] = _palletID;
                ////////newRow["ACTQTY"] = Util.NVC_Decimal(_cellQty);
                ////////newRow["ACTUQTY"] = 0;
                ////////newRow["WIPNOTE"] = "";

                ////inDataTable.Rows.Add(newRow);

                foreach (DataRow row in _dtTagPrint.Rows)
                {
                    newRow = inDataTable.NewRow();
                    newRow["LOTID"] = row["PALLETID"];
                    inDataTable.Rows.Add(newRow);
                }

                ////new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT", "INDATA,INLOT", null, indataSet);
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_DISPATCH_LOT_FO", "INDATA,INLOT", null, indataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }



        private string MCC_MCS_MCR_CHK(string sProdid)
        {
            string ReturnValue = string.Empty;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PRODID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PRODID"] = sProdid;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODID_MCC_MCS_MCR", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count > 0)
                {
                    ReturnValue = dtRslt.Rows[0]["CLSS3_CODE"].ToString();
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }


            return ReturnValue;

        }

        #endregion



    }
}
