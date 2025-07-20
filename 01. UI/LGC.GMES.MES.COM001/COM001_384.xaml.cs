/*************************************************************************************
 Created Date : 2023.05.29
      Creator : 
   Decription : 포장 Pallet 생산 출고 요청
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.29  주재홍 : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어
  2024.01.19  백광영 : 인수대상 선택 후 그리드 Sorting 시 선택항목 오류 수정
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
using System.Linq;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_384 : UserControl, IWorkArea
    {


        List<string> _MColumns1;
        List<string> _MColumns2;

        int dgRequestHistoryIndex = -1;

        string _keyCELL_SPLY_REQ_ID = string.Empty;
        string _keyCELL_SPLY_STAT_CODE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        C1.WPF.DataGrid.DataGridRowHeaderPresenter preCellHold = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        CheckBox chkAllCellHold = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };

        public COM001_384()
        {
            InitializeComponent();

            InitCombo();
            InitColumnsList();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void InitCombo()
        {
            SetAreaCombo(cboArea);
            SetRequestLineCombo(cboReqLine);

            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboBldgHist };
            string _cboCase = "cboArea";
            _combo.SetCombo(cboAreaHist, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: _cboCase);

            SetcboAreaBldg(cboBldgHist);
            SetRequestLineComboHist(cboReqLineHist);

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


        private void SetcboAreaBldg(C1ComboBox cbo)
        {
            try
            {
                if (cboAreaHist.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboAreaHist, MessageDic.Instance.GetMessage("SFU1499"));

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_BLDG_BY_AREA", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "WH_PHYS_PSTN_CODE";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult.DefaultView.ToTable(true));
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void GridClear()
        {
            Util.gridClear(dgPalletList);
        }

        private void SetAreaCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            const string bizRuleName = "DA_BAS_SEL_AUTH_AREA_CBO";
            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("SHOPID", typeof(string));
            inTable.Columns.Add("SYSTEM_ID", typeof(string));
            inTable.Columns.Add("USERID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            dr["SYSTEM_ID"] = LoginInfo.SYSID;
            dr["USERID"] = LoginInfo.USERID;
            inTable.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "CBO_NAME";
            cbo.SelectedValuePath = "CBO_CODE";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (string.IsNullOrEmpty(LoginInfo.CFG_AREA_ID))
            {
                cbo.SelectedIndex = 0;
            }
            else
            {
                cbo.SelectedValue = LoginInfo.CFG_AREA_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }
            }
        }

        private void SetRequestLineCombo(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboArea.SelectedValue.ToString();
            String _EQSGID = string.Empty;
            dr["EQSGID"] = Util.NVC(_EQSGID).Equals(string.Empty) ? null : _EQSGID;
            inTable.Rows.Add(dr);

            const string bizRuleName = "DA_PRD_GET_COMBO_EQSGID_BY_AREA";
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            cbo.DisplayMemberPath = "EQSGNAME";
            cbo.SelectedValuePath = "EQSGID";

            cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (dtResult.Rows.Count > 0 && cboReqLine.SelectedValue == null)
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void SetRequestLineComboHist(C1ComboBox cbo)
        {
            cbo.ItemsSource = null;
            cbo.Items.Clear();

            DataTable inTable = new DataTable("RQSTDT");
            inTable.Columns.Add("LANGID", typeof(string));
            inTable.Columns.Add("AREAID", typeof(string));
            inTable.Columns.Add("EQSGID", typeof(string));

            DataRow dr = inTable.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = cboAreaHist.SelectedValue.ToString();
            String _EQSGID = string.Empty;
            dr["EQSGID"] = Util.NVC(_EQSGID).Equals(string.Empty) ? null : _EQSGID;
            inTable.Rows.Add(dr);

            const string bizRuleName = "DA_PRD_GET_COMBO_EQSGID_BY_AREA";
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inTable);

            string _sdmp = "EQSGNAME";
            string _ssvp = "EQSGID";
            cbo.DisplayMemberPath = _sdmp;
            cbo.SelectedValuePath = _ssvp;
            cbo.ItemsSource = AddStatus(dtResult, _ssvp, _sdmp, "ALL").Copy().AsDataView();

            //cbo.ItemsSource = dtResult.Copy().AsDataView();

            if (dtResult.Rows.Count > 0 && cboReqLineHist.SelectedValue == null)
            {
                cbo.SelectedIndex = 0;
            }
        }

        private void InitColumnsList()
        {
            _MColumns1 = new List<string>();
            _MColumns2 = new List<string>();
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기
            this.Loaded -= UserControl_Loaded;
        }
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
            }
        }
        private void txtModlId_GotFocus(object sender, RoutedEventArgs e)
        {

        }
        public void GetAreaType(string sProcID)
        {
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

        private void ClearValue()
        {
        }

        private void AreaCheck(string sProcID)
        {
        }

        private void txtPalletCSTID_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key != Key.Enter)
            {
                return;
            }

            string _lines = txtPalletCSTID.Text;
            txtPalletCSTID.Text = string.Empty;

            if (_lines.Equals("") || _lines == null) return;

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));

            DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");
            INDATA_BOXID.TableName = "INDATA_BOXID";
            INDATA_BOXID.Columns.Add("BOXID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["LANGID"] = LoginInfo.LANGID;
            INDATA.Rows.Add(dr);
            DataRow drbox = INDATA_BOXID.NewRow();
            drbox["BOXID"] = _lines;
            INDATA_BOXID.Rows.Add(drbox);

            try
            {
                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_PLLT_INFO", "INDATA,INDATA_BOXID", "OUTDATA,OUTDATA_NG_BOXID", (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }

                    if (result.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        DataTable dtAdd = DataTableConverter.Convert(dgPalletList.ItemsSource);
                        DataTable dtRslt = result.Tables["OUTDATA"];

                        if (dtAdd.Rows.Count > 0)
                        {
                            foreach (DataRow rowRlt in dtRslt.Rows)
                            {
                                bool _sToBeBox = false;
                                string _aBox = Convert.ToString(rowRlt["BOXID"]);
                                foreach (DataRow rowAdd in dtAdd.Rows)
                                {
                                    string _bBox = Convert.ToString(rowAdd["BOXID"]);
                                    if (_aBox.Equals(_bBox))
                                    {
                                        _sToBeBox = true;
                                        break;
                                    }
                                }
                                if (!_sToBeBox)
                                {
                                    dtAdd.ImportRow(rowRlt);
                                }
                            }
                            Util.GridSetData(dgPalletList, dtAdd, FrameOperation, true);
                        }
                        else
                        {
                            Util.GridSetData(dgPalletList, dtRslt, FrameOperation, true);
                        }

                        int ii = -1;
                        foreach (DataRow row in dtAdd.Rows)
                        {
                            ii++;
                            string _valid = row["VALIDATION"].ToString();
                            if (_valid.Equals("OK"))
                            {
                                (dgPalletList.GetCell(ii, 0).Presenter.Content as CheckBox).IsChecked = true;
                            }
                        }
                    }

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtPalletCSTID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {

                ShowLoadingIndicator();

                string[] stringSeparators = { "\r\n" };
                string sPasteString = Clipboard.GetText();
                string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                if (sPasteStrings.Count() > 100)
                {
                    Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                    return;
                }


                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");
                INDATA_BOXID.TableName = "INDATA_BOXID";
                INDATA_BOXID.Columns.Add("BOXID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                foreach (string item in sPasteStrings)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item))
                    {
                        DataRow drbox = INDATA_BOXID.NewRow();
                        drbox["BOXID"] = item;
                        INDATA_BOXID.Rows.Add(drbox);
                    }
                }

                try
                {
                    new ClientProxy().ExecuteService_Multi("BR_PRD_GET_BOX_PLLT_INFO", "INDATA,INDATA_BOXID", "OUTDATA,OUTDATA_NG_BOXID", (result, bizex) =>
                    {
                        HiddenLoadingIndicator();

                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }
                        else
                        {
                            if (result.Tables["OUTDATA"].Rows.Count > 0)
                            {

                                DataTable dtAdd = DataTableConverter.Convert(dgPalletList.ItemsSource);
                                DataTable dtRslt = result.Tables["OUTDATA"];
                                if (dtAdd.Rows.Count > 0)
                                {
                                    foreach (DataRow rowRlt in dtRslt.Rows)
                                    {
                                        bool _sToBeBox = false;
                                        string _aBox = Convert.ToString(rowRlt["BOXID"]);
                                        foreach (DataRow rowAdd in dtAdd.Rows)
                                        {
                                            string _bBox = Convert.ToString(rowAdd["BOXID"]);
                                            if (_aBox.Equals(_bBox))
                                            {
                                                _sToBeBox = true;
                                                break;
                                            }
                                        }
                                        if (!_sToBeBox)
                                        {
                                            dtAdd.ImportRow(rowRlt);
                                        }
                                    }
                                    Util.GridSetData(dgPalletList, dtAdd, FrameOperation, true);
                                }
                                else
                                {
                                    Util.GridSetData(dgPalletList, dtRslt, FrameOperation, true);
                                }

                                int ii = -1;
                                foreach (DataRow row in dtAdd.Rows)
                                {
                                    ii++;

                                    string _valid = row["VALIDATION"].ToString();
                                    if (_valid.Equals("OK"))
                                    {
                                        (dgPalletList.GetCell(ii, 0).Presenter.Content as CheckBox).IsChecked = true;
                                    }
                                }

                                txtPalletCSTID.Text = string.Empty;
                            }
                        }
                    }, inDataSet);

                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }
        }


        private void ckPalletListRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;
            int index = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;

            DataTable dt = DataTableConverter.Convert(dgPalletList.ItemsSource);
            DataRow row = dt.Rows[index];

            string _reason = row["NG_REASON"].ToString();
            if (!string.IsNullOrWhiteSpace(_reason))
            {
                Util.AlertInfo(_reason);
                (dgPalletList.GetCell(index, 0).Presenter.Content as CheckBox).IsChecked = false;
            }
            else
            {
                string _valid = row["VALIDATION"].ToString();
                if (!_valid.Equals("OK"))
                {
                    // 데이터 확인중 문제가 발생하였습니다.
                    Util.MessageValidation("SFU3000");
                    (dgPalletList.GetCell(index, 0).Presenter.Content as CheckBox).IsChecked = false;
                }
            }


        }


        private void dgRequestHistoryChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked)
            {
                // 스크롤 움직임은 SKIP
                int rhIdx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                if (dgRequestHistoryIndex == rhIdx)
                    return;
                else
                    dgRequestHistoryIndex = rhIdx;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {

                    if (dgRequestHistoryIndex == i)
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }
                // 해당 row 선택
                dgRequestHistory.SelectedIndex = dgRequestHistoryIndex;

                DataTable dt = ((DataView)dgRequestHistory.ItemsSource).Table;
                DataRow row = dt.Rows[dgRequestHistoryIndex];

                _keyCELL_SPLY_REQ_ID = Convert.ToString(row["CELL_SPLY_REQ_ID"]);
                _keyCELL_SPLY_STAT_CODE = Convert.ToString(row["CELL_SPLY_STAT_CODE"]);

                GetPalletListHist(dgRequestHistoryIndex);

            }
        }


        private void btnRequestor_Click(object sender, RoutedEventArgs e)
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtRequestor.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }

        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtRequestor.Text = wndPerson.USERNAME;
                txtRequestor.Tag = wndPerson.USERID;
            }
            else
            {
                txtRequestor.Text = string.Empty;
                txtRequestor.Tag = string.Empty;
            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private void btnShippingOrder_Click(object sender, RoutedEventArgs e)
        {
            GridClear();
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            DataView dv = (DataView)dgPalletList.ItemsSource;
            if (dv == null)
            {

                Util.MessageValidation("SFU1498");  // 데이터가 없습니다.
                return;
            }

            DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;
            if (dt.Rows.Count < 1)
            {

                Util.MessageValidation("SFU1498");  // 데이터가 없습니다.
                return;
            }

            if (txtRequestor.Tag == null)
            {

                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                return;
            }

            if (txtRequestor.Tag.ToString() == "")
            {

                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                return;
            }

            List<System.Data.DataRow> list = DataTableConverter.Convert(dgPalletList.ItemsSource).Select("CHK = 1").ToList();
            if (list.Count < 1)
            {

                Util.MessageValidation("SFU8573");  // 선택된 파렛이 없습니다.
                return;
            }


            SaveProcess();
        }


        public void SaveProcess()
        {
            ShowLoadingIndicator();

            DataTable dt = DataTableConverter.Convert(dgPalletList.ItemsSource);
            DataRow firstRow = dt.Rows[0];

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable IN_REQ = inDataSet.Tables.Add("IN_REQ");
            DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");

            IN_REQ.TableName = "IN_REQ";
            IN_REQ.Columns.Add("CELL_SPLY_STAT_CODE", typeof(string));
            IN_REQ.Columns.Add("CELL_SPLY_TYPE_CODE", typeof(string));
            IN_REQ.Columns.Add("AREAID", typeof(string));
            IN_REQ.Columns.Add("PRODID", typeof(string));
            IN_REQ.Columns.Add("REQ_QTY", typeof(string));
            IN_REQ.Columns.Add("NOTE", typeof(string));
            IN_REQ.Columns.Add("USERID", typeof(string));
            IN_REQ.Columns.Add("REQ_DTTM", typeof(string));
            IN_REQ.Columns.Add("REQ_USERID", typeof(string));
            IN_REQ.Columns.Add("TO_EQSGID", typeof(string));
            IN_REQ.Columns.Add("CELL_SPLY_RSPN_AREAID", typeof(string));

            INDATA_BOXID.TableName = "INDATA_BOXID";
            INDATA_BOXID.Columns.Add("BOXID", typeof(string));
            INDATA_BOXID.Columns.Add("CSTID", typeof(string));

            DataRow drReq = IN_REQ.NewRow();
            drReq["CELL_SPLY_STAT_CODE"] = "REQUEST";
            drReq["CELL_SPLY_TYPE_CODE"] = "CELL";
            drReq["AREAID"] = cboArea.SelectedValue;
            drReq["PRODID"] = firstRow["PRODID"].ToString();
            drReq["REQ_QTY"] = "0";
            drReq["NOTE"] = txtRemark.Text;
            drReq["USERID"] = LoginInfo.USERID;
            drReq["REQ_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
            drReq["REQ_USERID"] = txtRequestor.Tag;
            drReq["TO_EQSGID"] = cboReqLine.SelectedValue.ToString();
            drReq["CELL_SPLY_RSPN_AREAID"] = firstRow["AREAID"].ToString();

            int _reqQty = 0;
            DataRow[] _drChk = DataTableConverter.Convert(dgPalletList.ItemsSource).Select("CHK = 1");
            foreach (DataRow row in _drChk)
            {
                DataRow drBox = INDATA_BOXID.NewRow();
                drBox["BOXID"] = row["BOXID"].ToString();
                drBox["CSTID"] = row["CSTID"].ToString();
                INDATA_BOXID.Rows.Add(drBox);
                _reqQty = _reqQty + 1;
            }

            drReq["REQ_QTY"] = _reqQty.ToString();
            IN_REQ.Rows.Add(drReq);

            try
            {
                string _bizRol = "BR_PRD_REG_CELL_PLLT_SHIP_REQ_PLLT";
                new ClientProxy().ExecuteService_Multi(_bizRol, "IN_REQ,INDATA_BOXID", null, (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }
                    else
                    {
                        //정상처리되었습니다. 
                        Util.MessageInfo("SFU1275");
                        txtRequestor.Text = string.Empty;
                        txtRequestor.Tag = string.Empty;
                        txtRemark.Text = string.Empty;
                        txtPalletCSTID.Text = string.Empty;
                        GridClear();
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnSearchRequestHist_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgRequestHistory);
            Util.gridClear(dgPalletListHist);
            GetPalletRequestHistory();  // Search 버튼
        }


        private void GetPalletRequestHistory()
        {

            string _lines = txtPalletCSTID.Text;
            txtPalletCSTID.Text = string.Empty;

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("AREAID", typeof(string));
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("REQ_DTTM_FROM", typeof(string));
            INDATA.Columns.Add("REQ_DTTM_TO", typeof(string));
            INDATA.Columns.Add("TO_EQSGID", typeof(string));
            INDATA.Columns.Add("BOXID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["AREAID"] = cboAreaHist.SelectedValue.ToString();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["REQ_DTTM_FROM"] = dtpDateFromHist.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
            dr["REQ_DTTM_TO"] = dtpDateToHist.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
            string _scboReqLineHistValue = cboReqLineHist.SelectedValue.ToString();
            dr["TO_EQSGID"] = Util.NVC(_scboReqLineHistValue).Equals(string.Empty) ? null : _scboReqLineHistValue;
            string _palletIDBCD = txtPalletIDBCD.Text;
            dr["BOXID"] = Util.NVC(_palletIDBCD).Equals(string.Empty) ? null : _palletIDBCD; ;

            INDATA.Rows.Add(dr);

            try
            {
                string _bizRol = "BR_PRD_GET_CELL_PLLT_SHIP_REQ_CELL_HIST";
                new ClientProxy().ExecuteService_Multi(_bizRol, "INDATA", "OUTDATA", (result, bizex) =>
                {
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }

                    if (result.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        Util.GridSetData(dgRequestHistory, result.Tables["OUTDATA"], FrameOperation, true);

                        dgRequestHistoryIndex = 0;
                        dgPalletList.SelectedIndex = dgRequestHistoryIndex;
                        DataTableConverter.SetValue(dgRequestHistory.Rows[dgRequestHistoryIndex].DataItem, "CHK", true);

                        _keyCELL_SPLY_REQ_ID = Convert.ToString(result.Tables["OUTDATA"].Rows[0]["CELL_SPLY_REQ_ID"]);
                        _keyCELL_SPLY_STAT_CODE = Convert.ToString(result.Tables["OUTDATA"].Rows[0]["CELL_SPLY_STAT_CODE"]);

                        GetPalletListHist(dgRequestHistoryIndex);
                    }
                    else
                    {
                        Util.gridClear(dgRequestHistory);
                        Util.gridClear(dgPalletListHist);
                    }

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void GetPalletListHist(int idx)
        {

            ShowLoadingIndicator();
            Util.gridClear(dgPalletListHist);

            DataTable dt = DataTableConverter.Convert(dgRequestHistory.ItemsSource);
            DataRow _row = dt.Rows[idx];

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LANGID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
            INDATA.Columns.Add("DEL_FLAG", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CELL_SPLY_REQ_ID"] = _row["CELL_SPLY_REQ_ID"].ToString();
            dr["DEL_FLAG"] = "N";
            INDATA.Rows.Add(dr);

            try
            {
                string _bizRol = "BR_PRD_GET_CELL_PLLT_SHIP_REQ_PLLT_LIST";
                new ClientProxy().ExecuteService_Multi(_bizRol, "INDATA", "OUTDATA", (result, bizex) =>
                {
                    HiddenLoadingIndicator();
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }

                    if (result.Tables["OUTDATA"].Rows.Count > 0)
                        Util.GridSetData(dgPalletListHist, result.Tables["OUTDATA"], FrameOperation, true);

                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtRequestor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnRequestor_Click(sender, e);
            }
        }

        private void btnCancelRequest_Click(object sender, RoutedEventArgs e)
        {

            if (dgRequestHistoryIndex < 0) return;

            DataTable dt = DataTableConverter.Convert(dgRequestHistory.ItemsSource);
            DataRow _row = dt.Rows[dgRequestHistoryIndex];

            if (!_row["CELL_SPLY_STAT_CODE"].ToString().Equals("REQUEST"))
            {
                Util.MessageValidation("SFU8558");   //REQUEST 상태가 아닙니다.
                return;
            }

            ShowLoadingIndicator();

            DataSet inDataSet = null;
            inDataSet = new DataSet();
            DataTable INDATA = inDataSet.Tables.Add("INDATA");
            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("USERID", typeof(string));
            INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));

            DataRow dr = INDATA.NewRow();
            dr["USERID"] = LoginInfo.USERID;
            dr["CELL_SPLY_REQ_ID"] = _row["CELL_SPLY_REQ_ID"].ToString();
            INDATA.Rows.Add(dr);

            string _bizRol = "BR_PRD_UPD_CELL_PLLT_SHIP_REQ_PLLT_CANCEL";
            new ClientProxy().ExecuteService_Multi(_bizRol, "INDATA", null, (result, bizex) =>
            {
                HiddenLoadingIndicator();
                try
                {
                    if (bizex != null)
                    {
                        Util.MessageException(bizex);
                        return;
                    }

                    // 정상처리 되었습니다.
                    Util.MessageInfo("SFU1275");

                    Util.gridClear(dgRequestHistory);
                    Util.gridClear(dgPalletListHist);

                    GetPalletRequestHistory();  // btnCancelRequest_Click
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }, inDataSet);


        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime baseDate = DateTime.Now;
            if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtpDateFrom.Text = baseDate.ToLongDateString();
                dtpDateFrom.SelectedDateTime = baseDate;
                Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                return;
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgRequestHistory.GetRowCount() == 0)
                    return;

                if (dgRequestHistory.SelectedIndex < 0)
                    return;

                string _reqid = Util.NVC(DataTableConverter.GetValue(dgRequestHistory.Rows[dgRequestHistory.SelectedIndex].DataItem, "CELL_SPLY_REQ_ID"));
                string _statcode = Util.NVC(DataTableConverter.GetValue(dgRequestHistory.Rows[dgRequestHistory.SelectedIndex].DataItem, "CELL_SPLY_STAT_CODE"));


                if (string.IsNullOrEmpty(_reqid))
                {
                    Util.MessageValidation("SFU8570");  // 요청번호를 선택하세요.
                    return;
                }

                if (_statcode != "SHIPPING")
                {
                    Util.MessageValidation("SFU8571");  // SHIPPING 상태만 확정 가능합니다.
                    return;
                }
                
                COM001_384_CONFIRM_REQUEST popup = new COM001_384_CONFIRM_REQUEST();
                popup.FrameOperation = FrameOperation;

                object[] Parameters = new object[1];
                Parameters[0] = _reqid;
                C1WindowExtension.SetParameters(popup, Parameters);

                popup.Closed += new EventHandler(popup_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));

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

        private void popup_Closed(object sender, EventArgs e)
        {
            COM001_384_CONFIRM_REQUEST popup = sender as COM001_384_CONFIRM_REQUEST;
            if (popup.DialogResult == MessageBoxResult.OK)
            {
                Util.gridClear(dgRequestHistory);
                Util.gridClear(dgPalletListHist);
                GetPalletRequestHistory();  // CONFIRM_REQUEST 팝업 종료후
            }
        }

        private void dgPalletList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }
            }));

        }


        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (dgPalletList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;

            //dt.Select("CHK = 0").ToList<DataRow>().ForEach(r => r["CHK"] = 1);
            foreach (DataRow dr in dt.Rows)
            {
                if (Convert.ToString(dr["VALIDATION"]).Equals("OK"))
                {
                    if (!Convert.ToBoolean(dr["CHK"])) dr["CHK"] = true;
                }
            }
            dt.AcceptChanges();

        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgPalletList.ItemsSource == null) return;

            DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;

            dt.Select("CHK = 1").ToList<DataRow>().ForEach(r => r["CHK"] = false);
            dt.AcceptChanges();
        }

    }
}