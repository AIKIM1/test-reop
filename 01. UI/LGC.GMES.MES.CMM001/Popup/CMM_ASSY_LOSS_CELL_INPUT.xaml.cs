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
using System.Configuration;
using C1.WPF.Excel;
using System.IO;
using System.Collections;

namespace LGC.GMES.MES.CMM001
{

    public partial class CMM_ASSY_LOSS_CELL_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor


        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
        DataTable LOSS_CELL = null;

        private CheckBoxHeaderType _inBoxHeaderType;

        private bool _load = true;

        public bool isInputQty = false;

        private string _Mode = string.Empty;    //C20210412-000321

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

        public CMM_ASSY_LOSS_CELL_INPUT()
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

            if (parameters == null)
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return;
            }

            string _ctnrID = parameters[0] as string;
            if (string.IsNullOrEmpty(_ctnrID))
            {
                Util.MessageValidation("SFU3538"); //선택된 데이터가 없습니다.
                return;
            }

            if(parameters.Length == 2)
            {
                _Mode = parameters[1] as string;    //C20210412-000321
            }

            SetComponentSettingByMode();    //C20210412-000321

            SetGridCartList(_ctnrID);

            if(dgCtnr.Rows.Count > 0)
            {
                SetGridLossList();

                if (dgCell.Rows.Count > 0)
                {
                    SetGridLotIdRt();
                }
            }
        }

        private void SetComponentSettingByMode()    //C20210412-000321
        {
            if(_Mode == "SELECT")
            {
                txtCellID.IsEnabled = false;
                btnExcelUpload.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }else
            {
                txtCellID.IsEnabled = true;
                btnExcelUpload.IsEnabled = true;
                btnDelete.IsEnabled = true;
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
                    string sCellID = txtCellID.Text.Trim();

                    InputCellProcess(sCellID);
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void InputCellProcess(string pStrCellID)
        {
            try
            {
                if (!Validation(pStrCellID))
                {
                    return;
                }

                string sLotid = pStrCellID;

                if (dgCell.GetRowCount() > 0)
                {
                    for (int i = 0; i < dgCell.GetRowCount(); i++)
                    {
                        if (DataTableConverter.GetValue(dgCell.Rows[i].DataItem, "CELLID").ToString() == sLotid)
                        {

                            dgCell.SelectedIndex = i;
                            dgCell.ScrollIntoView(i, dgCell.Columns["CHK"].Index);
                            DataTableConverter.SetValue(dgCell.Rows[i].DataItem, "CHK", 1);
                            txtCellID.Focus();
                            txtCellID.Text = string.Empty;
                            return;
                        }
                    }
                }

                InputCell(pStrCellID);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void InputCellProcessExcel(ArrayList list)
        {
            try
            {
                if (!ValidationExcel(list))
                {
                    return;
                }

                string sLotid;

                for (int i = 0; i < list.Count; i++)
                {
                    sLotid = list[i].ToString();

                    if (dgCell.GetRowCount() > 0)
                    {
                        for (int j = 0; j < dgCell.GetRowCount(); j++)
                        {
                            if (DataTableConverter.GetValue(dgCell.Rows[j].DataItem, "CELLID").ToString() == sLotid)
                            {
                                dgCell.SelectedIndex = j;
                                dgCell.ScrollIntoView(j, dgCell.Columns["CHK"].Index);
                                DataTableConverter.SetValue(dgCell.Rows[j].DataItem, "CHK", 1);
                                txtCellID.Focus();
                                txtCellID.Text = string.Empty;

                                Util.MessageValidation("SFU3784", sLotid);  //Cell ID가 이미 존재 합니다.[%1]
                                return;
                            }
                        }
                    }

                }

                InputCellExcel(list);
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
        private void SetGridCartList(string ctnrID)
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

                string bizName = string.Empty;
                if (_Mode == "SELECT")
                {
                    bizName = "DA_PRD_SEL_CTNR_LOSS_LIST_CTNR_ID";
                }
                else
                {
                    bizName = "DA_PRD_SEL_CTNR_LOSS_LIST";
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizName, "INDATA", "OUTDATA", dtCtnr);
                Util.GridSetData(dgCtnr, dtRslt, FrameOperation);

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

        #region 불량 Cell 등록 : InputCell()
        //불량 Cell 등록
        private void InputCell(string pStrCellID)
        {

            DataSet inData = new DataSet();

            DataTable inCtnrTable = inData.Tables.Add("INCTNR");
            inCtnrTable.Columns.Add("CTNR_ID", typeof(string));
            inCtnrTable.Columns.Add("CTNR_PRODID", typeof(string));

            DataRow row = null;

            row = inCtnrTable.NewRow();
            row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
            row["CTNR_PRODID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "PRODID"));
            inCtnrTable.Rows.Add(row);


            DataTable inSublotTable = inData.Tables.Add("INSUBLOT");
            inSublotTable.Columns.Add("SUBLOTID", typeof(string));

            row = inSublotTable.NewRow();
            row["SUBLOTID"] = pStrCellID;
            inSublotTable.Rows.Add(row);

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));

            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(row);

            try
            {
                ShowLoadingIndicator();

                string LossCellID = pStrCellID;

                txtCellID.Focus();
                txtCellID.Text = string.Empty;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CTNR_LOSS_SUBLOT", "INCTNR,INSUBLOT,INDATA", null, (Result, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    //불량 Cell 재조회
                    SetDefcCell(LossCellID);


                    if (dgCell.Rows.Count == 0)
                    {
                        Util.GridSetData(dgCell, LOSS_CELL, FrameOperation);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgCell.ItemsSource);

                        DataRow newRow = null;
                        newRow = dtSource.NewRow();
                        newRow["CHK"] = 0;
                        newRow["LOTID_RT"] = LOSS_CELL.Rows[0]["LOTID_RT"].ToString();
                        newRow["CELLID"] = LOSS_CELL.Rows[0]["CELLID"].ToString();
                        newRow["SUBLOTQTY"] = LOSS_CELL.Rows[0]["SUBLOTQTY"].ToString();
                        dtSource.Rows.Add(newRow);

                        for (int i = 0; i < dtSource.Rows.Count; i++)
                        {
                            dtSource.Rows[i]["CHK"] = 0;
                        }

                        Util.GridSetData(dgCell, dtSource, FrameOperation);
                        //txtCellID.Focus();
                        //txtCellID.Text = string.Empty;
                    }

                    SumScanQty();
                }, inData);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }

        }


        private void InputCellExcel(ArrayList list)
        {

            DataSet inData = new DataSet();

            DataTable inCtnrTable = inData.Tables.Add("INCTNR");
            inCtnrTable.Columns.Add("CTNR_ID", typeof(string));
            inCtnrTable.Columns.Add("CTNR_PRODID", typeof(string));

            DataRow row = null;

            row = inCtnrTable.NewRow();
            row["CTNR_ID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "CTNR_ID"));
            row["CTNR_PRODID"] = Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "PRODID"));
            inCtnrTable.Rows.Add(row);

            DataTable inSublotTable = inData.Tables.Add("INSUBLOT");
            inSublotTable.Columns.Add("SUBLOTID", typeof(string));


            for(int i = 0; i < list.Count; i++)
            {
                row = inSublotTable.NewRow();
                row["SUBLOTID"] = list[i].ToString();
                inSublotTable.Rows.Add(row);
            }

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("USERID", typeof(string));

            row = inDataTable.NewRow();
            row["USERID"] = LoginInfo.USERID;
            inDataTable.Rows.Add(row);

            try
            {
                ShowLoadingIndicator();

                txtCellID.Focus();
                txtCellID.Text = string.Empty;

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CTNR_LOSS_SUBLOT", "INCTNR,INSUBLOT,INDATA", null, (Result, ex) =>
                {

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        //불량 Cell 재조회
                        SetDefcCell(list[i].ToString());

                        if (dgCell.Rows.Count == 0)
                        {
                            Util.GridSetData(dgCell, LOSS_CELL, FrameOperation);
                        }
                        else
                        {
                            DataTable dtSource = DataTableConverter.Convert(dgCell.ItemsSource);

                            DataRow newRow = null;
                            newRow = dtSource.NewRow();
                            newRow["CHK"] = 0;
                            newRow["LOTID_RT"] = LOSS_CELL.Rows[0]["LOTID_RT"].ToString();
                            newRow["CELLID"] = LOSS_CELL.Rows[0]["CELLID"].ToString();
                            newRow["SUBLOTQTY"] = LOSS_CELL.Rows[0]["SUBLOTQTY"].ToString();
                            dtSource.Rows.Add(newRow);

                            for (int idx = 0; idx < dtSource.Rows.Count; idx++)
                            {
                                dtSource.Rows[idx]["CHK"] = 0;
                            }

                            Util.GridSetData(dgCell, dtSource, FrameOperation);
                            //txtCellID.Focus();
                            //txtCellID.Text = string.Empty;
                        }
                    }

                    SumScanQty();
                }, inData);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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

                // 대차 스프레드 수량 수정
                for (int i = 0; i < dgAssyLot.Rows.Count; i++)
                {
                    SumCtnrScanQty = SumCtnrScanQty + Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgAssyLot.Rows[i].DataItem, "SUBLOTQTY")).Replace(",", ""));
                }

                SumCtnrNonscanQty = Convert.ToInt32(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "WIPQTY")).Replace(",", "")) - SumCtnrScanQty;

                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "SCAN_QTY", SumCtnrScanQty);
                DataTableConverter.SetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY", SumCtnrNonscanQty);


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        #endregion

        #region 불량 Cell 삭제 : DeleteCell()

        //불량Cell 삭제
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

        private void SetDefcCell(string CellID)
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("SUBLOTID", typeof(string));
                DataRow dr = dtRqst.NewRow();
                dr["SUBLOTID"] = CellID;

                dtRqst.Rows.Add(dr);
                LOSS_CELL = new DataTable();
                LOSS_CELL = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CTNR_LOSS_CELL", "INDATA", "OUTDATA", dtRqst);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }
        #endregion

        #region Validation()
        // <summary>
        /// Validation
        /// </summary>
        private bool Validation(string pStrCellID)
        {

            if (pStrCellID == string.Empty)
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
                //스캔 Cell수량이 대차의 Cell 수량보다 큽니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4641"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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

        private bool ValidationExcel(ArrayList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].ToString() == string.Empty)
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
            }


            if (list.Count > Int32.Parse(Util.NVC(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY"))))
            {
                //스캔 Cell수량이 대차의 Cell 수량보다 큽니다.
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU4641"), null, "Information", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
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

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                ArrayList cellList = new ArrayList();

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                if(sheet.Rows.Count <= 1)
                {
                    Util.MessageValidation("SFU1495");  //대상 Cell ID가 입력되지 않았습니다.
                    return;
                }


                int iNonScanQty = Util.NVC_Int(DataTableConverter.GetValue(dgCtnr.Rows[0].DataItem, "NON_SCAN_QTY"));
                if (sheet.Rows.Count - 1 > iNonScanQty) //엑셀은 헤더(0번째) 제외한 수로 해야 해서 -1
                {
                    Util.MessageValidation("SFU3785", sheet.Rows.Count - 1, iNonScanQty);  //입력할 Cell 수량[%1]이 미Scan Cell 수량[%2]보다 큽니다.
                    return;
                }

                //헤더(0) 번째는 제외. 데이터는 1번째 부터로 인식한다.
                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    string sCellId = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                    if (String.IsNullOrEmpty(sCellId))
                    {
                        Util.MessageValidation("SFU3783");  //Cell ID가 입력되지 않은 ROW가 있습니다.
                        return;
                    }

                    if (dgCell.GetRowCount() > 0)
                    {
                        for (int inx = 0; inx < dgCell.GetRowCount(); inx++)
                        {
                            if (DataTableConverter.GetValue(dgCell.Rows[inx].DataItem, "CELLID").ToString() == sCellId)
                            {
                                Util.MessageValidation("SFU3784", sCellId);  //Cell ID가 이미 존재 합니다.[%1]
                                return;
                            }
                        }
                    }

                    for (int rowInx2 = 1; rowInx2 < sheet.Rows.Count; rowInx2++)
                    {
                        if(rowInx == rowInx2)   //동일한 데이터는 중복 판정 제외
                        {
                            continue;
                        }

                        string sCellId2 = Util.NVC(sheet.GetCell(rowInx2, 0).Text);
                        if(sCellId == sCellId2)
                        {
                            Util.MessageValidation("SFU3786", sCellId);  //입력할 파일에 동일한 Cell ID가 존재합니다.[%1]
                            return;
                        }
                    }
                    cellList.Add(sCellId);
                }

                if (cellList.Count > 0)
                    InputCellProcessExcel(cellList);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

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
    }
}
