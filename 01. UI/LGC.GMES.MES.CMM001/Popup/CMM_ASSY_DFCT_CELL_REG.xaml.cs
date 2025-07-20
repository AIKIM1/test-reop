/*************************************************************************************
 Created Date : 2019.05.31
      Creator : INS 김동일K
   Decription : CWA3동 증설 - 조립 공정 공통 - 불량 CELL 관리
--------------------------------------------------------------------------------------
 [Change History]
  2019.05.31  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ASSY_DFCT_CELL_REG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_DFCT_CELL_REG : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        string _EqptID = string.Empty;
        string _DfctCode = string.Empty;
        string _Line = string.Empty;
        string _ActID = string.Empty;
        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_DFCT_CELL_REG()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeControls();
            SetParameters();
            SetControl();
            SearchProcess();
            Loaded -= C1Window_Loaded;

        }

        private void InitializeControls()
        {
        }

        private void SetParameters()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _Line = Util.NVC(tmps[2]);
            _EqptID = Util.NVC(tmps[3]);
            _DfctCode = Util.NVC(tmps[4]);
            _ActID = Util.NVC(tmps[5]);
            txtDfctName.Text = Util.NVC(tmps[6]);

            DataTable dtTmp = new DataTable();
            dtTmp.Columns.Add("CBO_NAME", typeof(string));
            dtTmp.Columns.Add("CBO_CODE", typeof(string));

            if (tmps[0].GetType() == typeof(Dictionary<string, string>))
            {
                foreach (var pair in (tmps[0] as Dictionary<string, string>))
                {
                    DataRow dr = dtTmp.NewRow();
                    dr["CBO_NAME"] = pair.Key;
                    dr["CBO_CODE"] = pair.Value;
                    dtTmp.Rows.Add(dr);
                }
            }
            else
            {
                DataRow dr = dtTmp.NewRow();
                dr["CBO_NAME"] = Util.NVC(tmps[0]);
                dr["CBO_CODE"] = Util.NVC(tmps[1]);
                dtTmp.Rows.Add(dr);
            }
            
            cboLot.DisplayMemberPath = "CBO_NAME";
            cboLot.SelectedValuePath = "CBO_CODE";

            cboLot.ItemsSource = dtTmp.Copy().AsDataView();

            cboLot.SelectedIndex = 0;
        }

        private void SetControl()
        {
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //if (CommonVerify.HasDataGridRow(dgList))
            //{
            //    DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

            //    var queryEdit = (from t in dt.AsEnumerable()
            //                     where t.RowState == DataRowState.Added
            //                     select t).ToList();

            //    if (queryEdit.Any())
            //    {
            //        // 저장하지 않은 정보가 존재 합니다. 팝업을 종료 하시겠습니까?
            //        Util.MessageValidation("");
            //        return;
            //    }
            //}

            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtCellID_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtCellID == null) return;
                InputMethod.SetPreferredImeConversionMode(txtCellID, ImeConversionModeValues.Alphanumeric);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellID_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    try
                    {
                        //loadingIndicator2.Visibility = Visibility.Visible;

                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.None);

                        foreach (string item in sPasteStrings)
                        {
                            KeyEventArgs args = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Enter);
                            this.Dispatcher.BeginInvoke(new Action(() => txtCellID_KeyDown(item, args)));
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        //loadingIndicator2.Visibility = Visibility.Collapsed;
                        return;
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtCellID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    string sCellid = string.Empty;
                    string sCstID = string.Empty;
                    string sCstID2 = string.Empty;

                    try
                    {
                        if (sender.GetType() == typeof(string))
                        {
                            sCellid = sender.ToString();
                        }
                        else
                        {
                            sCellid = txtCellID.Text.Trim();
                        }

                        if (sCellid == "")
                        {
                            Util.MessageValidation("SFU2060");   //스캔한 데이터가 없습니다.
                            return;
                        }

                        DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                        #region [Scan Validation]                        
                        // 동일 Lot
                        DataRow[] dtRows = dt.Select("SCAN_ID = '" + sCellid + "' AND ADDYN = 'Y'");
                        if (dtRows?.Length > 0)
                        {
                            //동일한 LOT[%1]이 스캔되었습니다.
                            Util.MessageValidation("SFU4951", sCellid);
                            txtCellID.Text = "";
                            //txtCellID.Focus();
                            return;
                        }
                        #endregion

                        DataRow dr = dt.NewRow();

                        if (dt.Columns.Contains("ADDYN"))
                            dr["ADDYN"] = "Y";
                        if (dt.Columns.Contains("ROWNUM"))
                            dr["ROWNUM"] = dt.Rows.Count + 1;
                        if (dt.Columns.Contains("CELL_ID"))
                            dr["CELL_ID"] = sCellid;
                        if (dt.Columns.Contains("SCAN_ID"))
                            dr["SCAN_ID"] = sCellid;
                        if (dt.Columns.Contains("CLCT_DTTM"))
                            dr["CLCT_DTTM"] = "";
                        if (dt.Columns.Contains("CELL_CHK_CODE"))
                            dr["CELL_CHK_CODE"] = "";
                        if (dt.Columns.Contains("DFCT_QTY"))
                            dr["DFCT_QTY"] = "1";
                        if (dt.Columns.Contains("SRCTYPE"))
                            dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        if (dt.Columns.Contains("CLCT_DTTM2"))
                            dr["CLCT_DTTM2"] = DateTime.Now;

                        dt.Rows.Add(dr);

                        Util.GridSetData(dgList, dt, FrameOperation);

                        dgList.ScrollIntoView(dt.Rows.Count - 1, 0);

                        txtCellID.Text = "";
                        txtCellID.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex, (result) =>
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;

                            txtCellID.SelectAll();
                            txtCellID.Focus();
                        });

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex, (result) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    txtCellID.SelectAll();
                    txtCellID.Focus();
                });
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button bt = sender as Button;

                if (bt == null || bt.DataContext == null) return;

                if (string.Equals(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "SRCTYPE")), SRCTYPE.SRCTYPE_UI))
                {
                    Util.MessageConfirm("SFU1230", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            DeleteCell(Util.NVC(DataTableConverter.GetValue(bt.DataContext, "CELL_ID")), (DateTime)DataTableConverter.GetValue(bt.DataContext, "CLCT_DTTM2"));

                            DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                            string sIdx = Util.NVC(DataTableConverter.GetValue(bt.DataContext, "ROWNUM"));
                            int idx = 0;
                            int.TryParse(sIdx, out idx);

                            if (idx < 1) return;

                            dt.Rows.RemoveAt(idx - 1);

                            Util.GridSetData(dgList, dt, FrameOperation);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.GetRowCount() < 1)
            {
                // 저장할 DATA가 없습니다.
                Util.MessageValidation("SFU3552");
                HideLoadingIndicator();
                return;
            }

            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        DateTime dtSysTime = GetSystemTime();

                        ShowLoadingIndicator();
                        
                        DataTable inDataTable = new DataTable();
                        inDataTable.Columns.Add("CELL_ID", typeof(string));
                        inDataTable.Columns.Add("CLCT_DTTM", typeof(DateTime));
                        inDataTable.Columns.Add("PROD_LOTID", typeof(string));
                        inDataTable.Columns.Add("EQPTID", typeof(string));
                        inDataTable.Columns.Add("EQPT_DFCT_CODE", typeof(string));
                        inDataTable.Columns.Add("PORT_ID", typeof(string));
                        inDataTable.Columns.Add("CELL_CHK_CODE", typeof(string));
                        inDataTable.Columns.Add("SCAN_ID", typeof(string));
                        inDataTable.Columns.Add("SRCTYPE", typeof(string));
                        inDataTable.Columns.Add("INSUSER", typeof(string));
                        inDataTable.Columns.Add("INSDTTM", typeof(DateTime));
                        inDataTable.Columns.Add("UPDUSER", typeof(string));
                        inDataTable.Columns.Add("UPDDTTM", typeof(DateTime));
                        inDataTable.Columns.Add("ACTID", typeof(string));
                        inDataTable.Columns.Add("RESNCODE", typeof(string));
                        
                        for (int i = 0; i < dgList.Rows.Count - dgList.BottomRows.Count; i++)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "ADDYN")).Equals("Y"))
                            {
                                DataRow newRow = inDataTable.NewRow();

                                newRow["CELL_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SCAN_ID"));
                                newRow["CLCT_DTTM"] = dtSysTime;
                                newRow["PROD_LOTID"] = cboLot.Text;
                                newRow["EQPTID"] = _EqptID;
                                newRow["EQPT_DFCT_CODE"] = null;
                                newRow["PORT_ID"] = null;
                                newRow["CELL_CHK_CODE"] = "OK";
                                newRow["SCAN_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SCAN_ID"));
                                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                                newRow["INSUSER"] = LoginInfo.USERID;
                                newRow["INSDTTM"] = dtSysTime;
                                newRow["UPDUSER"] = LoginInfo.USERID; 
                                newRow["UPDDTTM"] = dtSysTime;
                                newRow["ACTID"] = _ActID;
                                newRow["RESNCODE"] = _DfctCode;

                                inDataTable.Rows.Add(newRow);
                            }
                        }

                        if (inDataTable.Rows.Count < 1)
                        {
                            // 저장할 DATA가 없습니다.
                            Util.MessageValidation("SFU3552");
                            HideLoadingIndicator();
                            return;
                        }

                        new ClientProxy().ExecuteService("DA_BAS_INS_TB_SFC_EQPT_CELL_DFCT_CLCT_HIST", "IN_EQP,IN_CELL", null, inDataTable, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    Util.MessageException(bizException);
                                    return;
                                }

                                Util.MessageInfo("SFU1275");

                                this.DialogResult = MessageBoxResult.OK;
                            }
                            catch (Exception ex)
                            {
                                Util.MessageException(ex);
                            }
                            finally
                            {
                                HideLoadingIndicator();
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }
            });
        }

        private void dgList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (!Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "SRCTYPE")).Equals(SRCTYPE.SRCTYPE_UI))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#D4D4D4"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;// new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E6F5FB"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void dgList_UnloadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        //e.Row.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                    }
                }
            }));
        }

        private void cboLot_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (sender == null) return;

            SearchProcess();
        }
        #endregion

        #region Mehod

        /// <summary>
        /// Cell 조회
        /// </summary>
        private void SearchProcess()
        {
            try
            {
                ShowLoadingIndicator();
                const string bizRuleName = "DA_BAS_SEL_TB_SFC_EQPT_CELL_DFCT_CLCT_HIST_INFO_L";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("ACTID", typeof(string));
                dtRqst.Columns.Add("RESNCODE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = cboLot.Text;
                dr["EQPTID"] = _EqptID;
                dr["ACTID"] = _ActID;
                dr["RESNCODE"] = _DfctCode;
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", dtRqst, (bizResult, bizException) =>
                {
                    HideLoadingIndicator();

                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, FrameOperation);

                    //for (int i = 0; i < dgList.Rows.Count; i++)
                    //{
                    //    if (!Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SRCTYPE")).Equals(SRCTYPE.SRCTYPE_UI))
                    //    {
                    //        dgList.Rows[i].
                    //    }
                    //}

                    InputMethod.SetPreferredImeConversionMode(txtCellID, ImeConversionModeValues.Alphanumeric);
                    txtCellID.Focus();
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void DeleteCell(string sCellID, DateTime sDttm)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CELL_ID", typeof(string));
                inTable.Columns.Add("CLCT_DTTM", typeof(DateTime));
                inTable.Columns.Add("DEL_FLAG", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
    
                DataRow newRow = inTable.NewRow();
                newRow["CELL_ID"] = sCellID;
                newRow["CLCT_DTTM"] = sDttm;
                newRow["DEL_FLAG"] = "Y";
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService("DA_BAS_UPD_TB_SFC_EQPT_CELL_DFCT_CLCT_HIST", "INDATA", null, inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275", (action) =>
                        {
                            InputMethod.SetPreferredImeConversionMode(txtCellID, ImeConversionModeValues.Alphanumeric);
                            txtCellID.Focus();
                        });
                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        #endregion

        #region [Func]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion
                
    }
}
