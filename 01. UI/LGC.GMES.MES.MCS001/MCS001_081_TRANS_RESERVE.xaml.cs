/*************************************************************************************
 Created Date : 2022.08.05
      Creator : 오화백
   Decription : HOLD Lot List [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2022.08.05  오화백 : Initial Created.    
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_081_TRANS_RESERVE : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
        private readonly Util _util = new Util();
        private string _SelectedRadioButtonValue;
        private string _Eqgrid;
        private string _EquipmentCode;
        private string _ProductVersion;
        private string _ProductCode;
        private string _ProjectName;
        private string _SelectedWipHold;
        private string _LotId;
        private string _HalfSlitterSideCode;
        private string _Em_section_roll_dirctn;
        private string _HoldCode;
        private string _PastDay;
        private string _ElectrodeTypeCode;
        private string _Eltr_type_code_lot;
        private string _SelectedQmsHold;
        private string _FaultyType;
        private string _SelectedQmsHold_ETC;
        private string _ISS_RSV_TRGT_PORT_ID;
        public bool IsUpdated;
        private string _BobbinID;
        private string _EqptName;

        public MCS001_081_TRANS_RESERVE()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _SelectedRadioButtonValue = parameters[0] as string;
                _Eqgrid = parameters[1] as string;
                _EquipmentCode = parameters[2] as string;
                _ProductVersion = parameters[3] as string;
                _ProductCode = parameters[4] as string;
                _ProjectName = parameters[5] as string;
                _SelectedWipHold = parameters[6] as string;
                _LotId = parameters[7] as string;
                _HalfSlitterSideCode = parameters[8] as string;
                _Em_section_roll_dirctn = parameters[9] as string;
                _HoldCode = parameters[10] as string;
                _PastDay = parameters[11] as string;
                _ElectrodeTypeCode = parameters[12] as string;
                _Eltr_type_code_lot = parameters[13] as string;
                _SelectedQmsHold = parameters[14] as string;
                _FaultyType = parameters[15] as string;
                _SelectedQmsHold_ETC = parameters[16] as string;
                _ISS_RSV_TRGT_PORT_ID = parameters[17] as string;
                _BobbinID = parameters[18] as string;
                _EqptName = parameters[19] as string;

                tbDepature.Text = _EqptName;

                SetProcEqpt();

                SelectList();
            }
        }
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #region Event

        #region  화면닫기 : btnClose_Click()
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region 순서위로 : btnUp_Click()
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataTable dt = (dgList.ItemsSource as DataView).ToTable();
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["REQ_TRFID"] = string.Empty;

                if (idx > 0)
                {
                    DataRow drTemp = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        drTemp[dc.ColumnName] = dt.Rows[idx][dc.ColumnName];
                    }
                    drTemp["REQ_TRFID"] = "Y";

                    dt.Rows.RemoveAt(idx);
                    dt.Rows.InsertAt(drTemp, idx - 1);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["ISS_RSV_PRIORITY_NO"] = i + 1;

                Util.GridSetData(dgList, dt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 순서아래로 : btnDown_Click()
        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int idx = ((DataGridCellPresenter)((sender as Button).Parent)).Row.Index;

                DataTable dt = (dgList.ItemsSource as DataView).ToTable();
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["REQ_TRFID"] = string.Empty;

                if (idx < dt.Rows.Count -1)
                {
                    DataRow drTemp = dt.NewRow();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        drTemp[dc.ColumnName] = dt.Rows[idx][dc.ColumnName];
                    }
                    drTemp["REQ_TRFID"] = "Y";

                    dt.Rows.RemoveAt(idx);
                    dt.Rows.InsertAt(drTemp, idx + 1);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["ISS_RSV_PRIORITY_NO"] = i + 1;

                Util.GridSetData(dgList, dt, FrameOperation, false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 리스트 이벤트 (색상표현) : dgList_LoadedCellPresenter(), dgList_UnloadedCellPresenter() 
        /// <summary>
        /// 리스트 색상표현
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "REQ_TRFID")).Equals(string.Empty))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;

                    }

                }

            }));
        }
        /// <summary>
        /// 스크롤 이동시 색상 초기화 막기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid dataGrid = sender as C1DataGrid;
            if (e.Cell.Presenter != null)
            {
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    e.Cell.Presenter.Background = null;
                }
            }
        }

        #endregion

        #region 예약취소 : btnCancel_Click()
        /// <summary>
        /// 예약 취소
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {


            if (_util.GetDataGridRowCountByCheck(dgList, "CHK") < 1)
            {
                Util.MessageValidation("SFU1636");
                return ;
            }


            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1243"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result != MessageBoxResult.OK)
                    return;

                try
                {
                    ShowLoadingIndicator();
                    const string bizRuleName = "BR_MHS_REG_ALL_CANCEL_BY_UI";

                    DataTable inTable = new DataTable("IN_REQ_TRF_INFO");
                    inTable.Columns.Add("CARRIERID", typeof(string));
                    inTable.Columns.Add("UPDUSER", typeof(string));

                    C1DataGrid dg = dgList; ;

                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        if (row.Type == DataGridRowType.Item && Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")).GetString() == "1")
                        {
                            DataRow newRow = inTable.NewRow();
                            newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "BOBBIN_ID").GetString();
                            newRow["UPDUSER"] = LoginInfo.USERID;
                            inTable.Rows.Add(newRow);
                        }
                    }

                    new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", null, inTable, (rslt, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            IsUpdated = true;
                            Util.MessageInfo("SFU1736"); //예약취소 완료
                            SelectList();
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
                finally
                {
                    HiddenLoadingIndicator();
                }

            });
        }
        #endregion

        #region 순서저장 : btnSave_Click()
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3472"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result != MessageBoxResult.OK)
                    return;

                try
                {
                    ShowLoadingIndicator();
                    const string bizRuleName = "BR_MHS_UPD_CST_RSV_PRIORITY_BY_UI";

                    DataTable inTable = new DataTable("IN_DATA");
                    inTable.Columns.Add("CARRIERID", typeof(string));
                    inTable.Columns.Add("PRIORITY_NO", typeof(Decimal));
                    inTable.Columns.Add("UPDUSER", typeof(string));

                    C1DataGrid dg = dgList; ;

                    foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
                    {
                        DataRow newRow = inTable.NewRow();
                        newRow["CARRIERID"] = DataTableConverter.GetValue(row.DataItem, "BOBBIN_ID").GetString();
                        newRow["PRIORITY_NO"] = DataTableConverter.GetValue(row.DataItem, "ISS_RSV_PRIORITY_NO").GetDecimal();
                        newRow["UPDUSER"] = LoginInfo.USERID;
                        inTable.Rows.Add(newRow);
                    }
                    new ClientProxy().ExecuteService(bizRuleName, "IN_DATA", null, inTable, (rslt, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }
                            IsUpdated = true;
                            Util.MessageInfo("SFU3473"); //순서변경완료
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
                finally
                {
                    HiddenLoadingIndicator();
                }

            });
        }
        #endregion

        #region 목적지 콤보 이벤트 : cboIssueProcEqpt_SelectedIndexChanged() 
        private void cboIssueProcEqpt_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            SelectList();
        }
        #endregion

        #endregion

        #region Mehod

        #region 리스트 조회 : SelectList()
        /// <summary>
        /// 리스트 조회
        /// </summary>
        /// <param name="bInit"></param>
        private void SelectList()
        {
            Util.gridClear(dgList);
            const string bizRuleName = "DA_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";

            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FIFO", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_VER_CODE", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                inTable.Columns.Add("HOLD_CODE", typeof(string));
                inTable.Columns.Add("PAST_DAY", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE_LOT", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("QA_INSP_JUDG_VALUE", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG_ETC", typeof(string));
                inTable.Columns.Add("ISS_RSV_FLAG", typeof(string));
                inTable.Columns.Add("ISS_RSV_TRGT_PORT_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["FIFO"] = _SelectedRadioButtonValue;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["EQGRID"] = _Eqgrid;
                dr["EQPTID"] = _EquipmentCode;
                //dr["PROD_VER_CODE"] = _ProductVersion;
                //dr["PRODID"] = _ProductCode;
                //dr["PRJT_NAME"] = _ProjectName;
                //dr["WIPHOLD"] = _SelectedWipHold;
                //dr["LOTID"] = _LotId;
                //dr["HALF_SLIT_SIDE"] = _HalfSlitterSideCode;
                //dr["EM_SECTION_ROLL_DIRCTN"] = _Em_section_roll_dirctn;
                //dr["HOLD_CODE"] = _HoldCode;
                //dr["PAST_DAY"] = _PastDay;
                //dr["ELTR_TYPE_CODE"] = _ElectrodeTypeCode;
                //dr["ELTR_TYPE_CODE_LOT"] = _Eltr_type_code_lot;
                //dr["QMS_HOLD_FLAG"] = _SelectedQmsHold;
                //dr["QA_INSP_JUDG_VALUE"] = _FaultyType;
                //dr["QMS_HOLD_FLAG_ETC"] = _SelectedQmsHold_ETC;
                dr["ISS_RSV_FLAG"] = "Y";
                dr["ISS_RSV_TRGT_PORT_ID"] = _ISS_RSV_TRGT_PORT_ID;
                dr["ISS_RSV_TRGT_PORT_ID"] = ((ContentControl)(cboIssueProcEqpt.Items[cboIssueProcEqpt.SelectedIndex])).Tag.ToString(); 

                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    if (bizResult.Rows.Count > 0)
                    {
                        for (int i = 0; i < bizResult.Rows.Count; i++)
                        {
                            bizResult.Rows[i]["CHK"] = 0;
                            bizResult.Rows[i]["ROW_NUM"] = i + 1;
                        }
                    }

                    DataView inputVm = bizResult.DefaultView;
                    inputVm.Sort = "ISS_RSV_PRIORITY_NO";

                    DataTable BindData = inputVm.ToTable().Copy();
                    Util.GridSetData(dgList, BindData, FrameOperation, false);
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 목적지 콤보 조회 : SetProcEqpt()
        private void SetProcEqpt()
        {
            try
            {
                this.cboIssueProcEqpt.SelectedIndexChanged -= cboIssueProcEqpt_SelectedIndexChanged;
                if (cboIssueProcEqpt.Items.Count > 0)
                {
                    cboIssueProcEqpt.ItemsSource = null;
                    for (int i = 0; i < cboIssueProcEqpt.Items.Count; i++)
                    {
                        cboIssueProcEqpt.Items.RemoveAt(i);
                        i--;
                    }
                    cboIssueProcEqpt.Items.Clear();
                    cboIssueProcEqpt.SelectedValue = null;
                }

                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));


                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CSTID"] = _BobbinID;

                inTable.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MHS_SEL_PROC_PORT_LIST", "RQSTDT", "RSLTDT", inTable);

                foreach (DataRow row in dtResult.Rows)
                {
                    C1ComboBoxItem comboBoxItem = new C1ComboBoxItem();
                    comboBoxItem.Content = row["PORT_NAME"].GetString();
                    comboBoxItem.Tag = row["DST_PORTID"].GetString();
                    comboBoxItem.Name = row["DST_EQPTID"].GetString();
                    comboBoxItem.DataContext = row["DST_PORTID"].GetString();

                    cboIssueProcEqpt.Items.Add(comboBoxItem);

                    if (row["DST_PORTID"].ToString() == _ISS_RSV_TRGT_PORT_ID)
                        cboIssueProcEqpt.SelectedItem = comboBoxItem;

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                this.cboIssueProcEqpt.SelectedIndexChanged += cboIssueProcEqpt_SelectedIndexChanged;
            }
        }
        #endregion

        #region 프로그래스 바  ShowLoadingIndicator(), HiddenLoadingIndicator()

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