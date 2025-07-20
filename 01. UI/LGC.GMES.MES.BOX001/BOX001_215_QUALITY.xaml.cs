/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : 자주검사
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_215_QUALITY : C1Window, IWorkArea
    {
        #region Declaration

        public UcFormShift UcFormShift { get; set; }

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드
        private string _wipSeq = string.Empty;

        int _maxColumn = 0;

        private bool _load = true;

        public string ShiftID { get; set; }
        public string ShiftName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerID { get; set; }
        public string ShiftDateTime { get; set; }

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

        public BOX001_215_QUALITY()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                _load = false;
            }
        }
        private void InitializeUserControls()
        {
            UcFormShift = grdShift.Children[0] as UcFormShift;
        }
        private void SetControl()
        {

            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[2] as string;
            _wipSeq = tmps[5] as string;

            // SET COMMON
            txtProcess.Text = tmps[1] as string;
            txtEquipment.Text = tmps[3] as string;
            txtLotID.Text = tmps[4] as string;

            // 자주검사 주기 콤보 조회
            SetQualityTime();

            cboQualityTime.SelectedValueChanged += cboQualityTime_SelectedValueChanged;

            // 자주검사 항목 조회
            GetGQualityInfo();

            // 작업자, 작업조
            UcFormShift.TextShift.Tag = ShiftID;
            UcFormShift.TextShift.Text = ShiftName;
            UcFormShift.TextWorker.Tag = WorkerID;
            UcFormShift.TextWorker.Text = WorkerName;
            UcFormShift.TextShiftDateTime.Text = ShiftDateTime;

            UcFormShift = grdShift.Children[0] as UcFormShift;
            if (UcFormShift != null)
            {
                UcFormShift.ButtonShift.Click += ButtonShift_Click;
            }

        }

        #endregion

        #region [저장]
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateQualitySave())
                return;

            // 검사 결과를 저장 하시겠습니까?
            Util.MessageConfirm("SFU2811", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    QualitySave();
                }
            });
        }
        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region [검사값 입력 : dgQuality_BeginningEdit, LoadedCellPresenter]
        private void dgQuality_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (!e.Row.Type.Equals(DataGridRowType.Top))
            {
                if (e.Column.Name.Length > 2)
                {
                    string ColName = e.Column.Name.Substring(e.Column.Name.Length - 2, 2).ToString();
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

                    if (int.TryParse(ColName, out ColCnt))
                    {
                        if (ColCnt > MaxCount)
                        {
                            e.Cancel = true;
                        }
                    }
                }

            }

        }
        private void dgQuality_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Cell.Row.Type.Equals(DataGridRowType.Top))
                return;

            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Column.Name.Length > 2)
                {
                    string ColName = e.Cell.Column.Name.Substring(e.Cell.Column.Name.Length - 2, 2).ToString();
                    int MaxCount = Util.NVC_Int(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CLCT_COUNT"));
                    int ColCnt = 0;

                    if (int.TryParse(ColName, out ColCnt))
                    {
                        if (ColCnt > MaxCount)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEBEBEB"));
                        }
                    }
                }

            }));

        }

        #endregion

        #region [검사 주기 변경: cboQualityTime_SelectedValueChanged]
        private void cboQualityTime_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            GetGQualityInfo();
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]

        /// <summary>
        /// 검사 시간
        /// </summary>
        private void SetQualityTime()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PROCID"] = _procID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_QCA_SEL_SELF_INSP_CBO_FO", "INDATA", "OUTDATA", inTable);

                DataTable dtQualityTime = new DataTable();
                dtQualityTime.Columns.Add(cboQualityTime.DisplayMemberPath.ToString(), typeof(string));
                dtQualityTime.Columns.Add(cboQualityTime.SelectedValuePath.ToString(), typeof(string));

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dtResult.Rows[0]["START_HHMMDD"].ToString()))
                    {
                        DateTime dtStart = Convert.ToDateTime(dtResult.Rows[0]["NOW_YMD"].ToString() + " " + dtResult.Rows[0]["START_HHMMDD"].ToString());
                        DateTime dtEnd = Convert.ToDateTime(dtResult.Rows[0]["NXT_YMD"].ToString() + " " + dtResult.Rows[0]["START_HHMMDD"].ToString());
                        DateTime dtCombo = dtStart;

                        int itvl =Util.NVC_Int(dtResult.Rows[0]["CLCT_ITVL"].ToString());

                        for (int loop = 0; loop < 999; loop++)
                        {
                            DataRow dr = dtQualityTime.NewRow();
                            dr[cboQualityTime.DisplayMemberPath.ToString()] = dtCombo.ToString("HH:mm");
                            dr[cboQualityTime.SelectedValuePath.ToString()] = dtCombo.ToString("yyyy-MM-dd HH:mm:ss"); 
                            dtQualityTime.Rows.Add(dr);

                            dtCombo = dtCombo.AddHours(itvl);

                            if (dtEnd <= dtCombo)
                                break;

                        }
                    }
                }

                DataRow drSelect = dtQualityTime.NewRow();
                drSelect[cboQualityTime.DisplayMemberPath.ToString()] = "-SELECT-";
                drSelect[cboQualityTime.SelectedValuePath.ToString()] = "SELECT";
                dtQualityTime.Rows.InsertAt(drSelect, 0);

                cboQualityTime.DisplayMemberPath = cboQualityTime.DisplayMemberPath.ToString();
                cboQualityTime.SelectedValuePath = cboQualityTime.SelectedValuePath.ToString();
                cboQualityTime.ItemsSource = dtQualityTime.Copy().AsDataView();
                cboQualityTime.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void GetGQualityInfo()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                Util.gridClear(dgQuality);
                return;
            }

            try
            {
                string sBizName = "DA_QCA_SEL_SELF_INSP_FO";

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("CLCTITEM_CLSS4", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(string));
                inTable.Columns.Add("ACTDTTM", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Util.NVC(_procID);
                newRow["CLCTITEM_CLSS4"] = "A";
                newRow["LOTID"] = txtLotID.Text;
                newRow["WIPSEQ"] = _wipSeq;
                newRow["ACTDTTM"] = cboQualityTime.SelectedValue ?? cboQualityTime.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", inTable);

                Util.GridSetData(dgQuality, dtResult, null);

                if (dtResult == null || dtResult.Rows.Count == 0)
                    return;

                // 검사 항목의 Max Column까지만 보이게
                _maxColumn = 0;
                _maxColumn = dtResult.AsEnumerable().ToList().Max(r => (int)r["CLCT_COUNT"]);

                int Startcol = dgQuality.Columns["ACTDTTM"].Index;
                int ForCount = 0;

                for (int col = Startcol + 1; col < dgQuality.Columns.Count; col++)
                {
                    ForCount++;

                    if (ForCount > _maxColumn)
                        dgQuality.Columns[col].Visibility = Visibility.Collapsed;
                }

                dgQuality.AlternatingRowBackground = null;

                // Merge
                string[] sColumnName = new string[] { "CLCTITEM_CLSS_NAME1" };
                _Util.SetDataGridMergeExtensionCol(dgQuality, sColumnName, DataGridMergeMode.VERTICAL);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void QualitySave()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_QCA_REG_WIP_DATA_CLCT";
                string colName = string.Empty;

                DataTable inTable = new DataTable();
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("WIPSEQ", typeof(decimal));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("CLCTSEQ", typeof(decimal));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("CLCTITEM", typeof(string));

                for (int col = 0; col < _maxColumn; col++)
                {
                    colName = "CLCTVAL" + (col + 1).ToString("00");
                    inTable.Columns.Add(colName, typeof(string));
                }
                inTable.Columns.Add("ACTDTTM", typeof(DateTime));

                foreach ( DataGridRow dRow in dgQuality.Rows)
                {
                    if (dRow.Type.Equals(DataGridRowType.Top))
                        continue;

                    DataRow newRow = inTable.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["LOTID"] = txtLotID.Text;
                    newRow["WIPSEQ"] = _wipSeq;
                    newRow["EQPTID"] = _eqptID;
                    newRow["CLCTSEQ"] = cboQualityTime.SelectedIndex;
                    newRow["USERID"] = LoginInfo.USERID;
                    newRow["CLCTITEM"] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, "INSP_CLCTITEM"));
                    newRow["ACTDTTM"] = cboQualityTime.SelectedValue.ToString();

                    int colValue = 0;

                    for (int col = dgQuality.Columns.Count - _maxColumn; col < dgQuality.Columns.Count; col++)
                    {
                        colValue++;

                        if (colValue > _maxColumn)
                            break;

                        colName = "CLCTVAL" + colValue.ToString("00");

                        newRow[colName] = Util.NVC(DataTableConverter.GetValue(dRow.DataItem, colName));
                    }
                    inTable.Rows.Add(newRow);
                }

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateQualitySave()
        {
            if (cboQualityTime.SelectedIndex < 0 || cboQualityTime.SelectedValue.GetString().Equals("SELECT"))
            {
                // 검사 주기를 선택 하세요.
                Util.MessageValidation("SFU4024");
                return false;
            }

            ////if (UcFormShift.TextShift.Tag == null || string.IsNullOrEmpty(UcFormShift.TextShift.Tag.ToString()))
            ////{
            ////    // 작업조를 입력해 주세요.
            ////    Util.MessageValidation("SFU1845");
            ////    return false;
            ////}

            ////if (UcFormShift.TextWorker.Tag == null || string.IsNullOrEmpty(UcFormShift.TextWorker.Tag.ToString()))
            ////{
            ////    // 작업자를 입력 해 주세요.
            ////    Util.MessageValidation("SFU1843");
            ////    return false;
            ////}

            return true;
        }
        #endregion

        #region [Func]

        #region 작업조, 작업자
        private void GetEqptWrkInfo()
        {
            try
            {
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_WRK_INFO";

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("SHOPID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _eqptID;
                newRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["EQSGID"] = LoginInfo.CFG_EQSG_ID;
                newRow["PROCID"] = _procID; ;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (result, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        if (UcFormShift != null)
                        {
                            if (result.Rows.Count > 0)
                            {
                                if (!result.Rows[0].ItemArray[0].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftStartTime.Text = Util.NVC(result.Rows[0]["WRK_STRT_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftStartTime.Text = string.Empty;
                                }

                                if (!result.Rows[0].ItemArray[1].ToString().Equals(""))
                                {
                                    UcFormShift.TextShiftEndTime.Text = Util.NVC(result.Rows[0]["WRK_END_DTTM"]);
                                }
                                else
                                {
                                    UcFormShift.TextShiftEndTime.Text = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(UcFormShift.TextShiftStartTime.Text) && !string.IsNullOrEmpty(UcFormShift.TextShiftEndTime.Text))
                                {
                                    UcFormShift.TextShiftDateTime.Text = UcFormShift.TextShiftStartTime.Text + " ~ " + UcFormShift.TextShiftEndTime.Text;
                                }
                                else
                                {
                                    UcFormShift.TextShiftDateTime.Text = string.Empty;
                                }

                                if (Util.NVC(result.Rows[0]["WRK_USERID"]).Equals(""))
                                {
                                    UcFormShift.TextWorker.Text = string.Empty;
                                    UcFormShift.TextWorker.Tag = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextWorker.Text = Util.NVC(result.Rows[0]["WRK_USERNAME"]);
                                    UcFormShift.TextWorker.Tag = Util.NVC(result.Rows[0]["WRK_USERID"]);
                                }

                                if (Util.NVC(result.Rows[0]["SHFT_ID"]).Equals(""))
                                {
                                    UcFormShift.TextShift.Tag = string.Empty;
                                    UcFormShift.TextShift.Text = string.Empty;
                                }
                                else
                                {
                                    UcFormShift.TextShift.Text = Util.NVC(result.Rows[0]["SHFT_NAME"]);
                                    UcFormShift.TextShift.Tag = Util.NVC(result.Rows[0]["SHFT_ID"]);
                                }
                            }
                            else
                            {
                                UcFormShift.ClearShiftControl();
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        private void ButtonShift_Click(object sender, RoutedEventArgs e)
        {
            CMM_SHIFT_USER2 popupShiftUser = new CMM_SHIFT_USER2 { FrameOperation = this.FrameOperation };

            object[] parameters = new object[8];
            parameters[0] = LoginInfo.CFG_SHOP_ID;
            parameters[1] = LoginInfo.CFG_AREA_ID;
            parameters[2] = LoginInfo.CFG_EQSG_ID;
            parameters[3] = _procID;
            parameters[4] = Util.NVC(UcFormShift.TextShift.Tag);
            parameters[5] = Util.NVC(UcFormShift.TextWorker.Tag);
            parameters[6] = _eqptID;
            parameters[7] = "Y"; // 저장 Flag "Y" 일때만 저장.
            C1WindowExtension.SetParameters(popupShiftUser, parameters);

            popupShiftUser.Closed += new EventHandler(popupShiftUser_Closed);

            foreach (System.Windows.Controls.Grid tmp in Util.FindVisualChildren<System.Windows.Controls.Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupShiftUser);
                    popupShiftUser.BringToFront();
                    break;
                }
            }
        }

        private void popupShiftUser_Closed(object sender, EventArgs e)
        {
            CMM_SHIFT_USER2 popup = sender as CMM_SHIFT_USER2;
            if (popup != null && popup.DialogResult == MessageBoxResult.OK)
            {
                GetEqptWrkInfo();
            }
            this.Focus();
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
