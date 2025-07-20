/*************************************************************************************
 Created Date : 2016.12.05
      Creator : cnslss
   Decription : ERP 실적 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.04.13  WS Son    : 조회 조건 전송일자 -> ERP 전기일 변경, 조회 결과에 ERP 전기일 추가,호출 비즈 변경




 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_050 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string _STATUS = string.Empty;
        private string _LOTID = string.Empty;
        private string _Unit = string.Empty;
        private string _ErrorExc = string.Empty;
        private string _ = string.Empty;

        public COM001_050()
        {
            InitializeComponent();
            InitCombo();
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            tbBoxHistory_cnt.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
        }

        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //Shop
            C1ComboBox[] cboShopChild = { cboArea };
            _combo.SetCombo(cboShop, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sCase: "SHOP_AUTH");

            //동
            C1ComboBox[] cboAreaParent = { cboShop };
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, cbParent: cboAreaParent);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            // 2017.04.13 Start ==
            //C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcess, cboStatus };
            C1ComboBox[] cboEquipmentSegmentChild = { cboShift, cboProcessERP, cboStatus };
            // 2017.04.13 End   ==
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboShift, cboStatus };
            // 2017.04.13 Start ==
            //_combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);
            _combo.SetCombo(cboProcessERP, CommonCombo.ComboStatus.ALL, cbChild: cboProcessChild, cbParent: cboProcessParent);
            // 2017.04.13 End   ==

            //작업조
            // 2017.04.13 Start ==
            //C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcess };
            C1ComboBox[] cboShiftParent = { cboArea, cboEquipmentSegment, cboProcessERP };
            // 2017.04.13 End   ==
            C1ComboBox[] cboShiftChild = { cboStatus };
            _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbChild: cboShiftChild, cbParent: cboShiftParent);

            //결과
            _combo.SetCombo(cboStatus, CommonCombo.ComboStatus.ALL, sCase: "CboErpStaus");
        }

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

        private void dgData_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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

        private void dgData_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;

            if (e.Cell != null &&
                e.Cell.Presenter != null &&
                e.Cell.Presenter.Content != null)
            {
                CheckBox chk = e.Cell.Presenter.Content as CheckBox;
                if (chk != null)
                {
                    switch (Convert.ToString(e.Cell.Column.Name))
                    {
                        case "CHK":

                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            DataTable dt = DataTableConverter.Convert(dg.ItemsSource);

                            if (!chk.IsChecked.Value)
                            {
                                chkAll.IsChecked = false;
                            }
                            else if (dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true).Count() == dt.Rows.Count)
                            {
                                chkAll.IsChecked = true;
                            }

                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                            break;
                    }
                }
            }
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Util.NVC(row["CMCDNAME"]).Equals("NG"))
                    {
                        for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
                        {
                            row["CHK"] = true;
                        }
                    }
                }
            }
            catch
            {
            }  
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
            {
                C1.WPF.DataGrid.DataGridRow row = dgErpHist.Rows[idx];
                DataTableConverter.SetValue(row.DataItem, "CHK", false);
            }
        }
        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                return;
            }
            GetList();
        }

        private void btnReserve_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage();
        }

        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgErpHist);
            }
            catch (Exception ex)
            {
                // LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                Util.Alert(ex.Message);
            }
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            ClearGrid();
            string sArea = cboArea.SelectedValue.ToString();
            
            REMAINERP();
        }

        private void REMAINERP()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("SHIFT_ID", typeof(string));
                dtRqst.Columns.Add("STATUSID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");// Util.StringToDateTime(Util.GetCondition(dtpDateFrom), "yyyyMMdd");
                dr["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");//Util.StringToDateTime(Util.GetCondition(dtpDateTo), "yyyyMMdd");
                dr["SHOPID"] = Util.GetCondition(cboShop, bAllNull: true);
                dr["AREAID"] = Util.GetCondition(cboArea, bAllNull: true);
                // 2017.04.13 Start ==
                //dr["PROCID"] = Util.GetCondition(cboProcess, bAllNull: true);
                dr["PROCID"] = Util.GetCondition(cboProcessERP, bAllNull: true);
                // 2017.04.13 End   ==
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                dr["SHIFT_ID"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["STATUSID"] = Util.GetCondition(cboStatus, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new DataTable();

                if (!(bool) chkBOX.IsChecked)
                {
                    // 2017.004.13 Start ========================================================================================
                    //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_HIST", "INDATA", "OUTDATA", dtRqst);
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_HIST_PACK", "INDATA", "OUTDATA", dtRqst);
                    // 2017.04.13 End   ========================================================================================
                }
                else
                {
                    dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ERP_HIST_PACK2", "INDATA", "OUTDATA", dtRqst);
                }

                dgErpHist.ItemsSource = DataTableConverter.Convert(dtRslt);

                Util.SetTextBlockText_DataGridRowCount(tbBoxHistory_cnt, Util.NVC(dtRslt.Rows.Count));

                DataAll();
                //DefaultHidden();

                DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    if (Util.NVC(row["BOXID"]).Equals("") || Util.NVC(row["LOTID"]).Equals(""))
                    {
                        if (Util.NVC(row["BOXID"]).Equals(""))
                        {
                            //DefaultLotId();
                        }
                        else
                        {
                            //DefaultBoxId();
                        }
                    }
                   
                }

                string[] sColumnName = new string[] { "EQSGNAME", "PROCNAME",  "PRODNAME",  "WOID", "LOTID" ,"EQPTNAME","MODLNAME"};

                //_Util.SetDataGridMergeExtensionCol(dgErpHist, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void ClearGrid()
        {
            dgErpHist.ItemsSource = null;
        }

        private void DataAll()
        {
            DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;
            foreach (DataRow row in dt.Rows)
            {
                for (int idx = 0; idx < dgErpHist.Columns.Count; idx++)
                {
                    dgErpHist.Columns[idx].Visibility = Visibility.Visible;
                }
            }
        }
        private void DefaultHidden()
        {
            dgErpHist.Columns[2].Visibility = Visibility.Hidden;
            dgErpHist.Columns[4].Visibility = Visibility.Hidden;
            dgErpHist.Columns[6].Visibility = Visibility.Hidden;
            //dgErpHist.Columns[7].Visibility = Visibility.Hidden;
            //dgErpHist.Columns[8].Visibility = Visibility.Hidden;
            dgErpHist.Columns[10].Visibility = Visibility.Hidden;
        }

        private void DefaultLotId()
        {
            dgErpHist.Columns[11].Visibility = Visibility.Hidden;
            dgErpHist.Columns[15].Visibility = Visibility.Hidden;
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
        }

        private void DefaultBoxId()
        {
            dgErpHist.Columns[9].Visibility = Visibility.Hidden;
            dgErpHist.Columns[11].Visibility = Visibility.Hidden;
            dgErpHist.Columns[17].Visibility = Visibility.Hidden;
        }

        private void ReserveSend()
        {
            try
            {
                DataSet dtRqst = new DataSet();
                DataTable inData = dtRqst.Tables.Add("INDATA");
                inData.Columns.Add("ERP_TRNF_SEQNO", typeof(string));

                DataRow row = inData.NewRow();

                DataTable dt = ((DataView)dgErpHist.ItemsSource).Table;

                for (int i = 0; i < dgErpHist.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "CHK")).Equals("True"))
                    {
                        row = inData.NewRow();

                        for (int idx = 0; idx < dgErpHist.Rows.Count; idx++)
                        {
                            row["ERP_TRNF_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgErpHist.Rows[i].DataItem, "ERP_TRNF_SEQNO"));
                        }
                        inData.Rows.Add(row);
                    }
                }
                new ClientProxy().ExecuteServiceSync("BR_ACT_REG_RESEND_ERP_PROD", "INDATA", null, inData);
                GetList();
                Util.AlertInfo("SFU1275"); //정상처리되었습니다.

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void ErrorMessage()
        {
            try
            {
                DataTable dts = ((DataView)dgErpHist.ItemsSource).Table;
                foreach (DataRow rows in dts.Rows)
                {
                    for (int i = 0; i < dgErpHist.Rows.Count(); i++)
                    {
                        if (rows["CHK"].ToString().Equals("True") || rows["CHK"].ToString().Equals("1"))
                        {
                            if (!Util.NVC(rows["CMCDNAME"]).Equals("NG"))
                            {
                                Util.Alert("전송실패(NG)된 항목만 재전송 가능합니다.");
                                return;
                            }
                        }
                    }
                }
                ReserveSend();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion

        
    }
}
