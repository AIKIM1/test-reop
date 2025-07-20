using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using System.Windows;
using System;
using System.Data;
using System.Windows.Controls;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_PROC_RESN_COATING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_PROC_RESN_COATING : C1Window, IWorkArea
    {
        private string _EQPTID = string.Empty;
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _WIPSEQ = string.Empty;
        private string _RESNPOSITION = string.Empty;
        private string _RESNTYPE = string.Empty;
        private string _RESNCODE = string.Empty;
        private decimal _RESNQTY = 0;

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string _ReturnResnCode
        {
            get { return _RESNCODE; }
        }

        public string _ReturnPosition
        {
            get { return _RESNPOSITION; }
        }

        public decimal _ReturnSumQty
        {
            get { return _RESNQTY; }
        }

        public decimal _ReturnDefectCount
        {
            get { return dgResnCollect.GetRowCount(); }
        }

        public CMM_ELEC_PROC_RESN_COATING()
        {
            InitializeComponent();
        }
        #endregion

        #region Loaded Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _EQPTID = Util.NVC(tmps[0]);
            _PROCID = Util.NVC(tmps[1]);
            _LOTID = Util.NVC(tmps[2]);
            _WIPSEQ = Util.NVC(tmps[3]);
            _RESNPOSITION = Util.NVC(tmps[4]);
            _RESNTYPE = Util.NVC(tmps[5]);
            _RESNCODE = Util.NVC(tmps[6]);
               
            if (!string.IsNullOrEmpty(Util.NVC(tmps[7])))
                ((DataGridNumericColumn)dgResnCollect.Columns["RESNQTY"]).Format = Util.NVC(tmps[7]);

            SetGridComboItem(dgResnCollect.Columns["DETL_RSN_CODE"]);
            GetChildLotInfo();
            GetReasonData();
            GetProcWipReasonData();
        }
        #endregion
        #region Event Method
        private void SetGridComboItem(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("CMCDTYPE", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["CMCDTYPE"] = _RESNTYPE;
            IndataTable.Rows.Add(Indata);

            DataTable dt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "INDATA", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dt);
        }

        private void GetChildLotInfo()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CHK", typeof(bool));
                dt.Columns.Add("LOTID", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["CHK"] = true;
                dataRow["LOTID"] = _LOTID;

                dt.Rows.Add(dataRow);

                Util.GridSetData(dgLot, dt, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetReasonData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("RESNPOSITION", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["PROCID"] = _PROCID;
                dataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                dataRow["RESNPOSITION"] = _RESNPOSITION;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_DEFECT", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgResn, result, FrameOperation, true);
                
                for( int i = 0; i < dgResn.Rows.Count; i++)
                {
                    if ( string.Equals(DataTableConverter.GetValue(dgResn.Rows[i].DataItem, "RESNCODE"), _RESNCODE))
                    {
                        DataTableConverter.SetValue(dgResn.Rows[i].DataItem, "CHK", 1);
                        C1.WPF.DataGrid.DataGridCell cell = dgResn.GetCell(i, dgResn.Columns["CHK"].Index);
                        if ( cell != null && cell.Presenter != null && cell.Presenter.Content != null)
                            ((RadioButton)cell.Presenter.Content).IsChecked = true;

                        break;
                    }
                }
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void GetProcWipReasonData()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("LOTID", typeof(string));
                dt.Columns.Add("WIPSEQ", typeof(string));
                dt.Columns.Add("RESNPOSITION", typeof(string));
                dt.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dataRow = dt.NewRow();
                dataRow["LANGID"] = LoginInfo.LANGID;
                dataRow["LOTID"] = _LOTID;
                dataRow["WIPSEQ"] = _WIPSEQ;
                dataRow["RESNPOSITION"] = _RESNPOSITION;
                dataRow["CMCDTYPE"] = _RESNTYPE;

                dt.Rows.Add(dataRow);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_WIPREASON_COAT", "INDATA", "RSLTDT", dt);

                Util.GridSetData(dgResnCollect, result, FrameOperation, true);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void RemoveProcWipReasonData(string sResnSeq)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("LOTID", typeof(string));
            dt.Columns.Add("WIPSEQ", typeof(string));
            dt.Columns.Add("RESNSEQNO", typeof(string));
            
            DataRow dataRow = dt.NewRow();
            dataRow["LOTID"] = _LOTID;
            dataRow["WIPSEQ"] = _WIPSEQ;
            dataRow["RESNSEQNO"] = sResnSeq;

            dt.Rows.Add(dataRow);

            new ClientProxy().ExecuteService("DA_PRD_DEL_PROC_WIPREASON", "RQSTDT", "RSLTDT", dt, (result, removeException) =>
            {
                try
                {
                    if (removeException != null)
                    {
                        Util.MessageException(removeException);
                        return;
                    }
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void SaveProcWipReasonData()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        DataTable inData = indataSet.Tables.Add("INDATA");
                        inData.Columns.Add("WIPSEQ", typeof(string));
                        inData.Columns.Add("PROCID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow inDataRow = null;
                        inDataRow = inData.NewRow();
                        inDataRow["WIPSEQ"] = _WIPSEQ;
                        inDataRow["PROCID"] = _PROCID;
                        inDataRow["USERID"] = LoginInfo.USERID;
                        inData.Rows.Add(inDataRow);

                        DataTable IndataDetailTable = indataSet.Tables.Add("IN_RESN");
                        IndataDetailTable.Columns.Add("LOTID", typeof(string));
                        IndataDetailTable.Columns.Add("ACTID", typeof(string));
                        IndataDetailTable.Columns.Add("RESNSEQNO", typeof(string));
                        IndataDetailTable.Columns.Add("RESNCODE", typeof(string));
                        IndataDetailTable.Columns.Add("RESNQTY", typeof(decimal));
                        IndataDetailTable.Columns.Add("RESNNOTE", typeof(string));
                        IndataDetailTable.Columns.Add("DETL_RSN_CODE", typeof(string));
                        IndataDetailTable.Columns.Add("ACTDTTM", typeof(string));

                        DataTable dt = DataTableConverter.Convert(dgResnCollect.ItemsSource);
                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]) == true)
                            {
                                DataRow inDetailDataRow = null;
                                inDetailDataRow = IndataDetailTable.NewRow();

                                inDetailDataRow["LOTID"] = Util.NVC(inRow["LOTID"]);
                                inDetailDataRow["ACTID"] = Util.NVC(inRow["ACTID"]);
                                inDetailDataRow["RESNSEQNO"] = Util.NVC(inRow["RESNSEQNO"]);
                                inDetailDataRow["RESNCODE"] = Util.NVC(inRow["RESNCODE"]);
                                inDetailDataRow["RESNQTY"] = Util.NVC(inRow["RESNQTY"]) == "" ? 0 : Util.NVC_Decimal(inRow["RESNQTY"]);
                                inDetailDataRow["RESNNOTE"] = Util.NVC(inRow["RESNNOTE"]);
                                inDetailDataRow["DETL_RSN_CODE"] = Util.NVC(inRow["CBO_CODE"]);
                                inDetailDataRow["ACTDTTM"] = Util.NVC(inRow["ACTDTTM"]);

                                IndataDetailTable.Rows.Add(inDetailDataRow);
                            }
                            _RESNQTY += Util.NVC(inRow["RESNQTY"]) == "" ? 0 : Util.NVC_Decimal(inRow["RESNQTY"]);
                        }
                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_PROC_WIPREASON", "INDATA,IN_RESN", null, indataSet);

                        Util.AlertInfo("SFU1270");  //저장되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }
        #endregion
        #region Event
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgResnCollect.ItemsSource == null || dgResnCollect.Rows.Count < 0)
                return;

            DataRow[] lotChecks = Util.gridGetChecked(ref dgLot, "CHK");
            if (lotChecks == null || lotChecks.Length == 0)
            {
                Util.MessageValidation("SFU1381");  //Lot을 선택하세요.
                return;
            }

            DataRow[] resnChecks = Util.gridGetChecked(ref dgResn, "CHK");
            if (resnChecks == null || resnChecks.Length == 0)
            {
                Util.MessageValidation("SFU3643");  //불량/Loss 항목을 선택하세요.
                return;
            }

            DataTable dt = ((DataView)dgResnCollect.ItemsSource).Table;

            /*
            DataRow[] wipResnChecks = dt.Select("LOTID = '" + Util.NVC(lotChecks[0]["LOTID"]) + "' AND RESNCODE = '" + Util.NVC(resnChecks[0]["RESNCODE"]) + "'");
            if (wipResnChecks != null && wipResnChecks.Length > 0)
            {
                Util.MessageValidation("SFU3644");  //이미 등록된 LOT의 불량/Loss 항목 입니다.
                return;
            }
            */

            DataRow dr = dt.NewRow();
            dr["CHK"] = true;
            dr["LOTID"] = Util.NVC(lotChecks[0]["LOTID"]);
            dr["ACTDTTM"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dr["ACTID"] = Util.NVC(resnChecks[0]["ACTID"]);
            dr["RESNCODE"] = Util.NVC(resnChecks[0]["RESNCODE"]);
            dr["RESNNAME"] = Util.NVC(resnChecks[0]["RESNNAME"]);
            //dr["RESNQTY"] = "";
            dr["RESNNOTE"] = "";
            dt.Rows.Add(dr);

            Util.GridSetData(dgResnCollect, dt, FrameOperation, true);
            dgResnCollect.ScrollIntoView(dt.Rows.Count - 1, 0);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgResnCollect.ItemsSource == null || dgResnCollect.Rows.Count < 0)
                return;

            DataTable dt = ((DataView)dgResnCollect.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
            {
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]))
                {
                    if (!string.IsNullOrEmpty(Util.NVC(dt.Rows[i]["RESNSEQNO"])))
                        RemoveProcWipReasonData(Util.NVC(dt.Rows[i]["RESNSEQNO"]));

                    dt.Rows[i].Delete();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 체크 된 거 있는지 확인
            /*
            bool isChecked = false;

            DataTable dt = DataTableConverter.Convert(dgResnCollect.ItemsSource);
            foreach (DataRow inRow in dt.Rows)
            {
                if (Convert.ToBoolean(inRow["CHK"]) == true)
                {
                    isChecked = true;
                    break;
                }
            }

            if ( isChecked == false)
            {
                Util.MessageValidation("SFU3538"); // 선택된 데이터가 없습니다.
                return;
            }
            */
            // SAVE DEFECT
            SaveProcWipReasonData();
        }

        private void dgChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, 1);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, 0);
                }
                rb.IsChecked = true;
                dg.SelectedIndex = idx;
            }
        }
        #endregion
    }
}
