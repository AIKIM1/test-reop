/*************************************************************************************
 Created Date : 2020.09.17
      Creator : 염규범
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.17  염규범S : Initial Created.  
  2022.04.06  이태규  : C1ComboBox -> MultiSelectionBox 변경처리(창고 ID, Rack ID)
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_073 : UserControl, IWorkArea
    {
        CommonCombo _combo = new CommonCombo();

        #region [ init ]
        public PACK001_073()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            iniCombo();

            this.Loaded -= C1Window_Loaded;
        }

        private void iniCombo()
        {
            try
            {
                //기존 소스 start--------------------------------------------------------------------------------------------------------------------------
                ////동
                //C1ComboBox[] cboAreaChild = { cboEqsg, cboPrjt, cboWhId, cboRackId };
                //String[] sFiltercboArea = { Area_Type.PACK };
                //_combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_PACK", cbChild: cboAreaChild);

                //C1ComboBox[] cboEqsgParent = { cboArea };
                //C1ComboBox[] cboEqsgChild = { cboPrjt };
                //_combo.SetCombo(cboEqsg, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgChild, cbParent: cboEqsgParent, sCase: "cboEquipmentSegment");

                ////PRJT
                //C1ComboBox[] cboModelShotParent = { cboArea, cboEqsg };
                //_combo.SetCombo(cboPrjt, CommonCombo.ComboStatus.ALL, cbParent: cboModelShotParent, sCase: "cboPRJModelPack");

                ////창고위치 (WH_ID)       
                //C1ComboBox[] cboWHIDRightParent = { cboArea };
                //C1ComboBox[] cboWHIDRightChild = { cboRackId };
                //_combo.SetCombo(cboWhId, CommonCombo.ComboStatus.ALL, cbParent: cboWHIDRightParent, cbChild: cboWHIDRightChild, sCase: "cboWHID");

                ////제품코드(RACK ID)
                //C1ComboBox[] cboRackIDRightParent = { cboWhId };
                //_combo.SetCombo(cboRackId, CommonCombo.ComboStatus.ALL, cbParent: cboRackIDRightParent, sCase: "cboRackId");

                ////Hold 여부
                //String[] sFilter2 = { "", "HOLD_STATUS" };
                //_combo.SetCombo(cboHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

                //String[] sFilterBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
                //_combo.SetCombo(cboBizType, CommonCombo.ComboStatus.ALL, sFilter: sFilterBizType, sCase: "cboPrdtClassByProcId");
                //기존 소스 end--------------------------------------------------------------------------------------------------------------------------

                //수정 소스 start------------------------------------------------------------------------------------------------------------------------
                //동
                C1ComboBox[] cboAreaChild = { cboEqsg, cboPrjt/*, cboWhId, cboRackId*/};
                String[] sFiltercboArea = { Area_Type.PACK };
                _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, sFilter: sFiltercboArea, sCase: "AREA_PACK", cbChild: cboAreaChild);

                C1ComboBox[] cboEqsgParent = { cboArea };
                C1ComboBox[] cboEqsgChild = { cboPrjt };
                _combo.SetCombo(cboEqsg, CommonCombo.ComboStatus.ALL, cbChild: cboEqsgChild, cbParent: cboEqsgParent, sCase: "cboEquipmentSegment");

                //PRJT
                C1ComboBox[] cboModelShotParent = { cboArea, cboEqsg };
                _combo.SetCombo(cboPrjt, CommonCombo.ComboStatus.ALL, cbParent: cboModelShotParent, sCase: "cboPRJModelPack");

                //창고위치 (WH_ID)       
                //C1ComboBox[] cboWHIDRightParent = { cboArea };
                //C1ComboBox[] cboWHIDRightChild = { cboRackId };
                //_combo.SetCombo(cboWhId, CommonCombo.ComboStatus.ALL, cbParent: cboWHIDRightParent, cbChild: cboWHIDRightChild, sCase: "cboWHID");
                SetWhId(mboWhId, cb: cboArea);

                ////제품코드(RACK ID)
                //C1ComboBox[] cboRackIDRightParent = { cboWhId };
                //_combo.SetCombo(cboRackId, CommonCombo.ComboStatus.ALL, cbParent: cboRackIDRightParent, sCase: "cboRackId");
                SetRackId(mboRackId, msb: mboWhId, cb: cboArea);

                //Hold 여부
                String[] sFilter2 = { "", "HOLD_STATUS" };
                _combo.SetCombo(cboHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

                String[] sFilterBizType = { "", LoginInfo.CFG_AREA_ID, "", Area_Type.PACK, "" };
                _combo.SetCombo(cboBizType, CommonCombo.ComboStatus.ALL, sFilter: sFilterBizType, sCase: "cboPrdtClassByProcId");
                //기존 소스 end--------------------------------------------------------------------------------------------------------------------------


                //C1ComboBox[] cboParnetMboEqsg = { cboArea };
                //SetcboEQSG(mboEqsg, cb: cboParnetMboEqsg);

                //C1ComboBox[] cboParnetMboPanCakeGrid = { cboArea };
                //SetMboPancakeGrId(mboPanCakeGrId, cb: cboParnetMboPanCakeGrid);

                //C1ComboBox[] cboParnetMboSnapWhId = { cboArea };
                //SetmboWhId(mboWhId, cb: cboParnetMboSnapWhId);

                //C1ComboBox[] cboParnetMboRackId = { cboArea };
                //MultiSelectionBox[] mbParnetMboRackId = { mboWhId };
                //SetmboRackId(mboRackId, msb: mbParnetMboRackId, cb: cboParnetMboRackId);

                //SetmboBizType(mboBizType);
                //String[] sFilter2 = { "", "HOLD_STATUS" };
                //_combo.SetCombo(cboHold, CommonCombo.ComboStatus.ALL, sFilter: sFilter2, sCase: "COMMCODES");

                cboArea.SelectedValueChanged += cboArea_SelectedValueChanged;
                //mboWhId.SelectionChanged += mboWhId_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ WPF Event ]

        #region ( 전산재고 Header )

        #region < 동 >
        private void cboArea_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboArea.SelectedValue == null) return;

                    //C1ComboBox[] cboParnetMboEqsg = { cboArea };
                    //SetcboEQSG(mboEqsg, cb: cboParnetMboEqsg);

                    //C1ComboBox[] cboParnetMboWhId = { cboArea };
                    //SetmboWhId(mboWhId, cb: cboParnetMboWhId);

                    //C1ComboBox[] cboParnetMboPanCakeGrid = { cboArea };
                    //SetMboPancakeGrId(mboPanCakeGrId, cb: cboParnetMboPanCakeGrid);
                    SetWhId(mboWhId, cb: cboArea);
                    SetRackId(mboRackId, msb: mboWhId, cb: cboArea);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >

        private void cboEqsg_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //C1ComboBox[] cb = { cboArea };
                    //MultiSelectionBox[] mb = { mboEqsg };
                    //SetMboPrjt(mboPrjt, mb, cb);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < WH ID >
        private void mboWhId_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //C1ComboBox[] cboParnetMboRackId = { cboArea };
                    //MultiSelectionBox[] mbParnetMboRackId = { mboWhId };
                    //SetmboRackId(mboRackId, msb: mbParnetMboRackId, cb: cboParnetMboRackId);
                    SetRackId(mboRackId, msb: mboWhId, cb: cboArea);
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion


        #endregion

        #endregion

        #region [ Biz Caller ] 

        #region < 동 호출 >
        /// <summary>
        /// CWA 경우 DB 분리로 인해서, 자신의 동만 보이도록 처리,
        /// 오창의 경우, DB 분리가 되지 않아서, 모든 곳을 검색할수 있도록 콤보 선택 처리 
        /// </summary>
        private void SetAreaByShopId(C1ComboBox cb)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_AREA_BY_AREATYPE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";
                    dt.Columns.Add("CBO_CODE", typeof(string));
                    dt.Columns.Add("CBO_NAME", typeof(string));

                    DataRow dtDr = dt.NewRow();
                    dtDr["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr);

                    cb.ItemsSource = DataTableConverter.Convert(dt);
                    cb.SelectedIndex = 0;
                }
                else
                {
                    DataRow cbDr = dtResult.NewRow();
                    cbDr["CBO_CODE"] = "";
                    cbDr["CBO_NAME"] = "-SELECT-";
                    dtResult.Rows.InsertAt(cbDr, 0);

                    cb.ItemsSource = DataTableConverter.Convert(dtResult);


                    cb.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < 라인 콤보 생성 >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, AREAID
        /// Event : cboSnapArea_SelectedValueChanged
        /// </summary>
        private void SetcboEQSG(MultiSelectionBox msbEqsg, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {

            try
            {

                string sSelectedValue = msbEqsg.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);


                new ClientProxy().ExecuteService("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        if (dtResult.Rows.Count > 0)
                        {

                            msbEqsg.ItemsSource = DataTableConverter.Convert(dtResult);

                            for (int i = 0; i < dtResult.Rows.Count; i++)
                            {
                                if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                                {
                                    for (int j = 0; j < sSelectedList.Length; j++)
                                    {
                                        if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                        {
                                            msbEqsg.Check(i);
                                            break;
                                        }
                                    }
                                }
                                else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                                {
                                    msbEqsg.Check(i);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            msbEqsg.ItemsSource = DataTableConverter.Convert(dtResult);
                        }

                    }
                    catch (Exception ex2)
                    {
                        Util.MessageException(ex2);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region < PRJT 콤보 생성 - S05, CELL 의 경우 S05가 없기 때문에 S01 > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// CELL 의 경우 검색 조건을 Optional 조건을 하기 위해서, CELL 구분자로 해놓고, NAME을 가지고 
        /// 분기 처리해서, 사용 예정
        /// INDATA : SHOPID, AREAID, EQSGID, SYSTEM_ID, USERID
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged
        /// Fixed : SYSTEM_ID
        /// </summary>
        private void SetMboPrjt(MultiSelectionBox mboModel, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                string sSelectedValue = mboModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    mboModel.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    mboModel.ItemsSource = DataTableConverter.Convert(dtResult);
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < 모델 콤보 생성 - S01 > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : SHOPID, AREAID, EQSGID, SYSTEM_ID, USERID
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged
        /// Fixed : SYSTEM_ID
        /// </summary>
        private void SetmboBizType(MultiSelectionBox mboModel)
        {
            try
            {
                string sSelectedValue = mboModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_UI_PRDT_CLSS_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO_WITHOUT_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    mboModel.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                        {
                            mboModel.Check(i);
                            break;
                        }
                    }
                }
                else
                {
                    mboModel.ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < PRODUCT > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, SHOPID, AREAID, EQSGID, PROCID, MODELID, AREA_TYPE_CODE, PRDT_CLSS_CODE 
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged, cboSnapProc_SelectionChanged, cboSnapModel_SelectionChanged, cboSnapBizType_SelectionChanged
        /// Optional : PROCID, MODELID, PRDT_CLSS_CODE
        /// Fixed : AREA_TYPE_CODE
        /// </summary>
        private void SetcboProduct(MultiSelectionBox cboProd, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                string sSelectedValue = cboProd.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["PROCID"] = msb[1] == null ? null : msb[1].SelectedItemsToString;
                dr["MODLID"] = msb[2] == null ? null : msb[2].SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PRDT_CLSS_CODE"] = msb[3] == null ? null : msb[3].SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboProd.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboProd.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    cboProd.ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < 제공 상태 >
        private void SetcboWipState(MultiSelectionBox cboWipState)
        {
            try
            {
                string sSelectedValue = cboWipState.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WIPSTAT_WIPSEARCH";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {

                    cboWipState.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboWipState.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        //#region < WH ID > 
        ///// <summary>
        ///// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        ///// INDATA : 
        ///// Event : 
        ///// Optional : 
        ///// Fixed : 
        ///// </summary>
        //private void SetmboWhId(MultiSelectionBox mboWhId, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        //{
        //    try
        //    {
        //        string sSelectedValue = mboWhId.SelectedItemsToString;
        //        string[] sSelectedList = sSelectedValue.Split(',');

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("SHOPID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
        //        dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO_PACK", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (dtResult.Rows.Count > 0)
        //        {
        //            mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);

        //            for (int i = 0; i < dtResult.Rows.Count; i++)
        //            {
        //                if (sSelectedList.Length > 0 && sSelectedList[0] != "")
        //                {
        //                    for (int j = 0; j < sSelectedList.Length; j++)
        //                    {
        //                        if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
        //                        {
        //                            mboWhId.Check(i);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //#endregion

        #region WHID 조회
        //창고 ID eventHandler 추가: 20220406_이태규
        private void SetWhId(MultiSelectionBox mboWhId, C1ComboBox cb = null) 
        {
            try
            {
                string sSelectedValue = mboWhId.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb == null ? null : cb.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WHID_CBO_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    mboWhId.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    mboWhId.ItemsSource = DataTableConverter.Convert(dtResult);
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        //#region < RACK ID > 
        ///// <summary>
        ///// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        ///// INDATA : 
        ///// Event : 
        ///// Optional : 
        ///// Fixed : 
        ///// </summary>
        //private void SetmboRackId(MultiSelectionBox mboRackId, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        //{
        //    try
        //    {
        //        string sSelectedValue = mboRackId.SelectedItemsToString;
        //        string[] sSelectedList = sSelectedValue.Split(',');

        //        DataTable RQSTDT = new DataTable();
        //        RQSTDT.TableName = "RQSTDT";
        //        RQSTDT.Columns.Add("LANGID", typeof(string));
        //        RQSTDT.Columns.Add("AREAID", typeof(string));
        //        RQSTDT.Columns.Add("WHID", typeof(string));

        //        DataRow dr = RQSTDT.NewRow();
        //        dr["LANGID"] = LoginInfo.LANGID;
        //        dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
        //        dr["WHID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;

        //        RQSTDT.Rows.Add(dr);

        //        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACK_RACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

        //        if (dtResult.Rows.Count > 0)
        //        {
        //            mboRackId.ItemsSource = DataTableConverter.Convert(dtResult);

        //            for (int i = 0; i < dtResult.Rows.Count; i++)
        //            {
        //                if (sSelectedList.Length > 0 && sSelectedList[0] != "")
        //                {
        //                    for (int j = 0; j < sSelectedList.Length; j++)
        //                    {
        //                        if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
        //                        {
        //                            mboRackId.Check(i);
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            mboRackId.ItemsSource = DataTableConverter.Convert(dtResult);

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        //#endregion

        #region  RACK ID 조회
        //RACK ID eventHandler 추가: 20220406_이태규
        private void SetRackId(MultiSelectionBox mboRackId, MultiSelectionBox msb = null, C1ComboBox cb = null)
        {
            try
            {
                string sSelectedValue = mboRackId.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cb == null ? null : cb.SelectedValue.ToString();
                //dr["WHID"] = msb == null ? null : msb.SelectedItemsToString;
                dr["WHID"] = Util.NVC(msb.SelectedItemsToString) == "" ? null : msb.SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PACK_RACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                //if (dtResult.Rows.Count > 0)
                //{
                mboRackId.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                mboRackId.Check(i);
                                break;
                            }
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }
        #endregion

        #region  < PANCAKE_GR_ID >
        private void SetMboPancakeGrId(MultiSelectionBox msbPancakeGrId, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            string sSelectedValue = msbPancakeGrId.SelectedItemsToString;
            string[] sSelectedList = sSelectedValue.Split(',');

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
            RQSTDT.Rows.Add(dr);


            new ClientProxy().ExecuteService("DA_PRD_SEL_PANCAKE_GR_FOR_PACK", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
            {
                try
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count > 0)
                    {

                        msbPancakeGrId.ItemsSource = DataTableConverter.Convert(dtResult);

                        for (int i = 0; i < dtResult.Rows.Count; i++)
                        {
                            if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                            {
                                for (int j = 0; j < sSelectedList.Length; j++)
                                {
                                    if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                    {
                                        msbPancakeGrId.Check(i);
                                        break;
                                    }
                                }
                            }
                            else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                            {
                                msbPancakeGrId.Check(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        msbPancakeGrId.ItemsSource = DataTableConverter.Convert(dtResult);
                    }

                }
                catch (Exception ex2)
                {
                    Util.MessageException(ex2);
                }
            });
        }
        #endregion

        #endregion

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetList();
        }

        private void SetList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (cboArea.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1499");
                    return;
                }

                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PJT", typeof(string));
                RQSTDT.Columns.Add("BIZTYPE", typeof(string));
                RQSTDT.Columns.Add("RACK_ID", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("PANCAKE_GR_ID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("HOLD", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();

                if (!string.IsNullOrEmpty(txtLotId.Text.ToString()))
                {
                    dr["LOTID"] = txtLotId.Text.ToString();
                }
                else if (!string.IsNullOrEmpty(txtPanCakeGrId.Text.ToString()))
                {
                    dr["PANCAKE_GR_ID"] = txtPanCakeGrId.Text.ToString();
                }
                else
                {
                    dr["EQSGID"] = cboEqsg.SelectedValue.ToString();
                    dr["PJT"] = cboPrjt.SelectedValue.ToString();
                    dr["BIZTYPE"] = cboBizType.SelectedValue.ToString();
                    //dr["RACK_ID"] = cboRackId.SelectedValue.ToString();
                    //dr["WH_ID"] = cboWhId.SelectedValue.ToString();
                    dr["RACK_ID"] = mboRackId.SelectedItemsToString;//C1ComboBox -> MultiSelectionBox 변경처리: 20220406_이태규 
                    dr["WH_ID"] = mboWhId.SelectedItemsToString;//C1ComboBox -> MultiSelectionBox 변경처리: 20220406_이태규
                    dr["HOLD"] = cboHold.SelectedValue.ToString();
                }

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_SEL_WH_WIP_PACK", "INDATA", "OUTDATA", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    Util.GridSetData(dgList, dtResult, FrameOperation, true);

                    loadingIndicator.Visibility = Visibility.Collapsed;
                });

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #region [ 엑셀 다운 로드 ]
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void txtPanCakeGrId_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    SetList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }
        
    }
}