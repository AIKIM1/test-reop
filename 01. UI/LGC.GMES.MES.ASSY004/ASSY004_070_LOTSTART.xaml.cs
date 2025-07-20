/*************************************************************************************
 Created Date : 2020.05.08
      Creator : 
   Decription : 재와인딩 작업시작
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

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// RW 작업시작 팝업.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_070_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _RUNLOT = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQSGID = string.Empty;

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

        public ASSY004_070_LOTSTART()
        {
            InitializeComponent();
            ApplyPermissions();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //object[] tmps = C1WindowExtension.GetParameters(this);

            //if (tmps == null)
            //    return;

            //_EQSGID = Util.NVC(tmps[0]);
            //_EQPTID = Util.NVC(tmps[1]);
        }

        public ASSY004_070_LOTSTART(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("RUNLOT")) _RUNLOT = dicParam["RUNLOT"];

                GetOutLot();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgOutLotInfo.GetRowCount() < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            int iRow = _Util.GetDataGridCheckFirstRowIndex(dgInputLotInfo, "CHK");

            if (iRow < 0)
            {
                Util.MessageValidation("SFU1651");  //선택된 항목이 없습니다.
                return;
            }

            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DataSet indataSet = new DataSet();
                        #region # IN_EQP
                        DataTable IN_EQP = indataSet.Tables.Add("IN_EQP");
                        IN_EQP.Columns.Add("SRCTYPE", typeof(string));
                        IN_EQP.Columns.Add("IFMODE", typeof(string));
                        IN_EQP.Columns.Add("EQPTID", typeof(string));
                        IN_EQP.Columns.Add("USERID", typeof(string));

                        DataRow row = IN_EQP.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["USERID"] = LoginInfo.USERID;
                        IN_EQP.Rows.Add(row);
                        #endregion

                        #region # IN_INPUT
                        DataTable IN_INPUT = indataSet.Tables.Add("IN_INPUT");
                        IN_INPUT.Columns.Add("INPUT_LOTID", typeof(string));
                        for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "CHK").Equals(1))
                            {
                                DataRow newRow = IN_INPUT.NewRow();
                                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "LOTID"));
                                IN_INPUT.Rows.Add(newRow);
                            }
                        }
                        #endregion

                        #region # IN_OUTPUT
                        DataTable IN_OUTPUT = indataSet.Tables.Add("IN_OUTPUT");
                        IN_OUTPUT.Columns.Add("OUT_CSTID", typeof(string));
                        DataRow outrow = IN_OUTPUT.NewRow();
                        outrow["OUT_CSTID"] = Util.NVC(DataTableConverter.GetValue(dgOutLotInfo.Rows[0].DataItem, "CSTID"));
                        IN_OUTPUT.Rows.Add(outrow);
                        #endregion

                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RW_START_OUT_LOT", "IN_EQP,IN_INPUT,IN_OUTPUT", "OUT_LOT", indataSet);

                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (xresult) =>
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        });
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
            _LOTID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtRWCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtRWCSTID.Text))
                    {
                        return;
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("AREAID", typeof(string));
                    RQSTDT.Columns.Add("CSTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                    dr["CSTID"] = Util.NVC(txtRWCSTID.Text);
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GET_RW_CSTID", "RQSTDT", "RSLTDT", RQSTDT);

                    Util.GridSetData(dgOutLotInfo, dtResult, FrameOperation);


                    //txtRWCSTID.Text = string.Empty;
                    //txtRWCSTID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtLOTID.Text))
                    {
                        Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                        return;
                    }

                    // 중복 체크 확인
                    if (CommonVerify.HasDataGridRow(dgInputLotInfo))
                    {
                        DataTable dt = ((DataView)dgInputLotInfo.ItemsSource).Table;
                        var queryEdit = (from t in dt.AsEnumerable()
                                         where t.Field<string>("LOTID") == txtLOTID.Text
                                         select t).ToList();

                        if (queryEdit.Any())
                        {
                            Util.MessageValidation("SFU1196");   //Lot이 이미 추가되었습니다.
                            return;
                        }
                    }

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.Columns.Add("LANGID", typeof(string));
                    RQSTDT.Columns.Add("LOTID", typeof(string));

                    DataRow dr = RQSTDT.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = Util.NVC(txtLOTID.Text);
                    RQSTDT.Rows.Add(dr);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_GET_RW_LOT", "RQSTDT", "RSLTDT", RQSTDT);

                    if (dtResult.Rows.Count != 0)
                    {
                        for (int i = 0; i < dgInputLotInfo.GetRowCount(); i++)
                        {
                            if (!Util.NVC(DataTableConverter.GetValue(dgInputLotInfo.Rows[i].DataItem, "PRODID")).Equals(dtResult.Rows[0]["PRODID"].ToString()))
                            {
                                Util.MessageValidation("SFU2929"); // 이전에 스캔한 LOT의 제품코드와 다릅니다.
                                return;
                            }
                        }
                    }

                    if (dgInputLotInfo.GetRowCount() > 0)
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgInputLotInfo.ItemsSource);
                        dtSource.Merge(dtResult);
                        Util.gridClear(dgInputLotInfo);
                        dgInputLotInfo.ItemsSource = DataTableConverter.Convert(dtSource);
                    }
                    else
                    {
                        dgInputLotInfo.ItemsSource = DataTableConverter.Convert(dtResult);
                        Util.GridSetData(dgInputLotInfo, dtResult, FrameOperation);
                    }

                    txtLOTID.Text = string.Empty;
                    txtLOTID.Focus();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInputLotInfo);
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
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetOutLot()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = _RUNLOT;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_WITH_ATTR", "INDATA", "RSLTDT", IndataTable);
                if (dtMain != null && dtMain.Rows.Count > 0)
                {
                    txtRWCSTID.Text = Util.NVC(dtMain.Rows[0]["CSTID"]);
                    KeyEventArgs e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter);
                    e.RoutedEvent = Keyboard.KeyDownEvent;
                    this.Dispatcher.BeginInvoke(new Action(() => txtRWCSTID_KeyDown(null, e)));
                }
            }
            catch (Exception ex) { }
        }
        #endregion

    }
}
