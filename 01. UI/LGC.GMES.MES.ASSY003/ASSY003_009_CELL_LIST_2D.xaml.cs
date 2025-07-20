/*************************************************************************************
 Created Date : 2020.10.27
      Creator : 주건태
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_009_CELL_LIST_2D.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_009_CELL_LIST_2D : C1Window, IWorkArea
    {
        private DataTable _bizResult = null;

        private int INT_MAX_X = 3;
        private int INT_MAX_Y = 4;
        private int INT_MAX_Z = 20;
        private int INT_BUCKET_COL = 1;

        private string _ProdID = string.Empty;
        private string _OutLotID = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ASSY003_009_CELL_LIST_2D()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 2)
            {
                txtProdID.Text = Util.NVC(tmps[0]);
                txtOutLotID.Text = Util.NVC(tmps[1]);
            }

            /*
            InitSlotInfoGridCell(INT_MAX_X, INT_MAX_Y);
            InitBucketInfoGridCell(INT_BUCKET_COL, INT_MAX_Z);
            InitializeTextBox();
            */

            GetSublotList(true);
        }

        private void InitializeTextBox(bool pIsInit)
        {
            txtCellQty.Value = 0;

            if(pIsInit == true)
            {
                txtCellPosX.Value = 0;
                txtCellPosY.Value = 0;
                txtCellPosZ.Value = 0;
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {
        }

        private void dgSlotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            var textBlock = e.Cell.Presenter.Content as TextBlock;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private void dgBucketInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            var textBlock = e.Cell.Presenter.Content as TextBlock;
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private void dgSlotInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Util.gridClear(dgInputLotInfo);
            SetLotTextBox();

            int selected_row = dgSlotInfo.CurrentRow.Index;
            int slotYIndex = INT_MAX_Y - selected_row;

            int selected_col = dgSlotInfo.CurrentColumn.Index;
            int slotXIndex = selected_col + 1;

            if (rdoFixZ.IsChecked == true)
            {
                txtCellPosX.Value = slotXIndex;
                txtCellPosY.Value = slotYIndex;

                GetInputLotList(Util.NVC(DataTableConverter.GetValue(dgSlotInfo.Rows[selected_row].DataItem, Util.NVC_DecimalStr(selected_col))));
            }
            else if (rdoFixXY.IsChecked == true)
            {
                txtCellPosX.Value = slotXIndex;
                txtCellPosY.Value = slotYIndex;
                txtCellPosZ.Value = 0;

                SetBucketGridData(slotXIndex, slotYIndex);
            }
        }

        private void SetLotTextBox()
        {
            txtProdID.Text = _ProdID;
            txtOutLotID.Text = _OutLotID;
        }

        private void SetLotGlobalVar()
        {
            _ProdID = txtProdID.Text.Trim();
            _OutLotID = txtOutLotID.Text.Trim();
        }

        private void dgBucketInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Util.gridClear(dgInputLotInfo);
            SetLotTextBox();

            int selected_row = dgBucketInfo.CurrentRow.Index;
            int bucketIndex = INT_MAX_Z - selected_row;

            int selected_col = dgBucketInfo.CurrentColumn.Index;

            if (rdoFixZ.IsChecked == true)
            {
                txtCellPosZ.Value = bucketIndex;
                txtCellPosX.Value = 0;
                txtCellPosY.Value = 0;

                SetSlotGridData(bucketIndex);
            }
            else if (rdoFixXY.IsChecked == true)
            {
                txtCellPosZ.Value = bucketIndex;

                GetInputLotList(Util.NVC(DataTableConverter.GetValue(dgBucketInfo.Rows[selected_row].DataItem, Util.NVC_DecimalStr(selected_col))));
            }
        }

        private void GetInputLotList(string pCell)
        {
            try
            {
                if (string.IsNullOrEmpty(pCell))
                {
                    return;
                }

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = pCell;
                dtRqst.Rows.Add(dr);

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTTRACE_STP_2D", "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgInputLotInfo, dtRslt, FrameOperation, true);
                
                /*
                new ClientProxy().ExecuteService("DA_PRD_SEL_LOTTRACE_STP_2D", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    dgInputLotInfo.ItemsSource = DataTableConverter.Convert(bizResult);
                });
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void InitSlotInfoGridCell(int pIntCol, int pIntRow)
        {
            DataTable dtCell = new DataTable();
            
            try
            {
                Util.gridClear(dgSlotInfo);

                //Column 생성
                for (int col = 0; col < pIntCol; col++)
                {
                    dtCell.Columns.Add(col.ToString());
                }

                //Row 생성
                for (int row = 0; row < pIntRow; row++)
                {
                    DataRow dr = dtCell.NewRow();
                    for (int col = 0; col < pIntCol; col++)
                    {
                        dr[col.ToString()] = string.Empty;
                    }
                    dtCell.Rows.Add(dr);
                }

                dgSlotInfo.ItemsSource = DataTableConverter.Convert(dtCell);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void InitBucketInfoGridCell(int pIntCol, int pIntRow)
        {
            DataTable dtCell = new DataTable();

            try
            {
                Util.gridClear(dgBucketInfo);

                //Column 생성
                for (int col = 0; col < pIntCol; col++)
                {
                    dtCell.Columns.Add(col.ToString());
                }

                //Row 생성
                for (int row = 0; row < pIntRow; row++)
                {
                    DataRow dr = dtCell.NewRow();
                    for (int col = 0; col < pIntCol; col++)
                    {
                        dr[col.ToString()] = string.Empty;
                    }
                    dtCell.Rows.Add(dr);
                }

                dgBucketInfo.ItemsSource = DataTableConverter.Convert(dtCell);
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

        private void GetSublotList(bool pIsInit)
        {
            try
            {
                if (string.IsNullOrEmpty(txtProdID.Text.Trim()) || string.IsNullOrEmpty(txtOutLotID.Text.Trim()))
                {
                    return;
                }

                InitSlotInfoGridCell(INT_MAX_X, INT_MAX_Y);
                InitBucketInfoGridCell(INT_BUCKET_COL, INT_MAX_Z);
                InitializeTextBox(pIsInit);
                Util.gridClear(dgInputLotInfo);

                DataTable dtRqst = new DataTable("RQSTDT");
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["PROD_LOTID"] = txtProdID.Text.Trim();
                dr["LOTID"] = txtOutLotID.Text.Trim();
                dtRqst.Rows.Add(dr);

                SetLotGlobalVar();

                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SUBLOT_LIST_STP_2D", "INDATA", "OUTDATA", dtRqst);

                _bizResult = dtRslt;
                txtCellQty.Value = _bizResult.Rows.Count;

                if(pIsInit == true)
                {
                    SetGridDataDefault();
                }
                else
                {
                    SetBucketGridData(Util.NVC_Int(txtCellPosX.Value), Util.NVC_Int(txtCellPosY.Value));
                    SetSlotGridData(Util.NVC_Int(txtCellPosZ.Value));
                }

                /*
                new ClientProxy().ExecuteService("DA_PRD_SEL_SUBLOT_LIST_STP_2D", "INDATA", "OUTDATA", dtRqst, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    _bizResult = bizResult;
                    txtCellQty.Value = _bizResult.Rows.Count;

                    SetGridDataDefault();
                });
                */
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetGridDataDefault()
        {

            if (rdoFixZ.IsChecked == true)
            {
                txtCellPosZ.Value = 1;
                txtCellPosX.Value = 0;
                txtCellPosY.Value = 0;
            }
            else if (rdoFixXY.IsChecked == true)
            {
                txtCellPosZ.Value = 0;
                txtCellPosX.Value = 1;
                txtCellPosY.Value = 1;
            }

            SetBucketGridData(1, 1); //다폴트로 첫번째 슬롯의 데이터 보여줌
            SetSlotGridData(1); //디폴트로 첫번째 버킷의 데이터를 보여줌
        }

        private void SetBucketGridData(int pIntX, int pIntY)
        {
            InitBucketInfoGridCell(INT_BUCKET_COL, INT_MAX_Z);

            if(rdoFixZ.IsChecked == true)
            {
                for (int inx = 1; inx <= INT_MAX_Z; inx++)
                {
                    int zCnt = Convert.ToInt16(_bizResult.Compute("Count(SUBLOTID)", "CSTSLOT_Z = " + inx ));
                    DataTableConverter.SetValue(dgBucketInfo.Rows[INT_MAX_Z - inx].DataItem, "0", Util.NVC_DecimalStr(zCnt));
                }
            }
            else if (rdoFixXY.IsChecked == true)
            {
                DataRow[] drBucket = _bizResult.Select("CSTSLOT_X = " + pIntX + " AND CSTSLOT_Y = " + pIntY);
                for (int inx = 0; inx < drBucket.Length; inx++)
                {
                    int z = Util.NVC_Int(drBucket[inx]["CSTSLOT_Z"]);
                    DataTableConverter.SetValue(dgBucketInfo.Rows[INT_MAX_Z - z].DataItem, "0", Util.NVC(drBucket[inx]["SUBLOTID"]));
                }
            }

        }

        private void SetSlotGridData(int pIntZ)
        {
            InitSlotInfoGridCell(INT_MAX_X, INT_MAX_Y);

            if (rdoFixZ.IsChecked == true)
            {
                DataRow[] drSlot = _bizResult.Select("CSTSLOT_Z = " + pIntZ);

                for (int inx = 0; inx < drSlot.Length; inx++)
                {
                    int x = Util.NVC_Int(drSlot[inx]["CSTSLOT_X"]);
                    int y = Util.NVC_Int(drSlot[inx]["CSTSLOT_Y"]);

                    DataTableConverter.SetValue(dgSlotInfo.Rows[INT_MAX_Y - y].DataItem, Util.NVC_DecimalStr(x - 1), Util.NVC(drSlot[inx]["SUBLOTID"]));
                }
            }
            else if (rdoFixXY.IsChecked == true)
            {
                for (int inxX = 1; inxX <= INT_MAX_X; inxX++)
                {
                    for (int inxY = 1; inxY <= INT_MAX_Y; inxY++)
                    {
                        int xyCnt = Convert.ToInt16(_bizResult.Compute("Count(SUBLOTID)", "CSTSLOT_X = " + inxX + " AND CSTSLOT_Y = " + inxY));
                        DataTableConverter.SetValue(dgSlotInfo.Rows[INT_MAX_Y - inxY].DataItem, (inxX - 1).ToString(), Util.NVC_DecimalStr(xyCnt));
                    }
                }
            }
        }

        private void rdoFixZ_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInputLotInfo);

            SetGridDataDefault();
        }

        private void rdoFixXY_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgInputLotInfo);

            SetGridDataDefault();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            /*
            InitSlotInfoGridCell(INT_MAX_X, INT_MAX_Y);
            InitBucketInfoGridCell(INT_BUCKET_COL, INT_MAX_Z);
            InitializeTextBox();
            */

            GetSublotList(true);
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
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDelSublot_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProdID.Text.Trim()) || string.IsNullOrEmpty(txtOutLotID.Text.Trim()))
            {
                return;
            }

            if (txtCellPosX.Value <= 0 || txtCellPosY.Value <= 0 || txtCellPosZ.Value <= 0)
            {
                return;
            }

            string cell = GetCell();
            if (string.IsNullOrEmpty(cell) == true)
            {
                return;
            }

            //[%1]를 삭제하시겠습니까?
            Util.MessageConfirm("SFU3475", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DelSublot(cell);
                }
            }, new object[] { cell });
            
        }

        private void btnRegSublot_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtProdID.Text.Trim()) || string.IsNullOrEmpty(txtOutLotID.Text.Trim()))
            {
                return;
            }

            if (txtCellPosX.Value <= 0 || txtCellPosY.Value <= 0 || txtCellPosZ.Value <= 0)
            {
                return;
            }

            string cell = GetCell();
            if (string.IsNullOrEmpty(cell) == false)
            {
                return;
            }

            cell = GetSublotId();
            //%1 (을)를 생성 하시겠습니까?
            Util.MessageConfirm("SFU4584", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RegSublot(cell);
                }
            }, new object[] { cell });
        }

        private void DelSublot(string pStrCell)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();


                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);


                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("PROD_LOTID", typeof(string));
                inSublotTable.Columns.Add("OUT_LOTID", typeof(string));
                inSublotTable.Columns.Add("CSTSLOT", typeof(decimal));

                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = pStrCell;
                newRow["PROD_LOTID"] = txtProdID.Text.Trim();
                newRow["OUT_LOTID"] = txtOutLotID.Text.Trim();
                newRow["CSTSLOT"] = GetCSTSLOT();
                inSublotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_DEL_SUBLOT_STP_2D", "INDATA,INSUBLOT", null, indataSet);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetSublotList(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void RegSublot(string pStrCell)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();


                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(newRow);


                DataTable inSublotTable = indataSet.Tables.Add("INSUBLOT");
                inSublotTable.Columns.Add("SUBLOTID", typeof(string));
                inSublotTable.Columns.Add("PROD_LOTID", typeof(string));
                inSublotTable.Columns.Add("OUT_LOTID", typeof(string));
                inSublotTable.Columns.Add("CSTSLOT", typeof(decimal));

                newRow = inSublotTable.NewRow();
                newRow["SUBLOTID"] = pStrCell;
                newRow["PROD_LOTID"] = txtProdID.Text.Trim();
                newRow["OUT_LOTID"] = txtOutLotID.Text.Trim();
                newRow["CSTSLOT"] = GetCSTSLOT();
                inSublotTable.Rows.Add(newRow);

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_SUBLOT_STP_2D", "INDATA,INSUBLOT", null, indataSet);

                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

                GetSublotList(false);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private string GetCell()
        {
            int selected_row = -1;
            int selected_col = -1;
            string cell = string.Empty;

            if (rdoFixZ.IsChecked == true)
            {
                selected_row = dgSlotInfo.CurrentRow.Index;
                selected_col = dgSlotInfo.CurrentColumn.Index;

                cell = Util.NVC(DataTableConverter.GetValue(dgSlotInfo.Rows[selected_row].DataItem, Util.NVC_DecimalStr(selected_col)));
            }
            else if (rdoFixXY.IsChecked == true)
            {
                selected_row = dgBucketInfo.CurrentRow.Index;
                cell = Util.NVC(DataTableConverter.GetValue(dgBucketInfo.Rows[selected_row].DataItem, Util.NVC_DecimalStr(0)));
            }

            return cell;
        }

        private string GetSublotId()
        {
            string sublotId = string.Empty;

            sublotId = txtProdID.Text.Trim() + txtOutLotID.Text.Trim() + GetCSTSLOT();

            return sublotId;
        }

        private decimal GetCSTSLOT()
        {
            decimal x = Util.NVC_Decimal(txtCellPosX.Value) * 1000;
            decimal y = Util.NVC_Decimal(txtCellPosY.Value) * 100;
            decimal z = Util.NVC_Decimal(txtCellPosZ.Value);

            return x + y + z;
        }

    }
}