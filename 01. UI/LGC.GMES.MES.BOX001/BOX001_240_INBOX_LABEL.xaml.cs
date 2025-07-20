/*************************************************************************************
 Created Date : 2019.12.04
      Creator : 이제섭
   Decription : 전지 5MEGA-GMES 구축 - 자동 포장 구성 (원/각형) 화면 - INBOX 라벨 발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.04  이제섭 : 최초생성
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_240_INBOX_LABEL : C1Window, IWorkArea
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

        private string _EQSGID = string.Empty;
        private string _PROCID = Process.CELL_BOXING;
        private string _EQPTID = string.Empty;
        private string _USERID = string.Empty;
        private string _PKG_LOTID = string.Empty;
        private string _PRDT_GRD_CODE = string.Empty;
        private string _PRODID = string.Empty;
        
        public BOX001_240_INBOX_LABEL()
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

            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] {"TESLA_INBOX_EQPT_CODE", LoginInfo.CFG_AREA_ID }, sCase: "COMMCODEATTR");
        } 

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _USERID = Util.NVC(tmps[0]);

            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            // Load 시 Defalut
            chkReprint.IsChecked = false;

        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _EQSGID = Util.NVC(tmps[0]);

            InitCombo();
            InitControl();
        }


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (chkReprint.IsChecked == true)
            {
                RePrint_Box();
            }
            else
            {
                Print_Box();
            }
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #region Mehod
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
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

                //System.Threading.Thread.Sleep(200);
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

        private void Print_Box()
        {
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

                if (cboEquipment.SelectedValue.ToString().Equals(string.Empty) || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 설비를 선택하세요.
                    Util.MessageInfo("SFU1153");
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("PRINTQTY");
                inDataTable.Columns.Add("LABELTYPE");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("PRDT_GRD_CODE");
                inDataTable.Columns.Add("PKG_LOTID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["PRINTQTY"] = txtInputQty.Value;
                newRow["LABELTYPE"] = "TESLA_DISP_INBOX"; // TESLA 1회용 INBOX
                newRow["USERID"] = _USERID;
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_LABEL_FOR_TESLA_NJ", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        string zplCode = string.Empty;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        foreach (DataRow dr in bizResult.Tables["OUTDATA"].Rows)
                        {
                            zplCode += dr["ZPLCODE"].ToString();
                        }
                        PrintLabel(zplCode, drPrtInfo);
                        //   Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                txtBoxID.Text = string.Empty;
            }
        }

        private void chkReprint_Checked(object sender, RoutedEventArgs e)
        {
            txtBoxID.IsReadOnly = false;
        }

        private void chkReprint_Unchecked(object sender, RoutedEventArgs e)
        {
            txtBoxID.IsReadOnly = true;
            txtBoxID.Text = string.Empty;
        }

        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RePrint_Box();
            }
        }
        private void RePrint_Box()
        {
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

                if (cboEquipment.SelectedValue.ToString().Equals(string.Empty) || cboEquipment.SelectedValue.ToString().Equals("SELECT"))
                {
                    // 설비를 선택하세요.
                    Util.MessageInfo("SFU1153");
                    return;
                }
                if (String.IsNullOrWhiteSpace(txtBoxID.Text.ToString().Trim()))
                {
                    //BOXID 먼저 입력하세요
                    Util.MessageInfo("SFU3387");
                    return;
                }

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("BOXID");

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["EQPTID"] = Util.NVC(cboEquipment.SelectedValue);
                newRow["USERID"] = _USERID;
                newRow["BOXID"] = txtBoxID.Text.ToString().Trim();
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_LABEL_FOR_TESLA_REPRT_NJ", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        string zplCode = string.Empty;
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        foreach (DataRow dr in bizResult.Tables["OUTDATA"].Rows)
                        {
                            zplCode += dr["ZPLCODE"].ToString();
                        }
                        PrintLabel(zplCode, drPrtInfo);
                        //   Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                txtBoxID.Text = string.Empty;
            }
        }
                
    }
}
