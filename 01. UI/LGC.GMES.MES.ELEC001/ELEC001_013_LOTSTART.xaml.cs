/******************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 작업시작 대기 Lot List 조회
-------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2020.07.15  오화백K : DA_PRD_SEL_WAIT_WIP_RP를 BR_PRD_SEL_WAIT_WIP_RP로 변경
*******************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    /// <summary>
    /// ELEC001_LOTSTART
    /// </summary>
    public partial class ELEC001_013_LOTSTART : C1Window, IWorkArea
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

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        #endregion

        #region Initialize
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC001_013_LOTSTART()
        {
            InitializeComponent();
        }

        public ELEC001_013_LOTSTART(string strPROCID, string strEQSGID, string strEQPTID, string strLOTID)
        {
            _PROCID = strPROCID;
            _EQPTID = strEQPTID;
            _EQSGID = strEQSGID;
            _LOTID = strLOTID;

            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            txtLotID.Text = _LOTID;

            if (!GetLotInfo())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }

            
            SelectRowByLotID();
        }
        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {

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
                    row["USERID"] = LoginInfo.USERID;
                    row["EQPTID"] = _EQPTID;
                    indataSet.Tables["IN_EQP"].Rows.Add(row);

                    DataTable inMtrl = indataSet.Tables.Add("IN_INPUT");
                    inMtrl.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                    inMtrl.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string)); //투입 코터lot의 proid
                    inMtrl.Columns.Add("INPUT_LOTID", typeof(string));

                    DataTable dt = new DataTable();
                    dt.Columns.Add("EQPTID", typeof(string));

                    DataRow tmprow = dt.NewRow();
                    tmprow["EQPTID"] = _EQPTID;
                    dt.Rows.Add(tmprow);

                    DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_CURR_MOUNT_MTRL", "INDATA", "RSLTDT", dt);

                    if (dtResult == null || dtResult.Rows.Count <= 0)
                    {
                        Util.AlertInfo("SFU2019");  //해당 설비의 자재투입부를 MMD에서 입력해주세요.
                        return;
                    }

                    row = inMtrl.NewRow();
                    row["EQPT_MOUNT_PSTN_ID"] = dtResult.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                    row["EQPT_MOUNT_PSTN_STATE"] = "A";
                    row["INPUT_LOTID"] = _LOTID;
                    inMtrl.Rows.Add(row);

                    DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_START_LOT_BW_EIF", "IN_EQP,IN_INPUT", "RSLTDT", indataSet);
                    //Util.AlertInfo("SFU1835");  //작업 시작
                    #region RTLS (2동 Only)
                    /*
                    if (LoginInfo.CFG_AREA_ID == "E6")
                    {
                        DataTable IndataTable = new DataTable("INDATA");
                        IndataTable.Columns.Add("CONDITION", typeof(string));
                        IndataTable.Columns.Add("CART_NO", typeof(string));
                        IndataTable.Columns.Add("LOTID", typeof(string));
                        IndataTable.Columns.Add("USERID", typeof(string));
                        IndataTable.Columns.Add("ZONE_ID", typeof(string));

                        DataRow Indata = IndataTable.NewRow();
                        Indata["CONDITION"] = "INPUT_LOT";
                        Indata["CART_NO"] = "";
                        Indata["LOTID"] = _LOTID;
                        Indata["USERID"] = "MES";
                        Indata["ZONE_ID"] = "";
                        IndataTable.Rows.Add(Indata);

                        //DataTable dtRTLS = new ClientProxy().ExecuteServiceSync("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, IndataTable);

                        new ClientProxy().ExecuteService("BR_RTLS_REG_MAPPING_BY_CONDITION", "INDATA", null, IndataTable, (dtRTLS, searchException) =>
                        {
                            if (searchException != null)
                            {
                                throw searchException;
                            }
                        });

                    }
                    */
                    #endregion
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
                        //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1838") + "\n" + ex.ToString(), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning); //작업시작 정보 확인
                        // LGC.GMES.MES.ControlsLibrary.MessageBox.ShowKeyEnter(MessageDic.Instance.GetMessage("LOT 착공 오류"), ex.Message, "Info", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    }
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void dgWaitListChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);

                dgWaitList.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgWaitList.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgWaitList.Rows[idx].DataItem, "PRODID").ToString();
            }
        }
        #endregion

        #region Mehod
        private void SelectRowByLotID()
        {
            try
            {
                // 그리드에 일치하는 lot 자동선택
                DataTable dt = DataTableConverter.Convert(dgWaitList.ItemsSource);
                int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLotID.Text + "'").FirstOrDefault());
                int cIdx = dgWaitList.Columns["CHK"].Index;

                if (rIdx >= 0)
                {
                    dt.Rows[rIdx][cIdx] = true;
                    dgWaitList.ItemsSource = DataTableConverter.Convert(dt);

                    dgWaitList.SelectedIndex = rIdx;
                    dgWaitList.UpdateLayout();
                    dgWaitList.ScrollIntoView(dgWaitList.SelectedIndex, 0);

                    _LOTID = DataTableConverter.GetValue(dgWaitList.Rows[rIdx].DataItem, "LOTID").ToString();
                    _MTRLID = DataTableConverter.GetValue(dgWaitList.Rows[rIdx].DataItem, "PRODID").ToString();
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
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

                dgWaitList.ItemsSource = DataTableConverter.Convert(dtMain);
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

                //dgWaitList.ItemsSource = DataTableConverter.Convert(dtMain);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void dgLotInfoChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            string tmp = (rb.DataContext as DataRowView).Row["CHK"].ToString();

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                for (int i = 0; i < dg.GetRowCount(); i++)
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);

                dgWaitList.SelectedIndex = idx;

                _LOTID = DataTableConverter.GetValue(dgWaitList.Rows[idx].DataItem, "LOTID").ToString();
                _MTRLID = DataTableConverter.GetValue(dgWaitList.Rows[idx].DataItem, "PRODID").ToString();

                txtLotID.Text = _LOTID;
                SelectRowByLotID();
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SelectRowByLotID();
        }
    }
}