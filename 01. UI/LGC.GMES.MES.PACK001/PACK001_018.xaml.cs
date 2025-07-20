/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2018.06.29  손우석  여러 장 바코드 발행시 SLEEP 추가
  2018.10.17  손우석  Slow 쿼리 처리를 위한 파라미터 수정
  2020.04.03  염규범  Keypart 시리얼 번호로 검색 가능하도록 변경 처리의 건
  2024.01.26  주재홍  Lot 정보 조회(Pack) 메뉴 내 ‘선별 LOT정보’ Tap을 신규 개발하여 선별한 LOT의 관련 상세 정보를 제공
  2024.04.03  강송모  Lot 정보 조회(Pack) 메뉴 내 ‘선별 LOT정보’ 기능 개선 및 변경
  2024.04.08  강송모  'Multi-LOT 정보' 탭 동 정보 조건 제거
  2024.04.24  강송모  'Multi-LOT 정보' 탭 조회 성능 개선 및 UI 수정
  2025.05.09  유현민  선별Lot정보 탭 활성화
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using Microsoft.Win32;

using System.Diagnostics;
using System.Collections;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_018 : UserControl, IWorkArea
    {
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter()
        {
            Background = new SolidColorBrush(Colors.Transparent),
            MouseOverBrush = new SolidColorBrush(Colors.Transparent),
        };

        CheckBox chkAll = new CheckBox()
        {
            Content = "",
            IsChecked = true,
            Background = new SolidColorBrush(Colors.Transparent),
            VerticalAlignment = System.Windows.VerticalAlignment.Center,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center
        };
        
        private bool blPrintStop = true;
        private BackgroundWorker bkWorker;

        //2018.10.17
        private int iCnt = 0;

        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        public PACK001_018()
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
                dateSearchType_Cbo();

                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnLabel);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                

                gdContent.ColumnDefinitions[0].Width = new GridLength(0);
                gdContent.ColumnDefinitions[1].Width = new GridLength(0);
                gdContentTitle.ColumnDefinitions[2].Width = new GridLength(8);

                gdContent.ColumnDefinitions[4].Width = new GridLength(0);
                gdContent.ColumnDefinitions[3].Width = new GridLength(0);
                gdContentTitle.ColumnDefinitions[10].Width = new GridLength(8);
                
                setComboBox();

                setComboBox_History();

                setComboBxo_Keypart();

                bkWorker = new System.ComponentModel.BackgroundWorker();
                bkWorker.WorkerSupportsCancellation = true;
                bkWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bkWorker_DoWork);
                bkWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);

                tbLotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbhistory_LotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbKeypart_LotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                tbselectLot_LotListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                txtInputLotCount.Text = ObjectDic.Instance.GetObjectName("입력") + ": 0" + ObjectDic.Instance.GetObjectName("건");
                txtNotice.Text = MessageDic.Instance.GetMessage("FM_ME_0458", "20,000");  // 0개 까지 조회 가능합니다.

                //ChooseLotInfo.Visibility = Visibility.Collapsed; // 2024.12.11 김영국 - 선별 LOT정보 Tab은 사용하지 않으므로 숨김처리. - 2025.05.09 활성화

                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtselectLotSearch_ChangedEventHandler(object sender, TextChangedEventArgs args)
        {
            try
            {
                string count = "";

                if (txtLotID.Text.Equals(""))
                {
                    count = "0";
                }
                else
                {
                    string[] totalInputArray = txtLotID.Text.Split(',');

                    ArrayList list = new ArrayList();

                    for (int i = 0; i < totalInputArray.Length; i++)
                    {
                        string s = totalInputArray[i];
                        if (string.IsNullOrWhiteSpace(s))
                        {
                            continue;
                        }
                        list.Add(s.Trim());
                    }

                    totalInputArray = (string[])list.ToArray(typeof(string));

                    count = totalInputArray.Length.ToString();
                }
                
                txtInputLotCount.Text = ObjectDic.Instance.GetObjectName("입력") + ": " + count + ObjectDic.Instance.GetObjectName("건");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region Event - Button

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2018.10.17
                if (!ValidationWindingLotSearch()) return;

                getLotList(null);
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
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        private void btnselectLot_Excel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgselectLotLotList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!bkWorker.IsBusy)
                {
                    bkWorker.RunWorkerAsync();
                    blPrintStop = false;
                }
                else
                {
                    btnLabel.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        btnLabel.Content = ObjectDic.Instance.GetObjectName("출력");
                    });
                    blPrintStop = true;
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnLeft_Checked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[0].Width = new GridLength(300);
                gdContent.ColumnDefinitions[1].Width = new GridLength(8);
                gdContentTitle.ColumnDefinitions[2].Width = new GridLength(290);
            }
        }

        private void btnLeft_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[0].Width = new GridLength(0);
                gdContent.ColumnDefinitions[1].Width = new GridLength(0);
                gdContentTitle.ColumnDefinitions[2].Width = new GridLength(8);
            }
        }

        private void btnSearch_id_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextRange textRange = new TextRange(txtNote_IDList.Document.ContentStart, txtNote_IDList.Document.ContentEnd);

                if (textRange.Text.Length > 0)
                {
                    string sIDList = textRange.Text.Replace("\r\n", ",");
                    sIDList = sIDList.Replace(" ", "");

                    if (sIDList.Length > 0)
                    {
                        getLotList(sIDList);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnRight_Checked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[4].Width = new GridLength(0);
                gdContent.ColumnDefinitions[3].Width = new GridLength(0);
                gdContentTitle.ColumnDefinitions[10].Width = new GridLength(8);
            }
        }

        private void btnRight_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gdContent != null)
            {
                gdContent.ColumnDefinitions[4].Width = new GridLength(300);
                gdContent.ColumnDefinitions[3].Width = new GridLength(8);
                gdContentTitle.ColumnDefinitions[10].Width = new GridLength(290);
            }
        }

        private void btnLabel_view_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtResult = null;
                string szpl = string.Empty;
                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                string[] sLot_List = System.Text.RegularExpressions.Regex.Split(textRange.Text.Trim(), @"\r\n");

                string sLotid = string.Empty;

                for (int i = 0; i < sLot_List.Length; i++)
                {
                    if (sLot_List[i].Length > 0)
                    {
                        sLotid = sLot_List[i];
                        break;
                    }
                }

                if (sLotid.Length > 0)
                {
                    dtResult = Util.getZPL_Pack(sLOTID: sLotid
                                               , sLABEL_TYPE : LABEL_TYPE_CODE.PACK
                                               , sLABEL_CODE : Util.NVC(cboLabelCode.SelectedValue)
                                               , sSAMPLE_FLAG : "N"
                                               , sPRN_QTY: "1"
                                               , sPRODID : txtLabelProdID.Text
                                               , sDPI : Util.NVC(cboLabelDPI.SelectedValue)
                                               );
                }
                else
                {
                    dtResult = Util.getZPL_Pack( sLABEL_TYPE: LABEL_TYPE_CODE.PACK
                                               , sLABEL_CODE: Util.NVC(cboLabelCode.SelectedValue)
                                               , sSAMPLE_FLAG: "Y"
                                               , sPRN_QTY: "1"
                                               , sPRODID: txtLabelProdID.Text
                                               , sDPI: Util.NVC(cboLabelDPI.SelectedValue)
                                               );
                }

                szpl = Util.NVC(dtResult.Rows[0]["ZPLSTRING"]);

                CMM_ZPL_VIEWER2 wndPopup = new CMM_ZPL_VIEWER2(szpl);
                wndPopup.Show();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        
        private void btnPrintList_Clear_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
            textRange.Text = "";
            txtLabelProdID.Text = "";
            cboLabelCode.ItemsSource = null;
            cboLabelCode.SelectedValue = null;
        }

        private void btnhistory_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2020.12.14 염규범S
                //검색 조건을 1주일로 변경
                //과거 데이터 조회를 위해서 1주일로 변경  
                if (!ValidationTime()) return;

                getLotList_Act();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnselectLot_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                getSelectLotList_Act();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void btnhistory_Excel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotHistoryList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnKeypart_Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(!(txtKeypartID.Text.Length >= 4))
                {
                    Util.MessageInfo("SFU3450"); //Lot ID는 4자리 이상 넣어 주세요.
                    return;
                }


                // 2020.04.03 염규범S
                // Keypart 시리얼 번호로 검색 가능하게 처리
                if (string.IsNullOrWhiteSpace(cboEquipmentSegmentKerpart.SelectedValue.ToString()))
                {
                    Util.MessageInfo("SFU1223"); //라인을 입력하세요
                    return;
                }

                getKeypart_Tracking();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnKeypart_Excel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLotKeypartList);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnSelectLot_SearchExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dtExcelData = GetExcelData();

                if (dtExcelData != null)
                {
                    loadingIndicator.Visibility = Visibility.Visible;

                    StringBuilder inputLotIDs = new StringBuilder();

                    for (int inx = 1; inx < dtExcelData.Rows.Count; inx++)
                    {
                        string input = Util.NVC(dtExcelData.Rows[inx][0]);
                        if (string.IsNullOrEmpty(input))
                        {
                            continue;
                        }

                        inputLotIDs.Append(input);
                        inputLotIDs.Append(",");

                        System.Windows.Forms.Application.DoEvents();
                    }

                    txtLotID.Text = inputLotIDs.ToString();
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
        #endregion

        #region Event - CheckBox
        private void chkToday_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
                cboTimeTo.SelectedValue = "23:59:59";

                dtpDateFrom.IsEnabled = false;
                dtpDateTo.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkToday_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtpDateFrom.IsEnabled = true;
                dtpDateTo.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }


        private void chkhistory_Today_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtphistory_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now;
                dtphistory_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
                cbohistory_TimeTo.SelectedValue = "23:59:59";

                dtphistory_DateFrom.IsEnabled = false;
                dtphistory_DateTo.IsEnabled = false;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void chkhistory_Today_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                dtphistory_DateFrom.IsEnabled = true;
                dtphistory_DateTo.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        #endregion

        #region Event - TextBox
        private void txtIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtIDInput.Text.Length > 0)
                    {
                        TextRange textRange = new TextRange(txtNote_IDList.Document.ContentStart, txtNote_IDList.Document.ContentEnd);
                        string sLotList = textRange.Text.Replace("\r\n", "").Trim();
                        if (sLotList.Length > 0)
                        {
                            txtNote_IDList.AppendText("\r\n" + txtIDInput.Text);
                        }
                        else
                        {
                            txtNote_IDList.AppendText(txtIDInput.Text);
                        }
                        //sLotList = sLotList + "\r\n" + txtIDInput.Text;

                        //2018.10.17
                        iCnt = iCnt + 1;

                        txtIDInput.SelectAll();
                    }
                    //2018.10.17
                    else
                    {
                        iCnt = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtPrintIDInput_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (txtPrintIDInput.Text.Length > 0)
                    {
                        TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                        string sLotList = textRange.Text.Replace("\r\n", "").Trim();

                        if (sLotList.Contains(txtPrintIDInput.Text))
                        {
                            return;
                        }

                        if (chkLot_LabelCode(txtPrintIDInput.Text))
                        {
                            txtNote_PrintIDList.AppendText("\r\n" + txtPrintIDInput.Text + "\r\n");

                            //if (sLotList.Length > 0)
                            //{
                            //    txtNote_PrintIDList.AppendText( txtPrintIDInput.Text+ "\r\n");
                            //}
                            //else
                            //{
                            //    txtNote_PrintIDList.AppendText(txtPrintIDInput.Text);
                            //}
                            //sLotList = sLotList + "\r\n" + txtIDInput.Text;

                            textRange.Text = textRange.Text.Replace("\r\n\r\n", "\r\n");

                            txtPrintIDInput.SelectAll();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void txtNote_PrintIDList_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
            string sText = textRange.Text;
            sText = sText.Replace("\r\n", "").Replace(" ","");
            if (!(sText.Length > 0))
            {
                txtLabelProdID.Text = "";
                cboLabelCode.ItemsSource = null;
                cboLabelCode.SelectedValue = null;
            }
        }



        private void txtKeypartID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnKeypart_Search_Click(null, null);
            }   
        }


        #endregion

        #region Event - ComboBox

        private void cboListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnSearch_Click(null, null);
        }

        private void cbohistory_ListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnhistory_Search_Click(null, null);
        }

        private void cboselectLot_ListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            btnselectLot_Search_Click(null, null);
        }


        private void cboKeypart_ListCount_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (txtKeypartID.Text.Length > 3)
            {
                btnKeypart_Search_Click(null, null);
            }
        }

        private void cbohistory_DateConfig_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (dgLotHistoryList.Columns["ACTDTTM"] != null)
            {
                dgLotHistoryList.Columns["ACTDTTM"].Header = cbohistory_DateConfig.Text;
            }

            //if (Util.NVC(cbohistory_DateConfig.SelectedValue) == "START_LOT")
            //{
            //    dgLotList.Columns["ACTDTTM"].Header = ObjectDic.Instance.GetObjectName("투입일시");
            //}
            //else
            //{
            //    dgLotList.Columns["ACTDTTM"].Header = ObjectDic.Instance.GetObjectName("완료일시");
            //}

        }
        #endregion

        #region Event - DataGrid

        private void dgLotList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //발행중 return;
                if (!blPrintStop) return;

                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CHK")
                    {


                        string sCHK = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[cell.Row.Index].DataItem, "CHK")) == "1" ? "True" : "False";

                        string sLOTID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[cell.Row.Index].DataItem, "LOTID"));
                        string sPRODID = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[cell.Row.Index].DataItem, "PRODID"));
                        string sPRDT_CLSS_CODE = Util.NVC(DataTableConverter.GetValue(dgLotList.Rows[cell.Row.Index].DataItem, "PRDT_CLSS_CODE"));


                        if (sPRDT_CLSS_CODE == "CELL")
                        {
                            //Util.AlertInfo("CELL제품은라벨프린트할수없습니다.");
                            ms.AlertWarning("SFU3331"); //선택오류 : CELL 제품은 라벨 프린트 할수 없습니다.[선택 LOT 확인]
                            //DataTableConverter.SetValue(dgLotList.Rows[cell.Row.Index].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgLotList.Rows[cell.Row.Index].DataItem, "CHK", 0);
                            dgLotList.Refresh();
                            return;
                        }

                        if (sPRODID != txtLabelProdID.Text && txtLabelProdID.Text.Length > 0)
                        {
                            //Util.AlertInfo("같은 제품만 프린트할 수 있습니다.");
                            ms.AlertWarning("SFU3396"); //같은 제품만 프린트할 수 있습니다.
                            //DataTableConverter.SetValue(dgLotList.Rows[cell.Row.Index].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgLotList.Rows[cell.Row.Index].DataItem, "CHK", 0);
                            dgLotList.Refresh();
                            return;
                        }

                        TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);


                        string sLot = checkLot_Move_PrintBox(cell.Row, textRange.Text);


                        if (sCHK == "True")
                        {
                            txtNote_PrintIDList.AppendText(sLot);
                        }
                        else
                        {
                            textRange.Text = textRange.Text.Replace(sLot, "");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgLotList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDC143C"));
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

        private void dgLotList_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            //this.Dispatcher.BeginInvoke(new Action(() =>
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;
            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(e.Column.Name) == false)
                {
                    if (e.Column.Name.Equals("CHK"))
                    {
                        pre.Content = chkAll;
                        e.Column.HeaderPresenter.Content = pre;
                        chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                        chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                    }
                }

            }
            ));
        }

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                //발행중 return;
                if (!blPrintStop) return;

                string sLotList = string.Empty;
                for (int idx = 0; idx < dgLotList.Rows.Count; idx++)
                {
                    C1.WPF.DataGrid.DataGridRow row = dgLotList.Rows[idx];

                    string sPRODID = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                    if (sPRODID != txtLabelProdID.Text && txtLabelProdID.Text.Length > 0)
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                    }
                    else
                    {
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);

                        sLotList += checkLot_Move_PrintBox(row, sLotList);
                    }
                }
                txtNote_PrintIDList.AppendText(sLotList.Replace("\r\n\r\n", "\r\n"));
            }
            catch(Exception ex)
            {
                Util.NVC(ex.ToString());
            }
        }

        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                //발행중 return;
                if (!blPrintStop) return;

                string sLotList = string.Empty;
                for (int idx = 0; idx < dgLotList.Rows.Count; idx++)
                {

                    C1.WPF.DataGrid.DataGridRow row = dgLotList.Rows[idx];
                    DataTableConverter.SetValue(row.DataItem, "CHK", false);

                    sLotList += checkLot_Move_PrintBox(row, sLotList);
                }
                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                string sTextRange = textRange.Text;
                string[] sDelLotList = System.Text.RegularExpressions.Regex.Split(sLotList.Trim(), @"\r\n");
                for (int i = 0; i < sDelLotList.Length; i++)
                {
                    sTextRange = sTextRange.Replace(sDelLotList[i] + "\r\n", "");
                }

                textRange.Text = sTextRange;
            }
            catch (Exception ex)
            {
                Util.NVC(ex.ToString());
            }
            

        }
        
        private void dgLotHistoryList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgLotHistoryList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgLotHistoryList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDC143C"));
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
        #endregion



        private void dgselectLotLotList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgselectLotLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "LOTID")
                    {
                        this.FrameOperation.OpenMenu("SFU010090090", true, cell.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void dgselectLotLotList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
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
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFDC143C"));
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
        #endregion


        private void bkWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {

                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);

                printView_Change();

                labelPrint(textRange);
                
            }
            catch (Exception ex)
            {
                blPrintStop = true;
                bkWorker.CancelAsync();

                Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                    //Util.AlertInfo(ex.Message + "\r\n" + ex.StackTrace);
                    Util.MessageException(ex);
                }));
            }

        }

        private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            blPrintStop = true;
            btnLabel.Content = ObjectDic.Instance.GetObjectName("출력");
            txtNote_PrintIDList.IsEnabled = true;
            cboLabelCode.IsEnabled = true;
            btnPrintList_Clear.IsEnabled = true;
            //btnLabel_view.IsEnabled = true;
            cboLabelDPI.IsEnabled = true;
            txtPrintIDInput.IsEnabled = true;
        }


        #region Mehod

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                String[] sFilter_LabelDPI = { "PRINTER_RESOLUTION" };
                _combo.SetCombo(cboLabelDPI, CommonCombo.ComboStatus.NONE, sFilter: sFilter_LabelDPI, sCase: "COMMCODE");
                //

                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID ,Area_Type.PACK};
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment , cboProductModel };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProcess, cboProductModel };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);

                //공정
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment, cboAreaByAreaType };
                C1ComboBox[] cbProcessChild = { cboProduct };
                string strCase = "cboProcessPack_Area";
                _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent ,cbChild: cbProcessChild, sCase:strCase);

                //모델
                C1ComboBox[] cboProductModelChild = { cboPrdtClass, cboProduct };
                C1ComboBox[] cboProductModelParent = { cboAreaByAreaType, cboEquipmentSegment};
                _combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent,sCase: "PRJ_MODEL");


                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboAREA_TYPE_CODE = new C1ComboBox();
                cboAREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cboPRODID = new C1ComboBox();
                cboPRODID.SelectedValue = null;
                ////모델
                //C1ComboBox[] cboProductModelChild = { cboPrdtClass, cboProduct };
                //C1ComboBox[] cboProductModelParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE, cboPRODID };
                //_combo.SetCombo(cboProductModel, CommonCombo.ComboStatus.ALL, cbChild: cboProductModelChild, cbParent: cboProductModelParent);


                //제품 CLASS TYPE : CELL CMA BMA
                C1ComboBox[] ccboPrdtClassChild = { cboProduct };
                C1ComboBox[] cboPrdtClassParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboAREA_TYPE_CODE };
                _combo.SetCombo(cboPrdtClass, CommonCombo.ComboStatus.ALL, cbChild: ccboPrdtClassChild, cbParent: cboPrdtClassParent);

                //제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment, cboProcess, cboProductModel, cboAREA_TYPE_CODE, cboPrdtClass };
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent);

                //작업조
                C1ComboBox cboTempShop = new C1ComboBox();
                cboTempShop.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cboTempEqsg = new C1ComboBox();
                cboTempEqsg.SelectedValue = "";
                C1ComboBox[] cboShiftParent = { cboTempShop, cboAreaByAreaType , cboTempEqsg };
                _combo.SetCombo(cboShift, CommonCombo.ComboStatus.ALL, cbParent: cboShiftParent, sCase: "SHIFT_AREA");

                this.cboListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
                this.cboListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboListCount_SelectedValueChanged);

                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
                Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

                SetcboWipState();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBox_History()
        {
            try
            {


                CommonCombo _combo = new CommonCombo();
                
                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cbohistory_EquipmentSegment, cbohistory_ProductModel };
                _combo.SetCombo(cbohistory_AreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sFilter: sFilter,sCase: "cboAreaByAreaType");

                //라인
                C1ComboBox[] cboLineParent = { cbohistory_AreaByAreaType };
                C1ComboBox[] cboLineChild = { cbohistory_Process, cbohistory_ProductModel };
                _combo.SetCombo(cbohistory_EquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent, sCase: "cboEquipmentSegment");

                //공정
                C1ComboBox[] cbProcessParent = { cbohistory_EquipmentSegment , cbohistory_AreaByAreaType };
                C1ComboBox[] cbProcessChild = { cbohistory_Product };
                string strCase = "cboProcessPack_Area";
                _combo.SetCombo(cbohistory_Process, CommonCombo.ComboStatus.SELECT, cbParent: cbProcessParent, cbChild: cbProcessChild, sCase:strCase);

                //모델
                C1ComboBox[] cbohistory_ProductModelChild = { cbohistory_PrdtClass, cbohistory_Product };
                C1ComboBox[] cbohistory_ProductModelParent = { cbohistory_AreaByAreaType, cbohistory_EquipmentSegment };
                _combo.SetCombo(cbohistory_ProductModel, CommonCombo.ComboStatus.ALL, cbChild: cbohistory_ProductModelChild, cbParent: cbohistory_ProductModelParent, sCase: "PRJ_MODEL");


                C1ComboBox cbohistory_SHOPID = new C1ComboBox();
                cbohistory_SHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cbohistory_AREA_TYPE_CODE = new C1ComboBox();
                cbohistory_AREA_TYPE_CODE.SelectedValue = "P";
                C1ComboBox cbohistory_PRODID = new C1ComboBox();
                cbohistory_PRODID.SelectedValue = null;
                ////모델
                //C1ComboBox[] cbohistory_ProductModelChild = { cbohistory_PrdtClass, cbohistory_Product };
                //C1ComboBox[] cbohistory_ProductModelParent = { cbohistory_SHOPID, cbohistory_AreaByAreaType, cbohistory_EquipmentSegment, cbohistory_AREA_TYPE_CODE, cbohistory_PRODID };
                //_combo.SetCombo(cbohistory_ProductModel, CommonCombo.ComboStatus.ALL, cbChild: cbohistory_ProductModelChild, cbParent: cbohistory_ProductModelParent);


                //제품 CLASS TYPE : CELL CMA BMA
                C1ComboBox[] ccbohistory_PrdtClassChild = { cbohistory_Product };
                C1ComboBox[] cbohistory_PrdtClassParent = { cbohistory_SHOPID, cbohistory_AreaByAreaType, cbohistory_EquipmentSegment, cbohistory_AREA_TYPE_CODE };
                _combo.SetCombo(cbohistory_PrdtClass, CommonCombo.ComboStatus.ALL, cbChild: ccbohistory_PrdtClassChild, cbParent: cbohistory_PrdtClassParent, sCase: "cboPrdtClass");

                //제품
                C1ComboBox[] cbohistory_ProductParent = { cbohistory_SHOPID, cbohistory_AreaByAreaType, cbohistory_EquipmentSegment, cbohistory_Process, cbohistory_ProductModel, cbohistory_AREA_TYPE_CODE, cbohistory_PrdtClass };
                _combo.SetCombo(cbohistory_Product, CommonCombo.ComboStatus.ALL, cbParent: cbohistory_ProductParent, sCase: "cboProduct");

                //작업조
                C1ComboBox cbohistory_TempShop = new C1ComboBox();
                cbohistory_TempShop.SelectedValue = LoginInfo.CFG_SHOP_ID;
                C1ComboBox cbohistory_TempEqsg = new C1ComboBox();
                cbohistory_TempEqsg.SelectedValue = "";
                C1ComboBox[] cbohistory_ShiftParent = { cbohistory_TempShop, cbohistory_AreaByAreaType, cbohistory_TempEqsg };
                _combo.SetCombo(cbohistory_Shift, CommonCombo.ComboStatus.ALL, cbParent: cbohistory_ShiftParent, sCase: "SHIFT_AREA");

                this.cbohistory_ListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cbohistory_ListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cbohistory_ListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
                this.cbohistory_ListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cbohistory_ListCount_SelectedValueChanged);
                                
                dtphistory_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtphistory_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;

                Util.Set_Pack_cboTimeList(cbohistory_TimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
                Util.Set_Pack_cboTimeList(cbohistory_TimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

                dateSearchType_Cbo_history();
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setComboBxo_Keypart()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //Keypart 라인
                String[] sKeypartLineFilter = { LoginInfo.CFG_AREA_ID };
                _combo.SetCombo(cboEquipmentSegmentKerpart, CommonCombo.ComboStatus.NONE, sFilter: sKeypartLineFilter, sCase: "cboLine");

                this.cboKeypart_ListCount.SelectedValueChanged -= new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboKeypart_ListCount_SelectedValueChanged);
                Util.Set_Pack_cboListCoount(cboKeypart_ListCount, "CBO_NAME", "CBO_CODE", 1000, 10000, 1000);
                this.cboKeypart_ListCount.SelectedValueChanged += new System.EventHandler<C1.WPF.PropertyChangedEventArgs<object>>(this.cboKeypart_ListCount_SelectedValueChanged);

                dtpKeypart_DateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7);
                dtpKeypart_DateTo.SelectedDateTime = (DateTime)System.DateTime.Now;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SetcboWipState()
        {
            try
            {
                string sSelectedValue = cboWipState.SelectedItemsToString;
                string[] sSelectedList = sSelectedValue.Split(',');

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "PACK_WIPSTAT_LOTSEARCH";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboWipState.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    if (sSelectedList.Length > 0 && sSelectedList[0] != "")
                    {
                        for (int j = 0; j < sSelectedList.Length; j++)
                        {
                            if (Util.NVC(dtResult.Rows[i]["CBO_CODE"]) == sSelectedList[j])
                            {
                                cboWipState.Check(i);
                                break;
                            }
                        }
                    }
                    else
                    {
                        cboWipState.Check(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setLabelCode(string sProdID)
        {
            try
            {
                if (sProdID.Length > 0)
                {
                    CommonCombo _combo = new CommonCombo();
                    //라벨 세팅
                    String[] sFilter = { sProdID, null, null, LABEL_TYPE_CODE.PACK };
                    _combo.SetCombo(cboLabelCode, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "LABELCODE_BY_PROD");
                }
                else
                {
                    cboLabelCode.ItemsSource = null;
                    cboLabelCode.SelectedValue = null;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private void dateSearchType_Cbo()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("생성일시"), "LOTDTTM_CR" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("변경일시"), "WIPSDTTM" };
            dtResult.Rows.Add(newRow);

            cboDateConfig.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        private void dateSearchType_Cbo_history()
        {
            DataTable dtResult = new DataTable();
            dtResult.Columns.Add("KEY", typeof(string));
            dtResult.Columns.Add("VALUE", typeof(string));

            DataRow newRow = dtResult.NewRow();

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("투입일시"), "START_LOT" };
            dtResult.Rows.Add(newRow);

            newRow = dtResult.NewRow();
            newRow.ItemArray = new object[] { ObjectDic.Instance.GetObjectName("완료일시"), "END_LOT" };
            dtResult.Rows.Add(newRow);

            cbohistory_DateConfig.ItemsSource = DataTableConverter.Convert(dtResult);
        }

        //2018.10.17
        private bool ValidationWindingLotSearch()
        {
            TimeSpan timeSpan = dtpDateTo.SelectedDateTime.Date - dtpDateFrom.SelectedDateTime.Date;
            if (timeSpan.Days < 0)
            {
                //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                Util.MessageValidation("SFU3569");
                return false;
            }

            if (timeSpan.Days > 30)
            {
                Util.MessageValidation("SFU4466");
                return false;
            }

            return true;
        }

        private void labelPrint(TextRange textRange)
        {
            try
            {
                bool bChk = false;
                string sLabelProdId = string.Empty;
                string sLabelCode = string.Empty;
                string[] sLot_List = System.Text.RegularExpressions.Regex.Split(textRange.Text.Trim(), @"\r\n");
                txtLabelProdID.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    sLabelProdId = txtLabelProdID.Text;
                });
                cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
                {
                    sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                });

                for (int i = 0; sLot_List.Length > i; i++)
                {
                    if (blPrintStop) break;

                    if (sLot_List[i].Length > 0)
                    {
                        string sLotID = sLot_List[i];


                        DataTable RQSTDT = new DataTable();
                        RQSTDT.TableName = "RQSTDT";
                        RQSTDT.Columns.Add("LANGID", typeof(string));
                        RQSTDT.Columns.Add("LOTID", typeof(string));

                        

                        DataRow dr = RQSTDT.NewRow();
                        dr["LANGID"] = LoginInfo.LANGID;
                        dr["LOTID"] = sLotID;
                        RQSTDT.Rows.Add(dr);
                        DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_LABELCODE", "RQSTDT", "RSLTDT", RQSTDT);
                        
                        if (dtResult != null)
                        {
                            if (dtResult.Rows.Count > 0)
                            {
                                
                                txtLabelProdID.Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    sLabelProdId = txtLabelProdID.Text;
                                });
                                cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
                                {
                                    sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                                });

                                if (sLabelProdId.Length > 0)
                                {
                                    if (Util.NVC(dtResult.Rows[0]["PRODID"]) == sLabelProdId)
                                    {
                                        bChk = true;
                                    }
                                    else
                                    {
                                        DataRow[] drLABEL_CODE = dtResult.Select("LABEL_CODE='" + sLabelCode + "'");

                                        if (drLABEL_CODE.Length > 0)
                                        {
                                            bChk = true;
                                        }
                                    }
                                }
                                else
                                {
                                    txtLabelProdID.Dispatcher.BeginInvoke((Action)delegate ()
                                    {
                                        txtLabelProdID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                                    });
                                    cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
                                    {
                                        setLabelCode(Util.NVC(dtResult.Rows[0]["PRODID"]));
                                    });
                                    cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
                                    {
                                        sLabelCode = Util.NVC(cboLabelCode.SelectedValue);
                                    });

                                    bChk = true;
                                }

                                
                            }
                        }
                        if (bChk)
                        {
                            if (!(sLabelCode.Length > 0))
                            {
                                continue;
                            }
                            //Util.printLabel_Pack(FrameOperation, loadingIndicator, sLotID, LABEL_TYPE_CODE.PACK, "N", "1", null);
                            Util.printLabel_Pack(FrameOperation, loadingIndicator, sLotID, LABEL_TYPE_CODE.PACK, sLabelCode, "N", "1", sLabelProdId);

                            // 2018.06.29
                            System.Threading.Thread.Sleep(800);
                            //2018.06.29

                            txtNote_PrintIDList.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                textRange.Text = textRange.Text.Trim().Replace(sLotID + "\r\n", "");
                                textRange.Text = textRange.Text.Trim().Replace(sLotID, "");
                            });
                            dgLotList.Dispatcher.BeginInvoke((Action)delegate ()
                            {
                                int iChk = Util.gridFindDataRow(ref dgLotList, "LOTID", sLotID, false);
                                if (iChk > 0)
                                {
                                    DataTableConverter.SetValue(dgLotList.Rows[iChk].DataItem, "CHK", false);
                                    dgLotList.Refresh();
                                }
                            });

                        }
                        //2018.06.29
                        //System.Threading.Thread.Sleep(800);
                        // 2018.06.29
                    }
                }
                if (!blPrintStop)
                {
                    txtLabelProdID.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        txtLabelProdID.Text = "";
                    });
                    cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
                    {
                        cboLabelCode.ItemsSource = null;
                        cboLabelCode.SelectedValue = null;
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void printView_Change()
        {
            btnLabel.Dispatcher.BeginInvoke((Action)delegate ()
            {
                btnLabel.Content = ObjectDic.Instance.GetObjectName("취소");
            });
            txtNote_PrintIDList.Dispatcher.BeginInvoke((Action)delegate ()
            {
                txtNote_PrintIDList.IsEnabled = false;
            });
            cboLabelCode.Dispatcher.BeginInvoke((Action)delegate ()
            {
                cboLabelCode.IsEnabled = false;
            });
            btnPrintList_Clear.Dispatcher.BeginInvoke((Action)delegate ()
            {
                btnPrintList_Clear.IsEnabled = false;
            });
            //btnLabel_view.Dispatcher.BeginInvoke((Action)delegate ()
            //{
            //    btnLabel_view.IsEnabled = false;
            //});
            cboLabelDPI.Dispatcher.BeginInvoke((Action)delegate ()
            {
                cboLabelDPI.IsEnabled = false;
            });
            txtPrintIDInput.Dispatcher.BeginInvoke((Action)delegate ()
            {
                txtPrintIDInput.IsEnabled = false;
            });
        }

        private string checkLot_Move_PrintBox(C1.WPF.DataGrid.DataGridRow row , string sLotList)
        {
            string sTemp = string.Empty;
            try
            {
                

                string sCHK = Util.NVC(DataTableConverter.GetValue(row.DataItem, "CHK")) == "1" ? "True" : "False";

                string sLOTID = Util.NVC(DataTableConverter.GetValue(row.DataItem, "LOTID"));
                string sPRODID = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRODID"));
                string sPRDT_CLSS_CODE = Util.NVC(DataTableConverter.GetValue(row.DataItem, "PRDT_CLSS_CODE"));

                

                TextRange textRange = new TextRange(txtNote_PrintIDList.Document.ContentStart, txtNote_PrintIDList.Document.ContentEnd);
                string sListBox_LotList = textRange.Text.Replace("\r\n", "").Trim();

                if (sCHK == "True" && !textRange.Text.Contains(sLOTID))
                {
                    if (!(txtLabelProdID.Text.Length > 0))
                    {
                        txtLabelProdID.Text = sPRODID;
                        //라벨코드셋팅
                        setLabelCode(sPRODID);


                        if ((sLotList.Length > 0) || (sListBox_LotList.Length > 0))
                        {
                            sTemp= "\r\n" + sLOTID;
                            //txtNote_PrintIDList.AppendText("\r\n" + sLOTID);
                        }
                        else
                        {
                            sTemp = sLOTID + "\r\n";
                            //txtNote_PrintIDList.AppendText(sLOTID);
                        }
                    }
                    else if (txtLabelProdID.Text == sPRODID)
                    {
                        if ((sLotList.Length > 0) || (sListBox_LotList.Length > 0))
                        {
                            sTemp = "\r\n" + sLOTID;
                            //txtNote_PrintIDList.AppendText("\r\n" + sLOTID);
                        }
                        else
                        {
                            sTemp = sLOTID + "\r\n";
                            //txtNote_PrintIDList.AppendText(sLOTID);
                        }
                    }
                }
                else if(bool.Parse(sCHK) == false)
                {
                    sTemp = sLOTID + "\r\n";
                    //textRange.Text = textRange.Text.Replace(sLOTID + "\r\n", "");
                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
            return sTemp;
        }

        private bool chkLot_LabelCode(string sLotid)
        {
            bool bReturn = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = sLotid;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_LABELCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null)
                {
                    if(dtResult.Rows.Count > 0)
                    {
                        if (txtLabelProdID.Text.Length > 0)
                        {
                            if(Util.NVC(dtResult.Rows[0]["PRODID"]) == txtLabelProdID.Text)
                            {
                                bReturn = true;
                            }
                            else
                            {
                                DataRow[] drLABEL_CODE = dtResult.Select("LABEL_CODE='" + Util.NVC(cboLabelCode.SelectedValue) + "'");

                                if (drLABEL_CODE.Length > 0)
                                {
                                    bReturn = true;
                                }
                            }
                        }
                        else
                        {
                            txtLabelProdID.Text = Util.NVC(dtResult.Rows[0]["PRODID"]);
                            setLabelCode(Util.NVC(dtResult.Rows[0]["PRODID"]));
                            bReturn = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }

            return bReturn;
        }

        private void getLotList(string sId_List)
        {
            try
            {
                //DA_PRD_SEL_WIP

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_FROM", typeof(string));
                RQSTDT.Columns.Add("LOTDTTM_CR_TO", typeof(string));
                RQSTDT.Columns.Add("WIPSDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("WIPSDTTM_TO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("ID_LIST", typeof(string));
                RQSTDT.Columns.Add("WIPSTAT", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (sId_List == null)
                {
                    string sSeachDateType = Util.NVC(cboDateConfig.SelectedValue);

                    dr["LOTDTTM_CR_FROM"] = sSeachDateType == "LOTDTTM_CR" ? dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString() : null;
                    dr["LOTDTTM_CR_TO"] = sSeachDateType == "LOTDTTM_CR" ? dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString() : null;
                    dr["WIPSDTTM_FROM"] = sSeachDateType == "WIPSDTTM" ? dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString() : null;
                    dr["WIPSDTTM_TO"] = sSeachDateType == "WIPSDTTM" ? dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString() : null;
                    dr["AREAID"] = cboAreaByAreaType.SelectedValue;
                    dr["EQSGID"] = cboEquipmentSegment.SelectedValue;
                    dr["PROCID"] = Util.NVC(cboProcess.SelectedValue) == "" ? null : cboProcess.SelectedValue;
                    dr["MODLID"] = Util.NVC(cboProductModel.SelectedValue) == "" ? null : cboProductModel.SelectedValue;
                    dr["PRDT_CLSS_CODE"] = Util.NVC(cboPrdtClass.SelectedValue) == "" ? null : cboPrdtClass.SelectedValue;
                    dr["PRODID"] = Util.NVC(cboProduct.SelectedValue) == "" ? null : cboProduct.SelectedValue;
                    dr["SHIFT"] = Util.NVC(cboShift.SelectedValue) == "" ? null : cboShift.SelectedValue;
                    dr["COUNT"] = cboListCount.SelectedValue;
                    dr["ID_LIST"] = sId_List;
                    dr["WIPSTAT"] = Util.NVC(cboWipState.SelectedItemsToString) == "" ? null : cboWipState.SelectedItemsToString;
                }
                else
                {
                    dr["LOTDTTM_CR_FROM"] = null;
                    dr["LOTDTTM_CR_TO"] = null;
                    dr["AREAID"] = null;
                    dr["EQSGID"] = null;
                    dr["PROCID"] = null;
                    dr["MODLID"] = null;
                    dr["PRDT_CLSS_CODE"] = null;
                    dr["PRODID"] = null;
                    dr["SHIFT"] = null;
                    //2018.10.17 
                    //dr["COUNT"] = "9223372036854775807";
                    dr["COUNT"] = iCnt;
                    dr["ID_LIST"] = sId_List;
                    dr["WIPSTAT"] = null;
                }

                RQSTDT.Rows.Add(dr);

                //DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_PACK", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_LOT_PACK", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgLotList, dtResult, FrameOperation, true);
                    //dgLotList.ItemsSource = DataTableConverter.Convert(dtResult);
                    Util.SetTextBlockText_DataGridRowCount(tbLotListCount, Util.NVC(dgLotList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void getLotList_Act()
        {
            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("ACTID", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("ACTDTTM_TO", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("MODLID", typeof(string));
                RQSTDT.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("SHIFT", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["ACTID"] = Util.NVC(cbohistory_DateConfig.SelectedValue);
                dr["ACTDTTM_FROM"] = dtphistory_DateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cbohistory_TimeFrom.SelectedValue.ToString();
                dr["ACTDTTM_TO"] = dtphistory_DateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cbohistory_TimeTo.SelectedValue.ToString();
                dr["AREAID"] = cbohistory_AreaByAreaType.SelectedValue;
                dr["EQSGID"] = cbohistory_EquipmentSegment.SelectedValue;
                dr["PROCID"] = cbohistory_Process.SelectedValue;
                dr["MODLID"] = Util.NVC(cbohistory_ProductModel.SelectedValue) == "" ? null : cbohistory_ProductModel.SelectedValue;
                dr["PRDT_CLSS_CODE"] = Util.NVC(cbohistory_PrdtClass.SelectedValue) == "" ? null : cbohistory_PrdtClass.SelectedValue;
                dr["PRODID"] = Util.NVC(cbohistory_Product.SelectedValue) == "" ? null : cbohistory_Product.SelectedValue;
                dr["SHIFT"] = Util.NVC(cbohistory_Shift.SelectedValue) == "" ? null : cbohistory_Shift.SelectedValue;
                dr["COUNT"] = cbohistory_ListCount.SelectedValue;

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_LOT_PACK_ACT", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_LOT_PACK_ACT", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    if(dtResult.Rows.Count > 0)
                    {
                        txtHistory_countName.Text = Util.NVC(cbohistory_DateConfig.SelectedValue) == "START_LOT" ? ObjectDic.Instance.GetObjectName("투입수량") : ObjectDic.Instance.GetObjectName("완료수량");
                        txtHistory_SearchEqsg.Text = Util.NVC(dtResult.Rows[0]["EQSGNAME"]);
                        txtHistory_SearchProc.Text = Util.NVC(dtResult.Rows[0]["PROCNAME"]);
                        txtHistory_SearchCount.Text = (Util.NVC_Int(dtResult.Rows[0]["TOTALCOUNT"])).ToString("##,#");
                    }else
                    {
                        txtHistory_countName.Text = Util.NVC(cbohistory_DateConfig.SelectedValue) == "START_LOT" ? ObjectDic.Instance.GetObjectName("투입수량") : ObjectDic.Instance.GetObjectName("완료수량");
                        txtHistory_SearchEqsg.Text = "";
                        txtHistory_SearchProc.Text = "";
                        txtHistory_SearchCount.Text = "";
                    }
                    Util.GridSetData(dgLotHistoryList, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbhistory_LotListCount, Util.NVC(dgLotHistoryList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void txtLotId_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.V && Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    string pasteString = Clipboard.GetText();
                    if (pasteString.Contains("\r\n"))
                    {
                        string commaString = pasteString.Replace("\r\n", ",");
                        txtLotID.Text = txtLotID.Text + "," + commaString;
                        Clipboard.Clear();
                        txtLotID.Select(txtLotID.Text.Length, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void getSelectLotList_Act()
        {
            try
            {
                string[] totalInputArray;
                if (txtLotID.Text.Trim().Equals(""))
                {
                    Util.MessageInfo("SFU1190"); //조회할 LOTID를 입력하세요.
                    return;
                }
                else totalInputArray = txtLotID.Text.Split(',');

                ArrayList list = new ArrayList();

                for (int i = 0; i < totalInputArray.Length; i++)
                {
                    string s = totalInputArray[i];
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        continue;
                    }
                    list.Add(s.Trim());
                }

                totalInputArray = (string[])list.ToArray(typeof(string));

                int totalInputCnt = totalInputArray.Length - 1;

                // 한 DA에 들어갈 입력 수
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "LIMITED_QTY_PACK";
                dr["CBO_CODE"] = "LIMIT_MULTILOT_QTY";

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_ATTRIBUTE", "INDATA", "OUTDATA", RQSTDT);

                int searchUnit = int.Parse(Util.NVC(dtResult.Rows[0]["ATTRIBUTE1"]));

                if (totalInputCnt >= (searchUnit * 3))
                {
                    Util.MessageInfo("FM_ME_0458", int.Parse(Util.NVC(dtResult.Rows[0]["ATTRIBUTE2"]))); // %1[30,000]개 까지 조회 가능합니다.
                    return;
                }

                if (totalInputCnt >= searchUnit) {
                    ms.AlertInfo("FM_ME_0375", (totalInputCnt + 1).ToString(), (totalInputCnt/searchUnit).ToString()); // 총 %1[입력건수]건, 예상소요시간 %2[DA당 1분 표기]분 이상
                }
                loadingIndicator.Visibility = Visibility.Visible;

                string[] inputs = new string[totalInputCnt / searchUnit + 1];

                for (int i = 0; i <= totalInputCnt / searchUnit; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    if (i == 0)
                    {
                        if (i == totalInputCnt / searchUnit)
                        {
                            sb.Append(String.Join(",", totalInputArray));
                        }
                        else
                        {
                            string[] newInput = new string[searchUnit];
                            Array.Copy(totalInputArray, 0, newInput, 0, searchUnit);
                            sb.Append(String.Join(",", newInput));
                        }
                    }
                    else if (i == totalInputCnt / searchUnit)
                    {
                        string[] newInput = new string[searchUnit];
                        Array.Copy(totalInputArray, i * searchUnit, newInput, 0, totalInputCnt + 1 - (i * searchUnit));
                        sb.Append(String.Join(",", newInput));
                    }
                    else
                    {
                        string[] newInput = new string[searchUnit];
                        Array.Copy(totalInputArray, i * searchUnit, newInput, 0, searchUnit);
                        sb.Append(String.Join(",", newInput));
                    }
                    inputs[i] = sb.ToString();
                }

                Action<DataTable[]> callback = (allResult) =>
                {
                    DataTable mergedTable = allResult[0];
                    for (int i = 1; i < allResult.Length; i++)
                    {
                        mergedTable.Merge(allResult[i]);
                    }

                    Application.Current.Dispatcher.Invoke(
                        new Action(delegate
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.GridSetData(dgselectLotLotList, mergedTable, FrameOperation, true);
                            Util.SetTextBlockText_DataGridRowCount(tbselectLot_LotListCount, Util.NVC(dgselectLotLotList.Rows.Count));
                        }),
                        System.Windows.Threading.DispatcherPriority.Input
                    );
                };

                Task task = Task.Run(() => getSelectLotList_Callback(inputs, callback));
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        private async Task getSelectLotList_Callback(string[] inputs, Action<DataTable[]> callback)
        {
            DataTable[] allResult = new DataTable[inputs.Length];

            try
            {
                Task[] tasks = new Task[inputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                {
                    int taskNum = i;
                    string input = inputs[i];
                    string langid = LoginInfo.LANGID;

                    tasks[i] = Task.Run(() =>
                    {
                        try
                        {
                            DataTable RQSTDT = new DataTable();
                            RQSTDT.TableName = "RQSTDT";
                            RQSTDT.Columns.Add("LANGID", typeof(string));
                            RQSTDT.Columns.Add("LOTID", typeof(string));

                            DataRow dr = RQSTDT.NewRow();
                            dr["LANGID"] = langid;
                            dr["LOTID"] = input;

                            RQSTDT.Rows.Add(dr);

                            allResult[taskNum] = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOT_INFO_PACK_MULTILOT", "RQSTDT", "RSLTDT", RQSTDT);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    });
                }
                
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    Application.Current.Dispatcher.Invoke(
                        new Action(delegate
                        {
                            loadingIndicator.Visibility = Visibility.Collapsed;
                            Util.MessageException(ex);

                        }),
                        System.Windows.Threading.DispatcherPriority.Input
                    );
                }

                return;
            }

            callback(allResult);
        }

        private void getKeypart_Tracking()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("COUNT", typeof(Int64));
                RQSTDT.Columns.Add("DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("DTTM_TO", typeof(string));
                RQSTDT.Columns.Add("KEYPARTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["COUNT"] = cboKeypart_ListCount.SelectedValue;
                dr["DTTM_FROM"] = dtpKeypart_DateFrom.SelectedDateTime.ToString("yyyyMMdd");
                dr["DTTM_TO"] = dtpKeypart_DateTo.SelectedDateTime.ToString("yyyyMMdd");
                dr["KEYPARTID"] = txtKeypartID.Text;
                // 2020.04.03 염규범S
                // Keypart 시리얼 번호로 검색 가능하게 처리
                dr["EQSGID"] = cboEquipmentSegmentKerpart.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_KEYPART_TRACKING", "RQSTDT", "RSLTDT", RQSTDT, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("DA_PRD_SEL_KEYPART_TRACKING", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgLotKeypartList, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbKeypart_LotListCount, Util.NVC(dgLotKeypartList.Rows.Count));
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private bool ValidationTime()
        {
            TimeSpan timeSpan = dtphistory_DateTo.SelectedDateTime.Date - dtphistory_DateFrom.SelectedDateTime.Date;

            if (!LoginInfo.CFG_SHOP_ID.Equals("A040"))
            {
                if (timeSpan.Days < 0)
                {
                    dtphistory_DateFrom.SelectedDateTime = dtphistory_DateTo.SelectedDateTime.Date.AddDays(-7);
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }
            
                if (timeSpan.Days > 7  )
                {
                    dtphistory_DateTo.SelectedDateTime = dtphistory_DateFrom.SelectedDateTime.Date.AddDays(+7);
                    Util.MessageValidation("SFU3567");
                    return false;
                }
            }
            else
            {
                if (timeSpan.Days < 0)
                {
                    dtphistory_DateFrom.SelectedDateTime = dtphistory_DateTo.SelectedDateTime.Date.AddDays(-30);
                    //조회 시작일자는 종료일자를 초과 할 수 없습니다.
                    Util.MessageValidation("SFU3569");
                    return false;
                }
                if (timeSpan.Days > 30)
                {
                    dtphistory_DateTo.SelectedDateTime = dtphistory_DateFrom.SelectedDateTime.Date.AddDays(+30);
                    Util.MessageValidation("SFU4466");
                    return false;
                }
            }


            return true;
        }

        #endregion

        private void dtphistory_DateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (!ValidationTime())
                //{
                //   dtphistory_DateFrom.SelectedDateTime = dtphistory_DateTo.FirstDate.Date.AddDays(-7);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dtphistory_DateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                //if (!ValidationTime())
                //{
                //    dtphistory_DateTo.SelectedDateTime = dtphistory_DateFrom.FirstDate.Date.AddDays(+7);
                    
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void dgselectLotLotList_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgselectLotLotList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    if (cell.Column.Name == "CHK")
                    {
                        string sCHK = Util.NVC(DataTableConverter.GetValue(dgselectLotLotList.Rows[cell.Row.Index].DataItem, "CHK"));
                        string sLOTID = Util.NVC(DataTableConverter.GetValue(dgselectLotLotList.Rows[cell.Row.Index].DataItem, "LOTID"));
                        string sPRODID = Util.NVC(DataTableConverter.GetValue(dgselectLotLotList.Rows[cell.Row.Index].DataItem, "PRODID"));
                        string sPRDT_CLSS_CODE = Util.NVC(DataTableConverter.GetValue(dgselectLotLotList.Rows[cell.Row.Index].DataItem, "PRDT_CLSS_CODE"));

                    }
                }
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private DataTable GetExcelData()
        {
            DataTable dtExcelData = null;

            try
            {
                OpenFileDialog fd = new OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";

                string sColData = string.Empty;

                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        dtExcelData = LoadExcelHelper.LoadExcelData(stream, 0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }

            return dtExcelData;
        }
    }
}
