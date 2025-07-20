/*************************************************************************************
 Created Date : 2021.11.02
      Creator : 
   Decription : Pallet 라벨 발행(2D)
--------------------------------------------------------------------------------------
 [Change History]
  2021.11.02  강동희 : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Configuration;
using Microsoft.Win32;
using System.IO;
using C1.WPF.Excel;
using System.Linq;
using System.Windows.Threading;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_119 : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        Util _Util = new Util();

        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;

        DataRow drPrtInfo = null;

        public FCS002_119()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation {
            get;
            set;
        }
        #endregion

        #region [Initialize]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 프린터 정보 조회
            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);

            InitCombo();
        }
        
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            string[] sFilter = { "CSTPROD" };
            _combo.SetCombo(cboCstProd, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter);
        }
        #endregion

        #region [Event]
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

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnOnePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(1);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnTwoPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(2);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnForePrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintClick(4);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
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

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgPalletList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgPalletList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgPalletList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgPalletList.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }

        private void dgPalletList_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;
            C1DataGrid dataGrid = e.Row.DataGrid as C1DataGrid;
            TextBlock tb = new TextBlock();

            if (e.Row.Index + 1 - dataGrid.TopRows.Count > 0)
            {
                tb.Text = (e.Row.Index + 1 - dataGrid.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region [Method]
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("CMCDTYPE", typeof(string));
                dtRqst.Columns.Add("ATTRIBUTE1", typeof(string));
                dtRqst.Columns.Add("CSTTYPE", typeof(string));
                dtRqst.Columns.Add("CSTPROD", typeof(string));
                dtRqst.Columns.Add("CSTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "CSTPROD";
                dr["ATTRIBUTE1"] = "PT";
                dr["CSTTYPE"] = "PT";
                dr["CSTPROD"] = Util.GetCondition(cboCstProd, bAllNull: true);
                dr["CSTID"] = string.IsNullOrEmpty(Util.NVC(txtPalletID.Text)) ? null : Util.NVC(txtPalletID.Text);

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_PRINT_PALLET_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgPalletList, dtRslt, FrameOperation, true);

                if (dtRslt.Rows.Count == 1)
                {
                    //dgPalletList.SelectedIndex = 0;
                    DataTableConverter.SetValue(dgPalletList.Rows[0].DataItem, "CHK", true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintClick(int PrintCnt)
        {
            if (!CommonVerify.HasDataGridRow(dgPalletList)) return;

            try
            {
                DataTable dt = ((DataView)dgPalletList.ItemsSource).Table;
                var query = (from t in dt.AsEnumerable()
                             where t.Field<string>("CHK") == "True"
                             select t).ToList();

                if (CommonVerify.HasDataGridRow(dgPalletList) && query.Any())
                {
                    Util.MessageConfirm("SFU1540", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            PrintLabel(query, PrintCnt);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void PrintLabel(List<DataRow> query, int PrintCnt)
        {
            loadingIndicator.Visibility = Visibility.Visible;

            try
            {
                const string bizRuleName = "DA_SEL_LABEL_PRINT_BY_TRAYID";

                const string item001 = "ITEM001";
                const string item002 = "ITEM002";
                const string item003 = "ITEM003";

                string labelCode = "LBL0294";//2D BarCode

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LABEL_CODE", typeof(string));

                DataRow inData = inDataTable.NewRow();
                inData["LABEL_CODE"] = labelCode;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, bizException) =>
                {
                    if (bizException != null)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageException(bizException);
                        return;
                    }

                    if (CommonVerify.HasTableRow(result))
                    {
                        foreach (DataRow dr in LoginInfo.CFG_SERIAL_PRINT.Rows)
                        {
                            if (Convert.ToBoolean(dr[CustomConfig.CONFIGTABLE_SERIALPRINTER_DEFAULT].ToString()))
                            {
                                // Setting 에 설정된 BarCord Print 정보를 통하여 일치하는 zpl 코드를 가져옴
                                string resolution = dr["DPI"].GetString();
                                string printmodel = dr["PRINTERTYPE"].GetString();
                                string portName = dr["PORTNAME"].GetString();

                                var zplText = (from t in result.AsEnumerable()
                                               where t.Field<string>("PRTR_RESOL_CODE") == resolution
                                                     && t.Field<string>("PRTR_MDL_ID") == printmodel
                                               select new { zplCode = t.Field<string>("DSGN_CNTT") }).FirstOrDefault();

                                if (zplText != null)
                                {
                                    foreach (var item in query)
                                    {
                                        string sITEM001 = string.Empty;
                                        string sITEM002 = string.Empty;
                                        string sITEM003 = string.Empty;
                                        string zplCode = string.Empty;

                                        sITEM001 = item["CSTID"].GetString();
                                        sITEM002 = item["CSTPROD_NAME"].GetString();
                                        sITEM003 = item["CSTID"].GetString();

                                        zplCode =
                                        zplText.zplCode.Replace(item001, sITEM001)
                                            .Replace(item002, sITEM002).Replace(item003, sITEM003);

                                        for (int Cnt = 0; Cnt < PrintCnt; Cnt++)
                                        {
                                            if (Cnt == (PrintCnt - 1))
                                            {
                                                if (chkFeed.IsChecked == true)
                                                {
                                                    string sFeedZpl = " ^XA^A0N,0,0^FO20,20^FD ^FS^PQ1,0,1,Y^XZ";
                                                    zplCode = zplCode + sFeedZpl;
                                                }
                                            }

                                            bool iszplPrint = portName.ToUpper().Equals("USB")
                                                ? FrameOperation.Barcode_ZPL_USB_Print(zplCode)
                                                : FrameOperation.Barcode_ZPL_Print(dr, zplCode);

                                            if (iszplPrint == false)
                                            {
                                                loadingIndicator.Visibility = Visibility.Collapsed;
                                                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                                return;
                                            }
                                            System.Threading.Thread.Sleep(500);
                                        }
                                    }

                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                }
                                else
                                {
                                    loadingIndicator.Visibility = Visibility.Collapsed;
                                    FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("Barcode Print Fail"));
                                    return;
                                }
                            }
                        }
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        Util.MessageInfo("FM_ME_0126");  //라벨 발행을 완료하였습니다.
                    }
                    else
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
