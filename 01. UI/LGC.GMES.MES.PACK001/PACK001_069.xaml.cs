/*************************************************************************************
 Created Date : 2020.07.23
      Creator : 염규범
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.23  염규범S : Initial Created.
**************************************************************************************/

using System;
using System.Data;
using System.Web.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.COM001;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_069 : UserControl, IWorkArea
    {
        CommonCombo _combo = new CommonCombo();

        #region [ init ]
        public PACK001_069()
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

                SetAreaByShopId(cboSnapArea);
                //SetAreaByShopId(cboRsltArea);
                SetAreaByShopId(cboCompareArea);


                cboSnapArea.SelectedValueChanged += cboSnapArea_SelectedValueChanged;
                cboRsltArea.SelectedValueChanged += cboRlstArea_SelectedValueChanged;
                cboCompareArea.SelectedValueChanged += cboCompareArea_SelectedValueChanged;
                
                cboSnapEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;
                cboSnapProc.SelectionChanged += cboSnapProc_SelectionChanged;
                cboSnapModel.SelectionChanged += cboSnapModel_SelectionChanged;
                cboSnapBizType.SelectionChanged += cboSnapBizType_SelectionChanged;

                
                //cboRsltEqsg.SelectionChanged += cboRsltEqsg_SelectionChanged;
                //cboRsltProc.SelectionChanged += cboRsltProc_SelectionChanged;
                //cboRsltModel.SelectionChanged += cboRsltModel_SelectionChanged;
                //cboRsltBizType.SelectionChanged += cboRsltBizType_SelectionChanged;

                
                //cboCompareEqsg.SelectionChanged += cboCompareEqsg_SelectionChanged;
                //cboCompareProc.SelectionChanged += cboCompareProc_SelectionChanged;
                cboCompareModel.SelectionChanged += cboCompareModel_SelectionChanged;
                cboCompareBizType.SelectionChanged += cboCompareBizType_SelectionChanged;

                ldpSnapMonthShot.SelectedDataTimeChanged += ldpSnapMonthShot_SelectedDataTimeChanged;
                ldpRsltMonthShot.SelectedDataTimeChanged += ldpRsltMonthShot_SelectedDataTimeChanged;
                ldpCompareMonthShot.SelectedDataTimeChanged += ldpCompareMonthShot_SelectedDataTimeChanged;

                string strAreagCase = "AREA_PACK";
                string[] sAreaFilter = { Area_Type.PACK };
                C1ComboBox[] cbAreaChild = { cboRsltEqsg };
                _combo.SetCombo(cboRsltArea, CommonCombo.ComboStatus.NONE, cbChild: cbAreaChild, sFilter: sAreaFilter, sCase: strAreagCase);

                object[] objStockSeqRsltParent = { cboRsltArea, ldpRsltMonthShot };
                String[] sFilterobjStockSeqRsltParentAll = { "" };
                _combo.SetComboObjParent(cboRsltStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqRsltParent, sFilter: sFilterobjStockSeqRsltParentAll);

                string strEqsgCase = "EQUIPMENTSEGMENT";
                C1ComboBox[] cbEqsgParent = { cboRsltArea };
                _combo.SetCombo(cboRsltEqsg, CommonCombo.ComboStatus.ALL, cbParent: cbEqsgParent, sCase: strEqsgCase);


                string[] sFilter = { "PACK_WIPSTAT_WIPSEARCH" };
                string strCase = "COMMCODE";

                _combo.SetCombo(cboSnapWipstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: strCase);
                //_combo.SetCombo(cboRsltWipstat, CommonCombo.ComboStatus.ALL, sFilter: sFilter, sCase: strCase);

                //cboSnapArea.SelectedIndex = 0;

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [ WPF Event ]

        #region ( 전산재고 Header )
        
        #region < 동 >
        private void cboSnapArea_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboSnapArea.ItemsSource == null) return;
                    
                    C1ComboBox[] cb = { cboSnapArea };
                    MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    SetcboEQSG(cboSnapEqsg, msb: msb, cb:cb );
                    SetcboProcess(cboSnapProc, msb: msb, cb:cb);
                    SetcboProductModel(cboSnapModel, msb: msb, cb: cb);
                    SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    object[] objStockSeqShotParent = { cboSnapArea, ldpSnapMonthShot };
                    String[] sFilterSnapAll = { "" };
                    _combo.SetComboObjParent(cboSnapStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterSnapAll);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >

        private void cboSnapEqsg_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboSnapEqsg.SelectionChanged -= cboSnapEqsg_SelectionChanged;

                    C1ComboBox[] cb = { cboSnapArea };
                    MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    SetcboProcess(cboSnapProc, msb: msb, cb: cb);
                    SetcboProductModel(cboSnapModel, msb: msb, cb: cb);
                    SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    cboSnapEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 공정 >
        private void cboSnapProc_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboSnapProc.SelectionChanged -= cboSnapProc_SelectionChanged;

                    C1ComboBox[] cb = { cboSnapArea };
                    MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    SetcboBizType(cboSnapBizType, msb: msb, cb: cb);
                    SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    cboSnapProc.SelectionChanged += cboSnapProc_SelectionChanged;

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 모델 > 
        private void cboSnapModel_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboSnapModel.SelectionChanged -= cboSnapModel_SelectionChanged;

                    C1ComboBox[] cb = { cboSnapArea };
                    MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    cboSnapModel.SelectionChanged += cboSnapModel_SelectionChanged;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < Biz Type >
        private void cboSnapBizType_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboSnapBizType.SelectionChanged -= cboSnapBizType_SelectionChanged;
                    
                    C1ComboBox[] cb = { cboSnapArea };
                    MultiSelectionBox[] msb = { cboSnapEqsg, cboSnapProc, cboSnapModel, cboSnapBizType };

                    SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

                    cboSnapBizType.SelectionChanged += cboSnapBizType_SelectionChanged;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion

        #region ( 실물재고 Header )

        #region < 동 >
        private void cboRlstArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboRsltArea.ItemsSource == null || string.IsNullOrWhiteSpace(cboRsltArea.SelectedValue.ToString())) return;

                    object[] objStockSeqRsltParent = { cboRsltArea, ldpRsltMonthShot };
                    String[] sFilterobjStockSeqRsltParentAll = { "" };
                    _combo.SetComboObjParent(cboRsltStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqRsltParent, sFilter: sFilterobjStockSeqRsltParentAll);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >
        //private void cboRsltEqsg_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.BeginInvoke(new System.Action(() =>
        //        {
        //            cboRsltEqsg.SelectionChanged -= cboRsltEqsg_SelectionChanged;

        //            C1ComboBox[] cb = { cboRsltArea };
        //            MultiSelectionBox[] msb = { cboRsltEqsg, cboRsltProc, cboRsltModel, cboRsltBizType };

        //            SetcboProcess(cboRsltProc, msb: msb, cb: cb);
        //            SetcboProductModel(cboRsltModel, msb: msb, cb: cb);
        //            SetcboBizType(cboRsltBizType, msb: msb, cb: cb);
        //            SetcboProduct(cboRsltProduct, msb: msb, cb: cb);

        //            cboRsltEqsg.SelectionChanged += cboRsltEqsg_SelectionChanged;
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #region < 공정 >
        //private void cboRsltProc_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.BeginInvoke(new System.Action(() =>
        //        {
        //            cboRsltProc.SelectionChanged -= cboRsltProc_SelectionChanged;

        //            C1ComboBox[] cb = { cboRsltArea };
        //            MultiSelectionBox[] msb = { cboRsltEqsg, cboRsltProc, cboRsltModel, cboRsltBizType };

        //            SetcboBizType(cboRsltBizType, msb: msb, cb: cb);
        //            SetcboProduct(cboRsltProduct, msb: msb, cb: cb);

        //            cboRsltProc.SelectionChanged += cboRsltProc_SelectionChanged;

        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #region < 모델 >
        //private void cboRsltModel_SelectionChanged(object sender, EventArgs e)
        //{
        //    this.Dispatcher.BeginInvoke(new System.Action(() =>
        //    {
        //        cboRsltModel.SelectionChanged -= cboRsltModel_SelectionChanged;

        //        C1ComboBox[] cb = { cboRsltArea };
        //        MultiSelectionBox[] msb = { cboRsltEqsg, cboRsltProc, cboRsltModel, cboRsltBizType };

        //        SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

        //        cboRsltModel.SelectionChanged += cboRsltModel_SelectionChanged;
        //    }));
        //}
        #endregion

        #region < Biz Type >
        //private void cboRsltBizType_SelectionChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.Dispatcher.BeginInvoke(new System.Action(() =>
        //        {
        //            cboRsltBizType.SelectionChanged -= cboRsltBizType_SelectionChanged;

        //            C1ComboBox[] cb = { cboRsltArea };
        //            MultiSelectionBox[] msb = { cboRsltEqsg, cboRsltProc, cboRsltModel, cboRsltBizType };

        //            SetcboProduct(cboSnapProduct, msb: msb, cb: cb);

        //            cboRsltBizType.SelectionChanged += cboRsltBizType_SelectionChanged;
        //        }));
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.MessageException(ex);
        //    }
        //}
        #endregion

        #endregion

        #region < 재고 비교 >
        #region < 동 >
        private void cboCompareArea_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    if (cboCompareArea.ItemsSource == null) return;

                    C1ComboBox[] cb = { cboCompareArea };
                    MultiSelectionBox[] msb = { null, null, cboCompareModel, cboCompareBizType };

                    //SetcboEQSG(cboCompareEqsg, msb: msb, cb: cb);
                    //SetcboProcess(cboCompareProc, msb: msb, cb: cb);
                    SetcboProductModel(cboCompareModel, msb: msb, cb: cb);
                    SetcboBizType(cboCompareBizType, msb: msb, cb: cb);
                    SetcboProduct(cboCompareProduct, msb: msb, cb: cb);

                    object[] objStockSeqCompareParent = { cboCompareArea, ldpCompareMonthShot };
                    String[] sFilterCompareAll = { "" };
                    _combo.SetComboObjParent(cboCompareStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqCompareParent, sFilter: sFilterCompareAll);

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 라인 >
        private void cboCompareEqsg_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //cboCompareEqsg.SelectionChanged -= cboCompareEqsg_SelectionChanged;

                    C1ComboBox[] cb = { cboCompareArea };
                    MultiSelectionBox[] msb = { null,null, cboCompareModel, cboCompareBizType };

                    //SetcboProcess(cboCompareProc, msb: msb, cb: cb);
                    SetcboProductModel(cboCompareModel, msb: msb, cb: cb);
                    SetcboBizType(cboCompareBizType, msb: msb, cb: cb);
                    SetcboProduct(cboCompareProduct, msb: msb, cb: cb);

                    //cboCompareEqsg.SelectionChanged += cboCompareEqsg_SelectionChanged;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 공정 >
        private void cboCompareProc_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    //cboCompareProc.SelectionChanged -= cboCompareProc_SelectionChanged;

                    C1ComboBox[] cb = { cboCompareArea };
                    MultiSelectionBox[] msb = { null,null, cboCompareModel, cboCompareBizType };

                    SetcboBizType(cboCompareBizType, msb: msb, cb: cb);
                    SetcboProduct(cboCompareProduct, msb: msb, cb: cb);

                    //cboCompareProc.SelectionChanged += cboCompareProc_SelectionChanged;

                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 모델 >
        private void cboCompareModel_SelectionChanged(object sender, EventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                cboCompareModel.SelectionChanged -= cboCompareModel_SelectionChanged;

                C1ComboBox[] cb = { cboCompareArea };
                MultiSelectionBox[] msb = { null,null,cboCompareModel, cboCompareBizType };

                SetcboProduct(cboCompareProduct, msb: msb, cb: cb);

                cboCompareModel.SelectionChanged += cboCompareModel_SelectionChanged;
            }));
        }
        #endregion

        #region < Biz Type >
        private void cboCompareBizType_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new System.Action(() =>
                {
                    cboCompareBizType.SelectionChanged -= cboCompareBizType_SelectionChanged;

                    C1ComboBox[] cb = { cboCompareArea };
                    MultiSelectionBox[] msb = {null,null,cboCompareModel, cboCompareBizType };

                    SetcboProduct(cboCompareProduct, msb: msb, cb: cb);

                    cboCompareBizType.SelectionChanged += cboCompareBizType_SelectionChanged;
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
                    dtDr["CBO_CODE"] = "";
                    dtDr["CBO_NAME"] = "-SELECT-";
                    dt.Rows.Add(dtDr);

                    DataRow dtDr2 = dt.NewRow();
                    dtDr2["CBO_CODE"] = LoginInfo.CFG_AREA_ID;
                    dtDr2["CBO_NAME"] = LoginInfo.CFG_AREA_NAME;
                    dt.Rows.Add(dtDr2);

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
                //cboEqsg.SelectionChanged -= cboSnapEqsg_SelectionChanged;
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
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0) {

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
                //cboEqsg.SelectionChanged += cboSnapEqsg_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 공정 콤보 생성 >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, EQSGID
        /// Event : cboSnapEqsg_SelectionChanged
        /// </summary>
        private void SetcboProcess(MultiSelectionBox msbProc, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboProc.SelectionChanged -= cboSnapProc_SelectionChanged;
                string sSelectedValue = msbProc.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PROCESS_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0) {

                    msbProc.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    msbProc.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_PROC_ID)
                        {
                            msbProc.Check(i);
                            break;
                        }
                    }
                }
                //cboProc.SelectionChanged += cboSnapProc_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region < 모델 콤보 생성 > 
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : SHOPID, AREAID, EQSGID, SYSTEM_ID, USERID
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged
        /// Fixed : SYSTEM_ID
        /// </summary>
        private void SetcboProductModel(MultiSelectionBox cboModel, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboModel.SelectionChanged -= cboSnapModel_SelectionChanged;

                string sSelectedValue = cboModel.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("SYSTEM_ID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["SYSTEM_ID"] = LoginInfo.SYSID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRJMODEL_PACK_CBO", "RQSTDT", "RSLTDT", RQSTDT);
                
                if(dtResult.Rows.Count > 0)
                {
                    cboModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboModel.Check(i);
                                    break;
                                }
                            }
                        }
                        else if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == LoginInfo.CFG_EQSG_ID)
                        {
                            cboModel.Check(i);
                            break;
                        }
                    }
                }
                //cboModel.SelectionChanged += cboSnapModel_SelectionChanged;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region < Biz Type >
        /// <summary>
        /// MultiSelectionBox 의 SelectionChanged 의 경우, 공통 모듈화 내용이 없기에, 해당 내용 사용
        /// INDATA : LANGID, SHOPID, AREAID, EQSGID, PROCID, AREA_TYPE_CODE
        /// Event : cboSnapArea_SelectedValueChanged, cboSnapEqsg_SelectionChanged, cboSnapProc_SelectionChanged
        /// Optional : PROCID
        /// Fixed : AREA_TYPE_CODE
        /// </summary>
        private void SetcboBizType(MultiSelectionBox cboBizType, MultiSelectionBox[] msb = null, C1ComboBox[] cb = null)
        {
            try
            {
                //cboBizType.SelectionChanged -= cboSnapBizType_SelectionChanged
                string sSelectedValue = cboBizType.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = cb[0] == null ? null : cb[0].SelectedValue.ToString();
                dr["EQSGID"] = msb[0] == null ? null : msb[0].SelectedItemsToString;
                dr["AREA_TYPE_CODE"] = Area_Type.PACK;
                dr["PROCID"] = msb[1] == null ? null : msb[1].SelectedItemsToString == "" ? null : msb[1].SelectedItemsToString;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCTTYPE_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
                {
                    cboBizType.ItemsSource = DataTableConverter.Convert(dtResult);

                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                        {
                            for (int j = 0; j < sSelectedList.Length; j++)
                            {
                                if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                                {
                                    cboBizType.Check(i);
                                    break;
                                }
                            }
                        }
                    }
                }
                //cboBizType.SelectionChanged += cboSnapBizType_SelectionChanged;
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
                dr["PRDT_CLSS_CODE"] = msb[3] == null  ? null : msb[3].SelectedItemsToString;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_PACK_MULTI_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count > 0)
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

                if(dtResult.Rows.Count > 0) { 

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

        #endregion

        private void btnSnapSearch_Click(object sender, RoutedEventArgs e)
        {
            SetListShot();
        }

        private void SetListShot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (cboSnapArea.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1499");
                    return;
                }

                if (Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }


                if (Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU1459");
                    return;
                }

                if (Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU8231");
                    return;
                }

                if (cboSnapStockSeqShot.SelectedValue == null)
                {
                    Util.MessageInfo("SFU2958");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_SEQNO"] = Util.GetCondition(cboSnapStockSeqShot);
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);
                dr["AREAID"] = Util.GetCondition(cboSnapArea);
                if (string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId)))
                {
                    dr["WIPSTAT"] = null;
                }
                else
                {
                    dr["WIPSTAT"] = Util.GetCondition(cboSnapWipstat);
                }
                   

                if (string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId )))
                {

                    dr["EQSGID"] = Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapEqsg.SelectedItemsToString);
                    dr["PROCID"] = Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapProc.SelectedItemsToString);
                    dr["PRODID"] = Util.ConvertEmptyToNull(cboSnapProduct.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapProduct.SelectedItemsToString);
                    dr["PRDTYPE"] = Util.ConvertEmptyToNull(cboSnapBizType.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapBizType.SelectedItemsToString);
                    dr["PRJT_ABBR_NAME"] = Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboSnapModel.SelectedItemsToString);
                }
                else
                {
                    dr["LOTID"] = string.IsNullOrWhiteSpace(Util.GetCondition(txtSnapLotId)) ? null : Util.GetCondition(txtSnapLotId);
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_SNAP", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListSnapStock, dtRslt, FrameOperation);
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

        private void btnRsltSearch_Click(object sender, RoutedEventArgs e)
        {
            SetListRslt();
        }

        private void SetListRslt()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                if (cboRsltArea.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1499");
                    return;
                }

                if (cboRsltEqsg.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }

                if (cboRsltStockSeqShot.SelectedValue == null)
                {
                    Util.MessageInfo("SFU2958");
                    return;
                }

                //if (Util.ConvertEmptyToNull(cboRsltModel.SelectedItemsToString) == null)
                //{
                //    Util.MessageInfo("SFU8231");
                //    return;
                //}

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("WIPSTAT", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("PROCID", typeof(string));
                //RQSTDT.Columns.Add("PRODID", typeof(string));
                //RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                //RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));
               


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_SEQNO"] = Util.GetCondition(cboRsltStockSeqShot);
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpRsltMonthShot);
                dr["AREAID"] = Util.GetCondition(cboRsltArea);

                if (string.IsNullOrWhiteSpace(txtRsltLotId.Text.ToString().Trim()))
                {

                    dr["EQSGID"] = string.IsNullOrWhiteSpace(cboRsltEqsg.SelectedValue.ToString()) ? null : cboRsltEqsg.SelectedValue.ToString();
                    //dr["EQSGID"] = Util.ConvertEmptyToNull(cboRsltEqsg.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboRsltEqsg.SelectedItemsToString);
                    //dr["PROCID"] = Util.ConvertEmptyToNull(cboRsltProc.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboRsltProc.SelectedItemsToString);
                    //dr["PRODID"] = Util.ConvertEmptyToNull(cboRsltProduct.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboRsltProduct.SelectedItemsToString);
                    //dr["PRDTYPE"] = Util.ConvertEmptyToNull(cboRsltBizType.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboRsltBizType.SelectedItemsToString);
                    //dr["PRJT_ABBR_NAME"] = Util.ConvertEmptyToNull(cboRsltModel.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboRsltModel.SelectedItemsToString);
                }
                else
                {
                    RQSTDT.Columns.Add("LOTID", typeof(string));
                    dr["LOTID"] = string.IsNullOrWhiteSpace(txtRsltLotId.ToString()) ? null : Util.GetCondition(txtRsltLotId);
                }

                

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_RSLT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListRsltStock, dtRslt, FrameOperation);
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

        private void ldpSnapMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboSnapArea.SelectedValue == null) return;

                object[] objStockSeqShotParent = { cboSnapArea, ldpSnapMonthShot };
                String[] sFilterSnapAll = { "" };
                _combo.SetComboObjParent(cboSnapStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqShotParent, sFilter: sFilterSnapAll);
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ldpRsltMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try 
            { 
            object[] objStockSeqRsltParent = { cboRsltArea, ldpRsltMonthShot };
            String[] sFilterobjStockSeqRsltParentAll = { "" };
            _combo.SetComboObjParent(cboRsltStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqRsltParent, sFilter: sFilterobjStockSeqRsltParentAll);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ldpCompareMonthShot_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cboCompareArea.SelectedValue == null) return;

                object[] objStockSeqCompareParent = { cboCompareArea, ldpCompareMonthShot };
                String[] sFilterCompareAll = { cboCompareArea.SelectedValue.ToString(),  };
                _combo.SetComboObjParent(cboCompareStockSeqShot, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objStockSeqCompareParent, sFilter: sFilterCompareAll);
                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetStockSeq(C1ComboBox cbo, String[] sFilter)
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_CMPL_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.ConvertEmptyToNull(sFilter[0]);
                dr["STCK_CNT_YM"] = Util.ConvertEmptyToNull(sFilter[1]);
                dr["STCK_CNT_CMPL_FLAG"] = Util.ConvertEmptyToNull(sFilter[2]);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListSnapStock.Rows[idx].DataItem;

                if (objRowIdx != null)
                {
                    if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                    {
                        DataTableConverter.SetValue(dgListSnapStock.Rows[idx].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgListSnapStock.Rows[idx].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExclude_SNAP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListSnapStock, "CHK");

                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                TextRange textRange = new TextRange(txtExcludeNote_SNAP.Document.ContentStart, txtExcludeNote_SNAP.Document.ContentEnd);

                if (string.IsNullOrEmpty(textRange.Text.Trim()))
                {
                    Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
                    return;
                }

                //전산재고 LOTID를 제외 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        string sSTCK_CNT_EXCL_FLAG = "Y";
                        Exclude_SNAP(sSTCK_CNT_EXCL_FLAG);
                    }
                }
                );
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Exclude_SNAP(string sSTCK_CNT_EXCL_FLAG)
        {
            try
            {
                this.dgListSnapStock.EndEdit();
                this.dgListSnapStock.EndEditRow(true);

                DataTable dtRSLT = ((DataView)dgListSnapStock.ItemsSource).Table;
                TextRange textRange = new TextRange(txtExcludeNote_SNAP.Document.ContentStart, txtExcludeNote_SNAP.Document.ContentEnd);

                //RQSTDT
                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_FLAG", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_USERID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_EXCL_NOTE", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();
                        dr["STCK_CNT_EXCL_FLAG"] = sSTCK_CNT_EXCL_FLAG;
                        dr["STCK_CNT_EXCL_USERID"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? LoginInfo.USERID : "";
                        dr["STCK_CNT_EXCL_NOTE"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? textRange.Text.ToString() : "";
                        dr["USERID"] = LoginInfo.USERID;

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_SNAP", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

                        SetListShot();
                        txtExcludeNote_SNAP.AppendText(string.Empty);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void CheckBox_Click_Rslt(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null) return;

                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;
                object objRowIdx = dgListRsltStock.Rows[idx].DataItem;

                if (objRowIdx != null)
                {
                    if (DataTableConverter.GetValue(objRowIdx, "CHK").Equals("True"))
                    {
                        DataTableConverter.SetValue(dgListRsltStock.Rows[idx].DataItem, "CHK", true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgListRsltStock.Rows[idx].DataItem, "CHK", false);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExclude_RSLT_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iRow = new Util().GetDataGridCheckFirstRowIndex(dgListRsltStock, "CHK");

                if (iRow < 0)
                {
                    Util.MessageValidation("SFU1632");  //선택된 LOT이 없습니다.
                    return;
                }

                TextRange textRange = new TextRange(txtExcludeNote_RSLT.Document.ContentStart, txtExcludeNote_RSLT.Document.ContentEnd);

                if (string.IsNullOrEmpty(textRange.Text.Trim()))
                {
                    Util.MessageValidation("SFU1590");  //비고를 입력해 주세요.
                    return;
                }

                //전산재고 LOTID를 제외 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4212"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result.ToString().Equals("OK"))
                    {
                        string sSTCK_CNT_EXCL_FLAG = "Y";
                        Exclude_RSLT(sSTCK_CNT_EXCL_FLAG);
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Exclude_RSLT(string sSTCK_CNT_EXCL_FLAG)
        {
            try
            {
                this.dgListRsltStock.EndEdit();
                this.dgListRsltStock.EndEditRow(true);

                DataTable dtRSLT = ((DataView)dgListRsltStock.ItemsSource).Table;
                TextRange textRange = new TextRange(txtExcludeNote_RSLT.Document.ContentStart, txtExcludeNote_RSLT.Document.ContentEnd);

                //RQSTDT
                DataSet inData = new DataSet();
                DataTable RQSTDT = inData.Tables.Add("RQSTDT");
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));

                for (int i = 0; i < dtRSLT.Rows.Count; i++)
                {
                    if (dtRSLT.Rows[i]["CHK"].ToString() == "True")
                    {
                        DataRow dr = RQSTDT.NewRow();
                        dr["STCK_CNT_YM"] = dtRSLT.Rows[i]["STCK_CNT_YM"].ToString();
                        dr["AREAID"] = dtRSLT.Rows[i]["AREAID"].ToString();
                        dr["STCK_CNT_SEQNO"] = dtRSLT.Rows[i]["STCK_CNT_SEQNO"].ToString();
                        dr["LOTID"] = dtRSLT.Rows[i]["LOTID"].ToString();             
                        dr["USERID"] = LoginInfo.USERID;
                        dr["USE_FLAG"] = "N";
                        dr["NOTE"] = sSTCK_CNT_EXCL_FLAG.Equals("Y") ? textRange.Text.ToString() : "";

                        RQSTDT.Rows.Add(dr);
                    }
                }

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("DA_PRD_UPD_STCK_CNT_RSLT_PACK", "RQSTDT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");//정상처리되었습니다.

                        SetListRslt();
                        txtExcludeNote_SNAP.AppendText(string.Empty);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }

                }, inData);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCompareSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                Util.gridClear(dgListCompareStock);

                if (cboCompareArea.SelectedValue == null)
                {
                    Util.MessageInfo("SFU1499");
                    return;
                }

                if (Util.ConvertEmptyToNull(cboCompareModel.SelectedItemsToString) == null)
                {
                    Util.MessageInfo("SFU8231");
                    return;
                }


                if (cboCompareStockSeqShot.SelectedValue == null)
                {
                    Util.MessageInfo("SFU2958");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                //RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));

                //

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STCK_CNT_SEQNO"] = Util.GetCondition(cboCompareStockSeqShot);
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpCompareMonthShot);
                dr["AREAID"] = Util.GetCondition(cboCompareArea);

                dr["PRDTYPE"] = Util.ConvertEmptyToNull(cboCompareBizType.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboCompareBizType.SelectedItemsToString);
                //dr["PRJT_NAME"] = Util.ConvertEmptyToNull(cboCompareModel.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboCompareModel.SelectedItemsToString);
                dr["PRJT_ABBR_NAME"] = Util.ConvertEmptyToNull(cboCompareModel.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboCompareModel.SelectedItemsToString);
                dr["PRODID"] = Util.ConvertEmptyToNull(cboCompareProduct.SelectedItemsToString) == null ? null : Util.ConvertEmptyToNull(cboCompareProduct.SelectedItemsToString);

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_CNT_SUMMARY", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgListCompareSummary, dtRslt, FrameOperation);

                
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

        private void dgListCompareChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                RadioButton rb = sender as RadioButton;

                //최초 체크시에만 로직 타도록 구현
                if (DataTableConverter.GetValue(rb.DataContext, "CHK").Equals("0"))
                {
                    Int64 iStckCntSeqNo = Util.NVC_Int(DataTableConverter.GetValue(rb.DataContext, "STCK_CNT_SEQNO"));
                    string strStckCntYM = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "STCK_CNT_YM"));
                    string strAreaId    = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "AREAID"));
                    string strProdId = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRODID"));
                    string strProdType  = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRDTYPE"));
                    string strPrjtName = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "PRJT_NAME"));
                    string strModeId    = Util.NVC(DataTableConverter.GetValue(rb.DataContext, "MODELID"));

                    

                    try
                    {
                        loadingIndicator.Visibility = Visibility.Visible;

                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(Int64));
                        RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                        RQSTDT.Columns.Add("AREAID", typeof(string));
                        RQSTDT.Columns.Add("PRODID", typeof(string));
                        RQSTDT.Columns.Add("PRDTYPE", typeof(string));
                        RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                        RQSTDT.Columns.Add("PRJT_ABBR_NAME", typeof(string));
                        

                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"]         = LoginInfo.LANGID;
                        dr["STCK_CNT_SEQNO"] = iStckCntSeqNo;
                        dr["STCK_CNT_YM"]    = strStckCntYM;
                        dr["AREAID"]         = strAreaId;
                        dr["PRODID"]         = strProdId;
                        dr["PRDTYPE"]        = strProdType;
                        dr["PRJT_NAME"]      = strPrjtName;
                        dr["PRJT_ABBR_NAME"] = strModeId.Equals("-") ? null : strModeId;

                        RQSTDT.Rows.Add(dr);

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STCK_CNT_SUMMARY_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);
                        Util.GridSetData(dgListCompareStock, dtRslt, FrameOperation);


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
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetListCompareDetail(string sProdID = null, string sEqsgID = null, string sProcID = null, string sElecType = null, string sPrjtName = null, string sAutoWhStckFlag = null, string sStckAdjFlag = null)
        {
            try
            {
                
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

        private void btnDegreeAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                COM001_011_STOCKCNT_START wndSTOCKCNT_START = new COM001_011_STOCKCNT_START();
                wndSTOCKCNT_START.FrameOperation = FrameOperation;

                if (wndSTOCKCNT_START != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = Convert.ToString(cboSnapArea.SelectedValue);
                    Parameters[1] = ldpSnapMonthShot.SelectedDateTime;

                    C1WindowExtension.SetParameters(wndSTOCKCNT_START, Parameters);

                    wndSTOCKCNT_START.Closed += new EventHandler(wndSTOCKCNT_START_Closed);

                    // 팝업 화면 숨겨지는 문제 수정.
                    this.Dispatcher.BeginInvoke(new Action(() => wndSTOCKCNT_START.ShowModal()));
                    wndSTOCKCNT_START.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDegreeClose_Click(object sender, RoutedEventArgs e)
        {
            //if (_sSTCK_CNT_CMPL_FLAG.Equals("Y"))
            //{
            //    Util.MessageValidation("SFU3499"); //마감된 차수입니다.
            //    return;
            //}

            //마감하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1276"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    DegreeClose();
                }
            }
            );
        }

        private void DegreeClose()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);
                dr["STCK_CNT_SEQNO"] = Convert.ToInt16(Util.GetCondition(cboSnapStockSeqShot, "SFU2958"));
                dr["AREAID"] = Util.GetCondition(cboSnapArea, "SFU3203"); //동은필수입니다.
                dr["USERID"] = LoginInfo.USERID;

                if (dr["AREAID"].Equals("")) return;

                RQSTDT.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_STOCKCNT", "INDATA", null, RQSTDT);
                SetStockSeq();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void wndSTOCKCNT_START_Closed(object sender, EventArgs e)
        {
            try
            {
                COM001_011_STOCKCNT_START window = sender as COM001_011_STOCKCNT_START;
                if (window.DialogResult == MessageBoxResult.OK)
                {
                    SetStockSeq();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //차수 마감, 추가시 콤보 생성
        //여기
        private void SetStockSeq()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("STCK_CNT_YM", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.NVC(cboSnapArea.SelectedValue) == "" ? null : cboSnapArea.SelectedValue.ToString();
                dr["STCK_CNT_YM"] = Util.GetCondition(ldpSnapMonthShot);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_STOCKCNT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboSnapStockSeqShot.ItemsSource = null;
                    cboSnapStockSeqShot.ItemsSource = DataTableConverter.Convert(dtResult);
                    cboSnapStockSeqShot.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}