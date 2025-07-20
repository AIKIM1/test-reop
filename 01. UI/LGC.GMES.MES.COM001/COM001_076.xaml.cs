/*************************************************************************************
 Created Date : 2017.05.16
      Creator : 
   Decription : 동-공정 별 로스내역 등록
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.14  DEVELOPER : Initial Created.
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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_076 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public COM001_076()
        {
            InitializeComponent();
           // InitGrid();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;

            InitCombo();
            InitGrid();
        }

        private void InitGrid()
        {
            Util.gridClear(dgSaveList);
            Util.gridClear(dgLossList);
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbParent: cboProcessParent);

        }

        #endregion

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchData();
        }
        private void btnUp_Click(object sender, RoutedEventArgs e)
        {

            if (dgSaveList.ItemsSource == null) return;
            if (dgSaveList.GetRowCount() == 0) return;
            if (dgSaveList.SelectedIndex == -1) return;

            int currentidx = dgSaveList.SelectedIndex;
            int preidx = currentidx - 1;
            if (preidx == -1) return;
            string tmp;
            



            for (int i = 0; i < dgSaveList.Columns.Count; i++)
            {
                tmp = dgSaveList.GetCell(currentidx - 1, i).Value.ToString();
                dgSaveList.GetCell(currentidx - 1, i).Value = dgSaveList.GetCell(currentidx, i).Value;
                dgSaveList.GetCell(currentidx, i).Value = tmp;


            }

            dgSaveList.SelectedIndex = currentidx - 1;
        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            if (dgSaveList.ItemsSource == null) return;
            if (dgSaveList.GetRowCount() == 0) return;
            if (dgSaveList.SelectedIndex == -1) return;
            

            int currentidx = dgSaveList.SelectedIndex;
            int nextidx = currentidx + 1;
            if (nextidx == dgSaveList.GetRowCount()) return;
            string tmp;


            for (int i = 0; i < dgSaveList.Columns.Count; i++)
            {
                tmp = dgSaveList.GetCell(currentidx + 1, i).Value.ToString();
                dgSaveList.GetCell(currentidx + 1, i).Value = dgSaveList.GetCell(currentidx, i).Value;
                dgSaveList.GetCell(currentidx, i).Value = tmp;
            }

            dgSaveList.SelectedIndex = currentidx + 1;
        }
        private void dgLossList_MergingCells(object sender, C1.WPF.DataGrid.DataGridMergingCellsEventArgs e)
        {
            if (dgLossList.ItemsSource == null) return;
            if (dgLossList.GetRowCount() == 0) return;

            string UPPR_LOSS_CODE = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[0].DataItem, "UPPR_LOSS_CODE"));
            string LOSS_CODE = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[0].DataItem, "LOSS_CODE"));

            if (UPPR_LOSS_CODE.Equals("") || LOSS_CODE.Equals("")) return;

            int idx = 0;

            for (int i = 0; i < dgLossList.GetRowCount(); i++)
            {
                if (i >= idx)
                {
                    if (!UPPR_LOSS_CODE.Equals(Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "UPPR_LOSS_CODE"))) || !LOSS_CODE.Equals(Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_CODE"))) || i == dgLossList.GetRowCount() - 1)
                    {
                        UPPR_LOSS_CODE = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "UPPR_LOSS_CODE"));
                        LOSS_CODE = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_CODE"));
                        for (int j = 3; j < 5; j++)
                        {
                            e.Merge(new DataGridCellsRange(dgLossList.GetCell(idx, j), dgLossList.GetCell((i == dgLossList.GetRowCount() - 1 ? i : i - 1), j)));
                        }

                        idx = i;

                    }
                }


            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender == null) return;

            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)(sender as CheckBox).Parent).Row.Index;

            if (DataTableConverter.Convert(dgSaveList.ItemsSource).Select("LOSS_DETL_CODE = '" + Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[idx].DataItem, "LOSS_DETL_CODE")) + "'").Length == 1)
            {
                Util.MessageValidation("SFU3471", Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[idx].DataItem, "LOSS_DETL_NAME")));
                DataTableConverter.SetValue(dgLossList.Rows[idx].DataItem, "CHK", 0);
                return;
            }
        }
        private void btnSortNumSave_Click(object sender, RoutedEventArgs e)
        {
            if (dgSaveList.ItemsSource == null) return;
            if (dgSaveList.GetRowCount() == 0) return;

            //순서를 변경하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3472"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {


                    DataSet ds = new DataSet();
                    DataTable indata = ds.Tables.Add("INDATA");

                    indata.Columns.Add("EQSGID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));
                    indata.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    indata.Columns.Add("SORT_NO", typeof(int));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("USE_FLAG", typeof(string));


                    DataRow row = null;

                    for (int i = 0; i < dgSaveList.GetRowCount(); i++)
                    {

                        row = indata.NewRow();
                        row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                        row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                        row["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSaveList.Rows[i].DataItem, "LOSS_DETL_CODE"));
                        row["SORT_NO"] = i + 1;
                        row["USERID"] = LoginInfo.USERID;
                        row["USE_FLAG"] = "Y";
                        indata.Rows.Add(row);


                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_AREA_PROC_UPD_LOSS_DETL", "INDATA", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU3473");//순서 변경 완료

                            SearchData();



                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, ds);
                }
            });
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgSaveList.ItemsSource == null) return;
            if (dgSaveList.GetRowCount() == 0) return;

            if (dgSaveList.SelectedIndex == -1)
            {
                Util.MessageValidation("SFU3474"); //삭제 할 부동내역 미선택
                return;
            }

            int idx = dgSaveList.SelectedIndex;

            //[%1]를 삭제하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3475", Util.NVC(DataTableConverter.GetValue(dgSaveList.Rows[idx].DataItem, "LOSS_DETL_NAME"))), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {

                    DataSet ds = new DataSet();
                    DataTable indata = ds.Tables.Add("INDATA");

                    indata.Columns.Add("EQSGID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));
                    indata.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    indata.Columns.Add("SORT_NO", typeof(int));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("USE_FLAG", typeof(string));

                    DataRow row = null;
                    for (int i = idx; i < dgSaveList.GetRowCount(); i++)
                    {
                        row = indata.NewRow();
                        row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                        row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                        row["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgSaveList.Rows[i].DataItem, "LOSS_DETL_CODE"));
                        row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                        row["SORT_NO"] = i == idx ?  -1 : i;
                        row["USERID"] = LoginInfo.USERID;
                        row["USE_FLAG"] = i== idx ? "N" : "Y";
                        indata.Rows.Add(row);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_AREA_PROC_UPD_LOSS_DETL", "INDATA", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1273"); //삭제되었습니다

                            SearchData();



                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, ds);
                }
            });
        }


        #endregion

        #region Mehod
        private void SearchData()
        {
            GetSavedLossList();
            GetLossList();
        }
        private void GetSavedLossList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQSGID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("USE_FLAG", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["USE_FLAG"] = "Y";
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_DETL_AREA_PROC", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    Util.GridSetData(dgSaveList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void GetLossList()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("USERID", typeof(string));
                dt.Columns.Add("AREAID", typeof(string));
                dt.Columns.Add("PROCID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow row = dt.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["USERID"] = LoginInfo.USERID;
                row["AREAID"] = Convert.ToString(cboArea.SelectedValue);
                row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                row["EQPTID"] = null;
                dt.Rows.Add(row);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_EQPTLOSS_LOSS_DETL_CODE", "RQSTDT", "RSLTDT", dt);

                if (result != null)
                {
                    Util.GridSetData(dgLossList, result, FrameOperation, true);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }




        #endregion

        private void btnLossSave_Click(object sender, RoutedEventArgs e)
        {
            if (_Util.GetDataGridCheckFirstRowIndex(dgLossList, "CHK") == -1)
            {
                Util.MessageValidation("SFU1636"); //선택된 대상이 없습니다. 
                return;
            }

            //부동내역을 등록하시겠습니까? 
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3476"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {

                if (result.ToString().Equals("OK"))
                {
                    DataSet ds = new DataSet();
                    DataTable indata = ds.Tables.Add("INDATA");

                    indata.Columns.Add("EQSGID", typeof(string));
                    indata.Columns.Add("PROCID", typeof(string));
                    indata.Columns.Add("LOSS_DETL_CODE", typeof(string));
                    indata.Columns.Add("USERID", typeof(string));
                    indata.Columns.Add("USE_FLAG", typeof(string));

                    DataRow row = null;

                    for (int i = 0; i < dgLossList.GetRowCount(); i++)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "CHK")).Equals("1") || Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "CHK")).Equals("True"))
                        {
                            row = indata.NewRow();
                            row["EQSGID"] = Convert.ToString(cboEquipmentSegment.SelectedValue);
                            row["PROCID"] = Convert.ToString(cboProcess.SelectedValue);
                            row["LOSS_DETL_CODE"] = Util.NVC(DataTableConverter.GetValue(dgLossList.Rows[i].DataItem, "LOSS_DETL_CODE"));
                            row["USERID"] = LoginInfo.USERID;
                            row["USE_FLAG"] = "Y";
                            indata.Rows.Add(row);

                        }
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_EQPT_EQPTLOSS_AREA_PROC_REG_LOSS_DETL", "INDATA", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            Util.MessageInfo("SFU1518"); //등록하였습니다 

                            SearchData();



                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        finally
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                        }
                    }, ds);

                }

            });
        }

   
    }
}
