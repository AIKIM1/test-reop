/*************************************************************************************
 Created Date : 2017.03.06
      Creator : 유관수
   Decription : 전지 5MEGA-GMES 구축 - 추가기능 - 믹서 자주검사 입력 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.06  유관수 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_ELEC_MIXCONFIRM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_ELEC_MIXCONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _lineCode = string.Empty;
        private string _eqptID = string.Empty;
        private string _eqptName = string.Empty;
        private string _procID = string.Empty;
        private string _lotID = string.Empty;
        private string _wipSeq = string.Empty;
        private string _shiftCode = string.Empty;
        private string _LANEQTY = string.Empty;
        public C1DataGrid COLORTAG_GRID { get; set; }

        string _CLCTSEQ = string.Empty;
        bool isChangeQuality = false;

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

        public CMM_COM_ELEC_MIXCONFIRM()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            ldpDateFrom.Text = (DateTime.Now.AddDays(-7)).ToString("yyyy-MM-dd");
            ldpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            ldpDateTo.Text = DateTime.Now.ToString("yyyy-MM-dd");
            ldpDateTo.SelectedDateTime = DateTime.Now;

            string[] sFilter2 = { _lineCode, _procID, null };
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);

            if (!string.IsNullOrEmpty(_eqptID))
                cboEquipment.SelectedValue = _eqptID;
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 5)
            {
                _lineCode = Util.NVC(tmps[0]);
                _eqptID = Util.NVC(tmps[1]);
                _procID = Util.NVC(tmps[2]);
                _lotID = Util.NVC(tmps[3]);
                _wipSeq = Util.NVC(tmps[4]);
                _eqptName = Util.NVC(tmps[5]);

                txtEqptId.Text = _eqptID;
                txtEqptName.Text = _eqptName;
            }

            ApplyPermissions();
            InitializeControls();
            SelectMixConfirmList();
            Loaded -= C1Window_Loaded;

            if (string.Equals(_procID, Process.ROLL_PRESSING) || string.Equals(_procID, Process.COATING))
            {
                txtEqptId.Visibility = Visibility.Hidden;
                txtEqptName.Visibility = Visibility.Hidden;

                cboEquipment.Visibility = Visibility.Visible;
                txbLotid.Visibility = Visibility.Visible;
                txtLotID.Visibility = Visibility.Visible;

                if (string.Equals(_procID, Process.ROLL_PRESSING))
                {
                    dgProduct.Columns["WIPSEQ"].Visibility = Visibility.Visible;
                    dgQuality.Columns["WIPSEQ"].Visibility = Visibility.Visible;
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod
        private void SelectMixConfirmList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgProduct);

                string bizRuleName;

                DataTable IndataTable = new DataTable();

                if (string.Equals(_procID, Process.ROLL_PRESSING) || string.Equals(_procID, Process.COATING))
                {
                    bizRuleName = "DA_PRD_SEL_WIP_CONFIRM";
                    
                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("LOTID", typeof(string));
                    IndataTable.Columns.Add("WIPSTAT", typeof(string));
                    IndataTable.Columns.Add("FROM_DATE", typeof(string));
                    IndataTable.Columns.Add("TO_DATE", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["PROCID"] = _procID;

                    if (cboEquipment.SelectedIndex > 0)
                        Indata["EQPTID"] = cboEquipment.SelectedValue.ToString();

                    if (!string.IsNullOrEmpty(txtLotID.Text))
                        Indata["LOTID"] = txtLotID.Text;

                    Indata["FROM_DATE"] = Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["TO_DATE"] = Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["WIPSTAT"] = ""; //Wip_State.EQPT_END;

                    IndataTable.Rows.Add(Indata);
                }
                else
                {
                    bizRuleName = "DA_PRD_SEL_WIP_MX_CONFIRM";

                    IndataTable.Columns.Add("LANGID", typeof(string));
                    IndataTable.Columns.Add("PROCID", typeof(string));
                    IndataTable.Columns.Add("EQPTID", typeof(string));
                    IndataTable.Columns.Add("WIPSTAT", typeof(string));
                    IndataTable.Columns.Add("FROM_DATE", typeof(string));
                    IndataTable.Columns.Add("TO_DATE", typeof(string));

                    DataRow Indata = IndataTable.NewRow();
                    Indata["LANGID"] = LoginInfo.LANGID;
                    Indata["PROCID"] = _procID;
                    Indata["EQPTID"] = _eqptID;
                    Indata["FROM_DATE"] = Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["TO_DATE"] = Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd");
                    Indata["WIPSTAT"] = ""; //Wip_State.EQPT_END;

                    IndataTable.Rows.Add(Indata);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    dgProduct.ItemsSource = DataTableConverter.Convert(result);
                    Util.GridSetData(dgProduct, result, null, true);
                    dgQuality.Refresh(true);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            //List<Button> listAuth = new List<Button> { btnSave, btnDelete, btnAdd };
            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        protected virtual void OnClickSaveQuality(object sender, RoutedEventArgs e)
        {
            if (!isChangeQuality)
            {
                return;
            }
            if (string.Equals(_procID, Process.MIXING) || string.Equals(_procID, Process.ROLL_PRESSING) || string.Equals(_procID, Process.COATING) ||
                string.Equals(_procID, Process.CMC) || string.Equals(_procID, Process.BS) || string.Equals(_procID, Process.PRE_MIXING) // 2024.10.11. 김영국 - CMC, Binder, PreMix 공정 추가
                )
            {
                // 20204.10.11. 김영국 - Data 저장 시 Question 추가함.
                //저장 하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveQuality(dgQuality);
                    }
                });
            }
        }

        private void SaveQuality(C1DataGrid dg)
        {
            if (dg.Rows.Count < 1)
                return;

            DataTable inEDCLot = dtDataCollectOfChildQuality(dg);

            var bizName = "BR_QCA_REG_WIP_DATA_CLCT_MIX";

            new ClientProxy().ExecuteService(bizName, "INDATA", null, inEDCLot, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.AlertByBiz(bizName, ex.Message, ex.ToString());
                    return;
                }

                isChangeQuality = false;
                Util.MessageInfo("SFU1998");  //품질 정보가 저장되었습니다.
                Util.gridClear(dgQuality);
                dgQuality.Refresh(true);
                SelectMixConfirmList();
            });
        }

        private DataTable dtDataCollectOfChildQuality(C1DataGrid dg)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));
            IndataTable.Columns.Add("CLCTVAL01", typeof(string));
            IndataTable.Columns.Add("WIPSEQ", typeof(string));
            IndataTable.Columns.Add("CLCTSEQ", typeof(string));

            DataTable dt = (dg.ItemsSource as DataView).Table;
            DataRow inData = null;

            GetWipSeq(_lotID, string.Empty);

            foreach (DataRow _iRow in dt.Rows)
            {
                inData = IndataTable.NewRow();

                inData["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inData["LOTID"] = _lotID;
                inData["EQPTID"] = _eqptID;
                inData["USERID"] = LoginInfo.USERID;
                inData["CLCTITEM"] = _iRow["CLCTITEM"];

                if (string.Equals(_procID, Process.ROLL_PRESSING) || string.Equals(_procID, Process.COATING))
                {
                    inData["CLCTVAL01"] = _iRow["CLCTVAL01"];

                    if (string.Equals(_procID, Process.ROLL_PRESSING))
                        inData["WIPSEQ"] = _iRow["WIPSEQ"];
                    else
                        inData["WIPSEQ"] = string.IsNullOrEmpty(_wipSeq) ? null : _wipSeq;
                }
                else
                {
                    inData["CLCTVAL01"] = _iRow["CLCTVAL01"].ToString().Equals("") ? 0 : _iRow["CLCTVAL01"];
                    inData["WIPSEQ"] = string.IsNullOrEmpty(_wipSeq) ? null : _wipSeq;
                }

                inData["CLCTSEQ"] = string.IsNullOrEmpty(_CLCTSEQ) ? null : _CLCTSEQ;
                IndataTable.Rows.Add(inData);
            }

            return IndataTable;
        }

        private void GetWipSeq(string sLotID, string sCLCTITEM)
        {
            _wipSeq = string.Empty;
            _CLCTSEQ = string.Empty;

            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LOTID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("CLCTITEM", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LOTID"] = sLotID;
            Indata["PROCID"] = _procID;
            Indata["CLCTITEM"] = sCLCTITEM;
            IndataTable.Rows.Add(Indata);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIPDATACOLLECT_WIPSEQ_MIX", "INDATA", "RSLTDT", IndataTable);

            if (dtResult.Rows.Count == 0)
            {
                _wipSeq = string.Empty;
                _CLCTSEQ = string.Empty;
            }
            else
            {
                _wipSeq = dtResult.Rows[0]["WIPSEQ"].ToString();
                _CLCTSEQ = dtResult.Rows[0]["CLCTSEQ"].ToString();
            }
        }

        private void ldpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }

            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        private void ldpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ldpDateFrom.Text == string.Empty || ldpDateTo.Text == string.Empty || ldpDateFrom.Text == null || ldpDateTo.Text == null)
            {
                return;
            }
            if (Convert.ToDecimal(Convert.ToDateTime(ldpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(ldpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
            {
                Util.MessageValidation("SFU1913");  //종료일자가 시작일자보다 빠릅니다.
                return;
            }
        }

        void GetQualityList(object SelectedItem)
        {
            try
            {
                DataTable _topDT = new DataTable();
                DataTable _backDT = new DataTable();

                DataRowView rowview = SelectedItem as DataRowView;

                if (rowview == null)
                    return;

                Util.gridClear(dgQuality);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("WIPSEQ", typeof(string));
                IndataTable.Columns.Add("CLCT_PONT_CODE", typeof(string));
                IndataTable.Columns.Add("LANEQTY", typeof(Int16));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["PROCID"] = _procID;
                Indata["LOTID"] = rowview["LOTID"].ToString();
                Indata["WIPSEQ"] = rowview["WIPSEQ"].ToString();

                IndataTable.Rows.Add(Indata);
                _lotID = rowview["LOTID"].ToString();
                _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_WIPDATACOLLECT_LOT_MIX", "INDATA", "RSLTDT", IndataTable);

                if (_topDT.Rows.Count == 0)
                {
                    _topDT = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_PROC_CLCTITEM_MIX", "INDATA", "RSLTDT", IndataTable);
                    Util.GridSetData(dgQuality, _topDT, FrameOperation, true);
                }
                else
                {
                    Util.GridSetData(dgQuality, _topDT, FrameOperation, true);
                }
                //dgQuality.Refresh(true);
                isChangeQuality = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectMixConfirmList();
            Util.gridClear(dgQuality);
        }

        private void dgQuality_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            isChangeQuality = true;
        }

        private void dgQuality_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            isChangeQuality = true;
        }

        private void dgProduct_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender == null)
                return;
            DataRowView drv = dgProduct.CurrentRow.DataItem as DataRowView;
            C1DataGrid dg = (sender as C1DataGrid);

            if (dg == null || dg.CurrentCell == null || dg.CurrentCell.Row == null)
                return;

            int idx = dg.CurrentCell.Row.Index;
            if (idx < 0)
                return;

            if ((dg.CurrentCell.Column.Index == dgProduct.Columns["CHK"].Index) && (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK") != null))
            {
                if (DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK").ToString().Equals("1") || 
                    DataTableConverter.GetValue(dg.Rows[idx].DataItem, "CHK").ToString().Equals("True")) // 2024.10.17. 김영국 - DB상에서 CHK값이 Ture또는 False로 올라오는 문제점이 있어 코드 추가해서 대응.
                {
                    dgProduct.SelectedIndex = idx;
                    DataTableConverter.SetValue(dgProduct.Rows[idx].DataItem, "CHK", true);

                    if (string.Equals(_procID, Process.ROLL_PRESSING) || string.Equals(_procID, Process.COATING))
                        _eqptID = drv.DataView.Table.Rows[0]["EQPTID"].ToString();

                    GetQualityList(drv);
                }
                else
                {
                    DataTableConverter.SetValue(dgProduct.Rows[idx].DataItem, "CHK", false);
                }
            }
            for (int i = 0; dgProduct.Rows.Count > i; i++)
            {
                if(i != idx)
                {
                    DataTableConverter.SetValue(dgProduct.Rows[i].DataItem, "CHK", false);
                }

            }
                dgProduct.Refresh(true);
                //dgQuality.Refresh(true);
        }
    }
}