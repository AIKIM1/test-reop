/*************************************************************************************
 Created Date : 2024.07.10
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2024.07.10  DEVELOPER : Initial Created.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using System.Windows.Threading;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_048 : UserControl, IWorkArea
    {
        #region #1.Variable
        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        DataTable dtCombo = null;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private bool bMaxCountChk = false;
        #endregion

        #region #2.Constructor
        public PACK001_048()
        {
            InitializeComponent();
            Initialize();
        }
        #endregion

        #region #3.Event
        /// <summary>
        /// name         : UserControl_Loaded
        /// desc         : 화면로드
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// name         : txtSendLotID_PreviewKeyDown
        /// desc         : multi enter
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSendLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {
                    string[] stringSeparators = new string[] { "^" };
                    string clipboardText = Clipboard.GetText().Replace("\r\n", "^").Replace("\n", "^").Replace("\"", "");

                    string[] arrClipboardText = clipboardText.Split(stringSeparators, StringSplitOptions.None);

                    var distinctWords = (from w in arrClipboardText where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                    if (null == distinctWords || distinctWords.Count() == 0)
                    {
                        this.txtSendLotID.Clear();
                        this.txtSendLotID.Focus();
                        return;
                    }

                    this.txtSendLotID.Text = distinctWords.Aggregate((current, next) => current + "," + next).ToString();

                    fnSendLotIdInput();
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
        /// name         : txtSearchLotId_PreviewKeyDown
        /// desc         : multi enter
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSearchLotId_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                try
                {                    
                    string[] stringSeparators = new string[] { "^" };
                    string clipboardText = Clipboard.GetText().Replace("\r\n", "^").Replace("\n", "^").Replace("\"", "");

                    string[] arrClipboardText = clipboardText.Split(stringSeparators, StringSplitOptions.None);

                    var distinctWords = (from w in arrClipboardText where !string.IsNullOrEmpty(w) select w).Distinct().ToList();                  

                    this.txtSearchLotId.Text = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                    this.txtSearchLotId_KeyDown(this, null);
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
        /// name         : txtSendLotID_KeyDown
        /// desc         : multi enter
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSendLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (null != e && e.Key == Key.Enter)
            {
                fnSendLotIdInput();
            }
        }

        /// <summary>
        /// name         : btnSendSearch_Click
        /// desc         : B2BI LOT 단위 재전송 데이터 조회
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSendSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSendLotID.Text))
            {
                // LOTID를 입력하세요
                Util.MessageInfo("SFU2839");
                return;
            }

            try
            {
                this.loadingIndicator.Visibility = Visibility.Visible;

                #region 중복LOT Check
                string sScan = this.txtSendLotID.Text.Trim();
                string sLotDs = sScan;
                if (sScan.Contains(","))
                {
                    string[] splt = sScan.Split(',');
                    var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                    int maxLOTIDCount = 299;
                    if (distinctWords.Count() > maxLOTIDCount)
                    {
                        bMaxCountChk = true;
                        this.txtSendLotID.Clear();
                        this.txtSendLotID.Focus();

                        distinctWords.RemoveRange(299, distinctWords.Count - 299);
                    }

                    sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                }
                #endregion

                SendSearchRowDataHeader(sLotDs);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtSendLotID.Clear();
                this.txtSendLotID.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// name         : CreatePopupWindow_Closed
        /// desc         : 기준에 적합하지않는 LOT POPUP
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void CreatePopupWindow_Closed(object sender, EventArgs e)
        {
            PACK001_048_POPUP window = sender as PACK001_048_POPUP;
            this.grdMain.Children.Remove(window);
        }

        /// <summary>
        /// name         : CreatePopupWindow_Closed2
        /// desc         : 포르쉐 전송유형의 경우 아우디 데이터 포함여부를 체크하여 재전송 데이터 생성
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void CreatePopupWindow_Closed2(object sender, EventArgs e)
        {
            PACK001_048_YESNOCANCEL window = sender as PACK001_048_YESNOCANCEL;
            this.grdMain.Children.Remove(window);

            int iRowCount = dgSendRowdata.GetRowCount();

            MessageBoxResult mresult = window.DialogResult;

            if (mresult == MessageBoxResult.Cancel) return;

            DataSet dsInput = new DataSet();

            DataTable INDATA = new DataTable();
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LOTID", typeof(string));
            INDATA.Columns.Add("NOTE", typeof(string));
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("AUDI_CONTAIN_YN", typeof(string));

            var distinctWords = (from row in dgSendRowdata.GetDataTable(true).AsEnumerable()
                                 select new
                                 {
                                     LOTID = row.Field<string>("LOTID")
                                 }).ToList();

            string Lots = string.Join(",", distinctWords.Select(dw => dw.LOTID));

            DataRow dr = INDATA.NewRow();
            dr["LOTID"] = Lots;
            dr["NOTE"] = txtRemakr.Text.Trim();
            dr["USERID"] = LoginInfo.USERID;
            dr["AUDI_CONTAIN_YN"] = mresult == MessageBoxResult.Yes ? "N" : "Y";
            INDATA.Rows.Add(dr);
            dsInput.Tables.Add(INDATA);

            if (INDATA.Rows.Count == 0) return;

            RemakeRowdataBiz(dsInput);
        }

        /// <summary>
        /// name         : btnReMake_Click
        /// desc         : B2BI LOT단위 재전송
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnReMake_Click(object sender, RoutedEventArgs e)
        {
            if (Util.pageAuthCheck(FrameOperation.AUTHORITY))
            {
                RemakeRowdataProcess();
            }
            else
            {
                //해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                Util.MessageInfo("SFU3520", new object[] { LoginInfo.USERID, "ReMake" });
            }
        }

        /// <summary>
        /// name         : btnReMake_Click
        /// desc         : 조회결과 초기화
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSendClear_Click(object sender, RoutedEventArgs e)
        {
            ClearSendData();
        }

        /// <summary>
        /// name         : cboLine_SelectedItemChanged
        /// desc         : 라인 콤보박스 이벤트
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void cboLine_SelectedItemChanged(object sender, C1.WPF.PropertyChangedEventArgs<object> e)
        {
            B2Bi_Line2ProdID_Combo();
        }

        /// <summary>
        /// name         : txtPalletId_KeyDown
        /// desc         : 박스ID 입력 이벤트
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtPalletId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SearchRowDataHeader();
                txtPalletId.SelectAll();
            }
        }

        /// <summary>
        /// name         : txtSearchLotId_KeyDown
        /// desc         : LOTID 입력 이벤트
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSearchLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (null == e || e.Key == Key.Enter)
            {
                SearchRowDataHeader();
                txtSearchLotId.SelectAll();
            }
        }

        /// <summary>
        /// name         : btnSearch_Click
        /// desc         : B2BI 전송이력 조회
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRowDataHeader();
        }

        /// <summary>
        /// name         : btnSaveExcel_Click
        /// desc         : B2BI 전송 이력 엑셀 저장 
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void btnSaveExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export_MergeHeader(dgRowdata);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// name         : dgCellList_MouseDoubleClick
        /// desc         : B2BI 상세정보 조회
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgCellList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.C1DataGrid c1Gd = (C1.WPF.DataGrid.C1DataGrid)sender;
                C1.WPF.DataGrid.DataGridCell crrCell = c1Gd.GetCellFromPoint(pnt);

                if (crrCell.Row.Index == 0) return;

                if (crrCell != null)
                {
                    if (c1Gd.GetRowCount() > 0 && crrCell.Row.Index >= 0)
                    {
                        string sBoxId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "BOXID"));
                        string sRcvIssId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "RCV_ISS_ID"));
                        string sPgmId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "CUST_TRNF_PGM_ID"));
                        string sIfId = Util.NVC(DataTableConverter.GetValue(c1Gd.Rows[crrCell.Row.Index].DataItem, "IF_ID"));

                        SearchRowDataDetail(sPgmId, sIfId, sRcvIssId, sBoxId);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// name         : dgSearchRowdataHd_LoadedCellPresenter
        /// desc         : B2BI 상세정보 Cell 색상 변경
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgSearchRowdataHd_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (dgSearchRowdataHd == null) return;

                C1DataGrid dataGrid = (sender as C1DataGrid);

                dgSearchRowdataHd.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    switch (e.Cell.Column.Name)
                    {
                        case "MAND_ERR_CNT":
                            {
                                if (e.Cell.Text.Equals("0"))
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                    e.Cell.Presenter.FontWeight = FontWeights.Bold;
                                }
                            }
                            break;
                        default:
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                                e.Cell.Presenter.FontWeight = FontWeights.Normal;
                            }
                            break;

                    }
                }));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// name         : chkViewErrOnly_Click
        /// desc         : B2BI ErrOnly 체크박스 이벤트
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void chkViewErrOnly_Click(object sender, RoutedEventArgs e)
        {
            SearchRowDataHeader();
        }

        /// <summary>
        /// name         : dgRowdata_LoadedCellPresenter
        /// desc         : B2BI 헤더 Cell 색상 변경
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void dgRowdata_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (dgRowdata == null) return;

                dgRowdata.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null) return;
                    if (e.Cell.Row.Type != DataGridRowType.Item) return;

                    switch (e.Cell.Column.Tag.ToString())
                    {
                        case "Y":
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                break;
                            }
                        default:
                            {
                                e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                break;
                            }
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// name         : txtSearchLotId_GotFocus
        /// desc         : B2BI LOTID SELECT ALL 
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 09:47:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void txtSearchLotId_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearchLotId.SelectAll();
        }
        #endregion

        #region #4.Function

        #region Initialize
        private void Initialize()
        {
            InitializeComponent();
            InitializeCombo();
        }

        private void InitializeCombo()
        {
            CommonCombo cbo = new CommonCombo();

            B2Bi_ShipTo_Combo();
            SetComboData();
            If_Flag_Combo();
        }
        
        #endregion

        #region 콤보 박스 세팅

        /// <summary>
        /// name         : If_Flag_Combo
        /// desc         : I/F 전송 여부
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void If_Flag_Combo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("CBO_CODE"), new DataColumn("CBO_NAME") });

                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });
                dtAllRow.Rows.Add(new object[] { "Y", "Y" });
                dtAllRow.Rows.Add(new object[] { "N", "N" });

                cboIfFlag.DisplayMemberPath = "CBO_NAME";
                cboIfFlag.SelectedValuePath = "CBO_CODE";

                cboIfFlag.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboIfFlag.SelectedIndex = 0;

            }
            catch { }
        }

        /// <summary>
        /// name         : SetComboData
        /// desc         : 라인
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SetComboData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                dtCombo = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_B2BI_LINE_PRODID_COMBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtCombo.Rows.Count != 0)
                {
                    //Line 정보 Set
                    DataTable dtLineGp = dtCombo.DefaultView.ToTable(true, new string[] { "EQSGID", "EQSGNAME" });
                    DataTable dtAllRow = new DataTable();
                    dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("EQSGID"), new DataColumn("EQSGNAME") });
                    dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                    if (dtLineGp != null && dtLineGp.Rows.Count > 0)
                    {
                        dtAllRow.Merge(dtLineGp);
                    }

                    cboLine.DisplayMemberPath = "EQSGNAME";
                    cboLine.SelectedValuePath = "EQSGID";
                    cboLine.ItemsSource = DataTableConverter.Convert(dtAllRow);

                    cboLine.SelectedIndex = 0;

                    //제품 정보 Set
                    B2Bi_Line2ProdID_Combo();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);

            }
        }

        /// <summary>
        /// name         : SetComboData
        /// desc         : 출하처
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void B2Bi_ShipTo_Combo()
        {
            try
            {
                DataTable dtIndata = new DataTable();
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("SHIPTO_ID"), new DataColumn("SHIPTO_NAME") });

                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("SHOPID", typeof(string));

                DataRow dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dtIndata.Rows.Add(dr);

                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                cboB2BiShipTo.DisplayMemberPath = "SHIPTO_NAME";
                cboB2BiShipTo.SelectedValuePath = "SHIPTO_ID";

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_B2BI_SHIPTOID_COMBO", "RQSTDT", "RSLTDT", dtIndata);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    dtAllRow.Merge(dtResult);
                }

                cboB2BiShipTo.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboB2BiShipTo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /// <summary>
        /// name         : B2Bi_Line2ProdID_Combo
        /// desc         : 제품
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void B2Bi_Line2ProdID_Combo()
        {
            try
            {
                cboProduct.ItemsSource = null;
                DataTable dtAllRow = new DataTable();
                dtAllRow.Columns.AddRange(new DataColumn[] { new DataColumn("PRODID"), new DataColumn("PRODNAME") });
                dtAllRow.Rows.Add(new object[] { null, "-ALL-" });

                if (dtCombo.Rows.Count != 0)
                {
                    string sEqsgId = Util.NVC(cboLine.SelectedValue);
                    if (!string.IsNullOrWhiteSpace(sEqsgId))
                    {
                        DataTable dtPrdt = dtCombo.Select("EQSGID = '" + sEqsgId + "'").CopyToDataTable().DefaultView.ToTable(true, new string[] { "PRODID", "PRODNAME" });


                        if (dtPrdt != null && dtPrdt.Rows.Count > 0)
                        {
                            dtAllRow.Merge(dtPrdt);
                        }
                    }
                }

                cboProduct.DisplayMemberPath = "PRODNAME";
                cboProduct.SelectedValuePath = "PRODID";

                cboProduct.ItemsSource = DataTableConverter.Convert(dtAllRow);
                cboProduct.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region BizCall - DA_PRD_SEL_TB_SFC_B2BI_LOT_PGM_TRNF_HDR

        /// <summary>
        /// name         : fnSendLotIdInput
        /// desc         : B2BI 재전송 데이터 조회
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void fnSendLotIdInput()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSendLotID.Text))
                {
                    // LOTID를 입력하세요
                    Util.MessageInfo("SFU2839");
                    return;
                }

                this.loadingIndicator.Visibility = Visibility.Visible;

                #region 중복LOT Check
                string sScan = this.txtSendLotID.Text.Trim();
                string sLotDs = sScan;
                if (sScan.Contains(","))
                {
                    string[] splt = sScan.Split(',');
                    var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                    int maxLOTIDCount = 299;
                    if (distinctWords.Count() > maxLOTIDCount)
                    {
                        bMaxCountChk = true;
                        this.txtSendLotID.Clear();
                        this.txtSendLotID.Focus();

                        distinctWords.RemoveRange(299, distinctWords.Count - 299);
                    }

                    sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                }
                #endregion

                SendSearchRowDataHeader(sLotDs);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.txtSendLotID.Clear();
                this.txtSendLotID.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// name         : SendSearchRowDataHeader
        /// desc         : B2BI 재전송 데이터 조회
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SendSearchRowDataHeader(string Lots)
        {
            try
            {
                //tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = Lots;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_GET_B2BI_HDRDATA_LOTID", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (dgSendRowdata.Rows.Count > 2)
                    {
                        #region 기존 조회 데이터가 있는 경우
                        //SFU3440 초기화 하시겠습니까?	
                        Util.MessageConfirm("SFU3440", (result) =>
                        {
                            if (result != MessageBoxResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                ClearSendData();

                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                if (dtResult.Rows.Count != 0)
                                {
                                    DataTable dtParameters = null;

                                    var distinctWords = dtResult.AsEnumerable().Where(x => (!String.IsNullOrEmpty(x.Field<string>("ERR_MSG"))));

                                    if (distinctWords.Count() > 0)
                                    {
                                        dtParameters = distinctWords.CopyToDataTable();
                                    }

                                    distinctWords = dtResult.AsEnumerable().Where(x => (String.IsNullOrEmpty(x.Field<string>("ERR_MSG"))
                                                                                        && ((x.Field<string>("SHIPTO_ID") != Util.NVC(Convert.ToString(dtResult.Rows[0]["SHIPTO_ID"])))
                                                                                        || (x.Field<string>("CUST_TRNF_PGM_ID") != Util.NVC(Convert.ToString(dtResult.Rows[0]["CUST_TRNF_PGM_ID"]))))));

                                    if (distinctWords.Count() > 0)
                                    {
                                        DataTable dtTemp = distinctWords.CopyToDataTable();
                                        for (int i = 0; i < dtTemp.Rows.Count; i++)
                                        {
                                            dtTemp.Rows[i]["ERR_MSG"] = "Different type of PROGRAM_ID";
                                        }

                                        dtParameters = dtTemp;
                                    }

                                    distinctWords = dtResult.AsEnumerable().Where(x => (x.Field<string>("RESEND_CHK") == "N"));
                                    if (distinctWords.Count() > 0)
                                    {
                                        DataTable dtTemp = distinctWords.CopyToDataTable();
                                        for (int i = 0; i < dtTemp.Rows.Count; i++)
                                        {
                                            dtTemp.Rows[i]["ERR_MSG"] = "The Data is waiting to be retransmitted.";
                                        }

                                        if (dtParameters == null)
                                            dtParameters = dtTemp;
                                        else
                                            dtParameters.Merge(dtTemp);
                                    }

                                    if (dtParameters != null && dtParameters.Rows.Count > 0)
                                    {
                                        dtParameters = dtParameters.Select("", "LOTID ASC").CopyToDataTable<DataRow>();

                                        PACK001_048_POPUP popup = new PACK001_048_POPUP();
                                        popup.FrameOperation = FrameOperation;

                                        if (popup != null)
                                        {
                                            object[] Parameters = new object[1];
                                            Parameters[0] = dtParameters;
                                            C1WindowExtension.SetParameters(popup, Parameters);
                                            popup.Closed += new EventHandler(CreatePopupWindow_Closed);
                                            grdMain.Children.Add(popup);
                                            popup.BringToFront();
                                        }

                                        var rowsToDelete = dtResult.AsEnumerable().Where(row1 => dtParameters.AsEnumerable().Any(row2 => row1.Field<string>("LOTID") == row2.Field<string>("LOTID"))).ToList();

                                        foreach (var row in rowsToDelete)
                                        {
                                            dtResult.Rows.Remove(row);
                                        }
                                    }

                                    if (bMaxCountChk)
                                    {
                                        bMaxCountChk = false;
                                        Util.MessageValidation("SFU8217", 299);
                                    }

                                    Util.GridSetData(dgSendRowdata, dtResult, FrameOperation);
                                    Util.SetTextBlockText_DataGridRowCount(tbRowDataHeaderCount, Util.NVC(dtResult.Rows.Count));
                                }
                            }
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        });
                        #endregion
                    }
                    else
                    {
                        #region 기존 조회 데이터가 없는 경우
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        if (dtResult.Rows.Count != 0)
                        {
                            DataTable dtParameters = null;

                            var distinctWords = dtResult.AsEnumerable().Where(x => (!String.IsNullOrEmpty(x.Field<string>("ERR_MSG"))));

                            if (distinctWords.Count() > 0)
                            {
                                dtParameters = distinctWords.CopyToDataTable();
                            }

                            distinctWords = dtResult.AsEnumerable().Where(x => (String.IsNullOrEmpty(x.Field<string>("ERR_MSG"))
                                                                                && ((x.Field<string>("SHIPTO_ID") != Util.NVC(Convert.ToString(dtResult.Rows[0]["SHIPTO_ID"])))
                                                                                || (x.Field<string>("CUST_TRNF_PGM_ID") != Util.NVC(Convert.ToString(dtResult.Rows[0]["CUST_TRNF_PGM_ID"]))))));

                            if (distinctWords.Count() > 0)
                            {
                                DataTable dtTemp = distinctWords.CopyToDataTable();
                                for (int i = 0; i < dtTemp.Rows.Count; i++)
                                {
                                    dtTemp.Rows[i]["ERR_MSG"] = "Different type of PROGRAM_ID";
                                }

                                dtParameters = dtTemp;
                            }

                            distinctWords = dtResult.AsEnumerable().Where(x => (x.Field<string>("RESEND_CHK") == "N"));
                            if (distinctWords.Count() > 0)
                            {
                                DataTable dtTemp = distinctWords.CopyToDataTable();
                                for (int i = 0; i < dtTemp.Rows.Count; i++)
                                {
                                    dtTemp.Rows[i]["ERR_MSG"] = "The Data is waiting to be retransmitted.";
                                }

                                if (dtParameters == null)
                                    dtParameters = dtTemp;
                                else
                                    dtParameters.Merge(dtTemp);
                            }

                            if (dtParameters != null && dtParameters.Rows.Count > 0)
                            {
                                dtParameters = dtParameters.Select("", "LOTID ASC").CopyToDataTable<DataRow>();

                                PACK001_048_POPUP popup = new PACK001_048_POPUP();
                                popup.FrameOperation = FrameOperation;

                                if (popup != null)
                                {
                                    object[] Parameters = new object[1];
                                    Parameters[0] = dtParameters;
                                    C1WindowExtension.SetParameters(popup, Parameters);
                                    popup.Closed += new EventHandler(CreatePopupWindow_Closed);
                                    grdMain.Children.Add(popup);
                                    popup.BringToFront();

                                    var rowsToDelete = dtResult.AsEnumerable().Where(row1 => dtParameters.AsEnumerable().Any(row2 => row1.Field<string>("LOTID") == row2.Field<string>("LOTID"))).ToList();

                                    foreach (var row in rowsToDelete)
                                    {
                                        dtResult.Rows.Remove(row);
                                    }
                                }
                            }

                            if (bMaxCountChk)
                            {
                                bMaxCountChk = false;
                                Util.MessageValidation("SFU8217", 299);
                            }

                            Util.GridSetData(dgSendRowdata, dtResult, FrameOperation);
                            Util.SetTextBlockText_DataGridRowCount(tbRowDataHeaderCount, Util.NVC(dtResult.Rows.Count));
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        #endregion
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region BizCall - BR_PRD_REG_REMAKE_ROWDATA_LOTID

        /// <summary>
        /// name         : RemakeRowdataProcess
        /// desc         : B2BI 재전송
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void RemakeRowdataProcess()
        {
            if (dgSendRowdata == null || dgSendRowdata.GetRowCount() == 0) return;

            //재전송 데이터를 생성하시겠습니까?
            Util.MessageConfirm("SFU8261", (result) =>
            {
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
                else
                {
                    if (DataTableConverter.GetValue(dgSendRowdata.Rows[1].DataItem, "CUST_TRNF_PGM_ID").ToString().Contains("PORSCHE"))
                    {
                        #region 포르쉐 전송 유형일 경우 아우디 데이터 포함 여부 체크
                        string message = MessageDic.Instance.GetMessage("SFU9954");
                        message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);

                        //mresult = System.Windows.MessageBox.Show(message, "Confirm", MessageBoxButton.YesNoCancel);

                        PACK001_048_YESNOCANCEL popup = new PACK001_048_YESNOCANCEL();
                        popup.FrameOperation = FrameOperation;

                        if (popup != null)
                        {
                            object[] Parameters = new object[1];
                            Parameters[0] = message;
                            C1WindowExtension.SetParameters(popup, Parameters);
                            popup.Closed += new EventHandler(CreatePopupWindow_Closed2);
                            grdMain.Children.Add(popup);
                            popup.BringToFront();
                        }
                        #endregion
                    }
                    else
                    {
                        int iRowCount = dgSendRowdata.GetRowCount();

                        DataSet dsInput = new DataSet();

                        DataTable INDATA = new DataTable();
                        INDATA.TableName = "INDATA";
                        INDATA.Columns.Add("LOTID", typeof(string));
                        INDATA.Columns.Add("NOTE", typeof(string));
                        INDATA.Columns.Add("USERID", typeof(string));
                        INDATA.Columns.Add("AUDI_CONTAIN_YN", typeof(string));

                        var distinctWords = (from row in dgSendRowdata.GetDataTable(true).AsEnumerable()
                                             select new
                                             {
                                                 LOTID = row.Field<string>("LOTID")
                                             }).ToList();

                        string Lots = string.Join(",", distinctWords.Select(dw => dw.LOTID));

                        DataRow dr = INDATA.NewRow();
                        dr["LOTID"] = Lots;
                        dr["NOTE"] = txtRemakr.Text.Trim();
                        dr["USERID"] = LoginInfo.USERID;
                        dr["AUDI_CONTAIN_YN"] = "N";
                        INDATA.Rows.Add(dr);
                        dsInput.Tables.Add(INDATA);

                        if (INDATA.Rows.Count == 0) return;

                        RemakeRowdataBiz(dsInput);
                    }                  
                }
            });
        }

        /// <summary>
        /// name         : RemakeRowdataBiz
        /// desc         : B2BI 재전송 Biz 호출
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void RemakeRowdataBiz(DataSet dsInput)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_REMAKE_ROWDATA_LOTID", "INDATA", null, (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }

                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                        //화면 초기화
                        ClearSendData();

                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }, dsInput);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Clear 

        /// <summary>
        /// name         : ClearSendData
        /// desc         : B2BI 재전송 조회결과 초기화
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void ClearSendData()
        {
            Util.gridClear(dgSendRowdata);
            txtSendLotID.Text = string.Empty;
            txtRemakr.Text = string.Empty;
            tbRowDataHeaderCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }
        #endregion

        #region BizCall - DA_PRD_SEL_TB_SFC_B2BI_PGM_TRNF_HDR

        /// <summary>
        /// name         : SearchRowDataHeader
        /// desc         : B2BI 전송 정보 헤더 조회 결과 
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SearchRowDataHeader()
        {
            try
            {
                if (String.IsNullOrEmpty(txtPalletId.Text)  && String.IsNullOrEmpty(txtSearchLotId.Text)
                     && (dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "30"); //기간은 %1일 이내 입니다.
                    return;
                }

                tbRowDataHeaderCountLot.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgSearchRowdataHd);
                Util.gridClear(dgRowdata);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_TO", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("B2BI_IF_GNRT_FLAG", typeof(string));
                RQSTDT.Columns.Add("IS_ERR_CNT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                if (string.IsNullOrWhiteSpace(txtPalletId.Text))
                {
                    dr["EQSGID"] = cboLine.SelectedValue;
                    dr["PRODID"] = cboProduct.SelectedValue;
                    dr["BOXID"] = null;

                    dr["ISS_DTTM_FROM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["ISS_DTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else
                {
                    dr["EQSGID"] = null;
                    dr["PRODID"] = null;
                    dr["ISS_DTTM_FROM"] = System.DBNull.Value;
                    dr["ISS_DTTM_TO"] = System.DBNull.Value;
                    dr["BOXID"] = txtPalletId.Text.Trim();
                }

                if (string.IsNullOrWhiteSpace(txtSearchLotId.Text))
                {
                    dr["LOTID"] = null;
                }
                else
                {

                    #region 중복LOT Check
                    string sScan = this.txtSearchLotId.Text.Trim();
                    string sLotDs = sScan;
                    if (sScan.Contains(","))
                    {
                        string[] splt = sScan.Split(',');
                        var distinctWords = (from w in splt where !string.IsNullOrEmpty(w) select w).Distinct().ToList();

                        int maxLOTIDCount = 299;
                        string messageID = "SFU8217";
                        if (distinctWords.Count() > maxLOTIDCount)
                        {
                            Util.MessageValidation(messageID, 299);     // 최대 299개 까지 가능합니다. 
                            this.txtSearchLotId.Clear();
                            this.txtSearchLotId.Focus();
                            return;
                        }

                        if (null == distinctWords || distinctWords.Count() == 0)
                        {
                            this.txtSearchLotId.Clear();
                            this.txtSearchLotId.Focus();
                            return;
                        }

                        sLotDs = distinctWords.Aggregate((current, next) => current + "," + next).ToString();
                    }
                    #endregion

                    dr["LOTID"] = sLotDs;
                }
                dr["SHIPTO_ID"] = cboB2BiShipTo.SelectedValue;
                dr["B2BI_IF_GNRT_FLAG"] = cboIfFlag.SelectedValue;
                dr["IS_ERR_CNT"] = chkViewErrOnly.IsChecked ?? false ? "Y" : null;

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_B2BI_PGM_TRNF_HDR", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    if (dtResult.Rows.Count != 0)
                    {
                        Util.GridSetData(dgSearchRowdataHd, dtResult, FrameOperation);
                        Util.SetTextBlockText_DataGridRowCount(tbRowDataHeaderCountLot, Util.NVC(dtResult.Rows.Count));
                    }
                    loadingIndicator.Visibility = Visibility.Collapsed;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region BizCall - BR_PRD_GET_B2BI_ROWDATA

        /// <summary>
        /// name         : SearchRowDataDetail
        /// desc         : B2BI 전송 정보 상세정보 조회 결과 
        /// author       : 최다솜
        /// create date  : 2024-08-05 오전 08:35:00
        /// update date  : 최종 수정일자, 수정자, 수정개요
        /// </summary>
        private void SearchRowDataDetail(string sPrgId, string sIfId, string sRcvIssId, string sBoxId)
        {
            try
            {
                txtRowdataCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Util.gridClear(dgRowdata);

                loadingIndicator.Visibility = Visibility.Visible;

                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CUST_TRNF_PGM_ID", typeof(string));
                INDATA.Columns.Add("IF_ID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("INPUT_SEQNO", typeof(Int64));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CUST_TRNF_PGM_ID"] = sPrgId.Trim();
                dr["IF_ID"] = sIfId.Trim();
                dr["RCV_ISS_ID"] = sRcvIssId.Trim();
                dr["BOXID"] = sBoxId.Trim();
                dr["INPUT_SEQNO"] = DBNull.Value;

                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_B2BI_ROWDATA", "INDATA", "OUT_HEADER,OUT_DATA", (dsResult, dataException) =>
                {
                    try
                    {
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            return;
                        }
                        else if (dsResult.Tables["OUT_DATA"].Rows.Count != 0)
                        {
                            //Header columns 변경
                            for (int i = 0; i < dgRowdata.Columns.Count; i++)
                            {
                                dgRowdata.Columns[i].Tag = "N";
                                new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#00FFFFFF"));
                                dgRowdata.Columns[i].Visibility = Visibility.Collapsed;
                            }

                            for (int i = 0; i < dsResult.Tables["OUT_HEADER"].Rows.Count; i++)
                            {
                                string sTbCol = dsResult.Tables["OUT_HEADER"].Rows[i]["COLNAME"].ToString();
                                string sColName = dsResult.Tables["OUT_HEADER"].Rows[i]["TRNF_ITEM_NAME"].ToString();

                                dgRowdata.Columns[sTbCol].Tag = dsResult.Tables["OUT_HEADER"].Rows[i]["MAND_ITEM_FLAG"].ToString();

                                dgRowdata.Columns[sTbCol].Header = sColName;
                                dgRowdata.Columns[sTbCol].Visibility = Visibility.Visible;

                            }

                            Util.GridSetData(dgRowdata, dsResult.Tables["OUT_DATA"], FrameOperation);
                            Util.SetTextBlockText_DataGridRowCount(txtRowdataCount, Util.NVC(dsResult.Tables["OUT_DATA"].Rows.Count));

                            dgRowdata.Refresh();
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                    catch (Exception ex)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        throw ex;
                    }
                }, dsInput);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion
    }
}
