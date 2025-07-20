/*************************************************************************************
 Created Date : 2020.10.22
      Creator : 
   Decription : 단선추가
--------------------------------------------------------------------------------------
 [Change History]
 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ELEC003
{
    /// <summary>
    /// ELEC003_WEB_BREAK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC003_WEB_BREAK : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _EQPTID = string.Empty;
        private string _LOTID = string.Empty;
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC003_WEB_BREAK()
        {
            InitializeComponent();
        }
        #endregion
        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _EQPTID = Util.NVC(tmps[0]);
            _LOTID = Util.NVC(tmps[1]);

            txtLotID.Text = _LOTID;
            GetWebBreakLot();

            /////////////////////////////////////////////////////////
            btnDelete.Visibility = Visibility.Collapsed;
            btnSave.Visibility = Visibility.Collapsed;
        }

        private void dgWebBreak_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                {
                    C1DataGrid dataGrid = sender as C1DataGrid;

                    C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                    CheckBox cb = cell.Presenter.Content as CheckBox;

                    int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;

                    if (dataGrid != null)
                    {
                        if (cb.IsChecked == true)
                        {
                            if (!string.Equals(DataTableConverter.GetValue(dataGrid.Rows[idx].DataItem, "MODE"), "OFFLINE"))
                            {
                                DataTableConverter.SetValue(dataGrid.Rows[idx].DataItem, "CHK", "False");
                                cb.IsChecked = false;
                                return;
                            }
                        }
                    }
                }
            }));
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            SetWebBreakRemove();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetWebBreakManual();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWebBreakLot();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion
        #region User Method
        private void GetWebBreakLot()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQPTID"] = _EQPTID;
                dr["LOTID"] = txtLotID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_DFCT_DATA_CLCT_CT", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgWebBreak, dtResult, FrameOperation, false);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void SetWebBreakManual()
        {
            DataTable inDataTable = new DataTable();
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("TRIG_CODE", typeof(string));
            inDataTable.Columns.Add("WEB_BREAK_CNT", typeof(Int16));

            DataRow inLotDetailDataRow = null;
            inLotDetailDataRow = inDataTable.NewRow();
            inLotDetailDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            inLotDetailDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
            inLotDetailDataRow["EQPTID"] = _EQPTID;
            inLotDetailDataRow["USERID"] = LoginInfo.USERID;
            inLotDetailDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
            inLotDetailDataRow["LOTID"] = txtLotID.Text;
            inLotDetailDataRow["TRIG_CODE"] = Util.NVC(cboCoatSide.Text);
            inLotDetailDataRow["WEB_BREAK_CNT"] = Util.NVC_Int(webBreakQty.Value);
            inDataTable.Rows.Add(inLotDetailDataRow);

            new ClientProxy().ExecuteService("BR_PRD_REG_EQPT_DFCT_WEB_BREAK_CLCT_MANUAL", "INDATA", null, inDataTable, (result, resultEx) =>
            {
                try
                {
                    if (resultEx != null)
                    {
                        Util.MessageException(resultEx);
                        return;
                    }
                    Util.MessageInfo("SFU1270");    //저장되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void SetWebBreakRemove()
        {
            DataSet inDataSet = new DataSet();

            DataTable inDataTable = inDataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("IFMODE", typeof(string));
            inDataTable.Columns.Add("EQPTID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("AREAID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));

            DataRow row = null;
            row = inDataTable.NewRow();
            row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            row["IFMODE"] = IFMODE.IFMODE_OFF;
            row["EQPTID"] = _EQPTID;
            row["USERID"] = LoginInfo.USERID;
            row["AREAID"] = LoginInfo.CFG_AREA_ID;
            row["LOTID"] = txtLotID.Text;
            inDataTable.Rows.Add(row);

            DataTable inSeqTable = inDataSet.Tables.Add("INSEQ");
            inSeqTable.Columns.Add("CLCT_SEQNO", typeof(Int64));
            
            for (int i = 0; i < dgWebBreak.GetRowCount(); i++)
            {
                if (Convert.ToBoolean(DataTableConverter.GetValue(dgWebBreak.Rows[i].DataItem, "CHK")) == true)
                {
                    row = inSeqTable.NewRow();
                    row["CLCT_SEQNO"] = Util.NVC_Int(DataTableConverter.GetValue(dgWebBreak.Rows[i].DataItem, "CLCT_SEQNO"));
                    inDataSet.Tables["INSEQ"].Rows.Add(row);
                }
            }

            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_DFCT_WEB_BREAK_CLCT_CANCEL", "INDATA,INSEQ", null, (result, resultEx) =>
            {
                try
                {
                    if (resultEx != null)
                    {
                        Util.MessageException(resultEx);
                        return;
                    }
                    Util.MessageInfo("SFU1273");    //삭제되었습니다.
                this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex) { Util.MessageException(ex); }
            }, inDataSet);
        }
        #endregion
    }
}