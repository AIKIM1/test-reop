/*************************************************************************************
 Created Date : 2025.03.14
      Creator : 김선준
   Decription : Tray-Cell 재구성
--------------------------------------------------------------------------------------
 [Change History]
  2025.03.14  DEVELOPER : Initial Created. 
**************************************************************************************/

using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.PACK001.Class;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_104 : UserControl, IWorkArea
    {
        #region #1.Variable
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();

        const string tagScanTray = "First Scan Tray ID";
        const string tagScanCell = "Scan Cell ID";

        bool bFirstTray = true;
        #endregion //#region #1.Variable

        #region #2.Constructor
        /// <summary>
        /// name         : PACK001_104
        /// desc         : Constructor
        /// author       : 김선준
        /// create date  : 2025-03-14 오후 13:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        public PACK001_104()
        {
            InitializeComponent();
        }
        #endregion //#2.Constructor

        #region #3.Event
        /// <summary>
        /// name         : UserControl_Loaded
        /// desc         : 화면로드
        /// author       : 김선준
        /// create date  : 2025-03-14 오후 13:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
            this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
        }

        /// <summary>
        /// name         : btnInit_Click
        /// desc         : 초기화
        /// author       : 김선준
        /// create date  : 2025-03-14 오후 13:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        #region #3.1 Scan
        /// <summary>
        /// name         : txtScan_KeyDown
        /// desc         : Scan시 Tray/Lot 조회
        /// author       : 김선준
        /// create date  : 2025-03-14 오후 13:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (bFirstTray)
                {
                    fnScanTray();
                }
                else
                {
                    fnScanLot();
                }
            }
        }

        /// <summary>
        /// name         : btnTagetSelectCancel_Click
        /// desc         : 선택 행 취소
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 16:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTagetList.Rows.Count == 0) return;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTagetList.ItemsSource);

                var query = dt.AsEnumerable().Where(x => !x.Field<string>("CHK").ToUpper().Equals("TRUE") && !x.Field<string>("CHK").ToUpper().Equals("1"));

                if (null == query && query.Count() == 0)
                {
                    this.Initialize("2");
                }
                else
                {
                    DataTable dtRst = dt.Clone();
                    foreach (DataRow row in query)
                    {
                        dtRst.Rows.Add(row);
                    }

                    Util.GridSetData(this.dgTagetList, dtRst, FrameOperation);
                    PackCommon.SearchRowCount(ref this.tbTagetListCount, this.dgTagetList.Rows.Count);
                }                 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// name         : btnTagetCancel_Click
        /// desc         : 전체취소
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 16:26:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTagetList.Rows.Count == 0) return;
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //전체 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        this.txtScan.Text = this.txtTrayID.Text;
                        this.fnScanTray();
                    }
                }
            );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion //#3.1 Scan

        #region #3.2 Confirm/Release
        /// <summary>
        /// name         : btnTagetInputComfirm_Click
        /// desc         : Tray/Cell 재구성
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 17:31:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTagetList.Rows.Count == 0) return;

            try
            {                
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("ME_0204"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //작업하신 Tray 정보를 저장하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ReTrayCell();
                    }
                }); 
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// name         : btnTagetInputRelease_Click
        /// desc         : Tray/Cell 해체
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 17:31:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnTagetInputRelease_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgTagetList.Rows.Count == 0) return;

            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1421"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //Release 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        ReTrayCellRelease();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion //#3.2 Confirm/Release

        #region #3.3 Retray 목록조회
        /// <summary>
        /// name         : btnSearch_Click
        /// desc         : 초기화
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 22:45:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                fnRetrayList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// name         : dgSearchResultList_LoadedCellPresenter
        /// desc         : Grid Cell 설정
        /// author       : 김선준
        /// create date  : 2025-03-20 오전 08:55:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name.Equals("CSTID"))
                    { 
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
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

        /// <summary>
        /// name         : dgSearchResultList_MouseDoubleClick
        /// desc         : Retray 상세 조회
        /// author       : 김선준
        /// create date  : 2025-03-20 오전 08:50:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.dgSearchResultList.Rows.Count == 0) return; 

            try
            { 
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = this.dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name.Equals("CSTID"))
                        {
                            this.txtScan.Text = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "CSTID"));
                            this.fnScanTray();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /// <summary>
        /// name         : dgSearchResultList_MouseDoubleClick
        /// desc         : Retray 상세 조회
        /// author       : 김선준
        /// create date  : 2025-03-20 오전 08:50:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new ExcelExporter().Export(dgSearchResultList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion //#3.3 Retray 목록조회
        #endregion //#3.Event

        #region #4.Function
        #region #4.1 초기화
        /// <summary>
        /// name         : Initialize
        /// desc         : 초기화
        /// author       : 김선준
        /// create date  : 2025-03-14 오후 13:14:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void Initialize(string Gubun = "123")
        { 
            if (Gubun.Contains("1"))
            {
                bFirstTray = true;
                this.txtScan.Tag = tagScanTray;

                this.txtTrayID.Text = string.Empty;
                this.txtPRODID.Text = string.Empty;
                this.txtTrayQty.Text = string.Empty;
                this.txtTrayStatus.Text = string.Empty;
                this.ChkPkgFlag.IsChecked = false;

                this.btnTagetSelectCancel.Visibility = Visibility.Visible;
                this.btnTagetCancel.Visibility = Visibility.Visible;
                this.btnTagetInputComfirm.Visibility = Visibility.Visible;
                this.btnTagetInputRelease.Visibility = Visibility.Collapsed;
            }
            if (Gubun.Contains("2"))
            {
                Util.gridClear(this.dgTagetList);
                PackCommon.SearchRowCount(ref this.tbTagetListCount, this.dgTagetList.Rows.Count);
            }

            if (Gubun.Contains("3"))
            {
                this.dtpDateFrom.Text = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                this.dtpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");

                Util.gridClear(this.dgSearchResultList);
                PackCommon.SearchRowCount(ref this.tbSearchListCount, this.dgSearchResultList.Rows.Count);
            }
            if (Gubun.Contains("A"))
            {
                this.btnTagetSelectCancel.Visibility = Visibility.Collapsed;
                this.btnTagetCancel.Visibility = Visibility.Collapsed;
                this.btnTagetInputComfirm.Visibility = Visibility.Collapsed;
                this.btnTagetInputRelease.Visibility = Visibility.Visible;
            }
        }
        #endregion //#4.1 초기화

        #region #4.2 Scan
        /// <summary>
        /// name         : fnScanTray
        /// desc         : Tray Scan
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 14:27:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void fnScanTray()
        {
            if (string.IsNullOrEmpty(this.txtScan.Text.Trim())) return;

            Initialize("12");

            PackCommon.DoEvents();

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;
                 
                DataSet ds = this.SearchInfo(this.txtScan.Text.Trim(), string.Empty);

                if (null != ds && ds.Tables["OUTDATA_TRAY"].Rows.Count > 0)
                {
                    this.txtTrayID.Text = this.txtScan.Text.Trim();
                    this.txtPRODID.Text = string.IsNullOrEmpty( Util.NVC(ds.Tables["OUTDATA_TRAY"].Rows[0]["TOP_PRODID"].ToString())) ? Util.NVC(ds.Tables["OUTDATA_TRAY"].Rows[0]["PRODID"].ToString()): Util.NVC(ds.Tables["OUTDATA_TRAY"].Rows[0]["TOP_PRODID"].ToString());
                    this.txtTrayQty.Text = ds.Tables["OUTDATA_TRAY"].Rows[0]["MAX_LOAD_QTY"].ToString();
                    this.txtTrayStatus.Text = ds.Tables["OUTDATA_TRAY"].Rows[0]["TRAYSTATUS"].ToString();

                    if (ds.Tables["OUTDATA_LOT"].Rows.Count > 0 && !string.IsNullOrEmpty(Util.NVC(ds.Tables["OUTDATA_TRAY"].Rows[0]["RCV_ISS_ID"].ToString())))
                    {
                        Initialize("A");

                        Util.GridSetData(this.dgTagetList, ds.Tables["OUTDATA_LOT"], FrameOperation);
                        PackCommon.SearchRowCount(ref this.tbTagetListCount, this.dgTagetList.Rows.Count);                        
                    }
 
                    if (ds.Tables["OUTDATA_TRAY"].Rows[0]["CSTSTAT"].ToString().Equals("E"))
                    {
                        bFirstTray = false;
                        this.txtScan.Tag = tagScanCell;
                    }
                    else if (null == ds.Tables["OUTDATA_LOT"] || ds.Tables["OUTDATA_LOT"].Rows.Count == 0)
                    {
                        Initialize("A");
                        Util.MessageConfirm("SFU9054", (result) => 
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReTrayCellRelease();
                            }
                        }, ObjectDic.Instance.GetObjectName("Release")); 
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// name         : fnScanLot
        /// desc         : Lot Scan
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 14:27:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void fnScanLot()
        {
            if (string.IsNullOrEmpty(this.txtScan.Text.Trim())) return;

            int iQty = Util.NVC_Int(this.txtTrayQty.Text);
            if (iQty > 0 && iQty == this.dgTagetList.Rows.Count)
            {
                //Tray에 적재 가능한 Cell 수량을 확인하세요.
                Util.MessageInfo("100257");
                return;
            }
            
            PackCommon.DoEvents();

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                DataSet ds = this.SearchInfo(this.txtTrayID.Text, this.txtScan.Text.Trim());

                if (null != ds && ds.Tables["OUTDATA_LOT"].Rows.Count > 0)
                {
                    if (this.dgTagetList.Rows.Count == 0)
                    {
                        Util.GridSetData(this.dgTagetList, ds.Tables["OUTDATA_LOT"], FrameOperation);
                        PackCommon.SearchRowCount(ref this.tbTagetListCount, this.dgTagetList.Rows.Count);
                    }
                    else
                    {
                        DataTable dt = (this.dgTagetList.ItemsSource as DataView).ToTable();

                        string lotid =  Util.NVC(dt.Rows[0]["LOTID"]);
                        string prodid = Util.NVC(dt.Rows[0]["PRODID"]);
                        string from_sloc = Util.NVC(dt.Rows[0]["FROM_SLOC_ID"]);
                        string to_sloc = Util.NVC(dt.Rows[0]["TO_SLOC_ID"]);

                        if (dt.AsEnumerable().Where(x => x.Field<string>("LOTID").Equals(this.txtScan.Text.Trim())).Count() > 0)
                        {
                            //동일한 LOT ID가 있습니다.
                            Util.MessageInfo("SFU1508"); 
                            return;
                        }

                        if (!prodid.Equals(Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["PRODID"])))
                        {
                            //동일한 제품이 아닙니다.
                            Util.MessageInfo("10000035");
                            return;
                        }

                        if (!from_sloc.Equals(Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["FROM_SLOC_ID"])))
                        {
                            //출고창고체크
                            Util.MessageInfo("96129", Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["FROM_SLOC_ID"]));
                            return;
                        }

                        if (!to_sloc.Equals(Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["TO_SLOC_ID"])))
                        {
                            //반품창고체크
                            Util.MessageInfo("96130", Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["TO_SLOC_ID"]));
                            return;
                        }

                        DataRow row = dt.NewRow();
                        row["RCV_ISS_ID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["RCV_ISS_ID"]);
                        row["PALLETID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["PALLETID"]); 
                        row["LOTID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["LOTID"]);
                        row["PRODID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["PRODID"]);
                        row["PROCID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["PROCID"]);
                        row["WIPSTAT"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["WIPSTAT"]);
                        row["RACK_ID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["RACK_ID"]);
                        row["FROM_AREAID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["FROM_AREAID"]);
                        row["FROM_SLOC_ID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["FROM_SLOC_ID"]);
                        row["TO_AREAID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["TO_AREAID"]);
                        row["TO_SLOC_ID"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["TO_SLOC_ID"]);
                        row["LOTTYPE"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["LOTTYPE"]);
                        row["LOTID_RT"] = Util.NVC(ds.Tables["OUTDATA_LOT"].Rows[0]["LOTID_RT"]);
                        dt.Rows.InsertAt(row, 0);

                        Util.GridSetData(this.dgTagetList, dt, FrameOperation);
                        PackCommon.SearchRowCount(ref this.tbTagetListCount, this.dgTagetList.Rows.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message);
            }
            finally
            {
                this.txtScan.Clear();
                this.txtScan.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// name         : SearchInfo
        /// desc         : 조회
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 14:27:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private DataSet SearchInfo(string Cstid, string Lotid)
        {
            #region SearchData
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("FLAG", typeof(string)); 
            inDataTable.Columns.Add("CSTID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("PRODID", typeof(string));
            inDataTable.Columns.Add("LOTTYPE", typeof(string));
            inDataTable.Columns.Add("PKG_LOT_MIX_FLAG", typeof(string));
            inDataTable.Columns.Add("LOTID_RT", typeof(string));

            inDataTable = indataSet.Tables["INDATA"];
            DataRow newRow = inDataTable.NewRow();
            newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["FLAG"] = bFirstTray ? "TRAY" : "LOT";
            newRow["CSTID"] = Cstid;
            newRow["LOTID"] = Lotid;
            newRow["PRODID"] = (this.dgTagetList.Rows.Count > 0) ? Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "PRODID")) : null;
            newRow["LOTTYPE"] = (this.dgTagetList.Rows.Count > 0) ? Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "LOTTYPE")) : null;

            if ((bool)this.ChkPkgFlag.IsChecked == false)
            {
                newRow["PKG_LOT_MIX_FLAG"] = "CHK";
                newRow["LOTID_RT"] = (this.dgTagetList.Rows.Count > 0) ? Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "LOTID_RT")) : null;
            }
 
            inDataTable.Rows.Add(newRow);
            return new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_RETRAY_CELL_INFO", "INDATA", "OUTDATA_TRAY,OUTDATA_LOT", indataSet);
            #endregion //SearchData
        }
        #endregion //#4.2 Scan

        #region #4.3 Retray
        /// <summary>
        /// name         : ReTrayCell
        /// desc         : Tray/Cell 재구성
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 17:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void ReTrayCell()
        {
            try
            { 
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string)); 
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("TOP_PRODID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));
                INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
                 
                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI; 
                dr["RCV_ISS_ID"] = string.Empty;  
                dr["CSTID"] = this.txtTrayID.Text;
                dr["PRODID"] = this.txtPRODID.Text;
                dr["TOP_PRODID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "PRODID"));
                dr["USERID"] = LoginInfo.USERID;
                dr["FROM_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "FROM_AREAID"));
                dr["FROM_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "FROM_SLOC_ID"));
                dr["TO_AREAID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "TO_AREAID"));
                dr["TO_SLOC_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "TO_SLOC_ID"));
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataRow drINLOT = null;
                DataTable IN_LOT = new DataTable();
                IN_LOT.TableName = "IN_LOT";
                IN_LOT.Columns.Add("LOTID", typeof(string));

                int iDiff = 0;
                string lotidrt = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[0].DataItem, "LOTID_RT"));

                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    if (!lotidrt.Equals(Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID_RT"))))
                    {
                        iDiff++;
                    }
                    drINLOT = IN_LOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    IN_LOT.Rows.Add(drINLOT);
                }

                //혼입여부 체크
                if (iDiff > 0)
                {
                    //혼합허용
                    if ((bool)this.ChkPkgFlag.IsChecked == false)
                    {
                        Util.MessageInfo("96214");
                        return;
                    }

                    Util.MessageConfirm("SFU9055", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            dsInput.Tables.Add(IN_LOT);

                            loadingIndicator.Visibility = Visibility.Visible;

                            new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETRAY_CELL", "INDATA,IN_LOT", "", dsInput, null);

                            //Tray 정보 변경을 완료하였습니다..
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ME_0074"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                            this.Initialize("12");
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, ObjectDic.Instance.GetObjectName("Confirm"));
                }
                else
                {
                    dsInput.Tables.Add(IN_LOT);

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETRAY_CELL", "INDATA,IN_LOT", "", dsInput, null);

                    //Tray 정보 변경을 완료하였습니다..
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ME_0074"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    this.Initialize("12");
                    loadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion //#4.3 Retray

        #region #4.4 Retray Release
        /// <summary>
        /// name         : ReTrayCellRelease
        /// desc         : Tray/Cell 해체
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 17:18:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void ReTrayCellRelease()
        {
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string)); 
                INDATA.Columns.Add("CSTID", typeof(string)); 
                INDATA.Columns.Add("USERID", typeof(string));
 
                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;  
                dr["CSTID"] = this.txtTrayID.Text; 
                dr["USERID"] = LoginInfo.USERID; ;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);
                 
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETRAY_CELL_RELEASE", "INDATA", "", dsInput, null);

                //Tray 정보 변경을 완료하였습니다..
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("ME_0074"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                this.Initialize();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion //#4.4 Retray Release

        #region #4.4 Retray List
        /// <summary>
        /// name         : SearchInfo
        /// desc         : 조회
        /// author       : 김선준
        /// create date  : 2025-03-19 오후 14:27:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void fnRetrayList()
        {
            try
            { 
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string)); 
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string)); 

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID; 
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd"); 
                RQSTDT.Rows.Add(dr);
                 
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETRAY_LIST", "RQSTDT", "RSLTDT", RQSTDT);


                Util.GridSetData(this.dgSearchResultList, dtResult, FrameOperation);
                PackCommon.SearchRowCount(ref this.tbSearchListCount, this.dgSearchResultList.Rows.Count); 
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Message);
            }
        }
        #endregion //#4.4 Retray List
        #endregion //#4.Function
    }

}
