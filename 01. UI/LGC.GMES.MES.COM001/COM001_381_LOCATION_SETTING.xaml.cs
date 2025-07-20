/*************************************************************************************
 Created Date : 2023.06.16
      Creator : 
   Decription : 포장 Pallet Loaction 현황 - LOCATION_SETTING
--------------------------------------------------------------------------------------
 [Change History]
  2023.06.16  DEVELOPER : Initial Created.
  2023.07.21  주재홍 : 현장 테스트후 3차 개선
  2023.07.31  주재홍 : BizAct 다국어

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_381_LOCATION_SETTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sAREAID = string.Empty;
        private string sBLDG = string.Empty;
        private string sSECTION = string.Empty;
        private string sRACKID = string.Empty;
        bool InputRow = false;
        int addRows;

        public COM001_381_LOCATION_SETTING()
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
            sSECTION = Util.NVC(tmps[2]);
            sRACKID = Util.NVC(tmps[3]);


            /*
            string[] _rackId = Util.NVC(tmps[3]).Split(',');
            for (int i = 0; i < _rackId.Count(); i++)
            {
                if (i != cboLocation.Items.Count - 1)
                {
                    _rackId += cboLocation.SelectedItems[i] + ",";
                }
                else
                {
                    _rackId += cboLocation.SelectedItems[i];
                }
            }
            string aa = _rackId.GetValue(0).ToString();
            */

            InitCombo();

            GetPalletByLocation();
        }


        #endregion

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            SetcboSection(cboSection);
            SetcboLocation(cboLocation);
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

                if (sSECTION != null && !sSECTION.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sSECTION.Equals(row["WH_ID"].ToString()))
                            cbo.SelectedIndex = i;
                    }
                } else {
                    cbo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboSection_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            string sTemp = sBLDG;
            SetcboLocation(cboLocation);

        }


        private void SetcboLocation(MultiSelectionBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("BLDG", typeof(string));
                RQSTDT.Columns.Add("WH_ID", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["BLDG"] = sBLDG;
                String _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                dr["LANGID"] = LoginInfo.LANGID;

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_RACK_ID_BY_SECTION", "RQSTDT", "RSLTDT", RQSTDT);
                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "RACK_ID";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                if (sRACKID != null && !sRACKID.Equals(""))
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow row = dtResult.Rows[i];
                        if (sRACKID.Contains(row["RACK_ID"].ToString()))
                            cbo.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetPalletByLocation();
        }


        public void GetPalletByLocation()
        {
            ShowLoadingIndicator();

            Util.gridClear(dgRackList);

            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("WH_PHYS_PSTN_CODE", typeof(string));
                INDATA.Columns.Add("WH_ID", typeof(string));
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["AREAID"] = Util.NVC(sAREAID).Equals(string.Empty) ? null : sAREAID;
                dr["WH_PHYS_PSTN_CODE"] = Util.NVC(sBLDG).Equals(string.Empty) ? null : sBLDG;
                string _whId = Util.GetCondition(cboSection, bAllNull: false).ToString();
                dr["WH_ID"] = Util.NVC(_whId).Equals(string.Empty) ? null : _whId;
                string _rackId = string.Empty;
                for (int i = 0; i < cboLocation.SelectedItems.Count; i++)
                {
                    if (i != cboLocation.SelectedItems.Count - 1)
                    {
                        _rackId += cboLocation.SelectedItems[i] + ",";
                    }
                    else
                    {
                        _rackId += cboLocation.SelectedItems[i];
                    }
                }
                dr["RACK_ID"] = Util.NVC(_rackId).Equals(string.Empty) ? null : _rackId;
                dr["LANGID"] = LoginInfo.LANGID;
                INDATA.Rows.Add(dr);

                string sBizName = "DA_PRD_GET_CELL_PLLT_LOC_MNG_LOCATION_SETTING";
                new ClientProxy().ExecuteService(sBizName, "INDATA", "OUTDATA", INDATA, (result, Exception) =>
                {
                    HiddenLoadingIndicator();
                    if (Exception != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (result.Rows.Count > 0)
                        Util.GridSetData(dgRackList, result, FrameOperation, true);

                    SetGridComboItem_EMPTY_PLLT_RACK_FLAG(dgRackList.Columns["EMPTY_PLLT_RACK_FLAG"]);
                    SetGridComboItem_LOCATION_STATUS(dgRackList.Columns["HOLD_RACK_FLAG"]);
                    SetGridComboItem_MDL_MIX_ENABLE_RACK_FLAG(dgRackList.Columns["MDL_MIX_ENABLE_RACK_FLAG"]);



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


        void SetGridComboItem_EMPTY_PLLT_RACK_FLAG(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.TableName = "RQSTDT";
            DataRow Indata = IndataTable.NewRow();
            IndataTable.Rows.Add(Indata);

            DataTable dtCbo = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_LOCATION_TYPE", "RQSTDT", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtCbo);
        }

        void SetGridComboItem_LOCATION_STATUS(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.TableName = "RQSTDT";
            DataRow Indata = IndataTable.NewRow();
            IndataTable.Rows.Add(Indata);

            DataTable dtCbo = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_LOCATION_STAT", "RQSTDT", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtCbo);
        }

        void SetGridComboItem_MDL_MIX_ENABLE_RACK_FLAG(C1.WPF.DataGrid.DataGridColumn col)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.TableName = "RQSTDT";
            DataRow Indata = IndataTable.NewRow();
            IndataTable.Rows.Add(Indata);

            DataTable dtCbo = new ClientProxy().ExecuteServiceSync("DA_PRD_GET_COMBO_MODEL_MIX", "RQSTDT", "RSLTDT", IndataTable);

            (col as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtCbo);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (isValid())
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
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            
        }
        private void rowCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c))
                {
                    return;
                }
                InputRow = true;
            }
        }
        private void rowCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!InputRow)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("숫자만 입력가능합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                    {
                    });
                    return;
                }
                else
                {
                    btnAdd_Click(sender, e);
                }
            }
        }
        #endregion

        #region Mehod
        private void DataGrid01RowAdd(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void Set_PopUp_ProductID()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void cboShop_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }
        private void cboArea_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }
        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
        }
        private bool isValid()
        {
            bool bRet = false;
            DataTable dt = DataTableConverter.Convert(dgRackList.ItemsSource);
            int ii = -1;
            foreach (DataRow row in dt.Rows)
            {
                ii++;
                if (row["CHK"] != null && !row["CHK"].ToString().Equals(""))
                {
                    if (Convert.ToBoolean(row["CHK"]))
                    {
                        int bxCnt = int.Parse(row["BOXID_CNT"].ToString());
                        string _EMPTY_PLLT_RACK_FLAG = row["EMPTY_PLLT_RACK_FLAG"].ToString();
                        if (bxCnt > 0 && _EMPTY_PLLT_RACK_FLAG.Equals("Y"))   // 0일 경우만 Empty 처리
                        {
                            string _RACK_NAME = row["RACK_NAME"].ToString();
                            // WH_ID[% 1]에 Pallet 가 존재합니다. Empty 처리가 불가능합니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8551", _RACK_NAME), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                            });
                            C1.WPF.DataGrid.C1DataGrid dg = dgRackList;
                            (dgRackList.GetCell(ii, 0).Presenter.Content as CheckBox).IsChecked = false;
                            return bRet;
                        }

                        int mdCnt = int.Parse(row["MDLLOT_ID_CNT"].ToString());
                        string _MDL_MIX_ENABLE_RACK_FLAG = row["MDL_MIX_ENABLE_RACK_FLAG"].ToString();
                        if (mdCnt > 1 && _MDL_MIX_ENABLE_RACK_FLAG == "N")   // 2 이상일경우 혼입불가
                        {
                            string _RACK_NAME = row["RACK_NAME"].ToString();
                            // WH_ID[%1]은 이미 모델이 혼합되어 있어 변경 불가능합니다.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8552", _RACK_NAME), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                            });
                            C1.WPF.DataGrid.C1DataGrid dg = dgRackList;
                            (dgRackList.GetCell(ii, 0).Presenter.Content as CheckBox).IsChecked = false;
                            return bRet;
                        }
                    }
                }
            }

            bRet = true;
            return bRet;
        }
        private void SaveData()
        {
            ShowLoadingIndicator();

            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("RACK_ID", typeof(string));
                INDATA.Columns.Add("EMPTY_PLLT_RACK_FLAG", typeof(string));
                INDATA.Columns.Add("MDL_MIX_ENABLE_RACK_FLAG", typeof(string));
                INDATA.Columns.Add("HOLD_RACK_FLAG", typeof(string));
                INDATA.Columns.Add("MAX_LOAD_QTY", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                
                DataTable dt = DataTableConverter.Convert(dgRackList.ItemsSource);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["CHK"] != null && !dr["CHK"].ToString().Equals(""))
                    {
                        if (Convert.ToBoolean(dr["CHK"]))
                        {
                            DataRow newRow = INDATA.NewRow();
                            newRow["RACK_ID"] = dr["RACK_ID"];
                            newRow["EMPTY_PLLT_RACK_FLAG"] = dr["EMPTY_PLLT_RACK_FLAG"];
                            newRow["MDL_MIX_ENABLE_RACK_FLAG"] = dr["MDL_MIX_ENABLE_RACK_FLAG"];
                            newRow["HOLD_RACK_FLAG"] = dr["HOLD_RACK_FLAG"];
                            newRow["MAX_LOAD_QTY"] = dr["MAX_LOAD_QTY"];
                            newRow["USERID"] = LoginInfo.USERID;
                            INDATA.Rows.Add(newRow);
                        }
                    }
                }


                new ClientProxy().ExecuteService("BR_PRD_UPD_TB_MMD_RACK_ATTR", "INDATA", null, INDATA, (result, bizex) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizex != null)
                        {
                            Util.MessageException(bizex);
                            return;
                        }

                        
                        Util.MessageValidation("SFU1270");  // 정상처리되었습니다. 

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                        // InitializeControls();
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
                Util.MessageException(ex);
            }

        }

        private void InitializeControls()
        {
            Util.gridClear(dgRackList);

            GetPalletByLocation();
        }
        #endregion

        private void dgRackList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            /*
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(dg.CurrentColumn.Name).ToString();

            Util.Alert(colName);
            */
        }


        private void dgRackList_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(dg.CurrentColumn.Name).ToString();
            if (dg.CurrentRow != null)
            {
                //DataRowView row = dg.CurrentRow.DataItem as DataRowView;
                
                /*
                if (colName.Equals("EMPTY_PLLT_RACK_FLAG") && int.Parse(row["BOXID_CNT"].ToString()) > 0)
                {
                    
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("Pallet 가 있으므로 Empty 변경이 불가합니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (Result) =>
                    {
                        DataTableConverter.SetValue(dg.CurrentRow.DataItem, colName, "N");
                    });
                    
                    (dg.GetCell(dg.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = false;
                    return;
                }
                */
                (dg.GetCell(dg.CurrentRow.Index, 0).Presenter.Content as CheckBox).IsChecked = true;
            }
        }

        private void dgRackList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dg = sender as C1.WPF.DataGrid.C1DataGrid;
            String colName = Util.NVC(e.Cell.Presenter.Cell.Column.Name).ToString();
            if (e.Cell.Presenter.Cell.Row != null)
            {
                DataRowView row = e.Cell.Presenter.Cell.Row.DataItem as DataRowView;
                string boxCount = row["BOXID_CNT"].ToString();
                if (colName.Equals("EMPTY_PLLT_RACK_FLAG") && int.Parse(boxCount) > 0)
                {
                    var chb = e.Cell.Presenter.Content as TextBlock;
                    chb.Visibility = Visibility.Collapsed;
                    //chb.IsEnabled = false;
                    //e.Cell.Presenter.Cell.IsEditable = false;
                }
            }
        }
    }
}
