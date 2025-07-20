/*************************************************************************************
 Created Date : 2023.06.16
      Creator : 
   Decription :    Decription : 포장 Pallet Loaction 현황 - CHANGE_LOCATION
--------------------------------------------------------------------------------------
 [Change History]
  2023.06.16  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_381_CHANGE_LOCATION : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sAREAID  = string.Empty;
        private string sBLDG    = string.Empty;
        private string sWHID    = string.Empty;
        private string sRACKID  = string.Empty;
        DataTable sdtBOX = new DataTable();

        private int toLocationIndex = -1;
        private bool _sTodoLocation = false;
        private bool _sSelectPallet = false;


        public COM001_381_CHANGE_LOCATION()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.Visibility = Visibility.Visible;
            }
                
        }
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            sAREAID = Util.NVC(tmps[0]);
            sBLDG = Util.NVC(tmps[1]);
            sWHID = Util.NVC(tmps[2]);
            sdtBOX = tmps[3] as DataTable;

            InitCombo();

            InitializeControls();

            Loaded -= C1Window_Loaded;
        }


        private void InitializeControls()
        {
            Util.gridClear(dgSelectPallet);
            Util.gridClear(dgToLocation);

            _sTodoLocation = false;
            _sSelectPallet = false;

            GetChangeLocationByPalletProduct();  // 초기 Loading 
            GetChangeLocationToLocation();  // 초기 Loading
        }

        #endregion

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            SetcboSection(cboSection);

            SetcboLocationStatus(cboLocationStatus);

            SetcboModelMix(cboModelMix);

        }


        private static DataTable AddStatus(DataTable dt, string sValue, string sDisplay, string statusType)
        {
            DataRow dr = dt.NewRow();
            switch (statusType)
            {
                case "ALL":
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "SELECT":
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "NA":
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case "EMPTY":
                    dr[sValue] = string.Empty;
                    dr[sDisplay] = string.Empty;
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SetcboSection(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = sAREAID;
                dr["WH_PHYS_PSTN_CODE"] = sBLDG;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_SECTION_BY_BLDG", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "WH_NAME";
                cbo.SelectedValuePath = "WH_ID";

                cbo.ItemsSource = AddStatus(dtResult, "WH_ID", "WH_NAME", "ALL").Copy().AsDataView();

                cbo.SelectedIndex = 0;

                if (sWHID != null && !sWHID.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sWHID.Equals(row["WH_ID"].ToString()))
                            cbo.SelectedIndex = i;
                    }
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void SetcboLocationStatus(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                DataRow dr = RQSTDT.NewRow();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_LOCATION_STAT", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "HOLD_RACK_FLAG_NAME";
                cbo.SelectedValuePath = "HOLD_RACK_FLAG";

                cbo.ItemsSource = AddStatus(dtResult, "HOLD_RACK_FLAG", "HOLD_RACK_FLAG_NAME", "ALL").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        private void SetcboModelMix(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                DataRow dr = RQSTDT.NewRow();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_MODEL_MIX", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "MDL_MIX_ENABLE_RACK_FLAG_NAME";
                cbo.SelectedValuePath = "MDL_MIX_ENABLE_RACK_FLAG";

                cbo.ItemsSource = AddStatus(dtResult, "MDL_MIX_ENABLE_RACK_FLAG", "MDL_MIX_ENABLE_RACK_FLAG_NAME", "ALL").Copy().AsDataView();

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        public void GetChangeLocationByPalletProduct()
        {
            _sSelectPallet = true;
            ShowLoadingIndicator();

            Util.gridClear(dgSelectPallet);

            try
            {
                
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BOXID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                string _boxId = string.Empty;
                for (int i = 0; i < sdtBOX.Rows.Count; i++)
                {
                    DataRow row = sdtBOX.Rows[i] as DataRow;
                    if (i != sdtBOX.Rows.Count - 1)
                    {
                        _boxId += row["BOXID"].ToString() + ",";
                    }
                    else
                    {
                        _boxId += row["BOXID"].ToString();
                    }
                }
                dr["BOXID"] = Util.NVC(_boxId).Equals(string.Empty) ? null : _boxId;
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_LOC_MNG_SEL_PLLT";
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
                {
                    _sSelectPallet = false;
                    if (!_sTodoLocation)  HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                        Util.GridSetData(dgSelectPallet, result, FrameOperation, true);

                });
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

        }


        public void GetChangeLocationToLocation()
        {
            _sTodoLocation = true;

            ShowLoadingIndicator();

            Util.gridClear(dgToLocation);

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("HOLD_RACK_FLAG", typeof(string));
                RQSTDT.Columns.Add("MDL_MIX_ENABLE_RACK_FLAG", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                string _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _holdRackFlag = string.Empty;
                if (cboLocationStatus.SelectedValue != null) 
                     _holdRackFlag = Util.GetCondition(cboLocationStatus, bAllNull: false).ToString();
                dr["HOLD_RACK_FLAG"] = Util.NVC(_holdRackFlag).Equals(string.Empty) ? null : _holdRackFlag;
                string _MdlMixEnableRackFlag = Util.GetCondition(cboModelMix, bAllNull: false).ToString();
                dr["MDL_MIX_ENABLE_RACK_FLAG"] = Util.NVC(_MdlMixEnableRackFlag).Equals(string.Empty) ? null : _MdlMixEnableRackFlag;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_LOC_MNG_TO_LOCATION";
                new ClientProxy().ExecuteService(sBizName, "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
                {
                    _sTodoLocation = false;
                    if (!_sSelectPallet) HiddenLoadingIndicator();

                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        Util.GridSetData(dgToLocation, result, FrameOperation, true);
                        // dgToLocation.SelectedIndex = 0;
                        // toLocationIndex = 0;
                        // DataTableConverter.SetValue(dgToLocation.Rows[0].DataItem, "CHK", true);
                    }
                        
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

        }
        
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid())
            {
                return;
            }
            else
            {
                Util.MessageConfirm("SFU3533", result =>   // 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveData();
                    }
                });
            }
            
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetChangeLocationToLocation();  // Search Button 누를때
        }
        #endregion

        #region Mehod
        private bool isValid()
        {
            bool bRet = false;

            if (toLocationIndex < 0)
            {
                // 저장 할 데이타가 선택되지 않았습니다
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3534"), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        private void SaveData()
        {
            ShowLoadingIndicator();

            try
            {

                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                DataTable INDATA_BOXID = inDataSet.Tables.Add("INDATA_BOXID");

                INDATA.TableName = "INDATA";
                INDATA_BOXID.TableName = "INDATA_BOXID";
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("ACTID", typeof(string));

                INDATA_BOXID.Columns.Add("BOXID", typeof(string));
                INDATA_BOXID.Columns.Add("CSTID", typeof(string));
                INDATA_BOXID.Columns.Add("LANGID", typeof(string));


                DataTable dt = DataTableConverter.Convert(dgToLocation.ItemsSource);
                string _sWhId = string.Empty;
                _sWhId = dt.Rows[toLocationIndex]["WH_ID"].ToString();
                sRACKID = dt.Rows[toLocationIndex]["RACK_ID"].ToString();

                DataRow newRow = INDATA.NewRow();
                newRow["WH_ID"] = _sWhId;
                newRow["RACK_ID"] = sRACKID;
                newRow["SRCTYPE"] = "UI";
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["ACTID"] = string.Empty;
                INDATA.Rows.Add(newRow);

                foreach (DataRow dr in sdtBOX.Rows)
                {
                    DataRow sRow = INDATA_BOXID.NewRow();
                    sRow["BOXID"] = dr["BOXID"];
                    sRow["CSTID"] = dr["CSTID"];
                    sRow["LANGID"] = LoginInfo.LANGID;
                    INDATA_BOXID.Rows.Add(sRow);
                }


                try
                {
                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CELL_PLLT_LOC_MNG_CHG_LOC", "INDATA,INDATA_BOXID", null, (result, bizex) =>
                    {
                        HiddenLoadingIndicator();

                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }
                        else
                        {
                           
                            Util.MessageValidation("SFU1270");   // 정상처리되었습니다. 

                            this.DialogResult = MessageBoxResult.OK;
                            this.Close();
                        }
                    }, inDataSet);

                }
                catch (Exception ex)
                {
                    HiddenLoadingIndicator();
                    Util.MessageException(ex);
                }
                finally
                {
                }
                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        #endregion

        private void dgToLocation_CurrentCellChanged(object sender, DataGridCellEventArgs e)
        {
            if (e.Cell != null)
            {
                /*
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    C1DataGrid dg = sender as C1DataGrid;
                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (dgToLocation.GetCell(i, 0).Presenter != null)
                        {
                            if (e.Cell.Row.Index == i)
                            {
                                (dgToLocation.GetCell(i, 0).Presenter.Content as RadioButton).IsChecked = true;
                            }
                            else
                            {
                                (dgToLocation.GetCell(i, 0).Presenter.Content as RadioButton).IsChecked = false;
                            }
                        }

                    }
                    toLocationIndex = e.Cell.Row.Index;
                }));
*/
                
            }
            
        }

        private void rbChoice_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if (sender == null)
                return;

            if ((bool)rb.IsChecked)
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;// - 2;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;

                for (int i = 0; i < dg.GetRowCount(); i++)
                {

                    if (idx == i)
                    {
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    }
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                toLocationIndex = idx;
            }

        }
    }
}
