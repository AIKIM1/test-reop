/*************************************************************************************
 Created Date : 2017.01.24
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - 조립 공정진척 화면 - LOT 종료취소 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.01.24  INS 김동일K : Initial Created.
  2020.05.27  김동일 : C20200513-000349 재고 및 수율 정합성 향상을 위한 투입Lot 종료 취소에 대한 기능변경
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_ASSY_CANCEL_TERM.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ASSY_CANCEL_TERM : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        private string _ProcID = string.Empty;
        private bool bViewMsg = false;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public CMM_ASSY_CANCEL_TERM()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {                    
                    _ProcID = Util.NVC(tmps[0]);
                }
                else
                {                    
                    _ProcID = "";
                }

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtLotID_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetTermLotInfo();
                }
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
                if (bViewMsg) return;

                if (!CanCancelTerm())
                    return;
                
                Util.MessageConfirm("SFU1887", (result) =>// 종료취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CancelTermLot();
                    }
                });                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void GetTermLotInfo()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inTable = _Biz.GetDA_PRD_SEL_CANCEL_TERMINATE();

                DataRow newRow = inTable.NewRow();                
                newRow["PROCID"] = _ProcID;
                newRow["LOTID"] = txtLotID.Text.Trim();

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_CANCEL_TERMINATE_CMM", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgLotInfo.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgLotInfo, searchResult, FrameOperation, true);

                        if (searchResult != null && searchResult.Rows.Count < 1)
                            Util.MessageValidation("SFU2885", txtLotID.Text);    // {0} 은 해당 공정에 투입LOT 중 종료된 정보가 없습니다.

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
                );
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }

        }

        private DataSet GetBR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK()
        {
            DataSet indataSet = new DataSet();

            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("WIPNOTE", typeof(string));
            inDataTable.Columns.Add("ERP_TRNF_FLAG", typeof(string));
            inDataTable.Columns.Add("PROCID", typeof(string));
            inDataTable.Columns.Add("IFTYPE", typeof(string));


            DataTable in_DATA = indataSet.Tables.Add("INLOT");
            in_DATA.Columns.Add("LOTID", typeof(string));
            in_DATA.Columns.Add("LOTSTAT", typeof(string));
            in_DATA.Columns.Add("WIPQTY", typeof(int));
            in_DATA.Columns.Add("WIPQTY2", typeof(int));
            in_DATA.Columns.Add("INPUT_SEQNO", typeof(string));

            return indataSet;
        }

        private void CancelTermLot()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                dgLotInfo.EndEdit();

                //DataSet indataSet = _Biz.GetBR_PRD_REG_CANCEL_TERMINATE_LOT();
                DataSet indataSet = GetBR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK();
                DataTable inTable = indataSet.Tables["INDATA"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;                
                newRow["USERID"] = LoginInfo.USERID;

                newRow["PROCID"] = _ProcID;
                newRow["IFTYPE"] = "NORMAL"; //임의로 설정한 값임 Biz 확인
                inTable.Rows.Add(newRow);
                
                DataTable in_DATA = indataSet.Tables["INLOT"];

                newRow = null;

                newRow = in_DATA.NewRow();
                newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "LOTID"));
                newRow["LOTSTAT"] = "RELEASED";                
                newRow["WIPQTY"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")));
                newRow["WIPQTY2"] = Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")));

                in_DATA.Rows.Add(newRow);
                
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CANCEL_TERMINATE_LOT_ERP_CLOSE_CHK", "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.

                        Util.gridClear(dgLotInfo);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }, indataSet);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [Validation]

        private bool CanCancelTerm()
        {
            bool bRet = false;

            if (dgLotInfo.ItemsSource == null || dgLotInfo.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2953");    // 종료 취소 할 항목이 없습니다.
                return bRet;
            }

            // 자동 Change 모드인 경우 투입취소 불가.
            if (_ProcID.Equals(Process.LAMINATION) &&
                DataTableConverter.Convert(dgLotInfo.ItemsSource).Columns.Contains("AUTO_STOP_FLAG") &&
                Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "AUTO_STOP_FLAG")).Equals("Y"))
            {
                // 투입취소 불가 : 설비 자동 Change 모드로 투입 완료처리된 LOT은 취소 불가.
                Util.MessageValidation("SFU6037");
                return bRet;
            }

            // 수량 체크.
            double dQty = 0;
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")), out dQty);
            if (Util.NVC(DataTableConverter.GetValue(dgLotInfo.Rows[0].DataItem, "WIPQTY2_ST")).Equals("") ||
                dQty < 1)
            {
                Util.MessageValidation("SFU1683");  // 수량은 0보다 커야 합니다.
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);
            
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        #endregion

        #endregion

        private void dgLotInfo_BeganEdit(object sender, DataGridBeganEditEventArgs e)
        {
            //try
            //{
            //    if (e == null)
            //        return;

            //    if (!Util.NVC(e.Column.Name).Equals("WIPQTY2_ST"))
            //        return;

            //    sPrvQtyValue = Util.NVC(DataTableConverter.GetValue(e.Row.DataItem, "WIPQTY2_ST"));
            //}
            //catch(Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }

        private void dgLotInfo_CommittedEdit(object sender, DataGridCellEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                if (!Util.NVC(e.Cell.Column.Name).Equals("WIPQTY2_ST"))
                    return;

                C1DataGrid grd = sender as C1DataGrid;

                grd.EndEdit();
                grd.EndEditRow(true);

                string sTmp = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY2_ST"));
                string sMax = Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "WIPQTY_IN"));

                double dMax = 0;
                double dNow = 0;

                double.TryParse(sMax, out dMax);
                double.TryParse(sTmp, out dNow);

                if (dMax >= 0 && dMax < dNow)
                {
                    bViewMsg = true;
                    Util.MessageValidation("SFU3107", (actin) =>{ bViewMsg = false; });   // 수량이 이전 수량보다 많이 입력 되었습니다.
                    DataTableConverter.SetValue(grd.Rows[e.Cell.Row.Index].DataItem, "WIPQTY2_ST", dMax == 0 ? 1 : dMax);

                    grd.UpdateLayout();
                }                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgLotInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(e.Cell.Column.Name).Equals("WIPQTY2_ST"))
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F8DAC0"));
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

        private void dgLotInfo_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void dgLotInfo_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null) return;

            C1DataGrid grd = sender as C1DataGrid;

            if ((bool)e.NewValue == false)
                grd.EndEdit();
        }
    }
}
