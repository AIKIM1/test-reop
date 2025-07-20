/*************************************************************************************
 Created Date : 2018.01.29
      Creator : 이진선
   Decription : 투입바구니 실적 수정 POPUP
--------------------------------------------------------------------------------------
 [Change History]
  2018.01.29  이진선 : Initial Created.
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
using System.Windows.Input;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY001
{
    public partial class UC_IN_HIST_CREATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet biz = new BizDataSet();
        private CommonCombo combo = new CommonCombo();
        private Util util = new Util();

        private string equipmentSegment;
        private string eqptid;
        private string wo_detl_id;
        private string procid;
        private string caldate;
        private string shift;
        private string prod_id;

        private string _Max_Pre_Proc_End_Day;
        private DateTime _Min_Valid_Date;

        public UC_IN_HIST_CREATE()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        #endregion

        #region Initialize

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null)
            {
                equipmentSegment = Util.NVC(tmps[0]);
                eqptid = Util.NVC(tmps[1]);
                wo_detl_id = Util.NVC(tmps[2]);
                procid = Util.NVC(tmps[3]);
                shift = Util.NVC(tmps[4]);
                caldate = Util.NVC(tmps[5]);
                prod_id = Util.NVC(tmps[6]);

                InitCombo();
                GetProcMtrlInputRule();
                GetWaitBoxList();
            }

        }
        private void btnHistSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (util.GetDataGridCheckFirstRowIndex(dgWaitBox, "CHK") == -1)
                {
                    Util.MessageValidation("SFU1632"); //선택된 LOT이 없습니다.
                    return;
                }

                DataSet ds = new DataSet();

                DataTable inData = new DataTable();
                inData.TableName = "INDATA";

                inData.Columns.Add("SRCTYPE", typeof(string));
                inData.Columns.Add("IFMODE", typeof(string));
                inData.Columns.Add("CALDATE", typeof(DateTime));
                inData.Columns.Add("SHIFT", typeof(string));
                inData.Columns.Add("EQPTID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("RECIPEID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("ACTDTTM_ST", typeof(DateTime));
                inData.Columns.Add("ACTDTTM_ED", typeof(DateTime));
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("EQPT_MOUNT_PSTN_ID", typeof(string));


                DataRow dr = inData.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["IFMODE"] = IFMODE.IFMODE_OFF;
                dr["CALDATE"] = DateTime.Now; //caldate.Equals("") ? DateTime.Now : caldate;
                dr["SHIFT"] = shift;
                dr["EQPTID"] = eqptid;
                dr["PROCID"] = procid;
                dr["RECIPEID"] = null;
                dr["USERID"] = LoginInfo.USERID;
                dr["ACTDTTM_ST"] = DateTime.Now;
                dr["ACTDTTM_ED"] = DateTime.Now;
                dr["PROD_LOTID"] = prod_id;
                dr["EQPT_MOUNT_PSTN_ID"] = Convert.ToString(cboHistMountPstsID.SelectedValue);

                inData.Rows.Add(dr);
                ds.Tables.Add(inData);

                DataTable inLot = new DataTable();
                inLot.TableName = "INLOT";

                inLot.Columns.Add("LOTID", typeof(string));
                inLot.Columns.Add("HOTFLAG", typeof(string));
                inLot.Columns.Add("WIPNOTE", typeof(string));
                inLot.Columns.Add("BTCH_ORD_ID", typeof(string));

                DataRow dr2 = inLot.NewRow();
                dr2["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[util.GetDataGridCheckFirstRowIndex(dgWaitBox, "CHK")].DataItem, "LOTID"));
                dr2["HOTFLAG"] = null;
                dr2["WIPNOTE"] = null;
                dr2["BTCH_ORD_ID"] = null;

                inLot.Rows.Add(dr2);
                ds.Tables.Add(inLot);


                new ClientProxy().ExecuteService_Multi("BR_SM_REG_INPUT_LOT_AUTO_TERM", "INDATA,INLOT", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1973"); //투입완료되었습니다.
                        this.DialogResult = MessageBoxResult.OK;
                        // GetWaitBoxList();


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                      

                    }
                    finally
                    {
                        txtLotId.Text = "";
                    }

                }, ds);



            }
            catch (Exception ex) { }
        }
        private void dgWaitBox_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (Util.NVC(_Max_Pre_Proc_End_Day).Equals(""))
                    {
                        e.Cell.Presenter.Background = null;
                    }
                    else
                    {
                        int iDay = 0;
                        int.TryParse(_Max_Pre_Proc_End_Day, out iDay);

                        if (iDay > 0)
                        {
                            if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")).Equals(""))
                            {
                                e.Cell.Presenter.Background = null;
                            }
                            else
                            {
                                DateTime dtValid;
                                DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "VALID_DATE_YMDHMS")), out dtValid);

                                if (_Min_Valid_Date.AddDays(iDay) >= dtValid)
                                {
                                    e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));
                                }
                                else
                                {
                                    e.Cell.Presenter.Background = null;
                                }
                            }
                        }
                        else
                        {
                            e.Cell.Presenter.Background = null;
                        }
                    }
                }
            }));
        }

        private void dgWaitBox_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    }
                }
            }));
        }

        private void rdoBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }

                //row 색 바꾸기
                dgWaitBox.SelectedIndex = idx;
            }
        }
        private void txtLotId_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender == null)
                return;

            if (e.Key == Key.Enter)
            {


                for (int i = 0; i < dgWaitBox.GetRowCount(); i++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgWaitBox.Rows[i].DataItem, "LOTID")).Equals(txtLotId.Text))
                    {
                        DataTableConverter.SetValue(dgWaitBox.Rows[i].DataItem, "CHK", true);

                        dgWaitBox.SelectedIndex = i;
                        dgWaitBox.ScrollIntoView(i, 0);
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgWaitBox.Rows[i].DataItem, "CHK", false);
                    }
                }
            }
        }

        #endregion

        #region Mehod

        private void InitCombo()
        {
            string[] sFilter = { eqptid, "PROD" };
            combo.SetCombo(cboHistMountPstsID, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "EQPT_CURR_MOUNT_MTRL_CBO");
        }
        private void GetProcMtrlInputRule()
        {
            try
            {
                //ShowParentLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = procid;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROC_MTRL_INPUT_RULE", "INDATA", "OUTDATA", inTable);

                if (dtRslt != null && dtRslt.Rows.Count > 0 && dtRslt.Columns.Contains("MAX_PRE_PROC_END_DAY"))
                {
                    _Max_Pre_Proc_End_Day = Util.NVC(dtRslt.Rows[0]["MAX_PRE_PROC_END_DAY"]);
                }
            }
            catch (Exception ex)
            {
                //HiddenParentLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        private void GetWaitBoxList()
        {
            try
            {

                DataTable inTable = biz.GetDA_PRD_SEL_WAIT_LIST_CL();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PROCID"] = Process.PACKAGING;
                newRow["EQSGID"] = equipmentSegment;
                newRow["EQPTID"] = eqptid;
                newRow["WO_DETL_ID"] = wo_detl_id;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_WAIT_LOT_LIST_CL", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        // FIFO 기준 Date
                        if (searchResult != null && searchResult.Rows.Count > 0 && searchResult.Columns.Contains("VALID_DATE_YMDHMS"))
                        {
                            DateTime.TryParse(Util.NVC(searchResult.Rows[0]["VALID_DATE_YMDHMS"]), out _Min_Valid_Date);
                        }


                        Util.GridSetData(dgWaitBox, searchResult, FrameOperation);


                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                    }
                }
                );
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }






        #endregion


    }
}
