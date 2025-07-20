using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace LGC.GMES.MES.CMM001
{

    public partial class CMM_ASSY_LOSS_CELL_CANCEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor


        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        DataTable LOSS_CELL = null;

        private CheckBoxHeaderType _inBoxHeaderType;

        private bool _load = true;

        public bool isInputQty = false;

        private int? _inputQty = 0;

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_ASSY_LOSS_CELL_CANCEL()
        {
            InitializeComponent();
        }


        private void InitializeUserControls()
        {
        }

        /// <summary>
        ///  팝업 셋팅
        /// </summary>
        private void SetControl()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            string _ctnrID = parameters[0] as string;
            int? _qty = parameters[1] as int?;
            //_inputQty = parameters[1] as int?;
            if (string.IsNullOrEmpty(_ctnrID))
                return;

            SetGridCartList(_ctnrID,_qty);

            SetGridLossList();
            if (dgCell.Rows.Count > 0)
            {
                SetGridLotIdRt();
            }


        }


        #endregion


        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();

                _load = false;
            }

        }

        #endregion

        #region Cell ID 바코드 영문 적용 : txtCellID_GotFocus()
        /// <summary>
        /// 바코드 영문 적용
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCellID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Cell 정보 조회 및 입력  : txtCellID_KeyDown()
        /// <summary>
        /// Cell 정보 조회 및 입력
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtCellID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Validation())
                    {
                        return;
                    }

                    string sLotid = txtCellID.Text.Trim();
                    if (dgCell.GetRowCount() > 0)
                    {
                        for (int i = 0; i < dgCell.GetRowCount(); i++)
                        {
                            if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID").ToString() == sLotid)
                            {
                                DeleteCell(sLotid);
                                txtCellID.Focus();
                                txtCellID.Text = string.Empty;
                                return;
                            }
                            if(i == dgCell.GetRowCount() - 1)
                            {
                                Util.Alert("PSS9106");
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Cell Header CHeck
        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgCell;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }
        #endregion

        #region  Cell 삭제  : btnDelete_Click()
        // 불량 Cell 삭제
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation_Delete())
            {
                return;
            }

            DeleteCell();
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion




        #region 대차ID  셋팅 : SetGridCartList()
        /// <summary>
        /// Cart List 셋팅
        /// </summary>
        private void SetGridCartList(string ctnrID,int ? qty)
        {
            try
            {
                DataTable dtCtnr = new DataTable();
                dtCtnr.Columns.Add("CTNR_ID", typeof(string));
                dtCtnr.Columns.Add("LANGID", typeof(string));

                DataRow dr = dtCtnr.NewRow();
                dr["CTNR_ID"] = ctnrID;
                dr["LANGID"] = LoginInfo.LANGID;

                dtCtnr.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CTNR_LOSS_LIST", "INDATA", "OUTDATA", dtCtnr);              

                Util.GridSetData(dgCtnr, dtRslt, FrameOperation);
               
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "WIPQTY", qty);
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", 0);
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", qty);

                /*string chkCellMngt = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CELL_ID_MNGT_FLAG"));

                if (!chkCellMngt.Equals("Y"))
                {
                    Util.Alert("SFU8307");
                    this.DialogResult = MessageBoxResult.Cancel;
                }*/


                //if (isInputQty)
                //{
                //    DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", _inputQty);
                //}
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        #region 팝업 로드 시 Cell List 조회 :SetGridDefectList()
        // <summary>
        /// 팝업 로드시 불량 Cell List
        /// </summary>
        private void SetGridLossList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LOSS_CTNR_ID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOSS_CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
                dtRqst.Rows.Add(dr);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CTNR_LOSS_CELL", "INDATA", "OUTDATA", dtRqst);
                Util.GridSetData(dgCell, dtRslt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region Cell 입력 및 삭제시 Scan 수량, 미 Scan 수량 계산:SumScanQty()
        //Scan 수량/ 미스캔수량 계산
        private void SumScanQty()
        {
            try
            {
                //조립LOT  계산
                //여러개의 같은데이터를 GROUP BY 
                DataTable LinQ = new DataTable();
                DataRow Linqrow = null;
                LinQ = DataTableConverter.Convert(dgCell.ItemsSource).Clone();

                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    Linqrow = LinQ.NewRow();
                    Linqrow["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "LOTID_RT"));
                    Linqrow["CELLID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID"));
                    Linqrow["SUBLOTQTY"] = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTQTY")).Replace(",", ""));
                    LinQ.Rows.Add(Linqrow);

                }
                var summarydata = from SUMrow in LinQ.AsEnumerable()
                                  group SUMrow by new
                                  {
                                      LOTID_RT = SUMrow.Field<string>("LOTID_RT")
                                  } into grp
                                  select new
                                  {
                                      LOTID_RT = grp.Key.LOTID_RT
                                      ,
                                      ALL_SUBLOTQTY = grp.Sum(r => r.Field<Int32>("SUBLOTQTY"))
                                  };


                DataTable SumDT = new DataTable();
                SumDT = LinQ.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = SumDT.NewRow();
                    nrow["LOTID_RT"] = data.LOTID_RT;
                    nrow["SUBLOTQTY"] = data.ALL_SUBLOTQTY;
                    SumDT.Rows.Add(nrow);
                }
                Util.GridSetData(dgAssyLot, SumDT, FrameOperation, false);

                int SumCtnrScanQty = 0;
                int SumCtnrNonscanQty = 0;

                /*
                // 대차 스프레드 수량 수정
                for (int i = 0; i < dgAssyLot.Rows.Count; i++)
                {
                    SumCtnrScanQty = SumCtnrScanQty + Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgAssyLot.Rows[i].DataItem, "SUBLOTQTY")).Replace(",", ""));
                }

                SumCtnrNonscanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "WIPQTY")).Replace(",", "")) - SumCtnrScanQty;

                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", SumCtnrScanQty);
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", SumCtnrNonscanQty);
                */

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region 불량 Cell 삭제 : DeleteCell()

        //불량Cell 삭제
        private void DeleteCell(string cellID)
        {
            DataSet indataSet = new DataSet();
            DataTable inData = indataSet.Tables.Add("INDATA");
            inData.Columns.Add("USERID", typeof(string));

            DataRow row = inData.NewRow();
            row["USERID"] = LoginInfo.USERID;
            indataSet.Tables["INDATA"].Rows.Add(row);

            DataTable inSublot = indataSet.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("SUBLOTID", typeof(string));

            DataRow dr = inSublot.NewRow();
            dr["SUBLOTID"] = cellID;
            indataSet.Tables["INSUBLOT"].Rows.Add(dr);
            try
            {
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CTNR_LOSS_SUBLOT_REMOVE", "INDATA,INSUBLOT", "", (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);
                        return;
                    }
                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                    SetGridLossList();
                    SumScanQty();

                    int SumCtnrScanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY")).Replace(",", "")) + 1;
                    int SumCtnrNonscanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY")).Replace(",", "")) - 1;

                    DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", SumCtnrScanQty);
                    DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", SumCtnrNonscanQty);
                }, indataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteCell()
        {

            DataSet indataSet = new DataSet();
            DataTable inData = indataSet.Tables.Add("INDATA");
            inData.Columns.Add("USERID", typeof(string));

            DataRow row = inData.NewRow();
            row["USERID"] = LoginInfo.USERID;
            indataSet.Tables["INDATA"].Rows.Add(row);

            DataTable inSublot = indataSet.Tables.Add("INSUBLOT");
            inSublot.Columns.Add("SUBLOTID", typeof(string));

            DataRow dr = null;

            int deleteCnt = 0;

            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    deleteCnt++;
                }

            }

            int CtnrWipQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "WIPQTY")).Replace(",", ""));
            int CtnrScanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY")).Replace(",", ""));

            if(CtnrWipQty < CtnrScanQty + deleteCnt)
            {
                Util.Alert("SFU8314");
                return;
            }

            for (int i = 0; i < dgCell.Rows.Count; i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CHK")) == "1")
                {
                    dr = inSublot.NewRow();
                    dr["SUBLOTID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID"));
                    indataSet.Tables["INSUBLOT"].Rows.Add(dr);
                }

            }


            try
            {
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CTNR_LOSS_SUBLOT_REMOVE", "INDATA,INSUBLOT", "", (result, Exception) =>
                {
                    if (Exception != null)
                    {
                        Util.MessageException(Exception);                  
                        return;
                    }
                    txtCellID.Focus();
                    txtCellID.Text = string.Empty;
                    SetGridLossList();
                    SumScanQty();

                    int SumCtnrScanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY")).Replace(",", "")) + 1;
                    int SumCtnrNonscanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY")).Replace(",", "")) - 1;

                    DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", SumCtnrScanQty);
                    DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", SumCtnrNonscanQty);
                }, indataSet);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion


        #region 공통

        #region 조립 LOT 조회 : SetGridLotIdRt()
        // <summary>
        /// 조립LOT 조회
        /// </summary>
        private void SetGridLotIdRt()
        {
            try
            {
                //조립LOT  계산
                //여러개의 같은데이터를 GROUP BY 
                DataTable LinQ = new DataTable();
                DataRow Linqrow = null;
                LinQ = DataTableConverter.Convert(dgCell.ItemsSource).Clone();

                for (int i = 0; i < dgCell.Rows.Count; i++)
                {
                    Linqrow = LinQ.NewRow();
                    Linqrow["LOTID_RT"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "LOTID_RT"));
                    Linqrow["CELLID"] = Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID"));
                    Linqrow["SUBLOTQTY"] = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "SUBLOTQTY")).Replace(",", ""));
                    LinQ.Rows.Add(Linqrow);

                }
                var summarydata = from SUMrow in LinQ.AsEnumerable()
                                  group SUMrow by new
                                  {
                                      LOTID_RT = SUMrow.Field<string>("LOTID_RT")
                                  } into grp
                                  select new
                                  {
                                      LOTID_RT = grp.Key.LOTID_RT
                                      ,
                                      ALL_SUBLOTQTY = grp.Sum(r => r.Field<Int32>("SUBLOTQTY"))
                                  };


                DataTable SumDT = new DataTable();
                SumDT = LinQ.Clone();
                foreach (var data in summarydata)
                {
                    DataRow nrow = SumDT.NewRow();
                    nrow["LOTID_RT"] = data.LOTID_RT;
                    nrow["SUBLOTQTY"] = data.ALL_SUBLOTQTY;
                    SumDT.Rows.Add(nrow);
                }
                Util.GridSetData(dgAssyLot, SumDT, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region Cell ID의 불량 Cell 조회 (Validation 시 사용) : SetDefcCell();
        // <summary>
        /// 불량 Cell 조회
        /// </summary>

      
        #endregion

        #region Validation()
        // <summary>
        /// Validation
        /// </summary>
        private bool Validation()
        {

            if (txtCellID.Text == string.Empty)
            {

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1319"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.Text = string.Empty;
                    }
                });

                return false;
            }

            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY")) == "0")
            {
                //스캔 Cell수량이 등록취소할 Cell 수량보다 큽니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8313"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.Text = string.Empty;
                    }
                });

                return false;
            }

            return true;
        }

        #endregion

        private bool Validation_Delete()
        {

            if (dgCell.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return false;
            }

            DataRow[] drInfo = Util.gridGetChecked(ref dgCell, "CHK");

            if (drInfo.Count() <= 0)
            {
                Util.MessageValidation("SFU3538");//선택된 데이터가 없습니다.
                return false;
            }


            if (Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY")) == "0")
            {
                //스캔 Cell수량이 등록취소할 Cell 수량보다 큽니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8313"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        txtCellID.Focus();
                        txtCellID.Text = string.Empty;
                    }
                });

                return false;
            }


            return true;
        }



        #region [Func]

        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }

        #endregion

        #endregion
    }
}
