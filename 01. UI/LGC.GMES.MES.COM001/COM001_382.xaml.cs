/*************************************************************************************
 Created Date : 2023.05.29
      Creator : 
   Decription : 포장 Pallet 정보조회
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.29  주재홍 : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_382 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 


        List<string> _MColumns1;
        List<string> _MColumns2;

        private BizDataSet _Biz = new BizDataSet();

        DataTable dtAdd = new DataTable();  // GRID DataTable
        Util _Util = new Util();

        private string _BOXID = string.Empty;

        public COM001_382()
        {
            InitializeComponent();
            Loaded += COM001_382_Loaded;
        }

        private void COM001_382_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= COM001_382_Loaded;

            InitializeControls();
            InitializeVariables();

            this.txtPalletBcdId.Focus();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        private void InitializeVariables()
        {
            this.txtPalletBcdId.Text = "";
            this.txtPalletBcdId.Focus();

            //this.txtPalletBcdId.CaptureMouse();
        }

        #endregion

        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns2 = new List<string>();
        }

        private void InitializeControls()
        {
            Util.gridClear(dgInfo); // Grid Initialize
            Util.gridClear(dgPalletHistory);

            dtAdd.Clear();          // datatable initialize
            dtAdd.Columns.Add("GITEM1", typeof(string));
            dtAdd.Columns.Add("GITEM2", typeof(string));
            dtAdd.Columns.Add("GDATA", typeof(string));
            dtAdd.AcceptChanges();

        }

        #region [Form Load]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }
        #endregion

        #region [조회]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletInfo();
        }
        #endregion

        private void btnPalletLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_382_LINK_PALLET _popup = new COM001_382_LINK_PALLET();
                _popup.FrameOperation = FrameOperation;

                if (_popup != null)
                {
                    string _ValueTobox = string.Empty;
                    string _ValueToPallet = string.Empty;

                    if (dgInfo.GetRowCount() > 0)
                    {
                        DataTable dt = (dgInfo.ItemsSource as DataView).Table;
                        _ValueTobox = Util.NVC(dt.Rows[0][2]);
                        _ValueToPallet = Util.NVC(dt.Rows[1][2]);
                    }
                    object[] Parameters = new object[3];
                    Parameters[0] = _ValueTobox;
                    Parameters[1] = _ValueToPallet;

                    C1WindowExtension.SetParameters(_popup, Parameters);

                    _popup.Closed += new EventHandler(_popup_Closed);
                    _popup.ShowModal();
                    _popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void _popup_Closed(object sender, EventArgs e)
        {
            COM001_382_LINK_PALLET runStartWindow = sender as COM001_382_LINK_PALLET;
            if (runStartWindow.DialogResult == MessageBoxResult.Cancel)
            {
                txtPalletBcdId.Text = string.Equals(runStartWindow.PALLETBCD,"") ? runStartWindow.BOXID : runStartWindow.PALLETBCD;
                GetPalletInfo();
            }
        }


        #region [LOT] - 조회 조건
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetPalletInfo();
            }
        }
        #endregion

        #region [Func]
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
        #endregion

        #region [Get Pallet Data]
        private void GetPalletInfo()
        {
            string bcd = txtPalletBcdId.Text;
            string rfid = txtRfidId.Text;

            Util.gridClear(dgInfo);
            Util.gridClear(dgPalletHistory);

            txtPalletBcdId.Text = String.Empty;
            txtRfidId.Text = String.Empty;

            if ( string.IsNullOrEmpty(bcd) && string.IsNullOrEmpty(rfid) )
            {
                Util.MessageValidation("SFU8562");
                return;
            }
            else
            {
                if ( !string.IsNullOrEmpty(bcd) )
                {
                    GetPalletBizCall(bcd);
                }
                else
                {
                    GetPalletBizCall(rfid);
                }
               
               // GetPalletBizCall(_BOXID);
            }

        }

        private void GetPalletBizCall(string cstId)
        {
            try
            {

                dtAdd.Clear();

                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CSTID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CSTID"] = cstId;              // txtPalletBcdId.Text;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CELL_PLLT_LOCATION_INFORMATION_UI";

                ShowLoadingIndicator();
                new ClientProxy().ExecuteService_Multi(sBizName, "INDATA", "OUTDATA_INFO,OUTDATA_HIST", (result, ex) =>
                {

                    HiddenLoadingIndicator();

                    if (ex != null) { 

                        Util.MessageException(ex);
                        return;
                    }

                    if (result.Tables["OUTDATA_INFO"].Rows.Count == 0)
                    {
                        InitializeVariables();
                        dtAdd.Clear();
                        Util.gridClear(dgInfo);
                        Util.gridClear(dgPalletHistory);
                        Util.AlertInfo("SFU1180");            //BOX 정보가 없습니다.
                        return;
                    }

                    DataTable dsInfo = result.Tables["OUTDATA_INFO"];
                    DataTable dsHist = result.Tables["OUTDATA_HIST"];

                    DataRow _infoRow = dsInfo.Rows[0];

                    DataRow newRow = null;
                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("Pallet ID"); 
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("Pallet ID");
                    newRow["GDATA"] = _infoRow["PALLET_ID"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("PALLET BCD");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("PALLET BCD");
                    newRow["GDATA"] = _infoRow["BCD_ID"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("Type");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("Type");
                    newRow["GDATA"] = _infoRow["PLLT_TYPE"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("QTY");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("QTY");
                    newRow["GDATA"] = _infoRow["QTY"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("제품 코드");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("제품 코드");
                    newRow["GDATA"] = _infoRow["PJT"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("WH ID");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("WH ID");
                    newRow["GDATA"] = _infoRow["SECTION"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("LOCATION_ID");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("LOCATION_ID");
                    newRow["GDATA"] = _infoRow["LOCATION"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("LINE");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("LINE");
                    newRow["GDATA"] = _infoRow["LINE"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("설비");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("설비");
                    newRow["GDATA"] = _infoRow["EQPT"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("PALLET상태");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("PALLET상태");
                    newRow["GDATA"] = _infoRow["STATUS"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("작업일시");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("작업일시");
                    newRow["GDATA"] = _infoRow["PACKDTTM"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("RFID");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("RFID");
                    newRow["GDATA"] = _infoRow["RFID"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("HOLD");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("MES");
                    newRow["GDATA"] = _infoRow["HOLD_MES"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("HOLD");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("QMS");
                    newRow["GDATA"] = _infoRow["HOLD_QMS"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("HOLD");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("CELL");
                    newRow["GDATA"] = _infoRow["HOLD_CELL"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("HOLD");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("PALLET");
                    newRow["GDATA"] = _infoRow["HOLD_PALLET"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("QA검사");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("성능검사"); ;
                    newRow["GDATA"] = _infoRow["QA_PER"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("QA검사");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("치수검사"); ;
                    newRow["GDATA"] = _infoRow["QA_DIM"].ToString();
                    dtAdd.Rows.Add(newRow);

                    newRow = dtAdd.NewRow();
                    newRow["GITEM1"] = ObjectDic.Instance.GetObjectName("QA검사");
                    newRow["GITEM2"] = ObjectDic.Instance.GetObjectName("LOW_VOLT"); ;
                    newRow["GDATA"] = _infoRow["QA_FAIL"].ToString();
                    dtAdd.Rows.Add(newRow);

                    dgInfo.ItemsSource = DataTableConverter.Convert(dtAdd);

                    string[] sCol = new string[] { "GITEM1", "GITEM2" };

                    _Util.SetDataGridMergeExtensionCol(dgInfo, sCol, DataGridMergeMode.VERTICALHIERARCHI);

                    dgPalletHistory.ItemsSource = DataTableConverter.Convert(dsHist);

                }, inDataSet);

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();

                if (ex != null)
                {
                    Util.MessageException(ex);
                    Util.gridClear(dgInfo);
                    dtAdd.Clear();            // datatable initialize
                    Util.gridClear(dgPalletHistory);
                    return;
                }
            }
            finally
            {

            }
        }

        private void GetBoxid()
        {

            try
            {

                string _inputText = txtPalletBcdId.Text;

                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["BOXID"] = _inputText;
                dr["RACK_ID"] = _inputText;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_CSTID_BOXID_INF";

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTDATA", inDataSet);

                if (ds.Tables["OUTDATA"].Rows.Count == 0)
                {
                    _BOXID = string.Empty;

                    Util.AlertInfo("SFU1180");  //BOX 정보가 없습니다.
                    return;
                }

                DataRow row = ds.Tables["OUTDATA"].Rows[0];
                txtPalletBcdId.Text = row["BOXID"].ToString();
                _BOXID = row["BOXID"].ToString();

                GetPalletBizCall(_BOXID);

            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    _BOXID = string.Empty;

                    Util.MessageException(ex);
                    InitializeVariables();
                    return;
                }
            }
            finally
            {
            }
        }

        private void GetRfidBizCall()
        {

            try
            {

                DataSet inDataSet = new DataSet();

                //마스터 정보
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("CSTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["CSTID"] = txtRfidId.Text;
                INDATA.Rows.Add(dr);

                string sBizName = "BR_PRD_GET_BOXID_BY_CSTID_RFID";

                DataSet ds = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTDATA", inDataSet);

                if (ds.Tables["OUTDATA"].Rows.Count == 0)
                {
                    _BOXID = string.Empty;

                    Util.AlertInfo("SFU1180");  //BOX 정보가 없습니다.
                    return;
                }

                DataTable dsRfid = ds.Tables["OUTDATA"];
                DataRow row = dsRfid.Rows[0];
                txtPalletBcdId.Text = row["CSTID"].ToString();
                _BOXID = txtPalletBcdId.Text;

                GetPalletBizCall(_BOXID);

            }
            catch (Exception ex)
            {
                if (ex != null)
                {
                    _BOXID = string.Empty;

                    Util.MessageException(ex);
                    InitializeVariables();
                    return;
                }
            }
            finally
            {
            }
        }


        #endregion

        private void txtPalletBcdId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) GetPalletInfo();
        }

        private void txtRfidId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) GetPalletInfo();
        }

    }
}