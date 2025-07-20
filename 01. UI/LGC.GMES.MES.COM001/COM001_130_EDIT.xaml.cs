/*************************************************************************************
 Created Date : 2018.07.24
      Creator : INS 김동일K
   Decription : 전지 GMES 고도화 - 조립 선별일지 저장 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.07.24  INS 김동일K : Initial Created.

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_130_EDIT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_130_EDIT : C1Window, IWorkArea
    {
        private string _KeyValue = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_130_EDIT()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                _KeyValue = Util.NVC(tmps[0]);
                //_eqsgNote = Util.NVC(tmps[1]);
                //_eqsgId = Util.NVC(tmps[2]);
                //_wrkDate = Util.NVC(tmps[3]);

                InitCombo();

                InitControls();

                if (_KeyValue.Equals(""))
                {

                    DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                    DataRow dr = dt.NewRow();
                    //if (dt.Columns.Contains("CHK"))
                    //{
                    //    dr["CHK"] = true;
                    //}
                    dt.Rows.Add(dr);

                    Util.GridSetData(dgList, dt, FrameOperation, false);
                }
                else
                {
                    btnAdd.Visibility = Visibility.Collapsed;
                    btnDelete.Visibility = Visibility.Collapsed;

                    GetList();
                }

                this.Loaded -= C1Window_Loaded;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                {
                    //Util.Alert("라인을 선택 하세요.");
                    Util.MessageValidation("SFU1223");
                    return;
                }
                
                SaveData();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void dgList_BeganEdit(object sender, C1.WPF.DataGrid.DataGridBeganEditEventArgs e)
        {
            try
            {
                if (e.Column.Name.Equals("PICK_TRGT_CODE") ||
                    e.Column.Name.Equals("PICK_UNIT") ||
                    e.Column.Name.Equals("DFCT_TRGT_NAME") ||
                    e.Column.Name.Equals("DFCT_UNIT"))
                {
                    var combo = e.EditingElement as C1ComboBox;

                    string sCmcdType = string.Empty;
                    if (e.Column.Name.Equals("SHFT_ID"))
                        sCmcdType = "SELECTION_SHFT_TYPE_CODE";
                    else if (e.Column.Name.Equals("PICK_TRGT_CODE") || e.Column.Name.Equals("DFCT_TRGT_NAME"))
                        sCmcdType = "SELECTION_TARGET_CODE";
                    else if (e.Column.Name.Equals("PICK_UNIT") || e.Column.Name.Equals("DFCT_UNIT"))
                        sCmcdType = "SELECTION_UNIT_CODE";


                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("CMCDTYPE", typeof(string));

                    DataRow tmpDataRow = inDataTable.NewRow();
                    tmpDataRow["LANGID"] = LoginInfo.LANGID;
                    tmpDataRow["CMCDTYPE"] = sCmcdType;

                    inDataTable.Rows.Add(tmpDataRow);
                    // 작업조
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "INDATA", "RSLTDT", inDataTable);

                    combo.ItemsSource = DataTableConverter.Convert(dtRslt.Copy());
                }
                else if (e.Column.Name.Equals("SHFT_ID"))
                {
                    var combo = e.EditingElement as C1ComboBox;
                    
                    DataTable inDataTable = new DataTable();
                    inDataTable.Columns.Add("LANGID", typeof(string));
                    inDataTable.Columns.Add("SHOPID", typeof(string));
                    inDataTable.Columns.Add("AREAID", typeof(string));
                    inDataTable.Columns.Add("EQSGID", typeof(string));
                    inDataTable.Columns.Add("PROCID", typeof(string));

                    DataRow tmpDataRow = inDataTable.NewRow();
                    tmpDataRow["LANGID"] = LoginInfo.LANGID;
                    tmpDataRow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                    tmpDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                    tmpDataRow["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue).Equals("") ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                    tmpDataRow["PROCID"] = Process.LAMINATION + "," + Process.STACKING_FOLDING;

                    inDataTable.Rows.Add(tmpDataRow);
                    // 작업조
                    DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_SHIFT_USE_MULTI_PROCID_CBO", "INDATA", "RSLTDT", inDataTable);

                    combo.ItemsSource = DataTableConverter.Convert(dtRslt.Copy());
                    
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            try
            {
                if (e == null || e.Row == null || e.Row.DataItem == null || e.Column == null)
                    return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
                
                //if (!e.Column.Name.Equals("CHK"))
                //{
                //    if (Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).ToUpper().Equals("FALSE") ||
                //        Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).Equals("0") ||
                //        Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["CHK"].Index).Value).Equals(""))
                //        e.Cancel = true;
                //}

                if (e.Column.Name.Equals("SHFT_ID"))
                {
                    if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
                    {
                        //Util.Alert("라인을 선택 하세요.");
                        Util.MessageValidation("SFU1223");
                        e.Cancel = true;
                    }
                }
                else if (e.Column.Name.Equals("LOTID") ||
                         e.Column.Name.Equals("PRJT_NAME"))
                {
                    if (InputMethod.Current != null)
                        InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgList_CommittedEdit(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (e == null || e.Cell == null) return;

                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Column.Name.Equals("PRJT_NAME") ||
                    e.Cell.Column.Name.Equals("DFCT_TYPE_NAME") ||
                    e.Cell.Column.Name.Equals("LOTID") ||
                    e.Cell.Column.Name.Equals("NOTE")
                    )
                {
                    if (Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns[e.Cell.Column.Name].Index).Value).Length > 15)
                    {
                        Util.MessageValidation("SFU4964", "15");  // 입력오류 : %1 글자 이하로 입력하세요.
                        DataTableConverter.SetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, e.Cell.Column.Name, "");
                        return;
                    }
                }

                if (e.Cell.Column.Name.Equals("LOTID") && !Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["LOTID"].Index).Value).Equals(""))
                {
                    CheckLotInfo(Util.NVC(dataGrid.GetCell(e.Cell.Row.Index, dataGrid.Columns["LOTID"].Index).Value), e.Cell.Row.Index);
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { LoginInfo.CFG_AREA_ID };   
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);
        }

        private void InitControls()
        {
            try
            {
                DataTable dt = new DataTable();
                for (int i = 0; i < dgList.Columns.Count; i++)
                {
                    dt.Columns.Add(dgList.Columns[i].Name);
                }

                //DataRow dr = dt.NewRow();
                //dt.Rows.Add(dr);
                Util.GridSetData(dgList, dt, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        public void GetList()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("SHOPID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("HIST_SEQNO", typeof(string));
            
                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["HIST_SEQNO"] = _KeyValue;
                
                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SELECTION_DIARY_LIST", "INDATA", "OUTDATA", dtRqst, (result, bizEx) =>
                {
                    try
                    {
                        if (bizEx != null)
                        {
                            Util.MessageException(bizEx);
                            return;
                        }

                        Util.GridSetData(dgList, result, FrameOperation, true);

                        if (result.Columns.Contains("WRK_DATE"))
                        {
                            DateTime dtTmp;
                            if (DateTime.TryParse(Util.NVC(result.Rows[0]["WRK_DATE"]), out dtTmp))
                                wrkTime.SelectedDateTime = dtTmp;
                        }

                        if (result.Columns.Contains("EQSGID"))
                        {
                            if (cboEquipmentSegment?.Items?.Count > 0)
                            {
                                cboEquipmentSegment.SelectedValue = Util.NVC(result.Rows[0]["EQSGID"]);
                                if (cboEquipmentSegment.SelectedIndex < 0)
                                {
                                    cboEquipmentSegment.SelectedIndex = 0;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveData()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";

                RQSTDT.Columns.Add("HIST_SEQNO", typeof(string));
                RQSTDT.Columns.Add("WRK_DATE", typeof(string));
                RQSTDT.Columns.Add("SHFT_ID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PRJT_NAME", typeof(string));
                RQSTDT.Columns.Add("DFCT_TYPE_NAME", typeof(string));
                RQSTDT.Columns.Add("PICK_TRGT_CODE", typeof(string));
                RQSTDT.Columns.Add("PICK_QTY", typeof(decimal));
                RQSTDT.Columns.Add("PICK_UNIT", typeof(string));
                RQSTDT.Columns.Add("PICK_STRT_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("PICK_END_DTTM", typeof(DateTime));
                RQSTDT.Columns.Add("PICK_WRKR_NUM", typeof(decimal));
                RQSTDT.Columns.Add("DFCT_TRGT_NAME", typeof(string));
                RQSTDT.Columns.Add("DFCT_QTY", typeof(decimal));
                RQSTDT.Columns.Add("OTH_DFCT_QTY", typeof(decimal));
                RQSTDT.Columns.Add("DFCT_UNIT", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("NOTE", typeof(string));
                RQSTDT.Columns.Add("DEL_FLAG", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = null;

                for (int i = 0; i < dgList.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SHFT_ID")).Equals(""))
                    {
                        Util.MessageValidation("SFU1845"); // 작업조를 입력하세요.
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        return;
                    }

                    //int iCheck = DateTime.Compare(Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_END_DTTM"))), 
                    //    Convert.ToDateTime(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_STRT_DTTM"))));

                    //if (iCheck < 0)
                    //{
                    //    Util.MessageValidation("");
                    //    loadingIndicator.Visibility = Visibility.Collapsed;
                    //    return;
                    //}

                    DateTime dateTimeST;
                    DateTime dateTimeED;
                    

                    if (!DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_STRT_DTTM")), out dateTimeST))
                    {
                        dateTimeST = new DateTime(wrkTime.SelectedDateTime.Year, wrkTime.SelectedDateTime.Month, wrkTime.SelectedDateTime.Day, 0, 0, 0, 0);
                    }

                    if (!DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_END_DTTM")), out dateTimeED))
                    {
                        dateTimeED = new DateTime(wrkTime.SelectedDateTime.Year, wrkTime.SelectedDateTime.Month, wrkTime.SelectedDateTime.Day, 0, 0, 0, 0);
                    }

                    //dateTimeST = dateTimeST.AddYears(-(dateTimeST.Year - 1)).AddMonths(-(dateTimeST.Month - 1)).AddDays(-(dateTimeST.Day - 1));
                    //dateTimeED = dateTimeED.AddYears(-(dateTimeED.Year - 1)).AddMonths(-(dateTimeED.Month - 1)).AddDays(-(dateTimeED.Day - 1));
                    dateTimeST = dateTimeST.AddYears(wrkTime.SelectedDateTime.Year - dateTimeST.Year).AddMonths(wrkTime.SelectedDateTime.Month - dateTimeST.Month).AddDays(wrkTime.SelectedDateTime.Day - dateTimeST.Day);
                    dateTimeED = dateTimeED.AddYears(wrkTime.SelectedDateTime.Year - dateTimeED.Year).AddMonths(wrkTime.SelectedDateTime.Month - dateTimeED.Month).AddDays(wrkTime.SelectedDateTime.Day - dateTimeED.Day);

                    dr = RQSTDT.NewRow();

                    dr["HIST_SEQNO"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "HIST_SEQNO")).Equals("") ? null : Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "HIST_SEQNO"));
                    dr["WRK_DATE"] = wrkTime.SelectedDateTime.ToString("yyyyMMdd");
                    dr["SHFT_ID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SHFT_ID"));
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue);
                    dr["PRJT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PRJT_NAME"));
                    dr["DFCT_TYPE_NAME"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DFCT_TYPE_NAME"));
                    dr["PICK_TRGT_CODE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_TRGT_CODE"));
                    dr["PICK_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_QTY")));
                    dr["PICK_UNIT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_UNIT"));
                    dr["PICK_STRT_DTTM"] = dateTimeST;
                    dr["PICK_END_DTTM"] = dateTimeED;
                    dr["PICK_WRKR_NUM"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_WRKR_NUM")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "PICK_WRKR_NUM")));
                    dr["DFCT_TRGT_NAME"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DFCT_TRGT_NAME"));
                    dr["DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DFCT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DFCT_QTY")));
                    dr["OTH_DFCT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OTH_DFCT_QTY")).Equals("") ? 0 : Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "OTH_DFCT_QTY")));
                    dr["DFCT_UNIT"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "DFCT_UNIT"));
                    dr["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "LOTID"));
                    dr["NOTE"] = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "NOTE"));
                    dr["DEL_FLAG"] = "N";
                    dr["USERID"] = LoginInfo.USERID;

                    RQSTDT.Rows.Add(dr);
                }

                if (RQSTDT.Rows.Count < 1)
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    return;
                }

                new ClientProxy().ExecuteService("BR_PRD_REG_PICK_WRK_HIST", "INDATA", "", RQSTDT, (result, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                        }
                        else
                        {
                            Util.MessageInfo("SFU1270");

                            AsynchronousClose();
                        }
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }            
        }

        private void AsynchronousClose()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void CheckLotInfo(string sLotID, int idx)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = sLotID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt?.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU1195");  //LOT 정보가 없습니다.
                    DataTableConverter.SetValue(dgList.Rows[idx].DataItem, "LOTID", "");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);

                Util.GridSetData(dgList, dt, FrameOperation);
                dgList.ScrollIntoView(dt.Rows.Count - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgList.ItemsSource);

                if (dt?.Rows?.Count > 0)
                {
                    dt.Rows.RemoveAt(dt.Rows.Count - 1);
                    Util.GridSetData(dgList, dt, FrameOperation);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
