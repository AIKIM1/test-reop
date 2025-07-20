/*************************************************************************************
 Created Date : 2018.06.19
      Creator : 정문교
   Decription : 전지 5MEGA-GMES 구축 (오창 소형 조립 -> 물류포장 -> 수동인쇄(2차))
                Inbox라벨 발행 팝업(BOX001_202_INBOX_LABEL_ADD) Copy 후 생성 
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.29  이제섭 : 최초생성
  
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

    public partial class BOX001_038_INBOX_ADD_LABEL : C1Window, IWorkArea
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
        private string _Prod = string.Empty;
        private string _Soc = string.Empty;
        DataRow[] _drLot = null;
        

        public BOX001_038_INBOX_ADD_LABEL()
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
            _Prod = Util.NVC(tmps[2]);
            txtSOC.Text = Util.NVC(tmps[3]) as string;
            _drLot = tmps[4] as DataRow[];
            






            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { LoginInfo.LANGID, "MOBILE_INBOX_ADD_LABEL_OC" };
            _combo.SetCombo(cboCustomer, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "COMMCODES");

            //setSocCombo(cboSOC, CommonCombo.ComboStatus.SELECT, _Prod);

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
       }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Mehod


        private void PrintProcessManual(string LotID )
        {
            

            try
            {
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LOTID");
                inDataTable.Columns.Add("CUSTOMERID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("SOC");
                inDataTable.Columns.Add("QTY");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("PRODID");
                inDataTable.Columns.Add("SHOPID");

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
                newRow["SOC"] = txtSOC.Text;
                newRow["QTY"] = txtCellQty.Value;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PRODID"] = _Prod;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                inDataTable.Rows.Add(newRow);

                newRow = inPrintTable.NewRow();
                newRow["PRMK"] = sPrt; // "ZEBRA"; Print type
                newRow["RESO"] = sRes; // "203"; DPI
                newRow["PRCN"] = sCopy; // "1"; Print Count
                newRow["MARH"] = sXpos; // "0"; Horizone pos
                newRow["MARV"] = sYpos; // "0"; Vertical pos
                newRow["DARK"] = sDark; // darkness
                inPrintTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INBOX_ADD_LABEL", "INDATA,INPRINT", "OUTDATA", (bizResult, bizException) =>
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


            if (txtCellQty.Value < 1 )
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

           

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
            {
                return false;
            }

            return true;
        }




        #endregion

    }
}
