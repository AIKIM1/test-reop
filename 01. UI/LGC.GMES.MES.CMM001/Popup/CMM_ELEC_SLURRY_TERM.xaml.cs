/*************************************************************************************
 Created Date : 2016.08.19
      Creator : 
   Decription : 작업시작 대기 Lot List 조회
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.19  : Initial Created.
  2017.01.12  : CMM으로 SLURRY 팝업 이동
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using C1.WPF.DataGrid;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_SLURRY_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SLURRY_TERM : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _PRODID = string.Empty;
        private string _PRDT_CLSS_CODE = string.Empty;
        private string _EQSGID = string.Empty;
        private string _WOID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _xEQPTID = string.Empty;
        private string _SLURRYID = string.Empty;
        private string _EQPT_MOUNT_PSTN_ID = string.Empty;
        private int _POSITION = -1;

        private bool isSingleCoater = false;
        private bool isAllConfirm = false;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }
        public string _ReturnCLSSCODE
        {
            get { return _PRDT_CLSS_CODE; }
        }
        public string _ReturnPRODID
        {
            get { return _PRODID; }
        }
        public int _ReturnPosition
        {
            get { return _POSITION; }
        }
        public bool _IsAllConfirm
        {
            get { return isAllConfirm; }
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
        public CMM_ELEC_SLURRY_TERM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                return;
            }

            _PROCID = Util.NVC(tmps[0]);
            _EQSGID = Util.NVC(tmps[1]);
            _WOID = Util.NVC(tmps[2]);

            // EQPTID 추가
            _EQPTID = Util.NVC(tmps[3]);

            // 이전 POSITION정보 기억용
            _POSITION = Util.NVC_Int(tmps[4]);

            // SLURRY 코터 적용 방식 변경으로 인하여 추가 (2017-02-11)
            _SLURRYID = Util.NVC(tmps[5]);
            isSingleCoater = string.Equals(Util.NVC(tmps[6]), "Y") ? true : false;  // 단면코터 확인

            //if (isSingleCoater)
                //chkCoater.Visibility = Visibility.Collapsed;

            SetMixerDefaultCombo(); // MIXER DEFAULT 추가
            GetLotList(Convert.ToString(cboEquipment.SelectedValue));
        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                return;
            }
            //선택한 자재를 탈착 처리하시겠습니까?
            Util.MessageConfirm("SFU3221", (vResult) =>
                {
                if (vResult == MessageBoxResult.OK)
                {
                    DataTable dt = ((DataView)dgLotInfo.ItemsSource).Table;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToBoolean(row["CHK"]))
                        {
                            if (!Util.NVC(row["EQPT_MOUNT_PSTN_ID"]).Equals(""))
                            {
                                SetPreSelectedMixerDesorption(row["EQPT_MOUNT_PSTN_ID"].ToString());
                            }
                        }
                    }
                    System.Threading.Thread.Sleep(1000);
                    this.DialogResult = MessageBoxResult.OK;
                }
            },new object[] { _SLURRYID, _LOTID });

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            _LOTID = string.Empty;
            _PRODID = string.Empty;
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void cboEquipment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.OldValue < 0)
                return;

            Util.gridClear(dgLotInfo);
            _xEQPTID = Convert.ToString(((System.Data.DataRowView)(cboEquipment.Items[e.NewValue])).Row.ItemArray[0]);
            GetLotList(Convert.ToString(((System.Data.DataRowView)(cboEquipment.Items[e.NewValue])).Row.ItemArray[0]));
        }
        #endregion

        #region Mehod
        private void SetMixerDefaultCombo()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQSGID"] = _EQSGID;
                Indata["EQPTID"] = _xEQPTID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMixer = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_EQUIPMENT_PRE_MIXER_INFO", "INDATA", "RSLTDT", IndataTable);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";
                cboEquipment.ItemsSource = DataTableConverter.Convert(dtMixer);

                int iSelectedIdx = 0;
                for (int i = 0; i < dtMixer.Rows.Count; i++)
                {
                    if (string.Equals(Convert.ToString(dtMixer.Rows[i]["USEFLAG"]), "Y"))
                    {
                        iSelectedIdx = i;
                        break;
                    }
                }
                cboEquipment.SelectedIndex = iSelectedIdx;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetLotList(string sMixerId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                //IndataTable.Columns.Add("EQSGID", typeof(string));
                //IndataTable.Columns.Add("PROCID", typeof(string));
                //IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                //IndataTable.Columns.Add("COT_EQPTID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                //IndataTable.Columns.Add("INPUT_LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                //Indata["EQSGID"] = _EQSGID;
                //Indata["PROCID"] = _PROCID;
                //Indata["WO_DETL_ID"] = _WOID;
                //Indata["COT_EQPTID"] = _EQPTID;

                // EQPTID 추가
                //if (!string.IsNullOrEmpty(sMixerId))
                Indata["EQPTID"] = _EQPTID;

                // 장착 SLURRY 추가
                //if (!string.IsNullOrEmpty(_SLURRYID))
                //    Indata["INPUT_LOTID"] = _SLURRYID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLURRY_WIP_MTRL_V01", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    Util.GridSetData(dgLotInfo, dtMain, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPreSelectedMixerDesorption(string sPstn)
        {
            // SET
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("SRCTYPE", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));
            IndataTable.Columns.Add("PROCID", typeof(string));
            IndataTable.Columns.Add("USERID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["SRCTYPE"] = "UI";
            Indata["EQPTID"] = _EQPTID;
            Indata["EQPT_MOUNT_PSTN_ID"] = sPstn;
            Indata["PROCID"] = _PROCID;
            Indata["USERID"] = LoginInfo.USERID;

            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("BR_PRD_REG_UNMOUNT_SLURRY_CT", "INDATA", null, IndataTable, (result, ex) =>
            {
                if (ex != null)
                {
                    Util.MessageException(ex);
                    return;
                }
            });
        }
        #endregion

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            IsChecked = false,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center

        };

        private void dgLotInfo_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            dgLotInfo.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (e.Column is C1.WPF.DataGrid.DataGridCheckBoxColumn)
                    {
                        C1.WPF.DataGrid.DataGridCell cell = (sender as C1DataGrid).GetCell(e.Row.Index, e.Column.Index);
                        CheckBox cb = cell.Presenter.Content as CheckBox;

                        int index = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }

            }));
        }

        private void dgLotInfo_LoadedRowPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            C1DataGrid dg = sender as C1DataGrid;
            if (dgLotInfo.GetRowCount() == 0)
            {
                if (e.Row.Type == DataGridRowType.Bottom)
                {
                    e.Row.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.CurrentRow.DataItem == null)
                return;

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            dgLotInfo.SelectedIndex = dgLotInfo.SelectedIndex + 1;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.CurrentRow.DataItem == null)
                return;

            //int rowIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
            dgLotInfo.SelectedIndex = dgLotInfo.SelectedIndex - 1;
        }
    }
}
