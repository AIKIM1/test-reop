/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_051_ADD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public DataTable dtAdd;

        string tmp = string.Empty;
        object[] tmps;
        string tmmp01 = string.Empty;
        string tmmp02 = string.Empty;
        string tmmp03 = string.Empty;

        public Boolean bCheck = false;

        public COM001_051_ADD()
        {
            InitializeComponent();

            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tmp = C1WindowExtension.GetParameter(this);
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as string;
            tmmp03 = tmps[2] as string;

            this.Loaded -= Window_Loaded;

            Initialize();

            txtLotID.Focus();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
            
            dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
            dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

            #region Combo Setting
            CommonCombo combo = new CommonCombo();

            ////combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.ALL);
            //C1ComboBox[] cboToChild = { cboTransLoc };
            //combo.SetCombo(cboArea, CommonCombo.ComboStatus.ALL, cbChild: cboToChild, sCase: "AREA");

            ////combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT);
            //C1ComboBox[] cboCompParent = { cboArea };
            //combo.SetCombo(cboTransLoc, CommonCombo.ComboStatus.SELECT, cbChild: null, cbParent: cboCompParent);

            #endregion

        }
        #endregion

        #region Event
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                dtAdd = new DataTable();
                dtAdd.TableName = "dtAdd";
                dtAdd.Columns.Add("LOTID", typeof(string));
                dtAdd.Columns.Add("INPUTQTY", typeof(string));

                for (int i = 0; i < dgAdd.GetRowCount(); i++)
                {
                    if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "CHK").ToString() == "1")
                    {
                        if (Convert.ToInt32(DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INPUTQTY")) == 0)
                        {
                            Util.Alert("SFU2973");   //투입수량 정보가 없습니다.
                            return;
                        }
                        else
                        {
                            DataRow drAdd = dtAdd.NewRow();

                            drAdd["LOTID"] = DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "LOTID");
                            drAdd["INPUTQTY"] = DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "INPUTQTY");
                            dtAdd.Rows.Add(drAdd);
                        }
                    }
                }

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bCheck = true;
                Util.gridClear(dgAdd);

                string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                string sLot_id = txtLotID.Text.Trim();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PROCID", typeof(String));
                RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                RQSTDT.Columns.Add("WIP_TYPE_CODE", typeof(String));
                RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                RQSTDT.Columns.Add("TO_DATE", typeof(String));
                RQSTDT.Columns.Add("PRODID", typeof(String));
                RQSTDT.Columns.Add("LANGID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["PROCID"] = "E0700";
                dr["WIPSTAT"] = Wip_State.WAIT;
                dr["WIP_TYPE_CODE"] = "IN";
                dr["FROM_DATE"] = sStart_date;
                dr["TO_DATE"] = sEnd_date;
                dr["PRODID"] = tmmp01;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1870");      //재공 정보가 없습니다.
                    return;
                }

                if (dgAdd.GetRowCount() == 0)
                {
                    Util.GridSetData(dgAdd, SearchResult, FrameOperation);
                }
                else
                {

                    DataTable dtSource = DataTableConverter.Convert(dgAdd.ItemsSource);
                    dtSource.Merge(SearchResult);

                    Util.gridClear(dgAdd);
                    dgAdd.ItemsSource = DataTableConverter.Convert(dtSource);

                }

                bCheck = false;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                return;
            }
        }

        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    bCheck = true;
                    string sStart_date = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                    string sEnd_date = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                    string sLot_id = txtLotID.Text.ToString().Trim();

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "RQSTDT";
                    RQSTDT.Columns.Add("PROCID", typeof(String));
                    RQSTDT.Columns.Add("WIPSTAT", typeof(String));
                    RQSTDT.Columns.Add("WIP_TYPE_CODE", typeof(String));
                    RQSTDT.Columns.Add("FROM_DATE", typeof(String));
                    RQSTDT.Columns.Add("TO_DATE", typeof(String));
                    RQSTDT.Columns.Add("LOTID", typeof(String));
                    RQSTDT.Columns.Add("PRODID", typeof(String));

                    DataRow dr = RQSTDT.NewRow();
                    dr["PROCID"] = "E0700";
                    dr["WIPSTAT"] = Wip_State.WAIT;
                    dr["WIP_TYPE_CODE"] = "IN";
                    dr["FROM_DATE"] = sStart_date;
                    dr["TO_DATE"] = sEnd_date;
                    dr["LOTID"] = sLot_id == "" ? null : sLot_id;
                    dr["PRODID"] = tmmp01;

                    RQSTDT.Rows.Add(dr);

                    DataTable SearchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WIP_INFO_POPUP", "RQSTDT", "RSLTDT", RQSTDT);

                    if (SearchResult.Rows.Count == 0)
                    {
                        Util.Alert("SFU1870");      //재공 정보가 없습니다.
                        return;
                    }
                    else if (SearchResult.Rows.Count > 1)
                    {
                        Util.AlertInfo("SFU2887", new object[] { "2" });   //{0}건 이상이 조회되었습니다.
                        return;
                    }

                    if (dgAdd.GetRowCount() >= 1)
                    {
                        for (int i = 0; i < dgAdd.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "LOTID").ToString() == sLot_id)
                            {
                                Util.Alert("SFU1504");  //동일한 LOT이 스캔되었습니다.
                                return;
                            }

                            if (DataTableConverter.GetValue(dgAdd.Rows[i].DataItem, "PRODID").ToString() != SearchResult.Rows[0]["PRODID"].ToString())
                            {
                                Util.Alert("SFU1502");   //동일 제품이 아닙니다.
                                return;
                            }
                        }
                    }

                    if (dgAdd.GetRowCount() == 0)
                    {
                        Util.GridSetData(dgAdd, SearchResult, FrameOperation);
                    }
                    else
                    {

                        DataTable dtSource = DataTableConverter.Convert(dgAdd.ItemsSource);
                        dtSource.Merge(SearchResult);

                        Util.gridClear(dgAdd);
                        dgAdd.ItemsSource = DataTableConverter.Convert(dtSource);
                    }

                    txtLotID.SelectAll();

                    bCheck = false;

                }
                catch (Exception ex)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
                    return;
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            int checkIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)((sender as CheckBox).Parent)).Row.Index;
        }


        #endregion

        #region Mehod
        private void dgAdd_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                if (bCheck == true)
                    return;

                int idx = e.Cell.Row.Index;

                decimal dWipqty = 0;
                decimal dInputqty = 0;

                if (idx >= 0)
                {
                    if (e.Cell.Column.Name.Equals("INPUTQTY"))
                    {
                        dWipqty = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[e.Cell.Row.Index].DataItem, "WIPQTY")));
                        dInputqty = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgAdd.Rows[e.Cell.Row.Index].DataItem, "INPUTQTY")));


                        if (dWipqty < dInputqty)
                        {
                            Util.AlertInfo("SFU3025");  //투입수량이 재공수량을 초과하였습니다.
                            DataTableConverter.SetValue(dgAdd.Rows[idx].DataItem, "INPUTQTY", 0);
                            return;
                        }
                        else if (Convert.ToDecimal(tmmp03) < dInputqty)
                        {
                            Util.AlertInfo("SFU3139");  //투입수량이 기준수량을 초과하였습니다.
                            DataTableConverter.SetValue(dgAdd.Rows[idx].DataItem, "INPUTQTY", 0);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        #endregion


    }
}
