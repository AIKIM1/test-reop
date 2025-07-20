/*************************************************************************************
 Created Date : 2023.05.29
      Creator : 
   Decription : 포장 출고 (Location 관리)
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.29  주재홍 : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어
  2024.01.29  백광영 : 스캔팔레트 리스트 없을때 요청팔레트 체크로직 오류 수정
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using LGC.GMES.MES.CMM001;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using System.Threading;
using System.Windows.Media.Animation;
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_383 : UserControl, IWorkArea
    {
        SolidColorBrush redBrush = new SolidColorBrush(Colors.BlueViolet);
        private readonly DispatcherTimer _monitorTimer = new DispatcherTimer();

        private int _keyOrderListIndex = -1;
        private int _keyRequestHistoryOderListIndex = -1;
        private int _sScanQty = 0;
        private int _sReqQty = 0;

        private string _sFROM_AREAID = string.Empty;
        private string _sFROM_LOC = string.Empty;
        private string _sTO_SHOPID = string.Empty;
        private string _sTO_AREAID = string.Empty;
        private string _sTO_LOC = string.Empty;
        private string _sSHIP_WAIT_YN = string.Empty;

        private string _sINTRANSIT_SLOC_ID = string.Empty;

        private string _keyCELL_SPLY_REQ_ID = string.Empty;
        private string _keyCELL_SPLY_RSPN_ID = string.Empty;

        private string _sRCV_ISS_ID = string.Empty;
        private string _sPRODID = string.Empty;

        //Return Lot Type 
        private string _ReturnLotType = string.Empty;

        List<string> _MColumns1;
        List<string> _MColumns2;

        public COM001_383()
        {
            InitializeComponent();

            InitCombo();
            InitColumnsList();
            InitializeControls();                       // Loading
            InitializeTab2Controls();                   // Loading
            //TimerSetting();

            _keyCELL_SPLY_REQ_ID = string.Empty;
            _keyCELL_SPLY_RSPN_ID = string.Empty;

            GetOrderList(null , false , false);         // Loading

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            string _cboCase = "cboArea";
            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.NONE, sCase: _cboCase);

            SetcboReqStatus(cboReqStatus);
            SetcboReqType(cboReqType);
        }

        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }


        private void SetcboReqStatus(C1ComboBox cbo)
        {
            try
            {
                if (cboAreaHist.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_RSPN_STAT", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "COMBO_NAME";
                cbo.SelectedValuePath = "COMBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, "COMBO_CODE", "COMBO_NAME", "ALL").Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetcboReqType(C1ComboBox cbo)
        {
            try
            {
                if (cboAreaHist.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_RSPN_TYPE", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "COMBO_NAME";
                cbo.SelectedValuePath = "COMBO_CODE";

                cbo.ItemsSource = AddStatus(dtResult, "COMBO_CODE", "COMBO_NAME", "ALL").Copy().AsDataView();
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns2 = new List<string>();
        }

        private void InitializeTab2Controls()
        {
            Util.gridClear(dgRequestHistoryOrderList);
            Util.gridClear(dgHistoryPalletList);
            Util.gridClear(dgScanedPalletList);

        }

        private void InitializeControls()
        {
            Util.gridClear(dgOrderList);

            InitializeSubControls();
        }

        private void InitializeSubControls()
        {
            Util.gridClear(dgOrderRequestPalletList);
            Util.gridClear(dgRequestPalletList);
            Util.gridClear(dgScanPalletList);

            txtPalletCSTID.Text = string.Empty;
            txtScanReqQty.Text = string.Empty;

        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            InitializeControls();                     // Search 버튼클릭

            _keyOrderListIndex = -1;
            _keyCELL_SPLY_REQ_ID = string.Empty;
            _keyCELL_SPLY_RSPN_ID = string.Empty;

            GetOrderList(null , false , false);  // Search 버튼클릭

            // Request arrived 설정
            HideRequestInfoMode();
            //TimerSetting();
        }

        private void txtPalletCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtPalletCSTID.Text.Equals("") || txtPalletCSTID.Text == null) return;

                if (!CheckSelectOrderList())     // txtPalletCSTID_KeyDown
                {
                    
                    Util.MessageValidation("SFU1654");  //선택된 요청이 없습니다.
                    txtPalletCSTID.Text = string.Empty;
                    txtPalletCSTID.Focus();
                    return;
                }

                if (_sScanQty + 1 > _sReqQty)
                {
                    
                    Util.MessageValidation("SFU8563");  // 스캔 수량이 요청 수량을 초과 하였습니다.
                    txtPalletCSTID.Text = string.Empty;
                    txtPalletCSTID.Focus();
                    return;
                }

                DataTable dt = ((DataView)dgOrderList.ItemsSource).Table;
                DataRow _curOLRow = dt.Rows[_keyOrderListIndex];
                string _type = Util.NVC(_curOLRow["CELL_SPLY_TYPE_CODE"]);

                // scan Pallet 가 없을경우는 체크안함
                if (dgScanPalletList.ItemsSource != null)
                {
                    DataTable psdt = ((DataView)dgScanPalletList.ItemsSource).Table;
                    string _scanCode = txtPalletCSTID.Text;
                    foreach (DataRow row in psdt.Rows)
                    {
                        string _rBOX = row["BOXID"].ToString();
                        string _rCST = row["PLLT_BCD_ID"].ToString();
                        if (_scanCode.Equals(_rBOX) || _scanCode.Equals(_rCST))
                        {
                           
                            Util.MessageValidation("SFU8467", _scanCode);   //입력할 파일에 동일한 BOX ID가 존재합니다.[%1]
                            txtPalletCSTID.Text = string.Empty;
                            txtPalletCSTID.Focus();
                            return;
                        }
                    }

                    // 첫번째 스캔값 Lottype
                    _ReturnLotType = string.Empty;
                    if (psdt.Columns.Contains("LOTTYPE") && psdt.Rows.Count > 0)
                    {
                        _ReturnLotType = Util.NVC(psdt.Rows[0]["LOTTYPE"].ToString());
                    }
                }

                if (!_type.Equals("CELL"))
                {
                    // 요청 대상 Pallet Validation
                    if (!GetDA_PRD_GET_CELL_PLLT_SHIP_REQ_PLLT_LIST(_curOLRow))
                    {
                        return;
                    }

                    /* ****************************************************************************
                        BR_CHK_SHIP_PALLET_BX 먼저 호출해서 데이타 있으면   
                        Biz(BR_PRD_REG_CELL_PLLT_SHIP_RSPN_PLLT) 호출 
                       ****************************************************************************/
                    if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                        GetBR_CHK_SHIP_PALLET_BX(_curOLRow);             // 활성화동
                    else
                        GetBR_PRD_GET_PALLET_INFO_FOR_SHIP(_curOLRow);   // 조립동
                }
                else
                {
                    RegistScanPalletResult(_curOLRow);  // txtPalletCSTID_KeyDown
                }

            }
        }


        private void RegistScanPalletResult(DataRow _curOLRow)
            {

                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                INDATA.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
                INDATA.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("INSUSER", typeof(string));
                INDATA.Columns.Add("UPDUSER", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));

                INDATA_BOXID.TableName = "INDATA_BOXID";
                INDATA_BOXID.Columns.Add("BOXID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["CELL_SPLY_REQ_ID"] = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
                dr["CELL_SPLY_RSPN_ID"] = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();
                dr["CELL_SPLY_TYPE_CODE"] = _curOLRow["CELL_SPLY_TYPE_CODE"].ToString();
                dr["PRODID"] = _curOLRow["PRODID"].ToString();
                dr["INSUSER"] = LoginInfo.USERID;
                dr["UPDUSER"] = LoginInfo.USERID;
                dr["LANGID"] = LoginInfo.LANGID;

                INDATA.Rows.Add(dr);

                DataRow drbox = INDATA_BOXID.NewRow();
                drbox["BOXID"] = getPalletBCD(txtPalletCSTID.Text);  // 
                INDATA_BOXID.Rows.Add(drbox);

                try
                {
                    string sBizName = "BR_PRD_REG_CELL_PLLT_SHIP_RSPN_PLLT";
                    new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INDATA_BOXID", null, inDataSet);

                    this.txtPalletCSTID.Text = string.Empty;
                    this.txtPalletCSTID.Focus();

                    GetScanPalletList(_curOLRow, false , false);  // 등록 결과 그리드에 조회

                }
                catch (Exception ex)
                {
                    txtPalletCSTID.Text = string.Empty;
                    txtPalletCSTID.Focus();

                    Util.MessageException(ex);
                }
             }


        private void ShipmentBR_CHK_SHIP_PALLET_BX(DataRow _curOLRow , DataRow _curSPRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("SHIP_WAIT_YN", typeof(string));
            INDATA.Columns.Add("LOTTYPE", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["BOXID"] = _curSPRow["BOXID"].ToString();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USERID"] = LoginInfo.USERID;
            string _sSHIP_WAIT_YN = string.Empty;
            if (ChkToSloc())
                _sSHIP_WAIT_YN = "Y";
            else
                _sSHIP_WAIT_YN = "N";
            dr["SHIP_WAIT_YN"] = Util.NVC(_sSHIP_WAIT_YN).Equals(string.Empty) ? null : _sSHIP_WAIT_YN;
            dr["LOTTYPE"] = _curSPRow["LOTTYPE"].ToString(); 

            INDATA.Rows.Add(dr);


                ShowLoadingIndicator();   // Shipment 활성화동

                string sBizName = "BR_CHK_SHIP_PALLET_BX";
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA", (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        int _sBoxCount = result.Tables["OUTDATA"].Rows.Count;
                        if (_sBoxCount > 0)
                            _sINTRANSIT_SLOC_ID = result.Tables["OUTDATA"].Rows[0]["INTRANSIT_SLOC_ID"].ToString();

                        string exMsg = ShipmentFDong(_curOLRow);

                        if (!string.IsNullOrEmpty(exMsg))
                        {
                            Util.MessageValidation(exMsg);  // SHIPMENT F DONG
                            return;
                        }

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }


                }, inDataSet);
        }

        private bool GetDA_PRD_GET_CELL_PLLT_SHIP_REQ_PLLT_LIST(DataRow _dRow)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["CELL_SPLY_REQ_ID"] = Util.NVC(_dRow["CELL_SPLY_REQ_ID"]);
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_REQ_PLLT_LIST";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                if (result != null && result.Rows.Count > 0)
                {
                    string _boxid = getPalletBCD(txtPalletCSTID.Text);
                    var query = result.AsEnumerable().Where(x => x.Field<string>("BOXID").Equals(_boxid));
                    if (query.Count() < 1)
                    {
                        // [%1]는 요청된 Pallet가 아닙니다.
                        Util.MessageValidation("SFU8559", e =>
                        {
                            txtPalletCSTID.Text = string.Empty;
                            txtPalletCSTID.Focus();
                        }, _boxid);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtPalletCSTID.Text = string.Empty;
                txtPalletCSTID.Focus();
                return false;
            }
            return true;
        }

        private void GetBR_CHK_SHIP_PALLET_BX(DataRow _curOLRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("SHIP_WAIT_YN", typeof(string));
            INDATA.Columns.Add("LOTTYPE", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["BOXID"] = getPalletBCD(txtPalletCSTID.Text);
            dr["LANGID"] = LoginInfo.LANGID;
            dr["USERID"] = LoginInfo.USERID;
            string _sSHIP_WAIT_YN = string.Empty;
            if (ChkToSloc())
                _sSHIP_WAIT_YN = "Y";
            else
                _sSHIP_WAIT_YN = "N";
            dr["SHIP_WAIT_YN"] = Util.NVC(_sSHIP_WAIT_YN).Equals(string.Empty) ? null : _sSHIP_WAIT_YN;

            // LotType 첫번째 스캔시 null, 두번째 스캔 시 첫번째 LotType 리턴값으로 호출
            dr["LOTTYPE"] = dgScanPalletList.Rows.Count == 0 ? null : _ReturnLotType;

            INDATA.Rows.Add(dr);

            string sBizName = "BR_CHK_SHIP_PALLET_BX";
            ShowLoadingIndicator();     // Scan Pallet List 활성화동
            new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA", (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            HiddenLoadingIndicator();

                            Util.MessageException(ex);
                            txtPalletCSTID.Text = string.Empty;
                            txtPalletCSTID.Focus();

                            return;
                        }

                        /* ****************************************************************************
                            BR_CHK_SHIP_PALLET_BX 먼저 호출해서 데이타 있으면   
                            From To Location 을 구하여 Biz(BR_PRD_REG_CELL_PLLT_SHIP_RSPN_PLLT) 호출 
                            ****************************************************************************/
                        int _sBoxCount = result.Tables["OUTDATA"].Rows.Count;
                        if (_sBoxCount > 0)
                        {
                            GetFromLOC(_curOLRow);
                            GetToLOC(_curOLRow);
                            RegistScanPalletResult(_curOLRow);  // F동 Scan Pallet 추가
                        }

                        HiddenLoadingIndicator();
                    }
                    catch (Exception bizex)
                    {
                        HiddenLoadingIndicator();

                        txtPalletCSTID.Text = string.Empty;
                        txtPalletCSTID.Focus();
                        Util.MessageException(bizex);
                    }

                }, inDataSet);


        }

        private void ShipmentBR_PRD_GET_PALLET_INFO_FOR_SHIP(DataRow _curOLRow , DataRow _curSPRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["BOXID"] = _curSPRow["BOXID"].ToString();
            dr["LANGID"] = LoginInfo.LANGID;
            INDATA.Rows.Add(dr);

            ShowLoadingIndicator();   // Shipment 조립동
            string sBizName = "BR_PRD_GET_PALLET_INFO_FOR_SHIP";

                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA", (result, bizex) =>
                {
                    HiddenLoadingIndicator();

                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        int _sBoxCount = result.Tables["OUTDATA"].Rows.Count;
                        if (_sBoxCount > 0) _sINTRANSIT_SLOC_ID = result.Tables["OUTDATA"].Rows[0]["INTRANSIT_SLOC_ID"].ToString();

                        string exMsg = ShipmentADong(_curOLRow);  // SHIPMENT A DONG

                        if (!string.IsNullOrEmpty(exMsg)) Util.MessageValidation(exMsg);  // SHIPMENT A DONG
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
        }

        private void GetBR_PRD_GET_PALLET_INFO_FOR_SHIP(DataRow _curOLRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["BOXID"] = getPalletBCD(txtPalletCSTID.Text);
            dr["LANGID"] = LoginInfo.LANGID;
            INDATA.Rows.Add(dr);

            ShowLoadingIndicator();  // Scan Pallet 조립동
            string sBizName = "BR_PRD_GET_PALLET_INFO_FOR_SHIP";

            new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA", (result, bizex) =>
            {
                try
                {
                    if (bizex != null)
                    {
                        HiddenLoadingIndicator();

                        Util.MessageException(bizex);
                        txtPalletCSTID.Text = string.Empty;
                        txtPalletCSTID.Focus();
                        return;
                    }

                    int _sBoxCount = result.Tables["OUTDATA"].Rows.Count;
                    if (_sBoxCount > 0)
                    {
                        GetFromLOC(_curOLRow);
                        GetToLOC(_curOLRow);
                        RegistScanPalletResult(_curOLRow);  // A동 Scan Pallet 추가
                    }
                    HiddenLoadingIndicator();
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();

                    txtPalletCSTID.Text = string.Empty;
                    txtPalletCSTID.Focus();
                    Util.MessageException(ex);
                }

            }, inDataSet);

        }
        private void btnRowDelete_Click(object sender, RoutedEventArgs e)
        {
            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as Button).Parent)).Row.Index;
            DataTable dt = ((DataView)dgScanPalletList.ItemsSource).Table;
            DataRow _curScanRow = dt.Rows[idx];

            DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
            DataRow _olcurRow = oldt.Rows[_keyOrderListIndex];

            SetDeleteScanPalletList(_curScanRow , _olcurRow);

        }


        private void SetDeleteScanPalletList(DataRow row , DataRow _olcurRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");

            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("PLLT_BCD_ID", typeof(string));
            INDATA.Columns.Add("DEL_FLAG", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow drReq = INDATA.NewRow();
            drReq["CELL_SPLY_RSPN_ID"] = _olcurRow["CELL_SPLY_RSPN_ID"].ToString();
            drReq["CELL_SPLY_REQ_ID"] = _olcurRow["CELL_SPLY_REQ_ID"].ToString();
            drReq["BOXID"] = row["BOXID"].ToString();
            drReq["PLLT_BCD_ID"] = row["PLLT_BCD_ID"].ToString();
            drReq["DEL_FLAG"] = "Y";
            drReq["USERID"] = LoginInfo.USERID;

            INDATA.Rows.Add(drReq);

            ShowLoadingIndicator();   // Scan Pallet Delete 버튼

            string _bizRol = "BR_PRD_UPD_CELL_PLLT_SHIP_RSPN_PLLT";
            new ClientProxy().ExecuteService_Multi(_bizRol, "INDATA", null, (result, bizex) =>
            {
                try
                {
                    if (bizex != null)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(bizex);
                        return;
                    }
                    else
                    {
                        GetScanPalletList(_olcurRow, true, false);   // 삭제 결과 그리드에 조회

                        txtPalletCSTID.Text = "";
                        txtPalletCSTID.Focus();
                    }
                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }

                //HiddenLoadingIndicator();

            }, inDataSet);

        }

        private void GetOrderList(DataRow _curOLRow , bool shipmentYN , bool deleteYN)
        {
            try
            {
                InitializeSubControls();

                if (!shipmentYN)
                {
                    loadingIndicator.Visibility = Visibility.Visible;       // Shipment 아닌경우 GetOrderList
                } 
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_SHIP_REQ_ORDER_LIST";

                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, bizex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizex != null)
                        {

                            Util.MessageException(bizex);
                            return;
                        }

                        if (result.Rows.Count > 0)
                        {
                            Util.GridSetData(dgOrderList, result, FrameOperation, true);

                            //if (!string.IsNullOrEmpty(_keyCELL_SPLY_REQ_ID) && result.Rows.Count > 0 && !shipmentYN)
                            //{
                            //    DataTable olapdt = ((DataView)dgOrderList.ItemsSource).Table;
                            //    int idx = -1;
                            //    foreach (DataRow row in olapdt.Rows)
                            //    {
                            //        idx++;
                            //        string _rREQ = row["CELL_SPLY_REQ_ID"].ToString();
                            //        string _rRSPN = row["CELL_SPLY_RSPN_ID"].ToString();
                            //        if (_rREQ.Equals(_keyCELL_SPLY_REQ_ID) && _rRSPN.Equals(_keyCELL_SPLY_RSPN_ID))
                            //        {
                            //            _keyOrderListIndex = idx;

                            //            _keyCELL_SPLY_REQ_ID = string.Empty;
                            //            _keyCELL_SPLY_RSPN_ID = string.Empty;

                            //            break;
                            //        }
                            //    }

                            //    DataTableConverter.SetValue(dgOrderList.Rows[_keyOrderListIndex].DataItem, "CHK", true);
                            //    int _sIdx = _keyOrderListIndex;                        //***********************************//
                            //    dgOrderList.SelectedIndex = _keyOrderListIndex;        // KEY Index  변동에 의한 Index 보존
                            //    _keyOrderListIndex = _sIdx;                            //***********************************//

                            //    // REQUEST 경우 SCAN 입력 불가
                            //    string _sCELL_SPLY_STAT_CODE = _curOLRow["CELL_SPLY_STAT_CODE"].ToString();

                            //    txtPalletCSTID.IsEnabled = true;
                            //    if (_sCELL_SPLY_STAT_CODE.Equals("REQUEST"))
                            //        txtPalletCSTID.IsEnabled = false;


                            //    ViewSubGridProcess();   // GetOrderList

                            //}

                            if (deleteYN) Util.MessageInfo("SFU1273");  // 삭제 되었습니다.

                            if (!shipmentYN) HiddenLoadingIndicator();
                        }
                    }


                    catch (Exception ex)
                    {
                        if (!shipmentYN) HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                if (!shipmentYN) HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                
            }
        }

        private void ViewSubGridProcess()
        {

            if (_keyOrderListIndex > -1)
            {
                DataTable dt = ((DataView)dgOrderList.ItemsSource).Table;
                DataRow _curOLRow = dt.Rows[_keyOrderListIndex];

                GetScanPalletList(_curOLRow , false , true);   // Radio Box 서브그리드 조회 
            }
        }

        private void GetFromLOC(DataRow _curRow)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_FROM_SLOC_ID_BY_AREAID";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                if (result.Rows.Count > 0)
                {
                    _sFROM_AREAID = result.Rows[0]["AREAID"].ToString();
                    _sFROM_LOC = result.Rows[0]["SLOC_ID"].ToString();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtPalletCSTID.Text = string.Empty;
                txtPalletCSTID.Focus();
            }
            finally
            {
            }
        }

        private void GetToLOC(DataRow _curRow)
        {

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("FROM_AREAID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["FROM_AREAID"] = LoginInfo.CFG_AREA_ID;

                string _sCELL_SPLY_TYPE_CODE = Util.NVC(_curRow["CELL_SPLY_TYPE_CODE"]);
                switch (_sCELL_SPLY_TYPE_CODE)
                {
                    case "MES":
                    case "PACK":
                        RQSTDT.Columns.Add("TO_AREAID", typeof(string));
                        dr["TO_AREAID"] = _curRow["CELL_SPLY_TYPE"];
                        _sTO_AREAID = _curRow["CELL_SPLY_TYPE"].ToString();
                        break;
                    case "CUST":
                        RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                        dr["SHIPTO_ID"] = _curRow["CELL_SPLY_TYPE"].ToString();
                        break; 
                    default:
                        break;
                }
                RQSTDT.Rows.Add(dr);


                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_TO_SLOC_ID_BY_AREA";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                if (result.Rows.Count > 0)
                {
                    _sTO_SHOPID = result.Rows[0]["SHIPTO_ID"].ToString();
                    _sTO_LOC = result.Rows[0]["TO_SLOC_ID"].ToString();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                txtPalletCSTID.Text = string.Empty;
                txtPalletCSTID.Focus();

            }
            finally
            {
            }
        }


        private void GetRequestPalletList(string _Prodid)
        {
            ShowLoadingIndicator();
            DoEvents();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["PRODID"] = _Prodid;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_REQ_ABLE_LOCATION";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);
                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgOrderRequestPalletList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
                //gridOrderRequestPalletList.Visibility = Visibility.Visible;
                //gridRequestPalletList.Visibility = Visibility.Collapsed;
            }
        }

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void GetCellRequestPalletList(DataRow _curRow)
        {
            try
            {

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                DataRow dr = INDATA.NewRow();
                dr["CELL_SPLY_REQ_ID"] = _curRow["CELL_SPLY_REQ_ID"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_SHIP_REQ_PLLT_LIST";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", INDATA);

                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgRequestPalletList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
            finally
            {
                if (dgRequestPalletList.GetRowCount() == 0)
                {
                    gridOrderRequestPalletList.Visibility = Visibility.Visible;
                    gridRequestPalletList.Visibility = Visibility.Collapsed;
                    btnLocation.Visibility = Visibility.Visible;
                }
                else
                {
                    gridOrderRequestPalletList.Visibility = Visibility.Collapsed;
                    gridRequestPalletList.Visibility = Visibility.Visible;
                    btnLocation.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void GetScanPalletList(DataRow _curOLRow , bool _updateYN , bool _rbChkYN)
        {

            Util.gridClear(dgScanPalletList);
            txtScanReqQty.Text = string.Empty;

            DataSet inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");

            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));
            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["CELL_SPLY_RSPN_ID"] = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();
            dr["LANGID"] = LoginInfo.LANGID;
            INDATA.Rows.Add(dr);

            ShowLoadingIndicator();   // GetScanPalletList 

            string sBizName = "BR_PRD_GET_CELL_PLLT_SHIP_RSPN_PLLT_LIST";
            new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA", (result, bizex) =>
            {

                try
                {
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }


                    DataTable dt = result.Tables["OUTDATA"];
                    if (dt.Rows.Count > 0) Util.GridSetData(dgScanPalletList, dt, null, true);

                    /**********************************************************************************
                       Scan Pallet 처리시 상단 그리드 읽을 필요 없슴 , 완료메세지 , Registe 또는 Delete
                    ***********************************************************************************/

                    _sScanQty = 0;
                    _sScanQty = dt.Rows.Count;
                    _sReqQty = int.Parse(_curOLRow["REQ_QTY"].ToString());

                    txtScanReqQty.Text = _sScanQty.ToString() + " / " + _sReqQty.ToString();

                    /**********************************************************************************
                        OrderList RadioBox 선택시 그리드 처리
                    ***********************************************************************************/
                    if (_rbChkYN)
                    {
                        _sFROM_AREAID = string.Empty;
                        _sFROM_LOC = string.Empty;
                        _sTO_AREAID = string.Empty;
                        _sTO_LOC = string.Empty;

                        string _type = Util.NVC(_curOLRow["CELL_SPLY_TYPE_CODE"]);

                        if (!_type.Equals("CELL"))
                        {
                            GetFromLOC(_curOLRow);
                            GetToLOC(_curOLRow);

                            if (ChkToSloc())
                                _sSHIP_WAIT_YN = "Y";
                            else
                                _sSHIP_WAIT_YN = "N";
                        }

                        GetCellRequestPalletList(_curOLRow);
                        //switch (_type)
                        //{
                        //    case "MES":
                        //    case "CUST":
                        //        //GetRequestPalletList(_curOLRow);
                        //        gridOrderRequestPalletList.Visibility = Visibility.Visible;
                        //        gridRequestPalletList.Visibility = Visibility.Collapsed;
                        //        btnLocation.Visibility = Visibility.Visible;
                        //        break;
                        //    case "CELL":
                        //    case "PACK":
                        //        GetCellRequestPalletList(_curOLRow);
                        //        break;
                        //    default:
                        //        //GetRequestPalletList(_curOLRow);
                        //        gridOrderRequestPalletList.Visibility = Visibility.Visible;
                        //        gridRequestPalletList.Visibility = Visibility.Collapsed;
                        //        btnLocation.Visibility = Visibility.Visible;
                        //        break;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    HiddenLoadingIndicator();
                }

            }, inDataSet);
        }


        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //_monitorTimer.Stop();
        }

        private void TimerSetting()
        {
            int second = 60;

            _monitorTimer.Tick += _dispatcherTimer_Tick;
            _monitorTimer.Interval = new TimeSpan(0, 0, second);

            _monitorTimer.Start();
        }

        
        private void rbOrderListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked)
            {
                int _newIndex = ((DataGridCellPresenter)rb.Parent).Row.Index;

                // scroll 일때 RadiButton Click Event 방지.
                if (_keyOrderListIndex == _newIndex) return; else _keyOrderListIndex = _newIndex;

                C1DataGrid dg = ((DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {

                    if (_keyOrderListIndex == i) DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, true);
                      else                       DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                // 해당 row 선택
                dgOrderList.SelectedIndex = _keyOrderListIndex;

                InitializeSubControls();

                ViewSubGridProcess();  // Radio Button Select 변경

                DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
                DataRow _curOLRow = oldt.Rows[_keyOrderListIndex];

                // REQUEST 경우 SCAN 입력 불가
                string _sCELL_SPLY_STAT_CODE = _curOLRow["CELL_SPLY_STAT_CODE"].ToString();

                txtPalletCSTID.IsEnabled = true;
                if (_sCELL_SPLY_STAT_CODE.Equals("REQUEST"))   txtPalletCSTID.IsEnabled = false;

                _keyCELL_SPLY_REQ_ID = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
                _keyCELL_SPLY_RSPN_ID = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();

                _sPRODID = Util.NVC(DataTableConverter.GetValue(dg.Rows[_newIndex].DataItem, "PRODID"));
            }
        }

        private void dgOrderList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }
                if (e.Cell.Column.Name == null)
                    return;

                string _col = e.Cell.Column.Name.ToString();

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (e.Cell.Column.Name != null)
                    {
                        // 자동공급여부
                        string _autoFlag = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "AUTO_LOGIS_FLAG"));
                        // 요청구분
                        string _splytype = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CELL_SPLY_TYPE_CODE"));

                        if (_col == "AUTO_FLAG" && _autoFlag.Equals("Y"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        }
                        else if (_col == "AUTO_FLAG" && _autoFlag.Equals("N") && _splytype.Equals("MES"))
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.Pink);
                        }
                        else
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                            e.Cell.Presenter.FontWeight = FontWeights.Regular;
                        }
                    }
                }
            }));
        }

        private void dgAvailableLocation_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                };

                // 해당 Row 값 가져오기
                //string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));

                // 해당 Row 색 변경
                if (e.Cell.Row.Index == 0)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                }
                if (e.Cell.Row.Index == 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }

            }));

        }

        private void _popupPalletInfoLoad_Closed(object sender, EventArgs e)
        {
            COM001_383_PALLET_INFO runStartWindow = sender as COM001_383_PALLET_INFO;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {

            }
        }


        private void ShowPopupPalletInfo(DataRowView drvRow)
        {

            try
            {
                COM001_383_PALLET_INFO _popupLoad = new COM001_383_PALLET_INFO();
                _popupLoad.FrameOperation = FrameOperation;

                if (_popupLoad != null)
                {
                    object[] Parameters = new object[7];
                    Parameters[0] = drvRow["AREAID"].ToString();
                    Parameters[1] = drvRow["WH_PHYS_PSTN_CODE"].ToString();
                    Parameters[2] = drvRow["WH_ID"].ToString();
                    Parameters[3] = drvRow["RACK_ID"].ToString();
                    Parameters[4] = drvRow["PRJT_NAME"].ToString();
                    Parameters[5] = drvRow["EQSGID"].ToString();
                    Parameters[6] = drvRow["PRODID"].ToString();

                    C1WindowExtension.SetParameters(_popupLoad, Parameters);

                    _popupLoad.Closed += new EventHandler(_popupPalletInfoLoad_Closed);
                    _popupLoad.ShowModal();
                    _popupLoad.CenterOnScreen();

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

        private void btnShippingOrder_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                COM001_383_SHIPPING_ORDER popupSO = new COM001_383_SHIPPING_ORDER();
                popupSO.FrameOperation = FrameOperation;


                if (ValidationGridAdd(popupSO.Name.ToString()) == false)
                    return;

                object[] Parameters = new object[1];
                Parameters[0] = string.Empty;
                C1WindowExtension.SetParameters(popupSO, Parameters);

                popupSO.Closed += new EventHandler(popupSO_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popupSO.ShowModal()));

                /*
                if (puHold != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = string.Empty;
                    C1WindowExtension.SetParameters(puHold, Parameters);

                    puHold.Closed += new EventHandler(puHold_Closed);
                    grdMain.Children.Add(puHold);
                    puHold.BringToFront();
                }
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

        }


        private bool ValidationGridAdd(string popName)
        {
            //foreach (UIElement ui in grdMain.Children)
            //{
            //    if (((System.Windows.FrameworkElement)ui).Name.Equals(popName))
            //    {
            //        // 프로그램이 이미 실행 중 입니다. 
            //        Util.MessageValidation("SFU3193");
            //        return false;
            //    }
            //}

            return true;
        }

        private void popupSO_Closed(object sender, EventArgs e)
        {
            COM001_383_SHIPPING_ORDER popup = sender as COM001_383_SHIPPING_ORDER;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataView dv = (DataView)dgOrderList.ItemsSource;

                DataRow _curOLRow = null;
                //if (dv != null)
                //{
                //    DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
                //    if (oldt.Rows.Count > 0 && _keyOrderListIndex > -1)
                //    {
                //        _curOLRow = oldt.Rows[_keyOrderListIndex];
                //    }
                //}

                GetOrderList(_curOLRow, false , false);        // Order 추가후
            }
        }

        /*
        private void puHold_Closed(object sender, EventArgs e)
        {
            COM001_383_SHIPPING_ORDER popup = sender as COM001_383_SHIPPING_ORDER;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
                DataRow _curOLRow = oldt.Rows[_keyOrderListIndex];

                GetOrderList(_curOLRow , false);
            }
            this.grdMain.Children.Remove(popup);
        }
        */

        private void btnSearchRequestHistory_Click(object sender, RoutedEventArgs e)
        {

            InitializeTab2Controls();        // 조회버튼 누를때

            GetRequestHistoryOrderList();
        }
        private void GetRequestHistoryOrderList()
        {

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("REQ_DTTM_FROM", typeof(string));
            INDATA.Columns.Add("REQ_DTTM_TO", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
            INDATA.Columns.Add("PRJT_NAME", typeof(string));
            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LANGID"] = LoginInfo.LANGID;
            dr["REQ_DTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            dr["REQ_DTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");

            string _CELL_SPLY_STAT_CODE = cboReqStatus.SelectedValue.ToString();
            dr["CELL_SPLY_STAT_CODE"] = Util.NVC(_CELL_SPLY_STAT_CODE).Equals(string.Empty) ? null : _CELL_SPLY_STAT_CODE;
            string _CELL_SPLY_TYPE_CODE = cboReqType.SelectedValue.ToString();
            dr["CELL_SPLY_TYPE_CODE"] = Util.NVC(_CELL_SPLY_TYPE_CODE).Equals(string.Empty) ? null : _CELL_SPLY_TYPE_CODE;
            dr["PRJT_NAME"] = Util.GetCondition(txtProjectName, bAllNull: true);
            INDATA.Rows.Add(dr);

            try
            {
                ShowLoadingIndicator();   // GetRequestHistoryOrderList

                string sBizName = "BR_PRD_GET_CELL_PLLT_SHIP_REQ_ORDER_LIST";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, ex) =>
                {
                    HiddenLoadingIndicator();

                    if (ex != null )
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        Util.GridSetData(dgRequestHistoryOrderList, result, FrameOperation, true);
                        //*********************************************************************************************************************//
                        //
                        // [ 첫번째 자동 선택 기능해제 ]
                        //
                        // _keyRequestHistoryOderListIndex = 0;
                        // DataTableConverter.SetValue(dgRequestHistoryOrderList.Rows[_keyRequestHistoryOderListIndex].DataItem, "CHK", true);
                        // dgRequestHistoryOrderList.SelectedIndex = _keyRequestHistoryOderListIndex;

                        // ViewRequestHistoryGridProcess();
                        //*********************************************************************************************************************//
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        private void ViewRequestHistoryGridProcess()
        {
            DataTable dt = ((DataView)dgRequestHistoryOrderList.ItemsSource).Table;
            DataRow _curHistRow = dt.Rows[_keyRequestHistoryOderListIndex];
            if (_keyRequestHistoryOderListIndex > -1)
            {
                GetShipmentHistoryRequestPalletList(_curHistRow);
                GetScanedPalletList(_curHistRow);
            }
        }


        private void GetShipmentHistoryRequestPalletList(DataRow _curHistRow)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                DataRow dr = RQSTDT.NewRow();
                dr["CELL_SPLY_REQ_ID"] = _curHistRow["CELL_SPLY_REQ_ID"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_SHIP_HIST_REQ_PLLT_LIST";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgHistoryPalletList, result, FrameOperation, true);
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

        private void GetScanedPalletList(DataRow _curHistRow)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                DataRow dr = INDATA.NewRow();
                dr["CELL_SPLY_REQ_ID"] = _curHistRow["CELL_SPLY_REQ_ID"].ToString();
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_SHIP_HIST_SCAN_PLLT_LIST";
                DataTable result = new ClientProxy().ExecuteServiceSync(sBizName, "INDATA", "OUTDATA", INDATA);

                if (result.Rows.Count > 0)
                {
                    Util.GridSetData(dgScanedPalletList, result, FrameOperation, true);
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

        private void rbRequestHistoryOrderListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null) return;

            if ((bool)rb.IsChecked)
            {
                Util.gridClear(dgHistoryPalletList);
                Util.gridClear(dgScanedPalletList);

                _keyRequestHistoryOderListIndex = ((DataGridCellPresenter)rb.Parent).Row.Index;
                C1DataGrid dg = ((DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (_keyRequestHistoryOderListIndex == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                // 해당 row 선택
                dgRequestHistoryOrderList.SelectedIndex = _keyRequestHistoryOderListIndex;

                ViewRequestHistoryGridProcess();  //rbRequestHistoryOrderListChoice_Checked
            }
        }

        private void btnShippment_Click(object sender, RoutedEventArgs e)
        {

            if (!CheckSelectOrderList())  // btnShippment_Click
            {
                
                Util.MessageValidation("SFU1654"); //선택된 요청이 없습니다.
                return;
            }

            DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
            DataRow _curOLRow = oldt.Rows[_keyOrderListIndex];

            if (dgScanPalletList.ItemsSource == null)
            {
                
                Util.MessageValidation("SFU2936");  // 작업 수량이 없습니다.
                return;
            }

            DataTable spdt = ((DataView)dgScanPalletList.ItemsSource).Table;
            if (spdt.Rows.Count < 1)
            {
                
                Util.MessageValidation("SFU2936");  // 작업 수량이 없습니다.
                return;
            }

            // Cell Line Split 처리
            string _msgid = "SFU8566"; // 출하 진행 하시겠습니까?
            int _remainqty = 0;
            if (Convert.ToString(_curOLRow["CELL_SPLY_TYPE_CODE"]).Equals("CELL"))
            {
                if (Int32.Parse(Convert.ToString(_curOLRow["REQ_QTY"])) != spdt.Rows.Count)
                {
                    _msgid = "SFU9025"; // 출하 진행하시겠습니까? 나머지 [%1]수량은 출고요청 됩니다.
                    _remainqty = Int32.Parse(Convert.ToString(_curOLRow["REQ_QTY"])) - spdt.Rows.Count;
                }
            }

            Util.MessageConfirm(_msgid, result =>   
            {
                if (result == MessageBoxResult.OK)
                {
                    DataRow _curSPRow = spdt.Rows[0];

                    string _type = Util.NVC(_curOLRow["CELL_SPLY_TYPE_CODE"]);
                    _sRCV_ISS_ID = string.Empty;

                    if (!_type.Equals("CELL"))
                    {

                        /* ****************************************************************************
                            CELL 이 아닐경우는 스캔 팔레트 Validation을 해서 "INTRANSIT_SLOC_ID"를 획득한다.
                            BR_CHK_SHIP_PALLET_BX 먼저 호출해서 데이타 있으면   
                            Biz(BR_PRD_REG_CELL_PLLT_SHIP_RSPN_PLLT) 호출 
                            ****************************************************************************/

                        _sINTRANSIT_SLOC_ID = string.Empty;

                        if (LoginInfo.CFG_SYSTEM_TYPE_CODE.Equals("F"))
                            ShipmentBR_CHK_SHIP_PALLET_BX(_curOLRow, _curSPRow);             // 활성화동
                        else
                            ShipmentBR_PRD_GET_PALLET_INFO_FOR_SHIP(_curOLRow, _curSPRow);   // 조립동

                    }
                    else
                    {
                        ShipmentCellProcess(_curOLRow);
                    }
                }
            }, _remainqty);            
        }

        // Shipment 조립동
        private string ShipmentADong( DataRow _curOLRow)
        {

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            DataTable INPALLET = inDataSet.Tables.Add("INPALLET");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("FROM_AREAID", typeof(string));
            INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
            INDATA.Columns.Add("TO_AREAID", typeof(string));
            INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
            INDATA.Columns.Add("ISS_QTY", typeof(string));
            INDATA.Columns.Add("ISS_NOTE", typeof(string));
            INDATA.Columns.Add("SHIPTO_ID", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            // INDATA.Columns.Add("OWMS_PROD_TYPE", typeof(string));
            INDATA.Columns.Add("SHIP_WAIT_YN", typeof(string));
            INDATA.Columns.Add("AUTO_WH_SHIP_FLAG", typeof(string));
            INDATA.Columns.Add("RFID_USE_FLAG", typeof(string));
            INDATA.Columns.Add("INTRANSIT_SLOC_ID", typeof(string));

            INPALLET.TableName = "INPALLET";
            INPALLET.Columns.Add("BOXID", typeof(string));
            INPALLET.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

            DataTable spdt = ((DataView)dgScanPalletList.ItemsSource).Table;
            DataRow _spcurRow = spdt.Rows[0];

            DataRow dr = INDATA.NewRow();
            dr["FROM_AREAID"] = Util.NVC(_sFROM_AREAID).Equals(string.Empty) ? null : _sFROM_AREAID;
            dr["FROM_SLOC_ID"] = Util.NVC(_sFROM_LOC).Equals(string.Empty) ? null : _sFROM_LOC;
            dr["TO_AREAID"] = Util.NVC(_sTO_AREAID).Equals(string.Empty) ? null : _sTO_AREAID;
            dr["TO_SLOC_ID"] = Util.NVC(_sTO_LOC).Equals(string.Empty) ? null : _sTO_LOC;
            string _sISS_NOTE = string.Empty;
            dr["ISS_NOTE"] = Util.NVC(_sISS_NOTE).Equals(string.Empty) ? null : _sISS_NOTE;
            dr["SHIPTO_ID"] = Util.NVC(_sTO_SHOPID).Equals(string.Empty) ? null : _sTO_SHOPID;
            string _sNOTE = string.Empty;
            dr["NOTE"] = Util.NVC(_sNOTE).Equals(string.Empty) ? null : _sNOTE;
            dr["USERID"] = LoginInfo.USERID;
            dr["AUTO_WH_SHIP_FLAG"] = "N";
            dr["RFID_USE_FLAG"] = "N";
            dr["INTRANSIT_SLOC_ID"] = Util.NVC(_sINTRANSIT_SLOC_ID).Equals(string.Empty) ? null : _sINTRANSIT_SLOC_ID;

            double _sScanPalletCellQtySummary = 0;
            foreach (DataRow row in spdt.Rows)
            {
                DataRow prbox = INPALLET.NewRow();
                prbox["BOXID"] = row["BOXID"].ToString();
                prbox["OWMS_BOX_TYPE_CODE"] = "AC";
                INPALLET.Rows.Add(prbox);

                double _sDoubleQty = 0;
                double.TryParse(row["QTY"].ToString(), out _sDoubleQty);
                _sScanPalletCellQtySummary = _sScanPalletCellQtySummary + _sDoubleQty;
            }
            dr["ISS_QTY"] = Convert.ToString(_sScanPalletCellQtySummary);

            INDATA.Rows.Add(dr);

            try
            {
                string sBizName = "BR_PRD_REG_SHIP_CELL";
                DataSet result = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA, INPALLET", null, inDataSet);
                if (result.Tables["OUTDATA"].Rows.Count > 0)
                {
                    _sRCV_ISS_ID = result.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"].ToString();
                }

                // SHIPMENT 버튼을 눌럿을 경우 SHIPMENT 처리인증.
                string _msg = ConfirmShipmentProcess(_curOLRow);

                if (string.IsNullOrEmpty(_msg))
                {
                    // 정상처리 되었습니다.
                    Util.MessageInfo("SFU1275");
                    InitializeSubControls();
                }
                else
                {
                    return _msg;      // 메세지가 있을경우
                }

                InitializeControls();                     // Shipment A동 이후

                _keyOrderListIndex = -1;
                _keyCELL_SPLY_REQ_ID = string.Empty;
                _keyCELL_SPLY_RSPN_ID = string.Empty;

                GetOrderList(_curOLRow , true , false);   // Shipment A동 이후

                return null;

            }
            catch (Exception ex)
            {
                return Util.ExceptionMessageToString(ex);   // Shipment A동 이후
            }

        }
        // Shipment 조립동
        private string ShipmentFDong( DataRow _curOLRow)
        {


            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            DataTable INPALLET = inDataSet.Tables.Add("INPALLET");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("FROM_AREAID", typeof(string));
            INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
            INDATA.Columns.Add("TO_AREAID", typeof(string));
            INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
            INDATA.Columns.Add("ISS_QTY", typeof(string));
            INDATA.Columns.Add("ISS_NOTE", typeof(string));
            INDATA.Columns.Add("SHIPTO_ID", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            // INDATA.Columns.Add("OWMS_PROD_TYPE", typeof(string));
            INDATA.Columns.Add("SHIP_WAIT_YN", typeof(string));
            INDATA.Columns.Add("AUTO_WH_ISS_FLAG", typeof(string));
            INDATA.Columns.Add("INTRANSIT_SLOC_ID", typeof(string));

            INPALLET.TableName = "INPALLET";
            INPALLET.Columns.Add("BOXID", typeof(string));
            INPALLET.Columns.Add("OWMS_BOX_TYPE_CODE", typeof(string));

            DataTable spdt = ((DataView)dgScanPalletList.ItemsSource).Table;
            DataRow _spcurRow = spdt.Rows[0];

            DataRow dr = INDATA.NewRow();
            dr["SRCTYPE"] = "UI";
            dr["FROM_AREAID"] = Util.NVC(_sFROM_AREAID).Equals(string.Empty) ? null : _sFROM_AREAID;
            dr["FROM_SLOC_ID"] = Util.NVC(_sFROM_LOC).Equals(string.Empty) ? null : _sFROM_LOC;
            dr["TO_AREAID"] = Util.NVC(_sTO_AREAID).Equals(string.Empty) ? null : _sTO_AREAID;
            dr["TO_SLOC_ID"] = Util.NVC(_sTO_LOC).Equals(string.Empty) ? null : _sTO_LOC;
            string _sISS_NOTE = string.Empty;
            dr["ISS_NOTE"] = Util.NVC(_sISS_NOTE).Equals(string.Empty) ? null : _sISS_NOTE;
            dr["SHIPTO_ID"] = Util.NVC(_sTO_SHOPID).Equals(string.Empty) ? null : _sTO_SHOPID;
            string _sNOTE = string.Empty;
            dr["NOTE"] = Util.NVC(_sNOTE).Equals(string.Empty) ? null : _sNOTE;
            dr["USERID"] = LoginInfo.USERID;
            dr["SHIP_WAIT_YN"] = Util.NVC(_sSHIP_WAIT_YN).Equals(string.Empty) ? null : _sSHIP_WAIT_YN;
            dr["AUTO_WH_ISS_FLAG"] = "N";
            dr["INTRANSIT_SLOC_ID"] = Util.NVC(_sINTRANSIT_SLOC_ID).Equals(string.Empty) ? null : _sINTRANSIT_SLOC_ID;

            double _sScanPalletCellQtySummary = 0;
            foreach (DataRow row in spdt.Rows)
            {
                DataRow prbox = INPALLET.NewRow();
                prbox["BOXID"] = row["BOXID"].ToString();
                prbox["OWMS_BOX_TYPE_CODE"] = "AC";
                INPALLET.Rows.Add(prbox);
                double _sDoubleQty = 0;
                double.TryParse(row["QTY"].ToString(),out _sDoubleQty);
                _sScanPalletCellQtySummary = _sScanPalletCellQtySummary + _sDoubleQty;
            }
            dr["ISS_QTY"] = Convert.ToString(_sScanPalletCellQtySummary);
            INDATA.Rows.Add(dr);

            try
            {
                string sBizName = "BR_SET_SHIP_CELL";
                DataSet result = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA,INPALLET", "OUTDATA", inDataSet);
                if (result.Tables["OUTDATA"].Rows.Count > 0)
                {
                    _sRCV_ISS_ID = Convert.ToString(result.Tables["OUTDATA"].Rows[0]["RCV_ISS_ID"]);
                }

                // SHIPMENT 버튼을 눌럿을 경우 SHIPMENT 처리인증.
                string _msg = ConfirmShipmentProcess(_curOLRow);  // Shipment F Dong

                if (string.IsNullOrEmpty(_msg))
                {
                    // 정상처리 되었습니다.
                    Util.MessageInfo("SFU1275");
                    InitializeSubControls();

                }
                else
                {
                    return _msg;  // 메세지가 있을경우
                }

                InitializeControls();                   // Shipment F동 이후

                _keyOrderListIndex = -1;

                _keyCELL_SPLY_REQ_ID = string.Empty;
                _keyCELL_SPLY_RSPN_ID = string.Empty;

                GetOrderList(_curOLRow , true, false);   // Shipment F동 이후

                return null;
            }
            catch (Exception ex)
            {
                return Util.ExceptionMessageToString(ex);   // Shipment F동 이후
            }

        }

        // Shipment Cell
        private string ShipmentCellProcess(DataRow _curOLRow)
        {


            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("BOXID", typeof(string));
            INDATA.Columns.Add("CSTID", typeof(string));
            INDATA.Columns.Add("WH_ID", typeof(string));
            INDATA.Columns.Add("RACK_ID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("SRCTYPE", typeof(string));
            INDATA.Columns.Add("ACTID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataTable oldt = ((DataView)dgOrderList.ItemsSource).Table;
            DataRow _olcurRow = oldt.Rows[_keyOrderListIndex];

            DataTable spdt = ((DataView)dgScanPalletList.ItemsSource).Table;
            DataRow _spcurRow = spdt.Rows[0];

            foreach (DataRow row in spdt.Rows)
            {
                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = row["BOXID"].ToString();
                dr["CSTID"] = row["PLLT_BCD_ID"].ToString();
                dr["USERID"] = LoginInfo.USERID;
                dr["SRCTYPE"] = "UI";
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);
            }

            try
            {

                ShowLoadingIndicator();   // ShipmentCellProcess

                string sBizName = "BR_PRD_REG_CELL_PLLT_LOCATION_TO_CELL";
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", null, (result, ex) =>
                {

                    // SHIPMENT 버튼을 눌럿을 경우 SHIPMENT 처리인증.
                    string _msg = ConfirmShipmentProcess(_curOLRow);

                    if (string.IsNullOrEmpty(_msg))
                    {
                        // 정상처리 되었습니다.
                        Util.MessageInfo("SFU1275");
                        InitializeSubControls();
                    }
                    else
                    {
                        // 메세지가 있을경우
                        Util.MessageInfo(_msg);
                        return;
                    }

                    InitializeControls();                     // Shipment Cell 이후

                    _keyCELL_SPLY_REQ_ID = string.Empty;
                    _keyCELL_SPLY_RSPN_ID = string.Empty;

                    GetOrderList(_curOLRow , true , false);   // Shipment Cell 이후

                }, inDataSet);

                return null;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return Util.ExceptionMessageToString(ex);    // Shipment Cell 이후
            }

        }


        private string ConfirmShipmentProcess(DataRow _curOLRow)
        {
            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["CELL_SPLY_REQ_ID"] = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
            dr["RCV_ISS_ID"] = _sRCV_ISS_ID;
            dr["USERID"] = LoginInfo.USERID;

            INDATA.Rows.Add(dr);

            try
            {
                string _bizName = "BR_PRD_UPD_CELL_PLLT_SHIP_RSPN_COMPLETE";
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(_bizName, "INDATA", null, INDATA);

                return null;
            }
            catch (Exception ex)
            {
                return Util.ExceptionMessageToString(ex);   // Confirm Shipment Process
            }    
            finally
            { }

        }


        private bool ChkToSloc()
        {
            DataTable inTable = new DataTable();
            inTable.Columns.Add("SRCTYPE", typeof(string));
            inTable.Columns.Add("SLOCID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["SLOCID"] = _sTO_LOC;
            dr["AREAID"] = _sFROM_AREAID;

            inTable.Rows.Add(dr);

            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_SLOC_CHECK_BX", "RQSTDT", "RSLTDT", inTable);

            if (dtRslt != null && dtRslt.Rows.Count > 0)
            {
                // 미판정 창고 이면
                if (dtRslt.Rows[0]["CHECK_TYPE"].ToString() == "NO_INSP_SLOC")
                {
                    return true;
                }
                // 양품 창고 이면
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private void dgRequestPalletList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                };

                // 해당 Row 값 가져오기
                //string sCheck = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CHECK_DELETE"));

                // 해당 Row 색 변경
                if (e.Cell.Row.Index == 0)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.GreenYellow);
                }
                if (e.Cell.Row.Index == 1)
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                }

            }));
        }

        private void dgRequestPalletList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;

                String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
                if (dg.CurrentRow != null)
                {
                    DataRowView drvRow = dg.CurrentRow.DataItem as DataRowView;
                    C1.WPF.DataGrid.DataGridCell cell = dg.GetCellFromPoint(e.GetPosition(null));
                    ShowPopupPalletInfo(drvRow);
                }
            }
            catch (Exception ex)
            { }
            finally
            { }
        }

        private bool CheckSelectOrderList()
        {
            C1DataGrid dg = dgOrderList;
            DataView dv = (DataView)dg.ItemsSource;
            if (dv == null)  return false;

            DataTable oldt = ((DataView)dg.ItemsSource).Table;
            _keyOrderListIndex = -1;
            for (int i = 0; i < oldt.Rows.Count; i++)
            {
                if (Convert.ToBoolean(DataTableConverter.GetValue(dg.Rows[i].DataItem, "CHK")) == true)
                {
                    _keyOrderListIndex = i;
                    return true;
                }
            }
            return false;
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {

            C1DataGrid dg = dgOrderList;
            DataView dvOrd = (DataView)dg.ItemsSource;
            if (dvOrd == null)
            {
                
                Util.MessageValidation("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            DataTable oldt = ((DataView)dg.ItemsSource).Table;

            if (!CheckSelectOrderList())   // btnAccept_Click
            {
                
                Util.MessageValidation("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            DataRow _curOLRow = oldt.Rows[_keyOrderListIndex];

            string _sStsCd = Convert.ToString(_curOLRow["CELL_SPLY_STAT_CODE"]);
            if (!_sStsCd.Equals("REQUEST"))   // REQUEST 상태가 아니면 Accept 하지 못함
            {
                
                Util.MessageValidation("SFU6015");  // 공급요청 상태에서만 선택 가능합니다.
                return;
            }

            Util.MessageConfirm("SFU2878", result =>   // 승인하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    AcceptProcess(_curOLRow);
                }
            });
        }

        private void AcceptProcess(DataRow _curOLRow)
        {
            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["CELL_SPLY_REQ_ID"] = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
            dr["USERID"] = LoginInfo.USERID;

            INDATA.Rows.Add(dr);

            // 선택 키보관
            _keyCELL_SPLY_REQ_ID = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
            _keyCELL_SPLY_RSPN_ID = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();

            ShowLoadingIndicator();   // Accept 버튼
            string sBizName = "BR_PRD_UPD_CELL_PLLT_SHIP_RSPN_ACCEPT";
            try
            {

                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", null, (result, bizex) =>
                {
                    try
                    {
                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        InitializeControls();  // 버튼 Accept 이후
                        GetOrderList(_curOLRow, false , false);  // 버튼 Accept 이후

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }

                }, inDataSet);

                return;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            C1DataGrid dg = dgOrderList;
            DataView dvOrd = (DataView)dg.ItemsSource;
            if (dvOrd == null)
            {
                
                Util.MessageValidation("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            DataTable oldt = ((DataView)dg.ItemsSource).Table;
            if (!CheckSelectOrderList())    // btnDeleteRequest_Click
            {
                
                Util.MessageValidation("SFU1654");  //선택된 요청이 없습니다.
                return;
            }

            DataRow _curOLRow = oldt.Rows[_keyOrderListIndex];

            Util.MessageConfirm("SFU1259", result =>   // 삭제처리 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    DeleteRequestProcess(_curOLRow);
                }
            });
        }


        private void DeleteRequestProcess(DataRow _curOLRow)
        {
            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_RSPN_ID", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["CELL_SPLY_REQ_ID"] = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
            dr["CELL_SPLY_RSPN_ID"] = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();
            dr["USERID"] = LoginInfo.USERID;
            dr["LANGID"] = LoginInfo.LANGID;

            INDATA.Rows.Add(dr);

            // 선택 키보관
            _keyCELL_SPLY_REQ_ID = _curOLRow["CELL_SPLY_REQ_ID"].ToString();
            _keyCELL_SPLY_RSPN_ID = _curOLRow["CELL_SPLY_RSPN_ID"].ToString();

            string sBizName = "BR_PRD_REG_CELL_PLLT_SHIP_REQ_DELETE";
            new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", null, (result, bizex) =>
            {
                try
                {
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }

                    InitializeControls();                   // 버튼 DeleteRequestProcess 이후
                    GetOrderList(_curOLRow, false , true);         // 버튼 DeleteRequestProcess 이후

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }, inDataSet);
        }


        /// <summary>
        /// 타이머 실행
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (sender == null)
                return;

            DispatcherTimer dpcTmr = sender as DispatcherTimer;
            dpcTmr?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    dpcTmr.Stop();
                    if (Math.Abs(dpcTmr.Interval.TotalSeconds) < 1) return;

                    _findRequestStatus();
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

        private void _findRequestStatus()
        {
            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
            INDATA.Columns.Add("AUTO_LOGIS_FLAG", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["CELL_SPLY_STAT_CODE"] = "REQUEST";
            dr["CELL_SPLY_TYPE_CODE"] = "MES";
            dr["AUTO_LOGIS_FLAG"] = "N";
            INDATA.Rows.Add(dr);

            new ClientProxy().ExecuteService("BR_PRD_SEL_SPLY_REQUEST_CNT", "INDATA", "OUTDATA", INDATA, (searchResult, searchException) =>
            {
                try
                {
                    HideRequestInfoMode();

                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }
                    if (searchResult.Rows.Count > 0 && searchResult != null)
                    {
                        if (searchResult.Columns.Contains("STATUS_CNT") && Util.NVC_Int(searchResult.Rows[0]["STATUS_CNT"]) > 0)
                        {
                            this.RegisterName("redBrush", redBrush);
                            ShowRequestInfoMode();
                        }
                    }
                }
                catch (Exception ex)
                {
                    HideRequestInfoMode();
                }
            });
        }
        private void btnDisplay_Click(object sender, RoutedEventArgs e)
        {
            this.RegisterName("redBrush", redBrush);
            ShowRequestInfoMode();
        }

        private void ShowRequestInfoMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdRequestMode.Visibility = Visibility.Visible;
                ColorAnimationInredRectangle(recRequestInfoMode);
            }));
        }

        private void HideRequestInfoMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                grdRequestMode.Visibility = Visibility.Collapsed;
            }));
        }

        private void ColorAnimationInredRectangle(System.Windows.Shapes.Rectangle rect)
        {
            rect.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
            opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));
            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);
        }

        private void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            GetRequestPalletList(_sPRODID);
        }

        // 팔레트바코드ID -> BoxID
        private string getPalletBCD(string palletid)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("CSTID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["CSTID"] = palletid;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CARRIER_BY_PLLT_BCD_ID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult != null && SearchResult.Rows.Count > 0)
                {
                    return Util.NVC(SearchResult.Rows[0]["CURR_LOTID"]);
                }
                return palletid;
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                return "";
            }
        }
    }
}