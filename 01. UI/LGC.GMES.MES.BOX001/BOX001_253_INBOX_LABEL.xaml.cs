/*************************************************************************************
 Created Date : 2022.08.22
      Creator : 김태균
   Decription : INBOX 라벨 발행 팝업 - NFF용
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.22  김태균 : 최초생성
  
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

    public partial class BOX001_253_INBOX_LABEL : C1Window, IWorkArea
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
        
        string _sPGM_ID = "BOX001_253_INBOX_LABEL";

        public BOX001_253_INBOX_LABEL()
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
           
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { _EQSGID, _PROCID }, sCase: "EQUIPMENT_BY_EQSGID_PROCID");     // DA_BAS_SEL_EQUIPMENT_BY_EQSGID_PROCID_CBO       
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] { LoginInfo.LANGID, LoginInfo.CFG_SHOP_ID }, sCase: "cboShopProduct");

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
                       
            _USERID = Util.NVC(tmps[2]);
            
            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);
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
            try
            {
                if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                {
                    return;
                }

                string sBizRule = "BR_PRD_GET_INBOX_LABEL_NFF";
                string sPrintZPL = string.Empty;
                string sBoxID = string.Empty;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("PRINTQTY");
                inDataTable.Columns.Add("LABELTYPE");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
                inDataTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용
                inDataTable.Columns.Add("EQSGID");

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
                newRow["LABELTYPE"] = "NFF_INBOX";
                newRow["USERID"] = _USERID;
                newRow["PRODID"] = cboProduct.SelectedValue;
                newRow["PGM_ID"] = _sPGM_ID;
                newRow["BZRULE_ID"] = sBizRule;
                newRow["EQSGID"] = _EQSGID;
                inDataTable.Rows.Add(newRow);
                
                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);           

                //new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_LABEL_NJ_CIRCULAR", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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
                            sBoxID = dr["BOXID"].ToString();
                            zplCode = dr["ZPLCODE"].ToString();
                            zplCode = RePlaceZPL(sBoxID, zplCode);
                            
                            sPrintZPL += zplCode;
                        }

                        PrintLabel(sPrintZPL, drPrtInfo);
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
            }
        }

        private string RePlaceZPL(string sBOXID, string sZPL)
        {
            string rtnZPL = string.Empty;

            rtnZPL = sZPL.Replace(sBOXID, sBOXID + ":ZA");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZB");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZC");
            rtnZPL = rtnZPL + sZPL.Replace(sBOXID, sBOXID + ":ZD");

            return rtnZPL;
        }

        private bool Validation()
        {

            if (cboProduct.SelectedValue == null || cboProduct.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //제품을 선택하세요.
                Util.MessageValidation("SFU1895");
                return false;
            }

            if (cboEquipment.SelectedValue == null || cboEquipment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1673 설비를 선택하세요.
                Util.MessageValidation("SFU1673");
                return false;
            }

            return true;
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

    }
}
