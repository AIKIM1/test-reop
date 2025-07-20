/*************************************************************************************
 Created Date : 2018.05.30
      Creator : JEONG
   Decription : 열처리 공정 작업 시작
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.30  : 이관받아서 진행
  
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
    /// ELEC001_033_LOTSTART
    /// </summary>
    public partial class ELEC001_033_LOTSTART : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID    = string.Empty;
        private string _EQPTID    = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _LOTID     = string.Empty;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
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
        public ELEC001_033_LOTSTART()
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
            listAuth.Add(btnLotStart);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
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

        public ELEC001_033_LOTSTART(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("LOTID"))  _LOTID  = dicParam["LOTID"];
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0 || string.IsNullOrEmpty(_LOTID))
            {
                Util.MessageValidation("SFU1381");  //LOT을 선택하세요.
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
                        DataTable inData = indataSet.Tables.Add("IN_EQP");
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("IFMODE", typeof(string));
                        inData.Columns.Add("EQPTID", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["IFMODE"] = IFMODE.IFMODE_OFF;
                        row["EQPTID"] = _EQPTID;
                        row["USERID"] = LoginInfo.USERID;


                        inData.Rows.Add(row);

                        DataTable ininput = indataSet.Tables.Add("IN_INPUT");
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                        ininput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string)); //투입 코터lot의 proid
                        ininput.Columns.Add("INPUT_LOTID", typeof(string));


                        ////투입자재 장착위치 가져오기
                        DataTable dt = new DataTable();
                        dt.Columns.Add("EQPTID", typeof(string));

                        DataRow tmprow = dt.NewRow();
                        tmprow["EQPTID"] = _EQPTID;
                        dt.Rows.Add(tmprow);

                        DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "INDATA", "RSLTDT", dt);
                        if (EQPT_PSTN_ID == null || EQPT_PSTN_ID.Rows.Count <= 0)
                        {
                            Util.MessageValidation("SFU1398");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                            return;
                        }

                        row = ininput.NewRow();
                        row["EQPT_MOUNT_PSTN_ID"] = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"];
                        row["EQPT_MOUNT_PSTN_STATE"] = "A";
                        row["INPUT_LOTID"] = _LOTID;
                        ininput.Rows.Add(row);

                        try
                        {
                            DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_HT_UI", "IN_EQP,IN_INPUT", "RSLTDT", indataSet);
                            //정상처리되었습니다.
                            Util.MessageInfo("SFU1275", (mResult) =>
                            {
                                this.DialogResult = MessageBoxResult.OK;
                            });
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }

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
                txtLotID.Text = _LOTID;
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SelectRowByLotID();
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
                    _PRODID = dtMain.Rows[0]["PRODID"].ToString();
                    txtLotID.Text = _LOTID;

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
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("WIPSTAT", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["PROCID"] = _PROCID;
                Indata["EQSGID"] = _EQSGID;
                Indata["WIPSTAT"] = Wip_State.WAIT;
                Indata["EQPTID"] = _EQPTID;
                Indata["PRODID"] = _PRODID;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_HT", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgLotInfo, dtMain, FrameOperation);

                string[] sColumnName = new string[] { "CUT_ID" };
                _Util.SetDataGridMergeExtensionCol(dgLotInfo, sColumnName, DataGridMergeMode.VERTICALHIERARCHI);
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
                // 그리드에 일치하는 lot 자동선택
                DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
                int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLotID.Text + "'").FirstOrDefault());
                int cIdx = dgLotInfo.Columns["CHK"].Index;

                if (rIdx >= 0)
                {
                    foreach (DataRow row in dt.Rows)
                        row[cIdx] = false;

                    dt.Rows[rIdx][cIdx] = true;
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                    dgLotInfo.SelectedIndex = rIdx;
                    dgLotInfo.UpdateLayout();
                    dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);
                }
                else
                {
                    txtLotID.Text = _LOTID;
                    txtLotID.Focus();
                    txtLotID.SelectAll();
                }
            }
            catch (Exception ex) {}
        }
        #endregion
    }
}
