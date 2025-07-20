/*************************************************************************************
 Created Date : 2024.01.19
      Creator : 백광영
   Decription : Create Cell Pallet Shipping Request
--------------------------------------------------------------------------------------
 [Change History]

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
using System.Linq;
using System.Collections;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_399 : UserControl, IWorkArea
    {
        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
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

        public COM001_399()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            Loaded -= UserControl_Loaded;
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            C1ComboBox[] cboAreaChild = { cboBldg };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            SetcboAreaBldg(cboBldg);
            SetcboSection(cboSection);

            SetcboTOSLOC(cboTOSLOC);
            SetcboSHIPTO(cboSHIPTO);
        }

        private void SetcboAreaBldg(C1ComboBox cbo)
        {
            try
            {
                if (cboArea.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));

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


        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                if (cboArea.Items.Count <= 0) return;
                if (cboBldg.Items.Count <= 0) return;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                dr["WH_PHYS_PSTN_CODE"] = Util.GetCondition(cboBldg, MessageDic.Instance.GetMessage("SFU1957"));
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_SECTION_BY_BLDG", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                //DataRow drIns = dtResult.NewRow();
                //drIns["WH_NAME"] = "-ALL-";
                //drIns["WH_ID"] = "";
                //dtResult.Rows.InsertAt(drIns, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboTOSLOC(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TOSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetcboSHIPTO(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["TO_SLOC_ID"] = cboTOSLOC.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIPSLOC_BY_AREA_FORM", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drIns, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();
                if (cbo.Items.Count == 2)
                    cbo.SelectedIndex = 1;
                else
                    cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboBldg_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetcboSection(cboSection);
        }

        private void cboTOSLOC_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetcboSHIPTO(cboSHIPTO);
        }

        private void txtRequestor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetRequestor();
            }
        }

        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) > Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtpDateFrom.Text = System.DateTime.Now.ToLongDateString();
                dtpDateFrom.SelectedDateTime = System.DateTime.Now;

                //오늘 이전 날짜는 선택할 수 없습니다.
                Util.MessageValidation("SFU1738");
                dtpDateFrom.Focus();
            }
        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (Util.NVC(txtPalletID.Text) == string.Empty)
                        return;

                    if (!chkValidation())
                        return;

                    ShowLoadingIndicator();
                    DoEvents();

                    DataTable _result = GetPalletList(_boxid: Util.NVC(getPalletBCD(txtPalletID.Text)));
                    if (_result == null)
                        return;

                    if (_result.Rows.Count > 0)
                    {
                        DataTable dtAdd = DataTableConverter.Convert(_result.Rows);
                        DataTable dtRslt = _result;

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
                                // 그리드 조회 Pallet 아닌 Pallet
                                if (!_sToBeBox)
                                {
                                    dtAdd.ImportRow(rowRlt);
                                }
                            }
                            Util.GridSetData(dgList, dtAdd, FrameOperation, true);
                        }
                        else
                        {
                            Util.GridSetData(dgList, _result, FrameOperation, true);
                        }

                        // Pallet List 체크박스 선택
                        foreach (DataRow _dRow in _result.Rows)
                        {
                            for (int i = 0; i < dgList.GetRowCount(); i++)
                            {
                                string _boxid = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "BOXID"));
                                string _valid = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "VALIDATION"));

                                if (!_valid.Equals("REQ_ERR") && _boxid.Equals(Util.NVC(_dRow["BOXID"])))
                                {
                                    (dgList.GetCell(i, 0).Presenter.Content as CheckBox).IsChecked = true;
                                }
                            }
                        }
                        txtPalletID.Clear();
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
            }
        }

        private void txtPalletID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                try
                {
                    if (!chkValidation())
                        return;

                    ShowLoadingIndicator();
                    DoEvents();

                    string[] stringSeparators = { "\r\n" };
                    string sPasteString = Clipboard.GetText();
                    string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                    if (sPasteStrings.Count() > 100)
                    {
                        Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                        return;
                    }
                    
                    // Pallet List
                    var ValueToFind = new List<string>();
                    foreach (string item in sPasteStrings)
                    {
                        ValueToFind.Add(getPalletBCD(item));
                    }

                    // 조회 BOXID
                    string _mboxid = string.Join(",", ValueToFind);

                    DataTable _result = GetPalletList(_boxid: _mboxid);
                    if (_result == null)
                        return;

                    if (dgList.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgList, _result, FrameOperation, true);
                        for (int i = 0; i < dgList.GetRowCount(); i++)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "VALIDATION")).Equals("REQ_ERR"))
                                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                        }
                    }
                    else
                    {
                        DataTable dtInfo = DataTableConverter.Convert(dgList.ItemsSource);
                        if (_result.Rows.Count != 0)
                        {
                            foreach (DataRow _dRow in _result.Rows)
                            {
                                // 중복 Pallet 체크
                                if (dtInfo.Select("BOXID = '" + Util.NVC(_dRow["BOXID"]) + "'").Count() == 0)
                                {
                                    dtInfo.ImportRow(_dRow);
                                }
                                if (!Util.NVC(_dRow["VALIDATION"]).Equals("REQ_ERR"))
                                {
                                    dtInfo.Select("BOXID = '" + Util.NVC(_dRow["BOXID"]) + "'").ToList<DataRow>().ForEach(r => r["CHK"] = true);
                                    dtInfo.AcceptChanges();                                    
                                }
                            }
                            Util.GridSetData(dgList, dtInfo, FrameOperation, true);
                        }
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
                e.Handled = true;
            }
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid.CurrentRow != null)
            {
                string _vld = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "VALIDATION"));
                if (_vld.Equals("REQ_ERR"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (!Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "VALIDATION")).Equals("REQ_ERR"))
                        DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", false);
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (!chkValidation())
                return;

            // 프로젝트명을 입력하세요.
            if (Util.NVC(txtProjectName.Text) == "")
            {
                Util.MessageValidation("SFU8917");
                return;
            }

            DataTable _result = GetPalletList(_fetchrows: Util.NVC_Int(txtTopRows.Value));
            if (_result == null)
                return;

            Util.GridSetData(dgList, _result, FrameOperation, false);
        }

        private void btnRowClear_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgList);
        }

        private void btnRequestor_Click(object sender, RoutedEventArgs e)
        {
            GetRequestor();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DataTable dt = ((DataView)dgList.ItemsSource).Table;

            var query = dt.AsEnumerable().Where(x => x.Field<string>("CHK").Equals("True"));
            if (query.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");    //선택된 데이터가 없습니다
                return;
            }

            // 제품ID 중복체크
            var queryitem = query.AsEnumerable().GroupBy(x => new
            {
                _prodid = x.Field<string>("PRODID")
            }).Select(g => new
            {
                _sprodid = g.Key._prodid
            }).ToList();

            if (queryitem.Count > 1)
            {
                Util.MessageValidation("SFU4338");    //동일한 제품만 작업 가능합니다.
                return;
            }

            // 입고창고
            string _whId = cboTOSLOC.SelectedValue.ToString();
            if (_whId == null || string.IsNullOrWhiteSpace(_whId) || _whId.Equals("-SELECT-"))
            {

                Util.MessageValidation("SFU2069");   // 입고창고를 선택하세요.
                cboTOSLOC.Focus();
                return;
            }

            // 출하처
            string _outWh = cboSHIPTO.SelectedValue.ToString();
            if (_outWh == null || string.IsNullOrWhiteSpace(_outWh) || _outWh.Equals("-SELECT-"))
            {

                Util.MessageValidation("SFU4096");  // 출하처를 선택하세요.
                cboSHIPTO.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtRequestor.Text))
            {

                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                return;
            }

            if (txtRequestor.Tag == null)
            {

                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                txtRequestor.Text = string.Empty;
                return;
            }

            if (txtRequestor.Tag.ToString() == "")
            {

                Util.MessageValidation("SFU4976");  // 요청자를 확인 하세요.
                txtRequestor.Focus();
                txtRequestor.Text = string.Empty;
                return;
            }

            if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) > Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtpDateFrom.Text = System.DateTime.Now.ToLongDateString();
                dtpDateFrom.SelectedDateTime = System.DateTime.Now;

                //오늘 이전 날짜는 선택할 수 없습니다.
                Util.MessageValidation("SFU1738");
                dtpDateFrom.Focus();
                return;
            }

            // 출고요청 진행 하시겠습니까?
            Util.MessageConfirm("SFU8567", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    SaveProcess();
                }
            });
        }

        private void GetRequestor()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtRequestor.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);

                this.Dispatcher.BeginInvoke(new Action(() => wndPerson.ShowModal()));
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

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }


        private bool chkValidation()
        {
            // 동을 선택하세요
            if (Util.NVC(cboArea.SelectedValue.ToString()) == "")
            {
                Util.MessageValidation("SFU1499");
                return false;
            }
            // 창고를 먼저 선택해주세요.
            if (Util.NVC(cboBldg.SelectedValue.ToString()) == "")
            {
                Util.MessageValidation("SFU2961");
                return false;
            }
            // 창고를 먼저 선택해주세요.
            if (Util.NVC(cboSection.SelectedValue.ToString()) == "")
            {
                Util.MessageValidation("SFU2961");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pallet List
        /// </summary>
        private DataTable GetPalletList(int _fetchrows = 0, string _boxid = null)
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("BLDGCODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("PRJT_NAME", typeof(string));
                INDATA.Columns.Add("BOXID", typeof(string));
                INDATA.Columns.Add("FETCH_ROWS", typeof(Int32));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = Util.GetCondition(cboArea);
                dr["BLDGCODE"] = Util.GetCondition(cboBldg, bAllNull: true);
                dr["WH_ID"] = Util.GetCondition(cboSection, bAllNull: true);
                dr["PRJT_NAME"] = Util.NVC(txtProjectName.Text);
                dr["BOXID"] = _boxid;
                if (_fetchrows > 0)
                    dr["FETCH_ROWS"] = _fetchrows;
                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_CELL_PLLT_SHIP_REQ_LIST", "INDATA", "OUTDATA", INDATA);
                txtPalletID.Clear();
                if (dtResult == null || dtResult.Rows.Count < 1)
                {
                    return null;
                }
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        /// <summary>
        /// Pallet BCD -> Box ID
        /// </summary>
        /// <param name="palletid"></param>
        /// <returns></returns>
        private string getPalletBCD(string palletid)
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

        /// <summary>
        /// Cell Pallet Request
        /// </summary>
        public void SaveProcess()
        {
            try
            {
                ShowLoadingIndicator();
                DoEvents();

                DataRow[] _drChk = DataTableConverter.Convert(dgList.ItemsSource).Select("CHK = 'True'");

                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable IN_REQ = inDataSet.Tables.Add("IN_REQ");
                IN_REQ.Columns.Add("SHOPID", typeof(string));
                IN_REQ.Columns.Add("AREAID", typeof(string));
                IN_REQ.Columns.Add("SLOC_ID", typeof(string));
                IN_REQ.Columns.Add("PRODID", typeof(string));
                IN_REQ.Columns.Add("REQ_QTY", typeof(decimal));
                IN_REQ.Columns.Add("NOTE", typeof(string));
                IN_REQ.Columns.Add("REQ_DTTM", typeof(DateTime));
                IN_REQ.Columns.Add("REQ_USERID", typeof(string));
                IN_REQ.Columns.Add("SHIPTO_ID", typeof(string));
                IN_REQ.Columns.Add("USERID", typeof(string));

                DataRow drReq = IN_REQ.NewRow();
                drReq["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drReq["AREAID"] = Util.GetCondition(cboArea);
                drReq["SLOC_ID"] = Util.GetCondition(cboTOSLOC);
                drReq["PRODID"] = _drChk[0]["PRODID"].ToString();
                drReq["REQ_QTY"] = _drChk.Length;
                drReq["NOTE"] = Util.NVC(txtReqNote.Text);
                drReq["REQ_DTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                drReq["REQ_USERID"] = txtRequestor.Tag;
                drReq["SHIPTO_ID"] = Util.GetCondition(cboSHIPTO);
                drReq["USERID"] = LoginInfo.USERID;
                IN_REQ.Rows.Add(drReq);

                DataTable IN_PALLET = inDataSet.Tables.Add("IN_PALLET");
                IN_PALLET.Columns.Add("BOXID", typeof(string));
                IN_PALLET.Columns.Add("CSTID", typeof(string));

                foreach (DataRow drow in _drChk)
                {
                    DataRow drBox = IN_PALLET.NewRow();
                    drBox["BOXID"] = drow["BOXID"].ToString();
                    drBox["CSTID"] = drow["CSTID"].ToString();
                    IN_PALLET.Rows.Add(drBox);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_CELL_PLLT_SHIP_ORDER_TO_PACK", "IN_REQ,IN_PALLET", null, inDataSet);

                Util.MessageInfo("SFU1275");
                InitRequest();
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();

                DataTable _result = GetPalletList(_fetchrows: Util.NVC_Int(txtTopRows.Value));
                Util.GridSetData(dgList, _result, FrameOperation, false);
            }
        }

        private void InitRequest()
        {
            cboTOSLOC.SelectedIndex = 0;
            cboSHIPTO.SelectedIndex = 0;
            txtRequestor.Clear();
            txtReqNote.Clear();
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }
    }
}