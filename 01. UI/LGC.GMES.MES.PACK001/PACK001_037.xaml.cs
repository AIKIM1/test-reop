/*************************************************************************************
 Created Date : 2019.01.10
      Creator : Kim Do Hyung
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.10 김도형 : Initial Created.
  2019.01.11 김도형 CSR ID:3870223 법인간 자동차 Cell이송 프로세스 구축 요청 건 [요청번호] C20181213_70223 
  2020.11.30 김준겸 IWMS Cell 반품 관련 개선  및 IWMS 사용유무 Logic 추가. [요청번호] C20201127-000086 [서비스 번호] 119235
  2021.11.17 김용준 IWMS 반품 개선 건
  2024.03.12 김민석 IWMS 재입고 탭에서 체크 없이 입고 시 EXCEPTION 발생하여 오류 메시지 추가 [요청번호] E20240228-001206
  2025.04.15 김영택 NERP 대응, NERP 적용시 반품 창고 변경, 공통코드 (RETURN_SLOC_MAPPING) (2025.06.23 git 머지)
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_037 : UserControl, IWorkArea
    {
        ExcelMng exl = new ExcelMng();
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        private string sFromArea = string.Empty;
        private string sToArea = string.Empty;
        private string sToShopID = string.Empty;

        Util util = new Util();
        string sTagetArea = string.Empty;
        string sTagetEqsg = string.Empty;
        string sTabMenu = string.Empty;

        #region Declaration & Constructor 
        string now_labelcode = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public PACK001_037()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnTagetInputComfirm2);
                //listAuth.Add(btnPalletInfoChangePopUpOpen);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                DateTime DateNow = DateTime.Now;
                DateTime firstOfThisMonth = new DateTime(DateNow.Year, DateNow.Month, 1);
                DateTime firstOfNextMonth = new DateTime(DateNow.Year, DateNow.Month, 1).AddMonths(1);
                DateTime lastOfThisMonth = firstOfNextMonth.AddDays(-1);

                dtpDateFrom2.SelectedDateTime = firstOfThisMonth;
                dtpDateTo2.SelectedDateTime = lastOfThisMonth;

                setComboBox();

                tbTagetListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";

                tbTagetListCount2.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbSearchListCount2.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);

                btnExcel.Visibility = Visibility.Collapsed; // 2024.11.18. 김영국 - 버튼 안보이게 끔..(임성운 선임 요청)
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtReturnID_KeyDown(object sender, KeyEventArgs e)
        {
            //반품 Cell등록 ID
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (Check_Input())
                    {
                        //loadingIndicator.Visibility = Visibility.Visible;

                        DataTable INPUT_ID_LIST = new DataTable();
                        INPUT_ID_LIST.TableName = "INPUT_ID_LIST";
                        INPUT_ID_LIST.Columns.Add("RCV_ISS_ID", typeof(string));
                        INPUT_ID_LIST.Columns.Add("BOXID", typeof(string));
                        INPUT_ID_LIST.Columns.Add("LOTID", typeof(string));
                        INPUT_ID_LIST.Columns.Add("RTN_RSN_NOTE", typeof(string));

                        DataRow drINPUT_ID_LIST = INPUT_ID_LIST.NewRow();
                        drINPUT_ID_LIST["RCV_ISS_ID"] = null;
                        drINPUT_ID_LIST["BOXID"] = (bool)rdoPallet.IsChecked ? txtReturnID.Text : null;
                        drINPUT_ID_LIST["LOTID"] = (bool)rdoCell.IsChecked ? txtReturnID.Text : null;
                        drINPUT_ID_LIST["RTN_RSN_NOTE"] = txtReturnResn.Text;
                        INPUT_ID_LIST.Rows.Add(drINPUT_ID_LIST);

                        getReturnID_getLotInfo(INPUT_ID_LIST);

                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtReturnID != null)
                {
                    txtReturnID.Focus();
                    txtReturnID.SelectAll();
                }

                if (rdoCell != null)
                {
                    if ((bool)rdoCell.IsChecked)
                    {
                        if (btnReturnFileUpload != null)
                        {
                            btnReturnFileUpload.IsEnabled = true;
                        }
                    }
                    else
                    {
                        if (btnReturnFileUpload != null)
                        {
                            btnReturnFileUpload.IsEnabled = false;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtReturnResn_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtReturnID.Text.Length > 0 && txtReturnResn.Text.Length > 0)
                    {
                        if (Check_Input())
                        {
                            //loadingIndicator.Visibility = Visibility.Visible;

                            DataTable INPUT_ID_LIST = new DataTable();
                            INPUT_ID_LIST.TableName = "INPUT_ID_LIST";
                            INPUT_ID_LIST.Columns.Add("RCV_ISS_ID", typeof(string));
                            INPUT_ID_LIST.Columns.Add("BOXID", typeof(string));
                            INPUT_ID_LIST.Columns.Add("LOTID", typeof(string));
                            INPUT_ID_LIST.Columns.Add("RTN_RSN_NOTE", typeof(string));

                            DataRow drINPUT_ID_LIST = INPUT_ID_LIST.NewRow();
                            drINPUT_ID_LIST["RCV_ISS_ID"] = null;
                            drINPUT_ID_LIST["BOXID"] = (bool)rdoPallet.IsChecked ? txtReturnID.Text : null;
                            drINPUT_ID_LIST["LOTID"] = (bool)rdoCell.IsChecked ? txtReturnID.Text : null;
                            drINPUT_ID_LIST["RTN_RSN_NOTE"] = txtReturnResn.Text;
                            INPUT_ID_LIST.Rows.Add(drINPUT_ID_LIST);

                            getReturnID_getLotInfo(INPUT_ID_LIST);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnFileUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgTagetList.Rows.Count > 0 || txtReturnNumber.Text.Length > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1701"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (sResult) =>
                    //신규작성하시겠습니까?
                    {
                        if (sResult == MessageBoxResult.OK)
                        {
                            Refresh();
                        }
                    });
                }
                else
                {
                    Refresh();
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtSelected_RCV_ISS_ID.Text.Length > 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3340", txtSelected_RCV_ISS_ID.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //반품 취소 하시겠습니까?\n반품번호[{0}]
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            ReturnCell_Cancel();
                        }
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnReturnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgSearchResultList.CurrentRow != null)
                {
                    string sRCV_ISS_ID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.CurrentRow.DataItem, "RCV_ISS_ID"));

                    //반품테그정보 조회
                    DataTable dtReturnTagInfo = getReturnTagInfo(sRCV_ISS_ID);

                    //반품테그발행
                    printReturnTag(dtReturnTagInfo);
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("발행할반품번호를선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    ms.AlertWarning("SFU3398"); //발행할 반품번호를 선택하세요.

                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkInputCell_Return_YN_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3341"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //투입된 CELL이 포함되어 반품됩니다.\n체크 하시겠습니까?
                {
                    if (result == MessageBoxResult.Cancel)
                    {
                        chkInputCell_Return_YN.IsChecked = false;
                    }
                }
                    );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #region Event - Button
        //private void btnPalletInfoChangePopUpOpen_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        PACK001_037_RECEIVEPRODUCT_CHANGE popup = new PACK001_037_RECEIVEPRODUCT_CHANGE();
        //        popup.FrameOperation = this.FrameOperation;

        //        if (popup != null)
        //        {
        //            string sPalletId = txtPalletID.Text;

        //            C1WindowExtension.SetParameter(popup, sPalletId);

        //            //popup02.Closed -= popup02_Closed;
        //            //popup02.Closed += popup02_Closed;
        //            popup.ShowModal();
        //            popup.CenterOnScreen();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Util.Alert(ex.ToString());
        //    }
        //}


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnList();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }
        private void btnExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgSearchResultList2);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }

        }

        

        private void btnTagetInputComfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Check_ReturnConfirm())
                {
                    DataTable dt = DataTableConverter.Convert(dgTagetList.ItemsSource);

                    DataRow[] drINPUT_LOT = dt.Select("PROC_INPUT_FLAG = 'Y'");
                    if (drINPUT_LOT.Length > 0)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("신규"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //투입된 CELL이 존재합니다.\n반품처리 하시겠습니까?
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReturnCell();
                            }
                        });
                    }
                    else
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU2074"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        //반품 처리 하시겠습니까?
                        {
                            if (result == MessageBoxResult.OK)
                            {
                                ReturnCell();
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void btnTagetCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //전체 취소 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Refresh();
                    }
                }
            );
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnTagetSelectCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtTempTagetList = DataTableConverter.Convert(dgTagetList.ItemsSource);

                for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                {
                    if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                        Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                    {

                        dtTempTagetList.Rows[i].Delete();
                        dtTempTagetList.AcceptChanges();
                    }
                }
                dgTagetList.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtTempTagetList.Rows.Count));

                if (!(dtTempTagetList.Rows.Count > 0))
                {
                    dgTagetList.ItemsSource = null;
                    Refresh();
                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private bool dgTagetList2_ChkValridtion(DataTable dtresult) {
            DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);            
            for(int i =0; i < dt.Rows.Count; i++)
            {
                if(dtresult.Rows[0]["RCV_ISS_ID"].ToString() != dt.Rows[i]["RCV_ISS_ID"].ToString())
                {
                    Util.MessageInfo("SFU8510", dtresult.Rows[0]["RCV_ISS_ID"].ToString(), dt.Rows[i]["RCV_ISS_ID"].ToString());
                    return false;
                }
                if (dtresult.Rows[0]["PRODID"].ToString() != dt.Rows[i]["PRODID"].ToString())
                {
                    Util.MessageInfo("SFU4268");
                    return false;
                }
            }

            return true;
        }



        //IWMS 재입고
        //2019.01.17
        private void btnPalletInfo_Click(object sender, RoutedEventArgs e)
        {
            popUpOpenPalletInfo(null, null, null);
        }
        #endregion

        #region Event - TextBox
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    getTagetPalletInfo();
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
        private void cboTagetModel_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                setComboBox_Route_schd(Util.NVC(cboTagetModel.SelectedValue), txtTagetPRODID.Text);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void cboTagetRoute_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
         



         }
        #endregion  Event - ComboBox

            #region Event - DataGrid
        private void dgTagetList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgTagetList2.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            sTabMenu = "dgTagetList";
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId,sTabMenu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void dgSearchResultList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "RCV_ISS_ID")
                        {
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));

                            popUpOpenPalletInfo(sRcvIssId,null,null);
                        }

                        //if (cell.Column.Name == "PALLETID")
                        //{
                        //    string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                        //    string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "PALLETID"));

                        //    popUpOpenPalletInfo(sRcvIssId, sSelectPalletId);
                        //}
                    }
                }
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
                    if (e.Cell.Column.Name == "RCV_ISS_ID")
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
        private void dgTagetList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "OCV_FLAG")
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
        #endregion  Event - DataGrid

        #region Event - Button
        private void btnSearch_Click2(object sender, RoutedEventArgs e)
        {
            try
            {
                getWareHousingData();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnTagetInputComfirm_Click2(object sender, RoutedEventArgs e)
        {
            try
            {
                int FCSDataCheck = 0;
                if (!this.chkInputData())
                {
                    return;
                }

                // FCS Data Check (OCV 데이터 유무)
                if (this.ChkOCV_Exist())
                {
                    if ((bool)chkFCS.IsChecked)
                    {
                        ms.AlertWarning("SFU3447");     // OCV DATA가 없는 CELL이 존재합니다.
                        return;
                    }
                    else
                    {
                        FCSDataCheck++;
                    }
                }

                // FCS Data Check (DCIR 데이터 유무)
                if (this.ChkDCIR_Exist())
                {
                    if ((bool)chkDCIR.IsChecked)
                    {
                        ms.AlertWarning("SFU3447");     // OCV DATA가 없는 CELL이 존재합니다.
                        return;
                    }
                    else
                    {
                        FCSDataCheck++;
                    }
                }

                // 위의 두가지 체크 함수 결과가 하나라도 true이면 확인후에 입고처리
                if (FCSDataCheck > 0)
                {
                    // OCV DATA가 없는 CELL이 존재합니다. 입고하시겠습니까?"
                    Util.MessageConfirm("SFU1405", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            checkPopUpOpen();
                        }
                    });
                }
                else    // 그렇지 않으면 그냥 입고처리
                {
                    
                    checkPopUpOpen();   // 선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
                }
            }
            catch (Exception ex)
            {                
                Util.MessageException(ex);
            }


            //try
            //{
            //    if (ChkOCV_Exist())
            //    {
            //        if ((bool)chkFCS.IsChecked)
            //        {
            //            ms.AlertWarning("SFU3447");//SFU3447   OCV DATA가 없는 CELL이 존재합니다.
            //            return;
            //        }
            //        else
            //        {
            //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1405"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //            //OCV DATA가 없는 CELL이 존재합니다. 입고하시겠습니까?"
            //            {
            //                if (result == MessageBoxResult.OK)
            //                {
            //                    checkPopUpOpen();//선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
            //                }
            //            }
            //            );
            //        }
            //    }
            //    else
            //    {
            //        checkPopUpOpen();//선택 체크 팝업 오픈 오픈 close 시 조회및 입고처리.
            //    }

            //}
            //catch (Exception ex)
            //{
            //    Util.Alert(ex.ToString());
            //}
        }
        private void btnTagetCancel_Click2(object sender, RoutedEventArgs e)
        {
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1885"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            //전체 취소 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    //for (int i = (dgTagetList.Rows.Count - 1); i >= 0; i--)
                    //{
                    //    dgTagetList.RemoveRow(i);
                    //}

                    Util.gridClear(dgTagetList2);

                    clearInput();
                }
            }
            );
        }
        private void btnTagetSelectCancel_Click2(object sender, RoutedEventArgs e)
        {
            try
            {
                if (util.GetDataGridCheckCnt(dgTagetList2, "CHK") > 0)
                {
                    DataTable dtTempTagetList = DataTableConverter.Convert(dgTagetList2.ItemsSource);

                    for (int i = (dtTempTagetList.Rows.Count - 1); i >= 0; i--)
                    {
                        if (Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "True" ||
                            Util.NVC(dtTempTagetList.Rows[i]["CHK"]) == "1")
                        {

                            dtTempTagetList.Rows[i].Delete();
                            dtTempTagetList.AcceptChanges();
                        }
                    }
                    dgTagetList2.ItemsSource = DataTableConverter.Convert(dtTempTagetList);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount2, Util.NVC(dtTempTagetList.Rows.Count));
                    if (!(dtTempTagetList.Rows.Count > 0))
                    {
                        Util.SetTextBlockText_DataGridRowCount(tbTagetListCount2, "0");
                        dgTagetList2.ItemsSource = null;
                        clearInput();
                    }

                }

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion Event - Button

        #endregion Event

        #region Mehod        
        /// <summary>
        /// 입력 Validation
        /// </summary>
        /// <returns>true:정상 , false: 입력/선택 오류</returns>

        private bool Check_Input()
        {
            bool bReturn = true;
            try
            {
                //ID 입력확인.
                if (!(txtReturnID.Text.Length > 0))
                {
                    ms.AlertWarning("SFU1379"); //LOT을 입력해주세요
                    bReturn = false;
                    txtReturnResn.Focus();
                    return bReturn;
                }
                //사유 입력확인.
                if (!(txtReturnResn.Text.Length > 0))
                {
                    ms.AlertWarning("SFU1594"); //사유를 입력하세요.
                    bReturn = false;
                    txtReturnResn.Focus();
                    return bReturn;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }

        private void getReturnID_getLotInfo(DataTable dtINPUT_ID_LIST)
        {
            try
            {
                string sInput_Flag = "";
                //if ((bool)rdoRcvIss.IsChecked) sInput_Flag = "RCV";
                if ((bool)rdoPallet.IsChecked) sInput_Flag = "BOX";
                if ((bool)rdoCell.IsChecked) sInput_Flag = "LOT";



                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RETURN_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("INPUT_ID_FLAG", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RETURN_ID"] = txtReturnNumber.Text; //반품번호
                dr["USERID"] = LoginInfo.USERID;
                dr["INPUT_ID_FLAG"] = sInput_Flag;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                INDATA.Rows.Add(dr);

                dsInput.Tables.Add(INDATA);



                dsInput.Tables.Add(dtINPUT_ID_LIST);

                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_ID_CREATE", "INDATA,INPUT_ID_LIST", "OUTDATA,LOT_INFO", dsInput );

                //DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_RETURN_ID_CREATE", "INDATA,INPUT_ID_LIST", "OUTDATA,LOT_INFO", dsInput, null);

                loadingIndicator.Visibility = Visibility.Visible;

                //new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_ID_CREATE", "INDATA,INPUT_ID_LIST", "OUTDATA,LOT_INFO", (dsResult, dataException) =>
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_IWMS_RETURN_ID_CREATE", "INDATA,INPUT_ID_LIST", "OUTDATA,LOT_INFO", (dsResult, dataException) =>                
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (dataException != null)
                        {
                            //Util.AlertByBiz("BR_PRD_REG_RETURN_ID_CREATE", dataException.Message, dataException.ToString());                              
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        txtReturnNumber.Text = Util.NVC(dsResult.Tables["OUTDATA"].Rows[0]["RETURN_ID"]);
                                    }
                                }
                                if ((dsResult.Tables.IndexOf("LOT_INFO") > -1))
                                {
                                    if (dsResult.Tables["LOT_INFO"].Rows.Count > 0)
                                    {
                                        DataTable dtResult = dsResult.Tables["LOT_INFO"];

                                        string sFromSloc = Util.NVC(dtResult.Rows[0]["FROM_SLOC_ID"]);
                                        string sFromSlocName = Util.NVC(dtResult.Rows[0]["FROM_SLOC_NAME"]);
                                        string sToSloc = Util.NVC(dtResult.Rows[0]["TO_SLOC_ID"]);
                                        string sToSlocName = Util.NVC(dtResult.Rows[0]["TO_SLOC_NAME"]);
                                        string sProdid = Util.NVC(dtResult.Rows[0]["PRODID"]);

                                        //입고된 정보에 반대로 set 하여 반품.
                                        sFromArea = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                                        sToArea = Util.NVC(dtResult.Rows[0]["FROM_AREAID"]);
                                        sToShopID = Util.NVC(dtResult.Rows[0]["FROM_SHOP_ID"]);


                                        //PRODID String      제품ID
                                        //FROM_AREAID String
                                        //TO_AREAID   String

                                        // 2025.04.17 반품창고 SLOC_ID 변경 
                                        string rcvIssID = Util.NVC(dtResult.Rows[0]["RCV_ISS_ID"]);

                                        DataSet dsInputSloc = new DataSet();

                                        DataTable INDATA_SLOC = new DataTable();
                                        INDATA_SLOC.TableName = "INDATA";
                                        INDATA_SLOC.Columns.Add("AREAID", typeof(string));
                                        INDATA_SLOC.Columns.Add("SHOPID", typeof(string));
                                        INDATA_SLOC.Columns.Add("RCV_ISS_ID", typeof(string));
                                        INDATA_SLOC.Columns.Add("FROM_SLOC_ID", typeof(string));
                                        INDATA_SLOC.Columns.Add("TO_SLOC_ID", typeof(string));

                                        DataRow drSLOC = INDATA_SLOC.NewRow();
                                        drSLOC["AREAID"] = sFromArea;
                                        drSLOC["SHOPID"] = sToShopID;
                                        drSLOC["RCV_ISS_ID"] = rcvIssID; //반품번호
                                        drSLOC["FROM_SLOC_ID"] = sFromSloc;
                                        drSLOC["TO_SLOC_ID"] = sToSloc;

                                        INDATA_SLOC.Rows.Add(drSLOC);

                                        dsInputSloc.Tables.Add(INDATA_SLOC);

                                        DataSet dsSLOC_MAPPPING = new ClientProxy().ExecuteServiceSync_Multi("BR_SEL_RETURN_SLOC_MAPPING", "INDATA", "OUTPUT", dsInputSloc);

                                        string sToSloc2 = Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_ID"]);
                                        string sToSlocName2 = String.IsNullOrEmpty(Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_NAME"])) ? sToSlocName : Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["TO_SLOC_NAME"]);


                                        string sFromSloc2 = Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_ID"]);
                                        string sFromSlocName2 = String.IsNullOrEmpty(Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_NAME"])) ? sFromSlocName : Util.NVC(dsSLOC_MAPPPING.Tables[0].Rows[0]["FROM_SLOC_NAME"]);

                                        if (!(txtSLocFrom.Text.Length > 0))
                                        {
                                            txtSLocFrom.Text = sToSloc2 + " : " + sToSlocName2;
                                            txtSLocFrom.Tag = sToSloc2;
                                            txtSLocTo.Text = sFromSloc2 + " : " + sFromSlocName2;
                                            txtSLocTo.Tag = sFromSloc2;
                                            txtPRODID.Text = sProdid;

                                            dgTagetList.ItemsSource = DataTableConverter.Convert(dtResult);
                                            Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtResult.Rows.Count));
                                        }
                                        else
                                        {

                                            DataTable dtBefore = DataTableConverter.Convert(dgTagetList.ItemsSource);

                                            DataRow[] drTemp = dtBefore.Select("FROM_SLOC_ID = '" + sFromSloc + "'");
                                            if (!(drTemp.Length > 0))
                                            {
                                                ms.AlertWarning("SFU1556"); //반품창고가다른정보가존재합니다.
                                                return;
                                            }

                                            dtBefore.Merge(dtResult);
                                            dgTagetList.ItemsSource = DataTableConverter.Convert(dtBefore);
                                            Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, Util.NVC(dtBefore.Rows.Count));
                                        }


                                    }
                                }
                            }
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Refresh()
        {
            try
            {
                //반품목록 조회
                getReturnList();

                //그리드 clear
                Util.gridClear(dgTagetList);
                Util.SetTextBlockText_DataGridRowCount(tbTagetListCount, "0");

                txtReturnID.Text = string.Empty;
                //txtReturnFileName.Text = string.Empty;
                txtReturnNumber.Text = string.Empty;
                txtReturnResn.Text = string.Empty;

                txtSLocFrom.Text = string.Empty;
                txtSLocFrom.Tag = null;
                txtSLocTo.Text = string.Empty;
                txtSLocTo.Tag = null;

                txtPRODID.Text = string.Empty;
                sFromArea = string.Empty;
                sToArea = string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getReturnList()
        {
            try
            {
                //DA_PRD_SEL_TB_SFC_RCV_ISS_RETURN
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_STAT_CODE", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("FROM_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("TO_SLOC_ID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_STAT_CODE"] = Util.NVC(cboReturStatus.SelectedValue) == "" ? null : Util.NVC(cboReturStatus.SelectedValue);
                dr["FROMDATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["FROM_SLOC_ID"] = Util.NVC(cboSearchSLocFrom.SelectedValue) == "" ? null : Util.NVC(cboSearchSLocFrom.SelectedValue);
                dr["TO_SLOC_ID"] = Util.NVC(cboSearchSLocTo.SelectedValue) == "" ? null : Util.NVC(cboSearchSLocTo.SelectedValue);
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_ISS_RETURN", "RQSTDT", "RSLTDT", RQSTDT);
                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_IWMS_RETURN_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgSearchResultList, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RETURN_LIST_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void ReturnCell()
        {
            try
            {


                //BR_PRD_REG_RETURN_IWMS_PRODUCT_PACK
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("FROM_AREAID", typeof(string));
                INDATA.Columns.Add("FROM_SLOC_ID", typeof(string));
                INDATA.Columns.Add("TO_SHOP_ID", typeof(string));
                INDATA.Columns.Add("TO_AREAID", typeof(string));
                INDATA.Columns.Add("TO_SLOC_ID", typeof(string));
                INDATA.Columns.Add("ISS_QTY", typeof(string));
                INDATA.Columns.Add("ISS_NOTE", typeof(string));
                INDATA.Columns.Add("SHIPTO_ID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("IWMS_RCV_ISS_TYPE_CODE", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = txtReturnNumber.Text; //반품번호 = 입출고id
                dr["FROM_AREAID"] = sFromArea;
                dr["FROM_SLOC_ID"] = Util.NVC(txtSLocFrom.Tag); //Util.NVC(cboSLocFrom.SelectedValue);
                dr["TO_SHOP_ID"] = sToShopID;
                dr["TO_AREAID"] = sToArea;
                dr["TO_SLOC_ID"] = Util.NVC(txtSLocTo.Tag);//Util.NVC(cboSLocTo.SelectedValue);
                dr["ISS_QTY"] = dgTagetList.Rows.Count;
                dr["ISS_NOTE"] = "";
                dr["SHIPTO_ID"] = "";
                dr["USERID"] = LoginInfo.USERID;
                dr["IWMS_RCV_ISS_TYPE_CODE"] = cboTagetCategory.SelectedValue;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataRow drINLOT = null;
                DataTable INLOT = new DataTable();
                INLOT.TableName = "INLOT";
                INLOT.Columns.Add("LOTID", typeof(string));
                INLOT.Columns.Add("RTN_RSN_CODE", typeof(string));
                INLOT.Columns.Add("RTN_RSN_NOTE", typeof(string));



                for (int i = 0; i < dgTagetList.Rows.Count; i++)
                {
                    drINLOT = INLOT.NewRow();
                    drINLOT["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "LOTID"));
                    drINLOT["RTN_RSN_CODE"] = "";
                    drINLOT["RTN_RSN_NOTE"] = Util.NVC(DataTableConverter.GetValue(dgTagetList.Rows[i].DataItem, "RTN_RSN_NOTE"));
                    INLOT.Rows.Add(drINLOT);
                }
                dsInput.Tables.Add(INLOT);
                loadingIndicator.Visibility = Visibility.Visible;


                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_IWMS_PRODUCT_PACK", "INDATA,INLOT", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            //Util.AlertByBiz("BR_PRD_REG_RETURN_PRODUCT_PACK", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        //CELL을반품하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1324"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                                        Refresh();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnCell_Cancel()
        {
            try
            {
                //BR_PRD_REG_RETURN_IWMS_PRODUCT_CANCEL_PACK
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("RCV_ISS_ID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("CNCL_NOTE", typeof(string));
                INDATA.Columns.Add("NOTE", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = txtSelected_RCV_ISS_ID.Text; //반품번호 = 입출고id
                dr["AREAID"] = txtSelected_RCV_ISS_ID.Tag;
                dr["CNCL_NOTE"] = "";
                dr["NOTE"] = "";
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RETURN_IWMS_PRODUCT_CANCEL_PACK", "INDATA", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            //Util.AlertByBiz("BR_PRD_REG_RETURN_PRODUCT_CANCEL_PACK", dataException.Message, dataException.ToString());
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {
                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        //반품 취소하였습니다.
                                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU3259"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);

                                        Refresh_Selected_RCV();

                                        getReturnList();
                                    }
                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsInput);

                //, dsInput, null);

                //if (dsResult != null && dsResult.Tables.Count > 0)
                //{
                //    if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                //    {
                //        if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                //        {
                //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("CELL을반품하였습니다."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Refresh_Selected_RCV()
        {
            txtSelected_RCV_ISS_ID.Text = string.Empty;
            txtSelected_RCV_ISS_ID.Tag = string.Empty;
            txtSelected_ISS_QTY.Text = string.Empty;
            txtSelected_TO_SLOC_NAME.Text = string.Empty;

            btnReturnCancel.IsEnabled = true;
        }

        private void getReturnTagetCell_By_Excel()
        {
            try
            {
                OpenFileDialog fd = new OpenFileDialog();
                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (*.xlsx,*.xls)|*.xlsx;*.xls";

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        DataTable dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0, 0);

                        if (dtExcelData != null)
                        {
                            ReturnChkAndReturnCellCreate_ExcelOpen(dtExcelData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ReturnChkAndReturnCellCreate_ExcelOpen(DataTable dt)
        {
            try
            {
                if (dt != null)
                {
                    // 테이블생성해야함!!!

                    int intFirstRow = 0;
                    if (dt.Rows.Count > 0 && !(dt.Rows[0][0].ToString().Length > 0))
                    {
                        intFirstRow = 1;
                    }

                    DataTable INPUT_ID_LIST = new DataTable();
                    INPUT_ID_LIST.TableName = "INPUT_ID_LIST";
                    INPUT_ID_LIST.Columns.Add("RCV_ISS_ID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("BOXID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("LOTID", typeof(string));
                    INPUT_ID_LIST.Columns.Add("RTN_RSN_NOTE", typeof(string));


                    for (int i = intFirstRow; i < dt.Rows.Count; i++)
                    {
                        string sLotId = "";
                        string sNote = "";

                        if (dt.Rows[i][0] != null)
                        {
                            if (dt.Rows[i][0].ToString().Length > 0)
                            {
                                sLotId = dt.Rows[i][0].ToString();
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            continue;
                        }

                        if (dt.Rows[i][1] != null)
                        {
                            if (dt.Rows[i][1].ToString().Length > 0)
                            {
                                sNote = dt.Rows[i][1].ToString();
                            }
                        }
                        //INPUT_ID_LIST.Rows.Add(new object[] { null, null, dt.Rows[i][0], dt.Rows[i][1] });
                        INPUT_ID_LIST.Rows.Add(new object[] { null, null, sLotId, sNote });
                    }


                    getReturnID_getLotInfo(INPUT_ID_LIST);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgSearchResultList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    string sRCV_ISS_ID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_ID"));
                    string sISS_QTY = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "ISS_QTY"));
                    string sTO_SLOC_NAME = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "TO_SLOC_NAME"));
                    string sFROM_AREAID = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "FROM_AREAID"));
                    string sRCV_ISS_STAT_CODE = Util.NVC(DataTableConverter.GetValue(dgSearchResultList.Rows[cell.Row.Index].DataItem, "RCV_ISS_STAT_CODE"));

                    if (sRCV_ISS_STAT_CODE == "SHIPPING")
                    {
                        btnReturnCancel.IsEnabled = true;
                    }
                    else
                    {
                        btnReturnCancel.IsEnabled = false;
                    }

                    txtSelected_RCV_ISS_ID.Text = sRCV_ISS_ID;
                    txtSelected_RCV_ISS_ID.Tag = sFROM_AREAID;
                    txtSelected_ISS_QTY.Text = sISS_QTY;
                    txtSelected_TO_SLOC_NAME.Text = sTO_SLOC_NAME;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }



        private bool chkInputData()
        {
            bool bReturn = true;

            try
            {

                if (dgTagetList2.Rows.Count < 1)
                {
                    ms.AlertWarning("SFU1796"); //입고 대상이 없습니다. PALLETID를 입력 하세요.
                }

                if (cboTagetRoute.SelectedIndex < 0)
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return bReturn;
        }

        public void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        public void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void checkPopUpOpen()
        {
            try
            {
            
                ShowLoadingIndicator();
                DataSet dataSet = new DataSet();
                DataTable inData = dataSet.Tables.Add("INDATA");
                inData.Columns.Add("LANGID", typeof(string));
                inData.Columns.Add("MODLID", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("ROUTID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                //inData.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow indataRow = inData.NewRow();
                indataRow["LANGID"] = LoginInfo.LANGID;
                indataRow["MODLID"] = cboTagetModel.SelectedValue;
                indataRow["AREAID"] = sTagetArea;
                indataRow["ROUTID"] = cboTagetRoute.SelectedValue;
                indataRow["USERID"] = LoginInfo.USERID;
                //indataRow["RCV_ISS_ID"] = LoginInfo.USERID;

                inData.Rows.Add(indataRow);
                DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);
                var list = dt.AsEnumerable().GroupBy(r => new
                {
                    ISSIDGROUP = r.Field<string>("RCV_ISS_ID")
                }).Select(g => g.First());
                DataTable dtRCV_ISS_IDList = list.CopyToDataTable();
                var serWareHousingList = dtRCV_ISS_IDList.AsEnumerable().Where(row => Convert.ToBoolean(row["CHK"]) == true);
                //2024.03.11 E20240228-001206 KIM MIN SEOK
                if (serWareHousingList.ToList().Count == 0)
                {
                    ms.AlertWarning("SFU1636");//선택된 대상이 없습니다.
                    HiddenLoadingIndicator();
                    return;
                }


                DataTable dtPallet = dataSet.Tables.Add("RCV_ISS");

                dtRCV_ISS_IDList = serWareHousingList.CopyToDataTable();
                dtPallet.Columns.Add("RCV_ISS_ID", typeof(string));
                dtPallet.Columns.Add("PALLETID", typeof(string));

                DataRow rcvRow = null;
                for (int i = 0; i < dtRCV_ISS_IDList.Rows.Count; i++)
                {
                    rcvRow = dtPallet.NewRow();
                    rcvRow["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"].ToString());
                    rcvRow["PALLETID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["PALLETID"].ToString());
                    dtPallet.Rows.Add(rcvRow);
                }
                

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_IWMS_RETURN_CELL", "INDATA,RCV_ISS", "OUTDATA", (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.
                        Util.gridClear(dgTagetList2);

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }, dataSet
                );  

            }
            catch(Exception ex)
            {
                Util.MessageException(ex);                
            }

            #region 선택확인 팝업
            //    PACK001_037_RECEIVEPRODUCT_SELECTCHECK popup = new PACK001_037_RECEIVEPRODUCT_SELECTCHECK();
            //popup.FrameOperation = this.FrameOperation;
            //    if (popup != null)
            //    {
            //        DataTable dtData = new DataTable();
            //dtData.Columns.Add("MODELNAME", typeof(string));
            //        dtData.Columns.Add("PRODUCTNAME", typeof(string));
            //        dtData.Columns.Add("PRODID", typeof(string));
            //        dtData.Columns.Add("ROUTENAME", typeof(string));
            //        dtData.Columns.Add("LOTTYPE", typeof(string));

            //        DataRow newRow = null;

            //newRow = dtData.NewRow();
            //        newRow["MODELNAME"] = cboTagetModel.Text;
            //        newRow["PRODUCTNAME"] = txtTagetPRODNAME.Text;
            //        newRow["PRODID"] = txtTagetPRODID.Text;
            //        newRow["ROUTENAME"] = cboTagetRoute.Text;
            //        newRow["LOTTYPE"] = "";//cboTagetLotType.Text;
            //        dtData.Rows.Add(newRow);

            //        //========================================================================
            //        object[] Parameters = new object[2];
            //Parameters[0] = dtData;
            //        Parameters[1] = GetSaveWarehousing_DataSet();
            //C1WindowExtension.SetParameters(popup, Parameters);
            //        //========================================================================
            //        popup.Closed -= popup_Closed;
            //        popup.Closed += popup_Closed;
            //        popup.ShowModal();
            //        popup.CenterOnScreen();
            //    }
            #endregion



        }
void popup_Closed(object sender, EventArgs e)
        {
            PACK001_019_RECEIVEPRODUCT_SELECTCHECK popup = sender as PACK001_019_RECEIVEPRODUCT_SELECTCHECK;
            if (popup.DialogResult == MessageBoxResult.OK)
            {

                setWarehousing(); //입고처리


            }
        }
        /// <summary>
        /// 입고처리 함수
        /// </summary>
        private void setWarehousing()
        {
            try
            {

                DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);
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
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTagetList2.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[i].DataItem, "RCV_ISS_ID"));
                    drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[i].DataItem, "PALLETID"));
                    dtRCV_ISS.Rows.Add(drRCV_ISS);
                }

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                dsIndata.Tables.Add(dtRCV_ISS);
                loadingIndicator.Visibility = Visibility.Visible;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RECEIVE_IWMS_RETURN_CELL", "INDATA,RCV_ISS", "OUTDATA", (dsResult, dataException) =>
                {
                    try
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        if (dataException != null)
                        {
                            Util.MessageException(dataException);
                            return;
                        }
                        else
                        {
                            if (dsResult != null && dsResult.Tables.Count > 0)
                            {

                                if ((dsResult.Tables.IndexOf("OUTDATA") > -1))
                                {
                                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                                    {
                                        ms.AlertInfo("SFU1412"); //PALLET을입고하였습니다

                                        Util.gridClear(dgTagetList2);

                                        Util.SetTextBlockText_DataGridRowCount(tbTagetListCount2, "0");

                                        clearInput();

                                        getWareHousingData();//조회
                                    }

                                }
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }, dsIndata);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private DataSet GetSaveWarehousing_DataSet()
        {
            DataSet dsIndata = new DataSet();
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);
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
                    drINDATA["MODLID"] = cboTagetModel.SelectedValue;
                    drINDATA["AREAID"] = sTagetArea;
                    drINDATA["EQSGID"] = sTagetEqsg;
                    drINDATA["ROUTID"] = cboTagetRoute.SelectedValue;
                    drINDATA["USERID"] = LoginInfo.USERID;
                    drINDATA["RCV_ISS_ID"] = Util.NVC(dtRCV_ISS_IDList.Rows[i]["RCV_ISS_ID"]);
                    INDATA.Rows.Add(drINDATA);
                }


                DataRow drRCV_ISS = null;
                DataTable dtRCV_ISS = new DataTable();
                dtRCV_ISS.TableName = "RCV_ISS";
                dtRCV_ISS.Columns.Add("RCV_ISS_ID", typeof(string));
                dtRCV_ISS.Columns.Add("PALLETID", typeof(string));

                for (int i = 0; i < dgTagetList2.Rows.Count; i++)
                {
                    drRCV_ISS = dtRCV_ISS.NewRow();
                    drRCV_ISS["RCV_ISS_ID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[i].DataItem, "RCV_ISS_ID"));
                    drRCV_ISS["PALLETID"] = Util.NVC(DataTableConverter.GetValue(dgTagetList2.Rows[i].DataItem, "PALLETID"));
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
        /// <summary>
        /// 입고 조회
        /// </summary>
        private void getWareHousingData()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("FROMDATE", typeof(string));
                RQSTDT.Columns.Add("TODATE", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ROUTID"] = null;
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedValue) == "" ? null : Util.NVC(cboProductModel.SelectedValue); //null;//cboSearchModel.SelectedValue;
                dr["PRODID"] = null;//cboSearchProduct.SelectedValue;
                dr["FROMDATE"] = dtpDateFrom2.SelectedDateTime.ToString("yyyyMMdd");
                dr["TODATE"] = dtpDateTo2.SelectedDateTime.ToString("yyyyMMdd");
                dr["PALLETID"] = null;//txtSearchPallet.Text;
                dr["LOTID"] = null;//txtSearchLot.Text;
                dr["EQSGID"] = Util.NVC(cboSearchEQSGID.SelectedValue) == "" ? null : Util.NVC(cboSearchEQSGID.SelectedValue);
                dr["AREAID"] = Util.NVC(cboSearchAREAID.SelectedValue) == "" ? null : Util.NVC(cboSearchAREAID.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RECEIVE_PRODUCT_IWMS_PACK", "RQSTDT", "RSLTDT", RQSTDT);


                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgSearchResultList2, dtResult, FrameOperation, true);
                Util.SetTextBlockText_DataGridRowCount(tbSearchListCount2, Util.NVC(dtResult.Rows.Count));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        /// <summary>
        /// 팔레트의 입고대기 lot 정보 조회
        /// </summary>
        private void getTagetPalletInfo()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                //2018.07.27
                RQSTDT.Columns.Add("OCV_FLAG", typeof(string));
                RQSTDT.Columns.Add("DCIR_FLAG", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = txtPalletID.Text;
                //2018.07.27
                if ((bool)chkFCS.IsChecked)
                {
                    dr["OCV_FLAG"] = "Y";
                }
                else
                {
                    dr["OCV_FLAG"] = null;
                }
                if ((bool)chkDCIR.IsChecked)
                {
                    dr["DCIR_FLAG"] = "Y";
                }
                else
                {
                    dr["DCIR_FLAG"] = null;
                }

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_RCV_IWMS_RETURN_PLLT_BOX", "RQSTDT", "RSLTDT", RQSTDT);

                if (!dgTagetList2_ChkValridtion(dtResult))
                {
                    return;
                }

                if (chkPalletInput(dtResult))
                {
                    txtTagetPRODID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                    txtTagetPRODNAME.Text = Util.NVC(dtResult.Rows[0]["PRODNAME"]);
                    sTagetArea = Util.NVC(dtResult.Rows[0]["TO_AREAID"]);
                    //생산예정모델 콤보 셋팅
                    setComboBox_Model_schd(txtTagetPRODID.Text);
                    //dgTagetList.ItemsSource = DataTableConverter.Convert(dtResult);
                    DataTable dtBefore = DataTableConverter.Convert(dgTagetList2.ItemsSource);
                    dtBefore.Merge(dtResult);
                    dgTagetList2.ItemsSource = DataTableConverter.Convert(dtBefore);
                    Util.SetTextBlockText_DataGridRowCount(tbTagetListCount2, Util.NVC(dtBefore.Rows.Count));
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        //2018.05.14

        //2018.05.14
        private bool chkPalletInput(DataTable dtResult)
        {
            bool bResult = true;
            try
            {
                if (!(dtResult.Rows.Count > 0))
                {
                    ms.AlertWarning("SFU1888"); //정보없는ID입니다
                    bResult = false;
                    return bResult;
                }
                if (Util.NVC(dtResult.Rows[0]["RCV_ISS_STAT_CODE"]) == "END_RECEIVE")
                {
                    ms.AlertWarning("SFU1800"); //입고완료된ID입니다
                    bResult = false;
                    return bResult;
                }

                #region 입력된이전값과 비교
                DataTable dtBefore = DataTableConverter.Convert(dgTagetList2.ItemsSource);
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
                    ms.AlertWarning("SFU1410", sCheckPalletId); //PALLETID는중복입력할수없습니다.\r\n({0})
                    bResult = false;
                    return bResult;
                }
                #endregion

                if (txtTagetPRODID.Text != "")
                {
                    if (txtTagetPRODID.Text != Util.NVC(dtResult.Rows[0]["PRODID"]))
                    {
                        ms.AlertWarning("SFU1481"); //다른제품ID를입고할수없습니다.
                        bResult = false;
                        return bResult;
                    }
                }
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
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboSearchAREAIDChild = { cboSearchEQSGID };
                _combo.SetCombo(cboSearchAREAID, CommonCombo.ComboStatus.SELECT, cbChild: cboSearchAREAIDChild, sFilter: sFilter, sCase: "AREA_AREATYPE");

                //작업자 조회 라인
                C1ComboBox[] cboSearchEQSGIDParent = { cboSearchAREAID };
                C1ComboBox[] cboSearchEQSGIDChild = { cboProductModel };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.ALL, cbParent: cboSearchEQSGIDParent, cbChild: cboSearchEQSGIDChild, sCase: "EQUIPMENTSEGMENT");

                //모델     
                C1ComboBox[] cboProductModelParent = { cboSearchAREAID, cboSearchEQSGID };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbParent: cboProductModelParent, sCase: "PRJ_MODEL");
                #endregion


                CommonCombo combo = new CommonCombo();

                C1ComboBox[] cboSearchSLocFrom_Child = { cboSearchSLocTo };

                string[] sFilter2 = new string[3];

                if (LoginInfo.CFG_SHOP_ID != "A040")
                {
                    sFilter2[0] = LoginInfo.CFG_SHOP_ID;
                    sFilter2[1] = LoginInfo.CFG_AREA_ID;
                    sFilter2[2] = SLOC_TYPE_CODE.SLOC02 + "," + SLOC_TYPE_CODE.SLOC03;
                }
                else
                {
                    sFilter2[0] = LoginInfo.CFG_SHOP_ID;
                    sFilter2[1] = LoginInfo.CFG_AREA_ID;
                    sFilter2[2] = SLOC_TYPE_CODE.SLOC02;
                }
                combo.SetCombo(cboSearchSLocFrom, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, cbChild: cboSearchSLocFrom_Child, sCase: "SLOC");


                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;

                C1ComboBox cboSLOC_TYPE_CODE = new C1ComboBox();
                //2017.10.17
                if (LoginInfo.CFG_SHOP_ID != "A040")
                {
                    cboSLOC_TYPE_CODE.SelectedValue = "SLOC02,SLOC03";
                }
                else
                {
                    cboSLOC_TYPE_CODE.SelectedValue = SLOC_TYPE_CODE.SLOC03;
                }

                C1ComboBox cboSHIP_TYPE_CODE = new C1ComboBox();
                cboSHIP_TYPE_CODE.SelectedValue = Ship_Type.CELL;

                C1ComboBox cboShip_Proc = new C1ComboBox();
                cboShip_Proc.SelectedValue = Process.CELL_BOXING;

                //C1ComboBox[] cboSearchSLocTo_Parent = { cboSHOPID, cboSearchSLocFrom, cboSLOC_TYPE_CODE, cboSHIP_TYPE_CODE, cboShip_Proc };
                //combo.SetCombo(cboSearchSLocTo, CommonCombo.ComboStatus.ALL, cbParent: cboSearchSLocTo_Parent, sCase: "SLOC_BY_TOSLOC_PROC");

                //C1ComboBox cboAREAID = new C1ComboBox();
                //cboAREAID.SelectedValue = LoginInfo.CFG_AREA_ID;

                //C1ComboBox cboCOM_TYPE_CODE = new C1ComboBox();
                //cboCOM_TYPE_CODE.SelectedValue = "PACK_IWMS_RETURN_WH";                

                //C1ComboBox[] cboSearchSLocTo_Parent = { cboAREAID, cboCOM_TYPE_CODE };

                String[] sFilter4 = { LoginInfo.CFG_AREA_ID, "PACK_IWMS_RETURN_WH" };
                combo.SetCombo(cboSearchSLocTo, CommonCombo.ComboStatus.ALL, sFilter: sFilter4, sCase: "AREA_COMMON_CODE_BY_SLOC");
                
                

                String[] sFilter3 = { "RETURN_RCV_ISS_STAT_CODE" };
                combo.SetCombo(cboReturStatus, CommonCombo.ComboStatus.ALL, sFilter: sFilter3, sCase: "COMMCODE");

                //2018.05.14 


                #region OLD
                /*                
                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = null; //LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;
                C1ComboBox cboAreaByAreaType = new C1ComboBox();
                cboAreaByAreaType.SelectedValue = null;
                C1ComboBox cboEquipmentSegment = new C1ComboBox();
                cboEquipmentSegment.SelectedValue = null;
                C1ComboBox cboProductType = new C1ComboBox();
                cboProductType.SelectedValue = "CELL";

                //입고대상입력 모델
                C1ComboBox[] cboTagetProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                C1ComboBox[] cboTagetProductModelChild = { cboTagetProduct , cboTagetRoute };
                _combo.SetCombo(cboTagetModel, CommonCombo.ComboStatus.NONE, cbChild: cboTagetProductModelChild, cbParent: cboTagetProductModelParent , sCase: "PRODUCTMODEL");

                //입고대상입력 제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboTagetModel, cboAREA_TYPE_CODE, cboProductType };
                C1ComboBox[] cboTagetProductChild = { cboTagetRoute };
                _combo.SetCombo(cboTagetProduct, CommonCombo.ComboStatus.ALL, cbChild: cboTagetProductChild, cbParent: cboProductParent, sCase: "PRODUCT");

                //입고대상입력 LOTTYPE
                _combo.SetCombo(cboTagetLotType, CommonCombo.ComboStatus.NONE, null, sCase: "LOTTYPE");

                //입고대상입력 route
                C1ComboBox[] cboTagetRouteParent = { cboTagetModel, cboTagetProduct };
                _combo.SetCombo(cboTagetRoute, CommonCombo.ComboStatus.NONE, cbParent: cboTagetRouteParent, sCase: "ROUTEBYMODLID");

                //입고대상 Line
                string[] sTagetEqsgFilter = { LoginInfo.CFG_SHOP_ID, null, Area_Type.PACK };
                _combo.SetCombo(cboTagetEQSGID, CommonCombo.ComboStatus.NONE, null, sFilter: sTagetEqsgFilter ,sCase: "EQUIPMENTSEGMENT_AREATYPE");


                //조회 Line
                string[] sFilter = { LoginInfo.CFG_SHOP_ID, null, Area_Type.PACK };
                _combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.NONE, null, sFilter: sFilter, sCase: "EQUIPMENTSEGMENT_AREATYPE");
                */


                /*
                //조회 모델
                C1ComboBox[] cboSearchModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                _combo.SetCombo(cboSearchModel, CommonCombo.ComboStatus.ALL, cbParent: cboSearchModelParent, sCase: "PRODUCTMODEL");

                //조회 제품
                C1ComboBox[] cboSearchProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboSearchModel, cboAREA_TYPE_CODE, cboProductType };
                C1ComboBox[] cboSearchProductChild = { cboSearchRoute };
                _combo.SetCombo(cboSearchProduct, CommonCombo.ComboStatus.ALL, cbChild: cboSearchProductChild, cbParent: cboSearchProductParent, sCase: "PRODUCT");

                //조회 route
                C1ComboBox[] cboSearchRouteParent = { cboSearchModel, cboSearchProduct };
                _combo.SetCombo(cboSearchRoute, CommonCombo.ComboStatus.NONE, cbParent: cboSearchRouteParent, sCase: "ROUTEBYMODLID");
                */

                //"EQUIPMENTSEGMENT_AREATYPE");

                /*
                
                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProcess, cboProductModel };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);

                //공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, null, cbParent: cbProcessParent);

                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;
                //모델
                C1ComboBox[] cboProductModelChild = { cboProductType, cboProduct };
                C1ComboBox[] cboProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent);

                //제품 CLASS TYPE : CELL CMA BMA
                C1ComboBox[] cboProductTypeChild = { cboProduct };
                C1ComboBox[] cboProductTypeParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                _combo.SetCombo(cboProductType, CommonCombo.ComboStatus.ALL, cbChild: cboProductTypeChild, cbParent: cboProductTypeParent);

                //제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProductModel, cboAREA_TYPE_CODE, cboProductType };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, null, cbParent: cboProductParent);
                */
                #endregion

                setComboBox_ReturnType(cboTagetCategory, CommonCombo.ComboStatus.NONE);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_ReturnType(C1ComboBox cbo, CommonCombo.ComboStatus cs)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "IWMS_RCV_ISS_TYPE_CODE";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURN_TYPE", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                cbo.ItemsSource = DataTableConverter.Convert(dtResult);

                cbo.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;

            }
        }


        /// <summary>
        /// 생산예정모델 콤보 셋팅
        /// </summary>
        private void setComboBox_Model_schd(string sMTRLID)
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
                drIndata["AREAID"] = sTagetArea == "" ? null : sTagetArea;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_MODLID_BY_MTRLID_MBOM_DETL_CBO", "INDATA", "OUTDATA", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
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
                    }


                    cboTagetModel.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetModel.SelectedIndex = 0;
                        cboTagetModel_SelectedValueChanged(null, null);
                    }
                    else
                    {
                        cboTagetModel_SelectedValueChanged(null, null);
                    }

                }
                );
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
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
                drIndata["AREAID"] = sTagetArea == "" ? null : sTagetArea;
                dtIndata.Rows.Add(drIndata);
                new ClientProxy().ExecuteService("DA_BAS_SEL_ROUTID_BY_MTRLID_MBOM_DETL_CBO", "RQSTDT", "RSLTDT", dtIndata, (dtResult, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    cboTagetRoute.ItemsSource = DataTableConverter.Convert(dtResult);

                    if (dtIndata.Rows.Count > 0)
                    {
                        cboTagetRoute.SelectedIndex = 0;
                    }
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
                DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);
                if (dt.Select("OCV_FLAG = 'N'").Length > 0) //OCV존재 여부 체크
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

        private bool ChkDCIR_Exist()
        {
            bool bReturn = false;
            try
            {
                DataTable dt = DataTableConverter.Convert(dgTagetList2.ItemsSource);
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

        private void popUpOpenPalletInfo(string sRcvIssId, string sPalletId, string TabMenu)
        {
            try
            {
                PACK001_037_RECEIVEPRODUCT_PALLETINFO popup = new PACK001_037_RECEIVEPRODUCT_PALLETINFO();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[3];
                    Parameters[0] = sRcvIssId;
                    Parameters[1] = sPalletId;
                    Parameters[2] = TabMenu;
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


      
        private void clearInput()
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
        }

        private void getOcvData(object sender)
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
                        Util.MessageException(ex);
                        return;
                    }
                    else
                    {

                        if (result.Rows.Count > 0)
                        {
                            string sOcv_Flag = Util.NVC(result.Rows[0]["OCV_FLAG"]);



                            DataTableConverter.SetValue(dgTagetList2.Rows[iRow].DataItem, "OCV_FLAG", sOcv_Flag);
                            C1.WPF.DataGrid.DataGridCell cell = dgTagetList2.GetCell(iRow, dgTagetList2.Columns["OCV_FLAG"].Index);
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

            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.Alert(ex.ToString());
            }

        }

        private bool Check_ReturnConfirm()
        {
            bool bReturn = true;
            try
            {
                //목록 확인
                if (!(dgTagetList.Rows.Count > 0))
                {
                    ms.AlertWarning("SFU1553"); //반품CELL을입력하세요.
                    bReturn = false;
                    return bReturn;
                }
                //반품창고선택확인
                //if (Util.NVC(cboSLocTo.SelectedValue) == "SELECT")
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("반품창고를선택하세요."), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //    bReturn = false;
                //    cboSLocTo.Focus();
                //    return bReturn;
                //}

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturn;
        }


        /// <summary>
        /// 반품테그 발행정보 조회
        /// </summary>
        /// <returns></returns>
        private DataTable getReturnTagInfo(string sRcv_iss_id)
        {
            DataTable dtReturn = null;
            try
            {
                //DA_PRD_SEL_TB_SFC_RCV_ISS_RETURN
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RCV_ISS_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RCV_ISS_ID"] = sRcv_iss_id;
                RQSTDT.Rows.Add(dr);

                dtReturn = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RETURNTAG_INFO", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RETURNTAG_INFO", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
            return dtReturn;
        }

        private void printReturnTag(DataTable dtReturnTagInfo)
        {
            try
            {
                if (!(dtReturnTagInfo.Rows.Count > 0))
                {
                    //Util.AlertInfo("발행정보가 없습니다.");
                    ms.AlertWarning("SFU3399"); //발행정보가 없습니다.
                    return;
                }
                ArrayList arrColumnNamesTemp = new ArrayList();

                string sNumberString = string.Empty;
                string sColumnName_LOT = string.Empty;
                string sColumnName_RETURN_NOTE = string.Empty;
                string sColumnName_CNT = string.Empty;

                for (int i = 0; i < dtReturnTagInfo.Rows.Count; i++)
                {
                    int iTemp = i + 1;
                    sNumberString = iTemp.ToString("00.##");
                    sColumnName_LOT = "LOT_" + sNumberString;
                    sColumnName_RETURN_NOTE = "RETURN_NOTE_" + sNumberString;
                    sColumnName_CNT = "CNT_" + sNumberString;
                    string[] sColumnNames = { sColumnName_LOT, sColumnName_RETURN_NOTE, sColumnName_CNT };
                    arrColumnNamesTemp.Add(sColumnNames);
                }

                DataTable dtReturnTag = new DataTable();
                dtReturnTag.TableName = "dtReturnTag";
                dtReturnTag.Columns.Add("PRODUCT_NAME", typeof(string));            // 제품명
                dtReturnTag.Columns.Add("RETURN_NUMBER", typeof(string));           // 반품번호
                dtReturnTag.Columns.Add("RETURN_NUMBER_BARCORD", typeof(string));   // 반품번호바코드
                dtReturnTag.Columns.Add("RETURN_DATE", typeof(string));             // 작업일자
                dtReturnTag.Columns.Add("TOTAL_COUNT", typeof(string));             // 제품수량
                dtReturnTag.Columns.Add("PRODUCTID", typeof(string));               // 제품ID
                dtReturnTag.Columns.Add("USER_NAME", typeof(string));               // 작업자
                dtReturnTag.Columns.Add("OUT_POSITION", typeof(string));            // 출고창고
                dtReturnTag.Columns.Add("IN_POSITION", typeof(string));             // 입고창고     

                if (arrColumnNamesTemp != null)
                {
                    for (int i = 0; i < arrColumnNamesTemp.Count; i++)
                    {
                        string[] sColumnNames = (string[])arrColumnNamesTemp[i];

                        dtReturnTag.Columns.Add(sColumnNames[0], typeof(string));
                        dtReturnTag.Columns.Add(sColumnNames[1], typeof(string));
                        dtReturnTag.Columns.Add(sColumnNames[2], typeof(string));
                    }
                }

                DataRow dr = dtReturnTag.NewRow();
                dr["PRODUCT_NAME"] = Util.NVC(dtReturnTagInfo.Rows[0]["PRODNAME"]);            // 제품명
                dr["RETURN_NUMBER"] = Util.NVC(dtReturnTagInfo.Rows[0]["RCV_ISS_ID"]);           // 반품번호
                dr["RETURN_NUMBER_BARCORD"] = Util.NVC(dtReturnTagInfo.Rows[0]["RCV_ISS_ID"]);   // 반품번호바코드
                dr["RETURN_DATE"] = Util.NVC(dtReturnTagInfo.Rows[0]["ISS_DTTM"]);             // 작업일자
                dr["TOTAL_COUNT"] = Util.NVC(dtReturnTagInfo.Rows[0]["TOTAL_QTY"]);             // 제품수량
                dr["PRODUCTID"] = Util.NVC(dtReturnTagInfo.Rows[0]["PRODID"]);               // 제품ID
                dr["USER_NAME"] = Util.NVC(dtReturnTagInfo.Rows[0]["INSUSER"]);               // 작업자
                dr["OUT_POSITION"] = Util.NVC(dtReturnTagInfo.Rows[0]["FROM_SLOC_ID"]);            // 출고창고
                //FROM_SLOC_NAME
                dr["IN_POSITION"] = Util.NVC(dtReturnTagInfo.Rows[0]["TO_SLOC_ID"]);             // 입고창고    
                //TO_SLOC_NAME
                if (arrColumnNamesTemp != null)
                {
                    for (int i = 0; i < arrColumnNamesTemp.Count; i++)
                    {
                        string[] sColumnNames = (string[])arrColumnNamesTemp[i];

                        dr[sColumnNames[0]] = Util.NVC(dtReturnTagInfo.Rows[i]["LOTID"]);
                        dr[sColumnNames[1]] = Util.NVC(dtReturnTagInfo.Rows[i]["RTN_RSN_NOTE"]);
                        dr[sColumnNames[2]] = Util.NVC(dtReturnTagInfo.Rows[i]["LOT_CNT"]);
                    }
                }
                dtReturnTag.Rows.Add(dr);

                LGC.GMES.MES.PACK001.Report_Multi rs = new LGC.GMES.MES.PACK001.Report_Multi();
                rs.FrameOperation = this.FrameOperation;

                if (rs != null)
                {
                    // 태그 발행 창 화면에 띄움.
                    object[] Parameters = new object[2];
                    Parameters[0] = "ReturnTag"; // "PalletHis_Tag";
                    Parameters[1] = dtReturnTag;

                    C1WindowExtension.SetParameters(rs, Parameters);

                    rs.Closed += new EventHandler(printPopUp_Closed);
                    this.Dispatcher.BeginInvoke(new Action(() => rs.ShowModal()));

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void printPopUp_Closed(object sender, EventArgs e)
        {
            try
            {
                LGC.GMES.MES.PACK001.Report_Multi printPopUp = sender as LGC.GMES.MES.PACK001.Report_Multi;
                if (printPopUp.DialogResult == MessageBoxResult.OK)
                {
                    Refresh_Selected_RCV();
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }



        #endregion Mehod

        private void btnReturnFileUpload_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                getReturnTagetCell_By_Excel();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgSearchResultList2_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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

        private void dgSearchResultList2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //PALLETID
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgSearchResultList2.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    if (cell.Row.Index > -1)
                    {
                        if (cell.Column.Name == "PALLETID")
                        {
                            sTabMenu = "dgSearchResultList2";
                            string sRcvIssId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList2.Rows[cell.Row.Index].DataItem, "IWMS_RCV_ID"));
                            string sSelectPalletId = Util.NVC(DataTableConverter.GetValue(dgSearchResultList2.Rows[cell.Row.Index].DataItem, "PALLETID"));

                            popUpOpenPalletInfo(sRcvIssId, sSelectPalletId,sTabMenu);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
    }
}
