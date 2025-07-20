/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_240_PLT_SPLIT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private Util _Util = new Util();

        private string _userID = string.Empty;
        private string _shftID = string.Empty;
        DataTable _dtSource = new DataTable();
        DataTable _dtTarget = new DataTable();

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

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_240_PLT_SPLIT()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
            InitDataTable();
        }

        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _shftID = tmps[0] as string;
            _userID = tmps[1] as string;
        }
        private void InitDataTable()
        {
            _dtSource.Columns.Add("CHK", typeof(bool));
            _dtSource.Columns.Add("BOXSEQ", typeof(int));
            _dtSource.Columns.Add("OUTBOXID2");
            _dtSource.Columns.Add("OUTBOXID");
            _dtSource.Columns.Add("OUTBOXQTY", typeof(int));
            _dtSource.Columns.Add("PACKDTTM", typeof(DateTime));

            _dtTarget.Columns.Add("CHK", typeof(bool));
            _dtTarget.Columns.Add("BOXSEQ", typeof(int));
            _dtTarget.Columns.Add("OUTBOXID2");
            _dtTarget.Columns.Add("OUTBOXID");
            _dtTarget.Columns.Add("OUTBOXQTY", typeof(int));
            _dtTarget.Columns.Add("PACKDTTM", typeof(DateTime));
        }
        
        #endregion

        #region Event
        private void text_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }

        private void dgTarget_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
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
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string boxID = string.Empty;
                List<int> idxList = _Util.GetDataGridCheckRowIndex(dgTarget, "CHK");
                List<DataRow> drList = new List<DataRow>();
                List<string> idList = new List<string>();

                foreach (int idx in idxList)
                {
                    boxID = Util.NVC(dgTarget.GetCell(idx, dgTarget.Columns["BOXID"].Index).Value);
                    idList.Add(boxID);
                }
                foreach (string id in idList)
                {
                    foreach (DataRow dr in _dtTarget.AsEnumerable().ToList())
                    {
                        if (dr["BOXID"].ToString().Equals(id))
                        {
                            _dtSource.ImportRow(dr);
                            _dtTarget.Rows.Remove(dr);
                        }
                    }
                }
                DataView dvSource = _dtSource.AsDataView();
                dvSource.Sort = "BOXID DESC";
                _dtSource = dvSource.ToTable();

                dgTarget.ItemsSource = DataTableConverter.Convert(_dtTarget);
                dgSource.ItemsSource = DataTableConverter.Convert(_dtSource);
                chkAll.IsChecked = false;
                txtInboxID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            _dtSource.Clear();
            dgSource.ItemsSource = null;
            _dtTarget.Clear();
            dgTarget.ItemsSource = null;
        }
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(txtPalletID.Text))
            {
                try
                {
                    DataTable inTable = new DataTable();
                    inTable.Columns.Add("EQSGID", typeof(string));
                    inTable.Columns.Add("PALLETID", typeof(string));
                    inTable.Columns.Add("BOXID", typeof(string));
                    inTable.Columns.Add("LANGID", typeof(string));

                    DataRow newRow = inTable.NewRow();

                    newRow["PALLETID"] = txtPalletID.Text.ToString().Trim();
                    newRow["LANGID"] = LoginInfo.LANGID;

                    inTable.Rows.Add(newRow);

                    new ClientProxy().ExecuteService("DA_PRD_SEL_COMPLETE_OUTBOX_PALLET", "RQSTDT", "RSLTDT", inTable, (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        if (!result.Columns.Contains("CHK"))
                            result = _Util.gridCheckColumnAdd(result, "CHK");
                        _dtSource = result;
                        Util.GridSetData(dgSource, _dtSource, FrameOperation);
                        _dtTarget.Clear();
                        dgTarget.ItemsSource = null;
                        if (dgSource.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgSource.Columns["OUTBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }
                        txtPalletID.Clear();
                        txtPalletID.Focus();
                    });
                }
                catch (Exception ex)
                {

                    Util.MessageException(ex);
                }
            }
        }
        private void txtInboxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(txtInboxID.Text))
            {
                try
                {
                    string input = txtInboxID.Text;
                    //var query = (from t in dt.AsEnumerable()
                    //             where t.Field<bool>("CHK")
                    //             select t.Field<string>("BOXID")).ToList().Distinct();
                    DataTable dt = DataTableConverter.Convert(dgSource.ItemsSource);
                    var query = (from t in dt.AsEnumerable()
                                 where t.Field<string>("OUTBOXID") == input
                                 select t).Distinct();
                    if (query.Any())
                    {
                        DataRow drSource = _dtSource.Select("OUTBOXID = '" + input + "'").FirstOrDefault();

                        _dtTarget.ImportRow(drSource);
                        _dtSource.Rows.Remove(drSource);

                        dgTarget.ItemsSource = DataTableConverter.Convert(_dtTarget);
                        dgSource.ItemsSource = DataTableConverter.Convert(_dtSource);

                        if (dgTarget.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgTarget.Columns["OUTBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }
                        if (dgSource.Rows.Count > 0)
                        {
                            DataGridAggregate.SetAggregateFunctions(dgSource.Columns["OUTBOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        }

                        txtInboxID.Clear();
                        txtInboxID.Focus();
                    }
                    else
                    {
                        Util.MessageValidation("SFU4308", (result) =>
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                txtInboxID.Clear();
                                txtInboxID.Focus();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgTarget.Rows.Count < 1)
            {
                return;
            }

            try
            {
                DataSet ds = new DataSet();

                DataTable dtInData = ds.Tables.Add("INDATA");
                dtInData.Columns.Add("USERID");
                dtInData.Columns.Add("SHFTID");
                DataRow drInData = dtInData.NewRow();
                drInData["USERID"] = _userID;
                drInData["SHFTID"] = _shftID;
                dtInData.Rows.Add(drInData);

                DataTable dtInPallet = ds.Tables.Add("INPALLET");
                dtInPallet.Columns.Add("PALLETID");
                dtInPallet.Columns.Add("SPLIT_QTY");
                DataRow drInPallet = dtInPallet.NewRow();

                int sumQty = 0;
                drInPallet["PALLETID"] = Util.NVC(dgTarget.GetCell(0, dgTarget.Columns["OUTBOXID2"].Index).Value);

                foreach (DataRow dr in _dtTarget.AsEnumerable())
                {
                    sumQty += int.Parse(dr["OUTBOXQTY"].ToString());
                }

                drInPallet["SPLIT_QTY"] = sumQty;
                dtInPallet.Rows.Add(drInPallet);

                DataTable dtInBox = ds.Tables.Add("INBOX");
                dtInBox.Columns.Add("BOXID");
                dtInBox.Columns.Add("BOXQTY");

                foreach (DataRow dr in _dtTarget.AsEnumerable())
                {
                    DataRow drInBox = dtInBox.NewRow();
                    drInBox["BOXID"] = dr["OUTBOXID"].ToString();
                    drInBox["BOXQTY"] = int.Parse(dr["OUTBOXQTY"].ToString());
                    dtInBox.Rows.Add(drInBox);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_TESLA_PALLET_NJ", "INDATA,INPALLET,INBOX", "OUTDATA", (result, ex) =>
                   {
                       if (ex != null)
                       {
                           Util.MessageException(ex);
                           return;
                       }
                       this.DialogResult = MessageBoxResult.OK;
                       this.Close();

                   }, ds);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if ((bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgTarget.GetRowCount(); i++)
                {
                    // 기존 저장자료는 제외
                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgTarget.Rows[i].DataItem, "CHK"))) || Util.NVC(DataTableConverter.GetValue(dgTarget.Rows[i].DataItem, "CHK")).Equals("0") || Util.NVC(DataTableConverter.GetValue(dgTarget.Rows[i].DataItem, "CHK")).Equals(bool.FalseString))
                        DataTableConverter.SetValue(dgTarget.Rows[i].DataItem, "CHK", true);
                }
            }
        }
        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!(bool)chkAll.IsChecked)
            {
                for (int i = 0; i < dgTarget.GetRowCount(); i++)
                {
                    DataTableConverter.SetValue(dgTarget.Rows[i].DataItem, "CHK", false);
                }
            }
        }


        #endregion

        #region Mehod        


        #endregion


    }
}
