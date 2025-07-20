using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY_004_050_SEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_050_SEL : UserControl, IWorkArea
    {

        #region [Declaration & Constructor]
        private UserControl _UCParent;
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private Util _Util;
        private int selectedWaitWipIdx;

        public ASSY004_050_SEL(UserControl parent)
        {
            InitializeComponent();
            LoginInfo.CFG_PROC_ID = Process.RWK_LNS;
            _UCParent = parent;
            _Util = new Util();
            selectedWaitWipIdx = -1;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region [Initialize Method]
        private void InitSearchArea()
        {
            //txtArea
            txtArea.Text = LoginInfo.CFG_AREA_NAME;

            //L&S 공정을 진행하는 라인들의 목록을 가져온다.
            CommonCombo _combo = new CommonCombo();
            String[] sFilter = { LoginInfo.CFG_AREA_ID, Process.RWK_LNS };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter, sCase: "PROCESSEQUIPMENTSEGMENT");

            //첫번째 라인이 자동 선택되도록 수정
            if (cboEquipmentSegment.Items.Count >= 2)
                cboEquipmentSegment.SelectedIndex = 1;
        }
        #endregion

        #region [Event]
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitSearchArea();

            //기존에 클릭해둔 상태가 Reload되어서 초기화 되는것 방지
            this.Loaded -= UserControl_Loaded;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWaitWip();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (selectedWaitWipIdx == -1)
            {
                Util.MessageInfo("SFU1632");
                return;
            }
            if (txtCreateLotCnt.Text == string.Empty || int.Parse(txtCreateLotCnt.Text) <= 0)
            {
                Util.MessageInfo("SFU3092");
                return;
            }

            CreateLot();
        }

        private void btnOutPrint_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer != null)
                dispatcherTimer.Stop();

            if (!CanSearch() || !CanUseWaitWipInfo() || !CanUseGoodProdInfo())
            {
                HideLoadingIndicator();
                return;
            }

            //인쇄하시겠습니까?
            Util.MessageConfirm("SFU1237", (result) =>
            {
                try
                {
                    // Timer Stop.
                    if (dispatcherTimer != null)
                        dispatcherTimer.Stop();

                    if (result == MessageBoxResult.OK)
                    {
                        int selectedLotIdx = _Util.GetDataGridCheckFirstRowIndex(dgGoodProd, "CHK");
                        string lotID = DataTableConverter.GetValue(dgGoodProd.Rows[selectedLotIdx].DataItem, "LOTID") as string;
                        PrintLabel(lotID);
                    }
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    if (dispatcherTimer != null && dispatcherTimer.Interval.TotalSeconds > 0)
                        dispatcherTimer.Start();
                }
            });
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveLot();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteLot();
        }

        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedWaitWipIdx == -1)
            {
                Util.MessageInfo("SFU1632");
                return;
            }
            if (txtDeftCount.Text == string.Empty || int.Parse(txtDeftCount.Text) <= 0)
            {
                Util.MessageInfo("SFU3092");
                return;
            }

            CompleteLot();
        }

        private void rbWaitWipChoice_Checked(object sender, RoutedEventArgs e)
        {
            //rb클릭시 Row선택한 것으로 되도록 설정
            //클릭한 Row의 PRODID를 가져옴
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if (rb.IsChecked.HasValue && rb.IsChecked.Value)
            {
                //rb.Parent는 부모가 보는 선택된 한 줄을 의미한다. 따라서 부모가 봤을 때는 선택된 줄이 몇번째인지 알 수 있다.
                int idx = ((DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                selectedWaitWipIdx = idx;
                dgWaitWip.SelectedIndex = idx;
            }

            //선별 양품 매거진 조회
            GetGoodProdMagazine();
        }

        private void txtCreateLotCnt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tbLot = sender as TextBox;
                if (tbLot == null)
                    return;

                if (selectedWaitWipIdx == -1)
                {
                    Util.MessageInfo("SFU1632");
                    return;
                }

                if (txtCreateLotCnt.Text == string.Empty || int.Parse(txtCreateLotCnt.Text) <= 0)
                {
                    Util.MessageInfo("SFU3092");
                    return;
                }

                //확정 하시겠습니까?
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CreateLot();
                    }
                });
            }
        }

        private void txtDeftCount_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBox tbLot = sender as TextBox;
                if (tbLot == null)
                    return;

                if (selectedWaitWipIdx == -1)
                {
                    Util.MessageInfo("SFU1632");
                    return;
                }
                if (txtDeftCount.Text == string.Empty || int.Parse(txtDeftCount.Text) <= 0)
                {
                    Util.MessageInfo("SFU3092");
                    return;
                }

                //확정 하시겠습니까?
                Util.MessageConfirm("SFU2044", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        CompleteLot();
                    }
                });
            }
        }

        private void cboEquipmentSegment_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            selectedWaitWipIdx = -1;

            if(cboEquipmentSegment.SelectedIndex > 0)
            { 
                GetWaitWip();
            }
            else
            {
                ClearGrid();
            }
        }
        #endregion

        #region [BizCall]
        private void CreateLot()
        {
            try
            {
                if (!CanSearch() || !CanUseWaitWipInfo() || !CanCreate())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));
                inDataTable.Columns.Add("WIPQTY", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                row["IFMODE"] = IFMODE.IFMODE_OFF;
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "PRODID"));
                row["WIPQTY"] = txtCreateLotCnt.Text;
                row["USERID"] = LoginInfo.USERID;
                row["EQSGID"] = cboEquipmentSegment.SelectedValue as string; //빈문자열이 아닌 null로 들어가길 원한다면 as를 사용, 그렇지 않다면 Util.NVC()사용r
                row["PROCID"] = Process.RWK_LNS;

                inDataTable.Rows.Add(row);

                new ClientProxy().ExecuteService("BR_PRD_REG_MAGAZINE_RWK_GQP_L", "INDATA", null, inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275", (result) =>
                        {
                            //수량 입력 초기화
                            txtCreateLotCnt.Clear();
                            GetWaitWip();
                        });
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                txtCreateLotCnt.Clear();
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void GetGoodProdMagazine()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PRODID", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["PROCID"] = Process.RWK_LNS;
                row["EQSGID"] = cboEquipmentSegment.SelectedValue;
                row["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "PRODID"));

                inDataTable.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_GPRD", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgGoodProd);
                        Util.GridSetData(dgGoodProd, searchResult, this.FrameOperation);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void GetWaitWip()
        {
            try
            {
                if (!CanSearch())
                {
                    HideLoadingIndicator();
                    return;
                }
                Util.gridClear(dgGoodProd);

                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("EQSGID", typeof(string));
                inDataTable.Columns.Add("PROD_CODE", typeof(string));

                DataRow row = inDataTable.NewRow();
                row["LANGID"] = LoginInfo.LANGID;
                row["PROCID"] = Process.RWK_LNS;
                row["EQSGID"] = cboEquipmentSegment.SelectedValue;
                row["PROD_CODE"] = txtProdCode.Text;

                inDataTable.Rows.Add(row);

                new ClientProxy().ExecuteService("DA_PRD_SEL_RWK_WAIT_WIP", "INDATA", "OUTDATA", inDataTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.gridClear(dgWaitWip);
                        Util.GridSetData(dgWaitWip, searchResult, this.FrameOperation);

                        //Reload하더라도 기존에 체크되어 있던 RadioButton을 살리기 위함.
                        if (selectedWaitWipIdx > -1)
                        {
                            DataTableConverter.SetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "CHK", false);
                            DataTableConverter.SetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "CHK", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void DeleteLot()
        {
            try
            {
                if (!CanSearch() || !CanUseWaitWipInfo() || !CanUseGoodProdInfo())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet dataSet = new DataSet();

                DataTable input_LOT = dataSet.Tables.Add("IN_LOT");
                input_LOT.Columns.Add("LOTID", typeof(string));

                DataTable inDataTable = dataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));

                foreach (DataRowView drv in dgGoodProd.ItemsSource)
                {
                    //CheckColumn이라고 해서 boolean으로 값이 저장되어 있지 않다. 0, 1로 저장되어 있다.
                    bool tmpFlag = (drv["CHK"] as int?).HasValue ? ((drv["CHK"] as int?).Value == 0 ? false : true) : false;

                    if (tmpFlag)
                    {
                        DataRow newRow = input_LOT.NewRow();
                        newRow["LOTID"] = Util.NVC(drv["LOTID"]);
                        input_LOT.Rows.Add(newRow);
                    }
                }

                DataRow newRow2 = inDataTable.NewRow();
                newRow2["USERID"] = LoginInfo.USERID;
                newRow2["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow2["IFMODE"] = IFMODE.IFMODE_OFF;
                inDataTable.Rows.Add(newRow2);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_DELETE_RWK_GQP_INPUT_L", "IN_INPUT,INDATA", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.MessageInfo("SFU1275", (result) =>
                        {
                           // GetGoodProdMagazine();
                            GetWaitWip();
                        });//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, dataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }

        private void CompleteLot()
        {
            try
            {
                if (!CanSearch() || !CanUseWaitWipInfo() || !CanComplete())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataTable inData = new DataTable();
                inData.Columns.Add("EQSGID", typeof(string));
                inData.Columns.Add("PRODID", typeof(string));
                inData.Columns.Add("PROCID", typeof(string));
                inData.Columns.Add("USERID", typeof(string));
                inData.Columns.Add("DEL_QTY", typeof(int));

                DataRow newRow = inData.NewRow();
                newRow["EQSGID"] = cboEquipmentSegment.SelectedValue;
                newRow["PRODID"] = Util.NVC(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "PRODID"));
                newRow["PROCID"] = Process.RWK_LNS;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["DEL_QTY"] = Convert.ToInt32(txtDeftCount.Text);
                inData.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_MODIFY_RWK_WAIT_WIPQTY_L", "INDATA", null, inData, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.MessageInfo("SFU1275", (result) =>
                        {
                            txtDeftCount.Clear();
                            GetWaitWip();
                        });//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                });
            }
            catch (Exception ex)
            {
                txtDeftCount.Clear();
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void SaveLot()
        {
            try
            {
                if (!CanSearch() || !CanUseWaitWipInfo() || !CanUseGoodProdInfo())
                {
                    HideLoadingIndicator();
                    return;
                }

                ShowLoadingIndicator();

                DataSet dataSet = new DataSet();

                DataTable input_LOT = dataSet.Tables.Add("IN_INPUT");
                input_LOT.Columns.Add("LOTID", typeof(string));
                input_LOT.Columns.Add("WIPSEQ", typeof(int));
                input_LOT.Columns.Add("WIPQTY_ED", typeof(int));

                DataTable inDataTable = dataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));

                foreach (DataRowView drv in dgGoodProd.ItemsSource)
                {
                    bool tmpFlag = (drv["CHK"] as int?).HasValue ? ((drv["CHK"] as int?).Value == 0 ? false : true) : false;

                    if (tmpFlag)
                    {
                        DataRow newRow = input_LOT.NewRow();
                        newRow["LOTID"] = Util.NVC(drv["LOTID"]);
                        newRow["WIPSEQ"] = 1;
                        newRow["WIPQTY_ED"] = Convert.ToDecimal(drv["QTY"]); //Numeric Column은 Decimal타입으로 들어온다.
                        input_LOT.Rows.Add(newRow);
                    }
                }

                DataRow newRow2 = inDataTable.NewRow();
                newRow2["USERID"] = LoginInfo.USERID;
                newRow2["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataTable.Rows.Add(newRow2);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_RWK_GQP_WIPQTY_L", "IN_INPUT,INDATA", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageInfo("SFU1275", (result) =>
                        {
                          //  GetGoodProdMagazine();
                            txtCreateLotCnt.Clear();
                            GetWaitWip();
                        });//정상 처리 되었습니다.
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }, dataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HideLoadingIndicator();
            }
        }
        #endregion

        #region [Valid Method]
        private bool CanSearch()
        {
            bool bRet = false;

            if (cboEquipmentSegment.SelectedIndex < 0 || cboEquipmentSegment.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                //Util.Alert("라인을 선택 하세요.");
                Util.MessageValidation("SFU1223");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanUseWaitWipInfo()
        {
            bool bRet = false;

            if (dgWaitWip.ItemsSource == null)
            {
                //조회 결과가 없습니다.
                Util.MessageValidation("SFU2816");
                return bRet;
            }

            if (selectedWaitWipIdx <= -1)
            {
                //선택된 대상이 없습니다.
                Util.MessageValidation("SFU1636");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanUseGoodProdInfo()
        {
            bool bRet = false;

            if (_Util.GetDataGridCheckFirstRowIndex(dgGoodProd, "CHK") == -1)
            {
                Util.MessageValidation("SFU1636");
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanCreate()
        {
            bool bRet = false;

            try
            {
                int cnt = Convert.ToInt32(txtCreateLotCnt.Text);

                if (cnt <= 0)
                {
                    //0보다 큰 수를 입력해주세요.
                    txtCreateLotCnt.Clear();
                    Util.MessageValidation("SFU7019");
                    return bRet;
                }

                decimal qty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "QTY"));
                if (Convert.ToUInt32(qty) < cnt)
                {
                    //LOT %1 재공수량보다 완공수량이 많아 처리할 수 없습니다.
                    object[] prms = new object[1];
                    prms[0] = ":";
                    txtCreateLotCnt.Clear();
                    Util.MessageValidation("1097", prms);
                    return bRet;
                }
            }
            catch (Exception ex)
            {
                txtCreateLotCnt.Clear();
                Util.MessageException(ex);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanComplete()
        {
            bool bRet = false;
            try
            {
                if (Convert.ToInt32(txtDeftCount.Text) < 0)
                {
                    //0보다 큰 수량을 입력하세요.
                    txtDeftCount.Clear();
                    Util.MessageValidation("SFU7019");
                    return bRet;
                }
                else if (Convert.ToInt32(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "QTY")) < Convert.ToInt32(txtDeftCount.Text))
                {
                    //폐기 수량이 너무 많습니다.
                    txtDeftCount.Clear();
                    Util.MessageValidation("SFU7020");
                    return bRet;
                }
            }
            catch (Exception ex)
            {
                txtDeftCount.Clear();
                Util.MessageException(ex);
                return bRet;
            }

            bRet = true;
            return bRet;
        }

        private bool CanSave()
        {
            bool bRet = false;

            try
            {
                int cnt = Convert.ToInt32(txtCreateLotCnt.Text);

                if (cnt <= 0)
                {
                    //0보다 큰 수를 입력해주세요.
                    Util.MessageValidation("SFU7019");
                    return bRet;
                }

                decimal qty = Util.NVC_Decimal(DataTableConverter.GetValue(dgWaitWip.Rows[selectedWaitWipIdx].DataItem, "QTY"));
                if (Convert.ToUInt32(qty) < cnt)
                {
                    //LOT %1 재공수량보다 완공수량이 많아 처리할 수 없습니다.
                    object[] prms = new object[1];
                    prms[0] = ":";
                    Util.MessageValidation("1097", prms);
                    return bRet;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return bRet;
            }

            bRet = true;
            return bRet;
        }
        #endregion

        #region [Util Method]
        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Visible)
                    loadingIndicator.Visibility = Visibility.Visible;
            }
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
            {
                if (loadingIndicator.Visibility != Visibility.Collapsed)
                    loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void PrintLabel(string lotID)
        {
            DataTable printInfo = new DataTable();
            printInfo.Columns.Add("LOTID");

            DataRow dr = printInfo.NewRow();
            dr["LOTID"] = lotID;
            printInfo.Rows.Add(dr);

            using (ThermalPrint thermalPrint = new ThermalPrint())
            {
                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RWK_PRINT_RWK_L", "INDATA", "OUTDATA", printInfo);
                thermalPrint.Print(result, Process.RWK_LNS, Util.NVC(cboEquipmentSegment.SelectedValue), null, THERMAL_PRT_TYPE.LAM_STK_RWK_GOOD_CELL, 1, false, false);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnCreate);
            listAuth.Add(btnSave);
            listAuth.Add(btnDelete);
            listAuth.Add(btnOutPrint);

            listAuth.Add(btnSearch);
            listAuth.Add(btnComplete);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ClearGrid()
        {
            Util.gridClear(dgWaitWip);
            Util.gridClear(dgGoodProd);
        }
        #endregion


    }
}
