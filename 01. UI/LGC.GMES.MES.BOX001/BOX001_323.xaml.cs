/*************************************************************************************
 Created Date : 2020.03.24
      Creator : 
   Decription : 21700 Tesla 자동포장기 실적처리 및 포장출고 화면.
--------------------------------------------------------------------------------------
 [Change History]
  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2020.03.24  0.1   이현호       21700 자동포장기 실적처리 및 포장출고 신규화면 개발.
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.UserControls;
using System.Linq;
using LGC.GMES.MES.CMM001;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_323 : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _util = new Util();


        private string _searchStat = string.Empty;
        //private string _searchShipStat = string.Empty;

        private bool bInit = true;

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


        string _sPrt = string.Empty;
        string _sRes = string.Empty;
        string _sCopy = string.Empty;
        string _sXpos = string.Empty;
        string _sYpos = string.Empty;
        string _sDark = string.Empty;
        DataRow _drPrtInfo = null;

        public BOX001_323()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();
            bInit = false;
        }


        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Events


        #region 조회 : btnSearch_Click()
        /// <summary>
        /// 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;
            GetPalletList();
            loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion
    

        #region  Packing LIst 발행 : btnPrint_Click()
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idxPallet = _util.GetDataGridCheckFirstRowIndex(dgPalletList, "CHK");

                if (idxPallet < 0)
                {
                    //SFU1645 선택된 작업대상이 없습니다.
                    Util.MessageValidation("SFU1645");
                    return;
                }

                string iPalletId = Util.NVC(dgPalletList.GetCell(idxPallet, dgPalletList.Columns["BOXID"].Index).Value);

                TagPrint(iPalletId);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

       

        #endregion

        #region [Method]

        #region Pallet 생성 조회 : GetPalletList()
        /// <summary>
        /// Pallet 생성 조회
        /// </summary>
        /// <param name="idx"></param>
        private void GetPalletList(int idx = -1)
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("FROM_DATE", typeof(string));
                RQSTDT.Columns.Add("TO_DATE", typeof(string));
                RQSTDT.Columns.Add("PROJECT", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["PROJECT"] = "M48F";

                if (!string.IsNullOrWhiteSpace(txtPalletID.Text))
                    dr["BOXID"] = txtPalletID.Text;
                else
                {
                    dr["FROM_DATE"] = Util.GetCondition(ldpDateFromDate);
                    dr["TO_DATE"] = Util.GetCondition(ldpDateToDate);
                }

                RQSTDT.Rows.Add(dr);

                DataTable RSLTDT = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_AUTOBOXING_LIST", "INDATA", "OUTDATA", RQSTDT);

                if (!RSLTDT.Columns.Contains("CHK"))
                    RSLTDT = _util.gridCheckColumnAdd(RSLTDT, "CHK");

                Util.GridSetData(dgPalletList, RSLTDT, FrameOperation, true);
                if (idx != -1)
                {
                    DataTableConverter.SetValue(dgPalletList.Rows[idx].DataItem, "CHK", true);
                    dgPalletList.SelectedIndex = idx;
                    dgPalletList.ScrollIntoView(idx, 0);
                }
                else
                {
                    dgPalletList.SelectedIndex = -1;
                }


                if (RSLTDT.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgPalletList.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
        #endregion


        #region Packing LIst 발행 : TagPrint(), popupTagPrint_Closed(), SetShpping()

        private void TagPrint(string sPalletId)
        {
            Report_Gogoro_Tag popup = new Report_Gogoro_Tag();
            popup.FrameOperation = this.FrameOperation;
            //  DataSet ds = GetPalletDataSet();
            if (popup != null)
            {
                object[] Parameters = new object[1];

                Parameters[0] = sPalletId;
                C1WindowExtension.SetParameters(popup, Parameters);

                this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));
            }
        }



        #endregion

        #endregion

        private void dgPalletListChoice_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals(bool.FalseString))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                for (int i = 0; i < ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows.Count; i++)
                {
                    if (idx == i)   // Mode = OneWay 이므로 Set 처리.
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", true);
                    else
                        DataTableConverter.SetValue(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.Rows[i].DataItem, "CHK", false);
                }
                dgPalletList.SelectedIndex = idx;
            }
        }
    }

}
