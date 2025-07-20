/*************************************************************************************
 Created Date : 2020.
      Creator : 
   Decription : 설비Trouble List
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.
  2023.09.09  조영대 : IWorkArea 추가
**************************************************************************************/
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.FCS001
{
    /// <summary>
    /// TSK_120.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS001_027_EQPT_TROUBLE_LIST : UserControl, IWorkArea
    {

        #region Declaration & Constructor 

        Util _Util = new Util();
        public FCS001_027_EQPT_TROUBLE_LIST()
        {
            InitializeComponent();

            //this.Loaded += UserControl_Loaded;
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        /// <summary>
        /// 화면 내 ComboBox Setting
        /// </summary>
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();

            string[] sFilter = { "Y" };
            _combo.SetCombo(cboLane, CommonCombo_Form.ComboStatus.ALL, sCase: "LANE", sFilter: sFilter);

            //C1ComboBox[] cboEqpKindParent = { cboEqp };
            _combo.SetCombo(cboEqp, CommonCombo_Form.ComboStatus.ALL, sCase: "EQPT_GR_TYPE_CODE");

            dtpFromDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = System.DateTime.Now.AddDays(-1);
        }
        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitCombo();
                //  InitControl();
                //  SetEvent();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
        #endregion

        #region Method
        private void GetList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("S70", typeof(string));
                inDataTable.Columns.Add("S71", typeof(string));
                inDataTable.Columns.Add("FROM_DATE", typeof(string));
                inDataTable.Columns.Add("TO_DATE", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["S70"] = Util.GetCondition(cboEqp, bAllNull: true);
                newRow["S71"] = Util.GetCondition(cboLane, bAllNull: true);
                newRow["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                newRow["TO_DATE"] = Util.GetCondition(dtpToDate);

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_EQP_TROUBLE_LIST", "INDATA", "OUTDATA", inDataTable);
                if (dtRslt.Rows.Count != 0)
                {
                    dtRslt.Columns.Add("CHK", typeof(bool));
                }
                Util.GridSetData(dgEqpTroubleList, dtRslt, this.FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void BtnUserConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("EQPTID");
                dtRqst.Columns.Add("EQPT_ALARM_CODE");
                dtRqst.Columns.Add("USERID");

                for (int i = 0; i < dgEqpTroubleList.Rows.Count; i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "CHK")).ToUpper().Equals("TRUE")
                        && !Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "WRKR_CHK_FLAG")).Equals("Y"))
                    {
                        DataRow dr = dtRqst.NewRow();
                        dr["EQPTID"] = Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "EQPTID"));
                        dr["EQPT_ALARM_CODE"] = Util.NVC(DataTableConverter.GetValue(dgEqpTroubleList.Rows[i].DataItem, "EQPT_ALARM_CODE"));
                        dr["USERID"] = LoginInfo.USERID;
                        dtRqst.Rows.Add(dr);
                    }
                }
                if (dtRqst.Rows.Count == 0) Util.MessageInfo("FM_ME_0165"); //선택된 데이터가 없습니다.
                else
                {
                    Util.MessageConfirm("FM_ME_0282", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_UPD_TC_TROUBLE", "RQSTDT", null, dtRqst);
                            GetList();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgEqpTroubleList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;
            if (e.Cell == null) return;

            C1DataGrid dataGrid = e.Cell.DataGrid;

            if (e.Cell.Column.Name.Equals("CHK"))
            {
                string userC = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "WRKR_CHK_FLAG"));
                if (userC.Equals("Y"))
                {
                    dataGrid.Rows[e.Cell.Row.Index].Presenter.Foreground = new SolidColorBrush(Colors.LightGray);
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                }
            }

        }

        private void chkHeaderAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgEqpTroubleList.Rows)
            {
                string wrkrChkFlag = "";
                wrkrChkFlag = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WRKR_CHK_FLAG"));
                if (wrkrChkFlag.Equals("N"))
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", true);
                }
            }
        }

        private void chkHeaderAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (C1.WPF.DataGrid.DataGridRow row in dgEqpTroubleList.Rows)
            {
                string wrkrChkFlag = "";
                wrkrChkFlag = Util.NVC(DataTableConverter.GetValue(row.DataItem, "WRKR_CHK_FLAG"));
                if (wrkrChkFlag.Equals("N"))
                {
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);
                }
            }

        }

        private void dgEqpTroubleList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name.Equals("CHK"))
            {
                if (Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Row.Index].DataItem, "WRKR_CHK_FLAG")).Equals("Y"))
                {
                    e.Cancel = true;
                }

            }
        }
    }
}

