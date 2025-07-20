/*************************************************************************************
 Created Date : 2020.05.08
      Creator : 
   Decription : 재와인딩 실적확정
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
using System.Windows.Media;
using System.Collections.Generic;


namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// 재와인딩 실적확정.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_070_END : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _RUNLOT = string.Empty;  // 작업시작 Lot
        private string _SHIFT = string.Empty;
        private string _WORKID = string.Empty;
        private string _WORKNAME = string.Empty;

        DataTable dtOutLotInfo = null;
        DataTable dtInputLotInfo = null;

        Util _Util = new Util();
        public string _ReturnLotID
        {
            get { return _LOTID; }
        }

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ASSY004_070_END()
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
                //dicParam = dic;

                //if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                //if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                //if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                //if (dicParam.ContainsKey("RUNLOT")) _RUNLOT = dicParam["RUNLOT"];
                //if (dicParam.ContainsKey("SHIFT")) _SHIFT = dicParam["SHIFT"];
                //if (dicParam.ContainsKey("WORK_USERID")) _WORKID = dicParam["WORK_USERID"];
                //if (dicParam.ContainsKey("WORK_USERNAME")) _WORKNAME = dicParam["WORK_USERNAME"];

                //parameters[0] = DataTableConverter.Convert(dgOutLotInfo.ItemsSource);       //완성Lot
                //parameters[1] = DataTableConverter.Convert(dgInputLotInfo.ItemsSource);     //투입Lot
                //parameters[2] = Process.ASSY_REWINDER;
                //parameters[3] = Util.NVC(cboEquipment.SelectedValue);
                //parameters[4] = Util.NVC(cboEquipmentSegment.SelectedValue);
                //parameters[5] = _CURRENT_OUTLOT;
                //parameters[6] = Util.NVC(txtShift.Tag);
                //parameters[7] = Util.NVC(txtWorker.Tag);
                //parameters[8] = Util.NVC(txtWorker.Text);

                object[] tmps = C1WindowExtension.GetParameters(this);

                dtOutLotInfo = tmps[0] as DataTable;
                dtInputLotInfo = tmps[1] as DataTable;
                _PROCID = tmps[2] as string;
                _EQPTID = tmps[3] as string;
                _EQSGID = tmps[4] as string;
                _RUNLOT = tmps[5] as string;
                _SHIFT = tmps[6] as string;
                _WORKID = tmps[7] as string;
                _WORKNAME = tmps[8] as string;

                dtInputLotInfo.Columns.Add("REMAIN_QTY", typeof(decimal));
                dtInputLotInfo.Columns.Add("BALANCE_QTY", typeof(decimal));

                GetOutLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public ASSY004_070_END( Dictionary<string, string> dic)
        {
            InitializeComponent();
         }
        private void txtInputQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (dgInputLotInfo.GetRowCount() < 1)
                    return;

                decimal exceedqty = Util.NVC_Decimal(txtInputQty.Value);
                DataTable dt = ((DataView)dgInputLotInfo.ItemsSource).Table;
                foreach (DataRow row in dt.Rows)
                {
                    decimal inputqty = row.Field<decimal>("INPUT_QTY_O");
                    decimal sum_qty = row.Field<decimal>("GOODQTY");

                    decimal totalqty = inputqty + exceedqty;  // 길이초과
                    decimal remainqty = totalqty - sum_qty;
                    decimal balanceqty = totalqty - sum_qty;

                    row.SetField("INPUT_QTY", totalqty);
                    row.SetField("REMAIN_QTY", remainqty);
                    row.SetField("BALANCE_QTY", balanceqty);
                }
                dgInputLotInfo.Refresh(true);
            }
        }

        private void btnLotEnd_Click(object sender, RoutedEventArgs e)
        {
            if (CommonVerify.HasDataGridRow(dgInputLotInfo))
            {
                DataTable dt = ((DataView)dgInputLotInfo.ItemsSource).Table;
                var queryEdit = (from t in dt.AsEnumerable()
                                 where t.Field<decimal>("BALANCE_QTY") != 0
                                 select t).ToList();

                if (queryEdit.Any())
                {
                    Util.MessageValidation("SFU3701");   // 차이수량이 존재하여 실적 확정이 불가 합니다.\r\n생산실적을 재 확인 해주세요.
                    return;
                }
            }
            
            //실적확정 하시겠습니까?
            Util.MessageConfirm("SFU1716", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("INDATA");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("SHIFT", typeof(string));
                        IN_EQP.Columns.Add("WRK_USERID", typeof(string));
                        IN_EQP.Columns.Add("WRK_USER_NAME", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["SHIFT"] = _SHIFT;
                        row["WRK_USERID"] = _WORKID;
                        row["WRK_USER_NAME"] = _WORKNAME;
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("INLOT");
                        IN_INPUT.Columns.Add("LOTID", typeof(string));
                        //IN_INPUT.Columns.Add("INPUTQTY", typeof(decimal));
                        IN_INPUT.Columns.Add("OUTPUTQTY", typeof(decimal));

                        decimal OUTPUT_QTY =0;
                        for (int i = dgInputLotInfo.TopRows.Count; i < dgInputLotInfo.GetRowCount() + dgInputLotInfo.TopRows.Count; i++)
                        {
                            OUTPUT_QTY += Util.NVC_Decimal(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "GOODQTY"));
                        }

                        DataRow newRow = IN_INPUT.NewRow();
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLotInfo.Rows[0].DataItem, "LOTID"));
                        //newRow["INPUTQTY"] = OUTPUT_QTY;
                        newRow["OUTPUTQTY"] = OUTPUT_QTY;
                        IN_INPUT.Rows.Add(newRow);

                        #endregion

                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RW_END_LOT", "INDATA,INLOT", null, (sResult, ex) =>
                        {
                            if (ex != null)
                            {
                                Util.MessageException(ex);
                                return;
                            }

                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (xresult) =>
                            {
                                _LOTID = _RUNLOT;
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
        private void dgInputLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                        return;

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if (e.Cell.Column.Name.Equals("BALANCE_QTY") && !Util.NVC_Decimal(DataTableConverter.GetValue(e.Cell.Row.DataItem, "BALANCE_QTY")).Equals(0))
                        {
                            e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void GetOutLot()
        {
            try
            {
                Util.GridSetData(dgOutLotInfo, dtOutLotInfo, FrameOperation);

                foreach (DataRow row in dtInputLotInfo.Rows)
                {
                    decimal inputqty = row.Field<decimal>("INPUT_QTY");
                    decimal eqptendqty = row.Field<decimal>("EQPT_END_QTY");
                    decimal defectsum = row.Field<int>("DEFECT_SUM");

                    decimal sum_qty = eqptendqty + Convert.ToDecimal(defectsum);
                    decimal remainqty = inputqty - sum_qty;
                    decimal balanceqty = inputqty - sum_qty;

                    row.SetField("GOODQTY", sum_qty);
                    row.SetField("REMAIN_QTY", remainqty);
                    row.SetField("BALANCE_QTY", balanceqty);
                }

                Util.GridSetData(dgInputLotInfo, dtInputLotInfo, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion
    }
}
