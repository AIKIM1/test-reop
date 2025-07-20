/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using System;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;


namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_007 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        DataTable dtPrintHistory;
        System.ComponentModel.BackgroundWorker bkWorker;
        String sBoxid = "";
        string iPrnCount = string.Empty;
        bool testYn = false;
        string zpl = string.Empty;

        private bool blPrintStop = true;

        public PACK001_007()
        {
            InitializeComponent();

            this.Loaded += PACK001_007_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void Initialize()
        {            
            InitCombo();
        }
        #endregion

        #region Event
        private void PACK001_007_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();

            this.Loaded -= PACK001_007_Loaded;

            bkWorker = new System.ComponentModel.BackgroundWorker();
            bkWorker.WorkerSupportsCancellation = true;
            bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
            bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

            //프린터 초기값 설정
            string Init_port = string.Empty;
            foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
            {
                if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT]))
                {                   
                    Init_port = dr["PORTNAME"].ToString();
                }
            }

            if (Init_port != null || Init_port != "")
            {
                cboPrintConnet.SelectedValue = Init_port;
            }

            cboPrintConnet.SelectedValueChanged += cboPrintConnet_SelectedValueChanged;            
            cboLabelName.SelectedValueChanged += CboLabelName_SelectedValueChanged;
        }

       

        private void cboProcess_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                CommonCombo _combo = new CMM001.Class.CommonCombo();

                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAreaByAreaType = new C1ComboBox();
                cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
                C1ComboBox cboProductModel = new C1ComboBox();
                cboProductModel.SelectedValue = "";

                string prdclass = string.Empty;

                if (Util.GetCondition(cboProcessPack) == "P5500")
                {
                    prdclass = "CMA";
                }
                else if (Util.GetCondition(cboProcessPack) == "P9500")
                {
                    prdclass = "BMA";
                }
                else
                {
                    prdclass = "";
                }

                C1ComboBox cboPrdtClass = new C1ComboBox();
                cboPrdtClass.SelectedValue = prdclass;

                //제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboPrdtClass };
                if(cboEquipmentSegment.SelectedValue.ToString() == "P2Q02")
                {
                    _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sCase: "PRJ_PRODUCT_PACK");
                }else
                {
                    _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");
                }
                
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }

        private void cboPrintConnet_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboPrintConnet == null)
            {
                return;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count == 0)
            {               
                ms.AlertWarning("SFU1729"); //연결된 PRINTER가 없습니다.
                return;
            }
            else
            {
                string saveed_port = string.Empty;
                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (cboPrintConnet.SelectedValue.ToString() == dr["PORTNAME"].ToString())
                    {
                        saveed_port = "OK";
                    }
                }

                if (saveed_port == null || saveed_port == "")
                {
                   // Util.AlertInfo(cboPrintConnet.SelectedValue + " : 설정값 없음");
                    ms.AlertWarning("SFU1679"); //설정값 없음(Configration 설정 후 사용하세요)
                    return;
                }
            }
        }

        private void btnTestOut_Click(object sender, RoutedEventArgs e)
        {
            testYn = true;         

            try
            {
                if(cboLabelName.SelectedIndex ==0)
                {                    
                    ms.AlertWarning("SFU1523"); // 라벨명을 선택하세요.
                    return;
                }

                BarcodePrint(testYn, "output_yes");              
            }
            catch (Exception ex)
            {
                //Util.AlertInfo(ex.Message);
                Util.MessageException(ex);
            }
        }     

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                testYn = false;

                if (btnPrint.Content.ToString() == ObjectDic.Instance.GetObjectName("취소"))
                {
                    bkWorker.Dispose();
                    blPrintStop = true;
                    btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
                    return;
                }

                if (cboLabelName.SelectedIndex < 1)
                {
                    ms.AlertWarning("SFU1522"); //라벨 타입을 선택하세요
                    cboLabelName.Focus();
                    return;
                }

                if (cboLabelVersion.Text == "")
                {
                    ms.AlertWarning("SFU1521"); //라벨 버전을 선택하세요
                    cboLabelVersion.Focus();
                    return;
                }
                
                if (String.IsNullOrEmpty(cboEquipmentSegment.SelectedValue.ToString()) || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
                {
                    ms.AlertWarning("MMD0047"); //라인을 선택해 주세요.
                    cboEquipmentSegment.Focus();
                    return;
                }

                if(String.IsNullOrEmpty(cboProcessPack.SelectedValue.ToString()) || cboProcessPack.SelectedValue.ToString().Equals("SELECT"))
                {
                    ms.AlertWarning("MMD0005"); //공정을 선택해 주세요.
                    cboProcessPack.Focus();
                    return;
                }

               
                if (String.IsNullOrEmpty(cboProduct.SelectedValue.ToString()) || cboProduct.SelectedValue.ToString().Equals("SELECT"))
                {
                    ms.AlertWarning("10054"); //제품을 선택하세요.
                    cboProduct.Focus();
                    return;
                }

                if(nbProductBox.Value <=0)
                {
                    ms.AlertWarning("SFU2005"); //한 BOX당 포장될 수량을 입력해주세요.
                    nbProductBox.Focus();
                    return;
                }

                if (nbSeq.Value <= 0)
                {
                    ms.AlertWarning("SFU1300"); //1개 이상의 출력 수량을 입력하세요.
                    nbSeq.Focus();
                    return;
                }

                PrintProcess();
            }
            catch(Exception ex)
            {
                bkWorker.Dispose();
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnAceept_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string set_port = string.Empty;
                foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                {
                    if (cboPrintConnet.SelectedValue.ToString().Contains(dr["PORTNAME"].ToString()))
                    {
                        dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT] = true;
                        set_port = dr["PORTNAME"].ToString();
                    }
                    else
                    {
                        dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT] = false;
                    }
                }

                if (set_port == null || set_port == "")
                {
                    ms.AlertWarning("SFU1679"); //설정값 없음(Configration 설정 후 사용하세요)                 
                    return;
                }

                //Util.AlertInfo("PRINTER 설정이 완료 되었습니다.(" + set_port + ")");
                ms.AlertWarning("SFU1420"); 
            }
            catch(Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
            
        }

        private void btnExpandFrame_Checked(object sender, RoutedEventArgs e)
        {
            content_grid.ColumnDefinitions[0].Width = new GridLength(8);
            content_grid.ColumnDefinitions[2].Width = new GridLength(8);
            content_grid.ColumnDefinitions[3].Width = new GridLength(8);
            content_grid.ColumnDefinitions[4].Width = new GridLength(8);
            content_grid.ColumnDefinitions[6].Width = new GridLength(8);
            content_grid.ColumnDefinitions[7].Width = new GridLength(8);
            content_grid.ColumnDefinitions[8].Width = new GridLength(8);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla_main = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            LGC.GMES.MES.CMM001.GridLengthAnimation gla_title = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            LGC.GMES.MES.CMM001.GridLengthAnimation gla_content = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla_main.From = new GridLength(0, GridUnitType.Star);
            gla_main.To = new GridLength(3, GridUnitType.Star);
            gla_main.Duration = new TimeSpan(0, 0, 0, 0, 500);
            gla_title.To = new GridLength(0.2, GridUnitType.Star);
            gla_title.Duration = new TimeSpan(0, 0, 0, 0, 500);
            gla_content.From = new GridLength(0, GridUnitType.Star);
            gla_content.To = new GridLength(1, GridUnitType.Star);
            gla_content.Duration = new TimeSpan(0, 0, 0, 0, 500);

            main_grid.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla_main);
            title_grid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla_title);
            content_grid.ColumnDefinitions[1].BeginAnimation(ColumnDefinition.WidthProperty, gla_content);
        }

        private void btnExpandFrame_Unchecked(object sender, RoutedEventArgs e)
        {
            content_grid.ColumnDefinitions[0].Width = new GridLength(0);
            content_grid.ColumnDefinitions[2].Width = new GridLength(0);
            content_grid.ColumnDefinitions[3].Width = new GridLength(0);
            content_grid.ColumnDefinitions[4].Width = new GridLength(0);
            content_grid.ColumnDefinitions[6].Width = new GridLength(0);
            content_grid.ColumnDefinitions[7].Width = new GridLength(0);
            content_grid.ColumnDefinitions[8].Width = new GridLength(0);

            LGC.GMES.MES.CMM001.GridLengthAnimation gla_main = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            LGC.GMES.MES.CMM001.GridLengthAnimation gla_title = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            LGC.GMES.MES.CMM001.GridLengthAnimation gla_content = new LGC.GMES.MES.CMM001.GridLengthAnimation();
            gla_main.From = new GridLength(1, GridUnitType.Star);
            gla_main.To = new GridLength(0, GridUnitType.Star);
            gla_main.Duration = new TimeSpan(0, 0, 0, 0, 500);
            gla_title.From = new GridLength(1, GridUnitType.Star);
            gla_title.To = new GridLength(0, GridUnitType.Star);
            gla_title.Duration = new TimeSpan(0, 0, 0, 0, 500);
            gla_content.From = new GridLength(0, GridUnitType.Star);
            gla_content.To = new GridLength(100, GridUnitType.Star);
            main_grid.RowDefinitions[1].BeginAnimation(RowDefinition.HeightProperty, gla_main);
            title_grid.RowDefinitions[0].BeginAnimation(RowDefinition.HeightProperty, gla_title);
            content_grid.ColumnDefinitions[1].BeginAnimation(ColumnDefinition.WidthProperty, gla_content);
        }

        private void zplBrowser_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (zpl.Length > 0)
            {
                if (Convert.ToBoolean(btnExpandFrame.IsChecked))
                {
                    btnExpandFrame_Unchecked(null, null);
                }
                else
                {
                    btnExpandFrame_Checked(null, null);
                }
            }
        }

        private void zplBrowser_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (zpl.Length > 0)
            {
                zplBrowser.ToolTip = zpl;
            }
        }

        private void browser_grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (zpl.Length > 0)
            {
                if (Convert.ToBoolean(btnExpandFrame.IsChecked))
                {
                    btnExpandFrame_Unchecked(null, null);
                }
                else
                {
                    btnExpandFrame_Checked(null, null);
                }
            }
        }

        private void browser_grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (zpl.Length > 0)
            {
                zplBrowser.ToolTip = zpl;
            }
        }
      

        private void CboLabelName_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            testYn = true;

            try
            {
                if (cboLabelName.SelectedIndex == 0)
                {                 
                    ms.AlertWarning("SFU1523"); //라벨을 선택 하세요.
                    return;
                }

                BarcodePrint(testYn, "output_no");
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }
        #endregion

        #region Mehod
        private void InitCombo()
        {
            CommonCombo _combo = new CMM001.Class.CommonCombo();

            #region 라벨선택 영역 콤보
            //Label Type            
            _combo.SetCombo(cboLabelName, CommonCombo.ComboStatus.SELECT);
            
            //Version    
            C1ComboBox[] cboLabelNameParent = { cboLabelName };
            _combo.SetCombo(cboLabelVersion, CommonCombo.ComboStatus.NONE, cbParent: cboLabelNameParent);

            #endregion


            #region 출력데이터 입력
            //제품(PRODCUCT)
            //비즈 호출시 테이블에 데이터가 없어서 콤보가 나오지 않아서
            //임시로 cell product를 뿌려줌.
            C1ComboBox cboSHOPID = new C1ComboBox();
            cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
            C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
            cboAREA_TYPE_CODE.SelectedValue = Area_Type.PACK;
            C1ComboBox cboAreaByAreaType = new C1ComboBox();
            cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;
            //C1ComboBox cboEquipmentSegment = new C1ComboBox();
            //cboEquipmentSegment.SelectedValue = LoginInfo.CFG_EQSG_ID;
            C1ComboBox cboPrdtClass = new C1ComboBox();
            cboPrdtClass.SelectedValue = "";
            C1ComboBox cboProductModel = new C1ComboBox();
            cboProductModel.SelectedValue = "";

            //라인            
            C1ComboBox[] cboEquipmentSegmentParent = { cboAreaByAreaType };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcessPack }; //,cboReason];
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentSegmentParent, cbChild: cboEquipmentSegmentChild);

            //공정    
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            //C1ComboBox[] cboProcessChild = { cboReworkReturnProcess };
            _combo.SetCombo(cboProcessPack, CommonCombo.ComboStatus.NONE, cbParent: cboProcessParent, sCase : "BOX_PROCESS");

            //동           
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL);

            //모델          
            //C1ComboBox[] cboProductModelParent = { cboSHOPID, cboArea, cboEquipmentSegment };
            //C1ComboBox[] cboProductModelChild = { cboProduct };
            //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, cbChild: cboProductModelChild);

            //제품    
            //C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
            //_combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);
            //getProduct(cboProduct);           

            //제품
            C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboPrdtClass };
            if (cboEquipmentSegment.SelectedValue.ToString() == "P2Q02")
            {
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sCase: "PRJ_PRODUCT_PACK");
            }
            else
            {
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.SELECT, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");
            }

            /*       
            string[] model_ProdcutType = { LoginInfo.CFG_SHOP_ID
                                          ,LoginInfo.CFG_AREA_ID
                                          ,LoginInfo.CFG_EQSG_ID
                                          ,""
                                          ,"P"
                                          ,"CELL" };
            _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, sFilter: model_ProdcutType);
            */

            //동(AREA)            
            //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT);

            //작업조
            //string[] shift_param = { "A1"    // LoginInfo.CFG_AREA_ID
            //                        ,"E2D01" // LoginInfo.CFG_EQSG_ID 
            //                        ,"E1000" // LoginInfo.CFG_PROC_ID
            //                       };
            //_combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, sFilter: shift_param);

            //C1ComboBox cboAreaByAreaType = new C1ComboBox();
            //cboAreaByAreaType.SelectedValue = LoginInfo.CFG_AREA_ID;

            //작업조            
            C1ComboBox[] cboShiftParent = { cboAreaByAreaType };
            string[] shift_param = { null, "ALL" };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.SELECT, cbParent: cboShiftParent, sFilter: shift_param);
            #endregion

            #region 프린터 설정
            //Printer 연결
            string[] sFliter = { "CONNECTION_TYPE" };
            _combo.SetCombo(cboPrintConnet, CommonCombo.ComboStatus.NONE, sFilter: sFliter, sCase: "COMMCODE");
            #endregion

        }

        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {               
                Print();                
             }
            catch (Exception ex)
            {
                blPrintStop = true;
                bkWorker.CancelAsync();
              
            }
        }

        public static string GetCondition_Thread(object oCondition, string sMsg = "", bool bAllNull = false)
        {
            string sRet = "";
            int idx = 0;
            string type = "";
            switch (oCondition.GetType().Name)
            {
                case "LGCDatePicker":
                    LGCDatePicker lgcDp = oCondition as LGCDatePicker;
                    lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        type = lgcDp.DatepickerType.ToString();
                    }));
                 
                    if (type.Equals("Month"))
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMM");
                        }));
                    }
                    else
                    {
                        lgcDp.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            sRet = lgcDp.SelectedDateTime.ToString("yyyyMMdd");
                        }));                     
                    }
                    break;
                case "C1ComboBox":
                    C1ComboBox cb = oCondition as C1ComboBox;
                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        idx = cb.SelectedIndex;
                    }));

                    if (idx < 0)
                    {
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }

                    cb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = cb.SelectedValue.ToString();
                    }));
               
                    if (sRet.Equals("SELECT"))
                    {
                        sRet = "";
                        if (!sMsg.Equals(""))
                        {
                            Util.Alert(sMsg);
                        }
                        break;
                    }
                    else if (sRet.Equals(""))
                    {
                        if (bAllNull)
                        {
                            sRet = null;
                        }
                    }
                    break;
                case "TextBox":
                    TextBox tb = oCondition as TextBox;
                    tb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = tb.Text;
                    }));
                  
                    if (sRet.Equals("") && !sMsg.Equals(""))
                    {
                        Util.Alert(sMsg);
                        break;
                    }
                    break;
                case "C1NumericBox":
                    C1NumericBox nb = oCondition as C1NumericBox;
                    nb.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        sRet = nb.Value.ToString();
                    }));                  
                    break;
            }
            return sRet;
        }

        private void Print()
        {
            try
            {
                DataTable dtResult;

                string sLabelType = string.Empty;
                string sProdID = string.Empty;      //제품
                string sEqsgid = string.Empty;      //라인
                string sProcessID = string.Empty;   //공정
                string iQty = string.Empty; ;       // 제품수
                int gridCount = 0;                  //라벨발행 리스트 row수
                string sProdName = string.Empty;    //제품이름
                string sShift = string.Empty;       //작업조
                string sEqsgName = string.Empty;    //라이명

                iPrnCount = Util.GetCondition_Thread(nbSeq); //뱔행수
                sProdName = Util.GetCondition_Thread(cboProduct);
                sShift = Util.GetCondition_Thread(cboShift);
                cboLabelName.Dispatcher.BeginInvoke(new Action(() => sLabelType = cboLabelName.Text.Replace("LABEL", "").Trim())); 
                iQty = Util.GetCondition_Thread(nbProductBox); //제품수               
                dgPrintHistory.Dispatcher.BeginInvoke(new Action(() => gridCount = dgPrintHistory.GetRowCount()));
                sProdID = Util.GetCondition_Thread(cboProduct);
                sProdName = Util.GetCondition_Thread(cboProduct);
                sEqsgid = Util.GetCondition_Thread(cboEquipmentSegment);
                sProcessID = Util.GetCondition_Thread(cboProcessPack);                
                cboEquipmentSegment.Dispatcher.BeginInvoke(new Action(() => sEqsgName = cboEquipmentSegment.Text));

                //boxid 생성 로직
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SRCTYPE", typeof(string));  //INPUT TYPE (UI OR EQ)         
                RQSTDT.Columns.Add("LANGID", typeof(string));   //LANGUAGE ID         
                RQSTDT.Columns.Add("LOTID", typeof(string));    //투입LOT(처음 LOT)
                RQSTDT.Columns.Add("PROCID", typeof(string));   //공정ID(포장전 마지막 공정) 
                RQSTDT.Columns.Add("PRODID", typeof(string));   //제품
                RQSTDT.Columns.Add("LOTQTY", typeof(Int32));   //투입 총수량
                RQSTDT.Columns.Add("EQSGID", typeof(string));   //라인ID
                RQSTDT.Columns.Add("USERID", typeof(string));   //사용자ID

                DataRow dr = RQSTDT.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = "";
                dr["PROCID"] = sProcessID; 
                dr["PRODID"] = sProdID; 
                dr["LOTQTY"] = Convert.ToInt32(iQty);
                dr["EQSGID"] = sEqsgid;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                for (int i = 0; i < Convert.ToInt32(iPrnCount); i++)
                {
                    if (blPrintStop) break;

                    dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOXIDREQUEST", "INDATA", "OUTDATA", RQSTDT);

                    if (dtResult != null && dtResult.Rows.Count > 0)
                    {
                        sBoxid = dtResult.Rows[0][0].ToString();

                        Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                        {
                            BarcodePrint(false, "output_yes");
                        }));

                        
                       
                        System.Threading.Thread.Sleep(500);

                        PrintHistory_ADD(Util.GetCondition_Thread(nbProductBox), sProdName, sEqsgName, gridCount, sEqsgid, sProcessID);

                        gridCount++;
                    }
                }

                dtResult = null;
                sBoxid = "";
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                }));
                
            }
            catch (Exception ex)
            {

                blPrintStop = true;
                bkWorker.CancelAsync();

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    //ms.AlertWarning(ex.Message);
                    Util.MessageException(ex);
                }));
            }
        }

        private void PrintHistory_ADD(string boxing_cnt, string sProdName, string lineName, int grid_cnt, string lineID, string procid)
        {    
            try
            {
                if (grid_cnt == 0)
                {
                    DataTable dtBoxHistory = new DataTable();
                    dtBoxHistory.Columns.Add("CHK", typeof(bool));
                    dtBoxHistory.Columns.Add("BOXID", typeof(string));
                    dtBoxHistory.Columns.Add("PRODNAME", typeof(string));
                    dtBoxHistory.Columns.Add("LINE", typeof(string));
                    dtBoxHistory.Columns.Add("PROD_CNT", typeof(string));
                    dtBoxHistory.Columns.Add("DATE", typeof(string));

                    DataRow dr = dtBoxHistory.NewRow();
                    dr["CHK"] = false;
                    dr["BOXID"] = Util.NVC(sBoxid);
                    dr["PRODNAME"] = Util.NVC(sProdName);
                    dr["LINE"] = Util.NVC(lineName);
                    dr["PROD_CNT"] = Util.NVC(boxing_cnt);
                    dr["DATE"] = Util.NVC(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

                    dtBoxHistory.Rows.Add(dr);

                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        dgPrintHistory.ItemsSource = DataTableConverter.Convert(dtBoxHistory);
                    }));
                }
                else
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                    {
                        dgPrintHistory.EndNewRow(true);
                        DataGridRowAdd(dgPrintHistory);

                        DataTableConverter.SetValue(dgPrintHistory.Rows[grid_cnt].DataItem, "BOXID", Util.NVC(sBoxid));
                        DataTableConverter.SetValue(dgPrintHistory.Rows[grid_cnt].DataItem, "PRODNAME", Util.NVC(sProdName));
                        DataTableConverter.SetValue(dgPrintHistory.Rows[grid_cnt].DataItem, "LINE", Util.NVC(lineName));
                        DataTableConverter.SetValue(dgPrintHistory.Rows[grid_cnt].DataItem, "PROD_CNT", Util.NVC(boxing_cnt.ToString()));
                        DataTableConverter.SetValue(dgPrintHistory.Rows[grid_cnt].DataItem, "DATE", Util.NVC(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")));
                    }));
                }

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    Util.SetTextBlockText_DataGridRowCount(tbBoxPrintList_cnt, Util.NVC(grid_cnt + 1));
                }));

                //재발행 일 경우 처리 : 기존 발행 정보 확인
                DataTable dtBoxPrintHistory = setBoxResultList(lineID, procid);

                string print_cnt = string.Empty;
                string print_yn = string.Empty;

                if (dtBoxPrintHistory == null || dtBoxPrintHistory.Rows.Count == 0)
                {
                    return;
                }

                print_cnt = dtBoxPrintHistory.Rows[0]["PRT_REQ_SEQNO"].ToString();
                print_yn = dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString();

                if (dtBoxPrintHistory.Rows[0]["PRT_FLAG"].ToString() == "N")//print 여부 N인경우 Y로 update
                {
                    updateTB_SFC_LABEL_PRT_REQ_HIST(print_yn, print_cnt);
                }
            }   
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }            
        }

        private void DataGridRowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            dg.BeginNewRow();
            dg.EndNewRow(true);
        }

        private void BarcodePrint(bool isTest, string ouput_yn)
        {
            try
            {
                string I_ATTVAL = string.Empty;

                if (isTest)
                {
                    I_ATTVAL = labelItemsGet_test();
                    //System.Threading.Thread.Sleep((int)2 * 1000);

                    getZpl(I_ATTVAL);

                    if (ouput_yn == "output_yes")
                    {
                        Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                        ms.AlertInfo("SFU1933"); //출력이 종료 되었습니다 
                    }

                    textBox.Text = zpl;

                    if(!(LoginInfo.USERID.Trim() == "cnswkdakscjf"))
                    {
                        //ms.AlertInfo(zpl);
                        txtzpltest.Text = zpl;
                        txtzpltest.Visibility = Visibility.Visible;

                        CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(zpl);
                        wndPopup.Show();
                    }


                    zplBrowser.Navigate("http://api.labelary.com/v1/printers/8dpmm/labels/10x10/0/" + zpl);
                }
                else
                {
                    I_ATTVAL = labelItemsGet_real();

                    getZpl(I_ATTVAL);

                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);

                    
                }
            }
            catch (Exception ex)
            {

                bkWorker.Dispose();
                throw ex;
            }
        }

        private void BarcodePrint_Real()
        {
            try
            {
                DataTable dtResult = Util.getZPL_Pack(null, null, null, null, "PACK", Util.GetCondition(cboLabelName), "N", nbSeq.Value.ToString(), null);               

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    string zpl = Util.NVC(dtResult.Rows[i]["ZPLSTRING"]);
                    Util.PrintLabel(FrameOperation, loadingIndicator, zpl);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void updateTB_SFC_LABEL_PRT_REQ_HIST(string sScanid, string sPRT_SEQ)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SCAN_ID", typeof(string));
                RQSTDT.Columns.Add("PRT_REQ_SEQNO", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SCAN_ID"] = sScanid;
                dr["PRT_REQ_SEQNO"] = sPRT_SEQ;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_TB_SFC_LABEL_PRT_REQ_HIST_PRTFLAG", "RQSTDT", "", RQSTDT);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable setBoxResultList(string lineID, string procid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(lineID);
                dr["PROCID"] = Util.NVC(procid);
                dr["EQPTID"] = null;
                dr["FROMDATE"] = null;
                dr["TODATE"] = null;
                dr["BOXID"] = sBoxid;
                RQSTDT.Rows.Add(dr);

                DataTable dtboxList = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_LIST_FOR_LABEL1", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtboxList.Rows.Count > 0)
                {
                    return dtboxList;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blPrintStop = true;
            btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
        }
     
        private DataTable AddStatus(DataTable dt, string sValue, string sDisplay)
        {

            DataRow dr = dt.NewRow();

            dr[sDisplay] = "-ALL-";
            dr[sValue] = "";
            dt.Rows.InsertAt(dr, 0);

            return dt;
        }

        private void getZpl(string I_ATTVAL)
        {
            try
            {
                //DataTable RQSTDT = new DataTable();
                //RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("I_LBCD", typeof(string));
                //RQSTDT.Columns.Add("I_PRMK", typeof(string));
                //RQSTDT.Columns.Add("I_RESO", typeof(string));
                //RQSTDT.Columns.Add("I_PRCN", typeof(string));
                //RQSTDT.Columns.Add("I_MARH", typeof(string));
                //RQSTDT.Columns.Add("I_MARV", typeof(string));
                //RQSTDT.Columns.Add("I_ATTVAL", typeof(string));

                //DataRow dr = RQSTDT.NewRow();
                //dr["I_LBCD"] = Util.GetCondition_Thread(cboLabelName);
                //dr["I_PRMK"] = "Z";
                //dr["I_RESO"] = "203";
                //dr["I_PRCN"] = "1";
                //dr["I_MARH"] = "0";
                //dr["I_MARV"] = "0";
                //dr["I_ATTVAL"] = I_ATTVAL;

                //RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_DESIGN_TEST", "INDATA", "OUTDATA", RQSTDT);

                DataTable dtResult = Util.getDirectZpl(
                                                      sLBCD: Util.GetCondition_Thread(cboLabelName)
                                                     ,sATTVAL: I_ATTVAL
                                                     );


                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    zpl = dtResult.Rows[0]["ZPLSTRING"].ToString();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet_test()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = Util.GetCondition(cboLabelName);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();

                        string label_code = Util.GetCondition(cboLabelName).ToString();

                        I_ATTVAL += item_code + "=" + item_value;

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string labelItemsGet_real()
        {
            string I_ATTVAL = string.Empty;
            string item_code = string.Empty;
            string item_value = string.Empty;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LABEL_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LABEL_CODE"] = Util.GetCondition_Thread(cboLabelName);

                RQSTDT.Rows.Add(dr);

                //ITEM001=TEST1^ITEM002=TEST2 : 코드=값^코드=값


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABEL_ITEM_FIND", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult == null || dtResult.Rows.Count > 0)
                {

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        item_code = dtResult.Rows[i]["ITEM_CODE"].ToString();
                        item_value = dtResult.Rows[i]["ITEM_VALUE"].ToString();

                        string label_code = Util.GetCondition_Thread(cboLabelName);

                        if (label_code != "" && testYn == false)
                        {
                            switch (label_code)
                            {
                                case "LBL0021":
                                    if (item_code == "ITEM004" || item_code == "ITEM005")
                                    {
                                        I_ATTVAL += item_code + "=" + sBoxid;
                                    }
                                    else if (item_code == "ITEM003")
                                    {
                                        //I_ATTVAL += item_code + "=" + nbProductBox.Value.ToString() + " EA";
                                        I_ATTVAL += item_code + "=" + Util.GetCondition_Thread(nbProductBox) + " EA";
                                    }
                                    else
                                    {
                                        I_ATTVAL += item_code + "=" + item_value;
                                    }

                                    break;
                                default:
                                    I_ATTVAL += item_code + "=" + item_value;
                                    break;
                            }
                        }
                        else
                        {
                            I_ATTVAL += item_code + "=" + item_value;
                        }

                        if (i < dtResult.Rows.Count - 1)
                        {
                            I_ATTVAL += "^";
                        }
                    }
                }

                return I_ATTVAL;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PrintProcess()
        {

            if (!bkWorker.IsBusy)
            {
                bkWorker.RunWorkerAsync();
                btnPrint.Content = ObjectDic.Instance.GetObjectName("취소");
                blPrintStop = false;
                //btnPrint.Foreground = new Brush(Colors.Red);
            }
            else
            {
                btnPrint.Content = ObjectDic.Instance.GetObjectName("출력");
                blPrintStop = true;
                //btnPrint.ForeColor = Color.Black;
            }
        }


        #endregion

        private void txtzpltest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            if(txtzpltest.Text.Length == 0 )
            {
                return;
            }

            CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(txtzpltest.Text);
            wndPopup.Show();
        }

    }
}
