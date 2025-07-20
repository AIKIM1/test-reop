/*************************************************************************************
 Created Date : 2019.11.07
      Creator : 이제섭
   Decription : 전지 5MEGA-GMES 구축 - 포장 Pallet 구성(Box) - 라벨 재발행 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.07  이제섭 : 최초생성
  
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

    public partial class BOX001_026_REPRINT : C1Window, IWorkArea
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

        private string _workUserID = string.Empty;
        private string _BoxID = string.Empty;

        public BOX001_026_REPRINT()
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

            _workUserID = Util.NVC(tmps[0]);
            _BoxID = Util.NVC(tmps[1]);

            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            txtboxid.Text = _BoxID;

            txtnetweight.IsReadOnly = true;
            txtgrossweight.IsReadOnly = true;
            txtratedpower.IsReadOnly = true;

        }


        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!Valid())
                return;
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("N_WEIGHT", typeof(string)); // Net Weight - CNB 전용
                inDataTable.Columns.Add("G_WEIGHT", typeof(string)); // Gross Weight - CNB 전용
                inDataTable.Columns.Add("R_POWER", typeof(string)); // Rated Power - CNB 전용

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _BoxID;
                newRow["LABEL_TYPE_CODE"] = "INBOX";
                newRow["USERID"] = _workUserID;
                newRow["N_WEIGHT"] = String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) ? null : txtnetweight.Text.ToString().Trim();
                newRow["G_WEIGHT"] = String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()) ? null : txtgrossweight.Text.ToString().Trim();
                newRow["R_POWER"] = String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()) ? null : txtratedpower.Text.ToString().Trim();

                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_LABEL_CP", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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

                        ////   Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        //this.DialogResult = MessageBoxResult.OK;
                        //this.Close();
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
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            //this.Close();
        }
        #endregion

        #region Mehod

        private void PrintProcess(string BoxID)
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LABEL_TYPE_CODE", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("N_WEIGHT", typeof(string)); // Net Weight - CNB 전용
                inDataTable.Columns.Add("G_WEIGHT", typeof(string)); // Gross Weight - CNB 전용
                inDataTable.Columns.Add("R_POWER", typeof(string)); // Rated Power - CNB 전용

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = BoxID;
                newRow["LABEL_TYPE_CODE"] = "INBOX";
                newRow["USERID"] = _workUserID;
                newRow["N_WEIGHT"] = String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) ? null : txtnetweight.Text.ToString().Trim();
                newRow["G_WEIGHT"] = String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()) ? null : txtgrossweight.Text.ToString().Trim();
                newRow["R_POWER"] = String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()) ? null : txtratedpower.Text.ToString().Trim();

                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_LABEL_CP", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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

                        ////   Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                        //this.DialogResult = MessageBoxResult.OK;
                        //this.Close();
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
            }
        }

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

        private bool Valid()
        {
            if (chkDetail.IsChecked == true)
            {
                if (String.IsNullOrWhiteSpace(txtnetweight.Text.ToString().Trim()) || String.IsNullOrWhiteSpace(txtgrossweight.Text.ToString().Trim()))
                {
                    // 포장 중량을 입력하세요.
                    Util.MessageInfo("SFU4390");
                    return false;
                }

                if (String.IsNullOrWhiteSpace(txtratedpower.Text.ToString().Trim()))
                {
                    // Rated Power를입력하세요.
                    Util.MessageInfo("SFU8121");
                    return false;
                }
            }

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
            {
                return false;
            }

            return true;
        }




        #endregion


        private void chkDetail_Checked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = false;
            txtgrossweight.IsReadOnly = false;
            txtratedpower.IsReadOnly = false;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;
        }

        private void chkDetail_Unchecked(object sender, RoutedEventArgs e)
        {
            txtnetweight.IsReadOnly = true;
            txtgrossweight.IsReadOnly = true;
            txtratedpower.IsReadOnly = true;

            txtnetweight.Text = string.Empty;
            txtgrossweight.Text = string.Empty;
            txtratedpower.Text = string.Empty;
        }
    }
}
