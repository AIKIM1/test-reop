/*************************************************************************************
 Created Date : 2020.09.24
      Creator : 
   Decription : 출하 팔레트 분할
--------------------------------------------------------------------------------------
 [Change History]
  2020.09.24  : Initial Created. 
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_094_SPLIT_SHIP_PLT : C1Window, IWorkArea
    {

        private int MIN_SPILT_BOX_CNT = 1;  //분할 가능한 최소 박스 수량
        
        #region Declaration & Constructor 
        public COM001_094_SPLIT_SHIP_PLT()
        {
            InitializeComponent();
            Initialize();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        private DataTable _Result = null;
        private string _Act = string.Empty;

        public DataTable RESULT
        {
            get { return _Result; }
        }

        public string ACT
        {
            get { return _Act; }
        }

        private void Initialize()
        {
            InitDgShipPltSplit();
            InitDgOutboxSplit();
            InitSelectedOutboxPallet();
            InitOutboxID();
        }

        void InitDgShipPltSplit()
        {
            Util.gridClear(dgShipPltSplit);

            dgShipPltSplit.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("PALLET_ID", typeof(decimal));
            emptyTransferTable.Columns.Add("LOTID", typeof(string));
            emptyTransferTable.Columns.Add("CELL_QTY", typeof(decimal));
            emptyTransferTable.Columns.Add("BOX_QTY", typeof(decimal));

            dgShipPltSplit.ItemsSource = DataTableConverter.Convert(emptyTransferTable);

            InitDgShipPltSplitColumns();
        }

        void InitDgShipPltSplitColumns()
        {
            try
            {
                if (_Act == "SPLIT_LOT_SELECT")
                {
                    dgShipPltSplit.Columns["LOTID"].Visibility = Visibility.Visible;
                }
                else
                {
                    dgShipPltSplit.Columns["LOTID"].Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void InitDgOutboxSplit()
        {
            Util.gridClear(dgOutboxSplit);

            dgOutboxSplit.ItemsSource = null;

            DataTable emptyTransferTable = new DataTable();
            emptyTransferTable.Columns.Add("CHK", typeof(Boolean));
            emptyTransferTable.Columns.Add("PALLET_ID", typeof(decimal));
            emptyTransferTable.Columns.Add("OUTBOX_ID", typeof(string));
            emptyTransferTable.Columns.Add("CELL_QTY", typeof(decimal));

            dgOutboxSplit.ItemsSource = DataTableConverter.Convert(emptyTransferTable);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null)
                {
                    txtPalletID.Text = Util.NVC(tmps[0]);
                    txtTotalLotID.Text = Util.NVC(tmps[1]);
                    _Act = Util.NVC(tmps[2]);
                    InitDgShipPltSplitColumns();

                    GetInitOutboxList(txtPalletID.Text, txtTotalLotID.Text);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void GetInitOutboxList(string pStrPalletID, string pPkgLotID)
        {
            try
            {
                if (string.IsNullOrEmpty(pStrPalletID))
                {
                    return;
                }

                if (string.IsNullOrEmpty(pPkgLotID))
                {
                    return;
                }

                Util.gridClear(dgShipPlt01);

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("OUTER_BOXID", typeof(string));
                dtRqst.Columns.Add("PKG_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["OUTER_BOXID"] = pStrPalletID;
                dr["PKG_LOTID"] = pPkgLotID;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_OUTBOX_SHIP_PALLET", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count <= 0)
                {
                    //조회된 Data가 없습니다.
                    Util.AlertInfo("SFU1905");
                }
                else
                {
                    Util.GridSetData(dgShipPlt01, dtRslt, FrameOperation, true);

                    InitCellAndBoxQty();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void InitCellAndBoxQty()
        {
            try
            {
                DataTable dtShipPlt01 = DataTableConverter.Convert(dgShipPlt01.ItemsSource);
                
                txtBeforeSplitQty.Text = String.Format("{0:#,##0}", Util.NVC_Decimal(dtShipPlt01.Compute("SUM(CELL_QTY)", "")));
                txtBeforeSplitQty_Box.Text = String.Format("{0:#,##0}", Util.NVC_Decimal(dtShipPlt01.Compute("COUNT(OUTBOX_ID)", "")));

                txtSplitQty02.Text = String.Format("{0:#,##0}", 0);
                txtSplitQty02_Box.Text = String.Format("{0:#,##0}", 0);

                txtAfterSplitQty.Text = String.Format("{0:#,##0}", 0);
                txtAfterSplitQty_Box.Text = String.Format("{0:#,##0}", 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCellAndBoxQty()
        {
            try
            {
                DataTable dtShipPlt01 = DataTableConverter.Convert(dgShipPlt01.ItemsSource);
                DataTable dtOutboxSplit = DataTableConverter.Convert(dgOutboxSplit.ItemsSource);

                decimal dSplitQty01 = Util.NVC_Decimal(dtShipPlt01.Compute("SUM(CELL_QTY)", ""));
                decimal dSplitQty01_Box = Util.NVC_Decimal(dtShipPlt01.Compute("COUNT(OUTBOX_ID)", ""));
                
                for (int inx = 0; inx < dgShipPltSplit.Rows.Count; inx++){
                    string sPltNo = DataTableConverter.GetValue(dgShipPltSplit.Rows[inx].DataItem, "PALLET_ID").ToString();

                    decimal dSumQty = Util.NVC_Decimal(dtOutboxSplit.Compute("SUM(CELL_QTY)", "PALLET_ID = '" + sPltNo + "'"));
                    decimal dSumBox = Util.NVC_Decimal(dtOutboxSplit.Compute("COUNT(OUTBOX_ID)", "PALLET_ID = '" + sPltNo + "'"));

                    DataTableConverter.SetValue(dgShipPltSplit.Rows[inx].DataItem, "CELL_QTY", dSumQty);
                    DataTableConverter.SetValue(dgShipPltSplit.Rows[inx].DataItem, "BOX_QTY", dSumBox);
                }

                DataTable dtShipPltSplit = DataTableConverter.Convert(dgShipPltSplit.ItemsSource);

                decimal dSplitQty02 = Util.NVC_Decimal(dtShipPltSplit.Compute("SUM(CELL_QTY)", ""));
                decimal dSplitQty02_Box = Util.NVC_Decimal(dtShipPltSplit.Compute("SUM(BOX_QTY)", ""));
                txtSplitQty02.Text = String.Format("{0:#,##0}", dSplitQty02);
                txtSplitQty02_Box.Text = String.Format("{0:#,##0}", dSplitQty02_Box);

                if (dSplitQty02 == 0)
                {
                    //분할한 박스가 없을 때는Before와 동일
                    txtAfterSplitQty.Text = txtBeforeSplitQty.Text;
                    txtAfterSplitQty_Box.Text = txtBeforeSplitQty_Box.Text;
                }
                else
                {
                    //분할한 박스가 있을 때는 최초수량 - 분할한 수량
                    txtAfterSplitQty.Text = String.Format("{0:#,##0}", Util.NVC_Decimal(txtBeforeSplitQty.Text) - dSplitQty02);
                    txtAfterSplitQty_Box.Text = String.Format("{0:#,##0}", Util.NVC_Decimal(txtBeforeSplitQty_Box.Text) - dSplitQty02_Box);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnAdd_Pallet_Click(object sender, RoutedEventArgs e)
        {
            AddPallet();
            InitSelectedOutboxPallet();
        }

        private void AddPallet()
        {
            try
            {
                if (dgShipPlt01.ItemsSource == null)
                {
                    //분할 할 박스가 없습니다.
                    Util.MessageValidation("SFU3760");
                    return;
                }

                if (dgShipPlt01.Rows.Count < MIN_SPILT_BOX_CNT)
                {
                    //남은 박스가 [%1]개 이상이어야 분할 가능합니다.
                    Util.MessageValidation("SFU3761", MIN_SPILT_BOX_CNT);
                    return;
                }

                DataTable dtShipPltSplit = DataTableConverter.Convert(dgShipPltSplit.ItemsSource);

                DataRow drShipPltSplit = dtShipPltSplit.NewRow();
                if (dgShipPltSplit.Rows.Count == 0)
                {
                    //첫 삽입일 경우 2부터 시작
                    drShipPltSplit["PALLET_ID"] = 2;
                }
                else
                {
                    drShipPltSplit["PALLET_ID"] = Util.NVC_Decimal(dtShipPltSplit.Compute("MAX(PALLET_ID)", "")) + 1;
                }
                drShipPltSplit["LOTID"] = txtTotalLotID.Text;
                drShipPltSplit["CELL_QTY"] = 0;
                drShipPltSplit["BOX_QTY"] = 0;
                dtShipPltSplit.Rows.Add(drShipPltSplit);

                dtShipPltSplit.AcceptChanges();

                Util.GridSetData(dgShipPltSplit, dtShipPltSplit, FrameOperation, true);

                // 스프레드 스크롤 하단으로 이동
                dgShipPltSplit.ScrollIntoView(dgShipPltSplit.GetRowCount() - 1, 0);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDel_Pallet_Click(object sender, RoutedEventArgs e)
        {
            DelPallet();
            InitSelectedOutboxPallet();
        }

        private void DelPallet()
        {
            try
            {
                int iSeIndex = dgShipPltSplit.SelectedIndex;
                if (iSeIndex < 0)
                {
                    //선택된 Pallet가 없습니다.
                    Util.MessageValidation("SFU3425");
                    return;
                }

                string sPltNo = DataTableConverter.GetValue(dgShipPltSplit.Rows[iSeIndex].DataItem, "PALLET_ID").ToString();

                //[%1]를 삭제하시겠습니까?
                Util.MessageConfirm("SFU3475", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataTable dtOutboxSplit = DataTableConverter.Convert(dgOutboxSplit.ItemsSource);
                        DataRow[] drSelectedOutboxSplit = dtOutboxSplit.Select("PALLET_ID = '" + sPltNo + "'");

                        MoveOutboxData(dgOutboxSplit, dgShipPlt01, drSelectedOutboxSplit, 1);

                        DataTable dtShipPltSplit = DataTableConverter.Convert(dgShipPltSplit.ItemsSource);
                        dtShipPltSplit.Rows.Remove(dtShipPltSplit.Select("PALLET_ID = '" + sPltNo + "'")[0]);

                        dtShipPltSplit.AcceptChanges();

                        Util.GridSetData(dgShipPltSplit, dtShipPltSplit, FrameOperation, true);

                        SetCellAndBoxQty();
                    }
                }, new object[] { lblPallet.Text + sPltNo });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnAdd_Outbox_Click(object sender, RoutedEventArgs e)
        {
            AddOutboxToOutboxSplit();
        }

        private void AddOutboxToOutboxSplit()
        {
            try
            {
                if (dgShipPlt01.Rows.Count < MIN_SPILT_BOX_CNT)
                {
                    //남은 박스가 [%1]개 이상이어야 분할 가능합니다.
                    Util.MessageValidation("SFU3761", MIN_SPILT_BOX_CNT);
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(tbPalletId.Tag)) || Util.NVC_Decimal(tbPalletId.Tag) <= 0)
                {
                    //선택된 Pallet가 없습니다.
                    Util.MessageValidation("SFU3425");
                    return;
                }

                if (String.IsNullOrEmpty(txtOutboxID.Text))
                {
                    //OUTBOX를 입력하세요
                    Util.MessageValidation("SFU5008");
                    return;
                }

                string outboxID = txtOutboxID.Text;

                DataTable dtShipPlt01 = DataTableConverter.Convert(dgShipPlt01.ItemsSource);
                DataTable dtOutboxSplit = DataTableConverter.Convert(dgOutboxSplit.ItemsSource);
                DataRow[] drSelectedShipPlt01 = dtShipPlt01.Select("OUTBOX_ID = '" + outboxID + "'");
                DataRow[] drSelectedOutboxSplit = dtOutboxSplit.Select("OUTBOX_ID = '" + outboxID + "'");

                if (drSelectedShipPlt01.Length <= 0)
                {
                    //입력한 OUTBOX가 원 팔레트에 없을경우

                    if (drSelectedOutboxSplit.Length <= 0)
                    {
                        //입력한 OUTBOX가 분할된 팔레트에도 없을 경우

                        //Outbox 정보가 없습니다
                        Util.MessageValidation("SFU5010");
                        return;
                    }
                    else
                    {
                        //입력한 OUTBOX가 분할된 팔레트에 이미 추가되어 있을 경우

                        //이미 추가 된 OUTBOX 입니다.
                        Util.MessageValidation("SFU5011");
                        return;
                    }
                }
                else
                {
                    //입력한 OUTBOX가 원 팔레트에 있을 경우 

                    if (drSelectedOutboxSplit.Length > 0)
                    {
                        //입력한 OUTBOX가 분할된 팔레트에 이미 추가되어 있을 경우

                        //이미 추가 된 OUTBOX 입니다.
                        Util.MessageValidation("SFU5011");
                        return;
                    }else
                    {
                        //입력한 OUTBOX가 분할된 팔레트에도 없을 경우
                        //선택한 분할 팔레트로 이동

                        MoveOutboxData(dgShipPlt01, dgOutboxSplit, drSelectedShipPlt01, Util.NVC_Decimal(tbPalletId.Tag));
                        SetCellAndBoxQty();
                        InitOutboxID();

                        txtOutboxID.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void InitOutboxID()
        {
            try
            {
                txtOutboxID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnDel_Outbox_Click(object sender, RoutedEventArgs e)
        {
            DelOutboxFromOutboxSplit();
        }

        private void DelOutboxFromOutboxSplit()
        {
            try
            {
                DataTable dtOutboxSplit = DataTableConverter.Convert(dgOutboxSplit.ItemsSource);
                DataRow[] drSelectedOutboxSplit = dtOutboxSplit.Select("CHK = 1");

                if (drSelectedOutboxSplit.Length <= 0)
                {
                    //대상 OUTBOX를 선택하세요.
                    Util.MessageValidation("SFU3762");
                    return;
                }

                //OUTBOX 삭제하시겠습니까?
                Util.MessageConfirm("SFU5000", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MoveOutboxData(dgOutboxSplit, dgShipPlt01, drSelectedOutboxSplit, 1);
                        SetCellAndBoxQty();

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void MoveOutboxData(C1.WPF.DataGrid.C1DataGrid pFromGrid, C1.WPF.DataGrid.C1DataGrid pToGrid, DataRow[] pSelectedRows, decimal pToPltID)
        {
            try
            {
                DataTable dtFromGrid = DataTableConverter.Convert(pFromGrid.ItemsSource);
                DataTable dtToGrid = DataTableConverter.Convert(pToGrid.ItemsSource);

                //From 팔레트에서 To 팔레트로 이동
                for (int inx = 0; inx < pSelectedRows.Length; inx++)
                {
                    DataRow drToFrid = dtToGrid.NewRow();
                    drToFrid["CHK"] = false;
                    drToFrid["PALLET_ID"] = pToPltID;
                    drToFrid["OUTBOX_ID"] = pSelectedRows[inx]["OUTBOX_ID"];
                    drToFrid["CELL_QTY"] = pSelectedRows[inx]["CELL_QTY"];

                    dtToGrid.Rows.Add(drToFrid);
                }

                dtToGrid.AcceptChanges();

                Util.GridSetData(pToGrid, dtToGrid, FrameOperation, true);
                pToGrid.ScrollIntoView(pToGrid.GetRowCount() - 1, 0);


                //From 팔레트에서 삭제
                for (int jnx = 0; jnx < pSelectedRows.Length; jnx++)
                {
                    dtFromGrid.Rows.Remove(dtFromGrid.Select("OUTBOX_ID = '" + pSelectedRows[jnx]["OUTBOX_ID"] + "'")[0]);
                }

                dtFromGrid.AcceptChanges();

                Util.GridSetData(pFromGrid, dtFromGrid, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void dgShipPltSplit_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetSelectedOutboxPallet();
        }

        private void SetSelectedOutboxPallet()
        {
            try
            {
                if (dgShipPltSplit.GetRowCount() == 0)
                {
                    return;
                }

                int seleted_row = dgShipPltSplit.CurrentRow.Index;

                tbPalletId.Text = DataTableConverter.GetValue(dgShipPltSplit.Rows[seleted_row].DataItem, "PALLET_ID").ToString();
                tbPalletId.Tag = DataTableConverter.GetValue(dgShipPltSplit.Rows[seleted_row].DataItem, "PALLET_ID").ToString();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void InitSelectedOutboxPallet()
        {
            try
            {
                tbPalletId.Text = string.Empty;
                tbPalletId.Tag = string.Empty;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void txtOutboxID_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    AddOutboxToOutboxSplit();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSplit_Clicked(object sender, RoutedEventArgs e)
        {
            SplitPallet();
        }

        private void SplitPallet()
        {
            try
            {
                if (String.IsNullOrEmpty(txtPalletID.Text))
                {
                    //PALLETID를 입력해주세요
                    Util.MessageValidation("SFU1411");
                    return;
                }

                string sTotalLotID = txtTotalLotID.Text;
                if (String.IsNullOrEmpty(sTotalLotID))
                {
                    //LOT ID 가 없습니다.
                    Util.MessageValidation("SFU1361");
                    return;
                }

                if (dgShipPltSplit.Rows.Count <= 0)
                {
                    //Pallet 정보가 없습니다.
                    Util.MessageValidation("SFU4245");
                    return;
                }

                DataTable dtShipPltSplit = DataTableConverter.Convert(dgShipPltSplit.ItemsSource);
                if(dtShipPltSplit.Select("CELL_QTY = 0").Length > 0)
                {
                    string sPltNo = Util.NVC(dtShipPltSplit.Select("CELL_QTY = 0")[0]["PALLET_ID"]);
                    //[%1]의 Cell 수량이 [%2] 입니다.
                    Util.MessageValidation("SFU3763", new String[] { lblPallet.Text + sPltNo, "0" });
                    return;
                }

                if (dgOutboxSplit.Rows.Count <= 0)
                {
                    //OUTBOX를 입력하세요
                    Util.MessageValidation("SFU5008");
                    return;
                }

                if(Util.NVC_Decimal(txtBeforeSplitQty.Text) != Util.NVC_Decimal(txtAfterSplitQty.Text) + Util.NVC_Decimal(txtSplitQty02.Text))
                {
                    //분할전 셀 수량과 분할후 셀 수량이 다릅니다.
                    Util.MessageValidation("SFU3764");
                    return;
                }

                if (Util.NVC_Decimal(txtBeforeSplitQty_Box.Text) != Util.NVC_Decimal(txtAfterSplitQty_Box.Text) + Util.NVC_Decimal(txtSplitQty02_Box.Text))
                {
                    //분할전 박스 수량과 분할후 박스 수량이 다릅니다.
                    Util.MessageValidation("SFU3765");
                    return;
                }

                if (_Act == "SPLIT_LOT_SELECT")
                {
                    for (int inx = 0; inx < dgShipPltSplit.Rows.Count; inx++)
                    {
                        string sLotid = DataTableConverter.GetValue(dgShipPltSplit.Rows[inx].DataItem, "LOTID").ToString();

                        if (string.IsNullOrEmpty(sLotid))
                        {
                            //조립LOT 정보가 없는 데이터가 존재합니다.
                            Util.MessageValidation("SFU4225");
                            dgShipPltSplit.SelectedIndex = inx;
                            return;
                        }

                        Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]+$");
                        Boolean ismatch = regex.IsMatch(sLotid);
                        if (!ismatch)
                        {
                            //LOTID는 숫자와 영문대문자만 입력가능합니다.
                            Util.MessageValidation("SFU3768");
                            dgShipPltSplit.SelectedIndex = inx;
                            return;
                        }

                        if (sTotalLotID.Length == 8) //원형
                        {
                            if (Util.NVC(sLotid).ToString().Trim().Length != 8)
                            {
                                //조립LOT 정보는 8자리 입니다.
                                Util.MessageValidation("SFU4228");
                                dgShipPltSplit.SelectedIndex = inx;
                                return;
                            }
                        }
                        else if (sTotalLotID.Length == 10) // 초소형
                        {
                            if (Util.NVC(sLotid).ToString().Trim().Length != 10)
                            {
                                //조립LOT 정보는 10자리 입니다.
                                Util.MessageValidation("SFU4229");
                                dgShipPltSplit.SelectedIndex = inx;
                                return;
                            }
                        }

                        if (dtShipPltSplit.Select("LOTID = '" + sLotid + "'").Length >= 2)
                        {
                            //중복된 조립LOT 정보가 존재합니다.
                            Util.MessageValidation("SFU4226");
                            return;
                        }
                    }
                }

                //분할하시겠습니까?
                Util.MessageConfirm("SFU4175", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DoSplitPallet();
                    }
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void DoSplitPallet()
        {
            try
            {
                _Result = null; //파라미터로 넘겨줄 결과 초기화

                DataSet inData = new DataSet();

                //마스터 정보
                DataTable inDataTable = inData.Tables.Add("INDATA");
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PALLETID", typeof(string));
                inDataTable.Columns.Add("FROM_OUTBOX_QTY", typeof(decimal));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("ACT", typeof(string));

                DataRow rowINDATA = inDataTable.NewRow();
                rowINDATA["SRCTYPE"] = "UI";
                rowINDATA["IFMODE"] = "OFF";
                rowINDATA["PALLETID"] = txtPalletID.Text;
                rowINDATA["FROM_OUTBOX_QTY"] = Convert.ToDecimal(txtAfterSplitQty_Box.Text.Replace(",", ""));
                rowINDATA["USERID"] = LoginInfo.USERID;
                rowINDATA["ACT"] = _Act;
                inDataTable.Rows.Add(rowINDATA);
                

                //분할 Pallet
                DataTable inLot = inData.Tables.Add("INLOT");
                inLot.Columns.Add("ACTQTY", typeof(decimal));
                inLot.Columns.Add("OUTBOX_QTY", typeof(decimal));
                inLot.Columns.Add("OUTBOX_LIST", typeof(string));
                inLot.Columns.Add("LOTID", typeof(string));

                DataRow rowINLOT = null;
                string sOutboxList = string.Empty;

                //원팔레트
                /*
                rowINLOT = inLot.NewRow();
                DataTable dtShipPlt01 = DataTableConverter.Convert(dgShipPlt01.ItemsSource);
                rowINLOT["ACTQTY"] = Util.NVC_Decimal(dtShipPlt01.Compute("SUM(CELL_QTY)", ""));
                rowINLOT["OUTBOX_QTY"] = Util.NVC_Decimal(dtShipPlt01.Compute("COUNT(OUTBOX_ID)", ""));
                sOutboxList = "";
                for (int inx = 0; inx < dgShipPlt01.Rows.Count; inx++)
                {
                    sOutboxList = sOutboxList + DataTableConverter.GetValue(dgShipPlt01.Rows[inx].DataItem, "OUTBOX_ID").ToString() + ",";
                }

                rowINLOT["OUTBOX_LIST"] = string.IsNullOrEmpty(sOutboxList) ? sOutboxList : sOutboxList.Remove(sOutboxList.Length - 1);
                inLot.Rows.Add(rowINLOT);
                */

                //분할된 팔레트
                string sPltNo = string.Empty;
                string sLotID = string.Empty;
                for (int jnx = 0; jnx < dgShipPltSplit.Rows.Count; jnx++)
                {
                    rowINLOT = inLot.NewRow();

                    rowINLOT["ACTQTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgShipPltSplit.Rows[jnx].DataItem, "CELL_QTY")));
                    rowINLOT["OUTBOX_QTY"] = Convert.ToDecimal(Util.NVC(DataTableConverter.GetValue(dgShipPltSplit.Rows[jnx].DataItem, "BOX_QTY")));

                    sPltNo = DataTableConverter.GetValue(dgShipPltSplit.Rows[jnx].DataItem, "PALLET_ID").ToString();
                    sOutboxList = "";
                    DataTable dtOutboxSplit = DataTableConverter.Convert(dgOutboxSplit.ItemsSource);
                    DataRow[] drSelectedOutboxSplit = dtOutboxSplit.Select("PALLET_ID = '" + sPltNo + "'");
                    for(int knx = 0; knx < drSelectedOutboxSplit.Length; knx++)
                    {
                        sOutboxList = sOutboxList + drSelectedOutboxSplit[knx]["OUTBOX_ID"] + ",";
                    }
                    rowINLOT["OUTBOX_LIST"] = string.IsNullOrEmpty(sOutboxList) ? sOutboxList : sOutboxList.Remove(sOutboxList.Length - 1);

                    sLotID = DataTableConverter.GetValue(dgShipPltSplit.Rows[jnx].DataItem, "LOTID").ToString();
                    if (_Act == "SPLIT_LOT_SELECT")
                    {
                        rowINLOT["LOTID"] = sLotID;
                    }
                    else
                    {
                        rowINLOT["LOTID"] = txtTotalLotID.Text;
                    }

                    inLot.Rows.Add(rowINLOT);
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_SPLIT_SHIP_PALLET", "INDATA,INLOT", "OUTDATA", (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    //분할 되었습니다.
                    Util.MessageValidation("SFU1573");

                    //2020-02-03 최상민 수정 Tables[1] -> Tables["OUTDATA"]
                    _Result = Result.Tables["OUTDATA"];
                    this.DialogResult = MessageBoxResult.OK;

                }, inData);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnSelAdd_Outbox_Click(object sender, RoutedEventArgs e)
        {
            SelectedMultiOutboxAdd();
        }

        private void SelectedMultiOutboxAdd()
        {
            try
            {
                DataTable dtShipPlt01 = DataTableConverter.Convert(dgShipPlt01.ItemsSource);
                DataRow[] drShipPlt01 = dtShipPlt01.Select("CHK = 1");

                if (drShipPlt01.Length <= 0)
                {
                    //대상 OUTBOX를 선택하세요.
                    Util.MessageValidation("SFU3762");
                    return;
                }

                if (dgShipPlt01.Rows.Count - drShipPlt01.Length < MIN_SPILT_BOX_CNT - 1)
                {
                    //남은 박스가 [%1]개 이상이어야 분할 가능합니다.
                    Util.MessageValidation("SFU3761", MIN_SPILT_BOX_CNT);
                    return;
                }

                if (dgShipPltSplit.Rows.Count <= 0 || dgShipPlt01.Rows.Count <= 0)
                {
                    //Pallet 정보가 없습니다.
                    Util.MessageValidation("SFU4245");
                    return;
                }

                if (string.IsNullOrEmpty(Util.NVC(tbPalletId.Tag)) || Util.NVC_Decimal(tbPalletId.Tag) <= 0)
                {
                    //선택된 Pallet가 없습니다.
                    Util.MessageValidation("SFU3425");
                    return;
                }

                //[%1]에서 선택한 [%2]개의 OUTBOX 를 [%3]에 추가하시겠습니까?
                Util.MessageConfirm("SFU3766", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        MoveOutboxData(dgShipPlt01, dgOutboxSplit, drShipPlt01, Util.NVC_Decimal(tbPalletId.Tag));
                        SetCellAndBoxQty();
                    }
                }, new object[] { lblPallet01.Text, Util.NVC_DecimalStr(drShipPlt01.Length), lblPallet.Text + tbPalletId.Tag });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void btnReSet_Clicked(object sender, RoutedEventArgs e)
        {
            Initialize();
            GetInitOutboxList(txtPalletID.Text, txtTotalLotID.Text);
        }

        private void txtLotID_Select_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                System.Windows.Controls.TextBox txtBox = sender as System.Windows.Controls.TextBox;
                if (txtBox.Text != string.Empty)
                {
                    Regex regex = new System.Text.RegularExpressions.Regex(@"^[0-9A-Z]+$");
                    Boolean ismatch = regex.IsMatch(txtBox.Text);
                    if (!ismatch)
                    {
                        txtBox.Focus();

                        //LOTID는 숫자와 영문대문자만 입력가능합니다.
                        Util.MessageValidation("SFU3768");
                        return;
                    }
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
