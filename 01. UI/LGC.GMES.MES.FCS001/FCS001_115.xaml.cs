/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : Tray ID 발행(2D)
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using System.Linq;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_115 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        Util _Util = new Util();

        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;

        DataRow drPrtInfo = null;

        public FCS001_115()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            InitCombo();
            InitControl();
            //getLabelPsoi();
                
            rdoNewPrint.Checked += rdoNewPrint_Checked;
            rdoRePrint.Checked += rdoRePrint_Checked;

            rdoNewPrint_Checked(null, null);
        }
        
        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            //dtpToDate.SelectedDateTime = DateTime.Now.AddDays(-1);
        }

        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            _combo.SetCombo(cboTrayType, CommonCombo_Form.ComboStatus.NONE, sCase: "TRAYTYPE");
            _combo.SetCombo(cboHistTrayType, CommonCombo_Form.ComboStatus.ALL, sCase: "TRAYTYPE");
        }
        #endregion
        
        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }

        private void rdoNewPrint_Checked(object sender, RoutedEventArgs e)
        {
            txtTrayId.IsReadOnly = true;
            txtTrayId.IsEnabled = false;
            txtTrayId.Clear();
            txtPerCnt.IsReadOnly = true;
            txtPerCnt.IsEnabled = false;
            txtPerCnt.Text = "4";
            txtTrayCnt.IsReadOnly = false;
            txtTrayCnt.IsEnabled = true;
            txtTrayCnt.Text = "10";
        }

        private void rdoRePrint_Checked(object sender, RoutedEventArgs e)
        {
            txtTrayId.IsReadOnly = false;
            txtTrayId.IsEnabled = true;
            txtPerCnt.IsReadOnly = false;
            txtPerCnt.IsEnabled = true;
            txtPerCnt.Text = "1";
            txtTrayCnt.IsReadOnly = true;
            txtTrayCnt.IsEnabled = false;
            txtTrayCnt.Text = "1";
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string IssueID = string.Empty;
                string IssueIDHeader = string.Empty;
                string IssueFlag = string.Empty; // I이면 신규발행 , R이면 재발행

                string[] IssueIDList = new string[int.Parse(txtTrayCnt.Text.Trim())];

                //신규 발행이면 할당을 가져옴
                if (rdoNewPrint.IsChecked == true)
                {
                    //신규 발행이면 할당을 가져옴
                    if (getAssignid() != 0)
                    {
                        Util.MessageInfo("FM_ME_0255");  //할당 정보를 가져오는중 에러가 발생하였습니다.
                        return;
                    }

                    if (string.IsNullOrEmpty(txtFromId.Text) || string.IsNullOrEmpty(txtToId.Text))
                    {
                        Util.MessageInfo("FM_ME_0254");  //할당 정보가 존재하지 않습니다.
                        return;
                    }

                    IssueFlag = "I"; //신규발행 Flag
                }
                else //재발행이면 발행 가능한지 여부 체크
                {
                    //입력된 값이 없거나 10자리가 아니면 에러
                    if (string.IsNullOrEmpty(txtTrayId.Text.Trim()) || (txtTrayId.Text.Trim()).Length != 10)
                    {
                        Util.MessageInfo("FM_ME_0208");  //재발행 ID가 입력되지 않았거나 10자리가 아닙니다.
                        return;
                    }

                    if (checkReissualbe(txtTrayId.Text) != 0) //재발행이력이 없을 경우 에러                    
                    {
                        Util.MessageInfo("FM_ME_0133");  //발행 이력이 없는 Tray ID는 재발행 할 수 없습니다.
                        return;
                    }
                    IssueFlag = "R"; //재발행 Flag
                }

                //발행할 Tray ID를 증가시킴
                for (int intLoop = 0; intLoop < int.Parse(txtTrayCnt.Text); intLoop++)
                {
                    if (intLoop == 0)
                    {
                        //재발행이면 입력한 ID를 넘겨줌.
                        if (rdoRePrint.IsChecked == true)
                        {
                            IssueID = txtTrayId.Text;  //입력한 ID를 가져옴.
                            IssueIDList[intLoop] = IssueID;
                        }
                        else  //신규 발행이면 할당된 ID를 가져옴.
                        {
                            IssueID = txtFromId.Text;  //할당된 발행 ID를 가져옴
                            IssueIDList[intLoop] = IssueID;
                        }
                        IssueIDHeader = IssueID.Substring(0, 4);
                    }
                    else
                    {
                        IssueID = IssueIDHeader + (double.Parse(IssueID.Substring(4, 6)) + 1).ToString("000000"); //기존 발행 ID에서 하나 증가시킴
                        IssueIDList[intLoop] = IssueID;
                    }

                    InserIssueHist(IssueID, IssueFlag, txtTrayCnt.Text.Trim(), IssueIDHeader, LoginInfo.USERID);
                    System.Threading.Thread.Sleep(100);
                }

                DataTable dt = new DataTable();
                dt.TableName = "TRAYLIST";
                dt.Columns.Add("TRAY_ID", typeof(string));

                for (int i = 0; i < IssueIDList.Length; i++)
                {
                    DataRow row1 = dt.NewRow();
                    row1["TRAY_ID"] = IssueIDList[i].ToString();
                    dt.Rows.Add(row1);
                }

                var query = (from t in dt.AsEnumerable()
                             select t).ToList();

                if (query.Any())
                {
                    Util.MessageConfirm("SFU1540", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            PrintLabel(query, int.Parse(txtPerCnt.Text));
                        }
                    });
                }


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetLastList();
        }

        #endregion

        #region [Method]
        //private void PrintLabel(object[] PrintData, string IssueFlag)
        //{
        //    try
        //    {
        //        string sTrayID = PrintData[0] as string;
        //        string sCnt = PrintData[1] as string;

        //        #region 공통꺼
        //        /* - 공통에 있는거 쓰지말자
        //        //string sLHXPos = string.Empty;
        //        //string sLHYPos = string.Empty;
        //        string sMD = string.Empty;

        //        foreach (DataRow row in LoginInfo.CFG_SERIAL_PRINT.Rows)
        //        {
        //            if (Boolean.Parse(Util.NVC(row["DEFAULT"])) == true)
        //            {
        //                //sLHXPos = string.IsNullOrEmpty(Util.NVC(row["X"])) ? "0" : Util.NVC(row["X"]);
        //                //sLHXPos = string.IsNullOrEmpty(Util.NVC(row["Y"])) ? "0" : Util.NVC(row["Y"]);
        //                sMD = string.IsNullOrEmpty(Util.NVC(row["DARKNESS"])) ? "15" : Util.NVC(row["DARKNESS"]);
        //                break;
        //            }
        //        }
        //        */
        //        #endregion

        //        String PrintCode;
        //        string ZplCode = string.Empty;

        //        ZplCode += "^XA";
        //        ZplCode += "^LH{0},{1}";
        //        ZplCode += "^MD{8}";
        //        ZplCode += "^FO{4}"; //바코드위치
        //        ZplCode += "^BY{5}"; //바코드 옵션
        //        if (sTrayID != "Separation")
        //        {
        //            ZplCode += "^B3N,N,N,N";
        //            ZplCode += "^FD{2}";
        //            ZplCode += "^FS";
        //        }
        //        ZplCode += "^FO{6}"; //글씨위치
        //        ZplCode += "^ADN,{7}";//글씨크기
        //        ZplCode += "^FD{2}";
        //        ZplCode += "^FS";
        //        ZplCode += "^PQ{3}";
        //        ZplCode += "^XZ";

        //        PrintCode = string.Format(ZplCode, "   " + txtXAXIS.Text, // 0 X축
        //                                                   txtYAXIS.Text, // 1 Y축
        //                                                   sTrayID,       // 2 Tray ID
        //                                                   sCnt,         // 3  발행 수량
        //                                                   txtBarcodeXY.Text, // 4 바코드 위치
        //                                                   txtBarcodeOption.Text, // 5 바코드 옵션
        //                                                   txtBarTextXY.Text, // 6 글씨 위치
        //                                                   txtBarTextSize.Text,// 7 글씨 크기
        //                                                   cboDarkness.SelectedValue.ToString()); //진하기

        //        Util.PrintLabel(FrameOperation, loadingIndicator, PrintCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public void InserIssueHist(string I_TRAYID, string I_TYPE, string I_POSI, string I_REMARK, string I_USER)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));
                dtRqst.Columns.Add("LABEL_CODE", typeof(string));
                dtRqst.Columns.Add("PRT_QTY", typeof(int));
                dtRqst.Columns.Add("REMARKS_CNTT", typeof(string));
                dtRqst.Columns.Add("USERID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = I_TRAYID;
                dr["LABEL_CODE"] = I_TYPE;
                dr["PRT_QTY"] = int.Parse(I_POSI);
                dr["REMARKS_CNTT"] = I_REMARK;
                dr["USERID"] = I_USER;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_INS_TB_SFC_CST_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRqst);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private int checkReissualbe(string trayID)
        {
            int iResult = -1;
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["CSTID"] = trayID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_SFC_CST_LABEL_PRT_HIST", "RQSTDT", "RSLTDT", dtRqst);

                if (dtRslt.Rows.Count <= 0)
                    iResult = -1;       //기존 발행 이력 없을 때는 에러처리
                else
                    iResult = 0;        //기존 발행 이력 있을 경우 정상
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            return iResult;
        }

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("TRAY_TYPE", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TRAY_TYPE"] = Util.GetCondition(cboHistTrayType, bAllNull: true);
                dr["FROM_DATE"] = dtpFromDate.SelectedDateTime.ToString("yyyyMMdd000000");
                dr["TO_DATE"] = dtpToDate.SelectedDateTime.ToString("yyyyMMdd235959");

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LABEL_HIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgHist, dtRslt, FrameOperation);

                GetLastList();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLastList()
        {
            try
            {
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_TRAY_LAST_ID_BY_TRAY_TYPE", "RQSTDT", "RSLTDT", null);
                Util.GridSetData(dgLastTray, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private int getAssignid()
        {
            int lngResult = -1;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("I_TYPE", typeof(string));
                dtRqst.Columns.Add("I_HEADER", typeof(string));
                dtRqst.Columns.Add("I_CNT", typeof(string));
                dtRqst.Columns.Add("I_USER", typeof(string));
                dtRqst.Columns.Add("I_AREAID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["I_TYPE"] = Util.GetCondition(cboTrayType);
                dr["I_HEADER"] = "0";
                dr["I_CNT"] = Util.GetCondition(txtTrayCnt, bAllNull: true);
                dr["I_USER"] = LoginInfo.USERID;
                dr["I_AREAID"] = LoginInfo.CFG_AREA_ID;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_ASSIGN_LOTID", "RQSTDT", "RSLTDT", dtRqst);

                txtFromId.Text = dtRslt.Rows[0]["O_ID_FROM"].ToString();
                txtToId.Text = dtRslt.Rows[0]["O_ID_TO"].ToString();

                lngResult = 0;
                return lngResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return lngResult;
            }
        }

        private void PrintLabel(List<DataRow> query, int PrintCnt)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                const string bizRuleName = "DA_SEL_LABEL_PRINT_BY_TRAYID";

                const string item001 = "ITEM001";
                const string item002 = "ITEM002";

                string labelCode = "LBL0290";//2D BarCode

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LABEL_CODE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LABEL_CODE"] = labelCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, bizException) =>
                {
                    if (bizException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(bizException);
                        return;
                    }


                    if (CommonVerify.HasTableRow(result))
                    {
                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()))
                            {
                                // Setting 에 설정된 BarCord Print 정보를 통하여 일치하는 zpl 코드를 가져옴
                                string resolution = dr["DPI"].GetString();
                                string printmodel = dr["PRINTERTYPE"].GetString();
                                string portName = dr["PORTNAME"].GetString();

                                var zplText = (from t in result.AsEnumerable()
                                               where t.Field<string>("PRTR_RESOL_CODE") == resolution
                                                     && t.Field<string>("PRTR_MDL_ID") == printmodel
                                               select new { zplCode = t.Field<string>("DSGN_CNTT") }).FirstOrDefault();

                                if (zplText != null)
                                {
                                    foreach (var item in query)
                                    {
                                        string sBarCode = item["TRAY_ID"].GetString();
                                        string sBarCodeText = sBarCode;

                                        string zplCode =
                                            zplText.zplCode.Replace(item001, sBarCodeText)
                                                .Replace(item002, sBarCode);

                                        for (int Cnt = 0; Cnt < PrintCnt; Cnt++)
                                        {
                                            bool iszplPrint = portName.ToUpper().Equals("USB")
                                                ? FrameOperation.Barcode_ZPL_USB_Print(zplCode)
                                                : FrameOperation.Barcode_ZPL_Print(dr, zplCode);

                                            if (iszplPrint == false)
                                            {
                                                loadingIndicator.Visibility = Visibility.Collapsed;
                                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                                return;
                                            }
                                            System.Threading.Thread.Sleep(500);
                                        }
                                    }

                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                    return;
                                }
                            }
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageInfo("FM_ME_0126");  //라벨 발행을 완료하였습니다.
                        GetLastList();
                    }
                    else
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        //private void getLabelPsoi()
        //{
        //    try
        //    {
        //        string xAxis = string.Empty;
        //        string yAxis = string.Empty;
        //        string barcodeXY = string.Empty;
        //        string barcodeOption = string.Empty;
        //        string barTextXY = string.Empty;
        //        string barTextSize = string.Empty;

        //        RegistryKey reg;
        //        Registry.CurrentUser.CreateSubKey(@"Software\LG Chem\FCS_UI");
        //        reg = Registry.CurrentUser.OpenSubKey(@"Software\LG Chem\FCS_UI", true);

        //        xAxis = reg.GetValue("XAXIS", "45").ToString(); //X 좌표 저장
        //        yAxis = reg.GetValue("YAXIS", "0").ToString(); //Y 좌표 저장
        //        barcodeXY = reg.GetValue("BARCODEXY", "28,80").ToString(); //바코드 위치
        //        barcodeOption = reg.GetValue("BARCODEOPTION", "4,2.5,170").ToString(); //바코드 옵션
        //        barTextXY = reg.GetValue("BARTEXTXY", "90,20").ToString(); //글씨 위치
        //        barTextSize = reg.GetValue("BARTEXTSIZE", "56,45").ToString(); //글씨 크기

        //        txtXAXIS.Text = xAxis;
        //        txtYAXIS.Text = yAxis;
        //        txtBarcodeXY.Text = barcodeXY;
        //        txtBarcodeOption.Text = barcodeOption;
        //        txtBarTextXY.Text = barTextXY;
        //        txtBarTextSize.Text = barTextSize;
        //    }
        //    catch (Exception e)
        //    {
        //        setLabelPsoi("45", "0", "28,80", "4,2.5,170", "90,20", "56,45");
        //    }
        //}

        //private void setLabelPsoi(string sXposi, string sYposi, string sBarcodeXY, string sBarcodeOption, string sBarTextXY, string sBarTextSize)
        //{
        //    try
        //    {
        //        RegistryKey reg;
        //        Registry.CurrentUser.CreateSubKey(@"Software\LG Chem\FCS_UI");
        //        reg = Registry.CurrentUser.OpenSubKey(@"Software\LG Chem\FCS_UI", true);

        //        //string key = @"HKEY_CURRENT_USER\Software\LGChem\FORMATION";

        //        reg.SetValue("XAXIS", sXposi); //X 좌표 저장)
        //        reg.SetValue("YAXIS", sYposi); //X 좌표 저장)
        //        reg.SetValue("BARCODEXY", sBarcodeXY); //X 바코드 위치)
        //        reg.SetValue("BARCODEOPTION", sBarcodeOption); //바코드 옵션)
        //        reg.SetValue("BARTEXTXY", sBarTextXY); //글씨 위치)
        //        reg.SetValue("BARTEXTSIZE", sBarTextSize); //글씨 크기)

        //    }
        //    catch (Exception e)
        //    {
        //        Util.MessageInfo("FM_ME_0064");  //Registry 정보를 가져오는 중 에러가 발생하였습니다.
        //    }
        //}

        #endregion

    }
}
