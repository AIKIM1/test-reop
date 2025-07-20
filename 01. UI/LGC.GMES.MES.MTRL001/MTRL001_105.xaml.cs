/*********************************************************************************************************************************
 Created Date : 2022.04.28
      Creator : 김광오
   Decription : 분리막 방치관리
-----------------------------------------------------------------------------------------------------------------------------------
 [Change History]
-----------------------------------------------------------------------------------------------------------------------------------
       DATE            CSR NO            DEVELOPER            DESCRIPTION
-----------------------------------------------------------------------------------------------------------------------------------
  2022.12.01                               주동석           Initial Created.
  2023.06.20                               강성묵           바코드, Lot 자재 수량 컬럼 추가
  2024.08.13           E20240626-000963    정재홍           IWMS 출고 후 PDA Scan 대기 모니터링 
  2025.03.26           HD 증설             오화백           영문으로 했을때 버튼이 보이지 않아서 xaml 텍스트박스 넓이 수정
***********************************************************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF;

namespace LGC.GMES.MES.MTRL001
{
    public partial class MTRL001_105 : UserControl, IWorkArea
    {
        #region Declaration
        private CommonCombo combo = new CommonCombo();
        private Util _Util = new Util();

        private string CSTStatus = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        private CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };
        #endregion

        public MTRL001_105()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void SetMTRLTypeCombo(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "MTRL_TYPE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = DataTableConverter.Convert(dtResult); ; // AddStatus(dtResult, CommonCombo.ComboStatus.NA, "CBO_CODE", "CBO_NAME").Copy().AsDataView();


                cbo.SelectedValue = LoginInfo.CFG_PROC_ID;
                if (cbo.SelectedIndex < 0)
                {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        //Combo 상태정보 : ALL, N/A, SELECT
        private static DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #region [ Event ] - dtpDateFrom
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }
        #endregion

        #region [ Event ] - Button
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
            {
                Util.MessageValidation("SFU2042", "31");  //기간은 {0}일 이내 입니다.
                return;
            }

            SearchData();
        }
        #endregion

        #region [ Method ] - Search
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //동
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, sCase: "cboArea");


            //SetMTRLTypeCombo(cboMTRLTYPE);
            dtpDateFrom.SelectedDateTime = System.DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = System.DateTime.Now;

            
        }

        private void init()
        {
            Util.gridClear(dgSearch);
        }


        private void SearchData()
        {
            try
            {
                init();
                DataTable dtINDATA = new DataTable();
                DataTable dtOUTDATA = new DataTable();

                //dtINDATA.Columns.Add("MTRLTYPE", typeof(string));
                dtINDATA.Columns.Add("LANGID", typeof(string));
                dtINDATA.Columns.Add("STARTDTTM", typeof(string));
                dtINDATA.Columns.Add("ENDDTTM", typeof(string));
                dtINDATA.Columns.Add("AREAID", typeof(string));
                dtINDATA.Columns.Add("EQPTID", typeof(string));
                dtINDATA.Columns.Add("MTRLID", typeof(string));
                dtINDATA.Columns.Add("PLLT_ID", typeof(string));
                //dtINDATA.Columns.Add("MTGRID", typeof(string));

                // 2023.06.20 강성묵 바코드, Lot 자재 수량 컬럼 추가
                dtINDATA.Columns.Add("IWMS_2D_BCD_STR", typeof(string));
                dtINDATA.Columns.Add("ORD_YN", typeof(string));

                DataRow Indata = dtINDATA.NewRow();
                //Indata["MTRLTYPE"] = cboMTRLTYPE.SelectedValue.ToString(); // cboMTRLTYPE.SelectedValue;
                Indata["LANGID"] = LoginInfo.LANGID;
                //Indata["STARTDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd"); Util.GetCondition(dtpDateFrom);
                Indata["STARTDTTM"] = Util.GetCondition(dtpDateFrom);
                //Indata["ENDDTTM"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                Indata["ENDDTTM"] = Util.GetCondition(dtpDateTo);
                //Indata["AREAID"] = cboArea.SelectedValue.ToString();
                Indata["AREAID"] = Util.GetCondition(cboArea) == "" ? null : Util.GetCondition(cboArea);
                Indata["EQPTID"] = LoginInfo.CFG_EQPT_ID;
                Indata["MTRLID"] = txtMtrlID.Text == "" ? null : txtMtrlID.Text;
                Indata["PLLT_ID"] = txtPlltID.Text == "" ? null : txtPlltID.Text;
                //Indata["MTGRID"] = cboMaterialGroup.SelectedValue.ToString();

                // 2023.06.20 강성묵 바코드, Lot 자재 수량 컬럼 추가
                Indata["IWMS_2D_BCD_STR"] = txtBarCode.Text == "" ? null : txtBarCode.Text;

                // CSR : E20240626-000963 
                Indata["ORD_YN"] = chkOrder.IsChecked == true ? "Y" : "N";

                dtINDATA.Rows.Add(Indata);

                string bizRule = "DA_PRD_SEL_SFC_RMTRL_INPUT_HIST_CNT_MNG";

                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService(bizRule, "INDATA", "OUTDATA", dtINDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex); //Util.AlertInfo(ex.Message);
                        return;
                    }

                    if (dtResult == null || dtResult.Rows.Count == 0)
                    {
                        Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                        return;
                    }

                    Util.GridSetData(dgSearch, dtResult, FrameOperation);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion


        #region [ Util ]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        private void dgSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void btnHist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                int iCnt = Util.NVC_Int(DataTableConverter.GetValue(bt.DataContext, "INPUT_COUNT").ToString());

                string sMtrl = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID")).ToString();
                string sPLLT_ID = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "PLLT_ID")).ToString();
                string sMtrl_LOT = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SUPPLIER_LOTID")).ToString();

                if (iCnt == 0)
                {
                    Util.MessageInfo("KOR1001", sPLLT_ID);
                    return;
                }

                MTRL001_105_POPUP popup = new MTRL001_105_POPUP();
                popup.FrameOperation = this.FrameOperation;

                //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((System.Windows.FrameworkElement)(((System.Windows.FrameworkElement)sender).Parent)).Parent).Row.Index;

                // CSR : E20240626-000963 기존 파라미터 주석 처리 및 신규
                //string sMtrlGubun = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRL_GUBUN")).ToString();
                //string sMtrl = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "MTRLID")).ToString();
                //string sSearch = sMtrlGubun == "RMTRL" ? Util.NVC(DataTableConverter.GetValue(bt.DataContext, "PLLT_ID")).ToString() : Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SUPPLIER_LOTID")).ToString();
                //string sInputDTTM = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "INPUT_DTTM")).ToString();
                //string sMtrl_LOT = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SUPPLIER_LOTID")).ToString();

                //if (sMtrlGubun.Equals(""))
                //{
                //    Util.MessageInfo("KOR1001", sMtrl);
                //    return;
                //}

                if (popup != null)
                {
                    // CSR : E20240626-000963 기존 파라미터 주석 처리 및 신규
                    //object[] Parameters = new object[5];
                    //DataTableConverter.GetValue(bt.DataContext, "LOTID")
                    //Parameters[0] = sMtrlGubun;
                    //Parameters[1] = sMtrl;
                    //Parameters[2] = sSearch;
                    //Parameters[3] = sInputDTTM;
                    //Parameters[4] = sMtrl_LOT;

                    object[] Parameters = new object[3];
                    Parameters[0] = sMtrl;
                    Parameters[1] = sPLLT_ID;
                    Parameters[2] = sMtrl_LOT;

                    C1WindowExtension.SetParameters(popup, Parameters);

                    this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void cboMaterialGroup_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //try
            //{
            //    DataTable RQSTDT = new DataTable();
            //    RQSTDT.TableName = "RQSTDT";
            //    RQSTDT.Columns.Add("LANGID", typeof(string));
            //    RQSTDT.Columns.Add("AREAID", typeof(string));
            //    RQSTDT.Columns.Add("MTGRID", typeof(string));

            //    DataRow dr = RQSTDT.NewRow();
            //    dr["LANGID"] = LoginInfo.LANGID;
            //    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
            //    dr["MTGRID"] = cboMaterialGroup.SelectedValue;
            //    RQSTDT.Rows.Add(dr);

            //    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RMTRL_BY_MTGR_MIX_CBO", "RQSTDT", "RSLTDT", RQSTDT);

            //    cboMaterial.DisplayMemberPath = "MTRLDISP2";
            //    cboMaterial.SelectedValuePath = "MTRLID";
            //    cboMaterial.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "MTRLID", "MTRLDISP2").Copy().AsDataView();
            //    cboMaterial.SelectedIndex = 0;
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            //C1DataGrid dg = sender as C1DataGrid;
            //if (dg != null)
            //{
            //    int iCnt_Day = 0;

            //    dg.Dispatcher.BeginInvoke(new Action(() =>
            //    {
            //        if (e.Cell != null && e.Cell.Presenter != null)
            //        {
            //            if (e.Cell.Row.Type == DataGridRowType.Item)
            //            {
            //                if (string.Equals(e.Cell.Column.Name, "DAY_CNT"))
            //                {
            //                    iCnt_Day = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "DAY_CNT"));

            //                    if (iCnt_Day >= 30)
            //                    {
            //                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
            //                    }
            //                    else
            //                    {
            //                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
            //                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
            //                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
            //                    }
            //                }
            //            }
            //        }
            //    }));
            //}
        }
    }
}

