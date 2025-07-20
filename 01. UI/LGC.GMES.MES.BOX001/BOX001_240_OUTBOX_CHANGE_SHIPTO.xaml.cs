/*************************************************************************************
 Created Date : 2024.04.22
      Creator : 
   Decription : OUTBOX 출하처 변경
--------------------------------------------------------------------------------------
 [Change History]
 2024.04.22  오수현    E20240318-000346 최초 생성
 2024.04.26  오수현    E20240318-000346 체크박스 체크 후 삭제 기능. 팝업 사이즈 조절
 2024.04.30  오수현    E20240318-000346 변경 출하처 콤보 PopupFindControl로 변경. 변경 처리시 data 체크 추가
 **************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;
using Microsoft.Win32;
using C1.WPF;
using C1.WPF.DataGrid;
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

    public partial class BOX001_240_OUTBOX_CHANGE_SHIPTO : C1Window, IWorkArea
    {
        Util _util = new Util();

        string _AREAID = string.Empty;
        string sUSERID = string.Empty;
        string sSHFTID = string.Empty;


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
        public BOX001_240_OUTBOX_CHANGE_SHIPTO()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _AREAID = Util.NVC(tmps[0]);
            sUSERID = Util.NVC(tmps[1]);
            sSHFTID = Util.NVC(tmps[2]);

            InitCombo();
            InitControl();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            setShipToPopControl("");
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
                DataTable dtSource = DataTableConverter.Convert(dgOutBoxShipto.ItemsSource);
                
                var query = (from t in dtSource.AsEnumerable()
                             where t.Field<string>("BOXID") == sInOutBox
                             select t.Field<string>("BOXID")).ToList();
                if (query.Any())
                {
                    Util.MessageInfo("SFU2051", sInOutBox); // 중복 데이터가 존재 합니다. %1
                    return;

                }
                #endregion

                AddValidationOutbox(sInOutBox);
            }
        }
        #endregion

        #region OUTBOX CheckAll
        private void dgOutBoxShipto_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
                for (int i = 0; i < dgOutBoxShipto.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgOutBoxShipto.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgOutBoxShipto.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgOutBoxShipto.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgOutBoxShipto.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgOutBoxShipto.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgOutBoxShipto.Rows[i].DataItem, "CHK", false);
                }
            }
        }
        #endregion

        #region [변경] 버튼 클릭
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutBoxShipto.GetRowCount() <= 0)
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

            if (string.IsNullOrEmpty(Util.NVC(popChangeShipto.SelectedValue)))
            {
                Util.MessageValidation("SFU4096"); // 출하처를 선택하세요.
                return;
            }

            // 출하처를 변경하시겠습니까?
            Util.MessageConfirm("SFU5001", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet inDataSet = new DataSet();
                    DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("SRCTYPE");
                    inDataTable.Columns.Add("TO_SHIPTO_ID");
                    inDataTable.Columns.Add("USERID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["SRCTYPE"] = "UI";
                    newRow["TO_SHIPTO_ID"] = popChangeShipto.SelectedValue;
                    newRow["USERID"] = sUSERID;

                    inDataTable.Rows.Add(newRow);


                    DataTable inBoxTable = inDataSet.Tables.Add("INOUTBOX");
                    inBoxTable.Columns.Add("BOXID");


                    DataTable dt = DataTableConverter.Convert(dgOutBoxShipto.ItemsSource);
                    newRow = null;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["BOXID"].ToString() != string.Empty)
                        {
                            newRow = inBoxTable.NewRow();
                            newRow["BOXID"] = Util.NVC(dgOutBoxShipto.GetCell(i, dgOutBoxShipto.Columns["BOXID"].Index).Value);

                            inBoxTable.Rows.Add(newRow);
                        }
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_OUTBOX_CHANGE_SHIPTO_NJ", "INDATA,INOUTBOX", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            
                            Util.MessageInfo("SFU1166"); // 변경되었습니다.
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, inDataSet);
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

        #region [삭제] 버튼 클릭
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxOutbox = _util.GetDataGridCheckFirstRowIndex(dgOutBoxShipto, "CHK");
                if (idxOutbox < 0)
                {
                    //SFU1651 선택된 항목이 없습니다.
                    Util.MessageValidation("SFU1651");
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgOutBoxShipto.ItemsSource);

                foreach (DataRow dr in dt.AsEnumerable().ToList())
                {
                    if (dr["CHK"].Equals(bool.TrueString))
                    {
                        dt.Rows.Remove(dr);
                    }
                }

                Util.GridSetData(dgOutBoxShipto, dt, FrameOperation, false);
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
            LoadExcel(dgOutBoxShipto);
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

                            #region 중복 OUTBOX는 추가 안함
                            DataTable dtSource = DataTableConverter.Convert(dgOutBoxShipto.ItemsSource);
                            var query = (from t in dtSource.AsEnumerable()
                                         where t.Field<string>("BOXID") == sInOutBox
                                         select t.Field<string>("BOXID")).ToList();
                            if (query.Any())
                            {
                                sDuplicationOutbox += sInOutBox +"\n";
                            }
                            else
                            {
                                AddValidationOutbox(sInOutBox);
                            }
                            #endregion

                        }

                        if (!string.IsNullOrEmpty(sDuplicationOutbox))
                        {
                            Util.MessageInfo("SFU2051", sDuplicationOutbox); // 중복 데이터가 존재 합니다. %1
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #endregion [EVENT]


        #region [Method]

        #region [변경출하처] 콤보
        private void setShipToPopControl(string prodID)
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO_NJ";
            string[] arrColumn = { "SHOPID", "PRODID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, prodID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popChangeShipto, arrColumn, arrCondition, (string)popChangeShipto.SelectedValuePath, (string)popChangeShipto.DisplayMemberPath);
        }
        #endregion

        #region 매칭 OUTBOX 체크 후 Grid에 outbox 추가
        private void AddValidationOutbox(string sInOutbox)
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable dtIndata = inDataSet.Tables.Add("INDATA");
                dtIndata.Columns.Add("LANGID");

                DataRow dr = null;
                dr = dtIndata.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;

                dtIndata.Rows.Add(dr);

                DataTable dtInbox = inDataSet.Tables.Add("INOUTBOX");
                dtInbox.Columns.Add("BOXID");

                dr = dtInbox.NewRow();
                dr["BOXID"] = sInOutbox;

                dtInbox.Rows.Add(dr);

                new ClientProxy().ExecuteService_Multi("BR_PRD_GET_INPUT_OUTBOX_MIX_NJ", "INDATA,INOUTBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult != null && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            dgOutBoxShipto.AddRows(1);
                            int n_AddRowIndex = dgOutBoxShipto.GetRowCount() - 1;

                            DataTableConverter.SetValue(dgOutBoxShipto.Rows[n_AddRowIndex].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgOutBoxShipto.Rows[n_AddRowIndex].DataItem, "BOXID", bizResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString());
                            DataTableConverter.SetValue(dgOutBoxShipto.Rows[n_AddRowIndex].DataItem, "SHIPTO_ID", bizResult.Tables["OUTDATA"].Rows[0]["SHIPTO_ID"].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                txtInOutBoxID.Text = string.Empty;
            }
        }
        #endregion

        #endregion [Method]
    }
}
