/*************************************************************************************
 Created Date : 2021.10.21
      Creator : 공민경
   Decription : VD 설비 SKID 공급 요청 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.21  공민경 : 신규 생성
  2024.07.18  오수현 : E20240527-000230 : 검색조건 변경 (추가 - PORT, STATE, 수정- EQPT, 삭제 - PRODID, SKID), GRID에 Port 컬럼 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_071.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_071 : UserControl
    {
        CommonCombo _combo = new CommonCombo();
        string s_VD_STK_EQPTID= string.Empty;

        public MCS001_071()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ldpDateTo.SelectedDateTime = System.DateTime.Today;
            ldpDateFrom.SelectedDateTime = System.DateTime.Today;

            InitCombo();
            GetList();
            this.Loaded -= UserControl_Loaded;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #region Initialize
        private void InitCombo()
        {
            // 동
            C1ComboBox[] cboAreaChild = { cboVDEquipment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild, sCase: "AREA");
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;

            // E20240527-000230 : VD STK 정보 조회
            s_VD_STK_EQPTID = GetVDStocker();
            if (!string.IsNullOrEmpty(s_VD_STK_EQPTID))
            { 
                // Port 콤보
                SetPortCombo(cboStockerPort);
            }

            // E20240527-000230 : Eqpt 콤보
            SetVDEqpCombo(cboVDEquipment);

            // E20240527-000230 : VD 설비 공급 요청 State 콤보
            String[] sFilter1 = { "VD_SPLY_REQ_STAT_CODE" };
            _combo.SetCombo(cboVDSplyReqState, CommonCombo.ComboStatus.ALL, sFilter: sFilter1, sCase: "COMMCODE");

            txtUserName.Text = LoginInfo.USERNAME;
            txtUserName.Tag = LoginInfo.USERID;
        }
        #endregion

        //동별 공통코드 - VD STK 정보
        private string GetVDStocker()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("AREAID", typeof(string));
            RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
            RQSTDT.Columns.Add("ATTR5", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            dr["COM_TYPE_CODE"] = "VD_STK_GRID_COL_NAME";
            dr["ATTR5"] = "VD";
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);
            
            return dtResult.Rows[0]["COM_CODE"].ToString();
        }

    #region Event

    //조회
    private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((ldpDateTo.SelectedDateTime - ldpDateFrom.SelectedDateTime).TotalDays > 7)
            {
                // 조회 기간 한달 To 일자 선택시 From은 해당월의 1일자로 변경
                Util.MessageValidation("SFU2042", "7"); //기간은 {0}일 이내 입니다.
                return;
            }

            GetList();
        }

        //요청자 입력
        private void txtUserName_Select_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GetUserWindow();
            }
        }

        //요청자 버튼 클릭
        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        //부분 요청 취소 버튼 클릭
        private void btnRequestCancel_Part_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation_RequestCancel())
            {
                return;
            }

            string SPLY_REQ_ID = string.Empty;

            for (int i = 0; i < dgList_Select.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "CHK")) == "1")
                {
                    SPLY_REQ_ID = DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "SPLY_REQ_ID").ToString();
                    //dgList_Select.SelectedIndex = i;
                    break;
                }
            }

            MCS001_071_REQUEST_CANCEL popupCancel = new MCS001_071_REQUEST_CANCEL { FrameOperation = FrameOperation };
            object[] parameters = new object[3];
            parameters[0] = SPLY_REQ_ID;
            parameters[1] = txtUserName.Tag.ToString();
            parameters[2] = txtRemark.Text.ToString();

            C1WindowExtension.SetParameters(popupCancel, parameters);

            popupCancel.Closed += popupCancel_Closed;
            Dispatcher.BeginInvoke(new Action(() => popupCancel.ShowModal()));

        }

        //라디오 버튼 선택
        private void dgChk_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb?.DataContext == null) return;

            DataRowView drv = rb.DataContext as DataRowView;

            if (drv != null && ((bool)rb.IsChecked && drv.Row["CHK"].ToString().Equals("0") || Convert.ToBoolean(drv.Row["CHK"]) == false))
            {
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                for (int i = 0; i < ((DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    DataTableConverter.SetValue(((DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", idx == i);
                }

                DataTable TempTable = DataTableConverter.Convert(dgList_Select.ItemsSource).Select("SPLY_REQ_ID = '" + Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[idx].DataItem, "SPLY_REQ_ID")) + "'").CopyToDataTable();
            }
        }

        private void popupCancel_Closed(object sender, EventArgs e)
        {
            MCS001_071_REQUEST_CANCEL popup = sender as MCS001_071_REQUEST_CANCEL;
            if (popup != null && popup.IsUpdated)
            {
                txtRemark.Text = "";
                GetList();
            }
        }

        //요청 취소 버튼 클릭
        private void btnRequestCancel_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validation_RequestCancel())
                {
                    return;
                }
                //취소 하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                        "SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                RequestCancel_All();
                            }
                        });
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;

            LGCDatePicker dtPik = (sender as LGCDatePicker);
            if (Convert.ToDecimal(ldpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpDateTo.SelectedDateTime;
                return;
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == null)
                return;

            LGCDatePicker dtPik = (sender as LGCDatePicker);
            if (Convert.ToDecimal(ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = ldpDateFrom.SelectedDateTime;
                return;
            }
        }

        //그리드 요청ID 더블클릭 (Lot 조회 팝업 오픈)
        private void dgList_Select_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            if (Util.NVC(dg.CurrentColumn.Name).Equals("SPLY_REQ_ID"))
            {
                string SPLY_REQ_ID = string.Empty;

                DataRow dr = dgList_Select.GetDataRow();

                SPLY_REQ_ID = dr["SPLY_REQ_ID"].ToString();

                MCS001_071_LOT popupLot = new MCS001_071_LOT { FrameOperation = FrameOperation };
                object[] parameters = new object[1];
                parameters[0] = SPLY_REQ_ID;

                C1WindowExtension.SetParameters(popupLot, parameters);

                popupLot.Closed += popupLot_Closed;
                Dispatcher.BeginInvoke(new Action(() => popupLot.ShowModal()));
            }
        }

        private void popupLot_Closed(object sender, EventArgs e)
        {
            MCS001_071_LOT popup = sender as MCS001_071_LOT;
            if (popup != null)
            {

            }
        }


        private void dgList_Select_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgList_Select.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //link 색변경
                if (e.Cell.Column.Name.Equals("SPLY_REQ_ID"))
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                }

            }));
        }

        private void cboStockerPort_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            SetVDEqpCombo(cboVDEquipment);
        }
        #endregion

        #region Mehod
        #region VD 설비 SKID 공급 요청 이력 조회
        //조회
        private void GetList()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_VD_EQPT_SPLY_REQ_HIST_NJ";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PORT_ID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SPLY_REQ_STAT_CODE", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROMDATE"] = ldpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = ldpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["AREAID"] = cboArea.SelectedValue;
                dr["PORT_ID"] = cboStockerPort.SelectedValue;
                dr["EQPTID"] = cboVDEquipment.SelectedValue;
                if (!string.IsNullOrEmpty(cboVDSplyReqState.SelectedValue.GetString()))
                {
                    dr["SPLY_REQ_STAT_CODE"] = cboVDSplyReqState.SelectedValue;
                }

                inTable.Rows.Add(dr);

                DataSet ds = new DataSet();
                ds.Tables.Add(inTable);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        dgList_Select.ItemsSource = DataTableConverter.Convert(result);
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        //요청 취소
        private void RequestCancel_All()
        {
            ShowLoadingIndicator();

            DataSet inData = new DataSet();

            //마스터 정보
            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SPLY_REQ_ID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("REQ_CNCL_RSN_CNTT", typeof(string));

            DataRow row = null;

            string SPLY_REQ_ID = string.Empty;

            for (int i = 0; i < dgList_Select.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "CHK")) == "1")
                {
                    SPLY_REQ_ID = DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "SPLY_REQ_ID").ToString();
                }
            }

            row = inDataTable.NewRow();
            row["SPLY_REQ_ID"] = SPLY_REQ_ID; //요청 ID
            row["USERID"] = txtUserName.Tag.ToString(); //요청자
            row["REQ_CNCL_RSN_CNTT"] = txtRemark.Text; //비고

            inDataTable.Rows.Add(row);

            try
            {
                //VD SKID 공급 요청 취소 (전체 취소)
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SKID_SPLY_REQ_CANCEL_ALL_NJ", "INDATA", null, (Result, ex) =>
                {
                    HiddenLoadingIndicator();
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    };
                    Util.MessageInfo("SFU1275"); //정상 처리 되었습니다.
                    txtRemark.Text = "";
                    GetList();
                }, inData);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.AlertByBiz("BR_PRD_REG_SKID_SPLY_REQ_CANCEL_ALL_NJ", ex.Message, ex.ToString());
            }
        }

        // Port 콤보 세팅
        private void SetPortCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_VD_EQPT_SPLY_REQ_HIST_PORT_CBO_NJ";
            string[] arrColumn = { "LANGID", "EQPTID" };
            string[] arrCondition = { LoginInfo.LANGID, s_VD_STK_EQPTID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        // EQPT 콤보 세팅
        private void SetVDEqpCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_MCS_SEL_VD_EQPT_SPLY_REQ_HIST_EQPT_CBO_NJ";
            string[] arrColumn;
            string[] arrCondition;
            if (!string.IsNullOrEmpty(cboStockerPort.SelectedValue.GetString()))
            {
                arrColumn = new string[] { "LANGID", "AREAID", "PORT_ID" };
                arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, cboStockerPort.SelectedValue.GetString() };
            }
            else
            {
                arrColumn = new string[] { "LANGID", "AREAID" };
                arrCondition = new string[] { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID };
            }
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }

        //요청자 세팅
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];

                C1WindowExtension.SetParameters(wndPerson, Parameters);

                Parameters[0] = txtUserName.Text;
                wndPerson.Closed += new EventHandler(wndUser_Closed);
                grdMain.Children.Add(wndPerson);
                wndPerson.BringToFront();
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {
                txtUserName.Text = wndPerson.USERNAME;
                txtUserName.Tag = wndPerson.USERID;
            }
        }

        private bool Validation_RequestCancel()
        {
            DataRow[] drSelect = DataTableConverter.Convert(dgList_Select.ItemsSource).Select("CHK = '1'");

            if (drSelect.Length == 0)
            {
                Util.MessageValidation("SFU1651"); // 선택된 항목이 없습니다.
                return false;
            }

            string REQ_CNCL_DTTM = string.Empty;
            string SPLY_REQ_ID = string.Empty;
            for (int i = 0; i < dgList_Select.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "CHK")) == "1")
                {
                    REQ_CNCL_DTTM = Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "REQ_CNCL_DTTM")).ToString();
                    SPLY_REQ_ID = Util.NVC(DataTableConverter.GetValue(dgList_Select.Rows[i].DataItem, "SPLY_REQ_ID")).ToString();
                    break;
                }
            }

            if (!string.IsNullOrEmpty(REQ_CNCL_DTTM))
            {
                Util.MessageValidation("SFU8248", SPLY_REQ_ID);//[%1] 이미 취소 처리되었습니다.
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserName.Text) || txtUserName.Tag == null)
            {
                Util.MessageValidation("SFU3467");//요청자를 선택해 주세요.
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtRemark.Text) || txtRemark.Text == null)
            {
                Util.MessageValidation("SFU1590");//비고를 입력 하세요.
                return false;
            }

            return true;
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


        #endregion

    }
}
