/*************************************************************************************
 Created Date : 2023.04.04
      Creator : 김선준
   Decription : Partial ILT Rack 입/출고
--------------------------------------------------------------------------------------
 [Change History]
  2023.04.04  김선준 : Initial Created.
  2023.06.29  김선준 : NG발생시 기존 SCAN LOT Clear, Note 입력 추가
  2023.08.03  김선준 : NG사유 추가
***************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_042 : UserControl, IWorkArea
    {
        #region #. Member Variable Lists...
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();
        CommonCombo _combo = new CommonCombo();
        const string sTag1 = "First RACKID Scan";
        const string sTag2 = "LOTID/2D/CSTID/Mono/DMC Scan";

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region #. Declaration & Constructor
        public PACK003_042()
        {
            InitializeComponent();
        }
        #endregion

        #region #. Event Lists...
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                #region #. tp1   
                txtRackID.Focus();
                #endregion

                #region #. tp2
                DateTime firstOfThisMonth = DateTime.Now.AddDays(-7);
                DateTime firstOfNextMonth = DateTime.Now;

                dtpFr.IsNullInitValue = true;
                dtpTo.IsNullInitValue = true;

                dtpFr.SelectedDateTime = firstOfThisMonth;
                dtpTo.SelectedDateTime = firstOfNextMonth;

                SetNgReasonCombo();

                this.txtLOTID.Tag = sTag2;
                #endregion

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        #region #4.1 Set Combo
        /// <summary>
        /// NG사유
        /// </summary>
        private void SetNgReasonCombo()
        {
            //
            string bizRuleName = "DA_BAS_SEL_AREA_COM_CODE_BY_SORT";

            DataTable dtRQSTDT = new DataTable("RQSTDT");
            DataTable dtRSLTDT = new DataTable("RSLTDT");

            try
            {
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string)); 

                DataRow drRQSTDT = dtRQSTDT.NewRow();
                drRQSTDT["LANGID"] = LoginInfo.LANGID;
                drRQSTDT["AREAID"] = LoginInfo.CFG_AREA_ID;
                drRQSTDT["COM_TYPE_CODE"] = "ILT_MNG_DESC_CODE"; 
                dtRQSTDT.Rows.Add(drRQSTDT);

                dtRSLTDT = new ClientProxy().ExecuteServiceSync(bizRuleName, dtRQSTDT.TableName, dtRSLTDT.TableName, dtRQSTDT);
                if (null != dtRSLTDT && dtRSLTDT.Rows.Count > 0)
                {
                    cboMngDesc.DisplayMemberPath = "CBO_CODE_NAME";
                    cboMngDesc.SelectedValuePath = "CBO_CODE";
                    cboMngDesc.ItemsSource = dtRSLTDT.AsDataView();

                    if (cboMngDesc.Items.Count > 0)
                    {
                        cboMngDesc.SelectedValue = string.Empty;
                    }
                }
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
            this.txtScan.Clear();
            this.txtRackID.Clear();
            this.txtRackName.Clear();
            Util.gridClear(this.grdMain);
            this.txtRackID.Focus();
            this.btRackRcv.Visibility = Visibility.Hidden;
            this.btRackIss.Visibility = Visibility.Hidden; 
            this.txtScan.Tag = string.Empty;
            this.rbRack_Rcv.IsEnabled = true;
            this.rbRack_Iss.IsEnabled = true;
            this.txtNote.Text = string.Empty;
            if (this.rbRack_Rcv.IsChecked == true)
            {
                this.btRackRcv.Visibility = Visibility.Visible;
                this.txtScan.Tag = sTag1;
                this.txtNote.Visibility = Visibility.Visible;
                this.lbNote.Visibility = Visibility.Visible;
                this.grdMngDesc.Visibility = Visibility.Collapsed;
                this.cboMngDesc.SelectedValue = string.Empty;
            }
            else
            {
                this.btRackIss.Visibility = Visibility.Visible;
                this.txtScan.Tag = sTag2;
                this.txtNote.Visibility = Visibility.Hidden;
                this.lbNote.Visibility = Visibility.Hidden;
                this.grdMngDesc.Visibility = Visibility.Collapsed;
                this.cboMngDesc.SelectedValue = string.Empty;
            }
            PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMain.GetRowCount());
        }
        private void rbRack_Rcv_Checked(object sender, RoutedEventArgs e)
        {
            inittp1(); 
        }         

        private void rbRack_Iss_Checked(object sender, RoutedEventArgs e)
        {
            inittp1(); 
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
            if (e.Key == Key.Enter)
            {
                if (rbRack_Rcv.IsChecked == false && rbRack_Iss.IsChecked == false)
                {
                    string sDetail = ObjectDic.Instance.GetObjectName(tbRadioButton.Text.ToString()).Replace("[#] ", string.Empty);
                    // 선택오류 : 필수조건을 선택하지 않았습니다. [%]

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8393", sDetail), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                    this.txtScan.Clear();
                    this.txtScan.Focus();
                    return;
                }

                string sScan = txtScan.Text.Trim();
                if (string.IsNullOrWhiteSpace(sScan)) return;

                #region 중복LOT Check
                if (sScan.Contains(","))
                {
                    string[] splt = sScan.Split(',');
                    var query = (splt.AsEnumerable().GroupBy(x => x).Select(g => new { LOTID = g.Key, COUNT = g.Count() })).Where(x => x.COUNT > 1).Select(x => x.LOTID);
                    if (null != query && query.Count() > 0)
                    {
                        foreach (string item in query)
                        { 
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1376", item), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);
                            this.txtScan.Clear();
                            this.txtScan.Focus();
                            return;
                        }
                    }
                }
                #endregion

                if (this.rbRack_Rcv.IsChecked == true)
                {
                    ScanRcv(sScan);
                }
                else
                {
                    ScanIss(sScan);
                }
                
                if (this.grdMain.Rows.Count > 0)
                {
                    this.rbRack_Rcv.IsEnabled = false;
                    this.rbRack_Iss.IsEnabled = false;
                }
                PackCommon.SearchRowCount(ref this.tbGrdMainTp1Cnt, this.grdMain.GetRowCount());
            }
        }

        /// <summary>
        /// 입고버튼 클릭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRackRcv_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdMain.Rows.Count == 0) return;
            RecieveLot();
        }

        private void btRackIss_Click(object sender, RoutedEventArgs e)
        {
            if (this.grdMain.Rows.Count == 0) return;
            IssuingLot();
        }
        
        private void btnInit2_Click(object sender, RoutedEventArgs e)
        {
            this.txtLOTID.Clear();
            Util.gridClear(this.grdMainTp2);
        }
        #endregion

        #region #. tp2
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            DateTime firstOfDay = dtpFr.SelectedDateTime;
            DateTime endOfDay = dtpTo.SelectedDateTime;
            TimeSpan dateDiff = endOfDay - firstOfDay;

            if (dateDiff.Days > 31)
            {
                //SFU5033 : 기간은 %1달 이내 입니다.
                Util.MessageValidation("SFU5033", "");
                return;
            }

            Util.gridClear(grdMainTp2);
            this.SearchProcess();
        }
        #endregion

        #endregion

        #region #. Function Recieve 
        /// <summary>
        /// RackID / Lot List 조회
        /// </summary>
        /// <param name="sScan"></param>
        public void ScanRcv(string sScan)
        {
            try
            {
                //RACK 두번 SCAN 방지
                if (this.txtRackID.Text.Trim().Equals(sScan)) return;

                #region 변수선언
                DataSet dsResult;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("JOB_FLAG", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("LOTIDS", typeof(string));
                #endregion //변수선언

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;

                //Scan 정보 조회
                if (string.IsNullOrEmpty(txtRackID.Text.Trim()))
                {
                    #region Rack정보조회
                    dr["JOB_FLAG"] = "R";
                    dr["RACK_ID"] = sScan;
                    dr["LOTIDS"] = null;

                    INDATA.Rows.Add(dr);
                    dsInput.Tables.Add(INDATA);

                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_PARTIAL_ILT_RACK_RCV", "INDATA", "OUTDATA,OUT_RACK,OUT_LOT", dsInput, null);

                    if (null != dsResult && dsResult.Tables["OUT_RACK"].Rows.Count > 0)
                    {
                        DataTable dtRack = dsResult.Tables["OUT_RACK"];
                        this.txtRackID.Text = dtRack.Rows[0]["RACK_ID"].ToString();
                        this.txtRackName.Text = dtRack.Rows[0]["DISP_RACK_NAME"].ToString();

                        this.txtScan.Tag = sTag2;

                        if (dtRack.Rows[0]["NG_RACK_YN"].ToString().Equals("Y"))
                        {
                            this.grdMngDesc.Visibility = Visibility.Visible;
                            this.cboMngDesc.SelectedValue = string.Empty;
                            this.txtNote.Clear();
                        }
                        else
                        {
                            this.grdMngDesc.Visibility = Visibility.Collapsed;
                            this.cboMngDesc.SelectedValue = string.Empty;
                        }
                    }
                    #endregion  //Rack정보조회
                }
                else
                {
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
                        dr["JOB_FLAG"] = "L"; //Lot List 조회
                        dr["RACK_ID"] = this.txtRackID.Text.Trim();
                        dr["LOTIDS"] = sScanLots;

                        INDATA.Rows.Add(dr);
                        dsInput.Tables.Add(INDATA);

                        dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_PARTIAL_ILT_RACK_RCV", "INDATA", "OUTDATA,OUT_RACK,OUT_LOT", dsInput, null);

                        if (null != dsResult && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            //Error 체크
                            if (dsResult.Tables["OUTDATA"].Rows[0]["RSLT_FLAG"].ToString() == "E")
                            {
                                PACK003_042_NG ngPopup = new PACK003_042_NG();
                                ngPopup.FrameOperation = FrameOperation;

                                if (ngPopup != null)
                                {
                                    object[] Parameters;
                                    Parameters = new object[1];
                                    Parameters[0] = dsResult.Tables["OUT_LOT"];
                                    C1WindowExtension.SetParameters(ngPopup, Parameters);

                                    this.Dispatcher.BeginInvoke(new Action(() => ngPopup.ShowModal()));
                                    ngPopup.BringToFront();
                                }

                                //NG발생시 기존 SCAN LOT Clear 2023-06-29 seonjun
                                Util.gridClear(this.grdMain);

                                return;
                            }

                            //Grid Data
                            if (null == this.grdMain.ItemsSource || this.grdMain.Rows.Count == 0)
                            {
                                #region 최초Scan 
                                Util.GridSetData(this.grdMain, dsResult.Tables["OUT_LOT"], FrameOperation, true);
                                #endregion
                            }
                            else
                            {
                                #region 다중Scan 
                                DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);
                                DataTable dtLot = dsResult.Tables["OUT_LOT"];

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

                                //Grid출력
                                int iRow = dtData.Rows.Count;
                                for (int i = dtLot.Rows.Count - 1; i >= 0; i--)
                                {
                                    iRow = iRow + 1;
                                    DataRow row = dtData.NewRow();
                                    row["SEQ_NO"] = iRow;
                                    row["LOTID"] = dtLot.Rows[i]["LOTID"];
                                    row["RSLT_MSG"] = dtLot.Rows[i]["RSLT_MSG"];
                                    dtData.Rows.InsertAt(row, 0);
                                }
                                Util.GridSetData(this.grdMain, dtData, FrameOperation, true);
                                #endregion // 다중Scan
                            }
                        }
                        #endregion //Lot조회
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus();
            }
        }

        /// <summary>
        /// LOT 입고
        /// </summary>
        public void RecieveLot()
        {
            if (this.grdMain.Rows.Count == 0) return;

            try
            {
                DataSet dsResult;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID",  typeof(string));
                INDATA.Columns.Add("AREAID",  typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("LOTIDS",  typeof(string));
                INDATA.Columns.Add("NOTE",    typeof(string)); 
                INDATA.Columns.Add("RACK_STAT", typeof(string));
                INDATA.Columns.Add("ILT_MNG_DESC_CODE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERID"] = LoginInfo.USERID;
                dr["RACK_ID"] = this.txtRackID.Text.Trim();
                dr["NOTE"] = this.txtNote.Text.Trim();
                dr["RACK_STAT"] = "IN";
                if (this.cboMngDesc.Items.Count > 0 && null != this.cboMngDesc.SelectedValue) dr["ILT_MNG_DESC_CODE"] = this.cboMngDesc.SelectedValue.ToString();

                DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);

                List<string> lotList = dtData.AsEnumerable().Select(x => x.Field<string>("LOTID")).ToList();
                string sLotIds = String.Join(",", lotList);

                dr["LOTIDS"] = sLotIds;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PARTIAL_ILT_RACK_RCV", "INDATA", "OUTDATA,OUT_LOT", dsInput, null);
                if (dsResult.Tables["OUTDATA"].Rows[0]["RSLT_FLAG"].ToString().Equals("S"))
                {
                    this.txtRackID.Clear();
                    this.txtRackName.Clear();
                    this.rbRack_Rcv.IsEnabled = true;
                    this.rbRack_Iss.IsEnabled = true;
                    this.grdMngDesc.Visibility = Visibility.Collapsed;
                    this.txtScan.Tag = sTag1;
                    this.cboMngDesc.SelectedValue = string.Empty;
                    this.txtNote.Clear();

                    Util.gridClear(grdMain);
                    Util.MessageInfo("SFU1798"); //입고하였습니다.
                }
                else
                {
                    //SFU4974 오류내용 확인이 필요합니다.
                    Util.GridSetData(this.grdMain, dsResult.Tables["OUT_LOT"], FrameOperation, true);
                    Util.MessageInfo("SFU4974");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus(); 
            }
        }
        #endregion
        
        #region #. Function Issuing

        /// <summary>
        /// Lot List 조회
        /// </summary>
        /// <param name="sScan"></param>
        public void ScanIss(string sScan)
        {
            try
            {
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
                    #region 변수선언
                    DataSet dsResult;
                    DataSet dsInput = new DataSet();

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    INDATA.Columns.Add("LOTIDS", typeof(string));
                    #endregion //변수선언

                    DataRow dr = INDATA.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["LOTIDS"] = sScanLots;

                    INDATA.Rows.Add(dr);
                    dsInput.Tables.Add(INDATA);

                    dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_PARTIAL_ILT_RACK_ISS", "INDATA", "OUTDATA,OUT_LOT", dsInput, null);

                    if (null != dsResult && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        //Error 체크
                        if (dsResult.Tables["OUTDATA"].Rows[0]["RSLT_FLAG"].ToString() == "E")
                        {
                            PACK003_042_NG ngPopup = new PACK003_042_NG();
                            ngPopup.FrameOperation = FrameOperation;

                            if (ngPopup != null)
                            {
                                object[] Parameters;
                                Parameters = new object[1];
                                Parameters[0] = dsResult.Tables["OUT_LOT"];
                                C1WindowExtension.SetParameters(ngPopup, Parameters);

                                this.Dispatcher.BeginInvoke(new Action(() => ngPopup.ShowModal()));
                                ngPopup.BringToFront();
                            }

                            //NG발생시 기존 SCAN LOT Clear 2023-06-29 seonjun
                            this.txtRackID.Clear();
                            this.txtRackName.Clear();
                            this.rbRack_Rcv.IsEnabled = true;
                            this.rbRack_Iss.IsEnabled = true;
                            Util.gridClear(this.grdMain);

                            return;
                        }

                        //Grid Data
                        if (null == this.grdMain.ItemsSource || this.grdMain.Rows.Count == 0)
                        {
                            #region 최초SCAN 
                            this.txtRackID.Text = dsResult.Tables["OUT_LOT"].Rows[0]["RACK_ID"].ToString();
                            this.txtRackName.Text = dsResult.Tables["OUT_LOT"].Rows[0]["DISP_RACK_NAME"].ToString();

                            Util.GridSetData(this.grdMain, dsResult.Tables["OUT_LOT"], FrameOperation, true);
                            #endregion //최초SCAN
                        }
                        else
                        {
                            #region 다중SCAN 
                            DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);
                            DataTable dtLot = dsResult.Tables["OUT_LOT"];

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

                            //Grid출력
                            int iRow = dtData.Rows.Count;
                            for (int i = dtLot.Rows.Count - 1; i >= 0; i--)
                            {
                                iRow = iRow + 1;
                                DataRow row = dtData.NewRow();
                                row["SEQ_NO"] = iRow;
                                row["LOTID"] = dtLot.Rows[i]["LOTID"];
                                row["RSLT_MSG"] = dtLot.Rows[i]["RSLT_MSG"];
                                dtData.Rows.InsertAt(row, 0);
                            }

                            Util.GridSetData(this.grdMain, dtData, FrameOperation, true);
                            #endregion //다중SCAN
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus();
            }
        }

        /// <summary>
        /// LOT 출고
        /// </summary>
        public void IssuingLot()
        {
            if (this.grdMain.Rows.Count == 0) return;

            try
            {
                DataSet dsResult;
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("LOTIDS", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["USERID"] = LoginInfo.USERID;

                DataTable dtData = DataTableConverter.Convert(this.grdMain.ItemsSource);

                List<string> lotList = dtData.AsEnumerable().Select(x => x.Field<string>("LOTID")).ToList();
                string sLotIds = String.Join(",", lotList);
                dr["LOTIDS"] = sLotIds;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PARTIAL_ILT_RACK_ISS", "INDATA", "OUTDATA,OUT_LOT", dsInput, null);
                if (dsResult.Tables["OUTDATA"].Rows[0]["RSLT_FLAG"].ToString().Equals("S"))
                {
                    this.txtRackID.Clear();
                    this.txtRackName.Clear();

                    this.rbRack_Rcv.IsEnabled = true;
                    this.rbRack_Iss.IsEnabled = true;

                    Util.gridClear(grdMain);
                    Util.MessageInfo("SFU1931"); //출고완료
                }
                else
                {
                    //SFU4974 오류내용 확인이 필요합니다.
                    Util.GridSetData(this.grdMain, dsResult.Tables["OUT_LOT"], FrameOperation, true);
                    Util.MessageInfo("SFU4974");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus(); 
                this.rbRack_Rcv.IsEnabled = true;
                this.rbRack_Iss.IsEnabled = true;
                this.grdMngDesc.Visibility = Visibility.Collapsed;
                this.cboMngDesc.SelectedValue = string.Empty;
            }
        }
        #endregion
        
        #region  Function History
        // 조회 - LOT Hold Relase 이력 Tab
        private void SearchProcess()
        {
            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                string bizRuleName = "BR_PRD_SEL_PARTIAL_ILT_RACK_HIST";
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("SCAN", typeof(string));
                INDATA.Columns.Add("ACTDTTM_FR", typeof(DateTime));
                INDATA.Columns.Add("ACTDTTM_TO", typeof(DateTime));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                if (!string.IsNullOrWhiteSpace(this.txtLOTID.Text.Trim())) dr["SCAN"] = this.txtLOTID.Text.Trim();
                dr["ACTDTTM_FR"] = this.dtpFr.SelectedDateTime.Date;
                dr["ACTDTTM_TO"] = this.dtpTo.SelectedDateTime.Date.AddDays(1);

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", "OUT_LOTLIST", dsInput, null);
                 
                Util.GridSetData(this.grdMainTp2, dsResult.Tables["OUT_LOTLIST"], FrameOperation);
                grdMainTp2.FrozenColumnCount = 2;
                PackCommon.SearchRowCount(ref this.tbGrdMainTp2Cnt, this.grdMainTp2.GetRowCount());                 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion #7. Function 공통 조회 함수 모음
         
    }
}