using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_TOPLOT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_TOPLOT : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _EQSGID = string.Empty;
        private string _PROCID = string.Empty;
        private string _LOTID = string.Empty;
        private string _WOID = string.Empty;
        private string _COREID = string.Empty;

        private int _POSITION = -1;
        private bool isCoreTerm = false;

        Util _Util = new Util();

        public string _ReturnLotID
        {
            get { return _LOTID; }
        }

        public int _ReturnPosition
        {
            get { return _POSITION; }
        }

        public bool _IsCoreTerm
        {
            get { return isCoreTerm; }
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
        public CMM_ELEC_TOPLOT()
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

            _EQSGID = Util.NVC(tmps[0]);
            _PROCID = Util.NVC(tmps[1]);
            _WOID = Util.NVC(tmps[2]);
            _POSITION = Util.NVC_Int(tmps[3]);
            _COREID = Util.NVC(tmps[4]);

            GetLotList();
        }
        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (dgLotInfo.SelectedIndex < 0)
            {
                Util.MessageValidation("SFU1651");    //선택된 항목이 없습니다.
                return;
            }

            if (string.Equals(_COREID, _LOTID))
            {
                Util.MessageValidation("SFU3592");
                return;
            }

            //이전 Slurry 종료 체크시 확인 안함.
            if (chkCore.Visibility == Visibility.Visible && chkCore.IsChecked == false)
            {
                this.DialogResult = MessageBoxResult.OK;
            }
            else
            {
                //사용중인 Core[{%1}]를 종료시키고, 선택한 Core[{%2}]로 장착 처리하시겠습니까?
                Util.MessageConfirm("SFU3593", (vResult) =>
                {
                    if (vResult == MessageBoxResult.OK)
                    {
                        isCoreTerm = true;

                        this.DialogResult = MessageBoxResult.OK;
                    }
                }, new object[] { _COREID, _LOTID });
            }
            //this.DialogResult = MessageBoxResult.OK;
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

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                //DataRow dtRow = (rb.DataContext as DataRowView).Row;

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
            }
        }
        #endregion

        #region Mehod
        private void GetLotList()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("EQSGID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("WO_DETL_ID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["EQSGID"] = _EQSGID;
                Indata["PROCID"] = _PROCID;
                Indata["WO_DETL_ID"] = _WOID;
                Indata["LOTID"] = _COREID;

                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SINGLE_CT_WIP_V01", "INDATA", "RSLTDT", IndataTable);
                if (dtMain.Rows.Count > 0)
                {
                    dgLotInfo.ItemsSource = DataTableConverter.Convert(dtMain);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}
