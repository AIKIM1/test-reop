/*************************************************************************************
 Created Date : 2019.05.17
      Creator : 이제섭
   Decription : 포장해체 (InBox)
--------------------------------------------------------------------------------------
 [Change History]
 2019.05.17 : 최초생성
    
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using System.Collections.Generic;

namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_231_UNPACK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_231_UNPACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private bool _load = true;

        Util _util = new Util();

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

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_231_UNPACK()
        {
            InitializeComponent();
        }

        #endregion


        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();

                _load = false;
            }
        }

        private void InitializeUserControls()
        {

        }

        #endregion

        #region 
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod  

        /// <summary>
        /// Box List
        /// </summary>
        private void GetBoxInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = String.IsNullOrWhiteSpace(txtboxid.Text.ToString()) ? null : txtboxid.Text;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_UNPACK_LIST_FOR_2D_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count == 0)
                {
                    //BOX 정보가 없습니다.
                    Util.MessageInfo("SFU1180");
                    Util.gridClear(dgbox);
                    return;
                }

                if (!dtResult.Columns.Contains("CHK"))
                    dtResult = _util.gridCheckColumnAdd(dtResult, "CHK");
                Util.GridSetData(dgbox, dtResult, FrameOperation, true);

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

        private void UnpackBox()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inPalletTable = indataSet.Tables.Add("INDATA");
                inPalletTable.Columns.Add("SRCTYPE");
                inPalletTable.Columns.Add("USERID");
                inPalletTable.Columns.Add("LANGID");
                inPalletTable.Columns.Add("EQPTID");

                DataTable inBoxTable = indataSet.Tables.Add("INBOX");
                inBoxTable.Columns.Add("BOXID");

                DataRow newRow = inPalletTable.NewRow();
                newRow["SRCTYPE"] = "UI";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = "N1BBOX514";

                inPalletTable.Rows.Add(newRow);

                List<int> idxBoxList = _util.GetDataGridCheckRowIndex(dgbox, "CHK");

                foreach (int idxBox in idxBoxList)
                {
                    newRow = inBoxTable.NewRow();
                    newRow["BOXID"] = Util.NVC(dgbox.GetCell(idxBox, dgbox.Columns["BOXID"].Index).Value);
                    inBoxTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_BOX_FOR_2D_NJ", "INDATA,INBOX", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        txtboxid.Text = string.Empty;
                        GetBoxInfo();

                        // SFU1275 정상처리되었습니다.
                        Util.MessageInfo("SFU1275");

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

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


        #region [버튼 클릭]
        private void btnUnpack_Click(object sender, RoutedEventArgs e)
        {

            // (내)포장을 해체하시겠습니까?
            Util.MessageConfirm("SFU1184", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    UnpackBox();

                }
            });
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            GetBoxInfo();
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            checkAllProcess();
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            uncheckProcess();
        }

        private void checkAllProcess()
        {
            if (dgbox == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgbox.ItemsSource);

            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = true;
                // C1.WPF.DataGrid.DataGridRow row = dgLotInfo.Rows[idx];
                // DataTableConverter.SetValue(row.DataItem, "CHK", true);
            }

            dgbox.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void uncheckProcess()
        {
            if (dgbox == null)
            {
                return;
            }

            DataTable dt = DataTableConverter.Convert(dgbox.ItemsSource);
            for (int idx = 0; idx < dt.Rows.Count; idx++)
            {
                dt.Rows[idx]["CHK"] = false;
            }
            dgbox.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void dgbox_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
    }
}
#endregion

