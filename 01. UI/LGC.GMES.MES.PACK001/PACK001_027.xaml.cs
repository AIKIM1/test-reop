/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.07.02  손우석 CSR ID 3726569 GMES 출하Lot 검색관련 시스템 개선 요청 件 요청번호 C20180629_26569
  2018.07.04  손우석 CSR ID 3730687 GMES 포장 출고 문제 해결 요청의 건 요청번호 C20180704_30687
  2018.12.17  손우석 CSR ID 3858186 [G.MES] Audi BEV C-sample G.MES 출하 구성 결합이력 데이터 출력 기능 추가 요청의 건 [요청번호]C20181130_58186
  2018.12.20  손우석 CSR ID 3858186 [G.MES] Audi BEV C-sample G.MES 출하 구성 결합이력 데이터 출력 기능 추가 요청의 건 [요청번호]C20181130_58186
  2019.01.24  손우석 GB/T 검색 오류 수정
  2019.09.24  손우석 개발 중단 부분 주석 처리
  2020.02.11  박상찬 편도연 사원_Pack 출고요청 전체 현황 신규 UI 생선 요청 건_C20200128-000439
  2025.04.14  김영택 출고처리시 TO_AREAID 가 없는 경우 FROM_AREAID를 처리함 
              1) 참고 (MES 1.0 : E20240912-001152  출고시 TO_AREAID가 공백으로 처리되어 INSERT시 ISNULL이 동작하지 않고 있음 
              이 문제를 해결하기 위해 TO_AREAID가 없으먼 FROM_AREAID로 대체함)
              2) BIZ [BR_PRD_REG_SHIP_PACK]에서 처리 대신 UI에서 처리함 

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_027 : UserControl, IWorkArea
    {
        Util _Util = new Util();

        private int isPalletQty = 0;
        private double isCellQty = 0;

        // 출고 LotID 로 조회하여, 처음으로 조회되는 조립Lot 저장하기 위한 변수
        private string isFirstLotID = string.Empty;

        // 출고 LotID 로 조회하여, 마지막으로 조회되는 조립Lot 저장하기 위한 변수
        private string isLastLotID = string.Empty;

        bool cust_palletidYN = false; //고객사 palletid를 관리하는 라인인지 체크

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        public PACK001_027()
        {
            InitializeComponent();

            Initialize();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnFileReg);
            listAuth.Add(btnPackOut);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            //dtpDateFrom.Text = System.DateTime.Now.ToString("yyyy-MM-dd");
            //dtpDateFrom.Text = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            //dtpDateTo.Text = System.DateTime.Now.ToString("yyyy-MM-dd");

            // ComboBox 추가 필요
            CommonCombo combo = new CommonCombo();

            String[] sFilter = { "PACK" };

            C1ComboBox[] cboToChild = { cboLocFrom , cboLocTo };
            combo.SetCombo(cboAreaAll, CommonCombo.ComboStatus.NONE, cbChild: cboToChild, sCase: "ALLAREA");

            //SLOC03 넘기도록 수정함.. 정현식 2016 12 20
            C1ComboBox cboSLOC_TYPE_CODE = new C1ComboBox();
            cboSLOC_TYPE_CODE.SelectedValue = SLOC_TYPE_CODE.SLOC03;

            C1ComboBox[] cboFromParent = { cboAreaAll , cboSLOC_TYPE_CODE };
            C1ComboBox[] cboToChild2 = { cboLocTo };
            combo.SetCombo(cboLocFrom, CommonCombo.ComboStatus.NONE, cbParent: cboFromParent, cbChild: cboToChild2, sCase: "FROMSLOC_BY_AREA");

            //C1ComboBox[] cboFromParent2 = { cboLocFrom };
            C1ComboBox[] cboToParent = { cboAreaAll };
            C1ComboBox[] cboFromChild = { cboComp };
            combo.SetCombo(cboLocTo, CommonCombo.ComboStatus.SELECT, cbParent: cboToParent, cbChild: cboFromChild, sCase: "cboLocToPack", sFilter: sFilter);

            C1ComboBox[] cboCompParent = { cboLocTo, cboAreaAll };
            combo.SetCombo(cboComp, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent, sCase: "cboCompPack", sFilter: sFilter);

            combo.SetCombo(cboAreaAll2, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: null, sCase: "ALLAREA");

            //2020.02.11
            combo.SetCombo(cboAreaAll2New, CommonCombo.ComboStatus.NONE, sCase: "ALLAREA");


            //2018.12.20
            //모델 
            C1ComboBox[] cboPCChild = { cboPrdtClass };
            combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.SELECT, cbParent: null, cbChild: cboPCChild, sCase: "PRJ_MODEL_AUTH");

            //제품분류(PACK 제품 분류)
            C1ComboBox[] cboMDParent = { cboProductModel };
            C1ComboBox[] cboMDChild = { cboProduct };
            string[] productType = { LoginInfo.CFG_SHOP_ID
                                    ,LoginInfo.CFG_AREA_ID
                                    ,LoginInfo.CFG_EQSG_ID
                                    ,Area_Type.PACK };
            combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.SELECT, cbParent: cboMDParent, cbChild: cboMDChild, sFilter: productType);

            //제품코드  
            C1ComboBox[] cboProductParent = { cboAreaAll3, cboProductModel, cboPrdtClass };
            combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: null);

            //2018.12.17
            combo.SetCombo(cboAreaAll3, CommonCombo.ComboStatus.NONE, cbChild: null, cbParent: null, sCase: "ALLAREA");

            txtPalletID.Focus();
            txtPalletID.SelectAll();

            ShippingHistoryGBT.Visibility = Visibility.Collapsed; // 2024.12.16. 김영국 - 해당 Tab  Hidden 처리.
        }

        private void wndConfirm_Closed(object sender, EventArgs e)
        {
            PACK001_027_CONFIRM window = sender as PACK001_027_CONFIRM;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                try
                {
                    isFirstLotID = "";
                    isLastLotID = "";

                    string sLotTerm = string.Empty;
                    string sArea = string.Empty;
                    string sLocFrom = string.Empty;
                    string sLocTo = string.Empty;
                    string sComp = string.Empty;

                    if (dgPackOut.GetRowCount() == 0)
                    {
                        ms.AlertWarning("SFU2060"); //스캔한 데이터가 없습니다.
                        return;
                    }

                    // 동 선택 확인
                    if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        ms.AlertWarning("SFU1499"); //동을 선택하세요
                        return;
                    }
                    else
                    {
                        sArea = cboAreaAll.SelectedValue.ToString();
                    }

                    // 출고창고 선택 확인
                    if (cboLocFrom.SelectedIndex < 0 || cboLocFrom.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        ms.AlertWarning("SFU2068"); //출고 창고를 선택하세요.
                        return;
                    }
                    else
                    {
                        sLocFrom = cboLocFrom.SelectedValue.ToString();
                    }

                    // 입고창고 선택 확인
                    if (cboLocTo.SelectedIndex < 0 || cboLocTo.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        ms.AlertWarning("SFU2069"); //입고 창고를 선택하세요.
                        return;
                    }
                    else
                    {
                        sLocTo = cboLocTo.SelectedValue.ToString();
                    }

                    // 출하지 선택 확인
                    if (cboComp.SelectedIndex < 0 || cboComp.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        ms.AlertWarning("SFU1936"); //출하지를 선택하세요.
                        return;
                    }
                    else
                    {
                        sComp = cboComp.SelectedValue.ToString();
                    }

                    DataTable RQSTDT4 = new DataTable();
                    RQSTDT4.TableName = "RQSTDT";
                    RQSTDT4.Columns.Add("FROM_AREAID", typeof(String));
                    RQSTDT4.Columns.Add("TO_SLOC_ID", typeof(String));
                    RQSTDT4.Columns.Add("SHIP_TYPE_CODE", typeof(String));

                    DataRow dr4 = RQSTDT4.NewRow();
                    dr4["FROM_AREAID"] = sArea;
                    dr4["TO_SLOC_ID"] = sLocTo;
                    dr4["SHIP_TYPE_CODE"] = "PACK";

                    RQSTDT4.Rows.Add(dr4);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TO_AREAID", "RQSTDT", "RSLTDT", RQSTDT4);

                    string sTo_Areaid = string.Empty;

                    if (SearchResult.Rows.Count > 0)
                    {
                        sTo_Areaid = SearchResult.Rows[0]["TO_AREAID"].ToString();
                    }
                    else if (SearchResult.Rows.Count == 0)
                    {
                        // 2025.04.14 출고처리시 TO_AREAID 값이 없는 경우 오류 방지: FROM_AREAID를 넣어줌 
                        // sTo_Areaid = null;
                        sTo_Areaid = sArea;
                    }


                    if (dgPackOut.GetRowCount() > 0)
                    {
                        // 포장 출고 처리
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("FROM_AREAID", typeof(string));
                        inData.Columns.Add("FROM_SLOC_ID", typeof(string));
                        inData.Columns.Add("TO_AREAID", typeof(string));
                        inData.Columns.Add("TO_SLOC_ID", typeof(string));
                        inData.Columns.Add("ISS_QTY", typeof(int));
                        inData.Columns.Add("ISS_NOTE", typeof(string));
                        inData.Columns.Add("SHIPTO_ID", typeof(string));
                        inData.Columns.Add("NOTE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["FROM_AREAID"] = sArea;
                        row["FROM_SLOC_ID"] = sLocFrom; // Util.GetCondition(cboLocFrom, "출고창고를선택하세요.");//출고창고
                        row["TO_AREAID"] = sTo_Areaid;          //입고 Area
                        row["TO_SLOC_ID"] = sLocTo;     // Util.GetCondition(cboLocTo, "입고창고를선택하세요.");//입고저장위치
                        row["ISS_QTY"] = isCellQty;     //출고수량
                        row["ISS_NOTE"] = "";
                        row["SHIPTO_ID"] = sComp;       // Util.GetCondition(cboComp, "출하처를선택하세요.");//출하처
                        row["NOTE"] = "";
                        //row["USERID"] = txtUserID.Text;
                        row["USERID"] = LoginInfo.USERID;//txtUserID.Tag;

                        if (row["FROM_SLOC_ID"].Equals("") || row["TO_SLOC_ID"].Equals("") || row["SHIPTO_ID"].Equals("")) return;

                        indataSet.Tables["INDATA"].Rows.Add(row);

                        DataTable inPallet = indataSet.Tables.Add("INPALLET");
                        inPallet.Columns.Add("BOXID", typeof(string));
                        inPallet.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            DataRow row2 = inPallet.NewRow();
                            row2["BOXID"] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                            row2["OWMS_BOX_TYPE_CODE"] = "PA";

                            indataSet.Tables["INPALLET"].Rows.Add(row2);
                        }

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SHIP_PACK", "INDATA,INPALLET", "OUTDATA, OUTDATA_LOTTERM", indataSet);

                        string sOut_ID = string.Empty;

                        //if (dsRslt.Tables[0].Rows.Count > 0)
                        if (dsRslt.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            sOut_ID = dsRslt.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString();
                        }
                        else
                        {
                            //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 ID 가 생성되지 않았습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                            ms.AlertWarning("SFU3010"); //출고 ID 가 생성되지 않았습니다.
                            return;
                        }

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                        DataRow dr = RQSTDT.NewRow();
                        dr["RCV_ISS_ID"] = sOut_ID;

                        RQSTDT.Rows.Add(dr);

                        DataTable dtLotterm = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT);

                        if (dtLotterm.Rows.Count < 0)
                        {
                            ms.AlertWarning("SFU1386"); //LOT정보가 없습니다.
                            return;
                        }

                        sLotTerm = dtLotterm.Rows[0]["LOTTERM"].ToString();

                        // 작업일자                     
                        string sProdDate = DateTime.Now.Year.ToString() + ObjectDic.Instance.GetObjectName("월") +
                                            DateTime.Now.Month.ToString() + ObjectDic.Instance.GetObjectName("월") +
                                            DateTime.Now.Day.ToString() + ObjectDic.Instance.GetObjectName("일") +
                                            DateTime.Now.Hour.ToString() + ObjectDic.Instance.GetObjectName("시") +
                                            DateTime.Now.Minute.ToString() + ObjectDic.Instance.GetObjectName("분") +
                                            DateTime.Now.Second.ToString() + ObjectDic.Instance.GetObjectName("초");

                        DataTable RQSTDT1 = new DataTable();
                        RQSTDT1.TableName = "RQSTDT";
                        RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                        RQSTDT1.Columns.Add("LANGID", typeof(String));

                        DataRow dr1 = RQSTDT1.NewRow();
                        dr1["RCV_ISS_ID"] = sOut_ID;
                        dr1["LANGID"] = LoginInfo.LANGID;

                        RQSTDT1.Rows.Add(dr1);

                        DataTable dtLineResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_LINE_QTY_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT1);

                        #region Report 출력

                        DataTable dtPacking_Tag = new DataTable();

                        // Line1 ~ Line8
                        for (int i = 1; i < 9; i++)
                        {
                            dtPacking_Tag.Columns.Add("Line_" + i, typeof(string));
                        }

                        // Qty1 ~ Qty8
                        for (int i = 1; i < 9; i++)
                        {
                            dtPacking_Tag.Columns.Add("Qty" + i, typeof(string));
                        }

                        // Model
                        dtPacking_Tag.Columns.Add("Model", typeof(string));

                        // 출고 Pallet 수량
                        dtPacking_Tag.Columns.Add("Pallet_Qty", typeof(string));

                        // Pack_ID
                        dtPacking_Tag.Columns.Add("Pack_ID", typeof(string));

                        // 출하처
                        dtPacking_Tag.Columns.Add("SHIP_TO_NAME", typeof(string));

                        // Pack_ID Barcode
                        dtPacking_Tag.Columns.Add("HEAD_BARCODE", typeof(string));

                        // Lot 최대 편차
                        dtPacking_Tag.Columns.Add("Lot_Qty", typeof(string));

                        // 작업일자
                        dtPacking_Tag.Columns.Add("Prod_Date", typeof(string));

                        // 제품수량
                        dtPacking_Tag.Columns.Add("Prod_Qty", typeof(string));

                        // 제품 ID
                        dtPacking_Tag.Columns.Add("Prod_ID", typeof(string));

                        // 작업자
                        dtPacking_Tag.Columns.Add("User", typeof(string));

                        // 출고창고
                        dtPacking_Tag.Columns.Add("Out_WH", typeof(string));

                        // 입고창고
                        dtPacking_Tag.Columns.Add("In_WH", typeof(string));

                        // Pallet ID ( P_ID1 ~ P_ID120 )
                        for (int i = 1; i < 121; i++)
                        {
                            dtPacking_Tag.Columns.Add("P_ID" + i, typeof(string));
                        }

                        // 수량 ( P_Qty1 ~ P_Qty120 )
                        for (int i = 1; i < 121; i++)
                        {
                            dtPacking_Tag.Columns.Add("P_Qty" + i, typeof(string));
                        }

                        // Lot ID ( L_ID1 ~ L_ID24 )
                        for (int i = 1; i < 25; i++)
                        {
                            dtPacking_Tag.Columns.Add("L_ID" + i, typeof(string));
                        }

                        // 수량 ( L_Qty1 ~ L_Qty24 )
                        for (int i = 1; i < 25; i++)
                        {
                            dtPacking_Tag.Columns.Add("L_Qty" + i, typeof(string));
                        }


                        string sType = string.Empty;

                        if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "PLT")
                        {
                            sType = "P";
                        }
                        else if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "MGZ")
                        {
                            sType = "M";
                        }
                        else if (DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXTYPE").ToString() == "TRY")
                        {
                            sType = "B";
                        }

                        DataRow drCrad = null;
                        drCrad = dtPacking_Tag.NewRow();

                        for (int i = 0; i < 8; i++)
                        {
                            drCrad["Line_" + (i + 1).ToString()] = "";
                            drCrad["Qty" + (i + 1).ToString()] = "";
                        }

                        for (int i = 0; i < dtLineResult.Rows.Count; i++)
                        {
                            drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGNAME"].ToString();
                            drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                        }

                        drCrad["Pallet_Qty"] = txtPalletQty.Text.ToString();

                        drCrad["Model"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString() == string.Empty ? DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString() : DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();

                        drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID

                        drCrad["SHIP_TO_NAME"] = ((System.Data.DataRowView)cboComp.SelectedItem).Row.ItemArray[1].ToString();    //출하처 발행 위치

                        drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID

                        drCrad["Lot_Qty"] = sLotTerm;         //최대편차

                        drCrad["Prod_Date"] = sProdDate;         //작업일자

                        drCrad["Prod_Qty"] = txtCellQty.Text.ToString();         //제품수량

                        drCrad["Prod_ID"] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();         //제품ID

                        drCrad["User"] = LoginInfo.USERNAME;//txtUserID.Text.ToString();         //작업자

                        drCrad["Out_WH"] = cboLocFrom.SelectedValue.ToString();         //출고창고

                        drCrad["In_WH"] = cboLocTo.SelectedValue.ToString();         //입고창고


                        int j = 1;
                        for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                        {
                            drCrad["P_ID" + j] = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                            drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);

                            // drCrad["Prod_Qty"] = Math.Round(Convert.ToDouble(txtCellQty.Text.ToString()), 2, MidpointRounding.AwayFromZero); 
                            j++;
                        }

                        int k = 1;
                        //for (int i = 0; i < LotResult.Rows.Count; i++)
                        //{
                        //    drCrad["L_ID" + k] = LotResult.Rows[i]["LOTID"].ToString();
                        //    drCrad["L_Qty" + k] = LotResult.Rows[i]["LOTQTY"].ToString();
                        //    k++;
                        //}


                        dtPacking_Tag.Rows.Add(drCrad);

                        object[] Parameters = new object[3];
                        Parameters[0] = "Packing_Tag";
                        Parameters[1] = dtPacking_Tag;
                        Parameters[2] = "2";

                        LGC.GMES.MES.PACK001.Report_Multi rs = new LGC.GMES.MES.PACK001.Report_Multi();
                        C1WindowExtension.SetParameters(rs, Parameters);

                        //rs.Closed += new EventHandler(wndQAMailSend_Closed);
                        rs.Closed += new EventHandler(wndPackingTag_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));

                        #endregion
                    }
                    else
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장 출고 버튼을 누르시기 전에 리스트에 등록을 하셔야 합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        ms.AlertWarning("SFU3401"); //포장 출고 버튼을 누르시기 전에 리스트에 등록을 하셔야 합니다.
                        return;
                    }
                }
                catch (Exception ex)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    //Util.Alert(ex.Message);
                    Util.MessageException(ex);
                    return;
                }
            }
        }

        private void wndPackingTag_Closed(object sender, EventArgs e)
        {
            Initialize_Out();
        }

        private void Initialize_Out()
        {
            Util.gridClear(dgPackOut);

            isPalletQty = 0;
            isCellQty = 0;

            txtPalletQty.Text = "0";
            txtCellQty.Text = "0";
            txtPalletID.Text = "";
            txtPalletID.Focus();

            cboLocTo.SelectedIndex = 0;
            cboLocFrom.SelectedIndex = 0;
        }
        #endregion

        #region Mehod
        //private Boolean SelectOutHistToExcelFile(string sLot_ID)
        //{
        //}
        private void Out_Hist()
        {
            try
            {
                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); //((DateTime) dtpDateFrom.SelectedDate).ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateFrom.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateFrom.SelectedDateTime);
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); //((DateTime)dtpDateTo.SelectedDate).ToString("yyyyMMdd"); //Convert.ToDateTime(dtpDateTo.Text).ToString("yyyyMMdd"); //string.Format("{0:yyyyMMdd}", dtpDateTo.SelectedDateTime);

                string sArea = string.Empty;

                if (cboAreaAll2.SelectedIndex < 0 || cboAreaAll2.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sArea = null;
                }
                else
                {
                    sArea = cboAreaAll2.SelectedValue.ToString();
                }

                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1) == "P")
                {
                    dr["BOXID"] = txtBoxID.Text.Trim();
                }
                else if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1) != "P")
                {
                    dr["RCV_ISS_ID"] = txtBoxID.Text.Trim();
                }
                else
                {
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                }
                dr["AREAID"] = sArea;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_SHIP_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutHist);
                dgOutHist.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //Util.Alert(ex.Message);
                Util.MessageException(ex);
                return;
            }
        }

        //2020.02.11
        private void Out_HistNew()
        {
            try
            {
                string sArea = string.Empty;

                if (cboAreaAll2New.SelectedIndex < 0 || cboAreaAll2New.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    sArea = null;
                }
                else
                {
                    sArea = cboAreaAll2New.SelectedValue.ToString();
                }

                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = sArea;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_SHIP_HIST_ALL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutHistNew);
                dgOutHistNew.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        //2020.02.11
        private void Out_HistDetailNew(String sRCV_ISS_ID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_BOX_BY_RCV_ALL_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(dgOutDetailNew, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setCustPalletidChk(string eqsgid)
        {
            try
            {
                cust_palletidYN = false;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "CUST_PALLETID_EQSGID";

                RQSTDT.Rows.Add(dr);

                DataTable dtCUSTPALLETIDEQSGID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CUST_PALLETID_EQSGID_FIND", "INDATA", "OUTDATA", RQSTDT);

                string eqsgid_codes = string.Empty;

                if (dtCUSTPALLETIDEQSGID != null && dtCUSTPALLETIDEQSGID.Rows.Count > 0)
                {
                    for (int i = 0; i < dtCUSTPALLETIDEQSGID.Columns.Count; i++)
                    {
                        if (eqsgid.Length != 0 && eqsgid == dtCUSTPALLETIDEQSGID.Rows[0][i].ToString())
                        {
                            cust_palletidYN = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private string eqsgidFind(string palletid)
        {
            //입력된 BOX id 상태
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["PALLETID"] = palletid;

                RQSTDT.Rows.Add(dr);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PALLET_EQSGID_FIND", "INDATA", "OUTDATA", RQSTDT); //박스가 있는지 확인

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    return dtResult.Rows[0][0].ToString();
                }
                else
                {
                    return "";
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Scan_Process(string sPallet_ID, string sArea)
        {
            try
            {
                //// Pallet 상태 체크
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["BOXID"] = sPallet_ID;
                dr["AREAID"] = sArea;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_BOX_INFO_FOR_SHIP", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                //if (dsRslt.Tables["OUTDATA"].Rows.Count <= 0)
                {
                    ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                    return;
                }

                if (dgPackOut.GetRowCount() != 0)
                {
                    if (SearchResult.Rows[0]["TO_SLOC_ID"].ToString() != DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "TO_SLOC_ID").ToString())
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("이전에 스캔한 Pallet 의 출고창고 정보와 다릅니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        ms.AlertWarning("SFU3351"); //작업오류 : 이전에 스캔한 PALLET 의 출고 정보가 다릅니다. [출고처리는 같은 출고창고 만 가능] 
                        return;
                    }

                    for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString() == sPallet_ID)
                        {
                            ms.AlertWarning("SFU1914"); //중복 스캔되었습니다.
                            return;
                        }

                        if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                        {
                            ms.AlertWarning("SFU1893"); //제품ID가 같지 않습니다.
                            return;
                        }
                    }

                    dgPackOut.IsReadOnly = false;
                    dgPackOut.BeginNewRow();
                    dgPackOut.EndNewRow(true);
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "BOXID", SearchResult.Rows[0]["BOXID"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PRODID", SearchResult.Rows[0]["PRODID"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "TOTAL_QTY", SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "BOXTYPE", SearchResult.Rows[0]["BOXTYPE"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "INNER_BOXTYPE", SearchResult.Rows[0]["INNER_BOXTYPE"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "TO_SLOC_ID", SearchResult.Rows[0]["TO_SLOC_ID"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "BOXSTAT", SearchResult.Rows[0]["BOXSTAT"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PROJECTNAME", SearchResult.Rows[0]["PROJECTNAME"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "PACK_NOTE", SearchResult.Rows[0]["PACK_NOTE"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "OUTER_BOXID2", SearchResult.Rows[0]["OUTER_BOXID2"].ToString());
                    DataTableConverter.SetValue(dgPackOut.CurrentRow.DataItem, "OCOP_RTN_CNT", SearchResult.Rows[0]["OCOP_RTN_CNT"].ToString());
                    dgPackOut.IsReadOnly = true;

                }
                else
                {
                    dgPackOut.ItemsSource = DataTableConverter.Convert(SearchResult);
                }

                isPalletQty = isPalletQty + 1;
                txtPalletQty.Text = isPalletQty.ToString();

                //isCellQty = isCellQty + Convert.ToInt32(dsRslt.Tables["OUTDATA"].Rows[0]["TOTAL_QTY"].ToString());
                isCellQty = isCellQty + Convert.ToDouble(SearchResult.Rows[0]["TOTAL_QTY"].ToString());
                txtCellQty.Text = isCellQty.ToString();

                txtPalletID.Text = "";
                txtPalletID.Focus();

            }

            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_GET_PALLET_INFO_FOR_SHIP", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return;
            }
        }

        private void Search_PalletInfo(int idx)
        {
            try
            {
                string sOutLotid = string.Empty;

                sOutLotid = DataTableConverter.GetValue(dgOutHist.Rows[idx].DataItem, "RCV_ISS_ID").ToString();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                //RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["RCV_ISS_ID"] = sOutLotid;

                RQSTDT.Rows.Add(dr);

                DataTable DetailResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_BOX_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgOutDetail);
                dgOutDetail.ItemsSource = DataTableConverter.Convert(DetailResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2018.12.17
        private void Get_GBT()
        {
            try
            {
                string sStart_date = dtpDateFrom1.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo1.SelectedDateTime.ToString("yyyyMMdd");

                string sArea = string.Empty;

                if (cboAreaAll3.SelectedIndex < 0)
                {
                    return;
                }
                else
                {
                    sArea = cboAreaAll3.SelectedValue.ToString();
                }

                // 조회 비즈 생성
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("AREAID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));
                //2018.12.20
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("MODLID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (txtRCVISSID.Text.Trim() != "")
                {
                    dr["RCV_ISS_ID"] = txtRCVISSID.Text.Trim();
                }
                else if (txtBoxID.Text.Trim() != "" && txtBoxID.Text.Substring(0, 1) != "P")
                {
                    dr["BOXID"] = txtPID.Text.Trim();
                }

                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["AREAID"] = sArea;

                //2018.12.20
                //2019.01.24
                //dr["PRODID"] = cboProduct.SelectedValue.ToString();
                dr["PRODID"] = cboProduct.SelectedValue.ToString() == "" ? null : cboProduct.SelectedValue.ToString();
                dr["MODLID"] = cboProductModel.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_GBT_SHIP_HIST", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dgGBT);
                dgGBT.ItemsSource = DataTableConverter.Convert(SearchResult);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion Method

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sOut_ID = string.Empty;
                string sLotTerm = string.Empty;

                isFirstLotID = "";
                isLastLotID = "";

                if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                {
                    // LOT ID, QTY 조회
                    sOut_ID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_ID").ToString();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("RCV_ISS_ID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["RCV_ISS_ID"] = sOut_ID;

                    RQSTDT.Rows.Add(dr);

                    DataTable PalletInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTERM_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT);

                    if (PalletInfo.Rows.Count < 0)
                    {                        
                        ms.AlertWarning("SFU1386"); //LOT정보가 없습니다.
                        return;
                    }                    

                    sLotTerm = PalletInfo.Rows[0]["LOTTERM"].ToString();

                    DataRow[] drChk = Util.gridGetChecked(ref dgOutHist, "CHK");

                    // 작업일자 
                    //DateTime ProdDate = Convert.ToDateTime(DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "INSDTTM").ToString());
                    DateTime ProdDate = Convert.ToDateTime(drChk[0]["INSDTTM"].ToString());
                    string sProdDate =  ProdDate.Year.ToString() + ObjectDic.Instance.GetObjectName("년") + 
                                        ProdDate.Month.ToString() + ObjectDic.Instance.GetObjectName("월") + 
                                        ProdDate.Day.ToString() + ObjectDic.Instance.GetObjectName("일") +
                                        ProdDate.Hour.ToString() + ObjectDic.Instance.GetObjectName("시") + 
                                        ProdDate.Minute.ToString() + ObjectDic.Instance.GetObjectName("분") + 
                                        ProdDate.Second.ToString() + ObjectDic.Instance.GetObjectName("초");


                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("RCV_ISS_ID", typeof(String));
                    RQSTDT1.Columns.Add("LANGID", typeof(String));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["RCV_ISS_ID"] = sOut_ID;
                    dr1["LANGID"] = LoginInfo.LANGID;

                    RQSTDT1.Rows.Add(dr1);

                    DataTable dtLineResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACK_LINE_QTY_BY_RCV", "RQSTDT", "RSLTDT", RQSTDT1);

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    #region Report

                    DataTable dtPacking_Tag = new DataTable();

                    // Line1 ~ Line8
                    for (int i = 1; i < 9; i++)
                    {
                        dtPacking_Tag.Columns.Add("Line_" + i, typeof(string));
                    }

                    // Qty1 ~ Qty8
                    for (int i = 1; i < 9; i++)
                    {
                        dtPacking_Tag.Columns.Add("Qty" + i, typeof(string));
                    }

                    // Model
                    dtPacking_Tag.Columns.Add("Model", typeof(string));

                    // 출고 Pallet 수량
                    dtPacking_Tag.Columns.Add("Pallet_Qty", typeof(string));

                    // Pack_ID
                    dtPacking_Tag.Columns.Add("Pack_ID", typeof(string));

                    // Pack_ID Barcode
                    dtPacking_Tag.Columns.Add("HEAD_BARCODE", typeof(string));

                    // Lot 최대 편차
                    dtPacking_Tag.Columns.Add("Lot_Qty", typeof(string));

                    // 작업일자
                    dtPacking_Tag.Columns.Add("Prod_Date", typeof(string));

                    // 제품수량
                    dtPacking_Tag.Columns.Add("Prod_Qty", typeof(string));

                    // 제품 ID
                    dtPacking_Tag.Columns.Add("Prod_ID", typeof(string));

                    // 작업자
                    dtPacking_Tag.Columns.Add("User", typeof(string));

                    // 출고창고
                    dtPacking_Tag.Columns.Add("Out_WH", typeof(string));

                    // 입고창고
                    dtPacking_Tag.Columns.Add("In_WH", typeof(string));

                    // 입고창고
                    dtPacking_Tag.Columns.Add("SHIP_TO_NAME", typeof(string));

                    // Pallet ID ( P_ID1 ~ P_ID160 )
                    //2018.07.02
                    //for (int i = 1; i < 105; i++)
                    for (int i = 1; i < 161; i++)
                    {
                        dtPacking_Tag.Columns.Add("P_ID" + i, typeof(string));
                    }

                    // 수량 ( P_Qty1 ~ P_Qty160 )
                    //2018.07.02
                    //for (int i = 1; i < 105; i++)
                    for (int i = 1; i < 161; i++)
                    {
                        dtPacking_Tag.Columns.Add("P_Qty" + i, typeof(string));
                    }

                    // Lot ID ( L_ID1 ~ L_ID24 )
                    for (int i = 1; i < 41; i++)
                    {
                        dtPacking_Tag.Columns.Add("L_ID" + i, typeof(string));
                    }

                    // 수량 ( L_Qty1 ~ L_Qty24 )
                    for (int i = 1; i < 41; i++)
                    {
                        dtPacking_Tag.Columns.Add("L_Qty" + i, typeof(string));
                    }

                    string sType = string.Empty;

                    if (drChk[0]["BOXTYPE"].ToString() == "PLT")
                    {
                        sType = "P";
                    }
                    else if (drChk[0]["BOXTYPE"].ToString() == "MGZ")
                    {
                        sType = "M";
                    }
                    else if (drChk[0]["BOXTYPE"].ToString() == "BOX")
                    {
                        sType = "B";
                    }

                    DataRow drCrad = null;
                    drCrad = dtPacking_Tag.NewRow();

                    for (int i = 0; i < 8; i++)
                    {
                        drCrad["Line_" + (i + 1).ToString()] = "";
                        drCrad["Qty" + (i + 1).ToString()] = "";
                    }

                    for (int i = 0; i < dtLineResult.Rows.Count; i++)
                    {
                        drCrad["Line_" + (i + 1).ToString()] = dtLineResult.Rows[i]["EQSGNAME"].ToString();
                        drCrad["Qty" + (i + 1).ToString()] = dtLineResult.Rows[i]["LINE_QTY"].ToString();
                    }

                    //drCrad["Line1"] = "";

                    //drCrad["Qty1"] = sType + "_" + drChk[0]["PALLETQTY"].ToString();

                    drCrad["Pallet_Qty"] = drChk[0]["PALLETQTY"].ToString();

                    drCrad["Model"] = drChk[0]["PROJECTNAME"].ToString() == string.Empty ? drChk[0]["MODLID"].ToString() : drChk[0]["PROJECTNAME"].ToString();

                    drCrad["Pack_ID"] = sOut_ID;    //포장출고 ID

                    drCrad["HEAD_BARCODE"] = sOut_ID;    //포장출고 ID

                    drCrad["Lot_Qty"] = sLotTerm;         //최대편차

                    drCrad["Prod_Date"] = sProdDate;         //작업일자

                    //Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);

                    drCrad["Prod_Qty"] = Math.Round(Convert.ToDouble(drChk[0]["TOTALQTY"].ToString()), 2, MidpointRounding.AwayFromZero);  //제품수량

                    drCrad["Prod_ID"] = drChk[0]["PRODID"].ToString();         //제품ID

                    drCrad["User"] = drChk[0]["INSUSERNAME"].ToString();         //작업자

                    drCrad["Out_WH"] = drChk[0]["FROM_SLOC_ID"].ToString();         //출고창고

                    drCrad["In_WH"] = drChk[0]["TO_SLOC_ID"].ToString();         //입고창고

                    drCrad["SHIP_TO_NAME"] = drChk[0]["SHIPTO_NAME"].ToString(); // 출하지
                    
                    // 현재..max 120
                    int j = 1;
                    for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                    {
                        //2018.07.04
                        //drCrad["P_ID" + j] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString();
                        ////drCrad["P_Qty" + i] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString();
                        //drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                        //j++;

                        if (i == 0)
                        {
                            drCrad["P_ID" + j] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString();
                            drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                            j++;
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgOutDetail.Rows[i-1].DataItem, "OUTER_BOXID").ToString() == DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString())
                            {

                            }
                            else
                            {
                                drCrad["P_ID" + j] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString();
                                drCrad["P_Qty" + j] = Math.Round(Convert.ToDouble(DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "TOTAL_QTY").ToString()), 2, MidpointRounding.AwayFromZero);
                                j++;
                            }
                        }

                    }

                    #region 2019.09.24 주석 처리
                    //DataTable LotInfo = new DataTable();
                    //int k = 1;

                    //DataTable DTLOT = new DataTable();
                    //DTLOT.TableName = "RQSTDT";
                    //DTLOT.Columns.Add("BOXID", typeof(String));

                    //DataRow DRLOT = DTLOT.NewRow();

                    //int lot_total_cnt = 0;
                    //for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                    //{
                    //    DRLOT = null;
                    //    DRLOT = DTLOT.NewRow();

                    //    DRLOT["BOXID"] = DataTableConverter.GetValue(dgOutDetail.Rows[0].DataItem, "OUTER_BOXID").ToString(); //PALLETID
                    //    DTLOT.Rows.Add(DRLOT);

                    //    LotInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKED_PALLET_WITH_LOT", "RQSTDT", "RSLTDT", DTLOT);

                    //    if(LotInfo != null && LotInfo.Rows.Count > 0)
                    //    {
                    //        lot_total_cnt += LotInfo.Rows.Count;
                    //    }                       
                    //}
                    #endregion 2019.09.24 주석 처리


                    #region 기존 주석
                    /*
                        if (lot_total_cnt > 40)
                        {
                            ms.AlertInfo("출력 DATA LOT 수량 제한 : 40개까지만 출력됩니다.\n                        현재 LOT수량(" + lot_total_cnt.ToString() + ")"); //
                            return;
                        }
                    */
                    /*  LOT및 LOT수량 부분 제거
                    for (int i = 0; i < dgOutDetail.GetRowCount(); i++)
                    {
                        DRLOT = null;
                        DRLOT = DTLOT.NewRow();

                        DRLOT["BOXID"] = DataTableConverter.GetValue(dgOutDetail.Rows[i].DataItem, "OUTER_BOXID").ToString(); //PALLETID
                        DTLOT.Rows.Add(DRLOT);

                        LotInfo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PACKED_PALLET_WITH_LOT", "RQSTDT", "RSLTDT", DTLOT);

                        if(LotInfo.Rows.Count > 40)
                        {
                            ms.AlertWarning("출력 DATA Row수 제한 : 40Row까지만 출력됩니다."); //
                            return;
                        }

                        if(LotInfo.Rows.Count>0)
                        {
                            // 현재..max 24

                            for (int p = 0; p < LotInfo.Rows.Count; p++)
                            {
                                if (k > 40)
                                {
                                    ms.AlertWarning("출력 DATA LOT 수량 제한 : 40개까지만 출력됩니다."); //
                                    return;
                                }

                                drCrad["L_ID" + k] = LotInfo.Rows[p]["LOTID"].ToString();
                                drCrad["L_Qty" + k] = Convert.ToInt16(LotInfo.Rows[p]["LOTQTY"]).ToString();
                                k++;
                            }
                        }
                    }       
*/
                    #endregion

                    dtPacking_Tag.Rows.Add(drCrad);

                    LGC.GMES.MES.PACK001.Report_Multi rs = new LGC.GMES.MES.PACK001.Report_Multi();
                    rs.FrameOperation = this.FrameOperation;

                    if (rs != null)
                    {
                        // 태그 발행 창 화면에 띄움.
                        object[] Parameters = new object[2];
                        Parameters[0] = "Packing_Tag";
                        Parameters[1] = dtPacking_Tag;

                        C1WindowExtension.SetParameters(rs, Parameters);

                        rs.Closed += new EventHandler(printPopUp_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));

                    }

                    #endregion

                }

                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3254"); //출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오.
                    return;
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.MessageException(ex);
                return;
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            string sLotID = string.Empty;

            if (dgOutHist.Rows.Count > 0)
            {
                if (_Util.GetDataGridCheckCnt(dgOutHist, "CHK") > 0)
                {
                    if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                    {
                        sLotID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "LOT_ID").ToString();

                        //SelectOutHistToExcelFile(sLotID)
                    }
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3254"); //출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오.
                    return;
                }
            }
            else
            {
               // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 이력을 조회 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                ms.AlertWarning("SFU3255"); //출고 이력을 조회 하신 후 버튼을 클릭해주십시오.
                return;
            }

        }

        private void btnPackOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                isFirstLotID = "";
                isLastLotID = "";

                string sLotTerm = string.Empty;
                string sArea = string.Empty;
                string sLocFrom = string.Empty;
                string sLocTo = string.Empty;
                string sComp = string.Empty;

                if (dgPackOut.GetRowCount() == 0)
                {
                    ms.AlertWarning("SFU2060"); //스캔한 데이터가 없습니다.
                    return;
                }

                // 동 선택 확인
                if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    ms.AlertWarning("SFU1499"); //동을 선택하세요.
                    return;
                }
                else
                {
                    sArea = cboAreaAll.SelectedValue.ToString();
                }

                // 출고창고 선택 확인
                if (cboLocFrom.SelectedIndex < 0 || cboLocFrom.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    ms.AlertWarning("SFU2068"); //출고창고를 선택하세요.
                    return;
                }
                else
                {
                    sLocFrom = cboLocFrom.SelectedValue.ToString();
                }

                // 입고창고 선택 확인
                if (cboLocTo.SelectedIndex < 0 || cboLocTo.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    ms.AlertWarning("SFU2069"); //입고창고를 선택하세요.
                    return;
                }
                else
                {
                    sLocTo = cboLocTo.SelectedValue.ToString();
                }

                // 출하지 선택 확인
                if (cboComp.SelectedIndex < 0 || cboComp.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    ms.AlertWarning("SFU1936"); //출하지를 선택 하십시오
                    return;
                }
                else
                {
                    sComp = cboComp.SelectedValue.ToString();
                }

                //PALLET의 라인 찾음.
                string eqsgid = eqsgidFind(DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "BOXID").ToString());

                //고객사 PALLETID를 관리하는 라인인지 확인
                setCustPalletidChk(eqsgid);

                //고객사 PALLETID를 관리하는 라인일 경우
                if (cust_palletidYN)
                {
                    bool noOut = false;
                    string NoPalletid = string.Empty;

                    for (int i = 0; i < dgPackOut.GetRowCount(); i++)
                    {
                        if (i == 0)
                        {
                            if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "OUTER_BOXID2").ToString().Length == 0)
                            {
                                noOut = true;
                                NoPalletid = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                            }
                        }
                        else
                        {
                            if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "OUTER_BOXID2").ToString().Length == 0)
                            {
                                noOut = true;

                                if (NoPalletid.Length == 0)
                                {
                                    NoPalletid = DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                }
                                else
                                {
                                    NoPalletid += "," + DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "BOXID").ToString();
                                }

                            }
                        }
                    }

                    if (noOut)//고객사 palletID 미입력
                    {
                        Util.Alert("[NO GTLID]\n" + NoPalletid);

                        return;
                    }

                    noOut = false;
                }

                if (dgPackOut.GetRowCount() > 0)
                {
                    PACK001_027_CONFIRM wndConfirm = new PACK001_027_CONFIRM();
                    wndConfirm.FrameOperation = FrameOperation;

                    if (wndConfirm != null)
                    {
                        object[] Parameters = new object[6];
                        Parameters[0] = cboComp.Text;
                        Parameters[1] = cboLocTo.Text;
                        Parameters[2] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PROJECTNAME").ToString();
                        Parameters[3] = DataTableConverter.GetValue(dgPackOut.Rows[0].DataItem, "PRODID").ToString();
                        Parameters[4] = txtPalletQty.Text;
                        Parameters[5] = txtCellQty.Text;
                        C1WindowExtension.SetParameters(wndConfirm, Parameters);

                        wndConfirm.Closed += new EventHandler(wndConfirm_Closed);
                        this.Dispatcher.BeginInvoke(new Action(() => wndConfirm.ShowModal()));
                    }

                    cust_palletidYN = false;
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("포장 출고 버튼을 누르시기 전에 리스트에 등록을 하셔야 합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3401"); //포장 출고 버튼을 누르시기 전에 리스트에 등록을 하셔야 합니다.
                    return;
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
                return;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1230"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.Warning, (result) =>
            //삭제하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;


                    double dqty = Convert.ToDouble(DataTableConverter.GetValue(dgPackOut.Rows[index].DataItem, "TOTAL_QTY").ToString());

                    isPalletQty = isPalletQty - 1;
                    txtPalletQty.Text = isPalletQty.ToString();

                    isCellQty = isCellQty - dqty;
                    txtCellQty.Text = isCellQty.ToString();

                    dgPackOut.IsReadOnly = false;
                    dgPackOut.RemoveRow(index);
                    dgPackOut.IsReadOnly = true;

                }
            });

            //if (dgPackOut.GetRowCount() == 0)
            //{
            //    return;
            //}

            //for (int i = 0; i < dgPackOut.Rows.Count; i++)
            //{
            //    if (DataTableConverter.GetValue(dgPackOut.Rows[i].DataItem, "CHK").ToString() == "True")
            //    {
            //        dgPackOut.IsReadOnly = false;
            //        dgPackOut.RemoveRow(i);
            //        dgPackOut.IsReadOnly = true;
            //    }
            //}
        }

        private void btnFileReg_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sArea = string.Empty;

                // 동 선택 확인
                if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    ms.AlertWarning("SFU1499"); //동을 선택하세요
                    return;
                }
                else
                {
                    sArea = cboAreaAll.SelectedValue.ToString();
                }

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        DataTable dataTable = new DataTable();
                        dataTable.Columns.Add("PALLETID", typeof(string));

                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            DataRow dataRow = dataTable.NewRow();
                            XLCell cell = sheet.GetCell(rowInx, 0);
                            if (cell != null)
                            {
                                dataRow["PALLETID"] = cell.Text;
                            }

                            dataTable.Rows.Add(dataRow);
                        }

                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            string sPalletID = dataTable.Rows[i]["PALLETID"].ToString();

                            Scan_Process(sPalletID, sArea);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.None);
                Util.Alert(ex.Message);
                return;
            }
        }

        private void btnWorker_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER wndPopup = new CMM_SHIFT_USER();
            wndPopup.FrameOperation = FrameOperation;

            if (wndPopup != null)
            {
                object[] Parameters = new object[5];
                Parameters[0] = LoginInfo.CFG_SHOP_ID;
                Parameters[1] = LoginInfo.CFG_AREA_ID;
                Parameters[2] = LoginInfo.CFG_EQPT_ID;
                Parameters[3] = LoginInfo.CFG_PROC_ID;
                Parameters[4] = "";
                C1WindowExtension.SetParameters(wndPopup, Parameters);

                wndPopup.Closed += new EventHandler(wndShiftUser_Closed);
                this.Dispatcher.BeginInvoke(new Action(() => wndPopup.ShowModal()));
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOutHist);
            Util.gridClear(dgOutDetail);

            Out_Hist();
        }

        //2020.02.11
        private void btnSearchNew_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgOutHistNew);
            Util.gridClear(dgOutDetailNew);

            Out_HistNew();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Initialize_Out();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string sOut_ID = string.Empty;
                string sAreaID = string.Empty;
                if (_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK") >= 0)
                {
                    // LOT ID, QTY 조회
                    if (DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_STAT_CODE").ToString() == "SHIPPING")
                    {
                        sOut_ID = DataTableConverter.GetValue(dgOutHist.Rows[_Util.GetDataGridCheckFirstRowIndex(dgOutHist, "CHK")].DataItem, "RCV_ISS_ID").ToString();

                        sAreaID = Util.NVC(cboAreaAll2.SelectedValue); //cboAreaAll2.SelectedValue.ToString();

                        loadingIndicator.Visibility = Visibility.Visible;
                        string[] sParam = { sAreaID, sOut_ID };
                        // 포장 출고 취소
                        this.FrameOperation.OpenMenu("SFU010100110", true, sParam);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("입고 전 출고요청만 취소 가능합니다. "), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        ms.AlertWarning("SFU3352"); //적업오류 : 입고 전 출고요청만 취소 가능합니다. [ 창고에서 이미 입고처리하여 출고취소는 불가]
                    }
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3254"); //출고 Lot ID를 선택을 하신 후 버튼을 클릭해주십시오.
                    return;
                }
            }
            catch (Exception ex)
            {
                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
                return;
            }
        }

        //2018.07.02
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgOutDetail);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2020.02.11
        private void btnExcelNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgOutDetailNew);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2018.12.17
        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationWindingLotSearch()) return;

            Util.gridClear(dgGBT);

            Get_GBT();
        }

        //2018.12.17
        private void btnExcelG_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgGBT);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Button

        #region Text
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    string sArea = string.Empty;

                    // 동 선택 확인
                    if (cboAreaAll.SelectedIndex < 0 || cboAreaAll.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {                        
                        ms.AlertWarning("SFU1499"); //동을 선택하세요
                        return;
                    }
                    else
                    {
                        sArea = cboAreaAll.SelectedValue.ToString();
                    }

                    string sPalletID = string.Empty;
                    sPalletID = txtPalletID.Text.ToString().Trim();

                    if (sPalletID == null)
                    {
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Pallet ID 가 없습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        ms.AlertWarning("SFU3350"); //입력오류 : PALLETID 를 입력해 주세요.
                        return;
                    }

                    Scan_Process(sPalletID, sArea);

                }
                catch (Exception ex)
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    Util.Alert(ex.Message);
                    return;
                }
            }
        }
        #endregion Text

        #region Grid
        private void dgOutHist_Choice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgOutHist.SelectedIndex = idx;

                Search_PalletInfo(idx);
            }
        }

        //2020.02.11
        private void dgOutHistNew_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "RCV_ISS_ID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (e.Cell.Column.Name == "MOVE_PERIOD")
                    {
                        double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                        if (nDiff <= 3)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                        else if (nDiff > 3 && nDiff <= 7)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }

                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        //2020.02.11
        private void dgOutHistNew_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgOutHistNew.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("RCV_ISS_ID"))
                    {
                        string sRCVISSID = Util.NVC(DataTableConverter.GetValue(dgOutHistNew.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));

                        Out_HistDetailNew(sRCVISSID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion Grid

        #region Combo
        private void cboPrdtClass_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                CommonCombo combo = new CommonCombo();
                C1ComboBox[] cboProductParent = { cboAreaAll3, cboProductModel, cboPrdtClass };
                combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: null);
            }
            catch
            {

            }
        }
        #endregion Combo

        private void wndShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER window = sender as CMM_SHIFT_USER;
            if (window.DialogResult == MessageBoxResult.OK)
            {
                txtUserID.Tag = window.USERID;
                txtUserID.Text = window.USERNAME;
            }
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Report_Multi printPopUp = sender as LGC.GMES.MES.PACK001.Report_Multi;
                if (printPopUp.DialogResult == MessageBoxResult.OK)
                {
                    
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        //2018.12.17
        private bool ValidationWindingLotSearch()
        {
            TimeSpan timeSpan = dtpDateTo1.SelectedDateTime.Date - dtpDateFrom1.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                Util.MessageValidation("SFU3569");
                return false;
            }

            if (timeSpan.Days > 7)
            {
                Util.MessageValidation("SFU3567");
                return false;
            }

            return true;
        }

    }
}
