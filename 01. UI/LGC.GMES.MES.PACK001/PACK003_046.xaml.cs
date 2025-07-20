/************************************************************************************
  Created Date : 2024.04.05
       Creator : 김선준
   Description : Out Line Stock Mgmt.
 ------------------------------------------------------------------------------------
  [Change History]
    2024.04.05  김선준 : Initial Created.
 ************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.PACK001.Class;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Media;
using System.Threading.Tasks;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_046 : UserControl, IWorkArea, IDisposable
    {
        #region #1.Variable
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        } 

        #region 
        InfoModel _InfoModel = new InfoModel();  //조회조건 모델
        DataRowView _Dv;   //전체 랙
        DataRow _Dr;       //선택된 랙

        DataTable _DtWh3;  //창고정보
        DataTable _DtSelWh; //선택된 창고정보

        bool _IsLoaded = false;                  //화면 로드 여부
        bool _IsWhAdd = false;                   //W/H 추가 여부
        bool _IsMove = false;                    //Rack Setting 여부
        bool _IsRackClick = false;               //Rack Click 여부
        bool _IsRackEdit = false;                //편집중         

        string _AREAID = LoginInfo.CFG_AREA_ID;
        string _strBtSetRackStart = "SET_MODE";  //Setting Button Name
        string _strBtSetRackEnd = "END";         //Setting Button Name
        string _strHidden = "HIDE";              //disable Rack Setting
        string _strShow = "VISIBLE";             //Visible Rack Setting
         
        object objFile;                          //Select Image Object 
         
        string sRackKindIntrnt = "INTRNT";

        #endregion
        #endregion //0.1 Variable

        #region #2.Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public PACK003_046()
        {
            InitializeComponent();
        }

        #region Dispose
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                if (null != _dispatcherMainTimer)
                {
                    _dispatcherMainTimer.Stop();
                }
            }

            disposed = true;
        }

        ~PACK003_046()
        {
            Dispose(false);
        }
        #endregion //Dispose
        #endregion //2.Constructor

        #region #3.Event
        /// <summary>
        /// name         : UserControl_Loaded
        /// desc         : 화면로드
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        { 
            this.Initialize();
            this.Loaded -= new RoutedEventHandler(this.UserControl_Loaded);
             
            this._IsLoaded = true;
        }

        /// <summary>
        /// name         : UserControl_UnLoaded
        /// desc         : 화면 언로드
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        /// name         : btnSearch_Click
        /// desc         : 창고 정보 조회
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            Iniallize_Screen("SEARCH");
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// name         : btnInit_Click
        /// desc         : 초기화 
        /// author       : 김선준
        /// create date  : 2024-05-24 오전 10:20:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            this.loadingIndicator.Visibility = Visibility.Visible;
            Iniallize_Screen();
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// name         : cboWhId_SelectedValueChanged
        /// desc         : 창고 선택시 정보 조회
        /// author       : 김선준
        /// create date  : 2024-04-12 오전 09:39:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboWhId_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == cboWhId.ItemsSource) return;

            this.loadingIndicator.Visibility = Visibility.Visible;
            Iniallize_Screen();
            this.loadingIndicator.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// name         : Iniallize_Screen
        /// desc         : 화면 초기화
        /// author       : 김선준
        /// create date  : 2024-05-24 오전 10:21:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void Iniallize_Screen(string sMode = "")
        { 
            #region #. validatoin and initialize 
            this.grDetail.Visibility = Visibility.Visible;
            this.grSet.Visibility = Visibility.Collapsed;

            Util.gridClear(this.grdWhRackInfo);
            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, 0);
            Util.gridClear(this.grdLotList);

            this.txtRackInfo.Text = string.Empty;
            this._IsMove = false;
            this.btnSet.Content = ObjectDic.Instance.GetObjectName(_strBtSetRackStart);

            //좌측 Grid
            Util.gridClear(this.grdSetRackList);

            this.grSetLotList.Visibility = Visibility.Collapsed;
            this.gsplit1.Visibility = Visibility.Collapsed;

            this.brRackSet.Visibility = Visibility.Hidden;
            this.grRackSet.Height = 0;

            this.btnRackSet.Content = ObjectDic.Instance.GetObjectName(_strShow);

            this.brWhSet.Visibility = Visibility.Visible;
            if (this.cboWhId.SelectedIndex == 0)
            {
                this.brWhInfo.Visibility = Visibility.Visible;
            }
            else
            {
                this.brWhInfo.Visibility = Visibility.Collapsed;
            }

            this.grWhSet.Height = Double.NaN;
            this.grWhAdd.Height = Double.NaN;

            this.objFile = null;
            this.txtSetFileName.Text = string.Empty;

            this.btnBlinkOff.Visibility = Visibility.Collapsed;
            #endregion

            if (null == this.cboWhId.SelectedValue || string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString()))
            {
                this.imgPlan.ImageSource = null;
                this.tStack1.ItemsSource = null;

                return;
            }

            this.btnRackSet.Visibility = Visibility.Visible;
            this.btnRackSet.Content = ObjectDic.Instance.GetObjectName(_strShow);

            //조회 안함
            if (cboWhId.SelectedIndex == 0)
            {
                return;
            }

            this.loadingIndicator.Visibility = Visibility.Visible;
            PackCommon.DoEvents();

            try
            {
                SetIntransitCombo();

                #region Rack List 조회
                this._InfoModel.InitVal();
                this._InfoModel.WHINFO_YN = "Y";
                this._InfoModel.RACKINFO_YN = "Y";
                this._InfoModel.AREAID = this._AREAID;
                this._InfoModel.WH_ID = this.cboWhId.SelectedValue.ToString();

                DataSet ds = SearchInfo();

                this.grdWhRackInfo.ItemsSource = ds.Tables["OUT_RACKCNT"].AsDataView();

                this._DtSelWh = ds.Tables["OUTDATA_WH"];
                DataTable dtData = ds.Tables["OUTDATA_RACK"];
                
                if (string.IsNullOrEmpty(sMode))
                {
                    this.tStack1.ItemsSource = dtData.AsDataView();
                    this.grdSetRackList.ItemsSource = dtData.AsDataView();
                }
                else
                {
                    #region Value만 수정
                    DataView dv = (DataView)this.tStack1.ItemsSource;
                    if (null != dv)
                    {
                        DataTable _newDataTable = new DataTable();
                        foreach (DataRowView dr in dv)
                        {
                            _newDataTable = dtData.Select(string.Format("RACK_ID = '{0}'", dr["RACK_ID"])).CopyToDataTable();

                            if (null != _newDataTable && _newDataTable.Rows.Count > 0)
                            {
                                #region Set Value  
                                dr["CHK"] = _newDataTable.Rows[0]["CHK"];
                                dr["RACK_NAME"] = _newDataTable.Rows[0]["RACK_NAME"];
                                dr["RACK_KDN"] = _newDataTable.Rows[0]["RACK_KDN"];
                                dr["RACK_TYPE"] = _newDataTable.Rows[0]["RACK_TYPE"];
                                dr["X_CODI"] =  _newDataTable.Rows[0]["X_CODI"];
                                dr["Y_CODI"] = _newDataTable.Rows[0]["Y_CODI"];
                                dr["RACK_TURN"] = _newDataTable.Rows[0]["RACK_TURN"];
                                dr["RACK_TSPC"] = _newDataTable.Rows[0]["RACK_TSPC"];
                                dr["RACK_HEIGHT"] = _newDataTable.Rows[0]["RACK_HEIGHT"];
                                dr["RACK_WIDTH"] = _newDataTable.Rows[0]["RACK_WIDTH"];
                                dr["RACK_BAS_COLR"] = _newDataTable.Rows[0]["RACK_BAS_COLR"];
                                dr["RACK_FONT_COLR"] = _newDataTable.Rows[0]["RACK_FONT_COLR"];
                                dr["RACK_DISP_NAME"] = _newDataTable.Rows[0]["RACK_DISP_NAME"];
                                dr["NAME_HEIGHT"] = _newDataTable.Rows[0]["NAME_HEIGHT"];
                                dr["NAME_WIDTH"] = _newDataTable.Rows[0]["NAME_WIDTH"];
                                dr["NAME_BAS_COLR"] = _newDataTable.Rows[0]["NAME_BAS_COLR"];
                                dr["NAME_FONT_COLR"] = _newDataTable.Rows[0]["NAME_FONT_COLR"];
                                dr["NAME_FONT_SIZE"] = _newDataTable.Rows[0]["NAME_FONT_SIZE"];

                                dr["EQSGID"] = _newDataTable.Rows[0]["EQSGID"];
                                dr["MOVING_FLAG"] = _newDataTable.Rows[0]["MOVING_FLAG"];
                                dr["MOVING_RACK_ID"] = _newDataTable.Rows[0]["MOVING_RACK_ID"];
                                dr["INSP_RACK_ID"] = _newDataTable.Rows[0]["INSP_RACK_ID"];

                                dr["BLINK_YN"] = _newDataTable.Rows[0]["BLINK_YN"];
                                dr["LINE_CHK_YN"] = _newDataTable.Rows[0]["LINE_CHK_YN"];
                                dr["LOTCNT"] = _newDataTable.Rows[0]["LOTCNT"];

                                dr["RACK_LOK_FLAG"] = _newDataTable.Rows[0]["RACK_LOK_FLAG"];
                                dr["RACK_VISBL_FLAG"] = _newDataTable.Rows[0]["RACK_VISBL_FLAG"];
                                #endregion //Set Value
                            }
                        }
                    }
                    #endregion //Value만 수정
                     
                    this.grdSetRackList.ItemsSource = (DataView)this.tStack1.ItemsSource;
                } 
                #endregion //Rack List 조회

                #region 도면설정 
                if (null != this._DtSelWh.Rows[0]["WH_DRW"] && DBNull.Value != this._DtSelWh.Rows[0]["WH_DRW"])
                {
                    Byte[] imageData = (Byte[])this._DtSelWh.Rows[0]["WH_DRW"];
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(imageData))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    this.imgPlan.ImageSource = image;
                    this.imgPlan.Opacity = Convert.ToDouble(this._DtSelWh.Rows[0]["WH_TSPC"]);
                }
                else
                {
                    this.imgPlan.ImageSource = null;
                }
                #endregion //도면설정

                this.cboSetWhOpacity.SelectedValue = this._DtSelWh.Rows[0]["WH_TSPC"];
                this.cboSetWhProcId.SelectedValue = this._DtSelWh.Rows[0]["LINE_LIMIT_PROCID"];
                this.cboSetWhMoveRack.SelectedValue = this._DtSelWh.Rows[0]["MOVING_RACK_ID"];
                this.txtSetWHKeepDays.Text = this._DtSelWh.Rows[0]["LINE_STCK_KEEP_DAYS"].ToString();
                this.cboSetWhAutoStock.SelectedValue = this._DtSelWh.Rows[0]["LINE_AUTO_STCK_FLAG"];
                this.cboSetWhUse.SelectedValue = this._DtSelWh.Rows[0]["USE_FLAG"];
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion //0.3 Event

        #region #4.Function
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            this.btnImg.Visibility = Visibility.Visible;
            this.btnImg.IsEnabled = true;

            #region 자동조회 - 사용안함
            //SetAutoSearchCombo(cboAutoSearch);

            //if (_dispatcherMainTimer != null)
            //{
            //    int second = 0;

            //    if (cboAutoSearch?.SelectedValue != null && !string.IsNullOrEmpty(cboAutoSearch.SelectedValue.ToString()) && cboAutoSearch.SelectedValue.ToString() != "SELECT")
            //        second = int.Parse(cboAutoSearch.SelectedValue.ToString());

            //    _dispatcherMainTimer.Tick -= DispatcherMainTimer_Tick;
            //    _dispatcherMainTimer.Tick += DispatcherMainTimer_Tick;
            //    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, second);
            //}
            #endregion // 자동조회

            SetInitData();

            this.btnRackSet.Content = ObjectDic.Instance.GetObjectName(_strShow);             
        }

        #region #4.1 Set Combo  
        /// <summary>
        /// 창고조회
        /// </summary>
        private void SetWHCombo(string sWhId = "")
        {
            try
            {
                this._InfoModel.InitVal();
                this._InfoModel.WHINFO_YN = "Y";
                this._InfoModel.WH_USE_FLAG = "Y";
                this._InfoModel.AREAID = this._AREAID;

                DataSet ds = SearchInfo();
                DataTable dt = ds.Tables["OUTDATA_WH"];
                this._DtWh3 = ds.Tables["OUTDATA_WH"].AsDataView().ToTable();

                DataRow dr = dt.NewRow();
                dr["WH_ID"] = "";
                dr["WH_NAME"] = "-SELECT-";
                dt.Rows.InsertAt(dr, 0);

                cboWhId.ItemsSource = dt.AsDataView();
                
                if (this.cboWhId.Items.Count > 1)
                {
                    if (string.IsNullOrEmpty(sWhId) && this.cboWhId.Items.Count <= 2)
                    {
                        this.cboWhId.SelectedIndex = this.cboWhId.Items.Count - 1;
                    }
                    else if (!string.IsNullOrEmpty(sWhId) )
                    { 
                        this.cboWhId.SelectedValue = sWhId;
                    }
                    else
                    {
                        this.cboWhId.SelectedIndex = 0;
                    }
                }
                else
                {
                    this.cboWhId.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            } 
        }

        /// <summary>
        /// 창고조회
        /// </summary>
        private void SetIntransitCombo()
        {
            try
            {
                this._InfoModel.InitVal();

                this._InfoModel.RACKKIND_YN = "Y";
                this._InfoModel.RACK_KDN = sRackKindIntrnt;
                this._InfoModel.AREAID = this._AREAID;
                this._InfoModel.WH_ID = this.cboWhId.SelectedValue.ToString();

                DataSet ds = SearchInfo();
                DataTable dt = ds.Tables["OUT_RACKKIND"];

                DataRow dr = dt.NewRow();
                dr["RACK_ID"] = "";
                dr["RACK_NAME"] = "";
                dt.Rows.InsertAt(dr, 0);

                string sMoveRack = null == this.cboSetWhMoveRack.SelectedValue ? string.Empty : this.cboSetWhMoveRack.SelectedValue.ToString();
                this.cboSetWhMoveRack.ItemsSource = dt.AsDataView();
                this.cboSetWhMoveRack.SelectedValue = sMoveRack;

                sMoveRack = null == this.cboItRack.SelectedValue ? string.Empty : this.cboItRack.SelectedValue.ToString();
                this.cboItRack.ItemsSource = dt.AsDataView();
                this.cboItRack.SelectedValue = sMoveRack;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// 자동조회
        /// </summary>
        /// <param name="cbo"></param>
        private void SetAutoSearchCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE" };
            string[] arrCondition = { LoginInfo.LANGID, "DRB_REFRESH_TERM" };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }        
        #endregion //4.1 Set Combo

        #region #4.2 Search 
        /// <summary>
        /// name         : SearchInfo
        /// desc         : 조회
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private DataSet SearchInfo()
        {
            #region SearchData
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("WH_ID", typeof(string));
            inDataTable.Columns.Add("RACK_ID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("WHINFO_YN", typeof(string));
            inDataTable.Columns.Add("RACKINFO_YN", typeof(string));
            inDataTable.Columns.Add("COLOR_YN", typeof(string));
            inDataTable.Columns.Add("LOTLIST_YN", typeof(string));
            inDataTable.Columns.Add("RACKKIND_YN", typeof(string));
            inDataTable.Columns.Add("RACK_KDN", typeof(string));
            inDataTable.Columns.Add("USE_FLAG", typeof(string));
            inDataTable.Columns.Add("WH_USE_FLAG", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("HOLD_YN", typeof(string));

            inDataTable = indataSet.Tables["INDATA"];
            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["AREAID"] = this._AREAID;
            newRow["USERID"] = LoginInfo.USERID; 

            if (null != cboWhId.SelectedValue && !string.IsNullOrEmpty(cboWhId.SelectedValue.ToString())) newRow["WH_ID"] = cboWhId.SelectedValue.ToString();
            if (null != cboHold.SelectedValue && !string.IsNullOrEmpty(cboHold.SelectedValue.ToString())) newRow["HOLD_YN"] = cboHold.SelectedValue.ToString();

            if (!string.IsNullOrEmpty(this._InfoModel.RACK_ID)) newRow["RACK_ID"] = this._InfoModel.RACK_ID;
            if (!string.IsNullOrEmpty(this._InfoModel.LOTID)) newRow["LOTID"] = this._InfoModel.LOTID;

            newRow["WHINFO_YN"] = this._InfoModel.WHINFO_YN;
            newRow["RACKINFO_YN"] = this._InfoModel.RACKINFO_YN;
            newRow["COLOR_YN"] = this._InfoModel.COLOR_YN;
            newRow["LOTLIST_YN"] = this._InfoModel.LOTLIST_YN;
            newRow["RACKKIND_YN"] = this._InfoModel.RACKKIND_YN;

            if (!string.IsNullOrEmpty(this._InfoModel.RACK_KDN)) newRow["RACK_KDN"] = this._InfoModel.RACK_KDN;
            if (!string.IsNullOrEmpty(this._InfoModel.USE_FLAG)) newRow["USE_FLAG"] = this._InfoModel.USE_FLAG;
            if (!string.IsNullOrEmpty(this._InfoModel.WH_USE_FLAG)) newRow["WH_USE_FLAG"] = this._InfoModel.WH_USE_FLAG;

            inDataTable.Rows.Add(newRow);
            return new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_OUTLINE_INFO", "INDATA", "OUTDATA_WH,OUTDATA_RACK,OUT_COLOR,OUT_LOTLIST,OUT_RACKCNT,OUT_RACKKIND,OUT_RACKTYPE,OUT_LINE,OUT_PROCESS,OUT_AUTH,OUT_LOTLIMIT", indataSet);
            #endregion //SearchData
        }

        /// <summary>
        /// name         : txtLotId_PreviewKeyDown
        /// desc         : multi enter
        /// author       : 김선준
        /// create date  : 2024-05-13 오후 12:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    string[] stringSeparators = new string[] { "\r\n" };
                    string clipboardText = Clipboard.GetText();
                    string[] arrClipboardText = clipboardText.Split(stringSeparators, StringSplitOptions.None);

                    int maxLOTIDCount = 500;
                    string messageID = "SFU8217";
                    if (arrClipboardText.Count() > maxLOTIDCount)
                    {
                        Util.MessageValidation(messageID, 500);     // 최대 500개 까지 가능합니다. 
                        this.txtLotId.Clear();
                        this.txtLotId.Focus();
                        return;
                    }

                    var distinctWords = (from w in arrClipboardText where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                    if (null == distinctWords || distinctWords.Count() == 0) {
                        this.txtLotId.Clear();
                        this.txtLotId.Focus();
                        return;
                    }

                    this.txtLotId.Text = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                
                    this.txtLotId_KeyDown(this, null);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// name         : txtLotId_KeyDown
        /// desc         : Lot이 위치한 Rack 찾기
        /// author       : 김선준
        /// create date  : 2024-04-24 오전 09:38:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        ///                2024-06-13, 김선준, Enter Key 체크
        /// </summary>
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (this._IsMove) return;
            if (string.IsNullOrEmpty(this.txtLotId.Text.Trim())) return;

            if (null == e || e.Key == Key.Enter)
            {
                BlinkOff();

                PackCommon.DoEvents();

                try
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;
                     
                    #region 중복LOT Check
                    string sScan = this.txtLotId.Text.Trim();
                    string sLotDs = sScan;
                    if (sScan.Contains(","))
                    {
                        string[] splt = sScan.Split(',');
                        var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();
                        sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();                    
                    } 
                    #endregion

                    this._InfoModel.InitVal();
                    this._InfoModel.WH_ID = this.cboWhId.SelectedValue.ToString();
                    this._InfoModel.LOTLIST_YN = "Y";
                    this._InfoModel.LOTID = sLotDs;

                    DataSet ds = this.SearchInfo();

                    this.txtRackInfo.Text = string.Empty;
                    Util.GridSetData(this.grdLotList, ds.Tables["OUT_LOTLIST"], FrameOperation);
                    PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, this.grdLotList.Rows.Count);

                    if (ds.Tables["OUT_LOTLIST"].Rows.Count > 0)
                    {
                        DataView dv = (DataView)this.tStack1.ItemsSource;
                        if (null == dv) return;
                        foreach (DataRowView dr in dv)
                        {
                            if (ds.Tables["OUT_LOTLIST"].AsEnumerable().Where(x => x.Field<string>("RACK_ID").Equals(dr["RACK_ID"].ToString())).Count() > 0)
                            {
                                dr["BLINK_YN"] = "Y";
                            }
                            else
                            {
                                dr["BLINK_YN"] = "N";
                            }
                        }
                        this.btnBlinkOff.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    this.txtLotId.Clear();
                    this.txtLotId.Focus();
                    this.loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// name         : btnBlinkOff_Click
        /// desc         : blink off
        /// author       : 김선준
        /// create date  : 2024-04-25 오후 12:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnBlinkOff_Click(object sender, RoutedEventArgs e)
        {
            BlinkOff();
        }

        /// <summary>
        /// name         : BlinkOff
        /// desc         : blink off
        /// author       : 김선준
        /// create date  : 2024-04-25 오후 12:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void BlinkOff()
        {
            DataView dv = (DataView)this.tStack1.ItemsSource;
            if (null == dv) return;
            foreach (DataRowView dr in dv)
            {
                dr["BLINK_YN"] = "N";
            }

            this.btnBlinkOff.Visibility = Visibility.Collapsed;
        }
        #endregion //4.2 Search

        #region #4.3 Setting  

        #region 4.3.0 Set Click
        /// <summary>
        /// name         : btnSet_Click
        /// desc         : Setting 화면 활성/비활성
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSet_Click(object sender, RoutedEventArgs e)
        { 
            SettingFunction();
        }

        /// <summary>
        /// name         : SettingFunction
        /// desc         : Location Setting 클릭시 활성/비활성 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        public void SettingFunction()
        {
            try
            { 
                if (this._IsMove == false)
                {
                    SetFunctionSw(false);

                    this._IsWhAdd = false;
                    this._IsRackEdit = false;

                    this.chkHeaderAllList.IsChecked = false;
                    chkHeaderAllList_Unchecked(this, null);

                    if (null != objFile) objFile = null;

                    SetWHAddCombo();
                    SetIntransitCombo();

                    this.btnSet.Content = ObjectDic.Instance.GetObjectName(_strBtSetRackEnd);
                    this.grDetail.Visibility = Visibility.Collapsed;
                    this.grSet.Visibility = Visibility.Visible;

                    if (null == this._DtWh3 || string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString()))
                    {
                        this.btnRackSet.Visibility = Visibility.Collapsed;
                        this.brRackSet.Visibility = Visibility.Collapsed;
                        this.brWhSet.Visibility = Visibility.Collapsed;

                        this.grSetLotList.Visibility = Visibility.Collapsed;
                        this.gsplit1.Visibility = Visibility.Collapsed;
                        this.grSetLotList.Width = double.NaN;
                        this.grSetLotList.MinWidth = 0;
                        this.gsplit1.Width = 0;
                        this.grCol1.Width = new GridLength(0, GridUnitType.Pixel);
                    }
                    else
                    {
                        #region Set W/H
                        this.txtSetWhId.Text = this._DtSelWh.Rows[0]["WH_ID"].ToString();
                        this.txtSetWhName.Text = this._DtSelWh.Rows[0]["WH_NAME"].ToString();
                        this.cboSetWhOpacity.SelectedValue = Convert.ToDouble(this._DtSelWh.Rows[0]["WH_TSPC"]).ToString("0.#");
                        this.cboSetWhProcId.SelectedValue = null == this._DtSelWh.Rows[0]["LINE_LIMIT_PROCID"] ? string.Empty : this._DtSelWh.Rows[0]["LINE_LIMIT_PROCID"].ToString();
                        this.txtSetWHKeepDays.Text = this._DtSelWh.Rows[0]["LINE_STCK_KEEP_DAYS"].ToString();
                        this.cboSetWhMoveRack.SelectedValue = null == this._DtSelWh.Rows[0]["MOVING_RACK_ID"] ? string.Empty : this._DtSelWh.Rows[0]["MOVING_RACK_ID"].ToString();
                        this.cboSetWhAutoStock.SelectedValue = null == this._DtSelWh.Rows[0]["LINE_AUTO_STCK_FLAG"] ? "N" : this._DtSelWh.Rows[0]["LINE_AUTO_STCK_FLAG"].ToString();
                        this.cboSetWhUse.SelectedValue = null == this._DtSelWh.Rows[0]["USE_FLAG"] ? "Y" : this._DtSelWh.Rows[0]["USE_FLAG"].ToString();
                        #endregion //Set W/H

                        this.grSetLotList.Visibility = Visibility.Visible;
                        this.gsplit1.Visibility = Visibility.Visible;
                        this.grCol1.Width = new GridLength(300, GridUnitType.Pixel); 

                        this.gsplit1.Width = 8;
                    }

                    this._IsMove = !_IsMove;
                }
                else
                {
                    this.chkHeaderAllList.IsChecked = true;
                    chkHeaderAllList_Checked(this, null);

                    #region 저장 Rack 여부 체크 
                    BlinkOff();

                    if (this._IsWhAdd)
                    {
                        SetFunctionSw(true);
                        SetClose();
                        return;
                    }

                    DataView dv = (DataView)this.tStack1.ItemsSource;
                    if (null == dv)
                    {
                        SetFunctionSw(true);
                        SetClose();
                        return;
                    }

                    int iCnt = dv.Table.AsEnumerable().Where(x => x.Field<string>("EDIT_YN").Equals("Y")).Count();
                    if (iCnt > 0)
                    {
                        string msg = MessageDic.Instance.GetMessage("FRA0002").Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                        if (MessageBox.Show(msg, "End of rack setup task", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
                        {
                            SetClose();
                            btnInit_Click(this, null);
                        }
                        else
                        {
                            foreach (DataRowView dr in dv)
                            {
                                if (dr["EDIT_YN"].ToString().Equals("Y"))
                                {
                                    dr["BLINK_YN"] = "Y";
                                }
                            }
                            this.btnBlinkOff.Visibility = Visibility.Visible;
                            return;
                        }                         
                    }
                    else
                    {
                        SetClose();
                        if (this._IsRackEdit)
                        {
                            btnInit_Click(this, null); 
                        } 
                    }

                    SetFunctionSw(true);
                    #endregion                    
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// name         : SetClose
        /// desc         : SetClose 
        /// author       : 김선준
        /// create date  : 2024-05-03 오전 00:56:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SetClose()
        {
            this.btnSet.Content = ObjectDic.Instance.GetObjectName(_strBtSetRackStart);
            this.grDetail.Visibility = Visibility.Visible;
            this.grSet.Visibility = Visibility.Collapsed;

            this.grSetLotList.Visibility = Visibility.Collapsed;
            this.gsplit1.Visibility = Visibility.Collapsed;
            this.gsplit1.Width = 0;
            this.grCol1.Width = new GridLength(0, GridUnitType.Pixel);
            this.chkHeaderAllList_Checked(this, null);

            if (this._IsWhAdd)
            {
                SetWHCombo(cboWhId.SelectedValue.ToString());
                this._IsWhAdd = false;
            }

            this._IsMove = !_IsMove;
        }

        /// <summary>
        /// name         : SetFunctionSw
        /// desc         : SetFunctionSw 
        /// author       : 김선준
        /// create date  : 2024-06-06 오후 16:28:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SetFunctionSw(bool bVal)
        {
            this.cboWhId.IsEnabled = bVal;
            this.txtLotId.IsEnabled = bVal; this.txtLotId.Clear();
            this.btnExcel.Visibility = bVal ? Visibility.Visible : Visibility.Collapsed;
            this.btnInit.Visibility = bVal ? Visibility.Visible : Visibility.Collapsed;
            this.btnSearch.Visibility = bVal ? Visibility.Visible : Visibility.Collapsed;             
        }

        /// <summary>
        /// name         : SetInitData
        /// desc         : 초기값 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        ///                2024-05-24, 김선준, 로드시 코드값 한번에 가져오게 수정
        /// </summary>
        private void SetInitData()
        {
            #region Combo

            #region Init Combo

            this._InfoModel.InitVal();
            this._InfoModel.WHINFO_YN = "Y";
            this._InfoModel.COLOR_YN = "Y";
            this._InfoModel.AREAID = this._AREAID;
            this._InfoModel.USE_FLAG = "Y";
            this._InfoModel.WH_USE_FLAG = "Y";
            this._InfoModel.HOLD_YN = "Y";

            DataSet ds = SearchInfo();

            #region W/H
            DataTable dt = ds.Tables["OUTDATA_WH"];
            this._DtWh3 = ds.Tables["OUTDATA_WH"].AsDataView().ToTable();

            DataRow dr = dt.NewRow();
            dr["WH_ID"] = "";
            dr["WH_NAME"] = "-SELECT-";
            dt.Rows.InsertAt(dr, 0);

            cboWhId.ItemsSource = dt.AsDataView();

            if (this.cboWhId.Items.Count > 1)
            {
                if (this.cboWhId.Items.Count <= 2)
                {
                    this.cboWhId.SelectedIndex = this.cboWhId.Items.Count - 1;
                }
                else
                {
                    this.cboWhId.SelectedIndex = 0;
                }
            }
            else
            {
                this.cboWhId.SelectedIndex = 0;
            }
            #endregion //W/H

            #region Color 
            this.cboSetRackBackColor.ItemsSource = ds.Tables["OUT_COLOR"].AsDataView();
            this.cboSetNameBackColor.ItemsSource = ds.Tables["OUT_COLOR"].AsDataView();
            this.cboSetRackFontColor.ItemsSource = ds.Tables["OUT_COLOR"].AsDataView();
            this.cboSetNameFontColor.ItemsSource = ds.Tables["OUT_COLOR"].AsDataView();
            #endregion //Color

            #region RackKind
            this.cboSetRackKind.ItemsSource = ds.Tables["OUT_RACKTYPE"].AsDataView();
            #endregion

            #region Line
            DataTable dt1 = ds.Tables["OUT_LINE"];
            DataRow dr1 = dt1.NewRow();
            dr1["CBO_CODE"] = "";
            dr1["CBO_NAME"] = "";
            dt1.Rows.InsertAt(dr1, 0);
            this.cboLine.ItemsSource = dt1.AsDataView();
            #endregion //Line

            #region Process
            DataTable dt2 = ds.Tables["OUT_PROCESS"];
            DataRow dr2 = dt2.NewRow();
            dr2["CBO_CODE"] = "";
            dr2["CBO_NAME"] = "";
            dt2.Rows.InsertAt(dr2, 0);
            this.cboSetWhProcId.ItemsSource = dt2.AsDataView();
            #endregion //Process

            #region Setting Button 권한 
            DataTable dt3 = ds.Tables["OUT_AUTH"];
            if (dt3 != null && dt3.Rows.Count > 0)
            {
                this.btnSet.Visibility = Visibility.Visible;
            }
            else
            {
                this.btnSet.Visibility = Visibility.Collapsed;
            }
            #endregion //Setting Button 권한 

            #endregion //Init Combo

            #region RackType
            DataTable dtType = new DataTable();
            dtType.Columns.Add("CBO_CODE", typeof(string));
            dtType.Columns.Add("CBO_NAME", typeof(string));
            dtType.Rows.Add(new object[] { "A", "All" });
            dtType.Rows.Add(new object[] { "R", "Rack" });
            dtType.Rows.Add(new object[] { "N", "Name" });
            this.cboSetRackType.ItemsSource = dtType.AsDataView();
            #endregion

            #region Font Size
            DataTable data2 = new DataTable();
            data2.Columns.Add("N_FONT_SIZE", typeof(double));

            for (int i = 6; i < 15; i++)
            {
                data2.Rows.Add(new object[] { i });
            }
            this.cboSetNameFontSize.ItemsSource = data2.AsDataView();
            #endregion //Font Size       

            #region Opacity
            DataTable data3 = new DataTable();
            data3.Columns.Add("RACK_OPACITY", typeof(string));

            for (int i = 1; i <= 10; i++)
            {
                data3.Rows.Add(new object[] { Convert.ToDouble(i / 10.0).ToString("0.0") });
            }
            this.cboSetOpacity.ItemsSource = data3.AsDataView();
            this.cboSetWhOpacity.ItemsSource = data3.AsDataView();
            #endregion//Opacity            

            #region Rotate
            DataTable data4 = new DataTable();
            data4.Columns.Add("RACK_ROTATE", typeof(double));
            data4.Rows.Add(new object[] { 0 });
            data4.Rows.Add(new object[] { -90 });
            data4.Rows.Add(new object[] { 90 });
            this.cboSetRotate.ItemsSource = data4.AsDataView();
            #endregion //Rotate

            #region YesNo
            DataTable dtY = new DataTable();
            dtY.Columns.Add("CBO_CODE", typeof(string));
            dtY.Columns.Add("CBO_NAME", typeof(string));

            dtY.Rows.Add("Y", "Y");
            dtY.Rows.Add("N", "N");

            this.cboSetWhAutoStock.ItemsSource = dtY.AsDataView();
            this.cboMovingFlag.ItemsSource = dtY.AsDataView();
            this.cboSetWhUse.ItemsSource = dtY.AsDataView(); 
            #endregion //YesNo


            #region YesNoDivide Hold
            DataTable dtYH = new DataTable();
            dtYH.Columns.Add("CBO_CODE", typeof(string));
            dtYH.Columns.Add("CBO_NAME", typeof(string));

            dtYH.Rows.Add("Y", Util.NVC(Common.ObjectDic.Instance.GetObjectName("HOLDINCLUDE")));
            dtYH.Rows.Add("N", Util.NVC(Common.ObjectDic.Instance.GetObjectName("HOLDEXCLUDE")));
            dtYH.Rows.Add("D", Util.NVC(Common.ObjectDic.Instance.GetObjectName("HOLDSPLIT")));

            this.cboHold.ItemsSource = dtYH.AsDataView();
            this.cboHold.SelectedValue = "D";
            #endregion //esNoDivide Hold
            #endregion //Combo
        }

        /// <summary>
        /// name         : btnRackSet_Click
        /// desc         : Rack Set 보이기/숨기기 
        /// author       : 김선준
        /// create date  : 2024-04-11 오후 16:06:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnRackSet_Click(object sender, RoutedEventArgs e)
        {
            if (this.brRackSet.Visibility == Visibility.Visible)
            {
                this.brRackSet.Visibility = Visibility.Collapsed;
                this.grRackSet.Height = 0;
                this.btnRackSet.Content = ObjectDic.Instance.GetObjectName(_strShow); 

                this.brWhSet.Visibility = Visibility.Visible;
                if (this.cboWhId.SelectedIndex == 0)
                {
                    this.brWhInfo.Visibility = Visibility.Visible;
                }
                else
                {
                    this.brWhInfo.Visibility = Visibility.Collapsed;
                }
                this.grWhSet.Height = Double.NaN;
                this.grWhAdd.Height = Double.NaN;
            }
            else
            {
                this.brRackSet.Visibility = Visibility.Visible;
                this.grRackSet.Height = Double.NaN;
                this.btnRackSet.Content = ObjectDic.Instance.GetObjectName(_strHidden); 

                this.brWhSet.Visibility = Visibility.Collapsed;
                this.brWhInfo.Visibility = Visibility.Collapsed;
                this.grWhSet.Height = 0;
                this.grWhAdd.Height = 0;
            }
        }
        #endregion //4.3.0 Set Click

        #region 4.3.1 Rack
        #region Value Change Event
        /// <summary>
        /// name         : MoveThumb_DragDelta
        /// desc         : Rack Move
        /// author       : 김선준
        /// create date  : 2024-04-12 오후 17:08:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (null == this._Dr) return;
            if (this._IsMove == false) return;

            if (this.chkRackLock.IsChecked == true)
            {
                e.Handled = true;
            }
            else
            {
                UIElement thumb = e.Source as UIElement;
                this._Dr["X_CODI"] = Convert.ToDouble(this._Dr["X_CODI"]) + e.HorizontalChange;
                this._Dr["Y_CODI"] = Convert.ToDouble(this._Dr["Y_CODI"]) + e.VerticalChange;

                this.numSetXCord.Value = Convert.ToDouble(this._Dr["X_CODI"]);
                this.numSetYCord.Value = Convert.ToDouble(this._Dr["Y_CODI"]);
            }
        }

        /// <summary>
        /// name         : RACK_MouseLeftButtonDown
        /// desc         : Rack List / Rack Set 
        /// author       : 김선준
        /// create date  : 2024-04-12 오후 17:08:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void RACK_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this._IsRackClick) return;

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                this._IsRackClick = true;

                if (sender.GetType().Name.ToString().Equals("StackPanel"))
                {
                    StackPanel sp = sender as StackPanel;
                    this._Dv = sp.DataContext as DataRowView;
                    this._Dr = ((System.Data.DataRowView)(sp.DataContext)).Row;
                }
                else if (sender.GetType().Name.ToString().Equals("Label"))
                {
                    Label lb = (sender as Label);

                    this._Dv = lb.DataContext as DataRowView;
                    this._Dr = ((System.Data.DataRowView)(lb.DataContext)).Row;
                }

                if (null != this._Dr)
                {
                    this._Dr["BLINK_YN"] = "N";
                    if (this._IsMove)
                    {
                        RackValueSet();
                    }
                    else
                    {
                        #region Lot List 조회
                        SearchLotList(this._Dr["RACK_ID"].ToString(), string.IsNullOrEmpty(this._Dr["RACK_DISP_NAME"].ToString()) ? this._Dr["RACK_NAME"].ToString() : string.Format("[{0}]{1}", this._Dr["RACK_ID"].ToString(), this._Dr["RACK_DISP_NAME"].ToString()));
                        #endregion //Lot List 조회
                    }
                }

                #region Blink off Button
                DataView dv = (DataView)this.grdSetRackList.ItemsSource;
                if (null == dv || dv.Table.AsEnumerable().Where(x => x.Field<object>("BLINK_YN") != null && x.Field<string>("BLINK_YN").Equals("Y")).Count() == 0)
                {
                    this.btnBlinkOff.Visibility = Visibility.Collapsed;
                }
                #endregion //Blink off Button

                if (this._IsMove) this.numSetXCord.Focus();
            }
            catch (Exception)
            { 
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
                this._IsRackClick = false;
            }
        }

        /// <summary>
        /// name         : RackValueSet
        /// desc         : Rack 설정값 보이기
        /// author       : 김선준
        /// create date  : 2024-05-08 오전 09:46:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary> 
        private void RackValueSet()
        {
            this.grRackSet.DataContext = this._Dr;
            this.brRackSet.Visibility = Visibility.Collapsed;
            this.btnRackSet_Click(this, null);

            #region Set Value
            this.txtSetRackId.Text = this._Dr["RACK_ID"].ToString();
            this.txtSetRackName.Text = this._Dr["RACK_NAME"].ToString();

            this.cboSetRackKind.SelectedValue = this._Dr["RACK_KDN"].ToString();
            this.cboSetRackType.SelectedValue = this._Dr["RACK_TYPE"].ToString();

            this.numSetXCord.Value = Convert.ToDouble(this._Dr["X_CODI"]);
            this.numSetYCord.Value = Convert.ToDouble(this._Dr["Y_CODI"]);
            this.cboSetRotate.SelectedValue = Convert.ToDouble(this._Dr["RACK_TURN"]);
            this.cboSetOpacity.SelectedValue = Convert.ToDouble(this._Dr["RACK_TSPC"]).ToString("0.0");

            this.numSetRHeight.Value = Convert.ToDouble(this._Dr["RACK_HEIGHT"]);
            this.numSetRWidth.Value = Convert.ToDouble(this._Dr["RACK_WIDTH"]);
            this.cboSetRackBackColor.SelectedValue = this._Dr["RACK_BAS_COLR"].ToString();
            this.cboSetRackFontColor.SelectedValue = this._Dr["RACK_FONT_COLR"].ToString();

            this.txtSetRackDispNm.Text = this._Dr["RACK_DISP_NAME"].ToString();
            this.numSetNHeight.Value = Convert.ToDouble(this._Dr["NAME_HEIGHT"]);
            this.numSetNWidth.Value = Convert.ToDouble(this._Dr["NAME_WIDTH"]);
            this.cboSetNameBackColor.SelectedValue = this._Dr["NAME_BAS_COLR"].ToString();
            this.cboSetNameFontColor.SelectedValue = this._Dr["NAME_FONT_COLR"].ToString();
            this.cboSetNameFontSize.SelectedValue = Convert.ToDouble(this._Dr["NAME_FONT_SIZE"]);

            DataView dv = (DataView)this.cboSetRackKind.ItemsSource;
            string linechkyn = dv.Table.AsEnumerable().Where(x => x.Field<string>("CBO_CODE").Equals(this._Dr["RACK_KDN"].ToString())).Select(x => x.Field<string>("ATTRIBUTE1")).FirstOrDefault();
            if (!string.IsNullOrEmpty(linechkyn) && linechkyn.Equals("Y"))
            {
                this.cboLine.IsEnabled = true;
                this.cboLine.SelectedValue = this._Dr["EQSGID"].ToString();
            }
            else
            {
                this.cboLine.IsEnabled = false;
                this.cboLine.SelectedIndex = -1;
            }
            this.cboMovingFlag.SelectedValue = this._Dr["MOVING_FLAG"].ToString();
            this.cboItRack.SelectedValue = this._Dr["MOVING_RACK_ID"].ToString();
            this.cboQcRack.SelectedValue = this._Dr["INSP_RACK_ID"].ToString();

            //lock
            this.chkRackLock.IsChecked = (string.IsNullOrEmpty(this._Dr["RACK_LOK_FLAG"].ToString()) || this._Dr["RACK_LOK_FLAG"].ToString().Equals("N")) ? false : true;

            //Visible
            this.chkRackVisible.IsChecked = (string.IsNullOrEmpty(this._Dr["RACK_VISBL_FLAG"].ToString()) || this._Dr["RACK_VISBL_FLAG"].ToString().Equals("N")) ? false : true;
             
            FuncChkControlSet();
            #endregion //Set Value
        }

        /// <summary>
        /// name         : TextBox_PreviewTextInput
        /// desc         : 숫자만 입력
        /// author       : 김선준
        /// create date  : 2024-04-12 오후 21:13:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        ///                2024-05-30 오후 21:13:00, 김선준, 복사/저장 기능 추가
        /// </summary> 
        private void txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (null == tb) return;
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                btnSaveRackSet.Focus();
                tb.Focus();
                btnSaveRackSet_Click(this, null);
                return;
            }

            string sParm = (null == tb.Tag) ? string.Empty : tb.Tag.ToString();

            if (sParm.Equals("P"))
            {
                if (!((Key.D0 <= e.Key && e.Key <= Key.D9) || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) || e.Key == Key.Back || e.Key == Key.OemPeriod || e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Delete
                    || (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control) || (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) || (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control) ))
                {
                    e.Handled = true;
                }

                if (tb.Text.Trim().Length > 0)
                {
                    string[] arVal = tb.Text.Trim().Split('.');
                    if (arVal.Length > 2)
                    {
                        tb.Text = string.Format("{0}.{1}", arVal[0], arVal[1]);
                    }
                }
            }
            else if (sParm.Equals("N"))
            {
                if (!((Key.D0 <= e.Key && e.Key <= Key.D9) || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9) || e.Key == Key.Back || e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Delete
                    || (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control) || (e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control) || (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)))
                {
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// name         : btnApplyView_Click
        /// desc         : Rack 설정 미리보기
        /// author       : 김선준
        /// create date  : 2024-04-12 오후 21:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnApplyView_Click(object sender, RoutedEventArgs e)
        {
            PreviewRackSet();
        }

        /// <summary>
        /// name         : PreviewRackSet
        /// desc         : Rack 설정 미리보기
        /// author       : 김선준
        /// create date  : 2024-04-12 오후 21:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void PreviewRackSet()
        {
            try
            {
                this._Dr["RACK_KDN"] = this.cboSetRackKind.SelectedValue;
                this._Dr["RACK_TYPE"] = this.cboSetRackType.SelectedValue;

                this._Dr["X_CODI"] = Convert.ToDouble(this.numSetXCord.Value);
                this._Dr["Y_CODI"] = Convert.ToDouble(this.numSetYCord.Value);

                this._Dr["RACK_TURN"] = Convert.ToDouble(this.cboSetRotate.SelectedValue);
                this._Dr["RACK_TSPC"] = Convert.ToDouble(this.cboSetOpacity.SelectedValue);

                this._Dr["RACK_HEIGHT"] = Convert.ToDouble(this.numSetRHeight.Value);
                this._Dr["RACK_WIDTH"] = Convert.ToDouble(this.numSetRWidth.Value);
                this._Dr["RACK_BAS_COLR"] = this.cboSetRackBackColor.SelectedValue;
                this._Dr["RACK_FONT_COLR"] = this.cboSetRackFontColor.SelectedValue;

                this._Dr["RACK_DISP_NAME"] = this.txtSetRackDispNm.Text;
                this._Dr["NAME_HEIGHT"] = Convert.ToDouble(this.numSetNHeight.Value);
                this._Dr["NAME_WIDTH"] = Convert.ToDouble(this.numSetNWidth.Value);
                this._Dr["NAME_BAS_COLR"] = this.cboSetNameBackColor.SelectedValue;
                this._Dr["NAME_FONT_COLR"] = this.cboSetNameFontColor.SelectedValue;
                this._Dr["NAME_FONT_SIZE"] = Convert.ToDouble(this.cboSetNameFontSize.SelectedValue);
                
                this._Dr["EQSGID"] = this.cboLine.SelectedValue;
                this._Dr["MOVING_FLAG"] = this.cboMovingFlag.SelectedValue;
                this._Dr["MOVING_RACK_ID"] = this.cboItRack.SelectedValue;
                this._Dr["INSP_RACK_ID"] = this.cboQcRack.SelectedValue;

                this._Dr["EDIT_YN"] = "Y";
                this._Dr["RACK_LOK_FLAG"] = (null == this.chkRackLock.IsChecked || this.chkRackLock.IsChecked == false) ? "N" : "Y";
                this._Dr["RACK_VISBL_FLAG"] = (null == this.chkRackVisible.IsChecked || this.chkRackVisible.IsChecked == false) ? "N" : "Y";

                this._IsRackEdit = true;

                FuncChkControlSet();
            }
            catch (Exception)
            {
                //Util.MessageException(ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// name         : Preview_SelectedValueChanged
        /// desc         : Rack 설정 ComboBox 변경시
        /// author       : 김선준
        /// create date  : 2024-04-13 오전 06:58:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void Preview_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (this._IsRackClick) return;

            if (sender.GetType().Name.Contains("C1ComboBox"))
            {
                C1ComboBox cbo = (C1ComboBox)sender;

                if (cbo.Name.Equals("cboSetRackKind"))
                {
                    DataView dv = (DataView)cbo.ItemsSource;
                    string linechkyn = dv.Table.AsEnumerable().Where(x => x.Field<string>("CBO_CODE").Equals(cbo.SelectedValue.ToString())).Select(x => x.Field<string>("ATTRIBUTE1")).FirstOrDefault();
                    if (!string.IsNullOrEmpty(linechkyn) && linechkyn.Equals("Y"))
                    {
                        this.cboLine.IsEnabled = true; 
                    }
                    else
                    {
                        this.cboLine.IsEnabled = false;
                        this.cboLine.SelectedIndex = -1;
                    }
                } 
            }

            PreviewRackSet();
        }

        /// <summary>
        /// name         : Preview_TextChanged
        /// desc         : Rack 설정 TextBox 변경시
        /// author       : 김선준
        /// create date  : 2024-04-13 오전 07:02:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void Preview_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._IsRackClick) return;

            PreviewRackSet();
        }

        /// <summary>
        /// name         : txt_GotFocus
        /// desc         : 전채선택
        /// author       : 김선준
        /// create date  : 2024-04-26 오전 08:08:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txt_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox tb = (TextBox)e.OriginalSource;
                tb.Dispatcher.BeginInvoke(
                    new Action(delegate
                    {
                        tb.SelectAll();
                    }), System.Windows.Threading.DispatcherPriority.Input);
            }
            catch (Exception)
            { 
            }     
        }

        /// <summary>
        /// name         : NumericBox_ValueChanged
        /// desc         : Numeric box value changed
        /// author       : 김선준
        /// create date  : 2024-07-10 오후 14:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void NumericBox_ValueChanged(object sender, PropertyChangedEventArgs<double> e)
        {
            if (this._IsRackClick) return;

            PreviewRackSet();
        }
        
        /// <summary>
        /// name         : chk_Checked
        /// desc         : Checked Event 처리
        /// author       : 김선준
        /// create date  : 2024-07-11 오전 09:40:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void chk_Checked(object sender, RoutedEventArgs e)
        {
            if (this._IsRackClick) return;

            PreviewRackSet();

        }

        /// <summary>
        /// name         : chk_Unchecked
        /// desc         : UnChecked Event 처리
        /// author       : 김선준
        /// create date  : 2024-07-11 오전 09:46:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void chk_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this._IsRackClick) return;

            PreviewRackSet();

        }

        /// <summary>
        /// name         : chkControlSet
        /// desc         : Rack Lock / Visible 처리
        /// author       : 김선준
        /// create date  : 2024-07-12 오후 14:36:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void FuncChkControlSet()
        {
            if (null == this.chkRackLock.IsChecked || this.chkRackLock.IsChecked == false)
            {
                this.numSetXCord.IsEnabled = true;
                this.numSetYCord.IsEnabled = true;
                this.cboSetRotate.IsEnabled = true;
                this.numSetRHeight.IsEnabled = true;
                this.numSetRWidth.IsEnabled = true;
                this.numSetNHeight.IsEnabled = true;
                this.numSetNWidth.IsEnabled = true;

                this.numSetXCord.Background = Brushes.White;
                this.numSetYCord.Background = Brushes.White;
                this.cboSetRotate.Background = Brushes.White;
                this.numSetRHeight.Background = Brushes.White;
                this.numSetRWidth.Background = Brushes.White;
                this.numSetNHeight.Background = Brushes.White;
                this.numSetNWidth.Background = Brushes.White;
            }
            else
            {
                this.numSetXCord.IsEnabled = false;
                this.numSetYCord.IsEnabled = false;
                this.cboSetRotate.IsEnabled = false;
                this.numSetRHeight.IsEnabled = false;
                this.numSetRWidth.IsEnabled = false;
                this.numSetNHeight.IsEnabled = false;
                this.numSetNWidth.IsEnabled = false;

                this.numSetXCord.Background = Brushes.LightGray;
                this.numSetYCord.Background = Brushes.LightGray;
                this.cboSetRotate.Background = Brushes.LightGray;
                this.numSetRHeight.Background = Brushes.LightGray;
                this.numSetRWidth.Background = Brushes.LightGray;
                this.numSetNHeight.Background = Brushes.LightGray;
                this.numSetNWidth.Background = Brushes.LightGray;
            }             
        }
        #endregion //Value Change Event

        #region Rack 설정 저장
        /// <summary>
        /// name         : btnSaveRackSet_Click
        /// desc         : Rack 설정 저장
        /// author       : 김선준
        /// create date  : 2024-04-13 오후 13:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSaveRackSet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboWhId.SelectedValue.ToString()) || string.IsNullOrEmpty(this.txtSetRackId.Text)) return;

            #region Line Check
            if (null == this.cboSetRackKind.SelectedValue || string.IsNullOrEmpty(this.cboSetRackKind.SelectedValue.ToString()))
            {
                Util.MessageInfo("Select Type");
                this.cboSetRackKind.Focus();
                return;
            }

            DataView dv = (DataView)this.cboSetRackKind.ItemsSource;
            string linechkyn = dv.Table.AsEnumerable().Where(x => x.Field<string>("CBO_CODE").Equals(this.cboSetRackKind.SelectedValue.ToString())).Select(x => x.Field<string>("ATTRIBUTE1")).FirstOrDefault();
            if (!string.IsNullOrEmpty(linechkyn) && linechkyn.Equals("Y") && (null == this.cboLine.SelectedValue || string.IsNullOrEmpty(this.cboLine.SelectedValue.ToString())))
            {
                Util.MessageInfo("Select Line");
                this.cboLine.Focus();
                return;
            }
            else if (string.IsNullOrEmpty(linechkyn) || !linechkyn.Equals("Y"))
            {
                this.cboLine.SelectedValue = string.Empty;
            }
            #endregion //Line Check 

            try
            {
                #region Rack 정보 저장
                DataSet dsInput = new DataSet();

                DataTable inDataTable = dsInput.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string)); 
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = this._AREAID;
                row["WH_ID"] = this.cboWhId.SelectedValue;  
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                DataTable inRackTable = dsInput.Tables.Add("IN_RACK");
                inRackTable.Columns.Add("RACK_ID", typeof(string));
                inRackTable.Columns.Add("RACK_KDN", typeof(string));
                inRackTable.Columns.Add("RACK_TYPE", typeof(string));
                inRackTable.Columns.Add("RACK_DISP_NAME", typeof(string));
                inRackTable.Columns.Add("X_CODI", typeof(decimal));
                inRackTable.Columns.Add("Y_CODI", typeof(decimal));
                inRackTable.Columns.Add("RACK_TURN", typeof(int));
                inRackTable.Columns.Add("RACK_TSPC", typeof(double));
                inRackTable.Columns.Add("RACK_HEIGHT", typeof(int));
                inRackTable.Columns.Add("RACK_WIDTH", typeof(int));
                inRackTable.Columns.Add("RACK_BAS_COLR", typeof(string));
                inRackTable.Columns.Add("RACK_FONT_COLR", typeof(string));
                inRackTable.Columns.Add("NAME_HEIGHT", typeof(int));
                inRackTable.Columns.Add("NAME_WIDTH", typeof(int));
                inRackTable.Columns.Add("NAME_BAS_COLR", typeof(string));
                inRackTable.Columns.Add("NAME_FONT_COLR", typeof(string));
                inRackTable.Columns.Add("NAME_FONT_SIZE", typeof(int));
                inRackTable.Columns.Add("RACK_HOLD_FLAG", typeof(string));
                inRackTable.Columns.Add("NOTE", typeof(string));
                inRackTable.Columns.Add("EQSGID", typeof(string));
                inRackTable.Columns.Add("MOVING_FLAG", typeof(string));
                inRackTable.Columns.Add("MOVING_RACK_ID", typeof(string));
                inRackTable.Columns.Add("INSP_RACK_ID", typeof(string));
                inRackTable.Columns.Add("RACK_LOK_FLAG", typeof(string));
                inRackTable.Columns.Add("RACK_VISBL_FLAG", typeof(string));

                DataRow dr = null;
                dr = inRackTable.NewRow();
                dr["RACK_ID"] = this.txtSetRackId.Text;
                dr["RACK_KDN"] = this.cboSetRackKind.SelectedValue;
                dr["RACK_TYPE"] = this.cboSetRackType.SelectedValue;
                dr["RACK_DISP_NAME"] = this.txtSetRackDispNm.Text.Trim();
                dr["X_CODI"] = Convert.ToDecimal(this.numSetXCord.Value) ;
                dr["Y_CODI"] = Convert.ToDecimal(this.numSetYCord.Value);
                dr["RACK_TURN"] = this.cboSetRotate.SelectedValue;
                dr["RACK_TSPC"] = Convert.ToDouble(this.cboSetOpacity.SelectedValue);
                dr["RACK_HEIGHT"] = Convert.ToInt16(this.numSetRHeight.Value);
                dr["RACK_WIDTH"] = Convert.ToInt16(this.numSetRWidth.Value); ;
                dr["RACK_BAS_COLR"] = this.cboSetRackBackColor.SelectedValue;
                dr["RACK_FONT_COLR"] = this.cboSetRackFontColor.SelectedValue;
                dr["NAME_HEIGHT"] = Convert.ToInt16(this.numSetNHeight.Value);
                dr["NAME_WIDTH"] = Convert.ToInt16(this.numSetNWidth.Value);
                dr["NAME_BAS_COLR"] = this.cboSetNameBackColor.SelectedValue;
                dr["NAME_FONT_COLR"] = this.cboSetNameFontColor.SelectedValue;
                dr["NAME_FONT_SIZE"] = Convert.ToInt16(this.cboSetNameFontSize.SelectedValue) ;
                dr["RACK_HOLD_FLAG"] = "N";
                dr["NOTE"] = null;

                dr["EQSGID"] = this.cboLine.SelectedValue; //INLINE이 아니면 null처리

                dr["MOVING_FLAG"] = this.cboMovingFlag.SelectedValue;
                dr["MOVING_RACK_ID"] = this.cboItRack.SelectedValue;
                dr["INSP_RACK_ID"] = this.cboQcRack.SelectedValue;

                dr["RACK_LOK_FLAG"] = (null == this.chkRackLock.IsChecked || this.chkRackLock.IsChecked == false) ? "N" : "Y";
                dr["RACK_VISBL_FLAG"] = (null == this.chkRackVisible.IsChecked || this.chkRackVisible.IsChecked == false) ? "N" : "Y";
                inRackTable.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTLINE_RACK", "INDATA,IN_RACK", "", dsInput, null);

                this._Dr["EDIT_YN"] = "N";
                #region Rack_kind 조회
                if (this.cboSetRackKind.SelectedValue.ToString().Equals(sRackKindIntrnt))
                {
                    this.SetIntransitCombo();
                }
                #endregion
                 
                #endregion //Rack 정보 저장
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }
        #endregion //Rack 설정 저장
        #endregion // 4.3.1 Rack

        #region 4.3.2 W/H 정보 저장
        /// <summary>
        /// name         : btnSaveWhSet_Click
        /// desc         : W/H 설정 저장
        /// author       : 김선준
        /// create date  : 2024-04-11 오후 21:17:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        ///                2024-07-09 김선준 사용안함 설정 저장 후 Refresh
        /// </summary>
        private void btnSaveWhSet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboWhId.SelectedValue.ToString()) || string.IsNullOrEmpty(this.txtSetWhId.Text)) return;
            //if (null == this.cboSetWhProcId.SelectedValue || string.IsNullOrEmpty(this.cboSetWhProcId.SelectedValue.ToString()))
            //{
            //    Util.MessageInfo("SFU4173");
            //    this.cboSetWhProcId.Focus();
            //    return;
            //}
            //if (string.IsNullOrEmpty(this.txtSetWHKeepDays.Text.Trim()))
            //{
            //    Util.MessageInfo("Input Line Keep Days");
            //    this.txtSetWHKeepDays.Focus();
            //    return;
            //}
            if (this.cboSetWhUse.SelectedValue.ToString().Equals("N"))
            {
                string msg = MessageDic.Instance.GetMessage("SFU1241").Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
                if (MessageBox.Show(msg, "Disable Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            else if  (null == this.cboSetWhMoveRack.SelectedValue || string.IsNullOrEmpty(this.cboSetWhMoveRack.SelectedValue.ToString())) 
            {
                Util.MessageInfo("SUF9002");
                this.cboSetWhMoveRack.Focus();
                return;
            }

            try
            {
                DataSet dsInput = new DataSet();

                DataTable inDataTable = dsInput.Tables.Add("INDATA");
                inDataTable.Columns.Add("IUD", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string));
                inDataTable.Columns.Add("WH_DRW", typeof(Byte[]));
                inDataTable.Columns.Add("WH_TSPC", typeof(double));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("IMGYN", typeof(string));
                inDataTable.Columns.Add("LINE_LIMIT_PROCID", typeof(string));
                inDataTable.Columns.Add("LINE_STCK_KEEP_DAYS", typeof(Int16));
                inDataTable.Columns.Add("MOVING_RACK_ID", typeof(string));
                inDataTable.Columns.Add("LINE_AUTO_STCK_FLAG", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["IUD"] = "U";
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = this._AREAID;
                row["WH_ID"] = this.txtSetWhId.Text;
                if (null != this.objFile)
                {
                    row["IMGYN"] = "Y";
                    row["WH_DRW"] = this.objFile;
                }
                else
                {
                    row["IMGYN"] = "N";
                }
                row["WH_TSPC"] = Convert.ToDouble(this.cboSetWhOpacity.SelectedValue);
                row["LINE_LIMIT_PROCID"] =  this.cboSetWhProcId.SelectedValue;
                row["LINE_STCK_KEEP_DAYS"] = (string.IsNullOrEmpty(this.txtSetWHKeepDays.Text) || this.txtSetWHKeepDays.Text.Equals("0")) ? 7 : Convert.ToDouble(this.txtSetWHKeepDays.Text);
                row["MOVING_RACK_ID"] = this.cboSetWhMoveRack.SelectedValue;
                row["LINE_AUTO_STCK_FLAG"] = this.cboSetWhAutoStock.SelectedValue;
                row["USE_FLAG"] = this.cboSetWhUse.SelectedValue;
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTLINE_WH", "INDATA", "", dsInput, null);
     
                Util.MessageInfo("FM_ME_0215"); //저장하였습니다.

                if (this.cboSetWhUse.SelectedValue.ToString().Equals("N"))
                {
                    #region 창고 "N"처리 
                    this._IsWhAdd = false;

                    cboWhId.ItemsSource = null;
                    this.cboWhId.SelectedValue = null;

                    SetFunctionSw(true);
                    SetClose();

                    SetWHCombo();                         
                    #endregion
                }
                else
                {
                    this._IsWhAdd = true;
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

        /// <summary>
        /// name         : btnImg_Click
        /// desc         : W/H 도면 선택
        /// author       : 김선준
        /// create date  : 2024-04-11 오후 21:21:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnImg_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString())) return;

            try
            {
                #region Image Find
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Image|*.jpg;*.png";

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fileDialog.InitialDirectory = @"\\Client\C$";
                }
                else
                {
                    fileDialog.InitialDirectory = @"C:\";
                }

                if (fileDialog.ShowDialog() == true)
                {
                    if (new System.IO.FileInfo(fileDialog.FileName).Length > 5 * 1024 * 1024) //파일크기 체크
                    {
                        Util.AlertInfo("SFU1926");  //첨부파일 크기는 5M 이하입니다.
                        this.txtSetFileName.Text = string.Empty;
                    }
                    else
                    {
                        this.txtSetFileName.Text = fileDialog.FileName;
                    }
                    this.objFile = File.ReadAllBytes(Util.GetCondition(this.txtSetFileName));

                    #region Image Change
                    Byte[] imageData = (Byte[])this.objFile;
                    var image = new BitmapImage();
                    using (var mem = new MemoryStream(imageData))
                    {
                        mem.Position = 0;
                        image.BeginInit();
                        image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = null;
                        image.StreamSource = mem;
                        image.EndInit();
                    }
                    image.Freeze();
                    this.imgPlan.ImageSource = image;
                    #endregion //Image Change                     
                }
                #endregion //Image Find
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// name         : cboSetWhOpacity_SelectedValueChanged
        /// desc         : W/H 투명도 선택
        /// author       : 김선준
        /// create date  : 2024-04-12 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboSetWhOpacity_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == this._DtSelWh) return;
            if (string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString())) return;

            try
            { 
                if (string.IsNullOrEmpty(cboSetWhOpacity.SelectedValue.ToString()))
                {
                    this.imgPlan.Opacity = 0.4;
                }
                else
                {
                    this.imgPlan.Opacity = Convert.ToDouble(cboSetWhOpacity.SelectedValue);
                    this._DtSelWh.Rows[0]["WH_TSPC"] = Convert.ToDouble(this.cboSetWhOpacity.SelectedValue);
                }
            }
            catch (Exception)
            {
                
            }
        }

        /// <summary>
        /// name         : cboSetWhMoveRack_SelectedValueChanged
        /// desc         : 이동 RACK 선택
        /// author       : 김선준
        /// create date  : 2024-05-02 오후 16:57:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboSetWhMoveRack_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == this._DtSelWh) return;
            if (string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString())) return;

            try
            {
                this._DtSelWh.Rows[0]["MOVING_RACK_ID"] = this.cboSetWhMoveRack.SelectedValue;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// name         : cboSetWhProcId_SelectedValueChanged
        /// desc         : LINE 한계 공정 선택
        /// author       : 김선준
        /// create date  : 2024-05-02 오후 16:57:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboSetWhProcId_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == this._DtSelWh) return;
            if (string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString())) return;

            try
            {
                this._DtSelWh.Rows[0]["LINE_LIMIT_PROCID"] = this.cboSetWhProcId.SelectedValue;
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// name         : txtSetWHKeepDays_TextChanged
        /// desc         : LINE재고유지기간
        /// author       : 김선준
        /// create date  : 2024-05-02 오후 16:57:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSetWHKeepDays_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (null == this._DtSelWh) return;
            if (string.IsNullOrEmpty(this.cboWhId.SelectedValue.ToString())) return;

            try
            {
                if (string.IsNullOrEmpty(this.txtSetWHKeepDays.Text) || this.txtSetWHKeepDays.Text.Equals("0")) this.txtSetWHKeepDays.Text = "7";
                this._DtSelWh.Rows[0]["LINE_STCK_KEEP_DAYS"] = Convert.ToInt16(this.txtSetWHKeepDays.Text);
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// name         : cboSetWhAutoStock_SelectedValueChanged
        /// desc         : LINE 자동 재고 Stock여부
        /// author       : 김선준
        /// create date  : 2024-05-06 오전 10:12:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboSetWhAutoStock_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == this._DtSelWh) return;
            this._DtSelWh.Rows[0]["LINE_AUTO_STCK_FLAG"] = this.cboSetWhAutoStock.SelectedValue;
        }

        /// <summary>
        /// name         : cboSetWhUse_SelectedValueChanged
        /// desc         : W/H 사용여부
        /// author       : 김선준
        /// create date  : 2024-07-08 오후 16:50:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboSetWhUse_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (null == this._DtSelWh) return;
            this._DtSelWh.Rows[0]["USE_FLAG"] = this.cboSetWhUse.SelectedValue;
        }
        #endregion //4.3.2 W/H 정보 저장

        #region 4.3.3 W/H Add
        /// <summary>
        /// name         : btnInitWhInfo_Click
        /// desc         : W/H 목록 다시 불러오기 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 12:23:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnInitWhInfo_Click(object sender, RoutedEventArgs e)
        {
            SetWHAddCombo();
        }

        /// <summary>
        /// name         : SetWHAddCombo
        /// desc         : W/H 목록 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 12:23:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SetWHAddCombo()
        { 
            C1ComboBox cbo = cboWhAdd;

            const string bizRuleName = "DA_BAS_SEL_WHID_CBO_PACK";
            string[] arrColumn = { "LANGID", "AREAID" };
            string[] arrCondition = { LoginInfo.LANGID, this._AREAID}; 
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        /// <summary>
        /// name         : btnSaveWhInfo_Click
        /// desc         : W/H 추가 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 12:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSaveWhInfo_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(cboWhAdd.SelectedValue.ToString())) return;

            try
            {
                DataSet dsInput = new DataSet();

                DataTable inDataTable = dsInput.Tables.Add("INDATA");
                inDataTable.Columns.Add("IUD", typeof(string));
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string));
                inDataTable.Columns.Add("USE_FLAG", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow row = null;
                row = inDataTable.NewRow();
                row["IUD"] = "I";
                row["LANGID"] = LoginInfo.LANGID;
                row["AREAID"] = this._AREAID; 
                row["WH_ID"] = this.cboWhAdd.SelectedValue.ToString();
                row["USE_FLAG"] = "Y";
                row["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(row);
                 
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_OUTLINE_WH", "INDATA", "", dsInput, null);
                this._IsWhAdd = true;

                Util.MessageInfo("FM_ME_0215"); //저장하였습니다.
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            { 
            }
        }
        #endregion //4.3.3 W/H Add

        #endregion 4.3 Setting

        #region #4.4 좌측 Lot List
        /// <summary>
        /// name         : chkHeaderAllList_Checked
        /// desc         : Rack 전체 선택
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 14:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void chkHeaderAllList_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in this.grdSetRackList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }
            this.grdSetRackList.EndEdit();
            this.grdSetRackList.EndEditRow(true);
        }

        /// <summary>
        /// name         : chkHeaderAllList_Unchecked
        /// desc         : Rack 전체 선택 해제
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 14:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void chkHeaderAllList_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in this.grdSetRackList.Rows)
            {
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
            this.grdSetRackList.EndEdit();
            this.grdSetRackList.EndEditRow(true);
        }

        /// <summary>
        /// name         : grdSetRackList_BeginningEdit
        /// desc         : Check 선택
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 14:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void grdSetRackList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid c1DataGrid = (C1DataGrid)sender;
            DataRowView dv = (DataRowView)e.Row.DataItem;

            if (!e.Column.Name.ToUpper().Equals("CHK"))
            {
                e.Cancel = true;
            }
            else {
                dv.Row["CHK"] = !(bool)dv.Row["CHK"];
                
                this.grdSetRackList.EndEdit();
                this.grdSetRackList.EndEditRow(true);
            }
        }

        /// <summary>
        /// name         : txtFilter_KeyDown
        /// desc         : filter
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 14:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
             
            this.chkHeaderAllList.IsChecked = false;
            chkHeaderAllList_Unchecked(this, null); 

            List<DataGridFilterInfo> filterInfoList = new List<DataGridFilterInfo>();
            filterInfoList.Add(new DataGridFilterInfo() { FilterOperation = DataGridFilterOperation.Contains, FilterType = DataGridFilterType.Text, Value = this.txtFilter.Text });
            DataGridFilterState filterState = new DataGridFilterState();
            filterState.FilterInfo = filterInfoList;
            this.grdSetRackList.FilterBy(this.grdSetRackList.Columns["RACK_NAME"], filterState);
        }

        /// <summary>
        /// name         : grdSetRackList_MouseDoubleClick
        /// desc         : 더블 Click시 Rack 기본 위치로 이동
        /// author       : 김선준
        /// create date  : 2024-05-07 오후 16:09:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void grdSetRackList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1DataGrid dg = ((e.Source) as C1DataGrid);
            if (null == dg || dg.SelectedIndex < 0 || null == dg.SelectedItem) return;
            DataRowView dv = dg.SelectedItem as DataRowView;
            if (Convert.ToDouble(dv["X_CODI"]) < -5 || Convert.ToDouble(dv["X_CODI"]) > 980 || Convert.ToDouble(dv["Y_CODI"]) > 755 || Convert.ToDouble(dv["Y_CODI"]) < -5)
            {
                dv["X_CODI"] = 25;
                dv["Y_CODI"] = 25;
            } 
        }
        #endregion //4.4 좌측 Lot List

        #region #4.5 Rack Lot List
        /// <summary>
        /// name         : SearchLotList
        /// desc         : Rack  Lot List 조회
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 04:23:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SearchLotList(string sRackId, string sRackName)
        {
            if (string.IsNullOrEmpty(sRackId)) return;
            
            try
            {
                Util.gridClear(this.grdLotList);
                PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, this.grdLotList.Rows.Count);
                this.txtRackInfo.Text = sRackName;

                PackCommon.DoEvents();
                this.loadingIndicator.Visibility = Visibility.Visible;                

                this._InfoModel.InitVal();
                this._InfoModel.WH_ID = this.cboWhId.SelectedValue.ToString();
                this._InfoModel.LOTLIST_YN = "Y";
                this._InfoModel.RACK_ID = sRackId;   

                DataSet ds = this.SearchInfo();

                if (null != ds.Tables["OUT_LOTLIMIT"] && ds.Tables["OUT_LOTLIMIT"].Rows.Count > 0)
                {
                    #region 분산조회
                    List<string> arLots = ds.Tables["OUT_LOTLIMIT"].AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Field<string>("LOTIDS"))).Select(x => x.Field<string>("LOTIDS")).ToList();

                    fnLotList(arLots, ds.Tables["OUT_LOTLIST"]);
                    #endregion //분산조회
                }
                else
                {
                    Util.GridSetData(this.grdLotList, ds.Tables["OUT_LOTLIST"], FrameOperation);
                    PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, this.grdLotList.Rows.Count);
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }            
        }

        /// <summary>
        /// name         : grdWhRackInfo_MouseDoubleClick
        /// desc         : Rack  Lot List 조회
        /// author       : 김선준
        /// create date  : 2024-04-23 오후 04:34:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void grdWhRackInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid datagrid = (sender as C1.WPF.DataGrid.C1DataGrid);
            if (datagrid.CurrentRow == null || datagrid.CurrentRow.Index < 0) return;

            //string sRackid = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["RACK_ID"].Index).Value);
            //string sRackName = Util.NVC(datagrid.GetCell(datagrid.CurrentRow.Index, datagrid.Columns["RACK_NAME"].Index).Value);
            //SearchLotList(sRackid, sRackName); 
        }

        #region 분산조회
        /// <summary>
        /// name         : fnLotList_Callback
        /// desc         : Rack Lot List를 분산 조회한다.
        /// author       : 김선준
        /// create date  : 2024-08-16 오전 11:27:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void fnLotList(List<string> arLots, DataTable dtFst)
        {
            //List<DataTable> dtLotList = new List<DataTable>();
            DataTable dtCall = dtFst;
            foreach (string item in arLots)
            {
                #region SearchData
                DataSet indataSet = new DataSet();
                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("WH_ID", typeof(string));
                inDataTable.Columns.Add("RACK_ID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("LOTLIST_YN", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("HOLD_YN", typeof(string));

                inDataTable = indataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = this._AREAID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WH_ID"] = this._InfoModel.WH_ID;
                newRow["RACK_ID"] = this._InfoModel.RACK_ID;
                newRow["LOTID"] = item;
                newRow["LOTLIST_YN"] = "Y";
                newRow["HOLD_YN"] = this._InfoModel.HOLD_YN;
                inDataTable.Rows.Add(newRow);

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_OUTLINE_INFO", "INDATA", "OUTDATA_WH,OUTDATA_RACK,OUT_COLOR,OUT_LOTLIST,OUT_RACKCNT,OUT_RACKKIND,OUT_RACKTYPE,OUT_LINE,OUT_PROCESS,OUT_AUTH,OUT_LOTLIMIT", indataSet);

                dtCall.Merge(ds.Tables["OUT_LOTLIST"]);
                #endregion //SearchData                  
            }

            Util.GridSetData(this.grdLotList, dtCall, FrameOperation, true);
            PackCommon.SearchRowCount(ref this.txtGridDetailRowCount, this.grdLotList.Rows.Count);
        }         
        #endregion
        #endregion //Rack Lot List

        #region 5. Excel  
        /// <summary>
        /// name         : btnExcel_Click
        /// desc         : 전체 Lot List 
        /// author       : 김선준
        /// create date  : 2024-04-11 오전 12:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            PackCommon.DoEvents();

            Util.gridClear(this.grdLotExcel);

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                this._InfoModel.InitVal();
                this._InfoModel.WH_ID = this.cboWhId.SelectedValue.ToString();
                this._InfoModel.LOTLIST_YN = "Y";

                DataSet ds = this.SearchInfo();

                if (ds.Tables["OUT_LOTLIST"].Rows.Count == 0)
                {
                    //SFU1905 조회된 Data가 없습니다.
                    Util.MessageInfo("SFU1905");
                    return;
                }

                Util.GridSetData(this.grdLotExcel, ds.Tables["OUT_LOTLIST"], FrameOperation, true);

                if (this.grdLotExcel.Rows.Count > 0) new ExcelExporter().Export(this.grdLotExcel);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
         
        /// <summary>
        /// name         : btnExcelUpload_Click
        /// desc         : Excel 양식 업로드
        /// author       : 김선준
        /// create date  : 2024-04-19 오후 15:00:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion //Excel

        #region #9.Auto Search
        private bool _isAutoSelectTime = false;
        private readonly System.Windows.Threading.DispatcherTimer _dispatcherMainTimer = new System.Windows.Threading.DispatcherTimer();

        /// <summary>
        /// Timer설정
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DispatcherMainTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null) return;

            System.Windows.Threading.DispatcherTimer dpcTmr = sender as System.Windows.Threading.DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();

                    // 0분이면 skip
                    if (dpcTmr.Interval.TotalSeconds.Equals(0)) return;
                    // data조회
                    Iniallize_Screen("AUTO");
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dpcTmr.Interval.TotalSeconds > 0)
                        dpcTmr.Start();
                }
            }));
        }

        /// <summary>
        /// 자동Combo 변경시
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cboAutoSearch_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (_dispatcherMainTimer != null)
                {
                    _dispatcherMainTimer.Stop();

                    int iSec = 0;

                    if (cboAutoSearch?.SelectedValue != null && !cboAutoSearch.SelectedValue.ToString().Equals("") && cboAutoSearch.SelectedValue.ToString() != "SELECT")
                        iSec = int.Parse(cboAutoSearch.SelectedValue.ToString());

                    if (iSec == 0 && _isAutoSelectTime)
                    {
                        _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                        // 자동조회가 사용하지 않도록 변경 되었습니다.
                        Util.MessageValidation("SFU8170");
                        return;
                    }

                    if (iSec == 0)
                    {
                        _isAutoSelectTime = true;
                        return;
                    }

                    _dispatcherMainTimer.Interval = new TimeSpan(0, 0, iSec);
                    _dispatcherMainTimer.Start();

                    if (_isAutoSelectTime)
                    {
                        // 자동조회  %1초로 변경 되었습니다.
                        Util.MessageValidation("SFU5127", cboAutoSearch?.SelectedValue?.ToString());
                    }

                    _isAutoSelectTime = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// Timer 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            Iniallize_Screen("AUTO");
        }
        #endregion //9.Auto Search

        private void txtGridDetailRowCount_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
#if DEBUG
            MessageBox.Show("DEBUG");
#endif
            if (LoginInfo.USERID.Equals("seonjun9"))
            {
                if (tbHold.Visibility == Visibility.Visible)
                {
                    tbHold.Visibility = Visibility.Collapsed;
                    cboHold.Visibility = Visibility.Collapsed;
                }
                else
                {
                    tbHold.Visibility = Visibility.Visible;
                    cboHold.Visibility = Visibility.Visible;
                }
            }
        }
        #endregion
    }

    public class InfoModel 
    {
        public string AREAID = LoginInfo.CFG_AREA_ID;
        public string WH_ID       { get; set; }
        public string RACK_ID     { get; set; }
        public string LOTID       { get; set; }
        public string WHINFO_YN   { get; set; }
        public string RACKINFO_YN { get; set; }
        public string COLOR_YN { get; set; }
        public string LOTLIST_YN  { get; set; }
        public string RACKKIND_YN { get; set; }
        public string RACK_KDN { get; set; }
        public string USE_FLAG    { get; set; }
        public string WH_USE_FLAG { get; set; }
        public string HOLD_YN { get; set; }

        public void InitVal()
        {
            AREAID = LoginInfo.CFG_AREA_ID;
            WH_ID = string.Empty;
            RACK_ID = string.Empty;
            LOTID = string.Empty;
            WHINFO_YN = "N";
            RACKINFO_YN = "N";
            COLOR_YN = "N";
            LOTLIST_YN = "N";
            RACKKIND_YN = "N";
            RACK_KDN = string.Empty;
            USE_FLAG = string.Empty;
            WH_USE_FLAG = string.Empty;
            HOLD_YN = "Y";
        }
    }    
}