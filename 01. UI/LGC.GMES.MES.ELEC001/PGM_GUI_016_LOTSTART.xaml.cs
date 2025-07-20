/*************************************************************************************
 Created Date :                       
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2020.07.15  오화백K : DA_PRD_SEL_WAIT_WIP_RP를 BR_PRD_SEL_WAIT_WIP_RP로 변경
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

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>  
    /// PGM_GUI_016_LOTSTART
    /// </summary>
    public partial class PGM_GUI_016_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _WORKORDER = string.Empty;
        private string _PROCID = string.Empty;
        private string _PROCNAME = string.Empty;
        private string _EQPTID = string.Empty;
        private string _EQPTNAME = string.Empty;
        private string _LOTID = string.Empty;
        private string _MTRLID = string.Empty;
        private string _EQSGID = string.Empty;

        Util _Util = new Util();
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

        Dictionary<string, string> dicParam = new Dictionary<string, string>();
        public PGM_GUI_016_LOTSTART()
        {
            InitializeComponent();
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //object[] tmps = C1WindowExtension.GetParameters(this);

                //if (tmps == null)
                //{
                //    return;
                //}

                //_PROCID = Util.NVC(tmps[0]);
                //_EQPTID = Util.NVC(tmps[1]);
                //_EQSGID = Util.NVC(tmps[2]);
                //_LOTID = Util.NVC(tmps[3]);
                //txtLotID.Text = _LOTID;

                if (!GetLotInfo())
                {
                    this.DialogResult = MessageBoxResult.Cancel;
                    return;
                }
                SelectRowByLotID();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public PGM_GUI_016_LOTSTART(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("LOTID"))   _LOTID = dicParam["LOTID"];
                txtLotID.Text = _LOTID;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            //작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DataSet indataSet = new DataSet();
                    DataTable inData = indataSet.Tables.Add("IN_EQP");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataTable ininput = indataSet.Tables.Add("IN_INPUT");
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));
                    ininput.Columns.Add("INPUT_LOTID", typeof(string));

                    DataTable eqpt_mount = new DataTable();
                    eqpt_mount.Columns.Add("EQPTID", typeof(string));

                    DataRow row = inData.NewRow();
                    row["SRCTYPE"] = "UI";
                    row["IFMODE"] = "OFF";
                    row["EQPTID"] = _EQPTID;
                    row["USERID"] = LoginInfo.USERID;
                    indataSet.Tables["IN_EQP"].Rows.Add(row);

                    DataRow eqpt_mount_row = eqpt_mount.NewRow();
                    eqpt_mount_row["EQPTID"] = _EQPTID;
                    eqpt_mount.Rows.Add(eqpt_mount_row);

                    DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);
                    if (EQPT_PSTN_ID.Rows.Count < 0)
                    {
                        Util.MessageValidation("SFU1398");  //MMD에 설비 투입를 입력해주세요.
                        return;
                    }

                    row = ininput.NewRow();
                    row["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                    row["EQPT_MOUNT_PSTN_STATE"] = "A";
                    row["INPUT_LOTID"] = _LOTID;
                    ininput.Rows.Add(row);

                    try
                    {
                        DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_TP_EIF", "IN_EQP,IN_INPUT", "RSLTDT", indataSet);
                        _LOTID = string.Empty;
                        //정상처리되었습니다.
                        Util.MessageInfo("SFU1275", (mResult) =>
                        {
                            this.DialogResult = MessageBoxResult.OK;
                            //this.Close();
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
        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("False") || (bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgLotInfo.BeginEdit();
                //    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
                //    dgLotInfo.EndEdit();
                //}

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                dgLotInfo.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                txtLotID.Text = _LOTID;
                SelectRowByLotID();

            }
        }
        #endregion

        #region Mehod
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
            //try
            //{
            //    DataTable IndataTable = new DataTable();
            //    IndataTable.Columns.Add("PROCID", typeof(string));
            //    IndataTable.Columns.Add("EQSGID", typeof(string));
            //    DataRow Indata = IndataTable.NewRow();
            //    Indata["PROCID"] = _PROCID;
            //    Indata["EQSGID"] = LoginInfo.CFG_EQSG_ID;
            //    IndataTable.Rows.Add(Indata);

            //    DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);

            //    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
            try
            {

                try
                {
                    DataTable IndataTable = new DataTable();
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("EQSGID", typeof(string));
                    IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                    //2020-07-15 오화백 변경
                    IndataTable.Columns.Add("EQPTID", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["PROCID"] = _PROCID;
                    Indata["EQSGID"] = _EQSGID;
                    Indata["WO_DETL_ID"] = txtWorkorder.Text;
                    //2020-07-15 오화백 변경
                    Indata["EQPTID"] = _EQPTID;
                    IndataTable.Rows.Add(Indata);

                    //2020-07-15 오화백 변경
                    //DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP_RP", "INDATA", "RSLTDT", IndataTable);
                    DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP_RP", "INDATA", "RSLTDT", IndataTable);

                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                //DataTable IndataTable = new DataTable();
                //IndataTable.Columns.Add("EQPTID", typeof(string));
                //IndataTable.Columns.Add("EQSGID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));

                //DataRow Indata = IndataTable.NewRow();
                //Indata["PROCID"] = _PROCID;
                //Indata["EQSGID"] = _EQSGID;
                //Indata["EQPTID"] = _EQPTID;
                //IndataTable.Rows.Add(Indata);

                //DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_LOT", "INDATA", "RSLTDT", IndataTable);
                //if (dtMain.Rows.Count <= 0 || dtMain == null)
                //{
                //    dgLotInfo.ItemsSource = null;
                //    return;
                //}

                //DataTable dt = new DataTable();
                //dt.Columns.Add("CHK", typeof(bool));
                //dt.Columns.Add("LOTID", typeof(string));
                //dt.Columns.Add("WIPQTY", typeof(string));
                //dt.Columns.Add("PRODID", typeof(string));
                //dt.Columns.Add("PRODNAME", typeof(string));

                //DataRow row = null;
                //for (int i = 0; i < dtMain.Rows.Count; i++)
                //{
                //    row = dt.NewRow();
                //    row["CHK"] = dtMain.Rows[i]["CHK"];
                //    row["LOTID"] = dtMain.Rows[i]["PARENTLOT"];
                //    row["WIPQTY"] = dtMain.Rows[i]["WIPQTY"];
                //    row["PRODID"] = dtMain.Rows[i]["PRODID"];
                //    row["PRODNAME"] = dtMain.Rows[i]["PRODNAME"];
                //    dt.Rows.Add(row);
                //}
                //dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void SelectRowByLotID()
        {
            try
            {
                //txtLotID.Text = _LOTID;
                DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLotID.Text + "'").FirstOrDefault());
                int cIdx = dgLotInfo.Columns["CHK"].Index;
                if (rIdx >= 0)
                {
                    dt.Rows[rIdx][cIdx] = true;
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                    dgLotInfo.SelectedIndex = rIdx;
                    dgLotInfo.UpdateLayout();
                    dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);

                }
            }
            catch (Exception ex)
            {
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectRowByLotID();
            }
        }
        #endregion

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
    }
}
