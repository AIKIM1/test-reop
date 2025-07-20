/*************************************************************************************
 Created Date : 2024.04.23
      Creator : 김선준
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.04.  김선준           Initialize 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_046_RACK_SET : C1Window, IWorkArea
    {
        #region "Member Variable & Constructor"
        string _WorkTYpe = string.Empty;
        const string _MV = "MV";
        const string _IN = "IN";
        public bool _bWork = false;
        const string sTag1 = "First RACKID Scan";
        const string sTag2 = "LOTID/2D/CSTID/Mono/DMC Scan";

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK003_046_RACK_SET()
        {
            InitializeComponent();
        }
        #endregion


        #region #. Event Lists...       
        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] objs = C1WindowExtension.GetParameters(this);
                if (objs != null && objs.Length >= 1)
                {
                    _WorkTYpe = Util.NVC(objs[0]); 
                }
                if (this._WorkTYpe.Equals(_MV))
                {
                    this.Header = "NG RACK MOVE";
                }
                else
                {
                    this.Header = "NG RACK MOVE CONFIRM";
                }

                SetMoveRackCombo();

                inittp1(); 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region #4.1 Set Combo
        /// <summary>
        /// NG Move Rack 선택
        /// </summary>
        private void SetMoveRackCombo()
        {
            string bizRuleName = "DA_PRD_SEL_PARTIAL_ILT_NG_RACK";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string)); 

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID; 
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                //if (null != dtRSLTDT && dtRSLTDT.Rows.Count > 0)
                //{
                //    this.cboMoveRack.DisplayMemberPath = "RACK_NAME";
                //    this.cboMoveRack.SelectedValuePath = "RACK_ID";
                //    this.cboMoveRack.ItemsSource = dtRSLTDT.AsDataView();

                //    if (this.cboMoveRack.Items.Count > 0)
                //    {
                //        this.cboMoveRack.SelectedValue = string.Empty;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion //4.1 Set Combo

        #region #. tp1
        private void inittp1()
        {
            //this.txtScan.Clear(); 
            //Util.gridClear(this.grdMain); 
            //this.btRackRcv.Visibility = Visibility.Hidden;
            //this.btRackIss.Visibility = Visibility.Hidden;
            //this.txtScan.Tag = string.Empty;
            //this.cboMoveRack.SelectedValue = string.Empty;

            //if (this._WorkTYpe.Equals(_IN))
            //{
            //    this.btRackRcv.Visibility = Visibility.Visible; 
            //    this.cboMoveRack.IsEnabled = false;
            //    this.txtScan.Tag = sTag1;
            //}
            //else
            //{
            //    this.btRackIss.Visibility = Visibility.Visible; 
            //    this.cboMoveRack.IsEnabled = true;
            //    this.txtScan.Tag = sTag2;
            //} 
        } 

        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdMain.Rows.Count != 0)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1815"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
                {
                    if (Result == MessageBoxResult.OK)
                    {
                        inittp1();
                    }
                });
            }
            else
            {
                inittp1();
            }
        }
         
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    string sScan = txtScan.Text.Trim();
            //    if (string.IsNullOrWhiteSpace(sScan)) return;

            //    #region 중복LOT Check
            //    if (sScan.Contains(","))
            //    {
            //        string[] splt = sScan.Split(',');
            //        var query = (splt.AsEnumerable().GroupBy(x => x).Select(g => new { LOTID = g.Key, COUNT = g.Count() })).Where(x => x.COUNT > 1).Select(x => x.LOTID);
            //        if (null != query && query.Count() > 0)
            //        {
            //            foreach (string item in query)
            //            {
            //                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1376", item), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
            //                this.txtScan.Clear();
            //                this.txtScan.Focus();
            //                return;
            //            }
            //        }
            //    }
            //    #endregion

               
            //    ScanData(sScan);
            //}
        }

        /// <summary>
        /// Rack Move버튼 클릭
        /// </summary> 
        private void btRackRcv_Click(object sender, RoutedEventArgs e)
        { 
            NgRackMove();
        }

        /// <summary>
        /// Rack Move Confirm버튼 클릭
        /// </summary> 
        private void btRackIss_Click(object sender, RoutedEventArgs e)
        {
            NgRackMove();
        }
        #endregion

        #endregion

        #region Scan Data
        /// <summary>
        /// RackID / Lot List 조회
        /// </summary>
        /// <param name="sScan"></param>
        public void ScanData(string sScan)
        {
            try
            {
                #region Scan Data

                //Scan 정보 조회
                //if (this._WorkTYpe.Equals(_IN) && (null == this.cboMoveRack.SelectedValue || string.IsNullOrEmpty(this.cboMoveRack.SelectedValue.ToString())))
                //{
                //    #region Rack정보조회
                //    int iCnt = DataTableConverter.Convert(this.cboMoveRack.ItemsSource).AsEnumerable().Where(x => x.Field<string>("RACK_ID").Equals(sScan)).Count();
                //    if (iCnt > 0)
                //    {
                //        this.cboMoveRack.SelectedValue = sScan;
                //        this.txtScan.Tag = sTag2;
                //        return;
                //    }
                //    else
                //    {
                //        Util.MessageInfo("SFU4925", new string[] { "RACK" });
                //        return; 
                //    }
                //    #endregion  //Rack정보조회
                //}

                //Scan 정보 조회 
                #region 중복LOT제거
                string sScanLots = sScan;
                if (null != this.grdMain.ItemsSource && this.grdMain.Rows.Count > 0)
                {
                    #region LOTID로 1차 체크
                    DataTable dtData1 = DataTableConverter.Convert(this.grdMain.ItemsSource);
                    var scanLists = sScan.Split(',').Select(x => new { LOTID = x });
                    var query_lot1 = dtData1.AsEnumerable().ToDictionary(p => p["LOTID"]);
                    var query_scanLots = from lot in scanLists.AsEnumerable()
                                         where query_lot1.ContainsKey(lot.LOTID)
                                         select new { LOTID = lot.LOTID };
                    if (null != query_scanLots && query_scanLots.Count() > 0)
                    {
                        //Lot 이 이미 추가되었습니다.
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1196"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                        return;
                    }
                    #endregion
                }
                #endregion //중복LOT제거

                if (!string.IsNullOrEmpty(sScanLots))
                {
                    #region Lot조회
                    DataSet dsResult;
                    DataSet dsInput = new DataSet();

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    INDATA.Columns.Add("SCAN", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    INDATA.Columns.Add("NG_MOVE_YN", typeof(string));
                    INDATA.Columns.Add("NG_MOVE_IN_YN", typeof(string));
                    INDATA.Columns.Add("RACK_ID_TO", typeof(string));
                    #endregion //변수선언

                    DataRow dr = INDATA.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["SCAN"] = sScanLots;
                    dr["USERID"] = LoginInfo.USERID;
                    dr["NG_MOVE_YN"] = (this._WorkTYpe.Equals(_IN)) ? "N" : "Y";
                    dr["NG_MOVE_IN_YN"] = (this._WorkTYpe.Equals(_IN)) ? "Y" : "N";
                    //if (this._WorkTYpe.Equals(_IN)) dr["RACK_ID_TO"] = this.cboMoveRack.SelectedValue.ToString();

                    INDATA.Rows.Add(dr);
                    dsInput.Tables.Add(INDATA);

                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_LOT_LIST", "INDATA", "OUT_LOTLIST", dsInput, null);

                    if (null != dsResult && dsResult.Tables["OUT_LOTLIST"].Rows.Count > 0)
                    {
                        //Grid Data
                        if (null == this.grdMain.ItemsSource || this.grdMain.Rows.Count == 0)
                        {
                            #region 최초Scan 
                            Util.GridSetData(this.grdMain, dsResult.Tables["OUT_LOTLIST"], FrameOperation, true);
                            #endregion
                        }
                        else
                        {
                            #region 다중Scan 
                            DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);
                            DataTable dtLot = dsResult.Tables["OUT_LOTLIST"];

                            #region LOTID로 2차 체크
                            if (!sScanLots.Contains(","))
                            {
                                var query_lot1 = dtLot.AsEnumerable().ToDictionary(p => p["LOTID"]);
                                var query_scanLots = from lot in dtData.AsEnumerable()
                                                     where query_lot1.ContainsKey(lot.Field<string>("LOTID"))
                                                     select new { LOTID = lot.Field<string>("LOTID") };
                                if (null != query_scanLots && query_scanLots.Count() > 0)
                                {
                                    //Lot 이 이미 추가되었습니다.
                                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1196"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                                    return;
                                }
                            }
                            #endregion //LOTID로 2차 체크

                            dtData.Merge(dtLot);
                            Util.GridSetData(this.grdMain, dtData, FrameOperation, true);
                            #endregion // 다중Scan
                        }
                    }
                    else
                    {
                       //대상 LOT정보[% 1]가 존재하지 않습니다.
                       Util.MessageInfo("100103", new string[] { sScanLots }); 
                    }
                    #endregion //Lot조회
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
     
            }
        }
        #endregion //Scan Data
         
        #region #. Function NG RACK 이동 
        /// <summary>
        /// RACK이동/확정
        /// </summary>
        public void NgRackMove()
        {
            if (this.grdMain.Rows.Count == 0) return; 
            //if (string.IsNullOrEmpty(this.cboMoveRack.SelectedValue.ToString()))
            //{
            //    Util.MessageInfo("SFU4925", new string[] { "RACK" });
            //    return;
            //}

            try
            { 
                DataSet dsInput = new DataSet();

                DataTable inDataTable = dsInput.Tables.Add("INDATA");
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("GUBUN", typeof(string));
                inDataTable.Columns.Add("RACK_ID_TO", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["AREAID"] = LoginInfo.CFG_AREA_ID;
                row["GUBUN"] = this._WorkTYpe;
                //row["RACK_ID_TO"] = this.cboMoveRack.SelectedValue.ToString();
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                //대상 LOT
                DataTable inLot = dsInput.Tables.Add("INLOT");
                inLot.Columns.Add("LOTID", typeof(string)); 

                DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);
                
                foreach (DataRow item in dtData.Rows)
                {
                    //if (this._WorkTYpe.Equals(_IN) || (this._WorkTYpe.Equals(_MV) && !this.cboMoveRack.SelectedValue.ToString().Equals(item["RACK_ID"].ToString())) )
                    //{
                    //    inLot.Rows.Add(item["LOTID"].ToString());
                    //}
                }

                if (inLot.Rows.Count > 0)
                {
                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_PARTIAL_ILT_NG_RACK", "INDATA,INLOT", "", dsInput, null);
                    
                    inittp1();

                    _bWork = true;
                    Util.MessageInfo("SFU1766"); //이동완료
                }
                else
                {
                    //Util.MessageInfo("SFU4534", new string[] { this.cboMoveRack.SelectedValue.ToString() }); 
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            { 
            }
        }

        #endregion

        private void btndown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}