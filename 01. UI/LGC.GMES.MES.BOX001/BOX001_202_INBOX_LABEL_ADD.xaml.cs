/*************************************************************************************
 Created Date : 2018.06.19
      Creator : 정문교
   Decription : 전지 5MEGA-GMES 구축 - 1차포장구성 화면 - 추가라벨발행 팝업
                Inbox라벨 발행 팝업(BOX001_202_INBOX_LABEL) 복사 생성
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.19  정문교 : 최초생성
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_202_INBOX_LABEL_ADD : C1Window, IWorkArea
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
        private string _addLableType = string.Empty;
        DataRow[] _drLot = null;

        string _sPGM_ID = "BOX001_202_INBOX_LABEL_ADD";

        public BOX001_202_INBOX_LABEL_ADD()
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

        ///// <summary>
        ///// 화면내 버튼 권한 처리
        ///// </summary>
        //private void ApplyPermissions()
        //{
        //    List<Button> listAuth = new List<Button>();
        //    //listAuth.Add(btnInReplace);
        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //}

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _workUserID = Util.NVC(tmps[0]);
            _addLableType = Util.NVC(tmps[1]);
            _drLot = tmps[2] as DataRow[];

            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            if (!_addLableType.Equals("MANUAL"))
            {
                grdQty.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { LoginInfo.LANGID, _addLableType.Equals("MANUAL") ? "MOBILE_INBOX_ADD_LABEL_OC" : "MOBILE_INBOX_ADD_LABEL_NJ" };
            _combo.SetCombo(cboCustomer, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "COMMCODES");
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitCombo();
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationPrint()) return;

            if (_addLableType.Equals("MANUAL"))
            {
                for (int cnt = 0; cnt < txtPrintQty.Value; cnt++)
                {
                    PrintProcessManual(Util.NVC(_drLot[0]["PKG_LOTID"]));
                }
            }
            else
            {
                foreach (DataRow dr in _drLot)
                {
                    PrintProcess(Util.NVC(dr["BOXID"]));
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Mehod

        private void PrintProcess(string BoxID)
        {
            try
            {
                string sBizRule = "BR_PRD_GET_INBOX_ADD_LABEL_NJ";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("SHIPTO_ID");
                inDataTable.Columns.Add("SOC_VALUE");
                inDataTable.Columns.Add("BOXID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["SHIPTO_ID"] = Util.NVC(cboCustomer.SelectedValue);
                newRow["SOC_VALUE"] = Util.NVC(txtSOC.Text);
                newRow["BOXID"] = BoxID;
                newRow["USERID"] = _workUserID;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;

                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_ADD_LABEL_NJ", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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

        private void PrintProcessManual(string LotID)
        {
            try
            {
                string sBizRule = "BR_PRD_GET_INBOX_ADD_LABEL";

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("CUSTOMERID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SOC");
                inDataTable.Columns.Add("QTY");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");

                DataRow newRow = inDataTable.NewRow();
                newRow["LOTID"] = LotID;
                newRow["CUSTOMERID"] = Util.NVC(cboCustomer.SelectedValue);
                newRow["USERID"] = _workUserID;
                newRow["SOC"] = Util.NVC(txtSOC.Text);
                newRow["QTY"] = txtCellQty.Value;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_ADD_LABEL", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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

        private bool ValidationPrint()
        {
            if (cboCustomer.SelectedIndex < 0 || cboCustomer.SelectedValue.GetString().Equals("SELECT"))
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("고객사"));
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtSOC.Text))
            {
                // SOC 정보를 입력하세요
                Util.MessageValidation("SFU4203");
                return false;
            }

            if(_addLableType.Equals("MANUAL"))
            {
                if (txtCellQty.Value == 0)
                {
                    // Cell 정보 수량을 입력하세요
                    Util.MessageValidation("SFU4484");
                    return false;
                }

                if (txtPrintQty.Value == 0)
                {
                    // 발행 수량을 확인하세요.
                    Util.MessageValidation("SFU4085");
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

    }
}
