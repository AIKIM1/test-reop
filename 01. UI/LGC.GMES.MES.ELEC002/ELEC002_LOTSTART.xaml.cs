/*************************************************************************************
 Created Date : 2019.10.18
      Creator : INS 김동일K
   Decription : CWA 전극 생산실적 자동화 - 작업시작 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.18  INS 김동일K : Initial Created.
  2020.07.15  오화백K : DA_PRD_SEL_WAIT_WIP을 BR_PRD_SEL_WAIT_WIP로 변경
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

namespace LGC.GMES.MES.ELEC002
{
    /// <summary>
    /// ELEC002_LOTSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ELEC002_LOTSTART : C1Window, IWorkArea
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
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public ELEC002_LOTSTART()
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

            //_PROCID = Util.NVC(tmps[0]);
            //_EQPTID = Util.NVC(tmps[1]);
            //_EQSGID = Util.NVC(tmps[2]);
            //_RUNLOT = Util.NVC(tmps[3]);
            //_COAT_SIDE_TYPE = Util.NVC(tmps[4]);

            if (!GetLotInfo())
            {
                this.DialogResult = MessageBoxResult.Cancel;
                return;
            }
        }

        public ELEC002_LOTSTART(Dictionary<string, string> dic)
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
            this.DialogResult = MessageBoxResult.OK;
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



                //_PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                //_LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LANE_QTY").ToString();

                _LOTID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LOTID").ToString();
                _PRODID = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "PRODID").ToString();
                _LANEQTY = DataTableConverter.GetValue(dgLotInfo.Rows[idx].DataItem, "LANE_QTY").ToString();
                txtLOTID.Text = _LOTID;

                //LotSelect();                    
            }
        }

        private void txtLOTID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //LotStart(txtLOTID.Text);
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
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
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
                Indata["COATING_SIDE_TYPE_CODE"] = _COAT_SIDE_TYPE.ToString().Equals("") == true ? null : _COAT_SIDE_TYPE;
                //2020-07-15 오화백 변경
                Indata["EQPTID"] = _EQPTID;
                IndataTable.Rows.Add(Indata);
                
                //2020-07-15 오화백 변경
                // DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WAIT_WIP", "INDATA", "RSLTDT", IndataTable);
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
        #endregion
        
    }
}
