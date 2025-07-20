/*************************************************************************************
 Created Date : 2020.05.08
      Creator : 
   Decription : 재와인딩 장비완료
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// 재와인딩 장비완료 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_070_EQPTEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _RUNLOT = string.Empty;  // 작업시작 Lot
        private string _STRTTM = string.Empty;

        Util _Util = new Util();
        decimal _OUTEQPTQTY = 0;
        DataTable dtOutLotInfo = null;
        DataTable dtInputLotInfo = null;

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ASSY004_070_EQPTEND()
        {
            InitializeComponent();
            ApplyPermissions();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                dtOutLotInfo = tmps[0] as DataTable;
                dtInputLotInfo = tmps[1] as DataTable;
                _PROCID = tmps[2] as string;
                _EQPTID = tmps[3] as string;
                _EQSGID = tmps[4] as string;
                _RUNLOT = tmps[5] as string;
                _STRTTM = tmps[6] as string;

                GetOutLot();

                if (ldpEndDate != null)
                    ldpEndDate.SelectedDateTime = (DateTime)System.DateTime.Now;
                if (teTimeEditor != null)
                    teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public ASSY004_070_EQPTEND(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("RUNLOT")) _RUNLOT = dicParam["RUNLOT"];

                GetOutLot();

                if (ldpEndDate != null)
                    ldpEndDate.SelectedDateTime = (DateTime)System.DateTime.Now;
                if (teTimeEditor != null)
                    teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotEqptEnd_Click(object sender, RoutedEventArgs e)
        {
            _OUTEQPTQTY = 0;
            for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
            {
                if (DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "WIPSTAT").Equals("PROC") && 
                    !Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "EQPT_END_QTY")).Equals(0))
                    _OUTEQPTQTY += Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "EQPT_END_QTY"));
            }
            if (_OUTEQPTQTY.Equals(0))
            {
                Util.MessageValidation("SFU1684");   // 수량을 입력하세요.
                return;
            }

            //장비완료 하시겠습니까?
            Util.MessageConfirm("SFU1865", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        string strIssueDate = ldpEndDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + teTimeEditor.ToString();

                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("IN_EQP");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("END_DTTM", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["END_DTTM"] = strIssueDate;
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("IN_INPUT");
                        IN_INPUT.Columns.Add("INPUT_LOTID", typeof(string));
                        IN_INPUT.Columns.Add("INPUT_QTY", typeof(decimal));
                        IN_INPUT.Columns.Add("OUTPUT_QTY", typeof(decimal));
                        for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "WIPSTAT").Equals("PROC")
                                && !Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "EQPT_END_QTY")).Equals(0))
                            {
                                DataRow newRow = IN_INPUT.NewRow();
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "INPUT_LOTID"));
                                newRow["INPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "INPUT_QTY"));
                                newRow["OUTPUT_QTY"] = Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "EQPT_END_QTY"));
                                IN_INPUT.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        #region # IN_OUTPUT
                        DataTable IN_OUTPUT = indataSet.Tables.Add("IN_OUTPUT");
                        IN_OUTPUT.Columns.Add("OUT_CSTID", typeof(string));

                        DataRow orow = IN_OUTPUT.NewRow();
                        orow["OUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLotInfo.Rows[0].DataItem, "CSTID"));
                        IN_OUTPUT.Rows.Add(orow);
                        #endregion

                        //DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_ACT_REG_RW_EQPT_END_LOT", "INDATA,INLOT", null, indataSet);

                        ////정상처리되었습니다.
                        //Util.MessageInfo("SFU1275", (xresult) =>
                        //{
                        //    this.DialogResult = MessageBoxResult.OK;
                        //    this.Close();
                        //});

                        new ClientProxy().ExecuteService_Multi("BR_ACT_REG_RW_EQPT_END_LOT", "IN_EQP,IN_INPUT,IN_OUTPUT", null, (sResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (xresult) =>
                            {
                                this.DialogResult = MessageBoxResult.OK;
                                this.Close();
                            });
                        }, indataSet);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetOutLot()
        {



            //foreach (DataRow row in dtInputLotInfo.Rows)
            //{
            //    decimal inputqty = row.Field<decimal>("INPUT_QTY");
            //    decimal eqptendqty = row.Field<decimal>("EQPT_END_QTY");
            //    decimal defectsum = row.Field<int>("DEFECT_SUM");

            //    decimal sum_qty = eqptendqty + Convert.ToDecimal(defectsum);
            //    decimal remainqty = inputqty - sum_qty;
            //    decimal balanceqty = inputqty - sum_qty;

            //    row.SetField("GOODQTY", sum_qty);
            //    row.SetField("REMAIN_QTY", remainqty);
            //    row.SetField("BALANCE_QTY", balanceqty);
            //}

            try
            {

                Util.GridSetData(dgOutLotInfo, dtOutLotInfo, FrameOperation);
                Util.GridSetData(dgInputLotInfo, dtInputLotInfo, FrameOperation);

                //DataTable IndataTable = new DataTable();
                //IndataTable.Columns.Add("LANGID", typeof(string));
                //IndataTable.Columns.Add("LOTID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("EQSGID", typeof(string));
                //IndataTable.Columns.Add("EQPTID", typeof(string));

                //DataRow Indata = IndataTable.NewRow();
                //Indata["LANGID"] = LoginInfo.LANGID;
                //Indata["LOTID"] = _RUNLOT;
                //Indata["PROCID"] = _PROCID;
                //Indata["EQSGID"] = _EQSGID;
                //Indata["EQPTID"] = _EQPTID;

                //IndataTable.Rows.Add(Indata);

                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GET_RW_EQPT_OUT_LOT", "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count <= 0 || dtMain == null)
                //{
                //    dgOutLotInfo.ItemsSource = null;
                //    return;
                //}
                //_STRTTM =  Util.NVC(dtMain.Rows[0]["WIPDTTM_ST"]);
                //dgOutLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);

                //GetProductLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetProductLot()
        {
            try
            {

                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _EQSGID;
                dr["PROCID"] = _PROCID;
                dr["EQPTID"] = _EQPTID;
                dr["LOTID"] = _RUNLOT;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INPUT_RW", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgInputLotInfo, dtResult, FrameOperation);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void dpSearch_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    if (ldpEndDate != null)
                    {
                        if (Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")) < Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")))
                        {
                            Util.MessageValidation("SFU1739");      //오늘 이후 날짜는 선택할 수 없습니다.
                            ldpEndDate.SelectedDateTime = DateTime.Now;

                            return;
                        }

                        if (Convert.ToDecimal(Convert.ToDateTime(_STRTTM).ToString("yyyyMMdd")) > Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")))
                        {
                            Util.MessageValidation("SFU2954");      //종료시간이 시작시간보다 빠를 수는 없습니다.
                            ldpEndDate.SelectedDateTime = DateTime.Now;

                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        //작업종료 팝업에서 종료시간을 미래시간으로 입력 못하게 막아야 함
        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            try
            {
                if (sender == null)
                    return;

                if (teTimeEditor != null)
                {
                    if (new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second) < teTimeEditor.Value)
                    {
                        teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        Util.MessageValidation("SFU1912");      //종료시간을 확인 하세요.
                        return;
                    }

                    if (Convert.ToDecimal(Convert.ToDateTime(_STRTTM).ToString("yyyyMMdd")) == Convert.ToDecimal(ldpEndDate.SelectedDateTime.ToString("yyyyMMdd")) &&
                           TimeSpan.Parse(Convert.ToDateTime(_STRTTM).ToString("HH:mm:ss")) > teTimeEditor.Value)
                    {
                        teTimeEditor.Value = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                        Util.MessageValidation("SFU2954");      //종료시간이 시작시간보다 빠를 수는 없습니다.
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        private void dgInputLotInfo_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1DataGrid grid = sender as C1DataGrid;
            if (grid != null)
            {
                if (e.Column.Index == grid.Columns["EQPT_END_QTY"].Index && DataTableConverter.GetValue(e.Row.DataItem, "WIPSTAT").Equals("EQPT_END"))
                {
                    e.Cancel = true;
                }
            }
        }

        private void dgInputLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            dgInputLotInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                {
                    return;
                }
                e.Cell.Presenter.Background = new SolidColorBrush(Colors.WhiteSmoke);

                if (e.Cell.Column.Name.Equals("EQPT_END_QTY") && !Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPSTAT")).Equals("EQPT_END"))
                {
                    e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                }
            }));
        }
    }
}
