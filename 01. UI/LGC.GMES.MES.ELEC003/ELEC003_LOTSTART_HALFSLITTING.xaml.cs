/*************************************************************************************
 Created Date : 2021.02.19
      Creator : 정문교
   Decription : Half Slitting 작업시작
--------------------------------------------------------------------------------------
 [Change History]
  2021.02.19  : Initial Created.   기존 전극 작업시작 Copy
  2021.07.19  조영대 : 자동물류 옵션 처리 수정.
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
    /// <summary>
    /// ELEC003_LOTSTART
    /// </summary>
    public partial class ELEC003_LOTSTART_HALFSLITTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _EQPTID = string.Empty;

        private string _LDR_LOT_IDENT_BAS_CODE = string.Empty;
        private string _eqptMountPositionCode = string.Empty;

        Util _Util = new Util();

        private string _LOTID = string.Empty;
        private string _PRODID = string.Empty;
        private string _LANEQTY = string.Empty;
        private string _CSTID = string.Empty;

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
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }
        public ELEC003_LOTSTART_HALFSLITTING()
        {
            InitializeComponent();
            ApplyPermissions();
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
            else
            {
                txtLotID.Focus();
            }
        }

        public ELEC003_LOTSTART_HALFSLITTING(Dictionary<string, string> dic)
        {
            InitializeComponent();
            try
            {
                dicParam = dic;

                if (dicParam.ContainsKey("PROCID")) _PROCID = dicParam["PROCID"];
                if (dicParam.ContainsKey("EQSGID")) _EQSGID = dicParam["EQSGID"];
                if (dicParam.ContainsKey("EQPTID")) _EQPTID = dicParam["EQPTID"];

                SetIdentInfo();

                dgLotInfo.Columns["CSTID"].Visibility = Visibility.Collapsed;
                lblCarrierID_L.Visibility = Visibility.Collapsed;
                txtCarrierID_L.Visibility = Visibility.Collapsed;
                lblCarrierID_R.Visibility = Visibility.Collapsed;
                txtCarrierID_R.Visibility = Visibility.Collapsed;
                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    dgLotInfo.Columns["CSTID"].Visibility = Visibility.Visible;
                    lblCarrierID_L.Visibility = Visibility.Visible;
                    txtCarrierID_L.Visibility = Visibility.Visible;
                    lblCarrierID_R.Visibility = Visibility.Visible;
                    txtCarrierID_R.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnLotStart_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationLotStart()) return;

            // 작업시작 하시겠습니까?
            Util.MessageConfirm("SFU1240", (sresult) =>
            {
                if (sresult == MessageBoxResult.OK)
                {
                    LotStart();
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
                _CSTID = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "CSTID")).ToString();

                txtLotID.Text = _LOTID;
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

        private void txtLotID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //LotStart(txtLOTID.Text);
        }
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtLotID.Text) || dgLotInfo.GetRowCount() == 0)
            {
                return;
            }

            if (e.Key == Key.Enter)
            {
                LotSelect();
            }
        }

        private void LotSelect()
        {
            // 그리드에 일치하는 lot 자동선택
            DataTable dt = DataTableConverter.Convert(dgLotInfo.ItemsSource);
            int rIdx = dt.Rows.IndexOf(dt.Select("LOTID = '" + txtLotID.Text + "'").FirstOrDefault());

            if (rIdx >= 0)
            {
                dt.Select().ToList<DataRow>().ForEach(r => r["CHK"] = false);
                dt.Rows[rIdx]["CHK"] = true;

                dgLotInfo.ItemsSource = DataTableConverter.Convert(dt);

                dgLotInfo.SelectedIndex = rIdx;
                dgLotInfo.UpdateLayout();
                dgLotInfo.ScrollIntoView(dgLotInfo.SelectedIndex, 0);

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "LANE_QTY").ToString();
                _CSTID = DataTableConverter.GetValue(dgLotInfo.Rows[rIdx].DataItem, "CSTID").ToString();
            }
        }
        #endregion

        #region Mehod

        #region [Func]

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
                    //txtWorkorder.Text = dtMain.Rows[0]["WO_DETL_ID"].ToString();
                    txtWorkorder.Text = dtMain.Rows[0]["WOID"].ToString();

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
                //IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                IndataTable.Columns.Add("COATING_SIDE_TYPE_CODE", typeof(string));
                //2020-07-15 오화백 변경
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = Util.NVC(txtWorkorder.Text);
                //2020-07-15 오화백 변경
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);

                //2020-07-15 오화백 변경
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count <= 0 || dtMain == null)
                {
                    dgLotInfo.ItemsSource = null;
                    return;
                }

                Util.GridSetData(dgLotInfo, dtMain, null, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private bool GetMountPSTN()
        {
            try
            {
                _eqptMountPositionCode = string.Empty;

                DataTable eqpt_mount = new DataTable();
                eqpt_mount.Columns.Add("EQPTID", typeof(string));

                DataRow eqpt_mount_row = eqpt_mount.NewRow();
                eqpt_mount_row["EQPTID"] = _EQPTID;
                eqpt_mount.Rows.Add(eqpt_mount_row);

                DataTable EQPT_PSTN_ID = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_MOUNT_PSTN_ID", "RQSTDT", "RSLTDT", eqpt_mount);

                if (EQPT_PSTN_ID.Rows.Count > 0)
                {
                    _eqptMountPositionCode = EQPT_PSTN_ID.Rows[0]["EQPT_MOUNT_PSTN_ID"].ToString();
                }

                if (string.IsNullOrWhiteSpace(_eqptMountPositionCode))
                    return false;
                else
                    return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void LotStart()
        {
            try
            {
                DataSet inDataSet = new DataSet();
                DataTable inEqp = inDataSet.Tables.Add("IN_EQP");
                inEqp.Columns.Add("SRCTYPE", typeof(string));
                inEqp.Columns.Add("IFMODE", typeof(string));
                inEqp.Columns.Add("EQPTID", typeof(string));
                inEqp.Columns.Add("USERID", typeof(string));

                DataTable InInput = inDataSet.Tables.Add("IN_INPUT");
                InInput.Columns.Add("INPUT_LOTID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
                InInput.Columns.Add("EQPT_MOUNT_PSTN_STATE", typeof(string));

                DataTable InOutput = inDataSet.Tables.Add("IN_OUTPUT");
                InOutput.Columns.Add("CSTID", typeof(string));
                //////////////////////////////////////////////////////////////////////////////////////////////
                DataRow newRow = inEqp.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPTID;
                newRow["USERID"] = LoginInfo.USERID;
                inEqp.Rows.Add(newRow);

                newRow = InInput.NewRow();

                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                    newRow["INPUT_LOTID"] = _CSTID;
                else
                    newRow["INPUT_LOTID"] = txtLotID.Text;

                newRow["EQPT_MOUNT_PSTN_ID"] = _eqptMountPositionCode;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";
                InInput.Rows.Add(newRow);

                if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
                {
                    newRow = InOutput.NewRow();
                    newRow["CSTID"] = txtCarrierID_L.Text;
                    InOutput.Rows.Add(newRow);

                    newRow = InOutput.NewRow();
                    newRow["CSTID"] = txtCarrierID_R.Text;
                    InOutput.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_START_LOT_HS_EIF", "IN_EQP,IN_INPUT,IN_OUTPUT", "RSLTDT", (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidationLotStart()
        {
            if (string.IsNullOrWhiteSpace(txtLotID.Text))
            {
                Util.MessageValidation("SFU1129");  //투입된 LOT 을 선택하세요
                return false;
            }

            if (_LDR_LOT_IDENT_BAS_CODE == "CST_ID" || _LDR_LOT_IDENT_BAS_CODE == "RF_ID")
            {
                if (string.IsNullOrWhiteSpace(txtCarrierID_L.Text) || string.IsNullOrWhiteSpace(txtCarrierID_R.Text))
                {
                    Util.MessageValidation("SFU7006"); // Carrier ID를 입력하세요.
                    return false;
                }
            }

            if (!GetMountPSTN())
            {
                Util.MessageValidation("SFU1397");  //MMD에 설비 투입 위치를 입력해 주세요.
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion


    }
}