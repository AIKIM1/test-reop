/*************************************************************************************
 Created Date : 
      Creator : 
   Decription : OUTBOX 일괄 삭제
--------------------------------------------------------------------------------------
 [Change History]
 2024.04.29  오수현    E20240318-000343 Pallet의 바인딩된 완성 OUTBOX 일괄 삭제 기능 추가. Excel Upload 기능 포함 
 2024.04.30  오수현    E20240318-000343 Outbox 삭제 biz 호출시 GUBUN 파라미터 추가(BizRULL 메세지 분기 위함). Outbox 삭제 처리시 data 존재 여부 체크
 **************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using Microsoft.Win32;
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using C1.WPF.Excel;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.IO;
using System.Windows.Media;
using System.Configuration;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_240_OUTBOX_MULTI_DELETE : C1Window, IWorkArea
    {
        Util _util = new Util();

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;
        string sEQSGID = string.Empty;
        string sMULTI_SHIPTO_FLAG = string.Empty;

        #region CheckBox
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
        #endregion

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public string PALLET_ID
        {
            get;
            set;
        }

        #region [Initialize]
        public BOX001_240_OUTBOX_MULTI_DELETE()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);
            PALLET_ID = Util.NVC(tmps[3]);
            sEQSGID = Util.NVC(tmps[4]);
            sMULTI_SHIPTO_FLAG = Util.NVC(tmps[5]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
        }
        #endregion [Initialize]

        #region [EVENT]

        #region 텍스트박스 포커스 
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region [투입 Outbox ID] 입력
        private void txtInOutBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string sInOutBox = Util.NVC(txtInOutBoxID.Text);

                #region 중복 OUTBOX 제거
                DataTable dtSource = DataTableConverter.Convert(dgOutbox.ItemsSource);

                var query = (from t in dtSource.AsEnumerable()
                             where t.Field<string>("OUTBOXID") == sInOutBox
                             select t.Field<string>("OUTBOXID")).ToList();
                if (query.Any())
                {
                    Util.MessageInfo("SFU2051", sInOutBox); // 중복 데이터가 존재 합니다. %1
                    txtInOutBoxID.Text = string.Empty;
                    return;
                }
                #endregion

                GetCompleteOutbox(sInOutBox);
            }
        }
        #endregion

        #region 목록 OUTBOX CheckAll
        private void dgOutbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                            if (e.Column.HeaderPresenter != null)
                            {
                                e.Column.HeaderPresenter.Content = pre;
                            }
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
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgOutbox.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutbox.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region 목록 [삭제] 버튼 클릭
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxOutbox = _util.GetDataGridCheckFirstRowIndex(dgOutbox, "CHK");
                if (idxOutbox < 0)
                {
                    //SFU1651 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgOutbox.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(1))
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                Util.GridSetData(dgOutbox, dt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Excel 등록] 버튼 클릭
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            LoadExcel(dgOutbox);
        }
        private void LoadExcel(C1.WPF.DataGrid.C1DataGrid dataGrid)
        {
            try
            {
                DataTable dtInfo = DataTableConverter.Convert(dataGrid.ItemsSource);

                dtInfo.Clear();

                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";


                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        C1XLBook book = new C1XLBook();
                        book.Load(stream, FileFormat.OpenXml);
                        XLSheet sheet = book.Sheets[0];

                        string sInOutBox = string.Empty;
                        string sDuplicationOutbox = string.Empty;
                        for (int rowInx = 0; rowInx < sheet.Rows.Count; rowInx++)
                        {
                            if (sheet.GetCell(rowInx, 0) == null)
                                return;

                            sInOutBox = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                            DataTable dtSource = DataTableConverter.Convert(dgOutbox.ItemsSource);
                            var query = (from t in dtSource.AsEnumerable()
                                         where t.Field<string>("OUTBOXID") == sInOutBox
                                         select t.Field<string>("OUTBOXID")).ToList();
                            if (query.Any())
                                sDuplicationOutbox += sInOutBox + "\n";
                            else
                                GetCompleteOutbox(sInOutBox);
                        }

                        if (!string.IsNullOrEmpty(sDuplicationOutbox))
                            Util.MessageInfo("SFU2051", sDuplicationOutbox); // 중복 데이터가 존재 합니다. %1
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [OUBOX 삭제] 버튼
        private void btnOutboxDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutbox.GetRowCount() <= 0)
            {
                //SFU5010		Outbox 정보가 없습니다.
                Util.MessageValidation("SFU5010", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtInOutBoxID.Focus();
                        txtInOutBoxID.Text = string.Empty;
                    }
                });
                return;
            }

            // OUTBOX 삭제하시겠습니까?
            Util.MessageConfirm("SFU5000", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                        inPalletTable.Columns.Add("SRCTYPE");
                        inPalletTable.Columns.Add("LANGID");
                        inPalletTable.Columns.Add("BOXID");
                        inPalletTable.Columns.Add("USERID");
                        inPalletTable.Columns.Add("GUBUN");


                        DataTable inBoxTable = indataSet.Tables.Add("INOUTBOX");
                        inBoxTable.Columns.Add("BOXID");

                        DataRow newRow = inPalletTable.NewRow();
                        newRow["SRCTYPE"] = "UI";
                        newRow["LANGID"] = LoginInfo.LANGID;
                        newRow["BOXID"] = PALLET_ID;
                        newRow["USERID"] = sUSERID;
                        newRow["GUBUN"] = "OUTBOX_DELETE";
                        inPalletTable.Rows.Add(newRow);


                        newRow = null;
                        for (int i = 0; i < dgOutbox.GetRowCount(); i++)
                        {
                            string sBoxId = Util.NVC(dgOutbox.GetCell(i, dgOutbox.Columns["OUTBOXID"].Index).Value);
                            var query = (from t in inBoxTable.AsEnumerable()
                                         where t.Field<string>("BOXID") == sBoxId
                                         select t.Field<string>("BOXID")).ToList();
                            if (query.Any())
                                continue;
                            newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = sBoxId;
                            inBoxTable.Rows.Add(newRow);
                        }

                        loadingIndicator.Visibility = Visibility.Visible;
                        new ClientProxy().ExecuteService_Multi("BR_PRD_UNPACK_OUTBOX_NEW_NJ", "INPALLET,INOUTBOX", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                loadingIndicator.Visibility = Visibility.Collapsed;
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }
                                Util.MessageValidation("SFU1273"); // 삭제되었습니다.
                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                                loadingIndicator.Visibility = Visibility.Collapsed;
                            }
                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }
        #endregion

        #region [닫기] 버튼 클릭
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion [EVENT]


        #region [Method]

        #region 완성 OUTBOX 조회 및 목록에 추가
        private void GetCompleteOutbox(string sInOutbox)
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PALLETID", typeof(string));
                inTable.Columns.Add("BOXID", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("MULTI_SHIPTO_FLAG", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["EQSGID"] = sEQSGID;
                newRow["PALLETID"] = PALLET_ID;
                newRow["BOXID"] = sInOutbox;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["MULTI_SHIPTO_FLAG"] = sMULTI_SHIPTO_FLAG;
                inTable.Rows.Add(newRow);

                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET", "RQSTDT", "RQLTDT", inTable);
                
                if (dtResult != null && dtResult.Rows.Count != 0)
                {
                    DataTable dtSource = DataTableConverter.Convert(dgOutbox.ItemsSource);

                    dtResult.Merge(dtSource);

                    Util.GridSetData(dgOutbox, dtResult, FrameOperation, false);

                    string[] sColumnName = new string[] { "OUTBOXID2", "BOXSEQ", "OUTBOXID", "OUTBOXQTY" };
                    if (dgOutbox.GetRowCount() > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgOutbox.Columns["INBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }
                    _util.SetDataGridMergeExtensionCol(dgOutbox, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
                }
                else
                {
                    Util.MessageInfo("SFU3827", sInOutbox, PALLET_ID); // OUTBOX [%1]가 Pallet [%2]에 없습니다. 다시 확인하세요.
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInOutBoxID.Text = string.Empty;
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion [Method]
    }
}
