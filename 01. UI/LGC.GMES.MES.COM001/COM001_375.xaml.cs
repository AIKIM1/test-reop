/*************************************************************************************
 Created Date : 2023.02.27 
      Creator : 김상민
   Decription : 테스트용 프로그램
--------------------------------------------------------------------------------------
 [Change History]

***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using C1.WPF.DataGrid;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Animation;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_375 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
//        LGC.GMES.MES.CMM001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.CMM001.Class.MESSAGE_PARAM();
        Util util = new Util();
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_375()
        {
            InitializeComponent();
            SetCombo(cboTagetModel);
            if (LoginInfo.CFG_AREA_ID == "P7" || LoginInfo.CFG_AREA_ID == "P8" || LoginInfo.CFG_AREA_ID == "PA")
            {
                //btnTagetMove.Visibility = Visibility.Visible;
            }
            else
            {
                //btnTagetMove.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #region Initialize
        //2020.02.04
        private void InitCombo()
        {
            setComboBox();
        }
        #endregion

        #region Event

        #region Event - UserControl

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                //listAuth.Add(btnTagetInputComfirm);
                //listAuth.Add(btnPalletInfoChangePopUpOpen);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                DateTime DateNow = DateTime.Now;
                DateTime firstOfThisMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                //2019.06.07
                //DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddMonths(1);
                DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddDays(7);
                //DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                //dtpDateFrom.SelectedDateTime = firstOfThisMonth;
                //2019.06.07
                //dtpDateTo.SelectedDateTime = lastOfThisMonth;
                //dtpDateTo.SelectedDateTime = firstOfNextMonth;

                //dtpWaitSearchDateFrom.IsNullInitValue = true;
                //dtpWaitSearchDateTo.IsNullInitValue = true;

                //dtpWaitSearchDateFrom.SelectedDateTime = firstOfThisMonth;
                //2019.06.07
                //dtpWaitSearchDateTo.SelectedDateTime = lastOfThisMonth;
                //dtpWaitSearchDateTo.SelectedDateTime = firstOfNextMonth;

                //2018.05.14
                //dtpCell_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                //dtpCell_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                //2020.02.04
                //setComboBox();
                InitCombo();

                //tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                //tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                //tbWaitSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        #endregion

        #region Event - Button

/*       private void btnPalletInfoChangePopUpOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                COM001_375_RECEIVEPRODUCT_CHANGE popup = new COM001_375_RECEIVEPRODUCT_CHANGE();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    string sPalletId = txtPalletID.Text;

                    C1WindowExtension.SetParameter(popup, sPalletId);

                    //popup02.Closed -= popup02_Closed;
                    //popup02.Closed += popup02_Closed;
                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
*/

/*        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //전체 취소 하시겠습니까?
            {
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                if (result == MessageBoxResult.OK)
                {
                    //for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)dgTagetList_MouseDoubleClick
                    //{
                    //    dgTagetList.RemoveRow(i);
                    //}

                    Util.gridClear(dgTargetList);

                    clearInput();
                }
            }
            );
        }*/

