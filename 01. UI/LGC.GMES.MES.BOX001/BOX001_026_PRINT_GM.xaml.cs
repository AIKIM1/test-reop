/*************************************************************************************
 Created Date : 2023.06.13
      Creator : 윤지해
   Decription : 포장 Pallet 구성(Box) - GM향 라벨 발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2023.06.13  윤지해 : 최초생성
  
**************************************************************************************/

using System;
using System.Windows;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_026_PRINT_GM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        // 팝업 초기변수 설정
        private string _sPGM_ID = "BOX001_026_PRINT_GM";
        private string _workUserID = string.Empty;
        private string _BoxID = string.Empty;
        private string _totalQty = string.Empty;
        private string _packDttm = string.Empty;

        public BOX001_026_PRINT_GM()
        {
            InitializeComponent();
        }

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
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            // 팝업 파라미터 변수 설정
            _workUserID = Util.NVC(tmps[0]);
            _BoxID = Util.NVC(tmps[1]);
            _totalQty = Util.NVC(tmps[2]);
            _packDttm = Util.NVC(tmps[3]);

            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
        }
        
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();

            txtPrintCnt.Text = "2";
        }

        // 프린트 이벤트
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!Valid())
                return;
            try
            {
                // LABEL_CODE = 'LBL0323'
                string sBizRule = "BR_PRD_GET_BOX_LABEL_NA_H07B";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("BOXID", typeof(string));
                inDataTable.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("PGM_ID", typeof(string));
                inDataTable.Columns.Add("BZRULE_ID", typeof(string));
                inDataTable.Columns.Add("TOTAL_QTY", typeof(string));
                inDataTable.Columns.Add("SHIPMENT_NO", typeof(string));
                inDataTable.Columns.Add("PACKDTTM", typeof(DateTime));

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["BOXID"] = _BoxID;
                newRow["LABEL_TYPE_CODE"] = "OUTBOX";
                newRow["USERID"] = _workUserID;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;
                newRow["TOTAL_QTY"] = _totalQty;
                newRow["SHIPMENT_NO"] = String.IsNullOrWhiteSpace(txtBatchNo.Text.ToString().Trim()) ? null : txtBatchNo.Text.ToString().Trim();
                newRow["PACKDTTM"] = _packDttm;

                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                for(int i = 0; i < int.Parse(txtPrintCnt.Text); i++)
                {
                    new ClientProxy().ExecuteService_Multi(sBizRule, "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            string zplCode = string.Empty;
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            for (int j = 0; j < bizResult.Tables["OUTDATA"].Rows.Count; j++)
                            {
                                zplCode = bizResult.Tables["OUTDATA"].Rows[j]["ZPLCODE"].ToString();
                                if (sRes.Equals("300"))
                                {
                                    #region 300dpi 치환
                                    zplCode = zplCode.Replace("^FO615,814^GFA", "^FO615,814^GFA,03020,03020,00020,"
                                                                + "00,00,00,00,00,00,00,00,00,00,00,00,0000000000000004555540,00000000000000000220"
                                                                + ",000000000000015FFFFFFD50,00000000000000AABFFBAA80,00000000000057FFFFFFFFFD40,00"
                                                                + "00000000002AFFFFFFFFEA,000000000001FFFFFFFFFFFFD0,00000000000BFFFFAAAABFFFFA,00"
                                                                + "000000001FFFFFFF77FFFFFD,0000000002FFFFA0000002BFFFE0,0000000001FFFFD1000015FF"
                                                                + "FFD0,000000000BFFE8000000000AFFF8,0000000017FFF50000000017FFF4,000000003FFF8000"
                                                                + "00000000AFFE80,000000005FFF4000000000007FFD80,00000000FFF80000000000000BFFA8,00"
                                                                + "000005FFF400000000000007FFF0,00000007FFC000000000000000FFF8,00000017FFD0000000"
                                                                + "00000001FFF8,0000000FFE0000000000000000BFFA,0000005FFF4000000000000001FFFE,0000"
                                                                + "003FE8000000000000000BFFFE80,0000005FFC000000000000001FFFFF80,000000FFA0000000"
                                                                + "00000000BFFFFFA0,0000017FE000000000000001FFFFFFE0,000003FE800000000000002FFFFF"
                                                                + "FFE0,000005FF800000000000001FFFFFFFF0,000003FA00000000000000BFFF8FFFF8,000017FE"
                                                                + "00000000000005FFFF5FFFFC,00000FE80000000000002FFFE80FFFFA,000017FC000000000001"
                                                                + "5FFFF50FFFFC,00001FE8000000000003FFFF800FE9FA,00005FF0000000000017FFFF400FFDFF"
                                                                + ",00003FA000000000003FFFE8000FE8FE80,00005FE000000000015FFFF4000FF9FF,00007F8000"
                                                                + "0000000BFFFA80000FE87F80,00017FC00000000017FFFF40000FF97FC0,0000FE8000000000FF"
                                                                + "FFA000000FE83FA0,0001FF80000000017FFFD000000FF85FC0,0001FE800000002BFFFA000000"
                                                                + "0FE81FA0,0005FF000000005FFFFF4000000FF85FF0,0001FA00000002FFFFA00000000FE80FE0"
                                                                + ",0005FE00000001FFFFD00000001FF81FF0,0003FA0000002BFFFA000000000FE80FE8,0007FC00"
                                                                + "0007FFFFC0000000000FF817FC,0007FA000002FFFEA0000000000FE807E8,0017F800007FFFF4"
                                                                + "00000000000FF807FC,0007E800002FFFE800000000000FE803F8,0017F00057FFFF4000000000"
                                                                + "000FF807FC,002FE80002FFFE0000000000000FE803FA,001FF0017FFFF00000000000000FF805"
                                                                + "FC,002FE800BFFFE00000000000000FE801FA,003FF057FFFF400000000000000FF805FE,002FE0"
                                                                + "0FFFFE000000000000000FE803FA,003FF1FFFFD0000000000000000FF805FE,002FA0BFFFA000"
                                                                + "0000000000000FE801FA,007FFFFFFD40000000000000000FF805FE40,002FEFFFEE0000000000"
                                                                + "0000000FE800FE,003FFFFFC000000000000000001FF801FF,003FFFFAA000000000000000000F"
                                                                + "E800FA,007FFFFC0000000000000000000FF801FF40,003FFFEA0000000000000000000FE800FE"
                                                                + ",003FFFFD0000000000000000000FF801FF,003FFFFA0000000000000000000FE800FE,007FFFFF"
                                                                + "F400000000000000000FF801FE40,003FFFFFA000000000000000000FE800FE,003FF7FFFD4000"
                                                                + "0000000000000FF801FE,003FABFFFA00000000000000000FE801FA,003FF57FFFF40000000000"
                                                                + "00000FF805FE40,002FE07FFFE0000000000000000FE801FE,001FF017FFFF400000000000000F"
                                                                + "F805FE,002FA003FFFF800000000000000FE801FA,0037F0017FFFF40000000000000FF805FE,00"
                                                                + "2FE8003FFFE80000000000000FE803FA,0017F80011FFFF5000000000001FF805FC,002FE80002"
                                                                + "FFFF8000000000000FE803F8,0017F800007FFFF700000000000FF807FC,0007E800002FFFFA00"
                                                                + "000000000FE807F8,0007FC000001FFFFD0000000000FF817F8,0003F8000002BFFFA000000000"
                                                                + "0FE807E8,0007FE0000005FFFFF000000000FF817F0,0003FA0000002FFFFA000000000FE80FE8"
                                                                + ",0005FE00000001FFFFD00000000FF81FF0,0001FA800000000BFFFA8000000FE81FA0,0005FF00"
                                                                + "0000001FFFFF4000000FF85FE0,0000FE80000000003FFFE800000FE83FA0,00017F8000000001"
                                                                + "5FFFF100000FF87FC0,00003F800000000003FFFB80000FE8FF80,00017FC00000000017FFFF40"
                                                                + "000FF97FC0,00003FA000000000003FFFE8000FE8FE80,00005FE000000000015FFFF5001FF9FF"
                                                                + ",00001FE8000000000000FFFF800FE9FA,00005FF0000000000005FFFFD00FFDFF,00000FF80000"
                                                                + "000000002FFFF80FFFFA,000017FC0000000000015FFFFD0FFFFC,000003FA00000000000000BF"
                                                                + "FF8FFFE8,000007FE00000000000005FFFFDFFFFC,000001FE800000000000002BFFFFFFE0,0000"
                                                                + "05FF800000000000001FFFFFFFF0,000000FFA000000000000000BFFFFFA0,0000017FE4000000"
                                                                + "00000001FFFFFFC0,0000003FE8000000000000000BFFFE80,0000005FFD000000000000001FFF"
                                                                + "FF80,0000000FFE0000000000000000BFFA,0000005FFF4000000000000000FFFE,00000003FFE0"
                                                                + "00000000000000FFF8,00000017FFD000000000000001FFF8,00000000FFF80000000000000BFF"
                                                                + "A8,00000005FFF500000000000007FFF0,000000003FFF800000000000BFFE80,000000005FFF40"
                                                                + "00000000017FFD80,000000000BFFE8000000000BFFF8,0000000017FFF54000000057FFF4,0000"
                                                                + "000000BFFFE2000002FFFF80,0000000001FFFFD5000115FFFFD0,00000000000BFFFBAAAABFFF"
                                                                + "F8,00000000001FFFFFFFFFFFFFFD,000000000002FFFFFFFFFFFFE0,000000000001FFFFFFFFFF"
                                                                + "FFD0,0000000000000BFFFFFFFFE8,000000000000007FFFFFFD40,000000000000002AFFFEEA,00"
                                                                + "000000000000001150,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,00,");
                                    #endregion

                                }
                                else
                                {
                                    #region 203dpi 치환
                                    zplCode = zplCode.Replace("^FO416,488^GFA", "^FO416,488^GFA,01648,01648,00016,"
                                                                + "00,00,00,00,00,00,00,00,0000000000455440,0000000002AFFEA0,0000000001FFFFD0,0000"
                                                                + "00000BFFFFFA80,000000017FFFFFFFF0,0000000BFFEAAAFFFA,00000007FFFD5FFFFC,000000"
                                                                + "0FFA80000BFA,0000007FF4000005FFE0,000000FF800000002FE8,000001FFC00000005FF8,00"
                                                                + "0003FE000000000FFA,00001FF40000000005FE,00002FE00000000000FE80,00003FF000000000"
                                                                + "01FF80,00003F80000000000FFF80,00007F00000000007FFFD0,0002FA0000000002FFFFF0,00"
                                                                + "03F8000000001FFF7FF8,0003F8000000000FFABFF8,0007F0000000017FF47FFC,0007E0000000"
                                                                + "0BFFA03E7C,0017C00000005FFD003E7D,000F800000003FF8003E3E,001FC0000005FFF4007E7F"
                                                                + ",003F8000000FFE80003E2F80,003F0000017FFC00003E1F80,003F000000BFF800003E0F80,00"
                                                                + "7F000007FFF000007E0FC0,003E00003FFE0000003E0FA0,007D0001FFF00000003E07F0,003800"
                                                                + "00FFF80000003E03A0,017C001FFFC00000007E07F0,00F8003FFE000000003E03E0,01FC05FFF0"
                                                                + "000000003E07F0,00F801FFA0000000003E03E0,01FC5FFF40000000007E07F0,00F8FFE8000000"
                                                                + "00003E02E0,01FFFFD000000000003E03F0,01FBFFA000000000003E03F0,01FFFF400000000000"
                                                                + "7E07F4,03FFE00000000000003E02F0,01FFF00000000000003E03F0,01FFF80000000000003E02"
                                                                + "F0,01FFFF4000000000007E03F4,00FAFFE000000000003E02E0,01F87FFC00000000003E03F0,00"
                                                                + "F83FF800000000003E03E0,01FC1FFFC0000000007E07F4,00F800FFE0000000003E03E0,017C00"
                                                                + "1FFD000000003E07F0,00F8003FFA000000003E03E0,017C0007FFC00000007E07F0,007C0000FF"
                                                                + "F80000003E07E0,007D00001FFF4000003E07C0,003E00003FFA0000003E0FA0,007F000007FFF4"
                                                                + "00007E0FC0,003F0000003FFA00003E2F80,001F8000001FFFC0003E3F80,003F8000000FFE8000"
                                                                + "3E2F80,001FC0000005FFF4007E7F,000F800000002FFE003E3E,0017D000000017FFD03F7D,00"
                                                                + "03C00000000BFFA03E78,0007F0000000017FFC7FFC,0002F8000000002FFEBFF8,0001FE000000"
                                                                + "0007FFFFF0,0000FA0000000002FFFFF0,00007F00000000007FFFD0,00003F80000000000FFF80"
                                                                + ",00003FF00000000001FF80,00000FF00000000001FE,00000FF40000000005FE,000003FE0000"
                                                                + "00000FFE,000001FFC00000005FF0,000000BFE8000002FFA0,0000007FF4000005FFC0,000000"
                                                                + "2FFEA0002FFE,00000007FFFFDFFFFC,00000000BFFFFFFFA0,000000017FFFFFFFF0,00000000"
                                                                + "2FFFFFFE,00000000017FFFD0,00,0000000000455440,00,00,00,00,00,00,00,00,00,00,");
                                    #endregion
                                }

                                PrintLabel(zplCode, drPrtInfo);
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, indataSet);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            // 프린터 환경설정 정보가 없습니다.
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                // Barcode Print 실패
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                // 프린터 환경설정에 포트명 항목이 없습니다.
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private bool Valid()
        {
            if (String.IsNullOrWhiteSpace(txtBatchNo.Text.ToString().Trim()))
            {
                // 배치 번호를 입력하세요.
                Util.MessageInfo("SFU4487");
                return false;
            }

            if (String.IsNullOrWhiteSpace(txtPrintCnt.Text.ToString().Trim()))
            {
                // 발행 수량을 확인하세요.
                Util.MessageInfo("SFU4085");
                return false;
            }

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
