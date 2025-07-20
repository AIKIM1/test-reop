/*************************************************************************************
 Created Date : 2023.07.24
      Creator : 이병윤
   Decription : E20230419-000979 : 자동 포장 구성(원/각형) > 추가기능 > Non IM OUTBOX발행 클릭시 OUTBOX 생성 팝업화면
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// FCS002_303_NONIMOUTBOX.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_303_LBL0085 : C1Window, IWorkArea
    {
        #region [Declaration & Constructor]
        Util _util = new Util();
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        string sPGMID  = string.Empty;
        string sBoxId  = string.Empty;
        string sZplCd  = string.Empty;

        public string AREAID
        {
            get;
            set;
        }
        public string PALLETID
        {
            get;
            set;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public FCS002_303_LBL0085()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            sBoxId = Util.NVC(tmps[0]);
            sZplCd = Util.NVC(tmps[1]);
            InitControl();
            SearchOutBox();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            // 프린터 정보 조회
            _util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

        }
        #endregion

        #region [Events]
        /// <summary>
        /// Print 버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 프린터 정보 조회
            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;

            DataRow drPrtInfo = null;
           
            //바코드일때 셋팅값 확인
            if (!_util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            // 인쇄 하시겟습니까? 
            Util.MessageConfirm("SFU1237", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    if(PrintZPL(sZplCd, drPrtInfo))
                    {
                        // 인쇄 완료 되었습니다.
                        Util.MessageInfo("SFU1236");    //정상 처리 되었습니다.
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        
        #endregion

        #region [Method]
        /// <summary>
        /// 생성 Outbox 조회
        /// </summary>
        private void SearchOutBox()
        {
            try
            {
                // 라벨타입조회
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("OUTER_BOXID2", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID2"] = sBoxId;
                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_INBOX_LIST_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtRslt != null && dtRslt.Rows.Count > 0)
                {
                    Util.GridSetData(dgResult, dtRslt, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [바코드 프린터 발행용]
        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        #endregion
        #endregion
    }
}