/*        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                if (util.GetDataGridCheckCnt(dgTargetList, "CHK") > 0)
                {
                    DataTable dtTempTagetList = DataTableConverter.Convert(dgTargetList.ItemsSource);

                    for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                            Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                        {

                            dtTempTagetList.Rows[i].Delete();
                            dtTempTagetList.AcceptChanges();
                        }
                    }
                    dgTargetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtTempTagetList.Rows.Count));
                    if (!(dtTempTagetList.Rows.Count > 0))
                    {
                        Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");
                        dgTargetList.ItemsSource = null;
                        clearInput();
                    }

                    /*
                    int iTotalRow = dgTagetList.Rows.Count;
                    for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")) == "True" ||
                            Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "CHK")) == "1")
                        {
                            dgTagetList.BeginEdit();
                            dgTagetList.IsReadOnly = false;
                            dgTagetList.RemoveRow(i);
                            dgTagetList.IsReadOnly = true;
                            dgTagetList.EndEdit();
                            iTotalRow--;
                        }
                    }

                    DataTableConverter.Convert(dgTagetList.ItemsSource).AcceptChanges();
                    int Test = dgTagetList.Rows.Count;

                    if (!(iTotalRow > 0))
                    {
                        dgTagetList.ItemsSource = null;
                        clearInput();
                    }

                    --

                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
*/
        private void btnSearch_Click(object sender, RoutedEventArgs e) // 조회버튼
        {
            try
            {
                //if (!dtDateCompare()) return;

                getWareHousingData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        private void btnSearch_log_Click(object sender, RoutedEventArgs e) // 조회버튼
        {
            try
            {                                                       
                    string ProcedureId = Util.NVC(dgTargetList.SelectedItem.GetValue("ID").ToString());         
                    SelectsIdInfo(ProcedureId);                       
                                          
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        


        /*        private void btnExcel_Click(object sender, RoutedEventArgs e)
                {
                    try
                    {
                        new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList);
                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }

                }
        */
        /*        private void btnWaitExcel_Click(object sender, RoutedEventArgs e)
                {
                    try
                    {
                        new LGC.GMES.MES.Common.ExcelExporter().Export(dgWaitSearchResultList);
                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }
                }
        */
        /*        private void btnWaitSearch_Click(object sender, RoutedEventArgs e)
                {
                    getInputWaitPalletInfo(null);
                }*/

        //2018.05.14
        private void btnCell_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getCell_InputState();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

/*        private void btnCell_Excel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotCellList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        } */
        //2018.05.14
/*        private void btnPalletInfo_Click(object sender, RoutedEventArgs e)
        {
            popUpOpenPalletInfo(null, null);
        }
*/
        //2010.11.06  강호운 CELL 정보 동간이동 기능 추가

        //2020.02.04
        private void btnSearchAll_Click(object sender, RoutedEventArgs e)
        {
            getRcvCellALL();
        }

/*        private void btnExcelAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgRcvList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnExcelAll1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgCellInfo);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
*/
        #endregion Event - Button

        #region Event - TextBox

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    dgTargetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                    dgTargetList.LoadedCellPresenter += dgTagetListCellPresenter;
                    //ChkGbtInPallet();
                    // 2019.12.27 염규범
                    // 김정균 책임님 요청의 건
                    // 입고 Validation
                    // getTagetPalletInfo();
                    getWareHousingData();

                    //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtWaitSearch_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    //if (txtWaitSearch.Text.Length > 0)
                    //{
                    //    getInputWaitPalletInfo(txtWaitSearch.Text);
                    //}
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion Event - TextBox

        #region Event - ComboBox

        private void cboSearchProduct_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                txtSearchProduct.Text = e.NewValue.ToString();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        /*        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
                {
                    try
                    {
                        setComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), txtTagetPRODID.Text);
                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }
                }*/


        //2018.12.12
        /*        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
                {

                    try
                    {
                        if (cboTagetRoute.SelectedItem == null)
                        {
                            sTagetArea = "";
                        }
                        else
                        {
                            sTagetArea = Convert.ToString(DataTableConverter.GetValue(cboTagetRoute.SelectedItem, "AREAID"));
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.Alert(ex.ToString());
                    }
                }*/

        #endregion  Event - ComboBox

        #region Event - DataGrid

        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                            string Name = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "ID"));
                            popUpOpenPalletInfo(Name);                       
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTagetList_MouseClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                /*
                //PALLETID
                 Point pnt = e.GetPosition(null);
                   C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCellFromPoint(pnt);
                   if (cell != null)
                   {
                       if (cell.Row.Index > -1)
                       {                                 
                               string ProcedureId = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[cell.Row.Index].DataItem, "ID"));         
                               SelectsIdInfo(ProcedureId);                       
                       }
                   }
                   */
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void dgSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "PALLETID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgTagetListCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                if (e.Cell.Presenter == null)
                {
                    return;
                }

                if (e.Cell.Row.Type != DataGridRowType.Item)
                    return;
                if (dgTargetList == null)
                    return;

                if (e.Cell.Column.Name == "RECEIVABLE_FLAG" || e.Cell.Column.Name == "OCV_FLAG")
                {
                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else if (e.Cell.Column.Name == "LOT_OVERLAP")
                {
                    if (e.Cell.Text == "Y")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                }
                else
                {
                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }


                /*if (e.Cell.Column.Name == "RECEIVABLE_FLAG")
                {
                    if (e.Cell.Text == "N")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        DataTableConverter.SetValue(dgTargetList.Rows[e.Cell.Row.Index].DataItem, "CHK", 0);

                        if (!(dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = false;
                        }
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                        DataTableConverter.SetValue(dgTargetList.Rows[e.Cell.Row.Index].DataItem, "CHK", 1);
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, 3).Presenter.FontWeight = FontWeights.Bold;
                        }
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, 4).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                            dataGrid.GetCell(e.Cell.Row.Index, 4).Presenter.FontWeight = FontWeights.Bold;
                        }
                        if (!(dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter == null))
                        {
                            dataGrid.GetCell(e.Cell.Row.Index, 0).Presenter.IsEnabled = true;

                        }
                    }
                }*/
                //dgTagetList.LoadedCellPresenter -= dgTagetListCellPresenter;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgWaitSearchResultList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "PALLETID")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void Ocv_Button_Click(object sender, RoutedEventArgs e)
        {
            getOcvData(sender);
        }

        //2018.12.12
        private void dgTagetList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dgTargetList.CurrentRow == null || dgTargetList.SelectedIndex == -1)
            {
                return;
            }

            string sColName = dgTargetList.CurrentColumn.Name;
            string strChkValue = "";
            string strOkValue = "";

            if (!sColName.Contains("CHK"))
            {
                return;
            }

            try
            {
                if (e.ChangedButton.ToString().Equals("Left"))
                {
                    int indexRow = dgTargetList.CurrentRow.Index;
                    int indexColumn = dgTargetList.CurrentColumn.Index;

                    strChkValue = Util.NVC(dgTargetList.GetCell(indexRow, indexColumn).Value);
                    strOkValue = Util.NVC(dgTargetList.GetCell(indexRow, 8).Value);

                    if (string.IsNullOrEmpty(strChkValue) || strChkValue.Equals(""))
                        return;
                    if (!strOkValue.Equals("N"))
                    {
                        if (strChkValue == "0" || strChkValue == "False")
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[dgTargetList.CurrentRow.Index].DataItem, sColName, true);
                        }
                        else if (strChkValue == "1" || strChkValue == "True")
                        {
                            DataTableConverter.SetValue(dgTargetList.Rows[dgTargetList.CurrentRow.Index].DataItem, sColName, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //2020.02.04
        private void dgRcvList_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell.Column.Name == "RCV_ISS_ID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else if (e.Cell.Column.Name == "MOVE_PERIOD")
                    {
                        SetCellColor(dataGrid, e);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        /*private void dgRcvList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgRcvList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name.Equals("RCV_ISS_ID"))
                    {
                        string sRCVISSID = Util.NVC(DataTableConverter.GetValue(dgRcvList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));

                        getRcvCellALL_DETAIL(sRCVISSID);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }*/

        private void dgCellInfo_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                    e.Cell.Presenter.FontWeight = FontWeights.Normal;
                }));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion  Event - DataGrid

        #endregion Event

        #region Mehod
        /// <summary>
        /// 입력 Validation
        /// </summary>
        /// <returns>true:정상 , false: 입력/선택 오류</returns>
        private bool ChkInputData()
        {
            bool bReturn = true;

            try
            {
                //if (cboTagetAREAID.SelectedIndex < 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("동을선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    bReturn = false;
                //    cboTagetRoute.Focus();
                //    return bReturn;
                //}

                //if (cboTagetEQSGID.SelectedIndex < 0)
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("라인을선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    bReturn = false;
                //    cboTagetRoute.Focus();
                //    return bReturn;
                //}

                /*if (cboTagetRoute.SelectedIndex < 0)
                {
                    ms.AlertWarning("SFU1455"); //경로를 선택 하세요
                    bReturn = false;
                    cboTagetRoute.Focus();
                    return bReturn;
                }

                if (cboTagetRoute.SelectedIndex < 0)
                {
                    ms.AlertWarning("SFU1455"); //경로를 선택 하세요
                    bReturn = false;
                    cboTagetRoute.Focus();
                    return bReturn;
                }

                if (cboTagetModel.SelectedIndex < 0)
                {
                    ms.AlertWarning("SFU1619"); //생산예정모델을 선택 하세요. 
                    bReturn = false;
                    cboTagetModel.Focus();
                    return bReturn;
                }

                bReturn = ChkGbtInPallet(); */
            }
            catch (Exception ex)
            {
                throw ex;
            }
                return bReturn;
            }
 

        
        #endregion

        private void setWarehousingUnitPallet()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                var serWareHousingList = dt.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true);

                DataTable dtPallet = serWareHousingList.CopyToDataTable();

                loadingIndicator.Visibility = Visibility.Visible;
                DoEvents();

                for (int i = 0; i < dtPallet.Rows.Count; i++)
                {
                    if (string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["PALLETID"])) || string.IsNullOrEmpty(Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"])))
                    {
                        Util.MessageInfo("SFU3256");
                        return;
                    }

                    DataTable INDATA = new DataTable();
                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("SRCTYPE", typeof(string));
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("MODLID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    INDATA.Columns.Add("EQSGID", typeof(string));
                    INDATA.Columns.Add("ROUTID", typeof(string));
                    INDATA.Columns.Add("USERID", typeof(string));
                    INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                    DataRow drINDATA = null;
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    //drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = LoginInfo.CFG_AREA_ID;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    //drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);

                    INDATA.Rows.Add(drINDATA);

                    DataTable RCV_ISS = new DataTable();
                    RCV_ISS.TableName = "RCV_ISS";
                    RCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                    RCV_ISS.Columns.Add("PALLETID", typeof(string));

                    DataRow drRCV_ISS = null;
                    drRCV_ISS = RCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(dtPallet.Rows[i]["RCV_ISS_ID"]);
                    drRCV_ISS["PALLETID"] = Util.NVC(dtPallet.Rows[i]["PALLETID"]);
                    RCV_ISS.Rows.Add(drRCV_ISS);

                    DataSet dsIndata = new DataSet();
                    dsIndata.Tables.Add(INDATA);
                    dsIndata.Tables.Add(RCV_ISS);

                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RECEIVE_PRODUCT_PACK", "INDATA,RCV_ISS", "OUTDATA", dsIndata);

                    if (dsResult != null && dsResult.Tables.Count > 0 && dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        dt.AcceptChanges();

                        foreach (DataRow drDel in dt.Rows)
                        {
                            if (drDel["PALLETID"].ToString() == dsResult.Tables["OUTDATA"].Rows[0]["PALLETID"].GetString())
                            {
                                drDel.Delete();
                                break;
                            }
                        }

                        dt.AcceptChanges();

                        Util.GridSetData(dgTargetList, dt, FrameOperation);

                    }

                }

//                ms.AlertInfo("SFU1412");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));

                loadingIndicator.Visibility = Visibility.Collapsed;

            }
        }

        private Boolean ChkGbtInPallet()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

                object[] arrPallet = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["PALLETID"]).ToArray();
                object[] arrRcv = dt.Select().Where(y => y["CHK"].ToString() == "True").Select(x => x["RCV_ISS_ID"]).ToArray();

                string[] arrPalletStr = arrPallet.Cast<string>().ToArray();
                string[] arrRcvStr = arrRcv.Cast<string>().ToArray();

                string strSeparator = ",";

                string strPallet = string.Join(strSeparator, arrPalletStr);
                string strRcv = string.Join(strSeparator, arrRcvStr);


                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["RCV_ISS_ID"] = strRcv;
                dr["PALLETID"] = strPallet;

                INDATA.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_GBT", "INDATA", null, INDATA);
                loadingIndicator.Visibility = Visibility.Collapsed;
                return true;


            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                //Util.MessageException(ex);
                //Util.MessageInfo(ex.Message.ToString());
                Util.MessageInfo(ex.Data["CODE"].ToString());
                
                return false;
            }
        }

        private DataSet GetSaveWarehousing_DataSet()
        {
            DataSet dsIndata = new DataSet();
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //RCV_ISS_ID groupby 추출
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();
                    drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    drINDATA["LANGID"] = LoginInfo.LANGID;
                    //drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    //drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTargetList.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "RCV_ISS_ID"));
                    drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTargetList.Rows[i].DataItem, "PALLETID"));
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }


                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtRCV_ISS);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dsIndata;
        }

        private void getWareHousingData()
        {
            try
            {
                string useYN = null;
                if (chkFCS.IsChecked == true)
                {
                    useYN = "N";
                }

                string cbo_code;
                cbo_code = cboTagetModel.SelectedValue.ToString();


                if (cbo_code == "ALL")
                {
                    cbo_code = null;
                }


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("ID", typeof(string));
                RQSTDT.Columns.Add("USEYN", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["ID"] = txtProcedureID.Text;
                dr["USEYN"] = useYN;
                dr["CBO_CODE"] = cbo_code;
                
                RQSTDT.Rows.Add(dr);

                
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_PROCEDUREID", "RQSTDT", "RSLTDT", RQSTDT);


                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgTargetList, dtResult, FrameOperation, true);
                //Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void SelectsIdInfo(string ProcedureId)
        {
            try
            {
                int selectCount = 0;
                if (txtSelectNum.Text == "")
                {
                    Util.MessageInfo("SFU1154");
                    return;
                }

                selectCount = Int32.Parse(txtSelectNum.Text);

                if (selectCount > 500)
                {
                    Util.MessageInfo("SFU8543");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("ID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int32));
    
                DataRow dr = RQSTDT.NewRow();
                dr["ID"] = ProcedureId;
                dr["COUNT"] = selectCount;
                RQSTDT.Rows.Add(dr);


                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_PROCEDUREID_LOG", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void textBoxNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!((Key.D0 <= e.Key && e.Key <= Key.D9)
                || (Key.NumPad0 <= e.Key && e.Key <= Key.NumPad9)
                || e.Key == Key.Back))
            {
                e.Handled = true;
            }
        }


        //2018.05.14
        private void getCell_InputState()
        {
            try
            {
                //2019.04.01
                string strStrTime = "";
                string strEndTime = "";

                switch (LoginInfo.CFG_SHOP_ID)
                {
                    case "A040":
                        strStrTime = " 06:00:00";
                        strEndTime = " 05:59:59";
                        break;

                    case "G451":
                        strStrTime = " 07:00:00";
                        strEndTime = " 06:59:59";
                        break;

                    case "G382":
                        strStrTime = " 06:00:00";
                        strEndTime = " 05:59:59";
                        break;

                    case "G481":
                        strStrTime = " 07:00:00";
                        strEndTime = " 06:59:59";
                        break;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_ST", typeof(string));
                RQSTDT.Columns.Add("ISS_DTTM_ED", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = Util.NVC(cboAREACell.SelectedValue);
                //2019.04.01
                //dr["ISS_DTTM_ST"] = dtpCell_DateFrom.SelectedDateTime.ToString("yyyy-MM-dd");
                //dr["ISS_DTTM_ED"] = dtpCell_DateTo.SelectedDateTime.ToString("yyyy-MM-dd");
                //dr["ISS_DTTM_ST"] = dtpCell_DateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + strStrTime;
                //dr["ISS_DTTM_ED"] = dtpCell_DateTo.SelectedDateTime.ToString("yyyy-MM-dd") + strEndTime;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_STOCK_CELL_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    //Util.GridSetData(dgLotCellList, dtResult, FrameOperation, true);
                    //Util.SetTextBlockText_DataGridRowCount(tbCell_LotListCount, Util.NVC(dgLotCellList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.05.14
        private bool chkPalletInput(DataTable dtResult)
        {
            bool bResult = true;
            try
            {
                if (!(dtResult.Rows.Count > 0))
                {
//                    ms.AlertWarning("SFU1888"); //정보없는ID입니다
                    bResult = false;
                    return bResult;
                }
                if (Util.NVC(dtResult.Rows[0]["RCV_ISS_STAT_CODE"]) == "END_RECEIVE")
                {
//                    ms.AlertWarning("SFU1800"); //입고완료된ID입니다
                    bResult = false;
                    return bResult;
                }

                #region 입력된이전값과 비교
                DataTable dtBefore = DataTableConverter.Convert(dgTargetList.ItemsSource);
                bool bCheck = false;
                string sCheckPalletId = string.Empty;
                if (dtBefore.Rows.Count > 0)
                {
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        DataRow[] drTemp = dtBefore.Select("PALLETID = '" + Util.NVC(dtResult.Rows[i]["PALLETID"]) + "'");
                        if (drTemp.Length > 0)
                        {
                            bCheck = true;
                            sCheckPalletId = Util.NVC(dtResult.Rows[i]["PALLETID"]);
                            break;
                        }
                    }
                }
                if (bCheck)
                {
//                    ms.AlertWarning("SFU1410", sCheckPalletId); //PALLETID는중복입력할수없습니다.\r\n({0})
                    bResult = false;
                    return bResult;
                }
                #endregion

                /*if (txtTagetPRODID.Text != "")
                {
                    if (txtTagetPRODID.Text != Util.NVC(dtResult.Rows[0]["PRODID"]))
                    {
//                        ms.AlertWarning("SFU1481"); //다른제품ID를입고할수없습니다.
                        bResult = false;
                        return bResult;
                    }
                }*/
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bResult;
        }

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();
                
                #region 입고
                //작업자 선택 동
                //C1ComboBox[] cboAreaChild = { cboTagetEQSGID };
                //_combo.SetCombo(cboTagetAREAID, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sCase: "AREA");

                ////작업자 선택 라인
                //C1ComboBox[] cboEquipmentSegmentParent = { cboTagetAREAID };
                //_combo.SetCombo(cboTagetEQSGID, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent, sCase: "EQUIPMENTSEGMENT");

                //작업자 조회 동
                /* cboAreaAll
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");
                */

                /*String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                    _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.NONE, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "cboAreaAll");
                }
                else
                {
                    C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                    _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");
                }

                //작업자 조회 라인
                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                C1ComboBox[] cboSearchEQSGIDChild = { cboProductModel };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.SELECT, cbParent: cboSearchEQSGIDParent, cbChild: cboSearchEQSGIDChild, sCase: "EQUIPMENTSEGMENT");

                //모델
                C1ComboBox[] cboProductModelParent = { cboSearchAREAID, cboSearchEQSGID };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");
                #endregion

                #region 입고대기 현황
                //동
                _combo.SetCombo(cboWaitSearchAREAID, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "AREA_AREATYPE");
                #endregion

                //2018.05.14
                #region Cell 투입 재고 현황
                //동
                _combo.SetCombo(cboAREACell, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "AREA_AREATYPE");
                #endregion
                //2018.05.14

                //2020.02.04
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

                SetFlag();*/

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void SetCombo(ComboBox cbo)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "ALL";
                dr["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["CBO_NAME"] = "PROCEDURE";
                dr["CBO_CODE"] = "PROCEDURE";
                dt.Rows.Add(dr);

                dr = dt.NewRow();
                dr["CBO_NAME"] = "FUNCTION";
                dr["CBO_CODE"] = "FUNCTION";
                dt.Rows.Add(dr);


                cbo.ItemsSource = DataTableConverter.Convert(dt);

                if (cbo.Items.Count > 0)
                    cbo.SelectedIndex = 1;

                
            }
            catch (Exception ex)
            {
                // ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);

            }

        }

        

        /*private void setComboBox_Model_schd(string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MTRLID"] = sMTRLID;

                // 2020-01-17 - 염규범S
                // 폴란드의 경우 Itransit 재공같은경우, AEARID가 없어서 DB가 분리 되어있는 상황에서, 타동에서 타동 CELL 입고시 ISSUSE 발생
                // 해당 내용에 대해서, Login된 동으로 입고 처리 가능하도록, Login 위치로 처리
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID == "" ? null : LoginInfo.CFG_AREA_ID;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    //PILOT2 호기 대응 으로 CELL 이 구분없이 투입되도록 하기위해 ALL 추가
                    //[TB_SFC_RCV_SUBLOT_INFO] PROD_SCHD_MODL NULL로 업데이트 하기위함.
                    if (dtResult.Rows.Count > 0)
                    {
                        DataRow dr = dtResult.NewRow();
                        dr["CBO_NAME"] = "-ALL-";
                        dr["CBO_CODE"] = null;
                        dtResult.Rows.Add(dr);
                    }*/


                    /*cboTagetModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetModel.SelectedIndex = 0;
                        cboTagetModel_SelectedValueChanged(null, null);
                    }
                    else
                    {
                        cboTagetModel_SelectedValueChanged(null, null);
                    }*/

   /*             }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }*/

        private void setComboBox_Route_schd(string sMODLID, string sMTRLID)
        {
            try
            {
                DataTable dtIndata = new DataTable();
                dtIndata.TableName = "RQSTDT";
                dtIndata.Columns.Add("LANGID", typeof(string));
                dtIndata.Columns.Add("MODLID", typeof(string));
                dtIndata.Columns.Add("MTRLID", typeof(string));
                dtIndata.Columns.Add("AREAID", typeof(string));
                DataRow drIndata = dtIndata.NewRow();

                drIndata["LANGID"] = LoginInfo.LANGID;
                drIndata["MODLID"] = sMODLID == "" ? null : sMODLID;
                drIndata["MTRLID"] = sMTRLID;
                // 2020-01-17 - 염규범S
                // 폴란드의 경우 Itransit 재공같은경우, AEARID가 없어서 DB가 분리 되어있는 상황에서, 타동에서 타동 CELL 입고시 ISSUSE 발생
                // 해당 내용에 대해서, Login된 동으로 입고 처리 가능하도록, Login 위치로 처리
                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID;
                }
                else
                {
                    drIndata["AREAID"] = LoginInfo.CFG_AREA_ID == "" ? null : LoginInfo.CFG_AREA_ID;
                }

                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }

                    /*cboTagetRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetRoute.SelectedIndex = 0;
                    }
                    else
                    {
                        Util.MessageInfo("라우터 정보가 존재하지 않습니다.");
                    }*/
                    //else
                    //{
                    //    cboTagetRoute_SelectionChanged(sender, null);
                    //}

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    
        private bool ChkOCV_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("EXEC_STAT_CODE = 'N'").Length > 0) //OCV존재 여부 체크 => 사용여부 존재 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

/*        private bool ChkDCIR_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("DCIR_FLAG = 'N'").Length > 0) // DCIR 존재 여부 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }
*/
        private bool ChkLotId_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                if (dt.Select("LOT_OVERLAP = 'Y'").Length > 0) // LOT 중복 여주 체크
                {
                    bReturn = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void popUpOpenPalletInfo(string Name)
        {
            try
            {
                COM001_375_POPUP popup = new COM001_375_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[1];
                    Parameters[0] = Name;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*private void clearInput()
        {
            try
            {
                txtTagetPRODID.Text = string.Empty;
                txtTagetPRODNAME.Text = string.Empty;

                cboTagetModel.ItemsSource = null;
                cboTagetModel.SelectedValue = null;

                cboTagetRoute.ItemsSource = null;
                cboTagetRoute.SelectedValue = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }*/

        private void getInputWaitPalletInfo(string sRCV_ISS)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("TO_AREA_NULL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = sRCV_ISS;
                //dr["FROMDATE"] = sRCV_ISS != null ? null : dtpWaitSearchDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                //dr["TODATE"] = sRCV_ISS != null ? null : dtpWaitSearchDateTo.SelectedDateTime.ToString("yyyyMMdd");
                //dr["AREAID"] = sRCV_ISS != null ? null : (Util.NVC(cboWaitSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboWaitSearchAREAID.SelectedValue));
                //dr["TO_AREA_NULL"] = (bool)chkNotToArea.IsChecked == true ? "Y" : "N";

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWaitSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                //Util.SetTextBlockText_DataGridRowCount(tbWaitSearchListCount, Util.NVC(dtResult.Rows.Count));

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    //dgWaitSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                    //Util.GridSetData(dgWaitSearchResultList, dtResult, FrameOperation, true);
                    //Util.SetTextBlockText_DataGridRowCount(tbWaitSearchListCount, Util.NVC(dgWaitSearchResultList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_TB_SFC_RCV_ISS_PLLT_LIST", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void getOcvData(object sender) // 사용여부 가져오게 바꾸기
        {
            try
            {
                Button btn = sender as Button;
                int iRow = -1;

                C1.WPF.DataGrid.DataGridRow row = new C1.WPF.DataGrid.DataGridRow();
                System.Collections.Generic.IList<System.Windows.FrameworkElement> ilist = btn.GetAllParents();
                foreach (var item in ilist)
                {
                    C1.WPF.DataGrid.DataGridRowPresenter presenter = item as C1.WPF.DataGrid.DataGridRowPresenter;
                    if (presenter != null)
                    {
                        row = presenter.Row;
                        iRow = row.Index;
                    }
                }


                DataRowView drv = row.DataItem as DataRowView;

                string selectPallet = drv["PALLETID"].ToString(); 
                string sOCV_FLAG = drv["OCV_FLAG"].ToString(); 
                if (sOCV_FLAG == "Y")
                {
                    return;
                }


                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PALLETID"] = selectPallet;
                INDATA.Rows.Add(drINDATA);

                //DataSet dsIndata = new DataSet();
                //dsIndata.Tables.Add(INDATA);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService("BR_PRD_CHK_RECEIVE_PRODUCT", "INDATA", "OUTDATA", INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_CHK_RECEIVE_PRODUCT", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {

                        if (result.Rows.Count > 0)
                        {
                            string sOcv_Flag = Util.NVC(result.Rows[0]["OCV_FLAG"]);



                            DataTableConverter.SetValue(dgTargetList.Rows[iRow].DataItem, "OCV_FLAG", sOcv_Flag);
                            C1.WPF.DataGrid.DataGridCell cell = dgTargetList.GetCell(iRow, dgTargetList.Columns["OCV_FLAG"].Index);
                            if (sOcv_Flag == "Y")
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                                cell.Presenter.FontWeight = FontWeights.Bold;
                            }
                            else
                            {
                                cell.Presenter.Foreground = new SolidColorBrush(Colors.Red);
                                cell.Presenter.FontWeight = FontWeights.Bold;
                            }


                        }
                        return;
                    }
                });

                //new ClientProxy().ExecuteService_Multi("BR_PRD_CHK_RECEIVE_PRODUCT", "INDATA", "OUTDATA", (dsResult, dataException) =>
                //{
                //    try
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        if (dataException != null)
                //        {
                //            Util.AlertByBiz("BR_PRD_CHK_RECEIVE_PRODUCT", dataException.Message, dataException.ToString());
                //            return;
                //        }
                //        else
                //        {
                //            if (dsResult != null && dsResult.Tables.Count > 0)
                //            {

                //                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                //                {
                //                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                //                    {
                //                        string sOcv_Flag = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["OCV_FLAG"]);

                //                        DataTableConverter.SetValue(dgTagetList.Rows[iRow].DataItem, "OCV_FLAG", sOcv_Flag);
                //                    }

                //                }
                //            }
                //            return;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        loadingIndicator.Visibility = Visibility.Collapsed;
                //        throw ex;
                //    }

                //}, dsIndata);
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
            }

        }

        //2019.11.07
        private bool ChkReceive()
        {
            bool bReturn = false;

            try
            {
                DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
                //RCV_ISS_ID groupby 추출
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();

                DataRow drINDATA = null;
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    drINDATA = INDATA.NewRow();

                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["PROCID"] = "P1000";
                    //drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["PRODID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["PRODID"]);
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    drINDATA["PALLETID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["PALLETID"]);

                    INDATA.Rows.Add(drINDATA);
                }

                //loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtReturn = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RECEIVE_PRODUCT_PACK", "INDATA", "OUTDATA", INDATA);

                if (dtReturn != null)
                {
                    int StandDay = int.Parse(dtReturn.Rows[0]["STAND_TIME"].ToString());
                    int OverDay = int.Parse(dtReturn.Rows[0]["OVER_TIIME"].ToString());

                    if (StandDay > 0)
                    {
                        if (OverDay >= StandDay)
                        {
                            bReturn = true;
                        }
                        else
                        {
                            bReturn = false;
                        }
                    }
                    else
                    {
                        bReturn = false;
                    }
                }

                return bReturn;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2018.12.12
        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                if (Util.NVC(dr["RECEIVABLE_FLAG"]).Equals("Y"))
                {
                    dr["CHK"] = true;
                }
            }
            dgTargetList.ItemsSource = DataTableConverter.Convert(dt);
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgTargetList);
        }

        //2020.02.04
        private void getRcvCellALL()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                //2020.02.13
                RQSTDT.Columns.Add("AREA_TYPE", typeof(string));
                RQSTDT.Columns.Add("TO_AREA_NULL", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                //dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);

                //2020.02.13
                //dr["AREA_TYPE"] = Util.NVC(cboAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaType.SelectedValue);
                //dr["TO_AREA_NULL"] = (bool)chkNotToArea2.IsChecked == true ? "Y" : "N";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_ALL", "RQSTDT", "RSLTDT", RQSTDT);

                //Util.GridSetData(dgRcvList, dtResult, FrameOperation, true);
                //Util.SetTextBlockText_DataGridRowCount(tbRcvCellAllListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getRcvCellALL_DETAIL(string sRCV_ISS_ID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRCV_ISS_ID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_ALL_DETAIL", "RQSTDT", "RSLTDT", RQSTDT);

                //Util.GridSetData(dgCellInfo, dtResult, FrameOperation, true);
                //Util.SetTextBlockText_DataGridRowCount(tbCellInfoCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetFlag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                //2020.02.13
                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "ALL";
                dr["CBO_CODE"] = "";
                dt.Rows.Add(dr);

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "Country";
                //2020.02.13
                //dr_["CBO_CODE"] = "IN";
                dr_["CBO_CODE"] = "Country";
                dt.Rows.Add(dr_);

                DataRow dr_1 = dt.NewRow();
                dr_1["CBO_NAME"] = "Foreign";
                //2020.02.13
                //dr_1["CBO_CODE"] = "OUT";
                dr_1["CBO_CODE"] = "Foreign";
                dt.Rows.Add(dr_1);

                dt.AcceptChanges();

                //cboAreaType.ItemsSource = DataTableConverter.Convert(dt);
                //cboAreaType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SetCellColor(C1DataGrid dataGrid, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (e.Cell.Row.DataItem != null)
            {
                if (dataGrid.Name.Equals("dgRcvList"))
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }

                    if (e.Cell != null && e.Cell.Presenter != null)
                    {
                        if (e.Cell.Row.Type == DataGridRowType.Item)
                        {
                            if (e != null && e.Cell != null && e.Cell.Presenter != null)
                            {
                                //국내Site   3일 이하 : 녹색
                                //           3일 초과 7일 이하 : 노란색
                                //           7일 초과 : 빨간색
                                // 해외Site 30일 이하 : 녹색
                                //          30일 초과 60일 이하 : 노란색
                                //          60일 초과 : 빨간색

                                //2020.02.13
                                //if (cboAreaType.SelectedValue.ToString() == "IN")
                                //{
                                //    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                //    if (nDiff <= 3)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //    else if (nDiff >3 && nDiff <= 7)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                //    }
                                //    else
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //}
                                //else
                                //{
                                //    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                //    if (nDiff <= 30)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //    else if (nDiff > 30 && nDiff <= 60)
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                //    }
                                //    else
                                //    {
                                //        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                //        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                //    }
                                //}

                                string str_AREA_TYPE = Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "AREA_TYPE"));

                                if (str_AREA_TYPE == "Country")
                                {
                                    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                    if (nDiff <= 3)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                    else if (nDiff > 3 && nDiff <= 7)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                }
                                else
                                {
                                    double nDiff = Double.Parse(Util.NVC(DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "MOVE_PERIOD")));

                                    if (nDiff <= 30)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Green);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                    else if (nDiff > 30 && nDiff <= 60)
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Yellow);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                    }
                                    else
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                    }
                                }



                            }
                        }
                    }
                }
            }
        }

        /*private Boolean dtDateCompare()
        {
            try
            {
                TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;

                if (timeSpan.Days < 0)
                {
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }

                if (timeSpan.Days > 7)
                {
                    dtpDateTo.SelectedDateTime = dtpDateFrom.SelectedDateTime.AddDays(+7);
                    //조회기간은 7일을 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3567");
                    return false;

                }

                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }*/

        #endregion
        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }

        private void txtSelectNum_TextChanged(object sender, TextChangedEventArgs e)
        {
         
        }

        private void txtProcedureID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}