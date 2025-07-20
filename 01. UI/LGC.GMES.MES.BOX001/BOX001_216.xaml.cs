/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리
--------------------------------------------------------------------------------------
 [Change History]


**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001.UserControls;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_216 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _util = new Util();
        /*컨트롤 변수 선언*/
        public UCBoxShift ucBoxShift { get; set; }
        public TextBox txtWorker_Main { get; set; }
        public TextBox txtShift_Main { get; set; }

        public BOX001_216()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitCombo();
            InitControl();
            SetEvent();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { LoginInfo.CFG_SHOP_ID, "ALL" }, sCase: "AREA_NO_AUTH");
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            dtpDateFrom.SelectedDateTime = DateTime.Today;
            dtpDateTo.SelectedDateTime = DateTime.Today;
            
            /* 공용 작업조 컨트롤 초기화 */
            ucBoxShift = grdShift.Children[0] as UCBoxShift;
            txtWorker_Main = ucBoxShift.TextWorker;
            txtShift_Main = ucBoxShift.TextShift;
            ucBoxShift.ProcessCode = Process.CELL_BOXING; //작업조 팝업에 넘길 공정

            if (this.FrameOperation.Parameters != null && this.FrameOperation.Parameters.Length > 0)
            {
                Array ary = FrameOperation.Parameters;
            //    this.FrameOperation.OpenMenu("SFU010060520", true, new object[] { sPalletID, txtShift_Main.Tag, txtShift_Main.Text, txtWorker_Main.Tag, txtWorker_Main.Text, ucBoxShift.TextShiftStartTime, ucBoxShift.TextShiftEndTime });

                txtPalletID.Text= ary.GetValue(0).ToString();
                
                txtShift_Main.Tag = ary.GetValue(1).ToString();
                txtShift_Main.Text = ary.GetValue(2).ToString();
                txtWorker_Main.Tag = ary.GetValue(3).ToString();
                txtWorker_Main.Text = ary.GetValue(4).ToString();
                ucBoxShift.TextShiftDateTime.Text = ary.GetValue(5).ToString();
                txtWorker_Main.IsReadOnly = true;

                Search();
            }
        }        
        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= UserControl_Loaded;
            dtpDateFrom.SelectedDataTimeChanged += dtpDateFrom_SelectedDataTimeChanged;
            dtpDateTo.SelectedDataTimeChanged += dtpDateTo_SelectedDataTimeChanged;
        }
        
        private void dtpDateFrom_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateTo.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateTo.SelectedDateTime;
                return;
            }
        }

        private void dtpDateTo_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            LGCDatePicker dtPik = sender as LGCDatePicker;
            if (Convert.ToDecimal(dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd")) > Convert.ToDecimal(dtPik.SelectedDateTime.ToString("yyyyMMdd")))
            {
                dtPik.SelectedDateTime = dtpDateFrom.SelectedDateTime;
                return;
            }
        }      

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_GET_UNPACK_PALLET_NJ
            Search();           
        }
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void btnSearch_Hist_Click(object sender, RoutedEventArgs e)
        {
            //BR_PRD_GET_UNPACK_PALLET_HIST_NJ
            Search_Hist();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //SFU3440  초기화 하시겠습니까? 
            Util.MessageConfirm("SFU3440", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Clear();
                }             
            });
        }

        private void Clear()
        {
            txtPalletID.Text = string.Empty;
            Util.gridClear(dgPallet);
            Util.gridClear(dgLot);
        }
        #endregion

        private void Search()
        {
            if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
            {
                // SFU1843   작업자를 입력 해 주세요.
                Util.MessageValidation("SFU1843");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPalletID.Text))
            {
                //SFU1411	PALLETID를 입력해주세요
                Util.MessageValidation("SFU1411");
                return;
            }

            DataTable dtDataInfo = DataTableConverter.Convert(dgPallet.ItemsSource);

            // 중복데이터 입력시 미입력
            if (dtDataInfo.Rows.Count > 0 
                && dtDataInfo.Select("BOXID = '" + txtPalletID.Text.Trim() + "'").ToList().Count > 0)
            {
                txtPalletID.Text = string.Empty;
                return;
            }

            DataTable dtLotInfo = DataTableConverter.Convert(dgLot.ItemsSource);
            
            DataSet indataSet = new DataSet();
            DataTable inDataTable = indataSet.Tables.Add("INDATA");
            inDataTable.Columns.Add("LANGID");
            inDataTable.Columns.Add("BOXID");

            DataRow newRow = inDataTable.NewRow();
            newRow["LANGID"] = LoginInfo.LANGID;
            newRow["BOXID"] = txtPalletID.Text.Trim();
            inDataTable.Rows.Add(newRow);

            loadingIndicator.Visibility = Visibility.Visible;
            txtPalletID.Text = string.Empty;

            new ClientProxy().ExecuteService_Multi("BR_PRD_GET_UNPACK_PALLET_NJ", "INDATA", "OUTDATA,OUTLOT", (bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtData = bizResult.Tables["OUTDATA"];
                    DataTable dtLot = bizResult.Tables["OUTLOT"];
                    
                    dtData.Merge(dtDataInfo);
                    dtLot.Merge(dtLotInfo);

                    if (dtData.AsEnumerable().Select(c => c.Field<string>("PRODID")).Distinct().Count() > 1)
                    {
                        //SFU4338	동일한 제품만 작업 가능합니다
                        Util.MessageValidation("SFU4178");
                        return;
                    }

                    if (dtData.AsEnumerable().Select(c => c.Field<string>("AOMM_GRD_CODE")).Distinct().Count() > 1)
                    {
                        //동일한 AOMM 등급을 선택하세요.
                        Util.MessageValidation("SFU3803");
                        return;
                    }

                    /* 2017.01.06 제품만 VAILDATION한다.*/
                    //if (dtLot.AsEnumerable().Select(c => c.Field<string>("PKG_EQSGID")).Distinct().Count() > 1)
                    //{
                    //    //SFU4337   동일한 조립라인만 작업 가능합니다.
                    //    Util.MessageValidation("SFU4337");
                    //    return;
                    //}

                    //if (dtLot.AsEnumerable().Select(c => c.Field<string>("EQPTID")).Distinct().Count() > 1)
                    //{
                    //    Util.MessageValidation("SFU4337");
                    //    return;
                    //}

                    Util.GridSetData(dgPallet, dtData, FrameOperation, true);

                    if (dgPallet.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["BOXID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgPallet.Columns["TOTAL_QTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    }

                    Util.GridSetData(dgLot, dtLot, FrameOperation, true);

                    if (dgLot.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgLot.Columns["PKG_LOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgLot.Columns["LOTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

            }, indataSet);
        }

        private void Search_Hist()
        {
            if (string.IsNullOrWhiteSpace(Util.NVC(cboArea.SelectedValue))
                || Util.NVC(cboArea.SelectedValue) == "SELECT")
            {
                //SFU1499	동을 선택하세요.
                Util.MessageValidation("SFU1499");
                return;
            }

             DataTable inDataTable = new DataTable("INDATA");
            inDataTable.Columns.Add("FROM_DATE");
            inDataTable.Columns.Add("TO_DATE");
            inDataTable.Columns.Add("AREAID");
            inDataTable.Columns.Add("BOXID");
            inDataTable.Columns.Add("PKG_LOTID");
            inDataTable.Columns.Add("LANGID");

            DataRow newRow = inDataTable.NewRow();
            newRow["FROM_DATE"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd 00:00:00");
            newRow["TO_DATE"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd 23:59:59");
            newRow["AREAID"] = cboArea.SelectedValue;
            newRow["BOXID"] = string.IsNullOrWhiteSpace(txtPalletID_Hist.Text)? null : txtPalletID_Hist.Text;
            newRow["PKG_LOTID"] = string.IsNullOrWhiteSpace(txtAssyLotID.Text) ? null : txtAssyLotID.Text;
            newRow["LANGID"] = LoginInfo.LANGID;         
            inDataTable.Rows.Add(newRow);

            loadingIndicator.Visibility = Visibility.Visible;

            new ClientProxy().ExecuteService("BR_PRD_GET_UNPACK_PALLET_HIST_NJ", "INDATA", "OUTDATA", inDataTable,(bizResult, bizException) =>
            {
                try
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    if (!bizResult.Columns.Contains("CHK"))
                        bizResult.Columns.Add("CHK");

                    Util.GridSetData(dgHist, bizResult, FrameOperation, true);

                    if (dgHist.Rows.Count > 0)
                    {
                        DataGridAggregate.SetAggregateFunctions(dgHist.Columns["LOTID"], new DataGridAggregatesCollection { new DataGridAggregateCount() });
                        DataGridAggregate.SetAggregateFunctions(dgHist.Columns["BOXQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                        DataGridAggregate.SetAggregateFunctions(dgHist.Columns["ACTQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
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

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            //	SFU4491		포장 해체하시겠습니까?
            Util.MessageConfirm("SFU4491", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {

                    if (string.IsNullOrEmpty(txtWorker_Main.Text) || txtWorker_Main.IsReadOnly == false)
                    {
                        // SFU1843   작업자를 입력 해 주세요.
                        Util.MessageValidation("SFU1843");
                        return;
                    }

                    if (dgPallet.GetRowCount() <= 0
                        || dgLot.GetRowCount() <= 0)
                    {
                        //SFU1645	선택된 작업대상이 없습니다.
                        Util.MessageValidation("SFU1645");
                        return;
                    }

                    DataTable dtInfo = DataTableConverter.Convert(dgPallet.ItemsSource);

                    DataSet indataSet = new DataSet();
                    DataTable inDataTable = indataSet.Tables.Add("INDATA");
                    inDataTable.Columns.Add("USERNAME");
                    inDataTable.Columns.Add("USERID");
                    inDataTable.Columns.Add("EQPTID");
                    inDataTable.Columns.Add("SHFT_ID");

                    DataTable inPalletTable = indataSet.Tables.Add("INPALLET");
                    inPalletTable.Columns.Add("BOXID");

                    DataRow newRow = inDataTable.NewRow();
                    newRow["USERNAME"] = txtWorker_Main.Text;
                    newRow["USERID"] = txtWorker_Main.Tag;
                    newRow["EQPTID"] = dtInfo.Rows[0]["EQPTID"];
                    newRow["SHFT_ID"] = txtShift_Main.Tag;
                    inDataTable.Rows.Add(newRow);

                    foreach (DataRow dr in dtInfo.Rows)
                    {
                        newRow = inPalletTable.NewRow();
                        newRow["BOXID"] = dr["BOXID"];
                        inPalletTable.Rows.Add(newRow);
                    }

                    loadingIndicator.Visibility = Visibility.Visible;

                    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_UNPACK_PALLET_NJ", "INDATA,INPALLET", "OUTDATA", (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (bizResult.Tables.Contains("OUTDATA")
                            && bizResult.Tables["OUTDATA"].Rows.Count > 0)
                            {
                                Print((string)bizResult.Tables["OUTDATA"].Rows[0]["CTNR_ID"]);
                            }
                            Clear();
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
            });
        }

        private void btnRePrint_Click(object sender, RoutedEventArgs e)
        {
            List<int> idxList = _util.GetDataGridCheckRowIndex(dgHist, "CHK");

            if (idxList.Count <= 0)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }

            foreach (int idx in idxList)
            {
                string sCtnrID = Util.NVC(dgHist.GetCell(idx, dgHist.Columns["CTNR_ID"].Index).Value);
                Print(sCtnrID);
            }
        }

        private void Print(string sCtnrID)
        {
            CMM_POLYMER_FORM_TAG_PRINT popupTagPrint = new CMM_POLYMER_FORM_TAG_PRINT();
            popupTagPrint.FrameOperation = this.FrameOperation;
            popupTagPrint.CART_MERGE = "Y";
            popupTagPrint.PrintCount = "1";

            object[] parameters = new object[5];
            parameters[0] = Process.CELL_BOXING; // "";       // _processCode;
            parameters[1] = ""; // dr["CURR_EQPTID"];     // Util.NVC(ComboEquipment.SelectedValue);
            parameters[2] = sCtnrID; // dr["CTNR_ID"];
            parameters[3] = "N";      // Direct 출력 여부
            parameters[4] = "N";      // 임시 대차 출력 여부

            C1WindowExtension.SetParameters(popupTagPrint, parameters);

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Add(popupTagPrint);
                    popupTagPrint.BringToFront();
                    break;
                }
            }
        }
    }
}
