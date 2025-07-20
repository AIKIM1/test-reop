/*************************************************************************************
 Created Date : 2018.05.01
      Creator : 
   Decription : 활성화 대차 LOSS 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.05.01  오화백 : Initial Created.
  2018.07.26  오화백 : 불량LOSS 취소.
  2024.06.13  이병윤 : E20240430-000753 1. 메뉴명 [활성화 불량 Loss/물품청구 이력 조회] 변경,Loss취소 -> 폐기취소 변경
                                        2. 활동에 물품청구 추가 후 검색 가능하게 수정, DPMS입고 관련 컬럼 추가
                                        3. DPMS입고 처리전 폐기취소 가능, DPMS입고 처리후 폐기취소 불가, 물품청구 경우 폐기취소 불가
**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using C1.WPF;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_234 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

       

        public COM001_234()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void UserControl_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            InitCombo();

            this.Loaded -= UserControl_Loaded;
        }

        #endregion

        #region Initialize
        private void Initialize()
        {
        }
        //화면내 combo 셋팅
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboProcid, cboEqsgid };
            _combo.SetCombo(cboAreaid, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, sCase: "AREA");
            cboAreaid.SelectedValue = LoginInfo.CFG_AREA_ID;

            //공정
            C1ComboBox[] cboLineParent = { cboAreaid };
            C1ComboBox[] cboLineChild = { cboEqsgid };
            _combo.SetCombo(cboProcid, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_PROCESS", cbChild: cboLineChild, cbParent: cboLineParent);
            cboProcid.SelectedValue = LoginInfo.CFG_PROC_ID;

            //라인
            C1ComboBox[] cboProcessParent = { cboAreaid, cboProcid };
            _combo.SetCombo(cboEqsgid, CommonCombo.ComboStatus.ALL, cbParent: cboProcessParent, sCase: "PROCESS_EQUIPMENT");

            SetLossCombo(cboLoss);

        }

        #endregion

        #region Event

        #region 활성화 대차 LOSS 이력 조회 : btnSearch_Click()
        //조회

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetLoss_Info();

        }
        #endregion
    
        #region 활성화 불량 LOSS 이력 취소 : btnLossCancel_Click()
        private void btnLossCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCancel()) return;
               popUpLossCancel();
        }
        #endregion

        #region 활성화 불량 Loss 취소 팝업 : popupDefectInput_Closed()

        private void popupDefectInput_Closed(object sender, EventArgs e)
        {
            COM001_234_LOSS_CANCEL popup = sender as COM001_234_LOSS_CANCEL;

            if (popup.DialogResult == MessageBoxResult.OK)
            {
                // 재조회
                GetLoss_Info();
            }
            this.grdMain.Children.Remove(popup);
        }


        #endregion



        #endregion

        #region Method

        #region 활성화 대차 LOSS 이력 조회 : GetLoss_Info()
        // 조회
        private void GetLoss_Info()
        {
            try
            {
                ShowLoadingIndicator();


                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }


                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("FR_DTTM", typeof(string));
                inTable.Columns.Add("TO_DTTM", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PJT_NAME", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("LOTID_RT", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));

                DataRow newRow = inTable.NewRow();

                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FR_DTTM"] = Util.GetCondition(dtpDateFrom);
                newRow["TO_DTTM"] = Util.GetCondition(dtpDateTo);
                newRow["AREAID"]  = Util.GetCondition(cboAreaid, "SFU4238"); //동을 선택하세요
                if (newRow["AREAID"].Equals("")) return;
                //newRow["PROCID"] = Util.GetCondition(cboProcid, "SFU1459"); //공정을 선택하세요
                //if (newRow["PROCID"].Equals("")) return;
                newRow["PROCID"] = Util.GetCondition(cboProcid, bAllNull: true);
                newRow["EQSGID"] = Util.GetCondition(cboEqsgid, bAllNull: true);
                newRow["PJT_NAME"] = txtPjt.Text == string.Empty ? null : txtPjt.Text;
                newRow["PRODID"] = txtProdid.Text == string.Empty ? null : txtProdid.Text;
                newRow["CTNR_ID"] = txtCartID.Text == string.Empty ? null : txtCartID.Text;
                newRow["LOTID_RT"] = txtLot_RT.Text == string.Empty ? null : txtLot_RT.Text;
                newRow["ACTID"] = Util.NVC(cboLoss.SelectedValue).Equals("") ? null : Util.NVC(cboLoss.SelectedValue); //Util.GetCondition(cboLoss, bAllNull: true);
                newRow["SUBLOTID"] = Util.ConvertEmptyToNull(txtCellID.Text.Trim());

                inTable.Rows.Add(newRow);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOSS_LIST_PC", "INDATA", "OUTDATA", inTable);

                decimal SumResnQty = 0;

                //for (int i = 0; i < dtMain.Rows.Count; i++)
                //{
                //    if (dtMain.Rows[i]["CANCELDTTM"].ToString() != string.Empty)
                //    {
                //        dtMain.Rows[i]["CANCEL_QTY"] = dtMain.Rows[i]["RESNQTY"];
                //        dtMain.Rows[i]["RESNQTY"] = dtMain.Rows[i]["PRE_RESNQTY"];
                //    }
                //    if (dtMain.Rows[i]["CANCELDTTM"].ToString() == string.Empty)
                //    {
                //        SumResnQty = SumResnQty + Convert.ToDecimal(dtMain.Rows[i]["RESNQTY"]);

                //    }
                //}

                Util.gridClear(dgSearch);
                Util.GridSetData(dgSearch, dtMain, FrameOperation, true);

                //DataGridAggregate.SetAggregateFunctions(dgSearch.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum { ResultFormat = SumResnQty.ToString("###,###") } });
                if (dtMain.Rows.Count > 0)
                {
                    DataGridAggregate.SetAggregateFunctions(dgSearch.Columns["RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                    DataGridAggregate.SetAggregateFunctions(dgSearch.Columns["PRE_RESNQTY"], new DataGridAggregatesCollection { new DataGridAggregateSum() });
                }

            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

        #region 활성화 대차 LOSS 취소 팝업 : popUpLossCancel()
        private void popUpLossCancel()
        {
            DataTable dtLossCancel = DataTableConverter.Convert(dgSearch.ItemsSource).Select("CHK = '1'").CopyToDataTable();

            COM001_234_LOSS_CANCEL popupDefectInput = new COM001_234_LOSS_CANCEL();


            object[] parameters = new object[2];
            parameters[0] = dtLossCancel;
            parameters[1] = cboAreaid.SelectedValue.ToString();
            C1WindowExtension.SetParameters(popupDefectInput, parameters);
            popupDefectInput.Closed += new EventHandler(popupDefectInput_Closed);
            grdMain.Children.Add(popupDefectInput);
            popupDefectInput.BringToFront();
        }

        #endregion


        #endregion

        #region Funct
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        private void SetLossCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_PRD_SEL_LOSS_CBO";
            string[] arrColumn = { "LANGID" };
            string[] arrCondition = { LoginInfo.LANGID };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText);
        }



        #endregion

        #region [Validation]
        /// <summary>
        /// Loss 췻 Validation
        /// </summary>
        private bool ValidationCancel()
        {
            if (dgSearch.Rows.Count == 1)
            {
                // 조회된 데이터가 없습니다.
                Util.MessageValidation("SFU3537");
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgSearch, "CHK");

            if (drInfo.Count() <= 0)
            {
                // 선택된 데이터가 없습니다.
                Util.MessageValidation("SFU3538");
                return false;
            }

            // E20240430-000753 : DPMS입고 처리전 폐기취소 가능, DPMS입고 처리후 폐기취소 불가,물품청구 경우 폐기취소 불가
            for (int k = 0; k < drInfo.Count(); k++)
            {
                string sActId = Util.NVC(drInfo[k]["ACTID"].ToString());
                string sDpmsCd = Util.NVC(drInfo[k]["DPMS_PRCS_STAT_CODE"].ToString());
                string sDpmsNo = Util.NVC(drInfo[k]["DPMS_RCV_ISS_NO"].ToString());
                if (sActId.Equals("CHARGE_PROD_LOT"))
                {
                    // 물품청구는 폐기취소를 할 수 없습니다.
                    Util.MessageValidation("SFU9914");
                    return false;
                }
                else if (sActId.Equals("LOSS_LOT"))
                {
                    if (sDpmsNo.Equals(""))
                    {
                        Util.MessageValidation("SFU9915");
                        return false;
                    }
                    if (!sDpmsCd.Equals("SHIP"))
                    {
                        Util.MessageValidation("SFU9915");
                        return false;
                    }
                }
                else if (sActId.Equals("CANCEL_LOSS_LOT"))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        private void dgSearch_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            if (e.Column.Name == "CHK")
            {
                if (Util.NVC(DataTableConverter.GetValue(dgSearch.Rows[e.Row.Index].DataItem, "CANCELDTTM")) != string.Empty)
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }

        private void dgSearch_LoadedCellPresenter(object sender, DataGridCellEventArgs e)
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
                    if (Util.NVC(DataTableConverter.GetValue(e.Cell.Row.DataItem, "CANCELDTTM")) != string.Empty)
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E8F7C8"));

                    }
                    else
                    {
                        e.Cell.Presenter.Background = null;
                    }
                }
            }));
        }

        private void dgSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
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

        private void btnCartCellRegisterCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] dr;

                if(dgSearch.ItemsSource == null)
                {
                    Util.AlertInfo("SFU1636"); // 선택된 대상이 없습니다.
                    return;
                }
                dr = DataTableConverter.Convert(dgSearch.ItemsSource).Select("CHK = 1");

                if (dr.Length > 1)
                {
                    Util.AlertInfo("SFU4468"); //한개의 데이터만 선택하세요
                    return;
                }
                else if (dr.Length == 0)
                {
                    Util.AlertInfo("SFU1636"); // 선택된 대상이 없습니다.
                    return;
                }

                string resnqty = Util.NVC(dr[0]["RESNQTY"]);

                if (string.IsNullOrEmpty(resnqty) || resnqty.Equals("0"))
                {
                    Util.Alert("SFU1683");
                    return;
                }

                int to_qty = 0;

                if (resnqty.Contains('.'))
                    to_qty = int.Parse(resnqty.Substring(0, resnqty.IndexOf('.')));
                else
                    to_qty = int.Parse(resnqty); 

                string ctnrID = Util.NVC(dr[0]["CTNR_ID"]);
                

                CMM001.CMM_ASSY_LOSS_CELL_CANCEL popupoutput = new CMM001.CMM_ASSY_LOSS_CELL_CANCEL();


                popupoutput.FrameOperation = this.FrameOperation;
                object[] parameters = new object[2];
                parameters[0] = ctnrID;
                parameters[1] = to_qty;
                C1WindowExtension.SetParameters(popupoutput, parameters);
                grdMain.Children.Add(popupoutput);
                popupoutput.BringToFront();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
    }
}

