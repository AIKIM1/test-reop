/*************************************************************************************
 Created Date : 2019.09.03
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.09.03  염규범 Initial Created. CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361
  2019.09.20  손우석 CSR ID 4087361  [요청] Cell 실시간 공급 정보 제공 |[요청번호]C20190918_87361  디자인 수정 및 용어 변경
  2019.10.14  염규범 Pallet 별로 보여주는 내용 추가, Grid 고정으로 변경
  2023.03.17  손병길 WA 셀공급관리 DB전환으로 미사용 컬럼 제거 비즈변동없음(비즈내부수정)
**************************************************************************************/

using System.Windows.Controls;
using LGC.GMES.MES.Common;
using System.Windows;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using System;
using C1.WPF;
using System.Windows.Media;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_045 : UserControl
    {

        #region Declaration & Constructor 
        public PACK001_045()
        {
            InitializeComponent();
            setComboBox();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void btnDdaySearch_Click(object sender, RoutedEventArgs e)
        {
            getDayTabData();
        }

        #region
        private void btnDday1Search_Click(object sender, RoutedEventArgs e)
        {
            getDay1TabData();
        }
        #endregion


        #region [ Method ]


        #region [ setComboBox ]
        private void setComboBox()
        {
            try
            {

                // HOLD 사유 코드
                CommonCombo _combo = new CommonCombo();

                // HOLD 사유
                string[] sFilter = { LoginInfo.CFG_SHOP_ID ,null, "A6" };
                _combo.SetCombo(cboShipToId, CommonCombo.ComboStatus.ALL, sCase: "SHIPTO_CP", sFilter: sFilter);

                string[] sFilter2 = { LoginInfo.CFG_SHOP_ID, null, "A6" };
                _combo.SetCombo(cboDay1ShipToId, CommonCombo.ComboStatus.ALL, sCase: "SHIPTO_CP", sFilter: sFilter2);
                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region [ Dday Search ] 
        private void getDayTabData()
        {
            try
            {
                ShowLoadingIndicator();
                 
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("D1DAY", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = Util.NVC(txtDdayTabProdID.Text) == "" ? null : Util.NVC(txtDdayTabProdID.Text);
                //2019.09.20
                //dr["EQSGID"] = Util.NVC(cboDdayTabLine.SelectedValue) == "" ? null : Util.NVC(cboDdayTabLine.SelectedValue);
                //dr["EQSGID"] = null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["SHIPTO_ID"] = Util.NVC(cboShipToId.SelectedValue) == "" ? null : Util.NVC(cboShipToId.SelectedValue);
                dr["D1DAY"] = "N";
                RQSTDT.Rows.Add(dr);

                                                                                                                                                                     DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CELL_STOCK_LIST_V2", "INDATA", "OUTDATA", RQSTDT);
                string temp = string.Empty;
                
                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgDdayPlan, dtResult, FrameOperation, true);
                    Util.GridSetData(dgDdayRemain, dtResult, FrameOperation, false);
                    Util.GridSetData(dgDdayShortage, dtResult, FrameOperation, true);
                    Util.GridSetData(dgGridAvailableAreaSearch, dtResult, FrameOperation, false);
                }
                
                else
                {
                    // 존재하지 않습니다 ? 
                    // 조회된 데이터가 없습니다. 메세지로 변경 요청
                    Util.MessageInfo("SFU2881");
                }
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
        #endregion

        #region [ Dday1 Search ] 
        private void getDay1TabData()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("SHIPTO_ID", typeof(string));
                RQSTDT.Columns.Add("D1DAY",  typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PRODID"] = Util.NVC(txtDay1TabProdID.Text) == "" ? null : Util.NVC(txtDay1TabProdID.Text);
                //2019.09.20
                //dr["EQSGID"] = Util.NVC(cboDay1TabLine.SelectedValue) == "" ? null : Util.NVC(cboDay1TabLine.SelectedValue);
                //dr["EQSGID"] = null;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                //dr["SHIPTO_ID"] = Util.NVC(cboDay1ShipToId.SelectedValue) == "" ? null : Util.NVC(cboDay1ShipToId.SelectedValue);
                dr["D1DAY"] = "Y";


                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_CELL_STOCK_LIST_V2", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgGrid1DdayPlan, dtResult, FrameOperation, true);
                    Util.GridSetData(dgGrid1DdayRemain, dtResult, FrameOperation, false);
                    Util.GridSetData(dgGrid1DdayShortage, dtResult, FrameOperation, true);
                    Util.GridSetData(dgGrid1AvailableAreaSearch, dtResult, FrameOperation, false);
                }
                else
                {
                    // 존재하지 않습니다 ? 
                    // 조회된 데이터가 없습니다. 메세지로 변경 요청
                    Util.MessageInfo("SFU2881");
                }
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
        #endregion

        #region [ EVENT ]
        //
        private void dgDdayShortage_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;
                C1DataGrid dg = sender as C1DataGrid;

                dg?.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        if(!float.Parse((DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHORTAGE").ToString())).Equals(0))
                        {
                            if(e.Cell.Column.Index == 4)
                            {
                                e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                            }
                            //dgGridAvailableAreaSearch.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }

                        //if (float.Parse((DataTableConverter.GetValue(e.Cell.Row.DataItem, "PALLET_STOCK_QTY").ToString())) < 5)
                        //{
                        //    dgGridAvailableAreaSearch.GetCell(0, e.Cell.Column.Index).Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        //}

                    }
                }
                ));

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        private void dgGridAvailableAreaSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;
            C1DataGrid dg = sender as C1DataGrid;

            dg?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type == DataGridRowType.Item)
                {

                    float temp = (float.Parse((DataTableConverter.GetValue(e.Cell.Row.DataItem, "MTRL_AVAILABLE_QTY").ToString())) - float.Parse((DataTableConverter.GetValue(e.Cell.Row.DataItem, "SHORTAGE").ToString())));

                    if (temp < 0)
                    {
                        if (e.Cell.Column.Index == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                            //dgGridAvailableAreaSearch.GetCell(e.Cell.Row.Index, e.Cell.Column.Index).Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF000000"));
                        }
                    }

                }
            }
            ));
        }

        // 로딩 화면 표시
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }
        // 로딩 화면 숨김
        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        private void PalletQty_Checked(object sender, RoutedEventArgs e)
        {
            Available.Visibility        = Visibility.Collapsed;
            Available_ALL.Visibility = Visibility.Visible;
            QAInsp.Visibility           = Visibility.Collapsed;
            QAInsp_ALL.Visibility    = Visibility.Visible;
            Hold.Visibility             = Visibility.Collapsed;
            Hold_ALL.Visibility      = Visibility.Visible;
            EXP.Visibility              = Visibility.Collapsed;
            EXP_ALL.Visibility       = Visibility.Visible;
            
            InTrans.Visibility = Visibility.Collapsed;
            InTransPallet.Visibility = Visibility.Visible;

            //PalletQty.Visibility        = Visibility.Collapsed;

            //CellQty.IsChecked = false;
            //CellQty.Visibility          = Visibility.Visible;

        }
        private void PalletQty_Unchecked(object sender, RoutedEventArgs e)
        {
            Available.Visibility = Visibility.Collapsed;
            Available_ALL.Visibility = Visibility.Collapsed;
            QAInsp.Visibility = Visibility.Collapsed;
            QAInsp_ALL.Visibility = Visibility.Collapsed;
            Hold.Visibility = Visibility.Collapsed;
            Hold_ALL.Visibility = Visibility.Collapsed;
            EXP.Visibility = Visibility.Collapsed;
            EXP_ALL.Visibility = Visibility.Collapsed;


            InTrans.Visibility = Visibility.Collapsed;
            InTransPallet.Visibility = Visibility.Collapsed;

            //CellQty.Visibility = Visibility.Collapsed;


            //PalletQty.Visibility = Visibility.Visible;
        }

        private void Day1PalletQty_Unchecked(object sender, RoutedEventArgs e)
        {
            Day1Available.Visibility = Visibility.Visible;
            Day1Available_ALL.Visibility = Visibility.Collapsed;
            Day1QAInsp.Visibility = Visibility.Visible;
            Day1QAInsp_ALL.Visibility = Visibility.Collapsed;
            Day1Hold.Visibility = Visibility.Visible;
            Day1Hold_ALL.Visibility = Visibility.Collapsed;
            Day1EXP.Visibility = Visibility.Visible;
            Day1EXP_ALL.Visibility = Visibility.Collapsed;

            Day1InTrans.Visibility = Visibility.Visible;
            Day1InTransPallet.Visibility = Visibility.Collapsed;


            //CellQty.Visibility = Visibility.Collapsed;


            //PalletQty.Visibility = Visibility.Visible;
        }

        private void Day1PalletQty_Checked(object sender, RoutedEventArgs e)
        {
            Day1Available.Visibility = Visibility.Collapsed;
            Day1Available_ALL.Visibility = Visibility.Visible;
            Day1QAInsp.Visibility = Visibility.Collapsed;
            Day1QAInsp_ALL.Visibility = Visibility.Visible;
            Day1Hold.Visibility = Visibility.Collapsed;
            Day1Hold_ALL.Visibility = Visibility.Visible;
            Day1EXP.Visibility = Visibility.Collapsed;
            Day1EXP_ALL.Visibility = Visibility.Visible;

            Day1InTrans.Visibility = Visibility.Collapsed;
            Day1InTransPallet.Visibility = Visibility.Visible;
        }
        
    }
}
