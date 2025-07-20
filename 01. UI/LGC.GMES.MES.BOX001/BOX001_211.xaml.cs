/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이슬아
   Decription : 포장출고
--------------------------------------------------------------------------------------
 [Change History]
  2017.07.06  이슬아 : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_211 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;
    
        DataTable _SearchResult;
        DataTable _MergeResult;

        string _sPGM_ID = "BOX001_211";

        private static System.Windows.Threading.DispatcherTimer _timer = null;
        Util _util = new Util();
        public BOX001_211()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            Search();
        }

        #endregion

        #region Initialize


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            // 프린터 정보 조회
            _util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            if (_timer == null)
            {
                _timer = new System.Windows.Threading.DispatcherTimer();
                _timer.Interval = TimeSpan.FromMilliseconds(1000);//TimeSpan.FromSeconds(Properties.Settings.Default.Interval);
                _timer.Tick += _timer_Tick;
                _timer.IsEnabled = true;
            }
            else
            {
                if (!_timer.IsEnabled)
                    _timer.IsEnabled = true;
            }
        }

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            try
            {
                Search(true);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event

        #endregion

        #region Method

        /// <summary>
        /// 조회
        /// BIZ : BR_PRD_GET_OUTBOX_LABEL_REQ_NJ
        /// </summary>
        private void Search(bool bAll = false)
        {
            try
            {
                _util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

                DataSet ds = new DataSet();
                DataTable dtIndata = ds.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("EQPTID", typeof(string));
                dtIndata.Columns.Add("PRT_FLAG", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                if(!bAll) dr["PRT_FLAG"] = "N";
                dtIndata.Rows.Add(dr);

                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");

                dr = dtInPrint.NewRow();
                dr["PRMK"] = sPrt;
                dr["RESO"] = sRes;
                dr["PRCN"] = sCopy;
                dr["MARH"] = sXpos;
                dr["MARV"] = sYpos;
                dr["DARK"] = sDark;
                dtInPrint.Rows.Add(dr);
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_LABEL_REQ_NJ", "INDATA,INPRINT", "OUTDATA", ds);

                _SearchResult = dsResult.Tables["OUTDATA"];  //new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OUTBOX_LABEL_REQ_NJ", "INDATA", "OUTDATA", dtIndata);

                List<DataRow> drList = _SearchResult.Select("PRT_FLAG = 'N'").ToList();

                // 발행 여부 N 존재시 Update
                if (drList.Count > 0)
                { 
                    _timer.IsEnabled = false;
                    Update(drList);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //_timer.IsEnabled = true;
            }
        }

        /// <summary>
        /// 출력 후 Flag 업데이트 후 재 조회
        /// </summary>
        /// <param name="drList"></param>
        private void Update(List<DataRow> drList)
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dtRQSTDT = ds.Tables.Add("INDATA");

                dtRQSTDT.Columns.Add("LANGID");
                dtRQSTDT.Columns.Add("EQPTID");
                dtRQSTDT.Columns.Add("SCAN_ID");
                dtRQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(int));
                dtRQSTDT.Columns.Add("PRT_FLAG");
                dtRQSTDT.Columns.Add("PRT_DTTM");
                dtRQSTDT.Columns.Add("UPDUSER");
                dtRQSTDT.Columns.Add("UPDDTTM");

                foreach (DataRow dr in drList)
                {
                    // 재출력
                    if (dr["LABEL_CODE"].ToString().Equals("LBL0102"))
                    {
                        dr["LABEL_ZPL_CNTT"] = dr["LABEL_ZPL_CNTT"].ToString().Replace("^GFA", "^GFA,01660,01660,00020,"
                        + "00,00000000000000000C,000000000000000033,0000000000000000C0C0,0000000000000001"
                        + "0020,00000000000000060018,00000000000000180006,00000000000000200001,0000000000"
                        + "0000C00000C0,0000000000000300000030,0000000000000C0000000C,00000000000010000000"
                        + "02,000000000000600000000180,000000000001800000000060,000000000002000000000010,00"
                        + "000000000C00000000000C,000000000030000000000003,00000000004000000000000080,0000"
                        + "0000018000000000000060,00000000060000000000000018,00000000080000000000000004,00"
                        + "000000300000000000000003,00000000C00000000000000000C0,000000010000000000000000"
                        + "0020,0000000600000000000000000018,0000001800000000000000000006,0000002000000000"
                        + "000000000001,000000C000000000000000000000C0,000003000000000000000000000030,0000"
                        + "04000000000000000000000008,000018000000000000000000000006,00006000000000000000"
                        + "000000000180,00018000000000000000000000000060,00020000000000000000000000000010"
                        + ",000C000000000000000000000000000C,00300000000000000000000000000003,004000000000"
                        + "0000000000000000000080,0180000000000000000000000000000060,06000000000000000000"
                        + "00000000000018,0800000000000000000000000000000004,3000000000000000000000000000"
                        + "000003,400000000000000000000000000000000080,3000000000000000000000000000000003"
                        + ",0800000000000000000000000000000004,0600000000000000000000000000000018,01800000"
                        + "00000000000000000000000060,0040000000000000000000000000000080,0030000000000000"
                        + "0000000000000003,000C000000000000000000000000000C,0002000000000000000000000000"
                        + "0010,00018000000000000000000000000060,00006000000000000000000000000180,00001800"
                        + "0000000000000000000006,000004000000000000000000000008,000003000000000000000000"
                        + "000030,000000C000000000000000000000C0,0000002000000000000000000001,000000180000"
                        + "0000000000000006,0000000600000000000000000018,0000000100000000000000000020,0000"
                        + "0000C00000000000000000C0,00000000300000000000000003,00000000080000000000000004"
                        + ",00000000060000000000000018,00000000018000000000000060,000000000040000000000000"
                        + "80,000000000030000000000003,00000000000C00000000000C,000000000002000000000010,00"
                        + "0000000001800000000060,000000000000600000000180,0000000000001000000002,00000000"
                        + "00000C0000000C,0000000000000300000030,00000000000000C00000C0,000000000000002000"
                        + "01,00000000000000180006,00000000000000060018,00000000000000010020,000000000000"
                        + "0000C0C0,000000000000000033,00000000000000000C,00,");
                    }

                    if (!PrintZPL((string)dr["LABEL_ZPL_CNTT"], drPrtInfo))
                        return;

                    DataRow drnewrow = dtRQSTDT.NewRow();
                    drnewrow["LANGID"] = LoginInfo.LANGID;
                    drnewrow["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                    drnewrow["SCAN_ID"] = dr["SCAN_ID"];
                    drnewrow["PRT_REQ_SEQNO"] = dr["PRT_REQ_SEQNO"];
                    drnewrow["UPDUSER"] = LoginInfo.USERID;

                    dtRQSTDT.Rows.Add(drnewrow);
                }

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTBOX_LABEL_PRT_NJ", "INDATA", "OUTDATA", ds);

                DataTable result = dsResult.Tables["OUTDATA"];

                if (!result.Columns.Contains("CHK"))
                    result.Columns.Add("CHK");
                Util.GridSetData(dgSearhResult, result, FrameOperation, true);

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                _timer.IsEnabled = true;
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

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            _util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
            DataTable dtInfo = DataTableConverter.Convert(dgSearhResult.ItemsSource);
            List<DataRow> drList = dtInfo.Select("CHK = 'True'").ToList();
            try
            {
                if (drList.Count > 0)
                {
                    string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                    DataSet ds = new DataSet();
                    DataTable dtInData = ds.Tables.Add("INDATA");
                    dtInData.Columns.Add("LANGID");
                    dtInData.Columns.Add("USERID");
                    dtInData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                    dtInData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                    DataRow newRow = null;
                    newRow = dtInData.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["USERID"] = LoginInfo.CFG_EQPT_ID;
                    newRow["PGM_ID"] = _sPGM_ID;
                    newRow["BZRULE_ID"] = sBizRule;
                    dtInData.Rows.Add(newRow);

                    DataTable dtInbox = ds.Tables.Add("INBOX");
                    dtInbox.Columns.Add("BOXID");
                    foreach (DataRow dRow in drList)
                    {
                        newRow = dtInbox.NewRow();
                        newRow["BOXID"] = dRow["LOTID"].ToString();
                        dtInbox.Rows.Add(newRow);

                    }
                    //for (int i = 0; i < dgBox.Rows.Count; i++)
                    //{
                    //    dr = dtInData.NewRow();
                    //    boxID = Util.NVC(dgBox.GetCell(idxPlt, dgBox.Columns["BOXID"].Index).Value);
                    //    dr["BOXID"] = boxID;
                    //    dr["USERID"] = txtWorker.Tag;
                    //}
                    DataTable dtInPrint = ds.Tables.Add("INPRINT");
                    dtInPrint.Columns.Add("PRMK");
                    dtInPrint.Columns.Add("RESO");
                    dtInPrint.Columns.Add("PRCN");
                    dtInPrint.Columns.Add("MARH");
                    dtInPrint.Columns.Add("MARV");
                    dtInPrint.Columns.Add("DARK");

                    newRow = dtInPrint.NewRow();
                    newRow["PRMK"] = sPrt;
                    newRow["RESO"] = sRes;
                    newRow["PRCN"] = sCopy;
                    newRow["MARH"] = sXpos;
                    newRow["MARV"] = sYpos;
                    newRow["DARK"] = sDark;
                    dtInPrint.Rows.Add(newRow);

                    //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRT_NJ", "INDATA,INBOX,INPRINT", "OUTDATA", ds);
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizRule, "INDATA,INBOX,INPRINT", "OUTDATA", ds);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        DataTable dtResult = dsResult.Tables["OUTDATA"];
                        string zplCode = string.Empty;
                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            zplCode += dtResult.Rows[i]["ZPLCODE"].ToString();
                        }
                        PrintZPL(zplCode, drPrtInfo);
                    }
                }
                else
                {
                    //SFU1645		선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
            
        }
    }
}
