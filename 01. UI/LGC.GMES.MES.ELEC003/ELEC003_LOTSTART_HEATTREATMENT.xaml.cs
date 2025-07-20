/*************************************************************************************
 Created Date : 2021.08.09
      Creator : 조영대
   Decription : 작업시작 대기 Lot List - Heat Treatment(E4600)
--------------------------------------------------------------------------------------
 [Change History]
 2021.08.09  조영대 : Initial Created.
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

namespace LGC.GMES.MES.ELEC003
{
    public partial class ELEC003_LOTSTART_HEATTREATMENT : C1Window, IWorkArea
    {
        #region Declaration & Constructor

        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _LANEQTY = string.Empty;
        private string _RUNLOT = string.Empty;  // 작업시작 Lot
        private string _COAT_SIDE_TYPE = string.Empty;
        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }

        public string _ReturnProdID
        {
            get { return _PRODID; }
        }

        public string _ReturnLaneQty
        {
            get { return _LANEQTY; }
        }

        Dictionary<string, string> dicParam = new Dictionary<string, string>();

        #endregion

        #region Initialize

        public IFrameOperation FrameOperation { get; set; }

        public ELEC003_LOTSTART_HEATTREATMENT()
        {
            InitializeComponent();

            ApplyPermissions();
        }

        public ELEC003_LOTSTART_HEATTREATMENT(Dictionary<string, string> dic)
        {
            InitializeComponent();

            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("RUNLOT")) _RUNLOT = dicParam["RUNLOT"];
                if (dicParam.ContainsKey("COAT_SIDE_TYPE")) _COAT_SIDE_TYPE = dicParam["COAT_SIDE_TYPE"];

                SetIdentInfo();
                dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!GetLotInfo())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }
        }

        private void SetIdentInfo()
        {
            try
            {
                if (string.IsNullOrEmpty(_PROCID) || string.IsNullOrEmpty(_EQSGID))
                {
                    _LDR_LOT_IDENT_BAS_CODE = "";
                    return;
                }

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = _PROCID;
                row["EQSGID"] = _EQSGID;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_LOT_IDENT_BAS_CODE", "INDATA", "RSLTDT", dt);

                if (result.Rows.Count > 0)
                {
                    _LDR_LOT_IDENT_BAS_CODE = result.Rows[0]["LDR_LOT_IDENT_BAS_CODE"].ToString();
                }
            }
            catch (Exception ex) { Util.MessageException(ex); };
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
                return;
            }

            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    string eqptMountPositionCode = string.Empty;

                    DataTable eqpt_mount = new DataTable();
                    eqpt_mount.Columns.Add("EQPTID", typeof(string));

                    DataRow eqpt_mount_row = eqpt_mount.NewRow();
                    eqpt_mount_row["EQPTID"] = _EQPTID;
                    eqpt_mount.Rows.Add(eqpt_mount_row);

                    DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                    if (EQPT_PSTN_ID.Rows.Count > 0)
                    {
                        eqptMountPositionCode = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                    }
                    else
                    {
                        Util.MessageValidation("SFU1397");  //MMD에 설비 투입 위치를 입력해 주세요.
                        return;
                    }

                    DataSet inDataSet = new DataSet();

                    #region MESSAGE SET
                    DataTable inLotDataTable = inDataSet.Tables.Add("IN_EQP");
                    inLotDataTable.Columns.Add("SRCTYPE", typeof(string));
                    inLotDataTable.Columns.Add("IFMODE", typeof(string));
                    inLotDataTable.Columns.Add("EQPTID", typeof(string));
                    inLotDataTable.Columns.Add("USERID", typeof(string));

                    DataRow inLotDataRow = inLotDataTable.NewRow();
                    inLotDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    inLotDataRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    inLotDataRow["EQPTID"] = _EQPTID;
                    inLotDataRow["USERID"] = LoginInfo.USERID;
                    inLotDataTable.Rows.Add(inLotDataRow);

                    DataTable InMtrldataTable = inDataSet.Tables.Add("IN_INPUT");
                    InMtrldataTable.Columns.Add("INPUT_LOTID", typeof(string));
                    InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    InMtrldataTable.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                    DataRow inMtrlDataRow = InMtrldataTable.NewRow();
                    inMtrlDataRow["INPUT_LOTID"] = _LOTID;
                    inMtrlDataRow["EQPT_MOUNT_PSTN_ID"] = eqptMountPositionCode;
                    inMtrlDataRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                    InMtrldataTable.Rows.Add(inMtrlDataRow);
                    #endregion

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_HT_UI", "IN_EQP,IN_INPUT", "RSLTDT", (result, ex) =>
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (mResult) =>
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        });

                    }, inDataSet);

                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;

            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked || (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                if (dg != null)
                {
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (idx == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LANE_QTY").ToString();
                txtLOTID.Text = _LOTID;
            }
        }
        
        private void txtLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLOTID.Text) || dgLotInfo.GetRowCount() == 0)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                LotSelect();
            }
        }

        private void dgLotInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender == null)
                return;

            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;
            dgLotInfo.SelectedIndex = idx;

            DataTableConverter.SetValue(dg.Rows[idx].DataItem, "CHK", true);
        }
        
        #endregion

        #region Mehod

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnLotStart);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private bool GetLotInfo()
        {
            try
            {
                bool rslt = false;
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EIO_WORKORDER", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count > 0)
                {
                    txtEquipment.Text = dtMain.Rows[0]["EQPTNAME"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                    txtWOID.Text = dtMain.Rows[0]["WOID"].ToString();
                    GetLotList();
                    rslt = true;
                }
                else
                {
                    Util.MessageValidation("SFU1436");  //W/O 선택 후 작업시작하세요
                    rslt = false;
                }
                return rslt;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                IndataTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = Util.NVC(txtWorkorder.Text);
                Indata["COATING_SIDE_TYPE_CODE"] = _COAT_SIDE_TYPE.ToString().Equals("") == true ? null : _COAT_SIDE_TYPE;
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count <= 0 || dtMain == null)
                {
                    dgLotInfo.ItemsSource = null;
                    return;
                }

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);

                txtLOTID.Text = _RUNLOT;
                LotSelect();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LotSelect()
        {
            // 그리드에 일치하는 lot 자동선택
            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLOTID.Text + "'").FirstOrDefault());
            int cIdx = dgLotInfo.Columns["CHK"].Index;

            if (rIdx >= 0)
            {
                dt.Rows[rIdx][cIdx] = true;
                dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                dgLotInfo.SelectedIndex = rIdx;
                dgLotInfo.UpdateLayout();
                dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LANE_QTY").ToString();


            }
        }
        
        #endregion

    }
}