/*************************************************************************************
 Created Date : 2018.06.22
      Creator : 
   Decription : 자동 포장 구성 (파우치형)
--------------------------------------------------------------------------------------
 [Change History]
  2017.02.22  DEVELOPER : Initial Created.
  2023.01.31 김린겸 C20221221-000550 Added GMES DB data check for packaged cell logins
  2023.05.24  성민식 : E20230424-000075 실제 OUTBOX 수량 BOX.SHIPTO_NOTE 저장 기능 추가
  2023.06.07 이병윤 :  E20230424-000073 추가기능에 BOX Realease버튼 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_231 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        private static string CREATED = "CREATED,";
        private static string PACKING = "PACKING,";
        private static string PACKED = "PACKED,";
        private static string SHIPPING = "SHIPPING,";
        private string _searchStat = string.Empty;
       // private string _rcvStat = string.Empty;
        private bool bInit = true;

        private string _processName = string.Empty;

        bool _AommGrdChkFlag = false;

        // 프린트 설정용
        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;

        string _sPGM_ID = "BOX001_231";

        DataRow _drPrtInfo = null;
     
        private delegate void SetWeightCallback(string text);

        Util _util = new Util();
       
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre2 = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        CheckBox chkAll2 = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        public BOX001_231()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
            bInit = false;

            if (LoginInfo.CFG_SHOP_ID == "G182" || LoginInfo.CFG_SHOP_ID == "G184")
            {
                txtRealQty.Visibility = Visibility.Visible;
                txtBlckRealQty.Visibility = Visibility.Visible;
            }

            // N5,N6동 파우치만
            if (LoginInfo.SYSID == "GMES-S-N5" || LoginInfo.SYSID == "GMES-S-N6")
            {
                btnBoxRelease.Visibility = Visibility.Visible;
            }

            this.Loaded -= UserControl_Loaded;
        }

        private void ApplyPermissions()
        {
            btnCancelShip.IsEnabled = LoginInfo.LOGGEDBYSSO == true ? true : false;
        }

        private void InitCombo()
        {
             CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboLine, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "MCP", Process.CELL_BOXING, LoginInfo.CFG_AREA_ID }, sCase: "LINEBYSHOP");

            _combo.SetCombo(cboProcType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "B" }, sCase: "PROCBYPCSGID");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");
            _combo.SetCombo(cboExpDomType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "EXP_DOM_TYPE_CODE" }, sCase: "COMMCODE_WITHOUT_CODE");

            SetAommGradeCombo();
        }

        public void SetAommGradeCombo()
        {
            try
            {
                cboAommType.ItemsSource = null;
                cboAommType.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "AOMM_GRADE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                cboAommType.DisplayMemberPath = "CBO_NAME";
                cboAommType.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);


                cboAommType.ItemsSource = dtResult.Copy().AsDataView();
                cboAommType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetAommGrdVisibility(string sProdID)
        {
            try
            {
                if (string.IsNullOrEmpty(sProdID))
                {
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "GRADE_CHK_PROD";
                dr["CMCODE"] = sProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _AommGrdChkFlag = true;
                    tbAommType.Visibility = Visibility.Visible;
                    cboAommType.Visibility = Visibility.Visible;
                }
                else
                {
                    _AommGrdChkFlag = false;
                    tbAommType.Visibility = Visibility.Collapsed;
                    cboAommType.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            try
            {
                /* 공용 작업조 컨트롤 초기화 */
                ucBoxShift = grdShift.Children[0] as UCBoxShift;
                txtWorker_Main = ucBoxShift.TextWorker;
                txtShift_Main = ucBoxShift.TextShift;
                ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정
                ucBoxShift.FrameOperation = this.FrameOperation; 

                // 라벨 발행 가능 여부 체크
                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                {
                    return;
                }

                if (_processName == string.Empty)
                    SelectProcessName();

                //chkAuto.IsChecked = true;

                //DataRow[] drList = LoginInfo.CFG_SERIAL_PRINT.Select(CustomConfig.CONFIGTABLE_SERIALPRINTER_PRINTERTYPE + "= 'S'");
                //if (drList.Length < 1)
                //{
                //    //SFU4328		설정된 전자저울이 없습니다. Setting창에서 Port를 지정해주세요.	2017.01.06 팝업 대신에 체크박스에 체크
                //   // Util.MessageValidation("SFU4328");
                //    chkAuto.IsChecked = true;
                //    chkAuto.IsEnabled = true;
                //    return;
                //}

                //if (!this.FrameOperation.CheckScannerState())
                //{
                //    //SFU4327		연결된 전자저울이 없습니다.
                //    Util.MessageValidation("SFU4327");
                //    chkAuto.IsChecked = false;
                //    chkAuto.IsEnabled = false;
                //    return;                    
                //}

                //this.FrameOperation.ReceiveScannerData += FrameOperation_ReceiveScannerData;               
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private void FrameOperation_ReceiveScannerData(string portName, string value)
        //{
        //    // <STX><POL>xxxxx.xx<LB/KG> <GR/NT><CR><LF>
        //    // "\u0002 12345.67KG LT\r\u0003"
        //    //this.Dispatcher.Invoke((ThreadStart)(() => { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        //    //Thread.Sleep(200); //약간의 딜레이를 주어야 데이터가 끊기지 않고 들어옴.

        //    string strData = value;

        //    if (strData.Length != 17)
        //        return;

        //    string strPlusMinus = strData.Substring(2, 1).Trim();
        //    string strWeight = strData.Substring(3, 7).Trim();
        //    double dWeight = Math.Round(Convert.ToDouble(strPlusMinus + strWeight), 2);
        //    string stUnit = strData.Substring(10, 2).Trim().ToUpper();

        //    // 무게 셋팅 후 자동 발행
        //    this.SetWeight(dWeight.ToString());
        //}
        
        //private void SetWeight(string data)
        //{
        //    if (!Dispatcher.CheckAccess())
        //    {
        //        SetWeightCallback d = new SetWeightCallback(SetWeight);
        //        this.Dispatcher.BeginInvoke(d, new object[] { data });   
        //    }
        //    else
        //    {
        //        this.txtBoxWeight.Text = data;
        //        PrintOutBox();
        //    }
        //}

        /// <summary>
        ///  1. InBox 스캔시 OutBox만들고 ZPL Return.
        ///  2. 재발행시 비즈에서 OutBox만들고 안만들고 ZPL만 Return.
        /// </summary>
        private void PrintOutBox()
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtBoxId.Text = string.Empty;
                        //txtBoxWeight.Text = string.Empty;
                        txtBoxId.Focus();
                    }
                });
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtBoxId.Text = string.Empty;
                        //txtBoxWeight.Text = string.Empty;
                        txtBoxId.Focus();
                    }
                });
                return;
            }

            //if (string.IsNullOrWhiteSpace(txtBoxWeight.Text) || Util.NVC_Decimal(txtBoxWeight.Text) <= 0)
            //{
            //    txtBoxId.Text = string.Empty;
            //    txtBoxWeight.Text = string.Empty;
            //    txtBoxId.Focus();
            //    return;
            //}

            //if (txtSOC.Value <= 0)
            //{
            //    //SFU4203	SOC 정보를 입력하세요. 
            //    Util.MessageValidation("SFU4203");
            //    return;
            //}

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            string sBizRule = "BR_PRD_REG_OUTBOX_FOR_2D_NJ";

            DataSet indataSet = new DataSet();
            DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
            inPalletTable.Columns.Add("LANGID");
            inPalletTable.Columns.Add("BOXID");
            inPalletTable.Columns.Add("USERID");
            inPalletTable.Columns.Add("SHFT_ID");
            inPalletTable.Columns.Add("REPRINT_YN");
            inPalletTable.Columns.Add("SOC");
            inPalletTable.Columns.Add("PGM_ID");    //라벨 이력 저장용
            inPalletTable.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

            DataRow newRow = inPalletTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["BOXID"] = sPalletId;
            newRow["USERID"] = txtWorker_Main.Tag;
            newRow["SHFT_ID"] = txtShift_Main.Tag;
            newRow["REPRINT_YN"] = "N";
            newRow["SOC"] = txtSOC.Value.ToString();
            newRow["PGM_ID"] = _sPGM_ID;
            newRow["BZRULE_ID"] = sBizRule;
            inPalletTable.Rows.Add(newRow);

            DataTable inBoxTable = indataSet.Tables.Add("INBOX");
            inBoxTable.Columns.Add("BOXID");
            inBoxTable.Columns.Add("WEIGHT");

            newRow = inBoxTable.NewRow();
            newRow["BOXID"] = txtBoxId.Text.Trim();
            inBoxTable.Rows.Add(newRow);

            DataTable inPrintTable = indataSet.Tables.Add("INPRINT");
            inPrintTable.Columns.Add("PRMK");
            inPrintTable.Columns.Add("RESO");
            inPrintTable.Columns.Add("PRCN");
            inPrintTable.Columns.Add("MARH");
            inPrintTable.Columns.Add("MARV");
            inPrintTable.Columns.Add("DARK");

            newRow = inPrintTable.NewRow();
            newRow["PRMK"] = _sPrt;
            newRow["RESO"] = _sRes;
            newRow["PRCN"] = _sCopy;
            newRow["MARH"] = _sXpos;
            newRow["MARV"] = _sYpos;
            newRow["DARK"] = _sDark;
            inPrintTable.Rows.Add(newRow);
            txtBoxId.Text = string.Empty;

            //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_FOR_2D_NJ", "INPALLET,INBOX,INPRINT", "OUTDATA", (bizResult, bizException) =>
            new ClientProxy().ExecuteService_Multi(sBizRule, "INPALLET,INBOX,INPRINT", "OUTDATA", (bizResult, bizException) =>
            {
                try

                {
                    if (bizException != null)
                    {
                        Util.MessageConfirmByWarning(Util.NVC(bizException.Data["DATA"]), msgResult =>
                        {
                            if (msgResult == MessageBoxResult.OK)
                            {
                                // txtBoxWeight.Text = string.Empty;
                                txtBoxId.Focus();
                                txtBoxId.Text = string.Empty;
                            }
                        }, bizException.Data["PARA"].ToString().Split(':'));
                        return;
                    }

                    if (bizResult.Tables.Contains("OUTDATA")
                    && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        string sZplCode = (string)bizResult.Tables["OUTDATA"].Rows[0]["ZPLCODE"];
                        string sLblCode = (string)bizResult.Tables["OUTDATA"].Rows[0]["LABEL_CODE"];

                        if (sLblCode.Equals("LBL0098"))
                        {
                            if (_sRes.Equals("300"))
                            {
                                #region Desay 신형 : LBL0098, 300dpi
                                sZplCode = sZplCode.Replace("^PQ", "^FO77,169^GFA,00264,00264,00008,"
    + "00,000038180180,00301C1FFFC0,3FFB98180180,06C398180180,06C318180180,06C3181801"
    + "80,06C318180180,06C318180180,06DB181FFF80,7FFF1818,06C31818,0CC318000030,0CC318"
    + "C00038,0CC318FFFFF8,0CC01806,18C01806,18C01806,30C1F80C0180,30F0700FFFC0,603C60"
    + "0001C0,0030C0000180,0030C0000180,1FFFE0000180,003000000180,003000000180,003000"
    + "000380,0030180003,00301800E7,7FFFFC003E,000000000C,00,00,"
    + "^FO59,225^GFA,00396,00396,00012,"
    + "00,0618000E00C03030,071C000E00E03818000C03,3618000D80C0301C030FFF803E1800CCF0C0"
    + "300C01F803,361800CCD8C0630C606003,36300C6D8CC061FFE060E3,37BFFC6D86C06D806060E3"
    + ",3FF66C7D86C0CF8060E0C3,6666CC3F00C0CD8060C0C3,6666CC6E00C0CD8060C0C6,66CCCC0D"
    + "80C1F98060C0C6,66CCCC0DF0C1D9FFE0C186,678CD8FFF8C1B18060CD86,0619980C0CC0318000"
    + "FF86C007D9980C0EC0618001CDFFE00731983F06C061C061CF00E00E31983F00D8C1FFF1CC00C0"
    + "3E63186D80F8FFDB63CC00C07E63186D80F0F1DB63CC00C066C6186D87C0C3DB63CC06C0678618"
    + "6CFCC003DB63CFFFC0060618CDE0C007FFE0CC00C0060C18CC00C00FDB60CC00C0061818CC00C0"
    + "3BDB60CC0180061818CC00C0F3DB60CC01800630180C00C1E6DB60FC01800660380C00C186DB60"
    + "CC01800603300C00C186DBE0D81B800607F00C00C00CDBE0C00F,0600600C00C000C0C00006,00"
    + ",00,"
    + "^FO83,285^GFA,00528,00528,00016,"
    + "00,0661800060000180000001C0C00C,0739C000700000C0000180E0FFFE,06318000303000E0C1"
    + "FFDCC0C00C,0C3180303070006060361CC0C00C,0C31803FFFF9FFFFF03618C0C00C,0C31803000"
    + "000C0C003618C0C00C,0C31B0300000060E003618C0C00C,1831B8300000060C003618C0C00C,1F"
    + "FFF83060C006180036D8C0FFFC,183180306070C318C3FFF8C0C0,3831803C6060C630E03618C0"
    + "C00000C03831803C3060FFFFE06618C0000181E07831803C30C0C330C06618C60001C1E0783180"
    + "3630C0C730C06618C7FFFFC0C07831803618C0C630C06600C030,7831803318C0C63FC0C600C030"
    + ",1831983318C0CC1FC0C600C030,1E319C3318C0D800C1860FC0600C,1BFFFC339980F60CC18783"
    + "807FFE,180000319980C7FCC301E300000E,180000319980C61CC0018600000C,183300318180C6"
    + "1CC0018600000C,183980618300C61CC0FFFF00000C,1870C0618300C61CC0018000000C,186060"
    + "600360C61CC0018000000C00C018E070600670C7FCC0018000001C01E018C0307FFFF8C600C001"
    + "80C0001800C0198038C00000C007C00180C00738,1F0018C00000C001C3FFFFE001F0,1E0000C0"
    + "0000C001C00000000060,00,00,"
    + "^FO25,342^GFA,00396,00396,00012,"
    + "00,00300000C000398C0000C0,003C000060003DC70000E0,03380000606031860001C0,03B800"
    + "0070703186000180,0738007FFFF03186001980,063800060700318603FD80C006380003070037"
    + "860003FFE0063860030601FF86000300C00C38700306603386000301800FFFF0630C7031B6C006"
    + "71800C38007FFFF831BEE00673,18380060000031FFF00C73,1838006000003186000C76,303800"
    + "6000003186001876,3038006000003786000070,6038C06000003D86000070,6038E06000007186"
    + "000C70,1FFFF0600001F186071C70,003800600001B18607F678,003800600001B186000078,00"
    + "380060000031860000D8,00380060000031860000CC,00380060000031863001CC,003800600000"
    + "3186300186,003800600000319E300386,00380060000031B6300307,003818C0000031E6300603"
    + "8060383CC00001B1C7700C01C07FFFFCC00000F187F01800E0000000C000007180003000C000,00"
    + "^FO101,401^GFA,00264,00264,00008,"
    + "0C,070600180180,6667800FFFC0,3676000C01C0,1E66000C0180,1EC6000C0180,1F86000FFF"
    + "80,063C1C0C0180,7FFFFC0C0180,1E0C300FFF80,1F8C30000030,1ED830C00070,1E7830FFFF"
    + "F8,367C30,667C300C0180,6006300FFFC0,0C06600C7180,0EE6600C7180,0CF6600FFF80,0FE6"
    + "600C7180,7EC3600C7180,78C3C00C7180,7983C00FFF80,3181C01870,1F81C00070C0,0381C0"
    + "7FFFE0,03C3606070,06E370007060,1C6638007070,700C1CFFFFF8,0038,00,00,"
    + "^FO32,461^GFA,00264,00264,00008,"
    + "00,00C0,00E0007030,0181801818,01FFC01C18,0303800C0C,0783000C0C60,06C700000C70,0C"
    + "C60003FFF8,186C00C00C,303800E00C,003C00700C,006E00300C,01C380380C,0381F8300C,0E"
    + "007C000C,3C0060000CC0,6FFFE00D8CE0,0C30600DFFF0,0C3060180C,0C3060180C,0C306018"
    + "0C,0C3060300C,0FFFE0300C,0C3060300C,0C3060600C,0C3060E00C,0C3060E00C30,0FFFE0CE"
    + "0C78,0C0060C7FFC0,0C0060,00,00,"
    + "^FO86,579^GFA,00264,00264,00008,"
    + "00,0660601C18,0730301E1C,061FF81818,0618301818C0,061830183FE0,0618301830E0,06DB"
    + "301B30C0,7FDBB0FFF8C0,061B301879C0,061B3018D980,061B30198D80,061B303F07,06DB30"
    + "3E07,66DB303B07,7FFB303B0D80,061BB07B19E0,0C1BF07830F0,0E3BF0786038,0F3BF0D9E0"
    + "60,0D87C0DB7FE0,0D87C0D86060,0DC7C0D86060,18C7CC186060,18CDCC186060,300DCC1860"
    + "60,3019CC186060,6039CC186060,6030FC186060,00E0FC187FF0,0180001860,00,00,"
    + "^FO115,638^GFA,00264,00264,00008,"
    + "00,0003800003,000780001F80,000E0003FC,003C003E60,01E0000060,1FC000006030,30C000"
    + "FFFFF8,00C1800060,00C0C00060,00C3E03060C0,00FE001FFFE0,1FC0001860C0,38C0001860"
    + "C0,00C0001860C0,00C0301FFFC0,00C0381860C0,00C3F01860C0,00FE001860C0,3FC0001860"
    + "C0,60C0001FFFC0,00C0001860C0,00C0000060,00C0180060C0,00C0186060E0,00C0183FFFE0"
    + ",00C0180060,00C018006030,00E01C006030,007FFCFFFFF8,00,00,00,^PQ");
                                #endregion
                            }
                            else
                            {
                                #region Desay 신형 : LBL0098, 203dpi
                                sZplCode = sZplCode.Replace("^PQ", "^FO52,114^GFA,00184,00184,00008,"
                        + "0000180C,01870FFE,7FFE0C0C,1B360C0C,1B360C0C,1B360C0C,1BF60FFE,7FF60C,1B360003"
                        + ",1B36700380,1B361FFE,330606,331E06,638607FE,60C00C06,018C0006,3FFC000C,0180000C"
                        + ",0180000C,0183018C,7FFF00F8,00000060,00,"
                        + "^FO40,153^GFA,00276,00276,00012,"
                        + "30000C,1CE00607061800C180,78C06D86060C0FFFC0,78C03DE60D8D830180,7D833DBE0CFFC6"
                        + "1D80,7FFF9F1E0DC1861980,7F6F1F061BC1861980,DB7B0C061BC1863180,DE7B0DE61EFF8631"
                        + "80,DEDB7FF61EC186F3,1EDB0C1E0CC007F360,1D9B1F1E0CE0CEDFE0,39B31F079BFFCEC060,FB"
                        + "333D87DEFEDEC060,DE633DBE18FEDEC3C0,18633DE601FFC6FEC0,18C36C0603FEC6C0C0,18C6"
                        + "6C060FFEC6C0C0,19860C061DFEC7C0C0,1B660C0619FFC6C6C0,183E0C06037FC60380,18180C"
                        + "06006180,00,"
                        + "^FO56,193^GFA,00276,00276,00012,"
                        + "1800018000C00000030180,1DDC00C000600030E1FFC0,1998306300618FFFC18180,19981FFF9F"
                        + "FFC366C18180,19981800030C0366C18180,319E1800018E0366C18180,3FFF18C60198037EC1FF"
                        + "C0,319818C399998FFEC180,31981C630FFFC366C00060E071981E6619998366CE0070E0F1981E"
                        + "6619998366C3FFC0,31981B66199F8660C0C0,319B1B661B018663C0C0,3FFF1B361F8D8C70C0FF"
                        + "C0,30001B3C19FD8C180180C0,31B0198C198D80318000C0,31D8318C198D87FF800180,318C30"
                        + "18198D8030000180E03306301B99F98030000180E036063FFF980D8030603180C03C0630001807"
                        + "8FFFE01F,3003000018030000000C,00,"
                        + "^FO17,232^GFA,00276,00276,00012,"
                        + "0000000000000030,00C000C007E60018,0F8000C306C60030,0D803FFF86C601B0,0D80061C06"
                        + "C60FF060,198C031837C6007FE0,198E031B1FC60060C0,1FF8333386DF806EC0,31801FFE067F"
                        + "C0CD80,3180180006C6018F,6180180007C6000C,C18C300007C6000C,318C30001EC618DC,1FFE"
                        + "30003EC61F9E,0180300036C6001E,0180300006C6001E,0180300006C6C033,0180300006DEC0"
                        + "33,0180300006DEC06180,0183300006F6E0C0C0,FFFF30001EC7C18070,000030000C0003,00,"
                        + "^FO68,272^GFA,00184,00184,00008,"
                        + "0C,6F300C06,6FE00FFE,3F600C06,3F630FFE,7FFF0C06,3CC78FFE,3EC6000180,3FC63FFF80"
                        + ",6FE6,D86607FE,186C0CC6,1F6C0CC6,1FEC0FFE,F63C0CC6,F63C07FE,361800C0,1C1830C7,0F"
                        + "3C1FFC,1B6600C3,70C3BFFF80,0180,00,"
                        + "^FO22,312^GFA,00184,00184,00008,"
                        + "00,03001860,06180C60,0FF80C30,0E300C33,1B6003FF80,31E03030,60C03030,03E01830,06"
                        + "381830,1C0F0030,780C0033,1FFC0DFF,198C0C30,198C0C30,198C1830,1FFC1830,198C1830"
                        + ",198C3033,1FFC303380,180C3FFE,00,00,"
                        + "^FO59,392^GFA,00184,00184,00008,"
                        + "00,1D860E30,18FE0C38,19860C63,19860E7F,7FB66FE6,19B63FE6,19B60DBC,19B60F1C,1FB6"
                        + "1E18,7FB61F1C,19BE3F37,19BE3CE3C0,1DBE3DC3,1E786FFF,36786CC3,367B0CC3,30DB0CC3"
                        + ",60DB0CC3,619B0CC3,030F8CFF,0E000C,00,"
                        + "^FO78,432^GFA,00184,00184,00008,"
                        + "0030,0038003C,00E007E6,03801CC0,3F0000C380,03003FFF80,031C00C0,03F818C7,1F801F"
                        + "FE,730018C6,030618C6,03071FFE,03FC18C6,7F8018C6,C3000FFE,030000C0,030300C6,0303"
                        + "1FFF,030300C0,030300C180,03FF3FFF80,00,00,^PQ");
                                #endregion
                            }
                        }
                        else if (sLblCode.Equals("LBL0108"))
                        {
                            if (_sRes.Equals("300"))
                            {
                                #region Desay 구형 : LBL0108, 300dpi
                                sZplCode = sZplCode.Replace("^PQ", "^FO260,179^GFA,00660,00660,00020,"
                   + "00,00300000C0000180,003C0000600000C0,03380000606000E0C00000000C,03B80000707000"
                   + "60600E78781F,0738007FFFF1FFFFF00C387037,0638000607000C0C000C387033,063800030700"
                   + "060E000C387030,063860030600060C001C387070,0C3870030660061800183CF070,0FFFF0630C"
                   + "70C318C0183CF070,0C38007FFFF8C630E0183CF07000C03003,183800600000FFFFE0183CF0FC"
                   + "07E0FC0780,183800600000C330C0383DF0700FE0CC0780,303800600000C730C0303DF0700781"
                   + "8C03,303800600000C630C0303DB07007018C,6038C0600000C63FC0303FB0700700C0,6038E060"
                   + "0000CC1FC03037B0700700E0,1FFFF0600000D800C07037B0700700F0,003800600000F60CC060"
                   + "37B070070078,003800600000C7FCC060373070070038,003800600000C61CC06037307007001C"
                   + ",003800600000C61CC06037307007000C,003800600000C61CC0E037307007018E,003800600000"
                   + "C61CC0C0333070070186,003800600000C61CC0C033707007018C03,003800600000C7FCC0C033"
                   + "70700701CC0780,003818C00000C600C1C07EF8FC0F81F803,60383CC00000C007C0,7FFFFCC000"
                   + "00C001C0,000000C00000C001C0,00,00,"
                   + "^FO303,220^GFA,00396,00396,00012,"
                   + "00,0700E00E00C000E0000C60,0700F00E00E00180001E38,0600C00D80C00183003830,0600C0"
                   + "CCF0C0038381F060,0600C0CCD8C003FFC33060,0600C06D8CC00603003060,06C0D86D86C00607"
                   + "003060607FFFFC7D86C00C060036FFF00600C03F00C01C0E03FFCCC00E01C06E00C0360C0070CC"
                   + "C00E01C00D80C0631C00718D800F01C00DF0C0C19800718C,0F83C0FFF8C001F0007B0C,0F83C0"
                   + "0C0CC001E0007E6F,0EC3C00C0EC000E000FC7F,1EC6C03F06C001C0C0FCED801EE6C03F00D803"
                   + "8060FECD801E0CC06D80F807FFE0F6CCC0360CC06D80F00F0061B6CCC03618C06D87C03B0061B1"
                   + "8CE06618C06CFCC0730063318C606630C0CDE0C1C30063318C606660C0CC00C0030063330C6066"
                   + "C0C0CC00C0030063360C600600C0CC00C0030060360C,0600C00C00C0030060300C,0600C00C00"
                   + "C0030060300C,060FC00C00C007FFF030EC,0601C00C00C0070000303C,0601800C00C000000030"
                   + "18,00,00,"
                   + "^FO229,262^GFA,00792,00792,00024,"
                   + "00,00,00,000000000000000000000060,F0F0000000000000600000E0000E0F,70E00000000000"
                   + "00E00001E0000706,70E000000000000060000060000706,70E0000C0000000000000060000706"
                   + ",70E0000C0000000000000060000786,79E0000C0000000000000060000786,79E0000C00000000"
                   + "000000600007C6,79E0601C00600180000600600007C6060198060060,79E1F81F80F80FC0601F"
                   + "80600006C61F87DE0F80F0,7BE3180C019C1FC1E03180600006E6318FFE19C0F0,7BE3180C030C"
                   + "0F00603180600006E631877630C060,7B63980C030C0E006039806000067639866630C0,7F6318"
                   + "0C030C0E006031806000067631866630C0,6F60180C03FC0E00600180600006360186663FC0,6F"
                   + "60780C03000E006007806000063E07866630,6F60D80C03000E00600D806000063E0D866630,6E"
                   + "61980C03000E006019806000061E19866630,6E63980C03000E006039806000061E39866630,6E"
                   + "63180C03060E006031806000060E3186663060,6E63180C03060E006031806000060E3186663060"
                   + ",6663180E038C0E006031806000060E31866638C0,66E3380F838C0E0060338060000606338666"
                   + "38C060,66E3FE0F81F80E00603FE0600006063FE6661F80F0,FDF3CC0700F01F01F83CC1F8000F"
                   + "863CCFFF0F0060,00,00,00,00,00,"
                   + "^FO274,301^GFA,00660,00660,00020,"
                   + "00,000038180180,00301C1FFFC0,3FFB98180180000000000000000180,06C398180180073C3C"
                   + "000070000380,06C318180180061C380000F0000780,06C318180180061C38000030000180,06C3"
                   + "18180180061C38000030000180,06C3181801800E1C38000030000180,06DB181FFF800C1E7800"
                   + "0030000180,7FFF181800000C1E78000030000180,06C3181800000C1E7818003018018018,0CC3"
                   + "180000300C1E787E07F03E01803C,0CC318C000381C1EF8C70C706701803C,0CC318FFFFF8181E"
                   + "F8C30C70C3018018,0CC018060000181ED8C39830C30180,18C018060000181FD9839830C30180"
                   + ",18C018060000181BD9839830FF0180,30C1F80C0180381BD9819830C00180,30F0700FFFC0301B"
                   + "D9819830C00180,603C600001C0301B99819830C00180,0030C0000180301B99C19830C00180,00"
                   + "30C0000180301B99C19830C18180,1FFFE0000180701B99C19C30C18180,003000000180601998"
                   + "C31C70E30180,0030000001806019B8E30E70E3018018,0030000003806019B8660FF87E01803C"
                   + ",003018000300E03F7C3E07B03C07E018,00301800E7,7FFFFC003E,000000000C,00,00,"
                   + "^FO263,343^GFA,00660,00660,00020,"
                   + "00,0618000300C03030,071C000780E03818000C03,3618001E00C0301C030FFF80,3E1800FC30"
                   + "C0300C01F8030FF001CE0F,3618000C18C0630C60600307BC018706,36300C0C0CC061FFE060E3"
                   + "030E018706,37BFFC0C0CC06D806060E30306018706,3FF66C0D8CC0CF8060E0C30307038786,66"
                   + "66CCFFC0C0CD8060C0C30307030786,6666CCDC00C0CD8060C0C603070307C6,66CCCC1C60C1F9"
                   + "8060C0C603070307C606,66CCCC1C30C1D9FFE0C18603070306C60F,678CD81C18C1B18060CD86"
                   + "03070706E60F,0619981E1CC0318000FF86C3060606E606,07D9983F0CC0618001CDFFE30E0606"
                   + "76,0731983F00C061C061CF00E3FC060676,0E31983F80F0C1FFF1CC00C300060636,3E63183D80"
                   + "F8FFDB63CC00C3000E063E,7E63186C01F8F1DB63CC00C3000C063E,66C6186C7FC0C3DB63CC06"
                   + "C3000C061E,678618CC00C003DB63CFFFC3000C061E,060618CC00C007FFE0CC00C3000C060E,06"
                   + "0C18CC00C00FDB60CC00C3001C060E,061818CC00C03BDB60CC01830018060E,0618180C00C0F3"
                   + "DB60CC01830018060606,0630180C00C1E6DB60FC0183801806060F,0660380C00C186DB60CC01"
                   + "8FE0380F8606,0603300C00C186DBE0D81B80,0607F00C00C00CDBE0C00F,0600600C00C000C0C0"
                   + "0006,00,00,"
                   + "^FO288,390^GFA,00528,00528,00016,"
                   + "00,0660601C18,0730301E1C,061FF81818,0618301818C00707F0,061830183FE0060670,0618"
                   + "301830E0060C30,06DB301B30C0060C38,7FDBB0FFF8C00E0C18,061B301879C00C0C18,061B30"
                   + "18D9800C0C18,061B30198D800C0E006001800C0180,061B303F07000C0E00FF03E03E03C0,06DB"
                   + "303E07001C0701FF06706303C0,66DB303B0700180780E38C30630180,7FFB303B0D801803C0E3"
                   + "8C30C3,061BB07B19E01801E0E18C30C0,0C1BF07830F01800F0E18FF0C0,0E3BF0786038380070"
                   + "E18C00C0,0F3BF0D9E060300038E18C00C0,0D87C0DB7FE0300038E18C00C0,0D87C0D860603018"
                   + "18E18C00C0,0DC7C0D86060301818E18C18C0,18C7CC186060701818E18C18C380,18CDCC186060"
                   + "601818E30E30E3,300DCC186060601C38E30E30F30180,3019CC186060601E30F707E07E03C0,60"
                   + "39CC186060E01BE0FE03C03C0180,6030FC186060000000E0,00E0FC187FF0000000E0,01800018"
                   + "6000000000E0,000000000000000000E0,000000000000000001F8,"
                   + "^FO301,438^GFA,00528,00528,00016,"
                   + "0C,070600180180,6667800FFFC0,3676000C01C0,1E66000C01800707E0,1EC6000C0180060660"
                   + ",1F86000FFF80060C30,063C1C0C0180061C3830,7FFFFC0C01800E181830,1E0C300FFF800C18"
                   + "1830,1F8C300000300C181830,1ED830C000700C381C70000018,1E7830FFFFF80C381C7E1F383C"
                   + ",367C300000001C381C300E303C,667C300C018018381C300E3018,6006300FFFC018381C300630"
                   + ",0C06600C718018381C300630,0EE6600C718018381C300660,0CF6600FFF8038381C300760,0F"
                   + "E6600C718030381C300360,7EC3600C718030381C300360,78C3C00C71803018183003E0,7983C0"
                   + "0FFF803018183001C0,3181C01870007018183001C0,1F81C00070C0600C383801C0,0381C07FFF"
                   + "E0600C303E01C018,03C3606070006006603E01803C,06E370007060E003C01C018018,1C663800"
                   + "7070000180000180,700C1CFFFFF80001C0000180,0038000000000000C0000F,00000000000000"
                   + "0070001F,00000000000000001C000C,"
                   + "^FO297,481^GFA,00396,00396,00012,"
                   + "00,060000060000000000C3,070000070000300300E3B0E00600180C0000300380C31FE0067FFC"
                   + "0C00003FFF80C318C00600C00C00C0300380C3D8C00780C00FFFE0300383FFF8C067C0C0180060"
                   + "300380C318C07FE0C0180060300380C318C00600C0180060300380C318C00600C03000C0300380"
                   + "C318C00600C03818C0300380FF1FC00600C07FFCC0300380C318C00600C07C18C0300380C318C0"
                   + "06C0C0DC18C0300380C318C006E0C0DC18C03FFF80C318C00780C01C18C0300380FF18C00E00C0"
                   + "1C18C0300380C330C03E00C01C18C0300380C33FC07600C01FFCC0300380C330C06600C01C00C0"
                   + "300380C3F0C00600C01C00C0300383C3F0C00600C01C0FF0300383FFF0C00600C01C03B0300380"
                   + "6030C00600C01C00303003807E30C00600C01C00303003806660C00600C01C00303FFF80E360C0"
                   + "0600C01C0030300380C3C0C06E0DC00E00F830038183C7C03E07C007FFE03003030181C00C0180"
                   + "00000000000301818000,00,"
                   + "^FO246,521^GFA,00660,00660,00020,"
                   + "00,00,00,0000000003,FF0000000300060000000000FE,7BC000000F000E00000000007780,30"
                   + "E00000030006000000000031C0,30600000030000000000000030C0000C,307000000300000000"
                   + "00000030E0000C,3070000003000000000000003060000C,307000000300000000000000306000"
                   + "0C,3070600303000000300C00003070601C006006,3071F80F833E0603781F80003071F81F80F8"
                   + "0F,30731818C3181E07F831E0003073180C019C0F,30631818C33006039C3180003073180C030C"
                   + "06,30E39830C36006038C31C0003073980C030C,3FC31830036006030C30C0003073180C030C,30"
                   + "00183003C006030C30C0003070180C03FC,3000783003C006030C30C0003070780C03,3000D830"
                   + "03C006030C3180003070D80C03,3001983003E006030C1980003071980C03,3003983003600603"
                   + "0C1F00003063980C03,30031830037006030C1E00003063180C0306,30031830E33006030C3000"
                   + "0030E3180C0306,30031838C33806030C30000030E3180E038C,3003383CC31806030C38000031"
                   + "C3380F838C06,3803FE1F839C06039C3FC0007383FE0F81F80F,FE03CC0F0FFF1F87FE1FE000FF"
                   + "03CC0700F006,0000000000000000003060,0000000000000000006060,00000000000000000070"
                   + "60,0000000000000000007FC0,0000000000000000001E,"
                   + "^FO317,568^GFA,00396,00396,00012,"
                   + "00,0003800003,000780001F80,000E0003FC,003C003E60000703FBFDC0,01E000006000060639"
                   + "B980,1FC000006030060C399980,30C000FFFFF8060C199980,00C1800060000E18199980,00C0"
                   + "C00060000C1819D980,00C3E03060C00C1819DD80,00FE001FFFE00C3801DD81801FC0001860C0"
                   + "0C3800FD83C038C0001860C01C3800FF03C000C0001860C0183000FF018000C0301FFFC018307C"
                   + "FF,00C0381860C0183038FF,00C3F01860C0183038FF,00FE001860C0383018FF,3FC0001860C0"
                   + "303818F7,60C0001FFFC030381867,00C0001860C030181867,00C00000600030181866,00C018"
                   + "0060C070181866,00C0186060E0601C1866,00C0183FFFE0600C1866018000C018006000600630"
                   + "6603C000C018006030E003E066018000E01C006030,007FFCFFFFF8,00,00,00,"
                   + "^FO250,615^GFA,00660,00660,00020,"
                   + "00,0060000006,0030001C07,0030301C06,003838180600073C3C000070000000FF,3FFFF818E6"
                   + "00061C380000F00000003C,03038018C600061C3800003000000018,01838018C660061C380000"
                   + "3000000018,01830018C6300E1C3800003000000018,0183301EC6F80C1E7800003000000018,31"
                   + "8638FFC7E00C1E7800003000000018,3FFFFC18DE600C1E781800301800001800C018,30000018"
                   + "F6600C1E787E07F03E0000180DE03C,30000019C6601C1EF8C60C70670000181FE03C,3000001B"
                   + "C660181EF8C60C70C30000180E7018,30000018C6C0181ED8E61830C30000180E30,30000018C6"
                   + "C0181FD8C61830C30000180C30,30000018C6C0181BD8061830FF0000180C30,30000018C6C038"
                   + "1BD81E1830C00000180C30,30000018C6C0301BD8361830C00000180C30,3000001BC780301B98"
                   + "661830C00000180C30,3000001EC600301B98E61830C00000180C30,3000001CC600301B98C618"
                   + "30C18000180C30,30000038C618701B98C61C30C18000180C30,300000F0C018601998C61C70E3"
                   + "0000180C30,300000E0C0186019B8CE0E70E30000180C3018,300000C0C0186019B8FF8FF87E00"
                   + "00180E703C,60000000C018E03F7CF307B03C0000FF1FF818,60000000E038,600000007FF8,60"
                   + ",00,00,"
                   + "^FO251,663^GFA,00660,00660,00020,"
                   + "00,00C0,00E0007030,018180181800000000000000000000C0,01FFC01C1800073FC000000000"
                   + "0000C0,0303800C0C00061EE0000000000003C0,0783000C0C60060C70000000000000C0,06C700"
                   + "000C70060C30000000000000C0,0CC60003FFF80E0C38000000000000C0,186C00C00C000C0C38"
                   + "000000000000C0,303800E00C000C0C38000000000000C0,003C00700C000C0C38180660180060"
                   + "C0018018,006E00300C000C0C383E1F787E03F0CF87E03C,01C380380C001C0C30673FF8C607F0"
                   + "C606603C,0381F8300C00180C70C31DD8C603C0CC0C6018,0E007C000C00180DE0C31998E60380"
                   + "D80C60,3C0060000CC0180F80C31998C60380D806,6FFFE00D8CE0180DC0FF1998060380F007,0C"
                   + "30600DFFF0380DC0C019981E0380F00780,0C3060180C00300CC0C01998360380F003C0,0C3060"
                   + "180C00300CE0C01998660380F801C0,0C3060180C00300CE0C01998E60380D800E0,0C3060300C"
                   + "00300C60C19998C60380DC0060,0FFFE0300C00700C70C19998C60380CC0C70,0C3060300C0060"
                   + "0C70E31998C60380CE0C30,0C3060600C00600C30E31998CE0380C60C6018,0C3060E00C00601E"
                   + "387E1998FF8380E70E603C,0C3060E00C30E03F3C3C3FFCF307C3FFCFC018,0FFFE0CE0C78,0C00"
                   + "60C7FFC0,0C0060,00,00,^PQ");
                                #endregion
                            }
                            else
                            {
                                #region Desay 구형 : LBL0108, 203dpi
                                sZplCode = sZplCode.Replace("^PQ", "^FO176,121^GFA,00368,00368,00016,"
                        + "0000000000C0,00C000C00060,0F8000C3006180C00038,0D803FFF9FFFC0CE38F8,0D80061C03"
                        + "0C00C630D8,198C0318018E00CE30C0,198E031B0198018F70C0,1FF833339999818F70C0,3180"
                        + "1FFE0FFFC18F71F07C7C1C,318018001999818FF0C0FC6C1C,618018001999818FF0C0606C,C18C"
                        + "3000199F830FF0C06060,318C30001B01830FF0C06070,1FFE30001F8D830DF0C06038,01803000"
                        + "19FD830DF0C0601C,01803000198D860DB0C0600C,01803000198D860DB0C0606C,01803000198D"
                        + "860DB0C0606C1C,0180300019F9860FF9F0F06C1C,01833000180D86000000003818,FFFF300018"
                        + "0780,000030001803,00,"
                        + "^FO205,149^GFA,00276,00276,00012,"
                        + "00000C000060,0C0E0607007000CE,0C0C6D8600C303EC,0C0C3DE600FF1F0C,0C0E3DBE018603"
                        + "1860,CF8F1F1E030603DFF0,7EFF9F06030C1FFB60,1C1C0C06078C0333C0,1C1C0DE60CD80333"
                        + "C0,1E3C7FF6187003E3,3E3C0C1E003003CF80,3F6C1F1E006187DBC0,3F6C1F0780FFC7DBC0,6C"
                        + "CC3D87C3818FFB60,6CCC3DBE07818F3360,CD8C3DE61D819B3360,CF0C6C0631819B6360,0E0C"
                        + "6C06018183C360,0C0C0C0601818303,0CCC0C060180C333,0C780C0601FF831F,18000C060000"
                        + "030C,00,"
                        + "^FO155,177^GFA,00368,00368,00016,"
                        + "00,00,000000000000C00030,E38000000000C000700073C0,630000000000C00030003180,E300"
                        + "06000000000030003980,F70006000000000030007980,F70006000000000030007980,F71E0F83"
                        + "C1F0C0F030006D8F1FE1E0E0FF330606C3F1C19830006D999FE360E0FF3B060C6180C1D830006F"
                        + "9D8DE630,FF33060FE180C198300067998DE7F0,FF07060C0180C038300067838DB6,DF0F060C01"
                        + "80C078300063878DB6,DF1B060C0180C0D83000638D8DB6,DB33060C6180C198300063998DB630"
                        + ",DB33060C6180C198300061998DB630,DB330606C180C198300061998DB360E0FFBF8787C3C1F1"
                        + "FC7C00319FDFF3E0E0001B0303000000D80000798D800180C000,00,00,"
                        + "^FO185,204^GFA,00368,00368,00016,"
                        + "0000180C,01870FFE,7FFE0C0C01800000300030,1B360C0C019C7000700070,1B360C0C018C60"
                        + "00300030,1B360C0C019C6000300030,1BF60FFE031EE000300030,7FF60C00031EE000300030,1B"
                        + "360003031EE3E1F078301C,1B367003831FE66330D8301C,1B361FFE031FE633318C30,33060600"
                        + "061FE63331FC30,331E0600061FE633318030,638607FE061BE633318030,60C00C06061BE63331"
                        + "8030,018C00060C1B6633318C30,3FFC000C0C1B6633318C30,0180000C0C1B673338D8301C,01"
                        + "80000C0C1FF361FCF87C1C,0183018C0C0001C0F0600018,7FFF00F8,00000060,00,"
                        + "^FO178,232^GFA,00368,00368,00016,"
                        + "30,1CE00387061800C180,78C01E06060C0FFFC00030,78C07C660D8D83018FE0339E,7D830C36"
                        + "0CFFC61D8630318C,7FFF8F360DC18619861831CC,7F6F7F861BC18619861863CC,DB7B0C061BC1"
                        + "8631861863CC,DE7B0C661EFF86318618636C38,DEDB0C361EC186F30618636C38,1EDB0E360CC0"
                        + "07F36630637C,1D9B1F060CE0CEDFE7E0C33C,39B31F079BFFCEC06600C33C,FB333C0FDEFEDEC0"
                        + "6600C31C,DE633CFE18FEDEC3C600C31C,18636C0601FFC6FEC601831C,18C36C0603FEC6C0C601"
                        + "830C,18C60C060FFEC6C0C601830C38,19860C061DFEC7C0CF81818C38,1B660C0619FFC6C6C001"
                        + "83CC30,183E0C06037FC60380,18180C0C006180,00,"
                        + "^FO195,264^GFA,00276,00276,00012,"
                        + "00,1D860E30,18FE0C38018360,19860C630187E0,19860E7F018C60,7FB66FE6018C60,19B63F"
                        + "E6030C60,19B60DBC030C60,19B60F1C030E03E0F07838,1FB61E18030707F1B0CC38,7FB61F1C"
                        + "03038333198C,19BE3F370601C333F980,19BE3CE3C600E3330180,1DBE3DC30600633B0180,1E"
                        + "786FFF060063330180,36786CC30C0C63331980,367B0CC30C0C633319CC,30DB0CC30C0C6331B0"
                        + "D838,60DB0CC30C0EC3E1F0F838,619B0CC30C0F83C0C06030,030F8CFF000003,0E000C000000"
                        + "07,0000000000000780,"
                        + "^FO204,296^GFA,00276,00276,00012,"
                        + "0C,6F300C06,6FE00FFE0183,3F600C060187C0,3F630FFE018C60,7FFF0C06018C6180,3CC78F"
                        + "FE030C6180,3EC6000183183180,3FC63FFF831833E3D870,6FE60000031831819870,D86607FE"
                        + "0318318198,186C0CC606183181B0,1F6C0CC606183181B0,1FEC0FFE06183180F0,F63C0CC606"
                        + "183180F0,F63C07FE0C0C6180E0,361800C00C0C618060,1C1830C70C0C61806070,0F3C1FFC0C"
                        + "06C1E06070,1B6600C30C0380C0C060,70C3BFFF80018000C0,018000000001C003C0,00000000"
                        + "0000700180,"
                        + "^FO201,326^GFA,00276,00276,00012,"
                        + "00000C,1C0007000603077C60,1803060007FF0667E0,1BFF0C000603067E60,1E180FFF06031F"
                        + "FE60,7F180C060603066660,181818060603066660,1818186606030667E0,18183FF6060307E6"
                        + "60,18183C360603066660,1B186C3607FF066660,1E180C36060307E660,38180C3606030667E0"
                        + ",F8180FF60603066C60,D8180C060603067C60,18180C3F86031FFC60,18180C0D860303CC60,18"
                        + "180C018603036C60,18180C0187FF067860,18180E018603067B60,78F803FF06030C31E0,3030"
                        + "00000000186180,00,"
                        + "^FO166,352^GFA,00368,00368,00016,"
                        + "00,00,0000000C0180,FE00001C0180000000FC,6300000C018000000066,6180000C0000000000"
                        + "630006,6180000C0000000000630006,6180000C0000000000618006,619E078DE18370F000619E"
                        + "0F83C1C0,61B30CCCC387F19C0061B30606C1C0,633B18CD8181B1980061BB060C60,7E33180F01"
                        + "8331980061B3060FE0,6007180E01833198006187060C,600F180F018330D800618F060C,601B18"
                        + "0F018330D800619B060C,6033180D818330F0006333060C60,60331CCD81833180006333060C60"
                        + ",60330D8CC18331C00067330606C1C0,F83F8F9FF3E7F8FC00FE3F8787C1C0,001B06000000018C"
                        + "00001B03030180,000000000000018C,00000000000001DC,00000000000000F0,"
                        + "^FO214,384^GFA,00276,00276,00012,"
                        + "0030,0038003C,00E007E60181E0,03801CC00187EFF8,3F0000C3818C6798,03003FFF818C66F0"
                        + ",031C00C0030C66F0,03F818C7031866F0,1F801FFE031806F0E0,730018C6031807F0E0,030618"
                        + "C6031807F0,03071FFE0618F3F0,03FC18C606186360,7F8018C606186360,C3000FFE06186360"
                        + ",030000C00C0C6360,030300C60C0C6360,03031FFF0C0C6360E0,030300C00C066360E0,030300"
                        + "C18C03C360C0,03FF3FFF80,00,00,"
                        + "^FO169,416^GFA,00368,00368,00016,"
                        + "00,01800C18,01860C180180000030,7FFF18D8019C70007000003E,0C381998018C6000300000"
                        + "18,0630199B019C600030000018,06361F9F831EE00030000018,66677F9F031EE00030000018,3F"
                        + "FC19FB031EE3C1F07800183707,30001BDB031FE66330D800187F07,3000199E031FE763318C00"
                        + "181B,6000199E061FE66331FC001833,6000199E061FE0E33180001833,6000199E061BE1E33180"
                        + "001833,60001B98061BE3633180001833,60001F980C1B6663318C001833,60001D818C1B666331"
                        + "8C001833,600071818C1B666338D800183307,600061818C1FF7F1FCF8003E7F87,600000C18C00"
                        + "0360F06000000006,600000FFC0,60,00,"
                        + "^FO170,448^GFA,00368,00368,00016,"
                        + "00,03001860,06180C600180000000000060,0FF80C30019FC000000000E0,0E300C33018CE000"
                        + "00000060,1B6003FF818C600000000060,31E03030030C600000000060,60C03030030C60000000"
                        + "0060,03E01830030C61E7F8F03E6F1F07,06381830030C6367F9987E661B07,1C0F0030030DC633"
                        + "79D8306C1B,780C0033060F87F37998307818,1FFC0DFF060D86036C3830701C,198C0C30060D86"
                        + "036C7830780E,198C0C30060DC6036CD8307807,198C18300C0CC6336D98306C03,1FFC18300C0C"
                        + "C6336D98306C1B,198C18300C0CE3636D9830661B07,198C30330C1F73E7FDFC78FF9B07,1FFC30"
                        + "338C00018000D800000E06,180C3FFE,00,00,^PQ");
                                #endregion
                            }
                        }
                        PrintLabel(sZplCode, _drPrtInfo);
                    }

                    GetPalletList();

                    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                    GetDetailInfo();
                    //   txtBoxWeight.Text = string.Empty;
                    //     txtBoxId.Text = string.Empty;
                    txtBoxId.Focus();
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    //if (chkAuto.IsChecked == true) txtBoxWeight.Text = string.Empty;

                }

            }, indataSet);
        }


        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Events

        private void txtRealQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!CheckDecimal(txtRealQty.Text, 0))
                {
                    txtRealQty.Clear();
                    return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool CheckDecimal(string str, decimal dMinValue)
        {
            bool bRet = false;

            if (str.Trim().Equals(""))
                return bRet;

            decimal value;
            if (!decimal.TryParse(str, out value))
            {
                //숫자필드에 부적절한 값이 입력 되었습니다.
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2877"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bRet;
            }
            if (value < dMinValue)
            {
                //숫자필드에 허용되지 않는 값이 입력 되었습니다.
                ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2877"), null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return bRet;
            }

            bRet = true;

            return bRet;
        }

        //ESNJ 실제 OUTBOX 수량 저장
        private void txtRealQty_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtRealQty.Text.Trim() != "")
                        ModifyRealQty();
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [추가기능]
        private void btnExtra_MouseLeave(object sender, MouseEventArgs e)
        {
            btnExtra.IsDropDownOpen = false;
        }

        /// <summary>
        /// 출하처변경
        /// </summary>
        private void btnChangeShipto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                BOX001_204_CHANGE_SHIPTO puChangeShipto = new BOX001_204_CHANGE_SHIPTO();
                puChangeShipto.FrameOperation = FrameOperation;

                if (puChangeShipto != null)
                {
                    string sBoxID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                    string sShipto = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_ID"].Index).Value);
                    string sShiptoName = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["SHIPTO_NAME"].Index).Value);
                    string sProdID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);

                    object[] Parameters = new object[4];
                    Parameters[0] = sBoxID;
                    Parameters[1] = sProdID;
                    Parameters[2] = sShiptoName;
                    Parameters[3] = txtWorker_Main.Tag;
                    C1WindowExtension.SetParameters(puChangeShipto, Parameters);

                    puChangeShipto.Closed += new EventHandler(puChangeShipto_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puChangeShipto);
                    puChangeShipto.BringToFront();
                }
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// 포장해체
        /// </summary>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                  && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
            {
                //	SFU4416	이미 출고된 팔레트 입니다.
                Util.MessageValidation("SFU4416");
                return;
            }

            string sPalletID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            this.FrameOperation.OpenMenu("SFU010060520", true, new object[] { sPalletID, txtShift_Main.Tag, txtShift_Main.Text, txtWorker_Main.Tag, txtWorker_Main.Text, ucBoxShift.TextShiftDateTime.Text });

        }

        private void puChangeShipto_Closed(object sender, EventArgs e)
        {
            BOX001_204_CHANGE_SHIPTO popup = sender as BOX001_204_CHANGE_SHIPTO;

            if (popup != null)
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                GetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion

        #region [작업시작 ~ BOX List 발행]
        /// <summary>
        /// 작업시작
        /// </summary>
        private void btnRunStart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtShipto.Text.ToString()))
            {
                // SFU4999 출하처 정보가 없습니다.
                Util.MessageValidation("SFU4999");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLabelType.Text))
            {
                // SFU1522 라벨 타입을 선택하세요
                Util.MessageValidation("SFU1522");
                return;
            }

            BOX001_231_RUNSTART popup = new BOX001_231_RUNSTART();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[9];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = cboLine.SelectedValue;
                Parameters[2] = txtWorker_Main.Tag;  
                Parameters[3] = txtShift_Main.Tag;
                Parameters[4] = txtShipto.Tag;
                Parameters[5] = txtShipto.Text;
                Parameters[6] = txtLabelType.Text;
                Parameters[7] = txtFIFO.Text;
                Parameters[8] = cboEquipment_Search.SelectedValue.ToString();

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puRun_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }
        }
        private void puRun_Closed(object sender, EventArgs e)
        {
            BOX001_231_RUNSTART popup = sender as BOX001_231_RUNSTART;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                //cboArea.SelectedValue = popup.AREAID;
                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", popup.PALLETID, true);
                GetDetailInfo();
            }
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 작업취소
        /// </summary>
        private void btnRunCancel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
            Util.MessageConfirm("SFU1168", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("BOXID");
                        dt.Columns.Add("USERID");
                        dt.Columns.Add("LANGID");
                        DataRow dr = dt.NewRow();
                        dr["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        dr["USERID"] = txtWorker_Main.Tag;
                        dr["LANGID"] = LoginInfo.LANGID;
                        dt.Rows.Add(dr);
                        new ClientProxy().ExecuteService("BR_PRD_DEL_12ND_PLT_2D_NJ", "INDATA", null, dt, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            GetPalletList();
                            SetDetailClear();
                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                        });
                                              
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        /// <summary>
        /// 실적확정
        /// </summary>
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU1235	이미 확정 되었습니다.
                    Util.MessageValidation("SFU1235");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKING")
                {
                    //SFU2048		확정할 수 없는 상태입니다.	
                    Util.MessageValidation("SFU2048");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                 && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4414	출고중 팔레트는 실적확정 불가합니다. 	
                    Util.MessageValidation("SFU4414");
                    return;
                }

                if (Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["INPUT_QTY"].Index).Value)
                    != Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value))
                {
                    //SFU4417	투입수량과 포장수량이 일치하지 않습니다.	
                    Util.MessageValidation("SFU4417");
                    return;

                }

                int iTotalQty = Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["TOTAL_QTY"].Index).Value);
              //  int iCellQty = Util.NVC_Int(txtTotalCellQty.Text);

              




                // SFU1716 실적확정 하시겠습니까? 
                Util.MessageConfirm("SFU1716", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_END_2ND_PLT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                //   TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 실적취소
        /// </summary>
        private void btnConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }
                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4262		실적 확정후 작업 가능합니다.	
                    Util.MessageValidation("SFU4262");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4415	출고중 팔레트는 실적취소 불가합니다. 	
                    Util.MessageValidation("SFU4415");
                    return;
                }

                //		SFU4263	실적 취소 하시겠습니까?	
                Util.MessageConfirm("SFU4263", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_END_2ND_PLT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 출고
        /// Biz : BR_PRD_REG_SHIPMENT_NJ
        /// </summary>
        private void btnShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4413		포장 완료된 팔레트만 출고 가능합니다.	
                    Util.MessageValidation("SFU4413");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value))
                    && Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) == "SHIPPING")
                {
                    //	SFU4416	이미 출고된 팔레트 입니다.
                    Util.MessageValidation("SFU4416");
                    return;
                }

                //SFU2802	포장출고를 하시겠습니까?
                Util.MessageConfirm("SFU2802", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        loadingIndicator.Visibility = Visibility.Visible;
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;

                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                //GetDetailInfo();

                                TagPrint();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// 출고취소
        /// Biz : BR_PRD_REG_CANCEL_SHIPMENT_NJ
        /// </summary>
        private void btnCancelShip_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
                {
                    //SFU4417   실적 확정된 팔레트만 출고 취소 가능합니다.
                    Util.MessageValidation("SFU4417");
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RCV_ISS_STAT_CODE"].Index).Value) != "SHIPPING")
                {
                    //SFU3717		출고중 상태인 팔레트만 출고취소 가능합니다.	
                    Util.MessageValidation("SFU3717");
                    return;
                }

                //	SFU2805		포장출고를 취소하시겠습니까?	
                Util.MessageConfirm("SFU2805", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataTable RQSTDT = new DataTable("INDATA");
                        RQSTDT.Columns.Add("BOXID");
                        RQSTDT.Columns.Add("USERID");

                        DataRow newRow = RQSTDT.NewRow();
                        newRow["USERID"] = txtWorker_Main.Tag;
                        newRow["BOXID"] = sPalletId;
                        RQSTDT.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_CANCEL_SHIPMENT_NJ", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }
                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                Util.MessageInfo("SFU3431");
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        /// <summary>
        /// PACKINGLIST 발행
        /// </summary>
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            TagPrint();
        }

        /// <summary>
        ///BOXLIST 발행
        /// </summary>
        private void btnBoxList_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            Report_Box_List popup = new Report_Box_List();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(BoxListPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void BoxListPopup_Closed(object sender, EventArgs e)
        {
            Report_Box_List popup = sender as Report_Box_List;
            if (popup != null)
            {
                string sPalletId = popup.PALLET_ID;

                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                // GetDetailInfo();
            }

        }
        #endregion

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetDetailClear();
            GetPalletList();
        }

        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPalletList();
            }
        }

        /// <summary>
        /// 포장대기, 포장중, 포장완료, 출고요청 Checked, Unchecked
        /// </summary>
        private void chkSearch_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    _searchStat += CREATED;
                    break;
                case "chkPacking":
                    _searchStat += PACKING;
                    break;
                case "chkPacked":
                    _searchStat += PACKED;
                    break;
                case "chkShipping":
                    _searchStat += SHIPPING;
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            // bInit = false;
        }

        private void chkSearch_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            switch (chk.Name)
            {
                case "chkCreated":
                    if (_searchStat.Contains(CREATED))
                        _searchStat = _searchStat.Replace(CREATED, "");
                    break;
                case "chkPacking":
                    if (_searchStat.Contains(PACKING))
                        _searchStat = _searchStat.Replace(PACKING, "");
                    break;
                case "chkPacked":
                    if (_searchStat.Contains(PACKED))
                        _searchStat = _searchStat.Replace(PACKED, "");
                    break;
                case "chkShipping":
                    //_rcvStat += SHIPPING;
                    if (_searchStat.Contains(SHIPPING))
                        _searchStat = _searchStat.Replace(SHIPPING, "");
                    break;
                default:
                    break;
            }
            if (!bInit)
                btnSearch_Click(null, null);
            //  bInit = false;
        }

        private void lblCreated_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkCreated.IsChecked == true)
                chkCreated.IsChecked = false;
            else
                chkCreated.IsChecked = true;
        }
        private void lblPacking_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacking.IsChecked == true)
                chkPacking.IsChecked = false;
            else
                chkPacking.IsChecked = true;
        }
        private void lblPacked_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkPacked.IsChecked == true)
                chkPacked.IsChecked = false;
            else
                chkPacked.IsChecked = true;
        }
        private void lblShipping_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (chkShipping.IsChecked == true)
                chkShipping.IsChecked = false;
            else
                chkShipping.IsChecked = true;
        }

        /// <summary>
        /// 작업 Pallet 선택
        /// </summary>
        private void dgPalletList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == C1.WPF.DataGrid.DataGridRowType.Item)
                {
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblCreated.Tag))
                    {
                        e.Cell.Presenter.Background = lblCreated.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacking.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacking.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblPacked.Tag))
                    {
                        e.Cell.Presenter.Background = lblPacked.Background;
                    }
                    else if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BOXSTAT_LIST")).Equals(lblShipping.Tag))
                    {
                        e.Cell.Presenter.Background = lblShipping.Background;
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString) || string.IsNullOrEmpty((rb.DataContext as DataRowView).Row["CHK"].ToString())))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
       
                }

                //row 색 바꾸기
                dgPalletList.SelectedIndex = idx;
                
                

                SetDetailClear();
                GetDetailInfo();
            }
        }

        #region 포장상세, 투입대차

        /// <summary>
        /// 포장상세 - 제품조회
        /// </summary>
        private void btnProdID_Click(object sender, RoutedEventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = new CMM001.Popup.CMM_BOX_PROD();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = LoginInfo.CFG_AREA_ID;
                Parameters[1] = txtPRODID.Text;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puProduct_Closed);

                //grdMain.Children.Add(popup);
                //popup.BringToFront();
                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }

        private void puProduct_Closed(object sender, EventArgs e)
        {
            CMM001.Popup.CMM_BOX_PROD popup = sender as CMM001.Popup.CMM_BOX_PROD;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                txtPRODID.Text = popup.PRODID;
                //btnPRODID.Tag = popup.PRODID;
            }
            //this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// 포장상세 저장
        /// </summary>
        private void btnUpdatePallet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string boxStat = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value);

                if (boxStat.Equals("PACKED"))
                {
                    //SFU4296	포장 완료된 팔레트입니다. 수정 불가합니다.
                    Util.MessageValidation("SFU4296");
                    return;
                }

                // SFU4007 Pallet를 수정 하시겠습니까?	
                Util.MessageConfirm("SFU4007", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        string textRange = new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text;
                        DataSet indataSet = new DataSet();
                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("LANGID");
                        inDataTable.Columns.Add("USERID");

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");
                        inBoxTable.Columns.Add("WIPQTY");
                        inBoxTable.Columns.Add("EQPTID");
                        inBoxTable.Columns.Add("PROCID");
                        inBoxTable.Columns.Add("SOC_VALUE");
                        inBoxTable.Columns.Add("EXP_DOM_TYPE_CODE");
                        inBoxTable.Columns.Add("PACK_NOTE");
                        inBoxTable.Columns.Add("LABEL_ID");
                        inBoxTable.Columns.Add("PKG_LOTID");

                        DataRow newRow = inDataTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        inDataTable.Rows.Add(newRow);

                        newRow = inBoxTable.NewRow();
                        newRow["BOXID"] = sPalletId;
                        newRow["WIPQTY"] = txtInputQty.Value;
                        newRow["EQPTID"] = cboEquipment.SelectedValue;
                        newRow["PROCID"] = cboProcType.SelectedValue;
                        newRow["EXP_DOM_TYPE_CODE"] = cboExpDomType.SelectedValue;
                        newRow["PACK_NOTE"] = textRange.LastIndexOf(System.Environment.NewLine) < 0 ? textRange : textRange.Substring(0, textRange.LastIndexOf(System.Environment.NewLine));
                        newRow["LABEL_ID"] = cboLabelType.SelectedValue;
                        newRow["PKG_LOTID"] = txtPkgLotID.Text;
                        inBoxTable.Rows.Add(newRow);

                        loadingIndicator.Visibility = Visibility.Visible;
                        new ClientProxy().ExecuteService_Multi("BR_PRD_UPD_PALLET_DETAIL_FOR_2D_NJ", "INDATA,INBOX", "OUTDATA,OUTINBOX,OUTCTNR", (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageExceptionNoEnter(bizException, msgResult =>
                                    {
                                        if (msgResult == MessageBoxResult.OK)
                                        {
                                            txtBoxId.Focus();
                                            txtBoxId.Text = string.Empty;
                                        }
                                    });
                                    return;
                                }

                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                //SFU1265 수정되었습니다.	
                                Util.MessageInfo("SFU1265");

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {

            }
        }

        /// <summary>
        /// 투입대차 - 시작INBOX/대차ID
        /// </summary>
        private void txtStartPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            //BR_PRD_REG_INPUT_CTNR_NJ
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                    {
                        //SFU1843	작업자를 입력 해 주세요.
                        Util.MessageValidation("SFU1843");
                        return;
                    }

                    int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                    if (idxPallet < 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtStartPalletID.Text))
                    {
                        //SFU3350	입력오류 : PALLETID 를 입력해 주세요.
                        Util.MessageValidation("SFU3350");
                        return;
                    }

                    //SFU1987	투입처리 하시겠습니까?        
                    Util.MessageConfirm("SFU1987", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                            string sProdId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["PRODID"].Index).Value);

                            DataSet indataSet = new DataSet();
                            DataTable inDataTable = indataSet.Tables.Add("INDATA");
                            inDataTable.Columns.Add("LANGID");
                            inDataTable.Columns.Add("BOXID");
                            inDataTable.Columns.Add("USERID");

                            DataTable inCtnrTable = indataSet.Tables.Add("INCTNR");
                            inCtnrTable.Columns.Add("LOTID");

                            DataRow newRow = inDataTable.NewRow();
                            newRow["LANGID"] = LoginInfo.LANGID;
                            newRow["BOXID"] = sPalletId;
                            newRow["USERID"] = txtWorker_Main.Tag;
                            inDataTable.Rows.Add(newRow);

                            newRow = inCtnrTable.NewRow();
                            newRow["LOTID"] = txtStartPalletID.Text;
                            inCtnrTable.Rows.Add(newRow);

                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_INPUT_ADD_CTNR_12ND_NJ", "INDATA,INCTNR", "OUTCTNR", (bizResult, bizException) =>
                            {
                                try
                                {
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }

                                    //SFU1973	투입완료되었습니다.
                                    Util.MessageInfo("SFU1973");

                                    GetPalletList();
                                    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                    GetDetailInfo();
                                    txtBoxId.Text = string.Empty;
                                    txtRemainQty.Value = 0;

                                }
                                catch (Exception ex)
                                {
                                    Util.MessageException(ex);
                                }
                                finally
                                {
                                    txtStartPalletID.Text = string.Empty;
                                    txtStartPalletID.Focus();
                                }
                            }, indataSet);
                        }
                        else
                            loadingIndicator.Visibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void checkAll2_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgInTray.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInTray.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll2.IsChecked)
            {
                for (int i = 0; i < dgInTray.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInTray.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        /// <summary>
        /// 투입대차 - 그리드
        /// </summary>
        private void dgInTray_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre2.Content = chkAll2;
                            e.Column.HeaderPresenter.Content = pre2;
                            chkAll2.Checked -= new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked -= new RoutedEventHandler(checkAll2_Unchecked);
                            chkAll2.Checked += new RoutedEventHandler(checkAll2_Checked);
                            chkAll2.Unchecked += new RoutedEventHandler(checkAll2_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgInTray_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string boxStat = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value);

                if (boxStat.Equals("PACKED"))
                {
                    //SFU4296	포장 완료된 팔레트입니다. 수정 불가합니다.
                    Util.MessageValidation("SFU4296");
                    e.Cancel = true;
                    return;
                }

                return;
            }

            if (Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "CHK")) != bool.TrueString)
            {
                e.Cancel = true;
                return;
            }
        }

        private void dgInTray_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {

            if (e.Cell.Column.Name == "RESTQTY")
            {
                int wipqty = Util.NVC_Int(dgInTray.GetCell(e.Cell.Row.Index, dgInTray.Columns["WIPQTY"].Index).Value);

                if (wipqty < Util.NVC_Int(e.Cell.Value))
                {
                    //SFU4053	잔량은 투입수량보다 작아야 합니다.	
                    Util.MessageValidation("SFU4053");
                    e.Cell.Value = 0;
                }

                DataTable dtInfo = DataTableConverter.Convert(dgInTray.ItemsSource);
                int restQty = 0;
                foreach (DataRow dr in dtInfo.Rows)
                {
                    restQty += Util.NVC_Int(dr["RESTQTY"]);
                }

                // Pallet의잔량과 비교
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
                int RemainQty =  Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RESTQTY"].Index).Value);

                //SFU4336		남은 수량이내에서 잔량 처리 가능합니다.	
                if (restQty > RemainQty)
                {
                    Util.MessageValidation("SFU4336");
                    txtRemainQty.Value = restQty - Util.NVC_Int(e.Cell.Value);
                    e.Cell.Value = 0;
                }
                else
                    txtRemainQty.Value = restQty;
            }
        }

        /// <summary>
        /// 투입대차 - 잔량처리
        /// </summary>
        private void btnRemain_Click(object sender, RoutedEventArgs e)
        {
            DataTable dtInfo = DataTableConverter.Convert(dgInTray.ItemsSource);
            int restCellQty = 0;
            foreach (DataRow dr in dtInfo.Rows)
            {
                restCellQty += Util.NVC_Int(dr["RESTQTY"]);
            }

            txtRemainQty.Value = restCellQty;

            //BR_PRD_REM_INPUT_CTNR_NJ

            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
            {
                //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                Util.MessageValidation("SFU3610");
                return;
            }

            //////////////////////// 불량수량 체크
            int restQty = Util.NVC_Int(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["RESTQTY"].Index).Value);

            if (restQty <= 0)
            {
                //SFU1859 SFU 잔량이 없습니다.
                Util.MessageValidation("SFU1859");
                return;
            }

            if (txtRemainQty.Value > restQty)
            {
                //SFU4336		남은 수량이내에서 잔량 처리 가능합니다.	
                Util.MessageValidation("SFU4336");
                return;
            }

            //if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            //{
            //    return;
            //}


            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

            if (idxBoxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            //SFU1862	잔량처리 하시겠습니까?
            Util.MessageConfirm("SFU1862", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SetRemain();
                }
            });
        }

        /// <summary>
        /// 투입대차 - 투입취소
        /// </summary>
        private void btnCancelInput_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_REG_CANCEL_INPUT_CTNR_NJ
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                // SFU1988  투입취소 하시겠습니까? 
                Util.MessageConfirm("SFU1988", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string sPalletID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                        inPalletTable.Columns.Add("LANGID");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");

                        DataTable inBoxTable = indataSet.Tables.Add("INCTNR");
                        inBoxTable.Columns.Add("CTNR_ID");
                        inBoxTable.Columns.Add("LOTID");

                        DataRow newRow = inPalletTable.NewRow();
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = sPalletID;
                        newRow["USERID"] = txtWorker_Main.Tag;

                        inPalletTable.Rows.Add(newRow);

                        foreach (int idxBox in idxBoxList)
                        {
                            int iWipQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["WIPQTY"].Index).Value);
                            int iRestQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["RESTQTY"].Index).Value);

                            newRow = inBoxTable.NewRow();
                            newRow["CTNR_ID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["CTNR_ID"].Index).Value);
                            newRow["LOTID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["LOTID"].Index).Value);
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_INPUT_CTNR_FOR_12ND_NJ", "INDATA,INCTNR", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                // SFU1275 정상처리되었습니다.
                                Util.MessageInfo("SFU1275");

                                GetPalletList();
                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletID, true);
                                GetDetailInfo();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            { }
        }

        #endregion

        #region Inbox


        /// <summary>
        /// Inbox
        /// </summary>
        private void txtBoxId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                    {
                        //SFU1843	작업자를 입력 해 주세요.
                        Util.MessageValidation("SFU1843");
                        return;
                    }

                    int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                    if (idxPallet < 0)
                    {
                        //SFU1645 선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    // 라벨 발행 가능 여부 체크
                    if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                    {
                        return;
                    }

                    PrintOutBox();

                    //// 전자저울 연결
                    //if (chkAuto.IsChecked == true)
                    //{
                    //    txtBoxId.SelectAll();
                    //    txtBoxWeight.Text = string.Empty;
                    //    FrameOperation.SendScanData("P");
                    //}
                    //else
                    //{
                    //    if (string.IsNullOrWhiteSpace(txtBoxId.Text))
                    //    {
                    //        //SFU4391		BoxID를 입력하세요.
                    //        Util.MessageValidation("SFU4391");
                    //        return;
                    //    }

                    //    if (string.IsNullOrWhiteSpace(txtBoxWeight.Text)
                    //        || Util.NVC_Decimal(txtBoxWeight.Text) <= 0)
                    //    {
                    //        //SFU4390		포장 중량을 입력하세요.	
                    //        Util.MessageValidation("SFU4390");
                    //        return;
                    //    }

                    //    PrintOutBox();
                    //}
                }
                catch (Exception ex)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    Util.MessageException(ex);
                }
            }
        }


        /// <summary>
        /// Inbox 셀등록
        /// </summary>
        private void btnRegCell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                int idxBoxList = _util.GetDataGridCheckFirstRowIndex(dgInbox, "CHK");

                //List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                //if (idxBoxList.Count <= 0)
                //{
                //    //SFU1645 선택된 작업대상이 없습니다.
                //    Util.MessageValidation("SFU1645");
                //    return;
                //}

                //if (idxBoxList.Count >1)
                //{                    
                //    Util.MessageValidation("하나의 박스만 선택해주세요.");
                //    return;
                //}

                BOX001_201_CELL_DETL puCellDetl = new BOX001_201_CELL_DETL();
                puCellDetl.FrameOperation = FrameOperation;

                if (puCellDetl != null)
                {
                    string sBoxID = idxBoxList < 0 ? string.Empty : Util.NVC(dgInbox.GetCell(idxBoxList, dgInbox.Columns["BOXID"].Index).Value);
                    string sPalletID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                    string sAommGrade = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["AOMM_GRD_CODE"].Index).Value);

                    object[] Parameters = new object[6];
                    Parameters[0] = sBoxID;
                    Parameters[1] = txtWorker_Main.Tag;
                    Parameters[2] = sPalletID;
                    Parameters[3] = sAommGrade;

                    C1WindowExtension.SetParameters(puCellDetl, Parameters);

                    puCellDetl.Closed += new EventHandler(puCellDetl_Closed);
                    // 팝업 화면 숨겨지는 문제 수정.
                    //this.Dispatcher.BeginInvoke(new Action(() => wndRun.ShowModal()));                    
                    grdMain.Children.Add(puCellDetl);
                    puCellDetl.BringToFront();
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

        private void puCellDetl_Closed(object sender, EventArgs e)
        {
            BOX001_201_CELL_DETL popup = sender as BOX001_201_CELL_DETL;

            //int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");
            //string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            //GetPalletList();

            ////미 선택 후 팝업 작업하는 경우가 있음.
            //if (idxPallet >= 0)
            //{               
            //    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
            //    GetDetailInfo();
            //}
            this.grdMain.Children.Remove(popup);
        }

        /// <summary>
        /// Inbox 재발행
        /// </summary>
        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                return;

            List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");
            if (idxBoxList.Count <= 0)
            {
                Util.MessageValidation("SFU1651");
                return;
            }

            try
            {
                string sBizRule = "BR_PRD_GET_OUTBOX_REPRT_NJ";

                DataSet ds = new DataSet();
                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("LANGID");
                dtInData.Columns.Add("USERID");
                dtInData.Columns.Add("PGM_ID");    //라벨 이력 저장용
                dtInData.Columns.Add("BZRULE_ID"); //라벨 이력 저장용

                DataRow dr = dtInData.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                dr["PGM_ID"] = _sPGM_ID;
                dr["BZRULE_ID"] = sBizRule;
                dtInData.Rows.Add(dr);

                DataTable dtInbox = ds.Tables.Add("INBOX");
                dtInbox.Columns.Add("BOXID");

                foreach (int idxBox in idxBoxList)
                {
                    string boxID = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index).Value);

                    dr = dtInbox.NewRow();
                    dr["BOXID"] = boxID;
                    dtInbox.Rows.Add(dr);
                }

                DataTable dtInPrint = ds.Tables.Add("INPRINT");
                dtInPrint.Columns.Add("PRMK");
                dtInPrint.Columns.Add("RESO");
                dtInPrint.Columns.Add("PRCN");
                dtInPrint.Columns.Add("MARH");
                dtInPrint.Columns.Add("MARV");
                dtInPrint.Columns.Add("DARK");
                dr = dtInPrint.NewRow();
                dr["PRMK"] = _sPrt;
                dr["RESO"] = _sRes;
                dr["PRCN"] = _sCopy;
                dr["MARH"] = _sXpos;
                dr["MARV"] = _sYpos;
                dr["DARK"] = _sDark;
                dtInPrint.Rows.Add(dr);

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
                    PrintLabel(zplCode, _drPrtInfo);

                    txtBoxId.Focus();
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }

        }

        /// <summary>
        ///  Inbox 삭제
        /// </summary>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                {
                    //SFU1843	작업자를 입력 해 주세요.
                    Util.MessageValidation("SFU1843");
                    return;
                }

                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInbox, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU3543	삭제할 데이터가 없습니다.
                    Util.MessageValidation("SFU3543");
                    return;
                }

                if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
                {
                    return;
                }

                if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) == "PACKED")
                {
                    //SFU3610	이미 포장 완료 됐습니다.[BOX의 정보 확인]	
                    Util.MessageValidation("SFU3610");
                    return;
                }

                //SFU1230	삭제 하시겠습니까?	
                Util.MessageConfirm("SFU1230", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");

                        DataRow newRow = inPalletTable.NewRow();
                        newRow["BOXID"] = sPalletId;
                        newRow["USERID"] = txtWorker_Main.Tag;
                        inPalletTable.Rows.Add(newRow);

                        DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                        inBoxTable.Columns.Add("BOXID");

                        foreach (int idxBox in idxBoxList)
                        {
                            string sBoxId = Util.NVC(dgInbox.GetCell(idxBox, dgInbox.Columns["BOXID"].Index).Value);

                            newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = sBoxId;
                            inBoxTable.Rows.Add(newRow);
                        }

                        new ClientProxy().ExecuteService_Multi("BR_PRD_DEL_OUTBOX_FOR_12ND_PLT_2D_NJ", "INDATA,INBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                GetPalletList();

                                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                                GetDetailInfo();

                                txtBoxId.Text = string.Empty;
                                txtBoxId.Focus();

                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }

                        }, indataSet);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgInbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInbox.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgInbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgInbox.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void dgInbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            e.Column.HeaderPresenter.Content = pre;
                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetPalletList()
        {
            try
            {
                if (cboEquipment_Search.SelectedIndex < 0 || cboEquipment_Search.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    // SFU1153 설비를 선택하세요
                    Util.MessageValidation("SFU1153");
                    return;
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID");
                RQSTDT.Columns.Add("AREAID");
                RQSTDT.Columns.Add("EQSGID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("PKG_LOTID");
                RQSTDT.Columns.Add("BOXSTAT_LIST");
                RQSTDT.Columns.Add("EQPTID");

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; 
                dr["EQSGID"] = Util.NVC(cboLine.SelectedValue);
                dr["EQPTID"] = Util.NVC(cboEquipment_Search.SelectedValue);

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                    dr["BOXID"] = txtPalletID.Text;
                else
                {
                    dr["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtAssyLotID.Text) ? null : txtAssyLotID.Text;
                    dr["BOXSTAT_LIST"] = string.IsNullOrEmpty(_searchStat) ? _searchStat : _searchStat.Remove(_searchStat.Length - 1);
                }

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_2ND_PALLET_LIST_2D_NJ", "INDATA", "OUTDATA", RQSTDT);
                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgPalletList, RSLTDT, FrameOperation, true);
                if (RSLTDT.Rows.Count == 1)
                {
                    _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", (string)RSLTDT.Rows[0]["BOXID"], true);
                    GetDetailInfo();
                }

                if (dgPalletList.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["INPUT_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 포장상세 - 설비조회
        /// </summary>
        private void setEquipmentCombo(C1ComboBox cbo, CommonCombo.ComboStatus cs, string eqsgID = null, string eqptID = null)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_BY_EQGRID_CBO_NJ";
            string[] arrColumn = { "LANGID", "EQSGID", "PROCID", "EQGRID" };
            string[] arrCondition = { LoginInfo.LANGID, eqsgID, Process.CELL_BOXING, "BOX" };
            string selectedValueText = "CBO_CODE";
            string displayMemberText = "CBO_NAME";

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, cs, selectedValueText, displayMemberText, eqptID);
        }

        /// <summary>
        /// 포장상세 - 투입대차 , 투입INBOX , 완성 OUTBOX 조회
        /// </summary>
        private void GetDetailInfo()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                chkAll.IsChecked = false;

                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID");
                inDataTable.Columns.Add("BOXID");

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = sPalletId;
                inDataTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_12ND_DETAIL_FOR_2D_NJ", "INDATA", "OUTDATA,OUTCTNR,OUTINPUTBOX,OUTINBOX", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult.Tables.Contains("OUTDATA"))
                        {
                            DataTable dtInfo = bizResult.Tables["OUTDATA"];

                            //  SFU2051 중복 데이터가 존재 합니다. % 1
                            if (dtInfo.Rows.Count > 1)
                            {
                                Util.MessageValidation("SFU2051", new object[] { sPalletId });
                                return;
                            }

                            chkAll.IsChecked = false;

                            string sProdId = Util.NVC(dtInfo.Rows[0]["PRODID"]);
                            string sProject = Util.NVC(dtInfo.Rows[0]["PROJECT"]);
                            string sEqptId = Util.NVC(dtInfo.Rows[0]["EQPTID"]);
                            string sLabelType = Util.NVC(dtInfo.Rows[0]["LABEL_ID"]);
                            string sPrdtGrd = Util.NVC(dtInfo.Rows[0]["PRDT_GRD_CODE"]);
                            string sLotId = Util.NVC(dtInfo.Rows[0]["PKG_LOTID"]);
                            string sNote = Util.NVC(dtInfo.Rows[0]["PACK_NOTE"]);
                            string sExpDom = Util.NVC(dtInfo.Rows[0]["EXP_DOM_TYPE_CODE"]);
                            string sProcID = Util.NVC(dtInfo.Rows[0]["PROCID"]);
                            int iWipQty = Util.NVC_Int(dtInfo.Rows[0]["WIPQTY"]);
                            int iCellQTy = Util.NVC_Int(dtInfo.Rows[0]["CELL_QTY"]);

                            txtCellQty.Text = iCellQTy.ToString();
                            cboProcType.SelectedValue = string.IsNullOrEmpty(sProcID) ? "SELECT" : sProcID;
                            txtPkgLotID.Text = sLotId;
                            setEquipmentCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sEqptId);
                            cboExpDomType.SelectedValue = string.IsNullOrEmpty(sExpDom) ? "SELECT" : sExpDom;
                            cboLabelType.SelectedValue = string.IsNullOrEmpty(sLabelType) ? "SELECT" : sLabelType;
                            txtPrdGrade.Text = sPrdtGrd;
                            txtPRODID.Text = sProdId;
                            txtInputQty.Value = iWipQty;
                            new TextRange(txtNote.Document.ContentStart, txtNote.Document.ContentEnd).Text = sNote;
                        }

                        SetAommGrdVisibility(txtPRODID.Text);

                        if (bizResult.Tables.Contains("OUTCTNR"))
                        {
                            DataTable dtCtnr = bizResult.Tables["OUTCTNR"];
                            if (!dtCtnr.Columns.Contains("CHK"))
                                dtCtnr.Columns.Add("CHK");

                            if (!dtCtnr.Columns.Contains("RESTQTY"))
                            {
                                DataColumn dc = new DataColumn("RESTQTY");
                                dc.DefaultValue = 0;
                                dtCtnr.Columns.Add(dc);
                            }

                            Util.GridSetData(dgInTray, dtCtnr, FrameOperation, true);

                            if (dgInTray.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInTray.Columns["WIPQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInTray.Columns["RESTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
                        }

                        if (bizResult.Tables.Contains("OUTINBOX"))
                        {
                            DataTable dtInbox = bizResult.Tables["OUTINBOX"];
                            if (!dtInbox.Columns.Contains("CHK"))
                                dtInbox.Columns.Add("CHK");

                            int totalCellQty = 0;

                            for (int i = 0; i < dtInbox.Rows.Count; i++)
                            {
                                totalCellQty += dtInbox.Rows[i]["CELL_IN_QTY"].ToString().SafeToInt32();
                            }

                            Util.GridSetData(dgInbox, dtInbox, FrameOperation, true);
                            if (dgInbox.Rows.Count > 0)
                            {
                                DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                                DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                                DataGridAggregate.SetAggregateFunctions(dgInbox.Columns["CELL_IN_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                            }
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

                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 투입대차 - 잔량처리
        /// </summary>
        private void SetRemain()
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string sPalletID = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("BOXID");
                inPalletTable.Columns.Add("USERID");

                DataTable inBoxTable = indataSet.Tables.Add("INCTNR");
                inBoxTable.Columns.Add("CTNR_ID");
                inBoxTable.Columns.Add("LOTID");
                inBoxTable.Columns.Add("WIPQTY");
                inBoxTable.Columns.Add("RESTQTY");
                if (_AommGrdChkFlag == true)
                {
                    inBoxTable.Columns.Add("AOMM_GRD_CODE");
                }

                DataRow newRow = inPalletTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["BOXID"] = sPalletID;
                newRow["USERID"] = txtWorker_Main.Tag;

                inPalletTable.Rows.Add(newRow);

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgInTray, "CHK");

                if (idxBoxList.Count <= 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                foreach (int idxBox in idxBoxList)
                {
                    int iWipQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["WIPQTY"].Index).Value);
                    int iRestQty = Util.NVC_Int(dgInTray.GetCell(idxBox, dgInTray.Columns["RESTQTY"].Index).Value);

                    newRow = inBoxTable.NewRow();
                    newRow["CTNR_ID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["CTNR_ID"].Index).Value);
                    newRow["LOTID"] = Util.NVC(dgInTray.GetCell(idxBox, dgInTray.Columns["LOTID"].Index).Value);
                    newRow["WIPQTY"] = iWipQty;
                    newRow["RESTQTY"] = iRestQty;
                    if (_AommGrdChkFlag == true)
                    {
                        newRow["AOMM_GRD_CODE"] = Util.NVC(cboAommType.SelectedValue);
                    }
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REM_INPUT_CTNR_FOR_2ND_PALLET_NJ", "INDATA,INCTNR", "OUTCTNR,OUTLOT", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        // SFU1275 정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                        GetPalletList();
                        _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletID, true);
                        GetDetailInfo();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            { }
        }

        //ESNJ 실제 OUTBOX 수량 저장
        private void ModifyRealQty()
        {
            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            //저장하시겠습니까?
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("BOXID");
                        dt.Columns.Add("USERID");
                        dt.Columns.Add("REAL_QTY");
                        DataRow dr = dt.NewRow();
                        dr["BOXID"] = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);
                        dr["USERID"] = LoginInfo.USERID;
                        dr["REAL_QTY"] = txtRealQty.Text;
                        dt.Rows.Add(dr);
                        new ClientProxy().ExecuteService("BR_PRD_REG_REAL_QTY", "INDATA", null, dt, (bizResult, bizException) =>
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            //정상 처리 되었습니다.
                            Util.MessageInfo("SFU1275");
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        #endregion

        #region [Validation]
        private bool ValidationOutInbox()
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return false;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return false;
            }

            // 라벨 발행 가능 여부 체크
            if (!_util.GetConfigPrintInfo(out _sPrt, out _sRes, out _sCopy, out _sXpos, out _sYpos, out _sDark, out _drPrtInfo))
            {
                return false;
            }

            PrintOutBox();

            //// 전자저울 연결
            //if (chkAuto.IsChecked == true)
            //{
            //    txtBoxId.SelectAll();
            //    txtBoxWeight.Text = string.Empty;
            //    FrameOperation.SendScanData("P");
            //}
            //else
            //{
            //    if (string.IsNullOrWhiteSpace(txtBoxId.Text))
            //    {
            //        //SFU4391		BoxID를 입력하세요.
            //        Util.MessageValidation("SFU4391");
            //        return false;
            //    }


            //    if (string.IsNullOrWhiteSpace(txtBoxWeight.Text)
            //        || Util.NVC_Decimal(txtBoxWeight.Text) <= 0)
            //    {
            //        //SFU4390		포장 중량을 입력하세요.	
            //        Util.MessageValidation("SFU4390");
            //        return false;
            //    }

            //    PrintOutBox();
            //}

            return true;
        }


        #endregion

        #region [Func]
        private void TagPrint()
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                //SFU1843	작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

            if (idxPallet < 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            if (Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXSTAT"].Index).Value) != "PACKED")
            {
                //SFU4262		실적 확정후 작업 가능합니다.	
                Util.MessageValidation("SFU4262");
                return;
            }

            string sPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

            Report_2nd_Boxing popup = new Report_2nd_Boxing();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(confirmPopup_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void confirmPopup_Closed(object sender, EventArgs e)
        {
            Report_2nd_Boxing popup = sender as Report_2nd_Boxing;
            if (popup != null)
            {
                string sPalletId = popup.PALLET_ID;

                GetPalletList();
                _util.SetDataGridCheck(dgPalletList, "CHK", "BOXID", sPalletId, true);
                // GetDetailInfo();
            }
        }

        private void SetDetailClear()
        {
            //txtBoxWeight.Text = string.Empty;
            txtStartPalletID.Text = string.Empty;
            Util.gridClear(dgInTray);
            Util.gridClear(dgInbox);
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

        #endregion

        #endregion

        private void btnCancelinbox_Click(object sender, RoutedEventArgs e)
        {

            BOX001_231_UNPACK popup = new BOX001_231_UNPACK();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.CFG_AREA_ID;

                C1WindowExtension.SetParameters(popup, Parameters);
                popup.Closed += new EventHandler(puUnpack_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
            else
            {
                //Message: 팔레트 구성 정보가 없습니다.
            }
        }
        private void puUnpack_Closed(object sender, EventArgs e)
        {
            BOX001_231_UNPACK popup = sender as BOX001_231_UNPACK;

            if (popup.DialogResult == MessageBoxResult.OK)
            {

            }
            this.grdMain.Children.Remove(popup);
        }

        private void cboLine_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            setEquipmentCombo(cboEquipment_Search, CommonCombo.ComboStatus.SELECT, (string)cboLine.SelectedValue, LoginInfo.CFG_EQPT_ID);
        }

        private void cboEquipment_Search_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetEquipmentShipTo();

            // GetPalletList();
        }

        private void btnShiptoSet_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment_Search.SelectedIndex == 0)
            {
                // "설비를 선택 하세요."
                Util.MessageValidation("SFU1673");
                return;
            }
            SettingShipTo();
        }

        private void btnCellPassFCS_Click(object sender, RoutedEventArgs e)
        {
            //CMM_BOX_FORM_CELL_PASS_FCS popup = new CMM_BOX_FORM_CELL_PASS_FCS();
            //popup.FrameOperation = this.FrameOperation;
            CMM_BOX_FORM_CELL_PASS_FCS popup = new CMM_BOX_FORM_CELL_PASS_FCS { FrameOperation = FrameOperation };

            if (ValidationGridAdd(popup.Name) == false)
                return;

            if (popup != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = LoginInfo.USERID;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puCellPassFCS_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puCellPassFCS_Closed(object sender, EventArgs e)
        {
            CMM_BOX_FORM_CELL_PASS_FCS popup = sender as CMM_BOX_FORM_CELL_PASS_FCS;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
            }
            grdMain.Children.Remove(popup);
        }

        private void SettingShipTo()
        {
            try
            {
                BOX001_235_SHIPTO_SETTING PopShipTo = new BOX001_235_SHIPTO_SETTING();
                PopShipTo.FrameOperation = FrameOperation;

                if (PopShipTo != null)
                {
                    object[] Parameters = new object[5];
                    Parameters[0] = cboLine.SelectedValue.ToString();// 라인정보
                    Parameters[1] = cboEquipment_Search.SelectedValue.ToString(); // 설비정보
                    Parameters[2] = txtShipto.Text.ToString();// 출하처
                    Parameters[3] = txtLabelType.Text.ToString();// 라벨 타입
                    Parameters[4] = txtFIFO.Text.ToString();

                    C1WindowExtension.SetParameters(PopShipTo, Parameters);

                    PopShipTo.Closed += new EventHandler(PopShipTo_Closed);
                    grdMain.Children.Add(PopShipTo);
                    PopShipTo.BringToFront();
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
        private void PopShipTo_Closed(object sender, EventArgs e)
        {
            BOX001_235_SHIPTO_SETTING window = sender as BOX001_235_SHIPTO_SETTING;

            if (window.DialogResult == MessageBoxResult.OK)
            {
                GetEquipmentShipTo();
            }

            this.grdMain.Children.Remove(window);
        }

        #region 설비별 출하처, 라벨타입 조회 : GetEquipmentShipTo()
        public void GetEquipmentShipTo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQUIPMENT", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQUIPMENT"] = cboEquipment_Search.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQUIPMENT_SHIP_TO", "INDATA", "OUTDATA", inTable);


                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    txtShipto.Text = dtResult.Rows[0]["BOXING_SHIPTO_NAME"].ToString();
                    txtShipto.Tag = dtResult.Rows[0]["BOXING_SHIPTO_ID"].ToString();
                    txtLabelType.Text = dtResult.Rows[0]["BOXING_LABEL_NAME"].ToString();
                    txtFIFO.Text = dtResult.Rows[0]["BOXING_FIFO_FLAG"].ToString();
                }
                else
                {
                    txtShipto.Text = string.Empty;
                    txtLabelType.Text = string.Empty;
                    txtShipto.Tag = string.Empty;
                    txtFIFO.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void btnTakeOver_Click(object sender, RoutedEventArgs e)
        {
            CMM_POLYMER_FORM_CART_TAKEOVER popupTakeOver = new CMM_POLYMER_FORM_CART_TAKEOVER { FrameOperation = FrameOperation };
            if (ValidationGridAdd(popupTakeOver.Name) == false)
                return;

            object[] parameters = new object[5];
            parameters[0] = Process.CELL_BOXING;
            parameters[1] = _processName;
            parameters[2] = string.Empty;
            parameters[3] = string.Empty;
            parameters[4] = string.Empty;
            C1WindowExtension.SetParameters(popupTakeOver, parameters);

            popupTakeOver.Closed += popupTakeOver_Closed;
            grdMain.Children.Add(popupTakeOver);
            popupTakeOver.BringToFront();
        }

        private void popupTakeOver_Closed(object sender, EventArgs e)
        {
            CMM_POLYMER_FORM_CART_TAKEOVER popup = sender as CMM_POLYMER_FORM_CART_TAKEOVER;

            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {

            }

            grdMain.Children.Remove(popup);
        }

        private bool ValidationGridAdd(string popName)
        {
            foreach (UIElement ui in grdMain.Children)
            {
                if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
                {
                    // 프로그램이 이미 실행 중 입니다. 
                    Util.MessageValidation("SFU3193");
                    return false;
                }
            }
            return true;
        }

        private void SelectProcessName()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.CELL_BOXING;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_FO", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    _processName = dtResult.Rows[0]["PROCNAME"].ToString();
                else
                    _processName = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #region [E20230424-000073 : 포장해체 팝업 호출]
        private void btnBoxRelease_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                // 작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (cboLine.SelectedIndex < 0 || cboLine.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //SFU1223 라인을 선택 하세요
                Util.MessageValidation("SFU1223");
                return;
            }

            BOX001_240_UNPACK popup = new BOX001_240_UNPACK();
            popup.FrameOperation = this.FrameOperation;

            if (popup != null)
            {
                object[] Parameters = new object[2];

                Parameters[0] = txtWorker_Main.Tag; // 작업자id
                Parameters[1] = _sPGM_ID; // 자동포장(파우치)


                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(puUnpackBox_Closed);
                grdMain.Children.Add(popup);
                popup.BringToFront();
            }
        }

        private void puUnpackBox_Closed(object sender, EventArgs e)
        {
            BOX001_240_UNPACK popup = sender as BOX001_240_UNPACK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
            }
            this.grdMain.Children.Remove(popup);
        }
        #endregion
    }
}
